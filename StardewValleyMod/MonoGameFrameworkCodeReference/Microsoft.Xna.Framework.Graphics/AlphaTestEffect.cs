namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Built-in effect that supports alpha testing.
/// </summary>
public class AlphaTestEffect : Effect, IEffectMatrices, IEffectFog
{
	private EffectParameter textureParam;

	private EffectParameter diffuseColorParam;

	private EffectParameter alphaTestParam;

	private EffectParameter fogColorParam;

	private EffectParameter fogVectorParam;

	private EffectParameter worldViewProjParam;

	private bool fogEnabled;

	private bool vertexColorEnabled;

	private Matrix world = Matrix.Identity;

	private Matrix view = Matrix.Identity;

	private Matrix projection = Matrix.Identity;

	private Matrix worldView;

	private Vector3 diffuseColor = Vector3.One;

	private float alpha = 1f;

	private float fogStart;

	private float fogEnd = 1f;

	private CompareFunction alphaFunction = CompareFunction.Greater;

	private int referenceAlpha;

	private bool isEqNe;

	private EffectDirtyFlags dirtyFlags = EffectDirtyFlags.All;

	/// <summary>
	/// Gets or sets the world matrix.
	/// </summary>
	public Matrix World
	{
		get
		{
			return this.world;
		}
		set
		{
			this.world = value;
			this.dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
		}
	}

	/// <summary>
	/// Gets or sets the view matrix.
	/// </summary>
	public Matrix View
	{
		get
		{
			return this.view;
		}
		set
		{
			this.view = value;
			this.dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
		}
	}

	/// <summary>
	/// Gets or sets the projection matrix.
	/// </summary>
	public Matrix Projection
	{
		get
		{
			return this.projection;
		}
		set
		{
			this.projection = value;
			this.dirtyFlags |= EffectDirtyFlags.WorldViewProj;
		}
	}

	/// <summary>
	/// Gets or sets the material diffuse color (range 0 to 1).
	/// </summary>
	public Vector3 DiffuseColor
	{
		get
		{
			return this.diffuseColor;
		}
		set
		{
			this.diffuseColor = value;
			this.dirtyFlags |= EffectDirtyFlags.MaterialColor;
		}
	}

	/// <summary>
	/// Gets or sets the material alpha.
	/// </summary>
	public float Alpha
	{
		get
		{
			return this.alpha;
		}
		set
		{
			this.alpha = value;
			this.dirtyFlags |= EffectDirtyFlags.MaterialColor;
		}
	}

	/// <summary>
	/// Gets or sets the fog enable flag.
	/// </summary>
	public bool FogEnabled
	{
		get
		{
			return this.fogEnabled;
		}
		set
		{
			if (this.fogEnabled != value)
			{
				this.fogEnabled = value;
				this.dirtyFlags |= EffectDirtyFlags.FogEnable | EffectDirtyFlags.ShaderIndex;
			}
		}
	}

	/// <summary>
	/// Gets or sets the fog start distance.
	/// </summary>
	public float FogStart
	{
		get
		{
			return this.fogStart;
		}
		set
		{
			this.fogStart = value;
			this.dirtyFlags |= EffectDirtyFlags.Fog;
		}
	}

	/// <summary>
	/// Gets or sets the fog end distance.
	/// </summary>
	public float FogEnd
	{
		get
		{
			return this.fogEnd;
		}
		set
		{
			this.fogEnd = value;
			this.dirtyFlags |= EffectDirtyFlags.Fog;
		}
	}

	/// <summary>
	/// Gets or sets the fog color.
	/// </summary>
	public Vector3 FogColor
	{
		get
		{
			return this.fogColorParam.GetValueVector3();
		}
		set
		{
			this.fogColorParam.SetValue(value);
		}
	}

	/// <summary>
	/// Gets or sets the current texture.
	/// </summary>
	public Texture2D Texture
	{
		get
		{
			return this.textureParam.GetValueTexture2D();
		}
		set
		{
			this.textureParam.SetValue(value);
		}
	}

	/// <summary>
	/// Gets or sets whether vertex color is enabled.
	/// </summary>
	public bool VertexColorEnabled
	{
		get
		{
			return this.vertexColorEnabled;
		}
		set
		{
			if (this.vertexColorEnabled != value)
			{
				this.vertexColorEnabled = value;
				this.dirtyFlags |= EffectDirtyFlags.ShaderIndex;
			}
		}
	}

	/// <summary>
	/// Gets or sets the alpha compare function (default Greater).
	/// </summary>
	public CompareFunction AlphaFunction
	{
		get
		{
			return this.alphaFunction;
		}
		set
		{
			this.alphaFunction = value;
			this.dirtyFlags |= EffectDirtyFlags.AlphaTest;
		}
	}

	/// <summary>
	/// Gets or sets the reference alpha value (default 0).
	/// </summary>
	public int ReferenceAlpha
	{
		get
		{
			return this.referenceAlpha;
		}
		set
		{
			this.referenceAlpha = value;
			this.dirtyFlags |= EffectDirtyFlags.AlphaTest;
		}
	}

