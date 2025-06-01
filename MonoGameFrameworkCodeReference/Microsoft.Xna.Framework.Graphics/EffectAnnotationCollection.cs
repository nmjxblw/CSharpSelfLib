using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics;

public class EffectAnnotationCollection : IEnumerable<EffectAnnotation>, IEnumerable
{
	internal static readonly EffectAnnotationCollection Empty = new EffectAnnotationCollection(new EffectAnnotation[0]);

	private readonly EffectAnnotation[] _annotations;

	public int Count => this._annotations.Length;

	public EffectAnnotation this[int index] => this._annotations[index];

	public EffectAnnotation this[string name]
	{
		get
		{
			EffectAnnotation[] annotations = this._annotations;
			foreach (EffectAnnotation annotation in annotations)
			{
				if (annotation.Name == name)
				{
					return annotation;
				}
			}
			return null;
		}
	}

	internal EffectAnnotationCollection(EffectAnnotation[] annotations)
	{
		this._annotations = annotations;
	}

	public IEnumerator<EffectAnnotation> GetEnumerator()
	{
		return ((IEnumerable<EffectAnnotation>)this._annotations).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this._annotations.GetEnumerator();
	}
}
