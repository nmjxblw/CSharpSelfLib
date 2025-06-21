using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley;
using StardewValley.Audio;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Projectiles;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace Firearm.Framework;

[XmlType("Mods_Enaium_Firearm")]
public sealed class Firearm : Tool
{
	[XmlIgnore]
	private bool _using;

	[XmlIgnore]
	private readonly NetEvent0 _finishEvent = new NetEvent0(false);

	[XmlIgnore]
	private readonly NetPoint _aimPos = ((NetFieldBase<Point, NetPoint>)new NetPoint()).Interpolated(true, true);

	[XmlIgnore]
	private double _lastFireTime;

	public const string Ak47Id = "Firearm_AK47";

	public const string M16Id = "Firearm_M16";

	public const string AmmoAssaultRifleId = "Firearm_Ammo_Assault_Rifle";

	public override string TypeDefinitionId => "(W)";

	public Firearm()
		: this("Firearm_AK47")
	{
	}

	public Firearm(string id)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		id = ((Item)this).ValidateUnqualifiedItemId(id);
		ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem("(W)" + (object)((Item)this).itemId);
		((Item)this).ItemId = id;
		((Item)this).Name = dataOrErrorItem.InternalName;
		((Tool)this).InitialParentTileIndex = dataOrErrorItem.SpriteIndex;
		((Tool)this).CurrentParentTileIndex = dataOrErrorItem.SpriteIndex;
		((Tool)this).IndexOfMenuItemView = dataOrErrorItem.SpriteIndex;
		((NetFieldBase<int, NetInt>)(object)base.numAttachmentSlots).Value = 1;
		((NetArray<Object, NetRef<Object>>)(object)base.attachments).SetCount(1);
	}

	protected override void initNetFields()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		base.initNetFields();
		((Item)this).NetFields.AddField((INetSerializable)(object)_finishEvent, "finishEvent").AddField((INetSerializable)(object)_aimPos, "aimPos");
		_finishEvent.onEvent += new Event(DoFinish);
	}

	public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
	{
		who.canReleaseTool = false;
		_using = true;
		return true;
	}

	public override void tickUpdate(GameTime time, Farmer who)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		base.lastUser = who;
		_finishEvent.Poll();
		if (!_using || !who.IsLocalPlayer)
		{
			return;
		}
		int x = _aimPos.X;
		int y = _aimPos.Y;
		Vector2 shootOrigin = GetShootOrigin(who);
		Vector2 vector2 = AdjustForHeight(new Vector2((float)x, (float)y)) - shootOrigin;
		if (!_using)
		{
			return;
		}
		UpdateAimPos();
		if (Math.Abs(vector2.X) > Math.Abs(vector2.Y))
		{
			if ((double)vector2.X < 0.0)
			{
				((Character)who).faceDirection(3);
			}
			if ((double)vector2.X > 0.0)
			{
				((Character)who).faceDirection(1);
			}
		}
		else
		{
			if ((double)vector2.Y < 0.0)
			{
				((Character)who).faceDirection(0);
			}
			if ((double)vector2.Y > 0.0)
			{
				((Character)who).faceDirection(2);
			}
		}
		DoFire(who, x, y, shootOrigin);
	}

	private void DoFire(Farmer who, int x, int y, Vector2 shootOrigin)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Expected O, but got Unknown
		Object attachment = ((NetArray<Object, NetRef<Object>>)(object)base.attachments)[0];
		if (attachment == null)
		{
			Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14254"), 3)
			{
				timeLeft = 100f
			});
		}
		else if (!(_lastFireTime + (double)(1000f / ((float)GetShotSpeed() / 60f)) >= Game1.currentGameTime.TotalGameTime.TotalMilliseconds))
		{
			Object one = (Object)((Item)attachment).getOne();
			if (((Item)attachment).ConsumeStack(1) == null)
			{
				((NetArray<Object, NetRef<Object>>)(object)base.attachments)[0] = null;
			}
			NetCollection<Projectile> projectiles = ((Character)who).currentLocation.projectiles;
			int ammoDamage = GetAmmoDamage(one);
			Vector2 velocityTowardPoint = Utility.getVelocityTowardPoint(GetShootOrigin(who), AdjustForHeight(new Vector2((float)x, (float)y)), (float)(15 + Game1.random.Next(4, 6)) * (1f + who.buffs.WeaponSpeedMultiplier));
			BasicProjectile basicProjectile = new BasicProjectile((int)((double)(ammoDamage + Game1.random.Next(-(ammoDamage / 2), ammoDamage + 2)) * (1.0 + (double)who.buffs.AttackMultiplier)), -1, 0, 0, 0f, velocityTowardPoint.X, velocityTowardPoint.Y, shootOrigin - new Vector2(32f, (((Character)base.lastUser).FacingDirection != 0) ? 32f : 96f), (string)null, (string)null, (string)null, false, true, ((Character)who).currentLocation, (Character)(object)who, (onCollisionBehavior)null, ((Item)one).ItemId)
			{
				IgnoreLocationCollision = (Game1.currentLocation.currentEvent != null || Game1.currentMinigame != null)
			};
			_lastFireTime = Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
			projectiles.Add((Projectile)(object)basicProjectile);
			((Character)who).playNearbySoundAll(GetAudioName(), (int?)null, (SoundContext)0);
		}
	}

	private int GetAmmoDamage(Object one)
	{
		if (1 == 0)
		{
		}
		int result;
		if (one != null)
		{
			string name = ((Item)one).Name;
			if (name == "Firearm_Ammo_Assault_Rifle")
			{
				result = ModEntry.GetInstance().Config.AssaultRifleDamage;
				goto IL_0034;
			}
		}
		result = 0;
		goto IL_0034;
		IL_0034:
		if (1 == 0)
		{
		}
		return result;
	}

	private string GetAudioName()
	{
		string itemId = ((Item)this).ItemId;
		if (1 == 0)
		{
		}
		string result = ((itemId == "Firearm_AK47") ? ModEntry.GetInstance().Config.Ak47ShotAudio : ((!(itemId == "Firearm_M16")) ? "" : ModEntry.GetInstance().Config.M16ShotAudio));
		if (1 == 0)
		{
		}
		return result;
	}

	private int GetShotSpeed()
	{
		string itemId = ((Item)this).ItemId;
		if (1 == 0)
		{
		}
		int result = ((itemId == "Firearm_AK47") ? ModEntry.GetInstance().Config.Ak47ShotSpeed : ((itemId == "Firearm_M16") ? ModEntry.GetInstance().Config.M16ShotSpeed : 0));
		if (1 == 0)
		{
		}
		return result;
	}

	public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
	{
		_finishEvent.Fire();
	}

	public override bool onRelease(GameLocation location, int x, int y, Farmer who)
	{
		((Tool)this).DoFunction(location, x, y, 1, who);
		return true;
	}

	private void DoFinish()
	{
		if (base.lastUser != null)
		{
			base.lastUser.usingSlingshot = false;
			base.lastUser.canReleaseTool = true;
			base.lastUser.UsingTool = false;
			base.lastUser.canMove = true;
			((Character)base.lastUser).Halt();
			_using = false;
		}
	}

	public override void draw(SpriteBatch b)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(((Item)this).QualifiedItemId);
		Vector2 position = ((Character)base.lastUser).Position;
		SpriteEffects spriteEffects = (SpriteEffects)0;
		position.X += 32f;
		position.Y -= 16f;
		int x = _aimPos.X;
		int y = _aimPos.Y;
		float rotation;
		if (((Character)base.lastUser).FacingDirection == 3)
		{
			spriteEffects = (SpriteEffects)2;
			rotation = 2.41661f;
			rotation += (float)Math.Atan2(position.Y - (float)y, position.X - (float)x);
		}
		else
		{
			rotation = (float)Math.PI / 4f;
			rotation += (float)Math.Atan2((float)y - position.Y, (float)x - position.X);
		}
		if (((Character)base.lastUser).FacingDirection != 0)
		{
			b.Draw(dataOrErrorItem.GetTexture(), Game1.GlobalToLocal(Game1.viewport, position), (Rectangle?)new Rectangle(0, 0, 32, 32), Color.White, rotation, new Vector2(16f, 16f), 3f, spriteEffects, 0.999999f);
		}
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(((Item)this).QualifiedItemId);
		spriteBatch.Draw(dataOrErrorItem.GetTexture(), location + new Vector2(32f, 29f), (Rectangle?)Game1.getSourceRectForStandardTileSheet(dataOrErrorItem.GetTexture(), 0, 32, 32), color * transparency, 0f, new Vector2(14f, 14f), scaleSize * 2f, (SpriteEffects)0, layerDepth);
		if ((int)drawStackNumber != 0 && ((NetArray<Object, NetRef<Object>>)(object)base.attachments)?[0] != null)
		{
			Utility.drawTinyDigits(((Item)((NetArray<Object, NetRef<Object>>)(object)base.attachments)[0]).Stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(((Item)((NetArray<Object, NetRef<Object>>)(object)base.attachments)[0]).Stack, 3f * scaleSize)) + 3f * scaleSize, (float)(64.0 - 18.0 * (double)scaleSize + 2.0)), 3f * scaleSize, 1f, Color.White);
		}
		((Item)this).DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
	}

	private void UpdateAimPos()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Invalid comparison between Unknown and I4
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Invalid comparison between Unknown and I4
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Invalid comparison between Unknown and I4
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Invalid comparison between Unknown and I4
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		Farmer lastUser = base.lastUser;
		if (lastUser == null || !lastUser.IsLocalPlayer)
		{
			return;
		}
		Point point = Game1.getMousePosition();
		if (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse)
		{
			GamePadThumbSticks thumbSticks = ((GamePadState)(ref Game1.oldPadState)).ThumbSticks;
			Vector2 vector2 = ((GamePadThumbSticks)(ref thumbSticks)).Left;
			if ((double)((Vector2)(ref vector2)).Length() < 0.25)
			{
				vector2.X = 0f;
				vector2.Y = 0f;
				GamePadDPad dpad = ((GamePadState)(ref Game1.oldPadState)).DPad;
				if ((int)((GamePadDPad)(ref dpad)).Down == 1)
				{
					vector2.Y = -1f;
				}
				else
				{
					dpad = ((GamePadState)(ref Game1.oldPadState)).DPad;
					if ((int)((GamePadDPad)(ref dpad)).Up == 1)
					{
						vector2.Y = 1f;
					}
				}
				dpad = ((GamePadState)(ref Game1.oldPadState)).DPad;
				if ((int)((GamePadDPad)(ref dpad)).Left == 1)
				{
					vector2.X = -1f;
				}
				dpad = ((GamePadState)(ref Game1.oldPadState)).DPad;
				if ((int)((GamePadDPad)(ref dpad)).Right == 1)
				{
					vector2.X = 1f;
				}
				if ((double)vector2.X != 0.0 && (double)vector2.Y != 0.0)
				{
					((Vector2)(ref vector2)).Normalize();
					vector2 *= 1f;
				}
			}
			Vector2 shootOrigin = GetShootOrigin(base.lastUser);
			if (!Game1.options.useLegacySlingshotFiring && (double)((Vector2)(ref vector2)).Length() < 0.25)
			{
				int facingDirection = ((Character)base.lastUser).FacingDirection;
				if (1 == 0)
				{
				}
				Vector2 val = (Vector2)(facingDirection switch
				{
					0 => new Vector2(0f, 1f), 
					1 => new Vector2(1f, 0f), 
					2 => new Vector2(0f, -1f), 
					3 => new Vector2(-1f, 0f), 
					_ => vector2, 
				});
				if (1 == 0)
				{
				}
				vector2 = val;
			}
			point = Utility.Vector2ToPoint(shootOrigin + new Vector2(vector2.X, 0f - vector2.Y) * 600f);
			point.X -= ((Rectangle)(ref Game1.viewport)).X;
			point.Y -= ((Rectangle)(ref Game1.viewport)).Y;
		}
		int num1 = point.X + ((Rectangle)(ref Game1.viewport)).X;
		int num2 = point.Y + ((Rectangle)(ref Game1.viewport)).Y;
		_aimPos.X = num1;
		_aimPos.Y = num2;
	}

	protected override void GetAttachmentSlotSprite(int slot, out Texture2D texture, out Rectangle sourceRect)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		((Tool)this).GetAttachmentSlotSprite(slot, ref texture, ref sourceRect);
		if (((NetArray<Object, NetRef<Object>>)(object)base.attachments)[0] == null)
		{
			sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 43, -1, -1);
		}
	}

	protected override bool canThisBeAttached(Object o, int slot)
	{
		string itemId = ((Item)this).ItemId;
		if (1 == 0)
		{
		}
		bool result = (itemId == "Firearm_AK47" || itemId == "Firearm_M16") && ((Item)o).Name == "Firearm_Ammo_Assault_Rifle";
		if (1 == 0)
		{
		}
		return result;
	}

	public override string? getHoverBoxText(Item? hoveredItem)
	{
		Object o = (Object)(object)((hoveredItem is Object) ? hoveredItem : null);
		if (o != null && ((Tool)this).canThisBeAttached(o))
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14256", (object)((Item)this).DisplayName, (object)((Item)o).DisplayName);
		}
		return (hoveredItem == null && ((NetArray<Object, NetRef<Object>>)(object)base.attachments)?[0] != null) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14258", (object)((Item)((NetArray<Object, NetRef<Object>>)(object)base.attachments)[0]).DisplayName) : null;
	}

	protected override void MigrateLegacyItemId()
	{
		((Item)this).ItemId = ((Tool)this).InitialParentTileIndex.ToString();
	}

	protected override Item GetOneNew()
	{
		return (Item)(object)new Firearm(((Item)this).ItemId);
	}

	protected override string loadDisplayName()
	{
		return ItemRegistry.GetDataOrErrorItem(((Item)this).QualifiedItemId).DisplayName;
	}

	protected override string loadDescription()
	{
		return ItemRegistry.GetDataOrErrorItem(((Item)this).QualifiedItemId).Description;
	}

	public override bool doesShowTileLocationMarker()
	{
		return false;
	}

	private Vector2 GetShootOrigin(Farmer who)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return AdjustForHeight(((Character)who).getStandingPosition(), forCursor: false);
	}

	private Vector2 AdjustForHeight(Vector2 position, bool forCursor = true)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		return (!Game1.options.useLegacySlingshotFiring && forCursor) ? new Vector2(position.X, position.Y) : new Vector2(position.X, (float)((double)position.Y - 32.0 - 8.0));
	}
}
