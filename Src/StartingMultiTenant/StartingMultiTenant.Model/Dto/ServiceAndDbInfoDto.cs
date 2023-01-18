using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class ServiceAndDbInfoDto
    {
        public Int64 ServiceInfoId { get; set; }
        public string ServiceIdentifier { get; set; }
        public List<DbInfoDto> DbInfos { get; set; }
    }

    public class DbInfoDto {
        public Int64 DbInfoId { get; set; }
        public string DbIdentifier { get; set;}
    }
}
