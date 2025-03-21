using System;
using System.Collections.Generic;
using System.Text;

namespace ZH.Struct
{
    /// <summary>
    /// 设备属性信息
    /// </summary>
    public struct StEquipInfo
    {
        /// <summary>
        /// 标识
        /// </summary>
        public string Key;
        /// <summary>
        /// 是否是COM通讯
        /// </summary>
        public bool IsCom;
        /// <summary>
        /// 端口号
        /// </summary>
        public int DataPort;
        /// <summary>
        /// 设置端口
        /// </summary>
        public int SettingPort;
        /// <summary>
        /// 端口设置参数
        /// </summary>
        public string Setting;

        /// <summary>
        /// 是否已经初始化过
        /// </summary>
        private bool InitializeFlag;

        /// <summary>
        /// 端口是否已经初始化
        /// </summary>
        /// <returns></returns>
        public bool IsInitialized()
        {
            return InitializeFlag;
        }

        /// <summary>
        /// 更新端口初始化标识
        /// </summary>
        /// <param name="newValue"></param>
        public void UpdateInitializeFlag(bool newValue)
        {
            InitializeFlag = newValue;
        }

        /// <summary>
        /// 获取端口名
        /// </summary>
        /// <param name="bDataPort"></param>
        /// <returns></returns>
        public string GetPortName(bool bDataPort)
        {
            if (bDataPort)
                return string.Format("Port_{0}", DataPort);
            else
                return string.Format("Port_{0}", SettingPort);
        }
    }
}
