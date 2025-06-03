namespace StardewValley;

public class LocationRequest
{
	public delegate void Callback();

	public string Name;

	public bool IsStructure;

	public GameLocation Location;

	public event Callback OnLoad;

	public event Callback OnWarp;

	public LocationRequest(string name, bool isStructure, GameLocation location)
	{
		this.Name = name;
		this.IsStructure = isStructure;
		this.Location = location;
	}

	public void Loaded(GameLocation location)
	{
		this.OnLoad?.Invoke();
	}

	public void Warped(GameLocation location)
	{
		this.OnWarp?.Invoke();
		Game1.player.ridingMineElevator = false;
		Game1.player.mount?.SyncPositionToRider();
		Game1.player.ClearCachedPosition();
		Game1.forceSnapOnNextViewportUpdate = true;
	}

	public bool IsRequestFor(GameLocation location)
	{
		if (!this.IsStructure && location.Name == this.Name)
		{
			return true;
		}
		if (location.NameOrUniqueName == this.Name)
		{
			return location.isStructure.Value;
		}
		return false;
	}

	public override string ToString()
	{
		return "LocationRequest(" + this.Name + ", " + this.IsStructure + ")";
	}
}
