namespace Newtonsoft.Json.Bson;

internal class BsonRegex : BsonToken
{
	public BsonString Pattern { get; set; }

	public BsonString Options { get; set; }

	public override BsonType Type => BsonType.Regex;

	public BsonRegex(string pattern, string options)
	{
		this.Pattern = new BsonString(pattern, includeLength: false);
		this.Options = new BsonString(options, includeLength: false);
	}
}
