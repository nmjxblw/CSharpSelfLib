using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewValley.Monsters;
using xTile.Layers;

namespace StardewValley.Locations;

public class BugLand : GameLocation
{
	[XmlElement("hasSpawnedBugsToday")]
	public bool hasSpawnedBugsToday;

	public BugLand()
	{
	}

	public BugLand(string map, string name)
		: base(map, name)
	{
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		if (l is BugLand bugLand)
		{
			this.hasSpawnedBugsToday = bugLand.hasSpawnedBugsToday;
		}
		base.TransferDataFromSavedLocation(l);
	}

	public override void hostSetup()
	{
		base.hostSetup();
		if (Game1.IsMasterGame && !this.hasSpawnedBugsToday)
		{
			this.InitializeBugLand();
		}
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		base.characters.RemoveWhere((NPC npc) => npc is Grub || npc is Fly);
		this.hasSpawnedBugsToday = false;
	}

	public virtual void InitializeBugLand()
	{
		if (this.hasSpawnedBugsToday)
		{
			return;
		}
		this.hasSpawnedBugsToday = true;
		Layer pathsLayer = base.map.RequireLayer("Paths");
		for (int x = 0; x < base.map.Layers[0].LayerWidth; x++)
		{
			for (int y = 0; y < base.map.Layers[0].LayerHeight; y++)
			{
				if (!(Game1.random.NextDouble() < 0.33))
				{
					continue;
				}
				int tileIndex = pathsLayer.GetTileIndexAt(x, y);
				if (tileIndex == -1)
				{
					continue;
				}
				Vector2 tile = new Vector2(x, y);
				switch (tileIndex)
				{
				case 13:
				case 14:
				case 15:
					if (!base.objects.ContainsKey(tile))
					{
						base.objects.Add(tile, ItemRegistry.Create<Object>(GameLocation.getWeedForSeason(Game1.random, Season.Spring)));
					}
					break;
				case 16:
					if (!base.objects.ContainsKey(tile))
					{
						base.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)343", "(O)450")));
					}
					break;
				case 17:
					if (!base.objects.ContainsKey(tile))
					{
						base.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)343", "(O)450")));
					}
					break;
				case 18:
					if (!base.objects.ContainsKey(tile))
					{
						base.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)294", "(O)295")));
					}
					break;
				case 28:
					if (this.CanSpawnCharacterHere(tile) && base.characters.Count < 50)
					{
						base.characters.Add(new Grub(new Vector2(tile.X * 64f, tile.Y * 64f), hard: true));
					}
					break;
				}
			}
		}
	}
}
