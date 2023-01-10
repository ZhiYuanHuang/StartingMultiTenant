using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class AppRequestDto
    {
        public long RequestId { get; set; }
        public string? RequestObj { get; set; }
    }

    public class AppRequestDto<T> : AppRequestDto
    {
        public List<T>? DataList { get; set; }
        public T Data { get; set; }
    }
}
