using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Business
{
    public class SchemaUpdateScriptBusiness:BaseBusiness<SchemaUpdateScriptModel>
    {
        private readonly SchemaUpdateScriptRepository _schemaUpdateScriptRepo;
        private readonly CreateDbScriptRepository _createDbScriptRepo;
        private readonly ILogger _logger;
        public SchemaUpdateScriptBusiness(CreateDbScriptRepository createDbScriptRepo,
            SchemaUpdateScriptRepository schemaUpdateScriptRepo,
            ILogger<SchemaUpdateScriptBusiness> logger):base(schemaUpdateScriptRepo,logger)
            { 
            _schemaUpdateScriptRepo= schemaUpdateScriptRepo;
            _createDbScriptRepo= createDbScriptRepo;
            _logger= logger;
        }

        public override SchemaUpdateScriptModel Get(long id) {
            return _schemaUpdateScriptRepo.GetNoContent(id);
        }

        public byte[] GetScriptContent(Int64 scriptId,bool getRollBack=false) {
            var updateScript = _schemaUpdateScriptRepo.GetEntityById(scriptId);

            object binaryContentObj = updateScript.BinaryContent;

            if(getRollBack) {
                binaryContentObj = updateScript.RollBackScriptBinaryContent;
            }
            if (binaryContentObj == null) {
                return null;
            }

            byte[] contentByteArr = binaryContentObj as byte[];
            if (contentByteArr == null || contentByteArr.Length == 0) {
                return null;
            }

            return contentByteArr;
        }

        public override List<SchemaUpdateScriptModel> Get(List<long> ids) {
            return _schemaUpdateScriptRepo.GetNoContent(ids);
        }

        public override bool Insert(SchemaUpdateScriptModel schemaUpdateScript,out Int64 id) {
            var createScript= _createDbScriptRepo.GetNoContent(schemaUpdateScript.CreateDbScriptId);
            id = 0;
            if (createScript == null) {
                _logger.LogError($"cann't found {schemaUpdateScript.CreateDbScriptId} createscript") ;
                return false;
            }

            
            return base.Insert(schemaUpdateScript,out id);
        }

        public PagingData<SchemaUpdateScriptModel> GetPageNoContent(string name,Int64? createDbScriptId, int pageSize, int pageIndex) {
            return _schemaUpdateScriptRepo.GetPageNoContent(pageSize, pageIndex, name, createDbScriptId);
        }

        public async Task<SchemaUpdateScriptModel> GetSchemaUpdateScriptByName(string updateScriptName) {
            return await Task.Factory.StartNew(() => _schemaUpdateScriptRepo.GetSchemaUpdateScripts(updateScriptName));
        }

        public List<SchemaUpdateScriptModel> GetSchemaUpdateScripts(Int64 createDbScriptId) {
            return _schemaUpdateScriptRepo.GetSchemaUpdateScripts(createDbScriptId);
        }
    }
}
