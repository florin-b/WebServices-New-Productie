using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace WebService1
{
    public class Aprobari
    {

        private enum EnumTipAprob
        {
            SD, DV
        }

        public string getAprobariNecesare(string nrComanda)
        {

            AprobariNecesare aprobariNecesare = new AprobariNecesare();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {


                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select a.depart,a.ul, a.accept1, a.ora_accept1, a.accept2, a.ora_accept2 " +
                                  " from sapprd.zcomhead_tableta a where a.id =:idcmd and a.status_aprov = 1 ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrComanda);

                oReader = cmd.ExecuteReader();

                string depart = "", ul = "";
                if (oReader.HasRows)
                {
                    oReader.Read();

                    depart = oReader.GetString(0);
                    ul = ComenziSiteHelper.getUlDistrib(oReader.GetString(1));

                    if (oReader.GetString(2).Equals("X") && oReader.GetString(3).Equals("000000"))
                        aprobariNecesare.aprobSD = getDateAprob(connection, depart, ul, EnumTipAprob.SD);

                    if (oReader.GetString(4).Equals("X") && oReader.GetString(5).Equals("000000"))
                        aprobariNecesare.aprobDV = getDateAprob(connection, depart, ul, EnumTipAprob.DV);

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

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(aprobariNecesare);

        }


        private string getDateAprob(OracleConnection connection, string depart, string ul, EnumTipAprob tipAprob)
        {

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string sqlString = "";

            if (tipAprob == EnumTipAprob.SD)
            {
                sqlString = " select nume, nrtel from agenti where tip = 'SD' and activ = 1 and filiala = :filiala and divizie =:depart and length(trim(nrtel)) > 0 ";

                cmd = connection.CreateCommand();

                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = ul;

                cmd.Parameters.Add(":depart", OracleType.NVarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = depart;


            }

            if (tipAprob == EnumTipAprob.DV)
            {
                sqlString = " select ag.nume , ag.nrtel from sapprd.zfil_dv d, agenti ag where " +
                            " d.prctr =:filiala and d.spart =:depart " +
                            " and ag.cod = d.pernr and ag.activ = 1 and length(trim(ag.nrtel)) > 0 ";

                cmd = connection.CreateCommand();

                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = ul;

                cmd.Parameters.Add(":depart", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = depart;

            }

            oReader = cmd.ExecuteReader();

            List<PersAprob> listAprobari = new List<PersAprob>();

            if (oReader.HasRows)
            {
                while (oReader.Read())
                {
                    PersAprob persAprob = new PersAprob();
                    persAprob.nume = oReader.GetString(0);
                    persAprob.telefon = oReader.GetString(1);
                    listAprobari.Add(persAprob);
                }
            }

            DatabaseConnections.CloseConnections(oReader, cmd);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(listAprobari);

        }


    }
}