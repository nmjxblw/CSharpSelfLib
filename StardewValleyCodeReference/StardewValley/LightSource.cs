using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Netcode.Validation;

namespace StardewValley;

[NotImplicitNetField]
public class LightSource : INetObject<NetFields>
{
	public enum LightContext
	{
		None,
		MapLight,
		WindowLight
	}

	public const int lantern = 1;

	public const int windowLight = 2;

	public const int sconceLight = 4;

	public const int cauldronLight = 5;

	public const int indoorWindowLight = 6;

	public const int projectorLight = 7;

	public const int fishTankLight = 8;

	public const int townWinterTreeLight = 9;

	public const int pinpointLight = 10;

	/// <summary>The sprite index within the <see cref="F:StardewValley.LightSource.lightTexture" />, usually matching a constant like <see cref="F:StardewValley.LightSource.lantern" />.</summary>
	public readonly NetInt textureIndex = new NetInt().Interpolated(interpolate: false, wait: false);

	/// <summary>The texture to draw for the light.</summary>
	public Texture2D lightTexture;

	/// <summary>The pixel position relative to the top-left corner of the location's map.</summary>
	public readonly NetVector2 position = new NetVector2().Interpolated(interpolate: true, wait: true);

	/// <summary>The tint color.</summary>
	public readonly NetColor color = new NetColor();

	/// <summary>The light radius.</summary>
	public readonly NetFloat radius = new NetFloat();

	/// <summary>A globally unique identifier for this light source.</summary>
	/// <remarks>Most code should use <see cref="P:StardewValley.LightSource.Id" /> instead.</remarks>
	public readonly NetString netId = new NetString();

	/// <summary>The light context.</summary>
	public readonly NetEnum<LightContext> lightContext = new NetEnum<LightContext>();

	/// <summary>The player to which the light is attached.</summary>
	public readonly NetLong playerID = new NetLong(0L).Interpolated(interpolate: false, wait: false);

	public readonly NetInt fadeOut = new NetInt(-1);

	/// <summary>If set, only render this light source if the player is in this location name.</summary>
	public readonly NetString onlyLocation = new NetString();

	/// <summary>A globally unique identifier for this light source.</summary>
	public string Id
	{
		get
		{
			return this.netId.Value;
		}
		set
		{
			this.netId.Value = value;
		}
	}

	public long PlayerID
	{
		get
		{
			return this.playerID.Value;
		}
		set
		{
			this.playerID.Value = value;
		}
	}

	public NetFields NetFields { get; } = new NetFields("LightSource");

