using System;

namespace Microsoft.Xna.Framework.Content;

internal class TimeSpanReader : ContentTypeReader<TimeSpan>
{
	protected internal override TimeSpan Read(ContentReader input, TimeSpan existingInstance)
	{
		return new TimeSpan(input.ReadInt64());
	}
}
