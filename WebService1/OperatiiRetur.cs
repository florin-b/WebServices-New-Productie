using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
using System.Data;
using System.Globalization;
using System.Web.Script.Serialization;
using WebService1.SAPWebServices;
using WebService1.General;
using System.Text;

namespace WebService1
{
    public class OperatiiRetur
    {



        public string getListDocumenteSalvate(string codAgent, string filiala, string tipUser, string depart, string interval, string stare)
        {
            return getListDocumenteSalvateToDb(codAgent, filiala, tipUser, depart, interval, stare);

        }



        private string getListDocumenteSalvateToDb(string codAgent, string filiala, string tipUser, string depart, string interval, string stare)
        {


            string serializedListComenzi = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            string query = "";

            string condData = "";

            if (interval == "0") //astazi
            {
                string dateInterval = DateTime.Today.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and a.datacreare = '" + dateInterval + "' ";
            }

            if (interval == "1") //ultimele 7 zile
            {
                string dateInterval = DateTime.Today.AddDays(-7).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and a.datacreare >= '" + dateInterval + "' ";
            }

            if (interval == "2") //ultimele 30 zile
            {
                string dateInterval = DateTime.Today.AddDays(-30).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and a.datacreare >= '" + dateInterval + "' ";
            }


            string stareComanda = "";

            if (tipUser.Equals("SD"))
                stareComanda = " and a.statusAprob = " + stare;

            try
            {

                if (tipUser.Equals("SD"))
                {

                    string condDepart = " and b.divizie like '" + depart + "%' ";

                    if (Utils.isFilialaMicaDep04(filiala, depart))
                        condDepart = " and substr(b.divizie,0,2) = '" + depart.Substring(0, 2) + "' ";

                    query = " select a.id, a.nrdocument, a.numeclient, to_char(to_date(a.datacreare,'yyyymmdd')),  a.statusaprob , b.nume from sapprd.zreturhead a, agenti b where " +
                            " a.codagent = b.cod and b.filiala =:filiala " + condDepart + condData + stareComanda + " order by a.id ";

                  

                }
                else 
                {
                    query = " select a.id, a.nrdocument, a.numeclient, to_char(to_date(a.datacreare,'yyyymmdd')),  a.statusaprob, ' '  from sapprd.zreturhead a, agenti b where " +
                            " a.codagent = b.cod and a.codagent =:codagent and b.filiala =:filiala " + condData + " order by a.id ";
                }


                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = query;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                if (!tipUser.Equals("SD"))
                {
                    cmd.Parameters.Add(":codagent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codAgent;
                }



                oReader = cmd.ExecuteReader();

                ComandaReturAfis retur = null;
                List<ComandaReturAfis> listComenzi = new List<ComandaReturAfis>();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        retur = new ComandaReturAfis();
                        retur.id = oReader.GetInt64(0).ToString();
                        retur.nrDocument = oReader.GetString(1);
                        retur.numeClient = oReader.GetString(2);
                        retur.dataCreare = oReader.GetString(3);
                        retur.status = oReader.GetString(4);

                        if (tipUser.Contains("AV") && retur.status.Equals("2"))
                            retur.status = getStareAlocareBorderou(retur.nrDocument);


                        retur.numeAgent = oReader.GetString(5);
                        listComenzi.Add(retur);

                    }

                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedListComenzi = serializer.Serialize(listComenzi);

            }
            catch (Exception ex)
            {
                string err = codAgent + " , " + filiala + " , " + tipUser + " , " + depart + " , " + interval + " , " + stare;
        
                    ErrorHandling.sendErrorToMail(ex.ToString() + " detalii = " +  err);
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }


            return serializedListComenzi;
        }




        private string getStareAlocareBorderou(string idComanda)
        {



            string stareBorderou = "-1";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " select distinct decode(a1.vbeln,'','30','31') borderou from sapprd.vbak k, sapprd.vbfa a, sapprd.vbfa a1, sapprd.likp l, sapprd.vbpa m, sapprd.vbpa m1, sapprd.adrc c, " +
                                  " sapprd.vbpa m2, sapprd.adrc c1 where k.mandt = '900' and k.audat >= to_char(sysdate-60,'yyyymmyy') and k.auart in ('ZRI', 'ZRS') and k.mandt = a.mandt(+) " +
                                  " and k.vbeln = a.vbelv(+) and a.vbtyp_v(+) = 'H' and a.vbtyp_n(+) = 'T' and a.mandt = a1.mandt(+) and a.vbeln = a1.vbelv(+) " +
                                  " and a1.vbtyp_v(+) = 'T' and a1.vbtyp_n(+) = '8' and a.mandt = l.mandt and a.vbeln = l.vbeln and k.mandt = m.mandt and k.vbeln = m.vbeln " +
                                  " and m.parvw in ('VE', 'ZC') and k.mandt = m1.mandt and k.vbeln = m1.vbeln and m1.parvw = 'WE' and m1.mandt = c.client and m1.adrnr = c.addrnumber " +
                                  " and k.mandt = m2.mandt and k.vbeln = m2.vbeln and m2.parvw = 'AP' and m2.mandt = c1.client  and m2.adrnr = c1.addrnumber and k.vgbel =:idComanda ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    stareBorderou = oReader.GetString(0);
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

            return stareBorderou;



        }


        public string opereazaComanda(string idComanda, string tipOperatie)
        {
            string retVal = "";

            //aprobare
            if (tipOperatie.Equals("2"))
            {
                string dateComanda = getArticoleDocumentSalvat(idComanda);
                string stareCmdSap = saveComandaReturToWS(dateComanda, idComanda);

                if (stareCmdSap.Equals("0"))
                    retVal = schimbaStareComanda(idComanda, tipOperatie);
                else
                    retVal = stareCmdSap;

            }

            //respingere
            if (tipOperatie.Equals("3"))
            {
                retVal = schimbaStareComanda(idComanda, tipOperatie);
            }


            return retVal;
        }


