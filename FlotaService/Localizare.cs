using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using System.Web.Script.Serialization;

namespace FlotaService
{
    public class Localizare
    {

        public string getPozitieCurenta(string soferi)
        {
            string serializedResult = "";

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Sofer> listaSoferi = serializer.Deserialize<List<Sofer>>(soferi);

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string strListSoferi = "";

            foreach (Sofer s in listaSoferi)
            {
                if (strListSoferi.Equals(""))
                    strListSoferi += "'" + s.cod + "'";
                else
                    strListSoferi += "," + "'" + s.cod + "'";
            }

            strListSoferi = "(" + strListSoferi + ")";

            string dataCurenta = DateTime.Today.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                string query = "";

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                query = " select distinct codsofer,g.id,  d.latitude, d.longitude,  to_char(d.record_time, 'dd-mon-yyyy hh24:mi:ss')  from sapprd.zevenimentsofer a, " +
                               " borderouri b, gps_masini g, gps_date d where a.codsofer in " + strListSoferi + " and a.data = '" + dataCurenta + "' " +
                               " and a.document = b.numarb and replace(b.masina, '-') = g.nr_masina and d.device_id = g.id and " +
                               " to_char(d.record_time, 'dd-mon-yyyy hh24:mi:ss') =  (select to_char(max(h.record_time), 'dd-mon-yyyy hh24:mi:ss') " +
                               " from gps_date h where h.device_id = d.device_id) ";


               



                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                PozitieActualaMasina pozitie;
                List<PozitieActualaMasina> listaPozitii = new List<PozitieActualaMasina>();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        pozitie = new PozitieActualaMasina();
                        pozitie.codSofer = oReader.GetString(0);
                        pozitie.latitudine = oReader.GetDouble(2).ToString().Replace(",", ".");
                        pozitie.longitudine = oReader.GetDouble(3).ToString().Replace(",", ".");
                        pozitie.data = oReader.GetString(4);
                        listaPozitii.Add(pozitie);
                    }

                }

                oReader.Close();
                oReader.Dispose();

                serializedResult = serializer.Serialize(listaPozitii);

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




        public string getPozitieCurentaMasini(string masini)
        {

            string serializedResult = "";

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Masina> listaMasini = serializer.Deserialize<List<Masina>>(masini);

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string strListMasini = "";

            foreach (Masina s in listaMasini)
            {
                if (!s.deviceId.Equals("-1"))
                {
                    if (strListMasini.Equals(""))
                        strListMasini += "'" + s.deviceId + "'";
                    else
                        strListMasini += "," + "'" + s.deviceId + "'";
                }
            }

            strListMasini = "(" + strListMasini + ")";

            


            try
            {
                if (strListMasini.Length > 2)
                {
                    string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                    string query = "";

                    connection.ConnectionString = connectionString;
                    connection.Open();

                    OracleCommand cmd = connection.CreateCommand();


                    query = " select d.device_id, nvl(d.latitude,-1), nvl(d.longitude,-1), to_char(d.record_time, 'dd-mon-yyyy hh24:mi:ss') datac, "
                          + " m.nr_masina, nvl(d.speed,-1) from gps_index d, gps_masini m where d.device_id in " + strListMasini + " and  d.device_id = m.id ";


                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    oReader = cmd.ExecuteReader();

                    PozitieActualaMasina pozitie;
                    List<PozitieActualaMasina> listaPozitii = new List<PozitieActualaMasina>();

                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            pozitie = new PozitieActualaMasina();
                            pozitie.deviceId = oReader.GetInt32(0).ToString();
                            pozitie.latitudine = oReader.GetDouble(1).ToString().Replace(",", ".");
                            pozitie.longitudine = oReader.GetDouble(2).ToString().Replace(",", ".");
                            pozitie.data = oReader.GetString(3);
                            pozitie.nrAuto = oReader.GetString(4);
                            pozitie.viteza = oReader.GetInt32(5).ToString();
                            listaPozitii.Add(pozitie);
                        }

                    }

                    oReader.Close();
                    oReader.Dispose();

                    serializedResult = serializer.Serialize(listaPozitii);
                }

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