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
    public interface IProduct : IBaseService<Product>
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
        /// 更新产品信息
        /// </summary>
        int Update(ProductQueryDto productQueryDto, Product parm);
        /// <summary>
        /// 批量更新产品信息
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="parm"></param>
        /// <returns></returns>
        int Update(IEnumerable<string> uids, IEnumerable<Product> parm);
    }
}
