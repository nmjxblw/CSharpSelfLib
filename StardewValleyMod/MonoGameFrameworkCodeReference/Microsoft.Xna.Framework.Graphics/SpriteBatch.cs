using System;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Helper class for drawing text strings and sprites in one or more optimized batches.
/// </summary>
public class SpriteBatch : GraphicsResource
{
	private readonly SpriteBatcher _batcher;

	private SpriteSortMode _sortMode;

	private BlendState _blendState;

	private SamplerState _samplerState;

	private DepthStencilState _depthStencilState;

	private RasterizerState _rasterizerState;

	private Effect _effect;

	private bool _beginCalled;

	private SpriteEffect _spriteEffect;

	private readonly EffectPass _spritePass;

	private Rectangle _tempRect = new Rectangle(0, 0, 0, 0);

	private Vector2 _texCoordTL = new Vector2(0f, 0f);

	private Vector2 _texCoordBR = new Vector2(0f, 0f);

	/// <summary>
	/// The amount of texels to tuck in the texture in order to avoid artifacts.
	/// </summary>
	public static float TextureTuckAmount;

	public static Matrix? globalMatrix;

	private void _TuckTextureCoordinates(Texture2D texture, ref Vector2 tl, ref Vector2 br)
	{
		tl.X += SpriteBatch.TextureTuckAmount * texture.TexelWidth;
		br.X -= SpriteBatch.TextureTuckAmount * texture.TexelWidth;
		tl.Y += SpriteBatch.TextureTuckAmount * texture.TexelHeight;
		br.Y -= SpriteBatch.TextureTuckAmount * texture.TexelHeight;
	}

	/// <summary>
	/// Constructs a <see cref="T:Microsoft.Xna.Framework.Graphics.SpriteBatch" />.
	/// </summary>
	/// <param name="graphicsDevice">The <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" />, which will be used for sprite rendering.</param>        
	/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="graphicsDevice" /> is null.</exception>
	public SpriteBatch(GraphicsDevice graphicsDevice)
		: this(graphicsDevice, 0)
	{
	}

	/// <summary>
	/// Constructs a <see cref="T:Microsoft.Xna.Framework.Graphics.SpriteBatch" />.
	/// </summary>
	/// <param name="graphicsDevice">The <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" />, which will be used for sprite rendering.</param>
	/// <param name="capacity">The initial capacity of the internal array holding batch items (the value will be rounded to the next multiple of 64).</param>
	/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="graphicsDevice" /> is null.</exception>
	public SpriteBatch(GraphicsDevice graphicsDevice, int capacity)
	{
		if (graphicsDevice == null)
		{
			throw new ArgumentNullException("graphicsDevice", "The GraphicsDevice must not be null when creating new resources.");
		}
		base.GraphicsDevice = graphicsDevice;
		this._spriteEffect = new SpriteEffect(graphicsDevice);
		this._spritePass = this._spriteEffect.CurrentTechnique.Passes[0];
		this._batcher = new SpriteBatcher(graphicsDevice, capacity);
		this._beginCalled = false;
	}

