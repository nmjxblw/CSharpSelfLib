namespace Microsoft.Xna.Framework.Content;

internal class PointReader : ContentTypeReader<Point>
{
	protected internal override Point Read(ContentReader input, Point existingInstance)
	{
		int x = input.ReadInt32();
		int Y = input.ReadInt32();
		return new Point(x, Y);
	}
}
