using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class CreateDbScriptRepository : BaseRepository<CreateDbScriptModel>
    {
        public override string TableName => "CreateDbScript";
        public CreateDbScriptRepository(TenantDbDataContext tenantDbDataContext) : base(tenantDbDataContext) {
        }

        public bool Insert(CreateDbScriptModel createDbScript) {
            string sql = @"Insert into CreateDbScript (Name,MajorVersion,ServiceIdentifier,DbIdentifier,DbNameWildcard,BinaryContent,DbType)
                            Values (@name,@majorVersion,@serviceIdentifier,@dbIdentifier,@dbNameWildcard,@binaryContent,@dbType)";

            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,createDbScript)>0;
        }

        public bool UpdateScriptContent(Int64 scriptId, byte[] binaryContent) {
            string sql = @"Update CreateDbScript Set BinaryContent=@binaryContent Where Id=@id";

            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, new { id=scriptId,binaryContent=binaryContent})>0;
        }
    }
}
