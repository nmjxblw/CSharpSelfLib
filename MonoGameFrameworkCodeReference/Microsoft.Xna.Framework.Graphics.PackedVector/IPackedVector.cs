namespace Microsoft.Xna.Framework.Graphics.PackedVector;

public interface IPackedVector
{
	void PackFromVector4(Vector4 vector);

	Vector4 ToVector4();
}
public interface IPackedVector<TPacked> : IPackedVector
{
	TPacked PackedValue { get; set; }
}
