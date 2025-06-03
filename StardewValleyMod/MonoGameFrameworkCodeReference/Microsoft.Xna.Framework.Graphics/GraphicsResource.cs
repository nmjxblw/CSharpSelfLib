using System;

namespace Microsoft.Xna.Framework.Graphics;

public abstract class GraphicsResource : IDisposable
{
	private bool disposed;

	private GraphicsDevice graphicsDevice;

	private WeakReference _selfReference;

	public GraphicsDevice GraphicsDevice
	{
		get
		{
			return this.graphicsDevice;
		}
		internal set
		{
			if (this.graphicsDevice != value)
			{
				if (this.graphicsDevice != null)
				{
					this.graphicsDevice.RemoveResourceReference(this._selfReference);
					this._selfReference = null;
				}
				this.graphicsDevice = value;
				this._selfReference = new WeakReference(this);
				this.graphicsDevice.AddResourceReference(this._selfReference);
			}
		}
	}

	public bool IsDisposed => this.disposed;

	public string Name { get; set; }

	public object Tag { get; set; }

	public event EventHandler<EventArgs> Disposing;

	internal GraphicsResource()
	{
	}

	~GraphicsResource()
	{
		this.Dispose(disposing: false);
	}

	/// <summary>
	/// Called before the device is reset. Allows graphics resources to 
	/// invalidate their state so they can be recreated after the device reset.
	/// Warning: This may be called after a call to Dispose() up until
	/// the resource is garbage collected.
	/// </summary>
	protected internal virtual void GraphicsDeviceResetting()
	{
	}

	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// The method that derived classes should override to implement disposing of managed and native resources.
	/// </summary>
	/// <param name="disposing">True if managed objects should be disposed.</param>
	/// <remarks>Native resources should always be released regardless of the value of the disposing parameter.</remarks>
	protected virtual void Dispose(bool disposing)
	{
		if (!this.disposed)
		{
			if (disposing)
			{
				EventHelpers.Raise(this, this.Disposing, EventArgs.Empty);
			}
			if (this.graphicsDevice != null)
			{
				this.graphicsDevice.RemoveResourceReference(this._selfReference);
			}
			this._selfReference = null;
			this.graphicsDevice = null;
			this.disposed = true;
		}
	}

	public override string ToString()
	{
		if (!string.IsNullOrEmpty(this.Name))
		{
			return this.Name;
		}
		return base.ToString();
	}
}
