using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics;

public class EffectTechniqueCollection : IEnumerable<EffectTechnique>, IEnumerable
{
	private readonly EffectTechnique[] _techniques;

	public int Count => this._techniques.Length;

	public EffectTechnique this[int index] => this._techniques[index];

	public EffectTechnique this[string name]
	{
		get
		{
			EffectTechnique[] techniques = this._techniques;
			foreach (EffectTechnique technique in techniques)
			{
				if (technique.Name == name)
				{
					return technique;
				}
			}
			return null;
		}
	}

	internal EffectTechniqueCollection(EffectTechnique[] techniques)
	{
		this._techniques = techniques;
	}

	internal EffectTechniqueCollection Clone(Effect effect)
	{
		EffectTechnique[] techniques = new EffectTechnique[this._techniques.Length];
		for (int i = 0; i < this._techniques.Length; i++)
		{
			techniques[i] = new EffectTechnique(effect, this._techniques[i]);
		}
		return new EffectTechniqueCollection(techniques);
	}

	public IEnumerator<EffectTechnique> GetEnumerator()
	{
		return ((IEnumerable<EffectTechnique>)this._techniques).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this._techniques.GetEnumerator();
	}
}
