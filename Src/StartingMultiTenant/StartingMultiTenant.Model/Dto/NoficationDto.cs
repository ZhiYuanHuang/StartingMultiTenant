using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class NoficationDto
    {
        public Int64? scriptId { get; set; }
        public int notifyLevel { get; set; }
        public string? notifyTitle { get; set; }
        public string? notifyBody { get; set; }
    }
}
