using System;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Provides helper methods to make it easier
/// to safely raise events.
/// </summary>
internal static class EventHelpers
{
	/// <summary>
	/// Safely raises an event by storing a copy of the event's delegate
	/// in the <paramref name="handler" /> parameter and checking it for
	/// null before invoking it.
	/// </summary>
	/// <typeparam name="TEventArgs"></typeparam>
	/// <param name="sender">The object raising the event.</param>
	/// <param name="handler"><see cref="T:System.EventHandler`1" /> to be invoked</param>
	/// <param name="e">The <typeparamref name="TEventArgs" /> passed to <see cref="T:System.EventHandler`1" /></param>
	internal static void Raise<TEventArgs>(object sender, EventHandler<TEventArgs> handler, TEventArgs e)
	{
		handler?.Invoke(sender, e);
	}

	/// <summary>
	/// Safely raises an event by storing a copy of the event's delegate
	/// in the <paramref name="handler" /> parameter and checking it for
	/// null before invoking it.
	/// </summary>
	/// <param name="sender">The object raising the event.</param>
	/// <param name="handler"><see cref="T:System.EventHandler" /> to be invoked</param>
	/// <param name="e">The <see cref="T:System.EventArgs" /> passed to <see cref="T:System.EventHandler" /></param>
	internal static void Raise(object sender, EventHandler handler, EventArgs e)
	{
		handler?.Invoke(sender, e);
	}
}
