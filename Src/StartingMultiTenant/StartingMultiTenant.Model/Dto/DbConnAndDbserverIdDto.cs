using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class DbConnAndDbserverIdDto
    {
        public Int64 DbConnId { get; set; }
        public Int64 OldDbServerId { get; set; }
        public Int64 DbServerId { get; set; }
    }
}
