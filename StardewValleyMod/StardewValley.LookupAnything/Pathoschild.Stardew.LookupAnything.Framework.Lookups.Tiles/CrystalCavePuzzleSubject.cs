using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles;

internal class CrystalCavePuzzleSubject : TileSubject
{
	private readonly int? CrystalId;

	public CrystalCavePuzzleSubject(GameHelper gameHelper, ModConfig config, GameLocation location, Vector2 position, bool showRawTileInfo, int? crystalId)
		: base(gameHelper, config, location, position, showRawTileInfo)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		base.Name = I18n.Puzzle_IslandCrystalCave_Title();
		base.Description = null;
		base.Type = null;
		this.CrystalId = crystalId;
	}

	public override IEnumerable<ICustomField> GetData()
	{
		IslandWestCave1 cave = (IslandWestCave1)base.Location;
		if (this.CrystalId.HasValue && base.Config.ShowPuzzleSolutions)
		{
			yield return new GenericField(I18n.Puzzle_IslandCrystalCave_CrystalId(), base.Stringify(this.CrystalId.Value));
		}
		string label = I18n.Puzzle_Solution();
		if (((NetFieldBase<bool, NetBool>)(object)cave.completed).Value)
		{
			yield return new GenericField(label, I18n.Puzzle_Solution_Solved());
		}
		else if (!base.Config.ShowPuzzleSolutions)
		{
			yield return new GenericField(label, new FormattedText(I18n.Puzzle_Solution_Hidden(), Color.Gray));
		}
		else if (!((NetFieldBase<bool, NetBool>)(object)cave.isActivated).Value)
		{
			yield return new GenericField(label, I18n.Puzzle_IslandCrystalCave_Solution_NotActivated());
		}
		else if (!cave.currentCrystalSequence.Any())
		{
			yield return new GenericField(label, I18n.Puzzle_IslandCrystalCave_Solution_Waiting());
		}
		else
		{
			Checkbox[] checkboxes = ((IEnumerable<int>)cave.currentCrystalSequence).Select(delegate(int id, int index)
			{
				string text = base.Stringify(id + 1);
				return new Checkbox(((NetFieldBase<int, NetInt>)(object)cave.currentCrystalSequenceIndex).Value > index, text);
			}).ToArray();
			CheckboxList checkboxList = new CheckboxList(checkboxes);
			checkboxList.AddIntro(I18n.Puzzle_IslandCrystalCave_Solution_Activated());
			yield return new CheckboxListField(label, checkboxList);
		}
		foreach (ICustomField datum in base.GetData())
		{
			yield return datum;
		}
	}
}
