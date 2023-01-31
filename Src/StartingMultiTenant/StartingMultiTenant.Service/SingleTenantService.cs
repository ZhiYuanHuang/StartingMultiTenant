using Microsoft.Extensions.Logging;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Service
{
    public class SingleTenantService
    {
        private readonly DbServerExecutorFactory _dbServerExecutorFactory;
        private readonly ILogger<SingleTenantService> _logger;
        private readonly CreateDbScriptBusiness _createDbScriptBusiness;
        private readonly DbServerBusiness _dbServerBusiness;
        private readonly SchemaUpdateScriptBusiness _schemaUpdateScriptBusiness;
        private readonly TenantServiceDbConnBusiness _tenantServiceDbConnBusiness;
        private readonly TenantActionNoticeService _actionNoticeService;

        private readonly Random _random;

        public SingleTenantService(DbServerExecutorFactory dbServerExecutorFactory,
            ILogger<SingleTenantService> logger,
            CreateDbScriptBusiness createDbScriptBusiness,
            DbServerBusiness dbServerBusiness,
            SchemaUpdateScriptBusiness schemaUpdateScriptBusiness,
            TenantActionNoticeService actionNoticeService,
            TenantServiceDbConnBusiness tenantServiceDbConnBusiness) {
            _random = new Random();
            _dbServerExecutorFactory = dbServerExecutorFactory;
            _logger = logger;
            _createDbScriptBusiness = createDbScriptBusiness;
            _dbServerBusiness = dbServerBusiness;
            _schemaUpdateScriptBusiness = schemaUpdateScriptBusiness;
            _tenantServiceDbConnBusiness = tenantServiceDbConnBusiness;
            _actionNoticeService = actionNoticeService;
        }

        public async Task<bool> CreateTenantDbs(string tenantDomain, string tenantIdentifier, List<string> createScriptNameList, bool overrideWhenExisted) {
            _actionNoticeService.PublishTenantStartCreate(tenantDomain,tenantIdentifier);

            bool result = await createTenantDbs(tenantDomain,tenantIdentifier,createScriptNameList,overrideWhenExisted);

            _actionNoticeService.PublishTenantCreated(tenantDomain,tenantIdentifier,result);

            return result;
        }

        public async Task<bool> CreateTenantDbs(string tenantDomain, string tenantIdentifier, List<Int64> createScriptIdList, bool overrideWhenExisted) {
            _actionNoticeService.PublishTenantStartCreate(tenantDomain, tenantIdentifier);

            bool result = await createTenantDbs(tenantDomain, tenantIdentifier, createScriptIdList, overrideWhenExisted);

            _actionNoticeService.PublishTenantCreated(tenantDomain, tenantIdentifier, result);

            return result;
        }

        internal async Task<bool> createTenantDbs(string tenantDomain, string tenantIdentifier, List<Int64> createScriptIdList, bool overrideWhenExisted) {

            List<CreateDbScriptModel> createDbScriptList = _createDbScriptBusiness.Get(createScriptIdList);

            if (createDbScriptList.Count != createScriptIdList.Count) {
                var noexistScriptList = createScriptIdList.Except(createDbScriptList.Select(x => x.Id)).ToList();
                _logger.LogError($"create db scripts {string.Join(',', noexistScriptList)} no exist");
                return false;
            }
            foreach (var createScriptId in createScriptIdList) {
                if (createDbScriptList.FirstOrDefault(x => x.Id == createScriptId) == null) {
                    _logger.LogError($"create db script {createScriptId} no exist");
                    return false;
                }
            }

            List<DbServerModel> dbServerList = _dbServerBusiness.GetDbServers();
            dbServerList = dbServerList.Where(x => x.CanCreateNew).Select(x => x).ToList();
            if (!dbServerList.Any()) {
                _logger.LogError($"no exist any db server");
                return false;
            }

            List<int> toUseDbTypes = createDbScriptList.Select(x => x.DbType).Distinct().ToList();
            List<int> existDbTypes = dbServerList.Select(x => x.DbType).Distinct().ToList();
            List<int> lackDbTypes = toUseDbTypes.Except(existDbTypes).ToList();
            if (lackDbTypes.Any()) {
                _logger.LogError($"lack {(DbTypeEnum)lackDbTypes[0]} type db");
                return false;
            }

            Dictionary<IDbServerExecutor, List<string>> serverAndDbDict = new Dictionary<IDbServerExecutor, List<string>>();

            bool success = true;
            List<TenantServiceDbConnModel> createDbSet = new List<TenantServiceDbConnModel>();
            foreach (var createScript in createDbScriptList) {
                var theDbServer = getRandomServer(createScript.DbType, dbServerList);
                var dbserverExecutor = _dbServerExecutorFactory.CreateDbServerExecutor(theDbServer);
                bool createResult = dbserverExecutor.CreateDb(createScript, tenantIdentifier, out string createDbName);
                if (createResult) {
                    if (serverAndDbDict.ContainsKey(dbserverExecutor)) {
                        serverAndDbDict[dbserverExecutor].Add(createDbName);
                    } else {
                        serverAndDbDict.Add(dbserverExecutor, new List<string>() { createDbName });
                    }

                    var schemaUpdateScripts = _schemaUpdateScriptBusiness.GetSchemaUpdateScripts(createScript.Id);
                    int lastMinorVersion = 0;
                    if (schemaUpdateScripts.Any()) {
                        schemaUpdateScripts = schemaUpdateScripts.OrderBy(x => x.MinorVersion).ToList();
                        bool updateSchemaError = false;
                        foreach (var schemaUpdateScript in schemaUpdateScripts) {
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
                        TenantDomain = tenantDomain,
                        TenantIdentifier = tenantIdentifier,
                        ServiceIdentifier = createScript.ServiceIdentifier,
                        DbIdentifier = createScript.DbIdentifier,
                        CreateScriptName = createScript.Name,
                        CreateScriptVersion = createScript.MajorVersion,
                        CurSchemaVersion = lastMinorVersion,
                        DbServerId = theDbServer.Id,
                        EncryptedConnStr = dbserverExecutor.GenerateEncryptDbConnStr(createDbName),
                    });
                } else {
                    success = false;
                    if (!string.IsNullOrEmpty(createDbName)) {
                        await dbserverExecutor.DeleteDb(createDbName).ConfigureAwait(false);
                    }
                    break;
                }
            }

            if (!success && serverAndDbDict.Values.Any()) {
                foreach (var pair in serverAndDbDict) {
                    var dbExecutor = pair.Key;
                    var createDbList = pair.Value;
                    foreach (var createDb in createDbList) {
                        await dbExecutor.DeleteDb(createDb).ConfigureAwait(false);
                    }
                }
                return false;
            }

            success = _tenantServiceDbConnBusiness.BatchInsertDbConns(createDbSet, overrideWhenExisted);
            if (!success) {
                foreach (var pair in serverAndDbDict) {
                    var dbExecutor = pair.Key;
                    var createDbList = pair.Value;
                    foreach (var createDb in createDbList) {
                        await dbExecutor.DeleteDb(createDb).ConfigureAwait(false);
                    }
                }

                return false;
            }

            return true;
        }


        internal async Task<bool> createTenantDbs(string tenantDomain,string tenantIdentifier,List<string> createScriptNameList,bool overrideWhenExisted) {

            List<CreateDbScriptModel> originCreateDbScriptList= await _createDbScriptBusiness.GetListByNames(createScriptNameList);

            List<CreateDbScriptModel> createDbScriptList= originCreateDbScriptList.GroupBy(x => x.Name).Select(g => {
                int tmpMaxVersion = g.Max(x => x.MajorVersion);
                return g.First(x => x.MajorVersion == tmpMaxVersion);
            }).ToList();

            if (createDbScriptList.Count != createScriptNameList.Count) {
                var noexistScriptList= createScriptNameList.Except(createDbScriptList.Select(x => x.Name)).ToList();
                _logger.LogError($"create db scripts {string.Join(',',noexistScriptList)} no exist");
                return false;
            }
            foreach(var createScriptName in createScriptNameList) {
                if (createDbScriptList.FirstOrDefault(x => x.Name == createScriptName) == null) {
                    _logger.LogError($"create db script {createScriptName} no exist");
                    return false;
                }
            }

            List<DbServerModel> dbServerList= _dbServerBusiness.GetDbServers();
            dbServerList= dbServerList.Where(x => x.CanCreateNew).Select(x => x).ToList();
            if(!dbServerList.Any()) {
                _logger.LogError($"no exist any db server");
                return false;
            }

            List<int> toUseDbTypes= createDbScriptList.Select(x => x.DbType).Distinct().ToList();
            List<int> existDbTypes = dbServerList.Select(x => x.DbType).Distinct().ToList();
            List<int> lackDbTypes= toUseDbTypes.Except(existDbTypes).ToList();
            if (lackDbTypes.Any()) {
                _logger.LogError($"lack {(DbTypeEnum)lackDbTypes[0]} type db");
                return false;
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

                    var schemaUpdateScripts= _schemaUpdateScriptBusiness.GetSchemaUpdateScripts(createScript.Id);
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
                return false;
            }

            success= _tenantServiceDbConnBusiness.BatchInsertDbConns(createDbSet,overrideWhenExisted);
            if (!success) {
                foreach (var pair in serverAndDbDict) {
                    var dbExecutor = pair.Key;
                    var createDbList = pair.Value;
                    foreach (var createDb in createDbList) {
                        await dbExecutor.DeleteDb(createDb).ConfigureAwait(false);
                    }
                }

                return false;
            }

            return true;
        }

        public async Task<bool> UpdateTenantDbSchema(TenantServiceDbConnModel tenantServiceDbConn, SchemaUpdateScriptModel schemaUpdateScript) {
            _actionNoticeService.PublishTenantStartUpdateSchema(tenantServiceDbConn.TenantDomain,tenantServiceDbConn.TenantIdentifier,tenantServiceDbConn.ServiceIdentifier);

            bool result = await updateTenantDbSchema(tenantServiceDbConn,schemaUpdateScript);

            _actionNoticeService.PublishTenantUpdated(tenantServiceDbConn.TenantDomain, tenantServiceDbConn.TenantIdentifier, tenantServiceDbConn.ServiceIdentifier, result);

            return result;
        }

        internal async Task<bool> updateTenantDbSchema(TenantServiceDbConnModel tenantServiceDbConn, SchemaUpdateScriptModel schemaUpdateScript) {
            if (tenantServiceDbConn.CurSchemaVersion >= schemaUpdateScript.MinorVersion ) {
                _logger.LogError($"TenantDomain {tenantServiceDbConn.TenantDomain} identifier: {tenantServiceDbConn.TenantIdentifier} updateScript :{schemaUpdateScript.Name} version is {schemaUpdateScript.MinorVersion} ,curSchemaVersion is {tenantServiceDbConn.CurSchemaVersion}");
                return false;
            }

            List<DbServerModel> dbServerList = _dbServerBusiness.GetDbServers(tenantServiceDbConn.DbServerId);
            if(!dbServerList.Any()) {
                _logger.LogError($"未找到 {tenantServiceDbConn.TenantDomain} {tenantServiceDbConn.TenantIdentifier} {schemaUpdateScript.CreateDbScriptId} 创建的数据库链接对应的数据库服务器");
                return false;
            }

            DbServerModel theDbServer = dbServerList[0];
            IDbServerExecutor dbServerExecutor = _dbServerExecutorFactory.CreateDbServerExecutor(theDbServer);

            var updateResult= dbServerExecutor.UpdateSchemaByConnStr(tenantServiceDbConn.EncryptedConnStr,schemaUpdateScript);
            if (updateResult) {
                tenantServiceDbConn.CurSchemaVersion=schemaUpdateScript.MinorVersion;
                _tenantServiceDbConnBusiness.InsertOrUpdate(tenantServiceDbConn);
            }
            return updateResult;
        }

        public async Task<bool> ExchangeTenantDbServer(TenantServiceDbConnModel toUpdateDbConn, DbServerModel toUseDbServer) {
            _actionNoticeService.PublishTenantStartExchange(toUpdateDbConn.TenantDomain,toUpdateDbConn.TenantIdentifier,toUpdateDbConn.ServiceIdentifier);

            bool result = await exchangeTenantDbServer(toUpdateDbConn,toUseDbServer);

            _actionNoticeService.PublishTenantExchanged(toUpdateDbConn.TenantDomain, toUpdateDbConn.TenantIdentifier, toUpdateDbConn.ServiceIdentifier,result);

            return result;
        }

        internal async Task<bool> exchangeTenantDbServer(TenantServiceDbConnModel toUpdateDbConn,DbServerModel toUseDbServer) {
            
            IDbServerExecutor dbServerExecutor = _dbServerExecutorFactory.CreateDbServerExecutor(toUseDbServer);
            string dbName= dbServerExecutor.ResolveDatabaseName(toUpdateDbConn.EncryptedConnStr);
            string encryptedConnStr= dbServerExecutor.GenerateEncryptDbConnStr(dbName);

            return _tenantServiceDbConnBusiness.ExchangeToAnotherDbServer(toUpdateDbConn, toUseDbServer.Id,encryptedConnStr);
        }

        private DbServerModel getRandomServer(int dbType, List<DbServerModel> dbServerList) {
            IEnumerable<DbServerModel> dbServers= dbServerList.Where(x => x.DbType == dbType).Select(x => x);
            int count = dbServers.Count();
            int tmp = dbServers.Count() * 10;
            return dbServers.ElementAt(_random.Next(0, tmp) % count);
        }
    }
}
