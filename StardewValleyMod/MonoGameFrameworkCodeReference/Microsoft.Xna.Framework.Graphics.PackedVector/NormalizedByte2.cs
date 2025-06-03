using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector;

public struct NormalizedByte2 : IPackedVector<ushort>, IPackedVector, IEquatable<NormalizedByte2>
{
	private ushort _packed;

	[CLSCompliant(false)]
	public ushort PackedValue
	{
		get
		{
			return this._packed;
		}
		set
		{
			this._packed = value;
		}
	}

	public NormalizedByte2(Vector2 vector)
	{
		this._packed = NormalizedByte2.Pack(vector.X, vector.Y);
	}

	public NormalizedByte2(float x, float y)
	{
		this._packed = NormalizedByte2.Pack(x, y);
	}

	public static bool operator !=(NormalizedByte2 a, NormalizedByte2 b)
	{
		return a._packed != b._packed;
	}

	public static bool operator ==(NormalizedByte2 a, NormalizedByte2 b)
	{
		return a._packed == b._packed;
	}

	public override bool Equals(object obj)
	{
		if (obj is NormalizedByte2)
		{
			return ((NormalizedByte2)obj)._packed == this._packed;
		}
		return false;
	}

	public bool Equals(NormalizedByte2 other)
	{
		return this._packed == other._packed;
	}

	public override int GetHashCode()
	{
		return this._packed.GetHashCode();
	}

	public override string ToString()
	{
		return this._packed.ToString("X");
	}

	private static ushort Pack(float x, float y)
	{
		int num = (ushort)Math.Round(MathHelper.Clamp(x, -1f, 1f) * 127f) & 0xFF;
		int byte1 = ((ushort)Math.Round(MathHelper.Clamp(y, -1f, 1f) * 127f) & 0xFF) << 8;
		return (ushort)(num | byte1);
	}

	void IPackedVector.PackFromVector4(Vector4 vector)
	{
		this._packed = NormalizedByte2.Pack(vector.X, vector.Y);
	}

	/// <summary>
	/// Gets the packed vector in Vector4 format.
	/// </summary>
	/// <returns>The packed vector in Vector4 format</returns>
	public Vector4 ToVector4()
	{
		return new Vector4(this.ToVector2(), 0f, 1f);
	}

	public Vector2 ToVector2()
	{
		return new Vector2((float)(sbyte)(this._packed & 0xFF) / 127f, (float)(sbyte)((this._packed >> 8) & 0xFF) / 127f);
	}
}
