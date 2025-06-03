using System.Collections.Generic;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

internal class ShaderProgram
{
	public readonly int Program;

	private readonly Dictionary<string, int> _uniformLocations = new Dictionary<string, int>();

	public ShaderProgram(int program)
	{
		this.Program = program;
	}

	public int GetUniformLocation(string name)
	{
		if (this._uniformLocations.ContainsKey(name))
		{
			return this._uniformLocations[name];
		}
		int location = GL.GetUniformLocation(this.Program, name);
		this._uniformLocations[name] = location;
		return location;
	}
}
