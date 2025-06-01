using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Characters;

namespace StardewValley.Menus;

public class AnimalPage : IClickableMenu
{
	/// <summary>An entry on the social page.</summary>
	public class AnimalEntry
	{
		/// <summary>The character instance.</summary>
		public Character Animal;

		/// <summary>The unique multiplayer ID for a player, or the internal name for an NPC.</summary>
		public readonly string InternalName;

		/// <summary>The translated display name.</summary>
		public readonly string DisplayName;

		public readonly string AnimalType;

		public readonly string AnimalBaseType;

		/// <summary>The current player's heart level with this animal. -1 means friendship is not tracked.</summary>
		public readonly int FriendshipLevel = -1;

		public readonly bool ReceivedAnimalCracker;

		/// <summary>
		/// 0 is no, 1 is auto-pet, 2 is hand pet
		/// </summary>
		public readonly int WasPetYet;

		public readonly int special;

		public Texture2D Texture;

		public Rectangle TextureSourceRect;

		/// <summary>Construct an instance.</summary>
		/// <param name="player">The player for which to create an entry.</param>
		/// <param name="friendship">The current player's friendship with this character.</param>
		public AnimalEntry(Character animal)
		{
			this.Animal = animal;
			this.DisplayName = animal.displayName;
			if (!(animal is FarmAnimal farmAnimal))
			{
				if (!(animal is Pet pet))
				{
					if (animal is Horse horse)
					{
						this.InternalName = horse.HorseId.ToString();
						this.Texture = horse.Sprite.Texture;
						this.TextureSourceRect = new Rectangle(0, horse.Sprite.SourceRect.Height * 2 - 26, horse.Sprite.SourceRect.Width, 24);
						this.AnimalType = "Horse";
						this.WasPetYet = -1;
						this.special = (horse.ateCarrotToday ? 1 : 0);
					}
				}
				else
				{
					this.InternalName = pet.petId?.ToString() ?? "";
					this.FriendshipLevel = pet.friendshipTowardFarmer.Value;
					this.Texture = pet.Sprite.Texture;
					this.TextureSourceRect = new Rectangle(0, pet.Sprite.SourceRect.Height * 2 - 24, pet.Sprite.SourceRect.Width, 24);
					this.AnimalType = pet.petType.Value;
					this.WasPetYet = (pet.grantedFriendshipForPet.Value ? 2 : 0);
				}
				return;
			}
			this.InternalName = farmAnimal.myID?.ToString() ?? "";
			this.FriendshipLevel = farmAnimal.friendshipTowardFarmer.Value;
			this.Texture = farmAnimal.Sprite.Texture;
			if (farmAnimal.Sprite.SourceRect.Height > 16)
			{
				if (farmAnimal.type.Equals("Ostrich"))
				{
					this.TextureSourceRect = new Rectangle(0, farmAnimal.Sprite.SourceRect.Height * 2 - 32, farmAnimal.Sprite.SourceRect.Width, 28);
				}
				else
				{
					this.TextureSourceRect = new Rectangle(0, farmAnimal.Sprite.SourceRect.Height * 2 - 28, farmAnimal.Sprite.SourceRect.Width, 28);
				}
			}
			else
			{
				this.TextureSourceRect = new Rectangle(0, 16, 16, 16);
			}
			this.AnimalType = farmAnimal.type.Value;
			if (this.AnimalType.Contains(' '))
			{
				this.AnimalBaseType = this.AnimalType.Split(' ')[1];
			}
			else
			{
				this.AnimalBaseType = this.AnimalType;
			}
			this.WasPetYet = (farmAnimal.wasPet.Value ? 2 : (farmAnimal.wasAutoPet.Value ? 1 : 0));
			this.ReceivedAnimalCracker = farmAnimal.hasEatenAnimalCracker.Value;
		}
	}

	public const int slotsOnPage = 5;

	public string hoverText = "";

	public ClickableTextureComponent upButton;

	public ClickableTextureComponent downButton;

	public ClickableTextureComponent scrollBar;

	public Rectangle scrollBarRunner;

	/// <summary>The players and social NPCs shown in the list.</summary>
	public List<AnimalEntry> AnimalEntries;

	/// <summary>The character portrait components.</summary>
	public readonly List<ClickableTextureComponent> sprites = new List<ClickableTextureComponent>();

	/// <summary>The index of the <see cref="F:StardewValley.Menus.AnimalPage.AnimalEntries" /> entry shown at the top of the scrolled view.</summary>
	public int slotPosition;

