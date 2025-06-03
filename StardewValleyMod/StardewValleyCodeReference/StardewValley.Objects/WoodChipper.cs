using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects;

public class WoodChipper : Object
{
	public const int CHIP_TIME = 1000;

	public readonly NetRef<Object> depositedItem = new NetRef<Object>();

	protected bool _isAnimatingChip;

	public int nextSmokeTime;

	public int nextShakeTime;

	/// <inheritdoc />
	public override string TypeDefinitionId => "(BC)";

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.depositedItem, "depositedItem");
		this.depositedItem.fieldChangeVisibleEvent += OnDepositedItemChange;
	}

	public void OnDepositedItemChange(NetRef<Object> field, Object old_value, Object new_value)
	{
		if (Game1.gameMode != 6 && new_value != null)
		{
			base.shakeTimer = 1000;
			this._isAnimatingChip = true;
		}
	}

	public WoodChipper()
	{
	}

	public WoodChipper(Vector2 position)
		: base(position, "211")
	{
		this.Name = "Wood Chipper";
		base.type.Value = "Crafting";
		base.bigCraftable.Value = true;
		base.canBeSetDown.Value = true;
	}

	public override void addWorkingAnimation()
	{
		GameLocation environment = this.Location;
		if (environment != null && environment.farmers.Any() && Game1.random.NextDouble() < 0.35)
		{
			for (int i = 0; i < 8; i++)
			{
				environment.temporarySprites.Add(new TemporaryAnimatedSprite(47, base.tileLocation.Value * 64f + new Vector2(0f, -76 + Game1.random.Next(-48, 0)), new Color(200, 110, 17), 8, flipped: false, 50f, 0, -1, 0.003f + Math.Max(0f, ((base.tileLocation.Y + 1f) * 64f - 24f) / 10000f) + base.tileLocation.X * 1E-05f)
				{
					delayBeforeAnimationStart = i * 100
				});
			}
			environment.playSound("woodchipper_occasional");
			base.shakeTimer = 1500;
		}
	}

	/// <inheritdoc />
	public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
	{
		GameLocation location = this.Location;
		if (location == null)
		{
			return false;
		}
		Object dropped_in_object = dropInItem as Object;
		if (base.heldObject.Value != null || this.depositedItem.Value != null)
		{
			return base.performObjectDropInAction(dropInItem, probe, who, returnFalseIfItemConsumed);
		}
		if (dropped_in_object == null)
		{
			return false;
		}
		if (base.PlaceInMachine(base.GetMachineData(), dropped_in_object, probe, who))
		{
			if (!probe)
			{
				this.depositedItem.Value = dropInItem.getOne() as Object;
				base.shakeTimer = 1800;
				for (int i = 0; i < 12; i++)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite(47, base.tileLocation.Value * 64f + new Vector2(0f, -76 + Game1.random.Next(-48, 0)), new Color(200, 110, 17), 8, flipped: false, 50f, 0, -1, 0.003f + Math.Max(0f, ((base.tileLocation.Y + 1f) * 64f - 24f) / 10000f) + base.tileLocation.X * 1E-05f)
					{
						delayBeforeAnimationStart = i * 100
					});
				}
				if (returnFalseIfItemConsumed)
				{
					return false;
				}
			}
			return true;
		}
		return base.performObjectDropInAction(dropInItem, probe, who, returnFalseIfItemConsumed);
	}

	public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
	{
		this.TileLocation = new Vector2(x / 64, y / 64);
		return true;
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		if (who.IsLocalPlayer && base.heldObject.Value != null && base.readyForHarvest.Value)
		{
			if (!justCheckingForActivity)
			{
				Object collected_object = base.heldObject.Value;
				base.heldObject.Value = null;
				if (who.isMoving())
				{
					Game1.haltAfterCheck = false;
				}
				if (!who.addItemToInventoryBool(collected_object))
				{
					base.heldObject.Value = collected_object;
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
					return false;
				}
				Game1.playSound("coin");
				base.readyForHarvest.Value = false;
				this.depositedItem.Value = null;
				base.heldObject.Value = null;
				this.AttemptAutoLoad(who);
			}
			return true;
		}
		return base.checkForAction(who, justCheckingForActivity);
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		if (this.Location != null && this.depositedItem.Value != null && base.MinutesUntilReady > 0)
		{
			this.nextShakeTime -= time.ElapsedGameTime.Milliseconds;
			this.nextSmokeTime -= time.ElapsedGameTime.Milliseconds;
			if (this.nextSmokeTime <= 0)
			{
				this.nextSmokeTime = Game1.random.Next(3000, 6000);
			}
			if (this.nextShakeTime <= 0)
			{
				this.nextShakeTime = Game1.random.Next(1000, 2000);
				if (base.shakeTimer <= 0)
				{
					this._isAnimatingChip = false;
					base.shakeTimer = 0;
				}
			}
		}
		base.updateWhenCurrentLocation(time);
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		if (base.isTemporarilyInvisible)
		{
			return;
		}
		Vector2 scale_factor = Vector2.One;
		scale_factor *= 4f;
		Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
		Rectangle destination = new Rectangle((int)(position.X - scale_factor.X / 2f) + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scale_factor.Y / 2f) + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scale_factor.X), (int)(128f + scale_factor.Y / 2f));
		float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
		ParsedItemData baseDraw = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		Texture2D baseTexture = baseDraw.GetTexture();
		spriteBatch.Draw(baseTexture, destination, baseDraw.GetSourceRect(base.readyForHarvest.Value ? 1 : 0), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
		if (base.shakeTimer > 0)
		{
			spriteBatch.Draw(baseTexture, new Rectangle(destination.X, destination.Y + 4, destination.Width, 60), new Rectangle(80, 833, 16, 15), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer + 0.0035f);
		}
		if (this.depositedItem.Value != null && base.shakeTimer > 0 && this._isAnimatingChip)
		{
			float completion = 1f - (float)base.shakeTimer / 1000f;
			Vector2 end_position = position + new Vector2(32f, 32f);
			Vector2 start_position = end_position + new Vector2(0f, -16f);
			Vector2 draw_position = new Vector2
			{
				X = Utility.Lerp(start_position.X, end_position.X, completion),
				Y = Utility.Lerp(start_position.Y, end_position.Y, completion)
			};
			draw_position.X += Game1.random.Next(-1, 2) * 2;
			draw_position.Y += Game1.random.Next(-1, 2) * 2;
			float draw_scale = Utility.Lerp(1f, 0.75f, completion);
			ParsedItemData itemDraw = ItemRegistry.GetDataOrErrorItem(this.depositedItem.Value.QualifiedItemId);
			Texture2D itemTexture = itemDraw.GetTexture();
			spriteBatch.Draw(itemTexture, draw_position, itemDraw.GetSourceRect(), Color.White * alpha, 0f, new Vector2(8f, 8f), 4f * draw_scale, this.depositedItem.Value.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, draw_layer + 0.00175f);
		}
		if (this.depositedItem.Value != null && base.MinutesUntilReady > 0)
		{
			int frame = (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 200.0) / 50;
			spriteBatch.Draw(baseTexture, position + new Vector2(6f, 17f) * 4f, new Rectangle(80 + frame % 2 * 8, 848 + frame / 2 * 7, 8, 7), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer + 1E-05f);
			spriteBatch.Draw(baseTexture, position + new Vector2(3f, 9f) * 4f + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)), new Rectangle(51, 841, 10, 6), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer + 1E-05f);
		}
		if (!base.readyForHarvest.Value)
		{
			return;
		}
		float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
		spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 - 8, (float)(y * 64 - 96 - 16) + yOffset)), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-06f + base.tileLocation.X / 10000f);
		if (base.heldObject.Value != null)
		{
			ParsedItemData itemDraw2 = ItemRegistry.GetDataOrErrorItem(base.heldObject.Value.QualifiedItemId);
			Texture2D itemTexture2 = itemDraw2.GetTexture();
			spriteBatch.Draw(itemTexture2, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, (float)(y * 64 - 64 - 8) + yOffset)), itemDraw2.GetSourceRect(), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-05f + base.tileLocation.X / 10000f);
			if (base.heldObject.Value is ColoredObject coloredObject)
			{
				Rectangle coloredSourceRect = itemDraw2.GetSourceRect(1, base.ParentSheetIndex);
				spriteBatch.Draw(itemTexture2, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, (float)(y * 64 - 64 - 8) + yOffset)), coloredSourceRect, coloredObject.color.Value * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-05f + base.tileLocation.X / 10000f);
			}
		}
	}
}
