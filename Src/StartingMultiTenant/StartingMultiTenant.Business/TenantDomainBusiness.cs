using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StartingMultiTenant.Business
{
    public class TenantDomainBusiness
    {
        private readonly TenantDomainRepository _tenantDomainRepo;
        private readonly TenantIdentifierRepository _tenantIdentifierRepo;
        private readonly ILogger<TenantDomainBusiness> _logger;
        public TenantDomainBusiness(TenantDomainRepository tenantDomainRepo,
            TenantIdentifierRepository tenantIdentifierRepo,
            ILogger<TenantDomainBusiness> logger) { 
            _tenantDomainRepo = tenantDomainRepo;
            _tenantIdentifierRepo = tenantIdentifierRepo;
            _logger = logger;
        }

        public bool Insert(TenantDomainModel tenantDomain) {
            try {
                return _tenantDomainRepo.Insert(tenantDomain);
            } catch {
                return false;
            }
        }

        public bool Delete(string tenantDomain) {
            List<TenantIdentifierModel> tenantList= _tenantIdentifierRepo.GetTenantListByDomain(tenantDomain);
            if (tenantList.Any()) {
                _logger.LogError($"tenantDomain {tenantDomain} still has {tenantList.Count} tenant");
                
                return false;
            }

            return _tenantDomainRepo.Delete(tenantDomain);
        }

        public List<TenantDomainModel> GetAll() {
            return _tenantDomainRepo.GetEntitiesByQuery();
        }
    }
}
