using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Model.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Service
{
    public interface IQueueNotice
    {
        void Init(string connStr);
        Task NoticeTenantAction(TenantActionInfoDto tenantActionInfo);
        Task NoticeTenantAction<T>(TenantActionInfoDto<T> tenantActionInfo);
    }

    public class QueueNoticeFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        public QueueNoticeFactory(ILoggerFactory loggerFactory) { 
            _loggerFactory = loggerFactory;
        }
        public IQueueNotice CreateQueueNotice(QueueNoticeEnum queueNoticeEnum) {
            IQueueNotice queueNotice = null;
            
            switch (queueNoticeEnum) {
                case QueueNoticeEnum.Redis: {
                        var logger = _loggerFactory.CreateLogger<RedisQueueNotice>();
                        queueNotice = new RedisQueueNotice(logger);
                    }
                    break;
                case QueueNoticeEnum.RabbitMQ: {
                        var logger = _loggerFactory.CreateLogger<RabbitQueueNotice>();
                        queueNotice = new RabbitQueueNotice(logger);
                    }
                    break;
                default:
                    queueNotice = null;
                    break;
            }

            return queueNotice;
        }
    }
}
