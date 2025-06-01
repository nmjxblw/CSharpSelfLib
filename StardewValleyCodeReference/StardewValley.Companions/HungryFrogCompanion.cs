using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Projectiles;

namespace StardewValley.Companions;

public class HungryFrogCompanion : HoppingCompanion
{
	private const int RANGE = 300;

	private const int FULLNESS_TIME = 12000;

	public float fullnessTime;

	private float monsterEatCheckTimer;

	private float tongueOutTimer;

	private readonly NetBool tongueOut = new NetBool(value: false);

	private readonly NetBool tongueReturn = new NetBool(value: false);

	private readonly NetPosition tonguePosition = new NetPosition();

	private readonly NetVector2 tongueVelocity = new NetVector2();

	private readonly NetNPCRef attachedMonsterField = new NetNPCRef();

	private readonly NetEvent0 fullnessTrigger = new NetEvent0();

	private float initialEquipDelay = 12000f;

	private float lastHopTimer;

	private Monster attachedMonster
	{
		get
		{
			if (base.Owner != null)
			{
				return this.attachedMonsterField.Get(base.Owner.currentLocation) as Monster;
			}
			return null;
		}
		set
		{
			this.attachedMonsterField.Set(base.Owner.currentLocation, value);
		}
	}

	public HungryFrogCompanion()
	{
	}

	public HungryFrogCompanion(int variant)
	{
		base.whichVariant.Value = variant;
	}

	public override void InitNetFields()
	{
		base.InitNetFields();
		base.NetFields.AddField(this.tongueOut, "tongueOut").AddField(this.tongueReturn, "tongueReturn").AddField(this.tonguePosition.NetFields, "tonguePosition.NetFields")
			.AddField(this.tongueVelocity, "tongueVelocity")
			.AddField(this.attachedMonsterField.NetFields, "attachedMonsterField.NetFields")
			.AddField(this.fullnessTrigger, "fullnessTrigger");
		this.fullnessTrigger.onEvent += triggerFullnessTimer;
	}

