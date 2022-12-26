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

            string originCreateDbScript = string.Empty;
            byte[]? tmpByteArr = null;
            if (createDbScriptModel.BinaryContent == null || (tmpByteArr =createDbScriptModel.BinaryContent as byte[])==null) {
                _logger.LogError($"create db script {createDbScriptModel.Name} content is empty");
                return false;
            }

            originCreateDbScript = Encoding.UTF8.GetString(tmpByteArr);

            string createDbTableScript = string.Empty;

            string dbConnStr = generateDbConnStr(null,false);
            bool createDbResult = false;
            try {
                uniqueDbName = SqlScriptHelper.GenerateRandomDbName(createDbScriptModel.DbIdentifier, tenantIdentifier);
                string createDbScriptStr = generateCreateDbStr(uniqueDbName);
                createDbResult= executeScript(dbConnStr, createDbScriptStr).GetAwaiter().GetResult(); 
            } catch (Exception ex){
                createDbResult = false;
                _logger.LogError($"create db script raise error,ex:{ex.Message}");
            }

            if (!createDbResult) {
                return false;
            }

            createDbTableScript = originCreateDbScript;
            dbConnStr = generateDbConnStr(uniqueDbName,false);
            return executeScript(dbConnStr, createDbTableScript).GetAwaiter().GetResult();
        }

        public virtual async Task DeleteDb(string dbName) {
            string script= SqlScriptHelper.GenerateDeleteDbScript(dbName);
            string dbConnStr = generateDbConnStr(null,false);
            await executeScript(dbConnStr, script);
        }

        public bool UpdateSchemaByConnStr(string encryptedConnStr,SchemaUpdateScriptModel schemaUpdateScript) {

            string connStr = decrypt_conn(encryptedConnStr);
            string dbName = resolveDatabaseName(connStr);

            return UpdateSchemaByDatabase(dbName,schemaUpdateScript);
        }

        public virtual bool UpdateSchemaByDatabase(string dataBaseName, SchemaUpdateScriptModel schemaUpdateScript) {
 
            string originUpdateSchemaScript = string.Empty;
            string originRollbackScript = string.Empty;
            byte[]? tmpByteArr = null;
            if(schemaUpdateScript.BinaryContent==null || (tmpByteArr=schemaUpdateScript.BinaryContent as byte[]) == null) {
                _logger.LogError($"updateSchema script {schemaUpdateScript.Name} content is empty");
                return false;
            }

            originUpdateSchemaScript = Encoding.UTF8.GetString(tmpByteArr);

            if(schemaUpdateScript.RollBackScriptBinaryContent==null || (tmpByteArr=schemaUpdateScript.RollBackScriptBinaryContent as byte[]) == null) {
                _logger.LogError($"updateSchema script {schemaUpdateScript.Name} rollback script content not exists");
                return false;
            }

            originRollbackScript=Encoding.UTF8.GetString(tmpByteArr);

            string updateSchemaScript = string.Empty;
            string rollbackScript = string.Empty;
            string connStr = string.Empty;
            try {
                connStr = generateDbConnStr( dataBaseName);

                updateSchemaScript = originUpdateSchemaScript;
                rollbackScript = originRollbackScript;

                //string dbNameWildcard = !string.IsNullOrEmpty(schemaUpdateScript.DbNameWildcard) ? schemaUpdateScript.DbNameWildcard : _sysConstService.DbNameWildcard;
                //var generateResult = SqlScriptHelper.GenerateUpdateSchemaScript(originUpdateSchemaScript, dbNameWildcard, dataBaseName);
                //if (generateResult.Item1) {
                //    updateSchemaScript = generateResult.Item2;
                //}

                //generateResult = SqlScriptHelper.GenerateRollbackSchemaScript(originRollbackScript, dbNameWildcard, dataBaseName);
                //if (generateResult.Item1) {
                //    updateSchemaScript = generateResult.Item2;
                //}
            } catch (Exception ex) {
                _logger.LogError($"generate create db script raise error,ex:{ex.Message}");
            }

            if (string.IsNullOrEmpty(connStr) || string.IsNullOrEmpty(updateSchemaScript) || string.IsNullOrEmpty(rollbackScript)) {
                return false;
            }

            return executeScript(connStr, updateSchemaScript, rollbackScript).GetAwaiter().GetResult();
        }

        public string GenerateEncryptDbConnStr(string dbName) {
            return encrypt_conn(generateDbConnStr(dbName));
        }

        public string ResolveDatabaseName(string encryptedDbConn) {
            return resolveDatabaseName(decrypt_conn(encryptedDbConn));
        }

        protected abstract string generateCreateDbStr(string dataBaseName);
        protected abstract string generateDbConnStr( string database = null,bool pooling=true);
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
