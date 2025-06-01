using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buffs;
using StardewValley.Enchantments;
using StardewValley.GameData.Tools;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;

namespace StardewValley;

[XmlInclude(typeof(Axe))]
[XmlInclude(typeof(ErrorTool))]
[XmlInclude(typeof(FishingRod))]
[XmlInclude(typeof(GenericTool))]
[XmlInclude(typeof(Hoe))]
[XmlInclude(typeof(MeleeWeapon))]
[XmlInclude(typeof(MilkPail))]
[XmlInclude(typeof(Pan))]
[XmlInclude(typeof(Pickaxe))]
[XmlInclude(typeof(Shears))]
[XmlInclude(typeof(Slingshot))]
[XmlInclude(typeof(Wand))]
[XmlInclude(typeof(WateringCan))]
public abstract class Tool : Item
{
	public const int standardStaminaReduction = 2;

	public const int stone = 0;

	public const int copper = 1;

	public const int steel = 2;

	public const int gold = 3;

	public const int iridium = 4;

	public const int hammerSpriteIndex = 105;

	public const int wateringCanSpriteIndex = 273;

	public const int fishingRodSpriteIndex = 8;

	public const int wateringCanMenuIndex = 296;

	public const string weaponsTextureName = "TileSheets\\weapons";

	public static Texture2D weaponsTexture;

	[XmlElement("initialParentTileIndex")]
	public readonly NetInt initialParentTileIndex = new NetInt();

	[XmlElement("currentParentTileIndex")]
	public readonly NetInt currentParentTileIndex = new NetInt();

	[XmlElement("indexOfMenuItemView")]
	public readonly NetInt indexOfMenuItemView = new NetInt();

	[XmlElement("instantUse")]
	public readonly NetBool instantUse = new NetBool();

	[XmlElement("isEfficient")]
	public readonly NetBool isEfficient = new NetBool();

	[XmlElement("animationSpeedModifier")]
	public readonly NetFloat animationSpeedModifier = new NetFloat(1f);

	/// <summary>
	/// increments every swing. Not accurate for how many times the tool has been swung
	/// </summary>
	public int swingTicker = Game1.random.Next(999999);

	[XmlIgnore]
	private string _description;

	[XmlElement("upgradeLevel")]
	public readonly NetInt upgradeLevel = new NetInt();

	[XmlElement("numAttachmentSlots")]
	public readonly NetInt numAttachmentSlots = new NetInt();

	/// <summary>The last player who used this tool, if any.</summary>
	/// <remarks>Most code should use <see cref="M:StardewValley.Tool.getLastFarmerToUse" /> instead.</remarks>
	[XmlIgnore]
	public Farmer lastUser;

	public readonly NetObjectArray<Object> attachments = new NetObjectArray<Object>();

	/// <summary>The cached value for <see cref="P:StardewValley.Tool.DisplayName" />.</summary>
	[XmlIgnore]
	protected string displayName;

	[XmlElement("enchantments")]
	public readonly NetList<BaseEnchantment, NetRef<BaseEnchantment>> enchantments = new NetList<BaseEnchantment, NetRef<BaseEnchantment>>();

	[XmlElement("previousEnchantments")]
	public readonly NetStringList previousEnchantments = new NetStringList();

	/// <summary>Whether to play sounds when this tool is applied to a tile.</summary>
	/// <remarks>This should nearly always be true. It can be disabled for automated tools to avoid hitting audio instance limits.</remarks>
	[XmlIgnore]
	public bool PlayUseSounds = true;

	[XmlIgnore]
	public string description
	{
		get
		{
			if (this._description == null)
			{
				this._description = this.loadDescription();
			}
			return this._description;
		}
		set
		{
			this._description = value;
		}
	}

	/// <inheritdoc />
	public override string TypeDefinitionId { get; } = "(T)";

	/// <inheritdoc />
	[XmlIgnore]
	public override string DisplayName => this.loadDisplayName();

	public string Description => this.description;

	[XmlIgnore]
	public int CurrentParentTileIndex
	{
		get
		{
			return this.currentParentTileIndex.Value;
		}
		set
		{
			this.currentParentTileIndex.Set(value);
		}
	}

	public int InitialParentTileIndex
	{
		get
		{
			return this.initialParentTileIndex.Value;
		}
		set
		{
			this.initialParentTileIndex.Set(value);
		}
	}

	public int IndexOfMenuItemView
	{
		get
		{
			return this.indexOfMenuItemView.Value;
		}
		set
		{
			this.indexOfMenuItemView.Set(value);
		}
	}

	[XmlIgnore]
	public int UpgradeLevel
	{
		get
		{
			return this.upgradeLevel.Value;
		}
		set
		{
			this.upgradeLevel.Value = value;
		}
	}

	[XmlIgnore]
	public int AttachmentSlotsCount
	{
		get
		{
			return this.attachmentSlots();
		}
		set
		{
			this.numAttachmentSlots.Value = value;
			this.attachments.SetCount(value);
		}
	}

	public bool InstantUse
	{
		get
		{
			return this.instantUse.Value;
		}
		set
		{
			this.instantUse.Value = value;
		}
	}

	public bool IsEfficient
	{
		get
		{
			return this.isEfficient.Value;
		}
		set
		{
			this.isEfficient.Value = value;
		}
	}

	public float AnimationSpeedModifier
	{
		get
		{
			return this.animationSpeedModifier.Value;
		}
		set
		{
			this.animationSpeedModifier.Value = value;
		}
	}

	public Tool()
	{
		this.initNetFields();
		base.Category = -99;
	}

