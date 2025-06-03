using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewValley.GameData.Movies;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;

namespace StardewValley.Events;

/// <summary>Generates the event that plays when watching a movie at the <see cref="T:StardewValley.Locations.MovieTheater" />.</summary>
public class MovieTheaterScreeningEvent
{
	public int currentResponse;

	public List<List<Character>> playerAndGuestAudienceGroups;

	public Dictionary<int, Character> _responseOrder = new Dictionary<int, Character>();

	protected Dictionary<Character, Character> _whiteListDependencyLookup;

	protected Dictionary<Character, string> _characterResponses;

	public MovieData movieData;

	protected List<Farmer> _farmers;

	protected Dictionary<Character, MovieConcession> _concessionsData;

	public Event getMovieEvent(string movieId, List<List<Character>> player_and_guest_audience_groups, List<List<Character>> npcOnlyAudienceGroups, Dictionary<Character, MovieConcession> concessions_data = null)
	{
		this._concessionsData = concessions_data;
		this._responseOrder = new Dictionary<int, Character>();
		this._whiteListDependencyLookup = new Dictionary<Character, Character>();
		this._characterResponses = new Dictionary<Character, string>();
		this.movieData = MovieTheater.GetMovieDataById()[movieId];
		this.playerAndGuestAudienceGroups = player_and_guest_audience_groups;
		this.currentResponse = 0;
		StringBuilder sb = new StringBuilder();
		Random theaterRandom = Utility.CreateDaySaveRandom();
		sb.Append("movieScreenAmbience/-2000 -2000/");
		string playerCharacterEventName = "farmer" + Utility.getFarmerNumberFromFarmer(Game1.player);
		string playerCharacterGuestName = "";
		bool hasPlayerGuest = false;
		foreach (List<Character> list in this.playerAndGuestAudienceGroups)
		{
			if (!list.Contains(Game1.player))
			{
				continue;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (!(list[i] is Farmer))
				{
					playerCharacterGuestName = list[i].name.Value;
					hasPlayerGuest = true;
					break;
				}
			}
		}
		this._farmers = new List<Farmer>();
		foreach (List<Character> playerAndGuestAudienceGroup in this.playerAndGuestAudienceGroups)
		{
			foreach (Character item in playerAndGuestAudienceGroup)
			{
				if (item is Farmer player && !this._farmers.Contains(player))
				{
					this._farmers.Add(player);
				}
			}
		}
		List<Character> allAudience = this.playerAndGuestAudienceGroups.SelectMany((List<Character> x) => x).ToList();
		if (allAudience.Count <= 12)
		{
			allAudience.AddRange(npcOnlyAudienceGroups.SelectMany((List<Character> x) => x).ToList());
		}
		bool first = true;
		foreach (Character c in allAudience)
		{
			if (c != null)
			{
				if (!first)
				{
					sb.Append(' ');
				}
				if (c is Farmer f)
				{
					sb.Append("farmer").Append(Utility.getFarmerNumberFromFarmer(f));
				}
				else
				{
					sb.Append(c.name.Value);
				}
				sb.Append(" -1000 -1000 0");
				first = false;
			}
		}
		sb.Append("/changeToTemporaryMap MovieTheaterScreen false/specificTemporarySprite movieTheater_setup/ambientLight 0 0 0/");
		string[] backRow = new string[8];
		string[] midRow = new string[6];
		string[] frontRow = new string[4];
		this.playerAndGuestAudienceGroups = this.playerAndGuestAudienceGroups.OrderBy((List<Character> x) => theaterRandom.Next()).ToList();
		int startingSeat = theaterRandom.Next(8 - Math.Min(this.playerAndGuestAudienceGroups.SelectMany((List<Character> x) => x).Count(), 8) + 1);
		int whichGroup = 0;
		if (this.playerAndGuestAudienceGroups.Count > 0)
		{
			for (int i2 = 0; i2 < 8; i2++)
			{
				int seat = (i2 + startingSeat) % 8;
				if (this.playerAndGuestAudienceGroups[whichGroup].Count == 2 && (seat == 3 || seat == 7))
				{
					i2++;
					seat++;
					seat %= 8;
				}
				for (int j = 0; j < this.playerAndGuestAudienceGroups[whichGroup].Count && seat + j < backRow.Length; j++)
				{
					backRow[seat + j] = ((this.playerAndGuestAudienceGroups[whichGroup][j] is Farmer) ? ("farmer" + Utility.getFarmerNumberFromFarmer(this.playerAndGuestAudienceGroups[whichGroup][j] as Farmer)) : this.playerAndGuestAudienceGroups[whichGroup][j].name.Value);
					if (j > 0)
					{
						i2++;
					}
				}
				whichGroup++;
				if (whichGroup >= this.playerAndGuestAudienceGroups.Count)
				{
					break;
				}
			}
		}
		else
		{
			Game1.log.Warn("The movie audience somehow has no players. This is likely a bug.");
		}
		bool usedMidRow = false;
		if (whichGroup < this.playerAndGuestAudienceGroups.Count)
		{
			startingSeat = 0;
			for (int i3 = 0; i3 < 4; i3++)
			{
				int seat2 = (i3 + startingSeat) % 4;
				for (int j2 = 0; j2 < this.playerAndGuestAudienceGroups[whichGroup].Count && seat2 + j2 < frontRow.Length; j2++)
				{
					frontRow[seat2 + j2] = ((this.playerAndGuestAudienceGroups[whichGroup][j2] is Farmer) ? ("farmer" + Utility.getFarmerNumberFromFarmer(this.playerAndGuestAudienceGroups[whichGroup][j2] as Farmer)) : this.playerAndGuestAudienceGroups[whichGroup][j2].name.Value);
					if (j2 > 0)
					{
						i3++;
					}
				}
				whichGroup++;
				if (whichGroup >= this.playerAndGuestAudienceGroups.Count)
				{
					break;
				}
			}
			if (whichGroup < this.playerAndGuestAudienceGroups.Count)
			{
				usedMidRow = true;
				startingSeat = 0;
				for (int i4 = 0; i4 < 6; i4++)
				{
					int seat3 = (i4 + startingSeat) % 6;
					if (this.playerAndGuestAudienceGroups[whichGroup].Count == 2 && seat3 == 2)
					{
						i4++;
						seat3++;
						seat3 %= 8;
					}
					for (int j3 = 0; j3 < this.playerAndGuestAudienceGroups[whichGroup].Count && seat3 + j3 < midRow.Length; j3++)
					{
						midRow[seat3 + j3] = ((this.playerAndGuestAudienceGroups[whichGroup][j3] is Farmer) ? ("farmer" + Utility.getFarmerNumberFromFarmer(this.playerAndGuestAudienceGroups[whichGroup][j3] as Farmer)) : this.playerAndGuestAudienceGroups[whichGroup][j3].name.Value);
						if (j3 > 0)
						{
							i4++;
						}
					}
					whichGroup++;
					if (whichGroup >= this.playerAndGuestAudienceGroups.Count)
					{
						break;
					}
				}
			}
		}
		if (!usedMidRow)
		{
			for (int j4 = 0; j4 < npcOnlyAudienceGroups.Count; j4++)
			{
				int seat4 = theaterRandom.Next(3 - npcOnlyAudienceGroups[j4].Count + 1) + j4 * 3;
				for (int i5 = 0; i5 < npcOnlyAudienceGroups[j4].Count; i5++)
				{
					midRow[seat4 + i5] = npcOnlyAudienceGroups[j4][i5].name.Value;
				}
			}
		}
		int soFar = 0;
		int sittingTogetherCount = 0;
		for (int i6 = 0; i6 < backRow.Length; i6++)
		{
			if (string.IsNullOrEmpty(backRow[i6]) || !(backRow[i6] != playerCharacterEventName) || !(backRow[i6] != playerCharacterGuestName))
			{
				continue;
			}
			soFar++;
			if (soFar < 2)
			{
				continue;
			}
			sittingTogetherCount++;
			Point seat5 = this.getBackRowSeatTileFromIndex(i6);
			sb.Append("warp ").Append(backRow[i6]).Append(' ')
				.Append(seat5.X)
				.Append(' ')
				.Append(seat5.Y)
				.Append("/positionOffset ")
				.Append(backRow[i6])
				.Append(" 0 -10/");
			if (sittingTogetherCount == 2)
			{
				sittingTogetherCount = 0;
				if (theaterRandom.NextBool() && backRow[i6] != playerCharacterGuestName && backRow[i6 - 1] != playerCharacterGuestName && backRow[i6 - 1] != null)
				{
					sb.Append("faceDirection ").Append(backRow[i6]).Append(" 3 true/");
					sb.Append("faceDirection ").Append(backRow[i6 - 1]).Append(" 1 true/");
				}
			}
		}
		soFar = 0;
		sittingTogetherCount = 0;
		for (int i7 = 0; i7 < midRow.Length; i7++)
		{
			if (string.IsNullOrEmpty(midRow[i7]) || !(midRow[i7] != playerCharacterEventName) || !(midRow[i7] != playerCharacterGuestName))
			{
				continue;
			}
			soFar++;
			if (soFar < 2)
			{
				continue;
			}
			sittingTogetherCount++;
			Point seat6 = this.getMidRowSeatTileFromIndex(i7);
			sb.Append("warp ").Append(midRow[i7]).Append(' ')
				.Append(seat6.X)
				.Append(' ')
				.Append(seat6.Y)
				.Append("/positionOffset ")
				.Append(midRow[i7])
				.Append(" 0 -10/");
			if (sittingTogetherCount == 2)
			{
				sittingTogetherCount = 0;
				if (i7 != 3 && theaterRandom.NextBool() && midRow[i7 - 1] != null)
				{
					sb.Append("faceDirection ").Append(midRow[i7]).Append(" 3 true/");
					sb.Append("faceDirection ").Append(midRow[i7 - 1]).Append(" 1 true/");
				}
			}
		}
		soFar = 0;
		sittingTogetherCount = 0;
		for (int i8 = 0; i8 < frontRow.Length; i8++)
		{
			if (string.IsNullOrEmpty(frontRow[i8]) || !(frontRow[i8] != playerCharacterEventName) || !(frontRow[i8] != playerCharacterGuestName))
			{
				continue;
			}
			soFar++;
			if (soFar < 2)
			{
				continue;
			}
			sittingTogetherCount++;
			Point seat7 = this.getFrontRowSeatTileFromIndex(i8);
			sb.Append("warp ").Append(frontRow[i8]).Append(' ')
				.Append(seat7.X)
				.Append(' ')
				.Append(seat7.Y)
				.Append("/positionOffset ")
				.Append(frontRow[i8])
				.Append(" 0 -10/");
			if (sittingTogetherCount == 2)
			{
				sittingTogetherCount = 0;
				if (theaterRandom.NextBool() && frontRow[i8 - 1] != null)
				{
					sb.Append("faceDirection ").Append(frontRow[i8]).Append(" 3 true/");
					sb.Append("faceDirection ").Append(frontRow[i8 - 1]).Append(" 1 true/");
				}
			}
		}
		Point warpPoint = new Point(1, 15);
		soFar = 0;
		for (int i9 = 0; i9 < backRow.Length; i9++)
		{
			if (!string.IsNullOrEmpty(backRow[i9]) && backRow[i9] != playerCharacterEventName && backRow[i9] != playerCharacterGuestName)
			{
				Point seat8 = this.getBackRowSeatTileFromIndex(i9);
				if (soFar == 1)
				{
					sb.Append("warp ").Append(backRow[i9]).Append(' ')
						.Append(seat8.X - 1)
						.Append(" 10")
						.Append("/advancedMove ")
						.Append(backRow[i9])
						.Append(" false 1 ")
						.Append(200)
						.Append(" 1 0 4 1000/")
						.Append("positionOffset ")
						.Append(backRow[i9])
						.Append(" 0 -10/");
				}
				else
				{
					sb.Append("warp ").Append(backRow[i9]).Append(" 1 12")
						.Append("/advancedMove ")
						.Append(backRow[i9])
						.Append(" false 1 200 ")
						.Append("0 -2 ")
						.Append(seat8.X - 1)
						.Append(" 0 4 1000/")
						.Append("positionOffset ")
						.Append(backRow[i9])
						.Append(" 0 -10/");
				}
				soFar++;
			}
			if (soFar >= 2)
			{
				break;
			}
		}
		soFar = 0;
		for (int i10 = 0; i10 < midRow.Length; i10++)
		{
			if (!string.IsNullOrEmpty(midRow[i10]) && midRow[i10] != playerCharacterEventName && midRow[i10] != playerCharacterGuestName)
			{
				Point seat9 = this.getMidRowSeatTileFromIndex(i10);
				if (soFar == 1)
				{
					sb.Append("warp ").Append(midRow[i10]).Append(' ')
						.Append(seat9.X - 1)
						.Append(" 8")
						.Append("/advancedMove ")
						.Append(midRow[i10])
						.Append(" false 1 ")
						.Append(400)
						.Append(" 1 0 4 1000/");
				}
				else
				{
					sb.Append("warp ").Append(midRow[i10]).Append(" 2 9")
						.Append("/advancedMove ")
						.Append(midRow[i10])
						.Append(" false 1 300 ")
						.Append("0 -1 ")
						.Append(seat9.X - 2)
						.Append(" 0 4 1000/");
				}
				soFar++;
			}
			if (soFar >= 2)
			{
				break;
			}
		}
		soFar = 0;
		for (int i11 = 0; i11 < frontRow.Length; i11++)
		{
			if (!string.IsNullOrEmpty(frontRow[i11]) && frontRow[i11] != playerCharacterEventName && frontRow[i11] != playerCharacterGuestName)
			{
				Point seat10 = this.getFrontRowSeatTileFromIndex(i11);
				if (soFar == 1)
				{
					sb.Append("warp ").Append(frontRow[i11]).Append(' ')
						.Append(seat10.X - 1)
						.Append(" 6")
						.Append("/advancedMove ")
						.Append(frontRow[i11])
						.Append(" false 1 ")
						.Append(400)
						.Append(" 1 0 4 1000/");
				}
				else
				{
					sb.Append("warp ").Append(frontRow[i11]).Append(" 3 7")
						.Append("/advancedMove ")
						.Append(frontRow[i11])
						.Append(" false 1 300 ")
						.Append("0 -1 ")
						.Append(seat10.X - 3)
						.Append(" 0 4 1000/");
				}
				soFar++;
			}
			if (soFar >= 2)
			{
				break;
			}
		}
		sb.Append("viewport 6 8 true/pause 500/");
		for (int i12 = 0; i12 < backRow.Length; i12++)
		{
			if (!string.IsNullOrEmpty(backRow[i12]))
			{
				Point seat11 = this.getBackRowSeatTileFromIndex(i12);
				if (backRow[i12] == playerCharacterEventName || backRow[i12] == playerCharacterGuestName)
				{
					sb.Append("warp ").Append(backRow[i12]).Append(' ')
						.Append(warpPoint.X)
						.Append(' ')
						.Append(warpPoint.Y)
						.Append("/advancedMove ")
						.Append(backRow[i12])
						.Append(" false 0 -5 ")
						.Append(seat11.X - warpPoint.X)
						.Append(" 0 4 1000/")
						.Append("pause ")
						.Append(1000)
						.Append("/");
				}
			}
		}
		for (int i13 = 0; i13 < midRow.Length; i13++)
		{
			if (!string.IsNullOrEmpty(midRow[i13]))
			{
				Point seat12 = this.getMidRowSeatTileFromIndex(i13);
				if (midRow[i13] == playerCharacterEventName || midRow[i13] == playerCharacterGuestName)
				{
					sb.Append("warp ").Append(midRow[i13]).Append(' ')
						.Append(warpPoint.X)
						.Append(' ')
						.Append(warpPoint.Y)
						.Append("/advancedMove ")
						.Append(midRow[i13])
						.Append(" false 0 -7 ")
						.Append(seat12.X - warpPoint.X)
						.Append(" 0 4 1000/")
						.Append("pause ")
						.Append(1000)
						.Append("/");
				}
			}
		}
		for (int i14 = 0; i14 < frontRow.Length; i14++)
		{
			if (!string.IsNullOrEmpty(frontRow[i14]))
			{
				Point seat13 = this.getFrontRowSeatTileFromIndex(i14);
				if (frontRow[i14] == playerCharacterEventName || frontRow[i14] == playerCharacterGuestName)
				{
					sb.Append("warp ").Append(frontRow[i14]).Append(' ')
						.Append(warpPoint.X)
						.Append(' ')
						.Append(warpPoint.Y)
						.Append("/advancedMove ")
						.Append(frontRow[i14])
						.Append(" false 0 -7 1 0 0 -1 1 0 0 -1 ")
						.Append(seat13.X - 3)
						.Append(" 0 4 1000/")
						.Append("pause ")
						.Append(1000)
						.Append("/");
				}
			}
		}
		sb.Append("pause 3000");
		if (hasPlayerGuest)
		{
			sb.Append("/proceedPosition ").Append(playerCharacterGuestName);
		}
		sb.Append("/pause 1000");
		if (!hasPlayerGuest)
		{
			sb.Append("/proceedPosition farmer");
		}
		sb.Append("/waitForAllStationary/pause 100");
		foreach (Character c2 in allAudience)
		{
			string actorName = MovieTheaterScreeningEvent.getEventName(c2);
			if (actorName != playerCharacterEventName && actorName != playerCharacterGuestName)
			{
				if (c2 is Farmer)
				{
					sb.Append("/faceDirection ").Append(actorName).Append(" 0 true/positionOffset ")
						.Append(actorName)
						.Append(" 0 42 true");
				}
				else
				{
					sb.Append("/faceDirection ").Append(actorName).Append(" 0 true/positionOffset ")
						.Append(actorName)
						.Append(" 0 12 true");
				}
				if (theaterRandom.NextDouble() < 0.2)
				{
					sb.Append("/pause 100");
				}
			}
		}
		sb.Append("/positionOffset ").Append(playerCharacterEventName).Append(" 0 32");
		if (hasPlayerGuest)
		{
			sb.Append("/positionOffset ").Append(playerCharacterGuestName).Append(" 0 8");
		}
		sb.Append("/ambientLight 210 210 120 true/pause 500/viewport move 0 -1 4000/pause 5000");
		List<Character> responding_characters = new List<Character>();
		foreach (List<Character> playerAndGuestAudienceGroup2 in this.playerAndGuestAudienceGroups)
		{
			foreach (Character character in playerAndGuestAudienceGroup2)
			{
				if (!(character is Farmer) && !responding_characters.Contains(character))
				{
					responding_characters.Add(character);
				}
			}
		}
		for (int i15 = 0; i15 < responding_characters.Count; i15++)
		{
			int index = theaterRandom.Next(responding_characters.Count);
			Character character2 = responding_characters[i15];
			responding_characters[i15] = responding_characters[index];
			responding_characters[index] = character2;
		}
		int current_response_index = 0;
		foreach (MovieScene scene in this.movieData.Scenes)
		{
			if (scene.ResponsePoint == null)
			{
				continue;
			}
			bool found_reaction = false;
			for (int i16 = 0; i16 < responding_characters.Count; i16++)
			{
				MovieCharacterReaction reaction = MovieTheater.GetReactionsForCharacter(responding_characters[i16] as NPC);
				if (reaction == null)
				{
					continue;
				}
				foreach (MovieReaction movie_reaction in reaction.Reactions)
				{
					if (!movie_reaction.ShouldApplyToMovie(this.movieData, MovieTheater.GetPatronNames(), MovieTheater.GetResponseForMovie(responding_characters[i16] as NPC)) || movie_reaction.SpecialResponses?.DuringMovie == null || (!(movie_reaction.SpecialResponses.DuringMovie.ResponsePoint == scene.ResponsePoint) && movie_reaction.Whitelist.Count <= 0))
					{
						continue;
					}
					if (!this._whiteListDependencyLookup.ContainsKey(responding_characters[i16]))
					{
						this._responseOrder[current_response_index] = responding_characters[i16];
						if (movie_reaction.Whitelist != null)
						{
							for (int j5 = 0; j5 < movie_reaction.Whitelist.Count; j5++)
							{
								Character white_list_character = Game1.getCharacterFromName(movie_reaction.Whitelist[j5]);
								if (white_list_character == null)
								{
									continue;
								}
								this._whiteListDependencyLookup[white_list_character] = responding_characters[i16];
								foreach (int key in this._responseOrder.Keys)
								{
									if (this._responseOrder[key] == white_list_character)
									{
										this._responseOrder.Remove(key);
									}
								}
							}
						}
					}
					responding_characters.RemoveAt(i16);
					i16--;
					found_reaction = true;
					break;
				}
				if (found_reaction)
				{
					break;
				}
			}
			if (!found_reaction)
			{
				for (int i17 = 0; i17 < responding_characters.Count; i17++)
				{
					MovieCharacterReaction reaction2 = MovieTheater.GetReactionsForCharacter(responding_characters[i17] as NPC);
					if (reaction2 == null)
					{
						continue;
					}
					foreach (MovieReaction movie_reaction2 in reaction2.Reactions)
					{
						if (!movie_reaction2.ShouldApplyToMovie(this.movieData, MovieTheater.GetPatronNames(), MovieTheater.GetResponseForMovie(responding_characters[i17] as NPC)) || movie_reaction2.SpecialResponses?.DuringMovie == null || !(movie_reaction2.SpecialResponses.DuringMovie.ResponsePoint == current_response_index.ToString()))
						{
							continue;
						}
						if (!this._whiteListDependencyLookup.ContainsKey(responding_characters[i17]))
						{
							this._responseOrder[current_response_index] = responding_characters[i17];
							if (movie_reaction2.Whitelist != null)
							{
								for (int j6 = 0; j6 < movie_reaction2.Whitelist.Count; j6++)
								{
									Character white_list_character2 = Game1.getCharacterFromName(movie_reaction2.Whitelist[j6]);
									if (white_list_character2 == null)
									{
										continue;
									}
									this._whiteListDependencyLookup[white_list_character2] = responding_characters[i17];
									foreach (int key2 in this._responseOrder.Keys)
									{
										if (this._responseOrder[key2] == white_list_character2)
										{
											this._responseOrder.Remove(key2);
										}
									}
								}
							}
						}
						responding_characters.RemoveAt(i17);
						i17--;
						found_reaction = true;
						break;
					}
					if (found_reaction)
					{
						break;
					}
				}
			}
			current_response_index++;
		}
		current_response_index = 0;
		for (int i18 = 0; i18 < responding_characters.Count; i18++)
		{
			if (!this._whiteListDependencyLookup.ContainsKey(responding_characters[i18]))
			{
				for (; this._responseOrder.ContainsKey(current_response_index); current_response_index++)
				{
				}
				this._responseOrder[current_response_index] = responding_characters[i18];
				current_response_index++;
			}
		}
		responding_characters = null;
		foreach (MovieScene scene2 in this.movieData.Scenes)
		{
			this._ParseScene(sb, scene2);
		}
		while (this.currentResponse < this._responseOrder.Count)
		{
			this._ParseResponse(sb);
		}
		sb.Append("/stopMusic");
		sb.Append("/fade/viewport -1000 -1000");
		sb.Append("/pause 500/message \"").Append(Game1.content.LoadString("Strings\\Locations:Theater_MovieEnd")).Append("\"/pause 500");
		sb.Append("/requestMovieEnd");
		return new Event(sb.ToString(), null, "MovieTheaterScreening");
	}

