using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthorizePolicyConst.Sys_Policy)]
    public class TenantDomainController : ControllerBase
    {
        private readonly TenantDomainBusiness _tenantDomainBusiness;
        public TenantDomainController(TenantDomainBusiness tenantDomainBusiness) {
            _tenantDomainBusiness = tenantDomainBusiness;
        }

        [HttpPost]
        public AppResponseDto<Int64> Add(AppRequestDto<TenantDomainModel> requestDto) {
            if (requestDto?.Data == null) {
                return new AppResponseDto<Int64>(false);
            }

            bool result= _tenantDomainBusiness.Insert(requestDto.Data,out Int64 id);
            return new AppResponseDto<Int64>(result) { Result=id};
        }

        [HttpPost]
        public AppResponseDto Delete(AppRequestDto<string> requestDto) {
            if (string.IsNullOrEmpty(requestDto?.Data)) {
                return new AppResponseDto(false);
            }

            bool result = _tenantDomainBusiness.Delete(requestDto.Data);
            return new AppResponseDto(result);
        }

        [HttpPost]
        public AppResponseDto<PagingData<TenantDomainModel>> GetList(AppRequestDto<PagingParam<Dictionary<string,string>>> requestDto) {
            string tenantDomain = null;
            if(requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("tenantDomain") && !string.IsNullOrEmpty(requestDto.Data.Filter["tenantDomain"])) {
                tenantDomain = requestDto.Data.Filter["tenantDomain"];
            }
            var list = _tenantDomainBusiness.GetPage(tenantDomain,requestDto.Data.PageSize,requestDto.Data.PageIndex);
            return new AppResponseDto<PagingData<TenantDomainModel>>() { Result = list };
        }
    }
}
