using System;
using Microsoft.Xna.Framework;

namespace StardewValley.BellsAndWhistles;

public class ScreenFade
{
	public bool globalFade;

	public bool fadeIn = true;

	public bool fadeToBlack;

	public bool nonWarpFade;

	public float fadeToBlackAlpha;

	public float globalFadeSpeed;

	private const float fadeToFudge = 0.1f;

	private Game1.afterFadeFunction afterFade;

	private Func<bool> onFadeToBlackComplete;

	private Action onFadedBackInComplete;

	public ScreenFade(Func<bool> onFadeToBlack, Action onFadeIn)
	{
		this.onFadeToBlackComplete = onFadeToBlack;
		this.onFadedBackInComplete = onFadeIn;
	}

	public bool UpdateFade(GameTime time)
	{
		if (this.fadeToBlack && (Game1.pauseTime == 0f || Game1.eventUp))
		{
			if (this.fadeToBlackAlpha > 1.1f && !Game1.messagePause)
			{
				this.fadeToBlackAlpha = 1f;
				if (this.onFadeToBlackComplete())
				{
					return true;
				}
				this.nonWarpFade = false;
				this.fadeIn = false;
				if (this.afterFade != null)
				{
					Game1.afterFadeFunction afterFadeFunction = this.afterFade;
					this.afterFade = null;
					afterFadeFunction();
				}
				this.globalFade = false;
			}
			if (this.fadeToBlackAlpha < -0.1f)
			{
				this.fadeToBlackAlpha = 0f;
				this.fadeToBlack = false;
				this.onFadedBackInComplete();
			}
			this.UpdateFadeAlpha(time);
		}
		return false;
	}

	public void UpdateFadeAlpha(GameTime time)
	{
		if (this.fadeIn)
		{
			this.fadeToBlackAlpha += ((Game1.eventUp || Game1.farmEvent != null) ? 0.0008f : 0.0019f) * (float)time.ElapsedGameTime.Milliseconds;
			this.fadeToBlackAlpha = (Game1.IsDedicatedHost ? 1.2f : this.fadeToBlackAlpha);
		}
		else if (!Game1.messagePause && !Game1.dialogueUp)
		{
			this.fadeToBlackAlpha -= ((Game1.eventUp || Game1.farmEvent != null) ? 0.0008f : 0.0019f) * (float)time.ElapsedGameTime.Milliseconds;
			this.fadeToBlackAlpha = (Game1.IsDedicatedHost ? (-0.2f) : this.fadeToBlackAlpha);
		}
	}

	public void FadeScreenToBlack(float startAlpha = 0f, bool stopMovement = true)
	{
		this.globalFade = false;
		this.fadeToBlack = true;
		this.fadeIn = true;
		this.fadeToBlackAlpha = startAlpha;
		if (stopMovement)
		{
			Game1.player.CanMove = false;
		}
	}

	public void FadeClear(float startAlpha = 1f)
	{
		this.globalFade = false;
		this.fadeIn = false;
		this.fadeToBlack = true;
		this.fadeToBlackAlpha = startAlpha;
	}

	public void GlobalFadeToBlack(Game1.afterFadeFunction afterFade = null, float fadeSpeed = 0.02f)
	{
		if (this.fadeToBlack && !this.fadeIn)
		{
			this.onFadedBackInComplete();
		}
		this.fadeToBlack = false;
		this.globalFade = true;
		this.fadeIn = false;
		this.afterFade = afterFade;
		this.globalFadeSpeed = fadeSpeed;
		this.fadeToBlackAlpha = 0f;
	}

	public void GlobalFadeToClear(Game1.afterFadeFunction afterFade = null, float fadeSpeed = 0.02f)
	{
		if (this.fadeToBlack && this.fadeIn)
		{
			this.onFadeToBlackComplete();
		}
		this.fadeToBlack = false;
		this.globalFade = true;
		this.fadeIn = true;
		this.afterFade = afterFade;
		this.globalFadeSpeed = fadeSpeed;
		this.fadeToBlackAlpha = 1f;
	}

	public void UpdateGlobalFade()
	{
		if (this.fadeIn)
		{
			if (this.fadeToBlackAlpha <= 0f)
			{
				this.globalFade = false;
				if (this.afterFade != null)
				{
					Game1.afterFadeFunction tmp = this.afterFade;
					this.afterFade();
					if (this.afterFade != null && this.afterFade.Equals(tmp))
					{
						this.afterFade = null;
					}
					if (Game1.nonWarpFade)
					{
						this.fadeToBlack = false;
					}
				}
			}
			this.fadeToBlackAlpha = (Game1.IsDedicatedHost ? 0f : Math.Max(0f, this.fadeToBlackAlpha - this.globalFadeSpeed));
			return;
		}
		if (this.fadeToBlackAlpha >= 1f)
		{
			this.globalFade = false;
			if (this.afterFade != null)
			{
				Game1.afterFadeFunction tmp2 = this.afterFade;
				this.afterFade();
				if (this.afterFade != null && this.afterFade.Equals(tmp2))
				{
					this.afterFade = null;
				}
				if (Game1.nonWarpFade)
				{
					this.fadeToBlack = false;
				}
			}
		}
		this.fadeToBlackAlpha = (Game1.IsDedicatedHost ? 1f : Math.Min(1f, this.fadeToBlackAlpha + this.globalFadeSpeed));
	}
}
