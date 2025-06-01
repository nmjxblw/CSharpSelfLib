using System.Xml;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters;

internal class XDeclarationWrapper : XObjectWrapper, IXmlDeclaration, IXmlNode
{
	internal XDeclaration Declaration { get; }

	public override XmlNodeType NodeType => XmlNodeType.XmlDeclaration;

	public string? Version => this.Declaration.Version;

	public string? Encoding
	{
		get
		{
			return this.Declaration.Encoding;
		}
		set
		{
			this.Declaration.Encoding = value;
		}
	}

	public string? Standalone
	{
		get
		{
			return this.Declaration.Standalone;
		}
		set
		{
			this.Declaration.Standalone = value;
		}
	}

	public XDeclarationWrapper(XDeclaration declaration)
		: base(null)
	{
		this.Declaration = declaration;
	}
}
