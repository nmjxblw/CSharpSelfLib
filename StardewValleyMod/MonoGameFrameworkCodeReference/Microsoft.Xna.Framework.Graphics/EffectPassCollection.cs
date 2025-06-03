using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics;

public class EffectPassCollection : IEnumerable<EffectPass>, IEnumerable
{
	public struct Enumerator : IEnumerator<EffectPass>, IEnumerator, IDisposable
	{
		private readonly EffectPass[] _array;

		private int _index;

		private EffectPass _current;

		public EffectPass Current => this._current;

		object IEnumerator.Current
		{
			get
			{
				if (this._index == this._array.Length + 1)
				{
					throw new InvalidOperationException();
				}
				return this.Current;
			}
		}

		internal Enumerator(EffectPass[] array)
		{
			this._array = array;
			this._index = 0;
			this._current = null;
		}

		public bool MoveNext()
		{
			if (this._index < this._array.Length)
			{
				this._current = this._array[this._index];
				this._index++;
				return true;
			}
			this._index = this._array.Length + 1;
			this._current = null;
			return false;
		}

		public void Dispose()
		{
		}

		void IEnumerator.Reset()
		{
			this._index = 0;
			this._current = null;
		}
	}

	private readonly EffectPass[] _passes;

	public EffectPass this[int index] => this._passes[index];

	public EffectPass this[string name]
	{
		get
		{
			EffectPass[] passes = this._passes;
			foreach (EffectPass pass in passes)
			{
				if (pass.Name == name)
				{
					return pass;
				}
			}
			return null;
		}
	}

	public int Count => this._passes.Length;

	internal EffectPassCollection(EffectPass[] passes)
	{
		this._passes = passes;
	}

	internal EffectPassCollection Clone(Effect effect)
	{
		EffectPass[] passes = new EffectPass[this._passes.Length];
		for (int i = 0; i < this._passes.Length; i++)
		{
			passes[i] = new EffectPass(effect, this._passes[i]);
		}
		return new EffectPassCollection(passes);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this._passes);
	}

	IEnumerator<EffectPass> IEnumerable<EffectPass>.GetEnumerator()
	{
		return ((IEnumerable<EffectPass>)this._passes).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this._passes.GetEnumerator();
	}
}
