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
public class JsonArrayContract : JsonContainerContract
{
	private readonly Type? _genericCollectionDefinitionType;

	private Type? _genericWrapperType;

	private ObjectConstructor<object>? _genericWrapperCreator;

	private Func<object>? _genericTemporaryCollectionCreator;

	private readonly ConstructorInfo? _parameterizedConstructor;

	private ObjectConstructor<object>? _parameterizedCreator;

	private ObjectConstructor<object>? _overrideCreator;

	/// <summary>
	/// Gets the <see cref="T:System.Type" /> of the collection items.
	/// </summary>
	/// <value>The <see cref="T:System.Type" /> of the collection items.</value>
	public Type? CollectionItemType { get; }

	/// <summary>
	/// Gets a value indicating whether the collection type is a multidimensional array.
	/// </summary>
	/// <value><c>true</c> if the collection type is a multidimensional array; otherwise, <c>false</c>.</value>
	public bool IsMultidimensionalArray { get; }

	internal bool IsArray { get; }

	internal bool ShouldCreateWrapper { get; }

	internal bool CanDeserialize { get; private set; }

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
			this.CanDeserialize = true;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the creator has a parameter with the collection values.
	/// </summary>
	/// <value><c>true</c> if the creator has a parameter with the collection values; otherwise, <c>false</c>.</value>
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
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.JsonArrayContract" /> class.
	/// </summary>
	/// <param name="underlyingType">The underlying type for the contract.</param>
	public JsonArrayContract(Type underlyingType)
		: base(underlyingType)
	{
		base.ContractType = JsonContractType.Array;
		this.IsArray = base.CreatedType.IsArray || (base.NonNullableUnderlyingType.IsGenericType() && base.NonNullableUnderlyingType.GetGenericTypeDefinition().FullName == "System.Linq.EmptyPartition`1");
		bool canDeserialize;
		Type implementingType;
		if (this.IsArray)
		{
			this.CollectionItemType = ReflectionUtils.GetCollectionItemType(base.UnderlyingType);
			base.IsReadOnlyOrFixedSize = true;
			this._genericCollectionDefinitionType = typeof(List<>).MakeGenericType(this.CollectionItemType);
			canDeserialize = true;
			this.IsMultidimensionalArray = base.CreatedType.IsArray && base.UnderlyingType.GetArrayRank() > 1;
		}
		else if (typeof(IList).IsAssignableFrom(base.NonNullableUnderlyingType))
		{
			if (ReflectionUtils.ImplementsGenericDefinition(base.NonNullableUnderlyingType, typeof(ICollection<>), out this._genericCollectionDefinitionType))
			{
				this.CollectionItemType = this._genericCollectionDefinitionType.GetGenericArguments()[0];
			}
			else
			{
				this.CollectionItemType = ReflectionUtils.GetCollectionItemType(base.NonNullableUnderlyingType);
			}
			if (base.NonNullableUnderlyingType == typeof(IList))
			{
				base.CreatedType = typeof(List<object>);
			}
			if (this.CollectionItemType != null)
			{
				this._parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(base.NonNullableUnderlyingType, this.CollectionItemType);
			}
			base.IsReadOnlyOrFixedSize = ReflectionUtils.InheritsGenericDefinition(base.NonNullableUnderlyingType, typeof(ReadOnlyCollection<>));
			canDeserialize = true;
		}
		else if (ReflectionUtils.ImplementsGenericDefinition(base.NonNullableUnderlyingType, typeof(ICollection<>), out this._genericCollectionDefinitionType))
		{
			this.CollectionItemType = this._genericCollectionDefinitionType.GetGenericArguments()[0];
			if (ReflectionUtils.IsGenericDefinition(base.NonNullableUnderlyingType, typeof(ICollection<>)) || ReflectionUtils.IsGenericDefinition(base.NonNullableUnderlyingType, typeof(IList<>)))
			{
				base.CreatedType = typeof(List<>).MakeGenericType(this.CollectionItemType);
			}
			if (ReflectionUtils.IsGenericDefinition(base.NonNullableUnderlyingType, typeof(ISet<>)))
			{
				base.CreatedType = typeof(HashSet<>).MakeGenericType(this.CollectionItemType);
			}
			this._parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(base.NonNullableUnderlyingType, this.CollectionItemType);
			canDeserialize = true;
			this.ShouldCreateWrapper = true;
		}
		else if (ReflectionUtils.ImplementsGenericDefinition(base.NonNullableUnderlyingType, typeof(IReadOnlyCollection<>), out implementingType))
		{
			this.CollectionItemType = implementingType.GetGenericArguments()[0];
			if (ReflectionUtils.IsGenericDefinition(base.NonNullableUnderlyingType, typeof(IReadOnlyCollection<>)) || ReflectionUtils.IsGenericDefinition(base.NonNullableUnderlyingType, typeof(IReadOnlyList<>)))
			{
				base.CreatedType = typeof(ReadOnlyCollection<>).MakeGenericType(this.CollectionItemType);
			}
			this._genericCollectionDefinitionType = typeof(List<>).MakeGenericType(this.CollectionItemType);
			this._parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(base.CreatedType, this.CollectionItemType);
			this.StoreFSharpListCreatorIfNecessary(base.NonNullableUnderlyingType);
			base.IsReadOnlyOrFixedSize = true;
			canDeserialize = this.HasParameterizedCreatorInternal;
		}
		else if (ReflectionUtils.ImplementsGenericDefinition(base.NonNullableUnderlyingType, typeof(IEnumerable<>), out implementingType))
		{
			this.CollectionItemType = implementingType.GetGenericArguments()[0];
			if (ReflectionUtils.IsGenericDefinition(base.UnderlyingType, typeof(IEnumerable<>)))
			{
				base.CreatedType = typeof(List<>).MakeGenericType(this.CollectionItemType);
			}
			this._parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(base.NonNullableUnderlyingType, this.CollectionItemType);
			this.StoreFSharpListCreatorIfNecessary(base.NonNullableUnderlyingType);
			if (base.NonNullableUnderlyingType.IsGenericType() && base.NonNullableUnderlyingType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				this._genericCollectionDefinitionType = implementingType;
				base.IsReadOnlyOrFixedSize = false;
				this.ShouldCreateWrapper = false;
				canDeserialize = true;
			}
			else
			{
				this._genericCollectionDefinitionType = typeof(List<>).MakeGenericType(this.CollectionItemType);
				base.IsReadOnlyOrFixedSize = true;
				this.ShouldCreateWrapper = true;
				canDeserialize = this.HasParameterizedCreatorInternal;
			}
		}
		else
		{
			canDeserialize = false;
			this.ShouldCreateWrapper = true;
		}
		this.CanDeserialize = canDeserialize;
		if (this.CollectionItemType != null && ImmutableCollectionsUtils.TryBuildImmutableForArrayContract(base.NonNullableUnderlyingType, this.CollectionItemType, out Type createdType, out ObjectConstructor<object> parameterizedCreator))
		{
			base.CreatedType = createdType;
			this._parameterizedCreator = parameterizedCreator;
			base.IsReadOnlyOrFixedSize = true;
			this.CanDeserialize = true;
		}
	}

