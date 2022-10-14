using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;

namespace WebService1
{
    public class Utils
    {
        public static string getFilialaGed(string filiala)
        {
            return filiala.Substring(0, 2) + "2" + filiala.Substring(3, 1);
        }

        public static string getCurrentDate()
        {
            string mDate = "";
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            mDate = year + month + day;
            return mDate;
        }

        public static string getCurrentTime()
        {
            string mTime = "";
            DateTime cDate = DateTime.Now;
            string hour = cDate.Hour.ToString("00");
            string minute = cDate.Minute.ToString("00");
            string sec = cDate.Second.ToString("00");
            mTime = hour + minute + sec;
            return mTime;
        }

        public static String getCurrentMonth()
        {
            DateTime cDate = DateTime.Now;
            string month = cDate.Month.ToString("00");
            return month;
        }


        public static String getCurrentYear()
        {
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            return year;
        }


        public static string getDescTipTransport(string codTransport)
        {

            string tipTransport = "";

            if (codTransport.Equals("TCLI"))
            {
                tipTransport = "client";
            }
            if (codTransport.Equals("TRAP"))
            {
                tipTransport = "Arabesque";
            }
            if (codTransport.Equals("TERT"))
            {
                tipTransport = "terti";
            }

            return tipTransport;

        }

        public static bool isMathausMare(string filiala)
        {
            return filiala.Equals("AG10") || filiala.Equals("BU10") || filiala.Equals("IS10");
        }


        public static bool isMathausMic(string filiala)
        {
            return filiala.Equals("GL10") || filiala.Equals("CT10") || filiala.Equals("DJ10") || filiala.Equals("BH10");
        }

        public static string getYearFromStrDate(string strDate)
        {
            if (strDate == null || strDate.Trim().Length == 0)
                return getCurrentYear();

            return strDate.Substring(0, 4);

        }


        public static string getMonthFromStrDate(string strDate)
        {
            if (strDate == null || strDate.Trim().Length == 0)
                return getCurrentMonth();

            return strDate.Substring(4, 2);
        }


