namespace Pathoschild.Stardew.Common;

internal static class SuppressReasons
{
	public const string UsedViaOnDeserialized = "This method is called by Json.NET automatically based on the [OnDeserialized] attribute.";

	public const string MethodValidatesNullability = "This is the method that prevents null values in the rest of the code.";
}
