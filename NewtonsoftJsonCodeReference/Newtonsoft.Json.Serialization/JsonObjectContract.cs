using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

/// <summary>
/// Contract details for a <see cref="T:System.Type" /> used by the <see cref="T:Newtonsoft.Json.JsonSerializer" />.
/// </summary>
public class JsonObjectContract : JsonContainerContract
{
	internal bool ExtensionDataIsJToken;

	private bool? _hasRequiredOrDefaultValueProperties;

	private ObjectConstructor<object>? _overrideCreator;

	private ObjectConstructor<object>? _parameterizedCreator;

	private JsonPropertyCollection? _creatorParameters;

	private Type? _extensionDataValueType;

	/// <summary>
	/// Gets or sets the object member serialization.
	/// </summary>
	/// <value>The member object serialization.</value>
	public MemberSerialization MemberSerialization { get; set; }

	/// <summary>
	/// Gets or sets the missing member handling used when deserializing this object.
	/// </summary>
	/// <value>The missing member handling.</value>
	public MissingMemberHandling? MissingMemberHandling { get; set; }

	/// <summary>
	/// Gets or sets a value that indicates whether the object's properties are required.
	/// </summary>
	/// <value>
	/// 	A value indicating whether the object's properties are required.
	/// </value>
	public Required? ItemRequired { get; set; }

	/// <summary>
	/// Gets or sets how the object's properties with null values are handled during serialization and deserialization.
	/// </summary>
	/// <value>How the object's properties with null values are handled during serialization and deserialization.</value>
	public NullValueHandling? ItemNullValueHandling { get; set; }

	/// <summary>
	/// Gets the object's properties.
	/// </summary>
	/// <value>The object's properties.</value>
	public JsonPropertyCollection Properties { get; }

	/// <summary>
	/// Gets a collection of <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> instances that define the parameters used with <see cref="P:Newtonsoft.Json.Serialization.JsonObjectContract.OverrideCreator" />.
	/// </summary>
	public JsonPropertyCollection CreatorParameters
	{
		get
		{
			if (this._creatorParameters == null)
			{
				this._creatorParameters = new JsonPropertyCollection(base.UnderlyingType);
			}
			return this._creatorParameters;
		}
	}

	/// <summary>
	/// Gets or sets the function used to create the object. When set this function will override <see cref="P:Newtonsoft.Json.Serialization.JsonContract.DefaultCreator" />.
	/// This function is called with a collection of arguments which are defined by the <see cref="P:Newtonsoft.Json.Serialization.JsonObjectContract.CreatorParameters" /> collection.
	/// </summary>
	/// <value>The function used to create the object.</value>
	public ObjectConstructor<object>? OverrideCreator
	{
		get
		{
			return this._overrideCreator;
		}
		set
		{
			this._overrideCreator = value;
		}
	}

	internal ObjectConstructor<object>? ParameterizedCreator
	{
		get
		{
			return this._parameterizedCreator;
		}
		set
		{
			this._parameterizedCreator = value;
		}
	}

	/// <summary>
	/// Gets or sets the extension data setter.
	/// </summary>
	public ExtensionDataSetter? ExtensionDataSetter { get; set; }

	/// <summary>
	/// Gets or sets the extension data getter.
	/// </summary>
	public ExtensionDataGetter? ExtensionDataGetter { get; set; }

	/// <summary>
	/// Gets or sets the extension data value type.
	/// </summary>
	public Type? ExtensionDataValueType
	{
		get
		{
			return this._extensionDataValueType;
		}
		set
		{
			this._extensionDataValueType = value;
			this.ExtensionDataIsJToken = value != null && typeof(JToken).IsAssignableFrom(value);
		}
	}

	/// <summary>
	/// Gets or sets the extension data name resolver.
	/// </summary>
	/// <value>The extension data name resolver.</value>
	public Func<string, string>? ExtensionDataNameResolver { get; set; }

	internal bool HasRequiredOrDefaultValueProperties
	{
		get
		{
			if (!this._hasRequiredOrDefaultValueProperties.HasValue)
			{
				this._hasRequiredOrDefaultValueProperties = false;
				if ((this.ItemRequired ?? Required.Default) != Required.Default)
				{
					this._hasRequiredOrDefaultValueProperties = true;
				}
				else
				{
					foreach (JsonProperty property in this.Properties)
					{
						if (property.Required != Required.Default || ((uint?)property.DefaultValueHandling & 2u) == 2)
						{
							this._hasRequiredOrDefaultValueProperties = true;
							break;
						}
					}
				}
			}
			return this._hasRequiredOrDefaultValueProperties == true;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.JsonObjectContract" /> class.
	/// </summary>
	/// <param name="underlyingType">The underlying type for the contract.</param>
	public JsonObjectContract(Type underlyingType)
		: base(underlyingType)
	{
		base.ContractType = JsonContractType.Object;
		this.Properties = new JsonPropertyCollection(base.UnderlyingType);
	}

	[SecuritySafeCritical]
	internal object GetUninitializedObject()
	{
		if (!JsonTypeReflector.FullyTrusted)
		{
			throw new JsonException("Insufficient permissions. Creating an uninitialized '{0}' type requires full trust.".FormatWith(CultureInfo.InvariantCulture, base.NonNullableUnderlyingType));
		}
		return FormatterServices.GetUninitializedObject(base.NonNullableUnderlyingType);
	}
}
