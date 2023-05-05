using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using DistributieWebServices.SMSService;
using System.Globalization;
using System.Web.Script.Serialization;

namespace DistributieWebServices
{
    public class Sms
    {

        private string filialaDocument = "";


        public void sendSMS(List<NotificareClient> listNotificari, String nrDocument)
        {

            System.Net.ServicePointManager.Expect100Continue = false;

            if (listNotificari.Count > 0)
            {

                try
                {
                    DateTime dateTime = new DateTime();
                    SMSService.SMSServiceService smsService = new SMSServiceService();
                    string sessionId = smsService.openSession("arabesque2", "arbsq123");

                    string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                    OracleConnection connection = new OracleConnection();
                    connection.ConnectionString = connectionString;
                    connection.Open();


                    foreach (NotificareClient notificare in listNotificari)
                    {

                        string textComanda = "Comanda";
                        string prep = "va";

                        if (notificare.dateComanda.emitere.Contains(","))
                        {
                            textComanda = "Comenzile";
                            prep = "vor";
                        }

                        string textDepartament = " din departamentul ";

                        if (notificare.dateComanda.departament.Contains(","))
                            textDepartament = " din departamentele ";
                        else if (notificare.dateComanda.departament.Trim().Length == 0)
                            textDepartament = " ";

                        string dataDocument = " din " + notificare.dateComanda.emitere.ToLower().Trim();

                        if (notificare.dateComanda.emitere.Trim().Length == 0)
                            dataDocument = " ";

                        string statusLink = getStatusLink(nrDocument, notificare);

                        

                        string msgExtra = textComanda + " dumneavoastra" + dataDocument + textDepartament + notificare.dateComanda.departament +
                                                        " se " + prep + " livra astazi, " + getRoCurrentDate() + statusLink + ". Va multumim! ";

                        string smsStatus = " ";

                        try
                        {
                            smsStatus = smsService.sendSession(sessionId, notificare.nrTelefon, msgExtra, dateTime, "", 0);
                            Sms.logSms(connection, notificare.nrTelefon, msgExtra, smsStatus, nrDocument);
                            Sms.logSmsBorderou(connection, nrDocument, notificare.codClient, notificare.codAdresa, notificare.nrTelefon);

                        }
                        catch (Exception ex)
                        {
                            smsStatus = ex.ToString() + " , client " + notificare.codClient + " , telefon " + notificare.nrTelefon;
                        }


                    }

                    connection.Close();
                    smsService.closeSession(sessionId);

                }
                catch (Exception ex)
                {
                    ErrorHandling.sendErrorToMail("sendSMS: " + ex.ToString() + " , " + nrDocument);
                }
            }

            return;
        }



        public string getStatusLink(string nrDocument, NotificareClient notificare)
        {

            string statusLink = "";
            string idAdresa = "";
            string telefonSofer = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            bool clientMat = false;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                telefonSofer = getTelefonSofer(connection, nrDocument, notificare.codClient, notificare.codAdresa);

                cmd = connection.CreateCommand();


                cmd.CommandText = " select i.latitude, i.longitude, b.fili from gps_masini g, borderouri b, gps_index i where " +
                                  " replace(g.nr_masina,'-','')=  replace(b.masina,'-','') and " +
                                  " b.numarb=:nrBord and b.sttrg in ('2','4','6') and g.id = i.device_id ";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":nrBord", OracleType.VarChar, 30).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                oReader = cmd.ExecuteReader();

                double latMasina = 0.0;
                double longMasina = 0.0;

                if (oReader.HasRows)
                {
                    oReader.Read();
                    latMasina = oReader.GetDouble(0);
                    longMasina = oReader.GetDouble(1);
                    filialaDocument = oReader.GetString(2);
                }



                cmd.CommandText = " select ad.latitude, ad.longitude, c.nume from sapprd.zadreseclienti ad, " +
                                  " sapprd.zdocumentebord z, clienti c  where z.nr_bord=:nrBord and c.cod = z.cod " +
                                  " and z.cod = ad.codclient and z.adresa = ad.codadresa and z.cod=:codClient " +
                                  " and upper(c.nume) not like '%PF VANZARE%' and upper(c.nume) not like '%BON FISCAL%'";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":nrBord", OracleType.VarChar, 30).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = notificare.codClient;


                oReader = cmd.ExecuteReader();

                double latClient = 0.0;
                double longClient = 0.0;

