using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Model.Enum;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Business
{
    public class TenantServiceDbConnBusiness:BaseBusiness<TenantServiceDbConnModel>
    {
        private readonly TenantServiceDbConnRepository _tenantServiceDbConnRepository;
        private readonly HistoryTenantServiceDbConnRepository _historyTenantServiceDbConnRepo;
        private readonly ILogger<TenantServiceDbConnBusiness> _logger;
        public TenantServiceDbConnBusiness(TenantServiceDbConnRepository tenantServiceDbConnRepository,
            HistoryTenantServiceDbConnRepository historyTenantServiceDbConnRepo,
            ILogger<TenantServiceDbConnBusiness> logger):base(tenantServiceDbConnRepository,logger) {
            _tenantServiceDbConnRepository = tenantServiceDbConnRepository;
            _historyTenantServiceDbConnRepo = historyTenantServiceDbConnRepo;
            _logger = logger;
        }

        public PagingData<TenantServiceDbConnDto> GetPage(Func<TenantServiceDbConnModel, TenantServiceDbConnDto> mappingFunc,string tenantDomain, string tenantIdentifier, string serviceIdentifier, int pageSize, int pageIndex) {
            return _tenantServiceDbConnRepository.GetPage(pageSize, pageIndex, mappingFunc, tenantDomain, tenantIdentifier, serviceIdentifier);
        }

        public List<TenantServiceDbConnModel> GetByTenant(string tenantDomain,string tenantIdentifier,string serviceIdentifier=null) {
            return _tenantServiceDbConnRepository.GetConnByTenantAndService(tenantDomain,tenantIdentifier, serviceIdentifier);
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
                    var existedDbConn= _tenantServiceDbConnRepository.GetConnByTenantAndCreateScript(dbConn.TenantDomain,dbConn.TenantIdentifier,dbConn.CreateScriptName);

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

        public List<HistoryTenantServiceDbConnModel> GetHistoryConnsByDbConn(Int64 dbConnId) {
            return _historyTenantServiceDbConnRepo.GetByDbConn(dbConnId);
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