	/// <summary>The clickable slots over which character info is drawn.</summary>
	public readonly List<ClickableTextureComponent> characterSlots = new List<ClickableTextureComponent>();

	public bool scrolling;

	public AnimalPage(int x, int y, int width, int height)
		: base(x, y, width, height)
	{
	}

	public void init()
	{
		this.AnimalEntries = this.FindAnimals();
		this.CreateComponents();
		this.slotPosition = 0;
		this.setScrollBarToCurrentIndex();
		this.updateSlots();
	}

	public override void populateClickableComponentList()
	{
		this.init();
		base.populateClickableComponentList();
	}

	/// <summary>Find all social NPCs which should be shown on the social page.</summary>
	public List<AnimalEntry> FindAnimals()
	{
		List<AnimalEntry> pets = new List<AnimalEntry>();
		List<AnimalEntry> farmAnimals = new List<AnimalEntry>();
		List<AnimalEntry> horses = new List<AnimalEntry>();
		foreach (Character animal in this.GetAllAnimals())
		{
			if (!(animal is Pet))
			{
				if (animal is Horse)
				{
					horses.Add(new AnimalEntry(animal));
				}
				else
				{
					farmAnimals.Add(new AnimalEntry(animal));
				}
			}
			else
			{
				pets.Add(new AnimalEntry(animal));
			}
		}
		foreach (Farmer f in Game1.getAllFarmers())
		{
			if (f.mount != null)
			{
				horses.Add(new AnimalEntry(f.mount));
			}
		}
		List<AnimalEntry> list = new List<AnimalEntry>();
		list.AddRange(pets);
		list.AddRange(horses);
		list.AddRange(from entry in farmAnimals
			orderby entry.AnimalBaseType, entry.AnimalType, entry.FriendshipLevel descending
			select entry);
		return list;
	}

