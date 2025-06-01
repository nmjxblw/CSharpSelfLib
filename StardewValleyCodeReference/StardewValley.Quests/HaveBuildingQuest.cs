using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Quests;

/// <summary>A quest which completes when a building is constructed.</summary>
public class HaveBuildingQuest : Quest
{
	/// <summary>The building type to construct, matching the key in <see cref="M:StardewValley.DataLoader.Buildings(StardewValley.LocalizedContentManager)" />.</summary>
	[XmlElement("buildingType")]
	public readonly NetString buildingType = new NetString();

	/// <summary>Construct an instance.</summary>
	public HaveBuildingQuest()
	{
		base.questType.Value = 8;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="buildingType">The building type to construct, matching the key in <see cref="M:StardewValley.DataLoader.Buildings(StardewValley.LocalizedContentManager)" />.</param>
	public HaveBuildingQuest(string buildingType)
		: this()
	{
		this.buildingType.Value = buildingType;
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.buildingType, "buildingType");
	}

	/// <inheritdoc />
	public override bool OnBuildingExists(string buildingType, bool probe = false)
	{
		bool baseChanged = base.OnBuildingExists(buildingType, probe);
		if (buildingType == this.buildingType.Value)
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
