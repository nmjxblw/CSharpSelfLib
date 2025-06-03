using System;

namespace GenericModConfigMenu.Framework.ModOption;

internal class ParagraphModOption : ReadOnlyModOption
{
	public ParagraphModOption(Func<string> text, ModConfig mod)
		: base(text, null, mod)
	{
	}
}
