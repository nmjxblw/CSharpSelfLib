namespace ZR.Model.social
{
    /// <summary>
    /// 粉丝表
    /// </summary>
    [SugarTable("fans")]
    public class Fans
    {
        [SugarColumn(IsPrimaryKey = true)]
        public long PId { get; set; }
        public int Userid { get; set; }
        public int ToUserid { get; set; }
        public DateTime FollowTime { get; set; }
        public string UserIP { get; set; }
    }
}
