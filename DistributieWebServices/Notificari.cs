using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using System.Net.Mail;

namespace DistributieWebServices
{
    public class Notificari
    {

        public void depozit(string tipNotificare, string codSofer, string borderou)
        {
            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;
            string query;
            string textToSend = "";
            string numeSofer = "", telSofer = "";

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                query = " select nume, telefon from soferi where cod=:cod ";
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":cod", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                cmd.CommandText = query;
                oReader = cmd.ExecuteReader();

                numeSofer = "";
                telSofer = "";
                if (oReader.HasRows)
                {
                    oReader.Read();

                    numeSofer = oReader.GetString(0);
                    telSofer = oReader.GetString(1);

                    string dataCautare = DateTime.Today.AddDays(-10).ToString("dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);

                    query = " select distinct b.nume ,b.cod codcl, c.nume numeag, a.nr_bord, a.pers_cont,to_number(a.poz) poz,  " +
                            " a.name1, c.filiala from sapprd.ZDOCUMENTESMS a,clienti b, agenti c where " +
                            " a.nr_bord = :brd and a.tip=2 " +
                            " and a.cod_av = c.cod(+) and a.cod_client=b.cod(+) order by numeag,to_number(a.poz) ";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":brd", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = borderou;

                    cmd.CommandText = query;
                    oReader = cmd.ExecuteReader();

                    HashSet<string> mailAgenti = new HashSet<string>();
                    string numeClient = "";
                    textToSend = "";
                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            if (oReader.GetString(6).ToString().Trim().Length > 0)
                            {
                                numeClient = oReader.GetString(6);
                            }
                            else
                            {
                                numeClient = oReader.GetString(0);
                            }

                            if (textToSend.Length == 0)
                            {
                                textToSend += numeClient + "-" + oReader.GetInt32(5).ToString();
                            }
                            else
                            {
                                textToSend += ", " + numeClient + "-" + oReader.GetInt32(5).ToString();
                            }

                            mailAgenti.Add(getMailAddr(oReader.GetString(2), oReader.GetString(7)));

                        }
                    }

                    oReader.Close();
                    oReader.Dispose();

                    string mailText = numeSofer + "-" + telSofer + " Marfa pentru " + textToSend + " a iesit pe poarta.";

                    foreach(string adresaMail in mailAgenti)
                    {
                        sendMail(adresaMail, mailText);
                    }



                }

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

           

            return;


        }


        public void client(string codSofer, string nrDocument, string codClient)
        {

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;
            string numeSofer = "", telSofer = "";
            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                string query = " select nume, telefon from soferi where cod=:cod ";
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":cod", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                cmd.CommandText = query;
                oReader = cmd.ExecuteReader();

                numeSofer = "";
                telSofer = "";
                if (oReader.HasRows)
                {
                    oReader.Read();
                    numeSofer = oReader.GetString(0);
                    telSofer = oReader.GetString(1);

                    query = " select distinct c.nume, a.city1, ag.nume, ag.filiala, s.name1 from sapprd.zdocumentesms s, clienti c, sapprd.adrc a, agenti ag " +
                            " where s.nr_bord=:nrBord and s.cod_client =:codClient and c.cod= s.cod_client " +
                            " and a.client = '900' and a.addrnumber = s.adresa_client and ag.cod = s.cod_av ";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":nrBord", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = nrDocument;

                    cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codClient;

                    cmd.CommandText = query;
                    oReader = cmd.ExecuteReader();


                    string mailText = "", numeClient = "";
                    if (oReader.HasRows)
                    {

                        while (oReader.Read())
                        {
                            if (oReader.GetString(4).Trim().Length > 0)
                                numeClient = oReader.GetString(4);
                            else
                                numeClient = oReader.GetString(0);

                            mailText = numeSofer + "-" + telSofer + ". S-a livrat " + numeClient + " din " + oReader.GetString(1) + ".";
                            sendMail(getMailAddr(oReader.GetString(2), oReader.GetString(3)), mailText);
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



        }


        public void sendSmsFromBuffer()
        {
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            List<string> listBorderouri = new List<string>();
            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment()  ;

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select document from sapprd.zbuffersmsclient t ";

                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        listBorderouri.Add(oReader.GetString(0));
                    }
                }

                OperatiiEvenimente opEvenimente = new OperatiiEvenimente();
                foreach (string borderou in listBorderouri)
                {
                    if (Utils.itsTimeToSendSmsAlert())
                    {
                        new Sms().sendSMS(opEvenimente.getClientsPhoneNumber(connection, borderou), borderou);
                        stergeSmsFromBuffer(connection, borderou);
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



        }


        private void stergeSmsFromBuffer(OracleConnection connection, string document)
        {
            OracleCommand cmd = connection.CreateCommand();

            try
            {
                cmd.CommandText = " delete from sapprd.zbuffersmsclient where document=:document ";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":document", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = document;

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + document);
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }



        }


        private string getNumeFiliala(string codFiliala)
        {
            string numeFiliala = "";

            switch (codFiliala.ToLower())
            {
                case "bu10":
                    numeFiliala = "glina";
                    break;
                case "bu11":
                    numeFiliala = "bm";
                    break;
                case "bu12":
                    numeFiliala = "otopeni";
                    break;
                case "bu13":
                    numeFiliala = "ba";
                    break;
                default:
                    numeFiliala = codFiliala.ToLower().Substring(0, 2);
                    break;
            }

            return numeFiliala;
        }



        public string getMailAddr(string numeAgent, string codFiliala)
        {
            string[] arrayNume = numeAgent.Trim().Split(' ');
            string[] nume = arrayNume[0].Trim().Replace('-', ' ').Split(' ');
            string[] prenume = arrayNume[1].Trim().Replace('-', ' ').Split(' ');
            return prenume[0].ToLower() + "." + nume[0].ToLower() + "@" + getNumeFiliala(codFiliala) + ".arabesque.ro";
        }


        private static void sendMail(string mailDest, string mailMessage)
        {

            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress("Notificari.Distributie@arabesque.ro");
                message.To.Add(mailDest);
                message.Subject = "Notificare distributie marfa";
                message.Body = mailMessage;
                SmtpClient client = new SmtpClient("mail.arabesque.ro");
                client.Send(message);
            }
            catch (Exception)
            {

            }

        }


    }
}