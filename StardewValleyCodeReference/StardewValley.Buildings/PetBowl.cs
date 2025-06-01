using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.GameData.Buildings;
using StardewValley.Tools;

namespace StardewValley.Buildings;

public class PetBowl : Building
{
	/// <summary>Whether the pet bowl is full.</summary>
	[XmlElement("watered")]
	public readonly NetBool watered = new NetBool();

	private int nameTimer;

	private string nameTimerMessage;

	/// <summary>The pet to which this bowl belongs, if any.</summary>
	/// <remarks>When a pet is assigned, this matches <see cref="F:StardewValley.Characters.Pet.petId" />.</remarks>
	[XmlElement("petGuid")]
	public readonly NetGuid petId = new NetGuid();

	public PetBowl(Vector2 tileLocation)
		: base("Pet Bowl", tileLocation)
	{
	}

	public PetBowl()
		: this(Vector2.Zero)
	{
	}

	/// <summary>Assign a pet to this pet bowl.</summary>
	/// <param name="pet">The pet to assign.</param>
	public virtual void AssignPet(Pet pet)
	{
		this.petId.Value = pet.petId.Value;
		pet.homeLocationName.Value = base.parentLocationName.Value;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.watered, "watered").AddField(this.petId, "petId");
	}

	public virtual Point GetPetSpot()
	{
		return new Point(base.tileX.Value, base.tileY.Value + 1);
	}

	public override bool doAction(Vector2 tileLocation, Farmer who)
	{
		if (!this.isTilePassable(tileLocation))
		{
			_ = this.petId.Value;
			Pet p = Utility.findPet(this.petId.Value);
			if (p != null)
			{
				this.nameTimer = 3500;
				this.nameTimerMessage = Game1.content.LoadString("Strings\\1_6_Strings:PetBowlName", p.displayName);
			}
		}
		return base.doAction(tileLocation, who);
	}

	public override void Update(GameTime time)
	{
		if (this.nameTimer > 0)
		{
			this.nameTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		}
		base.Update(time);
	}

	public override void performToolAction(Tool t, int tileX, int tileY)
	{
		if (t is WateringCan)
		{
			string value = null;
			if (this.doesTileHaveProperty(tileX, tileY, "PetBowl", "Buildings", ref value))
			{
				this.watered.Value = true;
			}
		}
		base.performToolAction(t, tileX, tileY);
	}

	/// <summary>Get whether any pet has been assigned to this pet bowl.</summary>
	public bool HasPet()
	{
		return this.petId.Value != Guid.Empty;
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (base.isMoving || base.isUnderConstruction())
		{
			return;
		}
		if (this.watered.Value)
		{
			BuildingData data = this.GetData();
			float sortY = (base.tileY.Value + base.tilesHigh.Value) * 64;
			if (data != null)
			{
				sortY -= data.SortTileOffset * 64f;
			}
			sortY += 1.5f;
			sortY /= 10000f;
			Vector2 drawPosition = new Vector2(base.tileX.Value * 64, base.tileY.Value * 64 + base.tilesHigh.Value * 64);
			Vector2 drawOffset = Vector2.Zero;
			if (data != null)
			{
				drawOffset = data.DrawOffset * 4f;
			}
			Rectangle sourceRect = this.getSourceRect();
			sourceRect.X += sourceRect.Width;
			b.Draw(origin: new Vector2(0f, sourceRect.Height), texture: base.texture.Value, position: Game1.GlobalToLocal(Game1.viewport, drawPosition + drawOffset), sourceRectangle: sourceRect, color: base.color * base.alpha, rotation: 0f, scale: 4f, effects: SpriteEffects.None, layerDepth: sortY);
		}
		if (this.nameTimer > 0)
		{
			BuildingData data2 = this.GetData();
			float sortY2 = (base.tileY.Value + base.tilesHigh.Value) * 64;
			if (data2 != null)
			{
				sortY2 -= data2.SortTileOffset * 64f;
			}
			sortY2 += 1.5f;
			sortY2 /= 10000f;
			SpriteText.drawSmallTextBubble(b, this.nameTimerMessage, Game1.GlobalToLocal(new Vector2(((float)base.tileX.Value + 1.5f) * 64f, base.tileY.Value * 64 - 32)), -1, sortY2 + 1E-06f);
		}
	}
}
