using System.Collections;
using System.Collections.Generic;

namespace Netcode;

public abstract class SerializationCollectionFacade<SerialT> : IEnumerable<SerialT>, IEnumerable
{
	protected abstract List<SerialT> Serialize();

	protected abstract void DeserializeAdd(SerialT serialElem);

	public IEnumerator<SerialT> GetEnumerator()
	{
		return this.Serialize().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	public void Add(SerialT value)
	{
		this.DeserializeAdd(value);
	}
}
