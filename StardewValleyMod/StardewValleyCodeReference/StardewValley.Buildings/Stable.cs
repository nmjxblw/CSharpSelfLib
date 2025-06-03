using System;
using Microsoft.Xna.Framework;
using StardewValley.Characters;
using StardewValley.Util;

namespace StardewValley.Buildings;

public class Stable : Building
{
	public Guid HorseId
	{
		get
		{
			return base.id.Value;
		}
		set
		{
			base.id.Value = value;
		}
	}

	public Stable()
		: this(Vector2.Zero)
	{
	}

	public Stable(Vector2 tileLocation)
		: this(tileLocation, GuidHelper.NewGuid())
	{
	}

	public Stable(Vector2 tileLocation, Guid horseId)
		: base("Stable", tileLocation)
	{
		this.HorseId = horseId;
	}

	public override Rectangle? getSourceRectForMenu()
	{
		return new Rectangle(0, 0, base.texture.Value.Bounds.Width, base.texture.Value.Bounds.Height);
	}

	public Horse getStableHorse()
	{
		return Utility.findHorse(this.HorseId);
	}

	/// <summary>Get the default tile position for a horse in this stable.</summary>
	public Point GetDefaultHorseTile()
	{
		return new Point(base.tileX.Value + 1, base.tileY.Value + 1);
	}

	public virtual void grabHorse()
	{
		if (base.daysOfConstructionLeft.Value <= 0)
		{
			Horse horse = Utility.findHorse(this.HorseId);
			Point defaultTile = this.GetDefaultHorseTile();
			if (horse == null)
			{
				horse = new Horse(this.HorseId, defaultTile.X, defaultTile.Y);
				base.GetParentLocation().characters.Add(horse);
			}
			else
			{
				Game1.warpCharacter(horse, base.parentLocationName.Value, defaultTile);
			}
			horse.ownerId.Value = base.owner.Value;
		}
	}

	public virtual void updateHorseOwnership()
	{
		if (base.daysOfConstructionLeft.Value > 0)
		{
			return;
		}
		Horse horse = Utility.findHorse(this.HorseId);
		if (horse == null)
		{
			return;
		}
		horse.ownerId.Value = base.owner.Value;
		if (horse.getOwner() != null)
		{
			if (horse.getOwner().horseName.Value != null)
			{
				horse.name.Value = horse.getOwner().horseName.Value;
				horse.displayName = horse.getOwner().horseName.Value;
			}
			else
			{
				horse.name.Value = "";
				horse.displayName = "";
			}
		}
	}

	public override void dayUpdate(int dayOfMonth)
	{
		base.dayUpdate(dayOfMonth);
		this.grabHorse();
	}

	/// <inheritdoc />
	public override void performActionOnDemolition(GameLocation location)
	{
		base.performActionOnDemolition(location);
		Horse horse = this.getStableHorse();
		horse?.currentLocation?.characters.Remove(horse);
	}
}
