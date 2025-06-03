using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Extensions;

namespace StardewValley.Quests;

public class LostItemQuest : Quest
{
	/// <summary>The internal name for the NPC who gave the quest.</summary>
	[XmlElement("npcName")]
	public readonly NetString npcName = new NetString();

	/// <summary>The internal name for the location where the item can be found.</summary>
	[XmlElement("locationOfItem")]
	public readonly NetString locationOfItem = new NetString();

	/// <summary>The qualified item ID for the item to find.</summary>
	[XmlElement("itemIndex")]
	public readonly NetString ItemId = new NetString();

	/// <summary>The X tile position within the location where the item can be found.</summary>
	[XmlElement("tileX")]
	public readonly NetInt tileX = new NetInt();

	/// <summary>The Y tile position within the location where the item can be found.</summary>
	[XmlElement("tileY")]
	public readonly NetInt tileY = new NetInt();

	/// <summary>Whether the player has found the item yet.</summary>
	[XmlElement("itemFound")]
	public readonly NetBool itemFound = new NetBool();

	/// <summary>The translatable text segments for the objective shown in the quest log.</summary>
	[XmlElement("objective")]
	public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

	/// <summary>Construct an instance.</summary>
	public LostItemQuest()
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="npcName">The internal name for the NPC who gave the quest.</param>
	/// <param name="locationOfItem">The internal name for the location where the item can be found.</param>
	/// <param name="itemId">The qualified or unqualified item ID for the item to find.</param>
	/// <param name="tileX">The X tile position within the location where the item can be found.</param>
	/// <param name="tileY">The Y tile position within the location where the item can be found.</param>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="itemId" /> matches a non-object-type item, which can't be placed in the world.</exception>
	public LostItemQuest(string npcName, string locationOfItem, string itemId, int tileX, int tileY)
	{
		this.npcName.Value = npcName;
		this.locationOfItem.Value = locationOfItem;
		this.ItemId.Value = ItemRegistry.QualifyItemId(itemId) ?? itemId;
		this.tileX.Value = tileX;
		this.tileY.Value = tileY;
		base.questType.Value = 9;
		if (!ItemRegistry.GetDataOrErrorItem(this.ItemId.Value).HasTypeObject())
		{
			throw new InvalidOperationException($"Can't create {base.GetType().Name} #{base.id.Value} because the lost item ({this.ItemId.Value}) isn't an object-type item.");
		}
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.objective, "objective").AddField(this.npcName, "npcName").AddField(this.locationOfItem, "locationOfItem")
			.AddField(this.ItemId, "ItemId")
			.AddField(this.tileX, "tileX")
			.AddField(this.tileY, "tileY")
			.AddField(this.itemFound, "itemFound");
	}

	/// <inheritdoc />
	public override bool OnWarped(GameLocation location, bool probe = false)
	{
		bool baseChanged = base.OnWarped(location, probe);
		if (!this.itemFound.Value && location.name.Equals(this.locationOfItem.Value))
		{
			Vector2 position = new Vector2(this.tileX.Value, this.tileY.Value);
			location.overlayObjects.Remove(position);
			Object o = ItemRegistry.Create<Object>(this.ItemId.Value);
			o.TileLocation = position;
			o.questItem.Value = true;
			o.questId.Value = base.id.Value;
			o.IsSpawnedObject = true;
			location.overlayObjects.Add(position, o);
			return true;
		}
		return baseChanged;
	}

	public new void reloadObjective()
	{
		if (this.objective.Value != null)
		{
			base.currentObjective = this.objective.Value.loadDescriptionElement();
		}
	}

	/// <inheritdoc />
	public override bool OnItemReceived(Item item, int numberAdded, bool probe = false)
	{
		bool baseChanged = base.OnItemReceived(item, numberAdded, probe);
		if (!base.completed.Value && !this.itemFound.Value && item != null && item.QualifiedItemId == this.ItemId.Value)
		{
			if (!probe)
			{
				this.itemFound.Value = true;
				string npcDisplayName = this.npcName.Value;
				NPC namedNpc = Game1.getCharacterFromName(this.npcName.Value);
				if (namedNpc != null)
				{
					npcDisplayName = namedNpc.displayName;
				}
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Quests:MessageFoundLostItem", item.DisplayName, npcDisplayName));
				this.objective.Value = new DescriptionElement("Strings\\Quests:ObjectiveReturnToNPC", namedNpc);
				Game1.playSound("jingle1");
			}
			return true;
		}
		return baseChanged;
	}

	public override bool OnNpcSocialized(NPC npc, bool probe = false)
	{
		bool baseChanged = base.OnNpcSocialized(npc, probe);
		if (!base.completed.Value && this.itemFound.Value && npc.Name == this.npcName.Value && npc.IsVillager && Game1.player.Items.ContainsId(this.ItemId.Value))
		{
			if (!probe)
			{
				this.questComplete();
				string[] fields = Quest.GetRawQuestFields(base.id.Value);
				Dialogue thankYou = new Dialogue(npc, null, ArgUtility.Get(fields, 9, "Data\\ExtraDialogue:LostItemQuest_DefaultThankYou", allowBlank: false));
				npc.setNewDialogue(thankYou);
				Game1.drawDialogue(npc);
				Game1.player.changeFriendship(250, npc);
				Game1.player.removeFirstOfThisItemFromInventory(this.ItemId.Value);
			}
			return true;
		}
		return baseChanged;
	}
}
