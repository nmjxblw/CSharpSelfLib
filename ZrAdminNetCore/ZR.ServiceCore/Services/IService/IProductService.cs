using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Dto;
using ZR.Model.Models;
using ZR.Model.Product;
using ZR.Model.Product.Dto;
namespace ZR.ServiceCore.Services
{
    public interface IProductService : IBaseService<Product>
    {
        /// <summary>
        /// 获取产品信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        Product GetInfo(string uid);
        /// <summary>
        /// 获取产品列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        PagedInfo<Product> GetPage(ProductQueryDto parm);
        /// <summary>
        /// 获取产品列表
        /// </summary>
        /// <param name="uids"></param>
        /// <returns></returns>
        PagedInfo<Product> GetPage(params string[] uids);
        /// <summary>
        /// 获取产品列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        List<Product> GetProducts(ProductQueryDto parm);
        /// <summary>
        /// 获取产品列表
        /// </summary>
        /// <param name="uids"></param>
        /// <returns></returns>
        List<Product> GetProducts(params string[] uids);
        /// <summary>
        /// 刷新产品信息
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        int Refersh(Product product);
        /// <summary>
        /// 更新产品信息
        /// </summary>
        int Update(ProductQueryDto productQueryDto, Product parm);
        /// <summary>
        /// 批量更新产品信息
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        int[] Update(IEnumerable<Product> parm);
        /// <summary>
        /// 获取产品后端数据查询传输对象
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        ProductQueryDto ToDto(Product product);
    }
}
