using System;

namespace Microsoft.Xna.Framework.Graphics;

internal class SpriteBatchItem : IComparable<SpriteBatchItem>
{
	public Texture2D Texture;

	public float SortKey;

	public VertexPositionColorTexture vertexTL;

	public VertexPositionColorTexture vertexTR;

	public VertexPositionColorTexture vertexBL;

	public VertexPositionColorTexture vertexBR;

	public SpriteBatchItem()
	{
		this.vertexTL = default(VertexPositionColorTexture);
		this.vertexTR = default(VertexPositionColorTexture);
		this.vertexBL = default(VertexPositionColorTexture);
		this.vertexBR = default(VertexPositionColorTexture);
	}

	public void Set(float x, float y, float dx, float dy, float w, float h, float sin, float cos, Color color, Vector2 texCoordTL, Vector2 texCoordBR, float depth)
	{
		this.vertexTL.Position.X = x + dx * cos - dy * sin;
		this.vertexTL.Position.Y = y + dx * sin + dy * cos;
		this.vertexTL.Position.Z = 0f;
		this.vertexTL.Color = color;
		this.vertexTL.TextureCoordinate.X = texCoordTL.X;
		this.vertexTL.TextureCoordinate.Y = texCoordTL.Y;
		this.vertexTR.Position.X = x + (dx + w) * cos - dy * sin;
		this.vertexTR.Position.Y = y + (dx + w) * sin + dy * cos;
		this.vertexTR.Position.Z = 0f;
		this.vertexTR.Color = color;
		this.vertexTR.TextureCoordinate.X = texCoordBR.X;
		this.vertexTR.TextureCoordinate.Y = texCoordTL.Y;
		this.vertexBL.Position.X = x + dx * cos - (dy + h) * sin;
		this.vertexBL.Position.Y = y + dx * sin + (dy + h) * cos;
		this.vertexBL.Position.Z = 0f;
		this.vertexBL.Color = color;
		this.vertexBL.TextureCoordinate.X = texCoordTL.X;
		this.vertexBL.TextureCoordinate.Y = texCoordBR.Y;
		this.vertexBR.Position.X = x + (dx + w) * cos - (dy + h) * sin;
		this.vertexBR.Position.Y = y + (dx + w) * sin + (dy + h) * cos;
		this.vertexBR.Position.Z = 0f;
		this.vertexBR.Color = color;
		this.vertexBR.TextureCoordinate.X = texCoordBR.X;
		this.vertexBR.TextureCoordinate.Y = texCoordBR.Y;
	}

	public void Set(float x, float y, float w, float h, Color color, Vector2 texCoordTL, Vector2 texCoordBR, float depth)
	{
		this.vertexTL.Position.X = x;
		this.vertexTL.Position.Y = y;
		this.vertexTL.Position.Z = 0f;
		this.vertexTL.Color = color;
		this.vertexTL.TextureCoordinate.X = texCoordTL.X;
		this.vertexTL.TextureCoordinate.Y = texCoordTL.Y;
		this.vertexTR.Position.X = x + w;
		this.vertexTR.Position.Y = y;
		this.vertexTR.Position.Z = 0f;
		this.vertexTR.Color = color;
		this.vertexTR.TextureCoordinate.X = texCoordBR.X;
		this.vertexTR.TextureCoordinate.Y = texCoordTL.Y;
		this.vertexBL.Position.X = x;
		this.vertexBL.Position.Y = y + h;
		this.vertexBL.Position.Z = 0f;
		this.vertexBL.Color = color;
		this.vertexBL.TextureCoordinate.X = texCoordTL.X;
		this.vertexBL.TextureCoordinate.Y = texCoordBR.Y;
		this.vertexBR.Position.X = x + w;
		this.vertexBR.Position.Y = y + h;
		this.vertexBR.Position.Z = 0f;
		this.vertexBR.Color = color;
		this.vertexBR.TextureCoordinate.X = texCoordBR.X;
		this.vertexBR.TextureCoordinate.Y = texCoordBR.Y;
	}

	public int CompareTo(SpriteBatchItem other)
	{
		return this.SortKey.CompareTo(other.SortKey);
	}
}
