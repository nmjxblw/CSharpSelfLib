using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework;

internal abstract class GamePlatform : IDisposable
{
	protected TimeSpan _inactiveSleepTime = TimeSpan.FromMilliseconds(20.0);

	protected bool _needsToResetElapsedTime;

	private bool disposed;

	protected bool InFullScreenMode;

	private bool _isActive;

	private bool _isMouseVisible;

	private GameWindow _window;

	protected bool IsDisposed => this.disposed;

	/// <summary>
	/// When implemented in a derived class, reports the default
	/// GameRunBehavior for this platform.
	/// </summary>
	public abstract GameRunBehavior DefaultRunBehavior { get; }

	/// <summary>
	/// Gets the Game instance that owns this GamePlatform instance.
	/// </summary>
	public Game Game { get; private set; }

	public bool IsActive
	{
		get
		{
			return this._isActive;
		}
		internal set
		{
			if (this._isActive != value)
			{
				this._isActive = value;
				EventHelpers.Raise(this, this._isActive ? this.Activated : this.Deactivated, EventArgs.Empty);
			}
		}
	}

	public bool IsMouseVisible
	{
		get
		{
			return this._isMouseVisible;
		}
		set
		{
			if (this._isMouseVisible != value)
			{
				this._isMouseVisible = value;
				this.OnIsMouseVisibleChanged();
			}
		}
	}

	public GameWindow Window
	{
		get
		{
			return this._window;
		}
		protected set
		{
			if (this._window == null)
			{
				Mouse.PrimaryWindow = value;
				TouchPanel.PrimaryWindow = value;
			}
			this._window = value;
		}
	}

	public event EventHandler<EventArgs> AsyncRunLoopEnded;

	public event EventHandler<EventArgs> Activated;

	public event EventHandler<EventArgs> Deactivated;

	protected GamePlatform(Game game)
	{
		if (game == null)
		{
			throw new ArgumentNullException("game");
		}
		this.Game = game;
	}

	~GamePlatform()
	{
		this.Dispose(disposing: false);
	}

	/// <summary>
	/// Raises the AsyncRunLoopEnded event.  This method must be called by
	/// derived classes when the asynchronous run loop they start has
	/// stopped running.
	/// </summary>
	protected void RaiseAsyncRunLoopEnded()
	{
		EventHelpers.Raise(this, this.AsyncRunLoopEnded, EventArgs.Empty);
	}

	/// <summary>
	/// Gives derived classes an opportunity to do work before any
	/// components are initialized.  Note that the base implementation sets
	/// IsActive to true, so derived classes should either call the base
	/// implementation or set IsActive to true by their own means.
	/// </summary>
	public virtual void BeforeInitialize()
	{
		this.IsActive = true;
	}

	/// <summary>
	/// Gives derived classes an opportunity to do work just before the
	/// run loop is begun.  Implementations may also return false to prevent
	/// the run loop from starting.
	/// </summary>
	/// <returns></returns>
	public virtual bool BeforeRun()
	{
		return true;
	}

	/// <summary>
	/// When implemented in a derived, ends the active run loop.
	/// </summary>
	public abstract void Exit();

	/// <summary>
	/// When implemented in a derived, starts the run loop and blocks
	/// until it has ended.
	/// </summary>
	public abstract void RunLoop();

	/// <summary>
	/// When implemented in a derived, starts the run loop and returns
	/// immediately.
	/// </summary>
	public abstract void StartRunLoop();

	/// <summary>
	/// Gives derived classes an opportunity to do work just before Update
	/// is called for all IUpdatable components.  Returning false from this
	/// method will result in this round of Update calls being skipped.
	/// </summary>
	/// <param name="gameTime"></param>
	/// <returns></returns>
	public abstract bool BeforeUpdate(GameTime gameTime);

	/// <summary>
	/// Gives derived classes an opportunity to do work just before Draw
	/// is called for all IDrawable components.  Returning false from this
	/// method will result in this round of Draw calls being skipped.
	/// </summary>
	/// <param name="gameTime"></param>
	/// <returns></returns>
	public abstract bool BeforeDraw(GameTime gameTime);

	/// <summary>
	/// When implemented in a derived class, causes the game to enter
	/// full-screen mode.
	/// </summary>
	public abstract void EnterFullScreen();

	/// <summary>
	/// When implemented in a derived class, causes the game to exit
	/// full-screen mode.
	/// </summary>
	public abstract void ExitFullScreen();

	/// <summary>
	/// Gives derived classes an opportunity to modify
	/// Game.TargetElapsedTime before it is set.
	/// </summary>
	/// <param name="value">The proposed new value of TargetElapsedTime.</param>
	/// <returns>The new value of TargetElapsedTime that will be set.</returns>
	public virtual TimeSpan TargetElapsedTimeChanging(TimeSpan value)
	{
		return value;
	}

	/// <summary>
	/// Starts a device transition (windowed to full screen or vice versa).
	/// </summary>
	/// <param name="willBeFullScreen">
	/// Specifies whether the device will be in full-screen mode upon completion of the change.
	/// </param>
	public abstract void BeginScreenDeviceChange(bool willBeFullScreen);

	/// <summary>
	/// Completes a device transition.
	/// </summary>
	/// <param name="screenDeviceName">
	/// Screen device name.
	/// </param>
	/// <param name="clientWidth">
	/// The new width of the game's client window.
	/// </param>
	/// <param name="clientHeight">
	/// The new height of the game's client window.
	/// </param>
	public abstract void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight);

	/// <summary>
	/// Gives derived classes an opportunity to take action after
	/// Game.TargetElapsedTime has been set.
	/// </summary>
	public virtual void TargetElapsedTimeChanged()
	{
	}

	/// <summary>
	/// MSDN: Use this method if your game is recovering from a slow-running state, and ElapsedGameTime is too large to be useful.
	/// Frame timing is generally handled by the Game class, but some platforms still handle it elsewhere. Once all platforms
	/// rely on the Game class's functionality, this method and any overrides should be removed.
	/// </summary>
	public virtual void ResetElapsedTime()
	{
	}

	public virtual void Present()
	{
	}

	protected virtual void OnIsMouseVisibleChanged()
	{
	}

	/// <summary>
	/// Called by the GraphicsDeviceManager to notify the platform
	/// that the presentation parameters have changed.
	/// </summary>
	/// <param name="pp">The new presentation parameters.</param>
	internal virtual void OnPresentationChanged(PresentationParameters pp)
	{
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing,
	/// releasing, or resetting unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!this.disposed)
		{
			Mouse.PrimaryWindow = null;
			TouchPanel.PrimaryWindow = null;
			this.disposed = true;
		}
	}

	/// <summary>
	/// Log the specified Message.
	/// </summary>
	/// <param name="Message">
	///
	/// </param>
	[Conditional("DEBUG")]
	public virtual void Log(string Message)
	{
	}

	internal static GamePlatform PlatformCreate(Game game)
	{
		return new SdlGamePlatform(game);
	}
}
