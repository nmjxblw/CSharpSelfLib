using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StardewValley.ConsoleAsync;

public class AsyncOperationManager
{
	private static AsyncOperationManager _instance;

	private List<IAsyncOperation> _pendingOps;

	private List<IAsyncOperation> _tempOps;

	private List<IAsyncOperation> _doneOps;

	public static AsyncOperationManager Use => AsyncOperationManager._instance;

	public static void Init()
	{
		AsyncOperationManager._instance = new AsyncOperationManager();
	}

	private AsyncOperationManager()
	{
		this._pendingOps = new List<IAsyncOperation>();
		this._tempOps = new List<IAsyncOperation>();
		this._doneOps = new List<IAsyncOperation>();
	}

	public void AddPending(Task task, Action<GenericResult> doneAction)
	{
		GenericOp op = new GenericOp();
		op.DoneCallback = OnDone;
		op.Task = task;
		if (task.Status > TaskStatus.Created)
		{
			op.TaskStarted = true;
		}
		this.AddPending(op);
		void OnDone()
		{
			GenericResult res = new GenericResult
			{
				Ex = op.Task.Exception
			};
			if (res.Ex != null)
			{
				res.Ex = res.Ex.GetBaseException();
			}
			res.Failed = res.Ex != null;
			res.Success = res.Ex == null;
			doneAction(res);
		}
	}

	public void AddPending(Action workAction, Action<GenericResult> doneAction)
	{
		GenericOp op = new GenericOp();
		op.DoneCallback = OnDone;
		Task task = new Task(workAction);
		op.Task = task;
		this.AddPending(op);
		void OnDone()
		{
			GenericResult res = new GenericResult
			{
				Ex = op.Task.Exception
			};
			if (res.Ex != null)
			{
				res.Ex = res.Ex.GetBaseException();
			}
			res.Failed = res.Ex != null;
			res.Success = res.Ex == null;
			doneAction(res);
		}
	}

	public void AddPending(IAsyncOperation op)
	{
		lock (this._pendingOps)
		{
			this._pendingOps.Add(op);
		}
	}

	public void Update()
	{
		lock (this._pendingOps)
		{
			this._doneOps.Clear();
			this._tempOps.Clear();
			this._tempOps.AddRange(this._pendingOps);
			this._pendingOps.Clear();
			bool working = false;
			for (int i = 0; i < this._tempOps.Count; i++)
			{
				IAsyncOperation op = this._tempOps[i];
				if (working)
				{
					this._pendingOps.Add(op);
					continue;
				}
				working = true;
				if (!op.Started)
				{
					op.Begin();
					this._pendingOps.Add(op);
				}
				else if (op.Done)
				{
					this._doneOps.Add(op);
				}
				else
				{
					this._pendingOps.Add(op);
				}
			}
			this._tempOps.Clear();
		}
		for (int j = 0; j < this._doneOps.Count; j++)
		{
			this._doneOps[j].Conclude();
		}
		this._doneOps.Clear();
	}
}
