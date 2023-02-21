using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class k8sPatchObjectDto
    {
        public k8sPatchObjectDto(string keyPath,string keyValue, bool isAdd=false) {
            if (isAdd) {
                op = "add";
            } else {
                op = "replace";
            }
            path = keyPath;
            value=keyValue;
        }
        public string op { get; private set; }
        public string path { get;private set; }
        public string value { get;private set; }
    }

    public class K8sSecretModifyDto {
        public Dictionary<string, string> data { get; set; }
    }
}
