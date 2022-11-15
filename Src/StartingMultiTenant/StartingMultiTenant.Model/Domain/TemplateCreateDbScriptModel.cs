using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class TemplateCreateDbScriptModel
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string ServiceIdentifier { get; set; }
    }
}
