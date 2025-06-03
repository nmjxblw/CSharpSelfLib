using System.Xml.Linq;

namespace Newtonsoft.Json.Converters;

internal class XProcessingInstructionWrapper : XObjectWrapper
{
	private XProcessingInstruction ProcessingInstruction => (XProcessingInstruction)base.WrappedNode;

	public override string? LocalName => this.ProcessingInstruction.Target;

	public override string? Value
	{
		get
		{
			return this.ProcessingInstruction.Data;
		}
		set
		{
			this.ProcessingInstruction.Data = value ?? string.Empty;
		}
	}

	public XProcessingInstructionWrapper(XProcessingInstruction processingInstruction)
		: base(processingInstruction)
	{
	}
}
