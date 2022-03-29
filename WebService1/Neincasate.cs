using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OracleClient;
using WebService1.General;

namespace WebService1
{
    public class Neincasate
    {

        public string getRaportNeincasateData(string reportParams, string filiala, string tipUserSap)
        {


            string sqlString = "";
            string[] mainToken = reportParams.Split('@');

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string clienti = mainToken[0];
            string agent = mainToken[1];

            List<FacturaNeincasata> listaFacturi = new List<FacturaNeincasata>();
            FacturaNeincasata oFactura = null;

            string listClienti = " ";
            string tabAgenti = " ", condAgenti = " ", campAg1 = " , '0' ", campAg2 = " ";

            if (!clienti.Equals("0"))
            {
                listClienti = " and kunnr in ('" + clienti.Replace("#", "','") + "') and prctr = '" + filiala + "' ";

                
                if (tipUserSap != null && (tipUserSap.Equals("CVIP") || tipUserSap.Equals("SDIP") || filiala == "NN10"))
                        listClienti = " and kunnr in ('" + clienti.Replace("#", "','") + "') ";

                tabAgenti = " , agenti ag ";
                condAgenti = " and ag.cod = angaj ";
                campAg1 = ", ag.nume ";
                campAg2 = ", ag.nume ";
            }
            else
            {
                condAgenti = " and angaj =:codAg ";
            }

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                sqlString = " select angaj, nume_client , kunnr, xblnr,to_char(to_date(bldat,'yyyymmdd')) bldat,decode(termen,'00000000','-',to_char(to_date(termen,'yyyymmdd'))) termen1,val_fml total, " +
                            " incasatml incasat,restplataml restplata,acoperitml acoperit,decode(scadentabo,'00000000','-',to_char(to_date(scadentabo,'yyyymmdd'))) scadentabo, metplata, bldat dat1 " + campAg1 +
                            " from sapprd.zneincasate " + tabAgenti + " where mandt='900' and waerkml='RON' " + condAgenti + listClienti + "  and bldat<=to_char(sysdate,'yyyymmdd') order by nume_client " + campAg2 + " , dat1 ";

                cmd.CommandText = sqlString;


                cmd.Parameters.Clear();

                if (clienti.Equals("0"))
                {
                    cmd.Parameters.Add(":codAg", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = agent;
                }

                oReader = cmd.ExecuteReader();


                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {


                        oFactura = new FacturaNeincasata();
                        oFactura.numeClient = oReader.GetString(1);
                        oFactura.codClient = oReader.GetString(2);
                        oFactura.referinta = oReader.GetString(3);
                        oFactura.emitere = oReader.GetString(4);
                        oFactura.scadenta = oReader.GetString(5);
                        oFactura.valoare = oReader.GetDouble(6).ToString();
                        oFactura.incasat = oReader.GetDouble(7).ToString();
                        oFactura.rest = oReader.GetDouble(8).ToString();
                        oFactura.acoperit = oReader.GetDouble(9).ToString();
                        oFactura.scadentaBO = oReader.GetString(10);
                        oFactura.tipPlata = oReader.GetString(11);
                        oFactura.numeAgent = oReader.GetString(13);
                        listaFacturi.Add(oFactura);


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

            return GeneralUtils.serializeObject(listaFacturi);
        }


        public string getArticoleDocumentNeincasat(string nrDocument)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<ArticolVanzari> listaArticole = new List<ArticolVanzari>();
            ArticolVanzari articol = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                string sqlString = " select decode(length(b.matnr),18,substr(b.matnr,-8),b.matnr) cod,ar.nume matdesc , " +
                                   " decode(a.fkart, 'ZFRA', 0, 'ZFRB', 0, decode(b.shkzg, 'X', -b.fklmg, b.fklmg)) cant, " +
                                   " 0 valoare " +
                                   " from  sapprd.vbrk a, sapprd.vbrp b, articole ar where a.mandt = '900' and b.mandt = '900' " +
                                   " and a.vbeln = b.vbeln and b.vbeln =:nrDocument and b.matnr = ar.cod order by b.matnr ,ar.nume asc";


                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrDocument", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        articol = new ArticolVanzari();
                        articol.codArticol = oReader.GetString(0);
                        articol.numeArticol = oReader.GetString(1);
                        articol.cantitateArticol = oReader.GetDouble(2).ToString();
                        articol.valoareArticol = oReader.GetDouble(3).ToString();
                        listaArticole.Add(articol);

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

            return GeneralUtils.serializeObject(listaArticole);
        }


    }
}