	/// <summary>Construct an empty instance.</summary>
	public LightSource()
	{
		this.NetFields.SetOwner(this).AddField(this.textureIndex, "textureIndex").AddField(this.position, "position")
			.AddField(this.color, "color")
			.AddField(this.radius, "radius")
			.AddField(this.netId, "netId")
			.AddField(this.lightContext, "lightContext")
			.AddField(this.playerID, "playerID")
			.AddField(this.fadeOut, "fadeOut")
			.AddField(this.onlyLocation, "onlyLocation");
		this.textureIndex.fieldChangeEvent += delegate(NetInt field, int oldValue, int newValue)
		{
			this.loadTextureFromConstantValue(newValue);
		};
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="id"><inheritdoc cref="F:StardewValley.LightSource.netId" path="/summary" /></param>
	/// <param name="textureIndex"><inheritdoc cref="F:StardewValley.LightSource.textureIndex" path="/summary" /></param>
	/// <param name="position"><inheritdoc cref="F:StardewValley.LightSource.position" path="/summary" /></param>
	/// <param name="radius"><inheritdoc cref="F:StardewValley.LightSource.radius" path="/summary" /></param>
	/// <param name="color"><inheritdoc cref="F:StardewValley.LightSource.color" path="/summary" /></param>
	/// <param name="lightContext"><inheritdoc cref="F:StardewValley.LightSource.lightContext" path="/summary" /></param>
	/// <param name="playerID"><inheritdoc cref="F:StardewValley.LightSource.playerID" path="/summary" /></param>
	/// <param name="onlyLocation"><inheritdoc cref="F:StardewValley.LightSource.onlyLocation" path="/summary" /></param>
	public LightSource(string id, int textureIndex, Vector2 position, float radius, Color color, LightContext lightContext = LightContext.None, long playerID = 0L, string onlyLocation = null)
		: this()
	{
		this.netId.Value = id;
		this.textureIndex.Value = textureIndex;
		this.position.Value = position;
		this.radius.Value = radius;
		this.color.Value = color;
		this.lightContext.Value = lightContext;
		this.playerID.Value = playerID;
		this.onlyLocation.Value = onlyLocation;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="id"><inheritdoc cref="F:StardewValley.LightSource.netId" path="/summary" /></param>
	/// <param name="textureIndex"><inheritdoc cref="F:StardewValley.LightSource.textureIndex" path="/summary" /></param>
	/// <param name="position"><inheritdoc cref="F:StardewValley.LightSource.position" path="/summary" /></param>
	/// <param name="radius"><inheritdoc cref="F:StardewValley.LightSource.radius" path="/summary" /></param>
	/// <param name="lightContext"><inheritdoc cref="F:StardewValley.LightSource.lightContext" path="/summary" /></param>
	/// <param name="playerID"><inheritdoc cref="F:StardewValley.LightSource.playerID" path="/summary" /></param>
	/// <param name="onlyLocation"><inheritdoc cref="F:StardewValley.LightSource.onlyLocation" path="/summary" /></param>
	public LightSource(string id, int textureIndex, Vector2 position, float radius, LightContext lightContext = LightContext.None, long playerID = 0L, string onlyLocation = null)
		: this(id, textureIndex, position, radius, Color.Black, lightContext, playerID, onlyLocation)
	{
	}

	private void loadTextureFromConstantValue(int value)
	{
		switch (value)
		{
		case 1:
			this.lightTexture = Game1.lantern;
			break;
		case 2:
			this.lightTexture = Game1.windowLight;
			break;
		case 4:
			this.lightTexture = Game1.sconceLight;
			break;
		case 5:
			this.lightTexture = Game1.cauldronLight;
			break;
		case 6:
			this.lightTexture = Game1.indoorWindowLight;
			break;
		case 7:
			this.lightTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Lighting\\projectorLight");
			break;
		case 8:
			this.lightTexture = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\fishTankLight");
			break;
		case 9:
			this.lightTexture = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\treeLights");
			break;
		case 10:
			this.lightTexture = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\pinpointLight");
			break;
		case 3:
			break;
		}
	}

	/// <summary>Get whether this light source is within the on-screen area.</summary>
	public bool IsOnScreen()
	{
		if (this.onlyLocation.Value != null && this.onlyLocation.Value != Game1.currentLocation?.NameOrUniqueName)
		{
			return false;
		}
		if (!Utility.isOnScreen(this.position.Value, (int)(this.radius.Value * 64f * 4f)))
		{
			return false;
		}
		if (this.PlayerID != 0L && this.PlayerID != Game1.player.UniqueMultiplayerID)
		{
			Farmer farmer = Game1.GetPlayer(this.PlayerID);
			if (farmer == null || farmer.hidden.Value || (farmer.currentLocation != null && farmer.currentLocation.Name != Game1.currentLocation?.Name))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Draw the light source to the screen if needed.</summary>
	/// <param name="spriteBatch">The sprite batch being drawn.</param>
	/// <param name="location">The location containing the light source.</param>
	/// <param name="lightMultiplier">A multiplier to apply to the light strength (e.g. for the darkness debuff).</param>
	public virtual void Draw(SpriteBatch spriteBatch, GameLocation location, float lightMultiplier)
	{
		if (this.fadeOut.Value > 0)
		{
			if (this.color.Value.A <= 0)
			{
				return;
			}
			this.color.Value = new Color(this.color.R, this.color.G, this.color.B, this.color.A - this.fadeOut.Value);
		}
		if (this.lightContext.Value == LightContext.WindowLight && (Game1.IsRainingHere() || Game1.isTimeToTurnOffLighting(location)))
		{
			this.fadeOut.Value = 4;
		}
		if (this.IsOnScreen())
		{
			Texture2D texture = this.lightTexture;
			int lightQuality = Game1.options.lightingQuality;
			spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, this.position.Value) / (lightQuality / 2), texture.Bounds, this.color.Value * lightMultiplier, 0f, new Vector2(texture.Bounds.Width / 2, texture.Bounds.Height / 2), this.radius.Value / (float)(lightQuality / 2), SpriteEffects.None, 0.9f);
		}
	}

	public LightSource Clone()
	{
		LightSource lightSource = new LightSource(this.Id, this.textureIndex.Value, this.position.Value, this.radius.Value, this.color.Value, this.lightContext.Value, this.playerID.Value);
		lightSource.onlyLocation.Value = this.onlyLocation.Value;
		return lightSource;
	}
}
