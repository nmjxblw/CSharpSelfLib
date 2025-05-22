namespace ZR.Model.Content.Dto
{
    public class CircleInfoDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        /// <summary>
        /// 背景图
        /// </summary>
        public string BgImg { get; set; }
        /// <summary>
        /// 介绍
        /// </summary>
        public string Introduce { get; set; }
        /// <summary>
        /// 文章数
        /// </summary>
        public int ArticleNum { get; set; }
        /// <summary>
        /// 加入人数
        /// </summary>
        public int JoinNum { get; set; }
        /// <summary>
        /// 是否加入
        /// </summary>
        public int IsJoin { get; set; }
    }
}
