using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace WebService1
{
    public class ComenziSite
    {

        public string salveazaComanda(string comanda, bool alertSD, bool alertDV, bool cmdAngajament, string tipUser, string JSONArt, string JSONComanda, string JSONDateLivrare)
        {

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            ComandaVanzare comandaVanzare = serializer.Deserialize<ComandaVanzare>(JSONComanda);
            DateLivrare dateLivrare = serializer.Deserialize<DateLivrare>(JSONDateLivrare);
            List<ArticolComanda> listArticole = serializer.Deserialize<List<ArticolComanda>>(JSONArt);

            List<ArticolComanda> tempListBV90 = new List<ArticolComanda>();
            List<ArticolComanda> tempListOrig = new List<ArticolComanda>();
            List<ArticolComanda> tempListAlta = new List<ArticolComanda>();

            string altaFiliala = "", filialaOrig = "";
            double totalCmdBV = 0, totalCmdOrig = 0, totalCmdAlta = 0;

            double pretTransp = 0.0;
            string idComanda = "";

            double valoareIncasareBV = 0, valoareIncasareOrig = 0, valoareIncasareAlta = 0;

            filialaOrig = ComenziSiteHelper.getUlDistrib(dateLivrare.unitLog);

            foreach (ArticolComanda articol in listArticole)
            {
                if (articol.filialaSite == null || articol.filialaSite.Length == 0)
                    continue;

                if (articol.filialaSite.Equals("BV90"))
                {
                    tempListBV90.Add(articol);
                    if (!articol.codArticol.Equals("000000000030101050"))
                        totalCmdBV += articol.pretUnit * Double.Parse(articol.cantUmb);
                    else
                        valoareIncasareBV = articol.pretUnit * Double.Parse(articol.cantUmb);
                }
                else if (articol.filialaSite.Substring(0, 2).Equals(dateLivrare.unitLog.Substring(0, 2)))
                {
                    tempListOrig.Add(articol);
                    if (!articol.codArticol.Equals("000000000030101050"))
                        totalCmdOrig += articol.pretUnit * Double.Parse(articol.cantUmb);
                    else
                        valoareIncasareOrig = articol.pretUnit * Double.Parse(articol.cantUmb);
                }
                else
                {
                    tempListAlta.Add(articol);
                    altaFiliala = articol.filialaSite;
                    if (!articol.codArticol.Equals("000000000030101050"))
                        totalCmdAlta += articol.pretUnit * Double.Parse(articol.cantUmb);
                    else
                        valoareIncasareAlta = articol.pretUnit * Double.Parse(articol.cantUmb);
                }

            }

            comandaVanzare.parrentId = CurrentMillis.Millis.ToString().Substring(0, 11);

            string JSONComandaLocal = "";
            string JSONDateLivrareLocal = "";
            string JSONArtLocal = "";
            string retVal = "";

            if (tempListOrig.Count > 0)
            {

                comandaVanzare.filialaAlternativa = filialaOrig;
                dateLivrare.totalComanda = totalCmdOrig.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                comandaVanzare.valoareIncasare = valoareIncasareOrig.ToString();

                JSONArtLocal = serializer.Serialize(tempListOrig);
                JSONComandaLocal = serializer.Serialize(comandaVanzare);
                JSONDateLivrareLocal = serializer.Serialize(dateLivrare);

                retVal = new Service1().saveAVNewCmd(comanda, alertSD, alertDV, cmdAngajament, "CV", JSONArtLocal, JSONComandaLocal, JSONDateLivrareLocal, true, "");

                pretTransp += getPretTransport(retVal);
                idComanda = getIdComanda(retVal);

            }

            if (tempListAlta.Count > 0)
            {

                dateLivrare.unitLog = ComenziSiteHelper.getUlGed(altaFiliala);
                dateLivrare.totalComanda = totalCmdAlta.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                comandaVanzare.valoareIncasare = valoareIncasareAlta.ToString();

                comandaVanzare.filialaAlternativa = altaFiliala;

                JSONArtLocal = serializer.Serialize(tempListAlta);
                JSONComandaLocal = serializer.Serialize(comandaVanzare);
                JSONDateLivrareLocal = serializer.Serialize(dateLivrare);
                retVal = new Service1().saveAVNewCmd(comanda, alertSD, alertDV, cmdAngajament, "CV", JSONArtLocal, JSONComandaLocal, JSONDateLivrareLocal, true, "");

                pretTransp += getPretTransport(retVal);

                if (idComanda == "")
                    idComanda = getIdComanda(retVal);
                else
                    idComanda += "," + getIdComanda(retVal);
            }


            if (tempListBV90.Count > 0)
            {

                comandaVanzare.filialaAlternativa = "BV90";
                dateLivrare.unitLog = ComenziSiteHelper.getUlGed(filialaOrig);
                dateLivrare.totalComanda = totalCmdBV.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                comandaVanzare.valoareIncasare = valoareIncasareBV.ToString();

                JSONComandaLocal = serializer.Serialize(comandaVanzare);
                JSONDateLivrareLocal = serializer.Serialize(dateLivrare);

                JSONArtLocal = serializer.Serialize(tempListBV90);
                retVal = new Service1().saveAVNewCmd(comanda, alertSD, alertDV, cmdAngajament, "CV", JSONArtLocal, JSONComandaLocal, JSONDateLivrareLocal, true, "");

                pretTransp += getPretTransport(retVal);

                if (idComanda == "")
                    idComanda = getIdComanda(retVal);
                else
                    idComanda += "," + getIdComanda(retVal);

            }

            if (!dateLivrare.Transport.Equals("TCLI"))
                retVal = "100#" + pretTransp.ToString() + "#" + idComanda;

            return retVal;
        }



        private double getPretTransport(string strTransp)
        {

            if (!strTransp.Contains("#"))
                return 0;

            string[] tokTransp = strTransp.Split('#');
            return Double.Parse(tokTransp[1]);

        }


        private string getIdComanda(string strComanda)
        {
            if (!strComanda.Contains("#"))
                return "-1";

            return strComanda.Split('#')[2];

        }



        public static class CurrentMillis
        {
            private static readonly DateTime Jan1St2016 = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            public static long Millis { get { return (long)((DateTime.UtcNow - Jan1St2016).TotalMilliseconds); } }
        }



    }
}