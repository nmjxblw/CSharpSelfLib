using System;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

public class SamplerState : GraphicsResource
{
	public static readonly SamplerState AnisotropicClamp;

	public static readonly SamplerState AnisotropicWrap;

	public static readonly SamplerState LinearClamp;

	public static readonly SamplerState LinearWrap;

	public static readonly SamplerState PointClamp;

	public static readonly SamplerState PointWrap;

	private readonly bool _defaultStateObject;

	private TextureAddressMode _addressU;

	private TextureAddressMode _addressV;

	private TextureAddressMode _addressW;

	private Color _borderColor;

	private TextureFilter _filter;

	private int _maxAnisotropy;

	private int _maxMipLevel;

	private float _mipMapLevelOfDetailBias;

	private TextureFilterMode _filterMode;

	private CompareFunction _comparisonFunction;

	private readonly float[] _openGLBorderColor = new float[4];

	internal const TextureParameterName TextureParameterNameTextureMaxAnisotropy = TextureParameterName.TextureMaxAnisotropyExt;

	internal const TextureParameterName TextureParameterNameTextureMaxLevel = TextureParameterName.TextureMaxLevel;

	public TextureAddressMode AddressU
	{
		get
		{
			return this._addressU;
		}
		set
		{
			this.ThrowIfBound();
			this._addressU = value;
		}
	}

	public TextureAddressMode AddressV
	{
		get
		{
			return this._addressV;
		}
		set
		{
			this.ThrowIfBound();
			this._addressV = value;
		}
	}

	public TextureAddressMode AddressW
	{
		get
		{
			return this._addressW;
		}
		set
		{
			this.ThrowIfBound();
			this._addressW = value;
		}
	}

	public Color BorderColor
	{
		get
		{
			return this._borderColor;
		}
		set
		{
			this.ThrowIfBound();
			this._borderColor = value;
		}
	}

	public TextureFilter Filter
	{
		get
		{
			return this._filter;
		}
		set
		{
			this.ThrowIfBound();
			this._filter = value;
		}
	}

	public int MaxAnisotropy
	{
		get
		{
			return this._maxAnisotropy;
		}
		set
		{
			this.ThrowIfBound();
			this._maxAnisotropy = value;
		}
	}

	public int MaxMipLevel
	{
		get
		{
			return this._maxMipLevel;
		}
		set
		{
			this.ThrowIfBound();
			this._maxMipLevel = value;
		}
	}

	public float MipMapLevelOfDetailBias
	{
		get
		{
			return this._mipMapLevelOfDetailBias;
		}
		set
		{
			this.ThrowIfBound();
			this._mipMapLevelOfDetailBias = value;
		}
	}

	/// <summary>
	/// When using comparison sampling, also set <see cref="P:Microsoft.Xna.Framework.Graphics.SamplerState.FilterMode" /> to <see cref="F:Microsoft.Xna.Framework.Graphics.TextureFilterMode.Comparison" />.
	/// </summary>
	public CompareFunction ComparisonFunction
	{
		get
		{
			return this._comparisonFunction;
		}
		set
		{
			this.ThrowIfBound();
			this._comparisonFunction = value;
		}
	}

	public TextureFilterMode FilterMode
	{
		get
		{
			return this._filterMode;
		}
		set
		{
			this.ThrowIfBound();
			this._filterMode = value;
		}
	}

	static SamplerState()
	{
		SamplerState.AnisotropicClamp = new SamplerState("SamplerState.AnisotropicClamp", TextureFilter.Anisotropic, TextureAddressMode.Clamp);
		SamplerState.AnisotropicWrap = new SamplerState("SamplerState.AnisotropicWrap", TextureFilter.Anisotropic, TextureAddressMode.Wrap);
		SamplerState.LinearClamp = new SamplerState("SamplerState.LinearClamp", TextureFilter.Linear, TextureAddressMode.Clamp);
		SamplerState.LinearWrap = new SamplerState("SamplerState.LinearWrap", TextureFilter.Linear, TextureAddressMode.Wrap);
		SamplerState.PointClamp = new SamplerState("SamplerState.PointClamp", TextureFilter.Point, TextureAddressMode.Clamp);
		SamplerState.PointWrap = new SamplerState("SamplerState.PointWrap", TextureFilter.Point, TextureAddressMode.Wrap);
	}

