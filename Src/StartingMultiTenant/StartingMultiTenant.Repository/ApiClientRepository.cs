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

        public bool Insert(string clientId,string encryptSecret) {
            string sql = "Insert Into ApiClient (ClientId,ClientSecret) Values (@clientId,@clientSecret)";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, new { clientId = clientId, clientSecret = encryptSecret }) > 0;
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
    }
}
