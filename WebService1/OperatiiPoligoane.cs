using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using System.Xml;

namespace WebService1
{
    public class OperatiiPoligoane : System.Web.Services.WebService
    {
        public string getDatePoligonLivrare(string coords)
        {
            DatePoligon datePoligon = new DatePoligon("", "", "", "", "");

            try
            {
                bool punctInZona = false;

                LatLng addressPoint = new LatLng();
                addressPoint.lat = Double.Parse(coords.Split(',')[0]);
                addressPoint.lon = Double.Parse(coords.Split(',')[1]);

                List<Poligon> listPoligoaneZona = getListPoligoane("ZM", "");

                foreach (Poligon poligon in listPoligoaneZona)
                {
                    if (punctInPoligonZona(addressPoint, poligon, datePoligon))
                    {
                        punctInZona = true;
                        break;
                    }
                }

                if (!punctInZona)
                {
                    listPoligoaneZona = getListPoligoane("ZEMA", "");

                    foreach (Poligon poligon in listPoligoaneZona)
                    {
                        if (punctInPoligonZona(addressPoint, poligon, datePoligon))
                        {
                            punctInZona = true;
                            break;
                        }
                    }
                }

                if (!punctInZona)
                {
                    listPoligoaneZona = getListPoligoane("ZEMB", "");

                    foreach (Poligon poligon in listPoligoaneZona)
                    {
                        if (punctInPoligonZona(addressPoint, poligon, datePoligon))
                        {
                            break;
                        }
                    }
                }


                List<Poligon> listPoligoaneTonaj = getListPoligoane("LT", datePoligon.filialaPrincipala);

                foreach (Poligon poligon in listPoligoaneTonaj)
                {
                    if (punctInPoligonTonaj(addressPoint, poligon, datePoligon))
                    {
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " -> " + coords);
            }

            return new JavaScriptSerializer().Serialize(datePoligon);

        }

        private List<LatLng> getCoordonatePoligon(string numeFisier)
        {

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Server.MapPath(General.Constants.GPS_DATA_FOLDER + "/" + numeFisier));

            XmlNodeList idNodes = xmlDoc.SelectNodes("gpx/trk/trkseg/trkpt");
            List<LatLng> listCoords = new List<LatLng>();

            foreach (XmlNode node in idNodes)
            {
                listCoords.Add(generateLatLngObject(node.OuterXml));
            }

            return listCoords;

        }


        private List<Poligon> getListPoligoane(string filtruPoligon, string filiala)
        {
            List<Poligon> listPoligoane = new List<Poligon>();

            DirectoryInfo d = new DirectoryInfo(Server.MapPath(General.Constants.GPS_DATA_FOLDER));
            FileInfo[] gpxFiles = d.GetFiles("*.gpx");

            XmlDocument xmlDoc = new XmlDocument();

            foreach (FileInfo file in gpxFiles)
            {
                xmlDoc.Load(Server.MapPath(General.Constants.GPS_DATA_FOLDER + "/" + file));
                Poligon poligon = new Poligon();

                if (xmlDoc.SelectSingleNode("gpx/tip") != null)
                    poligon.tipPoligon = xmlDoc.SelectSingleNode("gpx/tip").InnerText;
                else
                    poligon.tipPoligon = "LT";

                poligon.filiala = xmlDoc.SelectSingleNode("gpx/pct").InnerText;
                poligon.numeFisier = file.ToString();
                poligon.tonaj = "";
                poligon.nume = "";

                if (poligon.tipPoligon.Equals("LT"))
                {
                    poligon.tonaj = xmlDoc.SelectSingleNode("gpx/LT").InnerText;
                    poligon.nume = xmlDoc.SelectSingleNode("gpx/name").InnerText;
                }

                bool conditiePoligon = filtruPoligon.ToLower().Contains(poligon.tipPoligon.ToLower());

                if (filiala.Trim().Length != 0)
                    conditiePoligon = filtruPoligon.ToLower().Contains(poligon.tipPoligon.ToLower()) && poligon.filiala.Equals(filiala);

                if (conditiePoligon)
                    listPoligoane.Add(poligon);
            }

            return listPoligoane;
        }


        private LatLng generateLatLngObject(string coords)
        {
            LatLng latLng = new LatLng();

            string[] coordsArray = coords.Replace('<', ' ').Replace('>', ' ').Split(' ');

            foreach (string xmlRow in coordsArray)
            {
                if (xmlRow.ToLower().StartsWith("lat"))
                {
                    latLng.lat = Double.Parse(xmlRow.ToLower().Replace("lat", "").Replace("=", "").Replace("\\", "").Replace("\"", "").Trim());
                }

                if (xmlRow.ToLower().StartsWith("lon"))
                {
                    latLng.lon = Double.Parse(xmlRow.ToLower().Replace("lon", "").Replace("=", "").Replace("\\", "").Replace("\"", "").Trim());
                }
            }

            return latLng;
        }


        private bool punctInPoligonZona(LatLng punct, Poligon poligon, DatePoligon datePoligon)
        {
            bool inPoligon = false;

            List<LatLng> coordPoligon = getCoordonatePoligon(poligon.numeFisier);

            bool contains = ModelPoligoane.containsPoint(punct, coordPoligon, true);

            if (contains)
            {
                if (poligon.filiala.Contains(","))
                {
                    string[] arrayFiliale = poligon.filiala.Split(',');
                    datePoligon.filialaPrincipala = arrayFiliale[0].Trim();
                    datePoligon.filialaSecundara = arrayFiliale[1].Trim();
                }
                else {
                    datePoligon.filialaPrincipala = poligon.filiala.Trim();
                    datePoligon.filialaSecundara = "";
                }

                datePoligon.tipZona = poligon.tipPoligon;
                inPoligon = true;
            }

            return inPoligon;
        }


        private bool punctInPoligonTonaj(LatLng punct, Poligon poligon, DatePoligon datePoligon)
        {
            bool inPoligon = false;

            List<LatLng> coordPoligon = getCoordonatePoligon(poligon.numeFisier);
            bool contains = ModelPoligoane.containsPoint(punct, coordPoligon, true);

            if (contains)
            {
                datePoligon.limitareTonaj = poligon.tonaj;
                datePoligon.nume = poligon.nume;
                inPoligon = true;
            }

            return inPoligon;
        }
    }
}