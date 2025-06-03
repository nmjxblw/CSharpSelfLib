using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics;

public class EffectParameterCollection : IEnumerable<EffectParameter>, IEnumerable
{
	internal static readonly EffectParameterCollection Empty = new EffectParameterCollection(new EffectParameter[0]);

	private readonly EffectParameter[] _parameters;

	private readonly Dictionary<string, int> _indexLookup;

	public int Count => this._parameters.Length;

	public EffectParameter this[int index] => this._parameters[index];

	public EffectParameter this[string name]
	{
		get
		{
			if (this._indexLookup.TryGetValue(name, out var index))
			{
				return this._parameters[index];
			}
			return null;
		}
	}

	internal EffectParameterCollection(EffectParameter[] parameters)
	{
		this._parameters = parameters;
		this._indexLookup = new Dictionary<string, int>(this._parameters.Length);
		for (int i = 0; i < this._parameters.Length; i++)
		{
			string name = this._parameters[i].Name;
			if (!string.IsNullOrWhiteSpace(name))
			{
				this._indexLookup.Add(name, i);
			}
		}
	}

	private EffectParameterCollection(EffectParameter[] parameters, Dictionary<string, int> indexLookup)
	{
		this._parameters = parameters;
		this._indexLookup = indexLookup;
	}

	internal EffectParameterCollection Clone()
	{
		if (this._parameters.Length == 0)
		{
			return EffectParameterCollection.Empty;
		}
		EffectParameter[] parameters = new EffectParameter[this._parameters.Length];
		for (int i = 0; i < this._parameters.Length; i++)
		{
			parameters[i] = new EffectParameter(this._parameters[i]);
		}
		return new EffectParameterCollection(parameters, this._indexLookup);
	}

	public IEnumerator<EffectParameter> GetEnumerator()
	{
		return ((IEnumerable<EffectParameter>)this._parameters).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this._parameters.GetEnumerator();
	}
}