                if (oReader.HasRows)
                {
                    oReader.Read();
                    latClient = Double.Parse(oReader.GetString(0));
                    longClient = Double.Parse(oReader.GetString(1));
                    idAdresa = notificare.codAdresa;
                }
                else
                {

                    if (notificare.codAdresa != "0")
                    {

                        cmd.CommandText = " select distinct c.latitude, c.longitude from sapprd.zcoordcomenzi c, sapprd.zdocumentesms d where " +
                                          " d.mandt = '900' and c.mandt = '900' and d.adresa_client =:codAdresa and d.nr_bord =:codBorderou and c.idcomanda = d.idcomanda ";

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(":codAdresa", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = notificare.codAdresa;

                        cmd.Parameters.Add(":codBorderou", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[1].Value = nrDocument;

                        oReader = cmd.ExecuteReader();

                        if (oReader.HasRows)
                        {
                            oReader.Read();
                            latClient = Double.Parse(oReader.GetString(0));
                            longClient = Double.Parse(oReader.GetString(1));
                            idAdresa = notificare.codAdresa;
                        }
                        else
                        {
                            string[] coords = MapsUtils.getCoordonateClient(connection, idAdresa).Split('#');

                            idAdresa = notificare.codAdresa;
                            latClient = Double.Parse(coords[0]);
                            longClient = Double.Parse(coords[1]);

                            clientMat = true;
                             
                        }



                    }


                }


                if (latMasina > 0 && latClient > 0)
                {
                    statusLink = ". Pentru a afla pozitia gps a camionului accesati https://client.arabesque.ro/info-livrare/stat?p=" + nrDocument + "-" + idAdresa;

                   // if (clientMat)
                   //     ErrorHandling.sendErrorToMail(statusLink);


                }

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


            if (telefonSofer.Length >= 10)
            {
                statusLink = ". Tel " + telefonSofer + statusLink;
            }

            return statusLink;
        }


        public string getTelefonSofer(OracleConnection connection, string nrBord, string codClient, string codAdresa)
        {
            
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string telefonSofer = "";

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select  d.telefon, c.tip, a.tip_livrare from sapprd.zdocumentesms a, borderouri b, agenti c, soferi d where a.nr_bord =:nrBord " +
                                  " and a.nr_bord = b.numarb and a.cod_client =:codClient and a.adresa_client =:codAdresa and c.cod = a.cod_av and d.cod = b.cod_sofer ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":nrBord", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrBord;

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codClient;

                cmd.Parameters.Add(":codAdresa", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codAdresa;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        if (oReader.GetString(1).ToUpper().Equals("CVO") || oReader.GetString(2).ToUpper().Equals("ZLFH"))
                        {
                            telefonSofer = oReader.GetString(0);
                        }
                    }
                }


            }
            catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return telefonSofer;
        }

        public void sendSmsToBuffer(OracleConnection connection, string document)
        {

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " insert into sapprd.zbuffersmsclient(mandt, document) values ('900', :document) ";
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

        public string getRoCurrentDate()
        {
            CultureInfo ci = new CultureInfo("ro-RO");
            return DateTime.Now.ToString("M", ci).ToLower();

        }




        public string getTelClienti()
        {
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<string> listTelefon = new List<string>();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select count(*) from " +
                                  " (select distinct  k.kunnr, k.name1, " +
                                  " nvl((select a.tel_number from sapprd.adr2 a, sapprd.adrt t where a.client = '900' and a.client = t.client and a.addrnumber = t.addrnumber and a.consnumber = t.consnumber and a.persnumber = t.persnumber and t.comm_type = 'TEL' and a.addrnumber = k.adrnr and lower(t.remark) like 'prom%' and rownum = 1), " +
                                  " '0') telefon from sapprd.kna1 k, sapprd.knvv v, sapprd.t005u u where k.mandt = '900' and k.mandt = v.mandt and k.kunnr = v.kunnr " +
                                  " and k.ktokd = '1000' and k.mandt = u.mandt  and k.regio = u.bland  and u.land1 = 'RO'  and u.spras = '4') where(nvl(telefon, '0') <> '0') ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        if (oReader.GetString(2).Trim().Length >= 10)
                            listTelefon.Add(oReader.GetString(2));
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


            return new JavaScriptSerializer().Serialize(listTelefon);

        }



        public static void logSms(OracleConnection conn, string nrTel, string smsText, string response, string nrDocument)
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
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " +  nrTel + " , " +  smsText + " , " + response + " , " + nrDocument);
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }



        }


        public static void logSmsBorderou(OracleConnection conn, string nrBorderou, string codClient, string codAdresa, string nrtel)
        {

            OracleCommand cmd = null;

            try
            {
                cmd = conn.CreateCommand();

                string query = " insert into sapprd.zsmsclienti (mandt, codsofer, borderou, codclient, codadresa, dataexp, oraexp, nrtel ) " +
                               " values ('900',(select cod_sofer from borderouri where numarb = :numarb), :numarb, :codclient, :codadresa, to_char(sysdate,'yyyymmdd'), to_char(sysdate,'hh24mi'), :nrtel ) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":numarb", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrBorderou;

                cmd.Parameters.Add(":codclient", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codClient;

                cmd.Parameters.Add(":codadresa", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codAdresa;

                cmd.Parameters.Add(":nrtel", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = nrtel;

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                //ErrorHandling.sendErrorToMail(ex.ToString() + " , " + nrBorderou + " , " + codClient + " , " + codAdresa + " , " + nrtel);
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