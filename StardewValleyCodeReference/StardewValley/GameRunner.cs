using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley;

public class GameRunner : Game
{
	public static GameRunner instance;

	public List<Game1> gameInstances = new List<Game1>();

	public List<Game1> gameInstancesToRemove = new List<Game1>();

	public Game1 gamePtr;

	public bool shouldLoadContent;

	protected bool _initialized;

	protected bool _windowSizeChanged;

	public List<int> startButtonState = new List<int>();

	public List<KeyValuePair<Game1, IEnumerator<int>>> activeNewDayProcesses = new List<KeyValuePair<Game1, IEnumerator<int>>>();

	public int nextInstanceId;

	public static int MaxTextureSize = 4096;

	public GameRunner()
	{
		Program.sdk.EarlyInitialize();
		if (!Program.releaseBuild)
		{
			base.InactiveSleepTime = new TimeSpan(0L);
		}
		Game1.graphics = new GraphicsDeviceManager(this);
		Game1.graphics.PreparingDeviceSettings += delegate(object? sender, PreparingDeviceSettingsEventArgs args)
		{
			args.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
		};
		Game1.graphics.PreferredBackBufferWidth = 1280;
		Game1.graphics.PreferredBackBufferHeight = 720;
		base.Content.RootDirectory = "Content";
		SpriteBatch.TextureTuckAmount = 0.001f;
		LocalMultiplayer.Initialize();
		ItemRegistry.RegisterItemTypes();
		GameRunner.MaxTextureSize = int.MaxValue;
		base.Window.AllowUserResizing = true;
		this.SubscribeClientSizeChange();
		base.Exiting += delegate(object? sender, EventArgs args)
		{
			this.ExecuteForInstances(delegate(Game1 instance)
			{
				instance.exitEvent(sender, args);
			});
			Process.GetCurrentProcess().Kill();
		};
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		LocalizedContentManager.OnLanguageChange += delegate
		{
			this.ExecuteForInstances(delegate(Game1 instance)
			{
				instance.TranslateFields();
			});
		};
		DebugTools.GameConstructed(this);
	}

	protected override void OnActivated(object sender, EventArgs args)
	{
		this.ExecuteForInstances(delegate(Game1 instance)
		{
			instance.Instance_OnActivated(sender, args);
		});
	}

	public void SubscribeClientSizeChange()
	{
		base.Window.ClientSizeChanged += OnWindowSizeChange;
	}

	public void OnWindowSizeChange(object sender, EventArgs args)
	{
		base.Window.ClientSizeChanged -= OnWindowSizeChange;
		this._windowSizeChanged = true;
	}

