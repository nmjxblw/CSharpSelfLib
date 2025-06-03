using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace SpaceShared;

internal static class Util
{
	public static bool UsingMono => Type.GetType("Mono.Runtime") != null;

	public static Texture2D FetchTexture(IModRegistry modRegistry, string modIdAndPath)
	{
		if (modIdAndPath == null || modIdAndPath.IndexOf('/') == -1)
		{
			return Game1.staminaRect;
		}
		string packId = modIdAndPath.Substring(0, modIdAndPath.IndexOf('/'));
		string path = modIdAndPath.Substring(modIdAndPath.IndexOf('/') + 1);
		IModInfo modInfo = modRegistry.Get(packId);
		if (modInfo == null)
		{
			return Game1.staminaRect;
		}
		object? obj = ((object)modInfo).GetType().GetProperty("Mod")?.GetValue(modInfo);
		IMod mod = (IMod)((obj is IMod) ? obj : null);
		if (mod != null)
		{
			return mod.Helper.ModContent.Load<Texture2D>(path);
		}
		object? obj2 = ((object)modInfo).GetType().GetProperty("ContentPack")?.GetValue(modInfo);
		IContentPack pack = (IContentPack)((obj2 is IContentPack) ? obj2 : null);
		if (pack != null)
		{
			return pack.ModContent.Load<Texture2D>(path);
		}
		return Game1.staminaRect;
	}

	public static IAssetName? FetchTextureLocation(IModRegistry modRegistry, string modIdAndPath)
	{
		if (modIdAndPath == null || modIdAndPath.IndexOf('/') == -1)
		{
			return null;
		}
		string packId = modIdAndPath.Substring(0, modIdAndPath.IndexOf('/'));
		string path = modIdAndPath.Substring(modIdAndPath.IndexOf('/') + 1);
		IModInfo modInfo = modRegistry.Get(packId);
		if (modInfo == null)
		{
			return null;
		}
		object? obj = ((object)modInfo).GetType().GetProperty("Mod")?.GetValue(modInfo);
		IMod mod = (IMod)((obj is IMod) ? obj : null);
		if (mod != null)
		{
			return mod.Helper.ModContent.GetInternalAssetName(path);
		}
		object? obj2 = ((object)modInfo).GetType().GetProperty("ContentPack")?.GetValue(modInfo);
		IContentPack pack = (IContentPack)((obj2 is IContentPack) ? obj2 : null);
		if (pack != null)
		{
			return pack.ModContent.GetInternalAssetName(path);
		}
		return null;
	}

	public static string? FetchTexturePath(IModRegistry modRegistry, string modIdAndPath)
	{
		IAssetName? obj = Util.FetchTextureLocation(modRegistry, modIdAndPath);
		if (obj == null)
		{
			return null;
		}
		return obj.BaseName;
	}

	public static string FetchFullPath(IModRegistry modRegistry, string modIdAndPath)
	{
		if (modIdAndPath == null || modIdAndPath.IndexOf('/') == -1)
		{
			return null;
		}
		string packId = modIdAndPath.Substring(0, modIdAndPath.IndexOf('/'));
		string path = modIdAndPath.Substring(modIdAndPath.IndexOf('/') + 1);
		IModInfo modInfo = modRegistry.Get(packId);
		if (modInfo == null)
		{
			return null;
		}
		object? obj = ((object)modInfo).GetType().GetProperty("Mod")?.GetValue(modInfo);
		IMod mod = (IMod)((obj is IMod) ? obj : null);
		if (mod != null)
		{
			return Path.Combine(mod.Helper.DirectoryPath, path);
		}
		object? obj2 = ((object)modInfo).GetType().GetProperty("ContentPack")?.GetValue(modInfo);
		IContentPack pack = (IContentPack)((obj2 is IContentPack) ? obj2 : null);
		if (pack != null)
		{
			return Path.Combine(pack.DirectoryPath, path);
		}
		return null;
	}

	public static Texture2D DoPaletteSwap(Texture2D baseTex, Texture2D from, Texture2D to)
	{
		Color[] fromCols = (Color[])(object)new Color[from.Height];
		Color[] toCols = (Color[])(object)new Color[to.Height];
		from.GetData<Color>(fromCols);
		to.GetData<Color>(toCols);
		return Util.DoPaletteSwap(baseTex, fromCols, toCols);
	}

