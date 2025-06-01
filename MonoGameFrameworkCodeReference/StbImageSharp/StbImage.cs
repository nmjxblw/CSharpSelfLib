using System;

namespace StbImageSharp;

internal static class StbImage
{
	public unsafe delegate int ReadCallback(void* user, sbyte* data, int size);

	public unsafe delegate int SkipCallback(void* user, int n);

	public unsafe delegate int EofCallback(void* user);

	public unsafe delegate void idct_block_kernel(byte* output, int out_stride, short* data);

	public unsafe delegate void YCbCr_to_RGB_kernel(byte* output, byte* y, byte* pcb, byte* pcr, int count, int step);

	public unsafe delegate byte* Resampler(byte* a, byte* b, byte* c, int d, int e);

	public class stbi_io_callbacks
	{
		public ReadCallback read;

		public SkipCallback skip;

		public EofCallback eof;
	}

	public class stbi__context : IDisposable
	{
		public uint img_x;

		public uint img_y;

		public int img_n;

		public int img_out_n;

		public stbi_io_callbacks io = new stbi_io_callbacks();

		public unsafe void* io_user_data;

		public int read_from_callbacks;

		public int buflen;

		public unsafe byte* buffer_start = (byte*)CRuntime.malloc(128L);

		public unsafe byte* img_buffer;

		public unsafe byte* img_buffer_end;

		public unsafe byte* img_buffer_original;

		public unsafe byte* img_buffer_original_end;

		~stbi__context()
		{
			this.Dispose();
		}

		public unsafe void Dispose()
		{
			if (this.buffer_start != null)
			{
				CRuntime.free(this.buffer_start);
				this.buffer_start = null;
			}
		}
	}

	public struct img_comp
	{
		public int id;

		public int h;

		public int v;

		public int tq;

		public int hd;

		public int ha;

		public int dc_pred;

		public int x;

		public int y;

		public int w2;

		public int h2;

		public unsafe byte* data;

		public unsafe void* raw_data;

		public unsafe void* raw_coeff;

		public unsafe byte* linebuf;

		public unsafe short* coeff;

		public int coeff_w;

		public int coeff_h;
	}

	public class stbi__jpeg
	{
		public stbi__context s;

		public readonly stbi__huffman[] huff_dc = new stbi__huffman[4];

		public readonly stbi__huffman[] huff_ac = new stbi__huffman[4];

		public readonly ushort[][] dequant;

		public readonly short[][] fast_ac;

		public int img_h_max;

		public int img_v_max;

		public int img_mcu_x;

		public int img_mcu_y;

		public int img_mcu_w;

		public int img_mcu_h;

		public img_comp[] img_comp = new img_comp[4];

		public uint code_buffer;

		public int code_bits;

		public byte marker;

		public int nomore;

		public int progressive;

		public int spec_start;

		public int spec_end;

		public int succ_high;

		public int succ_low;

		public int eob_run;

		public int jfif;

		public int app14_color_transform;

		public int rgb;

		public int scan_n;

		public int[] order = new int[4];

		public int restart_interval;

		public int todo;

		public idct_block_kernel idct_block_kernel;

		public YCbCr_to_RGB_kernel YCbCr_to_RGB_kernel;

		public Resampler resample_row_hv_2_kernel;

		public stbi__jpeg()
		{
			for (int i = 0; i < 4; i++)
			{
				this.huff_ac[i] = new stbi__huffman();
				this.huff_dc[i] = new stbi__huffman();
			}
			for (int j = 0; j < this.img_comp.Length; j++)
			{
				this.img_comp[j] = default(img_comp);
			}
			this.fast_ac = new short[4][];
			for (int k = 0; k < this.fast_ac.Length; k++)
			{
				this.fast_ac[k] = new short[512];
			}
			this.dequant = new ushort[4][];
			for (int l = 0; l < this.dequant.Length; l++)
			{
				this.dequant[l] = new ushort[64];
			}
		}
	}

	public class stbi__resample
	{
		public Resampler resample;

		public unsafe byte* line0;

		public unsafe byte* line1;

		public int hs;

		public int vs;

		public int w_lores;

		public int ystep;

		public int ypos;
	}

	public struct stbi__gif_lzw
	{
		public short prefix;

		public byte first;

		public byte suffix;
	}

	public class stbi__gif : IDisposable
	{
		public int w;

		public int h;

		public unsafe byte* _out_;

		public unsafe byte* background;

		public unsafe byte* history;

		public int flags;

		public int bgindex;

		public int ratio;

		public int transparent;

		public int eflags;

		public int delay;

		public unsafe byte* pal;

		public unsafe byte* lpal;

		public unsafe stbi__gif_lzw* codes = (stbi__gif_lzw*)StbImage.stbi__malloc(8192 * sizeof(stbi__gif_lzw));

		public unsafe byte* color_table;

		public int parse;

		public int step;

		public int lflags;

		public int start_x;

		public int start_y;

		public int max_x;

		public int max_y;

		public int cur_x;

		public int cur_y;

		public int line_size;

		public unsafe stbi__gif()
		{
			this.pal = (byte*)StbImage.stbi__malloc(1024);
			this.lpal = (byte*)StbImage.stbi__malloc(1024);
		}

		~stbi__gif()
		{
			this.Dispose();
		}

		public unsafe void Dispose()
		{
			if (this.pal != null)
			{
				CRuntime.free(this.pal);
				this.pal = null;
			}
			if (this.lpal != null)
			{
				CRuntime.free(this.lpal);
				this.lpal = null;
			}
			if (this.codes != null)
			{
				CRuntime.free(this.codes);
				this.codes = null;
			}
		}
	}

	public struct stbi__result_info
	{
		public int bits_per_channel;

		public int num_channels;

		public int channel_order;
	}

	public class stbi__huffman
	{
		public byte[] fast = new byte[512];

		public ushort[] code = new ushort[256];

		public byte[] values = new byte[256];

		public byte[] size = new byte[257];

		public uint[] maxcode = new uint[18];

		public int[] delta = new int[17];
	}

	public struct stbi__zhuffman
	{
		public unsafe fixed ushort fast[512];

		public unsafe fixed ushort firstcode[16];

		public unsafe fixed int maxcode[17];

		public unsafe fixed ushort firstsymbol[16];

		public unsafe fixed byte size[288];

		public unsafe fixed ushort value[288];
	}

	public struct stbi__zbuf
	{
		public unsafe byte* zbuffer;

		public unsafe byte* zbuffer_end;

		public int num_bits;

		public uint code_buffer;

		public unsafe sbyte* zout;

		public unsafe sbyte* zout_start;

		public unsafe sbyte* zout_end;

		public int z_expandable;

		public stbi__zhuffman z_length;

		public stbi__zhuffman z_distance;
	}

	public struct stbi__pngchunk
	{
		public uint length;

		public uint type;
	}

	public class stbi__png
	{
		public stbi__context s;

		public unsafe byte* idata;

		public unsafe byte* expanded;

		public unsafe byte* _out_;

		public int depth;
	}

	public struct stbi__bmp_data
	{
		public int bpp;

		public int offset;

		public int hsz;

		public uint mr;

		public uint mg;

		public uint mb;

		public uint ma;

		public uint all_a;
	}

	public static string LastError;

	public const int STBI__ZFAST_BITS = 9;

	public static string stbi__g_failure_reason;

	public static int stbi__vertically_flip_on_load;

	public const int STBI_default = 0;

	public const int STBI_grey = 1;

	public const int STBI_grey_alpha = 2;

	public const int STBI_rgb = 3;

	public const int STBI_rgb_alpha = 4;

	public const int STBI_ORDER_RGB = 0;

	public const int STBI_ORDER_BGR = 1;

	public const int STBI__SCAN_load = 0;

	public const int STBI__SCAN_type = 1;

	public const int STBI__SCAN_header = 2;

	public const int STBI__F_none = 0;

	public const int STBI__F_sub = 1;

	public const int STBI__F_up = 2;

	public const int STBI__F_avg = 3;

	public const int STBI__F_paeth = 4;

	public const int STBI__F_avg_first = 5;

	public const int STBI__F_paeth_first = 6;

	public static float stbi__h2l_gamma_i = 0.45454544f;

	public static float stbi__h2l_scale_i = 1f;

	public static uint[] stbi__bmask = new uint[17]
	{
		0u, 1u, 3u, 7u, 15u, 31u, 63u, 127u, 255u, 511u,
		1023u, 2047u, 4095u, 8191u, 16383u, 32767u, 65535u
	};

	public static int[] stbi__jbias = new int[16]
	{
		0, -1, -3, -7, -15, -31, -63, -127, -255, -511,
		-1023, -2047, -4095, -8191, -16383, -32767
	};

	public static byte[] stbi__jpeg_dezigzag = new byte[79]
	{
		0, 1, 8, 16, 9, 2, 3, 10, 17, 24,
		32, 25, 18, 11, 4, 5, 12, 19, 26, 33,
		40, 48, 41, 34, 27, 20, 13, 6, 7, 14,
		21, 28, 35, 42, 49, 56, 57, 50, 43, 36,
		29, 22, 15, 23, 30, 37, 44, 51, 58, 59,
		52, 45, 38, 31, 39, 46, 53, 60, 61, 54,
		47, 55, 62, 63, 63, 63, 63, 63, 63, 63,
		63, 63, 63, 63, 63, 63, 63, 63, 63
	};

	public static int[] stbi__zlength_base = new int[31]
	{
		3, 4, 5, 6, 7, 8, 9, 10, 11, 13,
		15, 17, 19, 23, 27, 31, 35, 43, 51, 59,
		67, 83, 99, 115, 131, 163, 195, 227, 258, 0,
		0
	};

