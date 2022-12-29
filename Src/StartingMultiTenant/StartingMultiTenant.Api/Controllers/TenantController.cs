using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Repository;
using StartingMultiTenant.Service;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class TenantController : ControllerBase
    {
        private readonly SingleTenantService _singleTenantService;
        private readonly TenantDomainBusiness _tenantDomainBusiness;
        private readonly TenantIdentifierBusiness _tenantIdentifierBusiness;
        private readonly TenantServiceDbConnBusiness _tenantServiceDbConnBusiness;
        private readonly ExternalTenantServiceDbConnRepository _externalTenantServiceDbConnRepo;
        private readonly EncryptService _encryptService;
        private readonly ApiClientBusiness _apiClientBusiness;
        public TenantController(SingleTenantService singleTenantService,
            TenantDomainBusiness tenantDomainBusiness,
            TenantIdentifierBusiness tenantIdentifierBusiness,
            TenantServiceDbConnBusiness tenantServiceDbConnBusiness,
            ExternalTenantServiceDbConnRepository externalTenantServiceDbConnRepo,
            ApiClientBusiness apiClientBusiness,
            EncryptService encryptService) {
            _singleTenantService = singleTenantService;
            _tenantDomainBusiness = tenantDomainBusiness;
            _tenantIdentifierBusiness = tenantIdentifierBusiness;
            _tenantServiceDbConnBusiness = tenantServiceDbConnBusiness;
            _externalTenantServiceDbConnRepo = externalTenantServiceDbConnRepo;
            _encryptService = encryptService;
            _apiClientBusiness = apiClientBusiness;
        }

        [HttpPost]
        public async Task<AppResponseDto> CreateTenant(AppRequestDto<CreateTenantDto> requestDto) {
            if (requestDto.Data == null) {
                return new AppResponseDto(false);
            }

            CreateTenantDto createTenantDto = requestDto.Data;
            if (string.IsNullOrEmpty(createTenantDto.TenantDomain) || string.IsNullOrEmpty(createTenantDto.TenantIdentifier) || createTenantDto.CreateDbScripts == null || !createTenantDto.CreateDbScripts.Any()) {
                return new AppResponseDto(false);
            }

            if (!_tenantDomainBusiness.Exist(createTenantDto.TenantDomain)) {
                return new AppResponseDto(false) { ErrorMsg = $"tenantdomain {createTenantDto.TenantDomain} not exists" };
            }

            if(!_apiClientBusiness.CheckAuthorization(User.Identity.Name,createTenantDto.TenantDomain,ScopeNameConst.WriteScope)) {
                return new AppResponseDto(false) { ErrorMsg="no enough authorization"};
            }

            bool existed = _tenantIdentifierBusiness.ExistTenant(createTenantDto.TenantDomain, createTenantDto.TenantIdentifier);
            if (existed && !createTenantDto.OverrideWhenExisted) {
                return new AppResponseDto(false) { ErrorMsg = $"tenantdomain {createTenantDto.TenantDomain} identifier {createTenantDto.TenantIdentifier} had existed" };
            }

            string tenantGuid = string.Empty;
            if (!existed) {
                tenantGuid = Guid.NewGuid().ToString("N");
                bool toInsertSuccess = _tenantIdentifierBusiness.Insert(new TenantIdentifierModel() { TenantDomain = createTenantDto.TenantDomain, TenantIdentifier = createTenantDto.TenantIdentifier, TenantGuid = tenantGuid });
                if (!toInsertSuccess) {
                    return new AppResponseDto(false);
                }
            }

            var result = await _singleTenantService.CreateTenantDbs(createTenantDto.TenantDomain, createTenantDto.TenantIdentifier, createTenantDto.CreateDbScripts, createTenantDto.OverrideWhenExisted);

            if (!result) {
                if (!existed) {
                    _tenantIdentifierBusiness.Delete(tenantGuid);
                }
            }

            return new AppResponseDto(result);
        }

        [HttpPost]
        public AppResponseDto<TenantServiceDbConnsDto> GetTenantDbConn(AppRequestDto<TenantServiceInfoDto> requestDto) {
            if (requestDto.Data == null) {
                return new AppResponseDto<TenantServiceDbConnsDto>(false);
            }

            if (!_apiClientBusiness.CheckAuthorization(User.Identity.Name, requestDto.Data.TenantDomain, ScopeNameConst.ReadScope)) {
                return new AppResponseDto<TenantServiceDbConnsDto>(false) { ErrorMsg = "no enough authorization" };
            }

            List<ExternalTenantServiceDbConnModel> externalList = _externalTenantServiceDbConnRepo.GetByTenantAndService(requestDto.Data.TenantDomain, requestDto.Data.TenantIdentifier,requestDto.Data.ServiceIdentifier);
            List<ServiceDbConnsDto> mergeDbConns = new List<ServiceDbConnsDto>();
            if (externalList.Any()) {
                foreach (var externalDbConn in externalList) {
                    ServiceDbConnsDto dbConn = new ServiceDbConnsDto() {
                        Id = externalDbConn.Id,
                        ServiceIdentifier = externalDbConn.ServiceIdentifier,
                        DbIdentifier = externalDbConn.DbIdentifier,
                    };
                    dbConn.DecryptDbConn = _encryptService.Decrypt_DbConn(externalDbConn.EncryptedConnStr);

                    if (!string.IsNullOrEmpty(externalDbConn.OverrideEncryptedConnStr)) {
                        dbConn.OverrideDbConn = _encryptService.Decrypt_DbConn(externalDbConn.OverrideEncryptedConnStr);
                    }

                    mergeDbConns.Add(dbConn);
                }
            }

            List<TenantServiceDbConnModel> list = _tenantServiceDbConnBusiness.GetByTenant(requestDto.Data.TenantDomain, requestDto.Data.TenantIdentifier, requestDto.Data.ServiceIdentifier);
            if (list.Any()) {
                foreach (var tenantServiceDbConn in list) {
                    ServiceDbConnsDto dbConn = new ServiceDbConnsDto() {
                        Id = tenantServiceDbConn.Id,
                        ServiceIdentifier = tenantServiceDbConn.ServiceIdentifier,
                        DbIdentifier = tenantServiceDbConn.DbIdentifier,
                    };

                    dbConn.DecryptDbConn = _encryptService.Decrypt_DbConn(tenantServiceDbConn.EncryptedConnStr);
                    if (mergeDbConns.FirstOrDefault(x => x.ServiceIdentifier == tenantServiceDbConn.ServiceIdentifier && x.DbIdentifier == tenantServiceDbConn.DbIdentifier) == null) {
                        mergeDbConns.Add(dbConn);
                    }
                }
            }

            return new AppResponseDto<TenantServiceDbConnsDto>() {
                Result = new TenantServiceDbConnsDto() {
                    MergeDbConnList = mergeDbConns,
                    TenantDomain = requestDto.Data.TenantDomain,
                    TenantIdentifier = requestDto.Data.TenantIdentifier,
                }
            };
        }
    }
}
