using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[DataContract]
public struct VertexPosition : IVertexType
{
	[DataMember]
	public Vector3 Position;

	public static readonly VertexDeclaration VertexDeclaration;

	VertexDeclaration IVertexType.VertexDeclaration => VertexPosition.VertexDeclaration;

	public VertexPosition(Vector3 position)
	{
		this.Position = position;
	}

	public override int GetHashCode()
	{
		return this.Position.GetHashCode();
	}

	public override string ToString()
	{
		Vector3 position = this.Position;
		return "{{Position:" + position.ToString() + "}}";
	}

	public static bool operator ==(VertexPosition left, VertexPosition right)
	{
		return left.Position == right.Position;
	}

	public static bool operator !=(VertexPosition left, VertexPosition right)
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
		return this == (VertexPosition)obj;
	}

	static VertexPosition()
	{
		VertexPosition.VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0));
	}
}
