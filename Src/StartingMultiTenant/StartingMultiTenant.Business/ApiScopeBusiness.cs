using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Business
{
    public class ApiScopeBusiness
    {
        private readonly ApiScopeRepository _apiScopeRepo;
        private readonly ClientDomainScopeRepository _clientDomainScopeRepo;
        public ApiScopeBusiness(ApiScopeRepository apiScopeRepo,
            ClientDomainScopeRepository clientDomainScopeRepo) {
            _apiScopeRepo = apiScopeRepo;
            _clientDomainScopeRepo= clientDomainScopeRepo;
        }

        public List<ApiScopeModel> GetAll() {
            return _apiScopeRepo.GetEntitiesByQuery();
        }

        public bool Insert(ApiScopeModel apiScope) {
            return _apiScopeRepo.InsertOrUpdate(apiScope);
        }

        public bool Delete(string name) {
            int count= _clientDomainScopeRepo.GetCountByScope(name);
            if (count > 0) {
                return false;
            }
            return _apiScopeRepo.Delete(name);
        }
    }
}
