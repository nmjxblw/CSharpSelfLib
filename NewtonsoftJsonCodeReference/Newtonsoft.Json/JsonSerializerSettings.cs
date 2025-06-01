using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json;

/// <summary>
/// Specifies the settings on a <see cref="T:Newtonsoft.Json.JsonSerializer" /> object.
/// </summary>
public class JsonSerializerSettings
{
	internal const ReferenceLoopHandling DefaultReferenceLoopHandling = ReferenceLoopHandling.Error;

	internal const MissingMemberHandling DefaultMissingMemberHandling = MissingMemberHandling.Ignore;

	internal const NullValueHandling DefaultNullValueHandling = NullValueHandling.Include;

	internal const DefaultValueHandling DefaultDefaultValueHandling = DefaultValueHandling.Include;

	internal const ObjectCreationHandling DefaultObjectCreationHandling = ObjectCreationHandling.Auto;

	internal const PreserveReferencesHandling DefaultPreserveReferencesHandling = PreserveReferencesHandling.None;

	internal const ConstructorHandling DefaultConstructorHandling = ConstructorHandling.Default;

	internal const TypeNameHandling DefaultTypeNameHandling = TypeNameHandling.None;

	internal const MetadataPropertyHandling DefaultMetadataPropertyHandling = MetadataPropertyHandling.Default;

	internal static readonly StreamingContext DefaultContext;

	internal const Formatting DefaultFormatting = Formatting.None;

	internal const DateFormatHandling DefaultDateFormatHandling = DateFormatHandling.IsoDateFormat;

	internal const DateTimeZoneHandling DefaultDateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;

	internal const DateParseHandling DefaultDateParseHandling = DateParseHandling.DateTime;

	internal const FloatParseHandling DefaultFloatParseHandling = FloatParseHandling.Double;

	internal const FloatFormatHandling DefaultFloatFormatHandling = FloatFormatHandling.String;

	internal const StringEscapeHandling DefaultStringEscapeHandling = StringEscapeHandling.Default;

	internal const TypeNameAssemblyFormatHandling DefaultTypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;

	internal static readonly CultureInfo DefaultCulture;

	internal const bool DefaultCheckAdditionalContent = false;

	internal const string DefaultDateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

	internal const int DefaultMaxDepth = 64;

	internal Formatting? _formatting;

	internal DateFormatHandling? _dateFormatHandling;

	internal DateTimeZoneHandling? _dateTimeZoneHandling;

	internal DateParseHandling? _dateParseHandling;

	internal FloatFormatHandling? _floatFormatHandling;

	internal FloatParseHandling? _floatParseHandling;

	internal StringEscapeHandling? _stringEscapeHandling;

	internal CultureInfo? _culture;

	internal bool? _checkAdditionalContent;

	internal int? _maxDepth;

	internal bool _maxDepthSet;

	internal string? _dateFormatString;

	internal bool _dateFormatStringSet;

	internal TypeNameAssemblyFormatHandling? _typeNameAssemblyFormatHandling;

	internal DefaultValueHandling? _defaultValueHandling;

	internal PreserveReferencesHandling? _preserveReferencesHandling;

	internal NullValueHandling? _nullValueHandling;

	internal ObjectCreationHandling? _objectCreationHandling;

	internal MissingMemberHandling? _missingMemberHandling;

	internal ReferenceLoopHandling? _referenceLoopHandling;

	internal StreamingContext? _context;

	internal ConstructorHandling? _constructorHandling;

	internal TypeNameHandling? _typeNameHandling;

	internal MetadataPropertyHandling? _metadataPropertyHandling;

