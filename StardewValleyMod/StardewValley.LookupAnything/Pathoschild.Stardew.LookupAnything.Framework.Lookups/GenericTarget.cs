using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using xTile.Dimensions;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups;

internal abstract class GenericTarget<TValue> : ITarget
{
	protected const int PrecedenceForFlooring = 999;

	protected const int PrecedenceForTile = 1000;

	protected GameHelper GameHelper { get; }

	public SubjectType Type { get; protected set; }

	public Vector2 Tile { get; protected set; }

	public TValue Value { get; }

	public Func<ISubject> GetSubject { get; protected set; }

	public int Precedence { get; protected set; }

	public abstract Rectangle GetSpritesheetArea();

	public virtual Rectangle GetWorldArea()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return this.GameHelper.GetScreenCoordinatesFromTile(this.Tile);
	}

	public virtual bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return this.Tile == tile;
	}

	protected GenericTarget(GameHelper gameHelper, SubjectType type, TValue value, Vector2 tilePosition, Func<ISubject> getSubject)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		this.GameHelper = gameHelper;
		this.Type = type;
		this.Value = value;
		this.Tile = tilePosition;
		this.GetSubject = getSubject;
	}

	protected Rectangle GetSpriteArea(Rectangle boundingBox, Rectangle sourceRectangle)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		int height = sourceRectangle.Height * 4;
		int width = sourceRectangle.Width * 4;
		int x = ((Rectangle)(ref boundingBox)).Center.X - width / 2;
		int y = boundingBox.Y + boundingBox.Height - height;
		return new Rectangle(x - ((Rectangle)(ref Game1.uiViewport)).X, y - ((Rectangle)(ref Game1.uiViewport)).Y, width, height);
	}

	protected bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea, Texture2D? spriteSheet, Rectangle spriteSourceRectangle, SpriteEffects spriteEffects = (SpriteEffects)0)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (spriteSheet == null)
		{
			return false;
		}
		Vector2 spriteSheetPosition = this.GameHelper.GetSpriteSheetCoordinates(position, spriteArea, spriteSourceRectangle, spriteEffects);
		if (!((Rectangle)(ref spriteSourceRectangle)).Contains((int)spriteSheetPosition.X, (int)spriteSheetPosition.Y))
		{
			return false;
		}
		Color pixel = this.GameHelper.GetSpriteSheetPixel<Color>(spriteSheet, spriteSheetPosition);
		return ((Color)(ref pixel)).A != 0;
	}
}
