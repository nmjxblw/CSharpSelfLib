using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley;

public class AnimatedSprite : INetObject<NetFields>
{
	public delegate void endOfAnimationBehavior(Farmer who);

	public Texture2D spriteTexture;

	/// <summary>The texture asset name that was loaded, usually matching <see cref="F:StardewValley.AnimatedSprite.textureName" /> or <see cref="F:StardewValley.AnimatedSprite.overrideTextureName" />.</summary>
	public string loadedTexture;

	/// <summary>The texture asset name to load locally regardless of <see cref="F:StardewValley.AnimatedSprite.textureName" />, if set.</summary>
	/// <remarks>See <see cref="F:StardewValley.AnimatedSprite.loadedTexture" /> for the texture that was actually loaded.</remarks>
	public string overrideTextureName;

	/// <summary>The texture asset name to load if not overridden by <see cref="F:StardewValley.AnimatedSprite.overrideTextureName" />.</summary>
	/// <remarks>See <see cref="F:StardewValley.AnimatedSprite.loadedTexture" /> for the texture that was actually loaded.</remarks>
	public readonly NetString textureName = new NetString();

	public float timer;

	public float interval = 175f;

	public int framesPerAnimation = 4;

	public int currentFrame;

	public readonly NetInt spriteWidth = new NetInt(16);

	public readonly NetInt spriteHeight = new NetInt(24);

	public int tempSpriteHeight = -1;

	public Rectangle sourceRect;

	public bool loop = true;

	public bool ignoreStopAnimation;

	public bool textureUsesFlippedRightForLeft;

	public endOfAnimationBehavior endOfAnimationFunction;

	public readonly List<FarmerSprite.AnimationFrame> currentAnimation = new List<FarmerSprite.AnimationFrame>(12);

	public int oldFrame;

	public int currentAnimationIndex;

	protected ContentManager contentManager;

	public bool ignoreSourceRectUpdates;

	public NetFields NetFields { get; } = new NetFields("AnimatedSprite");

	public Texture2D Texture
	{
		get
		{
			this.loadTexture();
			return this.spriteTexture;
		}
	}

	protected int textureWidth => this.Texture?.Width ?? 96;

	protected int textureHeight => this.Texture?.Height ?? 128;

	public int SpriteWidth
	{
		get
		{
			return this.spriteWidth.Get();
		}
		set
		{
			this.spriteWidth.Value = value;
		}
	}

	public int SpriteHeight
	{
		get
		{
			if (this.tempSpriteHeight != -1)
			{
				return this.tempSpriteHeight;
			}
			return this.spriteHeight.Get();
		}
		set
		{
			this.spriteHeight.Value = value;
			this.tempSpriteHeight = -1;
		}
	}

	public virtual int CurrentFrame
	{
		get
		{
			return this.currentFrame;
		}
		set
		{
			this.currentFrame = value;
			this.UpdateSourceRect();
		}
	}

	public List<FarmerSprite.AnimationFrame> CurrentAnimation
	{
		get
		{
			if (this.currentAnimation.Count == 0)
			{
				return null;
			}
			return this.currentAnimation;
		}
		set
		{
			this.currentAnimation.Clear();
			if (value != null)
			{
				this.currentAnimation.AddRange(value);
			}
		}
	}

	public Rectangle SourceRect
	{
		get
		{
			return this.sourceRect;
		}
		set
		{
			this.sourceRect = value;
		}
	}

	/// <summary>The character which uses this sprite.</summary>
	public virtual Character Owner { get; protected set; }

	public AnimatedSprite()
	{
		this.initNetFields();
		this.contentManager = Game1.content;
	}

	public AnimatedSprite(ContentManager contentManager, string textureName, int currentFrame, int spriteWidth, int spriteHeight)
		: this()
	{
		this.contentManager = contentManager;
		this.currentFrame = currentFrame;
		this.SpriteWidth = spriteWidth;
		this.SpriteHeight = spriteHeight;
		this.LoadTexture(textureName);
	}

	public AnimatedSprite(ContentManager contentManager, string textureName)
		: this()
	{
		this.contentManager = contentManager;
		this.LoadTexture(textureName);
	}

