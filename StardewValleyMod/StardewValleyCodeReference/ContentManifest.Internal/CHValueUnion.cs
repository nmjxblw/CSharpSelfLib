using System.Runtime.InteropServices;

namespace ContentManifest.Internal;

[StructLayout(LayoutKind.Explicit)]
internal struct CHValueUnion
{
	[FieldOffset(0)]
	public CHObject ValueObject;

	[FieldOffset(0)]
	public CHArray ValueArray;

	[FieldOffset(0)]
	public CHString ValueString;

	[FieldOffset(0)]
	public CHNumber ValueNumber;

	[FieldOffset(0)]
	public CHBoolean ValueBoolean;

	[FieldOffset(0)]
	public object ValueNull;
}
