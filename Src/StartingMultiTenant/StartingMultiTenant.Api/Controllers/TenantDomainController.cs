using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TenantDomainController : ControllerBase
    {
        private readonly TenantDomainBusiness _tenantDomainBusiness;
        public TenantDomainController(TenantDomainBusiness tenantDomainBusiness) {
            _tenantDomainBusiness = tenantDomainBusiness;
        }

        [HttpPost]
        public AppResponseDto Add(AppRequestDto<TenantDomainModel> requestDto) {
            if (requestDto?.Data == null) {
                return new AppResponseDto(false);
            }

            bool result= _tenantDomainBusiness.Insert(requestDto.Data);
            return new AppResponseDto(result);
        }

        [HttpPost]
        public AppResponseDto Delete(AppRequestDto<string> requestDto) {
            if (string.IsNullOrEmpty(requestDto?.Data)) {
                return new AppResponseDto(false);
            }

            bool result = _tenantDomainBusiness.Delete(requestDto.Data);
            return new AppResponseDto(result);
        }

        [HttpGet]
        public AppResponseDto<TenantDomainModel> Get() {
            List<TenantDomainModel> list = _tenantDomainBusiness.GetAll();
            return new AppResponseDto<TenantDomainModel>() { ResultList = list };
        }
    }
}
