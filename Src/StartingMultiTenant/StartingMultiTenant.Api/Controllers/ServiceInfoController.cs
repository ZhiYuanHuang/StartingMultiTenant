using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Repository;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ServiceInfoController : ControllerBase
    {
        private readonly ServiceInfoRepository _serviceInfoRepository;
        private readonly DbInfoRepository _dbInfoRepo;
        public ServiceInfoController(ServiceInfoRepository serviceInfoRepository,
            DbInfoRepository dbInfoRepository) {
            _serviceInfoRepository = serviceInfoRepository;
            _dbInfoRepo = dbInfoRepository;
        }

        [HttpPost]
        public AppResponseDto AddServiceInfo(AppRequestDto<ServiceInfoModel> requestDto) {

            if (requestDto.Data == null) {
                return new AppResponseDto(false);
            }

            bool result= _serviceInfoRepository.Insert(requestDto.Data);
            return new AppResponseDto(result);
        }

        [HttpPost]
        public AppResponseDto UpdateServiceInfo(AppRequestDto<ServiceInfoModel> requestDto) {

            if (requestDto.Data == null || requestDto.Data.Id<=0) {
                return new AppResponseDto(false);
            }

            bool result = _serviceInfoRepository.Update(requestDto.Data);
            return new AppResponseDto(result);
        }

        [HttpPost]
        public AppResponseDto DeleteServiceInfo(AppRequestDto<ServiceInfoModel> requestDto) {

            if (requestDto.Data == null || requestDto.Data.Id <= 0) {
                return new AppResponseDto(false);
            }

            bool result = _serviceInfoRepository.Delete(requestDto.Data.Id);
            return new AppResponseDto(result);
        }

        [HttpGet]
        public AppResponseDto<ServiceInfoModel> GetServiceInfos() {
            var result= _serviceInfoRepository.GetEntitiesByQuery();
            return new AppResponseDto<ServiceInfoModel>() { ResultList=result};
        }

        [HttpPost]
        public AppResponseDto AddServiceDbInfo(AppRequestDto<DbInfoModel> requestDto) {
            if(requestDto.Data==null || string.IsNullOrEmpty(requestDto.Data.Identifier) || string.IsNullOrEmpty(requestDto.Data.ServiceIdentifier)) {
                return new AppResponseDto(false);
            }
            bool result= _dbInfoRepo.Insert(requestDto.Data);
            return new AppResponseDto(result);
        }

        [HttpPost]
        public AppResponseDto UpdateServiceDbInfo(AppRequestDto<DbInfoModel> requestDto) {
            if (requestDto.Data == null || requestDto.Data.Id<=0 || string.IsNullOrEmpty(requestDto.Data.Identifier) || string.IsNullOrEmpty(requestDto.Data.ServiceIdentifier)) {
                return new AppResponseDto(false);
            }
            bool result = _dbInfoRepo.Update(requestDto.Data);
            return new AppResponseDto(result);
        }

        [HttpPost]
        public AppResponseDto DeleteServiceDbInfo(AppRequestDto<DbInfoModel> requestDto) {

            if (requestDto.Data == null || requestDto.Data.Id <= 0) {
                return new AppResponseDto(false);
            }

            bool result = _dbInfoRepo.Delete(requestDto.Data.Id);
            return new AppResponseDto(result);
        }

        [HttpGet]
        public AppResponseDto<DbInfoModel> GetDbInfosByService(string serviceInfo) {
            if (string.IsNullOrEmpty(serviceInfo)) {
                return new AppResponseDto<DbInfoModel>() { ResultList=new List<DbInfoModel>()};
            }

            var list= _dbInfoRepo.GetDbInfosByService(serviceInfo);
            return new AppResponseDto<DbInfoModel>() { ResultList=list};
        }
    }
}
