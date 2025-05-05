using UnityEngine;

namespace Dopamine
{
	/// <summary>
	/// 测试用例
	/// </summary>
	internal class TestMono : MonoBehaviour
	{
		void Awake()
		{
			Recorder.Record("测试成功");
		}
	}
}