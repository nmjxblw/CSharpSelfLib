using Aliyun.OSS;
using Infrastructure.Attribute;
using Mapster;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Product;
using ZR.Model.Product.Dto;
using ZR.Model.System;
using ZR.Repository;

namespace ZR.ServiceCore.Services
{
    [AppService(ServiceType = typeof(IProductService), ServiceLifetime = LifeTime.Transient)]
    class ProductService : BaseService<Product>, IProductService
    {
        private readonly ISysRoleService RoleService;
        private readonly ISysUserRoleService UserRoleService;
        private readonly ISysUserPostService UserPostService;
        private readonly ISysUserService UserService;
        public ProductService(
            ISysRoleService roleService,
            ISysUserRoleService userRoleService,
            ISysUserPostService userPostService,
            ISysUserService userService
            )
        {
            RoleService = roleService;
            UserRoleService = userRoleService;
            UserPostService = userPostService;
            UserService = userService;
        }
        /// <summary>
        /// 根据UID获取产品信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public Product GetInfo(string uid)
        {
            Expressionable<Product> exp = Expressionable.Create<Product>();
            exp = exp.AndIF(string.IsNullOrWhiteSpace(uid), f => f.ManufactureTime <= DateTime.Now.ToShortDateString().ParseToDateTime());
            exp = exp.AndIF(uid.IfNotEmpty(), f => f.UID == uid);
            Product query = Queryable().Where(exp.ToExpression()).OrderBy(it => it.ManufactureTime, OrderByType.Desc).First();
            return query;
        }
        /// <summary>
        /// 根据查询对象获取符合条件的产品页面信息
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<Product> GetPage(ProductQueryDto parm)
        {
            Expressionable<Product> exp = Expressionable.Create<Product>();
            exp = exp.AndIF(parm.UID.IfNotEmpty(), f => f.UID == parm.UID);
            exp = exp.AndIF(parm.Barcode.IfNotEmpty(), f => f.Barcode == parm.Barcode);
            exp = exp.AndIF(parm.Model.IfNotEmpty(), f => f.Model == parm.Model);
            exp = exp.AndIF(parm.Type.IfNotEmpty(), f => f.Type == parm.Type);
            exp = exp.AndIF(parm.State.IfNotEmpty(), f => f.State == parm.State);
            exp = exp.AndIF(parm.ManufactureTime != null, f => f.ManufactureTime == parm.ManufactureTime);
            exp = exp.AndIF(parm.ManufactureTime == null, f => f.ManufactureTime <= DateTime.Now.ToShortDateString().ParseToDateTime());
            ISugarQueryable<Product> query = Queryable().Where(exp.ToExpression())
           .OrderBy(it => it.ManufactureTime, OrderByType.Desc);
            return query.ToPage(parm);
        }
        /// <summary>
        /// 根据UID集合获取符合条件的产品页面信息
        /// </summary>
        /// <param name="uids"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public PagedInfo<Product> GetPage(params string[] uids)
        {
            Expressionable<Product> exp = Expressionable.Create<Product>();
            exp = exp.AndIF(uids.Length > 0, f => uids.Contains(f.UID));
            exp = exp.AndIF(uids.Length <= 0, f => f.ManufactureTime <= DateTime.Now.ToShortDateString().ParseToDateTime());
            ISugarQueryable<Product> query = Queryable().Where(exp.ToExpression())
           .OrderBy(it => it.ManufactureTime, OrderByType.Desc);
            return query.ToPage(new ProductQueryDto());
        }

        public List<Product> GetProducts(ProductQueryDto parm)
        {
            Expressionable<Product> exp = Expressionable.Create<Product>();
            exp = exp.AndIF(parm.UID.IfNotEmpty(), f => f.UID == parm.UID);
            exp = exp.AndIF(parm.Barcode.IfNotEmpty(), f => f.Barcode == parm.Barcode);
            exp = exp.AndIF(parm.Model.IfNotEmpty(), f => f.Model == parm.Model);
            exp = exp.AndIF(parm.Type.IfNotEmpty(), f => f.Type == parm.Type);
            exp = exp.AndIF(parm.State.IfNotEmpty(), f => f.State == parm.State);
            exp = exp.AndIF(parm.ManufactureTime != null, f => f.ManufactureTime == parm.ManufactureTime);
            exp = exp.AndIF(parm.ManufactureTime == null, f => f.ManufactureTime <= DateTime.Now.ToShortDateString().ParseToDateTime());
            ISugarQueryable<Product> query = Queryable().Where(exp.ToExpression())
                .OrderBy(it => it.ManufactureTime, OrderByType.Desc);
            return query.ToList();
        }

        public List<Product> GetProducts(params string[] uids)
        {
            var exp = Expressionable.Create<Product>();
            exp = exp.AndIF(uids.Length > 0, f => uids.Contains(f.UID));
            exp = exp.AndIF(uids.Length <= 0, f => f.ManufactureTime <= DateTime.Now.ToShortDateString().ParseToDateTime());
            ISugarQueryable<Product> query = Queryable().Where(exp.ToExpression()).OrderBy(it => it.ManufactureTime, OrderByType.Desc);
            return query.ToList();
        }

        public int Update(ProductQueryDto productQueryDto, Product parm)
        {
            return Update(w => w.UID == productQueryDto.UID, it => new Product()
            {
                Barcode = parm.Barcode,
                Model = parm.Model,
                Type = parm.Type,
                State = parm.State,
                ManufactureTime = parm.ManufactureTime,
                LatesetUpdateTime = DateTime.Now.ToShortDateString().ParseToDateTime(),
                Log = parm.Log,
                Remark = parm.Remark,
            });
        }

        public int Update(IEnumerable<string> uids, IEnumerable<Product> parm)
        {
            int index = 0, count = 0;
            foreach (string uid in uids)
            {
                Product temp = parm.ElementAt(index);
                count += Update(w => w.UID == uid, it => new Product()
                {
                    Barcode = temp.Barcode,
                    Model = temp.Model,
                    Type = temp.Type,
                    State = temp.State,
                    ManufactureTime = temp.ManufactureTime,
                    LatesetUpdateTime = DateTime.Now.ToShortDateString().ParseToDateTime(),
                    Log = temp.Log,
                    Remark = temp.Remark,
                });
                index++;
            }
            return count;
        }
    }
}
