using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RTAudioProcessing;

internal static class RTAP
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct RTAPSpring
	{
		public IntPtr data;

		public int data_size;

		public int format;

		public int sample_rate;

		public int block_align;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct RTAPRiver
	{
		public IntPtr spring;

		public int read_head;

		public int cache_size;

		public int l_coeff1;

		public int l_coeff2;

		public int l_delta;

		public int l_sample1;

		public int l_sample2;

		public int r_coeff1;

		public int r_coeff2;

		public int r_delta;

		public int r_sample1;

		public int r_sample2;

		public byte cache0;

		public byte cache1;

		public byte cache2;

		public byte cache3;

		public byte cache4;

		public byte cache5;

		public byte cache6;

		public byte cache7;
	}

	private static int[] ADAPTATION_TABLE;

	private static int[] ADAPTATION_COEFF1;

	private static int[] ADAPTATION_COEFF2;

	private const int COEFF1 = 0;

	private const int COEFF2 = 1;

	private const int DELTA = 2;

	private const int SAMPLE1 = 3;

	private const int SAMPLE2 = 4;

	internal const int FLAG_16 = 1;

	internal const int FLAG_STEREO = 2;

	internal const int FLAG_ADPCM = 4;

	private static int ALLOC_SIZE_SPRING;

	private static int ALLOC_SIZE_RIVER;

	private static int RIVER_CACHE_OFFSET;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static void adpcm_write_header_samples(byte* dest, int* channels, int STEREO)
	{
		if ((IntPtr)dest == IntPtr.Zero)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			for (int c = 0; c <= STEREO; c++)
			{
				int base_idx = i * 2 * (STEREO + 1) + c * 2;
				int s = (channels + c * 5)[4 - i];
				dest[base_idx] = (byte)s;
				dest[base_idx + 1] = (byte)(s >> 8);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static int adpcm_expand_nibble(int* channel, int nibble)
	{
		int nibbleSign = nibble - (((nibble & 8) != 0) ? 16 : 0);
		int predictor = (channel[3] * *channel + channel[4] * channel[1]) / 256 + nibbleSign * channel[2];
		if (predictor < -32768)
		{
			predictor = -32768;
		}
		else if (predictor > 32767)
		{
			predictor = 32767;
		}
		channel[4] = channel[3];
		channel[3] = predictor;
		int delta = RTAP.ADAPTATION_TABLE[nibble] * channel[2] / 256;
		if (delta < 16)
		{
			delta = 16;
		}
		channel[2] = delta;
		return predictor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static void adpcm_write_payload_samples(byte* dest, int dest_offset, int* channels, int nibbles, int STEREO)
	{
		int sample1 = RTAP.adpcm_expand_nibble(channels, nibbles >> 4);
		int sample2 = RTAP.adpcm_expand_nibble(channels + STEREO * 5, nibbles & 0xF);
		if (!((IntPtr)dest == IntPtr.Zero))
		{
			dest += dest_offset;
			*dest = (byte)sample1;
			dest[1] = (byte)(sample1 >> 8);
			dest[2] = (byte)sample2;
			dest[3] = (byte)(sample2 >> 8);
		}
	}

	private unsafe static int rtap_river_read_adpcm_helper(RTAPRiver* _this, byte* dest, int dest_size)
	{
		RTAPSpring* spring = (RTAPSpring*)(void*)_this->spring;
		int STEREO = (((spring->format & 2) != 0) ? 1 : 0);
		int HEADER_SIZE = STEREO + 1 + (STEREO + 1 << 1) * 3;
		int HEADER_DBYTES = STEREO + 1 << 2;
		int* channels = stackalloc int[10];
		*channels = _this->l_coeff1;
		channels[1] = _this->l_coeff2;
		channels[2] = _this->l_delta;
		channels[3] = _this->l_sample1;
		channels[4] = _this->l_sample2;
		if (STEREO != 0)
		{
			int* num = channels + 5;
			*num = _this->r_coeff1;
			num[1] = _this->r_coeff2;
			num[2] = _this->r_delta;
			num[3] = _this->r_sample1;
			num[4] = _this->r_sample2;
		}
		byte* cache = stackalloc byte[8];
		int cache_size = 0;
		byte* buffer = (byte*)(void*)spring->data;
		int block_align = spring->block_align;
		int read_head = _this->read_head;
		int data_size = spring->data_size;
		while (read_head < data_size && dest_size > 0)
		{
			int block_size = block_align;
			int read_block = read_head / block_align;
			int remaining = data_size - read_block * block_align;
			if (remaining < block_size)
			{
				block_size = remaining;
			}
			if ((block_size >> STEREO) - 7 < 0)
			{
				break;
			}
			int block_offset = read_head % block_align;
			if (block_offset == 0)
			{
				for (int c = 0; c <= STEREO; c++)
				{
					int block_predictor = buffer[read_head + c];
					if (block_predictor > 6)
					{
						block_predictor = 6;
					}
					int* num2 = channels + c * 5;
					*num2 = RTAP.ADAPTATION_COEFF1[block_predictor];
					num2[1] = RTAP.ADAPTATION_COEFF2[block_predictor];
				}
				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j <= STEREO; j++)
					{
						int base_idx = read_head + (STEREO + 1) + (STEREO + 1 << 1) * i + (j << 1);
						int s = buffer[base_idx];
						s |= buffer[base_idx + 1] << 8;
						if ((s & 0x8000) != 0)
						{
							s -= 65536;
						}
						(channels + j * 5)[i + 2] = s;
					}
				}
				read_head += HEADER_SIZE;
				block_offset = HEADER_SIZE;
				if (dest_size < HEADER_DBYTES)
				{
					RTAP.adpcm_write_header_samples(cache, channels, STEREO);
					cache_size = HEADER_DBYTES;
					break;
				}
				RTAP.adpcm_write_header_samples(dest, channels, STEREO);
				if ((IntPtr)dest != IntPtr.Zero)
				{
					dest += HEADER_DBYTES;
				}
				dest_size -= HEADER_DBYTES;
			}
			block_size -= block_offset;
			int iterations = dest_size >> 2;
			if (iterations > block_size)
			{
				iterations = block_size;
			}
			for (int k = 0; k < iterations; k++)
			{
				RTAP.adpcm_write_payload_samples(dest, k << 2, channels, buffer[read_head + k], STEREO);
			}
			read_head += iterations;
			if ((IntPtr)dest != IntPtr.Zero)
			{
				dest += iterations << 2;
			}
			dest_size -= iterations << 2;
			if (iterations < block_size && (dest_size & 3) != 0)
			{
				RTAP.adpcm_write_payload_samples(cache, 0, channels, buffer[read_head], STEREO);
				cache_size = 4;
				read_head++;
				break;
			}
		}
		_this->read_head = read_head;
		_this->cache_size = cache_size;
		_this->l_coeff1 = *channels;
		_this->l_coeff2 = channels[1];
		_this->l_delta = channels[2];
		_this->l_sample1 = channels[3];
		_this->l_sample2 = channels[4];
		if (STEREO != 0)
		{
			int* right = channels + 5;
			_this->r_coeff1 = *right;
			_this->r_coeff2 = right[1];
			_this->r_delta = right[2];
			_this->r_sample1 = right[3];
			_this->r_sample2 = right[4];
		}
		byte* river_cache = (byte*)_this + RTAP.RIVER_CACHE_OFFSET;
		for (int l = 0; l < cache_size; l++)
		{
			river_cache[l] = cache[l];
		}
		return dest_size;
	}

	static RTAP()
	{
		RTAP.ADAPTATION_TABLE = new int[16]
		{
			230, 230, 230, 230, 307, 409, 512, 614, 768, 614,
			512, 409, 307, 230, 230, 230
		};
		RTAP.ADAPTATION_COEFF1 = new int[7] { 256, 512, 0, 192, 240, 460, 392 };
		RTAP.ADAPTATION_COEFF2 = new int[7] { 0, -256, 0, 64, 0, -208, -232 };
		RTAP.ALLOC_SIZE_SPRING = RTAP.rtap_alloc_size_for_spring();
		RTAP.ALLOC_SIZE_RIVER = RTAP.rtap_alloc_size_for_river();
		RTAP.RIVER_CACHE_OFFSET = (int)Marshal.OffsetOf(typeof(RTAPRiver), "cache0");
	}

	internal static int rtap_alloc_size_for_spring()
	{
		return Marshal.SizeOf(typeof(RTAPSpring));
	}

	internal static int rtap_alloc_size_for_river()
	{
		return Marshal.SizeOf(typeof(RTAPRiver));
	}

	internal unsafe static void rtap_spring_init(IntPtr _this, IntPtr data_ptr, int data_size, int format, int sample_rate, int block_align)
	{
		if (_this == IntPtr.Zero)
		{
			throw new InvalidOperationException("rtap_spring_init(...) called on a NULL pointer.");
		}
		RTAPSpring* spring = (RTAPSpring*)(void*)_this;
		spring->data = data_ptr;
		spring->data_size = data_size;
		spring->format = format;
		spring->sample_rate = sample_rate;
		spring->block_align = block_align;
	}

	internal unsafe static int rtap_spring_get_length(IntPtr _this)
	{
		if (_this == IntPtr.Zero)
		{
			throw new InvalidOperationException("rtap_spring_get_length(...) called on a NULL pointer.");
		}
		RTAPSpring* spring = (RTAPSpring*)(void*)_this;
		if ((spring->format & 4) == 0)
		{
			return spring->data_size;
		}
		int stereo = (((spring->format & 2) != 0) ? 1 : 0);
		int full_block_samples = ((spring->block_align >> stereo) - 7 << 1) + 2;
		int partial_block_bytes = spring->data_size % spring->block_align;
		int partial_block_samples = 0;
		if (partial_block_bytes > 0)
		{
			partial_block_samples = ((partial_block_bytes >> stereo) - 7 << 1) + 2;
		}
		if (partial_block_samples < 2)
		{
			partial_block_samples = 0;
		}
		return (spring->data_size / spring->block_align * full_block_samples + partial_block_samples) * 2 * (stereo + 1);
	}

	internal unsafe static double rtap_spring_get_duration(IntPtr _this)
	{
		if (_this == IntPtr.Zero)
		{
			throw new InvalidOperationException("rtap_spring_get_duration(...) called on a NULL pointer.");
		}
		RTAPSpring* spring = (RTAPSpring*)(void*)_this;
		double divisor = spring->sample_rate;
		if ((spring->format & 1) != 0)
		{
			divisor *= 2.0;
		}
		if ((spring->format & 2) != 0)
		{
			divisor *= 2.0;
		}
		if (divisor <= 1E-05)
		{
			return 0.0;
		}
		return (double)spring->data_size / divisor;
	}

	internal static void rtap_river_init(IntPtr _this, IntPtr spring_ptr)
	{
		if (_this == IntPtr.Zero)
		{
			throw new InvalidOperationException("rtap_river_init(...) called on a NULL pointer.");
		}
		RTAP.rtap_river_set_spring(_this, spring_ptr);
	}

	internal static void rtap_river_reset(IntPtr _this)
	{
		if (_this == IntPtr.Zero)
		{
			throw new InvalidOperationException("rtap_river_reset(...) called on a NULL pointer.");
		}
		RTAP.rtap_river_set_spring(_this, IntPtr.Zero);
	}

	internal unsafe static void rtap_river_set_spring(IntPtr _this, IntPtr spring_ptr)
	{
		if (_this == IntPtr.Zero)
		{
			throw new InvalidOperationException("rtap_river_set_spring(...) called on a NULL pointer.");
		}
		byte* memory = (byte*)(void*)_this;
		for (int i = 0; i < RTAP.ALLOC_SIZE_RIVER; i++)
		{
			memory[i] = 0;
		}
		RTAPRiver* river = (RTAPRiver*)(void*)_this;
		river->spring = spring_ptr;
		river->read_head = 0;
	}

	internal unsafe static int rtap_river_read_into(IntPtr _this, IntPtr buffer_ptr, int start_idx, int length)
	{
		if (_this == IntPtr.Zero)
		{
			throw new InvalidOperationException("rtap_river_read_into(...) called on a NULL pointer.");
		}
		RTAPRiver* river = (RTAPRiver*)(void*)_this;
		if (river->spring == IntPtr.Zero)
		{
			throw new InvalidOperationException("rtap_river_read_into(...) called on a river with a NULL spring pointer.");
		}
		if (length <= 0)
		{
			throw new ArgumentException("rtap_river_read_into(...) called with a length of 0.");
		}
		RTAPSpring* spring = (RTAPSpring*)(void*)river->spring;
		if ((spring->format & 4) == 0)
		{
			Buffer.MemoryCopy((byte*)(void*)spring->data + start_idx, (void*)buffer_ptr, length, length);
		}
		else
		{
			RTAP.rtap_river_read_adpcm(river, buffer_ptr, start_idx, length);
		}
		return 0;
	}

	private unsafe static void rtap_river_read_adpcm(RTAPRiver* _this, IntPtr buffer_ptr, int start_idx, int length)
	{
		RTAPSpring* spring = (RTAPSpring*)(void*)_this->spring;
		byte* dest = (byte*)(void*)buffer_ptr;
		byte* river_cache = (byte*)_this + RTAP.RIVER_CACHE_OFFSET;
		int dest_size = length;
		int stereo = (((spring->format & 2) != 0) ? 1 : 0);
		_ = *_this;
		int block_align = spring->block_align;
		_ = *spring;
		int dbytes_per_block = (((block_align >> stereo) - 7 << 1) + 2 << stereo) * 2;
		int request_block = start_idx / dbytes_per_block;
		int request_offset = start_idx - request_block * dbytes_per_block;
		_this->read_head = request_block * block_align;
		int cache_size;
		if (request_offset != 0)
		{
			_this->read_head = request_block * block_align;
			RTAP.rtap_river_read_adpcm_helper(_this, (byte*)(void*)IntPtr.Zero, request_offset);
			int read_head = _this->read_head;
			cache_size = _this->cache_size;
			int current_block = read_head / block_align;
			int current_offset_samples = (read_head - current_block * block_align - (7 << stereo) << 1) + (2 << stereo);
			if (current_offset_samples < 0)
			{
				current_offset_samples = 0;
			}
			int current_offset_dbytes = current_offset_samples * 2;
			int cache_request = current_block * dbytes_per_block + current_offset_dbytes - start_idx;
			if (cache_request < 0)
			{
				throw new InvalidOperationException("rtap_river_read_adpcm(...) somehow has a start_idx beyond the read_head.");
			}
			if (cache_request > 0 && cache_request <= cache_size)
			{
				byte* cache = river_cache + (cache_size - cache_request);
				for (int i = 0; i < cache_request; i++)
				{
					dest[i] = cache[i];
				}
				dest += cache_request;
				dest_size -= cache_request;
			}
		}
		if (dest_size <= 0)
		{
			return;
		}
		dest_size = RTAP.rtap_river_read_adpcm_helper(_this, dest, dest_size);
		if (dest_size <= 0)
		{
			return;
		}
		cache_size = _this->cache_size;
		if (cache_size > 0)
		{
			int cache_request2 = dest_size;
			if (cache_request2 > cache_size)
			{
				cache_request2 = cache_size;
			}
			byte* cache2 = river_cache;
			for (int j = 0; j < cache_request2; j++)
			{
				dest[j] = cache2[j];
			}
			dest += cache_request2;
			dest_size -= cache_request2;
		}
		for (int k = 0; k < dest_size; k++)
		{
			dest[k] = 0;
		}
	}
}
