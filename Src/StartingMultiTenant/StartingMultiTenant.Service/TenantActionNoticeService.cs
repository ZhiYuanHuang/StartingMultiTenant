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

        public void PublishTenantCreating(string tenantDomain,string tenantIdentifier,int rate,string tip) {
            TenantActionInfoDto<TenantProcessingDetailDto> infoDto = new TenantActionInfoDto<TenantProcessingDetailDto>(tenantDomain, tenantIdentifier, TenantActionTypeEnum.Creating) {
                DetailInfo = new TenantProcessingDetailDto() { Rate=rate,Tip=tip}
            };
            NoticeTenantAction(infoDto);
        }

        public void PublishTenantCreated(string tenantDomain, string tenantIdentifier,bool success) {
            TenantActionInfoDto infoDto = new TenantActionInfoDto(tenantDomain, tenantIdentifier,success? TenantActionTypeEnum.CreatedSuccess:TenantActionTypeEnum.CreatedFailed);
            NoticeTenantAction(infoDto);
        }

        public void PublishTenantStartUpdateSchema(string tenantDomain, string tenantIdentifier,string serviceIdentifier) {
            TenantActionInfoDto<string> infoDto = new TenantActionInfoDto<string>(tenantDomain, tenantIdentifier, TenantActionTypeEnum.StartUpdateDbSchema) { 
                DetailInfo= serviceIdentifier
            };
            NoticeTenantAction(infoDto);
        }


        public void PublishTenantUpdated(string tenantDomain,string tenantIdentifier, string serviceIdentifier, bool success) {
            TenantActionInfoDto<string> infoDto = new TenantActionInfoDto<string>(tenantDomain, tenantIdentifier, success ? TenantActionTypeEnum.UpdatedDbSchemaSuccess : TenantActionTypeEnum.UpdatedDbSchemaFailed) { 
                DetailInfo=serviceIdentifier
            };
            NoticeTenantAction(infoDto);
        }

        public void PublishTenantStartExchange(string tenantDomain,string tenantIdentifier, string serviceIdentifier) {
            TenantActionInfoDto<string> infoDto = new TenantActionInfoDto<string>(tenantDomain, tenantIdentifier, TenantActionTypeEnum.StartExchangeDbServer) { 
                DetailInfo=serviceIdentifier
            };
            NoticeTenantAction(infoDto);
        }

        public void PublishTenantExchanged(string tenantDomain,string tenantIdentifier, string serviceIdentifier, bool success) {
            TenantActionInfoDto<string> infoDto = new TenantActionInfoDto<string>(tenantDomain, tenantIdentifier, success ? TenantActionTypeEnum.ExchangedDbServerSuccess : TenantActionTypeEnum.ExchangedDbServerFailed) { 
                DetailInfo=serviceIdentifier
            };
            NoticeTenantAction(infoDto);
        }

        public void PublishTenantStartDelete(string tenantDomain, string tenantIdentifier) {
            TenantActionInfoDto tenantActionInfoDto = new TenantActionInfoDto(tenantDomain, tenantIdentifier, TenantActionTypeEnum.StartDelete);
            NoticeTenantAction(tenantActionInfoDto);
        }

        public void PublishTenantDeleting(string tenantDomain, string tenantIdentifier, int rate, string tip) {
            TenantActionInfoDto<TenantProcessingDetailDto> infoDto = new TenantActionInfoDto<TenantProcessingDetailDto>(tenantDomain, tenantIdentifier, TenantActionTypeEnum.Deleting) {
                DetailInfo = new TenantProcessingDetailDto() { Rate = rate, Tip = tip }
            };
            NoticeTenantAction(infoDto);
        }

        public void PublishTenantDeleted(string tenantDomain, string tenantIdentifier, bool success) {
            TenantActionInfoDto infoDto = new TenantActionInfoDto(tenantDomain, tenantIdentifier, success ? TenantActionTypeEnum.DeletedSuccess : TenantActionTypeEnum.DeletedFailed);
            NoticeTenantAction(infoDto);
        }

        public void PublishTenantManualModify(string tenantDomain=null,string tenantIdentifier = null) {
            TenantActionInfoDto infoDto = null;
            if (string.IsNullOrEmpty(tenantDomain)) {
                infoDto = new TenantActionInfoDto(null,null,TenantActionTypeEnum.ManualAllModify);
            } else {
                infoDto = new TenantActionInfoDto(tenantDomain, tenantIdentifier, TenantActionTypeEnum.ManualModify);
            }
            NoticeTenantAction(infoDto);
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
