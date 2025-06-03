namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Built-in effect that supports two-layer multitexturing.
/// </summary>
public class DualTextureEffect : Effect, IEffectMatrices, IEffectFog
{
	private EffectParameter textureParam;

	private EffectParameter texture2Param;

	private EffectParameter diffuseColorParam;

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
	/// Gets or sets the current base texture.
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
	/// Gets or sets the current overlay texture.
	/// </summary>
	public Texture2D Texture2
	{
		get
		{
			return this.texture2Param.GetValueTexture2D();
		}
		set
		{
			this.texture2Param.SetValue(value);
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
	/// Creates a new DualTextureEffect with default parameter settings.
	/// </summary>
	public DualTextureEffect(GraphicsDevice device)
		: base(device, EffectResource.DualTextureEffect.Bytecode)
	{
		this.CacheEffectParameters();
	}

	/// <summary>
	/// Creates a new DualTextureEffect by cloning parameter settings from an existing instance.
	/// </summary>
	protected DualTextureEffect(DualTextureEffect cloneSource)
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
	}

	/// <summary>
	/// Creates a clone of the current DualTextureEffect instance.
	/// </summary>
	public override Effect Clone()
	{
		return new DualTextureEffect(this);
	}

	/// <summary>
	/// Looks up shortcut references to our effect parameters.
	/// </summary>
	private void CacheEffectParameters()
	{
		this.textureParam = base.Parameters["Texture"];
		this.texture2Param = base.Parameters["Texture2"];
		this.diffuseColorParam = base.Parameters["DiffuseColor"];
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
			this.dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;
			base.CurrentTechnique = base.Techniques[shaderIndex];
		}
	}
}
