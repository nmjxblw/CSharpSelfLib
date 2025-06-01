using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace Newtonsoft.Json.Utilities;

internal sealed class DynamicProxyMetaObject<T> : DynamicMetaObject
{
	private delegate DynamicMetaObject Fallback(DynamicMetaObject? errorSuggestion);

	private sealed class GetBinderAdapter : GetMemberBinder
	{
		internal GetBinderAdapter(InvokeMemberBinder binder)
			: base(binder.Name, binder.IgnoreCase)
		{
		}

		public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject? errorSuggestion)
		{
			throw new NotSupportedException();
		}
	}

	private readonly DynamicProxy<T> _proxy;

	private static Expression[] NoArgs => CollectionUtils.ArrayEmpty<Expression>();

	internal DynamicProxyMetaObject(Expression expression, T value, DynamicProxy<T> proxy)
		: base(expression, BindingRestrictions.Empty, value)
	{
		this._proxy = proxy;
	}

	private bool IsOverridden(string method)
	{
		return ReflectionUtils.IsMethodOverridden(this._proxy.GetType(), typeof(DynamicProxy<T>), method);
	}

	public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
	{
		if (!this.IsOverridden("TryGetMember"))
		{
			return base.BindGetMember(binder);
		}
		return this.CallMethodWithResult("TryGetMember", binder, DynamicProxyMetaObject<T>.NoArgs, (DynamicMetaObject? e) => binder.FallbackGetMember(this, e));
	}

	public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
	{
		if (!this.IsOverridden("TrySetMember"))
		{
			return base.BindSetMember(binder, value);
		}
		return this.CallMethodReturnLast("TrySetMember", binder, DynamicProxyMetaObject<T>.GetArgs(value), (DynamicMetaObject? e) => binder.FallbackSetMember(this, value, e));
	}

	public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
	{
		if (!this.IsOverridden("TryDeleteMember"))
		{
			return base.BindDeleteMember(binder);
		}
		return this.CallMethodNoResult("TryDeleteMember", binder, DynamicProxyMetaObject<T>.NoArgs, (DynamicMetaObject? e) => binder.FallbackDeleteMember(this, e));
	}

	public override DynamicMetaObject BindConvert(ConvertBinder binder)
	{
		if (!this.IsOverridden("TryConvert"))
		{
			return base.BindConvert(binder);
		}
		return this.CallMethodWithResult("TryConvert", binder, DynamicProxyMetaObject<T>.NoArgs, (DynamicMetaObject? e) => binder.FallbackConvert(this, e));
	}

	public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
	{
		if (!this.IsOverridden("TryInvokeMember"))
		{
			return base.BindInvokeMember(binder, args);
		}
		Fallback fallback = (DynamicMetaObject? e) => binder.FallbackInvokeMember(this, args, e);
		return this.BuildCallMethodWithResult("TryInvokeMember", binder, DynamicProxyMetaObject<T>.GetArgArray(args), this.BuildCallMethodWithResult("TryGetMember", new GetBinderAdapter(binder), DynamicProxyMetaObject<T>.NoArgs, fallback(null), (DynamicMetaObject? e) => binder.FallbackInvoke(e, args, null)), null);
	}

	public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
	{
		if (!this.IsOverridden("TryCreateInstance"))
		{
			return base.BindCreateInstance(binder, args);
		}
		return this.CallMethodWithResult("TryCreateInstance", binder, DynamicProxyMetaObject<T>.GetArgArray(args), (DynamicMetaObject? e) => binder.FallbackCreateInstance(this, args, e));
	}

	public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
	{
		if (!this.IsOverridden("TryInvoke"))
		{
			return base.BindInvoke(binder, args);
		}
		return this.CallMethodWithResult("TryInvoke", binder, DynamicProxyMetaObject<T>.GetArgArray(args), (DynamicMetaObject? e) => binder.FallbackInvoke(this, args, e));
	}

	public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
	{
		if (!this.IsOverridden("TryBinaryOperation"))
		{
			return base.BindBinaryOperation(binder, arg);
		}
		return this.CallMethodWithResult("TryBinaryOperation", binder, DynamicProxyMetaObject<T>.GetArgs(arg), (DynamicMetaObject? e) => binder.FallbackBinaryOperation(this, arg, e));
	}

	public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
	{
		if (!this.IsOverridden("TryUnaryOperation"))
		{
			return base.BindUnaryOperation(binder);
		}
		return this.CallMethodWithResult("TryUnaryOperation", binder, DynamicProxyMetaObject<T>.NoArgs, (DynamicMetaObject? e) => binder.FallbackUnaryOperation(this, e));
	}

	public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
	{
		if (!this.IsOverridden("TryGetIndex"))
		{
			return base.BindGetIndex(binder, indexes);
		}
		return this.CallMethodWithResult("TryGetIndex", binder, DynamicProxyMetaObject<T>.GetArgArray(indexes), (DynamicMetaObject? e) => binder.FallbackGetIndex(this, indexes, e));
	}

	public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
	{
		if (!this.IsOverridden("TrySetIndex"))
		{
			return base.BindSetIndex(binder, indexes, value);
		}
		return this.CallMethodReturnLast("TrySetIndex", binder, DynamicProxyMetaObject<T>.GetArgArray(indexes, value), (DynamicMetaObject? e) => binder.FallbackSetIndex(this, indexes, value, e));
	}

	public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
	{
		if (!this.IsOverridden("TryDeleteIndex"))
		{
			return base.BindDeleteIndex(binder, indexes);
		}
		return this.CallMethodNoResult("TryDeleteIndex", binder, DynamicProxyMetaObject<T>.GetArgArray(indexes), (DynamicMetaObject? e) => binder.FallbackDeleteIndex(this, indexes, e));
	}

	private static IEnumerable<Expression> GetArgs(params DynamicMetaObject[] args)
	{
		return args.Select(delegate(DynamicMetaObject arg)
		{
			Expression expression = arg.Expression;
			return (!expression.Type.IsValueType()) ? expression : System.Linq.Expressions.Expression.Convert(expression, typeof(object));
		});
	}

	private static Expression[] GetArgArray(DynamicMetaObject[] args)
	{
		return new NewArrayExpression[1] { System.Linq.Expressions.Expression.NewArrayInit(typeof(object), DynamicProxyMetaObject<T>.GetArgs(args)) };
	}

	private static Expression[] GetArgArray(DynamicMetaObject[] args, DynamicMetaObject value)
	{
		Expression expression = value.Expression;
		return new Expression[2]
		{
			System.Linq.Expressions.Expression.NewArrayInit(typeof(object), DynamicProxyMetaObject<T>.GetArgs(args)),
			expression.Type.IsValueType() ? System.Linq.Expressions.Expression.Convert(expression, typeof(object)) : expression
		};
	}

	private static ConstantExpression Constant(DynamicMetaObjectBinder binder)
	{
		Type type = binder.GetType();
		while (!type.IsVisible())
		{
			type = type.BaseType();
		}
		return System.Linq.Expressions.Expression.Constant(binder, type);
	}

	/// <summary>
	/// Helper method for generating a MetaObject which calls a
	/// specific method on Dynamic that returns a result
	/// </summary>
	private DynamicMetaObject CallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, IEnumerable<Expression> args, Fallback fallback, Fallback? fallbackInvoke = null)
	{
		DynamicMetaObject fallbackResult = fallback(null);
		return this.BuildCallMethodWithResult(methodName, binder, args, fallbackResult, fallbackInvoke);
	}

	private DynamicMetaObject BuildCallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, IEnumerable<Expression> args, DynamicMetaObject fallbackResult, Fallback? fallbackInvoke)
	{
		ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(object), null);
		IList<Expression> list = new List<Expression>();
		list.Add(System.Linq.Expressions.Expression.Convert(base.Expression, typeof(T)));
		list.Add(DynamicProxyMetaObject<T>.Constant(binder));
		list.AddRange(args);
		list.Add(parameterExpression);
		DynamicMetaObject dynamicMetaObject = new DynamicMetaObject(parameterExpression, BindingRestrictions.Empty);
		if (binder.ReturnType != typeof(object))
		{
			dynamicMetaObject = new DynamicMetaObject(System.Linq.Expressions.Expression.Convert(dynamicMetaObject.Expression, binder.ReturnType), dynamicMetaObject.Restrictions);
		}
		if (fallbackInvoke != null)
		{
			dynamicMetaObject = fallbackInvoke(dynamicMetaObject);
		}
		return new DynamicMetaObject(System.Linq.Expressions.Expression.Block(new ParameterExpression[1] { parameterExpression }, System.Linq.Expressions.Expression.Condition(System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Constant(this._proxy), typeof(DynamicProxy<T>).GetMethod(methodName), list), dynamicMetaObject.Expression, fallbackResult.Expression, binder.ReturnType)), this.GetRestrictions().Merge(dynamicMetaObject.Restrictions).Merge(fallbackResult.Restrictions));
	}

	/// <summary>
	/// Helper method for generating a MetaObject which calls a
	/// specific method on Dynamic, but uses one of the arguments for
	/// the result.
	/// </summary>
	private DynamicMetaObject CallMethodReturnLast(string methodName, DynamicMetaObjectBinder binder, IEnumerable<Expression> args, Fallback fallback)
	{
		DynamicMetaObject dynamicMetaObject = fallback(null);
		ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(object), null);
		IList<Expression> list = new List<Expression>();
		list.Add(System.Linq.Expressions.Expression.Convert(base.Expression, typeof(T)));
		list.Add(DynamicProxyMetaObject<T>.Constant(binder));
		list.AddRange(args);
		list[list.Count - 1] = System.Linq.Expressions.Expression.Assign(parameterExpression, list[list.Count - 1]);
		return new DynamicMetaObject(System.Linq.Expressions.Expression.Block(new ParameterExpression[1] { parameterExpression }, System.Linq.Expressions.Expression.Condition(System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Constant(this._proxy), typeof(DynamicProxy<T>).GetMethod(methodName), list), parameterExpression, dynamicMetaObject.Expression, typeof(object))), this.GetRestrictions().Merge(dynamicMetaObject.Restrictions));
	}

	/// <summary>
	/// Helper method for generating a MetaObject which calls a
	/// specific method on Dynamic, but uses one of the arguments for
	/// the result.
	/// </summary>
	private DynamicMetaObject CallMethodNoResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, Fallback fallback)
	{
		DynamicMetaObject dynamicMetaObject = fallback(null);
		IList<Expression> list = new List<Expression>();
		list.Add(System.Linq.Expressions.Expression.Convert(base.Expression, typeof(T)));
		list.Add(DynamicProxyMetaObject<T>.Constant(binder));
		list.AddRange(args);
		return new DynamicMetaObject(System.Linq.Expressions.Expression.Condition(System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Constant(this._proxy), typeof(DynamicProxy<T>).GetMethod(methodName), list), System.Linq.Expressions.Expression.Empty(), dynamicMetaObject.Expression, typeof(void)), this.GetRestrictions().Merge(dynamicMetaObject.Restrictions));
	}

	/// <summary>
	/// Returns a Restrictions object which includes our current restrictions merged
	/// with a restriction limiting our type
	/// </summary>
	private BindingRestrictions GetRestrictions()
	{
		if (base.Value != null || !base.HasValue)
		{
			return BindingRestrictions.GetTypeRestriction(base.Expression, base.LimitType);
		}
		return BindingRestrictions.GetInstanceRestriction(base.Expression, null);
	}

	public override IEnumerable<string> GetDynamicMemberNames()
	{
		return this._proxy.GetDynamicMemberNames((T)base.Value);
	}
}
