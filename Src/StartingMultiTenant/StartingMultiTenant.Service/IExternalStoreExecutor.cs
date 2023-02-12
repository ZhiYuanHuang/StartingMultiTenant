using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Model.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Service
{
    public interface IExternalStoreExecutor:IDisposable
    {
        bool IsConnected { get; }
        void WriteToExternalStore(List<TenantServiceDbConnsDto> tenantServiceDbConns);
    }

    public class ExternalStoreFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        public ExternalStoreFactory(ILoggerFactory loggerFactory) {
            _loggerFactory = loggerFactory;
        }
        public IExternalStoreExecutor Create(ExternalStoreDataDto externalStoreData) {
            IExternalStoreExecutor externalStore = null;

            //_configuration.GetSection("ExternalStores").Get<ExternalStoreJsonDto[]>();

            switch (externalStoreData.StoreType) {
                case StoreTypeEnum.Redis: {
                        var logger = _loggerFactory.CreateLogger<RedisExternalStore>();
                        externalStore = new RedisExternalStore(externalStoreData.Conn, logger);
                    }
                    break;
                case StoreTypeEnum.k8s_secret: {
                        var logger = _loggerFactory.CreateLogger<K8sSecretExternalStore>();
                        externalStore = new K8sSecretExternalStore(externalStoreData.ConfigFilePath,externalStoreData.K8sNamespace,logger);
                        break;
                    }
                default:
                    externalStore = null;
                    break;
            }

            return externalStore;
        }
    }
}
