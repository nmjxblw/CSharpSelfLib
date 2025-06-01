using System;

namespace Microsoft.Xna.Framework.Content;

internal class EnumReader<T> : ContentTypeReader<T>
{
	private ContentTypeReader elementReader;

	protected internal override void Initialize(ContentTypeReaderManager manager)
	{
		Type readerType = Enum.GetUnderlyingType(typeof(T));
		this.elementReader = manager.GetTypeReader(readerType);
	}

	protected internal override T Read(ContentReader input, T existingInstance)
	{
		return input.ReadRawObject<T>(this.elementReader);
	}
}