	public Tool(string name, int upgradeLevel, int initialParentTileIndex, int indexOfMenuItemView, bool stackable, int numAttachmentSlots = 0)
		: this()
	{
		this.Name = name ?? ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).InternalName;
		this.SetSpriteIndex(initialParentTileIndex);
		this.IndexOfMenuItemView = indexOfMenuItemView;
		this.AttachmentSlotsCount = Math.Max(0, numAttachmentSlots);
		base.Category = -99;
		this.UpgradeLevel = upgradeLevel;
	}

	/// <summary>Set the single sprite index to display for this tool.</summary>
	/// <param name="spriteIndex">The sprite index.</param>
	/// <remarks>This overrides upgrade level adjustments, so this should be called before setting the upgrade level for tools that have a dynamic sprite index.</remarks>
	public virtual void SetSpriteIndex(int spriteIndex)
	{
		this.InitialParentTileIndex = spriteIndex;
		this.IndexOfMenuItemView = spriteIndex;
		this.CurrentParentTileIndex = spriteIndex;
	}

	protected new virtual void initNetFields()
	{
		base.NetFields.SetOwner(this).AddField(this.initialParentTileIndex, "initialParentTileIndex").AddField(this.currentParentTileIndex, "currentParentTileIndex")
			.AddField(this.indexOfMenuItemView, "indexOfMenuItemView")
			.AddField(this.instantUse, "instantUse")
			.AddField(this.upgradeLevel, "upgradeLevel")
			.AddField(this.numAttachmentSlots, "numAttachmentSlots")
			.AddField(this.attachments, "attachments")
			.AddField(this.enchantments, "enchantments")
			.AddField(this.isEfficient, "isEfficient")
			.AddField(this.animationSpeedModifier, "animationSpeedModifier")
			.AddField(this.previousEnchantments, "previousEnchantments");
	}

	/// <inheritdoc />
	protected override void MigrateLegacyItemId()
	{
		base.ItemId = base.GetType().Name;
	}

	protected virtual string loadDisplayName()
	{
		return ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).DisplayName;
	}

	protected virtual string loadDescription()
	{
		return ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).Description;
	}

	/// <inheritdoc />
	public override bool CanBeLostOnDeath()
	{
		if (base.CanBeLostOnDeath())
		{
			return this.GetToolData()?.CanBeLostOnDeath ?? true;
		}
		return false;
	}

	/// <inheritdoc />
	public override string getCategoryName()
	{
		return Object.GetCategoryDisplayName(-99);
	}

	/// <inheritdoc />
	protected override void GetOneCopyFrom(Item source)
	{
		base.GetOneCopyFrom(source);
		if (source is Tool fromTool)
		{
			this.SetSpriteIndex(fromTool.InitialParentTileIndex);
			this.Name = source.Name;
			this.CurrentParentTileIndex = fromTool.CurrentParentTileIndex;
			this.IndexOfMenuItemView = fromTool.IndexOfMenuItemView;
			this.InstantUse = fromTool.InstantUse;
			this.IsEfficient = fromTool.IsEfficient;
			this.AnimationSpeedModifier = fromTool.AnimationSpeedModifier;
			this.UpgradeLevel = fromTool.UpgradeLevel;
			this.AttachmentSlotsCount = fromTool.AttachmentSlotsCount;
			this.CopyEnchantments(fromTool, this);
		}
	}

	/// <summary>Update this tool when it's created by upgrading a previous tool.</summary>
	/// <param name="other">The previous tool instance being upgraded into this tool.</param>
	public virtual void UpgradeFrom(Tool other)
	{
		this.CopyEnchantments(other, this);
	}

	/// <inheritdoc />
	public override Color getCategoryColor()
	{
		return Color.DarkSlateGray;
	}

	/// <summary>Get the underlying tool data from <c>Data/Tools</c>, if available.</summary>
	public ToolData GetToolData()
	{
		return ItemRegistry.GetData(base.QualifiedItemId)?.RawData as ToolData;
	}

	public virtual void draw(SpriteBatch b)
	{
		Farmer farmer = this.lastUser;
		if (farmer == null || farmer.toolPower.Value <= 0 || !this.lastUser.canReleaseTool || !this.lastUser.IsLocalPlayer)
		{
			return;
		}
		foreach (Vector2 v in this.tilesAffected(this.lastUser.GetToolLocation() / 64f, this.lastUser.toolPower.Value, this.lastUser))
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((int)v.X * 64, (int)v.Y * 64)), new Rectangle(194, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
		}
	}

	public override void drawAttachments(SpriteBatch b, int x, int y)
	{
		y += ((this.enchantments.Count > 0) ? 8 : 4);
		for (int slot = 0; slot < this.AttachmentSlotsCount; slot++)
		{
			this.DrawAttachmentSlot(slot, b, x, y + slot * 68);
		}
	}

	/// <summary>Draw an attachment slot at the given position.</summary>
	/// <param name="slot">The attachment slot index.</param>
	/// <param name="b">The sprite batch being drawn.</param>
	/// <param name="x">The X position at which to draw the slot.</param>
	/// <param name="y">The Y position at which to draw the slot.</param>
	/// <remarks>This should draw a 64x64 slot.</remarks>
	protected virtual void DrawAttachmentSlot(int slot, SpriteBatch b, int x, int y)
	{
		Vector2 pixel = new Vector2(x, y);
		this.GetAttachmentSlotSprite(slot, out var texture, out var sourceRect);
		b.Draw(texture, pixel, sourceRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
		this.attachments[slot]?.drawInMenu(b, pixel, 1f);
	}

	/// <summary>Get the sprite to draw for an attachment slot background.</summary>
	/// <param name="slot">The attachment slot index.</param>
	/// <param name="texture">The texture to draw.</param>
	/// <param name="sourceRect">The pixel area within the texture to draw.</param>
	protected virtual void GetAttachmentSlotSprite(int slot, out Texture2D texture, out Rectangle sourceRect)
	{
		texture = Game1.menuTexture;
		sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10);
	}

	public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
	{
		base.drawTooltip(spriteBatch, ref x, ref y, font, alpha, overrideText);
		foreach (BaseEnchantment enchantment in this.enchantments)
		{
			if (enchantment.ShouldBeDisplayed())
			{
				Utility.drawWithShadow(spriteBatch, Game1.mouseCursors2, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(127, 35, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
				Utility.drawTextWithShadow(spriteBatch, BaseEnchantment.hideEnchantmentName ? "???" : enchantment.GetDisplayName(), font, new Vector2(x + 16 + 52, y + 16 + 12), new Color(120, 0, 210) * 0.9f * alpha);
				y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
			}
		}
	}

	public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
	{
		Point dimensions = base.getExtraSpaceNeededForTooltipSpecialIcons(font, minWidth, horizontalBuffer, startingHeight, descriptionText, boldTitleText, moneyAmountToDisplayAtBottom);
		dimensions.Y = startingHeight;
		foreach (BaseEnchantment enchantment in this.enchantments)
		{
			if (enchantment.ShouldBeDisplayed())
			{
				dimensions.Y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
			}
		}
		return dimensions;
	}

	public virtual void tickUpdate(GameTime time, Farmer who)
	{
	}

	public virtual bool isHeavyHitter()
	{
		if (!(this is MeleeWeapon) && !(this is Hoe) && !(this is Axe))
		{
			return this is Pickaxe;
		}
		return true;
	}

	/// <summary>Get whether this is a scythe tool.</summary>
	public virtual bool isScythe()
	{
		return false;
	}

	public virtual void Update(int direction, int farmerMotionFrame, Farmer who)
	{
		int offset = 0;
		if (!(this is WateringCan))
		{
			if (this is FishingRod)
			{
				switch (direction)
				{
				case 0:
					offset = 3;
					break;
				case 1:
					offset = 0;
					break;
				case 3:
					offset = 0;
					break;
				}
			}
			else
			{
				switch (direction)
				{
				case 0:
					offset = 3;
					break;
				case 1:
					offset = 2;
					break;
				case 3:
					offset = 2;
					break;
				}
			}
		}
		else
		{
			switch (direction)
			{
			case 0:
				offset = 4;
				break;
			case 1:
				offset = 2;
				break;
			case 2:
				offset = 0;
				break;
			case 3:
				offset = 2;
				break;
			}
		}
		if (base.QualifiedItemId != "(T)WateringCan")
		{
			if (farmerMotionFrame < 1)
			{
				this.CurrentParentTileIndex = this.InitialParentTileIndex;
			}
			else if (who.FacingDirection == 0 || (who.FacingDirection == 2 && farmerMotionFrame >= 2))
			{
				this.CurrentParentTileIndex = this.InitialParentTileIndex + 1;
			}
		}
		else if (farmerMotionFrame < 5 || direction == 0)
		{
			this.CurrentParentTileIndex = this.InitialParentTileIndex;
		}
		else
		{
			this.CurrentParentTileIndex = this.InitialParentTileIndex + 1;
		}
		this.CurrentParentTileIndex += offset;
	}

	/// <inheritdoc />
	public override int salePrice(bool ignoreProfitMargins = false)
	{
		ToolData data = this.GetToolData();
		if (data == null || data.SalePrice < 0)
		{
			return base.salePrice(ignoreProfitMargins);
		}
		return data.SalePrice;
	}

	public override int attachmentSlots()
	{
		return this.numAttachmentSlots.Value;
	}

	/// <summary>Get the last player who used this tool, if any.</summary>
	public Farmer getLastFarmerToUse()
	{
		return this.lastUser;
	}

	public virtual void leftClick(Farmer who)
	{
	}

	public virtual void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
	{
		this.lastUser = who;
		Game1.recentMultiplayerRandom = Utility.CreateRandom((short)Game1.random.Next(-32768, 32768));
		if (this.isHeavyHitter() && !(this is MeleeWeapon))
		{
			Rumble.rumble(0.1f + (float)(Game1.random.NextDouble() / 4.0), 100 + Game1.random.Next(50));
			location.damageMonster(new Rectangle(x - 32, y - 32, 64, 64), this.upgradeLevel.Value + 1, (this.upgradeLevel.Value + 1) * 3, isBomb: false, who);
		}
		if (this is MeleeWeapon weapon && (!who.UsingTool || Game1.mouseClickPolling >= 50 || weapon.type.Value == 1 || !(weapon.ItemId != "47") || MeleeWeapon.timedHitTimer > 0 || who.FarmerSprite.currentAnimationIndex != 5 || !(who.FarmerSprite.timer < who.FarmerSprite.interval / 4f)))
		{
			if (weapon.type.Value == 2 && weapon.isOnSpecial)
			{
				weapon.triggerClubFunction(who);
			}
			else if (who.FarmerSprite.currentAnimationIndex > 0)
			{
				MeleeWeapon.timedHitTimer = 500;
			}
		}
	}

	public virtual void endUsing(GameLocation location, Farmer who)
	{
		this.swingTicker++;
		who.stopJittering();
		who.canReleaseTool = false;
		int addedAnimationMultiplayer = ((!(who.Stamina <= 0f)) ? 1 : 2);
		if (Game1.isAnyGamePadButtonBeingPressed() || !who.IsLocalPlayer)
		{
			who.lastClick = who.GetToolLocation();
		}
		if (this is WateringCan wateringCan)
		{
			if (wateringCan.WaterLeft > 0 && who.ShouldHandleAnimationSound() && this.PlayUseSounds)
			{
				who.playNearbySoundLocal("wateringCan");
			}
			switch (who.FacingDirection)
			{
			case 2:
				((FarmerSprite)who.Sprite).animateOnce(164, 125f * (float)addedAnimationMultiplayer, 3);
				break;
			case 1:
				((FarmerSprite)who.Sprite).animateOnce(172, 125f * (float)addedAnimationMultiplayer, 3);
				break;
			case 0:
				((FarmerSprite)who.Sprite).animateOnce(180, 125f * (float)addedAnimationMultiplayer, 3);
				break;
			case 3:
				((FarmerSprite)who.Sprite).animateOnce(188, 125f * (float)addedAnimationMultiplayer, 3);
				break;
			}
		}
		else if (this is FishingRod rod && who.IsLocalPlayer && Game1.activeClickableMenu == null)
		{
			if (!rod.hit)
			{
				this.DoFunction(who.currentLocation, (int)who.lastClick.X, (int)who.lastClick.Y, 1, who);
			}
		}
		else if (!(this is MeleeWeapon) && !(this is Pan) && !(this is Shears) && !(this is MilkPail) && !(this is Slingshot))
		{
			switch (who.FacingDirection)
			{
			case 0:
				((FarmerSprite)who.Sprite).animateOnce(176, 60f * (float)addedAnimationMultiplayer, 8);
				break;
			case 1:
				((FarmerSprite)who.Sprite).animateOnce(168, 60f * (float)addedAnimationMultiplayer, 8);
				break;
			case 2:
				((FarmerSprite)who.Sprite).animateOnce(160, 60f * (float)addedAnimationMultiplayer, 8);
				break;
			case 3:
				((FarmerSprite)who.Sprite).animateOnce(184, 60f * (float)addedAnimationMultiplayer, 8);
				break;
			}
		}
	}

	public virtual bool beginUsing(GameLocation location, int x, int y, Farmer who)
	{
		this.lastUser = who;
		if (!this.instantUse.Value)
		{
			who.Halt();
			this.Update(who.FacingDirection, 0, who);
			if ((!(this is FishingRod) && this.upgradeLevel.Value <= 0 && !(this is MeleeWeapon)) || this is Pickaxe)
			{
				who.EndUsingTool();
				return true;
			}
		}
		if (this.instantUse.Value)
		{
			Game1.toolAnimationDone(who);
			who.CanMove = true;
			who.canReleaseTool = false;
			who.UsingTool = false;
		}
		else if (this is WateringCan && location.CanRefillWateringCanOnTile((int)who.GetToolLocation().X / 64, (int)who.GetToolLocation().Y / 64))
		{
			switch (who.FacingDirection)
			{
			case 2:
				((FarmerSprite)who.Sprite).animateOnce(166, 250f, 2);
				this.Update(2, 1, who);
				break;
			case 1:
				((FarmerSprite)who.Sprite).animateOnce(174, 250f, 2);
				this.Update(1, 0, who);
				break;
			case 0:
				((FarmerSprite)who.Sprite).animateOnce(182, 250f, 2);
				this.Update(0, 1, who);
				break;
			case 3:
				((FarmerSprite)who.Sprite).animateOnce(190, 250f, 2);
				this.Update(3, 0, who);
				break;
			}
			who.canReleaseTool = false;
		}
		else if (this is WateringCan { WaterLeft: <=0 })
		{
			Game1.toolAnimationDone(who);
			who.CanMove = true;
			who.canReleaseTool = false;
		}
		else if (this is WateringCan)
		{
			who.jitterStrength = 0.25f;
			switch (who.FacingDirection)
			{
			case 0:
				who.FarmerSprite.setCurrentFrame(180);
				this.Update(0, 0, who);
				break;
			case 1:
				who.FarmerSprite.setCurrentFrame(172);
				this.Update(1, 0, who);
				break;
			case 2:
				who.FarmerSprite.setCurrentFrame(164);
				this.Update(2, 0, who);
				break;
			case 3:
				who.FarmerSprite.setCurrentFrame(188);
				this.Update(3, 0, who);
				break;
			}
		}
		else if (this is FishingRod)
		{
			switch (who.FacingDirection)
			{
			case 0:
				((FarmerSprite)who.Sprite).animateOnce(295, 35f, 8, FishingRod.endOfAnimationBehavior);
				this.Update(0, 0, who);
				break;
			case 1:
				((FarmerSprite)who.Sprite).animateOnce(296, 35f, 8, FishingRod.endOfAnimationBehavior);
				this.Update(1, 0, who);
				break;
			case 2:
				((FarmerSprite)who.Sprite).animateOnce(297, 35f, 8, FishingRod.endOfAnimationBehavior);
				this.Update(2, 0, who);
				break;
			case 3:
				((FarmerSprite)who.Sprite).animateOnce(298, 35f, 8, FishingRod.endOfAnimationBehavior);
				this.Update(3, 0, who);
				break;
			}
			who.canReleaseTool = false;
		}
		else if (this is MeleeWeapon)
		{
			((MeleeWeapon)this).setFarmerAnimating(who);
		}
		else
		{
			switch (who.FacingDirection)
			{
			case 0:
				who.FarmerSprite.setCurrentFrame(176);
				this.Update(0, 0, who);
				break;
			case 1:
				who.FarmerSprite.setCurrentFrame(168);
				this.Update(1, 0, who);
				break;
			case 2:
				who.FarmerSprite.setCurrentFrame(160);
				this.Update(2, 0, who);
				break;
			case 3:
				who.FarmerSprite.setCurrentFrame(184);
				this.Update(3, 0, who);
				break;
			}
		}
		return false;
	}

	public virtual bool onRelease(GameLocation location, int x, int y, Farmer who)
	{
		return false;
	}

	public override bool canBeDropped()
	{
		return false;
	}

	/// <summary>Get whether an item can be added to or removed from an attachment slot.</summary>
	/// <param name="o">The item to attach, or <c>null</c> to remove an attached item.</param>
	public virtual bool canThisBeAttached(Object o)
	{
		NetObjectArray<Object> netObjectArray = this.attachments;
		if (netObjectArray != null && netObjectArray.Count > 0)
		{
			if (o == null)
			{
				return true;
			}
			for (int slot = 0; slot < this.attachments.Length; slot++)
			{
				if (this.canThisBeAttached(o, slot))
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>Get whether an item can be added to or removed from an attachment slot.</summary>
	/// <param name="o">The item to attach.</param>
	/// <param name="slot">The slot index. This is always a valid index when the method is called.</param>
	protected virtual bool canThisBeAttached(Object o, int slot)
	{
		return true;
	}

	/// <summary>Add an item to or remove it from an attachment slot.</summary>
	/// <param name="o">The item to attach, or <c>null</c> to remove an attached item.</param>
	public virtual Object attach(Object o)
	{
		if (o == null)
		{
			for (int slot = 0; slot < this.attachments.Length; slot++)
			{
				Object oldObj = this.attachments[slot];
				if (oldObj != null)
				{
					this.attachments[slot] = null;
					Game1.playSound("dwop");
					return oldObj;
				}
			}
			return null;
		}
		int originalStack = o.Stack;
		for (int i = 0; i < this.attachments.Length; i++)
		{
			if (!this.canThisBeAttached(o, i))
			{
				continue;
			}
			Object oldObj2 = this.attachments[i];
			if (oldObj2 == null)
			{
				this.attachments[i] = o;
				o = null;
				break;
			}
			if (oldObj2.canStackWith(o))
			{
				int toRemove = o.Stack - oldObj2.addToStack(o);
				if (o.ConsumeStack(toRemove) == null)
				{
					o = null;
					break;
				}
			}
		}
		if (o == null || o.Stack != originalStack)
		{
			Game1.playSound("button1");
			return o;
		}
		for (int j = 0; j < this.attachments.Length; j++)
		{
			Object oldObj3 = this.attachments[j];
			this.attachments[j] = null;
			if (this.canThisBeAttached(o, j))
			{
				this.attachments[j] = o;
				Game1.playSound("button1");
				return oldObj3;
			}
			this.attachments[j] = oldObj3;
		}
		return o;
	}

	public virtual void actionWhenClaimed()
	{
		if (this is GenericTool)
		{
			int value = this.indexOfMenuItemView.Value;
			if ((uint)(value - 13) <= 3u)
			{
				Game1.player.trashCanLevel++;
			}
		}
	}

	public override bool CanBuyItem(Farmer who)
	{
		if (Game1.player.toolBeingUpgraded.Value == null && (this is Axe || this is Pickaxe || this is Hoe || this is WateringCan || (this is GenericTool && this.indexOfMenuItemView.Value >= 13 && this.indexOfMenuItemView.Value <= 16)))
		{
			return true;
		}
		return base.CanBuyItem(who);
	}

	/// <inheritdoc />
	public override bool actionWhenPurchased(string shopId)
	{
		if (shopId == "ClintUpgrade" && Game1.player.toolBeingUpgraded.Value == null)
		{
			if (this is Axe || this is Pickaxe || this is Hoe || this is WateringCan || this is Pan)
			{
				string previousToolId = ShopBuilder.GetToolUpgradeData(this.GetToolData(), Game1.player)?.RequireToolId;
				if (previousToolId != null)
				{
					Item oldItem = Game1.player.Items.GetById(previousToolId).FirstOrDefault();
					Game1.player.removeItemFromInventory(oldItem);
					if (oldItem is Tool oldTool)
					{
						this.UpgradeFrom(oldTool);
					}
				}
				Game1.player.toolBeingUpgraded.Value = (Tool)base.getOne();
				Game1.player.daysLeftForToolUpgrade.Value = 2;
				Game1.playSound("parry");
				Game1.exitActiveMenu();
				Game1.DrawDialogue(Game1.getCharacterFromName("Clint"), "Strings\\StringsFromCSFiles:Tool.cs.14317");
				return true;
			}
			if (this is GenericTool)
			{
				int value = this.indexOfMenuItemView.Value;
				if ((uint)(value - 13) <= 3u)
				{
					Game1.player.toolBeingUpgraded.Value = (Tool)base.getOne();
					Game1.player.daysLeftForToolUpgrade.Value = 2;
					Game1.playSound("parry");
					Game1.exitActiveMenu();
					Game1.DrawDialogue(Game1.getCharacterFromName("Clint"), "Strings\\StringsFromCSFiles:Tool.cs.14317");
					return true;
				}
			}
		}
		return base.actionWhenPurchased(shopId);
	}

	protected List<Vector2> tilesAffected(Vector2 tileLocation, int power, Farmer who)
	{
		power++;
		List<Vector2> tileLocations = new List<Vector2>();
		tileLocations.Add(tileLocation);
		Vector2 extremePowerPosition = Vector2.Zero;
		switch (who.FacingDirection)
		{
		case 0:
			if (power >= 6)
			{
				extremePowerPosition = new Vector2(tileLocation.X, tileLocation.Y - 2f);
				break;
			}
			if (power >= 2)
			{
				tileLocations.Add(tileLocation + new Vector2(0f, -1f));
				tileLocations.Add(tileLocation + new Vector2(0f, -2f));
			}
			if (power >= 3)
			{
				tileLocations.Add(tileLocation + new Vector2(0f, -3f));
				tileLocations.Add(tileLocation + new Vector2(0f, -4f));
			}
			if (power >= 4)
			{
				tileLocations.RemoveAt(tileLocations.Count - 1);
				tileLocations.RemoveAt(tileLocations.Count - 1);
				tileLocations.Add(tileLocation + new Vector2(1f, -2f));
				tileLocations.Add(tileLocation + new Vector2(1f, -1f));
				tileLocations.Add(tileLocation + new Vector2(1f, 0f));
				tileLocations.Add(tileLocation + new Vector2(-1f, -2f));
				tileLocations.Add(tileLocation + new Vector2(-1f, -1f));
				tileLocations.Add(tileLocation + new Vector2(-1f, 0f));
			}
			if (power >= 5)
			{
				for (int i3 = tileLocations.Count - 1; i3 >= 0; i3--)
				{
					tileLocations.Add(tileLocations[i3] + new Vector2(0f, -3f));
				}
			}
			break;
		case 1:
			if (power >= 6)
			{
				extremePowerPosition = new Vector2(tileLocation.X + 2f, tileLocation.Y);
				break;
			}
			if (power >= 2)
			{
				tileLocations.Add(tileLocation + new Vector2(1f, 0f));
				tileLocations.Add(tileLocation + new Vector2(2f, 0f));
			}
			if (power >= 3)
			{
				tileLocations.Add(tileLocation + new Vector2(3f, 0f));
				tileLocations.Add(tileLocation + new Vector2(4f, 0f));
			}
			if (power >= 4)
			{
				tileLocations.RemoveAt(tileLocations.Count - 1);
				tileLocations.RemoveAt(tileLocations.Count - 1);
				tileLocations.Add(tileLocation + new Vector2(0f, -1f));
				tileLocations.Add(tileLocation + new Vector2(1f, -1f));
				tileLocations.Add(tileLocation + new Vector2(2f, -1f));
				tileLocations.Add(tileLocation + new Vector2(0f, 1f));
				tileLocations.Add(tileLocation + new Vector2(1f, 1f));
				tileLocations.Add(tileLocation + new Vector2(2f, 1f));
			}
			if (power >= 5)
			{
				for (int i2 = tileLocations.Count - 1; i2 >= 0; i2--)
				{
					tileLocations.Add(tileLocations[i2] + new Vector2(3f, 0f));
				}
			}
			break;
		case 2:
			if (power >= 6)
			{
				extremePowerPosition = new Vector2(tileLocation.X, tileLocation.Y + 2f);
				break;
			}
			if (power >= 2)
			{
				tileLocations.Add(tileLocation + new Vector2(0f, 1f));
				tileLocations.Add(tileLocation + new Vector2(0f, 2f));
			}
			if (power >= 3)
			{
				tileLocations.Add(tileLocation + new Vector2(0f, 3f));
				tileLocations.Add(tileLocation + new Vector2(0f, 4f));
			}
			if (power >= 4)
			{
				tileLocations.RemoveAt(tileLocations.Count - 1);
				tileLocations.RemoveAt(tileLocations.Count - 1);
				tileLocations.Add(tileLocation + new Vector2(1f, 2f));
				tileLocations.Add(tileLocation + new Vector2(1f, 1f));
				tileLocations.Add(tileLocation + new Vector2(1f, 0f));
				tileLocations.Add(tileLocation + new Vector2(-1f, 2f));
				tileLocations.Add(tileLocation + new Vector2(-1f, 1f));
				tileLocations.Add(tileLocation + new Vector2(-1f, 0f));
			}
			if (power >= 5)
			{
				for (int i4 = tileLocations.Count - 1; i4 >= 0; i4--)
				{
					tileLocations.Add(tileLocations[i4] + new Vector2(0f, 3f));
				}
			}
			break;
		case 3:
			if (power >= 6)
			{
				extremePowerPosition = new Vector2(tileLocation.X - 2f, tileLocation.Y);
				break;
			}
			if (power >= 2)
			{
				tileLocations.Add(tileLocation + new Vector2(-1f, 0f));
				tileLocations.Add(tileLocation + new Vector2(-2f, 0f));
			}
			if (power >= 3)
			{
				tileLocations.Add(tileLocation + new Vector2(-3f, 0f));
				tileLocations.Add(tileLocation + new Vector2(-4f, 0f));
			}
			if (power >= 4)
			{
				tileLocations.RemoveAt(tileLocations.Count - 1);
				tileLocations.RemoveAt(tileLocations.Count - 1);
				tileLocations.Add(tileLocation + new Vector2(0f, -1f));
				tileLocations.Add(tileLocation + new Vector2(-1f, -1f));
				tileLocations.Add(tileLocation + new Vector2(-2f, -1f));
				tileLocations.Add(tileLocation + new Vector2(0f, 1f));
				tileLocations.Add(tileLocation + new Vector2(-1f, 1f));
				tileLocations.Add(tileLocation + new Vector2(-2f, 1f));
			}
			if (power >= 5)
			{
				for (int i = tileLocations.Count - 1; i >= 0; i--)
				{
					tileLocations.Add(tileLocations[i] + new Vector2(-3f, 0f));
				}
			}
			break;
		}
		if (power >= 6)
		{
			tileLocations.Clear();
			for (int x = (int)extremePowerPosition.X - 2; (float)x <= extremePowerPosition.X + 2f; x++)
			{
				for (int y = (int)extremePowerPosition.Y - 2; (float)y <= extremePowerPosition.Y + 2f; y++)
				{
					tileLocations.Add(new Vector2(x, y));
				}
			}
		}
		return tileLocations;
	}

	public virtual bool doesShowTileLocationMarker()
	{
		return true;
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
		ParsedItemData data = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		spriteBatch.Draw(data.GetTexture(), location + new Vector2(32f, 32f), data.GetSourceRect(), color * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
		this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
	}

	public override bool isPlaceable()
	{
		return false;
	}

	public override int maximumStackSize()
	{
		return 1;
	}

	public override string getDescription()
	{
		return Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth());
	}

	protected override int getDescriptionWidth()
	{
		int amount = base.getDescriptionWidth();
		foreach (BaseEnchantment e in this.enchantments)
		{
			amount = Math.Max(amount, (int)(Game1.smallFont.MeasureString(e.GetDisplayName()).X + 128f));
		}
		return amount;
	}

	public virtual void ClearEnchantments()
	{
		for (int i = this.enchantments.Count - 1; i >= 0; i--)
		{
			this.enchantments[i].UnapplyTo(this);
		}
		this.enchantments.Clear();
	}

	public virtual int GetMaxForges()
	{
		return 0;
	}

	public virtual bool CanAddEnchantment(BaseEnchantment enchantment)
	{
		if (!enchantment.IsForge() && !enchantment.IsSecondaryEnchantment())
		{
			return true;
		}
		if (this.GetTotalForgeLevels() >= this.GetMaxForges() && !enchantment.IsSecondaryEnchantment())
		{
			return false;
		}
		if (enchantment != null)
		{
			foreach (BaseEnchantment existing_enchantment in this.enchantments)
			{
				if (enchantment.GetType() == existing_enchantment.GetType())
				{
					if (existing_enchantment.GetMaximumLevel() < 0 || existing_enchantment.GetLevel() < existing_enchantment.GetMaximumLevel())
					{
						return true;
					}
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public virtual void CopyEnchantments(Tool source, Tool destination)
	{
		foreach (BaseEnchantment enchantment in source.enchantments)
		{
			destination.enchantments.Add(enchantment.GetOne());
			enchantment.GetOne().ApplyTo(destination);
		}
		destination.previousEnchantments.Clear();
		destination.previousEnchantments.AddRange(source.previousEnchantments);
	}

	public int GetTotalForgeLevels(bool for_unforge = false)
	{
		int total = 0;
		foreach (BaseEnchantment existing_enchantment in this.enchantments)
		{
			if (existing_enchantment is DiamondEnchantment)
			{
				if (for_unforge)
				{
					return total;
				}
			}
			else if (existing_enchantment.IsForge())
			{
				total += existing_enchantment.GetLevel();
			}
		}
		return total;
	}

	public virtual bool AddEnchantment(BaseEnchantment enchantment)
	{
		if (enchantment != null)
		{
			if (this is MeleeWeapon && (enchantment.IsForge() || enchantment.IsSecondaryEnchantment()))
			{
				foreach (BaseEnchantment existing_enchantment in this.enchantments)
				{
					if (enchantment.GetType() == existing_enchantment.GetType())
					{
						if (existing_enchantment.GetMaximumLevel() < 0 || existing_enchantment.GetLevel() < existing_enchantment.GetMaximumLevel())
						{
							existing_enchantment.SetLevel(this, existing_enchantment.GetLevel() + 1);
							return true;
						}
						return false;
					}
				}
				this.enchantments.Add(enchantment);
				enchantment.ApplyTo(this, this.lastUser);
				return true;
			}
			for (int i = this.enchantments.Count - 1; i >= 0; i--)
			{
				BaseEnchantment prevEnchantment = this.enchantments[i];
				if (!prevEnchantment.IsForge() && !prevEnchantment.IsSecondaryEnchantment())
				{
					prevEnchantment.UnapplyTo(this);
					this.enchantments.RemoveAt(i);
				}
			}
			this.enchantments.Add(enchantment);
			enchantment.ApplyTo(this, this.lastUser);
			return true;
		}
		return false;
	}

	public bool hasEnchantmentOfType<T>()
	{
		foreach (BaseEnchantment enchantment in this.enchantments)
		{
			if (enchantment is T)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void RemoveEnchantment(BaseEnchantment enchantment)
	{
		if (enchantment != null)
		{
			this.enchantments.Remove(enchantment);
			enchantment.UnapplyTo(this, this.lastUser);
		}
	}

	public override void actionWhenBeingHeld(Farmer who)
	{
		base.actionWhenBeingHeld(who);
		if (!who.IsLocalPlayer)
		{
			return;
		}
		foreach (BaseEnchantment enchantment in this.enchantments)
		{
			enchantment.OnEquip(who);
		}
	}

	public override void actionWhenStopBeingHeld(Farmer who)
	{
		base.actionWhenStopBeingHeld(who);
		if (who.UsingTool)
		{
			who.UsingTool = false;
			if (who.FarmerSprite.PauseForSingleAnimation)
			{
				who.FarmerSprite.PauseForSingleAnimation = false;
			}
		}
		if (!who.IsLocalPlayer)
		{
			return;
		}
		foreach (BaseEnchantment enchantment in this.enchantments)
		{
			enchantment.OnUnequip(who);
		}
	}

	public virtual bool CanUseOnStandingTile()
	{
		return false;
	}

	public override void AddEquipmentEffects(BuffEffects effects)
	{
		base.AddEquipmentEffects(effects);
		if (this.hasEnchantmentOfType<MasterEnchantment>())
		{
			effects.FishingLevel.Value += 1f;
		}
	}

	public virtual bool CanForge(Item item)
	{
		BaseEnchantment enchantment = BaseEnchantment.GetEnchantmentFromItem(this, item);
		if (enchantment != null && this.CanAddEnchantment(enchantment))
		{
			return true;
		}
		if (item != null && item.QualifiedItemId == "(O)852" && this is MeleeWeapon weapon && weapon.getItemLevel() < 15 && !this.Name.Contains("Galaxy"))
		{
			return true;
		}
		return false;
	}

	public T GetEnchantmentOfType<T>() where T : BaseEnchantment
	{
		foreach (BaseEnchantment existing_enchantment in this.enchantments)
		{
			if (existing_enchantment.GetType() == typeof(T))
			{
				return existing_enchantment as T;
			}
		}
		return null;
	}

	public int GetEnchantmentLevel<T>() where T : BaseEnchantment
	{
		int total = 0;
		foreach (BaseEnchantment existing_enchantment in this.enchantments)
		{
			if (existing_enchantment.GetType() == typeof(T))
			{
				total += existing_enchantment.GetLevel();
			}
		}
		return total;
	}

	public virtual bool Forge(Item item, bool count_towards_stats = false)
	{
		BaseEnchantment enchantment = BaseEnchantment.GetEnchantmentFromItem(this, item);
		if (enchantment != null)
		{
			if (this.AddEnchantment(enchantment))
			{
				if (!(enchantment is DiamondEnchantment))
				{
					if (enchantment is GalaxySoulEnchantment && this is MeleeWeapon weapon && weapon.isGalaxyWeapon() && weapon.GetEnchantmentLevel<GalaxySoulEnchantment>() >= 3)
					{
						string newItemId = null;
						switch (base.QualifiedItemId)
						{
						case "(W)4":
							newItemId = "62";
							break;
						case "(W)29":
							newItemId = "63";
							break;
						case "(W)23":
							newItemId = "64";
							break;
						}
						if (newItemId != null)
						{
							weapon.transform(newItemId);
							if (count_towards_stats)
							{
								DelayedAction.playSoundAfterDelay("discoverMineral", 400);
								Game1.multiplayer.globalChatInfoMessage("InfinityWeapon", Game1.player.name.Value, TokenStringBuilder.ItemNameFor(this));
								Game1.getAchievement(42);
							}
						}
						GalaxySoulEnchantment enchant = this.GetEnchantmentOfType<GalaxySoulEnchantment>();
						if (enchant != null)
						{
							this.RemoveEnchantment(enchant);
						}
					}
				}
				else
				{
					int forges_left = this.GetMaxForges() - this.GetTotalForgeLevels();
					List<int> valid_forges = new List<int>();
					if (!this.hasEnchantmentOfType<EmeraldEnchantment>())
					{
						valid_forges.Add(0);
					}
					if (!this.hasEnchantmentOfType<AquamarineEnchantment>())
					{
						valid_forges.Add(1);
					}
					if (!this.hasEnchantmentOfType<RubyEnchantment>())
					{
						valid_forges.Add(2);
					}
					if (!this.hasEnchantmentOfType<AmethystEnchantment>())
					{
						valid_forges.Add(3);
					}
					if (!this.hasEnchantmentOfType<TopazEnchantment>())
					{
						valid_forges.Add(4);
					}
					if (!this.hasEnchantmentOfType<JadeEnchantment>())
					{
						valid_forges.Add(5);
					}
					for (int i = 0; i < forges_left; i++)
					{
						if (valid_forges.Count == 0)
						{
							break;
						}
						int index = Game1.random.Next(valid_forges.Count);
						int random_enchant = valid_forges[index];
						valid_forges.RemoveAt(index);
						switch (random_enchant)
						{
						case 0:
							this.AddEnchantment(new EmeraldEnchantment());
							break;
						case 1:
							this.AddEnchantment(new AquamarineEnchantment());
							break;
						case 2:
							this.AddEnchantment(new RubyEnchantment());
							break;
						case 3:
							this.AddEnchantment(new AmethystEnchantment());
							break;
						case 4:
							this.AddEnchantment(new TopazEnchantment());
							break;
						case 5:
							this.AddEnchantment(new JadeEnchantment());
							break;
						}
					}
				}
				if (count_towards_stats && !enchantment.IsForge())
				{
					this.previousEnchantments.Insert(0, enchantment.GetName());
					while (this.previousEnchantments.Count > 2)
					{
						this.previousEnchantments.RemoveAt(this.previousEnchantments.Count - 1);
					}
					Game1.stats.Increment("timesEnchanted");
				}
				return true;
			}
		}
		else if (item.QualifiedItemId == "(O)852" && this is MeleeWeapon weapon2)
		{
			List<BaseEnchantment> oldEnchantments = new List<BaseEnchantment>();
			weapon2.enchantments.RemoveWhere(delegate(BaseEnchantment curEnchantment)
			{
				if (curEnchantment.IsSecondaryEnchantment() && !(curEnchantment is GalaxySoulEnchantment))
				{
					oldEnchantments.Add(curEnchantment);
					return true;
				}
				return false;
			});
			MeleeWeapon.attemptAddRandomInnateEnchantment(weapon2, Game1.random, force: true, oldEnchantments);
			return true;
		}
		return false;
	}
}
