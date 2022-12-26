using StartingMultiTenant.Model.Domain;
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

        public bool Insert(DbServerModel dbServer) {
            string sql = @"Insert into DbServer (DbType,ServerHost,ServerPort,UserName,EncryptUserpwd,CanCreateNew)
                           Values (@dbType,@serverHost,@serverPort,@userName,@encryptUserpwd,true)";


            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,dbServer)>0;
        }

        public bool Delete(Int64 dbServerId) {
            string sql = "delete from dbserver where Id=@id";

            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,new { id=dbServerId})>0;
        }
    }
}
