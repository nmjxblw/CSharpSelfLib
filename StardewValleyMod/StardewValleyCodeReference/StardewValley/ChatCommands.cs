using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using StardewValley.Delegates;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;

namespace StardewValley;

/// <summary>The commands that can be executed through the in-game chat box.</summary>
/// <remarks>See also <see cref="T:StardewValley.DebugCommands" />.</remarks>
/// <summary>The commands that can be executed through the in-game chat box.</summary>
/// <remarks>See also <see cref="T:StardewValley.DebugCommands" />.</remarks>
public static class ChatCommands
{
	/// <summary>A chat command which can be invoked through the chat box.</summary>
	public class ChatCommand
	{
		/// <summary>The main chat command name, like <c>help</c>.</summary>
		public readonly string Name;

		/// <summary>The method which processes a command from the chat box.</summary>
		public readonly ChatCommandHandlerDelegate Handler;

		/// <summary>&gt;The text to show for this command when the player enters <c>/help</c>. This can be or return null to hide it from the help output. This receives the registered command name.</summary>
		public readonly Func<string, string> HelpDescription;

		/// <summary>Whether this command can only be used by the main player.</summary>
		public readonly bool IsMainPlayerOnly;

		/// <summary>Whether this command can only be used in multiplayer mode.</summary>
		public readonly bool IsMultiplayerOnly;

		/// <summary>Whether the command can only be used when cheats are enabled.</summary>
		public readonly bool IsCheatsOnly;

		/// <summary>Construct an instance.</summary>
		/// <param name="name">The main chat command name, like <c>help</c>.</param>
		/// <param name="handler">The method which processes a command from the chat box.</param>
		/// <param name="helpDescription">The text to show for this command when the player enters <c>/help</c>. This can be or return null to hide it from the help output.</param>
		/// <param name="isMainPlayerOnly">Whether this command can only be used by the main player.</param>
		/// <param name="isMultiplayerOnly">Whether this command can only be used in multiplayer mode.</param>
		/// <param name="isCheatsOnly">Whether the command can only be used when cheats are enabled.</param>
		public ChatCommand(string name, Func<string, string> helpDescription, ChatCommandHandlerDelegate handler, bool isMainPlayerOnly, bool isMultiplayerOnly, bool isCheatsOnly)
		{
			this.Name = name;
			this.HelpDescription = helpDescription;
			this.Handler = handler;
			this.IsMainPlayerOnly = isMainPlayerOnly;
			this.IsMultiplayerOnly = isMultiplayerOnly;
			this.IsCheatsOnly = isCheatsOnly;
		}

		/// <summary>Get whether this chat command can be used by the current player.</summary>
		public bool IsVisible()
		{
			if (this.IsMainPlayerOnly && !Game1.IsMasterGame)
			{
				return false;
			}
			if (this.IsMultiplayerOnly && !Game1.IsServer && !Game1.IsMultiplayer)
			{
				return false;
			}
			if (this.IsCheatsOnly && !ChatCommands.AllowCheats)
			{
				return false;
			}
			return true;
		}
	}