	public static int[] stbi__zlength_extra = new int[31]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
		1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
		4, 4, 4, 4, 5, 5, 5, 5, 0, 0,
		0
	};

	public static int[] stbi__zdist_base = new int[32]
	{
		1, 2, 3, 4, 5, 7, 9, 13, 17, 25,
		33, 49, 65, 97, 129, 193, 257, 385, 513, 769,
		1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577,
		0, 0
	};

	public static int[] stbi__zdist_extra = new int[30]
	{
		0, 0, 0, 0, 1, 1, 2, 2, 3, 3,
		4, 4, 5, 5, 6, 6, 7, 7, 8, 8,
		9, 9, 10, 10, 11, 11, 12, 12, 13, 13
	};

	public static byte[] stbi__zdefault_length = new byte[288]
	{
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		8, 8, 8, 8, 8, 8, 8, 8
	};

	public static byte[] stbi__zdefault_distance = new byte[32]
	{
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5
	};

	public static byte[] first_row_filter = new byte[5] { 0, 1, 0, 5, 6 };

	public static byte[] stbi__depth_scale_table = new byte[9] { 0, 255, 85, 0, 17, 0, 0, 0, 1 };

	public static int stbi__unpremultiply_on_load = 0;

	public static int stbi__de_iphone_flag = 0;

	private unsafe static void* stbi__malloc(int size)
	{
		return CRuntime.malloc((ulong)size);
	}

	private unsafe static void* stbi__malloc(ulong size)
	{
		return StbImage.stbi__malloc((int)size);
	}

	private static int stbi__err(string str)
	{
		StbImage.LastError = str;
		return 0;
	}

	public unsafe static void stbi__gif_parse_colortable(stbi__context s, byte* pal, int num_entries, int transp)
	{
		for (int i = 0; i < num_entries; i++)
		{
			pal[i * 4 + 2] = StbImage.stbi__get8(s);
			pal[i * 4 + 1] = StbImage.stbi__get8(s);
			pal[i * 4] = StbImage.stbi__get8(s);
			pal[i * 4 + 3] = (byte)((transp != i) ? 255u : 0u);
		}
	}

	public unsafe static void stbi__start_mem(stbi__context s, byte* buffer, int len)
	{
		s.io.read = null;
		s.read_from_callbacks = 0;
		s.img_buffer = (s.img_buffer_original = buffer);
		s.img_buffer_end = (s.img_buffer_original_end = buffer + len);
	}

	public unsafe static void stbi__start_callbacks(stbi__context s, stbi_io_callbacks c, void* user)
	{
		s.io = c;
		s.io_user_data = user;
		s.buflen = 128;
		s.read_from_callbacks = 1;
		s.img_buffer_original = s.buffer_start;
		StbImage.stbi__refill_buffer(s);
		s.img_buffer_original_end = s.img_buffer_end;
	}

	public unsafe static void stbi__rewind(stbi__context s)
	{
		s.img_buffer = s.img_buffer_original;
		s.img_buffer_end = s.img_buffer_original_end;
	}

	public static int stbi__addsizes_valid(int a, int b)
	{
		if (b < 0)
		{
			return 0;
		}
		return (a <= int.MaxValue - b) ? 1 : 0;
	}

	public static int stbi__mul2sizes_valid(int a, int b)
	{
		if (a < 0 || b < 0)
		{
			return 0;
		}
		if (b == 0)
		{
			return 1;
		}
		return (a <= int.MaxValue / b) ? 1 : 0;
	}

	public static int stbi__mad2sizes_valid(int a, int b, int add)
	{
		if (StbImage.stbi__mul2sizes_valid(a, b) == 0 || StbImage.stbi__addsizes_valid(a * b, add) == 0)
		{
			return 0;
		}
		return 1;
	}

	public static int stbi__mad3sizes_valid(int a, int b, int c, int add)
	{
		if (StbImage.stbi__mul2sizes_valid(a, b) == 0 || StbImage.stbi__mul2sizes_valid(a * b, c) == 0 || StbImage.stbi__addsizes_valid(a * b * c, add) == 0)
		{
			return 0;
		}
		return 1;
	}

	public unsafe static void* stbi__malloc_mad2(int a, int b, int add)
	{
		if (StbImage.stbi__mad2sizes_valid(a, b, add) == 0)
		{
			return null;
		}
		return StbImage.stbi__malloc((ulong)(a * b + add));
	}

	public unsafe static void* stbi__malloc_mad3(int a, int b, int c, int add)
	{
		if (StbImage.stbi__mad3sizes_valid(a, b, c, add) == 0)
		{
			return null;
		}
		return StbImage.stbi__malloc((ulong)(a * b * c + add));
	}

	public static void stbi_set_flip_vertically_on_load(int flag_true_if_should_flip)
	{
		StbImage.stbi__vertically_flip_on_load = flag_true_if_should_flip;
	}

	public unsafe static void* stbi__load_main(stbi__context s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri, int bpc)
	{
		ri->bits_per_channel = 8;
		ri->channel_order = 0;
		ri->num_channels = 0;
		if (StbImage.stbi__jpeg_test(s) != 0)
		{
			return StbImage.stbi__jpeg_load(s, x, y, comp, req_comp, ri);
		}
		if (StbImage.stbi__png_test(s) != 0)
		{
			return StbImage.stbi__png_load(s, x, y, comp, req_comp, ri);
		}
		if (StbImage.stbi__bmp_test(s) != 0)
		{
			return StbImage.stbi__bmp_load(s, x, y, comp, req_comp, ri);
		}
		if (StbImage.stbi__gif_test(s) != 0)
		{
			return StbImage.stbi__gif_load(s, x, y, comp, req_comp, ri);
		}
		if (StbImage.stbi__psd_test(s) != 0)
		{
			return StbImage.stbi__psd_load(s, x, y, comp, req_comp, ri, bpc);
		}
		if (StbImage.stbi__tga_test(s) != 0)
		{
			return StbImage.stbi__tga_load(s, x, y, comp, req_comp, ri);
		}
		return (void*)((StbImage.stbi__err("unknown image type") != 0) ? 0u : 0u);
	}

	public unsafe static byte* stbi__convert_16_to_8(ushort* orig, int w, int h, int channels)
	{
		int i = 0;
		int img_len = w * h * channels;
		byte* reduced = (byte*)StbImage.stbi__malloc((ulong)img_len);
		if (reduced == null)
		{
			return (byte*)((StbImage.stbi__err("outofmem") != 0) ? 0u : 0u);
		}
		for (i = 0; i < img_len; i++)
		{
			reduced[i] = (byte)((orig[i] >> 8) & 0xFF);
		}
		CRuntime.free(orig);
		return reduced;
	}

	public unsafe static ushort* stbi__convert_8_to_16(byte* orig, int w, int h, int channels)
	{
		int i = 0;
		int img_len = w * h * channels;
		ushort* enlarged = (ushort*)StbImage.stbi__malloc((ulong)(img_len * 2));
		if (enlarged == null)
		{
			return (ushort*)((StbImage.stbi__err("outofmem") != 0) ? 0u : 0u);
		}
		for (i = 0; i < img_len; i++)
		{
			enlarged[i] = (ushort)((orig[i] << 8) + orig[i]);
		}
		CRuntime.free(orig);
		return enlarged;
	}

	public unsafe static void stbi__vertical_flip(void* image, int w, int h, int bytes_per_pixel)
	{
		int row = 0;
		ulong bytes_per_row = (ulong)(w * bytes_per_pixel);
		byte* temp = stackalloc byte[2048];
		for (row = 0; row < h >> 1; row++)
		{
			byte* row2 = (byte*)image + (long)row * (long)bytes_per_row;
			byte* row3 = (byte*)image + (long)(h - row - 1) * (long)bytes_per_row;
			ulong bytes_left = bytes_per_row;
			while (bytes_left != 0L)
			{
				ulong bytes_copy = ((bytes_left < 2048) ? bytes_left : 2048);
				CRuntime.memcpy(temp, row2, bytes_copy);
				CRuntime.memcpy(row2, row3, bytes_copy);
				CRuntime.memcpy(row3, temp, bytes_copy);
				row2 += bytes_copy;
				row3 += bytes_copy;
				bytes_left -= bytes_copy;
			}
		}
	}

	public unsafe static void stbi__vertical_flip_slices(void* image, int w, int h, int z, int bytes_per_pixel)
	{
		int slice = 0;
		int slice_size = w * h * bytes_per_pixel;
		byte* bytes = (byte*)image;
		for (slice = 0; slice < z; slice++)
		{
			StbImage.stbi__vertical_flip(bytes, w, h, bytes_per_pixel);
			bytes += slice_size;
		}
	}

	public unsafe static byte* stbi__load_and_postprocess_8bit(stbi__context s, int* x, int* y, int* comp, int req_comp)
	{
		stbi__result_info ri = default(stbi__result_info);
		void* result = StbImage.stbi__load_main(s, x, y, comp, req_comp, &ri, 8);
		if (result == null)
		{
			return null;
		}
		if (ri.bits_per_channel != 8)
		{
			result = StbImage.stbi__convert_16_to_8((ushort*)result, *x, *y, (req_comp == 0) ? (*comp) : req_comp);
			ri.bits_per_channel = 8;
		}
		if (StbImage.stbi__vertically_flip_on_load != 0)
		{
			int channels = ((req_comp != 0) ? req_comp : (*comp));
			StbImage.stbi__vertical_flip(result, *x, *y, channels);
		}
		return (byte*)result;
	}

	public unsafe static ushort* stbi__load_and_postprocess_16bit(stbi__context s, int* x, int* y, int* comp, int req_comp)
	{
		stbi__result_info ri = default(stbi__result_info);
		void* result = StbImage.stbi__load_main(s, x, y, comp, req_comp, &ri, 16);
		if (result == null)
		{
			return null;
		}
		if (ri.bits_per_channel != 16)
		{
			result = StbImage.stbi__convert_8_to_16((byte*)result, *x, *y, (req_comp == 0) ? (*comp) : req_comp);
			ri.bits_per_channel = 16;
		}
		if (StbImage.stbi__vertically_flip_on_load != 0)
		{
			int channels = ((req_comp != 0) ? req_comp : (*comp));
			StbImage.stbi__vertical_flip(result, *x, *y, channels * 2);
		}
		return (ushort*)result;
	}

	public unsafe static ushort* stbi_load_16_from_memory(byte* buffer, int len, int* x, int* y, int* channels_in_file, int desired_channels)
	{
		stbi__context s = new stbi__context();
		StbImage.stbi__start_mem(s, buffer, len);
		return StbImage.stbi__load_and_postprocess_16bit(s, x, y, channels_in_file, desired_channels);
	}

	public unsafe static ushort* stbi_load_16_from_callbacks(stbi_io_callbacks clbk, void* user, int* x, int* y, int* channels_in_file, int desired_channels)
	{
		stbi__context s = new stbi__context();
		StbImage.stbi__start_callbacks(s, clbk, user);
		return StbImage.stbi__load_and_postprocess_16bit(s, x, y, channels_in_file, desired_channels);
	}

	public unsafe static byte* stbi_load_from_memory(byte* buffer, int len, int* x, int* y, int* comp, int req_comp)
	{
		stbi__context s = new stbi__context();
		StbImage.stbi__start_mem(s, buffer, len);
		return StbImage.stbi__load_and_postprocess_8bit(s, x, y, comp, req_comp);
	}

	public unsafe static byte* stbi_load_from_callbacks(stbi_io_callbacks clbk, void* user, int* x, int* y, int* comp, int req_comp)
	{
		stbi__context s = new stbi__context();
		StbImage.stbi__start_callbacks(s, clbk, user);
		return StbImage.stbi__load_and_postprocess_8bit(s, x, y, comp, req_comp);
	}

	public unsafe static byte* stbi_load_gif_from_memory(byte* buffer, int len, int** delays, int* x, int* y, int* z, int* comp, int req_comp)
	{
		stbi__context s = new stbi__context();
		StbImage.stbi__start_mem(s, buffer, len);
		byte* result = (byte*)StbImage.stbi__load_gif_main(s, delays, x, y, z, comp, req_comp);
		if (StbImage.stbi__vertically_flip_on_load != 0)
		{
			StbImage.stbi__vertical_flip_slices(result, *x, *y, *z, *comp);
		}
		return result;
	}

	public static void stbi_hdr_to_ldr_gamma(float gamma)
	{
		StbImage.stbi__h2l_gamma_i = 1f / gamma;
	}

	public static void stbi_hdr_to_ldr_scale(float scale)
	{
		StbImage.stbi__h2l_scale_i = 1f / scale;
	}

	public unsafe static void stbi__refill_buffer(stbi__context s)
	{
		int n = s.io.read(s.io_user_data, (sbyte*)s.buffer_start, s.buflen);
		if (n == 0)
		{
			s.read_from_callbacks = 0;
			s.img_buffer = s.buffer_start;
			s.img_buffer_end = s.buffer_start;
			s.img_buffer_end++;
			*s.img_buffer = 0;
		}
		else
		{
			s.img_buffer = s.buffer_start;
			s.img_buffer_end = s.buffer_start;
			s.img_buffer_end += n;
		}
	}

	public unsafe static byte stbi__get8(stbi__context s)
	{
		if (s.img_buffer < s.img_buffer_end)
		{
			return *(s.img_buffer++);
		}
		if (s.read_from_callbacks != 0)
		{
			StbImage.stbi__refill_buffer(s);
			return *(s.img_buffer++);
		}
		return 0;
	}

	public unsafe static int stbi__at_eof(stbi__context s)
	{
		if (s.io.read != null)
		{
			if (s.io.eof(s.io_user_data) == 0)
			{
				return 0;
			}
			if (s.read_from_callbacks == 0)
			{
				return 1;
			}
		}
		return (s.img_buffer >= s.img_buffer_end) ? 1 : 0;
	}

	public unsafe static void stbi__skip(stbi__context s, int n)
	{
		if (n < 0)
		{
			s.img_buffer = s.img_buffer_end;
			return;
		}
		if (s.io.read != null)
		{
			int blen = (int)(s.img_buffer_end - s.img_buffer);
			if (blen < n)
			{
				s.img_buffer = s.img_buffer_end;
				s.io.skip(s.io_user_data, n - blen);
				return;
			}
		}
		s.img_buffer += n;
	}

	public unsafe static int stbi__getn(stbi__context s, byte* buffer, int n)
	{
		if (s.io.read != null)
		{
			int blen = (int)(s.img_buffer_end - s.img_buffer);
			if (blen < n)
			{
				CRuntime.memcpy(buffer, s.img_buffer, (ulong)blen);
				bool result = s.io.read(s.io_user_data, (sbyte*)(buffer + blen), n - blen) == n - blen;
				s.img_buffer = s.img_buffer_end;
				return result ? 1 : 0;
			}
		}
		if (s.img_buffer + n <= s.img_buffer_end)
		{
			CRuntime.memcpy(buffer, s.img_buffer, (ulong)n);
			s.img_buffer += n;
			return 1;
		}
		return 0;
	}

	public static int stbi__get16be(stbi__context s)
	{
		return (StbImage.stbi__get8(s) << 8) + StbImage.stbi__get8(s);
	}

	public static uint stbi__get32be(stbi__context s)
	{
		return (uint)((uint)(StbImage.stbi__get16be(s) << 16) + StbImage.stbi__get16be(s));
	}

	public static int stbi__get16le(stbi__context s)
	{
		return StbImage.stbi__get8(s) + (StbImage.stbi__get8(s) << 8);
	}

	public static uint stbi__get32le(stbi__context s)
	{
		return (uint)((uint)StbImage.stbi__get16le(s) + (StbImage.stbi__get16le(s) << 16));
	}

	public static byte stbi__compute_y(int r, int g, int b)
	{
		return (byte)(r * 77 + g * 150 + 29 * b >> 8);
	}

	public unsafe static byte* stbi__convert_format(byte* data, int img_n, int req_comp, uint x, uint y)
	{
		int i = 0;
		int j = 0;
		if (req_comp == img_n)
		{
			return data;
		}
		byte* good = (byte*)StbImage.stbi__malloc_mad3(req_comp, (int)x, (int)y, 0);
		if (good == null)
		{
			CRuntime.free(data);
			return (byte*)((StbImage.stbi__err("outofmem") != 0) ? 0u : 0u);
		}
		for (j = 0; j < (int)y; j++)
		{
			byte* src = data + j * x * img_n;
			byte* dest = good + j * x * req_comp;
			switch (img_n * 8 + req_comp)
			{
			case 10:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = *src;
					dest[1] = byte.MaxValue;
					i--;
					src++;
					dest += 2;
				}
				break;
			case 11:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = (dest[1] = (dest[2] = *src));
					i--;
					src++;
					dest += 3;
				}
				break;
			case 12:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = (dest[1] = (dest[2] = *src));
					dest[3] = byte.MaxValue;
					i--;
					src++;
					dest += 4;
				}
				break;
			case 17:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = *src;
					i--;
					src += 2;
					dest++;
				}
				break;
			case 19:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = (dest[1] = (dest[2] = *src));
					i--;
					src += 2;
					dest += 3;
				}
				break;
			case 20:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = (dest[1] = (dest[2] = *src));
					dest[3] = src[1];
					i--;
					src += 2;
					dest += 4;
				}
				break;
			case 28:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = *src;
					dest[1] = src[1];
					dest[2] = src[2];
					dest[3] = byte.MaxValue;
					i--;
					src += 3;
					dest += 4;
				}
				break;
			case 25:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = StbImage.stbi__compute_y(*src, src[1], src[2]);
					i--;
					src += 3;
					dest++;
				}
				break;
			case 26:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = StbImage.stbi__compute_y(*src, src[1], src[2]);
					dest[1] = byte.MaxValue;
					i--;
					src += 3;
					dest += 2;
				}
				break;
			case 33:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = StbImage.stbi__compute_y(*src, src[1], src[2]);
					i--;
					src += 4;
					dest++;
				}
				break;
			case 34:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = StbImage.stbi__compute_y(*src, src[1], src[2]);
					dest[1] = src[3];
					i--;
					src += 4;
					dest += 2;
				}
				break;
			case 35:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = *src;
					dest[1] = src[1];
					dest[2] = src[2];
					i--;
					src += 4;
					dest += 3;
				}
				break;
			default:
				return (byte*)((StbImage.stbi__err("0") != 0) ? 0u : 0u);
			}
		}
		CRuntime.free(data);
		return good;
	}

	public static ushort stbi__compute_y_16(int r, int g, int b)
	{
		return (ushort)(r * 77 + g * 150 + 29 * b >> 8);
	}

	public unsafe static ushort* stbi__convert_format16(ushort* data, int img_n, int req_comp, uint x, uint y)
	{
		int i = 0;
		int j = 0;
		if (req_comp == img_n)
		{
			return data;
		}
		ushort* good = (ushort*)StbImage.stbi__malloc((ulong)(req_comp * x * y * 2));
		if (good == null)
		{
			CRuntime.free(data);
			return (ushort*)((StbImage.stbi__err("outofmem") != 0) ? 0u : 0u);
		}
		for (j = 0; j < (int)y; j++)
		{
			ushort* src = data + j * x * img_n;
			ushort* dest = good + j * x * req_comp;
			switch (img_n * 8 + req_comp)
			{
			case 10:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = *src;
					dest[1] = ushort.MaxValue;
					i--;
					src++;
					dest += 2;
				}
				break;
			case 11:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = (dest[1] = (dest[2] = *src));
					i--;
					src++;
					dest += 3;
				}
				break;
			case 12:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = (dest[1] = (dest[2] = *src));
					dest[3] = ushort.MaxValue;
					i--;
					src++;
					dest += 4;
				}
				break;
			case 17:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = *src;
					i--;
					src += 2;
					dest++;
				}
				break;
			case 19:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = (dest[1] = (dest[2] = *src));
					i--;
					src += 2;
					dest += 3;
				}
				break;
			case 20:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = (dest[1] = (dest[2] = *src));
					dest[3] = src[1];
					i--;
					src += 2;
					dest += 4;
				}
				break;
			case 28:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = *src;
					dest[1] = src[1];
					dest[2] = src[2];
					dest[3] = ushort.MaxValue;
					i--;
					src += 3;
					dest += 4;
				}
				break;
			case 25:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = StbImage.stbi__compute_y_16(*src, src[1], src[2]);
					i--;
					src += 3;
					dest++;
				}
				break;
			case 26:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = StbImage.stbi__compute_y_16(*src, src[1], src[2]);
					dest[1] = ushort.MaxValue;
					i--;
					src += 3;
					dest += 2;
				}
				break;
			case 33:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = StbImage.stbi__compute_y_16(*src, src[1], src[2]);
					i--;
					src += 4;
					dest++;
				}
				break;
			case 34:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = StbImage.stbi__compute_y_16(*src, src[1], src[2]);
					dest[1] = src[3];
					i--;
					src += 4;
					dest += 2;
				}
				break;
			case 35:
				i = (int)(x - 1);
				while (i >= 0)
				{
					*dest = *src;
					dest[1] = src[1];
					dest[2] = src[2];
					i--;
					src += 4;
					dest += 3;
				}
				break;
			default:
				return (ushort*)((StbImage.stbi__err("0") != 0) ? 0u : 0u);
			}
		}
		CRuntime.free(data);
		return good;
	}

	public unsafe static int stbi__build_huffman(stbi__huffman h, int* count)
	{
		int i = 0;
		int j = 0;
		int k = 0;
		uint code = 0u;
		for (i = 0; i < 16; i++)
		{
			for (j = 0; j < count[i]; j++)
			{
				h.size[k++] = (byte)(i + 1);
			}
		}
		h.size[k] = 0;
		code = 0u;
		k = 0;
		for (j = 1; j <= 16; j++)
		{
			h.delta[j] = (int)(k - code);
			if (h.size[k] == j)
			{
				while (h.size[k] == j)
				{
					h.code[k++] = (ushort)code++;
				}
				if (code - 1 >= (uint)(1 << j))
				{
					return StbImage.stbi__err("bad code lengths");
				}
			}
			h.maxcode[j] = code << 16 - j;
			code <<= 1;
		}
		h.maxcode[j] = uint.MaxValue;
		CRuntime.SetArray(h.fast, byte.MaxValue);
		for (i = 0; i < k; i++)
		{
			int s = h.size[i];
			if (s <= 9)
			{
				int c = h.code[i] << 9 - s;
				int m = 1 << 9 - s;
				for (j = 0; j < m; j++)
				{
					h.fast[c + j] = (byte)i;
				}
			}
		}
		return 1;
	}

	public static void stbi__build_fast_ac(short[] fast_ac, stbi__huffman h)
	{
		int i = 0;
		for (i = 0; i < 512; i++)
		{
			byte fast = h.fast[i];
			fast_ac[i] = 0;
			if (fast >= byte.MaxValue)
			{
				continue;
			}
			byte num = h.values[fast];
			int run = (num >> 4) & 0xF;
			int magbits = num & 0xF;
			int len = h.size[fast];
			if (magbits != 0 && len + magbits <= 9)
			{
				int k = ((i << len) & 0x1FF) >> 9 - magbits;
				int m = 1 << magbits - 1;
				if (k < m)
				{
					k += (-1 << magbits) + 1;
				}
				if (k >= -128 && k <= 127)
				{
					fast_ac[i] = (short)(k * 256 + run * 16 + (len + magbits));
				}
			}
		}
	}

	public static void stbi__grow_buffer_unsafe(stbi__jpeg j)
	{
		do
		{
			uint b = (uint)((j.nomore == 0) ? StbImage.stbi__get8(j.s) : 0);
			if (b == 255)
			{
				int c = StbImage.stbi__get8(j.s);
				while (true)
				{
					switch (c)
					{
					case 255:
						goto IL_002d;
					default:
						j.marker = (byte)c;
						j.nomore = 1;
						return;
					case 0:
						break;
					}
					break;
					IL_002d:
					c = StbImage.stbi__get8(j.s);
				}
			}
			j.code_buffer |= b << 24 - j.code_bits;
			j.code_bits += 8;
		}
		while (j.code_bits <= 24);
	}

	public static int stbi__jpeg_huff_decode(stbi__jpeg j, stbi__huffman h)
	{
		uint temp = 0u;
		int c = 0;
		int k = 0;
		if (j.code_bits < 16)
		{
			StbImage.stbi__grow_buffer_unsafe(j);
		}
		c = (int)((j.code_buffer >> 23) & 0x1FF);
		k = h.fast[c];
		if (k < 255)
		{
			int s = h.size[k];
			if (s > j.code_bits)
			{
				return -1;
			}
			j.code_buffer <<= s;
			j.code_bits -= s;
			return h.values[k];
		}
		temp = j.code_buffer >> 16;
		for (k = 10; temp >= h.maxcode[k]; k++)
		{
		}
		if (k == 17)
		{
			j.code_bits -= 16;
			return -1;
		}
		if (k > j.code_bits)
		{
			return -1;
		}
		c = (int)(((j.code_buffer >> 32 - k) & StbImage.stbi__bmask[k]) + h.delta[k]);
		j.code_bits -= k;
		j.code_buffer <<= k;
		return h.values[c];
	}

	public static int stbi__extend_receive(stbi__jpeg j, int n)
	{
		uint k = 0u;
		int sgn = 0;
		if (j.code_bits < n)
		{
			StbImage.stbi__grow_buffer_unsafe(j);
		}
		sgn = (int)j.code_buffer >> 31;
		k = CRuntime._lrotl(j.code_buffer, n);
		j.code_buffer = k & ~StbImage.stbi__bmask[n];
		k &= StbImage.stbi__bmask[n];
		j.code_bits -= n;
		return (int)(k + (StbImage.stbi__jbias[n] & ~sgn));
	}

	public static int stbi__jpeg_get_bits(stbi__jpeg j, int n)
	{
		uint k = 0u;
		if (j.code_bits < n)
		{
			StbImage.stbi__grow_buffer_unsafe(j);
		}
		k = CRuntime._lrotl(j.code_buffer, n);
		j.code_buffer = k & ~StbImage.stbi__bmask[n];
		k &= StbImage.stbi__bmask[n];
		j.code_bits -= n;
		return (int)k;
	}

	public static int stbi__jpeg_get_bit(stbi__jpeg j)
	{
		if (j.code_bits < 1)
		{
			StbImage.stbi__grow_buffer_unsafe(j);
		}
		uint code_buffer = j.code_buffer;
		j.code_buffer <<= 1;
		j.code_bits--;
		return (int)code_buffer & int.MinValue;
	}

	public unsafe static int stbi__jpeg_decode_block(stbi__jpeg j, short* data, stbi__huffman hdc, stbi__huffman hac, short[] fac, int b, ushort[] dequant)
	{
		int diff = 0;
		int dc = 0;
		int k = 0;
		int t = 0;
		if (j.code_bits < 16)
		{
			StbImage.stbi__grow_buffer_unsafe(j);
		}
		t = StbImage.stbi__jpeg_huff_decode(j, hdc);
		if (t < 0)
		{
			return StbImage.stbi__err("bad huffman code");
		}
		CRuntime.memset(data, 0, 128uL);
		diff = ((t != 0) ? StbImage.stbi__extend_receive(j, t) : 0);
		dc = j.img_comp[b].dc_pred + diff;
		j.img_comp[b].dc_pred = dc;
		*data = (short)(dc * dequant[0]);
		k = 1;
		do
		{
			uint zig = 0u;
			int c = 0;
			int r = 0;
			int s = 0;
			if (j.code_bits < 16)
			{
				StbImage.stbi__grow_buffer_unsafe(j);
			}
			c = (int)((j.code_buffer >> 23) & 0x1FF);
			r = fac[c];
			if (r != 0)
			{
				k += (r >> 4) & 0xF;
				s = r & 0xF;
				j.code_buffer <<= s;
				j.code_bits -= s;
				zig = StbImage.stbi__jpeg_dezigzag[k++];
				data[zig] = (short)((r >> 8) * dequant[zig]);
				continue;
			}
			int rs = StbImage.stbi__jpeg_huff_decode(j, hac);
			if (rs < 0)
			{
				return StbImage.stbi__err("bad huffman code");
			}
			s = rs & 0xF;
			r = rs >> 4;
			if (s == 0)
			{
				if (rs != 240)
				{
					break;
				}
				k += 16;
			}
			else
			{
				k += r;
				zig = StbImage.stbi__jpeg_dezigzag[k++];
				data[zig] = (short)(StbImage.stbi__extend_receive(j, s) * dequant[zig]);
			}
		}
		while (k < 64);
		return 1;
	}

	public unsafe static int stbi__jpeg_decode_block_prog_dc(stbi__jpeg j, short* data, stbi__huffman hdc, int b)
	{
		int diff = 0;
		int dc = 0;
		int t = 0;
		if (j.spec_end != 0)
		{
			return StbImage.stbi__err("can't merge dc and ac");
		}
		if (j.code_bits < 16)
		{
			StbImage.stbi__grow_buffer_unsafe(j);
		}
		if (j.succ_high == 0)
		{
			CRuntime.memset(data, 0, 128uL);
			t = StbImage.stbi__jpeg_huff_decode(j, hdc);
			diff = ((t != 0) ? StbImage.stbi__extend_receive(j, t) : 0);
			dc = j.img_comp[b].dc_pred + diff;
			j.img_comp[b].dc_pred = dc;
			*data = (short)(dc << j.succ_low);
		}
		else if (StbImage.stbi__jpeg_get_bit(j) != 0)
		{
			*data += (short)(1 << j.succ_low);
		}
		return 1;
	}

	public unsafe static int stbi__jpeg_decode_block_prog_ac(stbi__jpeg j, short* data, stbi__huffman hac, short[] fac)
	{
		int k = 0;
		if (j.spec_start == 0)
		{
			return StbImage.stbi__err("can't merge dc and ac");
		}
		if (j.succ_high == 0)
		{
			int shift = j.succ_low;
			if (j.eob_run != 0)
			{
				j.eob_run--;
				return 1;
			}
			k = j.spec_start;
			do
			{
				uint zig = 0u;
				int c = 0;
				int r = 0;
				int s = 0;
				if (j.code_bits < 16)
				{
					StbImage.stbi__grow_buffer_unsafe(j);
				}
				c = (int)((j.code_buffer >> 23) & 0x1FF);
				r = fac[c];
				if (r != 0)
				{
					k += (r >> 4) & 0xF;
					s = r & 0xF;
					j.code_buffer <<= s;
					j.code_bits -= s;
					zig = StbImage.stbi__jpeg_dezigzag[k++];
					data[zig] = (short)(r >> 8 << shift);
					continue;
				}
				int rs = StbImage.stbi__jpeg_huff_decode(j, hac);
				if (rs < 0)
				{
					return StbImage.stbi__err("bad huffman code");
				}
				s = rs & 0xF;
				r = rs >> 4;
				if (s == 0)
				{
					if (r < 15)
					{
						j.eob_run = 1 << r;
						if (r != 0)
						{
							j.eob_run += StbImage.stbi__jpeg_get_bits(j, r);
						}
						j.eob_run--;
						break;
					}
					k += 16;
				}
				else
				{
					k += r;
					zig = StbImage.stbi__jpeg_dezigzag[k++];
					data[zig] = (short)(StbImage.stbi__extend_receive(j, s) << shift);
				}
			}
			while (k <= j.spec_end);
		}
		else
		{
			short bit = (short)(1 << j.succ_low);
			if (j.eob_run != 0)
			{
				j.eob_run--;
				for (k = j.spec_start; k <= j.spec_end; k++)
				{
					short* p = data + (int)StbImage.stbi__jpeg_dezigzag[k];
					if (*p != 0 && StbImage.stbi__jpeg_get_bit(j) != 0 && (*p & bit) == 0)
					{
						if (*p > 0)
						{
							*p += bit;
						}
						else
						{
							*p -= bit;
						}
					}
				}
			}
			else
			{
				k = j.spec_start;
				do
				{
					int r2 = 0;
					int s2 = 0;
					int rs2 = StbImage.stbi__jpeg_huff_decode(j, hac);
					if (rs2 < 0)
					{
						return StbImage.stbi__err("bad huffman code");
					}
					s2 = rs2 & 0xF;
					r2 = rs2 >> 4;
					switch (s2)
					{
					case 0:
						if (r2 < 15)
						{
							j.eob_run = (1 << r2) - 1;
							if (r2 != 0)
							{
								j.eob_run += StbImage.stbi__jpeg_get_bits(j, r2);
							}
							r2 = 64;
						}
						break;
					default:
						return StbImage.stbi__err("bad huffman code");
					case 1:
						s2 = ((StbImage.stbi__jpeg_get_bit(j) == 0) ? (-bit) : bit);
						break;
					}
					while (k <= j.spec_end)
					{
						short* p2 = data + (int)StbImage.stbi__jpeg_dezigzag[k++];
						if (*p2 != 0)
						{
							if (StbImage.stbi__jpeg_get_bit(j) != 0 && (*p2 & bit) == 0)
							{
								if (*p2 > 0)
								{
									*p2 += bit;
								}
								else
								{
									*p2 -= bit;
								}
							}
						}
						else
						{
							if (r2 == 0)
							{
								*p2 = (short)s2;
								break;
							}
							r2--;
						}
					}
				}
				while (k <= j.spec_end);
			}
		}
		return 1;
	}

	public static byte stbi__clamp(int x)
	{
		if ((uint)x > 255u)
		{
			if (x < 0)
			{
				return 0;
			}
			if (x > 255)
			{
				return byte.MaxValue;
			}
		}
		return (byte)x;
	}

	public unsafe static void stbi__idct_block(byte* _out_, int out_stride, short* data)
	{
		int i = 0;
		int* val = stackalloc int[64];
		int* v = val;
		short* d = data;
		i = 0;
		while (i < 8)
		{
			if (d[8] == 0 && d[16] == 0 && d[24] == 0 && d[32] == 0 && d[40] == 0 && d[48] == 0 && d[56] == 0)
			{
				int dcterm = *d * 4;
				*v = (v[8] = (v[16] = (v[24] = (v[32] = (v[40] = (v[48] = (v[56] = dcterm)))))));
			}
			else
			{
				int t0 = 0;
				int t1 = 0;
				int t2 = 0;
				int t3 = 0;
				int p1 = 0;
				int p2 = 0;
				int p3 = 0;
				int p4 = 0;
				int x0 = 0;
				int x1 = 0;
				int x2 = 0;
				int x3 = 0;
				p2 = d[16];
				p3 = d[48];
				p1 = (p2 + p3) * 2217;
				t2 = p1 + p3 * -7567;
				t3 = p1 + p2 * 3135;
				p2 = *d;
				p3 = d[32];
				t0 = (p2 + p3) * 4096;
				t1 = (p2 - p3) * 4096;
				x0 = t0 + t3;
				x3 = t0 - t3;
				x1 = t1 + t2;
				x2 = t1 - t2;
				t0 = d[56];
				t1 = d[40];
				t2 = d[24];
				t3 = d[8];
				p3 = t0 + t2;
				p4 = t1 + t3;
				p1 = t0 + t3;
				p2 = t1 + t2;
				int num = (p3 + p4) * 4816;
				t0 *= 1223;
				t1 *= 8410;
				t2 *= 12586;
				t3 *= 6149;
				p1 = num + p1 * -3685;
				p2 = num + p2 * -10497;
				p3 *= -8034;
				p4 *= -1597;
				t3 += p1 + p4;
				t2 += p2 + p3;
				t1 += p2 + p4;
				t0 += p1 + p3;
				x0 += 512;
				x1 += 512;
				x2 += 512;
				x3 += 512;
				*v = x0 + t3 >> 10;
				v[56] = x0 - t3 >> 10;
				v[8] = x1 + t2 >> 10;
				v[48] = x1 - t2 >> 10;
				v[16] = x2 + t1 >> 10;
				v[40] = x2 - t1 >> 10;
				v[24] = x3 + t0 >> 10;
				v[32] = x3 - t0 >> 10;
			}
			i++;
			d++;
			v++;
		}
		i = 0;
		v = val;
		byte* o = _out_;
		while (i < 8)
		{
			int t4 = 0;
			int t5 = 0;
			int t6 = 0;
			int t7 = 0;
			int p5 = 0;
			int p6 = 0;
			int p7 = 0;
			int p8 = 0;
			int x4 = 0;
			int x5 = 0;
			int x6 = 0;
			int x7 = 0;
			p6 = v[2];
			p7 = v[6];
			p5 = (p6 + p7) * 2217;
			t6 = p5 + p7 * -7567;
			t7 = p5 + p6 * 3135;
			p6 = *v;
			p7 = v[4];
			t4 = (p6 + p7) * 4096;
			t5 = (p6 - p7) * 4096;
			x4 = t4 + t7;
			x7 = t4 - t7;
			x5 = t5 + t6;
			x6 = t5 - t6;
			t4 = v[7];
			t5 = v[5];
			t6 = v[3];
			t7 = v[1];
			p7 = t4 + t6;
			p8 = t5 + t7;
			p5 = t4 + t7;
			p6 = t5 + t6;
			int num2 = (p7 + p8) * 4816;
			t4 *= 1223;
			t5 *= 8410;
			t6 *= 12586;
			t7 *= 6149;
			p5 = num2 + p5 * -3685;
			p6 = num2 + p6 * -10497;
			p7 *= -8034;
			p8 *= -1597;
			t7 += p5 + p8;
			t6 += p6 + p7;
			t5 += p6 + p8;
			t4 += p5 + p7;
			x4 += 16842752;
			x5 += 16842752;
			x6 += 16842752;
			x7 += 16842752;
			*o = StbImage.stbi__clamp(x4 + t7 >> 17);
			o[7] = StbImage.stbi__clamp(x4 - t7 >> 17);
			o[1] = StbImage.stbi__clamp(x5 + t6 >> 17);
			o[6] = StbImage.stbi__clamp(x5 - t6 >> 17);
			o[2] = StbImage.stbi__clamp(x6 + t5 >> 17);
			o[5] = StbImage.stbi__clamp(x6 - t5 >> 17);
			o[3] = StbImage.stbi__clamp(x7 + t4 >> 17);
			o[4] = StbImage.stbi__clamp(x7 - t4 >> 17);
			i++;
			v += 8;
			o += out_stride;
		}
	}

	public static byte stbi__get_marker(stbi__jpeg j)
	{
		byte x = 0;
		if (j.marker != byte.MaxValue)
		{
			x = j.marker;
			j.marker = byte.MaxValue;
			return x;
		}
		x = StbImage.stbi__get8(j.s);
		if (x != byte.MaxValue)
		{
			return byte.MaxValue;
		}
		while (x == byte.MaxValue)
		{
			x = StbImage.stbi__get8(j.s);
		}
		return x;
	}

	public static void stbi__jpeg_reset(stbi__jpeg j)
	{
		j.code_bits = 0;
		j.code_buffer = 0u;
		j.nomore = 0;
		j.img_comp[0].dc_pred = (j.img_comp[1].dc_pred = (j.img_comp[2].dc_pred = (j.img_comp[3].dc_pred = 0)));
		j.marker = byte.MaxValue;
		j.todo = ((j.restart_interval != 0) ? j.restart_interval : int.MaxValue);
		j.eob_run = 0;
	}

	public unsafe static int stbi__parse_entropy_coded_data(stbi__jpeg z)
	{
		StbImage.stbi__jpeg_reset(z);
		if (z.progressive == 0)
		{
			if (z.scan_n == 1)
			{
				int i = 0;
				int j = 0;
				short* data = stackalloc short[64];
				int n = z.order[0];
				int w = z.img_comp[n].x + 7 >> 3;
				int h = z.img_comp[n].y + 7 >> 3;
				for (j = 0; j < h; j++)
				{
					for (i = 0; i < w; i++)
					{
						int ha = z.img_comp[n].ha;
						if (StbImage.stbi__jpeg_decode_block(z, data, z.huff_dc[z.img_comp[n].hd], z.huff_ac[ha], z.fast_ac[ha], n, z.dequant[z.img_comp[n].tq]) == 0)
						{
							return 0;
						}
						z.idct_block_kernel(z.img_comp[n].data + z.img_comp[n].w2 * j * 8 + i * 8, z.img_comp[n].w2, data);
						if (--z.todo <= 0)
						{
							if (z.code_bits < 24)
							{
								StbImage.stbi__grow_buffer_unsafe(z);
							}
							if (z.marker < 208 || z.marker > 215)
							{
								return 1;
							}
							StbImage.stbi__jpeg_reset(z);
						}
					}
				}
				return 1;
			}
			int i2 = 0;
			int j2 = 0;
			int k = 0;
			int x = 0;
			int y = 0;
			short* data2 = stackalloc short[64];
			for (j2 = 0; j2 < z.img_mcu_y; j2++)
			{
				for (i2 = 0; i2 < z.img_mcu_x; i2++)
				{
					for (k = 0; k < z.scan_n; k++)
					{
						int n2 = z.order[k];
						for (y = 0; y < z.img_comp[n2].v; y++)
						{
							for (x = 0; x < z.img_comp[n2].h; x++)
							{
								int x2 = (i2 * z.img_comp[n2].h + x) * 8;
								int y2 = (j2 * z.img_comp[n2].v + y) * 8;
								int ha2 = z.img_comp[n2].ha;
								if (StbImage.stbi__jpeg_decode_block(z, data2, z.huff_dc[z.img_comp[n2].hd], z.huff_ac[ha2], z.fast_ac[ha2], n2, z.dequant[z.img_comp[n2].tq]) == 0)
								{
									return 0;
								}
								z.idct_block_kernel(z.img_comp[n2].data + z.img_comp[n2].w2 * y2 + x2, z.img_comp[n2].w2, data2);
							}
						}
					}
					if (--z.todo <= 0)
					{
						if (z.code_bits < 24)
						{
							StbImage.stbi__grow_buffer_unsafe(z);
						}
						if (z.marker < 208 || z.marker > 215)
						{
							return 1;
						}
						StbImage.stbi__jpeg_reset(z);
					}
				}
			}
			return 1;
		}
		if (z.scan_n == 1)
		{
			int i3 = 0;
			int j3 = 0;
			int n3 = z.order[0];
			int w2 = z.img_comp[n3].x + 7 >> 3;
			int h2 = z.img_comp[n3].y + 7 >> 3;
			for (j3 = 0; j3 < h2; j3++)
			{
				for (i3 = 0; i3 < w2; i3++)
				{
					short* data3 = z.img_comp[n3].coeff + 64 * (i3 + j3 * z.img_comp[n3].coeff_w);
					if (z.spec_start == 0)
					{
						if (StbImage.stbi__jpeg_decode_block_prog_dc(z, data3, z.huff_dc[z.img_comp[n3].hd], n3) == 0)
						{
							return 0;
						}
					}
					else
					{
						int ha3 = z.img_comp[n3].ha;
						if (StbImage.stbi__jpeg_decode_block_prog_ac(z, data3, z.huff_ac[ha3], z.fast_ac[ha3]) == 0)
						{
							return 0;
						}
					}
					if (--z.todo <= 0)
					{
						if (z.code_bits < 24)
						{
							StbImage.stbi__grow_buffer_unsafe(z);
						}
						if (z.marker < 208 || z.marker > 215)
						{
							return 1;
						}
						StbImage.stbi__jpeg_reset(z);
					}
				}
			}
			return 1;
		}
		int i4 = 0;
		int j4 = 0;
		int k2 = 0;
		int x3 = 0;
		int y3 = 0;
		for (j4 = 0; j4 < z.img_mcu_y; j4++)
		{
			for (i4 = 0; i4 < z.img_mcu_x; i4++)
			{
				for (k2 = 0; k2 < z.scan_n; k2++)
				{
					int n4 = z.order[k2];
					for (y3 = 0; y3 < z.img_comp[n4].v; y3++)
					{
						for (x3 = 0; x3 < z.img_comp[n4].h; x3++)
						{
							int x4 = i4 * z.img_comp[n4].h + x3;
							int y4 = j4 * z.img_comp[n4].v + y3;
							short* data4 = z.img_comp[n4].coeff + 64 * (x4 + y4 * z.img_comp[n4].coeff_w);
							if (StbImage.stbi__jpeg_decode_block_prog_dc(z, data4, z.huff_dc[z.img_comp[n4].hd], n4) == 0)
							{
								return 0;
							}
						}
					}
				}
				if (--z.todo <= 0)
				{
					if (z.code_bits < 24)
					{
						StbImage.stbi__grow_buffer_unsafe(z);
					}
					if (z.marker < 208 || z.marker > 215)
					{
						return 1;
					}
					StbImage.stbi__jpeg_reset(z);
				}
			}
		}
		return 1;
	}

	public unsafe static void stbi__jpeg_dequantize(short* data, ushort[] dequant)
	{
		int i = 0;
		for (i = 0; i < 64; i++)
		{
			data[i] *= (short)dequant[i];
		}
	}

	public unsafe static void stbi__jpeg_finish(stbi__jpeg z)
	{
		if (z.progressive == 0)
		{
			return;
		}
		int i = 0;
		int j = 0;
		int n = 0;
		for (n = 0; n < z.s.img_n; n++)
		{
			int w = z.img_comp[n].x + 7 >> 3;
			int h = z.img_comp[n].y + 7 >> 3;
			for (j = 0; j < h; j++)
			{
				for (i = 0; i < w; i++)
				{
					short* data = z.img_comp[n].coeff + 64 * (i + j * z.img_comp[n].coeff_w);
					StbImage.stbi__jpeg_dequantize(data, z.dequant[z.img_comp[n].tq]);
					z.idct_block_kernel(z.img_comp[n].data + z.img_comp[n].w2 * j * 8 + i * 8, z.img_comp[n].w2, data);
				}
			}
		}
	}

	public unsafe static int stbi__process_marker(stbi__jpeg z, int m)
	{
		int L = 0;
		switch (m)
		{
		case 255:
			return StbImage.stbi__err("expected marker");
		case 221:
			if (StbImage.stbi__get16be(z.s) != 4)
			{
				return StbImage.stbi__err("bad DRI len");
			}
			z.restart_interval = StbImage.stbi__get16be(z.s);
			return 1;
		case 219:
			L = StbImage.stbi__get16be(z.s) - 2;
			while (L > 0)
			{
				byte num2 = StbImage.stbi__get8(z.s);
				int p = num2 >> 4;
				int sixteen = ((p != 0) ? 1 : 0);
				int t = num2 & 0xF;
				int i4 = 0;
				if (p != 0 && p != 1)
				{
					return StbImage.stbi__err("bad DQT type");
				}
				if (t > 3)
				{
					return StbImage.stbi__err("bad DQT table");
				}
				for (i4 = 0; i4 < 64; i4++)
				{
					z.dequant[t][StbImage.stbi__jpeg_dezigzag[i4]] = (ushort)((sixteen != 0) ? StbImage.stbi__get16be(z.s) : StbImage.stbi__get8(z.s));
				}
				L -= ((sixteen != 0) ? 129 : 65);
			}
			return (L == 0) ? 1 : 0;
		case 196:
			L = StbImage.stbi__get16be(z.s) - 2;
			while (L > 0)
			{
				int* sizes = stackalloc int[16];
				int i3 = 0;
				int n = 0;
				byte num = StbImage.stbi__get8(z.s);
				int tc = num >> 4;
				int th = num & 0xF;
				if (tc > 1 || th > 3)
				{
					return StbImage.stbi__err("bad DHT header");
				}
				for (i3 = 0; i3 < 16; i3++)
				{
					sizes[i3] = StbImage.stbi__get8(z.s);
					n += sizes[i3];
				}
				L -= 17;
				byte[] v;
				if (tc == 0)
				{
					if (StbImage.stbi__build_huffman(z.huff_dc[th], sizes) == 0)
					{
						return 0;
					}
					v = z.huff_dc[th].values;
				}
				else
				{
					if (StbImage.stbi__build_huffman(z.huff_ac[th], sizes) == 0)
					{
						return 0;
					}
					v = z.huff_ac[th].values;
				}
				for (i3 = 0; i3 < n; i3++)
				{
					v[i3] = StbImage.stbi__get8(z.s);
				}
				if (tc != 0)
				{
					StbImage.stbi__build_fast_ac(z.fast_ac[th], z.huff_ac[th]);
				}
				L -= n;
			}
			return (L == 0) ? 1 : 0;
		default:
			if ((m >= 224 && m <= 239) || m == 254)
			{
				L = StbImage.stbi__get16be(z.s);
				if (L < 2)
				{
					if (m == 254)
					{
						return StbImage.stbi__err("bad COM len");
					}
					return StbImage.stbi__err("bad APP len");
				}
				L -= 2;
				if (m == 224 && L >= 5)
				{
					byte* tag = stackalloc byte[5];
					*tag = 74;
					tag[1] = 70;
					tag[2] = 73;
					tag[3] = 70;
					tag[4] = 0;
					int ok = 1;
					int i = 0;
					for (i = 0; i < 5; i++)
					{
						if (StbImage.stbi__get8(z.s) != tag[i])
						{
							ok = 0;
						}
					}
					L -= 5;
					if (ok != 0)
					{
						z.jfif = 1;
					}
				}
				else if (m == 238 && L >= 12)
				{
					byte* tag2 = stackalloc byte[6];
					*tag2 = 65;
					tag2[1] = 100;
					tag2[2] = 111;
					tag2[3] = 98;
					tag2[4] = 101;
					tag2[5] = 0;
					int ok2 = 1;
					int i2 = 0;
					for (i2 = 0; i2 < 6; i2++)
					{
						if (StbImage.stbi__get8(z.s) != tag2[i2])
						{
							ok2 = 0;
						}
					}
					L -= 6;
					if (ok2 != 0)
					{
						StbImage.stbi__get8(z.s);
						StbImage.stbi__get16be(z.s);
						StbImage.stbi__get16be(z.s);
						z.app14_color_transform = StbImage.stbi__get8(z.s);
						L -= 6;
					}
				}
				StbImage.stbi__skip(z.s, L);
				return 1;
			}
			return StbImage.stbi__err("unknown marker");
		}
	}

	public static int stbi__process_scan_header(stbi__jpeg z)
	{
		int i = 0;
		int Ls = StbImage.stbi__get16be(z.s);
		z.scan_n = StbImage.stbi__get8(z.s);
		if (z.scan_n < 1 || z.scan_n > 4 || z.scan_n > z.s.img_n)
		{
			return StbImage.stbi__err("bad SOS component count");
		}
		if (Ls != 6 + 2 * z.scan_n)
		{
			return StbImage.stbi__err("bad SOS len");
		}
		for (i = 0; i < z.scan_n; i++)
		{
			int id = StbImage.stbi__get8(z.s);
			int which = 0;
			int q = StbImage.stbi__get8(z.s);
			for (which = 0; which < z.s.img_n && z.img_comp[which].id != id; which++)
			{
			}
			if (which == z.s.img_n)
			{
				return 0;
			}
			z.img_comp[which].hd = q >> 4;
			if (z.img_comp[which].hd > 3)
			{
				return StbImage.stbi__err("bad DC huff");
			}
			z.img_comp[which].ha = q & 0xF;
			if (z.img_comp[which].ha > 3)
			{
				return StbImage.stbi__err("bad AC huff");
			}
			z.order[i] = which;
		}
		int aa = 0;
		z.spec_start = StbImage.stbi__get8(z.s);
		z.spec_end = StbImage.stbi__get8(z.s);
		aa = StbImage.stbi__get8(z.s);
		z.succ_high = aa >> 4;
		z.succ_low = aa & 0xF;
		if (z.progressive != 0)
		{
			if (z.spec_start > 63 || z.spec_end > 63 || z.spec_start > z.spec_end || z.succ_high > 13 || z.succ_low > 13)
			{
				return StbImage.stbi__err("bad SOS");
			}
		}
		else
		{
			if (z.spec_start != 0)
			{
				return StbImage.stbi__err("bad SOS");
			}
			if (z.succ_high != 0 || z.succ_low != 0)
			{
				return StbImage.stbi__err("bad SOS");
			}
			z.spec_end = 63;
		}
		return 1;
	}

	public unsafe static int stbi__free_jpeg_components(stbi__jpeg z, int ncomp, int why)
	{
		int i = 0;
		for (i = 0; i < ncomp; i++)
		{
			if (z.img_comp[i].raw_data != null)
			{
				CRuntime.free(z.img_comp[i].raw_data);
				z.img_comp[i].raw_data = null;
				z.img_comp[i].data = null;
			}
			if (z.img_comp[i].raw_coeff != null)
			{
				CRuntime.free(z.img_comp[i].raw_coeff);
				z.img_comp[i].raw_coeff = null;
				z.img_comp[i].coeff = null;
			}
			if (z.img_comp[i].linebuf != null)
			{
				CRuntime.free(z.img_comp[i].linebuf);
				z.img_comp[i].linebuf = null;
			}
		}
		return why;
	}

	public unsafe static int stbi__process_frame_header(stbi__jpeg z, int scan)
	{
		stbi__context s = z.s;
		int Lf = 0;
		int i = 0;
		int q = 0;
		int h_max = 1;
		int v_max = 1;
		int c = 0;
		Lf = StbImage.stbi__get16be(s);
		if (Lf < 11)
		{
			return StbImage.stbi__err("bad SOF len");
		}
		if (StbImage.stbi__get8(s) != 8)
		{
			return StbImage.stbi__err("only 8-bit");
		}
		s.img_y = (uint)StbImage.stbi__get16be(s);
		if (s.img_y == 0)
		{
			return StbImage.stbi__err("no header height");
		}
		s.img_x = (uint)StbImage.stbi__get16be(s);
		if (s.img_x == 0)
		{
			return StbImage.stbi__err("0 width");
		}
		c = StbImage.stbi__get8(s);
		if (c != 3 && c != 1 && c != 4)
		{
			return StbImage.stbi__err("bad component count");
		}
		s.img_n = c;
		for (i = 0; i < c; i++)
		{
			z.img_comp[i].data = null;
			z.img_comp[i].linebuf = null;
		}
		if (Lf != 8 + 3 * s.img_n)
		{
			return StbImage.stbi__err("bad SOF len");
		}
		z.rgb = 0;
		for (i = 0; i < s.img_n; i++)
		{
			byte* rgb = stackalloc byte[3];
			*rgb = 82;
			rgb[1] = 71;
			rgb[2] = 66;
			z.img_comp[i].id = StbImage.stbi__get8(s);
			if (s.img_n == 3 && z.img_comp[i].id == rgb[i])
			{
				z.rgb++;
			}
			q = StbImage.stbi__get8(s);
			z.img_comp[i].h = q >> 4;
			if (z.img_comp[i].h == 0 || z.img_comp[i].h > 4)
			{
				return StbImage.stbi__err("bad H");
			}
			z.img_comp[i].v = q & 0xF;
			if (z.img_comp[i].v == 0 || z.img_comp[i].v > 4)
			{
				return StbImage.stbi__err("bad V");
			}
			z.img_comp[i].tq = StbImage.stbi__get8(s);
			if (z.img_comp[i].tq > 3)
			{
				return StbImage.stbi__err("bad TQ");
			}
		}
		if (scan != 0)
		{
			return 1;
		}
		if (StbImage.stbi__mad3sizes_valid((int)s.img_x, (int)s.img_y, s.img_n, 0) == 0)
		{
			return StbImage.stbi__err("too large");
		}
		for (i = 0; i < s.img_n; i++)
		{
			if (z.img_comp[i].h > h_max)
			{
				h_max = z.img_comp[i].h;
			}
			if (z.img_comp[i].v > v_max)
			{
				v_max = z.img_comp[i].v;
			}
		}
		z.img_h_max = h_max;
		z.img_v_max = v_max;
		z.img_mcu_w = h_max * 8;
		z.img_mcu_h = v_max * 8;
		z.img_mcu_x = (int)((s.img_x + z.img_mcu_w - 1) / z.img_mcu_w);
		z.img_mcu_y = (int)((s.img_y + z.img_mcu_h - 1) / z.img_mcu_h);
		for (i = 0; i < s.img_n; i++)
		{
			z.img_comp[i].x = (int)((s.img_x * z.img_comp[i].h + h_max - 1) / h_max);
			z.img_comp[i].y = (int)((s.img_y * z.img_comp[i].v + v_max - 1) / v_max);
			z.img_comp[i].w2 = z.img_mcu_x * z.img_comp[i].h * 8;
			z.img_comp[i].h2 = z.img_mcu_y * z.img_comp[i].v * 8;
			z.img_comp[i].coeff = null;
			z.img_comp[i].raw_coeff = null;
			z.img_comp[i].linebuf = null;
			z.img_comp[i].raw_data = StbImage.stbi__malloc_mad2(z.img_comp[i].w2, z.img_comp[i].h2, 15);
			if (z.img_comp[i].raw_data == null)
			{
				return StbImage.stbi__free_jpeg_components(z, i + 1, StbImage.stbi__err("outofmem"));
			}
			z.img_comp[i].data = (byte*)(((long)z.img_comp[i].raw_data + 15L) & -16);
			if (z.progressive != 0)
			{
				z.img_comp[i].coeff_w = z.img_comp[i].w2 / 8;
				z.img_comp[i].coeff_h = z.img_comp[i].h2 / 8;
				z.img_comp[i].raw_coeff = StbImage.stbi__malloc_mad3(z.img_comp[i].w2, z.img_comp[i].h2, 2, 15);
				if (z.img_comp[i].raw_coeff == null)
				{
					return StbImage.stbi__free_jpeg_components(z, i + 1, StbImage.stbi__err("outofmem"));
				}
				z.img_comp[i].coeff = (short*)(((long)z.img_comp[i].raw_coeff + 15L) & -16);
			}
		}
		return 1;
	}

	public static int stbi__decode_jpeg_header(stbi__jpeg z, int scan)
	{
		int m = 0;
		z.jfif = 0;
		z.app14_color_transform = -1;
		z.marker = byte.MaxValue;
		m = StbImage.stbi__get_marker(z);
		if (m != 216)
		{
			return StbImage.stbi__err("no SOI");
		}
		if (scan == 1)
		{
			return 1;
		}
		m = StbImage.stbi__get_marker(z);
		while (m != 192 && m != 193 && m != 194)
		{
			if (StbImage.stbi__process_marker(z, m) == 0)
			{
				return 0;
			}
			for (m = StbImage.stbi__get_marker(z); m == 255; m = StbImage.stbi__get_marker(z))
			{
				if (StbImage.stbi__at_eof(z.s) != 0)
				{
					return StbImage.stbi__err("no SOF");
				}
			}
		}
		z.progressive = ((m == 194) ? 1 : 0);
		if (StbImage.stbi__process_frame_header(z, scan) == 0)
		{
			return 0;
		}
		return 1;
	}

	public unsafe static int stbi__decode_jpeg_image(stbi__jpeg j)
	{
		int m = 0;
		for (m = 0; m < 4; m++)
		{
			j.img_comp[m].raw_data = null;
			j.img_comp[m].raw_coeff = null;
		}
		j.restart_interval = 0;
		if (StbImage.stbi__decode_jpeg_header(j, 0) == 0)
		{
			return 0;
		}
		m = StbImage.stbi__get_marker(j);
		while (true)
		{
			switch (m)
			{
			case 218:
				if (StbImage.stbi__process_scan_header(j) == 0)
				{
					return 0;
				}
				if (StbImage.stbi__parse_entropy_coded_data(j) == 0)
				{
					return 0;
				}
				if (j.marker != byte.MaxValue)
				{
					break;
				}
				while (StbImage.stbi__at_eof(j.s) == 0)
				{
					if (StbImage.stbi__get8(j.s) == byte.MaxValue)
					{
						j.marker = StbImage.stbi__get8(j.s);
						break;
					}
				}
				break;
			case 220:
			{
				int num = StbImage.stbi__get16be(j.s);
				uint NL = (uint)StbImage.stbi__get16be(j.s);
				if (num != 4)
				{
					return StbImage.stbi__err("bad DNL len");
				}
				if (NL != j.s.img_y)
				{
					return StbImage.stbi__err("bad DNL height");
				}
				break;
			}
			default:
				if (StbImage.stbi__process_marker(j, m) == 0)
				{
					return 0;
				}
				break;
			case 217:
				if (j.progressive != 0)
				{
					StbImage.stbi__jpeg_finish(j);
				}
				return 1;
			}
			m = StbImage.stbi__get_marker(j);
		}
	}

	public unsafe static byte* resample_row_1(byte* _out_, byte* in_near, byte* in_far, int w, int hs)
	{
		return in_near;
	}

	public unsafe static byte* stbi__resample_row_v_2(byte* _out_, byte* in_near, byte* in_far, int w, int hs)
	{
		int i = 0;
		for (i = 0; i < w; i++)
		{
			_out_[i] = (byte)(3 * in_near[i] + in_far[i] + 2 >> 2);
		}
		return _out_;
	}

	public unsafe static byte* stbi__resample_row_h_2(byte* _out_, byte* in_near, byte* in_far, int w, int hs)
	{
		int i = 0;
		if (w == 1)
		{
			*_out_ = (_out_[1] = *in_near);
			return _out_;
		}
		*_out_ = *in_near;
		_out_[1] = (byte)(*in_near * 3 + in_near[1] + 2 >> 2);
		for (i = 1; i < w - 1; i++)
		{
			int n = 3 * in_near[i] + 2;
			_out_[i * 2] = (byte)(n + in_near[i - 1] >> 2);
			_out_[i * 2 + 1] = (byte)(n + in_near[i + 1] >> 2);
		}
		_out_[i * 2] = (byte)(in_near[w - 2] * 3 + in_near[w - 1] + 2 >> 2);
		_out_[i * 2 + 1] = in_near[w - 1];
		return _out_;
	}

	public unsafe static byte* stbi__resample_row_hv_2(byte* _out_, byte* in_near, byte* in_far, int w, int hs)
	{
		int i = 0;
		int t0 = 0;
		int t1 = 0;
		if (w == 1)
		{
			*_out_ = (_out_[1] = (byte)(3 * *in_near + *in_far + 2 >> 2));
			return _out_;
		}
		t1 = 3 * *in_near + *in_far;
		*_out_ = (byte)(t1 + 2 >> 2);
		for (i = 1; i < w; i++)
		{
			t0 = t1;
			t1 = 3 * in_near[i] + in_far[i];
			_out_[i * 2 - 1] = (byte)(3 * t0 + t1 + 8 >> 4);
			_out_[i * 2] = (byte)(3 * t1 + t0 + 8 >> 4);
		}
		_out_[w * 2 - 1] = (byte)(t1 + 2 >> 2);
		return _out_;
	}

	public unsafe static byte* stbi__resample_row_generic(byte* _out_, byte* in_near, byte* in_far, int w, int hs)
	{
		int i = 0;
		int j = 0;
		for (i = 0; i < w; i++)
		{
			for (j = 0; j < hs; j++)
			{
				_out_[i * hs + j] = in_near[i];
			}
		}
		return _out_;
	}

	public unsafe static void stbi__YCbCr_to_RGB_row(byte* _out_, byte* y, byte* pcb, byte* pcr, int count, int step)
	{
		int i = 0;
		for (i = 0; i < count; i++)
		{
			int num = (y[i] << 20) + 524288;
			int r = 0;
			int g = 0;
			int b = 0;
			int cr = pcr[i] - 128;
			int cb = pcb[i] - 128;
			r = num + cr * 1470208;
			g = (int)(num + cr * -748800 + ((cb * -360960) & 0xFFFF0000u));
			b = num + cb * 1858048;
			r >>= 20;
			g >>= 20;
			b >>= 20;
			if ((uint)r > 255u)
			{
				r = ((r >= 0) ? 255 : 0);
			}
			if ((uint)g > 255u)
			{
				g = ((g >= 0) ? 255 : 0);
			}
			if ((uint)b > 255u)
			{
				b = ((b >= 0) ? 255 : 0);
			}
			*_out_ = (byte)r;
			_out_[1] = (byte)g;
			_out_[2] = (byte)b;
			_out_[3] = byte.MaxValue;
			_out_ += step;
		}
	}

	public unsafe static void stbi__setup_jpeg(stbi__jpeg j)
	{
		j.idct_block_kernel = stbi__idct_block;
		j.YCbCr_to_RGB_kernel = stbi__YCbCr_to_RGB_row;
		j.resample_row_hv_2_kernel = stbi__resample_row_hv_2;
	}

	public static void stbi__cleanup_jpeg(stbi__jpeg j)
	{
		StbImage.stbi__free_jpeg_components(j, j.s.img_n, 0);
	}

	public static byte stbi__blinn_8x8(byte x, byte y)
	{
		int num = x * y + 128;
		return (byte)((uint)(num + (num >>> 8)) >> 8);
	}

	public unsafe static byte* load_jpeg_image(stbi__jpeg z, int* out_x, int* out_y, int* comp, int req_comp)
	{
		int n = 0;
		int decode_n = 0;
		int is_rgb = 0;
		z.s.img_n = 0;
		if (req_comp < 0 || req_comp > 4)
		{
			return (byte*)((StbImage.stbi__err("bad req_comp") != 0) ? 0u : 0u);
		}
		if (StbImage.stbi__decode_jpeg_image(z) == 0)
		{
			StbImage.stbi__cleanup_jpeg(z);
			return null;
		}
		n = ((req_comp != 0) ? req_comp : ((z.s.img_n < 3) ? 1 : 3));
		is_rgb = ((z.s.img_n == 3 && (z.rgb == 3 || (z.app14_color_transform == 0 && z.jfif == 0))) ? 1 : 0);
		decode_n = ((z.s.img_n == 3 && n < 3 && is_rgb == 0) ? 1 : z.s.img_n);
		int k = 0;
		uint i = 0u;
		uint j = 0u;
		byte** coutput = stackalloc byte*[4];
		*coutput = null;
		coutput[1] = null;
		coutput[2] = null;
		coutput[3] = null;
		stbi__resample[] res_comp = new stbi__resample[4];
		for (int kkk = 0; kkk < res_comp.Length; kkk++)
		{
			res_comp[kkk] = new stbi__resample();
		}
		for (k = 0; k < decode_n; k++)
		{
			stbi__resample r = res_comp[k];
			z.img_comp[k].linebuf = (byte*)StbImage.stbi__malloc(z.s.img_x + 3);
			if (z.img_comp[k].linebuf == null)
			{
				StbImage.stbi__cleanup_jpeg(z);
				return (byte*)((StbImage.stbi__err("outofmem") != 0) ? 0u : 0u);
			}
			r.hs = z.img_h_max / z.img_comp[k].h;
			r.vs = z.img_v_max / z.img_comp[k].v;
			r.ystep = r.vs >> 1;
			r.w_lores = (int)((z.s.img_x + r.hs - 1) / r.hs);
			r.ypos = 0;
			r.line0 = (r.line1 = z.img_comp[k].data);
			if (r.hs == 1 && r.vs == 1)
			{
				r.resample = resample_row_1;
			}
			else if (r.hs == 1 && r.vs == 2)
			{
				r.resample = stbi__resample_row_v_2;
			}
			else if (r.hs == 2 && r.vs == 1)
			{
				r.resample = stbi__resample_row_h_2;
			}
			else if (r.hs == 2 && r.vs == 2)
			{
				r.resample = z.resample_row_hv_2_kernel;
			}
			else
			{
				r.resample = stbi__resample_row_generic;
			}
		}
		byte* output = (byte*)StbImage.stbi__malloc_mad3(n, (int)z.s.img_x, (int)z.s.img_y, 1);
		if (output == null)
		{
			StbImage.stbi__cleanup_jpeg(z);
			return (byte*)((StbImage.stbi__err("outofmem") != 0) ? 0u : 0u);
		}
		for (j = 0u; j < z.s.img_y; j++)
		{
			byte* _out_ = output + n * z.s.img_x * j;
			for (k = 0; k < decode_n; k++)
			{
				stbi__resample r2 = res_comp[k];
				int y_bot = ((r2.ystep >= r2.vs >> 1) ? 1 : 0);
				coutput[k] = r2.resample(z.img_comp[k].linebuf, (y_bot != 0) ? r2.line1 : r2.line0, (y_bot != 0) ? r2.line0 : r2.line1, r2.w_lores, r2.hs);
				if (++r2.ystep >= r2.vs)
				{
					r2.ystep = 0;
					r2.line0 = r2.line1;
					if (++r2.ypos < z.img_comp[k].y)
					{
						r2.line1 += z.img_comp[k].w2;
					}
				}
			}
			if (n >= 3)
			{
				byte* y = *coutput;
				if (z.s.img_n == 3)
				{
					if (is_rgb != 0)
					{
						for (i = 0u; i < z.s.img_x; i++)
						{
							*_out_ = y[i];
							_out_[1] = coutput[1][i];
							_out_[2] = coutput[2][i];
							_out_[3] = byte.MaxValue;
							_out_ += n;
						}
					}
					else
					{
						z.YCbCr_to_RGB_kernel(_out_, y, coutput[1], coutput[2], (int)z.s.img_x, n);
					}
				}
				else if (z.s.img_n == 4)
				{
					if (z.app14_color_transform == 0)
					{
						for (i = 0u; i < z.s.img_x; i++)
						{
							byte m = coutput[3][i];
							*_out_ = StbImage.stbi__blinn_8x8((*coutput)[i], m);
							_out_[1] = StbImage.stbi__blinn_8x8(coutput[1][i], m);
							_out_[2] = StbImage.stbi__blinn_8x8(coutput[2][i], m);
							_out_[3] = byte.MaxValue;
							_out_ += n;
						}
					}
					else if (z.app14_color_transform == 2)
					{
						z.YCbCr_to_RGB_kernel(_out_, y, coutput[1], coutput[2], (int)z.s.img_x, n);
						for (i = 0u; i < z.s.img_x; i++)
						{
							byte m2 = coutput[3][i];
							*_out_ = StbImage.stbi__blinn_8x8((byte)(255 - *_out_), m2);
							_out_[1] = StbImage.stbi__blinn_8x8((byte)(255 - _out_[1]), m2);
							_out_[2] = StbImage.stbi__blinn_8x8((byte)(255 - _out_[2]), m2);
							_out_ += n;
						}
					}
					else
					{
						z.YCbCr_to_RGB_kernel(_out_, y, coutput[1], coutput[2], (int)z.s.img_x, n);
					}
				}
				else
				{
					for (i = 0u; i < z.s.img_x; i++)
					{
						*_out_ = (_out_[1] = (_out_[2] = y[i]));
						_out_[3] = byte.MaxValue;
						_out_ += n;
					}
				}
				continue;
			}
			if (is_rgb != 0)
			{
				if (n == 1)
				{
					for (i = 0u; i < z.s.img_x; i++)
					{
						*(_out_++) = StbImage.stbi__compute_y((*coutput)[i], coutput[1][i], coutput[2][i]);
					}
					continue;
				}
				i = 0u;
				while (i < z.s.img_x)
				{
					*_out_ = StbImage.stbi__compute_y((*coutput)[i], coutput[1][i], coutput[2][i]);
					_out_[1] = byte.MaxValue;
					i++;
					_out_ += 2;
				}
				continue;
			}
			if (z.s.img_n == 4 && z.app14_color_transform == 0)
			{
				for (i = 0u; i < z.s.img_x; i++)
				{
					byte m3 = coutput[3][i];
					byte r3 = StbImage.stbi__blinn_8x8((*coutput)[i], m3);
					byte g = StbImage.stbi__blinn_8x8(coutput[1][i], m3);
					byte b = StbImage.stbi__blinn_8x8(coutput[2][i], m3);
					*_out_ = StbImage.stbi__compute_y(r3, g, b);
					_out_[1] = byte.MaxValue;
					_out_ += n;
				}
				continue;
			}
			if (z.s.img_n == 4 && z.app14_color_transform == 2)
			{
				for (i = 0u; i < z.s.img_x; i++)
				{
					*_out_ = StbImage.stbi__blinn_8x8((byte)(255 - (*coutput)[i]), coutput[3][i]);
					_out_[1] = byte.MaxValue;
					_out_ += n;
				}
				continue;
			}
			byte* y2 = *coutput;
			if (n == 1)
			{
				for (i = 0u; i < z.s.img_x; i++)
				{
					_out_[i] = y2[i];
				}
				continue;
			}
			for (i = 0u; i < z.s.img_x; i++)
			{
				*(_out_++) = y2[i];
				*(_out_++) = byte.MaxValue;
			}
		}
		StbImage.stbi__cleanup_jpeg(z);
		*out_x = (int)z.s.img_x;
		*out_y = (int)z.s.img_y;
		if (comp != null)
		{
			*comp = ((z.s.img_n < 3) ? 1 : 3);
		}
		return output;
	}

	public unsafe static void* stbi__jpeg_load(stbi__context s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri)
	{
		stbi__jpeg obj = new stbi__jpeg
		{
			s = s
		};
		StbImage.stbi__setup_jpeg(obj);
		return StbImage.load_jpeg_image(obj, x, y, comp, req_comp);
	}

	public static int stbi__jpeg_test(stbi__context s)
	{
		stbi__jpeg obj = new stbi__jpeg
		{
			s = s
		};
		StbImage.stbi__setup_jpeg(obj);
		int result = StbImage.stbi__decode_jpeg_header(obj, 1);
		StbImage.stbi__rewind(s);
		return result;
	}

	public unsafe static int stbi__jpeg_info_raw(stbi__jpeg j, int* x, int* y, int* comp)
	{
		if (StbImage.stbi__decode_jpeg_header(j, 2) == 0)
		{
			StbImage.stbi__rewind(j.s);
			return 0;
		}
		if (x != null)
		{
			*x = (int)j.s.img_x;
		}
		if (y != null)
		{
			*y = (int)j.s.img_y;
		}
		if (comp != null)
		{
			*comp = ((j.s.img_n < 3) ? 1 : 3);
		}
		return 1;
	}

	public unsafe static int stbi__jpeg_info(stbi__context s, int* x, int* y, int* comp)
	{
		return StbImage.stbi__jpeg_info_raw(new stbi__jpeg
		{
			s = s
		}, x, y, comp);
	}

	public static int stbi__bitreverse16(int n)
	{
		n = ((n & 0xAAAA) >> 1) | ((n & 0x5555) << 1);
		n = ((n & 0xCCCC) >> 2) | ((n & 0x3333) << 2);
		n = ((n & 0xF0F0) >> 4) | ((n & 0xF0F) << 4);
		n = ((n & 0xFF00) >> 8) | ((n & 0xFF) << 8);
		return n;
	}

	public static int stbi__bit_reverse(int v, int bits)
	{
		return StbImage.stbi__bitreverse16(v) >> 16 - bits;
	}

	public unsafe static int stbi__zbuild_huffman(stbi__zhuffman* z, byte* sizelist, int num)
	{
		int i = 0;
		int k = 0;
		int code = 0;
		int* next_code = stackalloc int[16];
		int* sizes = stackalloc int[17];
		CRuntime.memset(sizes, 0, 4uL);
		CRuntime.memset(z->fast, 0, 1024uL);
		for (i = 0; i < num; i++)
		{
			sizes[(int)sizelist[i]]++;
		}
		*sizes = 0;
		for (i = 1; i < 16; i++)
		{
			if (sizes[i] > 1 << i)
			{
				return StbImage.stbi__err("bad sizes");
			}
		}
		code = 0;
		for (i = 1; i < 16; i++)
		{
			next_code[i] = code;
			z->firstcode[i] = (ushort)code;
			z->firstsymbol[i] = (ushort)k;
			code += sizes[i];
			if (sizes[i] != 0 && code - 1 >= 1 << i)
			{
				return StbImage.stbi__err("bad codelengths");
			}
			z->maxcode[i] = code << 16 - i;
			code <<= 1;
			k += sizes[i];
		}
		z->maxcode[16] = 65536;
		for (i = 0; i < num; i++)
		{
			int s = sizelist[i];
			if (s == 0)
			{
				continue;
			}
			int c = next_code[s] - z->firstcode[s] + z->firstsymbol[s];
			ushort fastv = (ushort)((s << 9) | i);
			z->size[c] = (byte)s;
			z->value[c] = (ushort)i;
			if (s <= 9)
			{
				for (int j = StbImage.stbi__bit_reverse(next_code[s], s); j < 512; j += 1 << s)
				{
					z->fast[j] = fastv;
				}
			}
			next_code[s]++;
		}
		return 1;
	}

	public unsafe static byte stbi__zget8(stbi__zbuf* z)
	{
		if (z->zbuffer >= z->zbuffer_end)
		{
			return 0;
		}
		return *(z->zbuffer++);
	}

	public unsafe static void stbi__fill_bits(stbi__zbuf* z)
	{
		do
		{
			z->code_buffer |= (uint)(StbImage.stbi__zget8(z) << z->num_bits);
			z->num_bits += 8;
		}
		while (z->num_bits <= 24);
	}

	public unsafe static uint stbi__zreceive(stbi__zbuf* z, int n)
	{
		if (z->num_bits < n)
		{
			StbImage.stbi__fill_bits(z);
		}
		int result = (int)(z->code_buffer & ((1 << n) - 1));
		z->code_buffer >>>= n;
		z->num_bits -= n;
		return (uint)result;
	}

	public unsafe static int stbi__zhuffman_decode_slowpath(stbi__zbuf* a, stbi__zhuffman* z)
	{
		int b = 0;
		int s = 0;
		int k = 0;
		k = StbImage.stbi__bit_reverse((int)a->code_buffer, 16);
		for (s = 10; k >= z->maxcode[s]; s++)
		{
		}
		if (s == 16)
		{
			return -1;
		}
		b = (k >> 16 - s) - z->firstcode[s] + z->firstsymbol[s];
		a->code_buffer >>>= s;
		a->num_bits -= s;
		return z->value[b];
	}

	public unsafe static int stbi__zhuffman_decode(stbi__zbuf* a, stbi__zhuffman* z)
	{
		int b = 0;
		int s = 0;
		if (a->num_bits < 16)
		{
			StbImage.stbi__fill_bits(a);
		}
		b = z->fast[a->code_buffer & 0x1FF];
		if (b != 0)
		{
			s = b >> 9;
			a->code_buffer >>>= s;
			a->num_bits -= s;
			return b & 0x1FF;
		}
		return StbImage.stbi__zhuffman_decode_slowpath(a, z);
	}

	public unsafe static int stbi__zexpand(stbi__zbuf* z, sbyte* zout, int n)
	{
		int cur = 0;
		int limit = 0;
		z->zout = zout;
		if (z->z_expandable == 0)
		{
			return StbImage.stbi__err("output buffer limit");
		}
		cur = (int)(z->zout - z->zout_start);
		limit = (int)(z->zout_end - z->zout_start);
		while (cur + n > limit)
		{
			limit *= 2;
		}
		sbyte* q = (sbyte*)CRuntime.realloc(z->zout_start, (ulong)limit);
		if (q == null)
		{
			return StbImage.stbi__err("outofmem");
		}
		z->zout_start = q;
		z->zout = q + cur;
		z->zout_end = q + limit;
		return 1;
	}

	public unsafe static int stbi__parse_huffman_block(stbi__zbuf* a)
	{
		sbyte* zout = a->zout;
		while (true)
		{
			int z = StbImage.stbi__zhuffman_decode(a, &a->z_length);
			if (z < 256)
			{
				if (z < 0)
				{
					return StbImage.stbi__err("bad huffman code");
				}
				if (zout >= a->zout_end)
				{
					if (StbImage.stbi__zexpand(a, zout, 1) == 0)
					{
						return 0;
					}
					zout = a->zout;
				}
				*(zout++) = (sbyte)z;
				continue;
			}
			int len = 0;
			int dist = 0;
			if (z == 256)
			{
				a->zout = zout;
				return 1;
			}
			z -= 257;
			len = StbImage.stbi__zlength_base[z];
			if (StbImage.stbi__zlength_extra[z] != 0)
			{
				len += (int)StbImage.stbi__zreceive(a, StbImage.stbi__zlength_extra[z]);
			}
			z = StbImage.stbi__zhuffman_decode(a, &a->z_distance);
			if (z < 0)
			{
				return StbImage.stbi__err("bad huffman code");
			}
			dist = StbImage.stbi__zdist_base[z];
			if (StbImage.stbi__zdist_extra[z] != 0)
			{
				dist += (int)StbImage.stbi__zreceive(a, StbImage.stbi__zdist_extra[z]);
			}
			if (zout - a->zout_start < dist)
			{
				return StbImage.stbi__err("bad dist");
			}
			if (zout + len > a->zout_end)
			{
				if (StbImage.stbi__zexpand(a, zout, len) == 0)
				{
					break;
				}
				zout = a->zout;
			}
			byte* p = (byte*)(zout - dist);
			if (dist == 1)
			{
				byte v = *p;
				if (len != 0)
				{
					do
					{
						*(zout++) = (sbyte)v;
					}
					while (--len != 0);
				}
			}
			else if (len != 0)
			{
				do
				{
					*(zout++) = (sbyte)(*(p++));
				}
				while (--len != 0);
			}
		}
		return 0;
	}

	public unsafe static int stbi__compute_huffman_codes(stbi__zbuf* a)
	{
		byte* length_dezigzag = stackalloc byte[19];
		*length_dezigzag = 16;
		length_dezigzag[1] = 17;
		length_dezigzag[2] = 18;
		length_dezigzag[3] = 0;
		length_dezigzag[4] = 8;
		length_dezigzag[5] = 7;
		length_dezigzag[6] = 9;
		length_dezigzag[7] = 6;
		length_dezigzag[8] = 10;
		length_dezigzag[9] = 5;
		length_dezigzag[10] = 11;
		length_dezigzag[11] = 4;
		length_dezigzag[12] = 12;
		length_dezigzag[13] = 3;
		length_dezigzag[14] = 13;
		length_dezigzag[15] = 2;
		length_dezigzag[16] = 14;
		length_dezigzag[17] = 1;
		length_dezigzag[18] = 15;
		stbi__zhuffman z_codelength = default(stbi__zhuffman);
		byte* lencodes = stackalloc byte[455];
		byte* codelength_sizes = stackalloc byte[19];
		int i = 0;
		int n = 0;
		int hlit = (int)(StbImage.stbi__zreceive(a, 5) + 257);
		int hdist = (int)(StbImage.stbi__zreceive(a, 5) + 1);
		int hclen = (int)(StbImage.stbi__zreceive(a, 4) + 4);
		int ntot = hlit + hdist;
		CRuntime.memset(codelength_sizes, 0, 19uL);
		for (i = 0; i < hclen; i++)
		{
			int s = (int)StbImage.stbi__zreceive(a, 3);
			codelength_sizes[(int)length_dezigzag[i]] = (byte)s;
		}
		if (StbImage.stbi__zbuild_huffman(&z_codelength, codelength_sizes, 19) == 0)
		{
			return 0;
		}
		n = 0;
		while (n < ntot)
		{
			int c = StbImage.stbi__zhuffman_decode(a, &z_codelength);
			switch (c)
			{
			default:
				return StbImage.stbi__err("bad codelengths");
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
			case 10:
			case 11:
			case 12:
			case 13:
			case 14:
			case 15:
				lencodes[n++] = (byte)c;
				break;
			case 16:
			case 17:
			case 18:
			{
				byte fill = 0;
				switch (c)
				{
				case 16:
					c = (int)(StbImage.stbi__zreceive(a, 2) + 3);
					if (n == 0)
					{
						return StbImage.stbi__err("bad codelengths");
					}
					fill = lencodes[n - 1];
					break;
				case 17:
					c = (int)(StbImage.stbi__zreceive(a, 3) + 3);
					break;
				default:
					c = (int)(StbImage.stbi__zreceive(a, 7) + 11);
					break;
				}
				if (ntot - n < c)
				{
					return StbImage.stbi__err("bad codelengths");
				}
				CRuntime.memset(lencodes + n, fill, (ulong)c);
				n += c;
				break;
			}
			}
		}
		if (n != ntot)
		{
			return StbImage.stbi__err("bad codelengths");
		}
		if (StbImage.stbi__zbuild_huffman(&a->z_length, lencodes, hlit) == 0)
		{
			return 0;
		}
		if (StbImage.stbi__zbuild_huffman(&a->z_distance, lencodes + hlit, hdist) == 0)
		{
			return 0;
		}
		return 1;
	}

	public unsafe static int stbi__parse_uncompressed_block(stbi__zbuf* a)
	{
		byte* header = stackalloc byte[4];
		int len = 0;
		int k = 0;
		if ((a->num_bits & 7) != 0)
		{
			StbImage.stbi__zreceive(a, a->num_bits & 7);
		}
		k = 0;
		while (a->num_bits > 0)
		{
			header[k++] = (byte)(a->code_buffer & 0xFF);
			a->code_buffer >>>= 8;
			a->num_bits -= 8;
		}
		while (k < 4)
		{
			header[k++] = StbImage.stbi__zget8(a);
		}
		len = header[1] * 256 + *header;
		if (header[3] * 256 + header[2] != (len ^ 0xFFFF))
		{
			return StbImage.stbi__err("zlib corrupt");
		}
		if (a->zbuffer + len > a->zbuffer_end)
		{
			return StbImage.stbi__err("read past buffer");
		}
		if (a->zout + len > a->zout_end && StbImage.stbi__zexpand(a, a->zout, len) == 0)
		{
			return 0;
		}
		CRuntime.memcpy(a->zout, a->zbuffer, (ulong)len);
		a->zbuffer += len;
		a->zout += len;
		return 1;
	}

	public unsafe static int stbi__parse_zlib_header(stbi__zbuf* a)
	{
		byte num = StbImage.stbi__zget8(a);
		int cm = num & 0xF;
		int flg = StbImage.stbi__zget8(a);
		if ((num * 256 + flg) % 31 != 0)
		{
			return StbImage.stbi__err("bad zlib header");
		}
		if ((flg & 0x20) != 0)
		{
			return StbImage.stbi__err("no preset dict");
		}
		if (cm != 8)
		{
			return StbImage.stbi__err("bad compression");
		}
		return 1;
	}

	public unsafe static int stbi__parse_zlib(stbi__zbuf* a, int parse_header)
	{
		int final = 0;
		int type = 0;
		if (parse_header != 0 && StbImage.stbi__parse_zlib_header(a) == 0)
		{
			return 0;
		}
		a->num_bits = 0;
		a->code_buffer = 0u;
		do
		{
			final = (int)StbImage.stbi__zreceive(a, 1);
			switch ((int)StbImage.stbi__zreceive(a, 2))
			{
			case 0:
				if (StbImage.stbi__parse_uncompressed_block(a) == 0)
				{
					return 0;
				}
				continue;
			case 3:
				return 0;
			case 1:
				fixed (byte* b = StbImage.stbi__zdefault_length)
				{
					if (StbImage.stbi__zbuild_huffman(&a->z_length, b, 288) == 0)
					{
						return 0;
					}
				}
				fixed (byte* b2 = StbImage.stbi__zdefault_distance)
				{
					if (StbImage.stbi__zbuild_huffman(&a->z_distance, b2, 32) == 0)
					{
						return 0;
					}
				}
				break;
			default:
				if (StbImage.stbi__compute_huffman_codes(a) == 0)
				{
					return 0;
				}
				break;
			}
			if (StbImage.stbi__parse_huffman_block(a) == 0)
			{
				return 0;
			}
		}
		while (final == 0);
		return 1;
	}

	public unsafe static int stbi__do_zlib(stbi__zbuf* a, sbyte* obuf, int olen, int exp, int parse_header)
	{
		a->zout_start = obuf;
		a->zout = obuf;
		a->zout_end = obuf + olen;
		a->z_expandable = exp;
		return StbImage.stbi__parse_zlib(a, parse_header);
	}

	public unsafe static sbyte* stbi_zlib_decode_malloc_guesssize(sbyte* buffer, int len, int initial_size, int* outlen)
	{
		stbi__zbuf a = default(stbi__zbuf);
		sbyte* p = (sbyte*)StbImage.stbi__malloc((ulong)initial_size);
		if (p == null)
		{
			return null;
		}
		a.zbuffer = (byte*)buffer;
		a.zbuffer_end = (byte*)(buffer + len);
		if (StbImage.stbi__do_zlib(&a, p, initial_size, 1, 1) != 0)
		{
			if (outlen != null)
			{
				*outlen = (int)(a.zout - a.zout_start);
			}
			return a.zout_start;
		}
		CRuntime.free(a.zout_start);
		return null;
	}

	public unsafe static sbyte* stbi_zlib_decode_malloc(sbyte* buffer, int len, int* outlen)
	{
		return StbImage.stbi_zlib_decode_malloc_guesssize(buffer, len, 16384, outlen);
	}

	public unsafe static sbyte* stbi_zlib_decode_malloc_guesssize_headerflag(sbyte* buffer, int len, int initial_size, int* outlen, int parse_header)
	{
		stbi__zbuf a = default(stbi__zbuf);
		sbyte* p = (sbyte*)StbImage.stbi__malloc((ulong)initial_size);
		if (p == null)
		{
			return null;
		}
		a.zbuffer = (byte*)buffer;
		a.zbuffer_end = (byte*)(buffer + len);
		if (StbImage.stbi__do_zlib(&a, p, initial_size, 1, parse_header) != 0)
		{
			if (outlen != null)
			{
				*outlen = (int)(a.zout - a.zout_start);
			}
			return a.zout_start;
		}
		CRuntime.free(a.zout_start);
		return null;
	}

	public unsafe static int stbi_zlib_decode_buffer(sbyte* obuffer, int olen, sbyte* ibuffer, int ilen)
	{
		stbi__zbuf a = new stbi__zbuf
		{
			zbuffer = (byte*)ibuffer,
			zbuffer_end = (byte*)(ibuffer + ilen)
		};
		if (StbImage.stbi__do_zlib(&a, obuffer, olen, 0, 1) != 0)
		{
			return (int)(a.zout - a.zout_start);
		}
		return -1;
	}

	public unsafe static sbyte* stbi_zlib_decode_noheader_malloc(sbyte* buffer, int len, int* outlen)
	{
		stbi__zbuf a = default(stbi__zbuf);
		sbyte* p = (sbyte*)StbImage.stbi__malloc(16384uL);
		if (p == null)
		{
			return null;
		}
		a.zbuffer = (byte*)buffer;
		a.zbuffer_end = (byte*)(buffer + len);
		if (StbImage.stbi__do_zlib(&a, p, 16384, 1, 0) != 0)
		{
			if (outlen != null)
			{
				*outlen = (int)(a.zout - a.zout_start);
			}
			return a.zout_start;
		}
		CRuntime.free(a.zout_start);
		return null;
	}

	public unsafe static int stbi_zlib_decode_noheader_buffer(sbyte* obuffer, int olen, sbyte* ibuffer, int ilen)
	{
		stbi__zbuf a = new stbi__zbuf
		{
			zbuffer = (byte*)ibuffer,
			zbuffer_end = (byte*)(ibuffer + ilen)
		};
		if (StbImage.stbi__do_zlib(&a, obuffer, olen, 0, 0) != 0)
		{
			return (int)(a.zout - a.zout_start);
		}
		return -1;
	}

	public static stbi__pngchunk stbi__get_chunk_header(stbi__context s)
	{
		return new stbi__pngchunk
		{
			length = StbImage.stbi__get32be(s),
			type = StbImage.stbi__get32be(s)
		};
	}

	public unsafe static int stbi__check_png_header(stbi__context s)
	{
		byte* png_sig = stackalloc byte[8];
		*png_sig = 137;
		png_sig[1] = 80;
		png_sig[2] = 78;
		png_sig[3] = 71;
		png_sig[4] = 13;
		png_sig[5] = 10;
		png_sig[6] = 26;
		png_sig[7] = 10;
		int i = 0;
		for (i = 0; i < 8; i++)
		{
			if (StbImage.stbi__get8(s) != png_sig[i])
			{
				return StbImage.stbi__err("bad png sig");
			}
		}
		return 1;
	}

	public static int stbi__paeth(int a, int b, int c)
	{
		int num = a + b - c;
		int pa = CRuntime.abs(num - a);
		int pb = CRuntime.abs(num - b);
		int pc = CRuntime.abs(num - c);
		if (pa <= pb && pa <= pc)
		{
			return a;
		}
		if (pb <= pc)
		{
			return b;
		}
		return c;
	}

	public unsafe static int stbi__create_png_image_raw(stbi__png a, byte* raw, uint raw_len, int out_n, uint x, uint y, int depth, int color)
	{
		int bytes = ((depth != 16) ? 1 : 2);
		stbi__context s = a.s;
		uint i = 0u;
		uint j = 0u;
		uint stride = (uint)(x * out_n * bytes);
		uint img_len = 0u;
		uint img_width_bytes = 0u;
		int k = 0;
		int img_n = s.img_n;
		int output_bytes = out_n * bytes;
		int filter_bytes = img_n * bytes;
		int width = (int)x;
		a._out_ = (byte*)StbImage.stbi__malloc_mad3((int)x, (int)y, output_bytes, 0);
		if (a._out_ == null)
		{
			return StbImage.stbi__err("outofmem");
		}
		if (StbImage.stbi__mad3sizes_valid(img_n, (int)x, depth, 7) == 0)
		{
			return StbImage.stbi__err("too large");
		}
		img_width_bytes = (uint)(img_n * x * depth + 7 >> 3);
		img_len = (img_width_bytes + 1) * y;
		if (raw_len < img_len)
		{
			return StbImage.stbi__err("not enough pixels");
		}
		for (j = 0u; j < y; j++)
		{
			byte* cur = a._out_ + stride * j;
			int filter = *(raw++);
			if (filter > 4)
			{
				return StbImage.stbi__err("invalid filter");
			}
			if (depth < 8)
			{
				cur += x * out_n - img_width_bytes;
				filter_bytes = 1;
				width = (int)img_width_bytes;
			}
			byte* prior = cur - stride;
			if (j == 0)
			{
				filter = StbImage.first_row_filter[filter];
			}
			for (k = 0; k < filter_bytes; k++)
			{
				switch (filter)
				{
				case 0:
					cur[k] = raw[k];
					break;
				case 1:
					cur[k] = raw[k];
					break;
				case 2:
					cur[k] = (byte)((raw[k] + prior[k]) & 0xFF);
					break;
				case 3:
					cur[k] = (byte)((raw[k] + (prior[k] >> 1)) & 0xFF);
					break;
				case 4:
					cur[k] = (byte)((raw[k] + StbImage.stbi__paeth(0, prior[k], 0)) & 0xFF);
					break;
				case 5:
					cur[k] = raw[k];
					break;
				case 6:
					cur[k] = raw[k];
					break;
				}
			}
			switch (depth)
			{
			case 8:
				if (img_n != out_n)
				{
					cur[img_n] = byte.MaxValue;
				}
				raw += img_n;
				cur += out_n;
				prior += out_n;
				break;
			case 16:
				if (img_n != out_n)
				{
					cur[filter_bytes] = byte.MaxValue;
					cur[filter_bytes + 1] = byte.MaxValue;
				}
				raw += filter_bytes;
				cur += output_bytes;
				prior += output_bytes;
				break;
			default:
				raw++;
				cur++;
				prior++;
				break;
			}
			if (depth < 8 || img_n == out_n)
			{
				int nk = (width - 1) * filter_bytes;
				switch (filter)
				{
				case 0:
					CRuntime.memcpy(cur, raw, (ulong)nk);
					break;
				case 1:
					for (k = 0; k < nk; k++)
					{
						cur[k] = (byte)((raw[k] + cur[k - filter_bytes]) & 0xFF);
					}
					break;
				case 2:
					for (k = 0; k < nk; k++)
					{
						cur[k] = (byte)((raw[k] + prior[k]) & 0xFF);
					}
					break;
				case 3:
					for (k = 0; k < nk; k++)
					{
						cur[k] = (byte)((raw[k] + (prior[k] + cur[k - filter_bytes] >> 1)) & 0xFF);
					}
					break;
				case 4:
					for (k = 0; k < nk; k++)
					{
						cur[k] = (byte)((raw[k] + StbImage.stbi__paeth(cur[k - filter_bytes], prior[k], prior[k - filter_bytes])) & 0xFF);
					}
					break;
				case 5:
					for (k = 0; k < nk; k++)
					{
						cur[k] = (byte)((raw[k] + (cur[k - filter_bytes] >> 1)) & 0xFF);
					}
					break;
				case 6:
					for (k = 0; k < nk; k++)
					{
						cur[k] = (byte)((raw[k] + StbImage.stbi__paeth(cur[k - filter_bytes], 0, 0)) & 0xFF);
					}
					break;
				}
				raw += nk;
				continue;
			}
			switch (filter)
			{
			case 0:
				i = x - 1;
				while (i >= 1)
				{
					for (k = 0; k < filter_bytes; k++)
					{
						cur[k] = raw[k];
					}
					i--;
					cur[filter_bytes] = byte.MaxValue;
					raw += filter_bytes;
					cur += output_bytes;
					prior += output_bytes;
				}
				break;
			case 1:
				i = x - 1;
				while (i >= 1)
				{
					for (k = 0; k < filter_bytes; k++)
					{
						cur[k] = (byte)((raw[k] + cur[k - output_bytes]) & 0xFF);
					}
					i--;
					cur[filter_bytes] = byte.MaxValue;
					raw += filter_bytes;
					cur += output_bytes;
					prior += output_bytes;
				}
				break;
			case 2:
				i = x - 1;
				while (i >= 1)
				{
					for (k = 0; k < filter_bytes; k++)
					{
						cur[k] = (byte)((raw[k] + prior[k]) & 0xFF);
					}
					i--;
					cur[filter_bytes] = byte.MaxValue;
					raw += filter_bytes;
					cur += output_bytes;
					prior += output_bytes;
				}
				break;
			case 3:
				i = x - 1;
				while (i >= 1)
				{
					for (k = 0; k < filter_bytes; k++)
					{
						cur[k] = (byte)((raw[k] + (prior[k] + cur[k - output_bytes] >> 1)) & 0xFF);
					}
					i--;
					cur[filter_bytes] = byte.MaxValue;
					raw += filter_bytes;
					cur += output_bytes;
					prior += output_bytes;
				}
				break;
			case 4:
				i = x - 1;
				while (i >= 1)
				{
					for (k = 0; k < filter_bytes; k++)
					{
						cur[k] = (byte)((raw[k] + StbImage.stbi__paeth(cur[k - output_bytes], prior[k], prior[k - output_bytes])) & 0xFF);
					}
					i--;
					cur[filter_bytes] = byte.MaxValue;
					raw += filter_bytes;
					cur += output_bytes;
					prior += output_bytes;
				}
				break;
			case 5:
				i = x - 1;
				while (i >= 1)
				{
					for (k = 0; k < filter_bytes; k++)
					{
						cur[k] = (byte)((raw[k] + (cur[k - output_bytes] >> 1)) & 0xFF);
					}
					i--;
					cur[filter_bytes] = byte.MaxValue;
					raw += filter_bytes;
					cur += output_bytes;
					prior += output_bytes;
				}
				break;
			case 6:
				i = x - 1;
				while (i >= 1)
				{
					for (k = 0; k < filter_bytes; k++)
					{
						cur[k] = (byte)((raw[k] + StbImage.stbi__paeth(cur[k - output_bytes], 0, 0)) & 0xFF);
					}
					i--;
					cur[filter_bytes] = byte.MaxValue;
					raw += filter_bytes;
					cur += output_bytes;
					prior += output_bytes;
				}
				break;
			}
			if (depth == 16)
			{
				cur = a._out_ + stride * j;
				i = 0u;
				while (i < x)
				{
					cur[filter_bytes + 1] = byte.MaxValue;
					i++;
					cur += output_bytes;
				}
			}
		}
		if (depth < 8)
		{
			for (j = 0u; j < y; j++)
			{
				byte* cur2 = a._out_ + stride * j;
				byte* _in_ = a._out_ + stride * j + x * out_n - img_width_bytes;
				byte scale = (byte)((color != 0) ? 1 : StbImage.stbi__depth_scale_table[depth]);
				switch (depth)
				{
				case 4:
					k = (int)(x * img_n);
					while (k >= 2)
					{
						*(cur2++) = (byte)(scale * (*_in_ >> 4));
						*(cur2++) = (byte)(scale * (*_in_ & 0xF));
						k -= 2;
						_in_++;
					}
					if (k > 0)
					{
						*(cur2++) = (byte)(scale * (*_in_ >> 4));
					}
					break;
				case 2:
					k = (int)(x * img_n);
					while (k >= 4)
					{
						*(cur2++) = (byte)(scale * (*_in_ >> 6));
						*(cur2++) = (byte)(scale * ((*_in_ >> 4) & 3));
						*(cur2++) = (byte)(scale * ((*_in_ >> 2) & 3));
						*(cur2++) = (byte)(scale * (*_in_ & 3));
						k -= 4;
						_in_++;
					}
					if (k > 0)
					{
						*(cur2++) = (byte)(scale * (*_in_ >> 6));
					}
					if (k > 1)
					{
						*(cur2++) = (byte)(scale * ((*_in_ >> 4) & 3));
					}
					if (k > 2)
					{
						*(cur2++) = (byte)(scale * ((*_in_ >> 2) & 3));
					}
					break;
				case 1:
					k = (int)(x * img_n);
					while (k >= 8)
					{
						*(cur2++) = (byte)(scale * (*_in_ >> 7));
						*(cur2++) = (byte)(scale * ((*_in_ >> 6) & 1));
						*(cur2++) = (byte)(scale * ((*_in_ >> 5) & 1));
						*(cur2++) = (byte)(scale * ((*_in_ >> 4) & 1));
						*(cur2++) = (byte)(scale * ((*_in_ >> 3) & 1));
						*(cur2++) = (byte)(scale * ((*_in_ >> 2) & 1));
						*(cur2++) = (byte)(scale * ((*_in_ >> 1) & 1));
						*(cur2++) = (byte)(scale * (*_in_ & 1));
						k -= 8;
						_in_++;
					}
					if (k > 0)
					{
						*(cur2++) = (byte)(scale * (*_in_ >> 7));
					}
					if (k > 1)
					{
						*(cur2++) = (byte)(scale * ((*_in_ >> 6) & 1));
					}
					if (k > 2)
					{
						*(cur2++) = (byte)(scale * ((*_in_ >> 5) & 1));
					}
					if (k > 3)
					{
						*(cur2++) = (byte)(scale * ((*_in_ >> 4) & 1));
					}
					if (k > 4)
					{
						*(cur2++) = (byte)(scale * ((*_in_ >> 3) & 1));
					}
					if (k > 5)
					{
						*(cur2++) = (byte)(scale * ((*_in_ >> 2) & 1));
					}
					if (k > 6)
					{
						*(cur2++) = (byte)(scale * ((*_in_ >> 1) & 1));
					}
					break;
				}
				if (img_n == out_n)
				{
					continue;
				}
				int q = 0;
				cur2 = a._out_ + stride * j;
				if (img_n == 1)
				{
					for (q = (int)(x - 1); q >= 0; q--)
					{
						cur2[q * 2 + 1] = byte.MaxValue;
						cur2[q * 2] = cur2[q];
					}
					continue;
				}
				for (q = (int)(x - 1); q >= 0; q--)
				{
					cur2[q * 4 + 3] = byte.MaxValue;
					cur2[q * 4 + 2] = cur2[q * 3 + 2];
					cur2[q * 4 + 1] = cur2[q * 3 + 1];
					cur2[q * 4] = cur2[q * 3];
				}
			}
		}
		else if (depth == 16)
		{
			byte* cur3 = a._out_;
			ushort* cur16 = (ushort*)cur3;
			i = 0u;
			while (i < x * y * out_n)
			{
				*cur16 = (ushort)((*cur3 << 8) | cur3[1]);
				i++;
				cur16++;
				cur3 += 2;
			}
		}
		return 1;
	}

	public unsafe static int stbi__create_png_image(stbi__png a, byte* image_data, uint image_data_len, int out_n, int depth, int color, int interlaced)
	{
		int bytes = ((depth != 16) ? 1 : 2);
		int out_bytes = out_n * bytes;
		int p = 0;
		if (interlaced == 0)
		{
			return StbImage.stbi__create_png_image_raw(a, image_data, image_data_len, out_n, a.s.img_x, a.s.img_y, depth, color);
		}
		byte* final = (byte*)StbImage.stbi__malloc_mad3((int)a.s.img_x, (int)a.s.img_y, out_bytes, 0);
		for (p = 0; p < 7; p++)
		{
			int* xorig = stackalloc int[7];
			*xorig = 0;
			xorig[1] = 4;
			xorig[2] = 0;
			xorig[3] = 2;
			xorig[4] = 0;
			xorig[5] = 1;
			xorig[6] = 0;
			int* yorig = stackalloc int[7];
			*yorig = 0;
			yorig[1] = 0;
			yorig[2] = 4;
			yorig[3] = 0;
			yorig[4] = 2;
			yorig[5] = 0;
			yorig[6] = 1;
			int* xspc = stackalloc int[7];
			*xspc = 8;
			xspc[1] = 8;
			xspc[2] = 4;
			xspc[3] = 4;
			xspc[4] = 2;
			xspc[5] = 2;
			xspc[6] = 1;
			int* yspc = stackalloc int[7];
			*yspc = 8;
			yspc[1] = 8;
			yspc[2] = 8;
			yspc[3] = 4;
			yspc[4] = 4;
			yspc[5] = 2;
			yspc[6] = 2;
			int i = 0;
			int j = 0;
			int x = 0;
			int y = 0;
			x = (int)((a.s.img_x - xorig[p] + xspc[p] - 1) / xspc[p]);
			y = (int)((a.s.img_y - yorig[p] + yspc[p] - 1) / yspc[p]);
			if (x == 0 || y == 0)
			{
				continue;
			}
			uint img_len = (uint)(((a.s.img_n * x * depth + 7 >> 3) + 1) * y);
			if (StbImage.stbi__create_png_image_raw(a, image_data, image_data_len, out_n, (uint)x, (uint)y, depth, color) == 0)
			{
				CRuntime.free(final);
				return 0;
			}
			for (j = 0; j < y; j++)
			{
				for (i = 0; i < x; i++)
				{
					int out_y = j * yspc[p] + yorig[p];
					int out_x = i * xspc[p] + xorig[p];
					CRuntime.memcpy(final + out_y * a.s.img_x * out_bytes + out_x * out_bytes, a._out_ + (j * x + i) * out_bytes, (ulong)out_bytes);
				}
			}
			CRuntime.free(a._out_);
			image_data += img_len;
			image_data_len -= img_len;
		}
		a._out_ = final;
		return 1;
	}

	public unsafe static int stbi__compute_transparency(stbi__png z, byte* tc, int out_n)
	{
		stbi__context s = z.s;
		uint i = 0u;
		uint pixel_count = s.img_x * s.img_y;
		byte* p = z._out_;
		if (out_n == 2)
		{
			for (i = 0u; i < pixel_count; i++)
			{
				p[1] = (byte)((*p != *tc) ? 255u : 0u);
				p += 2;
			}
		}
		else
		{
			for (i = 0u; i < pixel_count; i++)
			{
				if (*p == *tc && p[1] == tc[1] && p[2] == tc[2])
				{
					p[3] = 0;
				}
				p += 4;
			}
		}
		return 1;
	}

	public unsafe static int stbi__compute_transparency16(stbi__png z, ushort* tc, int out_n)
	{
		stbi__context s = z.s;
		uint i = 0u;
		uint pixel_count = s.img_x * s.img_y;
		ushort* p = (ushort*)z._out_;
		if (out_n == 2)
		{
			for (i = 0u; i < pixel_count; i++)
			{
				p[1] = (ushort)((*p != *tc) ? 65535u : 0u);
				p += 2;
			}
		}
		else
		{
			for (i = 0u; i < pixel_count; i++)
			{
				if (*p == *tc && p[1] == tc[1] && p[2] == tc[2])
				{
					p[3] = 0;
				}
				p += 4;
			}
		}
		return 1;
	}

	public unsafe static int stbi__expand_png_palette(stbi__png a, byte* palette, int len, int pal_img_n)
	{
		uint i = 0u;
		uint pixel_count = a.s.img_x * a.s.img_y;
		byte* orig = a._out_;
		byte* p = (byte*)StbImage.stbi__malloc_mad2((int)pixel_count, pal_img_n, 0);
		if (p == null)
		{
			return StbImage.stbi__err("outofmem");
		}
		byte* temp_out = p;
		if (pal_img_n == 3)
		{
			for (i = 0u; i < pixel_count; i++)
			{
				int n = orig[i] * 4;
				*p = palette[n];
				p[1] = palette[n + 1];
				p[2] = palette[n + 2];
				p += 3;
			}
		}
		else
		{
			for (i = 0u; i < pixel_count; i++)
			{
				int n2 = orig[i] * 4;
				*p = palette[n2];
				p[1] = palette[n2 + 1];
				p[2] = palette[n2 + 2];
				p[3] = palette[n2 + 3];
				p += 4;
			}
		}
		CRuntime.free(a._out_);
		a._out_ = temp_out;
		return 1;
	}

	public static void stbi_set_unpremultiply_on_load(int flag_true_if_should_unpremultiply)
	{
		StbImage.stbi__unpremultiply_on_load = flag_true_if_should_unpremultiply;
	}

	public static void stbi_convert_iphone_png_to_rgb(int flag_true_if_should_convert)
	{
		StbImage.stbi__de_iphone_flag = flag_true_if_should_convert;
	}

	public unsafe static void stbi__de_iphone(stbi__png z)
	{
		stbi__context s = z.s;
		uint i = 0u;
		uint pixel_count = s.img_x * s.img_y;
		byte* p = z._out_;
		if (s.img_out_n == 3)
		{
			for (i = 0u; i < pixel_count; i++)
			{
				byte t = *p;
				*p = p[2];
				p[2] = t;
				p += 3;
			}
		}
		else if (StbImage.stbi__unpremultiply_on_load != 0)
		{
			for (i = 0u; i < pixel_count; i++)
			{
				byte a = p[3];
				byte t2 = *p;
				if (a != 0)
				{
					byte half = (byte)(a / 2);
					*p = (byte)((p[2] * 255 + half) / a);
					p[1] = (byte)((p[1] * 255 + half) / a);
					p[2] = (byte)((t2 * 255 + half) / a);
				}
				else
				{
					*p = p[2];
					p[2] = t2;
				}
				p += 4;
			}
		}
		else
		{
			for (i = 0u; i < pixel_count; i++)
			{
				byte t3 = *p;
				*p = p[2];
				p[2] = t3;
				p += 4;
			}
		}
	}

	public unsafe static int stbi__parse_png_file(stbi__png z, int scan, int req_comp)
	{
		byte* palette = stackalloc byte[1024];
		byte pal_img_n = 0;
		byte has_trans = 0;
		byte* tc = stackalloc byte[3];
		*tc = 0;
		ushort* tc16 = stackalloc ushort[3];
		uint ioff = 0u;
		uint idata_limit = 0u;
		uint i = 0u;
		uint pal_len = 0u;
		int first = 1;
		int k = 0;
		int interlace = 0;
		int color = 0;
		int is_iphone = 0;
		stbi__context s = z.s;
		z.expanded = null;
		z.idata = null;
		z._out_ = null;
		if (StbImage.stbi__check_png_header(s) == 0)
		{
			return 0;
		}
		if (scan == 1)
		{
			return 1;
		}
		while (true)
		{
			stbi__pngchunk c = StbImage.stbi__get_chunk_header(s);
			switch (c.type)
			{
			case 1130840649u:
				is_iphone = 1;
				StbImage.stbi__skip(s, (int)c.length);
				break;
			case 1229472850u:
				if (first == 0)
				{
					return StbImage.stbi__err("multiple IHDR");
				}
				first = 0;
				if (c.length != 13)
				{
					return StbImage.stbi__err("bad IHDR len");
				}
				s.img_x = StbImage.stbi__get32be(s);
				if (s.img_x > 16777216)
				{
					return StbImage.stbi__err("too large");
				}
				s.img_y = StbImage.stbi__get32be(s);
				if (s.img_y > 16777216)
				{
					return StbImage.stbi__err("too large");
				}
				z.depth = StbImage.stbi__get8(s);
				if (z.depth != 1 && z.depth != 2 && z.depth != 4 && z.depth != 8 && z.depth != 16)
				{
					return StbImage.stbi__err("1/2/4/8/16-bit only");
				}
				color = StbImage.stbi__get8(s);
				if (color > 6)
				{
					return StbImage.stbi__err("bad ctype");
				}
				if (color == 3 && z.depth == 16)
				{
					return StbImage.stbi__err("bad ctype");
				}
				if (color == 3)
				{
					pal_img_n = 3;
				}
				else if ((color & 1) != 0)
				{
					return StbImage.stbi__err("bad ctype");
				}
				if (StbImage.stbi__get8(s) != 0)
				{
					return StbImage.stbi__err("bad comp method");
				}
				if (StbImage.stbi__get8(s) != 0)
				{
					return StbImage.stbi__err("bad filter method");
				}
				interlace = StbImage.stbi__get8(s);
				if (interlace > 1)
				{
					return StbImage.stbi__err("bad interlace method");
				}
				if (s.img_x == 0 || s.img_y == 0)
				{
					return StbImage.stbi__err("0-pixel image");
				}
				if (pal_img_n == 0)
				{
					s.img_n = (((color & 2) == 0) ? 1 : 3) + (((color & 4) != 0) ? 1 : 0);
					if (1073741824 / s.img_x / s.img_n < s.img_y)
					{
						return StbImage.stbi__err("too large");
					}
					if (scan == 2)
					{
						return 1;
					}
				}
				else
				{
					s.img_n = 1;
					if (1073741824 / s.img_x / 4 < s.img_y)
					{
						return StbImage.stbi__err("too large");
					}
				}
				break;
			case 1347179589u:
				if (first != 0)
				{
					return StbImage.stbi__err("first not IHDR");
				}
				if (c.length > 768)
				{
					return StbImage.stbi__err("invalid PLTE");
				}
				pal_len = c.length / 3;
				if (pal_len * 3 != c.length)
				{
					return StbImage.stbi__err("invalid PLTE");
				}
				for (i = 0u; i < pal_len; i++)
				{
					palette[i * 4] = StbImage.stbi__get8(s);
					palette[i * 4 + 1] = StbImage.stbi__get8(s);
					palette[i * 4 + 2] = StbImage.stbi__get8(s);
					palette[i * 4 + 3] = byte.MaxValue;
				}
				break;
			case 1951551059u:
				if (first != 0)
				{
					return StbImage.stbi__err("first not IHDR");
				}
				if (z.idata != null)
				{
					return StbImage.stbi__err("tRNS after IDAT");
				}
				if (pal_img_n != 0)
				{
					if (scan == 2)
					{
						s.img_n = 4;
						return 1;
					}
					if (pal_len == 0)
					{
						return StbImage.stbi__err("tRNS before PLTE");
					}
					if (c.length > pal_len)
					{
						return StbImage.stbi__err("bad tRNS len");
					}
					pal_img_n = 4;
					for (i = 0u; i < c.length; i++)
					{
						palette[i * 4 + 3] = StbImage.stbi__get8(s);
					}
					break;
				}
				if ((s.img_n & 1) == 0)
				{
					return StbImage.stbi__err("tRNS with alpha");
				}
				if (c.length != (uint)(s.img_n * 2))
				{
					return StbImage.stbi__err("bad tRNS len");
				}
				has_trans = 1;
				if (z.depth == 16)
				{
					for (k = 0; k < s.img_n; k++)
					{
						tc16[k] = (ushort)StbImage.stbi__get16be(s);
					}
				}
				else
				{
					for (k = 0; k < s.img_n; k++)
					{
						tc[k] = (byte)((byte)(StbImage.stbi__get16be(s) & 0xFF) * StbImage.stbi__depth_scale_table[z.depth]);
					}
				}
				break;
			case 1229209940u:
				if (first != 0)
				{
					return StbImage.stbi__err("first not IHDR");
				}
				if (pal_img_n != 0 && pal_len == 0)
				{
					return StbImage.stbi__err("no PLTE");
				}
				if (scan == 2)
				{
					s.img_n = pal_img_n;
					return 1;
				}
				if ((int)(ioff + c.length) < (int)ioff)
				{
					return 0;
				}
				if (ioff + c.length > idata_limit)
				{
					if (idata_limit == 0)
					{
						idata_limit = ((c.length > 4096) ? c.length : 4096u);
					}
					while (ioff + c.length > idata_limit)
					{
						idata_limit *= 2;
					}
					byte* p = (byte*)CRuntime.realloc((void*)z.idata, (ulong)idata_limit);
					if (p == null)
					{
						return StbImage.stbi__err("outofmem");
					}
					z.idata = p;
				}
				if (StbImage.stbi__getn(s, z.idata + ioff, (int)c.length) == 0)
				{
					return StbImage.stbi__err("outofdata");
				}
				ioff += c.length;
				break;
			case 1229278788u:
			{
				uint raw_len = 0u;
				if (first != 0)
				{
					return StbImage.stbi__err("first not IHDR");
				}
				if (scan != 0)
				{
					return 1;
				}
				if (z.idata == null)
				{
					return StbImage.stbi__err("no IDAT");
				}
				raw_len = (uint)((uint)((int)((s.img_x * z.depth + 7) / 8) * (int)s.img_y) * s.img_n + s.img_y);
				z.expanded = (byte*)StbImage.stbi_zlib_decode_malloc_guesssize_headerflag((sbyte*)z.idata, (int)ioff, (int)raw_len, (int*)(&raw_len), (is_iphone == 0) ? 1 : 0);
				if (z.expanded == null)
				{
					return 0;
				}
				CRuntime.free(z.idata);
				z.idata = null;
				if ((req_comp == s.img_n + 1 && req_comp != 3 && pal_img_n == 0) || has_trans != 0)
				{
					s.img_out_n = s.img_n + 1;
				}
				else
				{
					s.img_out_n = s.img_n;
				}
				if (StbImage.stbi__create_png_image(z, z.expanded, raw_len, s.img_out_n, z.depth, color, interlace) == 0)
				{
					return 0;
				}
				if (has_trans != 0)
				{
					if (z.depth == 16)
					{
						if (StbImage.stbi__compute_transparency16(z, tc16, s.img_out_n) == 0)
						{
							return 0;
						}
					}
					else if (StbImage.stbi__compute_transparency(z, tc, s.img_out_n) == 0)
					{
						return 0;
					}
				}
				if (is_iphone != 0 && StbImage.stbi__de_iphone_flag != 0 && s.img_out_n > 2)
				{
					StbImage.stbi__de_iphone(z);
				}
				if (pal_img_n != 0)
				{
					s.img_n = pal_img_n;
					s.img_out_n = pal_img_n;
					if (req_comp >= 3)
					{
						s.img_out_n = req_comp;
					}
					if (StbImage.stbi__expand_png_palette(z, palette, (int)pal_len, s.img_out_n) == 0)
					{
						return 0;
					}
				}
				else if (has_trans != 0)
				{
					s.img_n++;
				}
				CRuntime.free(z.expanded);
				z.expanded = null;
				return 1;
			}
			default:
				if (first != 0)
				{
					return StbImage.stbi__err("first not IHDR");
				}
				if ((c.type & 0x20000000) == 0)
				{
					return StbImage.stbi__err(c.type + " PNG chunk not known");
				}
				StbImage.stbi__skip(s, (int)c.length);
				break;
			}
			StbImage.stbi__get32be(s);
		}
	}

	public unsafe static void* stbi__do_png(stbi__png p, int* x, int* y, int* n, int req_comp, stbi__result_info* ri)
	{
		void* result = null;
		if (req_comp < 0 || req_comp > 4)
		{
			return (void*)((StbImage.stbi__err("bad req_comp") != 0) ? 0u : 0u);
		}
		if (StbImage.stbi__parse_png_file(p, 0, req_comp) != 0)
		{
			if (p.depth < 8)
			{
				ri->bits_per_channel = 8;
			}
			else
			{
				ri->bits_per_channel = p.depth;
			}
			result = p._out_;
			p._out_ = null;
			if (req_comp != 0 && req_comp != p.s.img_out_n)
			{
				result = ((ri->bits_per_channel != 8) ? ((void*)StbImage.stbi__convert_format16((ushort*)result, p.s.img_out_n, req_comp, p.s.img_x, p.s.img_y)) : ((void*)StbImage.stbi__convert_format((byte*)result, p.s.img_out_n, req_comp, p.s.img_x, p.s.img_y)));
				p.s.img_out_n = req_comp;
				if (result == null)
				{
					return result;
				}
			}
			*x = (int)p.s.img_x;
			*y = (int)p.s.img_y;
			if (n != null)
			{
				*n = p.s.img_n;
			}
		}
		CRuntime.free(p._out_);
		p._out_ = null;
		CRuntime.free(p.expanded);
		p.expanded = null;
		CRuntime.free(p.idata);
		p.idata = null;
		return result;
	}

	public unsafe static void* stbi__png_load(stbi__context s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri)
	{
		return StbImage.stbi__do_png(new stbi__png
		{
			s = s
		}, x, y, comp, req_comp, ri);
	}

	public static int stbi__png_test(stbi__context s)
	{
		int result = StbImage.stbi__check_png_header(s);
		StbImage.stbi__rewind(s);
		return result;
	}

	public unsafe static int stbi__png_info_raw(stbi__png p, int* x, int* y, int* comp)
	{
		if (StbImage.stbi__parse_png_file(p, 2, 0) == 0)
		{
			StbImage.stbi__rewind(p.s);
			return 0;
		}
		if (x != null)
		{
			*x = (int)p.s.img_x;
		}
		if (y != null)
		{
			*y = (int)p.s.img_y;
		}
		if (comp != null)
		{
			*comp = p.s.img_n;
		}
		return 1;
	}

	public unsafe static int stbi__png_info(stbi__context s, int* x, int* y, int* comp)
	{
		return StbImage.stbi__png_info_raw(new stbi__png
		{
			s = s
		}, x, y, comp);
	}

	public unsafe static int stbi__png_is16(stbi__context s)
	{
		stbi__png p = new stbi__png();
		p.s = s;
		if (StbImage.stbi__png_info_raw(p, null, null, null) == 0)
		{
			return 0;
		}
		if (p.depth != 16)
		{
			StbImage.stbi__rewind(p.s);
			return 0;
		}
		return 1;
	}

	public static int stbi__bmp_test_raw(stbi__context s)
	{
		int sz = 0;
		if (StbImage.stbi__get8(s) != 66)
		{
			return 0;
		}
		if (StbImage.stbi__get8(s) != 77)
		{
			return 0;
		}
		StbImage.stbi__get32le(s);
		StbImage.stbi__get16le(s);
		StbImage.stbi__get16le(s);
		StbImage.stbi__get32le(s);
		sz = (int)StbImage.stbi__get32le(s);
		if (sz != 12 && sz != 40 && sz != 56 && sz != 108 && sz != 124)
		{
			return 0;
		}
		return 1;
	}

	public static int stbi__bmp_test(stbi__context s)
	{
		int result = StbImage.stbi__bmp_test_raw(s);
		StbImage.stbi__rewind(s);
		return result;
	}

	public static int stbi__high_bit(uint z)
	{
		int n = 0;
		if (z == 0)
		{
			return -1;
		}
		if (z >= 65536)
		{
			n += 16;
			z >>= 16;
		}
		if (z >= 256)
		{
			n += 8;
			z >>= 8;
		}
		if (z >= 16)
		{
			n += 4;
			z >>= 4;
		}
		if (z >= 4)
		{
			n += 2;
			z >>= 2;
		}
		if (z >= 2)
		{
			n++;
			z >>= 1;
		}
		return n;
	}

	public static int stbi__bitcount(uint a)
	{
		a = (a & 0x55555555) + ((a >> 1) & 0x55555555);
		a = (a & 0x33333333) + ((a >> 2) & 0x33333333);
		a = (a + (a >> 4)) & 0xF0F0F0F;
		a += a >> 8;
		a += a >> 16;
		return (int)(a & 0xFF);
	}

	public unsafe static int stbi__shiftsigned(uint v, int shift, int bits)
	{
		uint* mul_table = stackalloc uint[9];
		*mul_table = 0u;
		mul_table[1] = 255u;
		mul_table[2] = 85u;
		mul_table[3] = 73u;
		mul_table[4] = 17u;
		mul_table[5] = 33u;
		mul_table[6] = 65u;
		mul_table[7] = 129u;
		mul_table[8] = 1u;
		uint* shift_table = stackalloc uint[9];
		*shift_table = 0u;
		shift_table[1] = 0u;
		shift_table[2] = 0u;
		shift_table[3] = 1u;
		shift_table[4] = 0u;
		shift_table[5] = 2u;
		shift_table[6] = 4u;
		shift_table[7] = 6u;
		shift_table[8] = 0u;
		v = ((shift >= 0) ? (v >> shift) : (v << -shift));
		v >>= 8 - bits;
		return (int)(v * (int)mul_table[bits]) >> (int)shift_table[bits];
	}

	public unsafe static void* stbi__bmp_parse_header(stbi__context s, stbi__bmp_data* info)
	{
		int hsz = 0;
		if (StbImage.stbi__get8(s) != 66 || StbImage.stbi__get8(s) != 77)
		{
			return (void*)((StbImage.stbi__err("not BMP") != 0) ? 0u : 0u);
		}
		StbImage.stbi__get32le(s);
		StbImage.stbi__get16le(s);
		StbImage.stbi__get16le(s);
		info->offset = (int)StbImage.stbi__get32le(s);
		hsz = (info->hsz = (int)StbImage.stbi__get32le(s));
		info->mr = (info->mg = (info->mb = (info->ma = 0u)));
		if (hsz != 12 && hsz != 40 && hsz != 56 && hsz != 108 && hsz != 124)
		{
			return (void*)((StbImage.stbi__err("unknown BMP") != 0) ? 0u : 0u);
		}
		if (hsz == 12)
		{
			s.img_x = (uint)StbImage.stbi__get16le(s);
			s.img_y = (uint)StbImage.stbi__get16le(s);
		}
		else
		{
			s.img_x = StbImage.stbi__get32le(s);
			s.img_y = StbImage.stbi__get32le(s);
		}
		if (StbImage.stbi__get16le(s) != 1)
		{
			return (void*)((StbImage.stbi__err("bad BMP") != 0) ? 0u : 0u);
		}
		info->bpp = StbImage.stbi__get16le(s);
		if (hsz != 12)
		{
			int compress = (int)StbImage.stbi__get32le(s);
			if (compress == 1 || compress == 2)
			{
				return (void*)((StbImage.stbi__err("BMP RLE") != 0) ? 0u : 0u);
			}
			StbImage.stbi__get32le(s);
			StbImage.stbi__get32le(s);
			StbImage.stbi__get32le(s);
			StbImage.stbi__get32le(s);
			StbImage.stbi__get32le(s);
			if (hsz == 40 || hsz == 56)
			{
				if (hsz == 56)
				{
					StbImage.stbi__get32le(s);
					StbImage.stbi__get32le(s);
					StbImage.stbi__get32le(s);
					StbImage.stbi__get32le(s);
				}
				if (info->bpp == 16 || info->bpp == 32)
				{
					switch (compress)
					{
					case 0:
						if (info->bpp == 32)
						{
							info->mr = 16711680u;
							info->mg = 65280u;
							info->mb = 255u;
							info->ma = 4278190080u;
							info->all_a = 0u;
						}
						else
						{
							info->mr = 31744u;
							info->mg = 992u;
							info->mb = 31u;
						}
						break;
					case 3:
						info->mr = StbImage.stbi__get32le(s);
						info->mg = StbImage.stbi__get32le(s);
						info->mb = StbImage.stbi__get32le(s);
						if (info->mr == info->mg && info->mg == info->mb)
						{
							return (void*)((StbImage.stbi__err("bad BMP") != 0) ? 0u : 0u);
						}
						break;
					default:
						return (void*)((StbImage.stbi__err("bad BMP") != 0) ? 0u : 0u);
					}
				}
			}
			else
			{
				int i = 0;
				if (hsz != 108 && hsz != 124)
				{
					return (void*)((StbImage.stbi__err("bad BMP") != 0) ? 0u : 0u);
				}
				info->mr = StbImage.stbi__get32le(s);
				info->mg = StbImage.stbi__get32le(s);
				info->mb = StbImage.stbi__get32le(s);
				info->ma = StbImage.stbi__get32le(s);
				StbImage.stbi__get32le(s);
				for (i = 0; i < 12; i++)
				{
					StbImage.stbi__get32le(s);
				}
				if (hsz == 124)
				{
					StbImage.stbi__get32le(s);
					StbImage.stbi__get32le(s);
					StbImage.stbi__get32le(s);
					StbImage.stbi__get32le(s);
				}
			}
		}
		return (void*)1;
	}

	public unsafe static void* stbi__bmp_load(stbi__context s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri)
	{
		uint mr = 0u;
		uint mg = 0u;
		uint mb = 0u;
		uint ma = 0u;
		uint all_a = 0u;
		byte* pal = stackalloc byte[1024];
		int psize = 0;
		int i = 0;
		int j = 0;
		int width = 0;
		int flip_vertically = 0;
		int pad = 0;
		int target = 0;
		stbi__bmp_data info = new stbi__bmp_data
		{
			all_a = 255u
		};
		if (StbImage.stbi__bmp_parse_header(s, &info) == null)
		{
			return null;
		}
		flip_vertically = (((int)s.img_y > 0) ? 1 : 0);
		s.img_y = (uint)CRuntime.abs((int)s.img_y);
		mr = info.mr;
		mg = info.mg;
		mb = info.mb;
		ma = info.ma;
		all_a = info.all_a;
		if (info.hsz == 12)
		{
			if (info.bpp < 24)
			{
				psize = (info.offset - 14 - 24) / 3;
			}
		}
		else if (info.bpp < 16)
		{
			psize = info.offset - 14 - info.hsz >> 2;
		}
		s.img_n = ((ma != 0) ? 4 : 3);
		target = ((req_comp == 0 || req_comp < 3) ? s.img_n : req_comp);
		if (StbImage.stbi__mad3sizes_valid(target, (int)s.img_x, (int)s.img_y, 0) == 0)
		{
			return (void*)((StbImage.stbi__err("too large") != 0) ? 0u : 0u);
		}
		byte* _out_ = (byte*)StbImage.stbi__malloc_mad3(target, (int)s.img_x, (int)s.img_y, 0);
		if (_out_ == null)
		{
			return (void*)((StbImage.stbi__err("outofmem") != 0) ? 0u : 0u);
		}
		if (info.bpp < 16)
		{
			int z = 0;
			if (psize == 0 || psize > 256)
			{
				CRuntime.free(_out_);
				return (void*)((StbImage.stbi__err("invalid") != 0) ? 0u : 0u);
			}
			for (i = 0; i < psize; i++)
			{
				pal[i * 4 + 2] = StbImage.stbi__get8(s);
				pal[i * 4 + 1] = StbImage.stbi__get8(s);
				pal[i * 4] = StbImage.stbi__get8(s);
				if (info.hsz != 12)
				{
					StbImage.stbi__get8(s);
				}
				pal[i * 4 + 3] = byte.MaxValue;
			}
			StbImage.stbi__skip(s, info.offset - 14 - info.hsz - psize * ((info.hsz == 12) ? 3 : 4));
			if (info.bpp == 1)
			{
				width = (int)(s.img_x + 7 >> 3);
			}
			else if (info.bpp == 4)
			{
				width = (int)(s.img_x + 1 >> 1);
			}
			else
			{
				if (info.bpp != 8)
				{
					CRuntime.free(_out_);
					return (void*)((StbImage.stbi__err("bad bpp") != 0) ? 0u : 0u);
				}
				width = (int)s.img_x;
			}
			pad = -width & 3;
			if (info.bpp == 1)
			{
				for (j = 0; j < (int)s.img_y; j++)
				{
					int bit_offset = 7;
					int v = StbImage.stbi__get8(s);
					for (i = 0; i < (int)s.img_x; i++)
					{
						int color = (v >> bit_offset) & 1;
						_out_[z++] = pal[color * 4];
						_out_[z++] = pal[color * 4 + 1];
						_out_[z++] = pal[color * 4 + 2];
						if (target == 4)
						{
							_out_[z++] = byte.MaxValue;
						}
						if (i + 1 == (int)s.img_x)
						{
							break;
						}
						if (--bit_offset < 0)
						{
							bit_offset = 7;
							v = StbImage.stbi__get8(s);
						}
					}
					StbImage.stbi__skip(s, pad);
				}
			}
			else
			{
				for (j = 0; j < (int)s.img_y; j++)
				{
					for (i = 0; i < (int)s.img_x; i += 2)
					{
						int v2 = StbImage.stbi__get8(s);
						int v3 = 0;
						if (info.bpp == 4)
						{
							v3 = v2 & 0xF;
							v2 >>= 4;
						}
						_out_[z++] = pal[v2 * 4];
						_out_[z++] = pal[v2 * 4 + 1];
						_out_[z++] = pal[v2 * 4 + 2];
						if (target == 4)
						{
							_out_[z++] = byte.MaxValue;
						}
						if (i + 1 == (int)s.img_x)
						{
							break;
						}
						v2 = ((info.bpp == 8) ? StbImage.stbi__get8(s) : v3);
						_out_[z++] = pal[v2 * 4];
						_out_[z++] = pal[v2 * 4 + 1];
						_out_[z++] = pal[v2 * 4 + 2];
						if (target == 4)
						{
							_out_[z++] = byte.MaxValue;
						}
					}
					StbImage.stbi__skip(s, pad);
				}
			}
		}
		else
		{
			int rshift = 0;
			int gshift = 0;
			int bshift = 0;
			int ashift = 0;
			int rcount = 0;
			int gcount = 0;
			int bcount = 0;
			int acount = 0;
			int z2 = 0;
			int easy = 0;
			StbImage.stbi__skip(s, info.offset - 14 - info.hsz);
			width = (int)((info.bpp == 24) ? (3 * s.img_x) : ((info.bpp == 16) ? (2 * s.img_x) : 0));
			pad = -width & 3;
			if (info.bpp == 24)
			{
				easy = 1;
			}
			else if (info.bpp == 32 && mb == 255 && mg == 65280 && mr == 16711680 && ma == 4278190080u)
			{
				easy = 2;
			}
			if (easy == 0)
			{
				if (mr == 0 || mg == 0 || mb == 0)
				{
					CRuntime.free(_out_);
					return (void*)((StbImage.stbi__err("bad masks") != 0) ? 0u : 0u);
				}
				rshift = StbImage.stbi__high_bit(mr) - 7;
				rcount = StbImage.stbi__bitcount(mr);
				gshift = StbImage.stbi__high_bit(mg) - 7;
				gcount = StbImage.stbi__bitcount(mg);
				bshift = StbImage.stbi__high_bit(mb) - 7;
				bcount = StbImage.stbi__bitcount(mb);
				ashift = StbImage.stbi__high_bit(ma) - 7;
				acount = StbImage.stbi__bitcount(ma);
			}
			for (j = 0; j < (int)s.img_y; j++)
			{
				if (easy != 0)
				{
					for (i = 0; i < (int)s.img_x; i++)
					{
						byte a = 0;
						_out_[z2 + 2] = StbImage.stbi__get8(s);
						_out_[z2 + 1] = StbImage.stbi__get8(s);
						_out_[z2] = StbImage.stbi__get8(s);
						z2 += 3;
						a = ((easy == 2) ? StbImage.stbi__get8(s) : byte.MaxValue);
						all_a |= a;
						if (target == 4)
						{
							_out_[z2++] = a;
						}
					}
				}
				else
				{
					int bpp = info.bpp;
					for (i = 0; i < (int)s.img_x; i++)
					{
						uint v4 = ((bpp == 16) ? ((uint)StbImage.stbi__get16le(s)) : StbImage.stbi__get32le(s));
						uint a2 = 0u;
						_out_[z2++] = (byte)(StbImage.stbi__shiftsigned(v4 & mr, rshift, rcount) & 0xFF);
						_out_[z2++] = (byte)(StbImage.stbi__shiftsigned(v4 & mg, gshift, gcount) & 0xFF);
						_out_[z2++] = (byte)(StbImage.stbi__shiftsigned(v4 & mb, bshift, bcount) & 0xFF);
						a2 = ((ma != 0) ? ((uint)StbImage.stbi__shiftsigned(v4 & ma, ashift, acount)) : 255u);
						all_a |= a2;
						if (target == 4)
						{
							_out_[z2++] = (byte)(a2 & 0xFF);
						}
					}
				}
				StbImage.stbi__skip(s, pad);
			}
		}
		if (target == 4 && all_a == 0)
		{
			for (i = (int)(4 * s.img_x * s.img_y - 1); i >= 0; i -= 4)
			{
				_out_[i] = byte.MaxValue;
			}
		}
		if (flip_vertically != 0)
		{
			byte t = 0;
			for (j = 0; j < (int)s.img_y >> 1; j++)
			{
				byte* p1 = _out_ + j * s.img_x * target;
				byte* p2 = _out_ + (s.img_y - 1 - j) * s.img_x * target;
				for (i = 0; i < (int)s.img_x * target; i++)
				{
					t = p1[i];
					p1[i] = p2[i];
					p2[i] = t;
				}
			}
		}
		if (req_comp != 0 && req_comp != target)
		{
			_out_ = StbImage.stbi__convert_format(_out_, target, req_comp, s.img_x, s.img_y);
			if (_out_ == null)
			{
				return _out_;
			}
		}
		*x = (int)s.img_x;
		*y = (int)s.img_y;
		if (comp != null)
		{
			*comp = s.img_n;
		}
		return _out_;
	}

	public unsafe static int stbi__tga_get_comp(int bits_per_pixel, int is_grey, int* is_rgb16)
	{
		if (is_rgb16 != null)
		{
			*is_rgb16 = 0;
		}
		if (bits_per_pixel <= 16)
		{
			if (bits_per_pixel == 8)
			{
				return 1;
			}
			if ((uint)(bits_per_pixel - 15) <= 1u)
			{
				if (bits_per_pixel == 16 && is_grey != 0)
				{
					return 2;
				}
				if (is_rgb16 != null)
				{
					*is_rgb16 = 1;
				}
				return 3;
			}
		}
		else if (bits_per_pixel == 24 || bits_per_pixel == 32)
		{
			return bits_per_pixel / 8;
		}
		return 0;
	}

	public unsafe static int stbi__tga_info(stbi__context s, int* x, int* y, int* comp)
	{
		int tga_w = 0;
		int tga_h = 0;
		int tga_comp = 0;
		int tga_image_type = 0;
		int tga_bits_per_pixel = 0;
		int tga_colormap_bpp = 0;
		int sz = 0;
		int tga_colormap_type = 0;
		StbImage.stbi__get8(s);
		tga_colormap_type = StbImage.stbi__get8(s);
		if (tga_colormap_type > 1)
		{
			StbImage.stbi__rewind(s);
			return 0;
		}
		tga_image_type = StbImage.stbi__get8(s);
		if (tga_colormap_type == 1)
		{
			if (tga_image_type != 1 && tga_image_type != 9)
			{
				StbImage.stbi__rewind(s);
				return 0;
			}
			StbImage.stbi__skip(s, 4);
			sz = StbImage.stbi__get8(s);
			if (sz != 8 && sz != 15 && sz != 16 && sz != 24 && sz != 32)
			{
				StbImage.stbi__rewind(s);
				return 0;
			}
			StbImage.stbi__skip(s, 4);
			tga_colormap_bpp = sz;
		}
		else
		{
			if (tga_image_type != 2 && tga_image_type != 3 && tga_image_type != 10 && tga_image_type != 11)
			{
				StbImage.stbi__rewind(s);
				return 0;
			}
			StbImage.stbi__skip(s, 9);
			tga_colormap_bpp = 0;
		}
		tga_w = StbImage.stbi__get16le(s);
		if (tga_w < 1)
		{
			StbImage.stbi__rewind(s);
			return 0;
		}
		tga_h = StbImage.stbi__get16le(s);
		if (tga_h < 1)
		{
			StbImage.stbi__rewind(s);
			return 0;
		}
		tga_bits_per_pixel = StbImage.stbi__get8(s);
		StbImage.stbi__get8(s);
		if (tga_colormap_bpp != 0)
		{
			if (tga_bits_per_pixel != 8 && tga_bits_per_pixel != 16)
			{
				StbImage.stbi__rewind(s);
				return 0;
			}
			tga_comp = StbImage.stbi__tga_get_comp(tga_colormap_bpp, 0, null);
		}
		else
		{
			tga_comp = StbImage.stbi__tga_get_comp(tga_bits_per_pixel, (tga_image_type == 3 || tga_image_type == 11) ? 1 : 0, null);
		}
		if (tga_comp == 0)
		{
			StbImage.stbi__rewind(s);
			return 0;
		}
		if (x != null)
		{
			*x = tga_w;
		}
		if (y != null)
		{
			*y = tga_h;
		}
		if (comp != null)
		{
			*comp = tga_comp;
		}
		return 1;
	}

	public static int stbi__tga_test(stbi__context s)
	{
		int res = 0;
		int sz = 0;
		int tga_color_type = 0;
		StbImage.stbi__get8(s);
		tga_color_type = StbImage.stbi__get8(s);
		if (tga_color_type <= 1)
		{
			sz = StbImage.stbi__get8(s);
			if (tga_color_type == 1)
			{
				if (sz == 1 || sz == 9)
				{
					StbImage.stbi__skip(s, 4);
					sz = StbImage.stbi__get8(s);
					if (sz == 8 || sz == 15 || sz == 16 || sz == 24 || sz == 32)
					{
						StbImage.stbi__skip(s, 4);
						goto IL_007b;
					}
				}
			}
			else if (sz == 2 || sz == 3 || sz == 10 || sz == 11)
			{
				StbImage.stbi__skip(s, 9);
				goto IL_007b;
			}
		}
		goto IL_00bb;
		IL_007b:
		if (StbImage.stbi__get16le(s) >= 1 && StbImage.stbi__get16le(s) >= 1)
		{
			sz = StbImage.stbi__get8(s);
			if ((tga_color_type != 1 || sz == 8 || sz == 16) && (sz == 8 || sz == 15 || sz == 16 || sz == 24 || sz == 32))
			{
				res = 1;
			}
		}
		goto IL_00bb;
		IL_00bb:
		StbImage.stbi__rewind(s);
		return res;
	}

	public unsafe static void stbi__tga_read_rgb16(stbi__context s, byte* _out_)
	{
		ushort num = (ushort)StbImage.stbi__get16le(s);
		ushort fiveBitMask = 31;
		int r = (num >> 10) & fiveBitMask;
		int g = (num >> 5) & fiveBitMask;
		int b = num & fiveBitMask;
		*_out_ = (byte)(r * 255 / 31);
		_out_[1] = (byte)(g * 255 / 31);
		_out_[2] = (byte)(b * 255 / 31);
	}

	public unsafe static void* stbi__tga_load(stbi__context s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri)
	{
		int tga_offset = StbImage.stbi__get8(s);
		int tga_indexed = StbImage.stbi__get8(s);
		int tga_image_type = StbImage.stbi__get8(s);
		int tga_is_RLE = 0;
		int tga_palette_start = StbImage.stbi__get16le(s);
		int tga_palette_len = StbImage.stbi__get16le(s);
		int tga_palette_bits = StbImage.stbi__get8(s);
		StbImage.stbi__get16le(s);
		StbImage.stbi__get16le(s);
		int tga_width = StbImage.stbi__get16le(s);
		int tga_height = StbImage.stbi__get16le(s);
		int tga_bits_per_pixel = StbImage.stbi__get8(s);
		int tga_comp = 0;
		int tga_rgb16 = 0;
		int tga_inverted = StbImage.stbi__get8(s);
		byte* tga_palette = null;
		int i = 0;
		int j = 0;
		byte* raw_data = stackalloc byte[4];
		*raw_data = 0;
		int RLE_count = 0;
		int RLE_repeating = 0;
		int read_next_pixel = 1;
		if (tga_image_type >= 8)
		{
			tga_image_type -= 8;
			tga_is_RLE = 1;
		}
		tga_inverted = 1 - ((tga_inverted >> 5) & 1);
		tga_comp = ((tga_indexed == 0) ? StbImage.stbi__tga_get_comp(tga_bits_per_pixel, (tga_image_type == 3) ? 1 : 0, &tga_rgb16) : StbImage.stbi__tga_get_comp(tga_palette_bits, 0, &tga_rgb16));
		if (tga_comp == 0)
		{
			return (void*)((StbImage.stbi__err("bad format") != 0) ? 0u : 0u);
		}
		*x = tga_width;
		*y = tga_height;
		if (comp != null)
		{
			*comp = tga_comp;
		}
		if (StbImage.stbi__mad3sizes_valid(tga_width, tga_height, tga_comp, 0) == 0)
		{
			return (void*)((StbImage.stbi__err("too large") != 0) ? 0u : 0u);
		}
		byte* tga_data = (byte*)StbImage.stbi__malloc_mad3(tga_width, tga_height, tga_comp, 0);
		if (tga_data == null)
		{
			return (void*)((StbImage.stbi__err("outofmem") != 0) ? 0u : 0u);
		}
		StbImage.stbi__skip(s, tga_offset);
		if (tga_indexed == 0 && tga_is_RLE == 0 && tga_rgb16 == 0)
		{
			for (i = 0; i < tga_height; i++)
			{
				int row = ((tga_inverted != 0) ? (tga_height - i - 1) : i);
				byte* tga_row = tga_data + row * tga_width * tga_comp;
				StbImage.stbi__getn(s, tga_row, tga_width * tga_comp);
			}
		}
		else
		{
			if (tga_indexed != 0)
			{
				StbImage.stbi__skip(s, tga_palette_start);
				tga_palette = (byte*)StbImage.stbi__malloc_mad2(tga_palette_len, tga_comp, 0);
				if (tga_palette == null)
				{
					CRuntime.free(tga_data);
					return (void*)((StbImage.stbi__err("outofmem") != 0) ? 0u : 0u);
				}
				if (tga_rgb16 != 0)
				{
					byte* pal_entry = tga_palette;
					for (i = 0; i < tga_palette_len; i++)
					{
						StbImage.stbi__tga_read_rgb16(s, pal_entry);
						pal_entry += tga_comp;
					}
				}
				else if (StbImage.stbi__getn(s, tga_palette, tga_palette_len * tga_comp) == 0)
				{
					CRuntime.free(tga_data);
					CRuntime.free(tga_palette);
					return (void*)((StbImage.stbi__err("bad palette") != 0) ? 0u : 0u);
				}
			}
			for (i = 0; i < tga_width * tga_height; i++)
			{
				if (tga_is_RLE != 0)
				{
					if (RLE_count == 0)
					{
						int RLE_cmd = StbImage.stbi__get8(s);
						RLE_count = 1 + (RLE_cmd & 0x7F);
						RLE_repeating = RLE_cmd >> 7;
						read_next_pixel = 1;
					}
					else if (RLE_repeating == 0)
					{
						read_next_pixel = 1;
					}
				}
				else
				{
					read_next_pixel = 1;
				}
				if (read_next_pixel != 0)
				{
					if (tga_indexed != 0)
					{
						int pal_idx = ((tga_bits_per_pixel == 8) ? StbImage.stbi__get8(s) : StbImage.stbi__get16le(s));
						if (pal_idx >= tga_palette_len)
						{
							pal_idx = 0;
						}
						pal_idx *= tga_comp;
						for (j = 0; j < tga_comp; j++)
						{
							raw_data[j] = tga_palette[pal_idx + j];
						}
					}
					else if (tga_rgb16 != 0)
					{
						StbImage.stbi__tga_read_rgb16(s, raw_data);
					}
					else
					{
						for (j = 0; j < tga_comp; j++)
						{
							raw_data[j] = StbImage.stbi__get8(s);
						}
					}
					read_next_pixel = 0;
				}
				for (j = 0; j < tga_comp; j++)
				{
					tga_data[i * tga_comp + j] = raw_data[j];
				}
				RLE_count--;
			}
			if (tga_inverted != 0)
			{
				for (j = 0; j * 2 < tga_height; j++)
				{
					int index1 = j * tga_width * tga_comp;
					int index2 = (tga_height - 1 - j) * tga_width * tga_comp;
					for (i = tga_width * tga_comp; i > 0; i--)
					{
						byte temp = tga_data[index1];
						tga_data[index1] = tga_data[index2];
						tga_data[index2] = temp;
						index1++;
						index2++;
					}
				}
			}
			if (tga_palette != null)
			{
				CRuntime.free(tga_palette);
			}
		}
		if (tga_comp >= 3 && tga_rgb16 == 0)
		{
			byte* tga_pixel = tga_data;
			for (i = 0; i < tga_width * tga_height; i++)
			{
				byte temp2 = *tga_pixel;
				*tga_pixel = tga_pixel[2];
				tga_pixel[2] = temp2;
				tga_pixel += tga_comp;
			}
		}
		if (req_comp != 0 && req_comp != tga_comp)
		{
			tga_data = StbImage.stbi__convert_format(tga_data, tga_comp, req_comp, (uint)tga_width, (uint)tga_height);
		}
		tga_palette_len = (tga_palette_bits = 0);
		return tga_data;
	}

	public static int stbi__psd_test(stbi__context s)
	{
		bool result = StbImage.stbi__get32be(s) == 943870035;
		StbImage.stbi__rewind(s);
		return result ? 1 : 0;
	}

	public unsafe static int stbi__psd_decode_rle(stbi__context s, byte* p, int pixelCount)
	{
		int count = 0;
		int nleft = 0;
		int len = 0;
		count = 0;
		while ((nleft = pixelCount - count) > 0)
		{
			len = StbImage.stbi__get8(s);
			if (len == 128)
			{
				continue;
			}
			if (len < 128)
			{
				len++;
				if (len > nleft)
				{
					return 0;
				}
				count += len;
				while (len != 0)
				{
					*p = StbImage.stbi__get8(s);
					p += 4;
					len--;
				}
			}
			else if (len > 128)
			{
				byte val = 0;
				len = 257 - len;
				if (len > nleft)
				{
					return 0;
				}
				val = StbImage.stbi__get8(s);
				count += len;
				while (len != 0)
				{
					*p = val;
					p += 4;
					len--;
				}
			}
		}
		return 1;
	}

	public unsafe static void* stbi__psd_load(stbi__context s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri, int bpc)
	{
		int pixelCount = 0;
		int channelCount = 0;
		int compression = 0;
		int channel = 0;
		int i = 0;
		int bitdepth = 0;
		int w = 0;
		int h = 0;
		if (StbImage.stbi__get32be(s) != 943870035)
		{
			return (void*)((StbImage.stbi__err("not PSD") != 0) ? 0u : 0u);
		}
		if (StbImage.stbi__get16be(s) != 1)
		{
			return (void*)((StbImage.stbi__err("wrong version") != 0) ? 0u : 0u);
		}
		StbImage.stbi__skip(s, 6);
		channelCount = StbImage.stbi__get16be(s);
		if (channelCount < 0 || channelCount > 16)
		{
			return (void*)((StbImage.stbi__err("wrong channel count") != 0) ? 0u : 0u);
		}
		h = (int)StbImage.stbi__get32be(s);
		w = (int)StbImage.stbi__get32be(s);
		bitdepth = StbImage.stbi__get16be(s);
		if (bitdepth != 8 && bitdepth != 16)
		{
			return (void*)((StbImage.stbi__err("unsupported bit depth") != 0) ? 0u : 0u);
		}
		if (StbImage.stbi__get16be(s) != 3)
		{
			return (void*)((StbImage.stbi__err("wrong color format") != 0) ? 0u : 0u);
		}
		StbImage.stbi__skip(s, (int)StbImage.stbi__get32be(s));
		StbImage.stbi__skip(s, (int)StbImage.stbi__get32be(s));
		StbImage.stbi__skip(s, (int)StbImage.stbi__get32be(s));
		compression = StbImage.stbi__get16be(s);
		if (compression > 1)
		{
			return (void*)((StbImage.stbi__err("bad compression") != 0) ? 0u : 0u);
		}
		if (StbImage.stbi__mad3sizes_valid(4, w, h, 0) == 0)
		{
			return (void*)((StbImage.stbi__err("too large") != 0) ? 0u : 0u);
		}
		byte* _out_;
		if (compression == 0 && bitdepth == 16 && bpc == 16)
		{
			_out_ = (byte*)StbImage.stbi__malloc_mad3(8, w, h, 0);
			ri->bits_per_channel = 16;
		}
		else
		{
			_out_ = (byte*)StbImage.stbi__malloc((ulong)(4 * w * h));
		}
		if (_out_ == null)
		{
			return (void*)((StbImage.stbi__err("outofmem") != 0) ? 0u : 0u);
		}
		pixelCount = w * h;
		if (compression != 0)
		{
			StbImage.stbi__skip(s, h * channelCount * 2);
			for (channel = 0; channel < 4; channel++)
			{
				byte* p = _out_ + channel;
				if (channel >= channelCount)
				{
					i = 0;
					while (i < pixelCount)
					{
						*p = (byte)((channel == 3) ? 255u : 0u);
						i++;
						p += 4;
					}
				}
				else if (StbImage.stbi__psd_decode_rle(s, p, pixelCount) == 0)
				{
					CRuntime.free(_out_);
					return (void*)((StbImage.stbi__err("corrupt") != 0) ? 0u : 0u);
				}
			}
		}
		else
		{
			for (channel = 0; channel < 4; channel++)
			{
				if (channel >= channelCount)
				{
					if (bitdepth == 16 && bpc == 16)
					{
						ushort* q = (ushort*)_out_ + channel;
						ushort val = (ushort)((channel == 3) ? 65535u : 0u);
						i = 0;
						while (i < pixelCount)
						{
							*q = val;
							i++;
							q += 4;
						}
					}
					else
					{
						byte* p2 = _out_ + channel;
						byte val2 = (byte)((channel == 3) ? 255u : 0u);
						i = 0;
						while (i < pixelCount)
						{
							*p2 = val2;
							i++;
							p2 += 4;
						}
					}
					continue;
				}
				if (ri->bits_per_channel == 16)
				{
					ushort* q2 = (ushort*)_out_ + channel;
					i = 0;
					while (i < pixelCount)
					{
						*q2 = (ushort)StbImage.stbi__get16be(s);
						i++;
						q2 += 4;
					}
					continue;
				}
				byte* p3 = _out_ + channel;
				if (bitdepth == 16)
				{
					i = 0;
					while (i < pixelCount)
					{
						*p3 = (byte)(StbImage.stbi__get16be(s) >> 8);
						i++;
						p3 += 4;
					}
				}
				else
				{
					i = 0;
					while (i < pixelCount)
					{
						*p3 = StbImage.stbi__get8(s);
						i++;
						p3 += 4;
					}
				}
			}
		}
		if (channelCount >= 4)
		{
			if (ri->bits_per_channel == 16)
			{
				for (i = 0; i < w * h; i++)
				{
					ushort* pixel = (ushort*)_out_ + 4 * i;
					if (pixel[3] != 0 && pixel[3] != ushort.MaxValue)
					{
						float a = (float)(int)pixel[3] / 65535f;
						float ra = 1f / a;
						float inv_a = 65535f * (1f - ra);
						*pixel = (ushort)((float)(int)(*pixel) * ra + inv_a);
						pixel[1] = (ushort)((float)(int)pixel[1] * ra + inv_a);
						pixel[2] = (ushort)((float)(int)pixel[2] * ra + inv_a);
					}
				}
			}
			else
			{
				for (i = 0; i < w * h; i++)
				{
					byte* pixel2 = _out_ + 4 * i;
					if (pixel2[3] != 0 && pixel2[3] != byte.MaxValue)
					{
						float a2 = (float)(int)pixel2[3] / 255f;
						float ra2 = 1f / a2;
						float inv_a2 = 255f * (1f - ra2);
						*pixel2 = (byte)((float)(int)(*pixel2) * ra2 + inv_a2);
						pixel2[1] = (byte)((float)(int)pixel2[1] * ra2 + inv_a2);
						pixel2[2] = (byte)((float)(int)pixel2[2] * ra2 + inv_a2);
					}
				}
			}
		}
		if (req_comp != 0 && req_comp != 4)
		{
			_out_ = ((ri->bits_per_channel != 16) ? StbImage.stbi__convert_format(_out_, 4, req_comp, (uint)w, (uint)h) : ((byte*)StbImage.stbi__convert_format16((ushort*)_out_, 4, req_comp, (uint)w, (uint)h)));
			if (_out_ == null)
			{
				return _out_;
			}
		}
		if (comp != null)
		{
			*comp = 4;
		}
		*y = h;
		*x = w;
		return _out_;
	}

	public static int stbi__gif_test_raw(stbi__context s)
	{
		int sz = 0;
		if (StbImage.stbi__get8(s) != 71 || StbImage.stbi__get8(s) != 73 || StbImage.stbi__get8(s) != 70 || StbImage.stbi__get8(s) != 56)
		{
			return 0;
		}
		sz = StbImage.stbi__get8(s);
		if (sz != 57 && sz != 55)
		{
			return 0;
		}
		if (StbImage.stbi__get8(s) != 97)
		{
			return 0;
		}
		return 1;
	}

	public static int stbi__gif_test(stbi__context s)
	{
		int result = StbImage.stbi__gif_test_raw(s);
		StbImage.stbi__rewind(s);
		return result;
	}

	public unsafe static int stbi__gif_header(stbi__context s, stbi__gif g, int* comp, int is_info)
	{
		byte version = 0;
		if (StbImage.stbi__get8(s) != 71 || StbImage.stbi__get8(s) != 73 || StbImage.stbi__get8(s) != 70 || StbImage.stbi__get8(s) != 56)
		{
			return StbImage.stbi__err("not GIF");
		}
		version = StbImage.stbi__get8(s);
		if (version != 55 && version != 57)
		{
			return StbImage.stbi__err("not GIF");
		}
		if (StbImage.stbi__get8(s) != 97)
		{
			return StbImage.stbi__err("not GIF");
		}
		StbImage.stbi__g_failure_reason = "";
		g.w = StbImage.stbi__get16le(s);
		g.h = StbImage.stbi__get16le(s);
		g.flags = StbImage.stbi__get8(s);
		g.bgindex = StbImage.stbi__get8(s);
		g.ratio = StbImage.stbi__get8(s);
		g.transparent = -1;
		if (comp != null)
		{
			*comp = 4;
		}
		if (is_info != 0)
		{
			return 1;
		}
		if ((g.flags & 0x80) != 0)
		{
			StbImage.stbi__gif_parse_colortable(s, g.pal, 2 << (g.flags & 7), -1);
		}
		return 1;
	}

	public unsafe static int stbi__gif_info_raw(stbi__context s, int* x, int* y, int* comp)
	{
		stbi__gif g = new stbi__gif();
		if (StbImage.stbi__gif_header(s, g, comp, 1) == 0)
		{
			StbImage.stbi__rewind(s);
			return 0;
		}
		if (x != null)
		{
			*x = g.w;
		}
		if (y != null)
		{
			*y = g.h;
		}
		return 1;
	}

	public unsafe static void stbi__out_gif_code(stbi__gif g, ushort code)
	{
		int idx = 0;
		if (g.codes[(int)code].prefix >= 0)
		{
			StbImage.stbi__out_gif_code(g, (ushort)g.codes[(int)code].prefix);
		}
		if (g.cur_y >= g.max_y)
		{
			return;
		}
		idx = g.cur_x + g.cur_y;
		byte* p = g._out_ + idx;
		g.history[idx / 4] = 1;
		byte* c = g.color_table + g.codes[(int)code].suffix * 4;
		if (c[3] > 128)
		{
			*p = c[2];
			p[1] = c[1];
			p[2] = *c;
			p[3] = c[3];
		}
		g.cur_x += 4;
		if (g.cur_x >= g.max_x)
		{
			g.cur_x = g.start_x;
			g.cur_y += g.step;
			while (g.cur_y >= g.max_y && g.parse > 0)
			{
				g.step = (1 << g.parse) * g.line_size;
				g.cur_y = g.start_y + (g.step >> 1);
				g.parse--;
			}
		}
	}

	public unsafe static byte* stbi__process_gif_raster(stbi__context s, stbi__gif g)
	{
		byte lzw_cs = 0;
		int len = 0;
		int init_code = 0;
		uint first = 0u;
		int codesize = 0;
		int codemask = 0;
		int avail = 0;
		int oldcode = 0;
		int bits = 0;
		int valid_bits = 0;
		int clear = 0;
		lzw_cs = StbImage.stbi__get8(s);
		if (lzw_cs > 12)
		{
			return null;
		}
		clear = 1 << (int)lzw_cs;
		first = 1u;
		codesize = lzw_cs + 1;
		codemask = (1 << codesize) - 1;
		bits = 0;
		valid_bits = 0;
		for (init_code = 0; init_code < clear; init_code++)
		{
			g.codes[init_code].prefix = -1;
			g.codes[init_code].first = (byte)init_code;
			g.codes[init_code].suffix = (byte)init_code;
		}
		avail = clear + 2;
		oldcode = -1;
		len = 0;
		while (true)
		{
			if (valid_bits < codesize)
			{
				if (len == 0)
				{
					len = StbImage.stbi__get8(s);
					if (len == 0)
					{
						return g._out_;
					}
				}
				len--;
				bits |= StbImage.stbi__get8(s) << valid_bits;
				valid_bits += 8;
				continue;
			}
			int code = bits & codemask;
			bits >>= codesize;
			valid_bits -= codesize;
			if (code == clear)
			{
				codesize = lzw_cs + 1;
				codemask = (1 << codesize) - 1;
				avail = clear + 2;
				oldcode = -1;
				first = 0u;
				continue;
			}
			if (code == clear + 1)
			{
				StbImage.stbi__skip(s, len);
				while ((len = StbImage.stbi__get8(s)) > 0)
				{
					StbImage.stbi__skip(s, len);
				}
				return g._out_;
			}
			if (code > avail)
			{
				break;
			}
			if (first != 0)
			{
				return (byte*)((StbImage.stbi__err("no clear code") != 0) ? 0u : 0u);
			}
			if (oldcode >= 0)
			{
				stbi__gif_lzw* p = g.codes + avail++;
				if (avail > 8192)
				{
					return (byte*)((StbImage.stbi__err("too many codes") != 0) ? 0u : 0u);
				}
				p->prefix = (short)oldcode;
				p->first = g.codes[oldcode].first;
				p->suffix = ((code == avail) ? p->first : g.codes[code].first);
			}
			else if (code == avail)
			{
				return (byte*)((StbImage.stbi__err("illegal code in raster") != 0) ? 0u : 0u);
			}
			StbImage.stbi__out_gif_code(g, (ushort)code);
			if ((avail & codemask) == 0 && avail <= 4095)
			{
				codesize++;
				codemask = (1 << codesize) - 1;
			}
			oldcode = code;
		}
		return (byte*)((StbImage.stbi__err("illegal code in raster") != 0) ? 0u : 0u);
	}

	public unsafe static byte* stbi__gif_load_next(stbi__context s, stbi__gif g, int* comp, int req_comp, byte* two_back)
	{
		int dispose = 0;
		int first_frame = 0;
		int pi = 0;
		int pcount = 0;
		first_frame = 0;
		if (g._out_ == null)
		{
			if (StbImage.stbi__gif_header(s, g, comp, 0) == 0)
			{
				return null;
			}
			if (StbImage.stbi__mad3sizes_valid(4, g.w, g.h, 0) == 0)
			{
				return (byte*)((StbImage.stbi__err("too large") != 0) ? 0u : 0u);
			}
			pcount = g.w * g.h;
			g._out_ = (byte*)StbImage.stbi__malloc((ulong)(4 * pcount));
			g.background = (byte*)StbImage.stbi__malloc((ulong)(4 * pcount));
			g.history = (byte*)StbImage.stbi__malloc((ulong)pcount);
			if (g._out_ == null || g.background == null || g.history == null)
			{
				return (byte*)((StbImage.stbi__err("outofmem") != 0) ? 0u : 0u);
			}
			CRuntime.memset(g._out_, 0, (ulong)(4 * pcount));
			CRuntime.memset(g.background, 0, (ulong)(4 * pcount));
			CRuntime.memset(g.history, 0, (ulong)pcount);
			first_frame = 1;
		}
		else
		{
			dispose = (g.eflags & 0x1C) >> 2;
			pcount = g.w * g.h;
			if (dispose == 3 && two_back == null)
			{
				dispose = 2;
			}
			switch (dispose)
			{
			case 3:
				for (pi = 0; pi < pcount; pi++)
				{
					if (g.history[pi] != 0)
					{
						CRuntime.memcpy(g._out_ + pi * 4, two_back + pi * 4, 4uL);
					}
				}
				break;
			case 2:
				for (pi = 0; pi < pcount; pi++)
				{
					if (g.history[pi] != 0)
					{
						CRuntime.memcpy(g._out_ + pi * 4, g.background + pi * 4, 4uL);
					}
				}
				break;
			}
			CRuntime.memcpy(g.background, g._out_, (ulong)(4 * g.w * g.h));
		}
		CRuntime.memset(g.history, 0, (ulong)(g.w * g.h));
		while (true)
		{
			switch (StbImage.stbi__get8(s))
			{
			case 44:
			{
				int x = 0;
				int y = 0;
				int w = 0;
				int h = 0;
				x = StbImage.stbi__get16le(s);
				y = StbImage.stbi__get16le(s);
				w = StbImage.stbi__get16le(s);
				h = StbImage.stbi__get16le(s);
				if (x + w > g.w || y + h > g.h)
				{
					return (byte*)((StbImage.stbi__err("bad Image Descriptor") != 0) ? 0u : 0u);
				}
				g.line_size = g.w * 4;
				g.start_x = x * 4;
				g.start_y = y * g.line_size;
				g.max_x = g.start_x + w * 4;
				g.max_y = g.start_y + h * g.line_size;
				g.cur_x = g.start_x;
				g.cur_y = g.start_y;
				if (w == 0)
				{
					g.cur_y = g.max_y;
				}
				g.lflags = StbImage.stbi__get8(s);
				if ((g.lflags & 0x40) != 0)
				{
					g.step = 8 * g.line_size;
					g.parse = 3;
				}
				else
				{
					g.step = g.line_size;
					g.parse = 0;
				}
				if ((g.lflags & 0x80) != 0)
				{
					StbImage.stbi__gif_parse_colortable(s, g.lpal, 2 << (g.lflags & 7), ((g.eflags & 1) != 0) ? g.transparent : (-1));
					g.color_table = g.lpal;
				}
				else
				{
					if ((g.flags & 0x80) == 0)
					{
						return (byte*)((StbImage.stbi__err("missing color table") != 0) ? 0u : 0u);
					}
					g.color_table = g.pal;
				}
				byte* o = StbImage.stbi__process_gif_raster(s, g);
				if (o == null)
				{
					return null;
				}
				pcount = g.w * g.h;
				if (first_frame != 0 && g.bgindex > 0)
				{
					for (pi = 0; pi < pcount; pi++)
					{
						if (g.history[pi] == 0)
						{
							g.pal[g.bgindex * 4 + 3] = byte.MaxValue;
							CRuntime.memcpy(g._out_ + pi * 4, g.pal + g.bgindex, 4uL);
						}
					}
				}
				return o;
			}
			case 33:
			{
				int len = 0;
				if (StbImage.stbi__get8(s) == 249)
				{
					len = StbImage.stbi__get8(s);
					if (len != 4)
					{
						StbImage.stbi__skip(s, len);
						break;
					}
					g.eflags = StbImage.stbi__get8(s);
					g.delay = 10 * StbImage.stbi__get16le(s);
					if (g.transparent >= 0)
					{
						g.pal[g.transparent * 4 + 3] = byte.MaxValue;
					}
					if ((g.eflags & 1) != 0)
					{
						g.transparent = StbImage.stbi__get8(s);
						if (g.transparent >= 0)
						{
							g.pal[g.transparent * 4 + 3] = 0;
						}
					}
					else
					{
						StbImage.stbi__skip(s, 1);
						g.transparent = -1;
					}
				}
				while ((len = StbImage.stbi__get8(s)) != 0)
				{
					StbImage.stbi__skip(s, len);
				}
				break;
			}
			case 59:
				return null;
			default:
				return (byte*)((StbImage.stbi__err("unknown code") != 0) ? 0u : 0u);
			}
		}
	}

	public unsafe static void* stbi__load_gif_main(stbi__context s, int** delays, int* x, int* y, int* z, int* comp, int req_comp)
	{
		if (StbImage.stbi__gif_test(s) != 0)
		{
			int layers = 0;
			byte* u = null;
			byte* _out_ = null;
			byte* two_back = null;
			stbi__gif g = new stbi__gif();
			int stride = 0;
			if (delays != null)
			{
				*delays = null;
			}
			do
			{
				u = StbImage.stbi__gif_load_next(s, g, comp, req_comp, two_back);
				if (u == null)
				{
					continue;
				}
				*x = g.w;
				*y = g.h;
				layers++;
				stride = g.w * g.h * 4;
				if (_out_ != null)
				{
					_out_ = (byte*)CRuntime.realloc(_out_, (ulong)(layers * stride));
					if (delays != null)
					{
						*delays = (int*)CRuntime.realloc(*delays, (ulong)(4 * layers));
					}
				}
				else
				{
					_out_ = (byte*)StbImage.stbi__malloc((ulong)(layers * stride));
					if (delays != null)
					{
						*delays = (int*)StbImage.stbi__malloc((ulong)(layers * 4));
					}
				}
				CRuntime.memcpy(_out_ + (layers - 1) * stride, u, (ulong)stride);
				if (layers >= 2)
				{
					two_back = _out_ - 2 * stride;
				}
				if (delays != null)
				{
					(*delays)[(long)layers - 1L] = g.delay;
				}
			}
			while (u != null);
			CRuntime.free(g._out_);
			CRuntime.free(g.history);
			CRuntime.free(g.background);
			if (req_comp != 0 && req_comp != 4)
			{
				_out_ = StbImage.stbi__convert_format(_out_, 4, req_comp, (uint)(layers * g.w), (uint)g.h);
			}
			*z = layers;
			return _out_;
		}
		return (void*)((StbImage.stbi__err("not GIF") != 0) ? 0u : 0u);
	}

	public unsafe static void* stbi__gif_load(stbi__context s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri)
	{
		byte* u = null;
		stbi__gif g = new stbi__gif();
		u = StbImage.stbi__gif_load_next(s, g, comp, req_comp, null);
		if (u != null)
		{
			*x = g.w;
			*y = g.h;
			if (req_comp != 0 && req_comp != 4)
			{
				u = StbImage.stbi__convert_format(u, 4, req_comp, (uint)g.w, (uint)g.h);
			}
		}
		else if (g._out_ != null)
		{
			CRuntime.free(g._out_);
		}
		CRuntime.free(g.history);
		CRuntime.free(g.background);
		return u;
	}

	public unsafe static int stbi__gif_info(stbi__context s, int* x, int* y, int* comp)
	{
		return StbImage.stbi__gif_info_raw(s, x, y, comp);
	}

	public unsafe static int stbi__bmp_info(stbi__context s, int* x, int* y, int* comp)
	{
		stbi__bmp_data info = new stbi__bmp_data
		{
			all_a = 255u
		};
		void* intPtr = StbImage.stbi__bmp_parse_header(s, &info);
		StbImage.stbi__rewind(s);
		if (intPtr == null)
		{
			return 0;
		}
		if (x != null)
		{
			*x = (int)s.img_x;
		}
		if (y != null)
		{
			*y = (int)s.img_y;
		}
		if (comp != null)
		{
			*comp = ((info.ma != 0) ? 4 : 3);
		}
		return 1;
	}

	public unsafe static int stbi__psd_info(stbi__context s, int* x, int* y, int* comp)
	{
		int channelCount = 0;
		int dummy = 0;
		int depth = 0;
		if (x == null)
		{
			x = &dummy;
		}
		if (y == null)
		{
			y = &dummy;
		}
		if (comp == null)
		{
			comp = &dummy;
		}
		if (StbImage.stbi__get32be(s) != 943870035)
		{
			StbImage.stbi__rewind(s);
			return 0;
		}
		if (StbImage.stbi__get16be(s) != 1)
		{
			StbImage.stbi__rewind(s);
			return 0;
		}
		StbImage.stbi__skip(s, 6);
		channelCount = StbImage.stbi__get16be(s);
		if (channelCount < 0 || channelCount > 16)
		{
			StbImage.stbi__rewind(s);
			return 0;
		}
		*y = (int)StbImage.stbi__get32be(s);
		*x = (int)StbImage.stbi__get32be(s);
		depth = StbImage.stbi__get16be(s);
		if (depth != 8 && depth != 16)
		{
			StbImage.stbi__rewind(s);
			return 0;
		}
		if (StbImage.stbi__get16be(s) != 3)
		{
			StbImage.stbi__rewind(s);
			return 0;
		}
		*comp = 4;
		return 1;
	}

	public static int stbi__psd_is16(stbi__context s)
	{
		int channelCount = 0;
		if (StbImage.stbi__get32be(s) != 943870035)
		{
			StbImage.stbi__rewind(s);
			return 0;
		}
		if (StbImage.stbi__get16be(s) != 1)
		{
			StbImage.stbi__rewind(s);
			return 0;
		}
		StbImage.stbi__skip(s, 6);
		channelCount = StbImage.stbi__get16be(s);
		if (channelCount < 0 || channelCount > 16)
		{
			StbImage.stbi__rewind(s);
			return 0;
		}
		StbImage.stbi__get32be(s);
		StbImage.stbi__get32be(s);
		if (StbImage.stbi__get16be(s) != 16)
		{
			StbImage.stbi__rewind(s);
			return 0;
		}
		return 1;
	}

	public unsafe static int stbi__info_main(stbi__context s, int* x, int* y, int* comp)
	{
		if (StbImage.stbi__jpeg_info(s, x, y, comp) != 0)
		{
			return 1;
		}
		if (StbImage.stbi__png_info(s, x, y, comp) != 0)
		{
			return 1;
		}
		if (StbImage.stbi__gif_info(s, x, y, comp) != 0)
		{
			return 1;
		}
		if (StbImage.stbi__bmp_info(s, x, y, comp) != 0)
		{
			return 1;
		}
		if (StbImage.stbi__psd_info(s, x, y, comp) != 0)
		{
			return 1;
		}
		if (StbImage.stbi__tga_info(s, x, y, comp) != 0)
		{
			return 1;
		}
		return StbImage.stbi__err("unknown image type");
	}

	public static int stbi__is_16_main(stbi__context s)
	{
		if (StbImage.stbi__png_is16(s) != 0)
		{
			return 1;
		}
		if (StbImage.stbi__psd_is16(s) != 0)
		{
			return 1;
		}
		return 0;
	}

	public unsafe static int stbi_info_from_memory(byte* buffer, int len, int* x, int* y, int* comp)
	{
		stbi__context s = new stbi__context();
		StbImage.stbi__start_mem(s, buffer, len);
		return StbImage.stbi__info_main(s, x, y, comp);
	}

	public unsafe static int stbi_info_from_callbacks(stbi_io_callbacks c, void* user, int* x, int* y, int* comp)
	{
		stbi__context s = new stbi__context();
		StbImage.stbi__start_callbacks(s, c, user);
		return StbImage.stbi__info_main(s, x, y, comp);
	}

	public unsafe static int stbi_is_16_bit_from_memory(byte* buffer, int len)
	{
		stbi__context s = new stbi__context();
		StbImage.stbi__start_mem(s, buffer, len);
		return StbImage.stbi__is_16_main(s);
	}

	public unsafe static int stbi_is_16_bit_from_callbacks(stbi_io_callbacks c, void* user)
	{
		stbi__context s = new stbi__context();
		StbImage.stbi__start_callbacks(s, c, user);
		return StbImage.stbi__is_16_main(s);
	}
}
