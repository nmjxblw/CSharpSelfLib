using System;
using System.Collections.Generic;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// This class is used to Cache the links between Vertex/Pixel Shaders and Constant Buffers.
/// It will be responsible for linking the programs under OpenGL if they have not been linked
/// before. If an existing link exists it will be resused.
/// </summary>
internal class ShaderProgramCache : IDisposable
{
	private readonly Dictionary<int, ShaderProgram> _programCache = new Dictionary<int, ShaderProgram>();

	private GraphicsDevice _graphicsDevice;

	private bool disposed;

	public ShaderProgramCache(GraphicsDevice graphicsDevice)
	{
		this._graphicsDevice = graphicsDevice;
	}

	~ShaderProgramCache()
	{
		this.Dispose(disposing: false);
	}

	/// <summary>
	/// Clear the program cache releasing all shader programs.
	/// </summary>
	public void Clear()
	{
		foreach (KeyValuePair<int, ShaderProgram> pair in this._programCache)
		{
			this._graphicsDevice.DisposeProgram(pair.Value.Program);
		}
		this._programCache.Clear();
	}

	public ShaderProgram GetProgram(Shader vertexShader, Shader pixelShader)
	{
		int key = vertexShader.HashKey | pixelShader.HashKey;
		if (!this._programCache.ContainsKey(key))
		{
			this._programCache.Add(key, this.Link(vertexShader, pixelShader));
		}
		return this._programCache[key];
	}

	private ShaderProgram Link(Shader vertexShader, Shader pixelShader)
	{
		int program = GL.CreateProgram();
		GL.AttachShader(program, vertexShader.GetShaderHandle());
		GL.AttachShader(program, pixelShader.GetShaderHandle());
		GL.LinkProgram(program);
		GL.UseProgram(program);
		vertexShader.GetVertexAttributeLocations(program);
		pixelShader.ApplySamplerTextureUnits(program);
		int linked = 0;
		GL.GetProgram(program, GetProgramParameterName.LinkStatus, out linked);
		if (linked == 0)
		{
			Console.WriteLine(GL.GetProgramInfoLog(program));
			GL.DetachShader(program, vertexShader.GetShaderHandle());
			GL.DetachShader(program, pixelShader.GetShaderHandle());
			this._graphicsDevice.DisposeProgram(program);
			throw new InvalidOperationException("Unable to link effect program");
		}
		return new ShaderProgram(program);
	}

	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!this.disposed)
		{
			if (disposing)
			{
				this.Clear();
			}
			this.disposed = true;
		}
	}
}
