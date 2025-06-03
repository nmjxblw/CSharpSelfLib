using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Pathfinding;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class ScheduleField : GenericField
{
	private record ScheduleEntry(int Time, SchedulePathDescription Description);

	public ScheduleField(NPC npc, GameHelper gameHelper)
		: base(I18n.Npc_Schedule(), ScheduleField.GetText(npc, gameHelper))
	{
	}

	private static IEnumerable<IFormattedText> GetText(NPC npc, GameHelper gameHelper)
	{
		bool isFarmhand = !Context.IsMainPlayer;
		GameLocation location = ((Character)npc).currentLocation;
		if (isFarmhand && !(((location != null) ? new bool?(location.IsActiveLocation()) : ((bool?)null)) ?? false))
		{
			yield return new FormattedText(I18n.Npc_Schedule_Farmhand_UnknownPosition());
		}
		else
		{
			string locationName = ((location != null) ? gameHelper.GetLocationDisplayName(location.Name, location.GetData()) : "???");
			yield return new FormattedText(I18n.Npc_Schedule_CurrentPosition(locationName, ((Character)npc).TilePoint.X, ((Character)npc).TilePoint.Y));
		}
		yield return new FormattedText(Environment.NewLine + Environment.NewLine);
		if (isFarmhand)
		{
			yield return new FormattedText(I18n.Npc_Schedule_Farmhand_UnknownSchedule());
			yield break;
		}
		ScheduleEntry[] schedule = ScheduleField.FormatSchedule(npc.Schedule).ToArray();
		if (schedule.Length == 0)
		{
			yield return new FormattedText(I18n.Npc_Schedule_NoEntries());
			yield break;
		}
		if (npc.ignoreScheduleToday || !npc.followSchedule)
		{
			yield return new FormattedText(I18n.Npc_Schedule_NotFollowingSchedule());
			yield break;
		}
		int i = 0;
		while (i < schedule.Length)
		{
			schedule[i].Deconstruct(out int Time, out SchedulePathDescription Description);
			int time = Time;
			SchedulePathDescription entry = Description;
			string targetLocationName = entry.targetLocationName;
			GameLocation locationFromName = Game1.getLocationFromName(entry.targetLocationName);
			string locationName2 = gameHelper.GetLocationDisplayName(targetLocationName, (locationFromName != null) ? locationFromName.GetData() : null);
			bool isStarted = Game1.timeOfDay >= time;
			bool isFinished = i < schedule.Length - 1 && Game1.timeOfDay >= schedule[i + 1].Time;
			Color textColor = ((!isStarted) ? Color.Black : (isFinished ? Color.Gray : Color.Green));
			if (i > 0)
			{
				yield return new FormattedText(Environment.NewLine);
			}
			yield return new FormattedText(I18n.Npc_Schedule_Entry(CommonHelper.FormatTime(time), locationName2, entry.targetTile.X, entry.targetTile.Y), textColor);
			Time = i++;
		}
	}

	private static IEnumerable<ScheduleEntry> FormatSchedule(Dictionary<int, SchedulePathDescription>? schedule)
	{
		if (schedule == null)
		{
			yield break;
		}
		List<int> sortedKeys = schedule.Keys.OrderBy((int key) => key).ToList();
		string prevTargetLocationName = string.Empty;
		foreach (int time in sortedKeys)
		{
			if (schedule.TryGetValue(time, out SchedulePathDescription entry) && !(entry.targetLocationName == prevTargetLocationName))
			{
				prevTargetLocationName = entry.targetLocationName;
				yield return new ScheduleEntry(time, entry);
			}
		}
	}
}
