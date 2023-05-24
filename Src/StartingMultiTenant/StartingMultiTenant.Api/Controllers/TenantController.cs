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
    [Authorize(AuthorizePolicyConst.Sys_Policy)]
    public class TenantController : ControllerBase
    {
        private readonly SingleTenantService _singleTenantService;
        private readonly TenantDomainBusiness _tenantDomainBusiness;
        private readonly TenantIdentifierBusiness _tenantIdentifierBusiness;
        private readonly TenantServiceDbConnBusiness _tenantServiceDbConnBusiness;
        private readonly ExternalTenantServiceDbConnRepository _externalTenantServiceDbConnRepo;
        private readonly EncryptService _encryptService;
        private readonly TenantActionNoticeService _actionNoticeService;
        private readonly CreateDbScriptBusiness _createDbScriptBusiness;
        private readonly ExternalStoreSyncService _externalStoreSyncService;
        public TenantController(SingleTenantService singleTenantService,
            TenantDomainBusiness tenantDomainBusiness,
            TenantIdentifierBusiness tenantIdentifierBusiness,
            TenantServiceDbConnBusiness tenantServiceDbConnBusiness,
            ExternalTenantServiceDbConnRepository externalTenantServiceDbConnRepo,
            TenantActionNoticeService actionNoticeService,
            CreateDbScriptBusiness createDbScriptBusiness,
            EncryptService encryptService,
            ExternalStoreSyncService externalStoreSyncService) {
            _singleTenantService = singleTenantService;
            _tenantDomainBusiness = tenantDomainBusiness;
            _tenantIdentifierBusiness = tenantIdentifierBusiness;
            _tenantServiceDbConnBusiness = tenantServiceDbConnBusiness;
            _externalTenantServiceDbConnRepo = externalTenantServiceDbConnRepo;
            _encryptService = encryptService;
            _createDbScriptBusiness = createDbScriptBusiness;
            _actionNoticeService = actionNoticeService;
            _externalStoreSyncService = externalStoreSyncService;
        }

        [HttpPost]
        public async Task<AppResponseDto<Int64>> Add(AppRequestDto<CreateTenantDto> requestDto) {
            if (requestDto.Data == null) {
                return new AppResponseDto<Int64>(false);
            }

            CreateTenantDto createTenantDto = requestDto.Data;
            var domainModel = _tenantDomainBusiness.Get(requestDto.Data.TenantDomainId);
            if (domainModel != null) {
                createTenantDto.TenantDomain = domainModel.TenantDomain;
            } else {
                return new AppResponseDto<Int64>(false) { ErrorMsg = $"tenantdomain {requestDto.Data.TenantDomainId} not exists" };
            }

            if (string.IsNullOrEmpty(createTenantDto.TenantDomain) || string.IsNullOrEmpty(createTenantDto.TenantIdentifier)) {
                return new AppResponseDto<Int64>(false);
            }

            if (!string.IsNullOrEmpty(createTenantDto.TenantDomain) && (createTenantDto.TenantDomain.IndexOfAny(SysInnerConst.Invalid_Char_Arr) != -1)) {
                return new AppResponseDto<long>(false) { ErrorMsg = $"TenantDomain cann't use any of '{string.Join("','", SysInnerConst.Invalid_Char_Arr)}'" };
            }
            if (!string.IsNullOrEmpty(createTenantDto.TenantIdentifier) && (createTenantDto.TenantIdentifier.IndexOfAny(SysInnerConst.Invalid_Char_Arr) != -1)) {
                return new AppResponseDto<long>(false) { ErrorMsg = $"TenantDomain cann't use any of '{string.Join("','", SysInnerConst.Invalid_Char_Arr)}'" };
            }

            bool existed = _tenantIdentifierBusiness.ExistTenant(createTenantDto.TenantDomain, createTenantDto.TenantIdentifier);
            if(existed && !createTenantDto.OverrideWhenExisted) {
                return new AppResponseDto<Int64>(false) { ErrorMsg = $"tenantdomain {createTenantDto.TenantDomain} identifier {createTenantDto.TenantIdentifier} had existed" };
            }

            string tenantGuid = string.Empty;
            Int64 id = 0;
            if (!existed) {
                tenantGuid = Guid.NewGuid().ToString("N");
                bool toInsertSuccess = _tenantIdentifierBusiness.Insert(new TenantIdentifierModel() { 
                    TenantDomain=createTenantDto.TenantDomain,TenantIdentifier=createTenantDto.TenantIdentifier,TenantGuid=tenantGuid
                    ,TenantName=createTenantDto.TenantName,Description=createTenantDto.Description
                },out id);
                if(!toInsertSuccess) {
                    return new AppResponseDto<Int64>(false);
                }
            }

            bool result = true;
            if (createTenantDto.CreateDbs != null) {
                List<Int64> createDbScriptIds = createTenantDto.CreateDbs.Values.ToList();
                if (createDbScriptIds.Any()) {
                    result = await _singleTenantService.CreateTenantDbs(id,createTenantDto.TenantDomain, createTenantDto.TenantIdentifier, createDbScriptIds);
                }
            }

            if (!result) {
                if (!existed) {
                    _tenantIdentifierBusiness.Delete(id);
                }
            } 

            return new AppResponseDto<Int64>(result) {Result=id};
        }

        [HttpPut]
        public async Task<AppResponseDto<TenantIdentifierDto>> Update(AppRequestDto<CreateTenantDto> requestDto) {
            CreateTenantDto createTenantDto = requestDto.Data;

            TenantIdentifierDto tenantIdentifierDto = new TenantIdentifierDto() {
                Id = createTenantDto.Id,
                TenantDomain = createTenantDto.TenantDomain,
                TenantIdentifier = createTenantDto.TenantIdentifier,
                TenantGuid = createTenantDto.TenantGuid,
                TenantName=createTenantDto.TenantName,
                Description=createTenantDto.Description,
            };

            //don't change
            if (createTenantDto.CreateDbs==null) {
                return new AppResponseDto<TenantIdentifierDto>() { Result = tenantIdentifierDto };
            }

            List<Int64> newCreateDbScriptIds = createTenantDto.CreateDbs.Values.ToList();
            var createdScripts = _createDbScriptBusiness.GetTenantCreateScripts(createTenantDto.Id);
            var createdScriptIds = createdScripts.Select(x => x.Id);
            newCreateDbScriptIds = newCreateDbScriptIds.Except(createdScriptIds).ToList();

            _tenantIdentifierBusiness.Update(tenantIdentifierDto);
            if (!newCreateDbScriptIds.Any()) {
                return new AppResponseDto<TenantIdentifierDto>() { Result = tenantIdentifierDto };
            }

            var result = await _singleTenantService.OverrideTenantDbs(createTenantDto.Id,createTenantDto.TenantDomain, createTenantDto.TenantIdentifier, newCreateDbScriptIds);
            
          
            return new AppResponseDto<TenantIdentifierDto>(result) { Result = tenantIdentifierDto };
        }

        [HttpGet]
        public AppResponseDto<TenantIdentifierDto> Get(Int64 id) {
            var model = _tenantIdentifierBusiness.Get(id);

            if (model == null) {
                return new AppResponseDto<TenantIdentifierDto>(false);
            }
            var createScripts= _createDbScriptBusiness.GetTenantCreateScripts(model.Id);
            var dto=_tenantIdentifierBusiness.ConvertFromModel(model);
            dto.CreateDbScriptIds = createScripts.Select(x=>x.Id).ToList();
            dto.CreateDbs = createScripts.ToDictionary(x => x.Name, v => v.Id);
            
            return new AppResponseDto<TenantIdentifierDto>() { Result = dto };
        }

        [HttpPost]
        public AppResponseDto<PagingData<TenantIdentifierDto>> GetList(AppRequestDto<PagingParam<Dictionary<string,object>>> requestDto) {
            string tenantDomain = null;
            if (requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("tenantDomain") && requestDto.Data.Filter["tenantDomain"]!=null) {
                string idStr= requestDto.Data.Filter["tenantDomain"].ToString();
                if(int.TryParse(idStr,out int id)) {
                    var domainObj=_tenantDomainBusiness.Get(id);
                    if(domainObj!=null) {
                        tenantDomain = domainObj.TenantDomain;
                    }
                }
            }

            var pageList = _tenantIdentifierBusiness.GetPage(tenantDomain, requestDto.Data.PageSize, requestDto.Data.PageIndex);
            return new AppResponseDto<PagingData<TenantIdentifierDto>>() { 
                Result= pageList
            };
        }

        [HttpPost] 
        public AppResponseDto<TenantServiceDbConnsDto> GetTenantDbConn(AppRequestDto<TenantInfoDto> requestDto) {
            if (requestDto.Data == null) {
                return new AppResponseDto<TenantServiceDbConnsDto>(false);
            }

            List<ExternalTenantServiceDbConnModel> externalList = _externalTenantServiceDbConnRepo.GetByTenantAndService(requestDto.Data.TenantDomain,requestDto.Data.TenantIdentifier);
            List<ServiceDbConnDto> externalDbConns = new List<ServiceDbConnDto>();
            List<ServiceDbConnDto> mergeDbConns = new List<ServiceDbConnDto>();
            if (externalList.Any()) {
                foreach(var externalDbConn in externalList) {
                    ServiceDbConnDto dbConn = new ServiceDbConnDto() {
                        Id=externalDbConn.Id,
                        ServiceIdentifier = externalDbConn.ServiceIdentifier,
                        DbIdentifier = externalDbConn.DbIdentifier,
                    };
                    dbConn.DecryptDbConn = _encryptService.Decrypt_DbConn(externalDbConn.EncryptedConnStr);

                    if (!string.IsNullOrEmpty(externalDbConn.OverrideEncryptedConnStr)) {
                        dbConn.OverrideDbConn = _encryptService.Decrypt_DbConn(externalDbConn.OverrideEncryptedConnStr);
                    }

                    externalDbConns.Add(dbConn);
                    mergeDbConns.Add(dbConn);
                }
            }

            List<TenantServiceDbConnModel> list = _tenantServiceDbConnBusiness.GetByTenant(requestDto.Data.TenantDomain, requestDto.Data.TenantIdentifier);
            List<ServiceDbConnDto> innerDbConns = new List<ServiceDbConnDto>();
            if (list.Any()) {
                foreach (var tenantServiceDbConn in list) {
                    ServiceDbConnDto dbConn = new ServiceDbConnDto() {
                        Id=tenantServiceDbConn.Id,
                        ServiceIdentifier = tenantServiceDbConn.ServiceIdentifier,
                        DbIdentifier = tenantServiceDbConn.DbIdentifier,
                        MajorVersion=tenantServiceDbConn.CreateScriptVersion,
                        MinorVersion=tenantServiceDbConn.CurSchemaVersion
                    };

                    dbConn.DecryptDbConn = _encryptService.Decrypt_DbConn(tenantServiceDbConn.EncryptedConnStr);
                    innerDbConns.Add(dbConn);
                    if(mergeDbConns.FirstOrDefault(x=>x.ServiceIdentifier==tenantServiceDbConn.ServiceIdentifier && x.DbIdentifier == tenantServiceDbConn.DbIdentifier) == null) {
                        mergeDbConns.Add(dbConn);
                    }
                }
            }

            return new AppResponseDto<TenantServiceDbConnsDto>() {
                Result = new TenantServiceDbConnsDto() { 
                    InnerDbConnList=innerDbConns,
                    ExternalDbConnList=externalDbConns,
                    MergeDbConnList=mergeDbConns,
                    TenantDomain= requestDto.Data.TenantDomain,
                    TenantIdentifier= requestDto.Data.TenantIdentifier,
                }
            };
        }

        [HttpGet]
        public AppResponseDto TriggerDbConnsModify(Int64 id) {
            var model= _tenantIdentifierBusiness.Get(id);
            if(model != null)
            {
                _actionNoticeService.PublishTenantDbConnsModify(model.TenantDomain, model.TenantIdentifier);
            }

            return new AppResponseDto(true);
        }

        [HttpGet]
        public AppResponseDto TriggerAllClear() {
            _actionNoticeService.PublishManualAllClear();

            return new AppResponseDto(true);
        }

        [HttpGet]
        public async Task<AppResponseDto> SyncToExternalStore(Int64 id) {
            var result=await _externalStoreSyncService.SyncToExternalStore(id);
            return new AppResponseDto(result) { ErrorMsg="sync error"};
        }

        [HttpGet]
        public async Task<AppResponseDto> FullSyncToExternalStore() {
            var result = await _externalStoreSyncService.SyncToExternalStore();
            Thread.Sleep(1000*10);
            return new AppResponseDto(result) { ErrorMsg = "full sync error" };
        }
    }
}