	protected void _ParseScene(StringBuilder sb, MovieScene scene)
	{
		if (!string.IsNullOrWhiteSpace(scene.Sound))
		{
			sb.Append("/playSound ").Append(scene.Sound);
		}
		if (!string.IsNullOrWhiteSpace(scene.Music))
		{
			sb.Append("/playMusic ").Append(scene.Music);
		}
		if (scene.MessageDelay > 0)
		{
			sb.Append("/pause ").Append(scene.MessageDelay);
		}
		if (scene.Image >= 0)
		{
			sb.Append("/specificTemporarySprite movieTheater_screen ").Append(this.movieData.Id).Append(' ')
				.Append(scene.Image)
				.Append(' ')
				.Append(scene.Shake);
			if (this.movieData.Texture != null)
			{
				sb.Append(" \"").Append(ArgUtility.EscapeQuotes(this.movieData.Texture)).Append('"');
			}
		}
		if (!string.IsNullOrWhiteSpace(scene.Script))
		{
			sb.Append(TokenParser.ParseText(scene.Script));
		}
		if (!string.IsNullOrWhiteSpace(scene.Text))
		{
			sb.Append("/message \"").Append(ArgUtility.EscapeQuotes(TokenParser.ParseText(scene.Text))).Append('"');
		}
		if (scene.ResponsePoint != null)
		{
			this._ParseResponse(sb, scene);
		}
	}

