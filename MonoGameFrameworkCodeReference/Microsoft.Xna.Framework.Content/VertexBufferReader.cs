using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

internal class VertexBufferReader : ContentTypeReader<VertexBuffer>
{
	protected internal override VertexBuffer Read(ContentReader input, VertexBuffer existingInstance)
	{
		VertexDeclaration declaration = input.ReadRawObject<VertexDeclaration>();
		int vertexCount = (int)input.ReadUInt32();
		int dataSize = vertexCount * declaration.VertexStride;
		byte[] data = ContentManager.ScratchBufferPool.Get(dataSize);
		input.Read(data, 0, dataSize);
		VertexBuffer vertexBuffer = new VertexBuffer(input.GetGraphicsDevice(), declaration, vertexCount, BufferUsage.None);
		vertexBuffer.SetData(data, 0, dataSize);
		ContentManager.ScratchBufferPool.Return(data);
		return vertexBuffer;
	}
}
