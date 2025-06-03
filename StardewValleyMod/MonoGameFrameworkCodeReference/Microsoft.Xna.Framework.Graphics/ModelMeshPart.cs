using System;

namespace Microsoft.Xna.Framework.Graphics;

public sealed class ModelMeshPart
{
	private Effect _effect;

	internal ModelMesh parent;

	public Effect Effect
	{
		get
		{
			return this._effect;
		}
		set
		{
			if (value == this._effect)
			{
				return;
			}
			if (this._effect != null)
			{
				bool removeEffect = true;
				foreach (ModelMeshPart part in this.parent.MeshParts)
				{
					if (part != this && part._effect == this._effect)
					{
						removeEffect = false;
						break;
					}
				}
				if (removeEffect)
				{
					this.parent.Effects.Remove(this._effect);
				}
			}
			this._effect = value;
			if (this._effect != null && !this.parent.Effects.Contains(this._effect))
			{
				this.parent.Effects.Add(this._effect);
			}
		}
	}

	public IndexBuffer IndexBuffer { get; set; }

	public int NumVertices { get; set; }

	public int PrimitiveCount { get; set; }

	public int StartIndex { get; set; }

	public object Tag { get; set; }

	public VertexBuffer VertexBuffer { get; set; }

	public int VertexOffset { get; set; }

	internal int VertexBufferIndex { get; set; }

	internal int IndexBufferIndex { get; set; }

	internal int EffectIndex { get; set; }

	/// <summary>
	/// Using this constructor is strongly discouraged. Adding meshes to models at runtime is
	/// not supported and may lead to <see cref="T:System.NullReferenceException" />s if parent is not set.
	/// </summary>
	[Obsolete("This constructor is deprecated and will be made internal in a future release.")]
	public ModelMeshPart()
	{
	}
}
