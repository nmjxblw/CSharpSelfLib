using System;

namespace Microsoft.Xna.Framework.Graphics;

public struct RenderTargetBinding
{
	private readonly Texture _renderTarget;

	private readonly int _arraySlice;

	private DepthFormat _depthFormat;

	public Texture RenderTarget => this._renderTarget;

	public int ArraySlice => this._arraySlice;

	internal DepthFormat DepthFormat => this._depthFormat;

	public RenderTargetBinding(RenderTarget2D renderTarget)
	{
		if (renderTarget == null)
		{
			throw new ArgumentNullException("renderTarget");
		}
		this._renderTarget = renderTarget;
		this._arraySlice = 0;
		this._depthFormat = renderTarget.DepthStencilFormat;
	}

	public RenderTargetBinding(RenderTargetCube renderTarget, CubeMapFace cubeMapFace)
	{
		if (renderTarget == null)
		{
			throw new ArgumentNullException("renderTarget");
		}
		if (cubeMapFace < CubeMapFace.PositiveX || cubeMapFace > CubeMapFace.NegativeZ)
		{
			throw new ArgumentOutOfRangeException("cubeMapFace");
		}
		this._renderTarget = renderTarget;
		this._arraySlice = (int)cubeMapFace;
		this._depthFormat = renderTarget.DepthStencilFormat;
	}

	public static implicit operator RenderTargetBinding(RenderTarget2D renderTarget)
	{
		return new RenderTargetBinding(renderTarget);
	}
}
