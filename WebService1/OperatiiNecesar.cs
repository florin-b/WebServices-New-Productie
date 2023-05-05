using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
using System.Data.Common;
using WebService1.WebNecesar1;
using System.Web.Script.Serialization;

namespace WebService1
{
    public class OperatiiNecesar
    {

        static string getLast(string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
        }

        public string getMaterialeNecesar(string filiala, string departament)
        {
            string serializedResult = "";


            try
            {
                WebNecesar1.ZWBS_ZINTRARIMARFA service = new ZWBS_ZINTRARIMARFA();
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Auth.getUser(), Auth.getPass());
                service.Credentials = nc;

                WebNecesar1.ZgetIntrarimarfa necesarIn = new ZgetIntrarimarfa();

                WebNecesar1.Zsintetice[] sintetice = new WebNecesar1.Zsintetice[1];
                necesarIn.GtSintetice = sintetice;

                WebNecesar1.Zmateriale[] materiale = new WebNecesar1.Zmateriale[1];
                necesarIn.GtMateriale = materiale;


                necesarIn.Avarie = "X";
                necesarIn.Divizie = departament;
                necesarIn.Ul = filiala;
                necesarIn.CaDb = "X";
                necesarIn.CaNb = "X";
                necesarIn.CaUb = "X";
                necesarIn.Cantitativ = "X";
                necesarIn.ExLivrDirecta = "X";
                necesarIn.MatCuAr = "";
                necesarIn.Valoric = "";
                necesarIn.Canal = "10";


                WebNecesar1.ZintrarimarfaRez[] rez = new WebNecesar1.ZintrarimarfaRez[1];
                necesarIn.GtRezultat = rez;


                WebNecesar1.ZgetIntrarimarfaResponse necesarOut = new ZgetIntrarimarfaResponse();
                necesarOut = service.ZgetIntrarimarfa(necesarIn);

                int i = 0;
                int nrRec = necesarOut.GtRezultat.Length;

                List<MaterialNecesar> listaMateriale = new List<MaterialNecesar>();
                MaterialNecesar unMaterial = null;


                for (i = 0; i < nrRec; i++)
                {
                    unMaterial = new MaterialNecesar();
                    unMaterial.numeArticol = necesarOut.GtRezultat[i].Maktx;
                    unMaterial.codArticol = necesarOut.GtRezultat[i].Matnr.Length > 8 ? getLast(necesarOut.GtRezultat[i].Matnr, 8) : necesarOut.GtRezultat[i].Matnr;
                    unMaterial.codSintetic = necesarOut.GtRezultat[i].Matkl;
                    unMaterial.numeSintetic = necesarOut.GtRezultat[i].Wgbez;
                    unMaterial.consum30 = necesarOut.GtRezultat[i].ConsCant.ToString();
                    unMaterial.stoc = necesarOut.GtRezultat[i].Stocne.ToString();
                    unMaterial.propunereNecesar = "0";
                    unMaterial.CA = necesarOut.GtRezultat[i].CaNeelcNb.ToString();
                    unMaterial.interval1 = necesarOut.GtRezultat[i].Intr1c.ToString();
                    unMaterial.interval2 = necesarOut.GtRezultat[i].Intr2c.ToString();
                    unMaterial.interval3 = necesarOut.GtRezultat[i].Intr3c.ToString();

                    listaMateriale.Add(unMaterial);
                }



                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaMateriale);




            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + filiala + " , " + departament);
            }
            finally
            {

            }


            return serializedResult;

        }




    }
}