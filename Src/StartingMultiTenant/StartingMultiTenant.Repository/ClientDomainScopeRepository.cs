using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class ClientDomainScopeRepository : BaseRepository<ClientDomainScopeModel>
    {
      
        public override string TableName => "ClientDomainScope";
        public ClientDomainScopeRepository(TenantDbDataContext tenantDbDataContext) : base(tenantDbDataContext) {
        }

        public bool BatchInsert(List<ClientDomainScopeModel> clientDomainScopeList) {
            string sql = @"Insert Into ClientDomainScope (ClientId,TenantDomain,Scope)
                           Values (@clientId,@tenantDomain,@scope)
                           ON CONFLICT ON CONSTRAINT u_clientdomainscope_1
                           DO Update Set
                            UpdateTime=now()";

            bool success = false;

            try {
                BeginTransaction();
                foreach(var clientDomainScope in clientDomainScopeList) {
                    bool insertResult = _tenantDbDataContext.Master.ExecuteNonQuery(sql,clientDomainScope)>0;
                    if (!insertResult) {
                        success = false;
                        throw new Exception($"insert error,data:{Newtonsoft.Json.JsonConvert.SerializeObject(clientDomainScope)}");
                    }
                }

                CommitTransaction();
                success = true;
            }catch(Exception ex) {
                success = false;
                RollbackTransaction();
            }

            return success;
        }

        public void DeleteByClient(string clientId) {
            string sql = "Delete From ClientDomainScope Where ClientId=@clientId";
            _tenantDbDataContext.Master.ExecuteNonQuery(sql, new { clientId=clientId});
        }

        public int GetCountByScope(string scopeName) {
            string sql = "Select Count(Id) From ClientDomainScope Where Scope=@scopeName ";

            object obj= _tenantDbDataContext.Master.ExecuteScalar(sql, new { scopeName=scopeName});
            if (obj == null) {
                return 0;
            }
            int.TryParse(obj.ToString(),out int result);
            return result;
        }

        public ClientDomainScopeModel Get(string clientId,string tenantDomain,string scope) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "ClientId",clientId},
                { "TenantDomain",tenantDomain},
                { "Scope",scope}
            };

            return GetEntityByQuery(p);

        }
    }
}
