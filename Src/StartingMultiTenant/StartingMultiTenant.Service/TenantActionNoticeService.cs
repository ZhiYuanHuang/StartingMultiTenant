using Microsoft.Extensions.Configuration;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Model.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Service
{
    public class TenantActionNoticeService
    {
        private readonly IQueueNotice _queueNotice;
        private readonly bool _enableNotice;
        private readonly int _queueType;
        public TenantActionNoticeService(QueueNoticeFactory  queueNoticeFactory,IConfiguration configuration) {
            bool.TryParse(configuration["QueueNotice:Enable"], out _enableNotice);
            if (_enableNotice) {
                if (int.TryParse(configuration["QueueNotice:QueueType"],out _queueType)) {
                    var tmpQueueNotice= queueNoticeFactory.CreateQueueNotice((QueueNoticeEnum)_queueType);
                    if (tmpQueueNotice == null) {
                        _enableNotice = false;

                    } else {
                        _queueNotice = tmpQueueNotice;
                        _queueNotice.Init(configuration["QueueNotice:QueueConn"]);
                    }
                }
            }
        }

        public void PublishTenantStartCreate(string tenantDomain,string tenantIdentifier) {
            TenantActionInfoDto tenantActionInfoDto = new TenantActionInfoDto(tenantDomain, tenantIdentifier, (int)TenantActionTypeEnum.StartCreate);
            NoticeTenantAction(tenantActionInfoDto);

        }

        private void NoticeTenantAction(TenantActionInfoDto tenantActionInfo) {
            if (!_enableNotice) {
                return;
            }

            _queueNotice.NoticeTenantAction(tenantActionInfo).ConfigureAwait(false);
        }

        private void NoticeTenantAction<T>(TenantActionInfoDto<T> tenantActionInfo) {
            if (!_enableNotice) {
                return;
            }

            _queueNotice.NoticeTenantAction(tenantActionInfo).ConfigureAwait(false);
        }
    }
}
