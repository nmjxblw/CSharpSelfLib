using StardewValley.Menus;

namespace StardewValley.Delegates;

/// <summary>Handles a chat command.</summary>
/// <param name="command">The full chat command split by spaces, including the command name.</param>
/// <param name="chat">The chat box through which the command was entered.</param>
public delegate void ChatCommandHandlerDelegate(string[] command, ChatBox chat);
