using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Locations;

public class DwarfGate : INetObject<NetFields>
{
	public NetPoint tilePosition = new NetPoint();

	public NetLocationRef locationRef = new NetLocationRef();

	public bool triggeredOpen;

	public NetPointDictionary<bool, NetBool> switches = new NetPointDictionary<bool, NetBool>
	{
		InterpolationWait = false
	};

	public Dictionary<Point, bool> localSwitches = new Dictionary<Point, bool>();

	public NetBool opened = new NetBool(value: false);

	public bool localOpened;

	public NetInt pressedSwitches = new NetInt(0)
	{
		InterpolationWait = false
	};

	public int localPressedSwitches;

	public NetInt gateIndex = new NetInt(0);

	public NetEvent0 openEvent = new NetEvent0();

	public NetEvent1Field<Point, NetPoint> pressEvent = new NetEvent1Field<Point, NetPoint>
	{
		InterpolationWait = false
	};

	public NetFields NetFields { get; } = new NetFields("DwarfGate");

	public DwarfGate()
	{
		this.InitNetFields();
	}

	public DwarfGate(VolcanoDungeon location, int gate_index, int x, int y, int seed)
		: this()
	{
		this.locationRef.Value = location;
		this.tilePosition.X = x;
		this.tilePosition.Y = y;
		this.gateIndex.Value = gate_index;
		Random r = Utility.CreateRandom(seed);
		if (location.possibleSwitchPositions.TryGetValue(gate_index, out var positions))
		{
			int max_points = Math.Min(positions.Count, 3);
			if (gate_index > 0)
			{
				max_points = 1;
			}
			List<Point> points = new List<Point>(positions);
			Utility.Shuffle(r, points);
			int points_to_choose = r.Next(1, Math.Max(1, max_points));
			points_to_choose = Math.Min(points_to_choose, max_points);
			if (location.isMonsterLevel())
			{
				points_to_choose = max_points;
			}
			for (int i = 0; i < points_to_choose; i++)
			{
				this.switches[points[i]] = false;
			}
		}
		this.UpdateLocalStates();
		this.ApplyTiles();
	}

	public virtual void InitNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.tilePosition, "tilePosition").AddField(this.locationRef.NetFields, "locationRef.NetFields")
			.AddField(this.switches, "switches")
			.AddField(this.pressedSwitches, "pressedSwitches")
			.AddField(this.openEvent.NetFields, "openEvent.NetFields")
			.AddField(this.opened, "opened")
			.AddField(this.pressEvent.NetFields, "pressEvent.NetFields")
			.AddField(this.gateIndex, "gateIndex");
		this.pressEvent.onEvent += OnPress;
		this.openEvent.onEvent += OpenGate;
	}

	public virtual void OnPress(Point point)
	{
		if (Game1.IsMasterGame && this.switches.TryGetValue(point, out var wasPressed) && !wasPressed)
		{
			this.switches[point] = true;
			this.pressedSwitches.Value++;
		}
		if (Game1.currentLocation == this.locationRef.Value)
		{
			Game1.playSound("openBox");
		}
		this.localSwitches[point] = true;
		this.ApplyTiles();
	}

	public virtual void OpenGate()
	{
		if (Game1.currentLocation == this.locationRef.Value)
		{
			Game1.playSound("cowboy_gunload");
		}
		if (Game1.IsMasterGame)
		{
			if (this.gateIndex.Value == -1 && !Game1.MasterPlayer.hasOrWillReceiveMail("volcanoShortcutUnlocked"))
			{
				Game1.addMailForTomorrow("volcanoShortcutUnlocked", noLetter: true);
			}
			this.opened.Value = true;
		}
		this.localOpened = true;
		this.ApplyTiles();
	}

	public virtual void ResetLocalState()
	{
		this.UpdateLocalStates();
		this.ApplyTiles();
	}

	public virtual void UpdateLocalStates()
	{
		this.localOpened = this.opened.Value;
		this.localPressedSwitches = this.pressedSwitches.Value;
		foreach (Point key in this.switches.Keys)
		{
			this.localSwitches[key] = this.switches[key];
		}
	}

	public virtual void Draw(SpriteBatch b)
	{
		if (!this.localOpened)
		{
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.tilePosition.X, this.tilePosition.Y) * 64f + new Vector2(1f, -5f) * 4f), new Rectangle(178, 189, 14, 34), Color.White, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, (float)((this.tilePosition.Y + 2) * 64) / 10000f);
		}
	}

	public virtual void UpdateWhenCurrentLocation(GameTime time, GameLocation location)
	{
		this.openEvent.Poll();
		this.pressEvent.Poll();
		if (this.localPressedSwitches != this.pressedSwitches.Value)
		{
			this.localPressedSwitches = this.pressedSwitches.Value;
			this.ApplyTiles();
		}
		if (!this.localOpened && this.opened.Value)
		{
			this.localOpened = true;
			this.ApplyTiles();
		}
		foreach (Point key in this.switches.Keys)
		{
			if (this.switches[key] && !this.localSwitches[key])
			{
				this.localSwitches[key] = true;
				this.ApplyTiles();
			}
		}
	}

	public virtual void ApplyTiles()
	{
		int total_switches = 0;
		int local_pressed_switches = 0;
		int pressed_switches = 0;
		foreach (Point point in this.localSwitches.Keys)
		{
			total_switches++;
			if (this.switches[point])
			{
				pressed_switches++;
			}
			if (this.localSwitches[point])
			{
				local_pressed_switches++;
				this.locationRef.Value.setMapTile(point.X, point.Y, VolcanoDungeon.GetTileIndex(1, 31), "Back", "dungeon").Properties.Remove("TouchAction");
			}
			else
			{
				this.locationRef.Value.setMapTile(point.X, point.Y, VolcanoDungeon.GetTileIndex(0, 31), "Back", "dungeon").Properties["TouchAction"] = "DwarfSwitch";
			}
		}
		switch (total_switches)
		{
		case 1:
			this.locationRef.Value.setMapTile(this.tilePosition.X - 1, this.tilePosition.Y, VolcanoDungeon.GetTileIndex(10 + local_pressed_switches, 23), "Buildings", "dungeon");
			break;
		case 2:
			this.locationRef.Value.setMapTile(this.tilePosition.X - 1, this.tilePosition.Y, VolcanoDungeon.GetTileIndex(12 + local_pressed_switches, 23), "Buildings", "dungeon");
			break;
		case 3:
			this.locationRef.Value.setMapTile(this.tilePosition.X - 1, this.tilePosition.Y, VolcanoDungeon.GetTileIndex(10 + local_pressed_switches, 22), "Buildings", "dungeon");
			break;
		}
		if (!this.triggeredOpen && pressed_switches >= total_switches)
		{
			this.triggeredOpen = true;
			if (Game1.IsMasterGame)
			{
				DelayedAction.functionAfterDelay(this.openEvent.Fire, 500);
			}
		}
		if (this.localOpened)
		{
			this.locationRef.Value.removeTile(this.tilePosition.X, this.tilePosition.Y + 1, "Buildings");
		}
		else
		{
			this.locationRef.Value.setMapTile(this.tilePosition.X, this.tilePosition.Y + 1, 0, "Buildings", "dungeon");
		}
	}
}
