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
        private readonly ClientScopesRepository _clientScopesRepo;
        public ApiScopeBusiness(ApiScopeRepository apiScopeRepo,
            ClientScopesRepository clientScopesRepo) {
            _apiScopeRepo = apiScopeRepo;
            _clientScopesRepo= clientScopesRepo;
        }

        public List<ApiScopeModel> GetAll() {
            return _apiScopeRepo.GetEntitiesByQuery();
        }

        public bool Insert(ApiScopeModel apiScope) {
            return _apiScopeRepo.InsertOrUpdate(apiScope);
        }

        public bool Delete(string name) {
            int count= _clientScopesRepo.GetCountByScope(name);
            if (count > 0) {
                return false;
            }
            return _apiScopeRepo.Delete(name);
        }
    }
}
