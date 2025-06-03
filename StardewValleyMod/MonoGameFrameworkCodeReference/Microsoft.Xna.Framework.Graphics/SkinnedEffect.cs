using System;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Built-in effect for rendering skinned character models.
/// </summary>
public class SkinnedEffect : Effect, IEffectMatrices, IEffectLights, IEffectFog
{
	public const int MaxBones = 72;

	private EffectParameter textureParam;

	private EffectParameter diffuseColorParam;

	private EffectParameter emissiveColorParam;

	private EffectParameter specularColorParam;

	private EffectParameter specularPowerParam;

	private EffectParameter eyePositionParam;

	private EffectParameter fogColorParam;

	private EffectParameter fogVectorParam;

	private EffectParameter worldParam;

	private EffectParameter worldInverseTransposeParam;

	private EffectParameter worldViewProjParam;

	private EffectParameter bonesParam;

	private bool preferPerPixelLighting;

	private bool oneLight;

	private bool fogEnabled;

	private Matrix world = Matrix.Identity;

	private Matrix view = Matrix.Identity;

	private Matrix projection = Matrix.Identity;

	private Matrix worldView;

	private Vector3 diffuseColor = Vector3.One;

	private Vector3 emissiveColor = Vector3.Zero;

	private Vector3 ambientLightColor = Vector3.Zero;

	private float alpha = 1f;

	private DirectionalLight light0;

	private DirectionalLight light1;

	private DirectionalLight light2;

	private float fogStart;

	private float fogEnd = 1f;

	private int weightsPerVertex = 4;

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
			this.dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.World | EffectDirtyFlags.Fog;
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
			this.dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.EyePosition | EffectDirtyFlags.Fog;
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
	/// Gets or sets the material emissive color (range 0 to 1).
	/// </summary>
	public Vector3 EmissiveColor
	{
		get
		{
			return this.emissiveColor;
		}
		set
		{
			this.emissiveColor = value;
			this.dirtyFlags |= EffectDirtyFlags.MaterialColor;
		}
	}

	/// <summary>
	/// Gets or sets the material specular color (range 0 to 1).
	/// </summary>
	public Vector3 SpecularColor
	{
		get
		{
			return this.specularColorParam.GetValueVector3();
		}
		set
		{
			this.specularColorParam.SetValue(value);
		}
	}

	/// <summary>
	/// Gets or sets the material specular power.
	/// </summary>
	public float SpecularPower
	{
		get
		{
			return this.specularPowerParam.GetValueSingle();
		}
		set
		{
			this.specularPowerParam.SetValue(value);
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
	/// Gets or sets the per-pixel lighting prefer flag.
	/// </summary>
	public bool PreferPerPixelLighting
	{
		get
		{
			return this.preferPerPixelLighting;
		}
		set
		{
			if (this.preferPerPixelLighting != value)
			{
				this.preferPerPixelLighting = value;
				this.dirtyFlags |= EffectDirtyFlags.ShaderIndex;
			}
		}
	}

	/// <summary>
	/// Gets or sets the ambient light color (range 0 to 1).
	/// </summary>
	public Vector3 AmbientLightColor
	{
		get
		{
			return this.ambientLightColor;
		}
		set
		{
			this.ambientLightColor = value;
			this.dirtyFlags |= EffectDirtyFlags.MaterialColor;
		}
	}

	/// <summary>
	/// Gets the first directional light.
	/// </summary>
	public DirectionalLight DirectionalLight0 => this.light0;

	/// <summary>
	/// Gets the second directional light.
	/// </summary>
	public DirectionalLight DirectionalLight1 => this.light1;

	/// <summary>
	/// Gets the third directional light.
	/// </summary>
	public DirectionalLight DirectionalLight2 => this.light2;

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
	/// Gets or sets the number of skinning weights to evaluate for each vertex (1, 2, or 4).
	/// </summary>
	public int WeightsPerVertex
	{
		get
		{
			return this.weightsPerVertex;
		}
		set
		{
			if (value != 1 && value != 2 && value != 4)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this.weightsPerVertex = value;
			this.dirtyFlags |= EffectDirtyFlags.ShaderIndex;
		}
	}

	/// <summary>
	/// This effect requires lighting, so we explicitly implement
	/// IEffectLights.LightingEnabled, and do not allow turning it off.
	/// </summary>
	bool IEffectLights.LightingEnabled
	{
		get
		{
			return true;
		}
		set
		{
			if (!value)
			{
				throw new NotSupportedException("SkinnedEffect does not support setting LightingEnabled to false.");
			}
		}
	}

	/// <summary>
	/// Sets an array of skinning bone transform matrices.
	/// </summary>
	public void SetBoneTransforms(Matrix[] boneTransforms)
	{
		if (boneTransforms == null || boneTransforms.Length == 0)
		{
			throw new ArgumentNullException("boneTransforms");
		}
		if (boneTransforms.Length > 72)
		{
			throw new ArgumentException();
		}
		this.bonesParam.SetValue(boneTransforms);
	}

	/// <summary>
	/// Gets a copy of the current skinning bone transform matrices.
	/// </summary>
	public Matrix[] GetBoneTransforms(int count)
	{
		if (count <= 0 || count > 72)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		Matrix[] bones = this.bonesParam.GetValueMatrixArray(count);
		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].M44 = 1f;
		}
		return bones;
	}

