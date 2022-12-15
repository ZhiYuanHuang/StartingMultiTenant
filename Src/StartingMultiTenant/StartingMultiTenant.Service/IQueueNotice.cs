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

    public static class QueueNoticeFactory
    {
        public static IQueueNotice CreateQueueNotice(QueueNoticeEnum queueNoticeEnum) {
            switch (queueNoticeEnum) {
                case QueueNoticeEnum.Redis:
                    break;
                case QueueNoticeEnum.RabbitMQ:
                    break;
                default:
                    break;
            }

            return null;
        }
    }
}
