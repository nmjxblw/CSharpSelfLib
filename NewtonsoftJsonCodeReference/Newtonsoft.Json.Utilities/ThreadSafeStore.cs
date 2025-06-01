using System;
using System.Collections.Concurrent;

namespace Newtonsoft.Json.Utilities;

internal class ThreadSafeStore<TKey, TValue> where TKey : notnull
{
	private readonly ConcurrentDictionary<TKey, TValue> _concurrentStore;

	private readonly Func<TKey, TValue> _creator;

	public ThreadSafeStore(Func<TKey, TValue> creator)
	{
		ValidationUtils.ArgumentNotNull(creator, "creator");
		this._creator = creator;
		this._concurrentStore = new ConcurrentDictionary<TKey, TValue>();
	}

	public TValue Get(TKey key)
	{
		return this._concurrentStore.GetOrAdd(key, this._creator);
	}
}
