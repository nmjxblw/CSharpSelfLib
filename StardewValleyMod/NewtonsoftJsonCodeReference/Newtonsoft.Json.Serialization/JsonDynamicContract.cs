using System;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

/// <summary>
/// Contract details for a <see cref="T:System.Type" /> used by the <see cref="T:Newtonsoft.Json.JsonSerializer" />.
/// </summary>
public class JsonDynamicContract : JsonContainerContract
{
	private readonly ThreadSafeStore<string, CallSite<Func<CallSite, object, object>>> _callSiteGetters = new ThreadSafeStore<string, CallSite<Func<CallSite, object, object>>>(CreateCallSiteGetter);

	private readonly ThreadSafeStore<string, CallSite<Func<CallSite, object, object?, object>>> _callSiteSetters = new ThreadSafeStore<string, CallSite<Func<CallSite, object, object, object>>>(CreateCallSiteSetter);

	/// <summary>
	/// Gets the object's properties.
	/// </summary>
	/// <value>The object's properties.</value>
	public JsonPropertyCollection Properties { get; }

	/// <summary>
	/// Gets or sets the property name resolver.
	/// </summary>
	/// <value>The property name resolver.</value>
	public Func<string, string>? PropertyNameResolver { get; set; }

	private static CallSite<Func<CallSite, object, object>> CreateCallSiteGetter(string name)
	{
		return CallSite<Func<CallSite, object, object>>.Create(new NoThrowGetBinderMember((GetMemberBinder)DynamicUtils.BinderWrapper.GetMember(name, typeof(DynamicUtils))));
	}

	private static CallSite<Func<CallSite, object, object?, object>> CreateCallSiteSetter(string name)
	{
		return CallSite<Func<CallSite, object, object, object>>.Create(new NoThrowSetBinderMember((SetMemberBinder)DynamicUtils.BinderWrapper.SetMember(name, typeof(DynamicUtils))));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.JsonDynamicContract" /> class.
	/// </summary>
	/// <param name="underlyingType">The underlying type for the contract.</param>
	public JsonDynamicContract(Type underlyingType)
		: base(underlyingType)
	{
		base.ContractType = JsonContractType.Dynamic;
		this.Properties = new JsonPropertyCollection(base.UnderlyingType);
	}

	internal bool TryGetMember(IDynamicMetaObjectProvider dynamicProvider, string name, out object? value)
	{
		ValidationUtils.ArgumentNotNull(dynamicProvider, "dynamicProvider");
		CallSite<Func<CallSite, object, object>> callSite = this._callSiteGetters.Get(name);
		object obj = callSite.Target(callSite, dynamicProvider);
		if (obj != NoThrowExpressionVisitor.ErrorResult)
		{
			value = obj;
			return true;
		}
		value = null;
		return false;
	}

	internal bool TrySetMember(IDynamicMetaObjectProvider dynamicProvider, string name, object? value)
	{
		ValidationUtils.ArgumentNotNull(dynamicProvider, "dynamicProvider");
		CallSite<Func<CallSite, object, object, object>> callSite = this._callSiteSetters.Get(name);
		return callSite.Target(callSite, dynamicProvider, value) != NoThrowExpressionVisitor.ErrorResult;
	}
}
