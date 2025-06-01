using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

/// <summary>
/// Contract details for a <see cref="T:System.Type" /> used by the <see cref="T:Newtonsoft.Json.JsonSerializer" />.
/// </summary>
public class JsonDictionaryContract : JsonContainerContract
{
	private readonly Type? _genericCollectionDefinitionType;

	private Type? _genericWrapperType;

	private ObjectConstructor<object>? _genericWrapperCreator;

	private Func<object>? _genericTemporaryDictionaryCreator;

	private readonly ConstructorInfo? _parameterizedConstructor;

	private ObjectConstructor<object>? _overrideCreator;

	private ObjectConstructor<object>? _parameterizedCreator;

	/// <summary>
	/// Gets or sets the dictionary key resolver.
	/// </summary>
	/// <value>The dictionary key resolver.</value>
	public Func<string, string>? DictionaryKeyResolver { get; set; }

	/// <summary>
	/// Gets the <see cref="T:System.Type" /> of the dictionary keys.
	/// </summary>
	/// <value>The <see cref="T:System.Type" /> of the dictionary keys.</value>
	public Type? DictionaryKeyType { get; }

	/// <summary>
	/// Gets the <see cref="T:System.Type" /> of the dictionary values.
	/// </summary>
	/// <value>The <see cref="T:System.Type" /> of the dictionary values.</value>
	public Type? DictionaryValueType { get; }

	internal JsonContract? KeyContract { get; set; }

	internal bool ShouldCreateWrapper { get; }

	internal ObjectConstructor<object>? ParameterizedCreator
	{
		get
		{
			if (this._parameterizedCreator == null && this._parameterizedConstructor != null)
			{
				this._parameterizedCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(this._parameterizedConstructor);
			}
			return this._parameterizedCreator;
		}
	}

	/// <summary>
	/// Gets or sets the function used to create the object. When set this function will override <see cref="P:Newtonsoft.Json.Serialization.JsonContract.DefaultCreator" />.
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

	/// <summary>
	/// Gets a value indicating whether the creator has a parameter with the dictionary values.
	/// </summary>
	/// <value><c>true</c> if the creator has a parameter with the dictionary values; otherwise, <c>false</c>.</value>
	public bool HasParameterizedCreator { get; set; }

	internal bool HasParameterizedCreatorInternal
	{
		get
		{
			if (!this.HasParameterizedCreator && this._parameterizedCreator == null)
			{
				return this._parameterizedConstructor != null;
			}
			return true;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.JsonDictionaryContract" /> class.
	/// </summary>
	/// <param name="underlyingType">The underlying type for the contract.</param>
	public JsonDictionaryContract(Type underlyingType)
		: base(underlyingType)
	{
		base.ContractType = JsonContractType.Dictionary;
		Type keyType;
		Type valueType;
		if (ReflectionUtils.ImplementsGenericDefinition(base.NonNullableUnderlyingType, typeof(IDictionary<, >), out this._genericCollectionDefinitionType))
		{
			keyType = this._genericCollectionDefinitionType.GetGenericArguments()[0];
			valueType = this._genericCollectionDefinitionType.GetGenericArguments()[1];
			if (ReflectionUtils.IsGenericDefinition(base.NonNullableUnderlyingType, typeof(IDictionary<, >)))
			{
				base.CreatedType = typeof(Dictionary<, >).MakeGenericType(keyType, valueType);
			}
			else if (base.NonNullableUnderlyingType.IsGenericType() && base.NonNullableUnderlyingType.GetGenericTypeDefinition().FullName == "System.Collections.Concurrent.ConcurrentDictionary`2")
			{
				this.ShouldCreateWrapper = true;
			}
			base.IsReadOnlyOrFixedSize = ReflectionUtils.InheritsGenericDefinition(base.NonNullableUnderlyingType, typeof(ReadOnlyDictionary<, >));
		}
		else if (ReflectionUtils.ImplementsGenericDefinition(base.NonNullableUnderlyingType, typeof(IReadOnlyDictionary<, >), out this._genericCollectionDefinitionType))
		{
			keyType = this._genericCollectionDefinitionType.GetGenericArguments()[0];
			valueType = this._genericCollectionDefinitionType.GetGenericArguments()[1];
			if (ReflectionUtils.IsGenericDefinition(base.NonNullableUnderlyingType, typeof(IReadOnlyDictionary<, >)))
			{
				base.CreatedType = typeof(ReadOnlyDictionary<, >).MakeGenericType(keyType, valueType);
			}
			base.IsReadOnlyOrFixedSize = true;
		}
		else
		{
			ReflectionUtils.GetDictionaryKeyValueTypes(base.NonNullableUnderlyingType, out keyType, out valueType);
			if (base.NonNullableUnderlyingType == typeof(IDictionary))
			{
				base.CreatedType = typeof(Dictionary<object, object>);
			}
		}
		if (keyType != null && valueType != null)
		{
			this._parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(base.CreatedType, typeof(KeyValuePair<, >).MakeGenericType(keyType, valueType), typeof(IDictionary<, >).MakeGenericType(keyType, valueType));
			if (!this.HasParameterizedCreatorInternal && base.NonNullableUnderlyingType.Name == "FSharpMap`2")
			{
				FSharpUtils.EnsureInitialized(base.NonNullableUnderlyingType.Assembly());
				this._parameterizedCreator = FSharpUtils.Instance.CreateMap(keyType, valueType);
			}
		}
		if (!typeof(IDictionary).IsAssignableFrom(base.CreatedType))
		{
			this.ShouldCreateWrapper = true;
		}
		this.DictionaryKeyType = keyType;
		this.DictionaryValueType = valueType;
		if (this.DictionaryKeyType != null && this.DictionaryValueType != null && ImmutableCollectionsUtils.TryBuildImmutableForDictionaryContract(base.NonNullableUnderlyingType, this.DictionaryKeyType, this.DictionaryValueType, out Type createdType, out ObjectConstructor<object> parameterizedCreator))
		{
			base.CreatedType = createdType;
			this._parameterizedCreator = parameterizedCreator;
			base.IsReadOnlyOrFixedSize = true;
		}
	}

	internal IWrappedDictionary CreateWrapper(object dictionary)
	{
		if (this._genericWrapperCreator == null)
		{
			this._genericWrapperType = typeof(DictionaryWrapper<, >).MakeGenericType(this.DictionaryKeyType, this.DictionaryValueType);
			ConstructorInfo constructor = this._genericWrapperType.GetConstructor(new Type[1] { this._genericCollectionDefinitionType });
			this._genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(constructor);
		}
		return (IWrappedDictionary)this._genericWrapperCreator(dictionary);
	}

	internal IDictionary CreateTemporaryDictionary()
	{
		if (this._genericTemporaryDictionaryCreator == null)
		{
			Type type = typeof(Dictionary<, >).MakeGenericType(this.DictionaryKeyType ?? typeof(object), this.DictionaryValueType ?? typeof(object));
			this._genericTemporaryDictionaryCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(type);
		}
		return (IDictionary)this._genericTemporaryDictionaryCreator();
	}
}
