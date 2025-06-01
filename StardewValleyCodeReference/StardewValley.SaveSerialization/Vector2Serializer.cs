using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace StardewValley.SaveSerialization;

public class Vector2Serializer : XmlSerializer
{
	private Vector2Reader _reader = new Vector2Reader();

	private Vector2Writer _writer = new Vector2Writer();

	public Vector2Serializer()
		: base(typeof(Vector2))
	{
	}

	protected override XmlSerializationReader CreateReader()
	{
		return this._reader;
	}

	protected override XmlSerializationWriter CreateWriter()
	{
		return this._writer;
	}

	public override bool CanDeserialize(XmlReader xmlReader)
	{
		return xmlReader.IsStartElement("Vector2");
	}

	protected override void Serialize(object o, XmlSerializationWriter writer)
	{
		this._writer.WriteVector2((Vector2)o);
	}

	protected override object Deserialize(XmlSerializationReader reader)
	{
		return this._reader.ReadVector2();
	}
}
