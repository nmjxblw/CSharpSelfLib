using System.Xml.Linq;

namespace Newtonsoft.Json.Converters;

internal class XTextWrapper : XObjectWrapper
{
	private XText Text => (XText)base.WrappedNode;

	public override string? Value
	{
		get
		{
			return this.Text.Value;
		}
		set
		{
			this.Text.Value = value ?? string.Empty;
		}
	}

	public override IXmlNode? ParentNode
	{
		get
		{
			if (this.Text.Parent == null)
			{
				return null;
			}
			return XContainerWrapper.WrapNode(this.Text.Parent);
		}
	}

	public XTextWrapper(XText text)
		: base(text)
	{
	}
}
