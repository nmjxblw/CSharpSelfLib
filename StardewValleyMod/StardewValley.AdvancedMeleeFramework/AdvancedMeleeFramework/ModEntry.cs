using System;
using System.Collections.Generic;
using AdvancedMeleeFramework.Integrations;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Enchantments;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Projectiles;
using StardewValley.Tools;
using static StardewValley.Farm;
using static StardewValley.Projectiles.BasicProjectile;

namespace AdvancedMeleeFramework;

public class ModEntry : Mod
{
	public static ModEntry Instance;

	public ModConfig Config;

	public Random Random;

	public Dictionary<string, List<AdvancedMeleeWeapon>> AdvancedMeleeWeapons = new Dictionary<string, List<AdvancedMeleeWeapon>>();

	public Dictionary<int, List<AdvancedMeleeWeapon>> AdvancedMeleeWeaponsByType = new Dictionary<int, List<AdvancedMeleeWeapon>>
	{
		{
			1,
			new List<AdvancedMeleeWeapon>()
		},
		{
			2,
			new List<AdvancedMeleeWeapon>()
		},
		{
			3,
			new List<AdvancedMeleeWeapon>()
		}
	};

	public Dictionary<string, AdvancedEnchantmentData> AdvancedEnchantments = new Dictionary<string, AdvancedEnchantmentData>();

	public Dictionary<string, int> EnchantmentTriggers = new Dictionary<string, int>();

	public Dictionary<string, Action<Farmer, MeleeWeapon, Monster?, Dictionary<string, string>>> AdvancedEnchantmentCallbacks = new Dictionary<string, Action<Farmer, MeleeWeapon, Monster, Dictionary<string, string>>>();

	public Dictionary<string, Action<Farmer, MeleeWeapon, Dictionary<string, string>>> SpecialEffectCallbacks = new Dictionary<string, Action<Farmer, MeleeWeapon, Dictionary<string, string>>>();

	public int WeaponAnimationFrame = -1;

	public int WeaponAnimationTicks;

	public int WeaponStartFacingDirection;

	public MeleeWeapon WeaponAnimating;

	public AdvancedMeleeWeapon AdvancedWeaponAnimating;

	public IJsonAssetsApi? JsonAssetsApi;

	public override void Entry(IModHelper helper)
	{
		Instance = this;
		Config = ((Mod)this).Helper.ReadConfig<ModConfig>();
		AMFPatches.Initialize(this);
		Utils.Initialize(this);
		Random = new Random();
		((Mod)this).Helper.Events.Player.InventoryChanged += onInventoryChanged;
		((Mod)this).Helper.Events.GameLoop.GameLaunched += onGameLaunched;
		((Mod)this).Helper.Events.GameLoop.SaveLoaded += onSaveLoaded;
		((Mod)this).Helper.Events.GameLoop.UpdateTicking += onUpdateTicking;
		((Mod)this).Helper.Events.Input.ButtonPressed += onButtonPressed;
		registerDefaultEnchantments();
		registerDefaultSpecialEffects();
	}

	public override object GetApi(IModInfo mod)
	{
		return new AdvancedMeleeFrameworkApi(mod, this);
	}

	private void onInventoryChanged(object sender, InventoryChangedEventArgs e)
	{
		foreach (Item item in e.Player.Items)
		{
			MeleeWeapon mw = (MeleeWeapon)(object)((item is MeleeWeapon) ? item : null);
			if (mw != null)
			{
				Utils.AddEnchantments(mw);
			}
		}
	}

