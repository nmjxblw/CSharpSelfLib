using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles;

internal class IslandShrinePuzzleSubject : TileSubject
{
	public IslandShrinePuzzleSubject(GameHelper gameHelper, ModConfig config, GameLocation location, Vector2 position, bool showRawTileInfo)
		: base(gameHelper, config, location, position, showRawTileInfo)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		base.Name = I18n.Puzzle_IslandShrine_Title();
		base.Description = null;
		base.Type = null;
	}

	public override IEnumerable<ICustomField> GetData()
	{
		IslandShrine shrine = (IslandShrine)base.Location;
		bool complete = ((NetFieldBase<bool, NetBool>)(object)shrine.puzzleFinished).Value;
		if (!base.Config.ShowPuzzleSolutions && !complete)
		{
			yield return new GenericField(I18n.Puzzle_Solution(), new FormattedText(I18n.Puzzle_Solution_Hidden(), Color.Gray));
		}
		else
		{
			CheckboxList checkboxList = new CheckboxList(new Checkbox[4]
			{
				new Checkbox(text: I18n.Puzzle_IslandShrine_Solution_North(((Item)((NetFieldBase<Object, NetRef<Object>>)(object)shrine.northPedestal.requiredItem).Value).DisplayName), isChecked: complete || ((NetFieldBase<bool, NetBool>)(object)shrine.northPedestal.match).Value),
				new Checkbox(text: I18n.Puzzle_IslandShrine_Solution_East(((Item)((NetFieldBase<Object, NetRef<Object>>)(object)shrine.eastPedestal.requiredItem).Value).DisplayName), isChecked: complete || ((NetFieldBase<bool, NetBool>)(object)shrine.eastPedestal.match).Value),
				new Checkbox(text: I18n.Puzzle_IslandShrine_Solution_South(((Item)((NetFieldBase<Object, NetRef<Object>>)(object)shrine.southPedestal.requiredItem).Value).DisplayName), isChecked: complete || ((NetFieldBase<bool, NetBool>)(object)shrine.southPedestal.match).Value),
				new Checkbox(text: I18n.Puzzle_IslandShrine_Solution_West(((Item)((NetFieldBase<Object, NetRef<Object>>)(object)shrine.westPedestal.requiredItem).Value).DisplayName), isChecked: complete || ((NetFieldBase<bool, NetBool>)(object)shrine.westPedestal.match).Value)
			});
			checkboxList.AddIntro(complete ? I18n.Puzzle_Solution_Solved() : I18n.Puzzle_IslandShrine_Solution());
			yield return new CheckboxListField(I18n.Puzzle_Solution(), checkboxList);
		}
		foreach (ICustomField datum in base.GetData())
		{
			yield return datum;
		}
	}
}
