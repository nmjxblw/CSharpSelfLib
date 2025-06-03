using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewValley.Objects.Trinkets;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace StardewValley;

public class FarmerSprite : AnimatedSprite
{
	public struct AnimationFrame
	{
		public int frame;

		public int milliseconds;

		public int positionOffset;

		public int xOffset;

		public int armOffset;

		public bool flip;

		public endOfAnimationBehavior frameStartBehavior;

		public endOfAnimationBehavior frameEndBehavior;

		public AnimationFrame(int frame, int milliseconds, int position_offset, bool secondary_arm, bool flip, endOfAnimationBehavior frame_start_behavior, endOfAnimationBehavior frame_end_behavior, int x_offset, bool hideArms = false)
		{
			this.frame = frame;
			this.milliseconds = milliseconds;
			this.positionOffset = position_offset;
			if (hideArms)
			{
				this.armOffset = -1;
			}
			else
			{
				this.armOffset = (secondary_arm ? 12 : 6);
			}
			this.flip = flip;
			this.frameStartBehavior = frame_start_behavior;
			this.frameEndBehavior = frame_end_behavior;
			this.xOffset = x_offset;
		}

		public AnimationFrame(int frame, int milliseconds, int position_offset, int armOffset, bool flip, endOfAnimationBehavior frame_start_behavior, endOfAnimationBehavior frame_end_behavior, int x_offset)
		{
			this.frame = frame;
			this.milliseconds = milliseconds;
			this.positionOffset = position_offset;
			this.armOffset = armOffset;
			this.flip = flip;
			this.frameStartBehavior = frame_start_behavior;
			this.frameEndBehavior = frame_end_behavior;
			this.xOffset = x_offset;
		}

		public AnimationFrame(int frame, int milliseconds, int positionOffset, bool secondaryArm, bool flip, endOfAnimationBehavior frameBehavior = null, bool behaviorAtEndOfFrame = false, int xOffset = 0)
			: this(frame, milliseconds, positionOffset, secondaryArm, flip, null, null, xOffset)
		{
			if (!behaviorAtEndOfFrame)
			{
				this.frameStartBehavior = frameBehavior;
			}
			else
			{
				this.frameEndBehavior = frameBehavior;
			}
		}

		public AnimationFrame(int frame, int milliseconds, bool secondaryArm, bool flip, endOfAnimationBehavior frameBehavior = null, bool behaviorAtEndOfFrame = false)
			: this(frame, milliseconds, 0, secondaryArm, flip, frameBehavior, behaviorAtEndOfFrame)
		{
		}

		public AnimationFrame(int frame, int milliseconds, bool secondaryArm, bool flip, bool hideArm)
			: this(frame, milliseconds, 0, secondaryArm, flip, null, null, 0, hideArm)
		{
		}

		public AnimationFrame(int frame, int milliseconds)
			: this(frame, milliseconds, secondaryArm: false, flip: false)
		{
		}

		public AnimationFrame(int frame, int milliseconds, int armOffset, bool flip = false)
			: this(frame, milliseconds, 0, armOffset, flip, null, null, 0)
		{
		}

		public AnimationFrame AddFrameAction(endOfAnimationBehavior callback)
		{
			this.frameStartBehavior = (endOfAnimationBehavior)Delegate.Combine(this.frameStartBehavior, callback);
			return this;
		}

		public AnimationFrame AddFrameEndAction(endOfAnimationBehavior callback)
		{
			this.frameEndBehavior = (endOfAnimationBehavior)Delegate.Combine(this.frameEndBehavior, callback);
			return this;
		}
	}

	public const int walkDown = 0;

	public const int walkRight = 8;

	public const int walkUp = 16;

	public const int walkLeft = 24;

	public const int runDown = 32;

	public const int runRight = 40;

	public const int runUp = 48;

	public const int runLeft = 56;

	public const int grabDown = 64;

	public const int grabRight = 72;

	public const int grabUp = 80;

	public const int grabLeft = 88;

	public const int carryWalkDown = 96;

	public const int carryWalkRight = 104;

	public const int carryWalkUp = 112;

	public const int carryWalkLeft = 120;

	public const int carryRunDown = 128;

	public const int carryRunRight = 136;

	public const int carryRunUp = 144;

	public const int carryRunLeft = 152;

	public const int toolDown = 160;

	public const int toolRight = 168;

	public const int toolUp = 176;

	public const int toolLeft = 184;

	public const int toolChooseDown = 192;

	public const int toolChooseRight = 194;

	public const int toolChooseUp = 196;

	public const int toolChooseLeft = 198;

	public const int seedThrowDown = 200;

	public const int seedThrowRight = 204;

	public const int seedThrowUp = 208;

	public const int seedThrowLeft = 212;

	public const int eat = 216;

	public const int sick = 224;

	public const int swordswipeDown = 232;

	public const int swordswipeRight = 240;

	public const int swordswipeUp = 248;

	public const int swordswipeLeft = 256;

	public const int punchDown = 272;

	public const int punchRight = 274;

	public const int punchUp = 276;

	public const int punchLeft = 278;

	public const int harvestItemUp = 279;

	public const int harvestItemRight = 280;

	public const int harvestItemDown = 281;

	public const int harvestItemLeft = 282;

	public const int shearUp = 283;

	public const int shearRight = 284;

	public const int shearDown = 285;

	public const int shearLeft = 286;

	public const int milkUp = 287;

	public const int milkRight = 288;

	public const int milkDown = 289;

	public const int milkLeft = 290;

	public const int tired = 291;

	public const int tired2 = 292;

	public const int passOutTired = 293;

	public const int drink = 294;

	public const int fishingUp = 295;

	public const int fishingRight = 296;

	public const int fishingDown = 297;

	public const int fishingLeft = 298;

	public const int fishingDoneUp = 299;

	public const int fishingDoneRight = 300;

	public const int fishingDoneDown = 301;

	public const int fishingDoneLeft = 302;

	public const int pan = 303;

	public const int showHoldingEdible = 304;

	private int currentToolIndex;

	private float oldInterval;

	public bool pauseForSingleAnimation;

	public bool animateBackwards;

	public bool loopThisAnimation;

	public bool freezeUntilDialogueIsOver;

	public int currentSingleAnimation = -1;

	public int currentAnimationFrames;

	public float currentSingleAnimationInterval = 200f;

	public float intervalModifier = 1f;

	public string currentStep = "sandyStep";

	/// <summary>The farmer who uses this sprite.</summary>
	private Farmer owner;

	public bool animatingBackwards;

	public const int cheer = 97;

	/// <inheritdoc />
	public override Character Owner => this.owner;

	public AnimationFrame CurrentAnimationFrame
	{
		get
		{
			if (base.CurrentAnimation == null)
			{
				return new AnimationFrame(0, 100, 0, secondaryArm: false, flip: false);
			}
			return base.CurrentAnimation[base.currentAnimationIndex % base.CurrentAnimation.Count];
		}
	}

	public int CurrentSingleAnimation
	{
		get
		{
			if (base.CurrentAnimation != null)
			{
				return base.CurrentAnimation[0].frame;
			}
			return -1;
		}
	}

