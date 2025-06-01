using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;

namespace StardewValley.Menus;

public class Bundle : ClickableComponent
{
	/// <summary>The index in the raw <c>Data/Bundles</c> data for the internal name.</summary>
	public const int NameIndex = 0;

	/// <summary>The index in the raw <c>Data/Bundles</c> data for the reward data.</summary>
	public const int RewardIndex = 1;

	/// <summary>The index in the raw <c>Data/Bundles</c> data for the items needed to complete the bundle.</summary>
	public const int IngredientsIndex = 2;

	/// <summary>The index in the raw <c>Data/Bundles</c> data for the bundle color.</summary>
	public const int ColorIndex = 3;

	/// <summary>The index in the raw <c>Data/Bundles</c> data for the optional number of slots to fill.</summary>
	public const int NumberOfSlotsIndex = 4;

	/// <summary>The index in the raw <c>Data/Bundles</c> data for the optional override texture name and sprite index.</summary>
	public const int SpriteIndex = 5;

	/// <summary>The index in the raw <c>Data/Bundles</c> data for the display name.</summary>
	public const int DisplayNameIndex = 6;

	/// <summary>The number of slash-delimited fields in the raw <c>Data/Bundles</c> data.</summary>
	public const int FieldCount = 7;

	public const float shakeRate = (float)Math.PI / 200f;

	public const float shakeDecayRate = 0.0030679617f;

	public const int Color_Green = 0;

	public const int Color_Purple = 1;

	public const int Color_Orange = 2;

	public const int Color_Yellow = 3;

	public const int Color_Red = 4;

	public const int Color_Blue = 5;

	public const int Color_Teal = 6;

	public const float DefaultShakeForce = (float)Math.PI * 3f / 128f;

	public string rewardDescription;

	public List<BundleIngredientDescription> ingredients;

	public int bundleColor;

	public int numberOfIngredientSlots;

	public int bundleIndex;

	public int completionTimer;

	public bool complete;

	public bool depositsAllowed = true;

	public Texture2D bundleTextureOverride;

	public int bundleTextureIndexOverride = -1;

	public TemporaryAnimatedSprite sprite;

	private float maxShake;

	private bool shakeLeft;

	public Bundle(string name, string displayName, List<BundleIngredientDescription> ingredients, bool[] completedIngredientsList, string rewardListString = "")
		: base(new Rectangle(0, 0, 64, 64), "")
	{
		base.name = name;
		base.label = displayName;
		this.rewardDescription = rewardListString;
		this.numberOfIngredientSlots = ingredients.Count;
		this.ingredients = ingredients;
	}

	public Bundle(int bundleIndex, string rawBundleInfo, bool[] completedIngredientsList, Point position, string textureName, JunimoNoteMenu menu)
		: base(new Rectangle(position.X, position.Y, 64, 64), "")
	{
		if (menu != null && menu.fromGameMenu)
		{
			this.depositsAllowed = false;
		}
		this.bundleIndex = bundleIndex;
		string[] split = rawBundleInfo.Split('/');
		base.name = split[0];
		base.label = split[6];
		this.rewardDescription = split[1];
		if (!string.IsNullOrWhiteSpace(split[5]))
		{
			try
			{
				string[] parts = split[5].Split(':', 2);
				if (parts.Length == 2)
				{
					this.bundleTextureOverride = Game1.content.Load<Texture2D>(parts[0]);
					this.bundleTextureIndexOverride = int.Parse(parts[1]);
				}
				else
				{
					this.bundleTextureIndexOverride = int.Parse(split[5]);
				}
			}
			catch
			{
				this.bundleTextureOverride = null;
				this.bundleTextureIndexOverride = -1;
			}
		}
		string[] ingredientsSplit = ArgUtility.SplitBySpace(split[2]);
		this.complete = true;
		this.ingredients = new List<BundleIngredientDescription>();
		int tally = 0;
		for (int i = 0; i < ingredientsSplit.Length; i += 3)
		{
			this.ingredients.Add(new BundleIngredientDescription(ingredientsSplit[i], Convert.ToInt32(ingredientsSplit[i + 1]), Convert.ToInt32(ingredientsSplit[i + 2]), completedIngredientsList[i / 3]));
			if (!completedIngredientsList[i / 3])
			{
				this.complete = false;
			}
			else
			{
				tally++;
			}
		}
		this.bundleColor = Convert.ToInt32(split[3]);
		this.numberOfIngredientSlots = ArgUtility.GetInt(split, 4, this.ingredients.Count);
		if (tally >= this.numberOfIngredientSlots)
		{
			this.complete = true;
		}
		this.sprite = new TemporaryAnimatedSprite(textureName, new Rectangle(this.bundleColor * 256 % 512, 244 + this.bundleColor * 256 / 512 * 16, 16, 16), 70f, 3, 99999, new Vector2(base.bounds.X, base.bounds.Y), flicker: false, flipped: false, 0.8f, 0f, Color.White, 4f, 0f, 0f, 0f)
		{
			pingPong = true
		};
		this.sprite.paused = true;
		this.sprite.sourceRect.X += this.sprite.sourceRect.Width;
		if (base.name.ContainsIgnoreCase(Game1.currentSeason) && !this.complete)
		{
			this.shake();
		}
		if (this.complete)
		{
			this.completionAnimation(menu, playSound: false);
		}
	}

