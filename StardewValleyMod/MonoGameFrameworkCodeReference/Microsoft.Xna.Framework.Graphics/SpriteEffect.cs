namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// The default effect used by SpriteBatch.
/// </summary>
public class SpriteEffect : Effect
{
	private EffectParameter _matrixParam;

	private Viewport _lastViewport;

	private Matrix _projection;

	/// <summary>
	/// An optional matrix used to transform the sprite geometry. Uses <see cref="P:Microsoft.Xna.Framework.Matrix.Identity" /> if null.
	/// </summary>
	public Matrix? TransformMatrix { get; set; }

	/// <summary>
	/// Creates a new SpriteEffect.
	/// </summary>
	public SpriteEffect(GraphicsDevice device)
		: base(device, EffectResource.SpriteEffect.Bytecode)
	{
		this.CacheEffectParameters();
	}

	/// <summary>
	/// Creates a new SpriteEffect by cloning parameter settings from an existing instance.
	/// </summary>
	protected SpriteEffect(SpriteEffect cloneSource)
		: base(cloneSource)
	{
		this.CacheEffectParameters();
	}

	/// <summary>
	/// Creates a clone of the current SpriteEffect instance.
	/// </summary>
	public override Effect Clone()
	{
		return new SpriteEffect(this);
	}

	/// <summary>
	/// Looks up shortcut references to our effect parameters.
	/// </summary>
	private void CacheEffectParameters()
	{
		this._matrixParam = base.Parameters["MatrixTransform"];
	}

	/// <summary>
	/// Lazily computes derived parameter values immediately before applying the effect.
	/// </summary>
	protected internal override void OnApply()
	{
		Viewport vp = base.GraphicsDevice.Viewport;
		if (vp.Width != this._lastViewport.Width || vp.Height != this._lastViewport.Height)
		{
			Matrix.CreateOrthographicOffCenter(0f, vp.Width, vp.Height, 0f, 0f, -1f, out this._projection);
			if (base.GraphicsDevice.UseHalfPixelOffset)
			{
				this._projection.M41 += -0.5f * this._projection.M11;
				this._projection.M42 += -0.5f * this._projection.M22;
			}
			this._lastViewport = vp;
		}
		if (this.TransformMatrix.HasValue)
		{
			this._matrixParam.SetValue(this.TransformMatrix.GetValueOrDefault() * this._projection);
		}
		else
		{
			this._matrixParam.SetValue(this._projection);
		}
	}
}
