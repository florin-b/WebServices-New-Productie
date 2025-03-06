using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using System.Xml;
using System.Data;
using System.Data.OracleClient;

namespace WebService1
{
    public class OperatiiPoligoane : System.Web.Services.WebService
    {


        public string getDatePoligonLivrareDB(string coords)
        {
            DatePoligon datePoligon = new DatePoligon("", "", "", "", "", "");
            datePoligon.isRestrictionat = Boolean.FalseString;

            try
            {
                bool punctInZona = false;

                LatLng addressPoint = new LatLng();
                addressPoint.lat = Double.Parse(coords.Split(',')[0]);
                addressPoint.lon = Double.Parse(coords.Split(',')[1]);

                List<Poligon> listPoligoaneZona = getListPoligoaneDB("ZM", "");

                foreach (Poligon poligon in listPoligoaneZona)
                {
                    if (punctInPoligonZonaDB(addressPoint, poligon, datePoligon))
                    {
                        punctInZona = true;
                        break;
                    }
                }

                if (!punctInZona)
                {
                    listPoligoaneZona = getListPoligoaneDB("ZEMA", "");

                    foreach (Poligon poligon in listPoligoaneZona)
                    {
                        if (punctInPoligonZonaDB(addressPoint, poligon, datePoligon))
                        {
                            punctInZona = true;
                            break;
                        }
                    }
                }

                if (!punctInZona)
                {
                    listPoligoaneZona = getListPoligoaneDB("ZEMB", "");

                    foreach (Poligon poligon in listPoligoaneZona)
                    {
                        if (punctInPoligonZonaDB(addressPoint, poligon, datePoligon))
                        {
                            break;
                        }
                    }
                }


                List<Poligon> listPoligoaneTonaj = getListPoligoaneDB("PERMIS", datePoligon.filialaPrincipala);

                foreach (Poligon poligon in listPoligoaneTonaj)
                {
                    if (punctInPoligonTonajDB(addressPoint, poligon, datePoligon))
                    {
                        datePoligon.isRestrictionat = poligon.interzis.Equals("X") ? Boolean.TrueString : Boolean.FalseString;
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


        public string getDatePoligonLivrare(string coords)
        {
            DatePoligon datePoligon = new DatePoligon("", "", "", "", "","");

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


        public List<LatLng> getCoordonatePoligonDB(string idPoligon)
        {
            List<LatLng> listCoords = new List<LatLng>();

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();
            connection.ConnectionString = connectionString;
            connection.Open();
            OracleCommand cmd = connection.CreateCommand();

            try
            {
                cmd.CommandText = " select latitude, longitude from sapprd.zpoligon_det where mandt = '900' and idpoligon=:idPoligon order by poz ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idPoligon", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idPoligon);

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        listCoords.Add(new LatLng(Double.Parse(oReader.GetString(0)), Double.Parse(oReader.GetString(1))));
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


            return listCoords;
        }

        public List<Poligon> getListPoligoaneDB(string filtruPoligon, string filiala)
        {
            List<Poligon> listPoligoane = new List<Poligon>();

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();
            connection.ConnectionString = connectionString;
            connection.Open();
            OracleCommand cmd = connection.CreateCommand();



            string conditieTip = "tip";
            string conditieFiliala = "";
            string infoTonaj = "";
            if (filtruPoligon.Equals("PERMIS"))
            {
                infoTonaj = " , name, lt, interzis ";
                conditieFiliala = " and pct =:filiala ";
                conditieTip = "tippoligon";
            }

            try
            {
                cmd.CommandText = " select idpoligon, pct, tip " + infoTonaj + " from sapprd.zpoligon_head where mandt = '900' and " + conditieTip + "=:tipPoligon " + conditieFiliala;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":tipPoligon", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filtruPoligon;

                if (filtruPoligon.Equals("PERMIS"))
                {
                    cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = filiala;
                }

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        Poligon poligon = new Poligon();
                        poligon.numeFisier = oReader.GetInt32(0).ToString();
                        poligon.filiala = oReader.GetString(1);
                        poligon.tipPoligon = oReader.GetString(2);
                        poligon.tonaj = "";
                        poligon.nume = "";
                        poligon.interzis = "";

                        if (filtruPoligon.Equals("PERMIS"))
                        {
                            poligon.nume = oReader.GetString(3);
                            poligon.tonaj = oReader.GetDouble(4).ToString();
                            poligon.interzis = oReader.GetString(5);
                            poligon.tipPoligon = "LT";
                        }

                        listPoligoane.Add(poligon);
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

            return listPoligoane;
        }

        private bool punctInPoligonZonaDB(LatLng punct, Poligon poligon, DatePoligon datePoligon)
        {
            bool inPoligon = false;

            List<LatLng> coordPoligon = getCoordonatePoligonDB(poligon.numeFisier);

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

        private bool punctInPoligonTonajDB(LatLng punct, Poligon poligon, DatePoligon datePoligon)
        {
            bool inPoligon = false;

            List<LatLng> coordPoligon = getCoordonatePoligonDB(poligon.numeFisier);
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