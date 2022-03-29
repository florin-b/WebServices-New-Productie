using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using System.Net.Mail;
using System.Web.Script.Serialization;

namespace TiparireDocumente
{
    public class OperatiiDocumenteTEST
    {

        public string getDocumente(string filiala, string departament, string tipDocument)
        {
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string condFiliala = "";

            if (filiala.Equals("BV10"))
                condFiliala = " and s.werks in ('BV10','BV20','BV30','BV50' )";
            else if (filiala.Equals("BV90"))
                condFiliala = "and s.werks = 'BV90' ";
            else
                condFiliala = " and substr(s.werks, 0, 2) = '" + filiala.Substring(0, 2) + "' ";

            if (filiala.Substring(0, 2).Equals("BU"))
                condFiliala = " and s.werks in " + Service1.getUlBuc(filiala);


            string condTipDocument = "";
            string[] intervData = Service1.getIntervalCautare().Split(',');

            if (tipDocument == null)
                condTipDocument = condFiliala;
            else {

                if (tipDocument.Equals("TRANSFER"))
                {
                    condTipDocument = condFiliala + " and h.lfart = 'UL' ";
                }
                else if (tipDocument.Equals("DISTRIBUTIE"))
                {
                    condTipDocument = " and s.werks in " + Service1.getUlDistrib(filiala);
                }
                else
                {
                    condTipDocument = condFiliala;
                }
            }

            try
            {
                string connectionString = testConnectionString();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();




                cmd.CommandText = " select x.numecl, x.vbeln, x.emitere,  " +
                                  " decode(length(x.matnr),18,substr(x.matnr,-8),x.matnr) cod , x.numeart, x.lfimg, x.vrkme, x.posnr, " +
                                  " to_char(to_date(x.wadat,'yyyymmdd')) livrare, x.pregatire, x.tiparire, x.lfart from (select " +
                                  " h.kunnr, c.nume numecl, h.vbeln, To_Char(To_Date(H.Erdat||' '||H.Erzet,'yyyymmdd hh24miss'),'dd-MON-yyyy HH24:mi','NLS_DATE_LANGUAGE = Romanian') emitere, s.posnr , s.matnr, " +
                                  " art.nume numeart, s.spart, s.werks, s.lfimg, s.vrkme, k.datbg, h.wadat, " +
                                  " nvl((select '1' from sapprd.zpregmarfagest g where g.document=h.vbeln and rownum=1),'-1') pregatire, " +
                                  " nvl((select '1' from sapprd.ztipariredoc g where g.document = h.vbeln and rownum=1),'-1') tiparire, h.lfart " +
                                  " from sapprd.likp h, sapprd.lips s, sapprd.vttp p, sapprd.vttk k, clienti c, articole art " +
                                  " where h.mandt = '900'  and s.lgort not in ('DESC','PRT1','PRT2','GAR1','GAR2','02T1','05T1','SRT1','MAV1','MAV2') and c.cod = h.kunnr  and h.mandt = s.mandt  and h.vbeln = s.vbeln " +
                                  " and art.cod = s.matnr and h.wadat between :dataStart and :dataStop and art.spart =:depart " +
                                    condTipDocument + " and nvl(k.datbg, '00000000') = '00000000' and s.lfimg > 0 " +
                                  " and h.mandt = p.mandt(+) and substr(s.matnr,11,1) != '3' " +
                                  " and h.vbeln = p.vbeln(+) and h.lfart not in ('EL', 'ZUL', 'ZLR') and p.mandt = k.mandt(+) " +
                                  " and decode( substr(s.werks, 3, 1),4,(CASE WHEN art.spart = '02' then 'FALSE' else 'TRUE' end),'TRUE') = 'TRUE' " +
                                  " and ( ( h.traty = 'TRAP' and nvl(k.dalen,'00000000') = '00000000') or h.traty = 'TCLI' or h.traty = 'TERT' ) " +
                                  " and p.tknum = k.tknum(+) " +
                                  " and not exists " +
                                  " (select tp.ul_stoc from sapprd.vbfa a, sapprd.zcomhead_tableta t, sapprd.zcomdet_tableta tp " +
                                  " where a.mandt = h.mandt and a.vbeln = h.vbeln and a.posnn = s.posnr and a.vbtyp_n = 'J' and a.vbtyp_v = 'C' " +
                                  " and a.mandt = t.mandt and a.vbelv = t.nrcmdsap and t.mandt = tp.mandt and t.id = tp.id and tp.ul_stoc = 'BV90' and rownum = 1) " +
                                  " ) x  where nvl(datbg, '00000000') = '00000000' " +
                                  "  and x.lfimg > 0 order by livrare, x.kunnr, x.posnr ";





                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();


                cmd.Parameters.Add(":dataStart", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = intervData[0];

                cmd.Parameters.Add(":dataStop", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = intervData[1];

                cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = departament;

                oReader = cmd.ExecuteReader();

                List<Document> listaDocumente = new List<Document>();
                Document unDocument = null;

                string nrDocument = "";
                bool isDocOpen = false;

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {

                        
                            unDocument = new Document();
                            unDocument.client = oReader.GetString(0);
                            unDocument.id = oReader.GetString(1);
                            unDocument.emitere = oReader.GetString(2);
                            unDocument.codArticol = oReader.GetString(3);
                            unDocument.numeArticol = oReader.GetString(4);
                            unDocument.cantitate = oReader.GetDouble(5).ToString();
                            unDocument.um = oReader.GetString(6);
                            unDocument.pozitieArticol = oReader.GetString(7);
                            unDocument.isPregatit = oReader.GetString(9);
                            unDocument.isTiparit = oReader.GetString(10);
                            unDocument.tip = oReader.GetString(11).Equals("UL") ? "T" : "D";

                            string numeClient = Service1.getNumeClient(connection, unDocument.id);
                            if (!unDocument.client.Equals(numeClient))
                                unDocument.client = unDocument.client + " " + numeClient;

                            listaDocumente.Add(unDocument);
                       


                    }

                }



                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaDocumente);



            }
            catch (Exception ex)
            {
                Service1.sendErrorToMail(ex.ToString() + cmd.CommandText + " , " + departament);
            }
            finally
            {

                if (oReader != null)
                {
                    oReader.Close();
                    oReader.Dispose();
                }

                cmd.Dispose();
                connection.Close();
                connection.Dispose();



            }





            return serializedResult;
        }



