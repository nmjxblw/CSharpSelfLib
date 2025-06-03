using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Graphics;

public sealed class ModelEffectCollection : ReadOnlyCollection<Effect>
{
	public struct Enumerator : IEnumerator<Effect>, IEnumerator, IDisposable
	{
		private List<Effect>.Enumerator enumerator;

		private bool disposed;

		public Effect Current => this.enumerator.Current;

		object IEnumerator.Current => this.Current;

		internal Enumerator(List<Effect> list)
		{
			this.enumerator = list.GetEnumerator();
			this.disposed = false;
		}

		public void Dispose()
		{
			if (!this.disposed)
			{
				this.enumerator.Dispose();
				this.disposed = true;
			}
		}

		public bool MoveNext()
		{
			return this.enumerator.MoveNext();
		}

		void IEnumerator.Reset()
		{
			IEnumerator resetEnumerator = this.enumerator;
			resetEnumerator.Reset();
			this.enumerator = (List<Effect>.Enumerator)(object)resetEnumerator;
		}
	}

	internal ModelEffectCollection(IList<Effect> list)
		: base(list)
	{
	}

	internal ModelEffectCollection()
		: base((IList<Effect>)new List<Effect>())
	{
	}

	internal void Add(Effect item)
	{
		base.Items.Add(item);
	}

	internal void Remove(Effect item)
	{
		base.Items.Remove(item);
	}

	public new Enumerator GetEnumerator()
	{
		return new Enumerator((List<Effect>)base.Items);
	}
}