	protected void _ParseResponse(StringBuilder sb, MovieScene scene = null)
	{
		if (this._responseOrder.TryGetValue(this.currentResponse, out var responding_character))
		{
			sb.Append("/pause 500");
			bool hadUniqueScript = false;
			if (!this._whiteListDependencyLookup.ContainsKey(responding_character))
			{
				MovieCharacterReaction reaction = MovieTheater.GetReactionsForCharacter(responding_character as NPC);
				if (reaction != null)
				{
					foreach (MovieReaction movie_reaction in reaction.Reactions)
					{
						if (movie_reaction.ShouldApplyToMovie(this.movieData, MovieTheater.GetPatronNames(), MovieTheater.GetResponseForMovie(responding_character as NPC)) && movie_reaction.SpecialResponses?.DuringMovie != null && (string.IsNullOrEmpty(movie_reaction.SpecialResponses.DuringMovie.ResponsePoint) || (scene != null && movie_reaction.SpecialResponses.DuringMovie.ResponsePoint == scene.ResponsePoint) || movie_reaction.SpecialResponses.DuringMovie.ResponsePoint == this.currentResponse.ToString() || movie_reaction.Whitelist.Count > 0))
						{
							string script = TokenParser.ParseText(movie_reaction.SpecialResponses.DuringMovie.Script);
							string text = TokenParser.ParseText(movie_reaction.SpecialResponses.DuringMovie.Text);
							if (!string.IsNullOrWhiteSpace(script))
							{
								sb.Append(script);
								hadUniqueScript = true;
							}
							if (!string.IsNullOrWhiteSpace(text))
							{
								sb.Append("/speak ").Append(responding_character.name.Value).Append(" \"")
									.Append(text)
									.Append('"');
							}
							break;
						}
					}
				}
			}
			this._ParseCharacterResponse(sb, responding_character, hadUniqueScript);
			foreach (Character key in this._whiteListDependencyLookup.Keys)
			{
				if (this._whiteListDependencyLookup[key] == responding_character)
				{
					this._ParseCharacterResponse(sb, key);
				}
			}
		}
		this.currentResponse++;
	}