	public AnimatedSprite(string textureName, int currentFrame, int spriteWidth, int spriteHeight)
		: this(Game1.content, textureName, currentFrame, spriteWidth, spriteHeight)
	{
	}

	public AnimatedSprite(string textureName)
		: this(Game1.content, textureName)
	{
	}

	protected virtual void initNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.textureName, "textureName").AddField(this.spriteWidth, "spriteWidth")
			.AddField(this.spriteHeight, "spriteHeight");
	}

	/// <summary>Set the character which owns this sprite.</summary>
	/// <param name="owner">The owner to set.</param>
	public virtual void SetOwner(Character owner)
	{
		this.Owner = owner;
	}

	/// <summary>Load and set the texture if exists.</summary>
	/// <param name="textureName">The texture name to load.</param>
	/// <param name="syncTextureName">Whether to set the <see cref="F:StardewValley.AnimatedSprite.textureName" /> field so it's synced to other players in multiplayer (if true), else set <see cref="F:StardewValley.AnimatedSprite.overrideTextureName" /> which is local-only (if false).</param>
	public virtual void LoadTexture(string textureName, bool syncTextureName = true)
	{
		if (Game1.content.DoesAssetExist<Texture2D>(textureName))
		{
			if (syncTextureName)
			{
				this.textureName.Value = textureName;
				this.overrideTextureName = null;
			}
			else
			{
				this.overrideTextureName = textureName;
			}
			this.loadTexture();
		}
	}

	private void loadTexture()
	{
		string assetName = this.overrideTextureName ?? this.textureName.Value;
		if (!(this.loadedTexture == assetName))
		{
			this.spriteTexture = ((assetName != null) ? this.contentManager.Load<Texture2D>(assetName) : null);
			this.loadedTexture = assetName;
			if (this.spriteTexture != null)
			{
				this.UpdateSourceRect();
			}
		}
	}

	public int getHeight()
	{
		return this.SpriteHeight;
	}

	public int getWidth()
	{
		return this.SpriteWidth;
	}

	public virtual void StopAnimation()
	{
		if (this.ignoreStopAnimation)
		{
			return;
		}
		if (this.CurrentAnimation != null)
		{
			this.CurrentAnimation = null;
			this.currentFrame = this.oldFrame;
			this.UpdateSourceRect();
			return;
		}
		if (this is FarmerSprite && this.currentFrame >= 232)
		{
			this.currentFrame -= 8;
		}
		if (this.currentFrame >= 64 && this.currentFrame <= 155)
		{
			this.currentFrame = (this.currentFrame - this.currentFrame % (this.textureWidth / this.SpriteWidth)) % 32 + 96;
		}
		else if (this.textureUsesFlippedRightForLeft && this.currentFrame >= this.textureWidth / this.SpriteWidth * 3)
		{
			if (this.currentFrame == 14 && this.textureWidth / this.SpriteWidth == 4)
			{
				this.currentFrame = 4;
			}
		}
		else
		{
			this.currentFrame = (this.currentFrame - this.currentFrame % (this.textureWidth / this.SpriteWidth)) % 32;
		}
		this.UpdateSourceRect();
	}

	public virtual void standAndFaceDirection(int direction)
	{
		switch (direction)
		{
		case 0:
			this.currentFrame = 12;
			break;
		case 1:
			this.currentFrame = 6;
			break;
		case 2:
			this.currentFrame = 0;
			break;
		case 3:
			this.currentFrame = 6;
			break;
		}
		this.UpdateSourceRect();
	}

	public virtual void faceDirectionStandard(int direction)
	{
		switch (direction)
		{
		case 0:
			direction = 2;
			break;
		case 2:
			direction = 0;
			break;
		}
		this.currentFrame = direction * 4;
		this.UpdateSourceRect();
	}

	public virtual void faceDirection(int direction)
	{
		if (this.ignoreStopAnimation || this.CurrentAnimation != null)
		{
			return;
		}
		try
		{
			switch (direction)
			{
			case 0:
				this.currentFrame = this.textureWidth / this.SpriteWidth * 2 + this.currentFrame % (this.textureWidth / this.SpriteWidth);
				break;
			case 1:
				this.currentFrame = this.textureWidth / this.SpriteWidth + this.currentFrame % (this.textureWidth / this.SpriteWidth);
				break;
			case 2:
				this.currentFrame %= this.textureWidth / this.SpriteWidth;
				break;
			case 3:
				if (this.textureUsesFlippedRightForLeft)
				{
					this.currentFrame = this.textureWidth / this.SpriteWidth + this.currentFrame % (this.textureWidth / this.SpriteWidth);
				}
				else
				{
					this.currentFrame = this.textureWidth / this.SpriteWidth * 3 + this.currentFrame % (this.textureWidth / this.SpriteWidth);
				}
				break;
			}
		}
		catch (Exception)
		{
		}
		this.UpdateSourceRect();
	}

	public virtual void AnimateRight(GameTime gameTime, int intervalOffset = 0, string soundForFootstep = "")
	{
		if (this.currentFrame >= this.framesPerAnimation * 2 || this.currentFrame < this.framesPerAnimation)
		{
			this.currentFrame = this.framesPerAnimation + this.currentFrame % this.framesPerAnimation;
		}
		this.timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
		if (this.timer > this.interval + (float)intervalOffset)
		{
			this.currentFrame++;
			this.timer = 0f;
			if (this.currentFrame % 2 != 0 && soundForFootstep.Length > 0 && (Game1.currentSong == null || Game1.currentSong.IsStopped))
			{
				Game1.playSound(soundForFootstep);
			}
			if (this.currentFrame >= this.framesPerAnimation * 2 && this.loop)
			{
				this.currentFrame = this.framesPerAnimation;
			}
		}
		this.UpdateSourceRect();
	}

	public virtual void AnimateUp(GameTime gameTime, int intervalOffset = 0, string soundForFootstep = "")
	{
		if (this.currentFrame >= this.framesPerAnimation * 3 || this.currentFrame < this.framesPerAnimation * 2)
		{
			this.currentFrame = this.framesPerAnimation * 2 + this.currentFrame % this.framesPerAnimation;
		}
		this.timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
		if (this.timer > this.interval + (float)intervalOffset)
		{
			this.currentFrame++;
			this.timer = 0f;
			if (this.currentFrame % 2 != 0 && soundForFootstep.Length > 0 && (Game1.currentSong == null || Game1.currentSong.IsStopped))
			{
				Game1.playSound(soundForFootstep);
			}
			if (this.currentFrame >= this.framesPerAnimation * 3 && this.loop)
			{
				this.currentFrame = this.framesPerAnimation * 2;
			}
		}
		this.UpdateSourceRect();
	}

	public virtual void AnimateDown(GameTime gameTime, int intervalOffset = 0, string soundForFootstep = "")
	{
		if (this.currentFrame >= this.framesPerAnimation || this.currentFrame < 0)
		{
			this.currentFrame %= this.framesPerAnimation;
		}
		this.timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
		if (this.timer > this.interval + (float)intervalOffset)
		{
			this.currentFrame++;
			this.timer = 0f;
			if (this.currentFrame % 2 != 0 && soundForFootstep.Length > 0 && (Game1.currentSong == null || Game1.currentSong.IsStopped))
			{
				Game1.playSound(soundForFootstep);
			}
			if (this.currentFrame >= this.framesPerAnimation && this.loop)
			{
				this.currentFrame = 0;
			}
		}
		this.UpdateSourceRect();
	}

	public virtual void AnimateLeft(GameTime gameTime, int intervalOffset = 0, string soundForFootstep = "")
	{
		if (this.currentFrame >= this.framesPerAnimation * 4 || this.currentFrame < this.framesPerAnimation * 3)
		{
			this.currentFrame = this.framesPerAnimation * 3 + this.currentFrame % this.framesPerAnimation;
		}
		this.timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
		if (this.timer > this.interval + (float)intervalOffset)
		{
			this.currentFrame++;
			this.timer = 0f;
			if (this.currentFrame % 2 != 0 && soundForFootstep.Length > 0 && (Game1.currentSong == null || Game1.currentSong.IsStopped))
			{
				Game1.playSound(soundForFootstep);
			}
			if (this.currentFrame >= this.framesPerAnimation * 4 && this.loop)
			{
				this.currentFrame = this.framesPerAnimation * 3;
			}
		}
		this.UpdateSourceRect();
	}

	public virtual bool Animate(GameTime gameTime, int startFrame, int numberOfFrames, float interval)
	{
		if (this.currentFrame >= startFrame + numberOfFrames || this.currentFrame < startFrame)
		{
			this.currentFrame = startFrame + this.currentFrame % numberOfFrames;
		}
		this.timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
		if (this.timer > interval)
		{
			this.currentFrame++;
			this.timer = 0f;
			if (this.currentFrame >= startFrame + numberOfFrames)
			{
				if (this.loop)
				{
					this.currentFrame = startFrame;
				}
				this.UpdateSourceRect();
				return true;
			}
		}
		this.UpdateSourceRect();
		return false;
	}

	public virtual void ClearAnimation()
	{
		this.currentAnimation.Clear();
		this.oldFrame = this.currentFrame;
		this.currentAnimationIndex = 0;
	}

	public virtual void AddFrame(FarmerSprite.AnimationFrame frame)
	{
		if (this.currentAnimation.Count == 0)
		{
			this.timer = frame.milliseconds;
			this.currentFrame = frame.frame;
		}
		this.currentAnimation.Add(frame);
	}

	public virtual void setCurrentAnimation(List<FarmerSprite.AnimationFrame> animation)
	{
		this.currentAnimation.Clear();
		this.currentAnimation.AddRange(animation);
		this.oldFrame = this.currentFrame;
		this.currentAnimationIndex = 0;
		if (this.CurrentAnimation.Count > 0)
		{
			this.timer = this.CurrentAnimation[0].milliseconds;
			this.currentFrame = this.CurrentAnimation[0].frame;
		}
	}

	/// returns true when the animation is finished
	public virtual bool animateOnce(GameTime time)
	{
		if (this.CurrentAnimation != null)
		{
			this.timer -= time.ElapsedGameTime.Milliseconds;
			if (this.timer <= 0f)
			{
				if (this.CurrentAnimation[this.currentAnimationIndex].frameEndBehavior != null)
				{
					this.CurrentAnimation[this.currentAnimationIndex].frameEndBehavior(null);
					if (this.CurrentAnimation == null)
					{
						this.currentFrame = this.oldFrame;
						this.CurrentAnimation = null;
						this.UpdateSourceRect();
						return true;
					}
				}
				this.currentAnimationIndex++;
				if (this.currentAnimationIndex >= this.CurrentAnimation.Count)
				{
					if (!this.loop)
					{
						this.currentFrame = this.oldFrame;
						this.CurrentAnimation = null;
						this.UpdateSourceRect();
						return true;
					}
					this.currentAnimationIndex = 0;
				}
				if (this.CurrentAnimation[this.currentAnimationIndex].frameStartBehavior != null)
				{
					this.CurrentAnimation[this.currentAnimationIndex].frameStartBehavior(null);
				}
				if (this.CurrentAnimation != null)
				{
					this.timer = this.CurrentAnimation[this.currentAnimationIndex].milliseconds;
					this.currentFrame = this.CurrentAnimation[this.currentAnimationIndex].frame;
				}
			}
			this.UpdateSourceRect();
			return false;
		}
		this.UpdateSourceRect();
		return true;
	}

	public virtual void UpdateSourceRect()
	{
		if (!this.ignoreSourceRectUpdates)
		{
			int curSpriteWidth = this.SpriteWidth;
			int curSpriteHeight = this.SpriteHeight;
			int curTextureWidth = this.textureWidth;
			int curTextureHeight = this.textureHeight;
			this.SourceRect = AnimatedSprite.GetSourceRect(curTextureWidth, curSpriteWidth, curSpriteHeight, this.currentFrame);
			if (this.Texture != null && (this.SourceRect.Right > curTextureWidth || this.SourceRect.Bottom > curTextureHeight))
			{
				this.currentFrame = 0;
				this.SourceRect = AnimatedSprite.GetSourceRect(curTextureWidth, curSpriteWidth, curSpriteHeight, this.currentFrame);
			}
		}
	}

	public virtual void draw(SpriteBatch b, Vector2 screenPosition, float layerDepth)
	{
		if (this.Texture != null)
		{
			b.Draw(this.Texture, screenPosition, this.sourceRect, Color.White, 0f, Vector2.Zero, 4f, (this.CurrentAnimation != null && this.CurrentAnimation[this.currentAnimationIndex].flip) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
		}
	}

	public virtual void draw(SpriteBatch b, Vector2 screenPosition, float layerDepth, int xOffset, int yOffset, Color c, bool flip = false, float scale = 1f, float rotation = 0f, bool characterSourceRectOffset = false)
	{
		if (this.Texture != null)
		{
			b.Draw(this.Texture, screenPosition, new Rectangle(this.sourceRect.X + xOffset, this.sourceRect.Y + yOffset, this.sourceRect.Width, this.sourceRect.Height), c, rotation, characterSourceRectOffset ? new Vector2(this.SpriteWidth / 2, (float)this.SpriteHeight * 3f / 4f) : Vector2.Zero, scale, (flip || (this.CurrentAnimation != null && this.CurrentAnimation[this.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
		}
	}

	public virtual void drawShadow(SpriteBatch b, Vector2 screenPosition, float scale = 4f, float alpha = 1f)
	{
		b.Draw(Game1.shadowTexture, screenPosition + new Vector2((float)(this.SpriteWidth / 2 * 4) - scale, (float)(this.SpriteHeight * 4) - scale), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, Utility.PointToVector2(Game1.shadowTexture.Bounds.Center), scale, SpriteEffects.None, 1E-05f);
	}

	public virtual void drawShadow(SpriteBatch b, Vector2 screenPosition, float scale = 4f)
	{
		this.drawShadow(b, screenPosition, scale, 1f);
	}

	public virtual AnimatedSprite Clone()
	{
		AnimatedSprite animatedSprite = new AnimatedSprite();
		animatedSprite.spriteWidth.Set(this.spriteWidth.Value);
		animatedSprite.spriteHeight.Set(this.spriteHeight.Value);
		animatedSprite.spriteTexture = this.spriteTexture;
		animatedSprite.loadedTexture = this.loadedTexture;
		animatedSprite.textureName.Set(this.textureName.Value);
		animatedSprite.timer = this.timer;
		animatedSprite.interval = this.interval;
		animatedSprite.framesPerAnimation = this.framesPerAnimation;
		animatedSprite.currentFrame = this.currentFrame;
		animatedSprite.tempSpriteHeight = this.tempSpriteHeight;
		animatedSprite.sourceRect = new Rectangle(this.sourceRect.X, this.sourceRect.Y, this.sourceRect.Width, this.sourceRect.Height);
		animatedSprite.loop = this.loop;
		animatedSprite.ignoreStopAnimation = this.ignoreStopAnimation;
		animatedSprite.textureUsesFlippedRightForLeft = this.textureUsesFlippedRightForLeft;
		animatedSprite.CurrentAnimation = this.CurrentAnimation;
		animatedSprite.oldFrame = this.oldFrame;
		animatedSprite.currentAnimationIndex = this.currentAnimationIndex;
		animatedSprite.contentManager = this.contentManager;
		animatedSprite.UpdateSourceRect();
		return animatedSprite;
	}

	/// <summary>Calculate the source rectangle for a sprite in an NPC spritesheet.</summary>
	/// <param name="textureWidth">The pixel width of the full spritesheet texture.</param>
	/// <param name="spriteWidth">The pixel width of each sprite.</param>
	/// <param name="spriteHeight">The pixel height of each sprite.</param>
	/// <param name="frame">The frame index, starting at 0 for the top-left corner.</param>
	public static Rectangle GetSourceRect(int textureWidth, int spriteWidth, int spriteHeight, int frame)
	{
		return new Rectangle(frame * spriteWidth % textureWidth, frame * spriteWidth / textureWidth * spriteHeight, spriteWidth, spriteHeight);
	}
}
