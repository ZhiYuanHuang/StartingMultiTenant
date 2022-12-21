using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Service;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DbServerController : ControllerBase
    {
        private readonly DbServerBusiness _dbServerBusiness;
        private readonly EncryptService _encryptService;
        public DbServerController(DbServerBusiness dbServerBusiness,
            EncryptService encryptService) {
            _dbServerBusiness = dbServerBusiness;
            _encryptService = encryptService;
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
    }
}
