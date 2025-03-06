using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Globalization;
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



        public string getArticoleCategorie(string codCategorie, string filiala, string depart, string pagina, string tipArticol, string tipComanda)
        {

            RezultatArtMathaus rezultat = new RezultatArtMathaus();


            if (codCategorie.Equals("0"))
                rezultat = getArticoleLocal(filiala, depart, pagina, tipComanda);
            else {
                if (tipArticol == null || (tipArticol != null && tipArticol.Equals("SITE")))
                    rezultat = getArticoleCategorie(codCategorie, filiala, depart, pagina, tipComanda);
                else
                    rezultat = getArticoleND(filiala, codCategorie, pagina);
            }


            if (rezultat.listArticole.Count > 0)
                addExtraData(rezultat.listArticole, filiala);

            return new JavaScriptSerializer().Serialize(rezultat);
        }


        public int getNrArticoleCategorie(string codCategorie, string filiala, string depart, string tipComanda)
        {


            int nrArticole = 0;

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            string condDepart = " and (substr(ar.grup_vz, 0, 2) in " + HelperComenzi.getDepartExtra(depart) + " or ar.grup_vz = '11') ";

            if (depart != null && (depart.Equals("00") || depart.Equals("11")))
                condDepart = "";

            string condFasonate = "";
            if (tipComanda != null && tipComanda.ToLower().Contains("fasonat"))
                condFasonate = " and ar.sintetic in " + HelperComenzi.getSinteticeFasonate();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();
            try
            {

                cmd.CommandText = " select count(distinct s.matnr)  " +
                                  " from sapprd.zpath_hybris s, sapprd.marc c, websap.articole ar, sapprd.mvke e " +
                                  " where (s.nivel_0 = :codCateg or s.nivel_1 = :codCateg or s.nivel_2 = :codCateg or " +
                                  " s.nivel_3 = :codCateg or s.nivel_4 = :codCateg or s.nivel_5 = :codCateg or s.nivel_6 = :codCateg) " +
                                  condDepart + condFasonate +
                                  " and ar.cod = s.matnr and s.mandt = c.mandt and s.matnr = c.matnr " +
                                  " and e.mandt = '900' and e.matnr = s.matnr and e.vtweg = '20' ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codCateg", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codCategorie;

 

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
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " +  codCategorie + " , " +  filiala + " , " + depart);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return nrArticole;
        }

        public RezultatArtMathaus getArticoleCategorie(string codCategorie, string filiala, string depart, string pagina, string tipComanda)
        {


            RezultatArtMathaus rezultat = new RezultatArtMathaus();

            string paginaCrt = ((Int32.Parse(pagina) - 1) * 10).ToString();

            List<ArticolMathaus> listArticole = new List<ArticolMathaus>();

            depart = depart.Replace("040", "04").Replace("041", "04");

            rezultat.nrTotalArticole = getNrArticoleCategorie(codCategorie, filiala, depart, tipComanda).ToString();

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            string condDepart = " and (substr(ar.grup_vz, 0, 2) in " + HelperComenzi.getDepartExtra(depart) + " or ar.grup_vz = '11') ";

            if (depart != null && (depart.Equals("00") || depart.Equals("11")))
                condDepart = "";

            string condFasonate = "";
            if (tipComanda != null && tipComanda.ToLower().Contains("fasonat"))
                condFasonate = " and ar.sintetic in " + HelperComenzi.getSinteticeFasonate();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();
            try
            {


                cmd.CommandText = " select distinct s.matnr, ar.nume,  e.versg, " +
                                  " (select c.dismm from sapprd.marc c, articole ar where c.mandt = '900' and c.matnr = s.matnr " +
                                  " and ar.cod = c.matnr and c.werks = decode(ar.spart, '11', :filialaGed, :filiala)) planif1, " +
                                  " decode((select c.dismm from sapprd.marc c, articole ar where c.mandt = '900' and c.matnr = s.matnr " +
                                  " and ar.cod = c.matnr and c.werks = decode(ar.spart, '11', :filialaGed, :filiala)), " +
                                  " 'AR',1,'ZM',2,'AC',3,'ND',4,'ZM',5,'VM',6,7) cod_planif, " +
                                   " (select e.versg from sapprd.mvke e where e.mandt = '900' and " +
                                  " e.matnr = s.matnr and e.vtweg = '20') par_s " +
                                  " from sapprd.zpath_hybris s, sapprd.marc c, websap.articole ar, sapprd.mvke e " +
                                  " where (s.nivel_0 = :codCateg or s.nivel_1 = :codCateg or s.nivel_2 = :codCateg or " +
                                  " s.nivel_3 = :codCateg or s.nivel_4 = :codCateg or s.nivel_5 = :codCateg or s.nivel_6 = :codCateg) " +
                                  condDepart + condFasonate + 
                                  " and ar.cod = s.matnr and s.mandt = c.mandt and s.matnr = c.matnr " +
                                  " and e.mandt = '900' and e.matnr = s.matnr and e.vtweg = '20' " +
                                  " order by cod_planif, s.matnr OFFSET :paginaCrt ROWS FETCH NEXT 10 ROWS ONLY ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codCateg", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codCategorie;

                cmd.Parameters.Add(":paginaCrt", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = paginaCrt;

                cmd.Parameters.Add(":filialaGed", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = Utils.getFilialaGed(filiala);

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = filiala;

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
                        articol.planificator = oReader.GetString(3);
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


        private int getNrArticoleLocal(string filiala, string depart, string pagina, string tipComanda)
        {

            int nrArticole = 0;

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            string condDepart = " and (substr(a.grup_vz,0,2) =:depart or a.grup_vz = '11') ";
            if (depart.Equals("00"))
                condDepart = "";

            string condFasonate = "";
            if (tipComanda != null && tipComanda.ToLower().Contains("fasonat"))
                condFasonate = " and a.sintetic in " + HelperComenzi.getSinteticeFasonate();

            try
            {

                cmd.CommandText = " select count(distinct c.matnr) from sapprd.marc c, sapprd.zstoc_job b, websap.articole a, websap.sintetice g " +
                                  " where c.mandt = '900' and c.werks = :filiala and c.dismm in ('ND', 'AC', 'ZC') and b.mandt = c.mandt and b.matnr = c.matnr " +
                                  " and (b.werks = c.werks or (a.spart in ('01', '02', '05') and b.werks = 'BV90')) and b.stocne > 0 and not exists " +
                                  " (select * from sapprd.zpath_hybris s where s.mandt = '900' and s.matnr = c.matnr) and c.matnr = a.cod " +
                                  " and a.sintetic = g.cod " + condDepart + condFasonate + " order by c.matnr ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                if (!depart.Equals("00"))
                {
                    cmd.Parameters.Add(":depart", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = depart.Substring(0, 2);
                }

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
        private RezultatArtMathaus getArticoleLocal(string filiala, string depart, string pagina, string tipComanda)
        {

            RezultatArtMathaus rezultat = new RezultatArtMathaus();
            rezultat.nrTotalArticole = getNrArticoleLocal(filiala, depart, pagina, tipComanda).ToString();

            List<ArticolMathaus> listArticole = new List<ArticolMathaus>();

            string paginaMin = ((Int32.Parse(pagina) - 1) * 10).ToString();
            string paginaMax = (Int32.Parse(pagina) * 10).ToString();


            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            string condDepart = " and (substr(a.grup_vz,0,2) =:depart or a.grup_vz = '11') ";
            if (depart.Equals("00"))
                condDepart = "";

            try
            {

                cmd.CommandText = " select * from (" +
                                 " select distinct c.matnr, a.nume, dismm, row_number() over (ORDER BY c.matnr ASC) line_number from sapprd.marc c, sapprd.zstoc_job b, websap.articole a, websap.sintetice g " +
                                 " where c.mandt = '900' and c.werks = :filiala and c.dismm in ('ND', 'AC', 'ZC') and b.mandt = c.mandt and b.matnr = c.matnr " +
                                 " and (b.werks = c.werks or (a.spart in ('01', '02', '05') and b.werks = 'BV90')) and b.stocne > 0 and not exists " +
                                 " (select * from sapprd.zpath_hybris s where s.mandt = '900' and s.matnr = c.matnr) and c.matnr = a.cod " +
                                 " and a.sintetic = g.cod " + condDepart + "  order by c.matnr ) " +
                                 " where line_number between :pageMin and :pageMax order by line_number ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;


                cmd.Parameters.Add(":pageMin", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = paginaMin;

                cmd.Parameters.Add(":pageMax", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = paginaMax;

                if (!depart.Equals("00"))
                {
                    cmd.Parameters.Add(":depart", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = depart.Substring(0, 2);
                }


                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ArticolMathaus articol = new ArticolMathaus();
                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        articol.planificator = oReader.GetString(2);
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



        public int getNrArticoleCautare(string codArticol, string tipCautare, string filiala, string depart, string tipComanda)
        {

            int nrArticole = 0;

            RezultatArtMathaus rezultat = new RezultatArtMathaus();


            List<ArticolMathaus> listArticole = new List<ArticolMathaus>();

            string cautare;
            if (tipCautare.Equals("c"))
                cautare = " and lower(decode(length(a.cod),18,substr(a.cod,-8),a.cod)) like lower('" + codArticol.ToLower() + "%')";
            else
                cautare = " and lower(a.nume) like '" + codArticol.ToLower() + "%'";

            string condDepart = " and (a.grup_vz in " + HelperComenzi.getDepartExtra(depart) + " or a.grup_vz = '11' )  ";

            if (depart.Equals("00"))
                condDepart = "";

            string condFasonate = "";
            if (tipComanda != null && tipComanda.Trim().ToLower().Contains("fasonat"))
                condFasonate = " and a.sintetic in " + HelperComenzi.getSinteticeFasonate();


            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();
            try
            {
                cmd.CommandText = " select count(distinct a.cod)  " +
                                 " from articole a, sintetice b, sapprd.marc c where c.mandt = '900' " +
                                 " and c.matnr = a.cod and c.werks = :filiala and c.mmsta <> '01'  and a.sintetic = b.cod and a.cod != 'MAT GENERIC PROD' " +
                                 " and a.blocat <> '01' " + cautare + condDepart + condFasonate;


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

    

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




        public string cautaArticoleMathaus(string codArticol, string tipCautare, string filiala, string depart, string pagina, string tipComanda)
        {

            RezultatArtMathaus rezultat = new RezultatArtMathaus();

            string paginaCrt = ((Int32.Parse(pagina) - 1) * 10).ToString();

            List<ArticolMathaus> listArticole = new List<ArticolMathaus>();

            string cautare;
            if (tipCautare.Equals("c"))
                cautare = " and lower(decode(length(a.cod),18,substr(a.cod,-8),a.cod)) like lower('" + codArticol.ToLower() + "%')";
            else
                cautare = " and lower(a.nume) like '" + codArticol.ToLower() + "%'";

            rezultat.nrTotalArticole = getNrArticoleCautare(codArticol, tipCautare, filiala, depart, tipComanda).ToString();

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            string condDepart = " and ( a.grup_vz in " + HelperComenzi.getDepartExtra(depart) + " or a.grup_vz = '11' )  ";

            if (depart.Equals("00"))
                condDepart = "";

            string condFasonate = "";
            if (tipComanda != null && tipComanda.Trim().ToLower().Contains("fasonat"))
                condFasonate = " and a.sintetic in " + HelperComenzi.getSinteticeFasonate();

            OracleCommand cmd = connection.CreateCommand();
            try
            {
  
                cmd.CommandText = " select a.cod ,a.nume, " +
                                  " (select c.dismm from sapprd.marc c, articole ar where c.mandt = '900' and c.matnr = a.cod " +
                                  " and ar.cod = c.matnr and c.werks = decode(ar.spart, '11', :filialaGed, :filiala)) planif, " +
                                  " decode((select c.dismm from sapprd.marc c, articole ar where c.mandt = '900' and c.matnr = a.cod " +
                                  " and ar.cod = c.matnr and c.werks = decode(ar.spart, '11', :filialaGed, :filiala)), " +
                                  " 'AR',1,'ZM',2,'AC',3,'ND',4,'ZM',5,'VM',6,7) cod_planif, " +
                                  " (select e.versg from sapprd.mvke e where e.mandt = '900' and " +
                                  " e.matnr = a.cod and e.vtweg = '20') par_s " +
                                  " from articole a, sintetice b, sapprd.marc c " +
                                  " where c.mandt = '900'  and c.matnr = a.cod and c.werks = :filiala and c.mmsta <> '01' " +
                                  " and a.sintetic = b.cod and a.cod != 'MAT GENERIC PROD'  and a.blocat <> '01' " +
                                    cautare + condDepart + condFasonate + 
                                  " order by cod_planif, a.nume  OFFSET :paginaCrt ROWS FETCH NEXT 10 ROWS ONLY ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                cmd.Parameters.Add(":paginaCrt", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = paginaCrt;

                cmd.Parameters.Add(":filialaGed", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = Utils.getFilialaGed(filiala);

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ArticolMathaus articol = new ArticolMathaus();
                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        articol.tip1 = oReader.GetString(2);
                        articol.tip2 = oReader.GetString(4);
                        articol.planificator = oReader.GetString(2);
                        articol.isLocal = true;
                        articol.isArticolSite = false;
                        setDetaliiArticol(articol);
                        listArticole.Add(articol);
                    }
                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " +  codArticol + " , " +  tipCautare + " , " + filiala + " , " + depart + " , " + pagina);
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
                                  " select distinct c.matnr, a.nume, c.dismm , row_number() over (ORDER BY c.matnr ASC) line_number from sapprd.marc c, sapprd.zstoc_job b, websap.articole a, websap.sintetice g " +
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
                        articol.planificator = oReader.GetString(2);
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
            try {
                string serviceUrl = "https://wse1-sap-hybris-prod.arabesque.ro/solr/master_erp_Product_default/select?q=code_string:" + articol.cod;

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
                            if (data.Contains("https"))
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
            catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
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
                                articol.umVanz = oReader.GetString(4);
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


        public string getLivrariComandaCumulative(string antetComanda, string strComanda, string canal, string strPoligon)
        {

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            LivrareMathaus livrareMathaus = new LivrareMathaus();

            try
            {

                AntetCmdMathaus antetCmdMathaus = null;
                if (antetComanda != null)
                    antetCmdMathaus = serializer.Deserialize<AntetCmdMathaus>(antetComanda);

                ComandaMathaus comandaMathaus = serializer.Deserialize<ComandaMathaus>(strComanda);
                List<DateArticolMathaus> articole = comandaMathaus.deliveryEntryDataList;

                DatePoligon datePoligon = new DatePoligon("", "", "", "", "","");

                if (strPoligon != null && strPoligon.Trim().Length > 0)
                {
                    datePoligon = serializer.Deserialize<DatePoligon>(strPoligon);
                }

                ComandaMathaus comanda = new ComandaMathaus();
                comanda.sellingPlant = comandaMathaus.sellingPlant;
                comanda.countyCode = antetCmdMathaus.codJudet;
                comanda.deliveryZoneType = HelperComenzi.getTipZonaMathaus(datePoligon.tipZona);

                List<DateArticolMathaus> deliveryEntryDataList = new List<DateArticolMathaus>();

                Dictionary<string, string> dictionarUmIso = HelperComenzi.getDictionarUmIso(articole);

                foreach (DateArticolMathaus dateArticol in articole)
                {

                    if (!dateArticol.tip2.Equals("S"))
                        continue;

                    DateArticolMathaus articol = new DateArticolMathaus();
                    if (Char.IsDigit(dateArticol.productCode, 0))
                        articol.productCode = "0000000000" + dateArticol.productCode;
                    else
                        articol.productCode = dateArticol.productCode;

                    if (!dateArticol.unit.Equals(dateArticol.unit50) && dateArticol.quantity50 > 0)
                    {
                        articol.quantity = dateArticol.quantity50;
                        articol.unit = dictionarUmIso[dateArticol.unit50];
                    }
                    else {
                        articol.quantity = dateArticol.quantity;
                        articol.unit = dictionarUmIso[dateArticol.unit];
                    }

                    deliveryEntryDataList.Add(articol);

                }

                comanda.deliveryEntryDataList = deliveryEntryDataList;

                ComandaMathaus comandaRezultat;
                if (comanda.deliveryEntryDataList.Count > 0)
                {
                    string strComandaRezultat = callDeliveryService(serializer.Serialize(comanda), canal, antetCmdMathaus.tipPers, antetCmdMathaus.codPers);
                    comandaRezultat = serializer.Deserialize<ComandaMathaus>(strComandaRezultat);
                    var reversedDictionarUmIso = dictionarUmIso.ToDictionary(x => x.Value, x => x.Key);
                    HelperComenzi.convertUmFromIso(reversedDictionarUmIso, comandaRezultat.deliveryEntryDataList);
                }
                else
                {
                    ComandaMathaus cmdMathaus = new ComandaMathaus();
                    cmdMathaus.sellingPlant = comandaMathaus.sellingPlant.Split(',')[0];
                    cmdMathaus.deliveryEntryDataList = new List<DateArticolMathaus>();
                    comandaRezultat = cmdMathaus;
                }

                bool artFound = false;

                List<DateArticolMathaus> listArticoleComanda = new List<DateArticolMathaus>();

                foreach (DateArticolMathaus dateArticol in articole)
                {

                    DateArticolMathaus articolComanda = null;

                    if (!dateArticol.productCode.StartsWith("0000000000") && Char.IsDigit(dateArticol.productCode, 0))
                        dateArticol.productCode = "0000000000" + dateArticol.productCode;

                    artFound = false;
                    foreach (DateArticolMathaus dateArticolRez in comandaRezultat.deliveryEntryDataList)
                    {
                        if (dateArticolRez.productCode.Equals(dateArticol.productCode) && dateArticolRez.deliveryWarehouse != null && !dateArticolRez.deliveryWarehouse.Trim().Equals(String.Empty))
                        {

                            if (canal != null && canal.Equals("20") && !dateArticolRez.deliveryWarehouse.Equals("BV90"))
                            {
                                dateArticol.deliveryWarehouse = dateArticolRez.deliveryWarehouse;
                            }

                            else
                                dateArticol.deliveryWarehouse = dateArticolRez.deliveryWarehouse;

                            artFound = true;

                            articolComanda = new DateArticolMathaus();
                            articolComanda.productCode = dateArticolRez.productCode;
                            articolComanda.deliveryWarehouse = dateArticolRez.deliveryWarehouse;
                            articolComanda.depozit = dateArticol.depozit;
                            articolComanda.ulStoc = dateArticol.ulStoc;
                            articolComanda.tip2 = dateArticol.tip2;
                            articolComanda.unit = dateArticol.unit;

                            if (!dateArticol.unit.Equals(dateArticol.unit50))
                                articolComanda.quantity = Math.Round(dateArticolRez.quantity * (dateArticol.quantity / dateArticol.quantity50), 2);
                            else
                                articolComanda.quantity = dateArticolRez.quantity;

                            articolComanda.cantUmb = Math.Round(HelperComenzi.getCantitateUmb(articolComanda.productCode, articolComanda.quantity, articolComanda.unit), 2);
                            articolComanda.valPoz = Math.Round(((dateArticol.valPoz / dateArticol.quantity) * articolComanda.quantity), 2);
                            articolComanda.greutate = dateArticol.greutate;
                            articolComanda.tipStoc = dateArticol.tipStoc;

                            listArticoleComanda.Add(articolComanda);

                        }

                    }

                    if (!artFound)
                    {

                        articolComanda = new DateArticolMathaus();

                        if (dateArticol.ulStoc != null && dateArticol.ulStoc.Trim() != "" && !dateArticol.ulStoc.Equals("BV90"))
                            articolComanda.deliveryWarehouse = dateArticol.ulStoc;
                        else if (dateArticol.ulStoc != null && dateArticol.ulStoc.Equals("BV90"))
                            articolComanda.deliveryWarehouse = "BV90";
                        else if (canal != null && canal.Equals("20") && (dateArticol.ulStoc == null || !dateArticol.ulStoc.Equals("BV90")))
                            articolComanda.deliveryWarehouse = dateArticol.productCode.StartsWith("0000000000111") ? getULGed(comanda.sellingPlant) : comanda.sellingPlant.Split(',')[0];
                        else
                            articolComanda.deliveryWarehouse = dateArticol.productCode.StartsWith("0000000000111") ? getULGed(comanda.sellingPlant) : comanda.sellingPlant.Split(',')[0];

                        articolComanda.productCode = dateArticol.productCode;
                        articolComanda.depozit = dateArticol.depozit;
                        articolComanda.ulStoc = dateArticol.ulStoc;
                        articolComanda.tip2 = dateArticol.tip2;
                        articolComanda.unit = dateArticol.unit;
                        articolComanda.quantity = dateArticol.quantity;
                        articolComanda.valPoz = Math.Round(dateArticol.valPoz, 2);
                        articolComanda.greutate = dateArticol.greutate;
                        articolComanda.tipStoc = dateArticol.tipStoc;
                        listArticoleComanda.Add(articolComanda);
                    }


                }

                DateTransportMathaus dateTransport = null;

                comandaMathaus.deliveryEntryDataList = listArticoleComanda;

                if (antetCmdMathaus != null)
                    dateTransport = getTransportService(antetCmdMathaus, comandaMathaus, canal, datePoligon);

                bool stocSap = false;

                foreach (DateArticolMathaus articolMathaus in comandaMathaus.deliveryEntryDataList)
                {
                    stocSap = articolMathaus.tipStoc != null && articolMathaus.tipStoc.ToLower().Equals("sap");

                    foreach (DepozitArticolTransport depozitArticol in dateTransport.listDepozite)
                    {
                        bool conditieArticol = articolMathaus.productCode.TrimStart('0').Equals(depozitArticol.codArticol.TrimStart('0')) && articolMathaus.deliveryWarehouse.Equals(depozitArticol.filiala);

                        if (stocSap)
                            conditieArticol = articolMathaus.productCode.TrimStart('0').Equals(depozitArticol.codArticol.TrimStart('0'));

                        if (conditieArticol)
                        {
                            articolMathaus.depozit = depozitArticol.depozit;
                            articolMathaus.cmpCorectat = depozitArticol.cmpCorectat;

                            if (!articolMathaus.productCode.StartsWith("0000000000"))
                                articolMathaus.productCode = "0000000000" + articolMathaus.productCode;

                            if (stocSap)
                                articolMathaus.deliveryWarehouse = depozitArticol.filiala;

                            break;
                        }
                    }
                }


                livrareMathaus.comandaMathaus = comandaMathaus;
                livrareMathaus.costTransport = dateTransport.listCostTransport;
                livrareMathaus.taxeMasini = dateTransport.taxeMasini;
                livrareMathaus.listPaleti = dateTransport.listPaleti;


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("getLivrariComanda: " + ex.ToString());
            }

            return serializer.Serialize(livrareMathaus);

        }



        public string getLivrariComanda(string antetComanda, string strComanda, string canal, string strPoligon)
        {

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            LivrareMathaus livrareMathaus = new LivrareMathaus();

            try
            {

                AntetCmdMathaus antetCmdMathaus = null;
                if (antetComanda != null)
                    antetCmdMathaus = serializer.Deserialize<AntetCmdMathaus>(antetComanda);

                ComandaMathaus comandaMathaus = serializer.Deserialize<ComandaMathaus>(strComanda);
                List<DateArticolMathaus> articole = comandaMathaus.deliveryEntryDataList;

                DatePoligon datePoligon = new DatePoligon("", "", "", "", "","");

                if (strPoligon != null && strPoligon.Trim().Length > 0)
                {
                    datePoligon = serializer.Deserialize<DatePoligon>(strPoligon);
                }

                ComandaMathaus comanda = new ComandaMathaus();
                comanda.sellingPlant = comandaMathaus.sellingPlant;
                comanda.countyCode = antetCmdMathaus.codJudet;

                List<DateArticolMathaus> deliveryEntryDataList = new List<DateArticolMathaus>();
                Dictionary<string, string> dictionarUmIso = HelperComenzi.getDictionarUmIso(articole);

                foreach (DateArticolMathaus dateArticol in articole)
                {

                    if (!dateArticol.tip2.Equals("S"))
                        continue;

                    DateArticolMathaus articol = new DateArticolMathaus();

                    if (Char.IsDigit(dateArticol.productCode, 0))
                        articol.productCode = "0000000000" + dateArticol.productCode;
                    else
                        articol.productCode = dateArticol.productCode;

                    articol.quantity = Math.Ceiling(dateArticol.quantity); 
                    articol.unit = dictionarUmIso[dateArticol.unit];
                    deliveryEntryDataList.Add(articol);

                }

                comanda.deliveryEntryDataList = deliveryEntryDataList;

                ComandaMathaus comandaRezultat;
                if (comanda.deliveryEntryDataList.Count > 0)
                {
                    string strComandaRezultat = callDeliveryService(serializer.Serialize(comanda), canal, antetCmdMathaus.tipPers, antetCmdMathaus.codPers);
                    comandaRezultat = serializer.Deserialize<ComandaMathaus>(strComandaRezultat);
                    var reversedDictionarUmIso = dictionarUmIso.ToDictionary(x => x.Value, x => x.Key);
                    HelperComenzi.convertUmFromIso(reversedDictionarUmIso, comandaRezultat.deliveryEntryDataList);
                }
                else
                {
                    ComandaMathaus cmdMathaus = new ComandaMathaus();
                    cmdMathaus.sellingPlant = comandaMathaus.sellingPlant;
                    cmdMathaus.deliveryEntryDataList = new List<DateArticolMathaus>();
                    comandaRezultat = cmdMathaus;
                }

                bool artFound = false;
                foreach (DateArticolMathaus dateArticol in articole)
                {

                    if (!dateArticol.productCode.StartsWith("0000000000"))
                        dateArticol.productCode = "0000000000" + dateArticol.productCode;

                    artFound = false;
                    foreach (DateArticolMathaus dateArticolRez in comandaRezultat.deliveryEntryDataList)
                    {
                        if (dateArticolRez.productCode.Equals(dateArticol.productCode) && dateArticolRez.deliveryWarehouse != null && !dateArticolRez.deliveryWarehouse.Trim().Equals(String.Empty))
                        {

                            if (canal != null && canal.Equals("20") && !dateArticolRez.deliveryWarehouse.Equals("BV90"))
                                dateArticol.deliveryWarehouse = dateArticolRez.deliveryWarehouse;
                            else
                                dateArticol.deliveryWarehouse = dateArticolRez.deliveryWarehouse;

                            artFound = true;
                            break;
                        }

                    }

                    if (!artFound)
                    {
                        if (dateArticol.ulStoc != null && dateArticol.ulStoc.Equals("BV90"))
                            dateArticol.deliveryWarehouse = "BV90";
                        else if (canal != null && canal.Equals("20") && (dateArticol.ulStoc == null || !dateArticol.ulStoc.Equals("BV90")))
                            dateArticol.deliveryWarehouse = dateArticol.productCode.StartsWith("0000000000111") ? getULGed(comanda.sellingPlant) : comanda.sellingPlant;
                        else
                            dateArticol.deliveryWarehouse = dateArticol.productCode.StartsWith("0000000000111") ? getULGed(comanda.sellingPlant) : comanda.sellingPlant;
                    }

                }

                DateTransportMathaus dateTransport = null;

                if (antetCmdMathaus != null)
                    dateTransport = getTransportService(antetCmdMathaus, comandaMathaus, canal, datePoligon);

                foreach (DateArticolMathaus articolMathaus in comandaMathaus.deliveryEntryDataList)
                {
                    foreach (DepozitArticolTransport depozitArticol in dateTransport.listDepozite)

                    {
                        if (articolMathaus.productCode.TrimStart('0').Equals(depozitArticol.codArticol.TrimStart('0')) && articolMathaus.deliveryWarehouse.Equals(depozitArticol.filiala))
                        {
                            articolMathaus.depozit = depozitArticol.depozit;
                            break;
                        }
                    }
                }


                livrareMathaus.comandaMathaus = comandaMathaus;
                livrareMathaus.costTransport = dateTransport.listCostTransport;


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("getLivrariComanda: " + ex.ToString());
            }

            return serializer.Serialize(livrareMathaus);

        }



        public string callDeliveryService(string jsonData, string canal, string tipPers, string codPers)
        {

            string result = "";

            try
            {

                System.Net.ServicePointManager.Expect100Continue = false;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string urlDeliveryService = "https://wse1-hybris-b2c-prod.arabesque.ro/arbsqintegration/optimiseDeliveryB2B";

                urlDeliveryService = "https://wse1-sap-hybris-prod.arabesque.ro/arbsqintegration/optimiseDeliveryB2B";


                if (canal.Equals("10"))
                    urlDeliveryService = "https://wse1-sap-hybris-prod.arabesque.ro/arbsqintegration/cumulativeOptimiseDeliveryB2B";
                else
                {
                    if (tipPers.Equals("AV") || tipPers.Equals("SD"))
                        urlDeliveryService = "https://wse1-sap-hybris-prod.arabesque.ro/arbsqintegration/cumulativeOptimiseDeliveryB2B";
                    else
                        urlDeliveryService = "https://wse1-sap-hybris-prod.arabesque.ro/arbsqintegration/cumulativeOptimiseDeliveryB2C";

                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlDeliveryService);

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

                string deliveryResponse = sr.ReadToEnd().Trim();

                result = deliveryResponse;
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("callDeliveryService: " + ex.ToString() + " , " + jsonData  + " , " + canal + " , " + tipPers + " , " + codPers);
            }

            return result;

        }

        public string getStocMathaus(string filiala, string codArticol, string um, string tipCmd, string tipUserSap, string codUser, string tipZona)
        {

            

            StockMathaus stockMathaus = new StockMathaus();

            stockMathaus.plant = filiala;
            stockMathaus.deliveryZoneType = HelperComenzi.getTipZonaMathaus(tipZona);
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

            StockMathaus stockResponse = serializer.Deserialize<StockMathaus>(callStockService(serializer.Serialize(stockMathaus), tipCmd, tipUserSap, codUser));

            ComandaMathaus comandaMathaus = new ComandaMathaus();
            comandaMathaus.sellingPlant = stockResponse.plant;

            List<DateArticolMathaus> deliveryEntryDataList = new List<DateArticolMathaus>();

            DateArticolMathaus dateArticol = new DateArticolMathaus();
            dateArticol.deliveryWarehouse = stockResponse.stockEntryDataList[0].warehouse;
            dateArticol.quantity = stockResponse.stockEntryDataList[0].availableQuantity;
            dateArticol.productCode = stockResponse.stockEntryDataList[0].productCode;
            dateArticol.unit = getUmBaza(stockEntry.productCode);
            deliveryEntryDataList.Add(dateArticol);
            comandaMathaus.deliveryEntryDataList = deliveryEntryDataList;

            return serializer.Serialize(comandaMathaus);

        }


        private string getUmBaza(string codArticol)
        {

            string umBaza = "BUC";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select um from articole where cod = :codArticol ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codArticol", OracleType.NVarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    umBaza = oReader.GetString(0);
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

            return umBaza;

        }

        private string callStockService(string jsonData, string tipCmd, string tipUserSap, string codUser)
        {

            string stockResponse = "";
            string urlStockService = "https://wse1-hybris-b2c-prod.arabesque.ro/arbsqintegration/getStocksB2B";

            urlStockService = "https://wse1-sap-hybris-prod.arabesque.ro/arbsqintegration/getStocksB2B";

            try {

                System.Net.ServicePointManager.Expect100Continue = false;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


                if (tipCmd != null && tipCmd.Equals("D"))
                {
                    urlStockService = "https://wse1-sap-hybris-prod.arabesque.ro/arbsqintegration/getCumulativeStocksB2B";

                }
                else if (tipCmd != null && tipCmd.Equals("G"))
                {
                    if (tipUserSap.Equals("AV") || tipUserSap.Equals("SD"))
                        urlStockService = "https://wse1-sap-hybris-prod.arabesque.ro/arbsqintegration/getCumulativeStocksB2B";
                    else
                        urlStockService = "https://wse1-sap-hybris-prod.arabesque.ro/arbsqintegration/getCumulativeStocksB2C";
                }



                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlStockService);
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


                stockResponse = sr.ReadToEnd().Trim();

            }
            catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail("callStockService: " + ex.ToString() + " \n " + urlStockService + " \n" + jsonData);
            }


            return stockResponse;

        }

        private string getULGed(string unitLog)
        {
            return unitLog.Substring(0, 2) + "2" + unitLog.Substring(3, 1);
        }


        public DateTransportMathaus getTransportService_old(AntetCmdMathaus antetCmd, ComandaMathaus comandaMathaus, string canal, DatePoligon datePoligon)
        {
            DateTransportMathaus dateTransport = new DateTransportMathaus();
            List<CostTransportMathaus> listCostTransp = new List<CostTransportMathaus>();
            List<DepozitArticolTransport> listArticoleDepoz = new List<DepozitArticolTransport>();
            List<CostTransportMathaus> listTaxeTransp = new List<CostTransportMathaus>();

            List<OptiuneCamion> optiuniCamion;
            if (antetCmd.tipCamion != null)
                optiuniCamion = new JavaScriptSerializer().Deserialize<List<OptiuneCamion>>(antetCmd.tipCamion);
            else
                optiuniCamion = new List<OptiuneCamion>();

            string werks = comandaMathaus.sellingPlant.Split(',')[0];
            string departCmd = antetCmd.depart;

            if (canal != null && canal.Equals("20"))
            {
                werks = getULGed(comandaMathaus.sellingPlant);
                departCmd = "11";
            }

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
                inParam.IpWerks = werks;
                inParam.IpVkgrp = departCmd;
                inParam.IpPernr = antetCmd.codPers;
                inParam.IpTraty = antetCmd.tipTransp;
                

                SAPWebServices.ZstTaxeAcces taxeAcces = new SAPWebServices.ZstTaxeAcces();


                if (antetCmd.tipComandaCamion != null && antetCmd.tipComandaCamion.Trim() != "")
                    taxeAcces.TipComanda = antetCmd.tipComandaCamion;
                else
                    taxeAcces.TipComanda = "";

                if (antetCmd.greutateComanda != null && antetCmd.greutateComanda.Trim() != "")
                    taxeAcces.GreutMarfa = Decimal.Parse(String.Format("{0:0.00}", Double.Parse(antetCmd.greutateComanda)));
                else
                    taxeAcces.GreutMarfa = 0;

                taxeAcces.Zona = HelperComenzi.getTipZonaMathaus(datePoligon.tipZona);
                taxeAcces.MasinaDescoperita = antetCmd.camionDescoperit != null && Boolean.Parse(antetCmd.camionDescoperit) ? "X" : " ";
                taxeAcces.Macara = antetCmd.macara != null && Boolean.Parse(antetCmd.macara) ? "X" : " ";
                taxeAcces.CamionScurt = HelperComenzi.getOptiuneCamion(optiuniCamion, "Camion scurt");
                taxeAcces.CamionIveco = HelperComenzi.getOptiuneCamion(optiuniCamion, "Camioneta IVECO");
                taxeAcces.Poligon = datePoligon.nume;

                if (datePoligon.limitareTonaj != null && datePoligon.limitareTonaj.Trim() != "")
                {
                    if (datePoligon.limitareTonaj.Contains(","))
                        taxeAcces.LimitaTonaj = Decimal.Parse(datePoligon.limitareTonaj, new CultureInfo("ro"));
                    else
                        taxeAcces.LimitaTonaj = Decimal.Parse(datePoligon.limitareTonaj);
                }

                inParam.IsTaxaAcces = taxeAcces;

                SAPWebServices.ZsitemsComanda[] items = new SAPWebServices.ZsitemsComanda[comandaMathaus.deliveryEntryDataList.Count];

                int ii = 0;
                foreach (DateArticolMathaus dateArticol in comandaMathaus.deliveryEntryDataList)
                {
                    items[ii] = new SAPWebServices.ZsitemsComanda();
                    items[ii].Matnr = dateArticol.productCode;
                    items[ii].Kwmeng = Decimal.Parse(dateArticol.quantity.ToString());
                    items[ii].Vrkme = dateArticol.unit;
                    items[ii].ValPoz = Decimal.Parse(String.Format("{0:0.00}", dateArticol.valPoz));

                    
                    items[ii].Werks = dateArticol.deliveryWarehouse;

                    if (dateArticol.tipStoc != null && dateArticol.tipStoc.ToLower().Equals("sap"))
                        items[ii].Werks = "NN10";

                    if (dateArticol.depozit != null && dateArticol.depozit.Trim() != "")
                        items[ii].Lgort = dateArticol.depozit;

                    if (antetCmd.isComandaDL != null && Boolean.Parse(antetCmd.isComandaDL))
                        items[ii].Lgort = "DESC";

                    if (dateArticol.greutate != null && dateArticol.greutate.Trim() != "")
                        items[ii].BrgewMatnr = (Decimal)HelperComenzi.getGreutateArticol(dateArticol.productCode, dateArticol.quantity, comandaMathaus);
                    else
                        items[ii].BrgewMatnr = 0;

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

                    DepozitArticolTransport depozitArticol = new DepozitArticolTransport();
                    depozitArticol.codArticol = itemCmd.Matnr;
                    depozitArticol.filiala = itemCmd.Werks;
                    depozitArticol.depozit = itemCmd.Lgort;
                    depozitArticol.cmpCorectat = itemCmd.Cmpc.ToString();
                    listArticoleDepoz.Add(depozitArticol);

                }

                nrItems = resp.ItFilCost.Count();


                foreach (SAPWebServices.ZsfilTransp itemCost in resp.ItFilCost)
                {

                    foreach (CostTransportMathaus costTransp in listCostTransp)
                    {
                        if (costTransp.filiala.Equals(itemCost.Werks))
                        {

                            CostTransportMathaus taxaTransport = new CostTransportMathaus();
                            taxaTransport.filiala = costTransp.filiala;
                            taxaTransport.tipTransp = costTransp.tipTransp;

                            taxaTransport.valTransp = itemCost.ValTr.ToString();
                            taxaTransport.codArtTransp = itemCost.Matnr;
                            taxaTransport.depart = itemCost.Spart;
                            taxaTransport.numeCost = itemCost.Maktx.ToUpper();
                            listTaxeTransp.Add(taxaTransport);
                            break;
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                JavaScriptSerializer ser = new JavaScriptSerializer();
                ErrorHandling.sendErrorToMail("getTransportService: " + ex.ToString() + " \n\n antetCmd: " + ser.Serialize(antetCmd) + "\n\n comandaMathaus: " + ser.Serialize(comandaMathaus) + "\n\n canal:" + canal + "\n\n datePoligon: " + ser.Serialize(datePoligon));
            }


            if (antetCmd.tipTransp != null && (antetCmd.tipTransp.Equals("TCLI") || antetCmd.tipTransp.Equals("TFRN")))
                dateTransport.listCostTransport = new List<CostTransportMathaus>();
            else
                dateTransport.listCostTransport = listTaxeTransp;

            dateTransport.listDepozite = listArticoleDepoz;

            return dateTransport;

        }


        public DateTransportMathaus getTransportService(AntetCmdMathaus antetCmd, ComandaMathaus comandaMathaus, string canal, DatePoligon datePoligon)
        {


            DateTransportMathaus dateTransport = new DateTransportMathaus();
            List<CostTransportMathaus> listCostTransp = new List<CostTransportMathaus>();
            List<DepozitArticolTransport> listArticoleDepoz = new List<DepozitArticolTransport>();
            List<CostTransportMathaus> listTaxeTransp = new List<CostTransportMathaus>();
            List<TaxaMasina> listTaxeMasini = new List<TaxaMasina>();
            List<ArticolPalet> listPaleti = new List<ArticolPalet>();


            List<OptiuneCamion> optiuniCamion;
            if (antetCmd.tipCamion != null)
                optiuniCamion = new JavaScriptSerializer().Deserialize<List<OptiuneCamion>>(antetCmd.tipCamion);
            else
                optiuniCamion = new List<OptiuneCamion>();

            string werks = comandaMathaus.sellingPlant.Split(',')[0];


            string departCmd = antetCmd.depart;

            if (canal != null && canal.Equals("20"))
            {
                werks = getULGed(comandaMathaus.sellingPlant);
                departCmd = "11";
            }

            try
            {

                SAPWebServices.ZTBL_WEBSERVICE webService = new SAPWebServices.ZTBL_WEBSERVICE();

                SAPWebServices.ZdetTransportSfa inParam = new SAPWebServices.ZdetTransportSfa();
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Auth.getUser(), Auth.getPass());
                webService.Credentials = nc;
                webService.Timeout = 300000;

                inParam.IpCity = antetCmd.localitate;
                inParam.IpRegio = antetCmd.codJudet;
                inParam.IpKunnr = antetCmd.codClient;
                inParam.IpTippers = antetCmd.tipPers;
                inParam.IpWerks = werks;
                inParam.IpVkgrp = departCmd;
                inParam.IpPernr = antetCmd.codPers;
                inParam.IpTraty = antetCmd.tipTransp;
                inParam.IpCanal = canal;
                inParam.IpVbeln = antetCmd.nrCmdSap == null ? " " : antetCmd.nrCmdSap;
                inParam.IpAdresa = antetCmd.strada == null ? " " : antetCmd.strada;


                SAPWebServices.ZstTaxeAcces2 taxeAcces = new SAPWebServices.ZstTaxeAcces2();


                taxeAcces.TipComanda = antetCmd.tipComandaCamion;
                taxeAcces.Zona = HelperComenzi.getTipZonaMathaus(datePoligon.tipZona);
                taxeAcces.MasinaDescoperita = antetCmd.camionDescoperit != null && Boolean.Parse(antetCmd.camionDescoperit) ? "X" : " ";
                taxeAcces.Poligon = datePoligon.nume;

                if (datePoligon.limitareTonaj != null && datePoligon.limitareTonaj.Trim() != "")
                {
                    if (datePoligon.limitareTonaj.Contains(","))
                        taxeAcces.LimitaTonaj = Decimal.Parse(datePoligon.limitareTonaj, new CultureInfo("ro"));
                    else
                        taxeAcces.LimitaTonaj = Decimal.Parse(datePoligon.limitareTonaj);
                }


                inParam.IsTaxaAcces = taxeAcces;

                SAPWebServices.ZsitemsComanda[] items = new SAPWebServices.ZsitemsComanda[comandaMathaus.deliveryEntryDataList.Count];

                int ii = 0;
                foreach (DateArticolMathaus dateArticol in comandaMathaus.deliveryEntryDataList)
                {
                    items[ii] = new SAPWebServices.ZsitemsComanda();
                    items[ii].Matnr = dateArticol.productCode;
                    items[ii].Kwmeng = Decimal.Parse(dateArticol.quantity.ToString());
                    items[ii].Vrkme = dateArticol.unit;
                    items[ii].ValPoz = Decimal.Parse(String.Format("{0:0.00}", dateArticol.valPoz));

                    items[ii].Werks = dateArticol.deliveryWarehouse;

                    if (dateArticol.tipStoc != null && dateArticol.tipStoc.ToLower().Equals("sap"))
                        items[ii].Werks = "NN10";

                    if (dateArticol.depozit != null && dateArticol.depozit.Trim() != "")
                        items[ii].Lgort = dateArticol.depozit;

                    if (antetCmd.isComandaDL != null && Boolean.Parse(antetCmd.isComandaDL))
                        items[ii].Lgort = "DESC";

                    items[ii].BrgewMatnr = (Decimal)HelperComenzi.getGreutateArticol(dateArticol.productCode, dateArticol.quantity, comandaMathaus);

                    ii++;
                }

                inParam.ItItems = items;
                SAPWebServices.ZsfilTransp[] filCost = new SAPWebServices.ZsfilTransp[1];
                inParam.ItFilCost = filCost;
                inParam.ItMarfaPalet = new SAPWebServices.ZstEtMarfaPalet[1];
                inParam.ItTransCom = new SAPWebServices.ZstTransportCom[1];

                SAPWebServices.ZdetTransportSfaResponse resp = webService.ZdetTransportSfa(inParam);

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

                    DepozitArticolTransport depozitArticol = new DepozitArticolTransport();
                    depozitArticol.codArticol = itemCmd.Matnr;
                    depozitArticol.filiala = itemCmd.Werks;
                    depozitArticol.depozit = itemCmd.Lgort;
                    depozitArticol.cmpCorectat = itemCmd.Cmpc.ToString();
                    listArticoleDepoz.Add(depozitArticol);

                }

                nrItems = resp.ItFilCost.Count();

                foreach (SAPWebServices.ZsfilTransp itemCost in resp.ItFilCost)
                {

                    foreach (CostTransportMathaus costTransp in listCostTransp)
                    {
                        if (costTransp.filiala.Equals(itemCost.Werks))
                        {

                            CostTransportMathaus taxaTransport = new CostTransportMathaus();
                            taxaTransport.filiala = costTransp.filiala;
                            taxaTransport.tipTransp = costTransp.tipTransp;
                            taxaTransport.valTransp = itemCost.ValTr.ToString();
                            taxaTransport.codArtTransp = itemCost.Matnr;
                            taxaTransport.depart = itemCost.Spart;
                            taxaTransport.numeCost = itemCost.Maktx.ToUpper();

                            listTaxeTransp.Add(taxaTransport);
                            break;
                        }
                    }

                }

                Random rnd = new Random();
                foreach (SAPWebServices.ZstTransportCom itemTaxeMasini in resp.ItTransCom)
                {


                    TaxaMasina taxaMasina = new TaxaMasina();
                    taxaMasina.werks = itemTaxeMasini.Werks;
                    taxaMasina.vstel = itemTaxeMasini.Vstel;

                    taxaMasina.camionIveco = itemTaxeMasini.CamionIveco;
                    taxaMasina.camionScurt = itemTaxeMasini.CamionScurt;
                    taxaMasina.camionOricare = itemTaxeMasini.CamionOricare;
                    taxaMasina.macara = itemTaxeMasini.Macara;
                    taxaMasina.lift = itemTaxeMasini.Lift;

                    taxaMasina.taxaMacara = itemTaxeMasini.TaxaMacara.ToString();
                    taxaMasina.matnrMacara = itemTaxeMasini.MatnrMacara;
                    taxaMasina.maktxMacara = "PREST.SERV.DESCARCARE PALET";

                    taxaMasina.matnrZona = itemTaxeMasini.MatnrZona;
                    taxaMasina.maktxZona = itemTaxeMasini.MaktxZona;
                    taxaMasina.taxaZona = itemTaxeMasini.TaxaZona.ToString();
                    taxaMasina.taxaAcces = itemTaxeMasini.TaxaAcces.ToString();
                    taxaMasina.matnrAcces = itemTaxeMasini.MatnrAcces;
                    taxaMasina.maktxAcces = "Taxa extra metro";
                    taxaMasina.matnrTransport = itemTaxeMasini.MatnrTransport;
                    taxaMasina.maktxTransport = itemTaxeMasini.MaktxTransport;
                    taxaMasina.taxaTransport = itemTaxeMasini.TaxaTransport.ToString();
                    taxaMasina.spart = itemTaxeMasini.Spart;
                    taxaMasina.traty = itemTaxeMasini.Traty.Equals("TERA") ? "TERT" : itemTaxeMasini.Traty;

                    listTaxeMasini.Add(taxaMasina);
                }



                OracleConnection connection = new OracleConnection();
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                foreach (SAPWebServices.ZstEtMarfaPalet itemPalet in resp.ItMarfaPalet)
                {
                    ArticolPalet artPalet = new ArticolPalet();
                    artPalet.codPalet = itemPalet.MatnrPalet.TrimStart('0');
                    artPalet.depart = itemPalet.SpartPalet;
                    artPalet.cantitate = Decimal.ToInt32(itemPalet.CantPalet).ToString();
                    artPalet.numePalet = OperatiiMacara.getNumeArticol(connection, itemPalet.MatnrPalet);
                    artPalet.codArticol = itemPalet.MatnrMarfa.TrimStart('0');
                    artPalet.numeArticol = OperatiiMacara.getNumeArticol(connection, itemPalet.MatnrMarfa);
                    artPalet.furnizor = itemPalet.FurnizorPalet;
                    artPalet.pretUnit = itemPalet.PretPalet.ToString();
                    artPalet.cantArticol = itemPalet.CantMarfa.ToString();
                    artPalet.umArticol = itemPalet.MeinsMarfa;
                    listPaleti.Add(artPalet);


                }

                connection.Close();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("getTransportService: " + ex.ToString() + "\n\n" +  new JavaScriptSerializer().Serialize(antetCmd) 
                    + "\n\n" + new JavaScriptSerializer().Serialize(comandaMathaus));
            }

            if (antetCmd.tipTransp.Equals("TCLI") || antetCmd.tipTransp.Equals("TFRN"))
                dateTransport.listCostTransport = new List<CostTransportMathaus>();
            else
                dateTransport.listCostTransport = listTaxeTransp;

            dateTransport.listDepozite = listArticoleDepoz;
            dateTransport.taxeMasini = listTaxeMasini;
            dateTransport.listPaleti = listPaleti;

            return dateTransport;

        }


        private void trateazaLivrariGed(ComandaMathaus comandaMathaus, SAPWebServices.ZdetTransportResponse resp)
        {

            HashSet<string> filialeLivrare = new HashSet<string>();
            foreach (SAPWebServices.ZsitemsComanda itemCmd in resp.ItItems)
            {
                if (Utils.isUnitLogGed(itemCmd.Werks))
                    filialeLivrare.Add(itemCmd.Vstel);
            }


            foreach (string filLivrare in filialeLivrare)
            {
                foreach (SAPWebServices.ZsitemsComanda itemCmd in resp.ItItems)
                {
                    if (filLivrare.Equals(itemCmd.Vstel))
                    {
                        foreach (DateArticolMathaus articol in comandaMathaus.deliveryEntryDataList)
                        {
                            if (itemCmd.Matnr.Equals(articol.productCode))
                            {
                                articol.deliveryWarehouse = getULGed(articol.deliveryWarehouse);
                                break;
                            }
                        }
                    }
                }

            }


        }

        private DateTransportMathaus getTransportService_old(AntetCmdMathaus antetCmd, ComandaMathaus comandaMathaus)
        {
            DateTransportMathaus dateTransport = new DateTransportMathaus();
            List<CostTransportMathaus> listCostTransp = new List<CostTransportMathaus>();
            List<DepozitArticolTransport> listArticoleDepoz = new List<DepozitArticolTransport>();

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

                if (antetCmd.codPers != null)
                {
                    inParam.IpPernr = antetCmd.codPers;
                    inParam.IpTraty = antetCmd.tipTransp;
                }

                SAPWebServices.ZsitemsComanda[] items = new SAPWebServices.ZsitemsComanda[comandaMathaus.deliveryEntryDataList.Count];

                int ii = 0;
                foreach (DateArticolMathaus dateArticol in comandaMathaus.deliveryEntryDataList)
                {
                    items[ii] = new SAPWebServices.ZsitemsComanda();
                    items[ii].Matnr = dateArticol.productCode;
                    items[ii].Kwmeng = Decimal.Parse(dateArticol.quantity.ToString());
                    items[ii].Vrkme = dateArticol.unit;
                    items[ii].ValPoz = Decimal.Parse(String.Format("{0:0.00}", dateArticol.valPoz));
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

                    DepozitArticolTransport depozitArticol = new DepozitArticolTransport();
                    depozitArticol.codArticol = itemCmd.Matnr;
                    depozitArticol.filiala = itemCmd.Werks;
                    depozitArticol.depozit = itemCmd.Lgort;
                    listArticoleDepoz.Add(depozitArticol);

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
                            costTransp.depart = itemCost.Spart;
                            break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("getTransportService: " + ex.ToString());
            }

            dateTransport.listCostTransport = listCostTransp;
            dateTransport.listDepozite = listArticoleDepoz;

            return dateTransport;

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