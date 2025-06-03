using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

internal class ModelReader : ContentTypeReader<Model>
{
	private static int ReadBoneReference(ContentReader reader, uint boneCount)
	{
		uint boneId = ((boneCount >= 255) ? reader.ReadUInt32() : reader.ReadByte());
		if (boneId != 0)
		{
			return (int)(boneId - 1);
		}
		return -1;
	}

	protected internal override Model Read(ContentReader reader, Model existingInstance)
	{
		uint boneCount = reader.ReadUInt32();
		List<ModelBone> bones = new List<ModelBone>((int)boneCount);
		for (uint i = 0u; i < boneCount; i++)
		{
			string name = reader.ReadObject<string>();
			Matrix matrix = reader.ReadMatrix();
			ModelBone bone = new ModelBone
			{
				Transform = matrix,
				Index = (int)i,
				Name = name
			};
			bones.Add(bone);
		}
		for (int j = 0; j < boneCount; j++)
		{
			ModelBone bone2 = bones[j];
			int parentIndex = ModelReader.ReadBoneReference(reader, boneCount);
			if (parentIndex != -1)
			{
				bone2.Parent = bones[parentIndex];
			}
			uint childCount = reader.ReadUInt32();
			if (childCount == 0)
			{
				continue;
			}
			for (uint j2 = 0u; j2 < childCount; j2++)
			{
				int childIndex = ModelReader.ReadBoneReference(reader, boneCount);
				if (childIndex != -1)
				{
					bone2.AddChild(bones[childIndex]);
				}
			}
		}
		List<ModelMesh> meshes = new List<ModelMesh>();
		int meshCount = reader.ReadInt32();
		for (int k = 0; k < meshCount; k++)
		{
			string name2 = reader.ReadObject<string>();
			int parentBoneIndex = ModelReader.ReadBoneReference(reader, boneCount);
			BoundingSphere boundingSphere = reader.ReadBoundingSphere();
			object meshTag = reader.ReadObject<object>();
			int partCount = reader.ReadInt32();
			List<ModelMeshPart> parts = new List<ModelMeshPart>(partCount);
			for (uint j3 = 0u; j3 < partCount; j3++)
			{
				ModelMeshPart part = ((existingInstance == null) ? new ModelMeshPart() : existingInstance.Meshes[k].MeshParts[(int)j3]);
				part.VertexOffset = reader.ReadInt32();
				part.NumVertices = reader.ReadInt32();
				part.StartIndex = reader.ReadInt32();
				part.PrimitiveCount = reader.ReadInt32();
				part.Tag = reader.ReadObject<object>();
				parts.Add(part);
				int jj = (int)j3;
				reader.ReadSharedResource(delegate(VertexBuffer v)
				{
					parts[jj].VertexBuffer = v;
				});
				reader.ReadSharedResource(delegate(IndexBuffer v)
				{
					parts[jj].IndexBuffer = v;
				});
				reader.ReadSharedResource(delegate(Effect v)
				{
					parts[jj].Effect = v;
				});
			}
			if (existingInstance == null)
			{
				ModelMesh mesh = new ModelMesh(reader.GetGraphicsDevice(), parts);
				mesh.Tag = meshTag;
				mesh.Name = name2;
				mesh.ParentBone = bones[parentBoneIndex];
				mesh.ParentBone.AddMesh(mesh);
				mesh.BoundingSphere = boundingSphere;
				meshes.Add(mesh);
			}
		}
		if (existingInstance != null)
		{
			ModelReader.ReadBoneReference(reader, boneCount);
			reader.ReadObject<object>();
			return existingInstance;
		}
		int rootBoneIndex = ModelReader.ReadBoneReference(reader, boneCount);
		Model model = new Model(reader.GetGraphicsDevice(), bones, meshes);
		model.Root = bones[rootBoneIndex];
		model.BuildHierarchy();
		model.Tag = reader.ReadObject<object>();
		return model;
	}
}
