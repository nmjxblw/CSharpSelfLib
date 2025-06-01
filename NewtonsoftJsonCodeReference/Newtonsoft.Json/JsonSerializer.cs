using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json;

/// <summary>
/// Serializes and deserializes objects into and from the JSON format.
/// The <see cref="T:Newtonsoft.Json.JsonSerializer" /> enables you to control how objects are encoded into JSON.
/// </summary>
public class JsonSerializer
{
	internal TypeNameHandling _typeNameHandling;

	internal TypeNameAssemblyFormatHandling _typeNameAssemblyFormatHandling;

	internal PreserveReferencesHandling _preserveReferencesHandling;

	internal ReferenceLoopHandling _referenceLoopHandling;

	internal MissingMemberHandling _missingMemberHandling;

	internal ObjectCreationHandling _objectCreationHandling;

	internal NullValueHandling _nullValueHandling;

	internal DefaultValueHandling _defaultValueHandling;

	internal ConstructorHandling _constructorHandling;

	internal MetadataPropertyHandling _metadataPropertyHandling;

	internal JsonConverterCollection? _converters;

	internal IContractResolver _contractResolver;

	internal ITraceWriter? _traceWriter;

	internal IEqualityComparer? _equalityComparer;

	internal ISerializationBinder _serializationBinder;

	internal StreamingContext _context;

	private IReferenceResolver? _referenceResolver;

	private Formatting? _formatting;

	private DateFormatHandling? _dateFormatHandling;

	private DateTimeZoneHandling? _dateTimeZoneHandling;

	private DateParseHandling? _dateParseHandling;

	private FloatFormatHandling? _floatFormatHandling;

	private FloatParseHandling? _floatParseHandling;

	private StringEscapeHandling? _stringEscapeHandling;

	private CultureInfo _culture;

	private int? _maxDepth;

	private bool _maxDepthSet;

	private bool? _checkAdditionalContent;

	private string? _dateFormatString;

	private bool _dateFormatStringSet;

