using System;

namespace Microsoft.Xna.Framework.Media;

public sealed class Playlist : IDisposable
{
	public TimeSpan Duration { get; internal set; }

	public string Name { get; internal set; }

	public void Dispose()
	{
	}
}
