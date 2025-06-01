using System;

namespace Microsoft.Xna.Framework.Graphics;

public sealed class ResourceCreatedEventArgs : EventArgs
{
	/// <summary>
	/// The newly created resource object.
	/// </summary>
	public object Resource { get; internal set; }
}
