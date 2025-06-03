using System;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics;

public class DynamicVertexBuffer : VertexBuffer
{
	/// <summary>
	/// Special offset used internally by GraphicsDevice.DrawUserXXX() methods.
	/// </summary>
	internal int UserOffset;

	public bool IsContentLost => false;

	public DynamicVertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage)
		: base(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage, dynamic: true)
	{
	}

	public DynamicVertexBuffer(GraphicsDevice graphicsDevice, Type type, int vertexCount, BufferUsage bufferUsage)
		: base(graphicsDevice, VertexDeclaration.FromType(type), vertexCount, bufferUsage, dynamic: true)
	{
	}

	public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options) where T : struct
	{
		base.SetDataInternal(offsetInBytes, data, startIndex, elementCount, vertexStride, options);
	}

	public void SetData<T>(T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
	{
		int elementSizeInBytes = ReflectionHelpers.SizeOf<T>.Get();
		base.SetDataInternal(0, data, startIndex, elementCount, elementSizeInBytes, options);
	}
}
