using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;

namespace WebService1
{
    public class ComenziSiteHelper
    {

        public string getDepoziteUL(string ul)
        {

            string depozite = "";
            

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string sqlString = " select depozit from sapprd.zfilialesite where mandt = '900' and ul=:unitLog and ul != depozit order by depozit ";

                cmd.CommandText = sqlString;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":unitLog", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = ul;


                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        if (depozite.Length == 0)
                            depozite = oReader.GetString(0);
                        else
                            depozite += "," + oReader.GetString(0);
                    }
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }


            return depozite;
        }




        public static string getUlDistrib(string ulGed)
        {
            return ulGed.Replace(ulGed.Substring(2, 1), "1");
        }

        public static string getUlGed(string ulGed)
        {
            return ulGed.Replace(ulGed.Substring(2, 1), "2");
        }


    }
}