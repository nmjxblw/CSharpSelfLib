using System;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Stores the vertex layout (input elements) for the input assembler stage.
/// </summary>
/// <remarks>
/// In the DirectX version the input layouts are cached in a dictionary. The
/// <see cref="T:Microsoft.Xna.Framework.Graphics.VertexInputLayout" /> is used as the key in the dictionary and therefore needs to
/// implement <see cref="T:System.IEquatable`1" />. Two <see cref="T:Microsoft.Xna.Framework.Graphics.VertexInputLayout" /> instance are
/// considered equal if the vertex layouts are structurally identical.
/// </remarks>
internal abstract class VertexInputLayout : IEquatable<VertexInputLayout>
{
	protected VertexDeclaration[] VertexDeclarations { get; private set; }

	protected int[] InstanceFrequencies { get; private set; }

	/// <summary>
	/// Gets or sets the number of used input slots.
	/// </summary>
	/// <value>The number of used input slots.</value>
	public int Count { get; protected set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Graphics.VertexInputLayout" /> class.
	/// </summary>
	/// <param name="maxVertexBufferSlots">The maximum number of vertex buffer slots.</param>
	protected VertexInputLayout(int maxVertexBufferSlots)
		: this(new VertexDeclaration[maxVertexBufferSlots], new int[maxVertexBufferSlots], 0)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Graphics.VertexInputLayout" /> class.
	/// </summary>
	/// <param name="vertexDeclarations">The array for storing vertex declarations.</param>
	/// <param name="instanceFrequencies">The array for storing instance frequencies.</param>
	/// <param name="count">The number of used slots.</param>
	protected VertexInputLayout(VertexDeclaration[] vertexDeclarations, int[] instanceFrequencies, int count)
	{
		this.Count = count;
		this.VertexDeclarations = vertexDeclarations;
		this.InstanceFrequencies = instanceFrequencies;
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
		return this.Equals(obj as VertexInputLayout);
	}

	/// <summary>
	/// Determines whether the specified <see cref="T:Microsoft.Xna.Framework.Graphics.VertexInputLayout" /> is equal to this
	/// instance.
	/// </summary>
	/// <param name="other">The object to compare with the current object.</param>
	/// <returns>
	/// <see langword="true" /> if the specified <see cref="T:Microsoft.Xna.Framework.Graphics.VertexInputLayout" /> is equal to this
	/// instance; otherwise, <see langword="false" />.
	/// </returns>
	public bool Equals(VertexInputLayout other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (this.Count != other.Count)
		{
			return false;
		}
		for (int i = 0; i < this.Count; i++)
		{
			if (!this.VertexDeclarations[i].Equals(other.VertexDeclarations[i]))
			{
				return false;
			}
		}
		for (int j = 0; j < this.Count; j++)
		{
			if (!this.InstanceFrequencies[j].Equals(other.InstanceFrequencies[j]))
			{
				return false;
			}
		}
		return true;
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
		int hashCode = 0;
		if (this.Count > 0)
		{
			hashCode = this.VertexDeclarations[0].GetHashCode();
			hashCode = (hashCode * 397) ^ this.InstanceFrequencies[0];
			for (int i = 1; i < this.Count; i++)
			{
				hashCode = (hashCode * 397) ^ this.VertexDeclarations[i].GetHashCode();
				hashCode = (hashCode * 397) ^ this.InstanceFrequencies[i];
			}
		}
		return hashCode;
	}

	/// <summary>
	/// Compares two <see cref="T:Microsoft.Xna.Framework.Graphics.VertexInputLayout" /> instances to determine whether they are the
	/// same.
	/// </summary>
	/// <param name="left">The first instance.</param>
	/// <param name="right">The second instance.</param>
	/// <returns>
	/// <see langword="true" /> if the <paramref name="left" /> and <paramref name="right" /> are
	/// the same; otherwise, <see langword="false" />.
	/// </returns>
	public static bool operator ==(VertexInputLayout left, VertexInputLayout right)
	{
		return object.Equals(left, right);
	}

	/// <summary>
	/// Compares two <see cref="T:Microsoft.Xna.Framework.Graphics.VertexInputLayout" /> instances to determine whether they are
	/// different.
	/// </summary>
	/// <param name="left">The first instance.</param>
	/// <param name="right">The second instance.</param>
	/// <returns>
	/// <see langword="true" /> if the <paramref name="left" /> and <paramref name="right" /> are
	/// the different; otherwise, <see langword="false" />.
	/// </returns>
	public static bool operator !=(VertexInputLayout left, VertexInputLayout right)
	{
		return !object.Equals(left, right);
	}
}
