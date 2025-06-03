using System;

namespace Microsoft.Xna.Framework.Graphics;

internal class PresentationEventArgs : EventArgs
{
	public PresentationParameters PresentationParameters { get; private set; }

	public PresentationEventArgs(PresentationParameters presentationParameters)
	{
		this.PresentationParameters = presentationParameters;
	}
}
