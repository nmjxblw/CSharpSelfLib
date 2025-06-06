using System;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

/// <summary>
/// Maps a JSON property to a .NET member or constructor parameter.
/// </summary>
public class JsonProperty
{
	internal Required? _required;

	internal bool _hasExplicitDefaultValue;

	private object? _defaultValue;

	private bool _hasGeneratedDefaultValue;

	private string? _propertyName;

	internal bool _skipPropertyNameEscape;

	private Type? _propertyType;

	internal JsonContract? PropertyContract { get; set; }

	/// <summary>
	/// Gets or sets the name of the property.
	/// </summary>
	/// <value>The name of the property.</value>
	public string? PropertyName
	{
		get
		{
			return this._propertyName;
		}
		set
		{
			this._propertyName = value;
			this._skipPropertyNameEscape = !JavaScriptUtils.ShouldEscapeJavaScriptString(this._propertyName, JavaScriptUtils.HtmlCharEscapeFlags);
		}
	}

	/// <summary>
	/// Gets or sets the type that declared this property.
	/// </summary>
	/// <value>The type that declared this property.</value>
	public Type? DeclaringType { get; set; }

	/// <summary>
	/// Gets or sets the order of serialization of a member.
	/// </summary>
	/// <value>The numeric order of serialization.</value>
	public int? Order { get; set; }

	/// <summary>
	/// Gets or sets the name of the underlying member or parameter.
	/// </summary>
	/// <value>The name of the underlying member or parameter.</value>
	public string? UnderlyingName { get; set; }

	/// <summary>
	/// Gets the <see cref="T:Newtonsoft.Json.Serialization.IValueProvider" /> that will get and set the <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> during serialization.
	/// </summary>
	/// <value>The <see cref="T:Newtonsoft.Json.Serialization.IValueProvider" /> that will get and set the <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> during serialization.</value>
	public IValueProvider? ValueProvider { get; set; }

	/// <summary>
	/// Gets or sets the <see cref="T:Newtonsoft.Json.Serialization.IAttributeProvider" /> for this property.
	/// </summary>
	/// <value>The <see cref="T:Newtonsoft.Json.Serialization.IAttributeProvider" /> for this property.</value>
	public IAttributeProvider? AttributeProvider { get; set; }

	/// <summary>
	/// Gets or sets the type of the property.
	/// </summary>
	/// <value>The type of the property.</value>
	public Type? PropertyType
	{
		get
		{
			return this._propertyType;
		}
		set
		{
			if (this._propertyType != value)
			{
				this._propertyType = value;
				this._hasGeneratedDefaultValue = false;
			}
		}
	}

	/// <summary>
	/// Gets or sets the <see cref="T:Newtonsoft.Json.JsonConverter" /> for the property.
	/// If set this converter takes precedence over the contract converter for the property type.
	/// </summary>
	/// <value>The converter.</value>
	public JsonConverter? Converter { get; set; }

