using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Network;
using StardewValley.Objects;

namespace StardewValley.Locations;

public class IslandShrine : IslandForestLocation
{
	[XmlIgnore]
	public ItemPedestal northPedestal;

	[XmlIgnore]
	public ItemPedestal southPedestal;

	[XmlIgnore]
	public ItemPedestal eastPedestal;

	[XmlIgnore]
	public ItemPedestal westPedestal;

	[XmlIgnore]
	public NetEvent0 puzzleFinishedEvent = new NetEvent0();

	[XmlElement("puzzleFinished")]
	public NetBool puzzleFinished = new NetBool();

	public IslandShrine()
	{
	}

	public IslandShrine(string map, string name)
		: base(map, name)
	{
		this.AddMissingPedestals();
	}

	public override List<Vector2> GetAdditionalWalnutBushes()
	{
		return new List<Vector2>
		{
			new Vector2(23f, 34f)
		};
	}

	public ItemPedestal AddOrUpdatePedestal(Vector2 position, string birdLocation)
	{
		ItemPedestal pedestal = base.getObjectAtTile((int)position.X, (int)position.Y) as ItemPedestal;
		string itemId = IslandGemBird.GetItemIndex(IslandGemBird.GetBirdTypeForLocation(birdLocation));
		if (pedestal == null || !pedestal.isIslandShrinePedestal.Value)
		{
			OverlaidDictionary overlaidDictionary = base.objects;
			Vector2 key = position;
			ItemPedestal itemPedestal = new ItemPedestal(position, null, lock_on_success: false, Color.White);
			itemPedestal.Fragility = 2;
			itemPedestal.isIslandShrinePedestal.Value = true;
			pedestal = itemPedestal;
			overlaidDictionary[key] = itemPedestal;
		}
		pedestal.successColor.Value = Color.Transparent;
		if (pedestal.requiredItem.Value?.ItemId != itemId)
		{
			pedestal.requiredItem.Value = new Object(itemId, 1);
			if (pedestal.heldObject.Value?.ItemId != itemId)
			{
				pedestal.heldObject.Value = null;
			}
		}
		return pedestal;
	}

	public virtual void AddMissingPedestals()
	{
		this.westPedestal = this.AddOrUpdatePedestal(new Vector2(21f, 27f), "IslandWest");
		this.eastPedestal = this.AddOrUpdatePedestal(new Vector2(27f, 27f), "IslandEast");
		this.southPedestal = this.AddOrUpdatePedestal(new Vector2(24f, 28f), "IslandSouth");
		this.northPedestal = this.AddOrUpdatePedestal(new Vector2(24f, 25f), "IslandNorth");
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.puzzleFinished, "puzzleFinished").AddField(this.puzzleFinishedEvent, "puzzleFinishedEvent");
		this.puzzleFinishedEvent.onEvent += OnPuzzleFinish;
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (Game1.IsMasterGame)
		{
			this.AddMissingPedestals();
		}
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		if (this.puzzleFinished.Value)
		{
			this.ApplyFinishedTiles();
		}
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		base.TransferDataFromSavedLocation(l);
		if (l is IslandShrine shrine)
		{
			this.northPedestal = shrine.getObjectAtTile((int)this.northPedestal.TileLocation.X, (int)this.northPedestal.TileLocation.Y) as ItemPedestal;
			this.southPedestal = shrine.getObjectAtTile((int)this.southPedestal.TileLocation.X, (int)this.southPedestal.TileLocation.Y) as ItemPedestal;
			this.eastPedestal = shrine.getObjectAtTile((int)this.eastPedestal.TileLocation.X, (int)this.eastPedestal.TileLocation.Y) as ItemPedestal;
			this.westPedestal = shrine.getObjectAtTile((int)this.westPedestal.TileLocation.X, (int)this.westPedestal.TileLocation.Y) as ItemPedestal;
			this.puzzleFinished.Value = shrine.puzzleFinished.Value;
		}
	}

	public void OnPuzzleFinish()
	{
		if (Game1.IsMasterGame)
		{
			for (int i = 0; i < 5; i++)
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)73"), new Vector2(24f, 19f) * 64f, -1, this);
			}
		}
		if (Game1.currentLocation == this)
		{
			Game1.playSound("boulderBreak");
			Game1.playSound("secret1");
			Game1.flashAlpha = 1f;
			this.ApplyFinishedTiles();
		}
	}

	public virtual void ApplyFinishedTiles()
	{
		base.setMapTile(23, 19, 142, "AlwaysFront", "untitled tile sheet3");
		base.setMapTile(24, 19, 143, "AlwaysFront", "untitled tile sheet3");
		base.setMapTile(25, 19, 144, "AlwaysFront", "untitled tile sheet3");
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		if (Game1.IsMasterGame && !this.puzzleFinished.Value && this.northPedestal.match.Value && this.southPedestal.match.Value && this.eastPedestal.match.Value && this.westPedestal.match.Value)
		{
			Game1.player.team.MarkCollectedNut("IslandShrinePuzzle");
			this.puzzleFinishedEvent.Fire();
			this.puzzleFinished.Value = true;
			this.northPedestal.locked.Value = true;
			this.northPedestal.heldObject.Value = null;
			this.southPedestal.locked.Value = true;
			this.southPedestal.heldObject.Value = null;
			this.eastPedestal.locked.Value = true;
			this.eastPedestal.heldObject.Value = null;
			this.westPedestal.locked.Value = true;
			this.westPedestal.heldObject.Value = null;
		}
	}
}
