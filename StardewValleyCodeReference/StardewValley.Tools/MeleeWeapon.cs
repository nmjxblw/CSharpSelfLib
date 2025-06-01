using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buffs;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData.Weapons;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Projectiles;

namespace StardewValley.Tools;

public class MeleeWeapon : Tool
{
	public const int defenseCooldownTime = 1500;

	public const int daggerCooldownTime = 3000;

	public const int clubCooldownTime = 6000;

	public const int millisecondsPerSpeedPoint = 40;

	public const int defaultSpeed = 400;

	public const int stabbingSword = 0;

	public const int dagger = 1;

	public const int club = 2;

	public const int defenseSword = 3;

	public const int baseClubSpeed = -8;

	public const string scytheId = "47";

	public const string goldenScytheId = "53";

	public const string iridiumScytheID = "66";

	public const string galaxySwordId = "4";

	public const int MAX_FORGES = 3;

	[XmlElement("type")]
	public readonly NetInt type = new NetInt();

	[XmlElement("minDamage")]
	public readonly NetInt minDamage = new NetInt();

	[XmlElement("maxDamage")]
	public readonly NetInt maxDamage = new NetInt();

	[XmlElement("speed")]
	public readonly NetInt speed = new NetInt();

	[XmlElement("addedPrecision")]
	public readonly NetInt addedPrecision = new NetInt();

	[XmlElement("addedDefense")]
	public readonly NetInt addedDefense = new NetInt();

	[XmlElement("addedAreaOfEffect")]
	public readonly NetInt addedAreaOfEffect = new NetInt();

	[XmlElement("knockback")]
	public readonly NetFloat knockback = new NetFloat();

	[XmlElement("critChance")]
	public readonly NetFloat critChance = new NetFloat();

	[XmlElement("critMultiplier")]
	public readonly NetFloat critMultiplier = new NetFloat();

	/// <summary>The qualified item ID for the item whose appearance to use, or <c>null</c> to use the weapon's default appearance.</summary>
	[XmlElement("appearance")]
	public readonly NetString appearance = new NetString(null);

	public bool isOnSpecial;

	public static int defenseCooldown;

	public static int attackSwordCooldown;

	public static int daggerCooldown;

	public static int clubCooldown;

	public static int daggerHitsLeft;

	public static int timedHitTimer;

	private static float addedSwordScale = 0f;

	private static float addedClubScale = 0f;

	private static float addedDaggerScale = 0f;

	private float swipeSpeed;

	[XmlIgnore]
	public Rectangle mostRecentArea;

	[XmlIgnore]
	private readonly NetEvent0 animateSpecialMoveEvent = new NetEvent0();

	[XmlIgnore]
	private readonly NetEvent0 defenseSwordEvent = new NetEvent0();

	[XmlIgnore]
	private readonly NetEvent1Field<int, NetInt> daggerEvent = new NetEvent1Field<int, NetInt>();

	private WeaponData cachedData;

	private bool anotherClick;

	private static Vector2 center = new Vector2(1f, 15f);

	/// <inheritdoc />
	public override string TypeDefinitionId { get; } = "(W)";

	public MeleeWeapon()
	{
		base.Category = -98;
	}

	public MeleeWeapon(string itemId)
		: this()
	{
		itemId = base.ValidateUnqualifiedItemId(itemId);
		base.ItemId = itemId;
		this.Stack = 1;
		this.ReloadData();
	}

	protected void ReloadData()
	{
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		if (MeleeWeapon.TryGetData(base.itemId.Value, out var data))
		{
			this.cachedData = data;
			this.Name = data.Name ?? itemData.InternalName;
			this.minDamage.Value = data.MinDamage;
			this.maxDamage.Value = data.MaxDamage;
			this.knockback.Value = data.Knockback;
			this.speed.Value = data.Speed;
			this.addedPrecision.Value = data.Precision;
			this.addedDefense.Value = data.Defense;
			this.type.Value = data.Type;
			this.addedAreaOfEffect.Value = data.AreaOfEffect;
			this.critChance.Value = data.CritChance;
			this.critMultiplier.Value = data.CritMultiplier;
			if (this.type.Value == 0)
			{
				this.type.Value = 3;
			}
		}
		else
		{
			this.Name = "Error Item";
		}
		base.InitialParentTileIndex = itemData.SpriteIndex;
		base.CurrentParentTileIndex = itemData.SpriteIndex;
		base.IndexOfMenuItemView = itemData.SpriteIndex;
		base.Category = (this.isScythe() ? (-99) : (-98));
	}

	/// <inheritdoc />
	protected override void MigrateLegacyItemId()
	{
		base.ItemId = base.InitialParentTileIndex.ToString();
	}

	/// <summary>Get the weapon's data from <see cref="F:StardewValley.Game1.weaponData" />, if found.</summary>
	public WeaponData GetData()
	{
		if (this.cachedData == null)
		{
			MeleeWeapon.TryGetData(base.ItemId, out this.cachedData);
		}
		return this.cachedData;
	}

	/// <summary>Try to get a weapon's data from <see cref="F:StardewValley.Game1.weaponData" />.</summary>
	/// <param name="itemId">The weapon's unqualified item ID (i.e. the key in <see cref="F:StardewValley.Game1.weaponData" />).</param>
	/// <param name="data">The weapon data, if found.</param>
	/// <returns>Returns whether the crop data was found.</returns>
	public static bool TryGetData(string itemId, out WeaponData data)
	{
		if (itemId == null)
		{
			data = null;
			return false;
		}
		return Game1.weaponData.TryGetValue(itemId, out data);
	}

	/// <inheritdoc />
	public override bool CanBeLostOnDeath()
	{
		if (base.CanBeLostOnDeath())
		{
			return this.GetData()?.CanBeLostOnDeath ?? true;
		}
		return false;
	}

	public override void AddEquipmentEffects(BuffEffects effects)
	{
		base.AddEquipmentEffects(effects);
		effects.Defense.Value += this.addedDefense.Value;
		foreach (BaseEnchantment enchantment in base.enchantments)
		{
			enchantment.AddEquipmentEffects(effects);
		}
	}

	public override int GetMaxForges()
	{
		return 3;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new MeleeWeapon(base.ItemId);
	}

	/// <inheritdoc />
	protected override void GetOneCopyFrom(Item source)
	{
		base.GetOneCopyFrom(source);
		if (source is MeleeWeapon fromWeapon)
		{
			this.appearance.Value = fromWeapon.appearance.Value;
			base.IndexOfMenuItemView = fromWeapon.IndexOfMenuItemView;
		}
	}

