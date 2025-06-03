namespace Pathoschild.Stardew.LookupAnything.Components;

internal interface IScrollableMenu
{
	void ScrollUp(int? amount = null);

	void ScrollDown(int? amount = null);
}
