using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley;

public class InteriorDoor : NetField<bool, InteriorDoor>
{
	public GameLocation Location;

	public Point Position;

	public TemporaryAnimatedSprite Sprite;

	public Tile Tile;

	public InteriorDoor()
	{
	}

	public InteriorDoor(GameLocation location, Point position)
		: this()
	{
		this.Location = location;
		this.Position = position;
	}

	public override void Set(bool newValue)
	{
		if (newValue != base.value)
		{
			base.cleanSet(newValue);
			base.MarkDirty();
		}
	}

	protected override void ReadDelta(BinaryReader reader, NetVersion version)
	{
		bool newValue = reader.ReadBoolean();
		if (version.IsPriorityOver(base.ChangeVersion))
		{
			base.setInterpolationTarget(newValue);
		}
	}

	protected override void WriteDelta(BinaryWriter writer)
	{
		writer.Write(base.targetValue);
	}

	public void ResetLocalState()
	{
		int x = this.Position.X;
		int y = this.Position.Y;
		Location doorLocation = new Location(x, y);
		Layer buildingsLayer = this.Location.Map.RequireLayer("Buildings");
		Layer backLayer = this.Location.Map.RequireLayer("Back");
		if (this.Tile == null)
		{
			this.Tile = buildingsLayer.Tiles[doorLocation];
		}
		if (this.Tile == null)
		{
			return;
		}
		if (this.Tile.Properties.TryGetValue("Action", out var doorAction) && doorAction.Contains("Door"))
		{
			string[] actionParts = ArgUtility.SplitBySpace(doorAction, 2);
			if (actionParts.Length > 1)
			{
				Tile tile = backLayer.Tiles[doorLocation];
				if (tile != null && !tile.Properties.ContainsKey("TouchAction"))
				{
					tile.Properties.Add("TouchAction", "Door " + actionParts[1]);
				}
			}
		}
		Microsoft.Xna.Framework.Rectangle sourceRect = default(Microsoft.Xna.Framework.Rectangle);
		bool flip = false;
		switch (this.Tile.TileIndex)
		{
		case 824:
			sourceRect = new Microsoft.Xna.Framework.Rectangle(640, 144, 16, 48);
			break;
		case 825:
			sourceRect = new Microsoft.Xna.Framework.Rectangle(640, 144, 16, 48);
			flip = true;
			break;
		case 838:
			sourceRect = new Microsoft.Xna.Framework.Rectangle(576, 144, 16, 48);
			if (x == 10 && y == 5)
			{
				flip = true;
			}
			break;
		case 120:
			sourceRect = new Microsoft.Xna.Framework.Rectangle(512, 144, 16, 48);
			break;
		}
		this.Sprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, 100f, 4, 1, new Vector2(x, y - 2) * 64f, flicker: false, flip, (float)((y + 1) * 64 - 12) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
		{
			holdLastFrame = true,
			paused = true
		};
		if (base.Value)
		{
			this.Sprite.paused = false;
			this.Sprite.resetEnd();
		}
	}

	public virtual void ApplyMapModifications()
	{
		if (base.Value)
		{
			this.openDoorTiles();
		}
		else
		{
			this.closeDoorTiles();
		}
	}

	public void CleanUpLocalState()
	{
		this.closeDoorTiles();
	}

	private void closeDoorSprite()
	{
		this.Sprite.reset();
		this.Sprite.paused = true;
	}

	private void openDoorSprite()
	{
		this.Sprite.paused = false;
	}

	private void openDoorTiles()
	{
		this.Location.setTileProperty(this.Position.X, this.Position.Y, "Back", "TemporaryBarrier", "T");
		this.Location.removeTile(this.Position.X, this.Position.Y, "Buildings");
		DelayedAction.functionAfterDelay(delegate
		{
			this.Location.removeTileProperty(this.Position.X, this.Position.Y, "Back", "TemporaryBarrier");
		}, 400);
		this.Location.removeTile(this.Position.X, this.Position.Y - 1, "Front");
		this.Location.removeTile(this.Position.X, this.Position.Y - 2, "Front");
	}

	private void closeDoorTiles()
	{
		Location doorLocation = new Location(this.Position.X, this.Position.Y);
		Map map = this.Location.Map;
		if (map != null && this.Tile != null)
		{
			map.RequireLayer("Buildings").Tiles[doorLocation] = this.Tile;
			this.Location.removeTileProperty(this.Position.X, this.Position.Y, "Back", "TemporaryBarrier");
			doorLocation.Y--;
			map.RequireLayer("Front").Tiles[doorLocation] = new StaticTile(map.RequireLayer("Front"), this.Tile.TileSheet, BlendMode.Alpha, this.Tile.TileIndex - this.Tile.TileSheet.SheetWidth);
			doorLocation.Y--;
			map.RequireLayer("Front").Tiles[doorLocation] = new StaticTile(map.RequireLayer("Front"), this.Tile.TileSheet, BlendMode.Alpha, this.Tile.TileIndex - this.Tile.TileSheet.SheetWidth * 2);
		}
	}

	public void Update(GameTime time)
	{
		if (this.Sprite != null)
		{
			if (base.Value && this.Sprite.paused)
			{
				this.openDoorSprite();
				this.openDoorTiles();
			}
			else if (!base.Value && !this.Sprite.paused)
			{
				this.closeDoorSprite();
				this.closeDoorTiles();
			}
			this.Sprite.update(time);
		}
	}

	public void Draw(SpriteBatch b)
	{
		this.Sprite?.draw(b);
	}
}
