using System.IO;

namespace NVorbis;

internal abstract class VorbisMapping
{
	private class Mapping0 : VorbisMapping
	{
		internal Mapping0(VorbisStreamDecoder vorbis)
			: base(vorbis)
		{
		}

		protected override void Init(DataPacket packet)
		{
			int submapCount = 1;
			if (packet.ReadBit())
			{
				submapCount += (int)packet.ReadBits(4);
			}
			int couplingSteps = 0;
			if (packet.ReadBit())
			{
				couplingSteps = (int)packet.ReadBits(8) + 1;
			}
			int couplingBits = Utils.ilog(base._vorbis._channels - 1);
			base.CouplingSteps = new CouplingStep[couplingSteps];
			for (int j = 0; j < couplingSteps; j++)
			{
				int magnitude = (int)packet.ReadBits(couplingBits);
				int angle = (int)packet.ReadBits(couplingBits);
				if (magnitude == angle || magnitude > base._vorbis._channels - 1 || angle > base._vorbis._channels - 1)
				{
					throw new InvalidDataException();
				}
				base.CouplingSteps[j] = new CouplingStep
				{
					Angle = angle,
					Magnitude = magnitude
				};
			}
			if (packet.ReadBits(2) != 0L)
			{
				throw new InvalidDataException();
			}
			int[] mux = new int[base._vorbis._channels];
			if (submapCount > 1)
			{
				for (int c = 0; c < base.ChannelSubmap.Length; c++)
				{
					mux[c] = (int)packet.ReadBits(4);
					if (mux[c] >= submapCount)
					{
						throw new InvalidDataException();
					}
				}
			}
			base.Submaps = new Submap[submapCount];
			for (int i = 0; i < submapCount; i++)
			{
				packet.ReadBits(8);
				int floorNum = (int)packet.ReadBits(8);
				if (floorNum >= base._vorbis.Floors.Length)
				{
					throw new InvalidDataException();
				}
				if ((int)packet.ReadBits(8) >= base._vorbis.Residues.Length)
				{
					throw new InvalidDataException();
				}
				base.Submaps[i] = new Submap
				{
					Floor = base._vorbis.Floors[floorNum],
					Residue = base._vorbis.Residues[floorNum]
				};
			}
			base.ChannelSubmap = new Submap[base._vorbis._channels];
			for (int k = 0; k < base.ChannelSubmap.Length; k++)
			{
				base.ChannelSubmap[k] = base.Submaps[mux[k]];
			}
		}
	}

	internal class Submap
	{
		internal VorbisFloor Floor;

		internal VorbisResidue Residue;

		internal Submap()
		{
		}
	}

	internal class CouplingStep
	{
		internal int Magnitude;

		internal int Angle;

		internal CouplingStep()
		{
		}
	}

	private VorbisStreamDecoder _vorbis;

	internal Submap[] Submaps;

	internal Submap[] ChannelSubmap;

	internal CouplingStep[] CouplingSteps;

	internal static VorbisMapping Init(VorbisStreamDecoder vorbis, DataPacket packet)
	{
		int num = (int)packet.ReadBits(16);
		VorbisMapping mapping = null;
		if (num == 0)
		{
			mapping = new Mapping0(vorbis);
		}
		if (mapping == null)
		{
			throw new InvalidDataException();
		}
		mapping.Init(packet);
		return mapping;
	}

	protected VorbisMapping(VorbisStreamDecoder vorbis)
	{
		this._vorbis = vorbis;
	}

	protected abstract void Init(DataPacket packet);
}
