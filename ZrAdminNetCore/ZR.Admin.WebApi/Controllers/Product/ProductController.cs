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
            //long userId = HttpContext.GetUId();
            var model = ProductService.GetInfo(uid);
            ApiResult apiResult = model.IsNotEmpty() ? ApiResult.Success(model):ApiResult.Error("输入了无效UID");
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
    }
}
