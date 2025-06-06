using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.GameData.Machines;
using StardewValley.Locations;
using StardewValley.Tools;

namespace StardewValley.Objects;

public class Cask : Object
{
	public const int defaultDaysToMature = 56;

	[XmlElement("agingRate")]
	public readonly NetFloat agingRate = new NetFloat();

	[XmlElement("daysToMature")]
	public readonly NetFloat daysToMature = new NetFloat();

	/// <inheritdoc />
	public override string TypeDefinitionId => "(BC)";

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.agingRate, "agingRate").AddField(this.daysToMature, "daysToMature");
	}

	public Cask()
	{
	}

	public Cask(Vector2 v)
		: base(v, "163")
	{
	}

	public override bool performToolAction(Tool t)
	{
		if (t != null && t.isHeavyHitter() && !(t is MeleeWeapon))
		{
			if (base.heldObject.Value != null)
			{
				Game1.createItemDebris(base.heldObject.Value, base.tileLocation.Value * 64f, -1);
			}
			base.playNearbySoundAll("woodWhack");
			if (base.heldObject.Value == null)
			{
				return true;
			}
			base.heldObject.Value = null;
			base.readyForHarvest.Value = false;
			base.minutesUntilReady.Value = -1;
			return false;
		}
		return base.performToolAction(t);
	}

	public virtual bool IsValidCaskLocation()
	{
		GameLocation location = this.Location;
		if (location == null)
		{
			return false;
		}
		if (!(location is Cellar))
		{
			return location.HasMapPropertyWithValue("CanCaskHere");
		}
		return true;
	}

	/// <summary>Get the output item to produce for a cask.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.MachineOutputDelegate" />
	public static Item OutputCask(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady)
	{
		overrideMinutesUntilReady = null;
		if (!(machine is Cask cask))
		{
			return null;
		}
		if (!cask.IsValidCaskLocation())
		{
			if (Object.autoLoadFrom == null && !probe)
			{
				Game1.showRedMessageUsingLoadString("Strings\\Objects:CaskNoCellar");
			}
			return null;
		}
		if (cask.quality.Value >= 4)
		{
			return null;
		}
		if (inputItem.Quality >= 4)
		{
			return null;
		}
		float multiplier = 1f;
		if (outputData?.CustomData != null && outputData.CustomData.TryGetValue("AgingMultiplier", out var rawMultiplier) && (!float.TryParse(rawMultiplier, out multiplier) || multiplier <= 0f))
		{
			Game1.log.Error("Failed to parse cask aging multiplier '" + rawMultiplier + "' for trigger rule. This must be a positive float value.");
			return null;
		}
		if (multiplier > 0f)
		{
			Object output = (Object)inputItem.getOne();
			if (!probe)
			{
				cask.agingRate.Value = multiplier;
				cask.daysToMature.Value = cask.GetDaysForQuality(output.Quality);
				overrideMinutesUntilReady = ((output.Quality >= 4) ? 1 : 999999);
				return output;
			}
			return output;
		}
		return null;
	}

	/// <inheritdoc />
	public override bool TryApplyFairyDust(bool probe = false)
	{
		if (base.heldObject.Value == null)
		{
			return false;
		}
		if (base.heldObject.Value.Quality == 4)
		{
			return false;
		}
		if (!probe)
		{
			Utility.addSprinklesToLocation(this.Location, (int)base.tileLocation.X, (int)base.tileLocation.Y, 1, 2, 400, 40, Color.White);
			Game1.playSound("yoba");
			this.daysToMature.Value = this.GetDaysForQuality(this.GetNextQuality(base.heldObject.Value.Quality));
			this.checkForMaturity();
		}
		return true;
	}

	public override void DayUpdate()
	{
		base.DayUpdate();
		if (base.heldObject.Value != null)
		{
			base.minutesUntilReady.Value = 999999;
			this.daysToMature.Value -= this.agingRate.Value;
			this.checkForMaturity();
		}
	}

	public float GetDaysForQuality(int quality)
	{
		return quality switch
		{
			4 => 0f, 
			2 => 28f, 
			1 => 42f, 
			_ => 56f, 
		};
	}

	public int GetNextQuality(int quality)
	{
		switch (quality)
		{
		case 2:
		case 4:
			return 4;
		case 1:
			return 2;
		default:
			return 1;
		}
	}

	public void checkForMaturity()
	{
		if (this.daysToMature.Value <= this.GetDaysForQuality(this.GetNextQuality(base.heldObject.Value.quality.Value)))
		{
			base.heldObject.Value.quality.Value = this.GetNextQuality(base.heldObject.Value.quality.Value);
			if (base.heldObject.Value.Quality == 4)
			{
				base.minutesUntilReady.Value = 1;
			}
		}
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		base.draw(spriteBatch, x, y, alpha);
		Object value = base.heldObject.Value;
		if (value != null && value.quality.Value > 0)
		{
			Vector2 scaleFactor = ((base.MinutesUntilReady > 0) ? new Vector2(Math.Abs(base.scale.X - 5f), Math.Abs(base.scale.Y - 5f)) : Vector2.Zero);
			scaleFactor *= 4f;
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
			Rectangle destination = new Rectangle((int)(position.X + 32f - 8f - scaleFactor.X / 2f) + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y + 64f + 8f - scaleFactor.Y / 2f) + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(16f + scaleFactor.X), (int)(16f + scaleFactor.Y / 2f));
			spriteBatch.Draw(Game1.mouseCursors, destination, (base.heldObject.Value.quality.Value < 4) ? new Rectangle(338 + (base.heldObject.Value.quality.Value - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8), Color.White * 0.95f, 0f, Vector2.Zero, SpriteEffects.None, (float)((y + 1) * 64) / 10000f);
		}
	}
}