	/// <summary>Get all animals from the world and friendship data.</summary>
	public IEnumerable<Character> GetAllAnimals()
	{
		List<Character> animals = new List<Character>();
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			foreach (NPC current in location.characters)
			{
				if ((current is Pet || current is Horse) && !current.hideFromAnimalSocialMenu.Value)
				{
					animals.Add(current);
				}
			}
			foreach (FarmAnimal current2 in location.animals.Values)
			{
				if (!current2.hideFromAnimalSocialMenu.Value)
				{
					animals.Add(current2);
				}
			}
			return true;
		});
		return animals;
	}

	/// <summary>Load the clickable components to display.</summary>
	public void CreateComponents()
	{
		this.sprites.Clear();
		this.characterSlots.Clear();
		for (int i = 0; i < this.AnimalEntries.Count; i++)
		{
			this.sprites.Add(this.CreateSpriteComponent(this.AnimalEntries[i], i));
			ClickableTextureComponent slot = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth, 0, base.width - IClickableMenu.borderWidth * 2, this.rowPosition(1) - this.rowPosition(0)), null, new Rectangle(0, 0, 0, 0), 4f)
			{
				myID = i,
				downNeighborID = i + 1,
				upNeighborID = i - 1
			};
			if (slot.upNeighborID < 0)
			{
				slot.upNeighborID = 12342;
			}
			this.characterSlots.Add(slot);
		}
		this.upButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 16, base.yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
		this.downButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 16, base.yPositionOnScreen + base.height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
		this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upButton.bounds.X + 12, this.upButton.bounds.Y + this.upButton.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
		this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upButton.bounds.Y + this.upButton.bounds.Height + 4, this.scrollBar.bounds.Width, base.height - 128 - this.upButton.bounds.Height - 8);
	}

	/// <summary>Create the clickable texture component for a character's portrait.</summary>
	/// <param name="entry">The social character to render.</param>
	/// <param name="index">The index in the list of entries.</param>
	public ClickableTextureComponent CreateSpriteComponent(AnimalEntry entry, int index)
	{
		Rectangle bounds = new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth + 4, 0, base.width, 64);
		Rectangle sourceRect = entry.TextureSourceRect;
		if (sourceRect.Height <= 16)
		{
			bounds.Height--;
			bounds.X += 24;
		}
		return new ClickableTextureComponent(index.ToString(), bounds, null, "", entry.Texture, sourceRect, 4f);
	}

	/// <summary>Get the social entry from its index in the list.</summary>
	/// <param name="index">The index in the social list.</param>
	public AnimalEntry GetSocialEntry(int index)
	{
		if (index < 0 || index >= this.AnimalEntries.Count)
		{
			index = 0;
		}
		if (this.AnimalEntries.Count == 0)
		{
			return null;
		}
		return this.AnimalEntries[index];
	}

	public override void snapToDefaultClickableComponent()
	{
		if (this.slotPosition < this.characterSlots.Count)
		{
			base.currentlySnappedComponent = this.characterSlots[this.slotPosition];
		}
		this.snapCursorToCurrentSnappedComponent();
	}

	public void updateSlots()
	{
		for (int i = 0; i < this.characterSlots.Count; i++)
		{
			this.characterSlots[i].bounds.Y = this.rowPosition(i - 1);
		}
		int index = 0;
		for (int j = this.slotPosition; j < this.slotPosition + 5; j++)
		{
			if (this.slotPosition >= 0 && this.sprites.Count > j)
			{
				int y = base.yPositionOnScreen + IClickableMenu.borderWidth + 32 + 112 * index + 16;
				if (this.sprites[j].bounds.Height < 64)
				{
					y += 48;
				}
				this.sprites[j].bounds.Y = y;
			}
			index++;
		}
		base.populateClickableComponentList();
		this.addTabsToClickableComponents();
	}

	public void addTabsToClickableComponents()
	{
		if (Game1.activeClickableMenu is GameMenu gameMenu && !base.allClickableComponents.Contains(gameMenu.tabs[0]))
		{
			base.allClickableComponents.AddRange(gameMenu.tabs);
		}
	}

	protected void _SelectSlot(AnimalEntry entry)
	{
		bool found = false;
		for (int i = 0; i < this.AnimalEntries.Count; i++)
		{
			if (this.AnimalEntries[i].InternalName == entry.InternalName)
			{
				this._SelectSlot(this.characterSlots[i]);
				found = true;
				break;
			}
		}
		if (!found)
		{
			this._SelectSlot(this.characterSlots[0]);
		}
	}

	protected void _SelectSlot(ClickableComponent slot_component)
	{
		if (slot_component != null && this.characterSlots.Contains(slot_component))
		{
			int index = this.characterSlots.IndexOf(slot_component as ClickableTextureComponent);
			base.currentlySnappedComponent = slot_component;
			if (index < this.slotPosition)
			{
				this.slotPosition = index;
			}
			else if (index >= this.slotPosition + 5)
			{
				this.slotPosition = index - 5 + 1;
			}
			this.setScrollBarToCurrentIndex();
			this.updateSlots();
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				this.snapCursorToCurrentSnappedComponent();
			}
		}
	}

	public void ConstrainSelectionToVisibleSlots()
	{
		if (this.characterSlots.Contains(base.currentlySnappedComponent))
		{
			int index = this.characterSlots.IndexOf(base.currentlySnappedComponent as ClickableTextureComponent);
			if (index < this.slotPosition)
			{
				index = this.slotPosition;
			}
			else if (index >= this.slotPosition + 5)
			{
				index = this.slotPosition + 5 - 1;
			}
			base.currentlySnappedComponent = this.characterSlots[index];
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				this.snapCursorToCurrentSnappedComponent();
			}
		}
	}

	public override void snapCursorToCurrentSnappedComponent()
	{
		if (base.currentlySnappedComponent != null && this.characterSlots.Contains(base.currentlySnappedComponent))
		{
			Game1.setMousePosition(base.currentlySnappedComponent.bounds.Left + 64, base.currentlySnappedComponent.bounds.Center.Y);
		}
		else
		{
			base.snapCursorToCurrentSnappedComponent();
		}
	}

	public override void applyMovementKey(int direction)
	{
		base.applyMovementKey(direction);
		if (this.characterSlots.Contains(base.currentlySnappedComponent))
		{
			this._SelectSlot(base.currentlySnappedComponent);
		}
	}

	/// <inheritdoc />
	public override void leftClickHeld(int x, int y)
	{
		base.leftClickHeld(x, y);
		if (this.scrolling)
		{
			int y2 = this.scrollBar.bounds.Y;
			this.scrollBar.bounds.Y = Math.Min(base.yPositionOnScreen + base.height - 64 - 12 - this.scrollBar.bounds.Height, Math.Max(y, base.yPositionOnScreen + this.upButton.bounds.Height + 20));
			float percentage = (float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height;
			this.slotPosition = Math.Min(this.sprites.Count - 5, Math.Max(0, (int)((float)this.sprites.Count * percentage)));
			this.setScrollBarToCurrentIndex();
			if (y2 != this.scrollBar.bounds.Y)
			{
				Game1.playSound("shiny4");
			}
		}
	}

	/// <inheritdoc />
	public override void releaseLeftClick(int x, int y)
	{
		base.releaseLeftClick(x, y);
		this.scrolling = false;
	}

	private void setScrollBarToCurrentIndex()
	{
		if (this.sprites.Count > 0)
		{
			this.scrollBar.bounds.Y = this.scrollBarRunner.Height / Math.Max(1, this.sprites.Count - 5 + 1) * this.slotPosition + this.upButton.bounds.Bottom + 4;
			if (this.slotPosition == this.sprites.Count - 5)
			{
				this.scrollBar.bounds.Y = this.downButton.bounds.Y - this.scrollBar.bounds.Height - 4;
			}
		}
		this.updateSlots();
	}

	/// <inheritdoc />
	public override void receiveScrollWheelAction(int direction)
	{
		base.receiveScrollWheelAction(direction);
		if (direction > 0 && this.slotPosition > 0)
		{
			this.upArrowPressed();
			this.ConstrainSelectionToVisibleSlots();
			Game1.playSound("shiny4");
		}
		else if (direction < 0 && this.slotPosition < Math.Max(0, this.sprites.Count - 5))
		{
			this.downArrowPressed();
			this.ConstrainSelectionToVisibleSlots();
			Game1.playSound("shiny4");
		}
	}

	public void upArrowPressed()
	{
		this.slotPosition--;
		this.updateSlots();
		this.upButton.scale = 3.5f;
		this.setScrollBarToCurrentIndex();
	}

	public void downArrowPressed()
	{
		this.slotPosition++;
		this.updateSlots();
		this.downButton.scale = 3.5f;
		this.setScrollBarToCurrentIndex();
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.upButton.containsPoint(x, y) && this.slotPosition > 0)
		{
			this.upArrowPressed();
			Game1.playSound("shwip");
			return;
		}
		if (this.downButton.containsPoint(x, y) && this.slotPosition < this.sprites.Count - 5)
		{
			this.downArrowPressed();
			Game1.playSound("shwip");
			return;
		}
		if (this.scrollBar.containsPoint(x, y))
		{
			this.scrolling = true;
			return;
		}
		if (!this.downButton.containsPoint(x, y) && x > base.xPositionOnScreen + base.width && x < base.xPositionOnScreen + base.width + 128 && y > base.yPositionOnScreen && y < base.yPositionOnScreen + base.height)
		{
			this.scrolling = true;
			this.leftClickHeld(x, y);
			this.releaseLeftClick(x, y);
			return;
		}
		for (int i = 0; i < this.characterSlots.Count; i++)
		{
			if (i >= this.slotPosition)
			{
				_ = this.slotPosition + 5;
			}
		}
		this.slotPosition = Math.Max(0, Math.Min(this.sprites.Count - 5, this.slotPosition));
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.hoverText = "";
		this.upButton.tryHover(x, y);
		this.downButton.tryHover(x, y);
	}

	private bool isCharacterSlotClickable(int i)
	{
		this.GetSocialEntry(i);
		return false;
	}

	private void drawNPCSlot(SpriteBatch b, int i)
	{
		AnimalEntry entry = this.GetSocialEntry(i);
		if (entry == null || i < 0)
		{
			return;
		}
		if (this.isCharacterSlotClickable(i) && this.characterSlots[i].bounds.Contains(Game1.getMouseX(), Game1.getMouseY()))
		{
			b.Draw(Game1.staminaRect, new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth - 4, this.sprites[i].bounds.Y - 4, this.characterSlots[i].bounds.Width, this.characterSlots[i].bounds.Height - 12), Color.White * 0.25f);
		}
		this.sprites[i].draw(b);
		_ = entry.InternalName;
		_ = entry.FriendshipLevel;
		float lineHeight = Game1.smallFont.MeasureString("W").Y;
		float russianOffsetY = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? ((0f - lineHeight) / 2f) : 0f);
		int yOffset = ((entry.TextureSourceRect.Height <= 16) ? (-40) : 8);
		b.DrawString(Game1.dialogueFont, entry.DisplayName, new Vector2(base.xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 192 - 20 + 96 - (int)(Game1.dialogueFont.MeasureString(entry.DisplayName).X / 2f), (float)(this.sprites[i].bounds.Y + 48 + yOffset) + russianOffsetY - 20f), Game1.textColor);
		if (entry.FriendshipLevel != -1)
		{
			double loveLevel = (float)entry.FriendshipLevel / 1000f;
			int halfHeart = (int)((loveLevel * 1000.0 % 200.0 >= 100.0) ? (loveLevel * 1000.0 / 200.0) : (-100.0));
			int heartYOffset = (entry.ReceivedAnimalCracker ? (-24) : 0);
			for (int hearts = 0; hearts < 5; hearts++)
			{
				b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + 512 - 4 + hearts * 32, this.sprites[i].bounds.Y + heartYOffset + yOffset + 64 - 24), new Rectangle(211 + ((loveLevel * 1000.0 <= (double)((hearts + 1) * 195)) ? 7 : 0), 428, 7, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
				if (halfHeart == hearts)
				{
					b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + 512 - 4 + hearts * 32, this.sprites[i].bounds.Y + heartYOffset + yOffset + 64 - 24), new Rectangle(211, 428, 4, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.891f);
				}
			}
		}
		if (entry.WasPetYet != -1)
		{
			b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + 704 - 4, this.sprites[i].bounds.Y + yOffset + 64 - 52), new Rectangle(32, 0, 10, 10), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
			b.Draw(Game1.mouseCursors_1_6, new Vector2(base.xPositionOnScreen + 704 - 4, this.sprites[i].bounds.Y + yOffset + 64 - 8), new Rectangle(273 + entry.WasPetYet * 9, 253, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
		}
		if (entry.special == 1)
		{
			Utility.drawWithShadow(b, Game1.objectSpriteSheet_2, new Vector2(base.xPositionOnScreen + 704 - 16, this.sprites[i].bounds.Y + yOffset + 64 - 52), new Rectangle(0, 160, 16, 16), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.8f, 0, 8);
		}
		if (entry.ReceivedAnimalCracker)
		{
			Utility.drawWithShadow(b, Game1.objectSpriteSheet_2, new Vector2(base.xPositionOnScreen + 576 - 20, this.sprites[i].bounds.Y + yOffset + 64 - 16), new Rectangle(16, 242, 15, 11), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.8f);
		}
	}

	private int rowPosition(int i)
	{
		int j = i - this.slotPosition;
		int rowHeight = 112;
		return base.yPositionOnScreen + IClickableMenu.borderWidth + 160 + 4 + j * rowHeight;
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		b.End();
		b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
		if (this.sprites.Count > 0)
		{
			base.drawHorizontalPartition(b, base.yPositionOnScreen + IClickableMenu.borderWidth + 128 + 4, small: true);
		}
		if (this.sprites.Count > 1)
		{
			base.drawHorizontalPartition(b, base.yPositionOnScreen + IClickableMenu.borderWidth + 192 + 32 + 20, small: true);
		}
		if (this.sprites.Count > 2)
		{
			base.drawHorizontalPartition(b, base.yPositionOnScreen + IClickableMenu.borderWidth + 320 + 36, small: true);
		}
		if (this.sprites.Count > 3)
		{
			base.drawHorizontalPartition(b, base.yPositionOnScreen + IClickableMenu.borderWidth + 384 + 32 + 52, small: true);
		}
		for (int i = this.slotPosition; i < this.slotPosition + 5 && i < this.sprites.Count; i++)
		{
			if (this.GetSocialEntry(i) != null)
			{
				this.drawNPCSlot(b, i);
			}
		}
		Rectangle newClip = b.GraphicsDevice.ScissorRectangle;
		newClip.Y = Math.Max(0, this.rowPosition(4 - this.sprites.Count));
		newClip.Height -= newClip.Y;
		if (newClip.Height > 0)
		{
			int heightOverride = ((this.sprites.Count >= 5) ? (-1) : ((108 + this.sprites.Count) * this.sprites.Count));
			base.drawVerticalPartition(b, base.xPositionOnScreen + 448 + 12, small: true, -1, -1, -1, heightOverride);
			base.drawVerticalPartition(b, base.xPositionOnScreen + 256 + 12 + 376, small: true, -1, -1, -1, heightOverride);
		}
		this.upButton.draw(b);
		this.downButton.draw(b);
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f);
		this.scrollBar.draw(b);
		if (!this.hoverText.Equals(""))
		{
			IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont);
		}
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
	}
}
