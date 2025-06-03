using System.Collections.Generic;
using StardewValley.GameData.Machines;

namespace Pathoschild.Stardew.Common.Integrations.ExtraMachineConfig;

public interface IExtraMachineConfigApi
{
	IList<(string, int)> GetExtraRequirements(MachineItemOutput outputData);

	IList<(string, int)> GetExtraTagsRequirements(MachineItemOutput outputData);

	IList<MachineItemOutput> GetExtraOutputs(MachineItemOutput outputData, MachineData? machine);
}
