using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics;

public sealed class ModelMesh
{
	private GraphicsDevice graphicsDevice;

	public BoundingSphere BoundingSphere { get; set; }

	public ModelEffectCollection Effects { get; internal set; }

	public ModelMeshPartCollection MeshParts { get; set; }

	public string Name { get; set; }

	public ModelBone ParentBone { get; set; }

	public object Tag { get; set; }

	public ModelMesh(GraphicsDevice graphicsDevice, List<ModelMeshPart> parts)
	{
		this.graphicsDevice = graphicsDevice;
		this.MeshParts = new ModelMeshPartCollection(parts);
		for (int i = 0; i < parts.Count; i++)
		{
			parts[i].parent = this;
		}
		this.Effects = new ModelEffectCollection();
	}

	public void Draw()
	{
		for (int i = 0; i < this.MeshParts.Count; i++)
		{
			ModelMeshPart part = this.MeshParts[i];
			Effect effect = part.Effect;
			if (part.PrimitiveCount > 0)
			{
				this.graphicsDevice.SetVertexBuffer(part.VertexBuffer);
				this.graphicsDevice.Indices = part.IndexBuffer;
				for (int j = 0; j < effect.CurrentTechnique.Passes.Count; j++)
				{
					effect.CurrentTechnique.Passes[j].Apply();
					this.graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
				}
			}
		}
	}
}
