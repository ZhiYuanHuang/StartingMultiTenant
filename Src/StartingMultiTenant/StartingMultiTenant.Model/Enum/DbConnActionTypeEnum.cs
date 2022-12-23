using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Enum
{
    public enum DbConnActionTypeEnum
    {
        None = 0,
        CreateOverride=1,
        SchemaUpdateOverride=2,
        MigrateOverride=3,
    }
}
