using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;

namespace StardewValley.Menus;

public class FieldOfficeMenu : MenuWithInventory
{
	private Texture2D fieldOfficeMenuTexture;

	private IslandFieldOffice office;

	private bool madeADonation;

	private bool gotReward;

	public List<ClickableComponent> pieceHolders = new List<ClickableComponent>();

	private float bearTimer;

	private float snakeTimer;

	private float batTimer;

	private float frogTimer;

	public FieldOfficeMenu(IslandFieldOffice office)
		: base(highlightBones, okButton: true, trashCan: true, 16, 132)
	{
		FieldOfficeMenu fieldOfficeMenu = this;
		this.office = office;
		this.fieldOfficeMenuTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\FieldOfficeDonationMenu");
		Point topLeft = new Point(base.xPositionOnScreen + 32, base.yPositionOnScreen + 96);
		this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 76, topLeft.Y + 180, 64, 64), office.piecesDonated[0] ? ItemRegistry.Create("(O)823") : null)
		{
			label = "823"
		});
		this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 144, topLeft.Y + 180, 64, 64), office.piecesDonated[1] ? ItemRegistry.Create("(O)824") : null)
		{
			label = "824"
		});
		this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 212, topLeft.Y + 180, 64, 64), office.piecesDonated[2] ? ItemRegistry.Create("(O)823") : null)
		{
			label = "823"
		});
		this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 76, topLeft.Y + 112, 64, 64), office.piecesDonated[3] ? ItemRegistry.Create("(O)822") : null)
		{
			label = "822"
		});
		this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 144, topLeft.Y + 112, 64, 64), office.piecesDonated[4] ? ItemRegistry.Create("(O)821") : null)
		{
			label = "821"
		});
		this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 212, topLeft.Y + 112, 64, 64), office.piecesDonated[5] ? ItemRegistry.Create("(O)820") : null)
		{
			label = "820"
		});
		this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 412, topLeft.Y + 48, 64, 64), office.piecesDonated[6] ? ItemRegistry.Create("(O)826") : null)
		{
			label = "826"
		});
		this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 412, topLeft.Y + 128, 64, 64), office.piecesDonated[7] ? ItemRegistry.Create("(O)826") : null)
		{
			label = "826"
		});
		this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 412, topLeft.Y + 208, 64, 64), office.piecesDonated[8] ? ItemRegistry.Create("(O)825") : null)
		{
			label = "825"
		});
		this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 616, topLeft.Y + 36, 64, 64), office.piecesDonated[9] ? ItemRegistry.Create("(O)827") : null)
		{
			label = "827"
		});
		this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 624, topLeft.Y + 156, 64, 64), office.piecesDonated[10] ? ItemRegistry.Create("(O)828") : null)
		{
			label = "828"
		});
		if (Game1.activeClickableMenu == null)
		{
			Game1.playSound("bigSelect");
		}
		for (int i = 0; i < this.pieceHolders.Count; i++)
		{
			ClickableComponent clickableComponent = this.pieceHolders[i];
			clickableComponent.upNeighborID = (clickableComponent.downNeighborID = (clickableComponent.rightNeighborID = (clickableComponent.leftNeighborID = -99998)));
			clickableComponent.myID = 1000 + i;
		}
		foreach (ClickableComponent item in base.inventory.GetBorder(InventoryMenu.BorderSide.Top))
		{
			item.upNeighborID = -99998;
		}
		foreach (ClickableComponent item2 in base.inventory.GetBorder(InventoryMenu.BorderSide.Right))
		{
			item2.rightNeighborID = 4857;
			item2.rightNeighborImmutable = true;
		}
		this.populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			this.snapToDefaultClickableComponent();
		}
		base.trashCan.leftNeighborID = (base.okButton.leftNeighborID = 11);
		base.exitFunction = delegate
		{
			if (fieldOfficeMenu.madeADonation)
			{
				string text = "Strings\\Locations:FieldOfficeDonated_" + Game1.random.Next(4);
				string text2 = Game1.content.LoadString(text);
				if (fieldOfficeMenu.gotReward)
				{
					text2 = text2 + "#$b#" + Game1.content.LoadString("Strings\\Locations:FieldOfficeDonated_Reward");
				}
				Game1.DrawDialogue(new Dialogue(office.getSafariGuy(), text, text2));
				if (fieldOfficeMenu.gotReward)
				{
					Game1.multiplayer.globalChatInfoMessage("FieldOfficeCompleteSet", Game1.player.Name);
				}
			}
		};
	}

	public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
	{
		if (b.myID == 5948 && b.myID != 4857)
		{
			return false;
		}
		return base.IsAutomaticSnapValid(direction, a, b);
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(0);
		this.snapCursorToCurrentSnappedComponent();
	}

	public static bool highlightBones(Item i)
	{
		if (i != null)
		{
			IslandFieldOffice office = Game1.RequireLocation<IslandFieldOffice>("IslandFieldOffice");
			switch (i.QualifiedItemId)
			{
			case "(O)820":
				if (!office.piecesDonated[5])
				{
					return true;
				}
				break;
			case "(O)821":
				if (!office.piecesDonated[4])
				{
					return true;
				}
				break;
			case "(O)822":
				if (!office.piecesDonated[3])
				{
					return true;
				}
				break;
			case "(O)823":
				if (!office.piecesDonated[0] || !office.piecesDonated[2])
				{
					return true;
				}
				break;
			case "(O)824":
				if (!office.piecesDonated[1])
				{
					return true;
				}
				break;
			case "(O)825":
				if (!office.piecesDonated[8])
				{
					return true;
				}
				break;
			case "(O)826":
				if (!office.piecesDonated[7] || !office.piecesDonated[6])
				{
					return true;
				}
				break;
			case "(O)827":
				if (!office.piecesDonated[9])
				{
					return true;
				}
				break;
			case "(O)828":
				if (!office.piecesDonated[10])
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	public static int getPieceIndexForDonationItem(string qualifiedItemId)
	{
		return qualifiedItemId switch
		{
			"(O)820" => 5, 
			"(O)821" => 4, 
			"(O)822" => 3, 
			"(O)823" => 0, 
			"(O)824" => 1, 
			"(O)825" => 8, 
			"(O)826" => 7, 
			"(O)827" => 9, 
			"(O)828" => 10, 
			_ => -1, 
		};
	}

	public static int getDonationPieceIndexNeededForSpot(int donationSpotIndex)
	{
		switch (donationSpotIndex)
		{
		case 5:
			return 820;
		case 4:
			return 821;
		case 3:
			return 822;
		case 0:
		case 2:
			return 823;
		case 1:
			return 824;
		case 8:
			return 825;
		case 6:
		case 7:
			return 826;
		case 9:
			return 827;
		case 10:
			return 828;
		default:
			return -1;
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		base.receiveLeftClick(x, y, playSound);
		if (base.heldItem == null)
		{
			return;
		}
		int index = FieldOfficeMenu.getPieceIndexForDonationItem(base.heldItem.QualifiedItemId);
		if (index == -1)
		{
			return;
		}
		string qualifiedItemId = base.heldItem.QualifiedItemId;
		if (!(qualifiedItemId == "(O)823"))
		{
			if (qualifiedItemId == "(O)826")
			{
				if (!this.donate(7, x, y))
				{
					this.donate(6, x, y);
				}
			}
			else
			{
				this.donate(index, x, y);
			}
		}
		else if (!this.donate(0, x, y))
		{
			this.donate(2, x, y);
		}
	}

	/// <inheritdoc />
	protected override void cleanupBeforeExit()
	{
		base.cleanupBeforeExit();
		if (this.office != null && this.office.isRangeAllTrue(0, 11) && this.office.plantsRestoredRight.Value && this.office.plantsRestoredLeft.Value && !Game1.player.hasOrWillReceiveMail("fieldOfficeFinale"))
		{
			this.office.triggerFinaleCutscene();
		}
	}

	private bool donate(int index, int x, int y)
	{
		if (this.pieceHolders[index].containsPoint(x, y) && this.pieceHolders[index].item == null)
		{
			Item item = base.heldItem;
			base.heldItem = item.ConsumeStack(1);
			this.pieceHolders[index].item = ItemRegistry.Create(item.QualifiedItemId);
			this.checkForSetFinish();
			this.gotReward = this.office.donatePiece(index);
			this.madeADonation = true;
			Game1.playSound("newArtifact");
			Game1.multiplayer.globalChatInfoMessage("FieldOfficeDonation", Game1.player.Name, TokenStringBuilder.ItemNameFor(item));
			return true;
		}
		return false;
	}

	public void checkForSetFinish()
	{
		if (!this.office.centerSkeletonRestored.Value && this.pieceHolders[0].item != null && this.pieceHolders[1].item != null && this.pieceHolders[2].item != null && this.pieceHolders[3].item != null && this.pieceHolders[4].item != null && this.pieceHolders[5].item != null)
		{
			DelayedAction.functionAfterDelay(delegate
			{
				this.bearTimer = 500f;
				Game1.playSound("camel");
			}, 700);
		}
		if (!this.office.snakeRestored.Value && this.pieceHolders[6].item != null && this.pieceHolders[7].item != null && this.pieceHolders[8].item != null)
		{
			DelayedAction.functionAfterDelay(delegate
			{
				this.snakeTimer = 1500f;
				Game1.playSound("steam");
			}, 700);
		}
		if (!this.office.batRestored.Value && this.pieceHolders[9].item != null)
		{
			DelayedAction.functionAfterDelay(delegate
			{
				this.batTimer = 1500f;
				Game1.playSound("batScreech");
			}, 700);
		}
		if (!this.office.frogRestored.Value && this.pieceHolders[10].item != null)
		{
			DelayedAction.functionAfterDelay(delegate
			{
				this.frogTimer = 1000f;
				Game1.playSound("croak");
			}, 700);
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (this.bearTimer > 0f)
		{
			this.bearTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
		}
		if (this.snakeTimer > 0f)
		{
			this.snakeTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
		}
		if (this.batTimer > 0f)
		{
			this.batTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
		}
		if (this.frogTimer > 0f)
		{
			this.frogTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		base.draw(b, drawUpperPortion: true, drawDescriptionArea: false, 0, 80, 80);
		b.Draw(this.fieldOfficeMenuTexture, new Vector2(base.xPositionOnScreen + 32, base.yPositionOnScreen + 96), new Rectangle(0, 0, 204, 80), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
		b.Draw(this.fieldOfficeMenuTexture, new Vector2(base.xPositionOnScreen + base.width - 160, (float)(base.yPositionOnScreen + 108) + ((this.batTimer > 0f) ? ((float)Math.Sin((1500f - this.batTimer) / 80f) * 64f / 4f) : 0f)), new Rectangle(68, 84, 30, 20), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
		foreach (ClickableComponent c in this.pieceHolders)
		{
			c.item?.drawInMenu(b, Utility.PointToVector2(c.bounds.Location), 1f);
		}
		if (this.bearTimer > 0f)
		{
			b.Draw(this.fieldOfficeMenuTexture, new Vector2(base.xPositionOnScreen + 32 + 240, base.yPositionOnScreen + 96 + 36), new Rectangle(0, 81, 37, 29), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
		}
		else if (this.snakeTimer > 0f && this.snakeTimer / 300f % 2f != 0f)
		{
			b.Draw(this.fieldOfficeMenuTexture, new Vector2(base.xPositionOnScreen + 32 + 484, base.yPositionOnScreen + 96 + 232), new Rectangle(47, 84, 19, 19), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
		}
		else if (this.frogTimer > 0f)
		{
			b.Draw(this.fieldOfficeMenuTexture, new Vector2(base.xPositionOnScreen + 32 + 708, base.yPositionOnScreen + 96 + 140), new Rectangle(100, 89, 18, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
		}
		if (base.heldItem != null)
		{
			int highlight = FieldOfficeMenu.getPieceIndexForDonationItem(base.heldItem.QualifiedItemId);
			if (highlight != -1)
			{
				this.drawHighlightedSquare(highlight, b);
			}
		}
		base.drawMouse(b);
		base.heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
	}

	private void drawHighlightedSquare(int index, SpriteBatch b)
	{
		Rectangle source = default(Rectangle);
		switch (base.heldItem.QualifiedItemId)
		{
		case "(O)820":
		case "(O)821":
		case "(O)822":
		case "(O)823":
		case "(O)824":
			source = new Rectangle(119, 86, 18, 18);
			break;
		case "(O)825":
		case "(O)826":
			source = new Rectangle(138, 86, 18, 18);
			break;
		case "(O)827":
			source = new Rectangle(157, 86, 18, 18);
			break;
		case "(O)828":
			source = new Rectangle(176, 86, 18, 18);
			break;
		}
		if (this.pieceHolders[index].item == null)
		{
			b.Draw(this.fieldOfficeMenuTexture, Utility.PointToVector2(this.pieceHolders[index].bounds.Location) + new Vector2(-1f, -1f) * 4f, source, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
		}
		string qualifiedItemId = base.heldItem.QualifiedItemId;
		if (!(qualifiedItemId == "(O)823"))
		{
			if (qualifiedItemId == "(O)826" && index == 7 && this.pieceHolders[6].item == null)
			{
				b.Draw(this.fieldOfficeMenuTexture, Utility.PointToVector2(this.pieceHolders[6].bounds.Location) + new Vector2(-1f, -1f) * 4f, source, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			}
		}
		else if (index == 0 && this.pieceHolders[2].item == null)
		{
			b.Draw(this.fieldOfficeMenuTexture, Utility.PointToVector2(this.pieceHolders[2].bounds.Location) + new Vector2(-1f, -1f) * 4f, source, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
		}
	}
}
