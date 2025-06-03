namespace SpaceShared;

public class Weighted<T>
{
	public double Weight { get; set; }

	public T Value { get; set; }

	public Weighted()
		: this(1.0, default(T))
	{
	}

	public Weighted(double weight, T value)
	{
		this.Weight = weight;
		this.Value = value;
	}
}
