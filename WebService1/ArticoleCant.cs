using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data;

namespace WebService1
{
    public class ArticoleCant
    {

        public string getArticoleCant(string unitLog, string codArtPal)
        {

          

            List<ArticolCant> listArticole = new List<ArticolCant>();

            

            SAPWebServices.ZTBL_WEBSERVICE webService = new SAPWebServices.ZTBL_WEBSERVICE();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(DatabaseConnections.getUser(), DatabaseConnections.getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;

            SAPWebServices.Zpalcant inParam = new SAPWebServices.Zpalcant();

            SAPWebServices.Zmateriale[] artPal = new SAPWebServices.Zmateriale[1];
            artPal[0] = new SAPWebServices.Zmateriale();
            artPal[0].Matnr = codArtPal.Length == 8 ? "0000000000" + codArtPal : codArtPal;

            inParam.GvWerks = unitLog;
            inParam.GtMateriale = artPal;
            inParam.GtRezultat = new SAPWebServices.ZpalcantRez[1];

            SAPWebServices.ZpalcantResponse response = webService.Zpalcant(inParam);

            for (int i = 0; i < response.GtRezultat.Length; i++)
            {
                SAPWebServices.ZpalcantRez rezult = response.GtRezultat[i];

                ArticolCant artCant = new ArticolCant();
                artCant.cod = rezult.MatnrCant;
                artCant.sintetic = rezult.Coddecor;
                artCant.denumire = rezult.DenCant;
                artCant.caract = rezult.CaractCant;
                artCant.dimensiuni = rezult.LatimeCant + " x " + rezult.GrosimeCant;
                artCant.stoc = rezult.Stoc.ToString();
                artCant.um = rezult.UmBaza;
                artCant.tipCant = rezult.TipCant;
                artCant.ulStoc = rezult.Werks;
                artCant.depozit = rezult.Lgort;
                listArticole.Add(artCant);

            }

            if (listArticole.Count > 0)
                addArticolExtraData(listArticole);

            return new JavaScriptSerializer().Serialize(listArticole);

        }




        private void addArticolExtraData(List<ArticolCant> listArticole)
        {

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;
            OracleCommand cmd = null;

            string strArticole = "";

            foreach (ArticolCant articol in listArticole)
            {

                if (strArticole == "")
                    strArticole = "'" + articol.cod + "'";
                else
                    strArticole = strArticole + ", '" + articol.cod + "' ";
            }

            strArticole = "(" + strArticole + ")";

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            try
            {

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select  a.cod, b.cod_nivel1, a.umvanz, a.umvanz10, decode(a.grup_vz,' ','-1', a.grup_vz) depart, " +
                                  " decode(trim(a.dep_aprobare), '', '00', a.dep_aprobare) depart_aprob, nvl(a.tip_mat, ' ') tip_mat, " +
                                  " (select nvl((select 1 from sapprd.mara m where m.mandt = '900' and m.matnr = a.cod and m.categ_mat in ('PA','AM')),-1) palet from dual) palet, " +
                                  " a.categ_mat, a.lungime from articole a, sintetice b where a.cod in " + strArticole + " and a.sintetic = b.cod ";




                cmd.CommandType = CommandType.Text;
                oReader = cmd.ExecuteReader();
                string strCat;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        foreach (ArticolCant articol in listArticole)
                        {
                            if (articol.cod.Equals(oReader.GetString(0)))
                            {
                                articol.nivel1 = oReader.GetString(1);
                                articol.umVanz = oReader.GetString(2);
                                articol.umVanz10 = oReader.GetString(3);
                                articol.depart = oReader.GetString(4);
                                articol.departAprob = oReader.GetString(5);
                                articol.tipAB = oReader.GetString(6);
                                articol.umPalet = oReader.GetInt32(7).ToString();

                                strCat = oReader.GetString(8);

                                if (strCat.ToUpper().Equals("AM") || strCat.ToUpper().Equals("PA"))
                                    strCat = "AM";
                                else
                                    strCat = " ";

                                articol.categorie = strCat;
                                articol.lungime = oReader.GetDouble(9).ToString();

                            }
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + cmd.CommandText);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }


        }




    }







class ArticolCant
{
    public string cod;
    public string denumire;
    public string dimensiuni;
    public string caract;
    public string stoc;
    public string um;
    public string tipCant;
    public string ulStoc;
    public string sintetic;
    public string nivel1;
    public string umVanz10;
    public string umVanz;
    public string depart;
    public string departAprob;
    public string umPalet;
    public string tipAB;
    public string categorie;
    public string lungime;
    public string depozit;
}

}