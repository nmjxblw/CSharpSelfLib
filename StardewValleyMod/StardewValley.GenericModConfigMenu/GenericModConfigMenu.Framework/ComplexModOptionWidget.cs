using GenericModConfigMenu.Framework.ModOption;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared.UI;

namespace GenericModConfigMenu.Framework;

internal class ComplexModOptionWidget : Element
{
	public ComplexModOption ModOption { get; }

	public override int Width { get; }

	public override int Height => this.ModOption.Height();

	public ComplexModOptionWidget(ComplexModOption modOption)
	{
		this.ModOption = modOption;
	}

	public override void Update(bool isOffScreen = false)
	{
	}

	public override void Draw(SpriteBatch b)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsHidden())
		{
			this.ModOption.Draw(b, base.Position);
		}
	}
}
