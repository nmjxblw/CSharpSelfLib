using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley.Extensions;

namespace GenericModConfigMenu.Framework.Overlays;

internal class KeybindEdit
{
	public int Index { get; }

	public HashSet<SButton> PressedButtons { get; } = new HashSet<SButton>();

	public bool OnlyAllowSingleButton { get; }

	public KeybindEdit(int index, bool onlyAllowSingleButton)
	{
		this.Index = index;
		this.OnlyAllowSingleButton = onlyAllowSingleButton;
	}

	public bool Any()
	{
		return this.PressedButtons.Count > 0;
	}

	public bool Add(IEnumerable<SButton> keys)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		keys = keys.Where(IsValidKey);
		if (this.OnlyAllowSingleButton)
		{
			SButton button = keys.LastOrDefault((SButton)0);
			if ((int)button == 0 || (this.PressedButtons.Count == 1 && this.PressedButtons.Contains(button)))
			{
				return false;
			}
			this.PressedButtons.Clear();
			this.PressedButtons.Add(button);
			return true;
		}
		return CollectionExtensions.AddRange<SButton>((ISet<SButton>)this.PressedButtons, keys) > 0;
	}

	public bool IsValidKey(SButton button)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Invalid comparison between Unknown and I4
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Invalid comparison between Unknown and I4
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Invalid comparison between Unknown and I4
		if ((int)button <= 33556432)
		{
			if ((int)button <= 2099152)
			{
				if ((int)button == 6096)
				{
					return false;
				}
				if ((int)button == 2099152)
				{
					goto IL_006a;
				}
			}
			else if ((int)button == 16779216 || (int)button == 33556432)
			{
				goto IL_006a;
			}
		}
		else if ((int)button <= 134219728)
		{
			if ((int)button == 67110864 || (int)button == 134219728)
			{
				goto IL_006a;
			}
		}
		else if ((int)button == 268437456 || (int)button == 536872912 || (int)button == 1073743824)
		{
			goto IL_006a;
		}
		return true;
		IL_006a:
		return false;
	}

	public Keybind ToKeybind()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		return new Keybind(this.PressedButtons.ToArray());
	}

	public override string ToString()
	{
		return string.Join(" + ", this.PressedButtons);
	}
}
