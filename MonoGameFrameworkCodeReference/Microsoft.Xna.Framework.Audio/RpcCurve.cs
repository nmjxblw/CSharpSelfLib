namespace Microsoft.Xna.Framework.Audio;

internal struct RpcCurve
{
	public uint FileOffset;

	public int Variable;

	public bool IsGlobal;

	public RpcParameter Parameter;

	public RpcPoint[] Points;

	public float Evaluate(float position)
	{
		RpcPoint first = this.Points[0];
		if (position <= first.Position)
		{
			return first.Value;
		}
		RpcPoint second = this.Points[this.Points.Length - 1];
		if (position >= second.Position)
		{
			return second.Value;
		}
		for (int i = 1; i < this.Points.Length; i++)
		{
			second = this.Points[i];
			if (second.Position >= position)
			{
				break;
			}
			first = second;
		}
		_ = first.Type;
		float t = (position - first.Position) / (second.Position - first.Position);
		return first.Value + (second.Value - first.Value) * t;
	}
}
