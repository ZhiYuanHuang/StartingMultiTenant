using Microsoft.Extensions.Configuration;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Service
{
    public class EncryptService
    {
        private readonly string _sysAesKey;
        public EncryptService(IConfiguration config) {
            _sysAesKey = config["SysAesKey"];
        }

        public string Encrypt_Aes(string plainText, string aesKey=null) {
            return EncryptUtil.Encrypt_Aes(string.IsNullOrEmpty(aesKey)? _sysAesKey:aesKey, plainText) ;
        }

        public string Decrypt_Aes(string cipherText,string aesKey=null) {
            return EncryptUtil.Decrypt_Aes(string.IsNullOrEmpty(aesKey) ? _sysAesKey : aesKey, cipherText);
        }

        public void Encrypt_DbserverPwd(ref DbServerModel dbServer,string userPwd) {
            dbServer.EncryptUserpwd = EncryptUtil.Encrypt_Aes(_sysAesKey, userPwd);
            
        }

        public DbServerDto Decrypt_DbServerPwd(DbServerModel dbServerModel) {
            string decryptPwd = EncryptUtil.Decrypt_Aes(_sysAesKey,dbServerModel.EncryptUserpwd);
            return new DbServerDto() {
                DecryptUserPwd = decryptPwd,

                DbType =dbServerModel.DbType,
                EncryptUserpwd=dbServerModel.EncryptUserpwd,
                ServerHost=dbServerModel.ServerHost,
                ServerPort=dbServerModel.ServerPort,
                UserName=dbServerModel.UserName,
                Id=dbServerModel.Id,
            };
        }
 
    }
}
