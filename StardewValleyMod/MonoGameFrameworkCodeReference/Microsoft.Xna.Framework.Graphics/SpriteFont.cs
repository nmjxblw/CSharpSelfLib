using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics;

public sealed class SpriteFont
{
	internal static class Errors
	{
		public const string TextContainsUnresolvableCharacters = "Text contains characters that cannot be resolved by this SpriteFont.";

		public const string UnresolvableCharacter = "Character cannot be resolved by this SpriteFont.";
	}

	private class CharComparer : IEqualityComparer<char>
	{
		public static readonly CharComparer Default = new CharComparer();

		public bool Equals(char x, char y)
		{
			return x == y;
		}

		public int GetHashCode(char b)
		{
			return b;
		}
	}

	internal struct CharacterSource
	{
		private readonly string _string;

		private readonly StringBuilder _builder;

		public readonly int Length;

		public char this[int index]
		{
			get
			{
				if (this._string != null)
				{
					return this._string[index];
				}
				return this._builder[index];
			}
		}

		public CharacterSource(string s)
		{
			this._string = s;
			this._builder = null;
			this.Length = s.Length;
		}

		public CharacterSource(StringBuilder builder)
		{
			this._builder = builder;
			this._string = null;
			this.Length = this._builder.Length;
		}
	}

	/// <summary>
	/// Struct that defines the spacing, Kerning, and bounds of a character.
	/// </summary>
	/// <remarks>Provides the data necessary to implement custom SpriteFont rendering.</remarks>
	public struct Glyph
	{
		/// <summary>
		/// The char associated with this glyph.
		/// </summary>
		public char Character;

		/// <summary>
		/// Rectangle in the font texture where this letter exists.
		/// </summary>
		public Rectangle BoundsInTexture;

		/// <summary>
		/// Cropping applied to the BoundsInTexture to calculate the bounds of the actual character.
		/// </summary>
		public Rectangle Cropping;

		/// <summary>
		/// The amount of space between the left side ofthe character and its first pixel in the X dimention.
		/// </summary>
		public float LeftSideBearing;

		/// <summary>
		/// The amount of space between the right side of the character and its last pixel in the X dimention.
		/// </summary>
		public float RightSideBearing;

		/// <summary>
		/// Width of the character before kerning is applied. 
		/// </summary>
		public float Width;

		/// <summary>
		/// Width of the character before kerning is applied. 
		/// </summary>
		public float WidthIncludingBearings;

		public static readonly Glyph Empty;

		public override string ToString()
		{
			string[] obj = new string[12]
			{
				"CharacterIndex=",
				this.Character.ToString(),
				", Glyph=",
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null
			};
			Rectangle boundsInTexture = this.BoundsInTexture;
			obj[3] = boundsInTexture.ToString();
			obj[4] = ", Cropping=";
			boundsInTexture = this.Cropping;
			obj[5] = boundsInTexture.ToString();
			obj[6] = ", Kerning=";
			obj[7] = this.LeftSideBearing.ToString();
			obj[8] = ",";
			obj[9] = this.Width.ToString();
			obj[10] = ",";
			obj[11] = this.RightSideBearing.ToString();
			return string.Concat(obj);
		}
	}

	private struct CharacterRegion
	{
		public char Start;

		public char End;

		public int StartIndex;

		public CharacterRegion(char start, int startIndex)
		{
			this.Start = start;
			this.End = start;
			this.StartIndex = startIndex;
		}
	}

	private readonly Glyph[] _glyphs;

	private readonly CharacterRegion[] _regions;

	private char? _defaultCharacter;

	private int _defaultGlyphIndex = -1;

	private readonly Texture2D _texture;

	/// <summary>
	/// All the glyphs in this SpriteFont.
	/// </summary>
	public Glyph[] Glyphs => this._glyphs;

	/// <summary>
	/// Gets the texture that this SpriteFont draws from.
	/// </summary>
	/// <remarks>Can be used to implement custom rendering of a SpriteFont</remarks>
	public Texture2D Texture => this._texture;

