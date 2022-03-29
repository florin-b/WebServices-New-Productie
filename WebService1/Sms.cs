using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using WebService1.SMSService;
using System.Net;

namespace WebService1
{
    public class Sms
    {

        public enum TipUser { SM, CVS };

        private string numeClient;
        private string codClient;


        public void setNumeClient(String numeClient)
        {
            this.numeClient = numeClient;
        }

        public void setCodClient(String codClient)
        {
            this.codClient = codClient;
        }

        public string getNumeClient()
        {

            if (numeClient == null)
            {
                OperatiiClienti opClienti = new OperatiiClienti();
                numeClient = opClienti.getNumeClient(codClient);
            }

            return numeClient;
        }

        public void sendSMS(TipUser tipUser, string filiala)
        {

            List<String> listTelefoane = getListNrTelefon(tipUser, filiala);
            System.Net.ServicePointManager.Expect100Continue = false;
            string smsMessage = getSmsMessage(tipUser);

            if (listTelefoane.Count > 0)
            {

                try
                {
                    DateTime dateTime = new DateTime();
                    SMSService.SMSServiceService smsService = new SMSServiceService();
                    string sessionId = smsService.openSession("arabesque2", "arbsq123");

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                    OracleConnection connection = new OracleConnection();
                    connection.ConnectionString = connectionString;
                    connection.Open();

                    string respSms = "";

                    for (int i = 0; i < listTelefoane.Count; i++)
                    {
                          respSms = smsService.sendSession(sessionId, listTelefoane[i], smsMessage, dateTime, "", 0);
                          Sms.logSms(connection, listTelefoane[i], smsMessage, respSms);
                    }


                    connection.Close();

                    smsService.closeSession(sessionId);
                }
                catch (Exception ex)
                {
                    ErrorHandling.sendErrorToMail(ex.ToString());
                }
            }

            return;
        }



        private string getSmsMessage(TipUser tipUser)
        {
            string smsMessage = "";

            if (tipUser == TipUser.SM || tipUser == TipUser.CVS)
            {
                smsMessage = "S-a creat o comanda cu marfa din MAV1 pentru clientul " + getNumeClient();
            }

            return smsMessage;
        }




        private List<string> getListNrTelefon(TipUser tipUser, String filiala)
        {

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;
            List<String> listNrTel = new List<string>();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                OracleCommand cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd.CommandText = " select nrtel from agenti where tip=:tipUser and filiala =:filiala and activ = 1 ";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":tipUser", OracleType.VarChar, 36).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = tipUser;

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filiala;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        if (oReader.GetString(0).Trim().Length > 0)
                            listNrTel.Add(oReader.GetString(0));
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

            return listNrTel;
        }



        public static void logSms(string nrTel, string smsText, string response)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                string query = " insert into sapprd.zsmslog (mandt,nrtel,datac,orac,sms,resp) " +
                               " values ('900',:nrTel, :datac, :orac ,:sms ,:resp ) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrTel", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrTel;

                cmd.Parameters.Add(":datac", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = getNowDate();

                cmd.Parameters.Add(":orac", OracleType.NVarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = getNowTime();

                cmd.Parameters.Add(":sms", OracleType.NVarChar, 300).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = smsText;

                cmd.Parameters.Add(":resp", OracleType.NVarChar, 210).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = response;

                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + nrTel + " , " + smsText);
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }

        }



        public static void logSms(OracleConnection conn, string nrTel, string smsText, string response)
        {

            OracleCommand cmd = null;

            try
            {
                cmd = conn.CreateCommand();

                string query = " insert into sapprd.zsmslog (mandt,nrtel,datac,orac,sms,resp) " +
                               " values ('900',:nrTel, :datac, :orac ,:sms ,:resp ) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrTel", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrTel;

                cmd.Parameters.Add(":datac", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = getNowDate();

                cmd.Parameters.Add(":orac", OracleType.NVarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = getNowTime();

                cmd.Parameters.Add(":sms", OracleType.NVarChar, 300).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = smsText;

                cmd.Parameters.Add(":resp", OracleType.NVarChar, 210).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = response;

                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }



        }




        private static string getNowDate()
        {
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            string nowDate = year + month + day;

            return nowDate;

        }



        private static string getNowTime()
        {
            DateTime cDate = DateTime.Now;
            string hour = cDate.Hour.ToString("00");
            string minute = cDate.Minute.ToString("00");
            string sec = cDate.Second.ToString("00");
            string nowTime = hour + minute + sec;

            return nowTime;

        }





    }
}