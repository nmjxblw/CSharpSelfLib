using System.Collections.Generic;
using System.Xml;

namespace Newtonsoft.Json.Converters;

internal class XmlNodeWrapper : IXmlNode
{
	private readonly XmlNode _node;

	private List<IXmlNode>? _childNodes;

	private List<IXmlNode>? _attributes;

	public object? WrappedNode => this._node;

	public XmlNodeType NodeType => this._node.NodeType;

	public virtual string? LocalName => this._node.LocalName;

	public List<IXmlNode> ChildNodes
	{
		get
		{
			if (this._childNodes == null)
			{
				if (!this._node.HasChildNodes)
				{
					this._childNodes = XmlNodeConverter.EmptyChildNodes;
				}
				else
				{
					this._childNodes = new List<IXmlNode>(this._node.ChildNodes.Count);
					foreach (XmlNode childNode in this._node.ChildNodes)
					{
						this._childNodes.Add(XmlNodeWrapper.WrapNode(childNode));
					}
				}
			}
			return this._childNodes;
		}
	}

	protected virtual bool HasChildNodes => this._node.HasChildNodes;

	public List<IXmlNode> Attributes
	{
		get
		{
			if (this._attributes == null)
			{
				if (!this.HasAttributes)
				{
					this._attributes = XmlNodeConverter.EmptyChildNodes;
				}
				else
				{
					this._attributes = new List<IXmlNode>(this._node.Attributes.Count);
					foreach (XmlAttribute attribute in this._node.Attributes)
					{
						this._attributes.Add(XmlNodeWrapper.WrapNode(attribute));
					}
				}
			}
			return this._attributes;
		}
	}

	private bool HasAttributes
	{
		get
		{
			if (this._node is XmlElement xmlElement)
			{
				return xmlElement.HasAttributes;
			}
			XmlAttributeCollection? attributes = this._node.Attributes;
			if (attributes == null)
			{
				return false;
			}
			return attributes.Count > 0;
		}
	}

	public IXmlNode? ParentNode
	{
		get
		{
			XmlNode xmlNode = ((this._node is XmlAttribute xmlAttribute) ? xmlAttribute.OwnerElement : this._node.ParentNode);
			if (xmlNode == null)
			{
				return null;
			}
			return XmlNodeWrapper.WrapNode(xmlNode);
		}
	}

	public string? Value
	{
		get
		{
			return this._node.Value;
		}
		set
		{
			this._node.Value = value;
		}
	}

	public string? NamespaceUri => this._node.NamespaceURI;

	public XmlNodeWrapper(XmlNode node)
	{
		this._node = node;
	}

	internal static IXmlNode WrapNode(XmlNode node)
	{
		return node.NodeType switch
		{
			XmlNodeType.Element => new XmlElementWrapper((XmlElement)node), 
			XmlNodeType.XmlDeclaration => new XmlDeclarationWrapper((XmlDeclaration)node), 
			XmlNodeType.DocumentType => new XmlDocumentTypeWrapper((XmlDocumentType)node), 
			_ => new XmlNodeWrapper(node), 
		};
	}

	public IXmlNode AppendChild(IXmlNode newChild)
	{
		XmlNodeWrapper xmlNodeWrapper = (XmlNodeWrapper)newChild;
		this._node.AppendChild(xmlNodeWrapper._node);
		this._childNodes = null;
		this._attributes = null;
		return newChild;
	}
}