	/// <summary>
	/// Gets a collection of the characters in the font.
	/// </summary>
	public ReadOnlyCollection<char> Characters { get; private set; }

	/// <summary>
	/// Gets or sets the character that will be substituted when a
	/// given character is not included in the font.
	/// </summary>
	public char? DefaultCharacter
	{
		get
		{
			return this._defaultCharacter;
		}
		set
		{
			if (value.HasValue)
			{
				if (!this.TryGetGlyphIndex(value.Value, out this._defaultGlyphIndex))
				{
					throw new ArgumentException("Character cannot be resolved by this SpriteFont.");
				}
			}
			else
			{
				this._defaultGlyphIndex = -1;
			}
			this._defaultCharacter = value;
		}
	}

	/// <summary>
	/// Gets or sets the line spacing (the distance from baseline
	/// to baseline) of the font.
	/// </summary>
	public int LineSpacing { get; set; }

	/// <summary>
	/// Gets or sets the spacing (tracking) between characters in
	/// the font.
	/// </summary>
	public float Spacing { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Graphics.SpriteFont" /> class.
	/// </summary>
	/// <param name="texture">The font texture.</param>
	/// <param name="glyphBounds">The rectangles in the font texture containing letters.</param>
	/// <param name="cropping">The cropping rectangles, which are applied to the corresponding glyphBounds to calculate the bounds of the actual character.</param>
	/// <param name="characters">The characters.</param>
	/// <param name="lineSpacing">The line spacing (the distance from baseline to baseline) of the font.</param>
	/// <param name="spacing">The spacing (tracking) between characters in the font.</param>
	/// <param name="kerning">The letters kernings(X - left side bearing, Y - width and Z - right side bearing).</param>
	/// <param name="defaultCharacter">The character that will be substituted when a given character is not included in the font.</param>
	public SpriteFont(Texture2D texture, List<Rectangle> glyphBounds, List<Rectangle> cropping, List<char> characters, int lineSpacing, float spacing, List<Vector3> kerning, char? defaultCharacter)
	{
		this.Characters = new ReadOnlyCollection<char>(characters.ToArray());
		this._texture = texture;
		this.LineSpacing = lineSpacing;
		this.Spacing = spacing;
		this._glyphs = new Glyph[characters.Count];
		Stack<CharacterRegion> regions = new Stack<CharacterRegion>();
		for (int i = 0; i < characters.Count; i++)
		{
			this._glyphs[i] = new Glyph
			{
				BoundsInTexture = glyphBounds[i],
				Cropping = cropping[i],
				Character = characters[i],
				LeftSideBearing = kerning[i].X,
				Width = kerning[i].Y,
				RightSideBearing = kerning[i].Z,
				WidthIncludingBearings = kerning[i].X + kerning[i].Y + kerning[i].Z
			};
			if (regions.Count == 0 || characters[i] > regions.Peek().End + 1)
			{
				regions.Push(new CharacterRegion(characters[i], i));
				continue;
			}
			if (characters[i] == regions.Peek().End + 1)
			{
				CharacterRegion currentRegion = regions.Pop();
				currentRegion.End += '\u0001';
				regions.Push(currentRegion);
				continue;
			}
			throw new InvalidOperationException("Invalid SpriteFont. Character map must be in ascending order.");
		}
		this._regions = regions.ToArray();
		Array.Reverse(this._regions);
		this.DefaultCharacter = defaultCharacter;
	}

	/// <summary>
	/// Returns a copy of the dictionary containing the glyphs in this SpriteFont.
	/// </summary>
	/// <returns>A new Dictionary containing all of the glyphs inthis SpriteFont</returns>
	/// <remarks>Can be used to calculate character bounds when implementing custom SpriteFont rendering.</remarks>
	public Dictionary<char, Glyph> GetGlyphs()
	{
		Dictionary<char, Glyph> glyphsDictionary = new Dictionary<char, Glyph>(this._glyphs.Length, CharComparer.Default);
		Glyph[] glyphs = this._glyphs;
		for (int i = 0; i < glyphs.Length; i++)
		{
			Glyph glyph = glyphs[i];
			glyphsDictionary.Add(glyph.Character, glyph);
		}
		return glyphsDictionary;
	}

	/// <summary>
	/// Returns the size of a string when rendered in this font.
	/// </summary>
	/// <param name="text">The text to measure.</param>
	/// <returns>The size, in pixels, of 'text' when rendered in
	/// this font.</returns>
	public Vector2 MeasureString(string text)
	{
		CharacterSource source = new CharacterSource(text);
		this.MeasureString(ref source, out var size);
		return size;
	}

	/// <summary>
	/// Returns the size of the contents of a StringBuilder when
	/// rendered in this font.
	/// </summary>
	/// <param name="text">The text to measure.</param>
	/// <returns>The size, in pixels, of 'text' when rendered in
	/// this font.</returns>
	public Vector2 MeasureString(StringBuilder text)
	{
		CharacterSource source = new CharacterSource(text);
		this.MeasureString(ref source, out var size);
		return size;
	}

	internal unsafe void MeasureString(ref CharacterSource text, out Vector2 size)
	{
		if (text.Length == 0)
		{
			size = Vector2.Zero;
			return;
		}
		float width = 0f;
		float finalLineHeight = this.LineSpacing;
		Vector2 offset = Vector2.Zero;
		bool firstGlyphOfLine = true;
		fixed (Glyph* pGlyphs = this.Glyphs)
		{
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				switch (c)
				{
				case '\n':
					finalLineHeight = this.LineSpacing;
					offset.X = 0f;
					offset.Y += this.LineSpacing;
					firstGlyphOfLine = true;
					continue;
				case '\r':
					continue;
				}
				int currentGlyphIndex = this.GetGlyphIndexOrDefault(c);
				Glyph* pCurrentGlyph = pGlyphs + currentGlyphIndex;
				if (firstGlyphOfLine)
				{
					offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0f);
					firstGlyphOfLine = false;
				}
				else
				{
					offset.X += this.Spacing + pCurrentGlyph->LeftSideBearing;
				}
				offset.X += pCurrentGlyph->Width;
				float proposedWidth = offset.X + Math.Max(pCurrentGlyph->RightSideBearing, 0f);
				if (proposedWidth > width)
				{
					width = proposedWidth;
				}
				offset.X += pCurrentGlyph->RightSideBearing;
				if ((float)pCurrentGlyph->Cropping.Height > finalLineHeight)
				{
					finalLineHeight = pCurrentGlyph->Cropping.Height;
				}
			}
		}
		size.X = width;
		size.Y = offset.Y + finalLineHeight;
	}

	internal unsafe bool TryGetGlyphIndex(char c, out int index)
	{
		fixed (CharacterRegion* pRegions = this._regions)
		{
			int regionIdx = -1;
			int l = 0;
			int r = this._regions.Length - 1;
			while (l <= r)
			{
				int m = l + r >> 1;
				if (pRegions[m].End < c)
				{
					l = m + 1;
					continue;
				}
				if (pRegions[m].Start > c)
				{
					r = m - 1;
					continue;
				}
				regionIdx = m;
				break;
			}
			if (regionIdx == -1)
			{
				index = -1;
				return false;
			}
			index = pRegions[regionIdx].StartIndex + (c - pRegions[regionIdx].Start);
		}
		return true;
	}

	internal int GetGlyphIndexOrDefault(char c)
	{
		if (!this.TryGetGlyphIndex(c, out var glyphIdx))
		{
			if (this._defaultGlyphIndex == -1)
			{
				throw new ArgumentException("Text contains characters that cannot be resolved by this SpriteFont.", "text");
			}
			return this._defaultGlyphIndex;
		}
		return glyphIdx;
	}
}
