using System.Collections.Generic;

namespace StardewValley.Pathfinding;

public class PriorityQueue
{
	private int total_size;

	private SortedDictionary<int, Queue<PathNode>> nodes;

	public PriorityQueue()
	{
		this.nodes = new SortedDictionary<int, Queue<PathNode>>();
		this.total_size = 0;
	}

	public bool IsEmpty()
	{
		return this.total_size == 0;
	}

	public void Clear()
	{
		this.total_size = 0;
		foreach (KeyValuePair<int, Queue<PathNode>> node in this.nodes)
		{
			node.Value.Clear();
		}
	}

	public bool Contains(PathNode p, int priority)
	{
		if (!this.nodes.TryGetValue(priority, out var v))
		{
			return false;
		}
		return v.Contains(p);
	}

	public PathNode Dequeue()
	{
		if (!this.IsEmpty())
		{
			foreach (Queue<PathNode> q in this.nodes.Values)
			{
				if (q.Count > 0)
				{
					this.total_size--;
					return q.Dequeue();
				}
			}
		}
		return null;
	}

	public object Peek()
	{
		if (!this.IsEmpty())
		{
			foreach (Queue<PathNode> q in this.nodes.Values)
			{
				if (q.Count > 0)
				{
					return q.Peek();
				}
			}
		}
		return null;
	}

	public object Dequeue(int priority)
	{
		this.total_size--;
		return this.nodes[priority].Dequeue();
	}

	public void Enqueue(PathNode item, int priority)
	{
		if (!this.nodes.TryGetValue(priority, out var node))
		{
			this.nodes.Add(priority, new Queue<PathNode>());
			this.Enqueue(item, priority);
		}
		else
		{
			node.Enqueue(item);
			this.total_size++;
		}
	}
}
