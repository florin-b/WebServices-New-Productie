using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using System.Data.OracleClient;
using System.Data.Common;
using System.Net.Mail;
using System.Xml;
using System.IO;
using System.Globalization;
using WebService1.SAPWebServices;
using WebService1.SapWsClp;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Net;
using WebService1.General;
using System.Diagnostics;

namespace WebService1
{

    [WebService(Namespace = "http://SmartScan.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]

    public class Service1 : System.Web.Services.WebService
    {

        static string[]  agentiExtra02 = { "83113", "59365", "74182", "74284", "74261", "74055", "74058", "74031", "74347", "74392", "74410", "74426", "74449", "74456" };

        public static string[] agentiExtra03 = { "83309", "83409", "83444", "83297", "83431", "83356", "83051", "83407", "83309", "83409" };
        public static string[] agentiExtra06 = { "83436", "83090", "83409", "83444", "83311", "83436", "83435", "83431", "83356", "83051", "83090", "83409", "83311" };
        public static string[] agentiExtra07 = { "83436", "83090", "83311", "83436", "83297", "83435", "83309", "83090", "83311" };
        public static string[] agentiExtra08 = { "83402" };

        string globalParrentId = "0";

        Random randNum = new Random();

        [WebMethod]
        public string HelloWorld()
        {
            DateTime cDate = DateTime.Now;

            string year = cDate.Year.ToString();
            string month = cDate.Month.ToString("00");
            string day = cDate.Day.ToString("00");

            string nowDate = year + month + day;

            return "Hello World World Cup ! " + nowDate;
        }




        [WebMethod]
        public string retVal(string param1, string param2)
        {
            return param1 + param2;
        }

        [WebMethod]
        public string checkCNP(string cnp)
        {
            return Utils.checkROCnpClient(cnp);
        }

        [WebMethod]
        public string getArtUmVanz(string codArt)
        {
            string retVal = "", umVanz = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = GetConnectionString_android();
                OracleCommand cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd.CommandText = " select distinct vrkme from sapprd.mvke where matnr =:codArt and vkorg = '1000' and vtweg = '10' ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArt;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    umVanz = oReader.GetString(0);
                }

                if (umVanz.Trim() == "")
                {
                    cmd.CommandText = " select meins from sapprd.mara where matnr =:codArt ";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codArt;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        umVanz = oReader.GetString(0);
                    }


                }


                retVal = umVanz;

                oReader.Close();
                oReader.Dispose();
                cmd.Dispose();

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
        public string getClientiSemiactivi(string codAgent, string codDepart, string filiala)
        {

            OperatiiSemiactivi operatiiSemiactivi = new OperatiiSemiactivi();
            return operatiiSemiactivi.getClientiSemiactivi(codAgent, codDepart, filiala);

        }

        [WebMethod]
        public string getIstoricClientSemiactiv(string codClient)
        {

            OperatiiSemiactivi operatiiSemiactivi = new OperatiiSemiactivi();
            return operatiiSemiactivi.getIstoricVanzari(codClient);

        }

        [WebMethod]
        public string getCostMacara(string unitLog, string codAgent, string codClient, string codFurnizor, string listArt)
        {
            return new OperatiiMacara().getCostMacara(unitLog, codAgent, codClient, codFurnizor, listArt, "");
        }

        [WebMethod]
        public string getCostMacaraComenzi(string codAgent, string codClient, string codFurnizor, string listComenzi, string canal)
        {
            return new OperatiiMacara().getCostMacaraComenzi(codAgent, codClient, codFurnizor, listComenzi, canal);
        }


        [WebMethod]
        public String salarizareAV(string codAgent, string departament, string filiala, string dataRap)
        {
            Salarizare salarizare = new Salarizare();
            return salarizare.getSalarizareAV(codAgent, departament, filiala, dataRap);

        }

        [WebMethod]
        public string getNTCF(string codAgent)
        {
            return new Salarizare().getDateNTCF(codAgent);
        }


        [WebMethod]
        public string getSalarizareAgent(string codAgent, string ul, string divizie, string an, string luna)
        {
            return new SalarizareAgenti2019().getSalarizareAgent(codAgent, ul, divizie, an, luna);
        }

        [WebMethod]
        public string getSalarizareSd(string codAgent, string ul, string divizie, string an, string luna)
        {
            return new SalarizareAgenti2019().getSalarizareSD(codAgent, ul, divizie, an, luna);
        }

        [WebMethod]
        public string getSalarizareDepartament(string ul, string divizie, string an, string luna)
        {
            return new SalarizareAgenti2019().getSalarizareDepartament(ul, divizie, an, luna);
        }

        [WebMethod]
        public string getSalarizareKA(string codAgent, string ul, string an, string luna)
        {
            return new SalarizareAgenti2019().getSalarizareKA(codAgent, ul, an, luna);
        }

        [WebMethod]
        public string getSalarizareSDKA(string codAgent, string ul, string an, string luna)
        {
            return new SalarizareAgenti2019().getSalarizareSDKA(codAgent, ul, an, luna);
        }

        [WebMethod]
        public string getSalarizareDepartamentKA(string ul, string an, string luna)
        {
            return new SalarizareAgenti2019().getSalarizareDepartamentKA(ul, an, luna);
        }


        [WebMethod]
        public string getArtFactConvUM(string codArt, string unitMas)
        {
            string retVal = "1#1";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = GetConnectionString_android();
                OracleCommand cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd.CommandText = " select umrez, umren from sapprd.marm where mandt = '900' and matnr =:codArt and meinh=:unitMas ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArt;

                cmd.Parameters.Add(":unitMas", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = unitMas;


                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    retVal = oReader.GetInt32(0).ToString() + "#" + oReader.GetInt32(1);
                }



                oReader.Close();
                oReader.Dispose();
                cmd.Dispose();

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
        public string getValTransportConsilieri(string oras, string codClient, string valCmd, string codJudet, string unitLog, string articole)
        {
            string retVal = "";

            return retVal;
        }



        [WebMethod]
        public string getValTransportComandaSite(string oras, string codClient, string listArticole, string codJudet, string unitLog, string tipPlata)
        {
            string retVal = "";

            SAPWebServices.ZTBL_WEBSERVICE ws = new ZTBL_WEBSERVICE();

            try
            {
                string[] articoleData = listArticole.Split('?');
                string[] arrayArticole = articoleData[0].Split('#');
                string[] arrayUm = articoleData[1].Split('#');
                string[] arrayCant = articoleData[2].Split('#');
                string[] arrayVal = articoleData[3].Split('#');

                int artCount = arrayArticole.Length - 1;

                string localFilGed = unitLog.Substring(0, 2) + "2" + unitLog.Substring(3, 1);  //cu 20

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                ws.Credentials = nc;
                ws.Timeout = 300000;

                SAPWebServices.ZcalcTrapSite inParam = new ZcalcTrapSite();

                SAPWebServices.ZtrapSite[] articole = new SAPWebServices.ZtrapSite[artCount];

                for (int i = 0; i < artCount; i++)
                {
                    articole[i] = new SAPWebServices.ZtrapSite();
                    articole[i].Matnr = arrayArticole[i];
                    articole[i].Meins = arrayUm[i];
                    articole[i].Menge = Convert.ToDecimal(arrayCant[i]);
                    articole[i].Netwr = Convert.ToDecimal(arrayVal[i]);
                }

                inParam.ItMatnr = articole;
                inParam.GvKunnr = codClient;
                inParam.GvOras = oras;
                inParam.GvRegio = codJudet;
                inParam.GvTipplata = tipPlata;
                inParam.GvWerks = localFilGed;

                SAPWebServices.ZcalcTrapSiteResponse resp = new ZcalcTrapSiteResponse();
                resp = ws.ZcalcTrapSite(inParam);

                retVal = resp.ValTrap.ToString() + "#" + resp.GvTraty.ToString();



            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                ws.Dispose();
            }

            return retVal;
        }



        [WebMethod]
        public string getClientInfo(string codClient, string filiala, string depart, string tipInfo)
        {
            string retVal = " ";
            string connectionString = "";
            OracleConnection connection = null;

            try
            {

                OracleDataReader oReader = null;
                OracleCommand cmd = null;

                switch (tipInfo)
                {

                    case "0":   //vanzari art. B 30

                        //pondere articole b pentru ultimele 30 de zile

                        connection = new OracleConnection();

                        oReader = null;
                        connectionString = GetConnectionString_android();

                        connection.ConnectionString = connectionString;
                        connection.Open();

                        cmd = connection.CreateCommand();

                        cmd.CommandText = " select nvl(round(sum(nvl(g.tip_b,0)) * 100 / sum(nvl(g.netwr,0) - nvl(g.cre_m,0) + nvl(g.deb_m,0) - nvl(g.retur,0)),2),0)  " +
                                          " from sapprd.ZVANZARI_AG g where g.mandt = '900' and g.fkdat >=:dataSel and g.kunag =:codClient and g.spart =:depart " +
                                          " and g.fkart in ('ZFRA','ZFRB','ZFI','ZFS','ZFSC','ZFPA','ZFE','ZFDC','ZFM','ZFMC') ";


                        cmd.Parameters.Clear();

                        DateTime localDate = DateTime.Now.AddDays(-30);
                        string selectedDate = localDate.Year.ToString() + localDate.Month.ToString("00") + localDate.Day.ToString("00");


                        cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = codClient;

                        cmd.Parameters.Add(":dataSel", OracleType.VarChar, 10).Direction = ParameterDirection.Input;
                        cmd.Parameters[1].Value = selectedDate;

                        cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                        cmd.Parameters[2].Value = depart;


                        oReader = cmd.ExecuteReader();
                        string pondereB_30 = "0";
                        if (oReader.HasRows)
                        {
                            oReader.Read();
                            pondereB_30 = String.Format("{0:0.00}", oReader.GetDouble(0));
                        }
                        else
                        {
                            pondereB_30 = "0";
                        }

                        if (oReader != null)
                        {
                            oReader.Close();
                            oReader.Dispose();
                        }

                        retVal = pondereB_30;
                        break;


                    case "1":

                        //adrese de livrare
                        retVal = getAdreseLivrareClient(codClient, "");

                        //peroana de contact
                        connection = new OracleConnection();
                        oReader = null;
                        connectionString = GetConnectionString_android();

                        connection.ConnectionString = connectionString;
                        connection.Open();

                        cmd = connection.CreateCommand();

                        string condDepartPersCont = "";
                        if (!depart.Equals("00"))
                        {
                            condDepartPersCont = " and spart = '" + depart + "' ";
                        }

                        cmd = connection.CreateCommand();
                        cmd.CommandText = " select namev, name1, telf1 from sapprd.knvk u where u.mandt = '900' and " +
                            " (u.parnr, u.kunnr) in (select distinct p.parnr, p.kunnr from sapprd.knvp p where p.mandt = '900' " +
                            " and p.kunnr =:k and parvw = 'AP' and spart = '" + depart + "' and vtweg = '10'  )";

                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = codClient;

                        string nume = " ", tel = " ";

                        oReader = cmd.ExecuteReader();
                        if (oReader.HasRows)
                        {
                            oReader.Read();
                            nume = oReader.GetString(0) + " " + oReader.GetString(1);
                            tel = oReader.GetString(2);


                        }

                        if (oReader != null)
                        {
                            oReader.Close();
                            oReader.Dispose();
                        }

                        //sf. pers.cont.

                        retVal += nume + "#" + tel + "@@";


                        break;

                    case "2":   //credit + stare
                        DateTime cDate = DateTime.Now;
                        string year = cDate.Year.ToString();
                        string month = cDate.Month.ToString("00");
                        string day = cDate.Day.ToString("00");
                        string nowDate = year + month;
                        string clBlocat = " ";

                        connection = new OracleConnection();

                        oReader = null;
                        connectionString = GetConnectionString_android();

                        connection.ConnectionString = connectionString;
                        connection.Open();


                        cmd = connection.CreateCommand();
                        cmd.CommandText = " select distinct k.kunnr,k.klimk lcredit, " +
                            " k.skfor,k.ssobl, nvl((select s2.olikw+s2.ofakw from sapprd.s067 s2 where s2.mandt='900' and " +
                            " s2.kkber='1000' and s2.knkli=k.kunnr),0) olikw, nvl((select sum(s1.oeikw) from sapprd.s066 s1 " +
                            " where s1.mandt='900' and s1.kkber='1000' and spmon=:l  and s1.knkli=k.kunnr),0) oeikw,  k.crblb  from sapprd.knkk k " +
                            " where k.mandt='900' and k.kkber='1000' and k.kunnr=:k ";



                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":l", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = nowDate;

                        cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[1].Value = codClient;

                        oReader = cmd.ExecuteReader();

                        float limCredit = 0;
                        float restCredit = 0;

                        if (oReader.HasRows)
                        {
                            oReader.Read();
                            limCredit = oReader.GetFloat(1);
                            restCredit = oReader.GetFloat(1) - (oReader.GetFloat(2) + oReader.GetFloat(3)) - (oReader.GetFloat(4) + oReader.GetFloat(5));
                            clBlocat = oReader.GetString(6);

                        }
                        else
                        {
                            limCredit = 0;
                            restCredit = 0;
                        }

                        if (oReader != null)
                        {
                            oReader.Close();
                            oReader.Dispose();
                        }

                        retVal = limCredit.ToString() + "#" + restCredit.ToString();

                        break;



                }


                cmd.Dispose();

            }
            catch (Exception ex)
            {
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
        public string getAdreseGpsClient(string codAgent, string codClient)
        {
            string serializedResult = " ";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                OracleCommand cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd.CommandText = " select coordonategps, data, ora, tipadresa, codagent, codclient from sapprd.zadresagpsclient where codclient=:codClient order by data, ora";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                List<AdresaGpsClient> listaAdreseGps = new List<AdresaGpsClient>();
                AdresaGpsClient oAdresaGps = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        oAdresaGps = new AdresaGpsClient();

                        oAdresaGps.adresa = GetLocationAddress(oReader.GetString(0));
                        oAdresaGps.data = oReader.GetString(1);
                        oAdresaGps.ora = oReader.GetString(2);
                        oAdresaGps.tipLocatie = oReader.GetString(3);
                        oAdresaGps.codAgent = oReader.GetString(4);
                        oAdresaGps.codClient = oReader.GetString(5);
                        oAdresaGps.dateGps = oReader.GetString(0);

                        listaAdreseGps.Add(oAdresaGps);
                    }

                }

                oReader.Close();
                oReader.Dispose();
                cmd.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaAdreseGps);

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }



            return serializedResult;
        }


        [WebMethod]
        public string deleteAdresaGpsClient(string adresa)
        {

            string retVal = "0", query = "";

            var serializer = new JavaScriptSerializer();
            AdresaGpsClient adreseClient = serializer.Deserialize<AdresaGpsClient>(adresa);

            OracleConnection connection = new OracleConnection();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                OracleCommand cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                query = " delete from sapprd.zadresagpsclient where codclient =:codclient  and data =:data and ora =:ora ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codclient", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = adreseClient.codClient;

                cmd.Parameters.Add(":data", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = adreseClient.data;

                cmd.Parameters.Add(":ora", OracleType.NVarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = adreseClient.ora;

                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return retVal;
        }



        [WebMethod]
        public string getListaClienti(string codAgent, string numeClient)
        {

            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                OracleCommand cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd.CommandText = " select distinct p.kunnr, cl.nume  from sapprd.knvp p , agenti ag, clienti cl, clie_tip tp where p.mandt = '900' " +
                                  " and tp.cod_cli = p.kunnr and tp.canal = p.vtweg and tp.depart = p.spart " +
                                  " and p.parvw in ('VE', 'ZC') and p.vtweg = '10' and tp.tip in ('02','03','04','05','06') " +
                                  " and p.pernr =:codAgent and lower(cl.nume) like '" + numeClient.ToLower() + "%' and p.pernr = ag.cod  and cl.cod = p.kunnr  order by nume ";


                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgent;

                oReader = cmd.ExecuteReader();

                List<Client> listaAdreseGps = new List<Client>();
                Client oAdresaGps = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        oAdresaGps = new Client();
                        oAdresaGps.codClient = oReader.GetString(0);
                        oAdresaGps.numeClient = oReader.GetString(1);
                        oAdresaGps.tipClient = " ";
                        oAdresaGps.agenti = " ";
                        
                        listaAdreseGps.Add(oAdresaGps);
                    }

                }

                oReader.Close();
                oReader.Dispose();
                cmd.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaAdreseGps);

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return serializedResult;


        }



        [WebMethod]
        public string getListaAdreseGps(string codAgent, string tipAdresa, string filiala, string departament)
        {



            string serializedResult = "";
            string conditie = "";
            string tipAgent = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            //adrese necompletate
            if (tipAdresa.Equals("1"))
            {
                conditie = " and nvl(ad.codclient,'-1') = '-1' ";
            }

            //adrese completate
            if (tipAdresa.Equals("2"))
            {
                conditie = " and nvl(ad.codclient,'-1') != '-1' ";
            }

            if (codAgent.Equals("00000000"))
            {
                tipAgent = " and ag.filiala = '" + filiala + "' and divizie = '" + departament + "' ";
            }
            else
            {
                tipAgent = " and p.pernr =:codAgent ";
            }


            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                OracleCommand cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();


                cmd.CommandText = " select distinct (select nume from clienti where cod = p.kunnr) nume, p.kunnr, ag.nume nume_ag  " +
                                  " from sapprd.knvp p, agenti ag, sapprd.zadresagpsclient ad, clie_tip tp  where p.mandt = '900' " +
                                  " and p.parvw in ('VE', 'ZC') and p.vtweg = '10'  " +
                                  " and tp.cod_cli = p.kunnr and tp.canal = p.vtweg and tp.depart = p.spart " +
                                  " and tp.tip in ('02','03','04','05','06') " + tipAgent +
                                  " and p.pernr = ag.cod and ad.codclient(+) = p.kunnr " + conditie +
                                  " order by  nume_ag, nume ";



                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                if (!codAgent.Equals("00000000"))
                {
                    cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codAgent;
                }

                oReader = cmd.ExecuteReader();

                List<Clienti> listaAdreseGps = new List<Clienti>();
                Clienti oAdresaGps = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        oAdresaGps = new Clienti();
                        oAdresaGps.numeClient = oReader.GetString(0);
                        oAdresaGps.codClient = oReader.GetString(1);
                        oAdresaGps.tipClient = " ";

                        if (codAgent.Equals("00000000"))
                        {
                            oAdresaGps.tipClient = oReader.GetString(2);
                        }


                        listaAdreseGps.Add(oAdresaGps);
                    }

                }

                oReader.Close();
                oReader.Dispose();
                cmd.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaAdreseGps);



            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return serializedResult;

        }


        private string GetLocationAddress(string coordonate)
        {
            string currentAddress = "";
            HttpWebRequest request = default(HttpWebRequest);
            HttpWebResponse response = null;
            StreamReader reader = default(StreamReader);
            string json = null;

            try
            {
                string[] arrayCoord = coordonate.Split(',');
                string lat = arrayCoord[0];
                string lng = arrayCoord[1];

                request = (HttpWebRequest)WebRequest.Create("http://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + ", " + lng + "&sensor=false");

                response = (HttpWebResponse)request.GetResponse();

                reader = new StreamReader(response.GetResponseStream());
                json = reader.ReadToEnd();
                response.Close();

                if (json.Contains("ZERO_RESULTS"))
                {
                    currentAddress = "Adresa indisponibila";
                };
                if (json.Contains("formatted_address"))
                {

                    int start = json.IndexOf("formatted_address");
                    int end = json.IndexOf(", Romania");
                    string AddStart = json.Substring(start + 21);
                    string EndStart = json.Substring(end);
                    string FinalAddress = AddStart.Replace(EndStart, "");

                    currentAddress = FinalAddress.Replace('"', ' ');



                };
            }
            catch (Exception ex)
            {
                string Message = "Error: " + ex.ToString();
                currentAddress = "Adresa indisponibila";
            }
            finally
            {
                if ((response != null))
                    response.Close();
            }

            return currentAddress;

        }


        [WebMethod]
        public string setAdresaGpsClient(string adresa)
        {


            string retVal = "0", query = "";

            var serializer = new JavaScriptSerializer();
            AdresaGpsClient adreseClient = serializer.Deserialize<AdresaGpsClient>(adresa);

            OracleConnection connection = new OracleConnection();

            try
            {
                string connectionString = GetConnectionString_android_prd();
                OracleCommand cmd = connection.CreateCommand();

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


                query = " insert into sapprd.zadresagpsclient (mandt, codclient, codagent, coordonategps, data, ora, tipadresa) " +
                        " values ('900', :codclient, :codagent, :dategps, :data, :ora, :tipadresa  ) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codclient", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = adreseClient.codClient;

                cmd.Parameters.Add(":codagent", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = adreseClient.codAgent;

                cmd.Parameters.Add(":dategps", OracleType.NVarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = adreseClient.dateGps;

                cmd.Parameters.Add(":data", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = nowDate;

                cmd.Parameters.Add(":ora", OracleType.NVarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = nowTime;

                cmd.Parameters.Add(":tipadresa", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = adreseClient.tipLocatie;

                cmd.ExecuteNonQuery();




            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return retVal;
        }




        [WebMethod]
        public string getApprovalList(string parentId)
        {
            //lista aprobari comenzi KA
            string retVal = "-1";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                cmd.CommandText = " select a.depart, nvl(decode(a.accept1,'X',(select distinct nume||' (tel.'|| nrtel ||')' from agenti where  divizie = a.depart and filiala = a.ul " +
                                  " and activ = 1 and tip='SD'),'-1' ),-1) SD, ora_accept1, nvl(decode(a.accept2,'X',(select ename from sapprd.zfil_dv " +
                                  " where spart = a.depart and  prctr = a.ul and length(ename) > 1 ),'-1' ),-1)  DV, ora_accept2 from sapprd.zcomhead_tableta a where " +
                                  " (accept1 = 'X' or accept2 = 'X') and parent_id =:parentId order by depart ";



                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":parentId", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = parentId;

                oReader = cmd.ExecuteReader();


                if (oReader.HasRows)
                {
                    retVal = "";
                    while (oReader.Read())
                    {
                        retVal += oReader.GetString(0) + "#" + oReader.GetString(1) + "#" + oReader.GetString(2)
                               + "#" + oReader.GetString(3) + "#" + oReader.GetString(4) + "@@";

                    }

                }


                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();
            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }



            return retVal;
        }


        [WebMethod]
        public string getDocumenteRetur(string codClient, string codDepartament, string unitLog, string tipDocument, string interval, string tipUserSap)
        {
            OperatiiRetur opRetur = new OperatiiRetur();
            return opRetur.getDocumenteRetur(codClient, codDepartament, unitLog, tipDocument, interval, tipUserSap);
        }


        [WebMethod]
        public string getListDocumenteSalvate(string codAgent, string filiala, string tipUser, string depart, string interval, string stare)
        {
            OperatiiRetur opRetur = new OperatiiRetur();
            return opRetur.getListDocumenteSalvate(codAgent, filiala, tipUser, depart, interval, stare);
        }


        [WebMethod]
        public string getArticoleRetur(string nrDocument, string tipDocument)
        {
            OperatiiRetur opRetur = new OperatiiRetur();
            return opRetur.getArticoleRetur(nrDocument, tipDocument);
        }

        [WebMethod]
        public string saveComandaRetur(string dateRetur, string tipRetur)
        {
            OperatiiRetur opRetur = new OperatiiRetur();
            return opRetur.saveComandaRetur(dateRetur, tipRetur);
        }

        [WebMethod]
        public string getStocReturAvansat(string nrDocument, string codArticol, string um)
        {
            return new OperatiiRetur().getStocReturAvansat(nrDocument, codArticol, um);
        }


        [WebMethod]
        public string getListClientiCV(string numeClient, string unitLog, string tipCmd, string tipUserSap)
        {
            OperatiiRetur opRetur = new OperatiiRetur();
            return opRetur.getListClientiCV(numeClient, unitLog, tipCmd, tipUserSap);
        }


        [WebMethod]
        public string getArticoleDocumentSalvat(string idComanda)
        {
            OperatiiRetur opRetur = new OperatiiRetur();
            return opRetur.getArticoleDocumentSalvat(idComanda);
        }

        [WebMethod]
        public string opereazaComandaRetur(string idComanda, string tipOperatie)
        {
            OperatiiRetur opRetur = new OperatiiRetur();
            return opRetur.opereazaComanda(idComanda, tipOperatie);
        }

        [WebMethod]
        public string getArticoleDocumentReturSAP(string idComanda)
        {
            OperatiiRetur opRetur = new OperatiiRetur();
            return opRetur.getArticoleDocumentSalvat(idComanda);
        }

        [WebMethod]
        public string getListDocumenteSAP(string codAgent, string filiala, string tipUser, string depart, string interval, string stare)
        {
            OperatiiRetur opRetur = new OperatiiRetur();
            return opRetur.getListDocumenteSAP(codAgent, filiala, tipUser, depart, interval, stare);
        }


        [WebMethod]
        public string salveazaObiectiveCVA(string dateObiective, string beneficiari, string stadii)
        {
            OperatiiObiectiveCVA operatiiObiective = new OperatiiObiectiveCVA();
            //return operatiiObiective.adaugaObiectiv(dateObiective, beneficiari, stadii);

            return "0";

        }


        [WebMethod]
        public string getListObiectiveCVA(string tipUser, string codUser, string filiala)
        {
            OperatiiObiectiveCVA operatiiObiective = new OperatiiObiectiveCVA();
            return operatiiObiective.getObiective(tipUser, codUser, filiala);
        }

        [WebMethod]
        public bool schimbaAgentObiecticeCVA(string codAgentVechi, string codAgentNou)
        {
            return new OperatiiObiectiveCVA().schimbaAgentObiecticeCVA(codAgentVechi, codAgentNou);
        }


        [WebMethod]
        public string getObiectiveConsilieri(string codConsilier, string numeObiectiv)
        {
            return new ObiectiveConsilieri().getListObiective(codConsilier, numeObiectiv);
        }


        [WebMethod]
        public string getMeseriasi1(string codFiliala)
        {
            return new OperatiiClienti().getMeseriasi(codFiliala);
        }

        [WebMethod]
        public string getVanzariPierduteAv(string codAgent, string ul, string codDepart)
        {
            return new PierderiVanzari().getVanzariAv(codAgent, ul, codDepart);
        }

        [WebMethod]
        public string getVanzariPierduteDepart(string ul, string codDepart)
        {
            return new PierderiVanzari().getVanzariDepartament(ul, codDepart);
        }

        [WebMethod]
        public string getVanzariPierduteTotal(string codDepart)
        {
            return new PierderiVanzari().getVanzariTotal(codDepart);
        }

        [WebMethod]
        public string getAdresaFiliala(string filiala)
        {
            return new Utils().getAdresaFiliala(filiala);
        }

        [WebMethod]
        public string getMeseriasi(string codFiliala, string tipUser, string codUser, string codDepart)
        {
            if (tipUser != null)
                return new OperatiiClienti().getMeseriasi(codFiliala, tipUser, codUser, codDepart);
            else
                return new OperatiiClienti().getMeseriasi(codFiliala);
        }

        [WebMethod]
        public string getCmdDateLivrare(string idCmd)
        {
            string retVal = "-1";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                cmd.CommandText = " select cantar, tip_plata, pers_contact, telefon, adr_livrare, mt, fact_red, city, region, decode(trim(pmnttrms),'',' ',pmnttrms) , " +
                                                " decode(trim(obstra),'',' ',obstra), ketdat," +
                                                " docin, decode(trim(adr_noua),'',' ',adr_noua), decode(trim(obsplata),'',' ',obsplata), decode(trim(addrnumber),'',' ',addrnumber), " +
                                                " val_incasata, decode(trim(mod_av),'',' ',mod_av), " +
                                                " nvl((select p.prelucrare from sapprd.zprelucrare04 p where p.idcomanda = id ),'-1') prelucrare, " +
                                                " nvl((select t.greutate from sapprd.ztonajcomanda t where t.idcomanda = id ),'-1') tonaj, nvl(client_raft,' ') " +
                                                " from sapprd.zcomhead_tableta where id = :idCmd ";



                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idCmd", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idCmd;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    retVal = oReader.GetString(0) + "#" + oReader.GetString(1) + "#" + Regex.Replace(oReader.GetString(2), @"[!]|[#]|[@@]|[,]", " ") + "#"
                              + Regex.Replace(oReader.GetString(3), @"[!]|[#]|[@@]|[,]", " ") + "#" + Regex.Replace(oReader.GetString(4), @"[!]|[#]|[@@]|[,]", " ") + "#"
                              + oReader.GetString(5) + "#" + oReader.GetString(6) + "#" + Regex.Replace(oReader.GetString(7), @"[!]|[#]|[@@]|[,]", " ") + "#"
                              + oReader.GetString(8) + "#" + oReader.GetString(9) + "#" + Regex.Replace(oReader.GetString(10), @"[!]|[#]|[@@]|[,]", " ").Trim() + "#"
                              + oReader.GetString(11) + "#" + oReader.GetString(12) + "#" + oReader.GetString(13) + "#"
                              + Regex.Replace(oReader.GetString(14), @"[!]|[#]|[@@]|[,]", " ") + "#" + oReader.GetString(15) + "#" + oReader.GetDecimal(16) + "#"
                              + oReader.GetString(17) + "#" + oReader.GetString(18) + "#" + oReader.GetDouble(19).ToString() + "#" + oReader.GetString(20).ToString() + "#";

                }

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();
            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return retVal;
        }






        [WebMethod]
        public string getArticoleComplementare(string listaArticoleComanda)
        {

            OperatiiArticole articole = new OperatiiArticole();
            return articole.getListArticoleComplementare(listaArticoleComanda);

        }

        [WebMethod]
        public string getArticoleStatistic(string codClient, string filiala, string departament)
        {
            OperatiiArticole articole = new OperatiiArticole();
            return articole.getListArticoleStatistic(codClient, filiala, departament);
        }


        [WebMethod]
        public string getListArticoleCustodie(string codClient, string filiala, string departament)
        {
            OperatiiArticole articole = new OperatiiArticole();
            return articole.getListArticoleCustodie(codClient, filiala, departament);
        }


        [WebMethod]
        public string getArticoleDistributie(string searchString, string tipArticol, string tipCautare, string filiala, string departament, string afisStoc, string codUser, string modulCautare)
        {

            OperatiiArticole articole = new OperatiiArticole();
            return articole.getListArticoleDistributie(searchString, tipArticol, tipCautare, filiala, departament, afisStoc, codUser,  modulCautare);

        }


        [WebMethod]
        public string getSinteticeDistributie(string searchString, string tipArticol, string tipCautare, string filiala, string departament)
        {

            OperatiiArticole articole = new OperatiiArticole();
            return articole.getListSinteticeDistributie(searchString, tipArticol, tipCautare, filiala, departament);

        }

        [WebMethod]
        public string getNivel1Distributie(string searchString, string tipArticol, string tipCautare, string filiala, string departament)
        {

            OperatiiArticole articole = new OperatiiArticole();
            return articole.getListNivel1Distributie(searchString, tipArticol, tipCautare, filiala, departament);

        }

        #region Preturi Concurenta

        [WebMethod]
        public string getListArticoleConcurenta(string searchString, string tipArticol, string tipCautare, string tipActualizare)
        {
            OperatiiConcurenta articole = new OperatiiConcurenta();
            return articole.getListArticoleConcurenta(searchString, tipArticol, tipCautare, tipActualizare);
        }

        [WebMethod]
        public string getListArticoleConcurentaBulk(string codConcurent, string tipActualizare, string data, string codAgent)
        {
            OperatiiConcurenta articole = new OperatiiConcurenta();
            return articole.getListArticoleConcurentaBulk(codConcurent, tipActualizare, data, codAgent);
        }

        [WebMethod]
        public string getCompaniiConcurente()
        {
            OperatiiConcurenta concurenta = new OperatiiConcurenta();
            return concurenta.getCompaniiConcurente();
        }

        [WebMethod]
        public string getPretConcurenta(String codArt, string concurent, string codAgent)
        {
            OperatiiConcurenta concurenta = new OperatiiConcurenta();
            return concurenta.getPretConcurenta(codArt, concurent, codAgent);
        }

        [WebMethod]
        public string addPretConcurenta(string codArt, string codAgent, string concurent, string valoare)
        {
            OperatiiConcurenta concurenta = new OperatiiConcurenta();
            return concurenta.addPretConcurenta(codArt, codAgent, concurent, valoare);
        }

        [WebMethod]
        public string saveListPreturi(string codAgent, string dataSalvare, string listPreturi)
        {
            OperatiiConcurenta concurenta = new OperatiiConcurenta();
            return concurenta.saveListPreturi(codAgent, dataSalvare, listPreturi);
        }

        #endregion

        [WebMethod]
        public string syncArtAndroid(string depart, string codDepart, string filiala)
        {
            //transport articole in tabela device
            string retVal = "";

            if (!codDepart.Equals("00") && !codDepart.Equals("11"))
            {

                OracleConnection connection = new OracleConnection();
                OracleDataReader oReader = null;
                string condDepart = "", condDepart1 = "";
                string codSinteticVar = "";
                try
                {


                    string connectionString = GetConnectionString_android();

                    connection.ConnectionString = connectionString;
                    connection.Open();


                    OracleCommand cmd = connection.CreateCommand();

                    condDepart = " and b.depart <> 'MAGA' ";
                    if (depart != "TOAT")
                    {
                        condDepart = "  and b.depart =:depart ";
                        condDepart1 = " and spart =:codDepart ";
                    }



                    cmd.CommandText = " select decode(length(a.cod),18,substr(a.cod,-8),a.cod) codart,a.nume, a.sintetic, b.cod_nivel1, a.umvanz10, a.umvanz, nvl(a.tip_mat,' ') from articole a, " +
                                    " sintetice b where a.sintetic = b.cod and a.cod != 'MAT GENERIC PROD'  and substr(a.cod,11,1) != '3' " + condDepart +
                                    " union " +
                                    " select decode(length(a.cod),18,substr(a.cod,-8),a.cod) codart,a.nume, a.sintetic, b.cod_nivel1, a.umvanz10, a.umvanz, nvl(a.tip_mat,' ') from articole a, " +
                                    " sintetice b where a.sintetic = b.cod and a.cod != 'MAT GENERIC PROD' and length(a.cod) <= 8 " + condDepart +
                                    " union " +
                                    " select decode(length(a.cod),18,substr(a.cod,-8),a.cod) codart,a.nume, a.sintetic, '100', a.umvanz10, a.umvanz, nvl(a.tip_mat,' ') from articole a " +
                                    " where a.sintetic like '%SERVT%' ";






                    cmd.CommandType = CommandType.Text;

                    if (depart != "TOAT")
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(":depart", OracleType.VarChar, 4).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = depart;
                    }


                    oReader = cmd.ExecuteReader();
                    int nr = 0;

                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            retVal += oReader.GetString(0) + "#" + Regex.Replace(oReader.GetString(1), @"[!]|[#]|[@@]|[,]", " ") + "#" + oReader.GetString(2)
                                   + "#" + oReader.GetString(3) + "#" + oReader.GetString(4) + "#" + oReader.GetString(5) + "#" + oReader.GetString(6) + "@@";
                            nr++;
                        }

                    }



                    //grupe nivel1

                    retVal += "!";

                    cmd.CommandText = " select distinct b.cod_nivel1, b.nume_nivel1, b.cod from sintetice b where 1 = 1 " + condDepart;

                    cmd.CommandType = CommandType.Text;

                    if (depart != "TOAT")
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(":depart", OracleType.VarChar, 4).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = depart;
                    }


                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            retVal += oReader.GetString(0) + "#" + Regex.Replace(oReader.GetString(1), @"[!]|[#]|[@@]|[,]", " ") + "#" +
                                      oReader.GetString(2) + "@@";
                        }

                    }



                    //sf. grupe nivel1


                    //articole complementare

                    retVal += "!";  // formatul este : codDepart#sintetic#sintetic_compl#nivel1#nivel1_compl@@

                    cmd.CommandText = " select spart, niv_princ, matkl_princ, lista_mat_sec from sapprd.ZCOMPLEMENTAR where 1 = 1 " + condDepart1;

                    cmd.CommandType = CommandType.Text;

                    if (depart != "TOAT")
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(":codDepart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = codDepart;
                    }


                    oReader = cmd.ExecuteReader();
                    string artCompl = "";
                    string[] tokArt;
                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {

                            if (oReader.GetString(1).Trim().Length != 0) //nivel1
                            {
                                if (oReader.GetString(3).Contains(';'))
                                {
                                    tokArt = oReader.GetString(3).Split(';');

                                    for (int i = 0; i < tokArt.Length; i++)
                                    {
                                        artCompl += oReader.GetString(0) + "#-1#-1#" + oReader.GetString(1) + "#" + tokArt[i] + "@@";
                                    }
                                }
                                else
                                {
                                    artCompl += oReader.GetString(0) + "#-1#-1#" + oReader.GetString(1) + "#" + oReader.GetString(3) + "@@";
                                }

                            }
                            else //sintetic
                            {

                                if (oReader.GetString(3).Contains(';'))
                                {
                                    tokArt = oReader.GetString(3).Split(';');

                                    for (int i = 0; i < tokArt.Length; i++)
                                    {
                                        artCompl += oReader.GetString(0) + "#-1#-1#" + oReader.GetString(2) + "#" + tokArt[i] + "@@";
                                    }
                                }
                                else
                                {
                                    artCompl += oReader.GetString(0) + "#-1#-1#" + oReader.GetString(2) + "#" + oReader.GetString(3) + "@@";
                                }

                            }


                        }

                        retVal += artCompl;


                    }
                    else
                    {
                        artCompl = "-1#-1#-1#-1#-1@@";
                        retVal += artCompl;
                    }


                    //sf. articole complementare


                    //sintetice
                    retVal += "!";

                    cmd.CommandText = " select b.cod, b.nume, b.depart from sintetice b where 1=1 " + condDepart;
                    cmd.CommandType = CommandType.Text;

                    if (depart != "TOAT")
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(":depart", OracleType.VarChar, 4).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = depart;
                    }

                    oReader = cmd.ExecuteReader();
                    string codDepartSint = "";
                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            codSinteticVar = Regex.Replace(oReader.GetString(1), @"[!]|[#]|[@@]|[,]", " ");

                            codDepartSint = getDepCod(oReader.GetString(2));
                            retVal += oReader.GetString(0) + "#" + codSinteticVar.Replace('#', ' ') + "#" + codDepartSint + "@@";
                        }

                    }



                    //sf. sintetice

                    oReader.Close();
                    oReader.Dispose();

                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    sendErrorToMail(ex.ToString());
                    retVal = "-1";
                }
                finally
                {
                    connection.Close();
                    connection.Dispose();
                }


            }
            else //sincronizarea pentru KA si cons este offline
            {
                try
                {
                    //KA
                    if (codDepart.Equals("00"))
                    {
                        StreamReader streamReader = File.OpenText(Server.MapPath("~/").Substring(0, 1) + ":\\articole\\" + codDepart + ".txt");
                        retVal = streamReader.ReadToEnd();
                        streamReader.Close();
                        streamReader.Dispose();
                    }


                    //consilieri, pe filiala
                    if (codDepart.Equals("11"))
                    {
                        StreamReader streamReader = File.OpenText(Server.MapPath("~/").Substring(0, 1) + ":\\articole\\" + codDepart + "_" + filiala.Substring(0, 2) + ".txt");
                        retVal = streamReader.ReadToEnd();
                        streamReader.Close();
                        streamReader.Dispose();
                    }


                }
                catch (Exception ex)
                {
                    sendErrorToMail(ex.ToString());
                    retVal = "-1";
                }
            }



            return retVal;


        }



        [WebMethod]
        public string salveazaConditiiComanda(string conditiiComanda)
        {
            OperatiiConditiiComanda conditii = new OperatiiConditiiComanda();
            return conditii.salveazaConditiiComanda(conditiiComanda);

        }





        [WebMethod]
        public string saveCondAndroid(string artCond)
        {
            //salvare conditii suplimentare aprobare comanda
            string retVal = "-1";


            OracleConnection connection = new OracleConnection();

            string query = "";
            string[] mainToken = artCond.Split('@');

            try
            {
                string connectionString = GetConnectionString_android();

                DateTime cDate = DateTime.Now;
                string year = cDate.Year.ToString();
                string day = cDate.Day.ToString("00");
                string month = cDate.Month.ToString("00");
                string nowDate = year + month + day;
                string hour = cDate.Hour.ToString();
                string minute = cDate.Minute.ToString();
                string sec = cDate.Second.ToString();
                string nowTime = hour + minute + sec;

                connection.ConnectionString = connectionString;
                connection.Open();

                string[] antetCondToken = mainToken[0].Split('#');
                string[] articoleCmdToken;

                OracleCommand cmd = connection.CreateCommand();

                //date antet

                //inserare antet conditie
                query = " insert into sapprd.zcondheadtableta(mandt,id,codpers,datac,orac,cmdref, cmdmodif,condcalit,nrfact,observatii) " +
                        " values ('900',pk_key.nextval, :codAg,:datac,:orac,:cmdref, " +
                        " :cmdmodif,:condcalit,:nrfact,:observatii) returning id into :id ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAg", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = antetCondToken[0];

                cmd.Parameters.Add(":datac", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = nowDate;

                cmd.Parameters.Add(":orac", OracleType.VarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = nowTime;

                cmd.Parameters.Add(":cmdref", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = antetCondToken[1];

                cmd.Parameters.Add(":cmdmodif", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = "0";

                cmd.Parameters.Add(":condcalit", OracleType.Float, 26).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = antetCondToken[2];

                cmd.Parameters.Add(":nrfact", OracleType.Float, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = antetCondToken[3];

                cmd.Parameters.Add(":observatii", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                if (antetCondToken[4] == "")
                    cmd.Parameters[7].Value = " ";
                else
                    cmd.Parameters[7].Value = antetCondToken[4];


                OracleParameter idCmd = new OracleParameter("id", OracleType.Number);
                idCmd.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(idCmd);

                cmd.ExecuteNonQuery();

                //articole conditie
                int artLen = mainToken.Length - 2;
                int pozArt = 0;
                for (int i = 0; i < artLen; i++)
                {

                    pozArt = i + 1;
                    articoleCmdToken = mainToken[i + 1].Split('#');

                    string codArt = articoleCmdToken[0];
                    if (codArt.Length == 8)
                        codArt = "0000000000" + codArt;

                    query = " insert into sapprd.zconddettableta(mandt,id,poz,codart,cant,um,valoare) " +
                            " values ('900',:idCmd,:pozArt, :codArt, :cant, :um, :valoare)";


                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":idCmd", OracleType.Int32, 10).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = idCmd.Value;

                    cmd.Parameters.Add(":pozArt", OracleType.Int16, 3).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = pozArt;

                    cmd.Parameters.Add(":codArt", OracleType.NVarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = codArt;

                    cmd.Parameters.Add(":cant", OracleType.Double, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = articoleCmdToken[1];

                    cmd.Parameters.Add(":um", OracleType.NVarChar, 9).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = articoleCmdToken[2];

                    cmd.Parameters.Add(":valoare", OracleType.Double, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[5].Value = articoleCmdToken[3];

                    cmd.ExecuteNonQuery();
                }

                retVal = "0";

            }
            catch (Exception ex)
            {
                retVal = "-1";
                sendErrorToMail(ex.ToString() + " " + artCond);
            }
            finally
            {
                connection.Close();
                connection.Dispose();

            }

            return retVal;


        }


        [WebMethod]
        public string saveRedAndroid(string artRed)
        {



            //salvare Reduceri Ulterioare de pe tableta
            string retVal = "-1";

            OracleConnection connection = new OracleConnection();

            string query = "";
            string[] mainToken = artRed.Split('@');

            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            string nowDate = year + month + day;

            string hour = cDate.Hour.ToString();
            string minute = cDate.Minute.ToString();
            string sec = cDate.Second.ToString();
            string nowTime = hour + minute + sec;


            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                string[] antetRedToken = mainToken[0].Split('#');

                string[] clienti = antetRedToken[0].Split(';');
                string tipReducere = antetRedToken[1];
                string tipFrecv = antetRedToken[2];
                string codAgent = antetRedToken[3];
                string filiala = antetRedToken[4];
                string depart = antetRedToken[5];
                string startValabil = antetRedToken[6];
                string stopValabil = antetRedToken[7];
                string coefCal = antetRedToken[8] != "" ? antetRedToken[8] : "0";
                string valTotDepart = antetRedToken[9] != "-1" ? antetRedToken[9] : "0";
                string selTipClient = antetRedToken[10];
                string procRedGlobal = antetRedToken[11];
                string cmdAngajId = antetRedToken[12];
                string stareSablon = "X"; //blocat
                string patternID = "";
                int nrClienti = clienti.Length;

                if (tipFrecv == "F" || tipFrecv.Trim() == "Fara frecventa")
                    tipFrecv = " ";

                if (!cmdAngajId.Equals("-1"))
                    stareSablon = " ";


                string[] dtStart = startValabil.Split('.');
                string varStartV = dtStart[2] + dtStart[1] + dtStart[0];

                string[] dtStop = stopValabil.Split('.');
                string varStopV = dtStop[2] + dtStop[1] + dtStop[0];

                //daca sablonul este pe tip de clienti se face unul singur
                if (selTipClient.Equals("true"))
                    nrClienti = 1;

                OracleCommand cmd = connection.CreateCommand();

                //se genereaza cate un sablon pentru fiecare client;


                //procentul de reducere se scrie in header
                //string[] artVar = mainToken[1].Split('@');
                //string[] dateArtVar = artVar[0].Split('#');
                string procRedVar = procRedGlobal;

                int ii = 0;
                for (ii = 0; ii < nrClienti; ii++)
                {

                    //ZSD_RU_PATTERN - o linie
                    query = " insert into sapprd.ZSD_RU_PATTERN(mandt, pattern_id,pattern_desc,created_on,ru_type,valid_from, " +
                            " valid_to,spart,werks,frecventa, inactiv, isgroup, splag, toate_mat, flag_dd, deleted, reducere, id_tableta )  values ( " +
                            " ('900'), " +
                            " to_char('" + tipReducere + "' || '" + depart.Substring(0, 2) + "' || to_char((select max(to_number(substr(pattern_id,-9))) + 1  " +
                            " from sapprd.ZSD_RU_PATTERN where mandt='900' and length(pattern_id) = 14 ),'FM0000000000') ), " +
                            " to_char(" + codAgent + ",'FM00000000'), ('" + nowDate + "'), ('" + tipReducere + "'), ('" + varStartV + "'), ('" + varStopV + "'), " +
                            " ('" + depart + "'), ('" + filiala + "'), ('" + tipFrecv + "'), ('" + stareSablon + "') , ('X') , ('X'), (" + valTotDepart + "), ('X'), (' '), (" + procRedVar + ") " +
                            " , (" + cmdAngajId + ")) ";


                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Clear();
                    cmd.ExecuteNonQuery();

                    query = " select max(substr(pattern_id, 5)) from sapprd.ZSD_RU_PATTERN where mandt='900' and created_on =:datac and pattern_desc =:codAgent ";

                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":datac", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = nowDate;

                    cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 120).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codAgent;

                    OracleDataReader oReader = cmd.ExecuteReader();
                    oReader.Read();
                    patternID = tipReducere + depart.Substring(0, 2) + oReader.GetString(0);

                    oReader.Close();

                    //ZSD_RU_PATTERN_B - o linie
                    query = " insert into sapprd.ZSD_RU_PATTERN_B(mandt, pattern_id, werks, created_on, ru_type, valid_from, " +
                            "valid_to, frecventa, spart, flag_dd)  values ( " +
                            " ('900'), " +
                            " ('" + patternID + "'), " +
                            " ('" + filiala + "'), ('" + nowDate + "'), ('" + tipReducere + "'), ('" + varStartV + "'), ('" + varStopV + "'), " +
                            " ('" + tipFrecv + "'), ('" + depart.Substring(0, 2) + "'), ('X') ) ";

                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.ExecuteNonQuery();


                    //ZSD_RU_PATTERN_H - o linie
                    query = " insert into sapprd.ZSD_RU_PATTERN_H(mandt, pattern_id, created_on, created_time, ru_type, valid_from, " +
                            "valid_to, frecventa, spart, werks)  values ( " +
                            " ('900'), " +
                            " ('" + patternID + "'), " +
                            " ('" + nowDate + "') , ('" + nowTime + "'), ('" + tipReducere + "'), ('" + varStartV + "'), ('" + varStopV + "'), " +
                            " ('" + tipFrecv + "'), ('" + depart.Substring(0, 2) + "'), ('" + filiala + "') ) ";

                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.ExecuteNonQuery();


                    //ZSD_RU_PATT_SO - cate o linie pentru valabilitate, filiala, clienti

                    //interval valabilitate
                    query = " insert into sapprd.ZSD_RU_PATT_SO (mandt, pattern_id, pattern_sub_id, so_name, so_sign, counter, so_option, so_low, so_high)  values ( " +
                            " ('900'), " +
                            " ('" + patternID + "'), '1'," +
                            " ('SB5DATEA') ,('I') ,1, ('BT'), ('" + varStartV + "'), ('" + varStopV + "') ) ";

                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.ExecuteNonQuery();

                    //filiala
                    query = " insert into sapprd.ZSD_RU_PATT_SO (mandt, pattern_id, pattern_sub_id, so_name, so_sign, counter, so_option, so_low)  values ( " +
                            " ('900'), " +
                            " ('" + patternID + "'), '1'," +
                            " ('SB5WERKS') ,('I') ,1, ('EQ'), ('" + filiala + "') ) ";


                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.ExecuteNonQuery();

                    //client, cate unul pe sablon 
                    if (!selTipClient.Equals("true"))
                    {

                        var localCodClient = clienti[ii];
                        if (!localCodClient.StartsWith("41"))
                            localCodClient = OperatiiClienti.getCodClient(connection, clienti[ii]);

                        query = " insert into sapprd.ZSD_RU_PATT_SO (mandt, pattern_id, pattern_sub_id, so_name, so_sign, counter, so_option, so_low)  values ( " +
                           " ('900'), " +
                           " ('" + patternID + "'), '0001'," +
                           " ('SB5KUNNR'),('I') ," + (ii + 1) + ", ('EQ'), ('" + localCodClient + "') ) ";

                        cmd.CommandText = query;
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Clear();
                        cmd.ExecuteNonQuery();
                    }
                    else//tip de client, un sablon pentru toate tipurile
                    {
                        int jj = 0, kk = clienti.Length;
                        for (jj = 0; jj < kk; jj++)
                        {

                            var localCodCategorie = clienti[ii];
                            if (!localCodCategorie.StartsWith("0"))
                                localCodCategorie = OperatiiClienti.getCodCategorieClient(clienti[ii]);

                            query = " insert into sapprd.ZSD_RU_PATT_SO (mandt, pattern_id, pattern_sub_id, so_name, so_sign, counter, so_option, so_low)  values ( " +
                          " ('900'), " +
                          " ('" + patternID + "'), '0001'," +
                          " ('SB5KDGRP'),('I') ," + (jj + 1) + ", ('EQ'), ('" + localCodCategorie + "') ) ";

                            cmd.CommandText = query;
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Clear();
                            cmd.ExecuteNonQuery();

                        }

                    }


                    //ZSD_RU_B_PATTOBJ - cate o linie pentru fiecare articol/sintetic din sablon
                    //ZSD_RU_B_PATTOBE - cate o linie pentru fiecare articol exceptie din sablon

                    int i;
                    string[] unArticol;
                    string[] dateArticol;
                    string codArticol = " ";
                    string codSintetic = " ";
                    string codNivel1 = " ";
                    string numeArticol = " ";
                    string numeSintetic = " ";
                    string numeNivel1 = " ";
                    string pragCant = " ";
                    string umPrag = " ";
                    string pragVal = " ";
                    string procRed = " ";
                    string umPragVal = " ";
                    string valNivel = " ";
                    // string[] artExceptii;
                    int cntExc = 1;

                    //parcurgere articole
                    for (i = 1; i < mainToken.Length - 1; i++)
                    {
                        unArticol = mainToken[i].Split('@');
                        dateArticol = unArticol[0].Split('#');

                        if (dateArticol[0] != "3") //nu este exceptie
                        {
                            if (dateArticol[0] == "1") //sintetic
                            {
                                codSintetic = dateArticol[1];
                                numeSintetic = dateArticol[2];
                                codArticol = " ";
                                numeArticol = " ";
                                codNivel1 = " ";
                                numeNivel1 = " ";
                                valNivel = " ";

                            }
                            if (dateArticol[0] == "2") //articol
                            {
                                codArticol = "0000000000" + dateArticol[1];
                                numeArticol = dateArticol[2];
                                numeSintetic = " ";
                                codSintetic = " ";
                                codNivel1 = " ";
                                numeNivel1 = " ";
                                valNivel = " ";
                            }

                            if (dateArticol[0] == "4") //nivel1
                            {
                                codArticol = " ";
                                numeArticol = " ";
                                numeSintetic = " ";
                                codSintetic = " ";
                                codNivel1 = dateArticol[1];
                                numeNivel1 = dateArticol[2];
                                valNivel = "1";
                            }

                            if (dateArticol[4] == "RON") //unit. mas. prag
                            {
                                pragVal = dateArticol[3];
                                pragCant = "0";
                                umPrag = " ";
                                umPragVal = "RON";
                            }
                            else
                            {
                                pragCant = dateArticol[3];
                                umPrag = dateArticol[4];
                                pragVal = "0";
                                umPragVal = " ";
                            }


                            procRed = dateArticol[5];


                            query = " insert into sapprd.ZSD_RU_B_PATTOBJ(mandt, pattern_id, counter_pk, counter, " +
                                    " main_matkl, desc_main_matkl, main_matnr, desc_main_matnr, fkimg, vrkme, netwr, " +
                                    " waerk, reducere, codnivel, desc_nivel, coef_cal, pattern_sub_id, nivel )  values ( " +
                                    " ('900'), " +
                                    " ('" + patternID + "'), " +
                                    " (" + i + ") , (" + i + "), ('" + codSintetic + "') , ('" + numeSintetic + "') ,  " +
                                    " ('" + codArticol + "') , ('" + numeArticol + "') , (" + pragCant + "), " +
                                    " ('" + umPrag + "') , (" + pragVal + "), ('" + umPragVal + "') , (" + procRed + "), " +
                                    " ('" + codNivel1 + "') , ('" + numeNivel1 + "'), (" + coefCal + "), ('0001'), ('" + valNivel + "') ) ";





                            cntExc++;

                            cmd.CommandText = query;
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Clear();
                            cmd.ExecuteNonQuery();

                        }
                        else
                        {
                            //adaugare exceptii
                            string oExceptie = "";
                            string varCamp = "", varNume = "";

                            if (dateArticol[6].Length > 1)
                            {

                                oExceptie = dateArticol[6];

                                if (dateArticol[7] == "1") //exceptia este sintetic
                                {
                                    varCamp = " main_matkl, desc_main_matkl ";
                                    varNume = " ( select nume from sintetice where cod = '" + oExceptie + "') ) ";
                                }
                                else if (dateArticol[7] == "2") //exceptia este articol
                                {

                                    if (oExceptie.Length == 8)
                                        oExceptie = "0000000000" + oExceptie;

                                    varCamp = " main_matnr, desc_main_matnr ";
                                    varNume = " ( select nume from articole where cod = '" + oExceptie + "') ) ";
                                }
                                else if (dateArticol[7] == "3") //exceptia este nivel1
                                {
                                    varCamp = " codnivel, desc_nivel ";
                                    varNume = " ( select distinct NUME_NIVEL1 from sintetice where cod_nivel1 = '" + oExceptie + "') ) ";
                                }

                                query = " insert into sapprd.ZSD_RU_B_PATTOBE(mandt, pattern_id, counter_pk, counter, " + varCamp +
                                        "  )  values ( " +
                                        " ('900'), " +
                                        " ('" + patternID + "'), " +
                                        " (" + cntExc + ") , (" + cntExc + "), ('" + oExceptie + "'),  " + varNume;


                                cntExc++;

                                cmd.CommandText = query;
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Clear();
                                cmd.ExecuteNonQuery();

                            }
                        }//sf. exceptii


                    }//sf. parcurgere articole


                }//sf. clienti


                //webservice pentru discount clienti
                SAPWebServices.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE();

                SAPWebServices.ZactDiscMaxim inParam = new ZactDiscMaxim();

                inParam.PattId = patternID;

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                webService.Credentials = nc;
                webService.Timeout = 300000;

                SAPWebServices.ZactDiscMaximResponse outParam = webService.ZactDiscMaxim(inParam);

                webService.Dispose();
                //sf. web call




                retVal = patternID;


            }
            catch (Exception ex)
            {
                retVal = "-1";
                sendErrorToMail(ex.ToString() + " Art.red:" + artRed + " q = " + query);
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }


            return retVal;


        }


        [WebMethod]
        public string getAdreseLivrareClient(string codClient, string filiala)
        {

            string serializedResult = " ";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string tabFiliala = "";
            string condFiliala = "";

            /*
            if (filiala != null && filiala.Length == 4)
            {
                tabFiliala = " , sapprd.zpdl_jud j ";
                condFiliala = " and j.mandt='900' and j.werks =:filiala and j.zregio = a.region ";
            }
            */

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                OracleCommand cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();


                cmd.CommandText = " select nvl(a.city1,' ') city1 , nvl(a.street,' ') street, " +
                                  " nvl(a.house_num1, ' ') house_num, nvl(region, ' '), a.addrnumber, nvl(trim(c.latitude),-1), nvl(trim(c.longitude),-1), nvl(t.greutate, 0), " +
                                  " nvl(b1.oras, '-1') oras, nvl(b1.razakm, -1) raza, nvl(c1.latitudine, -1) lat, nvl(c1.longitudine, -1) lon " +
                                  " from sapprd.adrc a, sapprd.zadreseclienti c, sapprd.ztonajclient t, " +
                                  " sapprd.zlocalitati a1, sapprd.zoraseromania b1, sapprd.zcoordlocalitati c1 " + tabFiliala + 
                                  " where a.client = '900' and t.mandt(+) = '900' and t.kunnr(+) = c.codclient and t.adrnr(+) = c.codadresa " +
                                  " and c.codclient = :codClient and c.codadresa = a.addrnumber and " +
                                  " a1.mandt = '900' and b1.mandt(+) = '900' and c1.mandt(+) = '900' and a1.bland = region " +
                                  " and trim(upper(a1.localitate)) = trim(upper(a.city1)) and a1.bland = b1.codjudet(+) and upper(a1.localitate) = upper(b1.oras(+)) and c1.judet(+) = b1.numejudet " +
                                  " and upper(c1.localitate(+)) = upper(b1.oras) " + condFiliala + " order by region, city1 ";

                

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;
                /*
                if (filiala != null && filiala.Length == 4)
                {
                    cmd.Parameters.Add(":filiala", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = filiala;
                }
                */

                oReader = cmd.ExecuteReader();


                List<AdresaLivrareClient> listaAdreseLivrare = new List<AdresaLivrareClient>();
                AdresaLivrareClient oAdresa = null;



                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        oAdresa = new AdresaLivrareClient();
                        oAdresa.oras = oReader.GetString(0);
                        oAdresa.strada = oReader.GetString(1);
                        oAdresa.nrStrada = oReader.GetString(2);
                        oAdresa.codJudet = oReader.GetString(3);
                        oAdresa.codAdresa = oReader.GetString(4);
                        oAdresa.tonaj = oReader.GetDouble(7).ToString();

                        oAdresa.coords = oReader.GetString(5) + "," + oReader.GetString(6);

                        oAdresa.isOras = !oReader.GetString(8).Equals("-1");
                        oAdresa.razaKm = oReader.GetInt32(9);
                        oAdresa.coordsCentru = oReader.GetString(10) + "," + oReader.GetString(11);
                        

                        if (oAdresa.strada.Trim().Length > 0)
                            listaAdreseLivrare.Add(oAdresa);

                    }

                }

                DatabaseConnections.CloseConnections(oReader, cmd);

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaAdreseLivrare);


            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            

                return serializedResult;

        }





        [WebMethod]
        public string getClientDetAndroid(string codClient, string depart)
        {



            string retVal = "-1";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string adresa = "-";
            string exista = "";
            float limCredit = 0;
            float restCredit = 0;
            string condClient = "", condClient1 = "", condClient2 = "";
            try
            {



                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                if (depart == "11")
                {
                    condClient = " and canal = '20' ";
                    condClient1 = " and vtweg = '20' ";
                    condClient2 = "  and p.vtweg = '20' and p.spart = '11' ";
                }
                else
                {
                    condClient = " and canal = '10' ";
                    condClient1 = " and vtweg = '10' ";

                    string condDepS = "";
                    if (!depart.Equals("00"))
                        condDepS = " and p.spart = '" + depart + "'";

                    condClient2 = "  and p.vtweg = '10' " + condDepS + " ";
                }


                cmd = connection.CreateCommand();

                String condDepartTip = "";
                if (!depart.Equals("00"))
                {
                    condDepartTip = " and depart = '" + depart + "'";
                }


                cmd.CommandText = " select nvl(min(x.tip),0) exista from " +
                                  " (select tip from clie_tip where cod_cli=:codCl " + condClient + condDepartTip + "  ) x ";




                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codCl", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    exista = oReader.GetString(0);

                }

                if (exista.Equals("0"))  //clientul are restrictie pe departament
                {
                    retVal = "-1";
                    oReader.Close();
                    oReader.Dispose();
                }
                else
                {
                    //aflare adresa
                    string strada = "-", nrStr = "-", oras = "-", regiune = "-";

                    cmd = connection.CreateCommand();

                    cmd.CommandText = " select a.city1, a.street, a.house_num1, a.region " +
                            " from sapprd.adrc a " +
                            " where a.client = '900' and a.addrnumber = " +
                            " (select k.adrnr from sapprd.kna1 k where k.mandt = '900' and k.kunnr =:k) ";

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        oras = oReader.GetString(0);
                        strada = oReader.GetString(1);
                        nrStr = oReader.GetString(2);
                        regiune = oReader.GetString(3);

                        oReader.Close();
                        oReader.Dispose();

                    }

                    adresa = oras + "!" + strada + "!" + nrStr + "!" + regiune;
                    retVal = adresa;

                    //limita de credit
                    DateTime cDate = DateTime.Now;
                    string year = cDate.Year.ToString();
                    string month = cDate.Month.ToString("00");
                    string day = cDate.Day.ToString("00");
                    string nowDate = year + month;
                    string clBlocat = " ";

                    cmd = connection.CreateCommand();
                    cmd.CommandText = " select distinct k.kunnr,k.klimk lcredit, " +
                      " k.skfor,k.ssobl, nvl((select s2.olikw+s2.ofakw from sapprd.s067 s2 where s2.mandt='900' and " +
                      " s2.kkber='1000' and s2.knkli=k.kunnr),0) olikw, nvl((select sum(s1.oeikw) from sapprd.s066 s1 " +
                      " where s1.mandt='900' and s1.kkber='1000' and spmon=:l  and s1.knkli=k.kunnr),0) oeikw,  k.crblb  from sapprd.knkk k " +
                      " where k.mandt='900' and k.kkber='1000' and k.kunnr=:k ";



                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":l", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = nowDate;

                    cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codClient;

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        limCredit = oReader.GetFloat(1);
                        restCredit = oReader.GetFloat(1) - (oReader.GetFloat(2) + oReader.GetFloat(3)) - (oReader.GetFloat(4) + oReader.GetFloat(5));
                        clBlocat = oReader.GetString(6);

                        oReader.Close();
                        oReader.Dispose();
                    }
                    else
                    {
                        limCredit = 0;
                        restCredit = 0;
                    }

                    retVal += "#" + limCredit.ToString() + "#" + restCredit.ToString();


                    //peroana de contact

                    string condDepartPersCont = "";
                    if (!depart.Equals("00"))
                    {
                        condDepartPersCont = " and spart = '" + depart + "' ";
                    }

                    cmd = connection.CreateCommand();
                    cmd.CommandText = " select namev, name1, telf1 from sapprd.knvk u where u.mandt = '900' and " +
                        " (u.parnr, u.kunnr) in (select distinct p.parnr, p.kunnr from sapprd.knvp p where p.mandt = '900' " +
                        " and p.kunnr =:k and parvw = 'AP' " + condDepartPersCont + condClient1 + " )";



                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    string nume = "", tel = "";

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            nume += oReader.GetString(0) + " " + oReader.GetString(1) + ":";
                            tel += oReader.GetString(2) + ":";
                        }
                        oReader.Close();
                        oReader.Dispose();
                    }
                    //sf. pers.cont.

                    retVal += "#" + nume + "#" + tel + "#" + clBlocat + "#" + exista + "#";



                    //curs valutar
                    string cursDate = year + month + day;
                    cmd = connection.CreateCommand();

                    string condDepartCursV = "";
                    if (!depart.Equals("00"))
                    {
                        condDepartCursV = " and b.spart = '" + depart + "' ";
                    }

                    cmd.CommandText = " select x.ukurs from (select distinct a.ukurs, (100000000 - a.gdatu - 1) from sapprd.tcurr a, sapprd.knvv b where a.mandt='900' and " +
                                      " b.mandt='900' and b.kurst = a.kurst and b.kunnr =:k and fcurr = 'EUR' " +
                                      " and tcurr = 'RON' and (100000000 - a.gdatu - 1) <= " + cursDate + condDepartCursV +
                                      " order by 100000000 - a.gdatu - 1 desc ) x where rownum<2";



                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    string cursValut = "0";

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        cursValut = oReader.GetDouble(0).ToString();
                    }

                    retVal += cursValut + "#";


                    oReader.Close();
                    oReader.Dispose();


                    //motivul blocarii
                    string strBlocare = " ";
                    if (clBlocat.Equals("X"))
                    {
                        cmd = connection.CreateCommand();
                        cmd.CommandText = " select k.grupp, c.gtext from sapprd.knkk k, sapprd.zt691c c " +
                                          " where k.mandt = '900' and c.mandt = '900' and k.kunnr =:k " +
                                          " and k.mandt = c.mandt and k.grupp = c.grupp ";

                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = codClient;

                        oReader = cmd.ExecuteReader();
                        if (oReader.HasRows)
                        {
                            oReader.Read();
                            strBlocare = oReader.GetString(1).ToString();
                        }
                        else
                            strBlocare = " ";

                        oReader.Close();
                        oReader.Dispose();

                    }

                    retVal += strBlocare + "#";

                    //filiala clientului
                    string filClnt = " ";
                    cmd = connection.CreateCommand();
                    cmd.CommandText = " select p.kunn2 from sapprd.knvp p where p.mandt = '900' and p.kunnr =:k " +
                                      " and p.parvw = 'ZA' " + condClient2;


                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        filClnt = oReader.GetString(0).ToString();
                    }
                    else
                        filClnt = " ";

                    retVal += filClnt + "#";

                    oReader.Close();
                    oReader.Dispose();


                    //termen de plata
                    string termPlata = " ";
                    cmd = connection.CreateCommand();

                    cmd.CommandText = " select u.zterm from sapprd.T052u u, sapprd.TVZBT t where u.mandt = '900' and " +
                                      " u.spras = '4' and u.mandt = t.mandt and u.spras = t.spras and u.zterm = t.zterm " +
                                      " and u.zterm <= (select max(p.zterm) from sapprd.knvv p where p.mandt = '900' " +
                                      " and p.kunnr =:k " + condClient2 + " ) order by u.zterm ";



                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        termPlata = "";
                        while (oReader.Read())
                        {
                            termPlata += oReader.GetString(0).ToString() + ";";
                        }
                    }


                    retVal += termPlata + "#";

                    oReader.Close();
                    oReader.Dispose();

                }


            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = null;
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }


            return retVal;
        }



        [WebMethod]//Android
        public string getSablonArt(string nrSablon)
        {
            string retVal = "", nume = " ", cod = " ";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader1 = null;

            try
            {
                //antet comanda
                string connectionString = GetConnectionString_android();
                string cant_r = "", um_r = "", val_r = "", mon_r = "";

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();



                //continut sablon
                //articole/sintetice + valori realizate

                cmd.CommandText = " select decode(a.main_matkl,' ','-1',a.main_matkl) codsint , " +
                    " decode(a.desc_main_matkl,' ','-1',a.desc_main_matkl) numesint, " +
                    " decode(length(a.main_matnr),18,substr(a.main_matnr,-8),decode(a.main_matnr,' ','-1',a.main_matnr)) codart, " +
                    " decode(a.desc_main_matnr,' ','-1',a.desc_main_matnr) numeart, " +
                    " a.fkimg cant, a.vrkme um, a.netwr val, a.waerk mon, " +
                    " a.reducere, nvl(b.FKIMG_R,-1) cant_r,  nvl(b.VRKME_R,-1) um_r , nvl(b.NETWR_R,-1) val_r, " +
                    " nvl(b.WAERK_R,-1) mon_r, " +
                    " decode(a.codnivel,' ','-1',a.codnivel) codnivel , " +
                    " decode(a.desc_nivel,' ','-1',a.desc_nivel) descnivel  " +
                    " from sapprd.zsd_ru_b_pattobj a, sapprd.ZRED_GRAD_REALIZ b  where a.mandt = '900'   " +
                    " and b.mandt(+)='900' and b.pattern_id(+) = a.pattern_id and b.counter_pk(+) = a.counter_pk and " +
                    " a.pattern_id=:idsabl order by a.counter_pk";



                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idsabl", OracleType.NVarChar, 42).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrSablon;


                oReader1 = cmd.ExecuteReader();

                if (oReader1.HasRows)
                {
                    while (oReader1.Read())
                    {

                        if (oReader1.GetDouble(9) == -1) //nu exista realizari
                        {
                            cant_r = "-2";
                            um_r = "-2";
                            val_r = "-2";
                            mon_r = "-2";
                        }
                        else
                        {
                            if (oReader1.GetDouble(9) == 0) //pragul este valoric
                            {
                                cant_r = "-1";
                                um_r = "-1";
                                val_r = oReader1.GetDouble(11).ToString();
                                mon_r = oReader1.GetString(12);
                            }
                            else //pragul este cantitativ
                            {
                                cant_r = oReader1.GetDouble(9).ToString();
                                um_r = oReader1.GetString(10);
                                val_r = "-1";
                                mon_r = "-1";
                            }
                        }

                        retVal += oReader1.GetString(0) + "#" + oReader1.GetString(1) + "#" + oReader1.GetString(2) + "#"
                            + oReader1.GetString(3) + "#" + oReader1.GetDouble(4) + "#" + oReader1.GetString(5) + "#"
                            + oReader1.GetDouble(6) + "#" + oReader1.GetString(7) + "#" + oReader1.GetDouble(8) + "#"
                            + cant_r + "#" + um_r + "#" + val_r + "#" + mon_r + "#" + oReader1.GetString(13) + "#"
                            + oReader1.GetString(14) + "@@";
                    }


                    oReader1.Close();
                    oReader1.Dispose();

                }

                //sf. articole

                //exceptii
                cmd.CommandText = " select " +
                    " decode(main_matkl,' ','-1',main_matkl) codsint, " +
                    " decode(desc_main_matkl,' ','-1',desc_main_matkl) numesint, " +
                    " decode(length(main_matnr),18,substr(main_matnr,-8),decode(main_matnr,' ','-1',main_matnr)) codart, " +
                    " decode(desc_main_matnr,' ','-1',desc_main_matnr) numeart, " +
                    " decode(codnivel,' ','-1',codnivel) codnivel , " +
                    " decode(desc_nivel,' ','-1',desc_nivel) descnivel  " +
                    " from sapprd.zsd_ru_b_pattobe where mandt = '900' and  " +
                    " pattern_id=:idsabl ";



                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idsabl", OracleType.NVarChar, 42).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrSablon;


                oReader1 = cmd.ExecuteReader();

                if (oReader1.HasRows)
                {
                    while (oReader1.Read())
                    {

                        if (oReader1.GetString(0).Equals("-1"))
                        {
                            if (oReader1.GetString(2).Equals("-1"))
                            {
                                cod = oReader1.GetString(4);
                                nume = oReader1.GetString(5);
                            }
                            else
                            {
                                cod = oReader1.GetString(2);
                                nume = oReader1.GetString(3);
                            }

                        }
                        else
                        {
                            cod = oReader1.GetString(0);
                            nume = oReader1.GetString(1);
                        }

                        retVal += "-2" + "#" + cod + "#" + nume + "@@";

                    }


                    oReader1.Close();
                    oReader1.Dispose();

                }

                //sf. exceptii


            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + " Pattern_id: " + nrSablon);
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }


            return retVal;

        }


        [WebMethod]
        public string getArticoleComanda(string nrCmd, string afisCond, string tipUser)
        {
            OperatiiComenzi opComenzi = new OperatiiComenzi();
            return opComenzi.getArticoleComanda(nrCmd, afisCond, tipUser);
        }

        [WebMethod]
        public string getComenziDeschise(string codAgent)
        {
            OperatiiComenzi opComenzi = new OperatiiComenzi();
            return opComenzi.getComenziDeschise(codAgent);
        }

        [WebMethod]
        public string getClientiBorderou(string codBorderou)
        {
            OperatiiComenzi opComenzi = new OperatiiComenzi();
            return opComenzi.getClientiBorderou(codBorderou);
        }

        [WebMethod]
        public string getPozitieMasina(string nrMasina)
        {
            OperatiiComenzi opComenzi = new OperatiiComenzi();
            return opComenzi.getPozitieMasina(nrMasina);
        }

        [WebMethod]
        public string getStareTVA(string cuiClient)
        {
            return new VerificaTva().isPlatitorTva(cuiClient).ToString();
        }

        [WebMethod]
        public string getArticoleCategorieMathaus(string codCategorie, string filiala, string depart, string pagina, string tipArticol)
        {
            return new OperatiiMathaus().getArticoleCategorie(codCategorie, filiala, depart, pagina, tipArticol);
        }


        [WebMethod]
        public int getNrArticoleCategorieMathaus(string codCategorie, string filiala, string depart)
        {
            return new OperatiiMathaus().getNrArticoleCategorie( codCategorie,  filiala,  depart);
        }

        


        [WebMethod]
        public string getCategoriiMathaus()
        {
            return new OperatiiMathaus().getListaCategorii();
        }

        [WebMethod]
        public string getStocMathaus(string filiala, string codArticol, string um, string tipCmd, string tipUserSap, string codUser)
        {
            return new OperatiiMathaus().getStocMathaus(filiala, codArticol, um,  tipCmd,  tipUserSap,  codUser);
        }

        [WebMethod]
        public string getLivrariMathaus(string antetComanda, string comandaMathaus, string canal)
        {
            return new OperatiiMathaus().getLivrariComanda(antetComanda, comandaMathaus, canal);
        }

        [WebMethod]
        public string cautaArticoleMathaus(string codArticol, string tipCautare, string filiala, string depart, string pagina)
        {
            return new OperatiiMathaus().cautaArticoleMathaus(codArticol, tipCautare, filiala, depart, pagina);
        }

        [WebMethod]
        public string cautaArticoleLocal(string codArticol, string tipCautare, string filiala, string depart, string pagina)
        {
            return new OperatiiMathaus().cautaArticoleLocal(codArticol, tipCautare, filiala, depart, pagina);
        }

        [WebMethod]
        public string getInfoPretMathaus(string parametruPret, string cantitati)
        {
            return new OperatiiArticole().getInfoPretMathaus(parametruPret, cantitati);
        }

        [WebMethod]
        public double getValoareComenziNumerar(string codClient, string dataLivrare, string tipClient)
        {
            return new HelperClienti().getValoareComenziNumerar(codClient, dataLivrare, tipClient);
        }

        [WebMethod]
        public string getArticoleCant(string unitLog, string codArtPal)
        {
            return new ArticoleCant().getArticoleCant(unitLog, codArtPal);
        }



        [WebMethod]
        public string getCmdArt(string nrCmd, string afisCond, string tipUser)
        {
            string retVal = "";
            string unitLog1 = "", termenPlata = "", obsLivrare = "", tipPersClient = "";
            string cmp = "";
            //
            //afisCond:
            // 1 - pentru agent
            // 2 - pentru aprobare comanda SD
            // 3 - pentru aprobare comanda DV


            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null, oReader1 = null;
            int nrArt = 0;
            string varArtCmd = "", infoPret = " , '0' info";

            try
            {
                //antet comanda
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                cmd.CommandText = " select a.pers_contact, a.telefon, a.adr_livrare, a.mt, a.tip_plata, a.cantar, a.fact_red, a.city, a.region, a.ul, " +
                                  " decode(a.pmnttrms,'',' ',a.pmnttrms) pmnttrms, decode(a.obstra,'',' ',a.obstra) obstra, b.tip_pers from sapprd.zcomhead_tableta a, clienti b " +
                                  " where a.id=:idcmd and a.cod_client = b.cod ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrCmd);

                oReader = cmd.ExecuteReader();




                if (oReader.HasRows)
                {
                    oReader.Read();

                    retVal += oReader.GetString(0) + "#" + oReader.GetString(1) + "#" + oReader.GetString(2) + "#" +
                              oReader.GetString(3) + "#" + oReader.GetString(4) + "#" + oReader.GetString(5) + "#" +
                              oReader.GetString(6) + "#" + oReader.GetString(7) + "#" + oReader.GetString(8);



                    unitLog1 = oReader.GetString(9);
                    termenPlata = oReader.GetString(10);
                    obsLivrare = oReader.GetString(11);
                    tipPersClient = oReader.GetString(12);
                }
                else
                {
                    retVal = "-1";
                }

                oReader.Close();
                oReader.Dispose();

                //articole comenzi

                if (afisCond == "3" || afisCond == "1") //3 = pentru DV se afiseaza si CMP, 1 = modif.cmd pt. a verifica art. sub cmp
                {
                    cmp = " , nvl(( select nvl(to_char(decode(y.lbkum,0,y.verpr,y.salk3/y.lbkum),'99999.9999'),0) from sapprd.mbew y where " +
                                 " y.mandt='900' and y.matnr=a.cod  and y.bwkey = '" + unitLog1 + "'" +
                                 " ),0) cmp  ";
                }
                else
                {
                    cmp = ", '-1' cmp ";
                }

                string condTabKA = " , sapprd.zcomhead_tableta z ", condIdKA = " and a.id=:idcmd and z.id = a.id ", condOrderKA = " trim(poz) ";
                if (tipUser == "KA")
                {
                    // condTabKA = " , sapprd.zcomhead_tableta z ";
                    // condIdKA = " and z.parent_id=:idcmd and z.id = a.id ";
                    // condOrderKA = " depoz, poz ";
                }



                infoPret = ", nvl(a.inf_pret,'0') infopret";

                cmd.CommandText = " select a.status, decode(length(a.cod),18,substr(a.cod,-8),a.cod) cod ,nvl(b.nume,' '), a.cantitate, decode(trim(a.depoz),'','0000',a.depoz) depoz, " +
                                  " a.valoare, a.um, a.procent, nvl(a.procent_fc,0) procent_fc, decode(trim(a.conditie),'','0',a.conditie) conditie " + cmp +
                                  " , nvl(a.disclient,0) disclient, nvl(a1.discount,0) av, " +
                                  " nvl((select distinct discount from sapprd.zdisc_pers_sint where  functie='SD' and spart =substr(c.cod_nivel1,2,2) " +
                                  " and werks ='" + unitLog1 + "' and inactiv <> 'X' and matkl = c.cod),0) sd, " +
                                  " nvl((select distinct discount from sapprd.zdisc_pers_sint where  functie='DV' and spart =substr(c.COD_NIVEL1,2,2) " +
                                  " and werks ='" + unitLog1 + "' and inactiv <> 'X' and matkl = c.cod),0) dv, nvl(a.procent_aprob,0) procent_aprob, " +
                                  " decode(s.matkl,'','0','1') permitsubcmp, nvl(a.multiplu,1) multiplu, nvl(a.val_poz,0) " + infoPret +
                                  " ,nvl(a.cant_umb,0) cant_umb , nvl(a.umb,' ') umb, a.ul_stoc, z.depart, nvl(b.tip_mat,' ')  from sapprd.zcomdet_tableta a, sapprd.zdisc_pers_sint a1,  sintetice c," +
                                  " articole b, sapprd.zpretsubcmp s " + condTabKA + " where a.cod = b.cod(+) " + condIdKA + " and " +
                                  " a1.inactiv(+) <> 'X' and a1.functie(+)='AV' and a1.spart(+)=substr(c.COD_NIVEL1,2,2) and a1.werks(+) ='" + unitLog1 + "' " +
                                  " and b.sintetic = c.cod(+) and a1.matkl(+) = c.cod " +
                                  " and s.mandt(+)='900' and s.matkl(+) = c.cod order by " + condOrderKA;



                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrCmd);

                oReader1 = cmd.ExecuteReader();
                String unitLogAlt = "NN10", depart = "00", tipMat = "", lnumeArt = "", lDepoz = "";

                if (oReader1.HasRows)
                {
                    while (oReader1.Read())
                    {
                        depart = oReader1.GetString(23).Trim() != "" ? oReader1.GetString(23) : "00";
                        tipMat = oReader1.GetString(24);
                        lnumeArt = oReader1.GetString(2).Replace("#", "");
                        lDepoz = oReader1.GetString(4);

                        if (oReader1.GetString(22).Equals("BV90") && depart.Equals("02"))
                        {
                            //  lDepoz = "02T1";
                        }

                        if (oReader1.GetString(22).Equals("BV90") && depart.Equals("05"))
                        {
                            lDepoz = "BV90";
                        }



                        if (tipMat.Trim() != "")
                            lnumeArt += " (" + tipMat + ")";


                        if (oReader1.GetString(1).Equals("00000000"))
                            lnumeArt = "Taxa verde";

                        varArtCmd += oReader1.GetString(0) + "#" + oReader1.GetString(1) + "#" + lnumeArt + "#" +
                                  oReader1.GetFloat(3) + "#" + lDepoz + "#" + oReader1.GetFloat(5) + "#" +
                                  oReader1.GetString(6) + "#" + oReader1.GetFloat(7) + "#" + oReader1.GetFloat(8) + "#" +
                                  oReader1.GetString(9) + "#" + oReader1.GetString(10).Trim() + "#" +
                                  oReader1.GetFloat(11) + "#" + oReader1.GetFloat(12) + "#" + oReader1.GetFloat(13) + "#" +
                                  oReader1.GetFloat(14) + "#" + oReader1.GetFloat(15) + "#" + oReader1.GetString(16) + "#" +
                                  oReader1.GetFloat(17) + "#" + oReader1.GetFloat(18) + "#" + oReader1.GetString(19) + "#" +
                                  oReader1.GetFloat(20) + "#" + oReader1.GetString(21) + "#" + depart + "#" + tipMat + "@@";

                        unitLogAlt = oReader1.GetString(22).Trim() != "" ? oReader1.GetString(22) : "NN10";

                        nrArt++;
                    }


                    retVal += "#" + nrArt.ToString() + "#" + termenPlata + "#" + unitLogAlt + "#" + obsLivrare + "#" + tipPersClient + "@@";
                    retVal += varArtCmd;



                }
                else
                {
                    retVal = "-1";
                }

                oReader1.Close();
                oReader1.Dispose();

                //articole conditii


                //antet
                string condAfis = "";
                int IdCndArt = -1;

                if (afisCond == "1")
                    condAfis = " cmdref=:idcmd ";

                if (afisCond == "2" || afisCond == "3")
                    condAfis = " cmdmodif=:idcmd ";

                cmd.CommandText = " select id, condcalit, nrfact, nvl(observatii,' ') observatii from sapprd.zcondheadtableta where " + condAfis;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrCmd);

                oReader1 = cmd.ExecuteReader();
                if (oReader1.HasRows)
                {
                    oReader1.Read();
                    IdCndArt = oReader1.GetInt32(0);
                    retVal += oReader1.GetFloat(1) + "#" + oReader1.GetInt32(2) + "#" + oReader1.GetString(3) + "@@";

                }

                oReader1.Close();

                string condId = " and a.id = " + IdCndArt;


                //articole conditii
                if (IdCndArt != -1)
                {
                    cmd.CommandText = " select  decode(length(a.codart),18,substr(a.codart,-8),a.codart) codart ,b.nume, a.cant," +
                                      " a.um,a.valoare, nvl(multiplu,1)  " +
                                      " from sapprd.zconddettableta a, " +
                                      " articole b where  a.codart = b.cod " + condId + " order by poz ";




                    cmd.Parameters.Clear();
                    oReader1 = cmd.ExecuteReader();

                    if (oReader1.HasRows)
                    {
                        while (oReader1.Read())
                        {
                            retVal += oReader1.GetString(0) + "#" + oReader1.GetString(1) + "#" + oReader1.GetFloat(2) + "#"
                                      + oReader1.GetString(3) + "#" + oReader1.GetFloat(4) + "#"
                                      + oReader1.GetDouble(5) + "@@";
                        }

                    }

                    oReader1.Close();
                    oReader1.Dispose();

                }
                else
                {
                    retVal += "-1#-1#-1#-1#-1#-1#-1@@"; ;
                }

                //sf. conditii




            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + "Nr cmd: " + nrCmd);
                sendErrorToMail(cmd.CommandText);

                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }


            return retVal;

        }




        [WebMethod]
        public string getArticoleComandaVanzare(string nrCmd, string afisCond, string tipUser, string departament)
        {


            string serializedResult = "", retVal = "";
            string unitLog1 = "", termenPlata = "", obsLivrare = "", tipPersClient = "";
            string cmp = "";

            string conditieDepart = "";

            string condArticole111 = " and ((exists (select 1 from sapprd.zfilhome where  filiala = z.ul) and a.cod not like '0000000000111%') " +
                                     " or " +
                                     " (not exists (select 1 from sapprd.zfilhome where filiala = z.ul)))";

            if ((tipUser.Equals("DV") || tipUser.Equals("DD")) && !departament.Equals("00") && !departament.Equals("11"))
                conditieDepart = " and (b.spart = '" + departament + "' or b.dep_aprobare = '" + departament + "' or b.spart = '11') " + condArticole111;


            string infoPretTransp = ", a.val_transp  ";
            string condBlocAprov = " ,'-1' blocAprov ";

            if (tipUser.Equals("DV") || tipUser.Equals("DD"))
            {
                condBlocAprov = " ,nvl((select '1' from sapprd.mara m where m.mandt = '900' and m.matnr = a.cod and m.mstae = 'Z1'),'-1') blocAprov ";
                infoPretTransp = ", 0 val_transp  ";
            }

            string sinteticArt = " ,'000' sintetic ";
            string lungimeArt = ", 0 lungime ";
            if (tipUser.Equals("DV") && departament.Equals("01"))
            {
                sinteticArt = " ,b.sintetic ";
                lungimeArt = " ,b.lungime ";
            }

            string istoricPret = " ,a.istoric_pret istoricPret ";
            string vechime = ", a.vechime ";

            //
            //afisCond:
            // 1 - pentru agent
            // 2 - pentru aprobare comanda SD
            // 3 - pentru aprobare comanda DV


            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null, oReader1 = null;
            OracleCommand cmdInner = null;
            OracleDataReader oReaderInner = null;
            int nrArt = 0;
            string varArtCmd = "", infoPret = " , '0' info";
            String pretMediu = "0", adaosMediu = "0", unitMasPretMediu = "0";
            String coefCorectie = "0";

            DateLivrareCmd dateLivrare = new DateLivrareCmd();
            List<ArticolComandaRap> listArticole = new List<ArticolComandaRap>();
            Conditii conditii = new Conditii();

            bool isClientInstPubl = false;
            bool isComandaACZC = false;
            string nrCmdSap = "";

            try
            {
                //antet comanda
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                cmd.CommandText = " select a.pers_contact, a.telefon, a.adr_livrare, a.mt, a.tip_plata, a.cantar, a.fact_red, a.city, a.region, a.ul, " +
                                  " decode(a.pmnttrms,'',' ',a.pmnttrms) pmnttrms, decode(a.obstra,'',' ',a.obstra) obstra, b.tip_pers, a.email, a.obsplata, a.ketdat, " +
                                  " a.adr_livrare_d, a.city_d, a.region_d, a.macara, a.nume_client, a.stceg, a.id_obiectiv, a.adresa_obiectiv, " +
                                  " nvl((select latitude||','||longitude from sapprd.zcoordcomenzi where idcomanda = a.id),'0,0') coord, " +
                                  " 0 tonaj, nvl(client_raft,' '), a.meserias, a.fact_palet_separat, a.lifnr, a.lifnr_prod, a.descoperita,  nvl((trim(a.prog_livr)),'0'),  " +
                                  " a.livr_sambata, a.bloc, a.cod_client, a.ref_client , a.docin, a.ac_zc, a.nrcmdsap, a.tip_pers tip_pers_av, fil_plata, a.cod_agent  " +
                                  " from sapprd.zcomhead_tableta a, clienti b " +
                                  " where a.id=:idcmd and a.cod_client = b.cod ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrCmd);

                oReader = cmd.ExecuteReader();


                if (oReader.HasRows)
                {
                    oReader.Read();

                    unitLog1 = oReader.GetString(9);
                    termenPlata = oReader.GetString(10);
                    obsLivrare = oReader.GetString(11);
                    tipPersClient = oReader.GetString(12);

                    dateLivrare.persContact = oReader.GetString(0);
                    dateLivrare.nrTel = oReader.GetString(1);
                    dateLivrare.dateLivrare = oReader.GetString(2);
                    dateLivrare.Transport = oReader.GetString(3);
                    dateLivrare.tipPlata = oReader.GetString(4);
                    dateLivrare.Cantar = oReader.GetString(5);
                    dateLivrare.factRed = oReader.GetString(6);
                    dateLivrare.redSeparat = oReader.GetString(6);
                    dateLivrare.Oras = oReader.GetString(7);
                    dateLivrare.codJudet = oReader.GetString(8);
                    dateLivrare.unitLog = oReader.GetString(9);
                    dateLivrare.termenPlata = oReader.GetString(10);
                    dateLivrare.obsLivrare = oReader.GetString(11);
                    dateLivrare.tipPersClient = oReader.GetString(12);
                    dateLivrare.mail = oReader.GetString(13);
                    dateLivrare.obsPlata = oReader.GetString(14);
                    dateLivrare.dataLivrare = GeneralUtils.formatStrDate(oReader.GetString(15));
                    dateLivrare.adresaD = oReader.GetString(16);
                    dateLivrare.orasD = oReader.GetString(17);
                    dateLivrare.codJudetD = oReader.GetString(18);
                    dateLivrare.macara = oReader.GetString(19);
                    dateLivrare.numeClient = oReader.GetString(20);

                    if (tipPersClient.Equals("PF"))
                        dateLivrare.cnpClient = HelperClienti.getCnpClient(connection, Int32.Parse(nrCmd));
                    else
                        dateLivrare.cnpClient = oReader.GetString(21);

                    dateLivrare.idObiectiv = oReader.GetInt32(22).ToString();
                    dateLivrare.isAdresaObiectiv = oReader.GetString(23).Equals("X") ? true : false;
                    dateLivrare.coordonateGps = oReader.GetString(24);
                    dateLivrare.tonaj = oReader.GetDouble(25).ToString();
                    dateLivrare.clientRaft = oReader.GetString(26).ToString();

                    dateLivrare.meserias = " ";
                    dateLivrare.factPaletiSeparat = oReader.GetString(28).Equals("X") ? true : false;

                    dateLivrare.furnizorMarfa = oReader.GetString(29).Length > 4 ? oReader.GetString(29) : " ";
                    dateLivrare.furnizorProduse = oReader.GetString(30);
                    dateLivrare.isCamionDescoperit = oReader.GetString(31).Equals("X") ? true : false;
                    dateLivrare.diviziiClient = OperatiiClienti.getDiviziiClient(connection, nrCmd);
                    dateLivrare.programLivrare = oReader.GetString(32);
                    dateLivrare.livrareSambata = oReader.GetString(33);
                    dateLivrare.blocScara = oReader.GetString(34);
                    dateLivrare.filialaCLP = oReader.GetString(29).Length == 4 ? oReader.GetString(29) : " ";
                    dateLivrare.refClient = oReader.GetString(36);
                    dateLivrare.tipDocInsotitor = oReader.GetString(37);

                    if (tipUser.Equals("AV") || tipUser.Equals("KA"))
                        OperatiiSuplimentare.getDateDelegat(connection, Int32.Parse(nrCmd), dateLivrare);

                    if (tipUser.Equals("SD") || tipUser.Equals("DV"))
                    {
                        isClientInstPubl = HelperClienti.getDateClientInstPublica(connection, oReader.GetString(35)).isClientInstPublica;
                    }

                    dateLivrare.limitaCredit = 0;
                    if (!tipUser.Equals("DV") && !tipUser.Equals("DD"))
                    {
                        dateLivrare.limitaCredit = HelperClienti.getLimitaCreditClient(connection, oReader.GetString(35));
                    }

                    if (dateLivrare.filialaCLP.Length == 4)
                        dateLivrare.nrCmdClp = HelperComenzi.getNrCmdClp(connection, nrCmd); 
                    else
                        dateLivrare.nrCmdClp = "";

                    dateLivrare.isClientBlocat = HelperClienti.isClientBlocat(connection, oReader.GetString(35));

                    isComandaACZC = oReader.GetString(oReader.GetOrdinal("ac_zc")).Equals("X");

                    nrCmdSap = oReader.GetString(oReader.GetOrdinal("nrcmdsap"));
                    dateLivrare.tipPersAgent = oReader.GetString(oReader.GetOrdinal("tip_pers_av"));

                    dateLivrare.filialaPlata = oReader.GetString(oReader.GetOrdinal("fil_plata"));
                    dateLivrare.tonaj = OperatiiSuplimentare.getTonajComanda(connection, nrCmd);

                    if ((dateLivrare.tipPersAgent.Equals("CV") || dateLivrare.tipPersAgent.Equals("SITE")) && Utils.isUserTest(oReader.GetString(oReader.GetOrdinal("cod_agent"))))
                    {

                        dateLivrare.dateLivrare = oReader.GetString(16);
                        dateLivrare.Oras = oReader.GetString(17);
                        dateLivrare.codJudet = oReader.GetString(18);

                        dateLivrare.adresaD = oReader.GetString(2);
                        dateLivrare.orasD = oReader.GetString(7);
                        dateLivrare.codJudetD = oReader.GetString(8);
                    }



                }


                oReader.Close();
                oReader.Dispose();

                //articole comenzi

                if (afisCond == "3" || afisCond == "1") //3 = pentru DV se afiseaza si CMP, 1 = modif.cmd pt. a verifica art. sub cmp
                {
                    cmp = " , nvl(( select nvl(to_char(decode(y.lbkum,0,y.verpr / y.peinh, (y.salk3/y.lbkum) / y.peinh),'999999.9999'),0) from sapprd.mbew y where " +
                                 " y.mandt='900' and y.matnr=a.cod  and y.bwkey = '" + unitLog1 + "'" +
                                 " ),0) cmp  ";

                   
                }
                else
                {
                    cmp = ", '-1' cmp ";
                }

                string condTabKA = " , sapprd.zcomhead_tableta z ", condIdKA = " and a.id=:idcmd and z.id = a.id ", condOrderKA = " trim(poz) ";
                if (tipUser == "KA")
                {
                    // condTabKA = " , sapprd.zcomhead_tableta z ";
                    // condIdKA = " and z.parent_id=:idcmd and z.id = a.id ";
                    // condOrderKA = " depoz, poz ";
                }



                infoPret = ", nvl(a.inf_pret,'0') infopret";


                if (isFilialaWood(dateLivrare.unitLog))
                    conditieDepart = " and b.spart in ('01','02') ";

                cmd.CommandText = " select a.status, decode(length(a.cod),18,substr(a.cod,-8),a.cod) codart ,nvl(b.nume,' '), a.cantitate, decode(trim(a.depoz),'','0000',a.depoz) depoz, " +
                                  " a.valoare, a.um, a.procent, nvl(a.procent_fc,0) procent_fc, decode(trim(a.conditie),'','0',a.conditie) conditie " + cmp +
                                  " , nvl(a.disclient,0) disclient, nvl(a1.discount,0) av, " +
                                  " nvl((select distinct discount from sapprd.zdisc_pers_sint where  functie='SD' and spart =substr(c.cod_nivel1,2,2) " +
                                  " and werks ='" + unitLog1 + "' and inactiv <> 'X' and matkl = c.cod),0) sd, " +
                                  " nvl((select distinct discount from sapprd.zdisc_pers_sint where  functie='DV' and spart =substr(c.COD_NIVEL1,2,2) " +
                                  " and werks ='" + unitLog1 + "' and inactiv <> 'X' and matkl = c.cod),0) dv, nvl(a.procent_aprob,0) procent_aprob, " +
                                  " decode(s.matkl,'','0','1') permitsubcmp, nvl(a.multiplu,1) multiplu, nvl(a.val_poz,0) " + infoPret +
                                  " ,nvl(a.cant_umb,0) cant_umb , nvl(a.umb,' ') umb, a.ul_stoc, z.depart, nvl(b.tip_mat,' '), nvl(a.ponderat,'-1'), nvl(b.spart,' '), " +
                                  " decode(trim(b.dep_aprobare),'','00', b.dep_aprobare)  dep_aprobare " + condBlocAprov + istoricPret + vechime + infoPretTransp + sinteticArt +
                                  lungimeArt +
                                  " , (select nvl((select 1 from sapprd.mara m where m.mandt = '900' and m.matnr = a.cod and m.categ_mat in ('PA','AM')),-1) palet from dual) palet, nvl(a.brgew,0) greutate " +
                                  " from sapprd.zcomdet_tableta a, sapprd.zdisc_pers_sint a1,  sintetice c," +
                                  " articole b, sapprd.zpretsubcmp s " + condTabKA + " where a.cod = b.cod(+) " + condIdKA + " and " +
                                  " a1.inactiv(+) <> 'X' and a1.functie(+)='AV' and a1.spart(+)=substr(c.COD_NIVEL1,2,2) and a1.werks(+) ='" + unitLog1 + "' " +
                                  " and b.sintetic = c.cod(+) and a1.matkl(+) = c.cod " + conditieDepart +
                                  " and s.mandt(+)='900' and s.matkl(+) = c.cod order by " + condOrderKA;


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrCmd);

                oReader1 = cmd.ExecuteReader();
                String unitLogAlt = "NN10", depart = "00", tipMat = "", lnumeArt = "", lDepoz = "";
                String blocAprov = "";

                ArticolComandaRap articol;
                if (oReader1.HasRows)
                {
                    while (oReader1.Read())
                    {
                        depart = oReader1.GetString(23).Trim() != "" ? oReader1.GetString(23) : "00";
                        tipMat = oReader1.GetString(24);
                        blocAprov = oReader1.GetString(28);

                        lnumeArt = oReader1.GetString(2).Replace("#", "");
                        lDepoz = oReader1.GetString(4);


                        if (tipMat.Trim() != "")
                            lnumeArt += " (" + tipMat + ")";

                        if (!blocAprov.Equals("-1"))
                            lnumeArt += " (Bloc.aprov.)";

                        if (oReader1.GetString(1).Equals("00000000"))
                            lnumeArt = "Taxa verde";

                        if (tipUser.Equals("DV") && departament.Equals("01") && Array.IndexOf(HelperComenzi.listSinteticeCant, oReader1.GetString(32)) >= 0)
                        {
                            lnumeArt += " (---ARTICOL CANT---)";
                        }

                        unitLogAlt = oReader1.GetString(22).Trim() != "" ? oReader1.GetString(22) : "NN10";

                        articol = new ArticolComandaRap();
                        articol.status = oReader1.GetString(0);
                        articol.codArticol = oReader1.GetString(1);
                        articol.numeArticol = lnumeArt;
                        articol.cantitate = oReader1.GetDouble(3);
                        articol.depozit = lDepoz;
                        articol.pretUnit = oReader1.GetDouble(5);
                        articol.um = oReader1.GetString(6);
                        articol.procent = oReader1.GetDouble(7);
                        articol.procentFact = oReader1.GetDouble(8);
                        articol.conditie = false;

                        articol.cmp = oReader1.GetString(10).Trim().Equals(".0000") ? 0.0 : getTipValoare(Double.Parse(oReader1.GetString(10)), dateLivrare.unitLog, tipUser, isClientInstPubl);

                        articol.discClient = oReader1.GetDouble(11);
                        articol.discountAg = oReader1.GetDouble(12);
                        articol.discountSd = oReader1.GetDouble(13);
                        articol.discountDv = oReader1.GetDouble(14);
                        articol.procAprob = oReader1.GetDouble(15);
                        articol.permitSubCmp = oReader1.GetString(16);
                        articol.multiplu = oReader1.GetDouble(17);
                        articol.pret = oReader1.GetDouble(18);
                        articol.infoArticol = oReader1.GetString(19);
                        articol.cantUmb = oReader1.GetDouble(20).ToString();
                        articol.Umb = oReader1.GetString(21);
                        articol.unitLogAlt = unitLogAlt;
                        articol.depart = depart;
                        articol.tipArt = tipMat;
                        articol.ponderare = Int32.Parse(oReader1.GetString(25).Trim().Equals("") ? "1" : oReader1.GetString(25));
                        articol.departSintetic = oReader1.GetString(26);
                        articol.departAprob = oReader1.GetString(27);
                        articol.istoricPret = GeneralUtils.formatIstoricPret(oReader1.GetString(29));
                        articol.vechime = oReader1.GetDouble(30).ToString();
                        articol.moneda = "RON";
                        articol.valTransport = oReader1.GetDouble(31).ToString();
                        articol.procTransport = "0";
                        articol.sintetic = oReader1.GetString(32);
                        articol.lungime = oReader1.GetDouble(33);
                        articol.filialaSite = unitLogAlt;
                        articol.dataExp = "00000000";
                        articol.umPalet = oReader1.GetInt32(34).ToString();
                        articol.greutate = oReader1.GetDouble(35).ToString();

                        //verificare factori conversie
                        double factorConversie = 1.0;
                        if (!articol.um.Equals(articol.Umb))
                        {
                            String[] convArray = getArtFactConvUM(articol.codArticol.Length == 8 ? "0000000000" + articol.codArticol : articol.codArticol, articol.um).Split('#');
                            factorConversie = Double.Parse(convArray[0]) / Double.Parse(convArray[1]);

                        }




                        //preturi medii
                        if (((tipUser.Equals("CV") || tipUser.Equals("SM")) && afisCond.Equals("1") && !articol.departSintetic.Equals("11")) || (tipUser.Equals("DV") && departament.Equals("11")))
                            {

                            cmdInner = new OracleCommand();

                            cmdInner = connection.CreateCommand();
                            cmdInner.CommandText = " select to_char(pret_med/cant, '99990.999')  , to_char(adaos_med/cant,'99990.999')  , um from sapprd.zpret_mediu_oras r where mandt = '900' and pdl =:unitLog " +
                                              " and matnr=:articol ";


                            cmdInner.Parameters.Clear();
                            cmdInner.Parameters.Add(":unitLog", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                            cmdInner.Parameters[0].Value = unitLog1.Substring(0, 2) + "1" + unitLog1.Substring(3, 1);

                            cmdInner.Parameters.Add(":articol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                            cmdInner.Parameters[1].Value = articol.codArticol.Length == 8 ? "0000000000" + articol.codArticol : articol.codArticol;

                            oReaderInner = cmdInner.ExecuteReader();

                            if (oReaderInner.HasRows)
                            {
                                oReaderInner.Read();
                                pretMediu = oReaderInner.GetString(0).Trim();
                                adaosMediu = oReaderInner.GetString(1).Trim();
                                unitMasPretMediu = oReaderInner.GetString(2);
                            }
                            else
                            {
                                pretMediu = adaosMediu = unitMasPretMediu = "0";
                            }

                            //coeficient corectie
                            cmdInner.CommandText = " select a.coef_corr from sapprd.zexc_coef_marja a, articole b where b.cod =:articol  and a.matkl = b.sintetic " +
                                              " and a.pdl =:ul  and a.functie =:functie ";

                            cmdInner.CommandType = CommandType.Text;

                            cmdInner.Parameters.Clear();
                            cmdInner.Parameters.Add(":articol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                            cmdInner.Parameters[0].Value = articol.codArticol.Length == 8 ? "0000000000" + articol.codArticol : articol.codArticol;

                            cmdInner.Parameters.Add(":ul", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                            cmdInner.Parameters[1].Value = unitLog1.Substring(0, 2) + "1" + unitLog1.Substring(3, 1);      //ul. distributie

                            cmdInner.Parameters.Add(":functie", OracleType.VarChar, 36).Direction = ParameterDirection.Input;
                            cmdInner.Parameters[2].Value = "CV";

                            oReaderInner = cmdInner.ExecuteReader();
                            oReaderInner.Read();

                            if (oReaderInner.HasRows)
                                coefCorectie = oReaderInner.GetDouble(0).ToString();
                            else
                                coefCorectie = "0";
                            //sf. coef corectie



                        }

                        articol.pretMediu = pretMediu;
                        articol.adaosMediu = adaosMediu;
                        articol.unitMasPretMediu = unitMasPretMediu;
                        articol.coefCorectie = coefCorectie;

                        if (!tipUser.Equals("DV") && !tipUser.Equals("DD"))
                            articol.listCabluri = new Cabluri05().getCabluriArticol(connection, nrCmd, articol.codArticol);
                        else
                            articol.listCabluri = new JavaScriptSerializer().Serialize(new List<CantCablu>());

                        if (!tipUser.Equals("DV") && !tipUser.Equals("DD") && isComandaACZC)
                        {
                            HelperComenzi.setLivrariArtACZC(connection, nrCmdSap, articol);
                        }

                        listArticole.Add(articol);

                        nrArt++;
                    }


                }

                if (oReaderInner != null)
                {
                    oReaderInner.Close();
                    oReaderInner.Dispose();
                }

                if (oReader1 != null)
                {
                    oReader1.Close();
                    oReader1.Dispose();
                }


               

                //articole conditii

                if (tipUser.Equals("SD") || tipUser.Equals("DV"))
                    InfoComenziSap.getDateArticole(nrCmd, dateLivrare, listArticole, tipUser);

                //antet
                string condAfis = "";
                int IdCndArt = -1;

                if (afisCond == "1")
                    condAfis = " cmdref=:idcmd ";

                if (afisCond == "2" || afisCond == "3")
                    condAfis = " cmdmodif=:idcmd ";

                cmd.CommandText = "select id, condcalit, nrfact, nvl(observatii,' ') observatii from sapprd.zcondheadtableta where " + condAfis;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrCmd);

                oReader1 = cmd.ExecuteReader();


                ConditiiHeader conditiiHeader = new ConditiiHeader();
                ConditiiArticole conditiiArticole;

                List<ConditiiArticole> listArtCond = new List<ConditiiArticole>();

                if (oReader1.HasRows)
                {
                    oReader1.Read();
                    IdCndArt = oReader1.GetInt32(0);
                    retVal += oReader1.GetFloat(1) + "#" + oReader1.GetInt32(2) + "#" + oReader1.GetString(3) + "@@";

                    conditiiHeader.id = IdCndArt;
                    conditiiHeader.conditiiCalit = oReader1.GetDouble(1);
                    conditiiHeader.nrFact = oReader1.GetInt32(2);
                    conditiiHeader.observatii = oReader1.GetString(3);

                }

                conditii.header = conditiiHeader;

                oReader1.Close();



                string condId = " ";

                if (afisCond == "1")
                    condId = " and c.cmdref = " + nrCmd;

                if (afisCond == "2" || afisCond == "3")
                    condId = " and c.cmdmodif = " + nrCmd;




                //articole conditii
                if (IdCndArt != -1)
                {
                    cmd.CommandText = " select  decode(length(a.codart),18,substr(a.codart,-8),a.codart) codart ,b.nume, a.cant," +
                                      " a.um,a.valoare, nvl(multiplu,1)  " +
                                      " from sapprd.zconddettableta a, sapprd.zcondheadtableta c,  " +
                                      " articole b where c.id = a.id and a.codart = b.cod " + condId + " order by poz ";


                    cmd.Parameters.Clear();
                    oReader1 = cmd.ExecuteReader();

                    if (oReader1.HasRows)
                    {
                        while (oReader1.Read())
                        {
                            retVal += oReader1.GetString(0) + "#" + oReader1.GetString(1) + "#" + oReader1.GetFloat(2) + "#"
                                      + oReader1.GetString(3) + "#" + oReader1.GetFloat(4) + "#"
                                      + oReader1.GetDouble(5) + "@@";

                            conditiiArticole = new ConditiiArticole();
                            conditiiArticole.cod = oReader1.GetString(0);
                            conditiiArticole.nume = oReader1.GetString(1);
                            conditiiArticole.cantitate = oReader1.GetDouble(2);
                            conditiiArticole.um = oReader1.GetString(3);
                            conditiiArticole.valoare = oReader1.GetDouble(4);
                            conditiiArticole.multiplu = oReader1.GetDouble(5);
                            listArtCond.Add(conditiiArticole);

                        }

                    }

                    oReader1.Close();
                    oReader1.Dispose();

                    conditii.articole = listArtCond;


                }


                //sf. conditii




            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " +  nrCmd + " , " + afisCond + " , " + tipUser + " , " + departament);
            }
            finally
            {
                cmd.Dispose();
                if (cmdInner != null)
                    cmdInner.Dispose();

                connection.Close();
                connection.Dispose();
            }

            if (tipUser.Equals("DV") && departament.Equals("01"))
                HelperComenzi.setMarjaCantPal(listArticole, dateLivrare);

            ArticolComandaAfis articoleComanda = new ArticolComandaAfis();

            articoleComanda.dateLivrare = dateLivrare;
            articoleComanda.articoleComanda = listArticole;
            articoleComanda.conditii = conditii;

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializedResult = serializer.Serialize(articoleComanda);

            
            return serializedResult;




        }


        //adaugare tva
        private double getTipValoare(double valoare, string unitLog, string tipUser, bool isClientInstPubl)
        {
            bool canalDistrib20 = unitLog.Substring(2, 1).Equals("2") ? true : false;

            bool canalDistrib40 = unitLog.Substring(2, 1).Equals("4") ? true : false;

            if (tipUser.Equals("DV") && ((canalDistrib20 && !isClientInstPubl) || canalDistrib40))
                    return valoare * 1.19;
            else
                return valoare;
        }


        private static bool isDV_WOOD(string codUser)
        {
            //Oana Codreanu
            if (codUser.Equals("00000474") || codUser.Equals("00000724"))
                return true;

            return false;

        }


        [WebMethod]
        public string checkCmdCond(string agCode, string depart, string filiala, string tipUser)
        {


            string retVal = "0";
            string blocLimCredit = "0", comenziConditionate = "0", cereriClp = "0", cereriDl = "0";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                if (depart.Equals("-1") || tipUser.Equals("KA") || tipUser.Equals("CV") || tipUser.Equals("WOOD")) //doar comenzile refuzate cu conditii (pt. agenti, cv si ka)
                {
                    cmd.CommandText = " select count(id) nrcmd from sapprd.zcomhead_tableta " +
                                      " where status = '2' and status_aprov='4' and cod_agent=:agent ";


                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":agent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = agCode;


                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();

                        if (oReader.GetInt32(0) > 0)
                            retVal = "1";
                        else
                            retVal = "0";

                        oReader.Close();
                        oReader.Dispose();

                    }
                    else
                    {
                        retVal = "0";
                    }


                    //comenzi blocate pt. limita de credit
                    blocLimCredit = "0";
                    cmd.CommandText = " select count(id) nrcmd from sapprd.zcomhead_tableta " +
                                      " where status != '6' and status_aprov = '10'  and cod_agent=:agent ";


                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":agent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = agCode;


                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();

                        if (oReader.GetInt32(0) > 0)
                            blocLimCredit = "1";
                        else
                            blocLimCredit = "0";

                        oReader.Close();
                        oReader.Dispose();

                    }
                    else
                    {
                        blocLimCredit = "0";
                    }

                    //sf. lim credit
                    retVal += "#" + blocLimCredit;

                }
                else //doar comenzile ce necesita aprobare (pt. sd si dv) si cele blocate pt. lim. credit
                {
                    string localDepart = depart;
                    string condHome = "";
                    string tabelaHome = "";

                    string tipAprov = "", tabDV = "", condDV = "";
                    if (tipUser == "SD")
                    {
                        string localFilGed = filiala.Substring(0, 2) + "2" + filiala.Substring(3, 1);  //cu 20

                        if (Utils.isFilialaMicaDep04(connection,filiala, depart))
                        {
                            tipAprov = " and a.ul in ('" + filiala + "','" + localFilGed + "') and (a.accept1 = 'X' or a.accept2 = 'X' or a.ac_zc = 'X') and ora_accept1 = '000000' " +
                                  " and a.status_aprov in ('1','6','21')  and " +
                                  " ( ( substr(ag.divizie,0,2) = '" + localDepart.Substring(0, 2) + "'  and a.depart = '11') or substr(a.depart,0,2) ='" + localDepart.Substring(0, 2) + "') ";
                        }
                        else {
                            tipAprov = " and a.ul in ('" + filiala + "','" + localFilGed + "') and (a.accept1 = 'X' or a.accept2 = 'X' or a.ac_zc = 'X') and ora_accept1 = '000000' " +
                                   " and a.status_aprov in ('1','6','21')  and " +
                                   " ( (ag.divizie = '" + localDepart + "'  and a.depart = '11') or a.depart ='" + localDepart + "') ";
                        }


                    }

                    if (tipUser == "DV")
                    {

                        tabDV = " , sapprd.zfil_dv v ";


                        //Cristi Matei
                        if (agCode.Equals("00010281") || agCode.Equals("00018768"))
                        {
                            localDepart = "11";
                            condHome = " and h.filiala = a.ul  ";
                            tabelaHome = ", sapprd.zfilhome h ";
                        }
                        else
                        {
                            condHome = "  ";
                            tabelaHome = "";
                        }

                        if (isDV_WOOD(agCode))
                        {
                            tipAprov = " and a.accept2 = 'X' and ora_accept2 = '000000' and a.status_aprov in ('1','6') ";
                            condDV = " and v.pernr = '" + agCode + "' and v.spart ='" + localDepart + "' and v.prctr = a.ul ";

                        }
                        else
                        {
                            tipAprov = " and decode(decode(a.accept1,'X',a.ora_accept1,'1'),'000000',1,0)=0 and ((a.accept2 = 'X' and ora_accept2 = '000000'  and a.status_aprov in ('1','6') " +
                                       " and a.aprob_cv_necesar=' ')  ";

                            if (localDepart.Equals("99"))
                            {
                                tipAprov += " ) ";
                                condDV = " and v.pernr = '" + agCode + "' and substr(v.prctr,0,2) = substr(a.ul,0,2) and substr(a.ul,3,1) != 4 ";

                            }
                            else {
                                tipAprov += " or (a.tip_pers = 'CV' and a.aprob_cv_necesar like '%" + localDepart + "%' and " +
                                            " a.status_aprov in ('1','4','6','21') and a.aprob_cv_realiz not like '%" + localDepart + "%' and " +
                                            " a.cond_cv not like '%" + localDepart + "%' ))  ";

                                condDV = " and v.pernr = '" + agCode + "' and v.spart ='" + localDepart + "' and substr(v.prctr,0,2) = substr(a.ul,0,2) and substr(a.ul,3,1) != 4 and " +
                                         " ( v.spart = substr(ag.divizie,0,2) or v.spart = a.depart or ag.divizie = '11' ) ";
                            }
                        }
                    }

                    if (tipUser == "SM")
                    {
                        string filSM = filiala.Substring(0, 2) + "2" + filiala.Substring(3, 1);  //cu 20

                        tabDV = "";
                        tipAprov = " and a.ul = '" + filSM + "' and a.accept1 = 'X' and a.ora_accept1 = '000000' and a.status_aprov in ('1','6') and a.aprob_cv_necesar = ' ' ";
                        condDV = " and depart ='" + localDepart + "' ";
                    }


                    cmd.CommandText = " select count(a.id) from sapprd.zcomhead_tableta a, agenti ag " + tabelaHome + tabDV +
                                      " where a.status in ('2','11')   " + tipAprov + condDV + condHome +
                                      " and a.cod_agent = ag.cod ";




                    cmd.Parameters.Clear();


                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();

                        if (oReader.GetInt32(0) > 0)
                            retVal = "1";
                        else
                            retVal = "0";

                    }
                    else
                    {
                        retVal = "0";
                    }

                    oReader.Close();
                    oReader.Dispose();


                    //comenzi blocate pt. limita de credit pe tot departmentul (doar SD si DV)
                    blocLimCredit = "0";

                    if (tipUser == "SD" || tipUser == "DV")
                    {

                        string condFilBloc = " ";

                        if (tipUser == "SD")
                        {
                            condFilBloc = " and a.ul = :filiala ";
                        }

                        cmd.CommandText = " select count(a.id) nrcmd from sapprd.zcomhead_tableta a " +
                                          " where a.status != '6' and a.status_aprov = '10'  " + condFilBloc + " and a.depart =:depart";


                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = depart;

                        if (tipUser == "SD")
                        {
                            cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                            cmd.Parameters[1].Value = filiala;
                        }

                        oReader = cmd.ExecuteReader();

                        if (oReader.HasRows)
                        {
                            oReader.Read();

                            if (oReader.GetInt32(0) > 0)
                                blocLimCredit = "1";
                            else
                                blocLimCredit = "0";

                            oReader.Close();
                            oReader.Dispose();

                        }
                        else
                        {
                            blocLimCredit = "0";
                        }

                    }
                    //sf. lim credit




                    //comenzi conditionate pe tot departmentul (doar SD si DV)
                    comenziConditionate = "0";

                    if (tipUser == "SD" || tipUser == "DV")
                    {

                        string condFilBloc = " ";

                        if (tipUser.Equals("SD"))
                        {
                            condFilBloc = " and a.ul = :filiala ";
                        }

                        if (tipUser.Equals("DV") || tipUser.Equals("DD"))
                        {

                            tabDV = " , sapprd.zfil_dv v ";
                            condFilBloc = " and v.pernr = '" + agCode + "' and v.spart = '" + depart + "' and v.prctr = a.ul and v.spart = a.depart ";

                        }

                        cmd.CommandText = " select count(a.id) nrcmd from sapprd.zcomhead_tableta a " + tabDV +
                                          " where a.status = '2' and a.status_aprov = '4'  " + condFilBloc + " and a.depart =:depart";


                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = depart;

                        if (tipUser == "SD")
                        {
                            cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                            cmd.Parameters[1].Value = filiala;
                        }

                        oReader = cmd.ExecuteReader();

                        if (oReader.HasRows)
                        {
                            oReader.Read();

                            if (oReader.GetInt32(0) > 0)
                                comenziConditionate = "1";
                            else
                                comenziConditionate = "0";

                            oReader.Close();
                            oReader.Dispose();

                        }
                        else
                        {
                            comenziConditionate = "0";
                        }

                    }
                    //sf. comenzi conditionate


                    //---------------------- cereri clp
                    cereriClp = "0";
                    {


                        if (tipUser == "SD" || tipUser == "SM")
                        {

                                cmd.CommandText = " select  nvl((select count(a.id) from sapprd.zclphead a   where a.status = '2' and a.status_aprov = '1' " +
                                              " and a.ul_dest =:filiala and substr(a.depart,0,2) =substr(:depart,0,2) and a.accept1 ='1' and a.fasonate !='X' and a.dl !='X' and a.mt in ('TRAP','TCLI','TFRN')),0) " +
                                              " +  " +
                                              " nvl((select count(a.id) from sapprd.zclphead a   where a.status = '2' and a.status_aprov = '1' " +
                                              " and a.ul_dest =:filiala and substr(a.depart,0,2) =substr(:depart,0,2) and a.accept1 = '2' and a.fasonate !='X' and a.dl !='X' and a.mt ='TERT' ),0) " +
                                              " + " +
                                              " nvl(( select count(a.id) from sapprd.zclphead a  " +
                                              " where a.status = '2' and a.status_aprov = '1' and a.ul =:filiala and substr(a.depart,0,2) =substr(:depart,0,2) and a.dl !='X' and a.accept1 ='X'),0) rez from dual ";


                        }

                        if (tipUser == "DV")
                        {
                            cmd.CommandText = " select count(a.id) from sapprd.zclphead a where a.status = '2' and a.status_aprov = '1' and a.accept1 = '1' " +
                                              " and (a.mt = 'TERT' or ( a.mt = 'TRAP' and a.curier = 'BV90')) and substr(a.depart,0,2) =:depart and a.dl !='X' and a.ul in (select prctr from sapprd.zfil_dv where pernr = '" + agCode + "' ) ";
                        }


                        cmd.Parameters.Clear();

                        if (tipUser == "SD" || tipUser == "DV" || tipUser == "SM")
                        {
                            cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                            cmd.Parameters[0].Value = depart;
                        }

                        if (tipUser == "SD" || tipUser == "SM")
                        {
                            cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                            cmd.Parameters[1].Value = filiala;
                        }

                        oReader = cmd.ExecuteReader();

                        if (oReader.HasRows)
                        {
                            oReader.Read();

                            if (oReader.GetInt32(0) > 0)
                                cereriClp = "1";
                            else
                                cereriClp = "0";

                            oReader.Close();
                            oReader.Dispose();

                        }
                        else
                        {
                            cereriClp = "0";
                        }

                    }//---------------------------sf. cereri clp


                    //---------------------------cereri dl
                    {

                        cmd.CommandText = " select count(a.id) from sapprd.zclphead a where a.dl ='X' and a.status = '2' and a.status_aprov = '1' and a.accept1 = 'X' " +
                                          " and a.depart =:depart and a.ul =:filiala ";

                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = depart;

                        cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                        cmd.Parameters[1].Value = filiala;

                        oReader = cmd.ExecuteReader();

                        if (oReader.HasRows)
                        {
                            oReader.Read();

                            if (oReader.GetInt32(0) > 0)
                                cereriDl = "1";
                            else
                                cereriDl = "0";

                            oReader.Close();
                            oReader.Dispose();

                        }
                        else
                        {
                            cereriDl = "0";
                        }
                    }
                    //---------------------------sf. cereri dl



                    //--------------------------- cereri comenzi retur

                    string cereriRetur = "0";

                    if (tipUser == "SD")
                    {

                        string dateInterval = DateTime.Today.AddDays(-30).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

                        cmd.CommandText = " select count(a.id) from sapprd.zreturhead a, agenti b where a.statusaprob = 1 and substr(b.divizie,0,2) =:depart and b.filiala =:filiala " +
                                          " and a.codagent = b.cod and a.datacreare >=:dataCreare ";

                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = depart.Substring(0, 2);

                        cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                        cmd.Parameters[1].Value = filiala;

                        cmd.Parameters.Add(":dataCreare", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                        cmd.Parameters[2].Value = dateInterval;

                        oReader = cmd.ExecuteReader();

                        if (oReader.HasRows)
                        {
                            oReader.Read();

                            if (oReader.GetInt32(0) > 0)
                                cereriRetur = "1";
                            else
                                cereriRetur = "0";

                        }
                        else
                        {
                            cereriRetur = "0";
                        }

                        oReader.Close();
                        oReader.Dispose();
                    }

                    //-------------------------------sf. comenzi retur



                    retVal += "#" + blocLimCredit + "#" + comenziConditionate + "#" + cereriClp + "#" + cereriDl + "#" + cereriRetur;


                }



            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + ", " + cmd.CommandText + " cod: " + agCode);
                retVal = "0";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }

            return retVal;
        }




        [WebMethod]
        public int aprobCmdKA(string nrCmd, string codUser)
        {
            int retVal = 0;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null, oReader1 = null;
            string parentID = "-1";

            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select count(*), nvl(parent_id,-1) from sapprd.zcomhead_tableta where id=:cmdId and nvl(parent_id,-1) <> -1 group by parent_id";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":cmdId", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrCmd;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    parentID = oReader.GetInt32(1).ToString();

                    if (oReader.GetInt32(0) > 0) //este comanda KA
                    {

                        cmd.CommandText = " select a.tip, nvl(b.av,' '), nvl(b.inactiv,' ') from agenti a,sapprd.zsuperav b  where a.cod =:codUser and b.av(+) = a.cod ";

                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":codUser", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = codUser;

                        oReader = cmd.ExecuteReader();
                        string tipModif = "";

                        if (oReader.HasRows)
                        {
                            oReader.Read();
                            if (oReader.GetString(0).Equals("SD") || (oReader.GetString(1).Trim().Length > 0 && oReader.GetString(2) != "X"))
                            {
                                tipModif = "ora_accept1";

                                //preparare date alerta mail DV
                                cmd.CommandText = " select a.ul, a.valoare, b.nume nume_ag,a.depart, " +
                                  " c.nume nume_cl, a.accept2 from sapprd.zcomhead_tableta a, agenti b, clienti c  where a.id =:idCmd " +
                                  " and a.status = '2' and a.status_aprov = '1' and b.cod = a.cod_agent and c.cod = a.cod_client";

                                cmd.Parameters.Clear();

                                cmd.Parameters.Add(":idCmd", OracleType.VarChar, 11).Direction = ParameterDirection.Input;
                                cmd.Parameters[0].Value = nrCmd;

                                oReader1 = cmd.ExecuteReader();
                                string unitLog = "", valCmd = "", numeAg = "", depart = "", numeCl = "";

                                if (oReader1.HasRows)
                                {
                                    oReader1.Read();

                                    unitLog = oReader1.GetString(0);
                                    valCmd = oReader1.GetDouble(1).ToString();
                                    numeAg = oReader1.GetString(2);
                                    depart = oReader1.GetString(3);
                                    numeCl = oReader1.GetString(4);

                                    if (oReader1.GetString(5).Equals("X")) //este nevoie de aprobarea DV
                                        sendMailAlert(unitLog, depart, "2", numeAg, numeCl, valCmd);


                                }

                                oReader1.Close();
                                oReader1.Dispose();

                                //sf. preparare

                            }
                            if (oReader.GetString(0).Equals("DV") || oReader.GetString(0).Equals("DD"))
                            {
                                tipModif = "ora_accept2";
                            }


                            //modificare ora aprobare
                            if (!tipModif.Equals(""))
                            {
                                DateTime cDate = DateTime.Now;
                                string year = cDate.Year.ToString();
                                string day = cDate.Day.ToString("00");
                                string month = cDate.Month.ToString("00");
                                string nowDate = year + month + day;
                                string nowTime = cDate.Hour.ToString("00") + cDate.Minute.ToString("00") + cDate.Second.ToString("00");

                                cmd.CommandText = " update sapprd.zcomhead_tableta set " + tipModif + " = '" + nowTime + "' where id = " + nrCmd;
                                cmd.Parameters.Clear();
                                cmd.ExecuteNonQuery();


                                //verificare comenzi ramase de aprobat
                                cmd.CommandText = " select id from sapprd.zcomhead_tableta where parent_id =:parentId  and (accept1 = 'X' or accept2 = 'X')  " +
                                                  " minus " +
                                                  " select id from sapprd.zcomhead_tableta where parent_id =:parentId and " +
                                                  " accept1 = 'X' and ora_accept1 != '000000' and accept2 != 'X' " +
                                                  " minus " +
                                                  " select id from sapprd.zcomhead_tableta where parent_id =:parentId and " +
                                                  " accept2 = 'X' and ora_accept2 != '000000' and accept1 != 'X' " +
                                                  " minus " +
                                                  " select id from sapprd.zcomhead_tableta where parent_id =:parentId and " +
                                                  " accept1 = 'X' and ora_accept1 != '000000' and accept2 = 'X' and ora_accept2 != '000000' ";


                                cmd.Parameters.Clear();
                                cmd.Parameters.Add(":parentId", OracleType.Number, 11).Direction = ParameterDirection.Input;
                                cmd.Parameters[0].Value = parentID;

                                oReader = cmd.ExecuteReader();

                                if (oReader.HasRows)
                                {
                                    //mai sunt comenzi de aprobat
                                    retVal = 2;
                                    globalParrentId = "0";
                                }
                                else
                                {
                                    //au fost aprobate toate comenzile
                                    retVal = 1;
                                    globalParrentId = parentID;
                                }

                            }//sf. modificare ora

                        }//sf. has rows

                    }

                    oReader.Close();
                    oReader.Dispose();
                }
                else
                {
                    retVal = 1;
                }

            }
            catch (Exception ex)
            {
                retVal = -1;
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }



            return retVal;
        }


        //aprobarile necesare pentru comenzile CV
        [WebMethod]
        public int verificaAprobareDV(string nrCmd, string codUser, string tipOp, string codRespingere)
        {
            int retVal = 0;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;





            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string tipOperatie = "";

                //aprobare
                if (tipOp.Equals("0"))
                    tipOperatie = "true";

                //respingere
                if (tipOp.Equals("1"))
                    tipOperatie = "false";


                cmd.CommandText = " update sapprd.zcomhead_tableta set aprob_cv_realiz =  " +
                                  " (select substr(divizie,0,2) from agenti where cod =:codUser and activ = 1)||':" + tipOperatie + ";' || aprob_cv_realiz where id =:idCmd ";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idCmd", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrCmd;

                cmd.Parameters.Add(":codUser", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codUser;


                cmd.ExecuteNonQuery();


                //verificare stare aprobari
                cmd.CommandText = " select  a.aprob_cv_necesar, a.aprob_cv_realiz, b.divizie from sapprd.zcomhead_tableta a, agenti b " +
                                  " where a.id =:idCmd ";




                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idCmd", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrCmd;

                oReader = cmd.ExecuteReader();
                oReader.Read();

                string[] aprobariNecesar = oReader.GetString(0).Trim().Split(',');
                string[] aprobariRealizat = oReader.GetString(1).Trim().Split(';');
                string[] stareAprobare;

                oReader.Close();
                oReader.Dispose();

                int nrAprobariNecesare = aprobariNecesar.Length;
                Boolean comandaOK = true;

                for (int i = 0; i < aprobariNecesar.Length; i++)
                {

                    for (int j = 0; j < aprobariRealizat.Length; j++)
                    {
                        if (aprobariRealizat[j].Contains(aprobariNecesar[i].Trim()))
                        {
                            //stareAprobare = aprobariRealizat[i].Split(':');
                            stareAprobare = aprobariRealizat[j].Split(':');
                            if (stareAprobare[1].Equals("false"))
                                comandaOK = false;

                            nrAprobariNecesare--;
                            break;


                        }
                    }

                }

                retVal = 0;
                if (nrAprobariNecesare == 0 && comandaOK)
                {
                    cmd.CommandText = " update sapprd.zcomhead_tableta set ora_accept2 = (select to_char(systimestamp, 'hh24mmss') from dual) " +
                                      " where id =:idCmd ";

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idCmd", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = nrCmd;
                    cmd.ExecuteNonQuery();

                    retVal = 1;
                }


                //este necesara o singura aprobare si cmd este respinsa
                if (aprobariNecesar.Length == 1 && tipOp.Equals("1"))
                {
                    OperatiiComenzi.respingeComanda(connection, nrCmd, codUser, codRespingere);
                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = -1;
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }

            return retVal;
        }


        private int verificaAprobareSD(string nrCmd, string codUser)
        {
            int retVal = 0;


            bool accept1 = false, accept2 = false, oraAccept1 = false, oraAccept2 = false;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {

                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select a.accept1, a.accept2, a.ora_accept1, a.ora_accept2, a.ul, a.valoare, b.nume nume_ag,a.depart, " +
                                  " nvl(c.nume,'-') nume_cl from sapprd.zcomhead_tableta a, agenti b, clienti c  where a.id =:idCmd " +
                                  " and a.status = '2' and a.status_aprov in ('1','21')  and b.cod = a.cod_agent and c.cod(+) = a.cod_client";


                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idCmd", OracleType.VarChar, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrCmd;

                oReader = cmd.ExecuteReader();
                string unitLog = "", valCmd = "", numeAg = "", depart = "", numeCl = "";

                if (oReader.HasRows)
                {
                    oReader.Read();


                    accept1 = false; accept2 = false; oraAccept1 = false; oraAccept2 = false;


                    if (oReader.GetString(0).Trim().Equals("X"))
                        accept1 = true;

                    if (oReader.GetString(1).Trim().Equals("X"))
                        accept2 = true;

                    if (!oReader.GetString(2).Trim().Equals("000000"))
                        oraAccept1 = true;

                    if (!oReader.GetString(3).Trim().Equals("000000"))
                        oraAccept2 = true;

                    unitLog = oReader.GetString(4);
                    valCmd = oReader.GetDouble(5).ToString();
                    numeAg = oReader.GetString(6);
                    depart = oReader.GetString(7);
                    numeCl = oReader.GetString(8);


                    if (accept1 || accept2)
                    {


                        cmd.CommandText = " select a.tip, nvl(b.av,' '), nvl(b.inactiv,' ') from agenti a,sapprd.zsuperav b  where a.cod =:codUser and b.av(+) = a.cod ";

                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":codUser", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = codUser;

                        oReader = cmd.ExecuteReader();
                        string tipModif = "";

                        if (oReader.HasRows)
                        {
                            oReader.Read();
                            if (oReader.GetString(0).Equals("SD") || oReader.GetString(0).Equals("SM") || (oReader.GetString(1).Trim().Length > 0 && oReader.GetString(2) != "X"))
                            {
                                if (!oraAccept1)
                                    tipModif = "ora_accept1";
                            }
                            if (oReader.GetString(0).Equals("DV") || oReader.GetString(0).Equals("DD"))
                            {
                                if (!oraAccept2)
                                    tipModif = "ora_accept2";
                            }




                            //modificare ora aprobare
                            if (!tipModif.Equals(""))
                            {
                                DateTime cDate = DateTime.Now;
                                string year = cDate.Year.ToString();
                                string day = cDate.Day.ToString("00");
                                string month = cDate.Month.ToString("00");
                                string nowDate = year + month + day;
                                string nowTime = cDate.Hour.ToString("00") + cDate.Minute.ToString("00") + cDate.Second.ToString("00");

                                cmd.CommandText = " update sapprd.zcomhead_tableta set " + tipModif + " = '" + nowTime + "' where id = " + nrCmd;
                                cmd.Parameters.Clear();
                                cmd.ExecuteNonQuery();

                                retVal = 2;
                            }

                            if (tipModif.Equals("ora_accept1"))
                            {
                                oraAccept1 = true;

                                //trimitere alerta mail catre DV (daca e cazul)
                                if (accept2)
                                    sendMailAlert(unitLog, depart, "2", numeAg, numeCl, valCmd);
                            }

                            if (tipModif.Equals("ora_accept2"))
                                oraAccept2 = true;

                            //doar aprobare sd
                            if (accept1 && !accept2)
                            {
                                if (oraAccept1)
                                    retVal = 1;
                            }

                            //doar aprobare dv
                            if (!accept1 && accept2)
                            {
                                if (oraAccept2)
                                    retVal = 1;
                            }


                            if (accept1 && accept2)
                            {
                                if (oraAccept1 && oraAccept2)
                                    retVal = 1;
                            }



                        }


                    }

                    if (accept1 && accept2 && oraAccept1 && oraAccept2)
                    {
                        retVal = 1;
                    }

                }//if has roww
                else //este comanda KA
                {
                    retVal = 1;
                }


                oReader.Close();
                oReader.Dispose();


            }
            catch (Exception ex)
            {
                retVal = -1;
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }




            return retVal;
        }


        private int verificaDepartExceptie(string nrCmd, string codUser)
        {
            int retVal = 0;
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string departExc = "00";
            try
            {
                //departament comanda

                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                cmd.CommandText = " select depart from sapprd.zcomhead_tableta where id =:idCmd ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idCmd", OracleType.Number, 1).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrCmd;

                oReader = cmd.ExecuteReader();
                departExc = "00";
                if (oReader.HasRows)
                {
                    oReader.Read();
                    departExc = oReader.GetString(0);
                }
                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();
                connection.Close();
                connection.Dispose();

                //sf. departament

                if (departExc.Equals("01") || departExc.Equals("02"))
                {
                    retVal = verificaAprobareSD(nrCmd, codUser);
                }
                else
                {
                    retVal = aprobAdrLivrNoua(nrCmd, codUser);
                }


            }
            catch (Exception ex)
            {
                retVal = -1;
                sendErrorToMail(ex.ToString());
            }
            finally
            {

            }

            return retVal;
        }


        private int aprobAdrLivrNoua(string nrCmd, string codUser)
        {
            int retVal = 0;
            bool cmdResp = false;
            bool adrLivrNoua = false, accept1 = false, accept2 = false, oraAccept1 = false, oraAccept2 = false;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {

                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                cmd.CommandText = " select nvl(abgru,-1), nvl(adr_noua,-1), accept1, accept2, ora_accept1, ora_accept2 from sapprd.zcomhead_tableta where id =:idCmd " +
                                  " and status = '2' and status_aprov = '1' and  nvl(parent_id,-1) = -1 ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idCmd", OracleType.VarChar, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrCmd;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    adrLivrNoua = false;
                    accept1 = false; accept2 = false; oraAccept1 = false; oraAccept2 = false;

                    if (oReader.GetString(1).Trim().Equals("X"))
                        adrLivrNoua = true;

                    if (oReader.GetString(2).Trim().Equals("X"))
                        accept1 = true;

                    if (oReader.GetString(3).Trim().Equals("X"))
                        accept2 = true;

                    if (!oReader.GetString(4).Trim().Equals("000000"))
                        oraAccept1 = true;

                    if (!oReader.GetString(5).Trim().Equals("000000"))
                        oraAccept2 = true;

                    if (oReader.GetString(0).Trim() != "-1")
                        cmdResp = true;
                    else
                        cmdResp = false;

                }


                if (!cmdResp)
                {
                    if (adrLivrNoua)
                    {
                        if (accept1 && accept2 && oraAccept1 && oraAccept2)   //se creaza comanda
                        {
                            retVal = 1;
                        }
                        else
                        {
                            if (accept1 || accept2)
                            {
                                cmd.CommandText = " select tip from agenti where cod =:codUser ";

                                cmd.Parameters.Clear();

                                cmd.Parameters.Add(":codUser", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                                cmd.Parameters[0].Value = codUser;

                                oReader = cmd.ExecuteReader();
                                string tipModif = "";

                                if (oReader.HasRows)
                                {
                                    oReader.Read();
                                    if (oReader.GetString(0).Equals("SD"))
                                    {
                                        if (!oraAccept1)
                                            tipModif = "ora_accept1";
                                    }
                                    if (oReader.GetString(0).Equals("DV") || oReader.GetString(0).Equals("DD"))
                                    {
                                        if (!oraAccept2)
                                            tipModif = "ora_accept2";
                                    }


                                    //modificare ora aprobare
                                    if (!tipModif.Equals(""))
                                    {
                                        DateTime cDate = DateTime.Now;
                                        string year = cDate.Year.ToString();
                                        string day = cDate.Day.ToString("00");
                                        string month = cDate.Month.ToString("00");
                                        string nowDate = year + month + day;
                                        string nowTime = cDate.Hour.ToString("00") + cDate.Minute.ToString("00") + cDate.Second.ToString("00");

                                        cmd.CommandText = " update sapprd.zcomhead_tableta set " + tipModif + " = '" + nowTime + "' where id = " + nrCmd;
                                        cmd.Parameters.Clear();
                                        cmd.ExecuteNonQuery();

                                        retVal = 2;
                                    }

                                    if (tipModif.Equals("ora_accept1"))
                                        oraAccept1 = true;

                                    if (tipModif.Equals("ora_accept2"))
                                        oraAccept2 = true;


                                    if (accept1 && !accept2)
                                    {
                                        if (oraAccept1)
                                            retVal = 1;
                                    }

                                    if (accept1 && accept2)
                                    {
                                        if (oraAccept1 && oraAccept2)
                                            retVal = 1;
                                    }



                                }

                            }//if
                        }//esle

                    }
                    else       //nu are adresa de livrare noua
                    {
                        retVal = 1;
                    }

                }


                oReader.Close();
                oReader.Dispose();


            }
            catch (Exception ex)
            {
                retVal = -1;
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }

            return retVal;

        }


        private int aprobaComandaWood(string nrCmd)
        {
            int retVal = 0;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();


            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " update sapprd.zcomhead_tableta set ora_accept2 = (select to_char(systimestamp, 'hh24mmss') from dual) " +
                                  " where id =:idCmd ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idCmd", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrCmd;
                cmd.ExecuteNonQuery();

                retVal = 1;

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = -1;
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }

            return retVal;
        }



        private bool isFilialaWood(string filiala)
        {
            return filiala != null && filiala.Substring(2, 1).Equals("4");
        }


        [WebMethod]
        public string operatiiComenzi(string nrCmd, string nrCmdSAP, string tipOp, string codUser, string codRespingere, string divizieAgent, string elimTransp, string filiala, string codStare, bool isComandaACZC)
        {
            string retVal = "-1";

            try
            {

                if (tipOp.Equals("0")) //aprobare cmd
                {

                    try
                    {
                        new JurnalAprobari().salveazaAprobare(nrCmd, codUser, "A");

                        int verificaAprobariNecesare = 0;

                        if (divizieAgent.Equals("11"))
                        {
                            if (isFilialaWood(filiala))
                                verificaAprobariNecesare = aprobaComandaWood(nrCmd);
                            else
                                verificaAprobariNecesare = verificaAprobareDV(nrCmd, codUser, tipOp, codRespingere);
                        }
                        else
                        {
                            verificaAprobariNecesare = verificaAprobareSD(nrCmd, codUser);
                        }

                        if (verificaAprobariNecesare == 1)
                        {
                            SAPWebServices.ZTBL_WEBSERVICE webService = null;
                            webService = new ZTBL_WEBSERVICE();

                            string conditieTransp = " ";

                            if (elimTransp == null)
                            {
                                conditieTransp = " ";
                            }
                            else
                            {
                                conditieTransp = elimTransp;
                            }

                            //comenzile cu sau fara rezervare de stoc nu se creaza automat dupa toate aprobarile primite
                            if (codStare != null && !codStare.Equals("21") && !codStare.Equals("20"))
                            {

                                SAPWebServices.ZstareComanda inParam = new SAPWebServices.ZstareComanda();
                                string idCmd = nrCmd;
                                if (globalParrentId != "0")
                                    idCmd = globalParrentId;

                                inParam.NrCom = idCmd;
                                inParam.Stare = "2";
                                inParam.PernrCh = codUser;
                                inParam.FaraTrap = conditieTransp;

                                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                                webService.Credentials = nc;
                                webService.Timeout = 300000;

                                SAPWebServices.ZstareComandaResponse outParam = webService.ZstareComanda(inParam);


                                string response = outParam.VOk;
                                if (response.Equals("0"))
                                {
                                    retVal = "Comanda aprobata";
                                    new Mail().sendMailComenziOpFacturare(idCmd);
                                }
                                else
                                {
                                    retVal = outParam.VMess;
                                }
                            }
                            else
                            {
                                retVal = "Comanda aprobata";
                            }

                            webService.Dispose();

                            globalParrentId = "0";

                        }

                        if (verificaAprobariNecesare == 2)
                        {
                            retVal = "Comanda aprobata";
                        }

                        if (codStare != null && codStare.Equals("21"))
                        {
                            retVal = "Operatie efectuata";
                        }


                    }
                    catch (Exception ex)
                    {
                        retVal = "Operatie esuata.";
                        sendErrorToMail(ex.ToString() + " , idcmd = " + nrCmd + " , nrcmdsap = " + nrCmdSAP);

                    }

                }

                if (tipOp.Equals("1")) //respingere cmd
                {
                    //doar modificare stare cmd

                    new JurnalAprobari().salveazaAprobare(nrCmd, codUser, "R");

                    if (divizieAgent.Equals("11") && !isFilialaWood(filiala))
                    {
                        verificaAprobareDV(nrCmd, codUser, tipOp, codRespingere);
                        return "0";
                    }



                    OracleConnection connection = new OracleConnection();
                    OracleCommand cmd = new OracleCommand();

                    try
                    {

                       

                        //stergere cmd
                        SAPWebServices.ZTBL_WEBSERVICE webService = null;
                        webService = new ZTBL_WEBSERVICE();

                        SAPWebServices.ZstareComanda inParam = new SAPWebServices.ZstareComanda();
                        inParam.NrCom = nrCmd;
                        inParam.Stare = "3";
                        inParam.PernrCh = codUser;

                        System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                        webService.Credentials = nc;
                        webService.Timeout = 300000;

                        SAPWebServices.ZstareComandaResponse outParam = webService.ZstareComanda(inParam);

                        string response = outParam.VOk;

                        //---motiv respingere
                        string connectionString = GetConnectionString_android();

                        connection.ConnectionString = connectionString;
                        connection.Open();

                        cmd = connection.CreateCommand();

                        cmd.CommandText = " update sapprd.zcomhead_tableta set abgru = '" + codRespingere + "' where id=:cmd ";

                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(":cmd", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = nrCmd;

                        cmd.ExecuteNonQuery();

                        cmd.Dispose();
                        connection.Close();
                        connection.Dispose();
                        //---sf. motiv respingere



                        if (response.Equals("0"))
                        {
                            retVal = "Comanda respinsa.";
                        }
                        else
                        {
                            retVal = outParam.VMess;
                        }

                        webService.Dispose();


                    }
                    catch (Exception ex)
                    {
                        retVal = "Operatie esuata.";
                        sendErrorToMail(ex.ToString() + " , idcmd = " + nrCmd + " , nrcmdsap = " + nrCmdSAP);

                    }


                }


                if (tipOp.Equals("2")) //comanda cu conditii
                {

                    OracleConnection connection = new OracleConnection();
                    OracleCommand cmd = new OracleCommand();

                    try
                    {

                        //new JurnalAprobari().salveazaAprobare(nrCmd, codUser, "C");
                        string connectionString = GetConnectionString_android();

                        connection.ConnectionString = connectionString;
                        connection.Open();

                        cmd = connection.CreateCommand();

                        string status_aprov = "4";

                        if (codStare.Equals("21"))
                            status_aprov = "41";

                        cmd.CommandText = " update sapprd.zcomhead_tableta set status_aprov = :status where id=:cmd ";

                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":status", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = status_aprov;

                        cmd.Parameters.Add(":cmd", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[1].Value = nrCmd;

                        cmd.ExecuteNonQuery();

                        //aprobari pe comenzile CV
                        if (divizieAgent.Equals("11"))
                        {

                            cmd.CommandText = " update sapprd.zcomhead_tableta set cond_cv =  " +
                                              " (select divizie from agenti where cod =:codPers and activ = 1)||';'|| cond_cv where id =:cmd ";


                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.Clear();
                            cmd.Parameters.Add(":cmd", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                            cmd.Parameters[0].Value = nrCmd;

                            cmd.Parameters.Add(":codPers", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                            cmd.Parameters[1].Value = codUser;

                            cmd.ExecuteNonQuery();

                        }

                        retVal = "Operatie reusita.";
                       

                    }
                    catch (Exception ex)
                    {
                        sendErrorToMail(ex.ToString() + " , idcmd = " + nrCmd + " , nrcmdsap = " + nrCmdSAP);
                        retVal = "Operatie esuata.";
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                        connection.Dispose();

                    }

                }



                if (tipOp.Equals("3")) //stergere cmd
                {

                    SAPWebServices.ZTBL_WEBSERVICE webService = null;
                    webService = new ZTBL_WEBSERVICE();

                    SAPWebServices.ZstareComanda inParam = new SAPWebServices.ZstareComanda();
                    inParam.NrCom = nrCmd;
                    inParam.Stare = "5";
                    inParam.PernrCh = codUser;

                    System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                    webService.Credentials = nc;
                    webService.Timeout = 300000;

                    SAPWebServices.ZstareComandaResponse outParam = webService.ZstareComanda(inParam);

                    string response = outParam.VOk;
                    if (response.Equals("0"))
                    {
                        retVal = "Comanda stearsa";
                    }
                    else
                    {
                        retVal = outParam.VMess;
                    }

                    webService.Dispose();

                }

                if (tipOp.Equals("7")) //aprobare comanda angajament
                {

                    OracleConnection connection = new OracleConnection();
                    OracleCommand cmd = new OracleCommand();



                    try
                    {
                        string connectionString = GetConnectionString_android();

                        connection.ConnectionString = connectionString;
                        connection.Open();

                        cmd = connection.CreateCommand();

                        //transformare comanda in sablon

                        DateTime cDate = DateTime.Now;
                        string year = cDate.Year.ToString();
                        string day = cDate.Day.ToString("00");
                        string month = cDate.Month.ToString("00");
                        string dataStart = day + "." + month + "." + year;

                        DateTime firstDayOfTheMonth = new DateTime(cDate.Year, cDate.Month, 1);
                        string dataStop = firstDayOfTheMonth.AddMonths(1).AddDays(-1).ToString("dd.MM.yyyy");

                        //date antet
                        string codAgent = "", codClient = "", unitLog = "", depart = "";

                        OracleDataReader oReader = null;

                        cmd.CommandText = " select a.cod_agent, a.cod_client, a.ul,  substr(b.divizie,0,2) divizie from sapprd.zcomhead_tableta a, " +
                                          " agenti b where a.id=:nrCmd and a.cod_agent = b.cod ";

                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":nrCmd", OracleType.Number, 11).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = nrCmd;

                        oReader = cmd.ExecuteReader();

                        if (oReader.HasRows)
                        {
                            oReader.Read();
                            codAgent = oReader.GetString(0);
                            codClient = oReader.GetString(1);
                            unitLog = oReader.GetString(2);
                            depart = oReader.GetString(3);

                        }
                        //sf. date antet

                        string antetRed = codClient + "#" + "B5" + "#" + "F" + "#" + codAgent + "#" + unitLog + "#" + depart +
                                        "#" + dataStart + "#" + dataStop + "##" + "-1#false#0#" + nrCmd + "@";




                        //articole
                        cmd.CommandText = " select a.cod, b.nume,a.valoare*a.cantitate, a.procent  from sapprd.zcomdet_tableta a, articole b where a.id=:nrCmd " +
                                          " and a.cod = b.cod order by a.poz";

                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":nrCmd", OracleType.Number, 11).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = nrCmd;

                        oReader = cmd.ExecuteReader();
                        string articoleRed = "";
                        if (oReader.HasRows)
                        {
                            while (oReader.Read())
                            {
                                articoleRed += "2" + "#" + oReader.GetString(0) + "#" + oReader.GetString(1) + "#" +
                                                oReader.GetDouble(2) + "#" + "RON" + "#" + oReader.GetDouble(3) + "# # @";
                            }
                        }


                        //sf. articole

                        string dataRed = antetRed + articoleRed;
                        retVal = saveRedAndroid(dataRed);

                        //update
                        cmd.CommandText = " update sapprd.zcomhead_tableta set status_aprov = '7', patt_id = '" + retVal + "' where id=:cmd ";




                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(":cmd", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = nrCmd;

                        cmd.ExecuteNonQuery();


                        //sf. transformare



                    }
                    catch (Exception ex)
                    {
                        retVal = "Operatie esuata.";
                        sendErrorToMail(ex.ToString() + " , idcmd = " + nrCmd + " , nrcmdsap = " + nrCmdSAP);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                        connection.Dispose();

                    }

                }

                if (tipOp.Equals("8")) //respingere comanda angajament
                {

                    OracleConnection connection = new OracleConnection();
                    OracleCommand cmd = new OracleCommand();

                    try
                    {
                        string connectionString = GetConnectionString_android();

                        connection.ConnectionString = connectionString;
                        connection.Open();

                        cmd = connection.CreateCommand();

                        cmd.CommandText = " update sapprd.zcomhead_tableta set status_aprov = '8' where id=:cmd ";

                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(":cmd", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = nrCmd;

                        cmd.ExecuteNonQuery();
                        retVal = "Operatie reusita.";

                    }
                    catch (Exception ex)
                    {
                        sendErrorToMail(ex.ToString() + " , idcmd = " + nrCmd + " , nrcmdsap = " + nrCmdSAP);
                        retVal = "Operatie esuata.";
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                        connection.Dispose();
                    }
                }

                if (tipOp.Equals("9")) //transformare cmd ged simulata cu rezervare de stoc in comanda ferma
                {

                    OracleConnection connection = new OracleConnection();
                    OracleCommand cmd = new OracleCommand();

                    try
                    {
                        string connectionString = GetConnectionString_android();

                        connection.ConnectionString = connectionString;
                        connection.Open();

                        cmd = connection.CreateCommand();

                        //verificare necesitate aprobare sm
                        cmd.CommandText = " select accept1 from sapprd.zcomhead_tableta where id=:cmd ";

                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(":cmd", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = nrCmd;

                        OracleDataReader oReader = null;
                        oReader = cmd.ExecuteReader();

                        if (oReader.HasRows)
                        {
                            oReader.Read();

                            // este nevoie de aprobare SM, se schimba doar statusul
                            if (oReader.GetString(0).Equals("X"))
                            {
                                cmd.CommandText = " update sapprd.zcomhead_tableta set status_aprov = '1' where id=:cmd ";

                                cmd.CommandType = CommandType.Text;

                                cmd.Parameters.Clear();
                                cmd.Parameters.Add(":cmd", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                                cmd.Parameters[0].Value = nrCmd;

                                cmd.ExecuteNonQuery();

                                retVal = "Operatie reusita.";
                            }
                            //se transforma direct in comanda
                            else
                            {
                                SAPWebServices.ZTBL_WEBSERVICE webService = null;
                                webService = new ZTBL_WEBSERVICE();

                                SAPWebServices.ZstareComanda inParam = new SAPWebServices.ZstareComanda();

                                inParam.NrCom = nrCmd;
                                inParam.Stare = "2";
                                inParam.PernrCh = codUser;

                                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                                webService.Credentials = nc;
                                webService.Timeout = 300000;

                                SAPWebServices.ZstareComandaResponse outParam = webService.ZstareComanda(inParam);


                                string response = outParam.VOk;
                                if (response.Equals("0"))
                                {
                                    retVal = "Comanda creata";
                                }
                                else
                                {
                                    retVal = outParam.VMess;
                                }

                                webService.Dispose();
                            }




                        }
                        else
                        {
                            retVal = "Operatie esuata.";
                        }

                        if (oReader != null)
                        {
                            oReader.Close();
                            oReader.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        sendErrorToMail(ex.ToString() + " , idcmd = " + nrCmd + " , nrcmdsap = " + nrCmdSAP);
                        retVal = "Operatie esuata.";
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                        connection.Dispose();
                    }
                }

                //comanda simulata fara rezervare de stoc, cu sau fara conditii
                if (tipOp.Equals("10") || tipOp.Equals("41"))
                {

                    SAPWebServices.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE();

                    string conditieTransp = " ";

                    if (elimTransp == null)
                    {
                        conditieTransp = " ";
                    }
                    else
                    {
                        conditieTransp = elimTransp;
                    }


                    SAPWebServices.ZstareComanda inParam = new SAPWebServices.ZstareComanda();
                    string idCmd = nrCmd;
                    if (globalParrentId != "0")
                        idCmd = globalParrentId;

                    inParam.NrCom = idCmd;
                    inParam.Stare = "2";
                    inParam.PernrCh = codUser;
                    inParam.FaraTrap = conditieTransp;

                    System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                    webService.Credentials = nc;
                    webService.Timeout = 300000;

                    SAPWebServices.ZstareComandaResponse outParam = webService.ZstareComanda(inParam);



                    string response = outParam.VOk;
                    if (response.Equals("0"))
                    {
                        retVal = "Comanda creata";
                    }
                    else
                    {
                        retVal = outParam.VMess;
                    }
                }


            }
            catch (Exception ex)
            {
                retVal = "-1";

                String vals = nrCmd + " \n\n " + nrCmdSAP + " \n\n " + tipOp + " \n\n " + codUser + " \n\n " + codRespingere + " \n\n " + divizieAgent + " \n\n " + elimTransp + " \n\n " + filiala + " \n\n " + codStare;

                sendErrorToMail(ex.ToString() + " Nr. cmd: " + nrCmd + " vals: " + vals);


            }

            finally
            {


            }


            return retVal;
        }







        [WebMethod]
        public string getGradSabloane(string codUser, string filiala, string depart, string tipUser)
        {
            //grad incarcare sabloane
            string retVal = "";
            string sablon = "", cod = " ", nume = " ";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null, oReader1 = null;
            string cant_r = "", um_r = "", val_r = "", mon_r = "";


            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                //lista comenzi
                string condSablon = "", tabClienti = "", condClienti = "", tipSablon = "", condClienti1 = ""; ;

                if (tipUser == "AV") //agent, doar clientii lui
                {
                    tabClienti = ", sapprd.knvp p";
                    condClienti = " and p.mandt = '900' and  p.vtweg = '10' and a.spart = '" + depart + "' " +
                                   " and p.parvw = 'VE' and p.pernr = '" + codUser + "' and p.kunnr = c.cod  ";
                    tipSablon = " and b.so_name = 'SB5KUNNR'  ";
                    condClienti1 = " and c.cod = b.so_low ";
                }
                else    //sabloanele proprii
                {
                    condSablon = " and a.pattern_desc = '" + codUser + "' ";
                    tipSablon = " and (b.so_name = 'SB5KUNNR' or b.so_name = 'SB5KDGRP') ";
                    condClienti1 = " and c.cod(+) = b.so_low ";
                }

                cmd.CommandText = " select distinct a.pattern_id, to_char(to_date(a.valid_from,'yyyymmdd')) dstart, " +
                    "   to_char(to_date(a.valid_to,'yyyymmdd')) dstop,  decode(c.nume,'',' ',c.nume) numecl, b1.nume, b.so_low cod from " +
                    "   sapprd.ZSD_RU_PATTERN a, sapprd.ZSD_RU_PATT_SO b, clienti c,agenti b1 " + tabClienti +
                    "   where a.pattern_id = b.pattern_id " + condSablon + condClienti + tipSablon + condClienti1 +
                    "   and nvl(a.deleted,-1)<>'X' and a.used <> 'X' and b1.cod = a.pattern_desc  order by numecl asc, a.pattern_id desc";



                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        //date antet
                        retVal += oReader.GetString(0) + "#" + oReader.GetString(1) + "#" + oReader.GetString(2) + "#" +
                                    oReader.GetString(3) + "#" + oReader.GetString(4) + "#" + oReader.GetString(5) + "@@";

                        sablon = oReader.GetString(0);

                        //articole sabloane
                        //articole/sintetice + valori realizate

                        cmd.CommandText = " select decode(a.main_matkl,' ','-1',a.main_matkl) codsint , " +
                            " decode(a.desc_main_matkl,' ','-1',a.desc_main_matkl) numesint, " +
                            " decode(length(a.main_matnr),18,substr(a.main_matnr,-8),decode(a.main_matnr,' ','-1',a.main_matnr)) codart, " +
                            " decode(a.desc_main_matnr,' ','-1',a.desc_main_matnr) numeart, " +
                            " a.fkimg cant, a.vrkme um, a.netwr val, a.waerk mon, " +
                            " a.reducere, nvl(b.FKIMG_R,-1) cant_r,  nvl(b.VRKME_R,-1) um_r , nvl(b.NETWR_R,-1) val_r, " +
                            " nvl(b.WAERK_R,-1) mon_r, " +
                            " decode(a.codnivel,' ','-1',a.codnivel) codnivel , " +
                            " decode(a.desc_nivel,' ','-1',a.desc_nivel) descnivel, a.counter_pk  " +
                            " from sapprd.zsd_ru_b_pattobj a, sapprd.ZRED_GRAD_REALIZ b  where a.mandt = '900'   " +
                            " and b.mandt(+)='900' and b.pattern_id(+) = a.pattern_id and b.counter_pk(+) = a.counter_pk and " +
                            " a.pattern_id=:idsabl order by a.counter_pk ";


                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(":idsabl", OracleType.NVarChar, 42).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = sablon;



                        oReader1 = cmd.ExecuteReader();

                        if (oReader1.HasRows)
                        {
                            while (oReader1.Read())
                            {

                                if (oReader1.GetDouble(9) == -1) //nu exista realizari
                                {
                                    cant_r = "-2";
                                    um_r = "-2";
                                    val_r = "-2";
                                    mon_r = "-2";
                                }
                                else
                                {
                                    if (oReader1.GetDouble(9) == 0) //pragul este valoric
                                    {
                                        cant_r = "-1";
                                        um_r = "-1";
                                        val_r = oReader1.GetDouble(11).ToString();
                                        mon_r = oReader1.GetString(12);
                                    }
                                    else //pragul este cantitativ
                                    {
                                        cant_r = oReader1.GetDouble(9).ToString();
                                        um_r = oReader1.GetString(10);
                                        val_r = "-1";
                                        mon_r = "-1";
                                    }
                                }

                                retVal += oReader1.GetString(0) + "#" + oReader1.GetString(1) + "#" + oReader1.GetString(2) + "#"
                                    + oReader1.GetString(3) + "#" + oReader1.GetDouble(4) + "#" + oReader1.GetString(5) + "#"
                                    + oReader1.GetDouble(6) + "#" + oReader1.GetString(7) + "#" + oReader1.GetDouble(8) + "#"
                                    + cant_r + "#" + um_r + "#" + val_r + "#" + mon_r + "#" + oReader1.GetString(13) + "#"
                                    + oReader1.GetString(14) + "#" + oReader1.GetInt32(15) + "@@";
                            }


                        }

                        oReader1.Close();
                        oReader1.Dispose();

                        //sf. articole

                        //exceptii

                        cmd.CommandText = " select " +
                                           " decode(main_matkl,' ','-1',main_matkl) codsint, " +
                                           " decode(desc_main_matkl,' ','-1',desc_main_matkl) numesint, " +
                                           " decode(length(main_matnr),18,substr(main_matnr,-8),decode(main_matnr,' ','-1',main_matnr)) codart, " +
                                           " decode(desc_main_matnr,' ','-1',desc_main_matnr) numeart, " +
                                           " decode(codnivel,' ','-1',codnivel) codnivel , " +
                                           " decode(desc_nivel,' ','-1',desc_nivel) descnivel  " +
                                           " from sapprd.zsd_ru_b_pattobe where mandt = '900' and  " +
                                           " pattern_id=:idsabl ";

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(":idsabl", OracleType.NVarChar, 42).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = sablon;


                        oReader1 = cmd.ExecuteReader();

                        if (oReader1.HasRows)
                        {
                            while (oReader1.Read())
                            {
                                if (oReader1.GetString(0).Equals("-1"))
                                {
                                    if (oReader1.GetString(2).Equals("-1"))
                                    {
                                        cod = oReader1.GetString(4);
                                        nume = oReader1.GetString(5);
                                    }
                                    else
                                    {
                                        cod = oReader1.GetString(2);
                                        nume = oReader1.GetString(3);
                                    }

                                }
                                else
                                {
                                    cod = oReader1.GetString(0);
                                    nume = oReader1.GetString(1);
                                }

                                retVal += "-2" + "#" + cod + "#" + nume + "@@";
                            }


                        }

                        oReader1.Close();
                        oReader1.Dispose();

                        //sf. exceptii


                        //sf. art. sabloane

                        retVal += "!!";

                    }


                    oReader.Close();
                    oReader.Dispose();


                }
                else
                {
                    retVal = "-1";
                }
            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + "cod user: " + codUser);
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }


            return retVal;
        }


        [WebMethod]//Android
        public string trimiteSablonAprob(string codSablon, string pozitie, string codClient)
        {
            string retVal = "", query = "";


            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                //verificare existenta inregistrare
                cmd.CommandText = " select count(patt_id) from sapprd.ZSTARE_PATT where mandt='900' " +
                                  " and patt_id = '" + codSablon + "' and kunnr = '" + codClient + "' " +
                                  " and counter_pk = " + pozitie;



                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    if (oReader.GetInt32(0) == 0)
                    {
                        //inserare sablon
                        query = " insert into sapprd.zstare_patt(mandt,patt_id,kunnr,stat,counter_pk) " +
                                " values ('900','" + codSablon + "','" + codClient + "','1'," + pozitie + ")";


                        cmd.CommandText = query;
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Clear();
                        cmd.ExecuteNonQuery();


                    }

                }

                oReader.Close();
                oReader.Dispose();

                retVal = "1";

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + "query: " + query);
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }



            return retVal;

        }

        [WebMethod]
        public string sendSms(string nrTel, string msgText)
        {
            String response = "";
            System.Net.ServicePointManager.Expect100Continue = false;
            SMSService.SMSServiceService smsService = new SMSService.SMSServiceService();

            try
            {
                response = smsService.sendSmsAuthKey("arabesque2", "506fceb13086d332baebe16cc398fbaff295dced", "ARABESQUE", nrTel, msgText, new DateTime(), 0, "");
                Sms.logSms(nrTel, msgText, response);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                
            }



            return response;

        }


        [WebMethod]
        public string sendSmsFlota(string nrTel, string msgText)
        {
            String response = "";
            System.Net.ServicePointManager.Expect100Continue = false;
            SMSService.SMSServiceService smsService = new SMSService.SMSServiceService();

            try
            {
                response = smsService.sendSmsAuthKey("arabesque", "6a35f3f7eee22cbf56555937f639291c8ff10a23", "ARABESQUE", nrTel, msgText, new DateTime(), 0, "");
                Sms.logSms(nrTel, msgText, response);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
            }



            return response;

        }


        [WebMethod]//Android
        public string stergeSablonReduceri(string codSablon)
        {
            string retVal = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();

            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " update sapprd.ZSD_RU_PATTERN  set deleted = 'X' where pattern_id = '" + codSablon + "' ";

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + "cod sablon: " + codSablon);
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }

            return retVal;

        }




        [WebMethod]//Android
        public string getListSabloaneReduceri(string codUser, string filiala, string depart)
        {
            string retVal = "";



            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;



            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                //lista comenzi
                string condSablon = "";

                if (condSablon == "0") //toti agentii din depart
                {
                    condSablon = " and a.pattern_desc in ( select distinct cod from agenti where filiala = '" + filiala + "' and " +
                    " substr(divizie,0,2) = '" + depart + "' and activ = 1 )";
                }
                else
                {
                    condSablon = " and a.pattern_desc = '" + codUser + "' ";
                }



                cmd.CommandText = " select distinct a.pattern_id, to_char(to_date(a.valid_from,'yyyymmdd')) dstart, " +
                    "   to_char(to_date(a.valid_to,'yyyymmdd')) dstop, b.so_low codecl, decode(c.nume,'',' ',c.nume) numecl, nvl(a.inactiv,-1), a.toate_mat, d.coef_cal from " +
                    "   sapprd.ZSD_RU_PATTERN a, sapprd.ZSD_RU_PATT_SO b, clienti c, sapprd.ZSD_RU_B_PATTOBJ d " +
                    "   where a.mandt='900' and b.mandt='900' and d.mandt='900' and a.pattern_id = b.pattern_id " + condSablon +
                    "   and d.pattern_id = a.pattern_id and (b.so_name = 'SB5KUNNR' or b.so_name = 'SB5KDGRP') " +
                    "   and c.cod(+) = b.so_low and nvl(a.deleted,-1)<>'X' and a.used <> 'X' order by  a.pattern_id desc";




                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        retVal += oReader.GetString(0) + "#" + oReader.GetString(1) + "#" + oReader.GetString(2) + "#" +
                            oReader.GetString(3) + "#" + oReader.GetString(4) + "#" + oReader.GetString(5) + "#" +
                            oReader.GetDouble(6) + "#" + oReader.GetDouble(6) + "@@";
                    }


                }
                else
                {
                    retVal = "-1";
                }

                oReader.Close();
                oReader.Dispose();

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + "cod user: " + codUser);
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }



            return retVal;

        }


        [WebMethod]//Android
        public string getListMotiveRespingere()
        {
            string retVal = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select t.abgru, t.bezei from sapprd.TVAGT t where mandt = '900' and spras = '4' and abgru<>'00' order by bezei ";

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        retVal += oReader.GetString(0) + "#" + oReader.GetString(1) + "@@";
                    }

                }
                else
                {
                    retVal = "-1";
                }

                oReader.Close();
                oReader.Dispose();

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }



            return retVal;

        }


        [WebMethod]//clientii din comenzi 
        public string getListClientiGED(string codAgent, string tipAgent, string filiala)
        {
            //clientii din comenzile GED emise in ultimile 30 de zile
            string retVal = "";



            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string condAgent = "";
            if (tipAgent.Equals("CV"))
                condAgent = " and a.cod_agent = '" + codAgent + "' ";



            string dateInterval = DateTime.Today.AddDays(-90).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);


            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct a.stceg, a.nume_client from sapprd.zcomhead_tableta a,  agenti c where " +
                                  " a.ul = :fil and a.cod_agent = c.cod and " +
                                  " a.status_aprov in ('20','21') and " +
                                  " a.datac >= '" + dateInterval + "' " + condAgent + " order by a.nume_client ";



                cmd.Parameters.Clear();
                cmd.Parameters.Add(":fil", OracleType.VarChar, 120).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;



                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        retVal += oReader.GetString(0) + "#" + oReader.GetString(1) + "@@";
                    }

                }
                else
                {
                    retVal = "-1";
                }

                oReader.Close();
                oReader.Dispose();

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }



            return retVal;

        }


        [WebMethod]//clientii din comenzi 
        public string getListClienti(string codAgent, string depart, string filiala, string tipUser)
        {
            //clientii din comenzile emise in ultimile 30 de zile
            string retVal = "";



            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string condAgent = "";
            if (codAgent != "00000000")
                condAgent = " and a.cod_agent = '" + codAgent + "' ";



            string sqlString = "";
            string dateInterval = DateTime.Today.AddDays(-30).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            string filialaCautare = filiala;

            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                if (tipUser.Equals("CV"))
                {
                    sqlString = " select distinct ' ', a.nume_client from sapprd.zcomhead_tableta a, agenti c where " +
                                 " a.cod_agent = c.cod and " +
                                 " a.status in ('2','6','9','10','11') and a.status_aprov in ('0','1','2','4','6','7','9','10','11','15') and " +
                                 " a.datac >= '" + dateInterval + "' " + condAgent + " order by a.nume_client ";

                    filialaCautare = Utils.getFilialaGed(filiala);

                }
                else
                {
                    sqlString = " select distinct a.cod_client, b.nume from sapprd.zcomhead_tableta a, clienti b, agenti c where " +
                                 " a.cod_client = b.cod and a.cod_agent = c.cod and " +
                                 " a.status in ('2','6','9','10','11') and a.status_aprov in ('0','1','2','4','6','7','9','10','11','15') and " +
                                 " a.datac >= '" + dateInterval + "' " + condAgent + " order by b.nume ";
                }

                cmd.CommandText = sqlString;

                cmd.Parameters.Clear();


                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        retVal += oReader.GetString(0) + "#" + oReader.GetString(1) + "@@";
                    }

                }
                else
                {
                    retVal = "-1";
                }

                oReader.Close();
                oReader.Dispose();

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }



            return retVal;

        }



        [WebMethod]
        public string getListAgenti(string filiala, string depart, string tipUserSap, string codAgent)
        {
            string retVal = "", condTipAg = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();
                string filialaIP = "";

                if (depart == "11")
                {
                    if (tipUserSap != null && tipUserSap.Equals("SDIP"))
                    {
                        condTipAg = " and a.tip in ('CVIP','SDIP') ";
                        filialaIP = " or a.filiala in (select prctr from sapprd.zsd_ip where mandt='900' and pernr = '" + codAgent + "' ) ";
                    }
                    else if (tipUserSap != null && tipUserSap.Equals("SMG"))
                    {
                        condTipAg = " and a.tip in ('SMG', 'CAG', 'CAG1', 'CAG2', 'CAG3', 'CVG', 'CVA')";
                    }
                    else
                        condTipAg = " and a.tip in ('CONS-GED','SM','CAG', 'CAG1', 'CAG2', 'SMG', 'CVIP', 'SDIP') ";
                }
                else
                {
                    if (depart != "10")
                    {
                        condTipAg = " and a.tip in ('AV','SD') ";
                    }
                }


                if (Utils.isFilialaMicaDep04(connection,filiala, depart) || depart.Equals("04"))
                {
                    cmd.CommandText = " select distinct a.nume, a.cod from agenti a, sapprd.zpern_filiale b where (a.filiala=:fil or b.prctr=:fil " + filialaIP + ") and b.pernr(+) = a.cod and " +
                                      " substr(a.divizie,0,2) = substr(:div,0,2)  " + condTipAg + " and a.activ = 1 order by nume ";
                }
                else
                {
                    cmd.CommandText = " select distinct a.nume, a.cod from agenti a, sapprd.zpern_filiale b where (a.filiala=:fil or b.prctr=:fil " + filialaIP + ") and b.pernr(+) = a.cod and " +
                                      " a.divizie =:div  " + condTipAg + " and a.activ = 1 order by nume ";
                }


               

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":fil", OracleType.VarChar, 120).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                cmd.Parameters.Add(":div", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = depart;


                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        retVal += oReader.GetString(0) + "#" + oReader.GetString(1) + "@@";
                    }

                }
                else
                {
                    retVal = "-1";
                }

                oReader.Close();
                oReader.Dispose();

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }



            return retVal;

        }


        [WebMethod]
        public string getListAgentiJSON(string filiala, string depart, string tipAgent, string codAgent)
        {
            string serializedResult = "", condTipAg = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string filialaIP = " ";

                if (tipAgent != null && tipAgent.Equals("SMR"))
                {
                    condTipAg = " and a.tip in ('CVR', 'SMR', 'CVS', 'CONS-GED', 'SRM', 'CVIP')";
                }
                else if (tipAgent != null && tipAgent.Equals("SMW"))
                {
                    condTipAg = " and a.tip in ('SMW', 'CVW')";
                }
                else if (tipAgent != null && tipAgent.Equals("SMG"))
                {
                    condTipAg = " and a.tip in ('SMG', 'CAG', 'CAG1', 'CAG2', 'CAG3', 'CVG', 'CVA')";
                }
                else if (tipAgent != null && tipAgent.Equals("CVO"))
                {
                    condTipAg = " and a.tip = 'CVO' ";
                }
                else {
                    if (depart == "11")
                    {
                        if (tipAgent != null && tipAgent.Equals("SDCVA"))
                        {
                            condTipAg = " and a.tip in ('SDCVA','CAG', 'CAG1', 'CAG2') ";
                        }
                        else if (tipAgent != null && tipAgent.Equals("SDIP") && codAgent != null)
                        {
                            condTipAg = " and a.tip in ('CVIP', 'SDIP') ";
                            filialaIP = " or a.filiala in (select prctr from sapprd.zsd_ip where mandt='900' and pernr = '" + codAgent + "' ) ";
                        }
                        else {
                            condTipAg = " and a.tip in ('CONS-GED','SM','CAG', 'CAG1', 'CAG2', 'CVS', 'CVG','SMG','CVIP','SDIP') ";
                        }
                    }
                    else
                    {
                        if (depart != "10")
                        {
                            condTipAg = " and a.tip in ('AV','SD', 'ASDL') ";
                        }

                        if (depart == "10")
                        {
                            condTipAg = " and a.tip like '%KA%' ";
                        }

                    }
                }


                
                if (Utils.isFilialaMicaDep04(connection,filiala, depart) || depart.Equals("04"))
                {
                    cmd.CommandText = " select distinct a.nume, a.cod from agenti a, sapprd.zpern_filiale b where (a.filiala=:fil or b.prctr=:fil " + filialaIP + " ) and b.pernr(+) = a.cod and " +
                                     "  substr(a.divizie,0,2) =substr(:div,0,2)  " + condTipAg + " and a.activ = 1 order by nume ";
                }
                else
                {
                    cmd.CommandText = " select distinct a.nume, a.cod from agenti a, sapprd.zpern_filiale b where (a.filiala=:fil or b.prctr=:fil " + filialaIP + " ) and b.pernr(+) = a.cod and " +
                                     " a.divizie =:div  " + condTipAg + " and a.activ = 1 order by nume ";
                }



                cmd.Parameters.Clear();

                string paramFil = filiala;
                if (tipAgent != null && tipAgent.Equals("CVO"))
                    paramFil = "BU10";

                cmd.Parameters.Add(":fil", OracleType.VarChar, 120).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = paramFil;

                cmd.Parameters.Add(":div", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = depart;


                oReader = cmd.ExecuteReader();

                List<Agent> listAgenti = new List<Agent>();
                Agent unAgent;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        unAgent = new Agent();
                        unAgent.nume = oReader.GetString(0);
                        unAgent.cod = oReader.GetString(1);
                        listAgenti.Add(unAgent);
                    }

                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listAgenti);




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
        public string getListComenzi(string filiala, string codUser, string tipCmd, string tipUser, string depart, string interval, int restrictii, string codClient, string tipUserSap, string codSD)
        {

            
            string serializedResult = "";
            string retVal = "";
            string tipComanda = "";
            string selCmd = "";
            string tabDV = "", condDV = "";
            string condData = "", condRestr = "", condClient = "", sqlString = "", pondereB_30 = "0";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand(), cmd1 = new OracleCommand();
            OracleDataReader oReader = null, oReader1 = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string condHome = "";
                string tabelaHome = "";

                string condComenziEmise1 = "";
                string tabelaComenziEmise = "";
                string condComenziEmise2 = "";

                //lista comenzi

                if (tipCmd == "0") //comenzile create in sap, pentru vizualizare
                {
                    string condUser = "";

                    string condExtraUser = "";
                    string condSuperAv = "";

                    if (tipUserSap != null && tipUserSap.Equals("SMR"))
                        condExtraUser = " and ag.tip in ('CVR', 'SMR', 'CVS', 'CONS-GED')";
                    else if (tipUserSap != null && tipUserSap.Equals("SDIP"))
                        condExtraUser = " and ag.tip in ('CONS-GED', 'CVR') and exists (select 1 from clie_tip where cod_cli = a.cod_client and tip = '18') ";
                    else if (tipUser.Equals("SDCVA"))
                        condExtraUser = " and ag.tip in ('SDCVA', 'CAG', 'CAG1', 'CAG2')";
                    else if (tipUser.Equals("SMG"))
                        condExtraUser = " and ag.tip in ('SMG', 'CAG', 'CAG1', 'CAG2', 'CVA')";



                    if (codUser != "00000000")
                    {
                        if (tipUserSap != null && (tipUserSap.Equals(Constants.tipSuperAv) || tipUserSap.Equals(Constants.tipInfoAv)))
                        {
                            condSuperAv = " and ((sav.mandt = '900' and sav.cod_sagent = :codag and sav.id_comanda = a.id) or a.cod_agent = :codag ) ";
                        }
                        else
                        {
                            condSuperAv = " and sav.mandt(+) = '900' and sav.id_comanda(+) = a.id ";

                            string condExtraSAV = "";
                            if (tipUserSap != null && (tipUserSap.Equals("SD") || tipUserSap.Equals("DV") || tipUserSap.Equals("SMR") || tipUserSap.Equals("CVR") || tipUserSap.Equals("SSCM") || tipUserSap.Equals("CGED") || tipUserSap.Equals("OIVPD")))
                                condExtraSAV = " or sav.cod_sagent = :codag  ";


                            condUser = " and ( cod_agent=:codag " + condExtraSAV + ") and substr(a.ul,0,2) = '" + filiala.Substring(0, 2) + "' ";


                            if (tipUserSap != null && (tipUserSap.Equals("SDIP") || tipUserSap.Equals("CVIP")))
                            {
                                condUser = " and ( cod_agent=:codag " + condExtraSAV + ")  ";
                            }


                        }

                        condUser += condSuperAv;

                    }
                    else
                    {
                        if (Utils.isFilialaMicaDep04(connection,filiala, depart) || depart.Equals("04"))
                            condUser = " and substr(a.ul,0,2) = '" + filiala.Substring(0, 2) + "' and substr(ag.divizie,0,2) = '" + depart.Substring(0, 2) + "' " + condExtraUser;
                        else
                            condUser = " and substr(a.ul,0,2) = '" + filiala.Substring(0, 2) + "' and ag.divizie = '" + depart + "' " + condExtraUser;

                        if (tipUserSap != null && tipUserSap.Equals("SDIP") && codSD != null)
                        {
                            condUser = " and ag.tip in ('CVIP','SDIP') and ag.filiala in (select prctr from sapprd.zsd_ip where pernr = '" + codSD + "') ";
                        }

                        condSuperAv = " and sav.mandt(+) = '900' and sav.id_comanda(+) = a.id ";
                        condUser += condSuperAv;

                    }

                    if (codClient != "")
                    {
                        if (tipUser.Equals("CV") || tipUser.Equals("SM") || tipUser.Equals("SMR"))
                        {
                            condClient = " and (a.nume_client = '" + codClient + "' or a.cod_client = '" + codClient + "') ";
                        }
                        else
                        {
                            condClient = " and a.cod_client = '" + codClient + "' ";
                        }
                    }



                    if (interval == "0") //astazi
                    {
                        string dateInterval = DateTime.Today.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        condData = " and a.datac = '" + dateInterval + "' ";
                    }

                    if (interval == "1") //ultimele 7 zile
                    {
                        string dateInterval = DateTime.Today.AddDays(-7).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        condData = " and a.datac >= '" + dateInterval + "' ";
                    }

                    if (interval == "2") //ultimele 30 zile
                    {
                        string dateInterval = DateTime.Today.AddDays(-30).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        condData = " and a.datac >= '" + dateInterval + "' ";
                    }

                    if (interval.Length > 1) //interval
                    {
                        string[] intervalSel = interval.Split('#');
                        string[] data1 = intervalSel[0].Split('.');
                        string[] data2 = intervalSel[1].Split('.');
                        string dataStart = data1[2] + data1[1] + data1[0];
                        string dataStop = data2[2] + data2[1] + data2[0];
                        condData = " and a.datac between '" + dataStart + "' and '" + dataStop + "' ";



                    }


                    if (restrictii == 0) //comenzi emise
                    {

                        if (tipUser == "CV" || tipUser == "SM" || tipUser.Equals("SMR"))
                        {
                            tipComanda = condUser + " and a.status in ('0','2','8','11') ";
                            condRestr = " and a.status_aprov in ('0','1','2','4','6','7','15','20','21') ";
                        }
                        else
                        {
                            tipComanda = condUser + " and a.status in ('2','8','11') ";
                            condRestr = " and a.status_aprov in ('0','1','2','4','6','7','15','20','21') ";
                        }

                    }

                    if (restrictii == 1) //comenzi blocate
                    {
                        if (tipUser == "CV" || tipUser == "SM" || tipUser.Equals("SMR"))
                        {
                            tipComanda = condUser + " and a.status in ('0','2','6','9','10','11','16','99') ";
                            condRestr = " and a.status_aprov in ('3','5','8','9','10') ";
                        }
                        else
                        {
                            tipComanda = condUser + " and a.status in ('2','6','9','10','11','16','99') ";
                            condRestr = " and a.status_aprov in ('3','5','8','9','10','20','21') ";

                        }

                    }



                }
                if (tipCmd == "1") //doar comenzile pentru modificare
                {

                    if (tipUserSap != null && tipUserSap.Equals(Constants.tipSuperAv) || tipUserSap.Equals(Constants.tipOIVPD) || tipUserSap.Equals(Constants.tipCVR))
                    {
                        tipComanda = " and a.cod_agent =:codag and sav.mandt(+) = '900' and sav.cod_sagent(+) = a.cod_agent and sav.id_comanda(+) = a.id and a.status_aprov in ('1','3','4','9','0','2') and a.status in ('2','9','10') ";
                    }
                    else
                    {
                        tipComanda = " and cod_agent=:codag and a.status_aprov in ('1','3','4','9','0','2') and a.status in ('2','9','10') and sav.mandt(+) = '900' and sav.id_comanda(+) = a.id and nvl(sav.id_comanda, -1) = -1 ";
                    }


                    condComenziEmise1 = " and (case " +
                                     " when a.ac_zc = 'X' then " +
                                     " nvl((select 0 from sapprd.vbbe e where e.mandt = '900' and e.vbeln = a.nrcmdsap and vmeng > 0 and rownum < 2),1) " +
                                     " when( (a.auart = 'ZTP' or a.auart = 'CLP') and a.nrcmdsap is not null) then " +
                                     " nvl((select 1 from sapprd.vbfa f2 where f2.mandt = '900' and f2.vbelv = a.nrcmdsap and f2.vbtyp_n = 'V' and rownum = 1),0) " +
                                     " when (vbeln is not null) then  " +
                                     " nvl(nvl((select 1 from sapprd.vttp s where s.mandt = '900' and s.vbeln = f.vbeln and rownum < 2), (select 1 from sapprd.vbfa f1 where f1.mandt = '900' and f1.vbelv = f.vbeln and f1.vbtyp_n = 'R' AND BWART <> ' ' " +
                                     " and not exists(select * from sapprd.mseg g where g.mandt = '900' and g.smbln = f1.vbeln and sjahr = f1.mjahr) and rownum < 2)),0) " +
                                     " else 0 end) = 0 ";

                    tabelaComenziEmise = " , sapprd.vbfa f ";
                    condComenziEmise2 = " and a.mandt = f.mandt(+) and a.nrcmdsap = f.vbelv(+) and 'J' = f.vbtyp_n(+)  ";

                }
                if (tipCmd == "2") //doar comenzile pentru aprobare 
                {

                    tipComanda = " and a.status_aprov in ('111')  ";

                    if (tipUser.Equals("SD"))
                    {
                        string localFilGed = filiala.Substring(0, 2) + "2" + filiala.Substring(3, 1);  //cu 20


                        if (Utils.isFilialaMicaDep04(connection,filiala, depart))
                        {
                            selCmd = " and (a.accept1 = 'X' or a.accept2 = 'X' or a.ac_zc = 'X') and ora_accept1 = '000000' and " +
                                     " (( substr(ag.divizie,0,2) = '" + depart.Substring(0, 2) + "'  and a.depart='11') or substr(a.depart,0,2) = '" + depart.Substring(0, 2) + "') and a.ul in ('" + filiala + "','" + localFilGed + "')";
                        }
                        else
                        {
                            selCmd = " and (a.accept1 = 'X' or a.accept2 = 'X' or a.ac_zc = 'X') and ora_accept1 = '000000' and " +
                                     " ((ag.divizie = '" + depart + "'  and a.depart='11') or a.depart = '" + depart + "') and a.ul in ('" + filiala + "','" + localFilGed + "')";
                        }



                        tipComanda = " and a.status_aprov in ('1','6','21') and a.status in ('2','11') " + selCmd;

                    }

                    if (tipUser.Equals("DV"))
                    {

                        string localDepart = depart;
                        //CMATEI
                        if (codUser == "00010281" || codUser == "00018768")
                        {
                            localDepart = "11";
                            condHome = " and a.aprob_cv_necesar like '%11%' and a.aprob_cv_realiz not like '%11%' ";
                            tabelaHome = "";

                        }
                        else
                        {
                            condHome = "  ";
                            tabelaHome = "";
                        }



                        tabDV = " , sapprd.zfil_dv v ";

                        if (isDV_WOOD(codUser))
                        {
                            selCmd = " and a.accept2 = 'X' and a.ora_accept2 = '000000' ";
                            condDV = " and v.pernr = '" + codUser + "' and v.spart = '" + localDepart + "' and v.prctr = a.ul ";

                        }
                        else
                        {
                            selCmd = " and a.accept2 = 'X' and a.ora_accept2 = '000000' and decode(decode(a.accept1,'X',a.ora_accept1,'1'),'000000',1,0)=0  ";

                            condDV = " and v.pernr = '" + codUser + "' and v.spart = '" + localDepart + "' and substr(v.prctr,0,2) = substr(a.ul,0,2) and substr(a.ul,3,1) != 4 and " +
                                     " v.spart = substr(a.depart,0,2)  ";

                        }


                        tipComanda = " and a.status_aprov in ('1','6','21','20') and a.status in ('2','11') " + selCmd;



                    }

                    if (tipUser.Equals("SM"))
                    {
                        string filSM = filiala.Substring(0, 2) + "2" + filiala.Substring(3, 1);  //cu 20

                        selCmd = " and a.accept1 = 'X' and ora_accept1 = '000000' " +
                                 " and a.depart = '11' and a.ul = '" + filSM + "' ";

                        tipComanda = " and a.status_aprov in ('1') and a.status in ('2') " + selCmd;
                    }

                    string condSuperAv = " and sav.mandt(+) = '900' and sav.id_comanda(+) = a.id ";

                    tipComanda += condSuperAv;

                }
                if (tipCmd == "3") //doar comenzile blocate pt. limita de credit
                {
                    if (tipUser.Equals("AV"))
                    {
                        tipComanda = " and cod_agent=:codag and a.status not in ('6') and a.status_aprov in ('10') ";
                    }

                    else if (tipUser.Equals("SD"))
                    {

                        if (Utils.isFilialaMicaDep04(connection, filiala, depart))
                            tipComanda = " and a.ul = '" + filiala + "' and substr(a.depart,0,2) = '" + depart.Substring(0, 2) + "' and a.status not in ('6') and a.status_aprov in ('10') ";
                        else
                            tipComanda = " and a.ul = '" + filiala + "' and a.depart = '" + depart + "' and a.status not in ('6') and a.status_aprov in ('10') ";

                    }

                    else if (tipUser.Equals("DV"))
                    {
                        tabDV = " , sapprd.zfil_dv v ";
                        condDV = " and v.pernr = '" + codUser + "' and v.spart = '" + depart + "' and substr(v.prctr,0,2) = substr(a.ul,0,2) and v.spart = a.depart ";
                        tipComanda = " and a.depart = '" + depart + "' and a.status not in ('6') and a.status_aprov in ('10') ";
                    }

                    else if (tipUser.Equals("KA"))
                    {
                        tipComanda = " and cod_agent=:codag and a.status not in ('6') and a.status_aprov in ('10') ";
                    }

                    else if (tipUser.Equals("SK"))
                    {
                        tipComanda = " and a.ul = '" + filiala + "' and ag.divizie = '10' and a.status not in ('6') and a.status_aprov in ('10') ";
                    }

                    string condSuperAv = " and sav.mandt(+) = '900' and sav.id_comanda(+) = a.id ";

                    tipComanda += condSuperAv;

                }

                if (tipCmd == "4")  //comenzi ged simulate
                {
                    if (tipUser.Equals("CV"))
                    {
                        tipComanda = " and cod_agent=:codag and a.status not in ('6') and a.status_aprov in ('20', '21', '41') ";

                        if (codClient != "")
                            condClient = " and a.stceg = '" + codClient + "' ";

                        string condSuperAv = " and sav.mandt(+) = '900' and sav.id_comanda(+) = a.id ";

                        tipComanda += condSuperAv;

                    }
                }

                
                string condDepart = "";

                string sqlAvans = " , 0 avans";

                //consilieri
                if (tipUser == "CV" || tipUser == "SM" || tipUser == "SMR" || tipUser == "WOOD")
                {

                    //verificare avans

                    if (tipCmd.Equals("4"))
                    {
                        sqlAvans = " ,nvl((select nvl(sum(p.netwr + p.mwsbp), 0) val_avans from sapprd.vbrk k, sapprd.vbrp p, sapprd.vbfa f " +
                                   " where k.mandt = '900' and k.fkart in ('ZFAS', 'ZFA') and k.fksto <> 'X' and k.mandt = p.mandt " +
                                   " and k.vbeln = p.vbeln and k.mandt = f.mandt and k.vbeln = f.vbeln and p.posnr = f.posnn " +
                                   " and f.vbelv = a.nrcmdsap and f.vbtyp_v = 'C' and f.vbtyp_n = 'M'),0) avans ";
                    }

                    sqlString = " select * from ( select distinct a.id, nvl(a.nume_client,'-') ,to_char(to_date(a.datac,'yyyymmdd')) datac1, a.valoare,a.status, " +
                                " a.cod_client, decode(a.nrcmdsap,' ','-1',a.nrcmdsap) cmdsap, nvl(a.status_aprov,-1) status_aprov, fact_red, " +
                                " a.ul, a.accept1, a.accept2, '0' tip , ag.nume, decode(a.pmnttrms,'',' ',a.pmnttrms) pmnttrms, a.datac, nvl(a.docin,-1), " +
                                " nvl(a.adr_noua,-1), a.city ||', '|| a.adr_livrare, '11' divizie,'-1' nume_client, a.depart, " +
                                " a.aprob_cv_necesar, a.aprob_cv_realiz, a.cod_client cod_client_generic_ged, cond_cv conditii, nvl(ag.nrtel,'-1') telAgent, a.client_raft " +
                                sqlAvans + " , a.lifnr, cl.nume nume_cl, a.ora_accept1, a.ora_accept2, a.ac_zc " +
                                "  from sapprd.zcomhead_tableta a, " +
                                " agenti ag, sapprd.zcomsuperav sav, clienti cl " +
                                " where ag.cod = a.cod_agent and cl.cod = a.cod_client " + tipComanda + condData + condClient + condRestr +
                                " order by id desc ) ";






                }
                //restul (agenti, ka, directori)
                else
                {

                    string condCanal = " and substr(a.ul,3,1) = substr(cl.canal(+),1,1) ";
                    if (isDV_WOOD(codUser))
                        condCanal = "";


                    sqlString = " select distinct a.id, nvl(b.nume,'-') nume1, to_char(to_date(a.datac,'yyyymmdd')) datac1, a.valoare,a.status,  a.cod_client, " +
                                " decode(a.nrcmdsap,' ','-1',a.nrcmdsap) cmdsap, nvl(a.status_aprov,-1) status_aprov, a.fact_red,  a.ul, a.accept1, a.accept2, " +
                                " nvl((select  cl.tip from clie_tip cl where substr(cl.depart,0,2) = substr(a.depart,0,2)  " + condCanal + " and cl.cod_cli = a.cod_client and rownum=1),' ') tip" +
                                " , ag.nume, decode(a.pmnttrms,'',' ',a.pmnttrms) pmnttrms, a.datac, nvl(a.docin,-1) docin1, " +
                                " nvl(a.adr_noua,-1) adr_noua1, a.city ||', '|| a.adr_livrare adr_livrare1, ag.divizie, a.nume_client, a.depart, " +
                                " a.aprob_cv_necesar , a.aprob_cv_realiz, ' ' cod_client_generic_ged, ' ' conditii, nvl(ag.nrtel,'-1') telAgent,a.client_raft " +
                                 sqlAvans + " , a.lifnr, ' ' nume_cl, a.ora_accept1, a.ora_accept2, a.ac_zc " +
                                " from sapprd.zcomhead_tableta a, " +
                                " clienti b, agenti ag, clie_tip cl, sapprd.zcomsuperav sav " + tabelaComenziEmise + tabelaHome + tabDV +
                                " where a.cod_client=b.cod and ag.cod = a.cod_agent " + tipComanda + condDV + condData + condRestr + condClient + condDepart + condHome +
                                "  and cl.cod_cli = a.cod_client  " + condComenziEmise1 + condComenziEmise2 + " order by id ";




                    //pentru aprobare se afiseaza doar ultimele 10 comenzi
                    if (tipCmd.Equals("2") && !depart.StartsWith("04"))
                    {
                        sqlString = " select x.id, x.nume1, to_char(to_date(x.datac,'yyyymmdd')), x.valoare, x.status, x.cod_client, x.cmdsap, x.status_aprov, x.fact_red, x.ul, x.accept1, x.accept2, x.tip, x.nume, x.pmnttrms, x.datac, x.docin1, " +
                                    " x.adr_noua1, x.adr_livrare1, x.divizie, x.nume_client, x.depart, x.aprob_cv_necesar, x.aprob_cv_realiz,x.cod_client_generic_ged,x.conditii, x.telAgent,x.client_raft, x.avans, x.lifnr, x.nume_cl, x.ora_accept1, x.ora_accept2, x.ac_zc " +
                                    " from ( " + sqlString + " ) x where rownum<=15 ";
                    }



                }


                cmd.CommandText = sqlString;

                cmd.Parameters.Clear();


                if ((tipCmd.Equals("0") && codUser != "00000000") || tipCmd.Equals("1") || (tipCmd.Equals("3") && (tipUser.Equals("AV") || tipUser.Equals("KA"))) || (tipCmd.Equals("4") && tipUser.Equals("CV")))
                {
                    cmd.Parameters.Add(":codag", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codUser;
                }



                oReader = cmd.ExecuteReader();
                string cursValut = "0";
                string strNumeClient = " ";
                string isDL = " ";

                List<Comanda> listComenzi = new List<Comanda>();


                if (oReader.HasRows)
                {

                    Comanda comanda;

                    while (oReader.Read())
                    {
                        strNumeClient = oReader.GetString(1);


                        if (oReader.GetString(20).Trim().Length > 2)
                            strNumeClient = oReader.GetString(20);

                        if (strNumeClient.Equals("-1"))
                            strNumeClient = oReader.GetString(30);

                        if (oReader.GetString(29).Length == 4 )
                            isDL = " (CLP)";
                        else if (oReader.GetString(29).Length > 4 && !oReader.GetString(oReader.GetOrdinal("ac_zc")).Equals("X"))
                            isDL = " (DL)";
                        else if (oReader.GetString(oReader.GetOrdinal("ac_zc")).Equals("X"))
                            isDL = " (AC/ZC)";
                        else
                            isDL = " ";

                        comanda = new Comanda();
                        comanda.idComanda = oReader.GetInt32(0).ToString();
                        comanda.numeClient = strNumeClient + isDL;
                        comanda.dataComanda = oReader.GetString(2);
                        comanda.sumaComanda = oReader.GetFloat(3).ToString();

                        comanda.stareComanda = oReader.GetString(7);
                        comanda.codClient = oReader.GetString(5);
                        comanda.cmdSap = oReader.GetString(6);
                        comanda.factRed = oReader.GetString(8);
                        comanda.filiala = oReader.GetString(9);
                        comanda.accept1 = oReader.GetString(10);
                        comanda.accept2 = oReader.GetString(11);
                        comanda.tipClient = oReader.GetString(12);
                        comanda.numeAgent = oReader.GetString(13);
                        comanda.termenPlata = oReader.GetString(14);
                        comanda.docInsotitor = oReader.GetString(16);
                        comanda.adresaLivrare = oReader.GetString(18);
                        comanda.divizieAgent = oReader.GetString(19);
                        comanda.divizieComanda = oReader.GetString(21);
                        comanda.aprobariNecesare = oReader.GetString(22);
                        comanda.aprobariPrimite = oReader.GetString(23);
                        comanda.codClientGenericGed = oReader.GetString(24);
                        comanda.conditiiImpuse = oReader.GetString(25);
                        comanda.telAgent = oReader.GetString(26);
                        comanda.monedaComanda = "RON";
                        comanda.monedaTVA = "RON";

                        if (tipCmd.Equals("4"))
                            //comanda.avans = oReader.GetDouble(28).ToString();
                            comanda.avans = "25";
                        else
                            comanda.avans = "0";

                        double comandaCuTva = oReader.GetFloat(3);
                        string canalDistrib = "20";
                        if (oReader.GetString(9).Substring(2, 1).Equals("1"))
                        {
                            comandaCuTva = oReader.GetFloat(3) * 1.19;
                            canalDistrib = "10";
                        }

                        comanda.sumaTVA = comandaCuTva.ToString();
                        comanda.canalDistrib = canalDistrib;
                        comanda.clientRaft = oReader.GetString(27) == null ? " " : oReader.GetString(27);

                        comanda.isAprobatDistrib = HelperComenzi.isComandaDistribAprobata(comanda.accept1, oReader.GetString(oReader.GetOrdinal("ora_accept1")), comanda.accept2, oReader.GetString(oReader.GetOrdinal("ora_accept2")));
                        comanda.isComandaACZC = oReader.GetString(oReader.GetOrdinal("ac_zc")).Equals("X");

                        if (tipUser == "DV")
                        {
                            //curs valutar
                            cmd1 = connection.CreateCommand();
                            cmd1.CommandText = " select x.ukurs from (select distinct a.ukurs, (100000000 - a.gdatu - 1) " +
                                              " from sapprd.tcurr a, sapprd.knvv b where a.mandt='900' and " +
                                              " b.mandt='900' and b.kurst = a.kurst and b.kunnr =:codClient " +
                                              " and fcurr = 'EUR' and tcurr = 'RON' and (100000000 - a.gdatu - 1) <=:dataCurs " +
                                              " and b.spart =:depart order by 100000000 - a.gdatu - 1 desc ) x where rownum<2 ";

                            cmd1.Parameters.Clear();

                            cmd1.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                            cmd1.Parameters[0].Value = oReader.GetString(5);

                            cmd1.Parameters.Add(":dataCurs", OracleType.VarChar, 10).Direction = ParameterDirection.Input;
                            cmd1.Parameters[1].Value = oReader.GetString(15);

                            cmd1.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                            cmd1.Parameters[2].Value = depart;


                            oReader1 = cmd1.ExecuteReader();
                            if (oReader1.HasRows)
                            {
                                oReader1.Read();
                                cursValut = oReader1.GetDouble(0).ToString();
                            }
                            else
                            {
                                cursValut = "0";
                            }
                            //sf. curs




                        }//sd. tip DV


                        if (tipUser == "DV" || tipUser == "SD")
                        {
                            //pondere articole b pentru ultimele 30 de zile
                            OracleCommand cmd2 = new OracleCommand();
                            OracleDataReader oReader2 = null;

                            cmd2 = connection.CreateCommand();

                            cmd2.CommandText = " select nvl(sum(g.tip_b),0) , nvl(sum(g.netwr - g.cre_m + g.deb_m - g.retur),0)  " +
                                              " from sapprd.ZVANZARI_AG g where g.mandt = '900' and g.fkdat >=:dataSel and g.kunag =:codClient and g.spart =:depart " +
                                              " and g.fkart in ('ZFRA','ZFRB','ZFI','ZFS','ZFSC','ZFPA','ZFE','ZFDC','ZFM','ZFMC') ";

                            cmd2.Parameters.Clear();

                            DateTime localDate = DateTime.Now.AddDays(-30);
                            string selectedDate = localDate.Year.ToString() + localDate.Month.ToString("00") + localDate.Day.ToString("00");



                            cmd2.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                            cmd2.Parameters[0].Value = oReader.GetString(5);

                            cmd2.Parameters.Add(":dataSel", OracleType.VarChar, 10).Direction = ParameterDirection.Input;
                            cmd2.Parameters[1].Value = selectedDate;

                            cmd2.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                            cmd2.Parameters[2].Value = depart;


                            oReader2 = cmd2.ExecuteReader();
                            if (oReader2.HasRows)
                            {
                                oReader2.Read();
                                if (oReader2.GetDouble(1) == 0)
                                    pondereB_30 = "0";
                                else
                                    pondereB_30 = String.Format("{0:0.00}", oReader2.GetDouble(0) / oReader2.GetDouble(1) * 100);
                            }
                            else
                            {
                                pondereB_30 = "0";
                            }

                            if (oReader2 != null)
                            {
                                oReader2.Close();
                                oReader2.Dispose();
                            }
                            //sf. pondere

                        }//sf. if


                        String adresaNoua = "-1";
                        if (oReader.GetString(17).Equals("X"))
                            adresaNoua = oReader.GetString(18);

                        retVal += "#" + cursValut + "#" + oReader.GetString(16) + "#" + adresaNoua + "#" + oReader.GetString(19) + "#" + pondereB_30 + "@@";
                        comanda.cursValutar = cursValut;
                        comanda.adresaNoua = adresaNoua;
                        comanda.pondere30 = pondereB_30;

                        comanda.isCmdInstPublica = "False";
                        if (tipCmd == "1" && codUser != "00000000" && tipUserSap != null && (tipUserSap.Equals("CVR") || tipUserSap.Equals("CONS-GED") || tipUserSap.Equals("CVIP") || tipUserSap.Equals("SDIP")))
                        {
                            DateClientInstPublica dateClient = HelperClienti.getDateClientInstPublica(connection, comanda.codClient);

                            comanda.isCmdInstPublica = dateClient.isClientInstPublica.ToString();
                            comanda.tipClientInstPublica = dateClient.tipClientInstPublica;
                        }

                        listComenzi.Add(comanda);
                    }


                }


                //comenzile CV pentru aprobare
                if (tipCmd == "2" && tipUser == "DV" && !isDV_WOOD(codUser))
                {

                    string localDepart = depart;
                    string condSuplGed = " or (v.spart = substr(ag.divizie, 0, 2)) ";

                    //CMATEI
                    if (codUser == "00010281" || codUser == "00018768")
                    {
                        localDepart = "11";
                        condSuplGed = "";
                    }


                    sqlString = " select distinct a.id, nvl(b.nume,'-') nume1, to_char(to_date(a.datac,'yyyymmdd')) datac1, a.valoare,a.status,  a.cod_client, " +
                               " decode(a.nrcmdsap,' ','-1',a.nrcmdsap) cmdsap, nvl(a.status_aprov,-1) status_aprov, a.fact_red,  a.ul, a.accept1, a.accept2, " +
                               " ' ' cl_tip, ag.nume, decode(a.pmnttrms,'',' ',a.pmnttrms) pmnttrms, a.datac, nvl(a.docin,-1) docin1, " +
                               " nvl(a.adr_noua,-1) adr_noua1, a.city ||', '|| a.adr_livrare adr_livrare1, ag.divizie, a.nume_client, a.depart, " +
                               " a.aprob_cv_necesar, nvl(a.aprob_cv_realiz,' ') aprob_cv_realiz, nvl(ag.nrtel,'-1') telAgent, a.client_raft, a.ac_zc, a.lifnr " +
                               " from sapprd.zcomhead_tableta a, " +
                               " clienti b, agenti ag " + tabDV +
                               " where a.cod_client=b.cod and ag.cod = a.cod_agent and a.tip_pers in ('CV','CVS','SITE') " +
                               " and a.status_aprov in ('1','4','6','21') and a.status in ('2','11') " +
                               " and v.pernr = '" + codUser + "' and v.spart = '" + localDepart + "' and substr(v.prctr,0,2) = substr(a.ul,0,2) and substr(a.ul,3,1) != 4 and a.depart='11' " +
                               " and decode(decode(a.accept1,'X',a.ora_accept1,'1'),'000000',1,0)=0 and decode(a.status_aprov,'21',decode(a.accept2,'X',a.ora_accept2,'000000'),'000000') = '000000' " +
                               " and ((a.aprob_cv_necesar like '%" + localDepart + "%' and a.aprob_cv_realiz not like '%" + localDepart + "%') " + condSuplGed + "  ) " +
                               " and a.cond_cv not like '%" + localDepart + "%' " +
                               " order by id ";



                    //pentru aprobare se afiseaza doar ultimele x comenzi
                    if (tipCmd.Equals("2") && !depart.Equals("04"))
                    {
                        sqlString = " select x.id, x.nume1, to_char(to_date(x.datac,'yyyymmdd')), x.valoare, x.status, x.cod_client, x.cmdsap, x.status_aprov, " +
                                    " x.fact_red, x.ul, x.accept1, x.accept2, ' ' x_tip, x.nume, x.pmnttrms, x.datac, x.docin1, " +
                                    " x.adr_noua1, x.adr_livrare1, x.divizie, x.nume_client, x.depart, x.aprob_cv_necesar, x.aprob_cv_realiz, x.telAgent,x.client_raft, x.ac_zc, x.lifnr   from ( " + sqlString + " ) x where rownum<=200 ";
                    }




                    cmd.CommandText = sqlString;
                    cmd.Parameters.Clear();
                    oReader = cmd.ExecuteReader();

                    isDL = " ";

                    if (oReader.HasRows)
                    {

                        Comanda comanda;

                        while (oReader.Read())
                        {
                            strNumeClient = oReader.GetString(1);


                            if (oReader.GetString(oReader.GetOrdinal("lifnr")).Length == 4)
                                isDL = " (CLP)";
                            else if (oReader.GetString(oReader.GetOrdinal("lifnr")).Length > 4 && !oReader.GetString(oReader.GetOrdinal("ac_zc")).Equals("X"))
                                isDL = " (DL)";
                            else
                                isDL = " ";

                            if (oReader.GetString(20).Trim().Length > 2)
                                strNumeClient = oReader.GetString(20);


                            comanda = new Comanda();
                            comanda.idComanda = oReader.GetInt32(0).ToString();
                            comanda.numeClient = strNumeClient + isDL;
                            comanda.dataComanda = oReader.GetString(2);
                            comanda.sumaComanda = oReader.GetFloat(3).ToString();



                            comanda.stareComanda = oReader.GetString(7);
                            comanda.codClient = oReader.GetString(5);
                            comanda.cmdSap = oReader.GetString(6);
                            comanda.factRed = oReader.GetString(8);
                            comanda.filiala = oReader.GetString(9);
                            comanda.accept1 = oReader.GetString(10);
                            comanda.accept2 = oReader.GetString(11);
                            comanda.tipClient = oReader.GetString(12);
                            comanda.numeAgent = oReader.GetString(13);
                            comanda.termenPlata = oReader.GetString(14);
                            comanda.docInsotitor = oReader.GetString(16);
                            comanda.adresaLivrare = oReader.GetString(18);
                            comanda.divizieAgent = oReader.GetString(19);
                            comanda.divizieComanda = oReader.GetString(21);
                            comanda.aprobariNecesare = oReader.GetString(22);
                            comanda.aprobariPrimite = oReader.GetString(23);
                            comanda.telAgent = oReader.GetString(24);
                            comanda.avans = "0";

                            comanda.monedaComanda = "RON";
                            comanda.monedaTVA = "RON";

                            comanda.sumaTVA = oReader.GetFloat(3).ToString();
                            comanda.canalDistrib = "20";

                            comanda.cursValutar = "0";
                            comanda.adresaNoua = "-1";
                            comanda.pondere30 = "0";
                            comanda.clientRaft = oReader.GetString(25) == null ? " " : oReader.GetString(25);

                            comanda.isComandaACZC = oReader.GetString(oReader.GetOrdinal("ac_zc")).Equals("X");

                            bool alreadyExists = listComenzi.Any(x => x.idComanda == comanda.idComanda);

                            if (!alreadyExists)
                                listComenzi.Add(comanda);




                        }
                    }




                }



                oReader.Close();
                oReader.Dispose();

                if (oReader1 != null)
                {
                    oReader1.Close();
                    oReader1.Dispose();
                }


                if (tipCmd == "0" && (tipUserSap == "AV" || tipUserSap == "SD" || tipUserSap == "KA") && codClient == "" && codUser != "00000000")
                {
                    List<Comanda> listCustodii = new OperatiiComenzi().getLivrariCustodie(codUser, interval, "0");
                    listComenzi.AddRange(listCustodii);
                    listComenzi = listComenzi.OrderByDescending(o => o.idComanda).ToList();
                }


                if (tipCmd == "0" && (tipUserSap.Equals("AV") || tipUserSap.Equals("SD") || tipUserSap.Equals("DV") || tipUserSap.StartsWith("KA")))
                {
                    if (listComenzi.Count > 0)
                        InfoComenziSap.getDateHeader(listComenzi);
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listComenzi);

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + " \n\n" +  filiala + " \n\n" + codUser + " \n\n" + tipCmd + " \n\n" + tipUser + " \n\n" + depart + " \n\n" + interval + " \n\n" + restrictii + " \n\n" + codClient + " \n\n" + tipUserSap);
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                cmd1.Dispose();
                connection.Close();
                connection.Dispose();

            }


            return serializedResult;

        }


        private string getTipClient(string codTip)
        {
            string retVal = " ";

            if (codTip.Equals("01"))
                retVal = "Client final";

            if (codTip.Equals("02"))
                retVal = "Constructor general";

            if (codTip.Equals("03"))
                retVal = "Constructor special";

            if (codTip.Equals("04"))
                retVal = "Revanzator";

            if (codTip.Equals("05"))
                retVal = "Producator mobila";

            if (codTip.Equals("06"))
                retVal = "Debitor mat. lemnoase";

            if (codTip.Equals("07"))
                retVal = "Tepar";

            if (codTip.Equals("08"))
                retVal = "Nespecificat";

            if (codTip.Equals("09"))
                retVal = "Client extern UE";

            if (codTip.Equals("10"))
                retVal = "Client extern non-UE";

            return retVal;
        }



        [WebMethod]
        public string getInfoVenituriData(string codDepart, string filiala, string luna, string an, string codAgent)
        {
            return new Venituri().getInfoVenituriData(codDepart, filiala, luna, an, codAgent);
        }




        [WebMethod]//Android
        public string getListComenziConditii(string filiala, string depart, string tipUser, string codUser)
        {
            string retVal = "";

            string tabDV = "", condTipUser = "";
            string sqlString = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                if (tipUser.Equals("SD"))
                {
                    condTipUser = " and (substr(ag.divizie,0,2) = '" + depart + "' " +
                             " or a.depart = '" + depart + "') and a.ul = '" + filiala + "' ";


                }

                if (tipUser.Equals("DV") || tipUser.Equals("DD"))
                {

                    tabDV = " , sapprd.zfil_dv v ";
                    condTipUser = " and v.pernr = '" + codUser + "' and v.spart = '" + depart + "' and v.prctr = a.ul and (v.spart = substr(ag.divizie,0,2) or v.spart = a.depart) ";

                }


                sqlString = " select distinct a.id, nvl(b.nume,'-'), to_char(to_date(a.datac,'yyyymmdd')) datac, a.valoare,a.status, " +
                            " a.cod_client,decode(a.nrcmdsap,' ','-1',a.nrcmdsap) cmdsap, nvl(a.status_aprov,-1) status_aprov, fact_red, " +
                            " a.ul, a.accept1, a.accept2, '0' tip, ag.nume, decode(a.pmnttrms,'',' ',a.pmnttrms) pmnttrms, a.datac, nvl(a.docin,-1), " +
                            " nvl(a.adr_noua,-1), a.city ||', '|| a.adr_livrare, ag.divizie  from sapprd.zcomhead_tableta a, " +
                            " clienti b, agenti ag, clie_tip cl " + tabDV +
                            " where a.cod_client=b.cod and ag.cod = a.cod_agent and a.status_aprov in ('4') and a.status in ('2') "
                            + condTipUser +
                            " and cl.canal = 10 and cl.cod_cli = a.cod_client order by id desc";




                cmd.CommandText = sqlString;

                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();



                if (oReader.HasRows)
                {



                    while (oReader.Read())
                    {
                        retVal += oReader.GetInt32(0) + "#" + oReader.GetString(1) + "#" + oReader.GetString(2) +
                                "#" + oReader.GetFloat(3) + "#" + oReader.GetString(4) + "#" + oReader.GetString(5) +
                                "#" + oReader.GetString(6) + "#" + oReader.GetString(7) + "#" + oReader.GetString(8) +
                                "#" + oReader.GetString(9) + "#" + oReader.GetString(10) + "#" + oReader.GetString(11) +
                                "#" + oReader.GetString(12) + "#" + oReader.GetString(13) + "#" + oReader.GetString(14);




                        String adresaNoua = "-1";
                        if (oReader.GetString(17).Equals("X"))
                            adresaNoua = oReader.GetString(18);

                        retVal += "#-1#" + oReader.GetString(16) + "#" + adresaNoua + "#" + oReader.GetString(19) + "#-1@@";


                    }


                }
                else
                {
                    retVal = "-1";
                }


                oReader.Close();
                oReader.Dispose();


            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }




            return retVal;



        }


        [WebMethod]
        public string updateComandaSimulata(string idComanda, string totalComanda, string listArticole)
        {
            return new OperatiiComenzi().actualizeazaComandaSimulata(idComanda, totalComanda, listArticole);
        }

        [WebMethod]//lista dispozitii livrare
        public string getListDl(string filiala, string depart, string tipClp, string interval, string tipUser, string codUser)
        {
            string serializedResult = "";


            string sqlString = "", condData = "", dateInterval = "", tipComanda = "", statusCmd = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            if (tipClp.Equals("0")) //comenzi create
            {

                tipComanda = " ";
                if (tipUser == "SD")
                {
                    tipComanda = " and a.ul = '" + filiala + "' and a.depart = '" + depart + "'";
                }

                if (tipUser == "AV")
                {
                    tipComanda = " and (a.cod_agent = '" + codUser + "' or a.cod_agent2 = '" + codUser + "') ";
                }


                statusCmd = " and a.status in (2, 6) ";
            }

            if (tipClp.Equals("-1")) //comenzi ce urmeaza a fi aprobate
            {
                statusCmd = " and a.status = 2 and a.status_aprov = 1 ";

                if (tipUser == "SD")
                {
                    tipComanda = " and a.ul = '" + filiala + "' and a.accept1 in ('X') and a.depart = '" + depart + "'";
                }



            }



            if (interval == "0") //astazi
            {
                dateInterval = DateTime.Today.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and a.datac = '" + dateInterval + "' ";
            }

            if (interval == "1") //ultimele 7 zile
            {
                dateInterval = DateTime.Today.AddDays(-7).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and a.datac >= '" + dateInterval + "' ";
            }

            if (interval == "2") //ultimele 30 zile
            {
                dateInterval = DateTime.Today.AddDays(-30).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and a.datac >= '" + dateInterval + "' ";
            }

            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                sqlString = " select a.id,b.nume, nvl(c.nume,' '), to_char(to_date(a.datac,'yyyymmdd')), a.ul, decode(a.nrcmdsap,' ','-1',a.nrcmdsap) cmdsap, l.name1, a.depoz_dest, a.status_aprov " +
                            " from sapprd.zclphead a, " +
                            " clienti b, agenti c, sapprd.lfa1 l  where  l.mandt = '900' " + statusCmd + " and l.lifnr = a.ul_dest and a.dl = 'X' and " +
                            " a.cod_client = b.cod and c.cod(+) = a.cod_agent2 " + condData + tipComanda + " order by a.id desc ";




                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();
                oReader = cmd.ExecuteReader();

                List<DocumentCLP> listaDocumenteDl = new List<DocumentCLP>();
                DocumentCLP unDocumentDL = null;

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {

                        unDocumentDL = new DocumentCLP();
                        unDocumentDL.nrDocument = oReader.GetInt32(0).ToString();
                        unDocumentDL.numeClient = oReader.GetString(1);
                        unDocumentDL.numeAgent = oReader.GetString(2);
                        unDocumentDL.dataDocument = oReader.GetString(3);
                        unDocumentDL.unitLog = oReader.GetString(4);
                        unDocumentDL.nrDocumentSap = oReader.GetString(5);
                        unDocumentDL.statusDocument = oReader.GetString(6);
                        unDocumentDL.depozit = oReader.GetString(7);
                        unDocumentDL.furnizor = oReader.GetString(8);
                        listaDocumenteDl.Add(unDocumentDL);

                    }

                }


                oReader.Close();
                oReader.Dispose();


                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaDocumenteDl);


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





        [WebMethod]//lista clp
        public string getListClp(string filiala, string depart, string tipClp, string interval, string tipUser, string codUser, string tipUserSap)
        {


            string serializedResult = "";

            string sqlString = "", tipComanda = "", statusCmd = "", condData = "", dateInterval = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            if (tipClp.Equals("-1")) //comenzi ce urmeaza a fi aprobate
            {
                statusCmd = " a.status = 2 and a.status_aprov = 1 ";

                if (tipUser == "SD")
                {
                    tipComanda = " and (substr(a.depart,0,2) = '" + depart.Substring(0, 2) + "' or substr(c.divizie,0,2) = '" + depart.Substring(0, 2) + "')   and ((a.ul_dest = '" + filiala + "' and a.accept1 not in ('X') and a.fasonate !='X' ) or (a.ul = '" + filiala + "' and a.accept1 in ('X'))) ";
                }

                if (tipUser == "SM")
                {
                    tipComanda = " and a.depart = '11' and ((a.ul = '" + filiala + "' and a.accept1 not in ('X') and a.fasonate !='X' ) or (a.ul_dest = '" + filiala + "' and a.accept1 in ('X'))) ";
                }

                if (tipUser == "DV")
                {
                    tipComanda = " and (a.mt = 'TERT' or (a.mt = 'TRAP' and a.curier = 'BV90')) and a.accept1 = '1' and substr(a.depart,0,2) = '" + depart + "' and a.ul in (select prctr from sapprd.zfil_dv where pernr = '" + codUser + "' ) ";
                }

              
            }

            if (tipClp.Equals("0")) //comenzi create
            {

                tipComanda = " ";
                if (tipUser == "SD")
                {

                    string condDepart = " and a.depart = '" + depart + "'";

                    if (Utils.isFilialaMicaDep04(filiala, depart))
                        condDepart = " and substr(a.depart,0,2) = '" + depart.Substring(0, 2) + "'";

                    tipComanda = " and a.ul = '" + filiala + "' " + condDepart;
                    
                }

                if (tipUser == "SM")
                {
                    string localTipAngajat;

                    if (tipUserSap == null || tipUserSap.Trim().Length == 0)
                        localTipAngajat = Utils.getTipAngajat(codUser);
                    else
                        localTipAngajat = tipUserSap;

                    if (localTipAngajat.Equals("SMG"))
                    {
                        tipComanda = " and c.tip in ('SMG', 'CAG2') and (a.ul = '" + filiala + "' or a.ul_dest = '" + filiala + "' ) ";
                    }
                    else
                    {
                        tipComanda = " and a.depart = '11' and (a.ul = '" + filiala + "' or a.ul_dest = '" + filiala + "' ) ";
                    }
                }

                if (tipUser == "AV" || tipUser == "CV" || tipUser == "NN")
                {
                    tipComanda = " and (a.cod_agent = '" + codUser + "' or a.cod_agent2 = '" + codUser + "') ";
                }

                if (tipUser == "DV")
                {
                    tipComanda = " and (a.mt = 'TERT' or (a.mt = 'TRAP' and a.curier = 'BV90')) and a.status_aprov = 2 and a.depart = '" + depart + "' and a.ul in (select prctr from sapprd.zfil_dv where pernr = '" + codUser + "' ) ";
                }


                statusCmd = " a.status in (2, 3, 6) ";
            }

            if (tipClp.Equals("1")) //comenzi primite si aprobate
            {
                tipComanda = " and a.ul_dest = '" + filiala + "'";
                statusCmd = " a.status = 2 and a.status_aprov = 2 ";
            }

            if (tipClp.Equals("2")) //comenzi primite si respinse
            {
                tipComanda = " and a.ul_dest = '" + filiala + "'";
                statusCmd = " a.status = 6 and a.status_aprov = 3 ";
            }


            if (interval == "0") //astazi
            {
                dateInterval = DateTime.Today.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and a.datac = '" + dateInterval + "' ";
            }

            if (interval == "1") //ultimele 7 zile
            {
                dateInterval = DateTime.Today.AddDays(-7).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and a.datac >= '" + dateInterval + "' ";
            }

            if (interval == "2") //ultimele 30 zile
            {
                dateInterval = DateTime.Today.AddDays(-30).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and a.datac >= '" + dateInterval + "' ";
            }



            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                sqlString = " select distinct a.id,nvl(b.nume,' '),nvl(c.nume,' '), nvl(to_char(to_date(a.datac,'yyyymmdd')),' '), a.ul, decode(a.nrcmdsap,' ','-1',a.nrcmdsap) cmdsap, " +
                            " a.status_aprov, (select p.depoz from sapprd.zclpdet p where p.mandt = '900' and p.id = a.id and p.depoz <> ' ' and rownum = 1) depoz_dest,a.ul_dest, a.name1, a.obs, a.status, nvl(c.nrtel,' ')  " +
                            " from sapprd.zclphead a, " +
                            " clienti b, agenti c  where " + statusCmd +
                            " and a.cod_client = b.cod(+) and a.dl <> 'X' and c.cod(+) = a.cod_agent " + tipComanda + condData + " order by a.id desc ";



                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();
                oReader = cmd.ExecuteReader();


                List<DocumentCLP> listaDocumenteClp = new List<DocumentCLP>();
                DocumentCLP unDocumentCLP = null;

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        unDocumentCLP = new DocumentCLP();
                        unDocumentCLP.nrDocument = oReader.GetInt32(0).ToString();
                        unDocumentCLP.numeClient = oReader.GetString(9).Trim().Length == 0 ? oReader.GetString(1) : oReader.GetString(9);
                        unDocumentCLP.numeAgent = oReader.GetString(2) + " (" + oReader.GetString(12) + ")";
                        unDocumentCLP.dataDocument = oReader.GetString(3);
                        unDocumentCLP.unitLog = oReader.GetString(4);
                        unDocumentCLP.nrDocumentSap = oReader.GetString(5);
                        unDocumentCLP.statusDocument = oReader.GetString(11).Equals("3") ? "9" : oReader.GetString(6);
                        unDocumentCLP.depozit = oReader.GetString(7);
                        unDocumentCLP.furnizor = oReader.GetString(8);
                        unDocumentCLP.observatii = oReader.GetString(9);
                        listaDocumenteClp.Add(unDocumentCLP);

                    }

                }


                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaDocumenteClp);

                oReader.Close();
                oReader.Dispose();


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


        [WebMethod]//articole comenzi clp
        public string getListArtClp(string nrCmd)
        {

            string retVal = "", antetCmd = "", artCmd = "", tipPlata = "", tipTransp = "", aprobOC = "", clpDeSters = "", statusAprov = "", valComanda = "", obsComanda = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                //date livrare comanda
                cmd.CommandText = " select pers_contact,telefon,adr_livrare,city,region,decode(ketdat,' ',' ',to_char(to_date(ketdat,'yyyymmdd'))), felmarfa, masa, tipcamion, tipinc, " +
                                  " tip_plata, mt, " +
                                  " nvl((select  k.acc_ofc from sapprd.ekko k where k.mandt = '900' and k.ebeln = nrcmdsap ),' '), " +
                                  " (select count(*) from sapprd.ekbe e where e.mandt = '900' and e.ebeln = nrcmdsap " +
                                  " and ( e.bewtp <> 'L' or (e.bewtp = 'L' and exists (select * from sapprd.vttp p where p.mandt = '900' and p.vbeln = e.belnr)))), status_aprov, " +
                                  " nvl(val_comanda,0), nvl(obs,' ')" +
                                  " from sapprd.zclphead where id=:idcmd ";



                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrCmd);

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    antetCmd += oReader.GetString(0) + "#" + oReader.GetString(1) + "#" + oReader.GetString(2) + "#" +
                                oReader.GetString(3) + "#" + oReader.GetString(4) + "#" + oReader.GetString(5) + "#" + oReader.GetString(6) + "#" +
                                oReader.GetString(7) + "#" + oReader.GetString(8) + "#" + oReader.GetString(9);

                    tipPlata = oReader.GetString(10);
                    tipTransp = oReader.GetString(11);
                    aprobOC = oReader.GetString(12);
                    clpDeSters = oReader.GetInt32(13).ToString();
                    statusAprov = oReader.GetString(14);
                    valComanda = oReader.GetDouble(15).ToString();
                    obsComanda = oReader.GetString(16);

                }
                else
                {
                    antetCmd = "-1";
                }
                //sf. date livrare



                //articole comanda
                cmd.CommandText = " select decode(length(a.cod),18,substr(a.cod,-8),a.cod) cod ,b.nume,a.cantitate,a.umb,a.depoz,a.status from sapprd.zclpdet a, articole b " +
                                  " where a.cod = b.cod and id=:idcmd ";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrCmd);

                oReader = cmd.ExecuteReader();
                int nrArt = 0;
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        artCmd += oReader.GetString(0) + "#" + Regex.Replace(oReader.GetString(1), @"[!]|[#]|[@@]|[,]", " ") + "#" + oReader.GetDouble(2).ToString() + "#" +
                                    oReader.GetString(3) + "#" + oReader.GetString(4) + "#" + oReader.GetString(5) + "@@";

                        nrArt++;
                    }
                }
                else
                {
                    artCmd = "-1";
                }


                //sf. articole comanda

                oReader.Close();
                oReader.Dispose();

                retVal = antetCmd + "#" + nrArt.ToString() + "#" + tipPlata + "#" + tipTransp + "#" + aprobOC + "#" + clpDeSters + "#" + statusAprov + "#" + valComanda + "#" + obsComanda + "@@" + artCmd;



            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }


            return retVal;

        }


        [WebMethod]//articole comenzi clp
        public string getListArtClpJSON(string nrCmd)
        {
            OperatiiCLP opClp = new OperatiiCLP();
            return opClp.getListArtClpJSON(nrCmd);


        }



        [WebMethod]
        public string operatiiDl(string nrCmd, string nrCmdSap, string tipOp, string codUser, string tipUser, string filiala)
        {
            string retVal = "-1";

            try
            {

                if (tipOp.Equals("0"))  //aprobare
                {

                    SapWsClp.ZCLP_WEBSERVICE webServiceClp = null;

                    webServiceClp = new ZCLP_WEBSERVICE();

                    System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                    webServiceClp.Credentials = nc;
                    webServiceClp.Timeout = 300000;

                    SapWsClp.ZaprobaComanda inParam = new SapWsClp.ZaprobaComanda();
                    inParam.VId = Convert.ToDecimal(nrCmd);
                    inParam.PernrCh = codUser;

                    SapWsClp.ZaprobaComandaResponse outParam = webServiceClp.ZaprobaComanda(inParam);

                    if (outParam.VOk.ToString().Equals("0"))
                    {
                        retVal = "Comanda a fost aprobata";
                        if (!isClpTransferIntreFiliale(nrCmd))
                            sendAlertMailCreareClp(nrCmd);
                    }
                    else
                        retVal = "Eroare aprobare comanda";

                    webServiceClp.Dispose();


                }

                if (tipOp.Equals("1"))  //respingere
                {


                    SapWsClp.ZCLP_WEBSERVICE webServiceClp = null;

                    webServiceClp = new ZCLP_WEBSERVICE();

                    System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                    webServiceClp.Credentials = nc;
                    webServiceClp.Timeout = 300000;

                    SapWsClp.ZstergeSto inParam = new SapWsClp.ZstergeSto();
                    inParam.VId = Convert.ToDecimal(nrCmd);
                    inParam.Stare = "3";
                    inParam.PernrCh = codUser;

                    SapWsClp.ZstergeStoResponse outParam = webServiceClp.ZstergeSto(inParam);

                    if (outParam.VOk.ToString().Equals("0"))
                        retVal = "Comanda a fost respinsa";
                    else
                        retVal = outParam.VErr.ToString();


                    webServiceClp.Dispose();

                }


                if (tipOp.Equals("2"))  //stergere
                {


                    SapWsClp.ZCLP_WEBSERVICE webServiceClp = null;

                    webServiceClp = new ZCLP_WEBSERVICE();

                    System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                    webServiceClp.Credentials = nc;
                    webServiceClp.Timeout = 300000;

                    SapWsClp.ZstergeSto inParam = new SapWsClp.ZstergeSto();
                    inParam.VId = Convert.ToDecimal(nrCmd);
                    inParam.Stare = "5";
                    inParam.PernrCh = codUser;

                    SapWsClp.ZstergeStoResponse outParam = webServiceClp.ZstergeSto(inParam);

                    if (outParam.VOk.ToString().Equals("0"))
                        retVal = "Comanda a fost stearsa";
                    else
                        retVal = outParam.VErr.ToString();

                    webServiceClp.Dispose();

                }


            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {

            }

            return retVal;
        }




        [WebMethod]
        public string operatiiClp(string nrCmd, string nrCmdSap, string tipOp, string codUser, string tipUser, string filiala)
        {
            string retVal = "-1";

            try
            {

                if (tipOp.Equals("0"))  //aprobare
                {

                   
                  

                    //verificare filiala clp
                    OracleConnection connection = new OracleConnection();
                    OracleCommand cmd = new OracleCommand();
                    OracleDataReader oReader = null;

                    string connectionString = GetConnectionString_android();

                    connection.ConnectionString = connectionString;
                    connection.Open();

                    cmd = connection.CreateCommand();


                    //date livrare comanda
                    cmd.CommandText = " select ul, ul_dest, mt, fasonate from sapprd.zclphead where id=:idcmd ";

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = Int32.Parse(nrCmd);

                    oReader = cmd.ExecuteReader();
                    string ulInit = "NN", ulDest = "NN", tipTransp = " ", fasonate = " ";
                    bool isOKToRelease = false;

                    if (oReader.HasRows)
                    {
                        oReader.Read();

                        ulInit = oReader.GetString(0);
                        ulDest = oReader.GetString(1);

                        tipTransp = oReader.GetString(2);
                        fasonate = oReader.GetString(3);


                        //actualizare status clp
                        if (tipUser == "SD" || tipUser == "SM")
                        {
                            //acceptul sd-ului din filiala emitenta
                            if (filiala == ulInit)
                            {

                                cmd.CommandText = " update sapprd.zclphead set accept1 = '1' where id =:idCmd ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                                cmd.Parameters[0].Value = Int32.Parse(nrCmd);

                                cmd.ExecuteNonQuery();

                                retVal = "Comanda a fost aprobata";

                                if (fasonate == "X" && tipTransp != "TERT")
                                    isOKToRelease = true;

                            }

                            if (filiala == ulDest)
                            {
                                if (fasonate != "X")
                                    isOKToRelease = true;

                            }
                        }
                        else   //accept DV
                        {
                            cmd.CommandText = " update sapprd.zclphead set accept1 = '2' where id =:idCmd ";
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                            cmd.Parameters[0].Value = Int32.Parse(nrCmd);

                            cmd.ExecuteNonQuery();

                            retVal = "Comanda a fost aprobata";

                            if (fasonate == "X" && tipTransp == "TERT")
                                isOKToRelease = true;
                        }


                        //sf. actualizare




                    }

                    oReader.Close();
                    oReader.Dispose();

                    cmd.Dispose();
                    connection.Close();
                    connection.Dispose();


                    if (isOKToRelease)
                    {
                        SapWsClp.ZCLP_WEBSERVICE webServiceClp = null;

                        webServiceClp = new ZCLP_WEBSERVICE();

                        System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                        webServiceClp.Credentials = nc;
                        webServiceClp.Timeout = 300000;

                        SapWsClp.ZaprobaComanda inParam = new SapWsClp.ZaprobaComanda();
                        inParam.VId = Convert.ToDecimal(nrCmd);
                        inParam.PernrCh = codUser;

                        SapWsClp.ZaprobaComandaResponse outParam = webServiceClp.ZaprobaComanda(inParam);

                        if (outParam.VOk.ToString().Equals("0"))
                        {
                            retVal = "Comanda a fost aprobata";
                            if (!isClpTransferIntreFiliale(nrCmd))
                                sendAlertMailCreareClp(nrCmd);
                        }
                        else
                            retVal = "Eroare aprobare comanda";

                        webServiceClp.Dispose();

                    }


                    new JurnalAprobari().salveazaAprobare(nrCmd, codUser, "A");


                }

                if (tipOp.Equals("1"))  //respingere
                {

                    new JurnalAprobari().salveazaAprobare(nrCmd, codUser, "R");
                    

                    SapWsClp.ZCLP_WEBSERVICE webServiceClp = null;

                    webServiceClp = new ZCLP_WEBSERVICE();

                    System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                    webServiceClp.Credentials = nc;
                    webServiceClp.Timeout = 300000;

                    SapWsClp.ZstergeSto inParam = new SapWsClp.ZstergeSto();
                    inParam.VId = Convert.ToDecimal(nrCmd);
                    inParam.Stare = "3";
                    inParam.PernrCh = codUser;

                    SapWsClp.ZstergeStoResponse outParam = webServiceClp.ZstergeSto(inParam);

                    if (outParam.VOk.ToString().Equals("0"))
                        retVal = "Comanda a fost respinsa";
                    else
                        retVal = outParam.VErr.ToString();


                    webServiceClp.Dispose();

                }


                if (tipOp.Equals("2"))  //stergere
                {


                    SapWsClp.ZCLP_WEBSERVICE webServiceClp = null;

                    webServiceClp = new ZCLP_WEBSERVICE();

                    System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                    webServiceClp.Credentials = nc;
                    webServiceClp.Timeout = 300000;

                    SapWsClp.ZstergeSto inParam = new SapWsClp.ZstergeSto();
                    inParam.VId = Convert.ToDecimal(nrCmd);
                    inParam.Stare = "5";
                    inParam.PernrCh = codUser;

                    SapWsClp.ZstergeStoResponse outParam = webServiceClp.ZstergeSto(inParam);

                    if (outParam.VOk.ToString().Equals("0"))
                        retVal = "Comanda a fost stearsa";
                    else
                        retVal = outParam.VErr.ToString();

                    webServiceClp.Dispose();

                }


            }
            catch (Exception ex)
            {



                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {

            }

            return retVal;
        }



        //verificare transfer intre filiale la clp-uri
        //pentru ca of. credite sa nu primeasca mail
        public static bool isClpTransferIntreFiliale(string nrCmd)
        {
            bool isTransfer = false;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                DateTime cDate = DateTime.Now;
                string year = cDate.Year.ToString();
                string day = cDate.Day.ToString("00");
                string month = cDate.Month.ToString("00");
                string nowDate = year + month + day;

                cmd.CommandText = " select length(cod_client) from sapprd.zclphead where id =:idClp ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idClp", OracleType.UInt32, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Convert.ToInt32(nrCmd);

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    if (oReader.GetInt32(0) == 4)
                        isTransfer = true;
                    else
                        isTransfer = false;

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



            return isTransfer;
        }


        [WebMethod]
        public string getLocalitatiJudet(string codJudet)
        {
            OperatiiAdresa operatiiAdresa = new OperatiiAdresa();
            return operatiiAdresa.getLocalitatiJudet(codJudet);
        }


        [WebMethod]
        public string getAdreseJudet(string codJudet)
        {
            OperatiiAdresa operatiiAdresa = new OperatiiAdresa();
            return operatiiAdresa.getAdreseJudet(codJudet);
        }


        [WebMethod]
        public bool isAdresaValid(string codJudet, string localitate)
        {
            OperatiiAdresa operatiiAdresa = new OperatiiAdresa();
            return operatiiAdresa.isAdresaValida(codJudet, localitate);
        }

        [WebMethod]
        public string getFilialaJudetLivrare(string codJudet)
        {
            return new OperatiiAdresa().getFilialaJudetLivrare(codJudet);
        }

        [WebMethod]
        public string getLocalitatiLivrareRapida()
        {
            return new OperatiiAdresa().getLocalitatiLivrareRapida();
        }

        [WebMethod]
        public string getDepoziteUL(string ul)
        {
            return new ComenziSiteHelper().getDepoziteUL(ul);
        }

        [WebMethod]
        public string getClientiFacturatiKA(string codAgent)
        {
            return new ClientiFacturati().getClientiFacturatiKA(codAgent);
        }

        [WebMethod]
        public string getDetaliiClFacturatiKA(string codAgent, string codClient, string data)
        {
            return new ClientiFacturati().getDetaliiClFacturatiKA(codAgent, codClient, data);
        }


        [WebMethod]
        public string getAprobariNecesare(string nrComanda)
        {
            return new Aprobari().getAprobariNecesare(nrComanda);
        }


        [WebMethod]
        public string getClientJud(string filiala)
        {

            

            string retVal = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = GetConnectionString_android();


                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " SELECT regio FROM sapprd.ZPDL_JUD WHERE mandt = '900' AND werks = :filiala order by regio ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;



                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        if (retVal.Equals(""))
                            retVal = oReader.GetString(0);
                        else
                            retVal += "," + oReader.GetString(0);
                    }

                }

                oReader.Close();
                oReader.Dispose();

            }

            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = null;
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }

            return retVal;
        }




        [WebMethod]
        public string getArticolDetAndroid(string codArticol, string filiala, string codClient, string depozit)
        {
            string retVal = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            int stocArt = 0;

            try
            {
                string connectionString = GetConnectionString_android();


                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                //stoc la zi
                cmd.CommandText = " select nvl(sum(labst),0) stoc from sapprd.mard where mandt='900'  " +
                    " and matnr=:cod and werks =:filiala and lgort=:depozit ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":cod", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filiala;

                cmd.Parameters.Add(":depozit", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = depozit;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    stocArt = oReader.GetInt32(0);
                    retVal = stocArt.ToString();
                }
                else
                {
                    retVal = "0";
                }
                //sf. stoc

                oReader.Close();
                oReader.Dispose();

                retVal = "-1";

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = null;
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }

            return retVal;
        }


        [WebMethod]
        public string getPretSimplu(string client, string articol, string cantitate, string depart, string um, string ul, string tipAcces)
        {
            string retVal = "";
            SAPWebServices.ZTBL_WEBSERVICE webService = null;

            try
            {

                webService = new ZTBL_WEBSERVICE();
                SAPWebServices.ZgetPrice inParam = new SAPWebServices.ZgetPrice();

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                webService.Credentials = nc;
                webService.Timeout = 300000;

                inParam.GvKunnr = client;
                inParam.GvMatnr = articol;
                inParam.GvSpart = depart;
                inParam.GvVrkme = um;
                inParam.GvWerks = ul;
                inParam.GvLgort = " ";
                inParam.GvCant = Decimal.Parse(cantitate);
                inParam.GvCantSpecified = true;

                SAPWebServices.ZgetPriceResponse outParam = webService.ZgetPrice(inParam);

                string pretOut = outParam.GvNetwr.ToString() != "" ? outParam.GvNetwr.ToString() : "-1";
                string umOut = outParam.GvVrkme.ToString() != "" ? outParam.GvVrkme.ToString() : "-1";
                string noDiscOut = outParam.GvNoDisc.ToString();
                string codArtPromo = outParam.GvMatnrFree.ToString() != "" ? outParam.GvMatnrFree.ToString() : "-1";
                string cantArtPromo = outParam.GvCantFree.ToString() != "" ? outParam.GvCantFree.ToString() : "-1";
                string pretArtPromo = outParam.GvNetwrFree.ToString() != "" ? outParam.GvNetwrFree.ToString() : "-1";
                string umArtPromo = outParam.GvVrkmeFree.ToString() != "" ? outParam.GvVrkmeFree.ToString() : "-1";
                string pretLista = outParam.GvNetwrList.ToString() != "" ? outParam.GvNetwrList.ToString() : "-1";
                string cantOut = outParam.GvCant.ToString() != "" ? outParam.GvCant.ToString() : "-1";
                string condPret = outParam.GvCond.ToString() != "" ? outParam.GvCond.ToString() : "-1";
                string multiplu = outParam.Multiplu.ToString() != "" ? outParam.Multiplu.ToString() : "-1";
                string pretOutGed = " ";

                //pentru directori se afiseaza si pretul ged
                if (tipAcces.Equals("12") || tipAcces.Equals("14"))
                {

                    inParam.GvKunnr = client;
                    inParam.GvMatnr = articol;
                    inParam.GvSpart = "11";
                    inParam.GvVrkme = um;
                    inParam.GvWerks = ul.Replace(ul.Substring(2, 1), "2");
                    inParam.GvCant = Decimal.Parse(cantitate);
                    inParam.GvCantSpecified = true;

                    SAPWebServices.ZgetPriceResponse outParamGed = webService.ZgetPrice(inParam);


                    pretOutGed = outParamGed.GvNetwr.ToString() != "" ? outParamGed.GvNetwr.ToString() : "-1";


                }

                retVal = cantOut + "#" + pretOut + "#" + umOut + "#" + noDiscOut + "#" + codArtPromo + "#" +
                         cantArtPromo + "#" + pretArtPromo + "#" + umArtPromo + "#" + pretLista + "#" + pretOutGed + "#";


            }

            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + " Context: " + client + " \n " + articol + " \n " + cantitate + " \n " + depart + " \n " + um + " \n " + ul + " \n " + tipAcces);
                    
                retVal = "-1";
            }
            finally
            {
                webService.Dispose();

            }

            return retVal;

        }



        [WebMethod]
        public string getPretGedJson(string parametruPret)
        {
            OperatiiArticole opArticole = new OperatiiArticole();
            return opArticole.getPretGed(parametruPret);
        }


        [WebMethod]
        public string getPretGed(string client, string articol, string cantitate, string depart, string um, string ul, string depoz, string codUser)
        {


            string retVal = "";
            SAPWebServices.ZTBL_WEBSERVICE webService = null;
            string localUnitLog = "";



            try
            {

                webService = new ZTBL_WEBSERVICE();
                SAPWebServices.ZgetPrice inParam = new SAPWebServices.ZgetPrice();

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());

                webService.Credentials = nc;
                webService.Timeout = 300000;

                localUnitLog = ul;

                //preturi ged doar pe ul 20
                if (depart == "11" && !ul.Contains("4"))
                    localUnitLog = ul.Substring(0, 2) + "2" + ul.Substring(3, 1);

                inParam.GvKunnr = client;
                inParam.GvMatnr = articol;
                inParam.GvSpart = depart;
                inParam.GvVrkme = um;
                inParam.GvWerks = localUnitLog;
                inParam.GvLgort = depoz;
                inParam.GvCant = Decimal.Parse(cantitate);
                inParam.GvCantSpecified = true;
                inParam.GvSite = isConsVanzSite(codUser) ? "X" : " ";
                inParam.TipPers = "CV";
                inParam.Canal = "20";

                SAPWebServices.ZgetPriceResponse outParam = webService.ZgetPrice(inParam);

                string pretOut = outParam.GvNetwr.ToString() != "" ? outParam.GvNetwr.ToString() : "-1";
                string umOut = outParam.GvVrkme.ToString() != "" ? outParam.GvVrkme.ToString() : "-1";
                string noDiscOut = outParam.GvNoDisc.ToString();


                string codArtPromo = outParam.GvMatnrFree.ToString() != "" ? outParam.GvMatnrFree.ToString() : "-1";
                string cantArtPromo = outParam.GvCantFree.ToString() != "" ? outParam.GvCantFree.ToString() : "-1";
                string pretArtPromo = outParam.GvNetwrFree.ToString() != "" ? outParam.GvNetwrFree.ToString() : "-1";
                string umArtPromo = outParam.GvVrkmeFree.ToString() != "" ? outParam.GvVrkmeFree.ToString() : "-1";
                string pretLista = outParam.GvNetwrList.ToString() != "" ? outParam.GvNetwrList.ToString() : "-1";
                string cantOut = outParam.GvCant.ToString() != "" ? outParam.GvCant.ToString() : "-1";
                string condPret = outParam.GvCond.ToString() != "" ? outParam.GvCond.ToString() : "-1";
                string multiplu = outParam.Multiplu.ToString() != "" ? outParam.Multiplu.ToString() : "-1";
                string cantUmb = outParam.OutCantUmb.ToString() != "" ? outParam.OutCantUmb.ToString() : "-1";
                string Umb = outParam.OutUmb.ToString() != "" ? outParam.OutUmb.ToString() : "-1";


                //---verificare cmp

                OracleConnection connection = new OracleConnection();
                OracleCommand cmd = new OracleCommand();
                OracleDataReader oReader = null;

                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select nvl(to_char(decode(y.lbkum,0,y.verpr,y.salk3/y.lbkum),'99999.9999'),0) from sapprd.mbew y where " +
                                  " y.mandt='900' and y.matnr=:matnr  and y.bwkey = :unitLog  ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":matnr", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = articol;

                cmd.Parameters.Add(":unitLog", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = ul;   //ul. ged

                oReader = cmd.ExecuteReader();
                double cmpArticol = 0;
                if (oReader.HasRows)
                {
                    oReader.Read();
                    cmpArticol = Convert.ToDouble(oReader.GetString(0));



                    // daca nu este in ged se cauta in distributie
                    if (cmpArticol == 0)
                    {
                        cmd.CommandText = " select nvl(to_char(decode(y.lbkum,0,y.verpr,y.salk3/y.lbkum),'99999.9999'),0) from sapprd.mbew y where " +
                                          " y.mandt='900' and y.matnr=:matnr  and y.bwkey = :unitLog ";

                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(":matnr", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = articol;

                        cmd.Parameters.Add(":unitLog", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                        cmd.Parameters[1].Value = ul.Substring(0, 2) + "1" + ul.Substring(3, 1);      //ul. distributie

                        oReader = cmd.ExecuteReader();

                        oReader.Read();
                        cmpArticol = Convert.ToDouble(oReader.GetString(0));

                    }


                }

                //---sf. verificare cmp


                retVal = cantOut + "#" + pretOut + "#" + umOut + "#" + noDiscOut + "#" + codArtPromo + "#" +
                         cantArtPromo + "#" + pretArtPromo + "#" + umArtPromo + "#" + pretLista + "#";


                //descriere conditii pret
                string[] codReduceri = condPret.Split(';');
                string[] tokCod;


                //conditii pret, fara descriere, doar coduri
                condPret = "";

                for (int jj = 0; jj < codReduceri.Length; jj++)
                {

                    tokCod = codReduceri[jj].Split(':');

                    if (tokCod.Length > 1)
                    {
                        condPret += tokCod[0] + ":" + tokCod[1] + ";";
                    }


                }//sf. for               


                retVal += condPret + "#";



                //discounturi maxime
                string discMaxAV = "0", discMaxSD = "0", discMaxDV = "0", discMaxKA = "0";
                //sf. disc

                //pret si adaos mediu
                string pretMediu = ((Double.Parse(pretOut) / 1.20) / Double.Parse(cantOut) * Double.Parse(multiplu)).ToString(), adaosMediu = "0", unitMasPretMediu = "0";
                cmd = connection.CreateCommand();
                cmd.CommandText = " select to_char(pret_med/cant, '99990.999')  , to_char(adaos_med/cant,'99990.999')  , um from sapprd.zpret_mediu r where mandt = '900' and pdl =:unitLog " +
                                  " and matnr=:articol ";



                cmd.Parameters.Clear();
                cmd.Parameters.Add(":unitLog", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = ul.Substring(0, 2) + "1" + ul.Substring(3, 1);

                cmd.Parameters.Add(":articol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = articol;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    pretMediu = oReader.GetString(0).Trim();
                    adaosMediu = oReader.GetString(1).Trim();
                    unitMasPretMediu = oReader.GetString(2);
                }



                oReader.Close();
                oReader.Dispose();
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

                //sf. prt si adaos mediu


                retVal += discMaxAV + "#" + discMaxSD + "#" + discMaxDV + "#" +
                          Convert.ToInt32(Double.Parse(multiplu)).ToString() + "#" +
                          cantUmb + "#" + Umb + "#" + discMaxKA + "#" + cmpArticol.ToString() + "#" + pretMediu + "#" + adaosMediu + "#" + unitMasPretMediu + "#";

                if (pretOut.Equals("0.0"))
                    retVal = "-1";



            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                webService.Dispose();

            }





            return retVal;
        }


        [WebMethod]
        public string getArtCmdRapVanz(string nrDoc, string tipCmd)
        {
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {


                string sqlString = "";

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();


                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                List<ArticolVanzari> listaArticole = new List<ArticolVanzari>();
                ArticolVanzari unArticol = null;


                if (tipCmd.Equals("E")) //comenzi emise
                {

                    string localDoc = nrDoc.Substring(4, 10);

                    sqlString = " select  decode(length(b.matnr),18,substr(b.matnr,-8),b.matnr) cod,ar.nume matdesc , " +
                                       " decode(a.fkart,'ZFRA',0,'ZFRB',0,decode(b.shkzg,'X',-b.fklmg,b.fklmg)) cant, " +
                                       " decode(b.shkzg,'X',-1,1)*(b.netwr+b.mwsbp)*b.kursk valoare " +
                                       " from  sapprd.vbrk a, sapprd.vbrp b, articole ar where a.mandt='900' and b.mandt='900' and a.vbeln=b.vbeln and b.vbeln =:idcmd and " +
                                       " b.matnr=ar.cod order by b.matnr ,ar.nume asc ";

                    cmd.CommandText = sqlString;

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idcmd", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = localDoc;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {

                            unArticol = new ArticolVanzari();
                            unArticol.codArticol = oReader.GetString(0);
                            unArticol.numeArticol = oReader.GetString(1);
                            unArticol.cantitateArticol = oReader.GetDouble(2).ToString();
                            unArticol.valoareArticol = oReader.GetDouble(3).ToString();
                            listaArticole.Add(unArticol);

                        }

                    }

                }


                if (tipCmd.Equals("A")) //comenzi anulate
                {
                    sqlString = " select  decode(length(a.cod),18,substr(a.cod,-8),a.cod) cod, ar.nume matdesc , " +
                                       " a.cantitate, " +
                                       " a.cantitate * (a.valoare / a.multiplu) valoare " +
                                       " from  sapprd.zcomdet_tableta a, articole ar, sapprd.zcomhead_tableta t where a.mandt='900' " +
                                       " and t.mandt = '900' and a.id = t.id and t.nrcmdsap =:idcmd and " +
                                       " a.cod = ar.cod order by a.poz ";

                    cmd.CommandText = sqlString;

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idcmd", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = nrDoc;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {

                            unArticol = new ArticolVanzari();
                            unArticol.codArticol = oReader.GetString(0);
                            unArticol.numeArticol = oReader.GetString(1);
                            unArticol.cantitateArticol = oReader.GetDouble(2).ToString();
                            unArticol.valoareArticol = oReader.GetDouble(3).ToString();
                            listaArticole.Add(unArticol);

                        }

                    }

                }



                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaArticole);


            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }


            return serializedResult;
        }

        [WebMethod]
        public string getRaportNeincasateData(string reportParams, string filiala, string tipUserSap)
        {
            Neincasate neincasate = new Neincasate();
            return neincasate.getRaportNeincasateData(reportParams, filiala, tipUserSap);
        }


        [WebMethod]
        public string getArtDocNeincasat(string nrDocument)
        {
            Neincasate neincasate = new Neincasate();
            return neincasate.getArticoleDocumentNeincasat(nrDocument);
        }


        [WebMethod]
        public string getListaMaterialeNecesar(string filiala, string departament)
        {
            OperatiiNecesar necesar = new OperatiiNecesar();
            return necesar.getMaterialeNecesar(filiala, departament);

        }

        [WebMethod]
        public string getFacturiNeincasate(string codClient, string codDepart, string codAgent, string unitLog)
        {
            return new OperatiiDocumente().getFacturiNeincasate(codClient, codDepart, codAgent, unitLog);
        }


        [WebMethod]
        public string salveazaPlataNeincasata(string plataNeincasata)
        {
            return new OperatiiDocumente().salveazaPlataNeincasata(plataNeincasata);
        }

        [WebMethod]
        public string getStareComanda(string nrComanda)
        {
           return new OperatiiDocumente().getStareDocument(nrComanda);
           
        }

        [WebMethod]
        public bool savePinTableta(string codAgent, string codPin)
        {
            return MeniuTableta.savePinTableta(codAgent, codPin);
        }

        [WebMethod]
        public bool deblocheazaMeniu(string codAgent, string codPin)
        {
            return MeniuTableta.deblocheazaMeniu(codAgent, codPin);
        }

        [WebMethod]
        public bool blocheazaMeniu(string codAgent)
        {
            return MeniuTableta.blocheazaMeniu(codAgent);
        }


        [WebMethod]
        public string getRaportClientiInactivi(string reportParams)
        {

            string serializedResult = "";
            string sqlString = "";

            string[] mainToken = reportParams.Split('@');
            string agent = mainToken[0];
            string filiala = mainToken[1];
            string depart = mainToken[2];


            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();



                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();



                sqlString = " select distinct r.name1,r.kunnr,r.pernr,r.rosu, c.tip, c.tip_041, ag.divizie from sapprd.zclienti_rosii r, agenti ag, clie_tip c " +
                            " where r.pernr=:codAgent and ag.cod = r.pernr and c.canal ='10' and c.cod_cli = r.kunnr " +
                            " and c.tip in ('02','03','04','05','06') and c.depart = substr(ag.divizie,0,2) and substr(r.spart,0,2) = substr(ag.divizie,0,2) " +
                            " order by r.rosu,r.name1 ";

                cmd.CommandText = sqlString;



                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = agent;

                oReader = cmd.ExecuteReader();

                List<ClientInactiv> listaClienti = new List<ClientInactiv>();
                ClientInactiv unClient = null;

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        unClient = new ClientInactiv();
                        unClient.numeClient = oReader.GetString(0);
                        unClient.codClient = oReader.GetString(1);
                        unClient.codAgent = oReader.GetString(2);
                        unClient.stareClient = oReader.GetString(3);

                        if (oReader.GetString(6).Equals("041"))
                        {
                            unClient.tipClient = oReader.GetString(5);
                        }
                        else
                        {
                            unClient.tipClient = oReader.GetString(4);
                        }


                        listaClienti.Add(unClient);

                    }
                }


                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaClienti);

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }
            finally
            {
                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }

            return serializedResult;

        }





        [WebMethod]
        public string getRaportVanzariAgData(string reportParams)
        {

            var serializer = new JavaScriptSerializer();

            VanzariAgentiParam parametru = serializer.Deserialize<VanzariAgentiParam>(reportParams);
            List<string> clienti = serializer.Deserialize<List<string>>(parametru.clienti.ToString());
            List<string> articole = serializer.Deserialize<List<string>>(parametru.articole.ToString());

            string raportData = "";
            string sqlString = "";
            string[] mainToken = reportParams.Split('@');

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string tipArticol = parametru.tipArticol;


            string agent = parametru.agent;
            string filiala = parametru.filiala;
            string depart = parametru.departament;
            string[] varDataStart = parametru.startInterval.Split('.');
            string[] varDataStop = parametru.stopInterval.Split('.');
            string tipComanda = parametru.tipComanda;



            string listArticole = "('0000000000" + String.Join("','", articole.ToArray()) + "')";
            string listClienti = "('" + String.Join("','", clienti.ToArray()) + "')";
            string listFiliale = " and a.prefix in ('" + filiala + "','" + filiala.Substring(0, 2) + "2" + filiala.Substring(3, 1) + "','" + filiala.Substring(0, 2) + "3" + filiala.Substring(3, 1) + "')";

            if (parametru.tipUserSap != null && (parametru.tipUserSap.Equals("SDIP") || parametru.tipUserSap.Equals("CVIP")))
                listFiliale = "";

            string dataStart = varDataStart[2] + varDataStart[1] + varDataStart[0];
            string dataStop = varDataStop[2] + varDataStop[1] + varDataStop[0];



            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();


                if (tipComanda.Equals("E")) //comenzi emise
                {


                    string conditieClienti = "";
                    if (clienti.Count() > 0)
                    {
                        conditieClienti = " and k.cod in " + listClienti;
                    }

                    string conditieTipAgenti = "", conditieDepart = "";
                    if (depart.Equals("11"))    //consilieri
                    {
                        if (agent.Equals("0"))
                            conditieTipAgenti = " and  v.parvw = 'ZC' and ag.filiala = '" + filiala + "' and ag.divizie='11' ";
                        else
                            conditieTipAgenti = " and  v.parvw in ('ZC','VE') ";


                        conditieDepart = " ";
                    }
                    else   //agenti
                    {
                        conditieTipAgenti = " and v.parvw='VE' and upper(ag.nume) not like '%IMPUTATII%'  and  " +
                                             " ( substr(divizie,0,2) like '" + depart.Substring(0, 2) + "%' or substr(old_div,0,2) like '" + depart.Substring(0, 2) + "%')";

                        if (!depart.Equals("10"))
                            conditieDepart = " and (b.spart like '" + depart.Substring(0, 2) + "%' or b.spart = '11' ) ";

                    }

                    string conditieAgent = "";
                    if (!agent.Equals("0"))
                    {
                        conditieAgent = " and v.pernr in ('" + agent + "') ";
                        conditieDepart = " ";
                    }


                    connection.ConnectionString = connectionString;
                    connection.Open();

                    cmd = connection.CreateCommand();


                    //toate articolele, afisare pe document
                    if (articole.Count == 0)
                    {
                        sqlString = " select a.xblnr, k.nume , to_char(to_date(a.fkdat,'yyyymmdd')) emitere,  " +
                                     " ag.nume agnume ,a.knkli,  sum(decode(b.shkzg,'X',-1,1)*(b.netwr+b.mwsbp)) valoare  from sapprd.vbrk a, " +
                                     " sapprd.vbpa v, clienti k, agenti ag,sapprd.vbrp b where a.mandt='900' and v.mandt='900' and b.mandt='900'  " +
                                      listFiliale + " and v.vbeln=a.vbeln and b.vbeln = a.vbeln  and a.knkli=k.cod " + conditieClienti +
                                      conditieDepart + " and a.fksto=' ' and a.fkart not in ('ZF5','ZS1D','ZS2','ZS2C','ZS1','ZFA','ZFAS','ZFAD','ZF5S') and a.fkdat between " +
                                     " '" + dataStart + "' and '" + dataStop + "' " +
                                     " and b.mandt='900' and b.vbeln=a.vbeln and v.pernr=ag.cod " + conditieTipAgenti + conditieAgent +
                                     " group by a.xblnr, k.nume, a.fkdat, ag.nume  ,a.knkli " +
                                     " order by ag.nume, k.nume,a.fkdat  ";




                    }
                    else
                    {
                        if (tipArticol.Equals("S"))
                        {

                            sqlString = "select b.matkl cod,st.nume matdesc ,ag.nume agnume, sum(decode(a.fkart,'ZFRA',0,'ZFRB',0,decode(b.shkzg,'X',-b.fklmg,b.fklmg))) cant, " +
                                        " sum(decode(b.shkzg,'X',-1,1)*(b.netwr+b.mwsbp)*b.kursk) valoare from sapprd.vbrk a, sapprd.vbrp b, sapprd.vbpa v,sintetice st, " +
                                        " articole ar,agenti ag where a.mandt='900' and b.mandt='900' and v.mandt='900'  " + listFiliale +
                                        " and v.vbeln=a.vbeln and ag.cod(+)=v.pernr and a.fksto=' ' and a.fkart not in ('ZF5','ZS1D','ZS2','ZS2C','ZS1','ZFA','ZFAS','ZF5S') " +
                                        " and a.fkdat between '" + dataStart + "' and '" + dataStop + "' and b.vbeln=a.vbeln " + conditieTipAgenti + conditieAgent +
                                        " and b.matnr=ar.cod and ar.sintetic=st.cod and ar.sintetic in " + listArticole + " group by b.matkl , " +
                                        " st.nume ,v.pernr,ag.nume,b.meins order by agnume , b.matkl ,st.nume asc ";
                        }

                        if (tipArticol.Equals("A"))
                        {
                            sqlString = "select decode(length(b.matnr),18,substr(b.matnr,-8),b.matnr) cod,ar.nume matdesc ,ag.nume agnume, " +
                                         " sum(decode(a.fkart,'ZFRA',0,'ZFRB',0,decode(b.shkzg,'X',-b.fklmg,b.fklmg))) cant, sum(decode(b.shkzg,'X',-1,1)*(b.netwr+b.mwsbp)*b.kursk) valoare " +
                                         " from sapprd.vbrk a, sapprd.vbrp b, sapprd.vbpa v,sintetice st,articole ar,agenti ag where a.mandt='900' " +
                                         " and b.mandt='900' and v.mandt='900' " + listFiliale + " and v.vbeln=a.vbeln " +
                                         " and ag.cod(+)=v.pernr and a.fksto=' ' and a.fkart not in ('ZF5','ZS1D','ZS2','ZS2C','ZS1','ZFA','ZFAS','ZF5S') " +
                                         " and a.fkdat between '" + dataStart + "' and '" + dataStop + "' " +
                                         " and b.vbeln=a.vbeln " + conditieTipAgenti + conditieAgent +
                                         " and b.matnr=ar.cod and " +
                                         " ar.sintetic=st.cod and b.matnr in " + listArticole + " group by b.matnr ,ar.nume ,v.pernr,ag.nume,b.meins order by agnume , b.matnr ,ar.nume asc ";
                        }

                    }






                    cmd.CommandText = sqlString;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {

                            if (articole.Count == 0)
                            {
                                raportData += oReader.GetString(0) + "#" + Regex.Replace(oReader.GetString(1), @"[!]|[#]|[@@]|[,]", " ") + "#" + oReader.GetString(2) + "#" +
                                            Regex.Replace(oReader.GetString(3), @"[!]|[#]|[@@]|[,]", " ") + "#" + oReader.GetString(4) + "#" + oReader.GetDouble(5).ToString() + "@@";
                            }
                            else
                            {
                                raportData += oReader.GetString(0) + "#" + Regex.Replace(oReader.GetString(1), @"[!]|[#]|[@@]|[,]", " ") + "#" +
                                    Regex.Replace(oReader.GetString(2), @"[!]|[#]|[@@]|[,]", " ") + "#" +
                                    oReader.GetDouble(3).ToString() + "#" + oReader.GetDouble(4).ToString() + "@@";
                            }

                        }




                    }
                    else
                    {
                        raportData = "-1";
                    }


                }//sf. comenzi emise
                else    //comenzi anulate
                {


                    string conditieClienti = "";
                    if (clienti.Count > 0)
                    {
                        conditieClienti = " and a.cod_client in " + listClienti;
                    }

                    string conditieTipAgenti = "";
                    if (depart.Equals("11"))    //consilieri
                    {
                        if (agent.Equals("0"))
                            conditieTipAgenti = " and c.filiala = '" + filiala + "' and c.divizie='11' ";

                    }
                    else   //agenti
                    {
                        conditieTipAgenti = " and upper(c.nume) not like '%IMPUTATII%' and c.filiala = '" + filiala + "' and  " +
                                            " (c.divizie like '" + depart + "%' or c.old_div like '" + depart + "%')";


                    }

                    string conditieAgent = "";
                    if (!agent.Equals("0"))
                    {
                        conditieAgent = " and a.cod_agent in ('" + agent + "') ";
                        conditieTipAgenti = " ";
                    }



                    connection.ConnectionString = connectionString;
                    connection.Open();

                    cmd = connection.CreateCommand();

                    sqlString = " select  r.*  from ( select a.nrcmdsap,d.nume nume_client,to_char(to_date(a.datac,'yyyymmdd')) data,  c.nume nume_agent,d.cod, a.valoare " +
                                " from sapprd.zcomhead_tableta a,  agenti c,  clienti d where a.datac between '" + dataStart + "' and '" + dataStop + "'  and a.status in ('6') " +
                                " and (a.accept1 = 'X' or a.accept2 = 'X') and a.ADR_NOUA != 'X' and a.abgru is null and a.cod_agent = c.cod " + conditieClienti +
                                  conditieTipAgenti + conditieAgent +
                                " and a.cod_client=d.cod " +
                                " and length(a.nrcmdsap) > 1 ) r, sapprd.zcomhead_tableta t where t.com_referinta (+) = r.nrcmdsap and t.id is null order by nume_agent, data ";



                    cmd.CommandText = sqlString;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {

                            raportData += oReader.GetString(0) + "#" + Regex.Replace(oReader.GetString(1), @"[!]|[#]|[@@]|[,]", " ") + "#" + oReader.GetString(2) + "#" +
                                           Regex.Replace(oReader.GetString(3), @"[!]|[#]|[@@]|[,]", " ") + "#" + oReader.GetString(4) + "#" + oReader.GetDouble(5).ToString() + "@@";

                        }


                    }
                    else
                    {
                        raportData = "-1";
                    }



                }//sf. comenzi anulate


            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                raportData = "-1";
            }
            finally
            {
                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }

            return raportData;
        }



        public static string extindeClient(string client)
        {

            string response = "1";

            SAPWebServices.ZTBL_WEBSERVICE webService = null;
            webService = new ZTBL_WEBSERVICE();
            SAPWebServices.ZextindeClient inParam = new SAPWebServices.ZextindeClient();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());

            webService.Credentials = nc;
            webService.Timeout = 1200000;

            inParam.VKunnr = client;
            inParam.VSpart = "11";
            inParam.VVtweg = "20";

            SAPWebServices.ZextindeClientResponse outParam = webService.ZextindeClient(inParam);
            response = outParam.VOk;

            return response;

        }


        [WebMethod]
        public string getPret(string client, string articol, string cantitate, string depart, string um, string ul, string tipUser, string depoz, string codUser, string canalDistrib, string filialaAlternativa, string filialaClp)
        {
            Preturi preturi = new Preturi();
            return preturi.getPret(client, articol, cantitate, depart, um, ul, tipUser, depoz, codUser, canalDistrib, filialaAlternativa, filialaClp);

        }


        [WebMethod]
        public string getPret_old(string client, string articol, string cantitate, string depart, string um, string ul, string tipUser, string depoz, string codUser, string canalDistrib, string filialaAlternativa)
        {
            string retVal = "";
            SAPWebServices.ZTBL_WEBSERVICE webService = null;

            string tipUserLocal;

            if (tipUser == null || (tipUser != null && tipUser.Trim().Length == 0))
            {
                tipUserLocal = getTipUser(codUser);
            }
            else
            {
                tipUserLocal = tipUser;
            }



            try
            {

                webService = new ZTBL_WEBSERVICE();
                SAPWebServices.ZgetPrice inParam = new SAPWebServices.ZgetPrice();

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());

                webService.Credentials = nc;
                webService.Timeout = 300000;

                inParam.GvKunnr = client;
                inParam.GvMatnr = articol;
                inParam.GvSpart = depart;
                inParam.GvVrkme = um;
                inParam.GvWerks = ul;
                inParam.GvLgort = depoz;
                inParam.GvCant = Decimal.Parse(cantitate);
                inParam.GvCantSpecified = true;
                inParam.GvSite = " ";
                inParam.TipPers = tipUserLocal;
                inParam.Canal = canalDistrib;


                SAPWebServices.ZgetPriceResponse outParam = webService.ZgetPrice(inParam);

                string pretOut = outParam.GvNetwr.ToString() != "" ? outParam.GvNetwr.ToString() : "-1";
                string umOut = outParam.GvVrkme.ToString() != "" ? outParam.GvVrkme.ToString() : "-1";
                string noDiscOut = outParam.GvNoDisc.ToString();
                string codArtPromo = outParam.GvMatnrFree.ToString() != "" ? outParam.GvMatnrFree.ToString() : "-1";
                string cantArtPromo = outParam.GvCantFree.ToString() != "" ? outParam.GvCantFree.ToString() : "-1";
                string pretArtPromo = outParam.GvNetwrFree.ToString() != "" ? outParam.GvNetwrFree.ToString() : "-1";
                string umArtPromo = outParam.GvVrkmeFree.ToString() != "" ? outParam.GvVrkmeFree.ToString() : "-1";
                string pretLista = outParam.GvNetwrList.ToString() != "" ? outParam.GvNetwrList.ToString() : "-1";
                string cantOut = outParam.GvCant.ToString() != "" ? outParam.GvCant.ToString() : "-1";
                string condPret = outParam.GvCond.ToString() != "" ? outParam.GvCond.ToString() : "-1";
                string multiplu = outParam.Multiplu.ToString() != "" ? outParam.Multiplu.ToString() : "-1";
                string cantUmb = outParam.OutCantUmb.ToString() != "" ? outParam.OutCantUmb.ToString() : "-1";
                string Umb = outParam.OutUmb.ToString() != "" ? outParam.OutUmb.ToString() : "-1";


                string extindere11 = outParam.ErrorCode.ToString();

                if (depart.Equals("11") && extindere11.Equals("1"))
                {
                    if (extindeClient(client).Equals("0"))
                    {
                        return getPret(client, articol, cantitate, depart, um, ul, tipUserLocal, depoz, codUser, canalDistrib, filialaAlternativa, null);
                    }
                    else
                    {
                        return "-1";
                    }
                }


                //---verificare cmp

                OracleConnection connection = new OracleConnection();
                OracleCommand cmd = new OracleCommand();
                OracleDataReader oReader = null;

                string filialaCmp = filialaAlternativa;

                if (depart.Equals("11"))
                {
                    if (filialaAlternativa.Equals("BV90"))
                        filialaCmp = "BV92";
                    else
                        filialaCmp = filialaAlternativa.Substring(0, 2) + "2" + filialaAlternativa.Substring(3, 1);
                }

                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select nvl(to_char(decode(y.lbkum,0,y.verpr,y.salk3/y.lbkum),'99999.9999'),0) from sapprd.mbew y where " +
                                  " y.mandt='900' and y.matnr=:matnr  and y.bwkey = :unitLog ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":matnr", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = articol;

                cmd.Parameters.Add(":unitLog", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filialaCmp;



                oReader = cmd.ExecuteReader();
                double cmpArticol = 0;
                if (oReader.HasRows)
                {
                    oReader.Read();
                    cmpArticol = Convert.ToDouble(oReader.GetString(0));
                }

                //---sf. verificare cmp


                retVal = cantOut + "#" + pretOut + "#" + umOut + "#" + noDiscOut + "#" + codArtPromo + "#" +
                         cantArtPromo + "#" + pretArtPromo + "#" + umArtPromo + "#" + pretLista + "#";



                //descriere conditii pret
                string[] codReduceri = condPret.Split(';');
                string[] tokCod;



                condPret = "";
                for (int jj = 0; jj < codReduceri.Length; jj++)
                {

                    tokCod = codReduceri[jj].Split(':');


                    cmd = connection.CreateCommand();

                    //stoc la zi
                    cmd.CommandText = " SELECT vtext FROM SAPPRD.T685t r where mandt = '900' and spras = '4' " +
                                      " and r.kvewe = 'A' and r.kappl = 'V' and KSCHL=:codRed ";

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":codRed", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = tokCod[0];

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();

                        condPret += oReader.GetString(0) + ":" + tokCod[1] + ";";
                    }
                    else
                    {
                        condPret += "Taxa verde:0,00;Pret net:0,00;TVA încasat:0,00;";
                    }


                }//sf. for

                retVal += condPret + "#";

                oReader.Close();
                oReader.Dispose();


                //

                //discounturi maxime
                string discMaxAV = "0", discMaxSD = "0", discMaxDV = "0", discMaxKA = "0";

                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct nvl(a.discount,0) av, " +
                                  " nvl((select distinct discount from sapprd.zdisc_pers_sint where  functie='SD' and spart =:depart and werks =:filiala and inactiv <> 'X' and matkl = c.cod),0) sd, " +
                                  " nvl((select distinct discount from sapprd.zdisc_pers_sint where  functie='DV' and spart =:depart and werks =:filiala and inactiv <> 'X' and matkl = c.cod),0) dv, " +
                                  " nvl((select distinct discount from sapprd.zdisc_pers_sint where  functie='KA' and werks =:filiala and inactiv <> 'X' and matkl = c.cod),0) ka " +
                                  " from sapprd.zdisc_pers_sint a, articole b, sintetice c where  a.functie='AV' and a.spart =:depart and a.werks =:filiala " +
                                  " and b.sintetic = c.cod and inactiv <> 'X' and matkl = c.cod and b.cod =:cod ";




                cmd.Parameters.Clear();
                cmd.Parameters.Add(":cod", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = articol;

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = ul;

                cmd.Parameters.Add(":depart", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = depart;


                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    discMaxAV = oReader.GetDouble(0).ToString();
                    discMaxSD = oReader.GetDouble(1).ToString();
                    discMaxDV = oReader.GetDouble(2).ToString();
                    discMaxKA = oReader.GetDouble(3).ToString();
                }




                //sf. disc.

                //KA - la preturile promotionale nu se mai aplica alte discounturi
                if (noDiscOut.Equals("X"))
                {
                    discMaxKA = "0";
                }


                //pret mediu oras
                string pretMediu = "0";

                if (tipUserLocal.Equals("KA"))
                {
                    cmd.CommandText = " select pret_med, adaos_med, cant from sapprd.zpret_mediu_oras where matnr =:articol and pdl=:ul ";

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":articol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = articol;

                    cmd.Parameters.Add(":ul", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = ul;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        double dblPretMediu = oReader.GetDouble(0) / oReader.GetDouble(2) + (oReader.GetDouble(1) / oReader.GetDouble(2)) * 0.15;
                        pretMediu = dblPretMediu.ToString();

                    }

                }


                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();
                connection.Close();
                connection.Dispose();

                retVal += discMaxAV + "#" + discMaxSD + "#" + discMaxDV + "#" +
                         Convert.ToInt32(Double.Parse(multiplu)).ToString() + "#" +
                         cantUmb + "#" + Umb + "#" + discMaxKA + "#" + cmpArticol.ToString() + "#" + pretMediu + "#";




                if (pretOut.Equals("0.0"))
                    retVal = "-1";



            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                webService.Dispose();

            }

            return retVal;
        }

        [WebMethod]
        public string getDatePersonale(string numeClient, string tipClient)
        {
            return new OperatiiClienti().getDatePersonaleClient(numeClient, tipClient);
        }

        [WebMethod]
        public string getDivizieBV90(string codArticol)
        {
            return OperatiiArticole.getDivizieArticolBV90(codArticol);
        }


        public string saveKANewCmd(string comanda, bool alertSD, bool alertDV, bool cmdAngajament, string tipUser, string JSONArt, string JSONComanda, string JSONDateLivrare, bool calcTransport)
        {
            string retVal = "-1";

            OracleConnection connection = new OracleConnection();
            OracleParameter idCmd = null;

            var serializer = new JavaScriptSerializer();


            ComandaVanzare comandaVanzare = serializer.Deserialize<ComandaVanzare>(JSONComanda);
            DateLivrare dateLivrare = serializer.Deserialize<DateLivrare>(JSONDateLivrare);
            List<ArticolComanda> articolComanda = serializer.Deserialize<List<ArticolComanda>>(JSONArt);
            bool hasDepozMAV1 = false;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                DateTime cDate = DateTime.Now;
                string year = cDate.Year.ToString();
                string day = cDate.Day.ToString("00");
                string month = cDate.Month.ToString("00");
                string nowDate = year + month + day;
                string nowTime = cDate.Hour.ToString("00") + cDate.Minute.ToString("00") + cDate.Second.ToString("00");
                string uLog = "", transp = "";
                string cmdStatus = "0", unitLogAlt = "NN10";
                string refSAP = "0";
                string codClient = "", depart = "";

                string conditieID = "", termenPlata = "", observatiiLivr = "";
                connection.ConnectionString = connectionString;
                connection.Open();

                string tipCmd = "0";

                string query = "";

                cmdStatus = comandaVanzare.comandaBlocata;

                if (cmdAngajament)
                {
                    tipCmd = "11";
                    cmdStatus = "6";
                    alertDV = true;
                }

                if (dateLivrare.isComandaACZC)
                    cmdStatus = "1";

         

                unitLogAlt = comandaVanzare.filialaAlternativa;
                codClient = comandaVanzare.codClient;


                string codArtDepart = articolComanda[0].codArticol;

                if ((codArtDepart.StartsWith("30") || codArtDepart.StartsWith("00")) && articolComanda.Count >= 2)
                    codArtDepart = articolComanda[1].codArticol;

                if ((codArtDepart.StartsWith("30") || codArtDepart.StartsWith("00")) && articolComanda.Count >= 3)
                    codArtDepart = articolComanda[2].codArticol;


                if (codArtDepart.Length == 8)
                    codArtDepart = "0000000000" + codArtDepart;

                OracleCommand cmd = connection.CreateCommand();
                cmd.CommandText = " select grup_vz from articole where cod =:codArt ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArtDepart;

                OracleDataReader oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    depart = oReader.GetString(0);
                }
                oReader.Close();
                oReader.Dispose();


                if (articolComanda[0].depozit.ToUpper().Contains("MAV") && articolComanda[0].departAprob != null && !articolComanda[0].departAprob.Equals("00"))
                {
                    depart = articolComanda[0].departAprob;
                }





                //BV90 doar pentru electrice
                if (unitLogAlt == "BV90")
                {
                    //exceptie 
                    if (depart.Equals("05") || depart.Equals("02") || depart.Equals("11"))
                    {

                    }
                    else
                    {
                        retVal = "-1";
                        return retVal;
                    }

                }


                //comanda care se sterge, daca e modificare comanda
                refSAP = comandaVanzare.nrCmdSap.Trim().Length == 0 ? "-1" : comandaVanzare.nrCmdSap;
                conditieID = comandaVanzare.conditieID;

                {
                    termenPlata = dateLivrare.termenPlata;
                    observatiiLivr = dateLivrare.obsLivrare;
                }




                

                DateTime dataLivrare;
                if (dateLivrare.dataLivrare.Length == 1)
                    dataLivrare = DateTime.Today.AddDays(Double.Parse(dateLivrare.dataLivrare));
                else
                    dataLivrare = GeneralUtils.getDateFromString(dateLivrare.dataLivrare);

                //pt. calcul pret transport
                uLog = dateLivrare.unitLog;

                transp = dateLivrare.Transport;

                //inserare articole comanda
                string[] alerte;
                string globalStatusCmd = "0", statAprv = "0";

                //int artLen = mainToken.Length - 2;
                int pozArt = 10;
                double totalComanda = 0, totalGen = 0;
                string newDepart = "", parentId = "", lastCmdId = "", alerteSD = "", alerteDV = "", valAcc1 = " ", valAcc2 = " ", codArticol = " ";
                string localDepart = "00", codUltimArticol = " ", depozUltimArticol = " ", departAprobUltimArticol = "00";

                cmd = connection.CreateCommand();

                int artLen = articolComanda.Count;

                NumberFormatInfo nfi = new NumberFormatInfo();
                nfi.NumberDecimalSeparator = ".";
                nfi.NumberGroupSeparator = string.Empty;

                hasDepozMAV1 = false;

                String ulStoc = "";

                for (int i = 0; i < artLen; i++)
                {

                    alerte = comandaVanzare.alerteKA.Split('!');
                    alerteSD = alerte[0];
                    alerteDV = alerte[1];

                    if (!newDepart.Equals(articolComanda[i].depart))
                    {

                        //actualizare total comanda veche
                        if (totalComanda != 0)
                        {

                            //departament comanda
                            cmd.CommandText = " select grup_vz from articole where cod =:codArt ";

                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.Clear();

                            cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                            cmd.Parameters[0].Value = codUltimArticol;

                            oReader = cmd.ExecuteReader();
                            localDepart = "00";
                            if (oReader.HasRows)
                            {
                                oReader.Read();
                                localDepart = oReader.GetString(0);
                            }
                            oReader.Close();
                            oReader.Dispose();

                            if (depozUltimArticol.Contains("MAV") && !departAprobUltimArticol.Equals("00"))
                                localDepart = departAprobUltimArticol;

                            //sf. departament


                            query = " update sapprd.zcomhead_tableta set parent_id =:parentId, valoare =:valCmd, depart =:depart, accept1 =:acc1, accept2 =:acc2, status_aprov =:statAprv where id =:idCmd ";


                            cmd.CommandText = query;
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Clear();

                            cmd.Parameters.Add(":parentId", OracleType.Number, 11).Direction = ParameterDirection.Input;
                            cmd.Parameters[0].Value = parentId;

                            cmd.Parameters.Add(":valCmd", OracleType.Number, 13).Direction = ParameterDirection.Input;
                            cmd.Parameters[1].Value = totalComanda;

                            cmd.Parameters.Add(":idCmd", OracleType.Number, 11).Direction = ParameterDirection.Input;
                            cmd.Parameters[2].Value = lastCmdId;

                            cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                            cmd.Parameters[3].Value = localDepart;


                            cmd.Parameters.Add(":acc1", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                            valAcc1 = " ";
                            if (alerteSD.Contains(localDepart))
                                valAcc1 = "X";

                            if (alerteDV.Contains(localDepart))
                            {
                                // valAcc1 = "X";
                            }

                            cmd.Parameters[4].Value = valAcc1;

                            cmd.Parameters.Add(":acc2", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                            valAcc2 = " ";
                            if (alerteDV.Contains(localDepart))
                                valAcc2 = "X";
                            cmd.Parameters[5].Value = valAcc2;


                            cmd.Parameters.Add(":statAprv", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                            statAprv = "0";
                            if (valAcc1.Equals("X") || valAcc2.Equals("X"))
                            {
                                statAprv = "1";
                                globalStatusCmd = "1";
                            }

                            cmd.Parameters[6].Value = statAprv;

                            cmd.ExecuteNonQuery();


                            if (statAprv.Equals("0") && (valAcc1.Equals("X") || valAcc2.Equals("X")))
                                ErrorHandling.sendErrorToMail("saveKANewCmd: " +  comanda + "\n\n" +  alertSD + "\n\n" + alertDV + "\n\n" + cmdAngajament + "\n\n" + tipUser + "\n\n" + JSONArt + "\n\n" + JSONComanda + "\n\n" + JSONDateLivrare + "\n\n" + calcTransport);


                            totalComanda = 0;

                            //actualizare tabela conditii
                            if (conditieID != "-1")
                            {

                                query = " update sapprd.zcondheadtableta set cmdmodif = '" + idCmd.Value + "' where id = '" + conditieID + "' ";

                                cmd.CommandText = query;
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Clear();
                                cmd.ExecuteNonQuery();

                            }
                            //sf. actualizare

                            if (localDepart.Contains("04"))
                            {
                                savePrelucrare04(connection, idCmd.Value.ToString(), dateLivrare.prelucrare);
                            }

                            if (valAcc1.Equals("X"))
                            {
                                OperatiiSuplimentare.aprobaAutomatSD(connection, dateLivrare.codAgent, idCmd.Value.ToString());
                            }

                        }



                        //comanda noua
                        //inserare antet comanda

                        query = " insert into sapprd.zcomhead_tableta(mandt,id,cod_client,ul,status,status_aprov ,datac,cantar,cod_agent,cod_init,tip_plata,pers_contact,telefon,adr_livrare, " +
                               " valoare,mt,com_referinta,accept1,accept2,fact_red, city, region, pmnttrms , obstra, timpc, ketdat, docin, adr_noua, depart, obsplata, addrnumber, nume_client, " +
                               " stceg, tip_pers, val_incasata, site, email, mod_av, cod_j, adr_livrare_d, city_d, region_d, macara, id_obiectiv, adresa_obiectiv, client_raft,fact_palet_separat, " +
                               " lifnr, lifnr_prod, descoperita, prog_livr, livr_sambata, bloc, ref_client, ac_zc, fil_plata  ) " +
                               " values ('900',pk_key.nextval, :codCl,:ul,:status,:status_aprov, " +
                               " :datac,:cantar,:agent,:codinit,:plata,:perscont,:tel,:adr,:valoare,:transp,:comsap,:accept1,:accept2,:factred,:city,:region,:termplt,:obslivr,:timpc,:datalivrare, " +
                               " :tipDocIn, :adrNoua, :depart, :obsplata, :adrnumber, :numeClient, :cnpClient, :tipPers, :valIncasata, :cmdSite, :email, :mod_av, :codJ, " +
                               " :adr_livrare_d, :city_d, :region_d,:macara, :idObiectiv, :adresaObiectiv, :client_raft, :factPaletSeparat, :lifnr, :lifnr_prod, :descoperita, :progrLivr, :livrSambata, :bloc, :refClient, :aczc, :filPlata  ) " +
                               " returning id into :id ";


                        cmd.CommandText = query;
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(":codCl", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = comandaVanzare.codClient;

                        cmd.Parameters.Add(":ul", OracleType.VarChar, 4).Direction = ParameterDirection.Input;
                        cmd.Parameters[1].Value = dateLivrare.unitLog;

                        cmd.Parameters.Add(":status", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                        cmd.Parameters[2].Value = tipCmd;

                        cmd.Parameters.Add(":status_aprov", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                        cmd.Parameters[3].Value = cmdStatus;

                        cmd.Parameters.Add(":datac", OracleType.VarChar, 10).Direction = ParameterDirection.Input;
                        cmd.Parameters[4].Value = nowDate;

                        cmd.Parameters.Add(":cantar", OracleType.VarChar, 1).Direction = ParameterDirection.Input;
                        string cnt = "0";

                        if (dateLivrare.Cantar.Equals("DA"))
                            cnt = "1";
                        cmd.Parameters[5].Value = cnt;

                        cmd.Parameters.Add(":agent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                        cmd.Parameters[6].Value = dateLivrare.codAgent;

                        cmd.Parameters.Add(":codinit", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                        cmd.Parameters[7].Value = dateLivrare.codAgent;

                        cmd.Parameters.Add(":plata", OracleType.VarChar, 4).Direction = ParameterDirection.Input;
                        cmd.Parameters[8].Value = dateLivrare.tipPlata;

                        if (dateLivrare.tipPlata.Trim().Equals("LC"))
                        {
                            return "-1";
                        }

                        cmd.Parameters.Add(":perscont", OracleType.VarChar, 25).Direction = ParameterDirection.Input;
                        cmd.Parameters[9].Value = dateLivrare.persContact;

                        cmd.Parameters.Add(":tel", OracleType.VarChar, 15).Direction = ParameterDirection.Input;
                        cmd.Parameters[10].Value = dateLivrare.nrTel;

                        cmd.Parameters.Add(":adr", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                        cmd.Parameters[11].Value = dateLivrare.Strada + " ";

                        cmd.Parameters.Add(":valoare", OracleType.Number, 15).Direction = ParameterDirection.Input;
                        cmd.Parameters[12].Value = dateLivrare.totalComanda;

                        cmd.Parameters.Add(":transp", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                        cmd.Parameters[13].Value = dateLivrare.Transport;

                        cmd.Parameters.Add(":comsap", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[14].Value = refSAP;

                        cmd.Parameters.Add(":accept1", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                        string valSD = " ";
                        valSD = " ";
                        if (tipUser == "AV")
                        {
                            if (alertSD || dateLivrare.adrLivrNoua) //adr. livrare noua
                                valSD = "X";
                            else
                                valSD = " ";

                            if (alertDV)
                                valSD = "X";
                        }

                        if (tipUser == "KA")
                        {
                            if (alertSD)
                                valSD = "X";
                        }

                        if (dateLivrare.isComandaACZC)
                        {
                            valSD = "X";
                        }

                        cmd.Parameters[15].Value = valSD;

                        cmd.Parameters.Add(":accept2", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                        string valDV = " ";
                        if (alertDV)
                            valDV = "X";
                        cmd.Parameters[16].Value = valDV;

                        cmd.Parameters.Add(":factred", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                        string factRed = " ";

                        if (comandaVanzare.factRedSeparat.Equals("DA") || comandaVanzare.factRedSeparat.Equals("X"))
                            factRed = "X";

                        if (comandaVanzare.factRedSeparat.Equals("NU") || comandaVanzare.factRedSeparat.Equals(" "))
                            factRed = " ";

                        if (comandaVanzare.factRedSeparat.Equals("R"))
                            factRed = "R";

                        cmd.Parameters[17].Value = factRed;

                        cmd.Parameters.Add(":city", OracleType.VarChar, 75).Direction = ParameterDirection.Input;
                        cmd.Parameters[18].Value = dateLivrare.Oras;

                        cmd.Parameters.Add(":region", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                        cmd.Parameters[19].Value = dateLivrare.codJudet;

                        cmd.Parameters.Add(":termplt", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                        cmd.Parameters[20].Value = dateLivrare.termenPlata;

                        cmd.Parameters.Add(":obslivr", OracleType.VarChar, 300).Direction = ParameterDirection.Input;
                        cmd.Parameters[21].Value = dateLivrare.obsLivrare;

                        cmd.Parameters.Add(":timpc", OracleType.VarChar, 18).Direction = ParameterDirection.Input;
                        cmd.Parameters[22].Value = nowTime;

                        cmd.Parameters.Add(":dataLivrare", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[23].Value = dataLivrare.ToString("yyyyMMdd");

                        cmd.Parameters.Add(":tipDocIn", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                        cmd.Parameters[24].Value = dateLivrare.tipDocInsotitor;

                        cmd.Parameters.Add(":adrNoua", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                        string adrLivrNoua = " ";
                        if (dateLivrare.adrLivrNoua.Equals(true))
                            adrLivrNoua = "X";
                        cmd.Parameters[25].Value = adrLivrNoua;

                        cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                        cmd.Parameters[26].Value = depart;

                        cmd.Parameters.Add(":obsplata", OracleType.VarChar, 300).Direction = ParameterDirection.Input;
                        cmd.Parameters[27].Value = dateLivrare.obsPlata;

                        cmd.Parameters.Add(":adrnumber", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[28].Value = dateLivrare.addrNumber;

                        cmd.Parameters.Add(":numeClient", OracleType.VarChar, 120).Direction = ParameterDirection.Input;
                        cmd.Parameters[29].Value = " ";

                        cmd.Parameters.Add(":cnpClient", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                        cmd.Parameters[30].Value = " ";

                        cmd.Parameters.Add(":tipPers", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                        cmd.Parameters[31].Value = tipUser;

                        cmd.Parameters.Add(":valIncasata", OracleType.Number, 15).Direction = ParameterDirection.Input;
                        cmd.Parameters[32].Value = dateLivrare.valoareIncasare;

                        cmd.Parameters.Add(":cmdSite", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                        cmd.Parameters[33].Value = comandaVanzare.userSite;

                        cmd.Parameters.Add(":email", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                        cmd.Parameters[34].Value = comandaVanzare.userSiteMail.Replace("~", "@");

                        cmd.Parameters.Add(":mod_av", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                        cmd.Parameters[35].Value = comandaVanzare.isValIncModif;

                        cmd.Parameters.Add(":codJ", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                        cmd.Parameters[36].Value = comandaVanzare.codJ.Length > 0 ? comandaVanzare.codJ : " ";

                        cmd.Parameters.Add(":adr_livrare_d", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                        cmd.Parameters[37].Value = " ";

                        cmd.Parameters.Add(":city_d", OracleType.VarChar, 75).Direction = ParameterDirection.Input;
                        cmd.Parameters[38].Value = " ";

                        cmd.Parameters.Add(":region_d", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                        cmd.Parameters[39].Value = " ";

                        cmd.Parameters.Add(":macara", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                        cmd.Parameters[40].Value = dateLivrare.macara;

                        cmd.Parameters.Add(":idObiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;

                        int idObiectiv = 0;

                        if (dateLivrare.idObiectiv != null && dateLivrare.idObiectiv.Length > 1)
                            idObiectiv = Int32.Parse(dateLivrare.idObiectiv);
                        cmd.Parameters[41].Value = idObiectiv;

                        cmd.Parameters.Add(":adresaObiectiv", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                        cmd.Parameters[42].Value = dateLivrare.isAdresaObiectiv ? "X" : " ";

                        cmd.Parameters.Add(":client_raft", OracleType.VarChar, 3).Direction = ParameterDirection.Input;

                        string clientRaft = " ";
                        if (dateLivrare.clientRaft != null)
                            clientRaft = Boolean.Parse(dateLivrare.clientRaft) ? "X" : " ";

                        cmd.Parameters[43].Value = clientRaft;

                        cmd.Parameters.Add(":factPaletSeparat", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                        cmd.Parameters[44].Value = Convert.ToBoolean(dateLivrare.factPaletiSeparat) ? "X" : " ";

                        cmd.Parameters.Add(":lifnr", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[45].Value = dateLivrare.filialaCLP != null && dateLivrare.filialaCLP.Length > 0 ? dateLivrare.filialaCLP : dateLivrare.furnizorMarfa;


                        cmd.Parameters.Add(":lifnr_prod", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[46].Value = dateLivrare.furnizorProduse != null ? dateLivrare.furnizorProduse : " ";

                        cmd.Parameters.Add(":descoperita", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                        cmd.Parameters[47].Value = Convert.ToBoolean(dateLivrare.isCamionDescoperit) ? "X" : " ";

                        cmd.Parameters.Add(":progrLivr", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                        cmd.Parameters[48].Value = dateLivrare.programLivrare != null ? dateLivrare.programLivrare : "0";

                        string livrSambata = " ";
                        if (!isDataSambata(dataLivrare))
                            livrSambata = dateLivrare.livrareSambata != null && dateLivrare.livrareSambata.Length > 0 ? dateLivrare.livrareSambata : " ";

                        cmd.Parameters.Add(":livrSambata", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                        cmd.Parameters[49].Value = livrSambata;

                        cmd.Parameters.Add(":bloc", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                        cmd.Parameters[50].Value = dateLivrare.blocScara != null && dateLivrare.blocScara.Length > 0 ? dateLivrare.blocScara : " ";

                        cmd.Parameters.Add(":refClient", OracleType.VarChar, 105).Direction = ParameterDirection.Input;
                        cmd.Parameters[51].Value = dateLivrare.refClient != null && dateLivrare.refClient.Length > 0 ? dateLivrare.refClient : " ";

                        cmd.Parameters.Add(":aczc", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                        cmd.Parameters[52].Value = dateLivrare.isComandaACZC ? "X" : " ";

                        cmd.Parameters.Add(":filPlata", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                        cmd.Parameters[53].Value = dateLivrare.filialaPlata != null && dateLivrare.filialaPlata.Trim().Length > 0 ? dateLivrare.filialaPlata : " ";

                        idCmd = new OracleParameter("id", OracleType.Number);
                        idCmd.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(idCmd);

                        cmd.ExecuteNonQuery();

                        lastCmdId = idCmd.Value.ToString();

                        if (parentId.Equals(""))
                        {
                            parentId = idCmd.Value.ToString();
                        }

                        OperatiiAdresa.insereazaCoordonateAdresa(connection, idCmd.Value.ToString(), dateLivrare.coordonateGps, dateLivrare.codJudet, dateLivrare.Oras);
                        OperatiiSuplimentare.saveTonajComanda(connection, idCmd.Value.ToString(), dateLivrare.tonaj);

                        if (dateLivrare.adrLivrNoua)
                            OperatiiSuplimentare.saveTonajAdresa(connection, comandaVanzare.codClient, dateLivrare.addrNumber, dateLivrare.tonaj);

                    }



                    codArticol = articolComanda[i].codArticol;

                    if (codArticol.Equals("30101747") || codArticol.Equals("30101748"))
                        codArticol = HelperComenzi.getArtExcCherestea(connection, null, articolComanda, codArticol);

                    if (codArticol.Length == 8)
                        codArticol = "0000000000" + codArticol;

                    if ((articolComanda[i].codArticol.Length != 8 || Double.Parse(codArticol) > 0) && !codArticol.Equals("000000000000000000"))
                    {
                        codUltimArticol = codArticol;
                        depozUltimArticol = articolComanda[i].depozit;
                        departAprobUltimArticol = articolComanda[i].departAprob;
                    }

                    double valPoz = 0;
                    double pretUnitarArt = 0;
                    if (refSAP.Equals("-1"))
                    {
                        valPoz = articolComanda[i].pretUnit / articolComanda[i].multiplu * Double.Parse(articolComanda[i].cantUmb, CultureInfo.InvariantCulture);
                        pretUnitarArt = articolComanda[i].pretUnit / articolComanda[i].multiplu;
                    }
                    else
                    {
                        valPoz = articolComanda[i].pretUnit * Double.Parse(articolComanda[i].cantUmb, CultureInfo.InvariantCulture);
                        pretUnitarArt = articolComanda[i].pretUnit;
                    }

                    if (articolComanda[i].depozit.ToUpper().Contains("MAV"))
                        hasDepozMAV1 = true;

                    ulStoc = unitLogAlt;

                    if (ulStoc.Equals("BV90") && articolComanda[i].depozit.Contains("MAV"))
                        ulStoc = "BV92";

                    if (dateLivrare.filialaCLP!= null && dateLivrare.filialaCLP.Trim().Length > 0)
                        ulStoc = dateLivrare.filialaCLP;

                    string greutateArticol = articolComanda[i].greutate == null ? "0" : articolComanda[i].greutate;

                    query = " insert into sapprd.zcomdet_tableta(mandt,id,poz,status,cod,cantitate,valoare,depoz, " +
                            " transfer,valoaresap,ppoz,procent,um,procent_fc,conditie,disclient,procent_aprob,multiplu, " +
                            " val_poz,inf_pret,cant_umb,umb,ul_stoc, istoric_pret, brgew) " +
                            " values ('900'," + idCmd.Value + ",'" + pozArt + "','" + cmdStatus + "','" + codArticol + "'," + articolComanda[i].cantitate.ToString(nfi) + ", " +
                            "" + pretUnitarArt.ToString(nfi) + ",'" + articolComanda[i].depozit + "','0',0,'0'," + articolComanda[i].procent.ToString(nfi) + ",'" +
                            articolComanda[i].um + "'," + articolComanda[i].procentFact.ToString(nfi) + ",' '," +
                            articolComanda[i].discClient.ToString(nfi) + "," + articolComanda[i].procAprob.ToString(nfi) + "," + articolComanda[i].multiplu.ToString(nfi) + "," +
                            valPoz.ToString(nfi) + ",'" + articolComanda[i].infoArticol + "'," + articolComanda[i].cantUmb + ",'" +
                            articolComanda[i].Umb + "', '" + ulStoc + "', '" + articolComanda[i].istoricPret + "'," + greutateArticol + " ) ";


                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.ExecuteNonQuery();

                    if (articolComanda[i].listCabluri != null && !articolComanda[i].listCabluri.Equals(String.Empty))
                        new Cabluri05().insertCabluri05(connection, null, idCmd.Value.ToString(), codArticol, articolComanda[i].listCabluri);

                    pozArt += 10;

                    totalComanda += valPoz;
                    totalGen += valPoz;

                    newDepart = articolComanda[i].depart;

                }//sf. for


                //actualizare total ultima comanda
                if (totalComanda != 0)
                {

                    //departament comanda
                    cmd.CommandText = " select grup_vz from articole where cod =:codArt ";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codUltimArticol;

                    oReader = cmd.ExecuteReader();
                    localDepart = "00";
                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        localDepart = oReader.GetString(0);
                    }
                    oReader.Close();
                    oReader.Dispose();
                    //sf. departament

                    if (depozUltimArticol.Contains("MAV") && !departAprobUltimArticol.Equals("00"))
                        localDepart = departAprobUltimArticol;

                    query = " update sapprd.zcomhead_tableta set parent_id =:parentId, valoare =:valCmd, depart =:depart, accept1 =:acc1, accept2 =:acc2, status_aprov =:statAprv where id =:idCmd ";


                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":parentId", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = parentId;

                    cmd.Parameters.Add(":valCmd", OracleType.Number, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = totalComanda;

                    cmd.Parameters.Add(":idCmd", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = lastCmdId;

                    cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = localDepart;


                    cmd.Parameters.Add(":acc1", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    valAcc1 = " ";
                    if (alerteSD.Contains(localDepart))
                        valAcc1 = "X";

                    if (alerteDV.Contains(localDepart))
                    {
                        valAcc1 = "X";
                    }

                    cmd.Parameters[4].Value = valAcc1;

                    cmd.Parameters.Add(":acc2", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    valAcc2 = " ";
                    if (alerteDV.Contains(localDepart))
                        valAcc2 = "X";
                    cmd.Parameters[5].Value = valAcc2;


                    cmd.Parameters.Add(":statAprv", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                    statAprv = "0";
                    if (valAcc1.Equals("X") || valAcc2.Equals("X"))
                    {
                        statAprv = "1";
                        globalStatusCmd = "1";
                    }
                    cmd.Parameters[6].Value = statAprv;

                    cmd.ExecuteNonQuery();

                    if (statAprv.Equals("0") && (valAcc1.Equals("X") || valAcc2.Equals("X")))
                        ErrorHandling.sendErrorToMail("saveKANewCmd: " + comanda + "\n\n" + alertSD + "\n\n" + alertDV + "\n\n" + cmdAngajament + "\n\n" + tipUser + "\n\n" + JSONArt + "\n\n" + JSONComanda + "\n\n" + JSONDateLivrare + "\n\n" + calcTransport);

                    //actualizare tabela conditii
                    if (conditieID != "-1")
                    {

                        query = " update sapprd.zcondheadtableta set cmdmodif = '" + lastCmdId + "' where id = '" + conditieID + "' ";

                        cmd.CommandText = query;
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Clear();
                        cmd.ExecuteNonQuery();

                    }
                    //sf. actualizare


                    if (localDepart.Contains("04"))
                    {
                        savePrelucrare04(connection, lastCmdId, dateLivrare.prelucrare);
                    }

                    if (valAcc1.Equals("X"))
                    {
                        OperatiiSuplimentare.aprobaAutomatSD(connection, dateLivrare.codAgent, idCmd.Value.ToString());
                    }

                }


                //actualizare tabela zcomhead_status
                query = " insert into sapprd.zcomhead_status(mandt,parent_id,status, status_aprov, cod_client, valoare, old_par_id)  " +
                        " values ('900'," + parentId + ",'" + tipCmd + "','" + globalStatusCmd + "','" + comandaVanzare.codClient + "'," +
                         totalGen.ToString("0.00", CultureInfo.InvariantCulture) + ", " + refSAP + " ) ";

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();
                cmd.ExecuteNonQuery();
                //sf. actualizare


                SAPWebServices.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE();

                SAPWebServices.ZcreazaComanda inParam = new SAPWebServices.ZcreazaComanda();
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                webService.Credentials = nc;
                webService.Timeout = 600000;

                inParam.Id = parentId;
                inParam.GvEvent = "C";  //C - creaza comanda, S - simulare pret transport
                SAPWebServices.ZcreazaComandaResponse outParam = webService.ZcreazaComanda(inParam);

                retVal = outParam.VOk.ToString();

                webService.Dispose();


                
                OperatiiSuplimentare.saveDateDelegat(connection, Int32.Parse(idCmd.Value.ToString()), dateLivrare);

                if (dateLivrare.adrLivrNoua)
                {
                    OperatiiAdresa.adaugaAdresaClient(connection, idCmd.Value.ToString(), dateLivrare);

                    string paramAdrLivr = "";
                    if (dateLivrare.Strada.Trim() != "")
                        paramAdrLivr = dateLivrare.Oras + ", " + dateLivrare.Strada;
                    else
                        paramAdrLivr = dateLivrare.Oras;

                    paramAdrLivr = dateLivrare.numeJudet + ", " + paramAdrLivr;

                    Mail.sendAlertMailAdrLivrareNouaKA(connection, dateLivrare.codAgent, comandaVanzare.codClient, paramAdrLivr, dateLivrare.unitLog);
                }

                if (hasDepozMAV1)
                {
                    if (retVal.Equals("0") || retVal.Contains("#"))
                    {
                        string filialaUser = dateLivrare.unitLog.Substring(0, 2) + "1" + dateLivrare.unitLog.Substring(3, 1);
                        Sms.TipUser[] useri = { Sms.TipUser.SM, Sms.TipUser.CVS };
                        new Mail().sendComandaMail(idCmd.Value.ToString(), useri, filialaUser);
                    }
                }



            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + "\n\n" + JSONArt + "\n\n" + JSONComanda + "\n\n" + JSONDateLivrare);
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            


            return retVal;

        }


        private string getCurrentDate()
        {
            string mDate = "";
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            mDate = year + month + day;
            return mDate;
        }



        private string getCurrentTime()
        {
            string mTime = "";
            DateTime cDate = DateTime.Now;
            mTime = cDate.Hour.ToString("00") + cDate.Minute.ToString("00") + cDate.Second.ToString("00");
            return mTime;
        }




        [WebMethod]
        public string getListObiective(string codUser, string filiala, string tipUser, string interval, string tipObiective, string stadiu)
        {

            string serializedResult = "";


            string sqlString = "", condData = "", strTipOb = "", condStadiu = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;



            condData = "";
            string dataStart = "";
            string dataStop = "";


            if (interval.Contains(':'))
            {
                string[] tokenData = interval.Split(':');
                string[] varDataStart = tokenData[0].Split('.');
                string[] varDataStop = tokenData[1].Split('.');

                dataStart = varDataStart[2] + varDataStart[1] + varDataStart[0];
                dataStop = varDataStop[2] + varDataStop[1] + varDataStop[0];

                condData = " and a.data_creare between :data1 and :data2 ";
            }

            if (tipObiective.Equals("0"))
                strTipOb = " (0) ";

            if (tipObiective.Equals("1"))
                strTipOb = " (0, 1) ";

            if (tipObiective.Equals("2"))
                strTipOb = " (2) ";

            if (stadiu.Length > 0)
            {
                if (!stadiu.Equals("-1"))
                {
                    condStadiu = " and a.stadiu = '" + stadiu + "' ";
                }

            }




            string condCodUser = "";

            if (!codUser.Equals("0"))
            {
                condCodUser = " and cod_agent = '" + codUser + "' ";
            }
            else
            {
                condCodUser = " and ul = '" + filiala + "' ";
            }


            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                if (stadiu.Equals("-1"))
                {
                    sqlString = " select a.id, a.nume_obiectiv, to_char(to_date(a.data_creare,'yyyymmdd')), ' ' status, a.oras, a.strada, b.nume  from sapprd.zobiectiveka a, agenti b " +
                                " where a.cod_agent = b.cod and status in " + strTipOb + condData + " and ul = '" + filiala + "' " + condCodUser + " order by a.id desc ";
                }
                else
                {


                    sqlString = " select a.id, a.nume_obiectiv, to_char(to_date(a.data_creare,'yyyymmdd')), ' ' status, a.oras, a.strada, b.nume, nvl(max(to_number(k.text_vizita)),-1), decode(trim(a.stadiu),'',-1,a.stadiu) " +
                                " from sapprd.zobiectiveka a, agenti b , sapprd.zviziteobctvka k  where a.cod_agent = b.cod " +
                                " and status in  " + strTipOb + condData + " and ul = '" + filiala + "' " + condCodUser + " and k.id(+) = a.id and k.cod_vizita(+) = '01' " +
                                " group by a.id, a.nume_obiectiv, a.data_creare,a.oras,a.strada,b.nume,a.stadiu ";

                }



                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                if (interval.Contains(':'))
                {
                    cmd.Parameters.Add(":data1", OracleType.Number, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = dataStart;

                    cmd.Parameters.Add(":data2", OracleType.Number, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = dataStop;
                }

                oReader = cmd.ExecuteReader();

                List<ObiectivKA> listaObiectiveKA = new List<ObiectivKA>();
                ObiectivKA unObiectivKA = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        if (stadiu.Equals("-1"))
                        {

                            unObiectivKA = new ObiectivKA();
                            unObiectivKA.idObiectiv = oReader.GetInt32(0).ToString();
                            unObiectivKA.numeObiectiv = oReader.GetString(1);
                            unObiectivKA.dataObiectiv = oReader.GetString(2);
                            unObiectivKA.statusObiectiv = oReader.GetString(3);
                            unObiectivKA.orasObiectiv = oReader.GetString(4);
                            unObiectivKA.stradaObiectiv = oReader.GetString(5);
                            unObiectivKA.agentObiectiv = oReader.GetString(6);
                            listaObiectiveKA.Add(unObiectivKA);

                        }
                        else
                        {
                            if (oReader.GetInt32(8) == Int32.Parse(stadiu) || oReader.GetInt32(7) == Int32.Parse(stadiu))
                            {
                                if (Math.Max(oReader.GetInt32(8), oReader.GetInt32(7)) == Int32.Parse(stadiu))
                                {

                                    unObiectivKA = new ObiectivKA();
                                    unObiectivKA.idObiectiv = oReader.GetInt32(0).ToString();
                                    unObiectivKA.numeObiectiv = oReader.GetString(1);
                                    unObiectivKA.dataObiectiv = oReader.GetString(2);
                                    unObiectivKA.statusObiectiv = oReader.GetString(3);
                                    unObiectivKA.orasObiectiv = oReader.GetString(4);
                                    unObiectivKA.stradaObiectiv = oReader.GetString(5);
                                    unObiectivKA.agentObiectiv = oReader.GetString(6);
                                    listaObiectiveKA.Add(unObiectivKA);

                                }
                            }
                        }

                    }

                }

                oReader.Close();
                oReader.Dispose();


                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaObiectiveKA);



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
        public string getObiectivDet(string idObiectiv, string showIstoric)
        {

            string retVal = "";


            string sqlString = "", strIstoric = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                sqlString = "select nume_obiectiv, judet, oras, strada, emitere_autoriz, expir_autoriz, antreprenor, pers_cont, subant1, pers_cont1, tel_cont1, subant2, pers_cont2, tel_cont2, " +
                            " stadiu, colaborare, divizii, data_creare, tel_cont from sapprd.zobiectiveka where id =:idOb ";

                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idOb", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idObiectiv);

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        retVal += oReader.GetString(0) + "#" + oReader.GetString(1) + "#" + oReader.GetString(2) +
                                "#" + oReader.GetString(3) + "#" + oReader.GetString(4) + "#" + oReader.GetString(5) + "#" + oReader.GetString(6) + "#" + oReader.GetString(7) +
                                "#" + oReader.GetString(8) + "#" + oReader.GetString(9) + "#" + oReader.GetString(10) + "#" + oReader.GetString(11) + "#" + oReader.GetString(12) +
                                "#" + oReader.GetString(13) + "#" + oReader.GetString(14) +
                                "#" + oReader.GetString(15) + "#" + oReader.GetString(16) + "#" + oReader.GetString(17) + "#" + oReader.GetString(18);

                    }

                }
                else
                {
                    retVal = "-1";
                }


                if (showIstoric.Equals("1"))
                {

                    strIstoric = "#";

                    string varTemp = "";
                    sqlString = " select nvl(data_vizita,' '), text_vizita from sapprd.zviziteobctvka where id =:idVizita and cod_vizita = '01' order by data_vizita ";

                    cmd.CommandText = sqlString;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idVizita", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = Int32.Parse(idObiectiv);

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        varTemp = "";

                        while (oReader.Read())
                        {
                            varTemp += oReader.GetString(0) + "&" + oReader.GetString(1) + "!";
                        }

                        strIstoric += varTemp + "@@";
                    }
                    else
                    {
                        strIstoric += " @@";
                    }



                    sqlString = " select nvl(data_vizita,' '), text_vizita from sapprd.zviziteobctvka where id =:idVizita and cod_vizita = '02' order by data_vizita ";

                    cmd.CommandText = sqlString;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idVizita", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = Int32.Parse(idObiectiv);

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        varTemp = "";
                        while (oReader.Read())
                        {
                            varTemp += oReader.GetString(0) + "&" + oReader.GetString(1) + "!";
                        }

                        strIstoric += varTemp + "@@";

                    }
                    else
                    {
                        strIstoric += " @@";
                    }


                    sqlString = " select nvl(data_vizita,' '), nvl(text_vizita,' ') from sapprd.zviziteobctvka where id =:idVizita and cod_vizita = '03' order by data_vizita desc, ora_vizita desc ";

                    cmd.CommandText = sqlString;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idVizita", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = Int32.Parse(idObiectiv);

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        varTemp = "";
                        oReader.Read();
                        varTemp += oReader.GetString(0) + "&" + oReader.GetString(1) + "!";

                        strIstoric += varTemp + "@@";

                    }
                    else
                    {
                        strIstoric += " @@";
                    }


                    sqlString = " select nvl(data_vizita,' '), text_vizita from sapprd.zviziteobctvka where id =:idVizita and cod_vizita = '04' order by data_vizita desc, ora_vizita desc";

                    cmd.CommandText = sqlString;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idVizita", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = Int32.Parse(idObiectiv);

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        varTemp = "";
                        oReader.Read();
                        varTemp += oReader.GetString(0) + "&" + oReader.GetString(1) + "!";

                        strIstoric += varTemp + "@@";
                    }
                    else
                    {
                        strIstoric += " @@";
                    }


                    sqlString = " select nvl(data_vizita,' '), text_vizita from sapprd.zviziteobctvka where id =:idVizita and cod_vizita = '05' ";

                    cmd.CommandText = sqlString;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idVizita", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = Int32.Parse(idObiectiv);

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        varTemp = "";
                        oReader.Read();
                        varTemp += oReader.GetString(0) + "&" + oReader.GetString(1) + "!";

                        strIstoric += varTemp + "@@";
                    }
                    else
                    {
                        strIstoric += " @@";
                    }



                    retVal += "#" + strIstoric;

                }


                oReader.Close();
                oReader.Dispose();


            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + " , " + idObiectiv + ", " + sqlString);
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }


            return retVal;



        }


        [WebMethod]
        public string getVizitaStatus(string idVizita, string tipVizita)
        {
            string retVal = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                //cautare in tabela principala
                string sqlString = "";
                if (tipVizita.Equals("01"))
                {
                    sqlString = " select data_creare, stadiu from sapprd.zobiectiveka where id =:idVizita ";
                }

                if (tipVizita.Equals("02"))
                {
                    sqlString = " select data_creare, colaborare from sapprd.zobiectiveka where id =:idVizita ";
                }

                if (tipVizita.Equals("03"))
                {
                    sqlString = " select data_creare, divizii from sapprd.zobiectiveka where id =:idVizita ";
                }

                if (tipVizita.Equals("01") || tipVizita.Equals("02") || tipVizita.Equals("03"))
                {
                    cmd.CommandText = sqlString;

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":idVizita", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = Int32.Parse(idVizita);

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            retVal += oReader.GetString(0) + "#" + Regex.Replace(oReader.GetString(1), @"[!]|[#]|[@@]|[,]", " ") + "@@";
                        }

                    }
                    else
                    {
                        retVal = " ";
                    }
                }


                cmd.CommandText = " select data_vizita, text_vizita from sapprd.zviziteobctvka where id =:idVizita and cod_vizita =:codVizita order by data_vizita, ora_vizita ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idVizita", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idVizita);

                cmd.Parameters.Add(":codVizita", OracleType.Number, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = tipVizita;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        retVal += oReader.GetString(0) + "#" + Regex.Replace(oReader.GetString(1), @"[!]|[#]|[@@]|[,]", " ") + "@@";
                    }

                }


                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();
            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + " , " + idVizita + " , " + tipVizita);
                retVal = "-1";
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }


            return retVal;

        }




        [WebMethod]
        public string saveViziteObiectiveKA(string idVizita, string codVizita, string textVizita)
        {
            string retVal = "";


            OracleConnection connection = new OracleConnection();


            try
            {


                string connectionString = GetConnectionString_android();
                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();
                string query = "";

                query = " insert into sapprd.zviziteobctvka (mandt, id, data_vizita, cod_vizita, text_vizita, ora_vizita) values ('900', :idVizita, :dataVizita, :codVizita, :textVizita, :oraVizita) ";

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idVizita", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idVizita);

                cmd.Parameters.Add(":dataVizita", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = getCurrentDate();

                cmd.Parameters.Add(":codVizita", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codVizita;

                cmd.Parameters.Add(":textVizita", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = textVizita;

                cmd.Parameters.Add(":oraVizita", OracleType.VarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = getCurrentTime();

                cmd.ExecuteNonQuery();



                //inchidere obiectiv
                if (codVizita.Equals("05"))
                {
                    query = " update sapprd.zobiectiveka set status = 2 where id =:idVizita ";

                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":idVizita", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = Int32.Parse(idVizita);

                    cmd.ExecuteNonQuery();

                }
                else
                {
                    query = " update sapprd.zobiectiveka set status = 1 where id =:idVizita ";

                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":idVizita", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = Int32.Parse(idVizita);

                    cmd.ExecuteNonQuery();
                }




                retVal = "0";

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return retVal;


        }


        [WebMethod]
        public string salveazaObiectiv(string dateObiective)
        {
            OperatiiObiectiveKA operatiiObiective = new OperatiiObiectiveKA();
            return operatiiObiective.adaugaObiectiv(dateObiective);
        }


        [WebMethod]
        public string getListObiectiveKA(string codAgent, string filiala, string tip, string interval, string depart)
        {
            OperatiiObiectiveKA operatiiObiective = new OperatiiObiectiveKA();
            return operatiiObiective.getListObiective(codAgent, filiala, tip, interval, depart);
        }

        [WebMethod]
        public string getListObiectiveAgenti(string codAgent, string filiala, string depart, string tipUser)
        {
            OperatiiObiectiveKA operatiiObiective = new OperatiiObiectiveKA();
            return operatiiObiective.getListObiectiveAgenti(codAgent, filiala, depart, tipUser);
        }

        [WebMethod]
        public string getListObiectiveKAHarta(string codAgent, string filiala, string codJudet)
        {
            OperatiiObiectiveKA operatiiObiective = new OperatiiObiectiveKA();
            return operatiiObiective.getListObiectiveKA(codAgent, filiala, codJudet);
        }

        [WebMethod]
        public string getDetaliiObiectiv(string idObiectiv)
        {
            OperatiiObiectiveKA operatiiObiective = new OperatiiObiectiveKA();
            return operatiiObiective.getDetaliiObiectiv(idObiectiv);
        }

        [WebMethod]
        public string getStareClient(string codClient, string codDepartament)
        {
            OperatiiObiectiveKA operatiiObiective = new OperatiiObiectiveKA();
            return operatiiObiective.getStareClient(codClient, codDepartament);
        }

        [WebMethod]
        public string getClientiObiectiv(string idObiectiv)
        {
            OperatiiObiectiveKA operatiiObiective = new OperatiiObiectiveKA();
            return operatiiObiective.getClientiObiectiv(idObiectiv);
        }

        [WebMethod]
        public string salveazaEvenimentObiectiv(string eveniment)
        {
            OperatiiObiectiveKA operatiiObiective = new OperatiiObiectiveKA();
            return operatiiObiective.salveazaEveniment(eveniment);
        }

        [WebMethod]
        public string getEvenimenteObiectiv(string idObiectiv, string codClient, string codDepart)
        {
            OperatiiObiectiveKA operatiiObiective = new OperatiiObiectiveKA();
            return operatiiObiective.getEvenimenteObiectiv(idObiectiv, codClient, codDepart);
        }

        [WebMethod]
        public string getObiectiveDepartament(string filiala, string departament, string codClient, string tipUser, string codUser)
        {
            OperatiiObiectiveKA operatiiObiective = new OperatiiObiectiveKA();
            return operatiiObiective.getObiectiveDepartament(filiala, departament, codClient, tipUser, codUser);
        }


        [WebMethod]
        public string saveNewObiectiveKA(string codAgent, string filiala, string obiective, string selectedOb, string tipOp)
        {
            string retVal = "";



            OracleConnection connection = new OracleConnection();


            try
            {
                string[] tokenObiective = obiective.Split('#');

                string connectionString = GetConnectionString_android();
                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();
                string query = "";
                //insert
                if (tipOp == "0")
                {

                    query = " insert into sapprd.zobiectiveka(mandt, id, ul, cod_agent, data_creare, status, nume_obiectiv, judet, oras, strada, emitere_autoriz, expir_autoriz, " +
                            " antreprenor, pers_cont, subant1, pers_cont1, tel_cont1, subant2, pers_cont2, tel_cont2, stadiu, colaborare, divizii, tel_cont) values ('900', pk_clp.nextval, :ul, :codAg, " +
                            " :datac, :status, :nume_ob, :judet, :oras , " +
                            " :strada, :emitere, :expirare, :antreprenor, :pers_cont, :subant1, :pers_cont1, :tel_cont1, :subant2,:pers_cont2,:tel_cont2,:stadiu,:colaborare,:divizii,:tel_cont) " +
                            " returning id into :id ";



                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":ul", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = filiala;

                    cmd.Parameters.Add(":codAg", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codAgent;

                    cmd.Parameters.Add(":datac", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = getCurrentDate();

                    cmd.Parameters.Add(":status", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = "0";

                    cmd.Parameters.Add(":nume_ob", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = tokenObiective[0];

                    cmd.Parameters.Add(":judet", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[5].Value = tokenObiective[1];

                    cmd.Parameters.Add(":oras", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[6].Value = tokenObiective[2];

                    cmd.Parameters.Add(":strada", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[7].Value = tokenObiective[3];

                    cmd.Parameters.Add(":emitere", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    string strData = " ";
                    if (tokenObiective[4].Contains('.'))
                    {
                        string[] varData = tokenObiective[4].Split('.');
                        strData = varData[2] + varData[1] + varData[0];
                    }
                    cmd.Parameters[8].Value = strData;

                    cmd.Parameters.Add(":expirare", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    strData = " ";
                    if (tokenObiective[5].Contains('.'))
                    {
                        string[] varData = tokenObiective[5].Split('.');
                        strData = varData[2] + varData[1] + varData[0];
                    }
                    cmd.Parameters[9].Value = strData;

                    cmd.Parameters.Add(":antreprenor", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[10].Value = tokenObiective[6];

                    cmd.Parameters.Add(":pers_cont", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[11].Value = tokenObiective[7];

                    cmd.Parameters.Add(":subant1", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[12].Value = tokenObiective[8];

                    cmd.Parameters.Add(":pers_cont1", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[13].Value = tokenObiective[9];

                    cmd.Parameters.Add(":tel_cont1", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[14].Value = tokenObiective[10];

                    cmd.Parameters.Add(":subant2", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[15].Value = tokenObiective[11];

                    cmd.Parameters.Add(":pers_cont2", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[16].Value = tokenObiective[12];

                    cmd.Parameters.Add(":tel_cont2", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[17].Value = tokenObiective[13];

                    cmd.Parameters.Add(":stadiu", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[18].Value = tokenObiective[14];

                    cmd.Parameters.Add(":colaborare", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[19].Value = tokenObiective[15];

                    cmd.Parameters.Add(":divizii", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[20].Value = tokenObiective[16];

                    cmd.Parameters.Add(":tel_cont", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[21].Value = tokenObiective[17];

                    OracleParameter idCmd = new OracleParameter("id", OracleType.Number);
                    idCmd.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(idCmd);

                    cmd.ExecuteNonQuery();



                }


                //update
                if (tipOp == "1")
                {
                    query = " update sapprd.zobiectiveka set data_creare =:datac, nume_obiectiv = :nume_ob, judet =:judet, oras =:oras, strada =:strada, emitere_autoriz =:emitere, " +
                            " expir_autoriz =:expirare, antreprenor =:antreprenor, pers_cont =:pers_cont, subant1 =:subant1, pers_cont1 =:pers_cont1, tel_cont1 =:tel_cont1, " +
                            " subant2 =:subant2, pers_cont2 =:pers_cont2, tel_cont2 =:tel_cont2, stadiu =:stadiu, colaborare =:colaborare, divizii =:divizii, tel_cont =:tel_cont " +
                            " where id =:idV ";




                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":datac", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = getCurrentDate();

                    cmd.Parameters.Add(":nume_ob", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = tokenObiective[0];

                    cmd.Parameters.Add(":judet", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = tokenObiective[1];

                    cmd.Parameters.Add(":oras", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = tokenObiective[2];

                    cmd.Parameters.Add(":strada", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = tokenObiective[3];

                    cmd.Parameters.Add(":emitere", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    string strData = " ";
                    if (tokenObiective[4].Contains('.'))
                    {
                        string[] varData = tokenObiective[4].Split('.');
                        strData = varData[2] + varData[1] + varData[0];
                    }
                    cmd.Parameters[5].Value = strData;

                    cmd.Parameters.Add(":expirare", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    strData = " ";
                    if (tokenObiective[5].Contains('.'))
                    {
                        string[] varData = tokenObiective[5].Split('.');
                        strData = varData[2] + varData[1] + varData[0];
                    }
                    cmd.Parameters[6].Value = strData;

                    cmd.Parameters.Add(":antreprenor", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[7].Value = tokenObiective[6];

                    cmd.Parameters.Add(":pers_cont", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[8].Value = tokenObiective[7];

                    cmd.Parameters.Add(":subant1", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[9].Value = tokenObiective[8];

                    cmd.Parameters.Add(":pers_cont1", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[10].Value = tokenObiective[9];

                    cmd.Parameters.Add(":tel_cont1", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[11].Value = tokenObiective[10];

                    cmd.Parameters.Add(":subant2", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[12].Value = tokenObiective[11];

                    cmd.Parameters.Add(":pers_cont2", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[13].Value = tokenObiective[12];

                    cmd.Parameters.Add(":tel_cont2", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[14].Value = tokenObiective[13];

                    cmd.Parameters.Add(":stadiu", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[15].Value = tokenObiective[14];

                    cmd.Parameters.Add(":colaborare", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[16].Value = tokenObiective[15];

                    cmd.Parameters.Add(":divizii", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[17].Value = tokenObiective[16];

                    cmd.Parameters.Add(":tel_cont", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[18].Value = tokenObiective[17];

                    cmd.Parameters.Add(":idV", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[19].Value = Int32.Parse(selectedOb);

                    cmd.ExecuteNonQuery();


                }



                retVal = "0";

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return retVal;
        }



        [WebMethod]
        public string saveNewDl(string comanda, string codAgent, string filiala, string depart, bool alertSD)
        {

            string retVal = "-1";

            return "-1";

            OracleConnection connection = new OracleConnection();
            OracleTransaction transaction = null;

            try
            {

                string[] mainTokenClp = comanda.Split('@');
                string[] antetClpToken = mainTokenClp[0].Split('#');

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                string query = " insert into sapprd.zclphead(mandt, id, cod_client, cod_agent, ul, ul_dest, depart, status, nrcmdsap, datac, accept1, status_aprov, " +
                               " pers_contact, telefon, adr_livrare, city, region, ketdat, felmarfa, masa, tipcamion, tipinc, dl, depoz_dest, tip_plata, val_comanda, obs, furn_prod, cod_agent2, mt,fasonate) values ('900', pk_clp.nextval, :codCl, :codAg, :ul, :ulDest, :depart, :status, :nrcmdsap , " +
                               " :datac, :accept1, :status_aprov, :perscont, :tel, :adr, :city, :region, :ketdat,:felmarfa,:masa,:tipcamion,:tipinc, 'X',:depozDest, :tipplata, :valcomanda, :obs, :furnprod, :codAgent2, :tipTransp, ' ') " +
                               " returning id into :id ";

                transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codCl", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = antetClpToken[0];

                cmd.Parameters.Add(":codAg", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAgent;

                cmd.Parameters.Add(":ul", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = filiala;

                cmd.Parameters.Add(":ulDest", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = antetClpToken[6];

                cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = depart;

                cmd.Parameters.Add(":status", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = "0";

                cmd.Parameters.Add(":nrcmdsap", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = " ";

                cmd.Parameters.Add(":datac", OracleType.VarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[7].Value = getCurrentDate();

                cmd.Parameters.Add(":accept1", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                string valSD = "1";
                if (alertSD)
                    valSD = "X";
                cmd.Parameters[8].Value = valSD;

                string status_aprov = "0";
                if (alertSD)
                    status_aprov = "1";

                cmd.Parameters.Add(":status_aprov", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[9].Value = status_aprov;

                cmd.Parameters.Add(":perscont", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[10].Value = antetClpToken[4];

                cmd.Parameters.Add(":tel", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[11].Value = antetClpToken[5];

                cmd.Parameters.Add(":adr", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[12].Value = antetClpToken[3];

                cmd.Parameters.Add(":city", OracleType.VarChar, 75).Direction = ParameterDirection.Input;
                cmd.Parameters[13].Value = antetClpToken[2];

                cmd.Parameters.Add(":region", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[14].Value = antetClpToken[1];

                cmd.Parameters.Add(":ketdat", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                string[] dataLivareClp = antetClpToken[7].Split('.');

                cmd.Parameters[15].Value = dataLivareClp[2] + dataLivareClp[1] + dataLivareClp[0];

                cmd.Parameters.Add(":felmarfa", OracleType.VarChar, 180).Direction = ParameterDirection.Input;
                cmd.Parameters[16].Value = antetClpToken[8];

                cmd.Parameters.Add(":masa", OracleType.VarChar, 180).Direction = ParameterDirection.Input;
                cmd.Parameters[17].Value = antetClpToken[9];

                cmd.Parameters.Add(":tipcamion", OracleType.VarChar, 45).Direction = ParameterDirection.Input;
                cmd.Parameters[18].Value = antetClpToken[10];

                cmd.Parameters.Add(":tipinc", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[19].Value = antetClpToken[11];

                cmd.Parameters.Add(":depozDest", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[20].Value = antetClpToken[12];

                cmd.Parameters.Add(":tipPlata", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[21].Value = antetClpToken[13];

                cmd.Parameters.Add(":valComanda", OracleType.Double).Direction = ParameterDirection.Input;
                cmd.Parameters[22].Value = Convert.ToDouble(antetClpToken[14]);

                cmd.Parameters.Add(":obs", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                int maxLen = antetClpToken[15].Length;
                if (maxLen > 150)
                    maxLen = 150;

                cmd.Parameters[23].Value = antetClpToken[15].Substring(0, maxLen);

                cmd.Parameters.Add(":furnprod", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[24].Value = antetClpToken[16];

                cmd.Parameters.Add(":codAgent2", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[25].Value = antetClpToken[17];

                cmd.Parameters.Add(":tipTransp", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[26].Value = antetClpToken[18];


                OracleParameter idCmd = new OracleParameter("id", OracleType.Number);
                idCmd.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(idCmd);

                cmd.ExecuteNonQuery();




                //inserare articole comanda
                string[] articoleCmdTokenClp;



                int artLen = mainTokenClp.Length - 2;
                int pozArt = 0;
                string tokenArtClp = "";

                string unArticol = "";

                for (int i = 0; i < artLen; i++)
                {

                    pozArt = (i + 1) * 10;


                    tokenArtClp = mainTokenClp[i + 1];
                    articoleCmdTokenClp = tokenArtClp.Split('#');
                    string codArtClp = articoleCmdTokenClp[0];
                    if (codArtClp.Length == 8)
                        codArtClp = "0000000000" + codArtClp;


                    query = " insert into sapprd.zclpdet(mandt,id,poz,status,cod,cantitate,umb,depoz) " +
                            " values ('900'," + idCmd.Value + ",'" + pozArt + "','0','" + codArtClp + "',:cantArt, " +
                            "'" + articoleCmdTokenClp[2] + "','" + articoleCmdTokenClp[3] + "' ) ";


                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":cantArt", OracleType.Number, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = Double.Parse(articoleCmdTokenClp[1]);

                    cmd.ExecuteNonQuery();

                    unArticol = codArtClp;


                }


                //actualizare departament dl
                string codDepart = getDepartArticol(unArticol);

                query = " update sapprd.zclphead set depart =:depart where id = " + idCmd.Value;

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":depart", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codDepart;

                cmd.ExecuteNonQuery();
                //sf. actualizare departament

                transaction.Commit();


                SapWsClp.ZCLP_WEBSERVICE webServiceDl = null;

                webServiceDl = new ZCLP_WEBSERVICE();

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                webServiceDl.Credentials = nc;
                webServiceDl.Timeout = 300000;

                SapWsClp.ZcreazaCaDl inParam = new SapWsClp.ZcreazaCaDl();
                inParam.VId = Convert.ToDecimal(idCmd.Value);

                SapWsClp.ZcreazaCaDlResponse outParam = webServiceDl.ZcreazaCaDl(inParam);

                retVal = outParam.VOk.ToString();

                webServiceDl.Dispose();

                //nu este nevoie de aprobare, se trimite mail de instiintare
                if (status_aprov.Equals("0"))
                {
                    if (!isClpTransferIntreFiliale(idCmd.Value.ToString()))
                        sendAlertMailCreareClp(idCmd.Value.ToString());
                }
                //sf. alert



                OperatiiSuplimentare.saveTonajComanda(connection, idCmd.Value.ToString(), antetClpToken[19]);


            }
            catch (Exception ex)
            {
                retVal = "-1";
                sendErrorToMail(ex.ToString() + " " + comanda);
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }



            return retVal;


        }



        private string getDepartArticol(string codArticol)
        {
            string codDepart = "00";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();


                cmd.CommandText = " select substr(grup_vz,0,2) from articole where cod = :codArt ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codArt", OracleType.NVarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    codDepart = oReader.GetString(0);
                }


                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();



            }
            catch (Exception e)
            {
                sendErrorToMail(e.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return codDepart;
        }


        [WebMethod]
        public string saveNewClp(string comanda, string codAgent, string filiala, string depart, bool alertSD, string serData)
        {
            OperatiiCLP operatiiClp = new OperatiiCLP();
            return operatiiClp.saveNewClp(comanda, codAgent, filiala, depart, alertSD, serData);
        }

        [WebMethod]
        public string saveLivrareCustodie(string JSONArt, string JSONComanda, string JSONDateLivrare)
        {
            return new OperatiiComenzi().saveLivrareCustodie(JSONArt, JSONComanda, JSONDateLivrare);
        }

        [WebMethod]
        public string getLivrariCustodie(string codAgent, string interval, string tipCmd)
        {
            return new JavaScriptSerializer().Serialize(new OperatiiComenzi().getLivrariCustodie(codAgent, interval, tipCmd));
        }

        [WebMethod]
        public string getArticoleCustodie(string nrCmd)
        {
            return new OperatiiComenzi().getArticoleCustodie(nrCmd);
        }

        [WebMethod]
        public string setCustodieDataLivrare(string idComanda, string dataLivrare)
        {
            return new OperatiiComenzi().setCustodieDataLivrare(idComanda, dataLivrare);
        }

        [WebMethod]
        public string setCmdVanzDataLivrare(string idComanda, string dataLivrare)
        {
            
            return new OperatiiComenzi().setCmdVanzDataLivrare(idComanda, dataLivrare);
        }

        [WebMethod]
        public string setCustodieAdresaLivrare(string idComanda, string dateLivrare)
        {
            return new OperatiiComenzi().setCustodieAdresaLivrare(idComanda, dateLivrare);
        }

        [WebMethod]
        public string stergeLivrareCustodie(string idComanda)
        {
            return new OperatiiComenzi().stergeLivrareCustodie(idComanda);
        }

        [WebMethod]
        public string getCabluri05(string codArticol, string sinteticArticol)
        {
            return new Cabluri05().getCabluri05(codArticol, sinteticArticol);
        }

        [WebMethod]
        public string saveNewCmdAndroid(string comanda, bool alertSD, bool alertDV, bool cmdAngajament, string tipUser, string JSONArt, string JSONComanda, string JSONDateLivrare, string tipUserSap)
        {

            string retVal = "-1";

            if (tipUser.Equals("KA"))
            {
                retVal = verificaArticoleMAV_Mathaus(comanda, alertSD, alertDV, cmdAngajament, tipUser, JSONArt, JSONComanda, JSONDateLivrare,"");
            }
            else
            {
                
                string localtipUserSap = "";


                DateLivrare dateLivrare = new JavaScriptSerializer().Deserialize<DateLivrare>(JSONDateLivrare);
                if (!tipUser.Equals("AV"))
                {
                    
                    if (tipUserSap == null || tipUserSap.Trim().Length == 0)
                        localtipUserSap = Utils.getTipAngajat(dateLivrare.codAgent);
                    else
                        localtipUserSap = tipUserSap;
                }


                if (Utils.isUserTest(dateLivrare.codAgent))
                {
                    retVal = verificaArticoleMAV_nou(comanda, alertSD, alertDV, cmdAngajament, tipUser, JSONArt, JSONComanda, JSONDateLivrare, localtipUserSap, "-1");
                }
                else if (tipUser.Equals("CV") || tipUser.Equals("KA3"))
                {
                    retVal = OperatiiComenzi.trateazaComenziGED(comanda, alertSD, alertDV, cmdAngajament, tipUser, JSONArt, JSONComanda, JSONDateLivrare, localtipUserSap);
                }
                else if (tipUser.Equals("SITE"))
                {
                    retVal = new ComenziSite().salveazaComanda(comanda, alertSD, alertDV, cmdAngajament, tipUser, JSONArt, JSONComanda, JSONDateLivrare);
                }
                else
                {
                    retVal = verificaArticoleMAV_Mathaus(comanda, alertSD, alertDV, cmdAngajament, tipUser, JSONArt, JSONComanda, JSONDateLivrare, localtipUserSap);
                }

            }



            return retVal;
        }





        private string getCnpFromComanda(string comanda)
        {
            string cnpClient = "-1";

            if (comanda.Trim().Equals(""))
                return cnpClient;
            try
            {

                string[] mainToken = comanda.Split('@');
                string[] antetCmdToken = mainToken[0].Split('#');
                string[] dateClient = antetCmdToken[0].Split('~');

                if (dateClient.Length >= 2)
                    cnpClient = dateClient[2];


            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + " , " + comanda);
            }

            return cnpClient;
        }



        private bool isUnitLogGed(string unitLog)
        {

            if (unitLog.Substring(2, 1).Equals("2"))
                return true;
            else
                return false;

        }

        private string verificaArticoleMAV_Mathaus(string comanda, bool alertSD, bool alertDV, bool cmdAngajament, string tipUser, string JSONArt, string JSONComanda, string JSONDateLivrare, string tipUserSap)
        {
            string retVal = "-1";

            var serializer = new JavaScriptSerializer();

            try
            {


                DateLivrare dateLivrareGed, dateLivrareDistrib;
                List<ArticolComanda> articoleGed, articoleDistrib;

                double totalComandaGed = 0, totalComandaDistrib = 0;

                ComandaVanzare comandaVanzare = serializer.Deserialize<ComandaVanzare>(JSONComanda);
                DateLivrare dateLivrare = serializer.Deserialize<DateLivrare>(JSONDateLivrare);
                List<ArticolComanda> articolComanda = serializer.Deserialize<List<ArticolComanda>>(JSONArt);

                List<CostTransportMathaus> costTransport = new List<CostTransportMathaus>();

                if (dateLivrare.costTransportMathaus != null)
                    costTransport = serializer.Deserialize<List<CostTransportMathaus>>(dateLivrare.costTransportMathaus);

                dateLivrareGed = dateLivrare;
                dateLivrareDistrib = dateLivrare;

                comandaVanzare.parrentId = GeneralUtils.getUniqueIdFromCode(dateLivrareDistrib.codAgent);

                bool isCmdGed = isUnitLogGed(dateLivrare.unitLog);

                articoleGed = new List<ArticolComanda>();
                articoleDistrib = new List<ArticolComanda>();

                if (isCmdGed)
                {
                    articoleGed = articolComanda;
                    totalComandaGed = Double.Parse(dateLivrare.totalComanda, CultureInfo.InvariantCulture);
                }
                else
                {

                    foreach (var articol in articolComanda)
                    {
                        if (articol.depart.Equals("11"))
                        {
                            articoleGed.Add(articol);
                            totalComandaGed += articol.pret;
                        }
                        else
                        {
                            articoleDistrib.Add(articol);

                            if (comandaVanzare.nrCmdSap.Length < 4)
                            {
                                totalComandaDistrib += (articol.pretUnit / articol.multiplu) * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                totalComandaDistrib += articol.pretUnit * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                            }


                        }
                    }
                }

                if (articoleDistrib.Count > 0)
                {
                    dateLivrareDistrib.totalComanda = totalComandaDistrib.ToString();
                    List<ArticolComanda> sortedArticoleDistrib = articoleDistrib.OrderBy(order => order.depart).ThenBy(order => order.filialaSite).ToList();

                    {
                        string departArt = sortedArticoleDistrib[0].depart;
                        string ulStoc = sortedArticoleDistrib[0].filialaSite;
                        string tipTranspCmd = "";

                        List<ArticolComanda> articoleAgenti = new List<ArticolComanda>();
                        double totalComanda = 0;

                        foreach (var articol in sortedArticoleDistrib)
                        {
                            if (!departArt.Equals(articol.depart) || (ulStoc != null && articol.filialaSite != null && !ulStoc.Equals(articol.filialaSite)))
                            {
                                dateLivrareDistrib.totalComanda = totalComanda.ToString();

                                if (!articol.filialaSite.Equals(dateLivrareDistrib.unitLog) && !articol.filialaSite.Equals("BV90") && !ulStoc.Equals("BV90"))
                                    dateLivrareDistrib.filialaCLP = ulStoc;


                                if (tipTranspCmd != null && !tipTranspCmd.Equals(String.Empty))
                                    dateLivrareDistrib.Transport = tipTranspCmd;

                                retVal = saveAVNewCmd(comanda, alertSD, alertDV, cmdAngajament, tipUser, serializer.Serialize(articoleAgenti), serializer.Serialize(comandaVanzare), serializer.Serialize(dateLivrareDistrib), false, tipUserSap);
                                articoleAgenti.Clear();
                                totalComanda = 0;
                            }



                            if (comandaVanzare.nrCmdSap.Length < 4)
                            {
                                totalComanda += (articol.pretUnit / articol.multiplu) * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                totalComanda += articol.pretUnit * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                            }


                            articoleAgenti.Add(articol);
                            departArt = articol.depart;
                            ulStoc = articol.filialaSite;
                            tipTranspCmd = articol.tipTransport;

                        }

                        dateLivrareDistrib.totalComanda = totalComanda.ToString();

                        if (ulStoc != null && !ulStoc.Equals(dateLivrareDistrib.unitLog) && !ulStoc.Equals("BV90"))
                            dateLivrareDistrib.filialaCLP = ulStoc;


                        if (tipTranspCmd != null && !tipTranspCmd.Equals(String.Empty))
                            dateLivrareDistrib.Transport = tipTranspCmd;

                        retVal = saveAVNewCmd(comanda, alertSD, alertDV, cmdAngajament, tipUser, serializer.Serialize(articoleAgenti), serializer.Serialize(comandaVanzare), serializer.Serialize(dateLivrareDistrib), true, tipUserSap);
                    }


                }

                if (articoleGed.Count > 0)
                {

                    bool paramAlertDVGed = alertDV;
                    if ((tipUser.Equals("AV") || tipUser.Equals("SD") || tipUser.Equals("KA")) && comandaVanzare.canalDistrib.Equals("10"))
                    {
                        paramAlertDVGed = false;
                        comandaVanzare.comandaBlocata = "0";
                    }

                    if (!dateLivrareGed.unitLog.Contains("40"))
                        dateLivrareGed.unitLog = dateLivrareGed.unitLog.Substring(0, 2) + "2" + dateLivrareGed.unitLog.Substring(3, 1);

                    dateLivrareGed.totalComanda = totalComandaGed.ToString();

                    List<ArticolComanda> sortedArticoleDistrib = articoleGed.OrderBy(order => order.filialaSite).ToList();

                    bool calcTransport = dateLivrareGed.Transport.Equals("TRAP");

                    bool alertSDGed = false;
                    bool alertDVGed = getTipAlertDVGed(articoleGed);

                    if (alertDVGed)
                        alertSDGed = true;
                    else
                        alertSDGed = getTipAlertSDGed(articoleGed);

                    {

                        string ulStoc = sortedArticoleDistrib[0].filialaSite;
                        List<ArticolComanda> articoleAgenti = new List<ArticolComanda>();
                        double totalComanda = 0;
                        string comenziGed = "";
                        double valTransp = 0;
                        string tipTranspCmd = "";

                        foreach (var articol in sortedArticoleDistrib)
                        {

                            if ((ulStoc != null && articol.filialaSite != null && !ulStoc.Equals(articol.filialaSite)))
                            {
                                dateLivrareDistrib.totalComanda = totalComanda.ToString();

                                if (!articol.filialaSite.Equals(dateLivrareDistrib.unitLog) && !HelperComenzi.isUlEquals(articol.filialaSite, dateLivrareDistrib.unitLog) && !articol.filialaSite.Equals("BV90") && !ulStoc.Equals("BV90"))
                                    dateLivrareDistrib.filialaCLP = ulStoc;

                                if (tipTranspCmd != null && !tipTranspCmd.Equals(String.Empty))
                                    dateLivrareDistrib.Transport = tipTranspCmd;

                                retVal = saveAVNewCmd(comanda, alertSD, paramAlertDVGed, cmdAngajament, tipUser, serializer.Serialize(articoleAgenti), serializer.Serialize(comandaVanzare), serializer.Serialize(dateLivrareDistrib), calcTransport, tipUserSap);

                                if (retVal.Contains("#"))
                                {
                                    string[] varArrayI = retVal.Split('#');

                                    if (comenziGed == String.Empty)
                                        comenziGed = varArrayI[2];
                                    else
                                        comenziGed += "," + varArrayI[2];

                                    valTransp += Double.Parse(varArrayI[1]);
                                }

                                articoleAgenti.Clear();
                                totalComanda = 0;
                            }

                            if (!comandaVanzare.nrCmdSap.Equals("-1") || comandaVanzare.nrCmdSap.Length < 4)
                            {
                                totalComanda += (articol.pretUnit / articol.multiplu) * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                totalComanda += articol.pretUnit * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                            }

                            articoleAgenti.Add(articol);
                            ulStoc = articol.filialaSite;
                            tipTranspCmd = articol.tipTransport;
                        }


                        dateLivrareDistrib.totalComanda = totalComanda.ToString();

                        if (ulStoc != null && !ulStoc.Equals(dateLivrareDistrib.unitLog) && !HelperComenzi.isUlEquals(ulStoc, dateLivrareDistrib.unitLog) && !ulStoc.Equals("BV90"))
                            dateLivrareDistrib.filialaCLP = ulStoc;

                        if (tipTranspCmd != null && !tipTranspCmd.Equals(String.Empty))
                            dateLivrareDistrib.Transport = tipTranspCmd;

                        retVal = saveAVNewCmd(comanda, alertSD, paramAlertDVGed, cmdAngajament, tipUser, serializer.Serialize(articoleAgenti), serializer.Serialize(comandaVanzare), serializer.Serialize(dateLivrareDistrib), calcTransport, tipUserSap);

                        if (retVal.Contains("#"))
                        {
                            string[] varArray = retVal.Split('#');

                            if (comenziGed == String.Empty)
                                comenziGed = varArray[2];
                            else
                                comenziGed += "," + varArray[2];

                            valTransp += Double.Parse(varArray[1]);

                            retVal = "100#" + valTransp + "#" + comenziGed;
                        }


                    }

                }

            }
            catch (Exception ex)
            {
                sendErrorToMail("verificaArticoleMav: " + ex.ToString() + " \n " + alertSD + " \n " + alertDV + " \n " + cmdAngajament + " \n " + tipUser + " \n " + JSONArt + " \n " + JSONComanda + " \n " + JSONDateLivrare + " \n " + tipUserSap);
            }

            return retVal;
        }

        private string verificaArticoleMAV_nou(string comanda, bool alertSD, bool alertDV, bool cmdAngajament, string tipUser, string JSONArt, string JSONComanda, string JSONDateLivrare, string tipUserSap, string idCmdAmob)
        {
            string retVal = "-1";

            var serializer = new JavaScriptSerializer();

            try
            {

                DateLivrare dateLivrareGed, dateLivrareDistrib;
                List<ArticolComanda> articoleGed, articoleDistrib;

                double totalComandaGed = 0, totalComandaDistrib = 0;

                ComandaVanzare comandaVanzare = serializer.Deserialize<ComandaVanzare>(JSONComanda);
                DateLivrare dateLivrare = serializer.Deserialize<DateLivrare>(JSONDateLivrare);
                List<ArticolComanda> articolComanda = serializer.Deserialize<List<ArticolComanda>>(JSONArt);

                List<CostTransportMathaus> costTransport = new List<CostTransportMathaus>();

                if (dateLivrare.costTransportMathaus != null)
                    costTransport = serializer.Deserialize<List<CostTransportMathaus>>(dateLivrare.costTransportMathaus);

                dateLivrareGed = dateLivrare;
                dateLivrareDistrib = dateLivrare;

                comandaVanzare.parrentId = GeneralUtils.getUniqueIdFromCode(dateLivrareDistrib.codAgent);

                bool isCmdGed = isUnitLogGed(dateLivrare.unitLog);

                articoleGed = new List<ArticolComanda>();
                articoleDistrib = new List<ArticolComanda>();

                if (isCmdGed)
                {
                    articoleGed = articolComanda;
                    totalComandaGed = Double.Parse(dateLivrare.totalComanda, CultureInfo.InvariantCulture);
                }
                else
                {

                    foreach (var articol in articolComanda)
                    {
                        if (articol.depart.Equals("11"))
                        {
                            articoleGed.Add(articol);
                            totalComandaGed += articol.pret;
                        }
                        else
                        {
                            articoleDistrib.Add(articol);

                            if (comandaVanzare.nrCmdSap.Length < 4)
                            {
                                totalComandaDistrib += (articol.pretUnit / articol.multiplu) * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                totalComandaDistrib += articol.pretUnit * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                            }


                        }
                    }
                }

                if (articoleDistrib.Count > 0)
                {
                    dateLivrareDistrib.totalComanda = totalComandaDistrib.ToString();
                    List<ArticolComanda> sortedArticoleDistrib = articoleDistrib.OrderBy(order => order.depart).ThenBy(order => order.filialaSite).ToList();


                    {
                        string departArt = sortedArticoleDistrib[0].depart;
                        string ulStoc = sortedArticoleDistrib[0].filialaSite;
                        string tipTranspCmd = "";

                        List<ArticolComanda> articoleAgenti = new List<ArticolComanda>();
                        double totalComanda = 0;

                        foreach (var articol in sortedArticoleDistrib)
                        {
                            if (!departArt.Equals(articol.depart) || (ulStoc != null && articol.filialaSite != null && !ulStoc.Equals(articol.filialaSite)))
                            {
                                dateLivrareDistrib.totalComanda = totalComanda.ToString();

                                if (!articol.filialaSite.Equals(dateLivrareDistrib.unitLog) && !articol.filialaSite.Equals("BV90") && !ulStoc.Equals("BV90"))
                                    dateLivrareDistrib.filialaCLP = ulStoc;


                                if (tipTranspCmd != null && !tipTranspCmd.Equals(String.Empty))
                                    dateLivrareDistrib.Transport = tipTranspCmd;

                                retVal = saveAVNewCmd(comanda, alertSD, alertDV, cmdAngajament, tipUser, serializer.Serialize(articoleAgenti), serializer.Serialize(comandaVanzare), serializer.Serialize(dateLivrareDistrib), false, tipUserSap);
                                articoleAgenti.Clear();
                                totalComanda = 0;
                                dateLivrareDistrib.filialaCLP = " ";
                            }



                            if (comandaVanzare.nrCmdSap.Length < 4)
                            {
                                totalComanda += (articol.pretUnit / articol.multiplu) * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                totalComanda += articol.pretUnit * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                            }


                            articoleAgenti.Add(articol);
                            departArt = articol.depart;
                            ulStoc = articol.filialaSite;
                            tipTranspCmd = articol.tipTransport;

                        }

                        dateLivrareDistrib.totalComanda = totalComanda.ToString();

                        if (ulStoc != null && !ulStoc.Equals(dateLivrareDistrib.unitLog) && !ulStoc.Equals("BV90"))
                            dateLivrareDistrib.filialaCLP = ulStoc;


                        if (tipTranspCmd != null && !tipTranspCmd.Equals(String.Empty))
                            dateLivrareDistrib.Transport = tipTranspCmd;

                        retVal = saveAVNewCmd(comanda, alertSD, alertDV, cmdAngajament, tipUser, serializer.Serialize(articoleAgenti), serializer.Serialize(comandaVanzare), serializer.Serialize(dateLivrareDistrib), true, tipUserSap);
                    }


                }

                if (articoleGed.Count > 0)
                {

                    bool paramAlertDVGed = alertDV;
                    if ((tipUser.Equals("AV") || tipUser.Equals("SD") || tipUser.Equals("KA")) && comandaVanzare.canalDistrib.Equals("10"))
                    {
                        paramAlertDVGed = false;
                        comandaVanzare.comandaBlocata = "0";
                    }

                    if (!dateLivrareGed.unitLog.Contains("40"))
                        dateLivrareGed.unitLog = dateLivrareGed.unitLog.Substring(0, 2) + "2" + dateLivrareGed.unitLog.Substring(3, 1);

                    dateLivrareGed.totalComanda = totalComandaGed.ToString();

                    List<ArticolComanda> sortedArticoleDistrib = articoleGed.OrderBy(order => order.filialaSite).ToList();

                    bool calcTransport = dateLivrareGed.Transport.Equals("TRAP");

                    bool alertSDGed = false;
                    bool alertDVGed = getTipAlertDVGed(articoleGed);

                    if (alertDVGed)
                        alertSDGed = true;
                    else
                        alertSDGed = getTipAlertSDGed(articoleGed);


                    {

                        string ulStoc = sortedArticoleDistrib[0].filialaSite;
                        List<ArticolComanda> articoleAgenti = new List<ArticolComanda>();
                        double totalComanda = 0;
                        string comenziGed = "";
                        double valTransp = 0;
                        string tipTranspCmd = "";

                        foreach (var articol in sortedArticoleDistrib)
                        {

                            if ((ulStoc != null && articol.filialaSite != null && !ulStoc.Equals(articol.filialaSite)))
                            {
                                dateLivrareDistrib.totalComanda = totalComanda.ToString();

                                if (!HelperComenzi.isUlEquals(ulStoc, dateLivrareDistrib.unitLog) && !articol.filialaSite.Equals("BV90") && !ulStoc.Equals("BV90"))
                                    dateLivrareDistrib.filialaCLP = ulStoc;

                                if (tipTranspCmd != null && !tipTranspCmd.Equals(String.Empty))
                                    dateLivrareDistrib.Transport = tipTranspCmd;

                                retVal = saveAVNewCmd(comanda, alertSD, paramAlertDVGed, cmdAngajament, tipUser, serializer.Serialize(articoleAgenti), serializer.Serialize(comandaVanzare), serializer.Serialize(dateLivrareDistrib), calcTransport, tipUserSap);

                                if (retVal.Contains("#"))
                                {
                                    string[] varArrayI = retVal.Split('#');

                                    if (comenziGed == String.Empty)
                                        comenziGed = varArrayI[2];
                                    else
                                        comenziGed += "," + varArrayI[2];

                                    valTransp += Double.Parse(varArrayI[1]);
                                }

                                articoleAgenti.Clear();
                                totalComanda = 0;
                                dateLivrareDistrib.filialaCLP = " ";
                            }

                            if (!comandaVanzare.nrCmdSap.Equals("-1") || comandaVanzare.nrCmdSap.Length < 4)
                            {
                                totalComanda += (articol.pretUnit / articol.multiplu) * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                totalComanda += articol.pretUnit * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                            }

                            articoleAgenti.Add(articol);
                            ulStoc = articol.filialaSite;
                            tipTranspCmd = articol.tipTransport;
                        }


                        dateLivrareDistrib.totalComanda = totalComanda.ToString();

                        if (ulStoc != null && !HelperComenzi.isUlEquals(ulStoc, dateLivrareDistrib.unitLog) && !ulStoc.Equals("BV90"))
                            dateLivrareDistrib.filialaCLP = ulStoc;

                        if (tipTranspCmd != null && !tipTranspCmd.Equals(String.Empty))
                            dateLivrareDistrib.Transport = tipTranspCmd;

                        retVal = saveAVNewCmd(comanda, alertSD, paramAlertDVGed, cmdAngajament, tipUser, serializer.Serialize(articoleAgenti), serializer.Serialize(comandaVanzare), serializer.Serialize(dateLivrareDistrib), calcTransport, tipUserSap);

                        if (retVal.Contains("#"))
                        {
                            string[] varArray = retVal.Split('#');

                            if (comenziGed == String.Empty)
                                comenziGed = varArray[2];
                            else
                                comenziGed += "," + varArray[2];

                            valTransp += Double.Parse(varArray[1]);

                            retVal = "100#" + valTransp + "#" + comenziGed;
                        }


                    }

                }

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }

            return retVal;
        }

        private string verificaArticoleMAV_old(string comanda, bool alertSD, bool alertDV, bool cmdAngajament, string tipUser, string JSONArt, string JSONComanda, string JSONDateLivrare, string tipUserSap)
        {
            string retVal = "-1";


            var serializer = new JavaScriptSerializer();

            try
            {

                DateLivrare dateLivrareGed, dateLivrareDistrib;
                List<ArticolComanda> articoleGed, articoleDistrib;

                double totalComandaGed = 0, totalComandaDistrib = 0;

                ComandaVanzare comandaVanzare = serializer.Deserialize<ComandaVanzare>(JSONComanda);
                DateLivrare dateLivrare = serializer.Deserialize<DateLivrare>(JSONDateLivrare);
                List<ArticolComanda> articolComanda = serializer.Deserialize<List<ArticolComanda>>(JSONArt);


                dateLivrareGed = dateLivrare;
                dateLivrareDistrib = dateLivrare;

                comandaVanzare.parrentId = GeneralUtils.getUniqueIdFromCode(dateLivrareDistrib.codAgent);

                bool isCmdGed = isUnitLogGed(dateLivrare.unitLog);

                articoleGed = new List<ArticolComanda>();
                articoleDistrib = new List<ArticolComanda>();

                if (isCmdGed)
                {
                    articoleGed = articolComanda;
                    totalComandaGed = Double.Parse(dateLivrare.totalComanda, CultureInfo.InvariantCulture);
                }
                else
                {

                    foreach (var articol in articolComanda)
                    {
                        if (articol.depart.Equals("11"))
                        {
                            articoleGed.Add(articol);
                            totalComandaGed += articol.pret;
                        }
                        else
                        {
                            articoleDistrib.Add(articol);

                            if (comandaVanzare.nrCmdSap.Length < 4)
                            {
                                totalComandaDistrib += (articol.pretUnit / articol.multiplu) * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                totalComandaDistrib += articol.pretUnit * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                            }


                        }
                    }
                }

                if (articoleDistrib.Count > 0)
                {
                    dateLivrareDistrib.totalComanda = totalComandaDistrib.ToString();
                    List<ArticolComanda> sortedArticoleDistrib = articoleDistrib.OrderBy(order => order.depart).ThenBy(order => order.filialaSite).ToList();

                    if (tipUser.Equals("KA"))
                    {
                        retVal = saveKANewCmd(comanda, alertSD, alertDV, cmdAngajament, tipUser, serializer.Serialize(sortedArticoleDistrib), JSONComanda, serializer.Serialize(dateLivrareDistrib), true);
                    }
                    else
                    {
                        string departArt = sortedArticoleDistrib[0].depart;
                        string ulStoc = sortedArticoleDistrib[0].filialaSite;

                        List<ArticolComanda> articoleAgenti = new List<ArticolComanda>();
                        double totalComanda = 0;
                        

                        foreach (var articol in sortedArticoleDistrib)
                        {
                            if (!departArt.Equals(articol.depart) || (ulStoc != null && articol.filialaSite!= null && !ulStoc.Equals(articol.filialaSite)))
                            {
                                dateLivrareDistrib.totalComanda = totalComanda.ToString();
                                retVal = saveAVNewCmd(comanda, alertSD, alertDV, cmdAngajament, tipUser, serializer.Serialize(articoleAgenti), serializer.Serialize(comandaVanzare), serializer.Serialize(dateLivrareDistrib), false, tipUserSap);
                                articoleAgenti.Clear();
                                totalComanda = 0;
                            }



                            if (comandaVanzare.nrCmdSap.Length < 4)
                            {
                                totalComanda += (articol.pretUnit / articol.multiplu) * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                totalComanda += articol.pretUnit * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                            }


                            articoleAgenti.Add(articol);
                            departArt = articol.depart;
                            ulStoc = articol.filialaSite;

                        }

                        dateLivrareDistrib.totalComanda = totalComanda.ToString();
                        retVal = saveAVNewCmd(comanda, alertSD, alertDV, cmdAngajament, tipUser, serializer.Serialize(articoleAgenti), serializer.Serialize(comandaVanzare), serializer.Serialize(dateLivrareDistrib), true, tipUserSap);
                    }


                }

                if (articoleGed.Count > 0)
                {

                    if (!dateLivrareGed.unitLog.Contains("40"))
                        dateLivrareGed.unitLog = dateLivrareGed.unitLog.Substring(0, 2) + "2" + dateLivrareGed.unitLog.Substring(3, 1);

                    dateLivrareGed.totalComanda = totalComandaGed.ToString();

                    bool calcTransport = true;
                    if (articoleDistrib.Count > 0)
                        calcTransport = false;

                    bool alertSDGed = false;
                    bool alertDVGed = getTipAlertDVGed(articoleGed);

                    if (alertDVGed)
                        alertSDGed = true;
                    else
                        alertSDGed = getTipAlertSDGed(articoleGed);

                    if (tipUser.Equals("KA"))
                    {
                        retVal = saveKANewCmd(comanda, alertSDGed, alertDVGed, cmdAngajament, tipUser, serializer.Serialize(articoleGed), serializer.Serialize(comandaVanzare), serializer.Serialize(dateLivrareGed), calcTransport);
                    }
                    else
                    {
                        retVal = saveAVNewCmd(comanda, alertSDGed, alertDVGed, cmdAngajament, tipUser, serializer.Serialize(articoleGed), serializer.Serialize(comandaVanzare), serializer.Serialize(dateLivrareGed), calcTransport, tipUserSap);
                    }

                    if (retVal.Contains('#'))
                    {
                        string[] varArray = retVal.Split('#');
                        retVal = salveazaCmdGED(varArray[2], calcTransport, comandaVanzare.canalDistrib);
                    }


                }

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }


            return retVal;
        }

        private bool getTipAlertSDGed(List<ArticolComanda> articoleGed)
        {
            bool alert = false;

            foreach (ArticolComanda articol in articoleGed)
            {
                if (articol.observatii != null && articol.observatii.ToLower().Contains("sd"))
                {
                    alert = true;
                    break;
                }
            }


            return alert;
        }


        private bool getTipAlertDVGed(List<ArticolComanda> articoleGed)
        {
            bool alert = false;

            foreach (ArticolComanda articol in articoleGed)
            {
                if (articol.observatii != null && articol.observatii.ToLower().Contains("dv"))
                {
                    alert = true;
                    break;
                }
            }


            return alert;
        }


        private string salveazaCmdGED(string comanda, bool calTransport, string canalDistrib)
        {
            string retVal = "-1";

            SAPWebServices.ZTBL_WEBSERVICE webService = null;

            try
            {


                webService = new ZTBL_WEBSERVICE();

                SAPWebServices.ZcreazaComanda inParam = new SAPWebServices.ZcreazaComanda();
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                webService.Credentials = nc;
                webService.Timeout = 600000;
                inParam.FaraTransp = calTransport ? " " : "X";
                inParam.Id = comanda;
                inParam.GvEvent = "C";  //C - creaza comanda, S - simulare pret transport
                inParam.Canal = canalDistrib;
                SAPWebServices.ZcreazaComandaResponse outParam = webService.ZcreazaComanda(inParam);

                retVal = outParam.VOk.ToString();



            }
            catch (Exception ex)
            {
                retVal = "-1";
                sendErrorToMail(ex.ToString() + " " + comanda);
            }
            finally
            {
                try
                {
                    if (webService != null)
                        webService.Dispose();
                }
                catch (Exception ex)
                {

                }

            }

            return retVal;
        }



        public string saveAVNewCmd(string comanda, bool alertSD, bool alertDV, bool cmdAngajament, string tipUser, string JSONArt, string JSONComanda, string JSONDateLivrare, bool calcTransport, string tipUserSap)
        {

            string retVal = "-1";

            var serializer = new JavaScriptSerializer();

            ComandaVanzare comandaVanzare = serializer.Deserialize<ComandaVanzare>(JSONComanda);
            DateLivrare dateLivrare = serializer.Deserialize<DateLivrare>(JSONDateLivrare);
            List<ArticolComanda> articolComanda = serializer.Deserialize<List<ArticolComanda>>(JSONArt);

            SAPWebServices.ZTBL_WEBSERVICE webService = null;
            OracleConnection connection = new OracleConnection();
            OracleTransaction transaction = null;


            string query = "";
            string refSAP = "-1";
            bool hasDepozMAV1 = false;
            string idComanda = "";

            try
            {
                string connectionString = GetConnectionString_android();

                DateTime cDate = DateTime.Now;
                string year = cDate.Year.ToString();
                string day = cDate.Day.ToString("00");
                string month = cDate.Month.ToString("00");
                string nowDate = year + month + day;
                string nowTime = cDate.Hour.ToString("00") + cDate.Minute.ToString("00") + cDate.Second.ToString("00");

                string cmdStatus = "0";

                string valSD = "";
                string valDV = "";
                string conditieID = "", termenPlata = "", observatiiLivr = "";
                string uLog = "", transp = "", paramCmd = "";
                string codClient = "-1", numeClient = "-1", cnpClient = "-1";

                connection.ConnectionString = connectionString;
                connection.Open();
                string tipCmd = "0", unitLogAlt = "NN10";

                cmdStatus = comandaVanzare.comandaBlocata;


                if (cmdAngajament)
                {
                    tipCmd = "11";
                    cmdStatus = "6";
                    alertDV = true;
                }

                if (dateLivrare.isComandaACZC)
                    cmdStatus = "1";


                OracleCommand cmd = connection.CreateCommand();

                //departament comanda
                string depart = "00";
                string tempDepart = "00";  //pentru comenzile ged facute de agenti
                //consilieri
                if (tipUser.Equals("CV") || tipUser.Equals("SITE"))
                {
                    depart = "11";

                    tempDepart = getDepartAgent(dateLivrare.codAgent);

                    codClient = comandaVanzare.codClient;
                    numeClient = comandaVanzare.numeClient;
                    cnpClient = comandaVanzare.cnpClient;

                    if (cnpClient == null)
                        cnpClient = getCnpFromComanda(comanda);


                }
                //agenti si ka
                else
                {

                    codClient = comandaVanzare.codClient;

                    if (comandaVanzare.cnpClient != null)
                        cnpClient = comandaVanzare.cnpClient;


                    string codArtDepart = articolComanda[0].codArticol;

                    if ((codArtDepart.StartsWith("30") || codArtDepart.StartsWith("00")) && articolComanda.Count >= 2)
                        codArtDepart = articolComanda[1].codArticol;

                    if ((codArtDepart.StartsWith("30") || codArtDepart.StartsWith("00")) && articolComanda.Count >= 3)
                        codArtDepart = articolComanda[2].codArticol;

                    if (codArtDepart.Length == 8)
                        codArtDepart = "0000000000" + codArtDepart;

                    cmd.CommandText = " select grup_vz from articole where cod =:codArt ";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codArtDepart;

                    OracleDataReader oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        depart = oReader.GetString(0);
                    }
                    oReader.Close();
                    oReader.Dispose();

                    if (articolComanda[0].depozit.ToUpper().Contains("MAV") && articolComanda[0].departAprob != null && !articolComanda[0].departAprob.Equals("00"))
                    {
                        depart = articolComanda[0].departAprob;
                    }



                }
                //sf. departament



                unitLogAlt = comandaVanzare.filialaAlternativa;

                //BV90 doar pentru electrice
                if (unitLogAlt == "BV90")
                {
                    //exceptie pentru electrice si feronerie
                    if (depart.Equals("05") || depart.Equals("02") || depart.Equals("11") || depart.Equals("01"))
                    {

                    }
                    else
                    {
                        retVal = "-1";
                        //sendErrorToMail(comanda);
                        return retVal;
                    }

                }


                refSAP = comandaVanzare.nrCmdSap.Trim().Length == 0 ? "-1" : comandaVanzare.nrCmdSap;
                conditieID = comandaVanzare.conditieID != null ? comandaVanzare.conditieID : "-1";

                {
                    termenPlata = dateLivrare.termenPlata;
                    observatiiLivr = dateLivrare.obsLivrare;
                }

                

                DateTime dataLivrare;
                if (dateLivrare.dataLivrare.Length == 1)
                    dataLivrare = DateTime.Today.AddDays(Double.Parse(dateLivrare.dataLivrare));
                else
                    dataLivrare = GeneralUtils.getDateFromString(dateLivrare.dataLivrare);

               


                //pt. calcul pret transport

                uLog = dateLivrare.unitLog;
                transp = dateLivrare.Transport;


                //adresa livrare consilieri
                string codJudetLivrare = " ", orasLivrare = " ", stradaLivrare = " ";
                string codJudetLivrareComanda = dateLivrare.codJudet, orasLivrareComanda = dateLivrare.Oras, stradaLivrareComanda = dateLivrare.Strada + " ";

                if (comandaVanzare.adresaLivrareGed.Trim().Length > 0)
                {
                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    AdresaLivrareCV oAdrLivrare = ser.Deserialize<AdresaLivrareCV>(comandaVanzare.adresaLivrareGed);

                    if (oAdrLivrare.codJudet == null || oAdrLivrare.codJudet.Trim().Length == 0)
                    {

                    }
                    else {
                        codJudetLivrare = oAdrLivrare.codJudet;
                        orasLivrare = oAdrLivrare.oras;
                        stradaLivrare = oAdrLivrare.strada;

                        if (Utils.isUserTest(dateLivrare.codAgent))
                        {
                            codJudetLivrareComanda = oAdrLivrare.codJudet;
                            orasLivrareComanda = oAdrLivrare.oras;
                            stradaLivrareComanda = oAdrLivrare.strada + " ";

                            codJudetLivrare = dateLivrare.codJudet;
                            orasLivrare = dateLivrare.Oras;
                            stradaLivrare = dateLivrare.Strada;
                        }
                    }
                }

                //sf. adresa livrare



                transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;

                //inserare antet comanda
                query = " insert into sapprd.zcomhead_tableta(mandt,id,cod_client,ul,status,status_aprov ,datac,cantar,cod_agent,cod_init,tip_plata,pers_contact,telefon,adr_livrare, " +
                        " valoare,mt,com_referinta,accept1,accept2,fact_red, city, region, pmnttrms , obstra, timpc, ketdat, docin, adr_noua, depart, obsplata, addrnumber, nume_client, " +
                        " stceg, tip_pers, val_incasata, site, email, mod_av, cod_j, adr_livrare_d, city_d, region_d, aprob_cv_necesar, macara, val_min_tr, id_obiectiv, " +
                        " adresa_obiectiv, parent_id, client_raft, meserias,fact_palet_separat, lifnr, lifnr_prod, descoperita, prog_livr, livr_sambata, bloc, ref_client, ac_zc, fil_plata) " +
                        " values ('900',pk_key.nextval, :codCl,:ul,:status,:status_aprov, " +
                        " :datac,:cantar,:agent,:codinit,:plata,:perscont,:tel,:adr,:valoare,:transp,:comsap,:accept1,:accept2,:factred,:city,:region,:termplt,:obslivr,:timpc,:datalivrare, " +
                        " :tipDocIn, :adrNoua, :depart, :obsplata, :adrnumber, :numeClient, :cnpClient, :tipPers, :valIncasata, :cmdSite, :email, :mod_av, :codJ, :adr_livrare_d, :city_d, :region_d, " +
                        " :necesarCVAprob, :macara, :val_min_tr, :idObiectiv, :adresaObiectiv, :parent_id, :client_raft, :meserias, :factPaletSeparat, :lifnr, :lifnr_prod, :descoperita, :progrLivr, :livrSambata, :bloc, :refClient, :aczc, :filPlata ) " +
                        " returning id into :id ";


                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codCl", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                cmd.Parameters.Add(":ul", OracleType.VarChar, 4).Direction = ParameterDirection.Input;


                string filialaCmd = dateLivrare.unitLog;


                if (tipUser.Equals("CV") || tipUserSap.Equals(Constants.tipInfoAv) || tipUserSap.Equals(Constants.tipSMR) || tipUserSap.Equals(Constants.tipCVR) || tipUserSap.Equals(Constants.tipSSCM) || tipUserSap.Equals(Constants.tipCGED))
                {
                    if (!dateLivrare.unitLog.Substring(2, 1).Equals("4"))
                        filialaCmd = dateLivrare.unitLog.Substring(0, 2) + "2" + dateLivrare.unitLog.Substring(3, 1);
                }

                cmd.Parameters[1].Value = filialaCmd;

                cmd.Parameters.Add(":status", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = tipCmd;

                cmd.Parameters.Add(":status_aprov", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = cmdStatus;

                cmd.Parameters.Add(":datac", OracleType.VarChar, 10).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = nowDate;

                cmd.Parameters.Add(":cantar", OracleType.VarChar, 1).Direction = ParameterDirection.Input;
                string cnt = "0";
                if (dateLivrare.Cantar.Equals("DA"))
                    cnt = "1";
                cmd.Parameters[5].Value = cnt;

                string lclCodAgent = dateLivrare.codAgent;
                if (tipUserSap != null && (tipUserSap.Equals(Constants.tipInfoAv) || tipUserSap.Equals(Constants.tipSMR) || tipUserSap.Equals(Constants.tipCVR) || tipUserSap.Equals(Constants.tipSSCM) || tipUserSap.Equals(Constants.tipCGED) || tipUserSap.Equals(Constants.tipOIVPD)))
                    lclCodAgent = OperatiiClienti.getAgentAlocat(connection, transaction, depart, codClient, dateLivrare.codAgent);

                cmd.Parameters.Add(":agent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = lclCodAgent;

                cmd.Parameters.Add(":codinit", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[7].Value = lclCodAgent;

                cmd.Parameters.Add(":plata", OracleType.VarChar, 4).Direction = ParameterDirection.Input;
                cmd.Parameters[8].Value = dateLivrare.tipPlata;

                if (dateLivrare.tipPlata.Trim().Equals("LC"))
                {
                    return "-1";
                }

                cmd.Parameters.Add(":perscont", OracleType.VarChar, 25).Direction = ParameterDirection.Input;
                cmd.Parameters[9].Value = dateLivrare.persContact;

                cmd.Parameters.Add(":tel", OracleType.VarChar, 15).Direction = ParameterDirection.Input;
                cmd.Parameters[10].Value = dateLivrare.nrTel;

                cmd.Parameters.Add(":adr", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[11].Value = stradaLivrareComanda;

                cmd.Parameters.Add(":valoare", OracleType.Number, 15).Direction = ParameterDirection.Input;
                string strValoareComanda = dateLivrare.totalComanda;

                if (tipUser.Equals("CV") || tipUser.Equals("KA3"))   //se adauga valoare transport
                {
                    string strValoareIncasare = comandaVanzare.valoareIncasare == null ? "0" : comandaVanzare.valoareIncasare;
                    double valoareComanda = Double.Parse(dateLivrare.totalComanda, CultureInfo.InvariantCulture) + Double.Parse(strValoareIncasare, CultureInfo.InvariantCulture);

                    strValoareComanda = valoareComanda.ToString();
                }
                cmd.Parameters[12].Value = strValoareComanda;

                cmd.Parameters.Add(":transp", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[13].Value = dateLivrare.Transport;

                cmd.Parameters.Add(":comsap", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[14].Value = refSAP;

                cmd.Parameters.Add(":accept1", OracleType.VarChar, 12).Direction = ParameterDirection.Input;

                valSD = " ";
                if (tipUser == "AV")
                {
                    if (alertSD || dateLivrare.adrLivrNoua) //adr. livrare noua
                        valSD = "X";
                    else
                        valSD = " ";

                    if (alertDV)
                        valSD = "X";
                }

                if (tipUser == "KA")
                {
                    if (alertSD)
                        valSD = "X";
                }

                if (tipUser == "CV" && !tempDepart.Equals("11"))
                {
                    if (alertSD)
                        valSD = "X";
                }

                if (dateLivrare.isComandaACZC)
                {
                    valSD = "X";
                }

                cmd.Parameters[15].Value = valSD;


                cmd.Parameters.Add(":accept2", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                if (alertDV)
                    valDV = "X";
                else
                    valDV = " ";
                cmd.Parameters[16].Value = valDV;

                cmd.Parameters.Add(":factred", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                string factRed = dateLivrare.factRed;


                if (comandaVanzare.factRedSeparat.Equals("DA") || comandaVanzare.factRedSeparat.Equals("X"))
                    factRed = "X";

                if (comandaVanzare.factRedSeparat.Equals("NU") || comandaVanzare.factRedSeparat.Equals(" "))
                    factRed = " ";

                if (comandaVanzare.factRedSeparat.Equals("R"))
                    factRed = "R";

                cmd.Parameters[17].Value = factRed;

                cmd.Parameters.Add(":city", OracleType.VarChar, 75).Direction = ParameterDirection.Input;
                cmd.Parameters[18].Value = orasLivrareComanda;

                cmd.Parameters.Add(":region", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[19].Value = codJudetLivrareComanda;

                cmd.Parameters.Add(":termplt", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[20].Value = dateLivrare.termenPlata;

                cmd.Parameters.Add(":obslivr", OracleType.VarChar, 300).Direction = ParameterDirection.Input;
                cmd.Parameters[21].Value = dateLivrare.obsLivrare;

                cmd.Parameters.Add(":timpc", OracleType.VarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[22].Value = nowTime;

                cmd.Parameters.Add(":dataLivrare", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[23].Value = dataLivrare.ToString("yyyyMMdd");

                cmd.Parameters.Add(":tipDocIn", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[24].Value = dateLivrare.tipDocInsotitor;

                cmd.Parameters.Add(":adrNoua", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                string adrLivrNoua = " ";
                if (dateLivrare.adrLivrNoua.Equals(true))
                    adrLivrNoua = "X";
                cmd.Parameters[25].Value = adrLivrNoua;

                cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[26].Value = depart;

                cmd.Parameters.Add(":obsplata", OracleType.VarChar, 300).Direction = ParameterDirection.Input;
                cmd.Parameters[27].Value = dateLivrare.obsPlata;

                cmd.Parameters.Add(":adrnumber", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[28].Value = dateLivrare.addrNumber;

                cmd.Parameters.Add(":numeClient", OracleType.VarChar, 120).Direction = ParameterDirection.Input;
                cmd.Parameters[29].Value = comandaVanzare.numeClient == null ? "-1" : Regex.Replace(comandaVanzare.numeClient, "\\(.*", "");

                cmd.Parameters.Add(":cnpClient", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                string localCnpClient;
                if (Regex.Replace(cnpClient.ToUpper(), @"RO+", "").Length == 13)
                    localCnpClient = " ";
                else
                    localCnpClient = cnpClient.Equals("-1") ? " " : Utils.checkROCnpClient(cnpClient);
                cmd.Parameters[30].Value = localCnpClient;

                cmd.Parameters.Add(":tipPers", OracleType.VarChar, 6).Direction = ParameterDirection.Input;

                string paramTipUser = tipUser;

                cmd.Parameters[31].Value = paramTipUser;

                cmd.Parameters.Add(":valIncasata", OracleType.Number, 15).Direction = ParameterDirection.Input;
                string strValIncasata = dateLivrare.valoareIncasare == null ? "0" : dateLivrare.valoareIncasare;

                if (tipUser.Equals("CV") || tipUser.Equals("KA3"))
                    strValIncasata = "0";

                if (strValIncasata.Length > 3 && strValIncasata[strValIncasata.Length - 3] == ',')
                    strValIncasata = Double.Parse(strValIncasata, CultureInfo.GetCultureInfo("ro-RO")).ToString();

                cmd.Parameters[32].Value = strValIncasata;

                string strUserSite;
                if (tipUserSap != null && tipUserSap.Equals("CVO"))
                    strUserSite = "X";
                else
                    strUserSite = comandaVanzare.userSite;

                cmd.Parameters.Add(":cmdSite", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[33].Value = strUserSite;

                cmd.Parameters.Add(":email", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[34].Value = comandaVanzare.userSiteMail.Replace("~", "@");

                cmd.Parameters.Add(":mod_av", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[35].Value = comandaVanzare.isValIncModif;

                cmd.Parameters.Add(":codJ", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[36].Value = comandaVanzare.codJ.Length > 0 ? comandaVanzare.codJ : " ";

                cmd.Parameters.Add(":adr_livrare_d", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[37].Value = stradaLivrare + " ";

                cmd.Parameters.Add(":city_d", OracleType.VarChar, 75).Direction = ParameterDirection.Input;
                cmd.Parameters[38].Value = orasLivrare;

                cmd.Parameters.Add(":region_d", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[39].Value = codJudetLivrare;

                cmd.Parameters.Add(":necesarCVAprob", OracleType.VarChar, 90).Direction = ParameterDirection.Input;

                string necesarAprob = comandaVanzare.necesarAprobariCV != null ? comandaVanzare.necesarAprobariCV.Length > 0 ? comandaVanzare.necesarAprobariCV.Replace("040", "04").Replace("041", "04") : " " : " ";

                if ((tipUserSap.Equals("SMR") || tipUserSap.Equals("SRM") || tipUserSap.Equals("CVS") || tipUserSap.Equals("CVR") || tipUserSap.Equals("CVIP") || tipUserSap.Equals("SDIP")) && (comandaVanzare.necesarAprobariCV != null && comandaVanzare.necesarAprobariCV.Trim().Length > 0))
                    necesarAprob = "11";

                cmd.Parameters[40].Value = necesarAprob;

                cmd.Parameters.Add(":macara", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[41].Value = dateLivrare.macara;

                cmd.Parameters.Add(":val_min_tr", OracleType.Number, 15).Direction = ParameterDirection.Input;
                cmd.Parameters[42].Value = comandaVanzare.valTransportSap;

                cmd.Parameters.Add(":val_min_tr", OracleType.Number, 15).Direction = ParameterDirection.Input;
                cmd.Parameters[43].Value = comandaVanzare.valTransportSap;

                cmd.Parameters.Add(":idObiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;

                int idObiectiv = 0;

                if (dateLivrare.idObiectiv != null && dateLivrare.idObiectiv.Length > 1)
                    idObiectiv = Int32.Parse(dateLivrare.idObiectiv);
                cmd.Parameters[44].Value = idObiectiv;

                cmd.Parameters.Add(":adresaObiectiv", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[45].Value = dateLivrare.isAdresaObiectiv ? "X" : " ";

                cmd.Parameters.Add(":parent_id", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[46].Value = comandaVanzare.parrentId != null ? long.Parse(comandaVanzare.parrentId) : -1;


                cmd.Parameters.Add(":client_raft", OracleType.VarChar, 3).Direction = ParameterDirection.Input;

                string clientRaft = " ";
                if (dateLivrare.clientRaft != null)
                    clientRaft = Boolean.Parse(dateLivrare.clientRaft) ? "X" : " ";

                cmd.Parameters[47].Value = clientRaft;

                cmd.Parameters.Add(":meserias", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[48].Value = dateLivrare.meserias.Length > 1 ? dateLivrare.meserias : " ";

                cmd.Parameters.Add(":factPaletSeparat", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[49].Value = Convert.ToBoolean(dateLivrare.factPaletiSeparat) ? "X" : " ";

                cmd.Parameters.Add(":lifnr", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[50].Value = dateLivrare.filialaCLP != null && dateLivrare.filialaCLP.Trim().Length > 0 ? dateLivrare.filialaCLP : dateLivrare.furnizorMarfa;

                cmd.Parameters.Add(":lifnr_prod", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[51].Value = dateLivrare.furnizorProduse != null ? dateLivrare.furnizorProduse : " ";

                cmd.Parameters.Add(":descoperita", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[52].Value = Convert.ToBoolean(dateLivrare.isCamionDescoperit) ? "X" : " ";

                cmd.Parameters.Add(":progrLivr", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[53].Value = dateLivrare.programLivrare != null ? dateLivrare.programLivrare : "0";


                string livrSambata = " ";
                if (!isDataSambata(dataLivrare))
                    livrSambata = dateLivrare.livrareSambata != null && dateLivrare.livrareSambata.Length > 0 ? dateLivrare.livrareSambata : " ";

                cmd.Parameters.Add(":livrSambata", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[54].Value = livrSambata;

                cmd.Parameters.Add(":bloc", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[55].Value = dateLivrare.blocScara != null && dateLivrare.blocScara.Length > 0 ? dateLivrare.blocScara : " ";

                cmd.Parameters.Add(":refClient", OracleType.VarChar, 105).Direction = ParameterDirection.Input;
                cmd.Parameters[56].Value = dateLivrare.refClient != null && dateLivrare.refClient.Length > 0 ? dateLivrare.refClient : " ";

                cmd.Parameters.Add(":aczc", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[57].Value = dateLivrare.isComandaACZC ? "X" : " ";

                cmd.Parameters.Add(":filPlata", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[58].Value = dateLivrare.filialaPlata != null && dateLivrare.filialaPlata.Trim().Length > 0 ? dateLivrare.filialaPlata : " ";

                OracleParameter idCmd = new OracleParameter("id", OracleType.Number);
                idCmd.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(idCmd);

                cmd.ExecuteNonQuery();


                //inserare articole comanda


                int pozArt = 0;
                string fakeArt = " ";

                int artLen = articolComanda.Count;
                NumberFormatInfo nfi = new NumberFormatInfo();
                nfi.NumberDecimalSeparator = ".";
                nfi.NumberGroupSeparator = string.Empty;


                string codArt = "";
                double pretUnit = 0;

                hasDepozMAV1 = false;

                for (int i = 0; i < artLen; i++)
                {

                    pozArt = (i + 1) * 10;

                    codArt = articolComanda[i].codArticol;

                    if (codArt.Equals("30101747") || codArt.Equals("30101748"))
                        codArt = HelperComenzi.getArtExcCherestea(connection, transaction, articolComanda, codArt);

                    if (codArt.Length == 8)
                        codArt = "0000000000" + codArt;

                    //material transport
                    if (codArt.Equals("000000000030101050"))
                        codArt = "000000000030101060";

                    fakeArt = " ";
                    if (codArt.Equals("000000000000000000"))
                        fakeArt = "X";



                    double valPoz = 0;

                    if (articolComanda[i].depozit.ToUpper().Contains("MAV"))
                        hasDepozMAV1 = true;

                    string ulStoc = unitLogAlt;


                    if (dateLivrare.filialaCLP != null && dateLivrare.filialaCLP.Trim().Length > 0)
                        ulStoc = dateLivrare.filialaCLP;
                    else if (tipUserSap.Equals("WOOD"))
                        ulStoc = articolComanda[i].filialaSite == null || articolComanda[i].filialaSite == "" ? " " : articolComanda[i].filialaSite;

                    string greutateArticol = articolComanda[i].greutate == null ? "0" : articolComanda[i].greutate;

                    if (tipUser.Equals("CV"))
                    {

                        if (Utils.isUserTest(dateLivrare.codAgent))
                        {
                            if (articolComanda[i].filialaSite != null || !articolComanda[i].filialaSite.Trim().Equals(String.Empty))
                                ulStoc = articolComanda[i].filialaSite;

                            if (dateLivrare.filialaCLP != null && dateLivrare.filialaCLP.Trim().Length > 0)
                                ulStoc = dateLivrare.filialaCLP;
                        }


                        pretUnit = articolComanda[i].pretUnit;
                        valPoz = articolComanda[i].pretUnit * Double.Parse(articolComanda[i].cantUmb, CultureInfo.InvariantCulture);

                        string valTransport = "0";

                        if (articolComanda[i].valTransport != null && !dateLivrare.Transport.Equals("TCLI"))
                            valTransport = articolComanda[i].valTransport;

                        int ponderareArt = articolComanda[i].ponderare == -1 ? 1 : articolComanda[i].ponderare;

                        query = " insert into sapprd.zcomdet_tableta(mandt,id,poz,status,cod,cantitate,valoare,depoz, " +
                                " transfer,valoaresap,ppoz,procent,um,pret_cl,conditie,disclient,procent_aprob,multiplu, " +
                                " val_poz,inf_pret,cant_umb,umb, ul_stoc, fake, ponderat, istoric_pret, val_transp, brgew) " +
                                " values ('900'," + idCmd.Value + ",'" + pozArt + "','" + cmdStatus + "','" + codArt + "'," + articolComanda[i].cantitate.ToString(nfi) + ", " +
                                "" + pretUnit.ToString(nfi) + ",'" + articolComanda[i].depozit + "','0',0,'0'," + articolComanda[i].procent.ToString(nfi) + ",'" +
                                articolComanda[i].um + "'," + articolComanda[i].pretUnitarClient.ToString(nfi) + ",' '," +
                                articolComanda[i].discClient.ToString(nfi) + "," + articolComanda[i].procAprob.ToString(nfi) + "," + articolComanda[i].multiplu.ToString(nfi) + "," +
                                valPoz.ToString(nfi) + ",'" + articolComanda[i].infoArticol + "'," + articolComanda[i].cantUmb + ",'" +
                                articolComanda[i].Umb + "','" + ulStoc + "', '" + fakeArt + "','" + ponderareArt + "','" + articolComanda[i].istoricPret + "', " +
                                valTransport + "," + greutateArticol + " ) ";


                    }
                    else
                    {
                        if (refSAP.Equals("-1"))
                        {
                            pretUnit = articolComanda[i].pretUnit / articolComanda[i].multiplu;
                            valPoz = (articolComanda[i].pretUnit / articolComanda[i].multiplu) * Double.Parse(articolComanda[i].cantUmb, CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            pretUnit = articolComanda[i].pretUnit;
                            valPoz = articolComanda[i].pretUnit * Double.Parse(articolComanda[i].cantUmb, CultureInfo.InvariantCulture);
                        }


                        ulStoc = unitLogAlt;


                        if (articolComanda[i].filialaSite != null && !articolComanda[i].filialaSite.Trim().Equals(String.Empty))
                        {
                            ulStoc = articolComanda[i].filialaSite;
                        }
                        else {
                            if (ulStoc.Equals("BV90") && articolComanda[i].depozit.Contains("MAV"))
                                ulStoc = "BV92";
                            else if (dateLivrare.filialaCLP != null && dateLivrare.filialaCLP.Trim().Length > 0)
                                ulStoc = dateLivrare.filialaCLP;
                        }



                        query = " insert into sapprd.zcomdet_tableta(mandt,id,poz,status,cod,cantitate,valoare,depoz, " +
                                " transfer,valoaresap,ppoz,procent,um,procent_fc,conditie,disclient,procent_aprob,multiplu, " +
                                " val_poz,inf_pret,cant_umb,umb, ul_stoc, fake,istoric_pret, brgew) " +
                                " values ('900'," + idCmd.Value + ",'" + pozArt + "','" + cmdStatus + "','" + codArt + "'," + articolComanda[i].cantitate.ToString(nfi) + ", " +
                                "" + pretUnit.ToString(nfi) + ",'" + articolComanda[i].depozit + "','0',0,'0'," + articolComanda[i].procent.ToString(nfi) + ",'" +
                                articolComanda[i].um + "'," + articolComanda[i].procentFact.ToString(nfi) + ",' '," +
                                articolComanda[i].discClient.ToString(nfi) + "," + articolComanda[i].procAprob.ToString(nfi) + "," + articolComanda[i].multiplu.ToString(nfi) + "," +
                                valPoz.ToString(nfi) + ",'" + articolComanda[i].infoArticol + "'," + articolComanda[i].cantUmb + ",'" +
                                articolComanda[i].Umb + "','" + ulStoc + "', '" + fakeArt + "','" + articolComanda[i].istoricPret + "'," + greutateArticol + " ) ";


                       

                    }


                   

                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.ExecuteNonQuery();

                    if (articolComanda[i].listCabluri != null && !articolComanda[i].listCabluri.Equals(String.Empty))
                        new Cabluri05().insertCabluri05(connection, transaction, idCmd.Value.ToString(), codArt, articolComanda[i].listCabluri);

                }


                transaction.Commit();

                if (depart.Contains("04") || depart.Equals("11"))
                {
                    savePrelucrare04(connection, idCmd.Value.ToString(), dateLivrare.prelucrare);
                }

                idComanda = idCmd.Value.ToString();

                
                OperatiiAdresa.insereazaCoordonateAdresa(connection, idCmd.Value.ToString(), dateLivrare.coordonateGps, dateLivrare.codJudet, dateLivrare.Oras);
                OperatiiSuplimentare.saveTonajComanda(connection, idCmd.Value.ToString(), dateLivrare.tonaj);


                //comenzile angajament nu se creaza in SAP
                if (!cmdAngajament)
                {

                    //actualizare tabela conditii
                    if (conditieID != "-1")
                    {

                        query = " update sapprd.zcondheadtableta set cmdmodif = '" + idCmd.Value + "' where id = '" + conditieID + "' ";

                        cmd.CommandText = query;
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Clear();
                        cmd.ExecuteNonQuery();

                    }
                    //sf. actualizare

                    //scriere comanda in SAP

                    //vanzare din GED cu transp. ARBSQ se calculeaza intai pretul
                    double pretTransp = 0;
                    if (Utils.isUserTest(dateLivrare.codAgent))
                    {
                        paramCmd = "C";
                    }
                    else if ((uLog.Substring(2, 1).Equals("2") && comandaVanzare.canalDistrib.Equals("20") && (transp.Equals("TRAP") || transp.Equals("TERT")) && refSAP.Equals("-1")) || (isComandaWood(uLog) && refSAP.Equals("-1") && transp.Equals("TRAP")))
                    {
                        paramCmd = "S";
                    }
                    else
                    {
                        paramCmd = "C";
                    }

                    string calcPretTransp = " ";

                    if (comandaVanzare.canalDistrib.Equals("10") || Utils.isUserTest(dateLivrare.codAgent))
                        calcPretTransp = "X";

                    webService = new ZTBL_WEBSERVICE();

                    SAPWebServices.ZcreazaComanda inParam = new SAPWebServices.ZcreazaComanda();
                    System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                    webService.Credentials = nc;
                    webService.Timeout = 600000;

                    inParam.Id = idCmd.Value.ToString();
                    inParam.FaraTransp = calcTransport ? " " : "X";
                    inParam.GvEvent = paramCmd;  //C - creaza comanda, S - simulare pret transport
                    inParam.Canal = isComandaWood(uLog) ? "40" : comandaVanzare.canalDistrib;
                    inParam.Vers = calcPretTransp;
                                      
                    SAPWebServices.ZcreazaComandaResponse outParam = webService.ZcreazaComanda(inParam);

                    if ((uLog.Substring(2, 1).Equals("2") && comandaVanzare.canalDistrib.Equals("20") && (transp.Equals("TRAP") || transp.Equals("TERT")) && refSAP.Equals("-1")) || (isComandaWood(uLog) && refSAP.Equals("-1") && transp.Equals("TRAP")))
                    {
                        pretTransp = Convert.ToDouble(outParam.VTrans);
                        retVal = "100#" + pretTransp.ToString() + "#" + idCmd.Value.ToString();
                    }
                    else if (uLog.Substring(2, 1).Equals("2") && transp.Equals("TCLI") && cmdStatus.Equals("21"))    //cmd ged simulata fara rezervare de stoc cu transp. client
                    {
                        retVal = "0";
                    }
                    else
                    {
                        retVal = outParam.VOk.ToString();
                    }

                    if (Utils.isUserTest(dateLivrare.codAgent))
                        retVal = outParam.VOk.ToString();

                    webService.Dispose();


                }
                else
                {
                    retVal = "0";
                }


                //alerta mail pentru adrese de livrare noi
                if (dateLivrare.adrLivrNoua)
                {
                    string paramAdrLivr = "";


                    if (dateLivrare.Strada.Trim() != "")
                        paramAdrLivr = dateLivrare.Oras + ", " + dateLivrare.Strada;
                    else
                        paramAdrLivr = dateLivrare.Oras;

                    sendAlertMailAdrLivrareNoua(dateLivrare.codAgent, comandaVanzare.codClient, paramAdrLivr, dateLivrare.unitLog);


                }

                cmd.Dispose();

                if (uLog.Substring(2, 1).Equals("1") && dateLivrare.adrLivrNoua)
                {
                    OperatiiAdresa.adaugaAdresaClient(connection, idCmd.Value.ToString(), dateLivrare);

                    if (dateLivrare.addrNumber == "" || dateLivrare.addrNumber.Trim() == "")
                        OperatiiSuplimentare.saveTonajAdresa(connection, idCmd.Value.ToString(), dateLivrare.tonaj);

                }

                if (!cnpClient.Equals("-1"))
                {
                    OperatiiSuplimentare.saveDatePersonale(connection, dateLivrare, comandaVanzare);
                }

                if (cnpClient.Length == 13)
                {
                    OperatiiSuplimentare.saveCnpPF(connection, Int32.Parse(idCmd.Value.ToString()), comandaVanzare.cnpClient);
                }

                if (valSD.Equals("X"))
                {
                    OperatiiSuplimentare.aprobaAutomatSD(connection, dateLivrare.codAgent, idCmd.Value.ToString());
                }

                if (dateLivrare.codSuperAgent != null && dateLivrare.codSuperAgent.Trim().Length > 0 && !dateLivrare.codSuperAgent.Equals(dateLivrare.codAgent))
                {
                    OperatiiSuplimentare.saveComandaSuperAv(connection, dateLivrare.codSuperAgent, idCmd.Value.ToString());
                }

                if (!valSD.Equals("X") && !valDV.Equals("X") && tipUserSap.Equals("OIVPD") && dateLivrare.Transport.Equals("TCLI"))
                {
                    new Mail().sendMailComenziOpFacturare(idCmd.Value.ToString());
                }

                OperatiiSuplimentare.saveDateDelegat(connection, Int32.Parse(idCmd.Value.ToString()), dateLivrare);

            }
            catch (Exception ex)
            {
                retVal = "-1";

                var LineNumber = GeneralUtils.getLineNumber(ex);
                sendErrorToMail(ex.ToString() + "\n\n" + tipUser + "\n\n" + JSONArt + "\n\n" + JSONComanda + "\n\n" + JSONDateLivrare + "\n\n line nr = " + LineNumber);
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }


            if (hasDepozMAV1)
            {
                if (retVal.Equals("0") || retVal.Contains("#"))
                {
                    string filialaUser = dateLivrare.unitLog.Substring(0, 2) + "1" + dateLivrare.unitLog.Substring(3, 1);

                    Sms.TipUser[] useri = { Sms.TipUser.SM, Sms.TipUser.CVS };
                    new Mail().sendComandaMail(idComanda, useri, filialaUser);

                }
            }


            return retVal;


        }


        private void savePrelucrare04(OracleConnection conn, String idComanda, String prelucrare)
        {
            OperatiiSuplimentare opSuplimentare = new OperatiiSuplimentare();
            opSuplimentare.savePrelucrare04(conn, idComanda, prelucrare);
        }

        private bool isComandaWood(String unitLog)
        {
            return unitLog.Substring(2, 1).Equals("4") ? true : false;
        }

        public static string getDepartAgent(string codAgent)
        {
            string departAgent = "00";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                cmd.CommandText = " select divizie from agenti where cod =:codAgent ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgent;



                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    departAgent = oReader.GetString(0);
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


            return departAgent;
        }


        public static string getTipUser(string codAgent)
        {
            string tipAgent = "NN";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = GetConnectionString_android_prd();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select substr(tip,0,2) from agenti where cod =:codAgent ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgent.Trim();



                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    tipAgent = oReader.GetString(0);

                    if (tipAgent.Substring(0, 1).ToUpper().Equals("C"))
                        tipAgent = "CV";
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


            return tipAgent;
        }




        [WebMethod]
        public string saveCmdGED(string comanda, string canal)
        {
            string retVal = "-1";

            try
            {

                SAPWebServices.ZTBL_WEBSERVICE webService = null;

                webService = new ZTBL_WEBSERVICE();

                SAPWebServices.ZcreazaComanda inParam = new SAPWebServices.ZcreazaComanda();
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
                webService.Credentials = nc;
                webService.Timeout = 600000;
                inParam.FaraTransp = " ";
                inParam.Id = comanda;
                inParam.GvEvent = "C";  //C - creaza comanda, S - simulare pret transport
                inParam.Canal = canal == null ? "20" : canal;

                SAPWebServices.ZcreazaComandaResponse outParam = webService.ZcreazaComanda(inParam);

                retVal = outParam.VOk.ToString();

            }
            catch (Exception ex)
            {
                retVal = "-1";
                sendErrorToMail(ex.ToString() + " " + comanda);
            }

            return retVal;
        }




        [WebMethod]
        public string getArticoleFurnizor(string codArticol, string tip1, string tip2, string furnizor, string codDepart, string filiala, string aczc)
        {
            OperatiiArticole opArticole = new OperatiiArticole();
            return opArticole.getListArticoleFurnizor(codArticol, tip1, tip2, furnizor, codDepart, filiala, aczc);
        }



        [WebMethod]
        public string cautaArticoleAndroid(string codArticol, string tip, string depart)
        {
            string retVal = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string numeArticolVar = "-";
            string codArticolVar = "-";
            string conditie = "";
            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();
                if (tip == "1")
                {
                    conditie = " substr(a.cod,-8) like '" + codArticol + "%' ";
                }
                if (tip == "2")
                {
                    conditie = " upper(a.nume) like upper('" + codArticol + "%') ";
                }

                cmd.CommandText = " select x.cod, x.nume from (Select decode(length(a.cod),18,substr(a.cod,-8),a.cod) " +
                                  " cod, a.nume from articole a, sintetice b where a.sintetic = b.cod and   " +
                                  " b.depart =:depart and  " + conditie + " ) x where rownum < 10 order by x.nume ";




                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":depart", OracleType.VarChar, 4).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = depart;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        codArticolVar = oReader.GetString(0);
                        numeArticolVar = oReader.GetString(1);
                        retVal += numeArticolVar.Replace('#', ' ') + "#" + codArticolVar + "@@";
                    }


                }
                else
                {
                    retVal = "-1";
                }

                oReader.Close();
                oReader.Dispose();

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }



            return retVal;
        }


        [WebMethod]
        public string cautaSinteticeAndroid(string codSintetic, string tip, string depart)
        {
            string retVal = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string numeSinteticVar = "-";
            string codSinteticVar = "-";
            string conditie = "";
            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();
                if (tip == "1")
                {
                    conditie = " cod like '" + codSintetic + "%' ";
                }
                if (tip == "2")
                {
                    conditie = " upper(nume) like upper('" + codSintetic + "%') ";
                }

                cmd.CommandText = " select cod, nume from sintetice " +
                                  " where  " +
                                  " depart =:depart and  " + conditie + " order by nume ";




                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":depart", OracleType.VarChar, 4).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = depart;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        codSinteticVar = oReader.GetString(0);
                        numeSinteticVar = oReader.GetString(1).Replace(',', ' ');
                        retVal += numeSinteticVar + "#" + codSinteticVar + "#" + codSinteticVar + "@@";
                    }

                }
                else
                {
                    retVal = "-1";
                }

                oReader.Close();
                oReader.Dispose();

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }



            return retVal;
        }


        [WebMethod]
        public string getListClientiCautare(string numeClient, string depart, string departAg, string unitLog, string codUser, string tipUserSap)
        {
            OperatiiClienti clienti = new OperatiiClienti();
            return clienti.getListClienti(numeClient, depart, departAg, unitLog, codUser, tipUserSap);
        }

        [WebMethod]
        public string getListClientiInstPublice(string numeClient, string unitLog, string tipUser, string tipClient)
        {
            return new OperatiiClienti().getListClientiInstPublice(numeClient, unitLog, tipUser, tipClient);
        }

        [WebMethod]
        public string getProcMarjaComenziIP(string codAgent, string codClient)
        {
            return new OperatiiComenzi().getProcMarjaComenziIP(codAgent, codClient);
        }

        [WebMethod]
        public string getClientiAlocati(string codAgent, string filiala)
        {
            return new OperatiiClienti().getClientiAlocati(codAgent, filiala);
        }

        [WebMethod]
        public string getTermenPlataClientDepart(string codAgent, string codClient)
        {
            return new OperatiiClienti().getTermenPlataClientDepart(codAgent, codClient);
        }

        [WebMethod]
        public string getAgentClientInfo(string codClient, string filiala)
        {
            return new OperatiiComenzi().getAgentComandaInfo(codClient, filiala);
        }

        [WebMethod]
        public string getListMeseriasi(string numeClient, string unitLog)
        {
            OperatiiClienti clienti = new OperatiiClienti();
            return clienti.getListMeseriasi(numeClient, unitLog);
        }


        [WebMethod]
        public string getDetaliiClient(string codClient, string depart, string codUser)
        {
            OperatiiClienti clienti = new OperatiiClienti();
            return clienti.getDetaliiClient(codClient, depart, codUser);
        }

        [WebMethod]
        public string getInfoCreditClient(string codClient)
        {
            OperatiiClienti clienti = new OperatiiClienti();
            return clienti.getInfoCreditClient(codClient);
        }

        [WebMethod]
        public string getDateLivrareClient(string codClient)
        {
            return new OperatiiAdresa().getDateLivrareClient(codClient);
        }


        [WebMethod]
        public string cautaClientAndroid(string numeClient, string depart, string departAg, string unitLog, string tipUserSap)
        {



            string serializedResult = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string condClient = "";



            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string condDepart1 = "", condDepart2 = "";

                if (!depart.Equals("00"))
                {
                    condDepart1 = " and t.depart='" + depart + "'";
                    condDepart2 = " and p.spart = '" + depart + "'";

                }

                //exceptie Bucuresti se cauta pe district
                string exceptieClient = " and p.kunn2 ='" + unitLog + "'";

                if (unitLog.Substring(0, 2).Equals("BU"))
                    exceptieClient = " and p.kunn2 like 'BU%' ";

                //pentru DV nu trebuie restrictie pe filiala
                if (unitLog.Equals("NN10"))
                    exceptieClient = "";



                //Doar clientii definiti pe department, filiala
                if (depart == "11")
                {
                    condClient = " and exists (select 1 from clie_tip t where t.canal = '20'  and t.cod_cli=c.cod " +
                                 " and t.depart='11' )  and exists (select 1 from clie_tip t where t.canal = '20' " +
                                 " and t.cod_cli=c.cod and t.depart='" + departAg + "' ) " +
                                 " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod " +
                                 " and p.vtweg = '20' and p.spart = '11' and p.parvw in ('ZA','ZS') and p.kunn2 = '" + unitLog + "')";
                }
                else
                {
                    condClient = " and exists (select 1 from clie_tip t where t.canal = '10' " +
                                 " and t.cod_cli=c.cod  ) " +
                                 " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod " +
                                 " and p.vtweg = '10'  and p.parvw in ('ZA','ZS') " +
                                   exceptieClient + " )";

                }



                if (tipUserSap != null && (tipUserSap.Equals("SDIP") || tipUserSap.Equals("CVIP")))
                    cmd.CommandText = " select distinct c.nume, c.cod, c.tip_pers from websap.clienti c, sapprd.knvv v, sapprd.knvp p, sapprd.kna1 k " +
                                      " where c.cod = v.kunnr and v.mandt = '900' and v.vtweg = '20' " +
                                      " and v.spart = '11' and v.kdgrp in ('18','19') and v.mandt = p.mandt " +
                                      " and v.kunnr = p.kunnr and v.vtweg = p.vtweg and v.spart = p.spart and k.mandt='900' and k.kunnr = c.cod " +
                                      " and p.parvw in ('ZA','ZS') and lower(c.nume) like lower('" + numeClient + "%') order by nume ";

                else
                    cmd.CommandText = " select x.nume, x.cod, x.tip_pers from (select c.nume, c.cod, c.tip_pers from clienti c " +
                                  " where upper(c.nume) like upper('" + numeClient.Replace("'", "") + "%')  " + condClient +
                                  "  ) x " +
                                  " where rownum<=50 order by x.nume ";




                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                List<Client> listaClienti = new List<Client>();
                Client unClient = null;

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {

                        unClient = new Client();
                        unClient.numeClient = oReader.GetString(0);
                        unClient.codClient = oReader.GetString(1);
                        unClient.tipClient = oReader.GetString(2);
                        listaClienti.Add(unClient);

                    }

                }



                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaClienti);

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
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
        public string cautaFurnizorProduseAndroid(string numeClient, string depart, string departAg, string unitLog, string codFurnizor)
        {

            string serializedResult = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select lifn2, a.name1 from sapprd.WYT3 v, sapprd.lfa1 a where v.mandt = '900' and a.mandt = '900' and v.parvw = 'WL' " +
                                  " and v.lifnr =:codFurn and v.mandt = a.mandt and v.lifn2 = a.lifnr order by v.parza ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codFurn", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codFurnizor;

                oReader = cmd.ExecuteReader();

                List<FurnizorProduse> listaFurnizoriProduse = new List<FurnizorProduse>();
                FurnizorProduse unFurnizorProduse = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        unFurnizorProduse = new FurnizorProduse();
                        unFurnizorProduse.numeFurnizorProduse = oReader.GetString(1);
                        unFurnizorProduse.codFurnizorProduse = oReader.GetString(0);
                        listaFurnizoriProduse.Add(unFurnizorProduse);

                    }

                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaFurnizoriProduse);

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
        public string cautaFurnizorAndroid(string numeClient, string depart, string departAg, string unitLog)
        {



            string serializedResult = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = "  select a.lifnr, a.name1 from sapprd.lfa1 a, sapprd.lfb1 b, sapprd.wyt3 v where a.mandt = '900' and upper(a.name1) like upper('" + numeClient.Replace("'", "") + "%') " +
                                  "  and a.mandt = b.mandt and a.lifnr = b.lifnr and b.bukrs = '1000' and a.mandt = v.mandt and a.lifnr = v.lifnr and v.parvw = 'RS' and v.lifnr = v.lifn2 ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                List<Furnizor> listaFurnizori = new List<Furnizor>();
                Furnizor unFurnizor = null;

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {

                        unFurnizor = new Furnizor();
                        unFurnizor.numeFurnizor = oReader.GetString(1);
                        unFurnizor.codFurnizor = oReader.GetString(0);
                        listaFurnizori.Add(unFurnizor);

                    }

                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaFurnizori);

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
        public string userLogon(string userId, string userPass, string ipAdr, string deviceInfo)
        {




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

                OracleParameter comp = new OracleParameter("w", OracleType.NChar, 24);
                comp.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(comp);

                OracleParameter tipAcces = new OracleParameter("k", OracleType.Number, 2);
                tipAcces.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(tipAcces);

                OracleParameter ipAddr = new OracleParameter("ipAddr", OracleType.VarChar, 15);
                ipAddr.Direction = ParameterDirection.Input;
                ipAddr.Value = ipAdr;
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

                string localComp = comp.Value.ToString().Trim();
                if (localComp.Equals("SITE"))
                    localComp = "GLINA";

                //exceptie SDKA
                string strTipAcces = tipAcces.Value.ToString();

 
 

                retVal = resp.Value.ToString() + "#" + depart.Value.ToString().Trim() + "#" + localComp + "#" + userName.Value.ToString().Trim() + "#" + idAg.Value.ToString().Trim() + "#" + strTipAcces;


                OracleDataReader oReader = null;


                //tip user sap
                string tipAgent = "";
                string consWood = "";
                string filialaHome = "false";
                string initDivizie = "";

                if (idAg.Value.ToString().Trim().Length > 0)
                {

                    cmd.CommandText = " select b.tip, b.wood, nvl(h.filiala,'-1') filhome, b.init_divi from agenti b, sapprd.zfilhome h where b.cod =:codAg and substr(b.filiala,0,2) = substr(h.filiala(+),0,2) ";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":codAg", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = string.Format("{0:d8}", Int32.Parse(idAg.Value.ToString()));

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        tipAgent = oReader.GetString(0);

                        if (tipAgent.Equals("SRM"))
                            tipAgent = "CVR";

                        consWood = oReader.GetString(1);
                        filialaHome = oReader.GetString(2).Equals("-1") ? "false" : "true";

                        if (tipAgent.Equals("AV") || tipAgent.Equals("SD"))
                            initDivizie = oReader.GetString(3);
                        else
                            initDivizie = oReader.GetString(3).Substring(0, 2);
                    }

                    if (consWood.Trim().ToLower().Equals("y"))
                        tipAgent = "W";


                }else
                {
                    return "-1";
                }


                string consSite = " ";

                if (isConsVanzSite(idAg.Value.ToString()) || tipAgent.Equals("CVO") || tipAgent.Equals("SVO"))
                    consSite = "X";
                else if (tipAgent.ToUpper().Contains("CONS"))
                    consSite = "Y";
                else
                    consSite = " ";




                string localTipAgent = tipAgent;
                if (tipAgent.ToUpper().StartsWith("C"))
                    localTipAgent = "CV";

                if (localTipAgent.Equals("KA3") || localTipAgent.Equals("KA1") || localTipAgent.Equals("SDKA"))
                    localTipAgent = "KA";

                if (localTipAgent.Equals("SMR"))
                    localTipAgent = "SM";

                //citire procente discount consilieri si sm
                string procenteCons = "-1!-1!-1!-1";
                if (strTipAcces == "17" || strTipAcces == "18" || strTipAcces == "27")
                {
                    procenteCons = getProcenteConsilieri(idAg.Value.ToString(), localTipAgent);
                    if (procenteCons.Equals(""))
                        procenteCons = "1!1";

                    retVal += "#" + procenteCons + "#-1#" + consSite; //campuri disponibile
                }
                else
                {
                    retVal += "#-1#" + getDepartExtra(idAg.Value.ToString(), initDivizie, localComp) + "#" + consSite; //campuri disponibile
                }

                //pentru directori se afla filialele de influenta
                string filiale = "";
                if (tipAcces.Value.ToString() == "12" || tipAcces.Value.ToString() == "14" || tipAcces.Value.ToString() == "35" || tipAcces.Value.ToString() == "10")
                {
                    cmd.CommandText = " select distinct prctr from sapprd.zfil_dv where pernr =:pernr order by prctr  ";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":pernr", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = string.Format("{0:d8}", Int32.Parse(idAg.Value.ToString()));


                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            filiale += oReader.GetString(0).Trim() + ";";
                        }
                    }
                    else
                    {
                        if ((tipAcces.Value.ToString() == "10") && (localComp.Equals("GLINA") || localComp.Equals("MILITARI") || localComp.Equals("ANDRONACHE") || localComp.Equals("OTOPENI")))
                        {
                            filiale = "BU10;BU11;BU12;BU13;";
                        }
                        else
                        {
                            filiale = " ";
                        }
                    }


                }
                else
                    filiale = " ";
                //sf. filiale


                if (oReader != null)
                {
                    oReader.Close();
                    oReader.Dispose();
                }


                if (tipAgent.Equals("KA3") || tipAgent.Equals("KA1") || tipAgent.Equals("SDKA"))
                    tipAgent = "KA";

                if (tipAgent.Equals("SDCVA"))
                    tipAgent = "CAG1";



                //rezolvat problema comenzilor fara articole la aprobare
                filialaHome = "false";

                retVal += "#" + filiale + "#" + tipAgent + "#" + getExtraFiliale(idAg.Value.ToString(), tipAcces.Value.ToString(), localComp) + "#" + filialaHome + "#" + FtpHelper.getLocalFtpIp(localComp) + "#" + initDivizie + "#" + MeniuTableta.stareMeniuTableta(connection, idAg.Value.ToString()) + "#";


                //if (deviceInfo != null && !deviceInfo.Trim().Equals(String.Empty))
                //    OperatiiSuplimentare.saveDeviceInfo(connection, string.Format("{0:d8}", Int32.Parse(idAg.Value.ToString())), deviceInfo);


            }
            catch (Exception ex)
            {
                retVal = "-1";
                sendErrorToMail(ex.ToString() + " userid = " + userId + " , " + deviceInfo);
                retVal = ex.ToString();
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }



            return retVal;
        }




        private string getExtraFiliale(string codAgent, string tipAcces, string filiala)
        {


            string extraFiliale = "";



            if (codAgent.Length == 0)
                return extraFiliale;


            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;
            OracleCommand cmd = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();
                cmd.CommandText = " select prctr from sapprd.zpern_filiale where pernr =:codAg order by prctr ";
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codAg", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = string.Format("{0:d8}", Int32.Parse(codAgent));

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        if (extraFiliale.Length == 0)
                            extraFiliale = oReader.GetString(0);
                        else
                            extraFiliale += "," + oReader.GetString(0);
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
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }


            if ((tipAcces.Equals("9") || tipAcces.Equals("10")) && (filiala.Equals("GLINA") || filiala.Equals("MILITARI") || filiala.Equals("ANDRONACHE") || filiala.Equals("OTOPENI")))
            {
                if (extraFiliale.Length == 0)
                    extraFiliale = "BU10,BU11,BU12,BU13";
                else
                    extraFiliale += ",BU10,BU11,BU12,BU13";
            }

            if (extraFiliale.Length == 0)
                extraFiliale = " ";

            return extraFiliale;


        }




        private string getProcenteConsilieri(string codUser, string tipUser)
        {




            string procenteCons = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;
            OracleCommand cmd = null;

            if (tipUser.Equals("SDCVA"))
                tipUser = "CV";
            else if (tipUser.Equals("SMG"))
                tipUser = "SM";
            else if (tipUser.Equals("SVO"))
                tipUser = "CVO";

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select a.comision_cv, a.vkgrp, a.coef_corr from sapprd.zcoef_cons_dep a, agenti b where b.cod =:codAg " +
                                  " and a.functie =:tipUser and b.filiala = a.pdl order by a.vkgrp";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codAg", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = string.Format("{0:d8}", Int32.Parse(codUser));

                cmd.Parameters.Add(":tipUser", OracleType.VarChar, 35).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = tipUser;

                oReader = cmd.ExecuteReader();
                string procDepart = "", comisionCV = "";
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        comisionCV = oReader.GetDouble(0).ToString();
                        procDepart += oReader.GetString(1) + ":" + oReader.GetDouble(2).ToString() + ";";
                    }

                    procenteCons = comisionCV + "!" + procDepart;
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



            return procenteCons;


        }


        public static string getDepartExtra(String codUser, string divizie, string filiala)
        {
            string depExtra = null;

            for (int i = 0; i < agentiExtra02.Length; i++)
                if (agentiExtra02[i].Equals(codUser))
                    depExtra = "02";

            if (divizie.Equals("01"))
                depExtra = "02";

            if (divizie.Equals("041"))
                if (depExtra == null)
                    depExtra = "040";
                else
                    depExtra += ";040";


            if (divizie.Equals("040"))
                if (depExtra == null)
                    depExtra = "041";
                else
                    depExtra += ";041";


            if (divizie.Equals("07"))
                if (depExtra == null)
                    depExtra = "03;06";
                else
                    depExtra += ";03;06";


            if (divizie.Equals("03"))
                if (depExtra == null)
                    depExtra = "07;06";
                else
                    depExtra += ";07;06";


            if (divizie.Equals("06"))
                if (depExtra == null)
                    depExtra = "03;07";
                else
                    depExtra += ";03;07";


      
            return depExtra;

        }


        [WebMethod]
        public string userLogonDocumentatie(string userId, string userPass, string ipAdr)
        {

            string retVal = "";
            OracleConnection connection = null;
            OracleCommand cmd = new OracleCommand();



            try
            {
                string connectionString = GetConnectionString_android_prd();

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

                OracleParameter depart = new OracleParameter("z", OracleType.VarChar, 5);
                depart.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(depart);

                OracleParameter comp = new OracleParameter("w", OracleType.VarChar, 12);
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

                OracleParameter userName = new OracleParameter("numeUser", OracleType.VarChar, 128);
                userName.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(userName);

                OracleParameter usrId = new OracleParameter("userID", OracleType.Number, 5);
                usrId.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(usrId);

                cmd.ExecuteNonQuery();


                string strIpFtp = "0.0.0.0";

                if (tipAcces.Value.ToString().Equals("12") || tipAcces.Value.ToString().Equals("14"))
                {
                    strIpFtp = getFtpIp(ipAdr);
                }
                else
                {
                    strIpFtp = getLocalFtpIp(comp.Value.ToString().ToUpper());
                }

                retVal = resp.Value.ToString() + "#" + depart.Value.ToString() + "#" + comp.Value.ToString() + "#" + userName.Value.ToString() + "#" +
                         idAg.Value.ToString() + "#" + tipAcces.Value.ToString() + "#" + strIpFtp;



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

            return retVal;
        }


        private string getFtpIp(string ipAddr)
        {
            string strFtpIp = "0.0.0.0";

            string[] tokIpAddr = ipAddr.Split('.');
            string ipAddrMask = tokIpAddr[0] + "." + tokIpAddr[1] + "." + tokIpAddr[2];

            switch (ipAddrMask)
            {
                case "0.0.0":   //Andronache
                    strFtpIp = "0.0.0.0";
                    break;
                case "10.2.32": //Glina
                    strFtpIp = "10.2.32.1";
                    break;
                case "172.16.150":  //Militari
                    strFtpIp = "172.16.150.60";
                    break;
                case "10.2.40":   //Otopeni
                case "10.2.46":   //
                    strFtpIp = "10.2.40.1";
                    break;
                case "10.2.41":   //Otopeni
                    strFtpIp = "10.2.40.1";
                    break;
                case "172.16.190":  //Bacau
                    strFtpIp = "172.16.190.60";
                    break;
                case "10.14.0": //Baia
                    strFtpIp = "10.14.0.1";
                    break;
                case "172.16.160":  //Brasov
                    strFtpIp = "172.16.160.60";
                    break;
                case "172.16.130":  //Cluj
                    strFtpIp = "172.16.130.60";
                    break;
                case "172.16.180":  //Constanta
                    strFtpIp = "172.16.180.60";
                    break;
                case "172.16.170":  //Craiova
                    strFtpIp = "172.16.170.60";
                    break;
                case "FOCSANI":
                    strFtpIp = "0.0.0.0";
                    break;
                case "10.1.9":  //Galati
                    strFtpIp = "10.1.9.65";
                    break;
                case "10.1.6":  //Galati
                    strFtpIp = "10.1.0.6";
                    break;
                case "10.11.8": //Iasi
                    strFtpIp = "10.11.8.1";
                    break;
                case "10.16.0": //Mures
                    strFtpIp = "10.16.0.1";
                    break;
                case "172.16.200":  //Oradea
                    strFtpIp = "172.16.200.60";
                    break;
                case "10.7.8":  //Piatra
                    strFtpIp = "10.7.8.1";
                    break;
                case "10.15.0": //Pitesti
                    strFtpIp = "10.15.0.1";
                    break;
                case "172.16.210":  //Ploiesti
                    strFtpIp = "172.16.210.60";
                    break;
                case "172.16.120":  //Timisoara
                    strFtpIp = "172.16.120.60";
                    break;


            }


            return strFtpIp;
        }


        private string getLocalFtpIp(string filiala)
        {
            string ftpIp = "0.0.0.0";

            switch (filiala)
            {
                case "ANDRONACHE":
                    ftpIp = "0.0.0.0";
                    break;
                case "GLINA":
                    ftpIp = "10.2.32.1";
                    break;
                case "MILITARI":
                    ftpIp = "172.16.150.60";
                    break;
                case "OTOPENI":
                    ftpIp = "10.2.40.1";
                    break;
                case "BACAU":
                    ftpIp = "172.16.190.60";
                    break;
                case "BAIA":
                    ftpIp = "10.14.0.1";
                    break;
                case "BRASOV":
                    ftpIp = "172.16.160.60";
                    break;
                case "CLUJ":
                    ftpIp = "172.16.130.60";
                    break;
                case "CONSTANTA":
                    ftpIp = "172.16.180.60";
                    break;
                case "CRAIOVA":
                    ftpIp = "172.16.170.60";
                    break;
                case "FOCSANI":
                    ftpIp = "0.0.0.0";
                    break;
                case "GALATI":
                    ftpIp = "10.1.9.65";
                    break;
                case "IASI":
                    ftpIp = "10.11.8.1";
                    break;
                case "MURES":
                    ftpIp = "10.16.0.1";
                    break;
                case "ORADEA":
                    ftpIp = "172.16.200.60";
                    break;
                case "PIATRA":
                    ftpIp = "10.7.8.1";
                    break;
                case "PITESTI":
                    ftpIp = "10.15.0.1";
                    break;
                case "PLOIESTI":
                    ftpIp = "172.16.210.60";
                    break;
                case "TIMISOARA":
                    ftpIp = "172.16.120.60";
                    break;

            }


            return ftpIp;
        }


        public static void sendAlertMailCreareClp(string nrCmd)
        {
            //alerta mail pentru ofiterii de credite la creare clp

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {


                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select a.nrcmdsap, a.val_comanda,a.cod_client, b.nume, c.mail from sapprd.zclphead a, clienti b, sapprd.zdest_mail c " +
                                  " where a.cod_client = b.cod and a.nrcmdsap <> ' ' " +
                                  " and c.funct = 'OF' and c.prctr = a.ul and a.id =:idCmd ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idCmd", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Convert.ToInt32(nrCmd);


                oReader = cmd.ExecuteReader();
                string mailDest = "", codClient = "", numeClient = "", valCmd = "", nrCmdSap = "";
                if (oReader.HasRows)
                {

                    MailMessage message = new MailMessage();
                    message.From = new MailAddress("comenzi.tableta@arabesque.ro");

                    oReader.Read();

                    mailDest = oReader.GetString(4);
                    numeClient = oReader.GetString(3);
                    codClient = oReader.GetString(2);
                    valCmd = oReader.GetDouble(1).ToString();
                    nrCmdSap = oReader.GetString(0);



                    if (!mailDest.Trim().Equals(""))
                    {

                        message.To.Add(new MailAddress(mailDest));

                    }



                    message.Subject = "Aprobare CLP - OFC ";

                    message.Body = " S-a creat comanda de transfer " + nrCmdSap + " cu descarcare directa la clientul " + numeClient + " (" + codClient + ")" +
                                   " in valoare de " + valCmd + " RON (valoarea este calculata la preturile setate in sistem pentru acest client, " +
                                   " fara a tine cont de alte discounturi negociate de AV). In analiza limitei de credit pentru acest client va rog sa " +
                                   " tineti cont de aceasta valoarea. De asemenea, confirmati in tranzactia ZCLP valoarea rezervata din limita de credit. ";



                    SmtpClient client = new SmtpClient("mail.arabesque.ro");
                    client.Send(message);

                    message.Dispose();

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


        }


        private bool isDataSambata(DateTime dateInput)
        {
            bool isLivrareSambata = false;

            isLivrareSambata = dateInput.DayOfWeek == DayOfWeek.Saturday;

            return isLivrareSambata;
        }

        [WebMethod]
        public string getCodBare(string codArticol)
        {
            OperatiiArticole opArticole = new OperatiiArticole();
            return opArticole.getCodBare(codArticol);
        }


        [WebMethod]
        public string sendMailOfertaGed(string nrComanda, string mailAddress)
        {
            ExpediereMail mail = new ExpediereMail();
            return mail.sendOfertaGedMail(nrComanda, mailAddress);
        }


        private void sendAlertMailAdrLivrareNoua(string codAgent, string codClient, string adrLivrare, string unitLog)
        {
            //alerta mail pentru adrese de livrare introduse de agent

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {


                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct a.mail, b.nume, c.nume, b.divizie, tc.tip from sapprd.zdest_mail a, agenti b, clienti c, clie_tip cl, tip_client tc " +
                                  " where b.cod =:codag and c.cod =:codcl and cl.canal='10' and cl.depart = substr(b.divizie,0,2) and cl.cod_cli = c.cod and tc.cod = cl.tip " +
                                  " and (a.vkgrp=b.divizie or a.vkgrp = '00') and b.filiala = a.prctr and " +
                                  " funct in ('SD','DZ','OF') ";



                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codag", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgent;

                cmd.Parameters.Add(":codcl", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codClient;

                oReader = cmd.ExecuteReader();
                string mailDest = "", numeAgent = "", numeClient = "", depart = "", tipClient = "";
                if (oReader.HasRows)
                {

                    MailMessage message = new MailMessage();
                    message.From = new MailAddress("comenzi.tableta@arabesque.ro");

                    while (oReader.Read())
                    {
                        mailDest = oReader.GetString(0);
                        numeAgent = oReader.GetString(1);
                        numeClient = oReader.GetString(2);
                        depart = oReader.GetString(3);
                        tipClient = oReader.GetString(4);



                        if (!mailDest.Trim().Equals(""))
                        {
                            message.To.Add(new MailAddress(mailDest));

                        }

                    }//sf. while"

                    message.Subject = "Adresa livrare noua comanda " + unitLog;
                    message.Body = "Agentul " + numeAgent + " de la departamentul " + depart +
                                    " a introdus pentru clientul " + numeClient + " (" + tipClient + ") adresa de livrare " + adrLivrare + ".";
                    SmtpClient client = new SmtpClient("mail.arabesque.ro");
                    client.Send(message);

                    message.Dispose();

                }

                oReader.Close();
                oReader.Dispose();


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


        }

        [WebMethod]
        public string sendOfertaMail(string idCmd)
        {

           

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;



            try
            {
                using (StreamReader reader = File.OpenText(Server.MapPath("~/TemplateOferta.html")))
                {
                    string tableBody = "";

                    /*
                    tableBody = "<tbody><tr><td>data</td><td>data</td><td>data</td><td>data</td><td>data</td><td>data</td><td>data</td>" +
                    "</tr><tr class='alt'><td>data</td><td>data</td><td>data</td><td>data</td><td>data</td><td>data</td><td>data</td>" +
                    "</tr><tr><td>data</td><td>data</td><td>data</td><td>data</td><td>data</td><td>data</td><td>data</td>" +
                    "</tr><tr class='alt'><td>data</td><td>data</td><td>data</td><td>data</td><td>data</td><td>data</td><td>data</td></tr>" +
                    "<tr><td>data</td><td>data</td><td>data</td><td>data</td><td>data</td><td>data</td><td>data</td></tr>" +
                    "</tbody> ";
                    */




                    //antet oferta
                    string connectionString = GetConnectionString_android();
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    cmd = connection.CreateCommand();
                    cmd.CommandText = " select b.nume, c.nume, a.pers_contact,a.telefon, b.nrtel, a.ul from sapprd.zcomhead_tableta a, agenti b, clienti c where " +
                                      " id =:idCmd and b.cod = a.cod_agent and c.cod = a.cod_client ";


                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idCmd", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = idCmd;
                    oReader = cmd.ExecuteReader();

                    string numeClient = "", persContact = "", telContact = "", numeAgent = "", telAgent = "", unitLog = "";
                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        numeAgent = oReader.GetString(0);
                        numeClient = oReader.GetString(1);
                        persContact = oReader.GetString(2);
                        telContact = oReader.GetString(3);
                        telAgent = oReader.GetString(4);
                        unitLog = oReader.GetString(5);
                    }


                    //date filiala
                    string adresaUnitLog = "", telUnitLog = "", faxUnitLog = "", bancaUnitLog = "", contUnitLog = "";
                    string[] tokAdrUnitLog = getAdrUnitLog(unitLog).Split('#');
                    adresaUnitLog = tokAdrUnitLog[0];
                    telUnitLog = tokAdrUnitLog[1];
                    faxUnitLog = tokAdrUnitLog[2];
                    bancaUnitLog = tokAdrUnitLog[3];
                    contUnitLog = tokAdrUnitLog[4];



                    //articole oferta
                    cmd.CommandText = "select decode(length(a.cod),18,substr(a.cod,-8),a.cod), b.nume, a.cantitate, a.valoare, a.um from sapprd.zcomdet_tableta a, articole b where " +
                                      " a.id =:idCmd and a.cod = b.cod ";

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idCmd", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = idCmd;
                    oReader = cmd.ExecuteReader();

                    tableBody = "<tbody> ";

                    int nrArt = 1;
                    string oLinie = "", altColor = "";
                    decimal tva = 0.19M;
                    decimal totalVal = 0, totalTVA = 0, totalGen = 0, valArt = 0, valTVA = 0;


                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            altColor = "class='alt'";
                            if ((nrArt + 1) % 2 == 0)
                                altColor = "";

                            valArt = oReader.GetDecimal(3) * oReader.GetDecimal(2);
                            valTVA = oReader.GetDecimal(3) * oReader.GetDecimal(2) * tva;

                            oLinie = "<tr " + altColor + " ><td align='center'>" + nrArt.ToString() + ". </td><td align='left'>" + oReader.GetString(1) + " (" + oReader.GetString(0) + ")" +
                                     "</td><td align='center'>" + oReader.GetString(4) + " </td><td align='center'>" + oReader.GetDecimal(2) + "</td> " +
                                     "<td align='right'>" + String.Format("{0:0.00}", oReader.GetDecimal(3)) + " </td><td align = 'right'>" +
                                     String.Format("{0:0.00}", valArt) + "</td> " +
                                     "<td align='right'>" + String.Format("{0:0.00}", valTVA) + "</td><tr>";

                            tableBody += oLinie;

                            totalVal += decimal.Parse(String.Format("{0:0.00}", valArt));
                            totalTVA += decimal.Parse(String.Format("{0:0.00}", valTVA));

                            nrArt++;
                        }
                    }

                    totalGen = totalVal + totalTVA;

                    tableBody += "</tbody>";

                    oReader.Close();
                    oReader.Dispose();

                    string htmlFile = "<html><body><style type='text/css'>" +
                    ".datagrid table    {       border-collapse: collapse;       text-align: left; width: 100%;    } " +
                    ".datagrid    {     font: normal 12px/150% Courier New, Courier, monospace; " +
                    " background: #fff;overflow: hidden;border: 1px solid #006699;-webkit-border-radius: 3px;" +
                    "-moz-border-radius: 3px; border-radius: 3px; } " +
                    ".datagrid table td, .datagrid table th { padding: 2px 2px; } " +
                    ".datagrid table thead th { background-color:#006699;color:#EECFA1; font-size: 14px;border-left: 1px solid #0070A8;}" +
                    ".datagrid table thead th:first-child{border: none;}" +
                    ".datagrid table tbody td {color:#008B45;border-left: 1px solid #E1EEF4;font-size: 13px;font-weight: normal;}" +
                    ".datagrid table tbody .alt td {background: #E1EEF4;color: #008B45;}" +
                    ".datagrid table tbody td:first-child{border-left: none;}" +
                    ".datagrid table tbody tr:last-child td{border-bottom: none;}" +
                    ".datagrid table tfoot td { background-color:#006699;color:#EECFA1; font-weight:bold; font-size: 14px;border-left: 1px solid #0070A8;padding: 2px;} " +
                    ".customText1{font-family: Verdana;font-weight: normal;font-size: 9px;color: #68838B;}" +
                    ".customText2{font-family: Candara;font-weight: normal;font-size: 13px;color: #4F94CD;}" +
                    ".customText3{font-family: Serif;font-weight: bold;font-size: 32px;color: #A66D33;}" +
                    ".customText4{font-family: 'Bookman Old Style', serif;font-weight: normal;font-size: 12px;color: #A66D33;}" +
                    ".customText5{font-family: 'Bookman Old Style', serif;font-weight: bold;font-size: 12px;color: #473C8B;}" +
                    ".customText6{font-family: Verdana;font-weight: normal;font-size: 11px;color: #008B45;} </style>" +
                    " <table align='center' width='90%'>" +
                    "<tr><td>" +
                    "</td></tr>" +
                    "<tr><td><table class='customText2'><tr><td>Sediu</td><td>" + adresaUnitLog + "</td>" +
                    "</tr><tr><td>Telefon</td><td>" + telUnitLog + "</td></tr>" +
                    "<tr><td>Fax</td><td>" + faxUnitLog + "</td></tr><tr><td>Contul</td><td>" + contUnitLog + "</td></tr><tr><td>" +
                    "Banca</td><td>" + bancaUnitLog + "</td></tr><tr><td>Cota TVA</td><td>24%</td></tr>" +
                    " </table>" +
                    " </td> " +
                    " <td align = 'left' valign='top'><table class='customText' ><tr><td align='center' class='customText3' colspan='2'>" +
                    "  Oferta</td></tr><tr><td class='customText4'>Nr.</td><td class='customText4'>" + idCmd + "</td></tr><tr>" +
                    " <td class='customText4'>Din data</td><td class='customText4'>22.02.2013</td></tr><tr><td class='customText4'>" +
                    "  Valabila pana la</td><td class='customText4'>25.02.2013</td></tr></table></td>" +
                    "<td valign='top'><table class='customText2'  align='right'><tr><td>Client</td><td>" + numeClient + "</td>" +
                    "</tr><tr><td>Pers. de contact</td><td>" + persContact + "</td></tr><tr><td>Telefon</td><td>" + telContact + "</td></tr>" +
                    " </table></td></tr><tr><td colspan='3'><br><div class='datagrid'>" +
                    "<table ><thead><tr><th width='3%' align='center'>Nr.<br>crt</th><th width='30%' align='center'>Denumirea produselor sau a serviciilor</th>" +
                    "<th width='5%' align='center'>U.M.</th><th width='10%' align='center'>Cant.</th><th width='10%' align='center'>Pretul unitar <br> (fara T.V.A.)</th>" +
                    "<th width='10%' align='center'>Valoarea</th><th width='10%' align='center'>Valoarea T.V.A.</th>" +
                    "</tr><tr><th width='5%' align='center'>0</th><th width='30%' align='center'>1</th><th width='5%' align='center'>2</th><th width='10%' align='center'>3</th>" +
                    "<th width='10%' align='center'>4</th><th width='10%' align='center'>5(3x4)</th><th width='10%' align='center'>6</th>" +
                    "</tr></thead><tfoot><tr><td colspan='5' align='right'>Total:</td><td align='right'>" +
                    "" + String.Format("{0:0.00}", totalVal) + "</td>" +
                    "<td align='right'>" + String.Format("{0:0.00}", totalTVA) + "</td>" +
                    "</tr><tr><td colspan='6' align='right'>" +
                    "Total de plata:</td><td align='right'>" + String.Format("{0:0.00}", totalGen) + "</td></tr></tfoot>" +
                    tableBody +
                    "</table></div></td></tr><tr><td colspan='3'><table><tr><td class='customText5'><br>Reprezentant vanzari:" +
                    "</td></tr><tr><td class='customText6'>" + numeAgent + ", tel: " + telAgent + "</td></tr></table>" +
                    "</td></tr>" +
                    "<tr><td colspan='3'><table ><tr><td class='customText5'><br>Observatii:</td>" +
                    "</tr><tr><td class='customText6'>" +
                    "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et" +
                    "dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex " +
                    "ea commodo consequat. " +
                    "<br></td></tr></table></td></tr></table>";

                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlFile, null, "text/html");

                    MailMessage message = new MailMessage();
                    message.From = new MailAddress("Oferta.materiale@arabesque.ro");
                    message.To.Add(new MailAddress("florin.brasoveanu@arabesque.ro"));

                    message.Subject = "Oferta materiale si servicii SC Arabesque SRL";

                    message.AlternateViews.Add(htmlView);
                    SmtpClient client = new SmtpClient("mail.arabesque.ro");
                    client.Send(message);

                }

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }


            return "0";


        }


        public string getAdrUnitLog(string unitLog)
        {

            string retVal = "";
            switch (unitLog)
            {
                case "BC10":
                    retVal = "Str. Constantin Musat, nr. 1, cod 600092, Bacau#0334-401008#0334-401013#ING BANK#RO89INGB0008000005128917";
                    break;
                case "MM10":
                    retVal = "Str. Independentei, nr. 80, cod 430071, Baia Mare#0262-250155#0362-404931#ING BANK#RO60INGB0021000030428911";
                    break;
                case "BV10":
                    retVal = "Str. Bucegi, nr. 1, cod 500053, Brasov#0268-470171#0368-401147#UNICREDIT TIRIAC BANK#RO51BACX0000004568925021";
                    break;
                case "CJ10":
                    retVal = "Str. Calea Floresti, nr. 147-153, cod 400397, Cluj#0364-402345#0364-402347#ING BANK#RO59INGB0003000005128931";
                    break;
                case "CT10":
                    retVal = "B-dul Aurel Vlaicu, nr. 171, cod 900330, Constanta#0241-695303#0241-545850#ING BANK#RO88INGB0004000005128941";
                    break;
                case "DJ10":
                    retVal = "B-dul Decebal, nr. 111A, cod 200746, Craiova#0251-438597#0251-435196#ING BANK#RO22INGB0012000023928911";
                    break;
                case "VN10":
                    retVal = "B-dul Bucuresti, nr. 12, cod 620144, Focsani#0237-216110#0237-216111#UNICREDIT TIRIAC BANK#RO77BACX0000004568925038";
                    break;
                case "GL10":
                    retVal = "Str. Drumul de Centura, nr. 39, cod 800248, Galati#0236-416122#0336-401231#ING BANK#RO73INGB0010000005128911";
                    break;
                case "IS10":
                    retVal = "Comuna Miroslava, Sat Uricani, Trup Izolat, nr. 1, cod 707316, Iasi#0750-210102#0750-210222#ING BANK#RO59INGB0014000023928921";
                    break;
                case "NT10":
                    retVal = "Comuna Savinesti, Str. Uzinei, nr. 1, cod 610070, Piatra Neamt#0333-401004#0233-237326#ING BANK#RO12INGB0018000027028911";
                    break;
                case "BH10":
                    retVal = "Str. Calea Santandrei, nr. 3A, cod 410238, Oradea#0359-407630#0359-407631#ING BANK#RO02INGB0007000005128981";
                    break;
                case "AG10":
                    retVal = "Comuna Bradu, DN 65B, cod 117140, Pitesti#0348-457231#0348-445748#UNICREDIT TIRIAC BANK#RO07BACX0000004568925037";
                    break;
                case "PH10":
                    retVal = "Str. Poligonului, nr. 5, cod 100070, Ploiesti#0244-567312#0244-567312#ING BANK#RO47INGB0005009051289101";
                    break;
                case "MS10":
                    retVal = "Str. Depozitelor, nr. 26, cod 540240, Targu Mures#0750-211601#0365-430524#ING BANK#RO82INGB0011000032268911";
                    break;
                case "TM10":
                    retVal = "Str. Calea Sagului, nr. 205, cod 300517, Timisoara#0256-291023#0256-274745#UNICREDIT TIRIAC BANK#RO35BACX0000004568925018";
                    break;
                case "BU13":
                    retVal = "Soseaua Andronache, nr. 203, Sector 2, cod 022524, Bucuresti#021-2405591#021-2405591#BRD#RO61BRDE445SV44133934450";
                    break;
                case "BU10":
                    retVal = "Str. Drumul intre Tarlale, nr. 61A, sector 3, cod 032982, Bucuresti#031-8056632#031-8056633#UNICREDIT TIRIAC BANK#RO78BACX0000004568925020";
                    break;
                case "BU12":
                    retVal = "Str. Aleea Teisani, nr. 3-21, Sector 1, cod 014034, Bucuresti#031-4250255#031-4250253#UNICREDIT TIRIAC BANK#RO08BACX0000004568925019";
                    break;
                case "BU11":
                    retVal = "Str. Drumul Osiei, nr. 8-16, Sector 6, cod 062395, Bucuresti#021-3172196#021-3172154#UNICREDIT TIRIAC BANK#RO34BACX0000004568925036";
                    break;
            }


            return retVal;

        }




        [WebMethod]
        public string getStocAndroid(string codArt, string filiala, string showCmp, string depart)
        {
            Stocuri stocuri = new Stocuri();
            return stocuri.getStocAndroid(codArt, filiala, showCmp, depart);
        }

        //obsolete
        public static bool isArtPermited(string sintetic)
        {
            //pe departamentul 02 se poate vinde, din BV90, doar din aceste sintetice
            bool isPermited = false;


            string[] sinteticePermise = { "204", "205", "226", "229", "236", "237", "238", "240", "227", "209", "203", "241", "209", "211", "231", "213", "214", "233", "208" };

            for (int i = 0; i < sinteticePermise.Length; i++)
            {
                if (sinteticePermise[i].Equals(sintetic))
                {
                    isPermited = true;
                    break;
                }
            }

            return isPermited;
        }

        [WebMethod]
        public string getCantUmAlt(string codArt, string umAlt, string cantArt)
        {
            string retVal = "0";


            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            try
            {
                string connectionString = GetConnectionString_android();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select umrez, umren from sapprd.marm where mandt = '900' and matnr = :codArt and meinh =:umAlt ";
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArt;

                cmd.Parameters.Add(":umAlt", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = umAlt;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    double factConv = 0;
                    double umrez = oReader.GetDouble(0);
                    double umren = oReader.GetDouble(1);

                    if (umren != 0)
                        factConv = umrez / umren;

                    double newVal = Double.Parse(cantArt) * factConv;


                    retVal = newVal.ToString();
                }
                else
                {
                    retVal = "0";
                }


            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                oReader.Close();
                oReader.Dispose();
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }


            return retVal;




        }


        //materiale transport
       


        //consilieri vanzari site
        public static bool isConsVanzSite(string codAgent)
        {
            bool isCons = false;

            string[] consilieriVanzSite = { "00059566", "00059586", "00059585", "00059653", "00059660", "00059690", "00059694", "00059721", "00059732", "00059731", "00059783","00059680","00060067", "00060055", "00060185" };

            for (int i = 0; i < consilieriVanzSite.Length; i++)
            {
                if (consilieriVanzSite[i].Contains(codAgent))
                {
                    isCons = true;
                    break;
                }
            }


            return isCons;
        }


        [WebMethod]
        public string getStocWeight(string codArt, string filiala, string depozit, string depart)
        {

            string retVal = "";

            OperatiiArticole opArticole = new OperatiiArticole();
            string stocDepozit = getStocDepozit(codArt, filiala, depozit, depart,"");

            string umStocArticol = stocDepozit.Split('#')[1];

            string greutateArticol = "";
            if (umStocArticol.Length > 0)
                greutateArticol = opArticole.getGreutateArticol(codArt, umStocArticol, OperatiiArticole.EnumUnitMas.KG);

            string lastCharStoc = stocDepozit.Substring(stocDepozit.Length - 1, 1);

            if (lastCharStoc.Equals("#"))
                retVal = stocDepozit + greutateArticol;
            else
                retVal = stocDepozit + "#" + greutateArticol;


            return retVal;

        }


        [WebMethod]
        public bool testBV(string codArticol, string filiala)
        {
            return OperatiiArticole.isArtPermBV90(codArticol, filiala);
        }

        [WebMethod]
        public string getStocArticole(string listArticole)
        {
            OperatiiArticole opArticole = new OperatiiArticole();
            return opArticole.getStocArticole(listArticole);
        }

        [WebMethod]
        public string getStocCustodie(string codArticol, string codClient, string filiala)
        {
            return new OperatiiArticole().getStocCustodie(codArticol, codClient, filiala);
        }

        private string getUnitLogGed(string untiLog)
        {
            return untiLog.Substring(0, 2) + "2" + untiLog.Substring(3, 1);
        }

        [WebMethod]
        public string getStocDepozit(string codArt, string filiala, string depozit, string depart, string isArtMathaus)
        {
            return new OperatiiArticole().getStocDepozit(codArt, filiala, depozit, depart, isArtMathaus);
        }

        [WebMethod]
        public string getStocDepozit_old(string codArt, string filiala, string depozit, string depart)
        {


            //stoc articol
            string retVal = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string umArt = "";
            string cant = "0", sinteticArt = "";
            string showStocVal = "1";
            string localDepozit = depozit;

            if (depart != null && (depart.Equals("040") || depart.Equals("041")))
                depart = "04";

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                if ((depozit.Equals("01V1")) && OperatiiArticole.isArticolExceptie02(connection, codArt))
                    localDepozit = "02V1";
                else if ((depozit.Equals("01V2")) && OperatiiArticole.isArticolExceptie02(connection, codArt))
                    localDepozit = "02V2";

                cmd = connection.CreateCommand();


                cmd.CommandText = " select nvl(sum(labst),0) stoc, meins, ar.sintetic from " +
                                 " (select m.labst , mn.meins, mn.matnr  from sapprd.mard m, sapprd.mara mn " +
                                 " where m.mandt = '900' and m.mandt = mn.mandt " +
                                 " and m.matnr = mn.matnr and m.matnr =:art  and m.werks =:fil and m.lgort=:dep  " +
                                 " union all " +
                                 " select -1 * nvl(sum(e.omeng),0), e.meins, e.matnr  from sapprd.vbbe e " +
                                 " where e.mandt = '900' and e.matnr =:art and e.werks =:fil and e.lgort=:dep and e.sobkz <> 'E' " +
                                 " group by e.meins, e.matnr " +
                                 " union all " +
                                 " select - 1 * sum(b.bdmng) stoc, b.meins, b.matnr from sapprd.resb b " +
                                 " where b.mandt = '900' and b.bwart = '921' and b.xloek <> 'X' and bdter >= to_char(sysdate - 365, 'yyyymmdd') " +
                                 " and b.matnr = :art and b.werks = :fil and b.lgort = :dep " +
                                 " group by b.meins, b.matnr ), articole ar where ar.cod = matnr group by meins, ar.sintetic ";


                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":art", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArt;

                cmd.Parameters.Add(":fil", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = depozit.Equals("MAV2") || depozit.Equals("DSCM") ? getUnitLogGed(filiala) : filiala;

                cmd.Parameters.Add(":dep", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = localDepozit;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    cant = oReader.GetDouble(0).ToString() != "" ? oReader.GetDouble(0).ToString() : "-1";
                    umArt = oReader.GetString(1);
                    sinteticArt = oReader.GetString(2);

                    retVal = cant + "#" + umArt + "#" + showStocVal + "#";
                }
                else
                {
                    retVal = "0# #" + showStocVal + "#";
                }


                oReader.Close();


                cmd.CommandText = " select nvl(sum(w.menge - w.wamng),0) from sapprd.eket w, sapprd.ekpo o, sapprd.ekko q where w.menge <> w.wamng " +
                                  " and w.mandt = '900' and w.mandt = o.mandt and w.ebeln = o.ebeln and w.ebelp = o.ebelp and o.loekz <> 'L' and o.elikz <> 'X' " +
                                  " and o.matnr =:art and o.mandt = q.mandt and o.ebeln = q.ebeln and q.loekz <> 'L' and q.reswk =:fil and o.reslo=:dep " +
                                  " and not exists (select * from sapprd.ekbe e where e.mandt = '900' and e.ebeln = q.ebeln and e.ebelp = o.ebelp and bewtp = 'L') " +
                                  " and q.aedat >= to_char(sysdate-30,'yyyymmyy')";


                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":art", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArt;

                cmd.Parameters.Add(":fil", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = depozit.Equals("MAV2") || depozit.Equals("DSCM") ? getUnitLogGed(filiala) : filiala;

                cmd.Parameters.Add(":dep", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = depozit;

                oReader = cmd.ExecuteReader();
                string stocBV90 = "";
                double stocFinal = 0;
                if (oReader.HasRows)
                {
                    oReader.Read();
                    stocBV90 = oReader.GetDouble(0).ToString() != "" ? oReader.GetDouble(0).ToString() : "-1";

                }

                if (!cant.Equals("-1") && !stocBV90.Equals("-1"))
                    stocFinal = Double.Parse(cant, CultureInfo.InvariantCulture) - Double.Parse(stocBV90, CultureInfo.InvariantCulture);

                cant = stocFinal.ToString();

                //exceptii vanzare articole
                if (filiala.Equals("BV90") || filiala.Equals("BV92"))
                {

                    //tratare exceptii sintetice feronerie
                    if (depart != null && (depart.Equals("02") || depart.Equals("05")))
                    {

                        if (OperatiiArticole.isArtPermBV90(codArt, filiala))
                        {
                            showStocVal = "1";
                        }
                        else  //nu este permisa vanzarea altor articole, se afiseaza fara stoc
                        {
                            cant = "0";
                            umArt = "";
                            showStocVal = "1";
                        }
                    }
                }

                retVal = cant + "#" + umArt + "#" + showStocVal + "#";


                //exceptie material transport
                if (Utils.isMatTransport(codArt))
                {
                    retVal = "1#BUC#1";
                }

                if (ArticoleUtils.isMaterialServiciiWood(codArt))
                {
                    retVal = "9999#" + ArticoleUtils.getUmServicii(connection, codArt) + "#1";
                }

                //sf. exceptie


                oReader.Close();
                oReader.Dispose();

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString() + "\n\n" + codArt + "\n\n" + filiala + "\n\n" + depozit + "\n\n" + depart);
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }


            return retVal;

        }




        [WebMethod]
        public void sendMail(string msgBody)
        {
            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress("Lite.SFA@arabesque.ro");
                message.To.Add(new MailAddress("florin.brasoveanu@arabesque.ro"));
                message.Subject = "LiteSFA error";
                message.Body = msgBody;
                SmtpClient client = new SmtpClient("mail.arabesque.ro");
                client.Send(message);

            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
            }

        }





        [WebMethod]//Android
        public string sendMailAlert(string ul, string depart, string dest, string agent, string clnt, string suma)
        {

            string retVal = "-1";
            string mailDest = "";

            if (dest.Equals("1")) //sef de departament
            {
                mailDest = getDepName(depart) + "." + ul.Substring(0, 2).ToLower() + "@arabesque.ro";
            }

            if (dest.Equals("2")) //director
            {


                OracleConnection connection = new OracleConnection();
                OracleCommand cmd = new OracleCommand();
                OracleDataReader oReader = null;

                try
                {
                    string connectionString = GetConnectionString_android();

                    connection.ConnectionString = connectionString;
                    connection.Open();

                    cmd = connection.CreateCommand();

                    cmd.CommandText = " select distinct mail, mail_alt from sapprd.zfil_dv where mandt='900' and prctr =:fil " +
                                      " and spart =:depart ";


                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":fil", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = ul;

                    cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = depart;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {

                        while (oReader.Read())
                        {
                            mailDest = oReader.GetString(0);

                            if (!mailDest.Trim().Equals("") && !mailDest.ToUpper().Equals("CRISTIAN.MATEI@ARABESQUE.RO") && !mailDest.ToUpper().Equals("IONUT.VELESCU@ARAEBSQUE.RO"))
                            {
                                MailMessage message = new MailMessage();
                                message.From = new MailAddress("aprobare.comenzi@arabesque.ro");


                                message.To.Add(new MailAddress(mailDest));

                                //mail alternativ
                                if (!oReader.GetString(1).Trim().Equals(""))
                                    message.To.Add(new MailAddress(oReader.GetString(1)));

                                message.Subject = "Confirmare comanda " + agent;
                                message.Body = "Confirmati comanda pentru " + clnt + " in valoare de " + suma + " RON.";
                                SmtpClient client = new SmtpClient("mail.arabesque.ro");
                                client.Send(message);
                                message.Dispose();
                            }
                        }
                        retVal = "0";


                    }

                    oReader.Close();
                    oReader.Dispose();



                }
                catch (Exception ex)
                {
                    sendErrorToMail(ex.ToString() + " , mail dest: " + mailDest );
                    retVal = "-1";
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                    connection.Dispose();

                }





            }
            if (dest.Equals("3")) //ofiter credite
            {
                mailDest = getDepName(depart) + "." + ul.Substring(0, 2).ToLower() + "@arabesque.ro";
            }





            return retVal;
        }


        private string getDepName(string depCod)
        {
            string retVal = "nedef";

            switch (depCod)
            {
                case "01":
                    retVal = "lemnoase";
                    break;

                case "02":
                    retVal = "feronerie";
                    break;

                case "03":
                    retVal = "parchet";
                    break;

                case "04":
                    retVal = "prafoase";
                    break;

                case "05":
                    retVal = "electrice";
                    break;

                case "06":
                    retVal = "gips-carton";
                    break;

                case "07":
                    retVal = "chimice";
                    break;

                case "08":
                    retVal = "instalatii";
                    break;

                case "09":
                    retVal = "hidroizolatii";
                    break;

            }

            return retVal;
        }


        private string getDepCod(string depName)
        {
            string retVal = "00";

            switch (depName)
            {
                case "LEMN":
                    retVal = "01";
                    break;

                case "FERO":
                    retVal = "02";
                    break;

                case "PARC":
                    retVal = "03";
                    break;

                case "PRAF":
                    retVal = "04";
                    break;

                case "ELEC":
                    retVal = "05";
                    break;

                case "GIPS":
                    retVal = "06";
                    break;

                case "CHIM":
                    retVal = "07";
                    break;

                case "INST":
                    retVal = "08";
                    break;

                case "HIDR":
                    retVal = "09";
                    break;

                case "MATE":
                    retVal = "04";
                    break;

            }

            return retVal;
        }







        [WebMethod]
        public string GetApplicationVersionInfo(string ApplicationName)
        {
            try
            {
                String _data = "";

                XmlDocument _xml = new XmlDocument();
                _xml.Load
                     (AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\SmartScanAppVersion.xml");


                XmlNodeList _xmlNodeList = _xml.GetElementsByTagName("Applications");
                XmlElement _xmlAppRootNode = (XmlElement)_xmlNodeList.Item(0);
                XmlNodeList _xmlAppNodeList = _xmlAppRootNode.GetElementsByTagName(ApplicationName);

                if (_xmlAppNodeList.Count == 0)
                    return "";
                XmlElement _xmlApplicationNode = (XmlElement)_xmlAppNodeList.Item(0);


                String _cabFileName = _xmlApplicationNode.GetAttribute("UpdatesCabFile");
                String _cabFilePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') +
                  "\\UpdateFiles\\" + _cabFileName;


                FileInfo _file = new FileInfo(_cabFilePath);
                String _packageSizeInBytes = Convert.ToString(_file.Length);
                //separator virgula
                _data +=
                _xmlApplicationNode.GetAttribute("LatestVersion") +
                "," + _packageSizeInBytes + "," +
                _xmlApplicationNode.GetAttribute("VersionDate") + ","
                + _cabFileName;

                return _data;
            }
            catch (Exception ex)
            {
                sendErrorToMail(ex.ToString());
                return "";
            }

        }//sf. func


        static private string GetConnectionString_Dev()
        {
            return "Data Source=DEV;Persist Security Info=True;" +
                   "User ID=WEBSAP;Password=2INTER7;Integrated Security=false";
        }

        static private string GetConnectionString_Tes()
        {
            return "Data Source=TES.WORLD;Persist Security Info=True;" +
                   "User ID=WEBSAP;Password=2INTER7;Integrated Security=false";
        }

        static private string GetConnectionString_Qas()
        {
            return "Data Source=QAS.WORLD;Persist Security Info=True;" +
                   "User ID=WEBSAP;Password=2INTER7;Integrated Security=false";
        }


        static private string GetConnectionString()
        {


            return DatabaseConnections.ConnectToProdEnvironment();


        }


        static private string GetConnectionString_local()
        {
            return DatabaseConnections.ConnectToProdEnvironment();

        }

        static private string GetConnectionString_android_tes()
        {

            return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                    " (HOST = 10.1.3.73)(PORT = 1527)))(CONNECT_DATA = (SERVICE_NAME = TES.WORLD))); " +
                    " User Id = WEBSAP; Password = 2INTER7;";

        }

        static private string GetConnectionString_android()
        {


            //PRD
            return DatabaseConnections.ConnectToProdEnvironment();


        }




        static private string GetConnectionString_android_prd()
        {
            return DatabaseConnections.ConnectToProdEnvironment();

        }


        static private string GetConnectionString_local_test()
        {

            return "Data Source=TES;Persist Security Info=True;" +
                   "User ID=WEBSAP;Password=2INTER7;Integrated Security=false";

        }

        public static string getUser()
        {
            return "USER_RFC";
        }

        public static string getPass()
        {
            return "2rfc7tes3";
        }

        private void sendErrorToMail(string errMsg)
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

    }


}
