using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class DbServerRepository : BaseRepository<DbServerModel>
    {
        private readonly string _tableName;
        public override string TableName { get => _tableName; }
        public DbServerRepository(TenantDbDataContext tenantDbDataContext) :base(tenantDbDataContext) {
            _tableName = "DbServer";
        }

        public List<DbServerModel> GetDbServers(Int64? dbServerId = null) {
            Dictionary<string, object> dict = null;
            if (dbServerId.HasValue) {
                dict=new Dictionary<string, object>();
                dict.Add("Id",dbServerId.Value);
            }

            return GetEntitiesByQuery(dict);
        }

        public override bool Insert(DbServerModel dbServer,out Int64 id) {
            string sql = @"Insert into DbServer (DbType,ServerHost,ServerPort,UserName,EncryptUserpwd,CanCreateNew)
                           Values (@dbType,@serverHost,@serverPort,@userName,@encryptUserpwd,true) RETURNING Id";

            id = (Int64)_tenantDbDataContext.Master.ExecuteScalar(sql, dbServer);
            return true;
        }

        public PagingData<DbServerModel> GetPage(int pageSize, int pageIndex, string serverHost,int? dbType) {
            Dictionary<string, object> p = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(serverHost)) {
                p["serverHost"] = serverHost;
            }
            if (dbType.HasValue) {
                p["dbType"] = dbType.Value;
            }

            return GetPage(pageSize, pageIndex, p);
        }
    }
}
