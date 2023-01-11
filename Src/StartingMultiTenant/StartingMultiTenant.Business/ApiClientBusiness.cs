using Microsoft.Extensions.Logging;
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
    public class ApiClientBusiness:BaseBusiness<ApiClientModel>
    {
        private readonly ApiClientRepository _apiClientRepo;
        private readonly ClientDomainScopeRepository _clientDomainScopeRepo;
        public ApiClientBusiness(ApiClientRepository apiClientRepo,
            ClientDomainScopeRepository clientDomainScopeRepo,
            ILogger<ApiClientBusiness> logger):base(apiClientRepo,logger) {
            _apiClientRepo= apiClientRepo;
            _clientDomainScopeRepo= clientDomainScopeRepo;
        }

        public PagingData<ApiClientModel> GetPage(string clientId,int pageSize,int pageIndex) {
            return _apiClientRepo.GetPage(pageSize,pageIndex,clientId);
        }

        public ApiClientModel Get(string clientId) {
            return _apiClientRepo.Get(clientId);
        }

        public List<ApiClientModel> GetAdmins() {
            return _apiClientRepo.GetAdmins();
        }

        public bool Insert(string clientId,string encryptSecret,out Int64 id,string role=RoleConst.Role_User) {
            id = 0;
            try {
                return _apiClientRepo.Insert(clientId, encryptSecret,out id, role);
            }
            catch{
                return false;
            }
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
