using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.UI;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Components;

internal class SearchMenu : BaseMenu, IScrollableMenu, IDisposable
{
	private readonly Action<ISubject> ShowLookup;

	private readonly IMonitor Monitor;

	private readonly Vector2 AspectRatio = new Vector2((float)Sprites.Letter.Sprite.Width, (float)Sprites.Letter.Sprite.Height);

	private readonly ClickableTextureComponent ScrollUpButton;

	private readonly ClickableTextureComponent ScrollDownButton;

	private readonly int ScrollAmount;

	private int MaxScroll;

	private int CurrentScroll;

	private readonly ILookup<string, ISubject> SearchLookup;

	private readonly SearchTextBox SearchTextbox;

	private IEnumerable<SearchResultComponent> SearchResults = Array.Empty<SearchResultComponent>();

	private Rectangle SearchResultArea;

	private readonly int SearchResultGutter = 15;

	private readonly int ScrollButtonGutter = 15;

	public SearchMenu(IEnumerable<ISubject> searchSubjects, Action<ISubject> showLookup, IMonitor monitor, int scroll)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		this.ShowLookup = showLookup;
		this.Monitor = monitor;
		this.SearchLookup = searchSubjects.Where((ISubject p) => !string.IsNullOrWhiteSpace(p.Name)).ToLookup<ISubject, string>((ISubject p) => p.Name, StringComparer.OrdinalIgnoreCase);
		this.ScrollAmount = scroll;
		this.SearchTextbox = new SearchTextBox(Game1.smallFont, Color.Black);
		this.ScrollUpButton = new ClickableTextureComponent(Rectangle.Empty, CommonSprites.Icons.Sheet, CommonSprites.Icons.UpArrow, 1f, false);
		this.ScrollDownButton = new ClickableTextureComponent(Rectangle.Empty, CommonSprites.Icons.Sheet, CommonSprites.Icons.DownArrow, 1f, false);
		this.UpdateLayout();
		this.SearchTextbox.Select();
		this.SearchTextbox.OnChanged += delegate(object? _, string text)
		{
			this.ReceiveSearchTextboxChanged(text);
		};
	}

	public override bool overrideSnappyMenuCursorMovementBan()
	{
		return true;
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (((ClickableComponent)((IClickableMenu)this).upperRightCloseButton).containsPoint(x, y))
		{
			((IClickableMenu)this).exitThisMenu(true);
			return;
		}
		Rectangle bounds = this.SearchTextbox.Bounds;
		if (((Rectangle)(ref bounds)).Contains(x, y))
		{
			this.SearchTextbox.Select();
		}
		else if (((ClickableComponent)this.ScrollUpButton).containsPoint(x, y))
		{
			this.ScrollUp();
		}
		else if (((ClickableComponent)this.ScrollDownButton).containsPoint(x, y))
		{
			this.ScrollDown();
		}
		else
		{
			if (!((Rectangle)(ref this.SearchResultArea)).Contains(x, y))
			{
				return;
			}
			foreach (SearchResultComponent match in this.GetResultsPossiblyOnScreen())
			{
				if (((ClickableComponent)match).containsPoint(x, y))
				{
					this.ShowLookup(match.Subject);
					Game1.playSound("coin", (int?)null);
					break;
				}
			}
		}
	}

	public unsafe override void receiveKeyPress(Keys key)
	{
		if (((object)(*(Keys*)(&key))/*cast due to .constrained prefix*/).Equals((object)(Keys)27))
		{
			((IClickableMenu)this).exitThisMenu(true);
		}
	}

	public override void receiveGamePadButton(Buttons button)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if ((int)button != 8192)
		{
			if ((int)button != 16777216)
			{
				if ((int)button == 33554432)
				{
					this.ScrollDown();
				}
				else
				{
					((IClickableMenu)this).receiveGamePadButton(button);
				}
			}
			else
			{
				this.ScrollUp();
			}
		}
		else
		{
			((IClickableMenu)this).exitThisMenu(true);
		}
	}

	public override void receiveScrollWheelAction(int direction)
	{
		if (direction > 0)
		{
			this.ScrollUp();
		}
		else
		{
			this.ScrollDown();
		}
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		this.UpdateLayout();
	}

	public override void draw(SpriteBatch b)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Expected O, but got Unknown
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		int x = ((IClickableMenu)this).xPositionOnScreen;
		int y = ((IClickableMenu)this).yPositionOnScreen;
		int gutter = this.SearchResultGutter;
		float leftOffset = gutter;
		float topOffset = gutter;
		float contentHeight = this.SearchResultArea.Height;
		SpriteFont font = Game1.smallFont;
		float lineHeight = font.MeasureString("ABC").Y;
		float spaceWidth = DrawHelper.GetSpaceWidth(font);
		SpriteBatch backgroundBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);
		try
		{
			backgroundBatch.Begin((SpriteSortMode)0, BlendState.NonPremultiplied, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null, (Effect)null, (Matrix?)null);
			Texture2D sheet = Sprites.Letter.Sheet;
			Rectangle sprite = Sprites.Letter.Sprite;
			float x2 = x;
			float y2 = y;
			Rectangle sprite2 = Sprites.Letter.Sprite;
			Point size = ((Rectangle)(ref sprite2)).Size;
			float scale = (float)((IClickableMenu)this).width / (float)Sprites.Letter.Sprite.Width;
			backgroundBatch.DrawSprite(sheet, sprite, x2, y2, size, null, scale);
			backgroundBatch.End();
		}
		finally
		{
			((IDisposable)backgroundBatch)?.Dispose();
		}
		SpriteBatch contentBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);
		try
		{
			GraphicsDevice device = Game1.graphics.GraphicsDevice;
			Rectangle prevScissorRectangle = device.ScissorRectangle;
			try
			{
				device.ScissorRectangle = this.SearchResultArea;
				contentBatch.Begin((SpriteSortMode)0, BlendState.NonPremultiplied, SamplerState.PointClamp, (DepthStencilState)null, new RasterizerState
				{
					ScissorTestEnable = true
				}, (Effect)null, (Matrix?)null);
				this.CurrentScroll = Math.Max(0, this.CurrentScroll);
				this.CurrentScroll = Math.Min(this.MaxScroll, this.CurrentScroll);
				topOffset -= (float)this.CurrentScroll;
				float wrapWidth = (float)((IClickableMenu)this).width - leftOffset - (float)gutter;
				Vector2 nameSize = contentBatch.DrawTextBlock(font, "Search", new Vector2((float)x + leftOffset, (float)y + topOffset), wrapWidth, null, bold: true);
				Vector2 typeSize = contentBatch.DrawTextBlock(font, "(Lookup Anything)", new Vector2((float)x + leftOffset + nameSize.X + spaceWidth, (float)y + topOffset), wrapWidth);
				topOffset += Math.Max(nameSize.Y, typeSize.Y);
				this.SearchTextbox.Bounds = new Rectangle(x + (int)leftOffset, y + (int)topOffset, (int)wrapWidth, this.SearchTextbox.Bounds.Height);
				this.SearchTextbox.Draw(contentBatch);
				topOffset += (float)this.SearchTextbox.Bounds.Height;
				int mouseX = Game1.getMouseX();
				int mouseY = Game1.getMouseY();
				bool reachedViewport = false;
				bool reachedBottomOfViewport = false;
				bool isCursorInSearchArea = ((Rectangle)(ref this.SearchResultArea)).Contains(mouseX, mouseY) && !((ClickableComponent)this.ScrollUpButton).containsPoint(mouseX, mouseY) && !((ClickableComponent)this.ScrollDownButton).containsPoint(mouseX, mouseY);
				foreach (SearchResultComponent result in this.SearchResults)
				{
					if (!reachedViewport || !reachedBottomOfViewport)
					{
						if (this.IsResultPossiblyOnScreen(result))
						{
							reachedViewport = true;
							bool isHighlighted = isCursorInSearchArea && ((ClickableComponent)result).containsPoint(mouseX, mouseY);
							result.Draw(contentBatch, new Vector2((float)x + leftOffset, (float)y + topOffset), (int)wrapWidth, isHighlighted);
						}
						else if (reachedViewport)
						{
							reachedBottomOfViewport = true;
						}
					}
					topOffset += 70f;
				}
				topOffset += lineHeight;
				this.MaxScroll = Math.Max(0, (int)(topOffset - contentHeight + (float)this.CurrentScroll));
				if (this.MaxScroll > 0 && this.CurrentScroll > 0)
				{
					this.ScrollUpButton.draw(b);
				}
				if (this.MaxScroll > 0 && this.CurrentScroll < this.MaxScroll)
				{
					this.ScrollDownButton.draw(b);
				}
				contentBatch.End();
			}
			catch (ArgumentException ex) when (!BaseMenu.UseSafeDimensions && ex.ParamName == "value" && (ex.StackTrace?.Contains("Microsoft.Xna.Framework.Graphics.GraphicsDevice.set_ScissorRectangle") ?? false))
			{
				this.Monitor.Log("The viewport size seems to be inaccurate. Enabling compatibility mode; lookup menu may be misaligned.", (LogLevel)3);
				this.Monitor.Log(ex.ToString(), (LogLevel)0);
				BaseMenu.UseSafeDimensions = true;
				this.UpdateLayout();
			}
			finally
			{
				device.ScissorRectangle = prevScissorRectangle;
			}
		}
		finally
		{
			((IDisposable)contentBatch)?.Dispose();
		}
		((IClickableMenu)this).upperRightCloseButton.draw(b);
		((IClickableMenu)this).drawMouse(Game1.spriteBatch, false, -1);
	}

	public void Dispose()
	{
		this.SearchTextbox.Dispose();
	}

	public void ScrollUp(int? amount = null)
	{
		this.CurrentScroll -= amount ?? this.ScrollAmount;
	}

	public void ScrollDown(int? amount = null)
	{
		this.CurrentScroll += amount ?? this.ScrollAmount;
	}

	private IEnumerable<SearchResultComponent> GetResultsPossiblyOnScreen()
	{
		bool reachedViewport = false;
		foreach (SearchResultComponent result in this.SearchResults)
		{
			if (!this.IsResultPossiblyOnScreen(result))
			{
				if (reachedViewport)
				{
					yield break;
				}
			}
			else
			{
				reachedViewport = true;
				yield return result;
			}
		}
	}

	private bool IsResultPossiblyOnScreen(SearchResultComponent result)
	{
		int index = result.Index;
		int minY = (index - 3) * 70;
		int maxY = (index + 3) * 70;
		if (maxY > this.CurrentScroll)
		{
			return minY < this.CurrentScroll + ((IClickableMenu)this).height;
		}
		return false;
	}

	private void ReceiveSearchTextboxChanged(string? search)
	{
		string[] words = (search ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
		if (!words.Any())
		{
			this.SearchResults = Array.Empty<SearchResultComponent>();
			return;
		}
		this.SearchResults = this.SearchLookup.Where((IGrouping<string, ISubject> entry) => words.All((string word) => entry.Key.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)).SelectMany((IGrouping<string, ISubject> entry) => entry).OrderBy<ISubject, string>((ISubject subject) => subject.Name, StringComparer.OrdinalIgnoreCase)
			.Select((ISubject subject, int index) => new SearchResultComponent(subject, index))
			.ToArray();
	}

	private void UpdateLayout()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		Point viewport = base.GetViewportSize();
		((IClickableMenu)this).width = Math.Min(896, viewport.X);
		((IClickableMenu)this).height = Math.Min((int)(this.AspectRatio.Y / this.AspectRatio.X * (float)((IClickableMenu)this).width), viewport.Y);
		Vector2 origin = Utility.getTopLeftPositionForCenteringOnScreen(((IClickableMenu)this).width, ((IClickableMenu)this).height, 0, 0);
		int x = (((IClickableMenu)this).xPositionOnScreen = (int)origin.X);
		int y = (((IClickableMenu)this).yPositionOnScreen = (int)origin.Y);
		int searchGutter = this.SearchResultGutter;
		float contentWidth = ((IClickableMenu)this).width - searchGutter * 2;
		float contentHeight = ((IClickableMenu)this).height - searchGutter * 2;
		this.SearchResultArea = new Rectangle(x + searchGutter, y + searchGutter, (int)contentWidth, (int)contentHeight);
		int scrollGutter = this.ScrollButtonGutter;
		((ClickableComponent)this.ScrollUpButton).bounds = new Rectangle(x + scrollGutter, (int)((float)y + contentHeight - (float)CommonSprites.Icons.UpArrow.Height - (float)scrollGutter - (float)CommonSprites.Icons.DownArrow.Height), CommonSprites.Icons.UpArrow.Height, CommonSprites.Icons.UpArrow.Width);
		((ClickableComponent)this.ScrollDownButton).bounds = new Rectangle(x + scrollGutter, (int)((float)y + contentHeight - (float)CommonSprites.Icons.DownArrow.Height), CommonSprites.Icons.DownArrow.Height, CommonSprites.Icons.DownArrow.Width);
		((IClickableMenu)this).initializeUpperRightCloseButton();
	}
}
