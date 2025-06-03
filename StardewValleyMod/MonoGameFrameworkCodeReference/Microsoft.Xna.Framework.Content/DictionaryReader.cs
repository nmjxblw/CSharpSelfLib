using System;
using System.Collections.Generic;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content;

internal class DictionaryReader<TKey, TValue> : ContentTypeReader<Dictionary<TKey, TValue>>
{
	private ContentTypeReader keyReader;

	private ContentTypeReader valueReader;

	private Type keyType;

	private Type valueType;

	public override bool CanDeserializeIntoExistingObject => true;

	protected internal override void Initialize(ContentTypeReaderManager manager)
	{
		this.keyType = typeof(TKey);
		this.valueType = typeof(TValue);
		this.keyReader = manager.GetTypeReader(this.keyType);
		this.valueReader = manager.GetTypeReader(this.valueType);
	}

	protected internal override Dictionary<TKey, TValue> Read(ContentReader input, Dictionary<TKey, TValue> existingInstance)
	{
		int count = input.ReadInt32();
		Dictionary<TKey, TValue> dictionary = existingInstance;
		if (dictionary == null)
		{
			dictionary = new Dictionary<TKey, TValue>(count);
		}
		else
		{
			dictionary.Clear();
		}
		for (int i = 0; i < count; i++)
		{
			TKey key;
			if (ReflectionHelpers.IsValueType(this.keyType))
			{
				key = input.ReadObject<TKey>(this.keyReader);
			}
			else
			{
				int readerType = input.Read7BitEncodedInt();
				key = ((readerType > 0) ? input.ReadObject<TKey>(input.TypeReaders[readerType - 1]) : default(TKey));
			}
			TValue value;
			if (ReflectionHelpers.IsValueType(this.valueType))
			{
				value = input.ReadObject<TValue>(this.valueReader);
			}
			else
			{
				int readerType2 = input.Read7BitEncodedInt();
				value = ((readerType2 > 0) ? input.ReadObject<TValue>(input.TypeReaders[readerType2 - 1]) : default(TValue));
			}
			dictionary.Add(key, value);
		}
		return dictionary;
	}
}
