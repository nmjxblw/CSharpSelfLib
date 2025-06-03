using System;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

public class DepthStencilState : GraphicsResource
{
	private readonly bool _defaultStateObject;

	private bool _depthBufferEnable;

	private bool _depthBufferWriteEnable;

	private StencilOperation _counterClockwiseStencilDepthBufferFail;

	private StencilOperation _counterClockwiseStencilFail;

	private CompareFunction _counterClockwiseStencilFunction;

	private StencilOperation _counterClockwiseStencilPass;

	private CompareFunction _depthBufferFunction;

	private int _referenceStencil;

	private StencilOperation _stencilDepthBufferFail;

	private bool _stencilEnable;

	private StencilOperation _stencilFail;

	private CompareFunction _stencilFunction;

	private int _stencilMask;

	private StencilOperation _stencilPass;

	private int _stencilWriteMask;

	private bool _twoSidedStencilMode;

	public static readonly DepthStencilState Default;

	public static readonly DepthStencilState DepthRead;

	public static readonly DepthStencilState None;

	public bool DepthBufferEnable
	{
		get
		{
			return this._depthBufferEnable;
		}
		set
		{
			this.ThrowIfBound();
			this._depthBufferEnable = value;
		}
	}

	public bool DepthBufferWriteEnable
	{
		get
		{
			return this._depthBufferWriteEnable;
		}
		set
		{
			this.ThrowIfBound();
			this._depthBufferWriteEnable = value;
		}
	}

	public StencilOperation CounterClockwiseStencilDepthBufferFail
	{
		get
		{
			return this._counterClockwiseStencilDepthBufferFail;
		}
		set
		{
			this.ThrowIfBound();
			this._counterClockwiseStencilDepthBufferFail = value;
		}
	}

	public StencilOperation CounterClockwiseStencilFail
	{
		get
		{
			return this._counterClockwiseStencilFail;
		}
		set
		{
			this.ThrowIfBound();
			this._counterClockwiseStencilFail = value;
		}
	}

	public CompareFunction CounterClockwiseStencilFunction
	{
		get
		{
			return this._counterClockwiseStencilFunction;
		}
		set
		{
			this.ThrowIfBound();
			this._counterClockwiseStencilFunction = value;
		}
	}

	public StencilOperation CounterClockwiseStencilPass
	{
		get
		{
			return this._counterClockwiseStencilPass;
		}
		set
		{
			this.ThrowIfBound();
			this._counterClockwiseStencilPass = value;
		}
	}

	public CompareFunction DepthBufferFunction
	{
		get
		{
			return this._depthBufferFunction;
		}
		set
		{
			this.ThrowIfBound();
			this._depthBufferFunction = value;
		}
	}

	public int ReferenceStencil
	{
		get
		{
			return this._referenceStencil;
		}
		set
		{
			this.ThrowIfBound();
			this._referenceStencil = value;
		}
	}

	public StencilOperation StencilDepthBufferFail
	{
		get
		{
			return this._stencilDepthBufferFail;
		}
		set
		{
			this.ThrowIfBound();
			this._stencilDepthBufferFail = value;
		}
	}

	public bool StencilEnable
	{
		get
		{
			return this._stencilEnable;
		}
		set
		{
			this.ThrowIfBound();
			this._stencilEnable = value;
		}
	}

	public StencilOperation StencilFail
	{
		get
		{
			return this._stencilFail;
		}
		set
		{
			this.ThrowIfBound();
			this._stencilFail = value;
		}
	}

	public CompareFunction StencilFunction
	{
		get
		{
			return this._stencilFunction;
		}
		set
		{
			this.ThrowIfBound();
			this._stencilFunction = value;
		}
	}

	public int StencilMask
	{
		get
		{
			return this._stencilMask;
		}
		set
		{
			this.ThrowIfBound();
			this._stencilMask = value;
		}
	}

	public StencilOperation StencilPass
	{
		get
		{
			return this._stencilPass;
		}
		set
		{
			this.ThrowIfBound();
			this._stencilPass = value;
		}
	}

