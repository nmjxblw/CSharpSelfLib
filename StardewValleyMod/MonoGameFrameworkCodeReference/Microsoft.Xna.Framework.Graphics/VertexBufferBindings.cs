using System;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Stores the vertex buffers to be bound to the input assembler stage.
/// </summary>
internal sealed class VertexBufferBindings : VertexInputLayout
{
	private readonly VertexBuffer[] _vertexBuffers;

	private readonly int[] _vertexOffsets;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Graphics.VertexBufferBindings" /> class.
	/// </summary>
	/// <param name="maxVertexBufferSlots">The maximum number of vertex buffer slots.</param>
	public VertexBufferBindings(int maxVertexBufferSlots)
		: base(new VertexDeclaration[maxVertexBufferSlots], new int[maxVertexBufferSlots], 0)
	{
		this._vertexBuffers = new VertexBuffer[maxVertexBufferSlots];
		this._vertexOffsets = new int[maxVertexBufferSlots];
	}

	/// <summary>
	/// Clears the vertex buffer slots.
	/// </summary>
	/// <returns>
	/// <see langword="true" /> if the input layout was changed; otherwise,
	/// <see langword="false" />.
	/// </returns>
	public bool Clear()
	{
		if (base.Count == 0)
		{
			return false;
		}
		Array.Clear(base.VertexDeclarations, 0, base.Count);
		Array.Clear(base.InstanceFrequencies, 0, base.Count);
		Array.Clear(this._vertexBuffers, 0, base.Count);
		Array.Clear(this._vertexOffsets, 0, base.Count);
		base.Count = 0;
		return true;
	}

	/// <summary>
	/// Binds the specified vertex buffer to the first input slot.
	/// </summary>
	/// <param name="vertexBuffer">The vertex buffer.</param>
	/// <param name="vertexOffset">
	/// The offset (in vertices) from the beginning of the vertex buffer to the first vertex to 
	/// use.
	/// </param>
	/// <returns>
	/// <see langword="true" /> if the input layout was changed; otherwise,
	/// <see langword="false" />.
	/// </returns>
	public bool Set(VertexBuffer vertexBuffer, int vertexOffset)
	{
		if (base.Count == 1 && base.InstanceFrequencies[0] == 0 && this._vertexBuffers[0] == vertexBuffer && this._vertexOffsets[0] == vertexOffset)
		{
			return false;
		}
		base.VertexDeclarations[0] = vertexBuffer.VertexDeclaration;
		base.InstanceFrequencies[0] = 0;
		this._vertexBuffers[0] = vertexBuffer;
		this._vertexOffsets[0] = vertexOffset;
		if (base.Count > 1)
		{
			Array.Clear(base.VertexDeclarations, 1, base.Count - 1);
			Array.Clear(base.InstanceFrequencies, 1, base.Count - 1);
			Array.Clear(this._vertexBuffers, 1, base.Count - 1);
			Array.Clear(this._vertexOffsets, 1, base.Count - 1);
		}
		base.Count = 1;
		return true;
	}

	/// <summary>
	/// Binds the the specified vertex buffers to the input slots.
	/// </summary>
	/// <param name="vertexBufferBindings">The vertex buffer bindings.</param>
	/// <returns>
	/// <see langword="true" /> if the input layout was changed; otherwise,
	/// <see langword="false" />.
	/// </returns>
	public bool Set(params VertexBufferBinding[] vertexBufferBindings)
	{
		bool isDirty = false;
		for (int i = 0; i < vertexBufferBindings.Length; i++)
		{
			if (base.InstanceFrequencies[i] != vertexBufferBindings[i].InstanceFrequency || this._vertexBuffers[i] != vertexBufferBindings[i].VertexBuffer || this._vertexOffsets[i] != vertexBufferBindings[i].VertexOffset)
			{
				base.VertexDeclarations[i] = vertexBufferBindings[i].VertexBuffer.VertexDeclaration;
				base.InstanceFrequencies[i] = vertexBufferBindings[i].InstanceFrequency;
				this._vertexBuffers[i] = vertexBufferBindings[i].VertexBuffer;
				this._vertexOffsets[i] = vertexBufferBindings[i].VertexOffset;
				isDirty = true;
			}
		}
		if (base.Count > vertexBufferBindings.Length)
		{
			int startIndex = vertexBufferBindings.Length;
			int length = base.Count - startIndex;
			Array.Clear(base.VertexDeclarations, startIndex, length);
			Array.Clear(base.InstanceFrequencies, startIndex, length);
			Array.Clear(this._vertexBuffers, startIndex, length);
			Array.Clear(this._vertexOffsets, startIndex, length);
			isDirty = true;
		}
		base.Count = vertexBufferBindings.Length;
		return isDirty;
	}

	/// <summary>
	/// Gets vertex buffer bound to the specified input slots.
	/// </summary>
	/// <returns>The vertex buffer binding.</returns>
	public VertexBufferBinding Get(int slot)
	{
		return new VertexBufferBinding(this._vertexBuffers[slot], this._vertexOffsets[slot], base.InstanceFrequencies[slot]);
	}

	/// <summary>
	/// Gets vertex buffers bound to the input slots.
	/// </summary>
	/// <returns>The vertex buffer bindings.</returns>
	public VertexBufferBinding[] Get()
	{
		VertexBufferBinding[] bindings = new VertexBufferBinding[base.Count];
		for (int i = 0; i < bindings.Length; i++)
		{
			bindings[i] = new VertexBufferBinding(this._vertexBuffers[i], this._vertexOffsets[i], base.InstanceFrequencies[i]);
		}
		return bindings;
	}
}
