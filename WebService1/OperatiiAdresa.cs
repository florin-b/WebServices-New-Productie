using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using System.Web.Script.Serialization;
using System.Globalization;

namespace WebService1
{
    public class OperatiiAdresa
    {

        public String getLocalitatiJudet(String codJudet)
        {

            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " select upper(localitate) from sapprd.zlocalitati where bland=:codJudet order by localitate";

                cmd.CommandText = " select upper(a.localitate), nvl(b.oras,'-1') oras, nvl(b.razakm,-1) raza, nvl(c.latitudine,-1) lat, nvl(c.longitudine,-1) lon " +
                                  " from sapprd.zlocalitati a, sapprd.zoraseromania b, sapprd.zcoordlocalitati c " +
                                  " where a.mandt='900' and b.mandt(+) = '900' and c.mandt(+) = '900' and a.bland =:codJudet " +
                                  " and a.bland = b.codjudet(+) and a.localitate = b.oras(+) and c.judet(+) =  b.numejudet and c.localitate(+) = b.oras " +
                                  " order by a.localitate ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codJudet", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codJudet;


                oReader = cmd.ExecuteReader();

                List<Localitate> listLocalitati = new List<Localitate>();

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        Localitate loc = new Localitate();
                        loc.localitate = oReader.GetString(0);
                        loc.isOras = !oReader.GetString(1).Equals("-1");
                        loc.razaKm = oReader.GetInt32(2);
                        loc.coordonate = oReader.GetString(3) + "," + oReader.GetString(4);
                        listLocalitati.Add(loc);
                    }


                }





                if (oReader != null)
                {
                    oReader.Close();
                    oReader.Dispose();
                }

