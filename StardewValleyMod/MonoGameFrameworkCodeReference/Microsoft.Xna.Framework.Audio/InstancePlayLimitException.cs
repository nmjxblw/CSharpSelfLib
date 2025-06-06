using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Audio;

/// <summary>
/// The exception thrown when the system attempts to play more SoundEffectInstances than allotted.
/// </summary>
/// <remarks>
/// Most platforms have a hard limit on how many sounds can be played simultaneously. This exception is thrown when that limit is exceeded.
/// </remarks>
[DataContract]
public sealed class InstancePlayLimitException : ExternalException
{
}
