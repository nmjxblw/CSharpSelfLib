using System;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

public class OcclusionQuery : GraphicsResource
{
	private bool _inBeginEndPair;

	private bool _queryPerformed;

	private bool _isComplete;

	private int _pixelCount;

	private int glQueryId = -1;

	/// <summary>
	/// Gets a value indicating whether the occlusion query has completed.
	/// </summary>
	/// <value>
	/// <see langword="true" /> if the occlusion query has completed; otherwise,
	/// <see langword="false" />.
	/// </value>
	public bool IsComplete
	{
		get
		{
			if (this._isComplete)
			{
				return true;
			}
			if (!this._queryPerformed || this._inBeginEndPair)
			{
				return false;
			}
			this._isComplete = this.PlatformGetResult(out this._pixelCount);
			return this._isComplete;
		}
	}

	/// <summary>
	/// Gets the number of visible pixels.
	/// </summary>
	/// <value>The number of visible pixels.</value>
	/// <exception cref="T:System.InvalidOperationException">
	/// The occlusion query has not yet completed. Check <see cref="P:Microsoft.Xna.Framework.Graphics.OcclusionQuery.IsComplete" /> before reading
	/// the result!
	/// </exception>
	public int PixelCount
	{
		get
		{
			if (!this.IsComplete)
			{
				throw new InvalidOperationException("The occlusion query has not yet completed. Check IsComplete before reading the result.");
			}
			return this._pixelCount;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Graphics.OcclusionQuery" /> class.
	/// </summary>
	/// <param name="graphicsDevice">The graphics device.</param>
	/// <exception cref="T:System.ArgumentNullException">
	/// <paramref name="graphicsDevice" /> is <see langword="null" />.
	/// </exception>
	/// <exception cref="T:System.NotSupportedException">
	/// The current graphics profile does not support occlusion queries.
	/// </exception>
	public OcclusionQuery(GraphicsDevice graphicsDevice)
	{
		if (graphicsDevice == null)
		{
			throw new ArgumentNullException("graphicsDevice");
		}
		if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
		{
			throw new NotSupportedException("The Reach profile does not support occlusion queries.");
		}
		base.GraphicsDevice = graphicsDevice;
		this.PlatformConstruct();
	}

	/// <summary>
	/// Begins the occlusion query.
	/// </summary>
	/// <exception cref="T:System.InvalidOperationException">
	/// <see cref="M:Microsoft.Xna.Framework.Graphics.OcclusionQuery.Begin" /> is called again before calling <see cref="M:Microsoft.Xna.Framework.Graphics.OcclusionQuery.End" />.
	/// </exception>
	public void Begin()
	{
		if (this._inBeginEndPair)
		{
			throw new InvalidOperationException("End() must be called before calling Begin() again.");
		}
		this._inBeginEndPair = true;
		this._isComplete = false;
		this.PlatformBegin();
	}

	/// <summary>
	/// Ends the occlusion query.
	/// </summary>
	/// <exception cref="T:System.InvalidOperationException">
	/// <see cref="M:Microsoft.Xna.Framework.Graphics.OcclusionQuery.End" /> is called before calling <see cref="M:Microsoft.Xna.Framework.Graphics.OcclusionQuery.Begin" />.
	/// </exception>
	public void End()
	{
		if (!this._inBeginEndPair)
		{
			throw new InvalidOperationException("Begin() must be called before calling End().");
		}
		this._inBeginEndPair = false;
		this._queryPerformed = true;
		this.PlatformEnd();
	}

	private void PlatformConstruct()
	{
		GL.GenQueries(1, out this.glQueryId);
	}

	private void PlatformBegin()
	{
		GL.BeginQuery(QueryTarget.SamplesPassed, this.glQueryId);
	}

	private void PlatformEnd()
	{
		GL.EndQuery(QueryTarget.SamplesPassed);
	}

	private bool PlatformGetResult(out int pixelCount)
	{
		int resultReady = 0;
		GL.GetQueryObject(this.glQueryId, GetQueryObjectParam.QueryResultAvailable, out resultReady);
		if (resultReady == 0)
		{
			pixelCount = 0;
			return false;
		}
		GL.GetQueryObject(this.glQueryId, GetQueryObjectParam.QueryResult, out pixelCount);
		return true;
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed && this.glQueryId > -1)
		{
			base.GraphicsDevice.DisposeQuery(this.glQueryId);
			this.glQueryId = -1;
		}
		base.Dispose(disposing);
	}
}
