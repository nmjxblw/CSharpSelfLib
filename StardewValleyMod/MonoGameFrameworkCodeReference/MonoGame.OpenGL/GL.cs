using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace MonoGame.OpenGL;

internal class GL
{
	internal enum RenderApi
	{
		ES = 12448,
		GL = 12450
	}

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void EnableVertexAttribArrayDelegate(int attrib);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DisableVertexAttribArrayDelegate(int attrib);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void MakeCurrentDelegate(IntPtr window);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal unsafe delegate void GetIntegerDelegate(int param, [Out] int* data);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate IntPtr GetStringDelegate(StringName param);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void ClearDepthDelegate(float depth);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DepthRangedDelegate(double min, double max);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DepthRangefDelegate(float min, float max);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void ClearDelegate(ClearBufferMask mask);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void ClearColorDelegate(float red, float green, float blue, float alpha);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void ClearStencilDelegate(int stencil);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void ViewportDelegate(int x, int y, int w, int h);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate ErrorCode GetErrorDelegate();

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void FlushDelegate();

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void GenTexturesDelegte(int count, out int id);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void BindTextureDelegate(TextureTarget target, int id);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate int EnableDelegate(EnableCap cap);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate int DisableDelegate(EnableCap cap);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void CullFaceDelegate(CullFaceMode mode);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void FrontFaceDelegate(FrontFaceDirection direction);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void PolygonModeDelegate(MaterialFace face, PolygonMode mode);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void PolygonOffsetDelegate(float slopeScaleDepthBias, float depthbias);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DrawBuffersDelegate(int count, DrawBuffersEnum[] buffers);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void UseProgramDelegate(int program);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal unsafe delegate void Uniform4fvDelegate(int location, int size, float* values);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void Uniform1iDelegate(int location, int value);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void ScissorDelegate(int x, int y, int width, int height);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void ReadPixelsDelegate(int x, int y, int width, int height, PixelFormat format, PixelType type, IntPtr data);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void BindBufferDelegate(BufferTarget target, int buffer);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DrawElementsDelegate(GLPrimitiveType primitiveType, int count, DrawElementsType elementType, IntPtr offset);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DrawArraysDelegate(GLPrimitiveType primitiveType, int offset, int count);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void GenRenderbuffersDelegate(int count, out int buffer);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void BindRenderbufferDelegate(RenderbufferTarget target, int buffer);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DeleteRenderbuffersDelegate(int count, [In][Out] ref int buffer);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void RenderbufferStorageMultisampleDelegate(RenderbufferTarget target, int sampleCount, RenderbufferStorage storage, int width, int height);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void GenFramebuffersDelegate(int count, out int buffer);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void BindFramebufferDelegate(FramebufferTarget target, int buffer);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DeleteFramebuffersDelegate(int count, ref int buffer);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	public delegate void InvalidateFramebufferDelegate(FramebufferTarget target, int numAttachments, FramebufferAttachment[] attachments);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void FramebufferTexture2DDelegate(FramebufferTarget target, FramebufferAttachment attachement, TextureTarget textureTarget, int texture, int level);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void FramebufferTexture2DMultiSampleDelegate(FramebufferTarget target, FramebufferAttachment attachement, TextureTarget textureTarget, int texture, int level, int samples);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void FramebufferRenderbufferDelegate(FramebufferTarget target, FramebufferAttachment attachement, RenderbufferTarget renderBufferTarget, int buffer);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	public delegate void RenderbufferStorageDelegate(RenderbufferTarget target, RenderbufferStorage storage, int width, int hegiht);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void GenerateMipmapDelegate(GenerateMipmapTarget target);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void ReadBufferDelegate(ReadBufferMode buffer);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DrawBufferDelegate(DrawBufferMode buffer);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void BlitFramebufferDelegate(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, ClearBufferMask mask, BlitFramebufferFilter filter);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate FramebufferErrorCode CheckFramebufferStatusDelegate(FramebufferTarget target);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void TexParameterFloatDelegate(TextureTarget target, TextureParameterName name, float value);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal unsafe delegate void TexParameterFloatArrayDelegate(TextureTarget target, TextureParameterName name, float* values);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void TexParameterIntDelegate(TextureTarget target, TextureParameterName name, int value);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void GenQueriesDelegate(int count, out int queryId);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void BeginQueryDelegate(QueryTarget target, int queryId);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void EndQueryDelegate(QueryTarget target);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void GetQueryObjectDelegate(int queryId, GetQueryObjectParam getparam, out int ready);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DeleteQueriesDelegate(int count, [In][Out] ref int queryId);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void ActiveTextureDelegate(TextureUnit textureUnit);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate int CreateShaderDelegate(ShaderType type);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal unsafe delegate void ShaderSourceDelegate(int shaderId, int count, IntPtr code, int* length);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void CompileShaderDelegate(int shaderId);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal unsafe delegate void GetShaderDelegate(int shaderId, int parameter, int* value);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void GetShaderInfoLogDelegate(int shader, int bufSize, IntPtr length, StringBuilder infoLog);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate bool IsShaderDelegate(int shaderId);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DeleteShaderDelegate(int shaderId);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate int GetAttribLocationDelegate(int programId, string name);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate int GetUniformLocationDelegate(int programId, string name);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate bool IsProgramDelegate(int programId);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DeleteProgramDelegate(int programId);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate int CreateProgramDelegate();

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void AttachShaderDelegate(int programId, int shaderId);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void LinkProgramDelegate(int programId);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal unsafe delegate void GetProgramDelegate(int programId, int name, int* linked);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void GetProgramInfoLogDelegate(int program, int bufSize, IntPtr length, StringBuilder infoLog);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DetachShaderDelegate(int programId, int shaderId);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void BlendColorDelegate(float r, float g, float b, float a);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void BlendEquationSeparateDelegate(BlendEquationMode colorMode, BlendEquationMode alphaMode);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void BlendEquationSeparateiDelegate(int buffer, BlendEquationMode colorMode, BlendEquationMode alphaMode);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void BlendFuncSeparateDelegate(BlendingFactorSrc colorSrc, BlendingFactorDest colorDst, BlendingFactorSrc alphaSrc, BlendingFactorDest alphaDst);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void BlendFuncSeparateiDelegate(int buffer, BlendingFactorSrc colorSrc, BlendingFactorDest colorDst, BlendingFactorSrc alphaSrc, BlendingFactorDest alphaDst);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void ColorMaskDelegate(bool r, bool g, bool b, bool a);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DepthFuncDelegate(DepthFunction function);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DepthMaskDelegate(bool enabled);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void StencilFuncSeparateDelegate(StencilFace face, GLStencilFunction function, int referenceStencil, int mask);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void StencilOpSeparateDelegate(StencilFace face, StencilOp stencilfail, StencilOp depthFail, StencilOp pass);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void StencilFuncDelegate(GLStencilFunction function, int referenceStencil, int mask);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void StencilOpDelegate(StencilOp stencilfail, StencilOp depthFail, StencilOp pass);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void StencilMaskDelegate(int mask);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void CompressedTexImage2DDelegate(TextureTarget target, int level, PixelInternalFormat internalFormat, int width, int height, int border, int size, IntPtr data);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void TexImage2DDelegate(TextureTarget target, int level, PixelInternalFormat internalFormat, int width, int height, int border, PixelFormat format, PixelType pixelType, IntPtr data);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void CompressedTexSubImage2DDelegate(TextureTarget target, int level, int x, int y, int width, int height, PixelInternalFormat format, int size, IntPtr data);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void TexSubImage2DDelegate(TextureTarget target, int level, int x, int y, int width, int height, PixelFormat format, PixelType pixelType, IntPtr data);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void PixelStoreDelegate(PixelStoreParameter parameter, int size);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void FinishDelegate();

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void GetTexImageDelegate(TextureTarget target, int level, PixelFormat format, PixelType type, [Out] IntPtr pixels);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void GetCompressedTexImageDelegate(TextureTarget target, int level, [Out] IntPtr pixels);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void TexImage3DDelegate(TextureTarget target, int level, PixelInternalFormat internalFormat, int width, int height, int depth, int border, PixelFormat format, PixelType pixelType, IntPtr data);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void TexSubImage3DDelegate(TextureTarget target, int level, int x, int y, int z, int width, int height, int depth, PixelFormat format, PixelType pixelType, IntPtr data);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DeleteTexturesDelegate(int count, ref int id);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void GenBuffersDelegate(int count, out int buffer);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void BufferDataDelegate(BufferTarget target, IntPtr size, IntPtr n, BufferUsageHint usage);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate IntPtr MapBufferDelegate(BufferTarget target, BufferAccess access);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void UnmapBufferDelegate(BufferTarget target);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void BufferSubDataDelegate(BufferTarget target, IntPtr offset, IntPtr size, IntPtr data);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DeleteBuffersDelegate(int count, [In][Out] ref int buffer);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void VertexAttribPointerDelegate(int location, int elementCount, VertexAttribPointerType type, bool normalize, int stride, IntPtr data);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DrawElementsInstancedDelegate(GLPrimitiveType primitiveType, int count, DrawElementsType elementType, IntPtr offset, int instanceCount);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void DrawElementsInstancedBaseInstanceDelegate(GLPrimitiveType primitiveType, int count, DrawElementsType elementType, IntPtr offset, int instanceCount, int baseInstance);

