#if UNITY_EDITOR
#pragma warning disable CS0649
#endif
namespace Dopamine;
/// <summary>
/// 测试用Test Scriptable Object
/// </summary>
[CreateAssetMenu(fileName = "TestSO", menuName = "GameMaker/TestSO")]
public class TestSO : ScriptableObject, IComparable<TestSO>
{
	/// <summary>
	/// 测试文本资产
	/// </summary>
	[SerializeField]
	private TextAsset? m_TextAsset;
	/// <summary>
	/// 排序方法
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	public int CompareTo(TestSO? other)
	{
		return this.name.CompareTo(other?.name);
	}

	void OnEnable()
	{
		string? s = m_TextAsset?.text;
		UnityEngine.Debug.Log($"s={s}");
	}
}

/// <summary>
/// 测试用config
/// </summary>
[Serializable]
public class Config
{
	/// <summary>
	/// 名称
	/// </summary>
	public string? name;
	/// <summary>
	/// 数据
	/// </summary>
	public dynamic? data;
}
