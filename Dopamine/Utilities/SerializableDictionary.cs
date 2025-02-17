namespace Dopamine;
/// <summary>
/// 基于双链表实现的可序列化字典
/// </summary>
public class SerializableDictionary<TKey,TValue>:ConcurrentDictionary<TKey,TValue>
{
	private readonly ConcurrentBag<TKey> _keys = new ConcurrentBag<TKey>();
	private readonly ConcurrentBag<TValue> _values = new ConcurrentBag<TValue>();


}
