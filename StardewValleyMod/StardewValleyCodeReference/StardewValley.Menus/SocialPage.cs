using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Characters;
using StardewValley.GameData.Characters;

namespace StardewValley.Menus;

public class SocialPage : IClickableMenu
{
	/// <summary>An entry on the social page.</summary>
	public class SocialEntry
	{
		/// <summary>The backing field for <see cref="M:StardewValley.Menus.SocialPage.SocialEntry.IsMarriedToAnyone" />.</summary>
		private bool? CachedIsMarriedToAnyone;

		/// <summary>The character instance.</summary>
		public Character Character;

		/// <summary>The unique multiplayer ID for a player, or the internal name for an NPC.</summary>
		public readonly string InternalName;

		/// <summary>The translated display name.</summary>
		public readonly string DisplayName;

		/// <summary>Whether the current player has met this character.</summary>
		public readonly bool IsMet;

		/// <summary>Whether players can romance this character.</summary>
		public readonly bool IsDatable;

		/// <summary>How the NPC is shown on the social tab.</summary>
		public readonly SocialTabBehavior SocialTabBehavior;

		/// <summary>Whether this character is a child.</summary>
		public readonly bool IsChild;

		/// <summary>Whether this character is a player.</summary>
		public readonly bool IsPlayer;

		/// <summary>The character's gender identity.</summary>
		public readonly Gender Gender;

		/// <summary>The current player's heart level with this character.</summary>
		public readonly int HeartLevel;

		/// <summary>The current player's friendship data with the character, if any.</summary>
		public readonly Friendship Friendship;

		/// <summary>The NPC's character data, if applicable.</summary>
		public readonly CharacterData Data;

		/// <summary>The order in which the current player met this NPC, if applicable.</summary>
		public int? OrderMet;

		/// <summary>Construct an instance.</summary>
		/// <param name="player">The player for which to create an entry.</param>
		/// <param name="friendship">The current player's friendship with this character.</param>
		public SocialEntry(Farmer player, Friendship friendship)
		{
			this.Character = player;
			this.InternalName = player.UniqueMultiplayerID.ToString();
			this.DisplayName = player.Name;
			this.IsMet = true;
			this.IsPlayer = true;
			this.Gender = player.Gender;
			this.Friendship = friendship;
		}

		/// <summary>Construct an instance.</summary>
		/// <param name="npc">The NPC for which to create an entry.</param>
		/// <param name="friendship">The current player's friendship with this character.</param>
		/// <param name="data">The NPC's character data, if applicable.</param>
		/// <param name="overrideDisplayName">The translated display name, or <c>null</c> to get it from <paramref name="npc" />.</param>
		public SocialEntry(NPC npc, Friendship friendship, CharacterData data, string overrideDisplayName = null)
		{
			this.Character = npc;
			this.InternalName = npc.Name;
			this.DisplayName = overrideDisplayName ?? npc.displayName;
			this.IsMet = friendship != null || npc is Child;
			this.IsDatable = data?.CanBeRomanced ?? false;
			this.SocialTabBehavior = data?.SocialTab ?? SocialTabBehavior.AlwaysShown;
			this.IsChild = npc is Child;
			this.Gender = npc.Gender;
			this.HeartLevel = (friendship?.Points ?? 0) / 250;
			this.Friendship = friendship;
			this.Data = data;
		}

		/// <summary>Get whether the current player is dating this character.</summary>
		public bool IsDatingCurrentPlayer()
		{
			return this.Friendship?.IsDating() ?? false;
		}

		/// <summary>Get whether the current player is married to this character.</summary>
		public bool IsMarriedToCurrentPlayer()
		{
			return this.Friendship?.IsMarried() ?? false;
		}

		/// <summary>Get whether the current player is a roommate with this character.</summary>
		public bool IsRoommateForCurrentPlayer()
		{
			return this.Friendship?.IsRoommate() ?? false;
		}

		/// <summary>Get whether the current player is married to this character.</summary>
		public bool IsDivorcedFromCurrentPlayer()
		{
			return this.Friendship?.IsDivorced() ?? false;
		}

