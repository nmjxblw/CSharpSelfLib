using System;
using System.Collections.Generic;
using System.Text;
namespace ZH.Struct
{
    /// <summary>
    /// 电压参数
    /// </summary>
    public struct UIPara
    {
        /// <summary>
        /// 
        /// </summary>
        public double Ua;
        /// <summary>
        /// 
        /// </summary>
        public double Ub;
        /// <summary>
        /// 
        /// </summary>
        public double Uc;
        /// <summary>
        /// 
        /// </summary>
        public double Ia;
        /// <summary>
        /// 
        /// </summary>
        public double Ib;
        /// <summary>
        /// 
        /// </summary>
        public double Ic;
    }
    /// <summary>
    /// 
    /// </summary>
    public struct PhiPara
    {
        /// <summary>
        /// 
        /// </summary>
        public double PhiUa;
        /// <summary>
        /// 
        /// </summary>
        public double PhiUb;
        /// <summary>
        /// 
        /// </summary>
        public double PhiUc;
        /// <summary>
        /// 
        /// </summary>
        public double PhiIa;
        /// <summary>
        /// 
        /// </summary>
        public double PhiIb;
        /// <summary>
        /// 
        /// </summary>
        public double PhiIc;
    }


    /// <summary>
    /// 误差板功耗数据读取，用于计算功耗
    /// </summary>
    public struct stGHPram
    {
        /// <summary>
        /// 表位号
        /// </summary>
        public int MeterIndex;
        /// <summary>
        /// A相电压回路电流，单相电压回路电流值
        /// </summary>
        public float AU_Ia_or_I;
        /// <summary>
        /// B相电压回路电流，单相电流1回路电压值
        /// </summary>
        public float BU_Ib_or_L1_U;
        /// <summary>
        /// C相电压回路电流，单相电流2回路电压值
        /// </summary>
        public float CU_Ic_or_L2_U;
        /// <summary>
        /// A相电流回路电压
        /// </summary>
        public float AI_Ua;
        /// <summary>
        /// B相电流回路电压
        /// </summary>
        public float BI_Ub;
        /// <summary>
        /// C相电流回路电压
        /// </summary>
        public float CI_Uc;
        /// <summary>
        /// A相电压回路相位角，单相电压回路相位角
        /// </summary>
        public float AU_Phia_or_Phi;
        /// <summary>
        /// B相电压回路相位角
        /// </summary>
        public float BU_Phib;
        /// <summary>
        /// C相电压回路相位角
        /// </summary>
        public float CU_Phic;
    }
}
