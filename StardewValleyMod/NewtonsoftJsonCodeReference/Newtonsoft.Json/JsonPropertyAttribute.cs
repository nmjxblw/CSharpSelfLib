using System;

namespace Newtonsoft.Json;

/// <summary>
/// Instructs the <see cref="T:Newtonsoft.Json.JsonSerializer" /> to always serialize the member with the specified name.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class JsonPropertyAttribute : Attribute
{
	internal NullValueHandling? _nullValueHandling;

	internal DefaultValueHandling? _defaultValueHandling;

	internal ReferenceLoopHandling? _referenceLoopHandling;

	internal ObjectCreationHandling? _objectCreationHandling;

	internal TypeNameHandling? _typeNameHandling;

	internal bool? _isReference;

	internal int? _order;

	internal Required? _required;

	internal bool? _itemIsReference;

	internal ReferenceLoopHandling? _itemReferenceLoopHandling;

	internal TypeNameHandling? _itemTypeNameHandling;

	/// <summary>
	/// Gets or sets the <see cref="T:Newtonsoft.Json.JsonConverter" /> type used when serializing the property's collection items.
	/// </summary>
	/// <value>The collection's items <see cref="T:Newtonsoft.Json.JsonConverter" /> type.</value>
	public Type? ItemConverterType { get; set; }

	/// <summary>
	/// The parameter list to use when constructing the <see cref="T:Newtonsoft.Json.JsonConverter" /> described by <see cref="P:Newtonsoft.Json.JsonPropertyAttribute.ItemConverterType" />.
	/// If <c>null</c>, the default constructor is used.
	/// When non-<c>null</c>, there must be a constructor defined in the <see cref="T:Newtonsoft.Json.JsonConverter" /> that exactly matches the number,
	/// order, and type of these parameters.
	/// </summary>
	/// <example>
	/// <code>
	/// [JsonProperty(ItemConverterType = typeof(MyContainerConverter), ItemConverterParameters = new object[] { 123, "Four" })]
	/// </code>
	/// </example>
	public object[]? ItemConverterParameters { get; set; }

	/// <summary>
	/// Gets or sets the <see cref="T:System.Type" /> of the <see cref="T:Newtonsoft.Json.Serialization.NamingStrategy" />.
	/// </summary>
	/// <value>The <see cref="T:System.Type" /> of the <see cref="T:Newtonsoft.Json.Serialization.NamingStrategy" />.</value>
	public Type? NamingStrategyType { get; set; }

	/// <summary>
	/// The parameter list to use when constructing the <see cref="T:Newtonsoft.Json.Serialization.NamingStrategy" /> described by <see cref="P:Newtonsoft.Json.JsonPropertyAttribute.NamingStrategyType" />.
	/// If <c>null</c>, the default constructor is used.
	/// When non-<c>null</c>, there must be a constructor defined in the <see cref="T:Newtonsoft.Json.Serialization.NamingStrategy" /> that exactly matches the number,
	/// order, and type of these parameters.
	/// </summary>
	/// <example>
	/// <code>
	/// [JsonProperty(NamingStrategyType = typeof(MyNamingStrategy), NamingStrategyParameters = new object[] { 123, "Four" })]
	/// </code>
	/// </example>
	public object[]? NamingStrategyParameters { get; set; }

	/// <summary>
	/// Gets or sets the null value handling used when serializing this property.
	/// </summary>
	/// <value>The null value handling.</value>
	public NullValueHandling NullValueHandling
	{
		get
		{
			return this._nullValueHandling.GetValueOrDefault();
		}
		set
		{
			this._nullValueHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets the default value handling used when serializing this property.
	/// </summary>
	/// <value>The default value handling.</value>
	public DefaultValueHandling DefaultValueHandling
	{
		get
		{
			return this._defaultValueHandling.GetValueOrDefault();
		}
		set
		{
			this._defaultValueHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets the reference loop handling used when serializing this property.
	/// </summary>
	/// <value>The reference loop handling.</value>
	public ReferenceLoopHandling ReferenceLoopHandling
	{
		get
		{
			return this._referenceLoopHandling.GetValueOrDefault();
		}
		set
		{
			this._referenceLoopHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets the object creation handling used when deserializing this property.
	/// </summary>
	/// <value>The object creation handling.</value>
	public ObjectCreationHandling ObjectCreationHandling
	{
		get
		{
			return this._objectCreationHandling.GetValueOrDefault();
		}
		set
		{
			this._objectCreationHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets the type name handling used when serializing this property.
	/// </summary>
	/// <value>The type name handling.</value>
	public TypeNameHandling TypeNameHandling
	{
		get
		{
			return this._typeNameHandling.GetValueOrDefault();
		}
		set
		{
			this._typeNameHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets whether this property's value is serialized as a reference.
	/// </summary>
	/// <value>Whether this property's value is serialized as a reference.</value>
	public bool IsReference
	{
		get
		{
			return this._isReference == true;
		}
		set
		{
			this._isReference = value;
		}
	}

	/// <summary>
	/// Gets or sets the order of serialization of a member.
	/// </summary>
	/// <value>The numeric order of serialization.</value>
	public int Order
	{
		get
		{
			return this._order.GetValueOrDefault();
		}
		set
		{
			this._order = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this property is required.
	/// </summary>
	/// <value>
	/// 	A value indicating whether this property is required.
	/// </value>
	public Required Required
	{
		get
		{
			return this._required.GetValueOrDefault();
		}
		set
		{
			this._required = value;
		}
	}

	/// <summary>
	/// Gets or sets the name of the property.
	/// </summary>
	/// <value>The name of the property.</value>
	public string? PropertyName { get; set; }

	/// <summary>
	/// Gets or sets the reference loop handling used when serializing the property's collection items.
	/// </summary>
	/// <value>The collection's items reference loop handling.</value>
	public ReferenceLoopHandling ItemReferenceLoopHandling
	{
		get
		{
			return this._itemReferenceLoopHandling.GetValueOrDefault();
		}
		set
		{
			this._itemReferenceLoopHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets the type name handling used when serializing the property's collection items.
	/// </summary>
	/// <value>The collection's items type name handling.</value>
	public TypeNameHandling ItemTypeNameHandling
	{
		get
		{
			return this._itemTypeNameHandling.GetValueOrDefault();
		}
		set
		{
			this._itemTypeNameHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets whether this property's collection items are serialized as a reference.
	/// </summary>
	/// <value>Whether this property's collection items are serialized as a reference.</value>
	public bool ItemIsReference
	{
		get
		{
			return this._itemIsReference == true;
		}
		set
		{
			this._itemIsReference = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonPropertyAttribute" /> class.
	/// </summary>
	public JsonPropertyAttribute()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonPropertyAttribute" /> class with the specified name.
	/// </summary>
	/// <param name="propertyName">Name of the property.</param>
	public JsonPropertyAttribute(string propertyName)
	{
		this.PropertyName = propertyName;
	}
}
