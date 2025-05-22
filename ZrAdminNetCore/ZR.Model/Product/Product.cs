using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.System;

namespace ZR.Model.Product
{
    /// <summary>
    /// 产品表
    /// </summary>
    [SugarTable("Product", "产品表")]
    [Tenant("0")]
    public class Product : SysBase
    {
        /// <summary>
        /// 产品UID
        /// </summary>
        [SugarColumn(ColumnDescription = "产品UID", ColumnName = "UID", IsPrimaryKey = true, IsIdentity = true, ColumnDataType = StaticConfig.CodeFirst_BigString)]
        [JsonProperty(propertyName: "uid")]
        public string UID { get; set; }
        /// <summary>
        /// 产品条形码
        /// </summary>
        [SugarColumn(ColumnDescription = "产品条形码", ColumnName = "Barcode", IsPrimaryKey = true, ColumnDataType = StaticConfig.CodeFirst_BigString)]
        [JsonProperty(propertyName: "barcode")]
        public string Barcode { get; set; }
        /// <summary>
        /// 产品型号
        /// </summary>
        /// <remarks>比如LY3001</remarks>
        [SugarColumn(ColumnDescription = "产品型号", ColumnName = "Model", ColumnDataType = StaticConfig.CodeFirst_BigString)]
        [JsonProperty(propertyName: "model")]
        public string Model { get; set; }
        /// <summary>
        /// 产品类型
        /// </summary>
        /// <remarks>比如：功率源</remarks>
        [SugarColumn(ColumnDescription = "产品类型", ColumnName = "Type", ColumnDataType = StaticConfig.CodeFirst_BigString)]
        [JsonProperty(propertyName: "type")]
        public string Type { get; set; }
        /// <summary>
        /// 产品状态
        /// </summary>
        /// <remarks>如：生产中；出厂检测中；已入库；已发货；已交付；维修中；已报废</remarks>
        [SugarColumn(ColumnDescription = "产品状态", ColumnName = "State", ColumnDataType = StaticConfig.CodeFirst_BigString)]
        [JsonProperty(propertyName: "state")]
        public string State { get; set; }

        /// <summary>
        /// 产品物理参数
        /// </summary>
        /// <remarks>记录一些产品自带的参数，比如：尺寸规格；重量；颜色</remarks>
        [SugarColumn(ColumnDescription = "产品物理参数", ColumnName = "Parameter", ColumnDataType = StaticConfig.CodeFirst_BigString)]
        [JsonProperty(propertyName: "parameter")]
        public string Parameter { get; set; }
        /// <summary>
        /// 产品生产日期
        /// </summary>
        [SugarColumn(ColumnDescription = "产品生产日期", ColumnName = "ManufactureTime", ColumnDataType = "datetime")]
        [JsonProperty(propertyName: "manufactureTime")]
        [ExcelColumn(Format = "yyyy-MM-dd HH:mm:ss")]
        public DateTime? ManufactureTime { get; set; }
        /// <summary>
        /// 产品最近信息更新日期
        /// </summary>
        [SugarColumn(ColumnDescription = "产品最近信息更新日期", ColumnName = "LatestUpdateTime", ColumnDataType = "datetime")]
        [JsonProperty(propertyName: "latesetUpdateTime")]
        [ExcelColumn(Format = "yyyy-MM-dd HH:mm:ss")]
        public DateTime? LatesetUpdateTime { get; set; }
        /// <summary>
        /// 产品日志
        /// </summary>
        /// <remarks>
        /// 每次更新产品信息时的跟踪日志,举例：
        /// <para>
        /// [2025-05-22 11:52:26] 检定单位更新：基本误差试验|220V|1.0Ib|正向有功|1.0L - 合格；核检员：张三；检定单位：xx供电计量检测中心
        /// </para>
        /// <para>
        /// [2025-05-11 16:42:07] 质量部检测更新：产品外观出现划痕，已修复；维修员：李四；
        /// </para>
        /// <para>
        /// [2022-08-26 12:34:56] 产品入库更新：2022/08/26完成入库; 入库员：王五；入库单位：xx公司深圳智能仓储单位
        /// </para>
        /// </remarks>
        [SugarColumn(ColumnDescription = "产品日志", ColumnName = "Log", ColumnDataType = StaticConfig.CodeFirst_BigString)]
        [JsonProperty(propertyName: "log")]
        public string Log { get; set; }
        /// <summary>
        /// 产品备注/报告
        /// </summary>
        /// <remarks>产品的其他信息,如：
        /// <para>采用六角螺母固定，需配备相应的工具；</para></remarks>
        [SugarColumn(ColumnDescription = "产品备注", ColumnName = "Remark", ColumnDataType = StaticConfig.CodeFirst_BigString)]
        [JsonProperty(propertyName: "remark")]
        public new string Remark { get; set; }
    }
}
