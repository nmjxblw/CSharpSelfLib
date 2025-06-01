using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

internal class DefaultReferenceResolver : IReferenceResolver
{
	private int _referenceCount;

	private BidirectionalDictionary<string, object> GetMappings(object context)
	{
		JsonSerializerInternalBase jsonSerializerInternalBase = context as JsonSerializerInternalBase;
		if (jsonSerializerInternalBase == null)
		{
			if (!(context is JsonSerializerProxy jsonSerializerProxy))
			{
				throw new JsonException("The DefaultReferenceResolver can only be used internally.");
			}
			jsonSerializerInternalBase = jsonSerializerProxy.GetInternalSerializer();
		}
		return jsonSerializerInternalBase.DefaultReferenceMappings;
	}

	public object ResolveReference(object context, string reference)
	{
		this.GetMappings(context).TryGetByFirst(reference, out object second);
		return second;
	}

	public string GetReference(object context, object value)
	{
		BidirectionalDictionary<string, object> mappings = this.GetMappings(context);
		if (!mappings.TryGetBySecond(value, out string first))
		{
			this._referenceCount++;
			first = this._referenceCount.ToString(CultureInfo.InvariantCulture);
			mappings.Set(first, value);
		}
		return first;
	}

	public void AddReference(object context, string reference, object value)
	{
		this.GetMappings(context).Set(reference, value);
	}

	public bool IsReferenced(object context, object value)
	{
		string first;
		return this.GetMappings(context).TryGetBySecond(value, out first);
	}
}
