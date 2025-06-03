using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Network;

namespace StardewValley;

public class InteriorDoorDictionary : NetPointDictionary<bool, InteriorDoor>
{
	public struct DoorCollection : IEnumerable<InteriorDoor>, IEnumerable
	{
		public struct Enumerator : IEnumerator<InteriorDoor>, IEnumerator, IDisposable
		{
			private readonly InteriorDoorDictionary _dict;

			private Dictionary<Point, InteriorDoor>.Enumerator _enumerator;

			private InteriorDoor _current;

			private bool _done;

			public InteriorDoor Current => this._current;

			object IEnumerator.Current
			{
				get
				{
					if (this._done)
					{
						throw new InvalidOperationException();
					}
					return this._current;
				}
			}

			public Enumerator(InteriorDoorDictionary dict)
			{
				this._dict = dict;
				this._enumerator = this._dict.FieldDict.GetEnumerator();
				this._current = null;
				this._done = false;
			}

			public bool MoveNext()
			{
				if (this._enumerator.MoveNext())
				{
					KeyValuePair<Point, InteriorDoor> pair = this._enumerator.Current;
					this._current = pair.Value;
					this._current.Location = this._dict.location;
					this._current.Position = pair.Key;
					return true;
				}
				this._done = true;
				this._current = null;
				return false;
			}

			public void Dispose()
			{
			}

			void IEnumerator.Reset()
			{
				this._enumerator = this._dict.FieldDict.GetEnumerator();
				this._current = null;
				this._done = false;
			}
		}

		private InteriorDoorDictionary _dict;

		public DoorCollection(InteriorDoorDictionary dict)
		{
			this._dict = dict;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this._dict);
		}

		IEnumerator<InteriorDoor> IEnumerable<InteriorDoor>.GetEnumerator()
		{
			return new Enumerator(this._dict);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this._dict);
		}
	}

	private GameLocation location;

	public DoorCollection Doors => new DoorCollection(this);

	public InteriorDoorDictionary(GameLocation location)
	{
		this.location = location;
	}

	protected override void setFieldValue(InteriorDoor door, Point position, bool open)
	{
		door.Location = this.location;
		door.Position = position;
		base.setFieldValue(door, position, open);
	}

	public void ResetSharedState()
	{
		if (this.location.isOutdoors.Value)
		{
			return;
		}
		foreach (Point tile in InteriorDoorDictionary.GetDoorTilesFromMapProperty(this.location))
		{
			base[tile] = false;
		}
	}

	public void ResetLocalState()
	{
		if (this.location.isOutdoors.Value)
		{
			return;
		}
		foreach (Point doorPoint in InteriorDoorDictionary.GetDoorTilesFromMapProperty(this.location))
		{
			if (base.ContainsKey(doorPoint))
			{
				InteriorDoor interiorDoor = base.FieldDict[doorPoint];
				interiorDoor.Location = this.location;
				interiorDoor.Position = doorPoint;
				interiorDoor.ResetLocalState();
			}
		}
	}

	/// <summary>Get the tile positions containing doors based on the <c>Doors</c> map property.</summary>
	/// <param name="location">The location whose map property to read.</param>
	public static IEnumerable<Point> GetDoorTilesFromMapProperty(GameLocation location)
	{
		string[] fields = location.GetMapPropertySplitBySpaces("Doors");
		for (int i = 0; i < fields.Length; i += 4)
		{
			if (ArgUtility.TryGetPoint(fields, i, out var tile, out var error, "Point tile"))
			{
				yield return tile;
			}
			else
			{
				location.LogMapPropertyError("Doors", fields, error);
			}
		}
	}

	public void MakeMapModifications()
	{
		foreach (InteriorDoor door in this.Doors)
		{
			door.ApplyMapModifications();
		}
	}

	public void CleanUpLocalState()
	{
		foreach (InteriorDoor door in this.Doors)
		{
			door.CleanUpLocalState();
		}
	}

	public void Update(GameTime time)
	{
		foreach (InteriorDoor door in this.Doors)
		{
			door.Update(time);
		}
	}

	public void Draw(SpriteBatch b)
	{
		foreach (InteriorDoor door in this.Doors)
		{
			door.Draw(b);
		}
	}
}