	[SuppressUnmanagedCodeSecurity]
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[MonoNativeFunctionWrapper]
	internal delegate void VertexAttribDivisorDelegate(int location, int frequency);

	internal static RenderApi BoundApi = RenderApi.GL;

	private const CallingConvention callingConvention = CallingConvention.Winapi;

	internal static EnableVertexAttribArrayDelegate EnableVertexAttribArray;

	internal static DisableVertexAttribArrayDelegate DisableVertexAttribArray;

	internal static MakeCurrentDelegate MakeCurrent;

	internal static GetIntegerDelegate GetIntegerv;

	internal static GetStringDelegate GetStringInternal;

	internal static ClearDepthDelegate ClearDepth;

	internal static DepthRangedDelegate DepthRanged;

	internal static DepthRangefDelegate DepthRangef;

	internal static ClearDelegate Clear;

	internal static ClearColorDelegate ClearColor;

	internal static ClearStencilDelegate ClearStencil;

	internal static ViewportDelegate Viewport;

	internal static GetErrorDelegate GetError;

	internal static FlushDelegate Flush;

	internal static GenTexturesDelegte GenTextures;

	internal static BindTextureDelegate BindTexture;

	internal static EnableDelegate Enable;

	internal static DisableDelegate Disable;

	internal static CullFaceDelegate CullFace;

