using System;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Network;

/// <summary>A cached reference to a local location.</summary>
/// <remarks>This fetches and caches the location from <see cref="M:StardewValley.Game1.getLocationFromName(System.String)" /> based on the <see cref="P:StardewValley.Network.NetLocationRef.LocationName" /> and <see cref="P:StardewValley.Network.NetLocationRef.IsStructure" /> values.</remarks>
public class NetLocationRef : INetObject<NetFields>
{
	public readonly NetString locationName = new NetString();

	public readonly NetBool isStructure = new NetBool();

	protected GameLocation _gameLocation;

	protected bool _dirty = true;

	protected bool _usedLocalLocation;

	[XmlIgnore]
	public Action OnLocationChanged;

	/// <summary>The unique name of the target location.</summary>
	public string LocationName => this.locationName.Value;

	/// <summary>Whether the target location is a building interior.</summary>
	public bool IsStructure => this.isStructure.Value;

	/// <summary>The cached location instance.</summary>
	[XmlIgnore]
	public GameLocation Value
	{
		get
		{
			return this.Get();
		}
		set
		{
			this.Set(value);
		}
	}

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("NetLocationRef");

	public NetLocationRef()
	{
		this.NetFields.SetOwner(this).AddField(this.locationName, "locationName").AddField(this.isStructure, "isStructure");
		this.locationName.fieldChangeVisibleEvent += delegate
		{
			this._dirty = true;
		};
		this.isStructure.fieldChangeVisibleEvent += delegate
		{
			this._dirty = true;
		};
	}

	public NetLocationRef(GameLocation value)
		: this()
	{
		this.Set(value);
	}

	public bool IsChanging()
	{
		if (!this.locationName.IsChanging())
		{
			return this.isStructure.IsChanging();
		}
		return true;
	}

	/// <summary>Update the location instance if the <see cref="P:StardewValley.Network.NetLocationRef.LocationName" /> or <see cref="P:StardewValley.Network.NetLocationRef.IsStructure" /> values changed.</summary>
	/// <param name="forceUpdate">Whether to update the location reference even if the target values didn't change.</param>
	public void Update(bool forceUpdate = false)
	{
		if (forceUpdate)
		{
			this._dirty = true;
		}
		this.ApplyChangesIfDirty();
	}

	public void ApplyChangesIfDirty()
	{
		if (this._usedLocalLocation && this._gameLocation != Game1.currentLocation)
		{
			this._dirty = true;
			this._usedLocalLocation = false;
		}
		if (this._dirty)
		{
			this._gameLocation = Game1.getLocationFromName(this.locationName.Value, this.isStructure.Value);
			this._dirty = false;
			this.OnLocationChanged?.Invoke();
		}
		if (!this._usedLocalLocation && this._gameLocation != Game1.currentLocation && this.IsCurrentlyViewedLocation())
		{
			this._usedLocalLocation = true;
			this._gameLocation = Game1.currentLocation;
		}
	}

	public GameLocation Get()
	{
		this.ApplyChangesIfDirty();
		return this._gameLocation;
	}

	public void Set(GameLocation location)
	{
		if (location == null)
		{
			this.isStructure.Value = false;
			this.locationName.Value = "";
		}
		else
		{
			this.isStructure.Value = location.isStructure.Value;
			this.locationName.Value = location.NameOrUniqueName;
		}
		if (this.IsCurrentlyViewedLocation())
		{
			this._usedLocalLocation = true;
			this._gameLocation = Game1.currentLocation;
		}
		else
		{
			this._gameLocation = location;
		}
		if (this._gameLocation?.IsTemporary ?? false)
		{
			this._gameLocation = null;
		}
		this._dirty = false;
	}

	public bool IsCurrentlyViewedLocation()
	{
		if (Game1.currentLocation != null && this.locationName.Value == Game1.currentLocation.NameOrUniqueName)
		{
			return true;
		}
		return false;
	}
}
