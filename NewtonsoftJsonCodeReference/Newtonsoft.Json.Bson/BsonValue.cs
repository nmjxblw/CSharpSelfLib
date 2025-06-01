namespace Newtonsoft.Json.Bson;

internal class BsonValue : BsonToken
{
	private readonly object _value;

	private readonly BsonType _type;

	public object Value => this._value;

	public override BsonType Type => this._type;

	public BsonValue(object value, BsonType type)
	{
		this._value = value;
		this._type = type;
	}
}
