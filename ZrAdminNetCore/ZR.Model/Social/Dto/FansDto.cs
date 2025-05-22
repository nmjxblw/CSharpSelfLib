using ZR.Model.Content.Dto;

namespace ZR.Model.social
{
    /// <summary>
    /// 粉丝表
    /// </summary>
    public class FansDto
    {
        [JsonIgnore]
        public long PId { get; set; }
        public int Userid { get; set; }
        public int ToUserid { get; set; }
        [JsonIgnore]
        public DateTime FollowTime { get; set; }
        [JsonIgnore]
        public string UserIP { get; set; }
        public int Status { get; set; }

        public ArticleUser User { get; set; }
    }

    public class FansQueryDto : PagerInfo
    {
        public int SelectType { get; set; }
    }
}
