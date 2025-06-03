using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GenericModConfigMenu.Framework.ModOption;

internal class ComplexModOption : BaseModOption
{
	private readonly Action<SpriteBatch, Vector2> DrawImpl;

	private readonly Action BeforeSaveImpl;

	private readonly Action AfterSaveImpl;

	private readonly Action BeforeResetImpl;

	private readonly Action AfterResetImpl;

	private readonly Action BeforeMenuOpenedImpl;

	private readonly Action BeforeMenuClosedImpl;

	public Func<int> Height { get; }

	public ComplexModOption(string fieldId, Func<string> name, Func<string> tooltip, ModConfig mod, Func<int> height, Action<SpriteBatch, Vector2> draw, Action beforeMenuOpened, Action beforeSave, Action afterSave, Action beforeReset, Action afterReset, Action beforeMenuClosed)
		: base(fieldId, name, tooltip, mod)
	{
		if (height == null)
		{
			height = () => 0;
		}
		this.Height = height;
		this.DrawImpl = draw;
		this.BeforeMenuOpenedImpl = beforeMenuOpened;
		this.BeforeSaveImpl = beforeSave;
		this.AfterSaveImpl = afterSave;
		this.BeforeResetImpl = beforeReset;
		this.AfterResetImpl = afterReset;
		this.BeforeMenuClosedImpl = beforeMenuClosed;
	}

	public override void BeforeSave()
	{
		this.BeforeSaveImpl?.Invoke();
	}

	public override void AfterSave()
	{
		this.AfterSaveImpl?.Invoke();
	}

	public override void BeforeReset()
	{
		this.BeforeResetImpl?.Invoke();
	}

	public override void AfterReset()
	{
		this.AfterResetImpl?.Invoke();
	}

	public override void BeforeMenuOpened()
	{
		this.BeforeMenuOpenedImpl?.Invoke();
	}

	public override void BeforeMenuClosed()
	{
		this.BeforeMenuClosedImpl?.Invoke();
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		this.DrawImpl(spriteBatch, position);
	}
}
