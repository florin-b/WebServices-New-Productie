using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data;
using System.Globalization;

namespace WebService1
{
    public class OperatiiConcurenta
    {


        public OperatiiConcurenta()
        {
        }


        


        public string getCompaniiConcurente()
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

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = " select cod, companie from sapprd.zconcurenta where mandt = '900' order by cod ";

                oReader = cmd.ExecuteReader();

                List<CompanieConcurenta> listCompanii = new List<CompanieConcurenta>();
                CompanieConcurenta companie;
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        companie = new CompanieConcurenta();
                        companie.cod = oReader.GetInt32(0).ToString();
                        companie.nume = oReader.GetString(1);
                        listCompanii.Add(companie);
                    }

                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listCompanii);

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();
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


        public string getPretConcurenta(String codArt, string concurent, string codAgent)
        {
            string serializedResult = "";

            string localCodArt = codArt;
            if (codArt.Length == 8)
                localCodArt = "0000000000" + codArt;

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                cmd.CommandText = " select  to_char(to_date(datac,'yyyymmdd')), valoare from sapprd.zpretconcurenta where matnr=:matnr and codagent=:codagent " +
                                  " and codconcurent =:concurent order by datac desc, orac desc";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":matnr", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = localCodArt;

                cmd.Parameters.Add(":codagent", OracleType.NVarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAgent;

                cmd.Parameters.Add(":concurent", OracleType.Number, 5).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = Int32.Parse(concurent);

                oReader = cmd.ExecuteReader();

                List<PretConcurenta> listaPreturi = new List<PretConcurenta>();
                PretConcurenta unPret = null;

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        unPret = new PretConcurenta();
                        unPret.data = oReader.GetString(0);
                        unPret.valoare = oReader.GetDouble(1).ToString();
                        listaPreturi.Add(unPret);
                    }

                }

                oReader.Close();
                oReader.Dispose();
                cmd.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaPreturi);

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


        public string saveListPreturi(string codAgent, string dataSalvare, string listPreturi)
        {
            string retVal = "0", query;

            OracleConnection connection = new OracleConnection();

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            List<NewPretConcurenta> preturi = serializer.Deserialize<List<NewPretConcurenta>>(listPreturi);

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            string nowDate = Utils.getCurrentDate();
            string nowTime = Utils.getCurrentTime();

            connection.ConnectionString = connectionString;
            connection.Open();



            try
            {

                OracleCommand cmd = connection.CreateCommand();

                foreach (NewPretConcurenta unPret in preturi)
                {

                    saveObservatiiConcurenta(connection, codAgent, unPret);

                    string localCodArt = unPret.cod;
                    if (unPret.cod.Length == 8)
                        localCodArt = "0000000000" + unPret.cod;


                    if (!recordExists(connection, localCodArt, codAgent, unPret.concurent, unPret.valoare, dataSalvare))
                    {
                        query = " insert into sapprd.zpretconcurenta(mandt,matnr,codagent,datac,orac,valoare,codconcurent) " +
                                " values ('900',:matnr,:codag, :datac, :orac, :valoare, :concurent)";


                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = query;
                        cmd.Parameters.Clear();


                        cmd.Parameters.Add(":matnr", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = localCodArt;

                        cmd.Parameters.Add(":codag", OracleType.NVarChar, 54).Direction = ParameterDirection.Input;
                        cmd.Parameters[1].Value = codAgent;

                        cmd.Parameters.Add(":datac", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                        cmd.Parameters[2].Value = dataSalvare;

                        cmd.Parameters.Add(":orac", OracleType.NVarChar, 18).Direction = ParameterDirection.Input;
                        cmd.Parameters[3].Value = nowTime;

                        cmd.Parameters.Add(":valoare", OracleType.Double, 13).Direction = ParameterDirection.Input;
                        cmd.Parameters[4].Value = Double.Parse(unPret.valoare, CultureInfo.InvariantCulture);

                        cmd.Parameters.Add(":concurent", OracleType.Number, 5).Direction = ParameterDirection.Input;
                        cmd.Parameters[5].Value = Int32.Parse(unPret.concurent);

                        cmd.ExecuteNonQuery();
                    }


                }

                if (cmd != null)
                    cmd.Dispose();

            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();

            }

            return retVal;
        }


        private bool recordExists(OracleConnection connection, string codArt, string codAgent, string concurent, string valoare, string dataRec)
        {

            bool exists = false;

            try
            {
                OracleCommand cmd = connection.CreateCommand();

                string query = " update sapprd.zpretconcurenta set valoare =:valoare, datac =:dataSalvare  where codagent =:codAgent and matnr =:codArticol " +
                               " and codconcurent =:codConcurent and substr(datac,0,6) =:data";


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codArticol", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArt;

                cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAgent;

                cmd.Parameters.Add(":valoare", OracleType.Double, 13).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = Double.Parse(valoare, CultureInfo.InvariantCulture);

                cmd.Parameters.Add(":codConcurent", OracleType.Number, 5).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = Int32.Parse(concurent);

                cmd.Parameters.Add(":data", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = dataRec.Substring(0, 6);

                cmd.Parameters.Add(":dataSalvare", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = dataRec;

                int rowsAffected = cmd.ExecuteNonQuery();

                exists = rowsAffected > 0 ? true : false;

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                exists = false;
            }

            return exists;
        }

        public string addPretConcurenta(string codArt, string codAgent, string concurent, string valoare)
        {
            string retVal = "0", query;

            OracleConnection connection = new OracleConnection();

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            string nowDate = Utils.getCurrentDate();
            string nowTime = Utils.getCurrentTime();

            connection.ConnectionString = connectionString;
            connection.Open();

            string localCodArt = codArt;
            if (codArt.Length == 8)
                localCodArt = "0000000000" + codArt;

            try
            {

                OracleCommand cmd = connection.CreateCommand();

                query = " insert into sapprd.zpretconcurenta(mandt,matnr,codagent,datac,orac,valoare,codconcurent) " +
                        " values ('900',:matnr,:codag, :datac, :orac, :valoare, :concurent)";


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":matnr", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = localCodArt;

                cmd.Parameters.Add(":codag", OracleType.NVarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAgent;

                cmd.Parameters.Add(":datac", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = nowDate;

                cmd.Parameters.Add(":orac", OracleType.NVarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = nowTime;

                cmd.Parameters.Add(":valoare", OracleType.Double, 13).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = Double.Parse(valoare, CultureInfo.InvariantCulture);

                cmd.Parameters.Add(":concurent", OracleType.Number, 5).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = Int32.Parse(concurent);

                cmd.ExecuteNonQuery();

                if (cmd != null)
                    cmd.Dispose();

            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();

            }

            return retVal;
        }



        private void saveObservatiiConcurenta(OracleConnection connection, string codAgent, NewPretConcurenta articol)
        {

            if (!updateObservatiiArticol(connection, codAgent, articol))
                insertObservatiiArticol(connection, codAgent, articol);

        }



        private bool updateObservatiiArticol(OracleConnection connection, string codAgent, NewPretConcurenta articol)
        {

            bool exists = false;

            try
            {
                OracleCommand cmd = connection.CreateCommand();

                string query = " update sapprd.zobsconcurenta set obs =:obs where codarticol =:codArticol and ul = (select filiala from agenti where cod =:codAgent) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":obs", OracleType.NVarChar, 600).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = articol.observatii.Trim().Length > 0 ? articol.observatii : " ";

                cmd.Parameters.Add(":codArticol", OracleType.NVarChar, 54).Direction = ParameterDirection.Input;

                string localCodArt = articol.cod;
                if (articol.cod.Length == 8)
                    localCodArt = "0000000000" + articol.cod;

                cmd.Parameters[1].Value = localCodArt;

                cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codAgent;

                int rowsAffected = cmd.ExecuteNonQuery();

                exists = rowsAffected > 0 ? true : false;

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                exists = false;
            }

            return exists;


        }




        private void insertObservatiiArticol(OracleConnection connection, string codAgent, NewPretConcurenta articol)
        {


            try
            {
                OracleCommand cmd = connection.CreateCommand();

                string query = " insert into sapprd.zobsconcurenta(mandt, codarticol, ul, obs) values ('900',:codArticol, (select filiala from agenti where cod =:codAgent), :obs) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();


                cmd.Parameters.Add(":codArticol", OracleType.NVarChar, 54).Direction = ParameterDirection.Input;

                string localCodArt = articol.cod;
                if (articol.cod.Length == 8)
                    localCodArt = "0000000000" + articol.cod;

                cmd.Parameters[0].Value = localCodArt;

                cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAgent;

                cmd.Parameters.Add(":obs", OracleType.NVarChar, 600).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = articol.observatii.Trim().Length > 0 ? articol.observatii : " ";


                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }



        }



        private static bool isValoareActual(string dataActualizare, string tipActualizare)
        {

            DateTime dateThen = Convert.ToDateTime(dataActualizare);
            DateTime dateNow = DateTime.Now;

            int nrDays = (dateNow - dateThen).Days;

            switch (tipActualizare)
            {
                case "01":
                    return nrDays <= 14 ? true : false;
                case "02":
                    return nrDays <= 30 ? true : false;
                case "03":
                    return nrDays <= 180 ? true : false;
                default:
                    return false;

            }

        }

        public string getListArticoleConcurentaBulk(string codConcurent, string tipActualizare, string data, string codAgent)
        {



            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;
            string serializedResult = "";

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();


            string condFiliala = "";

            if (tipActualizare.Equals("03"))
                condFiliala = " and a.werks = (select filiala from agenti where cod = :codAgent ) ";


            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandType = CommandType.Text;

                cmd.CommandText = " select decode(length(a.matnr),18,substr(a.matnr,-8),a.matnr) codart, a.maktx, b.umvanz, nvl(c.valoare,-1),  " +
                                  " nvl((select obs from sapprd.zobsconcurenta where codarticol = a.matnr and ul = (select filiala from agenti where cod =:codAgent) ),' ') observatii" +
                                  " from sapprd.zmat_cocurenta a, " +
                                  " articole b, sapprd.zpretconcurenta c where a.matnr = b.cod and " +
                                  " c.matnr(+) = a.matnr and substr(c.datac(+), 0, 6) =:data and c.codconcurent(+) =:codConcurent " + condFiliala +
                                  " and c.codagent(+)=:codAgent and a.tip_actualiz =:tipActualizare  order by a.maktx ";


                

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":tipActualizare", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = tipActualizare;

                cmd.Parameters.Add(":data", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = data;

                cmd.Parameters.Add(":codConcurent", OracleType.Number, 5).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = Int32.Parse(codConcurent);

                cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = codAgent;



                oReader = cmd.ExecuteReader();

                List<ArticolConcurenta> listArticole = new List<ArticolConcurenta>();
                ArticolConcurenta articol;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        articol = new ArticolConcurenta();
                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        articol.umVanz = oReader.GetString(2);
                        articol.valoare = oReader.GetDecimal(3) == -1 ? "" : oReader.GetDecimal(3).ToString();
                        articol.dataValoare = " ";
                        articol.observatii = oReader.GetString(4).Trim();
                        listArticole.Add(articol);
                    }

                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listArticole);

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();
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


        public string getListArticoleConcurenta(string searchString, string tipArticol, string tipCautare, string tipActualizare)
        {
            string serializedResult = "";
            string condCautare = "";

            if (tipCautare.Equals("C"))
            {
                if (tipArticol.Equals("A"))
                    condCautare = " lower(decode(length(a.cod),18,substr(a.cod,-8),a.cod)) like lower('" + searchString + "%') ";

                if (tipArticol.Equals("S"))
                    condCautare = " a.sintetic in ('" + searchString + "') ";

            }


            if (tipCautare.Equals("N"))
            {
                if (tipArticol.Equals("A"))
                    condCautare = "  upper(a.nume) like upper('" + searchString.ToUpper() + "%')";

                if (tipArticol.Equals("S"))
                    condCautare = "  upper(b.nume) like upper('" + searchString.ToUpper() + "%')";

            }

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = " select decode(length(a.cod),18,substr(a.cod,-8),a.cod) codart, a.nume, a.umvanz " +
                                  " from articole a, sintetice b, sapprd.zmat_cocurenta c where c.mandt = '900' and c.matnr = a.cod  " +
                                  " and a.sintetic = b.cod and c.tip_actualiz =:tipActualizare and " + condCautare +
                                  " and rownum < 310 order by a.nume ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":tipActualizare", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = tipActualizare;

                oReader = cmd.ExecuteReader();

                List<ArticolCautare> listArticole = new List<ArticolCautare>();
                ArticolCautare articol;
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        articol = new ArticolCautare();
                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        articol.umVanz = oReader.GetString(2);
                        listArticole.Add(articol);
                    }

                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listArticole);

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();
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





    }
}