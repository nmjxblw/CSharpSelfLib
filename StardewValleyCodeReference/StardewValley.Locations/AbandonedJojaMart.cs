using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Menus;
using StardewValley.Network;

namespace StardewValley.Locations;

public class AbandonedJojaMart : GameLocation
{
	[XmlIgnore]
	private readonly NetEvent0 restoreAreaCutsceneEvent = new NetEvent0();

	[XmlIgnore]
	public NetMutex bundleMutex = new NetMutex();

	public AbandonedJojaMart()
	{
	}

	public AbandonedJojaMart(string mapPath, string name)
		: base(mapPath, name)
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.restoreAreaCutsceneEvent, "restoreAreaCutsceneEvent").AddField(this.bundleMutex.NetFields, "bundleMutex.NetFields");
		this.restoreAreaCutsceneEvent.onEvent += doRestoreAreaCutscene;
	}

	public void checkBundle()
	{
		this.bundleMutex.RequestLock(delegate
		{
			Dictionary<int, bool[]> bundlesComplete = Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundlesDict();
			Game1.activeClickableMenu = new JunimoNoteMenu(6, bundlesComplete);
		});
	}

	public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
	{
		base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
		this.bundleMutex.Update(this);
		if (this.bundleMutex.IsLockHeld() && Game1.activeClickableMenu == null)
		{
			this.bundleMutex.ReleaseLock();
		}
		this.restoreAreaCutsceneEvent.Poll();
	}

	public void restoreAreaCutscene()
	{
		this.restoreAreaCutsceneEvent.Fire();
	}

	private void doRestoreAreaCutscene()
	{
		if (Game1.currentLocation == this)
		{
			Game1.player.freezePause = 1000;
			DelayedAction.removeTileAfterDelay(8, 8, 100, Game1.currentLocation, "Buildings");
			Game1.RequireLocation("AbandonedJojaMart").startEvent(new Event(Game1.content.Load<Dictionary<string, string>>("Data\\Events\\AbandonedJojaMart")["missingBundleComplete"], "Data\\Events\\AbandonedJojaMart", "192393"));
		}
		Game1.addMailForTomorrow("ccMovieTheater", noLetter: true, sendToEveryone: true);
		if (Game1.player.team.theaterBuildDate.Value < 0)
		{
			Game1.player.team.theaterBuildDate.Set(Game1.Date.TotalDays + 1);
		}
	}

	protected override void resetSharedState()
	{
		if (Game1.netWorldState.Value.Bundles.TryGetValue(36, out var bundles) && !this.bundleMutex.IsLocked() && !Game1.eventUp && !Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
		{
			bool[] array = bundles;
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i])
				{
					return;
				}
			}
			this.restoreAreaCutscene();
		}
		base.resetSharedState();
	}
}