	public static Texture2D DoPaletteSwap(Texture2D baseTex, Color[] fromCols, Color[] toCols)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<Color, Color> colMap = new Dictionary<Color, Color>();
		for (int i = 0; i < fromCols.Length; i++)
		{
			colMap.Add(fromCols[i], toCols[i]);
		}
		Color[] cols = (Color[])(object)new Color[baseTex.Width * baseTex.Height];
		baseTex.GetData<Color>(cols);
		for (int j = 0; j < cols.Length; j++)
		{
			if (colMap.TryGetValue(cols[j], out var color))
			{
				cols[j] = color;
			}
		}
		Texture2D newTex = new Texture2D(Game1.graphics.GraphicsDevice, baseTex.Width, baseTex.Height);
		newTex.SetData<Color>(cols);
		return newTex;
	}

	public static T Clamp<T>(T min, T t, T max)
	{
		if (Comparer<T>.Default.Compare(min, t) > 0)
		{
			return min;
		}
		if (Comparer<T>.Default.Compare(max, t) < 0)
		{
			return max;
		}
		return t;
	}

	public static T Adjust<T>(T value, T interval)
	{
		if (value is float vFloat && interval is float iFloat)
		{
			value = (T)(object)(float)((decimal)vFloat - (decimal)vFloat % (decimal)iFloat);
		}
		if (value is int vInt && interval is int iInt)
		{
			value = (T)(object)(vInt - vInt % iInt);
		}
		return value;
	}

	public static void Swap<T>(ref T lhs, ref T rhs)
	{
		T temp = lhs;
		lhs = rhs;
		rhs = temp;
	}

	public static Color ColorFromHsv(double hue, double saturation, double value)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		int hi = Convert.ToInt32(Math.Floor(hue / 60.0)) % 6;
		double f = hue / 60.0 - Math.Floor(hue / 60.0);
		value *= 255.0;
		int v = Convert.ToInt32(value);
		int p = Convert.ToInt32(value * (1.0 - saturation));
		int q = Convert.ToInt32(value * (1.0 - f * saturation));
		int t = Convert.ToInt32(value * (1.0 - (1.0 - f) * saturation));
		return (Color)(hi switch
		{
			0 => new Color(v, t, p), 
			1 => new Color(q, v, p), 
			2 => new Color(p, v, t), 
			3 => new Color(p, q, v), 
			4 => new Color(t, p, v), 
			_ => new Color(v, p, q), 
		});
	}

	public static IEnumerable<Color> GetColorGradient(Color from, Color to, int totalNumberOfColors)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (totalNumberOfColors < 2)
		{
			throw new ArgumentException("Gradient cannot have less than two colors.", "totalNumberOfColors");
		}
		double diffA = ((Color)(ref to)).A - ((Color)(ref from)).A;
		double diffR = ((Color)(ref to)).R - ((Color)(ref from)).R;
		double diffG = ((Color)(ref to)).G - ((Color)(ref from)).G;
		double diffB = ((Color)(ref to)).B - ((Color)(ref from)).B;
		int steps = totalNumberOfColors - 1;
		double stepA = diffA / (double)steps;
		double stepR = diffR / (double)steps;
		double stepG = diffG / (double)steps;
		double stepB = diffB / (double)steps;
		yield return from;
		int i = 1;
		while (i < steps)
		{
			yield return new Color(c(((Color)(ref from)).R, stepR), c(((Color)(ref from)).G, stepG), c(((Color)(ref from)).B, stepB), c(((Color)(ref from)).A, stepA));
			int num = i + 1;
			i = num;
		}
		yield return to;
		int c(int fromC, double stepC)
		{
			return (int)Math.Round((double)fromC + stepC * (double)i);
		}
	}

	public static void InvokeEvent(string name, IEnumerable<Delegate> handlers, object sender)
	{
		EventArgs args = new EventArgs();
		foreach (EventHandler handler in handlers.Cast<EventHandler>())
		{
			try
			{
				handler(sender, args);
			}
			catch (Exception value)
			{
				Log.Error($"Exception while handling event {name}:\n{value}");
			}
		}
	}

	public static void InvokeEvent<T>(string name, IEnumerable<Delegate> handlers, object sender, T args)
	{
		foreach (EventHandler<T> handler in handlers.Cast<EventHandler<T>>())
		{
			try
			{
				handler(sender, args);
			}
			catch (Exception value)
			{
				Log.Error($"Exception while handling event {name}:\n{value}");
			}
		}
	}

	public static bool InvokeEventCancelable<T>(string name, IEnumerable<Delegate> handlers, object sender, T args) where T : CancelableEventArgs
	{
		foreach (EventHandler<T> handler in handlers.Cast<EventHandler<T>>())
		{
			try
			{
				handler(sender, args);
			}
			catch (Exception value)
			{
				Log.Error($"Exception while handling event {name}:\n{value}");
			}
		}
		return args.Cancel;
	}
}
