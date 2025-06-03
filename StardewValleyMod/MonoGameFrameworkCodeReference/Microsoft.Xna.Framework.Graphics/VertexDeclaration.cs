using System;
using System.Collections.Generic;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Defines per-vertex data of a vertex buffer.
/// </summary>
/// <remarks>
/// <see cref="T:Microsoft.Xna.Framework.Graphics.VertexDeclaration" /> implements <see cref="T:System.IEquatable`1" /> and can be used as
/// a key in a dictionary. Two vertex declarations are considered equal if the vertices are
/// structurally equivalent, i.e. the vertex elements and the vertex stride are identical. (The
/// properties <see cref="P:Microsoft.Xna.Framework.Graphics.GraphicsResource.Name" /> and <see cref="P:Microsoft.Xna.Framework.Graphics.GraphicsResource.Tag" /> are
/// ignored in <see cref="M:Microsoft.Xna.Framework.Graphics.VertexDeclaration.GetHashCode" /> and <see cref="M:Microsoft.Xna.Framework.Graphics.VertexDeclaration.Equals(Microsoft.Xna.Framework.Graphics.VertexDeclaration)" />!)
/// </remarks>
public class VertexDeclaration : GraphicsResource, IEquatable<VertexDeclaration>
{
	private sealed class Data : IEquatable<Data>
	{
		private readonly int _hashCode;

		public readonly int VertexStride;

		public VertexElement[] Elements;

		public Data(int vertexStride, VertexElement[] elements)
		{
			this.VertexStride = vertexStride;
			this.Elements = elements;
			this._hashCode = elements[0].GetHashCode();
			for (int i = 1; i < elements.Length; i++)
			{
				this._hashCode = (this._hashCode * 397) ^ elements[i].GetHashCode();
			}
			this._hashCode = (this._hashCode * 397) ^ elements.Length;
			this._hashCode = (this._hashCode * 397) ^ vertexStride;
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as Data);
		}

		public bool Equals(Data other)
		{
			if (other == null)
			{
				return false;
			}
			if (this == other)
			{
				return true;
			}
			if (this._hashCode != other._hashCode || this.VertexStride != other.VertexStride || this.Elements.Length != other.Elements.Length)
			{
				return false;
			}
			for (int i = 0; i < this.Elements.Length; i++)
			{
				if (!this.Elements[i].Equals(other.Elements[i]))
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			return this._hashCode;
		}
	}

	/// <summary>
	/// Vertex attribute information for a particular shader/vertex declaration combination.
	/// </summary>
	internal class VertexDeclarationAttributeInfo
	{
		internal class Element
		{
			public int Offset;

			public int AttributeLocation;

			public int NumberOfElements;

			public VertexAttribPointerType VertexAttribPointerType;

			public bool Normalized;
		}

		internal bool[] EnabledAttributes;

		internal List<Element> Elements;

		internal VertexDeclarationAttributeInfo(int maxVertexAttributes)
		{
			this.EnabledAttributes = new bool[maxVertexAttributes];
			this.Elements = new List<Element>();
		}
	}

	private static readonly Dictionary<Data, VertexDeclaration> _vertexDeclarationCache;

	private readonly Data _data;

	private readonly Dictionary<int, VertexDeclarationAttributeInfo> _shaderAttributeInfo = new Dictionary<int, VertexDeclarationAttributeInfo>();

	/// <summary>
	/// Gets the internal vertex elements array.
	/// </summary>
	/// <value>The internal vertex elements array.</value>
	internal VertexElement[] InternalVertexElements => this._data.Elements;

	/// <summary>
	/// Gets the size of a vertex (including padding) in bytes.
	/// </summary>
	/// <value>The size of a vertex (including padding) in bytes.</value>
	public int VertexStride => this._data.VertexStride;

	static VertexDeclaration()
	{
		VertexDeclaration._vertexDeclarationCache = new Dictionary<Data, VertexDeclaration>();
	}

	internal static VertexDeclaration GetOrCreate(int vertexStride, VertexElement[] elements)
	{
		lock (VertexDeclaration._vertexDeclarationCache)
		{
			Data data = new Data(vertexStride, elements);
			if (!VertexDeclaration._vertexDeclarationCache.TryGetValue(data, out var vertexDeclaration))
			{
				data.Elements = (VertexElement[])elements.Clone();
				vertexDeclaration = new VertexDeclaration(data);
				VertexDeclaration._vertexDeclarationCache[data] = vertexDeclaration;
			}
			return vertexDeclaration;
		}
	}

