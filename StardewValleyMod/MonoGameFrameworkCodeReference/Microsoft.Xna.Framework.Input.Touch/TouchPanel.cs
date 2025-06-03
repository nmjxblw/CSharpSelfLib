using System;

namespace Microsoft.Xna.Framework.Input.Touch;

/// <summary>
/// Allows retrieval of information from Touch Panel device.
/// </summary>
public static class TouchPanel
{
	internal static GameWindow PrimaryWindow;

	/// <summary>
	/// The window handle of the touch panel. Purely for Xna compatibility.
	/// </summary>
	public static IntPtr WindowHandle
	{
		get
		{
			return TouchPanel.PrimaryWindow.TouchPanelState.WindowHandle;
		}
		set
		{
			TouchPanel.PrimaryWindow.TouchPanelState.WindowHandle = value;
		}
	}

	/// <summary>
	/// Gets or sets the display height of the touch panel.
	/// </summary>
	public static int DisplayHeight
	{
		get
		{
			return TouchPanel.PrimaryWindow.TouchPanelState.DisplayHeight;
		}
		set
		{
			TouchPanel.PrimaryWindow.TouchPanelState.DisplayHeight = value;
		}
	}

	/// <summary>
	/// Gets or sets the display orientation of the touch panel.
	/// </summary>
	public static DisplayOrientation DisplayOrientation
	{
		get
		{
			return TouchPanel.PrimaryWindow.TouchPanelState.DisplayOrientation;
		}
		set
		{
			TouchPanel.PrimaryWindow.TouchPanelState.DisplayOrientation = value;
		}
	}

	/// <summary>
	/// Gets or sets the display width of the touch panel.
	/// </summary>
	public static int DisplayWidth
	{
		get
		{
			return TouchPanel.PrimaryWindow.TouchPanelState.DisplayWidth;
		}
		set
		{
			TouchPanel.PrimaryWindow.TouchPanelState.DisplayWidth = value;
		}
	}

	/// <summary>
	/// Gets or sets enabled gestures.
	/// </summary>
	public static GestureType EnabledGestures
	{
		get
		{
			return TouchPanel.PrimaryWindow.TouchPanelState.EnabledGestures;
		}
		set
		{
			TouchPanel.PrimaryWindow.TouchPanelState.EnabledGestures = value;
		}
	}

	public static bool EnableMouseTouchPoint
	{
		get
		{
			return TouchPanel.PrimaryWindow.TouchPanelState.EnableMouseTouchPoint;
		}
		set
		{
			TouchPanel.PrimaryWindow.TouchPanelState.EnableMouseTouchPoint = value;
		}
	}

	public static bool EnableMouseGestures
	{
		get
		{
			return TouchPanel.PrimaryWindow.TouchPanelState.EnableMouseGestures;
		}
		set
		{
			TouchPanel.PrimaryWindow.TouchPanelState.EnableMouseGestures = value;
		}
	}

	/// <summary>
	/// Returns true if a touch gesture is available.
	/// </summary>
	public static bool IsGestureAvailable => TouchPanel.PrimaryWindow.TouchPanelState.IsGestureAvailable;

	/// <summary>
	/// Gets the current state of the touch panel.
	/// </summary>
	/// <returns><see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchCollection" /></returns>
	public static TouchCollection GetState()
	{
		return TouchPanel.PrimaryWindow.TouchPanelState.GetState();
	}

	public static TouchPanelState GetState(GameWindow window)
	{
		return window.TouchPanelState;
	}

	public static TouchPanelCapabilities GetCapabilities()
	{
		return TouchPanel.PrimaryWindow.TouchPanelState.GetCapabilities();
	}

	internal static void AddEvent(int id, TouchLocationState state, Vector2 position)
	{
		TouchPanel.AddEvent(id, state, position, isMouse: false);
	}

	internal static void AddEvent(int id, TouchLocationState state, Vector2 position, bool isMouse)
	{
		TouchPanel.PrimaryWindow.TouchPanelState.AddEvent(id, state, position, isMouse);
	}

	/// <summary>
	/// Returns the next available gesture on touch panel device.
	/// </summary>
	/// <returns><see cref="T:Microsoft.Xna.Framework.Input.Touch.GestureSample" /></returns>
	public static GestureSample ReadGesture()
	{
		return TouchPanel.PrimaryWindow.TouchPanelState.GestureList.Dequeue();
	}
}
