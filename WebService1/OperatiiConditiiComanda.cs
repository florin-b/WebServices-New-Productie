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
    public class OperatiiConditiiComanda
    {

        public string salveazaConditiiComanda(string conditiiComanda)
        {
            string retVal = "-1";

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            ConditiiPrimite conditii = serializer.Deserialize<ConditiiPrimite>(conditiiComanda);
            ConditiiHeader conditiiHeader = serializer.Deserialize<ConditiiHeader>(conditii.header.ToString());
            List<ConditiiArticole> conditiiArticole = serializer.Deserialize<List<ConditiiArticole>>(conditii.articole.ToString());

            OracleConnection connection = new OracleConnection();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                DateTime cDate = DateTime.Now;
                string year = cDate.Year.ToString();
                string day = cDate.Day.ToString("00");
                string month = cDate.Month.ToString("00");
                string nowDate = year + month + day;
                string hour = cDate.Hour.ToString();
                string minute = cDate.Minute.ToString();
                string sec = cDate.Second.ToString();
                string nowTime = hour + minute + sec;

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                string query = " insert into sapprd.zcondheadtableta(mandt,id,codpers,datac,orac,cmdref, cmdmodif,condcalit,nrfact,observatii) " +
                        " values ('900',pk_key.nextval, :codAg,:datac,:orac,:cmdref, " +
                        " :cmdmodif,:condcalit,:nrfact,:observatii) returning id into :id ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAg", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = conditiiHeader.codAgent;

                cmd.Parameters.Add(":datac", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = nowDate;

                cmd.Parameters.Add(":orac", OracleType.VarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = nowTime;

                cmd.Parameters.Add(":cmdref", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = conditiiHeader.id;

                cmd.Parameters.Add(":cmdmodif", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = "0";

                cmd.Parameters.Add(":condcalit", OracleType.Float, 26).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = conditiiHeader.conditiiCalit;

                cmd.Parameters.Add(":nrfact", OracleType.Float, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = conditiiHeader.nrFact;

                cmd.Parameters.Add(":observatii", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                cmd.Parameters[7].Value = conditiiHeader.observatii == "" ? " " : conditiiHeader.observatii;

                OracleParameter idCmd = new OracleParameter("id", OracleType.Number);
                idCmd.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(idCmd);

                cmd.ExecuteNonQuery();

                int pozArt = 0;
                string codArt = "";

                for (int i = 0; i < conditiiArticole.Count; i++)
                {
                    pozArt = i + 1;

                    codArt = conditiiArticole[i].cod;
                    if (codArt.Length == 8)
                        codArt = "0000000000" + codArt;

                    query = " insert into sapprd.zconddettableta(mandt,id,poz,codart,cant,um,valoare) " +
                            " values ('900',:idCmd,:pozArt, :codArt, :cant, :um, :valoare) ";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":idCmd", OracleType.Int32, 10).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = idCmd.Value;

                    cmd.Parameters.Add(":pozArt", OracleType.Int16, 3).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = pozArt;

                    cmd.Parameters.Add(":codArt", OracleType.NVarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = codArt;

                    cmd.Parameters.Add(":cant", OracleType.Double, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = conditiiArticole[i].cantitate;

                    cmd.Parameters.Add(":um", OracleType.NVarChar, 9).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = conditiiArticole[i].um;

                    cmd.Parameters.Add(":valoare", OracleType.Double, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[5].Value = conditiiArticole[i].valoare;

                    cmd.ExecuteNonQuery();

                    cmd.Dispose();

                    retVal = "0";
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return retVal;
        }
    }
}