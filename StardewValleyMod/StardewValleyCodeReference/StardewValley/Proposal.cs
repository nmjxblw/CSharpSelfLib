using Netcode;
using StardewValley.Network;

namespace StardewValley;

public class Proposal : INetObject<NetFields>
{
	public readonly NetFarmerRef sender = new NetFarmerRef();

	public readonly NetFarmerRef receiver = new NetFarmerRef();

	public readonly NetEnum<ProposalType> proposalType = new NetEnum<ProposalType>(ProposalType.Gift);

	public readonly NetEnum<ProposalResponse> response = new NetEnum<ProposalResponse>(ProposalResponse.None);

	public readonly NetString responseMessageKey = new NetString();

	public readonly NetRef<Item> gift = new NetRef<Item>();

	public readonly NetBool canceled = new NetBool();

	public readonly NetBool cancelConfirmed = new NetBool();

	public NetFields NetFields { get; } = new NetFields("Proposal");

	public Proposal()
	{
		this.NetFields.SetOwner(this).AddField(this.sender.NetFields, "sender.NetFields").AddField(this.receiver.NetFields, "receiver.NetFields")
			.AddField(this.proposalType, "proposalType")
			.AddField(this.response, "response")
			.AddField(this.responseMessageKey, "responseMessageKey")
			.AddField(this.gift, "gift")
			.AddField(this.canceled, "canceled")
			.AddField(this.cancelConfirmed, "cancelConfirmed");
	}
}
