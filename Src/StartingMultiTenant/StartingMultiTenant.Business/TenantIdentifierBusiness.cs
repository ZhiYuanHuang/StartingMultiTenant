using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StartingMultiTenant.Business
{
    public class TenantIdentifierBusiness:BaseBusiness<TenantIdentifierModel>
    {
        private readonly TenantIdentifierRepository _tenantIdentifierRepo;
        private readonly TenantDomainRepository _tenantDomainRepo;
        private readonly CreateDbScriptRepository _createDbScriptRepo;
        public TenantIdentifierBusiness(TenantDomainRepository tenantDomainRepo,
            TenantIdentifierRepository tenantIdentifierRepo,
            CreateDbScriptRepository createDbScriptRepository,
            ILogger<TenantIdentifierBusiness> logger):base(tenantIdentifierRepo,logger) {
            _tenantDomainRepo= tenantDomainRepo;
            _tenantIdentifierRepo= tenantIdentifierRepo;
            _createDbScriptRepo = createDbScriptRepository;
        }

        public bool Insert(TenantIdentifierModel tenantIdentifier) {
            
            return _tenantIdentifierRepo.Insert(tenantIdentifier);
        }

        public bool Delete(string tenantGuid) {
            return _tenantIdentifierRepo.Delete(tenantGuid);
        }

        public bool ExistTenant(string tenantDomain,string tenantIdentifier) {
            List<TenantIdentifierModel> tenantList= _tenantIdentifierRepo.GetTenantListByDomain(tenantDomain);
            if (!tenantList.Any()) {
                return false;
            }

            var existTenant= tenantList.FirstOrDefault(x => string.Compare(x.TenantIdentifier, tenantIdentifier, 0) == 0);
            return existTenant!= null;
        }

        public PagingData<TenantIdentifierDto> GetPage(string tenantDomain, int pageSize, int pageIndex) {
            var pageData= _tenantIdentifierRepo.GetPage(pageSize,pageIndex, ConvertFromModel,tenantDomain);

            //if (pageData.Data.Any()) {
            //    List<Int64> tenantIds= pageData.Data.Select(x => x.Id).ToList();
            //    var tenantsCreateScriptDict= _createDbScriptRepo.GetTenantCreateScripts(tenantIds);
            //    pageData.Data.ForEach(x => {
            //        if (tenantsCreateScriptDict.ContainsKey(x.Id) && tenantsCreateScriptDict[x.Id].Any()) {
            //            x.CreateDbScriptIds=new List<long>(tenantsCreateScriptDict[x.Id]);

            //        }
            //    });
            //}

            return pageData;
        }

        public TenantIdentifierDto ConvertFromModel(TenantIdentifierModel model) {
            return new TenantIdentifierDto() {
                Id = model.Id,
                TenantIdentifier = model.TenantIdentifier,
                TenantDomain = model.TenantDomain,
                TenantGuid = model.TenantGuid,
                CreateDbScriptIds = new List<long>()
            };
        }
    }
}
