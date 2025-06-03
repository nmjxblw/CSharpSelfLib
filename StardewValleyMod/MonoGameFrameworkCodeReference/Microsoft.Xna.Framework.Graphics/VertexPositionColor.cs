using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[DataContract]
public struct VertexPositionColor : IVertexType
{
	[DataMember]
	public Vector3 Position;

	[DataMember]
	public Color Color;

	public static readonly VertexDeclaration VertexDeclaration;

	VertexDeclaration IVertexType.VertexDeclaration => VertexPositionColor.VertexDeclaration;

	public VertexPositionColor(Vector3 position, Color color)
	{
		this.Position = position;
		this.Color = color;
	}

	public override int GetHashCode()
	{
		return (this.Position.GetHashCode() * 397) ^ this.Color.GetHashCode();
	}

	public override string ToString()
	{
		string[] obj = new string[5] { "{{Position:", null, null, null, null };
		Vector3 position = this.Position;
		obj[1] = position.ToString();
		obj[2] = " Color:";
		Color color = this.Color;
		obj[3] = color.ToString();
		obj[4] = "}}";
		return string.Concat(obj);
	}

	public static bool operator ==(VertexPositionColor left, VertexPositionColor right)
	{
		if (left.Color == right.Color)
		{
			return left.Position == right.Position;
		}
		return false;
	}

	public static bool operator !=(VertexPositionColor left, VertexPositionColor right)
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
		return this == (VertexPositionColor)obj;
	}

	static VertexPositionColor()
	{
		VertexPositionColor.VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0));
	}
}
