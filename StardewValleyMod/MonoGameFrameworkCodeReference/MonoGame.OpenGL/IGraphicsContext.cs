using System;

namespace MonoGame.OpenGL;

internal interface IGraphicsContext : IDisposable
{
	int SwapInterval { get; set; }

	bool IsDisposed { get; }

	bool IsCurrent { get; }

	void MakeCurrent(IWindowInfo info);

	void SwapBuffers();
}
