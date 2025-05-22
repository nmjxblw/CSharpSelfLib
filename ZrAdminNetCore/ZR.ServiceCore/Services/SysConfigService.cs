using Infrastructure.Attribute;
using ZR.Model.System;

namespace ZR.ServiceCore.Services
{
    /// <summary>
    /// 参数配置Service业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(ISysConfigService), ServiceLifetime = LifeTime.Transient)]
    public class SysConfigService : BaseService<SysConfig>, ISysConfigService
    {
        public SysConfig GetSysConfigByKey(string key)
        {
            return Queryable().First(f => f.ConfigKey == key);
        }
    }
}