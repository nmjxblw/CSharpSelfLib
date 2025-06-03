using System;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content;

internal class ArrayReader<T> : ContentTypeReader<T[]>
{
	private ContentTypeReader elementReader;

	protected internal override void Initialize(ContentTypeReaderManager manager)
	{
		Type readerType = typeof(T);
		this.elementReader = manager.GetTypeReader(readerType);
	}

	protected internal override T[] Read(ContentReader input, T[] existingInstance)
	{
		uint count = input.ReadUInt32();
		T[] array = existingInstance;
		if (array == null)
		{
			array = new T[count];
		}
		if (ReflectionHelpers.IsValueType(typeof(T)))
		{
			for (uint i = 0u; i < count; i++)
			{
				array[i] = input.ReadObject<T>(this.elementReader);
			}
		}
		else
		{
			for (uint i2 = 0u; i2 < count; i2++)
			{
				int readerType = input.Read7BitEncodedInt();
				array[i2] = ((readerType > 0) ? input.ReadObject<T>(input.TypeReaders[readerType - 1]) : default(T));
			}
		}
		return array;
	}
}
