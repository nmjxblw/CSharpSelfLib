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
        [HttpGet("{uid}")]
        [AllowAnonymous]
        public IActionResult UID(string uid)
        {
            //long userId = HttpContext.GetUId();
            var model = ProductService.GetInfo(uid);
            ApiResult apiResult = ApiResult.Success(model);
            return ToResponse(apiResult);
        }

        [HttpPut]
        [AllowAnonymous]
        [Log("产品更新", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] object? inputObject)
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
    }
}