	private void onGameLaunched(object sender, GameLaunchedEventArgs e)
	{
		JsonAssetsApi = ((Mod)this).Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
		IGenericModConfigMenuApi gmcm = ((Mod)this).Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
		if (gmcm != null)
		{
			gmcm.Register(((Mod)this).ModManifest, delegate
			{
				Config = new ModConfig();
			}, delegate
			{
				((Mod)this).Helper.WriteConfig<ModConfig>(Config);
			});
			gmcm.AddBoolOption(((Mod)this).ModManifest, () => Config.EnableMod, delegate(bool v)
			{
				Config.EnableMod = v;
			}, () => "Enabled");
			gmcm.AddKeybind(((Mod)this).ModManifest, () => Config.ReloadButton, delegate(SButton v)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				Config.ReloadButton = v;
			}, () => "Reload Button");
			gmcm.AddBoolOption(((Mod)this).ModManifest, () => Config.RequireModKey, delegate(bool v)
			{
				Config.RequireModKey = v;
			}, () => "Require Activate Button");
			gmcm.AddKeybind(((Mod)this).ModManifest, () => Config.ModKey, delegate(SButton v)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				Config.ModKey = v;
			}, () => "Activate Button");
		}
	}

	private void onSaveLoaded(object sender, SaveLoadedEventArgs e)
	{
		Utils.LoadAdvancedMeleeWeapons();
		foreach (Item item in Game1.player.Items)
		{
			MeleeWeapon mw = (MeleeWeapon)(object)((item is MeleeWeapon) ? item : null);
			if (mw != null)
			{
				Utils.AddEnchantments(mw);
			}
		}
	}

	private void onUpdateTicking(object sender, UpdateTickingEventArgs e)
	{
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Expected O, but got Unknown
		if (WeaponAnimationFrame < 0 || AdvancedWeaponAnimating == null)
		{
			return;
		}
		MeleeActionFrame frame = AdvancedWeaponAnimating.frames[WeaponAnimationFrame];
		Farmer who = ((Tool)WeaponAnimating).getLastFarmerToUse();
		if (WeaponAnimationFrame == 0 && WeaponAnimationTicks == 0)
		{
			WeaponStartFacingDirection = ((Character)who).FacingDirection;
		}
		if ((object)who.CurrentTool != WeaponAnimating)
		{
			WeaponAnimating = null;
			WeaponAnimationTicks = 0;
			WeaponAnimationFrame = -1;
			AdvancedWeaponAnimating = null;
			return;
		}
		bool? invincible = frame.invincible;
		if (invincible.HasValue)
		{
			bool invincible2 = invincible == true;
			who.temporarilyInvincible = invincible2;
		}
		if (WeaponAnimationTicks == 0)
		{
			((Character)who).faceDirection((WeaponStartFacingDirection + frame.relativeFacingDirection) % 4);
			SpecialEffect special = frame.special;
			if (special != null)
			{
				try
				{
					if (!SpecialEffectCallbacks.TryGetValue(special.name, out var callback))
					{
						throw new Exception("No special effect found with name " + special.name);
					}
					callback(who, WeaponAnimating, special.parameters);
				}
				catch (Exception value)
				{
					((Mod)this).Monitor.Log($"Exception thrown on special effect:\n{value}", (LogLevel)4);
				}
			}
			if (frame.action == WeaponAction.NORMAL)
			{
				who.completelyStopAnimatingOrDoingAction();
				who.CanMove = false;
				who.UsingTool = true;
				who.canReleaseTool = true;
				WeaponAnimating.setFarmerAnimating(who);
			}
			else if (frame.action == WeaponAction.SPECIAL)
			{
				WeaponAnimating.animateSpecialMove(who);
			}
			if (frame.trajectoryX != 0f || frame.trajectoryY != 0f)
			{
				Vector2 rawTrajectory = Utils.TranslateVector(new Vector2(frame.trajectoryX, frame.trajectoryY), ((Character)who).FacingDirection);
				((Character)who).setTrajectory(new Vector2(rawTrajectory.X, 0f - rawTrajectory.Y));
			}
			if (frame.sound != null)
			{
				((Character)who).currentLocation.playSound(frame.sound, (Vector2?)null, (int?)null, (SoundContext)0);
			}
			foreach (AdvancedWeaponProjectile p in frame.projectiles)
			{
				Vector2 velocity = Utils.TranslateVector(new Vector2(p.xVelocity, p.yVelocity), ((Character)who).FacingDirection);
				Vector2 startPos = Utils.TranslateVector(new Vector2(p.startingPositionX, p.startingPositionY), ((Character)who).FacingDirection);
				int damage = ((AdvancedWeaponAnimating.type > 0) ? (p.damage * Random.Next(((NetFieldBase<int, NetInt>)(object)WeaponAnimating.minDamage).Value, ((NetFieldBase<int, NetInt>)(object)WeaponAnimating.maxDamage).Value)) : p.damage);
				((Character)who).currentLocation.projectiles.Add((Projectile)new BasicProjectile(damage, p.parentSheetIndex, p.bouncesTillDestruct, p.tailLength, p.rotationVelocity, velocity.X, velocity.Y, ((Character)who).Position + new Vector2(0f, -64f) + startPos, p.collisionSound, p.bounceSound, p.firingSound, p.explode, p.damagesMonsters, ((Character)who).currentLocation, (Character)(object)who, (onCollisionBehavior)null, p.shotItemId));
			}
		}
		if (++WeaponAnimationTicks >= frame.frameTicks)
		{
			WeaponAnimationFrame++;
			WeaponAnimationTicks = 0;
		}
		if (WeaponAnimationFrame < AdvancedWeaponAnimating.frames.Count)
		{
			return;
		}
		who.completelyStopAnimatingOrDoingAction();
		who.CanMove = true;
		who.UsingTool = false;
		((Character)who).setTrajectory(Vector2.Zero);
		if (who.IsLocalPlayer)
		{
			int cd = AdvancedWeaponAnimating.cooldown;
			if (((NetHashSet<int>)(object)who.professions).Contains(28))
			{
				cd /= 2;
			}
			if (((Tool)WeaponAnimating).hasEnchantmentOfType<ArtfulEnchantment>())
			{
				cd /= 2;
			}
			switch (((NetFieldBase<int, NetInt>)(object)WeaponAnimating.type).Value)
			{
			case 1:
				MeleeWeapon.daggerCooldown = cd;
				break;
			case 2:
				MeleeWeapon.clubCooldown = cd;
				break;
			case 3:
				MeleeWeapon.defenseCooldown = cd;
				break;
			}
		}
		WeaponAnimationFrame = -1;
		WeaponAnimating = null;
		AdvancedWeaponAnimating = null;
		WeaponAnimationTicks = 0;
	}

	private void onButtonPressed(object sender, ButtonPressedEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (e.Button == Config.ReloadButton)
		{
			Utils.LoadAdvancedMeleeWeapons();
		}
	}

	private void registerDefaultEnchantments()
	{
		AdvancedEnchantmentCallbacks.Add("heal", Heal);
		AdvancedEnchantmentCallbacks.Add("hurt", Hurt);
		AdvancedEnchantmentCallbacks.Add("coins", Coins);
		AdvancedEnchantmentCallbacks.Add("loot", Loot);
	}

	private void registerDefaultSpecialEffects()
	{
		SpecialEffectCallbacks.Add("lightning", LightningStrike);
		SpecialEffectCallbacks.Add("explosion", Explosion);
	}

	private float defaultMultFromTrigger(string trigger)
	{
		if (!(trigger == "slay"))
		{
			return 1f;
		}
		return 0.1f;
	}

	public void Heal(Farmer who, MeleeWeapon weapon, Monster? monster, Dictionary<string, string> parameters)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		if (Game1.random.NextDouble() < (double)(float.Parse(parameters["chance"]) / 100f))
		{
			string multStr;
			float mult = (parameters.TryGetValue("amountMult", out multStr) ? float.Parse(multStr) : defaultMultFromTrigger(parameters["trigger"]));
			if (!int.TryParse(parameters["amount"], out var amount))
			{
				amount = ((monster == null) ? 1 : monster.MaxHealth);
			}
			int heal = Math.Max(1, (int)((float)amount * mult));
			who.health = Math.Min(who.maxHealth, Game1.player.health + heal);
			((Character)who).currentLocation.debris.Add(new Debris(heal, ((Character)who).getStandingPosition(), Color.Lime, 1f, (Character)(object)who));
			if (parameters.TryGetValue("sound", out var sound))
			{
				Game1.playSound(sound, (int?)null);
			}
		}
	}

	public void Hurt(Farmer who, MeleeWeapon weapon, Monster? monster, Dictionary<string, string> parameters)
	{
		if (Game1.random.NextDouble() < (double)(float.Parse(parameters["chance"]) / 100f))
		{
			string multStr;
			float mult = (parameters.TryGetValue("amountMult", out multStr) ? float.Parse(multStr) : defaultMultFromTrigger(parameters["trigger"]));
			if (!int.TryParse(parameters["amount"], out var amount))
			{
				amount = ((monster == null) ? 1 : monster.MaxHealth);
			}
			int hurt = Math.Max(1, (int)((float)amount * mult));
			who.takeDamage(hurt, true, (Monster)null);
			if (parameters.TryGetValue("sound", out var sound))
			{
				Game1.playSound(sound, (int?)null);
			}
		}
	}

	public void Coins(Farmer who, MeleeWeapon weapon, Monster? monster, Dictionary<string, string> parameters)
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		if (!(Game1.random.NextDouble() < (double)(float.Parse(parameters["chance"]) / 100f)))
		{
			return;
		}
		string multStr;
		float mult = (parameters.TryGetValue("amountMult", out multStr) ? float.Parse(multStr) : defaultMultFromTrigger(parameters["trigger"]));
		if (!int.TryParse(parameters["amount"], out var amount))
		{
			amount = ((monster == null) ? 1 : monster.MaxHealth);
		}
		int coins = (int)Math.Round((float)amount * mult);
		if (parameters.TryGetValue("dropType", out var dropType) && dropType.ToLower() == "wallet")
		{
			who.Money += coins;
			if (parameters.TryGetValue("sound", out var sound2))
			{
				Game1.playSound(sound2, (int?)null);
			}
			return;
		}
		Item i = ItemRegistry.Create("(O)GoldCoin", 1, 0, false);
		((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)i.modData).Add(((Mod)this).ModManifest.UniqueID + "/moneyAmount", coins.ToString());
		Game1.createItemDebris(i, (monster != null) ? ((Character)monster).Position : Utility.PointToVector2(((Character)who).StandingPixel), ((Character)who).FacingDirection, ((Character)who).currentLocation, -1, false);
		if (parameters.TryGetValue("sound", out var sound3))
		{
			Game1.playSound(sound3, (int?)null);
		}
	}

	public void Loot(Farmer who, MeleeWeapon weapon, Monster? monster, Dictionary<string, string> parameters)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		if (monster == null || !(Game1.random.NextDouble() < (double)(float.Parse(parameters["chance"]) / 100f)))
		{
			return;
		}
		Vector2 position = ((Character)monster).Position;
		string extraDrops;
		if (parameters.ContainsKey("extraDropChecks"))
		{
			int extraChecks = Math.Max(1, int.Parse(parameters["extraDropChecks"]));
			for (int i = 0; i < extraChecks; i++)
			{
				GameLocation currentLocation = ((Character)who).currentLocation;
				Rectangle boundingBox = ((Character)monster).GetBoundingBox();
				int x = ((Rectangle)(ref boundingBox)).Center.X;
				boundingBox = ((Character)monster).GetBoundingBox();
				currentLocation.monsterDrop(monster, x, ((Rectangle)(ref boundingBox)).Center.Y, who);
			}
		}
		else if (parameters.TryGetValue("extraDropItems", out extraDrops))
		{
			string[] items = extraDrops.Split(',');
			string[] array = items;
			foreach (string item in array)
			{
				string[] itemData = item.Split('_');
				if (itemData.Length == 1)
				{
					Game1.createItemDebris(ItemRegistry.Create(item, 1, 0, false), position, Game1.random.Next(4), ((Character)who).currentLocation, -1, false);
				}
				else if (itemData.Length == 2)
				{
					float chance = (float)int.Parse(itemData[1]) / 100f;
					if (Game1.random.NextDouble() < (double)chance)
					{
						Game1.createItemDebris(ItemRegistry.Create(itemData[0], 1, 0, false), position, Game1.random.Next(4), ((Character)who).currentLocation, -1, false);
					}
				}
				else if (itemData.Length == 4)
				{
					float chance2 = (float)int.Parse(itemData[3]) / 100f;
					if (Game1.random.NextDouble() < (double)chance2)
					{
						Game1.createItemDebris(ItemRegistry.Create(itemData[0], Game1.random.Next(int.Parse(itemData[1]), int.Parse(itemData[2])), 0, false), position, Game1.random.Next(4), ((Character)who).currentLocation, -1, false);
					}
				}
			}
		}
		if (parameters.TryGetValue("sound", out var sound))
		{
			Game1.playSound(sound, (int?)null);
		}
	}

	public void LightningStrike(Farmer who, MeleeWeapon weapon, Dictionary<string, string> parameters)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		int minDamage = ((NetFieldBase<int, NetInt>)(object)weapon.minDamage).Value;
		int maxDamage = ((NetFieldBase<int, NetInt>)(object)weapon.maxDamage).Value;
		if (parameters.TryGetValue("minDamage", out var minDamageStr))
		{
			minDamage = int.Parse(minDamageStr);
		}
		if (parameters.TryGetValue("maxDamage", out var maxDamageStr))
		{
			maxDamage = int.Parse(maxDamageStr);
		}
		if (parameters.TryGetValue("damageMult", out var damageMultStr) && float.TryParse(damageMultStr, out var damageMult))
		{
			minDamage = (int)Math.Round((float)minDamage * damageMult);
			maxDamage = (int)Math.Round((float)maxDamage * damageMult);
		}
		if (!int.TryParse(parameters["radius"], out var radius))
		{
			radius = 3;
		}
		LightningStrikeEvent lightningEvent = new LightningStrikeEvent
		{
			bigFlash = true,
			createBolt = true
		};
		Vector2 offset = Vector2.Zero;
		if (!parameters.TryGetValue("offsetX", out var offsetX) || !float.TryParse(offsetX, out var x))
		{
			x = 0f;
		}
		if (!parameters.TryGetValue("offsetY", out var offsetY) || !float.TryParse(offsetY, out var y))
		{
			y = 0f;
		}
		if (x != 0f || y != 0f)
		{
			offset = Utils.TranslateVector(new Vector2(x, y), ((Character)who).FacingDirection);
		}
		lightningEvent.boltPosition = ((Character)who).Position + new Vector2(32f, 0f) + offset * 64f;
		Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
		if (parameters.TryGetValue("sound", out var sound))
		{
			Game1.playSound(sound, (int?)null);
		}
		Utility.drawLightningBolt(lightningEvent.boltPosition, ((Character)who).currentLocation);
		((Character)who).currentLocation.damageMonster(new Rectangle((int)Math.Round(lightningEvent.boltPosition.X / 64f - (float)radius) * 64, (int)Math.Round(lightningEvent.boltPosition.Y / 64f - (float)radius) * 64, (radius * 2 + 1) * 64, (radius * 2 + 1) * 64), minDamage, maxDamage, false, who, false);
	}

	public void Explosion(Farmer who, MeleeWeapon weapon, Dictionary<string, string> parameters)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		Vector2 tileLocation = ((Character)who).Tile;
		if (parameters.TryGetValue("offsetX", out var offsetX) && parameters.TryGetValue("offsetY", out var offsetY))
		{
			tileLocation += Utils.TranslateVector(new Vector2(float.Parse(offsetX), float.Parse(offsetY)), ((Character)who).FacingDirection);
		}
		if (!int.TryParse(parameters["radius"], out var radius))
		{
			radius = 3;
		}
		int damage = -1;
		if (parameters.TryGetValue("damageMult", out var damageMultStr) && float.TryParse(damageMultStr, out var damageMult))
		{
			damage = (int)Math.Round((float)Game1.random.Next(((NetFieldBase<int, NetInt>)(object)weapon.minDamage).Value, ((NetFieldBase<int, NetInt>)(object)weapon.maxDamage).Value + 1) * damageMult);
		}
		if (parameters.TryGetValue("minDamage", out var minDamage) && parameters.TryGetValue("maxDamage", out var maxDamage))
		{
			damage = Game1.random.Next(int.Parse(minDamage), int.Parse(maxDamage));
		}
		if (damage < 0)
		{
			damage = Game1.random.Next(((NetFieldBase<int, NetInt>)(object)weapon.minDamage).Value, ((NetFieldBase<int, NetInt>)(object)weapon.maxDamage).Value + 1);
		}
		((Character)who).currentLocation.explode(tileLocation, radius, who, false, damage, true);
	}
}
