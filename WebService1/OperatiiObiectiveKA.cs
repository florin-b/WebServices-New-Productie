using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
namespace WebService1
{
    public class OperatiiObiectiveKA
    {

        private enum TipUser { AV, KA };

        public string salveazaEveniment(String eveniment)
        {



            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Eveniment evenimentObiectiv = serializer.Deserialize<Eveniment>(eveniment);

            OracleConnection connection = new OracleConnection();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.ConnectionString = connectionString;
                connection.Open();



                OracleCommand cmd = connection.CreateCommand();
                string query = "";

                query = " delete from sapprd.zobiect_urm where idobiectiv =:idObiectiv and codclient =:codClient and coddepart =:codDepart and codeveniment =:codEveniment ";
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idObiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(evenimentObiectiv.idObiectiv);

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 90).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = evenimentObiectiv.codClient;

                cmd.Parameters.Add(":codDepart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = evenimentObiectiv.codDepart;

                cmd.Parameters.Add(":codEveniment", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = evenimentObiectiv.codEveniment;

                cmd.ExecuteNonQuery();


                query = " insert into sapprd.zobiect_urm(mandt, idobiectiv, codclient, coddepart, codeveniment, data, observatii) values " +
                        " ('900', :idObiectiv, :codClient, :codDepart, :codEveniment, :data, :observatii) ";

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idObiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(evenimentObiectiv.idObiectiv);

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 90).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = evenimentObiectiv.codClient;

                cmd.Parameters.Add(":codDepart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = evenimentObiectiv.codDepart;

                cmd.Parameters.Add(":codEveniment", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = evenimentObiectiv.codEveniment;

                cmd.Parameters.Add(":data", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = formatData(evenimentObiectiv.data);

                cmd.Parameters.Add(":observatii", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = evenimentObiectiv.observatii.Trim().Length > 0 ? evenimentObiectiv.observatii.Trim() : " ";


                cmd.ExecuteNonQuery();

                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                return "-1";
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }


            return "0";

        }


        public string getAllEvenimente(string idObiectiv)
        {

            string evenimenteObiectiv = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                string sqlString = " select codeveniment, data, observatii, coddepart, codclient " +
                                   " from sapprd.zobiect_urm where idobiectiv = :idObiectiv order by data ";

                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idObiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idObiectiv);


                oReader = cmd.ExecuteReader();

                Eveniment eveniment = null;
                List<Eveniment> listEvenimente = new List<Eveniment>();


                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {


                        eveniment = new Eveniment();
                        eveniment.codDepart = oReader.GetString(3);
                        eveniment.codEveniment = oReader.GetInt32(0).ToString();
                        eveniment.data = oReader.GetString(1);
                        eveniment.observatii = oReader.GetString(2);
                        eveniment.codClient = oReader.GetString(4);
                        listEvenimente.Add(eveniment);
                    }

                }

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                evenimenteObiectiv = serializer.Serialize(listEvenimente);
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

            return evenimenteObiectiv;


        }


        public string getEvenimenteObiectiv(string idObiectiv, string codClient, string codDepart)
        {


            string evenimenteObiectiv = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                string sqlString = " select codeveniment, data, observatii " +
                                   " from sapprd.zobiect_urm where idobiectiv = :idObiectiv and codclient =:codClient and coddepart =:codDepart  ";

                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idObiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idObiectiv);

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 90).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codClient;

                cmd.Parameters.Add(":codDepart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codDepart;


                oReader = cmd.ExecuteReader();

                Eveniment eveniment = null;
                List<Eveniment> listEvenimente = new List<Eveniment>();


                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {


                        eveniment = new Eveniment();
                        eveniment.codDepart = codDepart;
                        eveniment.codEveniment = oReader.GetInt32(0).ToString();
                        eveniment.data = oReader.GetString(1);
                        eveniment.observatii = oReader.GetString(2);
                        listEvenimente.Add(eveniment);
                    }

                }

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                evenimenteObiectiv = serializer.Serialize(listEvenimente);
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

            return evenimenteObiectiv;
        }


