using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley.BellsAndWhistles;

public class PlayerStatusList : INetObject<NetFields>
{
	public enum SortMode
	{
		None,
		NumberSort,
		NumberSortDescending,
		AlphaSort,
		AlphaSortDescending
	}

	public enum DisplayMode
	{
		Text,
		LocalizedText,
		Icons
	}

	public enum VerticalAlignment
	{
		Top,
		Bottom
	}

	public enum HorizontalAlignment
	{
		Left,
		Right
	}

	protected readonly NetLongDictionary<string, NetString> _statusList = new NetLongDictionary<string, NetString>
	{
		InterpolationWait = false
	};

	protected readonly Dictionary<long, string> _formattedStatusList = new Dictionary<long, string>();

	protected readonly Dictionary<string, Texture2D> _iconSprites = new Dictionary<string, Texture2D>();

	protected readonly List<Farmer> _sortedFarmers = new List<Farmer>();

	public int iconAnimationFrames = 1;

	public int largestSpriteWidth;

	public int largestSpriteHeight;

	public SortMode sortMode;

	public DisplayMode displayMode;

	protected Dictionary<string, KeyValuePair<string, Rectangle>> _iconDefinitions = new Dictionary<string, KeyValuePair<string, Rectangle>>();

	public NetFields NetFields { get; } = new NetFields("PlayerStatusList");

	public PlayerStatusList()
	{
		this.InitNetFields();
	}