	internal static FrontFaceDelegate FrontFace;

	internal static PolygonModeDelegate PolygonMode;

	internal static PolygonOffsetDelegate PolygonOffset;

	internal static DrawBuffersDelegate DrawBuffers;

	internal static UseProgramDelegate UseProgram;

	internal static Uniform4fvDelegate Uniform4fv;

	internal static Uniform1iDelegate Uniform1i;

	internal static ScissorDelegate Scissor;

	internal static ReadPixelsDelegate ReadPixelsInternal;

	internal static BindBufferDelegate BindBuffer;

	internal static DrawElementsDelegate DrawElements;

	internal static DrawArraysDelegate DrawArrays;

	internal static GenRenderbuffersDelegate GenRenderbuffers;

	internal static BindRenderbufferDelegate BindRenderbuffer;

	internal static DeleteRenderbuffersDelegate DeleteRenderbuffers;

	internal static RenderbufferStorageMultisampleDelegate RenderbufferStorageMultisample;

	internal static GenFramebuffersDelegate GenFramebuffers;

	internal static BindFramebufferDelegate BindFramebuffer;

	internal static DeleteFramebuffersDelegate DeleteFramebuffers;

	public static InvalidateFramebufferDelegate InvalidateFramebuffer;

	internal static FramebufferTexture2DDelegate FramebufferTexture2D;

	internal static FramebufferTexture2DMultiSampleDelegate FramebufferTexture2DMultiSample;

	internal static FramebufferRenderbufferDelegate FramebufferRenderbuffer;

	public static RenderbufferStorageDelegate RenderbufferStorage;

	internal static GenerateMipmapDelegate GenerateMipmap;

	internal static ReadBufferDelegate ReadBuffer;

	internal static DrawBufferDelegate DrawBuffer;

	internal static BlitFramebufferDelegate BlitFramebuffer;

	internal static CheckFramebufferStatusDelegate CheckFramebufferStatus;

	internal static TexParameterFloatDelegate TexParameterf;

	internal static TexParameterFloatArrayDelegate TexParameterfv;

	internal static TexParameterIntDelegate TexParameteri;

	internal static GenQueriesDelegate GenQueries;

	internal static BeginQueryDelegate BeginQuery;

	internal static EndQueryDelegate EndQuery;

	internal static GetQueryObjectDelegate GetQueryObject;

	internal static DeleteQueriesDelegate DeleteQueries;

	internal static ActiveTextureDelegate ActiveTexture;

	internal static CreateShaderDelegate CreateShader;

	internal static ShaderSourceDelegate ShaderSourceInternal;

	internal static CompileShaderDelegate CompileShader;

	internal static GetShaderDelegate GetShaderiv;

	internal static GetShaderInfoLogDelegate GetShaderInfoLogInternal;

	internal static IsShaderDelegate IsShader;

	internal static DeleteShaderDelegate DeleteShader;

	internal static GetAttribLocationDelegate GetAttribLocation;

	internal static GetUniformLocationDelegate GetUniformLocation;

	internal static IsProgramDelegate IsProgram;

	internal static DeleteProgramDelegate DeleteProgram;

	internal static CreateProgramDelegate CreateProgram;

	internal static AttachShaderDelegate AttachShader;

	internal static LinkProgramDelegate LinkProgram;

	internal static GetProgramDelegate GetProgramiv;

	internal static GetProgramInfoLogDelegate GetProgramInfoLogInternal;

	internal static DetachShaderDelegate DetachShader;

	internal static BlendColorDelegate BlendColor;

	internal static BlendEquationSeparateDelegate BlendEquationSeparate;

	internal static BlendEquationSeparateiDelegate BlendEquationSeparatei;

	internal static BlendFuncSeparateDelegate BlendFuncSeparate;

	internal static BlendFuncSeparateiDelegate BlendFuncSeparatei;

	internal static ColorMaskDelegate ColorMask;

	internal static DepthFuncDelegate DepthFunc;

	internal static DepthMaskDelegate DepthMask;

	internal static StencilFuncSeparateDelegate StencilFuncSeparate;

	internal static StencilOpSeparateDelegate StencilOpSeparate;

	internal static StencilFuncDelegate StencilFunc;

	internal static StencilOpDelegate StencilOp;

	internal static StencilMaskDelegate StencilMask;

	internal static CompressedTexImage2DDelegate CompressedTexImage2D;

	internal static TexImage2DDelegate TexImage2D;

	internal static CompressedTexSubImage2DDelegate CompressedTexSubImage2D;

	internal static TexSubImage2DDelegate TexSubImage2D;

	internal static PixelStoreDelegate PixelStore;

