using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

public sealed class GraphicsAdapter : IDisposable
{
	/// <summary>
	/// Defines the driver type for graphics adapter. Usable only on DirectX platforms for now.
	/// </summary>
	public enum DriverType
	{
		/// <summary>
		/// Hardware device been used for rendering. Maximum speed and performance.
		/// </summary>
		Hardware,
		/// <summary>
		/// Emulates the hardware device on CPU. Slowly, only for testing.
		/// </summary>
		Reference,
		/// <summary>
		/// Useful when <see cref="F:Microsoft.Xna.Framework.Graphics.GraphicsAdapter.DriverType.Hardware" /> acceleration does not work.
		/// </summary>
		FastSoftware
	}

	private static ReadOnlyCollection<GraphicsAdapter> _adapters;

	private DisplayModeCollection _supportedDisplayModes;

	private int _displayIndex;

	public string Description
	{
		get
		{
			try
			{
				return GL.GetString(StringName.Renderer);
			}
			catch
			{
				return string.Empty;
			}
		}
		private set
		{
		}
	}

	public DisplayMode CurrentDisplayMode
	{
		get
		{
			Sdl.Display.GetCurrentDisplayMode(Sdl.Display.GetWindowDisplayIndex(SdlGameWindow.Instance.Handle), out var mode);
			return new DisplayMode(mode.Width, mode.Height, SurfaceFormat.Color);
		}
	}

	public static GraphicsAdapter DefaultAdapter => GraphicsAdapter.Adapters[0];

	public static ReadOnlyCollection<GraphicsAdapter> Adapters
	{
		get
		{
			if (GraphicsAdapter._adapters == null)
			{
				GraphicsAdapter._adapters = new ReadOnlyCollection<GraphicsAdapter>(new GraphicsAdapter[1]
				{
					new GraphicsAdapter()
				});
			}
			return GraphicsAdapter._adapters;
		}
	}

	/// <summary>
	/// Used to request creation of the reference graphics device, 
	/// or the default hardware accelerated device (when set to false).
	/// </summary>
	/// <remarks>
	/// This only works on DirectX platforms where a reference graphics
	/// device is available and must be defined before the graphics device
	/// is created. It defaults to false.
	/// </remarks>
	public static bool UseReferenceDevice
	{
		get
		{
			return GraphicsAdapter.UseDriverType == DriverType.Reference;
		}
		set
		{
			GraphicsAdapter.UseDriverType = (value ? DriverType.Reference : DriverType.Hardware);
		}
	}

	/// <summary>
	/// Used to request creation of a specific kind of driver.
	/// </summary>
	/// <remarks>
	/// These values only work on DirectX platforms and must be defined before the graphics device
	/// is created. <see cref="F:Microsoft.Xna.Framework.Graphics.GraphicsAdapter.DriverType.Hardware" /> by default.
	/// </remarks>
	public static DriverType UseDriverType { get; set; }

	public DisplayModeCollection SupportedDisplayModes
	{
		get
		{
			bool displayChanged = false;
			int displayIndex = Sdl.Display.GetWindowDisplayIndex(SdlGameWindow.Instance.Handle);
			displayChanged = displayIndex != this._displayIndex;
			if (this._supportedDisplayModes == null || displayChanged)
			{
				List<DisplayMode> modes = new List<DisplayMode>(new DisplayMode[1] { this.CurrentDisplayMode });
				this._displayIndex = displayIndex;
				modes.Clear();
				int modeCount = Sdl.Display.GetNumDisplayModes(displayIndex);
				for (int i = 0; i < modeCount; i++)
				{
					Sdl.Display.GetDisplayMode(displayIndex, i, out var mode);
					DisplayMode displayMode = new DisplayMode(mode.Width, mode.Height, SurfaceFormat.Color);
					if (!modes.Contains(displayMode))
					{
						modes.Add(displayMode);
					}
				}
				modes.Sort(delegate(DisplayMode a, DisplayMode b)
				{
					if (a == b)
					{
						return 0;
					}
					return (a.Format > b.Format || a.Width > b.Width || a.Height > b.Height) ? 1 : (-1);
				});
				this._supportedDisplayModes = new DisplayModeCollection(modes);
			}
			return this._supportedDisplayModes;
		}
	}

	/// <summary>
	/// Gets a <see cref="T:System.Boolean" /> indicating whether
	/// <see cref="P:Microsoft.Xna.Framework.Graphics.GraphicsAdapter.CurrentDisplayMode" /> has a
	/// Width:Height ratio corresponding to a widescreen <see cref="T:Microsoft.Xna.Framework.Graphics.DisplayMode" />.
	/// Common widescreen modes include 16:9, 16:10 and 2:1.
	/// </summary>
	public bool IsWideScreen => this.CurrentDisplayMode.AspectRatio > 1.3333334f;

	public void Dispose()
	{
	}

	/// <summary>
	/// Queries for support of the requested render target format on the adaptor.
	/// </summary>
	/// <param name="graphicsProfile">The graphics profile.</param>
	/// <param name="format">The requested surface format.</param>
	/// <param name="depthFormat">The requested depth stencil format.</param>
	/// <param name="multiSampleCount">The requested multisample count.</param>
	/// <param name="selectedFormat">Set to the best format supported by the adaptor for the requested surface format.</param>
	/// <param name="selectedDepthFormat">Set to the best format supported by the adaptor for the requested depth stencil format.</param>
	/// <param name="selectedMultiSampleCount">Set to the best count supported by the adaptor for the requested multisample count.</param>
	/// <returns>True if the requested format is supported by the adaptor. False if one or more of the values was changed.</returns>
	public bool QueryRenderTargetFormat(GraphicsProfile graphicsProfile, SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount, out SurfaceFormat selectedFormat, out DepthFormat selectedDepthFormat, out int selectedMultiSampleCount)
	{
		selectedFormat = format;
		selectedDepthFormat = depthFormat;
		selectedMultiSampleCount = multiSampleCount;
		if (selectedFormat == SurfaceFormat.Alpha8 || selectedFormat == SurfaceFormat.NormalizedByte2 || selectedFormat == SurfaceFormat.NormalizedByte4 || selectedFormat == SurfaceFormat.Dxt1 || selectedFormat == SurfaceFormat.Dxt3 || selectedFormat == SurfaceFormat.Dxt5 || selectedFormat == SurfaceFormat.Dxt1a || selectedFormat == SurfaceFormat.Dxt1SRgb || selectedFormat == SurfaceFormat.Dxt3SRgb || selectedFormat == SurfaceFormat.Dxt5SRgb)
		{
			selectedFormat = SurfaceFormat.Color;
		}
		if (format == selectedFormat && depthFormat == selectedDepthFormat)
		{
			return multiSampleCount == selectedMultiSampleCount;
		}
		return false;
	}

	public bool IsProfileSupported(GraphicsProfile graphicsProfile)
	{
		if (GraphicsAdapter.UseReferenceDevice)
		{
			return true;
		}
		return graphicsProfile switch
		{
			GraphicsProfile.Reach => true, 
			GraphicsProfile.HiDef => true, 
			_ => throw new InvalidOperationException(), 
		};
	}
}
