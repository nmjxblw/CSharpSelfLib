using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;

namespace Pathoschild.Stardew.LookupAnything.Components;

internal class DebugInterface
{
	private readonly GameHelper GameHelper;

	private readonly TargetFactory TargetFactory;

	private readonly IMonitor Monitor;

	private readonly Func<ModConfig> Config;

	public bool Enabled { get; set; }

	public DebugInterface(GameHelper gameHelper, TargetFactory targetFactory, Func<ModConfig> config, IMonitor monitor)
	{
		this.GameHelper = gameHelper;
		this.TargetFactory = targetFactory;
		this.Config = config;
		this.Monitor = monitor;
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		if (!this.Enabled)
		{
			return;
		}
		this.Monitor.InterceptErrors("drawing debug info", delegate
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0222: Unknown result type (might be due to invalid IL or missing references)
			//IL_022a: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0245: Unknown result type (might be due to invalid IL or missing references)
			//IL_024a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0263: Unknown result type (might be due to invalid IL or missing references)
			//IL_0268: Unknown result type (might be due to invalid IL or missing references)
			//IL_0270: Unknown result type (might be due to invalid IL or missing references)
			//IL_0278: Unknown result type (might be due to invalid IL or missing references)
			//IL_0280: Unknown result type (might be due to invalid IL or missing references)
			//IL_028b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0290: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02db: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_030e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0316: Unknown result type (might be due to invalid IL or missing references)
			//IL_031d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0326: Unknown result type (might be due to invalid IL or missing references)
			//IL_0331: Unknown result type (might be due to invalid IL or missing references)
			//IL_0336: Unknown result type (might be due to invalid IL or missing references)
			//IL_0253: Unknown result type (might be due to invalid IL or missing references)
			//IL_025a: Unknown result type (might be due to invalid IL or missing references)
			//IL_025f: Unknown result type (might be due to invalid IL or missing references)
			ModConfig modConfig = this.Config();
			GameLocation currentLocation = Game1.currentLocation;
			Vector2 currentCursorTile = Game1.currentCursorTile;
			Vector2 screenCoordinatesFromCursor = this.GameHelper.GetScreenCoordinatesFromCursor();
			string label = $"Debug info enabled; press {string.Join(" or ", modConfig.Controls.ToggleDebug)} to disable. Cursor tile ({currentCursorTile.X}, {currentCursorTile.Y}), position ({screenCoordinatesFromCursor.X}, {screenCoordinatesFromCursor.Y}).";
			this.GameHelper.DrawHoverBox(spriteBatch, label, Vector2.Zero, ((Rectangle)(ref Game1.uiViewport)).Width);
			DrawHelper.DrawLine(spriteBatch, screenCoordinatesFromCursor.X - 1f, screenCoordinatesFromCursor.Y - 1f, new Vector2(4f, 4f), Color.DarkRed);
			Rectangle screenCoordinatesFromTile = this.GameHelper.GetScreenCoordinatesFromTile(Game1.currentCursorTile);
			IEnumerable<ITarget> enumerable = from p in this.TargetFactory.GetNearbyTargets(currentLocation, currentCursorTile)
				where p.Type != SubjectType.Tile
				select p;
			foreach (ITarget current in enumerable)
			{
				Rectangle worldArea = current.GetWorldArea();
				bool flag = ((Rectangle)(ref worldArea)).Intersects(screenCoordinatesFromTile);
				ISubject subject = current.GetSubject();
				Rectangle screenCoordinatesFromTile2 = this.GameHelper.GetScreenCoordinatesFromTile(current.Tile);
				Color value = ((subject != null) ? Color.Green : Color.Red) * 0.5f;
				DrawHelper.DrawLine(spriteBatch, screenCoordinatesFromTile2.X, screenCoordinatesFromTile2.Y, new Vector2((float)screenCoordinatesFromTile2.Width, (float)screenCoordinatesFromTile2.Height), value);
				if (subject != null)
				{
					int num = 3;
					Color val = Color.Green;
					if (!flag)
					{
						num = 1;
						val *= 0.5f;
					}
					Rectangle worldArea2 = current.GetWorldArea();
					DrawHelper.DrawLine(spriteBatch, worldArea2.X, worldArea2.Y, new Vector2((float)worldArea2.Width, (float)num), val);
					DrawHelper.DrawLine(spriteBatch, worldArea2.X, worldArea2.Y, new Vector2((float)num, (float)worldArea2.Height), val);
					DrawHelper.DrawLine(spriteBatch, worldArea2.X + worldArea2.Width, worldArea2.Y, new Vector2((float)num, (float)worldArea2.Height), val);
					DrawHelper.DrawLine(spriteBatch, worldArea2.X, worldArea2.Y + worldArea2.Height, new Vector2((float)worldArea2.Width, (float)num), val);
				}
			}
			ISubject subjectFrom = this.TargetFactory.GetSubjectFrom(Game1.player, currentLocation, Game1.wasMouseVisibleThisFrame);
			if (subjectFrom != null)
			{
				this.GameHelper.DrawHoverBox(spriteBatch, subjectFrom.Name, new Vector2((float)Game1.getMouseX(), (float)Game1.getMouseY()) + new Vector2(32f), (float)((Rectangle)(ref Game1.uiViewport)).Width / 4f);
			}
		}, OnDrawError);
	}

	private void OnDrawError(Exception ex)
	{
		this.Monitor.InterceptErrors("handling an error in the debug code", delegate
		{
			this.Enabled = false;
		});
	}
}
