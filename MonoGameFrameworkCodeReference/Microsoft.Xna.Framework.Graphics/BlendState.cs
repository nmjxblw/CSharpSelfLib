using System;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

public class BlendState : GraphicsResource
{
	private readonly TargetBlendState[] _targetBlendState;

	private readonly bool _defaultStateObject;

	private Color _blendFactor;

	private int _multiSampleMask;

	private bool _independentBlendEnable;

	public static readonly BlendState Additive;

	public static readonly BlendState AlphaBlend;

	public static readonly BlendState NonPremultiplied;

	public static readonly BlendState Opaque;

	/// <summary>
	/// Returns the target specific blend state.
	/// </summary>
	/// <param name="index">The 0 to 3 target blend state index.</param>
	/// <returns>A target blend state.</returns>
	public TargetBlendState this[int index] => this._targetBlendState[index];

	public BlendFunction AlphaBlendFunction
	{
		get
		{
			return this._targetBlendState[0].AlphaBlendFunction;
		}
		set
		{
			this.ThrowIfBound();
			this._targetBlendState[0].AlphaBlendFunction = value;
		}
	}

	public Blend AlphaDestinationBlend
	{
		get
		{
			return this._targetBlendState[0].AlphaDestinationBlend;
		}
		set
		{
			this.ThrowIfBound();
			this._targetBlendState[0].AlphaDestinationBlend = value;
		}
	}

	public Blend AlphaSourceBlend
	{
		get
		{
			return this._targetBlendState[0].AlphaSourceBlend;
		}
		set
		{
			this.ThrowIfBound();
			this._targetBlendState[0].AlphaSourceBlend = value;
		}
	}

	public BlendFunction ColorBlendFunction
	{
		get
		{
			return this._targetBlendState[0].ColorBlendFunction;
		}
		set
		{
			this.ThrowIfBound();
			this._targetBlendState[0].ColorBlendFunction = value;
		}
	}

	public Blend ColorDestinationBlend
	{
		get
		{
			return this._targetBlendState[0].ColorDestinationBlend;
		}
		set
		{
			this.ThrowIfBound();
			this._targetBlendState[0].ColorDestinationBlend = value;
		}
	}

	public Blend ColorSourceBlend
	{
		get
		{
			return this._targetBlendState[0].ColorSourceBlend;
		}
		set
		{
			this.ThrowIfBound();
			this._targetBlendState[0].ColorSourceBlend = value;
		}
	}

	public ColorWriteChannels ColorWriteChannels
	{
		get
		{
			return this._targetBlendState[0].ColorWriteChannels;
		}
		set
		{
			this.ThrowIfBound();
			this._targetBlendState[0].ColorWriteChannels = value;
		}
	}

	public ColorWriteChannels ColorWriteChannels1
	{
		get
		{
			return this._targetBlendState[1].ColorWriteChannels;
		}
		set
		{
			this.ThrowIfBound();
			this._targetBlendState[1].ColorWriteChannels = value;
		}
	}

	public ColorWriteChannels ColorWriteChannels2
	{
		get
		{
			return this._targetBlendState[2].ColorWriteChannels;
		}
		set
		{
			this.ThrowIfBound();
			this._targetBlendState[2].ColorWriteChannels = value;
		}
	}

	public ColorWriteChannels ColorWriteChannels3
	{
		get
		{
			return this._targetBlendState[3].ColorWriteChannels;
		}
		set
		{
			this.ThrowIfBound();
			this._targetBlendState[3].ColorWriteChannels = value;
		}
	}

	/// <summary>
	/// The color used as blend factor when alpha blending.
	/// </summary>
	/// <remarks>
	/// <see cref="P:Microsoft.Xna.Framework.Graphics.GraphicsDevice.BlendFactor" /> is set to this value when this <see cref="T:Microsoft.Xna.Framework.Graphics.BlendState" />
	/// is bound to a GraphicsDevice.
	/// </remarks>
	public Color BlendFactor
	{
		get
		{
			return this._blendFactor;
		}
		set
		{
			this.ThrowIfBound();
			this._blendFactor = value;
		}
	}

