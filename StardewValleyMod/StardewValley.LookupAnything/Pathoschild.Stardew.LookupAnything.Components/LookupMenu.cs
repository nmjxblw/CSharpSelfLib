using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.UI;
using Pathoschild.Stardew.Common.Utilities;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Components;

internal class LookupMenu : BaseMenu, IScrollableMenu, IDisposable
{
	private readonly ISubject Subject;

	private readonly IMonitor Monitor;

	private readonly Action<ISubject> ShowNewPage;

	private readonly ICustomField[] Fields;

	private readonly Vector2 AspectRatio = new Vector2((float)Sprites.Letter.Sprite.Width, (float)Sprites.Letter.Sprite.Height);

	private readonly IReflectionHelper Reflection;

	private readonly int ScrollAmount;

	private readonly bool ForceFullScreen;

	private readonly ClickableTextureComponent ScrollUpButton;

	private readonly ClickableTextureComponent ScrollDownButton;

	private readonly int ScrollButtonGutter = 15;

	private readonly BlendState ContentBlendState = new BlendState
	{
		AlphaBlendFunction = (BlendFunction)0,
		AlphaSourceBlend = (Blend)1,
		AlphaDestinationBlend = (Blend)0,
		ColorBlendFunction = (BlendFunction)0,
		ColorSourceBlend = (Blend)4,
		ColorDestinationBlend = (Blend)5
	};

	private int MaxScroll;

	private int CurrentScroll;

	private bool ValidatedDrawMode;

	private readonly Dictionary<ICustomField, Rectangle> LinkableFieldAreas = new Dictionary<ICustomField, Rectangle>(new ObjectReferenceComparer<ICustomField>());

	private readonly bool WasHudEnabled;

	private bool ExitOnNextTick;

