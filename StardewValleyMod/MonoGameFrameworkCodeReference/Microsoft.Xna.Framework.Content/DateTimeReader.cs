using System;

namespace Microsoft.Xna.Framework.Content;

internal class DateTimeReader : ContentTypeReader<DateTime>
{
	protected internal override DateTime Read(ContentReader input, DateTime existingInstance)
	{
		ulong num = input.ReadUInt64();
		ulong mask = 13835058055282163712uL;
		long ticks = (long)(num & ~mask);
		DateTimeKind kind = (DateTimeKind)((num >> 62) & 3);
		return new DateTime(ticks, kind);
	}
}
