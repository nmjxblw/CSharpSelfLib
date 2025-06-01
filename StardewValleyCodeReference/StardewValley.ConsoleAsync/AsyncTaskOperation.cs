using System.Threading.Tasks;

namespace StardewValley.ConsoleAsync;

public abstract class AsyncTaskOperation : IAsyncOperation
{
	public Task Task;

	public bool TaskStarted;

	bool IAsyncOperation.Started => this.TaskStarted;

	public abstract bool Done { get; }

	void IAsyncOperation.Begin()
	{
		DebugTools.Assert(!this.TaskStarted, "AsyncTaskOperation.Begin called but TaskStarted already is true!");
		this.TaskStarted = true;
		this.Task.Start();
	}

	public abstract void Conclude();
}
