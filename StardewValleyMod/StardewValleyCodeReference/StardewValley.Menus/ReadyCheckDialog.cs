using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus;

public class ReadyCheckDialog : ConfirmationDialog
{
	public string checkName;

	private bool allowCancel;

	public ReadyCheckDialog(string checkName, bool allowCancel, behavior onConfirm = null, behavior onCancel = null)
		: base(Game1.content.LoadString("Strings\\UI:ReadyCheck", "N", "M"), onConfirm, onCancel)
	{
		this.checkName = checkName;
		this.allowCancel = allowCancel;
		base.okButton.visible = false;
		base.cancelButton.visible = this.isCancelable();
		this.updateMessage();
		base.exitFunction = delegate
		{
			this.closeDialog(Game1.player);
		};
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	public bool isCancelable()
	{
		if (this.allowCancel)
		{
			return Game1.netReady.IsReadyCheckCancelable(this.checkName);
		}
		return false;
	}

	public override bool readyToClose()
	{
		return this.isCancelable();
	}

	public override void closeDialog(Farmer who)
	{
		base.closeDialog(who);
		Game1.displayFarmer = true;
		if (this.isCancelable())
		{
			Game1.netReady.SetLocalReady(this.checkName, ready: false);
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
	}

	private void updateMessage()
	{
		int readyNum = Game1.netReady.GetNumberReady(this.checkName);
		int requiredNum = Game1.netReady.GetNumberRequired(this.checkName);
		base.message = Game1.content.LoadString("Strings\\UI:ReadyCheck", readyNum, requiredNum);
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		base.cancelButton.visible = this.isCancelable();
		this.updateMessage();
		Game1.netReady.SetLocalReady(this.checkName, ready: true);
		if (Game1.netReady.IsReady(this.checkName))
		{
			base.confirm();
		}
	}
}
