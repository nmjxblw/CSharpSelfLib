using System;
using System.Collections.Generic;
using System.Linq;

namespace Newtonsoft.Json.Schema;

/// <summary>
/// <para>
/// Resolves <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> from an id.
/// </para>
/// <note type="caution">
/// JSON Schema validation has been moved to its own package. See <see href="https://www.newtonsoft.com/jsonschema">https://www.newtonsoft.com/jsonschema</see> for more details.
/// </note>
/// </summary>
[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
public class JsonSchemaResolver
{
	/// <summary>
	/// Gets or sets the loaded schemas.
	/// </summary>
	/// <value>The loaded schemas.</value>
	public IList<JsonSchema> LoadedSchemas { get; protected set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Schema.JsonSchemaResolver" /> class.
	/// </summary>
	public JsonSchemaResolver()
	{
		this.LoadedSchemas = new List<JsonSchema>();
	}

	/// <summary>
	/// Gets a <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> for the specified reference.
	/// </summary>
	/// <param name="reference">The id.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> for the specified reference.</returns>
	public virtual JsonSchema GetSchema(string reference)
	{
		JsonSchema jsonSchema = this.LoadedSchemas.SingleOrDefault((JsonSchema s) => string.Equals(s.Id, reference, StringComparison.Ordinal));
		if (jsonSchema == null)
		{
			jsonSchema = this.LoadedSchemas.SingleOrDefault((JsonSchema s) => string.Equals(s.Location, reference, StringComparison.Ordinal));
		}
		return jsonSchema;
	}
}
