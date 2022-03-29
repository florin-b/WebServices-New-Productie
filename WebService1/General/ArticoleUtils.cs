using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OracleClient;

namespace WebService1.General
{
    public class ArticoleUtils
    {

        public static bool isMaterialServiciiWood(string codArticol)
        {

            return codArticol.StartsWith("00000000003010");


        }


        public static string getUmServicii(OracleConnection connection, string codArticol)
        {
            string umServ = "BUC";
            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select um from articole where cod = :codArticol ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codArticol", OracleType.NVarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;

                oReader = cmd.ExecuteReader();


                if (oReader.HasRows)
                {
                    oReader.Read();
                    umServ = oReader.GetString(0);
                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return umServ;
        }



    }
}