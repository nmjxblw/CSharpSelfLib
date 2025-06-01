using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus;

public class SpecialCurrencyDisplay
{
	/// <summary>The metadata for a currency which can be rendered by <see cref="T:StardewValley.Menus.SpecialCurrencyDisplay" />.</summary>
	public class CurrencyDisplayType
	{
		/// <summary>The currency ID, like <see cref="F:StardewValley.Menus.SpecialCurrencyDisplay.currency_walnuts" />.</summary>
		public string key;

		/// <summary>The field which contains the currency amount.</summary>
		public NetIntDelta field;

		/// <summary>Play a sound when the currency amount changes.</summary>
		public Action<int> playSound;

		/// <summary>Draw the currency sprite at the given position.</summary>
		public Action<SpriteBatch, Vector2> drawIcon;
	}

	/// <summary>The render info for a currency being drawn to the screen.</summary>
	public class CurrencyRenderInfo
	{
		/// <summary>The currency to display.</summary>
		public CurrencyDisplayType currency;

		/// <summary>The currency dial UI to render.</summary>
		public MoneyDial moneyDial = new MoneyDial(3)
		{
			onPlaySound = null
		};

		/// <summary>The slide position of the display, as a value between 0 (hidden) and 1 (fully displayed).</summary>
		public float slidePosition;

		/// <summary>If set, pause the <see cref="F:StardewValley.Menus.SpecialCurrencyDisplay.CurrencyRenderInfo.timeToLive" /> until it returns false.</summary>
		public Func<bool> keepOpen;

		/// <summary>The number of seconds until the <see cref="F:StardewValley.Menus.SpecialCurrencyDisplay.displayedCurrencies" /> begins to slide out of view.</summary>
		public float timeToLive;

		/// <summary>Construct an instance.</summary>
		/// <param name="currency">The currency ID to display (like <see cref="F:StardewValley.Menus.SpecialCurrencyDisplay.currency_walnuts" />), or <c>null</c> to hide it.</param>
		/// <param name="keepOpen">If set, pause the <paramref name="timeToLive" /> until it returns false.</param>
		/// <param name="timeToLive">The number of seconds until the currency disappears.</param>
		public CurrencyRenderInfo(CurrencyDisplayType currency, Func<bool> keepOpen = null, float timeToLive = 5f)
		{
			this.currency = currency;
			this.keepOpen = keepOpen;
			this.timeToLive = timeToLive;
			this.moneyDial.currentValue = currency.field.TargetValue;
			this.moneyDial.previousTargetValue = currency.field.Value;
			this.moneyDial.onPlaySound = currency.playSound;
		}

		public void OnCurrencyChanged(int oldValue, int newValue)
		{
			this.timeToLive = Math.Max(this.timeToLive, 5f);
			this.moneyDial.currentValue = oldValue;
			this.moneyDial.onPlaySound?.Invoke(newValue - oldValue);
		}
	}

	/// <summary>The currency ID for golden walnuts.</summary>
	public const string currency_walnuts = "walnuts";

	/// <summary>The currency ID for Qi gems.</summary>
	public const string currency_qiGems = "qiGems";

	/// <summary>The default <see cref="F:StardewValley.Menus.SpecialCurrencyDisplay.CurrencyRenderInfo.timeToLive" /> value.</summary>
	public const int defaultSeconds = 5;

	/// <summary>The currencies which can be displayed, indexed by currency ID like <see cref="F:StardewValley.Menus.SpecialCurrencyDisplay.currency_walnuts" />.</summary>
	public Dictionary<string, CurrencyDisplayType> registeredCurrencyDisplays = new Dictionary<string, CurrencyDisplayType>();

	/// <summary>The currencies from <see cref="F:StardewValley.Menus.SpecialCurrencyDisplay.registeredCurrencyDisplays" /> to render.</summary>
	public readonly List<CurrencyRenderInfo> displayedCurrencies = new List<CurrencyRenderInfo>();

