using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Xna.Framework.Design;

public class Vector2TypeConverter : TypeConverter
{
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (VectorConversion.CanConvertTo(context, destinationType))
		{
			return true;
		}
		if (destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		Vector2 vec = (Vector2)value;
		if (VectorConversion.CanConvertTo(context, destinationType))
		{
			Vector4 vec4 = new Vector4(vec.X, vec.Y, 0f, 0f);
			return VectorConversion.ConvertToFromVector4(context, culture, vec4, destinationType);
		}
		if (destinationType == typeof(string))
		{
			return string.Join(value: new string[2]
			{
				vec.X.ToString("R", culture),
				vec.Y.ToString("R", culture)
			}, separator: culture.TextInfo.ListSeparator + " ");
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		Type type = value.GetType();
		Vector2 vec = Vector2.Zero;
		if (type == typeof(string))
		{
			string[] words = ((string)value).Split(culture.TextInfo.ListSeparator.ToCharArray());
			vec.X = float.Parse(words[0], culture);
			vec.Y = float.Parse(words[1], culture);
			return vec;
		}
		return base.ConvertFrom(context, culture, value);
	}
}