	internal void BindToGraphicsDevice(GraphicsDevice device)
	{
		if (this._defaultStateObject)
		{
			throw new InvalidOperationException("You cannot bind a default state object.");
		}
		if (base.GraphicsDevice != null && base.GraphicsDevice != device)
		{
			throw new InvalidOperationException("This sampler state is already bound to a different graphics device.");
		}
		base.GraphicsDevice = device;
	}

	internal void ThrowIfBound()
	{
		if (this._defaultStateObject)
		{
			throw new InvalidOperationException("You cannot modify a default sampler state object.");
		}
		if (base.GraphicsDevice != null)
		{
			throw new InvalidOperationException("You cannot modify the sampler state after it has been bound to the graphics device!");
		}
	}

	public SamplerState()
	{
		this.Filter = TextureFilter.Linear;
		this.AddressU = TextureAddressMode.Wrap;
		this.AddressV = TextureAddressMode.Wrap;
		this.AddressW = TextureAddressMode.Wrap;
		this.BorderColor = Color.White;
		this.MaxAnisotropy = 4;
		this.MaxMipLevel = 0;
		this.MipMapLevelOfDetailBias = 0f;
		this.ComparisonFunction = CompareFunction.Never;
		this.FilterMode = TextureFilterMode.Default;
	}

	private SamplerState(string name, TextureFilter filter, TextureAddressMode addressMode)
		: this()
	{
		base.Name = name;
		this._filter = filter;
		this._addressU = addressMode;
		this._addressV = addressMode;
		this._addressW = addressMode;
		this._defaultStateObject = true;
	}

	private SamplerState(SamplerState cloneSource)
	{
		base.Name = cloneSource.Name;
		this._filter = cloneSource._filter;
		this._addressU = cloneSource._addressU;
		this._addressV = cloneSource._addressV;
		this._addressW = cloneSource._addressW;
		this._borderColor = cloneSource._borderColor;
		this._maxAnisotropy = cloneSource._maxAnisotropy;
		this._maxMipLevel = cloneSource._maxMipLevel;
		this._mipMapLevelOfDetailBias = cloneSource._mipMapLevelOfDetailBias;
		this._comparisonFunction = cloneSource._comparisonFunction;
		this._filterMode = cloneSource._filterMode;
	}

	internal SamplerState Clone()
	{
		return new SamplerState(this);
	}

	protected override void Dispose(bool disposing)
	{
		_ = base.IsDisposed;
		base.Dispose(disposing);
	}

