﻿using Microsoft.Extensions.Logging;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StartingMultiTenant.Service
{
    public class TenantDbOperaService
    {
        private readonly DbServerExecutorFactory _dbServerExecutorFactory;
        private readonly ILogger<TenantDbOperaService> _logger;
        private readonly ICreateDbScriptBusiness _createDbScriptBusiness;
        private readonly IDbServerBusiness _dbServerBusiness;

        private readonly Random _random;

        public TenantDbOperaService(DbServerExecutorFactory dbServerExecutorFactory,
            ILogger<TenantDbOperaService> logger,
            ICreateDbScriptBusiness createDbScriptBusiness,
            IDbServerBusiness dbServerBusiness) {
            _random = new Random();
            _dbServerExecutorFactory = dbServerExecutorFactory;
            _logger = logger;
            _createDbScriptBusiness = createDbScriptBusiness;
            _dbServerBusiness = dbServerBusiness;
        }

        public async Tuple<bool, List<TenantServiceDbConnModel>> CreateTenantDb(string tenantDomain,string tenantIdentifier,List<string> createScriptNameList) {
            List<CreateDbScriptModel> createDbScriptList= await _createDbScriptBusiness.GetListByNames(createScriptNameList);
            if (createDbScriptList.Count != createScriptNameList.Count) {
                var noexistScriptList= createScriptNameList.Except(createDbScriptList.Select(x => x.Name)).ToList();
                _logger.LogError($"create db scripts {string.Join(',',noexistScriptList)} no exist");
                return Tuple.Create<bool, List<TenantServiceDbConnModel>>(false,null);
            }
            foreach(var createScriptName in createScriptNameList) {
                if (createDbScriptList.FirstOrDefault(x => x.Name == createScriptName) == null) {
                    _logger.LogError($"create db script {createScriptName} no exist");
                    return Tuple.Create<bool, List<TenantServiceDbConnModel>>(false, null);
                }
            }

            List<DbServerModel> dbServerList= await _dbServerBusiness.GetDbServers();
            if(!dbServerList.Any()) {
                _logger.LogError($"no exist any db server");
                return Tuple.Create<bool, List<TenantServiceDbConnModel>>(false, null);
            }

            List<int> toUseDbTypes= createDbScriptList.Select(x => x.DbType).Distinct().ToList();
            List<int> existDbTypes = dbServerList.Select(x => x.DbType).Distinct().ToList();
            List<int> lackDbTypes= toUseDbTypes.Except(existDbTypes).ToList();
            if (lackDbTypes.Any()) {
                _logger.LogError($"lack {(DbTypeEnum)lackDbTypes[0]} type db");
                return Tuple.Create<bool, List<TenantServiceDbConnModel>>(false, null);
            }

            Dictionary<IDbServerExecutor, List<string>> serverAndDbDict = new Dictionary<IDbServerExecutor, List<string>>();

            bool success = true;
            foreach(var createScript in createDbScriptList) {
                var theDbServer = getRandomServer(createScript.DbType, dbServerList);
                var dbserverExecutor= _dbServerExecutorFactory.CreateDbServerExecutor(theDbServer);
                bool createResult = dbserverExecutor.CreateDb(createScript, tenantIdentifier, out string createDbName);
                if (createResult) {
                    if (serverAndDbDict.ContainsKey(dbserverExecutor)) {
                        serverAndDbDict[dbserverExecutor].Add(createDbName);
                    } else {
                        serverAndDbDict.Add(dbserverExecutor,new List<string>() { createDbName});
                    }
                } else {
                    success = false;
                    if (!string.IsNullOrEmpty(createDbName)) {
                        dbserverExecutor.DeleteDb(createDbName).ConfigureAwait(false);
                    }
                    break;
                }
            }

            if(!success && serverAndDbDict.Values.Any()) {
                foreach(var pair in serverAndDbDict) {
                    var dbExecutor = pair.Key;
                    var createDbList = pair.Value;
                    foreach(var createDb in createDbList) {
                        dbExecutor.DeleteDb(createDb).ConfigureAwait(false);
                    }
                }
            }


        }

        private DbServerModel getRandomServer(int dbType, List<DbServerModel> dbServerList) {
            IEnumerable<DbServerModel> dbServers= dbServerList.Where(x => x.DbType == dbType).Select(x => x);
            int count = dbServers.Count();
            int tmp = dbServers.Count() * 10;
            return dbServers.ElementAt(_random.Next(0, tmp) % count);
        }
    }
}
