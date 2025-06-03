using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValley.Network.Dedicated;

public class DedicatedServer
{
	public class FarmerWarp
	{
		public Farmer who;

		public string name;

		public int facingDirection;

		public short x;

		public short y;

		public bool isStructure;

		public bool warpingForForcedRemoteEvent;

		public FarmerWarp(Farmer who, short x, short y, string name, bool isStructure, int facingDirection, bool warpingForForcedRemoteEvent)
		{
			this.who = who;
			this.name = name;
			this.facingDirection = facingDirection;
			this.x = x;
			this.y = y;
			this.isStructure = isStructure;
			this.warpingForForcedRemoteEvent = warpingForForcedRemoteEvent;
		}
	}

	private const string BROADCAST_EVENT_KEY = "BroadcastEvent";

	private readonly ConcurrentQueue<FarmerWarp> farmerWarps = new ConcurrentQueue<FarmerWarp>();

	private readonly Dictionary<string, Dictionary<string, long>> eventLocks = new Dictionary<string, Dictionary<string, long>>();

	private readonly HashSet<long> onlineIds = new HashSet<long>();

	private readonly HashSet<string> broadcastEvents = new HashSet<string>();

	private readonly HashSet<string> notBroadcastEvents = new HashSet<string>();

	private bool fakeWarp;

	private bool warpingSleep;

	private bool warpingFestival;

	private bool warpingHostBroadcastEvent;

	private bool startedFestivalMainEvent;

	private bool startedFestivalEnd;

	private bool shouldJudgeGrange;

	public bool CheckedHostPrecondition;

	private long fakeFarmerId;

	public bool FakeWarp
	{
		get
		{
			if (Game1.IsDedicatedHost)
			{
				return this.fakeWarp;
			}
			return false;
		}
	}

	public Farmer FakeFarmer
	{
		get
		{
			if (!Game1.IsDedicatedHost)
			{
				return Game1.player;
			}
			Farmer farmer = Game1.getFarmer(this.fakeFarmerId);
			if (!Game1.multiplayer.isDisconnecting(farmer))
			{
				return farmer;
			}
			return Game1.player;
		}
	}

	public DedicatedServer()
	{
		this.Reset();
	}

	public void Reset()
	{
		this.fakeWarp = false;
		this.warpingSleep = false;
		this.warpingFestival = false;
		this.startedFestivalMainEvent = false;
		this.startedFestivalEnd = false;
		this.shouldJudgeGrange = false;
		this.warpingHostBroadcastEvent = false;
		this.broadcastEvents.Clear();
		this.eventLocks.Clear();
	}

	public void ResetForNewDay()
	{
		if (Game1.IsDedicatedHost)
		{
			this.fakeWarp = false;
			this.warpingSleep = false;
			this.warpingFestival = false;
			this.startedFestivalMainEvent = false;
			this.startedFestivalEnd = false;
			this.shouldJudgeGrange = false;
			this.warpingHostBroadcastEvent = false;
			this.eventLocks.Clear();
		}
	}

	private bool TryForceClientHostEvent(FarmerWarp warp, GameLocation location, string eventId)
	{
		if (Game1.server == null)
		{
			return false;
		}
		string key = (warp.isStructure ? "1" : "0") + location.NameOrUniqueName;
		if (!this.eventLocks.TryGetValue(key, out var locationEvents))
		{
			this.eventLocks[key] = new Dictionary<string, long>();
		}
		else if (locationEvents.ContainsKey(eventId))
		{
			return false;
		}
		this.eventLocks[key][eventId] = warp.who.UniqueMultiplayerID;
		object[] message = Game1.multiplayer.generateForceEventMessage(eventId, location, warp.x, warp.y, use_local_farmer: true, notify_when_done: true);
		Game1.server.sendMessage(warp.who.UniqueMultiplayerID, 4, Game1.player, message);
		return true;
	}

