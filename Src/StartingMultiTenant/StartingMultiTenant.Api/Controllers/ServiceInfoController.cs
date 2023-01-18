using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Repository;
using StartingMultiTenant.Service;
using System.Reflection.Metadata.Ecma335;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthorizePolicyConst.Sys_Policy)]
    public class ServiceInfoController : ControllerBase
    {
        private readonly ServiceInfoBusiness _serviceInfoBusiness;
        public ServiceInfoController(
            ServiceInfoBusiness serviceInfoBusiness) {
            _serviceInfoBusiness= serviceInfoBusiness;
        }

        [HttpGet]
        public AppResponseDto<ServiceInfoModel> Get(Int64 id) {
            var model = _serviceInfoBusiness.Get(id);

            if (model == null) {
                return new AppResponseDto<ServiceInfoModel>(false);
            }

            return new AppResponseDto<ServiceInfoModel>() { Result = model };
        }

        [HttpPost]
        public AppResponseDto<ServiceInfoModel> GetMany(AppRequestDto<List<Int64>> requestDto) {
            var models = _serviceInfoBusiness.Get(requestDto.Data);
            return new AppResponseDto<ServiceInfoModel>() { ResultList = models };
        }

        [HttpPost]
        public AppResponseDto<Int64> Add(AppRequestDto<ServiceInfoModel> requestDto) {

            if (requestDto.Data == null) {
                return new AppResponseDto<Int64>(false);
            }

            bool result= _serviceInfoBusiness.Insert(requestDto.Data,out Int64 id);
            return new AppResponseDto<Int64>(result) { Result=id};
        }

        [HttpPut]
        public AppResponseDto<ServiceInfoModel> Update(AppRequestDto<ServiceInfoModel> requestDto) {

            if (requestDto.Data == null || requestDto.Data.Id<=0) {
                return new AppResponseDto<ServiceInfoModel>(false);
            }

            bool result = _serviceInfoBusiness.Update(requestDto.Data);
            return new AppResponseDto<ServiceInfoModel>(result) {Result=requestDto.Data};
        }

        [HttpDelete]
        public AppResponseDto Delete(AppRequestDto<ServiceInfoModel> requestDto) {

            if (requestDto.Data == null || requestDto.Data.Id <= 0) {
                return new AppResponseDto(false);
            }

            var result = _serviceInfoBusiness.Delete(requestDto.Data.Id);
            return new AppResponseDto(result.Item1) { ErrorMsg=result.Item2};
        }

        [HttpDelete]
        public AppResponseDto<List<Int64>> DeleteMany(AppRequestDto<List<Int64>> requestDto) {
            var resultTuple = _serviceInfoBusiness.DeleteMany(requestDto.Data);
            return new AppResponseDto<List<Int64>>(resultTuple.Item1) {
                Result = resultTuple.Item2,
                ErrorMsg = resultTuple.Item3
            };
        }

        [HttpGet]
        public AppResponseDto<ServiceInfoModel> GetAll() {
            var result= _serviceInfoBusiness.GetAll();
            return new AppResponseDto<ServiceInfoModel>() { ResultList=result};
        }

        [HttpPost]
        public AppResponseDto<PagingData<ServiceInfoModel>> GetList(AppRequestDto<PagingParam<Dictionary<string, string>>> requestDto) {
            string name = null;
            if (requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("name") && !string.IsNullOrEmpty(requestDto.Data.Filter["name"])) {
                name = requestDto.Data.Filter["name"];
            }
            string identifier = null;
            if (requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("identifier") && !string.IsNullOrEmpty(requestDto.Data.Filter["identifier"])) {
                identifier = requestDto.Data.Filter["identifier"];
            }
            var list = _serviceInfoBusiness.GetPage(name, identifier, requestDto.Data.PageSize, requestDto.Data.PageIndex);
            return new AppResponseDto<PagingData<ServiceInfoModel>>() { Result = list };
        }
    }
}