	public void InitNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this._statusList, "_statusList");
		this._statusList.OnValueRemoved += delegate
		{
			this._OnValueChanged();
		};
		this._statusList.OnValueAdded += delegate
		{
			this._OnValueChanged();
		};
		this._statusList.OnConflictResolve += delegate
		{
			this._OnValueChanged();
		};
		this._statusList.OnValueTargetUpdated += delegate(long key, string value, string targetValue)
		{
			if (this._statusList.FieldDict.TryGetValue(key, out var value2))
			{
				value2.CancelInterpolation();
			}
			this._OnValueChanged();
		};
	}

	public void AddSpriteDefinition(string key, string file, int x, int y, int width, int height)
	{
		if (!this._iconSprites.TryGetValue(file, out var iconSprite) || iconSprite.IsDisposed)
		{
			this._iconSprites[file] = Game1.content.Load<Texture2D>(file);
		}
		this._iconDefinitions[key] = new KeyValuePair<string, Rectangle>(file, new Rectangle(x, y, width, height));
		if (width > this.largestSpriteWidth)
		{
			this.largestSpriteWidth = width;
		}
		if (height > this.largestSpriteHeight)
		{
			this.largestSpriteHeight = height;
		}
	}

	public void UpdateState(string newState)
	{
		if (!this._statusList.TryGetValue(Game1.player.UniqueMultiplayerID, out var oldState) || oldState != newState)
		{
			this._statusList[Game1.player.UniqueMultiplayerID] = newState;
		}
	}

	public void WithdrawState()
	{
		this._statusList.Remove(Game1.player.UniqueMultiplayerID);
	}

	protected void _OnValueChanged()
	{
		foreach (long id in this._statusList.Keys)
		{
			this._formattedStatusList[id] = this.GetStatusText(id);
		}
		this._ResortList();
	}

	protected void _ResortList()
	{
		this._sortedFarmers.Clear();
		foreach (Farmer farmer in Game1.getOnlineFarmers())
		{
			this._sortedFarmers.Add(farmer);
		}
		foreach (Farmer farmer2 in Game1.getAllFarmers())
		{
			if (Game1.IsMasterGame && !this._sortedFarmers.Contains(farmer2))
			{
				this._statusList.Remove(farmer2.UniqueMultiplayerID);
			}
			if (!this._statusList.ContainsKey(farmer2.UniqueMultiplayerID))
			{
				this._sortedFarmers.Remove(farmer2);
			}
		}
		switch (this.sortMode)
		{
		case SortMode.AlphaSort:
		case SortMode.AlphaSortDescending:
			this._sortedFarmers.Sort((Farmer a, Farmer b) => this.GetStatusText(a.UniqueMultiplayerID).CompareTo(this.GetStatusText(b.UniqueMultiplayerID)));
			if (this.sortMode == SortMode.AlphaSortDescending)
			{
				this._sortedFarmers.Reverse();
			}
			break;
		case SortMode.NumberSort:
		case SortMode.NumberSortDescending:
			this._sortedFarmers.Sort((Farmer a, Farmer b) => this.GetStatusInt(a.UniqueMultiplayerID).CompareTo(this.GetStatusInt(b.UniqueMultiplayerID)));
			if (this.sortMode == SortMode.NumberSortDescending)
			{
				this._sortedFarmers.Reverse();
			}
			break;
		}
	}

	/// <summary>Try to get the status text for a player.</summary>
	/// <param name="id">The unique multiplayer ID for the player whose status to get.</param>
	/// <param name="statusText">The status text if found, else <c>null</c>.</param>
	/// <returns>Whether the status was found.</returns>
	public bool TryGetStatusText(long id, out string statusText)
	{
		if (this._statusList.TryGetValue(id, out statusText))
		{
			if (this.displayMode == DisplayMode.LocalizedText)
			{
				statusText = Game1.content.LoadString(statusText);
			}
			return true;
		}
		statusText = null;
		return false;
	}

	/// <summary>Get the string representation of a player's status.</summary>
	/// <param name="id">The unique multiplayer ID for the player whose status to get.</param>
	/// <param name="fallback">The value to return if no status is found for the player.</param>
	/// <returns>The string representation of the player's status, or <paramref name="fallback" /> if not found.</returns>
	public string GetStatusText(long id, string fallback = "")
	{
		if (!this.TryGetStatusText(id, out var statusText))
		{
			return fallback;
		}
		return statusText;
	}

	/// <summary>Get the integer representation of a player's status (e.g. number of eggs found at the Egg Festival).</summary>
	/// <param name="id">The unique multiplayer ID for the player whose status to get.</param>
	/// <param name="fallback">The value to return if no status is found for the player.</param>
	/// <returns>The integer representation of the player's status, or <paramref name="fallback" /> if not found.</returns>
	public int GetStatusInt(long id, int fallback = 0)
	{
		if (!this.TryGetStatusText(id, out var statusText) || !int.TryParse(statusText, out var status))
		{
			return fallback;
		}
		return status;
	}

	public void Draw(SpriteBatch b, Vector2 draw_position, float draw_scale = 4f, float draw_layer = 0.45f, HorizontalAlignment horizontal_origin = HorizontalAlignment.Left, VerticalAlignment vertical_origin = VerticalAlignment.Top)
	{
		float y_offset_per_entry = 12f;
		if (this.displayMode == DisplayMode.Icons && (float)this.largestSpriteHeight > y_offset_per_entry)
		{
			y_offset_per_entry = this.largestSpriteHeight;
		}
		if (horizontal_origin == HorizontalAlignment.Right)
		{
			float longest_string = 0f;
			if (this.displayMode == DisplayMode.Icons)
			{
				draw_position.X -= (float)this.largestSpriteWidth * draw_scale;
			}
			else
			{
				foreach (Farmer farmer in this._sortedFarmers)
				{
					if (!farmer.IsDedicatedPlayer && this._formattedStatusList.TryGetValue(farmer.UniqueMultiplayerID, out var state))
					{
						float string_length = Game1.dialogueFont.MeasureString(state).X;
						if (longest_string < string_length)
						{
							longest_string = string_length;
						}
					}
				}
				draw_position.X -= (longest_string + 16f) * draw_scale;
			}
		}
		if (vertical_origin == VerticalAlignment.Bottom)
		{
			draw_position.Y -= y_offset_per_entry * (float)this._statusList.Length * draw_scale;
		}
		foreach (Farmer farmer2 in this._sortedFarmers)
		{
			if (farmer2.IsDedicatedPlayer)
			{
				continue;
			}
			float sort_direction = ((!Game1.isUsingBackToFrontSorting) ? 1 : (-1));
			if (this._formattedStatusList.TryGetValue(farmer2.UniqueMultiplayerID, out var state2))
			{
				Vector2 draw_offset = Vector2.Zero;
				farmer2.FarmerRenderer.drawMiniPortrat(b, draw_position, draw_layer, draw_scale * 0.75f, 2, farmer2);
				if (this.displayMode == DisplayMode.Icons && this._iconDefinitions.TryGetValue(state2, out var spriteDefinition))
				{
					draw_offset.X += 12f * draw_scale;
					Rectangle currentSrcRect = spriteDefinition.Value;
					currentSrcRect.Y = (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % (double)(this.iconAnimationFrames * 100) / 100.0) * 16;
					b.Draw(this._iconSprites[spriteDefinition.Key], draw_position + draw_offset, currentSrcRect, Color.White, 0f, Vector2.Zero, draw_scale, SpriteEffects.None, draw_layer - 0.0001f * sort_direction);
				}
				else
				{
					draw_offset.X += 16f * draw_scale;
					draw_offset.Y += 2f * draw_scale;
					string drawn_string = state2;
					b.DrawString(Game1.dialogueFont, drawn_string, draw_position + draw_offset + Vector2.One * draw_scale, Color.Black, 0f, Vector2.Zero, draw_scale / 4f, SpriteEffects.None, draw_layer - 0.0001f * sort_direction);
					b.DrawString(Game1.dialogueFont, drawn_string, draw_position + draw_offset, Color.White, 0f, Vector2.Zero, draw_scale / 4f, SpriteEffects.None, draw_layer);
				}
				draw_position.Y += y_offset_per_entry * draw_scale;
			}
		}
	}
}