	/// <summary>
	/// Creates a new SkinnedEffect with default parameter settings.
	/// </summary>
	public SkinnedEffect(GraphicsDevice device)
		: base(device, EffectResource.SkinnedEffect.Bytecode)
	{
		this.CacheEffectParameters(null);
		this.DirectionalLight0.Enabled = true;
		this.SpecularColor = Vector3.One;
		this.SpecularPower = 16f;
		Matrix[] identityBones = new Matrix[72];
		for (int i = 0; i < 72; i++)
		{
			identityBones[i] = Matrix.Identity;
		}
		this.SetBoneTransforms(identityBones);
	}

	/// <summary>
	/// Creates a new SkinnedEffect by cloning parameter settings from an existing instance.
	/// </summary>
	protected SkinnedEffect(SkinnedEffect cloneSource)
		: base(cloneSource)
	{
		this.CacheEffectParameters(cloneSource);
		this.preferPerPixelLighting = cloneSource.preferPerPixelLighting;
		this.fogEnabled = cloneSource.fogEnabled;
		this.world = cloneSource.world;
		this.view = cloneSource.view;
		this.projection = cloneSource.projection;
		this.diffuseColor = cloneSource.diffuseColor;
		this.emissiveColor = cloneSource.emissiveColor;
		this.ambientLightColor = cloneSource.ambientLightColor;
		this.alpha = cloneSource.alpha;
		this.fogStart = cloneSource.fogStart;
		this.fogEnd = cloneSource.fogEnd;
		this.weightsPerVertex = cloneSource.weightsPerVertex;
	}

	/// <summary>
	/// Creates a clone of the current SkinnedEffect instance.
	/// </summary>
	public override Effect Clone()
	{
		return new SkinnedEffect(this);
	}

	/// <summary>
	/// Sets up the standard key/fill/back lighting rig.
	/// </summary>
	public void EnableDefaultLighting()
	{
		this.AmbientLightColor = EffectHelpers.EnableDefaultLighting(this.light0, this.light1, this.light2);
	}

	/// <summary>
	/// Looks up shortcut references to our effect parameters.
	/// </summary>
	private void CacheEffectParameters(SkinnedEffect cloneSource)
	{
		this.textureParam = base.Parameters["Texture"];
		this.diffuseColorParam = base.Parameters["DiffuseColor"];
		this.emissiveColorParam = base.Parameters["EmissiveColor"];
		this.specularColorParam = base.Parameters["SpecularColor"];
		this.specularPowerParam = base.Parameters["SpecularPower"];
		this.eyePositionParam = base.Parameters["EyePosition"];
		this.fogColorParam = base.Parameters["FogColor"];
		this.fogVectorParam = base.Parameters["FogVector"];
		this.worldParam = base.Parameters["World"];
		this.worldInverseTransposeParam = base.Parameters["WorldInverseTranspose"];
		this.worldViewProjParam = base.Parameters["WorldViewProj"];
		this.bonesParam = base.Parameters["Bones"];
		this.light0 = new DirectionalLight(base.Parameters["DirLight0Direction"], base.Parameters["DirLight0DiffuseColor"], base.Parameters["DirLight0SpecularColor"], cloneSource?.light0);
		this.light1 = new DirectionalLight(base.Parameters["DirLight1Direction"], base.Parameters["DirLight1DiffuseColor"], base.Parameters["DirLight1SpecularColor"], cloneSource?.light1);
		this.light2 = new DirectionalLight(base.Parameters["DirLight2Direction"], base.Parameters["DirLight2DiffuseColor"], base.Parameters["DirLight2SpecularColor"], cloneSource?.light2);
	}

	/// <summary>
	/// Lazily computes derived parameter values immediately before applying the effect.
	/// </summary>
	protected internal override void OnApply()
	{
		this.dirtyFlags = EffectHelpers.SetWorldViewProjAndFog(this.dirtyFlags, ref this.world, ref this.view, ref this.projection, ref this.worldView, this.fogEnabled, this.fogStart, this.fogEnd, this.worldViewProjParam, this.fogVectorParam);
		this.dirtyFlags = EffectHelpers.SetLightingMatrices(this.dirtyFlags, ref this.world, ref this.view, this.worldParam, this.worldInverseTransposeParam, this.eyePositionParam);
		if ((this.dirtyFlags & EffectDirtyFlags.MaterialColor) != 0)
		{
			EffectHelpers.SetMaterialColor(lightingEnabled: true, this.alpha, ref this.diffuseColor, ref this.emissiveColor, ref this.ambientLightColor, this.diffuseColorParam, this.emissiveColorParam);
			this.dirtyFlags &= ~EffectDirtyFlags.MaterialColor;
		}
		bool newOneLight = !this.light1.Enabled && !this.light2.Enabled;
		if (this.oneLight != newOneLight)
		{
			this.oneLight = newOneLight;
			this.dirtyFlags |= EffectDirtyFlags.ShaderIndex;
		}
		if ((this.dirtyFlags & EffectDirtyFlags.ShaderIndex) != 0)
		{
			int shaderIndex = 0;
			if (!this.fogEnabled)
			{
				shaderIndex++;
			}
			if (this.weightsPerVertex == 2)
			{
				shaderIndex += 2;
			}
			else if (this.weightsPerVertex == 4)
			{
				shaderIndex += 4;
			}
			if (this.preferPerPixelLighting)
			{
				shaderIndex += 12;
			}
			else if (this.oneLight)
			{
				shaderIndex += 6;
			}
			this.dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;
			base.CurrentTechnique = base.Techniques[shaderIndex];
		}
	}
}
