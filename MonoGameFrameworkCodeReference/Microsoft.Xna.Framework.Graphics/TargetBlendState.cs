namespace Microsoft.Xna.Framework.Graphics;

public class TargetBlendState
{
	private readonly BlendState _parent;

	private BlendFunction _alphaBlendFunction;

	private Blend _alphaDestinationBlend;

	private Blend _alphaSourceBlend;

	private BlendFunction _colorBlendFunction;

	private Blend _colorDestinationBlend;

	private Blend _colorSourceBlend;

	private ColorWriteChannels _colorWriteChannels;

	public BlendFunction AlphaBlendFunction
	{
		get
		{
			return this._alphaBlendFunction;
		}
		set
		{
			this._parent.ThrowIfBound();
			this._alphaBlendFunction = value;
		}
	}

	public Blend AlphaDestinationBlend
	{
		get
		{
			return this._alphaDestinationBlend;
		}
		set
		{
			this._parent.ThrowIfBound();
			this._alphaDestinationBlend = value;
		}
	}

	public Blend AlphaSourceBlend
	{
		get
		{
			return this._alphaSourceBlend;
		}
		set
		{
			this._parent.ThrowIfBound();
			this._alphaSourceBlend = value;
		}
	}

	public BlendFunction ColorBlendFunction
	{
		get
		{
			return this._colorBlendFunction;
		}
		set
		{
			this._parent.ThrowIfBound();
			this._colorBlendFunction = value;
		}
	}

	public Blend ColorDestinationBlend
	{
		get
		{
			return this._colorDestinationBlend;
		}
		set
		{
			this._parent.ThrowIfBound();
			this._colorDestinationBlend = value;
		}
	}

	public Blend ColorSourceBlend
	{
		get
		{
			return this._colorSourceBlend;
		}
		set
		{
			this._parent.ThrowIfBound();
			this._colorSourceBlend = value;
		}
	}

	public ColorWriteChannels ColorWriteChannels
	{
		get
		{
			return this._colorWriteChannels;
		}
		set
		{
			this._parent.ThrowIfBound();
			this._colorWriteChannels = value;
		}
	}

	internal TargetBlendState(BlendState parent)
	{
		this._parent = parent;
		this.AlphaBlendFunction = BlendFunction.Add;
		this.AlphaDestinationBlend = Blend.Zero;
		this.AlphaSourceBlend = Blend.One;
		this.ColorBlendFunction = BlendFunction.Add;
		this.ColorDestinationBlend = Blend.Zero;
		this.ColorSourceBlend = Blend.One;
		this.ColorWriteChannels = ColorWriteChannels.All;
	}

	internal TargetBlendState Clone(BlendState parent)
	{
		return new TargetBlendState(parent)
		{
			AlphaBlendFunction = this.AlphaBlendFunction,
			AlphaDestinationBlend = this.AlphaDestinationBlend,
			AlphaSourceBlend = this.AlphaSourceBlend,
			ColorBlendFunction = this.ColorBlendFunction,
			ColorDestinationBlend = this.ColorDestinationBlend,
			ColorSourceBlend = this.ColorSourceBlend,
			ColorWriteChannels = this.ColorWriteChannels
		};
	}
}
