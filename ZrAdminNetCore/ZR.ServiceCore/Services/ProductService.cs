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
    [AppService(ServiceType = typeof(ISysTasksQzService), ServiceLifetime = LifeTime.Transient)]
    class ProductService : BaseService<Product>, IProduct
    {
        public Product GetInfo(string uid)
        {
            throw new NotImplementedException();
        }

        public PagedInfo<Product> GetPage(ProductQueryDto parm)
        {
            var exp = Expressionable.Create<Product>();
            exp = exp.AndIF(parm.UID.IfNotEmpty(), f => f.UID == parm.UID);
            exp = exp.AndIF(parm.Barcode.IfNotEmpty(), f => f.Barcode == parm.Barcode);
            exp = exp.AndIF(parm.Model.IfNotEmpty(), f => f.Model == parm.Model);
            exp = exp.AndIF(parm.Type.IfNotEmpty(), f => f.Type == parm.Type);
            exp = exp.AndIF(parm.State.IfNotEmpty(), f => f.State == parm.State);
            exp = exp.AndIF(parm.ManufactureTime != null, f => f.ManufactureTime == parm.ManufactureTime);
            exp = exp.AndIF(parm.ManufactureTime == null, f => f.ManufactureTime <= DateTime.Now.ToShortDateString().ParseToDateTime());
            var query = Queryable().Where(exp.ToExpression())
           .OrderBy(it => it.ManufactureTime, OrderByType.Desc);
            return query.ToPage(parm);
        }

        public PagedInfo<Product> GetPage(params string[] uids)
        {
            throw new NotImplementedException();
        }

        public List<Product> GetProducts(ProductQueryDto parm)
        {
            throw new NotImplementedException();
        }

        public List<Product> GetProducts(params string[] uids)
        {
            throw new NotImplementedException();
        }

        public int Update(ProductQueryDto productQueryDto, Product parm)
        {
            throw new NotImplementedException();
        }

        public int Update(IEnumerable<string> uids, IEnumerable<Product> parm)
        {
            throw new NotImplementedException();
        }
    }
}
