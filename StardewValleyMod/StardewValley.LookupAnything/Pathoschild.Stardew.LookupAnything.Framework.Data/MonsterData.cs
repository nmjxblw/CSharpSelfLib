namespace Pathoschild.Stardew.LookupAnything.Framework.Data;

internal record MonsterData(string Name, int Health, int DamageToFarmer, bool IsGlider, int Resilience, double Jitteriness, int MoveTowardsPlayerThreshold, int Speed, double MissChance, bool IsMineMonster, ItemDropData[] Drops);
