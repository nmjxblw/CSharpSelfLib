using System;

namespace Microsoft.Xna.Framework.Content;

internal class NullableReader<T> : ContentTypeReader<T?> where T : struct
{
	private ContentTypeReader elementReader;

	protected internal override void Initialize(ContentTypeReaderManager manager)
	{
		Type readerType = typeof(T);
		this.elementReader = manager.GetTypeReader(readerType);
	}

	protected internal override T? Read(ContentReader input, T? existingInstance)
	{
		if (input.ReadBoolean())
		{
			return input.ReadObject<T>(this.elementReader);
		}
		return null;
	}
}
