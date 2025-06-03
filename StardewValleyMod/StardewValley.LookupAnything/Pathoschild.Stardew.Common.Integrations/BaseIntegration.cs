using System;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations;

internal abstract class BaseIntegration : IModIntegration
{
	protected string ModID { get; }

	protected IModRegistry ModRegistry { get; }

	protected IMonitor Monitor { get; }

	public string Label { get; }

	public virtual bool IsLoaded { get; }

	protected BaseIntegration(string label, string modID, string minVersion, IModRegistry modRegistry, IMonitor monitor)
	{
		this.Label = label;
		this.ModID = modID;
		this.ModRegistry = modRegistry;
		this.Monitor = monitor;
		IModInfo obj = modRegistry.Get(this.ModID);
		IManifest manifest = ((obj != null) ? obj.Manifest : null);
		if (manifest != null)
		{
			if (manifest.Version.IsOlderThan(minVersion))
			{
				monitor.Log($"Detected {label} {manifest.Version}, but need {minVersion} or later. Disabled integration with this mod.", (LogLevel)3);
			}
			else
			{
				this.IsLoaded = true;
			}
		}
	}

	protected TApi? GetValidatedApi<TApi>() where TApi : class
	{
		TApi api = this.ModRegistry.GetApi<TApi>(this.ModID);
		if (api == null)
		{
			this.Monitor.Log("Detected " + this.Label + ", but couldn't fetch its API. Disabled integration with this mod.", (LogLevel)3);
			return null;
		}
		return api;
	}

	protected virtual void AssertLoaded()
	{
		if (!this.IsLoaded)
		{
			throw new InvalidOperationException("The " + this.Label + " integration isn't loaded.");
		}
	}
}
internal abstract class BaseIntegration<TApi> : BaseIntegration where TApi : class
{
	public TApi? ModApi { get; }

	[MemberNotNullWhen(true, "ModApi")]
	public override bool IsLoaded
	{
		[MemberNotNullWhen(true, "ModApi")]
		get
		{
			return this.ModApi != null;
		}
	}

	protected BaseIntegration(string label, string modID, string minVersion, IModRegistry modRegistry, IMonitor monitor)
		: base(label, modID, minVersion, modRegistry, monitor)
	{
		if (base.IsLoaded)
		{
			this.ModApi = base.GetValidatedApi<TApi>();
		}
	}

	[MemberNotNull("ModApi")]
	protected override void AssertLoaded()
	{
		if (!this.IsLoaded)
		{
			throw new InvalidOperationException("The " + base.Label + " integration isn't loaded.");
		}
	}
}
