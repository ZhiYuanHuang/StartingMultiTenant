using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Enum;
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

        public List<TenantServiceDbConnModel> GetByTenant(string tenantDomain,string tenantIdentifier) {
            return _tenantServiceDbConnRepository.GetTenantServiceDbConns(tenantDomain,tenantIdentifier);
        }
       
        public async Task<List<TenantServiceDbConnModel>> GetTenantServiceDbConns(long? dbConnId = null) {
            return await Task.Factory.StartNew(() => _tenantServiceDbConnRepository.GetTenantServiceDbConns(dbConnId));
        }

        public List<TenantServiceDbConnModel> GetConnListByDbServer(Int64 dbServerId) {
            return _tenantServiceDbConnRepository.GetConnListByDbServer(dbServerId);
        }

        public bool InsertOrUpdate(TenantServiceDbConnModel tenantServiceDbConn) {
            return _tenantServiceDbConnRepository.InsertOrUpdate(tenantServiceDbConn);
        }

        public bool BatchInsertDbConns(List<TenantServiceDbConnModel> dbConnList,bool overrideWhenExisted) {
            bool success = false;

            if (!overrideWhenExisted) {
                return _tenantServiceDbConnRepository.BatchInsertDbConns(dbConnList);
            }

            try {
                _tenantServiceDbConnRepository.BeginTransaction();

                foreach (var dbConn in dbConnList) {
                    var existedDbConn= _tenantServiceDbConnRepository.GetTenantServiceDbConns(dbConn.TenantDomain,dbConn.TenantIdentifier,dbConn.CreateScriptName,dbConn.CreateScriptVersion);

                    if (existedDbConn != null) {
                        _historyTenantServiceDbConnRepo.InsertHistoryDbConn(existedDbConn, DbConnActionTypeEnum.CreateOverride);
                    }

                    _tenantServiceDbConnRepository.InsertOrUpdate(dbConn);
                }

                _tenantServiceDbConnRepository.CommitTransaction();
                success = true;
            }
            catch(Exception ex) {
                success= false;
                _logger.LogError(ex.Message);
                _tenantServiceDbConnRepository.RollbackTransaction();
            }

            return success;
        }

        public bool ExchangeToAnotherDbServer(TenantServiceDbConnModel toUpdateDbConn, Int64 newDbServerId, string newEncryptedConnStr) {

            bool success = false;
            try {
                _tenantServiceDbConnRepository.BeginTransaction();

                _historyTenantServiceDbConnRepo.InsertHistoryDbConn(toUpdateDbConn,DbConnActionTypeEnum.MigrateOverride);

                toUpdateDbConn.DbServerId= newDbServerId;
                toUpdateDbConn.EncryptedConnStr = newEncryptedConnStr;

                _tenantServiceDbConnRepository.InsertOrUpdate(toUpdateDbConn);

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
