using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

internal class VertexDeclarationReader : ContentTypeReader<VertexDeclaration>
{
	protected internal override VertexDeclaration Read(ContentReader reader, VertexDeclaration existingInstance)
	{
		int vertexStride = reader.ReadInt32();
		int elementCount = reader.ReadInt32();
		VertexElement[] elements = new VertexElement[elementCount];
		for (int i = 0; i < elementCount; i++)
		{
			int offset = reader.ReadInt32();
			VertexElementFormat elementFormat = (VertexElementFormat)reader.ReadInt32();
			VertexElementUsage elementUsage = (VertexElementUsage)reader.ReadInt32();
			int usageIndex = reader.ReadInt32();
			elements[i] = new VertexElement(offset, elementFormat, elementUsage, usageIndex);
		}
		return VertexDeclaration.GetOrCreate(vertexStride, elements);
	}
}
