using System;
using System.Collections.Generic;
using StardewValley.Network;

namespace StardewValley;

public class MultipleMutexRequest
{
	protected int _reportedCount;

	protected List<NetMutex> _acquiredLocks;

	protected List<NetMutex> _mutexList;

	protected Action<MultipleMutexRequest> _onSuccess;

	protected Action<MultipleMutexRequest> _onFailure;

	public MultipleMutexRequest(List<NetMutex> mutexes, Action<MultipleMutexRequest> success_callback = null, Action<MultipleMutexRequest> failure_callback = null)
	{
		this._onSuccess = success_callback;
		this._onFailure = failure_callback;
		this._acquiredLocks = new List<NetMutex>();
		this._mutexList = new List<NetMutex>(mutexes);
		this._RequestMutexes();
	}

	public MultipleMutexRequest(NetMutex[] mutexes, Action<MultipleMutexRequest> success_callback = null, Action<MultipleMutexRequest> failure_callback = null)
	{
		this._onSuccess = success_callback;
		this._onFailure = failure_callback;
		this._acquiredLocks = new List<NetMutex>();
		this._mutexList = new List<NetMutex>(mutexes);
		this._RequestMutexes();
	}

	protected void _RequestMutexes()
	{
		if (this._mutexList == null)
		{
			this._onFailure?.Invoke(this);
			return;
		}
		if (this._mutexList.Count == 0)
		{
			this._onSuccess?.Invoke(this);
			return;
		}
		for (int i = 0; i < this._mutexList.Count; i++)
		{
			if (this._mutexList[i].IsLocked())
			{
				this._onFailure?.Invoke(this);
				return;
			}
		}
		for (int j = 0; j < this._mutexList.Count; j++)
		{
			NetMutex mutex = this._mutexList[j];
			mutex.RequestLock(delegate
			{
				this._OnLockAcquired(mutex);
			}, delegate
			{
				this._OnLockFailed(mutex);
			});
		}
	}

	protected void _OnLockAcquired(NetMutex mutex)
	{
		this._reportedCount++;
		this._acquiredLocks.Add(mutex);
		if (this._reportedCount >= this._mutexList.Count)
		{
			this._Finalize();
		}
	}

	protected void _OnLockFailed(NetMutex mutex)
	{
		this._reportedCount++;
		if (this._reportedCount >= this._mutexList.Count)
		{
			this._Finalize();
		}
	}

	protected void _Finalize()
	{
		if (this._acquiredLocks.Count < this._mutexList.Count)
		{
			this.ReleaseLocks();
			this._onFailure(this);
		}
		else
		{
			this._onSuccess(this);
		}
	}

	public void ReleaseLocks()
	{
		for (int i = 0; i < this._acquiredLocks.Count; i++)
		{
			this._acquiredLocks[i].ReleaseLock();
		}
		this._acquiredLocks.Clear();
	}
}
