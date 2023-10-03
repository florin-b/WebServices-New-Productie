using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using System.Web.Script.Serialization;
using System.Globalization;
using WebService1.SAPWebServices;
using WebService1.General;

namespace WebService1
{
    public class OperatiiComenzi
    {

        public string getArticoleComanda(string nrComanda, string afisConditii, string tipUser)
        {

            string unitLog1 = "";
            string cmp = "";
            string serializedResult = "";

            //
            //afisCond:
            // 1 - pentru agent
            // 2 - pentru aprobare comanda SD
            // 3 - pentru aprobare comanda DV


            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleCommand cmdInner = null;
            OracleDataReader oReader = null, oReader1 = null;
            OracleDataReader oReaderInner = null;
            int nrArt = 0;
            string infoPret = " , '0' info";

            String pretMediu = "0", adaosMediu = "0", unitMasPretMediu = "0";

            try
            {
                //antet comanda
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                cmd.CommandText = " select a.pers_contact, a.telefon, a.adr_livrare, a.mt, a.tip_plata, a.cantar, a.fact_red, a.city, a.region, a.ul, " +
                                  " decode(a.pmnttrms,'',' ',a.pmnttrms) pmnttrms, decode(a.obstra,'',' ',a.obstra) obstra, b.tip_pers from sapprd.zcomhead_tableta a, clienti b " +
                                  " where a.id=:idcmd and a.cod_client = b.cod ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrComanda);

                oReader = cmd.ExecuteReader();

                ComandaAprobata comanda = new ComandaAprobata();


                if (oReader.HasRows)
                {
                    oReader.Read();

                    unitLog1 = oReader.GetString(9);

                    comanda.persoanaContact = oReader.GetString(0);
                    comanda.telefon = oReader.GetString(1);
                    comanda.adresaLivrare = oReader.GetString(2);
                    comanda.tipTransport = oReader.GetString(3);
                    comanda.metodaPlata = oReader.GetString(4);
                    comanda.cantarire = oReader.GetString(5);
                    comanda.factRedSeparat = oReader.GetString(6);
                    comanda.oras = oReader.GetString(7);
                    comanda.codJudet = oReader.GetString(8);
                    comanda.unitLog = oReader.GetString(9);
                    comanda.termenPlata = oReader.GetString(10);
                    comanda.obsLivrare = oReader.GetString(11);
                    comanda.tipPersClient = oReader.GetString(12);

                }


                oReader.Close();
                oReader.Dispose();

                //articole comenzi

                if (afisConditii == "3" || afisConditii == "1") //3 = pentru DV se afiseaza si CMP, 1 = modif.cmd pt. a verifica art. sub cmp
                {
                    cmp = " , nvl(( select nvl(to_char(decode(y.lbkum,0,y.verpr,y.salk3/y.lbkum),'99999.9999'),0) from sapprd.mbew y where " +
                                 " y.mandt='900' and y.matnr=a.cod  and y.bwkey = '" + unitLog1 + "'" +
                                 " ),0) cmp  ";
                }
                else
                {
                    cmp = ", '-1' cmp ";
                }

                string condTabKA = " , sapprd.zcomhead_tableta z ", condIdKA = " and a.id=:idcmd and z.id = a.id ", condOrderKA = " poz ";
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
                cmd.Parameters[0].Value = Int32.Parse(nrComanda);

                oReader1 = cmd.ExecuteReader();
                String unitLogAlt = "NN10", depart = "00", tipMat = "", lnumeArt = "", lDepoz = "";

                ArticolComandaRap articol = null;
                List<ArticolComandaRap> listArticole = new List<ArticolComandaRap>();

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
                            // lDepoz = "02T1";
                        }

                        if (oReader1.GetString(22).Equals("BV90") && depart.Equals("05"))
                        {
                            lDepoz = "BV90";
                        }


                        if (tipMat.Trim() != "")
                            lnumeArt += " (" + tipMat + ")";

                        if (oReader1.GetString(1).Equals("00000000"))
                            lnumeArt = "Taxa verde";


                        articol = new ArticolComandaRap();
                        articol.status = oReader1.GetString(0);
                        articol.codArticol = oReader1.GetString(1);
                        articol.numeArticol = lnumeArt;
                        articol.cantitate = oReader1.GetFloat(3);
                        articol.depozit = lDepoz;
                        articol.pretUnit = oReader1.GetFloat(5);
                        articol.um = oReader1.GetString(6);
                        articol.procent = oReader1.GetFloat(7);
                        articol.procentFact = oReader1.GetFloat(8);
                        articol.conditie = oReader1.GetString(9).Equals("X") ? true : false;
                        articol.cmp = Double.Parse(oReader1.GetString(10), CultureInfo.InvariantCulture);
                        articol.discClient = oReader1.GetFloat(11);
                        articol.discountAg = oReader1.GetFloat(12);
                        articol.discountSd = oReader1.GetFloat(13);
                        articol.discountDv = oReader1.GetFloat(14);
                        articol.procAprob = oReader1.GetFloat(15);
                        articol.permitSubCmp = oReader1.GetString(16);
                        articol.multiplu = oReader1.GetFloat(17);
                        articol.pret = oReader1.GetFloat(18);
                        articol.infoArticol = oReader1.GetString(19);
                        articol.cantUmb = oReader1.GetFloat(20).ToString();
                        articol.Umb = oReader1.GetString(21);
                        articol.depart = oReader1.GetString(22);
                        articol.tipArt = oReader1.GetString(23);

                        //preturi medii
                        if ((tipUser.Equals("CV") || tipUser.Equals("SM")) && afisConditii.Equals("1") && !articol.depart.Equals("11"))
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

                            articol.pretMediu = pretMediu;
                            articol.adaosMediu = adaosMediu;
                            articol.unitMasPretMediu = unitMasPretMediu;

                        }


                        listArticole.Add(articol);

                        unitLogAlt = oReader1.GetString(22).Trim() != "" ? oReader1.GetString(22) : "NN10";

