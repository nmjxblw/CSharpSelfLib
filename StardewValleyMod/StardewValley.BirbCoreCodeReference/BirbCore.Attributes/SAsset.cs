using System;
using System.Reflection;
using BirbCore.Extensions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace BirbCore.Attributes;

public class SAsset : ClassHandler
{
	public class Asset : FieldHandler
	{
		private readonly string _path;

		public Asset(string path, AssetLoadPriority priority = (AssetLoadPriority)0)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003Cpriority_003EP = priority;
			_path = PathUtilities.NormalizePath(path);
			base._002Ector();
		}

		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			IAssetName assetName = mod.Helper.ModContent.GetInternalAssetName(_path);
			Action<object, object> assetNameSetter2;
			if (instance == null)
			{
				if (fieldType.DeclaringType != null && fieldType.DeclaringType.TryGetSetterOfName(name + "AssetName", out Action<object, object> assetNameSetter))
				{
					assetNameSetter(instance, assetName);
				}
			}
			else if (instance.GetType().TryGetSetterOfName(name + "AssetName", out assetNameSetter2))
			{
				assetNameSetter2(instance, assetName);
			}
			mod.Helper.Events.Content.AssetRequested += delegate(object? sender, AssetRequestedEventArgs e)
			{
				//IL_005c: Unknown result type (might be due to invalid IL or missing references)
				if (e.Name.IsEquivalentTo(assetName, false))
				{
					((object)e).GetType().GetMethod("LoadFromModFile")?.MakeGenericMethod(fieldType).Invoke(e, new object[2] { _path, _003Cpriority_003EP });
					setter(instance, LoadValue(fieldType, _path, mod));
				}
			};
			mod.Helper.Events.Content.AssetReady += delegate(object? sender, AssetReadyEventArgs e)
			{
				if (e.Name.IsEquivalentTo(assetName, false))
				{
					setter(instance, LoadValue(fieldType, _path, mod));
				}
			};
			mod.Helper.Events.Content.AssetsInvalidated += delegate(object? sender, AssetsInvalidatedEventArgs e)
			{
				foreach (IAssetName current in e.Names)
				{
					if (current.IsEquivalentTo(assetName, false))
					{
						setter(instance, LoadValue(fieldType, _path, mod));
					}
				}
			};
			setter(instance, LoadValue(fieldType, _path, mod));
		}

		private static object? LoadValue(Type fieldType, string assetPath, IMod mod)
		{
			return ((object)mod.Helper.ModContent).GetType().GetMethod("Load", new Type[1] { typeof(string) })?.MakeGenericMethod(fieldType).Invoke(mod.Helper.ModContent, new object[1] { assetPath });
		}
	}

	private MemberInfo? _modAssets;

	public override void Handle(Type type, object? instance, IMod mod, object[]? args = null)
	{
		if (!((object)mod).GetType().TryGetMemberOfType(type, out MemberInfo memberInfo))
		{
			Log.Error("Mod must define an asset property");
			return;
		}
		_modAssets = memberInfo;
		Action<object, object> setter = _modAssets.GetSetter();
		setter(mod, instance);
		base.Handle(type, instance, mod, args);
	}
}
