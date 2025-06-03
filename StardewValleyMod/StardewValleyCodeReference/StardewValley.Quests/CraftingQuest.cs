using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Quests;

public class CraftingQuest : Quest
{
	/// <summary>Obsolete. This is only kept to preserve data from old save files, and isn't synced in multiplayer. Use <see cref="F:StardewValley.Quests.CraftingQuest.ItemId" /> instead.</summary>
	[XmlElement("isBigCraftable")]
	public bool? obsolete_isBigCraftable;

	/// <summary>The qualified item ID to craft.</summary>
	[XmlElement("indexToCraft")]
	public readonly NetString ItemId = new NetString();

	/// <summary>Construct an instance.</summary>
	public CraftingQuest()
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="itemId">The qualified or unqualified item ID to craft.</param>
	public CraftingQuest(string itemId)
	{
		this.ItemId.Value = ItemRegistry.QualifyItemId(itemId) ?? itemId;
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.ItemId, "ItemId");
	}

	/// <inheritdoc />
	public override bool OnRecipeCrafted(CraftingRecipe recipe, Item item, bool probe = false)
	{
		bool baseChanged = base.OnRecipeCrafted(recipe, item, probe);
		if (item.QualifiedItemId == this.ItemId.Value)
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
