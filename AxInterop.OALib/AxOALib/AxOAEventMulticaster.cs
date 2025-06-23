using System.Runtime.InteropServices;
using OALib;

namespace AxOALib;

[ClassInterface(ClassInterfaceType.None)]
public class AxOAEventMulticaster : _DOAEvents
{
	private AxOA parent;

	public AxOAEventMulticaster(AxOA parent)
	{
		this.parent = parent;
	}
}
