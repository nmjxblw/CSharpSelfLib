using System;
using System.Linq;
using System.Runtime.CompilerServices;
using GenericModConfigMenu.Framework.ModOption;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace GenericModConfigMenu.Framework;

public class Api : IGenericModConfigMenuApi, IGenericModConfigMenuApiWithObsoleteMethods
{
	private readonly ModConfigManager ConfigManager;

	private readonly Action<IManifest> OpenModMenuImpl;

	private readonly Action<IManifest> OpenModMenuImplChild;

	private readonly IManifest mod;

	private readonly Action<string> DeprecationWarner;

	internal Api(IManifest mod, ModConfigManager configManager, Action<IManifest> openModMenu, Action<IManifest> openModMenuChild, Action<string> DeprecationWarner)
	{
		this.mod = mod;
		this.ConfigManager = configManager;
		this.OpenModMenuImpl = openModMenu;
		this.OpenModMenuImplChild = openModMenuChild;
		this.DeprecationWarner = DeprecationWarner;
	}

	public void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = true)
	{
		if (mod == null)
		{
			mod = this.mod;
		}
		this.AssertNotNull(reset, "reset");
		this.AssertNotNull(save, "save");
		if (this.ConfigManager.Get(mod, assert: false) != null)
		{
			throw new InvalidOperationException("The '" + mod.Name + "' mod has already registered a config menu, so it can't do it again.");
		}
		if (this.mod.UniqueID != mod.UniqueID)
		{
			Log.Trace(this.mod.UniqueID + " is registering on behalf of " + mod.UniqueID);
		}
		this.ConfigManager.Set(mod, new ModConfig(mod, reset, save, titleScreenOnly));
	}

	public void AddSectionTitle(IManifest mod, Func<string> text, Func<string> tooltip = null)
	{
		if (mod == null)
		{
			mod = this.mod;
		}
		this.AssertNotNull(text, "text");
		ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
		modConfig.AddOption(new SectionTitleModOption(text, tooltip, modConfig));
	}

	public void AddParagraph(IManifest mod, Func<string> text)
	{
		if (mod == null)
		{
			mod = this.mod;
		}
		this.AssertNotNull(text, "text");
		ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
		modConfig.AddOption(new ParagraphModOption(text, modConfig));
	}

	public void AddImage(IManifest mod, Func<Texture2D> texture, Rectangle? texturePixelArea = null, int scale = 4)
	{
		if (mod == null)
		{
			mod = this.mod;
		}
		this.AssertNotNull(texture, "texture");
		ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
		modConfig.AddOption(new ImageModOption(texture, texturePixelArea, scale, modConfig));
	}

	public void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null)
	{
		this.AddSimpleOption(mod, name, tooltip, getValue, setValue, fieldId);
	}

	public void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name = null, Func<string> tooltip = null, int? min = null, int? max = null, int? interval = null, Func<int, string> formatValue = null, string fieldId = null)
	{
		this.AddNumericOption(mod, name, tooltip, getValue, setValue, min, max, interval, formatValue, fieldId);
	}

	public void AddNumberOption(IManifest mod, Func<float> getValue, Action<float> setValue, Func<string> name = null, Func<string> tooltip = null, float? min = null, float? max = null, float? interval = null, Func<float, string> formatValue = null, string fieldId = null)
	{
		this.AddNumericOption(mod, name, tooltip, getValue, setValue, min, max, interval, formatValue, fieldId);
	}

	public void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name = null, Func<string> tooltip = null, string[] allowedValues = null, Func<string, string> formatAllowedValue = null, string fieldId = null)
	{
		if (allowedValues != null && allowedValues.Any())
		{
			this.AddChoiceOption(mod, name, tooltip, getValue, setValue, allowedValues, formatAllowedValue, fieldId);
		}
		else
		{
			this.AddSimpleOption(mod, name, tooltip, getValue, setValue, fieldId);
		}
	}

	public void AddKeybind(IManifest mod, Func<SButton> getValue, Action<SButton> setValue, Func<string> name = null, Func<string> tooltip = null, string fieldId = null)
	{
		this.AddSimpleOption(mod, name, tooltip, getValue, setValue, fieldId);
	}

	public void AddKeybindList(IManifest mod, Func<KeybindList> getValue, Action<KeybindList> setValue, Func<string> name = null, Func<string> tooltip = null, string fieldId = null)
	{
		this.AddSimpleOption(mod, name, tooltip, getValue, setValue, fieldId);
	}

	public void AddPage(IManifest mod, string pageId, Func<string> pageTitle = null)
	{
		if (mod == null)
		{
			mod = this.mod;
		}
		this.AssertNotNull(pageId, "pageId");
		this.ConfigManager.Get(mod, assert: true).SetActiveRegisteringPage(pageId, pageTitle);
	}

	public void AddPageLink(IManifest mod, string pageId, Func<string> text, Func<string> tooltip = null)
	{
		if (mod == null)
		{
			mod = this.mod;
		}
		this.AssertNotNull(pageId, "pageId");
		ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
		modConfig.AddOption(new PageLinkModOption(pageId, text, tooltip, modConfig));
	}

	public void AddComplexOption(IManifest mod, Func<string> name, Action<SpriteBatch, Vector2> draw, Func<string> tooltip = null, Action beforeMenuOpened = null, Action beforeSave = null, Action afterSave = null, Action beforeReset = null, Action afterReset = null, Action beforeMenuClosed = null, Func<int> height = null, string fieldId = null)
	{
		ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
		modConfig.AddOption(new ComplexModOption(fieldId, name, tooltip, modConfig, height, draw, beforeMenuOpened, beforeSave, afterSave, beforeReset, afterReset, beforeMenuClosed));
	}

	public void SetTitleScreenOnlyForNextOptions(IManifest mod, bool titleScreenOnly)
	{
		if (mod == null)
		{
			mod = this.mod;
		}
		ModConfig config = this.ConfigManager.Get(mod, assert: true);
		config.DefaultTitleScreenOnly = titleScreenOnly;
	}

	public void OnFieldChanged(IManifest mod, Action<string, object> onChange)
	{
		if (mod == null)
		{
			mod = this.mod;
		}
		this.AssertNotNull(onChange, "onChange");
		ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
		modConfig.ChangeHandlers.Add(onChange);
	}

	public void OpenModMenu(IManifest mod)
	{
		if (mod == null)
		{
			mod = this.mod;
		}
		this.OpenModMenuImpl(mod);
	}

	public void OpenModMenuAsChildMenu(IManifest mod)
	{
		if (mod == null)
		{
			mod = this.mod;
		}
		this.OpenModMenuImplChild(mod);
	}

	public void Unregister(IManifest mod)
	{
		if (mod == null)
		{
			mod = this.mod;
		}
		this.ConfigManager.Remove(mod);
	}

	public bool TryGetCurrentMenu(out IManifest mod, out string page)
	{
		SpecificModConfigMenu menu = Mod.ActiveConfigMenu as SpecificModConfigMenu;
		if (menu == null)
		{
			menu = null;
		}
		mod = menu?.Manifest;
		page = menu?.CurrPage;
		return menu != null;
	}

	[Obsolete]
	public void AddComplexOption(IManifest mod, Func<string> name, Action<SpriteBatch, Vector2> draw, Func<string> tooltip = null, Action beforeSave = null, Action afterSave = null, Action beforeReset = null, Action afterReset = null, Func<int> height = null, string fieldId = null)
	{
		this.LogDeprecation(mod, "AddComplexOption(IManifest mod, Func<string> name, Action<SpriteBatch, Vector2> draw, Func<string> tooltip = null, Action beforeSave = null, Action afterSave = null, Action beforeReset = null, Action afterReset = null, Func<int> height = null, string fieldId = null)");
		this.AddComplexOption(mod, name, draw, tooltip, null, beforeSave, afterSave, beforeReset, afterReset, null, height, fieldId);
	}

	[Obsolete]
	public void AddComplexOption(IManifest mod, Func<string> name, Func<string> tooltip, Action<SpriteBatch, Vector2> draw, Action saveChanges, Func<int> height = null, string fieldId = null)
	{
		this.LogDeprecation(mod, "AddChoiceOption(IManifest mod, Func<string> name, Func<string> tooltip, Action<SpriteBatch, Vector2> draw, Action saveChanges, Func<int> height = null, string fieldId = null)");
		this.AddComplexOption(mod, name, draw, tooltip, null, saveChanges, null, null, null, null, height, fieldId);
	}

	[Obsolete]
	public void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name = null, Func<string> tooltip = null, int? min = null, int? max = null, int? interval = null, string fieldId = null)
	{
		this.LogDeprecation(mod, "AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name = null, Func<string> tooltip = null, int? min = null, int? max = null, int? interval = null, string fieldId = null)");
		this.AddNumericOption(mod, name, tooltip, getValue, setValue, min, max, interval, null, fieldId);
	}

	[Obsolete]
	public void AddNumberOption(IManifest mod, Func<float> getValue, Action<float> setValue, Func<string> name = null, Func<string> tooltip = null, float? min = null, float? max = null, float? interval = null, string fieldId = null)
	{
		this.LogDeprecation(mod, "AddNumberOption(IManifest mod, Func<float> getValue, Action<float> setValue, Func<string> name = null, Func<string> tooltip = null, float? min = null, float? max = null, float? interval = null, string fieldId = null)");
		this.AddNumericOption(mod, name, tooltip, getValue, setValue, min, max, interval, null, fieldId);
	}

	[Obsolete]
	public void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name = null, Func<string> tooltip = null, string[] allowedValues = null, string fieldId = null)
	{
		this.LogDeprecation(mod, "AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name = null, Func<string> tooltip = null, string[] allowedValues = null, string fieldId = null)");
		this.AddTextOption(mod, getValue, setValue, name, tooltip, allowedValues, null, fieldId);
	}

	[Obsolete]
	public void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile)
	{
		this.LogDeprecation(mod, "RegisterModConfig");
		this.Register(mod, revertToDefault, saveToFile);
	}

	[Obsolete]
	public void UnregisterModConfig(IManifest mod)
	{
		this.LogDeprecation(mod, "UnregisterModConfig");
		this.Unregister(mod);
	}

	[Obsolete]
	public void SetDefaultIngameOptinValue(IManifest mod, bool optedIn)
	{
		this.LogDeprecation(mod, "SetDefaultIngameOptinValue");
		this.SetTitleScreenOnlyForNextOptions(mod, !optedIn);
	}

	[Obsolete]
	public void StartNewPage(IManifest mod, string pageName)
	{
		this.LogDeprecation(mod, "StartNewPage");
		this.AddPage(mod, pageName, () => pageName);
	}

	[Obsolete]
	public void OverridePageDisplayName(IManifest mod, string pageName, string displayName)
	{
		this.LogDeprecation(mod, "OverridePageDisplayName");
		if (mod == null)
		{
			mod = this.mod;
		}
		ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
		ModConfigPage page = modConfig.Pages.GetOrDefault(pageName) ?? throw new ArgumentException("Page not registered");
		page.SetPageTitle(() => displayName);
	}

	[Obsolete]
	public void RegisterLabel(IManifest mod, string labelName, string labelDesc)
	{
		this.LogDeprecation(mod, "RegisterLabel");
		this.AddSectionTitle(mod, () => labelName, () => labelDesc);
	}

	[Obsolete]
	public void RegisterPageLabel(IManifest mod, string labelName, string labelDesc, string newPage)
	{
		this.LogDeprecation(mod, "RegisterPageLabel");
		this.AddPageLink(mod, newPage, () => labelName, () => labelDesc);
	}

	[Obsolete]
	public void RegisterParagraph(IManifest mod, string paragraph)
	{
		this.LogDeprecation(mod, "RegisterParagraph");
		this.AddParagraph(mod, () => paragraph);
	}

	[Obsolete]
	public void RegisterImage(IManifest mod, string texPath, Rectangle? texRect = null, int scale = 4)
	{
		this.LogDeprecation(mod, "RegisterImage");
		this.AddImage(mod, () => Game1.content.Load<Texture2D>(texPath), texRect, scale);
	}

	[Obsolete]
	public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet)
	{
		this.LogDeprecation(mod, "RegisterSimpleOption");
		this.AddBoolOption(mod, fieldId: optionName, getValue: optionGet, setValue: optionSet, name: () => optionName, tooltip: () => optionDesc);
	}

	[Obsolete]
	public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet)
	{
		this.LogDeprecation(mod, "RegisterSimpleOption");
		Func<string> name = () => optionName;
		Func<string> tooltip = () => optionDesc;
		string fieldId = optionName;
		this.AddNumericOption(mod, name, tooltip, optionGet, optionSet, null, null, null, null, fieldId);
	}

	[Obsolete]
	public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet)
	{
		this.LogDeprecation(mod, "RegisterSimpleOption");
		Func<string> name = () => optionName;
		Func<string> tooltip = () => optionDesc;
		string fieldId = optionName;
		this.AddNumericOption(mod, name, tooltip, optionGet, optionSet, null, null, null, null, fieldId);
	}

	[Obsolete]
	public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet)
	{
		this.LogDeprecation(mod, "RegisterSimpleOption");
		this.AddTextOption(mod, fieldId: optionName, getValue: optionGet, setValue: optionSet, name: () => optionName, tooltip: () => optionDesc);
	}

	[Obsolete]
	public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet)
	{
		this.LogDeprecation(mod, "RegisterSimpleOption");
		this.AddKeybind(mod, fieldId: optionName, getValue: optionGet, setValue: optionSet, name: () => optionName, tooltip: () => optionDesc);
	}

	[Obsolete]
	public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<KeybindList> optionGet, Action<KeybindList> optionSet)
	{
		this.LogDeprecation(mod, "RegisterSimpleOption");
		this.AddKeybindList(mod, fieldId: optionName, getValue: optionGet, setValue: optionSet, name: () => optionName, tooltip: () => optionDesc);
	}

	[Obsolete]
	public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max)
	{
		this.LogDeprecation(mod, "RegisterClampedOption");
		this.AddNumericOption<int>(mod, () => optionName, () => optionDesc, fieldId: optionName, getValue: optionGet, setValue: optionSet, min: min, max: max, interval: null, formatValue: null);
	}

	[Obsolete]
	public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max)
	{
		this.LogDeprecation(mod, "RegisterClampedOption");
		this.AddNumericOption<float>(mod, () => optionName, () => optionDesc, fieldId: optionName, getValue: optionGet, setValue: optionSet, min: min, max: max, interval: null, formatValue: null);
	}

	[Obsolete]
	public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max, int interval)
	{
		this.LogDeprecation(mod, "RegisterClampedOption");
		this.AddNumericOption<int>(mod, () => optionName, () => optionDesc, fieldId: optionName, getValue: optionGet, setValue: optionSet, min: min, max: max, interval: interval, formatValue: null);
	}

	[Obsolete]
	public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max, float interval)
	{
		this.LogDeprecation(mod, "RegisterClampedOption");
		this.AddNumericOption<float>(mod, () => optionName, () => optionDesc, fieldId: optionName, getValue: optionGet, setValue: optionSet, min: min, max: max, interval: interval, formatValue: null);
	}

	[Obsolete]
	public void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices)
	{
		this.LogDeprecation(mod, "RegisterChoiceOption");
		this.AddTextOption(mod, fieldId: optionName, getValue: optionGet, setValue: optionSet, name: () => optionName, tooltip: () => optionDesc, allowedValues: choices);
	}

	[Obsolete]
	public void RegisterComplexOption(IManifest mod, string optionName, string optionDesc, Func<Vector2, object, object> widgetUpdate, Func<SpriteBatch, Vector2, object, object> widgetDraw, Action<object> onSave)
	{
		this.LogDeprecation(mod, "RegisterComplexOption");
		if (mod == null)
		{
			mod = this.mod;
		}
		this.AssertNotNull(widgetUpdate, "widgetUpdate");
		this.AssertNotNull(widgetDraw, "widgetDraw");
		this.AssertNotNull(onSave, "onSave");
		object state = null;
		this.AddComplexOption(mod, () => optionName, () => optionDesc, fieldId: optionName, draw: Draw, saveChanges: Save);
		void Draw(SpriteBatch spriteBatch, Vector2 position)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			state = widgetUpdate(position, state);
			state = widgetDraw(spriteBatch, position, state);
		}
		void Save()
		{
			onSave(state);
		}
	}

	[Obsolete]
	public void SubscribeToChange(IManifest mod, Action<string, bool> changeHandler)
	{
		this.LogDeprecation(mod, "SubscribeToChange(IManifest mod, Action<string, bool> changeHandler)");
		this.SubscribeToChange<bool>(mod, changeHandler);
	}

	[Obsolete]
	public void SubscribeToChange(IManifest mod, Action<string, int> changeHandler)
	{
		this.LogDeprecation(mod, "SubscribeToChange(IManifest mod, Action<string, int> changeHandler)");
		this.SubscribeToChange<int>(mod, changeHandler);
	}

	[Obsolete]
	public void SubscribeToChange(IManifest mod, Action<string, float> changeHandler)
	{
		this.LogDeprecation(mod, "SubscribeToChange(IManifest mod, Action<string, float> changeHandler)");
		this.SubscribeToChange<float>(mod, changeHandler);
	}

	[Obsolete]
	public void SubscribeToChange(IManifest mod, Action<string, string> changeHandler)
	{
		this.LogDeprecation(mod, "SubscribeToChange(IManifest mod, Action<string, string> changeHandler)");
		this.SubscribeToChange<string>(mod, changeHandler);
	}

	private void AddSimpleOption<T>(IManifest mod, Func<string> name, Func<string> tooltip, Func<T> getValue, Action<T> setValue, string fieldId)
	{
		if (mod == null)
		{
			mod = this.mod;
		}
		this.AssertNotNull(name, "name");
		this.AssertNotNull(getValue, "getValue");
		this.AssertNotNull(setValue, "setValue");
		ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
		Type[] valid = new Type[6]
		{
			typeof(bool),
			typeof(int),
			typeof(float),
			typeof(string),
			typeof(SButton),
			typeof(KeybindList)
		};
		if (!valid.Contains(typeof(T)))
		{
			throw new ArgumentException("Invalid config option type.");
		}
		modConfig.AddOption(new SimpleModOption<T>(fieldId, name, tooltip, modConfig, getValue, setValue));
	}

	private void AddNumericOption<T>(IManifest mod, Func<string> name, Func<string> tooltip, Func<T> getValue, Action<T> setValue, T? min, T? max, T? interval, Func<T, string> formatValue, string fieldId) where T : struct
	{
		if (mod == null)
		{
			mod = this.mod;
		}
		this.AssertNotNull(name, "name");
		this.AssertNotNull(getValue, "getValue");
		this.AssertNotNull(setValue, "setValue");
		ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
		Type[] valid = new Type[2]
		{
			typeof(int),
			typeof(float)
		};
		if (!valid.Contains(typeof(T)))
		{
			throw new ArgumentException("Invalid config option type.");
		}
		modConfig.AddOption(new NumericModOption<T>(fieldId, name, tooltip, modConfig, getValue, setValue, min, max, interval, formatValue));
	}

	private void AddChoiceOption(IManifest mod, Func<string> name, Func<string> tooltip, Func<string> getValue, Action<string> setValue, string[] allowedValues, Func<string, string> formatAllowedValues, string fieldId)
	{
		if (mod == null)
		{
			mod = this.mod;
		}
		this.AssertNotNull(name, "name");
		this.AssertNotNull(getValue, "getValue");
		this.AssertNotNull(setValue, "setValue");
		if (name == null)
		{
			name = () => fieldId;
		}
		ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
		modConfig.AddOption(new ChoiceModOption<string>(fieldId, name, tooltip, modConfig, getValue, setValue, allowedValues, formatAllowedValues));
	}

	[Obsolete("This only exists to support obsolete methods.")]
	private void SubscribeToChange<TValue>(IManifest mod, Action<string, TValue> changeHandler)
	{
		this.AssertNotNull(changeHandler, "changeHandler");
		this.OnFieldChanged(mod, delegate(string fieldId, object rawValue)
		{
			if (rawValue is TValue arg)
			{
				changeHandler(fieldId, arg);
			}
		});
	}

	private void AssertNotNull(object value, [CallerArgumentExpression("value")] string paramName = "")
	{
		if (value == null)
		{
			throw new ArgumentNullException(paramName);
		}
	}

	private void AssertNotNullOrWhitespace(string value, [CallerArgumentExpression("value")] string paramName = "")
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentNullException(paramName);
		}
	}

	private void LogDeprecation(IManifest client, string method)
	{
		this.DeprecationWarner($"{this.mod.UniqueID} (registering for {client.UniqueID}) is using deprecated code ({method}) that will break in a future version of GMCM.");
	}
}
