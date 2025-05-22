
namespace ZR.Model.Content.Dto
{
    /// <summary>
    /// 用户加入圈子查询对象
    /// </summary>
    public class ArticleUserCirclesQueryDto : PagerInfo 
    {
    }

    /// <summary>
    /// 用户加入圈子输入输出对象
    /// </summary>
    public class ArticleUserCirclesDto
    {
        [Required(ErrorMessage = "主键不能为空")]
        public int Id { get; set; }

        [Required(ErrorMessage = "圈子ID不能为空")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "用户ID不能为空")]
        public int UserId { get; set; }

        public DateTime? JoinTime { get; set; }

        public int? Status { get; set; }



        [ExcelColumn(Name = "状态")]
        public string StatusLabel { get; set; }
    }
}