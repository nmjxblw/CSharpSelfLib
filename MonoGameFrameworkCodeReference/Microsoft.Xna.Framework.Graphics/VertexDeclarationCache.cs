namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Helper class which ensures we only lookup a vertex 
/// declaration for a particular type once.
/// </summary>
/// <typeparam name="T">A vertex structure which implements IVertexType.</typeparam>
internal class VertexDeclarationCache<T> where T : struct, IVertexType
{
	private static VertexDeclaration _cached;

	public static VertexDeclaration VertexDeclaration
	{
		get
		{
			if (VertexDeclarationCache<T>._cached == null)
			{
				VertexDeclarationCache<T>._cached = VertexDeclaration.FromType(typeof(T));
			}
			return VertexDeclarationCache<T>._cached;
		}
	}
}
