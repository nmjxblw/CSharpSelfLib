using System;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Xna.Framework.Graphics;

[DebuggerDisplay("{DebugDisplayString}")]
public class EffectParameter
{
	/// <summary>
	/// The next state key used when an effect parameter
	/// is updated by any of the 'set' methods.
	/// </summary>
	internal static ulong NextStateKey { get; private set; }

	public string Name { get; private set; }

	public string Semantic { get; private set; }

	public EffectParameterClass ParameterClass { get; private set; }

	public EffectParameterType ParameterType { get; private set; }

	public int RowCount { get; private set; }

	public int ColumnCount { get; private set; }

	public EffectParameterCollection Elements { get; private set; }

	public EffectParameterCollection StructureMembers { get; private set; }

	public EffectAnnotationCollection Annotations { get; private set; }

	internal object Data { get; private set; }

	/// <summary>
	/// The current state key which is used to detect
	/// if the parameter value has been changed.
	/// </summary>
	internal ulong StateKey { get; private set; }

	/// <summary>
	/// Property referenced by the DebuggerDisplayAttribute.
	/// </summary>
	private string DebugDisplayString
	{
		get
		{
			string semanticStr = string.Empty;
			if (!string.IsNullOrEmpty(this.Semantic))
			{
				semanticStr = " <" + this.Semantic + ">";
			}
			return string.Concat("[", this.ParameterClass, " ", this.ParameterType, "]", semanticStr, " ", this.Name, " : ", this.GetDataValueString());
		}
	}

	internal EffectParameter(EffectParameterClass class_, EffectParameterType type, string name, int rowCount, int columnCount, string semantic, EffectAnnotationCollection annotations, EffectParameterCollection elements, EffectParameterCollection structMembers, object data)
	{
		this.ParameterClass = class_;
		this.ParameterType = type;
		this.Name = name;
		this.Semantic = semantic;
		this.Annotations = annotations;
		this.RowCount = rowCount;
		this.ColumnCount = columnCount;
		this.Elements = elements;
		this.StructureMembers = structMembers;
		this.Data = data;
		this.StateKey = EffectParameter.NextStateKey++;
	}

	internal EffectParameter(EffectParameter cloneSource)
	{
		this.ParameterClass = cloneSource.ParameterClass;
		this.ParameterType = cloneSource.ParameterType;
		this.Name = cloneSource.Name;
		this.Semantic = cloneSource.Semantic;
		this.Annotations = cloneSource.Annotations;
		this.RowCount = cloneSource.RowCount;
		this.ColumnCount = cloneSource.ColumnCount;
		this.Elements = cloneSource.Elements.Clone();
		this.StructureMembers = cloneSource.StructureMembers.Clone();
		if (cloneSource.Data is Array array)
		{
			this.Data = array.Clone();
		}
		this.StateKey = EffectParameter.NextStateKey++;
	}

	private string GetDataValueString()
	{
		string valueStr;
		if (this.Data == null)
		{
			valueStr = ((this.Elements != null) ? string.Join(", ", this.Elements.Select((EffectParameter e) => e.GetDataValueString())) : "(null)");
		}
		else
		{
			switch (this.ParameterClass)
			{
			case EffectParameterClass.Object:
				valueStr = this.Data.ToString();
				break;
			case EffectParameterClass.Matrix:
				valueStr = "...";
				break;
			case EffectParameterClass.Scalar:
				valueStr = (this.Data as Array).GetValue(0).ToString();
				break;
			case EffectParameterClass.Vector:
			{
				Array array = this.Data as Array;
				string[] arrayStr = new string[array.Length];
				int idx = 0;
				foreach (object item in array)
				{
					_ = item;
					arrayStr[idx] = array.GetValue(idx).ToString();
					idx++;
				}
				valueStr = string.Join(" ", arrayStr);
				break;
			}
			default:
				valueStr = this.Data.ToString();
				break;
			}
		}
		return "{" + valueStr + "}";
	}

