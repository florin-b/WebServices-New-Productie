using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using WebService1.General;

namespace WebService1
{
    public class ClientiFacturati
    {

        public string getClientiFacturatiKA(string codAgent)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<ClientFacturat> listClienti = new List<ClientFacturat>();

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select x.nume, x.cod_client, max(decode(x.luna, :luna1, x.total, 0)) luna1, " +
                                  " max(decode(x.luna, :luna2, x.total, 0)) luna2, max(decode(x.luna, :luna3, x.total, 0)) luna3, " +
                                  " max(decode(x.luna, :luna4, x.total, 0)) luna4, max(decode(x.luna, :luna5, x.total, 0)) luna5, " +
                                  " max(decode(x.luna, :luna6, x.total, 0)) luna6, max(decode(x.luna, :luna7, x.total, 0)) luna7 " +
                                  " from ( select b.nume, a.cod_client, substr(a.datac, 0, 6) luna, sum(a.valoare) total from " +
                                  " sapprd.zcomhead_tableta a, clienti b where a.datac >=:dataRap " +
                                  " and a.cod_agent =:codAgent and a.status in ('0','2') and status_aprov in ('0','2','15') and a.cod_client = b.cod " +
                                  " group by a.cod_client, b.nume, substr(a.datac, 0, 6)  ) x group by x.nume, x.cod_client order by x.nume ";


                cmd.Parameters.Clear();

                cmd.Parameters.Add(":luna1", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = AddressUtils.getMonth(0);

                cmd.Parameters.Add(":luna2", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = AddressUtils.getMonth(-1);

                cmd.Parameters.Add(":luna3", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = AddressUtils.getMonth(-2);

                cmd.Parameters.Add(":luna4", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = AddressUtils.getMonth(-3);

                cmd.Parameters.Add(":luna5", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = AddressUtils.getMonth(-4);

                cmd.Parameters.Add(":luna6", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = AddressUtils.getMonth(-5);

                cmd.Parameters.Add(":luna7", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = AddressUtils.getMonth(-6);

                cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[7].Value = codAgent;

                cmd.Parameters.Add(":dataRap", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[8].Value = AddressUtils.getStartMonthDate(-7);

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ClientFacturat clientFacturat = new ClientFacturat();
                        clientFacturat.nume = oReader.GetString(0);
                        clientFacturat.cod = oReader.GetString(1);

                        clientFacturat.luna1 = oReader.GetDouble(2).ToString();
                        clientFacturat.luna2 = oReader.GetDouble(3).ToString();
                        clientFacturat.luna3 = oReader.GetDouble(4).ToString();
                        clientFacturat.luna4 = oReader.GetDouble(5).ToString();
                        clientFacturat.luna5 = oReader.GetDouble(6).ToString();
                        clientFacturat.luna6 = oReader.GetDouble(7).ToString();
                        clientFacturat.luna7 = oReader.GetDouble(8).ToString();

                        listClienti.Add(clientFacturat);

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

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(listClienti);
        }



        public string getDetaliiClFacturatiKA(string codAgent, string codClient, string data)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<DetaliiFactura> listFacturi = new List<DetaliiFactura>();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select nrcmdsap, to_char(to_date(datac,'yyyymmdd'),'DD-Mon-YYYY','NLS_DATE_LANGUAGE = ROMANIAN'), valoare from sapprd.zcomhead_tableta where cod_agent =:codAgent and " +
                                  " cod_client =:codClient and status in ('0','2') and status_aprov in ('0','2','15') and substr(datac,0, 6) =:dataSel order by datac ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgent;

                cmd.Parameters.Add(":codClient", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codClient;

                cmd.Parameters.Add(":dataSel", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = data;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        DetaliiFactura factura = new DetaliiFactura();
                        factura.nrComanda = oReader.GetString(0);
                        factura.dataEmitere = oReader.GetString(1);
                        factura.valoare = oReader.GetDouble(2).ToString();
                        listFacturi.Add(factura);
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

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(listFacturi);
        }



    }
}