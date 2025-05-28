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
        /// <summary>
        /// 修改特定的产品属性
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        int ModifyProperty(string propertyName, Product product);

       /// <summary>
       /// Removes the item associated with the specified unique identifier.
       /// </summary>
       /// <param name="uid">The unique identifier of the item to remove. Cannot be null or empty.</param>
       /// <returns>The number of items removed. Returns 0 if no item with the specified identifier was found.</returns>
        int Remove(string uid);
        /// <summary>
        /// Removes the specified items identified by their unique identifiers.
        /// </summary>
        /// <param name="uids">A collection of unique identifiers representing the items to be removed. Cannot be null or empty.</param>
        /// <returns>The number of items successfully removed.</returns>
        int[] Remove(IEnumerable<string> uids);
    }
}
