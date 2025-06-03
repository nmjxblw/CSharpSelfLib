using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.WildTrees;
using StardewValley.Network;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.TerrainFeatures;

internal class TreeSubject : BaseSubject
{
	private readonly Tree Target;

	private readonly Vector2 Tile;

	private readonly ISubjectRegistry Codex;

	public TreeSubject(ISubjectRegistry codex, GameHelper gameHelper, Tree tree, Vector2 tile)
		: base(gameHelper, TreeSubject.GetName(tree), null, I18n.Type_Tree())
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		this.Codex = codex;
		this.Target = tree;
		this.Tile = tile;
	}

	public override IEnumerable<ICustomField> GetData()
	{
		Tree tree = this.Target;
		WildTreeData data = tree.GetData();
		GameLocation location = ((TerrainFeature)tree).Location;
		bool isFertilized = ((NetFieldBase<bool, NetBool>)(object)tree.fertilized).Value;
		IModInfo fromMod = base.GameHelper.TryGetModFromStringId(((NetFieldBase<string, NetString>)(object)tree.treeType).Value);
		if (fromMod != null)
		{
			yield return new GenericField(I18n.AddedByMod(), I18n.AddedByMod_Summary(fromMod.Manifest.Name));
		}
		WildTreeGrowthStage stage = (WildTreeGrowthStage)Math.Min(((NetFieldBase<int, NetInt>)(object)tree.growthStage).Value, 5);
		bool isFullyGrown = (int)stage == 5;
		yield return new GenericField(I18n.Tree_Stage(), isFullyGrown ? I18n.Tree_Stage_Done() : I18n.Tree_Stage_Partial(I18n.For(stage), (int)stage, 5));
		if (!isFullyGrown)
		{
			string label = I18n.Tree_NextGrowth();
			if (!data.GrowsInWinter && (int)location.GetSeason() == 3 && !location.SeedsIgnoreSeasonsHere() && !isFertilized)
			{
				yield return new GenericField(label, I18n.Tree_NextGrowth_Winter());
			}
			else if ((int)stage == 4 && this.HasAdjacentTrees(this.Tile))
			{
				yield return new GenericField(label, I18n.Tree_NextGrowth_AdjacentTrees());
			}
			else
			{
				double chance = Math.Round((isFertilized ? data.FertilizedGrowthChance : data.GrowthChance) * 100f, 2);
				yield return new GenericField(label, I18n.Tree_NextGrowth_Chance(stage: I18n.For((WildTreeGrowthStage)(stage + 1)), chance: chance));
			}
		}
		if (!isFullyGrown)
		{
			if (!isFertilized)
			{
				yield return new GenericField(I18n.Tree_IsFertilized(), base.Stringify(false));
			}
			else
			{
				Item fertilizer = ItemRegistry.Create("(O)805", 1, 0, false);
				yield return new ItemIconField(base.GameHelper, I18n.Tree_IsFertilized(), fertilizer, this.Codex);
			}
		}
		if (isFullyGrown && !string.IsNullOrWhiteSpace(data.SeedItemId))
		{
			string seedName = GameI18n.GetObjectName(data.SeedItemId);
			if (((NetFieldBase<bool, NetBool>)(object)tree.hasSeed).Value)
			{
				yield return new ItemIconField(base.GameHelper, I18n.Tree_Seed(), ItemRegistry.Create(data.SeedItemId, 1, 0, false), this.Codex);
			}
			else
			{
				List<string> lines = new List<string>(2);
				if (data.SeedOnShakeChance > 0f)
				{
					lines.Add(I18n.Tree_Seed_ProbabilityDaily(data.SeedOnShakeChance * 100f, seedName));
				}
				if (data.SeedOnChopChance > 0f)
				{
					lines.Add(I18n.Tree_Seed_ProbabilityOnChop(data.SeedOnChopChance * 100f, seedName));
				}
				if (lines.Any())
				{
					yield return new GenericField(I18n.Tree_Seed(), I18n.Tree_Seed_NotReady() + Environment.NewLine + string.Join(Environment.NewLine, lines));
				}
			}
		}
		yield return new GenericField(I18n.InternalId(), ((NetFieldBase<string, NetString>)(object)tree.treeType).Value);
	}

	public override IEnumerable<IDebugField> GetDebugFields()
	{
		Tree target = this.Target;
		yield return new GenericDebugField("has seed", base.Stringify(((NetFieldBase<bool, NetBool>)(object)target.hasSeed).Value), null, pinned: true);
		yield return new GenericDebugField("growth stage", ((NetFieldBase<int, NetInt>)(object)target.growthStage).Value, null, pinned: true);
		yield return new GenericDebugField("health", ((NetFieldBase<float, NetFloat>)(object)target.health).Value, null, pinned: true);
		foreach (IDebugField item in base.GetDebugFieldsFrom(target))
		{
			yield return item;
		}
	}

	public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((TerrainFeature)this.Target).drawInMenu(spriteBatch, position, Vector2.Zero, 1f, 1f);
		return true;
	}

	private static string GetName(Tree tree)
	{
		return ((NetFieldBase<string, NetString>)(object)tree.treeType).Value switch
		{
			"7" => I18n.Tree_Name_BigMushroom(), 
			"8" => I18n.Tree_Name_Mahogany(), 
			"2" => I18n.Tree_Name_Maple(), 
			"1" => I18n.Tree_Name_Oak(), 
			"6" => I18n.Tree_Name_Palm(), 
			"9" => I18n.Tree_Name_Palm(), 
			"3" => I18n.Tree_Name_Pine(), 
			"10" => I18n.Tree_Name_Mossy(), 
			"11" => I18n.Tree_Name_Mossy(), 
			"12" => I18n.Tree_Name_Mossy(), 
			"13" => I18n.Tree_Name_Mystic(), 
			_ => I18n.Tree_Name_Unknown(), 
		};
	}

	private bool HasAdjacentTrees(Vector2 position)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		GameLocation location = Game1.currentLocation;
		return (from adjacentTile in Utility.getSurroundingTileLocationsArray(position)
			let otherTree = (Tree)(((NetDictionary<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>)(object)location.terrainFeatures).ContainsKey(adjacentTile) ? /*isinst with value type is only supported in some contexts*/: null)
			select otherTree != null && ((NetFieldBase<int, NetInt>)(object)otherTree.growthStage).Value >= 4).Any((bool p) => p);
	}
}
