using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Product.Dto
{
    /// <summary>
    /// 产品查询对象
    /// </summary>
    public class ProductQueryDto : PagerInfo
    {
        public string UID { get; set; }
        public string Barcode { get; set; }
        public string Model { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        public DateTime? ManufactureTime { get; set; }
    }
    class ProductDto
    {
        /// <summary>
        /// 产品uid
        /// </summary>
        /// <remarks>主键，唯一标识编码</remarks>
        [ExcelColumn(Name = "产品UID", Width = 15)]
        [ExcelColumnName("产品UID")]
        public string UID { get; set; }
        /// <summary>
        /// 条形码
        /// </summary>
        /// <remarks>主键，检索标识编码</remarks>
        [ExcelColumn(Name = "产品条形码", Width = 15)]
        [ExcelColumnName("产品条形码")]
        public string Barcode { get; set; }
        /// <summary>
        /// 产品型号
        /// </summary>
        /// <remarks>比如LY3001</remarks>
        [ExcelColumn(Name = "产品型号", Width = 15)]
        [ExcelColumnName("产品型号")]
        public string Model { get; set; }
        /// <summary>
        /// 产品类型
        /// </summary>
        /// <remarks>比如：功率源</remarks>
        [ExcelColumn(Name = "产品类型", Width = 15)]
        [ExcelColumnName("产品类型")]
        public string Type { get; set; }
        /// <summary>
        /// 产品状态
        /// </summary>
        /// <remarks>如：生产中；出厂检测中；已入库；已发货；已交付；维修中；已报废</remarks>
        [ExcelColumn(Name = "产品状态", Width = 15)]
        [ExcelColumnName("产品状态")]
        public string State { get; set; }

        /// <summary>
        /// 产品物理参数
        /// </summary>
        /// <remarks>记录一些产品自带的参数，比如：尺寸规格；重量；颜色</remarks>
        [ExcelColumn(Name = "产品物理参数")]
        [ExcelColumnName("产品物理参数")]
        public string Parameter { get; set; }
        /// <summary>
        /// 产品制造日期
        /// </summary>
        [ExcelColumn(Name = "产品制造日期")]
        [ExcelColumnName("产品制造日期")]
        public DateTime? ManufactureTime { get; set; }
        /// <summary>
        /// 产品最近信息更新日期
        /// </summary>
        [ExcelColumn(Name = "产品最近信息更新日期")]
        [ExcelColumnName("产品最近信息更新日期")]
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
        [ExcelColumn(Name = "产品最近信息更新日期")]
        [ExcelColumnName("产品最近信息更新日期")]
        public string Log { get; set; }
        /// <summary>
        /// 产品备注/报告
        /// </summary>
        /// <remarks>产品的其他信息,如：
        /// <para>采用六角螺母固定，需配备相应的工具；</para></remarks>
        [ExcelColumn(Name = "产品备注")]
        [ExcelColumnName("产品备注")]
        public string Remark { get; set; }
    }

}