        public static List<string> getMail(Sms.TipUser[] tipUser, String filiala)
        {

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;
            List<String> listMails = new List<string>();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                OracleCommand cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();


                foreach (Sms.TipUser tip in tipUser)
                {
                    cmd.CommandText = " select mail from sapprd.zdest_mail where funct=:tipUser and prctr =:filiala ";

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":tipUser", OracleType.VarChar, 36).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = tip;

                    cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = filiala;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            if (oReader.GetString(0).Trim().Length > 0)
                                listMails.Add(oReader.GetString(0));
                        }
                    }
                }

                oReader.Close();
                oReader.Dispose();



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

            return listMails;
        }



        public static string checkROCnpClient(string cnpClient)
        {
            if (cnpClient == null)
                return " ";

            string localCnp = cnpClient;

            if (cnpClient.ToUpper().StartsWith("RO"))
            {
                localCnp = cnpClient.ToUpper().Replace("RO", "");
                localCnp = "RO" + localCnp;
            }

            return localCnp;
        }

        public static bool isFilialaMica04(string filiala, string departament)
        {

            if (filiala == null)
                return false;
                                                
            if (departament.StartsWith("04") && (filiala.Equals("HD10") || filiala.Equals("VN10") || filiala.Equals("BU12") || filiala.Equals("BZ10") || filiala.Equals("MS10") || filiala.Equals("BC10") || filiala.Equals("CT10") || filiala.Equals("TM10") || filiala.Equals("CJ10") || filiala.Equals("SB10") || filiala.Equals("AG10") || filiala.Equals("NT10") || filiala.Equals("BV10") || filiala.Equals("DJ10") || filiala.Equals("BU11") || filiala.Equals("MM10") || filiala.Equals("BH10") || filiala.Equals("PH10") || filiala.Equals("GL10")))
                return true;

            return false;


        }

        public static bool isFilialaMicaDep04(OracleConnection connection, string filiala, string depart)
        {

            bool isFilialaMica = false;

            if (filiala == null || !depart.StartsWith("04"))
                return false;
            else
            {
                OracleCommand cmd = new OracleCommand();
                OracleDataReader oReader = null;

                cmd = connection.CreateCommand();

                cmd.CommandText = " select count(distinct cod) from agenti where activ = 1 and divizie like '04%' and tip = 'SD' and filiala = :filiala ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    if (oReader.GetInt32(0) == 1)
                        isFilialaMica = true;
                    else
                        isFilialaMica = false;

                }
                else
                    isFilialaMica = false;

                DatabaseConnections.CloseConnections(oReader, cmd);

                return isFilialaMica;
            }
        }

        public static bool isFilialaMicaDep04(string filiala, string depart)
        {

            bool isFilialaMica = false;

            if (filiala == null || !depart.StartsWith("04"))
                return false;
            else
            {
                OracleConnection connection = new OracleConnection();
                OracleCommand cmd = new OracleCommand();
                OracleDataReader oReader = null;

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select count(distinct cod) from agenti where activ = 1 and divizie like '04%' and  tip = 'SD' and filiala = :filiala ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    if (oReader.GetInt32(0) == 1)
                        isFilialaMica = true;
                    else
                        isFilialaMica = false;

                }
                else
                    isFilialaMica = false;

                DatabaseConnections.CloseConnections(oReader, cmd, connection);

                return isFilialaMica;
            }
        }



        public static string getTipConsilier(OracleConnection connection, string codConsilier)
        {
            string tipConsilier = "NN";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select tip from agenti where cod =:codAgent and upper(tip) like 'C%' ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codConsilier.Trim();

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    tipConsilier = oReader.GetString(0);

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


            return tipConsilier;
        }

        public static string getDepartArticol(OracleConnection connection, string codArticol)
        {
            string departament = "00";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            if (codArticol.Length == 8)
                codArticol = "0000000000" + codArticol;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select grup_vz from articole where cod =:codArt ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    departament = oReader.GetString(0).Trim();

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


            return departament;
        }



        public static string getNumeClient(OracleConnection connection, string codClient)
        {
            string numeClient = "Nedefinit";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select nume from clienti where cod =:codClient ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    numeClient = oReader.GetString(0).Trim();
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


            return numeClient;
        }

        public static string formatDateToSap(string date)
        {
            string[] toksDate = date.Split('.');
            return toksDate[2] + toksDate[1] + toksDate[0];

        }

        public static string getTipAngajat(string codAngajat)
        {
            string tipAngajat = "NN";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select tip from agenti where cod =:codAgent ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAngajat.Trim();

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    tipAngajat = oReader.GetString(0);

                }


                oReader.Close();
                oReader.Dispose();


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


            return tipAngajat;
        }


        public static string getTipAngajat(OracleConnection connection, OracleTransaction transaction, string codAngajat)
        {
            string tipAngajat = "NN";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select tip from agenti where cod =:codAgent ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAngajat.Trim();

                cmd.Transaction = transaction;
                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    tipAngajat = oReader.GetString(0);

                }


                oReader.Close();
                oReader.Dispose();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                cmd.Dispose();
            }


            return tipAngajat;
        }

        public static bool isMatTransport(string codArt)
        {
            bool isMat = false;

            string[] articolePermise = { "000000000030101220", "000000000030101221", "000000000030101223", "000000000030101224", "000000000030101225", "000000000030101226", "000000000030101227", "000000000030101228", "000000000030101230", "000000000030101222", "000000000030101111", "000000000030101240" };

            for (int i = 0; i < articolePermise.Length; i++)
            {
                if (articolePermise[i].Equals(codArt))
                {
                    isMat = true;
                    break;
                }
            }


            return isMat;
        }

        public static string getCodAngajat(OracleConnection connection, string numeAngajat, string filiala)
        {
            string codAngajat = "00000000";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select cod from agenti where activ = '1' and upper(nume) =:numeAngajat and substr(filiala,0,2) = :filiala ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":numeAngajat", OracleType.VarChar, 120).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = numeAngajat.ToUpper().Trim();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filiala.Substring(0, 2);

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    codAngajat = oReader.GetString(0);
                }

                oReader.Close();
                oReader.Dispose();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                cmd.Dispose();
            }

            return codAngajat;
        }


        public static bool itsTimeToSendSmsAlert()
        {

            DateTime t1 = DateTime.Now;

            DateTime t2 = Convert.ToDateTime("07:00:00 AM");

            DateTime t3 = Convert.ToDateTime("21:30:00 PM");

            int i = DateTime.Compare(t1, t2);

            int i1 = DateTime.Compare(t1, t3);

            if (i < 0 || i1 > 0)
                return false;


            return true;
        }


    }
}