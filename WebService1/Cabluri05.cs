using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace WebService1
{
    public class Cabluri05
    {
        public string getCabluri05(string codArticol, string sinteticArticol)
        {

            
            List<Stoc05> listStoc = new List<Stoc05>();

            try {


                if (!sinteticArticol.Equals("504") && !sinteticArticol.Equals("505"))
                    return new JavaScriptSerializer().Serialize(listStoc);


                SapWsCabluri05.ZWMS_UL10_GET_STOCK_ARAB webService = new SapWsCabluri05.ZWMS_UL10_GET_STOCK_ARAB();
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(DatabaseConnections.getUser(), DatabaseConnections.getPass());
                webService.Credentials = nc;
                webService.Timeout = 300000;

                SapWsCabluri05.ZwmsUl10GetStockArab inParam = new SapWsCabluri05.ZwmsUl10GetStockArab();

                inParam.Department = "05";
                inParam.Material = codArticol;
                inParam.PersonalNo = "00000000";
                inParam.Warehouse = "BV9";
                inParam.StockFinal = new SapWsCabluri05.ZstWmsLqua[1];


                SapWsCabluri05.ZwmsUl10GetStockArabResponse response = new SapWsCabluri05.ZwmsUl10GetStockArabResponse();

                response = webService.ZwmsUl10GetStockArab(inParam);

                for (int i = 0; i < response.StockFinal.Length; i++)
                {
                    Stoc05 stoc = new Stoc05();
                    stoc.numeBoxa = response.StockFinal[i].Lgpla;
                    stoc.codBoxa = response.StockFinal[i].Lenum;
                    stoc.stoc = response.StockFinal[i].Verme.ToString();

                    if (Double.Parse(stoc.stoc) > 0)
                        listStoc.Add(stoc);
                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + codArticol + " , " + sinteticArticol);
            }

            return new JavaScriptSerializer().Serialize(listStoc);

        }

        public void insertCabluri05(OracleConnection connection, OracleTransaction transaction, string idComanda, string codArticol, string serListCabluri)
        {

            List<CantCablu> listCabluri = new JavaScriptSerializer().Deserialize<List<CantCablu>>(serListCabluri);

            OracleCommand cmd = connection.CreateCommand();

            try
            {
                foreach (CantCablu cablu in listCabluri)
                {
                    cmd.CommandText = " insert into sapprd.ZWMS_POZ_TABLETA (mandt, id, matnr, lenum, lgpla, verme) " +
                                      " values ('900', :id, :matnr, :lenum, :lgpla, :verme) ";

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":id", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = idComanda;

                    cmd.Parameters.Add(":matnr", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codArticol;

                    cmd.Parameters.Add(":lenum", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = cablu.codBoxa;

                    cmd.Parameters.Add(":lgpla", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = cablu.numeBoxa;

                    cmd.Parameters.Add(":verme", OracleType.Number, 11).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = Double.Parse(cablu.cantitate);

                    if (transaction != null)
                        cmd.Transaction = transaction;

                    cmd.ExecuteNonQuery();
                }



            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }



        }

        public string getCabluriArticol(OracleConnection connection, string idComanda, string codArticol)
        {
            List<CantCablu> listCabluri = new List<CantCablu>();

            OracleCommand cmd = connection.CreateCommand();
            OracleDataReader oReader = null;


            try
            {

                cmd.CommandText = " select lenum, lgpla, verme from sapprd.ZWMS_POZ_TABLETA where mandt='900' and id=:id and matnr=:matnr ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":id", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                cmd.Parameters.Add(":matnr", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codArticol.Length == 8 ? "0000000000" + codArticol : codArticol;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        CantCablu cablu = new CantCablu();
                        cablu.codBoxa = oReader.GetString(0);
                        cablu.numeBoxa = oReader.GetString(1);
                        cablu.cantitate = oReader.GetDecimal(2).ToString();
                        listCabluri.Add(cablu);
                    }

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



            return new JavaScriptSerializer().Serialize(listCabluri);
        }


        class Stoc05
        {
            public string numeBoxa;
            public string codBoxa;
            public string stoc;
        }
    }
}