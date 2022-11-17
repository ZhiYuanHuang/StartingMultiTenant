using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Util
{
    public class MysqlScriptExecuteUtil
    {
        public static void ExecuteScript(string dbConnStr, string executeScript, string rollbackScript) {
            MySqlConnection connPg = new MySqlConnection(dbConnStr);

            try {
                var createdb_cmd = new MySqlCommand(executeScript, connPg);
                connPg.Open();

                if (connPg.State == System.Data.ConnectionState.Open) {
                    try {
                        createdb_cmd.ExecuteNonQuery();
                    } catch (Exception ex) {
                        if (!string.IsNullOrEmpty(rollbackScript)) {
                            try {
                                var rollback_cmd = new MySqlCommand(rollbackScript, connPg);
                                rollback_cmd.ExecuteNonQuery();
                            } catch {

                            }
                        }

                        throw ex;
                    }
                }

            } catch (Exception ex) {
                throw new Exception("ExecuteScript raise error", ex);
            } finally {
                connPg?.Dispose();
            }
        }
    }
}