	public int MultiSampleMask
	{
		get
		{
			return this._multiSampleMask;
		}
		set
		{
			this.ThrowIfBound();
			this._multiSampleMask = value;
		}
	}

	/// <summary>
	/// Enables use of the per-target blend states.
	/// </summary>
	public bool IndependentBlendEnable
	{
		get
		{
			return this._independentBlendEnable;
		}
		set
		{
			this.ThrowIfBound();
			this._independentBlendEnable = value;
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
			throw new InvalidOperationException("This blend state is already bound to a different graphics device.");
		}
		base.GraphicsDevice = device;
	}

	internal void ThrowIfBound()
	{
		if (this._defaultStateObject)
		{
			throw new InvalidOperationException("You cannot modify a default blend state object.");
		}
		if (base.GraphicsDevice != null)
		{
			throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");
		}
	}

	public BlendState()
	{
		this._targetBlendState = new TargetBlendState[4];
		this._targetBlendState[0] = new TargetBlendState(this);
		this._targetBlendState[1] = new TargetBlendState(this);
		this._targetBlendState[2] = new TargetBlendState(this);
		this._targetBlendState[3] = new TargetBlendState(this);
		this._blendFactor = Color.White;
		this._multiSampleMask = int.MaxValue;
		this._independentBlendEnable = false;
	}

	private BlendState(string name, Blend sourceBlend, Blend destinationBlend)
		: this()
	{
		base.Name = name;
		this.ColorSourceBlend = sourceBlend;
		this.AlphaSourceBlend = sourceBlend;
		this.ColorDestinationBlend = destinationBlend;
		this.AlphaDestinationBlend = destinationBlend;
		this._defaultStateObject = true;
	}

	private BlendState(BlendState cloneSource)
	{
		base.Name = cloneSource.Name;
		this._targetBlendState = new TargetBlendState[4];
		this._targetBlendState[0] = cloneSource[0].Clone(this);
		this._targetBlendState[1] = cloneSource[1].Clone(this);
		this._targetBlendState[2] = cloneSource[2].Clone(this);
		this._targetBlendState[3] = cloneSource[3].Clone(this);
		this._blendFactor = cloneSource._blendFactor;
		this._multiSampleMask = cloneSource._multiSampleMask;
		this._independentBlendEnable = cloneSource._independentBlendEnable;
	}

	static BlendState()
	{
		BlendState.Additive = new BlendState("BlendState.Additive", Blend.SourceAlpha, Blend.One);
		BlendState.AlphaBlend = new BlendState("BlendState.AlphaBlend", Blend.One, Blend.InverseSourceAlpha);
		BlendState.NonPremultiplied = new BlendState("BlendState.NonPremultiplied", Blend.SourceAlpha, Blend.InverseSourceAlpha);
		BlendState.Opaque = new BlendState("BlendState.Opaque", Blend.One, Blend.Zero);
	}

