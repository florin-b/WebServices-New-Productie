using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributieWebServices
{
    public class UtilsAddresses
    {

        public static string formatAdresa(string adresa)
        {

            string adrFormat = "";
            string[] arrayAddr = adresa.Split(',');


            if (arrayAddr.Length > 0)
            {
                adrFormat = getNumeJudet(arrayAddr[0]);

                for (int i = 1; i < arrayAddr.Length; i++)
                {
                    adrFormat += "," + arrayAddr[i];

                }

            }


            return adrFormat;


        }



        public static string getNumeJudet(string codJudet)
        {
            string retVal = "Nedefinit";

            if (codJudet.Equals("01"))
                retVal = "ALBA";

            if (codJudet.Equals("02"))
                retVal = "ARAD";

            if (codJudet.Equals("03"))
                retVal = "ARGES";

            if (codJudet.Equals("04"))
                retVal = "BACAU";

            if (codJudet.Equals("05"))
                retVal = "BIHOR";

            if (codJudet.Equals("06"))
                retVal = "BISTRITA-NASAUD";

            if (codJudet.Equals("07"))
                retVal = "BOTOSANI";

            if (codJudet.Equals("09"))
                retVal = "BRAILA";

            if (codJudet.Equals("08"))
                retVal = "BRASOV";

            if (codJudet.Equals("40"))
                retVal = "BUCURESTI";

            if (codJudet.Equals("10"))
                retVal = "BUZAU";

            if (codJudet.Equals("51"))
                retVal = "CALARASI";

            if (codJudet.Equals("11"))
                retVal = "CARAS-SEVERIN";

            if (codJudet.Equals("12"))
                retVal = "CLUJ";

            if (codJudet.Equals("13"))
                retVal = "CONSTANTA";

            if (codJudet.Equals("14"))
                retVal = "COVASNA";

            if (codJudet.Equals("15"))
                retVal = "DAMBOVITA";

            if (codJudet.Equals("16"))
                retVal = "DOLJ";

            if (codJudet.Equals("17"))
                retVal = "GALATI";

            if (codJudet.Equals("52"))
                retVal = "GIURGIU";

            if (codJudet.Equals("18"))
                retVal = "GORJ";

            if (codJudet.Equals("19"))
                retVal = "HARGHITA";

            if (codJudet.Equals("20"))
                retVal = "HUNEDOARA";

            if (codJudet.Equals("21"))
                retVal = "IALOMITA";

            if (codJudet.Equals("22"))
                retVal = "IASI";

            if (codJudet.Equals("23"))
                retVal = "ILFOV";

            if (codJudet.Equals("24"))
                retVal = "MARAMURES";

            if (codJudet.Equals("25"))
                retVal = "MEHEDINTI";

            if (codJudet.Equals("26"))
                retVal = "MURES";

            if (codJudet.Equals("27"))
                retVal = "NEAMT";

            if (codJudet.Equals("28"))
                retVal = "OLT";

            if (codJudet.Equals("29"))
                retVal = "PRAHOVA";

            if (codJudet.Equals("31"))
                retVal = "SALAJ";

            if (codJudet.Equals("30"))
                retVal = "SATU-MARE";

            if (codJudet.Equals("32"))
                retVal = "SIBIU";

            if (codJudet.Equals("33"))
                retVal = "SUCEAVA";

            if (codJudet.Equals("34"))
                retVal = "TELEORMAN";

            if (codJudet.Equals("35"))
                retVal = "TIMIS";

            if (codJudet.Equals("36"))
                retVal = "TULCEA";

            if (codJudet.Equals("38"))
                retVal = "VALCEA";

            if (codJudet.Equals("37"))
                retVal = "VASLUI";

            if (codJudet.Equals("39"))
                retVal = "VRANCEA";

            return retVal;

        }

    }
}