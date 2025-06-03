using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewValley;
using StardewValley.GameData.Locations;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles;

internal class TileSubject : BaseSubject
{
	protected readonly ModConfig Config;

	protected readonly GameLocation Location;

	protected readonly Vector2 Position;

	protected readonly bool ShowRawTileInfo;

	public TileSubject(GameHelper gameHelper, ModConfig config, GameLocation location, Vector2 position, bool showRawTileInfo)
		: base(gameHelper, I18n.Tile_Title(position.X, position.Y), showRawTileInfo ? I18n.Tile_Description() : null, null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		this.Config = config;
		this.Location = location;
		this.Position = position;
		this.ShowRawTileInfo = showRawTileInfo;
	}

	public static bool TryCreate(GameHelper gameHelper, ModConfig config, GameLocation location, Vector2 position, bool showRawTileInfo, [NotNullWhen(true)] out TileSubject? tileSubject)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		tileSubject = new TileSubject(gameHelper, config, location, position, showRawTileInfo);
		if (tileSubject.GetData().Any())
		{
			return true;
		}
		tileSubject = null;
		return false;
	}

	public override IEnumerable<ICustomField> GetData()
	{
		string key;
		PropertyValue value;
		StringBuilder summary;
		if (this.ShowRawTileInfo)
		{
			yield return new GenericField(I18n.Tile_MapName(), this.Location.Name);
			if (((ICollection<KeyValuePair<string, PropertyValue>>)((Component)this.Location.Map).Properties).Count > 0)
			{
				summary = new StringBuilder();
				foreach (KeyValuePair<string, PropertyValue> item in (IEnumerable<KeyValuePair<string, PropertyValue>>)((Component)this.Location.Map).Properties)
				{
					item.Deconstruct(out key, out value);
					string text = PropertyValue.op_Implicit(value);
					string key2 = key;
					string value2 = text;
					summary.AppendLine(I18n.Tile_MapProperties_Value(key2, value2));
				}
				GenericField field = new GenericField(I18n.Tile_MapProperties(), summary.ToString());
				if (summary.Length > 50)
				{
					field.CollapseByDefault(I18n.Generic_ShowXResults(((ICollection<KeyValuePair<string, PropertyValue>>)((Component)this.Location.Map).Properties).Count));
				}
				yield return field;
				summary.Clear();
			}
		}
		if (TileSubject.IsFishingArea(this.Location, this.Position))
		{
			string fishAreaId = default(string);
			FishAreaData val = default(FishAreaData);
			this.Location.TryGetFishAreaForTile(this.Position, ref fishAreaId, ref val);
			FishSpawnRulesField field2 = new FishSpawnRulesField(base.GameHelper, I18n.Item_FishSpawnRules(), this.Location, this.Position, fishAreaId, this.Config.ShowUncaughtFishSpawnRules);
			if (field2.HasValue)
			{
				yield return field2;
			}
		}
		if (!this.ShowRawTileInfo)
		{
			yield break;
		}
		Tile[] tiles = this.GetTiles(this.Location, this.Position).ToArray();
		if (!tiles.Any())
		{
			yield return new GenericField(I18n.Tile_LayerTileNone(), I18n.Tile_LayerTile_NoneHere());
			yield break;
		}
		summary = new StringBuilder();
		Tile[] array = tiles;
		foreach (Tile tile in array)
		{
			summary.AppendLine(I18n.Tile_LayerTile_Appearance(base.Stringify(tile.TileIndex), ((Component)tile.TileSheet).Id, tile.TileSheet.ImageSource.Replace("\\", ": ").Replace("/", ": ")));
			summary.AppendLine();
			if ((int)tile.BlendMode != 0)
			{
				summary.AppendLine(I18n.Tile_LayerTile_BlendMode(base.Stringify(tile.BlendMode)));
			}
			foreach (KeyValuePair<string, PropertyValue> item2 in (IEnumerable<KeyValuePair<string, PropertyValue>>)((Component)tile).Properties)
			{
				item2.Deconstruct(out key, out value);
				string text2 = PropertyValue.op_Implicit(value);
				string name = key;
				string value3 = text2;
				summary.AppendLine(I18n.Tile_LayerTile_TileProperty(name, value3));
			}
			foreach (KeyValuePair<string, PropertyValue> item3 in (IEnumerable<KeyValuePair<string, PropertyValue>>)tile.TileIndexProperties)
			{
				item3.Deconstruct(out key, out value);
				string text3 = PropertyValue.op_Implicit(value);
				string name2 = key;
				string value4 = text3;
				summary.AppendLine(I18n.Tile_LayerTile_IndexProperty(name2, value4));
			}
			yield return new GenericField(I18n.Tile_LayerTile(((Component)tile.Layer).Id), summary.ToString().TrimEnd());
			summary.Clear();
		}
	}

	public override IEnumerable<IDebugField> GetDebugFields()
	{
		string mapTileLabel = "map tile";
		string locationLabel = I18n.Tile_GameLocation();
		Tile[] tiles = this.GetTiles(this.Location, this.Position).ToArray();
		Tile[] array = tiles;
		foreach (Tile tile in array)
		{
			foreach (IDebugField field in base.GetDebugFieldsFrom(tile))
			{
				yield return new GenericDebugField(((Component)tile.Layer).Id + "::" + field.Label, field.Value, field.HasValue)
				{
					OverrideCategory = mapTileLabel
				};
			}
		}
		foreach (IDebugField field2 in base.GetDebugFieldsFrom(this.Location))
		{
			yield return new GenericDebugField(field2.Label, field2.Value, field2.HasValue, field2.IsPinned)
			{
				OverrideCategory = locationLabel
			};
		}
	}

	public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
	{
		return false;
	}

	public static bool IsFishingArea(GameLocation location, Vector2 tile)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return location.isTileFishable((int)tile.X, (int)tile.Y);
	}

	private IEnumerable<Tile> GetTiles(GameLocation location, Vector2 position)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (position.X < 0f || position.Y < 0f)
		{
			yield break;
		}
		foreach (Layer layer in location.map.Layers)
		{
			if (!(position.X > (float)layer.LayerWidth) && !(position.Y > (float)layer.LayerHeight))
			{
				Tile tile = layer.Tiles[(int)position.X, (int)position.Y];
				if (tile != null)
				{
					yield return tile;
				}
			}
		}
	}
}
