using StartingMultiTenant.Model.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class ExternalStoreJsonDto
    {
        public string StoreType { get; set; }
        public string? Conn { get; set; }
    }
    public class ExternalStoreDataDto
    {
        public StoreTypeEnum StoreType { get; set; }
        public string Conn { get; set; }
    }
}