	public override int CurrentFrame
	{
		get
		{
			return base.currentFrame;
		}
		set
		{
			if (base.currentFrame != value && !this.freezeUntilDialogueIsOver)
			{
				base.currentFrame = value;
				this.UpdateSourceRect();
			}
			if (value > FarmerRenderer.featureYOffsetPerFrame.Length - 1)
			{
				base.currentFrame = 0;
			}
		}
	}

	public bool PauseForSingleAnimation
	{
		get
		{
			return this.pauseForSingleAnimation;
		}
		set
		{
			this.pauseForSingleAnimation = value;
		}
	}

	public int CurrentToolIndex
	{
		get
		{
			return this.currentToolIndex;
		}
		set
		{
			this.currentToolIndex = value;
		}
	}

	/// <inheritdoc />
	public override void SetOwner(Character owner)
	{
		if (!(owner is Farmer farmer))
		{
			throw new InvalidOperationException("The owner of a FarmerSprite must be a Farmer.");
		}
		this.owner = farmer;
	}

	public void setCurrentAnimation(AnimationFrame[] animation)
	{
		this.currentSingleAnimation = -1;
		base.currentAnimation.Clear();
		base.currentAnimation.AddRange(animation);
		base.oldFrame = this.CurrentFrame;
		base.currentAnimationIndex = 0;
		if (base.CurrentAnimation.Count > 0)
		{
			base.interval = base.CurrentAnimation[0].milliseconds;
			this.CurrentFrame = base.CurrentAnimation[0].frame;
			this.currentAnimationFrames = base.CurrentAnimation.Count;
		}
	}

	public override void faceDirection(int direction)
	{
		bool carrying = this.owner?.IsCarrying() ?? false;
		if (!this.IsPlayingBasicAnimation(direction, carrying))
		{
			switch (direction)
			{
			case 0:
				this.setCurrentFrame(12, 1, 100, 1, flip: false, carrying);
				break;
			case 1:
				this.setCurrentFrame(6, 1, 100, 1, flip: false, carrying);
				break;
			case 2:
				this.setCurrentFrame(0, 1, 100, 1, flip: false, carrying);
				break;
			case 3:
				this.setCurrentFrame(6, 1, 100, 1, flip: true, carrying);
				break;
			}
			this.UpdateSourceRect();
		}
	}

