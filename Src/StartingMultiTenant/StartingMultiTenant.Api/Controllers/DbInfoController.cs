using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthorizePolicyConst.Sys_Policy)]
    public class DbInfoController : ControllerBase
    {
        private readonly DbInfoBusiness _dbInfoBusiness;
        public DbInfoController(DbInfoBusiness dbInfoBusiness) { 
            _dbInfoBusiness = dbInfoBusiness;
        }

        [HttpGet]
        public AppResponseDto<DbInfoModel> Get(Int64 id) {
            var model = _dbInfoBusiness.Get(id);

            if (model == null) {
                return new AppResponseDto<DbInfoModel>(false);
            }

            return new AppResponseDto<DbInfoModel>() { Result = model };
        }

        [HttpGet]
        public AppResponseDto<DbInfoModel> GetMany(AppRequestDto<List<Int64>> requestDto) {
            var models = _dbInfoBusiness.Get(requestDto.Data);
            return new AppResponseDto<DbInfoModel>() { ResultList = models };
        }

        [HttpPost]
        public AppResponseDto<Int64> Add(AppRequestDto<DbInfoModel> requestDto) {

            if (requestDto.Data == null) {
                return new AppResponseDto<Int64>(false);
            }

            bool result = _dbInfoBusiness.Insert(requestDto.Data, out Int64 id);
            return new AppResponseDto<Int64>(result) { Result = id };
        }

        [HttpPut]
        public AppResponseDto<DbInfoModel> Update(AppRequestDto<DbInfoModel> requestDto) {

            if (requestDto.Data == null || requestDto.Data.Id <= 0) {
                return new AppResponseDto<DbInfoModel>(false);
            }

            bool result = _dbInfoBusiness.Update(requestDto.Data);
            return new AppResponseDto<DbInfoModel>(result) { Result = requestDto.Data };
        }

        [HttpDelete]
        public AppResponseDto Delete(AppRequestDto<DbInfoModel> requestDto) {

            if (requestDto.Data == null || requestDto.Data.Id <= 0) {
                return new AppResponseDto(false);
            }

            var result = _dbInfoBusiness.Delete(requestDto.Data.Id);
            return new AppResponseDto(result.Item1) { ErrorMsg = result.Item2 };
        }
        [HttpDelete]
        public AppResponseDto<List<Int64>> DeleteMany(AppRequestDto<List<Int64>> requestDto) {
            var resultTuple = _dbInfoBusiness.DeleteMany(requestDto.Data);
            return new AppResponseDto<List<Int64>>(resultTuple.Item1) {
                Result = resultTuple.Item2,
                ErrorMsg = resultTuple.Item3
            };
        }

        [HttpPost]
        public AppResponseDto<PagingData<DbInfoModel>> GetList(AppRequestDto<PagingParam<Dictionary<string, object>>> requestDto) {
            string name = null;
            if (requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("name") && requestDto.Data.Filter["name"]!=null) {
                name = requestDto.Data.Filter["name"].ToString();
            }
            string identifier = null;
            if (requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("identifier") && requestDto.Data.Filter["identifier"] != null) {
                identifier = requestDto.Data.Filter["identifier"].ToString();
            }
            Int64? serviceInfoId = null;
            if (requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("serviceInfoId") && requestDto.Data.Filter["serviceInfoId"] != null) {
                if(Int64.TryParse(requestDto.Data.Filter["serviceInfoId"].ToString(),out Int64 tmpId) ) {
                    serviceInfoId= tmpId;
                }
            }
            var list = _dbInfoBusiness.GetPage(name, identifier,serviceInfoId, requestDto.Data.PageSize, requestDto.Data.PageIndex);
            return new AppResponseDto<PagingData<DbInfoModel>>() { Result = list };
        }

        [HttpGet]
        public AppResponseDto<DbInfoModel> GetDbInfosByService(Int64? serviceInfoId) {
            if (!serviceInfoId.HasValue) {
                return new AppResponseDto<DbInfoModel>() { ResultList = new List<DbInfoModel>() };
            }

            var list = _dbInfoBusiness.GetDbInfosByService(serviceInfoId.Value);
            return new AppResponseDto<DbInfoModel>() { ResultList = list };
        }
    }
}
