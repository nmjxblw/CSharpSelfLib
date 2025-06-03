using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus;

public class SaveGameMenu : IClickableMenu, IDisposable
{
	private IEnumerator<int> loader;

	private int completePause = -1;

	public bool quit;

	public bool hasDrawn;

	private SparklingText saveText;

	private int margin = 500;

	private StringBuilder _stringBuilder = new StringBuilder();

	private float _ellipsisDelay = 0.5f;

	private int _ellipsisCount;

	protected bool _hasSentFarmhandData;

	public SaveGameMenu()
	{
		this.saveText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGameMenu.cs.11378"), Color.LimeGreen, Color.Black * 0.001f, rainbow: false, 0.1, 1500, 32);
		this._hasSentFarmhandData = false;
	}

	public void complete()
	{
		Game1.playSound("money");
		this.completePause = 1500;
		this.loader = null;
		Game1.game1.IsSaving = false;
		if (Game1.IsMasterGame && Game1.newDaySync.hasInstance() && !Game1.newDaySync.hasSaved())
		{
			Game1.newDaySync.flagSaved();
		}
	}

	public override bool readyToClose()
	{
		return false;
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		if (this.quit)
		{
			if (Game1.activeClickableMenu.Equals(this) && Game1.PollForEndOfNewDaySync())
			{
				Game1.exitActiveMenu();
			}
			return;
		}
		base.update(time);
		if (Game1.client != null && Game1.client.timedOut)
		{
			this.quit = true;
			if (Game1.activeClickableMenu.Equals(this))
			{
				Game1.exitActiveMenu();
			}
			return;
		}
		this._ellipsisDelay -= (float)time.ElapsedGameTime.TotalSeconds;
		if (this._ellipsisDelay <= 0f)
		{
			this._ellipsisDelay += 0.75f;
			this._ellipsisCount++;
			if (this._ellipsisCount > 3)
			{
				this._ellipsisCount = 1;
			}
		}
		if (this.loader != null)
		{
			this.loader.MoveNext();
			if (this.loader.Current >= 100)
			{
				this.margin -= time.ElapsedGameTime.Milliseconds;
				if (this.margin <= 0)
				{
					this.complete();
				}
			}
		}
		else if (this.hasDrawn && this.completePause == -1)
		{
			if (Game1.IsMasterGame)
			{
				if (Game1.saveOnNewDay)
				{
					Game1.player.team.endOfNightStatus.UpdateState("ready");
					if (Game1.newDaySync.readyForSave())
					{
						Game1.multiplayer.saveFarmhands();
						Game1.game1.IsSaving = true;
						this.loader = SaveGame.Save();
					}
				}
				else
				{
					this.margin = -1;
					if (Game1.newDaySync.readyForSave())
					{
						Game1.game1.IsSaving = true;
						this.complete();
					}
				}
			}
			else
			{
				if (LocalMultiplayer.IsLocalMultiplayer())
				{
					LocalMultiplayer.SaveOptions();
				}
				if (!this._hasSentFarmhandData)
				{
					this._hasSentFarmhandData = true;
					Game1.multiplayer.sendFarmhand();
				}
				Game1.multiplayer.UpdateLate();
				Program.sdk.Update();
				Game1.multiplayer.UpdateEarly();
				Game1.newDaySync.readyForSave();
				Game1.player.team.endOfNightStatus.UpdateState("ready");
				if (Game1.newDaySync.hasSaved())
				{
					SaveGameMenu.saveClientOptions();
					this.complete();
				}
			}
		}
		if (this.completePause >= 0)
		{
			this.completePause -= time.ElapsedGameTime.Milliseconds;
			this.saveText.update(time);
			if (this.completePause < 0)
			{
				this.quit = true;
				this.completePause = -9999;
			}
		}
	}

	private static void saveClientOptions()
	{
		StartupPreferences startupPreferences = new StartupPreferences();
		startupPreferences.loadPreferences(async: false, applyLanguage: false);
		startupPreferences.clientOptions = Game1.options;
		startupPreferences.savePreferences(async: false);
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		Vector2 txtpos = new Vector2(64f, Game1.uiViewport.Height - 64);
		Vector2 txtsize = new Vector2(64f, 64f);
		txtpos = Utility.makeSafe(txtpos, txtsize);
		bool draw_ready_status = false;
		if (this.completePause >= 0)
		{
			if (Game1.saveOnNewDay)
			{
				this.saveText.draw(b, txtpos);
			}
		}
		else if (this.margin < 0 || Game1.IsClient)
		{
			if (Game1.IsMultiplayer)
			{
				int totalPlayers = Game1.getOnlineFarmers().Count - (Game1.HasDedicatedHost ? 1 : 0);
				this._stringBuilder.Clear();
				this._stringBuilder.Append(Game1.content.LoadString("Strings\\UI:ReadyCheck", Game1.newDaySync.numReadyForSave(), totalPlayers));
				for (int i = 0; i < this._ellipsisCount; i++)
				{
					this._stringBuilder.Append(".");
				}
				b.DrawString(Game1.dialogueFont, this._stringBuilder, txtpos, Color.White);
				draw_ready_status = true;
			}
		}
		else if (!Game1.IsMultiplayer)
		{
			this._stringBuilder.Clear();
			this._stringBuilder.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGameMenu.cs.11381"));
			for (int j = 0; j < this._ellipsisCount; j++)
			{
				this._stringBuilder.Append(".");
			}
			b.DrawString(Game1.dialogueFont, this._stringBuilder, txtpos, Color.White);
		}
		else
		{
			this._stringBuilder.Clear();
			this._stringBuilder.Append(Game1.content.LoadString("Strings\\UI:ReadyCheck", Game1.newDaySync.numReadyForSave(), Game1.getOnlineFarmers().Count));
			for (int k = 0; k < this._ellipsisCount; k++)
			{
				this._stringBuilder.Append(".");
			}
			b.DrawString(Game1.dialogueFont, this._stringBuilder, txtpos, Color.White);
			draw_ready_status = true;
		}
		if (this.completePause > 0)
		{
			draw_ready_status = false;
		}
		if (Game1.newDaySync.hasInstance() && Game1.newDaySync.hasSaved())
		{
			draw_ready_status = false;
		}
		if (Game1.IsMultiplayer && draw_ready_status && Game1.options.showMPEndOfNightReadyStatus)
		{
			Game1.player.team.endOfNightStatus.Draw(b, txtpos + new Vector2(0f, -32f), 4f, 0.99f, PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment.Bottom);
		}
		this.hasDrawn = true;
	}

	public void Dispose()
	{
		Game1.game1.IsSaving = false;
	}
}