	public override void Update(GameTime time, GameLocation location)
	{
		if (!this.tongueOut.Value)
		{
			base.Update(time, location);
		}
		if (!Game1.shouldTimePass())
		{
			return;
		}
		if (this.fullnessTime > 0f)
		{
			this.fullnessTime -= (float)time.ElapsedGameTime.TotalMilliseconds;
		}
		this.lastHopTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
		if (this.initialEquipDelay > 0f)
		{
			this.initialEquipDelay -= (float)time.ElapsedGameTime.TotalMilliseconds;
			return;
		}
		if (base.IsLocal)
		{
			this.monsterEatCheckTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.monsterEatCheckTimer >= 2000f && this.fullnessTime <= 0f && !this.tongueOut.Value)
			{
				this.monsterEatCheckTimer = 0f;
				if (!(location is SlimeHutch))
				{
					Monster closest_monster = Utility.findClosestMonsterWithinRange(location, base.Position, 300);
					if (closest_monster != null)
					{
						if (closest_monster is Bat && closest_monster.Age == 789)
						{
							this.monsterEatCheckTimer = 0f;
							return;
						}
						if (closest_monster.Name.Equals("Truffle Crab"))
						{
							this.monsterEatCheckTimer = 0f;
							return;
						}
						if (closest_monster is GreenSlime slime && slime.prismatic.Value)
						{
							this.monsterEatCheckTimer = 0f;
							return;
						}
						base.height = 0f;
						Vector2 motion = Utility.getVelocityTowardPoint(base.Position, closest_monster.getStandingPosition(), 12f);
						this.tongueOut.Value = true;
						this.tongueReturn.Value = false;
						this.tonguePosition.Value = base.Position + new Vector2(-32f, -32f) + new Vector2((base.direction.Value != 3) ? 28 : 0, -20f);
						this.tongueVelocity.Value = motion;
						location.playSound("croak");
						base.direction.Value = ((!(closest_monster.Position.X < base.Position.X)) ? 1 : 3);
					}
				}
				this.tongueOutTimer = 0f;
			}
			if (this.tongueOut.Value)
			{
				this.tongueOutTimer += (float)time.ElapsedGameTime.TotalMilliseconds * (float)((!this.tongueReturn.Value) ? 1 : (-1));
				this.tonguePosition.Value += this.tongueVelocity.Value;
				if (this.attachedMonster == null)
				{
					if (Vector2.Distance(base.Position, this.tonguePosition.Value) >= 300f)
					{
						this.tongueReachedMonster(null);
					}
					else
					{
						int damageSize = 40;
						Rectangle boundingBox = new Rectangle((int)this.tonguePosition.X + 32 - damageSize / 2, (int)this.tonguePosition.Y + 32 - damageSize / 2, damageSize, damageSize);
						if (base.Owner.currentLocation.doesPositionCollideWithCharacter(boundingBox) is Monster monster)
						{
							this.tongueReachedMonster(monster);
						}
					}
				}
				if (this.attachedMonster != null)
				{
					this.attachedMonster.Position = this.tonguePosition.Value;
					this.attachedMonster.xVelocity = 0f;
					this.attachedMonster.yVelocity = 0f;
				}
				if (this.tongueReturn.Value)
				{
					Vector2 homingVector = Vector2.Subtract(base.Position + new Vector2(-32f, -32f) + new Vector2((base.direction.Value != 3) ? 28 : 0, -20f), this.tonguePosition.Value);
					homingVector.Normalize();
					homingVector *= 12f;
					this.tongueVelocity.Value = homingVector;
				}
				if ((this.tongueReturn.Value && Vector2.Distance(base.Position, this.tonguePosition.Value) <= 48f) || this.tongueOutTimer <= 0f)
				{
					if (this.attachedMonster != null)
					{
						if (this.attachedMonster is HotHead hothead && hothead.timeUntilExplode.Value > 0f)
						{
							hothead.currentLocation?.netAudio.StopPlaying("fuse");
						}
						if (this.attachedMonster.currentLocation != null)
						{
							this.attachedMonster.currentLocation.characters.Remove(this.attachedMonster);
						}
						else
						{
							location.characters.Remove(this.attachedMonster);
						}
						this.fullnessTrigger.Fire();
						this.attachedMonster = null;
					}
					Vector2.Distance(base.Position, this.tonguePosition.Value);
					this.tongueOut.Value = false;
					this.tongueReturn.Value = false;
				}
			}
		}
		else if (this.tongueOut.Value && this.attachedMonster != null)
		{
			this.attachedMonster.Position = this.tonguePosition.Value;
			this.attachedMonster.position.Paused = true;
			this.attachedMonster.xVelocity = 0f;
			this.attachedMonster.yVelocity = 0f;
		}
		this.fullnessTrigger.Poll();
	}

	public override void OnOwnerWarp()
	{
		this.attachedMonster = null;
		this.tongueOut.Value = false;
		this.tongueReturn.Value = false;
		base.OnOwnerWarp();
	}

	public override void Hop(float amount)
	{
		base.Hop(amount);
		if (this.fullnessTime > 0f)
		{
			base.Owner?.currentLocation.localSound("frog_slap");
		}
		this.lastHopTimer = 0f;
	}

	private void triggerFullnessTimer()
	{
		this.fullnessTime = 12000f;
	}

	public void tongueReachedMonster(Monster m)
	{
		this.tongueReturn.Value = true;
		this.tongueVelocity.Value = this.tongueVelocity.Value * -1f;
		this.attachedMonster = m;
		if (m != null)
		{
			m.DamageToFarmer = 0;
			m.farmerPassesThrough = true;
			base.Owner?.currentLocation.localSound("fishSlap");
		}
	}

	public override void Draw(SpriteBatch b)
	{
		if (base.Owner?.currentLocation == null || (base.Owner.currentLocation.DisplayName == "Temp" && !Game1.isFestival()))
		{
			return;
		}
		Texture2D texture = Game1.content.Load<Texture2D>("TileSheets\\companions");
		SpriteEffects effect = SpriteEffects.None;
		Rectangle startingSourceRect = new Rectangle((this.fullnessTime > 0f) ? 128 : 0, 16 + base.whichVariant.Value * 16, 16, 16);
		Color c = ((base.whichVariant.Value == 7) ? Utility.GetPrismaticColor() : Color.White);
		if (base.direction.Value == 3)
		{
			effect = SpriteEffects.FlipHorizontally;
		}
		if (this.tongueOut.Value)
		{
			b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - base.height) * 4f)), Utility.translateRect(startingSourceRect, 112), c, 0f, new Vector2(8f, 16f), 4f, effect, (base._position.Y - 12f) / 10000f);
		}
		else if (base.height > 0f)
		{
			if (base.gravity > 0f)
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - base.height) * 4f)), Utility.translateRect(startingSourceRect, 16), c, 0f, new Vector2(8f, 16f), 4f, effect, (base._position.Y - 12f) / 10000f);
			}
			else if (base.gravity > -0.15f)
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - base.height) * 4f)), Utility.translateRect(startingSourceRect, 32), c, 0f, new Vector2(8f, 16f), 4f, effect, (base._position.Y - 12f) / 10000f);
			}
			else
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - base.height) * 4f)), Utility.translateRect(startingSourceRect, 48), c, 0f, new Vector2(8f, 16f), 4f, effect, (base._position.Y - 12f) / 10000f);
			}
		}
		else if (this.lastHopTimer > 5000f && !this.tongueOut.Value)
		{
			b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - base.height) * 4f)), Utility.translateRect(startingSourceRect, 80 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 >= 200.0) ? 16 : 0)), c, 0f, new Vector2(8f, 16f), 4f, effect, (base._position.Y - 12f) / 10000f);
		}
		else
		{
			b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - base.height) * 4f)), startingSourceRect, c, 0f, new Vector2(8f, 16f), 4f, effect, (base._position.Y - 12f) / 10000f);
		}
		b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f * Utility.Lerp(1f, 0.8f, Math.Min(base.height, 1f)), SpriteEffects.None, 0f);
		if (this.tongueOut.Value)
		{
			Vector2 v = Game1.GlobalToLocal(this.tonguePosition.Value + new Vector2(32f));
			Vector2 v2 = Game1.GlobalToLocal(base.Position + new Vector2(-32f, -32f) + new Vector2((base.direction.Value != 3) ? 44 : 24, 16f));
			Utility.drawLineWithScreenCoordinates((int)v2.X, (int)v2.Y, (int)v.X, (int)v.Y, b, Color.Red, 1f, 4);
			Texture2D projTex = Projectile.projectileSheet;
			Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(Projectile.projectileSheet, 19, 16, 16);
			b.Draw(projTex, Game1.GlobalToLocal(this.tonguePosition.Value + new Vector2(32f, 32f)) + base.Owner.drawOffset, sourceRect, Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, 1f);
		}
	}
}
