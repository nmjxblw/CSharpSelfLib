using System.Xml.Serialization;

namespace StardewValley.Util;

/// <summary>Stores the key/value pairs of a dictionary in an easily serializable way.</summary>
public struct SaveablePair<TKey, TValue>
{
	/// <summary>An 1-length array that stores the dictionary entry key.</summary>
	public TKey[] key;

	/// <summary>An 1-length array that stores the dictionary entry value.</summary>
	public TValue[] value;

	[XmlIgnore]
	public TKey Key => this.key[0];

	[XmlIgnore]
	public TValue Value => this.value[0];

	/// <summary>Constructs a key/value pair entry.</summary>
	/// <param name="key">The dictionary entry key.</param>
	/// <param name="value">The dictionary entry value.</param>
	public SaveablePair(TKey key, TValue value)
	{
		this.key = new TKey[1] { key };
		this.value = new TValue[1] { value };
	}
}