	public LookupMenu(ISubject subject, IMonitor monitor, IReflectionHelper reflectionHelper, int scroll, bool showDebugFields, bool forceFullScreen, Action<ISubject> showNewPage)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Expected O, but got Unknown
		this.Subject = subject;
		this.Fields = (from p in subject.GetData()
			where p.HasValue
			select p).ToArray();
		this.Monitor = monitor;
		this.Reflection = reflectionHelper;
		this.ScrollAmount = scroll;
		this.ForceFullScreen = forceFullScreen;
		this.ShowNewPage = showNewPage;
		this.WasHudEnabled = Game1.displayHUD;
		if (showDebugFields)
		{
			this.Fields = this.Fields.Concat(((IEnumerable<IGrouping<string, IDebugField>>)(from p in subject.GetDebugFields().GroupBy(delegate(IDebugField p)
				{
					if (p.IsPinned)
					{
						return "debug (pinned)";
					}
					return (p.OverrideCategory != null) ? ("debug (" + p.OverrideCategory + ")") : "debug (raw)";
				})
				orderby p.Key == "debug (pinned)" descending
				select p)).Select((Func<IGrouping<string, IDebugField>, ICustomField>)((IGrouping<string, IDebugField> p) => new DataMiningField(p.Key, p)))).ToArray();
		}
		this.ScrollUpButton = new ClickableTextureComponent(Rectangle.Empty, CommonSprites.Icons.Sheet, CommonSprites.Icons.UpArrow, 1f, false);
		this.ScrollDownButton = new ClickableTextureComponent(Rectangle.Empty, CommonSprites.Icons.Sheet, CommonSprites.Icons.DownArrow, 1f, false);
		this.UpdateLayout();
		Game1.displayHUD = false;
	}

	public override bool overrideSnappyMenuCursorMovementBan()
	{
		return true;
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		this.HandleLeftClick(x, y);
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
	}

	public override void receiveScrollWheelAction(int direction)
	{
		if (direction > 0)
		{
			this.ScrollUp();
		}
		else
		{
			this.ScrollDown();
		}
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		this.UpdateLayout();
	}

	public override void receiveGamePadButton(Buttons button)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		if ((int)button <= 8192)
		{
			if ((int)button != 4096)
			{
				if ((int)button == 8192)
				{
					((IClickableMenu)this).exitThisMenu(true);
				}
			}
			else
			{
				Point p = Game1.getMousePosition();
				this.HandleLeftClick(p.X, p.Y);
			}
		}
		else if ((int)button != 16777216)
		{
			if ((int)button == 33554432)
			{
				this.ScrollDown();
			}
		}
		else
		{
			this.ScrollUp();
		}
	}

	public override void update(GameTime time)
	{
		if (this.ExitOnNextTick && ((IClickableMenu)this).readyToClose())
		{
			((IClickableMenu)this).exitThisMenu(true);
		}
		else
		{
			((IClickableMenu)this).update(time);
		}
	}

	public void QueueExit()
	{
		this.ExitOnNextTick = true;
	}

	public void ScrollUp(int? amount = null)
	{
		this.CurrentScroll -= amount ?? this.ScrollAmount;
	}

	public void ScrollDown(int? amount = null)
	{
		this.CurrentScroll += amount ?? this.ScrollAmount;
	}

	public void HandleLeftClick(int x, int y)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (!((IClickableMenu)this).isWithinBounds(x, y) || ((ClickableComponent)((IClickableMenu)this).upperRightCloseButton).containsPoint(x, y))
		{
			((IClickableMenu)this).exitThisMenu(true);
			return;
		}
		if (((ClickableComponent)this.ScrollUpButton).containsPoint(x, y))
		{
			this.ScrollUp();
			return;
		}
		if (((ClickableComponent)this.ScrollDownButton).containsPoint(x, y))
		{
			this.ScrollDown();
			return;
		}
		foreach (KeyValuePair<ICustomField, Rectangle> linkableFieldArea in this.LinkableFieldAreas)
		{
			var (field, fieldArea) = (KeyValuePair<ICustomField, Rectangle>)(ref linkableFieldArea);
			if (((Rectangle)(ref fieldArea)).Contains(x, y) && field.TryGetLinkAt(x, y, out ISubject subject))
			{
				this.ShowNewPage(subject);
				break;
			}
		}
	}

	public override void draw(SpriteBatch b)
	{
		this.Monitor.InterceptErrors("drawing the lookup info", delegate
		{
			//IL_0805: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Expected O, but got Unknown
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Invalid comparison between Unknown and I4
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d4: Expected O, but got Unknown
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Expected O, but got Unknown
			//IL_0293: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0310: Unknown result type (might be due to invalid IL or missing references)
			//IL_0315: Unknown result type (might be due to invalid IL or missing references)
			//IL_0343: Unknown result type (might be due to invalid IL or missing references)
			//IL_0353: Unknown result type (might be due to invalid IL or missing references)
			//IL_036a: Unknown result type (might be due to invalid IL or missing references)
			//IL_031f: Unknown result type (might be due to invalid IL or missing references)
			//IL_036f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0373: Unknown result type (might be due to invalid IL or missing references)
			//IL_037a: Unknown result type (might be due to invalid IL or missing references)
			//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03db: Unknown result type (might be due to invalid IL or missing references)
			//IL_03df: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_051b: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0508: Unknown result type (might be due to invalid IL or missing references)
			//IL_050d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0550: Unknown result type (might be due to invalid IL or missing references)
			//IL_053e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0547: Unknown result type (might be due to invalid IL or missing references)
			//IL_0567: Unknown result type (might be due to invalid IL or missing references)
			//IL_056e: Unknown result type (might be due to invalid IL or missing references)
			//IL_057f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0584: Unknown result type (might be due to invalid IL or missing references)
			//IL_0592: Unknown result type (might be due to invalid IL or missing references)
			//IL_059c: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_05b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_05c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_05cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_05eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_061e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0625: Unknown result type (might be due to invalid IL or missing references)
			//IL_062a: Unknown result type (might be due to invalid IL or missing references)
			//IL_063d: Unknown result type (might be due to invalid IL or missing references)
			//IL_064d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0654: Unknown result type (might be due to invalid IL or missing references)
			//IL_0659: Unknown result type (might be due to invalid IL or missing references)
			//IL_0555: Unknown result type (might be due to invalid IL or missing references)
			//IL_06b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_06bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0688: Unknown result type (might be due to invalid IL or missing references)
			//IL_0690: Unknown result type (might be due to invalid IL or missing references)
			//IL_0698: Unknown result type (might be due to invalid IL or missing references)
			//IL_06a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_06a8: Unknown result type (might be due to invalid IL or missing references)
			this.LinkableFieldAreas.Clear();
			ISubject subject = this.Subject;
			if (!this.ValidatedDrawMode)
			{
				IReflectedField<SpriteSortMode> field = this.Reflection.GetField<SpriteSortMode>((object)Game1.spriteBatch, "_sortMode", true);
				if ((int)field.GetValue() == 1)
				{
					this.Monitor.Log("Aborted the lookup because the game's current rendering mode isn't compatible with the mod's UI. This only happens in rare cases (e.g. the Stardew Valley Fair).", (LogLevel)3);
					((IClickableMenu)this).exitThisMenu(false);
					return;
				}
				this.ValidatedDrawMode = true;
			}
			int xPositionOnScreen = ((IClickableMenu)this).xPositionOnScreen;
			int yPositionOnScreen = ((IClickableMenu)this).yPositionOnScreen;
			float num = 15f;
			float num2 = 15f;
			float num3 = ((IClickableMenu)this).width - 30;
			float num4 = ((IClickableMenu)this).height - 30;
			int num5 = 1;
			SpriteFont font = Game1.smallFont;
			float y = font.MeasureString("ABC").Y;
			float spaceWidth = DrawHelper.GetSpaceWidth(font);
			SpriteBatch val = new SpriteBatch(Game1.graphics.GraphicsDevice);
			try
			{
				float num6 = ((((IClickableMenu)this).width >= ((IClickableMenu)this).height) ? ((float)((IClickableMenu)this).width / (float)Sprites.Letter.Sprite.Width) : ((float)((IClickableMenu)this).height / (float)Sprites.Letter.Sprite.Height));
				val.Begin((SpriteSortMode)0, BlendState.NonPremultiplied, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null, (Effect)null, (Matrix?)null);
				Texture2D sheet = Sprites.Letter.Sheet;
				Rectangle sprite = Sprites.Letter.Sprite;
				float x = xPositionOnScreen;
				float y2 = yPositionOnScreen;
				Rectangle sprite2 = Sprites.Letter.Sprite;
				Point size = ((Rectangle)(ref sprite2)).Size;
				float scale = num6;
				val.DrawSprite(sheet, sprite, x, y2, size, null, scale);
				val.End();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			SpriteBatch val2 = new SpriteBatch(Game1.graphics.GraphicsDevice);
			try
			{
				GraphicsDevice graphicsDevice = Game1.graphics.GraphicsDevice;
				Rectangle scissorRectangle = graphicsDevice.ScissorRectangle;
				try
				{
					graphicsDevice.ScissorRectangle = new Rectangle(xPositionOnScreen + 15, yPositionOnScreen + 15, (int)num3, (int)num4);
					val2.Begin((SpriteSortMode)0, this.ContentBlendState, SamplerState.PointClamp, (DepthStencilState)null, new RasterizerState
					{
						ScissorTestEnable = true
					}, (Effect)null, (Matrix?)null);
					this.CurrentScroll = Math.Max(0, this.CurrentScroll);
					this.CurrentScroll = Math.Min(this.MaxScroll, this.CurrentScroll);
					num2 -= (float)this.CurrentScroll;
					if (subject.DrawPortrait(val2, new Vector2((float)xPositionOnScreen + num, (float)yPositionOnScreen + num2), new Vector2(70f, 70f)))
					{
						num += 72f;
					}
					float num7 = (float)((IClickableMenu)this).width - num - 15f;
					SpriteFont font2 = font;
					string text = subject.Name + ".";
					Vector2 position = new Vector2((float)xPositionOnScreen + num, (float)yPositionOnScreen + num2);
					bool allowBold = Constant.AllowBold;
					Vector2 val3 = val2.DrawTextBlock(font2, text, position, num7, null, allowBold);
					Vector2 val4 = ((subject.Type != null) ? val2.DrawTextBlock(font, subject.Type + ".", new Vector2((float)xPositionOnScreen + num + val3.X + spaceWidth, (float)yPositionOnScreen + num2), num7) : Vector2.Zero);
					num2 += Math.Max(val3.Y, val4.Y);
					if (subject.Description != null)
					{
						Vector2 val5 = val2.DrawTextBlock(font, subject.Description?.Replace(Environment.NewLine, " "), new Vector2((float)xPositionOnScreen + num, (float)yPositionOnScreen + num2), num7);
						num2 += val5.Y;
					}
					num2 += y;
					if (this.Fields.Any())
					{
						ICustomField[] fields = this.Fields;
						float num8 = 3f;
						float num9 = fields.Where((ICustomField p) => p.HasValue).Max((ICustomField p) => font.MeasureString(p.Label).X);
						float num10 = num7 - num9 - num8 * 4f - (float)num5;
						ICustomField[] array = fields;
						Vector2 val7 = default(Vector2);
						Vector2 val9 = default(Vector2);
						foreach (ICustomField customField in array)
						{
							if (customField.HasValue)
							{
								Vector2 val6 = val2.DrawTextBlock(font, customField.Label, new Vector2((float)xPositionOnScreen + num + num8, (float)yPositionOnScreen + num2 + num8), num7);
								((Vector2)(ref val7))._002Ector((float)xPositionOnScreen + num + num9 + num8 * 3f, (float)yPositionOnScreen + num2 + num8);
								Vector2 val8 = (Vector2)((customField.ExpandLink == null) ? (((_003F?)customField.DrawValue(val2, font, val7, num10)) ?? val2.DrawTextBlock(font, customField.Value, val7, num10)) : val2.DrawTextBlock(font, customField.ExpandLink.Value, val7, num10));
								((Vector2)(ref val9))._002Ector(num9 + num10 + num8 * 4f, Math.Max(val6.Y, val8.Y));
								Color gray = Color.Gray;
								DrawHelper.DrawLine(val2, (float)xPositionOnScreen + num, (float)yPositionOnScreen + num2, new Vector2(val9.X, (float)num5), gray);
								DrawHelper.DrawLine(val2, (float)xPositionOnScreen + num, (float)yPositionOnScreen + num2 + val9.Y, new Vector2(val9.X, (float)num5), gray);
								DrawHelper.DrawLine(val2, (float)xPositionOnScreen + num, (float)yPositionOnScreen + num2, new Vector2((float)num5, val9.Y), gray);
								DrawHelper.DrawLine(val2, (float)xPositionOnScreen + num + num9 + num8 * 2f, (float)yPositionOnScreen + num2, new Vector2((float)num5, val9.Y), gray);
								DrawHelper.DrawLine(val2, (float)xPositionOnScreen + num + val9.X, (float)yPositionOnScreen + num2, new Vector2((float)num5, val9.Y), gray);
								if (customField != null && customField.MayHaveLinks && customField.HasValue)
								{
									this.LinkableFieldAreas[customField] = new Rectangle((int)val7.X, (int)val7.Y, (int)val8.X, (int)val8.Y);
								}
								num2 += Math.Max(val6.Y, val8.Y);
							}
						}
					}
					this.MaxScroll = Math.Max(0, (int)(num2 - num4 + (float)this.CurrentScroll));
					if (this.MaxScroll > 0 && this.CurrentScroll > 0)
					{
						this.ScrollUpButton.draw(b);
					}
					if (this.MaxScroll > 0 && this.CurrentScroll < this.MaxScroll)
					{
						this.ScrollDownButton.draw(b);
					}
					val2.End();
				}
				catch (ArgumentException ex) when (!BaseMenu.UseSafeDimensions && ex.ParamName == "value" && (ex.StackTrace?.Contains("Microsoft.Xna.Framework.Graphics.GraphicsDevice.set_ScissorRectangle") ?? false))
				{
					this.Monitor.Log("The viewport size seems to be inaccurate. Enabling compatibility mode; lookup menu may be misaligned.", (LogLevel)3);
					this.Monitor.Log(ex.ToString(), (LogLevel)0);
					BaseMenu.UseSafeDimensions = true;
					this.UpdateLayout();
				}
				finally
				{
					graphicsDevice.ScissorRectangle = scissorRectangle;
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			((IClickableMenu)this).upperRightCloseButton.draw(b);
			((IClickableMenu)this).drawMouse(Game1.spriteBatch, false, -1);
		}, OnDrawError);
	}

	public void Dispose()
	{
		((GraphicsResource)this.ContentBlendState).Dispose();
		this.CleanupImpl();
	}

	private void UpdateLayout()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		Point viewport = base.GetViewportSize();
		if (this.ForceFullScreen)
		{
			((IClickableMenu)this).xPositionOnScreen = 0;
			((IClickableMenu)this).yPositionOnScreen = 0;
			((IClickableMenu)this).width = viewport.X;
			((IClickableMenu)this).height = viewport.Y;
		}
		else
		{
			((IClickableMenu)this).width = Math.Min(1280, viewport.X);
			((IClickableMenu)this).height = Math.Min((int)(this.AspectRatio.Y / this.AspectRatio.X * (float)((IClickableMenu)this).width), viewport.Y);
			Vector2 origin = default(Vector2);
			((Vector2)(ref origin))._002Ector((float)(viewport.X / 2 - ((IClickableMenu)this).width / 2), (float)(viewport.Y / 2 - ((IClickableMenu)this).height / 2));
			((IClickableMenu)this).xPositionOnScreen = (int)origin.X;
			((IClickableMenu)this).yPositionOnScreen = (int)origin.Y;
		}
		int x = ((IClickableMenu)this).xPositionOnScreen;
		int y = ((IClickableMenu)this).yPositionOnScreen;
		int gutter = this.ScrollButtonGutter;
		float contentHeight = ((IClickableMenu)this).height - gutter * 2;
		((ClickableComponent)this.ScrollUpButton).bounds = new Rectangle(x + gutter, (int)((float)y + contentHeight - (float)CommonSprites.Icons.UpArrow.Height - (float)gutter - (float)CommonSprites.Icons.DownArrow.Height), CommonSprites.Icons.UpArrow.Height, CommonSprites.Icons.UpArrow.Width);
		((ClickableComponent)this.ScrollDownButton).bounds = new Rectangle(x + gutter, (int)((float)y + contentHeight - (float)CommonSprites.Icons.DownArrow.Height), CommonSprites.Icons.DownArrow.Height, CommonSprites.Icons.DownArrow.Width);
		((IClickableMenu)this).initializeUpperRightCloseButton();
	}

	private void OnDrawError(Exception ex)
	{
		this.Monitor.InterceptErrors("handling an error in the lookup code", delegate
		{
			((IClickableMenu)this).exitThisMenu(true);
		});
	}

	protected override void cleanupBeforeExit()
	{
		this.CleanupImpl();
		((IClickableMenu)this).cleanupBeforeExit();
	}

	private void CleanupImpl()
	{
		Game1.displayHUD = this.WasHudEnabled;
	}
}
