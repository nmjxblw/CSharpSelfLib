using System;

namespace GenericModConfigMenu.Framework.ModOption;

internal class SectionTitleModOption : ReadOnlyModOption
{
	public SectionTitleModOption(Func<string> text, Func<string> tooltip, ModConfig mod)
		: base(text, tooltip, mod)
	{
	}
}
