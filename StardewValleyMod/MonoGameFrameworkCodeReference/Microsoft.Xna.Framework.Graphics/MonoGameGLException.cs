using System;

namespace Microsoft.Xna.Framework.Graphics;

internal class MonoGameGLException : Exception
{
	public MonoGameGLException(string message)
		: base(message)
	{
	}
}
