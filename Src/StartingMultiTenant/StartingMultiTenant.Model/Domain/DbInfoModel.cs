using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class DbInfoModel
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public string Identifier { get; set; }
        public Int64 ServiceInfoId { get; set; }
        public string Description { get; set; }

    }
}
