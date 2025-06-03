using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework;

/// <summary>
/// A <see cref="T:Microsoft.Xna.Framework.GameComponent" /> that is drawn when its <see cref="T:Microsoft.Xna.Framework.Game" /> is drawn.
/// </summary>
public class DrawableGameComponent : GameComponent, IDrawable
{
	private bool _initialized;

	private bool _disposed;

	private int _drawOrder;

	private bool _visible = true;

	/// <summary>
	/// Get the <see cref="P:Microsoft.Xna.Framework.DrawableGameComponent.GraphicsDevice" /> that this <see cref="T:Microsoft.Xna.Framework.DrawableGameComponent" /> uses for drawing.
	/// </summary>
	public GraphicsDevice GraphicsDevice => base.Game.GraphicsDevice;

	public int DrawOrder
	{
		get
		{
			return this._drawOrder;
		}
		set
		{
			if (this._drawOrder != value)
			{
				this._drawOrder = value;
				this.OnDrawOrderChanged(this, EventArgs.Empty);
			}
		}
	}

	public bool Visible
	{
		get
		{
			return this._visible;
		}
		set
		{
			if (this._visible != value)
			{
				this._visible = value;
				this.OnVisibleChanged(this, EventArgs.Empty);
			}
		}
	}

	/// <inheritdoc />
	public event EventHandler<EventArgs> DrawOrderChanged;

	/// <inheritdoc />
	public event EventHandler<EventArgs> VisibleChanged;

	/// <summary>
	/// Create a <see cref="T:Microsoft.Xna.Framework.DrawableGameComponent" />.
	/// </summary>
	/// <param name="game">The game that this component will belong to.</param>
	public DrawableGameComponent(Game game)
		: base(game)
	{
	}

	public override void Initialize()
	{
		if (!this._initialized)
		{
			this._initialized = true;
			this.LoadContent();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (!this._disposed)
		{
			this._disposed = true;
			this.UnloadContent();
		}
	}

	/// <summary>
	/// Load graphical resources needed by this component.
	/// </summary>
	protected virtual void LoadContent()
	{
	}

	/// <summary>
	/// Unload graphical resources needed by this component.
	/// </summary>
	protected virtual void UnloadContent()
	{
	}

	/// <summary>
	/// Draw this component.
	/// </summary>
	/// <param name="gameTime">The time elapsed since the last call to <see cref="M:Microsoft.Xna.Framework.DrawableGameComponent.Draw(Microsoft.Xna.Framework.GameTime)" />.</param>
	public virtual void Draw(GameTime gameTime)
	{
	}

	/// <summary>
	/// Called when <see cref="P:Microsoft.Xna.Framework.DrawableGameComponent.Visible" /> changed.
	/// </summary>
	/// <param name="sender">This <see cref="T:Microsoft.Xna.Framework.DrawableGameComponent" />.</param>
	/// <param name="args">Arguments to the <see cref="E:Microsoft.Xna.Framework.DrawableGameComponent.VisibleChanged" /> event.</param>
	protected virtual void OnVisibleChanged(object sender, EventArgs args)
	{
		EventHelpers.Raise(sender, this.VisibleChanged, args);
	}

	/// <summary>
	/// Called when <see cref="P:Microsoft.Xna.Framework.DrawableGameComponent.DrawOrder" /> changed.
	/// </summary>
	/// <param name="sender">This <see cref="T:Microsoft.Xna.Framework.DrawableGameComponent" />.</param>
	/// <param name="args">Arguments to the <see cref="E:Microsoft.Xna.Framework.DrawableGameComponent.DrawOrderChanged" /> event.</param>
	protected virtual void OnDrawOrderChanged(object sender, EventArgs args)
	{
		EventHelpers.Raise(sender, this.DrawOrderChanged, args);
	}
}
