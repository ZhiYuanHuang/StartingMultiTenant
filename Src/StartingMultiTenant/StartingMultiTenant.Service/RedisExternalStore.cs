using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using StackExchange.Redis;
using StartingMultiTenant.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Service
{
    internal class RedisExternalStore : IExternalStoreExecutor
    {
        private bool disposedValue;
        private readonly string _connStr;
        private readonly ILogger<RedisExternalStore> _logger;

        private ConnectionMultiplexer _pubConnection = null;
        private IDatabase _pubDb;

        private const int BatchMaxCount = 30;

        public bool IsConnected { get => _pubConnection?.IsConnected ?? false; }

        public RedisExternalStore(string connStr,
            ILogger<RedisExternalStore> logger) {
            _connStr = connStr;
            _logger = logger;
            _pubConnection = ConnectionMultiplexer.Connect(_connStr);
            _pubDb = _pubConnection.GetDatabase();
        }

        public void WriteToExternalStore(List<TenantServiceDbConnsDto> tenantsServiceDbConns) {

            IBatch batch = null;
            
            int groupCount = 0;
            for (int i = 0; i < tenantsServiceDbConns.Count; i++) {
                var tenantServiceDbConns = tenantsServiceDbConns[i];
                if (batch == null) {
                    batch = _pubDb.CreateBatch();
                }

                List<HashEntry> serviceDbConnList = new List<HashEntry>();
                if(tenantServiceDbConns.InnerDbConnList!=null && tenantServiceDbConns.InnerDbConnList.Any()) {
                    foreach (var innerDbConn in tenantServiceDbConns.InnerDbConnList) {
                        serviceDbConnList.Add(new HashEntry($"Inner_{innerDbConn.ServiceIdentifier}_{innerDbConn.DbIdentifier}", innerDbConn.DecryptDbConn));
                    }
                }

                if (tenantServiceDbConns.ExternalDbConnList != null && tenantServiceDbConns.ExternalDbConnList.Any()) {
                    foreach (var externalDbConn in tenantServiceDbConns.ExternalDbConnList) {
                        serviceDbConnList.Add(new HashEntry($"External_{externalDbConn.ServiceIdentifier}_{externalDbConn.DbIdentifier}", externalDbConn.DecryptDbConn));
                    }
                }

                if (serviceDbConnList.Any()) {
                    batch.HashSetAsync($"{tenantServiceDbConns.TenantDomain}:{tenantServiceDbConns.TenantIdentifier}", serviceDbConnList.ToArray());
                    groupCount++;
                }
                
                if (i == tenantsServiceDbConns.Count - 1) {
                    batch.Execute();
                    
                } else if (i > 0 && groupCount % BatchMaxCount == 0) {
                    batch.Execute();
                    batch = null;
                    groupCount = 0;
                }
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: 释放托管状态(托管对象)
                    _pubConnection?.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~RedisExternalStore()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose() {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