		/// <summary>Get whether this character is married to any player.</summary>
		public bool IsMarriedToAnyone()
		{
			if (!this.CachedIsMarriedToAnyone.HasValue)
			{
				if (this.IsMarriedToCurrentPlayer())
				{
					this.CachedIsMarriedToAnyone = true;
				}
				else
				{
					foreach (Farmer farmer in Game1.getAllFarmers())
					{
						if (farmer.spouse == this.InternalName && farmer.isMarriedOrRoommates())
						{
							this.CachedIsMarriedToAnyone = true;
							break;
						}
					}
					if (!this.CachedIsMarriedToAnyone.HasValue)
					{
						this.CachedIsMarriedToAnyone = false;
					}
				}
			}
			return this.CachedIsMarriedToAnyone.Value;
		}
	}

	public const int slotsOnPage = 5;

	public string hoverText = "";

	public ClickableTextureComponent upButton;

	public ClickableTextureComponent downButton;

	public ClickableTextureComponent scrollBar;

	public Rectangle scrollBarRunner;

	/// <summary>The players and social NPCs shown in the list.</summary>
	public readonly List<SocialEntry> SocialEntries;

	/// <summary>The character portrait components.</summary>
	public readonly List<ClickableTextureComponent> sprites = new List<ClickableTextureComponent>();

	/// <summary>The index of the <see cref="F:StardewValley.Menus.SocialPage.SocialEntries" /> entry shown at the top of the scrolled view.</summary>
	public int slotPosition;

	/// <summary>The number of players shown in the list.</summary>
	public int numFarmers;

	/// <summary>The clickable slots over which character info is drawn.</summary>
	public readonly List<ClickableTextureComponent> characterSlots = new List<ClickableTextureComponent>();

	public bool scrolling;

	public SocialPage(int x, int y, int width, int height)
		: base(x, y, width, height)
	{
		this.SocialEntries = this.FindSocialCharacters();
		this.numFarmers = this.SocialEntries.Count((SocialEntry p) => p.IsPlayer);
		this.CreateComponents();
		this.slotPosition = 0;
		for (int i = 0; i < this.SocialEntries.Count; i++)
		{
			if (!this.SocialEntries[i].IsPlayer)
			{
				this.slotPosition = i;
				break;
			}
		}
		this.setScrollBarToCurrentIndex();
		this.updateSlots();
	}

	/// <summary>Restore the page state when it's recreated for a window resize.</summary>
	/// <param name="oldPage">The previous page instance before it was recreated.</param>
	public void postWindowSizeChange(IClickableMenu oldPage)
	{
		if (oldPage is SocialPage oldCollectionsPage)
		{
			this.slotPosition = oldCollectionsPage.slotPosition;
			this.setScrollBarToCurrentIndex();
		}
	}

