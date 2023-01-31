using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Service;
using System.Diagnostics;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthorizePolicyConst.Sys_Policy)]
    public class DbServerController : ControllerBase
    {
        private readonly DbServerBusiness _dbServerBusiness;
        private readonly EncryptService _encryptService;
        private readonly MultiTenantService _multiTenantService;
        public DbServerController(DbServerBusiness dbServerBusiness,
            EncryptService encryptService,
            MultiTenantService multiTenantService) {
            _dbServerBusiness = dbServerBusiness;
            _encryptService = encryptService;
            _multiTenantService = multiTenantService;
        }

        [HttpPost]
        public AppResponseDto<Int64> Add(AppRequestDto<DbServerDto> appRequestDto) {
            if (appRequestDto.Data == null) {
                return new AppResponseDto<Int64>(false);
            }

            DbServerDto dbServerDto = appRequestDto.Data;

            DbServerModel dbServer = new DbServerModel() { 
                DbType= dbServerDto.DbType,
                ServerHost=dbServerDto.ServerHost,
                ServerPort=dbServerDto.ServerPort,
                UserName=dbServerDto.UserName,
                EncryptUserpwd=_encryptService.Encrypt_DbserverPwd(dbServerDto.UserPwd),
                CanCreateNew= dbServerDto.CanCreateNew
            };

            var result= _dbServerBusiness.Insert(dbServer,out Int64 id);
            return new AppResponseDto<Int64>(result) { Result=id};
        }

        [HttpDelete]
        public AppResponseDto Delete(AppRequestDto<DbServerModel> requestDto) {
            if (requestDto.Data==null || requestDto.Data.Id <= 0) {
                return new AppResponseDto(false);
            }

            var result= _dbServerBusiness.Delete(requestDto.Data.Id);
            return new AppResponseDto(result.Item1) {
                ErrorMsg=result.Item2
            };
        }

        [HttpDelete]
        public AppResponseDto<List<Int64>> DeleteMany(AppRequestDto<List<Int64>> requestDto) {
            var resultTuple = _dbServerBusiness.DeleteMany(requestDto.Data);
            return new AppResponseDto<List<Int64>>(resultTuple.Item1) {
                Result = resultTuple.Item2,
                ErrorMsg = resultTuple.Item3
            };
        }

        [HttpGet]
        public AppResponseDto<DbServerDto> Get(Int64 id) {
            var model = _dbServerBusiness.Get(id);

            if (model == null) {
                return new AppResponseDto<DbServerDto>(false);
            }

            var dto = ConvertFromModel(model);

            return new AppResponseDto<DbServerDto>() { Result = dto };
        }

        [HttpPost]
        public AppResponseDto<DbServerDto> GetMany(AppRequestDto<List<Int64>> requestDto) {
            var models = _dbServerBusiness.Get(requestDto.Data);

            var dtos = new List<DbServerDto>();
            models.ForEach(x => dtos.Add(ConvertFromModel(x)));

            return new AppResponseDto<DbServerDto>() { ResultList = dtos };
        }

        [HttpPost]
        public AppResponseDto<PagingData<DbServerDto>> GetList(AppRequestDto<PagingParam<Dictionary<string, object>>> requestDto) {
            string serverHost = null;
            if (requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("serverHost") && requestDto.Data.Filter["serverHost"] != null) {
                serverHost = requestDto.Data.Filter["serverHost"].ToString();
            }
            int? dbType=null;
            if(requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("dbType") && requestDto.Data.Filter["dbType"] != null) {
                if (int.TryParse(requestDto.Data.Filter["dbType"].ToString(),out int tmpDbType)) {
                    dbType = tmpDbType;
                }
            }
           
            var list = _dbServerBusiness.GetPage(serverHost,dbType, requestDto.Data.PageSize, requestDto.Data.PageIndex);

            PagingData<DbServerDto> dtoPagingData = new PagingData<DbServerDto>(list.PageSize, list.PageIndex, list.RecordCount, list.Data.Select(x => ConvertFromModel(x)).ToList());
            return new AppResponseDto<PagingData<DbServerDto>>() { Result = dtoPagingData };
        }

        private DbServerDto ConvertFromModel(DbServerModel model) {
            var dto = new DbServerDto() {
                Id=model.Id,
                DbType = model.DbType,
                ServerHost = model.ServerHost,
                ServerPort = model.ServerPort,
                UserName = model.UserName,
                EncryptUserpwd = model.EncryptUserpwd,
                CanCreateNew = model.CanCreateNew
            };
            try {
                dto.UserPwd = _encryptService.Decrypt_DbServerPwd(model.EncryptUserpwd);
            } catch {

            }
            return dto;
        }

        [HttpGet]
        public AppResponseDto<DbServerDto> GetDbServers() {
            var list= _dbServerBusiness.GetDbServers();

            var dtoList = list.Select(x => {
                var dto = new DbServerDto() {
                    Id=x.Id,
                    DbType = x.DbType,
                    ServerHost = x.ServerHost,
                    ServerPort = x.ServerPort,
                    UserName = x.UserName,
                    CanCreateNew = x.CanCreateNew
                };

                if (!string.IsNullOrEmpty(x.EncryptUserpwd)) {
                    try {
                        dto.UserPwd = _encryptService.Decrypt_DbServerPwd(x.EncryptUserpwd);
                    } catch(Exception ex) {

                    }
                }

                return dto;
            }).ToList();

            return new AppResponseDto<DbServerDto>() { 
                ResultList= dtoList
            };
        }

        [HttpPost]
        public async Task<AppResponseDto> ExchangeDbServer(AppRequestDto<DbConnAndDbserverIdDto> requestDto) {
            if (requestDto.Data == null) {
                return new AppResponseDto(false);
            }

            bool sameDbType = _dbServerBusiness.CheckSameTypeDb(requestDto.Data.OldDbServerId,requestDto.Data.DbServerId,out DbServerModel oldDbServer,out DbServerModel newDbServer);

            if (!sameDbType) {
                return new AppResponseDto(false);
            }

            var resultTuple=await _multiTenantService.ExchangeDbServer(oldDbServer,newDbServer);
            if (!resultTuple.Item1) {
                if (resultTuple.Item2 > 0) {
                    return new AppResponseDto(false) {
                        ErrorMsg = $"part success,successCount:{resultTuple.Item2},failureCount:{resultTuple.Item3}"
                    };
                }
                return new AppResponseDto(false);
            }

            return new AppResponseDto(true);
        }

    }
}
