namespace NVorbis.Ogg;

internal class Crc
{
	private const uint CRC32_POLY = 79764919u;

	private static uint[] crcTable;

	private uint _crc;

	static Crc()
	{
		Crc.crcTable = new uint[256];
		for (uint i = 0u; i < 256; i++)
		{
			uint s = i << 24;
			for (int j = 0; j < 8; j++)
			{
				s = (s << 1) ^ (uint)((s >= 2147483648u) ? 79764919 : 0);
			}
			Crc.crcTable[i] = s;
		}
	}

	public Crc()
	{
		this.Reset();
	}

	public void Reset()
	{
		this._crc = 0u;
	}

	public void Update(int nextVal)
	{
		this._crc = (this._crc << 8) ^ Crc.crcTable[nextVal ^ (this._crc >> 24)];
	}

	public bool Test(uint checkCrc)
	{
		return this._crc == checkCrc;
	}
}