	/// <summary>Find all social NPCs which should be shown on the social page.</summary>
	public List<SocialEntry> FindSocialCharacters()
	{
		List<SocialEntry> players = new List<SocialEntry>();
		Dictionary<string, SocialEntry> villagers = new Dictionary<string, SocialEntry>();
		List<SocialEntry> children = new List<SocialEntry>();
		foreach (NPC npc in this.GetAllNpcs())
		{
			if (!Game1.player.friendshipData.TryGetValue(npc.Name, out var friendship))
			{
				friendship = null;
			}
			if (npc is Child)
			{
				children.Add(new SocialEntry(npc, friendship, null, npc.displayName));
			}
			else
			{
				if (!npc.CanSocialize)
				{
					continue;
				}
				CharacterData data = npc.GetData();
				string displayName = npc.displayName;
				switch (data?.SocialTab)
				{
				case SocialTabBehavior.HiddenUntilMet:
					if (friendship == null)
					{
						continue;
					}
					break;
				case SocialTabBehavior.UnknownUntilMet:
					if (friendship == null)
					{
						displayName = "???";
					}
					break;
				case SocialTabBehavior.AlwaysShown:
					if (friendship == null)
					{
						Game1.player.friendshipData.Add(npc.Name, friendship = new Friendship());
					}
					break;
				case SocialTabBehavior.HiddenAlways:
					continue;
				}
				villagers[npc.Name] = new SocialEntry(npc, friendship, data, displayName);
			}
		}
		int orderMet = 0;
		foreach (KeyValuePair<string, Friendship> pair2 in Game1.player.friendshipData.Pairs)
		{
			if (villagers.TryGetValue(pair2.Key, out var entry))
			{
				entry.OrderMet = orderMet++;
			}
		}
		foreach (Farmer player in Game1.getAllFarmers())
		{
			if (!player.IsLocalPlayer && (player.IsMainPlayer || player.isCustomized.Value) && !player.IsDedicatedPlayer)
			{
				Friendship friendship2 = Game1.player.team.GetFriendship(Game1.player.UniqueMultiplayerID, player.UniqueMultiplayerID);
				players.Add(new SocialEntry(player, friendship2));
			}
		}
		List<SocialEntry> list = new List<SocialEntry>();
		list.AddRange(players);
		list.AddRange(from socialEntry in villagers.Values
			orderby socialEntry.Friendship?.Points descending, socialEntry.OrderMet, socialEntry.DisplayName
			select socialEntry);
		list.AddRange(children.OrderBy((SocialEntry p) => p.DisplayName));
		return list;
	}

	/// <summary>Get all child or villager NPCs from the world and friendship data.</summary>
	public IEnumerable<NPC> GetAllNpcs()
	{
		HashSet<string> nonSocial = new HashSet<string>();
		Dictionary<string, NPC> found = new Dictionary<string, NPC>();
		Utility.ForEachCharacter(delegate(NPC npc)
		{
			if (npc is Child)
			{
				found[npc.Name + "$$child"] = npc;
			}
			else if (npc.IsVillager)
			{
				NPC value;
				if (!npc.CanSocialize)
				{
					nonSocial.Add(npc.Name);
				}
				else if (found.TryGetValue(npc.Name, out value) && npc != value)
				{
					bool flag = true;
					if (Game1.IsClient)
					{
						bool num = value.currentLocation.IsActiveLocation();
						bool flag2 = npc.currentLocation.IsActiveLocation();
						if (num != flag2)
						{
							if (flag2)
							{
								found[npc.Name] = npc;
							}
							flag = false;
						}
					}
					if (flag)
					{
						Game1.log.Warn($"The social page found conflicting NPCs with name {npc.Name} (one at {value.currentLocation?.NameOrUniqueName} {value.TilePoint}, the other at {npc.currentLocation?.NameOrUniqueName} {npc.TilePoint}); only the first will be shown.");
					}
				}
				else
				{
					found[npc.Name] = npc;
				}
			}
			return true;
		});
		Event @event = Game1.currentLocation?.currentEvent;
		if (@event != null)
		{
			foreach (NPC actor in @event.actors)
			{
				if (actor.IsVillager && actor.CanSocialize)
				{
					found[actor.Name] = actor;
				}
			}
		}
		foreach (string name in Game1.player.friendshipData.Keys)
		{
			if (nonSocial.Contains(name) || found.ContainsKey(name) || !NPC.TryGetData(name, out var _))
			{
				continue;
			}
			string textureName = NPC.getTextureNameForCharacter(name);
			string spriteAssetName = "Characters\\" + textureName;
			string portraitAssetName = "Portraits\\" + textureName;
			if (Game1.content.DoesAssetExist<Texture2D>(spriteAssetName) && Game1.content.DoesAssetExist<Texture2D>(portraitAssetName))
			{
				try
				{
					AnimatedSprite sprite = new AnimatedSprite(spriteAssetName, 0, 16, 32);
					Texture2D portraits = Game1.content.Load<Texture2D>(portraitAssetName);
					found[name] = new NPC(sprite, Vector2.Zero, "Town", 0, name, portraits, eventActor: false);
				}
				catch
				{
				}
			}
		}
		return found.Values;
	}

