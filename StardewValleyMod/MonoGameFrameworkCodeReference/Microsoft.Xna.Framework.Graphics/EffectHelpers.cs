namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Helper code shared between the various built-in effects.
/// </summary>
internal static class EffectHelpers
{
	/// <summary>
	/// Sets up the standard key/fill/back lighting rig.
	/// </summary>
	internal static Vector3 EnableDefaultLighting(DirectionalLight light0, DirectionalLight light1, DirectionalLight light2)
	{
		light0.Direction = new Vector3(-0.5265408f, -0.5735765f, -0.6275069f);
		light0.DiffuseColor = new Vector3(1f, 0.9607844f, 0.8078432f);
		light0.SpecularColor = new Vector3(1f, 0.9607844f, 0.8078432f);
		light0.Enabled = true;
		light1.Direction = new Vector3(0.7198464f, 0.3420201f, 0.6040227f);
		light1.DiffuseColor = new Vector3(82f / 85f, 0.7607844f, 0.4078432f);
		light1.SpecularColor = Vector3.Zero;
		light1.Enabled = true;
		light2.Direction = new Vector3(0.4545195f, -0.7660444f, 0.4545195f);
		light2.DiffuseColor = new Vector3(0.3231373f, 0.3607844f, 0.3937255f);
		light2.SpecularColor = new Vector3(0.3231373f, 0.3607844f, 0.3937255f);
		light2.Enabled = true;
		return new Vector3(0.05333332f, 0.09882354f, 0.1819608f);
	}

	/// <summary>
	/// Lazily recomputes the world+view+projection matrix and
	/// fog vector based on the current effect parameter settings.
	/// </summary>
	internal static EffectDirtyFlags SetWorldViewProjAndFog(EffectDirtyFlags dirtyFlags, ref Matrix world, ref Matrix view, ref Matrix projection, ref Matrix worldView, bool fogEnabled, float fogStart, float fogEnd, EffectParameter worldViewProjParam, EffectParameter fogVectorParam)
	{
		if ((dirtyFlags & EffectDirtyFlags.WorldViewProj) != 0)
		{
			Matrix.Multiply(ref world, ref view, out worldView);
			Matrix.Multiply(ref worldView, ref projection, out var worldViewProj);
			worldViewProjParam.SetValue(worldViewProj);
			dirtyFlags &= ~EffectDirtyFlags.WorldViewProj;
		}
		if (fogEnabled)
		{
			if ((dirtyFlags & (EffectDirtyFlags.Fog | EffectDirtyFlags.FogEnable)) != 0)
			{
				EffectHelpers.SetFogVector(ref worldView, fogStart, fogEnd, fogVectorParam);
				dirtyFlags &= ~(EffectDirtyFlags.Fog | EffectDirtyFlags.FogEnable);
			}
		}
		else if ((dirtyFlags & EffectDirtyFlags.FogEnable) != 0)
		{
			fogVectorParam.SetValue(Vector4.Zero);
			dirtyFlags &= ~EffectDirtyFlags.FogEnable;
		}
		return dirtyFlags;
	}

	/// <summary>
	/// Sets a vector which can be dotted with the object space vertex position to compute fog amount.
	/// </summary>
	private static void SetFogVector(ref Matrix worldView, float fogStart, float fogEnd, EffectParameter fogVectorParam)
	{
		if (fogStart == fogEnd)
		{
			fogVectorParam.SetValue(new Vector4(0f, 0f, 0f, 1f));
			return;
		}
		float scale = 1f / (fogStart - fogEnd);
		fogVectorParam.SetValue(new Vector4
		{
			X = worldView.M13 * scale,
			Y = worldView.M23 * scale,
			Z = worldView.M33 * scale,
			W = (worldView.M43 + fogStart) * scale
		});
	}

	/// <summary>
	/// Lazily recomputes the world inverse transpose matrix and
	/// eye position based on the current effect parameter settings.
	/// </summary>
	internal static EffectDirtyFlags SetLightingMatrices(EffectDirtyFlags dirtyFlags, ref Matrix world, ref Matrix view, EffectParameter worldParam, EffectParameter worldInverseTransposeParam, EffectParameter eyePositionParam)
	{
		if ((dirtyFlags & EffectDirtyFlags.World) != 0)
		{
			Matrix.Invert(ref world, out var worldTranspose);
			Matrix.Transpose(ref worldTranspose, out var worldInverseTranspose);
			worldParam.SetValue(world);
			worldInverseTransposeParam.SetValue(worldInverseTranspose);
			dirtyFlags &= ~EffectDirtyFlags.World;
		}
		if ((dirtyFlags & EffectDirtyFlags.EyePosition) != 0)
		{
			Matrix.Invert(ref view, out var viewInverse);
			eyePositionParam.SetValue(viewInverse.Translation);
			dirtyFlags &= ~EffectDirtyFlags.EyePosition;
		}
		return dirtyFlags;
	}

	/// <summary>
	/// Sets the diffuse/emissive/alpha material color parameters.
	/// </summary>
	internal static void SetMaterialColor(bool lightingEnabled, float alpha, ref Vector3 diffuseColor, ref Vector3 emissiveColor, ref Vector3 ambientLightColor, EffectParameter diffuseColorParam, EffectParameter emissiveColorParam)
	{
		if (lightingEnabled)
		{
			Vector4 diffuse = default(Vector4);
			Vector3 emissive = default(Vector3);
			diffuse.X = diffuseColor.X * alpha;
			diffuse.Y = diffuseColor.Y * alpha;
			diffuse.Z = diffuseColor.Z * alpha;
			diffuse.W = alpha;
			emissive.X = (emissiveColor.X + ambientLightColor.X * diffuseColor.X) * alpha;
			emissive.Y = (emissiveColor.Y + ambientLightColor.Y * diffuseColor.Y) * alpha;
			emissive.Z = (emissiveColor.Z + ambientLightColor.Z * diffuseColor.Z) * alpha;
			diffuseColorParam.SetValue(diffuse);
			emissiveColorParam.SetValue(emissive);
		}
		else
		{
			diffuseColorParam.SetValue(new Vector4
			{
				X = (diffuseColor.X + emissiveColor.X) * alpha,
				Y = (diffuseColor.Y + emissiveColor.Y) * alpha,
				Z = (diffuseColor.Z + emissiveColor.Z) * alpha,
				W = alpha
			});
		}
	}
}
