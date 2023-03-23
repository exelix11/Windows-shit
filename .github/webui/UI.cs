class ElementBuilder
{
	public string? Label;
	public string? Category;
	public string[]? Presets;
	readonly List<IItem> Items = new();

	public void Clear()
	{
		Label = null;
		Items.Clear();
	}

	public void AddItem(IItem item)
	{
		Items.Add(item);
	}

	public bool Empty() => 
		Items.Count == 0;

	public UIElement Build()
	{
		if (Label == null || Category == null || Presets == null)
			throw new ArgumentException("Invalid registry file");

		return new UIElement(Category, Label, Presets, Items.ToArray());
	}
}

public interface IItem
{
	public string AsCmdCommand();
}

public record UIElement(string Category, string Label, string[] Presets, IItem[] Items)
{
	public string Id => $"L{Label.GetHashCode():x}";
}