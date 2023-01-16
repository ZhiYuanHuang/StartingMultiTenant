using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class ClientScopesRepository : BaseRepository<ClientScopesModel>
    {
      
        public override string TableName => "ClientScopes";
        public ClientScopesRepository(TenantDbDataContext tenantDbDataContext) : base(tenantDbDataContext) {
        }

        public bool BatchInsert(List<ClientScopesModel> clientScopesList) {

            string sql = @"Insert Into ClientScopes (ClientId,Scope)
                           Values (@clientId,@scope)
                           ON CONFLICT ON CONSTRAINT u_clientscopes_1
                           DO Update Set
                            UpdateTime=now()";

            bool success = false;

            try {
                BeginTransaction();

                List<string> clientIds= clientScopesList.Select(x => x.ClientId).Distinct().ToList();
                _tenantDbDataContext.Master.ExecuteNonQuery("Delete From ClientScopes Where ClientId =ANY(@clientIds)", new { clientIds=clientIds.ToArray()});
                
                foreach(var clientDomainScope in clientScopesList) {
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
            string sql = "Delete From ClientScopes Where ClientId=@clientId";
            _tenantDbDataContext.Master.ExecuteNonQuery(sql, new { clientId=clientId});
        }

        public int GetCountByScope(string scopeName) {
            string sql = "Select Count(Id) From ClientScopes Where Scope=@scopeName ";

            object obj= _tenantDbDataContext.Master.ExecuteScalar(sql, new { scopeName=scopeName});
            if (obj == null) {
                return 0;
            }
            int.TryParse(obj.ToString(),out int result);
            return result;
        }

        public List<ClientScopesModel> Get(string clientId) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "ClientId",clientId}
            };

            return GetEntitiesByQuery(p);
        }
    }
}
