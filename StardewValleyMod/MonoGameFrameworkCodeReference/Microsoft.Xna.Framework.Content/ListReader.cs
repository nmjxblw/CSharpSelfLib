using System;
using System.Collections.Generic;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content;

internal class ListReader<T> : ContentTypeReader<List<T>>
{
	private ContentTypeReader elementReader;

	public override bool CanDeserializeIntoExistingObject => true;

	protected internal override void Initialize(ContentTypeReaderManager manager)
	{
		Type readerType = typeof(T);
		this.elementReader = manager.GetTypeReader(readerType);
	}

	protected internal override List<T> Read(ContentReader input, List<T> existingInstance)
	{
		int count = input.ReadInt32();
		List<T> list = existingInstance;
		if (list == null)
		{
			list = new List<T>(count);
		}
		for (int i = 0; i < count; i++)
		{
			if (ReflectionHelpers.IsValueType(typeof(T)))
			{
				list.Add(input.ReadObject<T>(this.elementReader));
				continue;
			}
			int readerType = input.Read7BitEncodedInt();
			list.Add((readerType > 0) ? input.ReadObject<T>(input.TypeReaders[readerType - 1]) : default(T));
		}
		return list;
	}
}
