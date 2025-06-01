using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework;

internal class SdlGamePlatform : GamePlatform
{
	private readonly Game _game;

	private readonly List<Keys> _keys;

	private int _isExiting;

	private SdlGameWindow _view;

	public override GameRunBehavior DefaultRunBehavior => GameRunBehavior.Synchronous;

	public SdlGamePlatform(Game game)
		: base(game)
	{
		this._game = game;
		this._keys = new List<Keys>();
		Keyboard.SetKeys(this._keys);
		Sdl.GetVersion(out Sdl.version);
		Sdl.Major = Sdl.version.Major;
		Sdl.Minor = Sdl.version.Minor;
		Sdl.Patch = Sdl.version.Patch;
		Sdl.Version minVersion = new Sdl.Version
		{
			Major = 2,
			Minor = 0,
			Patch = 5
		};
		_ = Sdl.version < minVersion;
		if (Sdl.version >= minVersion && CurrentPlatform.OS == OS.Windows && Debugger.IsAttached)
		{
			Sdl.SetHint("SDL_WINDOWS_DISABLE_THREAD_NAMING", "1");
		}
		Sdl.Init(12832);
		Sdl.DisableScreenSaver();
		GamePad.InitDatabase();
		base.Window = (this._view = new SdlGameWindow(this._game));
	}

	public override void BeforeInitialize()
	{
		this.SdlRunLoop();
		base.BeforeInitialize();
	}

	protected override void OnIsMouseVisibleChanged()
	{
		this._view.SetCursorVisible(this._game.IsMouseVisible);
	}

	internal override void OnPresentationChanged(PresentationParameters pp)
	{
		string displayName = Sdl.Display.GetDisplayName(Sdl.Window.GetDisplayIndex(base.Window.Handle));
		this.BeginScreenDeviceChange(pp.IsFullScreen);
		this.EndScreenDeviceChange(displayName, pp.BackBufferWidth, pp.BackBufferHeight);
	}

	public override void RunLoop()
	{
		Sdl.Window.Show(base.Window.Handle);
		do
		{
			this.SdlRunLoop();
			base.Game.Tick();
			Threading.Run();
			GraphicsDevice.DisposeContexts();
		}
		while (this._isExiting <= 0);
	}

	private unsafe void SdlRunLoop()
	{
		Sdl.Event ev;
		while (Sdl.PollEvent(out ev) == 1)
		{
			switch (ev.Type)
			{
			case Sdl.EventType.Quit:
				this._isExiting++;
				break;
			case Sdl.EventType.JoyDeviceAdded:
				Joystick.AddDevices();
				break;
			case Sdl.EventType.JoyDeviceRemoved:
				Joystick.RemoveDevice(ev.JoystickDevice.Which);
				break;
			case Sdl.EventType.ControllerDeviceRemoved:
				GamePad.RemoveDevice(ev.ControllerDevice.Which);
				break;
			case Sdl.EventType.ControllerAxisMotion:
			case Sdl.EventType.ControllerButtonDown:
			case Sdl.EventType.ControllerButtonUp:
				GamePad.UpdatePacketInfo(ev.ControllerDevice.Which, ev.ControllerDevice.TimeStamp);
				break;
			case Sdl.EventType.MouseWheel:
				Mouse.ScrollY += ev.Wheel.Y * 120;
				Mouse.ScrollX += ev.Wheel.X * 120;
				break;
			case Sdl.EventType.KeyDown:
			{
				Keys key2 = KeyboardUtil.ToXna(ev.Key.Keysym.Sym);
				if (!this._keys.Contains(key2))
				{
					this._keys.Add(key2);
				}
				char character = (char)ev.Key.Keysym.Sym;
				this._view.OnKeyDown(new InputKeyEventArgs(key2));
				if (char.IsControl(character))
				{
					this._view.OnTextInput(new TextInputEventArgs(character, key2));
				}
				break;
			}
			case Sdl.EventType.KeyUp:
			{
				Keys key = KeyboardUtil.ToXna(ev.Key.Keysym.Sym);
				this._keys.Remove(key);
				this._view.OnKeyUp(new InputKeyEventArgs(key));
				break;
			}
			case Sdl.EventType.TextInput:
			{
				if (!this._view.IsTextInputHandled)
				{
					break;
				}
				int len = 0;
				int utf8character = 0;
				byte currentByte = 0;
				int charByteSize = 0;
				int remainingShift = 0;
				while ((currentByte = Marshal.ReadByte((IntPtr)ev.Text.Text, len)) != 0)
				{
					if (charByteSize == 0)
					{
						charByteSize = ((currentByte < 192) ? 1 : ((currentByte < 224) ? 2 : ((currentByte >= 240) ? 4 : 3)));
						utf8character = 0;
						remainingShift = 4;
					}
					utf8character <<= 8;
					utf8character |= currentByte;
					charByteSize--;
					remainingShift--;
					if (charByteSize == 0)
					{
						utf8character <<= remainingShift * 8;
						int codepoint = this.UTF8ToUnicode(utf8character);
						if (codepoint >= 0 && codepoint < 65535)
						{
							this._view.OnTextInput(new TextInputEventArgs((char)codepoint, KeyboardUtil.ToXna(codepoint)));
						}
					}
					len++;
				}
				break;
			}
			case Sdl.EventType.WindowEvent:
				switch (ev.Window.EventID)
				{
				case Sdl.Window.EventId.Resized:
				case Sdl.Window.EventId.SizeChanged:
					this._view.ClientResize(ev.Window.Data1, ev.Window.Data2);
					break;
				case Sdl.Window.EventId.FocusGained:
					base.IsActive = true;
					break;
				case Sdl.Window.EventId.FocusLost:
					base.IsActive = false;
					break;
				case Sdl.Window.EventId.Moved:
					this._view.Moved(ev.Window.Data1, ev.Window.Data2);
					break;
				case Sdl.Window.EventId.Close:
					this._isExiting++;
					break;
				}
				break;
			}
		}
		this._view.OnEventsHandled();
	}

