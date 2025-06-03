using System;

namespace GenericModConfigMenu.Framework.ModOption;

internal class PageLinkModOption : ReadOnlyModOption
{
	public string PageId { get; }

	public PageLinkModOption(string pageId, Func<string> text, Func<string> tooltip, ModConfig mod)
		: base(text, tooltip, mod)
	{
		this.PageId = pageId;
	}
}
