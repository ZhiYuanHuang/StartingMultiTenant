using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace StartingMultiTenant.Util
{
    public static class EncryptUtil
    {
        public static string Encrypt_Aes(string aesKey,string plainText)
        {
            byte[] toEncryptByteArr = Encoding.UTF8.GetBytes(plainText);
            byte[] resultByteArr;
            using (var aesInstance= Aes.Create()) {
                aesInstance.Key = Encoding.UTF8.GetBytes(aesKey);
                aesInstance.Padding = PaddingMode.PKCS7;
                aesInstance.Mode = CipherMode.ECB;
                var encryptor = aesInstance.CreateEncryptor();
                resultByteArr = encryptor.TransformFinalBlock(toEncryptByteArr, 0, toEncryptByteArr.Length);
            }
                
            return Convert.ToBase64String(resultByteArr);
        }

        public static string Decrypt_Aes(string aesKey, string cipherText)
        {
            byte[] toDecryptByteArr = Convert.FromBase64String(cipherText);
            byte[] resultByteArr;
            using (var aesInstance = Aes.Create()) {
                aesInstance.Key = Encoding.UTF8.GetBytes(aesKey);
                aesInstance.Padding = PaddingMode.PKCS7;
                aesInstance.Mode = CipherMode.ECB;
                var decryptor = aesInstance.CreateDecryptor();
                resultByteArr = decryptor.TransformFinalBlock(toDecryptByteArr, 0, toDecryptByteArr.Length);
            }

            return Encoding.UTF8.GetString(resultByteArr);
        }
    }
}
