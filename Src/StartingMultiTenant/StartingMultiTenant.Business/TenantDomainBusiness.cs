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
    public class TenantDomainBusiness: BaseBusiness<TenantDomainModel>
    {
        private readonly TenantDomainRepository _tenantDomainRepo;
        private readonly TenantIdentifierRepository _tenantIdentifierRepo;
        public TenantDomainBusiness(TenantDomainRepository tenantDomainRepo,
            TenantIdentifierRepository tenantIdentifierRepo,
            ILogger<TenantDomainBusiness> logger):base(tenantDomainRepo,logger) { 
            _tenantDomainRepo = tenantDomainRepo;
            _tenantIdentifierRepo = tenantIdentifierRepo;
        }

        public bool Exist(string tenantDomain) {
            var model= _tenantDomainRepo.Get(tenantDomain);
            return model!= null;
        }

        public TenantDomainModel Get(string tenantDomain) {
            return _tenantDomainRepo.Get(tenantDomain);
        }

        public bool Insert(TenantDomainModel tenantDomain,out Int64 id) {
            id = 0;
            try {
                return _tenantDomainRepo.Insert(tenantDomain,out id);
            } catch {
                return false;
            }
        }

        public override Tuple<bool,string> Delete(Int64 id) {
            var domain= Get(id);
            if (domain == null) {
                return Tuple.Create(false,"domain no exist") ;
            }

            return Delete(domain);
        }

        public Tuple<bool,string> Delete(TenantDomainModel model) {
            List<TenantIdentifierModel> tenantList= _tenantIdentifierRepo.GetTenantListByDomain(model.TenantDomain);
            if (tenantList.Any()) {
                _logger.LogError($"tenantDomain {model.TenantDomain} still has {tenantList.Count} tenant");
                
                return Tuple.Create(false, $"tenantDomain {model.TenantDomain} still has {tenantList.Count} tenant");
            }

            bool result = false;
            try {
                result= _tenantDomainRepo.Delete(model.Id);
            }
            catch(Exception ex) {
                _logger.LogError($"delete domain {model.TenantDomain} raise error", ex);
                return Tuple.Create(false, $"delete domain {model.TenantDomain} raise error");
            }

            return Tuple.Create(result,string.Empty);
        }

        public PagingData<TenantDomainModel> GetPage(string tenantDomain,int pageSize,int pageIndex) {
            return _tenantDomainRepo.GetPage(pageSize,pageIndex,tenantDomain);
        }
    }
}
