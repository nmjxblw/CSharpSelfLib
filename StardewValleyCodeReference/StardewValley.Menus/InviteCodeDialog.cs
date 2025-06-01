using Microsoft.Xna.Framework;

namespace StardewValley.Menus;

public class InviteCodeDialog : ConfirmationDialog
{
	private string code;

	public InviteCodeDialog(string code, behavior onClose)
		: base(Game1.content.LoadString("Strings\\UI:Server_InviteCode", code), onClose, onClose)
	{
		this.code = code;
		base.onCancel = copyCode;
		base.cancelButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 21, 64, 64), null, null, Game1.mouseCursors, new Rectangle(274, 284, 16, 16), 4f)
		{
			myID = 102,
			leftNeighborID = 101
		};
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			base.currentlySnappedComponent = base.getComponentWithID(101);
			this.snapCursorToCurrentSnappedComponent();
		}
	}

	protected void copyCode(Farmer who)
	{
		if (DesktopClipboard.SetText(this.code))
		{
			Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Server_InviteCode_Copied")));
		}
		else
		{
			Game1.showRedMessageUsingLoadString("Strings\\UI:Server_InviteCode_CopyFailed");
		}
	}
}
