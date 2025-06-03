namespace Microsoft.Xna.Framework.Graphics;

public class EffectTechnique
{
	public EffectPassCollection Passes { get; private set; }

	public EffectAnnotationCollection Annotations { get; private set; }

	public string Name { get; private set; }

	internal EffectTechnique(Effect effect, EffectTechnique cloneSource)
	{
		this.Name = cloneSource.Name;
		this.Annotations = cloneSource.Annotations;
		this.Passes = cloneSource.Passes.Clone(effect);
	}

	internal EffectTechnique(Effect effect, string name, EffectPassCollection passes, EffectAnnotationCollection annotations)
	{
		this.Name = name;
		this.Passes = passes;
		this.Annotations = annotations;
	}
}
