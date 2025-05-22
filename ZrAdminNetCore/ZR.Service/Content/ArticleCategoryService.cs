using Infrastructure.Attribute;
using Infrastructure.Extensions;
using Mapster;
using ZR.Model.Content;
using ZR.Model.Content.Dto;
using ZR.Repository;
using ZR.Service.Content.IService;

namespace ZR.Service.Content
{
    /// <summary>
    /// 文章目录Service业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IArticleCategoryService), ServiceLifetime = LifeTime.Transient)]
    public class ArticleCategoryService : BaseService<ArticleCategory>, IArticleCategoryService
    {
        /// <summary>
        /// 查询文章目录列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<ArticleCategoryDto> GetList(ArticleCategoryQueryDto parm)
        {
            var predicate = Expressionable.Create<ArticleCategory>();
            predicate.AndIF(parm.CategoryType != null, m => m.CategoryType == parm.CategoryType);
            predicate.AndIF(parm.ParentId != null, m => m.ParentId == parm.ParentId);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .WithCache(60 * 5)
                .ToPage<ArticleCategory, ArticleCategoryDto>(parm);

            return response;
        }

        /// <summary>
        /// 查询文章目录树列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public List<ArticleCategoryDto> GetTreeList(ArticleCategoryQueryDto parm)
        {
            var predicate = Expressionable.Create<ArticleCategory>();
            predicate.AndIF(parm.CategoryType != null, m => m.CategoryType == parm.CategoryType);

            var response = Queryable()
                .Where(predicate.ToExpression());

            if (parm.Sort.IsNotEmpty())
            {
                response = response.OrderByPropertyName(parm.Sort, parm.SortType.Contains("desc") ? OrderByType.Desc : OrderByType.Asc);
            }
            var treeList = response.ToTree(it => it.Children, it => it.ParentId, 0);

            return treeList.Adapt<List<ArticleCategoryDto>>();
        }

        /// <summary>
        /// 添加文章目录
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public int AddArticleCategory(ArticleCategory parm)
        {
            var response = Add(parm);
            return response;
        }

        /// <summary>
        /// 增加加入圈子人数
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public int PlusJoinNum(int categoryId)
        {
            return Update(w => w.CategoryId == categoryId, w => new ArticleCategory() { JoinNum = w.JoinNum +1 });
        }

        /// <summary>
        /// 减少加入圈子人数
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public int ReduceJoinNum(int categoryId)
        {
            return Update(w => w.CategoryId == categoryId, w => new ArticleCategory() { JoinNum = w.JoinNum - 1 });
        }
    }
}
