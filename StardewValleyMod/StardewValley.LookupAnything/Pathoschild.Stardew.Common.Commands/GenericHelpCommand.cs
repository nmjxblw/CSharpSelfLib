using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Commands;

internal class GenericHelpCommand : BaseCommand
{
	private readonly string ModName;

	private readonly string RootName;

	private readonly Func<InvariantDictionary<ICommand>> GetCommands;

	internal const string CommandName = "help";

	public GenericHelpCommand(string rootName, string modName, IMonitor monitor, Func<InvariantDictionary<ICommand>> getCommands)
		: base(monitor, "help")
	{
		this.ModName = modName;
		this.RootName = rootName;
		this.GetCommands = getCommands;
	}

	public override string GetDescription()
	{
		return $"\r\n                {this.RootName} {"help"}\r\n                   Usage: {this.RootName} {"help"}\r\n                   Lists all available {this.RootName} commands.\r\n\r\n                   Usage: {this.RootName} {"help"} <cmd>\r\n                   Provides information for a specific {this.RootName} command.\r\n                   - cmd: The {this.RootName} command name.\r\n            ";
	}

	public override void Handle(string[] args)
	{
		InvariantDictionary<ICommand> commands = this.GetCommands();
		StringBuilder help = new StringBuilder();
		ICommand command;
		if (!args.Any())
		{
			StringBuilder stringBuilder = help;
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(150, 5, stringBuilder);
			handler.AppendLiteral("The '");
			handler.AppendFormatted(this.RootName);
			handler.AppendLiteral("' command is the entry point for ");
			handler.AppendFormatted(this.ModName);
			handler.AppendLiteral(" commands. You use it by specifying a more ");
			handler.AppendLiteral("specific command (like '");
			handler.AppendFormatted("help");
			handler.AppendLiteral("' in '");
			handler.AppendFormatted(this.RootName);
			handler.AppendLiteral(" ");
			handler.AppendFormatted("help");
			handler.AppendLiteral("'). Here are the available commands:\n\n");
			stringBuilder2.AppendLine(ref handler);
			foreach (KeyValuePair<string, ICommand> item in commands.OrderBy<KeyValuePair<string, ICommand>, string>((KeyValuePair<string, ICommand> p) => p.Key, HumanSortComparer.DefaultIgnoreCase))
			{
				help.AppendLine(item.Value.Description);
				help.AppendLine();
				help.AppendLine();
			}
		}
		else if (commands.TryGetValue(args[0], out command))
		{
			help.AppendLine(command.Description);
		}
		else
		{
			StringBuilder stringBuilder = help;
			StringBuilder stringBuilder3 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(53, 4, stringBuilder);
			handler.AppendLiteral("Unknown command '");
			handler.AppendFormatted(this.RootName);
			handler.AppendLiteral(" ");
			handler.AppendFormatted(args[0]);
			handler.AppendLiteral("'. Type '");
			handler.AppendFormatted(this.RootName);
			handler.AppendLiteral(" ");
			handler.AppendFormatted("help");
			handler.AppendLiteral("' for available commands.");
			stringBuilder3.AppendLine(ref handler);
		}
		base.Monitor.Log(help.ToString().Trim(), (LogLevel)2);
	}
}
