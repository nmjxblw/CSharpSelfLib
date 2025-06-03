using System;
using SpaceShared;

namespace GenericModConfigMenu.Framework.ModOption;

internal class NumericModOption<T> : SimpleModOption<T> where T : struct
{
	private readonly Func<T, string> FormatValueImpl;

	public T? Minimum { get; }

	public T? Maximum { get; }

	public T? Interval { get; }

	public override T Value
	{
		get
		{
			return base.Value;
		}
		set
		{
			if (this.Minimum.HasValue || this.Maximum.HasValue)
			{
				T t = value;
				value = Util.Clamp(this.Minimum ?? value, t, this.Maximum ?? value);
			}
			if (this.Interval.HasValue)
			{
				value = Util.Adjust(value, this.Interval.Value);
			}
			base.Value = value;
		}
	}

	public NumericModOption(string fieldId, Func<string> name, Func<string> tooltip, ModConfig mod, Func<T> getValue, Action<T> setValue, T? min, T? max, T? interval, Func<T, string> formatValue)
		: base(fieldId, name, tooltip, mod, getValue, setValue)
	{
		this.Minimum = min;
		this.Maximum = max;
		this.Interval = interval;
		this.FormatValueImpl = formatValue;
	}

	public override string FormatValue()
	{
		return this.FormatValueImpl?.Invoke(this.Value) ?? this.Value.ToString();
	}
}
