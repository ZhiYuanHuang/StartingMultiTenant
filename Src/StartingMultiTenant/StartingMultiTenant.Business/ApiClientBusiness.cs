using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace StartingMultiTenant.Business
{
    public class ApiClientBusiness
    {
        private readonly ApiClientRepository _apiClientRepo;
        private readonly ClientDomainScopeRepository _clientDomainScopeRepo;
        public ApiClientBusiness(ApiClientRepository apiClientRepo,
            ClientDomainScopeRepository clientDomainScopeRepo) {
            _apiClientRepo= apiClientRepo;
            _clientDomainScopeRepo= clientDomainScopeRepo;
        }

        public ApiClientModel Get(string clientId) {
            return _apiClientRepo.Get(clientId);
        }

        public List<ApiClientModel> GetAll() {
            return _apiClientRepo.GetEntitiesByQuery();
        }

        public List<ApiClientModel> GetAdmins() {
            return _apiClientRepo.GetAdmins();
        }

        public bool Add(string clientId,string encryptSecret,string role=RoleConst.Role_User) {
            return _apiClientRepo.Insert(clientId,encryptSecret,role);
        }

        public bool Delete(string clientId) {
            bool result = false;
            try {
                _apiClientRepo.BeginTransaction();

                result= _apiClientRepo.Delete(clientId);
                if (!result) {
                    throw new Exception("delete client error");
                }

                _clientDomainScopeRepo.DeleteByClient(clientId);

                _apiClientRepo.CommitTransaction();
                result = true;
            }
            catch(Exception ex) {
                result= false;
                _apiClientRepo.RollbackTransaction();
            }

            return result;
        }

        public bool CheckAuthorization(string clientId,string tenantDomain,string scope) {
            return _clientDomainScopeRepo.Get(clientId, tenantDomain, scope) != null;
        }

        public bool Authorize(ClientDomainScopesDto clientDomainScopesDto) {
            List<ClientDomainScopeModel> list = new List<ClientDomainScopeModel>();
            string clientId= clientDomainScopesDto.ClientId;
            foreach(var domainScopes in clientDomainScopesDto.DomainScopes) {
                string tenantDomain= domainScopes.TenantDomain;
                foreach(var scope in domainScopes.Scopes) {
                    list.Add(new ClientDomainScopeModel() { 
                        ClientId= clientId,
                        TenantDomain=tenantDomain,
                        Scope=scope
                    });
                }
            }

            return _clientDomainScopeRepo.BatchInsert(list);
        }
    }
}
