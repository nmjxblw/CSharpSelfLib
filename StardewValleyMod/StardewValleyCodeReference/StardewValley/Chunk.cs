using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Network;

namespace StardewValley;

public class Chunk : INetObject<NetFields>
{
	/// <summary>The minimum number of milliseconds before an item in water sinks.</summary>
	public const int MinSinkTimer = 1900;

	/// <summary>The maximum number of milliseconds before an item in water sinks.</summary>
	public const int MaxSinkTimer = 2400;

	[XmlElement("position")]
	public NetPosition position = new NetPosition();

	[XmlIgnore]
	public readonly NetFloat xVelocity = new NetFloat().Interpolated(interpolate: true, wait: true);

	[XmlIgnore]
	public readonly NetFloat yVelocity = new NetFloat().Interpolated(interpolate: true, wait: true);

	[XmlIgnore]
	public readonly NetBool hasPassedRestingLineOnce = new NetBool(value: false);

	[XmlIgnore]
	public int bounces;

	/// <summary>If the item is floating in water, a visual Y pixel offset to apply for the bobbing animation.</summary>
	[XmlIgnore]
	public float bob;

	/// <summary>The number of milliseconds until this debris sinks, if it's in water and <see cref="F:StardewValley.Debris.isSinking" /> is true.</summary>
	public readonly NetInt sinkTimer = new NetInt();

	public readonly NetInt netDebrisType = new NetInt();

	[XmlIgnore]
	public bool hitWall;

	[XmlElement("xSpriteSheet")]
	public readonly NetInt xSpriteSheet = new NetInt();

	[XmlElement("ySpriteSheet")]
	public readonly NetInt ySpriteSheet = new NetInt();

	[XmlIgnore]
	public float rotation;

	[XmlIgnore]
	public float rotationVelocity;

	private readonly NetFloat netScale = new NetFloat().Interpolated(interpolate: true, wait: true);

	private readonly NetFloat netAlpha = new NetFloat();

	public int randomOffset
	{
		get
		{
			return this.netDebrisType.Value;
		}
		set
		{
			this.netDebrisType.Value = value;
		}
	}

	public float scale
	{
		get
		{
			return this.netScale.Value;
		}
		set
		{
			this.netScale.Value = value;
		}
	}

	public float alpha
	{
		get
		{
			return this.netAlpha.Value;
		}
		set
		{
			this.netAlpha.Value = value;
		}
	}

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("Chunk");

	public Chunk()
	{
		this.sinkTimer.Value = Game1.random.Next(1900, 2401);
		this.NetFields.SetOwner(this).AddField(this.position.NetFields, "position.NetFields").AddField(this.xVelocity, "xVelocity")
			.AddField(this.yVelocity, "yVelocity")
			.AddField(this.sinkTimer, "sinkTimer")
			.AddField(this.netDebrisType, "netDebrisType")
			.AddField(this.xSpriteSheet, "xSpriteSheet")
			.AddField(this.ySpriteSheet, "ySpriteSheet")
			.AddField(this.netScale, "netScale")
			.AddField(this.netAlpha, "netAlpha")
			.AddField(this.hasPassedRestingLineOnce, "hasPassedRestingLineOnce");
		if (LocalMultiplayer.IsLocalMultiplayer(is_local_only: true))
		{
			this.NetFields.DeltaAggregateTicks = 10;
		}
		else
		{
			this.NetFields.DeltaAggregateTicks = 30;
		}
	}

	public Chunk(Vector2 position, float xVelocity, float yVelocity, int random_offset)
		: this()
	{
		this.position.Value = position;
		this.xVelocity.Value = xVelocity;
		this.yVelocity.Value = yVelocity;
		this.randomOffset = random_offset;
		this.alpha = 1f;
	}

	public float getSpeed()
	{
		return (float)Math.Sqrt(this.xVelocity.Value * this.xVelocity.Value + this.yVelocity.Value * this.yVelocity.Value);
	}

	/// <summary>Get the visual pixel position, accounting for bob if it's sinking in water.</summary>
	public Vector2 GetVisualPosition()
	{
		if (this.bob == 0f)
		{
			return this.position.Value;
		}
		return new Vector2(this.position.X, this.position.Y + this.bob);
	}
}
