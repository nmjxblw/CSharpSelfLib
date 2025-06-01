using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Provides information about the capabilities of the
/// current graphics device. A very useful thread for investigating GL extenion names
/// http://stackoverflow.com/questions/3881197/opengl-es-2-0-extensions-on-android-devices
/// </summary>
internal class GraphicsCapabilities
{
	private const int MultiSampleCountLimit = 32;

	private int _maxMultiSampleCount;

	/// <summary>
	/// Whether the device fully supports non power-of-two textures, including
	/// mip maps and wrap modes other than CLAMP_TO_EDGE
	/// </summary>
	internal bool SupportsNonPowerOfTwo { get; private set; }

	/// <summary>
	/// Whether the device supports anisotropic texture filtering
	/// </summary>
	internal bool SupportsTextureFilterAnisotropic { get; private set; }

	internal bool SupportsDepth24 { get; private set; }

	internal bool SupportsPackedDepthStencil { get; private set; }

	internal bool SupportsDepthNonLinear { get; private set; }

	/// <summary>
	/// Gets the support for DXT1
	/// </summary>
	internal bool SupportsDxt1 { get; private set; }

	/// <summary>
	/// Gets the support for S3TC (DXT1, DXT3, DXT5)
	/// </summary>
	internal bool SupportsS3tc { get; private set; }

	/// <summary>
	/// Gets the support for PVRTC
	/// </summary>
	internal bool SupportsPvrtc { get; private set; }

	/// <summary>
	/// Gets the support for ETC1
	/// </summary>
	internal bool SupportsEtc1 { get; private set; }

	/// <summary>
	/// Gets the support for ETC2
	/// </summary>
	internal bool SupportsEtc2 { get; private set; }

	/// <summary>
	/// Gets the support for ATITC
	/// </summary>
	internal bool SupportsAtitc { get; private set; }

	internal bool SupportsTextureMaxLevel { get; private set; }

	/// <summary>
	/// True, if sRGB is supported. On Direct3D platforms, this is always <code>true</code>.
	/// On OpenGL platforms, it is <code>true</code> if both framebuffer sRGB
	/// and texture sRGB are supported.
	/// </summary>
	internal bool SupportsSRgb { get; private set; }

	internal bool SupportsTextureArrays { get; private set; }

	internal bool SupportsDepthClamp { get; private set; }

	internal bool SupportsVertexTextures { get; private set; }

	/// <summary>
	/// True, if the underlying platform supports floating point textures. 
	/// For Direct3D platforms this is always <code>true</code>.
	/// For OpenGL Desktop platforms it is always <code>true</code>.
	/// For OpenGL Mobile platforms it requires `GL_EXT_color_buffer_float`.
	/// If the requested format is not supported an <code>NotSupportedException</code>
	/// will be thrown.
	/// </summary>
	internal bool SupportsFloatTextures { get; private set; }

	/// <summary>
	/// True, if the underlying platform supports half floating point textures. 
	/// For Direct3D platforms this is always <code>true</code>.
	/// For OpenGL Desktop platforms it is always <code>true</code>.
	/// For OpenGL Mobile platforms it requires `GL_EXT_color_buffer_half_float`.
	/// If the requested format is not supported an <code>NotSupportedException</code>
	/// will be thrown.
	/// </summary>
	internal bool SupportsHalfFloatTextures { get; private set; }

	internal bool SupportsNormalized { get; private set; }

	/// <summary>
	/// Gets the max texture anisotropy. This value typically lies
	/// between 0 and 16, where 0 means anisotropic filtering is not
	/// supported.
	/// </summary>
	internal int MaxTextureAnisotropy { get; private set; }

	internal int MaxMultiSampleCount => this._maxMultiSampleCount;

	internal bool SupportsInstancing { get; private set; }

	internal bool SupportsBaseIndexInstancing { get; private set; }

	internal bool SupportsSeparateBlendStates { get; private set; }

	/// <summary>
	/// True, if GL_ARB_framebuffer_object is supported; false otherwise.
	/// </summary>
	internal bool SupportsFramebufferObjectARB { get; private set; }

