namespace MonoGame.OpenGL;

internal class ColorFormat
{
	internal int R { get; private set; }

	internal int G { get; private set; }

	internal int B { get; private set; }

	internal int A { get; private set; }

	internal ColorFormat(int r, int g, int b, int a)
	{
		this.R = r;
		this.G = g;
		this.B = b;
		this.A = a;
	}
}