        public string getStareClient(string codClient, string codDepartament)
        {
            string stareClient = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();


                string sqlString = " select distinct decode(trim(k.crblb),'','functional','blocat') stare, " +
                                   " 'nedefinit'  agent " +
                                   " from sapprd.knkk k where k.mandt='900' and  k.kunnr=:codClient  ";


                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;


                oReader = cmd.ExecuteReader();

                ObiectivConstructor constructor = new ObiectivConstructor();

                constructor.codClient = codClient;
                constructor.codDepart = codDepartament;

                if (oReader.HasRows)
                {
                    oReader.Read();
                    stareClient = "Client " + oReader.GetString(0) + ", agent " + oReader.GetString(1);
                }
                else
                {
                    stareClient = "Stare nedefinita";
                }

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

                constructor.stare = stareClient;

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                stareClient = serializer.Serialize(constructor);
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



            return stareClient;
        }


        public string getDetaliiObiectiv(string idObiectiv)
        {
            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;
            string serializedResult = "";

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                string sqlString = " select a.ul, a.codagent, a.datacreare, a.numeobiectiv, a.adresaobiectiv, a.beneficiar, a.antreprenor, a.arhitect, a.categorie, a.valoare, a.nrautorizatie," +
                                   " a.emitereautoriz, a.expirareautoriz, a.primariaemitenta, a.valoarefundatie, b.nume, a.id, a.status, a.codsuspendare from sapprd.zobiect_ka a, clienti b where a.id=:idObiectiv " +
                                   " and a.antreprenor = b.cod";

                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idObiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idObiectiv);

                oReader = cmd.ExecuteReader();

                ObiectivGeneral obiectiv = new ObiectivGeneral();


                if (oReader.HasRows)
                {
                    oReader.Read();

                    obiectiv.unitLog = oReader.GetString(0).Trim();
                    obiectiv.codAgent = oReader.GetString(1).Trim();
                    obiectiv.dataCreare = oReader.GetString(2);
                    obiectiv.numeObiectiv = oReader.GetString(3).Trim();
                    obiectiv.adresaObiectiv = oReader.GetString(4).Trim();
                    obiectiv.numeBeneficiar = oReader.GetString(5).Trim();
                    obiectiv.codAntreprenorGeneral = oReader.GetString(6);
                    obiectiv.numeArhitect = oReader.GetString(7).Trim();
                    obiectiv.codCategorieObiectiv = oReader.GetString(8).Trim();
                    obiectiv.valoareObiectiv = oReader.GetDouble(9).ToString().Trim();
                    obiectiv.nrAutorizatieConstructie = oReader.GetString(10).Trim();
                    obiectiv.dataEmitereAutorizatie = oReader.GetString(11).Trim();
                    obiectiv.dataExpirareAutorizatie = oReader.GetString(12).Trim();
                    obiectiv.primariaEmitenta = oReader.GetString(13).Trim();
                    obiectiv.valoareFundatie = oReader.GetDouble(14).ToString().Trim();
                    obiectiv.numeAntreprenorGeneral = oReader.GetString(15).Trim();
                    obiectiv.id = oReader.GetInt32(16).ToString();
                    obiectiv.codStadiuObiectiv = oReader.GetString(17).ToString();
                    obiectiv.codMotivSuspendare = oReader.GetString(18).ToString();

                }



