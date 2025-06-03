using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Graphics;

public sealed class ModelMeshPartCollection : ReadOnlyCollection<ModelMeshPart>
{
	public ModelMeshPartCollection(IList<ModelMeshPart> list)
		: base(list)
	{
	}
}