	internal IWrappedCollection CreateWrapper(object list)
	{
		if (this._genericWrapperCreator == null)
		{
			this._genericWrapperType = typeof(CollectionWrapper<>).MakeGenericType(this.CollectionItemType);
			Type type = ((!ReflectionUtils.InheritsGenericDefinition(this._genericCollectionDefinitionType, typeof(List<>)) && !(this._genericCollectionDefinitionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))) ? this._genericCollectionDefinitionType : typeof(ICollection<>).MakeGenericType(this.CollectionItemType));
			ConstructorInfo constructor = this._genericWrapperType.GetConstructor(new Type[1] { type });
			this._genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(constructor);
		}
		return (IWrappedCollection)this._genericWrapperCreator(list);
	}

	internal IList CreateTemporaryCollection()
	{
		if (this._genericTemporaryCollectionCreator == null)
		{
			Type type = ((this.IsMultidimensionalArray || this.CollectionItemType == null) ? typeof(object) : this.CollectionItemType);
			Type type2 = typeof(List<>).MakeGenericType(type);
			this._genericTemporaryCollectionCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(type2);
		}
		return (IList)this._genericTemporaryCollectionCreator();
	}

	private void StoreFSharpListCreatorIfNecessary(Type underlyingType)
	{
		if (!this.HasParameterizedCreatorInternal && underlyingType.Name == "FSharpList`1")
		{
			FSharpUtils.EnsureInitialized(underlyingType.Assembly());
			this._parameterizedCreator = FSharpUtils.Instance.CreateSeq(this.CollectionItemType);
		}
	}
}
