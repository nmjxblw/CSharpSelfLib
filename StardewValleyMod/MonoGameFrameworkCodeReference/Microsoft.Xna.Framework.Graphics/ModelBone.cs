using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics;

public sealed class ModelBone
{
	private List<ModelBone> children = new List<ModelBone>();

	private List<ModelMesh> meshes = new List<ModelMesh>();

	internal Matrix transform;

	public List<ModelMesh> Meshes
	{
		get
		{
			return this.meshes;
		}
		private set
		{
			this.meshes = value;
		}
	}

	public ModelBoneCollection Children { get; private set; }

	public int Index { get; set; }

	public string Name { get; set; }

	public ModelBone Parent { get; set; }

	public Matrix Transform
	{
		get
		{
			return this.transform;
		}
		set
		{
			this.transform = value;
		}
	}

	/// <summary>
	/// Transform of this node from the root of the model not from the parent
	/// </summary>
	public Matrix ModelTransform { get; set; }

	public ModelBone()
	{
		this.Children = new ModelBoneCollection(new List<ModelBone>());
	}

	public void AddMesh(ModelMesh mesh)
	{
		this.meshes.Add(mesh);
	}

	public void AddChild(ModelBone modelBone)
	{
		this.children.Add(modelBone);
		this.Children = new ModelBoneCollection(this.children);
	}
}
