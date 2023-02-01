using Microsoft.Extensions.Logging;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StartingMultiTenant.Service
{
    public class MultiTenantService
    {
        private readonly SingleTenantService _singleTenantService;
        private readonly SchemaUpdateScriptBusiness _schemaUpdateScriptBusiness;
        private readonly ILogger<MultiTenantService> _logger;
        private readonly TenantServiceDbConnBusiness _tenantServiceDbConnBusiness;
        private readonly DbServerBusiness _dbServerBusiness;
        private readonly CreateDbScriptBusiness _createDbScriptBusiness;
        public MultiTenantService(SingleTenantService singleTenantService,
            SchemaUpdateScriptBusiness schemaUpdateScriptBusiness,
            TenantServiceDbConnBusiness tenantServiceDbConnBusiness,
            DbServerBusiness dbServerBusiness,
            CreateDbScriptBusiness createDbScriptBusiness,
            ILogger<MultiTenantService> logger) { 
            _singleTenantService=singleTenantService;
            _schemaUpdateScriptBusiness = schemaUpdateScriptBusiness;
            _tenantServiceDbConnBusiness = tenantServiceDbConnBusiness;
            _dbServerBusiness = dbServerBusiness;
            _createDbScriptBusiness = createDbScriptBusiness;
            _logger=logger;
        }

        public async Task<Tuple<bool,int,int>> ExecuteSchemaUpdate(string scriptName) {
            var updateScript=await _schemaUpdateScriptBusiness.GetSchemaUpdateScriptByName(scriptName);
            if (updateScript == null) {
                _logger.LogError($"cann't find {scriptName} update script");
                return Tuple.Create(false, 0,0);
            }

            var createDbScript = _createDbScriptBusiness.GetNoContent(updateScript.Id);

            List<TenantServiceDbConnModel> tenantServiceDbConnList=await _tenantServiceDbConnBusiness.GetTenantServiceDbConns(createDbScript.Name, createDbScript.MajorVersion);

            if (!tenantServiceDbConnList.Any()) {
                _logger.LogError($"cann't find createScript:{createDbScript.Name} majorversion:{createDbScript.MajorVersion} db conn");
                return Tuple.Create(false,0,0);
            }

            var tenantGroups = tenantServiceDbConnList.GroupBy(x => x.TenantDomain);

            Semaphore semaphore = new Semaphore(10,10);

            int successCount = 0;
            int failureCount = 0;
            int taskCount = 0;
            foreach (var tenantGroup in tenantGroups) {
                string tenantDomain=tenantGroup.Key;
                foreach(var tenantDbConn in tenantGroup) {
                    string tenantIdentifier = tenantDbConn.TenantIdentifier;

                    if(tenantDbConn.CurSchemaVersion>= updateScript.MinorVersion) {
                        continue;
                    }

                    semaphore.WaitOne();
                    Interlocked.Increment(ref taskCount);
                    _singleTenantService.UpdateTenantDbSchema(tenantDbConn, updateScript).ContinueWith(t => {
                        
                        if (t.IsFaulted) {
                            Interlocked.Increment(ref failureCount);
                        }
                        else if (t.IsCompleted) {
                            if (!t.Result) {
                                Interlocked.Increment(ref failureCount);
                            } else {
                                Interlocked.Increment(ref successCount);
                            }
                        } else {
                            Interlocked.Increment(ref failureCount);
                        }

                        Interlocked.Decrement(ref taskCount);
                        semaphore.Release();
                    });
                }
            }

            while(Volatile.Read(ref taskCount) != 0) {
                Thread.Sleep(30);
            }

            return Tuple.Create(failureCount==0,successCount,failureCount);
        }

        public async Task<Tuple<bool,int,int>> ExchangeDbServer(DbServerModel oldDbServer, DbServerModel newDbServer) {
            var toExchangeDbConns= _tenantServiceDbConnBusiness.GetConnListByDbServer(oldDbServer.Id);
            if (!toExchangeDbConns.Any()) {
                return Tuple.Create(true,0,0);
            }

            int successCount = 0;
            int failureCount = 0;
            foreach(var dbConn in toExchangeDbConns) {
                bool success= await _singleTenantService.ExchangeTenantDbServer(dbConn,newDbServer);
                if (success) {
                    successCount++;
                } else {
                    failureCount++;
                }
            }

            return Tuple.Create(failureCount==0,successCount,failureCount);
        }
    }
}
