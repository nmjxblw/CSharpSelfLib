using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley.NativeClipboard;

namespace StardewValley;

public class KeyboardDispatcher
{
	protected string _enteredText;

	protected List<char> _commandInputs = new List<char>();

	protected List<Keys> _keysDown = new List<Keys>();

	protected List<char> _charsEntered = new List<char>();

	protected GameWindow _window;

	protected KeyboardState _oldKeyboardState;

	private IKeyboardSubscriber _subscriber;

	private string _pasteResult = "";

	public IKeyboardSubscriber Subscriber
	{
		get
		{
			return this._subscriber;
		}
		set
		{
			if (this._subscriber != value)
			{
				if (this._subscriber != null)
				{
					this._subscriber.Selected = false;
				}
				this._subscriber = value;
				if (this._subscriber != null)
				{
					this._subscriber.Selected = true;
				}
			}
		}
	}

	public void Cleanup()
	{
		if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.Win32NT)
		{
			this._window.TextInput -= Event_TextInput;
		}
		else
		{
			KeyboardInput.CharEntered -= EventInput_CharEntered;
			KeyboardInput.KeyDown -= EventInput_KeyDown;
		}
		this._window = null;
	}

	public KeyboardDispatcher(GameWindow window)
	{
		this._commandInputs = new List<char>();
		this._keysDown = new List<Keys>();
		this._charsEntered = new List<char>();
		this._window = window;
		if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.Win32NT)
		{
			window.TextInput += Event_TextInput;
			return;
		}
		if (Game1.game1.IsMainInstance)
		{
			KeyboardInput.Initialize(window);
		}
		KeyboardInput.CharEntered += EventInput_CharEntered;
		KeyboardInput.KeyDown += EventInput_KeyDown;
	}

	private void Event_KeyDown(object sender, Keys key)
	{
		if (this._subscriber != null)
		{
			switch (key)
			{
			case Keys.Back:
				this._commandInputs.Add('\b');
				break;
			case Keys.Enter:
				this._commandInputs.Add('\r');
				break;
			case Keys.Tab:
				this._commandInputs.Add('\t');
				break;
			}
			this._keysDown.Add(key);
		}
	}

	private void Event_TextInput(object sender, TextInputEventArgs e)
	{
		if (this._subscriber == null)
		{
			return;
		}
		switch (e.Key)
		{
		case Keys.Back:
			this._commandInputs.Add('\b');
			return;
		case Keys.Enter:
			this._commandInputs.Add('\r');
			return;
		case Keys.Tab:
			this._commandInputs.Add('\t');
			return;
		}
		if (!char.IsControl(e.Character))
		{
			this._charsEntered.Add(e.Character);
		}
	}

	private void EventInput_KeyDown(object sender, KeyEventArgs e)
	{
		this._keysDown.Add(e.KeyCode);
	}

	private void EventInput_CharEntered(object sender, CharacterEventArgs e)
	{
		if (this._subscriber == null)
		{
			return;
		}
		if (char.IsControl(e.Character))
		{
			if (e.Character == '\u0016')
			{
				Thread thread = new Thread(PasteThread);
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
				thread.Join();
				this._enteredText = this._pasteResult;
			}
			else
			{
				this._commandInputs.Add(e.Character);
			}
		}
		else
		{
			this._charsEntered.Add(e.Character);
		}
	}

	public bool ShouldSuppress()
	{
		return false;
	}

	public void Discard()
	{
		this._enteredText = null;
		this._charsEntered.Clear();
		this._commandInputs.Clear();
		this._keysDown.Clear();
	}

	public void Poll()
	{
		KeyboardState keyboard_state = Game1.input.GetKeyboardState();
		bool modifier_held = ((SdlClipboard.Platform != ClipboardPlatformType.OSX) ? (keyboard_state.IsKeyDown(Keys.LeftControl) || keyboard_state.IsKeyDown(Keys.RightControl)) : (keyboard_state.IsKeyDown(Keys.LeftWindows) || keyboard_state.IsKeyDown(Keys.RightWindows)));
		if (keyboard_state.IsKeyDown(Keys.V) && !this._oldKeyboardState.IsKeyDown(Keys.V) && modifier_held)
		{
			string pasted_text = null;
			DesktopClipboard.GetText(ref pasted_text);
			if (pasted_text != null)
			{
				this._enteredText = pasted_text;
			}
		}
		this._oldKeyboardState = keyboard_state;
		if (this._enteredText != null)
		{
			if (this._subscriber != null && !this.ShouldSuppress())
			{
				this._subscriber.RecieveTextInput(this._enteredText);
			}
			this._enteredText = null;
		}
		if (this._charsEntered.Count > 0)
		{
			if (this._subscriber != null && !this.ShouldSuppress())
			{
				foreach (char key in this._charsEntered)
				{
					this._subscriber.RecieveTextInput(key);
					if (this._subscriber == null)
					{
						break;
					}
				}
			}
			this._charsEntered.Clear();
		}
		if (this._commandInputs.Count > 0)
		{
			if (this._subscriber != null && !this.ShouldSuppress())
			{
				foreach (char key2 in this._commandInputs)
				{
					this._subscriber.RecieveCommandInput(key2);
					if (this._subscriber == null)
					{
						break;
					}
				}
			}
			this._commandInputs.Clear();
		}
		if (this._keysDown.Count <= 0)
		{
			return;
		}
		if (this._subscriber != null && !this.ShouldSuppress())
		{
			foreach (Keys key3 in this._keysDown)
			{
				this._subscriber.RecieveSpecialInput(key3);
				if (this._subscriber == null)
				{
					break;
				}
			}
		}
		this._keysDown.Clear();
	}

	[STAThread]
	private void PasteThread()
	{
		this._pasteResult = "";
	}
}
