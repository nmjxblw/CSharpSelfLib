using System.IO;

namespace Microsoft.Xna.Framework.Audio;

internal struct DspParameter
{
	public float Value;

	public readonly float MinValue;

	public readonly float MaxValue;

	public DspParameter(BinaryReader reader)
	{
		reader.ReadByte();
		this.Value = reader.ReadSingle();
		this.MinValue = reader.ReadSingle();
		this.MaxValue = reader.ReadSingle();
		reader.ReadUInt16();
	}

	public void SetValue(float value)
	{
		if (value < this.MinValue)
		{
			this.Value = this.MinValue;
		}
		else if (value > this.MaxValue)
		{
			this.Value = this.MaxValue;
		}
		else
		{
			this.Value = value;
		}
	}

	public override string ToString()
	{
		return "Value:" + this.Value + " MinValue:" + this.MinValue + " MaxValue:" + this.MaxValue;
	}
}
