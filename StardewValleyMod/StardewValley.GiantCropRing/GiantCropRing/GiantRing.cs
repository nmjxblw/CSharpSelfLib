using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.Objects;

namespace GiantCropRing;

public class GiantRing : Ring, ISaveElement
{
	public static Texture2D texture;

	public static int price;

	public override string DisplayName
	{
		get
		{
			return ((Item)this).Name;
		}
		set
		{
			((Item)this).Name = value;
		}
	}

	public GiantRing()
	{
		Build(getAdditionalSaveData());
	}

	public GiantRing(int id)
	{
		Build(new Dictionary<string, string>
		{
			{ "name", "Giant Crop Ring" },
			{
				"id",
				$"{id}"
			}
		});
	}

	public object getReplacement()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return (object)new Ring(517);
	}

	public Dictionary<string, string> getAdditionalSaveData()
	{
		int id = ((((NetFieldBase<int, NetInt>)(object)base.uniqueID).Value == 0) ? Guid.NewGuid().GetHashCode() : ((NetFieldBase<int, NetInt>)(object)base.uniqueID).Value);
		return new Dictionary<string, string>
		{
			{
				"name",
				((Item)this).Name
			},
			{
				"id",
				$"{id}"
			}
		};
	}

	public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
	{
		Build(additionalSaveData);
	}

	private void Build(IReadOnlyDictionary<string, string> additionalSaveData)
	{
		((Item)this).Category = -96;
		((Item)this).Name = "Giant Crop Ring";
		base.description = "Increases the chance of growing giant crops if you wear it before going to sleep.";
		((NetFieldBase<int, NetInt>)(object)base.uniqueID).Value = int.Parse(additionalSaveData["id"]);
		((Item)this).ParentSheetIndex = ((NetFieldBase<int, NetInt>)(object)base.uniqueID).Value;
		((NetFieldBase<int, NetInt>)(object)base.indexInTileSheet).Value = ((NetFieldBase<int, NetInt>)(object)base.uniqueID).Value;
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		spriteBatch.Draw(texture, location + new Vector2(32f, 32f) * scaleSize, (Rectangle?)Game1.getSourceRectForStandardTileSheet(texture, 0, 16, 16), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, scaleSize * 4f, (SpriteEffects)0, layerDepth);
	}

	public override Item getOne()
	{
		return (Item)(object)new GiantRing(((NetFieldBase<int, NetInt>)(object)base.uniqueID).Value);
	}
}
