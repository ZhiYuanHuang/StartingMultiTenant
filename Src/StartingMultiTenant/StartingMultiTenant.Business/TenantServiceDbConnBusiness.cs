using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Business
{
    public class TenantServiceDbConnBusiness 
    {
        private readonly TenantServiceDbConnRepository _tenantServiceDbConnRepository;
        public TenantServiceDbConnBusiness(TenantServiceDbConnRepository tenantServiceDbConnRepository) {
            _tenantServiceDbConnRepository = tenantServiceDbConnRepository;
        }
        public async Task<List<TenantServiceDbConnModel>> GetTenantServiceDbConns(string tenantDomain, string tenantIdentifier, string createScriptName) {
            return new List<TenantServiceDbConnModel>();
        }

        public async Task<List<TenantServiceDbConnModel>> GetTenantServiceDbConns(long? dbConnId = null) {
            return new List<TenantServiceDbConnModel>();
        }

        public bool BatchInsertDbConns(List<TenantServiceDbConnModel> dbConnList) {
            return _tenantServiceDbConnRepository.BatchInsertDbConns(dbConnList);
        }

        public async Task<bool> ExchangeToAnotherDbServer(Int64 dbConnId, long dbServerId, string encryptedConnStr) {
            //save to HistoryDbConn
            return true;
        }

        public Task<List<TenantServiceDbConnModel>> GetTenantServiceDbConns(string createScriptName, int createScriptVersion) {
            throw new NotImplementedException();
        }
    }
}
