using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
using System.Data;
using WebService1.SAPWebServices;
using WebService1.General;
using System.Globalization;

namespace WebService1
{
    public class Preturi
    {

        public string getPret(string client, string articol, string cantitate, string depart, string um, string ul, string tipUser, string depoz, string codUser, string canalDistrib, string filialaAlternativa, string filialaClp, string tipTransport)
        {
            string retVal = "";
            SAPWebServices.ZTBL_WEBSERVICE webService = null;

            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string month = cDate.Month.ToString("00");
            string day = cDate.Day.ToString("00");
            string nowDate = year + month + day;


            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string tipUserLocal;

            if (tipUser == null || (tipUser != null && tipUser.Trim().Length == 0))
            {
                tipUserLocal = Service1.getTipUser(codUser);
            }
            else
            {
                tipUserLocal = tipUser;
            }


            try
            {
                if (depoz != null && (depoz.Equals("040V1") || depoz.Equals("041V1")))
                    depoz = "04V1";

                webService = new ZTBL_WEBSERVICE();
                SAPWebServices.ZgetPrice inParam = new SAPWebServices.ZgetPrice();

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(DatabaseConnections.getUser(), DatabaseConnections.getPass());

                webService.Credentials = nc;
                webService.Timeout = 300000;

                inParam.GvKunnr = client;
                inParam.GvMatnr = articol;
                inParam.GvSpart = depart.Substring(0, 2);
                inParam.GvVrkme = um;
                inParam.GvWerks = ul;
                inParam.GvLgort = depoz;
                inParam.GvCant = Decimal.Parse(cantitate);
                inParam.GvCantSpecified = true;
                inParam.GvSite = " ";
                inParam.TipPers = tipUserLocal;
                inParam.Canal = canalDistrib;
                inParam.UlStoc = filialaAlternativa.Equals("BV90") ? "BV90" : filialaClp != null ? filialaClp : " ";
                inParam.Traty = tipTransport != null ? tipTransport : " ";
                inParam.CuRotunj = Utils.isUserTest(codUser, Constants.USER_TEST_1) ? "X" : " ";

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
                string impachetare = outParam.Impachet.ToString() != "" ? outParam.Impachet.ToString() : " ";
                string pretGed = outParam.GvNetwrFtva.ToString();
                string greutateArt = outParam.GvBrgew.ToString();
                string dataExp = outParam.GvDatbi;

                string greutateBruta = outParam.GvBrgewMatnr.ToString();

                string um50 = outParam.GvUm50;
                string cantitate50 = outParam.GvQty50.ToString();


                string extindere11 = outParam.ErrorCode.ToString();


                if (depart.Equals("11") && extindere11.Equals("1"))
                {
                    if (Service1.extindeClient(client).Equals("0"))
                    {
                        return getPret(client, articol, cantitate, depart, um, ul, tipUserLocal, depoz, codUser, canalDistrib, filialaAlternativa, filialaClp, tipTransport);
                    }
                    else
                    {
                        return "Eroare extindere client.";
                    }
                }


                //---verificare cmp



                string filialaCmp = filialaAlternativa;

                if (depart.Equals("11"))
                {
                    if (filialaAlternativa.Equals("BV90"))
                        filialaCmp = "BV92";
                    else
                        filialaCmp = filialaAlternativa.Substring(0, 2) + "2" + filialaAlternativa.Substring(3, 1);
                }

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();


                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select nvl(to_char(decode(y.lbkum,0,y.verpr,y.salk3/y.lbkum),'999999.9999'),0) from sapprd.mbew y where " +
                                  " y.mandt='900' and y.matnr=:matnr  and y.bwkey = :unitLog ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":matnr", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = articol;

                cmd.Parameters.Add(":unitLog", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filialaCmp;



                oReader = cmd.ExecuteReader();
                double cmpArticol = 0;
                double procRedCmp = 0;
                if (oReader.HasRows)
                {
                    oReader.Read();
                    cmpArticol = Convert.ToDouble(oReader.GetString(0));
                    procRedCmp = getProcReducereCmp(connection, articol);

                    cmpArticol = cmpArticol * (100 - procRedCmp) / 100;
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

                string istoricPret = " ";

                if (canalDistrib.Equals("10"))
                    istoricPret = getIstoricPret(connection, articol, client);

                string articoleRecom = new OperatiiArticole().getArticoleRecomandate(connection, articol, depart);

                ArticolProps articolProps = new OperatiiArticole().getPropsArticol(connection, articol);

                retVal += discMaxAV + "#" + discMaxSD + "#" + discMaxDV + "#" +
                         Convert.ToInt32(Double.Parse(multiplu)).ToString() + "#" +
                         cantUmb + "#" + Umb + "#" + discMaxKA + "#" + cmpArticol.ToString() + "#" + pretMediu + "#" + impachetare + "#" + 
                         istoricPret + "#" + procRedCmp + "#" + pretGed + "#" + greutateArt + "#" + dataExp + "#" + articoleRecom + "#"
                          + articolProps.tipMarfa + "#" + greutateBruta + "#" + articolProps.lungime + "#" + cantitate50 + "#" + um50 + "#";


                if (pretOut.Equals("0.0"))
                    retVal = outParam.VMess + ".";




            }
            catch (Exception ex)
            {
                string context =  client + "\n " +  articol + "\n " + cantitate + "\n " + depart + "\n " + um + "\n " + ul + "\n " + tipUser + "\n " + depoz + "\n " + codUser + "\n " + canalDistrib + "\n " + filialaAlternativa;
                ErrorHandling.sendErrorToMail(ex.ToString() + " context: " + context);
                retVal = "Eroare interogare preturi.";
            }
            finally
            {
                webService.Dispose();
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return retVal;

        }


        public string getIstoricPret(OracleConnection connection, String codArticol, String codClient)
        {

            OracleCommand cmd = null;
            OracleDataReader oReader = null;
            string istoric = " ";

            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct x.valoare, x.multiplu, x.um, x.data, x.cantitate " +
                                  " from (select b.valoare, ' ' multiplu, b.um, to_char(to_date(a.datac,'yyyymmdd')) data, b.cantitate " +
                                  " from sapprd.zcomhead_tableta a, sapprd.zcomdet_tableta b where " +
                                  " a.cod_client =:codClient and a.status_aprov in (0,2,15) and a.datac >=:dataStart " +
                                  " and b.id = a.id and b.cod =:codArticol order by datac desc) x where x.valoare > 0 and rownum<3 ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                cmd.Parameters.Add(":dataStart", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = AddressUtils.getMonthDate(-2);

                cmd.Parameters.Add(":codArticol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codArticol;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        istoric += oReader.GetDouble(0).ToString("#.###") + "@ @" + oReader.GetDouble(4) + " " + oReader.GetString(2).ToString() + "@" + oReader.GetString(3) + ":";
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


            return istoric;
        }


        private static string getFilialaCmpBV90(OracleConnection conn, string codArticol, string filialaAgent)
        {
            OracleCommand cmd = null;
            OracleDataReader oReader = null;
            string filialaCmp = filialaAgent;

            try
            {
                cmd = conn.CreateCommand();

                cmd.CommandText = " select lbkum from sapprd.mbew where mandt = '900' and bwkey =:ul and matnr =:codArt ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":ul", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filialaAgent;

                cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = formatFullCodArticol(codArticol);

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {

                    oReader.Read();
                    if (oReader.GetDouble(0) == 0)
                        filialaCmp = "BV90";

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

            return filialaCmp;

        }


        private static double calculeazaCmp(OracleConnection conn, string filiala, string codArticol, string filialaAgent)
        {
            OracleCommand cmd = null;
            OracleDataReader oReader = null;
            double valoareCmp = 0;

            if (codArticol.Length == 8)
                codArticol = "0000000000" + codArticol;

            string filialaCmp;

            if (filiala.Equals("BV90") && codArticol.StartsWith("105"))
                filialaCmp = getFilialaCmpBV90(conn, codArticol, filialaAgent);
            else
                filialaCmp = filiala;


            try
            {
                cmd = conn.CreateCommand();

                cmd.CommandText = " select nvl(to_char(decode(y.lbkum,0,y.verpr,y.salk3/y.lbkum),'99999.9999'),'0') from sapprd.mbew y where " +
                                  " y.mandt='900' and y.matnr=:codArticol  and y.bwkey =:filiala ";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codArticol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = formatFullCodArticol(codArticol);

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filialaCmp;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {

                    oReader.Read();
                    valoareCmp = Double.Parse(oReader.GetString(0).Trim(), CultureInfo.InvariantCulture);



                    double procRedCmp = getProcReducereCmp(conn, codArticol);

                    valoareCmp = valoareCmp * (100 - procRedCmp) / 100;




                }
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }


            return valoareCmp;
        }



        public static double getCmp(OracleConnection conn, string tipAfis, string filiala, string articol, string filialaAgent)
        {
            double cmp = 0;

            if (tipAfis == "3" || tipAfis == "1")
                cmp = calculeazaCmp(conn, filiala, articol, filialaAgent);
            else
                cmp = -1;

            return cmp;

        }




        private static string formatFullCodArticol(string codArticol)
        {
            string codArt = codArticol;

            if (codArt.Length == 8)
                codArt = "0000000000" + codArt;

            return codArt;
        }




        public static double getProcReducereCmp(OracleConnection conn, string codArt)
        {

            return 100;

            /*

            OracleCommand cmd = null;
            OracleDataReader oReader = null;


            

            double procReducere = 0;

            try
            {

                cmd = conn.CreateCommand();

                string sqlString = " select nvl(sum(decode(art,'articol',procent)),0) proc_articol, nvl(sum(decode(art,'sintetic',procent)),0) proc_sintetic from ( " +
                                   " select sum(procent)procent, 'articol' art from sapprd.ZSUBCMP p where p.mandt = '900'  and p.spart = (select spart from articole where cod =:articol) " +
                                   " and p.datab <= to_char(sysdate, 'yyyymmdd') and p.datbi >= to_char(sysdate, 'yyyymmdd') " +
                                   " and matnr =:articol group by 'articol' " +
                                   " union " +
                                   " select sum(procent) procent, 'sintetic' art from sapprd.ZSUBCMP p " +
                                   " where p.mandt = '900' and p.spart =(select spart from articole where cod =:articol)  and p.datab <= to_char(sysdate, 'yyyymmdd') and p.datbi >= to_char(sysdate, 'yyyymmdd') " +
                                   " and matkl = (select sintetic from articole where cod =:articol) group by 'sintetic') x group by 'articol'";

                cmd.CommandText = sqlString;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":articol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArt;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    procReducere = oReader.GetDouble(0) != 0 ? oReader.GetDouble(0) : oReader.GetDouble(1);
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

            return procReducere;

            */

        }




    }

}