	/// <summary>
	/// Gets or sets the <see cref="T:Newtonsoft.Json.Serialization.IReferenceResolver" /> used by the serializer when resolving references.
	/// </summary>
	public virtual IReferenceResolver? ReferenceResolver
	{
		get
		{
			return this.GetReferenceResolver();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value", "Reference resolver cannot be null.");
			}
			this._referenceResolver = value;
		}
	}

	/// <summary>
	/// Gets or sets the <see cref="P:Newtonsoft.Json.JsonSerializer.SerializationBinder" /> used by the serializer when resolving type names.
	/// </summary>
	[Obsolete("Binder is obsolete. Use SerializationBinder instead.")]
	public virtual SerializationBinder Binder
	{
		get
		{
			if (this._serializationBinder is SerializationBinder result)
			{
				return result;
			}
			if (this._serializationBinder is SerializationBinderAdapter serializationBinderAdapter)
			{
				return serializationBinderAdapter.SerializationBinder;
			}
			throw new InvalidOperationException("Cannot get SerializationBinder because an ISerializationBinder was previously set.");
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value", "Serialization binder cannot be null.");
			}
			this._serializationBinder = (value as ISerializationBinder) ?? new SerializationBinderAdapter(value);
		}
	}

	/// <summary>
	/// Gets or sets the <see cref="T:Newtonsoft.Json.Serialization.ISerializationBinder" /> used by the serializer when resolving type names.
	/// </summary>
	public virtual ISerializationBinder SerializationBinder
	{
		get
		{
			return this._serializationBinder;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value", "Serialization binder cannot be null.");
			}
			this._serializationBinder = value;
		}
	}

	/// <summary>
	/// Gets or sets the <see cref="T:Newtonsoft.Json.Serialization.ITraceWriter" /> used by the serializer when writing trace messages.
	/// </summary>
	/// <value>The trace writer.</value>
	public virtual ITraceWriter? TraceWriter
	{
		get
		{
			return this._traceWriter;
		}
		set
		{
			this._traceWriter = value;
		}
	}

	/// <summary>
	/// Gets or sets the equality comparer used by the serializer when comparing references.
	/// </summary>
	/// <value>The equality comparer.</value>
	public virtual IEqualityComparer? EqualityComparer
	{
		get
		{
			return this._equalityComparer;
		}
		set
		{
			this._equalityComparer = value;
		}
	}

	/// <summary>
	/// Gets or sets how type name writing and reading is handled by the serializer.
	/// The default value is <see cref="F:Newtonsoft.Json.TypeNameHandling.None" />.
	/// </summary>
	/// <remarks>
	/// <see cref="P:Newtonsoft.Json.JsonSerializer.TypeNameHandling" /> should be used with caution when your application deserializes JSON from an external source.
	/// Incoming types should be validated with a custom <see cref="P:Newtonsoft.Json.JsonSerializer.SerializationBinder" />
	/// when deserializing with a value other than <see cref="F:Newtonsoft.Json.TypeNameHandling.None" />.
	/// </remarks>
	public virtual TypeNameHandling TypeNameHandling
	{
		get
		{
			return this._typeNameHandling;
		}
		set
		{
			if (value < TypeNameHandling.None || value > TypeNameHandling.Auto)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._typeNameHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how a type name assembly is written and resolved by the serializer.
	/// The default value is <see cref="F:System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple" />.
	/// </summary>
	/// <value>The type name assembly format.</value>
	[Obsolete("TypeNameAssemblyFormat is obsolete. Use TypeNameAssemblyFormatHandling instead.")]
	public virtual FormatterAssemblyStyle TypeNameAssemblyFormat
	{
		get
		{
			return (FormatterAssemblyStyle)this._typeNameAssemblyFormatHandling;
		}
		set
		{
			if (value < FormatterAssemblyStyle.Simple || value > FormatterAssemblyStyle.Full)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._typeNameAssemblyFormatHandling = (TypeNameAssemblyFormatHandling)value;
		}
	}

	/// <summary>
	/// Gets or sets how a type name assembly is written and resolved by the serializer.
	/// The default value is <see cref="F:Newtonsoft.Json.TypeNameAssemblyFormatHandling.Simple" />.
	/// </summary>
	/// <value>The type name assembly format.</value>
	public virtual TypeNameAssemblyFormatHandling TypeNameAssemblyFormatHandling
	{
		get
		{
			return this._typeNameAssemblyFormatHandling;
		}
		set
		{
			if (value < TypeNameAssemblyFormatHandling.Simple || value > TypeNameAssemblyFormatHandling.Full)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._typeNameAssemblyFormatHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how object references are preserved by the serializer.
	/// The default value is <see cref="F:Newtonsoft.Json.PreserveReferencesHandling.None" />.
	/// </summary>
	public virtual PreserveReferencesHandling PreserveReferencesHandling
	{
		get
		{
			return this._preserveReferencesHandling;
		}
		set
		{
			if (value < PreserveReferencesHandling.None || value > PreserveReferencesHandling.All)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._preserveReferencesHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how reference loops (e.g. a class referencing itself) is handled.
	/// The default value is <see cref="F:Newtonsoft.Json.ReferenceLoopHandling.Error" />.
	/// </summary>
	public virtual ReferenceLoopHandling ReferenceLoopHandling
	{
		get
		{
			return this._referenceLoopHandling;
		}
		set
		{
			if (value < ReferenceLoopHandling.Error || value > ReferenceLoopHandling.Serialize)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._referenceLoopHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how missing members (e.g. JSON contains a property that isn't a member on the object) are handled during deserialization.
	/// The default value is <see cref="F:Newtonsoft.Json.MissingMemberHandling.Ignore" />.
	/// </summary>
	public virtual MissingMemberHandling MissingMemberHandling
	{
		get
		{
			return this._missingMemberHandling;
		}
		set
		{
			if (value < MissingMemberHandling.Ignore || value > MissingMemberHandling.Error)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._missingMemberHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how null values are handled during serialization and deserialization.
	/// The default value is <see cref="F:Newtonsoft.Json.NullValueHandling.Include" />.
	/// </summary>
	public virtual NullValueHandling NullValueHandling
	{
		get
		{
			return this._nullValueHandling;
		}
		set
		{
			if (value < NullValueHandling.Include || value > NullValueHandling.Ignore)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._nullValueHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how default values are handled during serialization and deserialization.
	/// The default value is <see cref="F:Newtonsoft.Json.DefaultValueHandling.Include" />.
	/// </summary>
	public virtual DefaultValueHandling DefaultValueHandling
	{
		get
		{
			return this._defaultValueHandling;
		}
		set
		{
			if (value < DefaultValueHandling.Include || value > DefaultValueHandling.IgnoreAndPopulate)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._defaultValueHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how objects are created during deserialization.
	/// The default value is <see cref="F:Newtonsoft.Json.ObjectCreationHandling.Auto" />.
	/// </summary>
	/// <value>The object creation handling.</value>
	public virtual ObjectCreationHandling ObjectCreationHandling
	{
		get
		{
			return this._objectCreationHandling;
		}
		set
		{
			if (value < ObjectCreationHandling.Auto || value > ObjectCreationHandling.Replace)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._objectCreationHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how constructors are used during deserialization.
	/// The default value is <see cref="F:Newtonsoft.Json.ConstructorHandling.Default" />.
	/// </summary>
	/// <value>The constructor handling.</value>
	public virtual ConstructorHandling ConstructorHandling
	{
		get
		{
			return this._constructorHandling;
		}
		set
		{
			if (value < ConstructorHandling.Default || value > ConstructorHandling.AllowNonPublicDefaultConstructor)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._constructorHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how metadata properties are used during deserialization.
	/// The default value is <see cref="F:Newtonsoft.Json.MetadataPropertyHandling.Default" />.
	/// </summary>
	/// <value>The metadata properties handling.</value>
	public virtual MetadataPropertyHandling MetadataPropertyHandling
	{
		get
		{
			return this._metadataPropertyHandling;
		}
		set
		{
			if (value < MetadataPropertyHandling.Default || value > MetadataPropertyHandling.Ignore)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._metadataPropertyHandling = value;
		}
	}

	/// <summary>
	/// Gets a collection <see cref="T:Newtonsoft.Json.JsonConverter" /> that will be used during serialization.
	/// </summary>
	/// <value>Collection <see cref="T:Newtonsoft.Json.JsonConverter" /> that will be used during serialization.</value>
	public virtual JsonConverterCollection Converters
	{
		get
		{
			if (this._converters == null)
			{
				this._converters = new JsonConverterCollection();
			}
			return this._converters;
		}
	}

	/// <summary>
	/// Gets or sets the contract resolver used by the serializer when
	/// serializing .NET objects to JSON and vice versa.
	/// </summary>
	public virtual IContractResolver ContractResolver
	{
		get
		{
			return this._contractResolver;
		}
		set
		{
			this._contractResolver = value ?? DefaultContractResolver.Instance;
		}
	}

	/// <summary>
	/// Gets or sets the <see cref="T:System.Runtime.Serialization.StreamingContext" /> used by the serializer when invoking serialization callback methods.
	/// </summary>
	/// <value>The context.</value>
	public virtual StreamingContext Context
	{
		get
		{
			return this._context;
		}
		set
		{
			this._context = value;
		}
	}

	/// <summary>
	/// Indicates how JSON text output is formatted.
	/// The default value is <see cref="F:Newtonsoft.Json.Formatting.None" />.
	/// </summary>
	public virtual Formatting Formatting
	{
		get
		{
			return this._formatting.GetValueOrDefault();
		}
		set
		{
			this._formatting = value;
		}
	}

	/// <summary>
	/// Gets or sets how dates are written to JSON text.
	/// The default value is <see cref="F:Newtonsoft.Json.DateFormatHandling.IsoDateFormat" />.
	/// </summary>
	public virtual DateFormatHandling DateFormatHandling
	{
		get
		{
			return this._dateFormatHandling.GetValueOrDefault();
		}
		set
		{
			this._dateFormatHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how <see cref="T:System.DateTime" /> time zones are handled during serialization and deserialization.
	/// The default value is <see cref="F:Newtonsoft.Json.DateTimeZoneHandling.RoundtripKind" />.
	/// </summary>
	public virtual DateTimeZoneHandling DateTimeZoneHandling
	{
		get
		{
			return this._dateTimeZoneHandling ?? DateTimeZoneHandling.RoundtripKind;
		}
		set
		{
			this._dateTimeZoneHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how date formatted strings, e.g. <c>"\/Date(1198908717056)\/"</c> and <c>"2012-03-21T05:40Z"</c>, are parsed when reading JSON.
	/// The default value is <see cref="F:Newtonsoft.Json.DateParseHandling.DateTime" />.
	/// </summary>
	public virtual DateParseHandling DateParseHandling
	{
		get
		{
			return this._dateParseHandling ?? DateParseHandling.DateTime;
		}
		set
		{
			this._dateParseHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how floating point numbers, e.g. 1.0 and 9.9, are parsed when reading JSON text.
	/// The default value is <see cref="F:Newtonsoft.Json.FloatParseHandling.Double" />.
	/// </summary>
	public virtual FloatParseHandling FloatParseHandling
	{
		get
		{
			return this._floatParseHandling.GetValueOrDefault();
		}
		set
		{
			this._floatParseHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how special floating point numbers, e.g. <see cref="F:System.Double.NaN" />,
	/// <see cref="F:System.Double.PositiveInfinity" /> and <see cref="F:System.Double.NegativeInfinity" />,
	/// are written as JSON text.
	/// The default value is <see cref="F:Newtonsoft.Json.FloatFormatHandling.String" />.
	/// </summary>
	public virtual FloatFormatHandling FloatFormatHandling
	{
		get
		{
			return this._floatFormatHandling.GetValueOrDefault();
		}
		set
		{
			this._floatFormatHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how strings are escaped when writing JSON text.
	/// The default value is <see cref="F:Newtonsoft.Json.StringEscapeHandling.Default" />.
	/// </summary>
	public virtual StringEscapeHandling StringEscapeHandling
	{
		get
		{
			return this._stringEscapeHandling.GetValueOrDefault();
		}
		set
		{
			this._stringEscapeHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how <see cref="T:System.DateTime" /> and <see cref="T:System.DateTimeOffset" /> values are formatted when writing JSON text,
	/// and the expected date format when reading JSON text.
	/// The default value is <c>"yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK"</c>.
	/// </summary>
	public virtual string DateFormatString
	{
		get
		{
			return this._dateFormatString ?? "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";
		}
		set
		{
			this._dateFormatString = value;
			this._dateFormatStringSet = true;
		}
	}

	/// <summary>
	/// Gets or sets the culture used when reading JSON.
	/// The default value is <see cref="P:System.Globalization.CultureInfo.InvariantCulture" />.
	/// </summary>
	public virtual CultureInfo Culture
	{
		get
		{
			return this._culture ?? JsonSerializerSettings.DefaultCulture;
		}
		set
		{
			this._culture = value;
		}
	}

	/// <summary>
	/// Gets or sets the maximum depth allowed when reading JSON. Reading past this depth will throw a <see cref="T:Newtonsoft.Json.JsonReaderException" />.
	/// A null value means there is no maximum.
	/// The default value is <c>64</c>.
	/// </summary>
	public virtual int? MaxDepth
	{
		get
		{
			return this._maxDepth;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentException("Value must be positive.", "value");
			}
			this._maxDepth = value;
			this._maxDepthSet = true;
		}
	}

	/// <summary>
	/// Gets a value indicating whether there will be a check for additional JSON content after deserializing an object.
	/// The default value is <c>false</c>.
	/// </summary>
	/// <value>
	/// 	<c>true</c> if there will be a check for additional JSON content after deserializing an object; otherwise, <c>false</c>.
	/// </value>
	public virtual bool CheckAdditionalContent
	{
		get
		{
			return this._checkAdditionalContent == true;
		}
		set
		{
			this._checkAdditionalContent = value;
		}
	}

	/// <summary>
	/// Occurs when the <see cref="T:Newtonsoft.Json.JsonSerializer" /> errors during serialization and deserialization.
	/// </summary>
	public virtual event EventHandler<ErrorEventArgs>? Error;

	internal bool IsCheckAdditionalContentSet()
	{
		return this._checkAdditionalContent.HasValue;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonSerializer" /> class.
	/// </summary>
	public JsonSerializer()
	{
		this._referenceLoopHandling = ReferenceLoopHandling.Error;
		this._missingMemberHandling = MissingMemberHandling.Ignore;
		this._nullValueHandling = NullValueHandling.Include;
		this._defaultValueHandling = DefaultValueHandling.Include;
		this._objectCreationHandling = ObjectCreationHandling.Auto;
		this._preserveReferencesHandling = PreserveReferencesHandling.None;
		this._constructorHandling = ConstructorHandling.Default;
		this._typeNameHandling = TypeNameHandling.None;
		this._metadataPropertyHandling = MetadataPropertyHandling.Default;
		this._context = JsonSerializerSettings.DefaultContext;
		this._serializationBinder = DefaultSerializationBinder.Instance;
		this._culture = JsonSerializerSettings.DefaultCulture;
		this._contractResolver = DefaultContractResolver.Instance;
	}

	/// <summary>
	/// Creates a new <see cref="T:Newtonsoft.Json.JsonSerializer" /> instance.
	/// The <see cref="T:Newtonsoft.Json.JsonSerializer" /> will not use default settings 
	/// from <see cref="P:Newtonsoft.Json.JsonConvert.DefaultSettings" />.
	/// </summary>
	/// <returns>
	/// A new <see cref="T:Newtonsoft.Json.JsonSerializer" /> instance.
	/// The <see cref="T:Newtonsoft.Json.JsonSerializer" /> will not use default settings 
	/// from <see cref="P:Newtonsoft.Json.JsonConvert.DefaultSettings" />.
	/// </returns>
	public static JsonSerializer Create()
	{
		return new JsonSerializer();
	}

	/// <summary>
	/// Creates a new <see cref="T:Newtonsoft.Json.JsonSerializer" /> instance using the specified <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// The <see cref="T:Newtonsoft.Json.JsonSerializer" /> will not use default settings 
	/// from <see cref="P:Newtonsoft.Json.JsonConvert.DefaultSettings" />.
	/// </summary>
	/// <param name="settings">The settings to be applied to the <see cref="T:Newtonsoft.Json.JsonSerializer" />.</param>
	/// <returns>
	/// A new <see cref="T:Newtonsoft.Json.JsonSerializer" /> instance using the specified <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// The <see cref="T:Newtonsoft.Json.JsonSerializer" /> will not use default settings 
	/// from <see cref="P:Newtonsoft.Json.JsonConvert.DefaultSettings" />.
	/// </returns>
	public static JsonSerializer Create(JsonSerializerSettings? settings)
	{
		JsonSerializer jsonSerializer = JsonSerializer.Create();
		if (settings != null)
		{
			JsonSerializer.ApplySerializerSettings(jsonSerializer, settings);
		}
		return jsonSerializer;
	}

	/// <summary>
	/// Creates a new <see cref="T:Newtonsoft.Json.JsonSerializer" /> instance.
	/// The <see cref="T:Newtonsoft.Json.JsonSerializer" /> will use default settings 
	/// from <see cref="P:Newtonsoft.Json.JsonConvert.DefaultSettings" />.
	/// </summary>
	/// <returns>
	/// A new <see cref="T:Newtonsoft.Json.JsonSerializer" /> instance.
	/// The <see cref="T:Newtonsoft.Json.JsonSerializer" /> will use default settings 
	/// from <see cref="P:Newtonsoft.Json.JsonConvert.DefaultSettings" />.
	/// </returns>
	public static JsonSerializer CreateDefault()
	{
		return JsonSerializer.Create(JsonConvert.DefaultSettings?.Invoke());
	}

	/// <summary>
	/// Creates a new <see cref="T:Newtonsoft.Json.JsonSerializer" /> instance using the specified <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// The <see cref="T:Newtonsoft.Json.JsonSerializer" /> will use default settings 
	/// from <see cref="P:Newtonsoft.Json.JsonConvert.DefaultSettings" /> as well as the specified <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// </summary>
	/// <param name="settings">The settings to be applied to the <see cref="T:Newtonsoft.Json.JsonSerializer" />.</param>
	/// <returns>
	/// A new <see cref="T:Newtonsoft.Json.JsonSerializer" /> instance using the specified <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// The <see cref="T:Newtonsoft.Json.JsonSerializer" /> will use default settings 
	/// from <see cref="P:Newtonsoft.Json.JsonConvert.DefaultSettings" /> as well as the specified <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// </returns>
	public static JsonSerializer CreateDefault(JsonSerializerSettings? settings)
	{
		JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();
		if (settings != null)
		{
			JsonSerializer.ApplySerializerSettings(jsonSerializer, settings);
		}
		return jsonSerializer;
	}

	private static void ApplySerializerSettings(JsonSerializer serializer, JsonSerializerSettings settings)
	{
		if (!CollectionUtils.IsNullOrEmpty(settings.Converters))
		{
			for (int i = 0; i < settings.Converters.Count; i++)
			{
				serializer.Converters.Insert(i, settings.Converters[i]);
			}
		}
		if (settings._typeNameHandling.HasValue)
		{
			serializer.TypeNameHandling = settings.TypeNameHandling;
		}
		if (settings._metadataPropertyHandling.HasValue)
		{
			serializer.MetadataPropertyHandling = settings.MetadataPropertyHandling;
		}
		if (settings._typeNameAssemblyFormatHandling.HasValue)
		{
			serializer.TypeNameAssemblyFormatHandling = settings.TypeNameAssemblyFormatHandling;
		}
		if (settings._preserveReferencesHandling.HasValue)
		{
			serializer.PreserveReferencesHandling = settings.PreserveReferencesHandling;
		}
		if (settings._referenceLoopHandling.HasValue)
		{
			serializer.ReferenceLoopHandling = settings.ReferenceLoopHandling;
		}
		if (settings._missingMemberHandling.HasValue)
		{
			serializer.MissingMemberHandling = settings.MissingMemberHandling;
		}
		if (settings._objectCreationHandling.HasValue)
		{
			serializer.ObjectCreationHandling = settings.ObjectCreationHandling;
		}
		if (settings._nullValueHandling.HasValue)
		{
			serializer.NullValueHandling = settings.NullValueHandling;
		}
		if (settings._defaultValueHandling.HasValue)
		{
			serializer.DefaultValueHandling = settings.DefaultValueHandling;
		}
		if (settings._constructorHandling.HasValue)
		{
			serializer.ConstructorHandling = settings.ConstructorHandling;
		}
		if (settings._context.HasValue)
		{
			serializer.Context = settings.Context;
		}
		if (settings._checkAdditionalContent.HasValue)
		{
			serializer._checkAdditionalContent = settings._checkAdditionalContent;
		}
		if (settings.Error != null)
		{
			serializer.Error += settings.Error;
		}
		if (settings.ContractResolver != null)
		{
			serializer.ContractResolver = settings.ContractResolver;
		}
		if (settings.ReferenceResolverProvider != null)
		{
			serializer.ReferenceResolver = settings.ReferenceResolverProvider();
		}
		if (settings.TraceWriter != null)
		{
			serializer.TraceWriter = settings.TraceWriter;
		}
		if (settings.EqualityComparer != null)
		{
			serializer.EqualityComparer = settings.EqualityComparer;
		}
		if (settings.SerializationBinder != null)
		{
			serializer.SerializationBinder = settings.SerializationBinder;
		}
		if (settings._formatting.HasValue)
		{
			serializer._formatting = settings._formatting;
		}
		if (settings._dateFormatHandling.HasValue)
		{
			serializer._dateFormatHandling = settings._dateFormatHandling;
		}
		if (settings._dateTimeZoneHandling.HasValue)
		{
			serializer._dateTimeZoneHandling = settings._dateTimeZoneHandling;
		}
		if (settings._dateParseHandling.HasValue)
		{
			serializer._dateParseHandling = settings._dateParseHandling;
		}
		if (settings._dateFormatStringSet)
		{
			serializer._dateFormatString = settings._dateFormatString;
			serializer._dateFormatStringSet = settings._dateFormatStringSet;
		}
		if (settings._floatFormatHandling.HasValue)
		{
			serializer._floatFormatHandling = settings._floatFormatHandling;
		}
		if (settings._floatParseHandling.HasValue)
		{
			serializer._floatParseHandling = settings._floatParseHandling;
		}
		if (settings._stringEscapeHandling.HasValue)
		{
			serializer._stringEscapeHandling = settings._stringEscapeHandling;
		}
		if (settings._culture != null)
		{
			serializer._culture = settings._culture;
		}
		if (settings._maxDepthSet)
		{
			serializer._maxDepth = settings._maxDepth;
			serializer._maxDepthSet = settings._maxDepthSet;
		}
	}

	/// <summary>
	/// Populates the JSON values onto the target object.
	/// </summary>
	/// <param name="reader">The <see cref="T:System.IO.TextReader" /> that contains the JSON structure to read values from.</param>
	/// <param name="target">The target object to populate values onto.</param>
	[DebuggerStepThrough]
	public void Populate(TextReader reader, object target)
	{
		this.Populate(new JsonTextReader(reader), target);
	}

	/// <summary>
	/// Populates the JSON values onto the target object.
	/// </summary>
	/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> that contains the JSON structure to read values from.</param>
	/// <param name="target">The target object to populate values onto.</param>
	[DebuggerStepThrough]
	public void Populate(JsonReader reader, object target)
	{
		this.PopulateInternal(reader, target);
	}

	internal virtual void PopulateInternal(JsonReader reader, object target)
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		ValidationUtils.ArgumentNotNull(target, "target");
		this.SetupReader(reader, out CultureInfo previousCulture, out DateTimeZoneHandling? previousDateTimeZoneHandling, out DateParseHandling? previousDateParseHandling, out FloatParseHandling? previousFloatParseHandling, out int? previousMaxDepth, out string previousDateFormatString);
		TraceJsonReader traceJsonReader = ((this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose) ? this.CreateTraceJsonReader(reader) : null);
		new JsonSerializerInternalReader(this).Populate(traceJsonReader ?? reader, target);
		if (traceJsonReader != null)
		{
			this.TraceWriter.Trace(TraceLevel.Verbose, traceJsonReader.GetDeserializedJsonMessage(), null);
		}
		this.ResetReader(reader, previousCulture, previousDateTimeZoneHandling, previousDateParseHandling, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);
	}

	/// <summary>
	/// Deserializes the JSON structure contained by the specified <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> that contains the JSON structure to deserialize.</param>
	/// <returns>The <see cref="T:System.Object" /> being deserialized.</returns>
	[DebuggerStepThrough]
	public object? Deserialize(JsonReader reader)
	{
		return this.Deserialize(reader, null);
	}

	/// <summary>
	/// Deserializes the JSON structure contained by the specified <see cref="T:System.IO.TextReader" />
	/// into an instance of the specified type.
	/// </summary>
	/// <param name="reader">The <see cref="T:System.IO.TextReader" /> containing the object.</param>
	/// <param name="objectType">The <see cref="T:System.Type" /> of object being deserialized.</param>
	/// <returns>The instance of <paramref name="objectType" /> being deserialized.</returns>
	[DebuggerStepThrough]
	public object? Deserialize(TextReader reader, Type objectType)
	{
		return this.Deserialize(new JsonTextReader(reader), objectType);
	}

	/// <summary>
	/// Deserializes the JSON structure contained by the specified <see cref="T:Newtonsoft.Json.JsonReader" />
	/// into an instance of the specified type.
	/// </summary>
	/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> containing the object.</param>
	/// <typeparam name="T">The type of the object to deserialize.</typeparam>
	/// <returns>The instance of <typeparamref name="T" /> being deserialized.</returns>
	[DebuggerStepThrough]
	public T? Deserialize<T>(JsonReader reader)
	{
		return (T)this.Deserialize(reader, typeof(T));
	}

	/// <summary>
	/// Deserializes the JSON structure contained by the specified <see cref="T:Newtonsoft.Json.JsonReader" />
	/// into an instance of the specified type.
	/// </summary>
	/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> containing the object.</param>
	/// <param name="objectType">The <see cref="T:System.Type" /> of object being deserialized.</param>
	/// <returns>The instance of <paramref name="objectType" /> being deserialized.</returns>
	[DebuggerStepThrough]
	public object? Deserialize(JsonReader reader, Type? objectType)
	{
		return this.DeserializeInternal(reader, objectType);
	}

	internal virtual object? DeserializeInternal(JsonReader reader, Type? objectType)
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		this.SetupReader(reader, out CultureInfo previousCulture, out DateTimeZoneHandling? previousDateTimeZoneHandling, out DateParseHandling? previousDateParseHandling, out FloatParseHandling? previousFloatParseHandling, out int? previousMaxDepth, out string previousDateFormatString);
		TraceJsonReader traceJsonReader = ((this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose) ? this.CreateTraceJsonReader(reader) : null);
		object? result = new JsonSerializerInternalReader(this).Deserialize(traceJsonReader ?? reader, objectType, this.CheckAdditionalContent);
		if (traceJsonReader != null)
		{
			this.TraceWriter.Trace(TraceLevel.Verbose, traceJsonReader.GetDeserializedJsonMessage(), null);
		}
		this.ResetReader(reader, previousCulture, previousDateTimeZoneHandling, previousDateParseHandling, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);
		return result;
	}

	internal void SetupReader(JsonReader reader, out CultureInfo? previousCulture, out DateTimeZoneHandling? previousDateTimeZoneHandling, out DateParseHandling? previousDateParseHandling, out FloatParseHandling? previousFloatParseHandling, out int? previousMaxDepth, out string? previousDateFormatString)
	{
		if (this._culture != null && !this._culture.Equals(reader.Culture))
		{
			previousCulture = reader.Culture;
			reader.Culture = this._culture;
		}
		else
		{
			previousCulture = null;
		}
		if (this._dateTimeZoneHandling.HasValue && reader.DateTimeZoneHandling != this._dateTimeZoneHandling)
		{
			previousDateTimeZoneHandling = reader.DateTimeZoneHandling;
			reader.DateTimeZoneHandling = this._dateTimeZoneHandling.GetValueOrDefault();
		}
		else
		{
			previousDateTimeZoneHandling = null;
		}
		if (this._dateParseHandling.HasValue && reader.DateParseHandling != this._dateParseHandling)
		{
			previousDateParseHandling = reader.DateParseHandling;
			reader.DateParseHandling = this._dateParseHandling.GetValueOrDefault();
		}
		else
		{
			previousDateParseHandling = null;
		}
		if (this._floatParseHandling.HasValue && reader.FloatParseHandling != this._floatParseHandling)
		{
			previousFloatParseHandling = reader.FloatParseHandling;
			reader.FloatParseHandling = this._floatParseHandling.GetValueOrDefault();
		}
		else
		{
			previousFloatParseHandling = null;
		}
		if (this._maxDepthSet && reader.MaxDepth != this._maxDepth)
		{
			previousMaxDepth = reader.MaxDepth;
			reader.MaxDepth = this._maxDepth;
		}
		else
		{
			previousMaxDepth = null;
		}
		if (this._dateFormatStringSet && reader.DateFormatString != this._dateFormatString)
		{
			previousDateFormatString = reader.DateFormatString;
			reader.DateFormatString = this._dateFormatString;
		}
		else
		{
			previousDateFormatString = null;
		}
		if (reader is JsonTextReader { PropertyNameTable: null } jsonTextReader && this._contractResolver is DefaultContractResolver defaultContractResolver)
		{
			jsonTextReader.PropertyNameTable = defaultContractResolver.GetNameTable();
		}
	}

	private void ResetReader(JsonReader reader, CultureInfo? previousCulture, DateTimeZoneHandling? previousDateTimeZoneHandling, DateParseHandling? previousDateParseHandling, FloatParseHandling? previousFloatParseHandling, int? previousMaxDepth, string? previousDateFormatString)
	{
		if (previousCulture != null)
		{
			reader.Culture = previousCulture;
		}
		if (previousDateTimeZoneHandling.HasValue)
		{
			reader.DateTimeZoneHandling = previousDateTimeZoneHandling.GetValueOrDefault();
		}
		if (previousDateParseHandling.HasValue)
		{
			reader.DateParseHandling = previousDateParseHandling.GetValueOrDefault();
		}
		if (previousFloatParseHandling.HasValue)
		{
			reader.FloatParseHandling = previousFloatParseHandling.GetValueOrDefault();
		}
		if (this._maxDepthSet)
		{
			reader.MaxDepth = previousMaxDepth;
		}
		if (this._dateFormatStringSet)
		{
			reader.DateFormatString = previousDateFormatString;
		}
		if (reader is JsonTextReader { PropertyNameTable: not null } jsonTextReader && this._contractResolver is DefaultContractResolver defaultContractResolver && jsonTextReader.PropertyNameTable == defaultContractResolver.GetNameTable())
		{
			jsonTextReader.PropertyNameTable = null;
		}
	}

	/// <summary>
	/// Serializes the specified <see cref="T:System.Object" /> and writes the JSON structure
	/// using the specified <see cref="T:System.IO.TextWriter" />.
	/// </summary>
	/// <param name="textWriter">The <see cref="T:System.IO.TextWriter" /> used to write the JSON structure.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to serialize.</param>
	public void Serialize(TextWriter textWriter, object? value)
	{
		this.Serialize(new JsonTextWriter(textWriter), value);
	}

	/// <summary>
	/// Serializes the specified <see cref="T:System.Object" /> and writes the JSON structure
	/// using the specified <see cref="T:Newtonsoft.Json.JsonWriter" />.
	/// </summary>
	/// <param name="jsonWriter">The <see cref="T:Newtonsoft.Json.JsonWriter" /> used to write the JSON structure.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to serialize.</param>
	/// <param name="objectType">
	/// The type of the value being serialized.
	/// This parameter is used when <see cref="P:Newtonsoft.Json.JsonSerializer.TypeNameHandling" /> is <see cref="F:Newtonsoft.Json.TypeNameHandling.Auto" /> to write out the type name if the type of the value does not match.
	/// Specifying the type is optional.
	/// </param>
	public void Serialize(JsonWriter jsonWriter, object? value, Type? objectType)
	{
		this.SerializeInternal(jsonWriter, value, objectType);
	}

	/// <summary>
	/// Serializes the specified <see cref="T:System.Object" /> and writes the JSON structure
	/// using the specified <see cref="T:System.IO.TextWriter" />.
	/// </summary>
	/// <param name="textWriter">The <see cref="T:System.IO.TextWriter" /> used to write the JSON structure.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to serialize.</param>
	/// <param name="objectType">
	/// The type of the value being serialized.
	/// This parameter is used when <see cref="P:Newtonsoft.Json.JsonSerializer.TypeNameHandling" /> is Auto to write out the type name if the type of the value does not match.
	/// Specifying the type is optional.
	/// </param>
	public void Serialize(TextWriter textWriter, object? value, Type objectType)
	{
		this.Serialize(new JsonTextWriter(textWriter), value, objectType);
	}

	/// <summary>
	/// Serializes the specified <see cref="T:System.Object" /> and writes the JSON structure
	/// using the specified <see cref="T:Newtonsoft.Json.JsonWriter" />.
	/// </summary>
	/// <param name="jsonWriter">The <see cref="T:Newtonsoft.Json.JsonWriter" /> used to write the JSON structure.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to serialize.</param>
	public void Serialize(JsonWriter jsonWriter, object? value)
	{
		this.SerializeInternal(jsonWriter, value, null);
	}

	private TraceJsonReader CreateTraceJsonReader(JsonReader reader)
	{
		TraceJsonReader traceJsonReader = new TraceJsonReader(reader);
		if (reader.TokenType != JsonToken.None)
		{
			traceJsonReader.WriteCurrentToken();
		}
		return traceJsonReader;
	}

	internal virtual void SerializeInternal(JsonWriter jsonWriter, object? value, Type? objectType)
	{
		ValidationUtils.ArgumentNotNull(jsonWriter, "jsonWriter");
		Formatting? formatting = null;
		if (this._formatting.HasValue && jsonWriter.Formatting != this._formatting)
		{
			formatting = jsonWriter.Formatting;
			jsonWriter.Formatting = this._formatting.GetValueOrDefault();
		}
		DateFormatHandling? dateFormatHandling = null;
		if (this._dateFormatHandling.HasValue && jsonWriter.DateFormatHandling != this._dateFormatHandling)
		{
			dateFormatHandling = jsonWriter.DateFormatHandling;
			jsonWriter.DateFormatHandling = this._dateFormatHandling.GetValueOrDefault();
		}
		DateTimeZoneHandling? dateTimeZoneHandling = null;
		if (this._dateTimeZoneHandling.HasValue && jsonWriter.DateTimeZoneHandling != this._dateTimeZoneHandling)
		{
			dateTimeZoneHandling = jsonWriter.DateTimeZoneHandling;
			jsonWriter.DateTimeZoneHandling = this._dateTimeZoneHandling.GetValueOrDefault();
		}
		FloatFormatHandling? floatFormatHandling = null;
		if (this._floatFormatHandling.HasValue && jsonWriter.FloatFormatHandling != this._floatFormatHandling)
		{
			floatFormatHandling = jsonWriter.FloatFormatHandling;
			jsonWriter.FloatFormatHandling = this._floatFormatHandling.GetValueOrDefault();
		}
		StringEscapeHandling? stringEscapeHandling = null;
		if (this._stringEscapeHandling.HasValue && jsonWriter.StringEscapeHandling != this._stringEscapeHandling)
		{
			stringEscapeHandling = jsonWriter.StringEscapeHandling;
			jsonWriter.StringEscapeHandling = this._stringEscapeHandling.GetValueOrDefault();
		}
		CultureInfo cultureInfo = null;
		if (this._culture != null && !this._culture.Equals(jsonWriter.Culture))
		{
			cultureInfo = jsonWriter.Culture;
			jsonWriter.Culture = this._culture;
		}
		string dateFormatString = null;
		if (this._dateFormatStringSet && jsonWriter.DateFormatString != this._dateFormatString)
		{
			dateFormatString = jsonWriter.DateFormatString;
			jsonWriter.DateFormatString = this._dateFormatString;
		}
		TraceJsonWriter traceJsonWriter = ((this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose) ? new TraceJsonWriter(jsonWriter) : null);
		new JsonSerializerInternalWriter(this).Serialize(traceJsonWriter ?? jsonWriter, value, objectType);
		if (traceJsonWriter != null)
		{
			this.TraceWriter.Trace(TraceLevel.Verbose, traceJsonWriter.GetSerializedJsonMessage(), null);
		}
		if (formatting.HasValue)
		{
			jsonWriter.Formatting = formatting.GetValueOrDefault();
		}
		if (dateFormatHandling.HasValue)
		{
			jsonWriter.DateFormatHandling = dateFormatHandling.GetValueOrDefault();
		}
		if (dateTimeZoneHandling.HasValue)
		{
			jsonWriter.DateTimeZoneHandling = dateTimeZoneHandling.GetValueOrDefault();
		}
		if (floatFormatHandling.HasValue)
		{
			jsonWriter.FloatFormatHandling = floatFormatHandling.GetValueOrDefault();
		}
		if (stringEscapeHandling.HasValue)
		{
			jsonWriter.StringEscapeHandling = stringEscapeHandling.GetValueOrDefault();
		}
		if (this._dateFormatStringSet)
		{
			jsonWriter.DateFormatString = dateFormatString;
		}
		if (cultureInfo != null)
		{
			jsonWriter.Culture = cultureInfo;
		}
	}

	internal IReferenceResolver GetReferenceResolver()
	{
		if (this._referenceResolver == null)
		{
			this._referenceResolver = new DefaultReferenceResolver();
		}
		return this._referenceResolver;
	}

	internal JsonConverter? GetMatchingConverter(Type type)
	{
		return JsonSerializer.GetMatchingConverter(this._converters, type);
	}

	internal static JsonConverter? GetMatchingConverter(IList<JsonConverter>? converters, Type objectType)
	{
		if (converters != null)
		{
			for (int i = 0; i < converters.Count; i++)
			{
				JsonConverter jsonConverter = converters[i];
				if (jsonConverter.CanConvert(objectType))
				{
					return jsonConverter;
				}
			}
		}
		return null;
	}

	internal void OnError(ErrorEventArgs e)
	{
		this.Error?.Invoke(this, e);
	}
}
