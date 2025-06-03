using System;
using System.Collections.Generic;
using GenericModConfigMenu.Framework.ModOption;

namespace GenericModConfigMenu.Framework;

internal class ModConfigPage
{
	public string PageId { get; }

	public Func<string> PageTitle { get; private set; }

	public List<BaseModOption> Options { get; } = new List<BaseModOption>();

	public ModConfigPage(string pageId, Func<string> pageTitle)
	{
		if (pageTitle == null)
		{
			pageTitle = () => pageId;
		}
		this.PageId = pageId;
		this.PageTitle = pageTitle;
	}

	[Obsolete("This is only intended to support backwards compatibility. Most code should set the value in the constructor instead.")]
	public void SetPageTitle(Func<string> value)
	{
		this.PageTitle = value;
	}
}