	public bool GetValueBoolean()
	{
		if (this.ParameterClass != EffectParameterClass.Scalar || this.ParameterType != EffectParameterType.Bool)
		{
			throw new InvalidCastException();
		}
		return ((float[])this.Data)[0] != 0f;
	}

	public int GetValueInt32()
	{
		if (this.ParameterClass != EffectParameterClass.Scalar || this.ParameterType != EffectParameterType.Int32)
		{
			throw new InvalidCastException();
		}
		return (int)((float[])this.Data)[0];
	}

	public int[] GetValueInt32Array()
	{
		if (this.Elements != null && this.Elements.Count > 0)
		{
			int[] ret = new int[this.RowCount * this.ColumnCount * this.Elements.Count];
			for (int i = 0; i < this.Elements.Count; i++)
			{
				int[] elmArray = this.Elements[i].GetValueInt32Array();
				for (int j = 0; j < elmArray.Length; j++)
				{
					ret[this.RowCount * this.ColumnCount * i + j] = elmArray[j];
				}
			}
			return ret;
		}
		if (this.ParameterClass == EffectParameterClass.Scalar)
		{
			return new int[1] { this.GetValueInt32() };
		}
		throw new NotImplementedException();
	}

	public Matrix GetValueMatrix()
	{
		if (this.ParameterClass != EffectParameterClass.Matrix || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		if (this.RowCount != 4 || this.ColumnCount != 4)
		{
			throw new InvalidCastException();
		}
		float[] floatData = (float[])this.Data;
		return new Matrix(floatData[0], floatData[4], floatData[8], floatData[12], floatData[1], floatData[5], floatData[9], floatData[13], floatData[2], floatData[6], floatData[10], floatData[14], floatData[3], floatData[7], floatData[11], floatData[15]);
	}

	public Matrix[] GetValueMatrixArray(int count)
	{
		if (this.ParameterClass != EffectParameterClass.Matrix || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		Matrix[] ret = new Matrix[count];
		for (int i = 0; i < count; i++)
		{
			ret[i] = this.Elements[i].GetValueMatrix();
		}
		return ret;
	}

	public Quaternion GetValueQuaternion()
	{
		if (this.ParameterClass != EffectParameterClass.Vector || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		float[] vecInfo = (float[])this.Data;
		return new Quaternion(vecInfo[0], vecInfo[1], vecInfo[2], vecInfo[3]);
	}

	public float GetValueSingle()
	{
		if (this.ParameterClass != EffectParameterClass.Scalar || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		return ((float[])this.Data)[0];
	}

	public float[] GetValueSingleArray()
	{
		if (this.Elements != null && this.Elements.Count > 0)
		{
			float[] ret = new float[this.RowCount * this.ColumnCount * this.Elements.Count];
			for (int i = 0; i < this.Elements.Count; i++)
			{
				float[] elmArray = this.Elements[i].GetValueSingleArray();
				for (int j = 0; j < elmArray.Length; j++)
				{
					ret[this.RowCount * this.ColumnCount * i + j] = elmArray[j];
				}
			}
			return ret;
		}
		switch (this.ParameterClass)
		{
		case EffectParameterClass.Scalar:
			return new float[1] { this.GetValueSingle() };
		case EffectParameterClass.Vector:
		case EffectParameterClass.Matrix:
			if (this.Data is Matrix)
			{
				return Matrix.ToFloatArray((Matrix)this.Data);
			}
			return (float[])this.Data;
		default:
			throw new NotImplementedException();
		}
	}

	public string GetValueString()
	{
		if (this.ParameterClass != EffectParameterClass.Object || this.ParameterType != EffectParameterType.String)
		{
			throw new InvalidCastException();
		}
		return ((string[])this.Data)[0];
	}

	public Texture2D GetValueTexture2D()
	{
		if (this.ParameterClass != EffectParameterClass.Object || this.ParameterType != EffectParameterType.Texture2D)
		{
			throw new InvalidCastException();
		}
		return (Texture2D)this.Data;
	}

	public Texture3D GetValueTexture3D()
	{
		if (this.ParameterClass != EffectParameterClass.Object || this.ParameterType != EffectParameterType.Texture3D)
		{
			throw new InvalidCastException();
		}
		return (Texture3D)this.Data;
	}

	public TextureCube GetValueTextureCube()
	{
		if (this.ParameterClass != EffectParameterClass.Object || this.ParameterType != EffectParameterType.TextureCube)
		{
			throw new InvalidCastException();
		}
		return (TextureCube)this.Data;
	}

	public Vector2 GetValueVector2()
	{
		if (this.ParameterClass != EffectParameterClass.Vector || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		float[] vecInfo = (float[])this.Data;
		return new Vector2(vecInfo[0], vecInfo[1]);
	}

	public Vector2[] GetValueVector2Array()
	{
		if (this.ParameterClass != EffectParameterClass.Vector || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		if (this.Elements != null && this.Elements.Count > 0)
		{
			Vector2[] result = new Vector2[this.Elements.Count];
			for (int i = 0; i < this.Elements.Count; i++)
			{
				float[] v = this.Elements[i].GetValueSingleArray();
				result[i] = new Vector2(v[0], v[1]);
			}
			return result;
		}
		return null;
	}

	public Vector3 GetValueVector3()
	{
		if (this.ParameterClass != EffectParameterClass.Vector || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		float[] vecInfo = (float[])this.Data;
		return new Vector3(vecInfo[0], vecInfo[1], vecInfo[2]);
	}

	public Vector3[] GetValueVector3Array()
	{
		if (this.ParameterClass != EffectParameterClass.Vector || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		if (this.Elements != null && this.Elements.Count > 0)
		{
			Vector3[] result = new Vector3[this.Elements.Count];
			for (int i = 0; i < this.Elements.Count; i++)
			{
				float[] v = this.Elements[i].GetValueSingleArray();
				result[i] = new Vector3(v[0], v[1], v[2]);
			}
			return result;
		}
		return null;
	}

	public Vector4 GetValueVector4()
	{
		if (this.ParameterClass != EffectParameterClass.Vector || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		float[] vecInfo = (float[])this.Data;
		return new Vector4(vecInfo[0], vecInfo[1], vecInfo[2], vecInfo[3]);
	}

	public Vector4[] GetValueVector4Array()
	{
		if (this.ParameterClass != EffectParameterClass.Vector || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		if (this.Elements != null && this.Elements.Count > 0)
		{
			Vector4[] result = new Vector4[this.Elements.Count];
			for (int i = 0; i < this.Elements.Count; i++)
			{
				float[] v = this.Elements[i].GetValueSingleArray();
				result[i] = new Vector4(v[0], v[1], v[2], v[3]);
			}
			return result;
		}
		return null;
	}

	public void SetValue(bool value)
	{
		if (this.ParameterClass != EffectParameterClass.Scalar || this.ParameterType != EffectParameterType.Bool)
		{
			throw new InvalidCastException();
		}
		((float[])this.Data)[0] = (value ? 1 : 0);
		this.StateKey = EffectParameter.NextStateKey++;
	}

	public void SetValue(int value)
	{
		if (this.ParameterClass != EffectParameterClass.Scalar || this.ParameterType != EffectParameterType.Int32)
		{
			throw new InvalidCastException();
		}
		((float[])this.Data)[0] = value;
		this.StateKey = EffectParameter.NextStateKey++;
	}

	public void SetValue(int[] value)
	{
		for (int i = 0; i < value.Length; i++)
		{
			this.Elements[i].SetValue(value[i]);
		}
		this.StateKey = EffectParameter.NextStateKey++;
	}

	public void SetValue(Matrix value)
	{
		if (this.ParameterClass != EffectParameterClass.Matrix || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		if (this.RowCount == 4 && this.ColumnCount == 4)
		{
			float[] obj = (float[])this.Data;
			obj[0] = value.M11;
			obj[1] = value.M21;
			obj[2] = value.M31;
			obj[3] = value.M41;
			obj[4] = value.M12;
			obj[5] = value.M22;
			obj[6] = value.M32;
			obj[7] = value.M42;
			obj[8] = value.M13;
			obj[9] = value.M23;
			obj[10] = value.M33;
			obj[11] = value.M43;
			obj[12] = value.M14;
			obj[13] = value.M24;
			obj[14] = value.M34;
			obj[15] = value.M44;
		}
		else if (this.RowCount == 4 && this.ColumnCount == 3)
		{
			float[] obj2 = (float[])this.Data;
			obj2[0] = value.M11;
			obj2[1] = value.M21;
			obj2[2] = value.M31;
			obj2[3] = value.M41;
			obj2[4] = value.M12;
			obj2[5] = value.M22;
			obj2[6] = value.M32;
			obj2[7] = value.M42;
			obj2[8] = value.M13;
			obj2[9] = value.M23;
			obj2[10] = value.M33;
			obj2[11] = value.M43;
		}
		else if (this.RowCount == 3 && this.ColumnCount == 4)
		{
			float[] obj3 = (float[])this.Data;
			obj3[0] = value.M11;
			obj3[1] = value.M21;
			obj3[2] = value.M31;
			obj3[3] = value.M12;
			obj3[4] = value.M22;
			obj3[5] = value.M32;
			obj3[6] = value.M13;
			obj3[7] = value.M23;
			obj3[8] = value.M33;
			obj3[9] = value.M14;
			obj3[10] = value.M24;
			obj3[11] = value.M34;
		}
		else if (this.RowCount == 3 && this.ColumnCount == 3)
		{
			float[] obj4 = (float[])this.Data;
			obj4[0] = value.M11;
			obj4[1] = value.M21;
			obj4[2] = value.M31;
			obj4[3] = value.M12;
			obj4[4] = value.M22;
			obj4[5] = value.M32;
			obj4[6] = value.M13;
			obj4[7] = value.M23;
			obj4[8] = value.M33;
		}
		else if (this.RowCount == 3 && this.ColumnCount == 2)
		{
			float[] obj5 = (float[])this.Data;
			obj5[0] = value.M11;
			obj5[1] = value.M21;
			obj5[2] = value.M31;
			obj5[3] = value.M12;
			obj5[4] = value.M22;
			obj5[5] = value.M32;
		}
		this.StateKey = EffectParameter.NextStateKey++;
	}

	public void SetValueTranspose(Matrix value)
	{
		if (this.ParameterClass != EffectParameterClass.Matrix || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		if (this.RowCount == 4 && this.ColumnCount == 4)
		{
			float[] obj = (float[])this.Data;
			obj[0] = value.M11;
			obj[1] = value.M12;
			obj[2] = value.M13;
			obj[3] = value.M14;
			obj[4] = value.M21;
			obj[5] = value.M22;
			obj[6] = value.M23;
			obj[7] = value.M24;
			obj[8] = value.M31;
			obj[9] = value.M32;
			obj[10] = value.M33;
			obj[11] = value.M34;
			obj[12] = value.M41;
			obj[13] = value.M42;
			obj[14] = value.M43;
			obj[15] = value.M44;
		}
		else if (this.RowCount == 4 && this.ColumnCount == 3)
		{
			float[] obj2 = (float[])this.Data;
			obj2[0] = value.M11;
			obj2[1] = value.M12;
			obj2[2] = value.M13;
			obj2[3] = value.M21;
			obj2[4] = value.M22;
			obj2[5] = value.M23;
			obj2[6] = value.M31;
			obj2[7] = value.M32;
			obj2[8] = value.M33;
			obj2[9] = value.M41;
			obj2[10] = value.M42;
			obj2[11] = value.M43;
		}
		else if (this.RowCount == 3 && this.ColumnCount == 4)
		{
			float[] obj3 = (float[])this.Data;
			obj3[0] = value.M11;
			obj3[1] = value.M12;
			obj3[2] = value.M13;
			obj3[3] = value.M14;
			obj3[4] = value.M21;
			obj3[5] = value.M22;
			obj3[6] = value.M23;
			obj3[7] = value.M24;
			obj3[8] = value.M31;
			obj3[9] = value.M32;
			obj3[10] = value.M33;
			obj3[11] = value.M34;
		}
		else if (this.RowCount == 3 && this.ColumnCount == 3)
		{
			float[] obj4 = (float[])this.Data;
			obj4[0] = value.M11;
			obj4[1] = value.M12;
			obj4[2] = value.M13;
			obj4[3] = value.M21;
			obj4[4] = value.M22;
			obj4[5] = value.M23;
			obj4[6] = value.M31;
			obj4[7] = value.M32;
			obj4[8] = value.M33;
		}
		else if (this.RowCount == 3 && this.ColumnCount == 2)
		{
			float[] obj5 = (float[])this.Data;
			obj5[0] = value.M11;
			obj5[1] = value.M12;
			obj5[2] = value.M13;
			obj5[3] = value.M21;
			obj5[4] = value.M22;
			obj5[5] = value.M23;
		}
		this.StateKey = EffectParameter.NextStateKey++;
	}

	public void SetValue(Matrix[] value)
	{
		if (this.ParameterClass != EffectParameterClass.Matrix || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		if (this.RowCount == 4 && this.ColumnCount == 4)
		{
			for (int i = 0; i < value.Length; i++)
			{
				float[] obj = (float[])this.Elements[i].Data;
				obj[0] = value[i].M11;
				obj[1] = value[i].M21;
				obj[2] = value[i].M31;
				obj[3] = value[i].M41;
				obj[4] = value[i].M12;
				obj[5] = value[i].M22;
				obj[6] = value[i].M32;
				obj[7] = value[i].M42;
				obj[8] = value[i].M13;
				obj[9] = value[i].M23;
				obj[10] = value[i].M33;
				obj[11] = value[i].M43;
				obj[12] = value[i].M14;
				obj[13] = value[i].M24;
				obj[14] = value[i].M34;
				obj[15] = value[i].M44;
			}
		}
		else if (this.RowCount == 4 && this.ColumnCount == 3)
		{
			for (int j = 0; j < value.Length; j++)
			{
				float[] obj2 = (float[])this.Elements[j].Data;
				obj2[0] = value[j].M11;
				obj2[1] = value[j].M21;
				obj2[2] = value[j].M31;
				obj2[3] = value[j].M41;
				obj2[4] = value[j].M12;
				obj2[5] = value[j].M22;
				obj2[6] = value[j].M32;
				obj2[7] = value[j].M42;
				obj2[8] = value[j].M13;
				obj2[9] = value[j].M23;
				obj2[10] = value[j].M33;
				obj2[11] = value[j].M43;
			}
		}
		else if (this.RowCount == 3 && this.ColumnCount == 4)
		{
			for (int k = 0; k < value.Length; k++)
			{
				float[] obj3 = (float[])this.Elements[k].Data;
				obj3[0] = value[k].M11;
				obj3[1] = value[k].M21;
				obj3[2] = value[k].M31;
				obj3[3] = value[k].M12;
				obj3[4] = value[k].M22;
				obj3[5] = value[k].M32;
				obj3[6] = value[k].M13;
				obj3[7] = value[k].M23;
				obj3[8] = value[k].M33;
				obj3[9] = value[k].M14;
				obj3[10] = value[k].M24;
				obj3[11] = value[k].M34;
			}
		}
		else if (this.RowCount == 3 && this.ColumnCount == 3)
		{
			for (int l = 0; l < value.Length; l++)
			{
				float[] obj4 = (float[])this.Elements[l].Data;
				obj4[0] = value[l].M11;
				obj4[1] = value[l].M21;
				obj4[2] = value[l].M31;
				obj4[3] = value[l].M12;
				obj4[4] = value[l].M22;
				obj4[5] = value[l].M32;
				obj4[6] = value[l].M13;
				obj4[7] = value[l].M23;
				obj4[8] = value[l].M33;
			}
		}
		else if (this.RowCount == 3 && this.ColumnCount == 2)
		{
			for (int m = 0; m < value.Length; m++)
			{
				float[] obj5 = (float[])this.Elements[m].Data;
				obj5[0] = value[m].M11;
				obj5[1] = value[m].M21;
				obj5[2] = value[m].M31;
				obj5[3] = value[m].M12;
				obj5[4] = value[m].M22;
				obj5[5] = value[m].M32;
			}
		}
		this.StateKey = EffectParameter.NextStateKey++;
	}

	public void SetValue(Quaternion value)
	{
		if (this.ParameterClass != EffectParameterClass.Vector || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		float[] obj = (float[])this.Data;
		obj[0] = value.X;
		obj[1] = value.Y;
		obj[2] = value.Z;
		obj[3] = value.W;
		this.StateKey = EffectParameter.NextStateKey++;
	}

	public void SetValue(float value)
	{
		if (this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		((float[])this.Data)[0] = value;
		this.StateKey = EffectParameter.NextStateKey++;
	}

	public void SetValue(float[] value)
	{
		for (int i = 0; i < value.Length; i++)
		{
			this.Elements[i].SetValue(value[i]);
		}
		this.StateKey = EffectParameter.NextStateKey++;
	}

	public void SetValue(Texture value)
	{
		if (this.ParameterType != EffectParameterType.Texture && this.ParameterType != EffectParameterType.Texture1D && this.ParameterType != EffectParameterType.Texture2D && this.ParameterType != EffectParameterType.Texture3D && this.ParameterType != EffectParameterType.TextureCube)
		{
			throw new InvalidCastException();
		}
		this.Data = value;
		this.StateKey = EffectParameter.NextStateKey++;
	}

	public void SetValue(Vector2 value)
	{
		if (this.ParameterClass != EffectParameterClass.Vector || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		float[] obj = (float[])this.Data;
		obj[0] = value.X;
		obj[1] = value.Y;
		this.StateKey = EffectParameter.NextStateKey++;
	}

	public void SetValue(Vector2[] value)
	{
		for (int i = 0; i < value.Length; i++)
		{
			this.Elements[i].SetValue(value[i]);
		}
		this.StateKey = EffectParameter.NextStateKey++;
	}

	public void SetValue(Vector3 value)
	{
		if (this.ParameterClass != EffectParameterClass.Vector || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		float[] obj = (float[])this.Data;
		obj[0] = value.X;
		obj[1] = value.Y;
		obj[2] = value.Z;
		this.StateKey = EffectParameter.NextStateKey++;
	}

	public void SetValue(Vector3[] value)
	{
		for (int i = 0; i < value.Length; i++)
		{
			this.Elements[i].SetValue(value[i]);
		}
		this.StateKey = EffectParameter.NextStateKey++;
	}

	public void SetValue(Vector4 value)
	{
		if (this.ParameterClass != EffectParameterClass.Vector || this.ParameterType != EffectParameterType.Single)
		{
			throw new InvalidCastException();
		}
		float[] obj = (float[])this.Data;
		obj[0] = value.X;
		obj[1] = value.Y;
		obj[2] = value.Z;
		obj[3] = value.W;
		this.StateKey = EffectParameter.NextStateKey++;
	}

	public void SetValue(Vector4[] value)
	{
		for (int i = 0; i < value.Length; i++)
		{
			this.Elements[i].SetValue(value[i]);
		}
		this.StateKey = EffectParameter.NextStateKey++;
	}
}