	/// <summary>Register a currency which can be rendered manually (via <see cref="M:StardewValley.Menus.SpecialCurrencyDisplay.ShowCurrency(System.String,System.Func{System.Boolean},System.Single)" />) or automatically (via <see cref="!:field" /> event listeners).</summary>
	/// <param name="key">The currency ID, like <see cref="F:StardewValley.Menus.SpecialCurrencyDisplay.currency_walnuts" />.</param>
	/// <param name="field">The field which contains the currency amount.</param>
	/// <param name="playSound">Play a sound when the currency amount changes, or <c>null</c> to play the default sound.</param>
	/// <param name="drawIcon">Draw the currency sprite at the given position, or <c>null</c> to draw the default sprite.</param>
	public virtual void Register(string key, NetIntDelta field, Action<int> playSound = null, Action<SpriteBatch, Vector2> drawIcon = null)
	{
		if (this.registeredCurrencyDisplays.ContainsKey(key))
		{
			this.Unregister(key);
		}
		playSound = playSound ?? ((Action<int>)delegate(int delta)
		{
			this.PlaySound(key, delta);
		});
		drawIcon = drawIcon ?? ((Action<SpriteBatch, Vector2>)delegate(SpriteBatch b, Vector2 position)
		{
			this.DrawIcon(key, b, position);
		});
		this.registeredCurrencyDisplays[key] = new CurrencyDisplayType
		{
			key = key,
			field = field,
			playSound = playSound,
			drawIcon = drawIcon
		};
		field.fieldChangeVisibleEvent += OnCurrencyChange;
	}

	/// <summary>Show the currency display.</summary>
	/// <param name="currency">The currency ID to display (like <see cref="F:StardewValley.Menus.SpecialCurrencyDisplay.currency_walnuts" />).</param>
	/// <param name="keepOpen">If set, pause the <see cref="!:timeToLive" /> until it returns false.</param>
	/// <param name="timeToLive">The number of seconds until the currency disappears.</param>
	public virtual void ShowCurrency(string currency, Func<bool> keepOpen = null, float timeToLive = 5f)
	{
		if (currency == null)
		{
			return;
		}
		foreach (CurrencyRenderInfo rendered in this.displayedCurrencies)
		{
			if (rendered.currency.key == currency)
			{
				rendered.keepOpen = keepOpen ?? rendered.keepOpen;
				rendered.timeToLive = Math.Max(rendered.timeToLive, timeToLive);
				return;
			}
		}
		if (this.registeredCurrencyDisplays.TryGetValue(currency, out var data))
		{
			this.displayedCurrencies.Add(new CurrencyRenderInfo(data, keepOpen, timeToLive));
		}
		else
		{
			Game1.log.Warn("Can't show unknown currency type '" + currency + "'.");
		}
	}

	/// <summary>Hide a currency if it's displayed.</summary>
	/// <param name="currency">The currency ID to hide (like <see cref="F:StardewValley.Menus.SpecialCurrencyDisplay.currency_walnuts" />).</param>
	/// <param name="immediate">Remove the currency immediately, instead of letting it slide out.</param>
	public virtual void HideCurrency(string currency, bool immediate = true)
	{
		if (immediate)
		{
			this.displayedCurrencies.RemoveAll((CurrencyRenderInfo p) => p.currency.key == currency);
			return;
		}
		foreach (CurrencyRenderInfo rendered in this.displayedCurrencies)
		{
			if (rendered.currency.key == currency)
			{
				rendered.keepOpen = null;
				rendered.timeToLive = 0f;
			}
		}
	}

	/// <summary>Update the display if needed when a currency value changes.</summary>
	/// <param name="field">The field containing the currency value.</param>
	/// <param name="oldValue">The previous currency value.</param>
	/// <param name="newValue">The new currency value.</param>
	public virtual void OnCurrencyChange(NetIntDelta field, int oldValue, int newValue)
	{
		if (Game1.gameMode != 3 || oldValue == newValue)
		{
			return;
		}
		foreach (CurrencyRenderInfo render in this.displayedCurrencies)
		{
			if ((object)render.currency.field == field)
			{
				render.OnCurrencyChanged(oldValue, newValue);
				return;
			}
		}
		foreach (CurrencyDisplayType currency in this.registeredCurrencyDisplays.Values)
		{
			if ((object)currency.field == field)
			{
				CurrencyRenderInfo render2 = new CurrencyRenderInfo(currency);
				render2.OnCurrencyChanged(oldValue, newValue);
				this.displayedCurrencies.Add(render2);
				return;
			}
		}
		Game1.log.Warn("Can't show currency change for unknown field '" + field.Name + "'.");
	}

