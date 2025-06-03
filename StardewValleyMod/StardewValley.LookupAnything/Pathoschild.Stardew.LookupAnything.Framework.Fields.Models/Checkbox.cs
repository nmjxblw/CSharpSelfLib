namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;

internal record Checkbox(bool IsChecked, params IFormattedText[] Text)
{
	public Checkbox(bool isChecked, string text)
		: this(isChecked, new FormattedText(text))
	{
	}
}
