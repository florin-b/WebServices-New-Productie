using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OracleClient;

namespace WebService1
{
    public class HelperClienti
    {

        public static DateClientInstPublica getDateClientInstPublica(OracleConnection connection, string codClient)
        {

            DateClientInstPublica dateClient = new DateClientInstPublica();
            dateClient.isClientInstPublica = false;

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select v.kdgrp from sapprd.knvv v, sapprd.knvp p where v.mandt = '900' and v.vtweg = '20' " +
                                  " and v.spart = '11' and v.kdgrp in ('18','19') and v.mandt = p.mandt and v.kunnr = p.kunnr and v.vtweg = p.vtweg and v.spart = p.spart " +
                                  " and p.parvw = 'ZA' and v.kunnr = :codClient ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    dateClient.isClientInstPublica = true;

                    if (oReader.GetString(0).Equals("18"))
                        dateClient.tipClientInstPublica = "CONSTR";
                    else
                        dateClient.tipClientInstPublica = "NONCONSTR";
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

            return dateClient;

        }


        public static bool isClientBlocat(OracleConnection connection, string codClient)
        {
            bool isBlocat = false;

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd = connection.CreateCommand();
                cmd.CommandText = " select k.crblb from sapprd.knkk k where k.mandt = '900' and k.kkber = '1000' and k.kunnr = :codClient ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    if (oReader.GetString(0).Equals("X"))
                        isBlocat = true;

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

            return isBlocat;
        }

        public static double getLimitaCreditClient(OracleConnection connection, string codClient)
        {

            double limitaCredit = 0;
            DateClientInstPublica dateClient = new DateClientInstPublica();
            dateClient.isClientInstPublica = false;

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select k.klimk  from sapprd.knkk k, sapprd.knb1 b where k.mandt='900' and b.mandt = k.mandt " +
                                  " and k.kunnr = :codClient and b.kunnr = k.kunnr and k.klimk > 1 ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    limitaCredit = oReader.GetDouble(0);
                }
                else
                {
                    cmd.CommandText = " select k.klimk from sapprd.knkk k, sapprd.knb1 b, sapprd.kna1 c " +
                                  " where k.mandt = '900' and b.mandt = '900' and c.mandt = '900' and c.kunnr = k.kunnr " +
                                  " and k.kunnr = :codClient and b.kunnr = k.kunnr " +
                                  " and c.ZDATAEXPCTR != '00000000' and to_date(c.ZDATAEXPCTR, 'yyyymmdd') >= sysdate and k.klimk > 1 ";

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        limitaCredit = oReader.GetDouble(0);
                    }
                    else
                    {
                        cmd.CommandText = " select k.klimk from sapprd.knkk k, sapprd.knb1 b, sapprd.kna1 c " +
                                  " where k.mandt = '900' and b.mandt = '900' and c.mandt = '900' and c.kunnr = k.kunnr " +
                                  " and k.kunnr = :codClient and b.kunnr = k.kunnr and k.klimk > 1 " +
                                  " and (exists (select 1 from sapprd.knvv v where v.mandt = '900' and v.kunnr = b.kunnr and v.vtweg = '20' and v.spart = '11' and v.kdgrp in ('18','19') ) " +
                                  " or ( c.ZDATAEXPCTR != '00000000' and to_date(c.ZDATAEXPCTR, 'yyyymmdd') >= sysdate )) and k.klimk > 1 ";

                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = codClient;

                        oReader = cmd.ExecuteReader();

                        if (oReader.HasRows)
                        {
                            oReader.Read();
                            limitaCredit = oReader.GetDouble(0);
                        }
                    }

                }



            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                limitaCredit = 0;
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return limitaCredit;

        }

        public double getValoareComenziNumerar(string codClient, string dataLivrare, string tipClient, string idComanda)
        {

            double valoareComenzi = 0;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string condClient = " cod_client = :codClient ";

                if (tipClient.Equals("PF"))
                    condClient = " telefon = :codClient ";
                else if (tipClient.Equals("PJG"))
                    condClient = " regexp_replace(stceg, 'RO', '') = :codClient ";

                string condComanda = "";
                if (idComanda != null && !idComanda.Equals(String.Empty))
                    condComanda = " and id != :idComanda ";

                cmd.CommandText = " select nvl(sum(valoare),0) from sapprd.zcomhead_tableta where status = '2' and status_aprov in ('0', '2', '15') and " +
                                    condClient + " and tip_plata in ('E','E1') and ketdat = :dataLivrare " + condComanda;


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                cmd.Parameters.Add(":dataLivrare", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = formatDataSap(dataLivrare);

                if (idComanda != null && !idComanda.Equals(String.Empty))
                {
                    cmd.Parameters.Add(":idComanda", OracleType.Int32, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = Int32.Parse(idComanda);
                }

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    valoareComenzi = oReader.GetDouble(0);
                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("getValoareComenzi: " + ex.ToString() + " params: " + codClient + " , " + dataLivrare + " , " + tipClient);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return valoareComenzi;
        }

        public static string getCnpClient(OracleConnection connection, Int32 idComanda)
        {
            string cnpClient = " ";
            OracleDataReader oReader = null;

            try
            {
                OracleCommand cmd = connection.CreateCommand();

                string query = " select stceg from sapprd.ZSFA_ID_CNP where mandt='900' and id =:idComanda ";
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Int32, 10).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();
                oReader.Read();

                if (oReader.HasRows)
                {
                    cnpClient = oReader.GetString(0);
                }

                if (oReader != null)
                    oReader.Close();

                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

            return cnpClient;

        }

        private static string formatDataSap(string dataRaw)
        {
            string[] dataArray = dataRaw.Split('.');

            return dataArray[2] + dataArray[1] + dataArray[0];
        }

    }
}