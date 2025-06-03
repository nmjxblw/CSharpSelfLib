using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace StardewValley;

public class SlimeHutch : DecoratableLocation
{
	[XmlElement("slimeMatingsLeft")]
	public readonly NetInt slimeMatingsLeft = new NetInt();

	public readonly NetArray<bool, NetBool> waterSpots = new NetArray<bool, NetBool>(4);

	protected int _slimeCapacity = -1;

	public SlimeHutch()
	{
	}

	public SlimeHutch(string m, string name)
		: base(m, name)
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.slimeMatingsLeft, "slimeMatingsLeft").AddField(this.waterSpots, "waterSpots");
	}

	/// <inheritdoc />
	public override void OnParentBuildingUpgraded(Building building)
	{
		base.OnParentBuildingUpgraded(building);
		this._slimeCapacity = -1;
	}

	public bool isFull()
	{
		if (this._slimeCapacity < 0)
		{
			this._slimeCapacity = base.ParentBuilding?.GetData()?.MaxOccupants ?? 20;
		}
		return base.characters.Count >= this._slimeCapacity;
	}

	public override bool canSlimeMateHere()
	{
		int matesLeft = this.slimeMatingsLeft.Value;
		this.slimeMatingsLeft.Value--;
		if (!this.isFull())
		{
			return matesLeft > 0;
		}
		return false;
	}

	public override bool canSlimeHatchHere()
	{
		return !this.isFull();
	}

	public override void DayUpdate(int dayOfMonth)
	{
		int waters = 0;
		int startIndex = Game1.random.Next(this.waterSpots.Length);
		for (int i = 0; i < this.waterSpots.Length; i++)
		{
			if (this.waterSpots[(i + startIndex) % this.waterSpots.Length] && waters * 5 < base.characters.Count)
			{
				waters++;
				this.waterSpots[(i + startIndex) % this.waterSpots.Length] = false;
			}
		}
		foreach (Object sprinkler in base.objects.Values)
		{
			if (!sprinkler.IsSprinkler())
			{
				continue;
			}
			foreach (Vector2 v in sprinkler.GetSprinklerTiles())
			{
				if (v.X == 16f && v.Y >= 6f && v.Y <= 9f)
				{
					this.waterSpots[(int)v.Y - 6] = true;
				}
			}
		}
		for (int numSlimeBalls = Math.Min(base.characters.Count / 5, waters); numSlimeBalls > 0; numSlimeBalls--)
		{
			int tries = 50;
			Vector2 tile = base.getRandomTile();
			while ((!this.CanItemBePlacedHere(tile, itemIsPassable: false, CollisionMask.All, CollisionMask.None) || this.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back") != null || tile.Y >= 12f) && tries > 0)
			{
				tile = base.getRandomTile();
				tries--;
			}
			if (tries > 0)
			{
				Object slimeBall = ItemRegistry.Create<Object>("(BC)56");
				slimeBall.fragility.Value = 2;
				base.objects.Add(tile, slimeBall);
			}
		}
		while (this.slimeMatingsLeft.Value > 0)
		{
			if (base.characters.Count > 1 && !this.isFull() && base.characters[Game1.random.Next(base.characters.Count)] is GreenSlime mate && mate.ageUntilFullGrown.Value <= 0)
			{
				for (int distance = 1; distance < 10; distance++)
				{
					GreenSlime otherMate = (GreenSlime)Utility.checkForCharacterWithinArea(mate.GetType(), mate.Position, this, new Rectangle((int)mate.Position.X - 64 * distance, (int)mate.Position.Y - 64 * distance, 64 * (distance * 2 + 1), 64 * (distance * 2 + 1)));
					if (otherMate != null && otherMate.cute.Value != mate.cute.Value && otherMate.ageUntilFullGrown.Value <= 0)
					{
						mate.mateWith(otherMate, this);
						break;
					}
				}
			}
			this.slimeMatingsLeft.Value--;
		}
		this.slimeMatingsLeft.Value = base.characters.Count / 5 + 1;
		base.DayUpdate(dayOfMonth);
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		if (l is SlimeHutch slimeHutch)
		{
			for (int i = 0; i < this.waterSpots.Length; i++)
			{
				if (i < slimeHutch.waterSpots.Count)
				{
					this.waterSpots[i] = slimeHutch.waterSpots[i];
				}
			}
		}
		base.TransferDataFromSavedLocation(l);
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (base.objects.TryGetValue(new Vector2(1f, 4f), out var tileObj))
		{
			tileObj.Fragility = 0;
		}
	}

	public override bool performToolAction(Tool t, int tileX, int tileY)
	{
		if (t is WateringCan && tileX == 16 && tileY >= 6 && tileY <= 9)
		{
			this.waterSpots[tileY - 6] = true;
		}
		return false;
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		for (int i = 0; i < this.waterSpots.Length; i++)
		{
			int tileIndex = (this.waterSpots[i] ? 2135 : 2134);
			base.setMapTile(16, 6 + i, tileIndex, "Buildings", "untitled tile sheet");
		}
	}
}