	/// <summary>
	/// Gets or sets the member converter.
	/// </summary>
	/// <value>The member converter.</value>
	[Obsolete("MemberConverter is obsolete. Use Converter instead.")]
	public JsonConverter? MemberConverter
	{
		get
		{
			return this.Converter;
		}
		set
		{
			this.Converter = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> is ignored.
	/// </summary>
	/// <value><c>true</c> if ignored; otherwise, <c>false</c>.</value>
	public bool Ignored { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> is readable.
	/// </summary>
	/// <value><c>true</c> if readable; otherwise, <c>false</c>.</value>
	public bool Readable { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> is writable.
	/// </summary>
	/// <value><c>true</c> if writable; otherwise, <c>false</c>.</value>
	public bool Writable { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> has a member attribute.
	/// </summary>
	/// <value><c>true</c> if has a member attribute; otherwise, <c>false</c>.</value>
	public bool HasMemberAttribute { get; set; }

	/// <summary>
	/// Gets the default value.
	/// </summary>
	/// <value>The default value.</value>
	public object? DefaultValue
	{
		get
		{
			if (!this._hasExplicitDefaultValue)
			{
				return null;
			}
			return this._defaultValue;
		}
		set
		{
			this._hasExplicitDefaultValue = true;
			this._defaultValue = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> is required.
	/// </summary>
	/// <value>A value indicating whether this <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> is required.</value>
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
	/// Gets a value indicating whether <see cref="P:Newtonsoft.Json.Serialization.JsonProperty.Required" /> has a value specified.
	/// </summary>
	public bool IsRequiredSpecified => this._required.HasValue;

	/// <summary>
	/// Gets or sets a value indicating whether this property preserves object references.
	/// </summary>
	/// <value>
	/// 	<c>true</c> if this instance is reference; otherwise, <c>false</c>.
	/// </value>
	public bool? IsReference { get; set; }

	/// <summary>
	/// Gets or sets the property null value handling.
	/// </summary>
	/// <value>The null value handling.</value>
	public NullValueHandling? NullValueHandling { get; set; }

	/// <summary>
	/// Gets or sets the property default value handling.
	/// </summary>
	/// <value>The default value handling.</value>
	public DefaultValueHandling? DefaultValueHandling { get; set; }

	/// <summary>
	/// Gets or sets the property reference loop handling.
	/// </summary>
	/// <value>The reference loop handling.</value>
	public ReferenceLoopHandling? ReferenceLoopHandling { get; set; }

	/// <summary>
	/// Gets or sets the property object creation handling.
	/// </summary>
	/// <value>The object creation handling.</value>
	public ObjectCreationHandling? ObjectCreationHandling { get; set; }

	/// <summary>
	/// Gets or sets or sets the type name handling.
	/// </summary>
	/// <value>The type name handling.</value>
	public TypeNameHandling? TypeNameHandling { get; set; }

	/// <summary>
	/// Gets or sets a predicate used to determine whether the property should be serialized.
	/// </summary>
	/// <value>A predicate used to determine whether the property should be serialized.</value>
	public Predicate<object>? ShouldSerialize { get; set; }

	/// <summary>
	/// Gets or sets a predicate used to determine whether the property should be deserialized.
	/// </summary>
	/// <value>A predicate used to determine whether the property should be deserialized.</value>
	public Predicate<object>? ShouldDeserialize { get; set; }

	/// <summary>
	/// Gets or sets a predicate used to determine whether the property should be serialized.
	/// </summary>
	/// <value>A predicate used to determine whether the property should be serialized.</value>
	public Predicate<object>? GetIsSpecified { get; set; }

	/// <summary>
	/// Gets or sets an action used to set whether the property has been deserialized.
	/// </summary>
	/// <value>An action used to set whether the property has been deserialized.</value>
	public Action<object, object?>? SetIsSpecified { get; set; }

	/// <summary>
	/// Gets or sets the converter used when serializing the property's collection items.
	/// </summary>
	/// <value>The collection's items converter.</value>
	public JsonConverter? ItemConverter { get; set; }

	/// <summary>
	/// Gets or sets whether this property's collection items are serialized as a reference.
	/// </summary>
	/// <value>Whether this property's collection items are serialized as a reference.</value>
	public bool? ItemIsReference { get; set; }

	/// <summary>
	/// Gets or sets the type name handling used when serializing the property's collection items.
	/// </summary>
	/// <value>The collection's items type name handling.</value>
	public TypeNameHandling? ItemTypeNameHandling { get; set; }

	/// <summary>
	/// Gets or sets the reference loop handling used when serializing the property's collection items.
	/// </summary>
	/// <value>The collection's items reference loop handling.</value>
	public ReferenceLoopHandling? ItemReferenceLoopHandling { get; set; }

	internal object? GetResolvedDefaultValue()
	{
		if (this._propertyType == null)
		{
			return null;
		}
		if (!this._hasExplicitDefaultValue && !this._hasGeneratedDefaultValue)
		{
			this._defaultValue = ReflectionUtils.GetDefaultValue(this._propertyType);
			this._hasGeneratedDefaultValue = true;
		}
		return this._defaultValue;
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> that represents this instance.
	/// </summary>
	/// <returns>
	/// A <see cref="T:System.String" /> that represents this instance.
	/// </returns>
	public override string ToString()
	{
		return this.PropertyName ?? string.Empty;
	}

	internal void WritePropertyName(JsonWriter writer)
	{
		string propertyName = this.PropertyName;
		if (this._skipPropertyNameEscape)
		{
			writer.WritePropertyName(propertyName, escape: false);
		}
		else
		{
			writer.WritePropertyName(propertyName);
		}
	}
}
