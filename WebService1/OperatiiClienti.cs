using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data;
using WebService1.General;

namespace WebService1
{
    public class OperatiiClienti
    {

        public OperatiiClienti()
        {
        }

        public string getNumeClient(string codClient)
        {
            string numeClient = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

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
                    numeClient = oReader.GetString(0);
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




            return numeClient;
        }



        public string getListClienti(string numeClient, string depart, string departAg, string unitLog, string codUser, string tipUserSap)
        {

            string serializedResult = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string condClient = "";

            if (depart.Equals("040") || depart.Equals("041"))
                depart = "04";

            if (departAg.Equals("040") || departAg.Equals("041"))
                departAg = "04";

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

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

                //pentru DV si SSCN nu trebuie restrictie pe filiala
                if (unitLog.Equals("NN10") || (tipUserSap != null && (tipUserSap.Equals("SSCM") || tipUserSap.Equals("OIVPD"))))
                    exceptieClient = "";



                //Doar clientii definiti pe department, filiala
                if (depart == "11")
                {
                    condClient = " and exists (select 1 from clie_tip t where t.canal = '20'  and t.cod_cli=c.cod " +
                                 " and t.depart='11' )  and exists (select 1 from clie_tip t where t.canal = '20' " +
                                 " and t.cod_cli=c.cod and t.depart='" + departAg + "' ) " +
                                 " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod " +
                                 " and p.vtweg = '20' and p.spart = '11' and p.parvw in ('ZA','ZS') " + exceptieClient + ")";
                }
                else
                {

                    string cond1_Canal = "  t.canal = '10' and ";
                    string cond2_Canal = " and p.vtweg = '10' ";

                    if (tipUserSap != null && tipUserSap.Equals("SSCM"))
                    {
                        cond1_Canal = "";
                        cond2_Canal = "";
                    }

                    condClient = " and exists (select 1 from clie_tip t where  " + cond1_Canal +
                                 " t.cod_cli=c.cod " + condDepart1 + " ) " +
                                 " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod " + cond2_Canal
                                 + condDepart2 + " and p.parvw in ('ZA','ZS') " +
                                   exceptieClient + " ) ";


                    string condExtraClient = " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod  and p.vtweg = '10' " +
                                            " and p.spart = '" + departAg + "' and p.parvw in ('VE', 'ZC')  and p.pernr = '" + codUser + "' ) ";


                    if (codUser != null)
                    {
                        string tipUser = getTipUser(connection, codUser);
                        
                        if (tipUser != "SD" && !tipUserSap.Contains("KA") && tipUserSap != null && !tipUserSap.Equals(Constants.tipSuperAv) && !tipUserSap.Equals(Constants.tipInfoAv) && !tipUserSap.Equals(Constants.tipSMR) && !tipUserSap.Equals(Constants.tipCVR) && !tipUserSap.Equals(Constants.tipSSCM) && !tipUserSap.Equals(Constants.tipCGED) && !tipUserSap.Equals(Constants.tipOIVPD))
                                condClient += condExtraClient;
                    }

                }

                cmd.CommandText = " select x.nume, x.cod, x.tip_pers, x.city1, x.street ||' '|| x.house_num1, x.region " +
                                  " from (select c.nume, c.cod, c.tip_pers, a.city1, a.street, a.house_num1, a.region from clienti c, sapprd.adrc a " +
                                  " where upper(c.nume) like upper('" + numeClient.Replace("'", "") + "%')  " +
                                  " and a.client = '900' and a.addrnumber = " +
                                  " (select k.adrnr from sapprd.kna1 k where k.mandt = '900' and k.kunnr = c.cod) " +
                                  condClient +
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
                        unClient.localitate = oReader.GetString(3);
                        unClient.strada = oReader.GetString(4);
                        unClient.codJudet = oReader.GetString(5);

                        if (tipUserSap != null && tipUserSap.Equals(Constants.tipSuperAv))
                        {
                            unClient.agenti = getAgentiAlocati(connection, depart, unClient.codClient);
                        }
                        else if (tipUserSap != null && (tipUserSap.Equals(Constants.tipInfoAv) || tipUserSap.Equals(Constants.tipSMR) || tipUserSap.Equals(Constants.tipCVR) || tipUserSap.Equals(Constants.tipCGED)))
                        {
                            unClient.agenti = getAgentiAlocati(connection, "00", unClient.codClient);
                        }
                        else if (tipUserSap != null && (tipUserSap.Equals(Constants.tipOIVPD) || tipUserSap.Equals(Constants.tipSSCM)))
                        {
                            unClient.agenti = getUltimAgentComanda(connection, "00", unClient.codClient);
                        }
                        else
                        {
                            unClient.agenti = "@";
                        }

                        unClient.codAgent = "0";
                        unClient.numeAgent = "0";
                        unClient.termenPlata = getTermenPlataClient(connection, unClient.codClient);

                        unClient.clientBlocat = HelperClienti.isClientBlocat(connection, unClient.codClient);
                        unClient.diviziiClient = getDiviziiClient(connection, unClient.codClient, codUser);

                        string tipPlataContract = getTipPlataContract(connection, unClient.codClient);

                        if (tipPlataContract.Trim().Equals(String.Empty))
                        {
                            unClient.tipPlata = " ";
                            unClient.termenPlata = new List<string>() { "C000" };
                        }
                        else
                        {
                            unClient.tipPlata = tipPlataContract;
                        }

                        if (unClient.agenti.Contains("@"))
                            listaClienti.Add(unClient);

                    }

                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaClienti);

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " : \n\n " +  numeClient + "\n\n" + depart + "\n\n" + departAg + "\n\n" + unitLog + "\n\n" + codUser + "\n\n" + tipUserSap);
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }


            return serializedResult;
        }


        private string getUltimAgentComanda(OracleConnection connection, string codDepart, string codClient)
        {
            string agenti = "";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            DateTime localDate = DateTime.Now.AddYears(-1);
            string selectedDate = localDate.Year.ToString() + localDate.Month.ToString("00") + localDate.Day.ToString("00");

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select x.pernr, x.nume from ( " +
                                  " select distinct pernr, nume, max(tb.datac) datacom from sapprd.knvp y, agenti ag, sapprd.zcomhead_tableta tb where y.mandt = '900' " +
                                  " and y.kunnr = :codClient and y.parvw in ('VE','ZC') " +
                                  " and tb.mandt = '900' and tb.cod_client = y.kunnr and tb.datac >= :dataCom and tb.cod_agent = pernr " +
                                  " and ag.cod = pernr and ag.activ = 1 group by pernr, nume order by datacom desc) x where rownum = 1 ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                cmd.Parameters.Add(":dataCom", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = selectedDate;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    agenti = oReader.GetString(0) + "#" + oReader.GetString(1) + "@";
                }
                else
                    agenti = getAgentiAlocati(connection, codDepart, codClient);

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return agenti;
        }


        private string getAgentiAlocati(OracleConnection connection, string codDepart, string codClient)
        {

            string agenti = "";
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string condDepart = " and vtweg = '10' and spart = '" + codDepart + "' ";

                if (codDepart.Equals("00"))
                    condDepart = "";

                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct pernr, nume from sapprd.knvp y, agenti ag where y.mandt = '900' and y.kunnr = '" + codClient +
                                  "' and y.parvw in ('VE','ZC') " + condDepart + " and ag.cod = pernr and ag.activ = 1 order by nume ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        agenti += oReader.GetString(0) + "#" + oReader.GetString(1) + "@";
                    }
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

            return agenti;


        }




        public string getListMeseriasi(string numeClient, string unitLog)
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

                cmd.CommandText = " select x.nume, x.cod, x.tip_pers from (select c.nume, c.cod, c.tip_pers from websap.clienti c  where upper(c.nume) " +
                                  " like upper('" + numeClient.Replace("'", "") + "%') " +
                                  " and exists (select 1 from websap.clie_tip t where t.canal = '20'  and t.cod_cli=c.cod and tip in ('12','13','14','15','16','17') " +
                                  " ) and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod  and p.vtweg = '20' " +
                                  " and p.parvw in ('ZA','ZS')  and p.kunn2 ='" + unitLog + "' )  ) x  where rownum<=50 order by x.nume ";


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

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaClienti);

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

        public string getDetaliiClient(string codClient, string depart, string codUser)
        {

            string retVal = "-1";

            string serializedResult = "";
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string adresa = "-";
            string exista = "";
            float limCredit = 0;
            float restCredit = 0;
            string condClient = "", condClient1 = "", condClient2 = "";

            if (depart.Equals("040") || depart.Equals("041"))
                depart = "04";

            try
            {

                DetaliiClient detaliiClient = new DetaliiClient();

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

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

                    detaliiClient.oras = oras;
                    detaliiClient.strada = strada;
                    detaliiClient.nrStrada = nrStr;
                    detaliiClient.regiune = regiune;


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
                      " where s1.mandt='900' and s1.kkber='1000' and spmon=:l  and s1.knkli=k.kunnr),0) oeikw,  k.crblb, " +
                      " nvl((select '1' from sapprd.lfa1 l where l.mandt = '900' and l.kunnr = k.kunnr),'-1') isFurnizor " +
                      " from sapprd.knkk k " +
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
                        detaliiClient.isFurnizor = oReader.GetString(7).Equals("-1") ? false.ToString() : true.ToString();

                        oReader.Close();
                        oReader.Dispose();
                    }
                    else
                    {
                        limCredit = 0;
                        restCredit = 0;
                    }

                    detaliiClient.limitaCredit = limCredit;
                    detaliiClient.restCredit = restCredit;
                    detaliiClient.stare = clBlocat;

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

                    detaliiClient.persContact = nume;
                    detaliiClient.telefon = tel;
                    detaliiClient.tipClient = exista;

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

                    detaliiClient.cursValutar = cursValut;

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

                    detaliiClient.motivBlocare = strBlocare;

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

                    detaliiClient.filiala = filClnt;

                    retVal += filClnt + "#";

                    oReader.Close();
                    oReader.Dispose();


                    //termen de plata
                    string termPlata = " ";
                    cmd = connection.CreateCommand();

                    cmd.CommandText = " select u.zterm from sapprd.T052u u, sapprd.TVZBT t where u.mandt = '900' and " +
                                      " u.spras = '4' and u.mandt = t.mandt and u.spras = t.spras and u.zterm = t.zterm " +
                                      " and u.zterm <= (select max(p.zterm) from sapprd.knvv p where p.mandt = '900' " +
                                      " and p.kunnr =:k " + condClient2 + " ) and u.zterm like 'C%' order by u.zterm ";



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

                    detaliiClient.termenPlata = termPlata;

                    retVal += termPlata + "#";

                    if (codUser != null)
                        detaliiClient.divizii = getDiviziiClient(connection, codClient, codUser);
                    else
                        detaliiClient.divizii = "";

                    //contract activ
                    //nu se mai verifica din 13.04.2022
                    bool contractActiv = true;


                    if (!contractActiv)
                    {
                        detaliiClient.tipPlata = "";
                        detaliiClient.termenPlata = "C000";
                    }
                    else {

                        //limita de credit definita si tip plata contract
                        cmd = connection.CreateCommand();

                        cmd.CommandText = " select k.klimk, b.zwels from sapprd.knkk k, sapprd.knb1 b where k.mandt='900' and b.mandt = k.mandt " +
                                          " and k.kunnr = :codClient and b.kunnr = k.kunnr ";

                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = codClient;

                        oReader = cmd.ExecuteReader();
                        if (oReader.HasRows)
                        {
                            oReader.Read();

                            if (oReader.GetDouble(0) > 1)
                            {
                                if (oReader.GetString(1) != null && oReader.GetString(1).Trim() != "")
                                    detaliiClient.tipPlata = oReader.GetString(1);
                                else
                                    detaliiClient.tipPlata = "O";
                            }
                            else
                                detaliiClient.tipPlata = "";
                        }
                        else {
                            detaliiClient.tipPlata = " ";
                            detaliiClient.termenPlata = "C000";
                        }


                    }

                    detaliiClient.email = " ";
                    string cuiClient = " ";

                    cmd = connection.CreateCommand();

                    cmd.CommandText = " select nvl(adrs_mail,' '), nvl(cui,' ') from clienti where cod = :codClient ";

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        detaliiClient.email = oReader.GetString(0);
                        cuiClient = oReader.GetString(1);
                    }

                    oReader.Close();
                    oReader.Dispose();

                    detaliiClient.errMsg = "";

                    if (cuiClient.Trim().Length > 0)
                    {
                        string stareClient = new VerificaTva().isPlatitorTva(cuiClient, codUser);
                        PlatitorTvaResponse starePlatitor = serializer.Deserialize<PlatitorTvaResponse>(stareClient);
                        if (starePlatitor.stareInregistrare != null && starePlatitor.stareInregistrare.ToLower().Contains("radiere"))
                            detaliiClient.errMsg = "Acest client este radiat.";

                    }

                }

                
                serializedResult = serializer.Serialize(detaliiClient);


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = null;
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }


            return serializedResult;

        }


        public static string getDiviziiClientCUI(OracleConnection connection, string codCui, string codAgent)
        {

            string diviziiClient = "";

            if (codAgent == null || codAgent.Trim().Equals(String.Empty))
                return diviziiClient;

            OracleDataReader oReader = null;

            try
            {
                OracleCommand cmd = null;

                cmd = connection.CreateCommand();

                string tipUser = getTipUser(connection, codAgent);
                string condAgent = " and p.pernr =:codAgent ";

                if (tipUser.Equals("SD"))
                    condAgent = " ";


                cmd.CommandText = " select distinct spart from sapprd.knvp p where p.mandt = '900' and p.kunnr = " +
                                  " (select k.cod from clienti k where k.tip2 in ('1000', 'OCAV', 'OCAZ') and k.tip_pers='PJ'  " +
                                  " and k.cui = TRANSLATE(:codCui, '0' || TRANSLATE(:codCui, '.0123456789', '.'), '0') and rownum = 1 )" +
                                    condAgent + " and p.vtweg = '10' and p.parvw in ('VE','ZC') order by spart ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codCui", OracleType.VarChar, 180).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codCui;

                if (!tipUser.Equals("SD"))
                {
                    cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codAgent;
                }

                oReader = cmd.ExecuteReader();
                string divizie = "";

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        divizie = oReader.GetString(0).ToString();

                        if (divizie.Equals("04"))
                            divizie = "040;041";

                        diviziiClient += divizie + ";";
                    }
                }


                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + codCui + " , " + codAgent);
            }

            return diviziiClient;


        }

        public static string getDiviziiClient(OracleConnection connection, string codClient, string codAgent)
        {

            string diviziiClient = "";

            if (codAgent == null || codAgent.Trim().Equals(""))
                return "";

            OracleDataReader oReader = null;

            try
            {
                OracleCommand cmd = null;

                cmd = connection.CreateCommand();

                string tipUser = getTipUser(connection, codAgent);
                string condAgent = " and p.pernr =:codAgent ";

                if (tipUser.Equals("SD"))
                    condAgent = " ";

                cmd.CommandText = " select distinct spart from sapprd.knvp p where p.mandt = '900' and p.kunnr =:codClient " +
                                  condAgent + " and p.vtweg = '10' and p.parvw in ('VE','ZC') order by spart ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();


                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                if (!tipUser.Equals("SD"))
                {
                    cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codAgent;
                }

                oReader = cmd.ExecuteReader();
                string divizie = "";

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        divizie = oReader.GetString(0).ToString();

                        if (divizie.Equals("04"))
                            divizie = "040;041";

                        diviziiClient += divizie + ";";
                    }
                }

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

            return diviziiClient;


        }


        public static string getDiviziiClient(OracleConnection connection, string idComanda)
        {

            string diviziiClient = "";

            OracleDataReader oReader = null;

            try
            {
                OracleCommand cmd = null;

                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct spart from sapprd.knvp p, sapprd.zcomhead_tableta t where p.mandt = '900' and t.mandt = '900' " +
                                  " and t.id =:idComanda and p.kunnr = t.cod_client " +
                                  " and p.pernr = t.cod_agent and p.vtweg = '10' and p.parvw in ('VE','ZC') ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();


                cmd.Parameters.Add(":idComanda", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idComanda);

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        diviziiClient += oReader.GetString(0).ToString() + ";";
                    }
                }

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

            return diviziiClient;


        }




        public string getMeseriasi(string codFiliala)
        {
            return getListMeseriasi("", codFiliala);

        }


        public string getMeseriasi(string codFiliala, string tipUser, string codUser, string codDepart)
        {
            string serializedResult = "";

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Client> listaClienti = new List<Client>();

            if (tipUser.Equals("AV") && !codDepart.Equals("08") && !codDepart.Equals("09"))
            {
                return serializer.Serialize(listaClienti);
            }

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string sqlString;
                string tipConsilier = "";

                if (tipUser.Equals("AV"))
                    sqlString = " select c.name1 nume, c.kunnr cod  from sapprd.kna1 c  where c.ktokd = '1180' and c.sperr != 'X' and " +
                                " exists(select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.kunnr  and p.vtweg = '20' " +
                                " and  p.parvw in ('ZA', 'ZS') and p.kunn2 =:filiala )  and exists " +
                                " (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.kunnr  and p.vtweg = '20' and p.parvw in ('ZC') and p.pernr =:codAgent ) " +
                                " and exists (select 1 from sapprd.ZTB_COD_MESERIAS s where s.cod_meserias = c.kunnr) order by c.name1 ";

                else {
                        sqlString = " select c.name1 nume, c.kunnr cod from sapprd.kna1 c where c.ktokd = '1180' and c.sperr != 'X' " +
                                    " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.kunnr and p.vtweg = '20' " +
                                    " and p.parvw in ('ZA', 'ZS') and p.kunn2 = :filiala ) " +
                                    " and (exists(select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.kunnr and p.vtweg = '20' " +
                                    " and p.parvw in ('ZC', 'VE') and p.pernr = :codAgent) " +
                                    " or exists (select 1 from websap.agenti a where a.tip in ('SMR', 'CVR', 'CVS') and a.cod = :codAgent )) " +
                                    " order by c.name1 ";
                }



                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codFiliala;

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codUser;

                oReader = cmd.ExecuteReader();
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        Client unClient = new Client();
                        unClient.numeClient = oReader.GetString(0);
                        unClient.codClient = oReader.GetString(1);
                        unClient.tipClient = " ";

                        listaClienti.Add(unClient);
                    }
                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " +  codFiliala+ " , " + tipUser + " , " + codUser + " , " + codDepart);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }


           
            serializedResult = serializer.Serialize(listaClienti);

            return serializedResult;
        }

        public string getDatePersonaleClient(string numeClient, string tipClient, string codAgent)
        {
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<BeanDatePersonale> listDatePersonale = new List<BeanDatePersonale>();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string sqlString;

                if (tipClient.Equals("PF"))
                    sqlString = " select name1, stceg, regio, city1, street||' '||nr from sapprd.zinformclmag where mandt='900' " +
                                       " and lower(name1) like lower('" + numeClient.Trim() + "%') and tip_cl ='PF' order by name1 ";
                else
                    sqlString = " select z.numefirma, z.cui, z.judet, z.localitate, z.adresa||' '||z.nr, " +
                                " nvl((select k.cod from clienti k where k.tip2 in ('1000', 'OCAV', 'OCAZ') and k.tip_pers='PJ' " +
                                " and k.cui = TRANSLATE(z.cui, '0' || TRANSLATE(z.cui, '.0123456789', '.'), '0')),'-1') codclient " +
                                " from sapprd.zverifcui z where z.mandt='900' and " +
                                " lower(z.numefirma) like lower('" + numeClient.Trim() + "%') and rownum <= 30 order by z.numefirma ";

                cmd.CommandText = sqlString;

                oReader = cmd.ExecuteReader();
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        BeanDatePersonale datePersonale = new BeanDatePersonale();

                        datePersonale.nume = oReader.GetString(0);
                        datePersonale.cnp = oReader.GetString(1);
                        datePersonale.codjudet = oReader.GetString(2);
                        datePersonale.localitate = oReader.GetString(3);
                        datePersonale.strada = oReader.GetString(4);
                        datePersonale.divizii = "";

                        if (tipClient.Equals("PF"))
                        {
                            datePersonale.clientBlocat = false;
                            datePersonale.tipPlata = " ";
                            datePersonale.termenPlata = new List<string>() { "C000" };
                            datePersonale.cnp = "";
                        }
                        else {

                            datePersonale.termenPlata = getTermenPlataClient(connection, datePersonale.cnp);
                            datePersonale.clientBlocat = HelperClienti.isClientBlocat(connection, datePersonale.cnp);
                            datePersonale.codClient = oReader.GetString(oReader.GetOrdinal("codClient"));

                            string tipPlataContract = getTipPlataContract(connection, datePersonale.cnp);

                            if (tipPlataContract.Trim().Equals(String.Empty))
                            {
                                datePersonale.tipPlata = " ";
                                datePersonale.termenPlata = new List<string>() { "C000" };
                            }
                            else
                            {
                                datePersonale.tipPlata = tipPlataContract;
                            }

                            if (codAgent != null && datePersonale.codClient != null && !datePersonale.codClient.Equals("-1"))
                                datePersonale.divizii = getDiviziiClient(connection, datePersonale.codClient, codAgent);

                        }

                        listDatePersonale.Add(datePersonale);

                    }
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " : " + numeClient + " , " +  tipClient);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return new JavaScriptSerializer().Serialize(listDatePersonale);

        }


        private string getTipPlataContract(OracleConnection connection, string codClient)
        {

            OracleDataReader oReader = null;
            OracleCommand cmd = new OracleCommand();
            string tipPlata = "";

            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select k.klimk, b.zwels from sapprd.knkk k, sapprd.knb1 b, sapprd.kna1 c " +
                                  " where k.mandt = '900' and b.mandt = '900' and c.mandt = '900' and c.kunnr = k.kunnr " +
                                  " and k.kunnr = :codClient and b.kunnr = k.kunnr " +
                                  " and c.ZDATAEXPCTR != '00000000' and to_date(c.ZDATAEXPCTR, 'yyyymmdd') >= sysdate";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();
                if (oReader.HasRows)
                {
                    oReader.Read();

                    if (oReader.GetDouble(0) > 1)
                    {
                        if (oReader.GetString(1) != null && oReader.GetString(1).Trim() != "")
                            tipPlata = oReader.GetString(1);
                        else
                            tipPlata = "O";
                    }

                }
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + codClient);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return tipPlata;
            
        }


        private string getTipPlataContractIP18(OracleConnection connection, string codClient)
        {

            OracleDataReader oReader = null;
            OracleCommand cmd = new OracleCommand();
            string tipPlata = "";

            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select k.klimk, b.zwels from sapprd.knkk k, sapprd.knb1 b, sapprd.kna1 c " +
                                  " where k.mandt = '900' and b.mandt = '900' and c.mandt = '900' and c.kunnr = k.kunnr " +
                                  " and k.kunnr = :codClient and b.kunnr = k.kunnr " +
                                  " and (exists (select 1 from sapprd.knvv v where v.mandt = '900' and v.kunnr = b.kunnr and v.vtweg = '20' and v.spart = '11' and v.kdgrp = '18') " +
                                  " or ( c.ZDATAEXPCTR != '00000000' and to_date(c.ZDATAEXPCTR, 'yyyymmdd') >= sysdate )) ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();
                if (oReader.HasRows)
                {
                    oReader.Read();

                    if (oReader.GetDouble(0) > 1)
                    {
                        if (oReader.GetString(1) != null && oReader.GetString(1).Trim() != "")
                            tipPlata = oReader.GetString(1);
                        else
                            tipPlata = "O";
                    }

                }
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + codClient);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return tipPlata;
        }

        private string getTipPlataContractIP19(OracleConnection connection, string codClient)
        {

            OracleDataReader oReader = null;
            OracleCommand cmd = new OracleCommand();
            string tipPlata = "";

            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select k.klimk, b.zwels from sapprd.knkk k, sapprd.knb1 b, sapprd.kna1 c " +
                                  " where k.mandt = '900' and b.mandt = '900' and c.mandt = '900' and c.kunnr = k.kunnr " +
                                  " and k.kunnr = :codClient and b.kunnr = k.kunnr " +
                                  " and(exists(select * from sapprd.knvv v where v.mandt = '900' and v.kunnr = b.kunnr and v.vtweg = '20' and v.spart = '11' and v.kdgrp = '19') " +
                                  " or(c.ZDATAEXPCTR != '00000000' and to_date(c.ZDATAEXPCTR, 'yyyymmdd') >= sysdate)) ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();
                if (oReader.HasRows)
                {
                    oReader.Read();

                    if (oReader.GetDouble(0) > 1)
                    {
                        if (oReader.GetString(1) != null && oReader.GetString(1).Trim() != "")
                            tipPlata = oReader.GetString(1);
                        else
                            tipPlata = "O";
                    }

                }
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + codClient);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return tipPlata;
        }

        public static string getTipUser(OracleConnection connection, string codAgent)
        {
            string tipAgent = "NN";

            if (codAgent == null)
                return "NN";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select upper(substr(tip,0,2)) from agenti where cod =:codAgent ";

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

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }


            return tipAgent;
        }


        public string getListClientiInstPublice(string numeClient, string unitLog, string tipUser, string tipClient)
        {

            

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            List<Client> listClienti = new List<Client>();

            try
            {

                int i = 0;
                bool isCUI = int.TryParse(numeClient, out i);

                string restrClient = " and lower(c.nume) like lower('" + numeClient + "%') ";

                if (isCUI)
                    restrClient = " and (k.stceg like '" + numeClient + "%' or upper(k.stceg) like 'RO" + numeClient + "%')";
                 

                string tipClientIP = "18";

                if (tipClient != null && tipClient.Equals("NONCONSTR"))
                    tipClientIP = "19";

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct c.nume, c.cod, c.tip_pers, k.stceg, v.kdgrp, p.kunn2 from websap.clienti c,  sapprd.knvv v, sapprd.knvp p, sapprd.kna1 k " +
                                  " where c.cod = v.kunnr and v.mandt = '900' and v.vtweg = '20' " +
                                  " and v.spart = '11' and v.kdgrp=:tipClient and v.mandt = p.mandt " +
                                  " and v.kunnr = p.kunnr and v.vtweg = p.vtweg and v.spart = p.spart and k.mandt='900' and k.kunnr = c.cod " +
                                  " and p.parvw in ('ZA','ZS')  " + restrClient + " order by nume ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":tipClient", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = tipClientIP;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ClientIP unClient = new ClientIP();
                        unClient.numeClient = oReader.GetString(0) + " (" + oReader.GetString(5) + ")";
                        unClient.codClient = oReader.GetString(1);
                        unClient.tipClient = oReader.GetString(2);
                        unClient.codCUI = oReader.GetString(3);
                        unClient.tipClientIP = oReader.GetString(4);
                        unClient.filiala = oReader.GetString(5);

                        unClient.termenPlata = getTermenPlataInstPublic(connection, unClient.codClient);

                        
                        unClient.clientBlocat = HelperClienti.isClientBlocat(connection, unClient.codClient);

                        string tipPlataContract ;

                        if (tipClientIP.Equals("18"))
                            tipPlataContract = getTipPlataContractIP18(connection, unClient.codClient);
                        else
                            tipPlataContract = getTipPlataContractIP19(connection, unClient.codClient);

                        if (tipPlataContract.Trim().Equals(String.Empty))
                        {
                            unClient.tipPlata = " ";
                            unClient.termenPlata = new List<string>() { "C000" };
                        }
                        else
                        {
                            unClient.tipPlata = tipPlataContract;
                        }
                        

                        listClienti.Add(unClient);
                    }
                }

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

            return new JavaScriptSerializer().Serialize(listClienti);


        }


        private List<string> getTermenPlataInstPublic(OracleConnection connection, string codClient)
        {

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<string> termenPlata = new List<string>();
            termenPlata.Add("C030");

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select u.zterm from sapprd.T052u u, sapprd.TVZBT t where u.mandt = '900' and  u.spras = '4' " +
                                  " and u.mandt = t.mandt and u.spras = t.spras and u.zterm = t.zterm  and u.zterm " +
                                  " <= (select max(p.zterm) from sapprd.knvv p where p.mandt = '900' " +
                                  " and p.kunnr = :codClient and p.vtweg = '20'  and p.spart = '11' ) and u.zterm like 'C%' order by u.zterm ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    termenPlata.Clear();
                    while (oReader.Read())
                    {
                        termenPlata.Add(oReader.GetString(0));
                    }
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

            return termenPlata;


        }


        private List<string> getTermenPlataClient(OracleConnection connection, string codClient)
        {

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<string> termenPlata = new List<string>();
            termenPlata.Add("C010");

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select u.zterm from sapprd.T052u u, sapprd.TVZBT t where u.mandt = '900' and  u.spras = '4' " +
                                  " and u.mandt = t.mandt and u.spras = t.spras and u.zterm = t.zterm  and u.zterm " +
                                  " <= (select max(p.zterm) from sapprd.knvv p where p.mandt = '900' " +
                                  " and p.kunnr = :codClient and p.vtweg = '10' ) and upper(u.zterm) like 'C%' order by u.zterm ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    termenPlata.Clear();
                    while (oReader.Read())
                    {
                        termenPlata.Add(oReader.GetString(0));
                    }
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


            return termenPlata;


        }


        public string getTermenPlataClientDepart(string codAgent, string codClient)
        {
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<string> termenPlata = new List<string>();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct u.zterm from sapprd.T052u u, sapprd.TVZBT t where u.mandt = '900' and  u.spras = '4' " +
                                  " and u.mandt = t.mandt and u.spras = t.spras and u.zterm = t.zterm  and u.zterm " +
                                  " <= (select max(p.zterm) from sapprd.knvv p where p.mandt = '900' " +
                                  " and p.kunnr = :codClient and p.vtweg = '20' and p.spart = (select divizie from agenti where activ = 1 and cod=:codAgent)) " +
                                  " and u.zterm != 'C000' and u.zterm like 'C%' order by u.zterm ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAgent;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    termenPlata.Clear();
                    while (oReader.Read())
                    {
                        termenPlata.Add(oReader.GetString(0));
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


            return new JavaScriptSerializer().Serialize(termenPlata);


        }


        public static string getAgentAlocat(OracleConnection connection, OracleTransaction transaction, string codDepart, string codClient, string codAgentInit)
        {

            string codAgent = "";
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            if (codDepart.Equals("11"))
                return codAgentInit;


            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select p.pernr from sapprd.knvp p where p.mandt = '900' and p.kunnr =:codClient  and " +
                                  " p.vtweg = '10'  and substr(p.spart,0,2) =:codDepart and p.parvw in ('VE', 'ZC') ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                cmd.Parameters.Add(":codDepart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codDepart.Substring(0, 2);

                cmd.Transaction = transaction;
                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    {
                        codAgent = oReader.GetString(0);
                    }
                }
                else
                    codAgent = codAgentInit;

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return codAgent;

        }


        public string getClientiAlocati(string codAgent, string filiala)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<ClientAlocat> listClientiAlocati = new List<ClientAlocat>();

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            try
            {

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select c.nume, " +
                                  " nvl(max(decode(t.depart, '01', t.tip)), ' ') d01, nvl(max(decode(t.depart, '02', t.tip)), ' ') d02, " +
                                  " nvl(max(decode(t.depart, '03', t.tip)), ' ') d03, nvl(max(decode(t.depart, '04', t.tip)), ' ') d04, " +
                                  " nvl(max(decode(t.depart, '05', t.tip)), ' ') d05, nvl(max(decode(t.depart, '06', t.tip)), ' ') d06, " +
                                  " nvl(max(decode(t.depart, '07', t.tip)), ' ') d07, nvl(max(decode(t.depart, '08', t.tip)), ' ') d08, " +
                                  " nvl(max(decode(t.depart, '09', t.tip)), ' ') d09 " +
                                  " from clie_tip t, clienti c where t.cod_cli = c.cod and t.canal = '10' " +
                                  " and exists (select 1 from clie_tip t where t.canal = '10'  and t.cod_cli = c.cod  ) " +
                                  " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod  and p.vtweg = '10' " +
                                  " and p.parvw in ('ZA', 'ZS')  and p.kunn2 =:filiala )  and exists (select 1 from sapprd.knvp p where p.mandt = '900' " +
                                  " and p.kunnr = c.cod  and p.vtweg = '10'  and p.parvw in ('VE', 'ZC')  and p.pernr =:codAgent ) group by c.nume order by c.nume ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAgent;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ClientAlocat client = new ClientAlocat();

                        client.numeClient = oReader.GetString(0);
                        client.tipClient01 = getTipClient(oReader.GetString(1));
                        client.tipClient02 = getTipClient(oReader.GetString(2));
                        client.tipClient03 = getTipClient(oReader.GetString(3));
                        client.tipClient04 = getTipClient(oReader.GetString(4));
                        client.tipClient05 = getTipClient(oReader.GetString(5));
                        client.tipClient06 = getTipClient(oReader.GetString(6));
                        client.tipClient07 = getTipClient(oReader.GetString(7));
                        client.tipClient08 = getTipClient(oReader.GetString(8));
                        client.tipClient09 = getTipClient(oReader.GetString(9));
                        listClientiAlocati.Add(client);
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

            return new JavaScriptSerializer().Serialize(listClientiAlocati);


        }

        private static string getTipClient(string codTip)
        {
            string tipClient = codTip;

            if (codTip.Equals("01"))
                tipClient = "Final";

            else if (codTip.Equals("02"))
                tipClient = "Constr. general";

            else if (codTip.Equals("03"))
                tipClient = "Constr. special";

            else if (codTip.Equals("04"))
                tipClient = "Revanzator";

            else if (codTip.Equals("05"))
                tipClient = "Prod. mobila";

            else if (codTip.Equals("06"))
                tipClient = "Debit. mat. lemn.";

            else if (codTip.Equals("07"))
                tipClient = "Fraudator";

            else if (codTip.Equals("08"))
                tipClient = "Nespecificat";

            else if (codTip.Equals("09"))
                tipClient = "Extern UE";

            else if (codTip.Equals("10"))
                tipClient = "Extern non-UE";

            else if (codTip.Equals("11"))
                tipClient = "Desfiintat";

            return tipClient;
        }


        public static string getCodClient(OracleConnection connection, string numeClient)
        {
            string codClient = "";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select cod from clienti where upper(trim(nume)) =:numeClient ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":numeClient", OracleType.VarChar, 105).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = numeClient.ToUpper().Trim();

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    codClient = oReader.GetString(0);
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

            return codClient;
        }

        public string getInfoCreditClient(string codClient)
        {

            CreditClient creditClient = new CreditClient();
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();
                string nowDate = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00");

                cmd = connection.CreateCommand();
                cmd.CommandText = " select distinct k.kunnr,k.klimk lcredit, " +
                  " k.skfor,k.ssobl, nvl((select s2.olikw+s2.ofakw from sapprd.s067 s2 where s2.mandt='900' and " +
                  " s2.kkber='1000' and s2.knkli=k.kunnr),0) olikw, nvl((select sum(s1.oeikw) from sapprd.s066 s1 " +
                  " where s1.mandt='900' and s1.kkber='1000' and spmon=:l  and s1.knkli=k.kunnr),0) oeikw,  k.crblb " +
                  " from sapprd.knkk k " +
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
                    creditClient.limitaCredit = oReader.GetFloat(1);
                    creditClient.restCredit = Math.Round(oReader.GetFloat(1) - (oReader.GetFloat(2) + oReader.GetFloat(3)) - (oReader.GetFloat(4) + oReader.GetFloat(5)), 2);
                    creditClient.isBlocat = oReader.GetString(6).Equals("X");
                }


                if (creditClient.isBlocat)
                {
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
                        creditClient.motivBlocat = oReader.GetString(1).ToString();
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

            return new JavaScriptSerializer().Serialize(creditClient);

        }


        public static string getCodCategorieClient(string numeCategorie)
        {
            string codCategorie = "00";

            if (numeCategorie.Equals("Client final"))
                codCategorie = "01";
            else if (numeCategorie.Equals("Constructor general"))
                codCategorie = "02";
            else if (numeCategorie.Equals("Constructor special"))
                codCategorie = "03";
            else if (numeCategorie.Equals("Revanzator"))
                codCategorie = "04";
            else if (numeCategorie.Equals("Producator mobila"))
                codCategorie = "05";
            else if (numeCategorie.Equals("Debitor materiale lemnoase"))
                codCategorie = "06";

            return codCategorie;
        }


    }
}