namespace Microsoft.Xna.Framework.Audio;

internal struct RpcVariable
{
	public string Name;

	public float Value;

	public byte Flags;

	public float InitValue;

	public float MaxValue;

	public float MinValue;

	public bool IsPublic => (this.Flags & 1) != 0;

	public bool IsReadOnly => (this.Flags & 2) != 0;

	public bool IsGlobal => (this.Flags & 4) == 0;

	public bool IsReserved => (this.Flags & 8) != 0;

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
}
