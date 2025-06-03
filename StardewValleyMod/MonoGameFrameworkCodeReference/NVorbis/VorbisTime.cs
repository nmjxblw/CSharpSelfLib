using System.IO;

namespace NVorbis;

internal abstract class VorbisTime
{
	private class Time0 : VorbisTime
	{
		internal Time0(VorbisStreamDecoder vorbis)
			: base(vorbis)
		{
		}

		protected override void Init(DataPacket packet)
		{
		}
	}

	private VorbisStreamDecoder _vorbis;

	internal static VorbisTime Init(VorbisStreamDecoder vorbis, DataPacket packet)
	{
		int num = (int)packet.ReadBits(16);
		VorbisTime time = null;
		if (num == 0)
		{
			time = new Time0(vorbis);
		}
		if (time == null)
		{
			throw new InvalidDataException();
		}
		time.Init(packet);
		return time;
	}

	protected VorbisTime(VorbisStreamDecoder vorbis)
	{
		this._vorbis = vorbis;
	}

	protected abstract void Init(DataPacket packet);
}
