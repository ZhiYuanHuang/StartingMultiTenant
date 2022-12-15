using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Enum
{
    public enum TenantActionTypeEnum
    {
        StartCreate = 0,
        Creating = 1,
        CreatedSuccess = 2,
        CreatedFailed = 3,

        StartUpdateDbSchema = 4,
        UpdatingDbSchema = 5,
        UpdatedDbSchemaSuccess = 6,
        UpdatedDbSchemaFailed = 7,

        StartExchangeDbServer = 8,
        ExchangingDbServer = 9,
        ExchangedDbServerSuccess = 10,
        ExchangedDbServerFailed = 11,

        StartDelete = 12,
        Deleting = 13,
        DeletedSuccess = 14,
        DeletedFailed = 15,
    }
}
