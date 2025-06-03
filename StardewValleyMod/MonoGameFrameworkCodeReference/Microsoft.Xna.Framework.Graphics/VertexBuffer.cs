using System;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

public class VertexBuffer : GraphicsResource
{
	private struct SetDataState<T> where T : struct
	{
		public VertexBuffer buffer;

		public int offsetInBytes;

		public T[] data;

		public int startIndex;

		public int elementCount;

		public int vertexStride;

		public SetDataOptions options;

		public int bufferSize;

		public int elementSizeInBytes;

		public static Action<SetDataState<T>> Action = delegate(SetDataState<T> s)
		{
			s.buffer.PlatformSetDataBody(s.offsetInBytes, s.data, s.startIndex, s.elementCount, s.vertexStride, s.options, s.bufferSize, s.elementSizeInBytes);
		};
	}

	private readonly bool _isDynamic;

	internal int vbo;

	public int VertexCount { get; private set; }

	public VertexDeclaration VertexDeclaration { get; private set; }

	public BufferUsage BufferUsage { get; private set; }

	protected VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage, bool dynamic)
	{
		if (graphicsDevice == null)
		{
			throw new ArgumentNullException("graphicsDevice", "The GraphicsDevice must not be null when creating new resources.");
		}
		base.GraphicsDevice = graphicsDevice;
		this.VertexDeclaration = vertexDeclaration;
		this.VertexCount = vertexCount;
		this.BufferUsage = bufferUsage;
		if (vertexDeclaration.GraphicsDevice != graphicsDevice)
		{
			vertexDeclaration.GraphicsDevice = graphicsDevice;
		}
		this._isDynamic = dynamic;
		this.PlatformConstruct();
	}

	public VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage)
		: this(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage, dynamic: false)
	{
	}

	public VertexBuffer(GraphicsDevice graphicsDevice, Type type, int vertexCount, BufferUsage bufferUsage)
		: this(graphicsDevice, VertexDeclaration.FromType(type), vertexCount, bufferUsage, dynamic: false)
	{
	}

	/// <summary>
	/// The GraphicsDevice is resetting, so GPU resources must be recreated.
	/// </summary>
	protected internal override void GraphicsDeviceResetting()
	{
		this.PlatformGraphicsDeviceResetting();
	}

	/// <summary>
	/// Get the vertex data froom this VertexBuffer.
	/// </summary>
	/// <typeparam name="T">The struct you want to fill.</typeparam>
	/// <param name="offsetInBytes">The offset to the first element in the vertex buffer in bytes.</param>
	/// <param name="data">An array of T's to be filled.</param>
	/// <param name="startIndex">The index to start filling the data array.</param>
	/// <param name="elementCount">The number of T's to get.</param>
	/// <param name="vertexStride">The size of how a vertex buffer element should be interpreted.</param>
	///
	/// <remarks>
	/// Note that this pulls data from VRAM into main memory and because of that is a very expensive operation.
	/// It is often a better idea to keep a copy of the data in main memory.
	/// </remarks>
	///
	/// <remarks>
	/// <p>Using this operation it is easy to get certain vertex elements from a VertexBuffer.</p>
	/// <p>
	/// For example to get the texture coordinates from a VertexBuffer of <see cref="T:Microsoft.Xna.Framework.Graphics.VertexPositionTexture" /> you can call 
	/// GetData(4 * 3, data, elementCount, 20). 'data'should be an array of <see cref="T:Microsoft.Xna.Framework.Vector2" /> in this example.
	/// The offsetInBytes is the number of bytes taken up by the <see cref="F:Microsoft.Xna.Framework.Graphics.VertexPositionTexture.Position" /> of the vertex.
	/// For vertexStride we pass the size of a <see cref="T:Microsoft.Xna.Framework.Graphics.VertexPositionTexture" />.
	/// </p>
	/// </remarks>
	public void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride = 0) where T : struct
	{
		int elementSizeInBytes = ReflectionHelpers.SizeOf<T>.Get();
		if (vertexStride == 0)
		{
			vertexStride = elementSizeInBytes;
		}
		int vertexByteSize = this.VertexCount * this.VertexDeclaration.VertexStride;
		if (vertexStride > vertexByteSize)
		{
			throw new ArgumentOutOfRangeException("vertexStride", "Vertex stride can not be larger than the vertex buffer size.");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length < startIndex + elementCount)
		{
			throw new ArgumentOutOfRangeException("elementCount", "This parameter must be a valid index within the array.");
		}
		if (this.BufferUsage == BufferUsage.WriteOnly)
		{
			throw new NotSupportedException("Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");
		}
		if (elementCount > 1 && elementCount * vertexStride > vertexByteSize)
		{
			throw new InvalidOperationException("The array is not the correct size for the amount of data requested.");
		}
		this.PlatformGetData(offsetInBytes, data, startIndex, elementCount, vertexStride);
	}

	public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
	{
		this.GetData(0, data, startIndex, elementCount);
	}

	public void GetData<T>(T[] data) where T : struct
	{
		int elementSizeInByte = ReflectionHelpers.SizeOf<T>.Get();
		this.GetData(0, data, 0, data.Length, elementSizeInByte);
	}

	/// <summary>
	/// Sets the vertex buffer data, specifying the index at which to start copying from the source data array,
	/// the number of elements to copy from the source data array, 
	/// and how far apart elements from the source data array should be when they are copied into the vertex buffer.
	/// </summary>
	/// <typeparam name="T">Type of elements in the data array.</typeparam>
	/// <param name="offsetInBytes">Offset in bytes from the beginning of the vertex buffer to the start of the copied data.</param>
	/// <param name="data">Data array.</param>
	/// <param name="startIndex">Index at which to start copying from <paramref name="data" />.
	/// Must be within the <paramref name="data" /> array bounds.</param>
	/// <param name="elementCount">Number of elements to copy from <paramref name="data" />.
	/// The combination of <paramref name="startIndex" /> and <paramref name="elementCount" /> 
	/// must be within the <paramref name="data" /> array bounds.</param>
	/// <param name="vertexStride">Specifies how far apart, in bytes, elements from <paramref name="data" /> should be when 
	/// they are copied into the vertex buffer.
	/// In almost all cases this should be <c>sizeof(T)</c>, to create a tightly-packed vertex buffer.
	/// If you specify <c>sizeof(T)</c>, elements from <paramref name="data" /> will be copied into the 
	/// vertex buffer with no padding between each element.
	/// If you specify a value greater than <c>sizeof(T)</c>, elements from <paramref name="data" /> will be copied 
	/// into the vertex buffer with padding between each element.
	/// If you specify <c>0</c> for this parameter, it will be treated as if you had specified <c>sizeof(T)</c>.
	/// With the exception of <c>0</c>, you must specify a value greater than or equal to <c>sizeof(T)</c>.</param>
	/// <remarks>
	/// If <c>T</c> is <c>VertexPositionTexture</c>, but you want to set only the position component of the vertex data,
	/// you would call this method as follows:
	/// <code>
	/// Vector3[] positions = new Vector3[numVertices];
	/// vertexBuffer.SetData(0, positions, 0, numVertices, vertexBuffer.VertexDeclaration.VertexStride);
	/// </code>
	///
	/// Continuing from the previous example, if you want to set only the texture coordinate component of the vertex data,
	/// you would call this method as follows (note the use of <paramref name="offsetInBytes" />:
	/// <code>
	/// Vector2[] texCoords = new Vector2[numVertices];
	/// vertexBuffer.SetData(12, texCoords, 0, numVertices, vertexBuffer.VertexDeclaration.VertexStride);
	/// </code>
	/// </remarks>
	/// <remarks>
	/// If you provide a <c>byte[]</c> in the <paramref name="data" /> parameter, then you should almost certainly
	/// set <paramref name="vertexStride" /> to <c>1</c>, to avoid leaving any padding between the <c>byte</c> values
	/// when they are copied into the vertex buffer.
	/// </remarks>
	public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
	{
		this.SetDataInternal(offsetInBytes, data, startIndex, elementCount, vertexStride, SetDataOptions.None);
	}

	/// <summary>
	/// Sets the vertex buffer data, specifying the index at which to start copying from the source data array,
	/// and the number of elements to copy from the source data array. This is the same as calling 
	/// <see cref="M:Microsoft.Xna.Framework.Graphics.VertexBuffer.SetData``1(System.Int32,``0[],System.Int32,System.Int32,System.Int32)" />  with <c>offsetInBytes</c> equal to <c>0</c>,
	/// and <c>vertexStride</c> equal to <c>sizeof(T)</c>.
	/// </summary>
	/// <typeparam name="T">Type of elements in the data array.</typeparam>
	/// <param name="data">Data array.</param>
	/// <param name="startIndex">Index at which to start copying from <paramref name="data" />.
	/// Must be within the <paramref name="data" /> array bounds.</param>
	/// <param name="elementCount">Number of elements to copy from <paramref name="data" />.
	/// The combination of <paramref name="startIndex" /> and <paramref name="elementCount" /> 
	/// must be within the <paramref name="data" /> array bounds.</param>
	public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
	{
		int elementSizeInBytes = ReflectionHelpers.SizeOf<T>.Get();
		this.SetDataInternal(0, data, startIndex, elementCount, elementSizeInBytes, SetDataOptions.None);
	}

	/// <summary>
	/// Sets the vertex buffer data. This is the same as calling <see cref="M:Microsoft.Xna.Framework.Graphics.VertexBuffer.SetData``1(System.Int32,``0[],System.Int32,System.Int32,System.Int32)" /> 
	/// with <c>offsetInBytes</c> and <c>startIndex</c> equal to <c>0</c>, <c>elementCount</c> equal to <c>data.Length</c>, 
	/// and <c>vertexStride</c> equal to <c>sizeof(T)</c>.
	/// </summary>
	/// <typeparam name="T">Type of elements in the data array.</typeparam>
	/// <param name="data">Data array.</param>
	public void SetData<T>(T[] data) where T : struct
	{
		int elementSizeInBytes = ReflectionHelpers.SizeOf<T>.Get();
		this.SetDataInternal(0, data, 0, data.Length, elementSizeInBytes, SetDataOptions.None);
	}

	protected void SetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options) where T : struct
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		int elementSizeInBytes = ReflectionHelpers.SizeOf<T>.Get();
		int bufferSize = this.VertexCount * this.VertexDeclaration.VertexStride;
		if (vertexStride == 0)
		{
			vertexStride = elementSizeInBytes;
		}
		int vertexByteSize = this.VertexCount * this.VertexDeclaration.VertexStride;
		if (vertexStride > vertexByteSize)
		{
			throw new ArgumentOutOfRangeException("vertexStride", "Vertex stride can not be larger than the vertex buffer size.");
		}
		if (startIndex + elementCount > data.Length || elementCount <= 0)
		{
			throw new ArgumentOutOfRangeException("data", "The array specified in the data parameter is not the correct size for the amount of data requested.");
		}
		if (elementCount > 1 && elementCount * vertexStride > bufferSize)
		{
			throw new InvalidOperationException("The vertex stride is larger than the vertex buffer.");
		}
		if (vertexStride < elementSizeInBytes)
		{
			throw new ArgumentOutOfRangeException("The vertex stride must be greater than or equal to the size of the specified data (" + elementSizeInBytes + ").");
		}
		this.PlatformSetData(offsetInBytes, data, startIndex, elementCount, vertexStride, options, bufferSize, elementSizeInBytes);
	}

	private void PlatformConstruct()
	{
		Threading.BlockOnUIThread(GenerateIfRequired);
	}

	private void PlatformGraphicsDeviceResetting()
	{
		this.vbo = 0;
	}

	/// <summary>
	/// If the VBO does not exist, create it.
	/// </summary>
	private void GenerateIfRequired()
	{
		if (this.vbo == 0)
		{
			GL.GenBuffers(1, out this.vbo);
			GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
			GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(this.VertexDeclaration.VertexStride * this.VertexCount), IntPtr.Zero, this._isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
		}
	}

	private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
	{
		Threading.BlockOnUIThread(delegate
		{
			this.GetBufferData(offsetInBytes, data, startIndex, elementCount, vertexStride);
		});
	}

	private void GetBufferData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
	{
		GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
		IntPtr ptr = (IntPtr)(GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadOnly).ToInt64() + offsetInBytes);
		if (typeof(T) == typeof(byte) && vertexStride == 1)
		{
			byte[] buffer = data as byte[];
			Marshal.Copy(ptr, buffer, startIndex * vertexStride, elementCount * vertexStride);
		}
		else
		{
			byte[] tmp = new byte[elementCount * vertexStride];
			Marshal.Copy(ptr, tmp, 0, tmp.Length);
			GCHandle tmpHandle = GCHandle.Alloc(tmp, GCHandleType.Pinned);
			try
			{
				IntPtr tmpPtr = tmpHandle.AddrOfPinnedObject();
				for (int i = 0; i < elementCount; i++)
				{
					data[startIndex + i] = (T)Marshal.PtrToStructure(tmpPtr, typeof(T));
					tmpPtr = (IntPtr)(tmpPtr.ToInt64() + vertexStride);
				}
			}
			finally
			{
				tmpHandle.Free();
			}
		}
		GL.UnmapBuffer(BufferTarget.ArrayBuffer);
	}

	private void PlatformSetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes) where T : struct
	{
		Threading.BlockOnUIThread(SetDataState<T>.Action, new SetDataState<T>
		{
			buffer = this,
			offsetInBytes = offsetInBytes,
			data = data,
			startIndex = startIndex,
			elementCount = elementCount,
			vertexStride = vertexStride,
			options = options,
			bufferSize = bufferSize,
			elementSizeInBytes = elementSizeInBytes
		});
	}

	private void PlatformSetDataBody<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes) where T : struct
	{
		this.GenerateIfRequired();
		GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
		if (options == SetDataOptions.Discard)
		{
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)bufferSize, IntPtr.Zero, this._isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
		}
		int elementSizeInByte = Marshal.SizeOf<T>();
		if (elementSizeInByte == vertexStride || elementSizeInByte % vertexStride == 0)
		{
			GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			try
			{
				IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInBytes);
				GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)(elementSizeInBytes * elementCount), dataPtr);
				return;
			}
			finally
			{
				dataHandle.Free();
			}
		}
		GCHandle dataHandle2 = GCHandle.Alloc(data, GCHandleType.Pinned);
		try
		{
			int dstOffset = offsetInBytes;
			IntPtr dataPtr2 = (IntPtr)(dataHandle2.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);
			for (int i = 0; i < elementCount; i++)
			{
				GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)dstOffset, (IntPtr)elementSizeInByte, dataPtr2);
				dstOffset += vertexStride;
				dataPtr2 = (IntPtr)(dataPtr2.ToInt64() + elementSizeInByte);
			}
		}
		finally
		{
			dataHandle2.Free();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed && base.GraphicsDevice != null)
		{
			base.GraphicsDevice.DisposeBuffer(this.vbo);
		}
		base.Dispose(disposing);
	}
}
