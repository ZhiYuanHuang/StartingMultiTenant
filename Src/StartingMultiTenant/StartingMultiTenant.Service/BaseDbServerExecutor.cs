using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Service
{
    public abstract class BaseDbServerExecutor : IDbServerExecutor
    {
        protected readonly DbServerModel _dbServer;
        protected readonly string _decryptUserPwd = string.Empty;
        protected readonly ILogger<IDbServerExecutor> _logger;
        protected readonly SysConstService _sysConstService;
        protected readonly EncryptService _encryptService;
        public BaseDbServerExecutor(DbServerModel dbServer,
            ILogger<IDbServerExecutor> logger,
            SysConstService sysConstService,
            EncryptService encryptService) {
            _dbServer = dbServer;
            _logger = logger;
            _sysConstService = sysConstService;
            _encryptService = encryptService;
        }

        public virtual bool CreateDb(CreateDbScriptModel createDbScriptModel, string tenantGuid, out string uniqueDbName) {
            uniqueDbName = string.Empty;
            if (!System.IO.File.Exists(createDbScriptModel.FilePath)) {
                _logger.LogError("createDb script file not exists");
                return false;
            }

            string createDbScript = string.Empty;
            try {

                uniqueDbName = SqlScriptHelper.GenerateRandomDbName(createDbScriptModel.DbIdentifier, tenantGuid);

                string dbNameWildcard = !string.IsNullOrEmpty(createDbScriptModel.DbNameWildcard) ? createDbScriptModel.DbNameWildcard : _sysConstService.DbNameWildcard;
                var generateResult = SqlScriptHelper.GenerateCreateDbScript(createDbScriptModel.FilePath, dbNameWildcard, uniqueDbName).GetAwaiter().GetResult();
                if (generateResult.Item1) {
                    createDbScript = generateResult.Item2;
                }
            } catch (Exception ex) {
                _logger.LogError($"generate create db script raise error,ex:{ex.Message}");
            }

            if (string.IsNullOrEmpty(createDbScript)) {
                uniqueDbName = string.Empty;
                return false;
            }

            string dbConnStr = generateDbConnStr(_dbServer);

            return executeScript(dbConnStr, createDbScript).GetAwaiter().GetResult();
        }

        public virtual async Task DeleteDb(string dbName) {
            string script= SqlScriptHelper.GenerateDeleteDbScript(dbName);
            string dbConnStr = generateDbConnStr(_dbServer);
            await executeScript(dbConnStr, script);
        }

        public virtual bool UpdateSchema(string encryptedConnStr,SchemaUpdateScriptModel schemaUpdateScript) {
            if(!System.IO.File.Exists(schemaUpdateScript.FilePath) || !System.IO.File.Exists(schemaUpdateScript.RollBackScriptPath)) {
                _logger.LogError("updateSchema or rollback script file not exists");
                return false;
            }

            
            string updateSchemaScript = string.Empty;
            string rollbackScript = string.Empty;
            string connStr = string.Empty ;
            try {
                connStr = decrypt_conn(encryptedConnStr);
                string dbName = resolveDatabaseName(connStr);
                connStr = generateDbConnStr(_dbServer, dbName);

                string dbNameWildcard = !string.IsNullOrEmpty(schemaUpdateScript.DbNameWildcard) ? schemaUpdateScript.DbNameWildcard : _sysConstService.DbNameWildcard;
                var generateResult = SqlScriptHelper.GenerateUpdateSchemaScript(schemaUpdateScript.FilePath, dbNameWildcard, dbName).GetAwaiter().GetResult();
                if (generateResult.Item1) {
                    updateSchemaScript = generateResult.Item2;
                }

                generateResult = SqlScriptHelper.GenerateRollbackSchemaScript(schemaUpdateScript.RollBackScriptPath, dbNameWildcard, dbName).GetAwaiter().GetResult();
                if (generateResult.Item1) {
                    updateSchemaScript = generateResult.Item2;
                }
            } catch (Exception ex) {
                _logger.LogError($"generate create db script raise error,ex:{ex.Message}");
            }

            if(string.IsNullOrEmpty(connStr) || string.IsNullOrEmpty(updateSchemaScript) || string.IsNullOrEmpty(rollbackScript)) {
                return false;
            }

            return executeScript(connStr, updateSchemaScript,rollbackScript).GetAwaiter().GetResult();
        }

        protected abstract string generateDbConnStr(DbServerModel dbServer, string database = null);
        protected abstract string resolveDatabaseName(string dbConnStr);
        protected abstract Task<bool> executeScript(string dbConnStr, string dbScriptStr);
        protected abstract Task<bool> executeScript(string dbConnStr, string updateSchemaScript,string rollbackScript);

        protected string decrypt_conn(string encryptedConnStr) {
            return _encryptService.Decrypt_DbConn(encryptedConnStr);
        }
    }
}
