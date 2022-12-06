using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Util
{
    public static class ConvertUtil
    {
        #region ToInt

        public static int ToInt(object o) {
            return Convert.ToInt32(o);
        }

        public static int ToInt(object o, int defaultValue) {
            if (o == null || o == DBNull.Value) {
                return defaultValue;
            }

            try {
                return Convert.ToInt32(o);
            } catch {
                return defaultValue;
            }
        }

        #endregion
    }
}
