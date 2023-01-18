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
    public class ServiceInfoBusiness : BaseBusiness<ServiceInfoModel>
    {
        private readonly ServiceInfoRepository _serviceInfoRepo;
        private readonly DbInfoRepository _dbInfoRepository;
        public ServiceInfoBusiness(ServiceInfoRepository repository,
            DbInfoRepository dbInfoRepository
            , ILogger<ServiceInfoBusiness> logger)
            : base(repository, logger) {
            _serviceInfoRepo= repository;
            _dbInfoRepository= dbInfoRepository;
        }

        public PagingData<ServiceInfoModel> GetPage(string name,string identifier, int pageSize, int pageIndex) {
            return _serviceInfoRepo.GetPage(pageSize,pageIndex,name,identifier);
        }

        public ServiceAndDbInfoDto GetByServiceInfo(Int64 serviceInfoId) {
            var model = _serviceInfoRepo.GetEntityById(serviceInfoId);
            if (model == null) {
                return null;
            }

            var dbInfos= _dbInfoRepository.GetDbInfosByService(serviceInfoId);
            var dbInfoDtos= dbInfos.Select(x => new DbInfoDto() { DbInfoId = x.Id, DbIdentifier = x.Identifier }).ToList();
            return new ServiceAndDbInfoDto() { 
                ServiceInfoId = model.Id,
                ServiceIdentifier=model.Identifier,
                DbInfos=dbInfoDtos,
            };
        }

        public Dictionary<string,Int64> GetByServiceInfo(List<string> identifierList) {
            var models= _serviceInfoRepo.GetByIdentifier(identifierList);
            return models.ToDictionary(x => x.Identifier, x => x.Id);
        }
    }
}
