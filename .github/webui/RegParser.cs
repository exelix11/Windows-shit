using System.Globalization;

public static class RegParser
{
	public static UIElement[] Parse(string reg)
	{
		var lines = reg.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (lines[0] != "Windows Registry Editor Version 5.00")
			throw new ArgumentException("Invalid registry file");

		if (lines[1] == ";IGNORE")
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

		string CurrentRoot = "";

		foreach (var line in lines.Skip(1))
		{
			if (line.StartsWith(";CAT:")) {
				FinishGroup();
			 	builder.Category = line[5..];
			}
			else if (line.StartsWith(";PRESET:")) {
				FinishGroup();
				builder.Presets = line[8..].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			}
			else if (line.StartsWith("["))
				CurrentRoot = line[1..^1];
			else if (line.StartsWith(";; "))
			{
				// Skip comment lines
			}
			else if (line.StartsWith("; "))
			{
				FinishGroup();
				builder.Label = line[2..];
			}
			else if (line.StartsWith("\"") || line.StartsWith("@"))
			{
				if (CurrentRoot == "")
					throw new ArgumentException("Invalid registry file");

				var eq = line.IndexOf('=');
				var name = line[0..eq];

				if (name[0] == '"')
					name = name[1..^1];
				else if (name == "@")
					name = null;

				var val = line[(eq + 1)..];

				RegValueKind type = RegValueKind.String;

				if (val[0] == '"')
					val = val[1..^1];
				else {
					var valType = val[0..val.IndexOf(':')];
					val = val[(val.IndexOf(':') + 1)..];

					type = valType switch {
						"dword" => RegValueKind.Dword,
						_ => throw new NotImplementedException()
					};
				}

				builder.AddItem(new RegValue(CurrentRoot, name, type, val));
			}
		}

		FinishGroup();

		return Elements.ToArray();
	}
}

public enum RegValueKind 
{
	Dword,
	String
}

public record RegValue(string Path, string? Name, RegValueKind Kind, string Value) : IItem
{
	public bool IsUserOnly => Path.StartsWith("HKEY_CURRENT_USER") || Path.StartsWith("HKCU");

	private string KindString => Kind switch
	{
		RegValueKind.Dword => "REG_DWORD",
		RegValueKind.String => "REG_SZ",
		_ => throw new NotImplementedException()
	};

	private string ValueForRegAdd => Kind switch 
	{
		RegValueKind.Dword => $"0x{Value}",
		RegValueKind.String => Value,
		_ => throw new NotImplementedException()
	};

	public string ToRegEntry()
	{
		string StringValue() 
		{
			if (Kind == RegValueKind.Dword)
				return $"dword:{uint.Parse(Value, NumberStyles.HexNumber):x8}";
			else
				return $"\"{Value}\"";
		}

		if (Name is not null)
			return $"\"{Name}\"={StringValue()}";
		else
			return $"@={StringValue()}";
	}

	public string AsCmdCommand() {
		throw new Exception("Deprecated");
		if (Name is not null)
			return $"reg add \"{Path}\" /v \"{Name}\" /t {KindString} /d \"{ValueForRegAdd}\" /f";
		else
			return $"reg add \"{Path}\" /ve /t {KindString} /d \"{Value}\" /f";
	}
}