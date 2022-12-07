using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class TenantServiceDbConnRepository : BaseRepository<TenantServiceDbConnModel>
    {
        public override string TableName => "TenantServiceDbConn";

        public TenantServiceDbConnRepository(TenantDbDataContext tenantDbDataContext)
            :base(tenantDbDataContext) { 
        }

        public List<TenantServiceDbConnModel> GetConnListByDbServer(Int64 dbServerId) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "DbServerId",dbServerId}
            };

            return GetEntitiesByQuery(p);
        }
    }
}
