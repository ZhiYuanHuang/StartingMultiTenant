using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Business
{
    public class DbServerBusiness
    {
        private readonly DbServerRepository _dbServerRepository;
        private readonly TenantServiceDbConnRepository _tenantServiceDbConnRepository;
        private readonly ILogger<DbServerBusiness> _logger;

        public DbServerBusiness(DbServerRepository dbServerRepository,
            TenantServiceDbConnRepository tenantServiceDbConnRepository,
            ILogger<DbServerBusiness> logger) {
            _dbServerRepository = dbServerRepository;
            _tenantServiceDbConnRepository=tenantServiceDbConnRepository;
            _logger = logger;
        }

        public List<DbServerModel> GetDbServers(Int64? dbServerId=null) {
            return _dbServerRepository.GetDbServers(dbServerId);
        }

        public bool Insert(DbServerModel dbServer) {
            return _dbServerRepository.Insert(dbServer);
        }

        public bool Delete(Int64 dbServerId) {
            var dbConnList= _tenantServiceDbConnRepository.GetConnListByDbServer(dbServerId);
            if (dbConnList.Any()) {
                _logger.LogError($"dbserver {dbServerId} still has {dbConnList.Count} dbconn refered");
                return false;
            }

            return _dbServerRepository.Delete(dbServerId);
        }

        public bool CheckSameTypeDbByConn(Int64 dbConnId,Int64 dbServerId,out TenantServiceDbConnModel dbConn,out DbServerModel newDbServer) {
            dbConn = null;
            newDbServer = null;

            List<TenantServiceDbConnModel> tenantServiceDbConns = _tenantServiceDbConnRepository.GetTenantServiceDbConns(dbConnId);

            if (!tenantServiceDbConns.Any()) {
                _logger.LogError($"cann't find id {dbConnId} dbconn");
                return false;
            }

            dbConn = tenantServiceDbConns[0];
            Int64 oldDbServerId = dbConn.DbServerId;

            return CheckSameTypeDb(oldDbServerId, dbServerId,out _,out newDbServer);
        }

        public bool CheckSameTypeDb(Int64 oldDbServerId,Int64 newDbServerId,out DbServerModel oldDbServer,out DbServerModel newDbServer) {
            oldDbServer = null;
            newDbServer = null;

            List<DbServerModel> oldDbServers = _dbServerRepository.GetDbServers(oldDbServerId);
            if (!oldDbServers.Any()) {
                _logger.LogError($"cann't find id {oldDbServerId} old dbserver");
                return false;
            }
            oldDbServer = oldDbServers[0];

            List<DbServerModel> newDbServers = _dbServerRepository.GetDbServers(newDbServerId);
            if (!newDbServers.Any()) {
                _logger.LogError($"cann't find id {newDbServerId} new dbserver");
                return false;
            }
            newDbServer = newDbServers[0];

            return oldDbServer.DbType == newDbServer.DbType;
        }
    }
}
