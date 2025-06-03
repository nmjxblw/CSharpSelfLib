using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace NVorbis;

internal class VorbisStreamDecoder : IVorbisStreamStatus, IDisposable
{
	internal int _upperBitrate;

	internal int _nominalBitrate;

	internal int _lowerBitrate;

	internal string _vendor;

	internal string[] _comments;

	internal int _channels;

	internal int _sampleRate;

	internal int Block0Size;

	internal int Block1Size;

	internal VorbisCodebook[] Books;

	internal VorbisTime[] Times;

	internal VorbisFloor[] Floors;

	internal VorbisResidue[] Residues;

	internal VorbisMapping[] Maps;

	internal VorbisMode[] Modes;

	private int _modeFieldBits;

	internal long _glueBits;

	internal long _metaBits;

	internal long _bookBits;

	internal long _timeHdrBits;

	internal long _floorHdrBits;

	internal long _resHdrBits;

	internal long _mapHdrBits;

	internal long _modeHdrBits;

	internal long _wasteHdrBits;

	internal long _modeBits;

	internal long _floorBits;

	internal long _resBits;

	internal long _wasteBits;

	internal long _samples;

	internal int _packetCount;

	internal Stopwatch _sw = new Stopwatch();

	private IPacketProvider _packetProvider;

	private DataPacket _parameterChangePacket;

	private List<int> _pagesSeen;

	private int _lastPageSeen;

	private bool _eosFound;

	private object _seekLock = new object();

	private float[] _prevBuffer;

	private RingBuffer _outputBuffer;

	private Queue<int> _bitsPerPacketHistory;

	private Queue<int> _sampleCountHistory;

	private int _preparedLength;

	internal bool _clipped;

	private Stack<DataPacket> _resyncQueue;

	private long _currentPosition;

	private long _reportedPosition;

	private VorbisMode _mode;

	private bool _prevFlag;

	private bool _nextFlag;

	private bool[] _noExecuteChannel;

	private VorbisFloor.PacketData[] _floorData;

	private float[][] _residue;

	private bool _isParameterChange;

	internal bool IsParameterChange
	{
		get
		{
			return this._isParameterChange;
		}
		set
		{
			if (value)
			{
				throw new InvalidOperationException("Only clearing is supported!");
			}
			this._isParameterChange = value;
		}
	}

	internal bool CanSeek => this._packetProvider.CanSeek;

	internal long CurrentPosition
	{
		get
		{
			return this._reportedPosition;
		}
		private set
		{
			this._reportedPosition = value;
			this._currentPosition = value;
			this._preparedLength = 0;
			this._eosFound = false;
			this.ResetDecoder(isFullReset: false);
			this._prevBuffer = null;
		}
	}

	internal long ContainerBits => this._packetProvider.ContainerBits;

	public int EffectiveBitRate
	{
		get
		{
			if (this._samples == 0L)
			{
				return 0;
			}
			double decodedSeconds = (double)(this._currentPosition - this._preparedLength) / (double)this._sampleRate;
			return (int)((double)this.AudioBits / decodedSeconds);
		}
	}

	public int InstantBitRate
	{
		get
		{
			int samples = this._sampleCountHistory.Sum();
			if (samples > 0)
			{
				return (int)((long)this._bitsPerPacketHistory.Sum() * (long)this._sampleRate / samples);
			}
			return -1;
		}
	}

	public TimeSpan PageLatency => TimeSpan.FromTicks(this._sw.ElapsedTicks / this.PagesRead);

	public TimeSpan PacketLatency => TimeSpan.FromTicks(this._sw.ElapsedTicks / this._packetCount);

	public TimeSpan SecondLatency => TimeSpan.FromTicks(this._sw.ElapsedTicks / this._samples * this._sampleRate);

	public long OverheadBits => this._glueBits + this._metaBits + this._timeHdrBits + this._wasteHdrBits + this._wasteBits + this._packetProvider.ContainerBits;

	public long AudioBits => this._bookBits + this._floorHdrBits + this._resHdrBits + this._mapHdrBits + this._modeHdrBits + this._modeBits + this._floorBits + this._resBits;

	public int PagesRead => this._pagesSeen.IndexOf(this._lastPageSeen) + 1;

	public int TotalPages => this._packetProvider.GetTotalPageCount();

