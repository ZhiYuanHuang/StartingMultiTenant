using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Repository;
using StartingMultiTenant.Service;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthorizePolicyConst.Sys_Policy)]
    public class DbConnController : ControllerBase
    {
        private readonly SingleTenantService _singleTenantService;
        private readonly EncryptService _encryptService;
        private readonly HistoryTenantServiceDbConnRepository _historyTenantServiceDbConnRepo;
        private readonly DbServerBusiness _dbServerBusiness;
        public DbConnController(SingleTenantService singleTenantService,
            EncryptService encryptService,
            DbServerBusiness dbServerBusiness,
            HistoryTenantServiceDbConnRepository historyTenantServiceDbConnRepo) { 
            _singleTenantService = singleTenantService;
            _encryptService = encryptService;
            _historyTenantServiceDbConnRepo= historyTenantServiceDbConnRepo;
            _dbServerBusiness = dbServerBusiness;
        }

        [HttpGet]
        public AppResponseDto<ServiceDbConnDto> GetHistoryDbConn(Int64 dbConnId) {
            var list= _historyTenantServiceDbConnRepo.GetByDbConn(dbConnId);

            List<ServiceDbConnDto> serviceDbConnsDtos = new List<ServiceDbConnDto>();
            if (list.Any()) {
                foreach(var dbConn in list) {
                    ServiceDbConnDto dbConnDto = new ServiceDbConnDto() { 
                        Id=dbConn.Id,
                        MajorVersion=dbConn.CreateScriptVersion,
                        MinorVersion=dbConn.CurSchemaVersion,
                        ActionType=dbConn.ActionType,
                    };

                    dbConnDto.DecryptDbConn = _encryptService.Decrypt_DbConn(dbConn.EncryptedConnStr);
                    serviceDbConnsDtos.Add(dbConnDto);
                }
            }

            return new AppResponseDto<ServiceDbConnDto>() { 
                ResultList=serviceDbConnsDtos
            };

        }

        [HttpPost]
        public async Task<AppResponseDto> ExchangeDbServer(AppRequestDto<DbConnAndDbserverIdDto> requestDto) {
            if (requestDto.Data == null) {
                return new AppResponseDto(false);
            }

            bool sameDbType = _dbServerBusiness.CheckSameTypeDbByConn(requestDto.Data.DbConnId, requestDto.Data.DbServerId, out TenantServiceDbConnModel dbConn, out DbServerModel toExchangeDbServer);
            if (!sameDbType) {
                return new AppResponseDto(false);
            }
            var result = await _singleTenantService.ExchangeTenantDbServer(dbConn, toExchangeDbServer);

            return new AppResponseDto(result);
        }

    }
}
