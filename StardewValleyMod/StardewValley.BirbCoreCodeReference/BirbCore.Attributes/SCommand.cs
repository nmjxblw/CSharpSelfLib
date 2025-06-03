using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BirbCore.Extensions;
using StardewModdingAPI;
using StardewValley;

namespace BirbCore.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class SCommand(string name, string help = "") : ClassHandler(2)
{
	public class Command(string help = "") : MethodHandler()
	{
		public override void Handle(MethodInfo method, object? instance, IMod mod, object[]? args = null)
		{
			if (instance == null)
			{
				Log.Error("SCommand class may be static? Cannot parse subcommands.");
				return;
			}
			if (args == null)
			{
				Log.Error("SCommand class didn't pass args");
				return;
			}
			Dictionary<string, Action<string[]>> commands = (Dictionary<string, Action<string[]>>)args[0];
			Dictionary<string, string> helps = (Dictionary<string, string>)args[1];
			string command = (string)args[2];
			string subCommand = method.Name.ToSnakeCase();
			commands.Add(subCommand, delegate(string[] commandArgs)
			{
				List<object> list = new List<object>();
				for (int i = 0; i < method.GetParameters().Length; i++)
				{
					ParameterInfo parameterInfo = method.GetParameters()[i];
					string arg = ((commandArgs != null && commandArgs.Length > i) ? commandArgs[i] : null);
					list.Add(ParseArg(arg, parameterInfo));
					if (parameterInfo.GetCustomAttribute(typeof(ParamArrayAttribute), inherit: false) != null)
					{
						for (int j = i + 1; j < ((commandArgs != null) ? commandArgs.Length : 0); j++)
						{
							list.Add(ParseArg((commandArgs != null) ? commandArgs[j] : null, parameterInfo));
						}
					}
				}
				method.Invoke(instance, list.ToArray());
			});
			StringBuilder help1 = new StringBuilder();
			StringBuilder stringBuilder = help1;
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(1, 2, stringBuilder);
			handler.AppendFormatted(command);
			handler.AppendLiteral(" ");
			handler.AppendFormatted(subCommand);
			stringBuilder2.Append(ref handler);
			ParameterInfo[] parameters = method.GetParameters();
			foreach (ParameterInfo parameter in parameters)
			{
				string dotDotDot = ((parameter.GetCustomAttribute<ParamArrayAttribute>() != null) ? "..." : "");
				if (parameter.IsOptional)
				{
					stringBuilder = help1;
					StringBuilder stringBuilder3 = stringBuilder;
					handler = new StringBuilder.AppendInterpolatedStringHandler(3, 2, stringBuilder);
					handler.AppendLiteral(" [");
					handler.AppendFormatted(parameter.Name);
					handler.AppendFormatted(dotDotDot);
					handler.AppendLiteral("]");
					stringBuilder3.Append(ref handler);
				}
				else
				{
					stringBuilder = help1;
					StringBuilder stringBuilder4 = stringBuilder;
					handler = new StringBuilder.AppendInterpolatedStringHandler(3, 2, stringBuilder);
					handler.AppendLiteral(" <");
					handler.AppendFormatted(parameter.Name);
					handler.AppendFormatted(dotDotDot);
					handler.AppendLiteral(">");
					stringBuilder4.Append(ref handler);
				}
			}
			stringBuilder = help1;
			StringBuilder stringBuilder5 = stringBuilder;
			handler = new StringBuilder.AppendInterpolatedStringHandler(3, 1, stringBuilder);
			handler.AppendLiteral("\n\t\t");
			handler.AppendFormatted(help);
			stringBuilder5.Append(ref handler);
			helps.Add(subCommand, help1.ToString());
		}

		private static object ParseArg(string? arg, ParameterInfo parameter)
		{
			if (arg == null)
			{
				return Type.Missing;
			}
			if (parameter.ParameterType == typeof(string))
			{
				return arg;
			}
			if (parameter.ParameterType == typeof(int))
			{
				return int.Parse(arg);
			}
			if (parameter.ParameterType == typeof(double) || parameter.ParameterType == typeof(float))
			{
				return float.Parse(arg);
			}
			if (parameter.ParameterType == typeof(bool))
			{
				return bool.Parse(arg);
			}
			if (parameter.ParameterType == typeof(GameLocation))
			{
				return Utility.fuzzyLocationSearch(arg);
			}
			if (parameter.ParameterType == typeof(NPC))
			{
				return Utility.fuzzyCharacterSearch(arg, true);
			}
			if (parameter.ParameterType == typeof(FarmAnimal))
			{
				return Utility.fuzzyAnimalSearch(arg);
			}
			if (parameter.ParameterType == typeof(Farmer))
			{
				if (long.TryParse(arg, out var playerId))
				{
					return Game1.GetPlayer(playerId, false);
				}
				if (!arg.Equals("host", StringComparison.InvariantCultureIgnoreCase))
				{
					return Game1.player;
				}
				return Game1.MasterPlayer;
			}
			if (!(parameter.ParameterType == typeof(Item)))
			{
				return Type.Missing;
			}
			return Utility.fuzzyItemSearch(arg, 1, false);
		}
	}

	private readonly Dictionary<string, Action<string[]>> _commands = new Dictionary<string, Action<string[]>>();

	private readonly Dictionary<string, string> _helps = new Dictionary<string, string>();

	public override void Handle(Type type, object? instance, IMod mod, object[]? args = null)
	{
		base.Handle(type, instance, mod, new object[3] { _commands, _helps, name });
		mod.Helper.ConsoleCommands.Add(name, GetHelp(), (Action<string, string[]>)delegate(string s, string[] commandArgs)
		{
			CallCommand(commandArgs);
		});
	}

	private string GetHelp(string? subCommand = null)
	{
		if (subCommand != null)
		{
			return _helps[subCommand];
		}
		StringBuilder sb = new StringBuilder();
		StringBuilder stringBuilder = sb;
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(3, 2, stringBuilder);
		handler.AppendFormatted(name);
		handler.AppendLiteral(": ");
		handler.AppendFormatted(help);
		handler.AppendLiteral("\n");
		stringBuilder2.Append(ref handler);
		foreach (string sub in _helps.Keys)
		{
			string helpText = _helps[sub];
			stringBuilder = sb;
			StringBuilder stringBuilder3 = stringBuilder;
			handler = new StringBuilder.AppendInterpolatedStringHandler(3, 1, stringBuilder);
			handler.AppendLiteral("\t");
			handler.AppendFormatted(helpText);
			handler.AppendLiteral("\n\n");
			stringBuilder3.Append(ref handler);
		}
		return sb.ToString();
	}

	private void CallCommand(string[] args)
	{
		if (args.Length == 0)
		{
			Log.Info(GetHelp());
			return;
		}
		if (args[0].Equals("help", StringComparison.InvariantCultureIgnoreCase) || args[0].Equals("-h", StringComparison.InvariantCultureIgnoreCase))
		{
			if (args.Length > 1 && _helps.ContainsKey(args[1]))
			{
				Log.Info(GetHelp(args[1]));
			}
			else
			{
				Log.Info(GetHelp());
			}
			return;
		}
		try
		{
			_commands[args[0]](args[1..]);
		}
		catch (Exception ex)
		{
			Log.Info(GetHelp(args[0]));
			Log.Trace("Args are:" + string.Join(" ", args));
			Log.Trace(ex.ToString());
		}
	}
}