	/// <summary>The low-level handlers for vanilla chat commands. Most code should call <see cref="M:StardewValley.ChatCommands.TryHandle(System.String[],StardewValley.Menus.ChatBox)" /> instead, which adds error-handling.</summary>
	public static class DefaultHandlers
	{
		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void Ban(string[] command, ChatBox chat)
		{
			int index = 0;
			Farmer farmer = chat.findMatchingFarmer(command, ref index, allowMatchingByUserName: true);
			if (farmer != null)
			{
				string userId = Game1.server.ban(farmer.UniqueMultiplayerID);
				if (userId == null || !Game1.bannedUsers.TryGetValue(userId, out var userName))
				{
					chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Ban_Failed"));
					return;
				}
				string userDisplay = ((userName != null) ? (userName + " (" + userId + ")") : userId);
				chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Ban_Done", userDisplay));
			}
			else
			{
				chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_NoSuchOnlinePlayer"));
				chat.listPlayers(otherPlayersOnly: true);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void Cheat(string[] command, ChatBox chat)
		{
			chat.addNiceTryEasterEggMessage();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void Clear(string[] command, ChatBox chat)
		{
			chat.messages.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void Color(string[] command, ChatBox chat)
		{
			if (command.Length > 1)
			{
				Game1.player.defaultChatColor = command[1];
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void ConcernedApe(string[] command, ChatBox chat)
		{
			if (Game1.player.mailReceived.Add("apeChat1"))
			{
				chat.addMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_ConcernedApe_1"), new Color(104, 214, 255));
			}
			else
			{
				chat.addMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_ConcernedApe_2"), Microsoft.Xna.Framework.Color.Yellow);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void ColorList(string[] command, ChatBox chat)
		{
			chat.addMessage("white, red, blue, green, jade, yellowgreen, pink, purple, yellow, orange, brown, gray, cream, salmon, peach, aqua, jungle, plum", Microsoft.Xna.Framework.Color.White);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void Debug(string[] command, ChatBox chat)
		{
			string commandText = ArgUtility.UnsplitQuoteAware(command, ' ', 1);
			if (string.IsNullOrWhiteSpace(commandText))
			{
				chat.addErrorMessage("invalid usage: requires a debug command to run");
			}
			else
			{
				chat.cheat(commandText, isDebug: true);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void Emote(string[] command, ChatBox chat)
		{
			if (!Game1.player.CanEmote())
			{
				return;
			}
			bool validEmote = false;
			if (command.Length > 1)
			{
				string emoteType = command[1].ToLowerInvariant();
				emoteType = emoteType.Substring(0, Math.Min(emoteType.Length, 16));
				for (int i = 0; i < Farmer.EMOTES.Length; i++)
				{
					if (emoteType == Farmer.EMOTES[i].emoteString)
					{
						validEmote = true;
						break;
					}
				}
				if (validEmote)
				{
					Game1.player.netDoEmote(emoteType);
				}
			}
			if (validEmote)
			{
				return;
			}
			string emoteList = "";
			for (int j = 0; j < Farmer.EMOTES.Length; j++)
			{
				if (!Farmer.EMOTES[j].hidden)
				{
					emoteList += Farmer.EMOTES[j].emoteString;
					if (j < Farmer.EMOTES.Length - 1)
					{
						emoteList += ", ";
					}
				}
			}
			chat.addMessage(emoteList, Microsoft.Xna.Framework.Color.White);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void Help(string[] command, ChatBox chat)
		{
			string searchCommandName = ArgUtility.Get(command, 1);
			if (searchCommandName != null)
			{
				if (ChatCommands.Handlers.TryGetValue(searchCommandName, out var handler))
				{
					string description = handler.HelpDescription?.Invoke(handler.Name);
					if (description != null)
					{
						chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Help_CommandDescription", description));
						return;
					}
				}
				chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Help_NoSuchCommand", searchCommandName));
			}
			List<string> commandNames = new List<string>();
			List<string> multiplayerCommandNames = new List<string>();
			foreach (ChatCommand handler2 in ChatCommands.Handlers.Values)
			{
				if (handler2.IsVisible() && handler2.HelpDescription?.Invoke(handler2.Name) != null)
				{
					if (handler2.IsMultiplayerOnly)
					{
						multiplayerCommandNames.Add(handler2.Name);
					}
					else
					{
						commandNames.Add(handler2.Name);
					}
				}
			}
			chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Help_Intro"));
			chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Help_CommandList", string.Join(", ", commandNames)));
			if (multiplayerCommandNames.Count > 0)
			{
				chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Help_MultiplayerCommandList", string.Join(", ", multiplayerCommandNames)));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void Kick(string[] command, ChatBox chat)
		{
			int index = 0;
			Farmer farmer = chat.findMatchingFarmer(command, ref index, allowMatchingByUserName: true);
			if (farmer != null)
			{
				Game1.server.kick(farmer.UniqueMultiplayerID);
				return;
			}
			chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_NoSuchOnlinePlayer"));
			chat.listPlayers(otherPlayersOnly: true);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void List(string[] command, ChatBox chat)
		{
			chat.listPlayers();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void MapScreenshot(string[] command, ChatBox chat)
		{
			if (Game1.game1.CanTakeScreenshots())
			{
				int scale = 25;
				string screenshotName = null;
				if (command.Length > 2 && !int.TryParse(command[2], out scale))
				{
					scale = 25;
				}
				if (command.Length > 1)
				{
					screenshotName = command[1];
				}
				if (scale <= 10)
				{
					scale = 10;
				}
				string result = Game1.game1.takeMapScreenshot((float)scale / 100f, screenshotName, null);
				if (result != null)
				{
					chat.addMessage("Wrote '" + result + "'.", Microsoft.Xna.Framework.Color.White);
				}
				else
				{
					chat.addMessage("Failed.", Microsoft.Xna.Framework.Color.Red);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void Message(string[] command, ChatBox chat)
		{
			chat.sendPrivateMessage(command);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void Money(string[] command, ChatBox chat)
		{
			ChatCommands.GetDebugPassThrough("Money")(command, chat);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void MoveBuildingPermission(string[] command, ChatBox chat)
		{
			if (command.Length <= 1)
			{
				chat.addMessage("off, owned, on", Microsoft.Xna.Framework.Color.White);
				return;
			}
			switch (command[1].ToLowerInvariant())
			{
			case "off":
				Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.Off;
				break;
			case "owned":
				Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.OwnedBuildings;
				break;
			case "on":
				Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.On;
				break;
			}
			chat.addMessage($"moveBuildingPermission {Game1.player.team.farmhandsCanMoveBuildings.Value}", Microsoft.Xna.Framework.Color.White);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		/// <remarks>See also <see cref="M:StardewValley.ChatCommands.DefaultHandlers.Resume(System.String[],StardewValley.Menus.ChatBox)" />.</remarks>
		public static void Pause(string[] command, ChatBox chat)
		{
			if (!Game1.IsMasterGame)
			{
				chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_HostOnly"));
				return;
			}
			Game1.netWorldState.Value.IsPaused = !Game1.netWorldState.Value.IsPaused;
			chat.globalInfoMessage(Game1.netWorldState.Value.IsPaused ? "Paused" : "Resumed");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void Ping(string[] command, ChatBox chat)
		{
			if (!Game1.IsMultiplayer)
			{
				return;
			}
			StringBuilder sb = new StringBuilder();
			if (Game1.IsServer)
			{
				foreach (KeyValuePair<long, Farmer> farmer in Game1.otherFarmers)
				{
					sb.Clear();
					sb.AppendFormat("Ping({0}) {1}ms ", farmer.Value.Name, (int)Game1.server.getPingToClient(farmer.Key));
					chat.addMessage(sb.ToString(), Microsoft.Xna.Framework.Color.White);
				}
				return;
			}
			sb.AppendFormat("Ping: {0}ms", (int)Game1.client.GetPingToHost());
			chat.addMessage(sb.ToString(), Microsoft.Xna.Framework.Color.White);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void PrintDiag(string[] command, ChatBox chat)
		{
			StringBuilder sb = new StringBuilder();
			Program.AppendDiagnostics(sb);
			chat.addInfoMessage(sb.ToString());
			Game1.log.Info(sb.ToString());
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void Qi(string[] command, ChatBox chat)
		{
			if (Game1.player.mailReceived.Add("QiChat1"))
			{
				chat.addMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Qi_1"), new Color(100, 50, 255));
			}
			else if (Game1.player.mailReceived.Add("QiChat2"))
			{
				chat.addMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Qi_2"), new Color(100, 50, 255));
				chat.addMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Qi_3"), Microsoft.Xna.Framework.Color.Yellow);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void RecountNuts(string[] command, ChatBox chat)
		{
			Game1.game1.RecountWalnuts();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void Reply(string[] command, ChatBox chat)
		{
			chat.replyPrivateMessage(command);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		/// <remarks>See also <see cref="M:StardewValley.ChatCommands.DefaultHandlers.Pause(System.String[],StardewValley.Menus.ChatBox)" />.</remarks>
		public static void Resume(string[] command, ChatBox chat)
		{
			if (!Game1.IsMasterGame)
			{
				chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_HostOnly"));
			}
			else if (Game1.netWorldState.Value.IsPaused)
			{
				Game1.netWorldState.Value.IsPaused = false;
				chat.globalInfoMessage("Resumed");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void SleepAnnounceMode(string[] command, ChatBox chat)
		{
			if (command.Length > 1)
			{
				switch (command[1].ToLowerInvariant())
				{
				case "all":
					Game1.player.team.sleepAnnounceMode.Value = FarmerTeam.SleepAnnounceModes.All;
					break;
				case "first":
					Game1.player.team.sleepAnnounceMode.Value = FarmerTeam.SleepAnnounceModes.First;
					break;
				case "off":
					Game1.player.team.sleepAnnounceMode.Value = FarmerTeam.SleepAnnounceModes.Off;
					break;
				}
				Game1.multiplayer.globalChatInfoMessage("SleepAnnounceModeSet", TokenStringBuilder.LocalizedText($"Strings\\UI:ChatCommands_SleepAnnounceMode_{Game1.player.team.sleepAnnounceMode.Value}"));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void Unban(string[] command, ChatBox chat)
		{
			if (Game1.bannedUsers.Count == 0)
			{
				chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Unban_NoPlayersBanned"));
				return;
			}
			bool listUnbannablePlayers = false;
			if (command.Length > 1)
			{
				string unbanId = command[1];
				string userId = null;
				if (Game1.bannedUsers.TryGetValue(unbanId, out var userName))
				{
					userId = unbanId;
				}
				else
				{
					foreach (KeyValuePair<string, string> bannedUser in Game1.bannedUsers)
					{
						if (bannedUser.Value == unbanId)
						{
							userId = bannedUser.Key;
							userName = bannedUser.Value;
							break;
						}
					}
				}
				if (userId != null)
				{
					string userDisplay = ((userName != null) ? (userName + " (" + userId + ")") : userId);
					chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Unban_Done", userDisplay));
					Game1.bannedUsers.Remove(userId);
				}
				else
				{
					listUnbannablePlayers = true;
					chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Unban_PlayerNotFound"));
				}
			}
			else
			{
				listUnbannablePlayers = true;
			}
			if (!listUnbannablePlayers)
			{
				return;
			}
			chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Unban_PlayerList"));
			foreach (KeyValuePair<string, string> bannedUser2 in Game1.bannedUsers)
			{
				string userDisplay2 = "- " + bannedUser2.Key;
				if (bannedUser2.Value != null)
				{
					userDisplay2 = $"- {bannedUser2.Value} ({bannedUser2.Key})";
				}
				chat.addInfoMessage(userDisplay2);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void UnbanAll(string[] command, ChatBox chat)
		{
			if (Game1.bannedUsers.Count == 0)
			{
				chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Unban_NoPlayersBanned"));
				return;
			}
			chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_UnbanAll_Done"));
			Game1.bannedUsers.Clear();
		}

		/// <summary>Unlink a farmer from its player, so it can be claimed by anyone.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.ChatCommandHandlerDelegate" />
		public static void UnlinkPlayer(string[] command, ChatBox chat)
		{
			int index = 0;
			Farmer farmer = chat.findMatchingFarmer(command, ref index, allowMatchingByUserName: true, onlineOnly: false);
			if (farmer != null)
			{
				farmer.userID.Value = string.Empty;
				Game1.log.Info($"Unlinked {(farmer.isActive() ? "active" : "inactive")} player {farmer.uniqueMultiplayerID} ('{farmer.Name}').");
			}
			else
			{
				chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_NoSuchPlayer"));
				chat.listPlayers(otherPlayersOnly: true, onlineOnly: false);
			}
		}
	}

	/// <summary>The supported commands and their handlers.</summary>
	private static readonly Dictionary<string, ChatCommand> Handlers;

	/// <summary>Alternate names for chat commands (e.g. shorthand or acronyms).</summary>
	private static readonly Dictionary<string, string> Aliases;

	/// <summary>Whether debug cheat commands are enabled.</summary>
	public static bool AllowCheats
	{
		get
		{
			if (!Program.enableCheats)
			{
				return Game1.player?.team?.allowChatCheats.Value == true;
			}
			return true;
		}
	}

	/// <summary>Register the default chat commands, defined as <see cref="T:StardewValley.ChatCommands.DefaultHandlers" /> methods.</summary>
	static ChatCommands()
	{
		ChatCommands.Handlers = new Dictionary<string, ChatCommand>(StringComparer.OrdinalIgnoreCase);
		ChatCommands.Aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		ChatCommands.Register("qi", DefaultHandlers.Qi, null);
		ChatCommands.Register("concernedApe", DefaultHandlers.ConcernedApe, null, new string[2] { "ape", "ca" });
		ChatCommands.Register("cheat", DefaultHandlers.Cheat, null, new string[5] { "showMeTheMoney", "imACheat", "cheats", "freeGold", "rosebud" });
		ChatCommands.Register("money", DefaultHandlers.Money, null, null, mainOnly: false, multiplayerOnly: false, cheatsOnly: true);
		ChatCommands.Register("help", DefaultHandlers.Help, null, new string[1] { "h" });
		ChatCommands.Register("clear", DefaultHandlers.Clear, (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Clear_Help", name));
		ChatCommands.Register("list", DefaultHandlers.List, (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_List_Help", name), new string[2] { "users", "players" });
		ChatCommands.Register("color", DefaultHandlers.Color, (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Color_Help", name));
		ChatCommands.Register("color-list", DefaultHandlers.ColorList, (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_ColorList_Help", name));
		ChatCommands.Register("emote", DefaultHandlers.Emote, (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Emote_Help", name), new string[1] { "e" });
		ChatCommands.Register("mapScreenshot", DefaultHandlers.MapScreenshot, (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_MapScreenshot_Help", name));
		ChatCommands.Register("pause", DefaultHandlers.Pause, (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Pause_Help", name));
		ChatCommands.Register("resume", DefaultHandlers.Resume, (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Resume_Help", name));
		ChatCommands.Register("message", DefaultHandlers.Message, (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Message_Help", name), new string[3] { "dm", "pm", "whisper" }, mainOnly: false, multiplayerOnly: true);
		ChatCommands.Register("reply", DefaultHandlers.Reply, (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Reply_Help", name), new string[1] { "r" }, mainOnly: false, multiplayerOnly: true);
		ChatCommands.Register("ping", DefaultHandlers.Ping, null, null, mainOnly: false, multiplayerOnly: true);
		ChatCommands.Register("kick", DefaultHandlers.Kick, (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Kick_Help", name), null, mainOnly: true, multiplayerOnly: true);
		ChatCommands.Register("ban", DefaultHandlers.Ban, (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Ban_Help", name), null, mainOnly: true, multiplayerOnly: true);
		ChatCommands.Register("unban", DefaultHandlers.Unban, (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Unban_Help", name), null, mainOnly: true, multiplayerOnly: true);
		ChatCommands.Register("unbanAll", DefaultHandlers.UnbanAll, (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_UnbanAll_Help", name), null, mainOnly: true, multiplayerOnly: true);
		ChatCommands.Register("moveBuildingPermission", DefaultHandlers.MoveBuildingPermission, null, new string[2] { "mbp", "movePermission" }, mainOnly: true, multiplayerOnly: true);
		ChatCommands.Register("sleepAnnounceMode", DefaultHandlers.SleepAnnounceMode, null, null, mainOnly: true, multiplayerOnly: true);
		ChatCommands.Register("unlinkPlayer", DefaultHandlers.UnlinkPlayer, (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_UnlinkPlayer_Help", name), null, mainOnly: true, multiplayerOnly: true);
		ChatCommands.Register("debug", DefaultHandlers.Debug, null, null, mainOnly: false, multiplayerOnly: false, cheatsOnly: true);
		ChatCommands.Register("logFile", ChatCommands.GetDebugPassThrough("LogFile"), null);
		ChatCommands.Register("printDiag", DefaultHandlers.PrintDiag, null);
		ChatCommands.Register("recountNuts", DefaultHandlers.RecountNuts, null);
		ChatCommands.Register("sdlVersion", ChatCommands.GetDebugPassThrough("SdlVersion"), null, new string[1] { "sdlv" });
	}

	/// <summary>Get whether a chat command exists.</summary>
	/// <param name="commandName">The chat command name, like <c>help</c>.</param>
	public static bool Exists(string commandName)
	{
		if (commandName == null)
		{
			return false;
		}
		if (!ChatCommands.Handlers.ContainsKey(commandName))
		{
			return ChatCommands.Aliases.ContainsKey(commandName);
		}
		return true;
	}

	/// <summary>Register a chat command handler.</summary>
	/// <param name="commandName">The chat command name, like <c>help</c>. This should only contain alphanumeric, underscore, dash, and dot characters. For custom chat command, this should be prefixed with your mod ID like <c>Example.ModId_Command</c>.</param>
	/// <param name="handler">The handler which processes the chat command entered by the player.</param>
	/// <param name="helpDescription">Get the text to show for this command when the player enter <c>/help</c>, or <c>null</c> to hide it from the help command. This receives the registered command name.</param>
	/// <param name="mainOnly">Whether the command can only be used by the main player.</param>
	/// <param name="multiplayerOnly">Whether the command can only be used in multiplayer mode.</param>
	/// <param name="cheatsOnly">Whether the command can only be used when cheats are enabled.</param>
	/// <param name="aliases">Alternate names for the command (e.g. shorthand or acronyms).</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="commandName" /> is null or whitespace-only.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="handler" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="commandName" /> is already registered.</exception>
	public static void Register(string commandName, ChatCommandHandlerDelegate handler, Func<string, string> helpDescription, string[] aliases = null, bool mainOnly = false, bool multiplayerOnly = false, bool cheatsOnly = false)
	{
		commandName = commandName?.Trim();
		if (string.IsNullOrWhiteSpace(commandName))
		{
			throw new ArgumentException("The chat command name can't be null or empty.", "commandName");
		}
		if (ChatCommands.Handlers.ContainsKey(commandName))
		{
			throw new InvalidOperationException("The chat command name '" + commandName + "' is already registered.");
		}
		if (ChatCommands.Aliases.TryGetValue(commandName, out var aliasFor))
		{
			throw new InvalidOperationException($"The chat command name '{commandName}' is already registered as an alias of '{aliasFor}'.");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		ChatCommands.Handlers[commandName] = new ChatCommand(commandName, helpDescription, handler, mainOnly, multiplayerOnly, cheatsOnly);
		if (aliases != null && aliases.Length != 0)
		{
			for (int i = 0; i < aliases.Length; i++)
			{
				ChatCommands.RegisterAlias(aliases[i], commandName);
			}
		}
	}

	/// <summary>Register an alternate name for a chat command.</summary>
	/// <param name="alias">The alias to register. This should only contain alphanumeric, underscore, and dot characters. For custom chat command, this should be prefixed with your mod ID like <c>Example.ModId_Command</c>.</param>
	/// <param name="commandName">The chat command name to map it to, like <c>help</c>. This should already be registered (e.g. via <see cref="M:StardewValley.ChatCommands.Register(System.String,StardewValley.Delegates.ChatCommandHandlerDelegate,System.Func{System.String,System.String},System.String[],System.Boolean,System.Boolean,System.Boolean)" />).</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="alias" /> or <paramref name="commandName" /> is null or whitespace-only.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="commandName" /> is already registered.</exception>
	public static void RegisterAlias(string alias, string commandName)
	{
		alias = alias?.Trim();
		if (string.IsNullOrWhiteSpace(alias))
		{
			throw new ArgumentException("The alias can't be null or empty.", "alias");
		}
		if (ChatCommands.Handlers.ContainsKey(alias))
		{
			throw new InvalidOperationException("The alias '" + alias + "' is already registered as a chat command name.");
		}
		if (ChatCommands.Aliases.TryGetValue(alias, out var otherQuery))
		{
			throw new InvalidOperationException($"The alias '{alias}' is already registered for '{otherQuery}'.");
		}
		if (string.IsNullOrWhiteSpace(commandName))
		{
			throw new ArgumentException("The chat command name can't be null or empty.", "alias");
		}
		if (!ChatCommands.Handlers.ContainsKey(commandName))
		{
			throw new InvalidOperationException($"The alias '{alias}' can't be registered for '{commandName}' because there's no chat command with that name.");
		}
		ChatCommands.Aliases[alias] = commandName;
	}

	/// <summary>Try to handle a chat command.</summary>
	/// <param name="command">The full chat command split by spaces, including the command name and arguments.</param>
	/// <param name="chat">The chat box through which the command was entered.</param>
	/// <returns>Returns whether the command was found and executed, regardless of whether the command logic succeeded.</returns>
	public static bool TryHandle(string[] command, ChatBox chat)
	{
		string commandName = ArgUtility.Get(command, 0);
		if (string.IsNullOrWhiteSpace(commandName))
		{
			return false;
		}
		if (ChatCommands.Aliases.TryGetValue(commandName, out var aliasTarget))
		{
			commandName = aliasTarget;
		}
		if (!ChatCommands.Handlers.TryGetValue(commandName, out var handler))
		{
			return false;
		}
		if (handler.IsMainPlayerOnly && !Game1.IsMasterGame)
		{
			chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_HostOnly"));
			return true;
		}
		if (handler.IsMultiplayerOnly && !Game1.IsServer && !Game1.IsMultiplayer)
		{
			chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_MultiplayerOnly"));
			return true;
		}
		if (handler.IsCheatsOnly && !ChatCommands.AllowCheats)
		{
			switch (handler.Name)
			{
			case "cheat":
			case "debug":
			case "money":
				chat.addNiceTryEasterEggMessage();
				return true;
			default:
				chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_CheatsOnly"));
				return true;
			}
		}
		try
		{
			handler.Handler(command, chat);
			return true;
		}
		catch (Exception exception)
		{
			Game1.log.Error("Error running chat command '" + string.Join(" ", command) + "'.", exception);
			return false;
		}
	}

	/// <summary>Create a chat command handler which passes a chat command through to a debug command directly without checking if <see cref="P:StardewValley.ChatCommands.AllowCheats" /> is enabled, with the same arguments</summary>
	/// <param name="debugCommandName">The name of the debug command registered with <see cref="T:StardewValley.DebugCommands" />.</param>
	public static ChatCommandHandlerDelegate GetDebugPassThrough(string debugCommandName)
	{
		return Handle;
		void Handle(string[] command, ChatBox chat)
		{
			command[0] = debugCommandName;
			string commandText = ArgUtility.UnsplitQuoteAware(command, ' ');
			chat.cheat(commandText);
		}
	}
}
