using System;
using System.Data.OracleClient;
using System.Data;

namespace WebService1
{
    public class Stocuri
    {

        public string getStocAndroid(string codArt, string filiala, string showCmp, string depart)
        {


            //stoc articol
            string retVal = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string depArt = "";
            string umArt = "";
            float cant = 0;
            string condFil1 = "", condFil2 = "", cmpVal = "", filGed = "", sinteticArt = "", filWood = "";
            string showStocVal_ = "1";

            if (filiala == "BU")
            {
                condFil1 = " and m.werks in ('BU10','BU11','BU12','BU13') ";
                condFil2 = " and e.werks in ('BU10','BU11','BU12','BU13') ";
            }
            else
            {
                filGed = filiala.Substring(0, 2) + "2" + filiala.Substring(3, 1);
                filWood = filiala.Substring(0, 2) + "4" + filiala.Substring(3, 1);
                condFil1 = " and m.werks in ('" + filiala + "','" + filGed + "','" + filWood + "')";
                condFil2 = " and e.werks in ('" + filiala + "','" + filGed + "','" + filWood + "')";
            }

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select lgort, nvl(sum(labst),0) stoc, meins,lgort, sintetic from " +
                                  " (select m.lgort,m.labst , mn.meins, mn.matnr  from sapprd.mard m, sapprd.mara mn " +
                                  " where m.mandt = '900'  and m.mandt = mn.mandt and m.matnr = mn.matnr and m.lgort != 'CUSF' " +
                                  " and m.matnr =:art " + condFil1 +
                                  " union all " +
                                  " select e.lgort,-1 * sum(e.omeng), e.meins, e.matnr  from sapprd.vbbe e " +
                                  " where e.mandt = '900' " +
                                  " and e.matnr =:art and e.lgort != 'CUSF' " + condFil2 +
                                  " group by e.meins,e.lgort, e.matnr), articole ar where ar.cod = matnr " +
                                  " group by meins,lgort, sintetic having sum(labst) > 0 ";



                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":art", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArt;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        cant = oReader.GetFloat(1);
                        depArt = oReader.GetString(0);
                        umArt = oReader.GetString(2);
                        sinteticArt = oReader.GetString(4);
                        retVal += cant.ToString() + "#" + umArt + "#" + depArt + "@@";
                    }
                }
                else
                {
                    retVal = "0# # @@";
                }

                cmpVal = "0";

                if (showCmp == "1")
                {

                    cmd.CommandText = " select nvl(to_char(decode(y.lbkum,0,y.verpr,y.salk3/y.lbkum),'99999.9999'),0) from sapprd.mbew y where " +
                                      " y.mandt='900' and y.matnr=:matnr  and y.bwkey = :unitLog  ";


                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":matnr", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codArt;

                    cmd.Parameters.Add(":unitLog", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = filiala;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        cmpVal = oReader.GetString(0);

                    }

                }
                else
                {
                    //exceptie sintetice
                    if (filiala == "BV90")
                    {
                        if (depart == "02")
                        {
                            if (Service1.isArtPermited(sinteticArt))
                            {
                                showStocVal_ = "1";
                            }
                            else  //nu este permisa vanzarea altor articole, se afiseaza fara stoc
                            {
                                cant = 0;
                                umArt = " ";
                                depArt = " ";
                                showStocVal_ = "1";

                                retVal = cant.ToString() + "#" + umArt + "#" + depArt + "@@";
                            }
                        }
                    }
                }

                retVal += "!" + cmpVal + "!" + showStocVal_ + "!" + getStocImbatranit(connection, codArt, filiala) + "!" + getStocBlocat(connection, codArt, filiala); ;


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }


            return retVal;

        }




        private string getStocImbatranit(OracleConnection connection, string codArt, string filiala)
        {

            double stocInit = 0, stocEpuizat = 0, stocImbatranit = 0;

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select nvl(sum(b.p5c + b.p6c + b.p7c + b.p8c),0) stoc_batran from sapprd.ZSTOC_JOB b where b.matnr =:codArt and werks =:filiala  ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArt;

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filiala;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    stocInit = oReader.GetDouble(0);
                }


                string sqlString = " select nvl(sum(p.cant_umb),0) stocEpuizat from sapprd.zcomhead_Tableta r, sapprd.zcomdet_Tableta p " +
                                   " where r.mandt = '900' and r.datac =:dataC and r.mandt = p.mandt and r.id = p.id and r.status in ('2','10') " +
                                   " and p.ul_stoc =:filiala  and p.cod =:codArt ";

                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":dataC", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Utils.getCurrentDate();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filiala;

                cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codArt;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    stocEpuizat = oReader.GetDouble(0);
                }

                stocImbatranit = stocInit - stocEpuizat;

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString()+ " , " + codArt + " , " + filiala);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return stocImbatranit.ToString();

        }


        private string getStocBlocat(OracleConnection connection, string codArticol, string filiala)
        {
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string stocBlocat = "#";
            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select (r.intr1c_nb + r.intr2c + r.intr3c + r.intr4c + r.intr5v + r.intr6c + r.intr1c_ub) qty_deschise, r.dismm, " +
                                  " r.werks, r.matnr from sapprd.zcalc_rotatii r " +
                                  " where r.mandt = '900'  and r.matnr = :codArt and r.werks = :filiala ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filiala;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    if (oReader.GetDouble(0) > 0)
                        stocBlocat = oReader.GetDouble(0) + "#" + oReader.GetString(1);
                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return stocBlocat;
        }



    }
}