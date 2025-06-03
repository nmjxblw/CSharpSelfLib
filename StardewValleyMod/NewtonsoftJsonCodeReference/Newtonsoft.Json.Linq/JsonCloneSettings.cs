namespace Newtonsoft.Json.Linq;

/// <summary>
/// Specifies the settings used when cloning JSON.
/// </summary>
public class JsonCloneSettings
{
	internal static readonly JsonCloneSettings SkipCopyAnnotations = new JsonCloneSettings
	{
		CopyAnnotations = false
	};

	/// <summary>
	/// Gets or sets a flag that indicates whether to copy annotations when cloning a <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// The default value is <c>true</c>.
	/// </summary>
	/// <value>
	/// A flag that indicates whether to copy annotations when cloning a <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </value>
	public bool CopyAnnotations { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JsonCloneSettings" /> class.
	/// </summary>
	public JsonCloneSettings()
	{
		this.CopyAnnotations = true;
	}
}
