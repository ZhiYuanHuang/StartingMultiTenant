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
        private readonly TenantDbOperaService _tenantDbOperaService;
        private readonly ISchemaUpdateScriptBusiness _schemaUpdateScriptBusiness;
        private readonly ILogger<MultiTenantService> _logger;
        private readonly ITenantServiceDbConnBusiness _tenantServiceDbConnBusiness;
        public MultiTenantService(TenantDbOperaService tenantDbOperaService,
            ISchemaUpdateScriptBusiness schemaUpdateScriptBusiness,
            ITenantServiceDbConnBusiness tenantServiceDbConnBusiness,
            ILogger<MultiTenantService> logger) { 
            _tenantDbOperaService=tenantDbOperaService;
            _schemaUpdateScriptBusiness = schemaUpdateScriptBusiness;
            _tenantServiceDbConnBusiness = tenantServiceDbConnBusiness;
            _logger=logger;
        }

        public async Task<Tuple<bool,bool>> ExecuteSchemaUpdate(string scriptName) {
            var updateScript=await _schemaUpdateScriptBusiness.GetSchemaUpdateScriptByName(scriptName);
            if (updateScript == null) {
                _logger.LogError($"cann't find {scriptName} update script");
                return Tuple.Create(false, false);
            }

            List<TenantServiceDbConnModel> tenantServiceDbConnList=await _tenantServiceDbConnBusiness.GetTenantServiceDbConns(updateScript.CreateScriptName,updateScript.BaseMajorVersion);

            if (!tenantServiceDbConnList.Any()) {
                _logger.LogError($"cann't find createScript:{updateScript.CreateScriptName} majorversion:{updateScript.BaseMajorVersion} db conn");
                return Tuple.Create(false, false);
            }

            var tenantGroups = tenantServiceDbConnList.GroupBy(x => x.TenantDomain);

            Semaphore semaphore = new Semaphore(0,10);

            bool atOnceError = false;
            int taskCount = 0;
            foreach (var tenantGroup in tenantGroups) {
                string tenantDomain=tenantGroup.Key;
                foreach(var tenantDbConn in tenantGroup) {
                    string tenantIdentifier = tenantDbConn.TenantIdentifier;
                    semaphore.WaitOne();
                    Interlocked.Increment(ref taskCount);
                    _tenantDbOperaService.UpdateTenantDb(tenantDbConn, updateScript).ContinueWith(t => {
                        Interlocked.Decrement(ref taskCount);

                        if (t.IsFaulted) {
                            atOnceError = true;
                        }
                        else if (t.IsCompleted) {
                            if (!t.Result) {
                                atOnceError = true;
                            }
                        }

                        semaphore.Release();
                    });
                }
            }

            while(Volatile.Read(ref taskCount) != 0) {
                Thread.Sleep(30);
            }

            if (atOnceError) {
                return Tuple.Create(false, true);
            }

            return Tuple.Create(true,true);
        }


    }
}
