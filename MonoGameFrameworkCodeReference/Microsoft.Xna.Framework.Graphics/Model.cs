using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// A basic 3D model with per mesh parent bones.
/// </summary>
public sealed class Model
{
	private static Matrix[] sharedDrawBoneMatrices;

	private GraphicsDevice graphicsDevice;

	/// <summary>
	/// A collection of <see cref="T:Microsoft.Xna.Framework.Graphics.ModelBone" /> objects which describe how each mesh in the
	/// mesh collection for this model relates to its parent mesh.
	/// </summary>
	public ModelBoneCollection Bones { get; private set; }

	/// <summary>
	/// A collection of <see cref="T:Microsoft.Xna.Framework.Graphics.ModelMesh" /> objects which compose the model. Each <see cref="T:Microsoft.Xna.Framework.Graphics.ModelMesh" />
	/// in a model may be moved independently and may be composed of multiple materials
	/// identified as <see cref="T:Microsoft.Xna.Framework.Graphics.ModelMeshPart" /> objects.
	/// </summary>
	public ModelMeshCollection Meshes { get; private set; }

	/// <summary>
	/// Root bone for this model.
	/// </summary>
	public ModelBone Root { get; set; }

	/// <summary>
	/// Custom attached object.
	/// <remarks>
	/// Skinning data is example of attached object for model.
	/// </remarks>
	/// </summary>
	public object Tag { get; set; }

	internal Model()
	{
	}

	/// <summary>
	/// Constructs a model. 
	/// </summary>
	/// <param name="graphicsDevice">A valid reference to <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" />.</param>
	/// <param name="bones">The collection of bones.</param>
	/// <param name="meshes">The collection of meshes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	/// <paramref name="graphicsDevice" /> is null.
	/// </exception>
	/// <exception cref="T:System.ArgumentNullException">
	/// <paramref name="bones" /> is null.
	/// </exception>
	/// <exception cref="T:System.ArgumentNullException">
	/// <paramref name="meshes" /> is null.
	/// </exception>
	public Model(GraphicsDevice graphicsDevice, List<ModelBone> bones, List<ModelMesh> meshes)
	{
		if (graphicsDevice == null)
		{
			throw new ArgumentNullException("graphicsDevice", "The GraphicsDevice must not be null when creating new resources.");
		}
		this.graphicsDevice = graphicsDevice;
		this.Bones = new ModelBoneCollection(bones);
		this.Meshes = new ModelMeshCollection(meshes);
	}

	internal void BuildHierarchy()
	{
		Matrix globalScale = Matrix.CreateScale(0.01f);
		foreach (ModelBone node in this.Root.Children)
		{
			this.BuildHierarchy(node, this.Root.Transform * globalScale, 0);
		}
	}

	private void BuildHierarchy(ModelBone node, Matrix parentTransform, int level)
	{
		node.ModelTransform = node.Transform * parentTransform;
		foreach (ModelBone child in node.Children)
		{
			this.BuildHierarchy(child, node.ModelTransform, level + 1);
		}
	}

	/// <summary>
	/// Draws the model meshes.
	/// </summary>
	/// <param name="world">The world transform.</param>
	/// <param name="view">The view transform.</param>
	/// <param name="projection">The projection transform.</param>
	public void Draw(Matrix world, Matrix view, Matrix projection)
	{
		int boneCount = this.Bones.Count;
		if (Model.sharedDrawBoneMatrices == null || Model.sharedDrawBoneMatrices.Length < boneCount)
		{
			Model.sharedDrawBoneMatrices = new Matrix[boneCount];
		}
		this.CopyAbsoluteBoneTransformsTo(Model.sharedDrawBoneMatrices);
		foreach (ModelMesh mesh in this.Meshes)
		{
			foreach (Effect effect in mesh.Effects)
			{
				IEffectMatrices obj = (effect as IEffectMatrices) ?? throw new InvalidOperationException();
				obj.World = Model.sharedDrawBoneMatrices[mesh.ParentBone.Index] * world;
				obj.View = view;
				obj.Projection = projection;
			}
			mesh.Draw();
		}
	}

	/// <summary>
	/// Copies bone transforms relative to all parent bones of the each bone from this model to a given array.
	/// </summary>
	/// <param name="destinationBoneTransforms">The array receiving the transformed bones.</param>
	public void CopyAbsoluteBoneTransformsTo(Matrix[] destinationBoneTransforms)
	{
		if (destinationBoneTransforms == null)
		{
			throw new ArgumentNullException("destinationBoneTransforms");
		}
		if (destinationBoneTransforms.Length < this.Bones.Count)
		{
			throw new ArgumentOutOfRangeException("destinationBoneTransforms");
		}
		int count = this.Bones.Count;
		for (int index1 = 0; index1 < count; index1++)
		{
			ModelBone modelBone = this.Bones[index1];
			if (modelBone.Parent == null)
			{
				destinationBoneTransforms[index1] = modelBone.transform;
				continue;
			}
			int index2 = modelBone.Parent.Index;
			Matrix.Multiply(ref modelBone.transform, ref destinationBoneTransforms[index2], out destinationBoneTransforms[index1]);
		}
	}

	/// <summary>
	/// Copies bone transforms relative to <see cref="P:Microsoft.Xna.Framework.Graphics.Model.Root" /> bone from a given array to this model.
	/// </summary>
	/// <param name="sourceBoneTransforms">The array of prepared bone transform data.</param>
	/// <exception cref="T:System.ArgumentNullException">
	/// <paramref name="sourceBoneTransforms" /> is null.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// <paramref name="sourceBoneTransforms" /> is invalid.
	/// </exception>
	public void CopyBoneTransformsFrom(Matrix[] sourceBoneTransforms)
	{
		if (sourceBoneTransforms == null)
		{
			throw new ArgumentNullException("sourceBoneTransforms");
		}
		if (sourceBoneTransforms.Length < this.Bones.Count)
		{
			throw new ArgumentOutOfRangeException("sourceBoneTransforms");
		}
		int count = this.Bones.Count;
		for (int i = 0; i < count; i++)
		{
			this.Bones[i].Transform = sourceBoneTransforms[i];
		}
	}

	/// <summary>
	/// Copies bone transforms relative to <see cref="P:Microsoft.Xna.Framework.Graphics.Model.Root" /> bone from this model to a given array.
	/// </summary>
	/// <param name="destinationBoneTransforms">The array receiving the transformed bones.</param>
	/// <exception cref="T:System.ArgumentNullException">
	/// <paramref name="destinationBoneTransforms" /> is null.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// <paramref name="destinationBoneTransforms" /> is invalid.
	/// </exception>
	public void CopyBoneTransformsTo(Matrix[] destinationBoneTransforms)
	{
		if (destinationBoneTransforms == null)
		{
			throw new ArgumentNullException("destinationBoneTransforms");
		}
		if (destinationBoneTransforms.Length < this.Bones.Count)
		{
			throw new ArgumentOutOfRangeException("destinationBoneTransforms");
		}
		int count = this.Bones.Count;
		for (int i = 0; i < count; i++)
		{
			destinationBoneTransforms[i] = this.Bones[i].Transform;
		}
	}
}
