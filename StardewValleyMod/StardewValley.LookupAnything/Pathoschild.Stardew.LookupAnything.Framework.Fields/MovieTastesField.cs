using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class MovieTastesField : GenericField
{
	public MovieTastesField(string label, IDictionary<GiftTaste, string[]> giftTastes, GiftTaste showTaste)
		: base(label, MovieTastesField.GetText(giftTastes, showTaste))
	{
	}

	private static string? GetText(IDictionary<GiftTaste, string[]> giftTastes, GiftTaste showTaste)
	{
		if (!giftTastes.TryGetValue(showTaste, out string[] names))
		{
			return null;
		}
		names = names.OrderBy((string p) => p).ToArray();
		return I18n.List(names);
	}
}