                sqlString = " select distinct decode(trim(k.crblb),'','functional','blocat') stare, " +
                            " 'nedefinit'  agent, " +
                            " a.codclient, a.coddepart, b.nume " +
                            " from sapprd.knkk k, sapprd.zobiect_constr a, clienti b  where k.mandt='900' and k.kunnr=b.cod and " +
                            " a.id=:idObiectiv and a.codclient = b.cod ";


                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idObiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idObiectiv);

                ObiectivConstructor constructor = null;
                List<ObiectivConstructor> listConstructori = new List<ObiectivConstructor>();

                oReader = cmd.ExecuteReader();
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        constructor = new ObiectivConstructor();
                        constructor.codClient = oReader.GetString(2);
                        constructor.codDepart = oReader.GetString(3);
                        constructor.numeClient = oReader.GetString(4);
                        constructor.stare = "Client " + oReader.GetString(0) + ", agent " + oReader.GetString(1);
                        listConstructori.Add(constructor);

                    }
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                String strListConstructori = serializer.Serialize(listConstructori);

                sqlString = " select coddepart, codstare from sapprd.zobiect_stadii where id =:idObiectiv";

                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idObiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idObiectiv);

                ObiectivStadii stadiu = null;
                List<ObiectivStadii> listStadii = new List<ObiectivStadii>();

                oReader = cmd.ExecuteReader();
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        stadiu = new ObiectivStadii();
                        stadiu.codDepart = oReader.GetString(0);
                        stadiu.codStadiu = oReader.GetString(1);
                        listStadii.Add(stadiu);
                    }
                }

                String strStadii = serializer.Serialize(listStadii);



                obiectiv.constructori = strListConstructori;
                obiectiv.stadii = strStadii;
                obiectiv.evenimente = getAllEvenimente(idObiectiv);




                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

                serializedResult = serializer.Serialize(obiectiv);


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



        public string getClientiObiectiv(string idObiectiv)
        {
            string clienti = "";
            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            try
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();



                string sqlString = " select a.codclient, a.coddepart, b.nume from sapprd.zobiect_constr a, clienti b, sapprd.zobiect_stadii c " +
                            " where a.id=:idObiectiv and a.codclient = b.cod and c.id = a.id and " +
                            " decode(a.coddepart,'03','00','06','00','07','00', a.coddepart) = c.coddepart and c.codstare = 1 order by a.coddepart";

                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idObiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idObiectiv);



                oReader = cmd.ExecuteReader();

                ObiectivConstructor constructor = null;
                List<ObiectivConstructor> listaConstructori = new List<ObiectivConstructor>();


                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        constructor = new ObiectivConstructor();
                        constructor.codClient = oReader.GetString(0);
                        constructor.codDepart = oReader.GetString(1);
                        constructor.numeClient = oReader.GetString(2);
                        listaConstructori.Add(constructor);
                    }
                }

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                clienti = serializer.Serialize(listaConstructori);


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

            return clienti;
        }


        public string getListObiectiveAgenti(string codAgent, string filiala, string depart, string tipUser)
        {



            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;
            string serializedResult = "";

            string condAg = " and p.pernr =:codAgent ";
            if (tipUser.ToUpper().Equals("SD"))
            {
                condAg = " and p.pernr in (select distinct cod from agenti where filiala = '" + filiala + "' and divizie = '" + depart + "' and activ = 1) ";
            }


            string sqlString = " select distinct x.numeobiectiv, x.beneficiar,x.data,x.id,  x.status from ( " +
                               " select a.numeobiectiv, a.beneficiar, to_char(to_date(a.datacreare, 'yyyymmdd')) data, a.id, a.status , a.antreprenor " +
                               " from sapprd.zobiect_ka a, agenti ag where a.status <> '20' and " +
                               " (a.antreprenor in (select distinct p.kunnr from sapprd.knvp p where p.mandt = '900' and p.parvw in ('VE', 'ZC') " + condAg + ") ) " +
                               " UNION ALL " +
                               " select a.numeobiectiv, a.beneficiar, to_char(to_date(a.datacreare, 'yyyymmdd')), a.id, a.status ,  b.codclient " +
                               " from sapprd.zobiect_ka a,  sapprd.zobiect_constr b  where " +
                               " a.status <> '20' and " +
                               " (b.codclient in (select distinct p.kunnr from sapprd.knvp p where p.mandt = '900' and p.parvw in ('VE', 'ZC') " + condAg + ") ) " +
                               " and b.coddepart = '" + depart + "' and b.id = a.id) x  order by x.numeobiectiv ";


            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                if (tipUser.ToUpper().Equals("AV") || tipUser.ToUpper().Equals("KA"))
                {
                    cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codAgent;
                }


                oReader = cmd.ExecuteReader();

                List<BeanObiectiv> listaObiective = new List<BeanObiectiv>();
                BeanObiectiv obiectiv = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        obiectiv = new BeanObiectiv();
                        obiectiv.nume = oReader.GetString(0);
                        obiectiv.beneficiar = oReader.GetString(1);
                        obiectiv.data = oReader.GetString(2);
                        obiectiv.id = oReader.GetInt32(3).ToString();
                        obiectiv.codStatus = oReader.GetString(4);
                        listaObiective.Add(obiectiv);
                    }
                }

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaObiective);


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


        public string getListObiectiveKA(string codAgent, string filiala, string codJudet)
        {



            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            string sqlString = "";

            string condAgent = "";
            if (codAgent != null && codAgent != "")
                condAgent = " and codAgent =:codAgent ";


            string condJudet = "";
            if (codJudet != null && codJudet != "")
                condJudet = " and a.adresaobiectiv  like '" + codJudet + "%'";



            try
            {

                cmd.CommandType = CommandType.Text;


                sqlString = " select a.id, a.numeobiectiv, a.beneficiar, to_char(to_date(a.datacreare,'yyyymmdd')) , a.adresaobiectiv, a.status, b.nume " +
                            " from sapprd.zobiect_ka a, agenti b where a.status <> '20' and a.codagent=b.cod " + condAgent + condJudet;



                cmd.CommandText = sqlString;


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                if (codAgent != null && codAgent != "")
                {
                    cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codAgent;
                }


                oReader = cmd.ExecuteReader();

                List<BeanObiectiv> listObiective = new List<BeanObiectiv>();
                BeanObiectiv obiectiv;
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        obiectiv = new BeanObiectiv();
                        obiectiv.id = oReader.GetInt32(0).ToString();
                        obiectiv.nume = oReader.GetString(1);
                        obiectiv.beneficiar = oReader.GetString(2);
                        obiectiv.data = oReader.GetString(3);
                        obiectiv.adresa = oReader.GetString(4);
                        obiectiv.codStatus = oReader.GetString(5);
                        obiectiv.numeAgent = oReader.GetString(6);
                        listObiective.Add(obiectiv);
                    }

                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listObiective);

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



        public string getListObiective(string codAgent, string filiala, string tip, string interval, string depart)
        {


            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;
            string condInterval = "", dataInterval = "";
            string condAgent = "";
            string serializedResult = "";

            if (interval == "0") //astazi
            {
                dataInterval = DateTime.Today.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condInterval = " and a.datacreare =:dataSelectie ";
            }

            if (interval == "1") //ultimele 7 zile
            {
                dataInterval = DateTime.Today.AddDays(-7).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condInterval = " and a.datacreare >=:dataSelectie ";
            }

            if (interval == "2") //ultimele 30 zile
            {
                dataInterval = DateTime.Today.AddDays(-30).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condInterval = " and a.datacreare >=:dataSelectie ";
            }

            if (interval == "3") //mai mult de 30 zile
            {
                dataInterval = DateTime.Today.AddDays(-30).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condInterval = " and a.datacreare <:dataSelectie ";
            }

            // string condAgent = "";
            if (!codAgent.Equals("-1") && !codAgent.Equals("00"))
            {
                condAgent = " and a.codagent = :codAgent ";
            }


            string condTip = "";

            //modificare
            if (tip.Equals("1"))
                condTip = "  a.status <> '20' and ";

            //urmarire
            if (tip.Equals("2"))
                condTip = "  a.status = '0' and ";

            string sqlString = "";

            if (!codAgent.Equals("-1") && !codAgent.Equals("00"))
            {
                sqlString = " select a.numeobiectiv, a.beneficiar, to_char(to_date(a.datacreare,'yyyymmdd')), id, status, c.nume from sapprd.zobiect_ka a, agenti c where " +
                             condTip +
                             "  c.cod = a.codagent and " +
                             " a.antreprenor in (select distinct kunnr from sapprd.knvp where mandt = '900' and parvw in ('VE', 'ZC') and pernr =:codAgent ) " + condInterval +
                             " order by c.nume, a.datacreare desc ";

            }
            else
            {
                sqlString = " select a.numeobiectiv, a.beneficiar, to_char(to_date(a.datacreare,'yyyymmdd')), a.id, a.status, c.nume from sapprd.zobiect_ka a, sapprd.zobiect_stadii b, " +
                            " agenti c where " + condTip + "  c.cod = a.codagent and " +
                            " substr(a.ul,0,2)=:filiala and a.id = b.id and b.coddepart=:codDepart and b.codstare <> 0 " + condInterval + " order by c.nume, a.datacreare desc ";


            }


            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();




                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                int paramOrd = 0;

                if (!interval.Equals("-1"))
                {
                    cmd.Parameters.Add(":dataSelectie", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[paramOrd++].Value = dataInterval;
                }



                if (!codAgent.Equals("-1") && !codAgent.Equals("00"))
                {
                    cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[paramOrd++].Value = codAgent;
                }
                else
                {
                    cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                    cmd.Parameters[paramOrd++].Value = filiala.Substring(0, 2);

                    cmd.Parameters.Add(":codDepart", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[paramOrd].Value = depart;
                }





                oReader = cmd.ExecuteReader();

                List<BeanObiectiv> listaObiective = new List<BeanObiectiv>();
                BeanObiectiv obiectiv = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        obiectiv = new BeanObiectiv();
                        obiectiv.nume = oReader.GetString(0);
                        obiectiv.beneficiar = oReader.GetString(1);
                        obiectiv.data = oReader.GetString(2);
                        obiectiv.id = oReader.GetInt32(3).ToString();
                        obiectiv.codStatus = oReader.GetString(4);
                        obiectiv.numeAgent = oReader.GetString(5);

                        listaObiective.Add(obiectiv);
                    }
                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaObiective);


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



        public string adaugaObiectiv(string dateObiectiv)
        {




            JavaScriptSerializer serializer = new JavaScriptSerializer();
            ObiectivGeneral obiective = serializer.Deserialize<ObiectivGeneral>(dateObiectiv);
            List<ObiectivConstructor> listConstructori = serializer.Deserialize<List<ObiectivConstructor>>(obiective.constructori);
            List<ObiectivStadii> listStadii = serializer.Deserialize<List<ObiectivStadii>>(obiective.stadii);

            OracleConnection connection = new OracleConnection();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();
                string query = "";


                string conditieId = " pk_clp.nextval ";

                if (obiective.id.Trim().Length > 0)
                {
                    query = " delete from sapprd.zobiect_ka where id=:idObiectiv ";
                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idObiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = obiective.id;
                    cmd.ExecuteNonQuery();

                    query = " delete from sapprd.zobiect_constr where id=:idObiectiv ";
                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idObiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = obiective.id;
                    cmd.ExecuteNonQuery();

                    query = " delete from sapprd.zobiect_stadii where id=:idObiectiv ";
                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idObiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = obiective.id;
                    cmd.ExecuteNonQuery();

                    conditieId = obiective.id.Trim();

                }


                query = " insert into sapprd.zobiect_ka(mandt, id, ul, codagent, datacreare, status, numeobiectiv, adresaobiectiv, beneficiar, antreprenor, arhitect, " +
                        " categorie, valoare, nrautorizatie, emitereautoriz, expirareautoriz, " +
                        " primariaemitenta, valoarefundatie, codsuspendare) values ('900', " + conditieId + " , :ul, :codag, :datac, :status, :numeobiectiv, :adresaobiectiv,  " +
                        " :beneficiar, :antreprenor, :arhitect, :categorie, :valoare, :nrautorizatie, :emitereautorizatie, :expirareautoriz, " +
                        " :primarie, :valoarefundatie, :codsuspendare) returning id into :id ";

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":ul", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = obiective.unitLog;

                cmd.Parameters.Add(":codag", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = obiective.codAgent;

                cmd.Parameters.Add(":datac", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = formatData(obiective.dataCreare);

                cmd.Parameters.Add(":status", OracleType.VarChar, 12).Direction = ParameterDirection.Input;

                string statusOb = obiective.codStadiuObiectiv;
                if (Convert.ToBoolean(obiective.inchis))
                    statusOb = "20";

                cmd.Parameters[3].Value = statusOb;

                cmd.Parameters.Add(":numeobiectiv", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = obiective.numeObiectiv;

                cmd.Parameters.Add(":adresaobiectiv", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = obiective.adresaObiectiv;

                cmd.Parameters.Add(":beneficiar", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = obiective.numeBeneficiar;

                cmd.Parameters.Add(":antreprenor", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[7].Value = obiective.codAntreprenorGeneral;

                cmd.Parameters.Add(":arhitect", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[8].Value = obiective.numeArhitect.Trim().Length > 0 ? obiective.numeArhitect : " ";

                cmd.Parameters.Add(":categorie", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[9].Value = obiective.codCategorieObiectiv.Trim().Length > 0 ? obiective.codCategorieObiectiv : " ";

                cmd.Parameters.Add(":valoare", OracleType.Number, 13).Direction = ParameterDirection.Input;
                cmd.Parameters[10].Value = obiective.valoareObiectiv.Trim().Length > 0 ? Double.Parse(obiective.valoareObiectiv) : 0;

                cmd.Parameters.Add(":nrautorizatie", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[11].Value = obiective.nrAutorizatieConstructie.Trim().Length > 0 ? obiective.nrAutorizatieConstructie : " ";

                cmd.Parameters.Add(":emitereautorizatie", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[12].Value = obiective.dataEmitereAutorizatie.Trim().Length > 0 ? obiective.dataEmitereAutorizatie : " ";

                cmd.Parameters.Add(":expirareautoriz", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[13].Value = obiective.dataExpirareAutorizatie.Trim().Length > 0 ? obiective.dataExpirareAutorizatie : " ";

                cmd.Parameters.Add(":primarie", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                cmd.Parameters[14].Value = obiective.primariaEmitenta;

                cmd.Parameters.Add(":valoarefundatie", OracleType.Number, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[15].Value = obiective.valoareFundatie.Trim().Length > 0 ? Double.Parse(obiective.valoareFundatie) : 0;

                cmd.Parameters.Add(":codsuspendare", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[16].Value = obiective.codMotivSuspendare;

                OracleParameter idCmd = new OracleParameter("id", OracleType.Number);
                idCmd.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(idCmd);

                cmd.ExecuteNonQuery();


                foreach (ObiectivConstructor constructor in listConstructori)
                {
                    query = " insert into sapprd.zobiect_constr (mandt, id, codclient, coddepart) values ('900', :idobiectiv, :codclient, :coddepart) ";

                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":idobiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = idCmd.Value;

                    cmd.Parameters.Add(":codclient", OracleType.VarChar, 90).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = constructor.codClient;

                    cmd.Parameters.Add(":coddepart", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = constructor.codDepart;

                    cmd.ExecuteNonQuery();


                }


                foreach (ObiectivStadii stadiu in listStadii)
                {
                    query = " insert into sapprd.zobiect_stadii (mandt, id, coddepart, codstare) values ('900', :idobiectiv, :coddepart, :codstare) ";

                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":idobiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = idCmd.Value;

                    cmd.Parameters.Add(":coddepart", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = stadiu.codDepart;

                    cmd.Parameters.Add(":codstare", OracleType.VarChar, 15).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = stadiu.codStadiu;

                    cmd.ExecuteNonQuery();


                }



                updateStadiuConstructori(Int32.Parse(idCmd.Value.ToString()), listConstructori);



            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                return "-1";
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            AlarmaObiectiveKA alarma = new AlarmaObiectiveKA();
            alarma.sendAlarmObiectiv(dateObiectiv);

            return "0";
        }



        private void updateStadiuConstructori(int idObiectiv, List<ObiectivConstructor> listConstructori)
        {


            OracleConnection connection = new OracleConnection();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();
                OracleDataReader oReader = null;
                string query = "";

                query = " select  codclient, coddepart from sapprd.zobiect_constr where id=:idobiectiv ";

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idobiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idObiectiv;


                oReader = cmd.ExecuteReader();


                string codClient = "", codDepart = "";

                if (oReader.HasRows)
                {
                outer:
                    while (oReader.Read())
                    {

                        codClient = oReader.GetString(0);
                        codDepart = oReader.GetString(1);

                        foreach (ObiectivConstructor constructor in listConstructori)
                        {
                            if (codClient.Equals(constructor.codClient) && codDepart.Equals(constructor.codDepart))
                            {
                                goto outer;
                            }

                        }

                        query = " delete from sapprd.zobiect_constr where id=:idobiectiv and codclient =:codclient and coddepart=:coddepart ";

                        cmd.CommandText = query;
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":idobiectiv", OracleType.Number, 11).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = idObiectiv;

                        cmd.Parameters.Add(":codclient", OracleType.VarChar, 90).Direction = ParameterDirection.Input;
                        cmd.Parameters[1].Value = codClient;

                        cmd.Parameters.Add(":coddepart", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                        cmd.Parameters[2].Value = codDepart;

                        cmd.ExecuteNonQuery();

                    }
                }


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
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }


        }




        public string getObiectiveDepartament(string filiala, string departament, string codClient, string tipUser, string codUser)
        {

            if (tipUser.ToUpper().Equals(TipUser.AV.ToString()))
            {
                return getListObiectiveAgenti(filiala, departament, codClient, tipUser, codUser);
            }
            else if (tipUser.ToUpper().Equals(TipUser.KA.ToString()))
            {
                return getListObiectiveKA(filiala, departament, codClient, tipUser, codUser);
            }

            return null;


        }

        private string getListObiectiveAgenti(string filiala, string departament, string codClient, string tipUser, string codUser)
        {
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            string sqlString = "";

            try
            {

                cmd.CommandType = CommandType.Text;

                sqlString = " select a.id, a.numeobiectiv, a.beneficiar, to_char(to_date(a.datacreare,'yyyymmdd')) ,a.adresaobiectiv from sapprd.zobiect_ka a, " +
                            " sapprd.zobiect_constr b where a.id = b.id " +
                            " and a.ul=:unitLog and b.coddepart =:depart and b.codclient=:codClient and a.status = '0' ";




                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();


                cmd.Parameters.Add(":unitLog", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                cmd.Parameters.Add(":depart", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = departament;

                cmd.Parameters.Add(":codClient", OracleType.NVarChar, 90).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codClient;


                oReader = cmd.ExecuteReader();

                List<ObiectivDepartament> listObiective = new List<ObiectivDepartament>();
                ObiectivDepartament obiectiv;
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        obiectiv = new ObiectivDepartament();
                        obiectiv.id = oReader.GetInt32(0).ToString();
                        obiectiv.nume = oReader.GetString(1);
                        obiectiv.beneficiar = oReader.GetString(2);
                        obiectiv.dataCreare = oReader.GetString(3);
                        obiectiv.adresa = oReader.GetString(4);
                        listObiective.Add(obiectiv);
                    }

                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listObiective);

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


        private string getListObiectiveKA(string filiala, string departament, string codClient, string tipUser, string codUser)
        {
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            string sqlString = "";

            try
            {

                cmd.CommandType = CommandType.Text;


                sqlString = " select distinct a.id, a.numeobiectiv, a.beneficiar, to_char(to_date(a.datacreare,'yyyymmdd')) , a.adresaobiectiv " +
                            " from sapprd.zobiect_ka a where a.codagent =:codAgent and a.status = '0'  and " +
                            " (a.antreprenor =:codClient or " +
                            " exists(select 1 from sapprd.zobiect_constr where mandt = '900' and codclient =:codClient and id = a.id )) " +
                            " order by numeobiectiv ";



                cmd.CommandText = sqlString;


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();


                cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codUser;

                cmd.Parameters.Add(":codClient", OracleType.NVarChar, 90).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codClient;



                oReader = cmd.ExecuteReader();

                List<ObiectivDepartament> listObiective = new List<ObiectivDepartament>();
                ObiectivDepartament obiectiv;
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        obiectiv = new ObiectivDepartament();
                        obiectiv.id = oReader.GetInt32(0).ToString();
                        obiectiv.nume = oReader.GetString(1);
                        obiectiv.beneficiar = oReader.GetString(2);
                        obiectiv.dataCreare = oReader.GetString(3);
                        obiectiv.adresa = oReader.GetString(4);
                        listObiective.Add(obiectiv);
                    }

                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listObiective);

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


        private static string formatData(string dataToFormat)
        {

            string[] tokenData = dataToFormat.Split('.');
            return tokenData[2] + tokenData[1] + tokenData[0];

        }

        private string getCurrentDate()
        {
            string mDate = "";
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            mDate = year + month + day;
            return mDate;
        }





    }

}