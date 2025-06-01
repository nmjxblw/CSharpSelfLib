using System;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Built-in effect that supports environment mapping.
/// </summary>
public class EnvironmentMapEffect : Effect, IEffectMatrices, IEffectLights, IEffectFog
{
	private EffectParameter textureParam;

	private EffectParameter environmentMapParam;

	private EffectParameter environmentMapAmountParam;

	private EffectParameter environmentMapSpecularParam;

	private EffectParameter fresnelFactorParam;

	private EffectParameter diffuseColorParam;

	private EffectParameter emissiveColorParam;

	private EffectParameter eyePositionParam;

	private EffectParameter fogColorParam;

	private EffectParameter fogVectorParam;

	private EffectParameter worldParam;

	private EffectParameter worldInverseTransposeParam;

	private EffectParameter worldViewProjParam;

	private bool oneLight;

	private bool fogEnabled;

	private bool fresnelEnabled;

	private bool specularEnabled;

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
	/// Gets or sets the current environment map texture.
	/// </summary>
	public TextureCube EnvironmentMap
	{
		get
		{
			return this.environmentMapParam.GetValueTextureCube();
		}
		set
		{
			this.environmentMapParam.SetValue(value);
		}
	}

	/// <summary>
	/// Gets or sets the amount of the environment map RGB that will be blended over 
	/// the base texture. Range 0 to 1, default 1. If set to zero, the RGB channels 
	/// of the environment map will completely ignored (but the environment map alpha 
	/// may still be visible if EnvironmentMapSpecular is greater than zero).
	/// </summary>
	public float EnvironmentMapAmount
	{
		get
		{
			return this.environmentMapAmountParam.GetValueSingle();
		}
		set
		{
			this.environmentMapAmountParam.SetValue(value);
		}
	}

	/// <summary>
	/// Gets or sets the amount of the environment map alpha channel that will 
	/// be added to the base texture. Range 0 to 1, default 0. This can be used 
	/// to implement cheap specular lighting, by encoding one or more specular 
	/// highlight patterns into the environment map alpha channel, then setting 
	/// EnvironmentMapSpecular to the desired specular light color.
	/// </summary>
	public Vector3 EnvironmentMapSpecular
	{
		get
		{
			return this.environmentMapSpecularParam.GetValueVector3();
		}
		set
		{
			this.environmentMapSpecularParam.SetValue(value);
			bool enabled = value != Vector3.Zero;
			if (this.specularEnabled != enabled)
			{
				this.specularEnabled = enabled;
				this.dirtyFlags |= EffectDirtyFlags.ShaderIndex;
			}
		}
	}

	/// <summary>
	/// Gets or sets the Fresnel factor used for the environment map blending. 
	/// Higher values make the environment map only visible around the silhouette 
	/// edges of the object, while lower values make it visible everywhere. 
	/// Setting this property to 0 disables Fresnel entirely, making the 
	/// environment map equally visible regardless of view angle. The default is 
	/// 1. Fresnel only affects the environment map RGB (the intensity of which is 
	/// controlled by EnvironmentMapAmount). The alpha contribution (controlled by 
	/// EnvironmentMapSpecular) is not affected by the Fresnel setting.
	/// </summary>
	public float FresnelFactor
	{
		get
		{
			return this.fresnelFactorParam.GetValueSingle();
		}
		set
		{
			this.fresnelFactorParam.SetValue(value);
			bool enabled = value != 0f;
			if (this.fresnelEnabled != enabled)
			{
				this.fresnelEnabled = enabled;
				this.dirtyFlags |= EffectDirtyFlags.ShaderIndex;
			}
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
				throw new NotSupportedException("EnvironmentMapEffect does not support setting LightingEnabled to false.");
			}
		}
	}

	/// <summary>
	/// Creates a new EnvironmentMapEffect with default parameter settings.
	/// </summary>
	public EnvironmentMapEffect(GraphicsDevice device)
		: base(device, EffectResource.EnvironmentMapEffect.Bytecode)
	{
		this.CacheEffectParameters(null);
		this.DirectionalLight0.Enabled = true;
		this.EnvironmentMapAmount = 1f;
		this.EnvironmentMapSpecular = Vector3.Zero;
		this.FresnelFactor = 1f;
	}

	/// <summary>
	/// Creates a new EnvironmentMapEffect by cloning parameter settings from an existing instance.
	/// </summary>
	protected EnvironmentMapEffect(EnvironmentMapEffect cloneSource)
		: base(cloneSource)
	{
		this.CacheEffectParameters(cloneSource);
		this.fogEnabled = cloneSource.fogEnabled;
		this.fresnelEnabled = cloneSource.fresnelEnabled;
		this.specularEnabled = cloneSource.specularEnabled;
		this.world = cloneSource.world;
		this.view = cloneSource.view;
		this.projection = cloneSource.projection;
		this.diffuseColor = cloneSource.diffuseColor;
		this.emissiveColor = cloneSource.emissiveColor;
		this.ambientLightColor = cloneSource.ambientLightColor;
		this.alpha = cloneSource.alpha;
		this.fogStart = cloneSource.fogStart;
		this.fogEnd = cloneSource.fogEnd;
	}

	/// <summary>
	/// Creates a clone of the current EnvironmentMapEffect instance.
	/// </summary>
	public override Effect Clone()
	{
		return new EnvironmentMapEffect(this);
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
	private void CacheEffectParameters(EnvironmentMapEffect cloneSource)
	{
		this.textureParam = base.Parameters["Texture"];
		this.environmentMapParam = base.Parameters["EnvironmentMap"];
		this.environmentMapAmountParam = base.Parameters["EnvironmentMapAmount"];
		this.environmentMapSpecularParam = base.Parameters["EnvironmentMapSpecular"];
		this.fresnelFactorParam = base.Parameters["FresnelFactor"];
		this.diffuseColorParam = base.Parameters["DiffuseColor"];
		this.emissiveColorParam = base.Parameters["EmissiveColor"];
		this.eyePositionParam = base.Parameters["EyePosition"];
		this.fogColorParam = base.Parameters["FogColor"];
		this.fogVectorParam = base.Parameters["FogVector"];
		this.worldParam = base.Parameters["World"];
		this.worldInverseTransposeParam = base.Parameters["WorldInverseTranspose"];
		this.worldViewProjParam = base.Parameters["WorldViewProj"];
		this.light0 = new DirectionalLight(base.Parameters["DirLight0Direction"], base.Parameters["DirLight0DiffuseColor"], null, cloneSource?.light0);
		this.light1 = new DirectionalLight(base.Parameters["DirLight1Direction"], base.Parameters["DirLight1DiffuseColor"], null, cloneSource?.light1);
		this.light2 = new DirectionalLight(base.Parameters["DirLight2Direction"], base.Parameters["DirLight2DiffuseColor"], null, cloneSource?.light2);
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
			if (this.fresnelEnabled)
			{
				shaderIndex += 2;
			}
			if (this.specularEnabled)
			{
				shaderIndex += 4;
			}
			if (this.oneLight)
			{
				shaderIndex += 8;
			}
			this.dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;
			base.CurrentTechnique = base.Techniques[shaderIndex];
		}
	}
}
