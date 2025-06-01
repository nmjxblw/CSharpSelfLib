using System;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

public sealed class SamplerStateCollection
{
	private readonly GraphicsDevice _graphicsDevice;

	private readonly SamplerState _samplerStateAnisotropicClamp;

	private readonly SamplerState _samplerStateAnisotropicWrap;

	private readonly SamplerState _samplerStateLinearClamp;

	private readonly SamplerState _samplerStateLinearWrap;

	private readonly SamplerState _samplerStatePointClamp;

	private readonly SamplerState _samplerStatePointWrap;

	private readonly SamplerState[] _samplers;

	private readonly SamplerState[] _actualSamplers;

	private readonly bool _applyToVertexStage;

	public SamplerState this[int index]
	{
		get
		{
			return this._samplers[index];
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (this._samplers[index] != value)
			{
				this._samplers[index] = value;
				SamplerState newSamplerState = value;
				if (value == SamplerState.AnisotropicClamp)
				{
					newSamplerState = this._samplerStateAnisotropicClamp;
				}
				else if (value == SamplerState.AnisotropicWrap)
				{
					newSamplerState = this._samplerStateAnisotropicWrap;
				}
				else if (value == SamplerState.LinearClamp)
				{
					newSamplerState = this._samplerStateLinearClamp;
				}
				else if (value == SamplerState.LinearWrap)
				{
					newSamplerState = this._samplerStateLinearWrap;
				}
				else if (value == SamplerState.PointClamp)
				{
					newSamplerState = this._samplerStatePointClamp;
				}
				else if (value == SamplerState.PointWrap)
				{
					newSamplerState = this._samplerStatePointWrap;
				}
				newSamplerState.BindToGraphicsDevice(this._graphicsDevice);
				this._actualSamplers[index] = newSamplerState;
				this.PlatformSetSamplerState(index);
			}
		}
	}

	internal SamplerStateCollection(GraphicsDevice device, int maxSamplers, bool applyToVertexStage)
	{
		this._graphicsDevice = device;
		this._samplerStateAnisotropicClamp = SamplerState.AnisotropicClamp.Clone();
		this._samplerStateAnisotropicWrap = SamplerState.AnisotropicWrap.Clone();
		this._samplerStateLinearClamp = SamplerState.LinearClamp.Clone();
		this._samplerStateLinearWrap = SamplerState.LinearWrap.Clone();
		this._samplerStatePointClamp = SamplerState.PointClamp.Clone();
		this._samplerStatePointWrap = SamplerState.PointWrap.Clone();
		this._samplers = new SamplerState[maxSamplers];
		this._actualSamplers = new SamplerState[maxSamplers];
		this._applyToVertexStage = applyToVertexStage;
		this.Clear();
	}

	internal void Clear()
	{
		for (int i = 0; i < this._samplers.Length; i++)
		{
			this._samplers[i] = SamplerState.LinearWrap;
			this._samplerStateLinearWrap.BindToGraphicsDevice(this._graphicsDevice);
			this._actualSamplers[i] = this._samplerStateLinearWrap;
		}
		this.PlatformClear();
	}

	/// <summary>
	/// Mark all the sampler slots as dirty.
	/// </summary>
	internal void Dirty()
	{
		this.PlatformDirty();
	}

	private void PlatformSetSamplerState(int index)
	{
	}

	private void PlatformClear()
	{
	}

	private void PlatformDirty()
	{
	}

	internal void PlatformSetSamplers(GraphicsDevice device)
	{
		for (int i = 0; i < this._actualSamplers.Length; i++)
		{
			SamplerState sampler = this._actualSamplers[i];
			Texture texture = device.Textures[i];
			if (sampler != null && texture != null && sampler != texture.glLastSamplerState)
			{
				GL.ActiveTexture((TextureUnit)(33984 + i));
				sampler.Activate(device, texture.glTarget, texture.LevelCount > 1);
				texture.glLastSamplerState = sampler;
			}
		}
	}
}
