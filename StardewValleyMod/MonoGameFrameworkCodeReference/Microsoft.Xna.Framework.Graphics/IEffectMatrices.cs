namespace Microsoft.Xna.Framework.Graphics;

public interface IEffectMatrices
{
	Matrix Projection { get; set; }

	Matrix View { get; set; }

	Matrix World { get; set; }
}
