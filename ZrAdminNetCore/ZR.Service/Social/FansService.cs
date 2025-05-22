using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Extensions;
using Infrastructure.Model;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Content.Dto;
using ZR.Model.social;
using ZR.Model.System;
using ZR.Repository;
using ZR.Service.Social.IService;

namespace ZR.Service.Social
{
    /// <summary>
    /// 粉丝
    /// </summary>
    [AppService(ServiceType = typeof(IFansService), ServiceLifetime = LifeTime.Transient)]
    public class FansService : BaseService<Fans>, IFansService, IDynamicApi
    {
        /// <summary>
        /// 查询关注/粉丝列表
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Verify]
        [HttpGet]
        public ApiResult FollowList([FromQuery] FansQueryDto dto)
        {
            var userid = (int)HttpContextExtension.GetUId(App.HttpContext);
            PagedInfo<FansDto> list = dto.SelectType == 1 ? GetFollow(dto, userid) : GetFans(dto, userid);

            return ApiResult.Success(list);
        }

        /// <summary>
        /// 是否关注
        /// </summary>
        /// <param name="toUserid"></param>
        /// <returns></returns>
        [Verify]
        public ApiResult IsFollow([FromQuery] int toUserid)
        {
            var userid = (int)HttpContextExtension.GetUId(App.HttpContext);
            if (userid == toUserid || toUserid <= 0)
            {
                return ApiResult.Success("");
            }
            var isFollow = Any(f => f.Userid == userid && f.ToUserid == toUserid);

            return ApiResult.Success(isFollow);
        }

        /// <summary>
        /// 关注
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [Verify]
        public ApiResult Follow([FromBody] FansDto dto)
        {
            dto.Userid = (int)HttpContextExtension.GetUId(App.HttpContext);
            if (dto.Userid == dto.ToUserid)
            {
                throw new CustomException("不能关注自己");
            };
            if (dto.ToUserid <= 0)
            {
                return ApiResult.Error("关注失败");
            }
            //TODO 判断用户是否存在

            var isFollow = GetFirst(x => x.Userid == dto.Userid && x.ToUserid == dto.ToUserid);
            if (isFollow != null)
            {
                throw new CustomException("已关注");
            }
            var fans = new Fans
            {
                Userid = dto.Userid,
                ToUserid = dto.ToUserid,
                FollowTime = DateTime.Now,
                UserIP = HttpContextExtension.GetClientUserIp(App.HttpContext)
            };
            var result = UseTran2(() =>
            {
                //添加粉丝
                InsertReturnSnowflakeId(fans);

                UpdateFollowInfo(dto.Userid, isFollowNum: true);
                UpdateFollowInfo(dto.ToUserid, isFollowNum: false);
            });

            if (!result)
            {
                return ApiResult.Error("关注失败");
            }
            var data = Context.Queryable<FansInfo>()
                    .Where(x => x.Userid == dto.Userid)
                    .First();
            return ApiResult.Success("关注成功", data);
        }

        /// <summary>
        /// 取消关注
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [Verify]
        public ApiResult CancelFollow([FromBody] FansDto dto)
        {
            dto.Userid = (int)HttpContextExtension.GetUId(App.HttpContext);
            var result = UseTran(() =>
            {
                //删除粉丝
                Delete(f => f.Userid == dto.Userid && f.ToUserid == dto.ToUserid);

                Context.Updateable<FansInfo>()
                .SetColumns(x => x.FollowNum == x.FollowNum - 1)
                .Where(x => x.Userid == dto.Userid)
                .ExecuteCommand();

                Context.Updateable<FansInfo>()
                .SetColumns(x => x.FansNum == x.FansNum - 1)
                .Where(x => x.Userid == dto.ToUserid)
                .ExecuteCommand();
            });

            if (!result.IsSuccess)
            {
                return ApiResult.Error("取消关注失败");
            }
            var data = Context.Queryable<FansInfo>()
                    .Where(x => x.Userid == dto.Userid)
                    .First();
            return ApiResult.Success("取消关注成功", data);
        }

        private void UpdateFollowInfo(int userId, bool isFollowNum)
        {
            var count = Context.Updateable<FansInfo>()
                .SetColumnsIF(isFollowNum, x => x.FollowNum == x.FollowNum + 1)
                .SetColumnsIF(!isFollowNum, x => x.FansNum == x.FansNum + 1)
                .Where(x => x.Userid == userId)
                .ExecuteCommand();

            if (count > 0) return;

            Context.Insertable(new FansInfo
            {
                Userid = userId,
                FollowNum = isFollowNum ? 1 : 0,
                FansNum = isFollowNum ? 0 : 1,
                UpdateTime = DateTime.Now
            }).ExecuteCommand();
        }

        private PagedInfo<FansDto> GetFollow(FansQueryDto dto, int userid)
        {
            return Queryable()
                            .LeftJoin<SysUser>((it, u) => it.ToUserid == u.UserId)
                            .Where(it => it.Userid == userid)
                            .Select((it, u) => new FansDto()
                            {
                                User = new ArticleUser()
                                {
                                    Avatar = u.Avatar,
                                    NickName = u.NickName,
                                    Sex = u.Sex,
                                },
                                Status = 1
                            }, true)
                            .ToPage(dto);
        }
        private PagedInfo<FansDto> GetFans(FansQueryDto dto, int userid)
        {
            return Queryable()
                            .LeftJoin<SysUser>((it, u) => it.Userid == u.UserId)
                            .Where(it => it.ToUserid == userid)
                            .Select((it, u) => new FansDto()
                            {
                                User = new ArticleUser()
                                {
                                    Avatar = u.Avatar,
                                    NickName = u.NickName,
                                    Sex = u.Sex,
                                },
                            }, true)
                            .ToPage(dto);
        }
    }
}
