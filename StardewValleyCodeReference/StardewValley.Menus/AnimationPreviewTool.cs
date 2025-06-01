using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus;

public class AnimationPreviewTool : IClickableMenu
{
	public List<List<ClickableTextureComponent>> components;

	public Rectangle scrollView;

	public List<ClickableTextureComponent> animationButtons;

	public ClickableTextureComponent okButton;

	public ClickableTextureComponent hairLabel;

	public ClickableTextureComponent shirtLabel;

	public ClickableTextureComponent pantsLabel;

	public float scrollY;

	public AnimationPreviewTool()
		: base(Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64, 632 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2 + 64)
	{
		Game1.player.faceDirection(2);
		Game1.player.FarmerSprite.StopAnimation();
		FieldInfo[] fields = typeof(FarmerSprite).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		this.animationButtons = new List<ClickableTextureComponent>();
		foreach (FieldInfo field in fields.Where((FieldInfo fi) => fi.IsLiteral && !fi.IsInitOnly))
		{
			ClickableTextureComponent component = new ClickableTextureComponent(new Rectangle(0, 0, 200, 48), null, default(Rectangle), 1f)
			{
				myID = (int)field.GetValue(null),
				name = field.Name
			};
			this.animationButtons.Add(component);
		}
		this.okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		};
		this.components = new List<List<ClickableTextureComponent>>();
		this.components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[1]
		{
			new ClickableTextureComponent("Hair Heading", new Rectangle(0, 0, 64, 16), "Hair", "", null, default(Rectangle), 1f)
		}));
		this.hairLabel = new ClickableTextureComponent("Hair Label", new Rectangle(0, 0, 64, 64), "0", "", null, default(Rectangle), 1f);
		this.components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[3]
		{
			new ClickableTextureComponent("Hair Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = -1
			},
			this.hairLabel,
			new ClickableTextureComponent("Hair Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 1
			}
		}));
		this.components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[1]
		{
			new ClickableTextureComponent("Shirt Heading", new Rectangle(0, 0, 64, 16), "Shirt", "", null, default(Rectangle), 1f)
		}));
		this.shirtLabel = new ClickableTextureComponent("Shirt Label", new Rectangle(0, 0, 64, 64), "0", "", null, default(Rectangle), 1f);
		this.components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[3]
		{
			new ClickableTextureComponent("Shirt Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = -1
			},
			this.shirtLabel,
			new ClickableTextureComponent("Shirt Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 1
			}
		}));
		this.components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[1]
		{
			new ClickableTextureComponent("Pants Heading", new Rectangle(0, 0, 64, 16), "Pants", "", null, default(Rectangle), 1f)
		}));
		this.pantsLabel = new ClickableTextureComponent("Pants Label", new Rectangle(0, 0, 64, 64), "0", "", null, default(Rectangle), 1f);
		this.components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[3]
		{
			new ClickableTextureComponent("Pants Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = -1
			},
			this.pantsLabel,
			new ClickableTextureComponent("Pants Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 1
			}
		}));
		this.components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[1]
		{
			new ClickableTextureComponent("Toggle Gender", new Rectangle(0, 0, 64, 64), "Toggle Gender", "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 25), 1f)
		}));
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

	public void SwitchShirt(int direction)
	{
		Game1.player.rotateShirt(direction);
		this.UpdateLabels();
	}

	public void SwitchHair(int direction)
	{
		Game1.player.changeHairStyle(Game1.player.hair.Value + direction);
		this.UpdateLabels();
	}

	public void SwitchPants(int direction)
	{
		Game1.player.rotatePantStyle(direction);
		this.UpdateLabels();
	}

	private void RepositionElements()
	{
		this.scrollView = new Rectangle(base.xPositionOnScreen + 320, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder, 250, 500);
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
		int component_y = base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 200;
		foreach (List<ClickableTextureComponent> component2 in this.components)
		{
			int component_x = base.xPositionOnScreen + 70;
			int max_height = 0;
			foreach (ClickableTextureComponent component in component2)
			{
				component.bounds.X = component_x;
				component.bounds.Y = component_y;
				component_x += component.bounds.Width + 8;
				max_height = Math.Max(component.bounds.Height, max_height);
			}
			component_y += max_height + 8;
		}
		this.RepositionScrollElements();
		this.UpdateLabels();
	}

	public void UpdateLabels()
	{
		this.pantsLabel.label = Game1.player.GetPantsIndex().ToString() ?? "";
		this.shirtLabel.label = Game1.player.GetShirtIndex().ToString() ?? "";
		this.hairLabel.label = Game1.player.getHair().ToString() ?? "";
	}

	public void RepositionScrollElements()
	{
		int y_offset = (int)this.scrollY;
		if (this.scrollY > 0f)
		{
			this.scrollY = 0f;
		}
		foreach (ClickableTextureComponent component in this.animationButtons)
		{
			component.bounds.X = this.scrollView.X;
			component.bounds.Y = this.scrollView.Y + y_offset;
			component.bounds.Width = this.scrollView.Width;
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
		foreach (ClickableTextureComponent component in this.animationButtons)
		{
			if (component.bounds.Contains(x, y) && this.scrollView.Contains(x, y))
			{
				if (component.name.Contains("Left"))
				{
					Game1.player.faceDirection(3);
				}
				else if (component.name.Contains("Right"))
				{
					Game1.player.faceDirection(1);
				}
				else if (component.name.Contains("Up"))
				{
					Game1.player.faceDirection(0);
				}
				else
				{
					Game1.player.faceDirection(2);
				}
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.player.animateOnce(component.myID);
			}
		}
		foreach (List<ClickableTextureComponent> component3 in this.components)
		{
			foreach (ClickableTextureComponent component2 in component3)
			{
				if (component2.containsPoint(x, y))
				{
					switch (component2.name)
					{
					case "Shirt Style":
						this.SwitchShirt(component2.myID);
						break;
					case "Pants Style":
						this.SwitchPants(component2.myID);
						break;
					case "Hair Style":
						this.SwitchHair(component2.myID);
						break;
					case "Toggle Gender":
						Game1.player.changeGender(!Game1.player.IsMale);
						break;
					}
				}
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
	}

	public bool canLeaveMenu()
	{
		return true;
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, speaker: false, drawOnlyBox: true);
		b.Draw(Game1.daybg, new Vector2(base.xPositionOnScreen + 64 + 42 - 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16), Color.White);
		Game1.player.FarmerRenderer.draw(b, Game1.player.FarmerSprite.CurrentAnimationFrame, Game1.player.FarmerSprite.CurrentFrame, Game1.player.FarmerSprite.SourceRect, new Vector2(base.xPositionOnScreen - 2 + 42 + 128 - 32, base.yPositionOnScreen + IClickableMenu.borderWidth - 16 + IClickableMenu.spaceToClearTopBorder + 32), Vector2.Zero, 0.8f, Color.White, 0f, 1f, Game1.player);
		b.End();
		Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
		b.GraphicsDevice.ScissorRectangle = this.scrollView;
		foreach (ClickableTextureComponent component in this.animationButtons)
		{
			if (component.visible)
			{
				Game1.DrawBox(component.bounds.X, component.bounds.Y, component.bounds.Width, component.bounds.Height);
				Utility.drawTextWithShadow(b, component.name, Game1.smallFont, new Vector2(component.bounds.X, component.bounds.Y), Color.Black);
			}
		}
		b.End();
		b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		foreach (List<ClickableTextureComponent> component2 in this.components)
		{
			foreach (ClickableTextureComponent item in component2)
			{
				item.draw(b);
			}
		}
		this.okButton.draw(b);
		base.drawMouse(b);
	}
}
