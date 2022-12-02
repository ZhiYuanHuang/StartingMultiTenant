using Microsoft.Extensions.Logging;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Service
{
    public class TenantDbOperaService
    {
        private readonly DbServerExecutorFactory _dbServerExecutorFactory;
        private readonly ILogger<TenantDbOperaService> _logger;
        private readonly ICreateDbScriptBusiness _createDbScriptBusiness;
        private readonly IDbServerBusiness _dbServerBusiness;
        private readonly ISchemaUpdateScriptBusiness _schemaUpdateScriptBusiness;
        private readonly ITenantServiceDbConnBusiness _tenantServiceDbConnBusiness;

        private readonly Random _random;

        public TenantDbOperaService(DbServerExecutorFactory dbServerExecutorFactory,
            ILogger<TenantDbOperaService> logger,
            ICreateDbScriptBusiness createDbScriptBusiness,
            IDbServerBusiness dbServerBusiness,
            ISchemaUpdateScriptBusiness schemaUpdateScriptBusiness,
            ITenantServiceDbConnBusiness tenantServiceDbConnBusiness) {
            _random = new Random();
            _dbServerExecutorFactory = dbServerExecutorFactory;
            _logger = logger;
            _createDbScriptBusiness = createDbScriptBusiness;
            _dbServerBusiness = dbServerBusiness;
            _schemaUpdateScriptBusiness = schemaUpdateScriptBusiness;
            _tenantServiceDbConnBusiness = tenantServiceDbConnBusiness;
        }

        public async Task<Tuple<bool, List<TenantServiceDbConnModel>>> CreateTenantDb(string tenantDomain,string tenantIdentifier,List<string> createScriptNameList) {
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
            dbServerList= dbServerList.Where(x => x.CanCreateNew).Select(x => x).ToList();
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
            List<TenantServiceDbConnModel> createDbSet = new List<TenantServiceDbConnModel>();
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

                    var schemaUpdateScripts=await _schemaUpdateScriptBusiness.GetSchemaUpdateScripts(createScript.Name,createScript.MajorVersion);
                    int lastMinorVersion = 0;
                    if (schemaUpdateScripts.Any()) {
                        schemaUpdateScripts = schemaUpdateScripts.OrderBy(x => x.MinorVersion).ToList();
                        bool updateSchemaError = false;
                        foreach(var schemaUpdateScript in schemaUpdateScripts) {
                            lastMinorVersion = schemaUpdateScript.MinorVersion;
                            if (!dbserverExecutor.UpdateSchemaByDatabase(createDbName, schemaUpdateScript)) {
                                updateSchemaError = true;
                                break;
                            }
                        }
                        if (updateSchemaError) {
                            success = false;
                            break;
                        }
                    }

                    createDbSet.Add(new TenantServiceDbConnModel() { 
                        TenantDomain=tenantDomain,
                        TenantIdentifier=tenantIdentifier,
                        ServiceIdentifier=createScript.ServiceIdentifier,
                        DbIdentifier=createScript.DbIdentifier,
                        CreateScriptName=createScript.Name,
                        CreateScriptVersion=createScript.MajorVersion,
                        CurSchemaVersion=lastMinorVersion,
                        DbServerId=theDbServer.Id,
                        EncryptedConnStr=dbserverExecutor.GenerateEncryptDbConnStr(createDbName),
                    });
                } else {
                    success = false;
                    if (!string.IsNullOrEmpty(createDbName)) {
                        await dbserverExecutor.DeleteDb(createDbName).ConfigureAwait(false);
                    }
                    break;
                }
            }

            if(!success && serverAndDbDict.Values.Any()) {
                foreach(var pair in serverAndDbDict) {
                    var dbExecutor = pair.Key;
                    var createDbList = pair.Value;
                    foreach(var createDb in createDbList) {
                        await dbExecutor.DeleteDb(createDb).ConfigureAwait(false);
                    }
                }
                return Tuple.Create<bool, List<TenantServiceDbConnModel>>(false, null);
            }

            return Tuple.Create<bool, List<TenantServiceDbConnModel>>(true, createDbSet);
        }

        public async Task<bool> UpdateTenantDb(string tenantDomain, string tenantIdentifier,SchemaUpdateScriptModel schemaUpdateScript) {
            List<TenantServiceDbConnModel> tenantServiceDbConnList=await _tenantServiceDbConnBusiness.GetTenantServiceDbConns(tenantDomain,tenantIdentifier,schemaUpdateScript.Name);
            var toUpdateSchemaDb= tenantServiceDbConnList.Where(x => x.CreateScriptVersion == schemaUpdateScript.BaseMajorVersion && x.CurSchemaVersion == (schemaUpdateScript.MinorVersion - 1)).FirstOrDefault();

            if (toUpdateSchemaDb == null) {
                _logger.LogError($"未找到 {tenantDomain} {tenantIdentifier} {schemaUpdateScript.Name} 创建的数据库链接");
                return false;
            }
            List<DbServerModel> dbServerList = await _dbServerBusiness.GetDbServers(toUpdateSchemaDb.DbServerId);
            if(!dbServerList.Any()) {
                _logger.LogError($"未找到 {tenantDomain} {tenantIdentifier} {schemaUpdateScript.Name} 创建的数据库链接对应的数据库服务器");
                return false;
            }

            DbServerModel theDbServer = dbServerList[0];
            IDbServerExecutor dbServerExecutor = _dbServerExecutorFactory.CreateDbServerExecutor(theDbServer);

            return dbServerExecutor.UpdateSchemaByConnStr(toUpdateSchemaDb.EncryptedConnStr,schemaUpdateScript);
        }

        private DbServerModel getRandomServer(int dbType, List<DbServerModel> dbServerList) {
            IEnumerable<DbServerModel> dbServers= dbServerList.Where(x => x.DbType == dbType).Select(x => x);
            int count = dbServers.Count();
            int tmp = dbServers.Count() * 10;
            return dbServers.ElementAt(_random.Next(0, tmp) % count);
        }
    }
}