	protected void _ParseCharacterResponse(StringBuilder sb, Character responding_character, bool ignoreScript = false)
	{
		string response = MovieTheater.GetResponseForMovie(responding_character as NPC);
		if (this._whiteListDependencyLookup.TryGetValue(responding_character, out var requestingCharacter))
		{
			response = MovieTheater.GetResponseForMovie(requestingCharacter as NPC);
		}
		switch (response)
		{
		case "love":
			sb.Append("/friendship ").Append(responding_character.Name).Append(' ')
				.Append(200);
			if (!ignoreScript)
			{
				sb.Append("/playSound reward/emote ").Append(responding_character.name.Value).Append(' ')
					.Append(20)
					.Append("/message \"")
					.Append(Game1.content.LoadString("Strings\\Characters:MovieTheater_LoveMovie", responding_character.displayName))
					.Append('"');
			}
			break;
		case "like":
			sb.Append("/friendship ").Append(responding_character.Name).Append(' ')
				.Append(100);
			if (!ignoreScript)
			{
				sb.Append("/playSound give_gift/emote ").Append(responding_character.name.Value).Append(' ')
					.Append(56)
					.Append("/message \"")
					.Append(Game1.content.LoadString("Strings\\Characters:MovieTheater_LikeMovie", responding_character.displayName))
					.Append('"');
			}
			break;
		case "dislike":
			sb.Append("/friendship ").Append(responding_character.Name).Append(' ')
				.Append(0);
			if (!ignoreScript)
			{
				sb.Append("/playSound newArtifact/emote ").Append(responding_character.name.Value).Append(' ')
					.Append(24)
					.Append("/message \"")
					.Append(Game1.content.LoadString("Strings\\Characters:MovieTheater_DislikeMovie", responding_character.displayName))
					.Append('"');
			}
			break;
		}
		if (this._concessionsData != null && this._concessionsData.TryGetValue(responding_character, out var concession))
		{
			string concession_response = MovieTheater.GetConcessionTasteForCharacter(responding_character, concession);
			string gender_tag = "";
			if (NPC.TryGetData(responding_character.name.Value, out var npcData))
			{
				switch (npcData.Gender)
				{
				case Gender.Female:
					gender_tag = "_Female";
					break;
				case Gender.Male:
					gender_tag = "_Male";
					break;
				}
			}
			string sound = "eat";
			if (concession.Tags != null && concession.Tags.Contains("Drink"))
			{
				sound = "gulp";
			}
			switch (concession_response)
			{
			case "love":
				sb.Append("/friendship ").Append(responding_character.Name).Append(' ')
					.Append(50);
				sb.Append("/tossConcession ").Append(responding_character.Name).Append(' ')
					.Append(concession.Id)
					.Append("/pause 1000");
				sb.Append("/playSound ").Append(sound).Append("/shake ")
					.Append(responding_character.Name)
					.Append(" 500/pause 1000");
				sb.Append("/playSound reward/emote ").Append(responding_character.name.Value).Append(' ')
					.Append(20)
					.Append("/message \"")
					.Append(Game1.content.LoadString("Strings\\Characters:MovieTheater_LoveConcession" + gender_tag, responding_character.displayName, concession.DisplayName))
					.Append('"');
				break;
			case "like":
				sb.Append("/friendship ").Append(responding_character.Name).Append(' ')
					.Append(25);
				sb.Append("/tossConcession ").Append(responding_character.Name).Append(' ')
					.Append(concession.Id)
					.Append("/pause 1000");
				sb.Append("/playSound ").Append(sound).Append("/shake ")
					.Append(responding_character.Name)
					.Append(" 500/pause 1000");
				sb.Append("/playSound give_gift/emote ").Append(responding_character.name.Value).Append(' ')
					.Append(56)
					.Append("/message \"")
					.Append(Game1.content.LoadString("Strings\\Characters:MovieTheater_LikeConcession" + gender_tag, responding_character.displayName, concession.DisplayName))
					.Append('"');
				break;
			case "dislike":
				sb.Append("/friendship ").Append(responding_character.Name).Append(' ')
					.Append(0);
				sb.Append("/playSound croak/pause 1000");
				sb.Append("/playSound newArtifact/emote ").Append(responding_character.name.Value).Append(' ')
					.Append(40)
					.Append("/message \"")
					.Append(Game1.content.LoadString("Strings\\Characters:MovieTheater_DislikeConcession" + gender_tag, responding_character.displayName, concession.DisplayName))
					.Append('"');
				break;
			}
		}
		this._characterResponses[responding_character] = response;
	}

