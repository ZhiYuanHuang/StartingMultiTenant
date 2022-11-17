using Microsoft.Extensions.Configuration;
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

        public string Encrypt_Aes(string plainText) {
            return EncryptUtil.Encrypt_Aes(_sysAesKey, plainText) ;
        }

        public string Decrypt_Aes(string cipherText) {
            return EncryptUtil.Decrypt_Aes(_sysAesKey, cipherText);
        }
    }
}
