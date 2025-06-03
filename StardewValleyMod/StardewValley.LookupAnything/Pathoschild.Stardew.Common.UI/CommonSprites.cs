using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.Stardew.Common.UI;

internal static class CommonSprites
{
	public static class Button
	{
		public static readonly Rectangle Background = new Rectangle(297, 364, 1, 1);

		public static readonly Rectangle Top = new Rectangle(279, 284, 1, 4);

		public static readonly Rectangle Bottom = new Rectangle(279, 296, 1, 4);

		public static readonly Rectangle Left = new Rectangle(274, 289, 4, 1);

		public static readonly Rectangle Right = new Rectangle(286, 289, 4, 1);

		public static readonly Rectangle TopLeft = new Rectangle(274, 284, 4, 4);

		public static readonly Rectangle TopRight = new Rectangle(286, 284, 4, 4);

		public static readonly Rectangle BottomLeft = new Rectangle(274, 296, 4, 4);

		public static readonly Rectangle BottomRight = new Rectangle(286, 296, 4, 4);

		public static Texture2D Sheet => Game1.mouseCursors;
	}

	public static class DropDown
	{
		public static readonly Texture2D Sheet = Game1.mouseCursors;

		public static readonly Rectangle ActiveBackground = new Rectangle(258, 258, 4, 4);

		public static readonly Rectangle InactiveBackground = new Rectangle(269, 258, 4, 4);

		public static readonly Rectangle HoverBackground = new Rectangle(161, 340, 4, 4);
	}

	public static class Icons
	{
		public static readonly Rectangle DownArrow = new Rectangle(12, 76, 40, 44);

		public static readonly Rectangle UpArrow = new Rectangle(76, 72, 40, 44);

		public static readonly Rectangle LeftArrow = new Rectangle(8, 268, 44, 40);

		public static readonly Rectangle RightArrow = new Rectangle(12, 204, 44, 40);

		public static readonly Rectangle SpeechBubble = new Rectangle(66, 4, 14, 12);

		public static readonly Rectangle EmptyCheckbox = new Rectangle(227, 425, 9, 9);

		public static readonly Rectangle FilledHeart = new Rectangle(211, 428, 7, 6);

		public static readonly Rectangle EmptyHeart = new Rectangle(218, 428, 7, 6);

		public static readonly Rectangle FilledCheckbox = new Rectangle(236, 425, 9, 9);

		public static readonly Rectangle ExitButton = new Rectangle(337, 494, 12, 12);

		public static readonly Rectangle Stardrop = new Rectangle(346, 392, 8, 8);

		public static Texture2D Sheet => Game1.mouseCursors;
	}

	public static class Scroll
	{
		public static readonly Rectangle Background = new Rectangle(334, 321, 1, 1);

		public static readonly Rectangle Top = new Rectangle(331, 318, 1, 2);

		public static readonly Rectangle Bottom = new Rectangle(327, 334, 1, 2);

		public static readonly Rectangle Left = new Rectangle(325, 320, 6, 1);

		public static readonly Rectangle Right = new Rectangle(344, 320, 6, 1);

		public static readonly Rectangle TopLeft = new Rectangle(325, 318, 6, 2);

		public static readonly Rectangle TopRight = new Rectangle(344, 318, 6, 2);

		public static readonly Rectangle BottomLeft = new Rectangle(325, 334, 6, 2);

		public static readonly Rectangle BottomRight = new Rectangle(344, 334, 6, 2);

		public static Texture2D Sheet => Game1.mouseCursors;
	}

	public static class Tab
	{
		public static readonly Texture2D Sheet = Game1.mouseCursors;

		public static readonly Rectangle TopLeft = new Rectangle(0, 384, 5, 5);

		public static readonly Rectangle TopRight = new Rectangle(11, 384, 5, 5);

		public static readonly Rectangle BottomLeft = new Rectangle(0, 395, 5, 5);

		public static readonly Rectangle BottomRight = new Rectangle(11, 395, 5, 5);

		public static readonly Rectangle Top = new Rectangle(4, 384, 1, 3);

		public static readonly Rectangle Left = new Rectangle(0, 388, 3, 1);

		public static readonly Rectangle Right = new Rectangle(13, 388, 3, 1);

		public static readonly Rectangle Bottom = new Rectangle(4, 397, 1, 3);

		public static readonly Rectangle Background = new Rectangle(5, 387, 1, 1);
	}
}
