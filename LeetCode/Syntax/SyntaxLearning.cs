using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetCode
{
    /// <summary>
    /// 语法学习
    /// </summary>
    public static unsafe class SyntaxLearning
    {

        class Point { public int x; }
        /// <summary>
        /// 主运行方法
        /// </summary>
        public static void Main()
        {
            byte[] data = new byte[1000];
            Point pt = new Point { x = 42 };
            fixed (int* pInt = &pt.x)/* 这里要固定pt的内存 否则使用GC后会导致地址无效 */
            {
                #region --- 强制触发GC ---
                GC.Collect();
                GC.WaitForPendingFinalizers();
                #endregion
                void* pVoid = pInt;        // 转为 void*
                *(int*)pVoid = 100;        // 安全修改值
            }
            Console.WriteLine(pt.x);       // 输出: 100
        }
    }
}
