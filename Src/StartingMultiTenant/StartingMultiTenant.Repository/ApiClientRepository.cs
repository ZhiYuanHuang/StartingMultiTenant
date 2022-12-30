using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class ApiClientRepository : BaseRepository<ApiClientModel>
    {
        public override string TableName => "ApiClient";
        public ApiClientRepository(TenantDbDataContext tenantDbDataContext) : base(tenantDbDataContext) {
        }

        public bool Insert(string clientId,string encryptSecret,string role= RoleConst.Role_User) {
            string sql = @"Insert Into ApiClient (ClientId,ClientSecret,Role) 
                           Values (@clientId,@clientSecret,@role)
                           On CONFLICT (ClientId)
                           Do Update Set
                            ClientSecret=EXCLUDED.ClientSecret,Role=EXCLUDED.Role ";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, new { clientId = clientId, clientSecret = encryptSecret,role=role }) > 0;
        }

        public bool Delete(string clientId) {
            string sql = "Delete From ApiClient Where ClientId=@clientId";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, new { clientId})>0;
        }

        public ApiClientModel Get(string clientId) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                {"ClientId",clientId },
            };

            return GetEntityByQuery(p);
        }

        public List<ApiClientModel> GetAdmins() {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                {"Role",RoleConst.Role_Admin },
            };

            return GetEntitiesByQuery(p);
        }
    }
}
