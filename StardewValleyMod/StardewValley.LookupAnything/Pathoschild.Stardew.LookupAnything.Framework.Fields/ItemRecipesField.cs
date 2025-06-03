using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class ItemRecipesField : GenericField
{
	private readonly RecipeByTypeGroup[] RecipesByType;

	private readonly GameHelper GameHelper;

	private readonly ISubjectRegistry Codex;

	private readonly bool ShowUnknownRecipes;

	private readonly bool ShowInvalidRecipes;

	private readonly bool ShowLabelForSingleGroup;

	private readonly bool ShowOutputLabels;

	private readonly float LineHeight = Game1.smallFont.MeasureString("ABC").Y;

	private readonly Item? Ingredient;

	private float IconSize => this.LineHeight;

	public ItemRecipesField(GameHelper gameHelper, ISubjectRegistry codex, string label, Item? ingredient, RecipeModel[] recipes, bool showUnknownRecipes, bool showInvalidRecipes, bool showLabelForSingleGroup = true, bool showOutputLabels = true)
		: base(label, hasValue: true)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		this.GameHelper = gameHelper;
		this.Codex = codex;
		this.RecipesByType = this.BuildRecipeGroups(ingredient, recipes).ToArray();
		this.ShowUnknownRecipes = showUnknownRecipes;
		this.ShowInvalidRecipes = showInvalidRecipes;
		this.ShowLabelForSingleGroup = showLabelForSingleGroup;
		this.ShowOutputLabels = showOutputLabels;
		this.Ingredient = ingredient;
	}

	public int GetShownRecipesCount()
	{
		int count = 0;
		RecipeByTypeGroup[] recipesByType = this.RecipesByType;
		foreach (RecipeByTypeGroup group in recipesByType)
		{
			RecipeEntry[] recipes = group.Recipes;
			foreach (RecipeEntry recipe in recipes)
			{
				if ((recipe.IsValid || this.ShowInvalidRecipes) && (recipe.IsKnown || this.ShowUnknownRecipes))
				{
					count++;
				}
			}
		}
		return count;
	}

	public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_061d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0624: Unknown result type (might be due to invalid IL or missing references)
		//IL_062e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Unknown result type (might be due to invalid IL or missing references)
		//IL_0590: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_0504: Unknown result type (might be due to invalid IL or missing references)
		//IL_051f: Unknown result type (might be due to invalid IL or missing references)
		//IL_052f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0551: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_042f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
		base.LinkTextAreas.Clear();
		Color knownIconColor = Color.White;
		Color knownTextColor = Color.Black;
		Color knownLinkColor = Color.Blue;
		Color unknownIconColor = knownIconColor * 0.5f;
		Color unknownTextColor = Color.Gray;
		Color unknownLinkColor = knownLinkColor * 0.65f;
		float inputDividerWidth = font.MeasureString("+").X;
		float itemSpacer = inputDividerWidth;
		Vector2 curPos = position;
		float absoluteWrapWidth = position.X + wrapWidth;
		float lineHeight = this.LineHeight;
		Vector2 iconSize = default(Vector2);
		((Vector2)(ref iconSize))._002Ector(this.IconSize);
		float joinerWidth = inputDividerWidth + itemSpacer * 2f;
		curPos.Y += 6f;
		RecipeByTypeGroup[] recipesByType = this.RecipesByType;
		Rectangle pixelArea = default(Rectangle);
		Rectangle pixelArea2 = default(Rectangle);
		foreach (RecipeByTypeGroup group in recipesByType)
		{
			bool alignColumns = wrapWidth >= group.TotalColumnWidth + itemSpacer + (float)(group.ColumnWidths.Length - 1) * joinerWidth;
			if (this.ShowLabelForSingleGroup || this.RecipesByType.Length > 1)
			{
				curPos.X = position.X + 0f;
				curPos += base.DrawIconText(spriteBatch, font, curPos, absoluteWrapWidth, group.Type + ":", Color.Black);
			}
			int hiddenUnknownRecipesCount = 0;
			RecipeEntry[] recipes = group.Recipes;
			foreach (RecipeEntry entry in recipes)
			{
				if (!this.ShowInvalidRecipes && !entry.IsValid)
				{
					continue;
				}
				if (!this.ShowUnknownRecipes && !entry.IsKnown)
				{
					hiddenUnknownRecipesCount++;
					continue;
				}
				Color iconColor = (entry.IsKnown ? knownIconColor : unknownIconColor);
				Color textColor = (entry.IsKnown ? knownTextColor : unknownTextColor);
				float recipeLeftMargin = position.X + 14f;
				((Vector2)(ref curPos))._002Ector(recipeLeftMargin, curPos.Y + 5f);
				float inputLeft = 0f;
				if (this.ShowOutputLabels)
				{
					ISubject subject = this.GetSubject(entry.Output.Entity);
					Color actualTextColor = ((subject == null) ? textColor : (entry.IsKnown ? knownLinkColor : unknownLinkColor));
					Vector2 outputSize = base.DrawIconText(spriteBatch, font, curPos, absoluteWrapWidth, entry.Output.DisplayText, actualTextColor, entry.Output.Sprite, iconSize, iconColor, entry.Output.Quality);
					float outputWidth = (alignColumns ? group.ColumnWidths[0] : outputSize.X);
					if (subject != null)
					{
						((Rectangle)(ref pixelArea))._002Ector((int)curPos.X, (int)curPos.Y, (int)outputWidth, (int)lineHeight);
						base.LinkTextAreas.Add(new LinkTextArea(subject, pixelArea));
					}
					inputLeft = (curPos.X = curPos.X + outputWidth + itemSpacer);
				}
				int k = 0;
				for (int last = entry.Inputs.Length - 1; k <= last; k++)
				{
					RecipeItemEntry input = entry.Inputs[k];
					ISubject subject2 = this.GetSubject(input.Entity);
					Vector2 curIconSize = iconSize;
					if ((object)input != null && input.IsGoldPrice && input.Sprite != null)
					{
						Rectangle sourceRectangle = input.Sprite.SourceRectangle;
						curIconSize = Utility.PointToVector2(((Rectangle)(ref sourceRectangle)).Size) * 4f;
					}
					Vector2 inputSize = base.DrawIconText(spriteBatch, font, curPos, absoluteWrapWidth, input.DisplayText, textColor, input.Sprite, curIconSize, iconColor, input.Quality, probe: true);
					if (alignColumns)
					{
						inputSize.X = group.ColumnWidths[k + 1];
					}
					if (curPos.X + inputSize.X > absoluteWrapWidth)
					{
						((Vector2)(ref curPos))._002Ector(inputLeft, curPos.Y + lineHeight + 2f);
					}
					Color actualTextColor2 = ((subject2 == null) ? textColor : (entry.IsKnown ? knownLinkColor : unknownLinkColor));
					base.DrawIconText(spriteBatch, font, curPos, absoluteWrapWidth, input.DisplayText, actualTextColor2, input.Sprite, curIconSize, iconColor, input.Quality);
					if (subject2 != null)
					{
						((Rectangle)(ref pixelArea2))._002Ector((int)curPos.X, (int)curPos.Y, (int)inputSize.X, (int)lineHeight);
						base.LinkTextAreas.Add(new LinkTextArea(subject2, pixelArea2));
					}
					((Vector2)(ref curPos))._002Ector(curPos.X + inputSize.X, curPos.Y);
					if (k != last)
					{
						if (curPos.X + joinerWidth > absoluteWrapWidth)
						{
							((Vector2)(ref curPos))._002Ector(inputLeft, curPos.Y + lineHeight + 2f);
						}
						else
						{
							curPos.X += itemSpacer;
						}
						Vector2 joinerSize = base.DrawIconText(spriteBatch, font, curPos, absoluteWrapWidth, "+", textColor);
						curPos.X += joinerSize.X + itemSpacer;
					}
				}
				curPos.X = recipeLeftMargin;
				curPos.Y += lineHeight;
				if (entry.Conditions != null)
				{
					ref float y = ref curPos.Y;
					float num = y;
					Vector2 position2 = curPos;
					position2.X = curPos.X + this.IconSize + (float)base.IconMargin;
					y = num + base.DrawIconText(spriteBatch, font, position2, absoluteWrapWidth, I18n.ConditionsSummary(entry.Conditions), textColor).Y;
				}
			}
			if (hiddenUnknownRecipesCount > 0)
			{
				((Vector2)(ref curPos))._002Ector(position.X + 14f + (float)base.IconMargin + this.IconSize, curPos.Y + 5f);
				base.DrawIconText(spriteBatch, font, curPos, absoluteWrapWidth, I18n.Item_UnknownRecipes(hiddenUnknownRecipesCount), Color.Gray);
				curPos.Y += lineHeight;
			}
			curPos.Y += lineHeight;
		}
		curPos.Y += 6f;
		return new Vector2(wrapWidth, curPos.Y - position.Y - lineHeight);
	}

	public override void CollapseIfLengthExceeds(int minResultsForCollapse, int countForLabel)
	{
		if (this.RecipesByType.Length != 0)
		{
			int shownRecipesCount = this.RecipesByType.Sum((RecipeByTypeGroup group) => group.Recipes.Count((RecipeEntry recipe) => this.ShowUnknownRecipes || recipe.IsKnown));
			if (shownRecipesCount >= minResultsForCollapse)
			{
				base.CollapseByDefault(I18n.Generic_ShowXResults(shownRecipesCount));
			}
		}
		else
		{
			base.CollapseIfLengthExceeds(minResultsForCollapse, countForLabel);
		}
	}

	private IEnumerable<RecipeByTypeGroup> BuildRecipeGroups(Item? ingredient, RecipeModel[] rawRecipes)
	{
		Dictionary<string, RecipeEntry[]> rawGroups = (from recipeEntry in rawRecipes.Select(delegate(RecipeModel recipeModel)
			{
				//IL_0244: Unknown result type (might be due to invalid IL or missing references)
				Item val = ((ingredient != null && recipeModel.IsForMachine(ingredient)) ? recipeModel.TryCreateItem(null) : recipeModel.TryCreateItem(ingredient));
				if (recipeModel.OutputQualifiedItemId == "__COMPLEX_RECIPE__")
				{
					string? key2 = recipeModel.Key;
					string displayType = recipeModel.DisplayType;
					bool isKnown = recipeModel.IsKnown();
					RecipeItemEntry[] inputs = Array.Empty<RecipeItemEntry>();
					ItemRecipesField itemRecipesField = this;
					string name = I18n.Item_RecipesForMachine_TooComplex();
					SpriteInfo? sprite = recipeModel.SpecialOutput?.Sprite;
					bool? isValid = true;
					return new RecipeEntry(key2, displayType, isKnown, inputs, itemRecipesField.CreateItemEntry(name, val, sprite, 1, 1, 100m, null, hasInputAndOutput: false, isValid), (recipeModel.Conditions.Length != 0) ? I18n.List(recipeModel.Conditions.Select(HumanReadableConditionParser.Format)) : null);
				}
				RecipeItemEntry output = ((!(ItemRegistry.GetDataOrErrorItem(recipeModel.OutputQualifiedItemId)?.ItemId == "DROP_IN")) ? this.CreateItemEntry(recipeModel.SpecialOutput?.DisplayText ?? ((val != null) ? val.DisplayName : null) ?? string.Empty, val, recipeModel.SpecialOutput?.Sprite, recipeModel.MinOutput, recipeModel.MaxOutput, recipeModel.OutputChance, recipeModel.Quality, hasInputAndOutput: true, recipeModel.SpecialOutput?.IsValid, recipeModel.SpecialOutput?.Entity) : this.CreateItemEntry(I18n.Item_RecipesForMachine_SameAsInput(), null, null, recipeModel.MinOutput, recipeModel.MaxOutput, recipeModel.OutputChance, recipeModel.Quality, hasInputAndOutput: true, true));
				IEnumerable<RecipeItemEntry> enumerable = recipeModel.Ingredients.Select(TryCreateItemEntry).WhereNotNull();
				RecipeType type2 = recipeModel.Type;
				if ((type2 != RecipeType.MachineInput && type2 != RecipeType.TailorInput) || 1 == 0)
				{
					enumerable = enumerable.OrderBy((RecipeItemEntry entry) => entry.DisplayText);
				}
				if (recipeModel.GoldPrice > 0)
				{
					enumerable = enumerable.Concat(new _003C_003Ez__ReadOnlySingleElementList<RecipeItemEntry>(new RecipeItemEntry(new SpriteInfo(Game1.debrisSpriteSheet, new Rectangle(5, 69, 6, 6)), Utility.getNumberWithCommas(recipeModel.GoldPrice), null, IsGoldPrice: true)));
				}
				return new RecipeEntry(recipeModel.Key, recipeModel.DisplayType, recipeModel.IsKnown(), enumerable.ToArray(), output, (recipeModel.Conditions.Length != 0) ? I18n.List(recipeModel.Conditions.Select(HumanReadableConditionParser.Format)) : null);
			})
			group recipeEntry by recipeEntry.UniqueKey into item
			select item.First() into p
			orderby p.Type, recipeEntry.Output.DisplayText
			group p by p.Type).ToDictionary((IGrouping<string, RecipeEntry> p) => p.Key, (IGrouping<string, RecipeEntry> p) => p.ToArray());
		List<float> columnWidths;
		foreach (KeyValuePair<string, RecipeEntry[]> item in rawGroups)
		{
			item.Deconstruct(out var key, out var value);
			string type = key;
			RecipeEntry[] recipes = value;
			columnWidths = new List<float>();
			RecipeEntry[] array = recipes;
			foreach (RecipeEntry recipe in array)
			{
				TrackWidth(0, recipe.Output.DisplayText + ":", recipe.Output.Sprite);
				for (int i = 0; i < recipe.Inputs.Length; i++)
				{
					TrackWidth(i + 1, recipe.Inputs[i].DisplayText, recipe.Inputs[i].Sprite);
				}
			}
			yield return new RecipeByTypeGroup(type, recipes, columnWidths.ToArray());
		}
		void TrackWidth(int index, string text, SpriteInfo? icon)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			while (columnWidths.Count < index + 1)
			{
				columnWidths.Add(0f);
			}
			float width = Game1.smallFont.MeasureString(text).X;
			if (icon != null)
			{
				width += this.IconSize + (float)base.IconMargin;
			}
			columnWidths[index] = Math.Max(columnWidths[index], width);
		}
	}

	private ISubject? GetSubject(object? entity)
	{
		if (entity == null)
		{
			return null;
		}
		Item item = (Item)((entity is Item) ? entity : null);
		if (item != null)
		{
			if (!(item.ItemId == "__COMPLEX_RECIPE__"))
			{
				string qualifiedItemId = item.QualifiedItemId;
				Item? ingredient = this.Ingredient;
				if (!(qualifiedItemId == ((ingredient != null) ? ingredient.QualifiedItemId : null)))
				{
					goto IL_0042;
				}
			}
			return null;
		}
		goto IL_0042;
		IL_0042:
		return this.Codex.GetByEntity(entity, null);
	}

	private RecipeItemEntry TryCreateItemEntry(RecipeIngredientModel ingredient)
	{
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		string inputId = ingredient.InputId;
		bool? isValid;
		if (inputId == "-777")
		{
			string name = I18n.Item_WildSeeds();
			int count = ingredient.Count;
			int count2 = ingredient.Count;
			isValid = true;
			return this.CreateItemEntry(name, null, null, count, count2, 100m, null, hasInputAndOutput: false, isValid);
		}
		if (int.TryParse(ingredient.InputId, out var category) && category < 0)
		{
			Item input = (Item)(object)this.GameHelper.GetObjectsByCategory(category).FirstOrDefault();
			if (input != null)
			{
				object name2 = input.Category switch
				{
					-5 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.572"), 
					-6 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.573"), 
					_ => input.getCategoryName(), 
				};
				int count3 = ingredient.Count;
				int count4 = ingredient.Count;
				isValid = true;
				return this.CreateItemEntry((string)name2, null, null, count3, count4, 100m, null, hasInputAndOutput: false, isValid);
			}
		}
		if (ingredient.InputId != null)
		{
			Item input2 = ItemRegistry.Create(ingredient.InputId, 1, 0, true);
			Object obj = (Object)(object)((input2 is Object) ? input2 : null);
			if (obj != null)
			{
				if (ingredient.PreservedItemId != null)
				{
					((NetFieldBase<string, NetString>)(object)obj.preservedParentSheetIndex).Value = ingredient.PreservedItemId;
				}
				if (ingredient.PreserveType.HasValue)
				{
					((NetFieldBase<PreserveType?, NetNullableEnum<PreserveType>>)(object)obj.preserve).Value = ingredient.PreserveType.Value;
				}
			}
			if (input2 != null)
			{
				string name3 = input2.DisplayName ?? input2.ItemId;
				if (ingredient.InputContextTags.Length != 0)
				{
					name3 = name3 + " (" + I18n.List(ingredient.InputContextTags.Select(HumanReadableContextTagParser.Format)) + ")";
				}
				return this.CreateItemEntry(name3, input2, null, ingredient.Count, ingredient.Count);
			}
		}
		if (ingredient.InputContextTags.Length != 0)
		{
			string name4 = I18n.List(ingredient.InputContextTags.Select(HumanReadableContextTagParser.Format));
			int count5 = ingredient.Count;
			int count6 = ingredient.Count;
			isValid = true;
			return this.CreateItemEntry(name4, null, null, count5, count6, 100m, null, hasInputAndOutput: false, isValid);
		}
		ObjectDataDefinition objectTypeDef = ItemRegistry.GetObjectTypeDefinition();
		string displayName = ingredient.InputId;
		if (ingredient.InputContextTags.Length != 0)
		{
			string text2;
			if (!string.IsNullOrWhiteSpace(displayName))
			{
				object obj2 = displayName;
				string[] inputContextTags = ingredient.InputContextTags;
				int num = 0;
				object[] array = new object[1 + inputContextTags.Length];
				array[num] = obj2;
				num++;
				string[] array2 = inputContextTags;
				foreach (string text in array2)
				{
					array[num] = text;
					num++;
				}
				text2 = I18n.List(new _003C_003Ez__ReadOnlyArray<object>(array));
			}
			else
			{
				text2 = I18n.List(ingredient.InputContextTags);
			}
			displayName = text2;
		}
		if (displayName == null)
		{
			displayName = "???";
		}
		string name5 = displayName;
		SpriteInfo sprite = new SpriteInfo(((BaseItemDataDefinition)objectTypeDef).GetErrorTexture(), ((BaseItemDataDefinition)objectTypeDef).GetErrorSourceRect());
		isValid = false;
		return this.CreateItemEntry(name5, null, sprite, 1, 1, 100m, null, hasInputAndOutput: false, isValid);
	}

	private RecipeItemEntry CreateItemEntry(string name, Item? item = null, SpriteInfo? sprite = null, int minCount = 1, int maxCount = 1, decimal chance = 100m, int? quality = null, bool hasInputAndOutput = false, bool? isValid = null, object? entity = null)
	{
		string text = ((minCount != maxCount) ? I18n.Item_RecipesForMachine_MultipleItems(name, I18n.Generic_Range(minCount, maxCount)) : ((minCount <= 1) ? name : I18n.Item_RecipesForMachine_MultipleItems(name, minCount)));
		if (chance > 0m && chance < 100m)
		{
			text = text + " (" + I18n.Generic_Percent(chance) + ")";
		}
		if (hasInputAndOutput)
		{
			text += ":";
		}
		return new RecipeItemEntry(sprite ?? this.GameHelper.GetSprite(item), text, quality, IsGoldPrice: false, isValid ?? (item != null && ItemRegistry.Exists(item.QualifiedItemId)), entity ?? item);
	}
}