	/// <summary>
	/// Gets or sets how reference loops (e.g. a class referencing itself) are handled.
	/// The default value is <see cref="F:Newtonsoft.Json.ReferenceLoopHandling.Error" />.
	/// </summary>
	/// <value>Reference loop handling.</value>
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
	/// Gets or sets how missing members (e.g. JSON contains a property that isn't a member on the object) are handled during deserialization.
	/// The default value is <see cref="F:Newtonsoft.Json.MissingMemberHandling.Ignore" />.
	/// </summary>
	/// <value>Missing member handling.</value>
	public MissingMemberHandling MissingMemberHandling
	{
		get
		{
			return this._missingMemberHandling.GetValueOrDefault();
		}
		set
		{
			this._missingMemberHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how objects are created during deserialization.
	/// The default value is <see cref="F:Newtonsoft.Json.ObjectCreationHandling.Auto" />.
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
	/// Gets or sets how null values are handled during serialization and deserialization.
	/// The default value is <see cref="F:Newtonsoft.Json.NullValueHandling.Include" />.
	/// </summary>
	/// <value>Null value handling.</value>
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
	/// Gets or sets how default values are handled during serialization and deserialization.
	/// The default value is <see cref="F:Newtonsoft.Json.DefaultValueHandling.Include" />.
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
	/// Gets or sets a <see cref="T:Newtonsoft.Json.JsonConverter" /> collection that will be used during serialization.
	/// </summary>
	/// <value>The converters.</value>
	public IList<JsonConverter> Converters { get; set; }

	/// <summary>
	/// Gets or sets how object references are preserved by the serializer.
	/// The default value is <see cref="F:Newtonsoft.Json.PreserveReferencesHandling.None" />.
	/// </summary>
	/// <value>The preserve references handling.</value>
	public PreserveReferencesHandling PreserveReferencesHandling
	{
		get
		{
			return this._preserveReferencesHandling.GetValueOrDefault();
		}
		set
		{
			this._preserveReferencesHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how type name writing and reading is handled by the serializer.
	/// The default value is <see cref="F:Newtonsoft.Json.TypeNameHandling.None" />.
	/// </summary>
	/// <remarks>
	/// <see cref="P:Newtonsoft.Json.JsonSerializerSettings.TypeNameHandling" /> should be used with caution when your application deserializes JSON from an external source.
	/// Incoming types should be validated with a custom <see cref="P:Newtonsoft.Json.JsonSerializerSettings.SerializationBinder" />
	/// when deserializing with a value other than <see cref="F:Newtonsoft.Json.TypeNameHandling.None" />.
	/// </remarks>
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
	/// Gets or sets how metadata properties are used during deserialization.
	/// The default value is <see cref="F:Newtonsoft.Json.MetadataPropertyHandling.Default" />.
	/// </summary>
	/// <value>The metadata properties handling.</value>
	public MetadataPropertyHandling MetadataPropertyHandling
	{
		get
		{
			return this._metadataPropertyHandling.GetValueOrDefault();
		}
		set
		{
			this._metadataPropertyHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how a type name assembly is written and resolved by the serializer.
	/// The default value is <see cref="F:System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple" />.
	/// </summary>
	/// <value>The type name assembly format.</value>
	[Obsolete("TypeNameAssemblyFormat is obsolete. Use TypeNameAssemblyFormatHandling instead.")]
	public FormatterAssemblyStyle TypeNameAssemblyFormat
	{
		get
		{
			return (FormatterAssemblyStyle)this.TypeNameAssemblyFormatHandling;
		}
		set
		{
			this.TypeNameAssemblyFormatHandling = (TypeNameAssemblyFormatHandling)value;
		}
	}

	/// <summary>
	/// Gets or sets how a type name assembly is written and resolved by the serializer.
	/// The default value is <see cref="F:Newtonsoft.Json.TypeNameAssemblyFormatHandling.Simple" />.
	/// </summary>
	/// <value>The type name assembly format.</value>
	public TypeNameAssemblyFormatHandling TypeNameAssemblyFormatHandling
	{
		get
		{
			return this._typeNameAssemblyFormatHandling.GetValueOrDefault();
		}
		set
		{
			this._typeNameAssemblyFormatHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how constructors are used during deserialization.
	/// The default value is <see cref="F:Newtonsoft.Json.ConstructorHandling.Default" />.
	/// </summary>
	/// <value>The constructor handling.</value>
	public ConstructorHandling ConstructorHandling
	{
		get
		{
			return this._constructorHandling.GetValueOrDefault();
		}
		set
		{
			this._constructorHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets the contract resolver used by the serializer when
	/// serializing .NET objects to JSON and vice versa.
	/// </summary>
	/// <value>The contract resolver.</value>
	public IContractResolver? ContractResolver { get; set; }

	/// <summary>
	/// Gets or sets the equality comparer used by the serializer when comparing references.
	/// </summary>
	/// <value>The equality comparer.</value>
	public IEqualityComparer? EqualityComparer { get; set; }

	/// <summary>
	/// Gets or sets the <see cref="T:Newtonsoft.Json.Serialization.IReferenceResolver" /> used by the serializer when resolving references.
	/// </summary>
	/// <value>The reference resolver.</value>
	[Obsolete("ReferenceResolver property is obsolete. Use the ReferenceResolverProvider property to set the IReferenceResolver: settings.ReferenceResolverProvider = () => resolver")]
	public IReferenceResolver? ReferenceResolver
	{
		get
		{
			return this.ReferenceResolverProvider?.Invoke();
		}
		set
		{
			this.ReferenceResolverProvider = ((value != null) ? ((Func<IReferenceResolver>)(() => value)) : null);
		}
	}

	/// <summary>
	/// Gets or sets a function that creates the <see cref="T:Newtonsoft.Json.Serialization.IReferenceResolver" /> used by the serializer when resolving references.
	/// </summary>
	/// <value>A function that creates the <see cref="T:Newtonsoft.Json.Serialization.IReferenceResolver" /> used by the serializer when resolving references.</value>
	public Func<IReferenceResolver?>? ReferenceResolverProvider { get; set; }

	/// <summary>
	/// Gets or sets the <see cref="T:Newtonsoft.Json.Serialization.ITraceWriter" /> used by the serializer when writing trace messages.
	/// </summary>
	/// <value>The trace writer.</value>
	public ITraceWriter? TraceWriter { get; set; }

	/// <summary>
	/// Gets or sets the <see cref="P:Newtonsoft.Json.JsonSerializerSettings.SerializationBinder" /> used by the serializer when resolving type names.
	/// </summary>
	/// <value>The binder.</value>
	[Obsolete("Binder is obsolete. Use SerializationBinder instead.")]
	public SerializationBinder? Binder
	{
		get
		{
			if (this.SerializationBinder == null)
			{
				return null;
			}
			if (this.SerializationBinder is SerializationBinderAdapter serializationBinderAdapter)
			{
				return serializationBinderAdapter.SerializationBinder;
			}
			throw new InvalidOperationException("Cannot get SerializationBinder because an ISerializationBinder was previously set.");
		}
		set
		{
			this.SerializationBinder = ((value == null) ? null : new SerializationBinderAdapter(value));
		}
	}

	/// <summary>
	/// Gets or sets the <see cref="T:Newtonsoft.Json.Serialization.ISerializationBinder" /> used by the serializer when resolving type names.
	/// </summary>
	/// <value>The binder.</value>
	public ISerializationBinder? SerializationBinder { get; set; }

	/// <summary>
	/// Gets or sets the error handler called during serialization and deserialization.
	/// </summary>
	/// <value>The error handler called during serialization and deserialization.</value>
	public EventHandler<ErrorEventArgs>? Error { get; set; }

	/// <summary>
	/// Gets or sets the <see cref="T:System.Runtime.Serialization.StreamingContext" /> used by the serializer when invoking serialization callback methods.
	/// </summary>
	/// <value>The context.</value>
	public StreamingContext Context
	{
		get
		{
			return this._context ?? JsonSerializerSettings.DefaultContext;
		}
		set
		{
			this._context = value;
		}
	}

	/// <summary>
	/// Gets or sets how <see cref="T:System.DateTime" /> and <see cref="T:System.DateTimeOffset" /> values are formatted when writing JSON text,
	/// and the expected date format when reading JSON text.
	/// The default value is <c>"yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK"</c>.
	/// </summary>
	public string DateFormatString
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
	/// Gets or sets the maximum depth allowed when reading JSON. Reading past this depth will throw a <see cref="T:Newtonsoft.Json.JsonReaderException" />.
	/// A null value means there is no maximum.
	/// The default value is <c>64</c>.
	/// </summary>
	public int? MaxDepth
	{
		get
		{
			if (!this._maxDepthSet)
			{
				return 64;
			}
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
	/// Indicates how JSON text output is formatted.
	/// The default value is <see cref="F:Newtonsoft.Json.Formatting.None" />.
	/// </summary>
	public Formatting Formatting
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
	public DateFormatHandling DateFormatHandling
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
	public DateTimeZoneHandling DateTimeZoneHandling
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
	public DateParseHandling DateParseHandling
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
	/// Gets or sets how special floating point numbers, e.g. <see cref="F:System.Double.NaN" />,
	/// <see cref="F:System.Double.PositiveInfinity" /> and <see cref="F:System.Double.NegativeInfinity" />,
	/// are written as JSON.
	/// The default value is <see cref="F:Newtonsoft.Json.FloatFormatHandling.String" />.
	/// </summary>
	public FloatFormatHandling FloatFormatHandling
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
	/// Gets or sets how floating point numbers, e.g. 1.0 and 9.9, are parsed when reading JSON text.
	/// The default value is <see cref="F:Newtonsoft.Json.FloatParseHandling.Double" />.
	/// </summary>
	public FloatParseHandling FloatParseHandling
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
	/// Gets or sets how strings are escaped when writing JSON text.
	/// The default value is <see cref="F:Newtonsoft.Json.StringEscapeHandling.Default" />.
	/// </summary>
	public StringEscapeHandling StringEscapeHandling
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
	/// Gets or sets the culture used when reading JSON.
	/// The default value is <see cref="P:System.Globalization.CultureInfo.InvariantCulture" />.
	/// </summary>
	public CultureInfo Culture
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
	/// Gets a value indicating whether there will be a check for additional content after deserializing an object.
	/// The default value is <c>false</c>.
	/// </summary>
	/// <value>
	/// 	<c>true</c> if there will be a check for additional content after deserializing an object; otherwise, <c>false</c>.
	/// </value>
	public bool CheckAdditionalContent
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

	static JsonSerializerSettings()
	{
		JsonSerializerSettings.DefaultContext = default(StreamingContext);
		JsonSerializerSettings.DefaultCulture = CultureInfo.InvariantCulture;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> class.
	/// </summary>
	[DebuggerStepThrough]
	public JsonSerializerSettings()
	{
		this.Converters = new List<JsonConverter>();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> class
	/// using values copied from the passed in <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// </summary>
	public JsonSerializerSettings(JsonSerializerSettings original)
	{
		this._floatParseHandling = original._floatParseHandling;
		this._floatFormatHandling = original._floatFormatHandling;
		this._dateParseHandling = original._dateParseHandling;
		this._dateTimeZoneHandling = original._dateTimeZoneHandling;
		this._dateFormatHandling = original._dateFormatHandling;
		this._formatting = original._formatting;
		this._maxDepth = original._maxDepth;
		this._maxDepthSet = original._maxDepthSet;
		this._dateFormatString = original._dateFormatString;
		this._dateFormatStringSet = original._dateFormatStringSet;
		this._context = original._context;
		this.Error = original.Error;
		this.SerializationBinder = original.SerializationBinder;
		this.TraceWriter = original.TraceWriter;
		this._culture = original._culture;
		this.ReferenceResolverProvider = original.ReferenceResolverProvider;
		this.EqualityComparer = original.EqualityComparer;
		this.ContractResolver = original.ContractResolver;
		this._constructorHandling = original._constructorHandling;
		this._typeNameAssemblyFormatHandling = original._typeNameAssemblyFormatHandling;
		this._metadataPropertyHandling = original._metadataPropertyHandling;
		this._typeNameHandling = original._typeNameHandling;
		this._preserveReferencesHandling = original._preserveReferencesHandling;
		this.Converters = original.Converters.ToList();
		this._defaultValueHandling = original._defaultValueHandling;
		this._nullValueHandling = original._nullValueHandling;
		this._objectCreationHandling = original._objectCreationHandling;
		this._missingMemberHandling = original._missingMemberHandling;
		this._referenceLoopHandling = original._referenceLoopHandling;
		this._checkAdditionalContent = original._checkAdditionalContent;
		this._stringEscapeHandling = original._stringEscapeHandling;
	}
}
