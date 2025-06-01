using Microsoft.Xna.Framework;

namespace StardewValley.Menus;

public class PendingProposalDialog : ConfirmationDialog
{
	public PendingProposalDialog()
		: base(Game1.content.LoadString("Strings\\UI:PendingProposal"), null)
	{
		base.okButton.visible = false;
		base.onCancel = cancelProposal;
		this.setCancelable(cancelable: true);
	}

	public void cancelProposal(Farmer who)
	{
		Proposal proposal = Game1.player.team.GetOutgoingProposal();
		if (proposal?.receiver.Value != null && proposal.receiver.Value.isActive())
		{
			proposal.canceled.Value = true;
			base.message = Game1.content.LoadString("Strings\\UI:PendingProposal_Canceling");
			this.setCancelable(cancelable: false);
		}
	}

	public void setCancelable(bool cancelable)
	{
		base.cancelButton.visible = cancelable;
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	public override bool readyToClose()
	{
		return false;
	}

	private bool consumesItem(ProposalType pt)
	{
		if (pt != ProposalType.Gift)
		{
			return pt == ProposalType.Marriage;
		}
		return true;
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		Proposal proposal = Game1.player.team.GetOutgoingProposal();
		if (proposal?.receiver.Value == null || !proposal.receiver.Value.isActive())
		{
			Game1.player.team.RemoveOutgoingProposal();
			this.closeDialog(Game1.player);
		}
		else if (proposal.cancelConfirmed.Value && proposal.response.Value != ProposalResponse.Accepted)
		{
			Game1.player.team.RemoveOutgoingProposal();
			this.closeDialog(Game1.player);
		}
		else
		{
			if (proposal.response.Value == ProposalResponse.None)
			{
				return;
			}
			if (proposal.response.Value == ProposalResponse.Accepted)
			{
				if (this.consumesItem(proposal.proposalType.Value))
				{
					Game1.player.reduceActiveItemByOne();
				}
				if (proposal.proposalType.Value == ProposalType.Dance)
				{
					Game1.player.dancePartner.Value = proposal.receiver.Value;
				}
				proposal.receiver.Value.doEmote(20);
			}
			Game1.player.team.RemoveOutgoingProposal();
			this.closeDialog(Game1.player);
			if (proposal.responseMessageKey.Value != null)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString(proposal.responseMessageKey.Value, proposal.receiver.Value.Name));
			}
		}
	}
}