	private int UTF8ToUnicode(int utf8)
	{
		int byte4 = utf8 & 0xFF;
		int byte5 = (utf8 >> 8) & 0xFF;
		int byte6 = (utf8 >> 16) & 0xFF;
		int byte7 = (utf8 >> 24) & 0xFF;
		if (byte7 < 128)
		{
			return byte7;
		}
		if (byte7 < 192)
		{
			return -1;
		}
		if (byte7 < 224 && byte6 >= 128 && byte6 < 192)
		{
			return byte7 % 32 * 64 + byte6 % 64;
		}
		if (byte7 < 240 && byte6 >= 128 && byte6 < 192 && byte5 >= 128 && byte5 < 192)
		{
			return byte7 % 16 * 64 * 64 + byte6 % 64 * 64 + byte5 % 64;
		}
		if (byte7 < 248 && byte6 >= 128 && byte6 < 192 && byte5 >= 128 && byte5 < 192 && byte4 >= 128 && byte4 < 192)
		{
			return byte7 % 8 * 64 * 64 * 64 + byte6 % 64 * 64 * 64 + byte5 % 64 * 64 + byte4 % 64;
		}
		return -1;
	}

	public override void StartRunLoop()
	{
		throw new NotSupportedException("The desktop platform does not support asynchronous run loops");
	}

	public override void Exit()
	{
		Interlocked.Increment(ref this._isExiting);
	}

	public override bool BeforeUpdate(GameTime gameTime)
	{
		return true;
	}

	public override bool BeforeDraw(GameTime gameTime)
	{
		return true;
	}

	public override void EnterFullScreen()
	{
	}

	public override void ExitFullScreen()
	{
	}

	public override void BeginScreenDeviceChange(bool willBeFullScreen)
	{
		this._view.BeginScreenDeviceChange(willBeFullScreen);
	}

	public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
	{
		this._view.EndScreenDeviceChange(screenDeviceName, clientWidth, clientHeight);
	}

	public override void Log(string message)
	{
		Console.WriteLine(message);
	}

	public override void Present()
	{
		if (base.Game.GraphicsDevice != null)
		{
			base.Game.GraphicsDevice.Present();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (this._view != null)
		{
			this._view.Dispose();
			this._view = null;
			Joystick.CloseDevices();
			Sdl.Quit();
		}
		base.Dispose(disposing);
	}
}
