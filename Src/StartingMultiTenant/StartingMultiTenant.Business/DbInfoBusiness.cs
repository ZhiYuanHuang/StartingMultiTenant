using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StartingMultiTenant.Business
{
    public class DbInfoBusiness : BaseBusiness<DbInfoModel>
    {
        private readonly DbInfoRepository _dbInfoRepo;
        public DbInfoBusiness(DbInfoRepository repository, ILogger<DbInfoBusiness> logger) : base(repository, logger) {
            _dbInfoRepo = repository;
        }

        public PagingData<DbInfoModel> GetPage(string name, string identifier,Int64? serviceInfoId, int pageSize, int pageIndex) {
            return _dbInfoRepo.GetPage(pageSize, pageIndex, name, identifier, serviceInfoId);
        }

        public List<DbInfoModel> GetDbInfosByService(Int64 serviceInfoId) {
            return _dbInfoRepo.GetDbInfosByService(serviceInfoId);
        }

        public Dictionary<string, Int64> GetByServiceInfo(List<string> identifierList) {
            var models = _dbInfoRepo.GetByIdentifier(identifierList);
            return models.ToDictionary(x => x.Identifier, x => x.Id);
        }
    }
}