	private void CheckForWarpEvents(FarmerWarp warp)
	{
		if (warp.warpingForForcedRemoteEvent || Game1.eventUp || Game1.farmEvent != null || this.IsWarping())
		{
			return;
		}
		GameLocation location = Game1.getLocationFromName(warp.name, warp.isStructure);
		Dictionary<string, string> events;
		try
		{
			if (!location.TryGetLocationEvents(out var _, out events) || events == null)
			{
				return;
			}
		}
		catch
		{
			return;
		}
		int xLocationAfterWarp = Game1.xLocationAfterWarp;
		int yLocationAfterWarp = Game1.yLocationAfterWarp;
		Game1.xLocationAfterWarp = warp.x;
		Game1.yLocationAfterWarp = warp.y;
		this.fakeWarp = true;
		EventCommandDelegate broadcastEventCommand = null;
		foreach (string eventKey in events.Keys)
		{
			this.CheckedHostPrecondition = false;
			string eventId = location.checkEventPrecondition(eventKey);
			if (!this.CheckedHostPrecondition || eventId == "-1" || string.IsNullOrEmpty(eventId) || !GameLocation.IsValidLocationEvent(eventKey, events[eventKey]) || (broadcastEventCommand == null && !Event.TryGetEventCommandHandler("BroadcastEvent", out broadcastEventCommand)))
			{
				continue;
			}
			if (this.notBroadcastEvents.Contains(eventId))
			{
				if (this.TryForceClientHostEvent(warp, location, eventId))
				{
					break;
				}
				continue;
			}
			if (this.broadcastEvents.Contains(eventId))
			{
				this.fakeFarmerId = warp.who.UniqueMultiplayerID;
				this.warpingHostBroadcastEvent = true;
				break;
			}
			string[] array = Event.ParseCommands(events[eventKey]);
			for (int i = 0; i < array.Length; i++)
			{
				string commandName = ArgUtility.Get(ArgUtility.SplitBySpaceQuoteAware(array[i]), 0);
				bool? flag = commandName?.StartsWith("--");
				if (flag.HasValue && flag != true && Event.TryGetEventCommandHandler(commandName, out var eventCommandHandler) && (object)eventCommandHandler == broadcastEventCommand)
				{
					this.fakeFarmerId = warp.who.UniqueMultiplayerID;
					this.warpingHostBroadcastEvent = true;
					this.broadcastEvents.Add(eventId);
					break;
				}
			}
			if (!this.warpingHostBroadcastEvent)
			{
				this.notBroadcastEvents.Add(eventId);
				if (this.TryForceClientHostEvent(warp, location, eventId))
				{
					break;
				}
			}
		}
		this.fakeWarp = false;
		Game1.xLocationAfterWarp = xLocationAfterWarp;
		Game1.yLocationAfterWarp = yLocationAfterWarp;
		if (this.warpingHostBroadcastEvent)
		{
			LocationRequest locationRequest = Game1.getLocationRequest(warp.name, warp.isStructure);
			locationRequest.OnWarp += delegate
			{
				this.warpingHostBroadcastEvent = false;
			};
			Game1.warpFarmer(locationRequest, warp.x, warp.y, warp.facingDirection);
		}
	}

	private bool IsWarping()
	{
		if (!Game1.isWarping && !this.warpingHostBroadcastEvent && !this.warpingSleep)
		{
			return this.warpingFestival;
		}
		return true;
	}

