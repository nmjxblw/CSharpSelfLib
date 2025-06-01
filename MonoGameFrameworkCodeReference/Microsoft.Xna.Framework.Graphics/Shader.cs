using System;
using System.IO;
using System.Text;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

internal class Shader : GraphicsResource
{
	private int _shaderHandle = -1;

	private string _glslCode;

	/// <summary>
	/// Returns the platform specific shader profile identifier.
	/// </summary>
	public static int Profile => Shader.PlatformProfile();

	/// <summary>
	/// A hash value which can be used to compare shaders.
	/// </summary>
	internal int HashKey { get; private set; }

	public SamplerInfo[] Samplers { get; private set; }

	public int[] CBuffers { get; private set; }

	public ShaderStage Stage { get; private set; }

	public VertexAttribute[] Attributes { get; private set; }

	internal Shader(GraphicsDevice device, BinaryReader reader)
	{
		base.GraphicsDevice = device;
		bool isVertexShader = reader.ReadBoolean();
		this.Stage = ((!isVertexShader) ? ShaderStage.Pixel : ShaderStage.Vertex);
		int shaderLength = reader.ReadInt32();
		byte[] shaderBytecode = reader.ReadBytes(shaderLength);
		int samplerCount = reader.ReadByte();
		this.Samplers = new SamplerInfo[samplerCount];
		for (int s = 0; s < samplerCount; s++)
		{
			this.Samplers[s].type = (SamplerType)reader.ReadByte();
			this.Samplers[s].textureSlot = reader.ReadByte();
			this.Samplers[s].samplerSlot = reader.ReadByte();
			if (reader.ReadBoolean())
			{
				this.Samplers[s].state = new SamplerState();
				this.Samplers[s].state.AddressU = (TextureAddressMode)reader.ReadByte();
				this.Samplers[s].state.AddressV = (TextureAddressMode)reader.ReadByte();
				this.Samplers[s].state.AddressW = (TextureAddressMode)reader.ReadByte();
				this.Samplers[s].state.BorderColor = new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
				this.Samplers[s].state.Filter = (TextureFilter)reader.ReadByte();
				this.Samplers[s].state.MaxAnisotropy = reader.ReadInt32();
				this.Samplers[s].state.MaxMipLevel = reader.ReadInt32();
				this.Samplers[s].state.MipMapLevelOfDetailBias = reader.ReadSingle();
			}
			this.Samplers[s].name = reader.ReadString();
			this.Samplers[s].parameter = reader.ReadByte();
		}
		int cbufferCount = reader.ReadByte();
		this.CBuffers = new int[cbufferCount];
		for (int c = 0; c < cbufferCount; c++)
		{
			this.CBuffers[c] = reader.ReadByte();
		}
		int attributeCount = reader.ReadByte();
		this.Attributes = new VertexAttribute[attributeCount];
		for (int a = 0; a < attributeCount; a++)
		{
			this.Attributes[a].name = reader.ReadString();
			this.Attributes[a].usage = (VertexElementUsage)reader.ReadByte();
			this.Attributes[a].index = reader.ReadByte();
			this.Attributes[a].location = reader.ReadInt16();
		}
		this.PlatformConstruct(this.Stage, shaderBytecode);
	}

	protected internal override void GraphicsDeviceResetting()
	{
		this.PlatformGraphicsDeviceResetting();
	}

	private static int PlatformProfile()
	{
		return 0;
	}

	private void PlatformConstruct(ShaderStage stage, byte[] shaderBytecode)
	{
		this._glslCode = Encoding.ASCII.GetString(shaderBytecode);
		this.HashKey = Hash.ComputeHash(shaderBytecode);
	}

	internal int GetShaderHandle()
	{
		if (this._shaderHandle != -1)
		{
			return this._shaderHandle;
		}
		this._shaderHandle = GL.CreateShader((this.Stage == ShaderStage.Vertex) ? ShaderType.VertexShader : ShaderType.FragmentShader);
		GL.ShaderSource(this._shaderHandle, this._glslCode);
		GL.CompileShader(this._shaderHandle);
		int compiled = 0;
		GL.GetShader(this._shaderHandle, ShaderParameter.CompileStatus, out compiled);
		if (compiled != 1)
		{
			GL.GetShaderInfoLog(this._shaderHandle);
			base.GraphicsDevice.DisposeShader(this._shaderHandle);
			this._shaderHandle = -1;
			throw new InvalidOperationException("Shader Compilation Failed");
		}
		return this._shaderHandle;
	}

	internal void GetVertexAttributeLocations(int program)
	{
		for (int i = 0; i < this.Attributes.Length; i++)
		{
			this.Attributes[i].location = GL.GetAttribLocation(program, this.Attributes[i].name);
		}
	}

	internal int GetAttribLocation(VertexElementUsage usage, int index)
	{
		for (int i = 0; i < this.Attributes.Length; i++)
		{
			if (this.Attributes[i].usage == usage && this.Attributes[i].index == index)
			{
				return this.Attributes[i].location;
			}
		}
		return -1;
	}

	internal void ApplySamplerTextureUnits(int program)
	{
		SamplerInfo[] samplers = this.Samplers;
		for (int i = 0; i < samplers.Length; i++)
		{
			SamplerInfo sampler = samplers[i];
			int loc = GL.GetUniformLocation(program, sampler.name);
			if (loc != -1)
			{
				GL.Uniform1(loc, sampler.textureSlot);
			}
		}
	}

	private void PlatformGraphicsDeviceResetting()
	{
		if (this._shaderHandle != -1)
		{
			base.GraphicsDevice.DisposeShader(this._shaderHandle);
			this._shaderHandle = -1;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed && this._shaderHandle != -1)
		{
			base.GraphicsDevice.DisposeShader(this._shaderHandle);
			this._shaderHandle = -1;
		}
		base.Dispose(disposing);
	}
}