	/// <summary>
	/// True, if GL_EXT_framebuffer_object is supported; false otherwise.
	/// </summary>
	internal bool SupportsFramebufferObjectEXT { get; private set; }

	/// <summary>
	/// True, if GL_IMG_multisampled_render_to_texture is supported; false otherwise.
	/// </summary>
	internal bool SupportsFramebufferObjectIMG { get; private set; }

	internal void Initialize(GraphicsDevice device)
	{
		this.PlatformInitialize(device);
	}

	private void PlatformInitialize(GraphicsDevice device)
	{
		this.SupportsNonPowerOfTwo = device._maxTextureSize >= 8192;
		this.SupportsTextureFilterAnisotropic = GL.Extensions.Contains("GL_EXT_texture_filter_anisotropic");
		this.SupportsDepth24 = true;
		this.SupportsPackedDepthStencil = true;
		this.SupportsDepthNonLinear = false;
		this.SupportsTextureMaxLevel = true;
		this.SupportsS3tc = GL.Extensions.Contains("GL_EXT_texture_compression_s3tc") || GL.Extensions.Contains("GL_OES_texture_compression_S3TC") || GL.Extensions.Contains("GL_EXT_texture_compression_dxt3") || GL.Extensions.Contains("GL_EXT_texture_compression_dxt5");
		this.SupportsDxt1 = this.SupportsS3tc || GL.Extensions.Contains("GL_EXT_texture_compression_dxt1");
		this.SupportsPvrtc = GL.Extensions.Contains("GL_IMG_texture_compression_pvrtc");
		this.SupportsEtc1 = GL.Extensions.Contains("GL_OES_compressed_ETC1_RGB8_texture");
		this.SupportsAtitc = GL.Extensions.Contains("GL_ATI_texture_compression_atitc") || GL.Extensions.Contains("GL_AMD_compressed_ATC_texture");
		if (GL.BoundApi == GL.RenderApi.ES)
		{
			this.SupportsEtc2 = device.glMajorVersion >= 3;
		}
		this.SupportsFramebufferObjectARB = device.glMajorVersion >= 3 || GL.Extensions.Contains("GL_ARB_framebuffer_object");
		this.SupportsFramebufferObjectEXT = GL.Extensions.Contains("GL_EXT_framebuffer_object");
		int anisotropy = 0;
		if (this.SupportsTextureFilterAnisotropic)
		{
			GL.GetInteger(GetPName.MaxTextureMaxAnisotropyExt, out anisotropy);
		}
		this.MaxTextureAnisotropy = anisotropy;
		this.SupportsSRgb = GL.Extensions.Contains("GL_EXT_texture_sRGB") && GL.Extensions.Contains("GL_EXT_framebuffer_sRGB");
		this.SupportsFloatTextures = GL.BoundApi == GL.RenderApi.GL && (device.glMajorVersion >= 3 || GL.Extensions.Contains("GL_ARB_texture_float"));
		this.SupportsHalfFloatTextures = GL.BoundApi == GL.RenderApi.GL && (device.glMajorVersion >= 3 || GL.Extensions.Contains("GL_ARB_half_float_pixel"));
		this.SupportsNormalized = GL.BoundApi == GL.RenderApi.GL && (device.glMajorVersion >= 3 || GL.Extensions.Contains("GL_EXT_texture_norm16"));
		this.SupportsTextureArrays = false;
		this.SupportsDepthClamp = GL.Extensions.Contains("GL_ARB_depth_clamp");
		this.SupportsVertexTextures = false;
		GL.GetInteger(GetPName.MaxSamples, out this._maxMultiSampleCount);
		this.SupportsInstancing = GL.VertexAttribDivisor != null;
		this.SupportsBaseIndexInstancing = GL.DrawElementsInstancedBaseInstance != null;
		this.SupportsSeparateBlendStates = device.glMajorVersion >= 4 || GL.Extensions.Contains("GL_ARB_draw_buffers_blend");
	}
}