	internal static FinishDelegate Finish;

	internal static GetTexImageDelegate GetTexImageInternal;

	internal static GetCompressedTexImageDelegate GetCompressedTexImageInternal;

	internal static TexImage3DDelegate TexImage3D;

	internal static TexSubImage3DDelegate TexSubImage3D;

	internal static DeleteTexturesDelegate DeleteTextures;

	internal static GenBuffersDelegate GenBuffers;

	internal static BufferDataDelegate BufferData;

	internal static MapBufferDelegate MapBuffer;

	internal static UnmapBufferDelegate UnmapBuffer;

	internal static BufferSubDataDelegate BufferSubData;

	internal static DeleteBuffersDelegate DeleteBuffers;

	internal static VertexAttribPointerDelegate VertexAttribPointer;

	internal static DrawElementsInstancedDelegate DrawElementsInstanced;

	internal static DrawElementsInstancedBaseInstanceDelegate DrawElementsInstancedBaseInstance;

	internal static VertexAttribDivisorDelegate VertexAttribDivisor;

	internal static List<string> Extensions = new List<string>();

	internal static int SwapInterval { get; set; }

	private static T LoadFunction<T>(string function, bool throwIfNotFound = false)
	{
		IntPtr ret = Sdl.GL.GetProcAddress(function);
		if (ret == IntPtr.Zero)
		{
			if (throwIfNotFound)
			{
				throw new EntryPointNotFoundException(function);
			}
			return default(T);
		}
		return Marshal.GetDelegateForFunctionPointer<T>(ret);
	}

	private static IGraphicsContext PlatformCreateContext(IWindowInfo info)
	{
		return new GraphicsContext(info);
	}

