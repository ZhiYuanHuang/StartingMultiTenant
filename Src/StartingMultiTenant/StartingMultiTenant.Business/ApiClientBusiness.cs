using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace StartingMultiTenant.Business
{
    public class ApiClientBusiness:BaseBusiness<ApiClientModel>
    {
        private readonly ApiClientRepository _apiClientRepo;
        private readonly ClientScopesRepository _clientScopesRepo;
        private readonly ApiScopeRepository _apiScopeRepo;
        public ApiClientBusiness(ApiClientRepository apiClientRepo,
            ClientScopesRepository clientScopesRepo,
            ApiScopeRepository apiScopeRepo,
            ILogger<ApiClientBusiness> logger):base(apiClientRepo,logger) {
            _apiClientRepo= apiClientRepo;
            _clientScopesRepo= clientScopesRepo;
            _apiScopeRepo= apiScopeRepo;
        }

        public PagingData<ApiClientModel> GetPage(string clientId,int pageSize,int pageIndex) {
            return _apiClientRepo.GetPage(pageSize,pageIndex,clientId);
        }

        public ApiClientModel Get(string clientId) {
            return _apiClientRepo.Get(clientId);
        }

        public ApiClientDto GetWithScopes(Int64 id) {
            var model = _apiClientRepo.GetEntityById(id);
            if (model == null) {
                return null;
            }

            return GetWithScopes(model.ClientId);
        }

        public ApiClientDto GetWithScopes(string clientId) {
            ApiClientModel apiClient= _apiClientRepo.Get(clientId);
            if(apiClient == null) {
                return null;
            }

            var dto= new ApiClientDto() { 
                ClientId= apiClient.ClientId,
                ClientSecret=apiClient.ClientSecret,
                Role=apiClient.Role
            };

            var clientScopes= _clientScopesRepo.Get(clientId);
            if(clientScopes == null) {
                return dto;
            }

            dto.Scopes=clientScopes.Select(x=>x.Scope).Distinct().ToList();
            return dto;
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

                _clientScopesRepo.DeleteByClient(clientId);

                _apiClientRepo.CommitTransaction();
                result = true;
            }
            catch(Exception ex) {
                result= false;
                _apiClientRepo.RollbackTransaction();
            }

            return result;
        }

        public bool Authorize(ApiClientDto clientScopesDto) {
            List<ClientScopesModel> list = new List<ClientScopesModel>();
            string clientId= clientScopesDto.ClientId;

            List<ApiScopeModel> allScopeList= _apiScopeRepo.GetEntitiesByQuery();
            foreach(var scope in clientScopesDto.Scopes) {
                if (allScopeList.FirstOrDefault(x => x.Name == scope) == null) {
                    _logger.LogError($"scope {scope} not exists!");
                    return false;
                }
                list.Add(new ClientScopesModel() {
                    ClientId = clientId,
                    Scope = scope
                });
            }

            return _clientScopesRepo.BatchInsert(list);
        }
    }
}
