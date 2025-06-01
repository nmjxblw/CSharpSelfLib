using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPositionNormalTexture : IVertexType
{
	public Vector3 Position;

	public Vector3 Normal;

	public Vector2 TextureCoordinate;

	public static readonly VertexDeclaration VertexDeclaration;

	VertexDeclaration IVertexType.VertexDeclaration => VertexPositionNormalTexture.VertexDeclaration;

	public VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
	{
		this.Position = position;
		this.Normal = normal;
		this.TextureCoordinate = textureCoordinate;
	}

	public override int GetHashCode()
	{
		return (((this.Position.GetHashCode() * 397) ^ this.Normal.GetHashCode()) * 397) ^ this.TextureCoordinate.GetHashCode();
	}

	public override string ToString()
	{
		string[] obj = new string[7] { "{{Position:", null, null, null, null, null, null };
		Vector3 position = this.Position;
		obj[1] = position.ToString();
		obj[2] = " Normal:";
		position = this.Normal;
		obj[3] = position.ToString();
		obj[4] = " TextureCoordinate:";
		Vector2 textureCoordinate = this.TextureCoordinate;
		obj[5] = textureCoordinate.ToString();
		obj[6] = "}}";
		return string.Concat(obj);
	}

	public static bool operator ==(VertexPositionNormalTexture left, VertexPositionNormalTexture right)
	{
		if (left.Position == right.Position && left.Normal == right.Normal)
		{
			return left.TextureCoordinate == right.TextureCoordinate;
		}
		return false;
	}

	public static bool operator !=(VertexPositionNormalTexture left, VertexPositionNormalTexture right)
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
		return this == (VertexPositionNormalTexture)obj;
	}

	static VertexPositionNormalTexture()
	{
		VertexPositionNormalTexture.VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0), new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));
	}
}