                cmd.Dispose();


                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listLocalitati);

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return serializedResult;

        }




        public string getAdreseJudet(String codJudet)
        {
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " select distinct mc_street from sapprd.m_strta where region =:codJudet order by mc_street ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codJudet", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codJudet;


                oReader = cmd.ExecuteReader();

                List<string> listStrazi = new List<string>();

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        listStrazi.Add(oReader.GetString(0));
                    }

                }

                if (oReader != null)
                {
                    oReader.Close();
                    oReader.Dispose();
                }

                cmd.Dispose();


                BeanAdreseJudet adreseJudet = new BeanAdreseJudet();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                adreseJudet.listLocalitati = getLocalitatiJudet(codJudet);
                adreseJudet.listStrazi = serializer.Serialize(listStrazi);

                serializedResult = serializer.Serialize(adreseJudet);

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return serializedResult;

        }



        public static void insereazaCoordonateAdresa(OracleConnection connection, string idComanda, string coordonate, string codJudet, string localitate)
        {

            string[] coords = coordonate.Split('#');

            if (coords[0].Equals("0") || coords[0].Equals("0.0"))
                return;

            string[] coordsLoc = getCoordsLocalitate(connection, codJudet, localitate).Split('#');

            if (!coordsLoc[0].Equals("-1"))
            {
                double distantaAdr = getDistanceMeters(Double.Parse(coordsLoc[0]), Double.Parse(coordsLoc[1]), Double.Parse(coords[0]), Double.Parse(coords[1]));

                if (distantaAdr > 5000)
                {
                    coords[0] = coordsLoc[0];
                    coords[1] = coordsLoc[1];
                }
            }

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " insert into sapprd.zcoordcomenzi(mandt, idcomanda, latitude, longitude) values ('900', :idComanda, :latitude, :longitude) ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                cmd.Parameters.Add(":latitude", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = coords[0];

                cmd.Parameters.Add(":longitude", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = coords[1];

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }

            return;


        }


        public static string getCoordsLocalitate(OracleConnection connection, string codJudet, string localitate)
        {
            string coords = "-1#-1";
            string numeJudet = getNumeJudet(codJudet);

            OracleCommand cmd = connection.CreateCommand();

            cmd.CommandText = " select count(*) from sapprd.zoraseromania where mandt='900' and codjudet=:codJudet and upper(oras)=:localitate ";

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Clear();

            cmd.Parameters.Add(":codJudet", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
            cmd.Parameters[0].Value = codJudet;

            cmd.Parameters.Add(":localitate", OracleType.VarChar, 75).Direction = ParameterDirection.Input;
            cmd.Parameters[1].Value = localitate.ToUpper();

            OracleDataReader oReader = cmd.ExecuteReader();
            oReader.Read();

            if (oReader.GetInt32(0) == 0)
            {
                cmd.CommandText = " select latitudine, longitudine from sapprd.zcoordlocalitati where mandt='900' and upper(judet)=:judet and upper(localitate)=:localitate ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":judet", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = getNumeJudet(codJudet).ToUpper();

                cmd.Parameters.Add(":localitate", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = localitate.ToUpper();

                OracleDataReader oReader1 = cmd.ExecuteReader();
                oReader1.Read();

                if (oReader1.HasRows)
                    coords = oReader1.GetString(0) + "#" + oReader1.GetString(1);

                oReader1.Close();
                oReader1.Dispose();

            }

            oReader.Close();
            oReader.Dispose();
            cmd.Dispose();

            return coords;
        }


        public static double getDistanceMeters(double longitude, double latitude, double otherLongitude, double otherLatitude)
        {
            var d1 = latitude * (Math.PI / 180.0);
            var num1 = longitude * (Math.PI / 180.0);
            var d2 = otherLatitude * (Math.PI / 180.0);
            var num2 = otherLongitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }


        public static void adaugaAdresaClient(OracleConnection connection, String idComanda, DateLivrare dateLivrare)
        {

            Adresa adresaComanda = new Adresa();

            try
            {

                ClientComanda clientComanda = OperatiiComenzi.getClientComanda(connection, idComanda);

                if (clientComanda.codClient != null)
                {
                    adresaComanda = OperatiiComenzi.getAdresaComanda(connection, idComanda, clientComanda.codAdresa);

                    if (dateLivrare.Strada == null || dateLivrare.Strada.Trim().Length == 0)
                        return;

                    List<Adresa> adreseClient = getAdreseClient(connection, clientComanda);


                    if (adresaComanda.latitude != null && !adresaInLista(clientComanda.codAdresa, adreseClient))
                        adaugaCodAdresa(connection, clientComanda, dateLivrare.coordonateGps);
                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }


        }

        private static bool adresaInLista(String codAdresa, List<Adresa> adreseClient)
        {
            foreach (Adresa adresa in adreseClient)
            {
                if (adresa.codAdresa.Equals(codAdresa))
                    return true;

            }

            return false;
        }

        private static bool adresaExista(Adresa adresaComanda, List<Adresa> adreseClient)
        {


            foreach (Adresa adresa in adreseClient)
            {
                double distAdrese = distanceXtoY(Double.Parse(adresaComanda.latitude), Double.Parse(adresaComanda.longitude), Double.Parse(adresa.latitude), Double.Parse(adresa.longitude), "K");

                if (distAdrese < 0.2)
                    return true;

            }

            return false;
        }



        private static List<Adresa> getAdreseClient(OracleConnection connection, ClientComanda clientComanda)
        {

            List<Adresa> adreseClient = new List<Adresa>();

            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {
                cmd = connection.CreateCommand();

                string sqlString = " select  a.latitude, a.longitude, a.codadresa  from sapprd.zadreseclienti a, sapprd.adrc b " +
                                   " where a.mandt = '900' and b.client = '900' and a.codadresa = b.addrnumber and a.codclient =:codClient ";

                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = clientComanda.codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        Adresa adresa = new Adresa();
                        adresa.latitude = oReader.GetString(0);
                        adresa.longitude = oReader.GetString(1);
                        adresa.codAdresa = oReader.GetString(2);
                        adreseClient.Add(adresa);

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

            return adreseClient;
        }


        private static double distanceXtoY(double lat1, double lon1, double lat2, double lon2, string unit)
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
            dist = dist * 60 * 1.1515;
            if (unit == "K")
            {
                dist = dist * 1.609344;
            }
            else if (unit == "N")
            {
                dist = dist * 0.8684;
            }
            return (dist);
        }


        private static double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private static double rad2deg(double rad)
        {
            return (rad * 180 / Math.PI);
        }


        private static void adaugaCodAdresa(OracleConnection connection, ClientComanda clientComanda, String coordonate)
        {

            String[] coords = coordonate.Split('#');

            if (coords[0].Equals("0") || coords[0].Equals("0.0"))
                return;

            OracleCommand cmd = null;
            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select 1 from sapprd.zadreseclienti where mandt='900' and codclient=:codClient and latitude=:latitude and longitude=:longitude ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = clientComanda.codClient;

                cmd.Parameters.Add(":latitude", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = coords[0];

                cmd.Parameters.Add(":longitude", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = coords[1];

                OracleDataReader oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Close();
                    oReader.Dispose();
                    cmd.Dispose();
                    return;
                }



                cmd.CommandText = " insert into sapprd.zadreseclienti(mandt, codclient, codadresa, latitude, longitude ) values ('900', :codClient, :codAdresa, :latitude, :longitude) ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = clientComanda.codClient;

                cmd.Parameters.Add(":codAdresa", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = clientComanda.codAdresa;

                cmd.Parameters.Add(":latitude", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = coords[0];

                cmd.Parameters.Add(":longitude", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = coords[1];

                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }


        }





        public bool isAdresaValida(string codJudet, string localitate)
        {

            bool isValid = false;

            Adresa adresa = new Adresa();
            adresa.codJudet = codJudet;
            adresa.localitate = localitate;

            OracleConnection connection = null;

            try
            {
                connection = new OracleConnection();

                OracleDataReader oReader = null;

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                cmd.CommandText = " select 1 from sapprd.zlocalitati where bland=:codJudet and lower(convert(localitate,'US7ASCII'))=:localitate ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codJudet", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = adresa.codJudet;

                cmd.Parameters.Add(":localitate", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = adresa.localitate.ToLower();


                oReader = cmd.ExecuteReader();

                List<string> listStrazi = new List<string>();

                if (oReader.HasRows)
                {
                    isValid = true;
                }

                if (oReader != null)
                {
                    oReader.Close();
                    oReader.Dispose();
                }

                
                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }

            return isValid;
        }



        public string getLocalitatiLivrareRapida()
        {

            string localitati = "Alba Iulia, Alexandria, Bacau, Bistrita, Botosani, Braila,Bucuresti,Voluntari, Pantelimon, Chiajna, Bragadiru, Magurele, Jilava, Berceni, Glina,Tunari, Otopeni, Brasov, Bucuresti, Buzau, Calarasi, Cluj-Napoca, Constanta, Deva, " +
                    "Drobeta Turnu Severin, Focsani, Galati, Gheorghieni, Giurgiu, Iasi, Medias, Miercurea Ciuc, Odorheiu Secuiesc, Onesti, Petrosani, Piatra-Neamt, " +
                    "Pitesti, Ploiesti, Ramnicu-Sarat, Ramnicu-Valcea, Resita, Roman, Satu-Mare, Sfantu Gheorghe, Sibiu, Slobozia, Slatina, Suceava, Targoviste, " +
                    "Targu-Jiu, Targu-Mures, Tulcea, Zalau, Sector 1, Sector 2, Sector 3, Sector 4, Sector 5, Sector 6";



            return localitati;

        }


        public static void actualizeazaCoordonateAdresa(OracleConnection connection, string idComanda, string coordonate)
        {

            String[] coords = coordonate.Split('#');

            if (coords[0].Equals("0") || coords[0].Equals("0.0"))
                return;

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " update sapprd.zcoordcomenzi set latitude =:latitude, longitude=:longitude where idcomanda =:idComanda ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":latitude", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = coords[0];

                cmd.Parameters.Add(":longitude", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = coords[1];

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = idComanda;

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }

            return;


        }


        public string getDateLivrareClient(string codClient)
        {

            DateLivrareClient dateLivrare = new DateLivrareClient();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string sqlString = " select c.region, c.city1, c.street, c.house_num1 num from sapprd.kna1 k, sapprd.adrc c " +
                                   " where k.mandt = '900' and k.kunnr = :codClient  and k.mandt = c.client and k.adrnr = c.addrnumber ";

                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    dateLivrare.codJudet = oReader.GetString(0);
                    dateLivrare.localitate = oReader.GetString(1);
                    dateLivrare.strada = oReader.GetString(2);
                    dateLivrare.nrStrada = oReader.GetString(3);
                }

                cmd.CommandText = " select namev, name1, telf1 from sapprd.knvk u where u.mandt = '900' and " +
                                  " (u.parnr, u.kunnr) in (select distinct p.parnr, p.kunnr from sapprd.knvp p where p.mandt = '900' " +
                                  " and p.kunnr =:k and parvw = 'AP' and vtweg = '20' )";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    dateLivrare.numePersContact = oReader.GetString(0) + " " + oReader.GetString(1);
                    dateLivrare.telPersContact = oReader.GetString(2);
                }

                cmd.CommandText = " select u.zterm from sapprd.T052u u, sapprd.TVZBT t where u.mandt = '900' and " +
                                  " u.spras = '4' and u.mandt = t.mandt and u.spras = t.spras and u.zterm = t.zterm " +
                                  " and u.zterm <= (select max(p.zterm) from sapprd.knvv p where p.mandt = '900' " +
                                  " and p.kunnr =:k and p.vtweg = '20' and p.spart = '11') and u.zterm like 'C%' order by u.zterm ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        dateLivrare.termenPlata = oReader.GetString(0).ToString() + ";";
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

            return new JavaScriptSerializer().Serialize(dateLivrare);
        }

        public string getFilialaJudetLivrare(string codJudet)
        {

            string filiala = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " SELECT werks FROM sapprd.ZPDL_JUD WHERE mandt = '900' AND regio=:codJudet ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codJudet", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codJudet;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        if (filiala.Equals(String.Empty))
                            filiala = oReader.GetString(0);
                        else
                            filiala += "," + oReader.GetString(0);

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

            return filiala;

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