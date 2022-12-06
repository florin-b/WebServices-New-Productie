using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data;

namespace WebService1
{
    public class OperatiiSuplimentare
    {

        public static void saveTonajAdresa(OracleConnection connection, string codClient, string codAdresa, string tonaj)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();


            if (codClient == null || codAdresa == null || codAdresa.Length < 5 || tonaj == null || tonaj.Equals("-1"))
                return;

            string query;


            string nowDate = Utils.getCurrentDate();
            string nowTime = Utils.getCurrentTime();

            OracleCommand cmd = null;

            try
            {

                cmd = connection.CreateCommand();

                if (!recordTonajExists(connection, codClient, codAdresa))
                {
                    query = " insert into sapprd.ztonajclient(mandt,kunnr,adrnr,greutate,gewei) " +
                            " values ('900', :kunnr, :adrnr, :greutate, 'TO')";


                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":kunnr", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    cmd.Parameters.Add(":adrnr", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codAdresa;

                    cmd.Parameters.Add(":greutate", OracleType.Number, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = tonaj;

                    cmd.ExecuteNonQuery();


                }


            }
            catch (Exception ex)
            {

                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }

            return;
        }


        public static void saveTonajComanda(OracleConnection connection, string idComanda, string tonaj)
        {

            if (tonaj == null || tonaj.Equals("-1") || tonaj.Trim().Equals(""))
                return;


            OracleCommand cmd = null;

            try
            {
                cmd = connection.CreateCommand();

                string query = " insert into sapprd.ztonajcomanda(mandt, idComanda, greutate, gewei) " +
                               " values ('900', :idComanda , :greutate, :gewei)";


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idComanda);

                cmd.Parameters.Add(":greutate", OracleType.Number, 13).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = tonaj;

                cmd.Parameters.Add(":gewei", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = "TO";

                cmd.ExecuteNonQuery();



            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }



        }




        private static bool recordTonajExists(OracleConnection connection, string codClient, string addrNumber)
        {

            bool exists = false;

            OracleDataReader oReader = null;

            try
            {
                OracleCommand cmd = connection.CreateCommand();

                string query = " select count(*) from sapprd.ztonajclient where kunnr = :codClient and adrnr = :codAdresa";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                cmd.Parameters.Add(":codAdresa", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = addrNumber;

                oReader = cmd.ExecuteReader();
                oReader.Read();

                if (oReader.GetInt32(0) > 0)
                    exists = true;
                else
                    exists = false;

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                exists = false;
            }


            return exists;
        }



        public void savePrelucrare04(OracleConnection connection, String idComanda, string tipPrelucrare)
        {



            if (tipPrelucrare == null || tipPrelucrare.Equals("-1"))
                return;

            string query = "";

            try
            {

                OracleCommand cmd = connection.CreateCommand();



                query = " insert into sapprd.zprelucrare04(mandt, idComanda, prelucrare) " +
                        " values ('900', :idComanda , :prelucrare)";


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idComanda);

                cmd.Parameters.Add(":prelucrare", OracleType.NVarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = tipPrelucrare.ToUpper();

                cmd.ExecuteNonQuery();
                

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + idComanda + " , " + tipPrelucrare);
            }

            return;
        }


        public static void saveTonajAdresa(OracleConnection connection, string idComanda, string tonaj)
        {


            if (tonaj == null || tonaj.Equals("-1"))
                return;

            ClientComanda clientComanda = OperatiiComenzi.getClientComanda(connection, idComanda);

            if (clientComanda.codClient != null)
                saveTonajAdresa(connection, clientComanda.codClient, clientComanda.codAdresa, tonaj);

        }

        public static void saveDatePersonale(OracleConnection connection, DateLivrare dateLivrare, ComandaVanzare comandaVanzare)
        {

            OracleCommand cmd = null;

            try
            {

                if (comandaVanzare.cnpClient.Length == 0)
                    return;

                if (existaDatePersonale(connection, dateLivrare, comandaVanzare))
                {
                    return;
                }


                cmd = connection.CreateCommand();


                string query = " insert into sapprd.zinformclmag(mandt, cod_cl, stceg, name1, street, city1, country, regio, tip_cl) " +
                               " values ('900', '0000000000', :stceg, :name1, :street, :city1, 'RO',:regio, :tip_cl) ";


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":stceg", OracleType.NVarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = comandaVanzare.cnpClient;

                cmd.Parameters.Add(":name1", OracleType.NVarChar, 90).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = comandaVanzare.numeClient;

                cmd.Parameters.Add(":street", OracleType.NVarChar, 180).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = dateLivrare.Strada;

                cmd.Parameters.Add(":city1", OracleType.NVarChar, 120).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = dateLivrare.Oras;

                cmd.Parameters.Add(":regio", OracleType.NVarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = dateLivrare.codJudet;

                string tipClient;

                if (comandaVanzare.cnpClient.Trim().Length == 13)
                    tipClient = "PF";
                else
                    tipClient = "PJ";

                cmd.Parameters.Add(":tip_cl", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = tipClient;

                cmd.ExecuteNonQuery();



            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }


        }

        public static void saveCnpPF(OracleConnection connection, Int32 idComanda, string cnp)
        {

            OracleCommand cmd = null;

            try
            {


                cmd = connection.CreateCommand();

                string query = " insert into sapprd.ZSFA_ID_CNP(mandt, id, stceg) " +
                               " values ('900', :idCmd, :stceg) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idCmd", OracleType.Int32, 10).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                cmd.Parameters.Add(":stceg", OracleType.NChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = cnp;

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }


        }


        public static void saveDateDelegat(OracleConnection connection, Int32 idComanda, DateLivrare dateLivrare)
        {

            OracleCommand cmd = null;

            try
            {

                if (dateLivrare.numeDelegat != null && dateLivrare.numeDelegat.Trim().Length == 0 || dateLivrare.ciDelegat.Trim().Length == 0)
                    return;

                cmd = connection.CreateCommand();

                string query = " insert into sapprd.zdatedelegat(mandt, id_comanda, nume, serie_nr_ci, nr_auto) " +
                               " values ('900', :idCmd, :nume, :serie, :auto) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idCmd", OracleType.Int32, 10).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                cmd.Parameters.Add(":nume", OracleType.NChar, 120).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = dateLivrare.numeDelegat;

                cmd.Parameters.Add(":serie", OracleType.NChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = dateLivrare.ciDelegat;

                cmd.Parameters.Add(":auto", OracleType.NChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = dateLivrare.autoDelegat == "" ? " " : dateLivrare.autoDelegat;

                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }


        }

        public static void getDateDelegat(OracleConnection connection, Int32 idComanda, DateLivrare dateLivrare)
        {
            OracleDataReader oReader = null;

            try
            {
                OracleCommand cmd = connection.CreateCommand();

                string query = " select nume, serie_nr_ci, nr_auto from sapprd.zdatedelegat where mandt='900' and id_comanda=:idComanda ";
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Int32, 10).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();
                oReader.Read();

                if (oReader.HasRows)
                {
                    dateLivrare.numeDelegat = oReader.GetString(0);
                    dateLivrare.ciDelegat = oReader.GetString(1);
                    dateLivrare.autoDelegat = oReader.GetString(2);
                }

                if (oReader != null)
                    oReader.Close();

                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

        }





        public static bool existaDatePersonale(OracleConnection connection, DateLivrare dateLivrare, ComandaVanzare comandaVanzare)
        {
            bool existaDatePers = false;

            OracleDataReader oReader = null;

            try
            {
                OracleCommand cmd = connection.CreateCommand();

                string query = " select count(*) from sapprd.zinformclmag where mandt='900' and  stceg = :cnp ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":cnp", OracleType.NVarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = comandaVanzare.cnpClient;

                oReader = cmd.ExecuteReader();
                oReader.Read();

                if (oReader.GetInt32(0) == 0)
                    existaDatePers = false;
                else
                {
                    query = " select upper(name1), regio, upper(city1), upper(street) from sapprd.zinformclmag where mandt='900' and  stceg =:cnp  ";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":cnp", OracleType.NVarChar, 60).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = comandaVanzare.cnpClient;

                    oReader = cmd.ExecuteReader();


                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            if (!oReader.GetString(0).Equals(comandaVanzare.numeClient.ToUpper()))
                            {
                                existaDatePers = true;
                                break;
                            }
                            else
                            {
                                if (oReader.GetString(1).Equals(dateLivrare.codJudet) && oReader.GetString(2).Equals(dateLivrare.Oras.ToUpper()) && oReader.GetString(3).Equals(dateLivrare.Strada.ToUpper()))
                                {
                                    existaDatePers = true;
                                    break;
                                }
                            }
                        }
                    }

                }

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                existaDatePers = false;
            }



            return existaDatePers;
        }


        public static void aprobaAutomatSD(OracleConnection connection, string codAgent, string idComanda)
        {
            OracleCommand cmd = connection.CreateCommand();
            OracleDataReader oReader = null;

            try
            {

                string query = " select count(*) from sapprd.zsuperav where mandt='900' and av = :codAgent ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgent;

                oReader = cmd.ExecuteReader();
                oReader.Read();

                if (oReader.GetInt32(0) == 0)
                {
                    oReader.Close();
                    oReader.Dispose();
                    cmd.Dispose();
                    return;
                }

                DateTime cDate = DateTime.Now;
                string nowTime = cDate.Hour.ToString("00") + cDate.Minute.ToString("00") + cDate.Second.ToString("00");

                cmd.CommandText = " update sapprd.zcomhead_tableta set ora_accept1 =:oraAccept where id =:idComanda ";
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":oraAccept", OracleType.NVarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nowTime;

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = idComanda;

                cmd.ExecuteNonQuery();

                oReader.Close();
                oReader.Dispose();
                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }


        }


        public static void saveComandaSuperAv(OracleConnection connection, string codSuperAv, string idComanda)
        {

            string query = "";

            try
            {
                OracleCommand cmd = connection.CreateCommand();

                query = " insert into sapprd.zcomsuperav(mandt, cod_sagent, id_comanda) " +
                        " values ('900', :codAgent , :idComanda)";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSuperAv;

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = Int32.Parse(idComanda);

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

            return;
        }

        public static void actualizeazaTonajComanda(OracleConnection connection, string idComanda, string tonaj)
        {

            if (tonaj == null || tonaj.Equals("-1") || tonaj.Trim().Equals(""))
                return;


            OracleCommand cmd = null;

            try
            {
                cmd = connection.CreateCommand();

                string query = " update sapprd.ztonajcomanda set greutate=:greutate where id =:idComanda ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":greutate", OracleType.Number, 13).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = tonaj;

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = Int32.Parse(idComanda);

                cmd.ExecuteNonQuery();



            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }



        }


        public static void saveDeviceInfo(OracleConnection connection, string userCode, string strDeviceInfo)
        {

            DeviceInfo deviceInfo = new JavaScriptSerializer().Deserialize<DeviceInfo>(strDeviceInfo);

            OracleCommand cmd = new OracleCommand();

            try
            {

                cmd = connection.CreateCommand();

                string query = " insert into sapprd.zlogtbl (mandt, coduser, datalog, oralog, sdkver, man, model, appname, appver) values " +
                               " ('900', :coduser, :datalog, :oralog, :sdkver, :man, :model, :appname, :appver) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":coduser", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = userCode;

                cmd.Parameters.Add(":datalog", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = General.GeneralUtils.getCurrentDate();

                cmd.Parameters.Add(":oralog", OracleType.NVarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = General.GeneralUtils.getCurrentTime();

                cmd.Parameters.Add(":sdkver", OracleType.NVarChar, 15).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = deviceInfo.sdkVer;

                cmd.Parameters.Add(":man", OracleType.NVarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = deviceInfo.man.ToUpper();

                cmd.Parameters.Add(":model", OracleType.NVarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = deviceInfo.model.ToUpper();

                cmd.Parameters.Add(":appname", OracleType.NVarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = deviceInfo.appName;

                cmd.Parameters.Add(":appver", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[7].Value = deviceInfo.appVer;

                cmd.ExecuteNonQuery();

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


    }
}