static class BatParser
{
	public static UIElement[] Parse(string reg)
	{
		var lines = reg.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (lines[1] == "::IGNORE")
			return new UIElement[0];

		List<UIElement> Elements = new();
		ElementBuilder builder = new();

		void FinishGroup()
		{
			if (!builder.Empty())
			{
				Elements.Add(builder.Build());
				builder.Clear();
			}
		}

		foreach (var line in lines)
		{
			if (line.StartsWith("::CAT:")) {
				FinishGroup();
				builder.Category = line[6..];
			}
			else if (line.StartsWith("::PRESET:")) 
			{
				FinishGroup();
				builder.Presets = line[9..].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			}
			else if (line.StartsWith("::::"))
			{
				// skip comment lines
			}
			else if (line.StartsWith(":: "))
			{
				FinishGroup();
				builder.Label = line[3..];
			}
			else 
			{
				if (!string.IsNullOrWhiteSpace(line))
					builder.AddItem(new BatLine(line));
			}
		}

		FinishGroup();

		return Elements.ToArray();
	}
}

public record BatLine(string Value) : IItem
{
	public string AsCmdCommand() => Value.StartsWith("reg add") ? 
		throw new Exception("Prefer using reg files for registry entries") : Value;
}