using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using System.Diagnostics;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthorizePolicyConst.Sys_Policy)]
    public class TenantDomainController : ControllerBase
    {
        private readonly TenantDomainBusiness _tenantDomainBusiness;
        public TenantDomainController(TenantDomainBusiness tenantDomainBusiness) {
            _tenantDomainBusiness = tenantDomainBusiness;
        }

        [HttpGet]
        public AppResponseDto<TenantDomainModel> Get(Int64 id) {
            var domain= _tenantDomainBusiness.Get(id);
            if (domain == null) {
                return new AppResponseDto<TenantDomainModel>(false);
            }

            return new AppResponseDto<TenantDomainModel>() { Result=domain};
        }

        [HttpPost]
        public AppResponseDto<TenantDomainModel> GetMany(AppRequestDto<List<Int64>> requestDto) {
            var domains = _tenantDomainBusiness.Get(requestDto.Data);

            return new AppResponseDto<TenantDomainModel>() { ResultList = domains };
        }

        [HttpPost]
        public AppResponseDto<Int64> Add(AppRequestDto<TenantDomainModel> requestDto) {
            if (requestDto?.Data == null) {
                return new AppResponseDto<Int64>(false);
            }

            bool result= _tenantDomainBusiness.Insert(requestDto.Data,out Int64 id);
            return new AppResponseDto<Int64>(result) { Result=id};
        }

        [HttpDelete]
        public AppResponseDto Delete(AppRequestDto<Int64> requestDto) {
            Tuple<bool,string> resultTuple = _tenantDomainBusiness.Delete(requestDto.Data);
            return new AppResponseDto(resultTuple.Item1) { ErrorMsg=resultTuple.Item2};
        }

        [HttpDelete]
        public AppResponseDto<List<Int64>> DeleteMany(AppRequestDto<List<Int64>> requestDto) {
            var resultTuple = _tenantDomainBusiness.DeleteMany(requestDto.Data);
            return new AppResponseDto<List<Int64>>(resultTuple.Item1) {
                Result = resultTuple.Item2,
                ErrorMsg = resultTuple.Item3
            };
        }

        [HttpPost]
        public AppResponseDto<PagingData<TenantDomainModel>> GetList(AppRequestDto<PagingParam<Dictionary<string,string>>> requestDto) {
            string tenantDomain = null;
            if(requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("tenantDomain") && !string.IsNullOrEmpty(requestDto.Data.Filter["tenantDomain"])) {
                tenantDomain = requestDto.Data.Filter["tenantDomain"];
            }
            var list = _tenantDomainBusiness.GetPage(tenantDomain,requestDto.Data.PageSize,requestDto.Data.PageIndex);
            return new AppResponseDto<PagingData<TenantDomainModel>>() { Result = list };
        }
    }
}
