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

    [WebService(Namespace = "http://tiparire.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class Service1 : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }


        static private string prdConnectionString()
        {

            return DatabaseConnections.ConnectToProdEnvironment();


        }


        static private string testConnectionString()
        {

            return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                               " (HOST = 10.1.3.89)(PORT = 1527)))(CONNECT_DATA = (SERVICE_NAME = TES))); " +
                               " User Id = WEBSAP; Password = 2INTER7; ";

        }

        static private string qasConnectionString()
        {


            return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                               " (HOST = 10.1.3.88)(PORT = 1527)))(CONNECT_DATA = (SERVICE_NAME = QAS))); " +
                               " User Id = WEBSAP; Password = 2INTER7; ";

        }

        [WebMethod]
        public string getDocumenteTEST(string filiala, string departament, string tipDocument)
        {
            OperatiiDocumenteTEST opDocTest = new OperatiiDocumenteTEST();
            return opDocTest.getDocumente(filiala, departament, tipDocument);
        }


        [WebMethod]
        public string getDocumenteTipariteTEST(string filiala, string departament, string dataTip)
        {
            OperatiiDocumenteTEST opDocTest = new OperatiiDocumenteTEST();
            return opDocTest.getDocumenteTiparite(filiala, departament, dataTip);
        }

        [WebMethod]
        public string setPrintedDocumentsTEST(string listaDocumente, string gestionar, string departament, string filiala)
        {
            OperatiiDocumenteTEST opDocTest = new OperatiiDocumenteTEST();
            return opDocTest.setPrintedDocuments(listaDocumente, gestionar, departament, filiala);
        }

        [WebMethod]
        public string setMarfaPregatitaTEST(string nrDocument, string gestionar, string isMarfaPregatita)
        {
            OperatiiDocumenteTEST opDocTest = new OperatiiDocumenteTEST();
            return opDocTest.setMarfaPregatita(nrDocument, gestionar);
        }




        [WebMethod]
        public string getDocumenteNEW(string filiala, string departament, string tipDocument, string depozit)
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
                condFiliala = " and s.werks in " + getUlBuc(filiala);


            string condTipDocument = "";
            string[] intervData = getIntervalCautare().Split(',');

            string exceptieDepozite;


            if (filiala.Equals("BU11"))
                exceptieDepozite = " ('DESC','PRT1','PRT2','02T1','05T1','SRT1','MAV1','MAV2') ";
            else
                exceptieDepozite = " ('DESC','PRT1','PRT2','GAR1','GAR2','02T1','05T1','SRT1','MAV1','MAV2') ";


            string conditieDepozite = "";

            if (depozit != null && (filiala.Equals("IS10") || filiala.Equals("BU10")) && depozit != "0")
            {
                string strDepozit = getDepozite(departament, depozit, filiala);


                if (strDepozit != null && depozit.Equals("1"))
                {
                    strDepozit = getDepozite(departament, depozit, filiala);
                    conditieDepozite = " and s.lgort in " + strDepozit;
                    exceptieDepozite = " ('DESC','PRT1','PRT2','GAR1','GAR2','02T1','05T1','SRT1','MAV1','SRV1') ";
                }
                else
                if (strDepozit != null && depozit.Trim().Length > 0)
                {
                    strDepozit = getDepozite(departament, depozit, filiala);
                    conditieDepozite = " and s.lgort in " + strDepozit;
                    exceptieDepozite = " ('DESC','PRT1','PRT2','GAR1','GAR2','02T1','05T1','SRT1','MAV1','SRV1') ";
                }

            }


            if (tipDocument == null)
                condTipDocument = condFiliala;
            else {

                if (tipDocument.Equals("TRANSFER"))
                {
                    condTipDocument = condFiliala + " and h.lfart = 'UL' ";
                }
                else if (tipDocument.Equals("DISTRIBUTIE"))
                {
                    condTipDocument = " and s.werks in " + getUlDistrib(filiala);
                }
                else
                {
                    condTipDocument = condFiliala;
                }
            }

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                cmd.CommandText = " select x.numecl, x.vbeln , x.emitere,  " +
                                                 " decode(length(x.matnr),18,substr(x.matnr,-8),x.matnr) cod , x.numeart, x.lfimg, x.vrkme, x.posnr, " +
                                                 " to_char(to_date(x.wadat,'yyyymmdd')) livrare, x.pregatire, x.tiparire, x.lfart, x.lgort, nvl(exidv,' '), nvl(x.nume,' '),  " +
                                                 " nvl(com_old,'-1'), x.traty from (select " +
                                                 " h.kunnr, c.nume numecl, h.vbeln, To_Char(To_Date(H.Erdat||' '||H.Erzet,'yyyymmdd hh24miss'),'dd-MON-yyyy HH24:mi','NLS_DATE_LANGUAGE = Romanian') emitere, s.posnr , s.matnr, " +
                                                 " art.nume numeart, s.spart, s.werks, s.lfimg, s.vrkme, k.datbg, h.wadat, " +
                                                 " nvl((select '1' from sapprd.zpregmarfagest g where g.document=h.vbeln and rownum=1),'-1') pregatire, " +
                                                 " nvl((select '1' from sapprd.ztipariredoc g where g.document = h.vbeln and rownum=1),'-1') tiparire, h.lfart, s.lgort, " +
                                                 " e.exidv, sf.nume, " +
                                                 " (select com_referinta from sapprd.zcomhead_tableta q where q.mandt = '900' and q.nrcmdsap = s.vgbel and rownum = 1) com_old, h.traty " +
                                                 " from sapprd.likp h, sapprd.lips s, sapprd.vttp p, sapprd.vttk k, clienti c, articole art, sapprd.vtpa v, sapprd.VEKP e, soferi sf " +
                                                 " where h.mandt = '900'  and s.lgort not in " + exceptieDepozite + conditieDepozite + " and c.cod = h.kunnr  and h.mandt = s.mandt  and h.vbeln = s.vbeln " +
                                                 " and art.cod = s.matnr and h.wadat between :dataStart and :dataStop and art.spart =:depart " +
                                                   condTipDocument + " and nvl(k.datbg, '00000000') = '00000000' and s.lfimg > 0 " +
                                                 " and h.mandt = p.mandt(+) " +
                                                 " and upper(art.nume) not like ('PREST%SERV%') " +
                                                 " and h.vbeln = p.vbeln(+) and h.lfart not in ('EL', 'ZUL', 'ZLR') and p.mandt = k.mandt(+) " +
                                                 " and decode( substr(s.werks, 3, 1),4,(CASE WHEN art.spart = '02' then 'FALSE' else 'TRUE' end),'TRUE') = 'TRUE' " +
                                                 " and ( ( h.traty = 'TRAP' and nvl(k.dalen,'00000000') = '00000000') or h.traty = 'TCLI' or h.traty = 'TERT' ) " +
                                                 " and p.tknum = k.tknum(+) " +
                                                 " and p.mandt = v.mandt(+) and p.tknum = v.vbeln(+) and 'ZF' = v.parvw(+) and p.mandt = e.mandt(+) " +
                                                 " and p.tknum = e.vpobjkey(+)  and sf.cod (+) = v.pernr " +
                                                 " and not exists " +
                                                 " (select tp.ul_stoc from sapprd.vbfa a, sapprd.zcomhead_tableta t, sapprd.zcomdet_tableta tp " +
                                                 " where a.mandt = h.mandt and a.vbeln = h.vbeln and a.posnn = s.posnr and a.vbtyp_n = 'J' and a.vbtyp_v = 'C' " +
                                                 " and a.mandt = t.mandt and a.vbelv = t.nrcmdsap and t.mandt = tp.mandt and t.id = tp.id and ( tp.ul_stoc = 'BV90' or tp.depoz = 'DESC') and rownum = 1) " +
                                                 " ) x, sapprd.zdeliv_deleted d  where nvl(datbg, '00000000') = '00000000' " +
                                                 " and d.mandt(+)='900' and d.comanda(+) = com_old and d.matnr(+) = x.matnr" +
                                                 " and x.lfimg > 0 order by x.vbeln, x.kunnr, x.posnr ";




                cmd.CommandText = " select x.numecl, x.vbeln, x.emitere, decode(length(x.matnr), 18, substr(x.matnr, -8), x.matnr) cod, x.numeart, x.lfimg, x.vrkme, x.posnr, " +
                                  " to_char(to_date(x.wadat, 'yyyymmdd')) livrare, x.pregatire, x.tiparire, x.lfart, x.lgort, nvl(exidv, ' '), nvl(x.nume, ' '),  nvl(com_old, '-1'), x.traty " +
                                  " from (select h.kunnr, c.nume numecl, h.vbeln, " +
                                  " To_Char(To_Date(H.Erdat || ' ' || H.Erzet, 'yyyymmdd hh24miss'), 'dd-MON-yyyy HH24:mi', 'NLS_DATE_LANGUAGE = Romanian') emitere, " +
                                  " s.posnr, s.matnr, art.nume numeart, s.spart, s.werks, s.lfimg, s.vrkme, k.datbg, h.wadat, " +
                                  " nvl((select '1' from sapprd.zpregmarfagest g where g.document = h.vbeln and rownum = 1), '-1') pregatire, " +
                                  " nvl((select '1' from sapprd.ztipariredoc g where g.document = h.vbeln and rownum = 1),'-1') tiparire, " +
                                  " h.lfart, s.lgort, e.exidv, sf.nume, " +
                                  " (select com_referinta from sapprd.zcomhead_tableta q where q.mandt = '900' " +
                                  " and q.nrcmdsap = s.vgbel and rownum = 1) com_old, h.traty " +
                                  " from sapprd.likp h, sapprd.lips s, sapprd.vttp p, sapprd.vttk k, websap.clienti c, websap.articole art, " +
                                  " sapprd.vtpa v, sapprd.VEKP e, websap.soferi sf " +
                                  " where h.mandt = '900' and s.lgort not in " + exceptieDepozite + conditieDepozite +
                                  " and c.cod = h.kunnr and h.mandt = s.mandt and h.vbeln = s.vbeln and art.cod = s.matnr and h.wadat between :dataStart and :dataStop " +
                                  " and art.spart =:depart " + condTipDocument + " and nvl(k.datbg, '00000000') = '00000000' and s.lfimg > 0 " +
                                  " and h.mandt = p.mandt(+) and upper(art.nume)not like('PREST%SERV%') and h.vbeln = p.vbeln(+) " +
                                  " and h.lfart not in ('EL', 'ZUL', 'ZLR') and p.mandt = k.mandt(+) " +
                                  " and decode(substr(s.werks, 3, 1), 4, (CASE WHEN art.spart = '02' then 'FALSE' else 'TRUE' end),'TRUE') = 'TRUE' " +
                                  " and((h.traty = 'TRAP' and nvl(k.dalen, '00000000') = '00000000') or h.traty = 'TCLI' or h.traty = 'TERT') " +
                                  " and p.tknum = k.tknum(+) and p.mandt = v.mandt(+) and p.tknum = v.vbeln(+) and 'ZF' = v.parvw(+) and p.mandt = e.mandt(+) and p.tknum = e.vpobjkey(+) " +
                                  " and sf.cod(+) = v.pernr and not exists " +
                                  " (select tp.ul_stoc from sapprd.vbfa a, sapprd.zcomhead_tableta t, sapprd.zcomdet_tableta  tp where a.mandt = h.mandt " +
                                  " and a.vbeln = h.vbeln and a.posnn = s.posnr and a.vbtyp_n = 'J' and a.vbtyp_v = 'C' and a.mandt = t.mandt and a.vbelv = t.nrcmdsap " +
                                  " and t.mandt = tp.mandt and s.matnr = tp.cod  and a.posnv = lpad(tp.poz, 6, '0') " +
                                  " and t.id = tp.id and(tp.ul_stoc = 'BV90' or tp.depoz = 'DESC' or tp.depoz = 'MAV1') and rownum = 1)) x, sapprd.zdeliv_deleted d " +
                                  " where nvl(datbg, '00000000') = '00000000' and d.mandt(+) = '900' and d.comanda(+) = com_old and d.matnr(+) = x.matnr " +
                                  " and x.lfimg > 0 order by x.vbeln, x.kunnr, x.posnr ";






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
                Document unDocument = new Document();

                string nrDocument = "";
                bool isDocOpen = false;

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {

                        if (nrDocument != oReader.GetString(1))
                        {
                            if (unDocument.id != null)
                                getArticoleSterse(connection, listaDocumente, unDocument);

                            isDocOpen = isDocumentOpen(connection, oReader.GetString(1), oReader.GetString(7));
                        }

                        if (isDocOpen)
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
                            unDocument.depozit = oReader.GetString(12);

                            string numeClient = getNumeClient(connection, unDocument.id);
                            if (!unDocument.client.Equals(numeClient))
                                unDocument.client = unDocument.client + " " + numeClient;

                            unDocument.nrMasina = oReader.GetString(13);
                            unDocument.numeSofer = oReader.GetString(14);
                            unDocument.modificare = "";
                            unDocument.infoStatus = "";
                            unDocument.cantitateModificata = unDocument.cantitate;

                            string comandaAnterioara = oReader.GetString(15);
                            unDocument.comandaVeche = oReader.GetString(15);
                            unDocument.tipTransport = oReader.GetString(16);


                            if (comandaAnterioara != "-1")
                            {
                                string[] statusArticol = getArticoleModificate(connection, comandaAnterioara, unDocument.codArticol, Double.Parse(unDocument.cantitate)).Split('#');

                                if (!statusArticol[0].Equals("-1"))
                                {
                                    unDocument.modificare = statusArticol[1];
                                    unDocument.infoStatus = "Livrare modificata";
                                    unDocument.cantitateModificata = unDocument.cantitate;

                                    if (unDocument.modificare.Contains("adaugat"))
                                    {
                                        unDocument.cantitate = "0";
                                    }
                                    else if (unDocument.modificare.Contains("modificat"))
                                    {
                                        unDocument.cantitate = statusArticol[0];
                                    }

                                }
                            }


                            listaDocumente.Add(unDocument);
                        }

                        nrDocument = oReader.GetString(1);
                    }

                    if (unDocument.id != null)
                        getArticoleSterse(connection, listaDocumente, unDocument);

                }

                listaDocumente.AddRange(getModificariComanda(filiala, departament));

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaDocumente);



            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + cmd.CommandText + " , " + departament + ", depoz: " + depozit);
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





        public static string getUser()
        {
            return "USER_RFC";
        }

        public static string getPass()
        {
            return "2rfc7tes3";
        }


        [WebMethod]
        public string getDocumenteWS(string filiala, string departament, string tipDocument, string depozit)
        {

            OracleConnection connection = new OracleConnection();

            SapWSPregatireMarfa.ZWS_PREGATIRE webService = new SapWSPregatireMarfa.ZWS_PREGATIRE();
            SapWSPregatireMarfa.ZpregatireMarfa inParam = new SapWSPregatireMarfa.ZpregatireMarfa();

            SapWSPregatireMarfa.Werks[] unitLogs;
            
            string[] intervData = getIntervalCautareWS().Split(',');

            string exceptieDepozite;

            if (filiala.Equals("BU11"))
            {
                exceptieDepozite = "DESC,PRT1,PRT2,02T1,05T1,SRT1,MAV1,MAV2";
            }
            else
                exceptieDepozite = "DESC,PRT1,PRT2,GAR1,GAR2,02T1,05T1,SRT1,MAV1,MAV2";


            string[] strDepozitIn = { };

            if (depozit != null && (filiala.Equals("IS10") || filiala.Equals("BU10")) && depozit != "0")
            {
                exceptieDepozite = "DESC,PRT1,PRT2,GAR1,GAR2,02T1,05T1,SRT1,MAV1,SRV1";
                strDepozitIn = getDepoziteWS(departament, depozit, filiala).Split(',');
            }

            SapWSPregatireMarfa.Zslgort[] lgortIn = new SapWSPregatireMarfa.Zslgort[0];

            if (strDepozitIn.Length > 0)
            {
                lgortIn = new SapWSPregatireMarfa.Zslgort[strDepozitIn.Length];

                for (int i=0;i<strDepozitIn.Length;i++)
                {
                    SapWSPregatireMarfa.Zslgort lgtIn = new SapWSPregatireMarfa.Zslgort();
                    lgtIn.Lgort = strDepozitIn[i];
                    lgortIn[i] = lgtIn;
                }

            }

            string[] exceptDepoz = exceptieDepozite.Split(',');

            SapWSPregatireMarfa.Zslgort[] lgortExc = new SapWSPregatireMarfa.Zslgort[exceptDepoz.Length];

            for (int i = 0; i < exceptDepoz.Length; i++)
            {
                SapWSPregatireMarfa.Zslgort dep = new SapWSPregatireMarfa.Zslgort();
                dep.Lgort = exceptDepoz[i].Trim();
                lgortExc[i] = dep;
            }


            if (tipDocument.Equals("TRANSFER"))
            {
                string strUl = getUnitLogsWS(filiala).Split(',')[0];
                unitLogs = new SapWSPregatireMarfa.Werks[1];

                SapWSPregatireMarfa.Werks ul = new SapWSPregatireMarfa.Werks();
                ul.Werks1 = strUl;
                unitLogs[0] = ul;

                tipDocument = "TRANSFER";

            }
            else if (tipDocument.Equals("DISTRIBUTIE"))
            {
                string strUl = getUnitLogsWS(filiala).Split(',')[0];
                unitLogs = new SapWSPregatireMarfa.Werks[1];

                SapWSPregatireMarfa.Werks ul = new SapWSPregatireMarfa.Werks();
                ul.Werks1 = strUl;
                unitLogs[0] = ul;

            }
            else
            {
                string[] strUl = getUnitLogsWS(filiala).Split(',');
                unitLogs = new SapWSPregatireMarfa.Werks[strUl.Length];

                for (int i = 0; i < strUl.Length; i++)
                {
                    SapWSPregatireMarfa.Werks ul = new SapWSPregatireMarfa.Werks();
                    ul.Werks1 = strUl[i];
                    unitLogs[i] = ul;
                }

            }

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());

            webService.Credentials = nc;
            webService.Timeout = 300000;

            inParam.IpDataStart = intervData[0];
            inParam.IpDataStop = intervData[1];
            inParam.IpDepart = departament;
            inParam.ItWerks = unitLogs;
            inParam.ItLgortEx = lgortExc;
            inParam.ItPreg = new SapWSPregatireMarfa.Zspregatire[1];
            inParam.ItLgortIn = lgortIn;
            inParam.IpTip = tipDocument;

            SapWSPregatireMarfa.ZpregatireMarfaResponse outParam = webService.ZpregatireMarfa(inParam);

            int recCount = outParam.ItPreg.Length;
            SapWSPregatireMarfa.Zspregatire[] listaArticole = outParam.ItPreg;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            List<Document> listaDocumente = new List<Document>();
            Document unDocument = new Document();

            string nrDocument = "";
            bool isDocOpen = false;

            for (int i = 0; i < recCount; i++)
            {
                SapWSPregatireMarfa.Zspregatire articol = listaArticole[i];

                if (articol.Traty.Length < 4)
                    continue;

                if (nrDocument != articol.Vbeln)
                {
                    if (unDocument.id != null)
                        getArticoleSterse(connection, listaDocumente, unDocument);

                    isDocOpen = isDocumentOpen(connection, articol.Vbeln, articol.Posnr);
                }

                if (isDocOpen)
                {

                    unDocument = new Document();
                    unDocument.client = articol.Numecl;
                    unDocument.id = articol.Vbeln;
                    unDocument.emitere = parseDate(articol.Erdat + " " + articol.Erzet.ToString().Replace("1/1/0001", "").Trim());
                    unDocument.codArticol = articol.Matnr.TrimStart(new Char[] { '0' });
                    unDocument.numeArticol = articol.Numeart;
                    unDocument.cantitate = articol.Lfimg.ToString("0.##");
                    unDocument.um = articol.Vrkme;
                    unDocument.pozitieArticol = articol.Posnr;
                    unDocument.isPregatit = articol.Pregatire;
                    unDocument.isTiparit = articol.Tiparire;
                    unDocument.tip = articol.Lfart.Equals("UL") ? "T" : "D";
                    unDocument.depozit = articol.Lgort;
                    unDocument.client = articol.Numecl;

                    string numeClient = getNumeClient(connection, unDocument.id);
                    if (!unDocument.client.Equals(numeClient))
                        unDocument.client = unDocument.client + " " + numeClient;


                    unDocument.nrMasina = articol.Exidv;
                    unDocument.numeSofer = articol.Nume;
                    unDocument.modificare = "";
                    unDocument.infoStatus = "";
                    unDocument.cantitateModificata = unDocument.cantitate;
                    unDocument.cantFractie = articol.LfimgFrac.ToString("0.##");
                    unDocument.nrPaleti = articol.EpaQty.ToString("0.##");
                    unDocument.umPaleti = articol.TipP;
                    unDocument.palBuc = articol.EpaBuc.ToString("0.##");

                    string comandaAnterioara = articol.ComOld;
                    unDocument.comandaVeche = articol.ComOld;
                    unDocument.tipTransport = articol.Traty;

                    if (comandaAnterioara != "-1")
                    {
                        string[] statusArticol = getArticoleModificate(connection, comandaAnterioara, unDocument.codArticol, Double.Parse(unDocument.cantitate)).Split('#');

                        if (!statusArticol[0].Equals("-1"))
                        {
                            unDocument.modificare = statusArticol[1];
                            unDocument.infoStatus = "Livrare modificata";
                            unDocument.cantitateModificata = unDocument.cantitate;

                            if (unDocument.modificare.Contains("adaugat"))
                            {
                                unDocument.cantitate = "0";
                            }
                            else if (unDocument.modificare.Contains("modificat"))
                            {
                                unDocument.cantitate = statusArticol[0];
                            }

                        }
                    }
                    listaDocumente.Add(unDocument);

                }
                nrDocument = articol.Vbeln;

            }

            listaDocumente.AddRange(getModificariComanda(filiala, departament));

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string serializedResult = serializer.Serialize(listaDocumente);

            connection.Close();


            return serializedResult;
        }



        [WebMethod]
        public string getDocumente(string filiala, string departament, string tipDocument, string depozit)
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
                condFiliala = " and s.werks in " + getUlBuc(filiala);


            string condTipDocument = "";
            string[] intervData = getIntervalCautare().Split(',');

            string exceptieDepozite;


            if (filiala.Equals("BU11"))
                exceptieDepozite = " ('DESC','PRT1','PRT2','02T1','05T1','SRT1','MAV1','MAV2') ";
            else
                exceptieDepozite = " ('DESC','PRT1','PRT2','GAR1','GAR2','02T1','05T1','SRT1','MAV1','MAV2') ";


            string conditieDepozite = "";

            if (depozit != null && (filiala.Equals("IS10") || filiala.Equals("BU10")) && depozit != "0")
            {
                string strDepozit = getDepozite(departament, depozit, filiala);


                if (strDepozit != null && depozit.Equals("1"))
                {
                    strDepozit = getDepozite(departament, depozit, filiala);
                    conditieDepozite = " and s.lgort in " + strDepozit;
                    exceptieDepozite = " ('DESC','PRT1','PRT2','GAR1','GAR2','02T1','05T1','SRT1','MAV1','SRV1') ";
                }
                else
                if (strDepozit != null && depozit.Trim().Length > 0)
                {
                    strDepozit = getDepozite(departament, depozit, filiala);
                    conditieDepozite = " and s.lgort in " + strDepozit;
                    exceptieDepozite = " ('DESC','PRT1','PRT2','GAR1','GAR2','02T1','05T1','SRT1','MAV1','SRV1') ";
                }

            }


            if (tipDocument == null)
                condTipDocument = condFiliala;
            else {

                if (tipDocument.Equals("TRANSFER"))
                {
                    condTipDocument = condFiliala + " and h.lfart = 'UL' ";
                }
                else if (tipDocument.Equals("DISTRIBUTIE"))
                {
                    condTipDocument = " and s.werks in " + getUlDistrib(filiala);
                }
                else
                {
                    condTipDocument = condFiliala;
                }
            }

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();





                cmd.CommandText = " select x.numecl, x.vbeln, x.emitere, decode(length(x.matnr), 18, substr(x.matnr, -8), x.matnr) cod, x.numeart, x.lfimg, x.vrkme, x.posnr, " +
                             " to_char(to_date(x.wadat, 'yyyymmdd')) livrare, x.pregatire, x.tiparire, x.lfart, x.lgort, nvl(exidv, ' '), nvl(x.nume, ' '),  nvl(com_old, '-1'), x.traty, x.sintetic " +
                             " from (select h.kunnr, c.nume numecl, h.vbeln, " +
                             " To_Char(To_Date(H.Erdat || ' ' || H.Erzet, 'yyyymmdd hh24miss'), 'dd-MON-yyyy HH24:mi', 'NLS_DATE_LANGUAGE = Romanian') emitere, " +
                             " s.posnr, s.matnr, art.nume numeart, s.spart, s.werks, s.lfimg, s.vrkme, k.datbg, h.wadat, " +
                             " nvl((select '1' from sapprd.zpregmarfagest g where g.document = h.vbeln and rownum = 1), '-1') pregatire, " +
                             " nvl((select '1' from sapprd.ztipariredoc g where g.document = h.vbeln and rownum = 1),'-1') tiparire, " +
                             " h.lfart, s.lgort, e.exidv, sf.nume, " +
                             " (select com_referinta from sapprd.zcomhead_tableta q where q.mandt = '900' " +
                             " and q.nrcmdsap = s.vgbel and rownum = 1) com_old, h.traty, art.sintetic " +
                             " from sapprd.likp h, sapprd.lips s, sapprd.vttp p, sapprd.vttk k, websap.clienti c, websap.articole art, " +
                             " sapprd.vtpa v, sapprd.VEKP e, websap.soferi sf " +
                             " where h.mandt = '900' and s.lgort not in " + exceptieDepozite + conditieDepozite +
                             " and c.cod = h.kunnr and h.mandt = s.mandt and h.vbeln = s.vbeln and art.cod = s.matnr and h.wadat between :dataStart and :dataStop " +
                             " and art.gest_spart =:depart " + condTipDocument + " and nvl(k.datbg, '00000000') = '00000000' and s.lfimg > 0 " +
                             " and h.mandt = p.mandt(+) and upper(art.nume)not like('PREST%SERV%') and h.vbeln = p.vbeln(+) " +
                             " and h.lfart not in ('EL', 'ZUL', 'ZLR') and p.mandt = k.mandt(+) " +
                             " and decode(substr(s.werks, 3, 1), 4, (CASE WHEN art.gest_spart = '02' then 'FALSE' else 'TRUE' end),'TRUE') = 'TRUE' " +
                             " and((h.traty = 'TRAP' and nvl(k.dalen, '00000000') = '00000000') or h.traty = 'TCLI' or h.traty = 'TERT') " +
                             " and p.tknum = k.tknum(+) and p.mandt = v.mandt(+) and p.tknum = v.vbeln(+) and 'ZF' = v.parvw(+) and p.mandt = e.mandt(+) and p.tknum = e.vpobjkey(+) " +
                             " and sf.cod(+) = v.pernr and not exists " +
                             " (select tp.ul_stoc from sapprd.vbfa a, sapprd.zcomhead_tableta t, sapprd.zcomdet_tableta  tp where a.mandt = h.mandt " +
                             " and a.vbeln = h.vbeln and a.posnn = s.posnr and a.vbtyp_n = 'J' and a.vbtyp_v = 'C' and a.mandt = t.mandt and a.vbelv = t.nrcmdsap " +
                             " and t.mandt = tp.mandt and s.matnr = tp.cod  and a.posnv = lpad(tp.poz, 6, '0') " +
                             " and t.id = tp.id and(tp.ul_stoc = 'BV90' or tp.depoz = 'DESC' or tp.depoz = 'MAV1') and rownum = 1)) x, sapprd.zdeliv_deleted d " +
                             " where nvl(datbg, '00000000') = '00000000' and d.mandt(+) = '900' and d.comanda(+) = com_old and d.matnr(+) = x.matnr " +
                             " and x.lfimg > 0 order by x.vbeln, x.kunnr, x.sintetic, x.posnr ";



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
                Document unDocument = new Document();

                string nrDocument = "";
                bool isDocOpen = false;

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {

                        if (nrDocument != oReader.GetString(1))
                        {
                            if (unDocument.id != null)
                                getArticoleSterse(connection, listaDocumente, unDocument);

                            isDocOpen = isDocumentOpen(connection, oReader.GetString(1), oReader.GetString(7));
                        }

                        if (isDocOpen)
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
                            unDocument.depozit = oReader.GetString(12);

                            string numeClient = getNumeClient(connection, unDocument.id);
                            if (!unDocument.client.Equals(numeClient))
                                unDocument.client = unDocument.client + " " + numeClient;

                            unDocument.nrMasina = oReader.GetString(13);
                            unDocument.numeSofer = oReader.GetString(14);
                            unDocument.modificare = "";
                            unDocument.infoStatus = "";
                            unDocument.cantitateModificata = unDocument.cantitate;

                            unDocument.cantFractie = "0";
                            unDocument.nrPaleti = "0";
                            unDocument.umPaleti = "PLI";

                            string comandaAnterioara = oReader.GetString(15);
                            unDocument.comandaVeche = oReader.GetString(15);
                            unDocument.tipTransport = oReader.GetString(16);


                            if (comandaAnterioara != "-1")
                            {
                                string[] statusArticol = getArticoleModificate(connection, comandaAnterioara, unDocument.codArticol, Double.Parse(unDocument.cantitate)).Split('#');

                                if (!statusArticol[0].Equals("-1"))
                                {
                                    unDocument.modificare = statusArticol[1];
                                    unDocument.infoStatus = "Livrare modificata";
                                    unDocument.cantitateModificata = unDocument.cantitate;

                                    if (unDocument.modificare.Contains("adaugat"))
                                    {
                                        unDocument.cantitate = "0";
                                    }
                                    else if (unDocument.modificare.Contains("modificat"))
                                    {
                                        unDocument.cantitate = statusArticol[0];
                                    }

                                }
                            }


                            listaDocumente.Add(unDocument);
                        }

                        nrDocument = oReader.GetString(1);
                    }

                    if (unDocument.id != null)
                        getArticoleSterse(connection, listaDocumente, unDocument);

                }

                listaDocumente.AddRange(getModificariComanda(filiala, departament));

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaDocumente);



            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + cmd.CommandText + " , " + departament + ", depoz: " + depozit);
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


        [WebMethod]
        public void getModificariComanda()
        {
            new Documente().getModificariComanda("GL10", "08");
        }



        private List<Document> getModificariComanda(string filiala, string departament)
        {

            List<Document> listaDocumente = new List<Document>();

            List<Livrare> livrariModificate = getLivrariModificate(filiala, departament);

            foreach (Livrare livrareModificata in livrariModificate)
            {
                List<Articol> articoleNoi = getComandaNoua(livrareModificata.nrComanda);


                bool comandaStearsa = true;
                foreach (Articol articolSters in livrareModificata.listArticole)
                {
                    bool articolGasit = false;
                    foreach (Articol articolNou in articoleNoi)
                    {
                        if (articolSters.codArticol.Equals(articolNou.codArticol))
                        {
                            articolGasit = true;
                            comandaStearsa = false;
                            break;
                        }

                    }

                    string depozit = "";
                    if (comandaStearsa)
                        depozit = getDepozitLivrareStearsa(livrareModificata.nrComanda);

                    if (!articolGasit)
                    {
                        Document docNou = new Document();
                        docNou.client = livrareModificata.numeClient;
                        docNou.id = livrareModificata.nrLivrare;
                        docNou.emitere = livrareModificata.emitere;
                        docNou.isPregatit = "-1";
                        docNou.isTiparit = livrareModificata.tiparit;
                        docNou.codArticol = articolSters.codArticol;
                        docNou.cantitate = articolSters.cantitate.ToString();
                        docNou.um = articolSters.um;
                        docNou.numeArticol = articolSters.numeArticol;
                        docNou.pozitieArticol = articolSters.poz;
                        docNou.depozit = depozit;
                        docNou.modificare = "Articol sters";
                        docNou.tip = "D";
                        docNou.nrMasina = " ";
                        docNou.numeSofer = " ";
                        docNou.cantitateModificata = "0";


                        docNou.tipTransport = "TRAP";
                        if (comandaStearsa)
                            docNou.infoStatus = "Livrare stearsa";
                        else
                            docNou.infoStatus = "Livrare modificata";
                        listaDocumente.Add(docNou);
                    }
                }


            }

            return listaDocumente;

        }


        public List<Livrare> getLivrariModificate(string filiala, string departament)
        {

            List<Livrare> listLivrari = new List<Livrare>();
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string[] intervData = getIntervalCautare().Split(',');

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select  comanda, decode(length(matnr),18,substr(matnr,-8),matnr) matnr, lfimg, vrkme, vbeln, nume_client, " +
                                  " To_Char(To_Date(Erdat||' '||Erzet,'yyyymmdd hh24miss'),'dd-MON-yyyy HH24:mi','NLS_DATE_LANGUAGE = Romanian'), maktx, posnr, " +
                                  " nvl((select '1' from sapprd.zpregmarfagest g where g.document = vbeln and rownum=1),'-1') pregatire, " +
                                  " nvl((select '1' from sapprd.ztipariredoc g where g.document = vbeln and rownum=1),'-1') tiparire " +
                                  " from sapprd.zdeliv_deleted where pregatit = 'X' and werks =:filiala and spart=:depart and erdat >=:datac " +
                                  " order by comanda, posnr ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = departament;

                cmd.Parameters.Add(":datac", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = intervData[0];

                oReader = cmd.ExecuteReader();

                int starePregatireLivrare = -1;
                string nrComanda = "";
                string nrLivrare = "";
                string numeClient = "";
                string emitere = "";
                string pregatire = "";
                string tiparire = "";
                Livrare livrare = new Livrare();
                List<Articol> listArticole = new List<Articol>();
                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {


                        if (!nrComanda.Equals(oReader.GetString(0)) && nrComanda != "")
                        {
                            starePregatireLivrare = getStarePregatireLivrare(connection, nrLivrare);

                            livrare = new Livrare();
                            livrare.nrComanda = nrComanda;
                            livrare.nrLivrare = nrLivrare;
                            livrare.numeClient = numeClient;
                            livrare.emitere = emitere;
                            livrare.pregatire = starePregatireLivrare.ToString();
                            livrare.tiparit = tiparire;
                            livrare.listArticole = listArticole;



                            if (starePregatireLivrare != 2)
                                listLivrari.Add(livrare);

                            listArticole = new List<Articol>();
                        }

                        Articol articol = new Articol();
                        articol.codArticol = oReader.GetString(1);
                        articol.cantitate = oReader.GetDouble(2);
                        articol.um = oReader.GetString(3);
                        articol.numeArticol = oReader.GetString(7);
                        articol.poz = oReader.GetString(8);
                        listArticole.Add(articol);

                        nrComanda = oReader.GetString(0);
                        nrLivrare = oReader.GetString(4);
                        numeClient = oReader.GetString(5);
                        emitere = oReader.GetString(6);
                        pregatire = oReader.GetString(9);
                        tiparire = oReader.GetString(10);
                    }


                    starePregatireLivrare = getStarePregatireLivrare(connection, nrLivrare);

                    livrare = new Livrare();
                    livrare.nrComanda = nrComanda;
                    livrare.nrLivrare = nrLivrare;
                    livrare.numeClient = numeClient;
                    livrare.emitere = emitere;
                    livrare.pregatire = starePregatireLivrare.ToString();
                    livrare.tiparit = tiparire;
                    livrare.listArticole = listArticole;


                    if (starePregatireLivrare != 2)
                        listLivrari.Add(livrare);

                }
            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return listLivrari;
        }


        private int getStarePregatireLivrare(OracleConnection connection, string idLivrare)
        {
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            int starePregatire = -1;
            try
            {

                string sqlString = " select doc_anulat from (select doc_anulat from sapprd.zpregmarfagest g where g.document =:idLivrare " +
                                   " order by to_date(datac || ' ' || orac, 'yyyy-mm-dd HH24:mi:ss') desc) where rownum<= 1  ";

                cmd = connection.CreateCommand();
                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idLivrare", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idLivrare;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        if (oReader.GetString(0).Equals("X"))
                            starePregatire = 2;


                    }
                }
            }

            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }
            finally
            {

            }

            return starePregatire;

        }

        public List<Articol> getComandaNoua(string nrComanda)
        {
            List<Articol> listArticole = new List<Articol>();
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();
                cmd.CommandText = " select a.status, decode(length(b.cod),18,substr(b.cod,-8),b.cod) cod, b.cantitate, b.um, b.depoz, c.nume, b.poz, a.mt from " +
                                  " sapprd.zcomhead_tableta a, sapprd.zcomdet_tableta b, articole c where " +
                                  " c.cod = b.cod and " +
                                  " a.com_referinta =:nrCmd and a.id = b.id and a.status != '6' order by b.poz ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();


                cmd.Parameters.Add(":nrCmd", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrComanda;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        if (oReader.GetString(0).Equals("6"))
                            break;

                        Articol articol = new Articol();
                        articol.codArticol = oReader.GetString(1);
                        articol.cantitate = oReader.GetDouble(2);
                        articol.um = oReader.GetString(3);
                        articol.depozit = oReader.GetString(4);
                        articol.numeArticol = oReader.GetString(5);
                        articol.poz = Int32.Parse(oReader.GetString(6).Trim()).ToString();
                        articol.transport = oReader.GetString(7);
                        listArticole.Add(articol);
                    }
                }

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }


            return listArticole;
        }



        public string getDepozitLivrareStearsa(string nrComanda)
        {
            string depozit = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                cmd.CommandText = " select distinct depoz from sapprd.zcomdet_tableta  " +
                                  " where id = (select id from sapprd.zcomhead_tableta where nrcmdsap = :nrCmd) ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrCmd", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrComanda;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    depozit = oReader.GetString(0);
                }
            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }


            return depozit;
        }


        private string getDepozite(string codDepartament, string codDepozit, string filiala)
        {
            string depozite = "";
            string antetDepVanz = codDepartament + "V";
            string antetDepGrat = codDepartament + "G";
            string antetDepDet = codDepartament + "D";


            if (codDepozit == "1")
                depozite = "('" + antetDepVanz + codDepozit + "','" + antetDepGrat + codDepozit + "','" + antetDepDet + codDepozit + "', 'SRV2','BRIC')";
            else if (codDepozit == "2")
                depozite = "('" + antetDepVanz + codDepozit + "','" + antetDepGrat + codDepozit + "','" + antetDepDet + codDepozit + "','MAV1','SRV1')";
            else if (codDepozit == "3" || codDepozit == "4")
                depozite = "('" + antetDepVanz + codDepozit + "','" + antetDepGrat + codDepozit + "','" + antetDepDet + codDepozit + "','MAV2','SRV2')";
            else if (codDepozit == "5")
                depozite = "('TAMP')";
            else if (codDepozit == "6")
                depozite = "('MAV1', 'MAV2','SRV1','SRV2')";
            else if (codDepozit == "7")
                depozite = "('MAV1', 'MAV2')";
            else if (codDepozit == "6")
                depozite = "('SRV1','SRV2')";
            else if (codDepozit == "100")
                depozite = "('GAR1', 'GAR2')";

            return depozite;
        }


        private string getDepoziteWS(string codDepartament, string codDepozit, string filiala)
        {
            string depozite = "";
            string antetDepVanz = codDepartament + "V";
            string antetDepGrat = codDepartament + "G";
            string antetDepDet = codDepartament + "D";


            if (codDepozit == "1")
                depozite =  antetDepVanz + codDepozit + "," + antetDepGrat + codDepozit + "," + antetDepDet + codDepozit + ",SRV2,BRIC";
            else if (codDepozit == "2")
                depozite =  antetDepVanz + codDepozit + "," + antetDepGrat + codDepozit + "," + antetDepDet + codDepozit + ",MAV1,SRV1";
            else if (codDepozit == "3" || codDepozit == "4")
                depozite = antetDepVanz + codDepozit + "," + antetDepGrat + codDepozit + "," + antetDepDet + codDepozit + ",MAV2,SRV2";
            else if (codDepozit == "5")
                depozite = "TAMP";
            else if (codDepozit == "6")
                depozite = "MAV1,MAV2,SRV1,SRV2";
            else if (codDepozit == "7")
                depozite = "MAV1,MAV2";
            else if (codDepozit == "6")
                depozite = "SRV1,SRV2";
            else if (codDepozit == "100")
                depozite = "GAR1,GAR2";

            return depozite;
        }



        private bool isDocumentOpen(OracleConnection connection, string nrDocument, string pozitie)
        {
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            Boolean isOpen = true;

            try
            {
                string sqlString = " select stat from miscstat where mblnr = (select distinct vbeln from sapprd.vbfa f where f.mandt = '900' and f.vbtyp_v = 'J' " +
                                   " and f.vbelv =:nrDocument and f.posnv =:pozitie and f.vbtyp_v = 'J' and f.vbtyp_n = 'R' and rownum<2 )  and rownum< 2  ";

                cmd = connection.CreateCommand();
                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrDocument", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                cmd.Parameters.Add(":pozitie", OracleType.VarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = pozitie;

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
                sendErrorToMail(ex.ToString() + " , " + nrDocument);
            }
            finally
            {
                if (oReader != null)
                {
                    oReader.Close();
                    oReader.Dispose();
                }

                cmd.Dispose();


            }

            return isOpen;

        }



        public static string getNumeClient(OracleConnection connection, string nrDocument)
        {
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string numeClient = "";

            try
            {
                string sqlString = "  select b.name1 from sapprd.vbpa a, sapprd.adrc b  Where A.Mandt = '900' and a.vbeln =:nrDocument " +
                                   "  and a.parvw = 'WE' And A.Adrnr = B.Addrnumber And B.Client = '900'  ";

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
                    numeClient = oReader.GetString(0);
                }



            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + " , " + nrDocument);
            }
            finally
            {
                if (oReader != null)
                {
                    oReader.Close();
                    oReader.Dispose();
                }

                cmd.Dispose();


            }

            return numeClient;
        }



        [WebMethod]
        public static string getIntervalCautare()
        {
            string dayName = DateTime.Now.DayOfWeek.ToString();

            DateTime dateStart = DateTime.Now.AddDays(-3);
            DateTime dateStop = DateTime.Now.AddDays(1);


            if (dayName.Equals("Friday", StringComparison.InvariantCultureIgnoreCase))
                dateStop = DateTime.Now.AddDays(3);

            if (dayName.Equals("Saturday", StringComparison.InvariantCultureIgnoreCase))
                dateStop = DateTime.Now.AddDays(2);

            string strStart = dateStart.Year + dateStart.Month.ToString("00") + dateStart.Day.ToString("00");
            string strStop = dateStop.Year + dateStop.Month.ToString("00") + dateStop.Day.ToString("00");

            return strStart + "," + strStop;


        }


        public static string getIntervalCautareWS()
        {
            string dayName = DateTime.Now.DayOfWeek.ToString();

            DateTime dateStart = DateTime.Now.AddDays(-3);
            DateTime dateStop = DateTime.Now.AddDays(1);


            if (dayName.Equals("Friday", StringComparison.InvariantCultureIgnoreCase))
                dateStop = DateTime.Now.AddDays(3);

            if (dayName.Equals("Saturday", StringComparison.InvariantCultureIgnoreCase))
                dateStop = DateTime.Now.AddDays(2);

            string strStart = dateStart.Year + "-" + dateStart.Month.ToString("00") + "-" + dateStart.Day.ToString("00");
            string strStop = dateStop.Year + "-" + dateStop.Month.ToString("00") + "-" + dateStop.Day.ToString("00");

            return strStart + "," + strStop;


        }


        private static string getUnitLogsWS(string unitLog)
        {
            string unitLogs = "";

            if (unitLog.Equals("BV10"))
                unitLogs = "BV10,BU11,BV20,BV30,BV50";
            else if (unitLog.Equals("BV90"))
                unitLogs = "BV90";
            else if (unitLog.StartsWith("BU"))
                unitLogs = getUlBuc(unitLog).Replace("(", "").Replace(")", "").Replace("'","").Trim();
            else
                unitLogs = getUnitLogsDistributieWS(unitLog);

            return unitLogs;
        }



        public static string getUnitLogsDistributieWS(string unitLog)
        {
            return unitLog.Substring(0, 2) + "10" + "," + unitLog.Substring(0, 2) + "11"  + "," + unitLog.Substring(0, 2) + "20" + "," + unitLog.Substring(0, 2) + "30";

        }



        public static string getUlBuc(String ul)
        {
            switch (ul)
            {

                case "BU10":
                    return " ('BU10','BU20','BU30') ";

                case "BU11":
                    return " ('BU11','BU21','BU31') ";

                case "BU12":
                    return " ('BU12','BU22','BU32') ";

                case "BU13":
                    return " ('BU13','BU23','BU33') ";

                default:
                    return " ('NN') ";
            }



        }


        public static string getUlDistrib(String filiala)
        {
            return " ('" + filiala.Substring(0, 2) + "1" + filiala.Substring(3, 1) + "')";

        }



        [WebMethod]
        public string getDocumenteTiparite(string filiala, string departament, string dataTip)
        {
            string serializedResult = "";



            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = prdConnectionString();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct y.numecl, y.vbeln, y.emitere, y.cod , y.numeart, y.lfimg, y.meins, y.posnr , y.kunnr, y.pregatire from ( " +
                                   " select x.numecl, x.vbeln, x.emitere, decode(length(x.matnr),18,substr(x.matnr,-8),x.matnr) cod , x.numeart, x.lfimg, x.meins, x.posnr, x.kunnr, " +
                                   " nvl((select '1' from sapprd.zpregmarfagest g where g.document = x.vbeln and rownum=1),'-1') pregatire " +
                                   "  from (select distinct " +
                                   " h.kunnr, c.nume numecl, h.vbeln, to_char(to_date(h.erdat,'yyyymmdd')) emitere, s.posnr , s.matnr, " +
                                   " art.nume numeart, s.spart, s.werks, s.lfimg, s.meins,  k.datbg " +
                                   " from sapprd.likp h, sapprd.lips s, sapprd.vttp p, sapprd.vttk k, clienti c, articole art " +
                                   " where h.mandt = '900'  and c.cod = h.kunnr  and h.mandt = s.mandt  and h.vbeln = s.vbeln " +
                                   " and art.cod = s.matnr and art.gest_spart =:depart and substr(s.matnr,11,1) != '3'" +
                                   " and substr(s.werks,0,2) =:filiala and h.mandt = p.mandt(+) " +
                                   " and h.vbeln = p.vbeln(+) and p.mandt = k.mandt(+) " +
                                   " and p.tknum = k.tknum(+)  " +
                                   " ) x, sapprd.ztipariredoc d  where  " +
                                   " d.document = x.vbeln and d.data = :dataTip ) y  order by y.kunnr, y.posnr ";





                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();


                cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = departament;

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filiala.Substring(0, 2);

                cmd.Parameters.Add(":dataTip", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = dataTip;



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
                        unDocument.isPregatit = oReader.GetString(9);
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
                sendErrorToMail(ex.ToString());

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

            string connectionString = prdConnectionString();

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
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }




            return retVal;
        }



        [WebMethod]
        public string userLogon(string userId, string userPass, string ipAdr)
        {

            string serializedResult = "";

            string retVal = "";
            OracleConnection connection = null;
            OracleCommand cmd = new OracleCommand();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection = new OracleConnection();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();
                cmd.CommandText = "web_pkg.wlogin";
                cmd.CommandType = CommandType.StoredProcedure;

                OracleParameter user = new OracleParameter("usname", OracleType.VarChar);
                user.Direction = ParameterDirection.Input;
                user.Value = userId;
                cmd.Parameters.Add(user);

                OracleParameter pass = new OracleParameter("uspass", OracleType.VarChar);
                pass.Direction = ParameterDirection.Input;
                pass.Value = userPass;
                cmd.Parameters.Add(pass);

                OracleParameter resp = new OracleParameter("x", OracleType.Number);
                resp.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(resp);

                OracleParameter depart = new OracleParameter("z", OracleType.NChar, 10);
                depart.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(depart);

                OracleParameter comp = new OracleParameter("w", OracleType.NChar, 20);
                comp.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(comp);

                OracleParameter tipAcces = new OracleParameter("k", OracleType.Number, 2);
                tipAcces.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(tipAcces);

                OracleParameter ipAddr = new OracleParameter("ipAddr", OracleType.VarChar, 15);
                ipAddr.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(ipAddr);

                OracleParameter idAg = new OracleParameter("agentID", OracleType.Number, 5);
                idAg.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(idAg);

                OracleParameter userName = new OracleParameter("numeUser", OracleType.NChar, 128);
                userName.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(userName);

                OracleParameter usrId = new OracleParameter("userID", OracleType.Number, 5);
                usrId.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(usrId);

                cmd.ExecuteNonQuery();

                UserInfo userInfo = new UserInfo();
                userInfo.logonStatus = "Status_" + resp.Value.ToString();
                userInfo.departament = depart.Value.ToString().Trim().Equals("NA") ? "0000" : depart.Value.ToString().Trim();
                userInfo.filiala = comp.Value.ToString().Trim();
                userInfo.numeUser = userName.Value.ToString().Trim();
                userInfo.codUser = idAg.Value.ToString();
                userInfo.tipAcces = tipAcces.Value.ToString();
                userInfo.depozit = getLocatieDepozit(connection, userId);
                setUserDepartamente(connection, userInfo, usrId.Value.ToString());

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(userInfo);


            }
            catch (Exception ex)
            {
                retVal = "-1";
                sendErrorToMail(ex.ToString() + " userid = " + userId);
                retVal = ex.ToString();
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }




            return serializedResult;
        }


        private void setUserDepartamente(OracleConnection conn, UserInfo userInfo, string codUser)
        {

            OracleCommand cmd = conn.CreateCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = " select d2.nume, d1.depid from user_departament d1, departamente d2 where d1.userid = :userId and d1.depid != 'DIVE' " +
                                  " and d1.depid = d2.cod ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":userId", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codUser;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        userInfo.extraDep += oReader.GetString(0) + ";";
                    }
                }

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }


        }



        private string getLocatieDepozit(OracleConnection conn, string userName)
        {
            string locatie = "-1";

            OracleCommand cmd = conn.CreateCommand();
            OracleDataReader oReader = null;


            try
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = " select depoz from  web_users where web_pkg.decrypt(useru) = :numeUser ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":numeUser", OracleType.NVarChar, 128).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = userName;


                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        locatie = oReader.GetString(0);

                    }
                }
            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                cmd.Dispose();
                oReader = null;
            }



            return locatie;
        }

        [WebMethod]
        public string setMarfaPregatita_old(string nrDocument, string gestionar, string isMarfaPreg)
        {
            string retVal = "0", query = "";


            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;

            string connectionString = prdConnectionString();

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

                cmd = connection.CreateCommand();

                if (Boolean.Parse(isMarfaPreg))
                {
                    query = " insert into sapprd.zpregmarfagest(mandt, codgestionar, document, datac, orac) " +
                            " values ('900', :codGest, :document, :datac, :orac) ";
                }
                else
                {
                    query = " delete from sapprd.zpregmarfagest where document=:document ";
                }


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":document", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                if (Boolean.Parse(isMarfaPreg))
                {
                    cmd.Parameters.Add(":codGest", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = gestionar;
                }

                if (Boolean.Parse(isMarfaPreg))
                {
                    cmd.Parameters.Add(":datac", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = nowDate;

                    cmd.Parameters.Add(":orac", OracleType.NVarChar, 18).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = nowTime;
                }

                cmd.ExecuteNonQuery();

            }

            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + " doc = " + nrDocument + " gest = " + gestionar);
            }
            finally
            {

                if (cmd != null)
                    cmd.Dispose();

                connection.Close();
                connection.Dispose();
            }




            return retVal;
        }


        private bool isDocumentTiparit(OracleConnection connection, string nrDocument, string codGest)
        {
            OracleCommand cmd = connection.CreateCommand();
            OracleDataReader oReader = null;

            bool documentExist = false;
            try
            {

                cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = " select 1 from sapprd.ztipariredoc where document=:document and codgestionar=:codGest ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":document", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                cmd.Parameters.Add(":codGest", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codGest;


                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                    documentExist = true;

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }
            finally
            {

                if (oReader != null)
                    oReader.Close();

                if (cmd != null)
                    cmd.Dispose();

            }

            return documentExist;

        }

        public static void sendErrorToMail(string errMsg)
        {

            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress("Android.WebService@arabesque.ro");
                message.To.Add(new MailAddress("florin.brasoveanu@arabesque.ro"));
                message.Subject = "Android WebService Error";
                message.Body = errMsg;
                SmtpClient client = new SmtpClient("mail.arabesque.ro");
                client.Send(message);
            }
            catch (Exception)
            {

            }

        }



        [WebMethod]
        public string getDocumenteBeta(string filiala, string departament, string tipDocument, string depozit)
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
                condFiliala = " and s.werks in " + getUlBuc(filiala);


            string condTipDocument = "";
            string[] intervData = getIntervalCautare().Split(',');

            string exceptieDepozite;


            if (filiala.Equals("BU11"))
                exceptieDepozite = " ('DESC','PRT1','PRT2','02T1','05T1','SRT1','MAV1','MAV2') ";
            else
                exceptieDepozite = " ('DESC','PRT1','PRT2','GAR1','GAR2','02T1','05T1','SRT1','MAV1','MAV2') ";


            string conditieDepozite = "";

            if (depozit != null && (filiala.Equals("IS10") || filiala.Equals("BU10")) && depozit != "0")
            {
                string strDepozit = getDepozite(departament, depozit, filiala);


                if (strDepozit != null && depozit.Equals("1"))
                {
                    strDepozit = getDepozite(departament, depozit, filiala);
                    conditieDepozite = " and s.lgort in " + strDepozit;
                    exceptieDepozite = " ('DESC','PRT1','PRT2','GAR1','GAR2','02T1','05T1','SRT1','MAV1','SRV1') ";
                }
                else
                if (strDepozit != null && depozit.Trim().Length > 0)
                {
                    strDepozit = getDepozite(departament, depozit, filiala);
                    conditieDepozite = " and s.lgort in " + strDepozit;
                    exceptieDepozite = " ('DESC','PRT1','PRT2','GAR1','GAR2','02T1','05T1','SRT1','MAV1','SRV1') ";
                }

            }


            if (tipDocument == null)
                condTipDocument = condFiliala;
            else {

                if (tipDocument.Equals("TRANSFER"))
                {
                    condTipDocument = condFiliala + " and h.lfart = 'UL' ";
                }
                else if (tipDocument.Equals("DISTRIBUTIE"))
                {
                    condTipDocument = " and s.werks in " + getUlDistrib(filiala);
                }
                else
                {
                    condTipDocument = condFiliala;
                }
            }

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                cmd.CommandText = " select x.numecl, x.vbeln , x.emitere,  " +
                                  " decode(length(x.matnr),18,substr(x.matnr,-8),x.matnr) cod , x.numeart, x.lfimg, x.vrkme, x.posnr, " +
                                  " to_char(to_date(x.wadat,'yyyymmdd')) livrare, x.pregatire, x.tiparire, x.lfart, x.lgort, nvl(exidv,' '), nvl(x.nume,' '),  " +
                                  " com_old, nvl(d.lfimg,-1) cant_old from (select " +
                                  " h.kunnr, c.nume numecl, h.vbeln, To_Char(To_Date(H.Erdat||' '||H.Erzet,'yyyymmdd hh24miss'),'dd-MON-yyyy HH24:mi','NLS_DATE_LANGUAGE = Romanian') emitere, s.posnr , s.matnr, " +
                                  " art.nume numeart, s.spart, s.werks, s.lfimg, s.vrkme, k.datbg, h.wadat, " +
                                  " nvl((select '1' from sapprd.zpregmarfagest g where g.document=h.vbeln and rownum=1),'-1') pregatire, " +
                                  " nvl((select '1' from sapprd.ztipariredoc g where g.document = h.vbeln and rownum=1),'-1') tiparire, h.lfart, s.lgort, " +
                                  " e.exidv, sf.nume, " +
                                  " (select com_referinta from sapprd.zcomhead_tableta q where q.mandt = '900' and q.nrcmdsap = s.vgbel and rownum = 1) com_old " +
                                  " from sapprd.likp h, sapprd.lips s, sapprd.vttp p, sapprd.vttk k, clienti c, articole art, sapprd.vtpa v, sapprd.VEKP e, soferi sf " +
                                  " where h.mandt = '900'  and s.lgort not in " + exceptieDepozite + conditieDepozite + " and c.cod = h.kunnr  and h.mandt = s.mandt  and h.vbeln = s.vbeln " +
                                  " and art.cod = s.matnr and h.wadat between :dataStart and :dataStop and art.spart =:depart " +
                                    condTipDocument + " and nvl(k.datbg, '00000000') = '00000000' and s.lfimg > 0 " +
                                  " and h.mandt = p.mandt(+) and substr(s.matnr,11,1) != '3' " +
                                  " and h.vbeln = p.vbeln(+) and h.lfart not in ('EL', 'ZUL', 'ZLR') and p.mandt = k.mandt(+) " +
                                  " and decode( substr(s.werks, 3, 1),4,(CASE WHEN art.spart = '02' then 'FALSE' else 'TRUE' end),'TRUE') = 'TRUE' " +
                                  " and ( ( h.traty = 'TRAP' and nvl(k.dalen,'00000000') = '00000000') or h.traty = 'TCLI' or h.traty = 'TERT' ) " +
                                  " and p.tknum = k.tknum(+) " +
                                  " and p.mandt = v.mandt(+) and p.tknum = v.vbeln(+) and 'ZF' = v.parvw(+) and p.mandt = e.mandt(+) " +
                                  " and p.tknum = e.vpobjkey(+)  and sf.cod (+) = v.pernr " +
                                  " and not exists " +
                                  " (select tp.ul_stoc from sapprd.vbfa a, sapprd.zcomhead_tableta t, sapprd.zcomdet_tableta tp " +
                                  " where a.mandt = h.mandt and a.vbeln = h.vbeln and a.posnn = s.posnr and a.vbtyp_n = 'J' and a.vbtyp_v = 'C' " +
                                  " and a.mandt = t.mandt and a.vbelv = t.nrcmdsap and t.mandt = tp.mandt and t.id = tp.id and ( tp.ul_stoc = 'BV90' or tp.depoz = 'DESC') and rownum = 1) " +
                                  " ) x, sapprd.zdeliv_deleted d  where nvl(datbg, '00000000') = '00000000' " +
                                  " and d.mandt(+)='900' and d.comanda(+) = com_old and d.matnr(+) = x.matnr" +
                                  " and x.lfimg > 0 order by x.vbeln, x.kunnr, x.posnr ";



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
                Document unDocument = new Document();

                string nrDocument = "";
                bool isDocOpen = false;

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {

                        if (nrDocument != oReader.GetString(1))
                        {

                            getArticoleSterse(connection, listaDocumente, unDocument);

                            //nrDocument = oReader.GetString(1);
                            //isDocOpen = isDocumentOpen(connection, oReader.GetString(1), oReader.GetString(7));
                        }


                        isDocOpen = true;

                        if (isDocOpen)
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
                            unDocument.depozit = oReader.GetString(12);

                            string numeClient = getNumeClient(connection, unDocument.id);
                            if (!unDocument.client.Equals(numeClient))
                                unDocument.client = unDocument.client + " " + numeClient;

                            unDocument.nrMasina = oReader.GetString(13);
                            unDocument.numeSofer = oReader.GetString(14);

                            unDocument.modificare = "";
                            unDocument.infoStatus = "";
                            unDocument.cantitateModificata = unDocument.cantitate;



                            string comandaAnterioara = oReader.GetString(15);
                            unDocument.comandaVeche = oReader.GetString(15);

                            if (comandaAnterioara != "-1")
                            {
                                string[] statusArticol = getArticoleModificate(connection, comandaAnterioara, unDocument.codArticol, Double.Parse(unDocument.cantitate)).Split('#');

                                if (!statusArticol[0].Equals("-1"))
                                {
                                    unDocument.modificare = statusArticol[1];
                                    unDocument.infoStatus = "Livrare modificata";

                                    if (unDocument.modificare.Contains("adaugat"))
                                    {
                                        unDocument.cantitateModificata = unDocument.cantitate;
                                        unDocument.cantitate = "0";
                                    }
                                    else if (unDocument.modificare.Contains("modificat"))
                                    {
                                        unDocument.cantitateModificata = unDocument.cantitate;
                                        unDocument.cantitate = statusArticol[0];
                                    }

                                }
                            }




                            listaDocumente.Add(unDocument);
                        }

                        nrDocument = oReader.GetString(1);
                    }

                    getArticoleSterse(connection, listaDocumente, unDocument);

                }

                listaDocumente.AddRange(getModificariComanda(filiala, departament));

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaDocumente);



            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + cmd.CommandText + " , " + departament + ", depoz: " + depozit);
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



        private string getArticoleModificate(OracleConnection connection, string idComanda, string codArticol, double cantitate)
        {
            string status = "#";
            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {

                string sqlString = " select 1 from sapprd.zdeliv_deleted where mandt = '900' and comanda = :idComanda ";

                cmd = connection.CreateCommand();
                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    sqlString = " select lfimg from sapprd.zdeliv_deleted where mandt = '900' and comanda = :idComanda and matnr = :codArticol ";

                    if (codArticol.Length == 8)
                        codArticol = "0000000000" + codArticol;

                    cmd = connection.CreateCommand();
                    cmd.CommandText = sqlString;

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":idComanda", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = idComanda;

                    cmd.Parameters.Add(":codArticol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codArticol;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();

                        if (oReader.GetDouble(0) != cantitate)
                        {
                            status = oReader.GetDouble(0).ToString() + "#Articol modificat";
                        }
                        else
                        {
                            status = "-1#-1";
                        }

                    }
                    else
                    {
                        status = "0#Articol adaugat";
                    }
                }
                else
                {
                    status = "-1#-1";
                }



            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }


            return status;

        }



        private string getArticoleSterse(OracleConnection connection, List<Document> listDocumente, Document documentNou)
        {

            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {

                string sqlString = " select matnr, maktx, lfimg, vrkme, lgort from sapprd.zdeliv_deleted where mandt = '900' and comanda = :idComanda ";

                cmd = connection.CreateCommand();
                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = documentNou.comandaVeche;

                oReader = cmd.ExecuteReader();
                bool artGasit = false;
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        artGasit = false;

                        foreach (Document document in listDocumente)
                        {
                            if (document.id.Equals(documentNou.id))
                            {
                                string codArticol = document.codArticol;
                                if (codArticol.Length == 8)
                                    codArticol = "0000000000" + codArticol;

                                if (oReader.GetString(0).Equals(codArticol))
                                    artGasit = true;




                            }


                        }


                        if (!artGasit)
                        {
                            Document docNou = new Document();
                            docNou.client = documentNou.client;
                            docNou.id = documentNou.id;
                            docNou.emitere = documentNou.emitere;
                            docNou.isPregatit = documentNou.isPregatit;
                            docNou.isTiparit = documentNou.isTiparit;
                            docNou.codArticol = oReader.GetString(0).TrimStart('0');
                            docNou.cantitate = oReader.GetDouble(2).ToString();
                            docNou.um = oReader.GetString(3);
                            docNou.numeArticol = oReader.GetString(1);
                            docNou.pozitieArticol = ((listDocumente.Count() + 1) * 10).ToString();
                            docNou.depozit = oReader.GetString(4);
                            docNou.modificare = "Articol sters";
                            docNou.tip = "D";
                            docNou.nrMasina = " ";
                            docNou.numeSofer = " ";
                            docNou.cantitateModificata = "0";
                            docNou.infoStatus = "Livrare modificata";
                            docNou.tipTransport = documentNou.tipTransport;
                            listDocumente.Add(docNou);
                        }

                    }





                }
            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return "!";
        }


        [WebMethod]
        public string setMarfaPregatita(string nrDocument, string gestionar, string isMarfaPreg, string isDocAnulat)
        {
            string retVal = "0", query = "";


            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

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

                cmd = connection.CreateCommand();

                if (isDocAnulat == null)
                    isDocAnulat = "false";

                if (Boolean.Parse(isDocAnulat))
                {
                    if (Boolean.Parse(isMarfaPreg))
                        query = query = " insert into sapprd.zpregmarfagest(mandt, codgestionar, document, datac, orac, doc_anulat) " +
                                        " values ('900', :codGest, :document, :datac, :orac, 'X') ";
                    else
                        query = " delete from sapprd.zpregmarfagest where document=:document and doc_anulat = 'X' ";

                }
                else
                {
                    if (Boolean.Parse(isMarfaPreg))
                    {
                        query = " insert into sapprd.zpregmarfagest(mandt, codgestionar, document, datac, orac) " +
                                " values ('900', :codGest, :document, :datac, :orac) ";
                    }
                    else
                    {
                        query = " delete from sapprd.zpregmarfagest where document=:document ";
                    }
                }


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":document", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                if (Boolean.Parse(isMarfaPreg) || (Boolean.Parse(isDocAnulat) && Boolean.Parse(isMarfaPreg)))
                {
                    cmd.Parameters.Add(":codGest", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = gestionar;
                }

                if (Boolean.Parse(isMarfaPreg) || (Boolean.Parse(isDocAnulat) && Boolean.Parse(isMarfaPreg)))
                {
                    cmd.Parameters.Add(":datac", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = nowDate;

                    cmd.Parameters.Add(":orac", OracleType.NVarChar, 18).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = nowTime;
                }

                cmd.ExecuteNonQuery();

            }

            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + " doc = " + nrDocument + " gest = " + gestionar);
            }
            finally
            {

                if (cmd != null)
                    cmd.Dispose();

                connection.Close();
                connection.Dispose();
            }




            return retVal;
        }




        
        private string parseDate(string strDate)
        {
            DateTime myDate = DateTime.ParseExact(strDate, "yyyy-MM-dd h:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
            return myDate.ToString("dd-MMM-yyyy HH:mm");
        }

    }
}