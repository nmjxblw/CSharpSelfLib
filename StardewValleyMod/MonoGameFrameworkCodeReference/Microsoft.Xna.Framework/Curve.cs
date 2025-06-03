using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Contains a collection of <see cref="T:Microsoft.Xna.Framework.CurveKey" /> points in 2D space and provides methods for evaluating features of the curve they define.
/// </summary>
[DataContract]
public class Curve
{
	private CurveLoopType _preLoop;

	private CurveLoopType _postLoop;

	private CurveKeyCollection _keys;

	/// <summary>
	/// Returns <c>true</c> if this curve is constant (has zero or one points); <c>false</c> otherwise.
	/// </summary>
	[DataMember]
	public bool IsConstant => this._keys.Count <= 1;

	/// <summary>
	/// Defines how to handle weighting values that are less than the first control point in the curve.
	/// </summary>
	[DataMember]
	public CurveLoopType PreLoop
	{
		get
		{
			return this._preLoop;
		}
		set
		{
			this._preLoop = value;
		}
	}

	/// <summary>
	/// Defines how to handle weighting values that are greater than the last control point in the curve.
	/// </summary>
	[DataMember]
	public CurveLoopType PostLoop
	{
		get
		{
			return this._postLoop;
		}
		set
		{
			this._postLoop = value;
		}
	}

	/// <summary>
	/// The collection of curve keys.
	/// </summary>
	[DataMember]
	public CurveKeyCollection Keys => this._keys;

	/// <summary>
	/// Constructs a curve.
	/// </summary>
	public Curve()
	{
		this._keys = new CurveKeyCollection();
	}

	/// <summary>
	/// Creates a copy of this curve.
	/// </summary>
	/// <returns>A copy of this curve.</returns>
	public Curve Clone()
	{
		return new Curve
		{
			_keys = this._keys.Clone(),
			_preLoop = this._preLoop,
			_postLoop = this._postLoop
		};
	}

	/// <summary>
	/// Evaluate the value at a position of this <see cref="T:Microsoft.Xna.Framework.Curve" />.
	/// </summary>
	/// <param name="position">The position on this <see cref="T:Microsoft.Xna.Framework.Curve" />.</param>
	/// <returns>Value at the position on this <see cref="T:Microsoft.Xna.Framework.Curve" />.</returns>
	public float Evaluate(float position)
	{
		if (this._keys.Count == 0)
		{
			return 0f;
		}
		if (this._keys.Count == 1)
		{
			return this._keys[0].Value;
		}
		CurveKey first = this._keys[0];
		CurveKey last = this._keys[this._keys.Count - 1];
		if (position < first.Position)
		{
			switch (this.PreLoop)
			{
			case CurveLoopType.Constant:
				return first.Value;
			case CurveLoopType.Linear:
				return first.Value - first.TangentIn * (first.Position - position);
			case CurveLoopType.Cycle:
			{
				int cycle = this.GetNumberOfCycle(position);
				float virtualPos = position - (float)cycle * (last.Position - first.Position);
				return this.GetCurvePosition(virtualPos);
			}
			case CurveLoopType.CycleOffset:
			{
				int cycle = this.GetNumberOfCycle(position);
				float virtualPos = position - (float)cycle * (last.Position - first.Position);
				return this.GetCurvePosition(virtualPos) + (float)cycle * (last.Value - first.Value);
			}
			case CurveLoopType.Oscillate:
			{
				int cycle = this.GetNumberOfCycle(position);
				float virtualPos = ((0f != (float)cycle % 2f) ? (last.Position - position + first.Position + (float)cycle * (last.Position - first.Position)) : (position - (float)cycle * (last.Position - first.Position)));
				return this.GetCurvePosition(virtualPos);
			}
			}
		}
		else if (position > last.Position)
		{
			switch (this.PostLoop)
			{
			case CurveLoopType.Constant:
				return last.Value;
			case CurveLoopType.Linear:
				return last.Value + first.TangentOut * (position - last.Position);
			case CurveLoopType.Cycle:
			{
				int cycle2 = this.GetNumberOfCycle(position);
				float virtualPos2 = position - (float)cycle2 * (last.Position - first.Position);
				return this.GetCurvePosition(virtualPos2);
			}
			case CurveLoopType.CycleOffset:
			{
				int cycle2 = this.GetNumberOfCycle(position);
				float virtualPos2 = position - (float)cycle2 * (last.Position - first.Position);
				return this.GetCurvePosition(virtualPos2) + (float)cycle2 * (last.Value - first.Value);
			}
			case CurveLoopType.Oscillate:
			{
				int cycle2 = this.GetNumberOfCycle(position);
				float virtualPos2 = position - (float)cycle2 * (last.Position - first.Position);
				virtualPos2 = ((0f != (float)cycle2 % 2f) ? (last.Position - position + first.Position + (float)cycle2 * (last.Position - first.Position)) : (position - (float)cycle2 * (last.Position - first.Position)));
				return this.GetCurvePosition(virtualPos2);
			}
			}
		}
		return this.GetCurvePosition(position);
	}

