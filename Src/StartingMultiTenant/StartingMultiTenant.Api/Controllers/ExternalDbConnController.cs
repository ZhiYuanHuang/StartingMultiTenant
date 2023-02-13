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
    public class ExternalDbConnController : ControllerBase
    {
        private readonly ExternalTenantServiceDbConnBusiness _externalConnBusiness;
        private readonly EncryptService _encryptService;
        private readonly TenantDomainBusiness _tenantDomainBusiness;
        private readonly ServiceInfoBusiness _serviceInfoBusiness;
        private readonly TenantIdentifierBusiness _tenantIdentifierBusiness;
        private readonly ExternalStoreSyncService _externalStoreSyncService;
        private readonly TenantActionNoticeService _actionNoticeService;
        public ExternalDbConnController(ExternalTenantServiceDbConnBusiness externalConnBusiness,
            EncryptService encryptService,
            TenantDomainBusiness tenantDomainBusiness,
            ServiceInfoBusiness serviceInfoBusiness,
            TenantIdentifierBusiness tenantIdentifierBusiness,
            ExternalStoreSyncService externalStoreSyncService,
            TenantActionNoticeService actionNoticeService) {
            _externalConnBusiness = externalConnBusiness;
            _encryptService= encryptService;
            _tenantDomainBusiness = tenantDomainBusiness;
            _serviceInfoBusiness = serviceInfoBusiness;
            _tenantIdentifierBusiness = tenantIdentifierBusiness;
            _externalStoreSyncService = externalStoreSyncService;
            _actionNoticeService= actionNoticeService;
        }

        [HttpPost]
        public AppResponseDto<Int64> Add(AppRequestDto<ExternalTenantServiceDbConnDto> requestDto) {
            if (requestDto?.Data == null || string.IsNullOrEmpty(requestDto.Data.DbConnStr)) {
                return new AppResponseDto<Int64>(false);
            }

            requestDto.Data.EncryptedConnStr = _encryptService.Encrypt_DbConn(requestDto.Data.DbConnStr);

            var serviceInfoDto= _serviceInfoBusiness.GetByServiceInfo(requestDto.Data.ServiceInfoId);
            if (serviceInfoDto != null) {
                requestDto.Data.ServiceIdentifier = serviceInfoDto.ServiceIdentifier;

                var dbInfoDto = serviceInfoDto.DbInfos.FirstOrDefault(x => x.DbInfoId == requestDto.Data.DbInfoId);
                if (dbInfoDto != null) {
                    requestDto.Data.DbIdentifier = dbInfoDto.DbIdentifier;
                }
            }

            var domainModel= _tenantDomainBusiness.Get(requestDto.Data.TenantDomainId);
            if(domainModel != null) {
                requestDto.Data.TenantDomain = domainModel.TenantDomain;
            }


            bool result = false;
            Int64 id = 0;
            try {
                result= _externalConnBusiness.Insert(requestDto.Data,out id);
                if (result && _tenantIdentifierBusiness.ExistTenant(requestDto.Data.TenantDomain,requestDto.Data.TenantIdentifier,out TenantIdentifierModel tenantModel)) {
                    _externalStoreSyncService.SyncToExternalStore(tenantModel.Id).ConfigureAwait(false);
                    _actionNoticeService.PublishTenantDbConnsModify(tenantModel.TenantDomain,tenantModel.TenantIdentifier);
                }
            }
            catch(Exception ex) {
                result= true;
            }

            return new AppResponseDto<Int64>(result) { Result=id};
            
        }

        [HttpPut]
        public AppResponseDto<ExternalTenantServiceDbConnDto> Update(AppRequestDto<ExternalTenantServiceDbConnDto> requestDto) {

            if (requestDto.Data == null || requestDto.Data.Id <= 0) {
                return new AppResponseDto<ExternalTenantServiceDbConnDto>(false);
            }

            requestDto.Data.EncryptedConnStr = _encryptService.Encrypt_DbConn(requestDto.Data.DbConnStr);

            bool result = _externalConnBusiness.Update(requestDto.Data);
            if (result) {
                var externalConn = _externalConnBusiness.Get(requestDto.Data.Id);
                if(_tenantIdentifierBusiness.ExistTenant(externalConn.TenantDomain,externalConn.TenantIdentifier,out TenantIdentifierModel tenantModel)) {
                    _externalStoreSyncService.SyncToExternalStore(tenantModel.Id).ConfigureAwait(false);
                    _actionNoticeService.PublishTenantDbConnsModify(tenantModel.TenantDomain, tenantModel.TenantIdentifier);
                }
                
            }
            return new AppResponseDto<ExternalTenantServiceDbConnDto>(result) { Result = requestDto.Data };
        }

        [HttpDelete]
        public AppResponseDto Delete(AppRequestDto<ExternalTenantServiceDbConnDto> requestDto) {

            if (requestDto.Data == null || requestDto.Data.Id <= 0) {
                return new AppResponseDto(false);
            }

            var externalConn = _externalConnBusiness.Get(requestDto.Data.Id);
            var result = _externalConnBusiness.Delete(requestDto.Data.Id);
            if (result.Item1) {
               
                if (_tenantIdentifierBusiness.ExistTenant(externalConn.TenantDomain, externalConn.TenantIdentifier, out TenantIdentifierModel tenantModel)) {
                    _externalStoreSyncService.SyncToExternalStore(tenantModel.Id).ConfigureAwait(false);
                    _actionNoticeService.PublishTenantDbConnsModify(externalConn.TenantDomain, externalConn.TenantIdentifier);
                }
                
            }
            return new AppResponseDto(result.Item1) { ErrorMsg = result.Item2 };
        }

        [HttpDelete]
        public AppResponseDto<List<Int64>> DeleteMany(AppRequestDto<List<Int64>> requestDto) {

            var externalConns= _externalConnBusiness.Get(requestDto.Data);
            var resultTuple = _externalConnBusiness.DeleteMany(requestDto.Data);

            if (resultTuple.Item2.Any()) {
                List<Int64> successIds = resultTuple.Item2;
                externalConns.ForEach(x => {
                    if (successIds.Contains(x.Id)) {
                       
                        if (_tenantIdentifierBusiness.ExistTenant(x.TenantDomain, x.TenantIdentifier, out TenantIdentifierModel tenantModel)) {
                            _externalStoreSyncService.SyncToExternalStore(tenantModel.Id).ConfigureAwait(false);
                            _actionNoticeService.PublishTenantDbConnsModify(x.TenantDomain, x.TenantIdentifier);
                        }
                    }
                });
            }
            
            return new AppResponseDto<List<Int64>>(resultTuple.Item1) {
                Result = resultTuple.Item2,
                ErrorMsg = resultTuple.Item3
            };
        }

        [HttpGet]
        public AppResponseDto<ExternalTenantServiceDbConnDto> Get(Int64 id) {
            var model = _externalConnBusiness.Get(id);

            if (model == null) {
                return new AppResponseDto<ExternalTenantServiceDbConnDto>(false);
            }

            var dto = convertFromModel(model);
            var domainModel= _tenantDomainBusiness.Get(dto.TenantDomain);
            if (domainModel != null) {
                dto.TenantDomainId = domainModel.Id;
            }

            return new AppResponseDto<ExternalTenantServiceDbConnDto>() { Result =dto };
        }

        [HttpPost]
        public AppResponseDto<ExternalTenantServiceDbConnDto> GetMany(AppRequestDto<List<Int64>> requestDto) {
            var models = _externalConnBusiness.Get(requestDto.Data);
            return new AppResponseDto<ExternalTenantServiceDbConnDto>() { ResultList = models.Select(x=>convertFromModel(x)).ToList() };
        }

        [HttpPost]
        public AppResponseDto<PagingData<ExternalTenantServiceDbConnDto>> GetList(AppRequestDto<PagingParam<Dictionary<string, object>>> requestDto) {
            string tenantDomain = null;
            if (requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("tenantDomain") && requestDto.Data.Filter["tenantDomain"] != null) {
                tenantDomain = requestDto.Data.Filter["tenantDomain"].ToString();
            }
            else if (requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("tenantDomainId") && requestDto.Data.Filter["tenantDomainId"] != null) {
                string idStr = requestDto.Data.Filter["tenantDomainId"].ToString();
                if (int.TryParse(idStr, out int id)) {
                    var domainObj = _tenantDomainBusiness.Get(id);
                    if (domainObj != null) {
                        tenantDomain = domainObj.TenantDomain;
                    }
                }
            }
            
            string tenantIdentifier = null;
            if (requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("tenantIdentifier") && requestDto.Data.Filter["tenantIdentifier"] != null) {
                tenantIdentifier = requestDto.Data.Filter["tenantIdentifier"].ToString();
            }

            string serviceIdentifier = null;
            if (requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("serviceIdentifier") && requestDto.Data.Filter["serviceIdentifier"] != null) {
                string idStr = requestDto.Data.Filter["serviceIdentifier"].ToString();
                if (Int64.TryParse(idStr, out Int64 id)) {
                    var serviceInfo = _serviceInfoBusiness.Get(id);
                    if (serviceInfo != null) {
                        serviceIdentifier = serviceInfo.Identifier;
                    }
                }
            }

            var list = _externalConnBusiness.GetPage(tenantDomain,tenantIdentifier,serviceIdentifier, requestDto.Data.PageSize, requestDto.Data.PageIndex);
            PagingData<ExternalTenantServiceDbConnDto> dtoPagingData = new PagingData<ExternalTenantServiceDbConnDto>(list.PageSize, list.PageIndex, list.RecordCount, list.Data.Select(x => convertFromModel(x)).ToList());
            return new AppResponseDto<PagingData<ExternalTenantServiceDbConnDto>>() { Result = dtoPagingData };
        }


        private ExternalTenantServiceDbConnDto convertFromModel(ExternalTenantServiceDbConnModel model) {
            var dto = new ExternalTenantServiceDbConnDto() {
                Id = model.Id,
                TenantDomain = model.TenantDomain,
                TenantIdentifier = model.TenantIdentifier,
                ServiceIdentifier = model.ServiceIdentifier,
                DbIdentifier = model.DbIdentifier,
                UpdateTime=model.UpdateTime,
            };

            if (!string.IsNullOrEmpty(model.EncryptedConnStr)) {
                try {
                    dto.DbConnStr = _encryptService.Decrypt_DbConn(model.EncryptedConnStr);
                } catch {
                }
            }

            if (!string.IsNullOrEmpty(model.OverrideEncryptedConnStr)) {
                try {
                    dto.OverrideDbConnStr = _encryptService.Decrypt_DbConn(model.OverrideEncryptedConnStr);
                } catch {
                }
            }

            return dto;
        }
    }
}