	/// <summary>Load the clickable components to display.</summary>
	public void CreateComponents()
	{
		this.sprites.Clear();
		this.characterSlots.Clear();
		for (int i = 0; i < this.SocialEntries.Count; i++)
		{
			this.sprites.Add(this.CreateSpriteComponent(this.SocialEntries[i], i));
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
	public ClickableTextureComponent CreateSpriteComponent(SocialEntry entry, int index)
	{
		Rectangle bounds = new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth + 4, 0, base.width, 64);
		Rectangle sourceRect = ((!entry.IsPlayer && entry.Character is NPC npc) ? npc.getMugShotSourceRect() : Rectangle.Empty);
		return new ClickableTextureComponent(index.ToString(), bounds, null, "", entry.Character.Sprite.Texture, sourceRect, 4f);
	}

	/// <summary>Get the social entry from its index in the list.</summary>
	/// <param name="index">The index in the social list.</param>
	public SocialEntry GetSocialEntry(int index)
	{
		if (index < 0 || index >= this.SocialEntries.Count)
		{
			index = 0;
		}
		return this.SocialEntries[index];
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
			if (this.sprites.Count > j)
			{
				int y = base.yPositionOnScreen + IClickableMenu.borderWidth + 32 + 112 * index + 32;
				this.sprites[j].bounds.Y = y;
			}
			index++;
		}
		this.populateClickableComponentList();
		this.addTabsToClickableComponents();
	}

	public void addTabsToClickableComponents()
	{
		if (Game1.activeClickableMenu is GameMenu gameMenu && !base.allClickableComponents.Contains(gameMenu.tabs[0]))
		{
			base.allClickableComponents.AddRange(gameMenu.tabs);
		}
	}

