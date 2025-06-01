using System;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

public sealed class TextureCollection
{
	private readonly GraphicsDevice _graphicsDevice;

	private readonly Texture[] _textures;

	private readonly bool _applyToVertexStage;

	private int _dirty;

	private TextureTarget[] _targets;

	public Texture this[int index]
	{
		get
		{
			return this._textures[index];
		}
		set
		{
			if (this._applyToVertexStage && !this._graphicsDevice.GraphicsCapabilities.SupportsVertexTextures)
			{
				throw new NotSupportedException("Vertex textures are not supported on this device.");
			}
			if (this._textures[index] != value)
			{
				this._textures[index] = value;
				this._dirty |= 1 << index;
			}
		}
	}

	internal TextureCollection(GraphicsDevice graphicsDevice, int maxTextures, bool applyToVertexStage)
	{
		this._graphicsDevice = graphicsDevice;
		this._textures = new Texture[maxTextures];
		this._applyToVertexStage = applyToVertexStage;
		this._dirty = int.MaxValue;
		this.PlatformInit();
	}

	internal void Clear()
	{
		for (int i = 0; i < this._textures.Length; i++)
		{
			this._textures[i] = null;
		}
		this.PlatformClear();
		this._dirty = int.MaxValue;
	}

	/// <summary>
	/// Marks all texture slots as dirty.
	/// </summary>
	internal void Dirty()
	{
		this._dirty = int.MaxValue;
	}

	internal void SetTextures(GraphicsDevice device)
	{
		if (!this._applyToVertexStage || device.GraphicsCapabilities.SupportsVertexTextures)
		{
			this.PlatformSetTextures(device);
		}
	}

	private void PlatformInit()
	{
		this._targets = new TextureTarget[this._textures.Length];
	}

	private void PlatformClear()
	{
		for (int i = 0; i < this._targets.Length; i++)
		{
			this._targets[i] = (TextureTarget)0;
		}
	}

	private void PlatformSetTextures(GraphicsDevice device)
	{
		if (this._dirty == 0)
		{
			return;
		}
		for (int i = 0; i < this._textures.Length; i++)
		{
			int mask = 1 << i;
			if ((this._dirty & mask) != 0)
			{
				Texture tex = this._textures[i];
				TextureTarget? bindTarget = null;
				int bindTexture = 0;
				if (this._targets[i] != 0 && (tex == null || this._targets[i] != tex.glTarget))
				{
					bindTarget = this._targets[i];
					bindTexture = 0;
					this._targets[i] = (TextureTarget)0;
				}
				if (tex != null)
				{
					this._targets[i] = tex.glTarget;
					bindTarget = tex.glTarget;
					bindTexture = tex.glTexture;
					this._graphicsDevice._graphicsMetrics._textureCount++;
				}
				if (bindTarget.HasValue)
				{
					GL.ActiveTexture((TextureUnit)(33984 + i));
					GL.BindTexture(bindTarget.Value, bindTexture);
				}
				this._dirty &= ~mask;
				if (this._dirty == 0)
				{
					break;
				}
			}
		}
		this._dirty = 0;
	}
}