	public virtual bool IsPlayingBasicAnimation(int direction, bool carrying)
	{
		bool moving = false;
		if (this.owner != null && this.owner.CanMove && this.owner.isMoving())
		{
			moving = true;
		}
		switch (direction)
		{
		case 0:
			if (carrying)
			{
				if (!moving)
				{
					if (this.CurrentFrame == 113)
					{
						return true;
					}
					return false;
				}
				if (this.currentSingleAnimation == 112 || this.currentSingleAnimation == 144)
				{
					return true;
				}
				break;
			}
			if (!moving)
			{
				if (this.CurrentFrame == 17)
				{
					return true;
				}
				return false;
			}
			if (this.currentSingleAnimation == 16 || this.currentSingleAnimation == 48)
			{
				return true;
			}
			break;
		case 2:
			if (carrying)
			{
				if (!moving)
				{
					if (this.CurrentFrame == 97)
					{
						return true;
					}
					return false;
				}
				if (this.currentSingleAnimation == 96 || this.currentSingleAnimation == 128)
				{
					return true;
				}
				break;
			}
			if (!moving)
			{
				if (this.CurrentFrame == 1)
				{
					return true;
				}
				return false;
			}
			if (this.currentSingleAnimation == 0 || this.currentSingleAnimation == 32)
			{
				return true;
			}
			break;
		case 3:
			if (carrying)
			{
				if (!moving)
				{
					if (this.CurrentFrame == 121)
					{
						return true;
					}
					return false;
				}
				if (this.currentSingleAnimation == 120 || this.currentSingleAnimation == 152)
				{
					return true;
				}
				break;
			}
			if (!moving)
			{
				if (this.CurrentFrame == 25)
				{
					return true;
				}
				return false;
			}
			if (this.currentSingleAnimation == 24 || this.currentSingleAnimation == 56)
			{
				return true;
			}
			break;
		case 1:
			if (carrying)
			{
				if (!moving)
				{
					if (this.CurrentFrame == 105)
					{
						return true;
					}
					return false;
				}
				if (this.currentSingleAnimation == 104 || this.currentSingleAnimation == 136)
				{
					return true;
				}
				break;
			}
			if (!moving)
			{
				if (this.CurrentFrame == 9)
				{
					return true;
				}
				return false;
			}
			if (this.currentSingleAnimation == 8 || this.currentSingleAnimation == 40)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public void setCurrentSingleFrame(int which, short interval = 32000, bool secondaryArm = false, bool flip = false)
	{
		this.loopThisAnimation = false;
		base.currentAnimation.Clear();
		base.currentAnimation.Add(new AnimationFrame((short)which, interval, secondaryArm, flip));
		this.CurrentFrame = base.CurrentAnimation[0].frame;
	}

	public void setCurrentFrame(int which)
	{
		this.setCurrentFrame(which, 0);
	}

	public void setCurrentFrame(int which, int offset)
	{
		this.setCurrentFrame(which, offset, 100, 1, flip: false, secondaryArm: false);
	}

	public void setCurrentFrameBackwards(int which, int offset, int interval, int numFrames, bool secondaryArm, bool flip)
	{
		this.getAnimationFromIndex(which, this, interval, numFrames, secondaryArm, flip);
		base.CurrentAnimation.Reverse();
		this.CurrentFrame = base.CurrentAnimation[Math.Min(base.CurrentAnimation.Count - 1, offset)].frame;
	}

	public void setCurrentFrame(int which, int offset, int interval, int numFrames, bool flip, bool secondaryArm)
	{
		this.getAnimationFromIndex(which, this, interval, numFrames, flip, secondaryArm);
		base.currentAnimationIndex = Math.Min(base.CurrentAnimation.Count - 1, offset);
		this.CurrentFrame = base.CurrentAnimation[base.currentAnimationIndex].frame;
		base.interval = this.CurrentAnimationFrame.milliseconds;
		base.timer = 0f;
	}

	public FarmerSprite()
	{
		base.interval /= 2f;
		base.SpriteWidth = 16;
		base.SpriteHeight = 32;
		this.UpdateSourceRect();
	}

	public FarmerSprite(string texture)
		: base(texture)
	{
		base.interval /= 2f;
		base.SpriteWidth = 16;
		base.SpriteHeight = 32;
		this.UpdateSourceRect();
	}

	public void animate(int whichAnimation, GameTime time)
	{
		this.animate(whichAnimation, time.ElapsedGameTime.Milliseconds);
	}

	public void animate(int whichAnimation, int milliseconds)
	{
		if (!this.PauseForSingleAnimation)
		{
			if (whichAnimation != this.currentSingleAnimation || base.CurrentAnimation == null || base.CurrentAnimation.Count <= 1)
			{
				float oldtimer = base.timer;
				int oldIndex = base.currentAnimationIndex;
				this.currentSingleAnimation = whichAnimation;
				this.setCurrentFrame(whichAnimation);
				base.timer = oldtimer;
				this.CurrentFrame = base.CurrentAnimation[Math.Min(oldIndex, base.CurrentAnimation.Count - 1)].frame;
				base.currentAnimationIndex = oldIndex % base.CurrentAnimation.Count;
				this.UpdateSourceRect();
			}
			this.animate(milliseconds);
		}
	}

	public void checkForSingleAnimation(GameTime time)
	{
		if (this.PauseForSingleAnimation)
		{
			if (!this.animateBackwards)
			{
				this.animateOnce(time);
			}
			else
			{
				this.animateBackwardsOnce(time);
			}
		}
	}

	public void animateOnce(int whichAnimation, float animationInterval, int numberOfFrames)
	{
		this.animateOnce(whichAnimation, animationInterval, numberOfFrames, null);
	}

	public void animateOnce(int whichAnimation, float animationInterval, int numberOfFrames, endOfAnimationBehavior endOfBehaviorFunction)
	{
		this.animateOnce(whichAnimation, animationInterval, numberOfFrames, endOfBehaviorFunction, flip: false, secondaryArm: false);
	}

	public void animateOnce(int whichAnimation, float animationInterval, int numberOfFrames, endOfAnimationBehavior endOfBehaviorFunction, bool flip, bool secondaryArm)
	{
		this.animateOnce(whichAnimation, animationInterval, numberOfFrames, endOfBehaviorFunction, flip, secondaryArm, backwards: false);
	}

	public void animateOnce(AnimationFrame[] animation, endOfAnimationBehavior endOfBehaviorFunction = null)
	{
		this.currentSingleAnimation = -1;
		this.CurrentFrame = this.currentSingleAnimation;
		this.PauseForSingleAnimation = true;
		base.oldFrame = this.CurrentFrame;
		this.oldInterval = base.interval;
		this.currentSingleAnimationInterval = 100f;
		base.timer = 0f;
		base.currentAnimation.Clear();
		base.currentAnimation.AddRange(animation);
		this.CurrentFrame = base.CurrentAnimation[0].frame;
		this.currentAnimationFrames = base.CurrentAnimation.Count;
		base.currentAnimationIndex = 0;
		base.interval = this.CurrentAnimationFrame.milliseconds;
		this.loopThisAnimation = false;
		base.endOfAnimationFunction = endOfBehaviorFunction;
		if (this.currentAnimationFrames > 0)
		{
			base.CurrentAnimation[0].frameStartBehavior?.Invoke(this.owner);
		}
	}

	public void showFrameUntilDialogueOver(int whichFrame)
	{
		this.freezeUntilDialogueIsOver = true;
		this.setCurrentFrame(whichFrame);
		this.UpdateSourceRect();
	}

	public void animateOnce(int whichAnimation, float animationInterval, int numberOfFrames, endOfAnimationBehavior endOfBehaviorFunction, bool flip, bool secondaryArm, bool backwards)
	{
		if (whichAnimation != this.currentSingleAnimation)
		{
			this.PauseForSingleAnimation = false;
		}
		if (this.PauseForSingleAnimation || this.freezeUntilDialogueIsOver)
		{
			return;
		}
		this.currentSingleAnimation = whichAnimation;
		this.CurrentFrame = this.currentSingleAnimation;
		this.PauseForSingleAnimation = true;
		base.oldFrame = this.CurrentFrame;
		this.oldInterval = base.interval;
		this.currentSingleAnimationInterval = animationInterval;
		base.endOfAnimationFunction = endOfBehaviorFunction;
		base.timer = 0f;
		this.animatingBackwards = false;
		if (backwards)
		{
			this.animatingBackwards = true;
			this.setCurrentFrameBackwards(this.currentSingleAnimation, 0, (int)animationInterval, numberOfFrames, secondaryArm, flip);
		}
		else
		{
			this.setCurrentFrame(this.currentSingleAnimation, 0, (int)animationInterval, numberOfFrames, secondaryArm, flip);
		}
		base.CurrentAnimation[0].frameStartBehavior?.Invoke(this.owner);
		if (this.owner.Stamina <= 0f && this.owner.usingTool.Value)
		{
			for (int i = 0; i < base.CurrentAnimation.Count; i++)
			{
				base.CurrentAnimation[i] = new AnimationFrame(base.CurrentAnimation[i].frame, base.CurrentAnimation[i].milliseconds * 2, base.CurrentAnimation[i].positionOffset, base.CurrentAnimation[i].armOffset, base.CurrentAnimation[i].flip, base.CurrentAnimation[i].frameStartBehavior, base.CurrentAnimation[i].frameEndBehavior, base.CurrentAnimation[i].xOffset);
			}
		}
		this.currentAnimationFrames = base.CurrentAnimation.Count;
		base.currentAnimationIndex = 0;
		base.interval = this.CurrentAnimationFrame.milliseconds;
		if (!this.owner.UsingTool || this.owner.CurrentTool == null)
		{
			return;
		}
		this.CurrentToolIndex = this.owner.CurrentTool.CurrentParentTileIndex;
		if (this.owner.CurrentTool is FishingRod)
		{
			if (this.owner.FacingDirection == 3 || this.owner.FacingDirection == 1)
			{
				this.CurrentToolIndex = 55;
			}
			else
			{
				this.CurrentToolIndex = 48;
			}
		}
	}

	public void animateBackwardsOnce(int whichAnimation, float animationInterval)
	{
		this.animateOnce(whichAnimation, animationInterval, 6, null, flip: false, secondaryArm: false, backwards: true);
	}

	public bool isUsingWeapon()
	{
		if (this.PauseForSingleAnimation)
		{
			if (this.currentSingleAnimation < 232 || this.currentSingleAnimation >= 264)
			{
				if (this.currentSingleAnimation >= 272)
				{
					return this.currentSingleAnimation < 280;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public int getWeaponTypeFromAnimation()
	{
		if (this.currentSingleAnimation >= 272 && this.currentSingleAnimation < 280)
		{
			return 1;
		}
		if (this.currentSingleAnimation >= 232 && this.currentSingleAnimation < 264)
		{
			return 3;
		}
		return -1;
	}

	public bool isOnToolAnimation()
	{
		if (this.PauseForSingleAnimation || this.owner.UsingTool)
		{
			if ((this.currentSingleAnimation < 160 || this.currentSingleAnimation >= 192) && (this.currentSingleAnimation < 232 || this.currentSingleAnimation >= 264))
			{
				if (this.currentSingleAnimation >= 272)
				{
					return this.currentSingleAnimation < 280;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public bool isPassingOut()
	{
		if (this.PauseForSingleAnimation)
		{
			if (this.currentSingleAnimation != 293)
			{
				return this.CurrentFrame == 5;
			}
			return true;
		}
		return false;
	}

	private void doneWithAnimation()
	{
		this.CurrentFrame--;
		base.interval = this.oldInterval;
		if (!Game1.eventUp)
		{
			this.owner.CanMove = true;
			this.owner.Halt();
		}
		this.PauseForSingleAnimation = false;
		this.animatingBackwards = false;
	}

	private void currentAnimationTick()
	{
		if (base.currentAnimationIndex < base.CurrentAnimation.Count)
		{
			if (base.CurrentAnimation[base.currentAnimationIndex].frameEndBehavior != null)
			{
				base.CurrentAnimation[base.currentAnimationIndex].frameEndBehavior(this.owner);
			}
			base.currentAnimationIndex++;
			if (this.loopThisAnimation)
			{
				base.currentAnimationIndex %= base.CurrentAnimation.Count;
			}
			else if (base.currentAnimationIndex >= base.CurrentAnimation.Count)
			{
				this.loopThisAnimation = false;
				return;
			}
			if (base.CurrentAnimation[base.currentAnimationIndex].frameStartBehavior != null)
			{
				base.CurrentAnimation[base.currentAnimationIndex].frameStartBehavior(this.owner);
			}
			if (base.currentAnimationIndex < base.CurrentAnimation?.Count)
			{
				this.currentSingleAnimationInterval = base.CurrentAnimation[base.currentAnimationIndex].milliseconds;
				this.CurrentFrame = base.CurrentAnimation[base.currentAnimationIndex].frame;
				base.interval = base.CurrentAnimation[base.currentAnimationIndex].milliseconds;
			}
			else
			{
				this.owner.completelyStopAnimatingOrDoingAction();
				this.owner.forceCanMove();
			}
		}
	}

	public override void UpdateSourceRect()
	{
		base.SourceRect = new Rectangle(this.CurrentFrame * base.SpriteWidth % 96, this.CurrentFrame * base.SpriteWidth / 96 * base.SpriteHeight, base.SpriteWidth, base.SpriteHeight);
	}

	private new void animateOnce(GameTime time)
	{
		if (this.freezeUntilDialogueIsOver || this.owner == null)
		{
			return;
		}
		base.timer += (float)time.ElapsedGameTime.TotalMilliseconds;
		if (base.timer > base.interval * this.intervalModifier)
		{
			this.currentAnimationTick();
			base.timer = 0f;
			if (base.currentAnimationIndex > this.currentAnimationFrames - 1)
			{
				this.CurrentAnimationFrame.frameEndBehavior?.Invoke(this.owner);
				if (base.endOfAnimationFunction != null)
				{
					endOfAnimationBehavior endOfAnimationBehavior = base.endOfAnimationFunction;
					base.endOfAnimationFunction = null;
					endOfAnimationBehavior(this.owner);
					if (!(this.owner.CurrentTool is MeleeWeapon weapon) || weapon.type.Value != 1)
					{
						this.doneWithAnimation();
					}
					return;
				}
				this.doneWithAnimation();
				if (this.owner.isEating)
				{
					this.owner.doneEating();
				}
			}
			switch (this.currentSingleAnimation)
			{
			case 160:
			case 161:
			case 165:
				this.owner.CurrentTool?.Update(2, base.currentAnimationIndex, this.owner);
				break;
			case 176:
			case 180:
			case 181:
				this.owner.CurrentTool?.Update(0, base.currentAnimationIndex, this.owner);
				break;
			}
			if (this.CurrentFrame == 109 && this.owner.ShouldHandleAnimationSound())
			{
				this.owner.playNearbySoundLocal("eat");
			}
			if (this.isOnToolAnimation() && !this.isUsingWeapon() && base.currentAnimationIndex == 4 && this.currentToolIndex % 2 == 0 && !(this.owner.CurrentTool is FishingRod))
			{
				this.currentToolIndex++;
			}
		}
		this.UpdateSourceRect();
	}

	private void checkForFootstep()
	{
		if (Game1.player.isRidingHorse() || this.owner == null || this.owner.currentLocation != Game1.currentLocation)
		{
			return;
		}
		Vector2 tileLocationOfPlayer = this.owner?.Tile ?? Game1.player.Tile;
		if (Game1.currentLocation.IsOutdoors || Game1.currentLocation.Name.ContainsIgnoreCase("mine") || Game1.currentLocation.Name.ContainsIgnoreCase("cave") || Game1.currentLocation.IsGreenhouse)
		{
			string stepType = Game1.currentLocation.doesTileHaveProperty((int)tileLocationOfPlayer.X, (int)tileLocationOfPlayer.Y, "Type", "Buildings");
			if (string.IsNullOrEmpty(stepType))
			{
				stepType = Game1.currentLocation.doesTileHaveProperty((int)tileLocationOfPlayer.X, (int)tileLocationOfPlayer.Y, "Type", "Back");
			}
			switch (stepType)
			{
			case "Dirt":
				this.currentStep = "sandyStep";
				break;
			case "Stone":
				this.currentStep = "stoneStep";
				break;
			case "Grass":
				this.currentStep = ((Game1.currentLocation.GetSeason() == Season.Winter) ? "snowyStep" : "grassyStep");
				break;
			case "Wood":
				this.currentStep = "woodyStep";
				break;
			}
		}
		else
		{
			this.currentStep = "thudStep";
		}
		if (((this.currentSingleAnimation >= 32 && this.currentSingleAnimation <= 56) || (this.currentSingleAnimation >= 128 && this.currentSingleAnimation <= 152)) && base.currentAnimationIndex % 4 == 0)
		{
			string played_step = this.currentStep;
			played_step = this.owner.currentLocation.getFootstepSoundReplacement(played_step);
			if (this.owner.onBridge.Value)
			{
				if (this.owner.currentLocation == Game1.currentLocation && Utility.isOnScreen(this.owner.Position, 384))
				{
					played_step = "thudStep";
				}
				this.owner.bridge?.OnFootstep(this.owner.Position);
			}
			if (Game1.currentLocation.terrainFeatures.TryGetValue(tileLocationOfPlayer, out var terrainFeature) && terrainFeature is Flooring flooring)
			{
				played_step = flooring.getFootstepSound();
			}
			Vector2 owner_position = this.owner.Position;
			if (this.owner.shouldShadowBeOffset)
			{
				owner_position += this.owner.drawOffset;
			}
			if (!(played_step == "sandyStep"))
			{
				if (played_step == "snowyStep")
				{
					TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(owner_position.X + 24f + (float)(Game1.random.Next(-4, 4) * 4), owner_position.Y + 8f + (float)(Game1.random.Next(-4, 4) * 4)), flicker: false, flipped: false, owner_position.Y / 10000000f, 0.01f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, (this.owner.FacingDirection == 1 || this.owner.FacingDirection == 3) ? (-(float)Math.PI / 4f) : 0f, 0f);
					Game1.currentLocation.temporarySprites.Add(sprite);
				}
			}
			else
			{
				TemporaryAnimatedSprite sprite2 = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(128, 2948, 64, 64), 80f, 8, 0, new Vector2(owner_position.X + 16f + (float)Game1.random.Next(-8, 8), owner_position.Y + (float)(Game1.random.Next(-3, -1) * 4)), flicker: false, Game1.random.NextBool(), owner_position.Y / 10000f, 0.03f, Color.Khaki * 0.45f, 0.75f + (float)Game1.random.Next(-3, 4) * 0.05f, 0f, 0f, 0f);
				Game1.currentLocation.temporarySprites.Add(sprite2);
				sprite2 = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(128, 2948, 64, 64), 80f, 8, 0, new Vector2(owner_position.X + 16f + (float)Game1.random.Next(-4, 4), owner_position.Y + (float)(Game1.random.Next(-3, -1) * 4)), flicker: false, Game1.random.NextBool(), owner_position.Y / 10000f, 0.03f, Color.Khaki * 0.45f, 0.55f + (float)Game1.random.Next(-3, 4) * 0.05f, 0f, 0f, 0f);
				sprite2.delayBeforeAnimationStart = 20;
				Game1.currentLocation.temporarySprites.Add(sprite2);
			}
			if (played_step != null && this.owner.currentLocation == Game1.currentLocation && Utility.isOnScreen(this.owner.Position, 384) && (this.owner == Game1.player || !LocalMultiplayer.IsLocalMultiplayer(is_local_only: true)))
			{
				Game1.playSound(played_step);
				if (this.owner.boots.Value != null && this.owner.boots.Value.ItemId == "853")
				{
					Game1.playSound("jingleBell");
				}
			}
			foreach (Trinket trinketItem in this.owner.trinketItems)
			{
				trinketItem?.OnFootstep(this.owner);
			}
			if (this.owner.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID)
			{
				Game1.stats.takeStep();
			}
		}
		else
		{
			if ((this.currentSingleAnimation < 0 || this.currentSingleAnimation > 24) && (this.currentSingleAnimation < 96 || this.currentSingleAnimation > 120))
			{
				return;
			}
			if (this.owner.onBridge.Value && base.currentAnimationIndex % 2 == 0)
			{
				if (this.owner.currentLocation == Game1.currentLocation && Utility.isOnScreen(this.owner.Position, 384) && (this.owner == Game1.player || !LocalMultiplayer.IsLocalMultiplayer(is_local_only: true)))
				{
					Game1.playSound("thudStep");
				}
				this.owner.bridge?.OnFootstep(this.owner.Position);
				foreach (Trinket trinketItem2 in this.owner.trinketItems)
				{
					trinketItem2?.OnFootstep(this.owner);
				}
			}
			if (base.currentAnimationIndex == 0 && this.owner.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID)
			{
				Game1.stats.takeStep();
			}
		}
	}

	private void animateBackwardsOnce(GameTime time)
	{
		base.timer += (float)time.ElapsedGameTime.TotalMilliseconds;
		if (base.timer > this.currentSingleAnimationInterval)
		{
			this.CurrentFrame--;
			base.timer = 0f;
			if (base.currentAnimationIndex > this.currentAnimationFrames - 1)
			{
				if (this.CurrentFrame < 63 || this.CurrentFrame > 96)
				{
					this.CurrentFrame = base.oldFrame;
				}
				else
				{
					this.CurrentFrame = this.CurrentFrame % 16 + 8;
				}
				base.interval = this.oldInterval;
				this.PauseForSingleAnimation = false;
				this.animatingBackwards = false;
				if (!Game1.eventUp)
				{
					this.owner.CanMove = true;
				}
				this.owner.Halt();
				if ((this.CurrentSingleAnimation >= 160 && this.CurrentSingleAnimation < 192) || (this.CurrentSingleAnimation >= 200 && this.CurrentSingleAnimation < 216) || (this.CurrentSingleAnimation >= 232 && this.CurrentSingleAnimation < 264))
				{
					Game1.toolAnimationDone(this.owner);
				}
			}
		}
		this.UpdateSourceRect();
	}

	public void setCurrentSingleAnimation(int which)
	{
		this.CurrentFrame = which;
		this.currentSingleAnimation = which;
		this.getAnimationFromIndex(which, this, 100, 1, flip: false, secondaryArm: false);
		List<AnimationFrame> list = base.CurrentAnimation;
		if (list != null && list.Count > 0)
		{
			this.currentAnimationFrames = base.CurrentAnimation.Count;
			AnimationFrame frame = base.CurrentAnimation[0];
			base.interval = frame.milliseconds;
			this.CurrentFrame = frame.frame;
		}
		if (base.interval <= 50f)
		{
			base.interval = 800f;
		}
		this.UpdateSourceRect();
	}

	private void animate(int Milliseconds)
	{
		base.timer += Milliseconds;
		if (base.timer > base.interval * this.intervalModifier)
		{
			this.currentAnimationTick();
			base.timer = 0f;
			this.checkForFootstep();
		}
		this.UpdateSourceRect();
	}

	public override void StopAnimation()
	{
		bool animation_dirty = false;
		if (this.pauseForSingleAnimation)
		{
			return;
		}
		base.interval = 0f;
		if (this.CurrentFrame >= 64 && this.CurrentFrame <= 155 && this.owner != null && !this.owner.bathingClothes.Value)
		{
			switch (this.owner.FacingDirection)
			{
			case 0:
				this.CurrentFrame = 12;
				break;
			case 1:
				this.CurrentFrame = 6;
				break;
			case 2:
				this.CurrentFrame = 0;
				break;
			case 3:
				this.CurrentFrame = 6;
				break;
			}
			animation_dirty = true;
		}
		else if (this.owner != null)
		{
			bool carrying = this.owner.ActiveObject != null && this.owner.ActiveObject.IsHeldOverHead() && Game1.eventUp;
			if (!this.IsPlayingBasicAnimation(this.owner.FacingDirection, carrying))
			{
				animation_dirty = true;
				switch (this.owner.FacingDirection)
				{
				case 0:
					if (this.owner.ActiveObject != null && !Game1.eventUp)
					{
						this.setCurrentFrame(112, 1);
					}
					else
					{
						this.setCurrentFrame(16, 1);
					}
					break;
				case 1:
					if (this.owner.ActiveObject != null && !Game1.eventUp)
					{
						this.setCurrentFrame(104, 1);
					}
					else
					{
						this.setCurrentFrame(8, 1);
					}
					break;
				case 2:
					if (this.owner.ActiveObject != null && !Game1.eventUp)
					{
						this.setCurrentFrame(96, 1);
					}
					else
					{
						this.setCurrentFrame(0, 1);
					}
					break;
				case 3:
					if (this.owner.ActiveObject != null && !Game1.eventUp)
					{
						this.setCurrentFrame(120, 1);
					}
					else
					{
						this.setCurrentFrame(24, 1);
					}
					break;
				}
				this.currentSingleAnimation = -1;
			}
		}
		if (animation_dirty)
		{
			base.currentAnimationIndex = 0;
			this.UpdateSourceRect();
		}
	}

	public virtual void getAnimationFromIndex(int index, FarmerSprite requester, int interval, int numberOfFrames, bool flip, bool secondaryArm)
	{
		bool showCarryingArm = (index >= 96 && index < 160) || index == 232 || index == 248;
		if (requester.owner?.ActiveObject != null && !requester.owner.ActiveObject.IsHeldOverHead())
		{
			showCarryingArm = false;
		}
		requester.loopThisAnimation = true;
		int frameOffset = 0;
		if (requester.owner != null && requester.owner.bathingClothes.Value)
		{
			frameOffset += 108;
		}
		List<AnimationFrame> outFrames = requester.currentAnimation;
		outFrames.Clear();
		float toolSpeedModifier = 1f;
		if (requester.owner?.CurrentTool != null)
		{
			toolSpeedModifier = requester.owner.CurrentTool.AnimationSpeedModifier;
		}
		requester.currentSingleAnimation = index;
		bool inBathingClothes = requester.owner?.bathingClothes.Value ?? false;
		switch (index)
		{
		case -1:
			outFrames.Add(new AnimationFrame(0, 100, showCarryingArm, flip: false));
			return;
		case 0:
		case 96:
			outFrames.Add(new AnimationFrame(1 + frameOffset, 200, showCarryingArm, flip: false, inBathingClothes));
			outFrames.Add(new AnimationFrame(frameOffset, 200, showCarryingArm, flip: false, inBathingClothes));
			outFrames.Add(new AnimationFrame(2 + frameOffset, 200, showCarryingArm, flip: false, inBathingClothes));
			outFrames.Add(new AnimationFrame(frameOffset, 200, showCarryingArm, flip: false, inBathingClothes));
			return;
		case 8:
		case 104:
			outFrames.Add(new AnimationFrame(7 + frameOffset, 200, showCarryingArm, flip: false, inBathingClothes));
			outFrames.Add(new AnimationFrame(6 + frameOffset, 200, showCarryingArm, flip: false, inBathingClothes));
			outFrames.Add(new AnimationFrame(8 + frameOffset, 200, showCarryingArm, flip: false, inBathingClothes));
			outFrames.Add(new AnimationFrame(6 + frameOffset, 200, showCarryingArm, flip: false, inBathingClothes));
			return;
		case 16:
		case 112:
			outFrames.Add(new AnimationFrame(13 + frameOffset, 200, showCarryingArm, flip: false, inBathingClothes));
			outFrames.Add(new AnimationFrame(12 + frameOffset, 200, showCarryingArm, flip: false, inBathingClothes));
			outFrames.Add(new AnimationFrame(14 + frameOffset, 200, showCarryingArm, flip: false, inBathingClothes));
			outFrames.Add(new AnimationFrame(12 + frameOffset, 200, showCarryingArm, flip: false, inBathingClothes));
			return;
		case 24:
		case 120:
			outFrames.Add(new AnimationFrame(7 + frameOffset, 200, showCarryingArm, flip: true, inBathingClothes));
			outFrames.Add(new AnimationFrame(6 + frameOffset, 200, showCarryingArm, flip: true, inBathingClothes));
			outFrames.Add(new AnimationFrame(8 + frameOffset, 200, showCarryingArm, flip: true, inBathingClothes));
			outFrames.Add(new AnimationFrame(6 + frameOffset, 200, showCarryingArm, flip: true, inBathingClothes));
			return;
		case 32:
		case 128:
			outFrames.Add(new AnimationFrame(0, 90, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(1, 60, -2, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(18, 120, -4, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(1, 60, -2, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(0, 90, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(2, 60, -2, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(19, 120, -4, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(2, 60, -2, showCarryingArm, flip: false));
			return;
		case 40:
		case 136:
			outFrames.Add(new AnimationFrame(6, 80, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(6, 10, -1, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(20, 140, -2, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(11, 100, 0, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(6, 80, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(6, 10, -1, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(21, 140, -2, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(17, 100, 0, showCarryingArm, flip: false));
			return;
		case 48:
		case 144:
			outFrames.Add(new AnimationFrame(12, 90, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(13, 60, -2, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(22, 120, -3, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(13, 60, -2, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(12, 90, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(14, 60, -2, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(23, 120, -3, showCarryingArm, flip: false));
			outFrames.Add(new AnimationFrame(14, 60, -2, showCarryingArm, flip: false));
			return;
		case 56:
		case 152:
			outFrames.Add(new AnimationFrame(6, 80, showCarryingArm, flip: true));
			outFrames.Add(new AnimationFrame(6, 10, -1, showCarryingArm, flip: true));
			outFrames.Add(new AnimationFrame(20, 140, -2, showCarryingArm, flip: true));
			outFrames.Add(new AnimationFrame(11, 100, 0, showCarryingArm, flip: true));
			outFrames.Add(new AnimationFrame(6, 80, showCarryingArm, flip: true));
			outFrames.Add(new AnimationFrame(6, 10, -1, showCarryingArm, flip: true));
			outFrames.Add(new AnimationFrame(21, 140, -2, showCarryingArm, flip: true));
			outFrames.Add(new AnimationFrame(17, 100, 0, showCarryingArm, flip: true));
			return;
		case 232:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(24, 55, showCarryingArm, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(25, 45, showCarryingArm, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(26, 25, showCarryingArm, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(27, 25, showCarryingArm, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(28, 25, showCarryingArm, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(29, (short)interval * 2, showCarryingArm, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(29, 0, showCarryingArm, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
			return;
		case 160:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(66, (int)(150f * toolSpeedModifier), secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(67, (int)(40f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
			outFrames.Add(new AnimationFrame(68, (int)(40f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.useTool));
			outFrames.Add(new AnimationFrame(69, (short)((float)(170 + requester.owner.toolPower.Value * 30) * toolSpeedModifier), secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(70, (int)(75f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
			return;
		case 297:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(66, 100, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(67, 40, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(68, 40, secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
			outFrames.Add(new AnimationFrame(69, 80, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(70, 200, secondaryArm: false, flip: false, FishingRod.doneWithCastingAnimation, behaviorAtEndOfFrame: true));
			return;
		case 301:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(74, 5000, secondaryArm: false, flip: false));
			return;
		case 240:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(30, 55, secondaryArm: true, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(31, 45, secondaryArm: true, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(32, 25, secondaryArm: true, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(33, 25, secondaryArm: true, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(34, 25, secondaryArm: true, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(35, (short)interval * 2, secondaryArm: true, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(35, 0, secondaryArm: true, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
			return;
		case 168:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(48, (int)(100f * toolSpeedModifier), secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(49, (int)(40f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
			outFrames.Add(new AnimationFrame(50, (int)(40f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.useTool));
			outFrames.Add(new AnimationFrame(51, (short)((float)(220 + requester.owner.toolPower.Value * 30) * toolSpeedModifier), secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(52, (int)(75f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
			return;
		case 296:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(48, 100, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(49, 40, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(50, 40, secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
			outFrames.Add(new AnimationFrame(51, 80, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(52, 200, secondaryArm: false, flip: false, FishingRod.doneWithCastingAnimation, behaviorAtEndOfFrame: true));
			return;
		case 300:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(72, 5000, secondaryArm: false, flip: false));
			return;
		case 248:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(36, 55, showCarryingArm, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(37, 45, showCarryingArm, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(38, 25, showCarryingArm, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(39, 25, showCarryingArm, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(40, 25, showCarryingArm, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(41, (short)interval * 2, showCarryingArm, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(41, 0, showCarryingArm, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
			return;
		case 176:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(36, (int)(100f * toolSpeedModifier), secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(37, (int)(40f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
			outFrames.Add(new AnimationFrame(38, (int)(40f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.useTool));
			outFrames.Add(new AnimationFrame(63, (short)((float)(220 + requester.owner.toolPower.Value * 30) * toolSpeedModifier), secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(62, (int)(75f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
			return;
		case 295:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(76, 100, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(38, 40, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(63, 40, secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
			outFrames.Add(new AnimationFrame(62, 80, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(63, 200, secondaryArm: false, flip: false, FishingRod.doneWithCastingAnimation, behaviorAtEndOfFrame: true));
			return;
		case 299:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(76, 5000, secondaryArm: false, flip: false));
			return;
		case 256:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(30, 55, secondaryArm: true, flip: true, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(31, 45, secondaryArm: true, flip: true, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(32, 25, secondaryArm: true, flip: true, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(33, 25, secondaryArm: true, flip: true, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(34, 25, secondaryArm: true, flip: true, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(35, (short)interval * 2, secondaryArm: true, flip: true, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(35, 0, secondaryArm: true, flip: true, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
			return;
		case 184:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(48, (int)(100f * toolSpeedModifier), secondaryArm: false, flip: true));
			outFrames.Add(new AnimationFrame(49, (int)(40f * toolSpeedModifier), secondaryArm: false, flip: true, Farmer.showToolSwipeEffect));
			outFrames.Add(new AnimationFrame(50, (int)(40f * toolSpeedModifier), secondaryArm: false, flip: true, Farmer.useTool));
			outFrames.Add(new AnimationFrame(51, (short)((float)(220 + requester.owner.toolPower.Value * 30) * toolSpeedModifier), secondaryArm: false, flip: true));
			outFrames.Add(new AnimationFrame(52, (int)(75f * toolSpeedModifier), secondaryArm: false, flip: true, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
			return;
		case 298:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(48, 100, secondaryArm: false, flip: true));
			outFrames.Add(new AnimationFrame(49, 40, secondaryArm: false, flip: true));
			outFrames.Add(new AnimationFrame(50, 40, secondaryArm: false, flip: true, Farmer.showToolSwipeEffect));
			outFrames.Add(new AnimationFrame(51, 80, secondaryArm: false, flip: true));
			outFrames.Add(new AnimationFrame(52, 200, secondaryArm: false, flip: true, FishingRod.doneWithCastingAnimation, behaviorAtEndOfFrame: true));
			return;
		case 302:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(72, 5000, secondaryArm: false, flip: true));
			return;
		case 234:
			index = 28;
			secondaryArm = true;
			break;
		case 258:
		case 259:
			index = 34;
			flip = true;
			break;
		case 242:
		case 243:
			index = 34;
			break;
		case 252:
			index = 40;
			secondaryArm = true;
			break;
		case 272:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(25, (short)interval, secondaryArm: true, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(27, (short)interval, secondaryArm: true, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(27, 0, secondaryArm: true, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
			return;
		case 274:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(34, (short)interval, secondaryArm: false, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(33, (short)interval, secondaryArm: false, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(33, 0, secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
			return;
		case 276:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(40, (short)interval, secondaryArm: true, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(38, (short)interval, secondaryArm: true, flip: false, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(38, 0, secondaryArm: true, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
			return;
		case 278:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(34, (short)interval, secondaryArm: false, flip: true, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(33, (short)interval, secondaryArm: false, flip: true, this.owner.showSwordSwipe));
			outFrames.Add(new AnimationFrame(33, 0, secondaryArm: false, flip: true, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
			return;
		case 192:
			index = 3;
			interval = 500;
			break;
		case 194:
			index = 9;
			interval = 500;
			break;
		case 196:
			index = 15;
			interval = 500;
			break;
		case 198:
			index = 9;
			flip = true;
			interval = 500;
			break;
		case 180:
		case 182:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(62, 0, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(62, 75, secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
			outFrames.Add(new AnimationFrame(63, 100, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
			outFrames.Add(new AnimationFrame(46, 500, secondaryArm: true, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
			return;
		case 172:
		case 174:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(58, 0, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(58, 75, secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
			outFrames.Add(new AnimationFrame(59, 100, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
			outFrames.Add(new AnimationFrame(45, 500, secondaryArm: true, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
			return;
		case 164:
		case 166:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(54, 0, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(54, 75, secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
			outFrames.Add(new AnimationFrame(55, 100, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
			outFrames.Add(new AnimationFrame(25, 500, secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
			return;
		case 188:
		case 190:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(58, 0, secondaryArm: false, flip: true));
			outFrames.Add(new AnimationFrame(58, 75, secondaryArm: false, flip: true, Farmer.showToolSwipeEffect));
			outFrames.Add(new AnimationFrame(59, 100, secondaryArm: false, flip: true, Farmer.useTool, behaviorAtEndOfFrame: true));
			outFrames.Add(new AnimationFrame(45, 500, secondaryArm: true, flip: true, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
			return;
		case 80:
		case 87:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(12, 0, secondaryArm: false, flip: false));
			return;
		case 72:
		case 79:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(6, 0, secondaryArm: false, flip: false));
			return;
		case 64:
		case 71:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(0, 0, secondaryArm: false, flip: false));
			return;
		case 88:
		case 95:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(6, 0, secondaryArm: false, flip: true));
			return;
		case 281:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(54, 0, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(54, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
			outFrames.Add(new AnimationFrame(55, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
			outFrames.Add(new AnimationFrame(56, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
			outFrames.Add(new AnimationFrame(57, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
			outFrames.Add(new AnimationFrame(57, 0, secondaryArm: false, flip: false, Farmer.showItemIntake));
			return;
		case 280:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(58, 0, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(58, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
			outFrames.Add(new AnimationFrame(59, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
			outFrames.Add(new AnimationFrame(60, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
			outFrames.Add(new AnimationFrame(61, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
			outFrames.Add(new AnimationFrame(61, 0, secondaryArm: false, flip: false, Farmer.showItemIntake));
			return;
		case 279:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(62, 0, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(62, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
			outFrames.Add(new AnimationFrame(63, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
			outFrames.Add(new AnimationFrame(64, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
			outFrames.Add(new AnimationFrame(65, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
			outFrames.Add(new AnimationFrame(65, 0, secondaryArm: false, flip: false, Farmer.showItemIntake));
			return;
		case 282:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(58, 0, secondaryArm: false, flip: true));
			outFrames.Add(new AnimationFrame(58, 100, secondaryArm: false, flip: true, Farmer.showItemIntake));
			outFrames.Add(new AnimationFrame(59, 100, secondaryArm: false, flip: true, Farmer.showItemIntake));
			outFrames.Add(new AnimationFrame(60, 100, secondaryArm: false, flip: true, Farmer.showItemIntake));
			outFrames.Add(new AnimationFrame(61, 100, secondaryArm: false, flip: true, Farmer.showItemIntake));
			outFrames.Add(new AnimationFrame(61, 0, secondaryArm: false, flip: true, Farmer.showItemIntake));
			return;
		case 283:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(82, 400));
			outFrames.Add(new AnimationFrame(83, 400, secondaryArm: false, flip: false, Shears.playSnip));
			outFrames.Add(new AnimationFrame(82, 400));
			outFrames.Add(new AnimationFrame(83, 400, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
			return;
		case 284:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(80, 400));
			outFrames.Add(new AnimationFrame(81, 400, secondaryArm: false, flip: false, Shears.playSnip));
			outFrames.Add(new AnimationFrame(80, 400));
			outFrames.Add(new AnimationFrame(81, 400, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
			return;
		case 285:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(78, 400));
			outFrames.Add(new AnimationFrame(79, 400, secondaryArm: false, flip: false, Shears.playSnip));
			outFrames.Add(new AnimationFrame(78, 400));
			outFrames.Add(new AnimationFrame(79, 400, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
			return;
		case 286:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(80, 400, secondaryArm: false, flip: true));
			outFrames.Add(new AnimationFrame(81, 400, secondaryArm: false, flip: true, Shears.playSnip));
			outFrames.Add(new AnimationFrame(80, 400, secondaryArm: false, flip: true));
			outFrames.Add(new AnimationFrame(81, 400, secondaryArm: false, flip: true, Farmer.useTool, behaviorAtEndOfFrame: true));
			return;
		case 287:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(62, 400));
			outFrames.Add(new AnimationFrame(63, 400));
			outFrames.Add(new AnimationFrame(62, 400));
			outFrames.Add(new AnimationFrame(63, 400, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
			return;
		case 288:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(58, 400));
			outFrames.Add(new AnimationFrame(59, 400));
			outFrames.Add(new AnimationFrame(58, 400));
			outFrames.Add(new AnimationFrame(59, 400, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
			return;
		case 289:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(54, 400));
			outFrames.Add(new AnimationFrame(55, 400));
			outFrames.Add(new AnimationFrame(54, 400));
			outFrames.Add(new AnimationFrame(55, 400, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
			return;
		case 290:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(58, 400, secondaryArm: false, flip: true));
			outFrames.Add(new AnimationFrame(59, 400, secondaryArm: false, flip: true));
			outFrames.Add(new AnimationFrame(58, 400, secondaryArm: false, flip: true));
			outFrames.Add(new AnimationFrame(59, 400, secondaryArm: false, flip: true, Farmer.useTool, behaviorAtEndOfFrame: true));
			return;
		case 216:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(0, 0));
			outFrames.Add(new AnimationFrame(84, (requester.owner?.mostRecentlyGrabbedItem?.QualifiedItemId == "(O)434") ? 1000 : 250, secondaryArm: false, flip: false, Farmer.showEatingItem));
			outFrames.Add(new AnimationFrame(85, 400, secondaryArm: false, flip: false, Farmer.showEatingItem));
			outFrames.Add(new AnimationFrame(86, 1, secondaryArm: false, flip: false, Farmer.showEatingItem, behaviorAtEndOfFrame: true));
			outFrames.Add(new AnimationFrame(86, 400, secondaryArm: false, flip: false, Farmer.showEatingItem, behaviorAtEndOfFrame: true));
			outFrames.Add(new AnimationFrame(87, 250, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(88, 250, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(87, 250, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(88, 250, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(87, 250, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(0, 250, secondaryArm: false, flip: false, Farmer.showEatingItem));
			return;
		case 304:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(84, 99999999, secondaryArm: false, flip: false, Farmer.showEatingItem));
			return;
		case 294:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(0, 1));
			outFrames.Add(new AnimationFrame(90, 250));
			outFrames.Add(new AnimationFrame(91, 150));
			outFrames.Add(new AnimationFrame(92, 250, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(93, 200, secondaryArm: false, flip: false, Farmer.drinkGlug));
			outFrames.Add(new AnimationFrame(92, 250, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(93, 200, secondaryArm: false, flip: false, Farmer.drinkGlug));
			outFrames.Add(new AnimationFrame(92, 250, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(93, 200, secondaryArm: false, flip: false, Farmer.drinkGlug));
			outFrames.Add(new AnimationFrame(91, 250));
			outFrames.Add(new AnimationFrame(90, 50));
			return;
		case 224:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(104, 350, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(105, 350, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(104, 350, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(105, 350, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(104, 350, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(105, 350, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(104, 350, secondaryArm: false, flip: false));
			outFrames.Add(new AnimationFrame(105, 350, secondaryArm: false, flip: false));
			return;
		case 83:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(0, 0, secondaryArm: false, flip: false));
			return;
		case 43:
			flip = requester.owner.FacingDirection == 3;
			break;
		case 999996:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(96, 800, secondaryArm: false, flip: false));
			return;
		case 97:
			requester.loopThisAnimation = false;
			flip = requester.owner.FacingDirection == 3;
			outFrames.Add(new AnimationFrame(97, 800, secondaryArm: false, flip));
			return;
		case 303:
		{
			int armOffset = Math.Max(3, 3 * requester.owner.CurrentTool.UpgradeLevel);
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(123, 150, armOffset, flip: true));
			outFrames.Add(new AnimationFrame(124, 150, 0, armOffset, flip: true, Pan.playSlosh, null, 0));
			outFrames.Add(new AnimationFrame(123, 150, armOffset, flip: true));
			outFrames.Add(new AnimationFrame(125, 150, armOffset, flip: true));
			outFrames.Add(new AnimationFrame(123, 150, armOffset, flip: true));
			outFrames.Add(new AnimationFrame(124, 150, 0, armOffset, flip: true, Pan.playSlosh, null, 0));
			outFrames.Add(new AnimationFrame(123, 150, armOffset, flip: true));
			outFrames.Add(new AnimationFrame(125, 150, armOffset, flip: true));
			outFrames.Add(new AnimationFrame(123, 150, armOffset, flip: true));
			outFrames.Add(new AnimationFrame(124, 150, 0, armOffset, flip: true, Pan.playSlosh, null, 0));
			outFrames.Add(new AnimationFrame(123, 150, armOffset, flip: true));
			outFrames.Add(new AnimationFrame(125, 150, armOffset, flip: true));
			outFrames.Add(new AnimationFrame(123, 150, armOffset, flip: true));
			outFrames.Add(new AnimationFrame(124, 150, 0, armOffset, flip: true, Pan.playSlosh, null, 0));
			outFrames.Add(new AnimationFrame(123, 500, 0, armOffset, flip: true, null, Farmer.useTool, 0));
			return;
		}
		case 291:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(16, 1500));
			outFrames.Add(new AnimationFrame(16, 1, secondaryArm: false, flip: false, Farmer.completelyStopAnimating));
			return;
		case 292:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(16, 500));
			outFrames.Add(new AnimationFrame(0, 500));
			outFrames.Add(new AnimationFrame(16, 500));
			outFrames.Add(new AnimationFrame(0, 500));
			outFrames.Add(new AnimationFrame(0, 1, secondaryArm: false, flip: false, Farmer.completelyStopAnimating));
			return;
		case 293:
			requester.loopThisAnimation = false;
			outFrames.Add(new AnimationFrame(16, 1000));
			outFrames.Add(new AnimationFrame(0, 500));
			outFrames.Add(new AnimationFrame(16, 1000));
			outFrames.Add(new AnimationFrame(4, 200));
			outFrames.Add(new AnimationFrame(5, 2000, secondaryArm: false, flip: false, Farmer.doSleepEmote));
			outFrames.Add(new AnimationFrame(5, 2000, secondaryArm: false, flip: false, Farmer.passOutFromTired));
			outFrames.Add(new AnimationFrame(5, 2000));
			return;
		}
		if (index > FarmerRenderer.featureYOffsetPerFrame.Length - 1)
		{
			index = 0;
		}
		requester.loopThisAnimation = false;
		for (int i = 0; i < numberOfFrames; i++)
		{
			outFrames.Add(new AnimationFrame((short)(i + index), (short)interval, secondaryArm, flip));
		}
	}
}
