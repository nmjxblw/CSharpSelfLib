using System;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Monsters;

namespace StardewValley.Objects;

[XmlInclude(typeof(CombinedRing))]
public class Ring : Item
{
	public const string SmallGlowRingId = "516";

	public const string GlowRingId = "517";

	public const string SmallMagnetRingId = "518";

	public const string MagnetRingId = "519";

	public const string SlimeCharmerRingId = "520";

	public const string WarriorRingId = "521";

	public const string VampireRingId = "522";

	public const string SavageRingId = "523";

	public const string YobaRingId = "524";

	public const string SturdyRingId = "525";

	public const string BurglarsRingId = "526";

	public const string IridiumBandId = "527";

	public const string AmethystRingId = "529";

	public const string TopazRingId = "530";

	public const string AquamarineRingId = "531";

	public const string JadeRingId = "532";

	public const string EmeraldRingId = "533";

	public const string RubyRingId = "534";

	public const string WeddingRingId = "801";

	public const string CrabshellRingId = "810";

	public const string NapalmRingId = "811";

	public const string ThornsRingId = "839";

	public const string LuckyRingId = "859";

	public const string HotJavaRingId = "860";

	public const string ProtectiveRingId = "861";

	public const string SoulSapperRingId = "862";

	public const string PhoenixRingId = "863";

	public const string CombinedRingId = "880";

	public const string ImmunityBandId = "887";

	public const string GlowstoneRingId = "888";

	[XmlElement("price")]
	public readonly NetInt price = new NetInt();

	/// <summary>Obsolete. This is only kept to preserve data from old save files, and isn't synchronized in multiplayer. Use <see cref="P:StardewValley.Item.ItemId" /> instead.</summary>
	[XmlElement("indexInTileSheet")]
	public int? obsolete_indexInTileSheet;

	[XmlIgnore]
	public string description;

	[XmlIgnore]
	public string displayName;

	[XmlIgnore]
	public string lightSourceId;

	/// <inheritdoc />
	public override string TypeDefinitionId { get; } = "(O)";

	/// <inheritdoc />
	[XmlIgnore]
	public override string DisplayName
	{
		get
		{
			if (this.displayName == null)
			{
				this.loadDisplayFields();
			}
			return this.displayName;
		}
	}

	/// <inheritdoc />
	protected override void MigrateLegacyItemId()
	{
		base.itemId.Value = this.obsolete_indexInTileSheet?.ToString() ?? base.ParentSheetIndex.ToString();
		this.obsolete_indexInTileSheet = null;
	}

	public Ring()
	{
	}

