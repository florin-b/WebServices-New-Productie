using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;

namespace WebService1
{
    public class OperatiiFurnizori
    {

        public string cautaFurnizorAndroid(string numeClient, string depart, string departAg, string unitLog, string tipCautare)
        {



            string serializedResult = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string sqlString = "  select a.lifnr, a.name1 from sapprd.lfa1 a, sapprd.lfb1 b, sapprd.wyt3 v where a.mandt = '900' and upper(a.name1) like upper('" + numeClient.Replace("'", "") + "%') " +
                                  "  and a.mandt = b.mandt and a.lifnr = b.lifnr and b.bukrs = '1000' and a.mandt = v.mandt and a.lifnr = v.lifnr and v.parvw = 'RS' and v.lifnr = v.lifn2 ";

                if (tipCautare != null && tipCautare.Equals("COD_ARTICOL"))
                    sqlString = " select distinct a.lifnr, a.name1 from sapprd.lfa1 a, sapprd.lfb1 b, sapprd.wyt3 v, sapprd.eina e where a.mandt = '900' and e.mandt = '900' " +
                                " and a.lifnr = e.lifnr  and e.matnr like '0000000000" + numeClient + "%' and e.loekz <> 'X' " +
                                " and a.mandt = b.mandt and a.lifnr = b.lifnr and b.bukrs = '1000' and a.mandt = v.mandt and a.lifnr = v.lifnr and v.parvw = 'RS' and v.lifnr = v.lifn2 order by a.name1 ";


                cmd.CommandText = sqlString;


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                List<Furnizor> listaFurnizori = new List<Furnizor>();
                Furnizor unFurnizor = null;

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {

                        unFurnizor = new Furnizor();
                        unFurnizor.numeFurnizor = oReader.GetString(1);
                        unFurnizor.codFurnizor = oReader.GetString(0);
                        listaFurnizori.Add(unFurnizor);

                    }

                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaFurnizori);

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());

            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }


            return serializedResult;

        }


    }
}