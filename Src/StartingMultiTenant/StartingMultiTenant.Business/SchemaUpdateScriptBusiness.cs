using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Business
{
    public class SchemaUpdateScriptBusiness
    {
        private readonly SchemaUpdateScriptRepository _schemaUpdateScriptRepo;
        private readonly CreateDbScriptRepository _createDbScriptRepo;
        private readonly ILogger _logger;
        public SchemaUpdateScriptBusiness(CreateDbScriptRepository createDbScriptRepo,
            SchemaUpdateScriptRepository schemaUpdateScriptRepo,
            ILogger<SchemaUpdateScriptBusiness> logger) { 
            _schemaUpdateScriptRepo= schemaUpdateScriptRepo;
            _createDbScriptRepo= createDbScriptRepo;
            _logger= logger;
        }

        public bool Insert(SchemaUpdateScriptModel schemaUpdateScript) {
            var createScript= _createDbScriptRepo.GetByNameByVersion(schemaUpdateScript.CreateScriptName,schemaUpdateScript.BaseMajorVersion);
            if (createScript == null) {
                _logger.LogError($"cann't found {schemaUpdateScript.CreateScriptName} {schemaUpdateScript.BaseMajorVersion} createscript") ;
                return false;
            }

            var existedUpdateScripts= _schemaUpdateScriptRepo.GetSchemaUpdateScripts(createScript.Name,createScript.MajorVersion);

            if (existedUpdateScripts.Any()) {
                int maxMinorVersion= existedUpdateScripts.Max(x => x.MinorVersion);
                if (maxMinorVersion >= schemaUpdateScript.MinorVersion) {
                    _logger.LogError($"createscript {schemaUpdateScript.CreateScriptName} {schemaUpdateScript.BaseMajorVersion} had {maxMinorVersion} version updateschema script");
                    return false;
                }
            }

            return _schemaUpdateScriptRepo.Insert(schemaUpdateScript);
        }

        public bool Delete(Int64 scriptId) {
            return _schemaUpdateScriptRepo.Delete(scriptId);
        }

        public async Task<SchemaUpdateScriptModel> GetSchemaUpdateScriptByName(string updateScriptName) {
            return await Task.Factory.StartNew(() => _schemaUpdateScriptRepo.GetSchemaUpdateScripts(updateScriptName));
        }

        public async Task<List<SchemaUpdateScriptModel>> GetSchemaUpdateScripts(string createScriptName,int baseMajorVersion) {
            return _schemaUpdateScriptRepo.GetSchemaUpdateScripts(createScriptName,baseMajorVersion);
        }

        public SchemaUpdateScriptModel GetById(Int64 scriptId) {
            return _schemaUpdateScriptRepo.GetEntityById(scriptId);
        }
    }
}
