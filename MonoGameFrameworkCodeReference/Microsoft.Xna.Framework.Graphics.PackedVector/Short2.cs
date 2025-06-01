using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector;

public struct Short2 : IPackedVector<uint>, IPackedVector, IEquatable<Short2>
{
	private uint _short2Packed;

	[CLSCompliant(false)]
	public uint PackedValue
	{
		get
		{
			return this._short2Packed;
		}
		set
		{
			this._short2Packed = value;
		}
	}

	public Short2(Vector2 vector)
	{
		this._short2Packed = Short2.PackInTwo(vector.X, vector.Y);
	}

	public Short2(float x, float y)
	{
		this._short2Packed = Short2.PackInTwo(x, y);
	}

	public static bool operator !=(Short2 a, Short2 b)
	{
		return a.PackedValue != b.PackedValue;
	}

	public static bool operator ==(Short2 a, Short2 b)
	{
		return a.PackedValue == b.PackedValue;
	}

	public override bool Equals(object obj)
	{
		if (obj is Short2)
		{
			return this == (Short2)obj;
		}
		return false;
	}

	public bool Equals(Short2 other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return this._short2Packed.GetHashCode();
	}

	public override string ToString()
	{
		return this._short2Packed.ToString("x8");
	}

	public Vector2 ToVector2()
	{
		return new Vector2
		{
			X = (short)(this._short2Packed & 0xFFFF),
			Y = (short)(this._short2Packed >> 16)
		};
	}

	private static uint PackInTwo(float vectorX, float vectorY)
	{
		uint num = (uint)Math.Round(MathHelper.Clamp(vectorX, -32768f, 32767f)) & 0xFFFF;
		uint word1 = ((uint)Math.Round(MathHelper.Clamp(vectorY, -32768f, 32767f)) & 0xFFFF) << 16;
		return num | word1;
	}

	void IPackedVector.PackFromVector4(Vector4 vector)
	{
		this._short2Packed = Short2.PackInTwo(vector.X, vector.Y);
	}

	/// <summary>
	/// Gets the packed vector in Vector4 format.
	/// </summary>
	/// <returns>The packed vector in Vector4 format</returns>
	public Vector4 ToVector4()
	{
		Vector4 v4 = new Vector4(0f, 0f, 0f, 1f);
		v4.X = (short)(this._short2Packed & 0xFFFF);
		v4.Y = (short)(this._short2Packed >> 16);
		return v4;
	}
}
