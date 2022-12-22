using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Text;

namespace StartingMultiTenant.Util
{
    public static class HashUtil
    {
        public static string Hash_8(string content) {
            string tmp= BitConverter.ToString(Crc32.Hash(Encoding.UTF8.GetBytes(content)));
            return tmp.Replace("-","").ToLower();
        }
    }
}
