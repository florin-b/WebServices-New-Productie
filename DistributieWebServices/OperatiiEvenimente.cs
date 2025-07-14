using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using System.Web.Script.Serialization;

namespace DistributieWebServices
{
    public class OperatiiEvenimente
    {

        public string saveNewEvent(string serializedEvent)
        {


            string retVal = "";

            if (serializedEvent.Contains("["))
                saveEventsList(serializedEvent);
            else
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                EvenimentNou newEvent = serializer.Deserialize<EvenimentNou>(serializedEvent);
               

                OracleConnection connection = new OracleConnection();
                OracleCommand cmd = null;
                OracleDataReader oReader = null;
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


                string strLocalEveniment = "0";

                //eveniment document
                if (newEvent.document.Equals(newEvent.client))
                {
                    if (newEvent.eveniment.Equals("0"))
                    {
                        strLocalEveniment = "P";
                    }

                    if (newEvent.eveniment.Equals("P"))
                        strLocalEveniment = "S";
                }
                else //eveniment client
                {
                    if (newEvent.eveniment.Equals("0") || newEvent.eveniment.Equals("P"))
                        strLocalEveniment = "S";

                    if (newEvent.eveniment.Equals("S"))
                        strLocalEveniment = "P";
                }


                try
                {

                    string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                    connection.ConnectionString = connectionString;
                    connection.Open();

                    cmd = connection.CreateCommand();

                    if (newEvent.tipEveniment == null || newEvent.tipEveniment.Equals("NOU", StringComparison.InvariantCultureIgnoreCase))
                    {
                        query = " select d.latitude, d.longitude, nvl(d.mileage,0) from gps_index d where d.device_id = (select distinct g.id from sapprd.zdocumentebord a, " +
                                " borderouri b, gps_masini g where  " +
                                " a.nr_bord = b.numarb and REPLACE(b.masina, '-') = g.nr_masina " +
                                " and a.nr_bord = :nrBord)  ";


                    }
                    else if (newEvent.tipEveniment.Equals("ARHIVAT", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string dataEveniment = newEvent.data + " " + newEvent.ora;

                        nowDate = newEvent.data;
                        nowTime = newEvent.ora;

                        query = " select latitude, longitude, mileage from ( " +
                                " select latitude, longitude, mileage, " +
                                " (to_date('" + dataEveniment + "', 'yyyymmdd hh24miss') - record_time) * 24 * 60  diff from gps_date where device_id = " +
                                " (select id from gps_masini where nr_masina = (select replace(masina, '-', '') from websap.borderouri where numarb =:nrBord)) and " +
                                " record_time between(to_date('" + dataEveniment + "', 'yyyymmdd hh24miss') - 15 / (24 * 60)) and(to_date('" + dataEveniment + "', 'yyyymmdd hh24miss') + 15 / (24 * 60)) " +
                                " and(to_date('" + dataEveniment + "', 'yyyymmdd hh24miss') - record_time) * 24 * 60 >= 0 order by diff ) where rownum< 2 ";
                    }

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":nrBord", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = newEvent.document;

                    cmd.CommandText = query;

                    oReader = cmd.ExecuteReader();
                    string latit = "0", longit = "0", mileage = "0";

                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        latit = oReader.GetDouble(0).ToString().Replace(",", ".");
                        longit = oReader.GetDouble(1).ToString().Replace(",", ".");
                        mileage = oReader.GetDecimal(2).ToString();
                    }

                   

                    if (recordExist(connection, newEvent.codSofer, newEvent.document, newEvent.client, strLocalEveniment, newEvent.codAdresa))
                    {
                        retVal = nowDate + "#" + nowTime;

                        if (newEvent.evBord != null && newEvent.evBord.Equals("STOP"))
                        {
                            addStopBord(connection, newEvent, latit, longit, mileage);
                        }

                        return retVal;
                    }


                    query = " insert into sapprd.zevenimentsofer(mandt,codsofer,data,ora,document,client,eveniment,gps,fms,codadresa) " +
                            " values ('900',:codsofer,:data,:ora,:document,:client,:eveniment,:gps,:fms,:codadresa) ";


                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":codsofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = newEvent.codSofer;

                    cmd.Parameters.Add(":data", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = nowDate;

                    cmd.Parameters.Add(":ora", OracleType.VarChar, 18).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = nowTime;

                    cmd.Parameters.Add(":document", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = newEvent.document;

                    cmd.Parameters.Add(":client", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = newEvent.client;

                    cmd.Parameters.Add(":eveniment", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                    cmd.Parameters[5].Value = strLocalEveniment;

                    cmd.Parameters.Add(":gps", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[6].Value = latit + "," + longit;

                    cmd.Parameters.Add(":fms", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[7].Value = mileage;

                    cmd.Parameters.Add(":codadresa", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[8].Value = newEvent.codAdresa;

                    cmd.ExecuteNonQuery();

                    retVal = nowDate + "#" + nowTime;


                    if (newEvent.bordParent != null &&  !newEvent.bordParent.Equals("-1"))
                    {
                        addBordStartCursa(connection, newEvent, latit, longit, mileage);
                        updateBordParentSfCursa(connection, newEvent, latit, longit, mileage);
                    }


                    if (newEvent.evBord != null && newEvent.evBord.Equals("STOP"))
                    {
                        addStopBord(connection, newEvent, latit, longit, mileage);
                    }

                   
                   /*
                    if (newEvent.document.Equals(newEvent.client) && newEvent.eveniment.Equals("0"))
                    {
                        string filialaBorderou = getFilialaBorderou(connection, newEvent.document);
                       
                        if (!filialaBorderou.Equals("GL10") && !filialaBorderou.Equals("IS10") && !filialaBorderou.Equals("DJ10") && !filialaBorderou.Equals("NT10"))
                        {
                            sendSmsAlerts(connection, newEvent.document);
                        }
                       
                    }
                    */


                    }
                catch (Exception ex)
                {
                    retVal = nowDate + "#" + nowTime;
                    ErrorHandling.sendErrorToMail(ex.ToString()  + " Ser: " + serializedEvent);
                }
                finally
                {
                    DatabaseConnections.CloseConnections(oReader, cmd, connection);
                }
            }


            return retVal;

        }


        public static string getFilialaBorderou(OracleConnection connection, string nrBorderou)
        {
            string filiala = "";
            OracleDataReader oReader = null;
            OracleCommand cmd = connection.CreateCommand();

            try
            {

                string query = " select fili from borderouri where numarb=:nrBorderou ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrBorderou", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrBorderou;

                oReader = cmd.ExecuteReader();
                if (oReader.HasRows)
                {
                    oReader.Read();
                    filiala = oReader.GetString(0);
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + nrBorderou);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return filiala;
        }


        private void sendSmsAlerts(OracleConnection connection, String nrDocument)
        {
            
            try
            {

                if (Utils.itsTimeToSendSmsAlert())
                    new Sms().sendSMS(getClientsPhoneNumber(connection, nrDocument), nrDocument);
                else if (Utils.itsProgramFinished())
                {
                    new Sms().sendSmsToBuffer(connection, nrDocument);
                }
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " con = " + connection.ToString() + " , doc = " + nrDocument);
            }
            finally
            {

            }

            return;

        }





       
        public List<NotificareClient> getClientsPhoneNumber(OracleConnection connection, string nrDocument)
        {


            OracleCommand cmd = null;
            OracleDataReader oReader = null;
            List<NotificareClient> notificariList = new List<NotificareClient>();

            try
            {

                cmd = connection.CreateCommand();


                string sqlString = " select distinct k.kunnr codclient,  tel_number, s.idcomanda, s.adresa_client from sapprd.kna1 k, sapprd.adr2 a, sapprd.adrt t, sapprd.zdocumentesms s " +
                            " where k.mandt = '900' and k.kunnr in (select distinct a.cod from sapprd.zdocumentebord a, borderouri b, soferi c where " +
                            " a.nr_bord = b.numarb and b.cod_sofer = c.cod and b.numarb =:borderou ) and k.mandt = a.client " +
                            " and k.adrnr = a.addrnumber  and a.client = t.client  and a.addrnumber = t.addrnumber  and a.consnumber = t.consnumber " +
                            " and t.comm_type = 'TEL' and s.mandt='900' and s.nr_bord = :borderou and s.cod_client = k.kunnr " +
                            " and upper(t.remark) in ('LIVRARI', 'LIVRARE')  " +
                            " union " +
                            " select s.cod_client codclient,  a.telefon, s.idcomanda, s.adresa_client from sapprd.zdocumentesms s, sapprd.zcomhead_tableta a, sapprd.kna1 k " +
                            " where nr_bord = :borderou  and s.idcomanda = a.id and s.mandt = a.mandt and a.mandt = k.mandt " +
                            " and a.cod_client = k.kunnr and k.ktokd = 'OCAV'  " +
                            " union " +
                            " select distinct s.cod_client codclient, c.tel_number, s.idcomanda, s.adresa_client " +
                            " from sapprd.zdocumentesms s, sapprd.adrc c " +
                            " where s.mandt = '900' and s.nr_bord =:borderou  and s.mandt = c.client and s.adresa_client = c.addrnumber " +
                            " and s.tip_livrare = 'ZLFH' and c.tel_number like '07%' order by codclient ";



                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":borderou", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                oReader = cmd.ExecuteReader();


                if (oReader.HasRows)
                {
                    List<NotificareClient> tempNotificariList = new List<NotificareClient>();

                    while (oReader.Read())
                    {

                        NotificareClient notificare = new NotificareClient();
                        notificare.codClient = oReader.GetString(0);
                        notificare.nrTelefon = oReader.GetString(1);
                        notificare.codAdresa = oReader.GetString(3);

                        notificare.dateComanda = getDateComanda(connection, nrDocument, notificare.codClient, notificare.codAdresa);
                        tempNotificariList.Add(notificare);

                    }

                    HashSet<NotificareClient> setNotificari = new HashSet<NotificareClient>(tempNotificariList);
                    notificariList = new List<NotificareClient>(setNotificari);

                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " nrdoc = " + nrDocument);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return notificariList;


        }


        private DateComanda getDateComanda(OracleConnection connection, string nrBorderou, string codClient, string codAdresa)
        {

            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            DateComanda dateComanda  = new DateComanda();

            try
            {

                cmd = connection.CreateCommand();

                string sqlString = " select distinct f.vbelv,to_char(to_date(t.datac,'yyyymmdd'),'dd MONTH', 'NLS_DATE_LANGUAGE = Romanian') emitere, " +
                            " t.depart from sapprd.zdocumentesms a, sapprd.vttp p, sapprd.vbfa f, sapprd.zcomhead_tableta t, sapprd.likp l , sapprd.vbpa pa " +
                            " where a.nr_bord =:nrBorderou and a.cod_client =:codClient and a.adresa_client =:codAdresa and a.mandt = '900' " +
                            " and t.nrcmdsap = f.vbelv and t.mandt = '900' and " +
                            " f.mandt = '900' and f.vbeln = p.vbeln and f.vbtyp_v = 'C' and a.nr_bord = p.tknum and p.mandt = a.mandt " +
                            " and p.mandt = l.mandt and p.vbeln = l.vbeln and l.kunnr =:codClient " +
                            " and pa.mandt = f.mandt and pa.vbeln = f.vbelv and pa.parvw = 'WE' and pa.adrnr = a.adresa_client " +
                            " order by emitere ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrBorderou", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrBorderou;

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codClient;

                cmd.Parameters.Add(":codAdresa", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codAdresa;

                oReader = cmd.ExecuteReader();
                string emitere = "", departament = "";
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        if (emitere.Equals(""))
                            emitere = oReader.GetString(1);
                        else if (!emitere.Equals("") && !emitere.Contains(oReader.GetString(1)))
                            emitere += ", " + oReader.GetString(1);


                        if (departament.Equals(""))
                            departament = Utils.getDepartName(oReader.GetString(2));
                        else if (!departament.Equals("") && !departament.Contains(Utils.getDepartName(oReader.GetString(2))))
                            departament += ", " + Utils.getDepartName(oReader.GetString(2));
                    }
                }

                dateComanda.emitere = emitere;
                dateComanda.departament = departament;

            }
            catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + nrBorderou + " , " + codClient + " , " + codAdresa);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return dateComanda;

        }

        private bool recordExist(OracleConnection connection, string codSofer, string document, string codClient, string eveniment, string codAdresa)
        {
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            bool exists = false;



            try
            {
                string sqlString = " select 1 from sapprd.zevenimentsofer where codsofer=:codSofer and document=:document and client=:codclient and eveniment=:eveniment  " +
                                   " and codadresa=:codAdresa ";

                cmd = connection.CreateCommand();
                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codsofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                cmd.Parameters.Add(":document", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = document;

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codClient;

                cmd.Parameters.Add(":eveniment", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = eveniment;

                cmd.Parameters.Add(":codadresa", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = codAdresa;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    exists = true;
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }




            return exists;


        }



        private void addStopBord(OracleConnection connection, EvenimentNou newEvent, String latit, String longit, String mileage)
        {


            try
            {

                OracleCommand cmd = connection.CreateCommand();

                String query = " insert into sapprd.zevenimentsofer(mandt,codsofer,data,ora,document,client,eveniment,gps,fms,codadresa) " +
                               " values ('900',:codsofer,:data,:ora,:document,:client,:eveniment,:gps,:fms,:codadresa) ";


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codsofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = newEvent.codSofer;

                cmd.Parameters.Add(":data", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = getDate();

                cmd.Parameters.Add(":ora", OracleType.VarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = getTimeAddSec();

                cmd.Parameters.Add(":document", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = newEvent.document;

                cmd.Parameters.Add(":client", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = newEvent.document;

                cmd.Parameters.Add(":eveniment", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = "S";

                cmd.Parameters.Add(":gps", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = latit + "," + longit;

                cmd.Parameters.Add(":fms", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                cmd.Parameters[7].Value = mileage;

                cmd.Parameters.Add(":codadresa", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[8].Value = " ";


                cmd.ExecuteNonQuery();

                if (cmd != null)
                    cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + newEvent);
            }
            finally
            {

            }



        }


        private void addBordStartCursa(OracleConnection connection, EvenimentNou newEvent, String latit, String longit, String mileage)
        {


            try
            {

                OracleCommand cmd = connection.CreateCommand();

                String query = " insert into sapprd.zevenimentsofer(mandt,codsofer,data,ora,document,client,eveniment,gps,fms,codadresa) " +
                               " values ('900',:codsofer,:data,:ora,:document,:client,:eveniment,:gps,:fms,:codadresa) ";


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codsofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = newEvent.codSofer;

                cmd.Parameters.Add(":data", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = getDate();

                cmd.Parameters.Add(":ora", OracleType.VarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = getTime();

                cmd.Parameters.Add(":document", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = newEvent.document;

                cmd.Parameters.Add(":client", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = newEvent.document;

                cmd.Parameters.Add(":eveniment", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = "P";

                cmd.Parameters.Add(":gps", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = latit + "," + longit;

                cmd.Parameters.Add(":fms", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                cmd.Parameters[7].Value = mileage;

                cmd.Parameters.Add(":codadresa", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[8].Value = newEvent.codAdresa;


                cmd.ExecuteNonQuery();

                if (cmd != null)
                    cmd.Dispose();

            }
            catch (Exception ex)
            {
                //ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {

            }


        }

        private void updateBordParentSfCursa(OracleConnection connection, EvenimentNou newEvent, String latit, String longit, String mileage)
        {


            try
            {

                OracleCommand cmd = connection.CreateCommand();


                string query = " update sapprd.zevenimentsofer set data=:data, ora=:ora, gps=:gps, fms=:fms " +
                               " where codsofer=:codSofer and document=:document and client = document  and eveniment='S'  ";


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":data", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = getDate();

                cmd.Parameters.Add(":ora", OracleType.VarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = getTime();

                cmd.Parameters.Add(":gps", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = latit + "," + longit;

                cmd.Parameters.Add(":fms", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = mileage;

                cmd.Parameters.Add(":codsofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = newEvent.codSofer;

                cmd.Parameters.Add(":document", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = newEvent.bordParent;


                cmd.ExecuteNonQuery();

                if (cmd != null)
                    cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + newEvent.ToString());
            }
            finally
            {

            }



        }



        public string getEvenimenteBorderou(string nrBorderou)
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

                cmd.CommandText = " select  b.nume,b.cod , " +
                                  " nvl((select  c.data||':'||c.ora||'?'|| c.fms from sapprd.zevenimentsofer c where c.document = a.nr_bord  and c.document = c.client and  " +
                                  " c.eveniment = 'P'),0) plecare_cursa, " +
                                  " nvl((select  c.data||':'||c.ora||'?'|| c.fms from sapprd.zevenimentsofer c where c.document = a.nr_bord and c.client = a.cod_client and " +
                                  " c.eveniment = 'S' and c.codadresa = a.adresa_client),0) sosire, nvl((select c.ora from sapprd.zevenimentsofer c where c.document = a.nr_bord and c.client = a.cod_client " +
                                  " and c.eveniment = 'P' and c.codadresa = a.adresa_client),0) plecare, a.adresa_client cod_adresa, " +
                                  " (select ad.city1||', '||ad.street||', '||ad.house_num1 from sapprd.adrc ad where ad.client = '900' and ad.addrnumber = a.adresa_client) adresa_client " +
                                  " from sapprd.zdocumentesms a, clienti b  where a.nr_bord =:nrbord " +
                                  " and a.cod_client = b.cod and tip = 2 order by a.poz ";



                cmd.Parameters.Clear();
                cmd.Parameters.Add(":nrbord", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrBorderou;

                oReader = cmd.ExecuteReader();
                string[] startCursa, sosireClient;
                string oraStartCursa = "0", kmStartCursa = "0";
                string oraSosireClient = "0", kmSosireClient = "0";

                List<EvenimentBorderou> listEvenimente = new List<EvenimentBorderou>();
                EvenimentBorderou eveniment = null;


                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        oraStartCursa = "0"; kmStartCursa = "0";
                        oraSosireClient = "0"; kmSosireClient = "0";

                        if (!oReader.GetString(2).Equals("0"))
                        {
                            startCursa = oReader.GetString(2).Split('?');
                            oraStartCursa = startCursa[0];
                            kmStartCursa = startCursa[1];
                        }

                        if (!oReader.GetString(3).Equals("0"))
                        {
                            sosireClient = oReader.GetString(3).Split('?');
                            oraSosireClient = sosireClient[0];
                            kmSosireClient = sosireClient[1];
                        }

                        eveniment = new EvenimentBorderou();
                        eveniment.numeClient = oReader.GetString(0);
                        eveniment.codClient = oReader.GetString(1);
                        eveniment.oraStartCursa = oraStartCursa;
                        eveniment.kmStartCursa = kmStartCursa;
                        eveniment.oraSosireClient = oraSosireClient;
                        eveniment.kmSosireClient = kmSosireClient;
                        eveniment.oraPlecare = oReader.GetString(4);
                        eveniment.codAdresa = oReader.GetString(5);
                        eveniment.adresa = oReader.GetString(6);
                        listEvenimente.Add(eveniment);

                    }

                }


                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listEvenimente);

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


        private void saveEventsList(string serializedEvents)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<EvenimentNou> evenimente = serializer.Deserialize<List<EvenimentNou>>(serializedEvents);

            for (int i = 0; i < evenimente.Count; i++)
                saveNewEvent(serializer.Serialize(evenimente[i]));


        }



        public string saveEvenimentStopIncarcare(string document, string codSofer)
        {

            OracleConnection connection = new OracleConnection();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                String query = " insert into sapprd.zsfarsitinc(mandt, document, codsofer, data, ora) " +
                               " values ('900', :document, :codsofer, :data, :ora) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":document", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = document;

                cmd.Parameters.Add(":codsofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":data", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = getDate();

                cmd.Parameters.Add(":ora", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = getTime();

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , document " +  document + " , sofer " +  codSofer);
                return "1";
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return "SOF";



        }


        public void saveOrdineEtape(string serializedEtape, string serializedEvent)
        {

            OracleConnection connection = new OracleConnection();
            OracleTransaction transaction = null;

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Etapa> listEtape = serializer.Deserialize<List<Etapa>>(serializedEtape);

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                

                if (ordineEtapeExista(connection, listEtape[0].borderou))
                {
                    connection.Close();
                    connection.Dispose();
                    return;
                }

                OracleCommand cmd = connection.CreateCommand();
                transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;

                foreach (Etapa etapa in listEtape)
                {
                    String query = " insert into sapprd.zordinelivrari(mandt, borderou, client, codadresa, pozitie) " +
                                   " values ('900', :boderou, :client, :codAdresa, :pozitie) ";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":boderou", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = etapa.borderou;

                    cmd.Parameters.Add(":client", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = etapa.client;

                    cmd.Parameters.Add(":codAdresa", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = etapa.codAdresa;

                    cmd.Parameters.Add(":pozitie", OracleType.Int32, 10).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = etapa.pozitie;



                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();

                cmd.Dispose();

            }
            catch (Exception ex)
            {
                //ErrorHandling.sendErrorToMail(ex.ToString() + " data: " + serializedEtape + " , event: " + serializedEvent);

            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return;

        }


        private bool ordineEtapeExista(OracleConnection connection, string bord)
        {
            OracleCommand cmd = connection.CreateCommand();
            OracleDataReader oReader = null;

            bool etapeExists = false;
            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select 1 from sapprd.zordinelivrari where borderou=:bord  ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":bord", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = bord;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                    etapeExists = true;

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {

                if (oReader != null)
                    oReader.Close();

                if (cmd != null)
                    cmd.Dispose();

            }

            return etapeExists;

        }



        public string getEvenimentStopIncarcare(string document, string codSofer)
        {
            string retValue = " ";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select 1 from sapprd.zsfarsitinc where document=:document and codsofer =:codSofer ";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":document", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = document;

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    retValue = "SOF";
                }
                else
                {
                    cmd.CommandText = " select dalen from sapprd.vttk where tknum=:document ";

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":document", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = document;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();

                        if (oReader.GetString(0) == "00000000")
                            retValue = " ";
                        else
                            retValue = "LOG";



                    }

                }

                oReader.Close();
                oReader.Dispose();

                string filialaSofer = getFilialaSofer(connection, codSofer);



            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                return "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }




            return retValue;


        }



        private string getFilialaSofer(OracleConnection connection, string codSofer)
        {
            string filiala = "";

            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {

                string query = " select fili from soferi where cod=:codSofer ";
                cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codSofer", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    filiala = oReader.GetString(0);
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

            return filiala;
        }



        public string saveNewStop(string idEveniment, string codSofer, string codBorderou, string codEveniment)
        {

          


            OracleConnection connection = new OracleConnection();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                String query = " insert into sapprd.zopririsoferi(mandt, ideveniment, codsofer, codborderou, codeveniment, data, ora) " +
                        " values ('900', :ideveniment, :codsofer, :codborderou, :codeveniment, :data, :ora) ";


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":ideveniment", OracleType.VarChar, 15).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idEveniment;

                cmd.Parameters.Add(":codsofer", OracleType.VarChar, 15).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":codborderou", OracleType.VarChar, 15).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codBorderou;

                cmd.Parameters.Add(":codeveniment", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = codEveniment;

                cmd.Parameters.Add(":data", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = getDate();

                cmd.Parameters.Add(":ora", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = getTime();

                cmd.ExecuteNonQuery();

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


            return "1";
        }




        public string getEvenimenteBorderouri(string codSofer, string interval)
        {
            string condData = "";
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                if (interval == "0") //astazi
                {
                    string dateInterval = DateTime.Today.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                    condData = " and s.data_e = '" + dateInterval + "' ";
                }

                if (interval == "1") //ultimele 7 zile
                {
                    string dateInterval = DateTime.Today.AddDays(-7).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                    condData = " and s.data_e >= '" + dateInterval + "' ";
                }

                if (interval == "2") //ultimele 30 zile
                {
                    string dateInterval = DateTime.Today.AddDays(-30).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                    condData = " and s.data_e >= '" + dateInterval + "' ";
                }


                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct b.document,  to_char(to_date(s.data_e,'yyyyMMdd')),  nvl((select nvl(ev.eveniment,'0') eveniment " +
                                  " from sapprd.zevenimentsofer ev where ev.document = b.document and ev.data = (select max(data) from sapprd.zevenimentsofer where document = ev.document and client = ev.document) " +
                                  " and ev.ora = (select max(ora) from sapprd.zevenimentsofer where document = ev.document and client = ev.document and data = ev.data)),0) eveniment " +
                                  " from  sapprd.zevenimentsofer b, sapprd.zdocumentesms s where b.codsofer=:codSofer and s.nr_bord = b.document " + condData + " order by b.document ";




                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;


                oReader = cmd.ExecuteReader();

                List<Borderouri> listaBorderouri = new List<Borderouri>();
                Borderouri unBorderou = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        unBorderou = new Borderouri();
                        unBorderou.numarBorderou = oReader.GetString(0);
                        unBorderou.dataEmiterii = oReader.GetString(1);
                        unBorderou.evenimentBorderou = oReader.GetString(2);
                        listaBorderouri.Add(unBorderou);


                    }

                }


                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaBorderouri);

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



        public static InitStatus getInitStatus(string codSofer)
        {
            InitStatus initStatus = null;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select ev.document, ev.client, ev.eveniment, b.shtyp from sapprd.zevenimentsofer ev, borderouri b where " +
                                  " b.numarb = ev.document and ev.codsofer =:codSofer and " +
                                  " ev.data = (select max(ev1.data) from sapprd.zevenimentsofer ev1 where ev1.codsofer = ev.codsofer) and " +
                                  " ev.ora = (select max(ev2.ora) from sapprd.zevenimentsofer ev2 where ev2.codsofer = ev.codsofer and ev2.data = ev.data) and " +
                                  " exists (select 1 from sapprd.zevenimentsofer z where z.codsofer = ev.codsofer and z.client = ev.document " +
                                  " and z.data = ev.data and z.ora = z.ora and z.eveniment = 'P') " +
                                  " and not exists (select 1 from sapprd.zevenimentsofer z where z.codsofer = ev.codsofer and z.client = ev.document " +
                                  " and z.eveniment = 'S') ";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    initStatus = new InitStatus();
                    initStatus.document = oReader.GetString(0);
                    initStatus.client = oReader.GetString(1);
                    initStatus.eveniment = oReader.GetString(2);
                    initStatus.tipDocument = oReader.GetString(3);
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

            return initStatus;
        }


        public string cancelEvent(string tipEveniment, string nrDocument, string codClient, string codSofer)
        {
            string retVal = "";

            if (tipEveniment.Equals("SF_INC"))
            {
                retVal = cancelSfarsitIncarcare(nrDocument, codSofer);
            }
            else if (tipEveniment.Equals("START") || tipEveniment.Equals("STOP"))
            {
                retVal = cancelStartBord(nrDocument, codSofer, tipEveniment);
            }
            else if (tipEveniment.Equals("SOSIRE"))
            {
                retVal = cancelSosire(nrDocument, codClient, codSofer);
            }



            return retVal;
        }



        public string getPozitieCurenta(string nrBorderou)
        {

            string retVal = "0x0";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                cmd.CommandText = " select nvl(latitude,0), nvl(longitude,0) from gps_index where device_id = ( " +
                                  " select distinct id from gps_masini where nr_masina = " +
                                  " (select replace(masina, '-', '') from borderouri where numarb =:nrBorderou and rownum=1)) ";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":nrBorderou", OracleType.VarChar, 300).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrBorderou;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    retVal = oReader.GetDouble(0) + "x" + oReader.GetDouble(1);

                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " bord = " + nrBorderou);
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }



            retVal = "0x0";
            return retVal;
        }


        private string cancelSfarsitIncarcare(string nrDocument, string codSofer)
        {
            OracleConnection connection = new OracleConnection();
            string retVal = "1";



            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                String query = " delete from sapprd.zsfarsitinc where document=:nrDocument and codsofer=:codSofer ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrDocument", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                cmd.Parameters.Add(":codsofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }


            return retVal;



        }



        private string cancelStartBord(string nrDocument, string codSofer, string tipEveniment)
        {
            OracleConnection connection = new OracleConnection();
            string retVal = "1";

           

            string strEveniment = "X";
            if (tipEveniment.Equals("START"))
                strEveniment = "P";
            else if (tipEveniment.Equals("STOP"))
                strEveniment = "S";



            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                String query = " delete from sapprd.zevenimentsofer where document=:nrDocument and client=:nrDocument and codsofer=:codSofer and eveniment =:eveniment ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrDocument", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                cmd.Parameters.Add(":codsofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":eveniment", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = strEveniment;

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }


            return retVal;



        }



        private string cancelSosire(string nrDocument, string codClient, string codSofer)
        {
            OracleConnection connection = new OracleConnection();
            string retVal = "1";



            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                String query = " delete from sapprd.zevenimentsofer where document=:nrDocument and client=:codClient and codsofer=:codSofer and eveniment = 'S' ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrDocument", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codClient;

                cmd.Parameters.Add(":codsofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codSofer;

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }


            return retVal;

        }


        public void getMasiniPlecateInCursa()
        {
           /*
            

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;
            OracleDataReader oReader = null;
            string query = "";

            int NR_MINUTE_PLECARE = 15;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


             

                query = " select b.numarb, b.masina, b.cod_sofer, k.datbg,k.uatbg , round((sysdate - to_date(k.datbg || ' ' || k.uatbg, 'yyyymmdd HH24:mi:ss')) * 24 * 60,-1) minute " +
                        " from websap.borderouri b join sapprd.vttk k on b.numarb = k.tknum where k.mandt = '900' and b.sttrg = 6 and datbg = to_char(sysdate, 'yyyymmdd') " +
                        " and not exists (select 1 from SAPPRD.zsmsclienti where mandt='900' and borderou = b.numarb and codsofer = b.cod_sofer) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                int nrMinute = 0;
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        nrMinute = oReader.GetInt32(5);

                        if (nrMinute <= NR_MINUTE_PLECARE)
                        {
                            if (Utils.itsTimeToSendSmsAlert())
                            {
                                new Sms().sendSMS(getClientsPhoneNumber(connection, oReader.GetString(0)), oReader.GetString(0));

                                List<NotificareClient> listNotificari = getClientsPhoneNumber(connection, oReader.GetString(0));

                                foreach (NotificareClient notificare in listNotificari)
                                {
                                   // Sms.logSmsBorderou(connection, oReader.GetString(0), notificare.codClient, notificare.codAdresa, notificare.nrTelefon);

                                   // ErrorHandling.sendErrorToMail("SMS Clienti automat : " + oReader.GetString(0)  + ", " + notificare.codClient + " , " + notificare.codAdresa + " , " + notificare.nrTelefon);
                                }


                            }
                            else if (Utils.itsProgramFinished())
                            {
                                new Sms().sendSmsToBuffer(connection, oReader.GetString(0));
                            }
                        }


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

            */

        }




        private static String getDate()
        {
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            return year + month + day;
        }

        private static String getTime()
        {
            DateTime cDate = DateTime.Now;
            string hour = cDate.Hour.ToString("00");
            string minute = cDate.Minute.ToString("00");
            string sec = cDate.Second.ToString("00");
            return hour + minute + sec;
        }

        private static String getTimeAddSec()
        {
            DateTime cDate = DateTime.Now.AddSeconds(1);
            string hour = cDate.Hour.ToString("00");
            string minute = cDate.Minute.ToString("00");
            string sec = cDate.Second.ToString("00");
            return hour + minute + sec;
        }


    }
}