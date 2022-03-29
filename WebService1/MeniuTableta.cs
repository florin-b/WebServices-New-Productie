using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
using System.Data;


namespace WebService1
{
    public class MeniuTableta
    {

        public static bool savePinTableta(string codAgent, string codPin)
        {
            bool opSucces = true;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();

            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            string nowDate = year + month + day;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string query = " insert into sapprd.zmeniu_tableta (mandt, cod_angajat, cod_pin, blocat, data_op) values " +
                               " ('900',:codAgent, :codPin, :blocat, :dataOp) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgent;

                cmd.Parameters.Add(":codPin", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codPin;

                cmd.Parameters.Add(":blocat", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = " ";

                cmd.Parameters.Add(":dataOp", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = nowDate;

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                opSucces = false;
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }

            return opSucces;

        }


        public static bool deblocheazaMeniu(string codAgent, string codPin)
        {
            bool opSucces = true;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select 1 from sapprd.zmeniu_tableta where cod_angajat = :codAgent and cod_pin = :codPin ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgent;

                cmd.Parameters.Add(":codPin", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codPin;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    cmd.CommandText = " update sapprd.zmeniu_tableta set blocat = ' ' where cod_angajat = :codAgent  ";

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codAgent;

                    oReader = cmd.ExecuteReader();

                }
                else
                {
                    opSucces = false;
                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                opSucces = false;
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return opSucces;
        }




        public static bool blocheazaMeniu(string codAgent)
        {
            bool opSucces = true;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " update sapprd.zmeniu_tableta set blocat = 'X' where cod_angajat = :codAgent ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgent;

                oReader = cmd.ExecuteReader();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                opSucces = false;
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return opSucces;
        }



        public static string stareMeniuTableta(OracleConnection connection, string codAgent)
        {
            bool isBlocat = true;
            string codPin = "-1";

            if (codAgent == null || codAgent.Equals(""))
            {
                return "[" + isBlocat + "," + codPin + "]";
            }

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select blocat, cod_pin from sapprd.zmeniu_tableta where cod_angajat = :codAgent ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = string.Format("{0:d8}", Int32.Parse(codAgent));

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    isBlocat = oReader.GetString(0).Equals("X");
                    codPin = oReader.GetString(1);
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + ", codAgent = "+ codAgent);
                isBlocat = true;
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return "[" + isBlocat + "," + codPin + "]";
        }



        public static string stareMeniuTableta(string codAgent)
        {
            bool isBlocat = true;
            string codPin = "-1";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select blocat, cod_pin from sapprd.zmeniu_tableta where cod_angajat = :codAgent ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = string.Format("{0:d8}", Int32.Parse(codAgent)); ;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    isBlocat = oReader.GetString(0).Equals("X");
                    codPin = oReader.GetString(1);
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                isBlocat = true;
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return "[" + isBlocat + "," + codPin + "]";
        }





    }
}