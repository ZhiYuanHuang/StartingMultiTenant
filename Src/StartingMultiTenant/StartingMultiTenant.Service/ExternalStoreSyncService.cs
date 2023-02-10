using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace StartingMultiTenant.Service
{
    public class ExternalStoreSyncService
    {
        private readonly List<ExternalStoreDataDto> _externalStoreList;
        private readonly TenantIdentifierBusiness _tenantIdentifierBusiness;
        private readonly ExternalStoreFactory _externalStoreFactory;
        private readonly ILogger<ExternalStoreSyncService> _logger;
        private readonly EncryptService _encryptService;
        private readonly TenantDomainBusiness _tenantDomainBusiness;

        private const int BatchHandleTenantCount = 100;

        public ExternalStoreSyncService(ExternalStoreFactory externalStoreFactory,
            TenantIdentifierBusiness tenantIdentifierBusiness,
            IConfiguration configuration,
            EncryptService encryptService,
            TenantDomainBusiness tenantDomainBusiness,
            ILogger<ExternalStoreSyncService> logger) {
            _logger = logger;
            _externalStoreFactory= externalStoreFactory;
            _tenantIdentifierBusiness = tenantIdentifierBusiness;
            _encryptService = encryptService;
            _tenantDomainBusiness = tenantDomainBusiness;
            var externalStoreJsonDtos= configuration.GetSection("ExternalStores").Get<ExternalStoreJsonDto[]>();
            if(externalStoreJsonDtos!=null && externalStoreJsonDtos.Any()) {
                Type enumType = typeof(StoreTypeEnum);
                _externalStoreList = externalStoreJsonDtos.Where(x => Enum.TryParse(enumType, x.StoreType,out _)).Select(x => {
                    object storeTypeObj= Enum.Parse(enumType, x.StoreType);
                    return new ExternalStoreDataDto() {
                        StoreType = (StoreTypeEnum)storeTypeObj,
                        Conn = x.Conn
                    };
                }).ToList();
            } else {
                _externalStoreList = new List<ExternalStoreDataDto>();
            }
        }

        public async Task<bool> SyncToExternalStore(Int64 id) {
            var tenantDbConnsDto= _tenantIdentifierBusiness.GetTenantsDbConns(id);
            if (tenantDbConnsDto == null) {
                return false;
            }

            tenantDbConnsDto.InnerDbConnList.ForEach(x=>x.DecryptDbConn=_encryptService.Decrypt_DbConn(x.EncryptedConnStr));
            tenantDbConnsDto.ExternalDbConnList.ForEach(x => x.DecryptDbConn = _encryptService.Decrypt_DbConn(x.EncryptedConnStr));

            List<TenantServiceDbConnsDto> list = new List<TenantServiceDbConnsDto>() { tenantDbConnsDto};

            List<Task<bool>> taskList = new List<Task<bool>>();
            foreach(var externalStoreData in _externalStoreList) {
                var task= Task.Factory.StartNew<bool>( (externalStoreDataObj) => {
                    var tmpExternalStoreData = externalStoreData as ExternalStoreDataDto;
                    try {
                        using (var externalStoreExecutor = _externalStoreFactory.Create(externalStoreData)) {
                            externalStoreExecutor.WriteToExternalStore(list);
                        }
                        return true;
                    }catch(Exception ex) {
                        _logger.LogError(ex, $"SyncToExternalStore {tmpExternalStoreData.StoreType.ToString()} {id} error");
                        return false;
                    }
                }, externalStoreData);
                taskList.Add(task);
            }
            var results= await Task.WhenAll(taskList.ToArray());
            foreach(var result in results) {
                if (!result) {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> SyncToExternalStore() {
            List<IExternalStoreExecutor> storeExecutors = await createConnectedExecutors();
            if (!storeExecutors.Any()) {
                return false;
            }

            await SyncToExternalStore(storeExecutors);
            return true;
        }

        public async Task SyncToExternalStore(List<IExternalStoreExecutor> storeExecutors) {
            var domainList= _tenantDomainBusiness.GetAll().OrderBy(x=>x.TenantDomain);
            Func<string, string> decryptDbConnFunc = (encryptConn) =>  _encryptService.Decrypt_DbConn(encryptConn);

            try {

                foreach (var domain in domainList) {
                    string tenantDomain = domain.TenantDomain;

                    var firstPage = _tenantIdentifierBusiness.GetPage(tenantDomain, BatchHandleTenantCount, 0);

                    if (firstPage.RecordCount == 0) {
                        continue;
                    }

                    var tmpPage = firstPage;
                    int pageCount = firstPage.PageCount;
                    int pageIndex = 0;

                    do {
                        List<Int64> ids = tmpPage.Data.Select(x => x.Id).ToList();

                        var tenantsDbConns = _tenantIdentifierBusiness.GetTenantsDbConns(ids, decryptDbConnFunc);
                        foreach(var storeExecutor in storeExecutors) {
                            SyncToExternalStore(storeExecutor, tenantsDbConns);
                        }

                        pageIndex++;
                        if (pageIndex == pageCount) {
                            break;
                        }
                        firstPage = _tenantIdentifierBusiness.GetPage(tenantDomain, BatchHandleTenantCount, 0);
                    } while (pageIndex < pageCount);
                }
            }catch(Exception ex) {
                _logger.LogError(ex,$"SyncToExternalStore error");
            } finally {
                storeExecutors.ForEach(x => x.Dispose());
            }
        }

        private void SyncToExternalStore(IExternalStoreExecutor storeExecutor,List<TenantServiceDbConnsDto> tenantsDbConns) {
            try {
                storeExecutor.WriteToExternalStore(tenantsDbConns);
            } catch(Exception ex) {
                _logger.LogError(ex, "SyncToExternalStore error");
            }
        }

        private async Task<List<IExternalStoreExecutor>> createConnectedExecutors() {
            return await Task.Factory.StartNew( () => {
                List<IExternalStoreExecutor> executorList = new List<IExternalStoreExecutor>();
                foreach (var externalStoreData in _externalStoreList) {
                    var externalStoreExecutor = _externalStoreFactory.Create(externalStoreData);
                    if(externalStoreExecutor!=null) {
                        if (externalStoreExecutor.IsConnected) {
                            executorList.Add(externalStoreExecutor);
                        } else {
                            externalStoreExecutor.Dispose();
                            externalStoreExecutor = null;
                        }
                    }
                    
                }
                return executorList;
            });
           
        }
    }
}
