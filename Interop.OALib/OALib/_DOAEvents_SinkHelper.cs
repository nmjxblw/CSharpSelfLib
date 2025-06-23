using System.Runtime.InteropServices;

namespace OALib;

[ClassInterface(ClassInterfaceType.None)]
[TypeLibType(TypeLibTypeFlags.FHidden)]
public sealed class _DOAEvents_SinkHelper : _DOAEvents
{
	public int m_dwCookie;

	internal _DOAEvents_SinkHelper()
	{
		//Error decoding local variables: Signature type sequence must have at least one element.
		m_dwCookie = 0;
	}
}