	internal BlendState Clone()
	{
		return new BlendState(this);
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed)
		{
			for (int i = 0; i < this._targetBlendState.Length; i++)
			{
				this._targetBlendState[i] = null;
			}
		}
		base.Dispose(disposing);
	}

	internal void PlatformApplyState(GraphicsDevice device, bool force = false)
	{
		bool blendEnabled = this.ColorSourceBlend != Blend.One || this.ColorDestinationBlend != Blend.Zero || this.AlphaSourceBlend != Blend.One || this.AlphaDestinationBlend != Blend.Zero;
		if (force || blendEnabled != device._lastBlendEnable)
		{
			if (blendEnabled)
			{
				GL.Enable(EnableCap.Blend);
			}
			else
			{
				GL.Disable(EnableCap.Blend);
			}
			device._lastBlendEnable = blendEnabled;
		}
		if (this._independentBlendEnable)
		{
			for (int i = 0; i < 4; i++)
			{
				if (force || this._targetBlendState[i].ColorBlendFunction != device._lastBlendState[i].ColorBlendFunction || this._targetBlendState[i].AlphaBlendFunction != device._lastBlendState[i].AlphaBlendFunction)
				{
					GL.BlendEquationSeparatei(i, this._targetBlendState[i].ColorBlendFunction.GetBlendEquationMode(), this._targetBlendState[i].AlphaBlendFunction.GetBlendEquationMode());
					device._lastBlendState[i].ColorBlendFunction = this._targetBlendState[i].ColorBlendFunction;
					device._lastBlendState[i].AlphaBlendFunction = this._targetBlendState[i].AlphaBlendFunction;
				}
				if (force || this._targetBlendState[i].ColorSourceBlend != device._lastBlendState[i].ColorSourceBlend || this._targetBlendState[i].ColorDestinationBlend != device._lastBlendState[i].ColorDestinationBlend || this._targetBlendState[i].AlphaSourceBlend != device._lastBlendState[i].AlphaSourceBlend || this._targetBlendState[i].AlphaDestinationBlend != device._lastBlendState[i].AlphaDestinationBlend)
				{
					GL.BlendFuncSeparatei(i, this._targetBlendState[i].ColorSourceBlend.GetBlendFactorSrc(), this._targetBlendState[i].ColorDestinationBlend.GetBlendFactorDest(), this._targetBlendState[i].AlphaSourceBlend.GetBlendFactorSrc(), this._targetBlendState[i].AlphaDestinationBlend.GetBlendFactorDest());
					device._lastBlendState[i].ColorSourceBlend = this._targetBlendState[i].ColorSourceBlend;
					device._lastBlendState[i].ColorDestinationBlend = this._targetBlendState[i].ColorDestinationBlend;
					device._lastBlendState[i].AlphaSourceBlend = this._targetBlendState[i].AlphaSourceBlend;
					device._lastBlendState[i].AlphaDestinationBlend = this._targetBlendState[i].AlphaDestinationBlend;
				}
			}
		}
		else
		{
			if (force || this.ColorBlendFunction != device._lastBlendState.ColorBlendFunction || this.AlphaBlendFunction != device._lastBlendState.AlphaBlendFunction)
			{
				GL.BlendEquationSeparate(this.ColorBlendFunction.GetBlendEquationMode(), this.AlphaBlendFunction.GetBlendEquationMode());
				for (int j = 0; j < 4; j++)
				{
					device._lastBlendState[j].ColorBlendFunction = this.ColorBlendFunction;
					device._lastBlendState[j].AlphaBlendFunction = this.AlphaBlendFunction;
				}
			}
			if (force || this.ColorSourceBlend != device._lastBlendState.ColorSourceBlend || this.ColorDestinationBlend != device._lastBlendState.ColorDestinationBlend || this.AlphaSourceBlend != device._lastBlendState.AlphaSourceBlend || this.AlphaDestinationBlend != device._lastBlendState.AlphaDestinationBlend)
			{
				GL.BlendFuncSeparate(this.ColorSourceBlend.GetBlendFactorSrc(), this.ColorDestinationBlend.GetBlendFactorDest(), this.AlphaSourceBlend.GetBlendFactorSrc(), this.AlphaDestinationBlend.GetBlendFactorDest());
				for (int k = 0; k < 4; k++)
				{
					device._lastBlendState[k].ColorSourceBlend = this.ColorSourceBlend;
					device._lastBlendState[k].ColorDestinationBlend = this.ColorDestinationBlend;
					device._lastBlendState[k].AlphaSourceBlend = this.AlphaSourceBlend;
					device._lastBlendState[k].AlphaDestinationBlend = this.AlphaDestinationBlend;
				}
			}
		}
		if (force || this.ColorWriteChannels != device._lastBlendState.ColorWriteChannels)
		{
			GL.ColorMask((this.ColorWriteChannels & ColorWriteChannels.Red) != 0, (this.ColorWriteChannels & ColorWriteChannels.Green) != 0, (this.ColorWriteChannels & ColorWriteChannels.Blue) != 0, (this.ColorWriteChannels & ColorWriteChannels.Alpha) != 0);
			device._lastBlendState.ColorWriteChannels = this.ColorWriteChannels;
		}
	}
}
