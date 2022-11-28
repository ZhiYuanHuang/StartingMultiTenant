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

        public string Encrypt_DbserverPwd(string userPwd) {
            return EncryptUtil.Encrypt_Aes(_sysAesKey, userPwd);
        }

        public string Decrypt_DbServerPwd(string encryptUserpwd) {
            return EncryptUtil.Decrypt_Aes(_sysAesKey, encryptUserpwd);
            
        }

        public string Encrypt_DbConn(string userPwd) {
            return EncryptUtil.Encrypt_Aes(_sysAesKey, userPwd);
        }

        public string Decrypt_DbConn(string encryptUserpwd) {
            return EncryptUtil.Decrypt_Aes(_sysAesKey, encryptUserpwd);

        }
    }
}
