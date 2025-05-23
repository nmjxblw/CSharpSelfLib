using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Content.Dto;
using ZR.Model.Dto;
using ZR.Model.Models;

namespace ZR.Admin.WebApi.Controllers.Product
{
    //[Verify]
    [Route("product/product")]
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
        public IActionResult Get(string uid)
        {
            //long userId = HttpContext.GetUId();
            var model = ProductService.GetInfo(uid);
            ApiResult apiResult = ApiResult.Success(model);
            return ToResponse(apiResult);
        }
    }
}
