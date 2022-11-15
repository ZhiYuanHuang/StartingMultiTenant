using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace StartingMultiTenant.Util
{
    public static class EncryptUtil
    {
        private static Dictionary<string, Tuple<ICryptoTransform, ICryptoTransform>> _dict;
        private static object _lockObj;

        static EncryptUtil()
        {
            _dict = new Dictionary<string, Tuple<ICryptoTransform, ICryptoTransform>>();
            _lockObj = new object();
        }

        

        public static string Encrypt_Aes(string aesKey,string plainText)
        {
            var aesHandlerTuple= getAesHanlder(aesKey);
            byte[] toEncryptByteArr = Encoding.UTF8.GetBytes(plainText);
            byte[] resultByteArr = aesHandlerTuple.Item1.TransformFinalBlock(toEncryptByteArr,0,toEncryptByteArr.Length);
            return Convert.ToBase64String(resultByteArr);
            
        }

        public static string Decrypt_Aes(string cipherText,string aesKey)
        {
            byte[] toDecryptByteArr = Convert.FromBase64String(cipherText);
            var aesHandlerTuple = getAesHanlder(aesKey);
            byte[] resultByteArr = aesHandlerTuple.Item2.TransformFinalBlock(toDecryptByteArr,0,toDecryptByteArr.Length);
            return Encoding.UTF8.GetString(resultByteArr);
        }

        private static Tuple<ICryptoTransform, ICryptoTransform> getAesHanlder(string aesKey)
        {
            string cacheKey = $"aes:{aesKey}";
            if (!_dict.ContainsKey(cacheKey))
            {
                lock (_lockObj)
                {
                    if (!_dict.ContainsKey(cacheKey))
                    {

                        Aes _aesInstance = Aes.Create();
                        _aesInstance.Key = Encoding.UTF8.GetBytes(aesKey);
                        _aesInstance.Padding = PaddingMode.PKCS7;
                        _aesInstance.Mode = CipherMode.ECB;
                        var _encryptor = _aesInstance.CreateEncryptor();
                        var _decryptor = _aesInstance.CreateDecryptor();
                        _dict.Add(cacheKey,new Tuple< ICryptoTransform, ICryptoTransform>(_encryptor,_decryptor));
                    }
                }
            }

            return _dict[cacheKey];
        }
    }
}