	internal static void LoadEntryPoints()
	{
		GL.LoadPlatformEntryPoints();
		if (GL.Viewport == null)
		{
			GL.Viewport = GL.LoadFunction<ViewportDelegate>("glViewport");
		}
		if (GL.Scissor == null)
		{
			GL.Scissor = GL.LoadFunction<ScissorDelegate>("glScissor");
		}
		if (GL.MakeCurrent == null)
		{
			GL.MakeCurrent = GL.LoadFunction<MakeCurrentDelegate>("glMakeCurrent");
		}
		GL.GetError = GL.LoadFunction<GetErrorDelegate>("glGetError");
		GL.TexParameterf = GL.LoadFunction<TexParameterFloatDelegate>("glTexParameterf");
		GL.TexParameterfv = GL.LoadFunction<TexParameterFloatArrayDelegate>("glTexParameterfv");
		GL.TexParameteri = GL.LoadFunction<TexParameterIntDelegate>("glTexParameteri");
		GL.EnableVertexAttribArray = GL.LoadFunction<EnableVertexAttribArrayDelegate>("glEnableVertexAttribArray");
		GL.DisableVertexAttribArray = GL.LoadFunction<DisableVertexAttribArrayDelegate>("glDisableVertexAttribArray");
		GL.GetIntegerv = GL.LoadFunction<GetIntegerDelegate>("glGetIntegerv");
		GL.GetStringInternal = GL.LoadFunction<GetStringDelegate>("glGetString");
		GL.ClearDepth = GL.LoadFunction<ClearDepthDelegate>("glClearDepth");
		if (GL.ClearDepth == null)
		{
			GL.ClearDepth = GL.LoadFunction<ClearDepthDelegate>("glClearDepthf");
		}
		GL.DepthRanged = GL.LoadFunction<DepthRangedDelegate>("glDepthRange");
		GL.DepthRangef = GL.LoadFunction<DepthRangefDelegate>("glDepthRangef");
		GL.Clear = GL.LoadFunction<ClearDelegate>("glClear");
		GL.ClearColor = GL.LoadFunction<ClearColorDelegate>("glClearColor");
		GL.ClearStencil = GL.LoadFunction<ClearStencilDelegate>("glClearStencil");
		GL.Flush = GL.LoadFunction<FlushDelegate>("glFlush");
		GL.GenTextures = GL.LoadFunction<GenTexturesDelegte>("glGenTextures");
		GL.BindTexture = GL.LoadFunction<BindTextureDelegate>("glBindTexture");
		GL.Enable = GL.LoadFunction<EnableDelegate>("glEnable");
		GL.Disable = GL.LoadFunction<DisableDelegate>("glDisable");
		GL.CullFace = GL.LoadFunction<CullFaceDelegate>("glCullFace");
		GL.FrontFace = GL.LoadFunction<FrontFaceDelegate>("glFrontFace");
		GL.PolygonMode = GL.LoadFunction<PolygonModeDelegate>("glPolygonMode");
		GL.PolygonOffset = GL.LoadFunction<PolygonOffsetDelegate>("glPolygonOffset");
		GL.BindBuffer = GL.LoadFunction<BindBufferDelegate>("glBindBuffer");
		GL.DrawBuffers = GL.LoadFunction<DrawBuffersDelegate>("glDrawBuffers");
		GL.DrawElements = GL.LoadFunction<DrawElementsDelegate>("glDrawElements");
		GL.DrawArrays = GL.LoadFunction<DrawArraysDelegate>("glDrawArrays");
		GL.Uniform1i = GL.LoadFunction<Uniform1iDelegate>("glUniform1i");
		GL.Uniform4fv = GL.LoadFunction<Uniform4fvDelegate>("glUniform4fv");
		GL.ReadPixelsInternal = GL.LoadFunction<ReadPixelsDelegate>("glReadPixels");
		GL.ReadBuffer = GL.LoadFunction<ReadBufferDelegate>("glReadBuffer");
		GL.DrawBuffer = GL.LoadFunction<DrawBufferDelegate>("glDrawBuffer");
		GL.GenRenderbuffers = GL.LoadFunction<GenRenderbuffersDelegate>("glGenRenderbuffers");
		GL.BindRenderbuffer = GL.LoadFunction<BindRenderbufferDelegate>("glBindRenderbuffer");
		GL.DeleteRenderbuffers = GL.LoadFunction<DeleteRenderbuffersDelegate>("glDeleteRenderbuffers");
		GL.GenFramebuffers = GL.LoadFunction<GenFramebuffersDelegate>("glGenFramebuffers");
		GL.BindFramebuffer = GL.LoadFunction<BindFramebufferDelegate>("glBindFramebuffer");
		GL.DeleteFramebuffers = GL.LoadFunction<DeleteFramebuffersDelegate>("glDeleteFramebuffers");
		GL.FramebufferTexture2D = GL.LoadFunction<FramebufferTexture2DDelegate>("glFramebufferTexture2D");
		GL.FramebufferRenderbuffer = GL.LoadFunction<FramebufferRenderbufferDelegate>("glFramebufferRenderbuffer");
		GL.RenderbufferStorage = GL.LoadFunction<RenderbufferStorageDelegate>("glRenderbufferStorage");
		GL.RenderbufferStorageMultisample = GL.LoadFunction<RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisample");
		GL.GenerateMipmap = GL.LoadFunction<GenerateMipmapDelegate>("glGenerateMipmap");
		GL.BlitFramebuffer = GL.LoadFunction<BlitFramebufferDelegate>("glBlitFramebuffer");
		GL.CheckFramebufferStatus = GL.LoadFunction<CheckFramebufferStatusDelegate>("glCheckFramebufferStatus");
		GL.GenQueries = GL.LoadFunction<GenQueriesDelegate>("glGenQueries");
		GL.BeginQuery = GL.LoadFunction<BeginQueryDelegate>("glBeginQuery");
		GL.EndQuery = GL.LoadFunction<EndQueryDelegate>("glEndQuery");
		GL.GetQueryObject = GL.LoadFunction<GetQueryObjectDelegate>("glGetQueryObjectuiv");
		if (GL.GetQueryObject == null)
		{
			GL.GetQueryObject = GL.LoadFunction<GetQueryObjectDelegate>("glGetQueryObjectivARB");
		}
		if (GL.GetQueryObject == null)
		{
			GL.GetQueryObject = GL.LoadFunction<GetQueryObjectDelegate>("glGetQueryObjectiv");
		}
		GL.DeleteQueries = GL.LoadFunction<DeleteQueriesDelegate>("glDeleteQueries");
		GL.ActiveTexture = GL.LoadFunction<ActiveTextureDelegate>("glActiveTexture");
		GL.CreateShader = GL.LoadFunction<CreateShaderDelegate>("glCreateShader");
		GL.ShaderSourceInternal = GL.LoadFunction<ShaderSourceDelegate>("glShaderSource");
		GL.CompileShader = GL.LoadFunction<CompileShaderDelegate>("glCompileShader");
		GL.GetShaderiv = GL.LoadFunction<GetShaderDelegate>("glGetShaderiv");
		GL.GetShaderInfoLogInternal = GL.LoadFunction<GetShaderInfoLogDelegate>("glGetShaderInfoLog");
		GL.IsShader = GL.LoadFunction<IsShaderDelegate>("glIsShader");
		GL.DeleteShader = GL.LoadFunction<DeleteShaderDelegate>("glDeleteShader");
		GL.GetAttribLocation = GL.LoadFunction<GetAttribLocationDelegate>("glGetAttribLocation");
		GL.GetUniformLocation = GL.LoadFunction<GetUniformLocationDelegate>("glGetUniformLocation");
		GL.IsProgram = GL.LoadFunction<IsProgramDelegate>("glIsProgram");
		GL.DeleteProgram = GL.LoadFunction<DeleteProgramDelegate>("glDeleteProgram");
		GL.CreateProgram = GL.LoadFunction<CreateProgramDelegate>("glCreateProgram");
		GL.AttachShader = GL.LoadFunction<AttachShaderDelegate>("glAttachShader");
		GL.UseProgram = GL.LoadFunction<UseProgramDelegate>("glUseProgram");
		GL.LinkProgram = GL.LoadFunction<LinkProgramDelegate>("glLinkProgram");
		GL.GetProgramiv = GL.LoadFunction<GetProgramDelegate>("glGetProgramiv");
		GL.GetProgramInfoLogInternal = GL.LoadFunction<GetProgramInfoLogDelegate>("glGetProgramInfoLog");
		GL.DetachShader = GL.LoadFunction<DetachShaderDelegate>("glDetachShader");
		GL.BlendColor = GL.LoadFunction<BlendColorDelegate>("glBlendColor");
		GL.BlendEquationSeparate = GL.LoadFunction<BlendEquationSeparateDelegate>("glBlendEquationSeparate");
		GL.BlendEquationSeparatei = GL.LoadFunction<BlendEquationSeparateiDelegate>("glBlendEquationSeparatei");
		GL.BlendFuncSeparate = GL.LoadFunction<BlendFuncSeparateDelegate>("glBlendFuncSeparate");
		GL.BlendFuncSeparatei = GL.LoadFunction<BlendFuncSeparateiDelegate>("glBlendFuncSeparatei");
		GL.ColorMask = GL.LoadFunction<ColorMaskDelegate>("glColorMask");
		GL.DepthFunc = GL.LoadFunction<DepthFuncDelegate>("glDepthFunc");
		GL.DepthMask = GL.LoadFunction<DepthMaskDelegate>("glDepthMask");
		GL.StencilFuncSeparate = GL.LoadFunction<StencilFuncSeparateDelegate>("glStencilFuncSeparate");
		GL.StencilOpSeparate = GL.LoadFunction<StencilOpSeparateDelegate>("glStencilOpSeparate");
		GL.StencilFunc = GL.LoadFunction<StencilFuncDelegate>("glStencilFunc");
		GL.StencilOp = GL.LoadFunction<StencilOpDelegate>("glStencilOp");
		GL.StencilMask = GL.LoadFunction<StencilMaskDelegate>("glStencilMask");
		GL.CompressedTexImage2D = GL.LoadFunction<CompressedTexImage2DDelegate>("glCompressedTexImage2D");
		GL.TexImage2D = GL.LoadFunction<TexImage2DDelegate>("glTexImage2D");
		GL.CompressedTexSubImage2D = GL.LoadFunction<CompressedTexSubImage2DDelegate>("glCompressedTexSubImage2D");
		GL.TexSubImage2D = GL.LoadFunction<TexSubImage2DDelegate>("glTexSubImage2D");
		GL.PixelStore = GL.LoadFunction<PixelStoreDelegate>("glPixelStorei");
		GL.Finish = GL.LoadFunction<FinishDelegate>("glFinish");
		GL.GetTexImageInternal = GL.LoadFunction<GetTexImageDelegate>("glGetTexImage");
		GL.GetCompressedTexImageInternal = GL.LoadFunction<GetCompressedTexImageDelegate>("glGetCompressedTexImage");
		GL.TexImage3D = GL.LoadFunction<TexImage3DDelegate>("glTexImage3D");
		GL.TexSubImage3D = GL.LoadFunction<TexSubImage3DDelegate>("glTexSubImage3D");
		GL.DeleteTextures = GL.LoadFunction<DeleteTexturesDelegate>("glDeleteTextures");
		GL.GenBuffers = GL.LoadFunction<GenBuffersDelegate>("glGenBuffers");
		GL.BufferData = GL.LoadFunction<BufferDataDelegate>("glBufferData");
		GL.MapBuffer = GL.LoadFunction<MapBufferDelegate>("glMapBuffer");
		GL.UnmapBuffer = GL.LoadFunction<UnmapBufferDelegate>("glUnmapBuffer");
		GL.BufferSubData = GL.LoadFunction<BufferSubDataDelegate>("glBufferSubData");
		GL.DeleteBuffers = GL.LoadFunction<DeleteBuffersDelegate>("glDeleteBuffers");
		GL.VertexAttribPointer = GL.LoadFunction<VertexAttribPointerDelegate>("glVertexAttribPointer");
		try
		{
			GL.DrawElementsInstanced = GL.LoadFunction<DrawElementsInstancedDelegate>("glDrawElementsInstanced");
			GL.VertexAttribDivisor = GL.LoadFunction<VertexAttribDivisorDelegate>("glVertexAttribDivisor");
			GL.DrawElementsInstancedBaseInstance = GL.LoadFunction<DrawElementsInstancedBaseInstanceDelegate>("glDrawElementsInstancedBaseInstance");
		}
		catch (EntryPointNotFoundException)
		{
		}
		if (GL.BoundApi == RenderApi.ES)
		{
			GL.InvalidateFramebuffer = GL.LoadFunction<InvalidateFramebufferDelegate>("glDiscardFramebufferEXT");
		}
		GL.LoadExtensions();
	}

