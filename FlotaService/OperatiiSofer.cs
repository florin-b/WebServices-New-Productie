using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data;
using System.Globalization;


namespace FlotaService
{
    public class OperatiiSofer
    {



        public string getListaSoferi(String filiala)
        {
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();
                cmd = connection.CreateCommand();

                cmd.CommandText = " select nume, cod from soferi where fili =:filiala order by nume ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                oReader = cmd.ExecuteReader();

                Sofer sofer = null;
                List<Sofer> listSoferi = new List<Sofer>();
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        sofer = new Sofer();
                        sofer.nume = oReader.GetString(0);
                        sofer.cod = oReader.GetString(1);
                        listSoferi.Add(sofer);
                    }
                }


                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listSoferi);


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }



            return serializedResult;
        }



        public string getListaMasini(String filiala)
        {
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();
                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct a.ktext, nvl(b.id,-1) from sapprd.coas a, gps_masini b where  a.phas3<>'X' and " + 
                                  " a.werks !=' ' and a.mandt = '900' and a.auart = '1001' and replace(a.ktext, '-') = b.nr_masina and a.werks=:filiala ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                oReader = cmd.ExecuteReader();

                Masina masina = null;
                List<Masina> listMasini = new List<Masina>();
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        masina = new Masina();
                        masina.nrAuto = oReader.GetString(0);
                        masina.deviceId = oReader.GetInt32(1).ToString();
                        listMasini.Add(masina);
                    }
                }


                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listMasini);


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }



            return serializedResult;
        }



        public string getActivitateSofer(string codSofer, string dataStartInterval, string dataStopInterval)
        {
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();
                cmd = connection.CreateCommand();

                cmd.CommandText = " select to_char(to_date(data,'yyyymmdd')) data, document, fms, ora, data data2 from sapprd.zevenimentsofer where codsofer=:codSofer " +
                                  " and data between :dataStart and :dataStop order by data2, ora ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                cmd.Parameters.Add(":dataStart", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = dataStartInterval;

                cmd.Parameters.Add(":dataStop", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = dataStopInterval;


                oReader = cmd.ExecuteReader();

                ActivitateBorderou actBorderou = null;
                List<ActivitateBorderou> listBorderouri = new List<ActivitateBorderou>();


                int poz = 0;
                string oraStart = "0";
                double kmStart = 0;
                double kmStop = 0;
                string dataStart = "0";
                string varKm = "0";
                string dataEv = "0";
                string oraEv = "0";
                string bord = "0";
                double difKm = 0;
                string dataStop = "0";
                string oraStop = "0";
                string timpStart = "0", timpStop = "0";
                string diffTime = "0";
                string resultString = "";
                string dataEveniment = "";

                DateTime dateStart;
                DateTime dateStop;
                TimeSpan ts;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        if (poz == 0)
                        {
                            if (oraStart.Equals("0"))
                            {

                                if (!oReader.GetString(2).Equals("0"))
                                {
                                    kmStart = Double.Parse(oReader.GetString(2));
                                }


                                dataStart = oReader.GetString(0);
                                oraStart = oReader.GetString(3);

                            }
                            else
                            {
                                kmStart = Double.Parse(varKm);
                                dataStart = dataEv;
                                oraStart = oraEv;
                            }


                        }


                        if (!bord.Equals(oReader.GetString(1)) && poz > 0)
                        {
                            kmStop = Double.Parse(varKm);
                            poz = -1;
                            difKm = kmStop - kmStart;


                            dataStop = dataEv;
                            oraStop = oraEv;

                            timpStart = dataStart + " " + oraStart.Substring(0, 2) + ":" + oraStart.Substring(2, 2) + ":" + oraStart.Substring(4, 2);
                            timpStop = dataStop + " " + oraStop.Substring(0, 2) + ":" + oraStop.Substring(2, 2) + ":" + oraStop.Substring(4, 2);

                            dateStart = Convert.ToDateTime(timpStart);
                            dateStop = Convert.ToDateTime(timpStop);

                            ts = dateStop - dateStart;
                            diffTime = getDuration(ts);


                            resultString += bord + "#" + dataEveniment + "#" + difKm + "#" + diffTime + "@";

                            actBorderou = new ActivitateBorderou();
                            actBorderou.data = dataEveniment;
                            actBorderou.nr = bord;
                            actBorderou.km = difKm.ToString();
                            actBorderou.durata = diffTime;
                            listBorderouri.Add(actBorderou);
                        }


                        bord = oReader.GetString(1);
                        dataEveniment = oReader.GetString(0);
                        dataEv = oReader.GetString(0);
                        oraEv = oReader.GetString(3);
                        varKm = oReader.GetString(2);


                        poz++;



                    }//end while




                    //ultimul client

                    kmStop = Double.Parse(varKm);
                    difKm = kmStop - kmStart;




                    dataStop = dataEv;
                    oraStop = oraEv;

                    timpStart = dataStart + " " + oraStart.Substring(0, 2) + ":" + oraStart.Substring(2, 2) + ":" + oraStart.Substring(4, 2);
                    timpStop = dataStop + " " + oraStop.Substring(0, 2) + ":" + oraStop.Substring(2, 2) + ":" + oraStop.Substring(4, 2);

                    dateStart = Convert.ToDateTime(timpStart);
                    dateStop = Convert.ToDateTime(timpStop);

                    ts = dateStop - dateStart;
                    diffTime = getDuration(ts);

                    resultString += bord + "#" + dataEveniment + "#" + difKm + "#" + diffTime + "@";

                }//end if


                actBorderou = new ActivitateBorderou();
                actBorderou.data = dataEveniment;
                actBorderou.nr = bord;
                actBorderou.km = difKm.ToString();
                actBorderou.durata = diffTime;
                listBorderouri.Add(actBorderou);

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listBorderouri);

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " cod sofer = " + codSofer + " , " + dataStartInterval + " , " + dataStopInterval);
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }

            return serializedResult;
        }



        public string getActivitateDocument(string nrDocument)
        {
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            List<DetaliiBorderou> listDetalii = new List<DetaliiBorderou>();
            DetaliiBorderou borderou = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();
                cmd = connection.CreateCommand();

                cmd.CommandText = " select a.data,a.ora,a.client,nvl(c.nume,0) nume, " +
                    " nvl((select max(to_date(s.data,'yyyymmdd')||':'||s.ora||':'||s.gps||':'||s.fms) from sapprd.zevenimentsofer s where s.document = a.document and " +
                    " s.eveniment = 'P' and s.client = a.client),0) plecare, " +
                    " nvl((select max(to_date(s.data,'yyyymmdd')||':'||s.ora||':'||s.gps||':'||s.fms) from sapprd.zevenimentsofer s where s.document = a.document and " +
                    " s.eveniment = 'S' and s.client = a.client),0) sosire " +
                    " from sapprd.zevenimentsofer a, clienti c " +
                    " where a.document =:bord and a.client = c.cod(+) order by a.data,a.ora ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":bord", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;



                int k = 0;
                string startBorderou = "";
                string stopBorderou = "";
                string client = "";
                string selClient = "";
                string gpsPlecare = "";

                string durataStationare = "";
                string durataTraseu = "";

                string timpStartTraseu = "";
                string timpStopTraseu = "";
                double kmStartTraseu = 0;
                double kmStopTraseu = 0;
                double kmTraseu = 0;

                string gpsStart = "";
                string gpsStop = "";
                double localKm = 0;
                string[] localVarPlecare, localVarSosire;
                string varStartTraseu = "", varStopTraseu = "";
                string timpStart = "", timpStop = "";



                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        if (!selClient.Equals(oReader.GetString(2)) || k == 0)
                        {

                            if (oReader.GetString(3) != null && oReader.GetString(3).Equals("0") && k == 0)
                            {
                                client = "Borderou";
                                if (oReader.GetString(5).Contains(':'))
                                    localVarPlecare = oReader.GetString(5).Split(':');

                                startBorderou = "";

                                localVarSosire = oReader.GetString(4).Split(':');
                                stopBorderou = localVarSosire[0] + " " + localVarSosire[1].Substring(0, 2) + ":" + localVarSosire[1].Substring(2, 2) + ":" + localVarSosire[1].Substring(4, 2);

                                varStartTraseu = localVarSosire[0] + " " + localVarSosire[1].Substring(0, 2) + ":" + localVarSosire[1].Substring(2, 2) + ":" + localVarSosire[1].Substring(4, 2);
                                timpStartTraseu = localVarSosire[0] + " " + localVarSosire[1].Substring(0, 2) + ":" + localVarSosire[1].Substring(2, 2) + ":" + localVarSosire[1].Substring(4, 2);

                                if (!localVarSosire[3].Equals("0"))
                                {
                                    localKm = Double.Parse(localVarSosire[3], CultureInfo.InvariantCulture);
                                }



                                kmStartTraseu = localKm;

                                gpsStart = "";
                                gpsStop = localVarSosire[2];
                                durataStationare = "";

                            }
                            else if (oReader.GetString(3).Equals("0") && k > 0)
                            {
                                client = "Borderou";
                                localVarPlecare = oReader.GetString(5).Split(':');
                                startBorderou = localVarPlecare[0] + " " + localVarPlecare[1].Substring(0, 2) + ":" + localVarPlecare[1].Substring(2, 2) + ":" + localVarPlecare[1].Substring(4, 2);

                                localVarSosire = oReader.GetString(4).Split(':');
                                stopBorderou = "";

                                varStopTraseu = localVarPlecare[0] + " " + localVarPlecare[1].Substring(0, 2) + ":" + localVarPlecare[1].Substring(2, 2) + ":" + localVarPlecare[1].Substring(4, 2);
                                timpStopTraseu = localVarPlecare[0] + " " + localVarPlecare[1].Substring(0, 2) + ":" + localVarPlecare[1].Substring(2, 2) + ":" + localVarPlecare[1].Substring(4, 2);

                                durataTraseu = "0";

                                if (!localVarPlecare[3].Equals("0"))
                                {
                                    localKm = Double.Parse(localVarPlecare[3], CultureInfo.InvariantCulture);

                                }


                                kmStopTraseu = localKm;

                                kmTraseu = kmStopTraseu - kmStartTraseu;

                                gpsStart = localVarPlecare[2];
                                gpsStop = "";
                                durataStationare = "";

                            }
                            else
                            {
                                client = oReader.GetString(3);
                                localVarPlecare = oReader.GetString(5).Split(':');
                                startBorderou = localVarPlecare[0] + " " + localVarPlecare[1].Substring(0, 2) + ":" + localVarPlecare[1].Substring(2, 2) + ":" + localVarPlecare[1].Substring(4, 2);

                                localVarSosire = oReader.GetString(4).Split(':');

                                if (!localVarSosire[0].Equals(""))
                                    stopBorderou = localVarSosire[0] + " " + localVarSosire[1].Substring(0, 2) + ":" + localVarSosire[1].Substring(2, 2) + ":" + localVarSosire[1].Substring(4, 2);


                                varStopTraseu = localVarPlecare[0] + " " + localVarPlecare[1].Substring(0, 2) + ":" + localVarPlecare[1].Substring(2, 2) + ":" + localVarPlecare[1].Substring(4, 2);
                                timpStopTraseu = localVarPlecare[0] + " " + localVarPlecare[1].Substring(0, 2) + ":" + localVarPlecare[1].Substring(2, 2) + ":" + localVarPlecare[1].Substring(4, 2);

                                if (!localVarPlecare[3].Equals("0"))
                                {
                                    localKm = Double.Parse(localVarPlecare[3], CultureInfo.InvariantCulture);
                                }


                                kmStopTraseu = localKm;

                                kmTraseu = kmStopTraseu - kmStartTraseu;

                                timpStart = localVarPlecare[0] + " " + localVarPlecare[1].Substring(0, 2) + ":" + localVarPlecare[1].Substring(2, 2) + ":" + localVarPlecare[1].Substring(4, 2);
                                durataTraseu = "0";


                                if (!localVarSosire[0].Equals(""))
                                {
                                    timpStop = localVarSosire[0] + " " + localVarSosire[1].Substring(0, 2) + ":" + localVarSosire[1].Substring(2, 2) + ":" + localVarSosire[1].Substring(4, 2);
                                    varStartTraseu = localVarSosire[0] + " " + localVarSosire[1].Substring(0, 2) + ":" + localVarSosire[1].Substring(2, 2) + ":" + localVarSosire[1].Substring(4, 2);

                                    timpStartTraseu = localVarSosire[0] + " " + localVarSosire[1].Substring(0, 2) + ":" + localVarSosire[1].Substring(2, 2) + ":" + localVarSosire[1].Substring(4, 2);


                                    if (!localVarSosire[3].Equals("0"))
                                    {
                                        localKm = Double.Parse(localVarSosire[3], CultureInfo.InvariantCulture);
                                    }




                                    kmStartTraseu = localKm;
                                    gpsStart = localVarSosire[2];
                                    gpsStop = localVarPlecare[2];
                                }
                                else
                                {
                                    kmStartTraseu = 0;
                                }

                                TimeSpan ts = Convert.ToDateTime(timpStop) - Convert.ToDateTime(timpStart);
                                durataStationare = getDuration(ts);


                            }

                            k++;

                            borderou = new DetaliiBorderou();
                            borderou.client = client;
                            borderou.oraSosire = startBorderou;
                            borderou.locatieSosire = gpsStop;
                            borderou.oraPlecare = stopBorderou;
                            borderou.locatiePlecare = gpsStart;
                            borderou.durataStationare = durataStationare;
                            borderou.distanta = kmTraseu.ToString();
                            listDetalii.Add(borderou);

                        }


                        selClient = oReader.GetString(2);

                    }//sf. while


                }//sf. if


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " doc = " + nrDocument);
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }


            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializedResult = serializer.Serialize(listDetalii);
            return serializedResult;
        }



        public string getRutaDocument(string nrDocument)
        {


            string idMasina = "";
            string minData = "", maxData = "";
            string coordGps = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select  g.id from sapprd.vekp v, gps_masini g where " +
                                  " v.vpobjkey =:nrBord and REPLACE(v.exidv, '-') =g.nr_masina ";


                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrBord", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    idMasina = oReader.GetInt32(0).ToString();
                }


                cmd.CommandText = " select to_char(min(TO_DATE(data||' '||substr(ora,0,6),'YYYY-MM-DD HH24MISS')),'DD-MM-YYYY HH24:MI:SS'), " +
                                  " to_char(max(TO_DATE(data||' '||substr(ora,0,6),'YYYY-MM-DD HH24MISS')),'DD-MM-YYYY HH24:MI:SS') from sapprd.zevenimentsofer where document =:nrBord ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrBord", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    minData = oReader.GetString(0);
                    maxData = oReader.GetString(1);
                }

                cmd.CommandText = " select latitude, longitude, to_char(record_time,'dd-Mon-yy hh24:mi:ss') from gps_date where device_id =:deviceId and " +
                                  " record_time between to_date('" + minData + "','DD-MM-YYYY HH24:MI:SS')  " +
                                  " and to_date('" + maxData + "','DD-MM-YYYY HH24:MI:SS') and speed > 0 order by record_time ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":deviceId", OracleType.Number).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Decimal.Parse(idMasina);

                oReader = cmd.ExecuteReader();
                coordGps = "";


                DateTime dateStart;
                DateTime dateStop;


                TimeSpan ts = new TimeSpan();

                int ii = 0;
                string recordTime = "0";
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {


                        if (!recordTime.Equals("0"))
                        {
                            dateStart = Convert.ToDateTime(recordTime);
                            dateStop = Convert.ToDateTime(oReader.GetString(2));
                            ts = dateStop - dateStart;
                        }


                        if (ts.Minutes > 10 || recordTime.Equals("0"))
                        {
                            if (coordGps.Equals(""))
                                coordGps = oReader.GetDouble(0).ToString().Replace(',', '.') + "," + oReader.GetDouble(1).ToString().Replace(',', '.');
                            else
                                coordGps += "|" + oReader.GetDouble(0).ToString().Replace(',', '.') + "," + oReader.GetDouble(1).ToString().Replace(',', '.');

                            recordTime = oReader.GetString(2);
                        }
                        ii++;



                    }
                }

                oReader.Close();
                oReader.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + nrDocument);
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }



            return coordGps;
        }


        public string getRutaDocument2(string nrDocument)
        {


            string idMasina = "";
            string minData = "", maxData = "";
            string coordGps = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            List<String> listaCoordonate = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select  g.id from sapprd.vekp v, gps_masini g where " +
                                  " v.vpobjkey =:nrBord and REPLACE(v.exidv, '-') =g.nr_masina ";


                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrBord", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    idMasina = oReader.GetInt32(0).ToString();
                }


                cmd.CommandText = " select to_char(min(TO_DATE(data||' '||substr(ora,0,6),'YYYY-MM-DD HH24MISS')),'DD-MM-YYYY HH24:MI:SS'), " +
                                  " to_char(max(TO_DATE(data||' '||substr(ora,0,6),'YYYY-MM-DD HH24MISS')),'DD-MM-YYYY HH24:MI:SS') from sapprd.zevenimentsofer where document =:nrBord ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrBord", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    minData = oReader.GetString(0);
                    maxData = oReader.GetString(1);
                }

                cmd.CommandText = " select latitude, longitude, to_char(record_time,'dd-Mon-yy hh24:mi:ss') from gps_date where device_id =:deviceId and " +
                                  " record_time between to_date('" + minData + "','DD-MM-YYYY HH24:MI:SS')  " +
                                  " and to_date('" + maxData + "','DD-MM-YYYY HH24:MI:SS') and speed > 0 order by record_time ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":deviceId", OracleType.Number).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Decimal.Parse(idMasina);

                oReader = cmd.ExecuteReader();
                coordGps = "";


                DateTime dateStart;
                DateTime dateStop;


                TimeSpan ts = new TimeSpan();

                int ii = 0;
                string recordTime = "0";
                listaCoordonate = new List<String>();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {


                        if (!recordTime.Equals("0"))
                        {
                            dateStart = Convert.ToDateTime(recordTime);
                            dateStop = Convert.ToDateTime(oReader.GetString(2));
                            ts = dateStop - dateStart;
                        }


                        if (ts.Minutes > 10 || recordTime.Equals("0"))
                        {
                           
                            coordGps = oReader.GetDouble(0).ToString().Replace(',', '.') + "," + oReader.GetDouble(1).ToString().Replace(',', '.');
                            listaCoordonate.Add(coordGps);

                            recordTime = oReader.GetString(2);
                        }
                        ii++;



                    }
                }

                oReader.Close();
                oReader.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + nrDocument);
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            coordGps = serializer.Serialize(listaCoordonate);

            return coordGps;
        }



        private string getDuration(TimeSpan ts)
        {
            string duration = "";

            if (ts.Days > 0)
                duration = ts.Days + "z ";

            if (ts.Hours > 0)
                duration += ts.Hours + "h ";

            duration += ts.Minutes + "m";

            return duration;
        }
    }

}