using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthorizePolicyConst.Sys_Policy)]
    public class ApiScopeController : ControllerBase
    {
        private readonly ApiScopeBusiness _apiScopeBusiness;
        public ApiScopeController(ApiScopeBusiness apiScopeBusiness) {
            _apiScopeBusiness= apiScopeBusiness;
        }

        [HttpGet]
        public AppResponseDto<ApiScopeModel> GetAll() {
            var list= _apiScopeBusiness.GetAll();
            return new AppResponseDto<ApiScopeModel>() { ResultList=list};
        }
    }
}
