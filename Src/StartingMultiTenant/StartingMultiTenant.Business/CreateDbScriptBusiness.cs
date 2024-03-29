﻿using Microsoft.Extensions.Logging;
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
    public class CreateDbScriptBusiness :BaseBusiness<CreateDbScriptModel>
    {
        private readonly CreateDbScriptRepository _createDbScriptRepo;
        private readonly SchemaUpdateScriptRepository _schemaUpdateScriptRepo;
        private readonly TenantServiceDbConnRepository _tenantServiceDbConnRepo;
        private readonly ILogger<CreateDbScriptBusiness> _logger;
        public CreateDbScriptBusiness(CreateDbScriptRepository createDbScriptRepo,
            SchemaUpdateScriptRepository schemaUpdateScriptRepo,
            TenantServiceDbConnRepository tenantServiceDbConnRepo,
            ILogger<CreateDbScriptBusiness> logger):base(createDbScriptRepo,logger) {
            _createDbScriptRepo=createDbScriptRepo;
            _schemaUpdateScriptRepo=schemaUpdateScriptRepo;
            _tenantServiceDbConnRepo = tenantServiceDbConnRepo;
            _logger=logger;
        }

        public CreateDbScriptModel GetNoContent(long id) {
            return _createDbScriptRepo.GetNoContent(id);
        }

        public List<CreateDbScriptModel> GetNoContent(List<long> ids) {
            return _createDbScriptRepo.GetNoContent(ids);

        }

        public List<CreateDbScriptDto> GetTenantCreateScripts(Int64 tenantId) {
            var dict= _createDbScriptRepo.GetTenantCreateScripts(new List<long>() { tenantId});
            if (dict.ContainsKey(tenantId)) {
                return dict[tenantId];
            }
            return new List<CreateDbScriptDto>();
        }

        public Dictionary<Int64, List<CreateDbScriptDto>> GetTenantCreateScripts(List<Int64> tenantIds) {
            return _createDbScriptRepo.GetTenantCreateScripts(tenantIds);
        }
        public override Tuple<bool,string> Delete(Int64 scriptId) {
            bool result = false;

            var createScript = _createDbScriptRepo.GetEntityById(scriptId);
            if (createScript == null) {
                return Tuple.Create(false,"not found");
            }

            string createScriptName = createScript.Name;
            int majorVersion = createScript.MajorVersion;

            string errMsg = string.Empty;
            try {
                _createDbScriptRepo.BeginTransaction();

                var connList= _tenantServiceDbConnRepo.GetTenantServiceDbConns(createScriptName,majorVersion);
                if (connList.Any()) {
                    throw new Exception($"create db script {createScriptName} version {majorVersion} still has {connList.Count} dbconn");
                }

                var schemaUpdateScriptList = _schemaUpdateScriptRepo.GetSchemaUpdateScripts(createScript.Id);
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
                errMsg = "delete create db script error";
                _logger.LogError(ex, errMsg);
                _createDbScriptRepo.RollbackTransaction();
                
            }

            return Tuple.Create(result,errMsg);
        }

        public PagingData<CreateDbScriptModel> GetPageNoContent(string name,int pageSize, int pageIndex) {
            return _createDbScriptRepo.GetPageNoContent(pageSize,pageIndex,name);
        }

        public List<CreateDbScriptModel> GetAllNoContent() {
            return _createDbScriptRepo.GetAllNoContent();
        }

        public List<VersionScriptDto> GetVersionScripts() {
            return _createDbScriptRepo.GetVersionScripts();
        }

        public List<CreateDbScriptModel> GetListByNames(List<string> nameList,bool onlyNewest=false) {
            return _createDbScriptRepo.GetListByNames(nameList,onlyNewest);
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