	/// <summary>Remove a currency that was registered via <see cref="M:StardewValley.Menus.SpecialCurrencyDisplay.Register(System.String,Netcode.NetIntDelta,System.Action{System.Int32},System.Action{Microsoft.Xna.Framework.Graphics.SpriteBatch,Microsoft.Xna.Framework.Vector2})" />.</summary>
	/// <param name="key">The currency ID, like <see cref="F:StardewValley.Menus.SpecialCurrencyDisplay.currency_walnuts" />.</param>
	public virtual void Unregister(string key)
	{
		this.HideCurrency(key);
		if (this.registeredCurrencyDisplays.TryGetValue(key, out var newCurrencyType))
		{
			newCurrencyType.field.fieldChangeVisibleEvent -= OnCurrencyChange;
			this.registeredCurrencyDisplays.Remove(key);
		}
	}

	/// <summary>Unregister all currencies.</summary>
	public virtual void Cleanup()
	{
		foreach (string key in new List<string>(this.registeredCurrencyDisplays.Keys))
		{
			this.Unregister(key);
		}
	}

	/// <summary>Draw the default icon for a currency.</summary>
	/// <param name="currency">The currency ID to render.</param>
	/// <param name="b">The sprite batch being drawn.</param>
	/// <param name="position">The position at which to draw the icon.</param>
	public virtual void DrawIcon(string currency, SpriteBatch b, Vector2 position)
	{
		if (!(currency == "walnuts"))
		{
			if (currency == "qiGems")
			{
				b.Draw(Game1.objectSpriteSheet, position, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 858, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
			}
		}
		else
		{
			b.Draw(Game1.objectSpriteSheet, position, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 73, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
		}
	}

	/// <summary>Play the default sound.</summary>
	/// <param name="currency">The currency ID whose sound to play.</param>
	/// <param name="direction">The change to the currency value.</param>
	public virtual void PlaySound(string currency, int direction)
	{
		if (currency == "walnuts")
		{
			Game1.playSound("goldenWalnut");
		}
	}

	/// <summary>Update the display if it's currently active.</summary>
	/// <param name="time">The elapsed game time.</param>
	public virtual void Update(GameTime time)
	{
		for (int i = 0; i < this.displayedCurrencies.Count; i++)
		{
			CurrencyRenderInfo render = this.displayedCurrencies[i];
			bool keepOpen = render.keepOpen?.Invoke() ?? false;
			if (!keepOpen)
			{
				render.keepOpen = null;
				render.timeToLive -= (float)time.ElapsedGameTime.TotalSeconds;
				if (render.timeToLive < 0f)
				{
					render.timeToLive = 0f;
				}
			}
			float positionDelta = (float)time.ElapsedGameTime.TotalSeconds / 0.5f;
			render.slidePosition += ((keepOpen || render.timeToLive > 0f) ? positionDelta : (0f - positionDelta));
			render.slidePosition = Utility.Clamp(render.slidePosition, 0f, 1f);
			if (!keepOpen && render.timeToLive <= 0f && render.slidePosition <= 0f)
			{
				this.displayedCurrencies.RemoveAt(i);
				i--;
			}
		}
	}

	/// <summary>Get the default draw position.</summary>
	/// <param name="slidePosition">The slide position of the display, as a value between 0 (hidden) and 1 (fully displayed)..</param>
	public Vector2 GetUpperLeft(float slidePosition)
	{
		return new Vector2(16f, (int)Utility.Lerp(-26f, 0f, slidePosition) * 4);
	}

	/// <summary>Draw the currency display if needed.</summary>
	/// <param name="b">The sprite batch being drawn.</param>
	public virtual void Draw(SpriteBatch b)
	{
		if (this.displayedCurrencies.Count == 0)
		{
			return;
		}
		int leftOffset = 0;
		foreach (CurrencyRenderInfo render in this.displayedCurrencies)
		{
			MoneyDial moneyDial = render.moneyDial;
			Vector2 drawPosition = this.GetUpperLeft(render.slidePosition);
			if (leftOffset > 0)
			{
				drawPosition.X += leftOffset;
			}
			Rectangle backgroundSourceRect = new Rectangle(48, 176, 52, 26);
			b.Draw(Game1.mouseCursors2, drawPosition, backgroundSourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
			leftOffset += backgroundSourceRect.Width * 4;
			int displayedValue = render.currency.field.Value;
			if (render.slidePosition < 0.5f)
			{
				displayedValue = moneyDial.previousTargetValue;
			}
			moneyDial.draw(b, drawPosition + new Vector2(108f, 40f), displayedValue);
			render.currency.drawIcon?.Invoke(b, drawPosition + new Vector2(4f, 6f) * 4f);
		}
	}

	/// <summary>Draw a currency display to the screen.</summary>
	/// <param name="b">The sprite batch being drawn.</param>
	/// <param name="drawPosition">The position at which to draw the display.</param>
	/// <param name="moneyDial">The currency dial to render.</param>
	/// <param name="displayedValue">The currency value.</param>
	/// <param name="drawSpriteTexture">The sprite texture for the currency icon.</param>
	/// <param name="drawSpriteSourceRect">The pixel area within the <paramref name="drawSpriteTexture" /> for the currency icon.</param>
	public static void Draw(SpriteBatch b, Vector2 drawPosition, MoneyDial moneyDial, int displayedValue, Texture2D drawSpriteTexture, Rectangle drawSpriteSourceRect)
	{
		if (moneyDial != null && moneyDial.numDigits > 3)
		{
			b.Draw(Game1.mouseCursors_1_6, drawPosition, new Rectangle(42, 0, 57, 26), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
		}
		else
		{
			b.Draw(Game1.mouseCursors2, drawPosition, new Rectangle(48, 176, 52, 26), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
		}
		moneyDial?.draw(b, drawPosition + new Vector2(108f, 40f), displayedValue);
		b.Draw(drawSpriteTexture, drawPosition + new Vector2(4f, 6f) * 4f, drawSpriteSourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
	}

	/// <summary>Draw a very basic static money dial which can only do 3 digits.</summary>
	/// <param name="b">The sprite batch being drawn.</param>
	/// <param name="drawPosition">The position at which to draw the display.</param>
	/// <param name="displayedValue">The currency value.</param>
	/// <param name="drawSpriteTexture">The sprite texture for the currency icon.</param>
	/// <param name="drawSpriteSourceRect">The pixel area within the <paramref name="drawSpriteTexture" /> for the currency icon.</param>
	public static void Draw(SpriteBatch b, Vector2 drawPosition, int displayedValue, Texture2D drawSpriteTexture, Rectangle drawSpriteSourceRect)
	{
		b.Draw(Game1.mouseCursors2, drawPosition, new Rectangle(48, 176, 52, 26), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
		int numDigits = 3;
		int xPosition = 0;
		int digitStrip = (int)Math.Pow(10.0, numDigits - 1);
		bool significant = false;
		for (int i = 0; i < numDigits; i++)
		{
			int currentDigit = displayedValue / digitStrip % 10;
			if (currentDigit > 0 || i == numDigits - 1)
			{
				significant = true;
			}
			if (significant)
			{
				b.Draw(Game1.mouseCursors, drawPosition + new Vector2(108f, 40f) + new Vector2(xPosition, 0f), new Rectangle(286, 502 - currentDigit * 8, 5, 8), Color.Maroon, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			xPosition += 24;
			digitStrip /= 10;
		}
		b.Draw(drawSpriteTexture, drawPosition + new Vector2(4f, 6f) * 4f, drawSpriteSourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
	}
}