	private static void LogExtensions()
	{
	}

	internal static void LoadExtensions()
	{
		if (GL.Extensions.Count == 0)
		{
			string extstring = GL.GetString(StringName.Extensions);
			ErrorCode error = GL.GetError();
			if (!string.IsNullOrEmpty(extstring) && error == ErrorCode.NoError)
			{
				GL.Extensions.AddRange(extstring.Split(' '));
			}
		}
		GL.LogExtensions();
		if (GL.GenRenderbuffers == null && GL.Extensions.Contains("GL_EXT_framebuffer_object"))
		{
			GL.LoadFrameBufferObjectEXTEntryPoints();
		}
		if (GL.RenderbufferStorageMultisample == null)
		{
			if (GL.Extensions.Contains("GL_APPLE_framebuffer_multisample"))
			{
				GL.RenderbufferStorageMultisample = GL.LoadFunction<RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisampleAPPLE");
				GL.BlitFramebuffer = GL.LoadFunction<BlitFramebufferDelegate>("glResolveMultisampleFramebufferAPPLE");
			}
			else if (GL.Extensions.Contains("GL_EXT_multisampled_render_to_texture"))
			{
				GL.RenderbufferStorageMultisample = GL.LoadFunction<RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisampleEXT");
				GL.FramebufferTexture2DMultiSample = GL.LoadFunction<FramebufferTexture2DMultiSampleDelegate>("glFramebufferTexture2DMultisampleEXT");
			}
			else if (GL.Extensions.Contains("GL_IMG_multisampled_render_to_texture"))
			{
				GL.RenderbufferStorageMultisample = GL.LoadFunction<RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisampleIMG");
				GL.FramebufferTexture2DMultiSample = GL.LoadFunction<FramebufferTexture2DMultiSampleDelegate>("glFramebufferTexture2DMultisampleIMG");
			}
			else if (GL.Extensions.Contains("GL_NV_framebuffer_multisample"))
			{
				GL.RenderbufferStorageMultisample = GL.LoadFunction<RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisampleNV");
				GL.BlitFramebuffer = GL.LoadFunction<BlitFramebufferDelegate>("glBlitFramebufferNV");
			}
		}
		if (GL.BlendFuncSeparatei == null && GL.Extensions.Contains("GL_ARB_draw_buffers_blend"))
		{
			GL.BlendFuncSeparatei = GL.LoadFunction<BlendFuncSeparateiDelegate>("BlendFuncSeparateiARB");
		}
		if (GL.BlendEquationSeparatei == null && GL.Extensions.Contains("GL_ARB_draw_buffers_blend"))
		{
			GL.BlendEquationSeparatei = GL.LoadFunction<BlendEquationSeparateiDelegate>("BlendEquationSeparateiARB");
		}
	}

