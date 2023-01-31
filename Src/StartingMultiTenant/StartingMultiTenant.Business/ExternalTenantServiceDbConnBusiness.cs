using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Business
{
    public class ExternalTenantServiceDbConnBusiness : BaseBusiness<ExternalTenantServiceDbConnModel>
    {
        private readonly ExternalTenantServiceDbConnRepository _repo;
        public ExternalTenantServiceDbConnBusiness(ExternalTenantServiceDbConnRepository repository, 
            ILogger<ExternalTenantServiceDbConnBusiness> logger)
            : base(repository, logger) {
            _repo = repository;
        }

        public PagingData<ExternalTenantServiceDbConnModel> GetPage(string tenantDomain, string tenantIdentifier, string serviceIdentifier,int pageSize, int pageIndex) {
            return _repo.GetPage(pageSize,pageIndex,tenantDomain,tenantIdentifier,serviceIdentifier);
        }

        public List<ExternalTenantServiceDbConnModel> GetByTenantAndService(string tenantDomain, string tenantIdentifier, string serviceIdentifier = null) {
            return _repo.GetByTenantAndService(tenantDomain,tenantIdentifier,serviceIdentifier);
        }
    }
}
