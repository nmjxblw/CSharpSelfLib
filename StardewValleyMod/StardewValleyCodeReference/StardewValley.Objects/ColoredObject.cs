using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects;

public class ColoredObject : Object
{
	[XmlElement("color")]
	public readonly NetColor color = new NetColor();

	[XmlElement("colorSameIndexAsParentSheetIndex")]
	public readonly NetBool colorSameIndexAsParentSheetIndex = new NetBool();

	public bool ColorSameIndexAsParentSheetIndex
	{
		get
		{
			return this.colorSameIndexAsParentSheetIndex.Value;
		}
		set
		{
			this.colorSameIndexAsParentSheetIndex.Value = value;
		}
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.color, "color").AddField(this.colorSameIndexAsParentSheetIndex, "colorSameIndexAsParentSheetIndex");
	}

	public ColoredObject()
	{
	}

	public ColoredObject(string itemId, int stack, Color color)
		: base(itemId, stack)
	{
		this.color.Value = color;
		if (Game1.objectData.TryGetValue(base.ItemId, out var data))
		{
			this.ColorSameIndexAsParentSheetIndex = !data.ColorOverlayFromNextIndex;
		}
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color colorOverride, bool drawShadow)
	{
		base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
		if (drawShadow && !base.bigCraftable.Value && base.QualifiedItemId != "(O)590" && base.QualifiedItemId != "(O)SeedSpot")
		{
			this.DrawShadow(spriteBatch, location, colorOverride, layerDepth);
		}
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.ItemId);
		Texture2D texture = itemData.GetTexture();
		Vector2 origin = (base.bigCraftable.Value ? new Vector2(32f, 64f) : new Vector2(8f, 8f));
		float scale = ((!base.bigCraftable.Value) ? (4f * scaleSize) : ((scaleSize < 0.2f) ? scaleSize : (scaleSize / 2f)));
		if (base.ItemId == "SmokedFish")
		{
			this.drawSmokedFish(spriteBatch, location, scaleSize, layerDepth, (transparency == 1f && colorOverride.A < byte.MaxValue) ? ((float)(int)colorOverride.A / 255f) : transparency);
		}
		else if (!this.ColorSameIndexAsParentSheetIndex)
		{
			Rectangle coloredSourceRect = itemData.GetSourceRect(1, base.ParentSheetIndex);
			transparency = ((transparency == 1f && colorOverride.A < byte.MaxValue) ? ((float)(int)colorOverride.A / 255f) : transparency);
			spriteBatch.Draw(texture, location + new Vector2(32f, 32f) * scaleSize, itemData.GetSourceRect(0, base.ParentSheetIndex), Color.White * transparency, 0f, origin * scaleSize, scale, SpriteEffects.None, layerDepth);
			spriteBatch.Draw(texture, location + new Vector2(32f, 32f) * scaleSize, coloredSourceRect, this.color.Value * transparency, 0f, origin * scaleSize, scale, SpriteEffects.None, Math.Min(1f, layerDepth + 2E-05f));
		}
		else
		{
			spriteBatch.Draw(texture, location + new Vector2(32f, 32f) * scaleSize, itemData.GetSourceRect(0, base.ParentSheetIndex), this.color.Value * transparency, 0f, origin * scaleSize, scale, SpriteEffects.None, Math.Min(1f, layerDepth + 2E-05f));
		}
		this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth + 3E-05f, drawStackNumber, colorOverride);
	}

	private void drawSmokedFish(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float layerDepth, float transparency = 1f)
	{
		Vector2 origin = new Vector2(8f, 8f);
		float scale = 4f * scaleSize;
		ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(base.preservedParentSheetIndex.Value);
		Texture2D itemTexture = dataOrErrorItem.GetTexture();
		Rectangle sourceRect = dataOrErrorItem.GetSourceRect();
		spriteBatch.Draw(itemTexture, location + new Vector2(32f, 32f) * scaleSize, sourceRect, Color.White * transparency, 0f, origin * scaleSize, scale, SpriteEffects.None, Math.Min(1f, layerDepth + 1E-05f));
		spriteBatch.Draw(itemTexture, location + new Vector2(32f, 32f) * scaleSize, sourceRect, new Color(80, 30, 10) * 0.6f * transparency, 0f, origin * scaleSize, scale, SpriteEffects.None, Math.Min(1f, layerDepth + 1.5E-05f));
		int interval = 700 + (base.price.Value + 17) * 7777 % 200;
		spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(32f, 32f) * scaleSize + new Vector2(0f, (float)((0.0 - Game1.currentGameTime.TotalGameTime.TotalMilliseconds) % 2000.0) * 0.03f), new Rectangle(372, 1956, 10, 10), new Color(80, 80, 80) * transparency * 0.53f * (1f - (float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2000.0) / 2000f), (float)((0.0 - Game1.currentGameTime.TotalGameTime.TotalMilliseconds) % 2000.0) * 0.001f, origin * scaleSize, scale / 2f, SpriteEffects.None, Math.Min(1f, layerDepth + 2E-05f));
		spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(24f, 40f) * scaleSize + new Vector2(0f, (float)((0.0 - (Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)interval)) % 2000.0) * 0.03f), new Rectangle(372, 1956, 10, 10), new Color(80, 80, 80) * transparency * 0.53f * (1f - (float)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)interval) % 2000.0) / 2000f), (float)((0.0 - (Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)interval)) % 2000.0) * 0.001f, origin * scaleSize, scale / 2f, SpriteEffects.None, Math.Min(1f, layerDepth + 2E-05f));
		spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(48f, 21f) * scaleSize + new Vector2(0f, (float)((0.0 - (Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(interval * 2))) % 2000.0) * 0.03f), new Rectangle(372, 1956, 10, 10), new Color(80, 80, 80) * transparency * 0.53f * (1f - (float)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(interval * 2)) % 2000.0) / 2000f), (float)((0.0 - (Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(interval * 2))) % 2000.0) * 0.001f, origin * scaleSize, scale / 2f, SpriteEffects.None, Math.Min(1f, layerDepth + 2E-05f));
	}

	public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
	{
		if (base.ItemId == "SmokedFish")
		{
			this.drawSmokedFish(spriteBatch, objectPosition, 1f, f.getDrawLayer() + 1E-05f);
		}
		else if (!this.ColorSameIndexAsParentSheetIndex)
		{
			base.drawWhenHeld(spriteBatch, objectPosition, f);
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			spriteBatch.Draw(itemData.GetTexture(), objectPosition, itemData.GetSourceRect(1, base.ParentSheetIndex), this.color.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.StandingPixel.Y + 4) / 10000f));
		}
		else
		{
			ParsedItemData itemData2 = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			spriteBatch.Draw(itemData2.GetTexture(), objectPosition, itemData2.GetSourceRect(0, base.ParentSheetIndex), this.color.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.StandingPixel.Y + 4) / 10000f));
		}
	}

	/// <summary>Get the hue value for the current <see cref="F:StardewValley.Objects.ColoredObject.color" />.</summary>
	public double GetHue()
	{
		Color curColor = this.color.Value;
		Utility.RGBtoHSL(curColor.R, curColor.G, curColor.B, out var hue, out var _, out var _);
		return hue;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new ColoredObject(base.ItemId, 1, this.color.Value);
	}

	/// <inheritdoc />
	protected override void GetOneCopyFrom(Item source)
	{
		base.GetOneCopyFrom(source);
		if (source is ColoredObject fromObj)
		{
			base.preserve.Value = fromObj.preserve.Value;
			base.preservedParentSheetIndex.Value = fromObj.preservedParentSheetIndex.Value;
			this.Name = fromObj.Name;
			this.colorSameIndexAsParentSheetIndex.Value = fromObj.colorSameIndexAsParentSheetIndex.Value;
		}
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		if (base.bigCraftable.Value)
		{
			Vector2 scaleFactor = this.getScale();
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
			Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f), (int)(position.Y - scaleFactor.Y / 2f), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
			int indexOffset = 0;
			if (base.showNextIndex.Value)
			{
				indexOffset = 1;
			}
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			Texture2D texture = itemData.GetTexture();
			if (!this.ColorSameIndexAsParentSheetIndex)
			{
				Rectangle coloredSourceRect = itemData.GetSourceRect(indexOffset + 1, base.ParentSheetIndex);
				spriteBatch.Draw(texture, destination, itemData.GetSourceRect(indexOffset, base.ParentSheetIndex), Color.White, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
				spriteBatch.Draw(texture, destination, coloredSourceRect, this.color.Value, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
			}
			else
			{
				spriteBatch.Draw(texture, destination, itemData.GetSourceRect(0, base.ParentSheetIndex), this.color.Value, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
			}
			if (base.QualifiedItemId == "(BC)17" && base.MinutesUntilReady > 0)
			{
				spriteBatch.Draw(Game1.objectSpriteSheet, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 0f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, 16, 16), Color.White, base.scale.X, new Vector2(32f, 32f), 1f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
			}
		}
		else if (!Game1.eventUp || this.Location.IsFarm)
		{
			if (base.QualifiedItemId != "(O)590")
			{
				spriteBatch.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 1E-07f);
			}
			ParsedItemData itemData2 = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			Texture2D texture2 = itemData2.GetTexture();
			Rectangle bounds = this.GetBoundingBoxAt(x, y);
			if (!this.ColorSameIndexAsParentSheetIndex)
			{
				Rectangle coloredSourceRect2 = itemData2.GetSourceRect(1, base.ParentSheetIndex);
				spriteBatch.Draw(texture2, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 32)), itemData2.GetSourceRect(0, base.ParentSheetIndex), Color.White, 0f, new Vector2(8f, 8f), (base.scale.Y > 1f) ? this.getScale().Y : 4f, base.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)bounds.Bottom / 10000f);
				spriteBatch.Draw(texture2, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), coloredSourceRect2, this.color.Value, 0f, new Vector2(8f, 8f), (base.scale.Y > 1f) ? this.getScale().Y : 4f, base.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)bounds.Bottom / 10000f);
			}
			else
			{
				spriteBatch.Draw(texture2, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), itemData2.GetSourceRect(0, base.ParentSheetIndex), this.color.Value, 0f, new Vector2(8f, 8f), (base.scale.Y > 1f) ? this.getScale().Y : 4f, base.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)bounds.Bottom / 10000f);
			}
		}
	}

	/// <summary>Set the tint color for an item, if it's a <see cref="T:StardewValley.Objects.ColoredObject" /> or can be converted to one.</summary>
	/// <param name="input">The input item whose color to set.</param>
	/// <param name="color">The tint color to apply.</param>
	/// <param name="coloredItem">The resulting colored item. This may be <paramref name="input" /> (if it was already a <see cref="T:StardewValley.Objects.ColoredObject" />), a new item (if the <paramref name="input" /> can be converted to a <see cref="T:StardewValley.Objects.ColoredObject" />), else null.</param>
	/// <returns>Returns whether the <paramref name="coloredItem" /> was successfully set.</returns>
	public static bool TrySetColor(Item input, Color color, out ColoredObject coloredItem)
	{
		if (input == null)
		{
			coloredItem = null;
			return false;
		}
		coloredItem = input as ColoredObject;
		if (coloredItem != null)
		{
			coloredItem.color.Value = color;
			return true;
		}
		if (input.HasTypeObject())
		{
			coloredItem = new ColoredObject(input.ItemId, input.Stack, color);
			coloredItem.CopyFieldsFrom(input);
			coloredItem.color.Value = color;
			return true;
		}
		coloredItem = null;
		return false;
	}
}
