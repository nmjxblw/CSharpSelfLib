using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

public class GraphicsDevice : IDisposable
{
	internal class FramebufferHelper
	{
		private static FramebufferHelper _instance;

		private static readonly FramebufferAttachment[] FramebufferAttachements = new FramebufferAttachment[3]
		{
			FramebufferAttachment.ColorAttachment0,
			FramebufferAttachment.DepthAttachment,
			FramebufferAttachment.StencilAttachment
		};

		public bool SupportsInvalidateFramebuffer { get; private set; }

		public bool SupportsBlitFramebuffer { get; private set; }

		public static FramebufferHelper Create(GraphicsDevice gd)
		{
			if (gd.GraphicsCapabilities.SupportsFramebufferObjectARB || gd.GraphicsCapabilities.SupportsFramebufferObjectEXT)
			{
				FramebufferHelper._instance = new FramebufferHelper(gd);
				return FramebufferHelper._instance;
			}
			throw new PlatformNotSupportedException("MonoGame requires either ARB_framebuffer_object or EXT_framebuffer_object.Try updating your graphics drivers.");
		}

		public static FramebufferHelper Get()
		{
			if (FramebufferHelper._instance == null)
			{
				throw new InvalidOperationException("The FramebufferHelper has not been created yet!");
			}
			return FramebufferHelper._instance;
		}

		internal FramebufferHelper(GraphicsDevice graphicsDevice)
		{
			this.SupportsBlitFramebuffer = GL.BlitFramebuffer != null;
			this.SupportsInvalidateFramebuffer = GL.InvalidateFramebuffer != null;
		}

		internal virtual void GenRenderbuffer(out int renderbuffer)
		{
			GL.GenRenderbuffers(1, out renderbuffer);
		}

		internal virtual void BindRenderbuffer(int renderbuffer)
		{
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer);
		}

		internal virtual void DeleteRenderbuffer(int renderbuffer)
		{
			GL.DeleteRenderbuffers(1, ref renderbuffer);
		}

