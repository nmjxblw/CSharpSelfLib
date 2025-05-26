using Aliyun.OSS;
using Infrastructure;
using Infrastructure.Attribute;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
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
            exp = exp.AndIF(string.IsNullOrWhiteSpace(uid), f => f.ManufactureTime <= DateTime.Now.ToLocalTime());
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
            exp = exp.AndIF(parm.ManufactureTime == null, f => f.ManufactureTime <= DateTime.Now.ToLocalTime());
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
            exp = exp.AndIF(uids.Length <= 0, f => f.ManufactureTime <= DateTime.Now.ToLocalTime());
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
            exp = exp.AndIF(parm.ManufactureTime == null, f => f.ManufactureTime <= DateTime.Now.ToLocalTime());
            ISugarQueryable<Product> query = Queryable().Where(exp.ToExpression())
                .OrderBy(it => it.ManufactureTime, OrderByType.Desc);
            return query.ToList();
        }

        public List<Product> GetProducts(params string[] uids)
        {
            var exp = Expressionable.Create<Product>();
            exp = exp.AndIF(uids.Length > 0, f => uids.Contains(f.UID));
            exp = exp.AndIF(uids.Length <= 0, f => f.ManufactureTime <= DateTime.Now.ToLocalTime());
            ISugarQueryable<Product> query = Queryable().Where(exp.ToExpression()).OrderBy(it => it.ManufactureTime, OrderByType.Desc);
            return query.ToList();
        }

        public ProductQueryDto ToDto(Product product)
        {
            return product.Adapt<ProductQueryDto>();
        }

        public int Update(ProductQueryDto productQueryDto, Product product)
        {
            Product temp = SoftUpdate(product);
            return Update(w => w.UID == productQueryDto.UID, it => new Product
            {
                UID = temp.UID,
                Barcode = temp.Barcode,
                Model = temp.Model,
                Type = temp.Type,
                State = temp.State,
                Parameter = temp.Parameter,
                ManufactureTime = temp.ManufactureTime,
                LatesetUpdateTime = temp.LatesetUpdateTime,
                Log = temp.Log,
                Remark = temp.Remark,
            });
        }
        /// <summary>
        /// 更新产品信息
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        public int[] Update(IEnumerable<Product> products)
        {
            int index = 0;
            int[] result = new int[products.Count()];
            Array.Fill(result, 0);
            foreach (Product product in products)
            {
                result[index] = this.Refersh(product);
                index++;
            }
            return result;
        }

        public int Refersh(Product product)
        {
            Product temp = SoftUpdate(product);
            return Update(w => w.UID == product.UID, it => new Product
            {
                UID = temp.UID,
                Barcode = temp.Barcode,
                Model = temp.Model,
                Type = temp.Type,
                State = temp.State,
                Parameter = temp.Parameter,
                ManufactureTime = temp.ManufactureTime,
                LatesetUpdateTime = temp.LatesetUpdateTime,
                Log = temp.Log,
                Remark = temp.Remark,
            });
        }
        /// <summary>
        /// 软更新产品信息
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public Product SoftUpdate(Product product)
        {
            Product temp = GetInfo(product.UID);
            if (temp != null || temp != default)
            {
                //制造时间只能使用初始时间，如果要修改制造时间需要访问特殊API
                product.ManufactureTime = temp.ManufactureTime;
                // 产品型号同理
                product.Model = temp.Model;
                // 产品类型同理
                product.Type = temp.Type;
                product.Barcode = temp.Barcode;
                // 日志只能添加，不能移除，移除日志需要访问特别API
                product.Log = product.Log + "|" + temp.Log;

                product.Remark = string.IsNullOrWhiteSpace(product.Remark) ? temp.Remark : product.Remark;
                product.State = string.IsNullOrWhiteSpace(product.State) ? temp.State : product.State;
                product.Parameter = string.IsNullOrWhiteSpace(product.Parameter) ? temp.Parameter : product.Parameter;

            }
            product.LatesetUpdateTime = DateTime.Now.ToLocalTime();
            return product;
        }
    }
}
