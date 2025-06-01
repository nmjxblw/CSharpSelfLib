using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;

namespace StardewValley.Tools;

public class Lantern : Tool
{
	public const float baseRadius = 10f;

	public const int millisecondsPerFuelUnit = 6000;

	public const int maxFuel = 100;

	public int fuelLeft;

	private int fuelTimer;

	public bool on;

	[XmlIgnore]
	public string lightSourceId;

	public Lantern()
		: base("Lantern", 0, 74, 74, stackable: false)
	{
		base.InstantUse = true;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new Lantern();
	}

	public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
	{
		base.DoFunction(location, x, y, power, who);
		this.on = !this.on;
		base.CurrentParentTileIndex = base.IndexOfMenuItemView;
		Utility.removeLightSource(this.lightSourceId);
		if (this.on)
		{
			this.lightSourceId = this.GenerateLightSourceId(who);
			Game1.currentLightSources.Add(new LightSource(this.lightSourceId, 1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 2.5f + (float)this.fuelLeft / 100f * 10f * 0.75f, new Color(0, 131, 255), LightSource.LightContext.None, 0L));
		}
	}

	public override void tickUpdate(GameTime time, Farmer who)
	{
		if (this.on && this.fuelLeft > 0 && Game1.drawLighting)
		{
			this.fuelTimer += time.ElapsedGameTime.Milliseconds;
			if (this.fuelTimer > 6000)
			{
				this.fuelLeft--;
				this.fuelTimer = 0;
			}
			Vector2 lightPosition = new Vector2(who.Position.X + 21f, who.Position.Y + 64f);
			if (Game1.currentLightSources.TryGetValue(this.lightSourceId, out var light))
			{
				light.position.Value = lightPosition;
			}
			else
			{
				this.lightSourceId = this.GenerateLightSourceId(who);
				Game1.currentLightSources.Add(new LightSource(this.lightSourceId, 1, lightPosition, 2.5f + (float)this.fuelLeft / 100f * 10f * 0.75f, new Color(0, 131, 255), LightSource.LightContext.None, 0L));
			}
		}
		if (this.on && this.fuelLeft <= 0)
		{
			Utility.removeLightSource(this.GenerateLightSourceId(who));
		}
	}
}
