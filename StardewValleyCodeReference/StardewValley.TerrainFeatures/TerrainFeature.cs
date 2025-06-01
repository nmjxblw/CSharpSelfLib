using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Mods;

namespace StardewValley.TerrainFeatures;

[XmlInclude(typeof(Flooring))]
[XmlInclude(typeof(FruitTree))]
[XmlInclude(typeof(Grass))]
[XmlInclude(typeof(HoeDirt))]
[XmlInclude(typeof(LargeTerrainFeature))]
[XmlInclude(typeof(ResourceClump))]
[XmlInclude(typeof(Tree))]
public abstract class TerrainFeature : INetObject<NetFields>, IHaveModData
{
	[XmlIgnore]
	public readonly bool NeedsTick;

	[XmlIgnore]
	public bool isTemporarilyInvisible;

	[XmlIgnore]
	protected bool _needsUpdate = true;

	/// <summary>The location containing this terrain feature.</summary>
	[XmlIgnore]
	public virtual GameLocation Location { get; set; }

	/// <summary>The top-left tile coordinate containing this terrain feature.</summary>
	[XmlIgnore]
	public virtual Vector2 Tile { get; set; }

	/// <inheritdoc />
	[XmlIgnore]
	public ModDataDictionary modData { get; } = new ModDataDictionary();

	/// <inheritdoc />
	[XmlElement("modData")]
	public ModDataDictionary modDataForSerialization
	{
		get
		{
			return this.modData.GetForSerialization();
		}
		set
		{
			this.modData.SetFromSerialization(value);
		}
	}

	/// <summary>Whether this terrain feature's <see cref="M:StardewValley.TerrainFeatures.TerrainFeature.tickUpdate(Microsoft.Xna.Framework.GameTime)" /> method should be called on each tick.</summary>
	/// <remarks>
	///   <para>This is different from <see cref="F:StardewValley.TerrainFeatures.TerrainFeature.NeedsTick" />, which is implemented as part of <see cref="T:Netcode.INetObject`1" />.</para>
	///
	///   <para>In most cases, this should only be changed by the terrain feature itself, since disabling it may prevent logic like removal on destruction. For example, terrain features can set this to true when they need to be animated (e.g. shaken), and then set to false once the animation has completed.</para>
	/// </remarks>
	[XmlIgnore]
	public bool NeedsUpdate
	{
		get
		{
			return this._needsUpdate;
		}
		set
		{
			if (value != this._needsUpdate)
			{
				this._needsUpdate = value;
				this.Location?.UpdateTerrainFeatureUpdateSubscription(this);
			}
		}
	}

	public NetFields NetFields { get; }

	protected TerrainFeature(bool needsTick)
	{
		this.NetFields = new NetFields(NetFields.GetNameForInstance(this));
		this.NeedsTick = needsTick;
		this.initNetFields();
	}

	public virtual void initNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.modData, "modData");
	}

	public virtual Rectangle getBoundingBox()
	{
		Vector2 tileLocation = this.Tile;
		return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
	}

	public virtual Rectangle getRenderBounds()
	{
		return this.getBoundingBox();
	}

	public virtual void loadSprite()
	{
	}

	/// <summary>Get whether NPCs can walk through or over this terrain feature.</summary>
	/// <param name="c">The NPC to check, or <c>null</c> to get whether all NPCs can walk through it.</param>
	public virtual bool isPassable(Character c = null)
	{
		return this.isTemporarilyInvisible;
	}

	public virtual void OnAddedToLocation(GameLocation location, Vector2 tile)
	{
		this.Location = location;
		this.Tile = tile;
	}

	public virtual void doCollisionAction(Rectangle positionOfCollider, int speedOfCollision, Vector2 tileLocation, Character who)
	{
	}

	public virtual bool performUseAction(Vector2 tileLocation)
	{
		return false;
	}

	public virtual bool performToolAction(Tool t, int damage, Vector2 tileLocation)
	{
		return false;
	}

	public virtual bool tickUpdate(GameTime time)
	{
		return false;
	}

	public virtual void dayUpdate()
	{
	}

	/// <summary>Update the terrain feature when the season changes.</summary>
	/// <param name="onLoad">Whether the season is being initialized as part of loading the save, instead of an actual in-game season change.</param>
	/// <returns>Returns <c>true</c> if the terrain feature should be removed, else <c>false</c>.</returns>
	public virtual bool seasonUpdate(bool onLoad)
	{
		return false;
	}

	public virtual bool isActionable()
	{
		return false;
	}

	public virtual void performPlayerEntryAction()
	{
		this.isTemporarilyInvisible = false;
	}

	public virtual void draw(SpriteBatch spriteBatch)
	{
	}

	public virtual bool forceDraw()
	{
		return false;
	}

	public virtual void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
	{
	}
}
