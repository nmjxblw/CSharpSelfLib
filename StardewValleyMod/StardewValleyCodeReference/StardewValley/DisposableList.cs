using System;
using System.Collections.Generic;

namespace StardewValley;

[Obsolete("This is only kept for backwards compatibility. It should no longer be used, and no longer does anything besides wrap the provided list.")]
public struct DisposableList<T>
{
	public struct Enumerator : IDisposable
	{
		private readonly DisposableList<T> _parent;

		private int _index;

		public T Current
		{
			get
			{
				if (this._parent._list == null || this._index == 0)
				{
					throw new InvalidOperationException();
				}
				return this._parent._list[this._index - 1];
			}
		}

		public Enumerator(DisposableList<T> parent)
		{
			this._parent = parent;
			this._index = 0;
		}

		public bool MoveNext()
		{
			this._index++;
			if (this._parent._list != null)
			{
				return this._parent._list.Count >= this._index;
			}
			return false;
		}

		public void Reset()
		{
			this._index = 0;
		}

		public void Dispose()
		{
		}
	}

	private readonly List<T> _list;

	public DisposableList(List<T> list)
	{
		this._list = list;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}
}
