using StackExchange.Redis;
using StartingMultiTenant.Model.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Service
{
    internal class RedisQueueNotice : IQueueNotice
    {
        private string _connStr;

        private readonly object _pubLockObj;
        private ConnectionMultiplexer _pubConnection = null;
        private IDatabase _pubDb;

        private const string _Action_Channel = "tenant_action_channel";

        public RedisQueueNotice() {
            _pubLockObj = new object();
        }

        public void Init(string connStr) {
            _connStr = connStr;
        }

        public async Task NoticeTenantAction(TenantActionInfoDto tenantActionInfo) {
            checkConnection();
            await _pubDb.PublishAsync(_Action_Channel, Newtonsoft.Json.JsonConvert.SerializeObject(tenantActionInfo));
        }

        public async Task NoticeTenantAction<T>(TenantActionInfoDto<T> tenantActionInfo) {
            checkConnection();
            await _pubDb.PublishAsync(_Action_Channel, Newtonsoft.Json.JsonConvert.SerializeObject(tenantActionInfo));
        }
        private void checkConnection() {
            if (_pubConnection == null) {
                lock (_pubLockObj) {
                    if (_pubConnection == null) {
                        _pubConnection = ConnectionMultiplexer.Connect(_connStr);
                        _pubDb = _pubConnection.GetDatabase();
                    }
                }
            }
        }

    }
}
