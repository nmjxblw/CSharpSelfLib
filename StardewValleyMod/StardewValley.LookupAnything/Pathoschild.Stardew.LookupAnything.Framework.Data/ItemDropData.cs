namespace Pathoschild.Stardew.LookupAnything.Framework.Data;

internal record ItemDropData(string ItemId, int MinDrop, int MaxDrop, float Probability, string? Conditions = null);