	internal static void LoadFrameBufferObjectEXTEntryPoints()
	{
		GL.GenRenderbuffers = GL.LoadFunction<GenRenderbuffersDelegate>("glGenRenderbuffersEXT");
		GL.BindRenderbuffer = GL.LoadFunction<BindRenderbufferDelegate>("glBindRenderbufferEXT");
		GL.DeleteRenderbuffers = GL.LoadFunction<DeleteRenderbuffersDelegate>("glDeleteRenderbuffersEXT");
		GL.GenFramebuffers = GL.LoadFunction<GenFramebuffersDelegate>("glGenFramebuffersEXT");
		GL.BindFramebuffer = GL.LoadFunction<BindFramebufferDelegate>("glBindFramebufferEXT");
		GL.DeleteFramebuffers = GL.LoadFunction<DeleteFramebuffersDelegate>("glDeleteFramebuffersEXT");
		GL.FramebufferTexture2D = GL.LoadFunction<FramebufferTexture2DDelegate>("glFramebufferTexture2DEXT");
		GL.FramebufferRenderbuffer = GL.LoadFunction<FramebufferRenderbufferDelegate>("glFramebufferRenderbufferEXT");
		GL.RenderbufferStorage = GL.LoadFunction<RenderbufferStorageDelegate>("glRenderbufferStorageEXT");
		GL.RenderbufferStorageMultisample = GL.LoadFunction<RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisampleEXT");
		GL.GenerateMipmap = GL.LoadFunction<GenerateMipmapDelegate>("glGenerateMipmapEXT");
		GL.BlitFramebuffer = GL.LoadFunction<BlitFramebufferDelegate>("glBlitFramebufferEXT");
		GL.CheckFramebufferStatus = GL.LoadFunction<CheckFramebufferStatusDelegate>("glCheckFramebufferStatusEXT");
	}

	private static void LoadPlatformEntryPoints()
	{
		GL.BoundApi = RenderApi.GL;
	}

	internal static IGraphicsContext CreateContext(IWindowInfo info)
	{
		return GL.PlatformCreateContext(info);
	}

	internal static void DepthRange(float min, float max)
	{
		if (GL.BoundApi == RenderApi.ES)
		{
			GL.DepthRangef(min, max);
		}
		else
		{
			GL.DepthRanged(min, max);
		}
	}

	internal static void Uniform1(int location, int value)
	{
		GL.Uniform1i(location, value);
	}

	internal unsafe static void Uniform4(int location, int size, float* value)
	{
		GL.Uniform4fv(location, size, value);
	}

	internal static string GetString(StringName name)
	{
		return Marshal.PtrToStringAnsi(GL.GetStringInternal(name));
	}

