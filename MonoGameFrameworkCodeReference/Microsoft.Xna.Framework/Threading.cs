using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Xna.Framework;

internal class Threading
{
	/// <summary>
	/// Static helper that provides a generic action queue
	/// but a non-generic dequeue-and-invoke <see cref="T:System.Action" />.
	/// </summary>
	/// <typeparam name="TState"></typeparam>
	private static class StateActionHelper<TState>
	{
		public struct QueuedAction
		{
			public ManualResetEventSlim ResetEvent;

			public Action<TState> Action;

			public TState State;
		}

		public static readonly Queue<QueuedAction> Queue = new Queue<QueuedAction>();

		public static readonly Action DequeueAction = Dequeue;

		public static void Dequeue()
		{
			QueuedAction item = StateActionHelper<TState>.Queue.Dequeue();
			item.Action(item.State);
			item.ResetEvent.Set();
		}
	}

	private static int _mainThreadId;

	private static Stack<ManualResetEventSlim> _resetEventPool;

	private static List<Action> _queuedActions;

	private static readonly Action<Action> _metaAction;

	static Threading()
	{
		Threading._resetEventPool = new Stack<ManualResetEventSlim>();
		Threading._queuedActions = new List<Action>();
		Threading._metaAction = delegate(Action a)
		{
			a();
		};
		Threading._mainThreadId = Thread.CurrentThread.ManagedThreadId;
	}

	/// <summary>
	/// Checks if the code is currently running on the UI thread.
	/// </summary>
	/// <returns>true if the code is currently running on the UI thread.</returns>
	public static bool IsOnUIThread()
	{
		return Threading._mainThreadId == Thread.CurrentThread.ManagedThreadId;
	}

	/// <summary>
	/// Throws an exception if the code is not currently running on the UI thread.
	/// </summary>
	/// <exception cref="T:System.InvalidOperationException">Thrown if the code is not currently running on the UI thread.</exception>
	public static void EnsureUIThread()
	{
		if (!Threading.IsOnUIThread())
		{
			throw new InvalidOperationException("Operation not called on UI thread.");
		}
	}

	/// <summary>
	/// Runs the given action on the UI thread and blocks the current thread while the action is running.
	/// If the current thread is the UI thread, the action will run immediately.
	/// </summary>
	/// <param name="action">The action to be run on the UI thread</param>
	internal static void BlockOnUIThread(Action action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		Threading.BlockOnUIThread(Threading._metaAction, action);
	}

	/// <summary>
	/// Runs the given action on the UI thread and blocks the current thread while the action is running.
	/// If the current thread is the UI thread, the action will run immediately.
	/// </summary>
	/// <param name="action">The action to be run on the UI thread</param>
	/// <param name="state">The data to pass to <paramref name="action" /></param>.
	internal static void BlockOnUIThread<TState>(Action<TState> action, TState state)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (Threading.IsOnUIThread())
		{
			action(state);
			return;
		}
		ManualResetEventSlim resetEvent = Threading.RentResetEvent();
		StateActionHelper<TState>.QueuedAction queuedAction = new StateActionHelper<TState>.QueuedAction
		{
			ResetEvent = resetEvent,
			Action = action,
			State = state
		};
		lock (Threading._queuedActions)
		{
			StateActionHelper<TState>.Queue.Enqueue(queuedAction);
			Threading._queuedActions.Add(StateActionHelper<TState>.DequeueAction);
		}
		try
		{
			resetEvent.Wait();
		}
		finally
		{
			Threading.ReturnResetEvent(resetEvent);
		}
	}

	private static ManualResetEventSlim RentResetEvent()
	{
		lock (Threading._resetEventPool)
		{
			if (Threading._resetEventPool.Count > 0)
			{
				return Threading._resetEventPool.Pop();
			}
		}
		return new ManualResetEventSlim();
	}

	private static void ReturnResetEvent(ManualResetEventSlim resetEvent)
	{
		resetEvent.Reset();
		lock (Threading._resetEventPool)
		{
			Threading._resetEventPool.Push(resetEvent);
		}
	}

	/// <summary>
	/// Runs all pending actions. Must be called from the UI thread.
	/// </summary>
	internal static void Run()
	{
		Threading.EnsureUIThread();
		lock (Threading._queuedActions)
		{
			foreach (Action queuedAction in Threading._queuedActions)
			{
				queuedAction();
			}
			Threading._queuedActions.Clear();
		}
	}
}
