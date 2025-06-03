using System.Xml.Linq;

namespace Newtonsoft.Json.Converters;

internal class XAttributeWrapper : XObjectWrapper
{
	private XAttribute Attribute => (XAttribute)base.WrappedNode;

	public override string? Value
	{
		get
		{
			return this.Attribute.Value;
		}
		set
		{
			this.Attribute.Value = value ?? string.Empty;
		}
	}

	public override string? LocalName => this.Attribute.Name.LocalName;

	public override string? NamespaceUri => this.Attribute.Name.NamespaceName;

	public override IXmlNode? ParentNode
	{
		get
		{
			if (this.Attribute.Parent == null)
			{
				return null;
			}
			return XContainerWrapper.WrapNode(this.Attribute.Parent);
		}
	}

	public XAttributeWrapper(XAttribute attribute)
		: base(attribute)
	{
	}
}
