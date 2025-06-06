using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Delegates;
using StardewValley.Internal;

namespace StardewValley.Objects;

public class Sign : Object
{
	public const int OBJECT = 1;

	public const int HAT = 2;

	public const int BIG_OBJECT = 3;

	public const int RING = 4;

	public const int FURNITURE = 5;

	[XmlElement("displayItem")]
	public readonly NetRef<Item> displayItem = new NetRef<Item>();

	[XmlElement("displayType")]
	public readonly NetInt displayType = new NetInt();

	/// <inheritdoc />
	public override string TypeDefinitionId => "(BC)";

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.displayItem, "displayItem").AddField(this.displayType, "displayType");
	}

	public Sign()
	{
	}

	public Sign(Vector2 tile, string itemId)
		: base(tile, itemId)
	{
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return who.CurrentItem != null;
		}
		Item dropIn = who.CurrentItem;
		if (dropIn != null)
		{
			if (who.isMoving())
			{
				Game1.haltAfterCheck = false;
			}
			this.displayItem.Value = dropIn.getOne();
			Game1.playSound("coin");
			this.displayType.Value = 1;
			Item value = this.displayItem.Value;
			if (!(value is Hat))
			{
				if (!(value is Ring))
				{
					if (!(value is Furniture))
					{
						if (value is Object obj)
						{
							this.displayType.Value = ((!obj.bigCraftable.Value) ? 1 : 3);
						}
					}
					else
					{
						this.displayType.Value = 5;
					}
				}
				else
				{
					this.displayType.Value = 4;
				}
			}
			else
			{
				this.displayType.Value = 2;
			}
			return true;
		}
		return false;
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		base.draw(spriteBatch, x, y, alpha);
		if (this.displayItem.Value != null)
		{
			switch (this.displayType.Value)
			{
			case 1:
				this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) + 1f, y * 64 - 64 + 21 + 8 - 2)), 0.75f, 0.45f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 1E-05f, StackDrawType.Hide, Color.Black, drawShadow: false);
				this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) + 1f, y * 64 - 64 + 21 + 4 - 1)), 0.75f, 1f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 2E-05f, StackDrawType.Hide, Color.White, drawShadow: false);
				break;
			case 3:
				this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64 + 21 + 4 - 1)), 0.75f, 1f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 1E-05f, StackDrawType.Hide, Color.White, drawShadow: false);
				break;
			case 2:
				this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) + 1f, y * 64 - 64 + 21 + 8 - 1)), 0.75f, 0.45f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 1E-05f, StackDrawType.Hide, Color.Black, drawShadow: false);
				this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) + 1f, y * 64 - 64 + 21 + 4 - 1)), 0.75f, 1f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 2E-05f, StackDrawType.Hide, Color.White, drawShadow: false);
				break;
			case 4:
				this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) - 1f, y * 64 - 64 + 21 + 8 - 1)), 0.75f, 0.45f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 1E-05f, StackDrawType.Hide, Color.Black, drawShadow: false);
				this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) - 1f, y * 64 - 64 + 21 + 4 - 1)), 0.75f, 1f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 2E-05f, StackDrawType.Hide, Color.White, drawShadow: false);
				break;
			case 5:
				this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64 + 21 + 8 - 1)), 0.75f, 0.45f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 1E-05f, StackDrawType.Hide, Color.Black, drawShadow: false);
				this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64 + 21 + 4 - 1)), 0.75f, 1f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 2E-05f, StackDrawType.Hide, Color.White, drawShadow: false);
				break;
			}
		}
	}

	/// <inheritdoc />
	public override bool ForEachItem(ForEachItemDelegate handler, GetForEachItemPathDelegate getPath)
	{
		if (base.ForEachItem(handler, getPath))
		{
			return ForEachItemHelper.ApplyToField(this.displayItem, handler, getPath);
		}
		return false;
	}
}
