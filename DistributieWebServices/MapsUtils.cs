using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace DistributieWebServices
{
    public class MapsUtils
    {
        private static string API_KEY = "AIzaSyBacSk9khZt7CGoqPe9UZFJGQAjWymAmBg";

        public static string geocodeAddress(string judet, string localitate, string strada)
        {

            string address = "Romania, " + judet + ", " + localitate + ", " + strada;

            string requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?key={1}&address={0}&sensor=false", Uri.EscapeDataString(address), API_KEY);

            WebRequest request = WebRequest.Create(requestUri);
            WebResponse response = request.GetResponse();

            System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);

            XDocument xdoc = XDocument.Parse(sr.ReadToEnd().Trim());

            XElement result = xdoc.Element("GeocodeResponse").Element("result");
            XElement locationElement = result.Element("geometry").Element("location");
            XElement lat = locationElement.Element("lat");
            XElement lng = locationElement.Element("lng");

            return lat.Value + "#" + lng.Value;
        }


        public static string getCoordonateClient(OracleConnection connection, string codAdresa)
        {

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string judet = "", localitate = "", strada = "";

            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select region, city1, street from sapprd.adrc where client = '900' and addrnumber = :adrnumber ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":adrnumber", OracleType.VarChar, 30).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = codAdresa;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        judet = getNumeJudet(oReader.GetString(0));
                        localitate = oReader.GetString(1);
                        strada = oReader.GetString(2);
                    }
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

            return MapsUtils.geocodeAddress(judet, localitate, strada);
        }


        private static string getNumeJudet(string codJudet)
        {
            String retVal = "Nedefinit";

            if (codJudet.Equals("01"))
                retVal = "ALBA";

            else if (codJudet.Equals("02"))
                retVal = "ARAD";

            else if (codJudet.Equals("03"))
                retVal = "ARGES";

            else if (codJudet.Equals("04"))
                retVal = "BACAU";

            else if (codJudet.Equals("05"))
                retVal = "BIHOR";

            else if (codJudet.Equals("06"))
                retVal = "BISTRITA-NASAUD";

            else if (codJudet.Equals("07"))
                retVal = "BOTOSANI";

            else if (codJudet.Equals("09"))
                retVal = "BRAILA";

            else if (codJudet.Equals("08"))
                retVal = "BRASOV";

            else if (codJudet.Equals("40"))
                retVal = "BUCURESTI";

            else if (codJudet.Equals("10"))
                retVal = "BUZAU";

            else if (codJudet.Equals("51"))
                retVal = "CALARASI";

            else if (codJudet.Equals("11"))
                retVal = "CARAS-SEVERIN";

            else if (codJudet.Equals("12"))
                retVal = "CLUJ";

            else if (codJudet.Equals("13"))
                retVal = "CONSTANTA";

            else if (codJudet.Equals("14"))
                retVal = "COVASNA";

            else if (codJudet.Equals("15"))
                retVal = "DAMBOVITA";

            else if (codJudet.Equals("16"))
                retVal = "DOLJ";

            else if (codJudet.Equals("17"))
                retVal = "GALATI";

            else if (codJudet.Equals("52"))
                retVal = "GIURGIU";

            else if (codJudet.Equals("18"))
                retVal = "GORJ";

            else if (codJudet.Equals("19"))
                retVal = "HARGHITA";

            else if (codJudet.Equals("20"))
                retVal = "HUNEDOARA";

            else if (codJudet.Equals("21"))
                retVal = "IALOMITA";

            else if (codJudet.Equals("22"))
                retVal = "IASI";

            else if (codJudet.Equals("23"))
                retVal = "ILFOV";

            else if (codJudet.Equals("24"))
                retVal = "MARAMURES";

            else if (codJudet.Equals("25"))
                retVal = "MEHEDINTI";

            else if (codJudet.Equals("26"))
                retVal = "MURES";

            else if (codJudet.Equals("27"))
                retVal = "NEAMT";

            else if (codJudet.Equals("28"))
                retVal = "OLT";

            else if (codJudet.Equals("29"))
                retVal = "PRAHOVA";

            else if (codJudet.Equals("31"))
                retVal = "SALAJ";

            else if (codJudet.Equals("30"))
                retVal = "SATU-MARE";

            else if (codJudet.Equals("32"))
                retVal = "SIBIU";

            else if (codJudet.Equals("33"))
                retVal = "SUCEAVA";

            else if (codJudet.Equals("34"))
                retVal = "TELEORMAN";

            else if (codJudet.Equals("35"))
                retVal = "TIMIS";

            else if (codJudet.Equals("36"))
                retVal = "TULCEA";

            else if (codJudet.Equals("38"))
                retVal = "VALCEA";

            else if (codJudet.Equals("37"))
                retVal = "VASLUI";

            else if (codJudet.Equals("39"))
                retVal = "VRANCEA";

            return retVal;
        }


    }
}