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
    public class CreateDbScriptBusiness 
    {
        private readonly CreateDbScriptRepository _createDbScriptRepo;
        private readonly SchemaUpdateScriptRepository _schemaUpdateScriptRepo;
        private readonly TenantServiceDbConnRepository _tenantServiceDbConnRepo;
        private readonly ILogger<CreateDbScriptBusiness> _logger;
        public CreateDbScriptBusiness(CreateDbScriptRepository createDbScriptRepo,
            SchemaUpdateScriptRepository schemaUpdateScriptRepo,
            TenantServiceDbConnRepository tenantServiceDbConnRepo,
            ILogger<CreateDbScriptBusiness> logger) {
            _createDbScriptRepo=createDbScriptRepo;
            _schemaUpdateScriptRepo=schemaUpdateScriptRepo;
            _tenantServiceDbConnRepo = tenantServiceDbConnRepo;
            _logger=logger;
        }

        public CreateDbScriptModel GetById(Int64 scriptId) {
            return _createDbScriptRepo.GetEntityById(scriptId);
        }

        public bool Insert(CreateDbScriptModel createDbScript) {
            bool result= false;
            try {
                result=_createDbScriptRepo.Insert(createDbScript);
                
            } catch(Exception ex) {
                result = false;
                _logger.LogError(ex,"insert create script error");
            }

            return result;
        }

        public bool Delete(Int64 scriptId) {
            bool result = false;

            var createScript = _createDbScriptRepo.GetEntityById(scriptId);
            if (createScript == null) {
                return false;
            }

            string createScriptName = createScript.Name;
            int majorVersion = createScript.MajorVersion;

            try {
                _createDbScriptRepo.BeginTransaction();

                var connList= _tenantServiceDbConnRepo.GetTenantServiceDbConns(createScriptName,majorVersion);
                if (connList.Any()) {
                    throw new Exception($"create db script {createScriptName} version {majorVersion} still has {connList.Count} dbconn");
                }

                var schemaUpdateScriptList = _schemaUpdateScriptRepo.GetSchemaUpdateScripts(createScriptName,majorVersion);
                if (schemaUpdateScriptList.Any()) {
                    throw new Exception($"create db script {createScriptName} version {majorVersion} still has {schemaUpdateScriptList.Count} update scripts");
                }

                result= _createDbScriptRepo.Delete(scriptId);
                if (!result) {
                    throw new Exception($"delete create db script {scriptId} error");
                }

                _createDbScriptRepo.CommitTransaction();
            } catch (Exception ex) {
                result = false;
                _logger.LogError(ex,"delete create db script error");
                _createDbScriptRepo.RollbackTransaction();
                
            }

            return result;
        }

        public List<CreateDbScriptModel> GetPageScriptsWithNoContent(int pageSize, int pageIndex) {
            return _createDbScriptRepo.GetPageScriptsWithNoContent(pageSize,pageIndex);
        }

        public async Task<List<CreateDbScriptModel>> GetListByNames(List<string> nameList) {
            return _createDbScriptRepo.GetListByNames(nameList);
        }

        public byte[] GetScriptContent(Int64 scriptId) {
            var createScript= _createDbScriptRepo.GetEntityById(scriptId);
            if (createScript == null || createScript.BinaryContent==null) {
                return null;
            }

            byte[] contentByteArr= createScript.BinaryContent as byte[];
            if (contentByteArr == null || contentByteArr.Length==0) {
                return null;
            }

            return contentByteArr;
        }
    }
}
