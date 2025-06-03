using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BirbCore.Extensions;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using Sickhead.Engine.Util;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using xTile;

namespace BirbCore.Attributes;

public class SEdit : ClassHandler
{
	public enum Frequency
	{
		Never,
		OnDayStart,
		OnLocationChange,
		OnTimeChange,
		OnTick
	}

	public abstract class BaseEdit : FieldHandler
	{
		private readonly string _target;

		private readonly string? _condition;

		private readonly AssetEditPriority _priority;

		protected IMod? Mod;

		private bool _isApplied;

		protected BaseEdit(string target, string? condition = null, Frequency frequency = Frequency.Never, AssetEditPriority priority = (AssetEditPriority)0)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			_003Cfrequency_003EP = frequency;
			_target = target;
			_condition = condition;
			_priority = priority;
			base._002Ector();
		}

		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			if (GameStateQuery.IsImmutablyFalse(_condition))
			{
				Log.Error($"Condition {_condition} will never be true, so edit {name} will never be applied.");
				return;
			}
			Mod = mod;
			switch (_003Cfrequency_003EP)
			{
			case Frequency.OnDayStart:
				Mod.Helper.Events.GameLoop.DayStarted += InvalidateIfNeeded;
				break;
			case Frequency.OnLocationChange:
				Mod.Helper.Events.Player.Warped += InvalidateIfNeeded;
				break;
			case Frequency.OnTimeChange:
				Mod.Helper.Events.GameLoop.TimeChanged += InvalidateIfNeeded;
				break;
			case Frequency.OnTick:
				Mod.Helper.Events.GameLoop.UpdateTicked += InvalidateIfNeeded;
				break;
			}
			BaseEdit edit = this;
			Mod.Helper.Events.Content.AssetRequested += delegate(object? sender, AssetRequestedEventArgs e)
			{
				//IL_0059: Unknown result type (might be due to invalid IL or missing references)
				if (e.Name.IsEquivalentTo(edit._target, false) && GameStateQuery.CheckConditions(edit._condition, (GameLocation)null, (Farmer)null, (Item)null, (Item)null, (Random)null, (HashSet<string>)null))
				{
					e.Edit((Action<IAssetData>)delegate(IAssetData asset)
					{
						edit.DoEdit(asset, getter(instance), name, fieldType, instance);
					}, edit._priority, (string)null);
				}
			};
		}

		protected abstract void DoEdit(IAssetData asset, object? edit, string name, Type fieldType, object? instance);

		private void InvalidateIfNeeded(object? sender, object e)
		{
			if (Mod != null && _isApplied != GameStateQuery.CheckConditions(_condition, (GameLocation)null, (Farmer)null, (Item)null, (Item)null, (Random)null, (HashSet<string>)null))
			{
				_isApplied = !_isApplied;
				Mod.Helper.GameContent.InvalidateCache(_target);
			}
		}
	}

	public class Data : BaseEdit
	{
		public Data(string target, string[]? field = null, string? condition = null, Frequency frequency = Frequency.Never, AssetEditPriority priority = (AssetEditPriority)0)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			_003Cfield_003EP = field;
			base._002Ector(target, condition, frequency, priority);
		}

		protected override void DoEdit(IAssetData asset, object? edit, string name, Type fieldType, object? instance)
		{
			if (Mod == null)
			{
				return;
			}
			List<object> toEdit = new List<object>(1) { ((IAssetData<object>)(object)asset).Data };
			string[] array = _003Cfield_003EP;
			if (array != null && array.Length >= 1)
			{
				string[] array2 = _003Cfield_003EP;
				foreach (string t in array2)
				{
					List<object> nextToEdit = new List<object>();
					foreach (object toEditValue in toEdit)
					{
						if (!(toEditValue is IList toEditValueList))
						{
							if (toEditValue is IDictionary toEditValueDictionary)
							{
								nextToEdit.AddRange(GetDictionaryEdits(t, toEditValueDictionary));
							}
							else
							{
								nextToEdit.AddRange(GetMemberEdits(t, toEditValue));
							}
						}
						else
						{
							nextToEdit.AddRange(GetListEdits(t, toEditValueList));
						}
					}
					toEdit = nextToEdit;
				}
			}
			foreach (object toEditValue2 in toEdit)
			{
				if (!(toEditValue2 is IList toEditValueList2))
				{
					if (toEditValue2 is IDictionary toEditValueDictionary2)
					{
						ApplyDictionaryEdit(toEditValueDictionary2, edit, name);
					}
					else
					{
						ApplyMemberEdit(toEditValue2, edit);
					}
				}
				else
				{
					ApplyListEdit(toEditValueList2, edit);
				}
			}
		}

		private static IEnumerable<object> GetListEdits(string field, IList toEdit)
		{
			List<object> nextToEdit = new List<object>();
			if (toEdit.Count <= 0)
			{
				return nextToEdit;
			}
			if (field == "*")
			{
				foreach (object item in toEdit)
				{
					nextToEdit.Add(item);
				}
				return nextToEdit;
			}
			if (field.StartsWith("#"))
			{
				if (!int.TryParse(field.Substring(1, field.Length - 1), out var index))
				{
					Log.Error("SEdit.Data could not parse field " + field + " because it expected a numeric index");
					return nextToEdit;
				}
				if (index >= toEdit.Count)
				{
					Log.Error("SEdit.Data could not parse field " + field + " because the index was out of bounds");
					return nextToEdit;
				}
				object item2 = toEdit[index];
				if (item2 == null)
				{
					Log.Error("SEdit.Data could not parse field " + field + " because the value at the index was null");
					return nextToEdit;
				}
				nextToEdit.Add(item2);
			}
			else
			{
				Type t = toEdit[0]?.GetType();
				if ((object)t == null)
				{
					Log.Error("SEdit.Data could not find type of index 0");
					return nextToEdit;
				}
				if (!t.TryGetMemberOfName("Id", out MemberInfo id) && !t.TryGetMemberOfName("ID", out id))
				{
					Log.Error("SEdit.Data could not find key field for list");
					return nextToEdit;
				}
				foreach (object item3 in toEdit)
				{
					if (!((string)MemberInfoExtensions.GetValue(id, item3) != field))
					{
						nextToEdit.Add(item3);
						return nextToEdit;
					}
				}
			}
			return nextToEdit;
		}

		private static IEnumerable<object> GetDictionaryEdits(string field, IDictionary toEdit)
		{
			if (field == "*")
			{
				List<object> edits = new List<object>();
				{
					foreach (object toEditItem in toEdit.Values)
					{
						edits.Add(toEditItem);
					}
					return edits;
				}
			}
			if (!toEdit.Contains(field))
			{
				Log.Error("SEdit.Data could not find dictionary key with value " + field);
				return Array.Empty<object>();
			}
			object item = toEdit[field];
			if (item != null)
			{
				return new _003C_003Ez__ReadOnlyArray<object>(new object[1] { item });
			}
			Log.Error("SEdit.Data dictionary contained null value for " + field);
			return Array.Empty<object>();
		}

		private static IEnumerable<object> GetMemberEdits(string field, object toEdit)
		{
			if (!toEdit.GetType().TryGetMemberOfName(field, out MemberInfo memberInfo))
			{
				Log.Error("SEdit.Data could not find field or property of name " + field);
				return Array.Empty<object>();
			}
			object nextToEdit = MemberInfoExtensions.GetValue(memberInfo, toEdit);
			if (nextToEdit != null)
			{
				return new _003C_003Ez__ReadOnlyArray<object>(new object[1] { nextToEdit });
			}
			Log.Error("SEdit.Data could not find field or property of name " + field);
			return Array.Empty<object>();
		}

		private static void ApplyListEdit(IList toEdit, object? edit)
		{
			Type t = toEdit[0]?.GetType();
			if ((object)t == null)
			{
				Log.Error("SEdit.Data could not find edits");
				return;
			}
			MemberInfo id;
			bool hasId = t.TryGetMemberOfName("Id", out id) || t.TryGetMemberOfName("ID", out id);
			if (!(edit is IList editList))
			{
				if (!hasId)
				{
					toEdit.Add(edit);
					return;
				}
				for (int i = 0; i < toEdit.Count; i++)
				{
					if (MemberInfoExtensions.GetValue(id, toEdit[i]) == MemberInfoExtensions.GetValue(id, edit))
					{
						toEdit[i] = edit;
						return;
					}
				}
				toEdit.Add(edit);
				return;
			}
			foreach (object editListItem in editList)
			{
				if (!hasId)
				{
					toEdit.Add(editListItem);
					break;
				}
				for (int j = 0; j < toEdit.Count; j++)
				{
					if (MemberInfoExtensions.GetValue(id, toEdit[j]) == MemberInfoExtensions.GetValue(id, editListItem))
					{
						toEdit[j] = editListItem;
						return;
					}
				}
				toEdit.Add(editListItem);
			}
		}

		private void ApplyDictionaryEdit(IDictionary toEdit, object? edit, string name)
		{
			if (!(edit is IDictionary editDictionary))
			{
				IMod? mod = Mod;
				string key = ((mod != null) ? mod.ModManifest.UniqueID : null) + "_" + name;
				toEdit[key] = edit;
				return;
			}
			foreach (DictionaryEntry editDictionaryEntry in editDictionary)
			{
				toEdit[editDictionaryEntry.Key] = editDictionaryEntry.Value;
			}
		}

		private static void ApplyMemberEdit(object toEdit, object? edit)
		{
			DeepClonerExtensions.ShallowCloneTo<object, object>(edit, toEdit);
		}
	}

	public class Image : BaseEdit
	{
		public Image(string target, PatchMode patchMode, string? condition = null, Frequency frequency = Frequency.Never, AssetEditPriority priority = (AssetEditPriority)0)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			_003CpatchMode_003EP = patchMode;
			base._002Ector(target, condition, frequency, priority);
		}

		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			if (fieldType != typeof(string))
			{
				Log.Error($"SEdit.Image only works with string fields or properties, but was {fieldType}");
			}
			else
			{
				base.Handle(name, fieldType, getter, setter, instance, mod, args);
			}
		}

		protected override void DoEdit(IAssetData asset, object? edit, string name, Type fieldType, object? instance)
		{
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			if (edit != null && Mod != null)
			{
				string filePath = (string)edit;
				IAssetDataForImage image = asset.AsImage();
				IRawTextureData source = Mod.Helper.ModContent.Load<IRawTextureData>(filePath);
				Rectangle? sourceRect = null;
				Rectangle? targetRect = null;
				if (fieldType.TryGetGetterOfName(name + "SourceArea", out Func<object, object> rectGetter))
				{
					sourceRect = (Rectangle?)rectGetter(instance);
				}
				if (fieldType.TryGetGetterOfName(name + "TargetArea", out rectGetter))
				{
					targetRect = (Rectangle?)rectGetter(instance);
				}
				image.PatchImage(source, sourceRect, targetRect, _003CpatchMode_003EP);
			}
		}
	}

	public class Map : BaseEdit
	{
		public Map(string target, PatchMapMode patchMode = (PatchMapMode)0, string? condition = null, Frequency frequency = Frequency.Never, AssetEditPriority priority = (AssetEditPriority)0)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			_003CpatchMode_003EP = patchMode;
			base._002Ector(target, condition, frequency, priority);
		}

		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			if (fieldType != typeof(string))
			{
				Log.Error($"SEdit.Map only works with string fields or properties, but was {fieldType}");
			}
			else
			{
				base.Handle(name, fieldType, getter, setter, instance, mod, args);
			}
		}

		protected override void DoEdit(IAssetData asset, object? edit, string name, Type fieldType, object? instance)
		{
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			if (edit != null && Mod != null)
			{
				string filePath = (string)edit;
				IAssetDataForMap map = asset.AsMap();
				Map source = Mod.Helper.ModContent.Load<Map>(filePath);
				Rectangle? sourceRect = null;
				Rectangle? targetRect = null;
				if (fieldType.TryGetGetterOfName(name + "SourceArea", out Func<object, object> rectGetter))
				{
					sourceRect = (Rectangle?)rectGetter(instance);
				}
				if (fieldType.TryGetGetterOfName(name + "TargetArea", out rectGetter))
				{
					targetRect = (Rectangle?)rectGetter(instance);
				}
				map.PatchMap(source, sourceRect, targetRect, _003CpatchMode_003EP);
			}
		}
	}

	public override void Handle(Type type, object? instance, IMod mod, object[]? args = null)
	{
		base.Handle(type, instance, mod, args);
		mod.Helper.Events.Content.AssetRequested += delegate(object? sender, AssetRequestedEventArgs e)
		{
			if (e.Name.IsEquivalentTo("Mods/" + mod.ModManifest.UniqueID + "/Strings", false))
			{
				e.Edit((Action<IAssetData>)delegate(IAssetData apply)
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					foreach (Translation current in mod.Helper.Translation.GetTranslations())
					{
						dictionary[current.Key] = ((object)current).ToString();
					}
					((IAssetData<object>)(object)apply).ReplaceWith((object)dictionary);
				}, (AssetEditPriority)(-1000), (string)null);
			}
		};
		mod.Helper.Events.Content.LocaleChanged += delegate
		{
			mod.Helper.GameContent.InvalidateCache("Mods/" + mod.ModManifest.UniqueID + "/Strings");
		};
	}
}
