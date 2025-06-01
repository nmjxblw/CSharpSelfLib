using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.GameData.Crafting;
using StardewValley.Objects;

namespace StardewValley.Menus;

public class TailorRecipeListTool : IClickableMenu
{
	public Rectangle scrollView;

	public List<ClickableTextureComponent> recipeComponents = new List<ClickableTextureComponent>();

	public ClickableTextureComponent okButton;

	public float scrollY;

	public Dictionary<string, KeyValuePair<Item, Item>> _recipeLookup = new Dictionary<string, KeyValuePair<Item, Item>>();

	public Item hoveredItem;

	public string hoverText = "";

	public Dictionary<string, string> _recipeHoverTexts = new Dictionary<string, string>();

	public Dictionary<string, string> _recipeOutputIds = new Dictionary<string, string>();

	public Dictionary<string, Color> _recipeColors = new Dictionary<string, Color>();

	public TailorRecipeListTool()
		: base(Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64, 632 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2 + 64)
	{
		TailoringMenu tailoring_menu = new TailoringMenu();
		Game1.player.faceDirection(2);
		Game1.player.FarmerSprite.StopAnimation();
		Item cloth = ItemRegistry.Create<Object>("(O)428");
		foreach (string allId in ItemRegistry.GetObjectTypeDefinition().GetAllIds())
		{
			Object key = new Object(allId, 1);
			if (key.Name.Contains("Seeds") || key.Name.Contains("Floor") || key.Name.Equals("Lumber") || key.Name.Contains("Fence") || key.Name.Equals("Gate") || key.Name.Contains("Starter") || key.Name.Equals("Secret Note") || key.Name.Contains("Guide") || key.Name.Contains("Path") || key.Name.Contains("Ring") || key.category.Value == -22 || key.Category == -999 || key.isSapling())
			{
				continue;
			}
			Item value = tailoring_menu.CraftItem(cloth, key);
			TailorItemRecipe recipe = tailoring_menu.GetRecipeForItems(cloth, key);
			KeyValuePair<Item, Item> kvp = new KeyValuePair<Item, Item>(key, value);
			this._recipeLookup[Utility.getStandardDescriptionFromItem(key, 1)] = kvp;
			string metadata = "";
			Color? dye_color = TailoringMenu.GetDyeColor(key);
			if (dye_color.HasValue)
			{
				this._recipeColors[Utility.getStandardDescriptionFromItem(key, 1)] = dye_color.Value;
			}
			if (recipe != null)
			{
				metadata = "clothes id: " + recipe.CraftedItemId + " from ";
				foreach (string context_tag in recipe.SecondItemTags)
				{
					metadata = metadata + context_tag + " ";
				}
				metadata.Trim();
			}
			this._recipeOutputIds[Utility.getStandardDescriptionFromItem(key, 1)] = TailoringMenu.ConvertLegacyItemId(recipe?.CraftedItemId) ?? value.QualifiedItemId;
			this._recipeHoverTexts[Utility.getStandardDescriptionFromItem(key, 1)] = metadata;
			ClickableTextureComponent component = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), null, default(Rectangle), 1f)
			{
				myID = 0,
				name = Utility.getStandardDescriptionFromItem(key, 1),
				label = key.DisplayName
			};
			this.recipeComponents.Add(component);
		}
		this.okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		};
		this.RepositionElements();
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		base.xPositionOnScreen = Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
		base.yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;
		this.RepositionElements();
	}

	private void RepositionElements()
	{
		this.scrollView = new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder, base.width - IClickableMenu.borderWidth, 500);
		if (this.scrollView.Left < Game1.graphics.GraphicsDevice.ScissorRectangle.Left)
		{
			int size_difference = Game1.graphics.GraphicsDevice.ScissorRectangle.Left - this.scrollView.Left;
			this.scrollView.X += size_difference;
			this.scrollView.Width -= size_difference;
		}
		if (this.scrollView.Right > Game1.graphics.GraphicsDevice.ScissorRectangle.Right)
		{
			int size_difference2 = this.scrollView.Right - Game1.graphics.GraphicsDevice.ScissorRectangle.Right;
			this.scrollView.X -= size_difference2;
			this.scrollView.Width -= size_difference2;
		}
		if (this.scrollView.Top < Game1.graphics.GraphicsDevice.ScissorRectangle.Top)
		{
			int size_difference3 = Game1.graphics.GraphicsDevice.ScissorRectangle.Top - this.scrollView.Top;
			this.scrollView.Y += size_difference3;
			this.scrollView.Width -= size_difference3;
		}
		if (this.scrollView.Bottom > Game1.graphics.GraphicsDevice.ScissorRectangle.Bottom)
		{
			int size_difference4 = this.scrollView.Bottom - Game1.graphics.GraphicsDevice.ScissorRectangle.Bottom;
			this.scrollView.Y -= size_difference4;
			this.scrollView.Width -= size_difference4;
		}
		this.RepositionScrollElements();
	}

	public void RepositionScrollElements()
	{
		int y_offset = (int)this.scrollY;
		if (this.scrollY > 0f)
		{
			this.scrollY = 0f;
		}
		foreach (ClickableTextureComponent component in this.recipeComponents)
		{
			component.bounds.X = this.scrollView.X;
			component.bounds.Y = this.scrollView.Y + y_offset;
			y_offset += component.bounds.Height;
			if (this.scrollView.Intersects(component.bounds))
			{
				component.visible = true;
			}
			else
			{
				component.visible = false;
			}
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		foreach (ClickableTextureComponent component in this.recipeComponents)
		{
			if (!component.bounds.Contains(x, y) || !this.scrollView.Contains(x, y))
			{
				continue;
			}
			try
			{
				Item item = ItemRegistry.Create(this._recipeOutputIds[component.name]);
				if (item is Clothing clothing && this._recipeColors.TryGetValue(component.name, out var color))
				{
					clothing.Dye(color, 1f);
				}
				Game1.player.addItemToInventoryBool(item);
			}
			catch (Exception)
			{
			}
		}
		if (this.okButton.containsPoint(x, y))
		{
			base.exitThisMenu();
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
	}

	/// <inheritdoc />
	public override void receiveScrollWheelAction(int direction)
	{
		this.scrollY += direction;
		this.RepositionScrollElements();
		base.receiveScrollWheelAction(direction);
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.hoveredItem = null;
		this.hoverText = "";
		foreach (ClickableTextureComponent component in this.recipeComponents)
		{
			if (component.containsPoint(x, y))
			{
				this.hoveredItem = this._recipeLookup[component.name].Value;
				this.hoverText = this._recipeHoverTexts[component.name];
			}
		}
	}

	public bool canLeaveMenu()
	{
		return true;
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, speaker: false, drawOnlyBox: true);
		b.End();
		Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
		b.GraphicsDevice.ScissorRectangle = this.scrollView;
		foreach (ClickableTextureComponent component in this.recipeComponents)
		{
			if (component.visible)
			{
				base.drawHorizontalPartition(b, component.bounds.Bottom - 32, small: true);
				KeyValuePair<Item, Item> kvp = this._recipeLookup[component.name];
				component.draw(b);
				kvp.Key.drawInMenu(b, new Vector2(component.bounds.X, component.bounds.Y), 1f);
				if (this._recipeColors.TryGetValue(component.name, out var color))
				{
					int size = 24;
					b.Draw(Game1.staminaRect, new Rectangle(this.scrollView.Left + this.scrollView.Width / 2 - size / 2, component.bounds.Center.Y - size / 2, size, size), color);
				}
				kvp.Value?.drawInMenu(b, new Vector2(this.scrollView.Left + this.scrollView.Width - 128, component.bounds.Y), 1f);
			}
		}
		b.End();
		b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		this.okButton.draw(b);
		base.drawMouse(b);
		if (this.hoveredItem != null)
		{
			Utility.drawTextWithShadow(b, this.hoverText, Game1.smallFont, new Vector2(base.xPositionOnScreen + IClickableMenu.borderWidth, base.yPositionOnScreen + base.height - 64), Color.Black);
			if (!Game1.oldKBState.IsKeyDown(Keys.LeftShift))
			{
				IClickableMenu.drawToolTip(b, this.hoveredItem.getDescription(), this.hoveredItem.DisplayName, this.hoveredItem);
			}
		}
	}
}
