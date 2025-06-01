using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Netcode;

public abstract class SerializationFacade<SerialT> : IEnumerable<SerialT>, IEnumerable
{
	protected abstract SerialT Serialize();

	protected abstract void Deserialize(SerialT serialValue);

	public IEnumerator<SerialT> GetEnumerator()
	{
		return Enumerable.Repeat(this.Serialize(), 1).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	public void Add(SerialT value)
	{
		this.Deserialize(value);
	}
}