        private string schimbaStareComanda(string idComanda, string tipOperatie)
        {

            string retVal = "";

            OracleConnection connection = new OracleConnection();
            string connectionString = DatabaseConnections.ConnectToProdEnvironment();


            try
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                string query = " update sapprd.zreturhead set statusaprob =:stare where id =:idcmd ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idcmd", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int64.Parse(idComanda);

                cmd.Parameters.Add(":stare", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = tipOperatie;

                cmd.ExecuteNonQuery();
                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            retVal = "0";

            return retVal;
        }

        public string getListDocumenteSAP(string codAgent, string filiala, string tipUser, string depart, string interval, string stare)
        {


            string serializedListComenzi = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            string query = "";

            string condData = "";
            string dateInterval = "";

            if (interval == "0") //astazi
            {
                dateInterval = DateTime.Today.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and k.audat = '" + dateInterval + "' ";
            }

            if (interval == "1") //ultimele 7 zile
            {
                dateInterval = DateTime.Today.AddDays(-7).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and k.audat >= '" + dateInterval + "' ";
            }

            if (interval == "2") //ultimele 30 zile
            {
                dateInterval = DateTime.Today.AddDays(-30).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and k.audat >= '" + dateInterval + "' ";
            }


            try
            {


                string condAgent = "";
                if (!tipUser.Equals("SD"))
                    condAgent = " and ag.cod =:codAgent ";

                query = " select distinct ag.nume,  k.vgbel, c.name1 nume_client,  l.traty,  l.wadat data_livrare,   decode(a1.vbeln,'','30','31') ,  m.pernr " +
                        " from sapprd.vbak k, sapprd.vbfa a, sapprd.vbfa a1, sapprd.likp l, sapprd.vbpa m, sapprd.vbpa m1,  sapprd.adrc c,  sapprd.vbpa m2, sapprd.adrc c1, agenti ag " +
                        " where k.mandt = '900'  " + condData + " and k.auart in ('ZRI', 'ZRS')  and k.mandt = a.mandt(+)  and k.vbeln = a.vbelv(+)  and a.vbtyp_v(+) = 'H' " +
                        " and substr(ag.divizie,0,2)=:divizie and a.vbtyp_n(+) = 'T' and a.mandt = a1.mandt(+) and a.vbeln = a1.vbelv(+) and a1.vbtyp_v(+) = 'T' and a1.vbtyp_n(+) = '8' " + condAgent +
                        " and a.mandt = l.mandt and a.vbeln = l.vbeln and k.mandt = m.mandt and k.vbeln = m.vbeln and m.parvw in ('VE', 'ZC') and m.pernr = ag.cod and ag.filiala =:filiala " +
                        " and k.mandt = m1.mandt and k.vbeln = m1.vbeln and m1.parvw = 'WE' and m1.mandt = c.client and m1.adrnr = c.addrnumber and k.mandt = m2.mandt and k.vbeln = m2.vbeln " +
                        " and m2.parvw = 'AP' and m2.mandt = c1.client and m2.adrnr = c1.addrnumber ";

                connection.ConnectionString = connectionString;
                connection.Open();
                cmd = connection.CreateCommand();
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                cmd.Parameters.Add(":divizie", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = depart;

                if (!tipUser.Equals("SD"))
                {
                    cmd.Parameters.Add(":codagent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = codAgent;
                }

                oReader = cmd.ExecuteReader();

                ComandaReturAfis retur = null;
                List<ComandaReturAfis> listComenzi = new List<ComandaReturAfis>();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        retur = new ComandaReturAfis();
                        retur.id = oReader.GetString(1);
                        retur.nrDocument = oReader.GetString(1);
                        retur.numeClient = oReader.GetString(2);
                        retur.dataCreare = " ";
                        retur.status = oReader.GetString(5);
                        retur.numeAgent = oReader.GetString(0);
                        listComenzi.Add(retur);

                    }

                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedListComenzi = serializer.Serialize(listComenzi);

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


            return serializedListComenzi;


        }

        public string getArticoleDocumentSAP(string idComanda)
        {

            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " select decode(length(p.matnr),18,substr(p.matnr,-8),p.matnr) , p.arktx, p.klmeng, p.vrkme, a1.vbeln,  l.traty, a1.vbeln borderou, c.name1, k.vgbel, " +
                                  " to_char(to_date(l.wadat,'yyyymmdd')) data_livrare,  c1.name1 pers_cont,  c1.tel_number tel_pc,  c.region, c.city1, c.street " +
                                  " from sapprd.vbak k, sapprd.vbap p, sapprd.vbfa a, sapprd.vbfa a1, sapprd.likp l, sapprd.vbpa m, sapprd.vbpa m1, sapprd.adrc c, sapprd.vbpa m2, " +
                                  " sapprd.adrc c1 where k.mandt = '900' and k.auart in ('ZRI', 'ZRS') and p.mandt = a.mandt(+) and p.vbeln = a.vbelv(+) " +
                                  " and p.posnr = a.posnv(+) and k.audat >= to_char(sysdate-60,'yyyymmyy') and a.vbtyp_v(+) = 'H' and a.vbtyp_n(+) = 'T' and k.mandt = p.mandt and k.vbeln = p.vbeln " +
                                  " and a.mandt = a1.mandt(+) and a.vbeln = a1.vbelv(+) and a1.vbtyp_v(+) = 'T' and a1.vbtyp_n(+) = '8' and a.mandt = l.mandt and a.vbeln = l.vbeln " +
                                  " and k.mandt = m.mandt and k.vbeln = m.vbeln and m.parvw in ('VE', 'ZC')  and k.mandt = m1.mandt and k.vbeln = m1.vbeln " +
                                  " and m1.parvw = 'WE' and m1.mandt = c.client and m1.adrnr = c.addrnumber and k.mandt = m2.mandt and k.vbeln = m2.vbeln and m2.parvw = 'AP' " +
                                  " and m2.mandt = c1.client and m2.adrnr = c1.addrnumber and k.vgbel =:idComanda ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();

                ComandaRetur retur = null;

                ArticolRetur artRetur = null;
                List<ArticolRetur> listArticole = new List<ArticolRetur>();

                retur = new ComandaRetur();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        retur.dataLivrare = oReader.GetString(9);
                        retur.tipTransport = oReader.GetString(5);
                        retur.numePersContact = oReader.GetString(10);
                        retur.telPersContact = oReader.GetString(11);
                        retur.adresaCodJudet = oReader.GetString(12);
                        retur.adresaOras = oReader.GetString(13);
                        retur.adresaStrada = oReader.GetString(14);
                        retur.nrDocument = oReader.GetString(8);
                        retur.codAgent = "";
                        retur.tipAgent = "";
                        retur.motivRetur = "";

                        artRetur = new ArticolRetur();
                        artRetur.cod = oReader.GetString(0);
                        artRetur.nume = oReader.GetString(1);
                        artRetur.cantitate = oReader.GetDouble(2).ToString();
                        artRetur.cantitateRetur = oReader.GetDouble(2).ToString();
                        artRetur.um = oReader.GetString(3);
                        listArticole.Add(artRetur);

                    }

                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                String listaArticoleSer = serializer.Serialize(listArticole);

                retur.listaArticole = listaArticoleSer;
                serializedResult = serializer.Serialize(retur);

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



        public string getArticoleDocumentSalvat(string idComanda)
        {

            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();
            string nrDocument = "";

            try
            {

                cmd.CommandText = " select datalivrare, tiptransport, numeperscontact, telperscontact, codjudet, localitate, strada,  " +
                                  " nrdocument, codagent, tipagent, motivretur, inlocuire, nvl(trim(com_retur),'0') " +
                                  " from sapprd.zreturhead where id =:idComanda ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();

                ComandaRetur retur = null;
                if (oReader.HasRows)
                {
                    oReader.Read();
                    {
                        retur = new ComandaRetur();
                        retur.dataLivrare = oReader.GetString(0);
                        retur.tipTransport = oReader.GetString(1);
                        retur.numePersContact = oReader.GetString(2);
                        retur.telPersContact = oReader.GetString(3);
                        retur.adresaCodJudet = oReader.GetString(4);
                        retur.adresaOras = oReader.GetString(5);
                        retur.adresaStrada = oReader.GetString(6);
                        retur.nrDocument = oReader.GetString(7);
                        retur.codAgent = oReader.GetString(8);
                        retur.tipAgent = oReader.GetString(9);
                        retur.motivRetur = oReader.GetString(10);
                        retur.inlocuire = oReader.GetString(11);
                        nrDocument = oReader.GetString(12);

                    }

                }

                cmd.CommandText = " select decode(length(a.codarticol),18,substr(a.codarticol,-8),a.codarticol), b.nume, a.cantitate, a.um " +
                                  " from sapprd.zreturdet a, articole b where a.id =:idComanda and a.codarticol = b.cod order by b.nume ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();

                ArticolRetur artRetur = null;
                List<ArticolRetur> listArticole = new List<ArticolRetur>();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        artRetur = new ArticolRetur();
                        artRetur.cod = oReader.GetString(0);
                        artRetur.nume = oReader.GetString(1);
                        artRetur.cantitate = oReader.GetDouble(2).ToString();
                        artRetur.cantitateRetur = oReader.GetDouble(2).ToString();
                        artRetur.um = oReader.GetString(3);
                        listArticole.Add(artRetur);
                    }
                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                String listaArticoleSer = serializer.Serialize(listArticole);
                string listStariSer = serializer.Serialize(new OperatiiRetur().getStareRetur(nrDocument));

                retur.listaArticole = listaArticoleSer;
                retur.listStari = listStariSer;
                serializedResult = serializer.Serialize(retur);

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


        public List<StareDocumentRetur> getStareRetur(string nrDocument)
        {
            List<StareDocumentRetur> listStari = new List<StareDocumentRetur>();

            SAPWebServices.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE();

            SAPWebServices.ZstareCurentaRetur inParam = new ZstareCurentaRetur();
            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Auth.getUser(), Auth.getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;

            inParam.IpVbeln = nrDocument;

            SAPWebServices.ZstareCurentaReturResponse resp = webService.ZstareCurentaRetur(inParam);

            foreach (ZstStareCurenta stare in resp.EtStatus)
            {
                StareDocumentRetur stareDoc = new StareDocumentRetur();
                stareDoc.nrDocument = stare.Vbeln;
                stareDoc.stare = stare.StatusLong;
                listStari.Add(stareDoc);
            }

            return listStari;
        }


        public string getDocumenteRetur(string codClient, string codDepartament, string unitLog, string tipDocument, string interval, string tipUserSap)
        {
            string serializedResult = "";



            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            string condPaleti = "";
            string condData = "";
            string nrZileIstoric = "30";
            string condDepart = "";

            if (tipDocument == null || (tipDocument != null && tipDocument.Equals("PAL")))
            {
                if (codDepartament.Equals("11"))
                    condPaleti = " and p.matkl in ('433', '433_1', '716', '626', '929_2', '515', '302') ";
                else
                    condPaleti = " and m.categ_mat in ('PA','AM') ";

                nrZileIstoric = "270";
            }

            if (interval != null && interval.Length > 1)
                condData = " and substr(k.fkdat,0,6) = '" + interval + "' ";

            if (!codDepartament.Equals("00") && !codDepartament.Equals("11"))
                condDepart = " and p.spart = substr(:depart,0,2) ";




            try
            {

                if (codDepartament.Equals("11"))
                {

                 

                    string tipDocuRetur = " ('ZFM','ZFMC','ZFS','ZFSC','ZFPA','ZFVS') ";

                    if (tipUserSap != null && tipUserSap.Equals("CVO"))
                        tipDocuRetur = " ('ZFHC','ZF2H','ZFVS','ZFCS','ZFVS') ";

                    cmd.CommandText = " select distinct k.vbeln, to_date(k.fkdat,'yyyymmdd'),  " +
                                      " nvl((select traty from sapprd.ekko e, sapprd.vbfa f where e.mandt = '900' and e.mandt = f.mandt and " +
                                      " e.ebeln = f.vbeln and f.vbelv = p.aubel and f.vbtyp_v = 'C' and f.vbtyp_n = 'V' and rownum = 1),k.traty) traty, " +
                                      " nvl((select t.ac_zc from sapprd.zcomhead_Tableta t where t.mandt = '900' and t.nrcmdsap = p.aubel ),' ') ac_zc " +
                                      " from sapprd.vbrk k, sapprd.vbrp p, sapprd.vbpa a, sapprd.adrc c where k.mandt = p.mandt " +
                                      " and k.vbeln = p.vbeln  and k.mandt = '900'  and k.fkart in " + tipDocuRetur +
                                      " and k.fksto <> 'X'  and k.fkdat >= to_char(sysdate - " + nrZileIstoric + ",'yyyymmdd') " +
                                      condPaleti +
                                      " and k.mandt = a.mandt  and k.vbeln = a.vbeln  and a.parvw = 'WE' " +
                                      " and p.prctr =:unitLog  and a.mandt = c.client and a.adrnr = c.addrnumber " +
                                      condData +
                                      " and c.name1 =  :numeClient  order by to_date(k.fkdat,'yyyymmdd') ";


                }
                else
                {
                    
                    cmd.CommandText = " select distinct k.vbeln, to_date(k.fkdat,'yyyymmdd'), " +
                                      " nvl((select traty from sapprd.ekko e, sapprd.vbfa f where e.mandt = '900' and e.mandt = f.mandt and " +
                                      " e.ebeln = f.vbeln and f.vbelv = p.aubel and f.vbtyp_v = 'C' and f.vbtyp_n = 'V' and rownum = 1),k.traty) traty, " +
                                      " nvl((select t.ac_zc from sapprd.zcomhead_Tableta t where t.mandt = '900' and t.nrcmdsap = p.aubel ),' ') ac_zc " +
                                      " from sapprd.vbrk k, " +
                                      " sapprd.vbrp p, sapprd.mara m where k.mandt = p.mandt and k.vbeln = p.vbeln " + condDepart +
                                      " and k.mandt = '900' and k.fkdat >= to_char(sysdate - " + nrZileIstoric + ", 'yyyymmdd') " +
                                      " and k.fkart = 'ZFI' and k.fksto <> 'X' and k.kunag = :codClient " + condData +
                                      " and p.mandt = m.mandt and p.matnr = m.matnr " + condPaleti +
                                      " order by to_date(k.fkdat, 'yyyymmdd') ";

                }

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                if (codDepartament.Equals("11"))
                {
                    cmd.Parameters.Add(":unitLog", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = unitLog;

                    cmd.Parameters.Add(":numeClient", OracleType.VarChar, 120).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codClient;
                }
                else
                {
                    cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    if (!codDepartament.Equals("00"))
                    {
                        cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                        cmd.Parameters[1].Value = codDepartament;
                    }
                }

                oReader = cmd.ExecuteReader();

                DocumentRetur docRetur = null;
                List<DocumentRetur> listDocumente = new List<DocumentRetur>();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        docRetur = new DocumentRetur();
                        docRetur.numar = oReader.GetString(0);
                        docRetur.data = oReader.GetDateTime(1).ToString("dd-MMM-yyyy");
                        docRetur.tipTransport = oReader.GetString(2);

                        if (tipDocument != null && !tipDocument.Equals("PAL"))
                            docRetur.dataLivrare = getDataLivrare(connection, docRetur.numar, docRetur.tipTransport);
                        else
                            docRetur.dataLivrare = "20000101";

                        docRetur.extraDate = getExtraDateRetur(connection, docRetur.numar);
                        docRetur.isCmdACZC = oReader.GetString(oReader.GetOrdinal("ac_zc")).Equals("X");

                        listDocumente.Add(docRetur);
                    }
                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();

                DateReturClient dateRetur = new DateReturClient();
                dateRetur.listaDocumente = serializer.Serialize(listDocumente);
                dateRetur.adreseLivrare = getAdreseLivrareClient(codClient);
                dateRetur.persoaneContact = getPersoaneContact(codClient);

                serializedResult = serializer.Serialize(dateRetur);

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , date: " + codClient + " , " + codDepartament + " , " + unitLog + " , " + interval);
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }


            
            return serializedResult;
        }


        private string getExtraDateRetur(OracleConnection connection, string nrDocument)
        {
            ExtraDate extraDate = new ExtraDate();

            OracleDataReader oReader = null;
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = connection;

            try
            {
                cmd.CommandText = " select  c.city1, c.transpzone, nvl(c.street,''), c.addrnumber from sapprd.vbpa a " +
                                  " inner join sapprd.adrc c on a.mandt = c.client and a.adrnr = c.addrnumber " +
                                  " where a.mandt = '900' and a.vbeln = :nrDoc and parvw = 'WE' ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add(":nrDoc", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    extraDate.localitate = oReader.GetString(0);
                    extraDate.codJudet = oReader.GetString(1);
                    extraDate.strada = oReader.GetString(2);
                    extraDate.codAdresa = oReader.GetString(3);
                }

                cmd.CommandText = " select c.name1, c.tel_number from sapprd.vbpa a inner join sapprd.adrc c on a.mandt = c.client and a.adrnr = c.addrnumber " +
                                  " where a.mandt = '900' and a.vbeln = :nrDoc and parvw = 'AP' ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrDoc", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    extraDate.numeContact = oReader.GetString(0);
                    extraDate.telContact = oReader.GetString(1);
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

            return new JavaScriptSerializer().Serialize(extraDate);
        }

        private string getDataLivrare(OracleConnection connection, string nrDocument, string tipTransport)
        {
            string dataLivrare = "19000101";
            OracleDataReader oReader = null;
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = connection;

            try
            {

               
                if (tipTransport.Equals("TRAP") || tipTransport.Equals("TCLI"))
                {
                    cmd.CommandText = " select k.daten from sapprd.vbfa f, sapprd.vttp p, sapprd.vttk k " +
                                      " where f.mandt = '900' and f.vbeln = :nrDoc and f.vbtyp_v = 'J' and f.vbtyp_n = 'M' " +
                                      " and f.mandt = p.mandt and f.vbelv = p.vbeln and p.mandt = k.mandt and p.tknum = k.tknum and rownum = 1 ";


                }
                else if (tipTransport.Equals("TERT") || tipTransport.Equals("TFRN"))
                {
                    cmd.CommandText = " select to_char(to_date(s.eventdate,'yyyy-mm-dd HH24:mi:ss'),'yyyymmdd') " +
                                      " from sapprd.vbfa f, sapprd.ZPOSTIS_EVENTS s where f.mandt = '900' " +
                                      " and f.vbeln = :nrDoc and f.vbtyp_v = 'C' and f.vbtyp_n = 'M' " +
                                      " and f.vbelv = s.vbeln and s.defaultclientid = 5 and rownum = 1 ";
                }


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrDoc", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    dataLivrare = oReader.GetString(0);
                }
                else
                {

                    cmd.CommandText = " select nvl((select k.daten from sapprd.vbfa f, sapprd.vttp p, sapprd.vttk k " +
                                      " where f.mandt = '900' and f.vbeln = :nrDoc and f.vbtyp_v = 'J' and f.vbtyp_n = 'M' " +
                                      " and f.mandt = p.mandt and f.vbelv = p.vbeln and p.mandt = k.mandt and p.tknum = k.tknum and rownum = 1), " +
                                      " (select v.fkdat from sapprd.vbrk v where v.mandt = '900' and v.vbeln = :nrDoc and v.traty in ('TCLI','TFRN') )) daten from dual ";

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":nrDoc", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = nrDocument;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        dataLivrare = oReader.GetString(0);
                    }

                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + nrDocument + " , " + tipTransport);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return dataLivrare;
        }

        public string getArticoleRetur(string nrDocument, string tipDocument)
        {



            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            string condPaleti = "";
            string tabelaPaleti = "";
            string valPalet1 = "";
            string valPalet2 = "";

            if (tipDocument == null || (tipDocument != null && tipDocument.Equals("PAL")))
            {
                tabelaPaleti = " , sapprd.mara m ";
                condPaleti = " and p.mandt = m.mandt and p.matnr = m.matnr and m.categ_mat in ('PA','AM') ";
                valPalet1 = " , sum(netwr) val_paleti";
                valPalet2 = " , p.netwr ";
            }

            try
            {


                cmd.CommandText = " select decode(length(matnr),18,substr(matnr,-8),matnr) codart, arktx,  sum(fkimg - returnate) cant, vrkme " + valPalet1 + "  from " +
                                 " (select p.matnr, p.fkimg, p.arktx,p.vrkme, p.posnr, p.vbeln " + valPalet2 + " , ( select " +
                                 " nvl(sum(cp.KWMENG),0) from sapprd.vbap cp, sapprd.vbfa a, sapprd.vbak vk " +
                                 " where a.mandt = '900' and a.vbelv = p.vbeln and a.posnv = p.posnr and a.vbtyp_v = 'M' " +
                                 " and a.vbtyp_n = 'H' and a.mandt = vk.mandt and a.vbeln = vk.vbeln and vk.auart in ('ZRI', 'ZRIA', 'ZRSA', 'ZRSA', 'ZRSS') " +
                                 " and a.mandt = cp.mandt and a.vbeln = cp.vbeln and a.posnn = cp.posnr and cp.abgru = ' ') returnate " +
                                 " from sapprd.vbrp p " + tabelaPaleti + " where p.mandt = '900' and p.vbeln =:nrDoc " + condPaleti + "  ) where fkimg - returnate > 0 " +
                                 " group by matnr, arktx, vrkme order by codart ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrDoc", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                oReader = cmd.ExecuteReader();

                ArticolRetur artRetur = null;
                List<ArticolRetur> listArticole = new List<ArticolRetur>();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        artRetur = new ArticolRetur();
                        artRetur.cod = oReader.GetString(0);
                        artRetur.nume = oReader.GetString(1);
                        artRetur.cantitate = oReader.GetDouble(2).ToString();
                        artRetur.um = oReader.GetString(3);

                        if (tipDocument == null || (tipDocument != null && tipDocument.Equals("PAL")))
                        {
                            artRetur.pretUnitPalet = (oReader.GetDouble(4) / oReader.GetDouble(2)).ToString();
                        }
                        else
                            artRetur.pretUnitPalet = "0";

                        listArticole.Add(artRetur);
                    }
                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listArticole);

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


        public string saveComandaReturToDB(string dateRetur)
        {
            string retVal = "0";

            OracleConnection connection = new OracleConnection();
            string connectionString = DatabaseConnections.ConnectToProdEnvironment();
            var serializer = new JavaScriptSerializer();

            OracleTransaction transaction = null;

            
            long idCmd = Convert.ToInt64(GeneralUtils.getCurrentMillis().ToString().Substring(0, 11));
            try
            {
                ComandaRetur comanda = serializer.Deserialize<ComandaRetur>(dateRetur);

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                string query = " insert into sapprd.zreturhead(mandt, id, nrdocument, statuscmd, statusaprob, datacreare, datalivrare, tiptransport, codagent, tipagent, motivretur, " +
                               " numeperscontact, telperscontact, codadresa, codjudet, localitate, strada, codclient, numeclient, acelasi_transp, inlocuire) " +
                               " values ('900', :id, :nrdocument, :statuscmd, :statusaprob, :datacreare, :datalivrare, :tiptransport, :codagent, :tipagent, :motivretur, " +
                               " :numeperscontact, :telperscontact, :codadresa, :codjudet, :localitate, :strada, :codclient, :numeclient, :acelasi_transp, :inlocuire) returning id into :id ";

                transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":id", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idCmd;

                cmd.Parameters.Add(":nrdocument", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = comanda.nrDocument;

                cmd.Parameters.Add(":statuscmd", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = "0";

                cmd.Parameters.Add(":statusaprob", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = "1";

                cmd.Parameters.Add(":datacreare", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = Utils.getCurrentDate();

                cmd.Parameters.Add(":datalivrare", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = comanda.dataLivrare;

                cmd.Parameters.Add(":tiptransport", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = comanda.tipTransport;

                cmd.Parameters.Add(":codagent", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[7].Value = comanda.codAgent;

                cmd.Parameters.Add(":tipagent", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[8].Value = comanda.tipAgent;

                cmd.Parameters.Add(":motivretur", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[9].Value = comanda.motivRetur;

                cmd.Parameters.Add(":numeperscontact", OracleType.NVarChar, 90).Direction = ParameterDirection.Input;
                cmd.Parameters[10].Value = comanda.numePersContact == null ? " " : comanda.numePersContact + " ";

                cmd.Parameters.Add(":telperscontact", OracleType.NVarChar, 75).Direction = ParameterDirection.Input;
                cmd.Parameters[11].Value = comanda.telPersContact == null ? " " : comanda.telPersContact + " ";

                cmd.Parameters.Add(":codadresa", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[12].Value = comanda.adresaCodAdresa;

                cmd.Parameters.Add(":codjudet", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[13].Value = comanda.adresaCodJudet;

                cmd.Parameters.Add(":localitate", OracleType.NVarChar, 75).Direction = ParameterDirection.Input;
                cmd.Parameters[14].Value = comanda.adresaOras;

                cmd.Parameters.Add(":strada", OracleType.NVarChar, 75).Direction = ParameterDirection.Input;
                cmd.Parameters[15].Value = comanda.adresaStrada + " ";

                cmd.Parameters.Add(":codclient", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[16].Value = comanda.codClient;

                cmd.Parameters.Add(":numeclient", OracleType.NVarChar, 75).Direction = ParameterDirection.Input;
                cmd.Parameters[17].Value = comanda.numeClient;

                string obsTransp = " ";
                if (comanda.transpBack != null && Boolean.Parse(comanda.transpBack))
                    obsTransp = "X";

                cmd.Parameters.Add(":acelasi_transp", OracleType.NVarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[18].Value = obsTransp;

                cmd.Parameters.Add(":inlocuire", OracleType.NVarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[19].Value = comanda.inlocuire != null && comanda.inlocuire.Equals("X") ? "X" : " ";


                cmd.ExecuteNonQuery();

                List<ArticolRetur> listaArticole = serializer.Deserialize<List<ArticolRetur>>(comanda.listaArticole);

                string fullCodeArticol = "";
                for (int i = 0; i < listaArticole.Count; i++)
                {

                    query = " insert into sapprd.zreturdet(mandt, id, codarticol, cantitate, um, motiv) values " +
                             " ('900', :idcmd, :codarticol, :cantitate, :um, :motiv )";

                    fullCodeArticol = listaArticole[i].cod;
                    if (fullCodeArticol.Length == 8)
                        fullCodeArticol = "0000000000" + fullCodeArticol;


                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":idcmd", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = idCmd;

                    cmd.Parameters.Add(":codarticol", OracleType.NVarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = fullCodeArticol;

                    cmd.Parameters.Add(":cantitate", OracleType.Number, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = Double.Parse(listaArticole[i].cantitateRetur, CultureInfo.InvariantCulture);

                    cmd.Parameters.Add(":um", OracleType.NVarChar, 9).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = listaArticole[i].um;

                    cmd.Parameters.Add(":motiv", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = listaArticole[i].motivRespingere != null ? listaArticole[i].motivRespingere : " ";

                    cmd.ExecuteNonQuery();

                    addReturImg(connection, transaction, idCmd.ToString(), listaArticole[i]);
                }

                transaction.Commit();

                if (!comanda.tipAgent.Equals("AV"))
                {
                    string dateComanda = getArticoleDocumentSalvat(idCmd.ToString());
                    string stareCmdSap = saveComandaReturToWS(dateComanda, idCmd.ToString());

                    if (stareCmdSap.Equals("0"))
                        retVal = schimbaStareComanda(idCmd.ToString(), "2");
                    else
                        retVal = stareCmdSap;
                }

            }
            catch (Exception ex)
            {

                if (transaction != null)
                    transaction.Rollback();

                ErrorHandling.sendErrorToMail(ex.ToString() + ", date retur = " + dateRetur  + " , id = " + idCmd );

                return "-1";
            }
            finally
            {
                connection.Close();
                connection.Dispose();

                if (transaction != null)
                    transaction.Dispose();

            }

            return retVal;

        }


        private void addReturImg(OracleConnection connection, OracleTransaction transaction, string document, ArticolRetur articolRetur)
        {
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = connection;
            cmd.Transaction = transaction;

            try
            {
                if (articolRetur.pozeArticol == null)
                    return;

                List<PozaArticol> listPoze = new JavaScriptSerializer().Deserialize<List<PozaArticol>>(articolRetur.pozeArticol);

                for (int i = 0; i < listPoze.Count; i++)
                {

                    string query = " insert into zpozeretur(idcmd, codarticol, idimg, imgarticol) " +
                                   " values (:document,:articol,:idimg, :img) ";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":document", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = document;

                    cmd.Parameters.Add(":articol", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = articolRetur.cod.Length == 8 ? "0000000000" + articolRetur.cod : articolRetur.cod;

                    cmd.Parameters.Add(":idimg", OracleType.VarChar, 2).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = i.ToString();

                    OracleParameter blobParameter = new OracleParameter();
                    blobParameter.OracleType = OracleType.Blob;
                    blobParameter.ParameterName = "img";
                    blobParameter.Value = Encoding.ASCII.GetBytes(listPoze[i].strData);

                    cmd.Parameters.Add(blobParameter).Direction = ParameterDirection.Input;

                    cmd.ExecuteNonQuery();

                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                cmd.Dispose();
            }


        }

        private string saveComandaReturToWS(string dateRetur, string idComanda)
        {
            string response = "";
            var serializer = new JavaScriptSerializer();

            try
            {
                ComandaRetur comanda = serializer.Deserialize<ComandaRetur>(dateRetur);
                List<ArticolRetur> listaArticole = serializer.Deserialize<List<ArticolRetur>>(comanda.listaArticole);

                SAPWebServices.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE();
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(DatabaseConnections.getUser(), DatabaseConnections.getPass());
                webService.Credentials = nc;
                webService.Timeout = 300000;

                SAPWebServices.ZretMarfa inParam = new ZretMarfa();
                inParam.VVbeln = comanda.nrDocument;
                inParam.VData = comanda.dataLivrare;
                inParam.VTraty = comanda.tipTransport;
                inParam.VPernr = comanda.codAgent;
                inParam.TipPers = comanda.tipAgent;
                inParam.VAugru = comanda.motivRetur;
                inParam.VPers = comanda.numePersContact;
                inParam.VAddrnumber = comanda.adresaCodAdresa;
                inParam.VTelef = comanda.telPersContact;
                inParam.VTranspzone = comanda.adresaCodJudet;
                inParam.VCity = comanda.adresaOras.Length < 25 ? comanda.adresaOras : comanda.adresaOras.Substring(0, 24);
                inParam.VStreet = comanda.adresaStrada;
                inParam.VInlocuire = comanda.inlocuire != null && comanda.inlocuire.Equals("X") ? "X" : " ";
                inParam.VId = Decimal.Parse(idComanda, CultureInfo.InvariantCulture);

                string obsTransp = " ";
                if (comanda.transpBack != null && Boolean.Parse(comanda.transpBack))
                    obsTransp = "X";

                inParam.VAcelasiTransp = obsTransp;

                SAPWebServices.ZmaterialeRetur[] articoleRetur = new ZmaterialeRetur[listaArticole.Count];

                for (int i = 0; i < listaArticole.Count; i++)
                {
                    articoleRetur[i] = new ZmaterialeRetur();
                    articoleRetur[i].Matnr = listaArticole[i].cod.Length == 8 ? "0000000000" + listaArticole[i].cod : listaArticole[i].cod;
                    articoleRetur[i].Cant = Decimal.Parse(listaArticole[i].cantitateRetur, CultureInfo.InvariantCulture);
                    articoleRetur[i].Um = listaArticole[i].um;
                    articoleRetur[i].Motiv = listaArticole[i].motivRespingere != null ? listaArticole[i].motivRespingere : " ";
                }

                inParam.ItMateriale = articoleRetur;

                SAPWebServices.ZretMarfaResponse responseRetur = new ZretMarfaResponse();
                responseRetur = webService.ZretMarfa(inParam);
                response = responseRetur.VOk.Equals("0") ? "0" : responseRetur.VMessage;


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                response = ex.ToString();
            }

            return response;
        }



        public string saveComandaRetur(string dateRetur, string tipRetur)
        {

            string retVal = "";

            if (tipRetur == null || (tipRetur != null && tipRetur.Equals("PAL")))
                retVal = saveReturPaleti(dateRetur);
            else if (tipRetur.Equals("CMD"))
                retVal = saveReturComanda(dateRetur);

            return retVal;

        }

        private string saveReturComanda(string dateRetur)
        {
            List<string> listComenzi = getComenziRetur(dateRetur);

            string retVal = "";

            var serializer = new JavaScriptSerializer();
            ComandaRetur comanda = serializer.Deserialize<ComandaRetur>(dateRetur);

            foreach (var cmdRetur in listComenzi)
                retVal = saveComandaReturToDB(cmdRetur);

            return retVal;

        }

        private List<string> getComenziRetur(string dateRetur)
        {
            List<string> listComenzi = new List<string>();

            try {

                var serializer = new JavaScriptSerializer();
                ComandaRetur comanda = serializer.Deserialize<ComandaRetur>(dateRetur);

                List<ArticolRetur> listaArticole = serializer.Deserialize<List<ArticolRetur>>(comanda.listaArticole);
                List<ArticolRetur> listArtInlocuire = new List<ArticolRetur>();
                List<ArticolRetur> listArtRetur = new List<ArticolRetur>();


                foreach (var articol in listaArticole)
                {
                    if (articol.inlocuire)
                        listArtInlocuire.Add(articol);
                    else
                        listArtRetur.Add(articol);
                }

                if (listArtInlocuire.Count > 0)
                {
                    ComandaRetur comandaInlocuire = serializer.Deserialize<ComandaRetur>(dateRetur);
                    comandaInlocuire.inlocuire = "X";
                    comandaInlocuire.listaArticole = serializer.Serialize(listArtInlocuire);
                    listComenzi.Add(serializer.Serialize(comandaInlocuire));
                }

                if (listArtRetur.Count > 0)
                {
                    ComandaRetur comandaRetur = serializer.Deserialize<ComandaRetur>(dateRetur);
                    comandaRetur.listaArticole = serializer.Serialize(listArtRetur);
                    listComenzi.Add(serializer.Serialize(comandaRetur));
                }
            }
            catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail("getComenziRetur: " + ex.ToString() + " -> " + dateRetur);
            }

            return listComenzi;
        }


        private string saveReturPaleti(string dateRetur)
        {
            string retVal = "";
            var serializer = new JavaScriptSerializer();

            ComandaRetur comanda = serializer.Deserialize<ComandaRetur>(dateRetur);
            dateRetur = setArticolTransportPaleti(dateRetur);
            retVal = saveComandaReturToDB(dateRetur);

            return retVal;
        }


        private string getAdreseLivrareClient(string codClient)
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

                cmd.CommandText = " select nvl(a.city1,' ') city1 , nvl(a.street,' ') street, " +
                                  " nvl(a.house_num1,' ') house_num, nvl(region,' '), a.addrnumber from sapprd.adrc a " +
                                  " where a.client = '900' and a.addrnumber =  " +
                                  " (select k.adrnr from sapprd.kna1 k where k.mandt = '900' and k.kunnr =:codClient) " +
                                  " union " +
                                  " select nvl(z.localitate,' '), nvl(z.adr_livrare,' ') , ' ', nvl(z.regio,' '), z.nr_crt from sapprd.zclient_adrese z " +
                                  " where kunnr =:codClient ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

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
                        listaAdreseLivrare.Add(oAdresa);

                    }

                }

                oReader.Close();
                oReader.Dispose();
                cmd.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaAdreseLivrare);

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

            return serializedResult;
        }


        private string getPersoanaContactComanda(OracleConnection connection, string nrDocument)
        {
            string serializedResult = "";
            OracleDataReader oReader = null;
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = connection;

            try
            {
                cmd.CommandText = " select c.name1, c.tel_number from sapprd.vbpa a inner join sapprd.adrc c on a.mandt = c.client and a.adrnr = c.addrnumber " +
                                  " where a.mandt = '900' and a.vbeln = :nrDoc and parvw = 'AP' ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrDoc", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                oReader = cmd.ExecuteReader();

                List<PersoanaContact> listContacte = new List<PersoanaContact>();
                PersoanaContact persoana = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        persoana = new PersoanaContact();
                        persoana.nume = oReader.GetString(0);
                        persoana.telefon = oReader.GetString(1);
                        listContacte.Add(persoana);
                    }
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listContacte);

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return serializedResult;

        }

        private string getPersoaneContact(string codClient)
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

                cmd.CommandText = " select namev, name1, telf1 from sapprd.knvk u where u.mandt = '900' and " +
                                  " (u.parnr, u.kunnr) in (select distinct p.parnr, p.kunnr from sapprd.knvp p where p.mandt = '900' " +
                                  " and p.kunnr =:codClient and parvw = 'AP')";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                List<PersoanaContact> listContacte = new List<PersoanaContact>();
                PersoanaContact persoana = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        persoana = new PersoanaContact();
                        persoana.nume = oReader.GetString(0) + " " + oReader.GetString(1);
                        persoana.telefon = oReader.GetString(2);
                        listContacte.Add(persoana);
                    }
                }

                oReader.Close();
                oReader.Dispose();
                cmd.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listContacte);

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

            return serializedResult;
        }



        public string getListClientiCV(string numeClient, string unitLog, string tipCmd, string tipUserSap)
        {
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string criteriuCautare = " lower(c.name1) like lower('%" + numeClient.ToLower() + "%') ";

            if (numeClient.StartsWith("tel:"))
                criteriuCautare = " c.tel_number like ('" + numeClient.Replace("tel:", "") + "%') ";

            string sintPaleti = " and p.matkl in ('433', '433_1', '716', '626', '929_2','515') ";

            if (tipCmd != null && !tipCmd.Equals(String.Empty) && tipCmd.Equals("CMD"))
                sintPaleti = "";

            string critFiliala = " and p.prctr =:unitLog ";
            if (tipUserSap != null && (tipUserSap.Equals("CVIP") || tipUserSap.Equals("SDIP")))
                critFiliala = "";

            string tipDocuRetur = " ('ZFM','ZFMC','ZFS','ZFSC','ZFPA', 'ZFVS','ZFCS') ";

            if (tipUserSap != null && tipUserSap.Equals("CVO"))
                tipDocuRetur = " ('ZFM','ZFMC','ZFS','ZFSC','ZFPA', 'ZFVS','ZFCS', 'ZFHC', 'ZF2H') ";

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                OracleCommand cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd.CommandText = " select distinct c.name1, k.kunrg from sapprd.vbrk k, sapprd.vbrp p, sapprd.vbpa a, sapprd.adrc c " +
                                  " where k.mandt = p.mandt and k.vbeln = p.vbeln and k.mandt = '900' and k.fkart in " + tipDocuRetur + 
                                  " and k.fksto <> 'X' and k.fkdat >= to_char(sysdate-270,'yyyymmdd') " + sintPaleti +
                                  " and k.mandt = a.mandt and k.vbeln = a.vbeln and a.parvw = 'WE' " + critFiliala + 
                                  " and a.mandt = c.client and a.adrnr = c.addrnumber  and " + criteriuCautare + " order by c.name1 ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                if (!critFiliala.Equals(String.Empty))
                {
                    cmd.Parameters.Add(":unitLog", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = unitLog;
                }

                oReader = cmd.ExecuteReader();

                List<ClientIP> listaClienti = new List<ClientIP>();
                ClientIP unClient = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        unClient = new ClientIP();
                        unClient.numeClient = oReader.GetString(0);
                        unClient.codClient = oReader.GetString(0);
                        unClient.tipClient = "0";
                        unClient.tipClientIP = HelperClienti.getDateClientInstPublica(connection, oReader.GetString(1)).tipClientInstPublica;
                        listaClienti.Add(unClient);
                    }
                }

                oReader.Close();
                oReader.Dispose();
                cmd.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaClienti);

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


            return serializedResult;
        }


        private string getArticolReturPaleti(string codAgent)
        {
            string artRetur = "000000000000000000";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.ConnectionString = connectionString;
                connection.Open();
                cmd = connection.CreateCommand();

                cmd.CommandText = " select ar.cod from agenti ag, articole ar where ag.cod = :codAgent " +
                                  " and ag.divizie = ar.grup_vz and ar.categ_mat = 'TR' ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgent;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    artRetur = oReader.GetString(0);
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

            return artRetur;
        }

        private string setArticolTransportPaleti(string comandaRetur)
        {

            var serializer = new JavaScriptSerializer();
            ComandaRetur comanda = serializer.Deserialize<ComandaRetur>(comandaRetur);

            string codArticolRetur = getArticolReturPaleti(comanda.codAgent);
            List<ArticolRetur> listaArticole = serializer.Deserialize<List<ArticolRetur>>(comanda.listaArticole);

            for (int i = 0; i < listaArticole.Count; i++)
            {
                if (listaArticole[i].cod.Equals("99999999"))
                {
                    listaArticole[i].cod = codArticolRetur;
                }
            }

            comanda.listaArticole = serializer.Serialize(listaArticole);
            return serializer.Serialize(comanda);
        }

        public string getStocReturAvansat(string nrDocument, string codArticol, string um)
        {
            string stocDisp = "0";
            var serializer = new JavaScriptSerializer();

            try
            {
                
                SAPWebServices.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE();
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(DatabaseConnections.getUser(), DatabaseConnections.getPass());
                webService.Credentials = nc;
                webService.Timeout = 300000;

                SAPWebServices.ZstocReturAvansat inParam = new ZstocReturAvansat();
                inParam.IpVbelnVf = nrDocument;
                inParam.IpMatnr = codArticol;
                inParam.IpUm = um;

                SAPWebServices.ZstocReturAvansatResponse responseRetur = new ZstocReturAvansatResponse();
                responseRetur = webService.ZstocReturAvansat(inParam);
                stocDisp = responseRetur.EpStocDisp.ToString();
                
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }


            return stocDisp;

        }

    }
}