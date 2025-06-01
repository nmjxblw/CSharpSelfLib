namespace Microsoft.Xna.Framework.Graphics;

public class EffectAnnotation
{
	public EffectParameterClass ParameterClass { get; private set; }

	public EffectParameterType ParameterType { get; private set; }

	public string Name { get; private set; }

	public int RowCount { get; private set; }

	public int ColumnCount { get; private set; }

	public string Semantic { get; private set; }

	internal EffectAnnotation(EffectParameterClass class_, EffectParameterType type, string name, int rowCount, int columnCount, string semantic, object data)
	{
		this.ParameterClass = class_;
		this.ParameterType = type;
		this.Name = name;
		this.RowCount = rowCount;
		this.ColumnCount = columnCount;
		this.Semantic = semantic;
	}

	internal EffectAnnotation(EffectParameter parameter)
	{
		this.ParameterClass = parameter.ParameterClass;
		this.ParameterType = parameter.ParameterType;
		this.Name = parameter.Name;
		this.RowCount = parameter.RowCount;
		this.ColumnCount = parameter.ColumnCount;
		this.Semantic = parameter.Semantic;
	}
}
