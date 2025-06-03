namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// A snapshot of rendering statistics from <see cref="P:Microsoft.Xna.Framework.Graphics.GraphicsDevice.Metrics" /> to be used for runtime debugging and profiling.
/// </summary>
public struct GraphicsMetrics
{
	internal long _clearCount;

	internal long _drawCount;

	internal long _pixelShaderCount;

	internal long _primitiveCount;

	internal long _spriteCount;

	internal long _targetCount;

	internal long _textureCount;

	internal long _vertexShaderCount;

	/// <summary>
	/// Number of times Clear was called.
	/// </summary>
	public long ClearCount => this._clearCount;

	/// <summary>
	/// Number of times Draw was called.
	/// </summary>
	public long DrawCount => this._drawCount;

	/// <summary>
	/// Number of times the pixel shader was changed on the GPU.
	/// </summary>
	public long PixelShaderCount => this._pixelShaderCount;

	/// <summary>
	/// Number of rendered primitives.
	/// </summary>
	public long PrimitiveCount => this._primitiveCount;

	/// <summary>
	/// Number of sprites and text characters rendered via <see cref="T:Microsoft.Xna.Framework.Graphics.SpriteBatch" />.
	/// </summary>
	public long SpriteCount => this._spriteCount;

	/// <summary>
	/// Number of times a target was changed on the GPU.
	/// </summary>
	public long TargetCount => this._targetCount;

	/// <summary>
	/// Number of times a texture was changed on the GPU.
	/// </summary>
	public long TextureCount => this._textureCount;

	/// <summary>
	/// Number of times the vertex shader was changed on the GPU.
	/// </summary>
	public long VertexShaderCount => this._vertexShaderCount;

	/// <summary>
	/// Returns the difference between two sets of metrics.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsMetrics" /> on the left of the sub sign.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsMetrics" /> on the right of the sub sign.</param>
	/// <returns>Difference between two sets of metrics.</returns>
	public static GraphicsMetrics operator -(GraphicsMetrics value1, GraphicsMetrics value2)
	{
		return new GraphicsMetrics
		{
			_clearCount = value1._clearCount - value2._clearCount,
			_drawCount = value1._drawCount - value2._drawCount,
			_pixelShaderCount = value1._pixelShaderCount - value2._pixelShaderCount,
			_primitiveCount = value1._primitiveCount - value2._primitiveCount,
			_spriteCount = value1._spriteCount - value2._spriteCount,
			_targetCount = value1._targetCount - value2._targetCount,
			_textureCount = value1._textureCount - value2._textureCount,
			_vertexShaderCount = value1._vertexShaderCount - value2._vertexShaderCount
		};
	}

	/// <summary>
	/// Returns the combination of two sets of metrics.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsMetrics" /> on the left of the add sign.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsMetrics" /> on the right of the add sign.</param>
	/// <returns>Combination of two sets of metrics.</returns>
	public static GraphicsMetrics operator +(GraphicsMetrics value1, GraphicsMetrics value2)
	{
		return new GraphicsMetrics
		{
			_clearCount = value1._clearCount + value2._clearCount,
			_drawCount = value1._drawCount + value2._drawCount,
			_pixelShaderCount = value1._pixelShaderCount + value2._pixelShaderCount,
			_primitiveCount = value1._primitiveCount + value2._primitiveCount,
			_spriteCount = value1._spriteCount + value2._spriteCount,
			_targetCount = value1._targetCount + value2._targetCount,
			_textureCount = value1._textureCount + value2._textureCount,
			_vertexShaderCount = value1._vertexShaderCount + value2._vertexShaderCount
		};
	}
}
