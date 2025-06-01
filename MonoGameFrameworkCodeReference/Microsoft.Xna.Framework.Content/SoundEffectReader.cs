using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework.Content;

internal class SoundEffectReader : ContentTypeReader<SoundEffect>
{
	protected internal override SoundEffect Read(ContentReader input, SoundEffect existingInstance)
	{
		int headerSize = input.ReadInt32();
		byte[] header = input.ReadBytes(headerSize);
		int dataSize = input.ReadInt32();
		byte[] data = ContentManager.ScratchBufferPool.Get(dataSize);
		input.Read(data, 0, dataSize);
		int loopStart = input.ReadInt32();
		int loopLength = input.ReadInt32();
		int durationMs = input.ReadInt32();
		SoundEffect result = new SoundEffect(header, data, dataSize, durationMs, loopStart, loopLength)
		{
			Name = input.AssetName
		};
		ContentManager.ScratchBufferPool.Return(data);
		return result;
	}
}
