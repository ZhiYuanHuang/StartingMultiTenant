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
    public interface IDbServerBusiness {
        Task<List<DbServerModel>> GetDbServers(Int64? dbServerId=null);
    }
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

        public async Task<List<DbServerModel>> GetDbServers(Int64? dbServerId=null) {
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
    }
}
