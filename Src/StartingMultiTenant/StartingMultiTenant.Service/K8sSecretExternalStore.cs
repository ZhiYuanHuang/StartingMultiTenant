using k8s;
using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Utilities;
using StartingMultiTenant.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StartingMultiTenant.Service
{
    public class K8sSecretExternalStore : IExternalStoreExecutor
    {
        private bool disposedValue;

        public bool IsConnected => checkStatus();
        private readonly ILogger<K8sSecretExternalStore> _logger;
        private readonly string _configFilePath;
        private readonly Kubernetes _client;
        private readonly string _k8sNamespace;

        public K8sSecretExternalStore(string configFilePath,
            string k8sNamespace,
            ILogger<K8sSecretExternalStore> logger) {
            _logger= logger;
            _configFilePath=configFilePath;
            _k8sNamespace = k8sNamespace;
            if (System.IO.File.Exists(configFilePath)) {
                var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(configFilePath);
                
                _client = new Kubernetes(config);
            }
        }

        //public void WriteToExternalStore(List<TenantServiceDbConnsDto> tenantServiceDbConns) {
        //    foreach(var tenantDbConns in tenantServiceDbConns) {
        //        string secretName = $"{tenantDbConns.TenantDomain}-{tenantDbConns.TenantIdentifier}-dbconns".ToLower();
        //        V1Secret secretBody = new V1Secret() {
        //            ApiVersion = "v1",
        //            Metadata = new V1ObjectMeta() {
        //                Name = secretName,
        //            },
        //            Type = "Opaque",
                   
        //            StringData = new Dictionary<string, string>()
        //        };
        //        if (tenantDbConns.InnerDbConnList != null && tenantDbConns.InnerDbConnList.Any()) {
        //            foreach (var innerDbConn in tenantDbConns.InnerDbConnList) {
        //                secretBody.StringData.Add($"Inner_{innerDbConn.ServiceIdentifier}_{innerDbConn.DbIdentifier}", innerDbConn.DecryptDbConn);
                        
        //            }
        //        }

        //        if (tenantDbConns.ExternalDbConnList != null && tenantDbConns.ExternalDbConnList.Any()) {
        //            foreach (var externalDbConn in tenantDbConns.ExternalDbConnList) {
        //                secretBody.StringData.Add($"External_{externalDbConn.ServiceIdentifier}_{externalDbConn.DbIdentifier}", externalDbConn.DecryptDbConn);
        //            }
        //        }
        //        try {
        //            var existed = _client.ReplaceNamespacedSecret(secretBody, secretName, _k8sNamespace);
                   
        //        } catch(Exception ex) {
        //            _logger.LogError(ex,$"{secretName} maybe not existed");
        //            try {
        //                var result = _client.CreateNamespacedSecret(secretBody, _k8sNamespace);
        //            } catch(Exception ex2) {
        //                _logger.LogError(ex, $"{secretName} create error");
        //            }
        //        }
        //    }
        //}

        public void WriteToExternalStore(List<TenantServiceDbConnsDto> tenantServiceDbConns) {
            Dictionary<string,List<ServiceDbConnDto>> serviceDbInnerDict= tenantServiceDbConns.Where(x=>x.InnerDbConnList!=null).SelectMany(x => x.InnerDbConnList).GroupBy(x => $"{x.ServiceIdentifier}-{x.DbIdentifier}-Inner".ToLower(), x => x).ToDictionary(g => g.Key, v => v.ToList());
            Dictionary<string, List<ServiceDbConnDto>> serviceDbExternalDict = tenantServiceDbConns.Where(x => x.ExternalDbConnList != null).SelectMany(x => x.ExternalDbConnList).GroupBy(x => $"{x.ServiceIdentifier}-{x.DbIdentifier}-External".ToLower(), x => x).ToDictionary(g => g.Key, v => v.ToList());

            var secretList= _client.ListNamespacedSecret(_k8sNamespace);
            var existedSecretDict= secretList.Items.Select(x => x.Name()).ToDictionary(x=>x);
            writeToExternalStore(existedSecretDict,serviceDbInnerDict);
            writeToExternalStore(existedSecretDict, serviceDbExternalDict);
        }

        private void writeToExternalStore(Dictionary<string,string> existedSecretDict, Dictionary<string, List<ServiceDbConnDto>> serviceDbDict) {
            foreach (var innerPair in serviceDbDict) {
                string secretName = innerPair.Key;
                List<ServiceDbConnDto> serviceDbConnDtos = innerPair.Value;

                try {
                    if (!existedSecretDict.ContainsKey(secretName)) {
                        var stringData = new Dictionary<string, string>();
                        List<ServiceDbConnDto> orderedServiceDbConnDtos = serviceDbConnDtos.OrderBy(x => x.TenantDomain).ThenBy(x => x.TenantIdentifier).ToList();

                        foreach (var serviceDbConn in orderedServiceDbConnDtos) {
                            stringData.Add($"{serviceDbConn.TenantDomain}_{serviceDbConn.TenantIdentifier}", serviceDbConn.DecryptDbConn);
                        }

                        V1Secret secretBody = new V1Secret() {
                            ApiVersion = "v1",
                            Metadata = new V1ObjectMeta() {
                                Name = secretName,
                            },
                            Type = "Opaque",

                            StringData = stringData,
                        };
                        _client.CreateNamespacedSecret(secretBody, _k8sNamespace);
                    } else {
                        //var existSecret = _client.ReadNamespacedSecret(secretName, _k8sNamespace);
                        //List<k8sPatchObjectDto> patchList = new List<k8sPatchObjectDto>();

                        //if (!existSecret.Data.Any()) {
                        //    patchList = serviceDbConnDtos.Select(x => new k8sPatchObjectDto($"{x.TenantDomain}_{x.TenantIdentifier}", Convert.ToBase64String(Encoding.Default.GetBytes(x.DecryptDbConn)), true)).ToList();
                        //} else {
                        //    patchList = serviceDbConnDtos.Select(x => {
                        //        if (existSecret.Data.ContainsKey($"{x.TenantDomain}_{x.TenantIdentifier}")) {
                        //            return new k8sPatchObjectDto($"/data/{x.TenantDomain}_{x.TenantIdentifier}", Convert.ToBase64String(Encoding.Default.GetBytes(x.DecryptDbConn)));
                        //        }
                        //        return new k8sPatchObjectDto($"/data/{x.TenantDomain}_{x.TenantIdentifier}", Convert.ToBase64String(Encoding.Default.GetBytes(x.DecryptDbConn)), true);
                        //    }).ToList();
                        //}
                        //string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(patchList);

                        //V1Patch patchBody = new V1Patch(jsonStr, V1Patch.PatchType.JsonPatch);  

                        K8sSecretModifyDto k8SSecretModifyDto = new K8sSecretModifyDto() { data=new Dictionary<string, string>()};
                        serviceDbConnDtos.ForEach(x => {
                            k8SSecretModifyDto.data.Add($"{x.TenantDomain}_{x.TenantIdentifier}", Convert.ToBase64String(Encoding.Default.GetBytes(x.DecryptDbConn)));
                        });

                        string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(k8SSecretModifyDto);
                        V1Patch patchBody = new V1Patch(jsonStr, V1Patch.PatchType.MergePatch);

                        var result= _client.PatchNamespacedSecret(patchBody, secretName, _k8sNamespace);
                    }
                } catch (Exception ex) {
                    _logger.LogError(ex, $"{secretName} write error");
                }
            }
        }

        private bool checkStatus() {
            try {
                var apiVersions= _client.Core.GetAPIVersions();
                if (apiVersions == null) {
                    return false;
                }
                return true;
            }catch(Exception ex) {
                return false;
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: 释放托管状态(托管对象)
                    _client.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~K8sSecretExternalStore()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose() {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
