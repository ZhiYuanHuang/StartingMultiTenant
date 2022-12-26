using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Repository;
using StartingMultiTenant.Service;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DbConnController : ControllerBase
    {
        private readonly SingleTenantService _singleTenantService;
        private readonly EncryptService _encryptService;
        private readonly HistoryTenantServiceDbConnRepository _historyTenantServiceDbConnRepo;
        public DbConnController(SingleTenantService singleTenantService,
            EncryptService encryptService,
            HistoryTenantServiceDbConnRepository historyTenantServiceDbConnRepo) { 
            _singleTenantService = singleTenantService;
            _encryptService = encryptService;
            _historyTenantServiceDbConnRepo= historyTenantServiceDbConnRepo;
        }

        [HttpGet]
        public AppResponseDto<ServiceDbConnsDto> GetHistoryDbConn(Int64 dbConnId) {
            var list= _historyTenantServiceDbConnRepo.GetByDbConn(dbConnId);

            List<ServiceDbConnsDto> serviceDbConnsDtos = new List<ServiceDbConnsDto>();
            if (list.Any()) {
                foreach(var dbConn in list) {
                    ServiceDbConnsDto dbConnDto = new ServiceDbConnsDto() { 
                        Id=dbConn.Id,
                        MajorVersion=dbConn.CreateScriptVersion,
                        MinorVersion=dbConn.CurSchemaVersion,
                        ActionType=dbConn.ActionType,
                    };

                    dbConnDto.DecryptDbConn = _encryptService.Decrypt_DbConn(dbConn.EncryptedConnStr);
                    serviceDbConnsDtos.Add(dbConnDto);
                }
            }

            return new AppResponseDto<ServiceDbConnsDto>() { 
                ResultList=serviceDbConnsDtos
            };

        }

        [HttpPost]
        public async Task<AppResponseDto> ExchangeDbServer(AppRequestDto<DbConnAndDbserverIdDto> requestDto) {
            if (requestDto.Data == null) {
                return new AppResponseDto(false);
            }

            var result = await _singleTenantService.ExchangeTenantConnDb(requestDto.Data.DbConnId, requestDto.Data.DbServerId);

            return new AppResponseDto(result);
        }

    }
}