	protected void _SelectSlot(SocialEntry entry)
	{
		bool found = false;
		for (int i = 0; i < this.SocialEntries.Count; i++)
		{
			SocialEntry cur = this.SocialEntries[i];
			if (cur.InternalName == entry.InternalName && cur.IsPlayer == entry.IsPlayer && cur.IsChild == entry.IsChild)
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
			if (i < this.slotPosition || i >= this.slotPosition + 5 || !this.characterSlots[i].bounds.Contains(x, y))
			{
				continue;
			}
			SocialEntry entry = this.GetSocialEntry(i);
			if (!entry.IsPlayer && !entry.IsChild)
			{
				Character character = entry.Character;
				if (Game1.player.friendshipData.ContainsKey(character.name.Value))
				{
					Game1.playSound("bigSelect");
					int cached_slot_position = this.slotPosition;
					ProfileMenu profileMenu = new ProfileMenu(entry, this.SocialEntries);
					profileMenu.exitFunction = delegate
					{
						if (((GameMenu)(Game1.activeClickableMenu = new GameMenu(GameMenu.socialTab, -1, playOpeningSound: false))).GetCurrentPage() is SocialPage socialPage)
						{
							socialPage.slotPosition = cached_slot_position;
							socialPage._SelectSlot(profileMenu.Current);
						}
					};
					Game1.activeClickableMenu = profileMenu;
					if (Game1.options.SnappyMenus)
					{
						profileMenu.snapToDefaultClickableComponent();
					}
					return;
				}
			}
			Game1.playSound("shiny4");
			break;
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

	public bool isCharacterSlotClickable(int i)
	{
		SocialEntry entry = this.GetSocialEntry(i);
		if (entry != null && !entry.IsPlayer && !entry.IsChild)
		{
			return entry.IsMet;
		}
		return false;
	}

	/// <summary>Draw an NPC's entry in the social page.</summary>
	/// <param name="b">The sprite batch being drawn.</param>
	/// <param name="i">The index of the NPC in <see cref="F:StardewValley.Menus.SocialPage.sprites" />.</param>
	public void drawNPCSlot(SpriteBatch b, int i)
	{
		SocialEntry entry = this.GetSocialEntry(i);
		if (entry == null)
		{
			return;
		}
		if (this.isCharacterSlotClickable(i) && this.characterSlots[i].bounds.Contains(Game1.getMouseX(), Game1.getMouseY()))
		{
			b.Draw(Game1.staminaRect, new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth - 4, this.sprites[i].bounds.Y - 4, this.characterSlots[i].bounds.Width, this.characterSlots[i].bounds.Height - 12), Color.White * 0.25f);
		}
		this.sprites[i].draw(b);
		string name = entry.InternalName;
		Gender gender = entry.Gender;
		bool datable = entry.IsDatable;
		bool isDating = entry.IsDatingCurrentPlayer();
		bool isCurrentSpouse = entry.IsMarriedToCurrentPlayer();
		bool housemate = entry.IsRoommateForCurrentPlayer();
		float lineHeight = Game1.smallFont.MeasureString("W").Y;
		float russianOffsetY = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? ((0f - lineHeight) / 2f) : 0f);
		b.DrawString(Game1.dialogueFont, entry.DisplayName, new Vector2((float)(base.xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 64 - 20 + 96) - Game1.dialogueFont.MeasureString(entry.DisplayName).X / 2f, (float)(this.sprites[i].bounds.Y + 48) + russianOffsetY - (float)(datable ? 24 : 20)), Game1.textColor);
		for (int hearts = 0; hearts < Math.Max(Utility.GetMaximumHeartsForCharacter(Game1.getCharacterFromName(name)), 10); hearts++)
		{
			this.drawNPCSlotHeart(b, i, entry, hearts, isDating, isCurrentSpouse);
		}
		if (datable || housemate)
		{
			string text = (housemate ? ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Housemate_Female") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Housemate_Male")) : (isCurrentSpouse ? ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Wife") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Husband")) : (entry.IsMarriedToAnyone() ? ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\UI:SocialPage_Relationship_MarriedToOtherPlayer_FemaleNpc") : Game1.content.LoadString("Strings\\UI:SocialPage_Relationship_MarriedToOtherPlayer_MaleNpc")) : ((!Game1.player.isMarriedOrRoommates() && isDating) ? ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Girlfriend") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Boyfriend")) : ((!entry.IsDivorcedFromCurrentPlayer()) ? ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Female") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Male")) : ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_ExWife") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_ExHusband")))))));
			int width = (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2;
			text = Game1.parseText(text, Game1.smallFont, width);
			Vector2 textSize = Game1.smallFont.MeasureString(text);
			b.DrawString(Game1.smallFont, text, new Vector2((float)(base.xPositionOnScreen + 192 + 8) - textSize.X / 2f, (float)this.sprites[i].bounds.Bottom - (textSize.Y - lineHeight)), Game1.textColor);
		}
		if (!isCurrentSpouse && !entry.IsChild)
		{
			Utility.drawWithShadow(b, Game1.mouseCursors2, new Vector2(base.xPositionOnScreen + 384 + 304, this.sprites[i].bounds.Y - 4), new Rectangle(166, 174, 14, 12), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f, 0, -1, 0.2f);
			Texture2D mouseCursors = Game1.mouseCursors;
			Vector2 position = new Vector2(base.xPositionOnScreen + 384 + 296, this.sprites[i].bounds.Y + 32 + 20);
			Friendship friendship = entry.Friendship;
			b.Draw(mouseCursors, position, new Rectangle(227 + ((friendship != null && friendship.GiftsThisWeek >= 2) ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			Texture2D mouseCursors2 = Game1.mouseCursors;
			Vector2 position2 = new Vector2(base.xPositionOnScreen + 384 + 336, this.sprites[i].bounds.Y + 32 + 20);
			Friendship friendship2 = entry.Friendship;
			b.Draw(mouseCursors2, position2, new Rectangle(227 + ((friendship2 != null && friendship2.GiftsThisWeek >= 1) ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			Utility.drawWithShadow(b, Game1.mouseCursors2, new Vector2(base.xPositionOnScreen + 384 + 424, this.sprites[i].bounds.Y), new Rectangle(180, 175, 13, 11), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f, 0, -1, 0.2f);
			Texture2D mouseCursors3 = Game1.mouseCursors;
			Vector2 position3 = new Vector2(base.xPositionOnScreen + 384 + 432, this.sprites[i].bounds.Y + 32 + 20);
			Friendship friendship3 = entry.Friendship;
			b.Draw(mouseCursors3, position3, new Rectangle(227 + ((friendship3 != null && friendship3.TalkedToToday) ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		}
		if (isCurrentSpouse)
		{
			if (!housemate || name == "Krobus")
			{
				b.Draw(Game1.objectSpriteSheet, new Vector2(base.xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, this.sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, housemate ? 808 : 460, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
			}
		}
		else if (isDating)
		{
			b.Draw(Game1.objectSpriteSheet, new Vector2(base.xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, this.sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, housemate ? 808 : 458, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
		}
	}

	/// <summary>Draw the heart sprite for an NPC's entry in the social page.</summary>
	/// <param name="b">The sprite batch being drawn.</param>
	/// <param name="npcIndex">The index of the NPC in <see cref="F:StardewValley.Menus.SocialPage.sprites" />.</param>
	/// <param name="entry">The NPC's cached social data.</param>
	/// <param name="hearts">The current heart index being drawn (starting at 0 for the first heart).</param>
	/// <param name="isDating">Whether the player is currently dating this NPC.</param>
	/// <param name="isCurrentSpouse">Whether the player is currently married to this NPC.</param>
	public void drawNPCSlotHeart(SpriteBatch b, int npcIndex, SocialEntry entry, int hearts, bool isDating, bool isCurrentSpouse)
	{
		bool isLockedHeart = entry.IsDatable && !isDating && !isCurrentSpouse && hearts >= 8;
		int heartX = ((hearts < entry.HeartLevel || isLockedHeart) ? 211 : 218);
		Color heartTint = ((hearts < 10 && isLockedHeart) ? (Color.Black * 0.35f) : Color.White);
		if (hearts < 10)
		{
			b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + 320 - 4 + hearts * 32, this.sprites[npcIndex].bounds.Y + 64 - 28), new Rectangle(heartX, 428, 7, 6), heartTint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		}
		else
		{
			b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + 320 - 4 + (hearts - 10) * 32, this.sprites[npcIndex].bounds.Y + 64), new Rectangle(heartX, 428, 7, 6), heartTint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		}
	}

	public int rowPosition(int i)
	{
		int j = i - this.slotPosition;
		int rowHeight = 112;
		return base.yPositionOnScreen + IClickableMenu.borderWidth + 160 + 4 + j * rowHeight;
	}

	public void drawFarmerSlot(SpriteBatch b, int i)
	{
		SocialEntry entry = this.GetSocialEntry(i);
		if (entry == null)
		{
			return;
		}
		if (!entry.IsPlayer)
		{
			Game1.log.Warn($"Social page can't draw farmer slot for index {i}: this is NPC '{entry.InternalName}', not a farmer.");
			return;
		}
		Farmer farmer = (Farmer)entry.Character;
		Gender gender = entry.Gender;
		ClickableTextureComponent clickableTextureComponent = this.sprites[i];
		int x = clickableTextureComponent.bounds.X;
		int y = clickableTextureComponent.bounds.Y;
		Rectangle origClip = b.GraphicsDevice.ScissorRectangle;
		Rectangle newClip = origClip;
		newClip.Height = Math.Min(newClip.Bottom, this.rowPosition(i)) - newClip.Y - 4;
		b.GraphicsDevice.ScissorRectangle = newClip;
		FarmerRenderer.isDrawingForUI = true;
		try
		{
			farmer.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(farmer.bathingClothes.Value ? 108 : 0, 0, secondaryArm: false, flip: false), farmer.bathingClothes.Value ? 108 : 0, new Rectangle(0, farmer.bathingClothes.Value ? 576 : 0, 16, 32), new Vector2(x, y), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, farmer);
		}
		finally
		{
			b.GraphicsDevice.ScissorRectangle = origClip;
		}
		FarmerRenderer.isDrawingForUI = false;
		bool num = entry.IsMarriedToCurrentPlayer();
		float lineHeight = Game1.smallFont.MeasureString("W").Y;
		float russianOffsetY = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? ((0f - lineHeight) / 2f) : 0f);
		b.DrawString(Game1.dialogueFont, farmer.Name, new Vector2(base.xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 96 - 20, (float)(this.sprites[i].bounds.Y + 48) + russianOffsetY - 24f), Game1.textColor);
		string text = ((!Game1.content.ShouldUseGenderedCharacterTranslations()) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Female") : ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Female").Split('/')[0] : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Female").Split('/').Last()));
		if (num)
		{
			text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Husband") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Wife"));
		}
		else if (farmer.isMarriedOrRoommates() && !farmer.hasRoommate())
		{
			text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\UI:SocialPage_Relationship_MarriedToOtherPlayer_MaleNpc") : Game1.content.LoadString("Strings\\UI:SocialPage_Relationship_MarriedToOtherPlayer_FemaleNpc"));
		}
		else if (!Game1.player.isMarriedOrRoommates() && entry.IsDatingCurrentPlayer())
		{
			text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Boyfriend") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Girlfriend"));
		}
		else if (entry.IsDivorcedFromCurrentPlayer())
		{
			text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_ExHusband") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_ExWife"));
		}
		text = Game1.parseText(width: (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2, text: text, whichFont: Game1.smallFont);
		Vector2 textSize = Game1.smallFont.MeasureString(text);
		b.DrawString(Game1.smallFont, text, new Vector2((float)(base.xPositionOnScreen + 192 + 8) - textSize.X / 2f, (float)this.sprites[i].bounds.Bottom - (textSize.Y - lineHeight)), Game1.textColor);
		if (num)
		{
			b.Draw(Game1.objectSpriteSheet, new Vector2(base.xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, this.sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 801, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
		}
		else if (entry.IsDatingCurrentPlayer())
		{
			b.Draw(Game1.objectSpriteSheet, new Vector2(base.xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, this.sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 458, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		b.End();
		b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
		base.drawHorizontalPartition(b, base.yPositionOnScreen + IClickableMenu.borderWidth + 128 + 4, small: true);
		base.drawHorizontalPartition(b, base.yPositionOnScreen + IClickableMenu.borderWidth + 192 + 32 + 20, small: true);
		base.drawHorizontalPartition(b, base.yPositionOnScreen + IClickableMenu.borderWidth + 320 + 36, small: true);
		base.drawHorizontalPartition(b, base.yPositionOnScreen + IClickableMenu.borderWidth + 384 + 32 + 52, small: true);
		for (int i = this.slotPosition; i < this.slotPosition + 5 && i < this.sprites.Count; i++)
		{
			SocialEntry entry = this.GetSocialEntry(i);
			if (entry != null)
			{
				if (entry.IsPlayer)
				{
					this.drawFarmerSlot(b, i);
				}
				else
				{
					this.drawNPCSlot(b, i);
				}
			}
		}
		Rectangle origClip = b.GraphicsDevice.ScissorRectangle;
		Rectangle newClip = origClip;
		newClip.Y = Math.Max(0, this.rowPosition(this.numFarmers - 1));
		newClip.Height -= newClip.Y;
		if (newClip.Height > 0)
		{
			b.GraphicsDevice.ScissorRectangle = newClip;
			try
			{
				base.drawVerticalPartition(b, base.xPositionOnScreen + 256 + 12, small: true);
				base.drawVerticalPartition(b, base.xPositionOnScreen + 384 + 368, small: true);
				base.drawVerticalPartition(b, base.xPositionOnScreen + 256 + 12 + 352, small: true);
			}
			finally
			{
				b.GraphicsDevice.ScissorRectangle = origClip;
			}
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
