using Microsoft.Extensions.Logging;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StartingMultiTenant.Service
{
    public class MultiTenantService
    {
        private readonly SingleTenantService _tenantDbOperaService;
        private readonly SchemaUpdateScriptBusiness _schemaUpdateScriptBusiness;
        private readonly ILogger<MultiTenantService> _logger;
        private readonly TenantServiceDbConnBusiness _tenantServiceDbConnBusiness;
        public MultiTenantService(SingleTenantService tenantDbOperaService,
            SchemaUpdateScriptBusiness schemaUpdateScriptBusiness,
            TenantServiceDbConnBusiness tenantServiceDbConnBusiness,
            ILogger<MultiTenantService> logger) { 
            _tenantDbOperaService=tenantDbOperaService;
            _schemaUpdateScriptBusiness = schemaUpdateScriptBusiness;
            _tenantServiceDbConnBusiness = tenantServiceDbConnBusiness;
            _logger=logger;
        }

        public async Task<Tuple<bool,int,int>> ExecuteSchemaUpdate(string scriptName) {
            var updateScript=await _schemaUpdateScriptBusiness.GetSchemaUpdateScriptByName(scriptName);
            if (updateScript == null) {
                _logger.LogError($"cann't find {scriptName} update script");
                return Tuple.Create(false, 0,0);
            }

            List<TenantServiceDbConnModel> tenantServiceDbConnList=await _tenantServiceDbConnBusiness.GetTenantServiceDbConns(updateScript.CreateScriptName,updateScript.BaseMajorVersion);

            if (!tenantServiceDbConnList.Any()) {
                _logger.LogError($"cann't find createScript:{updateScript.CreateScriptName} majorversion:{updateScript.BaseMajorVersion} db conn");
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
                    _tenantDbOperaService.UpdateTenantDbSchema(tenantDbConn, updateScript).ContinueWith(t => {
                        
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


    }
}
