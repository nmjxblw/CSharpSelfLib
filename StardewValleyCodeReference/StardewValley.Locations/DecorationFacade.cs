using System;
using System.Collections.Generic;
using System.Linq;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Locations;

public class DecorationFacade : SerializationCollectionFacade<int>
{
	public delegate void ChangeEvent(int whichRoom, int which);

	public readonly NetIntDictionary<int, NetInt> Field = new NetIntDictionary<int, NetInt>
	{
		InterpolationWait = false
	};

	private List<Action> pendingChanges = new List<Action>();

	[NonInstancedStatic]
	public static bool warnedDeprecated;

	public int this[int whichRoom]
	{
		get
		{
			this.WarnDeprecation();
			if (!this.Field.TryGetValue(whichRoom, out var value))
			{
				return 0;
			}
			return value;
		}
		set
		{
			this.WarnDeprecation();
			this.Field[whichRoom] = value;
		}
	}

	public int Count
	{
		get
		{
			if (this.Field.Length == 0)
			{
				return 0;
			}
			return this.Field.Keys.Max() + 1;
		}
	}

	public event ChangeEvent OnChange;

	public DecorationFacade()
	{
		this.Field.OnValueAdded += delegate(int whichRoom, int which)
		{
			this.Field.InterpolationWait = false;
			this.Field.FieldDict[whichRoom].fieldChangeEvent += delegate(NetInt field, int oldValue, int newValue)
			{
				this.changed(whichRoom, newValue);
			};
			this.changed(whichRoom, which);
		};
	}

	private void changed(int whichRoom, int which)
	{
		this.pendingChanges.Add(delegate
		{
			this.OnChange?.Invoke(whichRoom, which);
		});
	}

	protected override List<int> Serialize()
	{
		List<int> result = new List<int>();
		while (result.Count < this.Count)
		{
			result.Add(0);
		}
		foreach (KeyValuePair<int, int> pair in this.Field.Pairs)
		{
			result[pair.Key] = pair.Value;
		}
		return result;
	}

	protected override void DeserializeAdd(int serialValue)
	{
		this.Field[this.Count] = serialValue;
	}

	public void Set(DecorationFacade other)
	{
		this.Field.Set(other.Field.Pairs);
	}

	public void SetCountAtLeast(int targetCount)
	{
		while (this.Count < targetCount)
		{
			this[this.Count] = 0;
		}
	}

	public void Update()
	{
		foreach (Action pendingChange in this.pendingChanges)
		{
			pendingChange();
		}
		this.pendingChanges.Clear();
	}

	public virtual void WarnDeprecation()
	{
		if (Game1.gameMode != 6 && !DecorationFacade.warnedDeprecated)
		{
			DecorationFacade.warnedDeprecated = true;
			Game1.log.Warn("WARNING: DecorationFacade/DecoratableLocation.wallPaper and floor are deprecated. Use wallpaperIDs, appliedWallpaper, wallPaperTiles/floorIDs, appliedFloor, and floorTiles instead.");
		}
	}
}
