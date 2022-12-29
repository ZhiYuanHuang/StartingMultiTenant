using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class ApiScopeRepository : BaseRepository<ApiScopeModel>
    {
        public override string TableName => "ApiScope";
        public ApiScopeRepository(TenantDbDataContext tenantDbDataContext) : base(tenantDbDataContext) {
        }

        public bool InsertOrUpdate(ApiScopeModel apiScope) {
            string sql = @"Insert Into ApiScope (Name,DisplayName)
                           Values (@name,@displayName)
                           On CONFLICT (Name)
                           DO Update Set
                             DisplayName=EXCLUDED.DisplayName";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,apiScope)>0;
        }

        public bool Delete(string name) {
            string sql = "Delete From ApiScope Where Name=@name";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,new { name=name})>0;
        }

        public ApiScopeModel Get(string name) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "Name",name}
            };

            return GetEntityByQuery(p);
        }
    }
}
