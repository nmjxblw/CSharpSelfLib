using System;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

public class RasterizerState : GraphicsResource
{
	private readonly bool _defaultStateObject;

	private CullMode _cullMode;

	private float _depthBias;

	private FillMode _fillMode;

	private bool _multiSampleAntiAlias;

	private bool _scissorTestEnable;

	private float _slopeScaleDepthBias;

	private bool _depthClipEnable;

	public static readonly RasterizerState CullClockwise;

	public static readonly RasterizerState CullCounterClockwise;

	public static readonly RasterizerState CullNone;

	public CullMode CullMode
	{
		get
		{
			return this._cullMode;
		}
		set
		{
			this.ThrowIfBound();
			this._cullMode = value;
		}
	}

	public float DepthBias
	{
		get
		{
			return this._depthBias;
		}
		set
		{
			this.ThrowIfBound();
			this._depthBias = value;
		}
	}

	public FillMode FillMode
	{
		get
		{
			return this._fillMode;
		}
		set
		{
			this.ThrowIfBound();
			this._fillMode = value;
		}
	}

	public bool MultiSampleAntiAlias
	{
		get
		{
			return this._multiSampleAntiAlias;
		}
		set
		{
			this.ThrowIfBound();
			this._multiSampleAntiAlias = value;
		}
	}

	public bool ScissorTestEnable
	{
		get
		{
			return this._scissorTestEnable;
		}
		set
		{
			this.ThrowIfBound();
			this._scissorTestEnable = value;
		}
	}

	public float SlopeScaleDepthBias
	{
		get
		{
			return this._slopeScaleDepthBias;
		}
		set
		{
			this.ThrowIfBound();
			this._slopeScaleDepthBias = value;
		}
	}

	public bool DepthClipEnable
	{
		get
		{
			return this._depthClipEnable;
		}
		set
		{
			this.ThrowIfBound();
			this._depthClipEnable = value;
		}
	}

	internal void BindToGraphicsDevice(GraphicsDevice device)
	{
		if (this._defaultStateObject)
		{
			throw new InvalidOperationException("You cannot bind a default state object.");
		}
		if (base.GraphicsDevice != null && base.GraphicsDevice != device)
		{
			throw new InvalidOperationException("This rasterizer state is already bound to a different graphics device.");
		}
		base.GraphicsDevice = device;
	}

	internal void ThrowIfBound()
	{
		if (this._defaultStateObject)
		{
			throw new InvalidOperationException("You cannot modify a default rasterizer state object.");
		}
		if (base.GraphicsDevice != null)
		{
			throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");
		}
	}

	public RasterizerState()
	{
		this.CullMode = CullMode.CullCounterClockwiseFace;
		this.FillMode = FillMode.Solid;
		this.DepthBias = 0f;
		this.MultiSampleAntiAlias = true;
		this.ScissorTestEnable = false;
		this.SlopeScaleDepthBias = 0f;
		this.DepthClipEnable = true;
	}

	private RasterizerState(string name, CullMode cullMode)
		: this()
	{
		base.Name = name;
		this._cullMode = cullMode;
		this._defaultStateObject = true;
	}

	private RasterizerState(RasterizerState cloneSource)
	{
		base.Name = cloneSource.Name;
		this._cullMode = cloneSource._cullMode;
		this._fillMode = cloneSource._fillMode;
		this._depthBias = cloneSource._depthBias;
		this._multiSampleAntiAlias = cloneSource._multiSampleAntiAlias;
		this._scissorTestEnable = cloneSource._scissorTestEnable;
		this._slopeScaleDepthBias = cloneSource._slopeScaleDepthBias;
		this._depthClipEnable = cloneSource._depthClipEnable;
	}

	static RasterizerState()
	{
		RasterizerState.CullClockwise = new RasterizerState("RasterizerState.CullClockwise", CullMode.CullClockwiseFace);
		RasterizerState.CullCounterClockwise = new RasterizerState("RasterizerState.CullCounterClockwise", CullMode.CullCounterClockwiseFace);
		RasterizerState.CullNone = new RasterizerState("RasterizerState.CullNone", CullMode.None);
	}

	internal RasterizerState Clone()
	{
		return new RasterizerState(this);
	}

	protected override void Dispose(bool disposing)
	{
		_ = base.IsDisposed;
		base.Dispose(disposing);
	}

	internal void PlatformApplyState(GraphicsDevice device, bool force = false)
	{
		bool offscreen = device.IsRenderTargetBound;
		if (force)
		{
			GL.Disable(EnableCap.Dither);
		}
		if (this.CullMode == CullMode.None)
		{
			GL.Disable(EnableCap.CullFace);
		}
		else
		{
			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Back);
			if (this.CullMode == CullMode.CullClockwiseFace)
			{
				if (offscreen)
				{
					GL.FrontFace(FrontFaceDirection.Cw);
				}
				else
				{
					GL.FrontFace(FrontFaceDirection.Ccw);
				}
			}
			else if (offscreen)
			{
				GL.FrontFace(FrontFaceDirection.Ccw);
			}
			else
			{
				GL.FrontFace(FrontFaceDirection.Cw);
			}
		}
		if (this.FillMode == FillMode.Solid)
		{
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
		}
		else
		{
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
		}
		if (force || this.ScissorTestEnable != device._lastRasterizerState.ScissorTestEnable)
		{
			if (this.ScissorTestEnable)
			{
				GL.Enable(EnableCap.ScissorTest);
			}
			else
			{
				GL.Disable(EnableCap.ScissorTest);
			}
			device._lastRasterizerState.ScissorTestEnable = this.ScissorTestEnable;
		}
		if (force || this.DepthBias != device._lastRasterizerState.DepthBias || this.SlopeScaleDepthBias != device._lastRasterizerState.SlopeScaleDepthBias)
		{
			if (this.DepthBias != 0f || this.SlopeScaleDepthBias != 0f)
			{
				int depthMul;
				switch (device.ActiveDepthFormat)
				{
				case DepthFormat.None:
					depthMul = 0;
					break;
				case DepthFormat.Depth16:
					depthMul = 32768;
					break;
				case DepthFormat.Depth24:
				case DepthFormat.Depth24Stencil8:
					depthMul = 8388608;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				GL.Enable(EnableCap.PolygonOffsetFill);
				GL.PolygonOffset(this.SlopeScaleDepthBias, this.DepthBias * (float)depthMul);
			}
			else
			{
				GL.Disable(EnableCap.PolygonOffsetFill);
			}
			device._lastRasterizerState.DepthBias = this.DepthBias;
			device._lastRasterizerState.SlopeScaleDepthBias = this.SlopeScaleDepthBias;
		}
		if (device.GraphicsCapabilities.SupportsDepthClamp && (force || this.DepthClipEnable != device._lastRasterizerState.DepthClipEnable))
		{
			if (!this.DepthClipEnable)
			{
				GL.Enable((EnableCap)34383);
			}
			else
			{
				GL.Disable((EnableCap)34383);
			}
			device._lastRasterizerState.DepthClipEnable = this.DepthClipEnable;
		}
	}
}