	protected static IntPtr MarshalStringArrayToPtr(string[] strings)
	{
		IntPtr intPtr = IntPtr.Zero;
		if (strings != null && strings.Length != 0)
		{
			intPtr = Marshal.AllocHGlobal(strings.Length * IntPtr.Size);
			if (intPtr == IntPtr.Zero)
			{
				throw new OutOfMemoryException();
			}
			int i = 0;
			try
			{
				for (i = 0; i < strings.Length; i++)
				{
					IntPtr val = GL.MarshalStringToPtr(strings[i]);
					Marshal.WriteIntPtr(intPtr, i * IntPtr.Size, val);
				}
			}
			catch (OutOfMemoryException)
			{
				for (i--; i >= 0; i--)
				{
					Marshal.FreeHGlobal(Marshal.ReadIntPtr(intPtr, i * IntPtr.Size));
				}
				Marshal.FreeHGlobal(intPtr);
				throw;
			}
		}
		return intPtr;
	}

	protected unsafe static IntPtr MarshalStringToPtr(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return IntPtr.Zero;
		}
		int num = Encoding.ASCII.GetMaxByteCount(str.Length) + 1;
		IntPtr intPtr = Marshal.AllocHGlobal(num);
		if (intPtr == IntPtr.Zero)
		{
			throw new OutOfMemoryException();
		}
		fixed (char* chars = str + RuntimeHelpers.OffsetToStringData / 2)
		{
			int bytes = Encoding.ASCII.GetBytes(chars, str.Length, (byte*)(void*)intPtr, num);
			Marshal.WriteByte(intPtr, bytes, 0);
			return intPtr;
		}
	}

	protected static void FreeStringArrayPtr(IntPtr ptr, int length)
	{
		for (int i = 0; i < length; i++)
		{
			Marshal.FreeHGlobal(Marshal.ReadIntPtr(ptr, i * IntPtr.Size));
		}
		Marshal.FreeHGlobal(ptr);
	}

	internal static string GetProgramInfoLog(int programId)
	{
		int length = 0;
		GL.GetProgram(programId, GetProgramParameterName.LogLength, out length);
		StringBuilder sb = new StringBuilder(length, length);
		GL.GetProgramInfoLogInternal(programId, length, IntPtr.Zero, sb);
		return sb.ToString();
	}

	internal static string GetShaderInfoLog(int shaderId)
	{
		int length = 0;
		GL.GetShader(shaderId, ShaderParameter.LogLength, out length);
		StringBuilder sb = new StringBuilder(length, length);
		GL.GetShaderInfoLogInternal(shaderId, length, IntPtr.Zero, sb);
		return sb.ToString();
	}

	internal unsafe static void ShaderSource(int shaderId, string code)
	{
		int length = code.Length;
		IntPtr intPtr = GL.MarshalStringArrayToPtr(new string[1] { code });
		GL.ShaderSourceInternal(shaderId, 1, intPtr, &length);
		GL.FreeStringArrayPtr(intPtr, 1);
	}

	internal unsafe static void GetShader(int shaderId, ShaderParameter name, out int result)
	{
		fixed (int* ptr = &result)
		{
			GL.GetShaderiv(shaderId, (int)name, ptr);
		}
	}

	internal unsafe static void GetProgram(int programId, GetProgramParameterName name, out int result)
	{
		fixed (int* ptr = &result)
		{
			GL.GetProgramiv(programId, (int)name, ptr);
		}
	}

	internal unsafe static void GetInteger(GetPName name, out int value)
	{
		fixed (int* ptr = &value)
		{
			GL.GetIntegerv((int)name, ptr);
		}
	}

	internal unsafe static void GetInteger(int name, out int value)
	{
		fixed (int* ptr = &value)
		{
			GL.GetIntegerv(name, ptr);
		}
	}

	internal static void TexParameter(TextureTarget target, TextureParameterName name, float value)
	{
		GL.TexParameterf(target, name, value);
	}

	internal unsafe static void TexParameter(TextureTarget target, TextureParameterName name, float[] values)
	{
		fixed (float* ptr = &values[0])
		{
			GL.TexParameterfv(target, name, ptr);
		}
	}

	internal static void TexParameter(TextureTarget target, TextureParameterName name, int value)
	{
		GL.TexParameteri(target, name, value);
	}

	internal static void GetTexImage<T>(TextureTarget target, int level, PixelFormat format, PixelType type, T[] pixels) where T : struct
	{
		GCHandle pixelsPtr = GCHandle.Alloc(pixels, GCHandleType.Pinned);
		try
		{
			GL.GetTexImageInternal(target, level, format, type, pixelsPtr.AddrOfPinnedObject());
		}
		finally
		{
			pixelsPtr.Free();
		}
	}

	internal static void GetCompressedTexImage<T>(TextureTarget target, int level, T[] pixels) where T : struct
	{
		GCHandle pixelsPtr = GCHandle.Alloc(pixels, GCHandleType.Pinned);
		try
		{
			GL.GetCompressedTexImageInternal(target, level, pixelsPtr.AddrOfPinnedObject());
		}
		finally
		{
			pixelsPtr.Free();
		}
	}

	public static void ReadPixels<T>(int x, int y, int width, int height, PixelFormat format, PixelType type, T[] data)
	{
		GCHandle dataPtr = GCHandle.Alloc(data, GCHandleType.Pinned);
		try
		{
			GL.ReadPixelsInternal(x, y, width, height, format, type, dataPtr.AddrOfPinnedObject());
		}
		finally
		{
			dataPtr.Free();
		}
	}
}