	public int StencilWriteMask
	{
		get
		{
			return this._stencilWriteMask;
		}
		set
		{
			this.ThrowIfBound();
			this._stencilWriteMask = value;
		}
	}

	public bool TwoSidedStencilMode
	{
		get
		{
			return this._twoSidedStencilMode;
		}
		set
		{
			this.ThrowIfBound();
			this._twoSidedStencilMode = value;
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
			throw new InvalidOperationException("This depth stencil state is already bound to a different graphics device.");
		}
		base.GraphicsDevice = device;
	}

	internal void ThrowIfBound()
	{
		if (this._defaultStateObject)
		{
			throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
		}
		if (base.GraphicsDevice != null)
		{
			throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");
		}
	}

	public DepthStencilState()
	{
		this.DepthBufferEnable = true;
		this.DepthBufferWriteEnable = true;
		this.DepthBufferFunction = CompareFunction.LessEqual;
		this.StencilEnable = false;
		this.StencilFunction = CompareFunction.Always;
		this.StencilPass = StencilOperation.Keep;
		this.StencilFail = StencilOperation.Keep;
		this.StencilDepthBufferFail = StencilOperation.Keep;
		this.TwoSidedStencilMode = false;
		this.CounterClockwiseStencilFunction = CompareFunction.Always;
		this.CounterClockwiseStencilFail = StencilOperation.Keep;
		this.CounterClockwiseStencilPass = StencilOperation.Keep;
		this.CounterClockwiseStencilDepthBufferFail = StencilOperation.Keep;
		this.StencilMask = int.MaxValue;
		this.StencilWriteMask = int.MaxValue;
		this.ReferenceStencil = 0;
	}

	private DepthStencilState(string name, bool depthBufferEnable, bool depthBufferWriteEnable)
		: this()
	{
		base.Name = name;
		this._depthBufferEnable = depthBufferEnable;
		this._depthBufferWriteEnable = depthBufferWriteEnable;
		this._defaultStateObject = true;
	}

	private DepthStencilState(DepthStencilState cloneSource)
	{
		base.Name = cloneSource.Name;
		this._depthBufferEnable = cloneSource._depthBufferEnable;
		this._depthBufferWriteEnable = cloneSource._depthBufferWriteEnable;
		this._counterClockwiseStencilDepthBufferFail = cloneSource._counterClockwiseStencilDepthBufferFail;
		this._counterClockwiseStencilFail = cloneSource._counterClockwiseStencilFail;
		this._counterClockwiseStencilFunction = cloneSource._counterClockwiseStencilFunction;
		this._counterClockwiseStencilPass = cloneSource._counterClockwiseStencilPass;
		this._depthBufferFunction = cloneSource._depthBufferFunction;
		this._referenceStencil = cloneSource._referenceStencil;
		this._stencilDepthBufferFail = cloneSource._stencilDepthBufferFail;
		this._stencilEnable = cloneSource._stencilEnable;
		this._stencilFail = cloneSource._stencilFail;
		this._stencilFunction = cloneSource._stencilFunction;
		this._stencilMask = cloneSource._stencilMask;
		this._stencilPass = cloneSource._stencilPass;
		this._stencilWriteMask = cloneSource._stencilWriteMask;
		this._twoSidedStencilMode = cloneSource._twoSidedStencilMode;
	}

	static DepthStencilState()
	{
		DepthStencilState.Default = new DepthStencilState("DepthStencilState.Default", depthBufferEnable: true, depthBufferWriteEnable: true);
		DepthStencilState.DepthRead = new DepthStencilState("DepthStencilState.DepthRead", depthBufferEnable: true, depthBufferWriteEnable: false);
		DepthStencilState.None = new DepthStencilState("DepthStencilState.None", depthBufferEnable: false, depthBufferWriteEnable: false);
	}

	internal DepthStencilState Clone()
	{
		return new DepthStencilState(this);
	}

	protected override void Dispose(bool disposing)
	{
		_ = base.IsDisposed;
		base.Dispose(disposing);
	}

