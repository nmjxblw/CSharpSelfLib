using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Dopamine
{
	/// <summary>
	/// ��Ϸ������
	/// </summary>
	public class GameManager : Singleton<GameManager>
	{
		[SerializeField]
		private float _normalTimeScale = 1f;
		/// <summary>
		/// ʱ�������ٶ�
		/// </summary>
		public float NormalTimeScale => _normalTimeScale;
		/// <summary>
		/// ��Ϸ��ͣʱ�¼�
		/// </summary>
		/// <remarks>
		/// ��Ҫ�ֶ�ע�ᣬ��<c>GamePaused=true</c>ʱ�Զ�����
		/// </remarks>
		public UnityEvent? OnGamePaused;
		/// <summary>
		/// ��Ϸ�ָ�ʱ�¼�
		/// </summary>
		/// <remarks>��Ҫ�ֶ�ע�ᣬ��<c>GamePaused=false</c>ʱ�Զ�����</remarks>
		public UnityEvent? OnGameResumed;
		[SerializeField]
		private bool _gamePaused = false;
		/// <summary>
		/// ��Ϸ��ͣ��ʶ��
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
		/// �������
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
		/// ���ȡ������
		/// </summary>
		protected virtual void OnDisable()
		{
			OnGamePaused?.RemoveAllListeners();
			OnGameResumed?.RemoveAllListeners();
		}
		/// <summary>
		/// ÿ֡����
		/// </summary>
		protected virtual void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
				GamePaused = !GamePaused;
		}
		/// <summary>
		/// ����ʱ�������ٶ�
		/// </summary>
		/// <param name="timeScale"></param>
		public static void SetTimeScale(float timeScale) => Time.timeScale = timeScale;
		/// <summary>
		/// �����������
		/// </summary>
		/// <param name="value"></param>
		public static void SetCursorLockState(bool value) => Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
		/// <summary>
		/// �������ɼ�
		/// </summary>
		/// <param name="value"></param>
		public static void SetCursorVisible(bool value) => Cursor.visible = value;
		/// <summary>
		/// �رճ���
		/// </summary>
		public static void Quit() => Application.Quit();
	}
}