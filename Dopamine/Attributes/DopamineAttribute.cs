using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine
{
    /// <summary>
    /// Dopamine特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Delegate | AttributeTargets.All, Inherited = false)]
    [ComVisible(true)]
    public sealed class DopamineAttribute : Attribute
    {
        internal string _Name;
        /// <summary>
        /// 绑定对象的名称
        /// </summary>
        /// <remarks>完整程序集名称</remarks>
        public string Name
        {
            get { return _Name; }
        }
        /// <summary>
        /// 初始化 <see cref="DopamineAttribute"/> 实例
        /// </summary>
        /// <remarks>This constructor uses the provided caller information to generate a unique identifier
        /// based on the file path, member name, and line number. The identifier can be useful for debugging or tracking
        /// the origin of the attribute usage.</remarks>
        /// <param name="filePath">The full path of the source file containing the caller. Defaults to an empty string if not provided.</param>
        /// <param name="memberName">The name of the method or property from which this attribute is applied. Defaults to an empty string if not
        /// provided.</param>
        /// <param name="lineNumber">The line number in the source file where this attribute is applied. Defaults to 0 if not provided.</param>
        public DopamineAttribute([CallerFilePath] string filePath = "",
                                 [CallerMemberName] string memberName = "",
                                 [CallerLineNumber] int lineNumber = 0)
        {
            // 组合文件路径+成员名作为唯一标识（非完全限定名）
            _Name = $"{filePath}:{memberName} (Line {lineNumber})";
        }
    }
}
