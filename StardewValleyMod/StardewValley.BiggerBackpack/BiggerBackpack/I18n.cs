using System;
using System.CodeDom.Compiler;
using StardewModdingAPI;

namespace BiggerBackpack;

[GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
internal static class I18n
{
	private static ITranslationHelper? Translations;

	public static void Init(ITranslationHelper translations)
	{
		I18n.Translations = translations;
	}

	public static string SetBackpackSizeCommand()
	{
		return Translation.op_Implicit(I18n.GetByKey("SetBackpackSizeCommand"));
	}

	public static string Purchase(object? cost)
	{
		return Translation.op_Implicit(I18n.GetByKey("Purchase", new { cost }));
	}

	public static string BackpackUpgrade()
	{
		return Translation.op_Implicit(I18n.GetByKey("BackpackUpgrade"));
	}

	public static string PremiumPack()
	{
		return Translation.op_Implicit(I18n.GetByKey("PremiumPack"));
	}

	public static string BackpackCostName()
	{
		return Translation.op_Implicit(I18n.GetByKey("BackpackCostName"));
	}

	public static string BackpackCostTooltip()
	{
		return Translation.op_Implicit(I18n.GetByKey("BackpackCostTooltip"));
	}

	private static Translation GetByKey(string key, object? tokens = null)
	{
		if (I18n.Translations == null)
		{
			throw new InvalidOperationException("You must call I18n.Init from the mod's entry method before reading translations.");
		}
		return I18n.Translations.Get(key, tokens);
	}
}