	public Item getReward()
	{
		return Utility.getItemFromStandardTextDescription(this.rewardDescription, Game1.player);
	}

	public void shake(float force = (float)Math.PI * 3f / 128f)
	{
		if (this.sprite.paused)
		{
			this.maxShake = force;
		}
	}

	public void shake(int extraInfo)
	{
		this.maxShake = (float)Math.PI * 3f / 128f;
		if (extraInfo == 1)
		{
			Game1.playSound("leafrustle");
			TemporaryAnimatedSprite tempSprite = new TemporaryAnimatedSprite(50, this.sprite.position, Bundle.getColorFromColorIndex(this.bundleColor))
			{
				motion = new Vector2(-1f, 0.5f),
				acceleration = new Vector2(0f, 0.02f)
			};
			tempSprite.sourceRect.Y++;
			tempSprite.sourceRect.Height--;
			JunimoNoteMenu.tempSprites.Add(tempSprite);
			tempSprite = new TemporaryAnimatedSprite(50, this.sprite.position, Bundle.getColorFromColorIndex(this.bundleColor))
			{
				motion = new Vector2(1f, 0.5f),
				acceleration = new Vector2(0f, 0.02f),
				flipped = true,
				delayBeforeAnimationStart = 50
			};
			tempSprite.sourceRect.Y++;
			tempSprite.sourceRect.Height--;
			JunimoNoteMenu.tempSprites.Add(tempSprite);
		}
	}

	public void tryHoverAction(int x, int y)
	{
		if (base.bounds.Contains(x, y) && !this.complete)
		{
			this.sprite.paused = false;
			JunimoNoteMenu.hoverText = Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", base.label);
		}
		else if (!this.complete)
		{
			this.sprite.reset();
			this.sprite.sourceRect.X += this.sprite.sourceRect.Width;
			this.sprite.paused = true;
		}
	}

	public bool IsValidItemForThisIngredientDescription(Item item, BundleIngredientDescription ingredient)
	{
		if (item == null || ingredient.completed || ingredient.quality > item.Quality)
		{
			return false;
		}
		if (ingredient.preservesId != null)
		{
			ItemQueryContext context = new ItemQueryContext(Game1.currentLocation, Game1.player, Game1.random, "query 'FLAVORED_ITEM'");
			if (ItemQueryResolver.TryResolve("FLAVORED_ITEM " + ingredient.id + " " + ingredient.preservesId, context).FirstOrDefault()?.Item is Object resultObj && item is Object ingredientObj && ingredientObj.preservedParentSheetIndex.Value != null && item.QualifiedItemId == resultObj.QualifiedItemId && ingredientObj.preservedParentSheetIndex.Value.Contains(ingredient.preservesId))
			{
				return true;
			}
			return false;
		}
		if (ingredient.category.HasValue)
		{
			if (item.QualifiedItemId == "(O)107" && ingredient.category == -5)
			{
				return true;
			}
			return item.Category == ingredient.category;
		}
		return ItemRegistry.HasItemId(item, ingredient.id);
	}