	internal void Activate(GraphicsDevice device, TextureTarget target, bool useMipmaps = false)
	{
		if (base.GraphicsDevice == null)
		{
			base.GraphicsDevice = device;
		}
		switch (this.Filter)
		{
		case TextureFilter.Point:
			if (base.GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
			{
				GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropyExt, 1f);
			}
			GL.TexParameter(target, TextureParameterName.TextureMinFilter, useMipmaps ? 9984 : 9728);
			GL.TexParameter(target, TextureParameterName.TextureMagFilter, 9728);
			break;
		case TextureFilter.Linear:
			if (base.GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
			{
				GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropyExt, 1f);
			}
			GL.TexParameter(target, TextureParameterName.TextureMinFilter, useMipmaps ? 9987 : 9729);
			GL.TexParameter(target, TextureParameterName.TextureMagFilter, 9729);
			break;
		case TextureFilter.Anisotropic:
			if (base.GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
			{
				GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropyExt, MathHelper.Clamp(this.MaxAnisotropy, 1f, base.GraphicsDevice.GraphicsCapabilities.MaxTextureAnisotropy));
			}
			GL.TexParameter(target, TextureParameterName.TextureMinFilter, useMipmaps ? 9987 : 9729);
			GL.TexParameter(target, TextureParameterName.TextureMagFilter, 9729);
			break;
		case TextureFilter.PointMipLinear:
			if (base.GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
			{
				GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropyExt, 1f);
			}
			GL.TexParameter(target, TextureParameterName.TextureMinFilter, useMipmaps ? 9986 : 9728);
			GL.TexParameter(target, TextureParameterName.TextureMagFilter, 9728);
			break;
		case TextureFilter.LinearMipPoint:
			if (base.GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
			{
				GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropyExt, 1f);
			}
			GL.TexParameter(target, TextureParameterName.TextureMinFilter, useMipmaps ? 9985 : 9729);
			GL.TexParameter(target, TextureParameterName.TextureMagFilter, 9729);
			break;
		case TextureFilter.MinLinearMagPointMipLinear:
			if (base.GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
			{
				GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropyExt, 1f);
			}
			GL.TexParameter(target, TextureParameterName.TextureMinFilter, useMipmaps ? 9987 : 9729);
			GL.TexParameter(target, TextureParameterName.TextureMagFilter, 9728);
			break;
		case TextureFilter.MinLinearMagPointMipPoint:
			if (base.GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
			{
				GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropyExt, 1f);
			}
			GL.TexParameter(target, TextureParameterName.TextureMinFilter, useMipmaps ? 9985 : 9729);
			GL.TexParameter(target, TextureParameterName.TextureMagFilter, 9728);
			break;
		case TextureFilter.MinPointMagLinearMipLinear:
			if (base.GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
			{
				GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropyExt, 1f);
			}
			GL.TexParameter(target, TextureParameterName.TextureMinFilter, useMipmaps ? 9986 : 9728);
			GL.TexParameter(target, TextureParameterName.TextureMagFilter, 9729);
			break;
		case TextureFilter.MinPointMagLinearMipPoint:
			if (base.GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
			{
				GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropyExt, 1f);
			}
			GL.TexParameter(target, TextureParameterName.TextureMinFilter, useMipmaps ? 9984 : 9728);
			GL.TexParameter(target, TextureParameterName.TextureMagFilter, 9729);
			break;
		default:
			throw new NotSupportedException();
		}
		GL.TexParameter(target, TextureParameterName.TextureWrapS, this.GetWrapMode(this.AddressU));
		GL.TexParameter(target, TextureParameterName.TextureWrapT, this.GetWrapMode(this.AddressV));
		this._openGLBorderColor[0] = (float)(int)this.BorderColor.R / 255f;
		this._openGLBorderColor[1] = (float)(int)this.BorderColor.G / 255f;
		this._openGLBorderColor[2] = (float)(int)this.BorderColor.B / 255f;
		this._openGLBorderColor[3] = (float)(int)this.BorderColor.A / 255f;
		GL.TexParameter(target, TextureParameterName.TextureBorderColor, this._openGLBorderColor);
		GL.TexParameter(target, TextureParameterName.TextureLodBias, this.MipMapLevelOfDetailBias);
		switch (this.FilterMode)
		{
		case TextureFilterMode.Comparison:
			GL.TexParameter(target, TextureParameterName.TextureCompareMode, 34894);
			GL.TexParameter(target, TextureParameterName.TextureCompareFunc, (int)this.ComparisonFunction.GetDepthFunction());
			break;
		case TextureFilterMode.Default:
			GL.TexParameter(target, TextureParameterName.TextureCompareMode, 0);
			break;
		default:
			throw new InvalidOperationException("Invalid filter mode!");
		}
		if (base.GraphicsDevice.GraphicsCapabilities.SupportsTextureMaxLevel)
		{
			if (this.MaxMipLevel > 0)
			{
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, this.MaxMipLevel);
			}
			else
			{
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 1000);
			}
		}
	}

	private int GetWrapMode(TextureAddressMode textureAddressMode)
	{
		return textureAddressMode switch
		{
			TextureAddressMode.Clamp => 33071, 
			TextureAddressMode.Wrap => 10497, 
			TextureAddressMode.Mirror => 33648, 
			TextureAddressMode.Border => 33069, 
			_ => throw new ArgumentException("No support for " + textureAddressMode), 
		};
	}
}
