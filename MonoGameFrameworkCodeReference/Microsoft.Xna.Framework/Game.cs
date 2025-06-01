using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework;

/// <summary>
/// This class is the entry point for most games. Handles setting up
/// a window and graphics and runs a game loop that calls <see cref="M:Microsoft.Xna.Framework.Game.Update(Microsoft.Xna.Framework.GameTime)" /> and <see cref="M:Microsoft.Xna.Framework.Game.Draw(Microsoft.Xna.Framework.GameTime)" />.
/// </summary>
public class Game : IDisposable
{
	/// <summary>
	/// The SortingFilteringCollection class provides efficient, reusable
	/// sorting and filtering based on a configurable sort comparer, filter
	/// predicate, and associate change events.
	/// </summary>
	private class SortingFilteringCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable
	{
		private readonly List<T> _items;

		private readonly List<AddJournalEntry<T>> _addJournal;

		private readonly Comparison<AddJournalEntry<T>> _addJournalSortComparison;

		private readonly List<int> _removeJournal;

		private readonly List<T> _cachedFilteredItems;

		private bool _shouldRebuildCache;

		private readonly Predicate<T> _filter;

		private readonly Comparison<T> _sort;

		private readonly Action<T, EventHandler<EventArgs>> _filterChangedSubscriber;

		private readonly Action<T, EventHandler<EventArgs>> _filterChangedUnsubscriber;

		private readonly Action<T, EventHandler<EventArgs>> _sortChangedSubscriber;

		private readonly Action<T, EventHandler<EventArgs>> _sortChangedUnsubscriber;

		private static readonly Comparison<int> RemoveJournalSortComparison = (int x, int y) => Comparer<int>.Default.Compare(y, x);

		public int Count => this._items.Count;

		public bool IsReadOnly => false;

		public SortingFilteringCollection(Predicate<T> filter, Action<T, EventHandler<EventArgs>> filterChangedSubscriber, Action<T, EventHandler<EventArgs>> filterChangedUnsubscriber, Comparison<T> sort, Action<T, EventHandler<EventArgs>> sortChangedSubscriber, Action<T, EventHandler<EventArgs>> sortChangedUnsubscriber)
		{
			this._items = new List<T>();
			this._addJournal = new List<AddJournalEntry<T>>();
			this._removeJournal = new List<int>();
			this._cachedFilteredItems = new List<T>();
			this._shouldRebuildCache = true;
			this._filter = filter;
			this._filterChangedSubscriber = filterChangedSubscriber;
			this._filterChangedUnsubscriber = filterChangedUnsubscriber;
			this._sort = sort;
			this._sortChangedSubscriber = sortChangedSubscriber;
			this._sortChangedUnsubscriber = sortChangedUnsubscriber;
			this._addJournalSortComparison = CompareAddJournalEntry;
		}

		private int CompareAddJournalEntry(AddJournalEntry<T> x, AddJournalEntry<T> y)
		{
			int result = this._sort(x.Item, y.Item);
			if (result != 0)
			{
				return result;
			}
			return x.Order - y.Order;
		}

		public void ForEachFilteredItem<TUserData>(Action<T, TUserData> action, TUserData userData)
		{
			if (this._shouldRebuildCache)
			{
				this.ProcessRemoveJournal();
				this.ProcessAddJournal();
				this._cachedFilteredItems.Clear();
				for (int i = 0; i < this._items.Count; i++)
				{
					if (this._filter(this._items[i]))
					{
						this._cachedFilteredItems.Add(this._items[i]);
					}
				}
				this._shouldRebuildCache = false;
			}
			for (int j = 0; j < this._cachedFilteredItems.Count; j++)
			{
				action(this._cachedFilteredItems[j], userData);
			}
			if (this._shouldRebuildCache)
			{
				this._cachedFilteredItems.Clear();
			}
		}

		public void Add(T item)
		{
			this._addJournal.Add(new AddJournalEntry<T>(this._addJournal.Count, item));
			this.InvalidateCache();
		}

		public bool Remove(T item)
		{
			if (this._addJournal.Remove(AddJournalEntry<T>.CreateKey(item)))
			{
				return true;
			}
			int index = this._items.IndexOf(item);
			if (index >= 0)
			{
				this.UnsubscribeFromItemEvents(item);
				this._removeJournal.Add(index);
				this.InvalidateCache();
				return true;
			}
			return false;
		}

