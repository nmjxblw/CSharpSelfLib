using System;

namespace Microsoft.Xna.Framework;

/// <summary>
/// An object that can be attached to a <see cref="T:Microsoft.Xna.Framework.Game" /> and have its <see cref="M:Microsoft.Xna.Framework.GameComponent.Update(Microsoft.Xna.Framework.GameTime)" />
/// method called when <see cref="M:Microsoft.Xna.Framework.Game.Update(Microsoft.Xna.Framework.GameTime)" /> is called.
/// </summary>
public class GameComponent : IGameComponent, IUpdateable, IDisposable
{
	private bool _enabled = true;

	private int _updateOrder;

	/// <summary>
	/// The <see cref="P:Microsoft.Xna.Framework.GameComponent.Game" /> that owns this <see cref="T:Microsoft.Xna.Framework.GameComponent" />.
	/// </summary>
	public Game Game { get; private set; }

	public bool Enabled
	{
		get
		{
			return this._enabled;
		}
		set
		{
			if (this._enabled != value)
			{
				this._enabled = value;
				this.OnEnabledChanged(this, EventArgs.Empty);
			}
		}
	}

	public int UpdateOrder
	{
		get
		{
			return this._updateOrder;
		}
		set
		{
			if (this._updateOrder != value)
			{
				this._updateOrder = value;
				this.OnUpdateOrderChanged(this, EventArgs.Empty);
			}
		}
	}

	/// <inheritdoc />
	public event EventHandler<EventArgs> EnabledChanged;

	/// <inheritdoc />
	public event EventHandler<EventArgs> UpdateOrderChanged;

	/// <summary>
	/// Create a <see cref="T:Microsoft.Xna.Framework.GameComponent" />.
	/// </summary>
	/// <param name="game">The game that this component will belong to.</param>
	public GameComponent(Game game)
	{
		this.Game = game;
	}

	~GameComponent()
	{
		this.Dispose(disposing: false);
	}

	public virtual void Initialize()
	{
	}

	/// <summary>
	/// Update the component.
	/// </summary>
	/// <param name="gameTime"><see cref="T:Microsoft.Xna.Framework.GameTime" /> of the <see cref="P:Microsoft.Xna.Framework.GameComponent.Game" />.</param>
	public virtual void Update(GameTime gameTime)
	{
	}

	/// <summary>
	/// Called when <see cref="P:Microsoft.Xna.Framework.GameComponent.UpdateOrder" /> changed. Raises the <see cref="E:Microsoft.Xna.Framework.GameComponent.UpdateOrderChanged" /> event.
	/// </summary>
	/// <param name="sender">This <see cref="T:Microsoft.Xna.Framework.GameComponent" />.</param>
	/// <param name="args">Arguments to the <see cref="E:Microsoft.Xna.Framework.GameComponent.UpdateOrderChanged" /> event.</param>
	protected virtual void OnUpdateOrderChanged(object sender, EventArgs args)
	{
		EventHelpers.Raise(sender, this.UpdateOrderChanged, args);
	}

	/// <summary>
	/// Called when <see cref="P:Microsoft.Xna.Framework.GameComponent.Enabled" /> changed. Raises the <see cref="E:Microsoft.Xna.Framework.GameComponent.EnabledChanged" /> event.
	/// </summary>
	/// <param name="sender">This <see cref="T:Microsoft.Xna.Framework.GameComponent" />.</param>
	/// <param name="args">Arguments to the <see cref="E:Microsoft.Xna.Framework.GameComponent.EnabledChanged" /> event.</param>
	protected virtual void OnEnabledChanged(object sender, EventArgs args)
	{
		EventHelpers.Raise(sender, this.EnabledChanged, args);
	}

	/// <summary>
	/// Shuts down the component.
	/// </summary>
	protected virtual void Dispose(bool disposing)
	{
	}

	/// <summary>
	/// Shuts down the component.
	/// </summary>
	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
