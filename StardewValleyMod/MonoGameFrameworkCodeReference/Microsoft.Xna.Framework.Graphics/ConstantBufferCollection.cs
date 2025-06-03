namespace Microsoft.Xna.Framework.Graphics;

internal sealed class ConstantBufferCollection
{
	private readonly ConstantBuffer[] _buffers;

	private ShaderStage _stage;

	private int _valid;

	private ShaderStage Stage => this._stage;

	public ConstantBuffer this[int index]
	{
		get
		{
			return this._buffers[index];
		}
		set
		{
			if (this._buffers[index] != value)
			{
				if (value != null)
				{
					this._buffers[index] = value;
					this._valid |= 1 << index;
				}
				else
				{
					this._buffers[index] = null;
					this._valid &= ~(1 << index);
				}
			}
		}
	}

	internal ConstantBufferCollection(ShaderStage stage, int maxBuffers)
	{
		this._stage = stage;
		this._buffers = new ConstantBuffer[maxBuffers];
		this._valid = 0;
	}

	internal void Clear()
	{
		for (int i = 0; i < this._buffers.Length; i++)
		{
			this._buffers[i] = null;
		}
		this._valid = 0;
	}

	internal void SetConstantBuffers(GraphicsDevice device, ShaderProgram shaderProgram)
	{
		if (this._valid == 0)
		{
			return;
		}
		int valid = this._valid;
		for (int i = 0; i < this._buffers.Length; i++)
		{
			ConstantBuffer buffer = this._buffers[i];
			if (buffer != null && !buffer.IsDisposed)
			{
				buffer.PlatformApply(device, shaderProgram);
			}
			valid &= ~(1 << i);
			if (valid == 0)
			{
				break;
			}
		}
	}
}
