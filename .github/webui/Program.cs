using System.Linq;
using System.Text;

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
	{"SIM", "Simple"},
	{"ADV", "Advanced user"}
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
var htmlSb = new StringBuilder();
Dictionary<string, string> JsCommands = new Dictionary<string, string>();
foreach (var g in entries.GroupBy(x => x.Category))
{
	var catSb = new StringBuilder();

	foreach (var v in g)
	{
		catSb.AppendFormat(TemplateField, v.Id, v.Label);

		foreach(var pres in v.Presets)
			JsPresets[pres].Add(v.Id);

		JsCommands.Add(v.Id, string.Join("\r\n", v.Items.Select(x => x.AsCmdCommand())));
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
template = template.Replace("$JS_COMMANDS", System.Text.Json.JsonSerializer.Serialize(JsCommands));

File.WriteAllText("page.js", template);