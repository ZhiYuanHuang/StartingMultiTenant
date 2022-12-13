using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Business
{
    public class SchemaUpdateScriptBusiness
    {
        public async Task<SchemaUpdateScriptModel> GetSchemaUpdateScriptByName(string updateScriptName) {
            throw new NotImplementedException();
        }

        public async Task<List<SchemaUpdateScriptModel>> GetSchemaUpdateScriptsByCreateScript(string createScriptName,int baseMajorVersion) {
            return new List<SchemaUpdateScriptModel>();
        }
    }
}
