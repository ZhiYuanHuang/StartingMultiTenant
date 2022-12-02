using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Business
{
    public interface ITenantServiceDbConnBusiness {
        Task<List<TenantServiceDbConnModel>> GetTenantServiceDbConns(string tenantDomain, string tenantIdentifier, string createScriptName);
    }
    public class TenantServiceDbConnBusiness : ITenantServiceDbConnBusiness
    {
        public async Task<List<TenantServiceDbConnModel>> GetTenantServiceDbConns(string tenantDomain, string tenantIdentifier, string createScriptName) {
            return new List<TenantServiceDbConnModel>();
        }
    }
}
