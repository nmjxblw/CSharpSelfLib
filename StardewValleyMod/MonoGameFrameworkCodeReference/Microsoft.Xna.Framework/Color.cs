using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Describes a 32-bit packed color.
/// </summary>
[DataContract]
[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct Color : IEquatable<Color>
{
	private uint _packedValue;

	/// <summary>
	/// Gets or sets the blue component.
	/// </summary>
	[DataMember]
	public byte B
	{
		get
		{
			return (byte)(this._packedValue >> 16);
		}
		set
		{
			this._packedValue = (this._packedValue & 0xFF00FFFFu) | (uint)(value << 16);
		}
	}

	/// <summary>
	/// Gets or sets the green component.
	/// </summary>
	[DataMember]
	public byte G
	{
		get
		{
			return (byte)(this._packedValue >> 8);
		}
		set
		{
			this._packedValue = (this._packedValue & 0xFFFF00FFu) | (uint)(value << 8);
		}
	}

	/// <summary>
	/// Gets or sets the red component.
	/// </summary>
	[DataMember]
	public byte R
	{
		get
		{
			return (byte)this._packedValue;
		}
		set
		{
			this._packedValue = (this._packedValue & 0xFFFFFF00u) | value;
		}
	}

	/// <summary>
	/// Gets or sets the alpha component.
	/// </summary>
	[DataMember]
	public byte A
	{
		get
		{
			return (byte)(this._packedValue >> 24);
		}
		set
		{
			this._packedValue = (this._packedValue & 0xFFFFFF) | (uint)(value << 24);
		}
	}

	/// <summary>
	/// TransparentBlack color (R:0,G:0,B:0,A:0).
	/// </summary>
	[Obsolete("Use Color.Transparent instead. In future versions this method can be removed.")]
	public static Color TransparentBlack { get; private set; }

	/// <summary>
	/// Transparent color (R:0,G:0,B:0,A:0).
	/// </summary>
	public static Color Transparent { get; private set; }

	/// <summary>
	/// AliceBlue color (R:240,G:248,B:255,A:255).
	/// </summary>
	public static Color AliceBlue { get; private set; }

	/// <summary>
	/// AntiqueWhite color (R:250,G:235,B:215,A:255).
	/// </summary>
	public static Color AntiqueWhite { get; private set; }

	/// <summary>
	/// Aqua color (R:0,G:255,B:255,A:255).
	/// </summary>
	public static Color Aqua { get; private set; }

	/// <summary>
	/// Aquamarine color (R:127,G:255,B:212,A:255).
	/// </summary>
	public static Color Aquamarine { get; private set; }

	/// <summary>
	/// Azure color (R:240,G:255,B:255,A:255).
	/// </summary>
	public static Color Azure { get; private set; }

	/// <summary>
	/// Beige color (R:245,G:245,B:220,A:255).
	/// </summary>
	public static Color Beige { get; private set; }

	/// <summary>
	/// Bisque color (R:255,G:228,B:196,A:255).
	/// </summary>
	public static Color Bisque { get; private set; }

	/// <summary>
	/// Black color (R:0,G:0,B:0,A:255).
	/// </summary>
	public static Color Black { get; private set; }

	/// <summary>
	/// BlanchedAlmond color (R:255,G:235,B:205,A:255).
	/// </summary>
	public static Color BlanchedAlmond { get; private set; }

	/// <summary>
	/// Blue color (R:0,G:0,B:255,A:255).
	/// </summary>
	public static Color Blue { get; private set; }

	/// <summary>
	/// BlueViolet color (R:138,G:43,B:226,A:255).
	/// </summary>
	public static Color BlueViolet { get; private set; }

	/// <summary>
	/// Brown color (R:165,G:42,B:42,A:255).
	/// </summary>
	public static Color Brown { get; private set; }

	/// <summary>
	/// BurlyWood color (R:222,G:184,B:135,A:255).
	/// </summary>
	public static Color BurlyWood { get; private set; }

	/// <summary>
	/// CadetBlue color (R:95,G:158,B:160,A:255).
	/// </summary>
	public static Color CadetBlue { get; private set; }

	/// <summary>
	/// Chartreuse color (R:127,G:255,B:0,A:255).
	/// </summary>
	public static Color Chartreuse { get; private set; }

	/// <summary>
	/// Chocolate color (R:210,G:105,B:30,A:255).
	/// </summary>
	public static Color Chocolate { get; private set; }

	/// <summary>
	/// Coral color (R:255,G:127,B:80,A:255).
	/// </summary>
	public static Color Coral { get; private set; }

	/// <summary>
	/// CornflowerBlue color (R:100,G:149,B:237,A:255).
	/// </summary>
	public static Color CornflowerBlue { get; private set; }

	/// <summary>
	/// Cornsilk color (R:255,G:248,B:220,A:255).
	/// </summary>
	public static Color Cornsilk { get; private set; }

	/// <summary>
	/// Crimson color (R:220,G:20,B:60,A:255).
	/// </summary>
	public static Color Crimson { get; private set; }

	/// <summary>
	/// Cyan color (R:0,G:255,B:255,A:255).
	/// </summary>
	public static Color Cyan { get; private set; }

	/// <summary>
	/// DarkBlue color (R:0,G:0,B:139,A:255).
	/// </summary>
	public static Color DarkBlue { get; private set; }

	/// <summary>
	/// DarkCyan color (R:0,G:139,B:139,A:255).
	/// </summary>
	public static Color DarkCyan { get; private set; }

	/// <summary>
	/// DarkGoldenrod color (R:184,G:134,B:11,A:255).
	/// </summary>
	public static Color DarkGoldenrod { get; private set; }

	/// <summary>
	/// DarkGray color (R:169,G:169,B:169,A:255).
	/// </summary>
	public static Color DarkGray { get; private set; }

	/// <summary>
	/// DarkGreen color (R:0,G:100,B:0,A:255).
	/// </summary>
	public static Color DarkGreen { get; private set; }

	/// <summary>
	/// DarkKhaki color (R:189,G:183,B:107,A:255).
	/// </summary>
	public static Color DarkKhaki { get; private set; }

	/// <summary>
	/// DarkMagenta color (R:139,G:0,B:139,A:255).
	/// </summary>
	public static Color DarkMagenta { get; private set; }

	/// <summary>
	/// DarkOliveGreen color (R:85,G:107,B:47,A:255).
	/// </summary>
	public static Color DarkOliveGreen { get; private set; }

	/// <summary>
	/// DarkOrange color (R:255,G:140,B:0,A:255).
	/// </summary>
	public static Color DarkOrange { get; private set; }

	/// <summary>
	/// DarkOrchid color (R:153,G:50,B:204,A:255).
	/// </summary>
	public static Color DarkOrchid { get; private set; }

	/// <summary>
	/// DarkRed color (R:139,G:0,B:0,A:255).
	/// </summary>
	public static Color DarkRed { get; private set; }

	/// <summary>
	/// DarkSalmon color (R:233,G:150,B:122,A:255).
	/// </summary>
	public static Color DarkSalmon { get; private set; }

	/// <summary>
	/// DarkSeaGreen color (R:143,G:188,B:139,A:255).
	/// </summary>
	public static Color DarkSeaGreen { get; private set; }

	/// <summary>
	/// DarkSlateBlue color (R:72,G:61,B:139,A:255).
	/// </summary>
	public static Color DarkSlateBlue { get; private set; }

	/// <summary>
	/// DarkSlateGray color (R:47,G:79,B:79,A:255).
	/// </summary>
	public static Color DarkSlateGray { get; private set; }

	/// <summary>
	/// DarkTurquoise color (R:0,G:206,B:209,A:255).
	/// </summary>
	public static Color DarkTurquoise { get; private set; }

	/// <summary>
	/// DarkViolet color (R:148,G:0,B:211,A:255).
	/// </summary>
	public static Color DarkViolet { get; private set; }

	/// <summary>
	/// DeepPink color (R:255,G:20,B:147,A:255).
	/// </summary>
	public static Color DeepPink { get; private set; }

	/// <summary>
	/// DeepSkyBlue color (R:0,G:191,B:255,A:255).
	/// </summary>
	public static Color DeepSkyBlue { get; private set; }

	/// <summary>
	/// DimGray color (R:105,G:105,B:105,A:255).
	/// </summary>
	public static Color DimGray { get; private set; }

	/// <summary>
	/// DodgerBlue color (R:30,G:144,B:255,A:255).
	/// </summary>
	public static Color DodgerBlue { get; private set; }

	/// <summary>
	/// Firebrick color (R:178,G:34,B:34,A:255).
	/// </summary>
	public static Color Firebrick { get; private set; }

	/// <summary>
	/// FloralWhite color (R:255,G:250,B:240,A:255).
	/// </summary>
	public static Color FloralWhite { get; private set; }

	/// <summary>
	/// ForestGreen color (R:34,G:139,B:34,A:255).
	/// </summary>
	public static Color ForestGreen { get; private set; }

	/// <summary>
	/// Fuchsia color (R:255,G:0,B:255,A:255).
	/// </summary>
	public static Color Fuchsia { get; private set; }

	/// <summary>
	/// Gainsboro color (R:220,G:220,B:220,A:255).
	/// </summary>
	public static Color Gainsboro { get; private set; }

	/// <summary>
	/// GhostWhite color (R:248,G:248,B:255,A:255).
	/// </summary>
	public static Color GhostWhite { get; private set; }

	/// <summary>
	/// Gold color (R:255,G:215,B:0,A:255).
	/// </summary>
	public static Color Gold { get; private set; }

	/// <summary>
	/// Goldenrod color (R:218,G:165,B:32,A:255).
	/// </summary>
	public static Color Goldenrod { get; private set; }

	/// <summary>
	/// Gray color (R:128,G:128,B:128,A:255).
	/// </summary>
	public static Color Gray { get; private set; }

	/// <summary>
	/// Green color (R:0,G:128,B:0,A:255).
	/// </summary>
	public static Color Green { get; private set; }

	/// <summary>
	/// GreenYellow color (R:173,G:255,B:47,A:255).
	/// </summary>
	public static Color GreenYellow { get; private set; }

	/// <summary>
	/// Honeydew color (R:240,G:255,B:240,A:255).
	/// </summary>
	public static Color Honeydew { get; private set; }

	/// <summary>
	/// HotPink color (R:255,G:105,B:180,A:255).
	/// </summary>
	public static Color HotPink { get; private set; }

	/// <summary>
	/// IndianRed color (R:205,G:92,B:92,A:255).
	/// </summary>
	public static Color IndianRed { get; private set; }

	/// <summary>
	/// Indigo color (R:75,G:0,B:130,A:255).
	/// </summary>
	public static Color Indigo { get; private set; }

	/// <summary>
	/// Ivory color (R:255,G:255,B:240,A:255).
	/// </summary>
	public static Color Ivory { get; private set; }

	/// <summary>
	/// Khaki color (R:240,G:230,B:140,A:255).
	/// </summary>
	public static Color Khaki { get; private set; }

	/// <summary>
	/// Lavender color (R:230,G:230,B:250,A:255).
	/// </summary>
	public static Color Lavender { get; private set; }

	/// <summary>
	/// LavenderBlush color (R:255,G:240,B:245,A:255).
	/// </summary>
	public static Color LavenderBlush { get; private set; }

	/// <summary>
	/// LawnGreen color (R:124,G:252,B:0,A:255).
	/// </summary>
	public static Color LawnGreen { get; private set; }

	/// <summary>
	/// LemonChiffon color (R:255,G:250,B:205,A:255).
	/// </summary>
	public static Color LemonChiffon { get; private set; }

	/// <summary>
	/// LightBlue color (R:173,G:216,B:230,A:255).
	/// </summary>
	public static Color LightBlue { get; private set; }

	/// <summary>
	/// LightCoral color (R:240,G:128,B:128,A:255).
	/// </summary>
	public static Color LightCoral { get; private set; }

	/// <summary>
	/// LightCyan color (R:224,G:255,B:255,A:255).
	/// </summary>
	public static Color LightCyan { get; private set; }

	/// <summary>
	/// LightGoldenrodYellow color (R:250,G:250,B:210,A:255).
	/// </summary>
	public static Color LightGoldenrodYellow { get; private set; }

	/// <summary>
	/// LightGray color (R:211,G:211,B:211,A:255).
	/// </summary>
	public static Color LightGray { get; private set; }

	/// <summary>
	/// LightGreen color (R:144,G:238,B:144,A:255).
	/// </summary>
	public static Color LightGreen { get; private set; }

	/// <summary>
	/// LightPink color (R:255,G:182,B:193,A:255).
	/// </summary>
	public static Color LightPink { get; private set; }

	/// <summary>
	/// LightSalmon color (R:255,G:160,B:122,A:255).
	/// </summary>
	public static Color LightSalmon { get; private set; }

	/// <summary>
	/// LightSeaGreen color (R:32,G:178,B:170,A:255).
	/// </summary>
	public static Color LightSeaGreen { get; private set; }

	/// <summary>
	/// LightSkyBlue color (R:135,G:206,B:250,A:255).
	/// </summary>
	public static Color LightSkyBlue { get; private set; }

	/// <summary>
	/// LightSlateGray color (R:119,G:136,B:153,A:255).
	/// </summary>
	public static Color LightSlateGray { get; private set; }

	/// <summary>
	/// LightSteelBlue color (R:176,G:196,B:222,A:255).
	/// </summary>
	public static Color LightSteelBlue { get; private set; }

	/// <summary>
	/// LightYellow color (R:255,G:255,B:224,A:255).
	/// </summary>
	public static Color LightYellow { get; private set; }

	/// <summary>
	/// Lime color (R:0,G:255,B:0,A:255).
	/// </summary>
	public static Color Lime { get; private set; }

	/// <summary>
	/// LimeGreen color (R:50,G:205,B:50,A:255).
	/// </summary>
	public static Color LimeGreen { get; private set; }

	/// <summary>
	/// Linen color (R:250,G:240,B:230,A:255).
	/// </summary>
	public static Color Linen { get; private set; }

	/// <summary>
	/// Magenta color (R:255,G:0,B:255,A:255).
	/// </summary>
	public static Color Magenta { get; private set; }

	/// <summary>
	/// Maroon color (R:128,G:0,B:0,A:255).
	/// </summary>
	public static Color Maroon { get; private set; }

	/// <summary>
	/// MediumAquamarine color (R:102,G:205,B:170,A:255).
	/// </summary>
	public static Color MediumAquamarine { get; private set; }

	/// <summary>
	/// MediumBlue color (R:0,G:0,B:205,A:255).
	/// </summary>
	public static Color MediumBlue { get; private set; }

	/// <summary>
	/// MediumOrchid color (R:186,G:85,B:211,A:255).
	/// </summary>
	public static Color MediumOrchid { get; private set; }

	/// <summary>
	/// MediumPurple color (R:147,G:112,B:219,A:255).
	/// </summary>
	public static Color MediumPurple { get; private set; }

	/// <summary>
	/// MediumSeaGreen color (R:60,G:179,B:113,A:255).
	/// </summary>
	public static Color MediumSeaGreen { get; private set; }

	/// <summary>
	/// MediumSlateBlue color (R:123,G:104,B:238,A:255).
	/// </summary>
	public static Color MediumSlateBlue { get; private set; }

	/// <summary>
	/// MediumSpringGreen color (R:0,G:250,B:154,A:255).
	/// </summary>
	public static Color MediumSpringGreen { get; private set; }

	/// <summary>
	/// MediumTurquoise color (R:72,G:209,B:204,A:255).
	/// </summary>
	public static Color MediumTurquoise { get; private set; }

	/// <summary>
	/// MediumVioletRed color (R:199,G:21,B:133,A:255).
	/// </summary>
	public static Color MediumVioletRed { get; private set; }

	/// <summary>
	/// MidnightBlue color (R:25,G:25,B:112,A:255).
	/// </summary>
	public static Color MidnightBlue { get; private set; }

	/// <summary>
	/// MintCream color (R:245,G:255,B:250,A:255).
	/// </summary>
	public static Color MintCream { get; private set; }

	/// <summary>
	/// MistyRose color (R:255,G:228,B:225,A:255).
	/// </summary>
	public static Color MistyRose { get; private set; }

	/// <summary>
	/// Moccasin color (R:255,G:228,B:181,A:255).
	/// </summary>
	public static Color Moccasin { get; private set; }

	/// <summary>
	/// MonoGame orange theme color (R:231,G:60,B:0,A:255).
	/// </summary>
	public static Color MonoGameOrange { get; private set; }

	/// <summary>
	/// NavajoWhite color (R:255,G:222,B:173,A:255).
	/// </summary>
	public static Color NavajoWhite { get; private set; }

	/// <summary>
	/// Navy color (R:0,G:0,B:128,A:255).
	/// </summary>
	public static Color Navy { get; private set; }

	/// <summary>
	/// OldLace color (R:253,G:245,B:230,A:255).
	/// </summary>
	public static Color OldLace { get; private set; }

	/// <summary>
	/// Olive color (R:128,G:128,B:0,A:255).
	/// </summary>
	public static Color Olive { get; private set; }

	/// <summary>
	/// OliveDrab color (R:107,G:142,B:35,A:255).
	/// </summary>
	public static Color OliveDrab { get; private set; }

	/// <summary>
	/// Orange color (R:255,G:165,B:0,A:255).
	/// </summary>
	public static Color Orange { get; private set; }

	/// <summary>
	/// OrangeRed color (R:255,G:69,B:0,A:255).
	/// </summary>
	public static Color OrangeRed { get; private set; }

	/// <summary>
	/// Orchid color (R:218,G:112,B:214,A:255).
	/// </summary>
	public static Color Orchid { get; private set; }

	/// <summary>
	/// PaleGoldenrod color (R:238,G:232,B:170,A:255).
	/// </summary>
	public static Color PaleGoldenrod { get; private set; }

	/// <summary>
	/// PaleGreen color (R:152,G:251,B:152,A:255).
	/// </summary>
	public static Color PaleGreen { get; private set; }

	/// <summary>
	/// PaleTurquoise color (R:175,G:238,B:238,A:255).
	/// </summary>
	public static Color PaleTurquoise { get; private set; }

	/// <summary>
	/// PaleVioletRed color (R:219,G:112,B:147,A:255).
	/// </summary>
	public static Color PaleVioletRed { get; private set; }

	/// <summary>
	/// PapayaWhip color (R:255,G:239,B:213,A:255).
	/// </summary>
	public static Color PapayaWhip { get; private set; }

	/// <summary>
	/// PeachPuff color (R:255,G:218,B:185,A:255).
	/// </summary>
	public static Color PeachPuff { get; private set; }

	/// <summary>
	/// Peru color (R:205,G:133,B:63,A:255).
	/// </summary>
	public static Color Peru { get; private set; }

	/// <summary>
	/// Pink color (R:255,G:192,B:203,A:255).
	/// </summary>
	public static Color Pink { get; private set; }

	/// <summary>
	/// Plum color (R:221,G:160,B:221,A:255).
	/// </summary>
	public static Color Plum { get; private set; }

	/// <summary>
	/// PowderBlue color (R:176,G:224,B:230,A:255).
	/// </summary>
	public static Color PowderBlue { get; private set; }

	/// <summary>
	///  Purple color (R:128,G:0,B:128,A:255).
	/// </summary>
	public static Color Purple { get; private set; }

	/// <summary>
	/// Red color (R:255,G:0,B:0,A:255).
	/// </summary>
	public static Color Red { get; private set; }

	/// <summary>
	/// RosyBrown color (R:188,G:143,B:143,A:255).
	/// </summary>
	public static Color RosyBrown { get; private set; }

	/// <summary>
	/// RoyalBlue color (R:65,G:105,B:225,A:255).
	/// </summary>
	public static Color RoyalBlue { get; private set; }

	/// <summary>
	/// SaddleBrown color (R:139,G:69,B:19,A:255).
	/// </summary>
	public static Color SaddleBrown { get; private set; }

	/// <summary>
	/// Salmon color (R:250,G:128,B:114,A:255).
	/// </summary>
	public static Color Salmon { get; private set; }

	/// <summary>
	/// SandyBrown color (R:244,G:164,B:96,A:255).
	/// </summary>
	public static Color SandyBrown { get; private set; }

	/// <summary>
	/// SeaGreen color (R:46,G:139,B:87,A:255).
	/// </summary>
	public static Color SeaGreen { get; private set; }

	/// <summary>
	/// SeaShell color (R:255,G:245,B:238,A:255).
	/// </summary>
	public static Color SeaShell { get; private set; }

	/// <summary>
	/// Sienna color (R:160,G:82,B:45,A:255).
	/// </summary>
	public static Color Sienna { get; private set; }

	/// <summary>
	/// Silver color (R:192,G:192,B:192,A:255).
	/// </summary>
	public static Color Silver { get; private set; }

	/// <summary>
	/// SkyBlue color (R:135,G:206,B:235,A:255).
	/// </summary>
	public static Color SkyBlue { get; private set; }

	/// <summary>
	/// SlateBlue color (R:106,G:90,B:205,A:255).
	/// </summary>
	public static Color SlateBlue { get; private set; }

	/// <summary>
	/// SlateGray color (R:112,G:128,B:144,A:255).
	/// </summary>
	public static Color SlateGray { get; private set; }

	/// <summary>
	/// Snow color (R:255,G:250,B:250,A:255).
	/// </summary>
	public static Color Snow { get; private set; }

	/// <summary>
	/// SpringGreen color (R:0,G:255,B:127,A:255).
	/// </summary>
	public static Color SpringGreen { get; private set; }

	/// <summary>
	/// SteelBlue color (R:70,G:130,B:180,A:255).
	/// </summary>
	public static Color SteelBlue { get; private set; }

	/// <summary>
	/// Tan color (R:210,G:180,B:140,A:255).
	/// </summary>
	public static Color Tan { get; private set; }

	/// <summary>
	/// Teal color (R:0,G:128,B:128,A:255).
	/// </summary>
	public static Color Teal { get; private set; }

	/// <summary>
	/// Thistle color (R:216,G:191,B:216,A:255).
	/// </summary>
	public static Color Thistle { get; private set; }

	/// <summary>
	/// Tomato color (R:255,G:99,B:71,A:255).
	/// </summary>
	public static Color Tomato { get; private set; }

	/// <summary>
	/// Turquoise color (R:64,G:224,B:208,A:255).
	/// </summary>
	public static Color Turquoise { get; private set; }

	/// <summary>
	/// Violet color (R:238,G:130,B:238,A:255).
	/// </summary>
	public static Color Violet { get; private set; }

	/// <summary>
	/// Wheat color (R:245,G:222,B:179,A:255).
	/// </summary>
	public static Color Wheat { get; private set; }

	/// <summary>
	/// White color (R:255,G:255,B:255,A:255).
	/// </summary>
	public static Color White { get; private set; }

	/// <summary>
	/// WhiteSmoke color (R:245,G:245,B:245,A:255).
	/// </summary>
	public static Color WhiteSmoke { get; private set; }

	/// <summary>
	/// Yellow color (R:255,G:255,B:0,A:255).
	/// </summary>
	public static Color Yellow { get; private set; }

	/// <summary>
	/// YellowGreen color (R:154,G:205,B:50,A:255).
	/// </summary>
	public static Color YellowGreen { get; private set; }

	/// <summary>
	/// Gets or sets packed value of this <see cref="T:Microsoft.Xna.Framework.Color" />.
	/// </summary>
	[CLSCompliant(false)]
	public uint PackedValue
	{
		get
		{
			return this._packedValue;
		}
		set
		{
			this._packedValue = value;
		}
	}

	internal string DebugDisplayString => this.R + "  " + this.G + "  " + this.B + "  " + this.A;

	static Color()
	{
		Color.TransparentBlack = new Color(0u);
		Color.Transparent = new Color(0u);
		Color.AliceBlue = new Color(4294965488u);
		Color.AntiqueWhite = new Color(4292340730u);
		Color.Aqua = new Color(4294967040u);
		Color.Aquamarine = new Color(4292149119u);
		Color.Azure = new Color(4294967280u);
		Color.Beige = new Color(4292670965u);
		Color.Bisque = new Color(4291093759u);
		Color.Black = new Color(4278190080u);
		Color.BlanchedAlmond = new Color(4291685375u);
		Color.Blue = new Color(4294901760u);
		Color.BlueViolet = new Color(4293012362u);
		Color.Brown = new Color(4280953509u);
		Color.BurlyWood = new Color(4287084766u);
		Color.CadetBlue = new Color(4288716383u);
		Color.Chartreuse = new Color(4278255487u);
		Color.Chocolate = new Color(4280183250u);
		Color.Coral = new Color(4283465727u);
		Color.CornflowerBlue = new Color(4293760356u);
		Color.Cornsilk = new Color(4292671743u);
		Color.Crimson = new Color(4282127580u);
		Color.Cyan = new Color(4294967040u);
		Color.DarkBlue = new Color(4287299584u);
		Color.DarkCyan = new Color(4287335168u);
		Color.DarkGoldenrod = new Color(4278945464u);
		Color.DarkGray = new Color(4289309097u);
		Color.DarkGreen = new Color(4278215680u);
		Color.DarkKhaki = new Color(4285249469u);
		Color.DarkMagenta = new Color(4287299723u);
		Color.DarkOliveGreen = new Color(4281297749u);
		Color.DarkOrange = new Color(4278226175u);
		Color.DarkOrchid = new Color(4291572377u);
		Color.DarkRed = new Color(4278190219u);
		Color.DarkSalmon = new Color(4286224105u);
		Color.DarkSeaGreen = new Color(4287347855u);
		Color.DarkSlateBlue = new Color(4287315272u);
		Color.DarkSlateGray = new Color(4283387695u);
		Color.DarkTurquoise = new Color(4291939840u);
		Color.DarkViolet = new Color(4292018324u);
		Color.DeepPink = new Color(4287829247u);
		Color.DeepSkyBlue = new Color(4294950656u);
		Color.DimGray = new Color(4285098345u);
		Color.DodgerBlue = new Color(4294938654u);
		Color.Firebrick = new Color(4280427186u);
		Color.FloralWhite = new Color(4293982975u);
		Color.ForestGreen = new Color(4280453922u);
		Color.Fuchsia = new Color(4294902015u);
		Color.Gainsboro = new Color(4292664540u);
		Color.GhostWhite = new Color(4294965496u);
		Color.Gold = new Color(4278245375u);
		Color.Goldenrod = new Color(4280329690u);
		Color.Gray = new Color(4286611584u);
		Color.Green = new Color(4278222848u);
		Color.GreenYellow = new Color(4281335725u);
		Color.Honeydew = new Color(4293984240u);
		Color.HotPink = new Color(4290013695u);
		Color.IndianRed = new Color(4284243149u);
		Color.Indigo = new Color(4286709835u);
		Color.Ivory = new Color(4293984255u);
		Color.Khaki = new Color(4287424240u);
		Color.Lavender = new Color(4294633190u);
		Color.LavenderBlush = new Color(4294308095u);
		Color.LawnGreen = new Color(4278254716u);
		Color.LemonChiffon = new Color(4291689215u);
		Color.LightBlue = new Color(4293318829u);
		Color.LightCoral = new Color(4286611696u);
		Color.LightCyan = new Color(4294967264u);
		Color.LightGoldenrodYellow = new Color(4292016890u);
		Color.LightGray = new Color(4292072403u);
		Color.LightGreen = new Color(4287688336u);
		Color.LightPink = new Color(4290885375u);
		Color.LightSalmon = new Color(4286226687u);
		Color.LightSeaGreen = new Color(4289376800u);
		Color.LightSkyBlue = new Color(4294626951u);
		Color.LightSlateGray = new Color(4288252023u);
		Color.LightSteelBlue = new Color(4292789424u);
		Color.LightYellow = new Color(4292935679u);
		Color.Lime = new Color(4278255360u);
		Color.LimeGreen = new Color(4281519410u);
		Color.Linen = new Color(4293325050u);
		Color.Magenta = new Color(4294902015u);
		Color.Maroon = new Color(4278190208u);
		Color.MediumAquamarine = new Color(4289383782u);
		Color.MediumBlue = new Color(4291624960u);
		Color.MediumOrchid = new Color(4292040122u);
		Color.MediumPurple = new Color(4292571283u);
		Color.MediumSeaGreen = new Color(4285641532u);
		Color.MediumSlateBlue = new Color(4293814395u);
		Color.MediumSpringGreen = new Color(4288346624u);
		Color.MediumTurquoise = new Color(4291613000u);
		Color.MediumVioletRed = new Color(4286911943u);
		Color.MidnightBlue = new Color(4285536537u);
		Color.MintCream = new Color(4294639605u);
		Color.MistyRose = new Color(4292994303u);
		Color.Moccasin = new Color(4290110719u);
		Color.MonoGameOrange = new Color(4278205671u);
		Color.NavajoWhite = new Color(4289584895u);
		Color.Navy = new Color(4286578688u);
		Color.OldLace = new Color(4293326333u);
		Color.Olive = new Color(4278222976u);
		Color.OliveDrab = new Color(4280520299u);
		Color.Orange = new Color(4278232575u);
		Color.OrangeRed = new Color(4278207999u);
		Color.Orchid = new Color(4292243674u);
		Color.PaleGoldenrod = new Color(4289390830u);
		Color.PaleGreen = new Color(4288215960u);
		Color.PaleTurquoise = new Color(4293848751u);
		Color.PaleVioletRed = new Color(4287852763u);
		Color.PapayaWhip = new Color(4292210687u);
		Color.PeachPuff = new Color(4290370303u);
		Color.Peru = new Color(4282353101u);
		Color.Pink = new Color(4291543295u);
		Color.Plum = new Color(4292714717u);
		Color.PowderBlue = new Color(4293320880u);
		Color.Purple = new Color(4286578816u);
		Color.Red = new Color(4278190335u);
		Color.RosyBrown = new Color(4287598524u);
		Color.RoyalBlue = new Color(4292962625u);
		Color.SaddleBrown = new Color(4279453067u);
		Color.Salmon = new Color(4285694202u);
		Color.SandyBrown = new Color(4284523764u);
		Color.SeaGreen = new Color(4283927342u);
		Color.SeaShell = new Color(4293850623u);
		Color.Sienna = new Color(4281160352u);
		Color.Silver = new Color(4290822336u);
		Color.SkyBlue = new Color(4293643911u);
		Color.SlateBlue = new Color(4291648106u);
		Color.SlateGray = new Color(4287660144u);
		Color.Snow = new Color(4294638335u);
		Color.SpringGreen = new Color(4286578432u);
		Color.SteelBlue = new Color(4290019910u);
		Color.Tan = new Color(4287411410u);
		Color.Teal = new Color(4286611456u);
		Color.Thistle = new Color(4292394968u);
		Color.Tomato = new Color(4282868735u);
		Color.Turquoise = new Color(4291878976u);
		Color.Violet = new Color(4293821166u);
		Color.Wheat = new Color(4289978101u);
		Color.White = new Color(uint.MaxValue);
		Color.WhiteSmoke = new Color(4294309365u);
		Color.Yellow = new Color(4278255615u);
		Color.YellowGreen = new Color(4281519514u);
	}

	/// <summary>
	/// Constructs an RGBA color from a packed value.
	/// The value is a 32-bit unsigned integer, with R in the least significant octet.
	/// </summary>
	/// <param name="packedValue">The packed value.</param>
	[CLSCompliant(false)]
	public Color(uint packedValue)
	{
		this._packedValue = packedValue;
	}

	/// <summary>
	/// Constructs an RGBA color from the XYZW unit length components of a vector.
	/// </summary>
	/// <param name="color">A <see cref="T:Microsoft.Xna.Framework.Vector4" /> representing color.</param>
	public Color(Vector4 color)
		: this((int)(color.X * 255f), (int)(color.Y * 255f), (int)(color.Z * 255f), (int)(color.W * 255f))
	{
	}

	/// <summary>
	/// Constructs an RGBA color from the XYZ unit length components of a vector. Alpha value will be opaque.
	/// </summary>
	/// <param name="color">A <see cref="T:Microsoft.Xna.Framework.Vector3" /> representing color.</param>
	public Color(Vector3 color)
		: this((int)(color.X * 255f), (int)(color.Y * 255f), (int)(color.Z * 255f))
	{
	}

	/// <summary>
	/// Constructs an RGBA color from a <see cref="T:Microsoft.Xna.Framework.Color" /> and an alpha value.
	/// </summary>
	/// <param name="color">A <see cref="T:Microsoft.Xna.Framework.Color" /> for RGB values of new <see cref="T:Microsoft.Xna.Framework.Color" /> instance.</param>
	/// <param name="alpha">The alpha component value from 0 to 255.</param>
	public Color(Color color, int alpha)
	{
		if ((alpha & 0xFFFFFF00u) != 0L)
		{
			uint clampedA = (uint)MathHelper.Clamp(alpha, 0, 255);
			this._packedValue = (color._packedValue & 0xFFFFFF) | (clampedA << 24);
		}
		else
		{
			this._packedValue = (color._packedValue & 0xFFFFFF) | (uint)(alpha << 24);
		}
	}

	/// <summary>
	/// Constructs an RGBA color from color and alpha value.
	/// </summary>
	/// <param name="color">A <see cref="T:Microsoft.Xna.Framework.Color" /> for RGB values of new <see cref="T:Microsoft.Xna.Framework.Color" /> instance.</param>
	/// <param name="alpha">Alpha component value from 0.0f to 1.0f.</param>
	public Color(Color color, float alpha)
		: this(color, (int)(alpha * 255f))
	{
	}

	/// <summary>
	/// Constructs an RGBA color from scalars representing red, green and blue values. Alpha value will be opaque.
	/// </summary>
	/// <param name="r">Red component value from 0.0f to 1.0f.</param>
	/// <param name="g">Green component value from 0.0f to 1.0f.</param>
	/// <param name="b">Blue component value from 0.0f to 1.0f.</param>
	public Color(float r, float g, float b)
		: this((int)(r * 255f), (int)(g * 255f), (int)(b * 255f))
	{
	}

	/// <summary>
	/// Constructs an RGBA color from scalars representing red, green, blue and alpha values.
	/// </summary>
	/// <param name="r">Red component value from 0.0f to 1.0f.</param>
	/// <param name="g">Green component value from 0.0f to 1.0f.</param>
	/// <param name="b">Blue component value from 0.0f to 1.0f.</param>
	/// <param name="alpha">Alpha component value from 0.0f to 1.0f.</param>
	public Color(float r, float g, float b, float alpha)
		: this((int)(r * 255f), (int)(g * 255f), (int)(b * 255f), (int)(alpha * 255f))
	{
	}

	/// <summary>
	/// Constructs an RGBA color from scalars representing red, green and blue values. Alpha value will be opaque.
	/// </summary>
	/// <param name="r">Red component value from 0 to 255.</param>
	/// <param name="g">Green component value from 0 to 255.</param>
	/// <param name="b">Blue component value from 0 to 255.</param>
	public Color(int r, int g, int b)
	{
		this._packedValue = 4278190080u;
		if (((r | g | b) & 0xFFFFFF00u) != 0L)
		{
			uint clampedR = (uint)MathHelper.Clamp(r, 0, 255);
			uint clampedG = (uint)MathHelper.Clamp(g, 0, 255);
			uint clampedB = (uint)MathHelper.Clamp(b, 0, 255);
			this._packedValue |= (clampedB << 16) | (clampedG << 8) | clampedR;
		}
		else
		{
			this._packedValue |= (uint)((b << 16) | (g << 8) | r);
		}
	}

	/// <summary>
	/// Constructs an RGBA color from scalars representing red, green, blue and alpha values.
	/// </summary>
	/// <param name="r">Red component value from 0 to 255.</param>
	/// <param name="g">Green component value from 0 to 255.</param>
	/// <param name="b">Blue component value from 0 to 255.</param>
	/// <param name="alpha">Alpha component value from 0 to 255.</param>
	public Color(int r, int g, int b, int alpha)
	{
		if (((r | g | b | alpha) & 0xFFFFFF00u) != 0L)
		{
			uint clampedR = (uint)MathHelper.Clamp(r, 0, 255);
			uint clampedG = (uint)MathHelper.Clamp(g, 0, 255);
			uint clampedB = (uint)MathHelper.Clamp(b, 0, 255);
			uint clampedA = (uint)MathHelper.Clamp(alpha, 0, 255);
			this._packedValue = (clampedA << 24) | (clampedB << 16) | (clampedG << 8) | clampedR;
		}
		else
		{
			this._packedValue = (uint)((alpha << 24) | (b << 16) | (g << 8) | r);
		}
	}

	/// <summary>
	/// Constructs an RGBA color from scalars representing red, green, blue and alpha values.
	/// </summary>
	/// <remarks>
	/// This overload sets the values directly without clamping, and may therefore be faster than the other overloads.
	/// </remarks>
	/// <param name="r"></param>
	/// <param name="g"></param>
	/// <param name="b"></param>
	/// <param name="alpha"></param>
	public Color(byte r, byte g, byte b, byte alpha)
	{
		this._packedValue = (uint)((alpha << 24) | (b << 16) | (g << 8) | r);
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.Color" /> instances are equal.
	/// </summary>
	/// <param name="a"><see cref="T:Microsoft.Xna.Framework.Color" /> instance on the left of the equal sign.</param>
	/// <param name="b"><see cref="T:Microsoft.Xna.Framework.Color" /> instance on the right of the equal sign.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public static bool operator ==(Color a, Color b)
	{
		return a._packedValue == b._packedValue;
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.Color" /> instances are not equal.
	/// </summary>
	/// <param name="a"><see cref="T:Microsoft.Xna.Framework.Color" /> instance on the left of the not equal sign.</param>
	/// <param name="b"><see cref="T:Microsoft.Xna.Framework.Color" /> instance on the right of the not equal sign.</param>
	/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
	public static bool operator !=(Color a, Color b)
	{
		return a._packedValue != b._packedValue;
	}

	/// <summary>
	/// Gets the hash code of this <see cref="T:Microsoft.Xna.Framework.Color" />.
	/// </summary>
	/// <returns>Hash code of this <see cref="T:Microsoft.Xna.Framework.Color" />.</returns>
	public override int GetHashCode()
	{
		return this._packedValue.GetHashCode();
	}

	/// <summary>
	/// Compares whether current instance is equal to specified object.
	/// </summary>
	/// <param name="obj">The <see cref="T:Microsoft.Xna.Framework.Color" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public override bool Equals(object obj)
	{
		if (obj is Color)
		{
			return this.Equals((Color)obj);
		}
		return false;
	}

	/// <summary>
	/// Performs linear interpolation of <see cref="T:Microsoft.Xna.Framework.Color" />.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Color" />.</param>
	/// <param name="value2">Destination <see cref="T:Microsoft.Xna.Framework.Color" />.</param>
	/// <param name="amount">Interpolation factor.</param>
	/// <returns>Interpolated <see cref="T:Microsoft.Xna.Framework.Color" />.</returns>
	public static Color Lerp(Color value1, Color value2, float amount)
	{
		amount = MathHelper.Clamp(amount, 0f, 1f);
		return new Color((int)MathHelper.Lerp((int)value1.R, (int)value2.R, amount), (int)MathHelper.Lerp((int)value1.G, (int)value2.G, amount), (int)MathHelper.Lerp((int)value1.B, (int)value2.B, amount), (int)MathHelper.Lerp((int)value1.A, (int)value2.A, amount));
	}

	/// <summary>
	/// <see cref="M:Microsoft.Xna.Framework.Color.Lerp(Microsoft.Xna.Framework.Color,Microsoft.Xna.Framework.Color,System.Single)" /> should be used instead of this function.
	/// </summary>
	/// <returns>Interpolated <see cref="T:Microsoft.Xna.Framework.Color" />.</returns>
	[Obsolete("Color.Lerp should be used instead of this function.")]
	public static Color LerpPrecise(Color value1, Color value2, float amount)
	{
		amount = MathHelper.Clamp(amount, 0f, 1f);
		return new Color((int)MathHelper.LerpPrecise((int)value1.R, (int)value2.R, amount), (int)MathHelper.LerpPrecise((int)value1.G, (int)value2.G, amount), (int)MathHelper.LerpPrecise((int)value1.B, (int)value2.B, amount), (int)MathHelper.LerpPrecise((int)value1.A, (int)value2.A, amount));
	}

	/// <summary>
	/// Multiply <see cref="T:Microsoft.Xna.Framework.Color" /> by value.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Color" />.</param>
	/// <param name="scale">Multiplicator.</param>
	/// <returns>Multiplication result.</returns>
	public static Color Multiply(Color value, float scale)
	{
		return new Color((int)((float)(int)value.R * scale), (int)((float)(int)value.G * scale), (int)((float)(int)value.B * scale), (int)((float)(int)value.A * scale));
	}

	/// <summary>
	/// Multiply <see cref="T:Microsoft.Xna.Framework.Color" /> by value.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Color" />.</param>
	/// <param name="scale">Multiplicator.</param>
	/// <returns>Multiplication result.</returns>
	public static Color operator *(Color value, float scale)
	{
		return new Color((int)((float)(int)value.R * scale), (int)((float)(int)value.G * scale), (int)((float)(int)value.B * scale), (int)((float)(int)value.A * scale));
	}

	public static Color operator *(float scale, Color value)
	{
		return new Color((int)((float)(int)value.R * scale), (int)((float)(int)value.G * scale), (int)((float)(int)value.B * scale), (int)((float)(int)value.A * scale));
	}

	/// <summary>
	/// Gets a <see cref="T:Microsoft.Xna.Framework.Vector3" /> representation for this object.
	/// </summary>
	/// <returns>A <see cref="T:Microsoft.Xna.Framework.Vector3" /> representation for this object.</returns>
	public Vector3 ToVector3()
	{
		return new Vector3((float)(int)this.R / 255f, (float)(int)this.G / 255f, (float)(int)this.B / 255f);
	}

	/// <summary>
	/// Gets a <see cref="T:Microsoft.Xna.Framework.Vector4" /> representation for this object.
	/// </summary>
	/// <returns>A <see cref="T:Microsoft.Xna.Framework.Vector4" /> representation for this object.</returns>
	public Vector4 ToVector4()
	{
		return new Vector4((float)(int)this.R / 255f, (float)(int)this.G / 255f, (float)(int)this.B / 255f, (float)(int)this.A / 255f);
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Color" /> in the format:
	/// {R:[red] G:[green] B:[blue] A:[alpha]}
	/// </summary>
	/// <returns><see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Color" />.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(25);
		stringBuilder.Append("{R:");
		stringBuilder.Append(this.R);
		stringBuilder.Append(" G:");
		stringBuilder.Append(this.G);
		stringBuilder.Append(" B:");
		stringBuilder.Append(this.B);
		stringBuilder.Append(" A:");
		stringBuilder.Append(this.A);
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	/// <summary>
	/// Translate a non-premultipled alpha <see cref="T:Microsoft.Xna.Framework.Color" /> to a <see cref="T:Microsoft.Xna.Framework.Color" /> that contains premultiplied alpha.
	/// </summary>
	/// <param name="vector">A <see cref="T:Microsoft.Xna.Framework.Vector4" /> representing color.</param>
	/// <returns>A <see cref="T:Microsoft.Xna.Framework.Color" /> which contains premultiplied alpha data.</returns>
	public static Color FromNonPremultiplied(Vector4 vector)
	{
		return new Color(vector.X * vector.W, vector.Y * vector.W, vector.Z * vector.W, vector.W);
	}

	/// <summary>
	/// Translate a non-premultipled alpha <see cref="T:Microsoft.Xna.Framework.Color" /> to a <see cref="T:Microsoft.Xna.Framework.Color" /> that contains premultiplied alpha.
	/// </summary>
	/// <param name="r">Red component value.</param>
	/// <param name="g">Green component value.</param>
	/// <param name="b">Blue component value.</param>
	/// <param name="a">Alpha component value.</param>
	/// <returns>A <see cref="T:Microsoft.Xna.Framework.Color" /> which contains premultiplied alpha data.</returns>
	public static Color FromNonPremultiplied(int r, int g, int b, int a)
	{
		return new Color(r * a / 255, g * a / 255, b * a / 255, a);
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:Microsoft.Xna.Framework.Color" />.
	/// </summary>
	/// <param name="other">The <see cref="T:Microsoft.Xna.Framework.Color" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public bool Equals(Color other)
	{
		return this.PackedValue == other.PackedValue;
	}

	/// <summary>
	/// Deconstruction method for <see cref="T:Microsoft.Xna.Framework.Color" />.
	/// </summary>
	/// <param name="r">Red component value from 0 to 255.</param>
	/// <param name="g">Green component value from 0 to 255.</param>
	/// <param name="b">Blue component value from 0 to 255.</param>
	public void Deconstruct(out byte r, out byte g, out byte b)
	{
		r = this.R;
		g = this.G;
		b = this.B;
	}

	/// <summary>
	/// Deconstruction method for <see cref="T:Microsoft.Xna.Framework.Color" />.
	/// </summary>
	/// <param name="r">Red component value from 0.0f to 1.0f.</param>
	/// <param name="g">Green component value from 0.0f to 1.0f.</param>
	/// <param name="b">Blue component value from 0.0f to 1.0f.</param>
	public void Deconstruct(out float r, out float g, out float b)
	{
		r = (float)(int)this.R / 255f;
		g = (float)(int)this.G / 255f;
		b = (float)(int)this.B / 255f;
	}

	/// <summary>
	/// Deconstruction method for <see cref="T:Microsoft.Xna.Framework.Color" /> with Alpha.
	/// </summary>
	/// <param name="r">Red component value from 0 to 255.</param>
	/// <param name="g">Green component value from 0 to 255.</param>
	/// <param name="b">Blue component value from 0 to 255.</param>
	/// <param name="a">Alpha component value from 0 to 255.</param>
	public void Deconstruct(out byte r, out byte g, out byte b, out byte a)
	{
		r = this.R;
		g = this.G;
		b = this.B;
		a = this.A;
	}

	/// <summary>
	/// Deconstruction method for <see cref="T:Microsoft.Xna.Framework.Color" /> with Alpha.
	/// </summary>
	/// <param name="r">Red component value from 0.0f to 1.0f.</param>
	/// <param name="g">Green component value from 0.0f to 1.0f.</param>
	/// <param name="b">Blue component value from 0.0f to 1.0f.</param>
	/// <param name="a">Alpha component value from 0.0f to 1.0f.</param>
	public void Deconstruct(out float r, out float g, out float b, out float a)
	{
		r = (float)(int)this.R / 255f;
		g = (float)(int)this.G / 255f;
		b = (float)(int)this.B / 255f;
		a = (float)(int)this.A / 255f;
	}
}
