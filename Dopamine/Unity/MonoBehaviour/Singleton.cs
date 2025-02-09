namespace Dopamine;
/// <summary>
/// 单例基类
/// </summary>
/// <typeparam name="T">MonoBehaviour类</typeparam>
public class Singleton<T> : MonoBehaviour where T : notnull, MonoBehaviour
{
	private static T? _instance;
	/// <summary>
	/// 单例实例
	/// </summary>
	public static T Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindFirstObjectByType<T>();
				if (_instance == null)
				{
					GameObject obj = new GameObject()
					{
						name = typeof(T).Name
					};
					_instance = obj.AddComponent<T>();
				}
			}
			return _instance;
		}
	}
	/// <summary>
	/// 在Awake方法中初始化单例实例
	/// </summary>
	protected virtual void Awake()
	{
		if (_instance == null)
		{
			_instance = this as T;
		}
		else
		{
			Destroy(this.gameObject);
		}
		DontDestroyOnLoad(this.gameObject);
	}
}