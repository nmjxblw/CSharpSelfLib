using System.Collections.Generic;
using System.IO;
using Netcode;

namespace StardewValley;

public class StartMovieEvent : NetEventArg
{
	public long uid;

	public List<List<Character>> playerGroups;

	public List<List<Character>> npcGroups;

	public StartMovieEvent()
	{
	}

	public StartMovieEvent(long farmer_uid, List<List<Character>> player_groups, List<List<Character>> npc_groups)
	{
		this.uid = farmer_uid;
		this.playerGroups = player_groups;
		this.npcGroups = npc_groups;
	}

	public void Read(BinaryReader reader)
	{
		this.uid = reader.ReadInt64();
		this.playerGroups = this.ReadCharacterList(reader);
		this.npcGroups = this.ReadCharacterList(reader);
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(this.uid);
		this.WriteCharacterList(writer, this.playerGroups);
		this.WriteCharacterList(writer, this.npcGroups);
	}

	public List<List<Character>> ReadCharacterList(BinaryReader reader)
	{
		List<List<Character>> group_list = new List<List<Character>>();
		int group_list_count = reader.ReadInt32();
		for (int i = 0; i < group_list_count; i++)
		{
			List<Character> group = new List<Character>();
			int group_count = reader.ReadInt32();
			for (int j = 0; j < group_count; j++)
			{
				Character character = ((reader.ReadInt32() == 1) ? ((Character)(Game1.GetPlayer(reader.ReadInt64(), onlyOnline: true) ?? Game1.MasterPlayer)) : ((Character)Game1.getCharacterFromName(reader.ReadString())));
				group.Add(character);
			}
			group_list.Add(group);
		}
		return group_list;
	}

	public void WriteCharacterList(BinaryWriter writer, List<List<Character>> group_list)
	{
		writer.Write(group_list.Count);
		foreach (List<Character> group in group_list)
		{
			writer.Write(group.Count);
			foreach (Character character in group)
			{
				if (character is Farmer player)
				{
					writer.Write(1);
					writer.Write(player.UniqueMultiplayerID);
				}
				else
				{
					writer.Write(0);
					writer.Write(character.Name);
				}
			}
		}
	}
}
