namespace ZR.Model.social
{
    /// <summary>
    /// 粉丝信息表
    /// </summary>
    [SugarTable("fans_info")]
    public class FansInfo
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Userid { get; set; }
        /// <summary>
        /// 关注数
        /// </summary>
        public int FollowNum { get; set; }
        /// <summary>
        /// 粉丝数
        /// </summary>
        public int FansNum { get; set; }
        [JsonIgnore]
        public DateTime UpdateTime { get; set; }
    }
}
