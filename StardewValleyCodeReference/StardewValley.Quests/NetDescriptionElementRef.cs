using Netcode;

namespace StardewValley.Quests;

public class NetDescriptionElementRef : NetExtendableRef<DescriptionElement, NetDescriptionElementRef>
{
	public NetDescriptionElementRef()
	{
		base.Serializer = DescriptionElement.serializer;
	}

	public NetDescriptionElementRef(DescriptionElement value)
		: base(value)
	{
		base.Serializer = DescriptionElement.serializer;
	}
}