                        nrArt++;
                    }

                }

                comanda.filialaAlternativa = unitLogAlt;

                oReaderInner.Close();
                oReaderInner.Dispose();

                oReader1.Close();
                oReader1.Dispose();

                //articole conditii


                //antet
                string condAfis = "";
                int IdCndArt = -1;

                if (afisConditii == "1")
                    condAfis = " cmdref=:idcmd ";

                if (afisConditii == "2" || afisConditii == "3")
                    condAfis = " cmdmodif=:idcmd ";

                cmd.CommandText = " select id, condcalit, nrfact, nvl(observatii,' ') observatii from sapprd.zcondheadtableta where " + condAfis;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrComanda);

                oReader1 = cmd.ExecuteReader();

                ConditiiComanda conditiiComanda = new ConditiiComanda();
                ArticolComanda articolConditie = null;
                List<ArticolComanda> listArticoleConditii = new List<ArticolComanda>();

                if (oReader1.HasRows)
                {
                    oReader1.Read();
                    IdCndArt = oReader1.GetInt32(0);

                    conditiiComanda.condCalitative = oReader.GetFloat(1).ToString();
                    conditiiComanda.nrFacturi = oReader1.GetInt32(2).ToString();
                    conditiiComanda.observatii = oReader1.GetString(3);
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
                            articolConditie = new ArticolComanda();
                            articolConditie.codArticol = oReader1.GetString(0);
                            articolConditie.numeArticol = oReader1.GetString(1);
                            articolConditie.cantitate = oReader1.GetFloat(2);
                            articolConditie.um = oReader1.GetString(3);
                            articolConditie.pretUnit = oReader1.GetDouble(4);
                            articolConditie.multiplu = oReader1.GetDouble(5);
                            listArticoleConditii.Add(articolConditie);

                        }

                    }

                    oReader1.Close();
                    oReader1.Dispose();

                }


                //sf. conditii

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string stringArticoleConditii = serializer.Serialize(listArticoleConditii);
                conditiiComanda.articole = stringArticoleConditii;

                string stringConditiiComanda = serializer.Serialize(conditiiComanda);
                string stringArticoleComanda = serializer.Serialize(listArticole);

                comanda.articole = stringArticoleComanda;
                comanda.conditiiComanda = stringConditiiComanda;

                serializedResult = serializer.Serialize(comanda);




            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + "Nr cmd: " + nrComanda);
                ErrorHandling.sendErrorToMail(cmd.CommandText);
            }
            finally
            {
                cmd.Dispose();

                if (cmdInner != null)
                    cmdInner.Dispose();

                connection.Close();
                connection.Dispose();
            }


            return serializedResult;
        }




        public string getComenziDeschise(string codAgent)
        {

            string serializedResult = "";
            List<ComandaActiva> listComenzi = new List<ComandaActiva>();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                 string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                

                cmd.CommandText = " select b.name1, c.valoare, c.city, nvl(c.adr_livrare,' '),  c.nrcmdsap, c.cod_client, b.nr_bord, a.masina, c.region, d.nume, " +
                                 " nvl(d.telefon,'-') telefon, a.sttrg, nvl(c.telefon,'-') tel_client  " +
                                 " from borderouri a, sapprd.zdocumentesms b, sapprd.zcomhead_tableta c, soferi d" +
                                 "  where a.sttrg in ( 2, 3, 4, 6, 7) and b.cod_av=:codAgent and a.numarb = b.nr_bord and c.id = b.idcomanda and d.cod = a.cod_sofer " +
                                 " union " +
                                 " select b.name1 ||' (CLP)' name1, 0 valoare, c.city, nvl(c.adr_livrare, ' '),  c.nrcmdsap, c.cod_client, b.nr_bord, a.masina, c.region, d.nume, " +
                                 " nvl(d.telefon, '-') telefon, a.sttrg, nvl(c.telefon,'-') tel_client " +
                                 " from borderouri a, sapprd.zdocumentesms b, sapprd.zclphead c, soferi d " +
                                 " where a.sttrg in (2, 3, 4, 6, 7) and b.cod_av = :codAgent and a.numarb = b.nr_bord and c.id = b.iddlclp and d.cod = a.cod_sofer " +
                                 " union " +
                                 " select distinct b.name1 ||' (ONLINE)' name1, k.netwr valoare, a.city1, nvl(a.street,' '), " +
                                 " k.vbeln nrcmdsap, k.kunnr cod_client, b.nr_bord, a.masina, a.region, d.nume, " +
                                 " nvl(d.telefon, '-') telefon, a.sttrg, nvl(z.telefon,'-') tel_client " +
                                 " from borderouri a, sapprd.zdocumentesms b, sapprd.vbak k, soferi d, sapprd.vbfa f, sapprd.vbpa p, sapprd.adrc a, sapprd.zcomhead_tableta z " +
                                 " where a.sttrg in (2, 3, 4, 6, 7) and b.cod_av =:codAgent and a.numarb = b.nr_bord and f.mandt = '900' and f.vbeln = b.nr_doc and d.cod = a.cod_sofer " +
                                 " and k.mandt = '900' and k.vbeln = f.vbelv and f.vbtyp_v = 'C' and f.vbtyp_n = 'J' " +
                                 " and k.auart in ('ZTAH','ZEPH') and k.mandt = p.mandt and k.vbeln = p.vbeln and p.parvw = 'WE' and p.mandt = a.client and p.adrnr = a.addrnumber" +
                                 " and z.mandt(+) = '900' and z.id(+) = b.idcomanda " +
                                 " union all " +
                                 " select distinct b.name1 || ' (CUSTODIE)' name1, k.netwr valoare, " +
                                 " a.city1, nvl(a.street, ' '), k.vbeln nrcmdsap, k.kunnr cod_client, b.nr_bord, a.masina, a.region, d.nume, " +
                                 " nvl(d.telefon, '-') telefon, a.sttrg, nvl(z.telefon,'-') tel_client from websap.borderouri a, sapprd.zdocumentesms b, sapprd.likp k, " +
                                 " websap.soferi d, sapprd.vbpa p, sapprd.adrc a, sapprd.zcomhead_tableta z where a.sttrg in (2, 3, 4, 6, 7) and b.cod_av = :codAgent " +
                                 " and a.numarb = b.nr_bord and k.mandt = '900' and k.vbeln = b.nr_doc and d.cod = a.cod_sofer and k.lfart = 'ZW' " +
                                 " and k.mandt = p.mandt and k.vbeln = p.vbeln and p.parvw = 'WE' and p.mandt = a.client and p.adrnr = a.addrnumber " +
                                 " and z.mandt(+)='900' and z.id(+) = b.idcomanda " +
                                 " order by name1 ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgent;

                oReader = cmd.ExecuteReader();

                ComandaActiva comanda = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        comanda = new ComandaActiva();
                        comanda.numeClient = oReader.GetString(0);
                        comanda.valoare = oReader.GetDouble(1).ToString();
                        comanda.localitate = oReader.GetString(2);
                        comanda.strada = oReader.GetString(3);
                        comanda.idCmdSap = oReader.GetString(4);
                        comanda.codClient = oReader.GetString(5);
                        comanda.codBorderou = oReader.GetString(6);
                        comanda.nrMasina = oReader.GetString(7);
                        comanda.codJudet = oReader.GetString(8);
                        comanda.numeSofer = oReader.GetString(9);
                        comanda.telSofer = oReader.GetString(10);
                        comanda.stareComanda = oReader.GetString(11);
                        comanda.telClient = oReader.GetString(12);
                        listComenzi.Add(comanda);

                    }


                }


                oReader.Close();
                oReader.Dispose();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + cmd.CommandText + " , " + codAgent);
            }
            finally
            {
                serializedResult = "-1";
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializedResult = serializer.Serialize(listComenzi);

            
            return serializedResult;


        }


        public string getClientiBorderou(string codBorderou)
        {
            string serializedResult = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<ClientBorderou> listClienti = new List<ClientBorderou>();

            try
            {
                 string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                
                cmd.CommandText = " select b.cod_client, b.poz,  nvl(a.ora,' '), b.adresa_client, nvl(c.pozitie,-1) ordine_sofer  from sapprd.zevenimentsofer a, " +
                                  " sapprd.zdocumentesms b, sapprd.zordinelivrari c where b.nr_bord =:codBorderou " +
                                  " and a.document(+) = b.nr_bord and a.eveniment(+) = 'S'  and a.client(+) = b.cod_client " +
                                  " and a.codadresa(+) = b.adresa_client " + 
                                  " and c.mandt(+) = '900' and c.borderou(+) = b.nr_bord and c.client(+) = b.cod_client and c.codadresa(+) = b.adresa_client " +
                                  " order by ordine_sofer, b.poz ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codBorderou", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codBorderou;

                oReader = cmd.ExecuteReader();

                ClientBorderou client = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        client = new ClientBorderou();
                        client.codClient = oReader.GetString(0);
                        client.pozitie = oReader.GetInt32(4) == -1 ? oReader.GetString(1) : oReader.GetInt32(4).ToString();
                        client.dataEveniment = oReader.GetString(2);
                        listClienti.Add(client);

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
                serializedResult = "-1";
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializedResult = serializer.Serialize(listClienti);

            return serializedResult;


        }



        public string getPozitieMasina(string nrMasina)
        {



            string coords = "0#0";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<ClientBorderou> listClienti = new List<ClientBorderou>();

            try
            {
                 string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select nvl(latitude,0), nvl(longitude,0) from gps_index where device_id = ( select id from gps_masini where nr_masina = replace(:nrMasina, '-', '')) ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrMasina", OracleType.VarChar, 10).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrMasina;

                oReader = cmd.ExecuteReader();


                if (oReader.HasRows)
                {
                    oReader.Read();
                    coords = oReader.GetDouble(0).ToString() + "#" + oReader.GetDouble(1).ToString();
                }


                oReader.Close();
                oReader.Dispose();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                coords = "-1";
            }
            finally
            {

                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }


            return coords;
        }



        public static ClientComanda getClientComanda(OracleConnection connection, string idComanda)
        {
            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            ClientComanda clientComanda = new ClientComanda();

            try
            {
                cmd = connection.CreateCommand();

                string sqlString = " select a.cod_client, a.nrcmdsap, b.adrnr from sapprd.zcomhead_tableta a, sapprd.vbpa b, clienti c " +
                                   " where a.id=:idComanda and b.vbeln = a.nrcmdsap and b.parvw = 'WE' and b.mandt = '900' and c.cod = a.cod_client and c.tip_pers = 'PJ' ";

                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    clientComanda.codClient = oReader.GetString(0);
                    clientComanda.idComandaSap = oReader.GetString(1);
                    clientComanda.codAdresa = oReader.GetString(2);

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

            return clientComanda;
        }



        public static Adresa getAdresaComanda(OracleConnection connection, string idComanda, string codAdresa)
        {

            Adresa adresa = new Adresa();

            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {
                cmd = connection.CreateCommand();

                string sqlString = " select latitude, longitude from sapprd.zcoordcomenzi where idcomanda =:idComanda ";


                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;


                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    adresa.latitude = oReader.GetString(0);
                    adresa.longitude = oReader.GetString(1);

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

            return adresa;

        }


        public static string respingeComanda(OracleConnection connection, String nrCmd, String codUser, String codRespingere)
        {

            string retVal = " ";

            SAPWebServices.ZTBL_WEBSERVICE webService = null;
            webService = new ZTBL_WEBSERVICE();

            SAPWebServices.ZstareComanda inParam = new SAPWebServices.ZstareComanda();
            inParam.NrCom = nrCmd;
            inParam.Stare = "3";
            inParam.PernrCh = codUser;

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;

            SAPWebServices.ZstareComandaResponse outParam = webService.ZstareComanda(inParam);

            string response = outParam.VOk;

            OracleCommand cmd = new OracleCommand();

            cmd = connection.CreateCommand();

            cmd.CommandText = " update sapprd.zcomhead_tableta set abgru = '" + codRespingere + "' where id=:cmd ";

            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Clear();
            cmd.Parameters.Add(":cmd", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
            cmd.Parameters[0].Value = nrCmd;

            cmd.ExecuteNonQuery();

            cmd.Dispose();


            if (response.Equals("0"))
            {
                retVal = "Comanda respinsa.";
            }
            else
            {
                retVal = outParam.VMess;
            }

            webService.Dispose();


            return retVal;

        }


        public string saveLivrareCustodie(string JSONArt, string JSONComanda, string JSONDateLivrare)
        {

            var serializer = new JavaScriptSerializer();
            string query;
            string retVal = "-1#0";

            ComandaVanzare comandaVanzare = serializer.Deserialize<ComandaVanzare>(JSONComanda);
            DateLivrare dateLivrare = serializer.Deserialize<DateLivrare>(JSONDateLivrare);
            List<ArticolComanda> articolComanda = serializer.Deserialize<List<ArticolComanda>>(JSONArt);

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;
            OracleTransaction transaction = null;
            SAPWSCustodie.zwbs_custodie webService = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                cmd = connection.CreateCommand();

                DateTime cDate = DateTime.Now;
                string year = cDate.Year.ToString();
                string day = cDate.Day.ToString("00");
                string month = cDate.Month.ToString("00");
                string nowDate = year + month + day;

                connection.ConnectionString = connectionString;
                connection.Open();

                string departLivare = Utils.getDepartArticol(connection, articolComanda[0].codArticol);
                string numeClient = Utils.getNumeClient(connection, comandaVanzare.codClient);

                transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;


                query = " insert into sapprd.zcust_head(mandt, id, cod_client, cod_agent, ul, depart, status, datac,  pers_contact, telefon, adr_livrare, " +
                        "  city, region, den_cl, ketdat, addrnumber, macara, descoperita, traty ) values " +
                        " ('900', pk_key.nextval, :cod_client, :cod_agent, :ul, :depart, :status, :datac,  :pers_contact, :telefon, :adr_livrare, " +
                        "  :city, :region, :den_cl, :ketdat, :addrnumber, :macara, :descoperita, :traty ) returning id into :id ";


                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":cod_client", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = comandaVanzare.codClient;

                cmd.Parameters.Add(":cod_agent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = dateLivrare.codAgent;

                cmd.Parameters.Add(":ul", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = dateLivrare.unitLog;

                cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = departLivare;

                cmd.Parameters.Add(":status", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = "0";

                cmd.Parameters.Add(":datac", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = nowDate;

                cmd.Parameters.Add(":pers_contact", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = dateLivrare.persContact;

                cmd.Parameters.Add(":telefon", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[7].Value = dateLivrare.nrTel;

                cmd.Parameters.Add(":adr_livrare", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[8].Value = dateLivrare.Strada + " ";

                cmd.Parameters.Add(":city", OracleType.VarChar, 75).Direction = ParameterDirection.Input;
                cmd.Parameters[9].Value = dateLivrare.Oras;

                cmd.Parameters.Add(":region", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[10].Value = dateLivrare.codJudet;

                cmd.Parameters.Add(":den_cl", OracleType.VarChar, 90).Direction = ParameterDirection.Input;
                cmd.Parameters[11].Value = numeClient;

                cmd.Parameters.Add(":ketdat", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[12].Value = Utils.formatDateToSap(dateLivrare.dataLivrare);

                cmd.Parameters.Add(":addrnumber", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[13].Value = dateLivrare.addrNumber;

                cmd.Parameters.Add(":macara", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[14].Value = dateLivrare.macara;

                cmd.Parameters.Add(":descoperita", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[15].Value = Convert.ToBoolean(dateLivrare.isCamionDescoperit) ? "X" : " ";

                cmd.Parameters.Add(":traty", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[16].Value = dateLivrare.Transport;

                OracleParameter idCmd = new OracleParameter("id", OracleType.Number);
                idCmd.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(idCmd);

                cmd.ExecuteNonQuery();

                int artLen = articolComanda.Count;
                int pozArt;
                string codArt;

                for (int i = 0; i < artLen; i++)
                {
                    pozArt = (i + 1) * 10;

                    codArt = articolComanda[i].codArticol;

                    if (codArt.Length == 8)
                        codArt = "0000000000" + codArt;

                    query = " insert into sapprd.zcust_det(mandt, id, poz, status, cod, cantitate, um ) values " +
                            " ('900', :id, :poz, :status, :cod, :cantitate, :um ) ";

                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":id", OracleType.Int32, 10).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = idCmd.Value;

                    cmd.Parameters.Add(":poz", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = pozArt;

                    cmd.Parameters.Add(":status", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = "0";

                    cmd.Parameters.Add(":cod", OracleType.NVarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = codArt;

                    cmd.Parameters.Add(":cantitate", OracleType.Double, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = articolComanda[i].cantitate;

                    cmd.Parameters.Add(":um", OracleType.NVarChar, 9).Direction = ParameterDirection.Input;
                    cmd.Parameters[5].Value = articolComanda[i].um;

                    cmd.ExecuteNonQuery();


                }

                transaction.Commit();

                OperatiiAdresa.insereazaCoordonateAdresa(connection, idCmd.Value.ToString(), dateLivrare.coordonateGps, dateLivrare.codJudet, dateLivrare.Oras);
                OperatiiSuplimentare.saveTonajComanda(connection, idCmd.Value.ToString(), dateLivrare.tonaj);
                new OperatiiSuplimentare().savePrelucrare04(connection, idCmd.Value.ToString(), dateLivrare.prelucrare);

                if (dateLivrare.addrNumber.Trim() != "" && dateLivrare.adrLivrNoua)
                    OperatiiSuplimentare.saveTonajAdresa(connection, comandaVanzare.codClient, dateLivrare.addrNumber, dateLivrare.tonaj);

                webService = new SAPWSCustodie.zwbs_custodie();
                SAPWSCustodie.ZlivrareCustodie inParam = new SAPWSCustodie.ZlivrareCustodie();
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
                webService.Credentials = nc;
                webService.Timeout = 300000;

                inParam.IpId = idCmd.Value.ToString();

                SAPWSCustodie.ZlivrareCustodieResponse response = webService.ZlivrareCustodie(inParam);
                retVal = response.EpOk + "#" + response.EpMesaj;

                webService.Dispose();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }


            return retVal;

        }

        public List<Comanda> getLivrariCustodie(string codAgent, string interval, string tipCmd)
        {
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand(), cmd1 = new OracleCommand();
            OracleDataReader oReader = null;
            string dateInterval = DateTime.Today.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            string condData = " and cst.datac =:interval ";

            string tipComanda = "";

            //modificare
            if (tipCmd == "1")
                tipComanda = " and cst.status != '6' " +
                             " and not exists ( select * from sapprd.vttp p where p.mandt = '900' and p.vbeln = cst.vbeln) " +
                             " and nvl((select sum(decode(f.bwart, 'ZW4', -1 * f.rfmng, f.rfmng)) from sapprd.vbfa f where f.mandt = '900' " +
                             " and f.vbelv = cst.vbeln and f.vbtyp_v = 'J' and bwart in ('ZW3', 'ZW4')),0) = 0 ";

            List<Comanda> listComenzi = new List<Comanda>();

            if (interval == "0") //astazi
            {
                dateInterval = DateTime.Today.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and cst.datac =:interval ";
            }
            else if (interval == "1") //ultimele 7 zile
            {
                dateInterval = DateTime.Today.AddDays(-7).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and cst.datac >=:interval";
            }
            else if (interval == "2") //ultimele 30 zile
            {
                dateInterval = DateTime.Today.AddDays(-30).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and cst.datac >=:interval ";
            }

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string sqlString = " select cst.id, cl.nume, to_char(to_date(cst.datac,'yyyymmdd')), cst.status, cst.cod_client, cst.nrcmdsap, cst.ul, ag.nume numeag, " +
                                   " cst.adr_livrare, ag.divizie, cst.depart, ag.nrtel, cst.ketdat, cst.vbeln " +
                                   " from sapprd.zcust_head cst, clienti cl, agenti ag where cst.cod_agent =:codag  " + tipComanda +
                                   " and cst.cod_client = cl.cod and cst.cod_agent = ag.cod " + condData;

                cmd.CommandText = sqlString;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codag", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgent;

                cmd.Parameters.Add(":interval", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = dateInterval;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {

                    Comanda comanda;

                    while (oReader.Read())
                    {

                        comanda = new Comanda();
                        comanda.idComanda = oReader.GetInt32(0).ToString();
                        comanda.numeClient = oReader.GetString(1);
                        comanda.dataComanda = oReader.GetString(2) + "#" + oReader.GetString(12);
                        comanda.sumaComanda = "0";

                        comanda.stareComanda = oReader.GetString(3);
                        comanda.codClient = oReader.GetString(4);
                        comanda.cmdSap = oReader.GetString(13);
                        comanda.factRed = " ";
                        comanda.filiala = oReader.GetString(6);
                        comanda.accept1 = " ";
                        comanda.accept2 = " ";
                        comanda.tipClient = " ";
                        comanda.numeAgent = oReader.GetString(7);
                        comanda.termenPlata = " ";
                        comanda.docInsotitor = " ";
                        comanda.adresaLivrare = oReader.GetString(8);
                        comanda.divizieAgent = oReader.GetString(9);
                        comanda.divizieComanda = oReader.GetString(10);
                        comanda.aprobariNecesare = " ";
                        comanda.aprobariPrimite = " ";
                        comanda.codClientGenericGed = " ";
                        comanda.conditiiImpuse = " ";
                        comanda.telAgent = oReader.GetString(11);
                        comanda.monedaComanda = "RON";
                        comanda.monedaTVA = "RON";
                        comanda.sumaTVA = "0";
                        comanda.canalDistrib = "10";
                        comanda.cursValutar = "0";
                        comanda.adresaNoua = "-1";
                        comanda.pondere30 = "0";
                        comanda.avans = "0";
                        comanda.clientRaft = " ";
                        comanda.tipComanda = "CUST";



                        listComenzi.Add(comanda);
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


            return listComenzi;
        }




        public string getArticoleCustodie(string idComanda)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null, oReader1 = null;

            DateLivrareCmd dateLivrare = new DateLivrareCmd();
            List<ArticolComandaRap> listArticole = new List<ArticolComandaRap>();
            Conditii conditii = new Conditii();
            conditii.header = new ConditiiHeader();
            conditii.articole = new List<ConditiiArticole>();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select a.pers_contact, a.telefon, a.adr_livrare, a.traty,    a.city, a.region, a.ul, " +
                                  " b.tip_pers,   a.ketdat,  a.macara, " +
                                  " nvl((select latitude || ',' || longitude from sapprd.zcoordcomenzi where idcomanda = a.id),'0,0') coord,  " +
                                  " a.descoperita, nvl(a.addrnumber,'-1') from sapprd.zcust_head a, clienti b " +
                                  " where a.id = :idcmd and a.cod_client = b.cod ";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idComanda);

                oReader = cmd.ExecuteReader();


                if (oReader.HasRows)
                {
                    oReader.Read();

                    dateLivrare.persContact = oReader.GetString(0);
                    dateLivrare.nrTel = oReader.GetString(1);
                    dateLivrare.dateLivrare = oReader.GetString(2);
                    dateLivrare.Transport = oReader.GetString(3);
                    dateLivrare.tipPlata = " ";
                    dateLivrare.Cantar = " ";
                    dateLivrare.factRed = " ";
                    dateLivrare.redSeparat = " ";
                    dateLivrare.Oras = oReader.GetString(4);
                    dateLivrare.codJudet = oReader.GetString(5);
                    dateLivrare.unitLog = oReader.GetString(6);
                    dateLivrare.termenPlata = " ";
                    dateLivrare.obsLivrare = " ";
                    dateLivrare.tipPersClient = oReader.GetString(7);
                    dateLivrare.mail = " ";
                    dateLivrare.obsPlata = " ";
                    dateLivrare.dataLivrare = GeneralUtils.formatStrDate(oReader.GetString(8));
                    dateLivrare.adresaD = oReader.GetString(12);
                    dateLivrare.orasD = " ";
                    dateLivrare.codJudetD = " ";
                    dateLivrare.macara = oReader.GetString(9);
                    dateLivrare.numeClient = " ";
                    dateLivrare.cnpClient = " ";
                    dateLivrare.idObiectiv = " ";
                    dateLivrare.isAdresaObiectiv = false;
                    dateLivrare.coordonateGps = oReader.GetString(10);
                    dateLivrare.tonaj = "0";
                    dateLivrare.clientRaft = " ";
                    dateLivrare.meserias = " ";
                    dateLivrare.factPaletiSeparat = false;
                    dateLivrare.furnizorMarfa = " ";
                    dateLivrare.furnizorProduse = " ";
                    dateLivrare.isCamionDescoperit = oReader.GetString(11).Equals("X") ? true : false;
                    dateLivrare.diviziiClient = " ";
                    dateLivrare.filialaCLP = " ";
                    dateLivrare.nrCmdClp = " ";
                    dateLivrare.tipPlata = " ";



                }



                cmd.CommandText = " select a.status, decode(length(a.cod),18,substr(a.cod,-8),a.cod) codart ,nvl(b.nume,' '), a.cantitate, " +
                                  " a.um, z.depart, nvl(b.tip_mat, ' '),  nvl(b.spart, ' ') " +
                                  " from sapprd.zcust_det a, sintetice c, articole b,  sapprd.zcust_head z " +
                                  " where a.cod = b.cod(+) and a.id =:idcmd and z.id = a.id  and b.sintetic = c.cod(+)  order by trim(poz) ";





                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idComanda);

                oReader1 = cmd.ExecuteReader();

                ArticolComandaRap articol;
                if (oReader1.HasRows)
                {
                    while (oReader1.Read())
                    {


                        articol = new ArticolComandaRap();
                        articol.status = oReader1.GetString(0);
                        articol.codArticol = oReader1.GetString(1);
                        articol.numeArticol = oReader1.GetString(2);
                        articol.cantitate = oReader1.GetDouble(3);
                        articol.depozit = " ";
                        articol.pretUnit = 0.0;
                        articol.um = oReader1.GetString(4);
                        articol.procent = 0.0;
                        articol.procentFact = 0.0;
                        articol.conditie = false;

                        articol.cmp = 0.0;

                        articol.discClient = 0.0;
                        articol.discountAg = 0.0;
                        articol.discountSd = 0.0;
                        articol.discountDv = 0.0;
                        articol.procAprob = 0.0;
                        articol.permitSubCmp = " ";
                        articol.multiplu = 1;
                        articol.pret = 0.0;
                        articol.infoArticol = " ";
                        articol.cantUmb = oReader1.GetDouble(3).ToString();
                        articol.Umb = oReader1.GetString(4);
                        articol.unitLogAlt = " ";
                        articol.depart = " ";
                        articol.tipArt = "";
                        articol.ponderare = 1;
                        articol.departSintetic = " ";
                        articol.departAprob = " ";
                        articol.istoricPret = " ";
                        articol.vechime = " ";
                        articol.moneda = "RON";
                        articol.valTransport = "0";
                        articol.procTransport = "0";

                        articol.pretMediu = "0.0";
                        articol.adaosMediu = "0.0";
                        articol.unitMasPretMediu = "0.0";
                        articol.coefCorectie = "0.0";
                        articol.dataExp = "00000000";
                        articol.greutate = "0";
                        articol.listCabluri = new JavaScriptSerializer().Serialize(new List<CantCablu>());


                        listArticole.Add(articol);


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

            ArticolComandaAfis articoleComanda = new ArticolComandaAfis();

            articoleComanda.dateLivrare = dateLivrare;
            articoleComanda.articoleComanda = listArticole;
            articoleComanda.conditii = conditii;

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            return serializer.Serialize(articoleComanda);
        }


        public string setCustodieDataLivrare(string idComanda, string dataLivrare)
        {
            string retVal = "0#Operatie reusita.";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                string query = " update sapprd.zcust_head set ketdat=:dataLivrare where id=:idComanda ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":dataLivrare", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = dataLivrare;

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = Int32.Parse(idComanda);

                cmd.ExecuteNonQuery();

               
                SAPWSCustodie.zwbs_custodie webService = null;

                webService = new SAPWSCustodie.zwbs_custodie();
                SAPWSCustodie.ZcustChLivrare inParam = new SAPWSCustodie.ZcustChLivrare();

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
                webService.Credentials = nc;
                webService.Timeout = 300000;

                string nrLivrare = getLivrareCustodie(connection, idComanda);

                inParam.IpVbeln = nrLivrare;
                inParam.IpData = "X";
                inParam.IpMode = "U";

                SAPWSCustodie.ZcustChLivrareResponse response = webService.ZcustChLivrare(inParam);
                retVal = response.EpOk + "#" + response.EpMesaj;

                webService.Dispose();
                

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = "-1#Operatia esuata.";
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }

            return retVal;

        }

        private bool isDataSambata(string dateInput)
        {
            bool isLivrareSambata = false;

            DateTime parsedDate = DateTime.Parse(dateInput);
            isLivrareSambata = parsedDate.DayOfWeek == DayOfWeek.Saturday;

            return isLivrareSambata;
        }

        public string setCustodieAdresaLivrare(string idComanda, string JSONDateLivrare)
        {

            var serializer = new JavaScriptSerializer();
            DateLivrare dateLivrare = serializer.Deserialize<DateLivrare>(JSONDateLivrare);

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;
            string retVal = "0#Operatie reusita.";

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                string query = " update sapprd.zcust_head set adr_livrare=:adrLivrare, city=:city, region=:region, addrnumber=:addrnumber where id=:idComanda ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":adrLivrare", OracleType.NVarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = dateLivrare.Strada;

                cmd.Parameters.Add(":city", OracleType.NVarChar, 75).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = dateLivrare.Oras;

                cmd.Parameters.Add(":region", OracleType.NVarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = dateLivrare.codJudet;

                cmd.Parameters.Add(":addrnumber", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = dateLivrare.addrNumber;

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = Int32.Parse(idComanda);

                cmd.ExecuteNonQuery();

                
                OperatiiAdresa.actualizeazaCoordonateAdresa(connection, idComanda, dateLivrare.coordonateGps);
                OperatiiSuplimentare.actualizeazaTonajComanda(connection, idComanda, dateLivrare.tonaj);

                SAPWSCustodie.zwbs_custodie webService = null;

                webService = new SAPWSCustodie.zwbs_custodie();
                SAPWSCustodie.ZcustChLivrare inParam = new SAPWSCustodie.ZcustChLivrare();

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
                webService.Credentials = nc;
                webService.Timeout = 300000;

                string nrLivrare = getLivrareCustodie(connection, idComanda);

                inParam.IpVbeln = nrLivrare;
                inParam.IpAdresa = "X";
                inParam.IpMode = "U";

                SAPWSCustodie.ZcustChLivrareResponse response = webService.ZcustChLivrare(inParam);
                retVal = response.EpOk + "#" + response.EpMesaj;

                webService.Dispose();
                

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = "-1#Operatia esuata.";
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }


            return retVal;
        }



        public string stergeLivrareCustodie(string idComanda)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;
            string retVal = "0#Operatie reusita.";

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                string query = " update sapprd.zcust_head set status = 6 where id=:idComanda ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idComanda);

                cmd.ExecuteNonQuery();

                
                SAPWSCustodie.zwbs_custodie webService = null;

                webService = new SAPWSCustodie.zwbs_custodie();
                SAPWSCustodie.ZcustChLivrare inParam = new SAPWSCustodie.ZcustChLivrare();

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
                webService.Credentials = nc;
                webService.Timeout = 300000;


                string nrLivrare = getLivrareCustodie(connection, idComanda);

                inParam.IpVbeln = nrLivrare;
                inParam.IpMode = "D";

                SAPWSCustodie.ZcustChLivrareResponse response = webService.ZcustChLivrare(inParam);
                retVal = response.EpOk + "#" + response.EpMesaj;

                webService.Dispose();
                


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = "-1#Operatia esuata.";
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }

            return retVal;

        }



        private string getLivrareCustodie(OracleConnection connection, string idComanda)
        {

            string nrLivrare = "";

            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {

                string query = " select vbeln from sapprd.zcust_head where id=:idComanda ";
                cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idComanda);

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    nrLivrare = oReader.GetString(0);
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


            return nrLivrare;

        }

        public static string getNrComandaSap(string idComanda)
        {
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string nrCmdSap = "-1";

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string sqlString = " select nrcmdsap from sapprd.zcomhead_tableta  where id=:idComanda  ";

                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    nrCmdSap = oReader.GetString(0);
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

            return nrCmdSap;
        }

        public string getAgentComandaInfo(string codClient, string filiala)
        {

            //Ultimul agent care a facut o comanda pentru clientul din filiala 
            string agentInfo = "#";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            DateTime cDate = DateTime.Now.AddMonths(-3);
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            string nowDate = year + month + day;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string sqlString = " select x.nume, x.cod from (select b.nume, b.cod, a.* from sapprd.zcomhead_tableta a, agenti b where " +
                                   " a.cod_client = :codClient and substr(a.ul,0,2) = :unitLog and a.cod_agent = b.cod and a.status = 2 and b.activ = 1 " +
                                   " and a.datac >= :datac order by datac desc) x where rownum < 2  ";


                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                cmd.Parameters.Add(":unitLog", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filiala.Substring(0, 2);

                cmd.Parameters.Add(":datac", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = nowDate;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    agentInfo = oReader.GetString(0) + "#" + oReader.GetString(1);
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


            return agentInfo;

        }



        public string setCmdVanzDataLivrare(string idComanda, string dataLivrare)
        {

            SAPWebServices.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE();
            string retVal = "0#Operatie reusita.";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;

            try
            {

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
                webService.Credentials = nc;
                webService.Timeout = 300000;

                SAPWebServices.ZmodificaLivrare inParam = new ZmodificaLivrare();

                inParam.IpComanda = idComanda;
                inParam.IpDlivr = dataLivrare;

                SAPWebServices.ZmodificaLivrareResponse response = webService.ZmodificaLivrare(inParam);

                retVal = response.EpOk + "#" + response.EpMess;

                webService.Dispose();

                bool isLivrareSambata = isDataSambata(dataLivrare);


                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                string query = "";

                if (isLivrareSambata)
                    query = " update sapprd.zcomhead_tableta set ketdat=:dataLivrare, livr_sambata = ' ' where nrcmdsap=:idComanda ";
                else
                    query = " update sapprd.zcomhead_tableta set ketdat=:dataLivrare where nrcmdsap=:idComanda ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":dataLivrare", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = dataLivrare.Replace("-", "");

                cmd.Parameters.Add(":idComanda", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = idComanda;

                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = "-1#Operatia esuata.";
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }

            return retVal;
        }


        public string actualizeazaComandaSimulata(string idComanda, string totalComanda, string listArticole)
        {

            string retVal = "0";

            List<ArticolComanda> listArtCmd = new JavaScriptSerializer().Deserialize<List<ArticolComanda>>(listArticole);

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " update sapprd.zcomhead_tableta set valoare = :totalCmd, accept2 = ' ', aprob_cv_necesar = ' ', cond_cv = ' ' where mandt='900' and id=:idCmd ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":totalCmd", OracleType.Number, 13).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = totalComanda;

                cmd.Parameters.Add(":idCmd", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = idComanda;

                cmd.ExecuteNonQuery();

                string codArticol;

                foreach (ArticolComanda articol in listArtCmd)
                {
                    cmd.CommandText = " update sapprd.zcomdet_tableta set cantitate=:cant, valoare=:pretUnit, cant_umb=:cantUmb, procent=:procent , " +
                                      " procent_aprob=:procAprob, procent_fc=:procFact, ponderat=:pondere, inf_pret=:infPret, val_poz=:valPoz where id=:idComanda " +
                                      " and cod=:codArt ";

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":cant", OracleType.Number, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = articol.cantitate;

                    cmd.Parameters.Add(":pretUnit", OracleType.Number, 14).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = articol.pretUnit;

                    cmd.Parameters.Add(":cantUmb", OracleType.Number, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = articol.cantUmb;

                    cmd.Parameters.Add(":procent", OracleType.Number, 15).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = articol.procent;

                    cmd.Parameters.Add(":procAprob", OracleType.Number, 15).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = articol.procAprob;

                    cmd.Parameters.Add(":procFact", OracleType.Number, 16).Direction = ParameterDirection.Input;
                    cmd.Parameters[5].Value = articol.procentFact;

                    cmd.Parameters.Add(":pondere", OracleType.NVarChar, 3).Direction = ParameterDirection.Input;
                    cmd.Parameters[6].Value = articol.ponderare;

                    cmd.Parameters.Add(":infPret", OracleType.NVarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[7].Value = articol.infoArticol;

                    cmd.Parameters.Add(":valPoz", OracleType.Number, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[8].Value = articol.pretUnit * Double.Parse(articol.cantUmb);

                    cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[9].Value = idComanda;

                    codArticol = articol.codArticol;

                    if (codArticol.Length == 8)
                        codArticol = "0000000000" + codArticol;
                    cmd.Parameters.Add(":codArt", OracleType.NVarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[10].Value = codArticol;

                    cmd.ExecuteNonQuery();

                }




            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + idComanda + " , " + totalComanda + " , " + listArticole);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return retVal;


        }


        public static string trateazaComenziGED(string comanda, bool alertSD, bool alertDV, bool cmdAngajament, string tipUser, string JSONArt, string JSONComanda, string JSONDateLivrare, string tipUserSap)
        {

            string retVal = "-1";

            var serializer = new JavaScriptSerializer();

            try
            {

                DateLivrare dateLivrareGed, dateLivrareDistrib;
                List<ArticolComanda> articoleGed;

                double totalComandaGed = 0;

                ComandaVanzare comandaVanzare = serializer.Deserialize<ComandaVanzare>(JSONComanda);
                DateLivrare dateLivrare = serializer.Deserialize<DateLivrare>(JSONDateLivrare);
                List<ArticolComanda> articolComanda = serializer.Deserialize<List<ArticolComanda>>(JSONArt);

                bool isComandaCLP = dateLivrare.filialaCLP != null && dateLivrare.filialaCLP.Length == 4;

                if (isComandaCLP || (!isComandaCLP && Utils.isMathausMic(comandaVanzare.filialaAlternativa)) || !tipUserSap.Contains("IP") || comandaVanzare.filialaAlternativa.Equals("BV90"))
                {
                    return new Service1().saveAVNewCmd(comanda, alertSD, alertDV, cmdAngajament, tipUser, JSONArt, JSONComanda, JSONDateLivrare, true, tipUserSap);
                }


                dateLivrareGed = dateLivrare;
                dateLivrareDistrib = dateLivrare;

                comandaVanzare.parrentId = GeneralUtils.getUniqueIdFromCode(dateLivrareDistrib.codAgent);

                articoleGed = new List<ArticolComanda>();

                articoleGed = articolComanda;
                totalComandaGed = Double.Parse(dateLivrare.totalComanda, CultureInfo.InvariantCulture);

                if (!dateLivrareGed.unitLog.Contains("40"))
                    dateLivrareGed.unitLog = dateLivrareGed.unitLog.Substring(0, 2) + "2" + dateLivrareGed.unitLog.Substring(3, 1);

                dateLivrareGed.totalComanda = totalComandaGed.ToString();

                List<ArticolComanda> sortedArticoleDistrib = articoleGed.OrderBy(order => order.filialaSite).ToList();

                if (!isComandaCLP && Utils.isMathausMare(comandaVanzare.filialaAlternativa))
                    sortedArticoleDistrib = articoleGed.OrderBy(order => order.depozit).ToList();

                bool calcTransport = dateLivrareGed.Transport.Equals("TRAP");

                List<ArticolComanda> articoleAgenti = new List<ArticolComanda>();
                double totalComanda = 0;
                string comenziGed = "";
                double valTransp = 0;
                string depozitArt = "";


                foreach (var articol in sortedArticoleDistrib)
                {

                    bool condComandaNoua = !isComandaCLP && Utils.isMathausMare(comandaVanzare.filialaAlternativa) && !depozitArt.Equals(articol.depozit) && articol.depozit.Equals("MAV1");

                    if (condComandaNoua && articoleAgenti.Count > 0)
                    {

                        dateLivrareDistrib.totalComanda = totalComanda.ToString();
                        retVal = new Service1().saveAVNewCmd(comanda, alertSD, alertDV, cmdAngajament, tipUser, serializer.Serialize(articoleAgenti), serializer.Serialize(comandaVanzare), serializer.Serialize(dateLivrareDistrib), calcTransport, tipUserSap);

                        string[] varArrayI = retVal.Split('#');

                        if (varArrayI.Length == 3)
                        {
                            if (comenziGed == String.Empty)
                                comenziGed = varArrayI[2];
                            else
                                comenziGed += "," + varArrayI[2];

                            valTransp += Double.Parse(varArrayI[1]);
                        }

                        articoleAgenti.Clear();
                        totalComanda = 0;
                    }



                    if (!comandaVanzare.nrCmdSap.Equals("-1") || (!comandaVanzare.nrCmdSap.Equals("-1") && comandaVanzare.nrCmdSap.Length < 4))
                    {
                        totalComanda += (articol.pretUnit / articol.multiplu) * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        totalComanda += articol.pretUnit * Double.Parse(articol.cantUmb, CultureInfo.InvariantCulture);
                    }

                    articoleAgenti.Add(articol);
                    depozitArt = articol.depozit;
                }

                dateLivrareDistrib.totalComanda = totalComanda.ToString();
                retVal = new Service1().saveAVNewCmd(comanda, alertSD, alertDV, cmdAngajament, tipUser, serializer.Serialize(articoleAgenti), serializer.Serialize(comandaVanzare), serializer.Serialize(dateLivrareDistrib), calcTransport, tipUserSap);

                string[] varArray = retVal.Split('#');

                if (varArray.Length == 3)
                {
                    if (comenziGed == String.Empty)
                        comenziGed = varArray[2];
                    else
                        comenziGed += "," + varArray[2];

                    valTransp += Double.Parse(varArray[1]);

                }

                if (comenziGed != String.Empty)
                    retVal = "100#" + valTransp + "#" + comenziGed;


                //sf. mathaus

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }


            return retVal;
        }


        public string getOptiuniMasini(string filiala, string camionDescoperit, string macara, string zona, string greutateComanda, string comandaEnergofaga, string comandaExtralungi)
        {

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            bool camionScurt = false;
            bool camionIveco = false;

            try
            {

                cmd.CommandText = " select distinct camion_scurt, camion_iveco from sapprd.ZTIP_CAMION where masina_descoperita = :descoperit and macara = :macara " +
                                  " and zona = :zona and tip_comanda = :tipComanda and :greutate between greutate_min * 1000 and greutate_max * 1000 " +
                                  " and tip_camion in " +
                                  " (select distinct case " +
                                  " when substr(tip, 1, 5) = 'IV_SA' THEN 'IV_SA' WHEN substr(tip, 1, 5) = 'IV_IV' THEN 'IVECO' " +
                                  " WHEN tip = 'DAF_LFD' THEN 'DAF_LF' WHEN tip = 'DAF_SA' THEN 'DAF_CF' ELSE TIP END AS TIP " +
                                  " from sapprd.zszmasini_sort where mandt = '900' and tdlnr = :filiala AND DISPONIBILITATE_MAINE = 'DISPONIBILA' " +
                                  " AND TIP <> 'DAF_R' and erdat || erzet = (select max(erdat || erzet) " +
                                  " from sapprd.zszmasini_sort where mandt = '900' and tdlnr = :filiala " +
                                  " AND DISPONIBILITATE_MAINE = 'DISPONIBILA' AND TIP <> 'DAF_R')) ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":descoperit", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = camionDescoperit.Equals("true") ? "X" : " ";

                cmd.Parameters.Add(":macara", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = macara.Equals("true") ? "X" : " ";

                string zonaPoligon = zona;

                if (zona.ToUpper().Equals("ZM"))
                    zonaPoligon = "METRO";
                else if (zona.ToUpper().Equals("ZMA") || zona.ToUpper().Equals("ZEMA"))
                    zonaPoligon = "EXTRA_A";
                else if (zona.ToUpper().Equals("ZMB") || zona.ToUpper().Equals("ZEMB"))
                    zonaPoligon = "EXTRA_B";

                cmd.Parameters.Add(":zona", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = zonaPoligon;

                string tipComanda = "NORMALA";
                if (comandaExtralungi.Equals("true"))
                    tipComanda = "EXTRALUNGI";
                if (comandaEnergofaga.Equals("true"))
                    tipComanda = "ENERGOFAGA";


                cmd.Parameters.Add(":tipComanda", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = tipComanda;

                cmd.Parameters.Add(":greutate", OracleType.Number, 15).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = Double.Parse(greutateComanda);

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = Utils.getFilialaDistrib(filiala);

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        if (oReader.GetString(0).ToLower().Equals("y") || oReader.GetString(0).ToLower().Equals("n"))
                            camionScurt = true;

                        if (oReader.GetString(1).ToLower().Equals("y") || oReader.GetString(1).ToLower().Equals("n"))
                            camionIveco = true;
                    }
                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }


            string optiuniCamion = camionScurt ? "Camion scurt" : " ";
            optiuniCamion += "#";
            optiuniCamion += camionIveco ? "Camioneta IVECO" : " ";

            if (!camionScurt && !camionIveco)
                optiuniCamion = " ";

            return optiuniCamion;
        }

        public string getProcMarjaComenziIP(string codAgent, string codClient)
        {
            //procent + blocare comanda
            return "10#false";
        }

    }


}