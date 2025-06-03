using System.IO;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Internal helper for accessing the bytecode for stock effects.
/// </summary>
internal class EffectResource
{
	public static readonly EffectResource AlphaTestEffect = new EffectResource("Microsoft.Xna.Framework.Platform.Graphics.Effect.Resources.AlphaTestEffect.ogl.mgfxo");

	public static readonly EffectResource BasicEffect = new EffectResource("Microsoft.Xna.Framework.Platform.Graphics.Effect.Resources.BasicEffect.ogl.mgfxo");

	public static readonly EffectResource DualTextureEffect = new EffectResource("Microsoft.Xna.Framework.Platform.Graphics.Effect.Resources.DualTextureEffect.ogl.mgfxo");

	public static readonly EffectResource EnvironmentMapEffect = new EffectResource("Microsoft.Xna.Framework.Platform.Graphics.Effect.Resources.EnvironmentMapEffect.ogl.mgfxo");

	public static readonly EffectResource SkinnedEffect = new EffectResource("Microsoft.Xna.Framework.Platform.Graphics.Effect.Resources.SkinnedEffect.ogl.mgfxo");

	public static readonly EffectResource SpriteEffect = new EffectResource("Microsoft.Xna.Framework.Platform.Graphics.Effect.Resources.SpriteEffect.ogl.mgfxo");

	private readonly object _locker = new object();

	private readonly string _name;

	private volatile byte[] _bytecode;

	private const string AlphaTestEffectName = "Microsoft.Xna.Framework.Platform.Graphics.Effect.Resources.AlphaTestEffect.ogl.mgfxo";

	private const string BasicEffectName = "Microsoft.Xna.Framework.Platform.Graphics.Effect.Resources.BasicEffect.ogl.mgfxo";

	private const string DualTextureEffectName = "Microsoft.Xna.Framework.Platform.Graphics.Effect.Resources.DualTextureEffect.ogl.mgfxo";

	private const string EnvironmentMapEffectName = "Microsoft.Xna.Framework.Platform.Graphics.Effect.Resources.EnvironmentMapEffect.ogl.mgfxo";

	private const string SkinnedEffectName = "Microsoft.Xna.Framework.Platform.Graphics.Effect.Resources.SkinnedEffect.ogl.mgfxo";

	private const string SpriteEffectName = "Microsoft.Xna.Framework.Platform.Graphics.Effect.Resources.SpriteEffect.ogl.mgfxo";

	public byte[] Bytecode
	{
		get
		{
			if (this._bytecode == null)
			{
				lock (this._locker)
				{
					if (this._bytecode != null)
					{
						return this._bytecode;
					}
					Stream stream = ReflectionHelpers.GetAssembly(typeof(EffectResource)).GetManifestResourceStream(this._name);
					using MemoryStream ms = new MemoryStream();
					stream.CopyTo(ms);
					this._bytecode = ms.ToArray();
				}
			}
			return this._bytecode;
		}
	}

	private EffectResource(string name)
	{
		this._name = name;
	}
}
