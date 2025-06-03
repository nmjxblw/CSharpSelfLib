using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPositionColorTexture : IVertexType
{
	public Vector3 Position;

	public Color Color;

	public Vector2 TextureCoordinate;

	public static readonly VertexDeclaration VertexDeclaration;

	VertexDeclaration IVertexType.VertexDeclaration => VertexPositionColorTexture.VertexDeclaration;

	public VertexPositionColorTexture(Vector3 position, Color color, Vector2 textureCoordinate)
	{
		this.Position = position;
		this.Color = color;
		this.TextureCoordinate = textureCoordinate;
	}

	public override int GetHashCode()
	{
		return (((this.Position.GetHashCode() * 397) ^ this.Color.GetHashCode()) * 397) ^ this.TextureCoordinate.GetHashCode();
	}

	public override string ToString()
	{
		string[] obj = new string[7] { "{{Position:", null, null, null, null, null, null };
		Vector3 position = this.Position;
		obj[1] = position.ToString();
		obj[2] = " Color:";
		Color color = this.Color;
		obj[3] = color.ToString();
		obj[4] = " TextureCoordinate:";
		Vector2 textureCoordinate = this.TextureCoordinate;
		obj[5] = textureCoordinate.ToString();
		obj[6] = "}}";
		return string.Concat(obj);
	}

	public static bool operator ==(VertexPositionColorTexture left, VertexPositionColorTexture right)
	{
		if (left.Position == right.Position && left.Color == right.Color)
		{
			return left.TextureCoordinate == right.TextureCoordinate;
		}
		return false;
	}

	public static bool operator !=(VertexPositionColorTexture left, VertexPositionColorTexture right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != base.GetType())
		{
			return false;
		}
		return this == (VertexPositionColorTexture)obj;
	}

	static VertexPositionColorTexture()
	{
		VertexPositionColorTexture.VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0), new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));
	}
}