	public void DoHostAction(string action, params object[] data)
	{
		object[] messageData = new object[data.Length + 2];
		messageData[0] = (byte)1;
		messageData[1] = action;
		Array.Copy(data, 0, messageData, 2, data.Length);
		OutgoingMessage message = new OutgoingMessage(33, Game1.player, messageData);
		if (Game1.IsMasterGame)
		{
			IncomingMessage fakeMessage = new IncomingMessage();
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using BinaryWriter writer = new BinaryWriter(memoryStream);
				message.Write(writer);
				memoryStream.Seek(0L, SeekOrigin.Begin);
				using BinaryReader reader = new BinaryReader(memoryStream);
				fakeMessage.Read(reader);
			}
			Game1.multiplayer.processIncomingMessage(fakeMessage);
		}
		else if (Game1.HasDedicatedHost)
		{
			if (Game1.client != null)
			{
				Game1.client.sendMessage(message);
			}
		}
		else
		{
			Game1.log.Error("Tried to execute a host-only action '" + action + "' as a client on a non-dedicated server.");
		}
	}

	public void Tick()
	{
		if (!Game1.IsDedicatedHost)
		{
			return;
		}
		this.onlineIds.Clear();
		foreach (Farmer farmer in Game1.getOnlineFarmers())
		{
			if (!Game1.multiplayer.isDisconnecting(farmer) && farmer.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
			{
				this.onlineIds.Add(farmer.UniqueMultiplayerID);
			}
		}
		if (this.onlineIds.Count == 0)
		{
			this.farmerWarps.Clear();
			this.eventLocks.Clear();
			if (Game1.CurrentEvent?.isFestival ?? false)
			{
				if (Game1.netWorldState.Value.IsPaused)
				{
					Game1.netWorldState.Value.IsPaused = false;
				}
				if (!this.startedFestivalEnd)
				{
					Game1.CurrentEvent.TryStartEndFestivalDialogue(Game1.player);
					this.startedFestivalEnd = true;
				}
			}
			else if (!Game1.netWorldState.Value.IsPaused)
			{
				Game1.netWorldState.Value.IsPaused = true;
			}
			return;
		}
		if (Game1.netWorldState.Value.IsPaused)
		{
			Game1.netWorldState.Value.IsPaused = false;
		}
		if (Game1.player.Stamina < (float)Game1.player.MaxStamina)
		{
			Game1.player.Stamina = Game1.player.MaxStamina;
		}
		if (Game1.player.health < Game1.player.maxHealth)
		{
			Game1.player.health = Game1.player.maxHealth;
		}
		if (this.eventLocks.Count > 0)
		{
			List<string> removeLocations = new List<string>();
			List<string> removeEvents = new List<string>();
			foreach (KeyValuePair<string, Dictionary<string, long>> locationEntry in this.eventLocks)
			{
				removeEvents.Clear();
				foreach (KeyValuePair<string, long> eventEntry in locationEntry.Value)
				{
					if (!this.onlineIds.Contains(eventEntry.Value))
					{
						removeEvents.Add(eventEntry.Key);
					}
				}
				if (locationEntry.Value.Count - removeEvents.Count <= 0)
				{
					removeLocations.Add(locationEntry.Key);
					continue;
				}
				foreach (string eventToRemove in removeEvents)
				{
					locationEntry.Value.Remove(eventToRemove);
				}
			}
			foreach (string locationToRemove in removeLocations)
			{
				this.eventLocks.Remove(locationToRemove);
			}
		}
		FarmerWarp warp;
		while (this.farmerWarps.TryDequeue(out warp))
		{
			if (warp.who != null && this.onlineIds.Contains(warp.who.UniqueMultiplayerID))
			{
				this.CheckForWarpEvents(warp);
			}
		}
		if (this.IsWarping())
		{
			return;
		}
		if (Game1.activeClickableMenu is DialogueBox dialogueBox)
		{
			if (dialogueBox.isQuestion)
			{
				dialogueBox.selectedResponse = 0;
			}
			dialogueBox.receiveLeftClick(0, 0);
		}
		if (Game1.CurrentEvent != null)
		{
			if (!Game1.CurrentEvent.skipped && Game1.CurrentEvent.skippable)
			{
				Game1.CurrentEvent.skipped = true;
				Game1.CurrentEvent.skipEvent();
				Game1.freezeControls = false;
			}
			if (Game1.CurrentEvent.isFestival)
			{
				NPC festivalHost = Game1.CurrentEvent.festivalHost;
				if (festivalHost != null && !this.startedFestivalMainEvent && this.CheckOthersReady("MainEvent_" + Game1.CurrentEvent.id))
				{
					Game1.CurrentEvent.answerDialogueQuestion(festivalHost, "yes");
					this.startedFestivalMainEvent = true;
				}
			}
			if (!this.startedFestivalEnd && Game1.CurrentEvent.isFestival && this.CheckOthersReady("festivalEnd"))
			{
				Game1.CurrentEvent.TryStartEndFestivalDialogue(Game1.player);
				this.startedFestivalEnd = true;
			}
			return;
		}
		if (!this.warpingSleep && this.CheckOthersReady("sleep"))
		{
			if (Game1.currentLocation.NameOrUniqueName.EqualsIgnoreCase(Game1.player.homeLocation.Value))
			{
				this.HostSleepInBed();
			}
			else
			{
				this.warpingSleep = true;
				LocationRequest locationRequest = Game1.getLocationRequest(Game1.player.homeLocation.Value);
				locationRequest.OnWarp += delegate
				{
					this.HostSleepInBed();
				};
				Game1.warpFarmer(locationRequest, 5, 9, Game1.player.FacingDirection);
			}
		}
		if (!this.warpingFestival && Game1.whereIsTodaysFest != null && this.CheckOthersReady("festivalStart"))
		{
			this.warpingFestival = true;
			LocationRequest locationRequest2 = Game1.getLocationRequest(Game1.whereIsTodaysFest);
			locationRequest2.OnWarp += delegate
			{
				this.warpingFestival = false;
			};
			int tileX = -1;
			int tileY = -1;
			Utility.getDefaultWarpLocation(Game1.whereIsTodaysFest, ref tileX, ref tileY);
			Game1.warpFarmer(locationRequest2, tileX, tileY, 2);
		}
	}

	internal void HandleFarmerWarp(FarmerWarp warp)
	{
		if (Game1.IsDedicatedHost && warp.who != null)
		{
			this.farmerWarps.Enqueue(warp);
		}
	}

	private bool CheckOthersReady(string readyCheck)
	{
		if (readyCheck == "MainEvent_festival_fall16")
		{
			return this.shouldJudgeGrange;
		}
		int ready = Game1.netReady.GetNumberReady(readyCheck);
		if (ready <= 0)
		{
			return false;
		}
		if (!Game1.netReady.IsReady(readyCheck))
		{
			return ready >= Game1.netReady.GetNumberRequired(readyCheck) - 1;
		}
		return false;
	}

	private void HostSleepInBed()
	{
		if (Game1.currentLocation is FarmHouse farmHouse)
		{
			Game1.player.position.Set(Utility.PointToVector2(farmHouse.GetPlayerBedSpot()) * 64f);
			farmHouse.answerDialogueAction("Sleep_Yes", null);
		}
		this.warpingSleep = false;
	}

	private void ProcessEventDone(IncomingMessage message)
	{
		if (message.SourceFarmer == null)
		{
			return;
		}
		string name = message.Reader.ReadString();
		bool locationIsStructure = message.Reader.ReadByte() != 0;
		string eventId = message.Reader.ReadString();
		GameLocation location = Game1.getLocationFromName(name, locationIsStructure);
		if (location != null)
		{
			string key = (locationIsStructure ? "1" : "0") + location.NameOrUniqueName;
			if (this.eventLocks.TryGetValue(key, out var locationEvents) && locationEvents.TryGetValue(eventId, out var lockOwner) && lockOwner == message.SourceFarmer.UniqueMultiplayerID)
			{
				Game1.player.eventsSeen.Add(eventId);
				locationEvents.Remove(eventId);
			}
		}
	}

	private void ProcessHostAction(IncomingMessage message)
	{
		switch (message.Reader.ReadString())
		{
		case "ChooseCave":
			Event.hostActionChooseCave(message.SourceFarmer, message.Reader);
			break;
		case "NamePet":
			Event.hostActionNamePet(message.SourceFarmer, message.Reader);
			break;
		case "JudgeGrange":
			this.shouldJudgeGrange = true;
			break;
		}
	}

	public void ProcessMessage(IncomingMessage message)
	{
		switch ((DedicatedServerMessageType)message.Reader.ReadByte())
		{
		case DedicatedServerMessageType.EventDone:
			this.ProcessEventDone(message);
			break;
		case DedicatedServerMessageType.HostAction:
			this.ProcessHostAction(message);
			break;
		}
	}
}
