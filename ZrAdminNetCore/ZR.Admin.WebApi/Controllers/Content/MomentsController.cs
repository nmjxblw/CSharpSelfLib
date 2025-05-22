using Microsoft.AspNetCore.Mvc;
using ZR.Model.Content;
using ZR.Model.Content.Dto;
using ZR.Model.Enum;
using ZR.Service.Content.IService;

namespace ZR.Admin.WebApi.Controllers
{
    [Route("moment")]
    [ApiExplorerSettings(GroupName = "article")]
    public class MomentsController : BaseController
    {
        /// <summary>
        /// 动态接口
        /// </summary>
        private readonly IArticleService _ArticleService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ArticleService"></param>
        public MomentsController(
            IArticleService ArticleService)
        {
            _ArticleService = ArticleService;
        }

        /// <summary>
        /// 查询我的(后台)
        /// </summary>
        /// <returns></returns>
        [HttpGet("mylist")]
        public IActionResult QueryMyList([FromQuery] ArticleQueryDto parm)
        {
            parm.UserId = HttpContext.GetUId();
            parm.ArticleType = (int)ArticleTypeEnum.Monent;
            var response = _ArticleService.GetMyList(parm);

            return SUCCESS(response);
        }

        /// <summary>
        /// 查询动态列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("momentList")]
        [AllowAnonymous]
        public IActionResult QueryMonentList([FromQuery] ArticleQueryDto parm)
        {
            parm.UserId = HttpContext.GetUId();
            parm.ArticleType = (int)ArticleTypeEnum.Monent;
            parm.QueryMyJoin = parm.TabId == 100;//查询关注的圈子动态

            return SUCCESS(_ArticleService.GetMonentList(parm));
        }

        /// <summary>
        /// 动态发布
        /// </summary>
        /// <returns></returns>
        [HttpPost("publishMoment")]
        [ActionPermissionFilter(Permission = "common")]
        public IActionResult PublishMoment([FromBody] ArticleDto parm)
        {
            if (parm == null) { return ToResponse(ResultCode.PARAM_ERROR); }
            var addModel = parm.Adapt<Article>().ToCreate(context: HttpContext);
            addModel.Tags = parm.TopicName;
            addModel.ArticleType = ArticleTypeEnum.Monent;

            return SUCCESS(_ArticleService.Publish(addModel));
        }

        /// <summary>
        /// 动态信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("getInfo")]
        public IActionResult GetInfo()
        {
            var userId = HttpContext.GetUId();

            var monentNum = _ArticleService.Queryable()
                .Count(f => f.UserId == userId && f.ArticleType == ArticleTypeEnum.Monent);

            return SUCCESS(new { monentNum, commentNum = 0 });
        }
    }
}
