using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Service;
using System.Diagnostics;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
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
        public AppResponseDto AddDbServer(AppRequestDto<DbServerDto> appRequestDto) {
            if (appRequestDto.Data == null) {
                return new AppResponseDto(false);
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

            var result= _dbServerBusiness.Insert(dbServer);
            return new AppResponseDto(result);
        }

        [HttpPost]
        public AppResponseDto DeleteDbServer(AppRequestDto<Int64> requestDto) {
            if (requestDto.Data <= 0) {
                return new AppResponseDto(false);
            }

            var result= _dbServerBusiness.Delete(requestDto.Data);
            return new AppResponseDto(result);
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
