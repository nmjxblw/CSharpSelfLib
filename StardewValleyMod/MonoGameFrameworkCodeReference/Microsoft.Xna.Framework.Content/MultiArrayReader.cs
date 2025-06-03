using System;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content;

internal class MultiArrayReader<T> : ContentTypeReader<Array>
{
	private ContentTypeReader elementReader;

	protected internal override void Initialize(ContentTypeReaderManager manager)
	{
		Type readerType = typeof(T);
		this.elementReader = manager.GetTypeReader(readerType);
	}

	protected internal override Array Read(ContentReader input, Array existingInstance)
	{
		int rank = input.ReadInt32();
		if (rank < 1)
		{
			throw new RankException();
		}
		int[] dimensions = new int[rank];
		int count = 1;
		for (int d = 0; d < dimensions.Length; d++)
		{
			count *= (dimensions[d] = input.ReadInt32());
		}
		Array array = existingInstance;
		if (array == null)
		{
			array = Array.CreateInstance(typeof(T), dimensions);
		}
		else if (dimensions.Length != array.Rank)
		{
			throw new RankException("existingInstance");
		}
		int[] indices = new int[rank];
		for (int i = 0; i < count; i++)
		{
			T value;
			if (ReflectionHelpers.IsValueType(typeof(T)))
			{
				value = input.ReadObject<T>(this.elementReader);
			}
			else
			{
				int readerType = input.Read7BitEncodedInt();
				value = ((readerType <= 0) ? default(T) : input.ReadObject<T>(input.TypeReaders[readerType - 1]));
			}
			MultiArrayReader<T>.CalcIndices(array, i, indices);
			array.SetValue(value, indices);
		}
		return array;
	}

	private static void CalcIndices(Array array, int index, int[] indices)
	{
		if (array.Rank != indices.Length)
		{
			throw new Exception("indices");
		}
		for (int d = 0; d < indices.Length; d++)
		{
			if (index == 0)
			{
				indices[d] = 0;
				continue;
			}
			indices[d] = index % array.GetLength(d);
			index /= array.GetLength(d);
		}
		if (index != 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
	}
}
