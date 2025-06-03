using System;
using System.Runtime.Serialization;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.LookupAnything.Framework;

internal class ModConfigKeys
{
	public KeybindList ToggleLookup { get; set; } = new KeybindList((SButton)112);

	public KeybindList ToggleSearch { get; set; } = KeybindList.Parse($"{160} + {112}");

	public KeybindList ScrollUp { get; set; } = new KeybindList((SButton)38);

	public KeybindList ScrollDown { get; set; } = new KeybindList((SButton)40);

	public KeybindList PageUp { get; set; } = new KeybindList((SButton)33);

	public KeybindList PageDown { get; set; } = new KeybindList((SButton)34);

	public KeybindList ToggleDebug { get; set; } = new KeybindList(Array.Empty<Keybind>());

	[OnDeserialized]
	public void OnDeserialized(StreamingContext context)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_001a: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0034: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_004e: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0068: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_0082: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_009c: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00b6: Expected O, but got Unknown
		if (this.ToggleLookup == null)
		{
			KeybindList val = new KeybindList(Array.Empty<Keybind>());
			KeybindList val2 = val;
			this.ToggleLookup = val;
		}
		if (this.ToggleSearch == null)
		{
			KeybindList val3 = new KeybindList(Array.Empty<Keybind>());
			KeybindList val2 = val3;
			this.ToggleSearch = val3;
		}
		if (this.ScrollUp == null)
		{
			KeybindList val4 = new KeybindList(Array.Empty<Keybind>());
			KeybindList val2 = val4;
			this.ScrollUp = val4;
		}
		if (this.ScrollDown == null)
		{
			KeybindList val5 = new KeybindList(Array.Empty<Keybind>());
			KeybindList val2 = val5;
			this.ScrollDown = val5;
		}
		if (this.PageUp == null)
		{
			KeybindList val6 = new KeybindList(Array.Empty<Keybind>());
			KeybindList val2 = val6;
			this.PageUp = val6;
		}
		if (this.PageDown == null)
		{
			KeybindList val7 = new KeybindList(Array.Empty<Keybind>());
			KeybindList val2 = val7;
			this.PageDown = val7;
		}
		if (this.ToggleDebug == null)
		{
			KeybindList val8 = new KeybindList(Array.Empty<Keybind>());
			KeybindList val2 = val8;
			this.ToggleDebug = val8;
		}
	}
}
