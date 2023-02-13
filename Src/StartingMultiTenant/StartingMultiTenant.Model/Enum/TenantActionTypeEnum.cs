using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Enum
{
    public enum TenantActionTypeEnum
    {
        StartCreate = 1,
        Creating = 2,
        CreatedSuccess = 3,
        CreatedFailed = 4,

        StartUpdateDbSchema = 5,
        UpdatingDbSchema = 6,
        UpdatedDbSchemaSuccess = 7,
        UpdatedDbSchemaFailed = 8,

        StartExchangeDbServer = 9,
        ExchangingDbServer = 10,
        ExchangedDbServerSuccess = 11,
        ExchangedDbServerFailed = 12,

        StartDelete = 13,
        Deleting = 14,
        DeletedSuccess = 15,
        DeletedFailed = 16,

        DbConnsModify=17,
        ManualAllClear=18,
        //ManualModify=17,
        //ManualAllModify = 18,
    }
}
