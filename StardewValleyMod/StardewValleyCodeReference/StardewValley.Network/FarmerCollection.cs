using System;
using System.Collections;
using System.Collections.Generic;

namespace StardewValley.Network;

public class FarmerCollection : IEnumerable<Farmer>, IEnumerable
{
	public struct Enumerator : IEnumerator<Farmer>, IEnumerator, IDisposable
	{
		private GameLocation _locationFilter;

		private Dictionary<long, Netcode.NetRoot<Farmer>>.Enumerator _enumerator;

		private Farmer _player;

		private Farmer _current;

		private int _done;

		public Farmer Current => this._current;

		object IEnumerator.Current
		{
			get
			{
				if (this._done == 0)
				{
					throw new InvalidOperationException();
				}
				return this._current;
			}
		}

		public Enumerator(GameLocation locationFilter)
		{
			this._locationFilter = locationFilter;
			this._player = Game1.player;
			this._enumerator = Game1.otherFarmers.Roots.GetEnumerator();
			this._current = null;
			this._done = 2;
		}

		public bool MoveNext()
		{
			if (this._done == 2)
			{
				this._done = 1;
				if (this._locationFilter == null || (this._player.currentLocation != null && this._locationFilter.Equals(this._player.currentLocation)))
				{
					this._current = this._player;
					return true;
				}
			}
			while (this._enumerator.MoveNext())
			{
				Farmer player = this._enumerator.Current.Value.Value;
				if (player != this._player && (this._locationFilter == null || (player.currentLocation != null && this._locationFilter.Equals(player.currentLocation))))
				{
					this._current = player;
					return true;
				}
			}
			this._done = 0;
			this._current = null;
			return false;
		}

		public void Dispose()
		{
		}

		void IEnumerator.Reset()
		{
			this._player = Game1.player;
			this._enumerator = Game1.otherFarmers.Roots.GetEnumerator();
			this._current = null;
			this._done = 2;
		}
	}

	private GameLocation _locationFilter;

	public int Count
	{
		get
		{
			int count = 0;
			foreach (Farmer item in this)
			{
				_ = item;
				count++;
			}
			return count;
		}
	}

	public FarmerCollection(GameLocation locationFilter = null)
	{
		this._locationFilter = locationFilter;
	}

	public bool Any()
	{
		using (Enumerator enumerator = this.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				return true;
			}
		}
		return false;
	}

	public bool Contains(Farmer farmer)
	{
		foreach (Farmer item in this)
		{
			if (item == farmer)
			{
				return true;
			}
		}
		return false;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this._locationFilter);
	}

	IEnumerator<Farmer> IEnumerable<Farmer>.GetEnumerator()
	{
		return new Enumerator(this._locationFilter);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this._locationFilter);
	}
}
