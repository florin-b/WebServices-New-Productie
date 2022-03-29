using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace DistributieWebServices
{
    public class OperatiiSoferi
    {

        public string getSoferi()
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            List<Sofer> listSoferi = new List<Sofer>();
            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select a.fili, upper(a.nume), b.codTableta  from soferi a, sapprd.ztabletesoferi b where a.cod = b.codsofer " +
                                  " and b.stare = 1 order by a.fili,a.nume ";

                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        Sofer sofer = new Sofer();
                        sofer.filiala = oReader.GetString(0);
                        sofer.nume = oReader.GetString(1);
                        sofer.codTableta = oReader.GetString(2);
                        listSoferi.Add(sofer);

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

            return new JavaScriptSerializer().Serialize(listSoferi);

        }

        public static bool isSoferDTI(OracleConnection conn, String codSofer)
        {

            bool isDTI = false;

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            cmd = conn.CreateCommand();

            cmd.CommandText = " select fili from soferi where cod =:codSofer ";

            cmd.Parameters.Clear();
            cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            cmd.Parameters[0].Value = codSofer;

            oReader = cmd.ExecuteReader();

            oReader.Read();

            if (oReader.HasRows)
                if (oReader.GetString(0).Equals("GL90"))
                    isDTI = true;

            DatabaseConnections.CloseConnections(oReader, cmd);

            return isDTI;
        }



        public string getMasinaSoferBorderou(string codSofer)
        {
            string nrMasina = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select x.masina from (select to_char(b.data_e),  nvl((select nvl(ev.eveniment,'0') eveniment " +
                                  " from sapprd.zevenimentsofer ev where ev.document = b.numarb and ev.data = (select max(data) from sapprd.zevenimentsofer where document = ev.document and client = ev.document) " +
                                  " and ev.ora = (select max(ora) from sapprd.zevenimentsofer where document = ev.document and client = ev.document and data = ev.data)),0) eveniment, b.shtyp, " +
                                  " nvl((select distinct nr_bord from sapprd.zdocumentebord where nr_bord_urm =b.numarb and rownum=1),'-1') bordParent, b.masina " +
                                  " from  borderouri b, sapprd.zdocumentebord c  where  c.nr_bord = b.numarb and b.cod_sofer=:codSofer  order by trunc(c.data_e), c.ora_e) x where x.eveniment != 'S' and rownum<2 ";



                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    nrMasina = oReader.GetString(0);

                }

                oReader.Close();
                oReader.Dispose();

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


            return nrMasina;
        }


        public string getMasinaSofer(string codSofer)
        {
            string nrMasina = "";

            nrMasina = getMasinaSoferBorderou(codSofer);

            if (nrMasina.Length == 0)
                nrMasina = getMasinaSoferAlocata(codSofer);

            if (nrMasina.Length == 0)
                nrMasina = " ";

            return nrMasina;
        }


        public string getMasinaSoferAlocata(string codSofer)
        {
            string nrMasina = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
           
            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select ktext from( select distinct c.ktext,a.adatu " +
                                  " from sapprd.anlz a join sapprd.anla b on b.anln1 = a.anln1 and b.anln2 = a.anln2 and b.mandt = a.mandt " +
                                  " join sapprd.aufk c on c.aufnr = a.caufn and c.mandt = a.mandt where a.pernr = :codSofer " +
                                  " and a.bdatu >= (select to_char(sysdate - 5, 'YYYYMMDD') from dual) and b.deakt = '00000000' and a.mandt = '900' and c.auart = '1001' " +
                                  " order by a.adatu desc ) where rownum = 1 ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    nrMasina = oReader.GetString(0);
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

            return nrMasina;
        }



        public string getMasiniFiliala(string codFiliala)
        {
            string listMasini = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select ktext from sapprd.aufk where mandt='900' and auart='1001' and PHAS1='X' and werks =:codFiliala order by ktext ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codFiliala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codFiliala;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        if (listMasini.Length == 0)
                            listMasini = oReader.GetString(0);
                        else
                            listMasini += "," + oReader.GetString(0);
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

            return listMasini;
        }


        public string getKmMasina(string nrMasina)
        {
            string kmMasina = "0";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select x.mileage from( select mileage from gps_date where " +
                                  " device_id = (select id from gps_masini where nr_masina =:nrMasina) " +
                                  " and trunc(record_time) = trunc(sysdate) order by mileage) x where rownum = 1 ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrMasina", OracleType.VarChar, 10).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrMasina.Replace("-","").Replace(" ","");

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    kmMasina = oReader.GetInt32(0).ToString();
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

            return kmMasina;



        }



        public string getKmMasinaDeclarati(string nrMasina)
        {
            string kmMasina = "0";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select km from sapprd.zkmmasini where nrauto = :nrMasina " + 
                                  " and datac = (select max(datac) from sapprd.zkmmasini where nrauto =:nrMasina) ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrMasina", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrMasina.Replace("-", "").Replace(" ", "");

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    kmMasina = oReader.GetInt32(0).ToString();
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

            return kmMasina;



        }


        public String getStareMasina(string nrMasina)
        {
            StareMasina stareMasina = new StareMasina();

            stareMasina.kmGps = getKmMasina(nrMasina);
            stareMasina.kmDeclarati = getKmMasinaDeclarati(nrMasina);

            return new JavaScriptSerializer().Serialize(stareMasina);


        }

        public static void saveDeviceInfo(OracleConnection connection, string userCode, string strDeviceInfo)
        {

            DeviceInfo deviceInfo = new JavaScriptSerializer().Deserialize<DeviceInfo>(strDeviceInfo);

           
            OracleCommand cmd = new OracleCommand();

            try
            {

                cmd = connection.CreateCommand();

                string query = " insert into sapprd.zlogtbl (mandt, coduser, datalog, oralog, sdkver, man, model, appname, appver) values " +
                               " ('900', :coduser, :datalog, :oralog, :sdkver, :man, :model, :appname, :appver) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":coduser", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = userCode;

                cmd.Parameters.Add(":datalog", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = Utils.getCurrentDate();

                cmd.Parameters.Add(":oralog", OracleType.NVarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = Utils.getCurrentTime();

                cmd.Parameters.Add(":sdkver", OracleType.NVarChar, 15).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = deviceInfo.sdkVer;

                cmd.Parameters.Add(":man", OracleType.NVarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = deviceInfo.man.ToUpper();

                cmd.Parameters.Add(":model", OracleType.NVarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = deviceInfo.model.ToUpper();

                cmd.Parameters.Add(":appname", OracleType.NVarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = deviceInfo.appName;

                cmd.Parameters.Add(":appver", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[7].Value = deviceInfo.appVer;

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                cmd.Dispose();
            }

        }


    }
}