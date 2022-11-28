﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StartingMultiTenant.Util
{
    public static class SqlScriptHelper
    {
        private const string dropDbScriptFormat = "DROP DATABASE IF EXISTS {0};";

        public static async Task<Tuple<bool,string>> GenerateCreateDbScript(string scriptFilePath,string dbNameWildcard,string uniqueDbName) {
            if (string.IsNullOrEmpty(dbNameWildcard) || string.IsNullOrEmpty(uniqueDbName)) {
                return Tuple.Create(false, string.Empty);
            }

            string scriptContent=await System.IO.File.ReadAllTextAsync(scriptFilePath);
            if (string.IsNullOrEmpty(scriptContent)) {
                return Tuple.Create(false, string.Empty);
            }
            if (!Regex.IsMatch(scriptContent, dbNameWildcard)) {
                return Tuple.Create(false, string.Empty);
            }
            
            string scriptStr= Regex.Replace(scriptContent,dbNameWildcard,uniqueDbName);
            return Tuple.Create(true, scriptStr);
        }

        public static async Task<Tuple<bool, string>> GenerateUpdateSchemaScript(string scriptFilePath, string dbNameWildcard, string uniqueDbName) {
            if (string.IsNullOrEmpty(dbNameWildcard) || string.IsNullOrEmpty(uniqueDbName)) {
                return Tuple.Create(false, string.Empty);
            }

            string scriptContent = await System.IO.File.ReadAllTextAsync(scriptFilePath);
            if (string.IsNullOrEmpty(scriptContent)) {
                return Tuple.Create(false, string.Empty);
            }
            if (!Regex.IsMatch(scriptContent, dbNameWildcard)) {
                return Tuple.Create(false, string.Empty);
            }

            string scriptStr = Regex.Replace(scriptContent, dbNameWildcard, uniqueDbName);
            return Tuple.Create(true, scriptStr);
        }

        public static async Task<Tuple<bool, string>> GenerateRollbackSchemaScript(string scriptFilePath, string dbNameWildcard, string uniqueDbName) {
            if (string.IsNullOrEmpty(dbNameWildcard) || string.IsNullOrEmpty(uniqueDbName)) {
                return Tuple.Create(false, string.Empty);
            }

            string scriptContent = await System.IO.File.ReadAllTextAsync(scriptFilePath);
            if (string.IsNullOrEmpty(scriptContent)) {
                return Tuple.Create(false, string.Empty);
            }
            if (!Regex.IsMatch(scriptContent, dbNameWildcard)) {
                return Tuple.Create(false, string.Empty);
            }

            string scriptStr = Regex.Replace(scriptContent, dbNameWildcard, uniqueDbName);
            return Tuple.Create(true, scriptStr);
        }

        public static string GenerateDeleteDbScript(string dbName) {
            return string.Format(dropDbScriptFormat,dbName);
        }

        public static string GenerateRandomDbName(string dbIdentifier,string tenantGuid) {
            return string.Format("{0}_{1}", dbIdentifier, HashUtil.Hash_8(tenantGuid + $"_{DateTimeOffset.Now.ToUnixTimeSeconds()}"));
        }
    }
}
