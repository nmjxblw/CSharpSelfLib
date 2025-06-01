using System;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

internal class ConstantBuffer : GraphicsResource
{
	private readonly byte[] _buffer;

	private readonly int[] _parameters;

	private readonly int[] _offsets;

	private readonly string _name;

	private ulong _stateKey;

	private bool _dirty;

	private ShaderProgram _shaderProgram;

	private int _location;

	private static ConstantBuffer _lastConstantBufferApplied;

	private bool Dirty => this._dirty;

	/// <summary>
	/// A hash value which can be used to compare constant buffers.
	/// </summary>
	internal int HashKey { get; private set; }

	public ConstantBuffer(ConstantBuffer cloneSource)
	{
		base.GraphicsDevice = cloneSource.GraphicsDevice;
		this._name = cloneSource._name;
		this._parameters = cloneSource._parameters;
		this._offsets = cloneSource._offsets;
		this._buffer = (byte[])cloneSource._buffer.Clone();
		this.PlatformInitialize();
	}

	public ConstantBuffer(GraphicsDevice device, int sizeInBytes, int[] parameterIndexes, int[] parameterOffsets, string name)
	{
		base.GraphicsDevice = device;
		this._buffer = new byte[sizeInBytes];
		this._parameters = parameterIndexes;
		this._offsets = parameterOffsets;
		this._name = name;
		this.PlatformInitialize();
	}

	internal void Clear()
	{
		this.PlatformClear();
	}

	private void SetData(int offset, int rows, int columns, object data)
	{
		if (rows == 1 && columns == 1)
		{
			if (!(data is Array))
			{
				throw new NotImplementedException();
			}
			Buffer.BlockCopy(data as Array, 0, this._buffer, offset, 4);
		}
		else if (rows == 1 || (rows == 4 && columns == 4))
		{
			int len = rows * columns * 4;
			if (this._buffer.Length - offset > len)
			{
				len = this._buffer.Length - offset;
			}
			Buffer.BlockCopy(data as Array, 0, this._buffer, offset, rows * columns * 4);
		}
		else
		{
			Array source = data as Array;
			int stride = columns * 4;
			for (int y = 0; y < rows; y++)
			{
				Buffer.BlockCopy(source, stride * y, this._buffer, offset + 16 * y, columns * 4);
			}
		}
	}

	private int SetParameter(int offset, EffectParameter param)
	{
		int rowsUsed = 0;
		EffectParameterCollection elements = param.Elements;
		if (elements.Count > 0)
		{
			for (int i = 0; i < elements.Count; i++)
			{
				int rowsUsedSubParam = this.SetParameter(offset, elements[i]);
				offset += rowsUsedSubParam * 16;
				rowsUsed += rowsUsedSubParam;
			}
		}
		else if (param.Data != null)
		{
			EffectParameterType parameterType = param.ParameterType;
			if ((uint)(parameterType - 1) > 2u)
			{
				throw new NotSupportedException("Not supported!");
			}
			if (param.ParameterClass == EffectParameterClass.Matrix)
			{
				rowsUsed = param.ColumnCount;
				this.SetData(offset, param.ColumnCount, param.RowCount, param.Data);
			}
			else
			{
				rowsUsed = param.RowCount;
				this.SetData(offset, param.RowCount, param.ColumnCount, param.Data);
			}
		}
		return rowsUsed;
	}

	public void Update(EffectParameterCollection parameters)
	{
		if (this._stateKey > EffectParameter.NextStateKey)
		{
			this._stateKey = 0uL;
		}
		for (int p = 0; p < this._parameters.Length; p++)
		{
			int index = this._parameters[p];
			EffectParameter param = parameters[index];
			if (param.StateKey >= this._stateKey)
			{
				int offset = this._offsets[p];
				this._dirty = true;
				this.SetParameter(offset, param);
			}
		}
		this._stateKey = EffectParameter.NextStateKey;
	}

	private void PlatformInitialize()
	{
		byte[] data = new byte[this._parameters.Length];
		for (int i = 0; i < this._parameters.Length; i++)
		{
			data[i] = (byte)(this._parameters[i] | this._offsets[i]);
		}
		this.HashKey = Hash.ComputeHash(data);
	}

	private void PlatformClear()
	{
		this._shaderProgram = null;
	}

	public unsafe void PlatformApply(GraphicsDevice device, ShaderProgram program)
	{
		if (this._shaderProgram != program)
		{
			int location = program.GetUniformLocation(this._name);
			if (location == -1)
			{
				return;
			}
			this._shaderProgram = program;
			this._location = location;
			this._dirty = true;
		}
		if (this != ConstantBuffer._lastConstantBufferApplied)
		{
			this._dirty = true;
		}
		if (this._dirty)
		{
			fixed (byte* bytePtr = this._buffer)
			{
				GL.Uniform4(this._location, this._buffer.Length / 16, (float*)bytePtr);
			}
			this._dirty = false;
			ConstantBuffer._lastConstantBufferApplied = this;
		}
	}
}
