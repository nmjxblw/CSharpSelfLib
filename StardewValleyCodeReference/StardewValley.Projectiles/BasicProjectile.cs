using System;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;

namespace StardewValley.Projectiles;

public class BasicProjectile : Projectile
{
	public delegate void onCollisionBehavior(GameLocation location, int xPosition, int yPosition, Character who);

	/// <summary>The amount of damage caused when this projectile hits a monster or player.</summary>
	public readonly NetInt damageToFarmer = new NetInt();

	/// <summary>The sound played when the projectile collides with something.</summary>
	public readonly NetString collisionSound = new NetString();

	/// <summary>Whether the projectile explodes when it collides with something.</summary>
	public readonly NetBool explode = new NetBool();

	/// <summary>A callback to invoke after the projectile collides with a player, monster, or wall.</summary>
	public onCollisionBehavior collisionBehavior;

	/// <summary>The buff ID to apply to players hit by this projectile, if any.</summary>
	public NetString debuff = new NetString(null);

	/// <summary>The sound to play when <see cref="F:StardewValley.Projectiles.BasicProjectile.debuff" /> is applied to a player.</summary>
	public NetString debuffSound = new NetString("debuffHit");

	/// <summary>Construct an empty instance.</summary>
	public BasicProjectile()
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="damageToFarmer">The amount of damage caused when this projectile hits a monster or player.</param>
	/// <param name="spriteIndex">The index of the sprite to draw in <see cref="F:StardewValley.Projectiles.Projectile.projectileSheetName" />. Ignored if <paramref name="shotItemId" /> is set.</param>
	/// <param name="bouncesTillDestruct">The number of times the projectile can bounce off walls before being destroyed.</param>
	/// <param name="tailLength">The length of the tail which trails behind the main projectile.</param>
	/// <param name="rotationVelocity">The rotation velocity.</param>
	/// <param name="xVelocity">The speed at which the projectile moves along the X axis.</param>
	/// <param name="yVelocity">The speed at which the projectile moves along the Y axis.</param>
	/// <param name="startingPosition">The pixel world position at which the projectile will start moving.</param>
	/// <param name="collisionSound">The sound played when the projectile collides with something.</param>
	/// <param name="bounceSound">The sound played when the projectile bounces off a wall.</param>
	/// <param name="firingSound">The sound played when the projectile is fired.</param>
	/// <param name="explode">Whether the projectile explodes when it collides with something.</param>
	/// <param name="damagesMonsters">Whether the projectile damage monsters (true) or players (false).</param>
	/// <param name="location">The location containing the projectile.</param>
	/// <param name="firer">The character who fired the projectile.</param>
	/// <param name="collisionBehavior">A callback to invoke after the projectile collides with a player, monster, or wall.</param>
	/// <param name="shotItemId">The qualified or unqualified item ID to shoot. If set, this overrides <paramref name="spriteIndex" />.</param>
	public BasicProjectile(int damageToFarmer, int spriteIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, string collisionSound = null, string bounceSound = null, string firingSound = null, bool explode = false, bool damagesMonsters = false, GameLocation location = null, Character firer = null, onCollisionBehavior collisionBehavior = null, string shotItemId = null)
		: this()
	{
		this.damageToFarmer.Value = damageToFarmer;
		base.currentTileSheetIndex.Value = spriteIndex;
		base.bouncesLeft.Value = bouncesTillDestruct;
		base.tailLength.Value = tailLength;
		base.rotationVelocity.Value = rotationVelocity;
		base.xVelocity.Value = xVelocity;
		base.yVelocity.Value = yVelocity;
		base.position.Value = startingPosition;
		this.explode.Value = explode;
		this.collisionSound.Value = collisionSound;
		base.bounceSound.Value = bounceSound;
		base.damagesMonsters.Value = damagesMonsters;
		base.theOneWhoFiredMe.Set(location, firer);
		this.collisionBehavior = collisionBehavior;
		base.itemId.Value = ItemRegistry.QualifyItemId(shotItemId) ?? shotItemId;
		if (!string.IsNullOrEmpty(firingSound))
		{
			location?.playSound(firingSound);
		}
	}

	/// <summary>Construct an instance preconfigured for a spell cast by a monster.</summary>
	/// <param name="damageToFarmer">The amount of damage caused when this projectile hits a monster or player.</param>
	/// <param name="spriteIndex">The index of the sprite to draw in <see cref="F:StardewValley.Projectiles.Projectile.projectileSheetName" />.</param>
	/// <param name="bouncesTillDestruct">The number of times the projectile can bounce off walls before being destroyed.</param>
	/// <param name="tailLength">The length of the tail which trails behind the main projectile.</param>
	/// <param name="rotationVelocity">The rotation velocity.</param>
	/// <param name="xVelocity">The speed at which the projectile moves along the X axis.</param>
	/// <param name="yVelocity">The speed at which the projectile moves along the Y axis.</param>
	/// <param name="startingPosition">The pixel world position at which the projectile will start moving.</param>
	public BasicProjectile(int damageToFarmer, int spriteIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition)
		: this(damageToFarmer, spriteIndex, bouncesTillDestruct, tailLength, rotationVelocity, xVelocity, yVelocity, startingPosition, "flameSpellHit", "flameSpell", null, explode: true)
	{
	}

	public override void updatePosition(GameTime time)
	{
		base.xVelocity.Value += base.acceleration.X;
		base.yVelocity.Value += base.acceleration.Y;
		if (base.maxVelocity.Value != -1f && Math.Sqrt(base.xVelocity.Value * base.xVelocity.Value + base.yVelocity.Value * base.yVelocity.Value) >= (double)base.maxVelocity.Value)
		{
			base.xVelocity.Value -= base.acceleration.X;
			base.yVelocity.Value -= base.acceleration.Y;
		}
		base.position.X += base.xVelocity.Value;
		base.position.Y += base.yVelocity.Value;
	}

