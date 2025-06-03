using System;
using System.Reflection;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace BirbCore.Attributes;

public class Parser
{
	internal static event EventHandler? Priority1Event;

	internal static event EventHandler? Priority2Event;

	internal static event EventHandler? Priority3Event;

	internal static event EventHandler? Priority4Event;

	internal static event EventHandler? Priority5Event;

	internal static event EventHandler? Priority6Event;

	internal static event EventHandler? Priority7Event;

	internal static event EventHandler? Priority8Event;

	internal static event EventHandler? Priority9Event;

	public static void ParseAll(IMod mod)
	{
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		Assembly assembly = ((object)mod).GetType().Assembly;
		Log.Init(mod.Monitor, assembly);
		Type[] types = assembly.GetTypes();
		foreach (Type type in types)
		{
			ClassHandler[] classHandlers = (ClassHandler[])Attribute.GetCustomAttributes(type, typeof(ClassHandler));
			if (classHandlers.Length == 0)
			{
				continue;
			}
			object instance = Activator.CreateInstance(type);
			ClassHandler[] array = classHandlers;
			foreach (ClassHandler handler in array)
			{
				switch (handler.Priority)
				{
				case 0:
					handler.Handle(type, instance, mod);
					break;
				case 1:
					Priority1Event += delegate
					{
						WrapHandler(handler, type, instance, mod);
					};
					break;
				case 2:
					Priority2Event += delegate
					{
						WrapHandler(handler, type, instance, mod);
					};
					break;
				case 3:
					Priority3Event += delegate
					{
						WrapHandler(handler, type, instance, mod);
					};
					break;
				case 4:
					Priority4Event += delegate
					{
						WrapHandler(handler, type, instance, mod);
					};
					break;
				case 5:
					Priority5Event += delegate
					{
						WrapHandler(handler, type, instance, mod);
					};
					break;
				case 6:
					Priority6Event += delegate
					{
						WrapHandler(handler, type, instance, mod);
					};
					break;
				case 7:
					Priority7Event += delegate
					{
						WrapHandler(handler, type, instance, mod);
					};
					break;
				case 8:
					Priority8Event += delegate
					{
						WrapHandler(handler, type, instance, mod);
					};
					break;
				case 9:
					Priority9Event += delegate
					{
						WrapHandler(handler, type, instance, mod);
					};
					break;
				}
			}
		}
		new Harmony(mod.ModManifest.UniqueID).PatchAll(assembly);
	}

	private static void WrapHandler(ClassHandler handler, Type type, object? instance, IMod mod)
	{
		try
		{
			handler.Handle(type, instance, mod);
		}
		catch (Exception value)
		{
			mod.Monitor.Log($"BirbCore failed to parse {handler.GetType().Name} class {type}: {value}", (LogLevel)4);
		}
	}

	internal static void InitEvents()
	{
		((Mod)ModEntry.Instance).Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
		((Mod)ModEntry.Instance).Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
		((Mod)ModEntry.Instance).Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
	}

	private static void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
	{
		Log.Trace("=== Running Priority 1 events ===");
		Parser.Priority1Event?.Invoke(sender, EventArgs.Empty);
	}

	private static void GameLoop_UpdateTicking(object? sender, UpdateTickingEventArgs e)
	{
		switch (e.Ticks)
		{
		case 0u:
			Log.Trace("=== Running Priority 2 events ===");
			Parser.Priority2Event?.Invoke(sender, EventArgs.Empty);
			break;
		case 1u:
			Log.Trace("=== Running Priority 4 events ===");
			Parser.Priority4Event?.Invoke(sender, EventArgs.Empty);
			break;
		case 2u:
			Log.Trace("=== Running Priority 6 events ===");
			Parser.Priority6Event?.Invoke(sender, EventArgs.Empty);
			break;
		case 3u:
			Log.Trace("=== Running Priority 8 events ===");
			Parser.Priority8Event?.Invoke(sender, EventArgs.Empty);
			break;
		default:
			((Mod)ModEntry.Instance).Helper.Events.GameLoop.UpdateTicking -= GameLoop_UpdateTicking;
			break;
		}
	}

	private static void GameLoop_UpdateTicked(object? sender, UpdateTickedEventArgs e)
	{
		switch (e.Ticks)
		{
		case 0u:
			Log.Trace("=== Running Priority 3 events ===");
			Parser.Priority3Event?.Invoke(sender, EventArgs.Empty);
			break;
		case 1u:
			Log.Trace("=== Running Priority 5 events ===");
			Parser.Priority5Event?.Invoke(sender, EventArgs.Empty);
			break;
		case 2u:
			Log.Trace("=== Running Priority 7 events ===");
			Parser.Priority7Event?.Invoke(sender, EventArgs.Empty);
			break;
		case 3u:
			Log.Trace("=== Running Priority 9 events ===");
			Parser.Priority9Event?.Invoke(sender, EventArgs.Empty);
			break;
		default:
			((Mod)ModEntry.Instance).Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
			break;
		}
	}
}