        private bool isDocumentOpen(OracleConnection connection, string nrDocument)
        {
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            Boolean isOpen = true;

            try
            {
                string sqlString = " select stat from miscstat where mblnr = (select distinct vbeln from sapprd.vbfa f where f.mandt = '900' and f.vbtyp_v = 'J' " +
                                   " and f.vbelv =:nrDocument and f.posnv = '000010' and f.vbtyp_v = 'J' and f.vbtyp_n = 'R')  and rownum< 2  ";

                cmd = connection.CreateCommand();
                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrDocument", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    if (oReader.GetInt32(0) > 0)
                    {
                        isOpen = false;
                    }
                    else
                    {
                        isOpen = true;
                    }

                }


            }
            catch (Exception ex)
            {
                Service1.sendErrorToMail(ex.ToString() + " , " + nrDocument);

            }

            return isOpen;

        }

        
        public string getDocumenteTiparite(string filiala, string departament, string dataTip)
        {
            string serializedResult = "";



            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = testConnectionString();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct y.numecl, y.vbeln, y.emitere, y.cod , y.numeart, y.lfimg, y.meins, y.posnr , y.kunnr from ( " +
                                   " select x.numecl, x.vbeln, x.emitere, decode(length(x.matnr),18,substr(x.matnr,-8),x.matnr) cod , x.numeart, x.lfimg, x.meins, x.posnr, x.kunnr " +
                                   "  from (select distinct " +
                                   " h.kunnr, c.nume numecl, h.vbeln, to_char(to_date(h.erdat,'yyyymmdd')) emitere, s.posnr , s.matnr, " +
                                   " art.nume numeart, s.spart, s.werks, s.lfimg, s.meins,  k.datbg " +
                                   " from sapprd.likp h, sapprd.lips s, sapprd.vttp p, sapprd.vttk k, clienti c, articole art " +
                                   " where h.mandt = '900'  and c.cod = h.kunnr  and h.mandt = s.mandt  and h.vbeln = s.vbeln " +
                                   " and art.cod = s.matnr and art.spart =:depart and substr(s.matnr,11,1) != '3'" +
                                   " and substr(s.werks,0,2) = '" + filiala.Substring(0, 2) + "' and h.mandt = p.mandt(+) " +
                                   " and h.vbeln = p.vbeln(+) and p.mandt = k.mandt(+) " +
                                   " and p.tknum = k.tknum(+)  " +
                                   " ) x, sapprd.ztipariredoc d  where  " +
                                   " d.document = x.vbeln and d.data = :dataTip ) y  order by y.kunnr, y.posnr ";





                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();


                cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = departament;

                cmd.Parameters.Add(":dataTip", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = dataTip;

                oReader = cmd.ExecuteReader();

                List<Document> listaDocumente = new List<Document>();

                Document unDocument = null;

                if (oReader.HasRows)
                {



                    while (oReader.Read())
                    {

                        unDocument = new Document();
                        unDocument.client = oReader.GetString(0);
                        unDocument.id = oReader.GetString(1);
                        unDocument.emitere = oReader.GetString(2);
                        unDocument.codArticol = oReader.GetString(3);
                        unDocument.numeArticol = oReader.GetString(4);
                        unDocument.cantitate = oReader.GetDouble(5).ToString();
                        unDocument.um = oReader.GetString(6);
                        unDocument.pozitieArticol = oReader.GetString(7);
                        listaDocumente.Add(unDocument);

                    }

                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaDocumente);



            }
            catch (Exception ex)
            {
                Service1.sendErrorToMail(ex.ToString());

            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }



            return serializedResult;
        }




        [WebMethod]
        public string setPrintedDocuments(string listaDocumente, string gestionar, string departament, string filiala)
        {
            string retVal = "0", query = "";


            var serializer = new JavaScriptSerializer();
            List<DocumentTiparit> documente = serializer.Deserialize<List<DocumentTiparit>>(listaDocumente);

            OracleConnection connection = new OracleConnection();

            string connectionString = testConnectionString();

            try
            {

                connection.ConnectionString = connectionString;
                connection.Open();

                DateTime cDate = DateTime.Now;
                string year = cDate.Year.ToString();
                string day = cDate.Day.ToString("00");
                string month = cDate.Month.ToString("00");
                string nowDate = year + month + day;
                string hour = cDate.Hour.ToString("00");
                string minute = cDate.Minute.ToString("00");
                string sec = cDate.Second.ToString("00");
                string nowTime = hour + minute + sec;

                OracleCommand cmd = connection.CreateCommand();

                for (int ii = 0; ii < documente.Count; ii++)
                {
                    query = " insert into sapprd.ztipariredoc(mandt,document, gestionar, data, ora, filiala, departament) " +
                            " values ('900', :document, :codGest, :datac, :orac, :filiala, :departament) ";


                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":document", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = documente[ii].id;

                    cmd.Parameters.Add(":codGest", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = gestionar;

                    cmd.Parameters.Add(":datac", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = nowDate;

                    cmd.Parameters.Add(":orac", OracleType.NVarChar, 18).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = nowTime;

                    cmd.Parameters.Add(":filiala", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = filiala;

                    cmd.Parameters.Add(":departament", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                    cmd.Parameters[5].Value = departament;

                    cmd.ExecuteNonQuery();

                }


            }
            catch (Exception ex)
            {
                retVal = "-1";
                Service1.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }




            return retVal;
        }



        public string setMarfaPregatita(string nrDocument, string gestionar)
        {
            string retVal = "0", query = "";


            OracleConnection connection = new OracleConnection();

            string connectionString = testConnectionString();

            try
            {

                connection.ConnectionString = connectionString;
                connection.Open();

                DateTime cDate = DateTime.Now;
                string year = cDate.Year.ToString();
                string day = cDate.Day.ToString("00");
                string month = cDate.Month.ToString("00");
                string nowDate = year + month + day;
                string hour = cDate.Hour.ToString("00");
                string minute = cDate.Minute.ToString("00");
                string sec = cDate.Second.ToString("00");
                string nowTime = hour + minute + sec;

                OracleCommand cmd = connection.CreateCommand();


                query = " insert into sapprd.zpregmarfagest(mandt,codgestionar, document, datac, orac) " +
                        " values ('900', :codGest, :document, :datac, :orac) ";


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();


                cmd.Parameters.Add(":codGest", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = gestionar;


                cmd.Parameters.Add(":document", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = nrDocument;



                cmd.Parameters.Add(":datac", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = nowDate;

                cmd.Parameters.Add(":orac", OracleType.NVarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = nowTime;



                cmd.ExecuteNonQuery();




            }
            catch (Exception ex)
            {
                retVal = "-1";
                Service1.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }




            return retVal;
        }





        static private string testConnectionString()
        {


            return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                               " (HOST = 10.1.3.89)(PORT = 1527)))(CONNECT_DATA = (SERVICE_NAME = TES))); " +
                               " User Id = WEBSAP; Password = 2INTER7; ";

        }

        static private string prdConnectionString()
        {


            return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                   " (HOST = 10.1.3.95)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = TABLET) )); " +
                   " User Id = WEBSAP; Password = 2INTER7;";

        }


      

    }
}