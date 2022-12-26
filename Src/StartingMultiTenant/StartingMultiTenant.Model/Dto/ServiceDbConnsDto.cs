using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class ServiceDbConnsDto
    {
        public Int64 Id { get; set; }
        public string ServiceIdentifier { get; set; }
        public string DbIdentifier { get; set; }

        public string DecryptDbConn { get; set; }
        public string OverrideDbConn { get; set; }

        public int MajorVersion { get; set; }
        public int MinorVersion { get; set;}

        public int ActionType { get; set; }
    }
}
