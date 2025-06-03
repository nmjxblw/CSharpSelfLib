using System;

namespace Pathoschild.Stardew.Common.Utilities;

internal class Cached<TValue>
{
	private readonly Func<string> GetCacheKey;

	private readonly Func<TValue?, TValue> FetchNew;

	private string? LastCacheKey;

	private TValue? LastValue;

	public TValue Value
	{
		get
		{
			string cacheKey = this.GetCacheKey();
			if (cacheKey != this.LastCacheKey)
			{
				this.LastCacheKey = cacheKey;
				this.LastValue = this.FetchNew(this.LastValue);
			}
			return this.LastValue;
		}
	}

	public Cached(Func<string> getCacheKey, Func<TValue> fetchNew)
		: this(getCacheKey, (Func<TValue?, TValue>)((TValue? _) => fetchNew()))
	{
	}

	public Cached(Func<string> getCacheKey, Func<TValue?, TValue> fetchNew)
	{
		this.GetCacheKey = getCacheKey;
		this.FetchNew = fetchNew;
	}
}
