using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Business
{
    public interface ITenantServiceDbConnBusiness {
        Task<List<TenantServiceDbConnModel>> GetTenantServiceDbConns(string tenantDomain, string tenantIdentifier, string createScriptName);
        Task<List<TenantServiceDbConnModel>> GetTenantServiceDbConns(Int64? dbConnId=null);
        Task<bool> ExchangeToAnotherDbServer(Int64 dbConnId,Int64 dbServerId, string encryptedConnStr);
        Task<List<TenantServiceDbConnModel>> GetTenantServiceDbConns(string createScriptName,int createScriptVersion);
    }
    public class TenantServiceDbConnBusiness : ITenantServiceDbConnBusiness
    {
        public async Task<List<TenantServiceDbConnModel>> GetTenantServiceDbConns(string tenantDomain, string tenantIdentifier, string createScriptName) {
            return new List<TenantServiceDbConnModel>();
        }

        public async Task<List<TenantServiceDbConnModel>> GetTenantServiceDbConns(long? dbConnId = null) {
            return new List<TenantServiceDbConnModel>();
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
