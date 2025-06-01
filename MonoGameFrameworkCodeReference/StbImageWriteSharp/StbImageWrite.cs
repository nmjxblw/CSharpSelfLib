using System.Text;

namespace StbImageWriteSharp;

internal static class StbImageWrite
{
	public unsafe delegate int WriteCallback(void* context, void* data, int size);

	public class stbi__write_context
	{
		public WriteCallback func;

		public unsafe void* context;
	}

	public static int stbi_write_tga_with_rle = 1;

	public static int stbi__flip_vertically_on_write = 0;

	public static int stbi_write_png_compression_level = 8;

	public static int stbi_write_force_png_filter = -1;

	public static ushort[] lengthc = new ushort[30]
	{
		3, 4, 5, 6, 7, 8, 9, 10, 11, 13,
		15, 17, 19, 23, 27, 31, 35, 43, 51, 59,
		67, 83, 99, 115, 131, 163, 195, 227, 258, 259
	};

	public static byte[] lengtheb = new byte[29]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
		1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
		4, 4, 4, 4, 5, 5, 5, 5, 0
	};

	public static ushort[] distc = new ushort[31]
	{
		1, 2, 3, 4, 5, 7, 9, 13, 17, 25,
		33, 49, 65, 97, 129, 193, 257, 385, 513, 769,
		1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577,
		32768
	};

	public static byte[] disteb = new byte[30]
	{
		0, 0, 0, 0, 1, 1, 2, 2, 3, 3,
		4, 4, 5, 5, 6, 6, 7, 7, 8, 8,
		9, 9, 10, 10, 11, 11, 12, 12, 13, 13
	};

	public static uint[] crc_table = new uint[256]
	{
		0u, 1996959894u, 3993919788u, 2567524794u, 124634137u, 1886057615u, 3915621685u, 2657392035u, 249268274u, 2044508324u,
		3772115230u, 2547177864u, 162941995u, 2125561021u, 3887607047u, 2428444049u, 498536548u, 1789927666u, 4089016648u, 2227061214u,
		450548861u, 1843258603u, 4107580753u, 2211677639u, 325883990u, 1684777152u, 4251122042u, 2321926636u, 335633487u, 1661365465u,
		4195302755u, 2366115317u, 997073096u, 1281953886u, 3579855332u, 2724688242u, 1006888145u, 1258607687u, 3524101629u, 2768942443u,
		901097722u, 1119000684u, 3686517206u, 2898065728u, 853044451u, 1172266101u, 3705015759u, 2882616665u, 651767980u, 1373503546u,
		3369554304u, 3218104598u, 565507253u, 1454621731u, 3485111705u, 3099436303u, 671266974u, 1594198024u, 3322730930u, 2970347812u,
		795835527u, 1483230225u, 3244367275u, 3060149565u, 1994146192u, 31158534u, 2563907772u, 4023717930u, 1907459465u, 112637215u,
		2680153253u, 3904427059u, 2013776290u, 251722036u, 2517215374u, 3775830040u, 2137656763u, 141376813u, 2439277719u, 3865271297u,
		1802195444u, 476864866u, 2238001368u, 4066508878u, 1812370925u, 453092731u, 2181625025u, 4111451223u, 1706088902u, 314042704u,
		2344532202u, 4240017532u, 1658658271u, 366619977u, 2362670323u, 4224994405u, 1303535960u, 984961486u, 2747007092u, 3569037538u,
		1256170817u, 1037604311u, 2765210733u, 3554079995u, 1131014506u, 879679996u, 2909243462u, 3663771856u, 1141124467u, 855842277u,
		2852801631u, 3708648649u, 1342533948u, 654459306u, 3188396048u, 3373015174u, 1466479909u, 544179635u, 3110523913u, 3462522015u,
		1591671054u, 702138776u, 2966460450u, 3352799412u, 1504918807u, 783551873u, 3082640443u, 3233442989u, 3988292384u, 2596254646u,
		62317068u, 1957810842u, 3939845945u, 2647816111u, 81470997u, 1943803523u, 3814918930u, 2489596804u, 225274430u, 2053790376u,
		3826175755u, 2466906013u, 167816743u, 2097651377u, 4027552580u, 2265490386u, 503444072u, 1762050814u, 4150417245u, 2154129355u,
		426522225u, 1852507879u, 4275313526u, 2312317920u, 282753626u, 1742555852u, 4189708143u, 2394877945u, 397917763u, 1622183637u,
		3604390888u, 2714866558u, 953729732u, 1340076626u, 3518719985u, 2797360999u, 1068828381u, 1219638859u, 3624741850u, 2936675148u,
		906185462u, 1090812512u, 3747672003u, 2825379669u, 829329135u, 1181335161u, 3412177804u, 3160834842u, 628085408u, 1382605366u,
		3423369109u, 3138078467u, 570562233u, 1426400815u, 3317316542u, 2998733608u, 733239954u, 1555261956u, 3268935591u, 3050360625u,
		752459403u, 1541320221u, 2607071920u, 3965973030u, 1969922972u, 40735498u, 2617837225u, 3943577151u, 1913087877u, 83908371u,
		2512341634u, 3803740692u, 2075208622u, 213261112u, 2463272603u, 3855990285u, 2094854071u, 198958881u, 2262029012u, 4057260610u,
		1759359992u, 534414190u, 2176718541u, 4139329115u, 1873836001u, 414664567u, 2282248934u, 4279200368u, 1711684554u, 285281116u,
		2405801727u, 4167216745u, 1634467795u, 376229701u, 2685067896u, 3608007406u, 1308918612u, 956543938u, 2808555105u, 3495958263u,
		1231636301u, 1047427035u, 2932959818u, 3654703836u, 1088359270u, 936918000u, 2847714899u, 3736837829u, 1202900863u, 817233897u,
		3183342108u, 3401237130u, 1404277552u, 615818150u, 3134207493u, 3453421203u, 1423857449u, 601450431u, 3009837614u, 3294710456u,
		1567103746u, 711928724u, 3020668471u, 3272380065u, 1510334235u, 755167117u
	};

	public static byte[] stbiw__jpg_ZigZag = new byte[64]
	{
		0, 1, 5, 6, 14, 15, 27, 28, 2, 4,
		7, 13, 16, 26, 29, 42, 3, 8, 12, 17,
		25, 30, 41, 43, 9, 11, 18, 24, 31, 40,
		44, 53, 10, 19, 23, 32, 39, 45, 52, 54,
		20, 22, 33, 38, 46, 51, 55, 60, 21, 34,
		37, 47, 50, 56, 59, 61, 35, 36, 48, 49,
		57, 58, 62, 63
	};

	public static byte[] std_dc_luminance_nrcodes = new byte[17]
	{
		0, 0, 1, 5, 1, 1, 1, 1, 1, 1,
		0, 0, 0, 0, 0, 0, 0
	};

	public static byte[] std_dc_luminance_values = new byte[12]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
		10, 11
	};

	public static byte[] std_ac_luminance_nrcodes = new byte[17]
	{
		0, 0, 2, 1, 3, 3, 2, 4, 3, 5,
		5, 4, 4, 0, 0, 1, 125
	};

	public static byte[] std_ac_luminance_values = new byte[162]
	{
		1, 2, 3, 0, 4, 17, 5, 18, 33, 49,
		65, 6, 19, 81, 97, 7, 34, 113, 20, 50,
		129, 145, 161, 8, 35, 66, 177, 193, 21, 82,
		209, 240, 36, 51, 98, 114, 130, 9, 10, 22,
		23, 24, 25, 26, 37, 38, 39, 40, 41, 42,
		52, 53, 54, 55, 56, 57, 58, 67, 68, 69,
		70, 71, 72, 73, 74, 83, 84, 85, 86, 87,
		88, 89, 90, 99, 100, 101, 102, 103, 104, 105,
		106, 115, 116, 117, 118, 119, 120, 121, 122, 131,
		132, 133, 134, 135, 136, 137, 138, 146, 147, 148,
		149, 150, 151, 152, 153, 154, 162, 163, 164, 165,
		166, 167, 168, 169, 170, 178, 179, 180, 181, 182,
		183, 184, 185, 186, 194, 195, 196, 197, 198, 199,
		200, 201, 202, 210, 211, 212, 213, 214, 215, 216,
		217, 218, 225, 226, 227, 228, 229, 230, 231, 232,
		233, 234, 241, 242, 243, 244, 245, 246, 247, 248,
		249, 250
	};

	public static byte[] std_dc_chrominance_nrcodes = new byte[17]
	{
		0, 0, 3, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 0, 0, 0, 0, 0
	};

	public static byte[] std_dc_chrominance_values = new byte[12]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
		10, 11
	};

	public static byte[] std_ac_chrominance_nrcodes = new byte[17]
	{
		0, 0, 2, 1, 2, 4, 4, 3, 4, 7,
		5, 4, 4, 0, 1, 2, 119
	};

	public static byte[] std_ac_chrominance_values = new byte[162]
	{
		0, 1, 2, 3, 17, 4, 5, 33, 49, 6,
		18, 65, 81, 7, 97, 113, 19, 34, 50, 129,
		8, 20, 66, 145, 161, 177, 193, 9, 35, 51,
		82, 240, 21, 98, 114, 209, 10, 22, 36, 52,
		225, 37, 241, 23, 24, 25, 26, 38, 39, 40,
		41, 42, 53, 54, 55, 56, 57, 58, 67, 68,
		69, 70, 71, 72, 73, 74, 83, 84, 85, 86,
		87, 88, 89, 90, 99, 100, 101, 102, 103, 104,
		105, 106, 115, 116, 117, 118, 119, 120, 121, 122,
		130, 131, 132, 133, 134, 135, 136, 137, 138, 146,
		147, 148, 149, 150, 151, 152, 153, 154, 162, 163,
		164, 165, 166, 167, 168, 169, 170, 178, 179, 180,
		181, 182, 183, 184, 185, 186, 194, 195, 196, 197,
		198, 199, 200, 201, 202, 210, 211, 212, 213, 214,
		215, 216, 217, 218, 226, 227, 228, 229, 230, 231,
		232, 233, 234, 242, 243, 244, 245, 246, 247, 248,
		249, 250
	};

	public static ushort[,] YDC_HT = new ushort[12, 2]
	{
		{ 0, 2 },
		{ 2, 3 },
		{ 3, 3 },
		{ 4, 3 },
		{ 5, 3 },
		{ 6, 3 },
		{ 14, 4 },
		{ 30, 5 },
		{ 62, 6 },
		{ 126, 7 },
		{ 254, 8 },
		{ 510, 9 }
	};

	public static ushort[,] UVDC_HT = new ushort[12, 2]
	{
		{ 0, 2 },
		{ 1, 2 },
		{ 2, 2 },
		{ 6, 3 },
		{ 14, 4 },
		{ 30, 5 },
		{ 62, 6 },
		{ 126, 7 },
		{ 254, 8 },
		{ 510, 9 },
		{ 1022, 10 },
		{ 2046, 11 }
	};

	public static ushort[,] YAC_HT = new ushort[256, 2]
	{
		{ 10, 4 },
		{ 0, 2 },
		{ 1, 2 },
		{ 4, 3 },
		{ 11, 4 },
		{ 26, 5 },
		{ 120, 7 },
		{ 248, 8 },
		{ 1014, 10 },
		{ 65410, 16 },
		{ 65411, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 12, 4 },
		{ 27, 5 },
		{ 121, 7 },
		{ 502, 9 },
		{ 2038, 11 },
		{ 65412, 16 },
		{ 65413, 16 },
		{ 65414, 16 },
		{ 65415, 16 },
		{ 65416, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 28, 5 },
		{ 249, 8 },
		{ 1015, 10 },
		{ 4084, 12 },
		{ 65417, 16 },
		{ 65418, 16 },
		{ 65419, 16 },
		{ 65420, 16 },
		{ 65421, 16 },
		{ 65422, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 58, 6 },
		{ 503, 9 },
		{ 4085, 12 },
		{ 65423, 16 },
		{ 65424, 16 },
		{ 65425, 16 },
		{ 65426, 16 },
		{ 65427, 16 },
		{ 65428, 16 },
		{ 65429, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 59, 6 },
		{ 1016, 10 },
		{ 65430, 16 },
		{ 65431, 16 },
		{ 65432, 16 },
		{ 65433, 16 },
		{ 65434, 16 },
		{ 65435, 16 },
		{ 65436, 16 },
		{ 65437, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 122, 7 },
		{ 2039, 11 },
		{ 65438, 16 },
		{ 65439, 16 },
		{ 65440, 16 },
		{ 65441, 16 },
		{ 65442, 16 },
		{ 65443, 16 },
		{ 65444, 16 },
		{ 65445, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 123, 7 },
		{ 4086, 12 },
		{ 65446, 16 },
		{ 65447, 16 },
		{ 65448, 16 },
		{ 65449, 16 },
		{ 65450, 16 },
		{ 65451, 16 },
		{ 65452, 16 },
		{ 65453, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 250, 8 },
		{ 4087, 12 },
		{ 65454, 16 },
		{ 65455, 16 },
		{ 65456, 16 },
		{ 65457, 16 },
		{ 65458, 16 },
		{ 65459, 16 },
		{ 65460, 16 },
		{ 65461, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 504, 9 },
		{ 32704, 15 },
		{ 65462, 16 },
		{ 65463, 16 },
		{ 65464, 16 },
		{ 65465, 16 },
		{ 65466, 16 },
		{ 65467, 16 },
		{ 65468, 16 },
		{ 65469, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 505, 9 },
		{ 65470, 16 },
		{ 65471, 16 },
		{ 65472, 16 },
		{ 65473, 16 },
		{ 65474, 16 },
		{ 65475, 16 },
		{ 65476, 16 },
		{ 65477, 16 },
		{ 65478, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 506, 9 },
		{ 65479, 16 },
		{ 65480, 16 },
		{ 65481, 16 },
		{ 65482, 16 },
		{ 65483, 16 },
		{ 65484, 16 },
		{ 65485, 16 },
		{ 65486, 16 },
		{ 65487, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 1017, 10 },
		{ 65488, 16 },
		{ 65489, 16 },
		{ 65490, 16 },
		{ 65491, 16 },
		{ 65492, 16 },
		{ 65493, 16 },
		{ 65494, 16 },
		{ 65495, 16 },
		{ 65496, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 1018, 10 },
		{ 65497, 16 },
		{ 65498, 16 },
		{ 65499, 16 },
		{ 65500, 16 },
		{ 65501, 16 },
		{ 65502, 16 },
		{ 65503, 16 },
		{ 65504, 16 },
		{ 65505, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 2040, 11 },
		{ 65506, 16 },
		{ 65507, 16 },
		{ 65508, 16 },
		{ 65509, 16 },
		{ 65510, 16 },
		{ 65511, 16 },
		{ 65512, 16 },
		{ 65513, 16 },
		{ 65514, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 65515, 16 },
		{ 65516, 16 },
		{ 65517, 16 },
		{ 65518, 16 },
		{ 65519, 16 },
		{ 65520, 16 },
		{ 65521, 16 },
		{ 65522, 16 },
		{ 65523, 16 },
		{ 65524, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 2041, 11 },
		{ 65525, 16 },
		{ 65526, 16 },
		{ 65527, 16 },
		{ 65528, 16 },
		{ 65529, 16 },
		{ 65530, 16 },
		{ 65531, 16 },
		{ 65532, 16 },
		{ 65533, 16 },
		{ 65534, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 }
	};

	public static ushort[,] UVAC_HT = new ushort[256, 2]
	{
		{ 0, 2 },
		{ 1, 2 },
		{ 4, 3 },
		{ 10, 4 },
		{ 24, 5 },
		{ 25, 5 },
		{ 56, 6 },
		{ 120, 7 },
		{ 500, 9 },
		{ 1014, 10 },
		{ 4084, 12 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 11, 4 },
		{ 57, 6 },
		{ 246, 8 },
		{ 501, 9 },
		{ 2038, 11 },
		{ 4085, 12 },
		{ 65416, 16 },
		{ 65417, 16 },
		{ 65418, 16 },
		{ 65419, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 26, 5 },
		{ 247, 8 },
		{ 1015, 10 },
		{ 4086, 12 },
		{ 32706, 15 },
		{ 65420, 16 },
		{ 65421, 16 },
		{ 65422, 16 },
		{ 65423, 16 },
		{ 65424, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 27, 5 },
		{ 248, 8 },
		{ 1016, 10 },
		{ 4087, 12 },
		{ 65425, 16 },
		{ 65426, 16 },
		{ 65427, 16 },
		{ 65428, 16 },
		{ 65429, 16 },
		{ 65430, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 58, 6 },
		{ 502, 9 },
		{ 65431, 16 },
		{ 65432, 16 },
		{ 65433, 16 },
		{ 65434, 16 },
		{ 65435, 16 },
		{ 65436, 16 },
		{ 65437, 16 },
		{ 65438, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 59, 6 },
		{ 1017, 10 },
		{ 65439, 16 },
		{ 65440, 16 },
		{ 65441, 16 },
		{ 65442, 16 },
		{ 65443, 16 },
		{ 65444, 16 },
		{ 65445, 16 },
		{ 65446, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 121, 7 },
		{ 2039, 11 },
		{ 65447, 16 },
		{ 65448, 16 },
		{ 65449, 16 },
		{ 65450, 16 },
		{ 65451, 16 },
		{ 65452, 16 },
		{ 65453, 16 },
		{ 65454, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 122, 7 },
		{ 2040, 11 },
		{ 65455, 16 },
		{ 65456, 16 },
		{ 65457, 16 },
		{ 65458, 16 },
		{ 65459, 16 },
		{ 65460, 16 },
		{ 65461, 16 },
		{ 65462, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 249, 8 },
		{ 65463, 16 },
		{ 65464, 16 },
		{ 65465, 16 },
		{ 65466, 16 },
		{ 65467, 16 },
		{ 65468, 16 },
		{ 65469, 16 },
		{ 65470, 16 },
		{ 65471, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 503, 9 },
		{ 65472, 16 },
		{ 65473, 16 },
		{ 65474, 16 },
		{ 65475, 16 },
		{ 65476, 16 },
		{ 65477, 16 },
		{ 65478, 16 },
		{ 65479, 16 },
		{ 65480, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 504, 9 },
		{ 65481, 16 },
		{ 65482, 16 },
		{ 65483, 16 },
		{ 65484, 16 },
		{ 65485, 16 },
		{ 65486, 16 },
		{ 65487, 16 },
		{ 65488, 16 },
		{ 65489, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 505, 9 },
		{ 65490, 16 },
		{ 65491, 16 },
		{ 65492, 16 },
		{ 65493, 16 },
		{ 65494, 16 },
		{ 65495, 16 },
		{ 65496, 16 },
		{ 65497, 16 },
		{ 65498, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 506, 9 },
		{ 65499, 16 },
		{ 65500, 16 },
		{ 65501, 16 },
		{ 65502, 16 },
		{ 65503, 16 },
		{ 65504, 16 },
		{ 65505, 16 },
		{ 65506, 16 },
		{ 65507, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 2041, 11 },
		{ 65508, 16 },
		{ 65509, 16 },
		{ 65510, 16 },
		{ 65511, 16 },
		{ 65512, 16 },
		{ 65513, 16 },
		{ 65514, 16 },
		{ 65515, 16 },
		{ 65516, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 16352, 14 },
		{ 65517, 16 },
		{ 65518, 16 },
		{ 65519, 16 },
		{ 65520, 16 },
		{ 65521, 16 },
		{ 65522, 16 },
		{ 65523, 16 },
		{ 65524, 16 },
		{ 65525, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 1018, 10 },
		{ 32707, 15 },
		{ 65526, 16 },
		{ 65527, 16 },
		{ 65528, 16 },
		{ 65529, 16 },
		{ 65530, 16 },
		{ 65531, 16 },
		{ 65532, 16 },
		{ 65533, 16 },
		{ 65534, 16 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 },
		{ 0, 0 }
	};

	public static int[] YQT = new int[64]
	{
		16, 11, 10, 16, 24, 40, 51, 61, 12, 12,
		14, 19, 26, 58, 60, 55, 14, 13, 16, 24,
		40, 57, 69, 56, 14, 17, 22, 29, 51, 87,
		80, 62, 18, 22, 37, 56, 68, 109, 103, 77,
		24, 35, 55, 64, 81, 104, 113, 92, 49, 64,
		78, 87, 103, 121, 120, 101, 72, 92, 95, 98,
		112, 100, 103, 99
	};

	public static int[] UVQT = new int[64]
	{
		17, 18, 24, 47, 99, 99, 99, 99, 18, 21,
		26, 66, 99, 99, 99, 99, 24, 26, 56, 99,
		99, 99, 99, 99, 47, 66, 99, 99, 99, 99,
		99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
		99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
		99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
		99, 99, 99, 99
	};

	public static float[] aasf = new float[8] { 2.828427f, 3.9231412f, 3.6955183f, 3.3258781f, 2.828427f, 2.222281f, 1.5307337f, 0.7803613f };

	public static byte[] head0 = new byte[25]
	{
		255, 216, 255, 224, 0, 16, 74, 70, 73, 70,
		0, 1, 1, 0, 0, 1, 0, 1, 0, 0,
		255, 219, 0, 132, 0
	};

	public static byte[] head2 = new byte[14]
	{
		255, 218, 0, 12, 3, 1, 0, 2, 17, 3,
		17, 0, 63, 0
	};

	public unsafe static void stbi__start_write_callbacks(stbi__write_context s, WriteCallback c, void* context)
	{
		s.func = c;
		s.context = context;
	}

	public unsafe static void stbiw__writefv(stbi__write_context s, string fmt, params object[] v)
	{
		int vindex = 0;
		for (int i = 0; i < fmt.Length; i++)
		{
			switch (fmt[i])
			{
			case '1':
			{
				byte x3 = (byte)((int)v[vindex++] & 0xFF);
				s.func(s.context, &x3, 1);
				break;
			}
			case '2':
			{
				int x2 = (int)v[vindex++];
				byte* b2 = stackalloc byte[2];
				*b2 = (byte)(x2 & 0xFF);
				b2[1] = (byte)((x2 >> 8) & 0xFF);
				s.func(s.context, b2, 2);
				break;
			}
			case '4':
			{
				int x = (int)v[vindex++];
				byte* b = stackalloc byte[4];
				*b = (byte)(x & 0xFF);
				b[1] = (byte)((x >> 8) & 0xFF);
				b[2] = (byte)((x >> 16) & 0xFF);
				b[3] = (byte)((x >> 24) & 0xFF);
				s.func(s.context, b, 4);
				break;
			}
			}
		}
	}

	public static void stbiw__writef(stbi__write_context s, string fmt, params object[] v)
	{
		StbImageWrite.stbiw__writefv(s, fmt, v);
	}

	public unsafe static int stbiw__outfile(stbi__write_context s, int rgb_dir, int vdir, int x, int y, int comp, int expand_mono, void* data, int alpha, int pad, string fmt, params object[] v)
	{
		if (y < 0 || x < 0)
		{
			return 0;
		}
		StbImageWrite.stbiw__writefv(s, fmt, v);
		StbImageWrite.stbiw__write_pixels(s, rgb_dir, vdir, x, y, comp, data, alpha, pad, expand_mono);
		return 1;
	}

	public unsafe static int stbi_write_bmp_to_func(WriteCallback func, void* context, int x, int y, int comp, void* data)
	{
		stbi__write_context s = new stbi__write_context();
		StbImageWrite.stbi__start_write_callbacks(s, func, context);
		return StbImageWrite.stbi_write_bmp_core(s, x, y, comp, data);
	}

	public unsafe static int stbi_write_tga_to_func(WriteCallback func, void* context, int x, int y, int comp, void* data)
	{
		stbi__write_context s = new stbi__write_context();
		StbImageWrite.stbi__start_write_callbacks(s, func, context);
		return StbImageWrite.stbi_write_tga_core(s, x, y, comp, data);
	}

	public unsafe static int stbi_write_hdr_to_func(WriteCallback func, void* context, int x, int y, int comp, float* data)
	{
		stbi__write_context s = new stbi__write_context();
		StbImageWrite.stbi__start_write_callbacks(s, func, context);
		return StbImageWrite.stbi_write_hdr_core(s, x, y, comp, data);
	}

	public unsafe static int stbi_write_png_to_func(WriteCallback func, void* context, int x, int y, int comp, void* data, int stride_bytes)
	{
		int len = default(int);
		byte* png = StbImageWrite.stbi_write_png_to_mem((byte*)data, stride_bytes, x, y, comp, &len);
		if (png == null)
		{
			return 0;
		}
		func(context, png, len);
		CRuntime.free(png);
		return 1;
	}

	public unsafe static int stbi_write_jpg_to_func(WriteCallback func, void* context, int x, int y, int comp, void* data, int quality)
	{
		stbi__write_context s = new stbi__write_context();
		StbImageWrite.stbi__start_write_callbacks(s, func, context);
		return StbImageWrite.stbi_write_jpg_core(s, x, y, comp, data, quality);
	}

	public unsafe static int stbi_write_hdr_core(stbi__write_context s, int x, int y, int comp, float* data)
	{
		if (y <= 0 || x <= 0 || data == null)
		{
			return 0;
		}
		byte* scratch = (byte*)CRuntime.malloc((ulong)(x * 4));
		string header = "#?RADIANCE\n# Written by stb_image_write.h\nFORMAT=32-bit_rle_rgbe\n";
		byte[] bytes = Encoding.UTF8.GetBytes(header);
		fixed (byte* ptr = bytes)
		{
			s.func(s.context, ptr, bytes.Length);
		}
		string str = $"EXPOSURE=          1.0000000000000\n\n-Y {y} +X {x}\n";
		bytes = Encoding.UTF8.GetBytes(str);
		fixed (byte* ptr2 = bytes)
		{
			s.func(s.context, ptr2, bytes.Length);
		}
		for (int i = 0; i < y; i++)
		{
			StbImageWrite.stbiw__write_hdr_scanline(s, x, comp, scratch, data + comp * i * x);
		}
		CRuntime.free(scratch);
		return 1;
	}

	public static void stbi_flip_vertically_on_write(int flag)
	{
		StbImageWrite.stbi__flip_vertically_on_write = flag;
	}

	public unsafe static void stbiw__putc(stbi__write_context s, byte c)
	{
		s.func(s.context, &c, 1);
	}

	public unsafe static void stbiw__write3(stbi__write_context s, byte a, byte b, byte c)
	{
		byte* arr = stackalloc byte[3];
		*arr = a;
		arr[1] = b;
		arr[2] = c;
		s.func(s.context, arr, 3);
	}

	public unsafe static void stbiw__write_pixel(stbi__write_context s, int rgb_dir, int comp, int write_alpha, int expand_mono, byte* d)
	{
		byte* bg = stackalloc byte[3];
		*bg = byte.MaxValue;
		bg[1] = 0;
		bg[2] = byte.MaxValue;
		byte* px = stackalloc byte[3];
		int k = 0;
		if (write_alpha < 0)
		{
			s.func(s.context, d + (comp - 1), 1);
		}
		if ((uint)(comp - 1) > 1u)
		{
			if ((uint)(comp - 3) <= 1u)
			{
				if (comp == 4 && write_alpha == 0)
				{
					for (k = 0; k < 3; k++)
					{
						px[k] = (byte)(bg[k] + (d[k] - bg[k]) * d[3] / 255);
					}
					StbImageWrite.stbiw__write3(s, px[1 - rgb_dir], px[1], px[1 + rgb_dir]);
				}
				else
				{
					StbImageWrite.stbiw__write3(s, d[1 - rgb_dir], d[1], d[1 + rgb_dir]);
				}
			}
		}
		else if (expand_mono != 0)
		{
			StbImageWrite.stbiw__write3(s, *d, *d, *d);
		}
		else
		{
			s.func(s.context, d, 1);
		}
		if (write_alpha > 0)
		{
			s.func(s.context, d + (comp - 1), 1);
		}
	}

	public unsafe static void stbiw__write_pixels(stbi__write_context s, int rgb_dir, int vdir, int x, int y, int comp, void* data, int write_alpha, int scanline_pad, int expand_mono)
	{
		uint zero = 0u;
		int i = 0;
		int j = 0;
		int j_end = 0;
		if (y <= 0)
		{
			return;
		}
		if (StbImageWrite.stbi__flip_vertically_on_write != 0)
		{
			vdir *= -1;
		}
		if (vdir < 0)
		{
			j_end = -1;
			j = y - 1;
		}
		else
		{
			j_end = y;
			j = 0;
		}
		for (; j != j_end; j += vdir)
		{
			for (i = 0; i < x; i++)
			{
				byte* d = (byte*)data + (j * x + i) * comp;
				StbImageWrite.stbiw__write_pixel(s, rgb_dir, comp, write_alpha, expand_mono, d);
			}
			s.func(s.context, &zero, scanline_pad);
		}
	}

	public unsafe static int stbi_write_bmp_core(stbi__write_context s, int x, int y, int comp, void* data)
	{
		int pad = (-x * 3) & 3;
		return StbImageWrite.stbiw__outfile(s, -1, -1, x, y, comp, 1, data, 0, pad, "11 4 22 44 44 22 444444", 66, 77, 54 + (x * 3 + pad) * y, 0, 0, 54, 40, x, y, 1, 24, 0, 0, 0, 0, 0, 0);
	}

	public unsafe static int stbi_write_tga_core(stbi__write_context s, int x, int y, int comp, void* data)
	{
		int has_alpha = ((comp == 2 || comp == 4) ? 1 : 0);
		int colorbytes = ((has_alpha != 0) ? (comp - 1) : comp);
		int format = ((colorbytes < 2) ? 3 : 2);
		if (y < 0 || x < 0)
		{
			return 0;
		}
		if (StbImageWrite.stbi_write_tga_with_rle == 0)
		{
			return StbImageWrite.stbiw__outfile(s, -1, -1, x, y, comp, 0, data, has_alpha, 0, "111 221 2222 11", 0, 0, format, 0, 0, 0, 0, 0, x, y, (colorbytes + has_alpha) * 8, has_alpha * 8);
		}
		int i = 0;
		int j = 0;
		int k = 0;
		int jend = 0;
		int jdir = 0;
		StbImageWrite.stbiw__writef(s, "111 221 2222 11", 0, 0, format + 8, 0, 0, 0, 0, 0, x, y, (colorbytes + has_alpha) * 8, has_alpha * 8);
		if (StbImageWrite.stbi__flip_vertically_on_write != 0)
		{
			j = 0;
			jend = y;
			jdir = 1;
		}
		else
		{
			j = y - 1;
			jend = -1;
			jdir = -1;
		}
		for (; j != jend; j += jdir)
		{
			byte* row = (byte*)data + j * x * comp;
			int len = 0;
			for (i = 0; i < x; i += len)
			{
				byte* begin = row + i * comp;
				int diff = 1;
				len = 1;
				if (i < x - 1)
				{
					len++;
					diff = CRuntime.memcmp(begin, row + (i + 1) * comp, (ulong)comp);
					if (diff != 0)
					{
						byte* prev = begin;
						for (k = i + 2; k < x; k++)
						{
							if (len >= 128)
							{
								break;
							}
							if (CRuntime.memcmp(prev, row + k * comp, (ulong)comp) != 0)
							{
								prev += comp;
								len++;
								continue;
							}
							len--;
							break;
						}
					}
					else
					{
						for (k = i + 2; k < x; k++)
						{
							if (len >= 128)
							{
								break;
							}
							if (CRuntime.memcmp(begin, row + k * comp, (ulong)comp) != 0)
							{
								break;
							}
							len++;
						}
					}
				}
				if (diff != 0)
				{
					byte header = (byte)((len - 1) & 0xFF);
					s.func(s.context, &header, 1);
					for (k = 0; k < len; k++)
					{
						StbImageWrite.stbiw__write_pixel(s, -1, comp, has_alpha, 0, begin + k * comp);
					}
				}
				else
				{
					byte header2 = (byte)((len - 129) & 0xFF);
					s.func(s.context, &header2, 1);
					StbImageWrite.stbiw__write_pixel(s, -1, comp, has_alpha, 0, begin);
				}
			}
		}
		return 1;
	}

	public unsafe static void stbiw__linear_to_rgbe(byte* rgbe, float* linear)
	{
		int exponent = 0;
		float maxcomp = ((*linear > ((linear[1] > linear[2]) ? linear[1] : linear[2])) ? (*linear) : ((linear[1] > linear[2]) ? linear[1] : linear[2]));
		if (maxcomp < 1E-32f)
		{
			*rgbe = (rgbe[1] = (rgbe[2] = (rgbe[3] = 0)));
			return;
		}
		float normalize = (float)CRuntime.frexp(maxcomp, &exponent) * 256f / maxcomp;
		*rgbe = (byte)(*linear * normalize);
		rgbe[1] = (byte)(linear[1] * normalize);
		rgbe[2] = (byte)(linear[2] * normalize);
		rgbe[3] = (byte)(exponent + 128);
	}

	public unsafe static void stbiw__write_run_data(stbi__write_context s, int length, byte databyte)
	{
		byte lengthbyte = (byte)((length + 128) & 0xFF);
		s.func(s.context, &lengthbyte, 1);
		s.func(s.context, &databyte, 1);
	}

	public unsafe static void stbiw__write_dump_data(stbi__write_context s, int length, byte* data)
	{
		byte lengthbyte = (byte)(length & 0xFF);
		s.func(s.context, &lengthbyte, 1);
		s.func(s.context, data, length);
	}

	public unsafe static void stbiw__write_hdr_scanline(stbi__write_context s, int width, int ncomp, byte* scratch, float* scanline)
	{
		byte* scanlineheader = stackalloc byte[4];
		*scanlineheader = 2;
		scanlineheader[1] = 2;
		scanlineheader[2] = 0;
		scanlineheader[3] = 0;
		byte* rgbe = stackalloc byte[4];
		float* linear = stackalloc float[3];
		int x = 0;
		scanlineheader[2] = (byte)((width & 0xFF00) >> 8);
		scanlineheader[3] = (byte)(width & 0xFF);
		if (width < 8 || width >= 32768)
		{
			for (x = 0; x < width; x++)
			{
				if ((uint)(ncomp - 3) <= 1u)
				{
					linear[2] = scanline[x * ncomp + 2];
					linear[1] = scanline[x * ncomp + 1];
					*linear = scanline[x * ncomp];
				}
				else
				{
					*linear = (linear[1] = (linear[2] = scanline[x * ncomp]));
				}
				StbImageWrite.stbiw__linear_to_rgbe(rgbe, linear);
				s.func(s.context, rgbe, 4);
			}
			return;
		}
		int c = 0;
		int r = 0;
		for (x = 0; x < width; x++)
		{
			if ((uint)(ncomp - 3) <= 1u)
			{
				linear[2] = scanline[x * ncomp + 2];
				linear[1] = scanline[x * ncomp + 1];
				*linear = scanline[x * ncomp];
			}
			else
			{
				*linear = (linear[1] = (linear[2] = scanline[x * ncomp]));
			}
			StbImageWrite.stbiw__linear_to_rgbe(rgbe, linear);
			scratch[x] = *rgbe;
			scratch[x + width] = rgbe[1];
			scratch[x + width * 2] = rgbe[2];
			scratch[x + width * 3] = rgbe[3];
		}
		s.func(s.context, scanlineheader, 4);
		for (c = 0; c < 4; c++)
		{
			byte* comp = scratch + width * c;
			x = 0;
			while (x < width)
			{
				for (r = x; r + 2 < width && (comp[r] != comp[r + 1] || comp[r] != comp[r + 2]); r++)
				{
				}
				if (r + 2 >= width)
				{
					r = width;
				}
				int len;
				for (; x < r; x += len)
				{
					len = r - x;
					if (len > 128)
					{
						len = 128;
					}
					StbImageWrite.stbiw__write_dump_data(s, len, comp + x);
				}
				if (r + 2 >= width)
				{
					continue;
				}
				for (; r < width && comp[r] == comp[x]; r++)
				{
				}
				int len2;
				for (; x < r; x += len2)
				{
					len2 = r - x;
					if (len2 > 127)
					{
						len2 = 127;
					}
					StbImageWrite.stbiw__write_run_data(s, len2, comp[x]);
				}
			}
		}
	}

	public unsafe static void* stbiw__sbgrowf(void** arr, int increment, int itemsize)
	{
		int m = ((*arr != null) ? (2 * *((int*)(*arr) - 2) + increment) : (increment + 1));
		void* p = CRuntime.realloc((*arr != null) ? ((byte*)(*arr) - (nint)2 * (nint)4) : null, (ulong)(itemsize * m + 8));
		if (p != null)
		{
			if (*arr == null)
			{
				((int*)p)[1] = 0;
			}
			*arr = (byte*)p + (nint)2 * (nint)4;
			*((int*)(*arr) - 2) = m;
		}
		return *arr;
	}

	public unsafe static byte* stbiw__zlib_flushf(byte* data, uint* bitbuffer, int* bitcount)
	{
		while (*bitcount >= 8)
		{
			if (data == null || ((int*)(data - (nint)2 * (nint)4))[1] + 1 >= *((int*)data - 2))
			{
				StbImageWrite.stbiw__sbgrowf((void**)(&data), 1, 1);
			}
			data[((int*)(data - (nint)2 * (nint)4))[1]++] = (byte)(*bitbuffer & 0xFF);
			*bitbuffer >>>= 8;
			*bitcount -= 8;
		}
		return data;
	}

	public static int stbiw__zlib_bitrev(int code, int codebits)
	{
		int res = 0;
		while (codebits-- != 0)
		{
			res = (res << 1) | (code & 1);
			code >>= 1;
		}
		return res;
	}

	public unsafe static uint stbiw__zlib_countm(byte* a, byte* b, int limit)
	{
		int i = 0;
		for (i = 0; i < limit && i < 258 && a[i] == b[i]; i++)
		{
		}
		return (uint)i;
	}

	public unsafe static uint stbiw__zhash(byte* data)
	{
		int num = *data + (data[1] << 8) + (data[2] << 16);
		int num2 = num ^ (num << 3);
		int num3 = num2 + (num2 >>> 5);
		int num4 = num3 ^ (num3 << 4);
		int num5 = num4 + (num4 >>> 17);
		int num6 = num5 ^ (num5 << 25);
		return (uint)(num6 + (num6 >>> 6));
	}

	public unsafe static byte* stbi_zlib_compress(byte* data, int data_len, int* out_len, int quality)
	{
		uint bitbuf = 0u;
		int i = 0;
		int j = 0;
		int bitcount = 0;
		byte* _out_ = null;
		byte*** hash_table = (byte***)CRuntime.malloc((ulong)(16384 * sizeof(char**)));
		if (hash_table == null)
		{
			return null;
		}
		if (quality < 5)
		{
			quality = 5;
		}
		if (_out_ == null || ((int*)(_out_ - (nint)2 * (nint)4))[1] + 1 >= *((int*)_out_ - 2))
		{
			StbImageWrite.stbiw__sbgrowf((void**)(&_out_), 1, 1);
		}
		_out_[((int*)(_out_ - (nint)2 * (nint)4))[1]++] = 120;
		if (_out_ == null || ((int*)(_out_ - (nint)2 * (nint)4))[1] + 1 >= *((int*)_out_ - 2))
		{
			StbImageWrite.stbiw__sbgrowf((void**)(&_out_), 1, 1);
		}
		_out_[((int*)(_out_ - (nint)2 * (nint)4))[1]++] = 94;
		bitbuf |= (uint)(1 << bitcount);
		bitcount++;
		_out_ = StbImageWrite.stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
		bitbuf |= (uint)(1 << bitcount);
		bitcount += 2;
		_out_ = StbImageWrite.stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
		for (i = 0; i < 16384; i++)
		{
			hash_table[i] = null;
		}
		i = 0;
		while (i < data_len - 3)
		{
			int h = (int)(StbImageWrite.stbiw__zhash(data + i) & 0x3FFF);
			int best = 3;
			byte* bestloc = null;
			byte** hlist = hash_table[h];
			int n = ((hlist != null) ? ((int*)((byte*)hlist - (nint)2 * (nint)4))[1] : 0);
			for (j = 0; j < n; j++)
			{
				if (hlist[j] - data > i - 32768)
				{
					int d = (int)StbImageWrite.stbiw__zlib_countm(hlist[j], data + i, data_len - i);
					if (d >= best)
					{
						best = d;
						bestloc = hlist[j];
					}
				}
			}
			if (hash_table[h] != null && ((int*)((byte*)hash_table[h] - (nint)2 * (nint)4))[1] == 2 * quality)
			{
				CRuntime.memmove(hash_table[h], hash_table[h] + quality, (ulong)(sizeof(byte*) * quality));
				((int*)((byte*)hash_table[h] - (nint)2 * (nint)4))[1] = quality;
			}
			if (hash_table[h] == null || ((int*)((byte*)hash_table[h] - (nint)2 * (nint)4))[1] + 1 >= *((int*)hash_table[h] - 2))
			{
				StbImageWrite.stbiw__sbgrowf((void**)(hash_table + h), 1, sizeof(byte*));
			}
			hash_table[h][((int*)((byte*)hash_table[h] - (nint)2 * (nint)4))[1]++] = data + i;
			if (bestloc != null)
			{
				h = (int)(StbImageWrite.stbiw__zhash(data + i + 1) & 0x3FFF);
				hlist = hash_table[h];
				n = ((hlist != null) ? ((int*)((byte*)hlist - (nint)2 * (nint)4))[1] : 0);
				for (j = 0; j < n; j++)
				{
					if (hlist[j] - data > i - 32767 && (int)StbImageWrite.stbiw__zlib_countm(hlist[j], data + i + 1, data_len - i - 1) > best)
					{
						bestloc = null;
						break;
					}
				}
			}
			if (bestloc != null)
			{
				int d2 = (int)(data + i - bestloc);
				for (j = 0; best > StbImageWrite.lengthc[j + 1] - 1; j++)
				{
				}
				if (j + 257 <= 143)
				{
					bitbuf |= (uint)(StbImageWrite.stbiw__zlib_bitrev(48 + (j + 257), 8) << bitcount);
					bitcount += 8;
					_out_ = StbImageWrite.stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
				}
				else if (j + 257 <= 255)
				{
					bitbuf |= (uint)(StbImageWrite.stbiw__zlib_bitrev(400 + (j + 257) - 144, 9) << bitcount);
					bitcount += 9;
					_out_ = StbImageWrite.stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
				}
				else if (j + 257 <= 279)
				{
					bitbuf |= (uint)(StbImageWrite.stbiw__zlib_bitrev(j + 257 - 256, 7) << bitcount);
					bitcount += 7;
					_out_ = StbImageWrite.stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
				}
				else
				{
					bitbuf |= (uint)(StbImageWrite.stbiw__zlib_bitrev(192 + (j + 257) - 280, 8) << bitcount);
					bitcount += 8;
					_out_ = StbImageWrite.stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
				}
				if (StbImageWrite.lengtheb[j] != 0)
				{
					bitbuf |= (uint)(best - StbImageWrite.lengthc[j] << bitcount);
					bitcount += StbImageWrite.lengtheb[j];
					_out_ = StbImageWrite.stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
				}
				for (j = 0; d2 > StbImageWrite.distc[j + 1] - 1; j++)
				{
				}
				bitbuf |= (uint)(StbImageWrite.stbiw__zlib_bitrev(j, 5) << bitcount);
				bitcount += 5;
				_out_ = StbImageWrite.stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
				if (StbImageWrite.disteb[j] != 0)
				{
					bitbuf |= (uint)(d2 - StbImageWrite.distc[j] << bitcount);
					bitcount += StbImageWrite.disteb[j];
					_out_ = StbImageWrite.stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
				}
				i += best;
			}
			else
			{
				if (data[i] <= 143)
				{
					bitbuf |= (uint)(StbImageWrite.stbiw__zlib_bitrev(48 + data[i], 8) << bitcount);
					bitcount += 8;
					_out_ = StbImageWrite.stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
				}
				else
				{
					bitbuf |= (uint)(StbImageWrite.stbiw__zlib_bitrev(400 + data[i] - 144, 9) << bitcount);
					bitcount += 9;
					_out_ = StbImageWrite.stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
				}
				i++;
			}
		}
		for (; i < data_len; i++)
		{
			if (data[i] <= 143)
			{
				bitbuf |= (uint)(StbImageWrite.stbiw__zlib_bitrev(48 + data[i], 8) << bitcount);
				bitcount += 8;
				_out_ = StbImageWrite.stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
			}
			else
			{
				bitbuf |= (uint)(StbImageWrite.stbiw__zlib_bitrev(400 + data[i] - 144, 9) << bitcount);
				bitcount += 9;
				_out_ = StbImageWrite.stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
			}
		}
		bitbuf |= (uint)(StbImageWrite.stbiw__zlib_bitrev(0, 7) << bitcount);
		bitcount += 7;
		_out_ = StbImageWrite.stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
		while (bitcount != 0)
		{
			bitbuf |= (uint)(0 << bitcount);
			bitcount++;
			_out_ = StbImageWrite.stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
		}
		for (i = 0; i < 16384; i++)
		{
			if (hash_table[i] != null)
			{
				CRuntime.free((byte*)hash_table[i] - (nint)2 * (nint)4);
			}
		}
		CRuntime.free(hash_table);
		uint s1 = 1u;
		uint s2 = 0u;
		int blocklen = data_len % 5552;
		j = 0;
		while (j < data_len)
		{
			for (i = 0; i < blocklen; i++)
			{
				s1 += data[j + i];
				s2 += s1;
			}
			s1 %= 65521;
			s2 %= 65521;
			j += blocklen;
			blocklen = 5552;
		}
		if (_out_ == null || ((int*)(_out_ - (nint)2 * (nint)4))[1] + 1 >= *((int*)_out_ - 2))
		{
			StbImageWrite.stbiw__sbgrowf((void**)(&_out_), 1, 1);
		}
		_out_[((int*)(_out_ - (nint)2 * (nint)4))[1]++] = (byte)((s2 >> 8) & 0xFF);
		if (_out_ == null || ((int*)(_out_ - (nint)2 * (nint)4))[1] + 1 >= *((int*)_out_ - 2))
		{
			StbImageWrite.stbiw__sbgrowf((void**)(&_out_), 1, 1);
		}
		_out_[((int*)(_out_ - (nint)2 * (nint)4))[1]++] = (byte)(s2 & 0xFF);
		if (_out_ == null || ((int*)(_out_ - (nint)2 * (nint)4))[1] + 1 >= *((int*)_out_ - 2))
		{
			StbImageWrite.stbiw__sbgrowf((void**)(&_out_), 1, 1);
		}
		_out_[((int*)(_out_ - (nint)2 * (nint)4))[1]++] = (byte)((s1 >> 8) & 0xFF);
		if (_out_ == null || ((int*)(_out_ - (nint)2 * (nint)4))[1] + 1 >= *((int*)_out_ - 2))
		{
			StbImageWrite.stbiw__sbgrowf((void**)(&_out_), 1, 1);
		}
		_out_[((int*)(_out_ - (nint)2 * (nint)4))[1]++] = (byte)(s1 & 0xFF);
		*out_len = ((int*)(_out_ - (nint)2 * (nint)4))[1];
		CRuntime.memmove(_out_ - (nint)2 * (nint)4, _out_, (ulong)(*out_len));
		return _out_ - (nint)2 * (nint)4;
	}

	public unsafe static uint stbiw__crc32(byte* buffer, int len)
	{
		uint crc = uint.MaxValue;
		int i = 0;
		for (i = 0; i < len; i++)
		{
			crc = (crc >> 8) ^ StbImageWrite.crc_table[buffer[i] ^ (crc & 0xFF)];
		}
		return ~crc;
	}

	public unsafe static void stbiw__wpcrc(byte** data, int len)
	{
		uint crc = StbImageWrite.stbiw__crc32(*data - len - 4, len + 4);
		*(*data) = (byte)((crc >> 24) & 0xFF);
		(*data)[1] = (byte)((crc >> 16) & 0xFF);
		(*data)[2] = (byte)((crc >> 8) & 0xFF);
		(*data)[3] = (byte)(crc & 0xFF);
		*data += 4;
	}

	public static byte stbiw__paeth(int a, int b, int c)
	{
		int num = a + b - c;
		int pa = CRuntime.abs(num - a);
		int pb = CRuntime.abs(num - b);
		int pc = CRuntime.abs(num - c);
		if (pa <= pb && pa <= pc)
		{
			return (byte)(a & 0xFF);
		}
		if (pb <= pc)
		{
			return (byte)(b & 0xFF);
		}
		return (byte)(c & 0xFF);
	}

	public unsafe static void stbiw__encode_png_line(byte* pixels, int stride_bytes, int width, int height, int y, int n, int filter_type, sbyte* line_buffer)
	{
		int* mapping = stackalloc int[5];
		*mapping = 0;
		mapping[1] = 1;
		mapping[2] = 2;
		mapping[3] = 3;
		mapping[4] = 4;
		int* firstmap = stackalloc int[5];
		*firstmap = 0;
		firstmap[1] = 1;
		firstmap[2] = 0;
		firstmap[3] = 5;
		firstmap[4] = 6;
		int* num = ((y != 0) ? mapping : firstmap);
		int i = 0;
		int type = num[filter_type];
		byte* z = pixels + stride_bytes * ((StbImageWrite.stbi__flip_vertically_on_write != 0) ? (height - 1 - y) : y);
		int signed_stride = ((StbImageWrite.stbi__flip_vertically_on_write != 0) ? (-stride_bytes) : stride_bytes);
		if (type == 0)
		{
			CRuntime.memcpy(line_buffer, z, (ulong)(width * n));
			return;
		}
		for (i = 0; i < n; i++)
		{
			switch (type)
			{
			case 1:
				line_buffer[i] = (sbyte)z[i];
				break;
			case 2:
				line_buffer[i] = (sbyte)(z[i] - z[i - signed_stride]);
				break;
			case 3:
				line_buffer[i] = (sbyte)(z[i] - (z[i - signed_stride] >> 1));
				break;
			case 4:
				line_buffer[i] = (sbyte)(z[i] - StbImageWrite.stbiw__paeth(0, z[i - signed_stride], 0));
				break;
			case 5:
				line_buffer[i] = (sbyte)z[i];
				break;
			case 6:
				line_buffer[i] = (sbyte)z[i];
				break;
			}
		}
		switch (type)
		{
		case 1:
			for (i = n; i < width * n; i++)
			{
				line_buffer[i] = (sbyte)(z[i] - z[i - n]);
			}
			break;
		case 2:
			for (i = n; i < width * n; i++)
			{
				line_buffer[i] = (sbyte)(z[i] - z[i - signed_stride]);
			}
			break;
		case 3:
			for (i = n; i < width * n; i++)
			{
				line_buffer[i] = (sbyte)(z[i] - (z[i - n] + z[i - signed_stride] >> 1));
			}
			break;
		case 4:
			for (i = n; i < width * n; i++)
			{
				line_buffer[i] = (sbyte)(z[i] - StbImageWrite.stbiw__paeth(z[i - n], z[i - signed_stride], z[i - signed_stride - n]));
			}
			break;
		case 5:
			for (i = n; i < width * n; i++)
			{
				line_buffer[i] = (sbyte)(z[i] - (z[i - n] >> 1));
			}
			break;
		case 6:
			for (i = n; i < width * n; i++)
			{
				line_buffer[i] = (sbyte)(z[i] - StbImageWrite.stbiw__paeth(z[i - n], 0, 0));
			}
			break;
		}
	}

	public unsafe static byte* stbi_write_png_to_mem(byte* pixels, int stride_bytes, int x, int y, int n, int* out_len)
	{
		int force_filter = StbImageWrite.stbi_write_force_png_filter;
		int* ctype = stackalloc int[5];
		*ctype = -1;
		ctype[1] = 0;
		ctype[2] = 4;
		ctype[3] = 2;
		ctype[4] = 6;
		byte* sig = stackalloc byte[8];
		*sig = 137;
		sig[1] = 80;
		sig[2] = 78;
		sig[3] = 71;
		sig[4] = 13;
		sig[5] = 10;
		sig[6] = 26;
		sig[7] = 10;
		int j = 0;
		int zlen = 0;
		if (stride_bytes == 0)
		{
			stride_bytes = x * n;
		}
		if (force_filter >= 5)
		{
			force_filter = -1;
		}
		byte* filt = (byte*)CRuntime.malloc((ulong)((x * n + 1) * y));
		if (filt == null)
		{
			return null;
		}
		sbyte* line_buffer = (sbyte*)CRuntime.malloc((ulong)(x * n));
		if (line_buffer == null)
		{
			CRuntime.free(filt);
			return null;
		}
		for (j = 0; j < y; j++)
		{
			int filter_type = 0;
			if (force_filter > -1)
			{
				filter_type = force_filter;
				StbImageWrite.stbiw__encode_png_line(pixels, stride_bytes, x, y, j, n, force_filter, line_buffer);
			}
			else
			{
				int best_filter = 0;
				int best_filter_val = int.MaxValue;
				int est = 0;
				int i = 0;
				for (filter_type = 0; filter_type < 5; filter_type++)
				{
					StbImageWrite.stbiw__encode_png_line(pixels, stride_bytes, x, y, j, n, filter_type, line_buffer);
					est = 0;
					for (i = 0; i < x * n; i++)
					{
						est += CRuntime.abs(line_buffer[i]);
					}
					if (est < best_filter_val)
					{
						best_filter_val = est;
						best_filter = filter_type;
					}
				}
				if (filter_type != best_filter)
				{
					StbImageWrite.stbiw__encode_png_line(pixels, stride_bytes, x, y, j, n, best_filter, line_buffer);
					filter_type = best_filter;
				}
			}
			filt[j * (x * n + 1)] = (byte)filter_type;
			CRuntime.memmove(filt + j * (x * n + 1) + 1, line_buffer, (ulong)(x * n));
		}
		CRuntime.free(line_buffer);
		byte* zlib = StbImageWrite.stbi_zlib_compress(filt, y * (x * n + 1), &zlen, StbImageWrite.stbi_write_png_compression_level);
		CRuntime.free(filt);
		if (zlib == null)
		{
			return null;
		}
		byte* _out_ = (byte*)CRuntime.malloc((ulong)(45 + zlen + 12));
		if (_out_ == null)
		{
			return null;
		}
		*out_len = 45 + zlen + 12;
		byte* o = _out_;
		CRuntime.memmove(o, sig, 8uL);
		o += 8;
		*o = 0;
		o[1] = 0;
		o[2] = 0;
		o[3] = 13;
		o += 4;
		*o = (byte)("IHDR"[0] & 0xFF);
		o[1] = (byte)("IHDR"[1] & 0xFF);
		o[2] = (byte)("IHDR"[2] & 0xFF);
		o[3] = (byte)("IHDR"[3] & 0xFF);
		o += 4;
		*o = (byte)((x >> 24) & 0xFF);
		o[1] = (byte)((x >> 16) & 0xFF);
		o[2] = (byte)((x >> 8) & 0xFF);
		o[3] = (byte)(x & 0xFF);
		o += 4;
		*o = (byte)((y >> 24) & 0xFF);
		o[1] = (byte)((y >> 16) & 0xFF);
		o[2] = (byte)((y >> 8) & 0xFF);
		o[3] = (byte)(y & 0xFF);
		o += 4;
		*(o++) = 8;
		*(o++) = (byte)(ctype[n] & 0xFF);
		*(o++) = 0;
		*(o++) = 0;
		*(o++) = 0;
		StbImageWrite.stbiw__wpcrc(&o, 13);
		*o = (byte)((zlen >> 24) & 0xFF);
		o[1] = (byte)((zlen >> 16) & 0xFF);
		o[2] = (byte)((zlen >> 8) & 0xFF);
		o[3] = (byte)(zlen & 0xFF);
		o += 4;
		*o = (byte)("IDAT"[0] & 0xFF);
		o[1] = (byte)("IDAT"[1] & 0xFF);
		o[2] = (byte)("IDAT"[2] & 0xFF);
		o[3] = (byte)("IDAT"[3] & 0xFF);
		o += 4;
		CRuntime.memmove(o, zlib, (ulong)zlen);
		o += zlen;
		CRuntime.free(zlib);
		StbImageWrite.stbiw__wpcrc(&o, zlen);
		*o = 0;
		o[1] = 0;
		o[2] = 0;
		o[3] = 0;
		o += 4;
		*o = (byte)("IEND"[0] & 0xFF);
		o[1] = (byte)("IEND"[1] & 0xFF);
		o[2] = (byte)("IEND"[2] & 0xFF);
		o[3] = (byte)("IEND"[3] & 0xFF);
		o += 4;
		StbImageWrite.stbiw__wpcrc(&o, 0);
		return _out_;
	}

	public unsafe static void stbiw__jpg_writeBits(stbi__write_context s, int* bitBufP, int* bitCntP, ushort bs0, ushort bs1)
	{
		int bitBuf = *bitBufP;
		int bitCnt = *bitCntP;
		bitCnt += bs1;
		bitBuf |= bs0 << 24 - bitCnt;
		while (bitCnt >= 8)
		{
			byte c = (byte)((bitBuf >> 16) & 0xFF);
			StbImageWrite.stbiw__putc(s, c);
			if (c == byte.MaxValue)
			{
				StbImageWrite.stbiw__putc(s, 0);
			}
			bitBuf <<= 8;
			bitCnt -= 8;
		}
		*bitBufP = bitBuf;
		*bitCntP = bitCnt;
	}

	public unsafe static void stbiw__jpg_DCT(float* d0p, float* d1p, float* d2p, float* d3p, float* d4p, float* d5p, float* d6p, float* d7p)
	{
		float d0 = *d0p;
		float d1 = *d1p;
		float d2 = *d2p;
		float d3 = *d3p;
		float d4 = *d4p;
		float d5 = *d5p;
		float d6 = *d6p;
		float d7 = *d7p;
		float z1 = 0f;
		float z2 = 0f;
		float z3 = 0f;
		float z4 = 0f;
		float z5 = 0f;
		float z11 = 0f;
		float z13 = 0f;
		float num = d0 + d7;
		float tmp7 = d0 - d7;
		float tmp8 = d1 + d6;
		float tmp9 = d1 - d6;
		float tmp10 = d2 + d5;
		float tmp11 = d2 - d5;
		float tmp12 = d3 + d4;
		float tmp13 = d3 - d4;
		float tmp14 = num + tmp12;
		float tmp15 = num - tmp12;
		float tmp16 = tmp8 + tmp10;
		float tmp17 = tmp8 - tmp10;
		d0 = tmp14 + tmp16;
		d4 = tmp14 - tmp16;
		z1 = (tmp17 + tmp15) * 0.70710677f;
		d2 = tmp15 + z1;
		d6 = tmp15 - z1;
		tmp14 = tmp13 + tmp11;
		tmp16 = tmp11 + tmp9;
		tmp17 = tmp9 + tmp7;
		z5 = (tmp14 - tmp17) * 0.38268343f;
		z2 = tmp14 * 0.5411961f + z5;
		z4 = tmp17 * 1.306563f + z5;
		z3 = tmp16 * 0.70710677f;
		z11 = tmp7 + z3;
		z13 = tmp7 - z3;
		*d5p = z13 + z2;
		*d3p = z13 - z2;
		*d1p = z11 + z4;
		*d7p = z11 - z4;
		*d0p = d0;
		*d2p = d2;
		*d4p = d4;
		*d6p = d6;
	}

	public unsafe static void stbiw__jpg_calcBits(int val, ushort* bits)
	{
		int tmp1 = ((val < 0) ? (-val) : val);
		val = ((val < 0) ? (val - 1) : val);
		bits[1] = 1;
		while ((tmp1 >>= 1) != 0)
		{
			ushort* num = bits + 1;
			(*num)++;
		}
		*bits = (ushort)(val & ((1 << (int)bits[1]) - 1));
	}

	public unsafe static int stbiw__jpg_processDU(stbi__write_context s, int* bitBuf, int* bitCnt, float* CDU, float* fdtbl, int DC, ushort[,] HTDC, ushort[,] HTAC)
	{
		ushort* EOB = stackalloc ushort[2];
		*EOB = HTAC[0, 0];
		EOB[1] = HTAC[0, 1];
		ushort* M16zeroes = stackalloc ushort[2];
		*M16zeroes = HTAC[240, 0];
		M16zeroes[1] = HTAC[240, 1];
		int dataOff = 0;
		int i = 0;
		int diff = 0;
		int end0pos = 0;
		int* DU = stackalloc int[64];
		for (dataOff = 0; dataOff < 64; dataOff += 8)
		{
			StbImageWrite.stbiw__jpg_DCT(CDU + dataOff, CDU + (dataOff + 1), CDU + (dataOff + 2), CDU + (dataOff + 3), CDU + (dataOff + 4), CDU + (dataOff + 5), CDU + (dataOff + 6), CDU + (dataOff + 7));
		}
		for (dataOff = 0; dataOff < 8; dataOff++)
		{
			StbImageWrite.stbiw__jpg_DCT(CDU + dataOff, CDU + (dataOff + 8), CDU + (dataOff + 16), CDU + (dataOff + 24), CDU + (dataOff + 32), CDU + (dataOff + 40), CDU + (dataOff + 48), CDU + (dataOff + 56));
		}
		for (i = 0; i < 64; i++)
		{
			float v = CDU[i] * fdtbl[i];
			DU[(int)StbImageWrite.stbiw__jpg_ZigZag[i]] = (int)((v < 0f) ? (v - 0.5f) : (v + 0.5f));
		}
		diff = *DU - DC;
		if (diff == 0)
		{
			StbImageWrite.stbiw__jpg_writeBits(s, bitBuf, bitCnt, HTDC[0, 0], HTDC[0, 1]);
		}
		else
		{
			ushort* bits = stackalloc ushort[2];
			StbImageWrite.stbiw__jpg_calcBits(diff, bits);
			StbImageWrite.stbiw__jpg_writeBits(s, bitBuf, bitCnt, HTDC[bits[1], 0], HTDC[bits[1], 1]);
			StbImageWrite.stbiw__jpg_writeBits(s, bitBuf, bitCnt, *bits, bits[1]);
		}
		end0pos = 63;
		while (end0pos > 0 && DU[end0pos] == 0)
		{
			end0pos--;
		}
		if (end0pos == 0)
		{
			StbImageWrite.stbiw__jpg_writeBits(s, bitBuf, bitCnt, *EOB, EOB[1]);
			return *DU;
		}
		for (i = 1; i <= end0pos; i++)
		{
			int startpos = i;
			int nrzeroes = 0;
			ushort* bits2 = stackalloc ushort[2];
			for (; DU[i] == 0 && i <= end0pos; i++)
			{
			}
			nrzeroes = i - startpos;
			if (nrzeroes >= 16)
			{
				int lng = nrzeroes >> 4;
				int nrmarker = 0;
				for (nrmarker = 1; nrmarker <= lng; nrmarker++)
				{
					StbImageWrite.stbiw__jpg_writeBits(s, bitBuf, bitCnt, *M16zeroes, M16zeroes[1]);
				}
				nrzeroes &= 0xF;
			}
			StbImageWrite.stbiw__jpg_calcBits(DU[i], bits2);
			StbImageWrite.stbiw__jpg_writeBits(s, bitBuf, bitCnt, HTAC[(nrzeroes << 4) + bits2[1], 0], HTAC[(nrzeroes << 4) + bits2[1], 1]);
			StbImageWrite.stbiw__jpg_writeBits(s, bitBuf, bitCnt, *bits2, bits2[1]);
		}
		if (end0pos != 63)
		{
			StbImageWrite.stbiw__jpg_writeBits(s, bitBuf, bitCnt, *EOB, EOB[1]);
		}
		return *DU;
	}

	public unsafe static int stbi_write_jpg_core(stbi__write_context s, int width, int height, int comp, void* data, int quality)
	{
		int row = 0;
		int col = 0;
		int i = 0;
		int k = 0;
		float* fdtbl_Y = stackalloc float[64];
		float* fdtbl_UV = stackalloc float[64];
		byte* YTable = stackalloc byte[64];
		byte* UVTable = stackalloc byte[64];
		if (data == null || width == 0 || height == 0 || comp > 4 || comp < 1)
		{
			return 0;
		}
		quality = ((quality != 0) ? quality : 90);
		quality = ((quality < 1) ? 1 : ((quality > 100) ? 100 : quality));
		quality = ((quality < 50) ? (5000 / quality) : (200 - quality * 2));
		for (i = 0; i < 64; i++)
		{
			int uvti = 0;
			int yti = (StbImageWrite.YQT[i] * quality + 50) / 100;
			YTable[(int)StbImageWrite.stbiw__jpg_ZigZag[i]] = (byte)((yti < 1) ? 1u : ((yti > 255) ? 255u : ((uint)yti)));
			uvti = (StbImageWrite.UVQT[i] * quality + 50) / 100;
			UVTable[(int)StbImageWrite.stbiw__jpg_ZigZag[i]] = (byte)((uvti < 1) ? 1u : ((uvti > 255) ? 255u : ((uint)uvti)));
		}
		row = 0;
		k = 0;
		for (; row < 8; row++)
		{
			col = 0;
			while (col < 8)
			{
				fdtbl_Y[k] = 1f / ((float)(int)YTable[(int)StbImageWrite.stbiw__jpg_ZigZag[k]] * StbImageWrite.aasf[row] * StbImageWrite.aasf[col]);
				fdtbl_UV[k] = 1f / ((float)(int)UVTable[(int)StbImageWrite.stbiw__jpg_ZigZag[k]] * StbImageWrite.aasf[row] * StbImageWrite.aasf[col]);
				col++;
				k++;
			}
		}
		byte* head1 = stackalloc byte[24];
		*head1 = byte.MaxValue;
		head1[1] = 192;
		head1[2] = 0;
		head1[3] = 17;
		head1[4] = 8;
		head1[5] = (byte)(height >> 8);
		head1[6] = (byte)(height & 0xFF);
		head1[7] = (byte)(width >> 8);
		head1[8] = (byte)(width & 0xFF);
		head1[9] = 3;
		head1[10] = 1;
		head1[11] = 17;
		head1[12] = 0;
		head1[13] = 2;
		head1[14] = 17;
		head1[15] = 1;
		head1[16] = 3;
		head1[17] = 17;
		head1[18] = 1;
		head1[19] = byte.MaxValue;
		head1[20] = 196;
		head1[21] = 1;
		head1[22] = 162;
		head1[23] = 0;
		fixed (byte* h = StbImageWrite.head0)
		{
			s.func(s.context, h, StbImageWrite.head0.Length);
		}
		s.func(s.context, YTable, 64);
		StbImageWrite.stbiw__putc(s, 1);
		s.func(s.context, UVTable, 64);
		s.func(s.context, head1, 24);
		fixed (byte* d = &StbImageWrite.std_dc_luminance_nrcodes[1])
		{
			s.func(s.context, d, StbImageWrite.std_dc_chrominance_nrcodes.Length - 1);
		}
		fixed (byte* d2 = StbImageWrite.std_dc_luminance_values)
		{
			s.func(s.context, d2, StbImageWrite.std_dc_chrominance_values.Length);
		}
		StbImageWrite.stbiw__putc(s, 16);
		fixed (byte* a = &StbImageWrite.std_ac_luminance_nrcodes[1])
		{
			s.func(s.context, a, StbImageWrite.std_ac_luminance_nrcodes.Length - 1);
		}
		fixed (byte* d3 = StbImageWrite.std_ac_luminance_values)
		{
			s.func(s.context, d3, StbImageWrite.std_ac_chrominance_values.Length);
		}
		StbImageWrite.stbiw__putc(s, 1);
		fixed (byte* d4 = &StbImageWrite.std_dc_chrominance_nrcodes[1])
		{
			s.func(s.context, d4, StbImageWrite.std_dc_chrominance_nrcodes.Length - 1);
		}
		fixed (byte* d5 = StbImageWrite.std_dc_chrominance_values)
		{
			s.func(s.context, d5, StbImageWrite.std_dc_chrominance_values.Length);
		}
		StbImageWrite.stbiw__putc(s, 17);
		fixed (byte* a2 = &StbImageWrite.std_ac_chrominance_nrcodes[1])
		{
			s.func(s.context, a2, StbImageWrite.std_ac_chrominance_nrcodes.Length - 1);
		}
		fixed (byte* d6 = StbImageWrite.std_ac_chrominance_values)
		{
			s.func(s.context, d6, StbImageWrite.std_ac_chrominance_values.Length);
		}
		fixed (byte* h2 = StbImageWrite.head2)
		{
			s.func(s.context, h2, StbImageWrite.head2.Length);
		}
		ushort* fillBits = stackalloc ushort[2];
		*fillBits = 127;
		fillBits[1] = 7;
		int DCY = 0;
		int DCU = 0;
		int DCV = 0;
		int bitBuf = 0;
		int bitCnt = 0;
		int ofsG = ((comp > 2) ? 1 : 0);
		int ofsB = ((comp > 2) ? 2 : 0);
		int x = 0;
		int y = 0;
		int pos = 0;
		float* YDU = stackalloc float[64];
		float* UDU = stackalloc float[64];
		float* VDU = stackalloc float[64];
		for (y = 0; y < height; y += 8)
		{
			for (x = 0; x < width; x += 8)
			{
				row = y;
				pos = 0;
				for (; row < y + 8; row++)
				{
					int clamped_row = ((row < height) ? row : (height - 1));
					int base_p = ((StbImageWrite.stbi__flip_vertically_on_write != 0) ? (height - 1 - clamped_row) : clamped_row) * width * comp;
					col = x;
					while (col < x + 8)
					{
						float r = 0f;
						float g = 0f;
						float b = 0f;
						int p = base_p + ((col < width) ? col : (width - 1)) * comp;
						r = (int)((byte*)data)[p];
						g = (int)((byte*)data)[p + ofsG];
						b = (int)((byte*)data)[p + ofsB];
						YDU[pos] = 0.299f * r + 0.587f * g + 0.114f * b - 128f;
						UDU[pos] = -0.16874f * r - 0.33126f * g + 0.5f * b;
						VDU[pos] = 0.5f * r - 0.41869f * g - 0.08131f * b;
						col++;
						pos++;
					}
				}
				DCY = StbImageWrite.stbiw__jpg_processDU(s, &bitBuf, &bitCnt, YDU, fdtbl_Y, DCY, StbImageWrite.YDC_HT, StbImageWrite.YAC_HT);
				DCU = StbImageWrite.stbiw__jpg_processDU(s, &bitBuf, &bitCnt, UDU, fdtbl_UV, DCU, StbImageWrite.UVDC_HT, StbImageWrite.UVAC_HT);
				DCV = StbImageWrite.stbiw__jpg_processDU(s, &bitBuf, &bitCnt, VDU, fdtbl_UV, DCV, StbImageWrite.UVDC_HT, StbImageWrite.UVAC_HT);
			}
		}
		StbImageWrite.stbiw__jpg_writeBits(s, &bitBuf, &bitCnt, *fillBits, fillBits[1]);
		StbImageWrite.stbiw__putc(s, byte.MaxValue);
		StbImageWrite.stbiw__putc(s, 217);
		return 1;
	}
}
