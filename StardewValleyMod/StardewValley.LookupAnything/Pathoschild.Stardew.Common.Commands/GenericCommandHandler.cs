using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Commands;

internal class GenericCommandHandler
{
	private readonly IMonitor Monitor;

	private readonly InvariantDictionary<ICommand> Commands;

	public string ModName { get; }

	public string RootName { get; }

	public GenericCommandHandler(string rootName, string modName, IEnumerable<ICommand> commands, IMonitor monitor)
	{
		this.RootName = rootName;
		this.ModName = modName;
		this.Monitor = monitor;
		this.Commands = new InvariantDictionary<ICommand>(commands.ToDictionary((ICommand p) => p.Name));
		this.Commands["help"] = new GenericHelpCommand(rootName, modName, monitor, () => this.Commands);
	}

	public bool Handle(string[] args)
	{
		string commandName = args.FirstOrDefault() ?? "help";
		string[] commandArgs = args.Skip(1).ToArray();
		if (this.Commands.TryGetValue(commandName, out ICommand command))
		{
			command.Handle(commandArgs);
			return true;
		}
		this.Monitor.Log($"The '{this.RootName} {args[0]}' command isn't valid. Type '{this.RootName} {"help"}' for a list of valid commands.", (LogLevel)4);
		return false;
	}

	public void RegisterWith(ICommandHelper commandHelper)
	{
		commandHelper.Add(this.RootName, $"Starts a {this.ModName} command. Type '{this.RootName} {"help"}' for details.", (Action<string, string[]>)delegate(string _, string[] args)
		{
			this.Handle(args);
		});
	}
}
