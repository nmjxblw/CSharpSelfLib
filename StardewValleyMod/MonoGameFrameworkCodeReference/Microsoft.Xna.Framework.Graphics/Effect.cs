using System;
using System.IO;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics;

public class Effect : GraphicsResource
{
	private struct MGFXHeader
	{
		/// <summary>
		/// The MonoGame Effect file format header identifier ("MGFX"). 
		/// </summary>
		public static readonly int MGFXSignature = (BitConverter.IsLittleEndian ? 1481000781 : 1296516696);

		/// <summary>
		/// The current MonoGame Effect file format versions
		/// used to detect old packaged content.
		/// </summary>
		/// <remarks>
		/// We should avoid supporting old versions for very long if at all 
		/// as users should be rebuilding content when packaging their game.
		/// </remarks>
		public const int MGFXVersion = 9;

		public int Signature;

		public int Version;

		public int Profile;

		public int EffectKey;

		public int HeaderSize;
	}

	private Shader[] _shaders;

	private readonly bool _isClone;

	public EffectParameterCollection Parameters { get; private set; }

	public EffectTechniqueCollection Techniques { get; private set; }

	public EffectTechnique CurrentTechnique { get; set; }

	internal ConstantBuffer[] ConstantBuffers { get; private set; }

	internal Effect(GraphicsDevice graphicsDevice)
	{
		if (graphicsDevice == null)
		{
			throw new ArgumentNullException("graphicsDevice", "The GraphicsDevice must not be null when creating new resources.");
		}
		base.GraphicsDevice = graphicsDevice;
	}

	protected Effect(Effect cloneSource)
		: this(cloneSource.GraphicsDevice)
	{
		this._isClone = true;
		this.Clone(cloneSource);
	}

	public Effect(GraphicsDevice graphicsDevice, byte[] effectCode)
		: this(graphicsDevice, effectCode, 0, effectCode.Length)
	{
	}

