using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles;

internal class IslandMermaidPuzzleSubject : TileSubject
{
	public IslandMermaidPuzzleSubject(GameHelper gameHelper, ModConfig config, GameLocation location, Vector2 position, bool showRawTileInfo)
		: base(gameHelper, config, location, position, showRawTileInfo)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		base.Name = I18n.Puzzle_IslandMermaid_Title();
		base.Description = null;
		base.Type = null;
	}

	public override IEnumerable<ICustomField> GetData()
	{
		IslandSouthEast location = (IslandSouthEast)base.Location;
		bool complete = ((NetFieldBase<bool, NetBool>)(object)location.mermaidPuzzleFinished).Value;
		if (!base.Config.ShowPuzzleSolutions && !complete)
		{
			yield return new GenericField(I18n.Puzzle_Solution(), I18n.Puzzle_Solution_Hidden());
		}
		else
		{
			int[] sequence = base.GameHelper.Metadata.PuzzleSolutions.IslandMermaidFluteBlockSequence;
			int songIndex = location.songIndex;
			Checkbox[] checkboxes = sequence.Select(delegate(int pitch, int i)
			{
				string text = base.Stringify(pitch);
				return new Checkbox(complete || songIndex >= i, text);
			}).ToArray();
			CheckboxList checkboxList = new CheckboxList(checkboxes);
			checkboxList.AddIntro(complete ? I18n.Puzzle_Solution_Solved() : I18n.Puzzle_IslandMermaid_Solution_Intro());
			yield return new CheckboxListField(I18n.Puzzle_Solution(), checkboxList);
		}
		foreach (ICustomField datum in base.GetData())
		{
			yield return datum;
		}
	}
}
