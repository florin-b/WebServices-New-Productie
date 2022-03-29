
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data;
using System.Globalization;

namespace FlotaService
{
    public class OperatiiDocumente
    {

        public string getBorderouri(string codSofer, string dataStart, string dataStop)
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

                cmd.CommandText = " select numarb, to_char(data_e) from borderouri a where  cod_sofer =:codSofer and data_e between to_date('" + dataStart + "','yyyymmdd') " +
                                  " and to_date('" + dataStop + "','yyyymmdd') order by data_e ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                oReader = cmd.ExecuteReader();

                Borderou borderou = null;
                List<Borderou> listBorderouri = new List<Borderou>();
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        borderou = new Borderou();
                        borderou.cod = oReader.GetString(0);
                        borderou.dataEmitere = oReader.GetString(1);
                        listBorderouri.Add(borderou);
                    }
                }


                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listBorderouri);


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



        public string getCoordonateGPSClientiBorderou(string codBorderou)
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

                cmd.CommandText = " select  a.cod_client, b.coordonategps, c.nume from sapprd.zdocumentesms a, sapprd.zadresagpsclient b, clienti c " +
                                  " where a.nr_bord=:codBorderou and b.codclient = a.cod_client and c.cod = a.cod_client order by a.poz";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codBorderou", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codBorderou;

                oReader = cmd.ExecuteReader();

                PozitieClient pozitieClient = null;
                List<PozitieClient> listClienti = new List<PozitieClient>();
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        pozitieClient = new PozitieClient();
                        pozitieClient.codClient = oReader.GetString(0);
                        pozitieClient.latitudine = oReader.GetString(1).Split(',')[0];
                        pozitieClient.longitudine = oReader.GetString(1).Split(',')[1];
                        pozitieClient.numeClient = oReader.GetString(2);
                        listClienti.Add(pozitieClient);
                    }
                }


                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listClienti);


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



        public string getTraseuBorderou(string codBorderou)
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

                cmd.CommandText = " select to_char(c.record_time,'dd-Mon-yy hh24:mi:ss') , c.latitude, c.longitude, nvl(c.mileage,0) , nvl(c.speed,0) from borderouri a, gps_masini b, gps_date c  where a.numarb =:codBorderou " +
                                  " and b.nr_masina = replace(a.masina,'-','') and c.device_id = b.id " +
                                  " and  trunc(c.record_time) >= trunc(a.data_e) order by c.record_time  ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codBorderou", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codBorderou;

                oReader = cmd.ExecuteReader();

                TraseuBorderou pozitie = null;
                List<TraseuBorderou> listPozitii = new List<TraseuBorderou>();
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        pozitie = new TraseuBorderou();
                        pozitie.dataInreg = oReader.GetString(0);
                        pozitie.latitudine = oReader.GetDouble(1).ToString();
                        pozitie.longitudine = oReader.GetDouble(2).ToString();
                        pozitie.km = oReader.GetInt32(3).ToString();
                        pozitie.viteza = oReader.GetInt32(4).ToString();
                        listPozitii.Add(pozitie);
                    }
                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listPozitii);

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




        private string formatDate(string date)
        {
            return date.Substring(6, 2) + "-" + date.Substring(4, 2) + "-" + date.Substring(0, 4);
        }



    }

}