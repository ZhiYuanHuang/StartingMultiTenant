﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Service;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthorizePolicyConst.Sys_Policy)]
    public class InternalDbConnController : ControllerBase
    {
        private readonly TenantServiceDbConnBusiness _internalDbConnBusiness;
        private readonly EncryptService _encryptService;
        private readonly TenantDomainBusiness _tenantDomainBusiness;
        private readonly ServiceInfoBusiness _serviceInfoBusiness;
        public InternalDbConnController(TenantServiceDbConnBusiness internalDbConnBusiness,
            EncryptService encryptService,
            TenantDomainBusiness tenantDomainBusiness,
            ServiceInfoBusiness serviceInfoBusiness) {
            _internalDbConnBusiness = internalDbConnBusiness;
            _encryptService = encryptService;
            _tenantDomainBusiness = tenantDomainBusiness;
            _serviceInfoBusiness = serviceInfoBusiness;
        }

        [HttpGet]
        public AppResponseDto<TenantServiceDbConnDto> Get(Int64 id) {
            var model = _internalDbConnBusiness.Get(id);

            if (model == null) {
                return new AppResponseDto<TenantServiceDbConnDto>(false);
            }

            var dto = convertFromModel(model);

            return new AppResponseDto<TenantServiceDbConnDto>() { Result = dto };
        }

        [HttpPost]
        public AppResponseDto<PagingData<TenantServiceDbConnDto>> GetList(AppRequestDto<PagingParam<Dictionary<string, object>>> requestDto) {
            string tenantDomain = null;
            if (requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("tenantDomain") && requestDto.Data.Filter["tenantDomain"] != null) {
                tenantDomain = requestDto.Data.Filter["tenantDomain"].ToString();
            } else if (requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("tenantDomainId") && requestDto.Data.Filter["tenantDomainId"] != null) {
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

            var list = _internalDbConnBusiness.GetPage(convertFromModel,tenantDomain, tenantIdentifier, serviceIdentifier, requestDto.Data.PageSize, requestDto.Data.PageIndex);
           
            return new AppResponseDto<PagingData<TenantServiceDbConnDto>>() { Result = list };
        }


        private TenantServiceDbConnDto convertFromModel(TenantServiceDbConnModel model) {
            var dto = new TenantServiceDbConnDto() {
                Id = model.Id,
                TenantDomain = model.TenantDomain,
                TenantIdentifier = model.TenantIdentifier,
                ServiceIdentifier = model.ServiceIdentifier,
                DbIdentifier = model.DbIdentifier,
                CreateScriptName= model.CreateScriptName,
                CreateScriptVersion= model.CreateScriptVersion,
                CurSchemaVersion= model.CurSchemaVersion,
                DbServerId=model.DbServerId,
                EncryptedConnStr=model.EncryptedConnStr,
            };

            if (!string.IsNullOrEmpty(model.EncryptedConnStr)) {
                try {
                    dto.DbConnStr = _encryptService.Decrypt_DbConn(model.EncryptedConnStr);
                } catch {
                }
            }

            return dto;
        }
    }
}