	/// <inheritdoc />
	protected override void InitNetFields()
	{
		base.InitNetFields();
		base.NetFields.AddField(this.damageToFarmer, "damageToFarmer").AddField(this.collisionSound, "collisionSound").AddField(this.explode, "explode")
			.AddField(this.debuff, "debuff")
			.AddField(this.debuffSound, "debuffSound");
	}

	public override void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player)
	{
		if (base.damagesMonsters.Value)
		{
			return;
		}
		if (this.debuff.Value != null && player.CanBeDamaged() && Game1.random.Next(11) >= player.Immunity && !player.hasBuff("28") && !player.hasTrinketWithID("BasiliskPaw"))
		{
			if (Game1.player == player)
			{
				player.applyBuff(this.debuff.Value);
			}
			location.playSound(this.debuffSound.Value);
		}
		if (player.CanBeDamaged())
		{
			base.piercesLeft.Value--;
		}
		player.takeDamage(this.damageToFarmer.Value, overrideParry: false, null);
		this.explosionAnimation(location);
	}

	public override void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation location)
	{
		t.performUseAction(tileLocation);
		this.explosionAnimation(location);
		base.piercesLeft.Value--;
	}

	public override void behaviorOnCollisionWithOther(GameLocation location)
	{
		if (!base.ignoreObjectCollisions.Value)
		{
			this.explosionAnimation(location);
			base.piercesLeft.Value--;
		}
	}

	public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
	{
		if (!base.damagesMonsters.Value)
		{
			return;
		}
		Farmer player = this.GetPlayerWhoFiredMe(location);
		this.explosionAnimation(location);
		if (n is Monster)
		{
			location.damageMonster(n.GetBoundingBox(), this.damageToFarmer.Value, this.damageToFarmer.Value + 1, isBomb: false, player, isProjectile: true);
			if (base.currentTileSheetIndex.Value == 15)
			{
				Utility.addRainbowStarExplosion(location, base.position.Value, 11);
			}
			if (!(n as Monster).IsInvisible)
			{
				base.piercesLeft.Value--;
			}
		}
		else if (base.itemId.Value != null)
		{
			n.getHitByPlayer(player, location);
			string projectileTokenizedName = TokenStringBuilder.ItemName(base.itemId.Value);
			Game1.multiplayer.globalChatInfoMessage("Slingshot_Hit", player.Name, n.GetTokenizedDisplayName(), Lexicon.prependTokenizedArticle(projectileTokenizedName));
			base.piercesLeft.Value--;
		}
	}

	protected virtual void explosionAnimation(GameLocation location)
	{
		if (base.projectileID.Value == 14)
		{
			for (int i = 0; i < 12; i++)
			{
				Vector2 motion = new Vector2(0f, -1.5f + (float)Game1.random.Next(-10, 11) / 12f);
				motion = Vector2.Transform(motion, Matrix.CreateRotationZ((float)(Math.PI / 6.0 + (double)((float)Game1.random.Next(-10, 11) / 50f)) * (float)i));
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(144, 249, 7, 7), 80f, 6, 1, base.position.Value + new Vector2(8f, 8f) * 4f, flicker: false, flipped: false, 1f, 0f, Utility.Get2PhaseColor(Color.White, Color.Cyan, 0, 1f, Game1.random.Next(1000)), 4f, 0f, 0f, 0f)
				{
					drawAboveAlwaysFront = true,
					motion = motion
				});
			}
		}
		else
		{
			Rectangle sourceRect = base.GetSourceRect();
			sourceRect.X += 4;
			sourceRect.Y += 4;
			sourceRect.Width = 8;
			sourceRect.Height = 8;
			if (base.itemId.Value != null)
			{
				int whichDebris = 12;
				switch (base.itemId.Value)
				{
				case "(O)390":
					whichDebris = 14;
					break;
				case "(O)378":
					whichDebris = 0;
					break;
				case "(O)380":
					whichDebris = 2;
					break;
				case "(O)384":
					whichDebris = 6;
					break;
				case "(O)386":
					whichDebris = 10;
					break;
				case "(O)382":
					whichDebris = 4;
					break;
				}
				Game1.createRadialDebris(location, whichDebris, (int)(base.position.X + 32f) / 64, (int)(base.position.Y + 32f) / 64, 6, resource: false);
			}
			else
			{
				Game1.createRadialDebris_MoreNatural(location, "TileSheets\\Projectiles", sourceRect, 1, (int)base.position.X + 32, (int)base.position.Y + 32, 6, (int)(base.position.Y / 64f) + 1);
			}
		}
		if (!string.IsNullOrEmpty(this.collisionSound.Value))
		{
			location.playSound(this.collisionSound.Value);
		}
		if (this.explode.Value)
		{
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, base.position.Value, flicker: false, Game1.random.NextBool()));
		}
		this.collisionBehavior?.Invoke(location, this.getBoundingBox().Center.X, this.getBoundingBox().Center.Y, this.GetPlayerWhoFiredMe(location));
		base.destroyMe = true;
	}

	public static void explodeOnImpact(GameLocation location, int x, int y, Character who)
	{
		location.explode(new Vector2(x / 64, y / 64), 2, who as Farmer);
	}

	/// <summary>Get the player who fired this projectile.</summary>
	/// <param name="location">The location containing the player.</param>
	public virtual Farmer GetPlayerWhoFiredMe(GameLocation location)
	{
		return (base.theOneWhoFiredMe.Get(location) as Farmer) ?? Game1.player;
	}
}
