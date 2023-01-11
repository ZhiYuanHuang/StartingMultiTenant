using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
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

        public bool Insert(string clientId,string encryptSecret,out Int64 id,string role= RoleConst.Role_User) {
            string sql = @"Insert Into ApiClient (ClientId,ClientSecret,Role) 
                           Values (@clientId,@clientSecret,@role)
                           On CONFLICT (ClientId)
                           Do Update Set
                            ClientSecret=EXCLUDED.ClientSecret,Role=EXCLUDED.Role 
                           RETURNING Id";
            id=(long) _tenantDbDataContext.Master.ExecuteScalar(sql, new { clientId = clientId, clientSecret = encryptSecret,role=role });
            return true;
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

        public PagingData<ApiClientModel> GetPage(int pageSize, int pageIndex, string clientId = null) {
            StringBuilder countBuilder = new StringBuilder("Select Count(*) From ApiClient ");
            StringBuilder dataBuilder = new StringBuilder("Select * From ApiClient ");

            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "pageSize",pageSize},
                { "offSet",pageSize*pageIndex}
            };
            if (!string.IsNullOrEmpty(clientId)) {
                countBuilder.Append(" Where ClientId=@clientId ");
                dataBuilder.Append(" Where ClientId=@clientId ");
                p["clientId"] = clientId;
            }

            dataBuilder.Append(" Limit @pageSize OFFSET @offSet");
            int count = (int)((long)_tenantDbDataContext.Slave.ExecuteScalar(countBuilder.ToString(), p));

            if (count == 0) {
                return new PagingData<ApiClientModel>(pageIndex, pageSize, 0, new List<ApiClientModel>());
            }

            var list = _tenantDbDataContext.Slave.QueryList<ApiClientModel>(dataBuilder.ToString(), p);
            return new PagingData<ApiClientModel>(pageIndex, pageSize, count, list);

        }
    }
}
