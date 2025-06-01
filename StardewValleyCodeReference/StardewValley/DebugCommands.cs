using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Events;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Movies;
using StardewValley.GameData.Pets;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Network.Compress;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.SaveMigrations;
using StardewValley.SpecialOrders;
using StardewValley.SpecialOrders.Objectives;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using StardewValley.Triggers;
using StardewValley.Util;
using StardewValley.WorldMaps;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley;

/// <summary>The debug commands that can be executed through the console.</summary>
/// <remarks>See also <see cref="T:StardewValley.ChatCommands" />.</remarks>
/// <summary>The debug commands that can be executed through the console.</summary>
/// <remarks>See also <see cref="T:StardewValley.ChatCommands" />.</remarks>
public static class DebugCommands
{
	/// <summary>The low-level handlers for vanilla debug commands. Most code should call <see cref="M:StardewValley.DebugCommands.TryHandle(System.String[],StardewValley.Logging.IGameLogger)" /> instead, which adds error-handling.</summary>
	public static class DefaultHandlers
	{
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GrowWildTrees(string[] command, IGameLogger log)
		{
			TerrainFeature[] array = Game1.currentLocation.terrainFeatures.Values.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] is Tree tree)
				{
					tree.growthStage.Value = 4;
					tree.fertilized.Value = true;
					tree.dayUpdate();
					tree.fertilized.Value = false;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Emote(string[] command, IGameLogger log)
		{
			for (int i = 1; i < command.Length; i += 2)
			{
				if (!ArgUtility.TryGet(command, i, out var npcName, out var error, allowBlank: false, "string npcName") || !ArgUtility.TryGetInt(command, i + 1, out var emoteId, out error, "int emoteId"))
				{
					log.Warn(error);
					continue;
				}
				NPC npc = Utility.fuzzyCharacterSearch(npcName, must_be_villager: false);
				if (npc == null)
				{
					log.Error("Couldn't find character named " + npcName);
				}
				else
				{
					npc.doEmote(emoteId);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void EventTestSpecific(string[] command, IGameLogger log)
		{
			Game1.eventTest = new EventTest(command);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void EventTest(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptional(command, 1, out var locationName, out var error, null, allowBlank: true, "string locationName") || !ArgUtility.TryGetOptionalInt(command, 2, out var startingEventIndex, out error, 0, "int startingEventIndex"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.eventTest = new EventTest(locationName ?? "", startingEventIndex);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GetAllQuests(string[] command, IGameLogger log)
		{
			foreach (KeyValuePair<string, string> v in DataLoader.Quests(Game1.content))
			{
				Game1.player.addQuest(v.Key);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Movie(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptional(command, 1, out var movieId, out var error, null, allowBlank: false, "string movieId") || !ArgUtility.TryGetOptional(command, 2, out var invitedNpcName, out error, null, allowBlank: false, "string invitedNpcName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			if (movieId != null && !MovieTheater.TryGetMovieData(movieId, out var _))
			{
				log.Error("No movie found with ID '" + movieId + "'.");
				return;
			}
			if (invitedNpcName != null)
			{
				NPC npc = Utility.fuzzyCharacterSearch(invitedNpcName);
				if (npc != null)
				{
					MovieTheater.Invite(Game1.player, npc);
				}
				else
				{
					log.Error("No NPC found matching '" + invitedNpcName + "'.");
				}
			}
			if (movieId != null)
			{
				MovieTheater.forceMovieId = movieId;
			}
			LocationRequest locationRequest = Game1.getLocationRequest("MovieTheater");
			locationRequest.OnWarp += delegate
			{
				((MovieTheater)Game1.currentLocation).performAction("Theater_Doors", Game1.player, Location.Origin);
			};
			Game1.warpFarmer(locationRequest, 10, 10, 0);
		}

		/// <summary>Print the movie schedule for a specified year.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MovieSchedule(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalInt(command, 1, out var year, out var error, Game1.year, "int year"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(20, 1, stringBuilder);
			handler.AppendLiteral("Movie schedule for ");
			handler.AppendFormatted((year == Game1.year) ? $"this year (year {year})" : $"year {year}");
			handler.AppendLiteral(":");
			StringBuilder schedule = stringBuilder.AppendLine(ref handler).AppendLine();
			Season[] array = new Season[4]
			{
				StardewValley.Season.Spring,
				StardewValley.Season.Summer,
				StardewValley.Season.Fall,
				StardewValley.Season.Winter
			};
			foreach (Season season in array)
			{
				List<Tuple<MovieData, int>> movies = new List<Tuple<MovieData, int>>();
				string lastMovieId = null;
				for (int day = 1; day <= 28; day++)
				{
					MovieData movie = MovieTheater.GetMovieForDate(new WorldDate(year, season, day));
					if (movie.Id != lastMovieId)
					{
						movies.Add(Tuple.Create(movie, day));
						lastMovieId = movie.Id;
					}
				}
				for (int j = 0; j < movies.Count; j++)
				{
					MovieData item = movies[j].Item1;
					int startDay = movies[j].Item2;
					int endDay = ((movies.Count > j + 1) ? (movies[j + 1].Item2 - 1) : 28);
					string title = TokenParser.ParseText(item.Title);
					schedule.Append(season).Append(' ').Append(startDay);
					if (endDay != startDay)
					{
						schedule.Append("-").Append(endDay);
					}
					schedule.Append(": ").AppendLine(title);
				}
			}
			log.Info(schedule.ToString());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Shop(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var shopId, out var error, allowBlank: false, "string shopId") || !ArgUtility.TryGetOptional(command, 2, out var ownerName, out error, null, allowBlank: false, "string ownerName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			string foundShopId = Utility.fuzzySearch(shopId, DataLoader.Shops(Game1.content).Keys.ToArray());
			if (foundShopId == null)
			{
				log.Error("Couldn't find any shop in Data/Shops matching ID '" + shopId + "'.");
				return;
			}
			shopId = foundShopId;
			if ((ownerName != null) ? Utility.TryOpenShopMenu(shopId, ownerName) : Utility.TryOpenShopMenu(shopId, Game1.player.currentLocation, null, null, forceOpen: true))
			{
				log.Info("Opened shop with ID '" + shopId + "'.");
			}
			else
			{
				log.Error("Failed to open shop with ID '" + shopId + "'. Is the data in Data/Shops valid?");
			}
		}

		/// <summary>Export a summary of every shop's current inventory.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ExportShops(string[] command, IGameLogger log)
		{
			StringBuilder report = new StringBuilder();
			string[] openShopArgs = new string[2] { "Shop", null };
			foreach (string shopId in DataLoader.Shops(Game1.content).Keys)
			{
				report.AppendLine(shopId);
				report.AppendLine("".PadRight(Math.Max(50, shopId.Length), '-'));
				try
				{
					openShopArgs[1] = shopId;
					DefaultHandlers.Shop(openShopArgs, log);
				}
				catch (Exception ex)
				{
					StringBuilder stringBuilder = report.Append("    ");
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(23, 1, stringBuilder);
					handler.AppendLiteral("Failed to open shop '");
					handler.AppendFormatted(shopId);
					handler.AppendLiteral("'.");
					stringBuilder2.AppendLine(ref handler);
					report.AppendLine("    " + string.Join("\n    ", ex.ToString().Split('\n')));
					continue;
				}
				if (Game1.activeClickableMenu is ShopMenu shop)
				{
					switch (shop.currency)
					{
					case 0:
						report.AppendLine("    Currency: gold");
						break;
					case 1:
						report.AppendLine("    Currency: star tokens");
						break;
					case 2:
						report.AppendLine("    Currency: Qi coins");
						break;
					case 4:
						report.AppendLine("    Currency: Qi gems");
						break;
					default:
					{
						StringBuilder stringBuilder = report;
						StringBuilder stringBuilder3 = stringBuilder;
						StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(20, 2, stringBuilder);
						handler.AppendFormatted("    ");
						handler.AppendLiteral("Currency: unknown (");
						handler.AppendFormatted(shop.currency);
						handler.AppendLiteral(")");
						stringBuilder3.AppendLine(ref handler);
						break;
					}
					}
					report.AppendLine();
					var summary = shop.itemPriceAndStock.Select(delegate(KeyValuePair<ISalable, ItemStockInformation> keyValuePair)
					{
						ISalable key = keyValuePair.Key;
						ItemStockInformation value = keyValuePair.Value;
						return new
						{
							Id = key.QualifiedItemId,
							Name = key.DisplayName,
							Price = value.Price,
							Trade = ((value.TradeItem != null) ? (value.TradeItem + " x" + (value.TradeItemCount ?? 1)) : null),
							StockLimit = ((value.Stock != int.MaxValue && value.LimitedStockMode != LimitedStockMode.None) ? $"{value.LimitedStockMode} {value.Stock}" : null)
						};
					}).ToArray();
					int idWidth = "id".Length;
					int nameWidth = "name".Length;
					int priceWidth = "price".Length;
					int tradeWidth = "trade".Length;
					int stockWidth = "stock limit".Length;
					var array = summary;
					foreach (var entry in array)
					{
						idWidth = Math.Max(idWidth, entry.Id.Length);
						nameWidth = Math.Max(nameWidth, entry.Name.Length);
						priceWidth = Math.Max(priceWidth, entry.Price.ToString().Length);
						if (entry.Trade != null)
						{
							tradeWidth = Math.Max(tradeWidth, entry.Trade.Length);
						}
						if (entry.StockLimit != null)
						{
							tradeWidth = Math.Max(tradeWidth, entry.StockLimit.Length);
						}
					}
					report.Append("    ").Append("id".PadRight(idWidth)).Append(" | ")
						.Append("name".PadRight(nameWidth))
						.Append(" | ")
						.Append("price".PadRight(priceWidth))
						.Append(" | ")
						.Append("trade".PadRight(tradeWidth))
						.AppendLine(" | stock limit");
					report.Append("    ").Append("".PadRight(idWidth, '-')).Append(" | ")
						.Append("".PadRight(nameWidth, '-'))
						.Append(" | ")
						.Append("".PadRight(priceWidth, '-'))
						.Append(" | ")
						.Append("".PadRight(tradeWidth, '-'))
						.Append(" | ")
						.AppendLine("".PadRight(stockWidth, '-'));
					array = summary;
					foreach (var entry2 in array)
					{
						report.Append("    ").Append(entry2.Id.PadRight(idWidth)).Append(" | ")
							.Append(entry2.Name.PadRight(nameWidth))
							.Append(" | ")
							.Append(entry2.Price.ToString().PadRight(priceWidth))
							.Append(" | ")
							.Append((entry2.Trade ?? "").PadRight(tradeWidth))
							.Append(" | ")
							.AppendLine(entry2.StockLimit);
					}
				}
				else
				{
					StringBuilder stringBuilder = report.Append("    ");
					StringBuilder stringBuilder4 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(60, 1, stringBuilder);
					handler.AppendLiteral("Failed to open shop '");
					handler.AppendFormatted(shopId);
					handler.AppendLiteral("': shop menu unexpected failed to open.");
					stringBuilder4.AppendLine(ref handler);
				}
				report.AppendLine();
				report.AppendLine();
			}
			string exportFilePath = Path.Combine(Program.GetLocalAppDataFolder("Exports"), $"{DateTime.Now:yyyy-MM-dd} shop export.txt");
			File.WriteAllText(exportFilePath, report.ToString());
			log.Info("Exported shop data to " + exportFilePath + ".");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Dating(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.friendshipData[npcName].Status = FriendshipStatus.Dating;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearActiveDialogueEvents(string[] command, IGameLogger log)
		{
			Game1.player.activeDialogueEvents.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Buff(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var buffId, out var error, allowBlank: false, "string buffId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.applyBuff(buffId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearBuffs(string[] command, IGameLogger log)
		{
			Game1.player.ClearBuffs();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void PauseTime(string[] command, IGameLogger log)
		{
			Game1.isTimePaused = !Game1.isTimePaused;
			Game1.playSound(Game1.isTimePaused ? "bigSelect" : "bigDeSelect");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "fbf" })]
		public static void FrameByFrame(string[] command, IGameLogger log)
		{
			Game1.frameByFrame = !Game1.frameByFrame;
			Game1.playSound(Game1.frameByFrame ? "bigSelect" : "bigDeSelect");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "fbp", "fill", "fillbp" })]
		public static void FillBackpack(string[] command, IGameLogger log)
		{
			for (int i = 0; i < Game1.player.Items.Count; i++)
			{
				if (Game1.player.Items[i] != null)
				{
					continue;
				}
				ItemMetadata metadata = null;
				while (metadata == null)
				{
					metadata = ItemRegistry.ResolveMetadata(Game1.random.Next(1000).ToString());
					ParsedItemData data = metadata?.GetParsedData();
					if (data == null || data.Category == -999 || data.ObjectType == "Crafting" || data.ObjectType == "Seeds")
					{
						metadata = null;
					}
				}
				Game1.player.Items[i] = metadata.CreateItem();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Bobber(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var bobberStyle, out var error, "int bobberStyle"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.bobberStyle.Value = bobberStyle;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "sl" })]
		public static void ShiftToolbarLeft(string[] command, IGameLogger log)
		{
			Game1.player.shiftToolbar(right: false);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "sr" })]
		public static void ShiftToolbarRight(string[] command, IGameLogger log)
		{
			Game1.player.shiftToolbar(right: true);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CharacterInfo(string[] command, IGameLogger log)
		{
			Game1.showGlobalMessage(Game1.currentLocation.characters.Count + " characters on this map");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DoesItemExist(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var itemId, out var error, allowBlank: false, "string itemId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.showGlobalMessage(Utility.doesItemExistAnywhere(itemId) ? "Yes" : "No");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SpecialItem(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var itemId, out var error, allowBlank: false, "string itemId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.specialItems.Add(itemId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void AnimalInfo(string[] command, IGameLogger log)
		{
			int animalCount = 0;
			int locationCount = 0;
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				int length = location.animals.Length;
				if (length > 0)
				{
					animalCount += length;
					locationCount++;
				}
				return true;
			});
			Game1.showGlobalMessage($"{animalCount} animals in {locationCount} locations");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearChildren(string[] command, IGameLogger log)
		{
			Game1.player.getRidOfChildren();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CreateSplash(string[] command, IGameLogger log)
		{
			Point offset = default(Point);
			switch (Game1.player.FacingDirection)
			{
			case 3:
				offset.X = -4;
				break;
			case 1:
				offset.X = 4;
				break;
			case 0:
				offset.Y = 4;
				break;
			case 2:
				offset.Y = -4;
				break;
			}
			Game1.player.currentLocation.fishSplashPoint.Set(new Point(Game1.player.TilePoint.X + offset.X, Game1.player.TilePoint.Y + offset.Y));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Pregnant(string[] command, IGameLogger log)
		{
			WorldDate birthingDate = Game1.Date;
			birthingDate.TotalDays++;
			Game1.player.GetSpouseFriendship().NextBirthingDate = birthingDate;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SpreadSeeds(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var cropId, out var error, allowBlank: false, "string cropId"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.currentLocation?.ForEachDirt(delegate(HoeDirt dirt)
			{
				dirt.crop = new Crop(cropId, (int)dirt.Tile.X, (int)dirt.Tile.Y, dirt.Location);
				return true;
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SpreadDirt(string[] command, IGameLogger log)
		{
			GameLocation location = Game1.currentLocation;
			if (location == null)
			{
				return;
			}
			for (int x = 0; x < location.map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < location.map.Layers[0].LayerHeight; y++)
				{
					if (location.doesTileHaveProperty(x, y, "Diggable", "Back") != null && location.CanItemBePlacedHere(new Vector2(x, y), itemIsPassable: true, CollisionMask.All, CollisionMask.None))
					{
						location.terrainFeatures.Add(new Vector2(x, y), new HoeDirt());
					}
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void RemoveFurniture(string[] command, IGameLogger log)
		{
			Game1.currentLocation.furniture.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MakeEx(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.player.friendshipData[npcName].RoommateMarriage = false;
			Game1.player.friendshipData[npcName].Status = FriendshipStatus.Divorced;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DarkTalisman(string[] command, IGameLogger log)
		{
			GameLocation gameLocation = Game1.RequireLocation("Railroad");
			GameLocation witchHut = Game1.RequireLocation("WitchHut");
			gameLocation.setMapTile(54, 35, 287, "Buildings", "untitled tile sheet", "");
			gameLocation.setMapTile(54, 34, 262, "Front", "untitled tile sheet", "");
			witchHut.setMapTile(4, 11, 114, "Buildings", "untitled tile sheet", "MagicInk");
			Game1.player.hasDarkTalisman = true;
			Game1.player.hasMagicInk = false;
			Game1.player.mailReceived.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ConventionMode(string[] command, IGameLogger log)
		{
			Game1.conventionMode = !Game1.conventionMode;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FarmMap(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var farmType, out var error, "int farmType"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.locations.RemoveWhere((GameLocation location) => location is Farm || location is FarmHouse);
			Game1.whichFarm = farmType;
			Game1.locations.Add(new Farm("Maps\\" + Farm.getMapNameFromTypeInt(Game1.whichFarm), "Farm"));
			Game1.locations.Add(new FarmHouse("Maps\\FarmHouse", "FarmHouse"));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearMuseum(string[] command, IGameLogger log)
		{
			Game1.RequireLocation<LibraryMuseum>("ArchaeologyHouse").museumPieces.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Clone(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.currentLocation.characters.Add(Utility.fuzzyCharacterSearch(npcName));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "zl" })]
		public static void ZoomLevel(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var zoomLevel, out var error, "int zoomLevel"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.options.desiredBaseZoomLevel = (float)zoomLevel / 100f;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "us" })]
		public static void UiScale(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var uiScale, out var error, "int uiScale"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.options.desiredUIScale = (float)uiScale / 100f;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DeleteArch(string[] command, IGameLogger log)
		{
			Game1.player.archaeologyFound.Clear();
			Game1.player.fishCaught.Clear();
			Game1.player.mineralsFound.Clear();
			Game1.player.mailReceived.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Save(string[] command, IGameLogger log)
		{
			Game1.saveOnNewDay = !Game1.saveOnNewDay;
			Game1.playSound(Game1.saveOnNewDay ? "bigSelect" : "bigDeSelect");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "removeLargeTf" })]
		public static void RemoveLargeTerrainFeature(string[] command, IGameLogger log)
		{
			Game1.currentLocation.largeTerrainFeatures.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Test(string[] command, IGameLogger log)
		{
			Game1.currentMinigame = new Test();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FenceDecay(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var decayAmount, out var error, "int decayAmount"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			foreach (Object value in Game1.currentLocation.objects.Values)
			{
				if (value is Fence fence)
				{
					fence.health.Value -= decayAmount;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "sb" })]
		public static void ShowTextAboveHead(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Utility.fuzzyCharacterSearch(npcName).showTextAboveHead(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3206"));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Gamepad(string[] command, IGameLogger log)
		{
			Game1.options.gamepadControls = !Game1.options.gamepadControls;
			Game1.options.mouseControls = !Game1.options.gamepadControls;
			Game1.showGlobalMessage(Game1.options.gamepadControls ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3209") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3210"));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Slimecraft(string[] command, IGameLogger log)
		{
			Game1.player.craftingRecipes.Add("Slime Incubator", 0);
			Game1.player.craftingRecipes.Add("Slime Egg-Press", 0);
			Game1.playSound("crystal", 0);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "kms" })]
		public static void KillMonsterStat(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var monsterId, out var error, allowBlank: false, "string monsterId") || !ArgUtility.TryGetInt(command, 2, out var kills, out error, "int kills"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.stats.specificMonstersKilled[monsterId] = kills;
			log.Info(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3159", monsterId, kills));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void RemoveAnimals(string[] command, IGameLogger log)
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				location.Animals.Clear();
				foreach (Building building in location.buildings)
				{
					if (building.GetIndoors() is AnimalHouse animalHouse)
					{
						animalHouse.Animals.Clear();
					}
				}
				return true;
			}, includeInteriors: false);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FixAnimals(string[] command, IGameLogger log)
		{
			bool fixedAny = false;
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				int num = 0;
				foreach (Building building in location.buildings)
				{
					if (building.GetIndoors() is AnimalHouse animalHouse)
					{
						foreach (FarmAnimal animal in animalHouse.animals.Values)
						{
							foreach (Building current in location.buildings)
							{
								if (current.GetIndoors() is AnimalHouse animalHouse2 && animalHouse2.animalsThatLiveHere.Contains(animal.myID.Value) && !current.Equals(animal.home))
								{
									num += animalHouse2.animalsThatLiveHere.RemoveWhere((long id) => id == animal.myID.Value);
								}
							}
						}
						num += animalHouse.animalsThatLiveHere.RemoveWhere((long id) => Utility.getAnimal(id) == null);
					}
				}
				if (num > 0)
				{
					Game1.playSound("crystal", 0);
					log.Info($"Fixed {num} animals in the '{location.NameOrUniqueName}' location.");
					fixedAny = true;
				}
				return true;
			}, includeInteriors: false);
			if (!fixedAny)
			{
				log.Info("No animal issues found.");
			}
			Utility.fixAllAnimals();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DisplaceAnimals(string[] command, IGameLogger log)
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (location.animals.Length == 0 && location.buildings.Count == 0)
				{
					return true;
				}
				Utility.fixAllAnimals();
				foreach (Building building in location.buildings)
				{
					if (building.GetIndoors() is AnimalHouse animalHouse)
					{
						foreach (FarmAnimal current in animalHouse.animals.Values)
						{
							current.homeInterior = null;
							current.Position = Utility.recursiveFindOpenTileForCharacter(current, location, new Vector2(40f, 40f), 200) * 64f;
							location.animals.TryAdd(current.myID.Value, current);
						}
						animalHouse.animals.Clear();
						animalHouse.animalsThatLiveHere.Clear();
					}
				}
				return true;
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "sdkInfo" })]
		public static void SteamInfo(string[] command, IGameLogger log)
		{
			Program.sdk.DebugInfo();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Achieve(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var achievementId, out var error, allowBlank: false, "string achievementId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.getSteamAchievement(achievementId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ResetAchievements(string[] command, IGameLogger log)
		{
			Program.sdk.ResetAchievements();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Divorce(string[] command, IGameLogger log)
		{
			Game1.player.divorceTonight.Value = true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void BefriendAnimals(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalInt(command, 1, out var friendship, out var error, 1000, "int friendship"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			foreach (FarmAnimal value in Game1.currentLocation.animals.Values)
			{
				value.friendshipTowardFarmer.Value = friendship;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void PetToFarm(string[] command, IGameLogger log)
		{
			Game1.RequireCharacter<Pet>(Game1.player.getPetName(), mustBeVillager: false).setAtFarmPosition();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void BefriendPets(string[] command, IGameLogger log)
		{
			foreach (NPC allCharacter in Utility.getAllCharacters())
			{
				if (allCharacter is Pet pet)
				{
					pet.friendshipTowardFarmer.Value = 1000;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Version(string[] command, IGameLogger log)
		{
			log.Info(typeof(Game1).Assembly.GetName().Version?.ToString() ?? "");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "sdlv" })]
		public static void SdlVersion(string[] command, IGameLogger log)
		{
			Type sdlType = Assembly.GetAssembly(GameRunner.instance.Window.GetType())?.GetType("Sdl");
			if ((object)sdlType == null)
			{
				log.Error("Could not find type 'Sdl'");
				return;
			}
			Type versionType = null;
			object versionObject = null;
			FieldInfo versionField = sdlType.GetField("version", BindingFlags.Static | BindingFlags.Public);
			if ((object)versionField == null)
			{
				log.Error("SDL does not have field 'version'");
				return;
			}
			versionType = versionField.FieldType;
			versionObject = versionField.GetValue(null);
			if ((object)versionType == null)
			{
				log.Error("Could not find type 'Sdl::Type'");
				return;
			}
			if (versionObject == null)
			{
				log.Error("The obtained from from SDL was null");
				return;
			}
			byte[] versionBytes = new byte[3];
			string[] versionComponents = new string[3] { "Major", "Minor", "Patch" };
			for (int c = 0; c < 3; c++)
			{
				string componentName = versionComponents[c];
				FieldInfo componentField = versionType.GetField(componentName, BindingFlags.Instance | BindingFlags.Public);
				if ((object)componentField == null)
				{
					log.Error("SDL::Version does not have field '" + componentName + "'");
					return;
				}
				object componentObject = componentField.GetValue(versionObject);
				if (componentObject is byte)
				{
					_ = (byte)componentObject;
					versionBytes[c] = (byte)componentObject;
					continue;
				}
				log.Error("SDL::Version field '" + componentName + "' is not a byte");
				return;
			}
			log.Info($"SDL Version: {versionBytes[0]}.{versionBytes[1]}.{versionBytes[2]}");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "ns" })]
		public static void NoSave(string[] command, IGameLogger log)
		{
			Game1.saveOnNewDay = !Game1.saveOnNewDay;
			if (!Game1.saveOnNewDay)
			{
				Game1.playSound("bigDeSelect");
			}
			else
			{
				Game1.playSound("bigSelect");
			}
			log.Info("Saving is now " + (Game1.saveOnNewDay ? "enabled" : "disabled"));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "rfh" })]
		public static void ReadyForHarvest(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetVector2(command, 1, out var tile, out var error, integerOnly: true, "Vector2 tile"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.currentLocation.objects[tile].minutesUntilReady.Value = 1;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void BeachBridge(string[] command, IGameLogger log)
		{
			Beach beach = Game1.RequireLocation<Beach>("Beach");
			beach.bridgeFixed.Value = !beach.bridgeFixed.Value;
			if (!beach.bridgeFixed.Value)
			{
				beach.setMapTile(58, 13, 284, "Buildings", "untitled tile sheet");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		/// <remarks>See also <see cref="M:StardewValley.DebugCommands.DefaultHandlers.DaysPlayed(System.String[],StardewValley.Logging.IGameLogger)" />.</remarks>
		public static void Dp(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var daysPlayed, out var error, "int daysPlayed"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.stats.DaysPlayed = (uint)daysPlayed;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "fo" })]
		public static void FrameOffset(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var frame, out var error, "int frame") || !ArgUtility.TryGetInt(command, 2, out var offsetX, out error, "int offsetX") || !ArgUtility.TryGetInt(command, 3, out var offsetY, out error, "int offsetY"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			FarmerRenderer.featureXOffsetPerFrame[frame] = (short)offsetX;
			FarmerRenderer.featureYOffsetPerFrame[frame] = (short)offsetY;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Horse(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalInt(command, 1, out var tileX, out var error, Game1.player.TilePoint.X, "int tileX") || !ArgUtility.TryGetOptionalInt(command, 1, out var tileY, out error, Game1.player.TilePoint.Y, "int tileY"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.currentLocation.characters.Add(new Horse(GuidHelper.NewGuid(), tileX, tileY));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Owl(string[] command, IGameLogger log)
		{
			Game1.currentLocation.addOwl();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Pole(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalInt(command, 1, out var rodLevel, out var error, 0, "int rodLevel"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Item fishingRod = rodLevel switch
			{
				1 => ItemRegistry.Create("(T)TrainingRod"), 
				2 => ItemRegistry.Create("(T)FiberglassRod"), 
				3 => ItemRegistry.Create("(T)IridiumRod"), 
				_ => ItemRegistry.Create("(T)BambooRod"), 
			};
			Game1.player.addItemToInventoryBool(fishingRod);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void RemoveQuest(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var questId, out var error, allowBlank: false, "string questId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.removeQuest(questId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CompleteQuest(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var questId, out var error, allowBlank: false, "string questId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.completeQuest(questId);
			}
		}

		/// <summary>Set the current player's preferred pet type and breed. This doesn't change any existing pets; see <see cref="M:StardewValley.DebugCommands.DefaultHandlers.ChangePet(System.String[],StardewValley.Logging.IGameLogger)" /> for that.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SetPreferredPet(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var typeId, out var error, allowBlank: false, "string typeId") || !ArgUtility.TryGetOptional(command, 2, out var breedId, out error, null, allowBlank: false, "string breedId"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			if (!Pet.TryGetData(typeId, out var data))
			{
				log.Error($"Can't set the player's preferred pet type to '{typeId}': no such pet type found. Expected one of ['{string.Join("', '", Game1.petData.Keys)}'].");
				return;
			}
			if (breedId != null && data.Breeds.All((PetBreed p) => p.Id != breedId))
			{
				log.Error($"Can't set the player's preferred pet breed to '{breedId}': no such breed found. Expected one of ['{string.Join("', '", data.Breeds.Select((PetBreed p) => p.Id))}'].");
				return;
			}
			bool changed = false;
			if (Game1.player.whichPetType != typeId)
			{
				log.Info($"Changed preferred pet type from '{Game1.player.whichPetType}' to '{typeId}'.");
				Game1.player.whichPetType = typeId;
				changed = true;
				if (breedId == null)
				{
					breedId = data.Breeds.FirstOrDefault()?.Id;
				}
			}
			if (breedId != null && Game1.player.whichPetBreed != breedId)
			{
				log.Info($"Changed preferred pet breed from '{Game1.player.whichPetBreed}' to '{breedId}'.");
				Game1.player.whichPetBreed = breedId;
				changed = true;
			}
			if (!changed)
			{
				log.Info("The player's pet type and breed already match those values.");
			}
		}

		/// <summary>Change the pet type and/or breed for a specific pet. This doesn't change the player's preferred pet type/breed; see <see cref="M:StardewValley.DebugCommands.DefaultHandlers.SetPreferredPet(System.String[],StardewValley.Logging.IGameLogger)" /> for that.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ChangePet(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var petName, out var error, allowBlank: false, "string petName") || !ArgUtility.TryGet(command, 2, out var typeId, out error, allowBlank: false, "string typeId") || !ArgUtility.TryGetOptional(command, 3, out var breedId, out error, null, allowBlank: false, "string breedId"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			if (!Pet.TryGetData(typeId, out var data))
			{
				log.Error($"Can't set the pet type to '{typeId}': no such pet type found. Expected one of ['{string.Join("', '", Game1.petData.Keys)}'].");
				return;
			}
			if (breedId != null && data.Breeds.All((PetBreed p) => p.Id != breedId))
			{
				log.Error($"Can't set the pet breed to '{breedId}': no such breed found. Expected one of ['{string.Join("', '", data.Breeds.Select((PetBreed p) => p.Id))}'].");
				return;
			}
			Pet pet = Game1.getCharacterFromName<Pet>(petName, mustBeVillager: false);
			if (pet == null)
			{
				log.Error("No pet found with name '" + petName + "'.");
				return;
			}
			bool changed = false;
			if (pet.petType.Value != typeId)
			{
				log.Info($"Changed {pet.Name}'s type from '{pet.petType.Value}' to '{typeId}'.");
				pet.petType.Value = typeId;
				changed = true;
				if (breedId == null)
				{
					breedId = data.Breeds.FirstOrDefault()?.Id;
				}
			}
			if (breedId != null && pet.whichBreed.Value != breedId)
			{
				log.Info($"Changed {pet.Name}'s breed from '{pet.whichBreed.Value}' to '{breedId}'.");
				pet.whichBreed.Value = breedId;
				changed = true;
			}
			if (!changed)
			{
				log.Info(pet.Name + "'s type and breed already match those values.");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearCharacters(string[] command, IGameLogger log)
		{
			Game1.currentLocation.characters.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Cat(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetPoint(command, 1, out var tile, out var error, "Point tile") || !ArgUtility.TryGetOptional(command, 3, out var breedId, out error, "0", allowBlank: false, "string breedId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.currentLocation.characters.Add(new Pet(tile.X, tile.Y, breedId, "Cat"));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Dog(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetPoint(command, 1, out var tile, out var error, "Point tile") || !ArgUtility.TryGetOptional(command, 3, out var breedId, out error, "0", allowBlank: false, "string breedId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.currentLocation.characters.Add(new Pet(tile.X, tile.Y, breedId, "Dog"));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Quest(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var questId, out var error, allowBlank: false, "string questId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.addQuest(questId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DeliveryQuest(string[] command, IGameLogger log)
		{
			Game1.player.questLog.Add(new ItemDeliveryQuest());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CollectQuest(string[] command, IGameLogger log)
		{
			Game1.player.questLog.Add(new ResourceCollectionQuest());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SlayQuest(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalBool(command, 1, out var ignoreFarmMonsters, out var error, defaultValue: true, "bool ignoreFarmMonsters"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.player.questLog.Add(new SlayMonsterQuest
			{
				ignoreFarmMonsters = { ignoreFarmMonsters }
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Quests(string[] command, IGameLogger log)
		{
			foreach (string id in DataLoader.Quests(Game1.content).Keys)
			{
				if (!Game1.player.hasQuest(id))
				{
					Game1.player.addQuest(id);
				}
			}
			Game1.player.questLog.Add(new ItemDeliveryQuest());
			Game1.player.questLog.Add(new SlayMonsterQuest());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearQuests(string[] command, IGameLogger log)
		{
			Game1.player.questLog.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "fb" })]
		public static void FillBin(string[] command, IGameLogger log)
		{
			IInventory shippingBin = Game1.getFarm().getShippingBin(Game1.player);
			shippingBin.Add(ItemRegistry.Create("(O)24"));
			shippingBin.Add(ItemRegistry.Create("(O)82"));
			shippingBin.Add(ItemRegistry.Create("(O)136"));
			shippingBin.Add(ItemRegistry.Create("(O)16"));
			shippingBin.Add(ItemRegistry.Create("(O)388"));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Gold(string[] command, IGameLogger log)
		{
			Game1.player.Money += 1000000;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearFarm(string[] command, IGameLogger log)
		{
			Farm farm = Game1.getFarm();
			Layer layer = farm.map.Layers[0];
			farm.removeObjectsAndSpawned(0, 0, layer.LayerWidth, layer.LayerHeight);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SetupFarm(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalBool(command, 1, out var clearMore, out var error, defaultValue: false, "bool clearMore"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Farm farm = Game1.getFarm();
			Layer layer = farm.map.Layers[0];
			farm.buildings.Clear();
			farm.AddDefaultBuildings();
			farm.removeObjectsAndSpawned(0, 0, layer.LayerWidth, 16 + (clearMore ? 32 : 0));
			farm.removeObjectsAndSpawned(56, 17, 16, 18);
			for (int x = 58; x < 70; x++)
			{
				for (int y = 19; y < 29; y++)
				{
					farm.terrainFeatures.Add(new Vector2(x, y), new HoeDirt());
				}
			}
			if (farm.buildStructure("Coop", new Vector2(52f, 11f), Game1.player, out var coop))
			{
				coop.daysOfConstructionLeft.Value = 0;
			}
			if (farm.buildStructure("Silo", new Vector2(36f, 9f), Game1.player, out var silo))
			{
				silo.daysOfConstructionLeft.Value = 0;
			}
			if (farm.buildStructure("Barn", new Vector2(42f, 10f), Game1.player, out var barn))
			{
				barn.daysOfConstructionLeft.Value = 0;
			}
			for (int i = 0; i < Game1.player.Items.Count; i++)
			{
				if (Game1.player.Items[i] is Tool tool)
				{
					string newId = null;
					switch (tool.QualifiedItemId)
					{
					case "(T)Axe":
					case "(T)CopperAxe":
					case "(T)SteelAxe":
					case "(T)GoldAxe":
						newId = "(T)IridiumAxe";
						break;
					case "(T)Hoe":
					case "(T)CopperHoe":
					case "(T)SteelHoe":
					case "(T)GoldHoe":
						newId = "(T)IridiumHoe";
						break;
					case "(T)Pickaxe":
					case "(T)GoldPickaxe":
					case "(T)CopperPickaxe":
					case "(T)SteelPickaxe":
						newId = "(T)IridiumPickaxe";
						break;
					case "(T)WateringCan":
					case "(T)CopperWateringCan":
					case "(T)SteelWateringCan":
					case "(T)GoldWateringCan":
						newId = "(T)IridiumWateringCan";
						break;
					}
					if (newId != null)
					{
						Tool newTool = ItemRegistry.Create<Tool>(newId);
						newTool.UpgradeFrom(newTool);
						Game1.player.Items[i] = newTool;
					}
				}
			}
			Game1.player.Money += 20000;
			Game1.player.addItemToInventoryBool(ItemRegistry.Create("(T)Shears"));
			Game1.player.addItemToInventoryBool(ItemRegistry.Create("(T)MilkPail"));
			Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)472", 999));
			Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)473", 999));
			Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)322", 999));
			Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)388", 999));
			Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)390", 999));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void RemoveBuildings(string[] command, IGameLogger log)
		{
			Game1.currentLocation.buildings.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Build(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var buildingType, out var error, allowBlank: false, "string buildingType") || !ArgUtility.TryGetOptionalInt(command, 2, out var x, out error, Game1.player.TilePoint.X + 1, "int x") || !ArgUtility.TryGetOptionalInt(command, 3, out var y, out error, Game1.player.TilePoint.Y, "int y") || !ArgUtility.TryGetOptionalBool(command, 4, out var forceBuild, out error, ArgUtility.Get(command, 0) == "ForceBuild", "bool forceBuild"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			if (!Game1.buildingData.ContainsKey(buildingType))
			{
				buildingType = Game1.buildingData.Keys.FirstOrDefault((string key) => buildingType.EqualsIgnoreCase(key)) ?? buildingType;
			}
			Building constructed;
			if (!Game1.buildingData.ContainsKey(buildingType))
			{
				string[] matches = Utility.fuzzySearchAll(buildingType, Game1.buildingData.Keys, sortByScore: false).ToArray();
				log.Warn((matches.Length == 0) ? ("There's no building with type '" + buildingType + "'.") : ("There's no building with type '" + buildingType + "'. Did you mean one of these?\n- " + string.Join("\n- ", matches)));
			}
			else if (!Game1.currentLocation.buildStructure(buildingType, new Vector2(x, y), Game1.player, out constructed, magicalConstruction: false, forceBuild))
			{
				log.Warn($"Couldn't place a '{buildingType}' building at position ({x}, {y}).");
			}
			else
			{
				constructed.daysOfConstructionLeft.Value = 0;
				log.Info($"Placed '{buildingType}' at position ({x}, {y}).");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ForceBuild(string[] command, IGameLogger log)
		{
			if (ArgUtility.HasIndex(command, 0))
			{
				command[0] = "ForceBuild";
			}
			DefaultHandlers.Build(command, log);
		}

		[OtherNames(new string[] { "fab" })]
		public static void FinishAllBuilds(string[] command, IGameLogger log)
		{
			if (!Game1.IsMasterGame)
			{
				log.Error("Only the host can use this command.");
				return;
			}
			int count = 0;
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				foreach (Building current in location.buildings)
				{
					if (current.daysOfConstructionLeft.Value > 0 || current.daysUntilUpgrade.Value > 0)
					{
						current.FinishConstruction();
						int num = count + 1;
						count = num;
					}
				}
				return true;
			});
			log.Info($"Finished constructing {count} building(s).");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void LocalInfo(string[] command, IGameLogger log)
		{
			int grass = 0;
			int trees = 0;
			int other = 0;
			foreach (TerrainFeature t in Game1.currentLocation.terrainFeatures.Values)
			{
				if (!(t is Grass))
				{
					if (t is Tree)
					{
						trees++;
					}
					else
					{
						other++;
					}
				}
				else
				{
					grass++;
				}
			}
			string summary = "Grass:" + grass + ",  " + "Trees:" + trees + ",  " + "Other Terrain Features:" + other + ",  " + "Objects: " + Game1.currentLocation.objects.Length + ",  " + "temporarySprites: " + Game1.currentLocation.temporarySprites.Count + ",  ";
			log.Info(summary);
			Game1.drawObjectDialogue(summary);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "al" })]
		public static void AmbientLight(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var red, out var error, "int red") || !ArgUtility.TryGetInt(command, 2, out var green, out error, "int green") || !ArgUtility.TryGetInt(command, 3, out var blue, out error, "int blue"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.ambientLight = new Color(red, green, blue);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ResetMines(string[] command, IGameLogger log)
		{
			MineShaft.permanentMineChanges.Clear();
			Game1.playSound("jingle1");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "db" })]
		public static void SpeakTo(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptional(command, 1, out var npcName, out var error, "Pierre", allowBlank: false, "string npcName"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.activeClickableMenu = new DialogueBox(Utility.fuzzyCharacterSearch(npcName).CurrentDialogue.Peek());
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SkullKey(string[] command, IGameLogger log)
		{
			Game1.player.hasSkullKey = true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void TownKey(string[] command, IGameLogger log)
		{
			Game1.player.HasTownKey = true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Specials(string[] command, IGameLogger log)
		{
			Game1.player.hasRustyKey = true;
			Game1.player.hasSkullKey = true;
			Game1.player.hasSpecialCharm = true;
			Game1.player.hasDarkTalisman = true;
			Game1.player.hasMagicInk = true;
			Game1.player.hasClubCard = true;
			Game1.player.canUnderstandDwarves = true;
			Game1.player.hasMagnifyingGlass = true;
			Game1.player.eventsSeen.Add("2120303");
			Game1.player.eventsSeen.Add("3910979");
			Game1.player.HasTownKey = true;
			Game1.player.stats.Set("trinketSlots", 1);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SkullGear(string[] command, IGameLogger log)
		{
			int addSlots = 36 - Game1.player.MaxItems;
			if (addSlots > 0)
			{
				Game1.player.increaseBackpackSize(addSlots);
			}
			Game1.player.hasSkullKey = true;
			Game1.player.Equip(ItemRegistry.Create<Ring>("(O)527"), Game1.player.leftRing);
			Game1.player.Equip(ItemRegistry.Create<Ring>("(O)523"), Game1.player.rightRing);
			Game1.player.Equip(ItemRegistry.Create<Boots>("(B)514"), Game1.player.boots);
			Game1.player.clearBackpack();
			Game1.player.addItemToInventory(ItemRegistry.Create("(T)IridiumPickaxe"));
			Game1.player.addItemToInventory(ItemRegistry.Create("(W)4"));
			Game1.player.addItemToInventory(ItemRegistry.Create("(O)226", 20));
			Game1.player.addItemToInventory(ItemRegistry.Create("(O)288", 20));
			Game1.player.professions.Add(24);
			Game1.player.maxHealth = 75;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearSpecials(string[] command, IGameLogger log)
		{
			Game1.player.hasRustyKey = false;
			Game1.player.hasSkullKey = false;
			Game1.player.hasSpecialCharm = false;
			Game1.player.hasDarkTalisman = false;
			Game1.player.hasMagicInk = false;
			Game1.player.hasClubCard = false;
			Game1.player.canUnderstandDwarves = false;
			Game1.player.hasMagnifyingGlass = false;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Tv(string[] command, IGameLogger log)
		{
			string itemId = Game1.random.Choose("(F)1466", "(F)1468");
			Game1.player.addItemToInventoryBool(ItemRegistry.Create(itemId));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "sn" })]
		public static void SecretNote(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalInt(command, 1, out var noteId, out var error, -1, "int noteId"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.player.hasMagnifyingGlass = true;
			if (noteId > -1)
			{
				int whichNote = noteId;
				Object note = ItemRegistry.Create<Object>("(O)79");
				note.name = note.name + " #" + whichNote;
				Game1.player.addItemToInventory(note);
			}
			else
			{
				Game1.player.addItemToInventory(Game1.currentLocation.tryToCreateUnseenSecretNote(Game1.player));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Child2(string[] command, IGameLogger log)
		{
			Farmer player = Game1.player;
			List<Child> children = player.getChildren();
			if (children.Count > 1)
			{
				children[1].Age++;
				children[1].reloadSprite();
			}
			else
			{
				Utility.getHomeOfFarmer(player).characters.Add(new Child("Baby2", Game1.random.NextBool(), Game1.random.NextBool(), player));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "kid" })]
		public static void Child(string[] command, IGameLogger log)
		{
			Farmer player = Game1.player;
			List<Child> children = player.getChildren();
			if (children.Count > 0)
			{
				children[0].Age++;
				children[0].reloadSprite();
			}
			else
			{
				Utility.getHomeOfFarmer(player).characters.Add(new Child("Baby", Game1.random.NextBool(), Game1.random.NextBool(), player));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void KillAll(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var safeNpcName, out var error, allowBlank: false, "string safeNpcName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (!location.Equals(Game1.currentLocation))
				{
					location.characters.Clear();
				}
				else
				{
					location.characters.RemoveWhere((NPC npc) => npc.Name != safeNpcName);
				}
				return true;
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ResetWorldState(string[] command, IGameLogger log)
		{
			Game1.worldStateIDs.Clear();
			Game1.netWorldState.Value = new NetWorldState();
			Game1.game1.parseDebugInput("DeleteArch", log);
			Game1.player.mailReceived.Clear();
			Game1.player.eventsSeen.Clear();
			Game1.eventsSeenSinceLastLocationChange.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void KillAllHorses(string[] command, IGameLogger log)
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (location.characters.RemoveWhere((NPC npc) => npc is Horse) > 0)
				{
					Game1.playSound("drumkit0");
				}
				return true;
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DatePlayer(string[] command, IGameLogger log)
		{
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer != Game1.player && farmer.isCustomized.Value)
				{
					Game1.player.team.GetFriendship(Game1.player.UniqueMultiplayerID, farmer.UniqueMultiplayerID).Status = FriendshipStatus.Dating;
					break;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void EngagePlayer(string[] command, IGameLogger log)
		{
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer != Game1.player && farmer.isCustomized.Value)
				{
					Friendship friendship = Game1.player.team.GetFriendship(Game1.player.UniqueMultiplayerID, farmer.UniqueMultiplayerID);
					friendship.Status = FriendshipStatus.Engaged;
					friendship.WeddingDate = Game1.Date;
					friendship.WeddingDate.TotalDays++;
					break;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MarryPlayer(string[] command, IGameLogger log)
		{
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				if (farmer != Game1.player && farmer.isCustomized.Value)
				{
					Friendship friendship = Game1.player.team.GetFriendship(Game1.player.UniqueMultiplayerID, farmer.UniqueMultiplayerID);
					friendship.Status = FriendshipStatus.Married;
					friendship.WeddingDate = Game1.Date;
					break;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Marry(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			NPC spouse = Utility.fuzzyCharacterSearch(npcName);
			if (spouse == null)
			{
				log.Error("No character found matching '" + npcName + "'.");
				return;
			}
			if (!Game1.player.friendshipData.TryGetValue(spouse.Name, out var friendship))
			{
				friendship = (Game1.player.friendshipData[spouse.Name] = new Friendship());
			}
			Game1.player.changeFriendship(2500, spouse);
			Game1.player.spouse = spouse.Name;
			friendship.WeddingDate = new WorldDate(Game1.Date);
			friendship.Status = FriendshipStatus.Married;
			Game1.prepareSpouseForWedding(Game1.player);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Engaged(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			NPC spouse = Utility.fuzzyCharacterSearch(npcName);
			if (spouse == null)
			{
				log.Error("No character found matching '" + npcName + "'.");
				return;
			}
			if (!Game1.player.friendshipData.TryGetValue(spouse.Name, out var friendship))
			{
				friendship = (Game1.player.friendshipData[spouse.Name] = new Friendship());
			}
			Game1.player.changeFriendship(2500, spouse);
			Game1.player.spouse = spouse.Name;
			friendship.Status = FriendshipStatus.Engaged;
			WorldDate weddingDate = Game1.Date;
			weddingDate.TotalDays++;
			friendship.WeddingDate = weddingDate;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearLightGlows(string[] command, IGameLogger log)
		{
			Game1.currentLocation.lightGlows.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "wp" })]
		public static void Wallpaper(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalInt(command, 1, out var wallpaperId, out var error, -1, "int wallpaperId"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			if (wallpaperId > -1)
			{
				Game1.player.addItemToInventoryBool(new Wallpaper(wallpaperId));
				return;
			}
			bool floor = Game1.random.NextBool();
			Game1.player.addItemToInventoryBool(new Wallpaper(floor ? Game1.random.Next(40) : Game1.random.Next(112), floor));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearFurniture(string[] command, IGameLogger log)
		{
			Game1.currentLocation.furniture.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "ff" })]
		public static void Furniture(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptional(command, 1, out var furnitureId, out var error, null, allowBlank: false, "string furnitureId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else if (furnitureId == null)
			{
				Item furniture = null;
				while (furniture == null)
				{
					try
					{
						furniture = ItemRegistry.Create("(F)" + Game1.random.Next(1613));
					}
					catch
					{
					}
				}
				Game1.player.addItemToInventoryBool(furniture);
			}
			else
			{
				Game1.player.addItemToInventoryBool(ItemRegistry.Create("(F)" + furnitureId));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SpawnCoopsAndBarns(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var count, out var error, "int count"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				if (!(Game1.currentLocation is Farm farm))
				{
					return;
				}
				for (int i = 0; i < count; i++)
				{
					for (int j = 0; j < 20; j++)
					{
						bool coop = Game1.random.NextBool();
						if (farm.buildStructure(coop ? "Deluxe Coop" : "Deluxe Barn", farm.getRandomTile(), Game1.player, out var building))
						{
							building.daysOfConstructionLeft.Value = 0;
							building.doAction(Utility.PointToVector2(building.animalDoor.Value) + new Vector2(building.tileX.Value, building.tileY.Value), Game1.player);
							for (int k = 0; k < 16; k++)
							{
								Utility.addAnimalToFarm(new FarmAnimal(coop ? "White Chicken" : "Cow", Game1.random.Next(int.MaxValue), Game1.player.UniqueMultiplayerID));
							}
							break;
						}
					}
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SetupFishPondFarm(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalInt(command, 1, out var population, out var error, 10, "int population"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.game1.parseDebugInput("ClearFarm", log);
			for (int x = 4; x < 77; x += 6)
			{
				for (int y = 9; y < 60; y += 6)
				{
					Game1.game1.parseDebugInput($"{"Build"} \"Fish Pond\" {x} {y}", log);
				}
			}
			foreach (Building building in Game1.getFarm().buildings)
			{
				if (building is FishPond fishPond)
				{
					int fish = Game1.random.Next(128, 159);
					if (Game1.random.NextDouble() < 0.15)
					{
						fish = Game1.random.Next(698, 724);
					}
					if (Game1.random.NextDouble() < 0.05)
					{
						fish = Game1.random.Next(796, 801);
					}
					ParsedItemData data = ItemRegistry.GetData(fish.ToString());
					if (data != null && data.Category == -4)
					{
						fishPond.fishType.Value = fish.ToString();
					}
					else
					{
						fishPond.fishType.Value = Game1.random.Choose("393", "397");
					}
					fishPond.maxOccupants.Value = 10;
					fishPond.currentOccupants.Value = population;
					fishPond.GetFishObject();
				}
			}
			Game1.game1.parseDebugInput("DayUpdate 1", log);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Grass(string[] command, IGameLogger log)
		{
			GameLocation location = Game1.currentLocation;
			if (location == null)
			{
				return;
			}
			for (int x = 0; x < location.Map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < location.Map.Layers[0].LayerHeight; y++)
				{
					if (location.CanItemBePlacedHere(new Vector2(x, y), itemIsPassable: true, CollisionMask.All, CollisionMask.None))
					{
						location.terrainFeatures.Add(new Vector2(x, y), new Grass(1, 4));
					}
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SetupBigFarm(string[] command, IGameLogger log)
		{
			Farm farm = Game1.getFarm();
			Game1.game1.parseDebugInput("ClearFarm", log);
			Game1.game1.parseDebugInput("Build \"Deluxe Coop\" 4 9", log);
			Game1.game1.parseDebugInput("Build \"Deluxe Coop\" 10 9", log);
			Game1.game1.parseDebugInput("Build \"Deluxe Coop\" 36 11", log);
			Game1.game1.parseDebugInput("Build \"Deluxe Barn\" 16 9", log);
			Game1.game1.parseDebugInput("Build \"Deluxe Barn\" 3 16", log);
			Game1.game1.parseDebugInput("Build Mill 30 20", log);
			Game1.game1.parseDebugInput("Build Stable 46 10", log);
			Game1.game1.parseDebugInput("Build Silo 54 14", log);
			Game1.game1.parseDebugInput("Build \"Junimo Hut\" 48 52", log);
			Game1.game1.parseDebugInput("Build \"Junimo Hut\" 55 52", log);
			Game1.game1.parseDebugInput("Build \"Junimo Hut\" 59 52", log);
			Game1.game1.parseDebugInput("Build \"Junimo Hut\" 65 52", log);
			foreach (Building building in farm.buildings)
			{
				if (!(building.GetIndoors() is AnimalHouse animalHouse))
				{
					continue;
				}
				BuildingData buildingData = building.GetData();
				string[] validAnimalKeys = (from p in Game1.farmAnimalData
					where p.Value.House != null && buildingData.ValidOccupantTypes.Contains(p.Value.House)
					select p.Key).ToArray();
				for (int i = 0; i < animalHouse.animalLimit.Value; i++)
				{
					if (animalHouse.isFull())
					{
						break;
					}
					FarmAnimal animal = new FarmAnimal(Game1.random.ChooseFrom(validAnimalKeys), Game1.random.Next(int.MaxValue), Game1.player.UniqueMultiplayerID);
					if (Game1.random.NextBool())
					{
						animal.growFully();
					}
					animalHouse.adoptAnimal(animal);
				}
			}
			foreach (Building building2 in farm.buildings)
			{
				building2.doAction(Utility.PointToVector2(building2.animalDoor.Value) + new Vector2(building2.tileX.Value, building2.tileY.Value), Game1.player);
			}
			for (int x = 11; x < 23; x++)
			{
				for (int y = 14; y < 25; y++)
				{
					farm.terrainFeatures.Add(new Vector2(x, y), new Grass(1, 4));
				}
			}
			for (int x2 = 3; x2 < 23; x2++)
			{
				for (int y2 = 57; y2 < 61; y2++)
				{
					farm.terrainFeatures.Add(new Vector2(x2, y2), new Grass(1, 4));
				}
			}
			for (int y3 = 17; y3 < 25; y3++)
			{
				farm.terrainFeatures.Add(new Vector2(64f, y3), new Flooring("6"));
			}
			for (int x3 = 35; x3 < 64; x3++)
			{
				farm.terrainFeatures.Add(new Vector2(x3, 24f), new Flooring("6"));
			}
			for (int x4 = 38; x4 < 76; x4++)
			{
				for (int y4 = 18; y4 < 52; y4++)
				{
					if (farm.CanItemBePlacedHere(new Vector2(x4, y4), itemIsPassable: true, CollisionMask.All, CollisionMask.None))
					{
						HoeDirt dirt = new HoeDirt();
						farm.terrainFeatures.Add(new Vector2(x4, y4), dirt);
						dirt.plant((472 + Game1.random.Next(5)).ToString(), Game1.player, isFertilizer: false);
					}
				}
			}
			Game1.game1.parseDebugInput("GrowCrops 8", log);
			Vector2[] obj = new Vector2[18]
			{
				new Vector2(8f, 25f),
				new Vector2(11f, 25f),
				new Vector2(14f, 25f),
				new Vector2(17f, 25f),
				new Vector2(20f, 25f),
				new Vector2(23f, 25f),
				new Vector2(8f, 28f),
				new Vector2(11f, 28f),
				new Vector2(14f, 28f),
				new Vector2(17f, 28f),
				new Vector2(20f, 28f),
				new Vector2(23f, 28f),
				new Vector2(8f, 31f),
				new Vector2(11f, 31f),
				new Vector2(14f, 31f),
				new Vector2(17f, 31f),
				new Vector2(20f, 31f),
				new Vector2(23f, 31f)
			};
			NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>> terrainFeatures = farm.terrainFeatures;
			Vector2[] array = obj;
			foreach (Vector2 tile in array)
			{
				terrainFeatures.Add(tile, new FruitTree((628 + Game1.random.Next(2)).ToString(), 4));
			}
			for (int x5 = 3; x5 < 15; x5++)
			{
				for (int y5 = 36; y5 < 45; y5++)
				{
					if (farm.CanItemBePlacedHere(new Vector2(x5, y5)))
					{
						Object keg = ItemRegistry.Create<Object>("(BC)12");
						farm.objects.Add(new Vector2(x5, y5), keg);
						keg.performObjectDropInAction(ItemRegistry.Create<Object>("(O)454"), probe: false, Game1.player);
					}
				}
			}
			for (int x6 = 16; x6 < 26; x6++)
			{
				for (int y6 = 36; y6 < 45; y6++)
				{
					if (farm.CanItemBePlacedHere(new Vector2(x6, y6)))
					{
						farm.objects.Add(new Vector2(x6, y6), ItemRegistry.Create<Object>("(BC)13"));
					}
				}
			}
			for (int x7 = 3; x7 < 15; x7++)
			{
				for (int y7 = 47; y7 < 57; y7++)
				{
					if (farm.CanItemBePlacedHere(new Vector2(x7, y7)))
					{
						farm.objects.Add(new Vector2(x7, y7), ItemRegistry.Create<Object>("(BC)16"));
					}
				}
			}
			for (int x8 = 16; x8 < 26; x8++)
			{
				for (int y8 = 47; y8 < 57; y8++)
				{
					if (farm.CanItemBePlacedHere(new Vector2(x8, y8)))
					{
						farm.objects.Add(new Vector2(x8, y8), ItemRegistry.Create<Object>("(BC)15"));
					}
				}
			}
			for (int x9 = 28; x9 < 38; x9++)
			{
				for (int y9 = 26; y9 < 46; y9++)
				{
					if (farm.CanItemBePlacedHere(new Vector2(x9, y9)))
					{
						new Torch().placementAction(farm, x9 * 64, y9 * 64, null);
					}
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "hu", "house" })]
		public static void HouseUpgrade(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var upgradeLevel, out var error, "int upgradeLevel"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Utility.getHomeOfFarmer(Game1.player).moveObjectsForHouseUpgrade(upgradeLevel);
			Utility.getHomeOfFarmer(Game1.player).setMapForUpgradeLevel(upgradeLevel);
			Game1.player.HouseUpgradeLevel = upgradeLevel;
			Game1.addNewFarmBuildingMaps();
			Utility.getHomeOfFarmer(Game1.player).ReadWallpaperAndFloorTileData();
			Utility.getHomeOfFarmer(Game1.player).RefreshFloorObjectNeighbors();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "thu", "thishouse" })]
		public static void ThisHouseUpgrade(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var upgradeLevel, out var error, "int upgradeLevel"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			FarmHouse house = (Game1.currentLocation?.getBuildingAt(Game1.player.Tile + new Vector2(0f, -1f))?.GetIndoors() as FarmHouse) ?? (Game1.currentLocation as FarmHouse);
			if (house != null)
			{
				house.moveObjectsForHouseUpgrade(upgradeLevel);
				house.setMapForUpgradeLevel(upgradeLevel);
				house.upgradeLevel = upgradeLevel;
				Game1.addNewFarmBuildingMaps();
				house.ReadWallpaperAndFloorTileData();
				house.RefreshFloorObjectNeighbors();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "ci" })]
		public static void Clear(string[] command, IGameLogger log)
		{
			Game1.player.clearBackpack();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "w" })]
		public static void Wall(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var wallpaperId, out var error, allowBlank: false, "string wallpaperId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.RequireLocation<FarmHouse>("FarmHouse").SetWallpaper(wallpaperId, null);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Floor(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var floorId, out var error, allowBlank: false, "string floorId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.RequireLocation<FarmHouse>("FarmHouse").SetFloor(floorId, null);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Sprinkle(string[] command, IGameLogger log)
		{
			Utility.addSprinklesToLocation(Game1.currentLocation, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, 7, 7, 2000, 100, Color.White);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearMail(string[] command, IGameLogger log)
		{
			Game1.player.mailReceived.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void BroadcastMailbox(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var mailId, out var error, allowBlank: false, "string mailId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.addMail(mailId, noLetter: false, sendToEveryone: true);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "mft" })]
		public static void MailForTomorrow(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var mailId, out var error, allowBlank: false, "string mailId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.addMailForTomorrow(mailId, command.Length > 2);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void AllMail(string[] command, IGameLogger log)
		{
			foreach (string key in DataLoader.Mail(Game1.content).Keys)
			{
				Game1.addMailForTomorrow(key);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void AllMailRead(string[] command, IGameLogger log)
		{
			foreach (string key in DataLoader.Mail(Game1.content).Keys)
			{
				Game1.player.mailReceived.Add(key);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ShowMail(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var mailId, out var error, allowBlank: false, "string mailId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.activeClickableMenu = new LetterViewerMenu(DataLoader.Mail(Game1.content).GetValueOrDefault(mailId, ""), mailId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "where" })]
		public static void WhereIs(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			List<string> lines = new List<string>();
			if (Game1.CurrentEvent != null)
			{
				foreach (NPC npc in Game1.CurrentEvent.actors)
				{
					if (Utility.fuzzyCompare(npcName, npc.Name).HasValue)
					{
						lines.Add($"{npc.Name} is in this event at ({npc.TilePoint.X}, {npc.TilePoint.Y})");
					}
				}
			}
			Utility.ForEachCharacter(delegate(NPC character)
			{
				if (Utility.fuzzyCompare(npcName, character.Name).HasValue)
				{
					lines.Add($"'{character.Name}'{(character.EventActor ? " (event actor)" : "")} is at {character.currentLocation.NameOrUniqueName} ({character.TilePoint.X}, {character.TilePoint.Y})");
				}
				return true;
			}, includeEventActors: true);
			if (lines.Any())
			{
				log.Info(string.Join("\n", lines));
			}
			else
			{
				log.Error("No NPC found matching '" + npcName + "'.");
			}
		}

		/// <summary>List the locations of every item in the game state matching a given item ID or name.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "whereItem" })]
		public static void WhereIsItem(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var itemNameOrId, out var error, allowBlank: false, "string itemNameOrId"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			string itemId = ItemRegistry.GetData(itemNameOrId)?.QualifiedItemId;
			List<string> lines = new List<string>();
			long count = 0L;
			Utility.ForEachItemContext(delegate(in ForEachItemContext context)
			{
				Item item = context.Item;
				bool num;
				if (itemId == null)
				{
					if (Utility.fuzzyCompare(itemNameOrId, item.Name).HasValue)
					{
						goto IL_005e;
					}
					num = Utility.fuzzyCompare(itemNameOrId, item.DisplayName).HasValue;
				}
				else
				{
					num = item.QualifiedItemId == itemId;
				}
				if (num)
				{
					goto IL_005e;
				}
				goto IL_011c;
				IL_005e:
				count += Math.Min(item.Stack, 1);
				lines.Add($"  - {string.Join(" > ", context.GetDisplayPath(includeItem: true))} ({item.QualifiedItemId}{((item.Stack > 1) ? $" x {item.Stack}" : "")})");
				goto IL_011c;
				IL_011c:
				return true;
			});
			string label = ((itemId != null) ? ("ID '" + itemId + "'") : ("name '" + itemNameOrId + "'"));
			if (lines.Any())
			{
				log.Info($"Found {count} item{((count > 1) ? "s" : "")} matching {label}:\n{string.Join("\n", lines)}");
			}
			else
			{
				log.Error("No item found matching " + label + ".");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "pm" })]
		public static void PanMode(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptional(command, 1, out var option, out var error, null, allowBlank: false, "string option"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else if (option == null)
			{
				if (!Game1.panMode)
				{
					Game1.panMode = true;
					Game1.viewportFreeze = true;
					Game1.debugMode = true;
					Game1.game1.panFacingDirectionWait = false;
					Game1.game1.panModeString = "";
					log.Info("Screen pan mode enabled.");
				}
				else
				{
					Game1.panMode = false;
					Game1.viewportFreeze = false;
					Game1.game1.panModeString = "";
					Game1.debugMode = false;
					Game1.game1.panFacingDirectionWait = false;
					Game1.inputSimulator = null;
					log.Info("Screen pan mode disabled.");
				}
			}
			else if (Game1.panMode)
			{
				int time;
				string error2;
				if (option == "clear")
				{
					Game1.game1.panModeString = "";
					Game1.game1.panFacingDirectionWait = false;
				}
				else if (ArgUtility.TryGetInt(command, 1, out time, out error2, "int time"))
				{
					if (!Game1.game1.panFacingDirectionWait)
					{
						Game1 game = Game1.game1;
						game.panModeString = game.panModeString + ((Game1.game1.panModeString.Length > 0) ? "/" : "") + time + " ";
						log.Info(Game1.game1.panModeString + Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3191"));
					}
				}
				else
				{
					DebugCommands.LogArgError(log, command, "the first argument must be omitted (to toggle pan mode), 'clear', or a numeric time");
				}
			}
			else
			{
				log.Error("Screen pan mode isn't enabled. You can enable it by using this command without arguments.");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "is" })]
		public static void InputSim(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var option, out var error, allowBlank: false, "string option"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.inputSimulator = null;
			string text = option.ToLower();
			if (!(text == "spamtool"))
			{
				if (text == "spamlr")
				{
					Game1.inputSimulator = new LeftRightClickSpamInputSimulator();
				}
				else
				{
					log.Error("No input simulator found for " + option);
				}
			}
			else
			{
				Game1.inputSimulator = new ToolSpamInputSimulator();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Hurry(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Utility.fuzzyCharacterSearch(npcName).warpToPathControllerDestination();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MorePollen(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var amount, out var error, "int amount"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			for (int i = 0; i < amount; i++)
			{
				Game1.debrisWeather.Add(new WeatherDebris(new Vector2(Game1.random.Next(0, Game1.graphics.GraphicsDevice.Viewport.Width), Game1.random.Next(0, Game1.graphics.GraphicsDevice.Viewport.Height)), 0, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FillWithObject(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var id, out var error, allowBlank: false, "string id") || !ArgUtility.TryGetOptionalBool(command, 2, out var bigCraftable, out error, defaultValue: false, "bool bigCraftable"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			for (int y = 0; y < Game1.currentLocation.map.Layers[0].LayerHeight; y++)
			{
				for (int x = 0; x < Game1.currentLocation.map.Layers[0].LayerWidth; x++)
				{
					Vector2 loc = new Vector2(x, y);
					if (Game1.currentLocation.CanItemBePlacedHere(loc))
					{
						string typeId = (bigCraftable ? "(BC)" : "(O)");
						Game1.currentLocation.setObject(loc, ItemRegistry.Create<Object>(typeId + id));
					}
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SpawnWeeds(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var spawnPasses, out var error, "int spawnPasses"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			for (int i = 0; i < spawnPasses; i++)
			{
				Game1.currentLocation.spawnWeedsAndStones(1);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void BusDriveBack(string[] command, IGameLogger log)
		{
			Game1.RequireLocation<BusStop>("BusStop").busDriveBack();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void BusDriveOff(string[] command, IGameLogger log)
		{
			Game1.RequireLocation<BusStop>("BusStop").busDriveOff();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CompleteJoja(string[] command, IGameLogger log)
		{
			Game1.player.mailReceived.Add("ccCraftsRoom");
			Game1.player.mailReceived.Add("ccVault");
			Game1.player.mailReceived.Add("ccFishTank");
			Game1.player.mailReceived.Add("ccBoilerRoom");
			Game1.player.mailReceived.Add("ccPantry");
			Game1.player.mailReceived.Add("jojaCraftsRoom");
			Game1.player.mailReceived.Add("jojaVault");
			Game1.player.mailReceived.Add("jojaFishTank");
			Game1.player.mailReceived.Add("jojaBoilerRoom");
			Game1.player.mailReceived.Add("jojaPantry");
			Game1.player.mailReceived.Add("JojaMember");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CompleteCc(string[] command, IGameLogger log)
		{
			Game1.player.mailReceived.Add("ccCraftsRoom");
			Game1.player.mailReceived.Add("ccVault");
			Game1.player.mailReceived.Add("ccFishTank");
			Game1.player.mailReceived.Add("ccBoilerRoom");
			Game1.player.mailReceived.Add("ccPantry");
			Game1.player.mailReceived.Add("ccBulletin");
			Game1.player.mailReceived.Add("ccBoilerRoom");
			Game1.player.mailReceived.Add("ccPantry");
			Game1.player.mailReceived.Add("ccBulletin");
			CommunityCenter ccc = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
			for (int i = 0; i < ccc.areasComplete.Count; i++)
			{
				ccc.markAreaAsComplete(i);
				ccc.areasComplete[i] = true;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Break(string[] command, IGameLogger log)
		{
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void WhereOre(string[] command, IGameLogger log)
		{
			log.Info(Convert.ToString(Game1.currentLocation.orePanPoint.Value));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void AllBundles(string[] command, IGameLogger log)
		{
			foreach (KeyValuePair<int, NetArray<bool, NetBool>> b in Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundles.FieldDict)
			{
				for (int j = 0; j < b.Value.Count; j++)
				{
					b.Value[j] = true;
				}
			}
			Game1.playSound("crystal", 0);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void JunimoGoodbye(string[] command, IGameLogger log)
		{
			if (!(Game1.currentLocation is CommunityCenter communityCenter))
			{
				log.Error("The JunimoGoodbye command must be run while inside the community center.");
			}
			else
			{
				communityCenter.junimoGoodbyeDance();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Bundle(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var bundleKey, out var error, "int bundleKey"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			foreach (KeyValuePair<int, NetArray<bool, NetBool>> b in Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundles.FieldDict)
			{
				if (b.Key == bundleKey)
				{
					for (int j = 0; j < b.Value.Count; j++)
					{
						b.Value[j] = true;
					}
				}
			}
			Game1.playSound("crystal", 0);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "lu" })]
		public static void Lookup(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var search, out var error, ' ', "string search"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			foreach (ParsedItemData item in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				if (item.InternalName.EqualsIgnoreCase(search))
				{
					log.Info(item.InternalName + " " + item.ItemId);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CcLoadCutscene(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var areaId, out var error, "int areaId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.RequireLocation<CommunityCenter>("CommunityCenter").restoreAreaCutscene(areaId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CcLoad(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var areaId, out var error, "int areaId"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.RequireLocation<CommunityCenter>("CommunityCenter").loadArea(areaId);
			Game1.RequireLocation<CommunityCenter>("CommunityCenter").markAreaAsComplete(areaId);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Plaque(string[] command, IGameLogger log)
		{
			Game1.RequireLocation<CommunityCenter>("CommunityCenter").addStarToPlaque();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void JunimoStar(string[] command, IGameLogger log)
		{
			CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
			Junimo junimo = communityCenter.characters.OfType<Junimo>().FirstOrDefault();
			if (junimo == null)
			{
				log.Error("No Junimo found in the community center.");
			}
			else
			{
				junimo.returnToJunimoHutToFetchStar(communityCenter);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "j", "aj" })]
		public static void AddJunimo(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetVector2(command, 1, out var tile, out var error, integerOnly: true, "Vector2 tile") || !ArgUtility.TryGetInt(command, 3, out var areaId, out error, "int areaId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.RequireLocation<CommunityCenter>("CommunityCenter").addCharacter(new Junimo(tile * 64f, areaId));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ResetJunimoNotes(string[] command, IGameLogger log)
		{
			foreach (NetArray<bool, NetBool> b in Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundles.FieldDict.Values)
			{
				for (int i = 0; i < b.Count; i++)
				{
					b[i] = false;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "jn" })]
		public static void JunimoNote(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var areaId, out var error, "int areaId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.RequireLocation<CommunityCenter>("CommunityCenter").addJunimoNote(areaId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void WaterColor(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var red, out var error, "int red") || !ArgUtility.TryGetInt(command, 2, out var green, out error, "int green") || !ArgUtility.TryGetInt(command, 3, out var blue, out error, "int blue"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.currentLocation.waterColor.Value = new Color(red, green, blue) * 0.5f;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FestivalScore(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var score, out var error, "int score"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.festivalScore += score;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void AddOtherFarmer(string[] command, IGameLogger log)
		{
			Farmer f = new Farmer(new FarmerSprite("Characters\\Farmer\\farmer_base"), new Vector2(Game1.player.Position.X - 64f, Game1.player.Position.Y), 2, Dialogue.randomName(), null, isMale: true);
			f.changeShirt(Game1.random.Next(1000, 1040).ToString());
			f.changePantsColor(new Color(Game1.random.Next(255), Game1.random.Next(255), Game1.random.Next(255)));
			f.changeHairStyle(Game1.random.Next(FarmerRenderer.hairStylesTexture.Height / 96 * 8));
			if (Game1.random.NextBool())
			{
				f.changeHat(Game1.random.Next(-1, FarmerRenderer.hatsTexture.Height / 80 * 12));
			}
			else
			{
				Game1.player.changeHat(-1);
			}
			f.changeHairColor(new Color(Game1.random.Next(255), Game1.random.Next(255), Game1.random.Next(255)));
			f.changeSkinColor(Game1.random.Next(16));
			f.currentLocation = Game1.currentLocation;
			Game1.otherFarmers.Add(Game1.random.Next(), f);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void PlayMusic(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var trackName, out var error, allowBlank: false, "string trackName"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.changeMusicTrack(trackName);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Jump(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var target, out var error, allowBlank: false, "string target") || !ArgUtility.TryGetOptionalFloat(command, 2, out var jumpVelocity, out error, 8f, "float jumpVelocity"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else if (target == "farmer")
			{
				Game1.player.jump(jumpVelocity);
			}
			else
			{
				Utility.fuzzyCharacterSearch(target).jump(jumpVelocity);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Toss(string[] command, IGameLogger log)
		{
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(738, 2700f, 1, 0, Game1.player.Tile * 64f, flicker: false, flipped: false)
			{
				rotationChange = (float)Math.PI / 32f,
				motion = new Vector2(0f, -6f),
				acceleration = new Vector2(0f, 0.08f)
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Rain(string[] command, IGameLogger log)
		{
			string contextId = Game1.player.currentLocation.GetLocationContextId();
			LocationWeather weather = Game1.netWorldState.Value.GetWeatherForLocation(contextId);
			weather.IsRaining = !weather.IsRaining;
			weather.IsDebrisWeather = false;
			if (contextId == "Default")
			{
				Game1.isRaining = weather.IsRaining;
				Game1.isDebrisWeather = false;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GreenRain(string[] command, IGameLogger log)
		{
			string contextId = Game1.player.currentLocation.GetLocationContextId();
			LocationWeather weather = Game1.netWorldState.Value.GetWeatherForLocation(contextId);
			weather.IsGreenRain = !weather.IsGreenRain;
			weather.IsDebrisWeather = false;
			if (contextId == "Default")
			{
				Game1.isRaining = weather.IsRaining;
				Game1.isGreenRain = weather.IsGreenRain;
				Game1.isDebrisWeather = false;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "sf" })]
		public static void SetFrame(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var animationId, out var error, "int animationId"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.player.FarmerSprite.PauseForSingleAnimation = true;
			Game1.player.FarmerSprite.setCurrentSingleAnimation(animationId);
		}

		/// <summary>Immediately end the current event.</summary>
		[OtherNames(new string[] { "ee" })]
		public static void EndEvent(string[] command, IGameLogger log)
		{
			Event @event = Game1.CurrentEvent;
			if (@event == null)
			{
				log.Warn("Can't end an event because there's none playing.");
				return;
			}
			if (@event.id == "1590166")
			{
				Game1.player.mailReceived.Add("rejectedPet");
			}
			@event.skipped = true;
			@event.skipEvent();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Language(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new LanguageSelectionMenu();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "rte" })]
		public static void RunTestEvent(string[] command, IGameLogger log)
		{
			Game1.runTestEvent();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "qb" })]
		public static void QiBoard(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new SpecialOrdersBoard("Qi");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "ob" })]
		public static void OrdersBoard(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new SpecialOrdersBoard();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ReturnedDonations(string[] command, IGameLogger log)
		{
			Game1.player.team.CheckReturnedDonations();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "cso" })]
		public static void CompleteSpecialOrders(string[] command, IGameLogger log)
		{
			foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
			{
				foreach (OrderObjective objective in specialOrder.objectives)
				{
					objective.SetCount(objective.maxCount.Value);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SpecialOrder(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var orderId, out var error, allowBlank: false, "string orderId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.team.AddSpecialOrder(orderId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void BoatJourney(string[] command, IGameLogger log)
		{
			Game1.currentMinigame = new BoatJourney();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Minigame(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var minigame, out var error, allowBlank: false, "string minigame"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			switch (minigame)
			{
			case "cowboy":
				Game1.updateViewportForScreenSizeChange(fullscreenChange: false, Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight);
				Game1.currentMinigame = new AbigailGame();
				break;
			case "blastoff":
				Game1.currentMinigame = new RobotBlastoff();
				break;
			case "minecart":
				Game1.currentMinigame = new MineCart(0, 3);
				break;
			case "grandpa":
				Game1.currentMinigame = new GrandpaStory();
				break;
			case "marucomet":
				Game1.currentMinigame = new MaruComet();
				break;
			case "haleyCows":
				Game1.currentMinigame = new HaleyCowPictures();
				break;
			case "plane":
				Game1.currentMinigame = new PlaneFlyBy();
				break;
			case "slots":
				Game1.currentMinigame = new Slots();
				break;
			case "target":
				Game1.currentMinigame = new TargetGame();
				break;
			case "fishing":
				Game1.currentMinigame = new FishingGame();
				break;
			case "intro":
				Game1.currentMinigame = new Intro();
				break;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Event(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var locationName, out var error, allowBlank: false, "string locationName") || !ArgUtility.TryGetInt(command, 2, out var eventIndex, out error, "int eventIndex") || !ArgUtility.TryGetOptionalBool(command, 3, out var clearEventsSeen, out error, defaultValue: true, "bool clearEventsSeen"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			GameLocation location = Utility.fuzzyLocationSearch(locationName);
			if (location == null)
			{
				log.Error("No location with name " + locationName);
				return;
			}
			locationName = location.Name;
			if (locationName == "Pool")
			{
				locationName = "BathHouse_Pool";
			}
			if (clearEventsSeen)
			{
				Game1.player.eventsSeen.Clear();
			}
			string assetName = "Data\\Events\\" + locationName;
			KeyValuePair<string, string> entry = Game1.content.Load<Dictionary<string, string>>(assetName).ElementAt(eventIndex);
			if (entry.Key.Contains('/'))
			{
				LocationRequest locationRequest = Game1.getLocationRequest(locationName);
				locationRequest.OnLoad += delegate
				{
					Game1.currentLocation.currentEvent = new Event(entry.Value, assetName, StardewValley.Event.SplitPreconditions(entry.Key)[0]);
				};
				Game1.warpFarmer(locationRequest, 8, 8, Game1.player.FacingDirection);
			}
		}

		/// <summary>Find an event by ID and play it.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "ebi" })]
		public static void EventById(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var eventId, out var error, allowBlank: false, "string eventId"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.player.eventsSeen.Remove(eventId);
			Game1.eventsSeenSinceLastLocationChange.Remove(eventId);
			if (Game1.PlayEvent(eventId, checkPreconditions: false, checkSeen: false))
			{
				log.Info("Starting event " + eventId);
			}
			else
			{
				log.Error("Event '" + eventId + "' not found.");
			}
		}

		public static void EventScript(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var location, out var error, allowBlank: true, "string location") || !ArgUtility.TryGetRemainder(command, 2, out var script, out error, ' ', "string script"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else if (location != Game1.currentLocation.Name)
			{
				LocationRequest locationRequest = Game1.getLocationRequest(location);
				locationRequest.OnLoad += delegate
				{
					Game1.currentLocation.currentEvent = new Event(script);
				};
				int x = 8;
				int y = 8;
				Utility.getDefaultWarpLocation(locationRequest.Name, ref x, ref y);
				Game1.warpFarmer(locationRequest, x, y, Game1.player.FacingDirection);
			}
			else
			{
				Game1.globalFadeToBlack(delegate
				{
					Game1.forceSnapOnNextViewportUpdate = true;
					Game1.currentLocation.startEvent(new Event(script));
					Game1.globalFadeToClear();
				});
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "sfe" })]
		public static void SetFarmEvent(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var eventName, out var error, allowBlank: false, "string eventName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Dictionary<string, Func<FarmEvent>> farmEvents = new Dictionary<string, Func<FarmEvent>>(StringComparer.OrdinalIgnoreCase)
			{
				["dogs"] = () => new SoundInTheNightEvent(2),
				["earthquake"] = () => new SoundInTheNightEvent(4),
				["fairy"] = () => new FairyEvent(),
				["meteorite"] = () => new SoundInTheNightEvent(1),
				["owl"] = () => new SoundInTheNightEvent(3),
				["racoon"] = () => new SoundInTheNightEvent(5),
				["ufo"] = () => new SoundInTheNightEvent(0),
				["witch"] = () => new WitchEvent()
			};
			if (farmEvents.TryGetValue(eventName, out var getEvent))
			{
				Game1.farmEventOverride = getEvent();
				log.Info("Set farm event to '" + eventName + "'! The event will play if no other nightly event plays normally.");
			}
			else
			{
				log.Error("Unknown event type; expected one of '" + string.Join("', '", farmEvents.Keys) + "'.");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void TestWedding(string[] command, IGameLogger log)
		{
			Event weddingEvent = Utility.getWeddingEvent(Game1.player);
			LocationRequest locationRequest = Game1.getLocationRequest("Town");
			locationRequest.OnLoad += delegate
			{
				Game1.currentLocation.currentEvent = weddingEvent;
			};
			int x = 8;
			int y = 8;
			Utility.getDefaultWarpLocation(locationRequest.Name, ref x, ref y);
			Game1.warpFarmer(locationRequest, x, y, Game1.player.FacingDirection);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Festival(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var festivalId, out var error, allowBlank: false, "string festivalId"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Dictionary<string, string> festivalData = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + festivalId);
			if (festivalData != null)
			{
				string season = new string(festivalId.Where(char.IsLetter).ToArray());
				int day = Convert.ToInt32(new string(festivalId.Where(char.IsDigit).ToArray()));
				Game1.game1.parseDebugInput("Season " + season, log);
				Game1.game1.parseDebugInput($"{"Day"} {day}", log);
				string[] array = festivalData["conditions"].Split('/');
				int startTime = Convert.ToInt32(ArgUtility.SplitBySpaceAndGet(array[1], 0));
				Game1.game1.parseDebugInput($"{"Time"} {startTime}", log);
				string where = array[0];
				Game1.game1.parseDebugInput("Warp " + where + " 1 1", log);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "ps" })]
		public static void PlaySound(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var soundId, out var error, allowBlank: false, "string soundId") || !ArgUtility.TryGetOptionalInt(command, 2, out var pitch, out error, -1, "int pitch"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else if (pitch > -1)
			{
				Game1.playSound(soundId, pitch);
			}
			else
			{
				Game1.playSound(soundId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void LogSounds(string[] command, IGameLogger log)
		{
			Game1.sounds.LogSounds = !Game1.sounds.LogSounds;
			log.Info((Game1.sounds.LogSounds ? "Enabled" : "Disabled") + " sound logging.");
		}

		[OtherNames(new string[] { "poali" })]
		public static void PrintOpenAlInfo(string[] command, IGameLogger log)
		{
			Type oalType = Assembly.GetAssembly(Game1.staminaRect.GetType())?.GetType("Microsoft.Xna.Framework.Audio.OpenALSoundController");
			if ((object)oalType == null)
			{
				log.Error("Could not find type 'OpenALSoundController'");
			}
			else
			{
				if (!TryGetField("_instance", BindingFlags.Static | BindingFlags.NonPublic, out var instanceField) || !TryGetField("availableSourcesCollection", BindingFlags.Instance | BindingFlags.NonPublic, out var availableField) || !TryGetField("inUseSourcesCollection", BindingFlags.Instance | BindingFlags.NonPublic, out var inUseField))
				{
					return;
				}
				object instanceObject = instanceField.GetValue(null);
				if (instanceObject == null)
				{
					log.Error("OpenALSoundController._instance is null");
					return;
				}
				if (instanceObject.GetType() != oalType)
				{
					log.Error("OpenALSoundController._instance is not an instance of " + oalType.ToString());
					return;
				}
				object? value = availableField.GetValue(instanceObject);
				object inUseObject = inUseField.GetValue(instanceObject);
				List<int> availableSourcesCollection = value as List<int>;
				List<int> inUseSourcesCollection = inUseObject as List<int>;
				if (availableSourcesCollection == null)
				{
					log.Error("OpenALSoundController._instance.availableSourcesCollection is not an instance of List<int>");
					return;
				}
				if (inUseSourcesCollection == null)
				{
					log.Error("OpenALSoundController._instance.inUseSourcesCollection is not an instance of List<int>");
					return;
				}
				log.Info($"Available: {availableSourcesCollection.Count}\nIn Use: {inUseSourcesCollection.Count}");
			}
			bool TryGetField(string fieldName, BindingFlags fieldFlags, out FieldInfo destField)
			{
				destField = oalType.GetField(fieldName, fieldFlags);
				if ((object)destField == null)
				{
					log.Error("OpenALSoundController does not have field '" + fieldName + "'");
					return false;
				}
				return true;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Crafting(string[] command, IGameLogger log)
		{
			foreach (string s in CraftingRecipe.craftingRecipes.Keys)
			{
				Game1.player.craftingRecipes.TryAdd(s, 0);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Cooking(string[] command, IGameLogger log)
		{
			foreach (string s in CraftingRecipe.cookingRecipes.Keys)
			{
				Game1.player.cookingRecipes.TryAdd(s, 0);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Experience(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var skill, out var error, allowBlank: false, "string skill") | !ArgUtility.TryGetInt(command, 2, out var experiencePoints, out error, "int experiencePoints"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			switch (skill.ToLower())
			{
			case "all":
				Game1.player.gainExperience(0, experiencePoints);
				Game1.player.gainExperience(1, experiencePoints);
				Game1.player.gainExperience(3, experiencePoints);
				Game1.player.gainExperience(2, experiencePoints);
				Game1.player.gainExperience(4, experiencePoints);
				break;
			case "farming":
				Game1.player.gainExperience(0, experiencePoints);
				break;
			case "fishing":
				Game1.player.gainExperience(1, experiencePoints);
				break;
			case "mining":
				Game1.player.gainExperience(3, experiencePoints);
				break;
			case "foraging":
				Game1.player.gainExperience(2, experiencePoints);
				break;
			case "combat":
				Game1.player.gainExperience(4, experiencePoints);
				break;
			default:
			{
				if (int.TryParse(skill, out var which))
				{
					Game1.player.gainExperience(which, experiencePoints);
				}
				else
				{
					DebugCommands.LogArgError(log, command, "unknown skill ID '" + skill + "'");
				}
				break;
			}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ShowExperience(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var skillId, out var error, "int skillId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				log.Info(Game1.player.experiencePoints[skillId].ToString());
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Profession(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var professionId, out var error, "int professionId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.professions.Add(professionId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ClearFishCaught(string[] command, IGameLogger log)
		{
			Game1.player.fishCaught.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "caughtFish" })]
		public static void FishCaught(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var count, out var error, "int count"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.stats.FishCaught = (uint)count;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "r" })]
		public static void ResetForPlayerEntry(string[] command, IGameLogger log)
		{
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.currentLocation.resetForPlayerEntry();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Fish(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var fishId, out var error, allowBlank: false, "string fishId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else if (Game1.player.CurrentTool is FishingRod rod)
			{
				List<string> tackleIds = rod.GetTackleQualifiedItemIDs();
				Game1.activeClickableMenu = new BobberBar(fishId, 0.5f, treasure: true, tackleIds, null, isBossFish: false);
			}
			else
			{
				log.Error("The player must have a fishing rod equipped to use this command.");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GrowAnimals(string[] command, IGameLogger log)
		{
			foreach (FarmAnimal value in Game1.currentLocation.animals.Values)
			{
				value.growFully();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void PauseAnimals(string[] command, IGameLogger log)
		{
			foreach (FarmAnimal value in Game1.currentLocation.Animals.Values)
			{
				value.pauseTimer = int.MaxValue;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void UnpauseAnimals(string[] command, IGameLogger log)
		{
			foreach (FarmAnimal value in Game1.currentLocation.Animals.Values)
			{
				value.pauseTimer = 0;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "removetf" })]
		public static void RemoveTerrainFeatures(string[] command, IGameLogger log)
		{
			Game1.currentLocation.terrainFeatures.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MushroomTrees(string[] command, IGameLogger log)
		{
			foreach (TerrainFeature value in Game1.currentLocation.terrainFeatures.Values)
			{
				if (value is Tree tree)
				{
					tree.treeType.Value = "7";
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void TrashCan(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var trashCanLevel, out var error, "int trashCanLevel"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.trashCanLevel = trashCanLevel;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FruitTrees(string[] command, IGameLogger log)
		{
			foreach (KeyValuePair<Vector2, TerrainFeature> pair in Game1.currentLocation.terrainFeatures.Pairs)
			{
				if (pair.Value is FruitTree tree)
				{
					tree.daysUntilMature.Value -= 27;
					tree.dayUpdate();
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Train(string[] command, IGameLogger log)
		{
			Game1.RequireLocation<Railroad>("Railroad").setTrainComing(7500);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DebrisWeather(string[] command, IGameLogger log)
		{
			string contextId = Game1.player.currentLocation.GetLocationContextId();
			LocationWeather weather = Game1.netWorldState.Value.GetWeatherForLocation(contextId);
			weather.IsDebrisWeather = !weather.IsDebrisWeather;
			if (contextId == "Default")
			{
				Game1.isDebrisWeather = weather.isDebrisWeather.Value;
			}
			Game1.debrisWeather.Clear();
			if (weather.IsDebrisWeather)
			{
				Game1.populateDebrisWeatherArray();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Speed(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var speed, out var error, "int speed") || !ArgUtility.TryGetOptionalInt(command, 2, out var minutes, out error, 30, "int minutes"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			BuffEffects effects = new BuffEffects();
			effects.Speed.Value = speed;
			Game1.player.applyBuff(new Buff("debug_speed", "Debug Speed", "Debug Speed", minutes * Game1.realMilliSecondsPerGameMinute, null, 0, effects));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DayUpdate(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var days, out var error, "int days"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			for (int i = 0; i < days; i++)
			{
				Game1.currentLocation.DayUpdate(Game1.dayOfMonth);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FarmerDayUpdate(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var days, out var error, "int days"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			for (int i = 0; i < days; i++)
			{
				Game1.player.dayupdate(Game1.timeOfDay);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MuseumLoot(string[] command, IGameLogger log)
		{
			foreach (ParsedItemData allDatum in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				string id = allDatum.ItemId;
				string type = allDatum.ObjectType;
				if ((type == "Arch" || type == "Minerals") && !Game1.player.mineralsFound.ContainsKey(id) && !Game1.player.archaeologyFound.ContainsKey(id))
				{
					if (type == "Arch")
					{
						Game1.player.foundArtifact(id, 1);
					}
					else
					{
						Game1.player.addItemToInventoryBool(new Object(id, 1));
					}
				}
				if (Game1.player.freeSpotsInInventory() == 0)
				{
					break;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void NewMuseumLoot(string[] command, IGameLogger log)
		{
			foreach (ParsedItemData allDatum in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				string itemId = allDatum.QualifiedItemId;
				if (LibraryMuseum.IsItemSuitableForDonation(itemId) && !LibraryMuseum.HasDonatedArtifact(itemId))
				{
					Game1.player.addItemToInventoryBool(ItemRegistry.Create(itemId));
				}
				if (Game1.player.freeSpotsInInventory() == 0)
				{
					break;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CreateDebris(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var itemId, out var error, allowBlank: false, "string itemId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.createObjectDebris(itemId, Game1.player.TilePoint.X, Game1.player.TilePoint.Y);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void RemoveDebris(string[] command, IGameLogger log)
		{
			Game1.currentLocation.debris.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void RemoveDirt(string[] command, IGameLogger log)
		{
			Game1.currentLocation.terrainFeatures.RemoveWhere((KeyValuePair<Vector2, TerrainFeature> pair) => pair.Value is HoeDirt);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DyeAll(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.DyePots);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DyeShirt(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new CharacterCustomization(Game1.player.shirtItem.Value);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DyePants(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new CharacterCustomization(Game1.player.pantsItem.Value);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "cmenu", "customize" })]
		public static void CustomizeMenu(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.NewGame);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CopyOutfit(string[] command, IGameLogger log)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("<Item><OutfitParts>");
			if (Game1.player.hat.Value != null)
			{
				sb.Append("<Item><ItemId>" + Game1.player.hat.Value.QualifiedItemId + "</ItemId></Item>");
			}
			if (Game1.player.pantsItem.Value != null)
			{
				sb.Append("<Item><ItemId>" + Game1.player.pantsItem.Value.QualifiedItemId + "</ItemId><Color>" + Game1.player.pantsItem.Value.clothesColor.Value.R + " " + Game1.player.pantsItem.Value.clothesColor.Value.G + " " + Game1.player.pantsItem.Value.clothesColor.Value.B + "</Color></Item>");
			}
			if (Game1.player.shirtItem.Value != null)
			{
				sb.Append("<Item><ItemId>" + Game1.player.shirtItem.Value.QualifiedItemId + "</ItemId><Color>" + Game1.player.shirtItem.Value.clothesColor.Value.R + " " + Game1.player.shirtItem.Value.clothesColor.Value.G + " " + Game1.player.shirtItem.Value.clothesColor.Value.B + "</Color></Item>");
			}
			sb.Append("</OutfitParts></Item>");
			string text = sb.ToString();
			DesktopClipboard.SetText(text);
			Game1.debugOutput = text;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SkinColor(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var skinColor, out var error, "int skinColor"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.changeSkinColor(skinColor);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Hat(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var hatId, out var error, "int hatId"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.player.changeHat(hatId);
			Game1.playSound("coin");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Pants(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var red, out var error, "int red") || !ArgUtility.TryGetInt(command, 2, out var green, out error, "int green") || !ArgUtility.TryGetInt(command, 3, out var blue, out error, "int blue"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.changePantsColor(new Color(red, green, blue));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void HairStyle(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var hairStyle, out var error, "int hairStyle"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.changeHairStyle(hairStyle);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void HairColor(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var red, out var error, "int red") || !ArgUtility.TryGetInt(command, 2, out var green, out error, "int green") || !ArgUtility.TryGetInt(command, 3, out var blue, out error, "int blue"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.changeHairColor(new Color(red, green, blue));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Shirt(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var shirtId, out var error, allowBlank: false, "string shirtId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.changeShirt(shirtId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "m", "mv" })]
		public static void MusicVolume(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetFloat(command, 1, out var volume, out var error, "float volume"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.musicPlayerVolume = volume;
			Game1.options.musicVolumeLevel = volume;
			Game1.musicCategory.SetVolume(Game1.options.musicVolumeLevel);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void RemoveObjects(string[] command, IGameLogger log)
		{
			Game1.currentLocation.objects.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ListLights(string[] command, IGameLogger log)
		{
			StringBuilder report = new StringBuilder();
			StringBuilder stringBuilder = report;
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(69, 6, stringBuilder);
			handler.AppendLiteral("The viewport covers tiles (");
			handler.AppendFormatted(Game1.viewport.X / 64);
			handler.AppendLiteral(", ");
			handler.AppendFormatted(Game1.viewport.Y / 64);
			handler.AppendLiteral(") through (");
			handler.AppendFormatted(Game1.viewport.MaxCorner.X / 64);
			handler.AppendLiteral(", ");
			handler.AppendFormatted(Game1.viewport.MaxCorner.Y / 64);
			handler.AppendLiteral("), with the player at (");
			handler.AppendFormatted(Game1.player.TilePoint.X);
			handler.AppendLiteral(", ");
			handler.AppendFormatted(Game1.player.TilePoint.Y);
			handler.AppendLiteral(").");
			stringBuilder2.AppendLine(ref handler);
			report.AppendLine();
			if (Game1.currentLightSources.Count > 0)
			{
				foreach (IGrouping<bool, KeyValuePair<string, LightSource>> item in from p in Game1.currentLightSources.ToLookup((KeyValuePair<string, LightSource> keyValuePair) => keyValuePair.Value.IsOnScreen())
					orderby p.Key descending
					select p)
				{
					bool inView = item.Key;
					KeyValuePair<string, LightSource>[] lights = item.ToArray();
					if (lights.Length == 0)
					{
						continue;
					}
					stringBuilder = report;
					StringBuilder stringBuilder3 = stringBuilder;
					handler = new StringBuilder.AppendInterpolatedStringHandler(8, 1, stringBuilder);
					handler.AppendLiteral("Lights ");
					handler.AppendFormatted(inView ? "in view" : "out of view");
					handler.AppendLiteral(":");
					stringBuilder3.AppendLine(ref handler);
					int i = 1;
					KeyValuePair<string, LightSource>[] array = lights;
					for (int num = 0; num < array.Length; num++)
					{
						KeyValuePair<string, LightSource> pair = array[num];
						LightSource light = pair.Value;
						Vector2 tile = new Vector2(light.position.X / 64f, light.position.Y / 64f);
						stringBuilder = report;
						StringBuilder stringBuilder4 = stringBuilder;
						handler = new StringBuilder.AppendInterpolatedStringHandler(32, 5, stringBuilder);
						handler.AppendLiteral("  ");
						handler.AppendFormatted(i++);
						handler.AppendLiteral(". '");
						handler.AppendFormatted(light.Id);
						handler.AppendLiteral("' at tile (");
						handler.AppendFormatted(tile.X);
						handler.AppendLiteral(", ");
						handler.AppendFormatted(tile.Y);
						handler.AppendLiteral(") with radius ");
						handler.AppendFormatted(light.radius.Value);
						stringBuilder4.Append(ref handler);
						if (light.onlyLocation.Value != null && light.onlyLocation.Value != Game1.currentLocation?.NameOrUniqueName)
						{
							stringBuilder = report;
							StringBuilder stringBuilder5 = stringBuilder;
							handler = new StringBuilder.AppendInterpolatedStringHandler(28, 1, stringBuilder);
							handler.AppendLiteral(" [only shown in location '");
							handler.AppendFormatted(light.onlyLocation.Value);
							handler.AppendLiteral("']");
							stringBuilder5.Append(ref handler);
						}
						if (light.Id != pair.Key)
						{
							stringBuilder = report;
							StringBuilder stringBuilder6 = stringBuilder;
							handler = new StringBuilder.AppendInterpolatedStringHandler(74, 2, stringBuilder);
							handler.AppendLiteral(" [WARNING: ID mismatch between dictionary lookup (");
							handler.AppendFormatted(pair.Key);
							handler.AppendLiteral(") and light instance (");
							handler.AppendFormatted(light.Id);
							handler.AppendLiteral(")]");
							stringBuilder6.Append(ref handler);
						}
						report.AppendLine(".");
					}
					report.AppendLine();
				}
			}
			else
			{
				report.AppendLine("There are no current light sources.");
			}
			log.Info(report.ToString().TrimEnd());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void RemoveLights(string[] command, IGameLogger log)
		{
			Game1.currentLightSources.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "i" })]
		public static void Item(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var itemId, out var error, allowBlank: false, "string itemId") || !ArgUtility.TryGetOptionalInt(command, 2, out var count, out error, 1, "int count") || !ArgUtility.TryGetOptionalInt(command, 3, out var quality, out error, 0, "int quality"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Item item = ItemRegistry.Create(itemId, count, quality);
			Game1.playSound("coin");
			Game1.player.addItemToInventoryBool(item);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "iq" })]
		public static void ItemQuery(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var query, out var error, ' ', "string query"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			ItemQueryResult[] result = ItemQueryResolver.TryResolve(query, null, ItemQuerySearchMode.All, null, null, avoidRepeat: false, null, delegate(string _, string queryError)
			{
				log.Error("Failed parsing that query: " + queryError);
			});
			if (result.Length == 0)
			{
				log.Info("That query did not match any items.");
				return;
			}
			ShopMenu shop = new ShopMenu("DebugItemQuery", new Dictionary<ISalable, ItemStockInformation>());
			ItemQueryResult[] array = result;
			foreach (ItemQueryResult entry in array)
			{
				shop.AddForSale(entry.Item, new ItemStockInformation(0, int.MaxValue));
			}
			Game1.activeClickableMenu = shop;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "gq" })]
		public static void GameQuery(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var query, out var error, ' ', "string query"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			var rows = (from rawQuery in GameStateQuery.SplitRaw(query)
				select new
				{
					Query = rawQuery,
					Result = GameStateQuery.CheckConditions(rawQuery)
				}).ToArray();
			int queryLength = Math.Max("Query".Length, rows.Max(p => p.Query.Length));
			StringBuilder summary = new StringBuilder().AppendLine().Append("   ").Append("Query".PadRight(queryLength, ' '))
				.AppendLine(" | Result")
				.Append("   ")
				.Append("".PadRight(queryLength, '-'))
				.AppendLine(" | ------");
			bool result = true;
			var array = rows;
			foreach (var row in array)
			{
				result = result && row.Result;
				summary.Append("   ").Append(row.Query.PadRight(queryLength, ' ')).Append(" | ")
					.AppendLine(row.Result.ToString().ToLower());
			}
			summary.AppendLine().Append("Overall result: ").Append(result.ToString().ToLower())
				.AppendLine(".");
			log.Info(summary.ToString());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Tokens(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var input, out var error, ' ', "string input"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			string result = TokenParser.ParseText(input);
			log.Info("Result: \"" + result + "\".");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void DyeMenu(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new DyeMenu();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Tailor(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new TailoringMenu();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Forge(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new ForgeMenu();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ListTags(string[] command, IGameLogger log)
		{
			if (Game1.player.CurrentItem == null)
			{
				return;
			}
			string out_string = "Tags on " + Game1.player.CurrentItem.DisplayName + ": ";
			foreach (string tag in Game1.player.CurrentItem.GetContextTags())
			{
				out_string = out_string + tag + " ";
			}
			log.Info(out_string.Trim());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void QualifiedId(string[] command, IGameLogger log)
		{
			if (Game1.player.CurrentItem != null)
			{
				string result = "Qualified ID of " + Game1.player.CurrentItem.DisplayName + ": " + Game1.player.CurrentItem.QualifiedItemId;
				log.Info(result.Trim());
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Dye(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var slot, out var error, allowBlank: false, "string slot") || !ArgUtility.TryGet(command, 2, out var color, out error, allowBlank: false, "string color") || !ArgUtility.TryGetOptionalFloat(command, 3, out var dyeStrength, out error, 1f, "float dyeStrength"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Color target = Color.White;
			switch (color.ToLower().Trim())
			{
			case "black":
				target = Color.Black;
				break;
			case "red":
				target = new Color(220, 0, 0);
				break;
			case "blue":
				target = new Color(0, 100, 220);
				break;
			case "yellow":
				target = new Color(255, 230, 0);
				break;
			case "white":
				target = Color.White;
				break;
			case "green":
				target = new Color(10, 143, 0);
				break;
			}
			string text = slot.ToLower().Trim();
			if (!(text == "shirt"))
			{
				if (text == "pants")
				{
					Game1.player.pantsItem.Value?.Dye(target, dyeStrength);
				}
			}
			else
			{
				Game1.player.shirtItem.Value?.Dye(target, dyeStrength);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GetIndex(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var itemName, out var error, allowBlank: false, "string itemName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Item item = Utility.fuzzyItemSearch(itemName);
			if (item != null)
			{
				log.Info(item.DisplayName + "'s qualified ID is " + item.QualifiedItemId);
			}
			else
			{
				log.Error("No item found with name " + itemName);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "f", "fin" })]
		public static void FuzzyItemNamed(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var itemId, out var error, allowBlank: true, "string itemId") || !ArgUtility.TryGetOptionalInt(command, 2, out var count, out error, 0, "int count") || !ArgUtility.TryGetOptionalInt(command, 3, out var quality, out error, 0, "int quality"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Item item = Utility.fuzzyItemSearch(itemId, count);
			if (item == null)
			{
				log.Error("No item found with name '" + itemId + "'");
				return;
			}
			item.quality.Value = quality;
			MeleeWeapon.attemptAddRandomInnateEnchantment(item, null);
			Game1.player.addItemToInventory(item);
			Game1.playSound("coin");
			log.Info($"Added {item.DisplayName} ({item.QualifiedItemId})");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "in" })]
		public static void ItemNamed(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var itemName, out var error, allowBlank: false, "string itemName") || !ArgUtility.TryGetOptionalInt(command, 2, out var count, out error, 1, "int count") || !ArgUtility.TryGetOptionalInt(command, 3, out var quality, out error, 0, "int quality"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			foreach (ParsedItemData item in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				if (item.InternalName.EqualsIgnoreCase(itemName))
				{
					Game1.player.addItemToInventory(ItemRegistry.Create("(O)" + item.ItemId, count, quality));
					Game1.playSound("coin");
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Achievement(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var achievementId, out var error, "int achievementId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.getAchievement(achievementId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Heal(string[] command, IGameLogger log)
		{
			Game1.player.health = Game1.player.maxHealth;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Die(string[] command, IGameLogger log)
		{
			Game1.player.health = 0;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Energize(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalInt(command, 1, out var stamina, out var error, Game1.player.MaxStamina, "int stamina"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.Stamina = stamina;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Exhaust(string[] command, IGameLogger log)
		{
			Game1.player.Stamina = -15f;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Warp(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var locationName, out var error, allowBlank: false, "string locationName") || !ArgUtility.TryGetOptionalInt(command, 2, out var tileX, out error, -1, "int tileX") || !ArgUtility.TryGetOptionalInt(command, 3, out var tileY, out error, -1, "int tileY"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			if (tileX > -1 && tileY <= -1)
			{
				DebugCommands.LogArgError(log, command, "must specify both X and Y positions, or neither");
				return;
			}
			GameLocation location = Utility.fuzzyLocationSearch(locationName);
			if (location == null)
			{
				log.Error("No location with name " + locationName);
				return;
			}
			if (tileX < 0)
			{
				tileX = 0;
				tileY = 0;
				Utility.getDefaultWarpLocation(location.Name, ref tileX, ref tileY);
			}
			Game1.warpFarmer(new LocationRequest(location.NameOrUniqueName, location.uniqueName.Value != null, location), tileX, tileY, 2);
			log.Info($"Warping Game1.player to {location.NameOrUniqueName} at {tileX}, {tileY}");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "wh" })]
		public static void WarpHome(string[] command, IGameLogger log)
		{
			Game1.warpHome();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Money(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var amount, out var error, "int amount"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.Money = amount;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CatchAllFish(string[] command, IGameLogger log)
		{
			foreach (ParsedItemData itemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				if (itemData.ObjectType == "Fish")
				{
					Game1.player.caughtFish(itemData.ItemId, 9);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ActivateCalicoStatue(string[] command, IGameLogger log)
		{
			Game1.mine.calicoStatueSpot.Value = new Point(8, 8);
			Game1.mine.calicoStatueActivated(new NetPoint(new Point(8, 8)), Point.Zero, new Point(8, 8));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Perfection(string[] command, IGameLogger log)
		{
			Game1.game1.parseDebugInput("CompleteCc", log);
			Game1.game1.parseDebugInput("Specials", log);
			Game1.game1.parseDebugInput("FriendAll", log);
			Game1.game1.parseDebugInput("Cooking", log);
			Game1.game1.parseDebugInput("Crafting", log);
			foreach (string key in Game1.player.craftingRecipes.Keys)
			{
				Game1.player.craftingRecipes[key] = 1;
			}
			foreach (ParsedItemData item in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				string id = item.ItemId;
				if (item.ObjectType == "Fish")
				{
					Game1.player.fishCaught.Add(item.QualifiedItemId, new int[3]);
				}
				if (Object.isPotentialBasicShipped(id, item.Category, item.ObjectType))
				{
					Game1.player.basicShipped.Add(id, 1);
				}
				Game1.player.recipesCooked.Add(id, 1);
			}
			Game1.game1.parseDebugInput("Walnut 130", log);
			Game1.player.mailReceived.Add("CF_Fair");
			Game1.player.mailReceived.Add("CF_Fish");
			Game1.player.mailReceived.Add("CF_Sewer");
			Game1.player.mailReceived.Add("CF_Mines");
			Game1.player.mailReceived.Add("CF_Spouse");
			Game1.player.mailReceived.Add("CF_Statue");
			Game1.player.mailReceived.Add("museumComplete");
			Game1.player.miningLevel.Value = 10;
			Game1.player.fishingLevel.Value = 10;
			Game1.player.foragingLevel.Value = 10;
			Game1.player.combatLevel.Value = 10;
			Game1.player.farmingLevel.Value = 10;
			Farm farm = Game1.getFarm();
			farm.buildStructure("Water Obelisk", new Vector2(0f, 0f), Game1.player, out var constructed, magicalConstruction: true, skipSafetyChecks: true);
			farm.buildStructure("Earth Obelisk", new Vector2(4f, 0f), Game1.player, out constructed, magicalConstruction: true, skipSafetyChecks: true);
			farm.buildStructure("Desert Obelisk", new Vector2(8f, 0f), Game1.player, out constructed, magicalConstruction: true, skipSafetyChecks: true);
			farm.buildStructure("Island Obelisk", new Vector2(12f, 0f), Game1.player, out constructed, magicalConstruction: true, skipSafetyChecks: true);
			farm.buildStructure("Gold Clock", new Vector2(16f, 0f), Game1.player, out constructed, magicalConstruction: true, skipSafetyChecks: true);
			foreach (KeyValuePair<string, string> v in DataLoader.Monsters(Game1.content))
			{
				for (int i = 0; i < 500; i++)
				{
					Game1.stats.monsterKilled(v.Key);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Walnut(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var count, out var error, "int count"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.netWorldState.Value.GoldenWalnuts += count;
			Game1.netWorldState.Value.GoldenWalnutsFound += count;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Gem(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var count, out var error, "int count"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.QiGems += count;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "removeNpc" })]
		public static void KillNpc(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			bool anyFound = false;
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				location.characters.RemoveWhere(delegate(NPC npc)
				{
					if (npc.Name == npcName)
					{
						log.Info("Removed " + npc.Name + " from " + location.NameOrUniqueName);
						anyFound = true;
						return true;
					}
					return false;
				});
				return true;
			});
			if (!anyFound)
			{
				log.Error("Couldn't find " + npcName + " in any locations.");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		/// <remarks>See also <see cref="M:StardewValley.DebugCommands.DefaultHandlers.Dp(System.String[],StardewValley.Logging.IGameLogger)" />.</remarks>
		[OtherNames(new string[] { "dap" })]
		public static void DaysPlayed(string[] command, IGameLogger log)
		{
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3332", (int)Game1.stats.DaysPlayed));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FriendAll(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalInt(command, 1, out var friendship, out var error, 2500, "int friendship"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			if (Game1.year == 1)
			{
				Game1.AddCharacterIfNecessary("Kent", bypassConditions: true);
				Game1.AddCharacterIfNecessary("Leo", bypassConditions: true);
			}
			Utility.ForEachVillager(delegate(NPC n)
			{
				if (!n.CanSocialize && n.Name != "Sandy" && n.Name == "Krobus")
				{
					return true;
				}
				if (n.Name == "Marlon")
				{
					return true;
				}
				if (!Game1.player.friendshipData.ContainsKey(n.Name))
				{
					Game1.player.friendshipData.Add(n.Name, new Friendship());
				}
				Game1.player.changeFriendship(friendship, n);
				return true;
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "friend" })]
		public static void Friendship(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName") || !ArgUtility.TryGetInt(command, 2, out var friendshipPoints, out error, "int friendshipPoints"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			NPC npc = Utility.fuzzyCharacterSearch(npcName);
			if (npc == null)
			{
				log.Error("No character found matching '" + npcName + "'.");
				return;
			}
			if (!Game1.player.friendshipData.TryGetValue(npc.Name, out var friendship))
			{
				friendship = (Game1.player.friendshipData[npc.Name] = new Friendship());
			}
			friendship.Points = friendshipPoints;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GetStat(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var statName, out var error, allowBlank: false, "string statName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			uint value = Game1.stats.Get(statName);
			log.Info($"The '{statName}' stat is set to {value}.");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SetStat(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var statName, out var error, allowBlank: false, "string statName") || !ArgUtility.TryGetInt(command, 2, out var newValue, out error, "int newValue"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.stats.Set(statName, newValue);
			log.Info($"Set '{statName}' stat to {Game1.stats.Get(statName)}.");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "eventSeen" })]
		public static void SeenEvent(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var eventId, out var error, allowBlank: false, "string eventId") || !ArgUtility.TryGetOptionalBool(command, 2, out var seen, out error, defaultValue: true, "bool seen"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.player.eventsSeen.Toggle(eventId, seen);
			if (!seen)
			{
				Game1.eventsSeenSinceLastLocationChange.Remove(eventId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SeenMail(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var mailId, out var error, allowBlank: false, "string mailId") || !ArgUtility.TryGetOptionalBool(command, 2, out var seen, out error, defaultValue: true, "bool seen"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.mailReceived.Toggle(mailId, seen);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CookingRecipe(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var recipeName, out var error, ' ', "string recipeName"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.cookingRecipes.Add(recipeName.Trim(), 0);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "craftingRecipe" })]
		public static void AddCraftingRecipe(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var recipeName, out var error, ' ', "string recipeName"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.craftingRecipes.Add(recipeName.Trim(), 0);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void UpgradeHouse(string[] command, IGameLogger log)
		{
			Game1.player.HouseUpgradeLevel = Math.Min(3, Game1.player.HouseUpgradeLevel + 1);
			Game1.addNewFarmBuildingMaps();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void StopRafting(string[] command, IGameLogger log)
		{
			Game1.player.isRafting = false;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Time(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var time, out var error, "int time"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.timeOfDay = time;
			Game1.outdoorLight = Color.White;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void AddMinute(string[] command, IGameLogger log)
		{
			Game1.addMinute();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void AddHour(string[] command, IGameLogger log)
		{
			Game1.addHour();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Water(string[] command, IGameLogger log)
		{
			Game1.currentLocation?.ForEachDirt(delegate(HoeDirt dirt)
			{
				if (dirt.Pot != null)
				{
					dirt.Pot.Water();
				}
				else
				{
					dirt.state.Value = 1;
				}
				return true;
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GrowCrops(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var days, out var error, "int days"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.currentLocation?.ForEachDirt(delegate(HoeDirt dirt)
			{
				if (dirt?.crop != null)
				{
					for (int i = 0; i < days; i++)
					{
						dirt.crop.newDay(1);
						if (dirt.crop == null)
						{
							break;
						}
					}
				}
				return true;
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "c", "cm" })]
		public static void CanMove(string[] command, IGameLogger log)
		{
			Game1.player.isEating = false;
			Game1.player.CanMove = true;
			Game1.player.UsingTool = false;
			Game1.player.usingSlingshot = false;
			Game1.player.FarmerSprite.PauseForSingleAnimation = false;
			if (Game1.player.CurrentTool is FishingRod fishingRod)
			{
				fishingRod.isFishing = false;
			}
			Game1.player.mount?.dismount();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Backpack(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var increaseBy, out var error, "int increaseBy"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.increaseBackpackSize(Math.Min(36 - Game1.player.Items.Count, increaseBy));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Question(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var questionId, out var error, allowBlank: false, "string questionId") || !ArgUtility.TryGetOptionalBool(command, 2, out var seen, out error, defaultValue: true, "bool seen"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.dialogueQuestionsAnswered.Toggle(questionId, seen);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Year(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var year, out var error, "int year"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.year = year;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Day(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var day, out var error, "int day"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.stats.DaysPlayed = (uint)(Game1.seasonIndex * 28 + day + (Game1.year - 1) * 4 * 28);
			Game1.dayOfMonth = day;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Season(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetEnum<Season>(command, 1, out var season, out var error, "Season season"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.season = season;
			Game1.setGraphicsForSeason();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "dialogue" })]
		public static void AddDialogue(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var search, out var error, allowBlank: false, "string search") || !ArgUtility.TryGetRemainder(command, 2, out var dialogueText, out error, ' ', "string dialogueText"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			NPC npc = Utility.fuzzyCharacterSearch(search);
			if (npc == null)
			{
				log.Error("No NPC found matching search '" + search + "'.");
			}
			else
			{
				Game1.DrawDialogue(new Dialogue(npc, null, dialogueText));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Speech(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var search, out var error, allowBlank: false, "string search") || !ArgUtility.TryGetRemainder(command, 2, out var dialogueText, out error, ' ', "string dialogueText"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			NPC npc = Utility.fuzzyCharacterSearch(search);
			if (npc == null)
			{
				log.Error("No NPC found matching search '" + search + "'.");
			}
			else
			{
				Game1.DrawDialogue(new Dialogue(npc, null, dialogueText));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void LoadDialogue(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName") || !ArgUtility.TryGet(command, 2, out var translationKey, out error, allowBlank: false, "string translationKey"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			NPC npc = Utility.fuzzyCharacterSearch(npcName);
			string text = Game1.content.LoadString(translationKey).Replace("{", "<").Replace("}", ">");
			npc.CurrentDialogue.Push(new Dialogue(npc, translationKey, text));
			Game1.drawDialogue(npc);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Wedding(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.player.spouse = npcName;
			Game1.weddingsToday.Add(Game1.player.UniqueMultiplayerID);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GameMode(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var gameMode, out var error, "int gameMode"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.setGameMode((byte)gameMode);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Volcano(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var level, out var error, "int level"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.warpFarmer(VolcanoDungeon.GetLevelName(level), 0, 1, 2);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MineLevel(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var level, out var error, "int level") || !ArgUtility.TryGetOptionalInt(command, 2, out var layout, out error, -1, "int layout"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			int? forceLayout = layout;
			if (forceLayout < 0)
			{
				forceLayout = null;
			}
			Game1.enterMine(level, forceLayout);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MineInfo(string[] command, IGameLogger log)
		{
			log.Info($"MineShaft.lowestLevelReached = {MineShaft.lowestLevelReached}\nplayer.deepestMineLevel = {Game1.player.deepestMineLevel}");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Viewport(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetPoint(command, 1, out var tilePosition, out var error, "Point tilePosition"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.viewport.X = tilePosition.X * 64;
			Game1.viewport.Y = tilePosition.Y * 64;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MakeInedible(string[] command, IGameLogger log)
		{
			if (Game1.player.ActiveObject != null)
			{
				Game1.player.ActiveObject.edibility.Value = -300;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "watm" })]
		public static void WarpAnimalToMe(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var animalName, out var error, allowBlank: false, "string animalName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			FarmAnimal animal = Utility.fuzzyAnimalSearch(animalName);
			if (animal == null)
			{
				log.Info("Couldn't find character named " + animalName);
				return;
			}
			log.Info("Warping " + animal.displayName);
			animal.currentLocation.Animals.Remove(animal.myID.Value);
			Game1.currentLocation.Animals.Add(animal.myID.Value, animal);
			animal.Position = Game1.player.Position;
			animal.controller = null;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "wctm" })]
		public static void WarpCharacterToMe(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			NPC npc = Utility.fuzzyCharacterSearch(npcName, must_be_villager: false);
			if (npc == null)
			{
				log.Error("Couldn't find character named " + npcName);
				return;
			}
			log.Info("Warping " + npc.displayName);
			Game1.warpCharacter(npc, Game1.currentLocation.Name, new Vector2(Game1.player.TilePoint.X, Game1.player.TilePoint.Y));
			npc.controller = null;
			npc.Halt();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "wc" })]
		public static void WarpCharacter(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName") || !ArgUtility.TryGetPoint(command, 2, out var tile, out error, "Point tile") || !ArgUtility.TryGetOptionalInt(command, 4, out var facingDirection, out error, 2, "int facingDirection"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			NPC npc = Utility.fuzzyCharacterSearch(npcName, must_be_villager: false);
			if (npc == null)
			{
				log.Error("Couldn't find character named " + npcName);
				return;
			}
			Game1.warpCharacter(npc, Game1.currentLocation.Name, tile);
			npc.faceDirection(facingDirection);
			npc.controller = null;
			npc.Halt();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "wtp" })]
		public static void WarpToPlayer(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var playerName, out var error, allowBlank: false, "string playerName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Farmer otherFarmer = Game1.getOnlineFarmers().FirstOrDefault((Farmer other) => other.displayName.EqualsIgnoreCase(playerName));
			if (otherFarmer == null)
			{
				log.Error("Could not find other farmer " + playerName);
				return;
			}
			Game1.game1.parseDebugInput($"{"Warp"} {otherFarmer.currentLocation.NameOrUniqueName} {otherFarmer.TilePoint.X} {otherFarmer.TilePoint.Y}", log);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "wtc" })]
		public static void WarpToCharacter(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			NPC npc = Utility.fuzzyCharacterSearch(npcName);
			if (npc == null)
			{
				log.Error("Could not find valid character " + npcName);
				return;
			}
			Game1.game1.parseDebugInput($"{"Warp"} {Utility.getGameLocationOfCharacter(npc).Name} {npc.TilePoint.X} {npc.TilePoint.Y}", log);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "wct" })]
		public static void WarpCharacterTo(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName") || !ArgUtility.TryGet(command, 2, out var locationName, out error, allowBlank: false, "string locationName") || !ArgUtility.TryGetPoint(command, 3, out var tile, out error, "Point tile") || !ArgUtility.TryGetOptionalInt(command, 5, out var facingDirection, out error, 2, "int facingDirection"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			NPC npc = Utility.fuzzyCharacterSearch(npcName);
			if (npc == null)
			{
				log.Error("Could not find valid character " + npcName);
				return;
			}
			Game1.warpCharacter(npc, locationName, tile);
			npc.faceDirection(facingDirection);
			npc.controller = null;
			npc.Halt();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "ws" })]
		public static void WarpShop(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var shopKey, out var error, allowBlank: false, "string shopKey"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			switch (shopKey.ToLower())
			{
			case "pierre":
				Game1.game1.parseDebugInput("Warp SeedShop 4 19", log);
				Game1.game1.parseDebugInput("WarpCharacterTo Pierre SeedShop 4 17", log);
				break;
			case "robin":
				Game1.game1.parseDebugInput("Warp ScienceHouse 8 20", log);
				Game1.game1.parseDebugInput("WarpCharacterTo Robin ScienceHouse 8 18", log);
				break;
			case "krobus":
				Game1.game1.parseDebugInput("Warp Sewer 31 19", log);
				break;
			case "sandy":
				Game1.game1.parseDebugInput("Warp SandyHouse 2 7", log);
				Game1.game1.parseDebugInput("WarpCharacterTo Sandy SandyHouse 2 5", log);
				break;
			case "marnie":
				Game1.game1.parseDebugInput("Warp AnimalShop 12 16", log);
				Game1.game1.parseDebugInput("WarpCharacterTo Marnie AnimalShop 12 14", log);
				break;
			case "clint":
				Game1.game1.parseDebugInput("Warp Blacksmith 3 15", log);
				Game1.game1.parseDebugInput("WarpCharacterTo Clint Blacksmith 3 13", log);
				break;
			case "gus":
				Game1.game1.parseDebugInput("Warp Saloon 10 20", log);
				Game1.game1.parseDebugInput("WarpCharacterTo Gus Saloon 10 18", log);
				break;
			case "willy":
				Game1.game1.parseDebugInput("Warp FishShop 6 6", log);
				Game1.game1.parseDebugInput("WarpCharacterTo Willy FishShop 6 4", log);
				break;
			case "pam":
				Game1.game1.parseDebugInput("Warp BusStop 7 12", log);
				Game1.game1.parseDebugInput("WarpCharacterTo Pam BusStop 11 10", log);
				break;
			case "dwarf":
				Game1.game1.parseDebugInput("Warp Mine 43 7", log);
				break;
			case "wizard":
				Game1.player.eventsSeen.Add("418172");
				Game1.player.hasMagicInk = true;
				Game1.game1.parseDebugInput("Warp WizardHouse 2 14", log);
				break;
			default:
				log.Error("That npc doesn't have a shop or it isn't handled by this command");
				break;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FacePlayer(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			NPC npc = Utility.fuzzyCharacterSearch(npcName);
			if (npc == null)
			{
				log.Error("Can't find NPC '" + npcName + "'.");
			}
			else
			{
				npc.faceTowardFarmer = true;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Refuel(string[] command, IGameLogger log)
		{
			if (Game1.player.getToolFromName("Lantern") is Lantern lantern)
			{
				lantern.fuelLeft = 100;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Lantern(string[] command, IGameLogger log)
		{
			Game1.player.Items.Add(ItemRegistry.Create("(T)Lantern"));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void GrowGrass(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var iterations, out var error, "int iterations"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			Game1.currentLocation.spawnWeeds(weedsOnly: false);
			Game1.currentLocation.growWeedGrass(iterations);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void AddAllCrafting(string[] command, IGameLogger log)
		{
			foreach (string s in CraftingRecipe.craftingRecipes.Keys)
			{
				Game1.player.craftingRecipes.Add(s, 0);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Animal(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var animalName, out var error, ' ', "string animalName"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Utility.addAnimalToFarm(new FarmAnimal(animalName.Trim(), Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MoveBuilding(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetVector2(command, 1, out var fromTile, out var error, integerOnly: true, "Vector2 fromTile") || !ArgUtility.TryGetPoint(command, 3, out var toTile, out error, "Point toTile"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			GameLocation location = Game1.currentLocation;
			if (location != null)
			{
				Building building = location.getBuildingAt(fromTile);
				if (building != null)
				{
					building.tileX.Value = toTile.X;
					building.tileY.Value = toTile.Y;
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Fishing(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var level, out var error, "int level"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.fishingLevel.Value = level;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "fd", "face" })]
		public static void FaceDirection(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var targetName, out var error, allowBlank: false, "string targetName") || !ArgUtility.TryGetInt(command, 2, out var facingDirection, out error, "int facingDirection"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else if (targetName == "farmer")
			{
				Game1.player.Halt();
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.player.faceDirection(facingDirection);
			}
			else
			{
				Utility.fuzzyCharacterSearch(targetName).faceDirection(facingDirection);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Note(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var noteId, out var error, "int noteId"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			if (!Game1.player.archaeologyFound.TryGetValue("102", out var data))
			{
				data = (Game1.player.archaeologyFound["102"] = new int[2]);
			}
			data[0] = 18;
			Game1.netWorldState.Value.LostBooksFound = 18;
			Game1.currentLocation.readNote(noteId);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void NetHost(string[] command, IGameLogger log)
		{
			Game1.multiplayer.StartServer();
		}

		/// <summary>Connect to a specified IP address.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void NetJoin(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var address, out var error, allowBlank: false, "string address"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			FarmhandMenu farmhandMenu = new FarmhandMenu(Game1.multiplayer.InitClient(new LidgrenClient(address)));
			if (Game1.activeClickableMenu is TitleMenu)
			{
				TitleMenu.subMenu = farmhandMenu;
				return;
			}
			Game1.ExitToTitle(delegate
			{
				(Game1.activeClickableMenu as TitleMenu).skipToTitleButtons();
				TitleMenu.subMenu = farmhandMenu;
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ToggleNetCompression(string[] command, IGameLogger log)
		{
			if (Program.defaultCompression.GetType() == typeof(NullNetCompression))
			{
				log.Error("This command can only be used on platforms that support compression.");
				return;
			}
			if (Game1.activeClickableMenu is TitleMenu)
			{
				ToggleCompression();
				return;
			}
			Game1.ExitToTitle(delegate
			{
				(Game1.activeClickableMenu as TitleMenu).skipToTitleButtons();
				ToggleCompression();
			});
			void ToggleCompression()
			{
				bool shouldCompress = Program.netCompression.GetType() == typeof(NullNetCompression);
				INetCompression netCompression2;
				if (!shouldCompress)
				{
					INetCompression netCompression = new NullNetCompression();
					netCompression2 = netCompression;
				}
				else
				{
					netCompression2 = Program.defaultCompression;
				}
				Program.netCompression = netCompression2;
				log.Info((shouldCompress ? "Enabled" : "Disabled") + " net compression.");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void LevelUp(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var skill, out var error, "int skill") || !ArgUtility.TryGetInt(command, 2, out var level, out error, "int level"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.activeClickableMenu = new LevelUpMenu(skill, level);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Darts(string[] command, IGameLogger log)
		{
			Game1.currentMinigame = new Darts();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MineGame(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptional(command, 1, out var mode, out var error, null, allowBlank: false, "string mode"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			int gameMode = ((mode == "infinite") ? 2 : 3);
			Game1.currentMinigame = new MineCart(0, gameMode);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Crane(string[] command, IGameLogger log)
		{
			Game1.currentMinigame = new CraneGame();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "trlt" })]
		public static void TailorRecipeListTool(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new TailorRecipeListTool();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "apt" })]
		public static void AnimationPreviewTool(string[] command, IGameLogger log)
		{
			Game1.activeClickableMenu = new AnimationPreviewTool();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void CreateDino(string[] command, IGameLogger log)
		{
			Game1.currentLocation.characters.Add(new DinoMonster(Game1.player.position.Value + new Vector2(100f, 0f)));
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "pta" })]
		public static void PerformTitleAction(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var titleAction, out var error, allowBlank: false, "string titleAction"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			if (Game1.activeClickableMenu is TitleMenu titleMenu)
			{
				titleMenu.performButtonAction(titleAction);
				return;
			}
			Game1.ExitToTitle(delegate
			{
				if (Game1.activeClickableMenu is TitleMenu titleMenu2)
				{
					titleMenu2.skipToTitleButtons();
					titleMenu2.performButtonAction(titleAction);
				}
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Action(string[] command, IGameLogger log)
		{
			Exception ex;
			if (!ArgUtility.TryGetRemainder(command, 1, out var action, out var error, ' ', "string action"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else if (TriggerActionManager.TryRunAction(action, out error, out ex))
			{
				log.Info("Applied action '" + action + "'.");
			}
			else
			{
				log.Error("Couldn't apply action '" + action + "': " + error, ex);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void BroadcastMail(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var mailId, out var error, ' ', "string mailId"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.addMailForTomorrow(mailId, noLetter: false, sendToEveryone: true);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Phone(string[] command, IGameLogger log)
		{
			Game1.game1.ShowTelephoneMenu();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Renovate(string[] command, IGameLogger log)
		{
			HouseRenovation.ShowRenovationMenu();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Crib(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetInt(command, 1, out var style, out var error, "int style"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else if (Game1.getLocationFromName(Game1.player.homeLocation.Value) is FarmHouse house)
			{
				house.cribStyle.Value = style;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void TestNut(string[] command, IGameLogger log)
		{
			Game1.createItemDebris(ItemRegistry.Create("(O)73"), Vector2.Zero, 2);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ShuffleBundles(string[] command, IGameLogger log)
		{
			Game1.GenerateBundles(Game1.BundleType.Remixed, use_seed: false);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Split(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalInt(command, 1, out var playerIndex, out var error, -1, "int playerIndex"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else if (playerIndex > -1)
			{
				GameRunner.instance.AddGameInstance((PlayerIndex)playerIndex);
			}
			else
			{
				Game1.game1.ShowLocalCoopJoinMenu();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "bsm" })]
		public static void SkinBuilding(string[] command, IGameLogger log)
		{
			Building building = Game1.currentLocation?.getBuildingAt(Game1.player.Tile + new Vector2(0f, -1f));
			if (building != null)
			{
				if (building.CanBeReskinned())
				{
					Game1.activeClickableMenu = new BuildingSkinMenu(building);
				}
				else
				{
					log.Error("The '" + building.buildingType.Value + "' building in front of the player can't be skinned.");
				}
			}
			else
			{
				log.Error("No building found in front of player.");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "bpm" })]
		public static void PaintBuilding(string[] command, IGameLogger log)
		{
			Building building = Game1.currentLocation?.getBuildingAt(Game1.player.Tile + new Vector2(0f, -1f));
			if (building != null)
			{
				if (building.CanBePainted())
				{
					Game1.activeClickableMenu = new BuildingPaintMenu(building);
					return;
				}
				log.Error("The '" + building.buildingType.Value + "' building in front of the player can't be painted. Defaulting to main farmhouse.");
			}
			Building farmhouse = Game1.getFarm().GetMainFarmHouse();
			if (farmhouse == null)
			{
				log.Error("The main farmhouse wasn't found.");
			}
			else if (!farmhouse.CanBePainted())
			{
				log.Error("The main farmhouse can't be painted.");
			}
			else
			{
				Game1.activeClickableMenu = new BuildingPaintMenu(farmhouse);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "md" })]
		public static void MineDifficulty(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalInt(command, 1, out var difficulty, out var error, -1, "int difficulty"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			if (difficulty > -1)
			{
				Game1.netWorldState.Value.MinesDifficulty = difficulty;
			}
			log.Info($"Mine difficulty: {Game1.netWorldState.Value.MinesDifficulty}");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "scd" })]
		public static void SkullCaveDifficulty(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalInt(command, 1, out var difficulty, out var error, -1, "int difficulty"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			if (difficulty > -1)
			{
				Game1.netWorldState.Value.SkullCavesDifficulty = difficulty;
			}
			log.Info($"Skull Cave difficulty: {Game1.netWorldState.Value.SkullCavesDifficulty}");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "tls" })]
		public static void ToggleLightingScale(string[] command, IGameLogger log)
		{
			Game1.game1.useUnscaledLighting = !Game1.game1.useUnscaledLighting;
			log.Info($"Toggled Lighting Scale: useUnscaledLighting: {Game1.game1.useUnscaledLighting}");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void FixWeapons(string[] command, IGameLogger log)
		{
			SaveMigrator_1_5.ResetForges();
			log.Info("Reset forged weapon attributes.");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "plsf" })]
		public static void PrintLatestSaveFix(string[] command, IGameLogger log)
		{
			SaveFixes latestFix = SaveFixes.FixDuplicateMissedMail;
			log.Info($"The latest save fix is '{latestFix.ToString()}' (ID: {latestFix})");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "pdb" })]
		public static void PrintGemBirds(string[] command, IGameLogger log)
		{
			log.Info($"Gem birds: North {IslandGemBird.GetBirdTypeForLocation("IslandNorth")} South {IslandGemBird.GetBirdTypeForLocation("IslandSouth")} East {IslandGemBird.GetBirdTypeForLocation("IslandEast")} West {IslandGemBird.GetBirdTypeForLocation("IslandWest")}");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "ppp" })]
		public static void PrintPlayerPos(string[] command, IGameLogger log)
		{
			log.Info($"Player tile position is {Game1.player.Tile} (World position: {Game1.player.Position})");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ShowPlurals(string[] command, IGameLogger log)
		{
			List<string> item_names = new List<string>();
			foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				item_names.Add(data.InternalName);
			}
			foreach (ParsedItemData data2 in ItemRegistry.RequireTypeDefinition("(BC)").GetAllData())
			{
				item_names.Add(data2.InternalName);
			}
			item_names.Sort();
			foreach (string item_name in item_names)
			{
				log.Info(Lexicon.makePlural(item_name));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void HoldItem(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalBool(command, 1, out var showMessage, out var error, defaultValue: false, "bool showMessage"))
			{
				DebugCommands.LogArgError(log, command, error);
			}
			else
			{
				Game1.player.holdUpItemThenMessage(Game1.player.CurrentItem, showMessage);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "rm" })]
		public static void RunMacro(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptional(command, 1, out var fileName, out var error, "macro.txt", allowBlank: false, "string fileName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			if (Game1.isRunningMacro)
			{
				log.Error("You cannot run a macro from within a macro.");
				return;
			}
			Game1.isRunningMacro = true;
			try
			{
				StreamReader file = new StreamReader(fileName);
				string line;
				while ((line = file.ReadLine()) != null)
				{
					Game1.chatBox.textBoxEnter(line);
				}
				log.Info("Executed macro file " + fileName);
				file.Close();
			}
			catch (Exception exception)
			{
				log.Error("Error running macro file " + fileName + ".", exception);
			}
			Game1.isRunningMacro = false;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void InviteMovie(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var npcName, out var error, allowBlank: false, "string npcName"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			NPC npc = Utility.fuzzyCharacterSearch(npcName);
			if (npc == null)
			{
				log.Error("Invalid NPC");
			}
			else
			{
				MovieTheater.Invite(Game1.player, npc);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Monster(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGet(command, 1, out var typeName, out var error, allowBlank: false, "string typeName") || !ArgUtility.TryGetPoint(command, 2, out var tile, out error, "Point tile") || !ArgUtility.TryGetOptionalRemainder(command, 4, out var monsterNameOrNumber))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			string fullTypeName = "StardewValley.Monsters." + typeName;
			Type monsterType = Type.GetType(fullTypeName);
			if ((object)monsterType == null)
			{
				log.Error("There's no monster with type '" + fullTypeName + "'.");
				return;
			}
			Vector2 pos = new Vector2(tile.X * 64, tile.Y * 64);
			int numberArg;
			object[] args = (string.IsNullOrWhiteSpace(monsterNameOrNumber) ? new object[1] { pos } : ((!int.TryParse(monsterNameOrNumber, out numberArg)) ? new object[2] { pos, monsterNameOrNumber } : new object[2] { pos, numberArg }));
			Monster mon = Activator.CreateInstance(monsterType, args) as Monster;
			Game1.currentLocation.characters.Add(mon);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "shaft" })]
		public static void Ladder(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalInt(command, 1, out var tileX, out var error, Game1.player.TilePoint.X, "int tileX") || !ArgUtility.TryGetOptionalInt(command, 2, out var tileY, out error, Game1.player.TilePoint.Y + 1, "int tileY"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			bool forceShaft = command[0].EqualsIgnoreCase("shaft");
			Game1.mine.createLadderDown(tileX, tileY, forceShaft);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void NetLog(string[] command, IGameLogger log)
		{
			Game1.multiplayer.logging.IsLogging = !Game1.multiplayer.logging.IsLogging;
			log.Info("Turned " + (Game1.multiplayer.logging.IsLogging ? "on" : "off") + " network write logging");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void NetClear(string[] command, IGameLogger log)
		{
			Game1.multiplayer.logging.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void NetDump(string[] command, IGameLogger log)
		{
			log.Info("Wrote log to " + Game1.multiplayer.logging.Dump());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "tto" })]
		public static void ToggleTimingOverlay(string[] command, IGameLogger log)
		{
			if ((!(Game1.game1?.IsMainInstance)) ?? true)
			{
				log.Error("Cannot toggle timing overlay as a splitscreen instance.");
				return;
			}
			bool active = Game1.debugTimings.Toggle();
			log.Info((active ? "Enabled" : "Disabled") + " in-game timing overlay.");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void LogBandwidth(string[] command, IGameLogger log)
		{
			if (Game1.IsServer)
			{
				Game1.server.LogBandwidth = !Game1.server.LogBandwidth;
				log.Info("Turned " + (Game1.server.LogBandwidth ? "on" : "off") + " server bandwidth logging");
			}
			else if (Game1.IsClient)
			{
				Game1.client.LogBandwidth = !Game1.client.LogBandwidth;
				log.Info("Turned " + (Game1.client.LogBandwidth ? "on" : "off") + " client bandwidth logging");
			}
			else
			{
				log.Error("Cannot toggle bandwidth logging in non-multiplayer games");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void LogWallAndFloorWarnings(string[] command, IGameLogger log)
		{
			DecoratableLocation.LogTroubleshootingInfo = !DecoratableLocation.LogTroubleshootingInfo;
			log.Info((DecoratableLocation.LogTroubleshootingInfo ? "Enabled" : "Disabled") + " wall and floor warning logs.");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ChangeWallet(string[] command, IGameLogger log)
		{
			if (Game1.IsMasterGame)
			{
				Game1.player.changeWalletTypeTonight.Value = true;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void SeparateWallets(string[] command, IGameLogger log)
		{
			if (Game1.IsMasterGame)
			{
				ManorHouse.SeparateWallets();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void MergeWallets(string[] command, IGameLogger log)
		{
			if (Game1.IsMasterGame)
			{
				ManorHouse.MergeWallets();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "nd", "newDay", "s" })]
		public static void Sleep(string[] command, IGameLogger log)
		{
			Game1.player.isInBed.Value = true;
			Game1.player.sleptInTemporaryBed.Value = true;
			Game1.currentLocation.answerDialogueAction("Sleep_Yes", null);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "gm", "inv" })]
		public static void Invincible(string[] command, IGameLogger log)
		{
			if (Game1.player.temporarilyInvincible)
			{
				Game1.player.temporaryInvincibilityTimer = 0;
				Game1.playSound("bigDeSelect");
			}
			else
			{
				Game1.player.temporarilyInvincible = true;
				Game1.player.temporaryInvincibilityTimer = -1000000000;
				Game1.playSound("bigSelect");
			}
		}

		/// <summary>Toggle whether multiplayer sync fields should run detailed validation to detect possible bugs. See remarks on <see cref="F:Netcode.NetFields.ShouldValidateNetFields" />.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ValidateNetFields(string[] command, IGameLogger log)
		{
			NetFields.ShouldValidateNetFields = !NetFields.ShouldValidateNetFields;
			log.Info(NetFields.ShouldValidateNetFields ? "Enabled net field validation, which may impact performance. This only affects new net fields created after it's enabled." : "Disabled net field validation.");
		}

		/// <summary>Filter the saves shown in the current load or co-op menu based on a search term.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		[OtherNames(new string[] { "flm" })]
		public static void FilterLoadMenu(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetRemainder(command, 1, out var filter, out var error, ' ', "string filter"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			if (Game1.activeClickableMenu is TitleMenu)
			{
				IClickableMenu subMenu = TitleMenu.subMenu;
				if (subMenu is CoopMenu coopMenu)
				{
					TitleMenu.subMenu = new CoopMenu(coopMenu.tooManyFarms, splitScreen: false, coopMenu.currentTab, filter);
					return;
				}
				if (!(subMenu is FarmhandMenu) && subMenu is LoadGameMenu)
				{
					TitleMenu.subMenu = new LoadGameMenu(filter);
					return;
				}
			}
			log.Error("The FilterLoadMenu debug command must be run while the list of saved games is open.");
		}

		/// <summary>Toggle the <see cref="F:StardewValley.Menus.MapPage.EnableDebugLines" /> option.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void WorldMapLines(string[] command, IGameLogger log)
		{
			MapPage.WorldMapDebugLineType types;
			if (command.Length > 1)
			{
				if (!Utility.TryParseEnum<MapPage.WorldMapDebugLineType>(string.Join(", ", command.Skip(1)), out types))
				{
					DebugCommands.LogArgError(log, command, "unknown type '" + string.Join(" ", command.Skip(1)) + "', expected space-delimited list of " + string.Join(", ", Enum.GetNames(typeof(MapPage.WorldMapDebugLineType))));
					return;
				}
			}
			else
			{
				types = ((MapPage.EnableDebugLines == MapPage.WorldMapDebugLineType.None) ? MapPage.WorldMapDebugLineType.All : MapPage.WorldMapDebugLineType.None);
			}
			MapPage.EnableDebugLines = types;
			log.Info((types == MapPage.WorldMapDebugLineType.None) ? "World map debug lines disabled." : $"World map debug lines enabled for types {types}.");
		}

		/// <summary>Print info about the player's current position for <c>Data/WorldMaps</c> data.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		/// <remarks>This is derived from <see cref="M:StardewValley.WorldMaps.WorldMapManager.GetPositionData(StardewValley.GameLocation,Microsoft.Xna.Framework.Point)" />.</remarks>
		public static void WorldMapPosition(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptionalBool(command, 1, out var includeLog, out var error, defaultValue: false, "bool includeLog"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			GameLocation location = Game1.currentLocation;
			Point tile = Game1.player.TilePoint;
			LogBuilder logBuilder = (includeLog ? new LogBuilder(3) : null);
			MapAreaPositionWithContext? rawPosition = WorldMapManager.GetPositionData(location, tile, logBuilder);
			StringBuilder result = new StringBuilder();
			if (!rawPosition.HasValue)
			{
				result.AppendLine("The player's current position didn't match any entry in Data/WorldMaps.");
			}
			else
			{
				MapAreaPositionWithContext position = rawPosition.Value;
				MapAreaPosition data = rawPosition.Value.Data;
				StringBuilder stringBuilder = result;
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(33, 3, stringBuilder);
				handler.AppendLiteral("The player is currently at ");
				handler.AppendFormatted(location.NameOrUniqueName);
				handler.AppendLiteral(" (");
				handler.AppendFormatted(tile.X);
				handler.AppendLiteral(", ");
				handler.AppendFormatted(tile.Y);
				handler.AppendLiteral(").");
				stringBuilder2.AppendLine(ref handler);
				if (location.NameOrUniqueName != position.Location.NameOrUniqueName || tile != position.Tile)
				{
					stringBuilder = result;
					StringBuilder stringBuilder3 = stringBuilder;
					handler = new StringBuilder.AppendInterpolatedStringHandler(31, 3, stringBuilder);
					handler.AppendLiteral("That was translated to '");
					handler.AppendFormatted(position.Location.NameOrUniqueName);
					handler.AppendLiteral("' (");
					handler.AppendFormatted(position.Tile.X);
					handler.AppendLiteral(", ");
					handler.AppendFormatted(position.Tile.Y);
					handler.AppendLiteral(").");
					stringBuilder3.AppendLine(ref handler);
				}
				stringBuilder = result;
				StringBuilder stringBuilder4 = stringBuilder;
				handler = new StringBuilder.AppendInterpolatedStringHandler(53, 3, stringBuilder);
				handler.AppendLiteral("This matches region '");
				handler.AppendFormatted(data.Region.Id);
				handler.AppendLiteral("', area '");
				handler.AppendFormatted(data.Area.Id);
				handler.AppendLiteral("', and map position '");
				handler.AppendFormatted(data.Data.Id);
				handler.AppendLiteral("'.");
				stringBuilder4.AppendLine(ref handler);
				stringBuilder = result;
				StringBuilder stringBuilder5 = stringBuilder;
				handler = new StringBuilder.AppendInterpolatedStringHandler(79, 3, stringBuilder);
				handler.AppendLiteral("The position's pixel area is ");
				handler.AppendFormatted(data.GetPixelArea());
				handler.AppendLiteral(", with the player at position ");
				handler.AppendFormatted(position.GetMapPixelPosition());
				handler.AppendLiteral(" (position ratio: ");
				handler.AppendFormatted(position.GetPositionRatioIfValid());
				handler.AppendLiteral(").");
				stringBuilder5.AppendLine(ref handler);
				stringBuilder = result;
				StringBuilder stringBuilder6 = stringBuilder;
				handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder);
				handler.AppendLiteral("Scroll text: ");
				handler.AppendFormatted(position.GetScrollText() ?? "none");
				handler.AppendLiteral(".");
				stringBuilder6.AppendLine(ref handler);
			}
			result.AppendLine();
			result.AppendLine("Log:");
			if (logBuilder != null)
			{
				result.Append(logBuilder.Log);
			}
			else
			{
				result.AppendLine("   Run `debug WorldMapPosition true` to show the detailed log.");
			}
			log.Info(result.ToString());
		}

		/// <summary>List debug commands in the game.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void Search(string[] command, IGameLogger log)
		{
			if (!ArgUtility.TryGetOptional(command, 1, out var search, out var error, null, allowBlank: false, "string search"))
			{
				DebugCommands.LogArgError(log, command, error);
				return;
			}
			List<string> commands = DebugCommands.SearchCommandNames(search);
			if (commands.Count == 0)
			{
				log.Info("No debug commands found matching '" + search + "'.");
				return;
			}
			log.Info(((search != null) ? $"Found {commands.Count} debug commands matching search term '{search}':\n" : $"{commands.Count} debug commands registered:\n") + "  - " + string.Join("\n  - ", commands) + ((search == null) ? "\n\nTip: you can search debug commands like 'debug Search searchTermHere'." : ""));
		}

		/// <summary>Add artifact spots in every available spot in a 9x9 grid around the player.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ArtifactSpots(string[] command, IGameLogger log)
		{
			GameLocation location = Game1.player.currentLocation;
			Vector2 playerTile = Game1.player.Tile;
			if (location == null)
			{
				log.Info("You must be in a location to use this command.");
				return;
			}
			int spawned = 0;
			Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(playerTile);
			foreach (Vector2 tile in surroundingTileLocationsArray)
			{
				if (location.terrainFeatures.TryGetValue(tile, out var feature) && feature is HoeDirt { crop: null })
				{
					location.terrainFeatures.Remove(tile);
				}
				if (location.isTilePassable(tile) && !location.IsTileOccupiedBy(tile, ~(CollisionMask.Characters | CollisionMask.Farmers | CollisionMask.TerrainFeatures)))
				{
					location.objects.Add(tile, ItemRegistry.Create<Object>("(O)590"));
					spawned++;
				}
			}
			if (spawned == 0)
			{
				log.Info("No unoccupied tiles found around the player.");
				return;
			}
			log.Info($"Spawned {spawned} artifact spots around the player.");
		}

		/// <summary>Enable or disable writing messages to the debug log file.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void LogFile(string[] command, IGameLogger log)
		{
			if (Game1.log is DefaultLogger logger)
			{
				Game1.log = new DefaultLogger(logger.ShouldWriteToConsole, !logger.ShouldWriteToLogFile);
				log.Info((logger.ShouldWriteToLogFile ? "Disabled" : "Enabled") + " the game log file at " + Program.GetDebugLogPath() + ".");
			}
			else if (Game1.log?.GetType().FullName?.StartsWith("StardewModdingAPI.") ?? false)
			{
				log.Error("The debug log can't be enabled when SMAPI is installed. SMAPI already includes log messages in its own log file.");
			}
			else
			{
				log.Error("The debug log can't be enabled: the game logger has been replaced with unknown implementation '" + Game1.log?.GetType()?.FullName + "'.");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void ToggleCheats(string[] command, IGameLogger log)
		{
			Program.enableCheats = !Program.enableCheats;
			log.Info((Program.enableCheats ? "Enabled" : "Disabled") + " in-game cheats.");
		}
	}

	/// <summary>The supported commands and their resolvers.</summary>
	private static readonly Dictionary<string, DebugCommandHandlerDelegate> Handlers;

	/// <summary>Alternate names for debug commands (e.g. shorthand or acronyms).</summary>
	private static readonly Dictionary<string, string> Aliases;

	/// <summary>Register the default debug commands, defined as <see cref="T:StardewValley.DebugCommands.DefaultHandlers" /> methods.</summary>
	static DebugCommands()
	{
		DebugCommands.Handlers = new Dictionary<string, DebugCommandHandlerDelegate>(StringComparer.OrdinalIgnoreCase);
		DebugCommands.Aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		MethodInfo[] methods = typeof(DefaultHandlers).GetMethods(BindingFlags.Static | BindingFlags.Public);
		MethodInfo[] array = methods;
		foreach (MethodInfo method in array)
		{
			try
			{
				DebugCommands.Handlers[method.Name] = (DebugCommandHandlerDelegate)Delegate.CreateDelegate(typeof(DebugCommandHandlerDelegate), method);
			}
			catch (Exception exception)
			{
				Game1.log.Error("Failed to initialize debug command " + method.Name + ".", exception);
			}
		}
		array = methods;
		foreach (MethodInfo method2 in array)
		{
			OtherNamesAttribute attribute = method2.GetCustomAttribute<OtherNamesAttribute>();
			if (attribute == null)
			{
				continue;
			}
			string[] aliases = attribute.Aliases;
			foreach (string alias in aliases)
			{
				if (DebugCommands.Handlers.ContainsKey(alias))
				{
					Game1.log.Error($"Can't register alias '{alias}' for debug command '{method2.Name}', because there's a command with that name.");
				}
				if (DebugCommands.Aliases.TryGetValue(alias, out var conflictingName))
				{
					Game1.log.Error($"Can't register alias '{alias}' for debug command '{method2.Name}', because that's already an alias for '{conflictingName}'.");
				}
				DebugCommands.Aliases[alias] = method2.Name;
			}
		}
	}

	/// <summary>Try to handle a debug command.</summary>
	/// <param name="command">The full debug command split by spaces, including the command name and arguments.</param>
	/// <param name="log">The log to which to write command output, or <c>null</c> to use <see cref="F:StardewValley.Game1.log" />.</param>
	/// <returns>Returns whether the command was found and executed, regardless of whether the command logic succeeded.</returns>
	public static bool TryHandle(string[] command, IGameLogger log = null)
	{
		if (log == null)
		{
			log = Game1.log;
		}
		string commandName = ArgUtility.Get(command, 0);
		if (string.IsNullOrWhiteSpace(commandName))
		{
			log.Error("Can't parse an empty command.");
			return false;
		}
		if (DebugCommands.Aliases.TryGetValue(commandName, out var aliasTarget))
		{
			commandName = aliasTarget;
		}
		if (!DebugCommands.Handlers.TryGetValue(commandName, out var handler))
		{
			log.Error("Unknown debug command '" + commandName + "'.");
			string[] similar = DebugCommands.SearchCommandNames(commandName).Take(10).ToArray();
			if (similar.Length != 0)
			{
				log.Info("Did you mean one of these?\n- " + string.Join("\n- ", similar));
			}
			return false;
		}
		try
		{
			handler(command, log);
			return true;
		}
		catch (Exception exception)
		{
			log.Error("Error running debug command '" + string.Join(" ", command) + "'.", exception);
			return false;
		}
	}

	/// <summary>Get the list of commands which match the given search text.</summary>
	/// <param name="search">The text to match in command names, or <c>null</c> to list all command names.</param>
	/// <param name="displayAliases">Whether to append aliases in the results, like <c>"houseUpgrade (house, hu)"</c>.</param>
	public static List<string> SearchCommandNames(string search, bool displayAliases = true)
	{
		ILookup<string, string> aliasesByName = DebugCommands.Aliases.ToLookup((KeyValuePair<string, string> p) => p.Value, (KeyValuePair<string, string> p) => p.Key);
		List<string> commands = new List<string>();
		foreach (string name in DebugCommands.Handlers.Keys.OrderBy<string, string>((string p) => p, StringComparer.OrdinalIgnoreCase))
		{
			string[] aliases = aliasesByName[name].ToArray();
			if (aliases.Length == 0)
			{
				commands.Add(name);
			}
			else if (displayAliases)
			{
				commands.Add(name + " (" + string.Join(", ", aliases.OrderBy<string, string>((string p) => p, StringComparer.OrdinalIgnoreCase)) + ")");
			}
			else
			{
				commands.Add("###" + name + "###" + string.Join(",", aliases));
			}
		}
		if (search != null)
		{
			commands.RemoveAll((string line) => !Utility.fuzzyCompare(search, line).HasValue);
		}
		if (!displayAliases)
		{
			for (int i = 0; i < commands.Count; i++)
			{
				if (commands[i].StartsWith("###"))
				{
					commands[i] = commands[i].Split("###", 3)[1];
				}
			}
		}
		return commands;
	}

	/// <summary>Log an error indicating a command's arguments are invalid.</summary>
	/// <param name="log">The log to which to write debug command output.</param>
	/// <param name="command">The full debug command split by spaces, including the command name.</param>
	/// <param name="error">The error phrase to log.</param>
	private static void LogArgError(IGameLogger log, string[] command, string error)
	{
		string rawCommandName = ArgUtility.Get(command, 0);
		string commandLabel = rawCommandName;
		if (!string.IsNullOrWhiteSpace(rawCommandName))
		{
			if (!DebugCommands.Aliases.TryGetValue(rawCommandName, out var actualCommandName))
			{
				foreach (string handlerName in DebugCommands.Handlers.Keys)
				{
					if (rawCommandName.EqualsIgnoreCase(handlerName))
					{
						actualCommandName = handlerName;
						break;
					}
				}
			}
			commandLabel = actualCommandName ?? rawCommandName;
			if (!commandLabel.EqualsIgnoreCase(rawCommandName))
			{
				commandLabel = rawCommandName + " (" + commandLabel + ")";
			}
		}
		log.Error($"Failed parsing {commandLabel} command: {error}.");
	}
}
