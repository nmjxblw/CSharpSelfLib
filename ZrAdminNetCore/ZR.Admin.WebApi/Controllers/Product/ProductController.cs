using Aliyun.OSS;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Content;
using ZR.Model.Content.Dto;
using ZR.Model.Dto;
using ZR.Model.Models;
using ZR.Model.Product;
using ZR.Model.Product.Dto;
using ZR.Model.System;
namespace ZR.Admin.WebApi.Controllers
{
    //[Verify]
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "product")]
    public class ProductController : BaseController
    {
        private readonly ISysRoleService RoleService;
        private readonly ISysUserRoleService UserRoleService;
        private readonly ISysUserPostService UserPostService;
        private readonly ISysUserService UserService;
        private readonly IProductService ProductService;

        public ProductController(
            ISysRoleService roleService,
            ISysUserRoleService userRoleService,
            ISysUserPostService userPostService,
            ISysUserService userService,
            IProductService productService)
        {
            RoleService = roleService;
            UserRoleService = userRoleService;
            UserPostService = userPostService;
            UserService = userService;
            ProductService = productService;
        }
        /// <summary>
        /// 通过UID下载产品信息接口
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet("{uid}")]
        [AllowAnonymous]
        [Log("产品下载", BusinessType = BusinessType.DOWNLOAD)]
        public IActionResult Download(string uid)
        {
            Product model = ProductService.GetInfo(uid);
            ApiResult apiResult = model.IsNotEmpty() ? ApiResult.Success(model) : ApiResult.Error("输入了无效UID");
            return ToResponse(apiResult);
        }
        /// <summary>
        /// 常用产品信息更新接口（部分Property需要通过Modify相关API修改）
        /// </summary>
        /// <param name="inputObject"></param>
        /// <returns></returns>
        [HttpPut]
        [AllowAnonymous]
        [Log("产品上传/更新", BusinessType = BusinessType.UPDATE)]
        public IActionResult Upload([FromBody] object? inputObject)
        {
            if (inputObject == null)
            {
                return ToResponse(ApiResult.Error("输入对象不能为空"));
            }
            inputObject = inputObject.Adapt<Product>().ToUpdate(context: HttpContext);
            if (inputObject is Product product)
            {
                int result = ProductService.Refersh(product);
                return ToResponse(result > 0 ? ApiResult.Success("更新成功") : ApiResult.Error("更新失败"));
            }
            else
            {
                return ToResponse(ApiResult.Error("输入对象转换失败，请检查输入数据格式"));
            }
        }
        /// <summary>
        /// 修改指定产品信息
        /// </summary>
        [HttpPut("{propertyName}")]
        [ActionPermissionFilter("system:product:edit")]
        [Log("产品更新", BusinessType = BusinessType.UPDATE)]
        public IActionResult Modify(string propertyName, [FromBody] Product product)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return ToResponse(ApiResult.Error("属性名称不能为空"));
            }
            if (product == null)
            {
                return ToResponse(ApiResult.Error("输入对象不能为空"));
            }
            product = product.Adapt<Product>().ToUpdate(context: HttpContext);
            int result = ProductService.ModifyProperty(propertyName, product);
            return ToResponse(result > 0 ? ApiResult.Success("更新成功") : ApiResult.Error("更新失败"));
        }
        /// <summary>
        /// Deletes a product identified by the specified unique identifier (UID).
        /// </summary>
        /// <param name="uid">The unique identifier of the product to be deleted. Cannot be null or empty.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.  Returns a success response if the
        /// product was deleted successfully; otherwise, returns an error response.</returns>
        [HttpDelete("{uid}")]
        [ActionPermissionFilter("system:product:edit")]
        [Log("移除产品信息",BusinessType = BusinessType.DELETE)]
        public IActionResult Remove(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                return ToResponse(ApiResult.Error("UID不能为空"));
            }
            int result = ProductService.Remove(uid);
            return ToResponse(result > 0 ? ApiResult.Success("删除成功") : ApiResult.Error("删除失败"));
        }
        /// <summary>
        /// Deletes a batch of products identified by their unique identifiers (UIDs).
        /// </summary>
        /// <remarks>This method logs the operation and enforces permissions using the
        /// <c>ActionPermissionFilter</c> attribute.</remarks>
        /// <param name="uids">An array of unique identifiers representing the products to be removed. Cannot be null or empty.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.  Returns a success response if the
        /// products are successfully deleted, or an error response if the operation fails.</returns>
        [HttpDelete]
        [ActionPermissionFilter("system:product:edit")]
        [Log("批量移除产品信息", BusinessType = BusinessType.DELETE)]
        public IActionResult Remove([FromBody] string[] uids)
        {
            if (uids == null || uids.Length == 0)
            {
                return ToResponse(ApiResult.Error("UID列表不能为空"));
            }
            int[] results = ProductService.Remove(uids);
            string errorUIDs = string.Join(", ", results.Where(r => r <= 0).Select((r, i) => $"{uids[i]}"));
            return ToResponse(results.Length > 0 ? ApiResult.Success("删除成功") : ApiResult.Error(ResultCode.CUSTOM_ERROR,$"删除{{ {errorUIDs} }}失败"));
        }
    }
}
