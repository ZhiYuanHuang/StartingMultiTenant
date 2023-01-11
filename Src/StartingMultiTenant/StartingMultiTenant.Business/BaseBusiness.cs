using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Business
{
    public interface IBusiness<T> where T:new() {
        T Get(Int64 id);
        List<T> Get(List<Int64> ids);
        List<T> GetAll();
        PagingData<T> GetPage(int pageSize, int pageIndex);
        Tuple<bool, string> Delete(Int64 id);
        Tuple<bool,List<Int64>,string> DeleteMany(List<Int64> ids);
    }

    public abstract class BaseBusiness<T>: IBusiness<T> where T:class,new()
    {
        protected readonly IRepository<T> selfRepo;
        protected readonly ILogger<BaseBusiness<T>> _logger;
        public BaseBusiness(IRepository<T> repository,
            ILogger<BaseBusiness<T>> logger) {
            selfRepo= repository;
            _logger = logger;
        }

        public virtual Tuple<bool, string> Delete(long id) {
            bool result = false;
            string errMsg = string.Empty;
            try {
                result= selfRepo.Delete(id);
            }
            catch(Exception ex) {
                result = false;
                errMsg = $"delete id {id} raise error";
                _logger.LogError($"delete id {id} raise error",ex);
            }
            return Tuple.Create(result,errMsg);
        }

        public virtual Tuple<bool, List<long>, string> DeleteMany(List<long> ids) {
            bool result = true;
            StringBuilder errMsgBuilder = new StringBuilder();
            List<Int64> successDeleteIds = new List<long>();
            foreach (var id in ids) {
                var resultTuple = Delete(id);
                if (!resultTuple.Item1) {
                    result = false;
                    errMsgBuilder.AppendLine(resultTuple.Item2);
                } else {
                    successDeleteIds.Add(id);
                }
            }
            return Tuple.Create(result, successDeleteIds, errMsgBuilder.ToString());

        }

        public virtual T Get(long id) {
            return selfRepo.GetEntityById(id);
        }

        public virtual List<T> Get(List<long> ids) {
            return selfRepo.GetEntitiesByIds(ids);
        }

        public virtual List<T> GetAll() {
            return selfRepo.GetEntitiesByQuery();
        }

        public virtual PagingData<T> GetPage(int pageSize, int pageIndex) {
            return selfRepo.GetPage(pageSize,pageIndex);
        }
    }
}
