namespace Microsoft.Xna.Framework.Graphics;

public class EffectPass
{
	private readonly Effect _effect;

	private readonly Shader _pixelShader;

	private readonly Shader _vertexShader;

	private readonly BlendState _blendState;

	private readonly DepthStencilState _depthStencilState;

	private readonly RasterizerState _rasterizerState;

	public string Name { get; private set; }

	public EffectAnnotationCollection Annotations { get; private set; }

	internal EffectPass(Effect effect, string name, Shader vertexShader, Shader pixelShader, BlendState blendState, DepthStencilState depthStencilState, RasterizerState rasterizerState, EffectAnnotationCollection annotations)
	{
		this._effect = effect;
		this.Name = name;
		this._vertexShader = vertexShader;
		this._pixelShader = pixelShader;
		this._blendState = blendState;
		this._depthStencilState = depthStencilState;
		this._rasterizerState = rasterizerState;
		this.Annotations = annotations;
	}

	internal EffectPass(Effect effect, EffectPass cloneSource)
	{
		this._effect = effect;
		this.Name = cloneSource.Name;
		this._blendState = cloneSource._blendState;
		this._depthStencilState = cloneSource._depthStencilState;
		this._rasterizerState = cloneSource._rasterizerState;
		this.Annotations = cloneSource.Annotations;
		this._vertexShader = cloneSource._vertexShader;
		this._pixelShader = cloneSource._pixelShader;
	}

	public void Apply()
	{
		EffectTechnique current = this._effect.CurrentTechnique;
		this._effect.OnApply();
		if (this._effect.CurrentTechnique != current)
		{
			this._effect.CurrentTechnique.Passes[0].Apply();
			return;
		}
		GraphicsDevice device = this._effect.GraphicsDevice;
		if (this._vertexShader != null)
		{
			device.VertexShader = this._vertexShader;
			this.SetShaderSamplers(this._vertexShader, device.VertexTextures, device.VertexSamplerStates);
			for (int c = 0; c < this._vertexShader.CBuffers.Length; c++)
			{
				ConstantBuffer cb = this._effect.ConstantBuffers[this._vertexShader.CBuffers[c]];
				cb.Update(this._effect.Parameters);
				device.SetConstantBuffer(ShaderStage.Vertex, c, cb);
			}
		}
		if (this._pixelShader != null)
		{
			device.PixelShader = this._pixelShader;
			this.SetShaderSamplers(this._pixelShader, device.Textures, device.SamplerStates);
			for (int i = 0; i < this._pixelShader.CBuffers.Length; i++)
			{
				ConstantBuffer cb2 = this._effect.ConstantBuffers[this._pixelShader.CBuffers[i]];
				cb2.Update(this._effect.Parameters);
				device.SetConstantBuffer(ShaderStage.Pixel, i, cb2);
			}
		}
		if (this._rasterizerState != null)
		{
			device.RasterizerState = this._rasterizerState;
		}
		if (this._blendState != null)
		{
			device.BlendState = this._blendState;
		}
		if (this._depthStencilState != null)
		{
			device.DepthStencilState = this._depthStencilState;
		}
	}

	private void SetShaderSamplers(Shader shader, TextureCollection textures, SamplerStateCollection samplerStates)
	{
		SamplerInfo[] samplers = shader.Samplers;
		for (int i = 0; i < samplers.Length; i++)
		{
			SamplerInfo sampler = samplers[i];
			Texture texture = this._effect.Parameters[sampler.parameter].Data as Texture;
			textures[sampler.textureSlot] = texture;
			if (sampler.state != null)
			{
				samplerStates[sampler.samplerSlot] = sampler.state;
			}
		}
	}
}
