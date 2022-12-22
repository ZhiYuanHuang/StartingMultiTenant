using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Service;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SysTenantController : ControllerBase
    {
        private readonly SingleTenantService _singleTenantService;
        private readonly TenantDomainBusiness _tenantDomainBusiness;
        private readonly TenantIdentifierBusiness _tenantIdentifierBusiness;
        public SysTenantController(SingleTenantService singleTenantService,
            TenantDomainBusiness tenantDomainBusiness,
            TenantIdentifierBusiness tenantIdentifierBusiness) {
            _singleTenantService = singleTenantService;
            _tenantDomainBusiness = tenantDomainBusiness;
            _tenantIdentifierBusiness = tenantIdentifierBusiness;
        }

        [HttpPost]
        public async Task<AppResponseDto> CreateTenant(AppRequestDto<CreateTenantDto> requestDto) {
            if (requestDto.Data == null) {
                return new AppResponseDto(false);
            }

            CreateTenantDto createTenantDto = requestDto.Data;
            if (string.IsNullOrEmpty(createTenantDto.TenantDomain) || string.IsNullOrEmpty(createTenantDto.TenantIdentifier) || createTenantDto.CreateDbScripts==null || !createTenantDto.CreateDbScripts.Any()) {
                return new AppResponseDto(false);
            }

            if (!_tenantDomainBusiness.Exist(createTenantDto.TenantDomain)) {
                return new AppResponseDto(false) { ErrorMsg=$"tenantdomain {createTenantDto.TenantDomain} not exists"};
            }

            if (_tenantIdentifierBusiness.ExistTenant(createTenantDto.TenantDomain, createTenantDto.TenantIdentifier)) {
                return new AppResponseDto(false) { ErrorMsg=$"tenantdomain {createTenantDto.TenantDomain} identifier {createTenantDto.TenantIdentifier} had existed"};
            }

            var result=await _singleTenantService.CreateTenantDbs(createTenantDto.TenantDomain,createTenantDto.TenantIdentifier,createTenantDto.CreateDbScripts);

            return new AppResponseDto(result);
        }
    }
}
