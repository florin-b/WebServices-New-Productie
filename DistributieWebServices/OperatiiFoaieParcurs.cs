using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace DistributieWebServices
{
    public class OperatiiFoaieParcurs
    {

        public string afisFoaieParcurs(string codSofer, string nrMasina, string an, string luna)
        {

            AfisFoaieParcurs afisFoaieParcurs = new AfisFoaieParcurs();

            FoaieParcurs foaieParcurs = new JavaScriptSerializer().Deserialize<FoaieParcurs>(getFoaieParcurs(codSofer, nrMasina, an, luna, "AFIS"));
            afisFoaieParcurs.foaieParcurs = foaieParcurs;

            OperatiiMasina opMasina = new OperatiiMasina();
            afisFoaieParcurs.alimentariComb = opMasina.getAlimentariLuna(codSofer, nrMasina, an + luna.PadLeft(2, '0'));
            afisFoaieParcurs.consumSiroco = opMasina.getSirocoLuna(codSofer, nrMasina, an + luna.PadLeft(2, '0'));

            string[] kmLuna = opMasina.getKmLuna(codSofer, nrMasina, an + luna.PadLeft(2, '0')).Split('#');
            afisFoaieParcurs.kmStart = kmLuna[0];
            afisFoaieParcurs.kmStop = kmLuna[1];

            return new JavaScriptSerializer().Serialize(afisFoaieParcurs);

        }

        public string getFoaieParcurs(string codSofer, string nrMasina, string an, string luna, string tipOp)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            FoaieParcurs foaieParcurs = new FoaieParcurs();
            List<FoaieParcursItem> items = new List<FoaieParcursItem>();

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                if (tipOp.ToUpper().Equals("MODIF"))
                {

                    bool isDeschisa = isLunaDeschisa(connection, codSofer, nrMasina, an, luna);

                    if (!isDeschisa)
                    {
                        cmd = null;
                        connection.Close();
                        return "-1";
                    }

                }

                cmd = connection.CreateCommand();

                cmd.CommandText = " select id, locatie, data, ora, minut, kmbord from sapprd.zfoaieparcurs where mandt = '900' and nrauto =:nrAuto and " +
                                  " sofer = :codSofer and substr(data,0,6)=:dataf order by to_date(data||' '||ora||':'||minut,'yyyymmdd hh24:mi') ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 120).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrMasina.Replace("-", "");

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":dataf", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[2].Value = an + luna.PadLeft(2, '0');

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        FoaieParcursItem item = new FoaieParcursItem();
                        item.id = oReader.GetInt32(0).ToString();
                        item.locatie = oReader.GetString(1);
                        item.data = oReader.GetString(2);
                        item.ora = oReader.GetInt32(3).ToString("00");
                        item.minut = oReader.GetInt32(4).ToString("00");
                        item.km = oReader.GetString(5);
                        items.Add(item);

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

            foaieParcurs.items = items;

            return new JavaScriptSerializer().Serialize(foaieParcurs);

        }



        private string verificaKmFoaieParcurs(List<FoaieParcursItem> foaieItems, string codSofer, string nrMasina)
        {

            string status = "";

            int kmInceputZi = 0;
            foreach(FoaieParcursItem item in foaieItems){

                kmInceputZi = new OperatiiMasina().getKmInceputZi(codSofer, nrMasina, item.data);

                if (Int32.Parse(item.km) < kmInceputZi)
                {
                    return "Nr. de km " + item.km + " este mai mic decat " + kmInceputZi + " declarati pentru data de " + Utils.formatDateFromSap(item.data) + ".";
                }


            }

            return status;
        }


        public string saveFoaieParcurs(string foaieData, string codSofer, string nrMasina)
        {

            string status = "1";
            List<FoaieParcursItem> foaieItems = new JavaScriptSerializer().Deserialize<List<FoaieParcursItem>>(foaieData);

            string statusKm = verificaKmFoaieParcurs(foaieItems,  codSofer,  nrMasina);

            if (!statusKm.Equals(String.Empty))
            {
                return "INFO#" + statusKm;
            }

            

            OracleConnection connection = new OracleConnection();

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.ConnectionString = connectionString;
                connection.Open();

                bool updateStatus = updateItems(connection, foaieItems);
                bool addStatus = addItems(connection, foaieItems, codSofer, nrMasina);
                bool delStatus = deleteItems(connection, foaieItems);

                if (updateStatus && addStatus && delStatus)
                    status = "1";
                else
                    status = "-1";


            }
            catch (Exception ex)
            {
                status = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
            }


            return status;
        }

        private bool updateItems(OracleConnection connection, List<FoaieParcursItem> foaieItems)
        {

            bool succes = true;

            try
            {
                OracleCommand cmd = connection.CreateCommand();

                foreach (FoaieParcursItem item in foaieItems)
                {

                    if (item.status.Equals("DEL") || item.status.Equals("ADD"))
                        continue;

                    cmd.CommandText = "update sapprd.zfoaieparcurs set locatie =:locatie, data=:data, ora=:ora, minut=:minut, kmbord=:kmbord where id=:idItem ";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":locatie", OracleType.NVarChar, 300).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = item.locatie;

                    cmd.Parameters.Add(":data", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = item.data;

                    cmd.Parameters.Add(":ora", OracleType.Number, 3).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = Int32.Parse(item.ora);

                    cmd.Parameters.Add(":minut", OracleType.Number, 3).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = Int32.Parse(item.minut);

                    cmd.Parameters.Add(":kmbord", OracleType.NVarChar, 45).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = item.km;

                    cmd.Parameters.Add(":idItem", OracleType.Number, 10).Direction = ParameterDirection.Input;
                    cmd.Parameters[5].Value = Int32.Parse(item.id);

                    cmd.ExecuteNonQuery();
                }

                cmd.Dispose();
            }
            catch (Exception ex)
            {
                succes = false;
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

            return succes;
        }

        private bool addItems(OracleConnection connection, List<FoaieParcursItem> foaieItems, string codSofer, string nrAuto)
        {
            bool succes = true;

            try
            {
                OracleCommand cmd = connection.CreateCommand();

                foreach (FoaieParcursItem item in foaieItems)
                {

                    if (item.status.Equals("DEL") || item.status.Equals("INIT"))
                        continue;

                    cmd.CommandText = " insert into sapprd.zfoaieparcurs (mandt, id, nrauto, sofer, locatie, data, ora, minut, kmbord) " +
                                      " values ('900', pk_mas.nextval, :nrauto, :sofer, :locatie, :data, :ora, :minut, :kmbord) ";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":nrauto", OracleType.NVarChar, 120).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = nrAuto.Replace("-", "");

                    cmd.Parameters.Add(":sofer", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codSofer;

                    cmd.Parameters.Add(":locatie", OracleType.NVarChar, 300).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = item.locatie;

                    cmd.Parameters.Add(":data", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = item.data;

                    cmd.Parameters.Add(":ora", OracleType.Number, 3).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = Int32.Parse(item.ora);

                    cmd.Parameters.Add(":minut", OracleType.Number, 3).Direction = ParameterDirection.Input;
                    cmd.Parameters[5].Value = Int32.Parse(item.minut);

                    cmd.Parameters.Add(":kmbord", OracleType.NVarChar, 45).Direction = ParameterDirection.Input;
                    cmd.Parameters[6].Value = item.km;

                    cmd.ExecuteNonQuery();
                }

                cmd.Dispose();
            }
            catch (Exception ex)
            {
                succes = false;
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

            return succes;
        }


        private bool deleteItems(OracleConnection connection, List<FoaieParcursItem> foaieItems)
        {
            bool succes = true;
            try
            {
                OracleCommand cmd = connection.CreateCommand();

                foreach (FoaieParcursItem item in foaieItems)
                {

                    if (item.status.Equals("INIT") || item.status.Equals("ADD"))
                        continue;

                    cmd.CommandText = "delete from sapprd.zfoaieparcurs where id=:idItem ";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":idItem", OracleType.Number, 10).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = Int32.Parse(item.id);

                    cmd.ExecuteNonQuery();
                }

                cmd.Dispose();
            }
            catch (Exception ex)
            {
                succes = false;
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

            return succes;
        }


        private bool isLunaDeschisa(OracleConnection connection, string codSofer, string nrMasina, string an, string luna)
        {
            bool isLunaDeschisa = false;

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select 1 from sapprd.ZFPINCHISA where mandt = '900' and nrauto =:nrAuto and sofer =:codSofer and luna=:luna and anul=:an ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 120).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrMasina.Replace("-", "");

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":luna", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[2].Value = luna;

                cmd.Parameters.Add(":an", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[3].Value = an;

                oReader = cmd.ExecuteReader();

                isLunaDeschisa = !oReader.HasRows;
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }


            return isLunaDeschisa;
        }

        public string inchideLunaFP(string codSofer, string nrAuto, string an, string luna)
        {
            string succes = "1";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " insert into sapprd.ZFPINCHISA (mandt,  nrauto, sofer, luna, anul) " +
                                  " values ('900',  :nrauto, :sofer, :luna, :anul) ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrauto", OracleType.NVarChar, 120).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrAuto.Replace("-", "");

                cmd.Parameters.Add(":sofer", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":luna", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = luna;

                cmd.Parameters.Add(":anul", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = an;

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                succes = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }



            return succes;
        }




    }
}