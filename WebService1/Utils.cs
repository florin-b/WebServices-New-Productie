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

        public static string getFilialaDistrib(string filiala)
        {
            return filiala.Substring(0, 2) + "1" + filiala.Substring(3, 1);
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

        public string getAdresaFiliala(string unitLog)
        {

           


            string retVal = "Str. Principala nr. 1#Galati#17#Galati#45.67#23.81";
            switch (unitLog)
            {
                case "BC10":
                case "BC20":
                case "BC40":
                    retVal = "Calea Moinesti 30H#Bacau#04#Bacau#46.55487582684765#26.90338310116168";
                    break;
                case "MM10":
                case "MM20":
                case "MM40":
                    retVal = "Str. Independentei nr. 80#Baia Mare#24#Maramures#47.656458552468685#23.515027451992797";
                    break;
                case "BV10":
                case "BV20":
                case "BV40":
                    retVal = "Str. Bucegi nr. 1#Brasov#08#Brasov#45.66067452455647#25.545861874340353";
                    break;
                case "CJ10":
                case "CJ20":
                case "CJ40":
                    retVal = "Str. Calea Floresti nr. 147-153#Cluj Napoca#12#Cluj#46.75454790717061#23.537448192185416";
                    break;
                case "CT10":
                case "CT20":
                case "CT40":
                    retVal = "B-dul Aurel Vlaicu nr. 171#Constanta#13#Constanta#44.18382945854235#28.595962231681764";
                    break;
                case "DJ10":
                case "DJ20":
                case "DJ40":
                    retVal = "B-dul Decebal nr. 111A#Craiova#16#Dolj#44.30038467977317#23.83460388880816";
                    break;
                case "VN10":
                case "VN20":
                case "VN40":
                    retVal = "Soseaua Vrancei Km 3#Focsani#39#Vrancea#45.716460259106384#27.16264003203066";
                    break;
                case "GL10":
                case "GL20":
                case "GL40":
                    retVal = "Str. Drumul de Centura nr. 39#Galati#17#Galati#45.4245602490232#28.005324518149003";
                    break;
                case "IS10":
                case "IS20":
                case "IS40":
                    retVal = "Comuna Miroslava, Sat Uricani, Trup Izolat, nr. 1#Iasi#22#Iasi#47.179582902583455#27.475228691348015";
                    break;
                case "NT10":
                case "NT20":
                case "NT40":
                    retVal = "Comuna Savinesti, Str. Uzinei, nr. 1#Piatra Neamt#27#Neamt#46.930562133577666#26.35957463676251";
                    break;
                case "BH10":
                case "BH20":
                case "BH40":
                    retVal = "Str. Calea Santandrei nr. 3A#Oradea#05#Bihor#47.047994795431116#21.895566522025952";
                    break;
                case "AG10":
                case "AG20":
                case "AG40":
                    retVal = "Comuna Bradu DN 65B#Pitesti#03#Arges#44.82454633607773#24.90829740165149";
                    break;
                case "PH10":
                case "PH20":
                case "PH40":
                    retVal = "Str. Poligonului nr. 5#Ploiesti#29#Prahova#44.95067400326734#25.99013215742942";
                    break;
                case "MS10":
                case "MS20":
                case "MS40":
                    retVal = "Str. Depozitelor nr. 26#Targu Mures#26#Mures#46.52375988748359#24.525497066622783";
                    break;
                case "TM10":
                case "TM20":
                case "TM40":
                    retVal = "Str. Calea Sagului nr. 205#Timisoara#35#Timis#45.71132392091665#21.192902041519208";
                    break;
                case "BU13":
                    retVal = "Soseaua Andronache nr. 203, Sector 2#Bucuresti#40#Bucuresti#44.47901744847611#26.159451897542727";
                    break;
                case "BU10":
                case "BU20":
                case "BU40":
                    retVal = "Str. Drumul intre Tarlale, nr. 61A, sector 3#Bucuresti#40#Bucuresti#44.408351161138505#26.21965945769171";
                    break;
                case "BU12":
                    retVal = "Str. Aleea Teisani, nr. 3-21, Sector 1#Bucuresti#40#Bucuresti#44.53443269618948#26.07030375717042";
                    break;
                case "BU11":
                    retVal = "Str. Drumul Osiei, nr. 8-16, Sector 6#Bucuresti#40#Bucuresti#44.43538503092358#25.989049968214726";
                    break;
                case "SV10":
                case "SV20":
                case "SV40":
                    retVal = "Str. Traian Vuia nr. 17#Suceava#33#Suceava#47.65715885314641#26.2591477";
                    break;
                case "HD10":
                case "HD20":
                case "HD40":
                    retVal = "Str. Dorobanţilor nr. 34#Deva#20#Hunedoara#45.871323015722076#22.92019456931883";
                    break;
                case "SB10":
                case "SB20":
                case "SB40":
                    retVal = "Soseaua Alba Iulia nr. 112#Sibiu#32#Sibiu#45.79036779008271#24.08786676824017";
                    break;
            }


            return retVal;

        }

        public static bool isUnitLogGed(string unitLog)
        {

            if (unitLog.Substring(2, 1).Equals("2") || unitLog.Substring(2, 1).Equals("4"))
                return true;
            else
                return false;

        }

        public static bool isUserTest_Str(string codUser)
        {
            if (codUser == null || codUser.Trim().Length == 0)
                return false;

            string agentiTest = "18768#59733#59566#60055#60185#59530#95180#140755#89378#86148#86608#86603#59867#60223#19061#60114#60289#59783#89328#71390#71440#59972";
            return agentiTest.Contains(codUser.TrimStart('0'));
        }

        public static bool isUserTest_false(string codUser, string nrVers)
        {
            return true;
        }


        public static bool isUserTestDB(string codUser, string nrVers)
        {

            if (codUser == null || codUser.Trim().Length == 0)
                return false;

            bool isUsrTest = false;
            int nrRecords = 0;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();
                
                {

                    cmd.CommandText = " select 1 from sapprd.zuseritestsfa where ltrim(cod_agent,'0') =:codAgent and vers in (" + nrVers + ")" ;

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codUser.TrimStart('0');

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        isUsrTest = true;
                    }
                    else
                        isUsrTest = false;

                }


            }
            catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return isUsrTest;
        }


    }
}