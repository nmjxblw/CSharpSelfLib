using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Menus;

namespace StardewValley.Util;

public class EventTest
{
	private int currentEventIndex;

	private int currentLocationIndex;

	private int aButtonTimer;

	private List<string> specificEventsToDo = new List<string>();

	private bool doingSpecifics;

	public EventTest(string startingLocationName = "", int startingEventIndex = 0)
	{
		this.currentLocationIndex = 0;
		if (startingLocationName.Length > 0)
		{
			for (int i = 0; i < Game1.locations.Count; i++)
			{
				if (Game1.locations[i].Name.Equals(startingLocationName))
				{
					this.currentLocationIndex = i;
					break;
				}
			}
		}
		this.currentEventIndex = startingEventIndex;
	}

	public EventTest(string[] whichEvents)
	{
		for (int i = 1; i < whichEvents.Length; i += 2)
		{
			this.specificEventsToDo.Add(whichEvents[i] + " " + whichEvents[i + 1]);
		}
		this.doingSpecifics = true;
		this.currentLocationIndex = -1;
	}

	public void update()
	{
		if (!Game1.eventUp && !Game1.fadeToBlack)
		{
			if (this.currentLocationIndex >= Game1.locations.Count)
			{
				return;
			}
			if (this.doingSpecifics && this.currentLocationIndex == -1)
			{
				if (this.specificEventsToDo.Count == 0)
				{
					return;
				}
				for (int i = 0; i < Game1.locations.Count; i++)
				{
					string lastEvent = this.specificEventsToDo.Last();
					string[] lastEventParts = ArgUtility.SplitBySpace(lastEvent);
					if (!Game1.locations[i].Name.Equals(lastEventParts[0]))
					{
						continue;
					}
					this.currentLocationIndex = i;
					int j = -1;
					foreach (KeyValuePair<string, string> pair in Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + Game1.locations[i].Name))
					{
						j++;
						if (int.TryParse(pair.Key.Split('/')[0], out var result) && result == Convert.ToInt32(lastEventParts[1]))
						{
							this.currentEventIndex = j;
							break;
						}
					}
					this.specificEventsToDo.Remove(lastEvent);
					break;
				}
			}
			GameLocation l = Game1.locations[this.currentLocationIndex];
			if (l.currentEvent != null)
			{
				return;
			}
			string locationName = l.name.Value;
			if (locationName == "Pool")
			{
				locationName = "BathHouse_Pool";
			}
			bool exists = true;
			Dictionary<string, string> data = null;
			try
			{
				data = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + locationName);
			}
			catch (Exception)
			{
				exists = false;
			}
			if (exists && this.currentEventIndex < data.Count)
			{
				KeyValuePair<string, string> entry = data.ElementAt(this.currentEventIndex);
				string key = entry.Key;
				string script = entry.Value;
				if (key.Contains('/') && !script.Equals("null"))
				{
					if (Game1.currentLocation.Name.Equals(locationName))
					{
						Game1.eventUp = true;
						Game1.currentLocation.currentEvent = new Event(script);
					}
					else
					{
						LocationRequest locationRequest = Game1.getLocationRequest(locationName);
						locationRequest.OnLoad += delegate
						{
							Game1.currentLocation.currentEvent = new Event(script);
						};
						Game1.warpFarmer(locationRequest, 8, 8, Game1.player.FacingDirection);
					}
				}
			}
			this.currentEventIndex++;
			if (!exists || this.currentEventIndex >= data.Count)
			{
				this.currentEventIndex = 0;
				this.currentLocationIndex++;
			}
			if (this.doingSpecifics)
			{
				this.currentLocationIndex = -1;
			}
			return;
		}
		this.aButtonTimer -= (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
		if (this.aButtonTimer < 0)
		{
			this.aButtonTimer = 100;
			if (Game1.activeClickableMenu is DialogueBox dialogueBox)
			{
				dialogueBox.performHoverAction(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height - 64 - Game1.random.Next(300));
				dialogueBox.receiveLeftClick(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height - 64 - Game1.random.Next(300));
			}
		}
	}
}
