using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using StartingMultiTenant.Model.Dto;
using Microsoft.Extensions.Logging;
using StartingMultiTenant.Util;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data.Common;

namespace StartingMultiTenant.Service
{
    public class PgsqlDbServerExecutor: BaseDbServerExecutor
    {
        public PgsqlDbServerExecutor(DbServerModel dbServer,
            ILogger<PgsqlDbServerExecutor> logger,
            SysConstService sysConstService,
            EncryptService encryptService) :base(dbServer,logger,sysConstService, encryptService) {
        }

        protected override async Task<bool> executeScript(string dbConnStr,string dbScriptStr) {
            NpgsqlConnection conn = new NpgsqlConnection(dbConnStr);

            bool result = false;

            NpgsqlCommand npgsqlCommand = null;
            try {

                await conn.OpenAsync();

                if (conn.State == System.Data.ConnectionState.Open) {
                    //CREATE DATABASE "Test_db" WITH OWNER = postgres ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'en_US.utf8';
                    
                    npgsqlCommand = new NpgsqlCommand(dbScriptStr, conn);
                    npgsqlCommand.CommandTimeout = CmdTimeoutSec;
                    await npgsqlCommand.ExecuteNonQueryAsync();
                    result = true;
                }
            } catch (Exception ex) {
                result = false;
                _logger.LogError($"execute script raise error,ex:{ex.Message}");
            } finally {
                npgsqlCommand?.Dispose();
                conn?.Close();
                conn?.Dispose();
            }

            return result;
        }

        protected override async Task<bool> executeScript(string dbConnStr, string updateSchemaScript, string rollbackScript) {
            NpgsqlConnection conn = new NpgsqlConnection(dbConnStr);

            bool result = false;
            NpgsqlTransaction trans = null;
            try {

                await conn.OpenAsync();

                if (conn.State == System.Data.ConnectionState.Open) {
                    trans = conn.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
                    var npgsqlCommand = new NpgsqlCommand(updateSchemaScript, conn, trans);
                    npgsqlCommand.CommandTimeout = CmdTimeoutSec;
                    try {
                        await npgsqlCommand.ExecuteNonQueryAsync();
                        trans.Commit();
                        result = true;
                    } catch(Exception ex) {
                        trans.Rollback();
                        result = false;

                    } finally {
                        trans?.Dispose();
                    }

                    if (!result && !string.IsNullOrWhiteSpace(rollbackScript)) {
                        try {
                            trans = conn.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
                            npgsqlCommand = new NpgsqlCommand(rollbackScript,conn,trans);
                            npgsqlCommand.CommandTimeout = CmdTimeoutSec;
                            await npgsqlCommand.ExecuteNonQueryAsync();
                            trans.Commit();
                        } catch(Exception ex) {
                            trans.Rollback();
                        } finally {
                            trans?.Dispose();
                        }
                    }
                   
                }
            } catch (Exception ex) {
                result = false;
                _logger.LogError($"execute script raise error,ex:{ex.Message}");
            } finally {
                conn?.Dispose();
            }

            return result;
        }

        protected override string generateCreateDbStr(string dataBaseName) {
            //CREATE DATABASE "Test_db" WITH OWNER = postgres ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'en_US.utf8';
            string userName = DbServer.UserName;
            if (string.IsNullOrEmpty(userName)) {
                userName = "postgres";
            }

            return string.Format("CREATE DATABASE \"{0}\" WITH OWNER = {1} ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'en_US.utf8';",dataBaseName.ToLower(),userName);
        }

        protected override string generateDbConnStr(string database=null, bool pooling = true) {
            string decryptUserPwd = _encryptService.Decrypt_DbServerPwd(_dbServer.EncryptUserpwd);
            return $"Host={_dbServer.ServerHost};Port={_dbServer.ServerPort};UserName={_dbServer.UserName};Password={decryptUserPwd};{(pooling?"":"Pooling=false;")}{(string.IsNullOrEmpty(database)?"":$"Database={database};")}";
            //string connString = "Host=localhost;Port=5432;Username=postgres;Password=admin;Database=postgres";
            
        }

        protected override string resolveDatabaseName(string dbConnStr) {
            var match= Regex.Match(dbConnStr, "Database=([\\S]+?)(?=$|;)", RegexOptions.IgnoreCase);
            if (match.Success) {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }
    }
}
