/*
 * @lc app=leetcode.cn id=1114 lang=csharp
 *
 * [1114] 按序打印
 *
 * https://leetcode.cn/problems/print-in-order/description/
 *
 * concurrency
 * Easy (66.28%)
 * Likes:    531
 * Dislikes: 0
 * Total Accepted:    134.5K
 * Total Submissions: 202.6K
 * Testcase Example:  '[1,2,3]'
 *
 * 给你一个类：
 *
 *
 * public class Foo {
 * public void first() { print("first"); }
 * public void second() { print("second"); }
 * public void third() { print("third"); }
 * }
 *
 * 三个不同的线程 A、B、C 将会共用一个 Foo 实例。
 *
 *
 * 线程 A 将会调用 first() 方法
 * 线程 B 将会调用 second() 方法
 * 线程 C 将会调用 third() 方法
 *
 *
 * 请设计修改程序，以确保 second() 方法在 first() 方法之后被执行，third() 方法在 second() 方法之后被执行。
 *
 * 提示：
 *
 *
 * 尽管输入中的数字似乎暗示了顺序，但是我们并不保证线程在操作系统中的调度顺序。
 * 你看到的输入格式主要是为了确保测试的全面性。
 *
 *
 *
 *
 * 示例 1：
 *
 *
 * 输入：nums = [1,2,3]
 * 输出："firstsecondthird"
 * 解释：
 * 有三个线程会被异步启动。输入 [1,2,3] 表示线程 A 将会调用 first() 方法，线程 B 将会调用 second() 方法，线程 C
 * 将会调用 third() 方法。正确的输出是 "firstsecondthird"。
 *
 *
 * 示例 2：
 *
 *
 * 输入：nums = [1,3,2]
 * 输出："firstsecondthird"
 * 解释：
 * 输入 [1,3,2] 表示线程 A 将会调用 first() 方法，线程 B 将会调用 third() 方法，线程 C 将会调用 second()
 * 方法。正确的输出是 "firstsecondthird"。
 *
 *
 *
 *
 *
 * 提示：
 *
 *
 * nums 是 [1, 2, 3] 的一组排列
 *
 *
 */

// @lc code=start
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public partial class Foo
{
    private static AutoResetEvent[] autoResetEvents = new AutoResetEvent[]
    {
        new AutoResetEvent(false),
        new AutoResetEvent(false),
    };

    /// <summary>
    /// 按序打印，构造函数
    /// </summary>
    public Foo() { }

    /// <summary>
    /// 第一个方法
    /// </summary>
    /// <param name="printFirst"></param>
    public void First(Action printFirst)
    {
        // printFirst() outputs "first". Do not change or remove this line.
        printFirst();
        Foo.autoResetEvents[0].Set();
    }

    /// <summary>
    /// 第二个方法
    /// </summary>
    /// <param name="printSecond"></param>
    public void Second(Action printSecond)
    {
        autoResetEvents[0].WaitOne();

        // printSecond() outputs "second". Do not change or remove this line.
        printSecond();
        // 通知其他Foo实例解锁2号锁
        Foo.autoResetEvents[1].Set();
    }

    /// <summary>
    /// 第三个方法
    /// </summary>
    /// <param name="printThird"></param>
    public void Third(Action printThird)
    {
        autoResetEvents[1].WaitOne();
        printThird();
    }
}
// @lc code=end
// 36/36 cases passed (200 ms)
// Your runtime beats 58.33 % of csharp submissions
// Your memory usage beats 100 % of csharp submissions (43.1 MB)
