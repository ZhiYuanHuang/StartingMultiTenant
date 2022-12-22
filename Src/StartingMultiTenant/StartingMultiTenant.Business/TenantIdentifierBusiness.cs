using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StartingMultiTenant.Business
{
    public class TenantIdentifierBusiness
    {
        private readonly TenantIdentifierRepository _tenantIdentifierRepo;
        private readonly TenantDomainRepository _tenantDomainRepo;
        public TenantIdentifierBusiness(TenantDomainRepository tenantDomainRepo,
            TenantIdentifierRepository tenantIdentifierRepo) {
            _tenantDomainRepo= tenantDomainRepo;
            _tenantIdentifierRepo= tenantIdentifierRepo;
        }

        public bool ExistTenant(string tenantDomain,string tenantIdentifier) {
            List<TenantIdentifierModel> tenantList= _tenantIdentifierRepo.GetTenantListByDomain(tenantDomain);
            if (!tenantList.Any()) {
                return false;
            }

            var existTenant= tenantList.FirstOrDefault(x => string.Compare(x.TenantIdentifier, tenantIdentifier, 0) == 0);
            return existTenant!= null;
        }
    }
}
