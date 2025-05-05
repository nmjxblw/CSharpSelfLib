using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Dopamine
{
	/// <summary>
	/// 游戏管理器
	/// </summary>
	public class GameManager : Singleton<GameManager>
	{
		[SerializeField]
		private float _normalTimeScale = 1f;
		/// <summary>
		/// 时间流逝速度
		/// </summary>
		public float NormalTimeScale => _normalTimeScale;
		/// <summary>
		/// 游戏暂停时事件
		/// </summary>
		/// <remarks>
		/// 需要手动注册，在<c>GamePaused=true</c>时自动调用
		/// </remarks>
		public UnityEvent? OnGamePaused;
		/// <summary>
		/// 游戏恢复时事件
		/// </summary>
		/// <remarks>需要手动注册，在<c>GamePaused=false</c>时自动调用</remarks>
		public UnityEvent? OnGameResumed;
		[SerializeField]
		private bool _gamePaused = false;
		/// <summary>
		/// 游戏暂停标识符
		/// </summary>
		public bool GamePaused
		{
			get
			{
				return _gamePaused;
			}
			set
			{
				if (_gamePaused != value)
				{
					_gamePaused = value;
					if (_gamePaused)
						OnGamePaused?.Invoke();
					else
						OnGameResumed?.Invoke();
				}
			}
		}
		/// <summary>
		/// 组件激活
		/// </summary>
		protected virtual void OnEnable()
		{
			OnGamePaused?.AddListener(() => SetTimeScale(0f));
			OnGamePaused?.AddListener(() => SetCursorLockState(false));
			OnGamePaused?.AddListener(() => SetCursorVisible(true));
			OnGameResumed?.AddListener(() => SetTimeScale(NormalTimeScale));
			OnGameResumed?.AddListener(() => SetCursorLockState(true));
			OnGameResumed?.AddListener(() => SetCursorVisible(false));
		}
		/// <summary>
		/// 组件取消激活
		/// </summary>
		protected virtual void OnDisable()
		{
			OnGamePaused?.RemoveAllListeners();
			OnGameResumed?.RemoveAllListeners();
		}
		/// <summary>
		/// 每帧更新
		/// </summary>
		protected virtual void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
				GamePaused = !GamePaused;
		}
		/// <summary>
		/// 设置时间流逝速度
		/// </summary>
		/// <param name="timeScale"></param>
		public static void SetTimeScale(float timeScale) => Time.timeScale = timeScale;
		/// <summary>
		/// 设置鼠标锁定
		/// </summary>
		/// <param name="value"></param>
		public static void SetCursorLockState(bool value) => Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
		/// <summary>
		/// 设置鼠标可见
		/// </summary>
		/// <param name="value"></param>
		public static void SetCursorVisible(bool value) => Cursor.visible = value;
		/// <summary>
		/// 关闭程序
		/// </summary>
		public static void Quit() => Application.Quit();
	}
}