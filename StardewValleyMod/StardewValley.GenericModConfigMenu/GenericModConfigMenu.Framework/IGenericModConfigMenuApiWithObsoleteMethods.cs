using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace GenericModConfigMenu.Framework;

public interface IGenericModConfigMenuApiWithObsoleteMethods
{
	[Obsolete("Use AddComplexOption with more callback arguments instead.")]
	void AddComplexOption(IManifest mod, Func<string> name, Func<string> tooltip, Action<SpriteBatch, Vector2> draw, Action saveChanges, Func<int> height = null, string fieldId = null);

	[Obsolete("Use AddNumberOption with the formatValue callback instead.")]
	void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name, Func<string> tooltip = null, int? min = null, int? max = null, int? interval = null, string fieldId = null);

	[Obsolete("Use AddNumberOption with the formatValue callback instead.")]
	void AddNumberOption(IManifest mod, Func<float> getValue, Action<float> setValue, Func<string> name, Func<string> tooltip = null, float? min = null, float? max = null, float? interval = null, string fieldId = null);

	[Obsolete("Use AddComplexOption with more callback arguments instead.")]
	void AddComplexOption(IManifest mod, Func<string> name, Action<SpriteBatch, Vector2> draw, Func<string> tooltip = null, Action beforeSave = null, Action afterSave = null, Action beforeReset = null, Action afterReset = null, Func<int> height = null, string fieldId = null);

	[Obsolete("Use AddTextOption with the formatAllowedValue argument instead.")]
	void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name, Func<string> tooltip = null, string[] allowedValues = null, string fieldId = null);

	[Obsolete("Use Register instead.")]
	void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);

	[Obsolete("Use Unregister instead.")]
	void UnregisterModConfig(IManifest mod);

	[Obsolete("Use Register or SetTitleScreenOnlyForNextOptions instead.")]
	void SetDefaultIngameOptinValue(IManifest mod, bool optedIn);

	[Obsolete("Use AddPage instead.")]
	void StartNewPage(IManifest mod, string pageName);

	[Obsolete("Use AddPage instead.")]
	void OverridePageDisplayName(IManifest mod, string pageName, string displayName);

	[Obsolete("Use AddSectionTitle instead.")]
	void RegisterLabel(IManifest mod, string labelName, string labelDesc);

	[Obsolete("Use AddParagraph instead.")]
	void RegisterParagraph(IManifest mod, string paragraph);

	[Obsolete("Use AddPageLink instead.")]
	void RegisterPageLabel(IManifest mod, string labelName, string labelDesc, string newPage);

	[Obsolete("Use AddImage instead.")]
	void RegisterImage(IManifest mod, string texPath, Rectangle? texRect = null, int scale = 4);

	[Obsolete("Use AddBoolOption instead.")]
	void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet);

	[Obsolete("Use AddNumberOption instead.")]
	void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet);

	[Obsolete("Use AddNumberOption instead.")]
	void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet);

	[Obsolete("Use AddTextOption instead.")]
	void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet);

	[Obsolete("Use AddKeybind instead.")]
	void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet);

	[Obsolete("Use AddKeybindList instead.")]
	void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<KeybindList> optionGet, Action<KeybindList> optionSet);

	[Obsolete("Use AddNumberOption instead.")]
	void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max);

	[Obsolete("Use AddNumberOption instead.")]
	void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max);

	[Obsolete("Use AddNumberOption instead.")]
	void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max, int interval);

	[Obsolete("Use AddNumberOption instead.")]
	void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max, float interval);

	[Obsolete("Use AddTextOption instead.")]
	void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices);

	[Obsolete("Use AddComplexOption instead.")]
	void RegisterComplexOption(IManifest mod, string optionName, string optionDesc, Func<Vector2, object, object> widgetUpdate, Func<SpriteBatch, Vector2, object, object> widgetDraw, Action<object> onSave);

	[Obsolete("Use OnFieldChanged instead.")]
	void SubscribeToChange(IManifest mod, Action<string, bool> changeHandler);

	[Obsolete("Use OnFieldChanged instead.")]
	void SubscribeToChange(IManifest mod, Action<string, int> changeHandler);

	[Obsolete("Use OnFieldChanged instead.")]
	void SubscribeToChange(IManifest mod, Action<string, float> changeHandler);

	[Obsolete("Use OnFieldChanged instead.")]
	void SubscribeToChange(IManifest mod, Action<string, string> changeHandler);
}