	public bool Clipped => this._clipped;

	internal VorbisStreamDecoder(IPacketProvider packetProvider)
	{
		this._packetProvider = packetProvider;
		this._packetProvider.ParameterChange += SetParametersChanging;
		this._pagesSeen = new List<int>();
		this._lastPageSeen = -1;
	}

	internal bool TryInit()
	{
		if (!this.ProcessStreamHeader(this._packetProvider.PeekNextPacket()))
		{
			return false;
		}
		this._packetProvider.GetNextPacket().Done();
		DataPacket packet = this._packetProvider.GetNextPacket();
		if (!this.LoadComments(packet))
		{
			throw new InvalidDataException("Comment header was not readable!");
		}
		packet.Done();
		packet = this._packetProvider.GetNextPacket();
		if (!this.LoadBooks(packet))
		{
			throw new InvalidDataException("Book header was not readable!");
		}
		packet.Done();
		this.InitDecoder();
		return true;
	}

	private void SetParametersChanging(object sender, ParameterChangeEventArgs e)
	{
		this._parameterChangePacket = e.FirstPacket;
	}

	public void Dispose()
	{
		if (this._packetProvider != null)
		{
			IPacketProvider packetProvider = this._packetProvider;
			this._packetProvider = null;
			packetProvider.ParameterChange -= SetParametersChanging;
			packetProvider.Dispose();
		}
	}

	private void ProcessParameterChange(DataPacket packet)
	{
		this._parameterChangePacket = null;
		bool wasPeek = false;
		bool doFullReset = false;
		if (this.ProcessStreamHeader(packet))
		{
			packet.Done();
			wasPeek = true;
			doFullReset = true;
			packet = this._packetProvider.PeekNextPacket();
			if (packet == null)
			{
				throw new InvalidDataException("Couldn't get next packet!");
			}
		}
		if (this.LoadComments(packet))
		{
			if (wasPeek)
			{
				this._packetProvider.GetNextPacket().Done();
			}
			else
			{
				packet.Done();
			}
			wasPeek = true;
			packet = this._packetProvider.PeekNextPacket();
			if (packet == null)
			{
				throw new InvalidDataException("Couldn't get next packet!");
			}
		}
		if (this.LoadBooks(packet))
		{
			if (wasPeek)
			{
				this._packetProvider.GetNextPacket().Done();
			}
			else
			{
				packet.Done();
			}
		}
		this.ResetDecoder(doFullReset);
	}

	private bool ProcessStreamHeader(DataPacket packet)
	{
		if (!packet.ReadBytes(7).SequenceEqual(new byte[7] { 1, 118, 111, 114, 98, 105, 115 }))
		{
			this._glueBits += packet.Length * 8;
			return false;
		}
		if (!this._pagesSeen.Contains(this._lastPageSeen = packet.PageSequenceNumber))
		{
			this._pagesSeen.Add(this._lastPageSeen);
		}
		this._glueBits += 56L;
		long startPos = packet.BitsRead;
		if (packet.ReadInt32() != 0)
		{
			throw new InvalidDataException("Only Vorbis stream version 0 is supported.");
		}
		this._channels = packet.ReadByte();
		this._sampleRate = packet.ReadInt32();
		this._upperBitrate = packet.ReadInt32();
		this._nominalBitrate = packet.ReadInt32();
		this._lowerBitrate = packet.ReadInt32();
		this.Block0Size = 1 << (int)packet.ReadBits(4);
		this.Block1Size = 1 << (int)packet.ReadBits(4);
		if (this._nominalBitrate == 0 && this._upperBitrate > 0 && this._lowerBitrate > 0)
		{
			this._nominalBitrate = (this._upperBitrate + this._lowerBitrate) / 2;
		}
		this._metaBits += packet.BitsRead - startPos + 8;
		this._wasteHdrBits += 8 * packet.Length - packet.BitsRead;
		return true;
	}

