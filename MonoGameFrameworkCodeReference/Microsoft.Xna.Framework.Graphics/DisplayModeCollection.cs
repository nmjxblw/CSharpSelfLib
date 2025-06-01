using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics;

public class DisplayModeCollection : IEnumerable<DisplayMode>, IEnumerable
{
	private readonly List<DisplayMode> _modes;

	public IEnumerable<DisplayMode> this[SurfaceFormat format]
	{
		get
		{
			List<DisplayMode> list = new List<DisplayMode>();
			foreach (DisplayMode mode in this._modes)
			{
				if (mode.Format == format)
				{
					list.Add(mode);
				}
			}
			return list;
		}
	}

	public IEnumerator<DisplayMode> GetEnumerator()
	{
		return this._modes.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this._modes.GetEnumerator();
	}

	internal DisplayModeCollection(List<DisplayMode> modes)
	{
		modes.Sort(delegate(DisplayMode a, DisplayMode b)
		{
			if (a == b)
			{
				return 0;
			}
			return (a.Format > b.Format || a.Width > b.Width || a.Height > b.Height) ? 1 : (-1);
		});
		this._modes = modes;
	}
}
