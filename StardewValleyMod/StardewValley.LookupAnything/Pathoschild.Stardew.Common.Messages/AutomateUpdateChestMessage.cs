using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.Common.Messages;

internal class AutomateUpdateChestMessage
{
	public string? LocationName { get; set; }

	public Vector2 Tile { get; set; }
}
