using System;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

public class IndexBuffer : GraphicsResource
{
	private struct SetDataState<T> where T : struct
	{
		public IndexBuffer buffer;

		public int offsetInBytes;

		public T[] data;

		public int startIndex;

		public int elementCount;

		public SetDataOptions options;

		public static Action<SetDataState<T>> Action = delegate(SetDataState<T> s)
		{
			s.buffer.PlatformSetDataBody(s.offsetInBytes, s.data, s.startIndex, s.elementCount, s.options);
		};
	}

	private readonly bool _isDynamic;

	internal int ibo;

	public BufferUsage BufferUsage { get; private set; }

	public int IndexCount { get; private set; }

	public IndexElementSize IndexElementSize { get; private set; }

	protected IndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage, bool dynamic)
		: this(graphicsDevice, IndexBuffer.SizeForType(graphicsDevice, indexType), indexCount, usage, dynamic)
	{
	}

	protected IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage usage, bool dynamic)
	{
		if (graphicsDevice == null)
		{
			throw new ArgumentNullException("graphicsDevice", "The GraphicsDevice must not be null when creating new resources.");
		}
		base.GraphicsDevice = graphicsDevice;
		this.IndexElementSize = indexElementSize;
		this.IndexCount = indexCount;
		this.BufferUsage = usage;
		this._isDynamic = dynamic;
		this.PlatformConstruct(indexElementSize, indexCount);
	}

	public IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage bufferUsage)
		: this(graphicsDevice, indexElementSize, indexCount, bufferUsage, dynamic: false)
	{
	}

	public IndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage)
		: this(graphicsDevice, IndexBuffer.SizeForType(graphicsDevice, indexType), indexCount, usage, dynamic: false)
	{
	}

	/// <summary>
	/// Gets the relevant IndexElementSize enum value for the given type.
	/// </summary>
	/// <param name="graphicsDevice">The graphics device.</param>
	/// <param name="type">The type to use for the index buffer</param>
	/// <returns>The IndexElementSize enum value that matches the type</returns>
	private static IndexElementSize SizeForType(GraphicsDevice graphicsDevice, Type type)
	{
		switch (ReflectionHelpers.ManagedSizeOf(type))
		{
		case 2:
			return IndexElementSize.SixteenBits;
		case 4:
			if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
			{
				throw new NotSupportedException("The profile does not support an elementSize of IndexElementSize.ThirtyTwoBits; use IndexElementSize.SixteenBits or a type that has a size of two bytes.");
			}
			return IndexElementSize.ThirtyTwoBits;
		default:
			throw new ArgumentOutOfRangeException("type", "Index buffers can only be created for types that are sixteen or thirty two bits in length");
		}
	}

	/// <summary>
	/// The GraphicsDevice is resetting, so GPU resources must be recreated.
	/// </summary>
	protected internal override void GraphicsDeviceResetting()
	{
		this.PlatformGraphicsDeviceResetting();
	}

	public void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length < startIndex + elementCount)
		{
			throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");
		}
		if (this.BufferUsage == BufferUsage.WriteOnly)
		{
			throw new NotSupportedException("This IndexBuffer was created with a usage type of BufferUsage.WriteOnly. Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");
		}
		this.PlatformGetData(offsetInBytes, data, startIndex, elementCount);
	}

	public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
	{
		this.GetData(0, data, startIndex, elementCount);
	}

	public void GetData<T>(T[] data) where T : struct
	{
		this.GetData(0, data, 0, data.Length);
	}

	public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
	{
		this.SetDataInternal(offsetInBytes, data, startIndex, elementCount, SetDataOptions.None);
	}

	public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
	{
		this.SetDataInternal(0, data, startIndex, elementCount, SetDataOptions.None);
	}

	public void SetData<T>(T[] data) where T : struct
	{
		this.SetDataInternal(0, data, 0, data.Length, SetDataOptions.None);
	}

	protected void SetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length < startIndex + elementCount)
		{
			throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");
		}
		this.PlatformSetData(offsetInBytes, data, startIndex, elementCount, options);
	}

	private void PlatformConstruct(IndexElementSize indexElementSize, int indexCount)
	{
		Threading.BlockOnUIThread(GenerateIfRequired);
	}

	private void PlatformGraphicsDeviceResetting()
	{
		this.ibo = 0;
	}

	/// <summary>
	/// If the IBO does not exist, create it.
	/// </summary>
	private void GenerateIfRequired()
	{
		if (this.ibo == 0)
		{
			int sizeInBytes = this.IndexCount * ((this.IndexElementSize == IndexElementSize.SixteenBits) ? 2 : 4);
			GL.GenBuffers(1, out this.ibo);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.ibo);
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)sizeInBytes, IntPtr.Zero, this._isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
		}
	}

	private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
	{
		if (Threading.IsOnUIThread())
		{
			this.GetBufferData(offsetInBytes, data, startIndex, elementCount);
			return;
		}
		Threading.BlockOnUIThread(delegate
		{
			this.GetBufferData(offsetInBytes, data, startIndex, elementCount);
		});
	}

	private void GetBufferData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
	{
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.ibo);
		int elementSizeInByte = Marshal.SizeOf<T>();
		IntPtr ptr = new IntPtr(GL.MapBuffer(BufferTarget.ElementArrayBuffer, BufferAccess.ReadOnly).ToInt64() + offsetInBytes);
		if (typeof(T) == typeof(byte))
		{
			byte[] buffer = data as byte[];
			Marshal.Copy(ptr, buffer, startIndex * elementSizeInByte, elementCount * elementSizeInByte);
		}
		else
		{
			byte[] buffer2 = new byte[elementCount * elementSizeInByte];
			Marshal.Copy(ptr, buffer2, 0, buffer2.Length);
			Buffer.BlockCopy(buffer2, 0, data, startIndex * elementSizeInByte, elementCount * elementSizeInByte);
		}
		GL.UnmapBuffer(BufferTarget.ElementArrayBuffer);
	}

	private void PlatformSetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
	{
		Threading.BlockOnUIThread(SetDataState<T>.Action, new SetDataState<T>
		{
			buffer = this,
			offsetInBytes = offsetInBytes,
			data = data,
			startIndex = startIndex,
			elementCount = elementCount,
			options = options
		});
	}

	private void PlatformSetDataBody<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
	{
		this.GenerateIfRequired();
		int elementSizeInByte = Marshal.SizeOf<T>();
		int sizeInBytes = elementSizeInByte * elementCount;
		GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
		try
		{
			IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);
			int bufferSize = this.IndexCount * ((this.IndexElementSize == IndexElementSize.SixteenBits) ? 2 : 4);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.ibo);
			if (options == SetDataOptions.Discard)
			{
				GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)bufferSize, IntPtr.Zero, this._isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
			}
			GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, dataPtr);
		}
		finally
		{
			dataHandle.Free();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed && base.GraphicsDevice != null)
		{
			base.GraphicsDevice.DisposeBuffer(this.ibo);
		}
		base.Dispose(disposing);
	}
}
