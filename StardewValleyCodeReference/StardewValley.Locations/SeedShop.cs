using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Locations;

public class SeedShop : ShopLocation
{
	public SeedShop()
	{
	}

	public SeedShop(string map, string name)
		: base(map, name)
	{
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (Game1.player.maxItems.Value == 12)
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(456f, 1088f)), new Rectangle(255, 1436, 12, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1232f);
		}
		else if (Game1.player.maxItems.Value < 36)
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(456f, 1088f)), new Rectangle(267, 1436, 12, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1232f);
		}
		else
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Rectangle(452, 1184, 112, 20)), new Rectangle(258, 1449, 1, 1), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.1232f);
		}
	}
}
