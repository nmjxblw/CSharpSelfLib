using System.Collections.Generic;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters;

internal class XContainerWrapper : XObjectWrapper
{
	private List<IXmlNode>? _childNodes;

	private XContainer Container => (XContainer)base.WrappedNode;

	public override List<IXmlNode> ChildNodes
	{
		get
		{
			if (this._childNodes == null)
			{
				if (!this.HasChildNodes)
				{
					this._childNodes = XmlNodeConverter.EmptyChildNodes;
				}
				else
				{
					this._childNodes = new List<IXmlNode>();
					foreach (XNode item in this.Container.Nodes())
					{
						this._childNodes.Add(XContainerWrapper.WrapNode(item));
					}
				}
			}
			return this._childNodes;
		}
	}

	protected virtual bool HasChildNodes => this.Container.LastNode != null;

	public override IXmlNode? ParentNode
	{
		get
		{
			if (this.Container.Parent == null)
			{
				return null;
			}
			return XContainerWrapper.WrapNode(this.Container.Parent);
		}
	}

	public XContainerWrapper(XContainer container)
		: base(container)
	{
	}

	internal static IXmlNode WrapNode(XObject node)
	{
		if (node is XDocument document)
		{
			return new XDocumentWrapper(document);
		}
		if (node is XElement element)
		{
			return new XElementWrapper(element);
		}
		if (node is XContainer container)
		{
			return new XContainerWrapper(container);
		}
		if (node is XProcessingInstruction processingInstruction)
		{
			return new XProcessingInstructionWrapper(processingInstruction);
		}
		if (node is XText text)
		{
			return new XTextWrapper(text);
		}
		if (node is XComment text2)
		{
			return new XCommentWrapper(text2);
		}
		if (node is XAttribute attribute)
		{
			return new XAttributeWrapper(attribute);
		}
		if (node is XDocumentType documentType)
		{
			return new XDocumentTypeWrapper(documentType);
		}
		return new XObjectWrapper(node);
	}

	public override IXmlNode AppendChild(IXmlNode newChild)
	{
		this.Container.Add(newChild.WrappedNode);
		this._childNodes = null;
		return newChild;
	}
}
