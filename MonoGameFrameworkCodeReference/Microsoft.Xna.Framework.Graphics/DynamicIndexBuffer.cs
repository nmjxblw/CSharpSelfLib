using System;

namespace Microsoft.Xna.Framework.Graphics;

public class DynamicIndexBuffer : IndexBuffer
{
	/// <summary>
	/// Special offset used internally by GraphicsDevice.DrawUserXXX() methods.
	/// </summary>
	internal int UserOffset;

	public bool IsContentLost => false;

	public DynamicIndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
		: base(graphicsDevice, indexElementSize, indexCount, usage, dynamic: true)
	{
	}

	public DynamicIndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage)
		: base(graphicsDevice, indexType, indexCount, usage, dynamic: true)
	{
	}

	public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
	{
		base.SetDataInternal(offsetInBytes, data, startIndex, elementCount, options);
	}

	public void SetData<T>(T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
	{
		base.SetDataInternal(0, data, startIndex, elementCount, options);
	}
}
