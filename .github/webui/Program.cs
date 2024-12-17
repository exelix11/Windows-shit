using System.Linq;
using System.Text;
using System.Text.Json;

const string TemplateSection = 
""""
<section style="margin-bottom: 1em" id="{0}">
	<h3 style="margin-bottom: .4em;">{1}</h3>
	<fieldset>
		{2}
	</fieldset>
</section>
"""";

const string TemplateField = 
""""
<label for="{0}">
	  <input type="checkbox" id="{0}" name="{0}" role="switch" checked>
	  {1}
</label>
"""";

const string TempaltePreset = "<option value=\"{0}\">{1}</option>";

Dictionary<string, string> PresetNames = new() {
	{"NONE", "Clear all selected"},
	{"SIM", "Simple user"},
	{"ADV", "Advanced user"},
	{"NOSEC", "Don't need security, don't get in my way. Only for Test VMs." }
};

// Parse input files
List<UIElement> entries = new();
foreach (var file in Directory.GetFiles(args[0], "*.reg", SearchOption.AllDirectories))
{
	entries.AddRange(RegParser.Parse(File.ReadAllText(file)));
}

foreach (var file in Directory.GetFiles(args[0], "*.bat", SearchOption.AllDirectories))
{
	entries.AddRange(BatParser.Parse(File.ReadAllText(file)));
}

// Generate HTML and prepare JS data
Dictionary<string, List<string>> JsPresets = PresetNames.Keys.ToDictionary(x => x, x => new List<string>());
Dictionary<string, string> JsCommands = new Dictionary<string, string>();
Dictionary<string, string> JsRegUser = new Dictionary<string, string>();
Dictionary<string, string> JsRegAdmin = new Dictionary<string, string>();

var htmlSb = new StringBuilder();
foreach (var g in entries.GroupBy(x => x.Category))
{
	var catSb = new StringBuilder();

	foreach (var v in g)
	{
		catSb.AppendFormat(TemplateField, v.Id, v.Label);

		foreach(var pres in v.Presets)
			JsPresets[pres].Add(v.Id);

		if (v.Items[0] is RegValue r)
		{
			string? processGroup(IEnumerable<RegValue> values)
			{
				StringBuilder sb = new StringBuilder();
				foreach (var k in values.GroupBy(x => x.Path))
				{
					sb.AppendLine($"[{k.Key}]");
					sb.AppendJoin("\r\n", k.Select(x => x.ToRegEntry()));
					sb.AppendLine();
				}
				if (sb.Length > 0)
					return sb.ToString();
				return null;
			}

			var admin = processGroup(v.Items.Cast<RegValue>().Where(x => !x.IsUserOnly));
			var user = processGroup(v.Items.Cast<RegValue>().Where(x => x.IsUserOnly));

			if (admin != null)
				JsRegAdmin.Add(v.Id, admin);

			if (user != null)
				JsRegUser.Add(v.Id, user);
		}
		else 
		{
			JsCommands.Add(v.Id, string.Join("\r\n", v.Items.Select(x => x.AsCmdCommand())));
		}
	}

	var name = g.Key;

	htmlSb.AppendFormat(TemplateSection, $"S{name.GetHashCode():x}", name, catSb.ToString());
}

var Commit = Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "Unknown";
string HtmlPresets = PresetNames.Select(x => string.Format(TempaltePreset, x.Key, x.Value)).Aggregate((a, b) => a + b);

var template = File.ReadAllText("template/page.html");

template = template.Replace("$GIT_HASH", Commit);
template = template.Replace("$PAGE_PRESETS", HtmlPresets);
template = template.Replace("$PAGE_CONTENT", htmlSb.ToString());

File.WriteAllText("index.html", template);
htmlSb.Clear();

// Generate JS
template = File.ReadAllText("template/page.js");

for (int i = 0; i < JsPresets.Count; i++)
{
	var (key, value) = JsPresets.ElementAt(i);
	htmlSb.AppendFormat("'{0}': [{1}]", key, string.Join(", ", value.Select(x => $"'{x}'")));
	if (i != JsPresets.Count - 1)
		htmlSb.Append(", ");
}

template = template.Replace("$JS_PRESETS", htmlSb.ToString());
template = template.Replace("$JS_COMMANDS", JsonSerializer.Serialize(JsCommands));
template = template.Replace("$JS_REG_USER", JsonSerializer.Serialize(JsRegUser));
template = template.Replace("$JS_REG_ADMIN", JsonSerializer.Serialize(JsRegAdmin));

File.WriteAllText("page.js", template);