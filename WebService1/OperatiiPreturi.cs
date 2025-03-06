using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Globalization;

namespace WebService1
{
    public class OperatiiPreturi
    {

        public string getPretUnic(string parametruPret)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            ParametruPretGed paramPret = serializer.Deserialize<ParametruPretGed>(parametruPret);

            PretArticolGed pretArticolGed = new PretArticolGed();

            try
            {

                SAPWebServices.ZTBL_WEBSERVICE webService = new SAPWebServices.ZTBL_WEBSERVICE();
                SAPWebServices.ZgetPrice20 inParam = new SAPWebServices.ZgetPrice20();

                webService.Credentials = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
                webService.Timeout = 300000;

                string localUnitLog = paramPret.ul;

                if (paramPret.depart == "11" || paramPret.canalDistrib.Equals("20"))
                    localUnitLog = paramPret.ul.Substring(0, 2) + "2" + paramPret.ul.Substring(3, 1);

                string depozitPret = paramPret.depoz;

                if (depozitPret != null && (depozitPret.Equals("040V1") || depozitPret.Equals("041V1")))
                    depozitPret = "04V1";

                inParam.GvKunnr = paramPret.client;
                inParam.GvMatnr = paramPret.articol;
                inParam.GvSpart = paramPret.canalDistrib.Equals("20") ? "11" : paramPret.depart.Substring(0, 2);
                inParam.GvVrkme = paramPret.um;
                inParam.GvWerks = localUnitLog;
                inParam.GvLgort = depozitPret;
                inParam.GvCant = Decimal.Parse(paramPret.cantitate);
                inParam.GvCantSpecified = true;
                inParam.GvSite = Service1.isConsVanzSite(paramPret.codUser) ? "X" : " ";
                inParam.TipPers = paramPret.tipUser;
                inParam.Canal = paramPret.canalDistrib;
                inParam.Mp = paramPret.metodaPlata;
                inParam.Dzterm = paramPret.termenPlata;
                inParam.Regio = paramPret.codJudet;
                inParam.City = paramPret.localitate == null ? " " : paramPret.localitate.Length <= 25 ? paramPret.localitate : paramPret.localitate.Substring(0, 25);
                inParam.UlStoc = paramPret.filialaAlternativa.Equals("BV90") ? "BV90" : paramPret.filialaClp != null ? paramPret.filialaClp : " ";
                inParam.Traty = paramPret.tipTransport != null ? paramPret.tipTransport : " ";
                inParam.CuRotunj = "X";

                SAPWebServices.ZgetPrice20Response outParam = webService.ZgetPrice20(inParam);

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
                string pretFaraTva = outParam.GvNetwrFtva.ToString();

                string greutateBruta = outParam.GvBrgewMatnr.ToString();

                pretArticolGed.pret = pretOut;
                pretArticolGed.um = umOut;
                pretArticolGed.faraDiscount = noDiscOut;
                pretArticolGed.codArticolPromo = codArtPromo;
                pretArticolGed.cantitateArticolPromo = cantArtPromo;
                pretArticolGed.pretArticolPromo = pretArtPromo;
                pretArticolGed.umArticolPromo = umArtPromo;
                pretArticolGed.pretLista = pretLista;
                pretArticolGed.cantitate = cantOut;
                pretArticolGed.conditiiPret = condPret;
                pretArticolGed.multiplu = multiplu;
                pretArticolGed.cantitateUmBaza = cantUmb;
                pretArticolGed.umBaza = Umb;
                pretArticolGed.procTransport = "0";
                pretArticolGed.impachetare = impachetare;
                pretArticolGed.pretFaraTva = ((outParam.GvNetwrFtva / outParam.GvCant) * outParam.Multiplu).ToString("0.00", CultureInfo.InvariantCulture);
                pretArticolGed.valTrap = "0";
                pretArticolGed.dataExp = outParam.GvDatbi;
                pretArticolGed.greutate = outParam.GvBrgew.ToString();

                pretArticolGed.errMsg = outParam.VMess;
                pretArticolGed.um50 = outParam.GvUm50;
                pretArticolGed.cantitate50 = outParam.GvQty50.ToString();
                pretArticolGed.pretMinim = outParam.GvNetwrMin.ToString();
                pretArticolGed.promo = outParam.GvPromo;


                OracleConnection connection = new OracleConnection();
                OracleCommand cmd = new OracleCommand();
                OracleDataReader oReader = null;

                string strFilialaCmp = paramPret.ul;

                if (paramPret.filialaClp != null && paramPret.filialaClp.Trim() != "")
                    strFilialaCmp = paramPret.filialaClp;

                if (!paramPret.depart.Equals("11"))
                    strFilialaCmp = strFilialaCmp.Substring(0, 2) + "1" + strFilialaCmp.Substring(3, 1);

                if (paramPret.canalDistrib.Equals("20"))
                    strFilialaCmp = strFilialaCmp.Substring(0, 2) + "2" + strFilialaCmp.Substring(3, 1);

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select nvl(to_char(decode(y.lbkum,0,y.verpr,y.salk3/y.lbkum),'999999.9999'),0) from sapprd.mbew y where " +
                                  " y.mandt='900' and y.matnr=:matnr  and y.bwkey = :unitLog  ";

                cmd.CommandType = System.Data.CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":matnr", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = paramPret.articol;

                cmd.Parameters.Add(":unitLog", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = strFilialaCmp;   



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

                        cmd.CommandType = System.Data.CommandType.Text;

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(":matnr", OracleType.VarChar, 54).Direction = System.Data.ParameterDirection.Input;
                        cmd.Parameters[0].Value = paramPret.articol;

                        cmd.Parameters.Add(":unitLog", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                        cmd.Parameters[1].Value = paramPret.ul.Substring(0, 2) + "1" + paramPret.ul.Substring(3, 1);      //ul. distributie

                        oReader = cmd.ExecuteReader();

                        oReader.Read();
                        cmpArticol = Convert.ToDouble(oReader.GetString(0));

                    }


                }


                //coeficient corectie
                cmd.CommandText = " select a.coef_corr from sapprd.zexc_coef_marja a, articole b where b.cod =:articol  and a.matkl = b.sintetic " +
                                  " and a.pdl =:ul and a.functie =:functie ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":articol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = paramPret.articol;

                cmd.Parameters.Add(":ul", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = paramPret.ul.Substring(0, 2) + "1" + paramPret.ul.Substring(3, 1);      //ul. distributie

                cmd.Parameters.Add(":functie", OracleType.VarChar, 36).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = paramPret.tipUser;

                oReader = cmd.ExecuteReader();
                oReader.Read();

                if (oReader.HasRows)
                    pretArticolGed.coefCorectie = oReader.GetDouble(0).ToString();
                else
                    pretArticolGed.coefCorectie = "0";



                pretArticolGed.cmp = cmpArticol.ToString();

                pretArticolGed.procReducereCmp = Preturi.getProcReducereCmp(connection, paramPret.articol).ToString();


                //descriere conditii pret
                string[] codReduceri = condPret.Split(';');
                string[] tokCod;


                condPret = "";

                for (int jj = 0; jj < codReduceri.Length; jj++)
                {

                    tokCod = codReduceri[jj].Split(':');


                    cmd = connection.CreateCommand();


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

                pretArticolGed.conditiiPret = condPret;





                //discounturi maxime
                string discMaxAV = "0", discMaxSD = "0", discMaxDV = "0", discMaxKA = "0";


                pretArticolGed.discMaxAV = discMaxAV;
                pretArticolGed.discMaxSD = discMaxSD;
                pretArticolGed.discMaxDV = discMaxDV;
                pretArticolGed.discMaxKA = discMaxKA;

                //sf. disc

                //pret si adaos mediu
                string pretMediu = "0";
                string adaosMediu = "0";
                string unitMasPretMediu = "0";


                if (paramPret.depart.Equals("11"))
                {



                    double dMarjaBruta = (Double.Parse(pretArticolGed.pret) / Double.Parse(pretArticolGed.cantitate)) - Double.Parse(pretArticolGed.cmp) * 1.19;
                    double dPretMediu = (Double.Parse(pretArticolGed.pret) / Double.Parse(pretArticolGed.cantitate)) - dMarjaBruta * 0.19;
                    double dMarjaMedie = dPretMediu - Double.Parse(pretArticolGed.cmp) * 1.19;

                    pretMediu = dPretMediu.ToString();
                    adaosMediu = dMarjaMedie.ToString();
                    unitMasPretMediu = pretArticolGed.um;

                }
                else
                {


                    pretMediu = ((Double.Parse(pretOut) / 1.19) / Double.Parse(cantOut)).ToString();


                    cmd = connection.CreateCommand();
                    cmd.CommandText = " select to_char(pret_med/cant, '99990.999')  , to_char(adaos_med/cant,'99990.999')  , um from sapprd.zpret_mediu_oras r where mandt = '900' and pdl =:unitLog " +
                                      " and matnr=:articol ";


                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":unitLog", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = paramPret.ul.Substring(0, 2) + "1" + paramPret.ul.Substring(3, 1);

                    cmd.Parameters.Add(":articol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = paramPret.articol;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();

                        pretMediu = oReader.GetString(0).Trim();
                        adaosMediu = oReader.GetString(1).Trim();
                        unitMasPretMediu = oReader.GetString(2);
                    }

                }


                string istoricPret = " ";

                if (paramPret.canalDistrib.Equals("10"))
                    istoricPret = new Preturi().getIstoricPret(connection, paramPret.articol, paramPret.client);

                if (paramPret.tipUser.Equals("WOOD"))
                    istoricPret = new Preturi().getIstoricPretWood(connection, paramPret.articol, paramPret.client);

                pretArticolGed.articoleRecomandate = new OperatiiArticole().getArticoleRecomandate(connection, paramPret.articol, "11");
                ArticolProps articolProps = new OperatiiArticole().getPropsArticol(connection, paramPret.articol);

                DatabaseConnections.CloseConnections(oReader, cmd, connection);

                pretArticolGed.pretMediu = pretMediu;
                pretArticolGed.adaosMediu = adaosMediu;
                pretArticolGed.umPretMediu = unitMasPretMediu;
                pretArticolGed.istoricPret = istoricPret;

                pretArticolGed.tipMarfa = articolProps.tipMarfa;
                pretArticolGed.greutateBruta = greutateBruta;
                pretArticolGed.lungime = articolProps.lungime;
                pretArticolGed.tipTransport = articolProps.transpTert.ToLower().Equals("y") ? "TERT" : "TRAP";

                webService.Dispose();

                if (Double.Parse(pretArticolGed.pretMinim) == 0)
                {

                    if (pretArticolGed.errMsg.Trim().Length == 0)
                        pretArticolGed.errMsg = "Eroare determinare pret.";

                   

                    return "-1";

                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + parametruPret);
            }

            return serializer.Serialize(pretArticolGed);

        }

    }
}