using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
using System.Data;

namespace WebService1
{
    public class JurnalAprobari
    {

        private const int MARJA_MIN = 7;
        private const int MARJA_MAX = 40;
        private const int PUNCTAJ_MIN = 3;
        private const int PUNCTAJ_MAX = 12;

        private const double proc1 = 0.6;
        private const double proc2 = 0.4;


        public void salveazaAprobare(string idComanda, string codAngajat, string tipOperatie)
        {

            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            string nowDate = year + month + day;
            string nowTime = cDate.Hour.ToString("00") + cDate.Minute.ToString("00") + cDate.Second.ToString("00");
            saveDateOperatieComanda(idComanda, codAngajat, tipOperatie, 0,0,0, nowDate, nowTime);
        }

        public void salveazaAprobare_old(string idComanda, string codAngajat, string tipOperatie)
        {

            double marja = 0;
            double punctajClient = 0;

         

            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            string nowDate = year + month + day;
            string nowTime = cDate.Hour.ToString("00") + cDate.Minute.ToString("00") + cDate.Second.ToString("00");

            OracleConnection connection = new OracleConnection();
            string connectionString = DatabaseConnections.ConnectToProdEnvironment();
            OracleCommand cmd = connection.CreateCommand();
            OracleDataReader oReader = null;

            try
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                BazaSalariala bazaSalariala = Salarizare.getBazaSalariala(connection, idComanda.Trim(), "APROB");
                marja = bazaSalariala.procentT1 * 100;


                cmd.CommandText = " select b.punctaj from sapprd.zcomhead_tableta a, sapprd.zsegmpunctaj b where a.mandt = '900' and " +
                                  " a.id = :idComanda and b.cod_client = a.cod_client  and b.filiala = a.ul and b.depart = a.depart ";

                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    punctajClient = oReader.GetDouble(0);
                }

                double marjaScal = rescalVal(marja, MARJA_MIN, MARJA_MAX);
                double punctajScal = rescalVal(punctajClient, PUNCTAJ_MIN, PUNCTAJ_MAX);


                saveDateOperatieComanda(idComanda, codAngajat, tipOperatie, bazaSalariala.procentT1 * 100, bazaSalariala.marjaT1, punctajClient, nowDate, nowTime);


                


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
                connection.Close();
            }


        }



        private void saveDateOperatieComanda(string idComanda, string codAngajat, string tipOperatie, double procentT1, double marjaT1, double punctajClient, string nowDate, string nowTime)
        {
            OracleConnection connection = new OracleConnection();
            string connectionString = DatabaseConnections.ConnectToProdEnvironment();
            OracleCommand cmd = connection.CreateCommand();

            try
            {

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd.CommandText = " insert into sapprd.zaprob_comenzi (mandt, id_comanda, cod_angajat, operatie, proc_marja_t1, val_marja_t1, punctaj_client, data_op, ora_op) " +
                                    " values ('900', :id_comanda, :cod_angajat, :operatie, :proc_marja_t1, :val_marja_t1, :punctaj_client, :data_op, :ora_op) ";


                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":id_comanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                cmd.Parameters.Add(":cod_angajat", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAngajat;

                cmd.Parameters.Add(":operatie", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = tipOperatie;

                cmd.Parameters.Add(":proc_marja_t1", OracleType.Double, 13).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = procentT1;

                cmd.Parameters.Add(":val_marja_t1", OracleType.Double, 13).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = marjaT1;

                cmd.Parameters.Add(":punctaj_client", OracleType.Double, 13).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = punctajClient;

                cmd.Parameters.Add(":data_op", OracleType.Double, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = nowDate;

                cmd.Parameters.Add(":ora_op", OracleType.Double, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[7].Value = nowTime;

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }

        }


        private double rescalVal(double valInit, int valMin, int valMax)
        {
            double newVal = 0;

            int newMin = 4;
            int newMax = 10;

            if (valInit >= valMax)
                valInit = valMax;

            if (valInit <= valMin)
                valInit = valMin;

            newVal = (valInit - valMin) * ((float)(newMax - newMin) / (float)(valMax - valMin)) + newMin;

            return newVal;



        }


    }
}