		internal virtual void RenderbufferStorageMultisample(int samples, int internalFormat, int width, int height)
		{
			if (samples > 0 && GL.RenderbufferStorageMultisample != null)
			{
				GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, samples, (RenderbufferStorage)internalFormat, width, height);
			}
			else
			{
				GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, (RenderbufferStorage)internalFormat, width, height);
			}
		}

		internal virtual void GenFramebuffer(out int framebuffer)
		{
			GL.GenFramebuffers(1, out framebuffer);
		}

		internal virtual void BindFramebuffer(int framebuffer)
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
		}

		internal virtual void BindReadFramebuffer(int readFramebuffer)
		{
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, readFramebuffer);
		}

		internal virtual void InvalidateDrawFramebuffer()
		{
			GL.InvalidateFramebuffer(FramebufferTarget.Framebuffer, 3, FramebufferHelper.FramebufferAttachements);
		}

		internal virtual void InvalidateReadFramebuffer()
		{
			GL.InvalidateFramebuffer(FramebufferTarget.Framebuffer, 3, FramebufferHelper.FramebufferAttachements);
		}

		internal virtual void DeleteFramebuffer(int framebuffer)
		{
			GL.DeleteFramebuffers(1, ref framebuffer);
		}

		internal virtual void FramebufferTexture2D(int attachement, int target, int texture, int level = 0, int samples = 0)
		{
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, (FramebufferAttachment)attachement, (TextureTarget)target, texture, level);
		}

		internal virtual void FramebufferRenderbuffer(int attachement, int renderbuffer, int level = 0)
		{
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, (FramebufferAttachment)attachement, RenderbufferTarget.Renderbuffer, renderbuffer);
		}

		internal virtual void GenerateMipmap(int target)
		{
			GL.GenerateMipmap((GenerateMipmapTarget)target);
		}

		internal virtual void BlitFramebuffer(int iColorAttachment, int width, int height)
		{
			GL.ReadBuffer((ReadBufferMode)(36064 + iColorAttachment));
			GL.DrawBuffer((DrawBufferMode)(36064 + iColorAttachment));
			GL.BlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
		}

		internal virtual void CheckFramebufferStatus()
		{
			FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
			if (status != FramebufferErrorCode.FramebufferComplete)
			{
				string message = "Framebuffer Incomplete.";
				switch (status)
				{
				case FramebufferErrorCode.FramebufferIncompleteAttachment:
					message = "Not all framebuffer attachment points are framebuffer attachment complete.";
					break;
				case FramebufferErrorCode.FramebufferIncompleteMissingAttachment:
					message = "No images are attached to the framebuffer.";
					break;
				case FramebufferErrorCode.FramebufferUnsupported:
					message = "The combination of internal formats of the attached images violates an implementation-dependent set of restrictions.";
					break;
				case FramebufferErrorCode.FramebufferIncompleteMultisample:
					message = "Not all attached images have the same number of samples.";
					break;
				}
				throw new InvalidOperationException(message);
			}
		}
	}

	private enum ResourceType
	{
		Texture,
		Buffer,
		Shader,
		Program,
		Query,
		Framebuffer
	}

	private struct ResourceHandle
	{
		public ResourceType type;

		public int handle;

		public static ResourceHandle Texture(int handle)
		{
			return new ResourceHandle
			{
				type = ResourceType.Texture,
				handle = handle
			};
		}

		public static ResourceHandle Buffer(int handle)
		{
			return new ResourceHandle
			{
				type = ResourceType.Buffer,
				handle = handle
			};
		}

		public static ResourceHandle Shader(int handle)
		{
			return new ResourceHandle
			{
				type = ResourceType.Shader,
				handle = handle
			};
		}

		public static ResourceHandle Program(int handle)
		{
			return new ResourceHandle
			{
				type = ResourceType.Program,
				handle = handle
			};
		}

		public static ResourceHandle Query(int handle)
		{
			return new ResourceHandle
			{
				type = ResourceType.Query,
				handle = handle
			};
		}

		public static ResourceHandle Framebuffer(int handle)
		{
			return new ResourceHandle
			{
				type = ResourceType.Framebuffer,
				handle = handle
			};
		}

		public void Free()
		{
			switch (this.type)
			{
			case ResourceType.Texture:
				GL.DeleteTextures(1, ref this.handle);
				break;
			case ResourceType.Buffer:
				GL.DeleteBuffers(1, ref this.handle);
				break;
			case ResourceType.Shader:
				if (GL.IsShader(this.handle))
				{
					GL.DeleteShader(this.handle);
				}
				break;
			case ResourceType.Program:
				if (GL.IsProgram(this.handle))
				{
					GL.DeleteProgram(this.handle);
				}
				break;
			case ResourceType.Query:
				GL.DeleteQueries(1, ref this.handle);
				break;
			case ResourceType.Framebuffer:
				GL.DeleteFramebuffers(1, ref this.handle);
				break;
			}
		}
	}

	private class RenderTargetBindingArrayComparer : IEqualityComparer<RenderTargetBinding[]>
	{
		public bool Equals(RenderTargetBinding[] first, RenderTargetBinding[] second)
		{
			if (first == second)
			{
				return true;
			}
			if (first == null || second == null)
			{
				return false;
			}
			if (first.Length != second.Length)
			{
				return false;
			}
			for (int i = 0; i < first.Length; i++)
			{
				if (first[i].RenderTarget != second[i].RenderTarget || first[i].ArraySlice != second[i].ArraySlice)
				{
					return false;
				}
			}
			return true;
		}

		public int GetHashCode(RenderTargetBinding[] array)
		{
			if (array != null)
			{
				int hash = 17;
				for (int i = 0; i < array.Length; i++)
				{
					RenderTargetBinding item = array[i];
					if (item.RenderTarget != null)
					{
						hash = hash * 23 + item.RenderTarget.GetHashCode();
					}
					hash = hash * 23 + item.ArraySlice.GetHashCode();
				}
				return hash;
			}
			return 0;
		}
	}

	private class BufferBindingInfo
	{
		public VertexDeclaration.VertexDeclarationAttributeInfo AttributeInfo;

		public IntPtr VertexOffset;

		public int InstanceFrequency;

		public int Vbo;

		public BufferBindingInfo(VertexDeclaration.VertexDeclarationAttributeInfo attributeInfo, IntPtr vertexOffset, int instanceFrequency, int vbo)
		{
			this.AttributeInfo = attributeInfo;
			this.VertexOffset = vertexOffset;
			this.InstanceFrequency = instanceFrequency;
			this.Vbo = vbo;
		}
	}

	private Viewport _viewport;

	private bool _isDisposed;

	private static Color _discardColor = new Color(0, 0, 0, 255);

	private Color _blendFactor = Color.White;

	private bool _blendFactorDirty;

	private BlendState _blendState;

	private BlendState _actualBlendState;

	private bool _blendStateDirty;

	private BlendState _blendStateAdditive;

	private BlendState _blendStateAlphaBlend;

	private BlendState _blendStateNonPremultiplied;

	private BlendState _blendStateOpaque;

	private DepthStencilState _depthStencilState;

	private DepthStencilState _actualDepthStencilState;

	private bool _depthStencilStateDirty;

	private DepthStencilState _depthStencilStateDefault;

	private DepthStencilState _depthStencilStateDepthRead;

	private DepthStencilState _depthStencilStateNone;

	private RasterizerState _rasterizerState;

	private RasterizerState _actualRasterizerState;

	private bool _rasterizerStateDirty;

	private RasterizerState _rasterizerStateCullClockwise;

	private RasterizerState _rasterizerStateCullCounterClockwise;

	private RasterizerState _rasterizerStateCullNone;

	private Rectangle _scissorRectangle;

	private bool _scissorRectangleDirty;

	private VertexBufferBindings _vertexBuffers;

	private bool _vertexBuffersDirty;

	private IndexBuffer _indexBuffer;

	private bool _indexBufferDirty;

	private readonly RenderTargetBinding[] _currentRenderTargetBindings = new RenderTargetBinding[4];

	private int _currentRenderTargetCount;

	private readonly RenderTargetBinding[] _tempRenderTargetBinding = new RenderTargetBinding[1];

	/// <summary>
	/// The active vertex shader.
	/// </summary>
	private Shader _vertexShader;

	private bool _vertexShaderDirty;

	/// <summary>
	/// The active pixel shader.
	/// </summary>
	private Shader _pixelShader;

	private bool _pixelShaderDirty;

	private readonly ConstantBufferCollection _vertexConstantBuffers = new ConstantBufferCollection(ShaderStage.Vertex, 16);

	private readonly ConstantBufferCollection _pixelConstantBuffers = new ConstantBufferCollection(ShaderStage.Pixel, 16);

	/// <summary>
	/// The cache of effects from unique byte streams.
	/// </summary>
	internal Dictionary<int, Effect> EffectCache;

	private readonly object _resourcesLock = new object();

	private readonly List<WeakReference> _resources = new List<WeakReference>();

	private int _maxVertexBufferSlots;

	internal int MaxTextureSlots;

	internal int MaxVertexTextureSlots;

	internal GraphicsMetrics _graphicsMetrics;

	private GraphicsDebug _graphicsDebug;

	private readonly GraphicsProfile _graphicsProfile;

	private DrawBuffersEnum[] _drawBuffers;

	private List<ResourceHandle> _disposeThisFrame = new List<ResourceHandle>();

	private List<ResourceHandle> _disposeNextFrame = new List<ResourceHandle>();

	private object _disposeActionsLock = new object();

	private static List<IntPtr> _disposeContexts = new List<IntPtr>();

	private static object _disposeContextsLock = new object();

	private ShaderProgramCache _programCache;

	private ShaderProgram _shaderProgram;

	private static readonly float[] _posFixup = new float[4];

	private static BufferBindingInfo[] _bufferBindingInfos;

	private static int _activeBufferBindingInfosCount;

	private static bool[] _newEnabledVertexAttributes;

	internal static readonly List<int> _enabledVertexAttributes = new List<int>();

	internal static bool _attribsDirty;

	internal FramebufferHelper framebufferHelper;

	internal int glMajorVersion;

	internal int glMinorVersion;

	internal int glFramebuffer;

	internal int MaxVertexAttributes;

	internal int _maxTextureSize;

	internal bool _lastBlendEnable;

	internal BlendState _lastBlendState = new BlendState();

	internal DepthStencilState _lastDepthStencilState = new DepthStencilState();

	internal RasterizerState _lastRasterizerState = new RasterizerState();

	private Vector4 _lastClearColor = Vector4.Zero;

	private float _lastClearDepth = 1f;

	private int _lastClearStencil;

	private DepthStencilState clearDepthStencilState = new DepthStencilState
	{
		StencilEnable = true
	};

	private Dictionary<RenderTargetBinding[], int> glFramebuffers = new Dictionary<RenderTargetBinding[], int>(new RenderTargetBindingArrayComparer());

	private Dictionary<RenderTargetBinding[], int> glResolveFramebuffers = new Dictionary<RenderTargetBinding[], int>(new RenderTargetBindingArrayComparer());

	/// <summary>
	/// Indicates if DX9 style pixel addressing or current standard
	/// pixel addressing should be used. This flag is set to
	/// <c>false</c> by default. If `UseHalfPixelOffset` is
	/// `true` you have to add half-pixel offset to a Projection matrix.
	/// See also <see cref="P:Microsoft.Xna.Framework.GraphicsDeviceManager.PreferHalfPixelOffset" />.
	/// </summary>
	/// <remarks>
	/// XNA uses DirectX9 for its graphics. DirectX9 interprets UV
	/// coordinates differently from other graphics API's. This is
	/// typically referred to as the half-pixel offset. MonoGame
	/// replicates XNA behavior if this flag is set to <c>true</c>.
	/// </remarks>
	public bool UseHalfPixelOffset { get; private set; }

	internal GraphicsCapabilities GraphicsCapabilities { get; private set; }

	public TextureCollection VertexTextures { get; private set; }

	public SamplerStateCollection VertexSamplerStates { get; private set; }

	public TextureCollection Textures { get; private set; }

	public SamplerStateCollection SamplerStates { get; private set; }

	/// <summary>
	/// Get or set the color a <see cref="T:Microsoft.Xna.Framework.Graphics.RenderTarget2D" /> is cleared to when it is set.
	/// </summary>
	public static Color DiscardColor
	{
		get
		{
			return GraphicsDevice._discardColor;
		}
		set
		{
			GraphicsDevice._discardColor = value;
		}
	}

	private bool VertexShaderDirty => this._vertexShaderDirty;

	private bool PixelShaderDirty => this._pixelShaderDirty;

	public bool IsDisposed => this._isDisposed;

	public bool IsContentLost => this.IsDisposed;

	internal bool IsRenderTargetBound => this._currentRenderTargetCount > 0;

	internal DepthFormat ActiveDepthFormat
	{
		get
		{
			if (!this.IsRenderTargetBound)
			{
				return this.PresentationParameters.DepthStencilFormat;
			}
			return this._currentRenderTargetBindings[0].DepthFormat;
		}
	}

	public GraphicsAdapter Adapter { get; private set; }

	/// <summary>
	/// The rendering information for debugging and profiling.
	/// The metrics are reset every frame after draw within <see cref="M:Microsoft.Xna.Framework.Graphics.GraphicsDevice.Present" />. 
	/// </summary>
	public GraphicsMetrics Metrics
	{
		get
		{
			return this._graphicsMetrics;
		}
		set
		{
			this._graphicsMetrics = value;
		}
	}

	/// <summary>
	/// Access debugging APIs for the graphics subsystem.
	/// </summary>
	public GraphicsDebug GraphicsDebug
	{
		get
		{
			return this._graphicsDebug;
		}
		set
		{
			this._graphicsDebug = value;
		}
	}

	public RasterizerState RasterizerState
	{
		get
		{
			return this._rasterizerState;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (this._rasterizerState != value)
			{
				if (!value.DepthClipEnable && !this.GraphicsCapabilities.SupportsDepthClamp)
				{
					throw new InvalidOperationException("Cannot set RasterizerState.DepthClipEnable to false on this graphics device");
				}
				this._rasterizerState = value;
				RasterizerState newRasterizerState = this._rasterizerState;
				if (this._rasterizerState == RasterizerState.CullClockwise)
				{
					newRasterizerState = this._rasterizerStateCullClockwise;
				}
				else if (this._rasterizerState == RasterizerState.CullCounterClockwise)
				{
					newRasterizerState = this._rasterizerStateCullCounterClockwise;
				}
				else if (this._rasterizerState == RasterizerState.CullNone)
				{
					newRasterizerState = this._rasterizerStateCullNone;
				}
				newRasterizerState.BindToGraphicsDevice(this);
				this._actualRasterizerState = newRasterizerState;
				this._rasterizerStateDirty = true;
			}
		}
	}

	/// <summary>
	/// The color used as blend factor when alpha blending.
	/// </summary>
	/// <remarks>
	/// When only changing BlendFactor, use this rather than <see cref="P:Microsoft.Xna.Framework.Graphics.BlendState.BlendFactor" /> to
	/// only update BlendFactor so the whole BlendState does not have to be updated.
	/// </remarks>
	public Color BlendFactor
	{
		get
		{
			return this._blendFactor;
		}
		set
		{
			if (!(this._blendFactor == value))
			{
				this._blendFactor = value;
				this._blendFactorDirty = true;
			}
		}
	}

	public BlendState BlendState
	{
		get
		{
			return this._blendState;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (this._blendState != value)
			{
				this._blendState = value;
				BlendState newBlendState = this._blendState;
				if (this._blendState == BlendState.Additive)
				{
					newBlendState = this._blendStateAdditive;
				}
				else if (this._blendState == BlendState.AlphaBlend)
				{
					newBlendState = this._blendStateAlphaBlend;
				}
				else if (this._blendState == BlendState.NonPremultiplied)
				{
					newBlendState = this._blendStateNonPremultiplied;
				}
				else if (this._blendState == BlendState.Opaque)
				{
					newBlendState = this._blendStateOpaque;
				}
				if (newBlendState.IndependentBlendEnable && !this.GraphicsCapabilities.SupportsSeparateBlendStates)
				{
					throw new PlatformNotSupportedException("Independent blend states requires at least OpenGL 4.0 or GL_ARB_draw_buffers_blend. Try upgrading your graphics drivers.");
				}
				newBlendState.BindToGraphicsDevice(this);
				this._actualBlendState = newBlendState;
				this.BlendFactor = this._actualBlendState.BlendFactor;
				this._blendStateDirty = true;
			}
		}
	}

	public DepthStencilState DepthStencilState
	{
		get
		{
			return this._depthStencilState;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (this._depthStencilState != value)
			{
				this._depthStencilState = value;
				DepthStencilState newDepthStencilState = this._depthStencilState;
				if (this._depthStencilState == DepthStencilState.Default)
				{
					newDepthStencilState = this._depthStencilStateDefault;
				}
				else if (this._depthStencilState == DepthStencilState.DepthRead)
				{
					newDepthStencilState = this._depthStencilStateDepthRead;
				}
				else if (this._depthStencilState == DepthStencilState.None)
				{
					newDepthStencilState = this._depthStencilStateNone;
				}
				newDepthStencilState.BindToGraphicsDevice(this);
				this._actualDepthStencilState = newDepthStencilState;
				this._depthStencilStateDirty = true;
			}
		}
	}

	public DisplayMode DisplayMode => this.Adapter.CurrentDisplayMode;

	public GraphicsDeviceStatus GraphicsDeviceStatus => GraphicsDeviceStatus.Normal;

	public PresentationParameters PresentationParameters { get; private set; }

	public Viewport Viewport
	{
		get
		{
			return this._viewport;
		}
		set
		{
			this._viewport = value;
			this.PlatformSetViewport(ref value);
		}
	}

	public GraphicsProfile GraphicsProfile => this._graphicsProfile;

	public Rectangle ScissorRectangle
	{
		get
		{
			return this._scissorRectangle;
		}
		set
		{
			if (!(this._scissorRectangle == value))
			{
				this._scissorRectangle = value;
				this._scissorRectangleDirty = true;
			}
		}
	}

	public int RenderTargetCount => this._currentRenderTargetCount;

	public IndexBuffer Indices
	{
		get
		{
			return this._indexBuffer;
		}
		set
		{
			this.SetIndexBuffer(value);
		}
	}

	internal Shader VertexShader
	{
		get
		{
			return this._vertexShader;
		}
		set
		{
			if (this._vertexShader != value)
			{
				this._vertexShader = value;
				this._vertexConstantBuffers.Clear();
				this._vertexShaderDirty = true;
			}
		}
	}

	internal Shader PixelShader
	{
		get
		{
			return this._pixelShader;
		}
		set
		{
			if (this._pixelShader != value)
			{
				this._pixelShader = value;
				this._pixelConstantBuffers.Clear();
				this._pixelShaderDirty = true;
			}
		}
	}

	public bool ResourcesLost { get; set; }

	internal IGraphicsContext Context { get; private set; }

	private int ShaderProgramHash
	{
		get
		{
			if (this._vertexShader == null && this._pixelShader == null)
			{
				throw new InvalidOperationException("There is no shader bound!");
			}
			if (this._vertexShader == null)
			{
				return this._pixelShader.HashKey;
			}
			if (this._pixelShader == null)
			{
				return this._vertexShader.HashKey;
			}
			return this._vertexShader.HashKey ^ this._pixelShader.HashKey;
		}
	}

	public event EventHandler<EventArgs> DeviceLost;

	public event EventHandler<EventArgs> DeviceReset;

	public event EventHandler<EventArgs> DeviceResetting;

	public event EventHandler<ResourceCreatedEventArgs> ResourceCreated;

	public event EventHandler<ResourceDestroyedEventArgs> ResourceDestroyed;

	public event EventHandler<EventArgs> Disposing;

	internal event EventHandler<PresentationEventArgs> PresentationChanged;

	internal GraphicsDevice()
	{
		this.PresentationParameters = new PresentationParameters();
		this.PresentationParameters.DepthStencilFormat = DepthFormat.Depth24;
		this.Setup();
		this.GraphicsCapabilities = new GraphicsCapabilities();
		this.GraphicsCapabilities.Initialize(this);
		this.Initialize();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" /> class.
	/// </summary>
	/// <param name="adapter">The graphics adapter.</param>
	/// <param name="graphicsProfile">The graphics profile.</param>
	/// <param name="presentationParameters">The presentation options.</param>
	/// <exception cref="T:System.ArgumentNullException">
	/// <paramref name="presentationParameters" /> is <see langword="null" />.
	/// </exception>
	public GraphicsDevice(GraphicsAdapter adapter, GraphicsProfile graphicsProfile, PresentationParameters presentationParameters)
	{
		if (adapter == null)
		{
			throw new ArgumentNullException("adapter");
		}
		if (!adapter.IsProfileSupported(graphicsProfile))
		{
			throw new NoSuitableGraphicsDeviceException($"Adapter '{adapter.Description}' does not support the {graphicsProfile} profile.");
		}
		if (presentationParameters == null)
		{
			throw new ArgumentNullException("presentationParameters");
		}
		this.Adapter = adapter;
		this.PresentationParameters = presentationParameters;
		this._graphicsProfile = graphicsProfile;
		this.Setup();
		this.GraphicsCapabilities = new GraphicsCapabilities();
		this.GraphicsCapabilities.Initialize(this);
		this.Initialize();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" /> class.
	/// </summary>
	/// <param name="adapter">The graphics adapter.</param>
	/// <param name="graphicsProfile">The graphics profile.</param>
	/// <param name="preferHalfPixelOffset"> Indicates if DX9 style pixel addressing or current standard pixel addressing should be used. This value is passed to <see cref="P:Microsoft.Xna.Framework.Graphics.GraphicsDevice.UseHalfPixelOffset" /></param>
	/// <param name="presentationParameters">The presentation options.</param>
	/// <exception cref="T:System.ArgumentNullException">
	/// <paramref name="presentationParameters" /> is <see langword="null" />.
	/// </exception>
	public GraphicsDevice(GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
	{
		if (adapter == null)
		{
			throw new ArgumentNullException("adapter");
		}
		if (!adapter.IsProfileSupported(graphicsProfile))
		{
			throw new NoSuitableGraphicsDeviceException($"Adapter '{adapter.Description}' does not support the {graphicsProfile} profile.");
		}
		if (presentationParameters == null)
		{
			throw new ArgumentNullException("presentationParameters");
		}
		this.Adapter = adapter;
		this._graphicsProfile = graphicsProfile;
		this.UseHalfPixelOffset = preferHalfPixelOffset;
		this.PresentationParameters = presentationParameters;
		this.Setup();
		this.GraphicsCapabilities = new GraphicsCapabilities();
		this.GraphicsCapabilities.Initialize(this);
		this.Initialize();
	}

	private void Setup()
	{
		this._viewport = new Viewport(0, 0, this.DisplayMode.Width, this.DisplayMode.Height);
		this._viewport.MaxDepth = 1f;
		this.PlatformSetup();
		this.VertexTextures = new TextureCollection(this, this.MaxVertexTextureSlots, applyToVertexStage: true);
		this.VertexSamplerStates = new SamplerStateCollection(this, this.MaxVertexTextureSlots, applyToVertexStage: true);
		this.Textures = new TextureCollection(this, this.MaxTextureSlots, applyToVertexStage: false);
		this.SamplerStates = new SamplerStateCollection(this, this.MaxTextureSlots, applyToVertexStage: false);
		this._blendStateAdditive = BlendState.Additive.Clone();
		this._blendStateAlphaBlend = BlendState.AlphaBlend.Clone();
		this._blendStateNonPremultiplied = BlendState.NonPremultiplied.Clone();
		this._blendStateOpaque = BlendState.Opaque.Clone();
		this.BlendState = BlendState.Opaque;
		this._depthStencilStateDefault = DepthStencilState.Default.Clone();
		this._depthStencilStateDepthRead = DepthStencilState.DepthRead.Clone();
		this._depthStencilStateNone = DepthStencilState.None.Clone();
		this.DepthStencilState = DepthStencilState.Default;
		this._rasterizerStateCullClockwise = RasterizerState.CullClockwise.Clone();
		this._rasterizerStateCullCounterClockwise = RasterizerState.CullCounterClockwise.Clone();
		this._rasterizerStateCullNone = RasterizerState.CullNone.Clone();
		this.RasterizerState = RasterizerState.CullCounterClockwise;
		this.EffectCache = new Dictionary<int, Effect>();
	}

	~GraphicsDevice()
	{
		this.Dispose(disposing: false);
	}

	internal int GetClampedMultisampleCount(int multiSampleCount)
	{
		if (multiSampleCount > 1)
		{
			int msc = multiSampleCount;
			msc |= msc >> 1;
			msc |= msc >> 2;
			msc |= msc >> 4;
			msc -= msc >> 1;
			if (msc > this.GraphicsCapabilities.MaxMultiSampleCount)
			{
				msc = this.GraphicsCapabilities.MaxMultiSampleCount;
			}
			return msc;
		}
		return 0;
	}

	internal void Initialize()
	{
		this.PlatformInitialize();
		this._blendStateDirty = (this._depthStencilStateDirty = (this._rasterizerStateDirty = true));
		this.BlendState = BlendState.Opaque;
		this.DepthStencilState = DepthStencilState.Default;
		this.RasterizerState = RasterizerState.CullCounterClockwise;
		this.VertexTextures.Clear();
		this.VertexSamplerStates.Clear();
		this.Textures.Clear();
		this.SamplerStates.Clear();
		this._vertexConstantBuffers.Clear();
		this._pixelConstantBuffers.Clear();
		this._vertexBuffers = new VertexBufferBindings(this._maxVertexBufferSlots);
		this._vertexBuffersDirty = true;
		this._indexBufferDirty = true;
		this._vertexShaderDirty = true;
		this._pixelShaderDirty = true;
		this._scissorRectangleDirty = true;
		this.ScissorRectangle = this._viewport.Bounds;
		this.ApplyRenderTargets(null);
	}

	internal void ApplyState(bool applyShaders)
	{
		this.PlatformBeginApplyState();
		this.PlatformApplyBlend();
		if (this._depthStencilStateDirty)
		{
			this._actualDepthStencilState.PlatformApplyState(this);
			this._depthStencilStateDirty = false;
		}
		if (this._rasterizerStateDirty)
		{
			this._actualRasterizerState.PlatformApplyState(this);
			this._rasterizerStateDirty = false;
		}
		this.PlatformApplyState(applyShaders);
	}

	public void Clear(Color color)
	{
		ClearOptions options = ClearOptions.Target;
		options |= ClearOptions.DepthBuffer;
		options |= ClearOptions.Stencil;
		this.PlatformClear(options, color.ToVector4(), this._viewport.MaxDepth, 0);
		this._graphicsMetrics._clearCount++;
	}

	public void Clear(ClearOptions options, Color color, float depth, int stencil)
	{
		this.PlatformClear(options, color.ToVector4(), depth, stencil);
		this._graphicsMetrics._clearCount++;
	}

	public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
	{
		this.PlatformClear(options, color, depth, stencil);
		this._graphicsMetrics._clearCount++;
	}

	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (this._isDisposed)
		{
			return;
		}
		if (disposing)
		{
			lock (this._resourcesLock)
			{
				WeakReference[] array = this._resources.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].Target is IDisposable target)
					{
						target.Dispose();
					}
				}
				this._resources.Clear();
			}
			this.EffectCache.Clear();
			this._blendState = null;
			this._actualBlendState = null;
			this._blendStateAdditive.Dispose();
			this._blendStateAlphaBlend.Dispose();
			this._blendStateNonPremultiplied.Dispose();
			this._blendStateOpaque.Dispose();
			this._depthStencilState = null;
			this._actualDepthStencilState = null;
			this._depthStencilStateDefault.Dispose();
			this._depthStencilStateDepthRead.Dispose();
			this._depthStencilStateNone.Dispose();
			this._rasterizerState = null;
			this._actualRasterizerState = null;
			this._rasterizerStateCullClockwise.Dispose();
			this._rasterizerStateCullCounterClockwise.Dispose();
			this._rasterizerStateCullNone.Dispose();
			this.PlatformDispose();
		}
		this._isDisposed = true;
		EventHelpers.Raise(this, this.Disposing, EventArgs.Empty);
	}

	internal void AddResourceReference(WeakReference resourceReference)
	{
		lock (this._resourcesLock)
		{
			this._resources.Add(resourceReference);
		}
	}

	internal void RemoveResourceReference(WeakReference resourceReference)
	{
		lock (this._resourcesLock)
		{
			this._resources.Remove(resourceReference);
		}
	}

	public void Present()
	{
		if (this._currentRenderTargetCount != 0)
		{
			throw new InvalidOperationException("Cannot call Present when a render target is active.");
		}
		this._graphicsMetrics = default(GraphicsMetrics);
		this.PlatformPresent();
	}

	public void Reset()
	{
		EventHelpers.Raise(this, this.DeviceResetting, EventArgs.Empty);
		this.OnPresentationChanged();
		EventHelpers.Raise(this, this.PresentationChanged, new PresentationEventArgs(this.PresentationParameters));
		EventHelpers.Raise(this, this.DeviceReset, EventArgs.Empty);
	}

	public void Reset(PresentationParameters presentationParameters)
	{
		if (presentationParameters == null)
		{
			throw new ArgumentNullException("presentationParameters");
		}
		this.PresentationParameters = presentationParameters;
		this.Reset();
	}

	/// <summary>
	/// Trigger the DeviceResetting event
	/// Currently internal to allow the various platforms to send the event at the appropriate time.
	/// </summary>
	internal void OnDeviceResetting()
	{
		EventHelpers.Raise(this, this.DeviceResetting, EventArgs.Empty);
		lock (this._resourcesLock)
		{
			foreach (WeakReference resource in this._resources)
			{
				if (resource.Target is GraphicsResource target)
				{
					target.GraphicsDeviceResetting();
				}
			}
			this._resources.RemoveAll((WeakReference wr) => !wr.IsAlive);
		}
	}

	/// <summary>
	/// Trigger the DeviceReset event to allow games to be notified of a device reset.
	/// Currently internal to allow the various platforms to send the event at the appropriate time.
	/// </summary>
	internal void OnDeviceReset()
	{
		EventHelpers.Raise(this, this.DeviceReset, EventArgs.Empty);
	}

	public void SetRenderTarget(RenderTarget2D renderTarget)
	{
		if (renderTarget == null)
		{
			this.SetRenderTargets((RenderTargetBinding[])null);
			return;
		}
		this._tempRenderTargetBinding[0] = new RenderTargetBinding(renderTarget);
		this.SetRenderTargets(this._tempRenderTargetBinding);
	}

	public void SetRenderTarget(RenderTargetCube renderTarget, CubeMapFace cubeMapFace)
	{
		if (renderTarget == null)
		{
			this.SetRenderTargets((RenderTargetBinding[])null);
			return;
		}
		this._tempRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, cubeMapFace);
		this.SetRenderTargets(this._tempRenderTargetBinding);
	}

	public void SetRenderTargets(params RenderTargetBinding[] renderTargets)
	{
		int renderTargetCount = 0;
		if (renderTargets != null)
		{
			renderTargetCount = renderTargets.Length;
			if (renderTargetCount == 0)
			{
				renderTargets = null;
			}
		}
		if (this._currentRenderTargetCount == renderTargetCount)
		{
			bool isEqual = true;
			for (int i = 0; i < this._currentRenderTargetCount; i++)
			{
				if (this._currentRenderTargetBindings[i].RenderTarget != renderTargets[i].RenderTarget || this._currentRenderTargetBindings[i].ArraySlice != renderTargets[i].ArraySlice)
				{
					isEqual = false;
					break;
				}
			}
			if (isEqual)
			{
				return;
			}
		}
		this.ApplyRenderTargets(renderTargets);
		if (renderTargetCount == 0)
		{
			this._graphicsMetrics._targetCount++;
		}
		else
		{
			this._graphicsMetrics._targetCount += renderTargetCount;
		}
	}

	internal void ApplyRenderTargets(RenderTargetBinding[] renderTargets)
	{
		bool clearTarget = false;
		this.PlatformResolveRenderTargets();
		Array.Clear(this._currentRenderTargetBindings, 0, this._currentRenderTargetBindings.Length);
		int renderTargetWidth;
		int renderTargetHeight;
		if (renderTargets == null)
		{
			this._currentRenderTargetCount = 0;
			this.PlatformApplyDefaultRenderTarget();
			clearTarget = this.PresentationParameters.RenderTargetUsage == RenderTargetUsage.DiscardContents;
			renderTargetWidth = this.PresentationParameters.BackBufferWidth;
			renderTargetHeight = this.PresentationParameters.BackBufferHeight;
		}
		else
		{
			Array.Copy(renderTargets, this._currentRenderTargetBindings, renderTargets.Length);
			this._currentRenderTargetCount = renderTargets.Length;
			IRenderTarget renderTarget = this.PlatformApplyRenderTargets();
			clearTarget = renderTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents;
			renderTargetWidth = renderTarget.Width;
			renderTargetHeight = renderTarget.Height;
		}
		this.Viewport = new Viewport(0, 0, renderTargetWidth, renderTargetHeight);
		this.ScissorRectangle = new Rectangle(0, 0, renderTargetWidth, renderTargetHeight);
		if (clearTarget)
		{
			this.Clear(GraphicsDevice.DiscardColor);
		}
	}

	public RenderTargetBinding[] GetRenderTargets()
	{
		RenderTargetBinding[] bindings = new RenderTargetBinding[this._currentRenderTargetCount];
		Array.Copy(this._currentRenderTargetBindings, bindings, this._currentRenderTargetCount);
		return bindings;
	}

	public void GetRenderTargets(RenderTargetBinding[] outTargets)
	{
		Array.Copy(this._currentRenderTargetBindings, outTargets, this._currentRenderTargetCount);
	}

	public void SetVertexBuffer(VertexBuffer vertexBuffer)
	{
		this._vertexBuffersDirty |= ((vertexBuffer == null) ? this._vertexBuffers.Clear() : this._vertexBuffers.Set(vertexBuffer, 0));
	}

	public void SetVertexBuffer(VertexBuffer vertexBuffer, int vertexOffset)
	{
		if (vertexOffset < 0 || (vertexBuffer == null && vertexOffset != 0) || (vertexBuffer != null && vertexOffset >= vertexBuffer.VertexCount))
		{
			throw new ArgumentOutOfRangeException("vertexOffset");
		}
		this._vertexBuffersDirty |= ((vertexBuffer == null) ? this._vertexBuffers.Clear() : this._vertexBuffers.Set(vertexBuffer, vertexOffset));
	}

	public void SetVertexBuffers(params VertexBufferBinding[] vertexBuffers)
	{
		if (vertexBuffers == null || vertexBuffers.Length == 0)
		{
			this._vertexBuffersDirty |= this._vertexBuffers.Clear();
			return;
		}
		if (vertexBuffers.Length > this._maxVertexBufferSlots)
		{
			string message = string.Format(CultureInfo.InvariantCulture, "Max number of vertex buffers is {0}.", this._maxVertexBufferSlots);
			throw new ArgumentOutOfRangeException("vertexBuffers", message);
		}
		this._vertexBuffersDirty |= this._vertexBuffers.Set(vertexBuffers);
	}

	private void SetIndexBuffer(IndexBuffer indexBuffer)
	{
		if (this._indexBuffer != indexBuffer)
		{
			this._indexBuffer = indexBuffer;
			this._indexBufferDirty = true;
		}
	}

	internal void SetConstantBuffer(ShaderStage stage, int slot, ConstantBuffer buffer)
	{
		if (stage == ShaderStage.Vertex)
		{
			this._vertexConstantBuffers[slot] = buffer;
		}
		else
		{
			this._pixelConstantBuffers[slot] = buffer;
		}
	}

	/// <summary>
	/// Draw geometry by indexing into the vertex buffer.
	/// </summary>
	/// <param name="primitiveType">The type of primitives in the index buffer.</param>
	/// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
	/// <param name="minVertexIndex">This is unused and remains here only for XNA API compatibility.</param>
	/// <param name="numVertices">This is unused and remains here only for XNA API compatibility.</param>
	/// <param name="startIndex">The index within the index buffer to start drawing from.</param>
	/// <param name="primitiveCount">The number of primitives to render from the index buffer.</param>
	/// <remarks>Note that minVertexIndex and numVertices are unused in MonoGame and will be ignored.</remarks>
	[Obsolete("Use DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount) instead. In future versions this method can be removed.")]
	public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount)
	{
		this.DrawIndexedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount);
	}

	/// <summary>
	/// Draw geometry by indexing into the vertex buffer.
	/// </summary>
	/// <param name="primitiveType">The type of primitives in the index buffer.</param>
	/// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
	/// <param name="startIndex">The index within the index buffer to start drawing from.</param>
	/// <param name="primitiveCount">The number of primitives to render from the index buffer.</param>
	public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
	{
		if (this._vertexShader == null)
		{
			throw new InvalidOperationException("Vertex shader must be set before calling DrawIndexedPrimitives.");
		}
		if (this._vertexBuffers.Count == 0)
		{
			throw new InvalidOperationException("Vertex buffer must be set before calling DrawIndexedPrimitives.");
		}
		if (this._indexBuffer == null)
		{
			throw new InvalidOperationException("Index buffer must be set before calling DrawIndexedPrimitives.");
		}
		if (primitiveCount <= 0)
		{
			throw new ArgumentOutOfRangeException("primitiveCount");
		}
		this.PlatformDrawIndexedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount);
		this._graphicsMetrics._drawCount++;
		this._graphicsMetrics._primitiveCount += primitiveCount;
	}

	/// <summary>
	/// Draw primitives of the specified type from the data in an array of vertices without indexing.
	/// </summary>
	/// <typeparam name="T">The type of the vertices.</typeparam>
	/// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
	/// <param name="vertexData">An array of vertices to draw.</param>
	/// <param name="vertexOffset">The index in the array of the first vertex that should be rendered.</param>
	/// <param name="primitiveCount">The number of primitives to draw.</param>
	/// <remarks>The <see cref="T:Microsoft.Xna.Framework.Graphics.VertexDeclaration" /> will be found by getting <see cref="P:Microsoft.Xna.Framework.Graphics.IVertexType.VertexDeclaration" />
	/// from an instance of <typeparamref name="T" /> and cached for subsequent calls.</remarks>
	public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount) where T : struct, IVertexType
	{
		this.DrawUserPrimitives(primitiveType, vertexData, vertexOffset, primitiveCount, VertexDeclarationCache<T>.VertexDeclaration);
	}

	/// <summary>
	/// Draw primitives of the specified type from the data in the given array of vertices without indexing.
	/// </summary>
	/// <typeparam name="T">The type of the vertices.</typeparam>
	/// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
	/// <param name="vertexData">An array of vertices to draw.</param>
	/// <param name="vertexOffset">The index in the array of the first vertex that should be rendered.</param>
	/// <param name="primitiveCount">The number of primitives to draw.</param>
	/// <param name="vertexDeclaration">The layout of the vertices.</param>
	public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
	{
		if (vertexData == null)
		{
			throw new ArgumentNullException("vertexData");
		}
		if (vertexData.Length == 0)
		{
			throw new ArgumentOutOfRangeException("vertexData");
		}
		if (vertexOffset < 0 || vertexOffset >= vertexData.Length)
		{
			throw new ArgumentOutOfRangeException("vertexOffset");
		}
		if (primitiveCount <= 0)
		{
			throw new ArgumentOutOfRangeException("primitiveCount");
		}
		int vertexCount = GraphicsDevice.GetElementCountArray(primitiveType, primitiveCount);
		if (vertexOffset + vertexCount > vertexData.Length)
		{
			throw new ArgumentOutOfRangeException("primitiveCount");
		}
		if (vertexDeclaration == null)
		{
			throw new ArgumentNullException("vertexDeclaration");
		}
		this.PlatformDrawUserPrimitives(primitiveType, vertexData, vertexOffset, vertexDeclaration, vertexCount);
		this._graphicsMetrics._drawCount++;
		this._graphicsMetrics._primitiveCount += primitiveCount;
	}

	/// <summary>
	/// Draw primitives of the specified type from the currently bound vertexbuffers without indexing.
	/// </summary>
	/// <param name="primitiveType">The type of primitives to draw.</param>
	/// <param name="vertexStart">Index of the vertex to start at.</param>
	/// <param name="primitiveCount">The number of primitives to draw.</param>
	public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount)
	{
		if (this._vertexShader == null)
		{
			throw new InvalidOperationException("Vertex shader must be set before calling DrawPrimitives.");
		}
		if (this._vertexBuffers.Count == 0)
		{
			throw new InvalidOperationException("Vertex buffer must be set before calling DrawPrimitives.");
		}
		if (primitiveCount <= 0)
		{
			throw new ArgumentOutOfRangeException("primitiveCount");
		}
		int vertexCount = GraphicsDevice.GetElementCountArray(primitiveType, primitiveCount);
		this.PlatformDrawPrimitives(primitiveType, vertexStart, vertexCount);
		this._graphicsMetrics._drawCount++;
		this._graphicsMetrics._primitiveCount += primitiveCount;
	}

	/// <summary>
	/// Draw primitives of the specified type by indexing into the given array of vertices with 16-bit indices.
	/// </summary>
	/// <typeparam name="T">The type of the vertices.</typeparam>
	/// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
	/// <param name="vertexData">An array of vertices to draw.</param>
	/// <param name="vertexOffset">The index in the array of the first vertex to draw.</param>
	/// <param name="indexOffset">The index in the array of indices of the first index to use</param>
	/// <param name="primitiveCount">The number of primitives to draw.</param>
	/// <param name="numVertices">The number of vertices to draw.</param>
	/// <param name="indexData">The index data.</param>
	/// <remarks>The <see cref="T:Microsoft.Xna.Framework.Graphics.VertexDeclaration" /> will be found by getting <see cref="P:Microsoft.Xna.Framework.Graphics.IVertexType.VertexDeclaration" />
	/// from an instance of <typeparamref name="T" /> and cached for subsequent calls.</remarks>
	/// <remarks>All indices in the vertex buffer are interpreted relative to the specified <paramref name="vertexOffset" />.
	/// For example a value of zero in the array of indices points to the vertex at index <paramref name="vertexOffset" />
	/// in the array of vertices.</remarks>
	public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount) where T : struct, IVertexType
	{
		this.DrawUserIndexedPrimitives(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, VertexDeclarationCache<T>.VertexDeclaration);
	}

	/// <summary>
	/// Draw primitives of the specified type by indexing into the given array of vertices with 16-bit indices.
	/// </summary>
	/// <typeparam name="T">The type of the vertices.</typeparam>
	/// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
	/// <param name="vertexData">An array of vertices to draw.</param>
	/// <param name="vertexOffset">The index in the array of the first vertex to draw.</param>
	/// <param name="indexOffset">The index in the array of indices of the first index to use</param>
	/// <param name="primitiveCount">The number of primitives to draw.</param>
	/// <param name="numVertices">The number of vertices to draw.</param>
	/// <param name="indexData">The index data.</param>
	/// <param name="vertexDeclaration">The layout of the vertices.</param>
	/// <remarks>All indices in the vertex buffer are interpreted relative to the specified <paramref name="vertexOffset" />.
	/// For example a value of zero in the array of indices points to the vertex at index <paramref name="vertexOffset" />
	/// in the array of vertices.</remarks>
	public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
	{
		if (vertexData == null || vertexData.Length == 0)
		{
			throw new ArgumentNullException("vertexData");
		}
		if (vertexOffset < 0 || vertexOffset >= vertexData.Length)
		{
			throw new ArgumentOutOfRangeException("vertexOffset");
		}
		if (numVertices <= 0 || numVertices > vertexData.Length)
		{
			throw new ArgumentOutOfRangeException("numVertices");
		}
		if (vertexOffset + numVertices > vertexData.Length)
		{
			throw new ArgumentOutOfRangeException("numVertices");
		}
		if (indexData == null || indexData.Length == 0)
		{
			throw new ArgumentNullException("indexData");
		}
		if (indexOffset < 0 || indexOffset >= indexData.Length)
		{
			throw new ArgumentOutOfRangeException("indexOffset");
		}
		if (primitiveCount <= 0)
		{
			throw new ArgumentOutOfRangeException("primitiveCount");
		}
		if (indexOffset + GraphicsDevice.GetElementCountArray(primitiveType, primitiveCount) > indexData.Length)
		{
			throw new ArgumentOutOfRangeException("primitiveCount");
		}
		if (vertexDeclaration == null)
		{
			throw new ArgumentNullException("vertexDeclaration");
		}
		if (vertexDeclaration.VertexStride < ReflectionHelpers.SizeOf<T>.Get())
		{
			throw new ArgumentOutOfRangeException("vertexDeclaration", "Vertex stride of vertexDeclaration should be at least as big as the stride of the actual vertices.");
		}
		this.PlatformDrawUserIndexedPrimitives(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, vertexDeclaration);
		this._graphicsMetrics._drawCount++;
		this._graphicsMetrics._primitiveCount += primitiveCount;
	}

	/// <summary>
	/// Draw primitives of the specified type by indexing into the given array of vertices with 32-bit indices.
	/// </summary>
	/// <typeparam name="T">The type of the vertices.</typeparam>
	/// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
	/// <param name="vertexData">An array of vertices to draw.</param>
	/// <param name="vertexOffset">The index in the array of the first vertex to draw.</param>
	/// <param name="indexOffset">The index in the array of indices of the first index to use</param>
	/// <param name="primitiveCount">The number of primitives to draw.</param>
	/// <param name="numVertices">The number of vertices to draw.</param>
	/// <param name="indexData">The index data.</param>
	/// <remarks>The <see cref="T:Microsoft.Xna.Framework.Graphics.VertexDeclaration" /> will be found by getting <see cref="P:Microsoft.Xna.Framework.Graphics.IVertexType.VertexDeclaration" />
	/// from an instance of <typeparamref name="T" /> and cached for subsequent calls.</remarks>
	/// <remarks>All indices in the vertex buffer are interpreted relative to the specified <paramref name="vertexOffset" />.
	/// For example a value of zero in the array of indices points to the vertex at index <paramref name="vertexOffset" />
	/// in the array of vertices.</remarks>
	public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount) where T : struct, IVertexType
	{
		this.DrawUserIndexedPrimitives(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, VertexDeclarationCache<T>.VertexDeclaration);
	}

	/// <summary>
	/// Draw primitives of the specified type by indexing into the given array of vertices with 32-bit indices.
	/// </summary>
	/// <typeparam name="T">The type of the vertices.</typeparam>
	/// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
	/// <param name="vertexData">An array of vertices to draw.</param>
	/// <param name="vertexOffset">The index in the array of the first vertex to draw.</param>
	/// <param name="indexOffset">The index in the array of indices of the first index to use</param>
	/// <param name="primitiveCount">The number of primitives to draw.</param>
	/// <param name="numVertices">The number of vertices to draw.</param>
	/// <param name="indexData">The index data.</param>
	/// <param name="vertexDeclaration">The layout of the vertices.</param>
	/// <remarks>All indices in the vertex buffer are interpreted relative to the specified <paramref name="vertexOffset" />.
	/// For example value of zero in the array of indices points to the vertex at index <paramref name="vertexOffset" />
	/// in the array of vertices.</remarks>
	public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
	{
		if (vertexData == null || vertexData.Length == 0)
		{
			throw new ArgumentNullException("vertexData");
		}
		if (vertexOffset < 0 || vertexOffset >= vertexData.Length)
		{
			throw new ArgumentOutOfRangeException("vertexOffset");
		}
		if (numVertices <= 0 || numVertices > vertexData.Length)
		{
			throw new ArgumentOutOfRangeException("numVertices");
		}
		if (vertexOffset + numVertices > vertexData.Length)
		{
			throw new ArgumentOutOfRangeException("numVertices");
		}
		if (indexData == null || indexData.Length == 0)
		{
			throw new ArgumentNullException("indexData");
		}
		if (indexOffset < 0 || indexOffset >= indexData.Length)
		{
			throw new ArgumentOutOfRangeException("indexOffset");
		}
		if (primitiveCount <= 0)
		{
			throw new ArgumentOutOfRangeException("primitiveCount");
		}
		if (indexOffset + GraphicsDevice.GetElementCountArray(primitiveType, primitiveCount) > indexData.Length)
		{
			throw new ArgumentOutOfRangeException("primitiveCount");
		}
		if (vertexDeclaration == null)
		{
			throw new ArgumentNullException("vertexDeclaration");
		}
		if (vertexDeclaration.VertexStride < ReflectionHelpers.SizeOf<T>.Get())
		{
			throw new ArgumentOutOfRangeException("vertexDeclaration", "Vertex stride of vertexDeclaration should be at least as big as the stride of the actual vertices.");
		}
		this.PlatformDrawUserIndexedPrimitives(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, vertexDeclaration);
		this._graphicsMetrics._drawCount++;
		this._graphicsMetrics._primitiveCount += primitiveCount;
	}

	/// <summary>
	/// Draw instanced geometry from the bound vertex buffers and index buffer.
	/// </summary>
	/// <param name="primitiveType">The type of primitives in the index buffer.</param>
	/// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
	/// <param name="minVertexIndex">This is unused and remains here only for XNA API compatibility.</param>
	/// <param name="numVertices">This is unused and remains here only for XNA API compatibility.</param>
	/// <param name="startIndex">The index within the index buffer to start drawing from.</param>
	/// <param name="primitiveCount">The number of primitives in a single instance.</param>
	/// <param name="instanceCount">The number of instances to render.</param>
	/// <remarks>Note that minVertexIndex and numVertices are unused in MonoGame and will be ignored.</remarks>
	[Obsolete("Use DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int instanceCount) instead. In future versions this method can be removed.")]
	public void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount, int instanceCount)
	{
		this.DrawInstancedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount, 0, instanceCount);
	}

	/// <summary>
	/// Draw instanced geometry from the bound vertex buffers and index buffer.
	/// </summary>
	/// <param name="primitiveType">The type of primitives in the index buffer.</param>
	/// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
	/// <param name="startIndex">The index within the index buffer to start drawing from.</param>
	/// <param name="primitiveCount">The number of primitives in a single instance.</param>
	/// <param name="instanceCount">The number of instances to render.</param>
	/// <remarks>Draw geometry with data from multiple bound vertex streams at different frequencies.</remarks>
	public void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int instanceCount)
	{
		this.DrawInstancedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount, 0, instanceCount);
	}

	/// <summary>
	/// Draw instanced geometry from the bound vertex buffers and index buffer.
	/// </summary>
	/// <param name="primitiveType">The type of primitives in the index buffer.</param>
	/// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
	/// <param name="startIndex">The index within the index buffer to start drawing from.</param>
	/// <param name="primitiveCount">The number of primitives in a single instance.</param>
	/// <param name="baseInstance">Used to offset the instance range indexed from the instance buffer.</param>
	/// <param name="instanceCount">The number of instances to render.</param>
	/// <remarks>Draw geometry with data from multiple bound vertex streams at different frequencies.</remarks>
	public void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int baseInstance, int instanceCount)
	{
		if (this._vertexShader == null)
		{
			throw new InvalidOperationException("Vertex shader must be set before calling DrawInstancedPrimitives.");
		}
		if (this._vertexBuffers.Count == 0)
		{
			throw new InvalidOperationException("Vertex buffer must be set before calling DrawInstancedPrimitives.");
		}
		if (this._indexBuffer == null)
		{
			throw new InvalidOperationException("Index buffer must be set before calling DrawInstancedPrimitives.");
		}
		if (primitiveCount <= 0)
		{
			throw new ArgumentOutOfRangeException("primitiveCount");
		}
		this.PlatformDrawInstancedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount, baseInstance, instanceCount);
		this._graphicsMetrics._drawCount++;
		this._graphicsMetrics._primitiveCount += primitiveCount * instanceCount;
	}

	/// <summary>
	/// Gets the Pixel data of what is currently drawn on screen.
	/// The format is whatever the current format of the backbuffer is.
	/// </summary>
	/// <typeparam name="T">A byte[] of size (ViewPort.Width * ViewPort.Height * 4)</typeparam>
	public void GetBackBufferData<T>(T[] data) where T : struct
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		this.GetBackBufferData(null, data, 0, data.Length);
	}

	public void GetBackBufferData<T>(T[] data, int startIndex, int elementCount) where T : struct
	{
		this.GetBackBufferData(null, data, startIndex, elementCount);
	}

	public void GetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		int width;
		int height;
		if (rect.HasValue)
		{
			Rectangle rectangle = rect.Value;
			width = rectangle.Width;
			height = rectangle.Height;
			if (rectangle.X < 0 || rectangle.Y < 0 || rectangle.Width <= 0 || rectangle.Height <= 0 || rectangle.Right > this.PresentationParameters.BackBufferWidth || rectangle.Top > this.PresentationParameters.BackBufferHeight)
			{
				throw new ArgumentException("Rectangle must fit in BackBuffer dimensions");
			}
		}
		else
		{
			width = this.PresentationParameters.BackBufferWidth;
			height = this.PresentationParameters.BackBufferHeight;
		}
		int tSize = ReflectionHelpers.SizeOf<T>.Get();
		int fSize = this.PresentationParameters.BackBufferFormat.GetSize();
		if (tSize > fSize || fSize % tSize != 0)
		{
			throw new ArgumentException("Type T is of an invalid size for the format of this texture.", "T");
		}
		if (startIndex < 0 || startIndex >= data.Length)
		{
			throw new ArgumentException("startIndex must be at least zero and smaller than data.Length.", "startIndex");
		}
		if (data.Length < startIndex + elementCount)
		{
			throw new ArgumentException("The data array is too small.");
		}
		int dataByteSize = width * height * fSize;
		if (elementCount * tSize != dataByteSize)
		{
			throw new ArgumentException($"elementCount is not the right size, elementCount * sizeof(T) is {elementCount * tSize}, but data size is {dataByteSize} bytes.", "elementCount");
		}
		this.PlatformGetBackBufferData(rect, data, startIndex, elementCount);
	}

	private static int GetElementCountArray(PrimitiveType primitiveType, int primitiveCount)
	{
		return primitiveType switch
		{
			PrimitiveType.LineList => primitiveCount * 2, 
			PrimitiveType.LineStrip => primitiveCount + 1, 
			PrimitiveType.TriangleList => primitiveCount * 3, 
			PrimitiveType.TriangleStrip => primitiveCount + 2, 
			_ => throw new NotSupportedException(), 
		};
	}

	internal static Rectangle GetDefaultTitleSafeArea(int x, int y, int width, int height)
	{
		int marginX = (width + 19) / 20;
		int marginY = (height + 19) / 20;
		x += marginX;
		y += marginY;
		width -= marginX * 2;
		height -= marginY * 2;
		return new Rectangle(x, y, width, height);
	}

	internal static Rectangle GetTitleSafeArea(int x, int y, int width, int height)
	{
		return GraphicsDevice.PlatformGetTitleSafeArea(x, y, width, height);
	}

	internal void SetVertexAttributeArray(bool[] attrs)
	{
		for (int x = 0; x < attrs.Length; x++)
		{
			if (attrs[x] && !GraphicsDevice._enabledVertexAttributes.Contains(x))
			{
				GraphicsDevice._enabledVertexAttributes.Add(x);
				GL.EnableVertexAttribArray(x);
			}
			else if (!attrs[x] && GraphicsDevice._enabledVertexAttributes.Contains(x))
			{
				GraphicsDevice._enabledVertexAttributes.Remove(x);
				GL.DisableVertexAttribArray(x);
			}
		}
	}

	private void ApplyAttribs(Shader shader, int baseVertex)
	{
		int programHash = this.ShaderProgramHash;
		bool bindingsChanged = false;
		for (int slot = 0; slot < this._vertexBuffers.Count; slot++)
		{
			VertexBufferBinding vertexBufferBinding = this._vertexBuffers.Get(slot);
			VertexDeclaration vertexDeclaration = vertexBufferBinding.VertexBuffer.VertexDeclaration;
			VertexDeclaration.VertexDeclarationAttributeInfo attrInfo = vertexDeclaration.GetAttributeInfo(shader, programHash);
			int vertexStride = vertexDeclaration.VertexStride;
			IntPtr offset = (IntPtr)(vertexDeclaration.VertexStride * (baseVertex + vertexBufferBinding.VertexOffset));
			if (!GraphicsDevice._attribsDirty && slot < GraphicsDevice._activeBufferBindingInfosCount && GraphicsDevice._bufferBindingInfos[slot].VertexOffset == offset && GraphicsDevice._bufferBindingInfos[slot].AttributeInfo == attrInfo && GraphicsDevice._bufferBindingInfos[slot].InstanceFrequency == vertexBufferBinding.InstanceFrequency && GraphicsDevice._bufferBindingInfos[slot].Vbo == vertexBufferBinding.VertexBuffer.vbo)
			{
				continue;
			}
			bindingsChanged = true;
			GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferBinding.VertexBuffer.vbo);
			if (!this.GraphicsCapabilities.SupportsInstancing && vertexBufferBinding.InstanceFrequency > 0)
			{
				throw new PlatformNotSupportedException("Instanced geometry drawing requires at least OpenGL 3.2 or GLES 3.2. Try upgrading your graphics drivers.");
			}
			foreach (VertexDeclaration.VertexDeclarationAttributeInfo.Element element in attrInfo.Elements)
			{
				GL.VertexAttribPointer(element.AttributeLocation, element.NumberOfElements, element.VertexAttribPointerType, element.Normalized, vertexStride, (IntPtr)(offset.ToInt64() + element.Offset));
				if (this.GraphicsCapabilities.SupportsInstancing)
				{
					GL.VertexAttribDivisor(element.AttributeLocation, vertexBufferBinding.InstanceFrequency);
				}
			}
			GraphicsDevice._bufferBindingInfos[slot].VertexOffset = offset;
			GraphicsDevice._bufferBindingInfos[slot].AttributeInfo = attrInfo;
			GraphicsDevice._bufferBindingInfos[slot].InstanceFrequency = vertexBufferBinding.InstanceFrequency;
			GraphicsDevice._bufferBindingInfos[slot].Vbo = vertexBufferBinding.VertexBuffer.vbo;
		}
		GraphicsDevice._attribsDirty = false;
		if (bindingsChanged)
		{
			Array.Clear(GraphicsDevice._newEnabledVertexAttributes, 0, GraphicsDevice._newEnabledVertexAttributes.Length);
			for (int i = 0; i < this._vertexBuffers.Count; i++)
			{
				foreach (VertexDeclaration.VertexDeclarationAttributeInfo.Element element2 in GraphicsDevice._bufferBindingInfos[i].AttributeInfo.Elements)
				{
					GraphicsDevice._newEnabledVertexAttributes[element2.AttributeLocation] = true;
				}
			}
			GraphicsDevice._activeBufferBindingInfosCount = this._vertexBuffers.Count;
		}
		this.SetVertexAttributeArray(GraphicsDevice._newEnabledVertexAttributes);
	}

	private void PlatformSetup()
	{
		this._programCache = new ShaderProgramCache(this);
		WindowInfo windowInfo = new WindowInfo(SdlGameWindow.Instance.Handle);
		if (this.Context == null || this.Context.IsDisposed)
		{
			this.Context = GL.CreateContext(windowInfo);
		}
		this.Context.MakeCurrent(windowInfo);
		this.Context.SwapInterval = this.PresentationParameters.PresentationInterval.GetSwapInterval();
		this.Context.MakeCurrent(windowInfo);
		GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out this.MaxTextureSlots);
		GL.GetInteger(GetPName.MaxTextureSize, out this._maxTextureSize);
		GL.GetInteger(GetPName.MaxVertexAttribs, out this.MaxVertexAttributes);
		this._maxVertexBufferSlots = this.MaxVertexAttributes;
		GraphicsDevice._newEnabledVertexAttributes = new bool[this.MaxVertexAttributes];
		try
		{
			string version = GL.GetString(StringName.Version);
			if (string.IsNullOrEmpty(version))
			{
				throw new NoSuitableGraphicsDeviceException("Unable to retrieve OpenGL version");
			}
			this.glMajorVersion = Convert.ToInt32(version.Substring(0, 1));
			this.glMinorVersion = Convert.ToInt32(version.Substring(2, 1));
		}
		catch (FormatException)
		{
			this.glMajorVersion = 1;
			this.glMinorVersion = 1;
		}
		GL.GetInteger(GetPName.MaxDrawBuffers, out var maxDrawBuffers);
		this._drawBuffers = new DrawBuffersEnum[maxDrawBuffers];
		for (int i = 0; i < maxDrawBuffers; i++)
		{
			this._drawBuffers[i] = (DrawBuffersEnum)(36064 + i);
		}
	}

	private void PlatformInitialize()
	{
		this._viewport = new Viewport(0, 0, this.PresentationParameters.BackBufferWidth, this.PresentationParameters.BackBufferHeight);
		GraphicsDevice._enabledVertexAttributes.Clear();
		this._programCache.Clear();
		this._shaderProgram = null;
		this.framebufferHelper = FramebufferHelper.Create(this);
		this.PlatformApplyBlend(force: true);
		this.DepthStencilState.PlatformApplyState(this, force: true);
		this.RasterizerState.PlatformApplyState(this, force: true);
		GraphicsDevice._bufferBindingInfos = new BufferBindingInfo[this._maxVertexBufferSlots];
		for (int i = 0; i < GraphicsDevice._bufferBindingInfos.Length; i++)
		{
			GraphicsDevice._bufferBindingInfos[i] = new BufferBindingInfo(null, IntPtr.Zero, 0, -1);
		}
	}

	private void PlatformClear(ClearOptions options, Vector4 color, float depth, int stencil)
	{
		Rectangle prevScissorRect = this.ScissorRectangle;
		DepthStencilState prevDepthStencilState = this.DepthStencilState;
		BlendState prevBlendState = this.BlendState;
		this.ScissorRectangle = this._viewport.Bounds;
		this.DepthStencilState = this.clearDepthStencilState;
		this.BlendState = BlendState.Opaque;
		this.ApplyState(applyShaders: false);
		ClearBufferMask bufferMask = (ClearBufferMask)0;
		if ((options & ClearOptions.Target) == ClearOptions.Target)
		{
			if (color != this._lastClearColor)
			{
				GL.ClearColor(color.X, color.Y, color.Z, color.W);
				this._lastClearColor = color;
			}
			bufferMask |= ClearBufferMask.ColorBufferBit;
		}
		if ((options & ClearOptions.Stencil) == ClearOptions.Stencil)
		{
			if (stencil != this._lastClearStencil)
			{
				GL.ClearStencil(stencil);
				this._lastClearStencil = stencil;
			}
			bufferMask |= ClearBufferMask.StencilBufferBit;
		}
		if ((options & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer)
		{
			if (depth != this._lastClearDepth)
			{
				GL.ClearDepth(depth);
				this._lastClearDepth = depth;
			}
			bufferMask |= ClearBufferMask.DepthBufferBit;
		}
		GL.Clear(bufferMask);
		this.ScissorRectangle = prevScissorRect;
		this.DepthStencilState = prevDepthStencilState;
		this.BlendState = prevBlendState;
	}

	private void PlatformDispose()
	{
		this._programCache.Dispose();
		this.Context.Dispose();
		this.Context = null;
	}

	internal void DisposeTexture(int handle)
	{
		if (!this._isDisposed)
		{
			lock (this._disposeActionsLock)
			{
				this._disposeNextFrame.Add(ResourceHandle.Texture(handle));
			}
		}
	}

	internal void DisposeBuffer(int handle)
	{
		if (!this._isDisposed)
		{
			lock (this._disposeActionsLock)
			{
				this._disposeNextFrame.Add(ResourceHandle.Buffer(handle));
			}
		}
	}

	internal void DisposeShader(int handle)
	{
		if (!this._isDisposed)
		{
			lock (this._disposeActionsLock)
			{
				this._disposeNextFrame.Add(ResourceHandle.Shader(handle));
			}
		}
	}

	internal void DisposeProgram(int handle)
	{
		if (!this._isDisposed)
		{
			lock (this._disposeActionsLock)
			{
				this._disposeNextFrame.Add(ResourceHandle.Program(handle));
			}
		}
	}

	internal void DisposeQuery(int handle)
	{
		if (!this._isDisposed)
		{
			lock (this._disposeActionsLock)
			{
				this._disposeNextFrame.Add(ResourceHandle.Query(handle));
			}
		}
	}

	internal void DisposeFramebuffer(int handle)
	{
		if (!this._isDisposed)
		{
			lock (this._disposeActionsLock)
			{
				this._disposeNextFrame.Add(ResourceHandle.Framebuffer(handle));
			}
		}
	}

	internal static void DisposeContext(IntPtr resource)
	{
		lock (GraphicsDevice._disposeContextsLock)
		{
			GraphicsDevice._disposeContexts.Add(resource);
		}
	}

	internal static void DisposeContexts()
	{
		lock (GraphicsDevice._disposeContextsLock)
		{
			int count = GraphicsDevice._disposeContexts.Count;
			for (int i = 0; i < count; i++)
			{
				Sdl.GL.DeleteContext(GraphicsDevice._disposeContexts[i]);
			}
			GraphicsDevice._disposeContexts.Clear();
		}
	}

	private void PlatformPresent()
	{
		this.Context.SwapBuffers();
		int count = this._disposeThisFrame.Count;
		for (int i = 0; i < count; i++)
		{
			this._disposeThisFrame[i].Free();
		}
		this._disposeThisFrame.Clear();
		lock (this._disposeActionsLock)
		{
			List<ResourceHandle> temp = this._disposeThisFrame;
			this._disposeThisFrame = this._disposeNextFrame;
			this._disposeNextFrame = temp;
		}
	}

	private void PlatformSetViewport(ref Viewport value)
	{
		if (this.IsRenderTargetBound)
		{
			GL.Viewport(value.X, value.Y, value.Width, value.Height);
		}
		else
		{
			GL.Viewport(value.X, this.PresentationParameters.BackBufferHeight - value.Y - value.Height, value.Width, value.Height);
		}
		GL.DepthRange(value.MinDepth, value.MaxDepth);
		this._vertexShaderDirty = true;
	}

	private void PlatformApplyDefaultRenderTarget()
	{
		this.framebufferHelper.BindFramebuffer(this.glFramebuffer);
		this._rasterizerStateDirty = true;
		this.Textures.Dirty();
	}

	internal void PlatformCreateRenderTarget(IRenderTarget renderTarget, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
	{
		int color = 0;
		int depth = 0;
		int stencil = 0;
		if (preferredMultiSampleCount > 0 && this.framebufferHelper.SupportsBlitFramebuffer)
		{
			this.framebufferHelper.GenRenderbuffer(out color);
			this.framebufferHelper.BindRenderbuffer(color);
			this.framebufferHelper.RenderbufferStorageMultisample(preferredMultiSampleCount, 32856, width, height);
		}
		if (preferredDepthFormat != DepthFormat.None)
		{
			RenderbufferStorage depthInternalFormat = RenderbufferStorage.DepthComponent16;
			RenderbufferStorage stencilInternalFormat = (RenderbufferStorage)0;
			switch (preferredDepthFormat)
			{
			case DepthFormat.Depth16:
				depthInternalFormat = RenderbufferStorage.DepthComponent16;
				break;
			case DepthFormat.Depth24:
				depthInternalFormat = RenderbufferStorage.DepthComponent24;
				break;
			case DepthFormat.Depth24Stencil8:
				depthInternalFormat = RenderbufferStorage.Depth24Stencil8;
				break;
			}
			if (depthInternalFormat != 0)
			{
				this.framebufferHelper.GenRenderbuffer(out depth);
				this.framebufferHelper.BindRenderbuffer(depth);
				this.framebufferHelper.RenderbufferStorageMultisample(preferredMultiSampleCount, (int)depthInternalFormat, width, height);
				if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
				{
					stencil = depth;
					if (stencilInternalFormat != 0)
					{
						this.framebufferHelper.GenRenderbuffer(out stencil);
						this.framebufferHelper.BindRenderbuffer(stencil);
						this.framebufferHelper.RenderbufferStorageMultisample(preferredMultiSampleCount, (int)stencilInternalFormat, width, height);
					}
				}
			}
		}
		if (color != 0)
		{
			renderTarget.GLColorBuffer = color;
		}
		else
		{
			renderTarget.GLColorBuffer = renderTarget.GLTexture;
		}
		renderTarget.GLDepthBuffer = depth;
		renderTarget.GLStencilBuffer = stencil;
	}

	internal void PlatformDeleteRenderTarget(IRenderTarget renderTarget)
	{
		int color = 0;
		int depth = 0;
		int stencil = 0;
		bool colorIsRenderbuffer = false;
		color = renderTarget.GLColorBuffer;
		depth = renderTarget.GLDepthBuffer;
		stencil = renderTarget.GLStencilBuffer;
		colorIsRenderbuffer = color != renderTarget.GLTexture;
		if (color == 0)
		{
			return;
		}
		if (colorIsRenderbuffer)
		{
			this.framebufferHelper.DeleteRenderbuffer(color);
		}
		if (stencil != 0 && stencil != depth)
		{
			this.framebufferHelper.DeleteRenderbuffer(stencil);
		}
		if (depth != 0)
		{
			this.framebufferHelper.DeleteRenderbuffer(depth);
		}
		List<RenderTargetBinding[]> bindingsToDelete = new List<RenderTargetBinding[]>();
		foreach (RenderTargetBinding[] bindings in this.glFramebuffers.Keys)
		{
			RenderTargetBinding[] array = bindings;
			foreach (RenderTargetBinding binding in array)
			{
				if (binding.RenderTarget == renderTarget)
				{
					bindingsToDelete.Add(bindings);
					break;
				}
			}
		}
		foreach (RenderTargetBinding[] bindings2 in bindingsToDelete)
		{
			int fbo = 0;
			if (this.glFramebuffers.TryGetValue(bindings2, out fbo))
			{
				this.framebufferHelper.DeleteFramebuffer(fbo);
				this.glFramebuffers.Remove(bindings2);
			}
			if (this.glResolveFramebuffers.TryGetValue(bindings2, out fbo))
			{
				this.framebufferHelper.DeleteFramebuffer(fbo);
				this.glResolveFramebuffers.Remove(bindings2);
			}
		}
	}

	private void PlatformResolveRenderTargets()
	{
		if (this._currentRenderTargetCount == 0)
		{
			return;
		}
		RenderTargetBinding renderTargetBinding = this._currentRenderTargetBindings[0];
		IRenderTarget renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
		if (renderTarget.MultiSampleCount > 0 && this.framebufferHelper.SupportsBlitFramebuffer)
		{
			int glResolveFramebuffer = 0;
			if (!this.glResolveFramebuffers.TryGetValue(this._currentRenderTargetBindings, out glResolveFramebuffer))
			{
				this.framebufferHelper.GenFramebuffer(out glResolveFramebuffer);
				this.framebufferHelper.BindFramebuffer(glResolveFramebuffer);
				for (int i = 0; i < this._currentRenderTargetCount; i++)
				{
					IRenderTarget rt = this._currentRenderTargetBindings[i].RenderTarget as IRenderTarget;
					this.framebufferHelper.FramebufferTexture2D(36064 + i, (int)rt.GetFramebufferTarget(renderTargetBinding), rt.GLTexture);
				}
				this.glResolveFramebuffers.Add((RenderTargetBinding[])this._currentRenderTargetBindings.Clone(), glResolveFramebuffer);
			}
			else
			{
				this.framebufferHelper.BindFramebuffer(glResolveFramebuffer);
			}
			if (this._lastRasterizerState.ScissorTestEnable)
			{
				GL.Disable(EnableCap.ScissorTest);
			}
			int glFramebuffer = this.glFramebuffers[this._currentRenderTargetBindings];
			this.framebufferHelper.BindReadFramebuffer(glFramebuffer);
			for (int j = 0; j < this._currentRenderTargetCount; j++)
			{
				renderTargetBinding = this._currentRenderTargetBindings[j];
				renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
				this.framebufferHelper.BlitFramebuffer(j, renderTarget.Width, renderTarget.Height);
			}
			if (renderTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents && this.framebufferHelper.SupportsInvalidateFramebuffer)
			{
				this.framebufferHelper.InvalidateReadFramebuffer();
			}
			if (this._lastRasterizerState.ScissorTestEnable)
			{
				GL.Enable(EnableCap.ScissorTest);
			}
		}
		for (int k = 0; k < this._currentRenderTargetCount; k++)
		{
			renderTargetBinding = this._currentRenderTargetBindings[k];
			renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
			if (renderTarget.LevelCount > 1)
			{
				GL.BindTexture(renderTarget.GLTarget, renderTarget.GLTexture);
				this.framebufferHelper.GenerateMipmap((int)renderTarget.GLTarget);
			}
		}
	}

	private IRenderTarget PlatformApplyRenderTargets()
	{
		int glFramebuffer = 0;
		if (!this.glFramebuffers.TryGetValue(this._currentRenderTargetBindings, out glFramebuffer))
		{
			this.framebufferHelper.GenFramebuffer(out glFramebuffer);
			this.framebufferHelper.BindFramebuffer(glFramebuffer);
			RenderTargetBinding renderTargetBinding = this._currentRenderTargetBindings[0];
			IRenderTarget renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
			this.framebufferHelper.FramebufferRenderbuffer(36096, renderTarget.GLDepthBuffer);
			this.framebufferHelper.FramebufferRenderbuffer(36128, renderTarget.GLStencilBuffer);
			for (int i = 0; i < this._currentRenderTargetCount; i++)
			{
				renderTargetBinding = this._currentRenderTargetBindings[i];
				renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
				int attachement = 36064 + i;
				if (renderTarget.GLColorBuffer != renderTarget.GLTexture)
				{
					this.framebufferHelper.FramebufferRenderbuffer(attachement, renderTarget.GLColorBuffer);
				}
				else
				{
					this.framebufferHelper.FramebufferTexture2D(attachement, (int)renderTarget.GetFramebufferTarget(renderTargetBinding), renderTarget.GLTexture, 0, renderTarget.MultiSampleCount);
				}
			}
			this.glFramebuffers.Add((RenderTargetBinding[])this._currentRenderTargetBindings.Clone(), glFramebuffer);
		}
		else
		{
			this.framebufferHelper.BindFramebuffer(glFramebuffer);
		}
		GL.DrawBuffers(this._currentRenderTargetCount, this._drawBuffers);
		this._rasterizerStateDirty = true;
		this.Textures.Dirty();
		return this._currentRenderTargetBindings[0].RenderTarget as IRenderTarget;
	}

	private static GLPrimitiveType PrimitiveTypeGL(PrimitiveType primitiveType)
	{
		return primitiveType switch
		{
			PrimitiveType.LineList => GLPrimitiveType.Lines, 
			PrimitiveType.LineStrip => GLPrimitiveType.LineStrip, 
			PrimitiveType.TriangleList => GLPrimitiveType.Triangles, 
			PrimitiveType.TriangleStrip => GLPrimitiveType.TriangleStrip, 
			_ => throw new ArgumentException(), 
		};
	}

	/// <summary>
	/// Activates the Current Vertex/Pixel shader pair into a program.         
	/// </summary>
	private unsafe void ActivateShaderProgram()
	{
		ShaderProgram shaderProgram = this._programCache.GetProgram(this.VertexShader, this.PixelShader);
		if (shaderProgram.Program == -1)
		{
			return;
		}
		if (this._shaderProgram != shaderProgram)
		{
			GL.UseProgram(shaderProgram.Program);
			this._shaderProgram = shaderProgram;
		}
		int posFixupLoc = shaderProgram.GetUniformLocation("posFixup");
		if (posFixupLoc != -1)
		{
			GraphicsDevice._posFixup[0] = 1f;
			GraphicsDevice._posFixup[1] = 1f;
			if (this.UseHalfPixelOffset)
			{
				GraphicsDevice._posFixup[2] = 63f / 64f / (float)this.Viewport.Width;
				GraphicsDevice._posFixup[3] = -63f / 64f / (float)this.Viewport.Height;
			}
			else
			{
				GraphicsDevice._posFixup[2] = 0f;
				GraphicsDevice._posFixup[3] = 0f;
			}
			if (this.IsRenderTargetBound)
			{
				GraphicsDevice._posFixup[1] *= -1f;
				GraphicsDevice._posFixup[3] *= -1f;
			}
			fixed (float* floatPtr = GraphicsDevice._posFixup)
			{
				GL.Uniform4(posFixupLoc, 1, floatPtr);
			}
		}
	}

	internal void PlatformBeginApplyState()
	{
		Threading.EnsureUIThread();
	}

	private void PlatformApplyBlend(bool force = false)
	{
		this._actualBlendState.PlatformApplyState(this, force);
		this.ApplyBlendFactor(force);
	}

	private void ApplyBlendFactor(bool force)
	{
		if (force || this.BlendFactor != this._lastBlendState.BlendFactor)
		{
			GL.BlendColor((float)(int)this.BlendFactor.R / 255f, (float)(int)this.BlendFactor.G / 255f, (float)(int)this.BlendFactor.B / 255f, (float)(int)this.BlendFactor.A / 255f);
			this._lastBlendState.BlendFactor = this.BlendFactor;
		}
	}

	internal void PlatformApplyState(bool applyShaders)
	{
		if (this._scissorRectangleDirty)
		{
			Rectangle scissorRect = this._scissorRectangle;
			if (!this.IsRenderTargetBound)
			{
				scissorRect.Y = this.PresentationParameters.BackBufferHeight - (scissorRect.Y + scissorRect.Height);
			}
			GL.Scissor(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height);
			this._scissorRectangleDirty = false;
		}
		if (!applyShaders)
		{
			return;
		}
		if (this._indexBufferDirty)
		{
			if (this._indexBuffer != null)
			{
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, this._indexBuffer.ibo);
			}
			this._indexBufferDirty = false;
		}
		if (this._vertexShader == null)
		{
			throw new InvalidOperationException("A vertex shader must be set!");
		}
		if (this._pixelShader == null)
		{
			throw new InvalidOperationException("A pixel shader must be set!");
		}
		if (this._vertexShaderDirty || this._pixelShaderDirty)
		{
			this.ActivateShaderProgram();
			if (this._vertexShaderDirty)
			{
				this._graphicsMetrics._vertexShaderCount++;
			}
			if (this._pixelShaderDirty)
			{
				this._graphicsMetrics._pixelShaderCount++;
			}
			this._vertexShaderDirty = (this._pixelShaderDirty = false);
		}
		this._vertexConstantBuffers.SetConstantBuffers(this, this._shaderProgram);
		this._pixelConstantBuffers.SetConstantBuffers(this, this._shaderProgram);
		this.Textures.SetTextures(this);
		this.SamplerStates.PlatformSetSamplers(this);
	}

	private void PlatformDrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
	{
		this.ApplyState(applyShaders: true);
		bool num = this._indexBuffer.IndexElementSize == IndexElementSize.SixteenBits;
		DrawElementsType indexElementType = (num ? DrawElementsType.UnsignedShort : DrawElementsType.UnsignedInt);
		int indexElementSize = (num ? 2 : 4);
		IntPtr indexOffsetInBytes = (IntPtr)(startIndex * indexElementSize);
		int indexElementCount = GraphicsDevice.GetElementCountArray(primitiveType, primitiveCount);
		GLPrimitiveType target = GraphicsDevice.PrimitiveTypeGL(primitiveType);
		this.ApplyAttribs(this._vertexShader, baseVertex);
		GL.DrawElements(target, indexElementCount, indexElementType, indexOffsetInBytes);
	}

	private void PlatformDrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount) where T : struct
	{
		this.ApplyState(applyShaders: true);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		this._indexBufferDirty = true;
		GCHandle vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
		try
		{
			vertexDeclaration.GraphicsDevice = this;
			vertexDeclaration.Apply(this._vertexShader, vbHandle.AddrOfPinnedObject(), this.ShaderProgramHash);
			GL.DrawArrays(GraphicsDevice.PrimitiveTypeGL(primitiveType), vertexOffset, vertexCount);
		}
		finally
		{
			vbHandle.Free();
		}
	}

	private void PlatformDrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
	{
		this.ApplyState(applyShaders: true);
		this.ApplyAttribs(this._vertexShader, 0);
		if (vertexStart < 0)
		{
			vertexStart = 0;
		}
		GL.DrawArrays(GraphicsDevice.PrimitiveTypeGL(primitiveType), vertexStart, vertexCount);
	}

	private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
	{
		this.ApplyState(applyShaders: true);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		this._indexBufferDirty = true;
		GCHandle vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
		GCHandle ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);
		try
		{
			IntPtr vertexAddr = (IntPtr)(vbHandle.AddrOfPinnedObject().ToInt64() + vertexDeclaration.VertexStride * vertexOffset);
			vertexDeclaration.GraphicsDevice = this;
			vertexDeclaration.Apply(this._vertexShader, vertexAddr, this.ShaderProgramHash);
			GL.DrawElements(GraphicsDevice.PrimitiveTypeGL(primitiveType), GraphicsDevice.GetElementCountArray(primitiveType, primitiveCount), DrawElementsType.UnsignedShort, (IntPtr)(ibHandle.AddrOfPinnedObject().ToInt64() + indexOffset * 2));
		}
		finally
		{
			ibHandle.Free();
			vbHandle.Free();
		}
	}

	private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
	{
		this.ApplyState(applyShaders: true);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		this._indexBufferDirty = true;
		GCHandle vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
		GCHandle ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);
		try
		{
			IntPtr vertexAddr = (IntPtr)(vbHandle.AddrOfPinnedObject().ToInt64() + vertexDeclaration.VertexStride * vertexOffset);
			vertexDeclaration.GraphicsDevice = this;
			vertexDeclaration.Apply(this._vertexShader, vertexAddr, this.ShaderProgramHash);
			GL.DrawElements(GraphicsDevice.PrimitiveTypeGL(primitiveType), GraphicsDevice.GetElementCountArray(primitiveType, primitiveCount), DrawElementsType.UnsignedInt, (IntPtr)(ibHandle.AddrOfPinnedObject().ToInt64() + indexOffset * 4));
		}
		finally
		{
			ibHandle.Free();
			vbHandle.Free();
		}
	}

	private void PlatformDrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int baseInstance, int instanceCount)
	{
		if (!this.GraphicsCapabilities.SupportsInstancing)
		{
			throw new PlatformNotSupportedException("Instanced geometry drawing requires at least OpenGL 3.2 or GLES 3.2. Try upgrading your graphics card drivers.");
		}
		this.ApplyState(applyShaders: true);
		bool num = this._indexBuffer.IndexElementSize == IndexElementSize.SixteenBits;
		DrawElementsType indexElementType = (num ? DrawElementsType.UnsignedShort : DrawElementsType.UnsignedInt);
		int indexElementSize = (num ? 2 : 4);
		IntPtr indexOffsetInBytes = (IntPtr)(startIndex * indexElementSize);
		int indexElementCount = GraphicsDevice.GetElementCountArray(primitiveType, primitiveCount);
		GLPrimitiveType target = GraphicsDevice.PrimitiveTypeGL(primitiveType);
		this.ApplyAttribs(this._vertexShader, baseVertex);
		if (baseInstance > 0)
		{
			if (!this.GraphicsCapabilities.SupportsBaseIndexInstancing)
			{
				throw new PlatformNotSupportedException("Instanced geometry drawing with base instance requires at least OpenGL 4.2. Try upgrading your graphics card drivers.");
			}
			GL.DrawElementsInstancedBaseInstance(target, indexElementCount, indexElementType, indexOffsetInBytes, instanceCount, baseInstance);
		}
		else
		{
			GL.DrawElementsInstanced(target, indexElementCount, indexElementType, indexOffsetInBytes, instanceCount);
		}
	}

	private void PlatformGetBackBufferData<T>(Rectangle? rectangle, T[] data, int startIndex, int count) where T : struct
	{
		Rectangle rect = rectangle ?? new Rectangle(0, 0, this.PresentationParameters.BackBufferWidth, this.PresentationParameters.BackBufferHeight);
		int tSize = Marshal.SizeOf<T>();
		int flippedY = this.PresentationParameters.BackBufferHeight - rect.Y - rect.Height;
		GL.ReadPixels(rect.X, flippedY, rect.Width, rect.Height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
		int rowSize = rect.Width * this.PresentationParameters.BackBufferFormat.GetSize() / tSize;
		T[] row = new T[rowSize];
		for (int dy = 0; dy < rect.Height / 2; dy++)
		{
			int topRow = startIndex + dy * rowSize;
			int bottomRow = startIndex + (rect.Height - dy - 1) * rowSize;
			Array.Copy(data, bottomRow, row, 0, rowSize);
			Array.Copy(data, topRow, data, bottomRow, rowSize);
			Array.Copy(row, 0, data, topRow, rowSize);
			count -= rowSize;
		}
	}

	private static Rectangle PlatformGetTitleSafeArea(int x, int y, int width, int height)
	{
		return new Rectangle(x, y, width, height);
	}

	internal void PlatformSetMultiSamplingToMaximum(PresentationParameters presentationParameters, out int quality)
	{
		presentationParameters.MultiSampleCount = 4;
		quality = 0;
	}

	internal void OnPresentationChanged()
	{
		this.Context.MakeCurrent(new WindowInfo(SdlGameWindow.Instance.Handle));
		this.Context.SwapInterval = this.PresentationParameters.PresentationInterval.GetSwapInterval();
		this.ApplyRenderTargets(null);
	}

	private void GetModeSwitchedSize(out int width, out int height)
	{
		Sdl.Display.Mode mode = new Sdl.Display.Mode
		{
			Width = this.PresentationParameters.BackBufferWidth,
			Height = this.PresentationParameters.BackBufferHeight,
			Format = 0u,
			RefreshRate = 0,
			DriverData = IntPtr.Zero
		};
		Sdl.Display.GetClosestDisplayMode(0, mode, out var closest);
		width = closest.Width;
		height = closest.Height;
	}

	private void GetDisplayResolution(out int width, out int height)
	{
		Sdl.Display.GetCurrentDisplayMode(0, out var mode);
		width = mode.Width;
		height = mode.Height;
	}
}