	/// <summary>
	/// Computes tangents for all keys in the collection.
	/// </summary>
	/// <param name="tangentType">The tangent type for both in and out.</param>
	public void ComputeTangents(CurveTangent tangentType)
	{
		this.ComputeTangents(tangentType, tangentType);
	}

	/// <summary>
	/// Computes tangents for all keys in the collection.
	/// </summary>
	/// <param name="tangentInType">The tangent in-type. <see cref="P:Microsoft.Xna.Framework.CurveKey.TangentIn" /> for more details.</param>
	/// <param name="tangentOutType">The tangent out-type. <see cref="P:Microsoft.Xna.Framework.CurveKey.TangentOut" /> for more details.</param>
	public void ComputeTangents(CurveTangent tangentInType, CurveTangent tangentOutType)
	{
		for (int i = 0; i < this.Keys.Count; i++)
		{
			this.ComputeTangent(i, tangentInType, tangentOutType);
		}
	}

	/// <summary>
	/// Computes tangent for the specific key in the collection.
	/// </summary>
	/// <param name="keyIndex">The index of a key in the collection.</param>
	/// <param name="tangentType">The tangent type for both in and out.</param>
	public void ComputeTangent(int keyIndex, CurveTangent tangentType)
	{
		this.ComputeTangent(keyIndex, tangentType, tangentType);
	}

	/// <summary>
	/// Computes tangent for the specific key in the collection.
	/// </summary>
	/// <param name="keyIndex">The index of key in the collection.</param>
	/// <param name="tangentInType">The tangent in-type. <see cref="P:Microsoft.Xna.Framework.CurveKey.TangentIn" /> for more details.</param>
	/// <param name="tangentOutType">The tangent out-type. <see cref="P:Microsoft.Xna.Framework.CurveKey.TangentOut" /> for more details.</param>
	public void ComputeTangent(int keyIndex, CurveTangent tangentInType, CurveTangent tangentOutType)
	{
		CurveKey key = this._keys[keyIndex];
		float p1;
		float p2;
		float p0 = (p1 = (p2 = key.Position));
		float v1;
		float v2;
		float v0 = (v1 = (v2 = key.Value));
		if (keyIndex > 0)
		{
			p0 = this._keys[keyIndex - 1].Position;
			v0 = this._keys[keyIndex - 1].Value;
		}
		if (keyIndex < this._keys.Count - 1)
		{
			p2 = this._keys[keyIndex + 1].Position;
			v2 = this._keys[keyIndex + 1].Value;
		}
		switch (tangentInType)
		{
		case CurveTangent.Flat:
			key.TangentIn = 0f;
			break;
		case CurveTangent.Linear:
			key.TangentIn = v1 - v0;
			break;
		case CurveTangent.Smooth:
		{
			float pn = p2 - p0;
			if (Math.Abs(pn) < float.Epsilon)
			{
				key.TangentIn = 0f;
			}
			else
			{
				key.TangentIn = (v2 - v0) * ((p1 - p0) / pn);
			}
			break;
		}
		}
		switch (tangentOutType)
		{
		case CurveTangent.Flat:
			key.TangentOut = 0f;
			break;
		case CurveTangent.Linear:
			key.TangentOut = v2 - v1;
			break;
		case CurveTangent.Smooth:
		{
			float pn2 = p2 - p0;
			if (Math.Abs(pn2) < float.Epsilon)
			{
				key.TangentOut = 0f;
			}
			else
			{
				key.TangentOut = (v2 - v0) * ((p2 - p1) / pn2);
			}
			break;
		}
		}
	}

	private int GetNumberOfCycle(float position)
	{
		float cycle = (position - this._keys[0].Position) / (this._keys[this._keys.Count - 1].Position - this._keys[0].Position);
		if (cycle < 0f)
		{
			cycle -= 1f;
		}
		return (int)cycle;
	}

	private float GetCurvePosition(float position)
	{
		CurveKey prev = this._keys[0];
		for (int i = 1; i < this._keys.Count; i++)
		{
			CurveKey next = this.Keys[i];
			if (next.Position >= position)
			{
				if (prev.Continuity == CurveContinuity.Step)
				{
					if (position >= 1f)
					{
						return next.Value;
					}
					return prev.Value;
				}
				float t = (position - prev.Position) / (next.Position - prev.Position);
				float ts = t * t;
				float tss = ts * t;
				return (2f * tss - 3f * ts + 1f) * prev.Value + (tss - 2f * ts + t) * prev.TangentOut + (3f * ts - 2f * tss) * next.Value + (tss - ts) * next.TangentIn;
			}
			prev = next;
		}
		return 0f;
	}
}