	private bool LoadComments(DataPacket packet)
	{
		if (!packet.ReadBytes(7).SequenceEqual(new byte[7] { 3, 118, 111, 114, 98, 105, 115 }))
		{
			return false;
		}
		if (!this._pagesSeen.Contains(this._lastPageSeen = packet.PageSequenceNumber))
		{
			this._pagesSeen.Add(this._lastPageSeen);
		}
		this._glueBits += 56L;
		this._vendor = Encoding.UTF8.GetString(packet.ReadBytes(packet.ReadInt32()));
		this._comments = new string[packet.ReadInt32()];
		for (int i = 0; i < this._comments.Length; i++)
		{
			this._comments[i] = Encoding.UTF8.GetString(packet.ReadBytes(packet.ReadInt32()));
		}
		this._metaBits += packet.BitsRead - 56;
		this._wasteHdrBits += 8 * packet.Length - packet.BitsRead;
		return true;
	}

	private bool LoadBooks(DataPacket packet)
	{
		if (!packet.ReadBytes(7).SequenceEqual(new byte[7] { 5, 118, 111, 114, 98, 105, 115 }))
		{
			return false;
		}
		if (!this._pagesSeen.Contains(this._lastPageSeen = packet.PageSequenceNumber))
		{
			this._pagesSeen.Add(this._lastPageSeen);
		}
		long bits = packet.BitsRead;
		this._glueBits += packet.BitsRead;
		this.Books = new VorbisCodebook[packet.ReadByte() + 1];
		for (int i = 0; i < this.Books.Length; i++)
		{
			this.Books[i] = VorbisCodebook.Init(this, packet, i);
		}
		this._bookBits += packet.BitsRead - bits;
		bits = packet.BitsRead;
		this.Times = new VorbisTime[(int)packet.ReadBits(6) + 1];
		for (int j = 0; j < this.Times.Length; j++)
		{
			this.Times[j] = VorbisTime.Init(this, packet);
		}
		this._timeHdrBits += packet.BitsRead - bits;
		bits = packet.BitsRead;
		this.Floors = new VorbisFloor[(int)packet.ReadBits(6) + 1];
		for (int k = 0; k < this.Floors.Length; k++)
		{
			this.Floors[k] = VorbisFloor.Init(this, packet);
		}
		this._floorHdrBits += packet.BitsRead - bits;
		bits = packet.BitsRead;
		this.Residues = new VorbisResidue[(int)packet.ReadBits(6) + 1];
		for (int l = 0; l < this.Residues.Length; l++)
		{
			this.Residues[l] = VorbisResidue.Init(this, packet);
		}
		this._resHdrBits += packet.BitsRead - bits;
		bits = packet.BitsRead;
		this.Maps = new VorbisMapping[(int)packet.ReadBits(6) + 1];
		for (int m = 0; m < this.Maps.Length; m++)
		{
			this.Maps[m] = VorbisMapping.Init(this, packet);
		}
		this._mapHdrBits += packet.BitsRead - bits;
		bits = packet.BitsRead;
		this.Modes = new VorbisMode[(int)packet.ReadBits(6) + 1];
		for (int n = 0; n < this.Modes.Length; n++)
		{
			this.Modes[n] = VorbisMode.Init(this, packet);
		}
		this._modeHdrBits += packet.BitsRead - bits;
		if (!packet.ReadBit())
		{
			throw new InvalidDataException();
		}
		this._glueBits++;
		this._wasteHdrBits += 8 * packet.Length - packet.BitsRead;
		this._modeFieldBits = Utils.ilog(this.Modes.Length - 1);
		return true;
	}

	private void InitDecoder()
	{
		this._currentPosition = 0L;
		this._resyncQueue = new Stack<DataPacket>();
		this._bitsPerPacketHistory = new Queue<int>();
		this._sampleCountHistory = new Queue<int>();
		this.ResetDecoder(isFullReset: true);
	}

	private void ResetDecoder(bool isFullReset)
	{
		if (this._preparedLength > 0)
		{
			this.SaveBuffer();
		}
		if (isFullReset)
		{
			this._noExecuteChannel = new bool[this._channels];
			this._floorData = new VorbisFloor.PacketData[this._channels];
			this._residue = new float[this._channels][];
			for (int i = 0; i < this._channels; i++)
			{
				this._residue[i] = new float[this.Block1Size];
			}
			this._outputBuffer = new RingBuffer(this.Block1Size * 2 * this._channels);
			this._outputBuffer.Channels = this._channels;
		}
		else
		{
			this._outputBuffer.Clear();
		}
		this._preparedLength = 0;
	}

