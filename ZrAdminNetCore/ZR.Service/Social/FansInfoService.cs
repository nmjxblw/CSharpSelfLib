using Infrastructure.Attribute;
using ZR.Model.social;
using ZR.Service.Social.IService;

namespace ZR.Service.Social
{
    [AppService(ServiceType = typeof(IFansInfoService), ServiceLifetime = LifeTime.Transient)]
    public class FansInfoService : BaseService<FansInfo>, IFansInfoService
    {
    }
}
