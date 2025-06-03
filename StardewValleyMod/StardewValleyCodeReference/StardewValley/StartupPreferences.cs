using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using StardewValley.GameData;
using StardewValley.SaveSerialization;

namespace StardewValley;

public class StartupPreferences
{
	public const int windowed_borderless = 0;

	public const int windowed = 1;

	public const int fullscreen = 2;

	private static readonly string _filename = "startup_preferences";

	public static XmlSerializer serializer = null;

	public bool startMuted;

	public bool levelTenFishing;

	public bool levelTenMining;

	public bool levelTenForaging;

	public bool levelTenCombat;

	public bool skipWindowPreparation;

	public bool sawAdvancedCharacterCreationIndicator;

	public int timesPlayed;

	public int windowMode;

	public int displayIndex = -1;

	public Options.GamepadModes gamepadMode;

	public int playerLimit = -1;

	public int fullscreenResolutionX;

	public int fullscreenResolutionY;

	public string lastEnteredIP = "";

	public string languageCode;

	public Options clientOptions = new Options();

	[XmlIgnore]
	public bool isLoaded;

	private bool _isBusy;

	private bool _pendingApplyLanguage;

	private Task _task;

	[XmlIgnore]
	public bool IsBusy
	{
		get
		{
			lock (this)
			{
				if (!this._isBusy)
				{
					return false;
				}
				if (this._task == null)
				{
					throw new Exception("StartupPreferences.IsBusy; was busy but task is null?");
				}
				if (this._task.IsFaulted)
				{
					Exception e = this._task.Exception.GetBaseException();
					Game1.log.Error("StartupPreferences._task failed with an exception.", e);
					throw e;
				}
				if (this._task.IsCompleted)
				{
					this._task = null;
					this._isBusy = false;
					if (this._pendingApplyLanguage)
					{
						this._SetLanguageFromCode(this.languageCode);
					}
				}
				return this._isBusy;
			}
		}
	}

	private void Init()
	{
		this.isLoaded = false;
		this.ensureFolderStructureExists();
	}

	public void OnLanguageChange(LocalizedContentManager.LanguageCode code)
	{
		string language_id = code.ToString();
		if (code == LocalizedContentManager.LanguageCode.mod && LocalizedContentManager.CurrentModLanguage != null)
		{
			language_id = LocalizedContentManager.CurrentModLanguage.Id;
		}
		if (this.isLoaded && this.languageCode != language_id)
		{
			this.savePreferences(async: false, update_language_from_ingame_language: true);
		}
	}

	private void ensureFolderStructureExists()
	{
		Program.GetAppDataFolder();
	}

	public void savePreferences(bool async, bool update_language_from_ingame_language = false)
	{
		lock (this)
		{
			if (update_language_from_ingame_language)
			{
				if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
				{
					this.languageCode = LocalizedContentManager.CurrentModLanguage.Id;
				}
				else
				{
					this.languageCode = LocalizedContentManager.CurrentLanguageCode.ToString();
				}
			}
			try
			{
				this._savePreferences();
			}
			catch (Exception ex)
			{
				Game1.log.Error("StartupPreferences._task failed with an exception.", ex);
				throw ex;
			}
		}
	}

	private void _savePreferences()
	{
		string fullFilePath = Path.Combine(Program.GetAppDataFolder(), StartupPreferences._filename);
		try
		{
			this.ensureFolderStructureExists();
			if (File.Exists(fullFilePath))
			{
				File.Delete(fullFilePath);
			}
			using FileStream stream = File.Create(fullFilePath);
			this.writeSettings(stream);
		}
		catch (Exception ex)
		{
			Game1.debugOutput = Game1.parseText(ex.Message);
		}
	}

