using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input;

internal static class KeyboardUtil
{
	private static Dictionary<int, Keys> _map;

	static KeyboardUtil()
	{
		KeyboardUtil._map = new Dictionary<int, Keys>();
		KeyboardUtil._map.Add(8, Keys.Back);
		KeyboardUtil._map.Add(9, Keys.Tab);
		KeyboardUtil._map.Add(13, Keys.Enter);
		KeyboardUtil._map.Add(27, Keys.Escape);
		KeyboardUtil._map.Add(32, Keys.Space);
		KeyboardUtil._map.Add(39, Keys.OemQuotes);
		KeyboardUtil._map.Add(43, Keys.Add);
		KeyboardUtil._map.Add(44, Keys.OemComma);
		KeyboardUtil._map.Add(45, Keys.OemMinus);
		KeyboardUtil._map.Add(46, Keys.OemPeriod);
		KeyboardUtil._map.Add(47, Keys.OemQuestion);
		KeyboardUtil._map.Add(48, Keys.D0);
		KeyboardUtil._map.Add(49, Keys.D1);
		KeyboardUtil._map.Add(50, Keys.D2);
		KeyboardUtil._map.Add(51, Keys.D3);
		KeyboardUtil._map.Add(52, Keys.D4);
		KeyboardUtil._map.Add(53, Keys.D5);
		KeyboardUtil._map.Add(54, Keys.D6);
		KeyboardUtil._map.Add(55, Keys.D7);
		KeyboardUtil._map.Add(56, Keys.D8);
		KeyboardUtil._map.Add(57, Keys.D9);
		KeyboardUtil._map.Add(59, Keys.OemSemicolon);
		KeyboardUtil._map.Add(60, Keys.OemBackslash);
		KeyboardUtil._map.Add(61, Keys.OemPlus);
		KeyboardUtil._map.Add(91, Keys.OemOpenBrackets);
		KeyboardUtil._map.Add(92, Keys.OemPipe);
		KeyboardUtil._map.Add(93, Keys.OemCloseBrackets);
		KeyboardUtil._map.Add(96, Keys.OemTilde);
		KeyboardUtil._map.Add(97, Keys.A);
		KeyboardUtil._map.Add(98, Keys.B);
		KeyboardUtil._map.Add(99, Keys.C);
		KeyboardUtil._map.Add(100, Keys.D);
		KeyboardUtil._map.Add(101, Keys.E);
		KeyboardUtil._map.Add(102, Keys.F);
		KeyboardUtil._map.Add(103, Keys.G);
		KeyboardUtil._map.Add(104, Keys.H);
		KeyboardUtil._map.Add(105, Keys.I);
		KeyboardUtil._map.Add(106, Keys.J);
		KeyboardUtil._map.Add(107, Keys.K);
		KeyboardUtil._map.Add(108, Keys.L);
		KeyboardUtil._map.Add(109, Keys.M);
		KeyboardUtil._map.Add(110, Keys.N);
		KeyboardUtil._map.Add(111, Keys.O);
		KeyboardUtil._map.Add(112, Keys.P);
		KeyboardUtil._map.Add(113, Keys.Q);
		KeyboardUtil._map.Add(114, Keys.R);
		KeyboardUtil._map.Add(115, Keys.S);
		KeyboardUtil._map.Add(116, Keys.T);
		KeyboardUtil._map.Add(117, Keys.U);
		KeyboardUtil._map.Add(118, Keys.V);
		KeyboardUtil._map.Add(119, Keys.W);
		KeyboardUtil._map.Add(120, Keys.X);
		KeyboardUtil._map.Add(121, Keys.Y);
		KeyboardUtil._map.Add(122, Keys.Z);
		KeyboardUtil._map.Add(127, Keys.Delete);
		KeyboardUtil._map.Add(1073741881, Keys.CapsLock);
		KeyboardUtil._map.Add(1073741882, Keys.F1);
		KeyboardUtil._map.Add(1073741883, Keys.F2);
		KeyboardUtil._map.Add(1073741884, Keys.F3);
		KeyboardUtil._map.Add(1073741885, Keys.F4);
		KeyboardUtil._map.Add(1073741886, Keys.F5);
		KeyboardUtil._map.Add(1073741887, Keys.F6);
		KeyboardUtil._map.Add(1073741888, Keys.F7);
		KeyboardUtil._map.Add(1073741889, Keys.F8);
		KeyboardUtil._map.Add(1073741890, Keys.F9);
		KeyboardUtil._map.Add(1073741891, Keys.F10);
		KeyboardUtil._map.Add(1073741892, Keys.F11);
		KeyboardUtil._map.Add(1073741893, Keys.F12);
		KeyboardUtil._map.Add(1073741894, Keys.PrintScreen);
		KeyboardUtil._map.Add(1073741895, Keys.Scroll);
		KeyboardUtil._map.Add(1073741896, Keys.Pause);
		KeyboardUtil._map.Add(1073741897, Keys.Insert);
		KeyboardUtil._map.Add(1073741898, Keys.Home);
		KeyboardUtil._map.Add(1073741899, Keys.PageUp);
		KeyboardUtil._map.Add(1073741901, Keys.End);
		KeyboardUtil._map.Add(1073741902, Keys.PageDown);
		KeyboardUtil._map.Add(1073741903, Keys.Right);
		KeyboardUtil._map.Add(1073741904, Keys.Left);
		KeyboardUtil._map.Add(1073741905, Keys.Down);
		KeyboardUtil._map.Add(1073741906, Keys.Up);
		KeyboardUtil._map.Add(1073741907, Keys.NumLock);
		KeyboardUtil._map.Add(1073741908, Keys.Divide);
		KeyboardUtil._map.Add(1073741909, Keys.Multiply);
		KeyboardUtil._map.Add(1073741910, Keys.Subtract);
		KeyboardUtil._map.Add(1073741911, Keys.Add);
		KeyboardUtil._map.Add(1073741912, Keys.Enter);
		KeyboardUtil._map.Add(1073741913, Keys.NumPad1);
		KeyboardUtil._map.Add(1073741914, Keys.NumPad2);
		KeyboardUtil._map.Add(1073741915, Keys.NumPad3);
		KeyboardUtil._map.Add(1073741916, Keys.NumPad4);
		KeyboardUtil._map.Add(1073741917, Keys.NumPad5);
		KeyboardUtil._map.Add(1073741918, Keys.NumPad6);
		KeyboardUtil._map.Add(1073741919, Keys.NumPad7);
		KeyboardUtil._map.Add(1073741920, Keys.NumPad8);
		KeyboardUtil._map.Add(1073741921, Keys.NumPad9);
		KeyboardUtil._map.Add(1073741922, Keys.NumPad0);
		KeyboardUtil._map.Add(1073741923, Keys.Decimal);
		KeyboardUtil._map.Add(1073741925, Keys.Apps);
		KeyboardUtil._map.Add(1073741928, Keys.F13);
		KeyboardUtil._map.Add(1073741929, Keys.F14);
		KeyboardUtil._map.Add(1073741930, Keys.F15);
		KeyboardUtil._map.Add(1073741931, Keys.F16);
		KeyboardUtil._map.Add(1073741932, Keys.F17);
		KeyboardUtil._map.Add(1073741933, Keys.F18);
		KeyboardUtil._map.Add(1073741934, Keys.F19);
		KeyboardUtil._map.Add(1073741935, Keys.F20);
		KeyboardUtil._map.Add(1073741936, Keys.F21);
		KeyboardUtil._map.Add(1073741937, Keys.F22);
		KeyboardUtil._map.Add(1073741938, Keys.F23);
		KeyboardUtil._map.Add(1073741939, Keys.F24);
		KeyboardUtil._map.Add(1073741951, Keys.VolumeMute);
		KeyboardUtil._map.Add(1073741952, Keys.VolumeUp);
		KeyboardUtil._map.Add(1073741953, Keys.VolumeDown);
		KeyboardUtil._map.Add(1073742040, Keys.OemClear);
		KeyboardUtil._map.Add(1073742044, Keys.Decimal);
		KeyboardUtil._map.Add(1073742048, Keys.LeftControl);
		KeyboardUtil._map.Add(1073742049, Keys.LeftShift);
		KeyboardUtil._map.Add(1073742050, Keys.LeftAlt);
		KeyboardUtil._map.Add(1073742051, Keys.LeftWindows);
		KeyboardUtil._map.Add(1073742052, Keys.RightControl);
		KeyboardUtil._map.Add(1073742053, Keys.RightShift);
		KeyboardUtil._map.Add(1073742054, Keys.RightAlt);
		KeyboardUtil._map.Add(1073742055, Keys.RightWindows);
		KeyboardUtil._map.Add(1073742082, Keys.MediaNextTrack);
		KeyboardUtil._map.Add(1073742083, Keys.MediaPreviousTrack);
		KeyboardUtil._map.Add(1073742084, Keys.MediaStop);
		KeyboardUtil._map.Add(1073742085, Keys.MediaPlayPause);
		KeyboardUtil._map.Add(1073742086, Keys.VolumeMute);
		KeyboardUtil._map.Add(1073742087, Keys.SelectMedia);
		KeyboardUtil._map.Add(1073742089, Keys.LaunchMail);
		KeyboardUtil._map.Add(1073742092, Keys.BrowserSearch);
		KeyboardUtil._map.Add(1073742093, Keys.BrowserHome);
		KeyboardUtil._map.Add(1073742094, Keys.BrowserBack);
		KeyboardUtil._map.Add(1073742095, Keys.BrowserForward);
		KeyboardUtil._map.Add(1073742096, Keys.BrowserStop);
		KeyboardUtil._map.Add(1073742097, Keys.BrowserRefresh);
		KeyboardUtil._map.Add(1073742098, Keys.BrowserFavorites);
		KeyboardUtil._map.Add(1073742106, Keys.Sleep);
	}

	public static Keys ToXna(int key)
	{
		if (KeyboardUtil._map.TryGetValue(key, out var xnaKey))
		{
			return xnaKey;
		}
		return Keys.None;
	}
}
