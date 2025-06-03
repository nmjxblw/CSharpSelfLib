using System;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace GenericModConfigMenu.Framework.ModOption;

internal class SimpleModOption<T> : BaseModOption
{
	private T CachedValue;

	protected readonly Func<T> GetValue;

	protected readonly Action<T> SetValue;

	public Type Type => typeof(T);

	public virtual T Value
	{
		get
		{
			return this.CachedValue;
		}
		set
		{
			ref T cachedValue = ref this.CachedValue;
			object obj = value;
			if (!cachedValue.Equals(obj))
			{
				base.Owner.ChangeHandlers.ForEach(delegate(Action<string, object> handler)
				{
					handler(base.FieldId, value);
				});
			}
			this.CachedValue = value;
		}
	}

	public SimpleModOption(string fieldId, Func<string> name, Func<string> tooltip, ModConfig mod, Func<T> getValue, Action<T> setValue)
		: base(fieldId, name, tooltip, mod)
	{
		this.GetValue = getValue;
		this.SetValue = setValue;
		this.CachedValue = this.GetValue();
	}

	public override void BeforeReset()
	{
		this.GetLatest();
	}

	public override void AfterReset()
	{
		this.GetLatest();
	}

	public override void BeforeSave()
	{
		Log.Trace("saving " + base.Name() + " " + base.Tooltip());
		this.SetValue(this.CachedValue);
	}

	public override void AfterSave()
	{
	}

	public override void BeforeMenuOpened()
	{
		this.GetLatest();
	}

	public override void BeforeMenuClosed()
	{
	}

	public virtual string FormatValue()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		T value = this.Value;
		if (!(value is SButton button))
		{
			object obj = value;
			KeybindList keybind = (KeybindList)((obj is KeybindList) ? obj : null);
			if (keybind != null && !keybind.IsBound)
			{
				goto IL_0045;
			}
		}
		else if ((int)button == 0)
		{
			goto IL_0045;
		}
		T value2 = this.Value;
		if (value2 == null)
		{
			return null;
		}
		return value2.ToString();
		IL_0045:
		return I18n.Config_RebindKey_NoKey();
	}

	private void GetLatest()
	{
		this.CachedValue = this.GetValue();
	}
}