	protected override void Draw(GameTime gameTime)
	{
		if (this._windowSizeChanged)
		{
			this.ExecuteForInstances(delegate(Game1 game)
			{
				game.Window_ClientSizeChanged(null, null);
			});
			this._windowSizeChanged = false;
			this.SubscribeClientSizeChange();
		}
		foreach (Game1 instance in this.gameInstances)
		{
			GameRunner.LoadInstance(instance);
			Viewport old_viewport = base.GraphicsDevice.Viewport;
			Game1.graphics.GraphicsDevice.Viewport = new Viewport(0, 0, Math.Min(instance.localMultiplayerWindow.Width, Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth), Math.Min(instance.localMultiplayerWindow.Height, Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight));
			instance.Instance_Draw(gameTime);
			base.GraphicsDevice.Viewport = old_viewport;
			GameRunner.SaveInstance(instance);
		}
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			base.GraphicsDevice.Clear(Game1.bgColor);
			foreach (Game1 gameInstance in this.gameInstances)
			{
				Game1.isRenderingScreenBuffer = true;
				gameInstance.DrawSplitScreenWindow();
				Game1.isRenderingScreenBuffer = false;
			}
		}
		base.Draw(gameTime);
	}

	public int GetNewInstanceID()
	{
		return this.nextInstanceId++;
	}

	protected override void Initialize()
	{
		DebugTools.BeforeGameInitialize(this);
		this.InitializeMainInstance();
		base.IsFixedTimeStep = true;
		base.Initialize();
		Game1.graphics.SynchronizeWithVerticalRetrace = true;
		Program.sdk.Initialize();
	}

	public bool WasWindowSizeChanged()
	{
		return this._windowSizeChanged;
	}

	public int GetMaxSimultaneousPlayers()
	{
		return 4;
	}

	public void InitializeMainInstance()
	{
		this.gameInstances = new List<Game1>();
		this.AddGameInstance(PlayerIndex.One);
	}

	public virtual void ExecuteForInstances(Action<Game1> action)
	{
		Game1 old_game1 = Game1.game1;
		if (old_game1 != null)
		{
			GameRunner.SaveInstance(old_game1);
		}
		foreach (Game1 instance in this.gameInstances)
		{
			GameRunner.LoadInstance(instance);
			action(instance);
			GameRunner.SaveInstance(instance);
		}
		if (old_game1 != null)
		{
			GameRunner.LoadInstance(old_game1);
		}
		else
		{
			Game1.game1 = null;
		}
	}

	public virtual void RemoveGameInstance(Game1 instance)
	{
		if (this.gameInstances.Contains(instance) && !this.gameInstancesToRemove.Contains(instance))
		{
			this.gameInstancesToRemove.Add(instance);
		}
	}

	public virtual void AddGameInstance(PlayerIndex player_index)
	{
		Game1 old_game1 = Game1.game1;
		if (old_game1 != null)
		{
			GameRunner.SaveInstance(old_game1, force: true);
		}
		if (this.gameInstances.Count > 0)
		{
			Game1 game = this.gameInstances[0];
			GameRunner.LoadInstance(game);
			Game1.StartLocalMultiplayerIfNecessary();
			GameRunner.SaveInstance(game, force: true);
		}
		Game1 new_instance = ((this.gameInstances.Count == 0) ? this.CreateGameInstance() : this.CreateGameInstance(player_index, this.gameInstances.Count));
		this.gameInstances.Add(new_instance);
		if (this.gamePtr == null)
		{
			this.gamePtr = new_instance;
		}
		if (this.gameInstances.Count > 0)
		{
			new_instance.staticVarHolder = Activator.CreateInstance(LocalMultiplayer.StaticVarHolderType);
			GameRunner.SetInstanceDefaults(new_instance);
			GameRunner.LoadInstance(new_instance);
		}
		Game1.game1 = new_instance;
		new_instance.Instance_Initialize();
		if (this.shouldLoadContent)
		{
			new_instance.Instance_LoadContent();
		}
		GameRunner.SaveInstance(new_instance);
		if (old_game1 != null)
		{
			GameRunner.LoadInstance(old_game1);
		}
		else
		{
			Game1.game1 = null;
		}
		this._windowSizeChanged = true;
	}

	public virtual Game1 CreateGameInstance(PlayerIndex player_index = PlayerIndex.One, int index = 0)
	{
		return new Game1(player_index, index);
	}

	protected override void LoadContent()
	{
		Game1.graphics.PreferredBackBufferWidth = 1280;
		Game1.graphics.PreferredBackBufferHeight = 720;
		Game1.graphics.ApplyChanges();
		GameRunner.LoadInstance(this.gamePtr);
		this.gamePtr.Instance_LoadContent();
		GameRunner.SaveInstance(this.gamePtr);
		DebugTools.GameLoadContent(this);
		foreach (Game1 instance in this.gameInstances)
		{
			if (instance != this.gamePtr)
			{
				GameRunner.LoadInstance(instance);
				instance.Instance_LoadContent();
				GameRunner.SaveInstance(instance);
			}
		}
		this.shouldLoadContent = true;
		base.LoadContent();
	}

	protected override void UnloadContent()
	{
		this.gamePtr.Instance_UnloadContent();
		base.UnloadContent();
	}

	protected override void Update(GameTime gameTime)
	{
		GameStateQuery.Update();
		for (int i = 0; i < this.activeNewDayProcesses.Count; i++)
		{
			KeyValuePair<Game1, IEnumerator<int>> active_new_days = this.activeNewDayProcesses[i];
			Game1 instance = this.activeNewDayProcesses[i].Key;
			GameRunner.LoadInstance(instance);
			if (!active_new_days.Value.MoveNext())
			{
				instance.isLocalMultiplayerNewDayActive = false;
				this.activeNewDayProcesses.RemoveAt(i);
				i--;
				Utility.CollectGarbage();
			}
			GameRunner.SaveInstance(instance);
		}
		while (this.startButtonState.Count < 4)
		{
			this.startButtonState.Add(-1);
		}
		for (PlayerIndex player_index = PlayerIndex.One; player_index <= PlayerIndex.Four; player_index++)
		{
			if (GamePad.GetState(player_index).IsButtonDown(Buttons.Start))
			{
				if (this.startButtonState[(int)player_index] >= 0)
				{
					this.startButtonState[(int)player_index]++;
				}
			}
			else
			{
				this.startButtonState[(int)player_index] = 0;
			}
		}
		for (int j = 0; j < this.gameInstances.Count; j++)
		{
			Game1 instance2 = this.gameInstances[j];
			GameRunner.LoadInstance(instance2);
			if (j == 0)
			{
				PlayerIndex start_player_index = PlayerIndex.Two;
				if (instance2.instanceOptions.gamepadMode == Options.GamepadModes.ForceOff)
				{
					start_player_index = PlayerIndex.One;
				}
				for (PlayerIndex player_index2 = start_player_index; player_index2 <= PlayerIndex.Four; player_index2++)
				{
					bool fail = false;
					foreach (Game1 gameInstance in this.gameInstances)
					{
						if (gameInstance.instancePlayerOneIndex == player_index2)
						{
							fail = true;
							break;
						}
					}
					if (!fail && instance2.IsLocalCoopJoinable() && this.IsStartDown(player_index2) && instance2.ShowLocalCoopJoinMenu())
					{
						this.InvalidateStartPress(player_index2);
					}
				}
			}
			else
			{
				Game1.options.gamepadMode = Options.GamepadModes.ForceOn;
			}
			Game1.debugTimings.StartUpdateTimer();
			instance2.Instance_Update(gameTime);
			Game1.debugTimings.StopUpdateTimer();
			GameRunner.SaveInstance(instance2);
		}
		if (this.gameInstancesToRemove.Count > 0)
		{
			foreach (Game1 instance3 in this.gameInstancesToRemove)
			{
				GameRunner.LoadInstance(instance3);
				instance3.exitEvent(null, null);
				this.gameInstances.Remove(instance3);
				Game1.game1 = null;
			}
			for (int k = 0; k < this.gameInstances.Count; k++)
			{
				this.gameInstances[k].instanceIndex = k;
			}
			if (this.gameInstances.Count == 1)
			{
				Game1 game = this.gameInstances[0];
				GameRunner.LoadInstance(game, force: true);
				game.staticVarHolder = null;
				Game1.EndLocalMultiplayer();
			}
			bool controller_1_assigned = false;
			if (this.gameInstances.Count > 0)
			{
				foreach (Game1 gameInstance2 in this.gameInstances)
				{
					if (gameInstance2.instancePlayerOneIndex == PlayerIndex.One)
					{
						controller_1_assigned = true;
						break;
					}
				}
				if (!controller_1_assigned)
				{
					this.gameInstances[0].instancePlayerOneIndex = PlayerIndex.One;
				}
			}
			this.gameInstancesToRemove.Clear();
			this._windowSizeChanged = true;
		}
		base.Update(gameTime);
	}

	public virtual void InvalidateStartPress(PlayerIndex index)
	{
		if (index >= PlayerIndex.One && (int)index < this.startButtonState.Count)
		{
			this.startButtonState[(int)index] = -1;
		}
	}

	public virtual bool IsStartDown(PlayerIndex index)
	{
		if (index >= PlayerIndex.One && (int)index < this.startButtonState.Count)
		{
			return this.startButtonState[(int)index] == 1;
		}
		return false;
	}

	private static void SetInstanceDefaults(InstanceGame instance)
	{
		for (int i = 0; i < LocalMultiplayer.staticDefaults.Count; i++)
		{
			object value = LocalMultiplayer.staticDefaults[i]?.DeepClone();
			LocalMultiplayer.staticFields[i].SetValue(null, value);
		}
		GameRunner.SaveInstance(instance);
	}

	public static void SaveInstance(InstanceGame instance, bool force = false)
	{
		if (force || LocalMultiplayer.IsLocalMultiplayer())
		{
			if (instance.staticVarHolder == null)
			{
				instance.staticVarHolder = Activator.CreateInstance(LocalMultiplayer.StaticVarHolderType);
			}
			LocalMultiplayer.StaticSave(instance.staticVarHolder);
		}
	}

	public static void LoadInstance(InstanceGame instance, bool force = false)
	{
		Game1.game1 = instance as Game1;
		if ((force || LocalMultiplayer.IsLocalMultiplayer()) && instance.staticVarHolder != null)
		{
			LocalMultiplayer.StaticLoad(instance.staticVarHolder);
			if (Game1.player != null && Game1.player.isCustomized.Value && Game1.splitscreenOptions.TryGetValue(Game1.player.UniqueMultiplayerID, out var options))
			{
				Game1.options = options;
			}
		}
	}
}