	public Dictionary<Character, string> GetCharacterResponses()
	{
		return this._characterResponses;
	}

	private static string getEventName(Character c)
	{
		if (c is Farmer player)
		{
			return "farmer" + Utility.getFarmerNumberFromFarmer(player);
		}
		return c.name.Value;
	}

	private Point getBackRowSeatTileFromIndex(int index)
	{
		return index switch
		{
			0 => new Point(2, 10), 
			1 => new Point(3, 10), 
			2 => new Point(4, 10), 
			3 => new Point(5, 10), 
			4 => new Point(8, 10), 
			5 => new Point(9, 10), 
			6 => new Point(10, 10), 
			7 => new Point(11, 10), 
			_ => new Point(4, 12), 
		};
	}

	private Point getMidRowSeatTileFromIndex(int index)
	{
		return index switch
		{
			0 => new Point(3, 8), 
			1 => new Point(4, 8), 
			2 => new Point(5, 8), 
			3 => new Point(8, 8), 
			4 => new Point(9, 8), 
			5 => new Point(10, 8), 
			_ => new Point(4, 12), 
		};
	}

	private Point getFrontRowSeatTileFromIndex(int index)
	{
		return index switch
		{
			0 => new Point(4, 6), 
			1 => new Point(5, 6), 
			2 => new Point(8, 6), 
			3 => new Point(9, 6), 
			_ => new Point(4, 12), 
		};
	}
}