	/// <summary>
	/// Begins a new sprite and text batch with the specified render state.
	/// </summary>
	/// <param name="sortMode">The drawing order for sprite and text drawing. <see cref="F:Microsoft.Xna.Framework.Graphics.SpriteSortMode.Deferred" /> by default.</param>
	/// <param name="blendState">State of the blending. Uses <see cref="F:Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend" /> if null.</param>
	/// <param name="samplerState">State of the sampler. Uses <see cref="F:Microsoft.Xna.Framework.Graphics.SamplerState.LinearClamp" /> if null.</param>
	/// <param name="depthStencilState">State of the depth-stencil buffer. Uses <see cref="F:Microsoft.Xna.Framework.Graphics.DepthStencilState.None" /> if null.</param>
	/// <param name="rasterizerState">State of the rasterization. Uses <see cref="F:Microsoft.Xna.Framework.Graphics.RasterizerState.CullCounterClockwise" /> if null.</param>
	/// <param name="effect">A custom <see cref="T:Microsoft.Xna.Framework.Graphics.Effect" /> to override the default sprite effect. Uses default sprite effect if null.</param>
	/// <param name="transformMatrix">An optional matrix used to transform the sprite geometry. Uses <see cref="P:Microsoft.Xna.Framework.Matrix.Identity" /> if null.</param>
	/// <exception cref="T:System.InvalidOperationException">Thrown if <see cref="M:Microsoft.Xna.Framework.Graphics.SpriteBatch.Begin(Microsoft.Xna.Framework.Graphics.SpriteSortMode,Microsoft.Xna.Framework.Graphics.BlendState,Microsoft.Xna.Framework.Graphics.SamplerState,Microsoft.Xna.Framework.Graphics.DepthStencilState,Microsoft.Xna.Framework.Graphics.RasterizerState,Microsoft.Xna.Framework.Graphics.Effect,System.Nullable{Microsoft.Xna.Framework.Matrix})" /> is called next time without previous <see cref="M:Microsoft.Xna.Framework.Graphics.SpriteBatch.End" />.</exception>
	/// <remarks>This method uses optional parameters.</remarks>
	/// <remarks>The <see cref="M:Microsoft.Xna.Framework.Graphics.SpriteBatch.Begin(Microsoft.Xna.Framework.Graphics.SpriteSortMode,Microsoft.Xna.Framework.Graphics.BlendState,Microsoft.Xna.Framework.Graphics.SamplerState,Microsoft.Xna.Framework.Graphics.DepthStencilState,Microsoft.Xna.Framework.Graphics.RasterizerState,Microsoft.Xna.Framework.Graphics.Effect,System.Nullable{Microsoft.Xna.Framework.Matrix})" /> Begin should be called before drawing commands, and you cannot call it again before subsequent <see cref="M:Microsoft.Xna.Framework.Graphics.SpriteBatch.End" />.</remarks>
	public void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null)
	{
		if (this._beginCalled)
		{
			throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");
		}
		this._sortMode = sortMode;
		this._blendState = blendState ?? BlendState.AlphaBlend;
		this._samplerState = samplerState ?? SamplerState.LinearClamp;
		this._depthStencilState = depthStencilState ?? DepthStencilState.None;
		this._rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
		this._effect = effect;
		this._spriteEffect.TransformMatrix = transformMatrix;
		if (SpriteBatch.globalMatrix.HasValue)
		{
			if (!this._spriteEffect.TransformMatrix.HasValue)
			{
				this._spriteEffect.TransformMatrix = SpriteBatch.globalMatrix;
			}
			else
			{
				this._spriteEffect.TransformMatrix = SpriteBatch.globalMatrix * this._spriteEffect.TransformMatrix;
			}
		}
		if (sortMode == SpriteSortMode.Immediate)
		{
			this.Setup();
		}
		this._beginCalled = true;
	}

	/// <summary>
	/// Flushes all batched text and sprites to the screen.
	/// </summary>
	/// <remarks>This command should be called after <see cref="M:Microsoft.Xna.Framework.Graphics.SpriteBatch.Begin(Microsoft.Xna.Framework.Graphics.SpriteSortMode,Microsoft.Xna.Framework.Graphics.BlendState,Microsoft.Xna.Framework.Graphics.SamplerState,Microsoft.Xna.Framework.Graphics.DepthStencilState,Microsoft.Xna.Framework.Graphics.RasterizerState,Microsoft.Xna.Framework.Graphics.Effect,System.Nullable{Microsoft.Xna.Framework.Matrix})" /> and drawing commands.</remarks>
	public void End()
	{
		if (!this._beginCalled)
		{
			throw new InvalidOperationException("Begin must be called before calling End.");
		}
		this._beginCalled = false;
		if (this._sortMode != SpriteSortMode.Immediate)
		{
			this.Setup();
		}
		this._batcher.DrawBatch(this._sortMode, this._effect);
	}

	private void Setup()
	{
		GraphicsDevice obj = base.GraphicsDevice;
		obj.BlendState = this._blendState;
		obj.DepthStencilState = this._depthStencilState;
		obj.RasterizerState = this._rasterizerState;
		obj.SamplerStates[0] = this._samplerState;
		this._spritePass.Apply();
	}

	private void CheckValid(Texture2D texture)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		if (!this._beginCalled)
		{
			throw new InvalidOperationException("Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
		}
		if (texture.IsDisposed)
		{
			throw new ObjectDisposedException("Can't draw texture" + ((texture.Name != null) ? (" '" + texture.Name + "'") : "") + " because it's disposed.");
		}
	}

	private void CheckValid(SpriteFont spriteFont, string text)
	{
		if (spriteFont == null)
		{
			throw new ArgumentNullException("spriteFont");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (!this._beginCalled)
		{
			throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
		}
	}

	private void CheckValid(SpriteFont spriteFont, StringBuilder text)
	{
		if (spriteFont == null)
		{
			throw new ArgumentNullException("spriteFont");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (!this._beginCalled)
		{
			throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
		}
	}

	/// <summary>
	/// Submit a sprite for drawing in the current batch.
	/// </summary>
	/// <param name="texture">A texture.</param>
	/// <param name="position">The drawing location on screen.</param>
	/// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
	/// <param name="color">A color mask.</param>
	/// <param name="rotation">A rotation of this sprite.</param>
	/// <param name="origin">Center of the rotation. 0,0 by default.</param>
	/// <param name="scale">A scaling of this sprite.</param>
	/// <param name="effects">Modificators for drawing. Can be combined.</param>
	/// <param name="layerDepth">A depth of the layer of this sprite.</param>
	public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
	{
		this.CheckValid(texture);
		SpriteBatchItem item = this._batcher.CreateBatchItem();
		item.Texture = texture;
		switch (this._sortMode)
		{
		case SpriteSortMode.Texture:
			item.SortKey = texture.SortingKey;
			break;
		case SpriteSortMode.FrontToBack:
			item.SortKey = layerDepth;
			break;
		case SpriteSortMode.BackToFront:
			item.SortKey = 0f - layerDepth;
			break;
		}
		origin *= scale;
		float w;
		float h;
		if (sourceRectangle.HasValue)
		{
			Rectangle srcRect = sourceRectangle.GetValueOrDefault();
			w = (float)srcRect.Width * scale.X;
			h = (float)srcRect.Height * scale.Y;
			this._texCoordTL.X = (float)srcRect.X * texture.TexelWidth;
			this._texCoordTL.Y = (float)srcRect.Y * texture.TexelHeight;
			this._texCoordBR.X = (float)(srcRect.X + srcRect.Width) * texture.TexelWidth;
			this._texCoordBR.Y = (float)(srcRect.Y + srcRect.Height) * texture.TexelHeight;
		}
		else
		{
			w = (float)texture.Width * scale.X;
			h = (float)texture.Height * scale.Y;
			this._texCoordTL = Vector2.Zero;
			this._texCoordBR.X = (float)texture.width * texture.TexelWidth;
			this._texCoordBR.Y = (float)texture.height * texture.TexelHeight;
		}
		this._TuckTextureCoordinates(texture, ref this._texCoordTL, ref this._texCoordBR);
		if ((effects & SpriteEffects.FlipVertically) != SpriteEffects.None)
		{
			float temp = this._texCoordBR.Y;
			this._texCoordBR.Y = this._texCoordTL.Y;
			this._texCoordTL.Y = temp;
		}
		if ((effects & SpriteEffects.FlipHorizontally) != SpriteEffects.None)
		{
			float temp2 = this._texCoordBR.X;
			this._texCoordBR.X = this._texCoordTL.X;
			this._texCoordTL.X = temp2;
		}
		if (rotation == 0f)
		{
			item.Set(position.X - origin.X, position.Y - origin.Y, w, h, color, this._texCoordTL, this._texCoordBR, layerDepth);
		}
		else
		{
			item.Set(position.X, position.Y, 0f - origin.X, 0f - origin.Y, w, h, (float)Math.Sin(rotation), (float)Math.Cos(rotation), color, this._texCoordTL, this._texCoordBR, layerDepth);
		}
		this.FlushIfNeeded();
	}

	/// <summary>
	/// Submit a sprite for drawing in the current batch.
	/// </summary>
	/// <param name="texture">A texture.</param>
	/// <param name="position">The drawing location on screen.</param>
	/// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
	/// <param name="color">A color mask.</param>
	/// <param name="rotation">A rotation of this sprite.</param>
	/// <param name="origin">Center of the rotation. 0,0 by default.</param>
	/// <param name="scale">A scaling of this sprite.</param>
	/// <param name="effects">Modificators for drawing. Can be combined.</param>
	/// <param name="layerDepth">A depth of the layer of this sprite.</param>
	public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
	{
		Vector2 scaleVec = new Vector2(scale, scale);
		this.Draw(texture, position, sourceRectangle, color, rotation, origin, scaleVec, effects, layerDepth);
	}

	/// <summary>
	/// Submit a sprite for drawing in the current batch.
	/// </summary>
	/// <param name="texture">A texture.</param>
	/// <param name="destinationRectangle">The drawing bounds on screen.</param>
	/// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
	/// <param name="color">A color mask.</param>
	/// <param name="rotation">A rotation of this sprite.</param>
	/// <param name="origin">Center of the rotation. 0,0 by default.</param>
	/// <param name="effects">Modificators for drawing. Can be combined.</param>
	/// <param name="layerDepth">A depth of the layer of this sprite.</param>
	public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
	{
		this.CheckValid(texture);
		SpriteBatchItem item = this._batcher.CreateBatchItem();
		item.Texture = texture;
		switch (this._sortMode)
		{
		case SpriteSortMode.Texture:
			item.SortKey = texture.SortingKey;
			break;
		case SpriteSortMode.FrontToBack:
			item.SortKey = layerDepth;
			break;
		case SpriteSortMode.BackToFront:
			item.SortKey = 0f - layerDepth;
			break;
		}
		if (sourceRectangle.HasValue)
		{
			Rectangle srcRect = sourceRectangle.GetValueOrDefault();
			this._texCoordTL.X = (float)srcRect.X * texture.TexelWidth;
			this._texCoordTL.Y = (float)srcRect.Y * texture.TexelHeight;
			this._texCoordBR.X = (float)(srcRect.X + srcRect.Width) * texture.TexelWidth;
			this._texCoordBR.Y = (float)(srcRect.Y + srcRect.Height) * texture.TexelHeight;
			if (srcRect.Width != 0)
			{
				origin.X = origin.X * (float)destinationRectangle.Width / (float)srcRect.Width;
			}
			else
			{
				origin.X = origin.X * (float)destinationRectangle.Width * texture.TexelWidth;
			}
			if (srcRect.Height != 0)
			{
				origin.Y = origin.Y * (float)destinationRectangle.Height / (float)srcRect.Height;
			}
			else
			{
				origin.Y = origin.Y * (float)destinationRectangle.Height * texture.TexelHeight;
			}
		}
		else
		{
			this._texCoordTL = Vector2.Zero;
			this._texCoordBR.X = (float)texture.width * texture.TexelWidth;
			this._texCoordBR.Y = (float)texture.height * texture.TexelHeight;
			origin.X = origin.X * (float)destinationRectangle.Width * texture.TexelWidth;
			origin.Y = origin.Y * (float)destinationRectangle.Height * texture.TexelHeight;
		}
		this._TuckTextureCoordinates(texture, ref this._texCoordTL, ref this._texCoordBR);
		if ((effects & SpriteEffects.FlipVertically) != SpriteEffects.None)
		{
			float temp = this._texCoordBR.Y;
			this._texCoordBR.Y = this._texCoordTL.Y;
			this._texCoordTL.Y = temp;
		}
		if ((effects & SpriteEffects.FlipHorizontally) != SpriteEffects.None)
		{
			float temp2 = this._texCoordBR.X;
			this._texCoordBR.X = this._texCoordTL.X;
			this._texCoordTL.X = temp2;
		}
		if (rotation == 0f)
		{
			item.Set((float)destinationRectangle.X - origin.X, (float)destinationRectangle.Y - origin.Y, destinationRectangle.Width, destinationRectangle.Height, color, this._texCoordTL, this._texCoordBR, layerDepth);
		}
		else
		{
			item.Set(destinationRectangle.X, destinationRectangle.Y, 0f - origin.X, 0f - origin.Y, destinationRectangle.Width, destinationRectangle.Height, (float)Math.Sin(rotation), (float)Math.Cos(rotation), color, this._texCoordTL, this._texCoordBR, layerDepth);
		}
		this.FlushIfNeeded();
	}

	internal void FlushIfNeeded()
	{
		if (this._sortMode == SpriteSortMode.Immediate)
		{
			this._batcher.DrawBatch(this._sortMode, this._effect);
		}
	}

	/// <summary>
	/// Submit a sprite for drawing in the current batch.
	/// </summary>
	/// <param name="texture">A texture.</param>
	/// <param name="position">The drawing location on screen.</param>
	/// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
	/// <param name="color">A color mask.</param>
	public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
	{
		this.CheckValid(texture);
		SpriteBatchItem spriteBatchItem = this._batcher.CreateBatchItem();
		spriteBatchItem.Texture = texture;
		spriteBatchItem.SortKey = ((this._sortMode == SpriteSortMode.Texture) ? texture.SortingKey : 0);
		Vector2 size;
		if (sourceRectangle.HasValue)
		{
			Rectangle srcRect = sourceRectangle.GetValueOrDefault();
			size = new Vector2(srcRect.Width, srcRect.Height);
			this._texCoordTL.X = (float)srcRect.X * texture.TexelWidth;
			this._texCoordTL.Y = (float)srcRect.Y * texture.TexelHeight;
			this._texCoordBR.X = (float)(srcRect.X + srcRect.Width) * texture.TexelWidth;
			this._texCoordBR.Y = (float)(srcRect.Y + srcRect.Height) * texture.TexelHeight;
		}
		else
		{
			size = new Vector2(texture.width, texture.height);
			this._texCoordTL = Vector2.Zero;
			this._texCoordBR.X = (float)texture.width * texture.TexelWidth;
			this._texCoordBR.Y = (float)texture.height * texture.TexelHeight;
		}
		this._TuckTextureCoordinates(texture, ref this._texCoordTL, ref this._texCoordBR);
		spriteBatchItem.Set(position.X, position.Y, size.X, size.Y, color, this._texCoordTL, this._texCoordBR, 0f);
		this.FlushIfNeeded();
	}

	/// <summary>
	/// Submit a sprite for drawing in the current batch.
	/// </summary>
	/// <param name="texture">A texture.</param>
	/// <param name="destinationRectangle">The drawing bounds on screen.</param>
	/// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
	/// <param name="color">A color mask.</param>
	public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
	{
		this.CheckValid(texture);
		SpriteBatchItem spriteBatchItem = this._batcher.CreateBatchItem();
		spriteBatchItem.Texture = texture;
		spriteBatchItem.SortKey = ((this._sortMode == SpriteSortMode.Texture) ? texture.SortingKey : 0);
		if (sourceRectangle.HasValue)
		{
			Rectangle srcRect = sourceRectangle.GetValueOrDefault();
			this._texCoordTL.X = (float)srcRect.X * texture.TexelWidth;
			this._texCoordTL.Y = (float)srcRect.Y * texture.TexelHeight;
			this._texCoordBR.X = (float)(srcRect.X + srcRect.Width) * texture.TexelWidth;
			this._texCoordBR.Y = (float)(srcRect.Y + srcRect.Height) * texture.TexelHeight;
		}
		else
		{
			this._texCoordTL = Vector2.Zero;
			this._texCoordBR.X = (float)texture.width * texture.TexelWidth;
			this._texCoordBR.Y = (float)texture.height * texture.TexelHeight;
		}
		this._TuckTextureCoordinates(texture, ref this._texCoordTL, ref this._texCoordBR);
		spriteBatchItem.Set(destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height, color, this._texCoordTL, this._texCoordBR, 0f);
		this.FlushIfNeeded();
	}

	/// <summary>
	/// Submit a sprite for drawing in the current batch.
	/// </summary>
	/// <param name="texture">A texture.</param>
	/// <param name="position">The drawing location on screen.</param>
	/// <param name="color">A color mask.</param>
	public void Draw(Texture2D texture, Vector2 position, Color color)
	{
		this.CheckValid(texture);
		SpriteBatchItem spriteBatchItem = this._batcher.CreateBatchItem();
		spriteBatchItem.Texture = texture;
		spriteBatchItem.SortKey = ((this._sortMode == SpriteSortMode.Texture) ? texture.SortingKey : 0);
		spriteBatchItem.Set(position.X, position.Y, texture.Width, texture.Height, color, Vector2.Zero, new Vector2((float)texture.width * texture.TexelWidth, (float)texture.height * texture.TexelHeight), 0f);
		this.FlushIfNeeded();
	}

	/// <summary>
	/// Submit a sprite for drawing in the current batch.
	/// </summary>
	/// <param name="texture">A texture.</param>
	/// <param name="destinationRectangle">The drawing bounds on screen.</param>
	/// <param name="color">A color mask.</param>
	public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
	{
		this.CheckValid(texture);
		SpriteBatchItem spriteBatchItem = this._batcher.CreateBatchItem();
		spriteBatchItem.Texture = texture;
		spriteBatchItem.SortKey = ((this._sortMode == SpriteSortMode.Texture) ? texture.SortingKey : 0);
		spriteBatchItem.Set(destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height, color, Vector2.Zero, new Vector2((float)texture.width * texture.TexelWidth, (float)texture.height * texture.TexelHeight), 0f);
		this.FlushIfNeeded();
	}

	/// <summary>
	/// Submit a text string of sprites for drawing in the current batch.
	/// </summary>
	/// <param name="spriteFont">A font.</param>
	/// <param name="text">The text which will be drawn.</param>
	/// <param name="position">The drawing location on screen.</param>
	/// <param name="color">A color mask.</param>
	public unsafe void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color)
	{
		this.CheckValid(spriteFont, text);
		position = new Vector2((int)position.X, (int)position.Y);
		float sortKey = ((this._sortMode == SpriteSortMode.Texture) ? spriteFont.Texture.SortingKey : 0);
		Vector2 offset = Vector2.Zero;
		bool firstGlyphOfLine = true;
		fixed (SpriteFont.Glyph* pGlyphs = spriteFont.Glyphs)
		{
			foreach (char c in text)
			{
				switch (c)
				{
				case '\n':
					offset.X = 0f;
					offset.Y += spriteFont.LineSpacing;
					firstGlyphOfLine = true;
					continue;
				case '\r':
					continue;
				}
				int currentGlyphIndex = spriteFont.GetGlyphIndexOrDefault(c);
				SpriteFont.Glyph* pCurrentGlyph = pGlyphs + currentGlyphIndex;
				if (firstGlyphOfLine)
				{
					offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0f);
					firstGlyphOfLine = false;
				}
				else
				{
					offset.X += spriteFont.Spacing + pCurrentGlyph->LeftSideBearing;
				}
				Vector2 p = offset;
				p.X += pCurrentGlyph->Cropping.X;
				p.Y += pCurrentGlyph->Cropping.Y;
				p += position;
				SpriteBatchItem spriteBatchItem = this._batcher.CreateBatchItem();
				spriteBatchItem.Texture = spriteFont.Texture;
				spriteBatchItem.SortKey = sortKey;
				this._texCoordTL.X = (float)pCurrentGlyph->BoundsInTexture.X * spriteFont.Texture.TexelWidth;
				this._texCoordTL.Y = (float)pCurrentGlyph->BoundsInTexture.Y * spriteFont.Texture.TexelHeight;
				this._texCoordBR.X = (float)(pCurrentGlyph->BoundsInTexture.X + pCurrentGlyph->BoundsInTexture.Width) * spriteFont.Texture.TexelWidth;
				this._texCoordBR.Y = (float)(pCurrentGlyph->BoundsInTexture.Y + pCurrentGlyph->BoundsInTexture.Height) * spriteFont.Texture.TexelHeight;
				this._TuckTextureCoordinates(spriteFont.Texture, ref this._texCoordTL, ref this._texCoordBR);
				spriteBatchItem.Set(p.X, p.Y, pCurrentGlyph->BoundsInTexture.Width, pCurrentGlyph->BoundsInTexture.Height, color, this._texCoordTL, this._texCoordBR, 0f);
				offset.X += pCurrentGlyph->Width + pCurrentGlyph->RightSideBearing;
			}
		}
		this.FlushIfNeeded();
	}

	/// <summary>
	/// Submit a text string of sprites for drawing in the current batch.
	/// </summary>
	/// <param name="spriteFont">A font.</param>
	/// <param name="text">The text which will be drawn.</param>
	/// <param name="position">The drawing location on screen.</param>
	/// <param name="color">A color mask.</param>
	/// <param name="rotation">A rotation of this string.</param>
	/// <param name="origin">Center of the rotation. 0,0 by default.</param>
	/// <param name="scale">A scaling of this string.</param>
	/// <param name="effects">Modificators for drawing. Can be combined.</param>
	/// <param name="layerDepth">A depth of the layer of this string.</param>
	public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
	{
		Vector2 scaleVec = new Vector2(scale, scale);
		this.DrawString(spriteFont, text, position, color, rotation, origin, scaleVec, effects, layerDepth);
	}

	/// <summary>
	/// Submit a text string of sprites for drawing in the current batch.
	/// </summary>
	/// <param name="spriteFont">A font.</param>
	/// <param name="text">The text which will be drawn.</param>
	/// <param name="position">The drawing location on screen.</param>
	/// <param name="color">A color mask.</param>
	/// <param name="rotation">A rotation of this string.</param>
	/// <param name="origin">Center of the rotation. 0,0 by default.</param>
	/// <param name="scale">A scaling of this string.</param>
	/// <param name="effects">Modificators for drawing. Can be combined.</param>
	/// <param name="layerDepth">A depth of the layer of this string.</param>
	public unsafe void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
	{
		this.CheckValid(spriteFont, text);
		position = new Vector2((int)position.X, (int)position.Y);
		float sortKey = 0f;
		switch (this._sortMode)
		{
		case SpriteSortMode.Texture:
			sortKey = spriteFont.Texture.SortingKey;
			break;
		case SpriteSortMode.FrontToBack:
			sortKey = layerDepth;
			break;
		case SpriteSortMode.BackToFront:
			sortKey = 0f - layerDepth;
			break;
		}
		Vector2 flipAdjustment = Vector2.Zero;
		bool flippedVert = (effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
		bool flippedHorz = (effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
		if (flippedVert || flippedHorz)
		{
			SpriteFont.CharacterSource source = new SpriteFont.CharacterSource(text);
			spriteFont.MeasureString(ref source, out var size);
			if (flippedHorz)
			{
				origin.X *= -1f;
				flipAdjustment.X = 0f - size.X;
			}
			if (flippedVert)
			{
				origin.Y *= -1f;
				flipAdjustment.Y = (float)spriteFont.LineSpacing - size.Y;
			}
		}
		Matrix transformation = Matrix.Identity;
		float cos = 0f;
		float sin = 0f;
		if (rotation == 0f)
		{
			transformation.M11 = (flippedHorz ? (0f - scale.X) : scale.X);
			transformation.M22 = (flippedVert ? (0f - scale.Y) : scale.Y);
			transformation.M41 = (flipAdjustment.X - origin.X) * transformation.M11 + position.X;
			transformation.M42 = (flipAdjustment.Y - origin.Y) * transformation.M22 + position.Y;
		}
		else
		{
			cos = (float)Math.Cos(rotation);
			sin = (float)Math.Sin(rotation);
			transformation.M11 = (flippedHorz ? (0f - scale.X) : scale.X) * cos;
			transformation.M12 = (flippedHorz ? (0f - scale.X) : scale.X) * sin;
			transformation.M21 = (flippedVert ? (0f - scale.Y) : scale.Y) * (0f - sin);
			transformation.M22 = (flippedVert ? (0f - scale.Y) : scale.Y) * cos;
			transformation.M41 = (flipAdjustment.X - origin.X) * transformation.M11 + (flipAdjustment.Y - origin.Y) * transformation.M21 + position.X;
			transformation.M42 = (flipAdjustment.X - origin.X) * transformation.M12 + (flipAdjustment.Y - origin.Y) * transformation.M22 + position.Y;
		}
		Vector2 offset = Vector2.Zero;
		bool firstGlyphOfLine = true;
		fixed (SpriteFont.Glyph* pGlyphs = spriteFont.Glyphs)
		{
			foreach (char c in text)
			{
				switch (c)
				{
				case '\n':
					offset.X = 0f;
					offset.Y += spriteFont.LineSpacing;
					firstGlyphOfLine = true;
					continue;
				case '\r':
					continue;
				}
				int currentGlyphIndex = spriteFont.GetGlyphIndexOrDefault(c);
				SpriteFont.Glyph* pCurrentGlyph = pGlyphs + currentGlyphIndex;
				if (firstGlyphOfLine)
				{
					offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0f);
					firstGlyphOfLine = false;
				}
				else
				{
					offset.X += spriteFont.Spacing + pCurrentGlyph->LeftSideBearing;
				}
				Vector2 p = offset;
				if (flippedHorz)
				{
					p.X += pCurrentGlyph->BoundsInTexture.Width;
				}
				p.X += pCurrentGlyph->Cropping.X;
				if (flippedVert)
				{
					p.Y += pCurrentGlyph->BoundsInTexture.Height - spriteFont.LineSpacing;
				}
				p.Y += pCurrentGlyph->Cropping.Y;
				Vector2.Transform(ref p, ref transformation, out p);
				SpriteBatchItem item = this._batcher.CreateBatchItem();
				item.Texture = spriteFont.Texture;
				item.SortKey = sortKey;
				this._texCoordTL.X = (float)pCurrentGlyph->BoundsInTexture.X * spriteFont.Texture.TexelWidth;
				this._texCoordTL.Y = (float)pCurrentGlyph->BoundsInTexture.Y * spriteFont.Texture.TexelHeight;
				this._texCoordBR.X = (float)(pCurrentGlyph->BoundsInTexture.X + pCurrentGlyph->BoundsInTexture.Width) * spriteFont.Texture.TexelWidth;
				this._texCoordBR.Y = (float)(pCurrentGlyph->BoundsInTexture.Y + pCurrentGlyph->BoundsInTexture.Height) * spriteFont.Texture.TexelHeight;
				this._TuckTextureCoordinates(spriteFont.Texture, ref this._texCoordTL, ref this._texCoordBR);
				if ((effects & SpriteEffects.FlipVertically) != SpriteEffects.None)
				{
					float temp = this._texCoordBR.Y;
					this._texCoordBR.Y = this._texCoordTL.Y;
					this._texCoordTL.Y = temp;
				}
				if ((effects & SpriteEffects.FlipHorizontally) != SpriteEffects.None)
				{
					float temp2 = this._texCoordBR.X;
					this._texCoordBR.X = this._texCoordTL.X;
					this._texCoordTL.X = temp2;
				}
				if (rotation == 0f)
				{
					item.Set(p.X, p.Y, (float)pCurrentGlyph->BoundsInTexture.Width * scale.X, (float)pCurrentGlyph->BoundsInTexture.Height * scale.Y, color, this._texCoordTL, this._texCoordBR, layerDepth);
				}
				else
				{
					item.Set(p.X, p.Y, 0f, 0f, (float)pCurrentGlyph->BoundsInTexture.Width * scale.X, (float)pCurrentGlyph->BoundsInTexture.Height * scale.Y, sin, cos, color, this._texCoordTL, this._texCoordBR, layerDepth);
				}
				offset.X += pCurrentGlyph->Width + pCurrentGlyph->RightSideBearing;
			}
		}
		this.FlushIfNeeded();
	}

	/// <summary>
	/// Submit a text string of sprites for drawing in the current batch.
	/// </summary>
	/// <param name="spriteFont">A font.</param>
	/// <param name="text">The text which will be drawn.</param>
	/// <param name="position">The drawing location on screen.</param>
	/// <param name="color">A color mask.</param>
	public unsafe void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
	{
		this.CheckValid(spriteFont, text);
		position = new Vector2((int)position.X, (int)position.Y);
		float sortKey = ((this._sortMode == SpriteSortMode.Texture) ? spriteFont.Texture.SortingKey : 0);
		Vector2 offset = Vector2.Zero;
		bool firstGlyphOfLine = true;
		fixed (SpriteFont.Glyph* pGlyphs = spriteFont.Glyphs)
		{
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				switch (c)
				{
				case '\n':
					offset.X = 0f;
					offset.Y += spriteFont.LineSpacing;
					firstGlyphOfLine = true;
					continue;
				case '\r':
					continue;
				}
				int currentGlyphIndex = spriteFont.GetGlyphIndexOrDefault(c);
				SpriteFont.Glyph* pCurrentGlyph = pGlyphs + currentGlyphIndex;
				if (firstGlyphOfLine)
				{
					offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0f);
					firstGlyphOfLine = false;
				}
				else
				{
					offset.X += spriteFont.Spacing + pCurrentGlyph->LeftSideBearing;
				}
				Vector2 p = offset;
				p.X += pCurrentGlyph->Cropping.X;
				p.Y += pCurrentGlyph->Cropping.Y;
				p += position;
				SpriteBatchItem spriteBatchItem = this._batcher.CreateBatchItem();
				spriteBatchItem.Texture = spriteFont.Texture;
				spriteBatchItem.SortKey = sortKey;
				this._texCoordTL.X = (float)pCurrentGlyph->BoundsInTexture.X * spriteFont.Texture.TexelWidth;
				this._texCoordTL.Y = (float)pCurrentGlyph->BoundsInTexture.Y * spriteFont.Texture.TexelHeight;
				this._texCoordBR.X = (float)(pCurrentGlyph->BoundsInTexture.X + pCurrentGlyph->BoundsInTexture.Width) * spriteFont.Texture.TexelWidth;
				this._texCoordBR.Y = (float)(pCurrentGlyph->BoundsInTexture.Y + pCurrentGlyph->BoundsInTexture.Height) * spriteFont.Texture.TexelHeight;
				this._TuckTextureCoordinates(spriteFont.Texture, ref this._texCoordTL, ref this._texCoordBR);
				spriteBatchItem.Set(p.X, p.Y, pCurrentGlyph->BoundsInTexture.Width, pCurrentGlyph->BoundsInTexture.Height, color, this._texCoordTL, this._texCoordBR, 0f);
				offset.X += pCurrentGlyph->Width + pCurrentGlyph->RightSideBearing;
			}
		}
		this.FlushIfNeeded();
	}

	/// <summary>
	/// Submit a text string of sprites for drawing in the current batch.
	/// </summary>
	/// <param name="spriteFont">A font.</param>
	/// <param name="text">The text which will be drawn.</param>
	/// <param name="position">The drawing location on screen.</param>
	/// <param name="color">A color mask.</param>
	/// <param name="rotation">A rotation of this string.</param>
	/// <param name="origin">Center of the rotation. 0,0 by default.</param>
	/// <param name="scale">A scaling of this string.</param>
	/// <param name="effects">Modificators for drawing. Can be combined.</param>
	/// <param name="layerDepth">A depth of the layer of this string.</param>
	public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
	{
		Vector2 scaleVec = new Vector2(scale, scale);
		this.DrawString(spriteFont, text, position, color, rotation, origin, scaleVec, effects, layerDepth);
	}

	/// <summary>
	/// Submit a text string of sprites for drawing in the current batch.
	/// </summary>
	/// <param name="spriteFont">A font.</param>
	/// <param name="text">The text which will be drawn.</param>
	/// <param name="position">The drawing location on screen.</param>
	/// <param name="color">A color mask.</param>
	/// <param name="rotation">A rotation of this string.</param>
	/// <param name="origin">Center of the rotation. 0,0 by default.</param>
	/// <param name="scale">A scaling of this string.</param>
	/// <param name="effects">Modificators for drawing. Can be combined.</param>
	/// <param name="layerDepth">A depth of the layer of this string.</param>
	public unsafe void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
	{
		this.CheckValid(spriteFont, text);
		position = new Vector2((int)position.X, (int)position.Y);
		float sortKey = 0f;
		switch (this._sortMode)
		{
		case SpriteSortMode.Texture:
			sortKey = spriteFont.Texture.SortingKey;
			break;
		case SpriteSortMode.FrontToBack:
			sortKey = layerDepth;
			break;
		case SpriteSortMode.BackToFront:
			sortKey = 0f - layerDepth;
			break;
		}
		Vector2 flipAdjustment = Vector2.Zero;
		bool flippedVert = (effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
		bool flippedHorz = (effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
		if (flippedVert || flippedHorz)
		{
			SpriteFont.CharacterSource source = new SpriteFont.CharacterSource(text);
			spriteFont.MeasureString(ref source, out var size);
			if (flippedHorz)
			{
				origin.X *= -1f;
				flipAdjustment.X = 0f - size.X;
			}
			if (flippedVert)
			{
				origin.Y *= -1f;
				flipAdjustment.Y = (float)spriteFont.LineSpacing - size.Y;
			}
		}
		Matrix transformation = Matrix.Identity;
		float cos = 0f;
		float sin = 0f;
		if (rotation == 0f)
		{
			transformation.M11 = (flippedHorz ? (0f - scale.X) : scale.X);
			transformation.M22 = (flippedVert ? (0f - scale.Y) : scale.Y);
			transformation.M41 = (flipAdjustment.X - origin.X) * transformation.M11 + position.X;
			transformation.M42 = (flipAdjustment.Y - origin.Y) * transformation.M22 + position.Y;
		}
		else
		{
			cos = (float)Math.Cos(rotation);
			sin = (float)Math.Sin(rotation);
			transformation.M11 = (flippedHorz ? (0f - scale.X) : scale.X) * cos;
			transformation.M12 = (flippedHorz ? (0f - scale.X) : scale.X) * sin;
			transformation.M21 = (flippedVert ? (0f - scale.Y) : scale.Y) * (0f - sin);
			transformation.M22 = (flippedVert ? (0f - scale.Y) : scale.Y) * cos;
			transformation.M41 = (flipAdjustment.X - origin.X) * transformation.M11 + (flipAdjustment.Y - origin.Y) * transformation.M21 + position.X;
			transformation.M42 = (flipAdjustment.X - origin.X) * transformation.M12 + (flipAdjustment.Y - origin.Y) * transformation.M22 + position.Y;
		}
		Vector2 offset = Vector2.Zero;
		bool firstGlyphOfLine = true;
		fixed (SpriteFont.Glyph* pGlyphs = spriteFont.Glyphs)
		{
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				switch (c)
				{
				case '\n':
					offset.X = 0f;
					offset.Y += spriteFont.LineSpacing;
					firstGlyphOfLine = true;
					continue;
				case '\r':
					continue;
				}
				int currentGlyphIndex = spriteFont.GetGlyphIndexOrDefault(c);
				SpriteFont.Glyph* pCurrentGlyph = pGlyphs + currentGlyphIndex;
				if (firstGlyphOfLine)
				{
					offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0f);
					firstGlyphOfLine = false;
				}
				else
				{
					offset.X += spriteFont.Spacing + pCurrentGlyph->LeftSideBearing;
				}
				Vector2 p = offset;
				if (flippedHorz)
				{
					p.X += pCurrentGlyph->BoundsInTexture.Width;
				}
				p.X += pCurrentGlyph->Cropping.X;
				if (flippedVert)
				{
					p.Y += pCurrentGlyph->BoundsInTexture.Height - spriteFont.LineSpacing;
				}
				p.Y += pCurrentGlyph->Cropping.Y;
				Vector2.Transform(ref p, ref transformation, out p);
				SpriteBatchItem item = this._batcher.CreateBatchItem();
				item.Texture = spriteFont.Texture;
				item.SortKey = sortKey;
				this._texCoordTL.X = (float)pCurrentGlyph->BoundsInTexture.X * spriteFont.Texture.TexelWidth;
				this._texCoordTL.Y = (float)pCurrentGlyph->BoundsInTexture.Y * spriteFont.Texture.TexelHeight;
				this._texCoordBR.X = (float)(pCurrentGlyph->BoundsInTexture.X + pCurrentGlyph->BoundsInTexture.Width) * spriteFont.Texture.TexelWidth;
				this._texCoordBR.Y = (float)(pCurrentGlyph->BoundsInTexture.Y + pCurrentGlyph->BoundsInTexture.Height) * spriteFont.Texture.TexelHeight;
				this._TuckTextureCoordinates(spriteFont.Texture, ref this._texCoordTL, ref this._texCoordBR);
				if ((effects & SpriteEffects.FlipVertically) != SpriteEffects.None)
				{
					float temp = this._texCoordBR.Y;
					this._texCoordBR.Y = this._texCoordTL.Y;
					this._texCoordTL.Y = temp;
				}
				if ((effects & SpriteEffects.FlipHorizontally) != SpriteEffects.None)
				{
					float temp2 = this._texCoordBR.X;
					this._texCoordBR.X = this._texCoordTL.X;
					this._texCoordTL.X = temp2;
				}
				if (rotation == 0f)
				{
					item.Set(p.X, p.Y, (float)pCurrentGlyph->BoundsInTexture.Width * scale.X, (float)pCurrentGlyph->BoundsInTexture.Height * scale.Y, color, this._texCoordTL, this._texCoordBR, layerDepth);
				}
				else
				{
					item.Set(p.X, p.Y, 0f, 0f, (float)pCurrentGlyph->BoundsInTexture.Width * scale.X, (float)pCurrentGlyph->BoundsInTexture.Height * scale.Y, sin, cos, color, this._texCoordTL, this._texCoordBR, layerDepth);
				}
				offset.X += pCurrentGlyph->Width + pCurrentGlyph->RightSideBearing;
			}
		}
		this.FlushIfNeeded();
	}

	/// <summary>
	/// Immediately releases the unmanaged resources used by this object.
	/// </summary>
	/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed && disposing && this._spriteEffect != null)
		{
			this._spriteEffect.Dispose();
			this._spriteEffect = null;
		}
		base.Dispose(disposing);
	}
}
