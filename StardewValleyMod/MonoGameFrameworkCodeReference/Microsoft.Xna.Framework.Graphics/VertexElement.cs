using System;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Defines a single element in a vertex.
/// </summary>
public struct VertexElement : IEquatable<VertexElement>
{
	private int _offset;

	private VertexElementFormat _format;

	private VertexElementUsage _usage;

	private int _usageIndex;

	/// <summary>
	/// Gets or sets the offset in bytes from the beginning of the stream to the vertex element.
	/// </summary>
	/// <value>The offset in bytes.</value>
	public int Offset
	{
		get
		{
			return this._offset;
		}
		set
		{
			this._offset = value;
		}
	}

	/// <summary>
	/// Gets or sets the data format.
	/// </summary>
	/// <value>The data format.</value>
	public VertexElementFormat VertexElementFormat
	{
		get
		{
			return this._format;
		}
		set
		{
			this._format = value;
		}
	}

	/// <summary>
	/// Gets or sets the HLSL semantic of the element in the vertex shader input.
	/// </summary>
	/// <value>The HLSL semantic of the element in the vertex shader input.</value>
	public VertexElementUsage VertexElementUsage
	{
		get
		{
			return this._usage;
		}
		set
		{
			this._usage = value;
		}
	}

	/// <summary>
	/// Gets or sets the semantic index.
	/// </summary>
	/// <value>
	/// The semantic index, which is required if the semantic is used for more than one vertex
	/// element.
	/// </value>
	/// <remarks>
	/// Usage indices in a vertex declaration usually start with 0. When multiple vertex buffers
	/// are bound to the input assembler stage (see <see cref="M:Microsoft.Xna.Framework.Graphics.GraphicsDevice.SetVertexBuffers(Microsoft.Xna.Framework.Graphics.VertexBufferBinding[])" />),
	/// MonoGame internally adjusts the usage indices based on the order in which the vertex
	/// buffers are bound.
	/// </remarks>
	public int UsageIndex
	{
		get
		{
			return this._usageIndex;
		}
		set
		{
			this._usageIndex = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Graphics.VertexElement" /> struct.
	/// </summary>
	/// <param name="offset">The offset in bytes from the beginning of the stream to the vertex element.</param>
	/// <param name="elementFormat">The element format.</param>
	/// <param name="elementUsage">The HLSL semantic of the element in the vertex shader input-signature.</param>
	/// <param name="usageIndex">The semantic index, which is required if the semantic is used for more than one vertex element.</param>
	public VertexElement(int offset, VertexElementFormat elementFormat, VertexElementUsage elementUsage, int usageIndex)
	{
		this._offset = offset;
		this._format = elementFormat;
		this._usageIndex = usageIndex;
		this._usage = elementUsage;
	}

	/// <summary>
	/// Returns a hash code for this instance.
	/// </summary>
	/// <returns>
	/// A hash code for this instance, suitable for use in hashing algorithms and data
	/// structures like a hash table.
	/// </returns>
	public override int GetHashCode()
	{
		return this._offset ^ ((int)this._format << 9) ^ ((int)this._usage << 13) ^ (this._usageIndex << 17);
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> that represents this instance.
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> that represents this instance.</returns>
	public override string ToString()
	{
		return "{Offset:" + this._offset + " Format:" + this._format.ToString() + " Usage:" + this._usage.ToString() + " UsageIndex: " + this._usageIndex + "}";
	}

	/// <summary>
	/// Determines whether the specified <see cref="T:System.Object" /> is equal to this instance.
	/// </summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>
	/// <see langword="true" /> if the specified <see cref="T:System.Object" /> is equal to this instance;
	/// otherwise, <see langword="false" />.
	/// </returns>
	public override bool Equals(object obj)
	{
		if (obj is VertexElement)
		{
			return this.Equals((VertexElement)obj);
		}
		return false;
	}

	/// <summary>
	/// Determines whether the specified <see cref="T:Microsoft.Xna.Framework.Graphics.VertexElement" /> is equal to this
	/// instance.
	/// </summary>
	/// <param name="other">The object to compare with the current object.</param>
	/// <returns>
	/// <see langword="true" /> if the specified <see cref="T:Microsoft.Xna.Framework.Graphics.VertexElement" /> is equal to this
	/// instance; otherwise, <see langword="false" />.
	/// </returns>
	public bool Equals(VertexElement other)
	{
		if (this._offset == other._offset && this._format == other._format && this._usage == other._usage)
		{
			return this._usageIndex == other._usageIndex;
		}
		return false;
	}

	/// <summary>
	/// Compares two <see cref="T:Microsoft.Xna.Framework.Graphics.VertexElement" /> instances to determine whether they are the
	/// same.
	/// </summary>
	/// <param name="left">The first instance.</param>
	/// <param name="right">The second instance.</param>
	/// <returns>
	/// <see langword="true" /> if the <paramref name="left" /> and <paramref name="right" /> are
	/// the same; otherwise, <see langword="false" />.
	/// </returns>
	public static bool operator ==(VertexElement left, VertexElement right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// Compares two <see cref="T:Microsoft.Xna.Framework.Graphics.VertexElement" /> instances to determine whether they are
	/// different.
	/// </summary>
	/// <param name="left">The first instance.</param>
	/// <param name="right">The second instance.</param>
	/// <returns>
	/// <see langword="true" /> if the <paramref name="left" /> and <paramref name="right" /> are
	/// the different; otherwise, <see langword="false" />.
	/// </returns>
	public static bool operator !=(VertexElement left, VertexElement right)
	{
		return !left.Equals(right);
	}
}
