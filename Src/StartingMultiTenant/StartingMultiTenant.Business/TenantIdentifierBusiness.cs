using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StartingMultiTenant.Business
{
    public class TenantIdentifierBusiness:BaseBusiness<TenantIdentifierModel>
    {
        private readonly TenantIdentifierRepository _tenantIdentifierRepo;
        private readonly TenantDomainRepository _tenantDomainRepo;
        private readonly CreateDbScriptRepository _createDbScriptRepo;
        private readonly TenantServiceDbConnRepository _innerDbConnRepo;
        private readonly ExternalTenantServiceDbConnRepository _externalDbConnRepo;
        public TenantIdentifierBusiness(TenantDomainRepository tenantDomainRepo,
            TenantIdentifierRepository tenantIdentifierRepo,
            CreateDbScriptRepository createDbScriptRepository,
            TenantServiceDbConnRepository tenantServiceDbConnRepo,
            ExternalTenantServiceDbConnRepository externalDbConnRepo,
            ILogger<TenantIdentifierBusiness> logger):base(tenantIdentifierRepo,logger) {
            _tenantDomainRepo= tenantDomainRepo;
            _tenantIdentifierRepo= tenantIdentifierRepo;
            _createDbScriptRepo = createDbScriptRepository;
            _innerDbConnRepo = tenantServiceDbConnRepo;
            _externalDbConnRepo = externalDbConnRepo;
        }

        public bool Insert(TenantIdentifierModel tenantIdentifier) {
            
            return _tenantIdentifierRepo.Insert(tenantIdentifier);
        }

        public bool Delete(string tenantGuid) {
            return _tenantIdentifierRepo.Delete(tenantGuid);
        }

        public bool ExistTenant(string tenantDomain,string tenantIdentifier) {
            List<TenantIdentifierModel> tenantList= _tenantIdentifierRepo.GetTenantListByDomain(tenantDomain);
            if (!tenantList.Any()) {
                return false;
            }

            var existTenant= tenantList.FirstOrDefault(x => string.Compare(x.TenantIdentifier, tenantIdentifier, 0) == 0);
            return existTenant!= null;
        }

        public bool ExistTenant(string tenantDomain, string tenantIdentifier,out TenantIdentifierModel model) {
            return _tenantIdentifierRepo.ExistTenant(tenantDomain,tenantIdentifier,out model);
        }

        public PagingData<TenantIdentifierDto> GetPage(string tenantDomain, int pageSize, int pageIndex) {
            var pageData= _tenantIdentifierRepo.GetPage(pageSize,pageIndex, ConvertFromModel,tenantDomain);

            return pageData;
        }



        public TenantServiceDbConnsDto GetTenantsDbConns(Int64 id) {
            var model= Get(id);
            if (model == null) {
                return null;
            }

            var innerDbConns= _innerDbConnRepo.GetConnByTenantAndService(model.TenantDomain,model.TenantIdentifier);
            var externalDbConns = _externalDbConnRepo.GetByTenantAndService(model.TenantDomain, model.TenantIdentifier);

            TenantServiceDbConnsDto dto = new TenantServiceDbConnsDto() { 
                TenantDomain=model.TenantDomain,
                TenantIdentifier=model.TenantIdentifier,
                InnerDbConnList=new List<ServiceDbConnDto>(),
                ExternalDbConnList=new List<ServiceDbConnDto>()
            };
            if (innerDbConns.Any()) {
                dto.InnerDbConnList.AddRange(innerDbConns.Select(x=>new ServiceDbConnDto() { 
                    ServiceIdentifier=x.ServiceIdentifier,
                    DbIdentifier=x.DbIdentifier,
                    EncryptedConnStr=x.EncryptedConnStr
                }));
            }
            if (externalDbConns.Any()) {
                dto.ExternalDbConnList.AddRange(externalDbConns.Select(x => new ServiceDbConnDto() {
                    ServiceIdentifier = x.ServiceIdentifier,
                    DbIdentifier = x.DbIdentifier,
                    EncryptedConnStr = x.EncryptedConnStr
                }));
            }
            return dto;
        }

        public List<TenantServiceDbConnsDto> GetTenantsDbConns(List<Int64> ids,Func<string,string> decryptDbConnFunc) {
           
            var innerDbConns = _innerDbConnRepo.GetConnByTenantIds(ids).OrderBy(x=>x.TenantIdentifier).ToList();
            var externalDbConns = _externalDbConnRepo.GetConnByTenantIds(ids).OrderBy(x => x.TenantIdentifier).ToList();

            var models= Get(ids).OrderBy(x=>x.TenantIdentifier).ToList();

            List<TenantServiceDbConnsDto> dtoList = models.Select(x => {
                var dto = new TenantServiceDbConnsDto() {
                    TenantDomain = x.TenantDomain,
                    TenantIdentifier = x.TenantIdentifier,
                };

                if (innerDbConns.Any()) {
                    int startIndex= innerDbConns.FindIndex(conn => string.Compare(conn.TenantIdentifier, x.TenantIdentifier, true) == 0);
                    if (startIndex != -1) {
                        int endIndex = innerDbConns.FindLastIndex(conn => string.Compare(conn.TenantIdentifier, x.TenantIdentifier, true) == 0);
                        TenantServiceDbConnModel[] arr = new TenantServiceDbConnModel[endIndex-startIndex+1];
                        innerDbConns.CopyTo(startIndex, arr, 0, endIndex - startIndex + 1);
                        dto.InnerDbConnList = arr.Select(x => new ServiceDbConnDto() {
                            ServiceIdentifier = x.ServiceIdentifier,
                            DbIdentifier = x.DbIdentifier,
                            DecryptDbConn = decryptDbConnFunc( x.EncryptedConnStr)
                        }).ToList();
                    }
                }
                if (externalDbConns.Any()) {
                    int startIndex = externalDbConns.FindIndex(conn => string.Compare(conn.TenantIdentifier, x.TenantIdentifier, true) == 0);
                    if (startIndex != -1) {
                        int endIndex = externalDbConns.FindLastIndex(conn => string.Compare(conn.TenantIdentifier, x.TenantIdentifier, true) == 0);
                        ExternalTenantServiceDbConnModel[] arr = new ExternalTenantServiceDbConnModel[endIndex - startIndex + 1];
                        externalDbConns.CopyTo(startIndex, arr, 0, endIndex - startIndex + 1);
                        dto.ExternalDbConnList = arr.Select(x => new ServiceDbConnDto() {
                            ServiceIdentifier = x.ServiceIdentifier,
                            DbIdentifier = x.DbIdentifier,
                            DecryptDbConn = decryptDbConnFunc(x.EncryptedConnStr)
                        }).ToList();
                    }
                }

                return dto;
            }).ToList();
            
            return dtoList;
        }

        public TenantIdentifierDto ConvertFromModel(TenantIdentifierModel model) {
            return new TenantIdentifierDto() {
                Id = model.Id,
                TenantIdentifier = model.TenantIdentifier,
                TenantDomain = model.TenantDomain,
                TenantGuid = model.TenantGuid,
                TenantName=model.TenantName,
                Description=model.Description,
                CreateTime=model.CreateTime,
                UpdateTime=model.UpdateTime,
                CreateDbScriptIds = new List<long>()
            };
        }
    }
}
