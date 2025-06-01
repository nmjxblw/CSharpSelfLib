using System.Collections.Generic;
using System.Xml.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

internal class XElementWrapper : XContainerWrapper, IXmlElement, IXmlNode
{
	private List<IXmlNode>? _attributes;

	private XElement Element => (XElement)base.WrappedNode;

	public override List<IXmlNode> Attributes
	{
		get
		{
			if (this._attributes == null)
			{
				if (!this.Element.HasAttributes && !this.HasImplicitNamespaceAttribute(this.NamespaceUri))
				{
					this._attributes = XmlNodeConverter.EmptyChildNodes;
				}
				else
				{
					this._attributes = new List<IXmlNode>();
					foreach (XAttribute item in this.Element.Attributes())
					{
						this._attributes.Add(new XAttributeWrapper(item));
					}
					string namespaceUri = this.NamespaceUri;
					if (this.HasImplicitNamespaceAttribute(namespaceUri))
					{
						this._attributes.Insert(0, new XAttributeWrapper(new XAttribute("xmlns", namespaceUri)));
					}
				}
			}
			return this._attributes;
		}
	}

	public override string? Value
	{
		get
		{
			return this.Element.Value;
		}
		set
		{
			this.Element.Value = value ?? string.Empty;
		}
	}

	public override string? LocalName => this.Element.Name.LocalName;

	public override string? NamespaceUri => this.Element.Name.NamespaceName;

	public bool IsEmpty => this.Element.IsEmpty;

	public XElementWrapper(XElement element)
		: base(element)
	{
	}

	public void SetAttributeNode(IXmlNode attribute)
	{
		XObjectWrapper xObjectWrapper = (XObjectWrapper)attribute;
		this.Element.Add(xObjectWrapper.WrappedNode);
		this._attributes = null;
	}

	private bool HasImplicitNamespaceAttribute(string namespaceUri)
	{
		if (!StringUtils.IsNullOrEmpty(namespaceUri) && namespaceUri != this.ParentNode?.NamespaceUri && StringUtils.IsNullOrEmpty(this.GetPrefixOfNamespace(namespaceUri)))
		{
			bool flag = false;
			if (this.Element.HasAttributes)
			{
				foreach (XAttribute item in this.Element.Attributes())
				{
					if (item.Name.LocalName == "xmlns" && StringUtils.IsNullOrEmpty(item.Name.NamespaceName) && item.Value == namespaceUri)
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				return true;
			}
		}
		return false;
	}

	public override IXmlNode AppendChild(IXmlNode newChild)
	{
		IXmlNode result = base.AppendChild(newChild);
		this._attributes = null;
		return result;
	}

	public string? GetPrefixOfNamespace(string namespaceUri)
	{
		return this.Element.GetPrefixOfNamespace(namespaceUri);
	}
}
