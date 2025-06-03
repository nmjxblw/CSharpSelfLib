using StardewValley;
using xTile.Dimensions;

namespace SpaceShared.UI;

internal class RootElement : Container
{
	public bool Obscured { get; set; }

	public override int Width => ((Rectangle)(ref Game1.viewport)).Width;

	public override int Height => ((Rectangle)(ref Game1.viewport)).Height;

	public override void Update(bool isOffScreen = false)
	{
		base.Update(isOffScreen || this.Obscured);
		if (Dropdown.ActiveDropdown?.GetRoot() != this)
		{
			Dropdown.ActiveDropdown = null;
		}
		if (Dropdown.SinceDropdownWasActive > 0)
		{
			Dropdown.SinceDropdownWasActive--;
		}
	}

	internal override RootElement GetRootImpl()
	{
		return this;
	}
}
