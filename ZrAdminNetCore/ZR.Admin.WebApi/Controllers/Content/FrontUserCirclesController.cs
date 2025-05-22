using Microsoft.AspNetCore.Mvc;
using ZR.Service.Content.IService;

//创建时间：2025-04-07
namespace ZR.Admin.WebApi.Controllers
{
    /// <summary>
    /// 用户加入圈子
    /// </summary>
    [Route("front/circles")]
    [ApiExplorerSettings(GroupName = "userCircle")]
    [ApiController]
    public class ArticleUserCirclesController : BaseController
    {
        /// <summary>
        /// 用户加入圈子接口
        /// </summary>
        private readonly IArticleUserCirclesService _ArticleUserCirclesService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ArticleUserCirclesService"></param>
        public ArticleUserCirclesController(
            IArticleUserCirclesService ArticleUserCirclesService)
        {
            _ArticleUserCirclesService = ArticleUserCirclesService;
        }

        /// <summary>
        /// 查询用户加入圈子列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("myCircleList")]
        public IActionResult QueryArticleUserCircles()
        {
            var userId = HttpContext.GetUId();
            var response = _ArticleUserCirclesService.GetMyJoinCircles((int)userId);
            return SUCCESS(response);
        }

        /// <summary>
        /// 用户加入圈子
        /// </summary>
        /// <returns></returns>
        [HttpPost("join/{id}")]
        public IActionResult JoinCircles([FromRoute] int id)
        {
            var userId = HttpContext.GetUId();
            return ToResponse(_ArticleUserCirclesService.JoinCircle((int)userId, id));
        }

        /// <summary>
        /// 删除用户加入圈子
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{id}")]
        public IActionResult DeleteCircles([FromRoute] int id)
        {
            var userId = HttpContext.GetUId();
            return ToResponse(_ArticleUserCirclesService.RemoveCircle((int)userId, id));
        }
    }
}