using Infrastructure;
using Infrastructure.Attribute;
using ZR.Model.Content;
using ZR.Model.Content.Dto;
using ZR.Repository;
using ZR.Service.Content.IService;

namespace ZR.Service.Content
{
    /// <summary>
    /// 用户加入圈子Service业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IArticleUserCirclesService), ServiceLifetime = LifeTime.Transient)]
    public class ArticleUserCirclesService : BaseService<ArticleUserCircles>, IArticleUserCirclesService
    {
        private readonly IArticleCategoryService _articleCategoryService;
        public ArticleUserCirclesService(IArticleCategoryService articleCategoryService)
        {
            _articleCategoryService = articleCategoryService;
        }

        /// <summary>
        /// 查询用户加入圈子列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<ArticleUserCirclesDto> GetList(ArticleUserCirclesQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .ToPage<ArticleUserCircles, ArticleUserCirclesDto>(parm);

            return response;
        }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ArticleUserCircles GetInfo(int Id)
        {
            var response = Queryable()
                .Where(x => x.Id == Id)
                .First();

            return response;
        }

        /// <summary>
        /// 查询表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<ArticleUserCircles> QueryExp(ArticleUserCirclesQueryDto parm)
        {
            var predicate = Expressionable.Create<ArticleUserCircles>();

            return predicate;
        }


        #region 前端接口

        /// <summary>
        /// 用户加入圈子
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public int JoinCircle(int userId, int categoryId)
        {
            var join = IsJoin(userId, categoryId);
            if (join == 1)
            {
                throw new CustomException("您已加入");
            }
            var entity = new ArticleUserCircles
            {
                UserId = userId,
                CategoryId = categoryId,
                JoinTime = DateTime.Now,
                Status = 1
            };
            var result = UseTran2(() =>
            {
                // 插入用户圈子
                Insert(entity);
                // 更新圈子加入人数
                _articleCategoryService.PlusJoinNum(categoryId);
            });

            return result ? 1 : 0;
        }
        
        /// <summary>
        /// 删除用户加入圈子
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public int RemoveCircle(int userId, int categoryId)
        {
            var result = UseTran2(() =>
            {
                Delete(x => x.CategoryId == categoryId && x.UserId == userId);
                // 更新圈子加入人数
                _articleCategoryService.ReduceJoinNum(categoryId);
            });

            return result ? 1 : 0;
        }

        /// <summary>
        /// 是否加入
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public int IsJoin(int userId, int categoryId)
        {
            var query = Queryable()
                            .Where(x => x.UserId == userId && x.CategoryId == categoryId)
                            .First();
            return query == null ? 0 : 1;
        }

        /// <summary>
        /// 查询我的圈子
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<ArticleCategoryDto> GetMyJoinCircles(int userId)
        {
            var response = Queryable()
                .LeftJoin<ArticleCategory>((it, c) => it.CategoryId == c.CategoryId)
                .Where((it,c) => it.UserId == userId)
                .Select((it, c) => new ArticleCategoryDto()
                {

                }, true)
                .ToList();

            return response;
        }
        #endregion
    }
}