using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Schema;

/// <summary>
/// <para>
/// Contains the JSON schema extension methods.
/// </para>
/// <note type="caution">
/// JSON Schema validation has been moved to its own package. See <see href="https://www.newtonsoft.com/jsonschema">https://www.newtonsoft.com/jsonschema</see> for more details.
/// </note>
/// </summary>
[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
public static class Extensions
{
	/// <summary>
	/// <para>
	/// Determines whether the <see cref="T:Newtonsoft.Json.Linq.JToken" /> is valid.
	/// </para>
	/// <note type="caution">
	/// JSON Schema validation has been moved to its own package. See <see href="https://www.newtonsoft.com/jsonschema">https://www.newtonsoft.com/jsonschema</see> for more details.
	/// </note>
	/// </summary>
	/// <param name="source">The source <see cref="T:Newtonsoft.Json.Linq.JToken" /> to test.</param>
	/// <param name="schema">The schema to test with.</param>
	/// <returns>
	/// 	<c>true</c> if the specified <see cref="T:Newtonsoft.Json.Linq.JToken" /> is valid; otherwise, <c>false</c>.
	/// </returns>
	[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
	public static bool IsValid(this JToken source, JsonSchema schema)
	{
		bool valid = true;
		source.Validate(schema, delegate
		{
			valid = false;
		});
		return valid;
	}

	/// <summary>
	/// <para>
	/// Determines whether the <see cref="T:Newtonsoft.Json.Linq.JToken" /> is valid.
	/// </para>
	/// <note type="caution">
	/// JSON Schema validation has been moved to its own package. See <see href="https://www.newtonsoft.com/jsonschema">https://www.newtonsoft.com/jsonschema</see> for more details.
	/// </note>
	/// </summary>
	/// <param name="source">The source <see cref="T:Newtonsoft.Json.Linq.JToken" /> to test.</param>
	/// <param name="schema">The schema to test with.</param>
	/// <param name="errorMessages">When this method returns, contains any error messages generated while validating. </param>
	/// <returns>
	/// 	<c>true</c> if the specified <see cref="T:Newtonsoft.Json.Linq.JToken" /> is valid; otherwise, <c>false</c>.
	/// </returns>
	[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
	public static bool IsValid(this JToken source, JsonSchema schema, out IList<string> errorMessages)
	{
		IList<string> errors = new List<string>();
		source.Validate(schema, delegate(object sender, ValidationEventArgs args)
		{
			errors.Add(args.Message);
		});
		errorMessages = errors;
		return errorMessages.Count == 0;
	}

	/// <summary>
	/// <para>
	/// Validates the specified <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </para>
	/// <note type="caution">
	/// JSON Schema validation has been moved to its own package. See <see href="https://www.newtonsoft.com/jsonschema">https://www.newtonsoft.com/jsonschema</see> for more details.
	/// </note>
	/// </summary>
	/// <param name="source">The source <see cref="T:Newtonsoft.Json.Linq.JToken" /> to test.</param>
	/// <param name="schema">The schema to test with.</param>
	[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
	public static void Validate(this JToken source, JsonSchema schema)
	{
		source.Validate(schema, null);
	}

	/// <summary>
	/// <para>
	/// Validates the specified <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </para>
	/// <note type="caution">
	/// JSON Schema validation has been moved to its own package. See <see href="https://www.newtonsoft.com/jsonschema">https://www.newtonsoft.com/jsonschema</see> for more details.
	/// </note>
	/// </summary>
	/// <param name="source">The source <see cref="T:Newtonsoft.Json.Linq.JToken" /> to test.</param>
	/// <param name="schema">The schema to test with.</param>
	/// <param name="validationEventHandler">The validation event handler.</param>
	[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
	public static void Validate(this JToken source, JsonSchema schema, ValidationEventHandler validationEventHandler)
	{
		ValidationUtils.ArgumentNotNull(source, "source");
		ValidationUtils.ArgumentNotNull(schema, "schema");
		using JsonValidatingReader jsonValidatingReader = new JsonValidatingReader(source.CreateReader());
		jsonValidatingReader.Schema = schema;
		if (validationEventHandler != null)
		{
			jsonValidatingReader.ValidationEventHandler += validationEventHandler;
		}
		while (jsonValidatingReader.Read())
		{
		}
	}
}