	private long writeSettings(Stream stream)
	{
		XmlWriterSettings settings = new XmlWriterSettings
		{
			CloseOutput = true,
			Indent = true
		};
		using XmlWriter writer = XmlWriter.Create(stream, settings);
		writer.WriteStartDocument();
		StartupPreferences.serializer.SerializeFast(writer, this);
		writer.WriteEndDocument();
		writer.Flush();
		return stream.Length;
	}

	public void loadPreferences(bool async, bool applyLanguage)
	{
		lock (this)
		{
			this._pendingApplyLanguage = applyLanguage;
			this.Init();
			try
			{
				this._loadPreferences();
			}
			catch (Exception ex)
			{
				Exception e = this._task.Exception?.GetBaseException() ?? ex;
				Game1.log.Error("StartupPreferences._task failed with an exception.", e);
				throw e;
			}
			if (applyLanguage)
			{
				this._SetLanguageFromCode(this.languageCode);
			}
		}
	}

	protected virtual void _SetLanguageFromCode(string language_code_string)
	{
		List<ModLanguage> mod_languages = DataLoader.AdditionalLanguages(Game1.content);
		bool found_language = false;
		if (mod_languages != null)
		{
			foreach (ModLanguage mod_language in mod_languages)
			{
				if (mod_language.Id == language_code_string)
				{
					LocalizedContentManager.SetModLanguage(mod_language);
					found_language = true;
					break;
				}
			}
		}
		if (!found_language)
		{
			if (Utility.TryParseEnum<LocalizedContentManager.LanguageCode>(language_code_string, out var language_code) && language_code != LocalizedContentManager.LanguageCode.mod)
			{
				LocalizedContentManager.CurrentLanguageCode = language_code;
			}
			else
			{
				LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.GetDefaultLanguageCode();
			}
		}
	}

	private void _loadPreferences()
	{
		string fullFilePath = Path.Combine(Program.GetAppDataFolder(), StartupPreferences._filename);
		if (!File.Exists(fullFilePath))
		{
			Game1.log.Verbose("path '" + fullFilePath + "' did not exist and will be created");
			try
			{
				if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
				{
					this.languageCode = LocalizedContentManager.CurrentModLanguage.Id;
				}
				else
				{
					this.languageCode = LocalizedContentManager.CurrentLanguageCode.ToString();
				}
				using FileStream stream = File.Create(fullFilePath);
				this.writeSettings(stream);
			}
			catch (Exception ex)
			{
				Game1.log.Error("_loadPreferences; exception occurred trying to create/write.", ex);
				Game1.debugOutput = Game1.parseText(ex.Message);
				return;
			}
		}
		try
		{
			using (FileStream stream2 = File.Open(fullFilePath, FileMode.Open, FileAccess.Read))
			{
				this.readSettings(stream2);
			}
			this.isLoaded = true;
		}
		catch (Exception ex2)
		{
			Game1.log.Error("_loadPreferences; exception occurred trying open/read.", ex2);
			Game1.debugOutput = Game1.parseText(ex2.Message);
		}
	}

	private void readSettings(Stream stream)
	{
		StartupPreferences p = (StartupPreferences)StartupPreferences.serializer.DeserializeFast(stream);
		this.startMuted = p.startMuted;
		this.timesPlayed = p.timesPlayed + 1;
		this.levelTenCombat = p.levelTenCombat;
		this.levelTenFishing = p.levelTenFishing;
		this.levelTenForaging = p.levelTenForaging;
		this.levelTenMining = p.levelTenMining;
		this.skipWindowPreparation = p.skipWindowPreparation;
		this.windowMode = p.windowMode;
		this.displayIndex = p.displayIndex;
		this.playerLimit = p.playerLimit;
		this.gamepadMode = p.gamepadMode;
		this.fullscreenResolutionX = p.fullscreenResolutionX;
		this.fullscreenResolutionY = p.fullscreenResolutionY;
		this.lastEnteredIP = p.lastEnteredIP;
		this.languageCode = p.languageCode;
		this.clientOptions = p.clientOptions;
	}
}
