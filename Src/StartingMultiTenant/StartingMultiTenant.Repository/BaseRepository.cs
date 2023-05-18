using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Repository
{
    public abstract class BaseRepository<T>:IRepository<T> where T : class, new ()
    {
        protected readonly TenantDbDataContext _tenantDbDataContext;
        public abstract string TableName { get; }
        public BaseRepository(TenantDbDataContext tenantDbDataContext
            ) {
            _tenantDbDataContext = tenantDbDataContext;
        }

        public virtual void BeginTransaction() {
            _tenantDbDataContext.Master.BeginTransaction();
        }

        public virtual void CommitTransaction() {
            _tenantDbDataContext.Master.CommitTransaction();
        }

        public virtual void RollbackTransaction() {
            _tenantDbDataContext.Master.RollbackTransaction();
        }

        public List<T> GetEntitiesByQuery(Dictionary<string, object> fieldValueDict = null) {
            CheckTableNameNotNull();

            StringBuilder builder = new StringBuilder($"select * from {TableName} ");

            if (fieldValueDict != null && fieldValueDict.Count > 0) {

                int i = 0;
                builder.Append(" where ");

                foreach (KeyValuePair<string, object> kvp in fieldValueDict) {
                    builder.Append(string.Format($" {kvp.Key.ToUpper()}=@{kvp.Key} "));
                    if (++i < fieldValueDict.Count) {
                        builder.Append(" And ");
                    }
                }

                return _tenantDbDataContext.Slave.QueryList<T>(builder.ToString(), fieldValueDict);
            }

            return _tenantDbDataContext.Slave.QueryList<T>(builder.ToString(),null);
        }

        public T GetEntityByQuery(Dictionary<string, object> fieldValueDict = null) {
            CheckTableNameNotNull();

            StringBuilder builder = new StringBuilder($"select * from {TableName} ");

            if (fieldValueDict != null && fieldValueDict.Count > 0) {

                int i = 0;
                builder.Append(" where ");
                foreach (KeyValuePair<string, object> kvp in fieldValueDict) {
                    builder.Append(string.Format($" {kvp.Key.ToUpper()}=@{kvp.Key} "));
                    if (++i < fieldValueDict.Count) {
                        builder.Append(" And ");
                    }
                }

                return _tenantDbDataContext.Slave.Query<T>(builder.ToString(), fieldValueDict);
            }

            return _tenantDbDataContext.Slave.Query<T>(builder.ToString(),null);
        }

        public T GetEntityById(Int64 Id) {
            string sql = $"Select * From {TableName} Where Id=@id";
            return _tenantDbDataContext.Slave.Query<T>(sql,new { id=Id});
        }

        public List<T> GetEntitiesByIds(List<Int64> ids) {
            if(ids==null || !ids.Any()) {
                return new List<T>();
            }
            string sql = $"Select * From {TableName} Where Id=ANY(@ids)";
            return _tenantDbDataContext.Slave.QueryList<T>(sql, new { ids = ids.ToArray() });
        }

        public bool Delete(Int64 id) {
            string sql = $"Delete From {TableName} Where Id=@id";
    
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, new { id = id })>0;
        }

        public PagingData<T> GetPage(int pageSize,int pageIndex) {
            StringBuilder countBuilder = new StringBuilder($"Select Count(Id) From {TableName} ");
            StringBuilder dataBuilder = new StringBuilder($"Select * From {TableName} ");

            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "pageSize",pageSize},
                { "offSet",pageSize*pageIndex}
            };

            dataBuilder.Append(" Limit @pageSize OFFSET @offSet");

            return GetPage(countBuilder.ToString(),dataBuilder.ToString(),p,pageSize,pageIndex);
        }

        public PagingData<T> GetPage(int pageSize,int pageIndex,Dictionary<string,object> whereFieldDict,Dictionary<string,bool> orderFieldDict=null,List<string> selectFields=null) {
            StringBuilder countBuilder = new StringBuilder($"Select Count(Id) From {TableName} ");
            StringBuilder dataBuilder = new StringBuilder($"Select * From {TableName} ");

            if(selectFields!=null && selectFields.Any()) {
                dataBuilder = new StringBuilder(string.Format("Select {0} From {1} ",string.Join(',', selectFields),TableName));
            }

            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "pageSize",pageSize},
                { "offSet",pageSize*pageIndex}
            };

            if(whereFieldDict != null && whereFieldDict.Any()) {
                bool first = true;
                StringBuilder whereBuilder = new StringBuilder();

                foreach (var pair in whereFieldDict) {
                    if (!first) {
                        whereBuilder.Append(" And ");
                    } else {
                        first = false;
                        whereBuilder.Append(" Where ");
                    }

                    string paramName = pair.Key.ToLower();

                    if (pair.Value == null) {
                        whereBuilder.Append($" {pair.Key} Is NULL ");
                    } else if (pair.Value is ICollection) {
                        ICollection? objects = pair.Value as ICollection;
                        if (objects != null && objects.Count > 0) {
                            whereBuilder.Append($" {pair.Key}=ANY(@{paramName}) ");
                            object[] objArr = new object[objects.Count];
                            objects.CopyTo(objArr, 0);
                            p[paramName] = objArr;
                        }
                    } else {
                        whereBuilder.Append($" {pair.Key}=@{paramName} ");
                        p[paramName] = pair.Value;
                    }
                }

                string whereStr = whereBuilder.ToString();
                countBuilder.Append(whereStr);
                dataBuilder.Append(whereStr);
            }
            
            if(orderFieldDict != null && orderFieldDict.Any()) {
                StringBuilder orderBuilder = new StringBuilder();
                bool first = true;
                foreach(var pair in orderFieldDict) {
                    if (!first) {
                        orderBuilder.Append(",");
                    } else {
                        first = false;
                        orderBuilder.Append(" Order By ");
                    }

                    //true:asc,false:desc
                    if (pair.Value) {
                        orderBuilder.Append($" {pair.Key}");
                    } else {
                        orderBuilder.Append($" {pair.Key} Desc");
                    }
                }

                string orderStr = orderBuilder.ToString();
                dataBuilder.Append(orderStr);
            }

            dataBuilder.Append(" Limit @pageSize OFFSET @offSet");

            return GetPage(countBuilder.ToString(),dataBuilder.ToString(),p,pageSize,pageIndex);
        }

        public PagingData<T> GetPage(string countSqlStr,string getPageStr,Dictionary<string,object> p,int pageSize,int pageIndex) {
            int count = (int)((long)_tenantDbDataContext.Slave.ExecuteScalar(countSqlStr, p));
            if (count == 0) {
                return new PagingData<T>(pageIndex, pageSize, 0, new List<T>());
            }

            var list = _tenantDbDataContext.Slave.QueryList<T>(getPageStr, p);
            return new PagingData<T>(pageIndex, pageSize, count, list);
        }

        public PagingData<TDto> GetPageWithMaping<TDto>(int pageSize, int pageIndex, Dictionary<string, object> whereFieldDict,Func<T, TDto> mappingFunc, Dictionary<string, bool> orderFieldDict = null, List<string> selectFields = null)
            where TDto : new() 
        {
            StringBuilder countBuilder = new StringBuilder($"Select Count(Id) From {TableName} ");
            StringBuilder dataBuilder = new StringBuilder($"Select * From {TableName} ");

            if (selectFields != null && selectFields.Any()) {
                dataBuilder = new StringBuilder(string.Format("Select {0} From {1} ", string.Join(',', selectFields), TableName));
            }

            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "pageSize",pageSize},
                { "offSet",pageSize*pageIndex}
            };

            if (whereFieldDict != null && whereFieldDict.Any()) {
                bool first = true;
                StringBuilder whereBuilder = new StringBuilder();

                foreach (var pair in whereFieldDict) {
                    if (!first) {
                        whereBuilder.Append(" And ");
                    } else {
                        first = false;
                        whereBuilder.Append(" Where ");
                    }

                    string paramName = pair.Key.ToLower();

                    if (pair.Value == null) {
                        whereBuilder.Append($" {pair.Key} Is NULL ");
                    } else if (pair.Value is ICollection) {
                        ICollection? objects = pair.Value as ICollection;
                        if (objects != null && objects.Count > 0) {
                            whereBuilder.Append($" {pair.Key}=ANY(@{paramName}) ");
                            object[] objArr = new object[objects.Count];
                            objects.CopyTo(objArr, 0);
                            p[paramName] = objArr;
                        }
                    } else {
                        whereBuilder.Append($" {pair.Key}=@{paramName} ");
                        p[paramName] = pair.Value;
                    }
                }

                string whereStr = whereBuilder.ToString();
                countBuilder.Append(whereStr);
                dataBuilder.Append(whereStr);
            }

            if (orderFieldDict != null && orderFieldDict.Any()) {
                StringBuilder orderBuilder = new StringBuilder();
                bool first = true;
                foreach (var pair in orderFieldDict) {
                    if (!first) {
                        orderBuilder.Append(",");
                    } else {
                        first = false;
                        orderBuilder.Append(" Order By ");
                    }

                    //true:asc,false:desc
                    if (pair.Value) {
                        orderBuilder.Append($" {pair.Key}");
                    } else {
                        orderBuilder.Append($" {pair.Key} Desc");
                    }
                }

                string orderStr = orderBuilder.ToString();
                dataBuilder.Append(orderStr);
            }

            dataBuilder.Append(" Limit @pageSize OFFSET @offSet");

            return GetPageWithMaping(countBuilder.ToString(), dataBuilder.ToString(), p, pageSize, pageIndex,mappingFunc);
        }

        public PagingData<TDto> GetPageWithMaping<TDto>(string countSqlStr, string getPageStr, Dictionary<string, object> p, int pageSize, int pageIndex, Func<T, TDto> mappingFunc)
            where TDto : new()
        { 
            int count = (int)((long)_tenantDbDataContext.Slave.ExecuteScalar(countSqlStr, p));
            if (count == 0) {
                return new PagingData<TDto>(pageIndex, pageSize, 0, new List<TDto>());
            }

            var list = _tenantDbDataContext.Slave.QueryList<T>(getPageStr, p);
            var list2= list.Select(x => mappingFunc(x)).ToList();
            return new PagingData<TDto>(pageIndex, pageSize, count, list2);
        }

        public void CheckTableNameNotNull() {
            if (string.IsNullOrEmpty(TableName)) {
                throw new Exception("tableName is empty,cann't auto generate sql");
            }
        }

        public virtual bool Insert(T t, out long id) {
            throw new NotImplementedException();
        }

        public virtual bool Update(T t) {
            throw new NotImplementedException();
        }
    }
}