	/// <summary>
	/// Creates a new AlphaTestEffect with default parameter settings.
	/// </summary>
	public AlphaTestEffect(GraphicsDevice device)
		: base(device, EffectResource.AlphaTestEffect.Bytecode)
	{
		this.CacheEffectParameters();
	}

	/// <summary>
	/// Creates a new AlphaTestEffect by cloning parameter settings from an existing instance.
	/// </summary>
	protected AlphaTestEffect(AlphaTestEffect cloneSource)
		: base(cloneSource)
	{
		this.CacheEffectParameters();
		this.fogEnabled = cloneSource.fogEnabled;
		this.vertexColorEnabled = cloneSource.vertexColorEnabled;
		this.world = cloneSource.world;
		this.view = cloneSource.view;
		this.projection = cloneSource.projection;
		this.diffuseColor = cloneSource.diffuseColor;
		this.alpha = cloneSource.alpha;
		this.fogStart = cloneSource.fogStart;
		this.fogEnd = cloneSource.fogEnd;
		this.alphaFunction = cloneSource.alphaFunction;
		this.referenceAlpha = cloneSource.referenceAlpha;
	}

	/// <summary>
	/// Creates a clone of the current AlphaTestEffect instance.
	/// </summary>
	public override Effect Clone()
	{
		return new AlphaTestEffect(this);
	}

	/// <summary>
	/// Looks up shortcut references to our effect parameters.
	/// </summary>
	private void CacheEffectParameters()
	{
		this.textureParam = base.Parameters["Texture"];
		this.diffuseColorParam = base.Parameters["DiffuseColor"];
		this.alphaTestParam = base.Parameters["AlphaTest"];
		this.fogColorParam = base.Parameters["FogColor"];
		this.fogVectorParam = base.Parameters["FogVector"];
		this.worldViewProjParam = base.Parameters["WorldViewProj"];
	}

	/// <summary>
	/// Lazily computes derived parameter values immediately before applying the effect.
	/// </summary>
	protected internal override void OnApply()
	{
		this.dirtyFlags = EffectHelpers.SetWorldViewProjAndFog(this.dirtyFlags, ref this.world, ref this.view, ref this.projection, ref this.worldView, this.fogEnabled, this.fogStart, this.fogEnd, this.worldViewProjParam, this.fogVectorParam);
		if ((this.dirtyFlags & EffectDirtyFlags.MaterialColor) != 0)
		{
			this.diffuseColorParam.SetValue(new Vector4(this.diffuseColor * this.alpha, this.alpha));
			this.dirtyFlags &= ~EffectDirtyFlags.MaterialColor;
		}
		if ((this.dirtyFlags & EffectDirtyFlags.AlphaTest) != 0)
		{
			Vector4 alphaTest = default(Vector4);
			bool eqNe = false;
			float reference = (float)this.referenceAlpha / 255f;
			switch (this.alphaFunction)
			{
			case CompareFunction.Less:
				alphaTest.X = reference - 0.0019607844f;
				alphaTest.Z = 1f;
				alphaTest.W = -1f;
				break;
			case CompareFunction.LessEqual:
				alphaTest.X = reference + 0.0019607844f;
				alphaTest.Z = 1f;
				alphaTest.W = -1f;
				break;
			case CompareFunction.GreaterEqual:
				alphaTest.X = reference - 0.0019607844f;
				alphaTest.Z = -1f;
				alphaTest.W = 1f;
				break;
			case CompareFunction.Greater:
				alphaTest.X = reference + 0.0019607844f;
				alphaTest.Z = -1f;
				alphaTest.W = 1f;
				break;
			case CompareFunction.Equal:
				alphaTest.X = reference;
				alphaTest.Y = 0.0019607844f;
				alphaTest.Z = 1f;
				alphaTest.W = -1f;
				eqNe = true;
				break;
			case CompareFunction.NotEqual:
				alphaTest.X = reference;
				alphaTest.Y = 0.0019607844f;
				alphaTest.Z = -1f;
				alphaTest.W = 1f;
				eqNe = true;
				break;
			case CompareFunction.Never:
				alphaTest.Z = -1f;
				alphaTest.W = -1f;
				break;
			default:
				alphaTest.Z = 1f;
				alphaTest.W = 1f;
				break;
			}
			this.alphaTestParam.SetValue(alphaTest);
			this.dirtyFlags &= ~EffectDirtyFlags.AlphaTest;
			if (this.isEqNe != eqNe)
			{
				this.isEqNe = eqNe;
				this.dirtyFlags |= EffectDirtyFlags.ShaderIndex;
			}
		}
		if ((this.dirtyFlags & EffectDirtyFlags.ShaderIndex) != 0)
		{
			int shaderIndex = 0;
			if (!this.fogEnabled)
			{
				shaderIndex++;
			}
			if (this.vertexColorEnabled)
			{
				shaderIndex += 2;
			}
			if (this.isEqNe)
			{
				shaderIndex += 4;
			}
			this.dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;
			base.CurrentTechnique = base.Techniques[shaderIndex];
		}
	}
}
