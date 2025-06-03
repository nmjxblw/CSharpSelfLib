using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using StardewValley.SaveSerialization;

namespace StardewValley;

/// <summary>A dictionary that can be read and written in the save XML.</summary>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
[XmlRoot("dictionary")]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
{
	public struct ChangeArgs
	{
		public readonly ChangeType Type;

		public readonly TKey Key;

		public readonly TValue Value;

		public ChangeArgs(ChangeType type, TKey k, TValue v)
		{
			this.Type = type;
			this.Key = k;
			this.Value = v;
		}
	}

	public delegate void ChangeCallback(object sender, ChangeArgs args);

	private static XmlSerializer _keySerializer;

	private static XmlSerializer _valueSerializer;

	public event ChangeCallback CollectionChanged;

	static SerializableDictionary()
	{
		SerializableDictionary<TKey, TValue>._keySerializer = SaveSerializer.GetSerializer(typeof(TKey));
		SerializableDictionary<TKey, TValue>._valueSerializer = SaveSerializer.GetSerializer(typeof(TValue));
	}

	/// <summary>Construct an empty instance.</summary>
	public SerializableDictionary()
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="data">The data to copy.</param>
	public SerializableDictionary(IDictionary<TKey, TValue> data)
		: base(data)
	{
	}

	/// <summary>Create an instance from a dictionary with a different value type.</summary>
	/// <typeparam name="TSourceValue">The value type in the source data to copy.</typeparam>
	/// <param name="data">The data to copy.</param>
	/// <param name="getValue">Get the value to use for an entry in the original data.</param>
	public static SerializableDictionary<TKey, TValue> BuildFrom<TSourceValue>(IDictionary<TKey, TSourceValue> data, Func<TSourceValue, TValue> getValue)
	{
		SerializableDictionary<TKey, TValue> result = new SerializableDictionary<TKey, TValue>();
		foreach (KeyValuePair<TKey, TSourceValue> entry in data)
		{
			result[entry.Key] = getValue(entry.Value);
		}
		return result;
	}

	/// <summary>Create an instance from a dictionary with different key and value types.</summary>
	/// <typeparam name="TSourceKey">The key type in the source data to copy.</typeparam>
	/// <typeparam name="TSourceValue">The value type in the source data to copy.</typeparam>
	/// <param name="data">The data to copy.</param>
	/// <param name="getKey">Get the key to use for an entry in the original data.</param>
	/// <param name="getValue">Get the value to use for an entry in the original data.</param>
	public static SerializableDictionary<TKey, TValue> BuildFrom<TSourceKey, TSourceValue>(IDictionary<TSourceKey, TSourceValue> data, Func<TSourceKey, TKey> getKey, Func<TSourceValue, TValue> getValue)
	{
		SerializableDictionary<TKey, TValue> result = new SerializableDictionary<TKey, TValue>();
		foreach (KeyValuePair<TSourceKey, TSourceValue> entry in data)
		{
			result[getKey(entry.Key)] = getValue(entry.Value);
		}
		return result;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="comparer">The equality comparer to use when comparing keys, or null to use the default comparer for the key type.</param>
	protected SerializableDictionary(IEqualityComparer<TKey> comparer = null)
		: base(comparer)
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="data">The data to copy.</param>
	/// <param name="comparer">The equality comparer to use when comparing keys, or null to use the default comparer for the key type.</param>
	protected SerializableDictionary(IDictionary<TKey, TValue> data, IEqualityComparer<TKey> comparer = null)
		: base(data, comparer)
	{
	}

	public new void Add(TKey key, TValue value)
	{
		base.Add(key, value);
		this.OnCollectionChanged(this, new ChangeArgs(ChangeType.Add, key, value));
	}

	public new bool Remove(TKey key)
	{
		if (base.TryGetValue(key, out var val))
		{
			base.Remove(key);
			this.OnCollectionChanged(this, new ChangeArgs(ChangeType.Remove, key, val));
			return true;
		}
		return false;
	}

	public new void Clear()
	{
		base.Clear();
		this.OnCollectionChanged(this, new ChangeArgs(ChangeType.Clear, default(TKey), default(TValue)));
	}

	private void OnCollectionChanged(object sender, ChangeArgs args)
	{
		this.CollectionChanged?.Invoke(sender ?? this, args);
	}

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		bool isEmptyElement = reader.IsEmptyElement;
		reader.Read();
		if (isEmptyElement)
		{
			return;
		}
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			reader.ReadStartElement("item");
			reader.ReadStartElement("key");
			bool read = false;
			TKey key = default(TKey);
			if (typeof(TKey) == typeof(string))
			{
				string name = reader.Name;
				if (!(name == "int"))
				{
					if (name == "LocationContext")
					{
						reader.ReadStartElement();
						key = (TKey)Convert.ChangeType(reader.ReadContentAsString(), typeof(TKey));
						reader.ReadEndElement();
						read = true;
					}
				}
				else
				{
					key = (TKey)Convert.ChangeType(SaveSerializer.Deserialize<int>(reader), typeof(TKey));
					read = true;
				}
			}
			if (!read)
			{
				key = (TKey)SerializableDictionary<TKey, TValue>._keySerializer.DeserializeFast(reader);
			}
			reader.ReadEndElement();
			reader.ReadStartElement("value");
			TValue value = default(TValue);
			read = false;
			if (typeof(TValue) == typeof(string) && reader.Name == "int")
			{
				value = (TValue)Convert.ChangeType(SaveSerializer.Deserialize<int>(reader), typeof(TValue));
				read = true;
			}
			if (!read)
			{
				value = (TValue)SerializableDictionary<TKey, TValue>._valueSerializer.DeserializeFast(reader);
			}
			reader.ReadEndElement();
			this.AddDuringDeserialization(key, value);
			reader.ReadEndElement();
			reader.MoveToContent();
		}
		reader.ReadEndElement();
	}

	public void WriteXml(XmlWriter writer)
	{
		foreach (TKey key in base.Keys)
		{
			writer.WriteStartElement("item");
			writer.WriteStartElement("key");
			SerializableDictionary<TKey, TValue>._keySerializer.SerializeFast(writer, key);
			writer.WriteEndElement();
			writer.WriteStartElement("value");
			TValue value = base[key];
			SerializableDictionary<TKey, TValue>._valueSerializer.SerializeFast(writer, value);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}

	/// <summary>Add a pair read from the raw XML during deserialization.</summary>
	/// <param name="key">The key to add.</param>
	/// <param name="value">The value to add.</param>
	protected virtual void AddDuringDeserialization(TKey key, TValue value)
	{
		base.Add(key, value);
	}
}
