using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media;

public sealed class MediaSource
{
	private MediaSourceType _type;

	private string _name;

	public MediaSourceType MediaSourceType => this._type;

	public string Name => this._name;

	internal MediaSource(string name, MediaSourceType type)
	{
		this._name = name;
		this._type = type;
	}

	public static IList<MediaSource> GetAvailableMediaSources()
	{
		return new MediaSource[1]
		{
			new MediaSource("DummpMediaSource", MediaSourceType.LocalDevice)
		};
	}
}
