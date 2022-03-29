using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data;

namespace FlotaService
{
    public class OperatiiTablete
    {


        public string addTabletaSofer(string codSofer, string codTableta, string creatDe)
        {
            string opResult = "";

            OracleConnection connection = new OracleConnection();
           
            string connectionString = DatabaseConnections.ConnectToProdEnvironment();
            string query = "";

            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            string nowDate = year + month + day;

            string hour = cDate.Hour.ToString("00");
            string minute = cDate.Minute.ToString("00");
            string sec = cDate.Second.ToString("00");
            string nowTime = hour + minute + sec;

            try
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                query = " update sapprd.ztabletesoferi set stare = '0' where (codtableta =:codtableta or codsofer = :codsofer) ";
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codtableta", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codTableta;

                cmd.Parameters.Add(":codSofer", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.ExecuteNonQuery();

                query = " insert into sapprd.ztabletesoferi(mandt, codsofer, codtableta, datainreg, stare, creatde, orainreg) " +
                        " values ('900', :codsofer, :codtableta, :datainreg, :stare, :creatde, :orainreg) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codSofer", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                cmd.Parameters.Add(":codtableta", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codTableta;

                cmd.Parameters.Add(":datainreg", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = nowDate;

                cmd.Parameters.Add(":stare", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = "1";

                cmd.Parameters.Add(":creatde", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = creatDe;

                cmd.Parameters.Add(":orainreg", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = nowTime;

                cmd.ExecuteNonQuery();
                opResult = "1";

                return "1";

            }
            catch (Exception ex)
            {
                opResult = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return opResult;

        }


        public string removeTabletaSofer(string codSofer, string creatDe)
        {
            string opResult = "";

            OracleConnection connection = new OracleConnection();

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();
            string query = "";


            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            string nowDate = year + month + day;

            try
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                query = " update sapprd.ztabletesoferi set stare = '0' where codsofer =:codsofer  ";
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codSofer", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                cmd.ExecuteNonQuery();



            }
            catch (Exception ex)
            {
                opResult = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return opResult;

        }



        public string getTableteSofer(string codSofer)
        {
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            try
            {


                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                string query = "";

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                query = " select codtableta, to_char(to_date(datainreg,'yyyymmdd')) datainreg, decode(stare,'1','Activ','Inactiv') stare from sapprd.Ztabletesoferi where codsofer =:codsofer order by datainreg desc";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codSofer", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                oReader = cmd.ExecuteReader();

                List<TabletaSofer> listTablete = new List<TabletaSofer>();
                TabletaSofer tableta = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        tableta = new TabletaSofer();
                        tableta.idTableta = oReader.GetString(0);
                        tableta.dataInreg = oReader.GetString(1);
                        tableta.stare = oReader.GetString(2);
                        listTablete.Add(tableta);
                    }

                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listTablete);

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }


           

            return serializedResult;
        }



    }
}