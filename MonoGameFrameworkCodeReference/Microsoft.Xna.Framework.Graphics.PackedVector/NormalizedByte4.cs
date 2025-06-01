using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector;

public struct NormalizedByte4 : IPackedVector<uint>, IPackedVector, IEquatable<NormalizedByte4>
{
	private uint _packed;

	[CLSCompliant(false)]
	public uint PackedValue
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

	public NormalizedByte4(Vector4 vector)
	{
		this._packed = NormalizedByte4.Pack(vector.X, vector.Y, vector.Z, vector.W);
	}

	public NormalizedByte4(float x, float y, float z, float w)
	{
		this._packed = NormalizedByte4.Pack(x, y, z, w);
	}

	public static bool operator !=(NormalizedByte4 a, NormalizedByte4 b)
	{
		return a._packed != b._packed;
	}

	public static bool operator ==(NormalizedByte4 a, NormalizedByte4 b)
	{
		return a._packed == b._packed;
	}

	public override bool Equals(object obj)
	{
		if (obj is NormalizedByte4)
		{
			return ((NormalizedByte4)obj)._packed == this._packed;
		}
		return false;
	}

	public bool Equals(NormalizedByte4 other)
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

	private static uint Pack(float x, float y, float z, float w)
	{
		uint num = (uint)Math.Round(MathHelper.Clamp(x, -1f, 1f) * 127f) & 0xFF;
		uint byte3 = ((uint)Math.Round(MathHelper.Clamp(y, -1f, 1f) * 127f) & 0xFF) << 8;
		uint byte4 = ((uint)Math.Round(MathHelper.Clamp(z, -1f, 1f) * 127f) & 0xFF) << 16;
		uint byte5 = ((uint)Math.Round(MathHelper.Clamp(w, -1f, 1f) * 127f) & 0xFF) << 24;
		return num | byte3 | byte4 | byte5;
	}

	void IPackedVector.PackFromVector4(Vector4 vector)
	{
		this._packed = NormalizedByte4.Pack(vector.X, vector.Y, vector.Z, vector.W);
	}

	public Vector4 ToVector4()
	{
		return new Vector4((float)(sbyte)(this._packed & 0xFF) / 127f, (float)(sbyte)((this._packed >> 8) & 0xFF) / 127f, (float)(sbyte)((this._packed >> 16) & 0xFF) / 127f, (float)(sbyte)((this._packed >> 24) & 0xFF) / 127f);
	}
}
