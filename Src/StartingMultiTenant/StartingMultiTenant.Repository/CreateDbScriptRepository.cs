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

        public List<CreateDbScriptModel> GetPageScriptsWithNoContent(int pageSize,int pageIndex) {
            string sql = $"Select Id,Name,MajorVersion,ServiceIdentifier,DbIdentifier,DbNameWildcard,DbType From CreateDbScript Limit {pageSize} OFFSET {pageSize*pageIndex}";
            return _tenantDbDataContext.Slave.QueryList<CreateDbScriptModel>(sql);
        }

        public List<CreateDbScriptModel> GetListByNames(List<string> nameList) {
            string sql = @"Select * From CreateDbScript Where Name = ANY( @names)";
            //return _tenantDbDataContext.Slave.QueryList<CreateDbScriptModel>(sql, new { 
            //    names=nameList
            //});
            return _tenantDbDataContext.Slave.QueryList<CreateDbScriptModel>(sql, new Dictionary<string, object>() {
                { "names",nameList.ToArray()}
            });
        }

        public CreateDbScriptModel GetByNameByVersion(string createScriptName,int majorVersion) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "Name",createScriptName},
                { "MajorVersion",majorVersion}
            };

            return GetEntityByQuery(p);
        }

        public bool Insert(CreateDbScriptModel createDbScript) {
            string sql = @"Insert into CreateDbScript (Name,MajorVersion,ServiceIdentifier,DbIdentifier,DbNameWildcard,BinaryContent,DbType)
                            Values (@name,@majorVersion,@serviceIdentifier,@dbIdentifier,@dbNameWildcard,@binaryContent,@dbType)";
           
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,createDbScript)>0;
        }

        public bool Delete(Int64 scriptId) {
            string sql = "Delete From CreateDbScript Where Id=@id";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, new { id = scriptId }) > 0;
        }

         
    }
}
