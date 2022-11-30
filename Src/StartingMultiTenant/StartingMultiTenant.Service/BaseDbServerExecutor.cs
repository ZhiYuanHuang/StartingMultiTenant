using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StartingMultiTenant.Service
{
    public abstract class BaseDbServerExecutor : IDbServerExecutor
    {
        protected readonly DbServerModel _dbServer;
        protected readonly string _decryptUserPwd = string.Empty;
        protected readonly ILogger<IDbServerExecutor> _logger;
        protected readonly SysConstService _sysConstService;
        protected readonly EncryptService _encryptService;

        public DbServerModel DbServer { get => _dbServer; }

        public BaseDbServerExecutor(DbServerModel dbServer,
            ILogger<IDbServerExecutor> logger,
            SysConstService sysConstService,
            EncryptService encryptService) {
            _dbServer = dbServer;
            _logger = logger;
            _sysConstService = sysConstService;
            _encryptService = encryptService;
        }

        public virtual bool CreateDb(CreateDbScriptModel createDbScriptModel, string tenantIdentifier, out string uniqueDbName) {
            uniqueDbName = string.Empty;
            if (!System.IO.File.Exists(createDbScriptModel.FilePath)) {
                _logger.LogError("createDb script file not exists");
                return false;
            }

            string createDbScript = string.Empty;
            try {

                uniqueDbName = SqlScriptHelper.GenerateRandomDbName(createDbScriptModel.DbIdentifier, tenantIdentifier);

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

            string dbConnStr = generateDbConnStr();

            return executeScript(dbConnStr, createDbScript).GetAwaiter().GetResult();
        }

        public virtual async Task DeleteDb(string dbName) {
            string script= SqlScriptHelper.GenerateDeleteDbScript(dbName);
            string dbConnStr = generateDbConnStr();
            await executeScript(dbConnStr, script);
        }

        public virtual bool UpdateSchemaByConnStr(string encryptedConnStr,SchemaUpdateScriptModel schemaUpdateScript) {

            string connStr = decrypt_conn(encryptedConnStr);
            string dbName = resolveDatabaseName(connStr);

            return UpdateSchemaByDatabase(dbName,schemaUpdateScript);
        }

        public virtual bool UpdateSchemaByDatabase(string dataBaseName, SchemaUpdateScriptModel schemaUpdateScript) {
            if (!System.IO.File.Exists(schemaUpdateScript.FilePath) || !System.IO.File.Exists(schemaUpdateScript.RollBackScriptPath)) {
                _logger.LogError("updateSchema or rollback script file not exists");
                return false;
            }

            string updateSchemaScript = string.Empty;
            string rollbackScript = string.Empty;
            string connStr = string.Empty;
            try {
                connStr = generateDbConnStr( dataBaseName);

                string dbNameWildcard = !string.IsNullOrEmpty(schemaUpdateScript.DbNameWildcard) ? schemaUpdateScript.DbNameWildcard : _sysConstService.DbNameWildcard;
                var generateResult = SqlScriptHelper.GenerateUpdateSchemaScript(schemaUpdateScript.FilePath, dbNameWildcard, dataBaseName).GetAwaiter().GetResult();
                if (generateResult.Item1) {
                    updateSchemaScript = generateResult.Item2;
                }

                generateResult = SqlScriptHelper.GenerateRollbackSchemaScript(schemaUpdateScript.RollBackScriptPath, dbNameWildcard, dataBaseName).GetAwaiter().GetResult();
                if (generateResult.Item1) {
                    updateSchemaScript = generateResult.Item2;
                }
            } catch (Exception ex) {
                _logger.LogError($"generate create db script raise error,ex:{ex.Message}");
            }

            if (string.IsNullOrEmpty(connStr) || string.IsNullOrEmpty(updateSchemaScript) || string.IsNullOrEmpty(rollbackScript)) {
                return false;
            }

            return executeScript(connStr, updateSchemaScript, rollbackScript).GetAwaiter().GetResult();
        }

        public string GenerateEncryptDbConnStr(string dbName) {
            return decrypt_conn(generateDbConnStr(dbName));
        }

        protected abstract string generateDbConnStr( string database = null);
        protected abstract string resolveDatabaseName(string dbConnStr);
        protected abstract Task<bool> executeScript(string dbConnStr, string dbScriptStr);
        protected abstract Task<bool> executeScript(string dbConnStr, string updateSchemaScript,string rollbackScript);

        protected string decrypt_conn(string encryptedConnStr) {
            return _encryptService.Decrypt_DbConn(encryptedConnStr);
        }

        protected string encrypt_conn(string connStr) {
            return _encryptService.Encrypt_DbConn(connStr);
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            IDbServerExecutor otherDbServerExecutor= obj as IDbServerExecutor;
            if (otherDbServerExecutor == null) {
                return false;
            }
            DbServerModel otherDbserver = otherDbServerExecutor.DbServer;
            if(DbServer==null || otherDbserver == null) {
                return false;
            }
            if(DbServer.DbType!= otherDbserver.DbType || DbServer.ServerHost!= otherDbserver.ServerHost || DbServer.ServerPort!=otherDbserver.ServerPort || DbServer.UserName != otherDbserver.UserName) {
                return false;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode() {
            if (DbServer == null) {
                return base.GetHashCode();
            }
            return String.Format("{0}|{1}|{2}|{3}", DbServer.DbType, DbServer.ServerHost, DbServer.ServerPort,DbServer.UserName).GetHashCode();
        }
    }
}
