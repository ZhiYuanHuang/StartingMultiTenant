using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Repository;
using StartingMultiTenant.Service;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ExternalDbConnController : ControllerBase
    {
        private readonly ExternalTenantServiceDbConnRepository _externalDbConnRepo;
        private readonly EncryptService _encryptService;
        public ExternalDbConnController(ExternalTenantServiceDbConnRepository externalDbConnRepo,
            EncryptService encryptService) {
            _externalDbConnRepo= externalDbConnRepo;
            _encryptService= encryptService;
        }

        [HttpPost]
        public AppResponseDto AddOrUpdate(AppRequestDto<ExternalTenantServiceDbConnDto> requestDto) {
            if (requestDto?.Data == null || string.IsNullOrEmpty(requestDto.Data.DbConnStr)) {
                return new AppResponseDto(false);
            }

            requestDto.Data.EncryptedConnStr = _encryptService.Encrypt_DbConn(requestDto.Data.DbConnStr);

            bool result = false;
            try {
                result= _externalDbConnRepo.InsertOrUpdate(requestDto.Data);
            }
            catch(Exception ex) {
                result= true;
            }

            return new AppResponseDto(result);
            
        }

        [HttpPost]
        public AppResponseDto Delete(AppRequestDto<Int64> requestDto) {
            if (requestDto?.Data==null || requestDto.Data<=0) {
                return new AppResponseDto(false);
            }

            bool result = _externalDbConnRepo.Delete(requestDto.Data);
            return new AppResponseDto(result);
        }

        [HttpGet]
        public AppResponseDto<ExternalTenantServiceDbConnDto> Get(string tenantDomain,string tenantIdentifier) {
            var list= _externalDbConnRepo.GetByTenantAndService(tenantDomain, tenantIdentifier);
            var dtoList = list.OrderBy(x => x.ServiceIdentifier).ThenBy(x => x.DbIdentifier).Select(x => {
                var dto = new ExternalTenantServiceDbConnDto() {
                    Id = x.Id,
                    TenantDomain = x.TenantDomain,
                    TenantIdentifier = x.TenantIdentifier,
                    ServiceIdentifier = x.ServiceIdentifier,
                    DbIdentifier = x.DbIdentifier,
                };

                if (!string.IsNullOrEmpty(x.EncryptedConnStr)) {
                    try {
                        dto.DbConnStr = _encryptService.Decrypt_DbConn(x.EncryptedConnStr);
                    } catch { 
                    }
                }

                if (!string.IsNullOrEmpty(x.OverrideEncryptedConnStr)) {
                    try {
                        dto.OverrideDbConnStr = _encryptService.Decrypt_DbConn(x.OverrideEncryptedConnStr);
                    } catch {
                    }
                }

                return dto;
            }).ToList();
            return new AppResponseDto<ExternalTenantServiceDbConnDto>() { ResultList=dtoList};
        }
    }
}
