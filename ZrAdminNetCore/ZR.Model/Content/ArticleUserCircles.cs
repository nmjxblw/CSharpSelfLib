
namespace ZR.Model.Content
{
    /// <summary>
    /// 用户加入圈子
    /// </summary>
    [SugarTable("article_userCircles")]
    [Tenant(0)]
    public class ArticleUserCircles
    {
        /// <summary>
        /// 主键 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 圈子ID 
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 用户ID 
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 加入时间 
        /// </summary>
        public DateTime? JoinTime { get; set; }

        /// <summary>
        /// 状态 0=待审核，1=已加入，2=已拒绝
        /// </summary>
        public int Status { get; set; }
    }
}