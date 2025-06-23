using System.Runtime.InteropServices;

namespace OALib;

[ComImport]
[Guid("16EFE4A7-6641-459C-8BB3-B02AC6F088F4")]
[CoClass(typeof(OAClass))]
public interface OA : _DOA, _DOAEvents_Event
{
}
