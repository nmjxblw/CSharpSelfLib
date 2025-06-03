using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content;

internal class EffectMaterialReader : ContentTypeReader<EffectMaterial>
{
	protected internal override EffectMaterial Read(ContentReader input, EffectMaterial existingInstance)
	{
		EffectMaterial effectMaterial = new EffectMaterial(input.ReadExternalReference<Effect>());
		foreach (KeyValuePair<string, object> item in input.ReadObject<Dictionary<string, object>>())
		{
			EffectParameter parameter = effectMaterial.Parameters[item.Key];
			if (parameter == null)
			{
				continue;
			}
			Type itemType = item.Value.GetType();
			if (ReflectionHelpers.IsAssignableFromType(typeof(Texture), itemType))
			{
				parameter.SetValue((Texture)item.Value);
				continue;
			}
			if (ReflectionHelpers.IsAssignableFromType(typeof(int), itemType))
			{
				parameter.SetValue((int)item.Value);
				continue;
			}
			if (ReflectionHelpers.IsAssignableFromType(typeof(int[]), itemType))
			{
				parameter.SetValue((int[])item.Value);
				continue;
			}
			if (ReflectionHelpers.IsAssignableFromType(typeof(bool), itemType))
			{
				parameter.SetValue((bool)item.Value);
				continue;
			}
			if (ReflectionHelpers.IsAssignableFromType(typeof(float), itemType))
			{
				parameter.SetValue((float)item.Value);
				continue;
			}
			if (ReflectionHelpers.IsAssignableFromType(typeof(float[]), itemType))
			{
				parameter.SetValue((float[])item.Value);
				continue;
			}
			if (ReflectionHelpers.IsAssignableFromType(typeof(Vector2), itemType))
			{
				parameter.SetValue((Vector2)item.Value);
				continue;
			}
			if (ReflectionHelpers.IsAssignableFromType(typeof(Vector2[]), itemType))
			{
				parameter.SetValue((Vector2[])item.Value);
				continue;
			}
			if (ReflectionHelpers.IsAssignableFromType(typeof(Vector3), itemType))
			{
				parameter.SetValue((Vector3)item.Value);
				continue;
			}
			if (ReflectionHelpers.IsAssignableFromType(typeof(Vector3[]), itemType))
			{
				parameter.SetValue((Vector3[])item.Value);
				continue;
			}
			if (ReflectionHelpers.IsAssignableFromType(typeof(Vector4), itemType))
			{
				parameter.SetValue((Vector4)item.Value);
				continue;
			}
			if (ReflectionHelpers.IsAssignableFromType(typeof(Vector4[]), itemType))
			{
				parameter.SetValue((Vector4[])item.Value);
				continue;
			}
			if (ReflectionHelpers.IsAssignableFromType(typeof(Matrix), itemType))
			{
				parameter.SetValue((Matrix)item.Value);
				continue;
			}
			if (ReflectionHelpers.IsAssignableFromType(typeof(Matrix[]), itemType))
			{
				parameter.SetValue((Matrix[])item.Value);
				continue;
			}
			if (ReflectionHelpers.IsAssignableFromType(typeof(Quaternion), itemType))
			{
				parameter.SetValue((Quaternion)item.Value);
				continue;
			}
			throw new NotSupportedException("Parameter type is not supported");
		}
		return effectMaterial;
	}
}
