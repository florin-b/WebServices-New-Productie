using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OracleClient;
using System.Web.Script.Serialization;

namespace WebService1
{
    public class ObiectiveConsilieri
    {

        public string getListObiective(string codConsilier, string numeObiectiv)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string condNume = "";

            if (numeObiectiv != null && numeObiectiv.Trim().Length > 0)
                condNume = " and lower(name) like lower('" + numeObiectiv + "%')";

            List<ObiectivConsilier> listObiective = new List<ObiectivConsilier>();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select ora_id, name, creation_date, address, region_id, gps " +
                                  " from SAPPRD.ztbl_objectives where cva_code =:codConsilier and status = 1 " + condNume + " order by creation_date ";
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codConsilier", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codConsilier;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ObiectivConsilier obiectiv = new ObiectivConsilier();
                        obiectiv.id = oReader.GetDecimal(0).ToString();
                        obiectiv.beneficiar = oReader.GetString(1);
                        obiectiv.dataCreare = oReader.GetString(2);
                        obiectiv.adresa = oReader.GetString(3);
                        obiectiv.codJudet = oReader.GetString(4);
                        obiectiv.coordGps = oReader.GetString(5).Replace("(", "").Replace(")", "").Trim();
                        listObiective.Add(obiectiv);

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


            return new JavaScriptSerializer().Serialize(listObiective);
        }

    }
}