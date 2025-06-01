using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Content;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct LzxConstants
{
	public enum BLOCKTYPE
	{
		INVALID,
		VERBATIM,
		ALIGNED,
		UNCOMPRESSED
	}

	public const ushort MIN_MATCH = 2;

	public const ushort MAX_MATCH = 257;

	public const ushort NUM_CHARS = 256;

	public const ushort PRETREE_NUM_ELEMENTS = 20;

	public const ushort ALIGNED_NUM_ELEMENTS = 8;

	public const ushort NUM_PRIMARY_LENGTHS = 7;

	public const ushort NUM_SECONDARY_LENGTHS = 249;

	public const ushort PRETREE_MAXSYMBOLS = 20;

	public const ushort PRETREE_TABLEBITS = 6;

	public const ushort MAINTREE_MAXSYMBOLS = 656;

	public const ushort MAINTREE_TABLEBITS = 12;

	public const ushort LENGTH_MAXSYMBOLS = 250;

	public const ushort LENGTH_TABLEBITS = 12;

	public const ushort ALIGNED_MAXSYMBOLS = 8;

	public const ushort ALIGNED_TABLEBITS = 7;

	public const ushort LENTABLE_SAFETY = 64;
}
