using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Business
{
    public class TenantServiceDbConnBusiness 
    {
        private readonly TenantServiceDbConnRepository _tenantServiceDbConnRepository;
        private readonly HistoryTenantServiceDbConnRepository _historyTenantServiceDbConnRepo;
        private readonly ILogger<TenantServiceDbConnBusiness> _logger;
        public TenantServiceDbConnBusiness(TenantServiceDbConnRepository tenantServiceDbConnRepository,
            HistoryTenantServiceDbConnRepository historyTenantServiceDbConnRepo,
            ILogger<TenantServiceDbConnBusiness> logger) {
            _tenantServiceDbConnRepository = tenantServiceDbConnRepository;
            _historyTenantServiceDbConnRepo = historyTenantServiceDbConnRepo;
            _logger = logger;
        }
        public async Task<List<TenantServiceDbConnModel>> GetTenantServiceDbConns(string tenantDomain, string tenantIdentifier, string createScriptName) {
            return await Task.Factory.StartNew(()=>_tenantServiceDbConnRepository.GetTenantServiceDbConns(tenantDomain,tenantIdentifier,createScriptName));
        }

        public async Task<List<TenantServiceDbConnModel>> GetTenantServiceDbConns(long? dbConnId = null) {
            return await Task.Factory.StartNew(() => _tenantServiceDbConnRepository.GetTenantServiceDbConns(dbConnId));
        }

        public bool BatchInsertDbConns(List<TenantServiceDbConnModel> dbConnList) {
            return _tenantServiceDbConnRepository.BatchInsertDbConns(dbConnList);
        }

        public bool ExchangeToAnotherDbServer(TenantServiceDbConnModel toUpdateDbConn, Int64 newDbServerId, string newEncryptedConnStr) {

            bool success = false;
            try {
                _tenantServiceDbConnRepository.BeginTransaction();

                _tenantServiceDbConnRepository.ExchangeDbServer(toUpdateDbConn.Id,newDbServerId,newEncryptedConnStr);

                _historyTenantServiceDbConnRepo.InsertHistoryDbConn(toUpdateDbConn);

                _tenantServiceDbConnRepository.CommitTransaction();
                success = true;
            }catch(Exception ex) {
                success = false;
                _logger.LogError(ex.Message);
                _tenantServiceDbConnRepository.RollbackTransaction();
            }
            return success;
        }

        public async Task<List<TenantServiceDbConnModel>> GetTenantServiceDbConns(string createScriptName, int createScriptVersion) {
            return await Task.Factory.StartNew(() => _tenantServiceDbConnRepository.GetTenantServiceDbConns(createScriptName, createScriptVersion));
        }
    }
}
