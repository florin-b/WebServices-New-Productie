using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using WebService1.SAPWebServices;

namespace WebService1
{
    public class OperatiiDocumente
    {
        public string getDocumente(String depart)
        {

            string documente = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string condDepart = "";
                if (!depart.Equals("TOAT"))
                    condDepart = " where depart=:depart ";

                cmd.CommandText = " select document from sapprd.zdocprod " + condDepart;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                if (!depart.Equals("TOAT"))
                {
                    cmd.Parameters.Add(":depart", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = depart;
                }

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                        if (documente == "")
                            documente = oReader.GetString(0);
                        else
                            documente += "#" + oReader.GetString(0);
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


            return documente;

        }



        public string getFacturiNeincasate(string codClient, string codDepart, string codAgent, string unitLog)
        {

           

            List<FacturaNeincasataLite> listFacturi = new List<FacturaNeincasataLite>();

            PlataNeincasate.ZFG_INSTR_PLATA plataNeincasata = new PlataNeincasate.ZFG_INSTR_PLATA();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            plataNeincasata.Credentials = nc;

            PlataNeincasate.ZlistaFacturi serviceObj = new PlataNeincasate.ZlistaFacturi();

            serviceObj.PiKunnr = codClient;
            serviceObj.ItDocs = new PlataNeincasate.Zsneincasate[0];

            PlataNeincasate.ZlistaFacturiResponse response = plataNeincasata.ZlistaFacturi(serviceObj);

            for (int i = 0; i < response.ItDocs.Length; i++)
            {

                FacturaNeincasataLite factura = new FacturaNeincasataLite();
                factura.nrFactura = response.ItDocs[i].Belnr;
                factura.restPlata = response.ItDocs[i].Amount.ToString();
                factura.dataEmitere = formatDateSapService(response.ItDocs[i].Docdate);
                factura.anDocument = response.ItDocs[i].Gjahr;
                factura.nrDocument = response.ItDocs[i].Xblnr;
                
                listFacturi.Add(factura);
            }


            return new JavaScriptSerializer().Serialize(listFacturi);


        }

        public string salveazaPlataNeincasata(string strPlata)
        {

            string responseStr;

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            PlataNeincasata objPlata = serializer.Deserialize<PlataNeincasata>(strPlata);
            List<IncasareDocument> objDocumente = serializer.Deserialize<List<IncasareDocument>>(objPlata.listaDocumente);

            try
            {


                PlataNeincasate.ZtbSaveDocCliring serviceObj = new PlataNeincasate.ZtbSaveDocCliring();
                PlataNeincasate.ZFG_INSTR_PLATA plataNeincasata = new PlataNeincasate.ZFG_INSTR_PLATA();

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
                plataNeincasata.Credentials = nc;

                PlataNeincasate.ZfiBoCec antet = new PlataNeincasate.ZfiBoCec();

                antet.Client = objPlata.codClient;
                antet.CodAgent = objPlata.codAgent;

                if (objPlata.dataEmitere != null && objPlata.dataEmitere != "")
                    antet.DataEmitere = getFormattedDate(objPlata.dataEmitere);

                if (objPlata.dataScadenta != null && objPlata.dataScadenta != "")
                    antet.DataScadenta = getFormattedDate(objPlata.dataScadenta);


                antet.SerieNumar = objPlata.seriaDocument;
                antet.StareCambie = objPlata.tipDocument;
                antet.Sgtxt = objPlata.girant;
               

                antet.Suma = Decimal.Parse(objPlata.sumaPlata);

                PlataNeincasate.ZstDocsClearing[] documente = new PlataNeincasate.ZstDocsClearing[objDocumente.Count];

                for (int i = 0; i < objDocumente.Count; i++)
                {
                    documente[i] = new PlataNeincasate.ZstDocsClearing();
                    documente[i].NrDocument = objDocumente[i].nrDocument;
                    documente[i].SumaDocument = Decimal.Parse(objDocumente[i].sumaIncasata);
                    documente[i].AnDocument = objDocumente[i].anDocument;
                   

                }

                serviceObj.IsBoCec = antet;
                serviceObj.ItDocsClearing = documente;

                PlataNeincasate.ZtbSaveDocCliringResponse response = plataNeincasata.ZtbSaveDocCliring(serviceObj);

                string respDoc = response.EtDocsPlata.ToString();

                responseStr = response.EpMessage.ToString() + "#" + response.EpSucces + "#";



            }
            catch (Exception ex)
            {
                responseStr = ex.ToString();
                ErrorHandling.sendErrorToMail("date: " + strPlata + " , " + ex.ToString());
            }

            return responseStr;
        }


        public string getStareDocument(string nrDocument)
        {

            string stareResponse = "Stare nedefinita";

            SAPWebServices.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE();

            SAPWebServices.ZstareCurenta inParam = new ZstareCurenta();
            inParam.PVbeln = nrDocument;

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;

            SAPWebServices.ZstareCurentaResponse outParam = webService.ZstareCurenta(inParam);

            if (outParam.EtStatus.Length > 0)
                stareResponse = "";

            string transfer = "";
            string livrare = "";

            for (int i = 0; i < outParam.EtStatus.Length; i++)
            {
                transfer = "";
                livrare = "";

                if (outParam.EtStatus[i].Ebeln != null && outParam.EtStatus[i].Ebeln.Trim() != "")
                    transfer = " Transf. " + outParam.EtStatus[i].Ebeln + ". ";

                if (outParam.EtStatus[i].Livr != null && outParam.EtStatus[i].Livr.Trim() != "")
                    livrare = " Livr. " + outParam.EtStatus[i].Livr + ". ";

                stareResponse += "Cmd. " + outParam.EtStatus[i].Vbeln + " - " + transfer + livrare + outParam.EtStatus[i].StatusLong + "." + System.Environment.NewLine;
            }

            webService.Dispose();

            return stareResponse;


        }


        private string getFormattedDate(string strDate)
        {
            string[] arrayDate = strDate.Split('.');
            return arrayDate[2] + arrayDate[1] + arrayDate[0];


        }

        private string formatDateSapService(string strDate)
        {
            string[] arrayDate = strDate.Split('-');
            return arrayDate[2] + "-" + arrayDate[1] + "-" + arrayDate[0];


        }
    }
}