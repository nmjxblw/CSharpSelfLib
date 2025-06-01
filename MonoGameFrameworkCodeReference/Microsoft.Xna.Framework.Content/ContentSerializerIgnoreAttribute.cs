using System;

namespace Microsoft.Xna.Framework.Content;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ContentSerializerIgnoreAttribute : Attribute
{
}
