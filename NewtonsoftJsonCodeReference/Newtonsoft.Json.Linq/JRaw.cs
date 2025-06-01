using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Linq;

/// <summary>
/// Represents a raw JSON string.
/// </summary>
public class JRaw : JValue
{
	/// <summary>
	/// Asynchronously creates an instance of <see cref="T:Newtonsoft.Json.Linq.JRaw" /> with the content of the reader's current token.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the asynchronous creation. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns an instance of <see cref="T:Newtonsoft.Json.Linq.JRaw" /> with the content of the reader's current token.</returns>
	public static async Task<JRaw> CreateAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
	{
		using StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
		using JsonTextWriter jsonWriter = new JsonTextWriter(sw);
		await jsonWriter.WriteTokenSyncReadingAsync(reader, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return new JRaw(sw.ToString());
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JRaw" /> class from another <see cref="T:Newtonsoft.Json.Linq.JRaw" /> object.
	/// </summary>
	/// <param name="other">A <see cref="T:Newtonsoft.Json.Linq.JRaw" /> object to copy from.</param>
	public JRaw(JRaw other)
		: base(other, null)
	{
	}

	internal JRaw(JRaw other, JsonCloneSettings? settings)
		: base(other, settings)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JRaw" /> class.
	/// </summary>
	/// <param name="rawJson">The raw json.</param>
	public JRaw(object? rawJson)
		: base(rawJson, JTokenType.Raw)
	{
	}

	/// <summary>
	/// Creates an instance of <see cref="T:Newtonsoft.Json.Linq.JRaw" /> with the content of the reader's current token.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <returns>An instance of <see cref="T:Newtonsoft.Json.Linq.JRaw" /> with the content of the reader's current token.</returns>
	public static JRaw Create(JsonReader reader)
	{
		using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		using JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter);
		jsonTextWriter.WriteToken(reader);
		return new JRaw(stringWriter.ToString());
	}

	internal override JToken CloneToken(JsonCloneSettings? settings)
	{
		return new JRaw(this, settings);
	}
}
