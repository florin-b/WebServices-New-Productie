using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace WebService1
{
    public class Venituri
    {

        public string getInfoVenituriData(string codDepart, string filiala, string luna, string an, string codAgent)
        {
            string serializedResult = "", sqlString = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            List<InfoVenituri> listaVenituri = new List<InfoVenituri>();


            if (codDepart.Equals("040") || codDepart.Equals("041"))
                codDepart = "04";

            string condAgent = "";
            if ((an.Equals("2017") && (Int32.Parse(luna) >= 4)) || Int32.Parse(an) >=2018)
            {
                condAgent = " and cod_agent = '" + codAgent + "' ";
            }


            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                if (codDepart.StartsWith("04"))
                {
                    sqlString = " select distinct a.id, " +
                                " nvl((select b.venitnet_p from  sapprd.zxy_coef b where b.mandt = a.mandt and b.id = a.id and b.prctr = a.prctr and b.anul = a.anul and " +
                                " b.luna = a.luna and b.zvkgrp like '040' " + condAgent + "),0) venitnet_p_040, " +
                                " nvl((select b.m_p from  sapprd.zxy_coef b where b.mandt = a.mandt and b.id = a.id and b.prctr = a.prctr and b.anul = a.anul " +
                                " and b.luna = a.luna and b.zvkgrp = '040' " + condAgent + "),0) m_p_040, " +
                                " nvl((select b.venitnet_p from  sapprd.zxy_coef b where b.mandt = a.mandt and b.id = a.id and b.prctr = a.prctr and b.anul = a.anul " +
                                " and b.luna = a.luna and b.zvkgrp = '041' " + condAgent + "),0) venitnet_p_041, " +
                                " nvl((select b.m_p from  sapprd.zxy_coef b where b.mandt = a.mandt and b.id = a.id and b.prctr = a.prctr and b.anul = a.anul and " +
                                " b.luna = a.luna and b.zvkgrp = '041' " + condAgent + "),0) m_p_041 " +
                                " from sapprd.zxy_coef a where a.mandt = '900' and a.prctr =:filiala and a.anul =:an and a.luna =:luna " +
                                " and a.zvkgrp in ('040','041') and a.id in (1,2,3,4,5) " + condAgent + " order by a.id ";



                  
                        
                }
                else
                {
                    sqlString = " select a.id, a.venitnet_p, a.m_p from sapprd.zxy_coef a where a.mandt = '900' and a.prctr =:filiala and a.anul =:an and a.luna =:luna " +
                                " and substr(a.zvkgrp,0,2) = :codDepart and a.id in (1,2,3,4,5) " + condAgent + " order by a.id ";
                }

               

                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                cmd.Parameters.Add(":an", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = an;

                cmd.Parameters.Add(":luna", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = luna;

                if (!codDepart.StartsWith("04"))
                {
                    cmd.Parameters.Add(":codDepart", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = codDepart;
                }

                oReader = cmd.ExecuteReader();


                InfoVenituri unVenit = null;

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        if (codDepart.StartsWith("04"))
                        {
                            unVenit = new InfoVenituri();
                            unVenit.id = oReader.GetInt32(0).ToString();
                            unVenit.venitNetP040 = oReader.GetDouble(1).ToString();
                            unVenit.mP040 = oReader.GetDouble(2).ToString();
                            unVenit.venitNetP041 = oReader.GetDouble(3).ToString();
                            unVenit.mP041 = oReader.GetDouble(4).ToString();
                            unVenit.venitNetP = "0";
                            unVenit.mP = "0";
                            listaVenituri.Add(unVenit);
                        }
                        else
                        {
                            unVenit = new InfoVenituri();
                            unVenit.id = oReader.GetInt32(0).ToString();
                            unVenit.venitNetP = oReader.GetDouble(1).ToString();
                            unVenit.mP = oReader.GetDouble(2).ToString();
                            unVenit.venitNetP040 = "0";
                            unVenit.venitNetP041 = "0";
                            unVenit.mP040 = "0";
                            unVenit.mP041 = "0";
                            listaVenituri.Add(unVenit);
                        }

                    }

                }


                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaVenituri);


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " det: " + codDepart +" , " +  filiala + ", " + luna + " ," + an + " , " + codAgent);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }


            return serializedResult;
        }


    }
}