	public Ring(string itemId)
		: this()
	{
		itemId = base.ValidateUnqualifiedItemId(itemId);
		ObjectData data = Game1.objectData[itemId];
		base.ItemId = itemId;
		base.Category = -96;
		this.Name = data.Name ?? ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).InternalName;
		this.price.Value = data.Price;
		base.ResetParentSheetIndex();
		this.loadDisplayFields();
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.price, "price");
	}

	/// <inheritdoc />
	public override bool CanBeLostOnDeath()
	{
		return false;
	}

	/// <inheritdoc />
	public override void onEquip(Farmer who)
	{
		base.onEquip(who);
		GameLocation location = who.currentLocation;
		location.removeLightSource(this.lightSourceId);
		this.lightSourceId = null;
		switch (base.ItemId)
		{
		case "516":
			this.lightSourceId = this.GenerateLightSourceId(who);
			location.sharedLights.AddLight(new LightSource(this.lightSourceId, 1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 5f, new Color(0, 50, 170), LightSource.LightContext.None, who.UniqueMultiplayerID));
			break;
		case "517":
			this.lightSourceId = this.GenerateLightSourceId(who);
			location.sharedLights.AddLight(new LightSource(this.lightSourceId, 1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 30, 150), LightSource.LightContext.None, who.UniqueMultiplayerID));
			break;
		case "888":
		case "527":
			this.lightSourceId = this.GenerateLightSourceId(who);
			location.sharedLights.AddLight(new LightSource(this.lightSourceId, 1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 80, 0), LightSource.LightContext.None, who.UniqueMultiplayerID));
			break;
		}
	}

	/// <inheritdoc />
	public override void onUnequip(Farmer who)
	{
		base.onUnequip(who);
		switch (base.ItemId)
		{
		case "516":
		case "517":
		case "888":
		case "527":
			who.currentLocation.removeLightSource(this.lightSourceId);
			this.lightSourceId = null;
			break;
		}
	}

	public override void AddEquipmentEffects(BuffEffects effects)
	{
		base.AddEquipmentEffects(effects);
		string text = base.ItemId;
		if (text == null)
		{
			return;
		}
		int length = text.Length;
		if (length != 3)
		{
			return;
		}
		switch (text[2])
		{
		case '8':
			if (!(text == "518"))
			{
				if (text == "888")
				{
					effects.MagneticRadius.Value += 128f;
				}
			}
			else
			{
				effects.MagneticRadius.Value += 64f;
			}
			break;
		case '9':
			switch (text)
			{
			case "519":
				effects.MagneticRadius.Value += 128f;
				break;
			case "529":
				effects.KnockbackMultiplier.Value += 0.1f;
				break;
			case "859":
				effects.LuckLevel.Value += 1f;
				break;
			}
			break;
		case '7':
			if (!(text == "527"))
			{
				if (text == "887")
				{
					effects.Immunity.Value += 4f;
				}
			}
			else
			{
				effects.MagneticRadius.Value += 128f;
				effects.AttackMultiplier.Value += 0.1f;
			}
			break;
		case '0':
			if (!(text == "530"))
			{
				if (text == "810")
				{
					effects.Defense.Value += 5f;
				}
			}
			else
			{
				effects.Defense.Value += 1f;
			}
			break;
		case '1':
			if (text == "531")
			{
				effects.CriticalChanceMultiplier.Value += 0.1f;
			}
			break;
		case '2':
			if (text == "532")
			{
				effects.CriticalPowerMultiplier.Value += 0.1f;
			}
			break;
		case '3':
			if (text == "533")
			{
				effects.WeaponSpeedMultiplier.Value += 0.1f;
			}
			break;
		case '4':
			if (text == "534")
			{
				effects.AttackMultiplier.Value += 0.1f;
			}
			break;
		case '5':
		case '6':
			break;
		}
	}

	/// <inheritdoc />
	public override string getCategoryName()
	{
		return Object.GetCategoryDisplayName(-96);
	}

	public virtual void onNewLocation(Farmer who, GameLocation environment)
	{
		environment.removeLightSource(this.lightSourceId);
		this.lightSourceId = null;
		switch (base.ItemId)
		{
		case "516":
		case "517":
		{
			GameLocation oldLocation = who.currentLocation;
			who.currentLocation = environment;
			this.onEquip(who);
			who.currentLocation = oldLocation;
			break;
		}
		case "888":
		case "527":
			this.lightSourceId = this.GenerateLightSourceId(who);
			environment.sharedLights.AddLight(new LightSource(this.lightSourceId, 1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 30, 150), LightSource.LightContext.None, who.UniqueMultiplayerID));
			break;
		}
	}

	public virtual void onLeaveLocation(Farmer who, GameLocation environment)
	{
		switch (base.ItemId)
		{
		case "516":
		case "517":
		case "527":
		case "888":
			environment.removeLightSource(this.lightSourceId);
			this.lightSourceId = null;
			break;
		}
	}

	/// <inheritdoc />
	public override int salePrice(bool ignoreProfitMargins = false)
	{
		return this.price.Value;
	}

	/// <summary>Apply ring effects when a monster is slain.</summary>
	/// <param name="monster">The monster slain.</param>
	/// <param name="location">The location where the monster was slain.</param>
	/// <param name="who">The player receiving ring effects.</param>
	public virtual void onMonsterSlay(Monster monster, GameLocation location, Farmer who)
	{
		string text = base.ItemId;
		if (!(text == "811"))
		{
			if (text == "860")
			{
				if (Game1.random.NextBool(0.25))
				{
					monster?.objectsToDrop.Add("395");
				}
				else if (Game1.random.NextBool(0.1))
				{
					monster?.objectsToDrop.Add("253");
				}
			}
		}
		else if (monster != null)
		{
			location?.explode(monster.Tile, 2, who, damageFarmers: false, -1, !(location is Farm) && !(location is SlimeHutch));
		}
		if (!who.IsLocalPlayer)
		{
			return;
		}
		switch (base.ItemId)
		{
		case "521":
			if (Game1.random.NextBool(0.1 + (double)((float)who.LuckLevel / 100f)))
			{
				who.applyBuff("20");
				Game1.playSound("warrior");
			}
			break;
		case "522":
			who.health = Math.Min(who.maxHealth, who.health + 2);
			break;
		case "523":
			who.applyBuff("22");
			break;
		case "862":
			who.Stamina = Math.Min(who.MaxStamina, who.Stamina + 4f);
			break;
		}
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		spriteBatch.Draw(itemData.GetTexture(), location + new Vector2(32f, 32f) * scaleSize, itemData.GetSourceRect(), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
		this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
	}

	public virtual void update(GameTime time, GameLocation environment, Farmer who)
	{
		if (this.lightSourceId == null)
		{
			return;
		}
		Vector2 offset = Vector2.Zero;
		if (who.shouldShadowBeOffset)
		{
			offset += who.drawOffset;
		}
		environment.repositionLightSource(this.lightSourceId, new Vector2(who.Position.X + 21f, who.Position.Y) + offset);
		if (!environment.isOutdoors.Value && !(environment is MineShaft) && !(environment is VolcanoDungeon))
		{
			LightSource l = environment.getLightSource(this.lightSourceId);
			if (l != null)
			{
				l.radius.Value = 3f;
			}
		}
	}

	public override int maximumStackSize()
	{
		return 1;
	}

	public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
	{
		Point dimensions = new Point(0, startingHeight);
		int extra_rows_needed = 0;
		if (this.GetsEffectOfRing("810"))
		{
			extra_rows_needed++;
		}
		if (this.GetsEffectOfRing("887") || this.GetsEffectOfRing("530"))
		{
			extra_rows_needed++;
		}
		if (this.GetsEffectOfRing("859"))
		{
			extra_rows_needed++;
		}
		dimensions.X = (int)Math.Max(minWidth, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", 9999)).X + (float)horizontalBuffer);
		dimensions.Y += extra_rows_needed * Math.Max((int)font.MeasureString("TT").Y, 48);
		return dimensions;
	}

	public virtual bool GetsEffectOfRing(string ringId)
	{
		return base.ItemId == ringId;
	}

	public virtual int GetEffectsOfRingMultiplier(string ringId)
	{
		if (this.GetsEffectOfRing(ringId))
		{
			return 1;
		}
		return 0;
	}

	public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
	{
		if (this.description == null)
		{
			this.loadDisplayFields();
		}
		Utility.drawTextWithShadow(spriteBatch, Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth()), font, new Vector2(x + 16, y + 16 + 4), Game1.textColor);
		y += (int)font.MeasureString(Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth())).Y;
		if (this.GetsEffectOfRing("810") || this.GetsEffectOfRing("530"))
		{
			Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", this.GetsEffectOfRing("810") ? (5 * this.GetEffectsOfRingMultiplier("810")) : this.GetEffectsOfRingMultiplier("530")), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
			y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		}
		if (this.GetsEffectOfRing("887"))
		{
			Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(150, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", 4 * this.GetEffectsOfRingMultiplier("887")), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
			y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		}
		if (this.GetsEffectOfRing("859"))
		{
			Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(50, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Utility.drawTextWithShadow(spriteBatch, "+" + Game1.content.LoadString("Strings\\UI:ItemHover_Buff4", this.GetEffectsOfRingMultiplier("859")), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
			y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		}
	}

	public override string getDescription()
	{
		if (this.description == null)
		{
			this.loadDisplayFields();
		}
		return Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth());
	}

	public override bool isPlaceable()
	{
		return false;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new Ring(base.ItemId);
	}

	protected virtual bool loadDisplayFields()
	{
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		this.displayName = itemData.DisplayName;
		this.description = itemData.Description;
		return true;
	}

	public virtual bool CanCombine(Ring ring)
	{
		if (ring is CombinedRing || this is CombinedRing)
		{
			return false;
		}
		if (base.QualifiedItemId == ring.QualifiedItemId)
		{
			return false;
		}
		return true;
	}

	public Ring Combine(Ring ring)
	{
		return new CombinedRing
		{
			combinedRings = 
			{
				base.getOne() as Ring,
				ring.getOne() as Ring
			}
		};
	}
}
