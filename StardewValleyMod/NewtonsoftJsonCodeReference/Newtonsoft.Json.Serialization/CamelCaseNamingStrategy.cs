using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

/// <summary>
/// A camel case naming strategy.
/// </summary>
public class CamelCaseNamingStrategy : NamingStrategy
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.CamelCaseNamingStrategy" /> class.
	/// </summary>
	/// <param name="processDictionaryKeys">
	/// A flag indicating whether dictionary keys should be processed.
	/// </param>
	/// <param name="overrideSpecifiedNames">
	/// A flag indicating whether explicitly specified property names should be processed,
	/// e.g. a property name customized with a <see cref="T:Newtonsoft.Json.JsonPropertyAttribute" />.
	/// </param>
	public CamelCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames)
	{
		base.ProcessDictionaryKeys = processDictionaryKeys;
		base.OverrideSpecifiedNames = overrideSpecifiedNames;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.CamelCaseNamingStrategy" /> class.
	/// </summary>
	/// <param name="processDictionaryKeys">
	/// A flag indicating whether dictionary keys should be processed.
	/// </param>
	/// <param name="overrideSpecifiedNames">
	/// A flag indicating whether explicitly specified property names should be processed,
	/// e.g. a property name customized with a <see cref="T:Newtonsoft.Json.JsonPropertyAttribute" />.
	/// </param>
	/// <param name="processExtensionDataNames">
	/// A flag indicating whether extension data names should be processed.
	/// </param>
	public CamelCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames, bool processExtensionDataNames)
		: this(processDictionaryKeys, overrideSpecifiedNames)
	{
		base.ProcessExtensionDataNames = processExtensionDataNames;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.CamelCaseNamingStrategy" /> class.
	/// </summary>
	public CamelCaseNamingStrategy()
	{
	}

	/// <summary>
	/// Resolves the specified property name.
	/// </summary>
	/// <param name="name">The property name to resolve.</param>
	/// <returns>The resolved property name.</returns>
	protected override string ResolvePropertyName(string name)
	{
		return StringUtils.ToCamelCase(name);
	}
}
