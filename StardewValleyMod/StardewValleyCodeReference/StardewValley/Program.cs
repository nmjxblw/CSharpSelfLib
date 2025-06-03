using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using StardewValley.Network.Compress;
using StardewValley.SDKs;
using StardewValley.SDKs.Steam;

namespace StardewValley;

public static class Program
{
	public enum LogType
	{
		Error,
		Disconnect
	}

	public const int build_steam = 0;

	public const int build_gog = 1;

	public const int build_rail = 2;

	public const int build_gdk = 3;

	public static bool GameTesterMode = false;

	public static bool releaseBuild = true;

	public static bool enableCheats = !Program.releaseBuild;

	public const int buildType = 0;

	private static SDKHelper _sdk;

	internal static readonly INetCompression defaultCompression = new LZ4NetCompression();

	internal static INetCompression netCompression = Program.defaultCompression;

	public static Game1 gamePtr;

	public static bool handlingException;

	public static bool hasTriedToPrintLog;

	public static bool successfullyPrintedLog;

	internal static SDKHelper sdk
	{
		get
		{
			if (Program._sdk == null)
			{
				Program._sdk = new SteamHelper();
				if (Program._sdk == null)
				{
					Program._sdk = new NullSDKHelper();
				}
			}
			return Program._sdk;
		}
	}

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	public static void Main(string[] args)
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		Program.GameTesterMode = true;
		AppDomain.CurrentDomain.UnhandledException += handleException;
		using GameRunner game = new GameRunner();
		GameRunner.instance = game;
		game.Run();
	}

	/// <summary>Get the absolute path to the folder containing local app data (like error logs and screenshots), creating it if needed.</summary>
	/// <param name="subfolder">The name of the subfolder to append to the path, if any.</param>
	/// <param name="createIfMissing">Whether to create the folder if it doesn't exist already.</param>
	public static string GetLocalAppDataFolder(string subfolder = null, bool createIfMissing = true)
	{
		if (Environment.OSVersion.Platform == PlatformID.Unix)
		{
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			if (!string.IsNullOrWhiteSpace(appDataPath))
			{
				string fullPath = ((subfolder != null) ? Path.Combine(appDataPath, "StardewValley", subfolder) : Path.Combine(appDataPath, "StardewValley"));
				if (createIfMissing)
				{
					Directory.CreateDirectory(fullPath);
				}
				return fullPath;
			}
		}
		return Program.GetAppDataFolder(subfolder, createIfMissing);
	}

	/// <summary>Get the absolute path to the folder containing global app data (like saves), creating it if needed.</summary>
	/// <param name="subfolder">The name of the subfolder to append to the path, if any.</param>
	/// <param name="createIfMissing">Whether to create the folder if it doesn't exist already.</param>
	public static string GetAppDataFolder(string subfolder = null, bool createIfMissing = true)
	{
		string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		string fullPath = ((subfolder != null) ? Path.Combine(appDataPath, "StardewValley", subfolder) : Path.Combine(appDataPath, "StardewValley"));
		if (createIfMissing)
		{
			Directory.CreateDirectory(fullPath);
		}
		return fullPath;
	}

	/// <summary>Get the absolute path to the debug log file.</summary>
	public static string GetDebugLogPath()
	{
		return Path.Combine(Program.GetLocalAppDataFolder("ErrorLogs"), "game-latest.txt");
	}

	/// <summary>Get the absolute path to the folder containing save folders, creating it if needed.</summary>
	public static string GetSavesFolder()
	{
		return Program.GetAppDataFolder("Saves");
	}

	public static string WriteLog(LogType logType, string message, bool append = false)
	{
		string filename = Game1.player?.Name ?? "NullPlayer";
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		filename = string.Join("-", filename.Split(invalidFileNameChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
		filename = string.Join("-", filename.Split(new char[0], StringSplitOptions.RemoveEmptyEntries));
		string logFolderName;
		if (logType == LogType.Disconnect)
		{
			logFolderName = "DisconnectLogs";
			filename += $"_{DateTime.Now.Month}-{DateTime.Now.Day}.txt";
		}
		else
		{
			logFolderName = "ErrorLogs";
			filename += $"_{Game1.uniqueIDForThisGame}_{((Game1.player == null) ? ((long)Game1.random.Next(999999)) : ((long)Game1.player.millisecondsPlayed))}.txt";
		}
		string logFolderPath = Program.GetLocalAppDataFolder(logFolderName);
		if (logFolderPath == null)
		{
			Game1.log.Error("WriteLog failed on GetLocalAppDataFolder(\"" + logFolderName + "\")");
			return null;
		}
		string fullFilePath = Path.Combine(logFolderPath, filename);
		try
		{
			if (append)
			{
				File.AppendAllText(fullFilePath, message + Environment.NewLine);
			}
			else
			{
				File.WriteAllText(fullFilePath, message);
			}
		}
		catch (Exception exception)
		{
			Game1.log.Error("WriteLog failed with exception:", exception);
			return null;
		}
		return fullFilePath;
	}

	public static void AppendDiagnostics(StringBuilder sb)
	{
		sb.AppendLine("Game Version: " + Game1.GetVersionString());
		try
		{
			if (Program.sdk != null)
			{
				sb.AppendLine("SDK Helper: " + Program.sdk.GetType().Name);
			}
			sb.AppendLine("Game Language: " + LocalizedContentManager.CurrentLanguageCode);
			try
			{
				sb.AppendLine("GPU: " + Game1.graphics.GraphicsDevice.Adapter.Description);
			}
			catch (Exception)
			{
				sb.AppendLine("GPU: Could not detect.");
			}
			sb.AppendLine("OS: " + Environment.OSVersion.Platform.ToString() + " " + Environment.OSVersion.VersionString);
			if (GameRunner.instance != null && GameRunner.instance.GetType().FullName.StartsWith("StardewModdingAPI."))
			{
				sb.AppendLine("Running SMAPI");
			}
			if (Game1.IsMultiplayer)
			{
				if (LocalMultiplayer.IsLocalMultiplayer())
				{
					sb.AppendLine("Multiplayer (Split Screen)");
				}
				else if (Game1.IsMasterGame)
				{
					sb.AppendLine("Multiplayer (Host)");
				}
				else
				{
					sb.AppendLine("Multiplayer (Client)");
				}
			}
			if (Game1.options.gamepadControls)
			{
				sb.AppendLine("Playing on Controller");
			}
			sb.AppendLine("In-game Date: " + Game1.season.ToString() + " " + Game1.dayOfMonth + " Y" + Game1.year + " Time of Day: " + Game1.timeOfDay);
			sb.AppendLine("Game Location: " + ((Game1.currentLocation == null) ? "null" : Game1.currentLocation.NameOrUniqueName));
		}
		catch (Exception)
		{
		}
	}

	public static void handleException(object sender, UnhandledExceptionEventArgs args)
	{
		if (Program.handlingException || !Program.GameTesterMode)
		{
			return;
		}
		Game1.gameMode = 11;
		Program.handlingException = true;
		StringBuilder sb = new StringBuilder();
		if (args != null)
		{
			Exception e = (Exception)args.ExceptionObject;
			sb.AppendLine("Message: " + e.Message);
			sb.AppendLine("InnerException: " + e.InnerException);
			sb.AppendLine("Stack Trace: " + e.StackTrace);
			sb.AppendLine("");
		}
		Program.AppendDiagnostics(sb);
		Game1.errorMessage = sb.ToString();
		if (!Program.hasTriedToPrintLog)
		{
			Program.hasTriedToPrintLog = true;
			string successfulErrorPath = Program.WriteLog(LogType.Error, Game1.errorMessage);
			if (successfulErrorPath != null)
			{
				Program.successfullyPrintedLog = true;
				Game1.errorMessage = "(Error Report created at " + successfulErrorPath + ")" + Environment.NewLine + Game1.errorMessage;
			}
		}
		if (args != null)
		{
			Game1.gameMode = 3;
		}
	}
}