		public void Clear()
		{
			for (int i = 0; i < this._items.Count; i++)
			{
				this._filterChangedUnsubscriber(this._items[i], Item_FilterPropertyChanged);
				this._sortChangedUnsubscriber(this._items[i], Item_SortPropertyChanged);
			}
			this._addJournal.Clear();
			this._removeJournal.Clear();
			this._items.Clear();
			this.InvalidateCache();
		}

		public bool Contains(T item)
		{
			return this._items.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			this._items.CopyTo(array, arrayIndex);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this._items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)this._items).GetEnumerator();
		}

		private void ProcessRemoveJournal()
		{
			if (this._removeJournal.Count != 0)
			{
				this._removeJournal.Sort(SortingFilteringCollection<T>.RemoveJournalSortComparison);
				for (int i = 0; i < this._removeJournal.Count; i++)
				{
					this._items.RemoveAt(this._removeJournal[i]);
				}
				this._removeJournal.Clear();
			}
		}

		private void ProcessAddJournal()
		{
			if (this._addJournal.Count == 0)
			{
				return;
			}
			this._addJournal.Sort(this._addJournalSortComparison);
			int iAddJournal = 0;
			for (int iItems = 0; iItems < this._items.Count; iItems++)
			{
				if (iAddJournal >= this._addJournal.Count)
				{
					break;
				}
				T addJournalItem = this._addJournal[iAddJournal].Item;
				if (this._sort(addJournalItem, this._items[iItems]) < 0)
				{
					this.SubscribeToItemEvents(addJournalItem);
					this._items.Insert(iItems, addJournalItem);
					iAddJournal++;
				}
			}
			for (; iAddJournal < this._addJournal.Count; iAddJournal++)
			{
				T addJournalItem2 = this._addJournal[iAddJournal].Item;
				this.SubscribeToItemEvents(addJournalItem2);
				this._items.Add(addJournalItem2);
			}
			this._addJournal.Clear();
		}

		private void SubscribeToItemEvents(T item)
		{
			this._filterChangedSubscriber(item, Item_FilterPropertyChanged);
			this._sortChangedSubscriber(item, Item_SortPropertyChanged);
		}

		private void UnsubscribeFromItemEvents(T item)
		{
			this._filterChangedUnsubscriber(item, Item_FilterPropertyChanged);
			this._sortChangedUnsubscriber(item, Item_SortPropertyChanged);
		}

		private void InvalidateCache()
		{
			this._shouldRebuildCache = true;
		}

		private void Item_FilterPropertyChanged(object sender, EventArgs e)
		{
			this.InvalidateCache();
		}

		private void Item_SortPropertyChanged(object sender, EventArgs e)
		{
			T item = (T)sender;
			int index = this._items.IndexOf(item);
			this._addJournal.Add(new AddJournalEntry<T>(this._addJournal.Count, item));
			this._removeJournal.Add(index);
			this.UnsubscribeFromItemEvents(item);
			this.InvalidateCache();
		}
	}

	private struct AddJournalEntry<T>
	{
		public readonly int Order;

		public readonly T Item;

		public AddJournalEntry(int order, T item)
		{
			this.Order = order;
			this.Item = item;
		}

		public static AddJournalEntry<T> CreateKey(T item)
		{
			return new AddJournalEntry<T>(-1, item);
		}

		public override int GetHashCode()
		{
			return this.Item.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is AddJournalEntry<T>))
			{
				return false;
			}
			return object.Equals(this.Item, ((AddJournalEntry<T>)obj).Item);
		}
	}

	private GameComponentCollection _components;

	private GameServiceContainer _services;

	private ContentManager _content;

	internal GamePlatform Platform;

	private SortingFilteringCollection<IDrawable> _drawables = new SortingFilteringCollection<IDrawable>((IDrawable d) => d.Visible, delegate(IDrawable d, EventHandler<EventArgs> handler)
	{
		d.VisibleChanged += handler;
	}, delegate(IDrawable d, EventHandler<EventArgs> handler)
	{
		d.VisibleChanged -= handler;
	}, (IDrawable d1, IDrawable d2) => Comparer<int>.Default.Compare(d1.DrawOrder, d2.DrawOrder), delegate(IDrawable d, EventHandler<EventArgs> handler)
	{
		d.DrawOrderChanged += handler;
	}, delegate(IDrawable d, EventHandler<EventArgs> handler)
	{
		d.DrawOrderChanged -= handler;
	});

	private SortingFilteringCollection<IUpdateable> _updateables = new SortingFilteringCollection<IUpdateable>((IUpdateable u) => u.Enabled, delegate(IUpdateable u, EventHandler<EventArgs> handler)
	{
		u.EnabledChanged += handler;
	}, delegate(IUpdateable u, EventHandler<EventArgs> handler)
	{
		u.EnabledChanged -= handler;
	}, (IUpdateable u1, IUpdateable u2) => Comparer<int>.Default.Compare(u1.UpdateOrder, u2.UpdateOrder), delegate(IUpdateable u, EventHandler<EventArgs> handler)
	{
		u.UpdateOrderChanged += handler;
	}, delegate(IUpdateable u, EventHandler<EventArgs> handler)
	{
		u.UpdateOrderChanged -= handler;
	});

	private IGraphicsDeviceManager _graphicsDeviceManager;

	private IGraphicsDeviceService _graphicsDeviceService;

	private bool _initialized;

	private bool _isFixedTimeStep = true;

	private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(166667L);

	private TimeSpan _inactiveSleepTime = TimeSpan.FromSeconds(0.02);

	private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500.0);

	private bool _shouldExit;

	private bool _suppressDraw;

	private bool _isDisposed;

	private static Game _instance = null;

	private TimeSpan _accumulatedElapsedTime;

	private readonly GameTime _gameTime = new GameTime();

	private Stopwatch _gameTimer;

	private long _previousTicks;

	private int _updateFrameLag;

	private const int PREVIOUS_SLEEP_TIME_COUNT = 128;

	private const int SLEEP_TIME_MASK = 127;

	private TimeSpan[] _previousSleepTimes = new TimeSpan[128];

	private int _sleepTimeIndex;

	private TimeSpan _worstCaseSleepPrecision = TimeSpan.FromMilliseconds(1.0);

	private static readonly Action<IDrawable, GameTime> DrawAction = delegate(IDrawable drawable, GameTime gameTime)
	{
		drawable.Draw(gameTime);
	};

	private static readonly Action<IUpdateable, GameTime> UpdateAction = delegate(IUpdateable updateable, GameTime gameTime)
	{
		updateable.Update(gameTime);
	};

	internal static Game Instance => Game._instance;

	/// <summary>
	/// The start up parameters for this <see cref="T:Microsoft.Xna.Framework.Game" />.
	/// </summary>
	public LaunchParameters LaunchParameters { get; private set; }

	/// <summary>
	/// A collection of game components attached to this <see cref="T:Microsoft.Xna.Framework.Game" />.
	/// </summary>
	public GameComponentCollection Components => this._components;

	public TimeSpan InactiveSleepTime
	{
		get
		{
			return this._inactiveSleepTime;
		}
		set
		{
			if (value < TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException("The time must be positive.", (Exception?)null);
			}
			this._inactiveSleepTime = value;
		}
	}

	/// <summary>
	/// The maximum amount of time we will frameskip over and only perform Update calls with no Draw calls.
	/// MonoGame extension.
	/// </summary>
	public TimeSpan MaxElapsedTime
	{
		get
		{
			return this._maxElapsedTime;
		}
		set
		{
			if (value < TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException("The time must be positive.", (Exception?)null);
			}
			if (value < this._targetElapsedTime)
			{
				throw new ArgumentOutOfRangeException("The time must be at least TargetElapsedTime", (Exception?)null);
			}
			this._maxElapsedTime = value;
		}
	}

	/// <summary>
	/// Indicates if the game is the focused application.
	/// </summary>
	public bool IsActive => this.Platform.IsActive;

	/// <summary>
	/// Indicates if the mouse cursor is visible on the game screen.
	/// </summary>
	public bool IsMouseVisible
	{
		get
		{
			return this.Platform.IsMouseVisible;
		}
		set
		{
			this.Platform.IsMouseVisible = value;
		}
	}

	/// <summary>
	/// The time between frames when running with a fixed time step. <seealso cref="P:Microsoft.Xna.Framework.Game.IsFixedTimeStep" />
	/// </summary>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Target elapsed time must be strictly larger than zero.</exception>
	public TimeSpan TargetElapsedTime
	{
		get
		{
			return this._targetElapsedTime;
		}
		set
		{
			value = this.Platform.TargetElapsedTimeChanging(value);
			if (value <= TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException("The time must be positive and non-zero.", (Exception?)null);
			}
			if (value != this._targetElapsedTime)
			{
				this._targetElapsedTime = value;
				this.Platform.TargetElapsedTimeChanged();
			}
		}
	}

	/// <summary>
	/// Indicates if this game is running with a fixed time between frames.
	///
	/// When set to <code>true</code> the target time between frames is
	/// given by <see cref="P:Microsoft.Xna.Framework.Game.TargetElapsedTime" />.
	/// </summary>
	public bool IsFixedTimeStep
	{
		get
		{
			return this._isFixedTimeStep;
		}
		set
		{
			this._isFixedTimeStep = value;
		}
	}

	/// <summary>
	/// Get a container holding service providers attached to this <see cref="T:Microsoft.Xna.Framework.Game" />.
	/// </summary>
	public GameServiceContainer Services => this._services;

	/// <summary>
	/// The <see cref="T:Microsoft.Xna.Framework.Content.ContentManager" /> of this <see cref="T:Microsoft.Xna.Framework.Game" />.
	/// </summary>
	/// <exception cref="T:System.ArgumentNullException">If Content is set to <code>null</code>.</exception>
	public ContentManager Content
	{
		get
		{
			return this._content;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			this._content = value;
		}
	}

	/// <summary>
	/// Gets the <see cref="P:Microsoft.Xna.Framework.Game.GraphicsDevice" /> used for rendering by this <see cref="T:Microsoft.Xna.Framework.Game" />.
	/// </summary>
	/// <exception cref="T:System.InvalidOperationException">
	/// There is no <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" /> attached to this <see cref="T:Microsoft.Xna.Framework.Game" />.
	/// </exception>
	public GraphicsDevice GraphicsDevice
	{
		get
		{
			if (this._graphicsDeviceService == null)
			{
				this._graphicsDeviceService = (IGraphicsDeviceService)this.Services.GetService(typeof(IGraphicsDeviceService));
				if (this._graphicsDeviceService == null)
				{
					throw new InvalidOperationException("No Graphics Device Service");
				}
			}
			return this._graphicsDeviceService.GraphicsDevice;
		}
	}

	/// <summary>
	/// The system window that this game is displayed on.
	/// </summary>
	[CLSCompliant(false)]
	public GameWindow Window => this.Platform.Window;

	internal bool Initialized => this._initialized;

	internal GraphicsDeviceManager graphicsDeviceManager
	{
		get
		{
			if (this._graphicsDeviceManager == null)
			{
				this._graphicsDeviceManager = (IGraphicsDeviceManager)this.Services.GetService(typeof(IGraphicsDeviceManager));
			}
			return (GraphicsDeviceManager)this._graphicsDeviceManager;
		}
		set
		{
			if (this._graphicsDeviceManager != null)
			{
				throw new InvalidOperationException("GraphicsDeviceManager already registered for this Game object");
			}
			this._graphicsDeviceManager = value;
		}
	}

	/// <summary>
	/// Raised when the game gains focus.
	/// </summary>
	public event EventHandler<EventArgs> Activated;

	/// <summary>
	/// Raised when the game loses focus.
	/// </summary>
	public event EventHandler<EventArgs> Deactivated;

	/// <summary>
	/// Raised when this game is being disposed.
	/// </summary>
	public event EventHandler<EventArgs> Disposed;

	/// <summary>
	/// Raised when this game is exiting.
	/// </summary>
	public event EventHandler<EventArgs> Exiting;

	/// <summary>
	/// Create a <see cref="T:Microsoft.Xna.Framework.Game" />.
	/// </summary>
	public Game()
	{
		Game._instance = this;
		this.LaunchParameters = new LaunchParameters();
		this._services = new GameServiceContainer();
		this._components = new GameComponentCollection();
		this._content = new ContentManager(this._services);
		this.Platform = GamePlatform.PlatformCreate(this);
		this.Platform.Activated += OnActivated;
		this.Platform.Deactivated += OnDeactivated;
		this._services.AddService(typeof(GamePlatform), this.Platform);
		FrameworkDispatcher.Update();
	}

	~Game()
	{
		this.Dispose(disposing: false);
	}

	[Conditional("DEBUG")]
	internal void Log(string Message)
	{
		_ = this.Platform;
	}

	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
		EventHelpers.Raise(this, this.Disposed, EventArgs.Empty);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (this._isDisposed)
		{
			return;
		}
		if (disposing)
		{
			for (int i = 0; i < this._components.Count; i++)
			{
				if (this._components[i] is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
			this._components = null;
			if (this._content != null)
			{
				this._content.Dispose();
				this._content = null;
			}
			if (this._graphicsDeviceManager != null)
			{
				(this._graphicsDeviceManager as GraphicsDeviceManager).Dispose();
				this._graphicsDeviceManager = null;
			}
			if (this.Platform != null)
			{
				this.Platform.Activated -= OnActivated;
				this.Platform.Deactivated -= OnDeactivated;
				this._services.RemoveService(typeof(GamePlatform));
				this.Platform.Dispose();
				this.Platform = null;
			}
			ContentTypeReaderManager.ClearTypeCreators();
			if (SoundEffect._systemState == SoundEffect.SoundSystemState.Initialized)
			{
				SoundEffect.PlatformShutdown();
			}
		}
		this._isDisposed = true;
		Game._instance = null;
	}

	[DebuggerNonUserCode]
	private void AssertNotDisposed()
	{
		if (this._isDisposed)
		{
			string name = base.GetType().Name;
			throw new ObjectDisposedException(name, $"The {name} object was used after being Disposed.");
		}
	}

	/// <summary>
	/// Exit the game at the end of this tick.
	/// </summary>
	public void Exit()
	{
		this._shouldExit = true;
		this._suppressDraw = true;
	}

	/// <summary>
	/// Reset the elapsed game time to <see cref="F:System.TimeSpan.Zero" />.
	/// </summary>
	public void ResetElapsedTime()
	{
		this.Platform.ResetElapsedTime();
		this._gameTimer.Reset();
		this._gameTimer.Start();
		this._accumulatedElapsedTime = TimeSpan.Zero;
		this._gameTime.ElapsedGameTime = TimeSpan.Zero;
		this._previousTicks = 0L;
	}

	/// <summary>
	/// Supress calling <see cref="M:Microsoft.Xna.Framework.Game.Draw(Microsoft.Xna.Framework.GameTime)" /> in the game loop.
	/// </summary>
	public void SuppressDraw()
	{
		this._suppressDraw = true;
	}

	/// <summary>
	/// Run the game for one frame, then exit.
	/// </summary>
	public void RunOneFrame()
	{
		if (this.Platform != null && this.Platform.BeforeRun())
		{
			if (!this._initialized)
			{
				this.DoInitialize();
				this._gameTimer = Stopwatch.StartNew();
				this._initialized = true;
			}
			this.BeginRun();
			this.Tick();
			this.EndRun();
		}
	}

	/// <summary>
	/// Run the game using the default <see cref="T:Microsoft.Xna.Framework.GameRunBehavior" /> for the current platform.
	/// </summary>
	public void Run()
	{
		this.Run(this.Platform.DefaultRunBehavior);
	}

	/// <summary>
	/// Run the game.
	/// </summary>
	/// <param name="runBehavior">Indicate if the game should be run synchronously or asynchronously.</param>
	public void Run(GameRunBehavior runBehavior)
	{
		this.AssertNotDisposed();
		if (!this.Platform.BeforeRun())
		{
			this.BeginRun();
			this._gameTimer = Stopwatch.StartNew();
			return;
		}
		if (!this._initialized)
		{
			this.DoInitialize();
			this._initialized = true;
		}
		this.BeginRun();
		this._gameTimer = Stopwatch.StartNew();
		switch (runBehavior)
		{
		case GameRunBehavior.Asynchronous:
			this.Platform.AsyncRunLoopEnded += Platform_AsyncRunLoopEnded;
			this.Platform.StartRunLoop();
			break;
		case GameRunBehavior.Synchronous:
			this.DoUpdate(new GameTime());
			this.Platform.RunLoop();
			this.EndRun();
			this.DoExiting();
			break;
		default:
			throw new ArgumentException($"Handling for the run behavior {runBehavior} is not implemented.");
		}
	}

	/// <summary>
	/// Run one iteration of the game loop.
	///
	/// Makes at least one call to <see cref="M:Microsoft.Xna.Framework.Game.Update(Microsoft.Xna.Framework.GameTime)" />
	/// and exactly one call to <see cref="M:Microsoft.Xna.Framework.Game.Draw(Microsoft.Xna.Framework.GameTime)" /> if drawing is not supressed.
	/// When <see cref="P:Microsoft.Xna.Framework.Game.IsFixedTimeStep" /> is set to <code>false</code> this will
	/// make exactly one call to <see cref="M:Microsoft.Xna.Framework.Game.Update(Microsoft.Xna.Framework.GameTime)" />.
	/// </summary>
	public void Tick()
	{
		if (!this.IsActive && this.InactiveSleepTime.TotalMilliseconds >= 1.0)
		{
			Thread.Sleep((int)this.InactiveSleepTime.TotalMilliseconds);
		}
		long currentTicks = this._gameTimer.Elapsed.Ticks;
		this._accumulatedElapsedTime += TimeSpan.FromTicks(currentTicks - this._previousTicks);
		this._previousTicks = currentTicks;
		this.AdvanceElapsedTime();
		if (this.IsFixedTimeStep)
		{
			while (this._accumulatedElapsedTime + this._worstCaseSleepPrecision < this.TargetElapsedTime)
			{
				Thread.Sleep(1);
				TimeSpan timeAdvancedSinceSleeping = this.AdvanceElapsedTime();
				this.UpdateEstimatedSleepPrecision(timeAdvancedSinceSleeping);
			}
			while (this._accumulatedElapsedTime < this.TargetElapsedTime)
			{
				Thread.SpinWait(1);
				this.AdvanceElapsedTime();
			}
		}
		if (this._accumulatedElapsedTime > this._maxElapsedTime)
		{
			this._accumulatedElapsedTime = this._maxElapsedTime;
		}
		if (this.IsFixedTimeStep)
		{
			while (this._accumulatedElapsedTime + this._worstCaseSleepPrecision < this.TargetElapsedTime)
			{
				Thread.Sleep(1);
				TimeSpan timeAdvancedSinceSleeping2 = this.AdvanceElapsedTime();
				this.UpdateEstimatedSleepPrecision(timeAdvancedSinceSleeping2);
			}
			this._gameTime.ElapsedGameTime = this.TargetElapsedTime;
			int stepCount = 0;
			while (this._accumulatedElapsedTime >= this.TargetElapsedTime && !this._shouldExit)
			{
				this._gameTime.TotalGameTime += this.TargetElapsedTime;
				this._accumulatedElapsedTime -= this.TargetElapsedTime;
				stepCount++;
				this.DoUpdate(this._gameTime);
			}
			this._updateFrameLag += Math.Max(0, stepCount - 1);
			if (this._gameTime.IsRunningSlowly)
			{
				if (this._updateFrameLag == 0)
				{
					this._gameTime.IsRunningSlowly = false;
				}
			}
			else if (this._updateFrameLag >= 5)
			{
				this._gameTime.IsRunningSlowly = true;
			}
			if (stepCount == 1 && this._updateFrameLag > 0)
			{
				this._updateFrameLag--;
			}
			this._gameTime.ElapsedGameTime = TimeSpan.FromTicks(this.TargetElapsedTime.Ticks * stepCount);
		}
		else
		{
			this._gameTime.ElapsedGameTime = this._accumulatedElapsedTime;
			this._gameTime.TotalGameTime += this._accumulatedElapsedTime;
			this._accumulatedElapsedTime = TimeSpan.Zero;
			this.DoUpdate(this._gameTime);
		}
		if (this._suppressDraw)
		{
			this._suppressDraw = false;
		}
		else
		{
			this.DoDraw(this._gameTime);
		}
		if (this._shouldExit)
		{
			this.Platform.Exit();
			this._shouldExit = false;
		}
	}

	private TimeSpan AdvanceElapsedTime()
	{
		long currentTicks = this._gameTimer.Elapsed.Ticks;
		TimeSpan timeAdvanced = TimeSpan.FromTicks(currentTicks - this._previousTicks);
		this._accumulatedElapsedTime += timeAdvanced;
		this._previousTicks = currentTicks;
		return timeAdvanced;
	}

	private void UpdateEstimatedSleepPrecision(TimeSpan timeSpentSleeping)
	{
		TimeSpan upperTimeBound = TimeSpan.FromMilliseconds(4.0);
		if (timeSpentSleeping > upperTimeBound)
		{
			timeSpentSleeping = upperTimeBound;
		}
		if (timeSpentSleeping >= this._worstCaseSleepPrecision)
		{
			this._worstCaseSleepPrecision = timeSpentSleeping;
		}
		else if (this._previousSleepTimes[this._sleepTimeIndex] == this._worstCaseSleepPrecision)
		{
			TimeSpan maxSleepTime = TimeSpan.MinValue;
			for (int i = 0; i < this._previousSleepTimes.Length; i++)
			{
				if (this._previousSleepTimes[i] > maxSleepTime)
				{
					maxSleepTime = this._previousSleepTimes[i];
				}
			}
			this._worstCaseSleepPrecision = maxSleepTime;
		}
		this._previousSleepTimes[this._sleepTimeIndex] = timeSpentSleeping;
		this._sleepTimeIndex = (this._sleepTimeIndex + 1) & 0x7F;
	}

	/// <summary>
	/// Called right before <see cref="M:Microsoft.Xna.Framework.Game.Draw(Microsoft.Xna.Framework.GameTime)" /> is normally called. Can return <code>false</code>
	/// to let the game loop not call <see cref="M:Microsoft.Xna.Framework.Game.Draw(Microsoft.Xna.Framework.GameTime)" />.
	/// </summary>
	/// <returns>
	///   <code>true</code> if <see cref="M:Microsoft.Xna.Framework.Game.Draw(Microsoft.Xna.Framework.GameTime)" /> should be called, <code>false</code> if it should not.
	/// </returns>
	protected virtual bool BeginDraw()
	{
		return true;
	}

	/// <summary>
	/// Called right after <see cref="M:Microsoft.Xna.Framework.Game.Draw(Microsoft.Xna.Framework.GameTime)" />. Presents the
	/// rendered frame in the <see cref="T:Microsoft.Xna.Framework.GameWindow" />.
	/// </summary>
	protected virtual void EndDraw()
	{
		this.Platform.Present();
	}

	/// <summary>
	/// Called after <see cref="M:Microsoft.Xna.Framework.Game.Initialize" />, but before the first call to <see cref="M:Microsoft.Xna.Framework.Game.Update(Microsoft.Xna.Framework.GameTime)" />.
	/// </summary>
	protected virtual void BeginRun()
	{
	}

	/// <summary>
	/// Called when the game loop has been terminated before exiting.
	/// </summary>
	protected virtual void EndRun()
	{
	}

	/// <summary>
	/// Override this to load graphical resources required by the game.
	/// </summary>
	protected virtual void LoadContent()
	{
	}

	/// <summary>
	/// Override this to unload graphical resources loaded by the game.
	/// </summary>
	protected virtual void UnloadContent()
	{
	}

	/// <summary>
	/// Override this to initialize the game and load any needed non-graphical resources.
	///
	/// Initializes attached <see cref="T:Microsoft.Xna.Framework.GameComponent" /> instances and calls <see cref="M:Microsoft.Xna.Framework.Game.LoadContent" />.
	/// </summary>
	protected virtual void Initialize()
	{
		this.applyChanges(this.graphicsDeviceManager);
		this.InitializeExistingComponents();
		this._graphicsDeviceService = (IGraphicsDeviceService)this.Services.GetService(typeof(IGraphicsDeviceService));
		if (this._graphicsDeviceService != null && this._graphicsDeviceService.GraphicsDevice != null)
		{
			this.LoadContent();
		}
	}

	/// <summary>
	/// Called when the game should draw a frame.
	///
	/// Draws the <see cref="T:Microsoft.Xna.Framework.DrawableGameComponent" /> instances attached to this game.
	/// Override this to render your game.
	/// </summary>
	/// <param name="gameTime">A <see cref="T:Microsoft.Xna.Framework.GameTime" /> instance containing the elapsed time since the last call to <see cref="M:Microsoft.Xna.Framework.Game.Draw(Microsoft.Xna.Framework.GameTime)" /> and the total time elapsed since the game started.</param>
	protected virtual void Draw(GameTime gameTime)
	{
		this._drawables.ForEachFilteredItem(Game.DrawAction, gameTime);
	}

	/// <summary>
	/// Called when the game should update.
	///
	/// Updates the <see cref="T:Microsoft.Xna.Framework.GameComponent" /> instances attached to this game.
	/// Override this to update your game.
	/// </summary>
	/// <param name="gameTime">The elapsed time since the last call to <see cref="M:Microsoft.Xna.Framework.Game.Update(Microsoft.Xna.Framework.GameTime)" />.</param>
	protected virtual void Update(GameTime gameTime)
	{
		this._updateables.ForEachFilteredItem(Game.UpdateAction, gameTime);
	}

	/// <summary>
	/// Called when the game is exiting. Raises the <see cref="E:Microsoft.Xna.Framework.Game.Exiting" /> event.
	/// </summary>
	/// <param name="sender">This <see cref="T:Microsoft.Xna.Framework.Game" />.</param>
	/// <param name="args">The arguments to the <see cref="E:Microsoft.Xna.Framework.Game.Exiting" /> event.</param>
	protected virtual void OnExiting(object sender, EventArgs args)
	{
		EventHelpers.Raise(sender, this.Exiting, args);
	}

	/// <summary>
	/// Called when the game gains focus. Raises the <see cref="E:Microsoft.Xna.Framework.Game.Activated" /> event.
	/// </summary>
	/// <param name="sender">This <see cref="T:Microsoft.Xna.Framework.Game" />.</param>
	/// <param name="args">The arguments to the <see cref="E:Microsoft.Xna.Framework.Game.Activated" /> event.</param>
	protected virtual void OnActivated(object sender, EventArgs args)
	{
		this.AssertNotDisposed();
		EventHelpers.Raise(sender, this.Activated, args);
	}

	/// <summary>
	/// Called when the game loses focus. Raises the <see cref="E:Microsoft.Xna.Framework.Game.Deactivated" /> event.
	/// </summary>
	/// <param name="sender">This <see cref="T:Microsoft.Xna.Framework.Game" />.</param>
	/// <param name="args">The arguments to the <see cref="E:Microsoft.Xna.Framework.Game.Deactivated" /> event.</param>
	protected virtual void OnDeactivated(object sender, EventArgs args)
	{
		this.AssertNotDisposed();
		EventHelpers.Raise(sender, this.Deactivated, args);
	}

	private void Components_ComponentAdded(object sender, GameComponentCollectionEventArgs e)
	{
		e.GameComponent.Initialize();
		this.CategorizeComponent(e.GameComponent);
	}

	private void Components_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
	{
		this.DecategorizeComponent(e.GameComponent);
	}

	private void Platform_AsyncRunLoopEnded(object sender, EventArgs e)
	{
		this.AssertNotDisposed();
		((GamePlatform)sender).AsyncRunLoopEnded -= Platform_AsyncRunLoopEnded;
		this.EndRun();
		this.DoExiting();
	}

	internal void applyChanges(GraphicsDeviceManager manager)
	{
		this.Platform.BeginScreenDeviceChange(this.GraphicsDevice.PresentationParameters.IsFullScreen);
		if (this.GraphicsDevice.PresentationParameters.IsFullScreen)
		{
			this.Platform.EnterFullScreen();
		}
		else
		{
			this.Platform.ExitFullScreen();
		}
		Viewport viewport = new Viewport(0, 0, this.GraphicsDevice.PresentationParameters.BackBufferWidth, this.GraphicsDevice.PresentationParameters.BackBufferHeight);
		this.GraphicsDevice.Viewport = viewport;
		this.Platform.EndScreenDeviceChange(string.Empty, viewport.Width, viewport.Height);
	}

	internal void DoUpdate(GameTime gameTime)
	{
		this.AssertNotDisposed();
		if (this.Platform.BeforeUpdate(gameTime))
		{
			FrameworkDispatcher.Update();
			this.Update(gameTime);
			TouchPanelState.CurrentTimestamp = gameTime.TotalGameTime;
		}
	}

	internal void DoDraw(GameTime gameTime)
	{
		this.AssertNotDisposed();
		if (this.Platform.BeforeDraw(gameTime) && this.BeginDraw())
		{
			this.Draw(gameTime);
			this.EndDraw();
		}
	}

	internal void DoInitialize()
	{
		this.AssertNotDisposed();
		if (this.GraphicsDevice == null && this.graphicsDeviceManager != null)
		{
			this._graphicsDeviceManager.CreateDevice();
		}
		this.Platform.BeforeInitialize();
		this.Initialize();
		this.CategorizeComponents();
		this._components.ComponentAdded += Components_ComponentAdded;
		this._components.ComponentRemoved += Components_ComponentRemoved;
	}

	internal void DoExiting()
	{
		this.OnExiting(this, EventArgs.Empty);
		this.UnloadContent();
	}

	private void InitializeExistingComponents()
	{
		for (int i = 0; i < this.Components.Count; i++)
		{
			this.Components[i].Initialize();
		}
	}

	private void CategorizeComponents()
	{
		this.DecategorizeComponents();
		for (int i = 0; i < this.Components.Count; i++)
		{
			this.CategorizeComponent(this.Components[i]);
		}
	}

	private void DecategorizeComponents()
	{
		this._updateables.Clear();
		this._drawables.Clear();
	}

	private void CategorizeComponent(IGameComponent component)
	{
		if (component is IUpdateable)
		{
			this._updateables.Add((IUpdateable)component);
		}
		if (component is IDrawable)
		{
			this._drawables.Add((IDrawable)component);
		}
	}

	private void DecategorizeComponent(IGameComponent component)
	{
		if (component is IUpdateable)
		{
			this._updateables.Remove((IUpdateable)component);
		}
		if (component is IDrawable)
		{
			this._drawables.Remove((IDrawable)component);
		}
	}
}