	protected override string loadDisplayName()
	{
		return ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).DisplayName;
	}

	protected override string loadDescription()
	{
		return ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).Description;
	}

	/// <inheritdoc />
	public override string getCategoryName()
	{
		if (!this.isScythe())
		{
			int value = this.type.Value;
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14303", this.getItemLevel(), Game1.content.LoadString(value switch
			{
				1 => "Strings\\StringsFromCSFiles:Tool.cs.14304", 
				2 => "Strings\\StringsFromCSFiles:Tool.cs.14305", 
				_ => "Strings\\StringsFromCSFiles:Tool.cs.14306", 
			}));
		}
		return base.getCategoryName();
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.type, "type").AddField(this.minDamage, "minDamage").AddField(this.maxDamage, "maxDamage")
			.AddField(this.speed, "speed")
			.AddField(this.addedPrecision, "addedPrecision")
			.AddField(this.addedDefense, "addedDefense")
			.AddField(this.addedAreaOfEffect, "addedAreaOfEffect")
			.AddField(this.knockback, "knockback")
			.AddField(this.critChance, "critChance")
			.AddField(this.critMultiplier, "critMultiplier")
			.AddField(this.appearance, "appearance")
			.AddField(this.animateSpecialMoveEvent, "animateSpecialMoveEvent")
			.AddField(this.defenseSwordEvent, "defenseSwordEvent")
			.AddField(this.daggerEvent, "daggerEvent");
		this.animateSpecialMoveEvent.onEvent += doAnimateSpecialMove;
		this.defenseSwordEvent.onEvent += doDefenseSwordFunction;
		this.daggerEvent.onEvent += doDaggerFunction;
		base.itemId.fieldChangeVisibleEvent += delegate
		{
			this.ReloadData();
		};
	}

	public override string checkForSpecialItemHoldUpMeessage()
	{
		if (base.QualifiedItemId == "(W)4")
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14122");
		}
		return null;
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
		float coolDownLevel = 0f;
		float addedScale = 0f;
		if (!this.isScythe())
		{
			switch (this.type.Value)
			{
			case 0:
			case 3:
				if (MeleeWeapon.defenseCooldown > 0)
				{
					coolDownLevel = (float)MeleeWeapon.defenseCooldown / 1500f;
				}
				addedScale = MeleeWeapon.addedSwordScale;
				break;
			case 2:
				if (MeleeWeapon.clubCooldown > 0)
				{
					coolDownLevel = (float)MeleeWeapon.clubCooldown / 6000f;
				}
				addedScale = MeleeWeapon.addedClubScale;
				break;
			case 1:
				if (MeleeWeapon.daggerCooldown > 0)
				{
					coolDownLevel = (float)MeleeWeapon.daggerCooldown / 3000f;
				}
				addedScale = MeleeWeapon.addedDaggerScale;
				break;
			}
		}
		bool drawing_as_debris = drawShadow && drawStackNumber == StackDrawType.Hide;
		if (!drawShadow || drawing_as_debris)
		{
			addedScale = 0f;
		}
		ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(this.GetDrawnItemId());
		Texture2D texture = dataOrErrorItem.GetTexture();
		Rectangle sourceRect = dataOrErrorItem.GetSourceRect();
		spriteBatch.Draw(texture, location + ((this.type.Value == 1) ? new Vector2(38f, 25f) : new Vector2(32f, 32f)), sourceRect, color * transparency, 0f, new Vector2(8f, 8f), 4f * (scaleSize + addedScale), SpriteEffects.None, layerDepth);
		if (coolDownLevel > 0f && drawShadow && !drawing_as_debris && !this.isScythe() && (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is ShopMenu) || scaleSize != 1f))
		{
			spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X, (int)location.Y + (64 - (int)(coolDownLevel * 64f)), 64, (int)(coolDownLevel * 64f)), Color.Red * 0.66f);
		}
		this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
	}

	public override int maximumStackSize()
	{
		return 1;
	}

	/// <inheritdoc />
	public override int salePrice(bool ignoreProfitMargins = false)
	{
		if (!MeleeWeapon.IsScythe(base.itemId.Value))
		{
			return this.getItemLevel() * 100;
		}
		return 0;
	}

	public static void weaponsTypeUpdate(GameTime time)
	{
		if (MeleeWeapon.addedSwordScale > 0f)
		{
			MeleeWeapon.addedSwordScale -= 0.01f;
		}
		if (MeleeWeapon.addedClubScale > 0f)
		{
			MeleeWeapon.addedClubScale -= 0.01f;
		}
		if (MeleeWeapon.addedDaggerScale > 0f)
		{
			MeleeWeapon.addedDaggerScale -= 0.01f;
		}
		if ((float)MeleeWeapon.timedHitTimer > 0f)
		{
			MeleeWeapon.timedHitTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		}
		if (MeleeWeapon.defenseCooldown > 0)
		{
			MeleeWeapon.defenseCooldown -= time.ElapsedGameTime.Milliseconds;
			if (MeleeWeapon.defenseCooldown <= 0)
			{
				MeleeWeapon.addedSwordScale = 0.5f;
				Game1.playSound("objectiveComplete");
			}
		}
		if (MeleeWeapon.attackSwordCooldown > 0)
		{
			MeleeWeapon.attackSwordCooldown -= time.ElapsedGameTime.Milliseconds;
			if (MeleeWeapon.attackSwordCooldown <= 0)
			{
				MeleeWeapon.addedSwordScale = 0.5f;
				Game1.playSound("objectiveComplete");
			}
		}
		if (MeleeWeapon.daggerCooldown > 0)
		{
			MeleeWeapon.daggerCooldown -= time.ElapsedGameTime.Milliseconds;
			if (MeleeWeapon.daggerCooldown <= 0)
			{
				MeleeWeapon.addedDaggerScale = 0.5f;
				Game1.playSound("objectiveComplete");
			}
		}
		if (MeleeWeapon.clubCooldown > 0)
		{
			MeleeWeapon.clubCooldown -= time.ElapsedGameTime.Milliseconds;
			if (MeleeWeapon.clubCooldown <= 0)
			{
				MeleeWeapon.addedClubScale = 0.5f;
				Game1.playSound("objectiveComplete");
			}
		}
	}

	public override void tickUpdate(GameTime time, Farmer who)
	{
		base.lastUser = who;
		base.tickUpdate(time, who);
		this.animateSpecialMoveEvent.Poll();
		this.defenseSwordEvent.Poll();
		this.daggerEvent.Poll();
		if (this.isOnSpecial && this.type.Value == 1 && MeleeWeapon.daggerHitsLeft > 0 && !who.UsingTool)
		{
			this.quickStab(who);
			this.triggerDaggerFunction(who, MeleeWeapon.daggerHitsLeft);
		}
		if (this.anotherClick)
		{
			this.leftClick(who);
		}
	}

	public override bool doesShowTileLocationMarker()
	{
		return false;
	}

	public int getNumberOfDescriptionCategories()
	{
		int number = 1;
		if (this.speed.Value != ((this.type.Value == 2) ? (-8) : 0))
		{
			number++;
		}
		if (this.addedDefense.Value > 0)
		{
			number++;
		}
		float effectiveCritChance = this.critChance.Value;
		if (this.type.Value == 1)
		{
			effectiveCritChance += 0.005f;
			effectiveCritChance *= 1.12f;
		}
		if ((double)effectiveCritChance / 0.02 >= 1.100000023841858)
		{
			number++;
		}
		if ((double)(this.critMultiplier.Value - 3f) / 0.02 >= 1.0)
		{
			number++;
		}
		if (this.knockback.Value != this.defaultKnockBackForThisType(this.type.Value))
		{
			number++;
		}
		if (base.enchantments.Count > 0 && base.enchantments[base.enchantments.Count - 1] is DiamondEnchantment)
		{
			number++;
		}
		return number;
	}

	public override void leftClick(Farmer who)
	{
		if (who.health > 0 && Game1.activeClickableMenu == null && Game1.farmEvent == null && !Game1.eventUp && !who.swimming.Value && !who.bathingClothes.Value && !who.onBridge.Value)
		{
			if (!this.isScythe() && who.FarmerSprite.currentAnimationIndex > ((this.type.Value == 2) ? 5 : ((this.type.Value != 1) ? 5 : 0)))
			{
				who.completelyStopAnimatingOrDoingAction();
				who.CanMove = false;
				who.UsingTool = true;
				who.canReleaseTool = true;
				this.setFarmerAnimating(who);
			}
			else if (!this.isScythe() && who.FarmerSprite.currentAnimationIndex > ((this.type.Value == 2) ? 3 : ((this.type.Value != 1) ? 3 : 0)))
			{
				this.anotherClick = true;
			}
		}
	}

	/// <inheritdoc />
	public override bool isScythe()
	{
		return MeleeWeapon.IsScythe(base.QualifiedItemId);
	}

	/// <summary>Get whether an item ID matches a scythe tool.</summary>
	/// <param name="id">The item ID.</param>
	public static bool IsScythe(string id)
	{
		switch (id)
		{
		case "(W)47":
		case "(W)53":
		case "(W)66":
		case "47":
		case "53":
		case "66":
			return true;
		default:
			return false;
		}
	}

	public virtual int getItemLevel()
	{
		float weaponPoints = 0f;
		weaponPoints += (float)(int)((double)((this.maxDamage.Value + this.minDamage.Value) / 2) * (1.0 + 0.03 * (double)(Math.Max(0, this.speed.Value) + ((this.type.Value == 1) ? 15 : 0))));
		weaponPoints += (float)(int)((double)(this.addedPrecision.Value / 2 + this.addedDefense.Value) + ((double)this.critChance.Value - 0.02) * 200.0 + (double)((this.critMultiplier.Value - 3f) * 6f));
		string qualifiedItemId = base.QualifiedItemId;
		if (!(qualifiedItemId == "(W)2"))
		{
			if (qualifiedItemId == "(W)3")
			{
				weaponPoints += 15f;
			}
		}
		else
		{
			weaponPoints += 20f;
		}
		weaponPoints += (float)(this.addedDefense.Value * 2);
		return (int)(weaponPoints / 7f + 1f);
	}

	public static Item attemptAddRandomInnateEnchantment(Item item, Random r, bool force = false, List<BaseEnchantment> enchantsToReroll = null)
	{
		if (r == null)
		{
			r = Game1.random;
		}
		if (item is MeleeWeapon weapon && (force || r.NextBool()))
		{
			while (true)
			{
				int weaponLevel = weapon.getItemLevel();
				if (r.NextDouble() < 0.125 && weaponLevel <= 10)
				{
					weapon.AddEnchantment(new DefenseEnchantment
					{
						Level = Math.Max(1, Math.Min(2, r.Next(weaponLevel + 1) / 2 + 1))
					});
				}
				else if (r.NextDouble() < 0.125)
				{
					weapon.AddEnchantment(new LightweightEnchantment
					{
						Level = r.Next(1, 6)
					});
				}
				else if (r.NextDouble() < 0.125)
				{
					weapon.AddEnchantment(new SlimeGathererEnchantment());
				}
				switch (r.Next(5))
				{
				case 0:
					weapon.AddEnchantment(new AttackEnchantment
					{
						Level = Math.Max(1, Math.Min(5, r.Next(weaponLevel + 1) / 2 + 1))
					});
					break;
				case 1:
					weapon.AddEnchantment(new CritEnchantment
					{
						Level = Math.Max(1, Math.Min(3, r.Next(weaponLevel) / 3))
					});
					break;
				case 2:
					weapon.AddEnchantment(new WeaponSpeedEnchantment
					{
						Level = Math.Max(1, Math.Min(Math.Max(1, 4 - weapon.speed.Value), r.Next(weaponLevel)))
					});
					break;
				case 3:
					weapon.AddEnchantment(new SlimeSlayerEnchantment());
					break;
				case 4:
					weapon.AddEnchantment(new CritPowerEnchantment
					{
						Level = Math.Max(1, Math.Min(3, r.Next(weaponLevel) / 3))
					});
					break;
				}
				if (enchantsToReroll == null)
				{
					break;
				}
				bool foundMatch = false;
				foreach (BaseEnchantment e in enchantsToReroll)
				{
					foreach (BaseEnchantment w_e in weapon.enchantments)
					{
						if (e.GetType().Equals(w_e.GetType()))
						{
							foundMatch = true;
							break;
						}
					}
					if (foundMatch)
					{
						break;
					}
				}
				if (!foundMatch)
				{
					break;
				}
				weapon.enchantments.RemoveWhere((BaseEnchantment enchantment) => enchantment.IsSecondaryEnchantment() && !(enchantment is GalaxySoulEnchantment));
			}
		}
		return item;
	}

	public override string getDescription()
	{
		if (!this.isScythe())
		{
			StringBuilder b = new StringBuilder();
			b.AppendLine(Game1.parseText(base.description, Game1.smallFont, this.getDescriptionWidth()));
			b.AppendLine();
			b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14132", this.minDamage, this.maxDamage));
			if (this.speed.Value != 0)
			{
				b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14134", (this.speed.Value > 0) ? "+" : "-", Math.Abs(this.speed.Value)));
			}
			if (this.addedAreaOfEffect.Value > 0)
			{
				b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14136", this.addedAreaOfEffect));
			}
			if (this.addedPrecision.Value > 0)
			{
				b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14138", this.addedPrecision));
			}
			if (this.addedDefense.Value > 0)
			{
				b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14140", this.addedDefense));
			}
			if ((double)this.critChance.Value / 0.02 >= 2.0)
			{
				b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14142", (int)((double)this.critChance.Value / 0.02)));
			}
			if ((double)(this.critMultiplier.Value - 3f) / 0.02 >= 1.0)
			{
				b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14144", (int)((double)(this.critMultiplier.Value - 3f) / 0.02)));
			}
			if (this.knockback.Value != this.defaultKnockBackForThisType(this.type.Value))
			{
				b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14140", (this.knockback.Value > this.defaultKnockBackForThisType(this.type.Value)) ? "+" : "", (int)Math.Ceiling(Math.Abs(this.knockback.Value - this.defaultKnockBackForThisType(this.type.Value)) * 10f)));
			}
			return b.ToString();
		}
		return Game1.parseText(base.description, Game1.smallFont, this.getDescriptionWidth());
	}

	public virtual float defaultKnockBackForThisType(int type)
	{
		switch (type)
		{
		case 1:
			return 0.5f;
		case 0:
		case 3:
			return 1f;
		case 2:
			return 1.5f;
		default:
			return -1f;
		}
	}

	public virtual Rectangle getAreaOfEffect(int x, int y, int facingDirection, ref Vector2 tileLocation1, ref Vector2 tileLocation2, Rectangle wielderBoundingBox, int indexInCurrentAnimation)
	{
		Rectangle areaOfEffect = Rectangle.Empty;
		int width;
		int height;
		int upHeightOffset;
		int horizontalYOffset;
		if (this.type.Value == 1)
		{
			width = 74;
			height = 48;
			upHeightOffset = 42;
			horizontalYOffset = -32;
		}
		else
		{
			width = 64;
			height = 64;
			horizontalYOffset = -32;
			upHeightOffset = 0;
		}
		if (this.type.Value == 1)
		{
			switch (facingDirection)
			{
			case 0:
				areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Y - height - upHeightOffset, width / 2, height + upHeightOffset);
				tileLocation1 = new Vector2(Game1.random.Choose(areaOfEffect.Left, areaOfEffect.Right) / 64, areaOfEffect.Top / 64);
				tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Top / 64);
				areaOfEffect.Offset(20, -16);
				areaOfEffect.Height += 16;
				areaOfEffect.Width += 20;
				break;
			case 1:
				areaOfEffect = new Rectangle(wielderBoundingBox.Right, y - height / 2 + horizontalYOffset, (int)((float)height * 1.15f), width);
				tileLocation1 = new Vector2(areaOfEffect.Center.X / 64, Game1.random.Choose(areaOfEffect.Top, areaOfEffect.Bottom) / 64);
				tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Center.Y / 64);
				areaOfEffect.Offset(-4, 0);
				areaOfEffect.Width += 16;
				break;
			case 2:
				areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Bottom, width, (int)((float)height * 1.75f));
				tileLocation1 = new Vector2(Game1.random.Choose(areaOfEffect.Left, areaOfEffect.Right) / 64, areaOfEffect.Center.Y / 64);
				tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Center.Y / 64);
				areaOfEffect.Offset(12, -8);
				areaOfEffect.Width -= 21;
				break;
			case 3:
				areaOfEffect = new Rectangle(wielderBoundingBox.Left - (int)((float)height * 1.15f), y - height / 2 + horizontalYOffset, (int)((float)height * 1.15f), width);
				tileLocation1 = new Vector2(areaOfEffect.Left / 64, Game1.random.Choose(areaOfEffect.Top, areaOfEffect.Bottom) / 64);
				tileLocation2 = new Vector2(areaOfEffect.Left / 64, areaOfEffect.Center.Y / 64);
				areaOfEffect.Offset(-12, 0);
				areaOfEffect.Width += 16;
				break;
			}
		}
		else
		{
			switch (facingDirection)
			{
			case 0:
				areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Y - height - upHeightOffset, width, height + upHeightOffset);
				tileLocation1 = new Vector2(Game1.random.Choose(areaOfEffect.Left, areaOfEffect.Right) / 64, areaOfEffect.Top / 64);
				tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Top / 64);
				switch (indexInCurrentAnimation)
				{
				case 5:
					areaOfEffect.Offset(76, -32);
					break;
				case 4:
					areaOfEffect.Offset(56, -32);
					areaOfEffect.Height += 32;
					break;
				case 3:
					areaOfEffect.Offset(40, -60);
					areaOfEffect.Height += 48;
					break;
				case 2:
					areaOfEffect.Offset(-12, -68);
					areaOfEffect.Height += 48;
					break;
				case 1:
					areaOfEffect.Offset(-48, -56);
					areaOfEffect.Height += 32;
					break;
				case 0:
					areaOfEffect.Offset(-60, -12);
					break;
				}
				break;
			case 2:
				areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Bottom, width, (int)((float)height * 1.5f));
				tileLocation1 = new Vector2(Game1.random.Choose(areaOfEffect.Left, areaOfEffect.Right) / 64, areaOfEffect.Center.Y / 64);
				tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Center.Y / 64);
				switch (indexInCurrentAnimation)
				{
				case 0:
					areaOfEffect.Offset(72, -92);
					break;
				case 1:
					areaOfEffect.Offset(56, -32);
					break;
				case 2:
					areaOfEffect.Offset(40, -28);
					break;
				case 3:
					areaOfEffect.Offset(-12, -8);
					break;
				case 4:
					areaOfEffect.Offset(-80, -24);
					areaOfEffect.Width += 32;
					break;
				case 5:
					areaOfEffect.Offset(-68, -44);
					break;
				}
				break;
			case 1:
				areaOfEffect = new Rectangle(wielderBoundingBox.Right, y - height / 2 + horizontalYOffset, height, width);
				tileLocation1 = new Vector2(areaOfEffect.Center.X / 64, Game1.random.Choose(areaOfEffect.Top, areaOfEffect.Bottom) / 64);
				tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Center.Y / 64);
				switch (indexInCurrentAnimation)
				{
				case 0:
					areaOfEffect.Offset(-44, -84);
					break;
				case 1:
					areaOfEffect.Offset(4, -44);
					break;
				case 2:
					areaOfEffect.Offset(12, -4);
					break;
				case 3:
					areaOfEffect.Offset(12, 37);
					break;
				case 4:
					areaOfEffect.Offset(-28, 60);
					break;
				case 5:
					areaOfEffect.Offset(-60, 72);
					break;
				}
				break;
			case 3:
				areaOfEffect = new Rectangle(wielderBoundingBox.Left - height, y - height / 2 + horizontalYOffset, height, width);
				tileLocation1 = new Vector2(areaOfEffect.Left / 64, Game1.random.Choose(areaOfEffect.Top, areaOfEffect.Bottom) / 64);
				tileLocation2 = new Vector2(areaOfEffect.Left / 64, areaOfEffect.Center.Y / 64);
				switch (indexInCurrentAnimation)
				{
				case 0:
					areaOfEffect.Offset(56, -76);
					break;
				case 1:
					areaOfEffect.Offset(-8, -56);
					break;
				case 2:
					areaOfEffect.Offset(-16, -4);
					break;
				case 3:
					areaOfEffect.Offset(0, 37);
					break;
				case 4:
					areaOfEffect.Offset(24, 60);
					break;
				case 5:
					areaOfEffect.Offset(64, 64);
					break;
				}
				break;
			}
		}
		areaOfEffect.Inflate(this.addedAreaOfEffect.Value, this.addedAreaOfEffect.Value);
		return areaOfEffect;
	}

	public void triggerDefenseSwordFunction(Farmer who)
	{
		this.defenseSwordEvent.Fire();
	}

	private void doDefenseSwordFunction()
	{
		this.isOnSpecial = false;
		base.lastUser.UsingTool = false;
		base.lastUser.CanMove = true;
		base.lastUser.FarmerSprite.PauseForSingleAnimation = false;
	}

	public void triggerDaggerFunction(Farmer who, int dagger_hits_left)
	{
		this.daggerEvent.Fire(dagger_hits_left);
	}

	private void doDaggerFunction(int dagger_hits)
	{
		Vector2 v = base.lastUser.getUniformPositionAwayFromBox(base.lastUser.FacingDirection, 48);
		int num = MeleeWeapon.daggerHitsLeft;
		MeleeWeapon.daggerHitsLeft = dagger_hits;
		this.DoDamage(Game1.currentLocation, (int)v.X, (int)v.Y, base.lastUser.FacingDirection, 1, base.lastUser);
		MeleeWeapon.daggerHitsLeft = num;
		if (base.lastUser != null && base.lastUser.IsLocalPlayer)
		{
			MeleeWeapon.daggerHitsLeft--;
		}
		this.isOnSpecial = false;
		base.lastUser.UsingTool = false;
		base.lastUser.CanMove = true;
		base.lastUser.FarmerSprite.PauseForSingleAnimation = false;
		if (MeleeWeapon.daggerHitsLeft > 0 && base.lastUser != null && base.lastUser.IsLocalPlayer)
		{
			this.quickStab(base.lastUser);
		}
	}

	public void triggerClubFunction(Farmer who)
	{
		if (base.PlayUseSounds)
		{
			who.playNearbySoundAll("clubSmash");
		}
		who.currentLocation.damageMonster(new Rectangle((int)who.Position.X - 192, who.GetBoundingBox().Y - 192, 384, 384), this.minDamage.Value, this.maxDamage.Value, isBomb: false, 1.5f, 100, 0f, 1f, triggerMonsterInvincibleTimer: false, who);
		Game1.viewport.Y -= 21;
		Game1.viewport.X += Game1.random.Next(-32, 32);
		Vector2 v = who.getUniformPositionAwayFromBox(who.FacingDirection, 64);
		switch (who.FacingDirection)
		{
		case 0:
		case 2:
			v.X -= 32f;
			v.Y -= 32f;
			break;
		case 1:
			v.X -= 42f;
			v.Y -= 32f;
			break;
		case 3:
			v.Y -= 32f;
			break;
		}
		Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 128, 64, 64), 40f, 4, 0, v, flicker: false, who.FacingDirection == 1));
		who.jitterStrength = 2f;
	}

	private void beginSpecialMove(Farmer who)
	{
		if (!Game1.fadeToBlack)
		{
			this.isOnSpecial = true;
			who.UsingTool = true;
			who.CanMove = false;
		}
	}

	private void quickStab(Farmer who)
	{
		AnimatedSprite.endOfAnimationBehavior endOfAnimFunc = delegate(Farmer f)
		{
			this.triggerDaggerFunction(f, MeleeWeapon.daggerHitsLeft);
		};
		if (!who.IsLocalPlayer)
		{
			endOfAnimFunc = null;
		}
		switch (who.FacingDirection)
		{
		case 0:
			((FarmerSprite)who.Sprite).animateOnce(276, 15f, 2, endOfAnimFunc);
			this.Update(0, 0, who);
			break;
		case 1:
			((FarmerSprite)who.Sprite).animateOnce(274, 15f, 2, endOfAnimFunc);
			this.Update(1, 0, who);
			break;
		case 2:
			((FarmerSprite)who.Sprite).animateOnce(272, 15f, 2, endOfAnimFunc);
			this.Update(2, 0, who);
			break;
		case 3:
			((FarmerSprite)who.Sprite).animateOnce(278, 15f, 2, endOfAnimFunc);
			this.Update(3, 0, who);
			break;
		}
		this.FireProjectile(who);
		this.beginSpecialMove(who);
		if (base.PlayUseSounds)
		{
			who.playNearbySoundLocal("daggerswipe");
		}
	}

	protected virtual int specialCooldown()
	{
		return this.type.Value switch
		{
			3 => MeleeWeapon.defenseCooldown, 
			1 => MeleeWeapon.daggerCooldown, 
			2 => MeleeWeapon.clubCooldown, 
			0 => MeleeWeapon.attackSwordCooldown, 
			_ => 0, 
		};
	}

	public virtual void animateSpecialMove(Farmer who)
	{
		base.lastUser = who;
		if ((this.type.Value != 3 || (!this.Name.Contains("Scythe") && !this.isScythe())) && !Game1.fadeToBlack && this.specialCooldown() <= 0)
		{
			this.animateSpecialMoveEvent.Fire();
		}
	}

	protected virtual void doAnimateSpecialMove()
	{
		if (base.lastUser == null || base.lastUser.CurrentTool != this)
		{
			return;
		}
		if (base.lastUser.isEmoteAnimating)
		{
			base.lastUser.EndEmoteAnimation();
		}
		switch (this.type.Value)
		{
		case 3:
		{
			AnimatedSprite.endOfAnimationBehavior endOfAnimFunc = triggerDefenseSwordFunction;
			if (!base.lastUser.IsLocalPlayer)
			{
				endOfAnimFunc = null;
			}
			switch (base.lastUser.FacingDirection)
			{
			case 0:
				((FarmerSprite)base.lastUser.Sprite).animateOnce(252, 500f, 1, endOfAnimFunc);
				this.Update(0, 0, base.lastUser);
				break;
			case 1:
				((FarmerSprite)base.lastUser.Sprite).animateOnce(243, 500f, 1, endOfAnimFunc);
				this.Update(1, 0, base.lastUser);
				break;
			case 2:
				((FarmerSprite)base.lastUser.Sprite).animateOnce(234, 500f, 1, endOfAnimFunc);
				this.Update(2, 0, base.lastUser);
				break;
			case 3:
				((FarmerSprite)base.lastUser.Sprite).animateOnce(259, 500f, 1, endOfAnimFunc);
				this.Update(3, 0, base.lastUser);
				break;
			}
			if (base.PlayUseSounds)
			{
				base.lastUser.playNearbySoundLocal("batFlap");
			}
			this.beginSpecialMove(base.lastUser);
			if (base.lastUser.IsLocalPlayer)
			{
				MeleeWeapon.defenseCooldown = 1500;
			}
			if (base.lastUser.professions.Contains(28))
			{
				MeleeWeapon.defenseCooldown /= 2;
			}
			if (base.hasEnchantmentOfType<ArtfulEnchantment>())
			{
				MeleeWeapon.defenseCooldown /= 2;
			}
			break;
		}
		case 2:
		{
			AnimatedSprite.endOfAnimationBehavior endOfAnimFunc2 = triggerClubFunction;
			if (!base.lastUser.IsLocalPlayer)
			{
				endOfAnimFunc2 = null;
			}
			if (base.PlayUseSounds)
			{
				base.lastUser.playNearbySoundLocal("clubswipe");
			}
			switch (base.lastUser.FacingDirection)
			{
			case 0:
				((FarmerSprite)base.lastUser.Sprite).animateOnce(176, 40f, 8, endOfAnimFunc2);
				this.Update(0, 0, base.lastUser);
				break;
			case 1:
				((FarmerSprite)base.lastUser.Sprite).animateOnce(168, 40f, 8, endOfAnimFunc2);
				this.Update(1, 0, base.lastUser);
				break;
			case 2:
				((FarmerSprite)base.lastUser.Sprite).animateOnce(160, 40f, 8, endOfAnimFunc2);
				this.Update(2, 0, base.lastUser);
				break;
			case 3:
				((FarmerSprite)base.lastUser.Sprite).animateOnce(184, 40f, 8, endOfAnimFunc2);
				this.Update(3, 0, base.lastUser);
				break;
			}
			this.beginSpecialMove(base.lastUser);
			if (base.lastUser.IsLocalPlayer)
			{
				MeleeWeapon.clubCooldown = 6000;
			}
			if (base.lastUser.professions.Contains(28))
			{
				MeleeWeapon.clubCooldown /= 2;
			}
			if (base.hasEnchantmentOfType<ArtfulEnchantment>())
			{
				MeleeWeapon.clubCooldown /= 2;
			}
			break;
		}
		case 1:
			MeleeWeapon.daggerHitsLeft = 4;
			this.quickStab(base.lastUser);
			if (base.lastUser.IsLocalPlayer)
			{
				MeleeWeapon.daggerCooldown = 3000;
			}
			if (base.lastUser.professions.Contains(28))
			{
				MeleeWeapon.daggerCooldown /= 2;
			}
			if (base.hasEnchantmentOfType<ArtfulEnchantment>())
			{
				MeleeWeapon.daggerCooldown /= 2;
			}
			break;
		}
	}

	public void doSwipe(int type, Vector2 position, int facingDirection, float swipeSpeed, Farmer f)
	{
		if (f == null || f.CurrentTool != this)
		{
			return;
		}
		if (f.IsLocalPlayer)
		{
			f.TemporaryPassableTiles.Clear();
			f.currentLocation.lastTouchActionLocation = Vector2.Zero;
		}
		swipeSpeed *= 1.3f;
		switch (type)
		{
		case 3:
			if (f.CurrentTool == this)
			{
				switch (f.FacingDirection)
				{
				case 0:
					((FarmerSprite)f.Sprite).animateOnce(248, swipeSpeed, 6);
					this.Update(0, 0, f);
					break;
				case 1:
					((FarmerSprite)f.Sprite).animateOnce(240, swipeSpeed, 6);
					this.Update(1, 0, f);
					break;
				case 2:
					((FarmerSprite)f.Sprite).animateOnce(232, swipeSpeed, 6);
					this.Update(2, 0, f);
					break;
				case 3:
					((FarmerSprite)f.Sprite).animateOnce(256, swipeSpeed, 6);
					this.Update(3, 0, f);
					break;
				}
			}
			if (base.PlayUseSounds && f.ShouldHandleAnimationSound())
			{
				f.playNearbySoundLocal("swordswipe");
			}
			break;
		case 2:
			if (f.CurrentTool == this)
			{
				switch (f.FacingDirection)
				{
				case 0:
					((FarmerSprite)f.Sprite).animateOnce(248, swipeSpeed, 8);
					this.Update(0, 0, f);
					break;
				case 1:
					((FarmerSprite)f.Sprite).animateOnce(240, swipeSpeed, 8);
					this.Update(1, 0, f);
					break;
				case 2:
					((FarmerSprite)f.Sprite).animateOnce(232, swipeSpeed, 8);
					this.Update(2, 0, f);
					break;
				case 3:
					((FarmerSprite)f.Sprite).animateOnce(256, swipeSpeed, 8);
					this.Update(3, 0, f);
					break;
				}
			}
			if (base.PlayUseSounds)
			{
				f.playNearbySoundLocal("clubswipe");
			}
			break;
		}
	}

	public virtual void FireProjectile(Farmer who)
	{
		if (this.cachedData?.Projectiles == null)
		{
			return;
		}
		foreach (WeaponProjectile data in this.cachedData.Projectiles)
		{
			float shotAngle = 0f;
			float angleOffsetMultiplier = 1f;
			switch (who.facingDirection.Value)
			{
			case 0:
				shotAngle = 90f;
				break;
			case 1:
				shotAngle = 0f;
				break;
			case 3:
				shotAngle = 180f;
				angleOffsetMultiplier = -1f;
				break;
			case 2:
				shotAngle = 270f;
				break;
			}
			shotAngle += (data.MinAngleOffset + (float)Game1.random.NextDouble() * (data.MaxAngleOffset - data.MinAngleOffset)) * angleOffsetMultiplier;
			shotAngle *= (float)Math.PI / 180f;
			string shotItemId = null;
			if (data.Item != null)
			{
				shotItemId = ItemQueryResolver.TryResolveRandomItem(data.Item, new ItemQueryContext(who.currentLocation, who, null, $"weapon '{base.QualifiedItemId}' > projectile data '{data.Id}'"))?.QualifiedItemId;
				if (shotItemId == null)
				{
					continue;
				}
			}
			Vector2 shotOrigin = who.getStandingPosition() - new Vector2(32f, 32f);
			BasicProjectile projectile = new BasicProjectile(data.Damage, data.SpriteIndex, data.Bounces, data.TailLength, (float)data.RotationVelocity * ((float)Math.PI / 180f), (float)data.Velocity * (float)Math.Cos(shotAngle), (float)data.Velocity * (float)(0.0 - Math.Sin(shotAngle)), shotOrigin, firingSound: data.FireSound, collisionSound: data.CollisionSound, bounceSound: data.BounceSound, explode: data.Explodes, damagesMonsters: true, location: who.currentLocation, firer: who, collisionBehavior: null, shotItemId: shotItemId);
			projectile.ignoreTravelGracePeriod.Value = true;
			projectile.ignoreMeleeAttacks.Value = true;
			projectile.maxTravelDistance.Value = data.MaxDistance * 64;
			projectile.height.Value = 32f;
			who.currentLocation.projectiles.Add(projectile);
		}
	}

	public virtual void setFarmerAnimating(Farmer who)
	{
		this.anotherClick = false;
		who.FarmerSprite.PauseForSingleAnimation = false;
		who.FarmerSprite.StopAnimation();
		this.swipeSpeed = (float)(400 - this.speed.Value * 40) - who.addedSpeed * 40f;
		this.swipeSpeed *= 1f - who.buffs.WeaponSpeedMultiplier;
		if (who.IsLocalPlayer)
		{
			foreach (BaseEnchantment enchantment in base.enchantments)
			{
				if (enchantment is BaseWeaponEnchantment weaponEnchantment)
				{
					weaponEnchantment.OnSwing(this, who);
				}
			}
			this.FireProjectile(who);
		}
		if (this.type.Value != 1)
		{
			this.doSwipe(this.type.Value, who.Position, who.FacingDirection, this.swipeSpeed / (float)((this.type.Value == 2) ? 5 : 8), who);
			who.lastClick = Vector2.Zero;
			Vector2 actionTile = who.GetToolLocation(ignoreClick: true);
			this.DoDamage(who.currentLocation, (int)actionTile.X, (int)actionTile.Y, who.FacingDirection, 1, who);
		}
		else
		{
			if (base.PlayUseSounds && who.IsLocalPlayer)
			{
				who.playNearbySoundAll("daggerswipe");
			}
			this.swipeSpeed /= 4f;
			switch (who.FacingDirection)
			{
			case 0:
				((FarmerSprite)who.Sprite).animateOnce(276, this.swipeSpeed, 2);
				this.Update(0, 0, who);
				break;
			case 1:
				((FarmerSprite)who.Sprite).animateOnce(274, this.swipeSpeed, 2);
				this.Update(1, 0, who);
				break;
			case 2:
				((FarmerSprite)who.Sprite).animateOnce(272, this.swipeSpeed, 2);
				this.Update(2, 0, who);
				break;
			case 3:
				((FarmerSprite)who.Sprite).animateOnce(278, this.swipeSpeed, 2);
				this.Update(3, 0, who);
				break;
			}
			Vector2 actionTile2 = who.GetToolLocation(ignoreClick: true);
			this.DoDamage(who.currentLocation, (int)actionTile2.X, (int)actionTile2.Y, who.FacingDirection, 1, who);
		}
		if (who.CurrentTool == null)
		{
			who.completelyStopAnimatingOrDoingAction();
			who.forceCanMove();
		}
	}

	public override void actionWhenStopBeingHeld(Farmer who)
	{
		who.UsingTool = false;
		this.anotherClick = false;
		base.actionWhenStopBeingHeld(who);
	}

	public virtual void RecalculateAppliedForges(bool force = false)
	{
		if (base.enchantments.Count == 0 && !force)
		{
			return;
		}
		foreach (BaseEnchantment enchantment in base.enchantments)
		{
			if (enchantment.IsForge())
			{
				enchantment.UnapplyTo(this);
			}
		}
		WeaponData data = this.GetData();
		if (data != null)
		{
			this.Name = data.Name;
			this.minDamage.Value = data.MinDamage;
			this.maxDamage.Value = data.MaxDamage;
			this.knockback.Value = data.Knockback;
			this.speed.Value = data.Speed;
			this.addedPrecision.Value = data.Precision;
			this.addedDefense.Value = data.Defense;
			this.type.Value = data.Type;
			this.addedAreaOfEffect.Value = data.AreaOfEffect;
			this.critChance.Value = data.CritChance;
			this.critMultiplier.Value = data.CritMultiplier;
			if (this.type.Value == 0)
			{
				this.type.Value = 3;
			}
		}
		foreach (BaseEnchantment enchantment2 in base.enchantments)
		{
			if (enchantment2.IsForge())
			{
				enchantment2.ApplyTo(this);
			}
		}
	}

	public virtual void DoDamage(GameLocation location, int x, int y, int facingDirection, int power, Farmer who)
	{
		if (!who.IsLocalPlayer)
		{
			return;
		}
		this.isOnSpecial = false;
		if (this.type.Value != 2)
		{
			base.DoFunction(location, x, y, power, who);
		}
		base.lastUser = who;
		Vector2 tileLocation1 = Vector2.Zero;
		Vector2 tileLocation2 = Vector2.Zero;
		Rectangle areaOfEffect = this.getAreaOfEffect(x, y, facingDirection, ref tileLocation1, ref tileLocation2, who.GetBoundingBox(), who.FarmerSprite.currentAnimationIndex);
		this.mostRecentArea = areaOfEffect;
		float effectiveCritChance = this.critChance.Value;
		if (this.type.Value == 1)
		{
			effectiveCritChance += 0.005f;
			effectiveCritChance *= 1.12f;
		}
		if (location.damageMonster(areaOfEffect, (int)((float)this.minDamage.Value * (1f + who.buffs.AttackMultiplier)), (int)((float)this.maxDamage.Value * (1f + who.buffs.AttackMultiplier)), isBomb: false, this.knockback.Value * (1f + who.buffs.KnockbackMultiplier), (int)((float)this.addedPrecision.Value * (1f + who.buffs.WeaponPrecisionMultiplier)), effectiveCritChance * (1f + who.buffs.CriticalChanceMultiplier), this.critMultiplier.Value * (1f + who.buffs.CriticalPowerMultiplier), this.type.Value != 1 || !this.isOnSpecial, who) && this.type.Value == 2 && base.PlayUseSounds)
		{
			who.playNearbySoundAll("clubhit");
		}
		string soundToPlay = "";
		location.projectiles.RemoveWhere(delegate(Projectile projectile)
		{
			if (areaOfEffect.Intersects(projectile.getBoundingBox()) && !projectile.ignoreMeleeAttacks.Value)
			{
				projectile.behaviorOnCollisionWithOther(location);
			}
			return projectile.destroyMe;
		});
		foreach (Vector2 v in Utility.removeDuplicates(Utility.getListOfTileLocationsForBordersOfNonTileRectangle(areaOfEffect)))
		{
			if (location.terrainFeatures.TryGetValue(v, out var terrainFeature) && terrainFeature.performToolAction(this, 0, v))
			{
				location.terrainFeatures.Remove(v);
			}
			if (location.objects.TryGetValue(v, out var obj) && obj.performToolAction(this))
			{
				location.objects.Remove(v);
			}
			if (location.performToolAction(this, (int)v.X, (int)v.Y))
			{
				break;
			}
		}
		if (base.PlayUseSounds && !soundToPlay.Equals(""))
		{
			Game1.playSound(soundToPlay);
		}
		base.CurrentParentTileIndex = base.IndexOfMenuItemView;
		if (who != null && who.isRidingHorse())
		{
			who.completelyStopAnimatingOrDoingAction();
		}
	}

	/// <summary>Get the qualified item ID to draw for this weapon.</summary>
	public string GetDrawnItemId()
	{
		return this.appearance.Value ?? base.QualifiedItemId;
	}

	public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
	{
		Utility.drawTextWithShadow(spriteBatch, Game1.parseText(base.description, Game1.smallFont, this.getDescriptionWidth()), font, new Vector2(x + 16, y + 16 + 4), Game1.textColor);
		y += (int)font.MeasureString(Game1.parseText(base.description, Game1.smallFont, this.getDescriptionWidth())).Y;
		if (this.isScythe())
		{
			return;
		}
		Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(120, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
		Color co = Game1.textColor;
		if (base.hasEnchantmentOfType<RubyEnchantment>())
		{
			co = new Color(0, 120, 120);
		}
		Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_Damage", this.minDamage, this.maxDamage), font, new Vector2(x + 16 + 52, y + 16 + 12), co * 0.9f * alpha);
		y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		if (this.speed.Value != ((this.type.Value == 2) ? (-8) : 0))
		{
			Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(130, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			bool negativeSpeed = (this.type.Value == 2 && this.speed.Value < -8) || (this.type.Value != 2 && this.speed.Value < 0);
			Color c = Game1.textColor;
			if (base.hasEnchantmentOfType<EmeraldEnchantment>())
			{
				c = new Color(0, 120, 120);
			}
			Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_Speed", ((((this.type.Value == 2) ? (this.speed.Value - -8) : this.speed.Value) > 0) ? "+" : "") + ((this.type.Value == 2) ? (this.speed.Value - -8) : this.speed.Value) / 2), font, new Vector2(x + 16 + 52, y + 16 + 12), negativeSpeed ? Color.DarkRed : (c * 0.9f * alpha));
			y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		}
		if (this.addedDefense.Value > 0)
		{
			Color c2 = Game1.textColor;
			if (base.hasEnchantmentOfType<TopazEnchantment>())
			{
				c2 = new Color(0, 120, 120);
			}
			Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", this.addedDefense), font, new Vector2(x + 16 + 52, y + 16 + 12), c2 * 0.9f * alpha);
			y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		}
		float effectiveCritChance = this.critChance.Value;
		if (this.type.Value == 1)
		{
			effectiveCritChance += 0.005f;
			effectiveCritChance *= 1.12f;
		}
		if ((double)effectiveCritChance / 0.02 >= 1.100000023841858)
		{
			Color c3 = Game1.textColor;
			if (base.hasEnchantmentOfType<AquamarineEnchantment>())
			{
				c3 = new Color(0, 120, 120);
			}
			Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(40, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_CritChanceBonus", (int)Math.Round((double)(effectiveCritChance - 0.001f) / 0.02)), font, new Vector2(x + 16 + 52, y + 16 + 12), c3 * 0.9f * alpha);
			y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		}
		if ((double)(this.critMultiplier.Value - 3f) / 0.02 >= 1.0)
		{
			Color c4 = Game1.textColor;
			if (base.hasEnchantmentOfType<JadeEnchantment>())
			{
				c4 = new Color(0, 120, 120);
			}
			Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16, y + 16 + 4), new Rectangle(160, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_CritPowerBonus", (int)((double)(this.critMultiplier.Value - 3f) / 0.02)), font, new Vector2(x + 16 + 44, y + 16 + 12), c4 * 0.9f * alpha);
			y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		}
		if (this.knockback.Value != this.defaultKnockBackForThisType(this.type.Value))
		{
			Color c5 = Game1.textColor;
			if (base.hasEnchantmentOfType<AmethystEnchantment>())
			{
				c5 = new Color(0, 120, 120);
			}
			Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(70, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_Weight", (((float)(int)Math.Ceiling(Math.Abs(this.knockback.Value - this.defaultKnockBackForThisType(this.type.Value)) * 10f) > this.defaultKnockBackForThisType(this.type.Value)) ? "+" : "") + (int)Math.Ceiling(Math.Abs(this.knockback.Value - this.defaultKnockBackForThisType(this.type.Value)) * 10f)), font, new Vector2(x + 16 + 52, y + 16 + 12), c5 * 0.9f * alpha);
			y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		}
		if (base.enchantments.Count > 0 && base.enchantments[base.enchantments.Count - 1] is DiamondEnchantment)
		{
			Color c6 = new Color(0, 120, 120);
			int random_forges = this.GetMaxForges() - base.GetTotalForgeLevels();
			string random_forge_string = ((random_forges == 1) ? Game1.content.LoadString("Strings\\UI:ItemHover_DiamondForge_Singular", random_forges) : Game1.content.LoadString("Strings\\UI:ItemHover_DiamondForge_Plural", random_forges));
			Utility.drawTextWithShadow(spriteBatch, random_forge_string, font, new Vector2(x + 16, y + 16 + 12), c6 * 0.9f * alpha);
			y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		}
		foreach (BaseEnchantment enchantment in base.enchantments)
		{
			if (enchantment.ShouldBeDisplayed())
			{
				Color c7 = new Color(120, 0, 210);
				if (enchantment.IsSecondaryEnchantment())
				{
					Utility.drawWithShadow(spriteBatch, Game1.mouseCursors_1_6, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(502, 430, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
					c7 = new Color(120, 50, 100);
				}
				else
				{
					Utility.drawWithShadow(spriteBatch, Game1.mouseCursors2, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(127, 35, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
				}
				Utility.drawTextWithShadow(spriteBatch, ((BaseEnchantment.hideEnchantmentName && !enchantment.IsSecondaryEnchantment()) || (BaseEnchantment.hideSecondaryEnchantName && enchantment.IsSecondaryEnchantment())) ? "???" : enchantment.GetDisplayName(), font, new Vector2(x + 16 + 52, y + 16 + 12), c7 * 0.9f * alpha);
				y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
			}
		}
	}

	public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
	{
		int maxStat = 9999;
		Point dimensions = new Point(0, 0);
		dimensions.Y += Math.Max(60, (int)((boldTitleText != null) ? (Game1.dialogueFont.MeasureString(boldTitleText).Y + 16f) : 0f) + 32) + (int)font.MeasureString("T").Y + (int)((moneyAmountToDisplayAtBottom > -1) ? (font.MeasureString(moneyAmountToDisplayAtBottom.ToString() ?? "").Y + 4f) : 0f);
		dimensions.Y += ((!this.isScythe()) ? (this.getNumberOfDescriptionCategories() * 4 * 12) : 0);
		dimensions.Y += (int)font.MeasureString(Game1.parseText(base.description, Game1.smallFont, this.getDescriptionWidth())).Y;
		dimensions.X = (int)Math.Max(minWidth, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Damage", maxStat, maxStat)).X + (float)horizontalBuffer, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Speed", maxStat)).X + (float)horizontalBuffer, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", maxStat)).X + (float)horizontalBuffer, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_CritChanceBonus", maxStat)).X + (float)horizontalBuffer, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_CritPowerBonus", maxStat)).X + (float)horizontalBuffer, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Weight", maxStat)).X + (float)horizontalBuffer))))));
		if (base.enchantments.Count > 0 && base.enchantments[base.enchantments.Count - 1] is DiamondEnchantment)
		{
			dimensions.X = (int)Math.Max(dimensions.X, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DiamondForge_Plural", this.GetMaxForges())).X);
		}
		foreach (BaseEnchantment enchantment in base.enchantments)
		{
			if (enchantment.ShouldBeDisplayed())
			{
				dimensions.Y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
			}
		}
		return dimensions;
	}

	public virtual void ResetIndexOfMenuItemView()
	{
		base.IndexOfMenuItemView = base.InitialParentTileIndex;
	}

	public virtual void drawDuringUse(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f)
	{
		MeleeWeapon.drawDuringUse(frameOfFarmerAnimation, facingDirection, spriteBatch, playerPosition, f, this.GetDrawnItemId(), this.type.Value, this.isOnSpecial);
	}

	public override bool CanForge(Item item)
	{
		if (item is MeleeWeapon other_weapon && other_weapon.type.Value == this.type.Value)
		{
			return true;
		}
		return base.CanForge(item);
	}

	public override bool CanAddEnchantment(BaseEnchantment enchantment)
	{
		if (enchantment is GalaxySoulEnchantment && !this.isGalaxyWeapon())
		{
			return false;
		}
		return base.CanAddEnchantment(enchantment);
	}

	public bool isGalaxyWeapon()
	{
		if (!(base.QualifiedItemId == "(W)4") && !(base.QualifiedItemId == "(W)23"))
		{
			return base.QualifiedItemId == "(W)29";
		}
		return true;
	}

	/// <summary>Convert this weapon to a new item ID. This reloads the weapon data but keeps any previous enchantments, mod data, etc.</summary>
	/// <param name="newItemId">The new unqualified item ID.</param>
	public void transform(string newItemId)
	{
		base.ItemId = newItemId;
		this.appearance.Value = null;
		this.RecalculateAppliedForges(force: true);
	}

	public override bool Forge(Item item, bool count_towards_stats = false)
	{
		if (this.isScythe())
		{
			return false;
		}
		if (item is MeleeWeapon other_weapon && other_weapon.type.Value == this.type.Value)
		{
			this.appearance.Value = other_weapon.QualifiedItemId;
			return true;
		}
		return base.Forge(item, count_towards_stats);
	}

	public static void drawDuringUse(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f, string weaponItemId, int type, bool isOnSpecial)
	{
		ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(weaponItemId);
		Texture2D texture = dataOrErrorItem.GetTexture() ?? Tool.weaponsTexture;
		Rectangle sourceRect = dataOrErrorItem.GetSourceRect();
		float baseSortLayer = f.getDrawLayer();
		FarmerRenderer.FarmerSpriteLayers weaponSortLayer = f.FacingDirection switch
		{
			0 => FarmerRenderer.FarmerSpriteLayers.ToolUp, 
			2 => FarmerRenderer.FarmerSpriteLayers.ToolDown, 
			_ => FarmerRenderer.FarmerSpriteLayers.TOOL_IN_USE_SIDE, 
		};
		float sortBehindLayer = FarmerRenderer.GetLayerDepth(baseSortLayer, FarmerRenderer.FarmerSpriteLayers.ToolUp);
		float sortLayer = FarmerRenderer.GetLayerDepth(baseSortLayer, weaponSortLayer);
		if (type != 1)
		{
			if (isOnSpecial)
			{
				switch (type)
				{
				case 3:
					switch (f.FacingDirection)
					{
					case 0:
						spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 44f), sourceRect, Color.White, (float)Math.PI * -9f / 16f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
						break;
					case 1:
						spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 4f), sourceRect, Color.White, (float)Math.PI * -3f / 16f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
						break;
					case 2:
						spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 52f, playerPosition.Y + 4f), sourceRect, Color.White, -5.105088f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
						break;
					case 3:
						spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 56f, playerPosition.Y - 4f), sourceRect, Color.White, (float)Math.PI * 3f / 16f, new Vector2(15f, 15f), 4f, SpriteEffects.FlipHorizontally, sortLayer);
						break;
					}
					break;
				case 2:
					switch (facingDirection)
					{
					case 1:
						switch (frameOfFarmerAnimation)
						{
						case 0:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X - 32f - 12f, playerPosition.Y - 80f), sourceRect, Color.White, (float)Math.PI * -3f / 8f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 1:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f, playerPosition.Y - 64f - 48f), sourceRect, Color.White, (float)Math.PI / 8f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 2:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 128f - 16f, playerPosition.Y - 64f - 12f), sourceRect, Color.White, (float)Math.PI * 3f / 8f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 3:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 72f, playerPosition.Y - 64f + 16f - 32f), sourceRect, Color.White, (float)Math.PI / 8f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 4:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 96f, playerPosition.Y - 64f + 16f - 16f), sourceRect, Color.White, (float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 5:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 96f - 12f, playerPosition.Y - 64f + 16f), sourceRect, Color.White, (float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 6:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 96f - 16f, playerPosition.Y - 64f + 40f - 8f), sourceRect, Color.White, (float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 7:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 96f - 8f, playerPosition.Y + 40f), sourceRect, Color.White, (float)Math.PI * 5f / 16f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						}
						break;
					case 3:
						switch (frameOfFarmerAnimation)
						{
						case 0:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 4f + 8f, playerPosition.Y - 56f - 64f), sourceRect, Color.White, (float)Math.PI / 8f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 1:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X - 32f, playerPosition.Y - 32f), sourceRect, Color.White, (float)Math.PI * -5f / 8f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 2:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X - 12f, playerPosition.Y + 8f), sourceRect, Color.White, (float)Math.PI * -7f / 8f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 3:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X - 32f - 4f, playerPosition.Y + 8f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 4:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f - 24f, playerPosition.Y + 64f + 12f - 64f), sourceRect, Color.White, 4.31969f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 5:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X - 20f, playerPosition.Y + 64f + 40f - 64f), sourceRect, Color.White, 3.926991f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 6:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f, playerPosition.Y + 64f + 56f), sourceRect, Color.White, 3.926991f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 7:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X - 8f, playerPosition.Y + 64f + 64f), sourceRect, Color.White, 3.7306414f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						}
						break;
					default:
						switch (frameOfFarmerAnimation)
						{
						case 0:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X - 24f, playerPosition.Y - 21f - 8f - 64f), sourceRect, Color.White, -(float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 1:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 21f - 64f + 4f), sourceRect, Color.White, -(float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 2:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 21f + 20f - 64f), sourceRect, Color.White, -(float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							break;
						case 3:
							if (facingDirection == 2)
							{
								spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f + 8f, playerPosition.Y + 32f), sourceRect, Color.White, -3.926991f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							}
							else
							{
								spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 21f + 32f - 64f), sourceRect, Color.White, -(float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							}
							break;
						case 4:
							if (facingDirection == 2)
							{
								spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f + 8f, playerPosition.Y + 32f), sourceRect, Color.White, -3.926991f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							}
							break;
						case 5:
							if (facingDirection == 2)
							{
								spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f + 12f, playerPosition.Y + 64f - 20f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							}
							break;
						case 6:
							if (facingDirection == 2)
							{
								spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f + 12f, playerPosition.Y + 64f + 54f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							}
							break;
						case 7:
							if (facingDirection == 2)
							{
								spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f + 12f, playerPosition.Y + 64f + 58f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							}
							break;
						}
						if (f.FacingDirection == 0)
						{
							f.FarmerRenderer.draw(spriteBatch, f.FarmerSprite, f.FarmerSprite.SourceRect, f.getLocalPosition(Game1.viewport), new Vector2(0f, (f.yOffset + 128f - (float)(f.GetBoundingBox().Height / 2)) / 4f + 4f), sortLayer, Color.White, 0f, f);
						}
						break;
					}
					break;
				}
				return;
			}
			switch (facingDirection)
			{
			case 1:
				switch (frameOfFarmerAnimation)
				{
				case 0:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 40f, playerPosition.Y - 64f + 8f), sourceRect, Color.White, -(float)Math.PI / 4f, MeleeWeapon.center, 4f, SpriteEffects.None, sortBehindLayer);
					break;
				case 1:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 56f, playerPosition.Y - 64f + 28f), sourceRect, Color.White, 0f, MeleeWeapon.center, 4f, SpriteEffects.None, sortBehindLayer);
					break;
				case 2:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 4f, playerPosition.Y - 16f), sourceRect, Color.White, (float)Math.PI / 4f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 3:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 4f, playerPosition.Y - 4f), sourceRect, Color.White, (float)Math.PI / 2f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 4:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 28f, playerPosition.Y + 4f), sourceRect, Color.White, (float)Math.PI * 5f / 8f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 5:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 48f, playerPosition.Y + 4f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 6:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 48f, playerPosition.Y + 4f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 7:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y + 64f + 12f), sourceRect, Color.White, 1.9634954f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				}
				break;
			case 3:
				switch (frameOfFarmerAnimation)
				{
				case 0:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 64f - 16f), sourceRect, Color.White, (float)Math.PI / 4f, MeleeWeapon.center, 4f, SpriteEffects.FlipHorizontally, sortBehindLayer);
					break;
				case 1:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X - 48f, playerPosition.Y - 64f + 20f), sourceRect, Color.White, 0f, MeleeWeapon.center, 4f, SpriteEffects.FlipHorizontally, sortBehindLayer);
					break;
				case 2:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X - 64f + 32f, playerPosition.Y + 16f), sourceRect, Color.White, -(float)Math.PI / 4f, MeleeWeapon.center, 4f, SpriteEffects.FlipHorizontally, sortLayer);
					break;
				case 3:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 4f, playerPosition.Y + 44f), sourceRect, Color.White, -(float)Math.PI / 2f, MeleeWeapon.center, 4f, SpriteEffects.FlipHorizontally, sortLayer);
					break;
				case 4:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 44f, playerPosition.Y + 52f), sourceRect, Color.White, (float)Math.PI * -5f / 8f, MeleeWeapon.center, 4f, SpriteEffects.FlipHorizontally, sortLayer);
					break;
				case 5:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 80f, playerPosition.Y + 40f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, MeleeWeapon.center, 4f, SpriteEffects.FlipHorizontally, sortLayer);
					break;
				case 6:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 80f, playerPosition.Y + 40f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, MeleeWeapon.center, 4f, SpriteEffects.FlipHorizontally, sortLayer);
					break;
				case 7:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X - 44f, playerPosition.Y + 96f), sourceRect, Color.White, -5.105088f, MeleeWeapon.center, 4f, SpriteEffects.FlipVertically, sortLayer);
					break;
				}
				break;
			case 0:
				switch (frameOfFarmerAnimation)
				{
				case 0:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 32f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 1:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 48f), sourceRect, Color.White, -(float)Math.PI / 2f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 2:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), sourceRect, Color.White, (float)Math.PI * -3f / 8f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 3:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), sourceRect, Color.White, -(float)Math.PI / 8f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 4:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 40f), sourceRect, Color.White, 0f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 5:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f, playerPosition.Y - 40f), sourceRect, Color.White, (float)Math.PI / 8f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 6:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f, playerPosition.Y - 40f), sourceRect, Color.White, (float)Math.PI / 8f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 7:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 44f, playerPosition.Y + 64f), sourceRect, Color.White, -1.9634954f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				}
				break;
			case 2:
				switch (frameOfFarmerAnimation)
				{
				case 0:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 56f, playerPosition.Y - 16f), sourceRect, Color.White, (float)Math.PI / 8f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 1:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 52f, playerPosition.Y - 8f), sourceRect, Color.White, (float)Math.PI / 2f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 2:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 40f, playerPosition.Y), sourceRect, Color.White, (float)Math.PI / 2f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 3:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 16f, playerPosition.Y + 4f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 4:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 8f, playerPosition.Y + 8f), sourceRect, Color.White, (float)Math.PI, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 5:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 12f, playerPosition.Y), sourceRect, Color.White, 3.5342917f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 6:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 12f, playerPosition.Y), sourceRect, Color.White, 3.5342917f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 7:
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 44f, playerPosition.Y + 64f), sourceRect, Color.White, -5.105088f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				}
				break;
			}
			return;
		}
		frameOfFarmerAnimation %= 2;
		switch (facingDirection)
		{
		case 1:
			switch (frameOfFarmerAnimation)
			{
			case 0:
				spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y - 16f), sourceRect, Color.White, (float)Math.PI / 4f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
				break;
			case 1:
				spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 24f), sourceRect, Color.White, (float)Math.PI / 4f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
				break;
			}
			break;
		case 3:
			switch (frameOfFarmerAnimation)
			{
			case 0:
				spriteBatch.Draw(texture, new Vector2(playerPosition.X + 16f, playerPosition.Y - 16f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
				break;
			case 1:
				spriteBatch.Draw(texture, new Vector2(playerPosition.X + 8f, playerPosition.Y - 24f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
				break;
			}
			break;
		case 0:
			switch (frameOfFarmerAnimation)
			{
			case 0:
				spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 4f, playerPosition.Y - 40f), sourceRect, Color.White, -(float)Math.PI / 4f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
				break;
			case 1:
				spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y - 48f), sourceRect, Color.White, -(float)Math.PI / 4f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
				break;
			}
			break;
		case 2:
			switch (frameOfFarmerAnimation)
			{
			case 0:
				spriteBatch.Draw(texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 8f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
				break;
			case 1:
				spriteBatch.Draw(texture, new Vector2(playerPosition.X + 21f, playerPosition.Y + 20f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
				break;
			}
			break;
		}
	}
}
