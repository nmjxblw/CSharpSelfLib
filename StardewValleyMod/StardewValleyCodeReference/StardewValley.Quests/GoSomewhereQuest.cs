using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Quests;

public class GoSomewhereQuest : Quest
{
	[XmlElement("whereToGo")]
	public readonly NetString whereToGo = new NetString();

	public GoSomewhereQuest()
	{
	}

	public GoSomewhereQuest(string where)
	{
		this.whereToGo.Value = where;
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.whereToGo, "whereToGo");
	}

	/// <inheritdoc />
	public override bool OnWarped(GameLocation location, bool probe = false)
	{
		bool baseChanged = base.OnWarped(location, probe);
		if (location?.NameOrUniqueName == this.whereToGo.Value)
		{
			if (!probe)
			{
				this.questComplete();
			}
			return true;
		}
		return baseChanged;
	}
}
