using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

internal class SpriteFontReader : ContentTypeReader<SpriteFont>
{
	protected internal override SpriteFont Read(ContentReader input, SpriteFont existingInstance)
	{
		if (existingInstance != null)
		{
			input.ReadObject(existingInstance.Texture);
			input.ReadObject<List<Rectangle>>();
			input.ReadObject<List<Rectangle>>();
			input.ReadObject<List<char>>();
			input.ReadInt32();
			input.ReadSingle();
			input.ReadObject<List<Vector3>>();
			if (input.ReadBoolean())
			{
				input.ReadChar();
			}
			return existingInstance;
		}
		Texture2D texture = input.ReadObject<Texture2D>();
		List<Rectangle> glyphs = input.ReadObject<List<Rectangle>>();
		List<Rectangle> cropping = input.ReadObject<List<Rectangle>>();
		List<char> charMap = input.ReadObject<List<char>>();
		int lineSpacing = input.ReadInt32();
		float spacing = input.ReadSingle();
		List<Vector3> kerning = input.ReadObject<List<Vector3>>();
		char? defaultCharacter = null;
		if (input.ReadBoolean())
		{
			defaultCharacter = input.ReadChar();
		}
		return new SpriteFont(texture, glyphs, cropping, charMap, lineSpacing, spacing, kerning, defaultCharacter);
	}
}
