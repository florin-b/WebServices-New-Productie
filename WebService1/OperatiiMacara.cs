using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using WebService1.SAPWebServices;
using System.Data.OracleClient;
using System.Data;

namespace WebService1
{
    public class OperatiiMacara
    {

        public String getCostMacara(string unitLog, string codAgent, string codClient, string codFurnizor, string listArt)
        {


            JavaScriptSerializer serializer = new JavaScriptSerializer();

            List<ArticolCalculDesc> listArtCmd = serializer.Deserialize<List<ArticolCalculDesc>>(listArt);

            SAPWebServices.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE(); 

            SAPWebServices.ZNrPaleti inParam = new SAPWebServices.ZNrPaleti(); 
            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;

            OracleConnection connection = new OracleConnection();
            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            SAPWebServices.ZstNrPaleti[] nrpal = new ZstNrPaleti[listArtCmd.Count];

            for (int i = 0; i < listArtCmd.Count; i++)
            {
                nrpal[i] = new ZstNrPaleti();

                string codArt = listArtCmd[i].cod;
                if (listArtCmd[i].cod.Length == 8)
                    codArt = "0000000000" + listArtCmd[i].cod;


                nrpal[i].Matnr = codArt;
                nrpal[i].Meins = listArtCmd[i].um;
                nrpal[i].Menge = Convert.ToDecimal(listArtCmd[i].cant);
                nrpal[i].Lgort = listArtCmd[i].depoz.Equals("041V1") ? "04V1" : listArtCmd[i].depoz;

            }


            inParam.ItTable = nrpal;
            inParam.IpWerks = unitLog;
            inParam.IpPernr = codAgent;
            inParam.IpKunnr = codClient;
            inParam.IpLifnr = codFurnizor != null ? codFurnizor : " ";

          
            SAPWebServices.ZNrPaletiResponse resp = webService.ZNrPaleti(inParam);
            SAPWebServices.ZstEtPaleti[] valPaleti = resp.EtValpal;
            SAPWebServices.ZstEtMarfaPalet[] listMarfaPaleti = resp.EtMarfaPalet;

            CostDescarcare costDescarcare = new CostDescarcare();
            costDescarcare.sePermite = !resp.EpMacara.Equals("X");
            costDescarcare.filiala = unitLog;

            List<ArticolDescarcare> listArticole = new List<ArticolDescarcare>();

            ArticolDescarcare articol = new ArticolDescarcare();

            for (int i = 0; i < valPaleti.Length; i++)
            {
                articol = new ArticolDescarcare();
                articol.cod = valPaleti[i].Matnr.TrimStart('0');
                articol.depart = valPaleti[i].Spart.Equals("04") ? "041" : valPaleti[i].Spart;
                articol.valoare = valPaleti[i].Valpal.Trim();
                articol.cantitate = valPaleti[i].Nrpal.ToString();
                articol.valoareMin = valPaleti[i].Valmin.Trim();
                listArticole.Add(articol);
            }

            if (valPaleti.Length == 0)
            {
                articol = new ArticolDescarcare();
                articol.cod = "30101791";
                articol.depart = "01";
                articol.valoare = "0";
                articol.cantitate = "0";
                articol.valoareMin = "0";
                listArticole.Add(articol);
            }

            List<ArticolPalet> listPaleti = new List<ArticolPalet>();

            for (int i = 0; i < listMarfaPaleti.Length; i++)
            {
                ArticolPalet artPalet = new ArticolPalet();
                artPalet.codPalet = listMarfaPaleti[i].MatnrPalet.TrimStart('0');
                artPalet.depart = listMarfaPaleti[i].SpartPalet;
                artPalet.cantitate = Decimal.ToInt32(listMarfaPaleti[i].CantPalet).ToString();
                artPalet.numePalet = getNumeArticol(connection, listMarfaPaleti[i].MatnrPalet);
                artPalet.codArticol = listMarfaPaleti[i].MatnrMarfa.TrimStart('0');
                artPalet.numeArticol = getNumeArticol(connection, listMarfaPaleti[i].MatnrMarfa);
                artPalet.furnizor = listMarfaPaleti[i].FurnizorPalet;
                artPalet.pretUnit = listMarfaPaleti[i].PretPalet.ToString();
                artPalet.cantArticol = listMarfaPaleti[i].CantMarfa.ToString();
                artPalet.umArticol = listMarfaPaleti[i].MeinsMarfa;
                listPaleti.Add(artPalet);


            }


            costDescarcare.articoleDescarcare = listArticole;
            costDescarcare.articolePaleti = getPaletiDistincti(listPaleti);

            return new JavaScriptSerializer().Serialize(costDescarcare);

        }


        public string getCostMacaraComenzi(string codAgent, string codClient, string codFurnizor, string listComenzi)
        {
            List<CostDescarcare> costDescarcare = new List<CostDescarcare>();

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<ComandaCalculDescarcare> listCom = serializer.Deserialize<List<ComandaCalculDescarcare>>(listComenzi);

            foreach (ComandaCalculDescarcare comanda in listCom)
            {
                string costDescComanda = getCostMacara(comanda.filiala, codAgent, codClient, codFurnizor, comanda.listArticole);
                costDescarcare.Add(serializer.Deserialize<CostDescarcare>(costDescComanda));
            }



            return serializer.Serialize(costDescarcare);
        }

        private List<ArticolPalet> getPaletiDistincti(List<ArticolPalet> listPaleti)
        {
            List<ArticolPalet> paletiDistincti = new List<ArticolPalet>();
            List<string> coduriUnice = listPaleti.Select(p => p.codPalet).Distinct().ToList<string>();

            foreach (string codPalet in coduriUnice)
            {
                ArticolPalet palet = getDatePalet(listPaleti, codPalet);
                palet.cantitate = getCantitatePalet(listPaleti, codPalet).ToString();
                paletiDistincti.Add(palet);
            }

            return paletiDistincti;
        }

        private ArticolPalet getDatePalet(List<ArticolPalet> listPaleti, string codPalet)
        {
            ArticolPalet palet = new ArticolPalet();

            foreach (ArticolPalet pal in listPaleti)
            {

                if (pal.codPalet.Equals(codPalet))
                {
                    palet.codPalet = pal.codPalet;
                    palet.depart = pal.depart;
                    palet.numePalet = pal.numePalet;
                    palet.pretUnit = pal.pretUnit;
                    palet.codArticol = pal.codArticol;
                    break;
                }

            }

            return palet;
        }

        private int getCantitatePalet(List<ArticolPalet> listPaleti, string codPalet)
        {
            int cantPalet = 0;

            foreach (ArticolPalet pal in listPaleti)
            {
                if (pal.codPalet.Equals(codPalet))
                {
                    cantPalet += Int32.Parse(pal.cantitate);
                }
            }

            return cantPalet;
        }

        private static string getNumeArticol(OracleConnection connection, string codArticol)
        {
            string numeArticol = "Nedefinit";

            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {
                cmd = connection.CreateCommand();

                string sqlString = " select nume from articole where cod = :codArticol ";

                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codArticol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    numeArticol = oReader.GetString(0);
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

            return numeArticol;

        }


    }
}