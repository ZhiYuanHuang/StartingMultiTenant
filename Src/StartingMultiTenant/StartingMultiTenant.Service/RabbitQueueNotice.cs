using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using StackExchange.Redis;
using StartingMultiTenant.Model.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Service
{
    public class RabbitQueueNotice : IQueueNotice
    {
        private string _connStr;

        private string _host;
        private int _port;
        private string _userName;
        private string _password;

        private readonly object _lockObj = new object();
        private IConnection _conn;

        private const string Exchange_Name = "tenant_action_exchange";

        private readonly ILogger<RabbitQueueNotice> _logger;
        public RabbitQueueNotice(ILogger<RabbitQueueNotice> logger) {
            _lockObj = new object();
            _logger = logger;
        }

        public void Init(string connStr) {
            _connStr = connStr;
            if (!string.IsNullOrEmpty(connStr)) {
                var arr = connStr.Split(';');
                if (arr.Length > 0) {
                    for(var i = 0; i < arr.Length; i++) {
                        if (i == 0) {
                            _host = arr[i];
                        }
                        else if (i == 1) {
                            int.TryParse(arr[i],out _port);
                        }else if (i == 2) {
                            _userName = arr[i];
                        }
                        else if (i == 3) {
                            _password = arr[i];
                        }
                    }
                }
            }
        }


        public async Task NoticeTenantAction(TenantActionInfoDto tenantActionInfo) {
            bool connected= await checkConnectionAsync();
            if (!connected) {
                return;
            }
            await Task.Factory.StartNew(() => {
                try {
                    var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(tenantActionInfo);
                    var body = Encoding.UTF8.GetBytes(jsonContent);
                    using (var channel = _conn.CreateModel()) {
                        channel.ExchangeDeclare(Exchange_Name, ExchangeType.Fanout, true, false);
                        channel.BasicPublish(Exchange_Name, "", null, body);
                    }
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "rabbit queue push error");
                    Exception exception = new Exception("rabbit queue push error", ex);
                    throw exception;
                }
               
            });
            
        }

        public async Task NoticeTenantAction<T>(TenantActionInfoDto<T> tenantActionInfo) {
            bool connected = await checkConnectionAsync();
            if (!connected) {
                return;
            }
            await Task.Factory.StartNew(() => {
               
                try {
                    
                    var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(tenantActionInfo);
                    var body = Encoding.UTF8.GetBytes(jsonContent);

                    using (var channel = _conn.CreateModel()) {
                        channel.ExchangeDeclare(Exchange_Name, ExchangeType.Fanout, true, false);
                        channel.BasicPublish(Exchange_Name, "", null, body);
                    }
                } catch (Exception ex) {
                    _logger.LogError(ex, "rabbit queue push error");
                    Exception exception = new Exception("rabbit queue push error", ex);
                    throw exception;
                }
                
            });
        }

        private async Task<bool> checkConnectionAsync(int timeoutMilliSec=1000*10) {
            Task timeoutTask = Task.Delay(timeoutMilliSec);
            var checkTask= Task.Factory.StartNew<bool>(() => {
                try {
                    return checkConnection();

                } catch {
                    return false;
                }
            });

            var completeTask= await Task.WhenAny(timeoutTask, checkTask);
            if (completeTask == timeoutTask) {
                return false;
            }
            return checkTask.Result;
        }

        private bool checkConnection() {
            if (_conn == null) {
                lock (_lockObj) {
                    if (_conn == null) {
                        ConnectionFactory connectionFactory= new ConnectionFactory();
                        connectionFactory.HostName= _host;
                        if (_port > 0) {
                            connectionFactory.Port = _port;
                        }
                        if (!string.IsNullOrEmpty(_userName)) {
                            connectionFactory.UserName = _userName;
                        }
                        if(!string.IsNullOrEmpty(_password)) {
                            connectionFactory.Password = _password;
                        }
                        _conn=connectionFactory.CreateConnection();
                    }
                }
            }
            
            return _conn?.IsOpen??false;
        }
    }
}