	private VertexDeclaration(Data data)
	{
		this._data = data;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Graphics.VertexDeclaration" /> class.
	/// </summary>
	/// <param name="elements">The vertex elements.</param>
	/// <exception cref="T:System.ArgumentNullException">
	/// <paramref name="elements" /> is <see langword="null" /> or empty.
	/// </exception>
	public VertexDeclaration(params VertexElement[] elements)
		: this(VertexDeclaration.GetVertexStride(elements), elements)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Graphics.VertexDeclaration" /> class.
	/// </summary>
	/// <param name="vertexStride">The size of a vertex (including padding) in bytes.</param>
	/// <param name="elements">The vertex elements.</param>
	/// <exception cref="T:System.ArgumentNullException">
	/// <paramref name="elements" /> is <see langword="null" /> or empty.
	/// </exception>
	public VertexDeclaration(int vertexStride, params VertexElement[] elements)
	{
		if (elements == null || elements.Length == 0)
		{
			throw new ArgumentNullException("elements", "Elements cannot be empty");
		}
		lock (VertexDeclaration._vertexDeclarationCache)
		{
			Data data = new Data(vertexStride, elements);
			if (VertexDeclaration._vertexDeclarationCache.TryGetValue(data, out var vertexDeclaration))
			{
				this._data = vertexDeclaration._data;
				return;
			}
			data.Elements = (VertexElement[])elements.Clone();
			this._data = data;
			VertexDeclaration._vertexDeclarationCache[data] = this;
		}
	}

	private static int GetVertexStride(VertexElement[] elements)
	{
		int max = 0;
		for (int i = 0; i < elements.Length; i++)
		{
			int start = elements[i].Offset + elements[i].VertexElementFormat.GetSize();
			if (max < start)
			{
				max = start;
			}
		}
		return max;
	}

	/// <summary>
	/// Returns the VertexDeclaration for Type.
	/// </summary>
	/// <param name="vertexType">A value type which implements the IVertexType interface.</param>
	/// <returns>The VertexDeclaration.</returns>
	/// <remarks>
	/// Prefer to use VertexDeclarationCache when the declaration lookup
	/// can be performed with a templated type.
	/// </remarks>
	internal static VertexDeclaration FromType(Type vertexType)
	{
		if (vertexType == null)
		{
			throw new ArgumentNullException("vertexType", "Cannot be null");
		}
		if (!ReflectionHelpers.IsValueType(vertexType))
		{
			throw new ArgumentException("Must be value type", "vertexType");
		}
		VertexDeclaration vertexDeclaration = ((Activator.CreateInstance(vertexType) as IVertexType) ?? throw new ArgumentException("vertexData does not inherit IVertexType")).VertexDeclaration;
		if (vertexDeclaration == null)
		{
			throw new Exception("VertexDeclaration cannot be null");
		}
		return vertexDeclaration;
	}

	/// <summary>
	/// Gets a copy of the vertex elements.
	/// </summary>
	/// <returns>A copy of the vertex elements.</returns>
	public VertexElement[] GetVertexElements()
	{
		return (VertexElement[])this._data.Elements.Clone();
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
		return this.Equals(obj as VertexDeclaration);
	}

	/// <summary>
	/// Determines whether the specified <see cref="T:Microsoft.Xna.Framework.Graphics.VertexDeclaration" /> is equal to this
	/// instance.
	/// </summary>
	/// <param name="other">The object to compare with the current object.</param>
	/// <returns>
	/// <see langword="true" /> if the specified <see cref="T:Microsoft.Xna.Framework.Graphics.VertexDeclaration" /> is equal to this
	/// instance; otherwise, <see langword="false" />.
	/// </returns>
	public bool Equals(VertexDeclaration other)
	{
		if (other != null)
		{
			return this._data == other._data;
		}
		return false;
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
		return this._data.GetHashCode();
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
	public static bool operator ==(VertexDeclaration left, VertexDeclaration right)
	{
		return object.Equals(left, right);
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
	public static bool operator !=(VertexDeclaration left, VertexDeclaration right)
	{
		return !object.Equals(left, right);
	}

	internal VertexDeclarationAttributeInfo GetAttributeInfo(Shader shader, int programHash)
	{
		if (this._shaderAttributeInfo.TryGetValue(programHash, out var attrInfo))
		{
			return attrInfo;
		}
		attrInfo = new VertexDeclarationAttributeInfo(base.GraphicsDevice.MaxVertexAttributes);
		VertexElement[] internalVertexElements = this.InternalVertexElements;
		for (int i = 0; i < internalVertexElements.Length; i++)
		{
			VertexElement ve = internalVertexElements[i];
			int attributeLocation = shader.GetAttribLocation(ve.VertexElementUsage, ve.UsageIndex);
			if (attributeLocation >= 0)
			{
				attrInfo.Elements.Add(new VertexDeclarationAttributeInfo.Element
				{
					Offset = ve.Offset,
					AttributeLocation = attributeLocation,
					NumberOfElements = ve.VertexElementFormat.OpenGLNumberOfElements(),
					VertexAttribPointerType = ve.VertexElementFormat.OpenGLVertexAttribPointerType(),
					Normalized = ve.OpenGLVertexAttribNormalized()
				});
				attrInfo.EnabledAttributes[attributeLocation] = true;
			}
		}
		this._shaderAttributeInfo.Add(programHash, attrInfo);
		return attrInfo;
	}

	internal void Apply(Shader shader, IntPtr offset, int programHash)
	{
		VertexDeclarationAttributeInfo attrInfo = this.GetAttributeInfo(shader, programHash);
		foreach (VertexDeclarationAttributeInfo.Element element in attrInfo.Elements)
		{
			GL.VertexAttribPointer(element.AttributeLocation, element.NumberOfElements, element.VertexAttribPointerType, element.Normalized, this.VertexStride, (IntPtr)(offset.ToInt64() + element.Offset));
			if (base.GraphicsDevice.GraphicsCapabilities.SupportsInstancing)
			{
				GL.VertexAttribDivisor(element.AttributeLocation, 0);
			}
		}
		base.GraphicsDevice.SetVertexAttributeArray(attrInfo.EnabledAttributes);
		GraphicsDevice._attribsDirty = true;
	}
}