	private void SaveBuffer()
	{
		float[] buf = new float[this._preparedLength * this._channels];
		this.ReadSamples(buf, 0, buf.Length);
		this._prevBuffer = buf;
	}

	private bool UnpackPacket(DataPacket packet)
	{
		if (packet.ReadBit())
		{
			return false;
		}
		int modeBits = this._modeFieldBits;
		this._mode = this.Modes[(uint)packet.ReadBits(this._modeFieldBits)];
		if (this._mode.BlockFlag)
		{
			this._prevFlag = packet.ReadBit();
			this._nextFlag = packet.ReadBit();
			modeBits += 2;
		}
		else
		{
			this._prevFlag = (this._nextFlag = false);
		}
		if (packet.IsShort)
		{
			return false;
		}
		long startBits = packet.BitsRead;
		int halfBlockSize = this._mode.BlockSize / 2;
		for (int i = 0; i < this._channels; i++)
		{
			this._floorData[i] = this._mode.Mapping.ChannelSubmap[i].Floor.UnpackPacket(packet, this._mode.BlockSize, i);
			this._noExecuteChannel[i] = !this._floorData[i].ExecuteChannel;
			Array.Clear(this._residue[i], 0, halfBlockSize);
		}
		VorbisMapping.CouplingStep[] couplingSteps = this._mode.Mapping.CouplingSteps;
		foreach (VorbisMapping.CouplingStep step in couplingSteps)
		{
			if (this._floorData[step.Angle].ExecuteChannel || this._floorData[step.Magnitude].ExecuteChannel)
			{
				this._floorData[step.Angle].ForceEnergy = true;
				this._floorData[step.Magnitude].ForceEnergy = true;
			}
		}
		long floorBits = packet.BitsRead - startBits;
		startBits = packet.BitsRead;
		VorbisMapping.Submap[] submaps = this._mode.Mapping.Submaps;
		foreach (VorbisMapping.Submap subMap in submaps)
		{
			for (int k = 0; k < this._channels; k++)
			{
				if (this._mode.Mapping.ChannelSubmap[k] != subMap)
				{
					this._floorData[k].ForceNoEnergy = true;
				}
			}
			float[][] rTemp = subMap.Residue.Decode(packet, this._noExecuteChannel, this._channels, this._mode.BlockSize);
			for (int c = 0; c < this._channels; c++)
			{
				float[] r = this._residue[c];
				float[] rt = rTemp[c];
				for (int l = 0; l < halfBlockSize; l++)
				{
					r[l] += rt[l];
				}
			}
		}
		this._glueBits++;
		this._modeBits += modeBits;
		this._floorBits += floorBits;
		this._resBits += packet.BitsRead - startBits;
		this._wasteBits += 8 * packet.Length - packet.BitsRead;
		this._packetCount++;
		return true;
	}

	private void DecodePacket()
	{
		VorbisMapping.CouplingStep[] steps = this._mode.Mapping.CouplingSteps;
		int halfSizeW = this._mode.BlockSize / 2;
		for (int i = steps.Length - 1; i >= 0; i--)
		{
			if (this._floorData[steps[i].Angle].ExecuteChannel || this._floorData[steps[i].Magnitude].ExecuteChannel)
			{
				float[] magnitude = this._residue[steps[i].Magnitude];
				float[] angle = this._residue[steps[i].Angle];
				for (int j = 0; j < halfSizeW; j++)
				{
					float newM;
					float newA;
					if (magnitude[j] > 0f)
					{
						if (angle[j] > 0f)
						{
							newM = magnitude[j];
							newA = magnitude[j] - angle[j];
						}
						else
						{
							newA = magnitude[j];
							newM = magnitude[j] + angle[j];
						}
					}
					else if (angle[j] > 0f)
					{
						newM = magnitude[j];
						newA = magnitude[j] + angle[j];
					}
					else
					{
						newA = magnitude[j];
						newM = magnitude[j] - angle[j];
					}
					magnitude[j] = newM;
					angle[j] = newA;
				}
			}
		}
		for (int c = 0; c < this._channels; c++)
		{
			VorbisFloor.PacketData floorData = this._floorData[c];
			float[] res = this._residue[c];
			if (floorData.ExecuteChannel)
			{
				this._mode.Mapping.ChannelSubmap[c].Floor.Apply(floorData, res);
				Mdct.Reverse(res, this._mode.BlockSize);
			}
			else
			{
				Array.Clear(res, halfSizeW, halfSizeW);
			}
		}
	}

