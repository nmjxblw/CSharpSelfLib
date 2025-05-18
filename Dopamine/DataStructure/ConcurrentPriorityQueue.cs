using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
namespace Dopamine
{
	/// <summary>
	/// 线程安全优先队列
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ConcurrentPriorityQueue<T> : IProducerConsumerCollection<T>, IReadOnlyCollection<T>
	{
		#region 核心数据结构
		private readonly List<T> _heap = new List<T>();
		private readonly Func<T, IComparable> _prioritySelector;
		private readonly object _syncRoot = new object();
		private const int DefaultCapacity = 4;
		#endregion

		#region 构造函数
		/// <summary>
		/// 
		/// </summary>
		/// <param name="prioritySelector"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public ConcurrentPriorityQueue(Func<T, IComparable> prioritySelector)
		{
			_prioritySelector = prioritySelector ?? throw new ArgumentNullException(nameof(prioritySelector));
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="prioritySelector"></param>
		/// <param name="collection"></param>
		public ConcurrentPriorityQueue(Func<T, IComparable> prioritySelector, IEnumerable<T> collection)
			: this(prioritySelector)
		{
			foreach (var item in collection)
			{
				Enqueue(item);
			}
		}
		#endregion

		#region 实现 ConcurrentQueue 的核心接口
		/// <summary>
		/// 入队
		/// </summary>
		/// <param name="item"></param>
		public void Enqueue(T item)
		{
			lock (_syncRoot)
			{
				_heap.Add(item);
				HeapifyUp(_heap.Count - 1);
			}
		}
		/// <summary>
		/// 尝试入队
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		public bool TryDequeue(out T? result)
		{
			lock (_syncRoot)
			{
				if (_heap.Count == 0)
				{
					result = default;
					return false;
				}

				result = _heap[0];
				Swap(0, _heap.Count - 1);
				_heap.RemoveAt(_heap.Count - 1);
				HeapifyDown(0);
				return true;
			}
		}
		/// <summary>
		/// 取头部
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		public bool TryPeek(out T? result)
		{
			lock (_syncRoot)
			{
				if (_heap.Count == 0)
				{
					result = default;
					return false;
				}

				result = _heap[0];
				return true;
			}
		}
		/// <summary>
		/// 清空
		/// </summary>
		public void Clear()
		{
			lock (_syncRoot)
			{
				_heap.Clear();
			}
		}
		/// <summary>
		/// 是否为空
		/// </summary>
		public bool IsEmpty => Count == 0;
		#endregion

		#region 实现 IProducerConsumerCollection<T>
		/// <summary>
		/// 添加
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool TryAdd(T item)
		{
			Enqueue(item);
			return true;
		}
		/// <summary>
		/// 尝试出队
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool TryTake(out T item) => TryDequeue(out item);
		/// <summary>
		/// 复制到新数组
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public void CopyTo(T[] array, int index)
		{
			lock (_syncRoot)
			{
				var sorted = SortedClone();
				Array.Copy(sorted.ToArray(), 0, array, index, sorted.Count);
			}
		}
		/// <summary>
		/// 转数组
		/// </summary>
		/// <returns></returns>
		public T[] ToArray()
		{
			lock (_syncRoot)
			{
				return SortedClone().ToArray();
			}
		}
		/// <summary>
		/// 获取迭代器
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			List<T> heapClone;
			lock (_syncRoot)
			{
				heapClone = new List<T>(_heap);
			}
			return SortedClone(heapClone).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		#endregion

		#region 实现 ICollection
		/// <summary>
		/// 个数
		/// </summary>
		public int Count
		{
			get
			{
				lock (_syncRoot)
				{
					return _heap.Count;
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public object SyncRoot => _syncRoot;
		/// <summary>
		/// 
		/// </summary>
		public bool IsSynchronized => true;
		/// <summary>
		/// 复制
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		/// <exception cref="ArgumentException"></exception>
		void ICollection.CopyTo(Array array, int index)
		{
			if (array is T[] tArray)
			{
				CopyTo(tArray, index);
			}
			else
			{
				throw new ArgumentException("Invalid array type");
			}
		}
		#endregion

		#region 堆排序核心算法
		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		private void HeapifyUp(int index)
		{
			while (index > 0)
			{
				int parent = (index - 1) / 2;
				if (Compare(index, parent) >= 0) break;
				Swap(index, parent);
				index = parent;
			}
		}

		private void HeapifyDown(int index)
		{
			int lastIndex = _heap.Count - 1;
			while (true)
			{
				int leftChild = 2 * index + 1;
				int rightChild = 2 * index + 2;
				int smallest = index;

				if (leftChild <= lastIndex && Compare(leftChild, smallest) < 0)
					smallest = leftChild;

				if (rightChild <= lastIndex && Compare(rightChild, smallest) < 0)
					smallest = rightChild;

				if (smallest == index) break;

				Swap(index, smallest);
				index = smallest;
			}
		}

		private int Compare(int i, int j)
		{
			return _prioritySelector(_heap[i]).CompareTo(_prioritySelector(_heap[j]));
		}

		private void Swap(int i, int j)
		{
			T temp = _heap[i];
			_heap[i] = _heap[j];
			_heap[j] = temp;
		}
		#endregion

		#region 辅助方法
		private List<T> SortedClone(IEnumerable<T>? source = default)
		{
			var tempHeap = new List<T>(source ?? _heap);
			tempHeap.Sort((x, y) => _prioritySelector(x).CompareTo(_prioritySelector(y)));
			return tempHeap;
		}
		#endregion

		#region 扩展方法 (与 ConcurrentQueue 保持兼容)
		/// <summary>
		/// 添加
		/// </summary>
		/// <param name="items"></param>
		public void EnqueueRange(IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				Enqueue(item);
			}
		}
		/// <summary>
		/// 全部出队
		/// </summary>
		/// <returns></returns>
		public List<T> DequeueAll()
		{
			lock (_syncRoot)
			{
				var result = SortedClone();
				Clear();
				return result;
			}
		}
		#endregion
	}
}