	public Effect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count)
		: this(graphicsDevice)
	{
		MGFXHeader mGFXHeader = this.ReadHeader(effectCode, index);
		int effectKey = mGFXHeader.EffectKey;
		int headerSize = mGFXHeader.HeaderSize;
		if (!graphicsDevice.EffectCache.TryGetValue(effectKey, out var cloneSource))
		{
			using MemoryStream stream = new MemoryStream(effectCode, index + headerSize, count - headerSize, writable: false);
			using BinaryReaderEx reader = new BinaryReaderEx(stream);
			cloneSource = new Effect(graphicsDevice);
			cloneSource.ReadEffect(reader);
			graphicsDevice.EffectCache.Add(effectKey, cloneSource);
		}
		this._isClone = true;
		this.Clone(cloneSource);
	}

	private MGFXHeader ReadHeader(byte[] effectCode, int index)
	{
		MGFXHeader header = default(MGFXHeader);
		header.Signature = BitConverter.ToInt32(effectCode, index);
		index += 4;
		header.Version = effectCode[index++];
		header.Profile = effectCode[index++];
		header.EffectKey = BitConverter.ToInt32(effectCode, index);
		index += 4;
		header.HeaderSize = index;
		if (header.Signature != MGFXHeader.MGFXSignature)
		{
			throw new Exception("This does not appear to be a MonoGame MGFX file!");
		}
		if (header.Version < 9)
		{
			throw new Exception("This MGFX effect is for an older release of MonoGame and needs to be rebuilt.");
		}
		if (header.Version > 9)
		{
			throw new Exception("This MGFX effect seems to be for a newer release of MonoGame.");
		}
		if (header.Profile != Shader.Profile)
		{
			throw new Exception("This MGFX effect was built for a different platform!");
		}
		return header;
	}

	/// <summary>
	/// Clone the source into this existing object.
	/// </summary>
	/// <remarks>
	/// Note this is not overloaded in derived classes on purpose.  This is
	/// only a reason this exists is for caching effects.
	/// </remarks>
	/// <param name="cloneSource">The source effect to clone from.</param>
	private void Clone(Effect cloneSource)
	{
		this.Parameters = cloneSource.Parameters.Clone();
		this.Techniques = cloneSource.Techniques.Clone(this);
		this.ConstantBuffers = new ConstantBuffer[cloneSource.ConstantBuffers.Length];
		for (int i = 0; i < cloneSource.ConstantBuffers.Length; i++)
		{
			this.ConstantBuffers[i] = new ConstantBuffer(cloneSource.ConstantBuffers[i]);
		}
		for (int j = 0; j < cloneSource.Techniques.Count; j++)
		{
			if (cloneSource.Techniques[j] == cloneSource.CurrentTechnique)
			{
				this.CurrentTechnique = this.Techniques[j];
				break;
			}
		}
		this._shaders = cloneSource._shaders;
	}

	/// <summary>
	/// Returns a deep copy of the effect where immutable types 
	/// are shared and mutable data is duplicated.
	/// </summary>
	/// <remarks>
	/// See "Cloning an Effect" in MSDN:
	/// http://msdn.microsoft.com/en-us/library/windows/desktop/ff476138(v=vs.85).aspx
	/// </remarks>
	/// <returns>The cloned effect.</returns>
	public virtual Effect Clone()
	{
		return new Effect(this);
	}

	protected internal virtual void OnApply()
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed && disposing)
		{
			if (!this._isClone && this._shaders != null)
			{
				Shader[] shaders = this._shaders;
				for (int i = 0; i < shaders.Length; i++)
				{
					shaders[i].Dispose();
				}
			}
			if (this.ConstantBuffers != null)
			{
				ConstantBuffer[] constantBuffers = this.ConstantBuffers;
				for (int i = 0; i < constantBuffers.Length; i++)
				{
					constantBuffers[i].Dispose();
				}
				this.ConstantBuffers = null;
			}
		}
		base.Dispose(disposing);
	}

	protected internal override void GraphicsDeviceResetting()
	{
		for (int i = 0; i < this.ConstantBuffers.Length; i++)
		{
			this.ConstantBuffers[i].Clear();
		}
	}

	private void ReadEffect(BinaryReaderEx reader)
	{
		int buffers = reader.ReadByte();
		this.ConstantBuffers = new ConstantBuffer[buffers];
		for (int c = 0; c < buffers; c++)
		{
			string name = reader.ReadString();
			int sizeInBytes = reader.ReadInt16();
			int[] parameters = new int[reader.ReadByte()];
			int[] offsets = new int[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				parameters[i] = reader.ReadByte();
				offsets[i] = reader.ReadUInt16();
			}
			ConstantBuffer buffer = new ConstantBuffer(base.GraphicsDevice, sizeInBytes, parameters, offsets, name);
			this.ConstantBuffers[c] = buffer;
		}
		int shaders = reader.ReadByte();
		this._shaders = new Shader[shaders];
		for (int s = 0; s < shaders; s++)
		{
			this._shaders[s] = new Shader(base.GraphicsDevice, reader);
		}
		this.Parameters = Effect.ReadParameters(reader);
		int techniqueCount = reader.ReadByte();
		EffectTechnique[] techniques = new EffectTechnique[techniqueCount];
		for (int t = 0; t < techniqueCount; t++)
		{
			string name2 = reader.ReadString();
			EffectAnnotationCollection annotations = Effect.ReadAnnotations(reader);
			EffectPassCollection passes = Effect.ReadPasses(reader, this, this._shaders);
			techniques[t] = new EffectTechnique(this, name2, passes, annotations);
		}
		this.Techniques = new EffectTechniqueCollection(techniques);
		this.CurrentTechnique = this.Techniques[0];
	}

	private static EffectAnnotationCollection ReadAnnotations(BinaryReader reader)
	{
		int count = reader.ReadByte();
		if (count == 0)
		{
			return EffectAnnotationCollection.Empty;
		}
		return new EffectAnnotationCollection(new EffectAnnotation[count]);
	}

	private static EffectPassCollection ReadPasses(BinaryReader reader, Effect effect, Shader[] shaders)
	{
		int count = reader.ReadByte();
		EffectPass[] passes = new EffectPass[count];
		for (int i = 0; i < count; i++)
		{
			string name = reader.ReadString();
			EffectAnnotationCollection annotations = Effect.ReadAnnotations(reader);
			Shader vertexShader = null;
			int shaderIndex = reader.ReadByte();
			if (shaderIndex != 255)
			{
				vertexShader = shaders[shaderIndex];
			}
			Shader pixelShader = null;
			shaderIndex = reader.ReadByte();
			if (shaderIndex != 255)
			{
				pixelShader = shaders[shaderIndex];
			}
			BlendState blend = null;
			DepthStencilState depth = null;
			RasterizerState raster = null;
			if (reader.ReadBoolean())
			{
				blend = new BlendState
				{
					AlphaBlendFunction = (BlendFunction)reader.ReadByte(),
					AlphaDestinationBlend = (Blend)reader.ReadByte(),
					AlphaSourceBlend = (Blend)reader.ReadByte(),
					BlendFactor = new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
					ColorBlendFunction = (BlendFunction)reader.ReadByte(),
					ColorDestinationBlend = (Blend)reader.ReadByte(),
					ColorSourceBlend = (Blend)reader.ReadByte(),
					ColorWriteChannels = (ColorWriteChannels)reader.ReadByte(),
					ColorWriteChannels1 = (ColorWriteChannels)reader.ReadByte(),
					ColorWriteChannels2 = (ColorWriteChannels)reader.ReadByte(),
					ColorWriteChannels3 = (ColorWriteChannels)reader.ReadByte(),
					MultiSampleMask = reader.ReadInt32()
				};
			}
			if (reader.ReadBoolean())
			{
				depth = new DepthStencilState
				{
					CounterClockwiseStencilDepthBufferFail = (StencilOperation)reader.ReadByte(),
					CounterClockwiseStencilFail = (StencilOperation)reader.ReadByte(),
					CounterClockwiseStencilFunction = (CompareFunction)reader.ReadByte(),
					CounterClockwiseStencilPass = (StencilOperation)reader.ReadByte(),
					DepthBufferEnable = reader.ReadBoolean(),
					DepthBufferFunction = (CompareFunction)reader.ReadByte(),
					DepthBufferWriteEnable = reader.ReadBoolean(),
					ReferenceStencil = reader.ReadInt32(),
					StencilDepthBufferFail = (StencilOperation)reader.ReadByte(),
					StencilEnable = reader.ReadBoolean(),
					StencilFail = (StencilOperation)reader.ReadByte(),
					StencilFunction = (CompareFunction)reader.ReadByte(),
					StencilMask = reader.ReadInt32(),
					StencilPass = (StencilOperation)reader.ReadByte(),
					StencilWriteMask = reader.ReadInt32(),
					TwoSidedStencilMode = reader.ReadBoolean()
				};
			}
			if (reader.ReadBoolean())
			{
				raster = new RasterizerState
				{
					CullMode = (CullMode)reader.ReadByte(),
					DepthBias = reader.ReadSingle(),
					FillMode = (FillMode)reader.ReadByte(),
					MultiSampleAntiAlias = reader.ReadBoolean(),
					ScissorTestEnable = reader.ReadBoolean(),
					SlopeScaleDepthBias = reader.ReadSingle()
				};
			}
			passes[i] = new EffectPass(effect, name, vertexShader, pixelShader, blend, depth, raster, annotations);
		}
		return new EffectPassCollection(passes);
	}

	private static EffectParameterCollection ReadParameters(BinaryReaderEx reader)
	{
		int count = reader.Read7BitEncodedInt();
		if (count == 0)
		{
			return EffectParameterCollection.Empty;
		}
		EffectParameter[] parameters = new EffectParameter[count];
		for (int i = 0; i < count; i++)
		{
			EffectParameterClass class_ = (EffectParameterClass)reader.ReadByte();
			EffectParameterType type = (EffectParameterType)reader.ReadByte();
			string name = reader.ReadString();
			string semantic = reader.ReadString();
			EffectAnnotationCollection annotations = Effect.ReadAnnotations(reader);
			int rowCount = reader.ReadByte();
			int columnCount = reader.ReadByte();
			EffectParameterCollection elements = Effect.ReadParameters(reader);
			EffectParameterCollection structMembers = Effect.ReadParameters(reader);
			object data = null;
			if (elements.Count == 0 && structMembers.Count == 0)
			{
				switch (type)
				{
				case EffectParameterType.Bool:
				case EffectParameterType.Int32:
				case EffectParameterType.Single:
				{
					float[] buffer = new float[rowCount * columnCount];
					for (int j = 0; j < buffer.Length; j++)
					{
						buffer[j] = reader.ReadSingle();
					}
					data = buffer;
					break;
				}
				case EffectParameterType.String:
					throw new NotSupportedException();
				}
			}
			parameters[i] = new EffectParameter(class_, type, name, rowCount, columnCount, semantic, annotations, elements, structMembers, data);
		}
		return new EffectParameterCollection(parameters);
	}
}
