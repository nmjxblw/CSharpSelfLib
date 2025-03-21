using System;
using System.Collections.Generic;
using System.Text;

namespace ZH
{
    /// <summary>
    /// 台体 工作流状态
    /// </summary>
    public enum WorkFlow
    {
        None,
        Unknow,
        WarmUp,
        Starting,
        Creeping,
        DuiSeBiao,
        BasicError,
        RecordingUsage,
        XLZQWC,
        Dgn,
        WithstandVoltage
    }
}
