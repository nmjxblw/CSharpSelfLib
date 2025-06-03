using System;
using System.Linq;

namespace GenericModConfigMenu.Framework.ModOption;

internal class ChoiceModOption<T> : SimpleModOption<T>
{
	public T[] Choices { get; }

	public Func<string, string> FormatChoice { get; }

	public override T Value
	{
		get
		{
			return base.Value;
		}
		set
		{
			if (this.Choices.Contains(value))
			{
				base.Value = value;
			}
		}
	}

	public ChoiceModOption(string fieldId, Func<string> name, Func<string> tooltip, ModConfig mod, Func<T> getValue, Action<T> setValue, T[] choices, Func<string, string> formatChoice = null)
		: base(fieldId, name, tooltip, mod, getValue, setValue)
	{
		this.Choices = choices;
		this.FormatChoice = formatChoice;
	}
}
