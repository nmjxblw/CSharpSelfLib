using System;
using System.Collections.Generic;
using Microsoft.Build.Utilities;
namespace MSBuild
{
    /// <summary>
    /// 自定义 MSBuild 任务
    /// </summary>
    public class CustomTask : Microsoft.Build.Utilities.Task
    {
        /// <summary>
        /// Executes the task logic and returns a value indicating whether the execution was successful.
        /// </summary>
        /// <returns><see langword="true"/> if the task executed successfully; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="NotImplementedException">Thrown if the method is not implemented.</exception>
        public override bool Execute()
        {
            return true;
        }
    }
}
