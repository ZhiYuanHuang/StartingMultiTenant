using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Service
{
    public class TenantDbOperaService
    {
        private readonly DbServerExecutorFactory _dbServerExecutorFactory;
        private readonly ILogger<TenantDbOperaService> _logger;
        public TenantDbOperaService(DbServerExecutorFactory dbServerExecutorFactory,
            ILogger<TenantDbOperaService> logger) {
            _dbServerExecutorFactory = dbServerExecutorFactory;
            _logger = logger;
        }

        public Tuple<bool, TenantServiceDbConnModel> CreateTenantDb(string tenantDomain,string tenantIdentifire,List<string> createScriptNameList) {

        }
    }
}