	internal void PlatformApplyState(GraphicsDevice device, bool force = false)
	{
		if (force || this.DepthBufferEnable != device._lastDepthStencilState.DepthBufferEnable)
		{
			if (!this.DepthBufferEnable)
			{
				GL.Disable(EnableCap.DepthTest);
			}
			else
			{
				GL.Enable(EnableCap.DepthTest);
			}
			device._lastDepthStencilState.DepthBufferEnable = this.DepthBufferEnable;
		}
		if (force || this.DepthBufferFunction != device._lastDepthStencilState.DepthBufferFunction)
		{
			GL.DepthFunc(this.DepthBufferFunction.GetDepthFunction());
			device._lastDepthStencilState.DepthBufferFunction = this.DepthBufferFunction;
		}
		if (force || this.DepthBufferWriteEnable != device._lastDepthStencilState.DepthBufferWriteEnable)
		{
			GL.DepthMask(this.DepthBufferWriteEnable);
			device._lastDepthStencilState.DepthBufferWriteEnable = this.DepthBufferWriteEnable;
		}
		if (force || this.StencilEnable != device._lastDepthStencilState.StencilEnable)
		{
			if (!this.StencilEnable)
			{
				GL.Disable(EnableCap.StencilTest);
			}
			else
			{
				GL.Enable(EnableCap.StencilTest);
			}
			device._lastDepthStencilState.StencilEnable = this.StencilEnable;
		}
		if (this.TwoSidedStencilMode)
		{
			StencilFace cullFaceModeFront = StencilFace.Front;
			StencilFace cullFaceModeBack = StencilFace.Back;
			StencilFace stencilFaceFront = StencilFace.Front;
			StencilFace stencilFaceBack = StencilFace.Back;
			if (force || this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode || this.StencilFunction != device._lastDepthStencilState.StencilFunction || this.ReferenceStencil != device._lastDepthStencilState.ReferenceStencil || this.StencilMask != device._lastDepthStencilState.StencilMask)
			{
				GL.StencilFuncSeparate(cullFaceModeFront, DepthStencilState.GetStencilFunc(this.StencilFunction), this.ReferenceStencil, this.StencilMask);
				device._lastDepthStencilState.StencilFunction = this.StencilFunction;
				device._lastDepthStencilState.ReferenceStencil = this.ReferenceStencil;
				device._lastDepthStencilState.StencilMask = this.StencilMask;
			}
			if (force || this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode || this.CounterClockwiseStencilFunction != device._lastDepthStencilState.CounterClockwiseStencilFunction || this.ReferenceStencil != device._lastDepthStencilState.ReferenceStencil || this.StencilMask != device._lastDepthStencilState.StencilMask)
			{
				GL.StencilFuncSeparate(cullFaceModeBack, DepthStencilState.GetStencilFunc(this.CounterClockwiseStencilFunction), this.ReferenceStencil, this.StencilMask);
				device._lastDepthStencilState.CounterClockwiseStencilFunction = this.CounterClockwiseStencilFunction;
				device._lastDepthStencilState.ReferenceStencil = this.ReferenceStencil;
				device._lastDepthStencilState.StencilMask = this.StencilMask;
			}
			if (force || this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode || this.StencilFail != device._lastDepthStencilState.StencilFail || this.StencilDepthBufferFail != device._lastDepthStencilState.StencilDepthBufferFail || this.StencilPass != device._lastDepthStencilState.StencilPass)
			{
				GL.StencilOpSeparate(stencilFaceFront, DepthStencilState.GetStencilOp(this.StencilFail), DepthStencilState.GetStencilOp(this.StencilDepthBufferFail), DepthStencilState.GetStencilOp(this.StencilPass));
				device._lastDepthStencilState.StencilFail = this.StencilFail;
				device._lastDepthStencilState.StencilDepthBufferFail = this.StencilDepthBufferFail;
				device._lastDepthStencilState.StencilPass = this.StencilPass;
			}
			if (force || this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode || this.CounterClockwiseStencilFail != device._lastDepthStencilState.CounterClockwiseStencilFail || this.CounterClockwiseStencilDepthBufferFail != device._lastDepthStencilState.CounterClockwiseStencilDepthBufferFail || this.CounterClockwiseStencilPass != device._lastDepthStencilState.CounterClockwiseStencilPass)
			{
				GL.StencilOpSeparate(stencilFaceBack, DepthStencilState.GetStencilOp(this.CounterClockwiseStencilFail), DepthStencilState.GetStencilOp(this.CounterClockwiseStencilDepthBufferFail), DepthStencilState.GetStencilOp(this.CounterClockwiseStencilPass));
				device._lastDepthStencilState.CounterClockwiseStencilFail = this.CounterClockwiseStencilFail;
				device._lastDepthStencilState.CounterClockwiseStencilDepthBufferFail = this.CounterClockwiseStencilDepthBufferFail;
				device._lastDepthStencilState.CounterClockwiseStencilPass = this.CounterClockwiseStencilPass;
			}
		}
		else
		{
			if (force || this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode || this.StencilFunction != device._lastDepthStencilState.StencilFunction || this.ReferenceStencil != device._lastDepthStencilState.ReferenceStencil || this.StencilMask != device._lastDepthStencilState.StencilMask)
			{
				GL.StencilFunc(DepthStencilState.GetStencilFunc(this.StencilFunction), this.ReferenceStencil, this.StencilMask);
				device._lastDepthStencilState.StencilFunction = this.StencilFunction;
				device._lastDepthStencilState.ReferenceStencil = this.ReferenceStencil;
				device._lastDepthStencilState.StencilMask = this.StencilMask;
			}
			if (force || this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode || this.StencilFail != device._lastDepthStencilState.StencilFail || this.StencilDepthBufferFail != device._lastDepthStencilState.StencilDepthBufferFail || this.StencilPass != device._lastDepthStencilState.StencilPass)
			{
				GL.StencilOp(DepthStencilState.GetStencilOp(this.StencilFail), DepthStencilState.GetStencilOp(this.StencilDepthBufferFail), DepthStencilState.GetStencilOp(this.StencilPass));
				device._lastDepthStencilState.StencilFail = this.StencilFail;
				device._lastDepthStencilState.StencilDepthBufferFail = this.StencilDepthBufferFail;
				device._lastDepthStencilState.StencilPass = this.StencilPass;
			}
		}
		device._lastDepthStencilState.TwoSidedStencilMode = this.TwoSidedStencilMode;
		if (force || this.StencilWriteMask != device._lastDepthStencilState.StencilWriteMask)
		{
			GL.StencilMask(this.StencilWriteMask);
			device._lastDepthStencilState.StencilWriteMask = this.StencilWriteMask;
		}
	}

