using System;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// This class handles the queueing of batch items into the GPU by creating the triangle tesselations
/// that are used to draw the sprite textures. This class supports int.MaxValue number of sprites to be
/// batched and will process them into short.MaxValue groups (strided by 6 for the number of vertices
/// sent to the GPU). 
/// </summary>
internal class SpriteBatcher
{
	/// <summary>
	/// Initialization size for the batch item list and queue.
	/// </summary>
	private const int InitialBatchSize = 256;

	/// <summary>
	/// The maximum number of batch items that can be processed per iteration
	/// </summary>
	private const int MaxBatchSize = 5461;

	/// <summary>
	/// Initialization size for the vertex array, in batch units.
	/// </summary>
	private const int InitialVertexArraySize = 256;

	/// <summary>
	/// The list of batch items to process.
	/// </summary>
	private SpriteBatchItem[] _batchItemList;

	/// <summary>
	/// Index pointer to the next available SpriteBatchItem in _batchItemList.
	/// </summary>
	private int _batchItemCount;

	/// <summary>
	/// The target graphics device.
	/// </summary>
	private readonly GraphicsDevice _device;

	/// <summary>
	/// Vertex index array. The values in this array never change.
	/// </summary>
	private short[] _index;

	private VertexPositionColorTexture[] _vertexArray;

	public SpriteBatcher(GraphicsDevice device, int capacity = 0)
	{
		this._device = device;
		capacity = ((capacity > 0) ? ((capacity + 63) & -64) : 256);
		this._batchItemList = new SpriteBatchItem[capacity];
		this._batchItemCount = 0;
		for (int i = 0; i < capacity; i++)
		{
			this._batchItemList[i] = new SpriteBatchItem();
		}
		this.EnsureArrayCapacity(capacity);
	}

	/// <summary>
	/// Reuse a previously allocated SpriteBatchItem from the item pool. 
	/// if there is none available grow the pool and initialize new items.
	/// </summary>
	/// <returns></returns>
	public SpriteBatchItem CreateBatchItem()
	{
		if (this._batchItemCount >= this._batchItemList.Length)
		{
			int num = this._batchItemList.Length;
			int newSize = num + num / 2;
			newSize = (newSize + 63) & -64;
			Array.Resize(ref this._batchItemList, newSize);
			for (int i = num; i < newSize; i++)
			{
				this._batchItemList[i] = new SpriteBatchItem();
			}
			this.EnsureArrayCapacity(Math.Min(newSize, 5461));
		}
		return this._batchItemList[this._batchItemCount++];
	}

	/// <summary>
	/// Resize and recreate the missing indices for the index and vertex position color buffers.
	/// </summary>
	/// <param name="numBatchItems"></param>
	private unsafe void EnsureArrayCapacity(int numBatchItems)
	{
		int neededCapacity = 6 * numBatchItems;
		if (this._index != null && neededCapacity <= this._index.Length)
		{
			return;
		}
		short[] newIndex = new short[6 * numBatchItems];
		int start = 0;
		if (this._index != null)
		{
			this._index.CopyTo(newIndex, 0);
			start = this._index.Length / 6;
		}
		fixed (short* indexFixedPtr = newIndex)
		{
			short* indexPtr = indexFixedPtr + start * 6;
			int i = start;
			while (i < numBatchItems)
			{
				*indexPtr = (short)(i * 4);
				indexPtr[1] = (short)(i * 4 + 1);
				indexPtr[2] = (short)(i * 4 + 2);
				indexPtr[3] = (short)(i * 4 + 1);
				indexPtr[4] = (short)(i * 4 + 3);
				indexPtr[5] = (short)(i * 4 + 2);
				i++;
				indexPtr += 6;
			}
		}
		this._index = newIndex;
		this._vertexArray = new VertexPositionColorTexture[4 * numBatchItems];
	}

	/// <summary>
	/// Sorts the batch items and then groups batch drawing into maximal allowed batch sets that do not
	/// overflow the 16 bit array indices for vertices.
	/// </summary>
	/// <param name="sortMode">The type of depth sorting desired for the rendering.</param>
	/// <param name="effect">The custom effect to apply to the drawn geometry</param>
	public unsafe void DrawBatch(SpriteSortMode sortMode, Effect effect)
	{
		if (effect != null && effect.IsDisposed)
		{
			throw new ObjectDisposedException("effect");
		}
		if (this._batchItemCount == 0)
		{
			return;
		}
		if ((uint)(sortMode - 2) <= 2u)
		{
			Array.Sort(this._batchItemList, 0, this._batchItemCount);
		}
		int batchIndex = 0;
		int batchCount = this._batchItemCount;
		this._device._graphicsMetrics._spriteCount += batchCount;
		while (batchCount > 0)
		{
			int startIndex = 0;
			int index = 0;
			Texture2D tex = null;
			int numBatchesToProcess = batchCount;
			if (numBatchesToProcess > 5461)
			{
				numBatchesToProcess = 5461;
			}
			fixed (VertexPositionColorTexture* vertexArrayFixedPtr = this._vertexArray)
			{
				VertexPositionColorTexture* vertexArrayPtr = vertexArrayFixedPtr;
				int i = 0;
				while (i < numBatchesToProcess)
				{
					SpriteBatchItem item = this._batchItemList[batchIndex];
					if (item.Texture != tex)
					{
						this.FlushVertexArray(startIndex, index, effect, tex);
						tex = item.Texture;
						startIndex = (index = 0);
						vertexArrayPtr = vertexArrayFixedPtr;
						this._device.Textures[0] = tex;
					}
					*vertexArrayPtr = item.vertexTL;
					vertexArrayPtr[1] = item.vertexTR;
					vertexArrayPtr[2] = item.vertexBL;
					vertexArrayPtr[3] = item.vertexBR;
					item.Texture = null;
					i++;
					batchIndex++;
					index += 4;
					vertexArrayPtr += 4;
				}
			}
			this.FlushVertexArray(startIndex, index, effect, tex);
			batchCount -= numBatchesToProcess;
		}
		this._batchItemCount = 0;
	}

	/// <summary>
	/// Sends the triangle list to the graphics device. Here is where the actual drawing starts.
	/// </summary>
	/// <param name="start">Start index of vertices to draw. Not used except to compute the count of vertices to draw.</param>
	/// <param name="end">End index of vertices to draw. Not used except to compute the count of vertices to draw.</param>
	/// <param name="effect">The custom effect to apply to the geometry</param>
	/// <param name="texture">The texture to draw.</param>
	private void FlushVertexArray(int start, int end, Effect effect, Texture texture)
	{
		if (start == end)
		{
			return;
		}
		int vertexCount = end - start;
		if (effect != null)
		{
			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				this._device.Textures[0] = texture;
				this._device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, this._vertexArray, 0, vertexCount, this._index, 0, vertexCount / 4 * 2, VertexPositionColorTexture.VertexDeclaration);
			}
			return;
		}
		this._device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, this._vertexArray, 0, vertexCount, this._index, 0, vertexCount / 4 * 2, VertexPositionColorTexture.VertexDeclaration);
	}
}
