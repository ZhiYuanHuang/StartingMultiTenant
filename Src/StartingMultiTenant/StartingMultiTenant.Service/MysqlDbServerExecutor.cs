﻿using Microsoft.Extensions.Logging;
using MySqlConnector;
using Npgsql;
using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StartingMultiTenant.Service
{
    public class MysqlDbServerExecutor : BaseDbServerExecutor
    {
        public MysqlDbServerExecutor(DbServerModel dbServer,
            ILogger<MysqlDbServerExecutor> logger,
            SysConstService sysConstService,
            EncryptService encryptService) : base(dbServer, logger, sysConstService, encryptService) { 

        }

        protected override async Task<bool> executeScript(string dbConnStr, string dbScriptStr) {
            MySqlConnection conn = new MySqlConnection(dbConnStr);
        

            bool result = false;
            MySqlCommand mysqlCommand=null;
            try {

                
                await conn.OpenAsync();

                if (conn.State == System.Data.ConnectionState.Open) {
                    mysqlCommand = new MySqlCommand(dbScriptStr, conn);
                    mysqlCommand.CommandTimeout = CmdTimeoutSec;
                    await mysqlCommand.ExecuteNonQueryAsync();
                    result = true;
                }
            } catch (Exception ex) {
                result = false;
                _logger.LogError($"execute script raise error,ex:{ex.Message}");
            } finally {
                mysqlCommand?.Dispose();
                conn?.Close();
                conn?.Dispose();
            }

            return result;
        }

        protected override async Task<bool> executeScript(string dbConnStr, string updateSchemaScript, string rollbackScript) {
            MySqlConnection conn = new MySqlConnection(dbConnStr);

            bool result = false;
            try {

                await conn.OpenAsync();

                if (conn.State == System.Data.ConnectionState.Open) {

                    var mysqlCommand = new MySqlCommand(updateSchemaScript, conn);
                    mysqlCommand.CommandTimeout = CmdTimeoutSec;
                    try {

                        await mysqlCommand.ExecuteNonQueryAsync();
                        result = true;
                    } catch {
                        result = false;
                    }

                    if (!result && !string.IsNullOrWhiteSpace(rollbackScript)) {
                        try {
                            mysqlCommand = new MySqlCommand(rollbackScript, conn);
                            mysqlCommand.CommandTimeout = CmdTimeoutSec;
                            await mysqlCommand.ExecuteNonQueryAsync();
                        } catch {
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
            //CREATE DATABASE /*!32312 IF NOT EXISTS*/ `tunano.ldc.pfv` /*!40100 DEFAULT CHARACTER SET utf8 */;

            return string.Format("CREATE DATABASE `{0}` DEFAULT CHARACTER SET utf8mb4 DEFAULT COLLATE utf8mb4_general_ci;", dataBaseName.ToLower());
        }

        protected override string generateDbConnStr(string database = null, bool pooling = true) {
            //Database=tenantstore.mulids;Data Source=127.0.0.1;Port=3307;User Id=root;Password=123456;Charset=utf8;
            string decryptUserPwd = _encryptService.Decrypt_DbServerPwd(_dbServer.EncryptUserpwd);
            return $"Data Source={_dbServer.ServerHost};Port={_dbServer.ServerPort};User Id={_dbServer.UserName};Password={decryptUserPwd};Charset=utf8;{(pooling ? "" : "Pooling=false;")}{(string.IsNullOrEmpty(database) ? "" : $"Database={database};")}";
        }

        protected override string resolveDatabaseName(string dbConnStr) {
            var match = Regex.Match(dbConnStr, "Database=([\\S]+?)(?=$|;)",RegexOptions.IgnoreCase);
            if (match.Success) {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }
    }
}