	private static GLStencilFunction GetStencilFunc(CompareFunction function)
	{
		return function switch
		{
			CompareFunction.Always => GLStencilFunction.Always, 
			CompareFunction.Equal => GLStencilFunction.Equal, 
			CompareFunction.Greater => GLStencilFunction.Greater, 
			CompareFunction.GreaterEqual => GLStencilFunction.Gequal, 
			CompareFunction.Less => GLStencilFunction.Less, 
			CompareFunction.LessEqual => GLStencilFunction.Lequal, 
			CompareFunction.Never => GLStencilFunction.Never, 
			CompareFunction.NotEqual => GLStencilFunction.Notequal, 
			_ => GLStencilFunction.Always, 
		};
	}

	private static StencilOp GetStencilOp(StencilOperation operation)
	{
		return operation switch
		{
			StencilOperation.Keep => StencilOp.Keep, 
			StencilOperation.Decrement => StencilOp.DecrWrap, 
			StencilOperation.DecrementSaturation => StencilOp.Decr, 
			StencilOperation.IncrementSaturation => StencilOp.Incr, 
			StencilOperation.Increment => StencilOp.IncrWrap, 
			StencilOperation.Invert => StencilOp.Invert, 
			StencilOperation.Replace => StencilOp.Replace, 
			StencilOperation.Zero => StencilOp.Zero, 
			_ => StencilOp.Keep, 
		};
	}
}
