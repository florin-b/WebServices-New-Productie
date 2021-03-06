using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;


namespace WebService1
{
    public class OperatiiMathaus
    {

        public string getListaCategorii()
        {
            List<CategorieMathaus> listCategorii = new List<CategorieMathaus>();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select cod, nume, cod_hybris, nvl(cod_parinte,'') from sapprd.zcatmathaus order by cod ";
                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        CategorieMathaus cat = new CategorieMathaus();
                        cat.cod = oReader.GetString(0);
                        cat.nume = oReader.GetString(1);
                        cat.codHybris = oReader.GetString(2);
                        cat.codParinte = oReader.GetString(3);
                        listCategorii.Add(cat);

                    }
                }

                CategorieMathaus cat1 = new CategorieMathaus();
                cat1.cod = "0";
                cat1.nume = "Diverse";
                cat1.codHybris = "0";
                cat1.codParinte = "1";
                listCategorii.Add(cat1);


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return new JavaScriptSerializer().Serialize(listCategorii);
        }



        public string getArticoleCategorie(string codCategorie, string filiala, string depart, string pagina, string tipArticol)
        {

            RezultatArtMathaus rezultat = new RezultatArtMathaus();


            if (codCategorie.Equals("0"))
                rezultat = getArticoleLocal(filiala, depart, pagina);
            else {
                if (tipArticol == null || (tipArticol != null && tipArticol.Equals("SITE")))
                    rezultat = getArticoleCategorie(codCategorie, filiala, depart, pagina);
                else
                    rezultat = getArticoleND(filiala, codCategorie, pagina);
            }


            if (rezultat.listArticole.Count > 0)
                addExtraData(rezultat.listArticole, filiala);

            return new JavaScriptSerializer().Serialize(rezultat);
        }


        private int getNrArticoleCategorie(string codCategorie, string filiala, string depart)
        {


            int nrArticole = 0;

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();
            try
            {

                cmd.CommandText = " select count(distinct s.matnr)  " +
                                  " from sapprd.zpath_hybris s, sapprd.marc c, websap.articole ar, sapprd.mvke e " +
                                  " where (s.nivel_0 = :codCateg or s.nivel_1 = :codCateg or s.nivel_2 = :codCateg or " +
                                  " s.nivel_3 = :codCateg or s.nivel_4 = :codCateg or s.nivel_5 = :codCateg or s.nivel_6 = :codCateg) " +
                                  " and (substr(ar.grup_vz, 0, 2) = :depart or ar.grup_vz = '11') " +
                                  " and ar.cod = s.matnr and s.mandt = c.mandt and s.matnr = c.matnr " +
                                  " and e.mandt = '900' and e.matnr = s.matnr and e.vtweg = '20' ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codCateg", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codCategorie;

                cmd.Parameters.Add(":depart", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = depart;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        nrArticole = oReader.GetInt32(0);
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

            return nrArticole;
        }

        public RezultatArtMathaus getArticoleCategorie(string codCategorie, string filiala, string depart, string pagina)
        {





            RezultatArtMathaus rezultat = new RezultatArtMathaus();

            string paginaCrt = ((Int32.Parse(pagina) - 1) * 10).ToString();

            List<ArticolMathaus> listArticole = new List<ArticolMathaus>();

            rezultat.nrTotalArticole = getNrArticoleCategorie(codCategorie, filiala, depart).ToString();

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();
            try
            {



                cmd.CommandText = " select distinct s.matnr, ar.nume,  e.versg " +
                                  " from sapprd.zpath_hybris s, sapprd.marc c, websap.articole ar, sapprd.mvke e " +
                                  " where (s.nivel_0 = :codCateg or s.nivel_1 = :codCateg or s.nivel_2 = :codCateg or " +
                                  " s.nivel_3 = :codCateg or s.nivel_4 = :codCateg or s.nivel_5 = :codCateg or s.nivel_6 = :codCateg) " +
                                  " and (substr(ar.grup_vz, 0, 2) in " + HelperComenzi.getDepartExtra(depart) + " or ar.grup_vz = '11') " +
                                  " and ar.cod = s.matnr and s.mandt = c.mandt and s.matnr = c.matnr " +
                                  " and e.mandt = '900' and e.matnr = s.matnr and e.vtweg = '20' " +
                                  " order by s.matnr OFFSET :paginaCrt ROWS FETCH NEXT 10 ROWS ONLY ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codCateg", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codCategorie;

                cmd.Parameters.Add(":paginaCrt", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = paginaCrt;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ArticolMathaus articol = new ArticolMathaus();
                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        articol.tip1 = "";
                        articol.tip2 = oReader.GetString(2);
                        articol.isLocal = true;
                        articol.isArticolSite = false;
                        setDetaliiArticol(articol);
                        listArticole.Add(articol);
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

            rezultat.listArticole = listArticole;


            return rezultat;
        }


       
        private int getNrArticoleND(string filiala, string codCategorie)
        {
            int nrArticole = 0;

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {
                cmd.CommandText = " select count(distinct s.matnr) from sapprd.zpath_hybris s, sapprd.marc c, sapprd.zstoc_job b, websap.articole a " +
                                  " where(s.nivel_0 = :codCateg or s.nivel_1 = :codCateg or s.nivel_2 = :codCateg or s.nivel_3 = :codCateg or s.nivel_4 = :codCateg or s.nivel_5 = :codCateg or s.nivel_6 = :codCateg) " +
                                  " and s.mandt = c.mandt and s.matnr = c.matnr and c.werks = :filiala and c.dismm = 'ND' and b.mandt = c.mandt " +
                                  " and b.matnr = s.matnr and b.werks = c.werks and b.stocne > 0 and c.matnr = a.cod ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codCateg", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codCategorie;

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filiala;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        nrArticole = oReader.GetInt32(0);
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


            return nrArticole;
        }

        
        private RezultatArtMathaus getArticoleND(string filiala, string codCategorie, string pagina)
        {
            RezultatArtMathaus rezultat = new RezultatArtMathaus();

            string paginaMin = ((Int32.Parse(pagina) - 1) * 10).ToString();
            string paginaMax = (Int32.Parse(pagina) * 10).ToString();

            List<ArticolMathaus> listArticole = new List<ArticolMathaus>();

            rezultat.nrTotalArticole = getNrArticoleND(filiala, codCategorie).ToString();

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {
                cmd.CommandText = " select * from (" +
                                  " select distinct s.matnr, a.nume, row_number() over (ORDER BY s.matnr ASC) line_number from sapprd.zpath_hybris s, sapprd.marc c, sapprd.zstoc_job b, websap.articole a " +
                                  " where(s.nivel_0 = :codCateg or s.nivel_1 = :codCateg or s.nivel_2 = :codCateg or s.nivel_3 = :codCateg or s.nivel_4 = :codCateg or s.nivel_5 = :codCateg or s.nivel_6 = :codCateg) " +
                                  " and s.mandt = c.mandt and s.matnr = c.matnr and c.werks = :filiala and c.dismm = 'ND' and b.mandt = c.mandt " +
                                  " and b.matnr = s.matnr and b.werks = c.werks and b.stocne > 0 and c.matnr = a.cod order by s.matnr ) " +
                                  " where line_number between :pageMin and :pageMax order by line_number ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codCateg", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codCategorie;

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filiala;

                cmd.Parameters.Add(":pageMin", OracleType.VarChar, 2).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = paginaMin;

                cmd.Parameters.Add(":pageMax", OracleType.VarChar, 2).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = paginaMax;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ArticolMathaus articol = new ArticolMathaus();
                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        articol.isLocal = true;
                        articol.isArticolSite = false;
                        setDetaliiArticol(articol);
                        listArticole.Add(articol);
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

            rezultat.listArticole = listArticole;

            return rezultat;
        }


        private int getNrArticoleLocal(string filiala, string depart, string pagina)
        {

            int nrArticole = 0;

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " select count(distinct c.matnr) from sapprd.marc c, sapprd.zstoc_job b, websap.articole a, websap.sintetice g " +
                                  " where c.mandt = '900' and c.werks = :filiala and c.dismm = 'ND' and b.mandt = c.mandt and b.matnr = c.matnr " +
                                  " and b.werks = c.werks and b.stocne > 0 and not exists " +
                                  " (select * from sapprd.zpath_hybris s where s.mandt = '900' and s.matnr = c.matnr) and c.matnr = a.cod " +
                                  " and a.sintetic = g.cod and (substr(a.grup_vz,0,2) =:depart or a.grup_vz = '11' ) order by c.matnr ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                cmd.Parameters.Add(":depart", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = depart.Substring(0, 2);

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        nrArticole = oReader.GetInt32(0);
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

            return nrArticole;
        }

        //neclasificate
        private RezultatArtMathaus getArticoleLocal(string filiala, string depart, string pagina)
        {

            RezultatArtMathaus rezultat = new RezultatArtMathaus();
            rezultat.nrTotalArticole = getNrArticoleLocal(filiala, depart, pagina).ToString();

            List<ArticolMathaus> listArticole = new List<ArticolMathaus>();

            string paginaMin = ((Int32.Parse(pagina) - 1) * 10).ToString();
            string paginaMax = (Int32.Parse(pagina) * 10).ToString();


            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " select * from (" +
                                  " select distinct c.matnr, a.nume, row_number() over (ORDER BY c.matnr ASC) line_number from sapprd.marc c, sapprd.zstoc_job b, websap.articole a, websap.sintetice g " +
                                  " where c.mandt = '900' and c.werks = :filiala and c.dismm = 'ND' and b.mandt = c.mandt and b.matnr = c.matnr " +
                                  " and b.werks = c.werks and b.stocne > 0 and not exists " +
                                  " (select * from sapprd.zpath_hybris s where s.mandt = '900' and s.matnr = c.matnr) and c.matnr = a.cod " +
                                  " and a.sintetic = g.cod and (substr(a.grup_vz,0,2) =:depart or a.grup_vz = '11' ) order by c.matnr ) " +
                                  " where line_number between :pageMin and :pageMax order by line_number ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                cmd.Parameters.Add(":depart", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = depart.Substring(0, 2);


                cmd.Parameters.Add(":pageMin", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = paginaMin;

                cmd.Parameters.Add(":pageMax", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = paginaMax;


                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ArticolMathaus articol = new ArticolMathaus();
                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        articol.isLocal = true;
                        articol.isArticolSite = false;
                        setDetaliiArticol(articol);
                        listArticole.Add(articol);
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

            rezultat.listArticole = listArticole;

            return rezultat;
        }


        private RezultatArtMathaus getArticoleWebService(string codCategorie, string depart, string urlService, string pagina)
        {

            RezultatArtMathaus rezultat = new RezultatArtMathaus();

            List<ArticolMathaus> listArticole = new List<ArticolMathaus>();

            int paginaCurenta = (Int32.Parse(pagina) - 1) * 10;

            string paginare = "&rows=10&start=" + paginaCurenta;

            string serviceUrl = "https://idx.arabesque.ro/solr/master_erp_Product_default/select?q=categoryCode_string_mv:" + codCategorie + paginare;

            if (urlService != null && !urlService.Equals(""))
                serviceUrl = urlService + paginare;

            ErrorHandling.sendErrorToMail("getArticoleWebService: " + serviceUrl);

            System.Net.ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            System.Net.WebRequest request = System.Net.WebRequest.Create(serviceUrl);

            CredentialCache credential = new CredentialCache();
            credential.Add(new System.Uri(serviceUrl), "Basic", new System.Net.NetworkCredential("erpClient", "S3EjkNEm"));
            request.Credentials = credential;

            System.Net.WebResponse response = request.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());

            string jsonResponse = sr.ReadToEnd().Trim();

            int docsFoundStart = jsonResponse.IndexOf("numFound\":");

            int docsFoundStop = jsonResponse.IndexOf("start\":", docsFoundStart) - 1;

            string nrDocs = jsonResponse.Substring(docsFoundStart, docsFoundStop - docsFoundStart - 1).Split(':')[1];

            rezultat.nrTotalArticole = nrDocs;

            int startResponse = jsonResponse.IndexOf("docs\":[") + 7;

            jsonResponse = jsonResponse.Substring(startResponse, jsonResponse.Length - startResponse - 1);

            string[] articole = Regex.Split(jsonResponse, "\"id\":");
            string divizieArt = "";

            foreach (string art in articole)
            {

                string[] artData = Regex.Split(art, "\",");
                ArticolMathaus articol = new ArticolMathaus();

                foreach (string data in artData)
                {

                    if (data.Contains("code_string"))
                    {
                        articol.cod = data.Split(':')[1].Replace("\"", "");
                    }

                    if (data.Contains("name_text_ro"))
                    {
                        articol.nume = data.Split(':')[1].Replace("\"", "").ToUpper();
                    }

                    if (data.Contains("image_m_string"))
                    {
                        articol.adresaImg = "https" + Regex.Split(data, "https")[1].Replace("\"", "");
                    }

                    if (data.Contains("image_l_string"))
                    {
                        articol.adresaImgMare = "https" + Regex.Split(data, "https")[1].Replace("\"", "");
                    }

                    if (data.Contains("description_text_ro"))
                    {
                        articol.descriere = Regex.Replace(Regex.Split(data.Trim(), "\":\"")[1].Replace("\"", "").Replace("\\n", " ").Replace("\\t", " ").Replace("&nbsp;", " "), "<.*?>", String.Empty);
                    }

                    if (data.Contains("divizie_string"))
                    {
                        divizieArt = data.Split(':')[1].Replace("\"", "");
                    }

                }
                if (articol.nume != null && (divizieArt.Equals(depart) || divizieArt.Equals("11")))
                {
                    if (articol.descriere == null)
                        articol.descriere = " ";

                    articol.isLocal = false;
                    articol.isArticolSite = true;
                    listArticole.Add(articol);
                }

            }

            rezultat.listArticole = listArticole;

            return rezultat;
        }



        public int getNrArticoleCautare(string codArticol, string tipCautare, string filiala, string depart)
        {

            int nrArticole = 0;

            RezultatArtMathaus rezultat = new RezultatArtMathaus();


            List<ArticolMathaus> listArticole = new List<ArticolMathaus>();

            string cautare;
            if (tipCautare.Equals("c"))
                cautare = " and lower(ar.cod) like '0000000000" + codArticol.ToLower() + "%'";
            else
                cautare = " and lower(ar.nume) like '" + codArticol.ToLower() + "%'";



            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();
            try
            {
                cmd.CommandText = " select count (distinct s.matnr) " +
                                  " from sapprd.zpath_hybris s, sapprd.marc c, articole ar " +
                                  " where (substr(ar.grup_vz,0,2) =:depart or ar.grup_vz = '11' ) " +
                                  " and ar.cod = s.matnr and s.mandt = c.mandt and s.matnr = c.matnr and c.werks = :filiala " + cautare + "  ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                cmd.Parameters.Add(":depart", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = depart;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        nrArticole = oReader.GetInt32(0);
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

            rezultat.listArticole = listArticole;

            return nrArticole;
        }




        public string cautaArticoleMathaus(string codArticol, string tipCautare, string filiala, string depart, string pagina)
        {

            RezultatArtMathaus rezultat = new RezultatArtMathaus();

            string paginaCrt = ((Int32.Parse(pagina) - 1) * 10).ToString();

            List<ArticolMathaus> listArticole = new List<ArticolMathaus>();

            string cautare;
            if (tipCautare.Equals("c"))
                cautare = " and lower(ar.cod) like '0000000000" + codArticol.ToLower() + "%'";
            else
                cautare = " and lower(ar.nume) like '" + codArticol.ToLower() + "%'";

            rezultat.nrTotalArticole = getNrArticoleCautare(codArticol, tipCautare, filiala, depart).ToString();

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();
            try
            {
                cmd.CommandText = " select distinct s.matnr, ar.nume, c.dismm, " +
                                  " (select e.versg from sapprd.mvke e where e.mandt = '900' and e.matnr = s.matnr and e.vtweg = '20') par_s " +
                                  " from sapprd.zpath_hybris s, sapprd.marc c, articole ar " +
                                  " where (substr(ar.grup_vz,0,2) in " + HelperComenzi.getDepartExtra(depart) + " or ar.grup_vz = '11' ) " +
                                  " and ar.cod = s.matnr and s.mandt = c.mandt and s.matnr = c.matnr and c.werks = :filiala " + cautare + " order by s.matnr  " +
                                  " OFFSET :paginaCrt ROWS FETCH NEXT 10 ROWS ONLY ";




                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                cmd.Parameters.Add(":paginaCrt", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = paginaCrt;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ArticolMathaus articol = new ArticolMathaus();
                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        articol.tip1 = oReader.GetString(2);
                        articol.tip2 = oReader.GetString(3);
                        articol.isLocal = true;
                        articol.isArticolSite = false;
                        setDetaliiArticol(articol);
                        listArticole.Add(articol);
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

            rezultat.listArticole = listArticole;

            if (rezultat.listArticole.Count > 0)
                addExtraData(rezultat.listArticole, filiala);


            return new JavaScriptSerializer().Serialize(rezultat);
        }



        private int getNrCautaArticoleLocal(string codArticol, string tipCautare, string filiala, string depart)
        {
            int nrArticole = 0;

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string cautare;

            if (tipCautare.Equals("c"))
                cautare = " and lower(a.cod) like '0000000000" + codArticol.ToLower() + "%'";
            else
                cautare = " and lower(a.nume) like '" + codArticol.ToLower() + "%'";

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " select count(distinct c.matnr) from sapprd.marc c, sapprd.zstoc_job b, websap.articole a, websap.sintetice g " +
                                  " where c.mandt = '900' and c.werks = :filiala and c.dismm = 'ND' and b.mandt = c.mandt and b.matnr = c.matnr " +
                                  " and b.werks = c.werks and b.stocne > 0 and not exists " +
                                  " (select * from sapprd.zpath_hybris s where s.mandt = '900' and s.matnr = c.matnr) and c.matnr = a.cod " + cautare +
                                  " and a.sintetic = g.cod and (substr(a.grup_vz,0,2) =:depart or a.grup_vz = '11' ) order by c.matnr ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                cmd.Parameters.Add(":depart", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = depart.Substring(0, 2);

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        nrArticole = oReader.GetInt32(0);
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

            return nrArticole;
        }



        public string cautaArticoleLocal(string codArticol, string tipCautare, string filiala, string depart, string pagina)
        {

            RezultatArtMathaus rezultat = new RezultatArtMathaus();
            rezultat.nrTotalArticole = getNrCautaArticoleLocal(codArticol, tipCautare, filiala, depart).ToString().ToString();

            List<ArticolMathaus> listArticole = new List<ArticolMathaus>();

            string paginaMin = ((Int32.Parse(pagina) - 1) * 10).ToString();
            string paginaMax = (Int32.Parse(pagina) * 10).ToString();

            string cautare;
            if (tipCautare.Equals("c"))
                cautare = " and lower(a.cod) like '0000000000" + codArticol.ToLower() + "%'";
            else
                cautare = " and lower(a.nume) like '" + codArticol.ToLower() + "%'";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " select * from (" +
                                  " select distinct c.matnr, a.nume, row_number() over (ORDER BY c.matnr ASC) line_number from sapprd.marc c, sapprd.zstoc_job b, websap.articole a, websap.sintetice g " +
                                  " where c.mandt = '900' and c.werks = :filiala and c.dismm = 'ND' and b.mandt = c.mandt and b.matnr = c.matnr " +
                                  " and b.werks = c.werks and b.stocne > 0 and not exists " +
                                  " (select * from sapprd.zpath_hybris s where s.mandt = '900' and s.matnr = c.matnr) and c.matnr = a.cod " + cautare +
                                  " and a.sintetic = g.cod and (substr(a.grup_vz,0,2) =:depart or a.grup_vz = '11' ) order by c.matnr ) " +
                                  " where line_number between :pageMin and :pageMax order by line_number ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                cmd.Parameters.Add(":depart", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = depart.Substring(0, 2);

                cmd.Parameters.Add(":pageMin", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = paginaMin;

                cmd.Parameters.Add(":pageMax", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = paginaMax;


                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ArticolMathaus articol = new ArticolMathaus();
                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        articol.isLocal = true;
                        articol.isArticolSite = false;
                        setDetaliiArticol(articol);
                        listArticole.Add(articol);
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

            rezultat.listArticole = listArticole;

            return new JavaScriptSerializer().Serialize(rezultat);
        }



        private void setDetaliiArticol(ArticolMathaus articol)
        {

            string serviceUrl = "https://wse1-sap-prod.arabesque.ro/solr/master_erp_Product_default/select?q=code_string:" + articol.cod;

            System.Net.ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            System.Net.WebRequest request = System.Net.WebRequest.Create(serviceUrl);

            CredentialCache credential = new CredentialCache();
            credential.Add(new System.Uri(serviceUrl), "Basic", new System.Net.NetworkCredential("erpClient", "S3EjkNEm"));
            request.Credentials = credential;

            System.Net.WebResponse response = request.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());

            string jsonResponse = sr.ReadToEnd().Trim();

            int startResponse = jsonResponse.IndexOf("docs\":[") + 7;

            jsonResponse = jsonResponse.Substring(startResponse, jsonResponse.Length - startResponse - 1);

            string[] articole = Regex.Split(jsonResponse, "\"id\":");

            foreach (string art in articole)
            {

                string[] artData = Regex.Split(art, "\",");

                foreach (string data in artData)
                {

                    if (data.Contains("image_m_string"))
                    {
                        articol.adresaImg = "https" + Regex.Split(data, "https")[1].Replace("\"", "");
                    }

                    if (data.Contains("image_l_string"))
                    {
                        articol.adresaImgMare = "https" + Regex.Split(data, "https")[1].Replace("\"", "");
                    }

                    if (data.Contains("description_text_ro"))
                    {
                        articol.descriere = Regex.Replace(Regex.Split(data.Trim(), "\":\"")[1].Replace("\"", "").Replace("\\n", " ").Replace("\\t", " ").Replace("&nbsp;", " "), "<.*?>", String.Empty);
                    }

                }
            }

        }


        private void addExtraData(List<ArticolMathaus> listArticole, string filiala)
        {

            string listCodArt = "";
            string filialaGed = filiala.Substring(0, 2) + "2" + filiala.Substring(3, 1);

            string magMathaus = filiala;

            List<ArticolMathaus> eliminate = new List<ArticolMathaus>();

            foreach (ArticolMathaus articol in listArticole)
            {
                if (listCodArt.Equals(""))
                    listCodArt = "'" + articol.cod + "'";
                else
                    listCodArt += ",'" + articol.cod + "'";

            }

            listCodArt = "(" + listCodArt + ")";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " select distinct a.cod, a.sintetic, b.cod_nivel1, a.umvanz10, a.umvanz, nvl(a.tip_mat, ' '),  b.cod nume_sint, " +
                                  " decode(a.grup_vz, ' ', '-1', a.grup_vz), decode(trim(a.dep_aprobare), '', '00', a.dep_aprobare)  dep_aprobare, " +
                                  " (select nvl((select 1 from sapprd.mara m where m.mandt = '900' and m.matnr = a.cod and m.categ_mat in ('PA','AM')),-1) " +
                                  " palet from dual) palet  , nvl ((select sum(stocne) from sapprd.zstoc_job where matnr=a.cod and werks=:filiala2),-1) stoc , categ_mat, " +
                                  " nvl(lungime,0), a.s_indicator from articole a, sintetice b, sapprd.marc c   where c.mandt = '900' and c.matnr = a.cod " +
                                  " and c.werks = :filiala and c.mmsta <> '01'  and a.sintetic = b.cod and a.cod != 'MAT GENERIC PROD' " +
                                  " and a.blocat <> '01' and a.cod in " + listCodArt + "   ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filialaGed;

                cmd.Parameters.Add(":filiala2", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filiala;


                oReader = cmd.ExecuteReader();
                string strCat;


                int ii = 0;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        foreach (ArticolMathaus articol in listArticole)
                        {
                            if (oReader.GetString(0).Equals(articol.cod))
                            {

                                string codArtBrut = articol.cod;
                                articol.cod = articol.cod.TrimStart('0');
                                articol.sintetic = oReader.GetString(1);
                                articol.nivel1 = oReader.GetString(2);
                                articol.umVanz10 = oReader.GetString(3);
                                articol.umVanz = oReader.GetString(7).Substring(0, 2).Equals("11") ? oReader.GetString(4) : oReader.GetString(3);
                                articol.tipAB = oReader.GetString(5);
                                articol.depart = oReader.GetString(7);
                                articol.departAprob = oReader.GetString(8);
                                articol.umPalet = oReader.GetInt32(9).ToString();

                                articol.stoc = "0";

                                strCat = oReader.GetString(11);
                                if (strCat.ToUpper().Equals("AM") || strCat.ToUpper().Equals("PA"))
                                    strCat = "AM";
                                else
                                    strCat = " ";

                                articol.categorie = strCat;
                                articol.lungime = oReader.GetDouble(12).ToString();
                                articol.catMathaus = oReader.GetString(13).Equals("Y") ? "S" : " ";
                                articol.pretUnitar = " ";

                                break;



                            }

                            ii++;
                        }
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

        }


        public string getStocSite(string codArticol, string filiala)
        {
            string stocSite = "0";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string query = " select  labst from SAPPRD.zhybris_zhstock where mandt = '900' and matnr = :matnr and werks = :werks ";
                cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":matnr", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;

                cmd.Parameters.Add(":werks", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filiala;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    stocSite = oReader.GetDouble(0).ToString();
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


            return stocSite;
        }


        private byte[] GetImage(string url)
        {
            Stream stream = null;
            byte[] buf;

            try
            {
                WebProxy myProxy = new WebProxy();
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                stream = response.GetResponseStream();

                using (BinaryReader br = new BinaryReader(stream))
                {
                    int len = (int)(response.ContentLength);
                    buf = br.ReadBytes(len);
                    br.Close();
                }

                stream.Close();
                response.Close();
            }
            catch (Exception exp)
            {
                buf = null;
            }

            return (buf);
        }





        public string getLivrariComanda(string antetComanda, string strComanda)
        {

            ErrorHandling.sendErrorToMail("getLivrariComanda: " + antetComanda + " \n\n " + strComanda);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            LivrareMathaus livrareMathaus = new LivrareMathaus();

            try
            {

                AntetCmdMathaus antetCmdMathaus = null;
                if (antetComanda != null)
                    antetCmdMathaus = serializer.Deserialize<AntetCmdMathaus>(antetComanda);

                ComandaMathaus comandaMathaus = serializer.Deserialize<ComandaMathaus>(strComanda);
                List<DateArticolMathaus> articole = comandaMathaus.deliveryEntryDataList;

                ComandaMathaus comanda = new ComandaMathaus();
                comanda.sellingPlant = comandaMathaus.sellingPlant;
                List<DateArticolMathaus> deliveryEntryDataList = new List<DateArticolMathaus>();

                foreach (DateArticolMathaus dateArticol in articole)
                {

                    DateArticolMathaus articol = new DateArticolMathaus();
                    articol.productCode = "0000000000" + dateArticol.productCode;
                    articol.quantity = dateArticol.quantity;
                    articol.unit = dateArticol.unit;
                    deliveryEntryDataList.Add(articol);

                }

                comanda.deliveryEntryDataList = deliveryEntryDataList;


                string strComandaRezultat = callDeliveryService(serializer.Serialize(comanda));


                ComandaMathaus comandaRezultat = serializer.Deserialize<ComandaMathaus>(strComandaRezultat);

                bool artFound = false;
                foreach (DateArticolMathaus dateArticol in articole)
                {

                    dateArticol.productCode = "0000000000" + dateArticol.productCode;

                    artFound = false;
                    foreach (DateArticolMathaus dateArticolRez in comandaRezultat.deliveryEntryDataList)
                    {
                        if (dateArticolRez.productCode.Equals(dateArticol.productCode) && !dateArticolRez.deliveryWarehouse.Trim().Equals(String.Empty))
                        {
                            dateArticol.deliveryWarehouse = dateArticolRez.deliveryWarehouse;
                            artFound = true;
                            break;
                        }

                    }

                    if (!artFound)
                        dateArticol.deliveryWarehouse = dateArticol.productCode.StartsWith("0000000000111") ? getULGed(comanda.sellingPlant) : comanda.sellingPlant;

                }

                List<CostTransportMathaus> listCostTransport = null;

                if (antetCmdMathaus != null)
                    listCostTransport = getTransportService(antetCmdMathaus, comandaMathaus);


                livrareMathaus.comandaMathaus = comandaMathaus;
                livrareMathaus.costTransport = listCostTransport;


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("getLivrariComanda: " + ex.ToString());
            }

            return serializer.Serialize(livrareMathaus);


        }





        private string callDeliveryService(string jsonData)
        {

            string result = "";

            try
            {

                System.Net.ServicePointManager.Expect100Continue = false;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://pcm.arabesque.ro/arbsqintegration/optimiseDeliveryB2B");

                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = jsonData.Length;

                string encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes("arbsqservice" + ":" + "kw8tcTbVVqX2fjb"));
                request.Headers.Add("Authorization", "Basic " + encoded);

                using (Stream webStream = request.GetRequestStream())
                using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
                {
                    requestWriter.Write(jsonData);
                }

                System.Net.WebResponse response = request.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());

                result = sr.ReadToEnd().Trim();
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("callDeliveryService: " + ex.ToString());
            }

            return result;

        }

        public string getStocMathaus(string filiala, string codArticol, string um)
        {

            StockMathaus stockMathaus = new StockMathaus();

            stockMathaus.plant = filiala;
            List<StockEntryDataList> stockEntryDataList = new List<StockEntryDataList>();

            StockEntryDataList stockEntry = new StockEntryDataList();

            if (codArticol.StartsWith("0000000000"))
                stockEntry.productCode = codArticol;
            else
                stockEntry.productCode = "0000000000" + codArticol;

            stockEntry.warehouse = "";
            stockEntry.availableQuantity = 0;
            stockEntryDataList.Add(stockEntry);
            stockMathaus.stockEntryDataList = stockEntryDataList;

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            StockMathaus stockResponse = serializer.Deserialize<StockMathaus>(callStockService(serializer.Serialize(stockMathaus)));

            ComandaMathaus comandaMathaus = new ComandaMathaus();
            comandaMathaus.sellingPlant = stockResponse.plant;

            List<DateArticolMathaus> deliveryEntryDataList = new List<DateArticolMathaus>();

            DateArticolMathaus dateArticol = new DateArticolMathaus();
            dateArticol.deliveryWarehouse = stockResponse.stockEntryDataList[0].warehouse;
            dateArticol.quantity = stockResponse.stockEntryDataList[0].availableQuantity;
            dateArticol.productCode = stockResponse.stockEntryDataList[0].productCode;
            dateArticol.unit = um;
            deliveryEntryDataList.Add(dateArticol);
            comandaMathaus.deliveryEntryDataList = deliveryEntryDataList;

            return serializer.Serialize(comandaMathaus);

        }

        private string callStockService(string jsonData)
        {

            ErrorHandling.sendErrorToMail("callStockService: " + jsonData);

            System.Net.ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://pcm.arabesque.ro/arbsqintegration/getStocksB2B");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = jsonData.Length;

            string encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes("arbsqservice" + ":" + "kw8tcTbVVqX2fjb"));
            request.Headers.Add("Authorization", "Basic " + encoded);

            using (Stream webStream = request.GetRequestStream())
            using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
            {
                requestWriter.Write(jsonData);
            }

            System.Net.WebResponse response = request.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());

            return sr.ReadToEnd().Trim();

        }

        private string getULGed(string unitLog)
        {
            return unitLog.Substring(0, 2) + "2" + unitLog.Substring(3, 1);
        }


        private List<CostTransportMathaus> getTransportService(AntetCmdMathaus antetCmd, ComandaMathaus comandaMathaus)
        {
            List<CostTransportMathaus> listCostTransp = new List<CostTransportMathaus>();

           

            try
            {

                SAPWebServices.ZTBL_WEBSERVICE webService = new SAPWebServices.ZTBL_WEBSERVICE();

                SAPWebServices.ZdetTransport inParam = new SAPWebServices.ZdetTransport();
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Auth.getUser(), Auth.getPass());
                webService.Credentials = nc;
                webService.Timeout = 300000;

                inParam.IpCity = antetCmd.localitate;
                inParam.IpRegio = antetCmd.codJudet;
                inParam.IpKunnr = antetCmd.codClient;
                inParam.IpTippers = antetCmd.tipPers;
                inParam.IpWerks = comandaMathaus.sellingPlant;
                inParam.IpVkgrp = antetCmd.depart;

                SAPWebServices.ZsitemsComanda[] items = new SAPWebServices.ZsitemsComanda[comandaMathaus.deliveryEntryDataList.Count];

                int ii = 0;
                foreach (DateArticolMathaus dateArticol in comandaMathaus.deliveryEntryDataList)
                {
                    items[ii] = new SAPWebServices.ZsitemsComanda();
                    items[ii].Matnr = dateArticol.productCode;
                    items[ii].Kwmeng = Decimal.Parse(dateArticol.quantity.ToString());
                    items[ii].Vrkme = dateArticol.unit;
                    items[ii].ValPoz = Decimal.Parse(String.Format("{0:0.00}",dateArticol.valPoz));
                    items[ii].Werks = dateArticol.deliveryWarehouse;
                    ii++;
                }

                inParam.ItItems = items;
                SAPWebServices.ZsfilTransp[] filCost = new SAPWebServices.ZsfilTransp[1];
                inParam.ItFilCost = filCost;

                SAPWebServices.ZdetTransportResponse resp = webService.ZdetTransport(inParam);

                int nrItems = resp.ItItems.Count();

                bool artFound = false;
                foreach (SAPWebServices.ZsitemsComanda itemCmd in resp.ItItems)
                {
                    if (listCostTransp.Count == 0)
                    {
                        CostTransportMathaus cost = new CostTransportMathaus();
                        cost.filiala = itemCmd.Werks;
                        cost.tipTransp = itemCmd.Traty;
                        listCostTransp.Add(cost);
                    }
                    else
                    {
                        artFound = false;
                        foreach (CostTransportMathaus costTransp in listCostTransp)
                        {
                            if (costTransp.filiala.Equals(itemCmd.Werks))
                            {
                                artFound = true;
                                break;
                            }
                        }

                        if (!artFound)
                        {
                            CostTransportMathaus cost = new CostTransportMathaus();
                            cost.filiala = itemCmd.Werks;
                            cost.tipTransp = itemCmd.Traty;
                            listCostTransp.Add(cost);
                        }

                    }


                }

                nrItems = resp.ItFilCost.Count();

                foreach (SAPWebServices.ZsfilTransp itemCost in resp.ItFilCost)
                {

                    foreach (CostTransportMathaus costTransp in listCostTransp)
                    {
                        if (costTransp.filiala.Equals(itemCost.Werks))
                        {
                            costTransp.valTransp = itemCost.ValTr.ToString();
                            costTransp.codArtTransp = itemCost.Matnr;
                            break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("getTransportService: " + ex.ToString());
            }


    


            return listCostTransp;

        }



        public static string getTipTransportMathaus(List<CostTransportMathaus> costTransport, string filiala, string tipTransport)
        {

            if (costTransport == null)
                return tipTransport;
            else if (costTransport.Count == 0)
                return tipTransport;
            else
            {
                foreach (CostTransportMathaus transp in costTransport)
                {
                    if (transp.filiala.Equals(filiala))
                        return transp.tipTransp;
                }
            }
            return tipTransport;
        }


    }
}