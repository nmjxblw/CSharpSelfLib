using System;

namespace Newtonsoft.Json.Schema;

/// <summary>
/// <para>
/// The value types allowed by the <see cref="T:Newtonsoft.Json.Schema.JsonSchema" />.
/// </para>
/// <note type="caution">
/// JSON Schema validation has been moved to its own package. See <see href="https://www.newtonsoft.com/jsonschema">https://www.newtonsoft.com/jsonschema</see> for more details.
/// </note>
/// </summary>
[Flags]
[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
public enum JsonSchemaType
{
	/// <summary>
	/// No type specified.
	/// </summary>
	None = 0,
	/// <summary>
	/// String type.
	/// </summary>
	String = 1,
	/// <summary>
	/// Float type.
	/// </summary>
	Float = 2,
	/// <summary>
	/// Integer type.
	/// </summary>
	Integer = 4,
	/// <summary>
	/// Boolean type.
	/// </summary>
	Boolean = 8,
	/// <summary>
	/// Object type.
	/// </summary>
	Object = 0x10,
	/// <summary>
	/// Array type.
	/// </summary>
	Array = 0x20,
	/// <summary>
	/// Null type.
	/// </summary>
	Null = 0x40,
	/// <summary>
	/// Any type.
	/// </summary>
	Any = 0x7F
}
