using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    /// <summary>
    /// 查询结果分页数据
    /// </summary>
    /// <typeparam name="T">当前页结果类型</typeparam>
    public class PagingData<T> where T : new()
    {
        public int PageSize { get; }
        public int PageIndex { get; }
        public int PageCount { get; }
        public int RecordCount { get; }

        public List<T> Data { get; }
        public object Extra { get; set; }

        public PagingData(int pageIndex, int pageSize, int recordCount, List<T> datalist) {
            PageSize = pageSize > 0 ? pageSize : 10;
            PageIndex = pageIndex > 0 ? pageIndex : 1;
            RecordCount = recordCount > 0 ? recordCount : 0;
            Data = datalist ?? new List<T>();

            PageCount = RecordCount > 0 ? (int)Math.Ceiling(RecordCount * 1.0 / PageSize) : 0;
            if (pageIndex > PageCount) { pageIndex = PageCount; }
        }
    }

    /// <summary>
    /// 分页查询参数基类
    /// </summary>
    public class PagingParam
    {
        public int PageSize { get; set; }
        public int PageIndex { get; set; }

        public PagingParam() {
            PageSize = 10;
            PageIndex = 1;
        }
    }
}