	public int GetBundleIngredientDescriptionIndexForItem(Item item)
	{
		for (int i = 0; i < this.ingredients.Count; i++)
		{
			if (this.IsValidItemForThisIngredientDescription(item, this.ingredients[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public bool canAcceptThisItem(Item item, ClickableTextureComponent slot)
	{
		return this.canAcceptThisItem(item, slot, ignore_stack_count: false);
	}

	public bool canAcceptThisItem(Item item, ClickableTextureComponent slot, bool ignore_stack_count = false)
	{
		if (!this.depositsAllowed)
		{
			return false;
		}
		for (int i = 0; i < this.ingredients.Count; i++)
		{
			if (this.IsValidItemForThisIngredientDescription(item, this.ingredients[i]) && (ignore_stack_count || this.ingredients[i].stack <= item.Stack) && slot?.item == null)
			{
				return true;
			}
		}
		return false;
	}

	public Item tryToDepositThisItem(Item item, ClickableTextureComponent slot, string noteTextureName, JunimoNoteMenu parentMenu)
	{
		if (!this.depositsAllowed)
		{
			if (Game1.player.hasCompletedCommunityCenter())
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:JunimoNote_MustBeAtAJM"));
			}
			else
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:JunimoNote_MustBeAtCC"));
			}
			return item;
		}
		CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
		for (int i = 0; i < this.ingredients.Count; i++)
		{
			BundleIngredientDescription ingredient = this.ingredients[i];
			if (this.IsValidItemForThisIngredientDescription(item, ingredient) && slot.item == null)
			{
				item = item.ConsumeStack(ingredient.stack);
				ingredient = (this.ingredients[i] = new BundleIngredientDescription(ingredient, completed: true));
				this.ingredientDepositAnimation(slot, noteTextureName);
				string id = JunimoNoteMenu.GetRepresentativeItemId(ingredient);
				if (ingredient.preservesId != null)
				{
					slot.item = Utility.CreateFlavoredItem(ingredient.id, ingredient.preservesId, ingredient.quality, ingredient.stack);
				}
				else
				{
					slot.item = ItemRegistry.Create(id, ingredient.stack, ingredient.quality);
				}
				Game1.playSound("newArtifact");
				slot.sourceRect.X = 512;
				slot.sourceRect.Y = 244;
				if (parentMenu.onIngredientDeposit != null)
				{
					parentMenu.onIngredientDeposit(i);
					break;
				}
				communityCenter.bundles.FieldDict[this.bundleIndex][i] = true;
				Game1.multiplayer.globalChatInfoMessage("BundleDonate", Game1.player.displayName, TokenStringBuilder.ItemNameFor(slot.item));
				break;
			}
		}
		return item;
	}

	public void ingredientDepositAnimation(ClickableTextureComponent slot, string noteTextureName, bool skipAnimation = false)
	{
		TemporaryAnimatedSprite t = new TemporaryAnimatedSprite(noteTextureName, new Rectangle(530, 244, 18, 18), 50f, 6, 1, new Vector2(slot.bounds.X, slot.bounds.Y), flicker: false, flipped: false, 0.88f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			holdLastFrame = true,
			endSound = "cowboy_monsterhit"
		};
		if (skipAnimation)
		{
			t.sourceRect.Offset(t.sourceRect.Width * 5, 0);
			t.sourceRectStartingPos = new Vector2(t.sourceRect.X, t.sourceRect.Y);
			t.animationLength = 1;
		}
		JunimoNoteMenu.tempSprites.Add(t);
	}

	public bool canBeClicked()
	{
		return !this.complete;
	}

	public void completionAnimation(JunimoNoteMenu menu, bool playSound = true, int delay = 0)
	{
		if (delay <= 0)
		{
			this.completionAnimation(playSound);
		}
		else
		{
			this.completionTimer = delay;
		}
	}

	private void completionAnimation(bool playSound = true)
	{
		if (Game1.activeClickableMenu is JunimoNoteMenu junimoNoteMenu)
		{
			junimoNoteMenu.takeDownBundleSpecificPage();
		}
		this.sprite.pingPong = false;
		this.sprite.paused = false;
		this.sprite.sourceRect.X = (int)this.sprite.sourceRectStartingPos.X;
		this.sprite.sourceRect.X += this.sprite.sourceRect.Width;
		this.sprite.animationLength = 15;
		this.sprite.interval = 50f;
		this.sprite.totalNumberOfLoops = 0;
		this.sprite.holdLastFrame = true;
		this.sprite.endFunction = shake;
		this.sprite.extraInfoForEndBehavior = 1;
		if (this.complete)
		{
			this.sprite.sourceRect.X += this.sprite.sourceRect.Width * 14;
			this.sprite.sourceRectStartingPos = new Vector2(this.sprite.sourceRect.X, this.sprite.sourceRect.Y);
			this.sprite.currentParentTileIndex = 14;
			this.sprite.interval = 0f;
			this.sprite.animationLength = 1;
			this.sprite.extraInfoForEndBehavior = 0;
		}
		else
		{
			if (playSound)
			{
				Game1.playSound("dwop");
			}
			base.bounds.Inflate(64, 64);
			JunimoNoteMenu.tempSprites.AddRange(Utility.sparkleWithinArea(base.bounds, 8, Bundle.getColorFromColorIndex(this.bundleColor) * 0.5f));
			base.bounds.Inflate(-64, -64);
		}
		this.complete = true;
	}

	public void update(GameTime time)
	{
		this.sprite.update(time);
		if (this.completionTimer > 0 && JunimoNoteMenu.screenSwipe == null)
		{
			this.completionTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.completionTimer <= 0)
			{
				this.completionAnimation();
			}
		}
		if (Game1.random.NextDouble() < 0.005 && (this.complete || base.name.ContainsIgnoreCase(Game1.currentSeason)))
		{
			this.shake();
		}
		if (this.maxShake > 0f)
		{
			if (this.shakeLeft)
			{
				this.sprite.rotation -= (float)Math.PI / 200f;
				if (this.sprite.rotation <= 0f - this.maxShake)
				{
					this.shakeLeft = false;
				}
			}
			else
			{
				this.sprite.rotation += (float)Math.PI / 200f;
				if (this.sprite.rotation >= this.maxShake)
				{
					this.shakeLeft = true;
				}
			}
		}
		if (this.maxShake > 0f)
		{
			this.maxShake = Math.Max(0f, this.maxShake - 0.0007669904f);
		}
	}

	public void draw(SpriteBatch b)
	{
		this.sprite.draw(b, localPosition: true);
	}

	public static Color getColorFromColorIndex(int color)
	{
		return color switch
		{
			5 => Color.LightBlue, 
			0 => Color.Lime, 
			2 => Color.Orange, 
			1 => Color.DeepPink, 
			4 => Color.Red, 
			6 => Color.Cyan, 
			3 => Color.Orange, 
			_ => Color.Lime, 
		};
	}
}
