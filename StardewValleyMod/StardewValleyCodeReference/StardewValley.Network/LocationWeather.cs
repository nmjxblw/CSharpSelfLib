using System;
using Netcode;
using StardewValley.GameData.LocationContexts;

namespace StardewValley.Network;

public class LocationWeather : INetObject<NetFields>
{
	public readonly NetString weatherForTomorrow = new NetString();

	public readonly NetString weather = new NetString();

	public readonly NetBool isRaining = new NetBool();

	public readonly NetBool isSnowing = new NetBool();

	public readonly NetBool isLightning = new NetBool();

	public readonly NetBool isDebrisWeather = new NetBool();

	public readonly NetBool isGreenRain = new NetBool();

	public readonly NetInt monthlyNonRainyDayCount = new NetInt();

	public NetFields NetFields { get; } = new NetFields("LocationWeather");

	public string WeatherForTomorrow
	{
		get
		{
			return this.weatherForTomorrow.Value;
		}
		set
		{
			this.weatherForTomorrow.Value = value;
		}
	}

	public string Weather
	{
		get
		{
			return this.weather.Value;
		}
		set
		{
			this.weather.Value = value;
		}
	}

	public bool IsRaining
	{
		get
		{
			return this.isRaining.Value;
		}
		set
		{
			this.isRaining.Value = value;
		}
	}

	public bool IsSnowing
	{
		get
		{
			return this.isSnowing.Value;
		}
		set
		{
			this.isSnowing.Value = value;
		}
	}

	public bool IsLightning
	{
		get
		{
			return this.isLightning.Value;
		}
		set
		{
			this.isLightning.Value = value;
		}
	}

	public bool IsDebrisWeather
	{
		get
		{
			return this.isDebrisWeather.Value;
		}
		set
		{
			this.isDebrisWeather.Value = value;
		}
	}

	public bool IsGreenRain
	{
		get
		{
			return this.isGreenRain.Value;
		}
		set
		{
			this.isGreenRain.Value = value;
			if (value)
			{
				this.IsRaining = true;
			}
		}
	}

	public LocationWeather()
	{
		this.NetFields.SetOwner(this).AddField(this.weatherForTomorrow, "weatherForTomorrow").AddField(this.weather, "weather")
			.AddField(this.isRaining, "isRaining")
			.AddField(this.isSnowing, "isSnowing")
			.AddField(this.isLightning, "isLightning")
			.AddField(this.isDebrisWeather, "isDebrisWeather")
			.AddField(this.isGreenRain, "isGreenRain")
			.AddField(this.monthlyNonRainyDayCount, "monthlyNonRainyDayCount");
	}

	public void InitializeDayWeather()
	{
		this.Weather = this.WeatherForTomorrow;
		this.IsRaining = false;
		this.IsSnowing = false;
		this.IsLightning = false;
		this.IsDebrisWeather = false;
		this.IsGreenRain = false;
	}

	public void UpdateDailyWeather(string locationContextId, LocationContextData data, Random random)
	{
		this.InitializeDayWeather();
		switch (this.WeatherForTomorrow)
		{
		case "Rain":
			this.IsRaining = true;
			break;
		case "GreenRain":
			this.IsGreenRain = true;
			break;
		case "Storm":
			this.IsRaining = true;
			this.IsLightning = true;
			break;
		case "Wind":
			this.IsDebrisWeather = true;
			break;
		case "Snow":
			this.IsSnowing = true;
			break;
		}
		this.WeatherForTomorrow = "Sun";
		WorldDate tomorrow = new WorldDate(Game1.Date);
		tomorrow.TotalDays++;
		if (Utility.isFestivalDay(tomorrow.DayOfMonth, tomorrow.Season, locationContextId))
		{
			this.WeatherForTomorrow = "Festival";
			return;
		}
		if (Utility.TryGetPassiveFestivalDataForDay(tomorrow.DayOfMonth, tomorrow.Season, locationContextId, out var _, out var _))
		{
			this.WeatherForTomorrow = "Sun";
			return;
		}
		foreach (WeatherCondition weatherCondition in data.WeatherConditions)
		{
			if (GameStateQuery.CheckConditions(weatherCondition.Condition, null, null, null, null, random))
			{
				this.WeatherForTomorrow = weatherCondition.Weather;
				break;
			}
		}
	}

	public void CopyFrom(LocationWeather other)
	{
		this.Weather = other.Weather;
		this.IsRaining = other.IsRaining;
		this.IsSnowing = other.IsSnowing;
		this.IsLightning = other.IsLightning;
		this.IsDebrisWeather = other.IsDebrisWeather;
		this.IsGreenRain = other.IsGreenRain;
		this.WeatherForTomorrow = other.WeatherForTomorrow;
		this.monthlyNonRainyDayCount.Value = other.monthlyNonRainyDayCount.Value;
		if (this.Weather == null)
		{
			if (this.IsLightning)
			{
				this.Weather = "Storm";
			}
			else if (this.IsRaining)
			{
				this.Weather = "Rain";
			}
			else if (this.IsSnowing)
			{
				this.Weather = "Snow";
			}
			else if (this.IsDebrisWeather)
			{
				this.Weather = "Wind";
			}
			else
			{
				this.Weather = "Sun";
			}
		}
	}
}
