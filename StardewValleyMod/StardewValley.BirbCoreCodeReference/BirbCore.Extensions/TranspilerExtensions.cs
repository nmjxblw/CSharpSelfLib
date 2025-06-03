using System.Collections.Generic;
using System.Linq;
using BirbCore.Attributes;
using HarmonyLib;

namespace BirbCore.Extensions;

public static class TranspilerExtensions
{
	public static int FindBCloseToA(this IEnumerable<CodeInstruction> instructions, CodeInstruction a, CodeInstruction b, int maxDistance = 10)
	{
		CodeInstruction[] instrs = instructions.ToArray();
		int distanceRemaining = 0;
		int result = -1;
		for (int i = 0; i < instrs.Length; i++)
		{
			distanceRemaining--;
			if (CodeInstructionExtensions.Is(instrs[i], a.opcode, a.operand))
			{
				distanceRemaining = maxDistance;
			}
			else if (distanceRemaining > 0 && CodeInstructionExtensions.Is(instrs[i], b.opcode, b.operand))
			{
				if (result >= 0)
				{
					Log.Error($"FindBCloseToA found multiple matches within distance {maxDistance}: {a}, {b}");
					return -1;
				}
				result = i;
			}
		}
		if (result < 0)
		{
			Log.Error($"FindBCloseToA found no matches within distance {maxDistance}: {a}, {b}");
		}
		return result;
	}

	public static IEnumerable<CodeInstruction> InsertAfterIndex(this IEnumerable<CodeInstruction> instructions, CodeInstruction[] toInsert, int index)
	{
		CodeInstruction[] instrs = instructions.ToArray();
		for (int i = 0; i < instrs.Length; i++)
		{
			yield return instrs[i];
			if (i == index)
			{
				for (int j = 0; j < toInsert.Length; j++)
				{
					yield return toInsert[j];
				}
			}
		}
	}
}