	private int OverlapSamples()
	{
		float[] window = this._mode.GetWindow(this._prevFlag, this._nextFlag);
		int sizeW = this._mode.BlockSize;
		int right = sizeW;
		int center = right >> 1;
		int left = 0;
		int begin = -center;
		int end = center;
		if (this._mode.BlockFlag)
		{
			if (!this._prevFlag)
			{
				left = this.Block1Size / 4 - this.Block0Size / 4;
				center = left + this.Block0Size / 2;
				begin = this.Block0Size / -2 - left;
			}
			if (!this._nextFlag)
			{
				right -= sizeW / 4 - this.Block0Size / 4;
				end = sizeW / 4 + this.Block0Size / 4;
			}
		}
		int idx = this._outputBuffer.Length / this._channels + begin;
		for (int c = 0; c < this._channels; c++)
		{
			this._outputBuffer.Write(c, idx, left, center, right, this._residue[c], window);
		}
		int newPrepLen = this._outputBuffer.Length / this._channels - end;
		int result = newPrepLen - this._preparedLength;
		this._preparedLength = newPrepLen;
		return result;
	}

	private void UpdatePosition(int samplesDecoded, DataPacket packet)
	{
		this._samples += samplesDecoded;
		if (packet.IsResync)
		{
			this._currentPosition = -packet.PageGranulePosition;
			this._resyncQueue.Push(packet);
		}
		else
		{
			if (samplesDecoded <= 0)
			{
				return;
			}
			this._currentPosition += samplesDecoded;
			packet.GranulePosition = this._currentPosition;
			if (this._currentPosition < 0)
			{
				if (packet.PageGranulePosition > -this._currentPosition)
				{
					long gp = this._currentPosition - samplesDecoded;
					while (this._resyncQueue.Count > 0)
					{
						DataPacket dataPacket = this._resyncQueue.Pop();
						long temp = dataPacket.GranulePosition + gp;
						dataPacket.GranulePosition = gp;
						gp = temp;
					}
				}
				else
				{
					packet.GranulePosition = -samplesDecoded;
					this._resyncQueue.Push(packet);
				}
			}
			else if (packet.IsEndOfStream && this._currentPosition > packet.PageGranulePosition)
			{
				int diff = (int)(this._currentPosition - packet.PageGranulePosition);
				if (diff >= 0)
				{
					this._preparedLength -= diff;
					this._currentPosition -= diff;
				}
				else
				{
					this._preparedLength = 0;
				}
				packet.GranulePosition = packet.PageGranulePosition;
				this._eosFound = true;
			}
		}
	}

	private void DecodeNextPacket()
	{
		this._sw.Start();
		DataPacket packet = null;
		try
		{
			IPacketProvider packetProvider = this._packetProvider;
			if (packetProvider != null)
			{
				packet = packetProvider.GetNextPacket();
			}
			if (packet == null)
			{
				this._eosFound = true;
				return;
			}
			if (!this._pagesSeen.Contains(this._lastPageSeen = packet.PageSequenceNumber))
			{
				this._pagesSeen.Add(this._lastPageSeen);
			}
			if (packet.IsResync)
			{
				this.ResetDecoder(isFullReset: false);
			}
			if (packet == this._parameterChangePacket)
			{
				this._isParameterChange = true;
				this.ProcessParameterChange(packet);
				return;
			}
			if (!this.UnpackPacket(packet))
			{
				packet.Done();
				this._wasteBits += 8 * packet.Length;
				return;
			}
			packet.Done();
			this.DecodePacket();
			int samplesDecoded = this.OverlapSamples();
			if (!packet.GranuleCount.HasValue)
			{
				packet.GranuleCount = samplesDecoded;
			}
			this.UpdatePosition(samplesDecoded, packet);
			int sc = Utils.Sum(this._sampleCountHistory) + samplesDecoded;
			this._bitsPerPacketHistory.Enqueue((int)packet.BitsRead);
			this._sampleCountHistory.Enqueue(samplesDecoded);
			while (sc > this._sampleRate)
			{
				this._bitsPerPacketHistory.Dequeue();
				sc -= this._sampleCountHistory.Dequeue();
			}
		}
		catch
		{
			packet?.Done();
			throw;
		}
		finally
		{
			this._sw.Stop();
		}
	}

