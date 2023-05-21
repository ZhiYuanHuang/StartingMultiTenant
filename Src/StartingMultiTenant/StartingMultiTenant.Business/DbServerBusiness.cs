using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Business
{
    public class DbServerBusiness:BaseBusiness<DbServerModel>
    {
        private readonly DbServerRepository _dbServerRepository;
        private readonly TenantServiceDbConnRepository _tenantServiceDbConnRepository;
        private readonly ILogger<DbServerBusiness> _logger;

        public DbServerBusiness(DbServerRepository dbServerRepository,
            TenantServiceDbConnRepository tenantServiceDbConnRepository,
            ILogger<DbServerBusiness> logger):base(dbServerRepository,logger) {
            _dbServerRepository = dbServerRepository;
            _tenantServiceDbConnRepository=tenantServiceDbConnRepository;
            _logger = logger;
        }

        public List<DbServerModel> GetDbServers(Int64? dbServerId=null) {
            return _dbServerRepository.GetDbServers(dbServerId);
        }

        public override Tuple<bool,string> Delete(Int64 dbServerId) {
            var dbConnList= _tenantServiceDbConnRepository.GetConnListByDbServer(dbServerId);
            if (dbConnList.Any()) {
                _logger.LogError($"dbserver {dbServerId} still has {dbConnList.Count} dbconn refered");
                return Tuple.Create(false,"still has use");
            }

            return base.Delete(dbServerId);
        }

        public PagingData<DbServerModel> GetPage(string serverHost,int? dbType, int pageSize, int pageIndex) {
            return _dbServerRepository.GetPage(pageSize,pageIndex,serverHost,dbType);
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

        public DbServerRefDto StatRef(Int64 dbServerId) {
            var model= Get(dbServerId);

            var statDto= new DbServerRefDto() { DbServerId =dbServerId};
            if (model == null) {
                return statDto;
            }

            var dbConns= _tenantServiceDbConnRepository.GetConnListByDbServer(dbServerId);
            if (!dbConns.Any()) {
                return statDto;
            }

            statDto.DbConnCount = dbConns.Count;
            Dictionary<string, int> domainCountDict = new Dictionary<string, int>();
            Dictionary<string, int> tenantCountDict = new Dictionary<string, int>();
            Dictionary<string, int> serviceCountDict = new Dictionary<string, int>();
            dbConns.ForEach(x => { 
                
                if(!domainCountDict.ContainsKey(x.TenantDomain)) {
                    domainCountDict.Add(x.TenantDomain, 1);
                }
                string tenantKey = $"{x.TenantDomain}_{x.TenantIdentifier}";
                if (!tenantCountDict.ContainsKey(tenantKey)) {
                    tenantCountDict.Add(tenantKey, 1);
                } 

                if (!serviceCountDict.ContainsKey(x.ServiceIdentifier)) {
                    serviceCountDict.Add(x.ServiceIdentifier,1);
                }
            });

            statDto.TenantDomainCount = domainCountDict.Keys.Count;
            statDto.TenantCount = tenantCountDict.Keys.Count;
            statDto.ServiceCount = serviceCountDict.Keys.Count;
            return statDto;
        }
    }
}
