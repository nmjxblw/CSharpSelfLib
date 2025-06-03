using System;

namespace GenericModConfigMenu.Framework.ModOption;

internal abstract class ReadOnlyModOption : BaseModOption
{
	public override void BeforeReset()
	{
	}

	public override void AfterReset()
	{
	}

	public override void BeforeSave()
	{
	}

	public override void AfterSave()
	{
	}

	public override void BeforeMenuOpened()
	{
	}

	public override void BeforeMenuClosed()
	{
	}

	protected ReadOnlyModOption(Func<string> name, Func<string> tooltip, ModConfig mod)
		: base(null, name, tooltip, mod)
	{
	}
}
