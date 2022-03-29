using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace DistributieWebServices
{
    public class OperatiiMasina
    {

        private static readonly int MEDIA_KM_ZI = 750;

        public void adaugaKmMasina(string codAngajat, string nrAuto, string km)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " insert into sapprd.zkmmasini(mandt, codangajat, datac, nrauto, km) values ('900',:codAngajat, :datac, :nrAuto, :km) ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAngajat", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = codAngajat;

                cmd.Parameters.Add(":datac", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = getCurrentDate();

                cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 30).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[2].Value = nrAuto;

                cmd.Parameters.Add(":km", OracleType.Number, 13).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[3].Value = Double.Parse(km, CultureInfo.InvariantCulture);

                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }


        }



        public bool getKmMasinaDeclarati(string nrAuto)
        {

            bool hasKm = false;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select 1 from sapprd.zkmmasini where datac=:datac and nrauto=:nrAuto ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":datac", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = getCurrentDate();

                cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 30).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = nrAuto;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                    hasKm = true;


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }

            return hasKm;

        }


        public string verificaKmSalvati(string nrAuto, string kmNoi)
        {

            int kmSalvati = 0;
            string dataKmSalvati = "";
            string msgStatus = "";
            int msgId = 0;
            string numeSofer = "";

            bool isKmSalvatiValid = true;

            StareValidareKm stareValidare = new StareValidareKm();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select to_char(to_date(k.datac,'yyyymmdd')), k.km, s.nume from sapprd.zkmmasini k, soferi s where k.nrauto =:nrAuto " +
                                  " and k.datac = (select max(datac) from sapprd.zkmmasini where nrauto = k.nrAuto) and s.cod = k.codangajat ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 30).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrAuto;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    kmSalvati = oReader.GetInt32(1);
                    dataKmSalvati = oReader.GetString(0);
                    numeSofer = oReader.GetString(2);

                    if (Int32.Parse(kmNoi) < kmSalvati)
                    {
                        msgStatus = "Acest index este mai mic decat cel salvat anterior. In data de " + formatDate(dataKmSalvati) + ", " + numeSofer + " a introdus indexul " + kmSalvati + ".";
                        isKmSalvatiValid = false;
                        msgId = 1;
                    }
                    else if (Int32.Parse(kmNoi) > kmSalvati)
                    {
                        string dataStart = dataKmSalvati;
                        string dataStop = getCurrentDateFormatted();

                        DateTime dateStart = Convert.ToDateTime(dataStart);
                        DateTime dateStop = Convert.ToDateTime(dataStop);

                        TimeSpan ts = dateStop - dateStart;

                        int mediaKm = 1;

                        if (ts.Days > 0)
                            mediaKm = (Int32.Parse(kmNoi) - kmSalvati) / (ts.Days);

                        if (mediaKm > MEDIA_KM_ZI)
                        {
                            msgStatus = "Conform acestui index au fost parcursi mai mult de " + MEDIA_KM_ZI + " km/zi.";
                            isKmSalvatiValid = false;
                            msgId = 2;
                        }

                    }


                    if (isKmSalvatiValid)
                    {

                        cmd.CommandText = " select nvl(min(mileage),0) km, 'astazi' timp from gps_date where " +
                                          " device_id = (select id from gps_masini where nr_masina =:nrAuto) and trunc(record_time) = trunc(sysdate) " +
                                          " union " +
                                          " select nvl(min(mileage),0) km, 'trecut' timp from gps_date where " +
                                          " device_id = (select id from gps_masini where nr_masina =:nrAuto)  and trunc(record_time) =:dataSalvare  ";

                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 10).Direction = System.Data.ParameterDirection.Input;
                        cmd.Parameters[0].Value = nrAuto.Replace("-", "").Replace(" ", "");

                        cmd.Parameters.Add(":dataSalvare", OracleType.VarChar, 10).Direction = System.Data.ParameterDirection.Input;
                        cmd.Parameters[1].Value = dataKmSalvati;

                        oReader = cmd.ExecuteReader();

                        int kmGpsAstazi = 0;
                        int kmGpsTrecut = 0;

                        if (oReader.HasRows)
                        {
                            while (oReader.Read())
                            {
                                if (oReader.GetString(1).Equals("astazi"))
                                    kmGpsAstazi = oReader.GetInt32(0);

                                if (oReader.GetString(1).Equals("trecut"))
                                    kmGpsTrecut = oReader.GetInt32(0);
                            }

                        }

                        double distEfectuataGps = kmGpsAstazi - kmGpsTrecut;
                        double distEfectuataDeclarat = Int32.Parse(kmNoi) - kmSalvati;

                        double difKmDistanta = Math.Abs(distEfectuataDeclarat - distEfectuataGps);

                        bool isDifProcent = Math.Abs(distEfectuataGps - distEfectuataDeclarat) > distEfectuataGps / 10;

                        if (difKmDistanta > 10 || isDifProcent)
                        {
                            msgStatus = "De la ultima salvare, facuta in data de " + formatDate(dataKmSalvati) + ", conform GPS, masina a parcurs " + distEfectuataGps + " km, dar conform km bord introdusi sunt " + distEfectuataDeclarat + " km. Confirmati km din bord?";
                            isKmSalvatiValid = false;
                            msgId = 3;
                        }



                    }


                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , nrauto " + nrAuto + " , kmNoi " + kmNoi);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            stareValidare.isKmValid = isKmSalvatiValid;
            stareValidare.statusMsg = msgStatus;
            stareValidare.statusId = msgId;

            return new JavaScriptSerializer().Serialize(stareValidare);
        }






        public bool valideazaKmMasina(string nrAuto, string kmNoi)
        {


            bool isValid = true;
            int kmSalvati = 0;
            string dataKmSalvati = getCurrentDateFormatted();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();



                cmd.CommandText = " select to_char(to_date(datac,'yyyymmdd')), km from sapprd.zkmmasini where nrauto =:nrAuto " +
                                  " and datac = (select max(datac) from sapprd.zkmmasini where nrauto =:nrAuto) ";


                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 30).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrAuto;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    kmSalvati = oReader.GetInt32(1);
                    dataKmSalvati = oReader.GetString(0);


                    string dataStart = dataKmSalvati;
                    string dataStop = getCurrentDateFormatted();

                    DateTime dateStart = Convert.ToDateTime(dataStart);
                    DateTime dateStop = Convert.ToDateTime(dataStop);

                    TimeSpan ts = dateStop - dateStart;

                    int mediaKm = 1;

                    if (ts.Days > 0)
                        mediaKm = (Int32.Parse(kmNoi) - kmSalvati) / ts.Days;

                    if (mediaKm > MEDIA_KM_ZI)
                        isValid = false;

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


            return isValid;

        }

        public string adaugaSiroco(string sirocoData)
        {
            Alimentare siroco = new JavaScriptSerializer().Deserialize<Alimentare>(sirocoData);

            if (siroco.id.Equals("null"))
                return insereazaSiroco(siroco);
            else
                return actualizeazaSiroco(siroco);
        }

        public string insereazaSiroco(Alimentare siroco)
        {
            string retVal = "1";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " insert into sapprd.zsiroco_tab(mandt, id, nrauto, sofer, data, litri) values ('900', pk_mas.nextval, :nrauto, :sofer, :data, :litri) ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrauto", OracleType.NVarChar, 120).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = siroco.nrAuto.Replace("-", "");

                cmd.Parameters.Add(":sofer", OracleType.NVarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = siroco.codSofer;

                cmd.Parameters.Add(":data", OracleType.NVarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[2].Value = siroco.data;

                cmd.Parameters.Add(":litri", OracleType.Number, 3).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[3].Value = Int32.Parse(siroco.litri);

                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }


            return retVal;
        }

        private string actualizeazaSiroco(Alimentare siroco)
        {
            string retVal = "1";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " update sapprd.zsiroco_tab set litri=:litri where mandt='900' and id=:id  ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":litri", OracleType.Number, 3).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(siroco.litri);

                cmd.Parameters.Add(":id", OracleType.Number, 3).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = Int32.Parse(siroco.id);

                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }

            return retVal;
        }

        public string getSiroco(string codSofer, string nrMasina, string data)
        {
            Alimentare alimentare = new Alimentare();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select id, litri from sapprd.zsiroco_tab where mandt = '900' and nrauto =:nrAuto and " +
                                  " sofer =:codSofer and data=:data ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 120).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrMasina.Replace("-", "");

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":data", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[2].Value = data;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    alimentare.id = oReader.GetInt32(0).ToString();
                    alimentare.litri = oReader.GetInt32(1).ToString();
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

            return new JavaScriptSerializer().Serialize(alimentare);
        }

        public string addSiroco(string codSofer, string nrMasina, string data, string litri)
        {
            string retVal = "1";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " insert into sapprd.zsiroco_tab(mandt, id, nrauto, sofer, data, litri) values ('900', pk_mas.nextval, :nrauto, :sofer, :data, :litri) ";


                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrauto", OracleType.NVarChar, 120).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrMasina.Replace("-", "");

                cmd.Parameters.Add(":sofer", OracleType.NVarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":data", OracleType.NVarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[2].Value = data;

                cmd.Parameters.Add(":litri", OracleType.Number, 3).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[3].Value = Int32.Parse(litri);

                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }


            return retVal;
        }


        public string getAlimentare(string codSofer, string nrMasina, string data)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            Alimentare alimentare = new Alimentare();

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select id, kmbord, litri from sapprd.zalim_tab where mandt = '900' and nrauto =:nrAuto and " +
                                  " sofer =:codSofer and data=:data ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 120).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrMasina.Replace("-", "");

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":data", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[2].Value = data;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    alimentare.id = oReader.GetInt32(0).ToString();
                    alimentare.kmBord = oReader.GetString(1);
                    alimentare.litri = oReader.GetInt32(2).ToString();
                }
                else
                    alimentare.kmBord = getKmMasina(nrMasina, data).ToString();
            }

            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return new JavaScriptSerializer().Serialize(alimentare);
        }


        public int getKmMasina(string nrMasina, string data)
        {
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            int nrKm = 0;

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select a.mileage from gps_index a, gps_masini b where b.nr_masina = :nrAuto and b.id = a.device_id " +
                                  " and trunc(a.record_time) = to_date('" + data + "','YYYYMMDD') ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 120).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrMasina.Replace("-", "");

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    nrKm = oReader.GetInt32(0);
                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + nrMasina + " , " + data);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }


            return nrKm;

        }

        private string verificaKmAlimentare(Alimentare alimentare)
        {
            string status = "";
            int kmInceputZi = getKmInceputZi(alimentare.codSofer, alimentare.nrAuto, alimentare.data);

            if (Int32.Parse(alimentare.kmBord) < kmInceputZi)
            {
                return "Nr. de km " + alimentare.kmBord + " este mai mic decat " + kmInceputZi + " declarati la inceput de zi.";
            }

            int kmFoaie = getMaxKmFoaie(alimentare.codSofer, alimentare.nrAuto, alimentare.data);

            if (Int32.Parse(alimentare.kmBord) > kmFoaie && kmFoaie != 0)
            {
                return "Nr. de km " + alimentare.kmBord + " este mai mare decat cel din foaia de parcurs.";
            }


            return status;
        }


        public string adaugaAlimentare(string alimentareData)
        {

            Alimentare alimentare = new JavaScriptSerializer().Deserialize<Alimentare>(alimentareData);

            string statusKm = verificaKmAlimentare(alimentare);
            if (!statusKm.Equals(String.Empty))
            {
                return "INFO#" + statusKm;
            }


            if (alimentare.id.Equals("null"))
                return insereazaAlimentare(alimentare);
            else
                return actualizeazaAlimentare(alimentare);

        }

        private string insereazaAlimentare(Alimentare alimentare)
        {
            string retVal = "1";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " insert into sapprd.ZALIM_TAB(mandt, id, nrauto, sofer, data, kmbord, litri) values ('900', pk_mas.nextval, :nrauto, :sofer, :data, :kmbord, :litri) ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrauto", OracleType.NVarChar, 120).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = alimentare.nrAuto.Replace("-", "");

                cmd.Parameters.Add(":sofer", OracleType.NVarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = alimentare.codSofer;

                cmd.Parameters.Add(":data", OracleType.NVarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[2].Value = alimentare.data;

                cmd.Parameters.Add(":kmbord", OracleType.NVarChar, 45).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[3].Value = alimentare.kmBord;

                cmd.Parameters.Add(":litri", OracleType.Number, 3).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[4].Value = Int32.Parse(alimentare.litri);

                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }

            return retVal;

        }


        private string actualizeazaAlimentare(Alimentare alimentare)
        {
            string retVal = "1";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " update sapprd.ZALIM_TAB set kmbord=:kmbord, litri=:litri where mandt='900' and id=:id  ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":kmbord", OracleType.NVarChar, 45).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = alimentare.kmBord;

                cmd.Parameters.Add(":litri", OracleType.Number, 3).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = Int32.Parse(alimentare.litri);

                cmd.Parameters.Add(":id", OracleType.Number, 3).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[2].Value = Int32.Parse(alimentare.id);



                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }

            return retVal;
        }

        public int getKmInceputZi(string codSofer, string nrMasina, string data)
        {
            int km = 0;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select km from sapprd.zkmmasini where mandt = '900' and replace(nrauto,'-','') =:nrAuto and " +
                                  " codangajat =:codSofer and datac=:data ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 30).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrMasina.Replace("-", "");

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":data", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[2].Value = data;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    km = oReader.GetInt32(0);
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

            return km;
        }


        public string getKmLuna(string codSofer, string nrMasina, string data)
        {
            string nrMinKm = "0";
            string nrMaxKm = "0";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select kmbord, datac from ( " +
                                  " select kmbord, to_date(data||' '||ora||':'||minut,'yyyymmdd hh24:mi') datac From  sapprd.zfoaieparcurs where sofer = :codSofer and nrauto = :nrAuto " +
                                  " and to_char(to_date(data || ' ' || ora || ':' || minut, 'yyyymmdd hh24:mi'), 'yyyymmdd hh24:mi') = " +
                                  " (select min(to_char(to_date(data || ' ' || ora || ':' || minut, 'yyyymmdd hh24:mi'), 'yyyymmdd hh24:mi')) from sapprd.zfoaieparcurs " +
                                  " where sofer = :codSofer and nrauto = :nrAuto and substr(data,0, 6) = :data) " +
                                  " union all " +
                                  " select kmbord, to_date(data||' '||ora||':'||minut,'yyyymmdd hh24:mi') datac From  sapprd.zfoaieparcurs where sofer = :codSofer and nrauto = :nrAuto " +
                                  " and to_char(to_date(data || ' ' || ora || ':' || minut, 'yyyymmdd hh24:mi'), 'yyyymmdd hh24:mi') = " +
                                  " (select max(to_char(to_date(data || ' ' || ora || ':' || minut, 'yyyymmdd hh24:mi'), 'yyyymmdd hh24:mi')) from sapprd.zfoaieparcurs " +
                                  " where sofer = :codSofer and nrauto = :nrAuto and substr(data,0, 6) = :data) ) order by datac ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 120).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrMasina.Replace("-", "");

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":data", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[2].Value = data;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    nrMinKm = oReader.GetString(0);

                    oReader.Read();
                    nrMaxKm = oReader.GetString(0);
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }


            return nrMinKm + "#" + nrMaxKm;
        }



        public int getMaxKmFoaie(string codSofer, string nrMasina, string data)
        {
            int maxKm = 0;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select max(to_number(kmbord)) from sapprd.zfoaieparcurs where mandt = '900' and replace(nrauto,'-','') =:nrAuto and " +
                                  " sofer =:codSofer and data=:data ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 30).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrMasina.Replace("-", "");

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":data", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[2].Value = data;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    maxKm = oReader.GetInt32(0);
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

            return maxKm;
        }


        public List<Alimentare> getSirocoLuna(string codSofer, string nrMasina, string luna)
        {
            List<Alimentare> listAlimentari = new List<Alimentare>();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select id, litri, data from sapprd.zsiroco_tab where mandt = '900' and nrauto =:nrAuto and " +
                                  " sofer =:codSofer and substr(data,0,6)=:data order by data ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 120).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrMasina.Replace("-", "");

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":data", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[2].Value = luna;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        Alimentare alimentare = new Alimentare();
                        alimentare.id = oReader.GetInt32(0).ToString();
                        alimentare.litri = oReader.GetInt32(1).ToString();
                        alimentare.data = oReader.GetString(2);
                        listAlimentari.Add(alimentare);
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

            return listAlimentari;
        }


        public List<Alimentare> getAlimentariLuna(string codSofer, string nrMasina, string luna)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<Alimentare> listAlimentari = new List<Alimentare>();

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select id, kmbord, litri, data from sapprd.zalim_tab where mandt = '900' and nrauto =:nrAuto and " +
                                  " sofer =:codSofer and substr(data,0,6) = :data order by data ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 120).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrMasina.Replace("-", "");

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":data", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[2].Value = luna;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        Alimentare alimentare = new Alimentare();
                        alimentare.id = oReader.GetInt32(0).ToString();
                        alimentare.kmBord = oReader.GetString(1);
                        alimentare.litri = oReader.GetInt32(2).ToString();
                        alimentare.data = oReader.GetString(3);
                        listAlimentari.Add(alimentare);
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

            return listAlimentari;
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


        private string getCurrentDateFormatted()
        {
            string mDate = "";
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString("0000");
            string day = cDate.Day.ToString("00");
            string month = cDate.ToString("MMM");
            mDate = day + "-" + month + "-" + year;
            return mDate;
        }

        private string formatDate(string strDate)
        {
            string[] tokDate = strDate.Split('-');
            return tokDate[0] + "-" + tokDate[1] + "-20" + tokDate[2];


        }

    }
}