using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPositionTexture : IVertexType
{
	public Vector3 Position;

	public Vector2 TextureCoordinate;

	public static readonly VertexDeclaration VertexDeclaration;

	VertexDeclaration IVertexType.VertexDeclaration => VertexPositionTexture.VertexDeclaration;

	public VertexPositionTexture(Vector3 position, Vector2 textureCoordinate)
	{
		this.Position = position;
		this.TextureCoordinate = textureCoordinate;
	}

	public override int GetHashCode()
	{
		return (this.Position.GetHashCode() * 397) ^ this.TextureCoordinate.GetHashCode();
	}

	public override string ToString()
	{
		string[] obj = new string[5] { "{{Position:", null, null, null, null };
		Vector3 position = this.Position;
		obj[1] = position.ToString();
		obj[2] = " TextureCoordinate:";
		Vector2 textureCoordinate = this.TextureCoordinate;
		obj[3] = textureCoordinate.ToString();
		obj[4] = "}}";
		return string.Concat(obj);
	}

	public static bool operator ==(VertexPositionTexture left, VertexPositionTexture right)
	{
		if (left.Position == right.Position)
		{
			return left.TextureCoordinate == right.TextureCoordinate;
		}
		return false;
	}

	public static bool operator !=(VertexPositionTexture left, VertexPositionTexture right)
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
		return this == (VertexPositionTexture)obj;
	}

	static VertexPositionTexture()
	{
		VertexPositionTexture.VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));
	}
}
