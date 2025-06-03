using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewValley;

public static class DebugTools
{
	private static int _mainThreadId;

	private const string CommentFormat = "#----------------------------------------------------------------------------#";

	public static DebugMetricsComponent _metrics;

	private static bool _noFpsCap;

	public static string FormatDivider(string label = null)
	{
		if (string.IsNullOrEmpty(label))
		{
			return "#----------------------------------------------------------------------------#";
		}
		label = " " + label + " ";
		int src = "#----------------------------------------------------------------------------#".Length / 2 - label.Length / 2;
		int dst = src + label.Length;
		return "#----------------------------------------------------------------------------#".Substring(0, src) + label + "#----------------------------------------------------------------------------#".Substring(dst);
	}

	[Conditional("VALIDATE_MAIN_THREAD_ENABLED")]
	public static void ValidateIsMainThread(bool req)
	{
		if (Thread.CurrentThread.ManagedThreadId == DebugTools._mainThreadId != req)
		{
			Game1.log.Warn(DebugTools.FormatDivider("ERROR: CODE EXECUTED ON UNSAFE THREAD!"));
			Debugger.Break();
			Environment.Exit(1);
		}
	}

	public static bool IsMainThread()
	{
		return Thread.CurrentThread.ManagedThreadId == DebugTools._mainThreadId;
	}

	public static void Assert(bool expression, string failureMessage)
	{
		if (!expression)
		{
			Game1.log.Error(failureMessage);
		}
	}

	public static void GameConstructed(Game game)
	{
		DebugTools._mainThreadId = Thread.CurrentThread.ManagedThreadId;
	}

	public static void GameLoadContent(Game game)
	{
	}

	public static void BeforeGameInitialize(Game game)
	{
		DebugTools.ApplyNoFpsCap(DebugTools._noFpsCap);
	}

	public static void BeforeGameUpdate(Game1 game, ref GameTime gameTime)
	{
		if (!Program.releaseBuild)
		{
			DebugTools.CheckInput(game);
			if (DebugTools._noFpsCap)
			{
				gameTime = new GameTime(gameTime.TotalGameTime, game.TargetElapsedTime, gameTime.IsRunningSlowly);
			}
		}
	}

	public static void BeforeGameDraw(Game1 game, ref GameTime time)
	{
		if (DebugTools._noFpsCap)
		{
			time = new GameTime(time.TotalGameTime, game.TargetElapsedTime, time.IsRunningSlowly);
		}
	}

	private static void CheckInput(Game1 game)
	{
		GamePadState state = Game1.input.GetGamePadState();
		if (Game1.IsPressEvent(ref state, Buttons.LeftStick))
		{
			if (DebugTools._metrics != null)
			{
				DebugTools._metrics.Visible = !DebugTools._metrics.Visible;
			}
			Game1.log.Verbose("Toggling Metrics (" + ((DebugTools._metrics == null) ? "[null]" : DebugTools._metrics.Visible.ToString()) + ")");
		}
		if (Game1.IsPressEvent(ref state, Buttons.RightStick) && state.IsButtonDown(Buttons.LeftStick))
		{
			DebugTools._noFpsCap = !DebugTools._noFpsCap;
			DebugTools.ApplyNoFpsCap(DebugTools._noFpsCap);
		}
	}

	private static void ApplyNoFpsCap(bool nocap)
	{
	}
}
