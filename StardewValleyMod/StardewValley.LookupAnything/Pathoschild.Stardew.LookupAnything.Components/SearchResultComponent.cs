using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Components;

internal class SearchResultComponent : ClickableComponent
{
	public const int FixedHeight = 70;

	public ISubject Subject { get; }

	public int Index { get; }

	public SearchResultComponent(ISubject subject, int index)
		: base(Rectangle.Empty, subject.Name)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		this.Subject = subject;
		this.Index = index;
	}

	public Vector2 Draw(SpriteBatch spriteBatch, Vector2 position, int width, bool highlight = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		base.bounds.X = (int)position.X;
		base.bounds.Y = (int)position.Y;
		base.bounds.Width = width;
		base.bounds.Height = 70;
		int iconSize = 70;
		int topPadding = base.bounds.Height / 2;
		if (highlight)
		{
			DrawHelper.DrawLine(spriteBatch, base.bounds.X, base.bounds.Y, new Vector2((float)base.bounds.Width, (float)base.bounds.Height), Color.Beige);
		}
		DrawHelper.DrawLine(spriteBatch, base.bounds.X, base.bounds.Y, new Vector2((float)base.bounds.Width, 2f), Color.Black);
		spriteBatch.DrawTextBlock(Game1.smallFont, this.Subject.Name + " (" + this.Subject.Type + ")", new Vector2((float)base.bounds.X, (float)base.bounds.Y) + new Vector2((float)iconSize, (float)topPadding), base.bounds.Width - iconSize);
		this.Subject.DrawPortrait(spriteBatch, position, new Vector2((float)iconSize));
		return new Vector2((float)base.bounds.Width, (float)base.bounds.Height);
	}
}
