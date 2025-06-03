using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using xTile.Dimensions;

namespace Pathoschild.Stardew.Common.UI;

internal abstract class BaseOverlay : IDisposable
{
	private readonly IModEvents Events;

	protected readonly IInputHelper InputHelper;

	protected readonly IReflectionHelper Reflection;

	private readonly int ScreenId;

	private Rectangle LastViewport;

	private readonly Func<bool>? KeepAliveCheck;

	private readonly bool? AssumeUiMode;

	public virtual void Dispose()
	{
		this.Events.Display.RenderedActiveMenu -= OnRendered;
		this.Events.Display.RenderedWorld -= OnRenderedWorld;
		this.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
		this.Events.Input.ButtonPressed -= OnButtonPressed;
		this.Events.Input.ButtonsChanged -= OnButtonsChanged;
		this.Events.Input.CursorMoved -= OnCursorMoved;
		this.Events.Input.MouseWheelScrolled -= OnMouseWheelScrolled;
	}

	protected BaseOverlay(IModEvents events, IInputHelper inputHelper, IReflectionHelper reflection, Func<bool>? keepAlive = null, bool? assumeUiMode = null)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		this.Events = events;
		this.InputHelper = inputHelper;
		this.Reflection = reflection;
		this.KeepAliveCheck = keepAlive;
		this.LastViewport = new Rectangle(((Rectangle)(ref Game1.uiViewport)).X, ((Rectangle)(ref Game1.uiViewport)).Y, ((Rectangle)(ref Game1.uiViewport)).Width, ((Rectangle)(ref Game1.uiViewport)).Height);
		this.ScreenId = Context.ScreenId;
		this.AssumeUiMode = assumeUiMode;
		events.GameLoop.UpdateTicked += OnUpdateTicked;
		if (this.IsMethodOverridden("DrawUi"))
		{
			events.Display.RenderedActiveMenu += OnRendered;
		}
		if (this.IsMethodOverridden("DrawWorld"))
		{
			events.Display.RenderedWorld += OnRenderedWorld;
		}
		if (this.IsMethodOverridden("ReceiveLeftClick"))
		{
			events.Input.ButtonPressed += OnButtonPressed;
		}
		if (this.IsMethodOverridden("ReceiveButtonsChanged"))
		{
			events.Input.ButtonsChanged += OnButtonsChanged;
		}
		if (this.IsMethodOverridden("ReceiveCursorHover"))
		{
			events.Input.CursorMoved += OnCursorMoved;
		}
		if (this.IsMethodOverridden("ReceiveScrollWheelAction"))
		{
			events.Input.MouseWheelScrolled += OnMouseWheelScrolled;
		}
	}

	protected virtual void Update()
	{
	}

	protected virtual void DrawUi(SpriteBatch batch)
	{
	}

	protected virtual void DrawWorld(SpriteBatch batch)
	{
	}

	protected virtual bool ReceiveLeftClick(int x, int y)
	{
		return false;
	}

	protected virtual void ReceiveButtonsChanged(object? sender, ButtonsChangedEventArgs e)
	{
	}

	protected virtual bool ReceiveScrollWheelAction(int amount)
	{
		return false;
	}

	protected virtual bool ReceiveCursorHover(int x, int y)
	{
		return false;
	}

	protected virtual void ReceiveGameWindowResized()
	{
	}

	protected void DrawCursor()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		if (!Game1.options.hardwareCursor)
		{
			Vector2 cursorPos = default(Vector2);
			((Vector2)(ref cursorPos))._002Ector((float)Game1.getMouseX(), (float)Game1.getMouseY());
			if ((int)Constants.TargetPlatform == 0)
			{
				cursorPos *= Game1.options.zoomLevel / this.Reflection.GetProperty<float>(typeof(Game1), "NativeZoomLevel", true).GetValue();
			}
			Game1.spriteBatch.Draw(Game1.mouseCursors, cursorPos, (Rectangle?)Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.options.SnappyMenus ? 44 : 0, 16, 16), Color.White * Game1.mouseCursorTransparency, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, (SpriteEffects)0, 1f);
		}
	}

	private void OnRendered(object? sender, RenderedActiveMenuEventArgs e)
	{
		if (Context.ScreenId == this.ScreenId)
		{
			this.DrawUi(Game1.spriteBatch);
		}
	}

	private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
	{
		if (Context.ScreenId == this.ScreenId)
		{
			this.DrawWorld(e.SpriteBatch);
		}
	}

	private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if (Context.ScreenId != this.ScreenId)
		{
			if (!Context.HasScreenId(this.ScreenId))
			{
				this.Dispose();
			}
			return;
		}
		if (this.KeepAliveCheck != null && !this.KeepAliveCheck())
		{
			this.Dispose();
			return;
		}
		Rectangle newViewport = Game1.uiViewport;
		if (((Rectangle)(ref this.LastViewport)).Width != ((Rectangle)(ref newViewport)).Width || ((Rectangle)(ref this.LastViewport)).Height != ((Rectangle)(ref newViewport)).Height)
		{
			((Rectangle)(ref newViewport))._002Ector(((Rectangle)(ref newViewport)).X, ((Rectangle)(ref newViewport)).Y, ((Rectangle)(ref newViewport)).Width, ((Rectangle)(ref newViewport)).Height);
			this.ReceiveGameWindowResized();
			this.LastViewport = newViewport;
		}
		this.Update();
	}

	private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
	{
		if (Context.ScreenId == this.ScreenId)
		{
			this.ReceiveButtonsChanged(sender, e);
		}
	}

	private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		if (Context.ScreenId == this.ScreenId && ((int)e.Button == 1000 || SButtonExtensions.IsUseToolButton(e.Button)))
		{
			bool uiMode = this.AssumeUiMode ?? Game1.uiMode;
			bool handled;
			if ((int)Constants.TargetPlatform == 0)
			{
				float nativeZoomLevel = this.Reflection.GetProperty<float>(typeof(Game1), "NativeZoomLevel", true).GetValue();
				handled = this.ReceiveLeftClick((int)((float)Game1.getMouseX() * Game1.options.zoomLevel / nativeZoomLevel), (int)((float)Game1.getMouseY() * Game1.options.zoomLevel / nativeZoomLevel));
			}
			else
			{
				handled = this.ReceiveLeftClick(Game1.getMouseX(uiMode), Game1.getMouseY(uiMode));
			}
			if (handled)
			{
				this.InputHelper.Suppress(e.Button);
			}
		}
	}

	private void OnMouseWheelScrolled(object? sender, MouseWheelScrolledEventArgs e)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (Context.ScreenId == this.ScreenId && this.ReceiveScrollWheelAction(e.Delta))
		{
			MouseState cur = Game1.oldMouseState;
			Game1.oldMouseState = new MouseState(((MouseState)(ref cur)).X, ((MouseState)(ref cur)).Y, e.NewValue, ((MouseState)(ref cur)).LeftButton, ((MouseState)(ref cur)).MiddleButton, ((MouseState)(ref cur)).RightButton, ((MouseState)(ref cur)).XButton1, ((MouseState)(ref cur)).XButton2);
		}
	}

	private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
	{
		if (Context.ScreenId == this.ScreenId)
		{
			bool uiMode = this.AssumeUiMode ?? Game1.uiMode;
			if (this.ReceiveCursorHover(Game1.getMouseX(uiMode), Game1.getMouseY(uiMode)))
			{
				Game1.InvalidateOldMouseMovement();
			}
		}
	}

	private bool IsMethodOverridden(string name)
	{
		MethodInfo method = base.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (method == null)
		{
			throw new InvalidOperationException($"Can't find method {base.GetType().FullName}.{name}.");
		}
		return method.DeclaringType != typeof(BaseOverlay);
	}
}
