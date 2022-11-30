using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Business
{
    public interface ISchemaUpdateScriptBusiness {
        Task<List<SchemaUpdateScriptModel>> GetSchemaUpdateScripts(string createScriptName, int baseMajorVersion);
    }
    public class SchemaUpdateScriptBusiness: ISchemaUpdateScriptBusiness
    {
        public async Task<List<SchemaUpdateScriptModel>> GetSchemaUpdateScripts(string createScriptName,int baseMajorVersion) {
            return new List<SchemaUpdateScriptModel>();
        }
    }
}
