using System;

namespace MonoGame.OpenGL;

[Flags]
internal enum ClearBufferMask
{
	DepthBufferBit = 0x100,
	StencilBufferBit = 0x400,
	ColorBufferBit = 0x4000
}
