using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;

namespace SpaceShared.ConsoleCommands;

internal static class ConsoleCommandHelper
{
	public static IEnumerable<IConsoleCommand> FindCommandsInAssembly(Mod mod)
	{
		return from type in ((object)mod).GetType().Assembly.GetTypes()
			where !type.IsAbstract && typeof(IConsoleCommand).IsAssignableFrom(type)
			select (IConsoleCommand)Activator.CreateInstance(type);
	}

	public static void RegisterCommandsInAssembly(Mod mod)
	{
		foreach (IConsoleCommand command in ConsoleCommandHelper.FindCommandsInAssembly(mod))
		{
			mod.Helper.ConsoleCommands.Add(command.Name, command.Description, (Action<string, string[]>)delegate(string name, string[] args)
			{
				command.Handle(mod.Monitor, name, args);
			});
		}
	}
}
