using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Model.Dto.ExportInterfaceDto;
using StartingMultiTenant.Repository;
using StartingMultiTenant.Service;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TenantCenterController : ControllerBase
    {
        private readonly SingleTenantService _singleTenantService;
        private readonly TenantDomainBusiness _tenantDomainBusiness;
        private readonly TenantIdentifierBusiness _tenantIdentifierBusiness;
        private readonly TenantServiceDbConnBusiness _tenantServiceDbConnBusiness;
        private readonly ExternalTenantServiceDbConnBusiness _externalDbConnBusiness;
        private readonly EncryptService _encryptService;
        private readonly ApiClientBusiness _apiClientBusiness;
        public TenantCenterController(SingleTenantService singleTenantService,
            TenantDomainBusiness tenantDomainBusiness,
            TenantIdentifierBusiness tenantIdentifierBusiness,
            TenantServiceDbConnBusiness tenantServiceDbConnBusiness,
            ExternalTenantServiceDbConnBusiness externalDbConnBusiness,
            ApiClientBusiness apiClientBusiness,
            EncryptService encryptService) {
            _singleTenantService = singleTenantService;
            _tenantDomainBusiness = tenantDomainBusiness;
            _tenantIdentifierBusiness = tenantIdentifierBusiness;
            _tenantServiceDbConnBusiness = tenantServiceDbConnBusiness;
            _externalDbConnBusiness = externalDbConnBusiness;
            _encryptService = encryptService;
            _apiClientBusiness = apiClientBusiness;
        }

        [HttpPost]
        [Authorize(Policy = AuthorizePolicyConst.User_Write_Policy)]
        public async Task<AppResponseDto> Create(AppRequestDto<TenantCenterCreateDto> requestDto) {
            TenantCenterCreateDto createTenantDto = requestDto.Data;
            if (createTenantDto == null || string.IsNullOrEmpty(createTenantDto.TenantDomain) || string.IsNullOrEmpty(createTenantDto.TenantIdentifier)) {
                return new AppResponseDto(false) { ErrorMsg = "must commit tenant domain and identitifer" };
            }

            if (!_tenantDomainBusiness.Exist(createTenantDto.TenantDomain)) {
                return new AppResponseDto(false) { ErrorMsg = $"tenantdomain {createTenantDto.TenantDomain} not exists" };
            }

            bool existed = _tenantIdentifierBusiness.ExistTenant(createTenantDto.TenantDomain, createTenantDto.TenantIdentifier);
            if (existed) {
                return new AppResponseDto(false) { ErrorMsg = $"tenant {createTenantDto.TenantIdentifier}.{createTenantDto.TenantDomain} had existed" };
            }

            Int64 id = 0;
            string tenantGuid = string.Empty;
            if (!existed) {
                tenantGuid = Guid.NewGuid().ToString("N");
                var tenantModel = new TenantIdentifierModel() {
                    TenantDomain = createTenantDto.TenantDomain,
                    TenantIdentifier = createTenantDto.TenantIdentifier,
                    TenantGuid = tenantGuid,
                };
                if (!string.IsNullOrEmpty(createTenantDto.TenantName)) {
                    tenantModel.TenantName = createTenantDto.TenantName;
                }
                if (!string.IsNullOrEmpty(createTenantDto.Description)) {
                    tenantModel.Description = createTenantDto.Description;
                }
                bool toInsertSuccess = _tenantIdentifierBusiness.Insert(tenantModel, out id);
                if (!toInsertSuccess) {
                    return new AppResponseDto(false) { ErrorMsg = "insert tenant error!" };
                }
            }

            //no db to create
            if (createTenantDto.CreateDbScripts == null || !createTenantDto.CreateDbScripts.Any()) {
                return new AppResponseDto(true);
            }

            var createDbResult = await _singleTenantService.CreateTenantDbs(id, createTenantDto.TenantDomain, createTenantDto.TenantIdentifier, createTenantDto.CreateDbScripts);

            if (!createDbResult) {
                _tenantIdentifierBusiness.Delete(tenantGuid);
            }

            return new AppResponseDto(createDbResult) { ErrorMsg=createDbResult?string.Empty:"create tenant dbs error"};
        }

        [HttpGet]
        [Authorize(Policy = AuthorizePolicyConst.User_Read_Policy)]
        public AppResponseDto<TenantCenterDbConnsDto> GetDbConn(string tenantDomain,string tenantIdentifier,string? serviceIdentifier) {
            if(string.IsNullOrEmpty(tenantDomain) || string.IsNullOrEmpty(tenantIdentifier)) {
                return new AppResponseDto<TenantCenterDbConnsDto>(false) { 
                    ErrorMsg="tenantdomain and tenantidentifier cann't be empty!"
                };
            }
            
            List<ExternalTenantServiceDbConnModel> externalList = _externalDbConnBusiness.GetByTenantAndService(tenantDomain, tenantIdentifier, serviceIdentifier);
            List<TenantCenterDbConnDto> externalDbConnList = new List<TenantCenterDbConnDto>();
            if (externalList.Any()) {
                foreach (var externalDbConn in externalList) {
                    string decryptConn = _encryptService.Decrypt_DbConn(externalDbConn.EncryptedConnStr);

                    externalDbConnList.Add(new TenantCenterDbConnDto() { 
                        ServiceIdentifier=externalDbConn.ServiceIdentifier,
                        DbIdentifier=externalDbConn.DbIdentifier,
                        DbConn= decryptConn,
                    });
                }
            }

            List<TenantServiceDbConnModel> innerlist = _tenantServiceDbConnBusiness.GetByTenant(tenantDomain, tenantIdentifier, serviceIdentifier);
            List<TenantCenterDbConnDto> innerDbConnList = new List<TenantCenterDbConnDto>();
            if (innerlist.Any()) {
                foreach (var innerDbConn in innerlist) {
                    string decryptConn = _encryptService.Decrypt_DbConn(innerDbConn.EncryptedConnStr);

                    innerDbConnList.Add(new TenantCenterDbConnDto() {
                        ServiceIdentifier = innerDbConn.ServiceIdentifier,
                        DbIdentifier = innerDbConn.DbIdentifier,
                        DbConn = decryptConn,
                    });
                }
            }

            return new AppResponseDto<TenantCenterDbConnsDto>() {
                Result = new TenantCenterDbConnsDto() {
                    TenantDomain = tenantDomain,
                    TenantIdentifier = tenantIdentifier,
                    InnerDbConnList= innerDbConnList,
                    ExternalDbConnList=externalDbConnList
                }
            };
        }
    }
}