	internal int GetPacketLength(DataPacket curPacket, DataPacket lastPacket)
	{
		if (lastPacket == null || curPacket.IsResync)
		{
			return 0;
		}
		if (curPacket.ReadBit())
		{
			return 0;
		}
		if (lastPacket.ReadBit())
		{
			return 0;
		}
		int modeIdx = (int)curPacket.ReadBits(this._modeFieldBits);
		if (modeIdx < 0 || modeIdx >= this.Modes.Length)
		{
			return 0;
		}
		VorbisMode mode = this.Modes[modeIdx];
		modeIdx = (int)lastPacket.ReadBits(this._modeFieldBits);
		if (modeIdx < 0 || modeIdx >= this.Modes.Length)
		{
			return 0;
		}
		VorbisMode prevMode = this.Modes[modeIdx];
		return mode.BlockSize / 4 + prevMode.BlockSize / 4;
	}

	internal int ReadSamples(float[] buffer, int offset, int count)
	{
		int samplesRead = 0;
		lock (this._seekLock)
		{
			if (this._prevBuffer != null)
			{
				int cnt = Math.Min(count, this._prevBuffer.Length);
				Buffer.BlockCopy(this._prevBuffer, 0, buffer, offset, cnt * 4);
				if (cnt < this._prevBuffer.Length)
				{
					float[] buf = new float[this._prevBuffer.Length - cnt];
					Buffer.BlockCopy(this._prevBuffer, cnt * 4, buf, 0, (this._prevBuffer.Length - cnt) * 4);
					this._prevBuffer = buf;
				}
				else
				{
					this._prevBuffer = null;
				}
				count -= cnt;
				offset += cnt;
				samplesRead = cnt;
			}
			else if (this._isParameterChange)
			{
				throw new InvalidOperationException("Currently pending a parameter change.  Read new parameters before requesting further samples!");
			}
			int minSize = count + this.Block1Size * this._channels;
			this._outputBuffer.EnsureSize(minSize);
			while (this._preparedLength * this._channels < count && !this._eosFound && !this._isParameterChange)
			{
				this.DecodeNextPacket();
				if (this._prevBuffer != null)
				{
					return this.ReadSamples(buffer, offset, this._prevBuffer.Length);
				}
			}
			if (this._preparedLength * this._channels < count)
			{
				count = this._preparedLength * this._channels;
			}
			this._outputBuffer.CopyTo(buffer, offset, count);
			this._preparedLength -= count / this._channels;
			this._reportedPosition = this._currentPosition - this._preparedLength;
		}
		return samplesRead + count;
	}

	internal void SeekTo(long granulePos)
	{
		if (!this._packetProvider.CanSeek)
		{
			throw new NotSupportedException();
		}
		if (granulePos < 0)
		{
			throw new ArgumentOutOfRangeException("granulePos");
		}
		DataPacket packet;
		if (granulePos > 0)
		{
			packet = this._packetProvider.FindPacket(granulePos, GetPacketLength);
			if (packet == null)
			{
				throw new ArgumentOutOfRangeException("granulePos");
			}
		}
		else
		{
			packet = this._packetProvider.GetPacket(4);
		}
		lock (this._seekLock)
		{
			this._packetProvider.SeekToPacket(packet, 1);
			DataPacket dataPacket = this._packetProvider.PeekNextPacket();
			this.CurrentPosition = dataPacket.GranulePosition;
			int cnt = (int)((granulePos - this.CurrentPosition) * this._channels);
			if (cnt <= 0)
			{
				return;
			}
			float[] seekBuffer = new float[cnt];
			while (cnt > 0)
			{
				int temp = this.ReadSamples(seekBuffer, 0, cnt);
				if (temp == 0)
				{
					break;
				}
				cnt -= temp;
			}
		}
	}

	internal long GetLastGranulePos()
	{
		return this._packetProvider.GetGranuleCount();
	}

	public void ResetStats()
	{
		this._clipped = false;
		this._packetCount = 0;
		this._floorBits = 0L;
		this._glueBits = 0L;
		this._modeBits = 0L;
		this._resBits = 0L;
		this._wasteBits = 0L;
		this._samples = 0L;
		this._sw.Reset();
	}
}
