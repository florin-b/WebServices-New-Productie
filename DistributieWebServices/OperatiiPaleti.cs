using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace DistributieWebServices
{
    public class OperatiiPaleti
    {

        public string getPaletiNereturnati(string codSofer)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            List<Palet> listPaleti = new List<Palet>();

            
            try
            {
                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select ad.name1 nume_client, b.vbeln borderou, decode(length(a.cod),18,substr(a.cod,-8),a.cod) codart, a.nume palet, p.qty, " +
                                  " p.returnat, decode(p.data_ret,'00000000',p.data_ret, to_date(p.data_ret,'yyyymmdd')) , p.ora_ret from sapprd.zimprumut_palet p, websap.articole a, sapprd.vbpa s, websap.soferi n, " +
                                  " sapprd.vbfa b, sapprd.vbpa c, sapprd.adrc ad where qty > returnat and p.palet = a.cod and p.mandt = s.mandt and p.mandt='900' " +
                                  " and p.vbeln = s.vbeln and s.parvw = 'ZF' and s.pernr = n.cod and s.pernr = :codSofer and p.mandt = b.mandt " +
                                  " and p.vbeln = b.vbelv and b.vbtyp_v = 'J' and b.vbtyp_n = '8' and p.mandt = c.mandt and p.vbeln = c.vbeln " +
                                  " and c.parvw = 'WE' and c.mandt = ad.client and c.adrnr = ad.addrnumber order by b.vbeln, ad.name1, p.data_ret ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        Palet palet = new Palet();
                        palet.numeClient = oReader.GetString(0);
                        palet.borderou = oReader.GetString(1);
                        palet.codPalet = oReader.GetString(2);
                        palet.numePalet = oReader.GetString(3);
                        palet.cantitate = oReader.GetDouble(4).ToString();
                        palet.returnat = oReader.GetDouble(5).ToString();
                        palet.dataRetur = oReader.GetString(6) + " " + oReader.GetString(7).Substring(0, 2) + ":" + oReader.GetString(7).Substring(3, 2);
                        listPaleti.Add(palet);

                    }

                }
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + codSofer);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return new JavaScriptSerializer().Serialize(listPaleti);
            

        }





        public void getPaletiComanda(List<ArticoleFactura> listArticole, string borderou, string codClient, string codAdresa)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select  a.nume, p.qty, a.grup_vz, a.umvanz " +
                                  " from sapprd.zimprumut_palet p, websap.articole a, sapprd.vbpa s, websap.soferi n, sapprd.vbfa b, sapprd.vbpa c, sapprd.adrc ad " +
                                  " where qty >= returnat and p.mandt = '900' and p.palet = a.cod and p.mandt = s.mandt and p.vbeln = s.vbeln and s.parvw = 'ZF' " +
                                  " and s.pernr = n.cod and p.mandt = b.mandt and p.vbeln = b.vbelv and b.vbtyp_v = 'J' and b.vbtyp_n = '8' and p.mandt = c.mandt " +
                                  " and p.vbeln = c.vbeln and c.parvw = 'WE' and c.mandt = ad.client and c.adrnr = ad.addrnumber " +
                                  " and b.vbeln = :borderou and ad.addrnumber = :codAdresa and c.kunnr = :codClient ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":borderou", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = borderou;

                cmd.Parameters.Add(":codAdresa", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAdresa;

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ArticoleFactura articol = new ArticoleFactura();
                        articol.nume = oReader.GetString(0) + " (palet imprumutat)";
                        articol.cantitate = oReader.GetDouble(1).ToString();
                        articol.departament = oReader.GetString(2);
                        articol.umCant = oReader.GetString(3);
                        articol.greutate = "0";
                        articol.umGreutate = "KG";
                        articol.tipOperatiune = "desc";
                        listArticole.Add(articol);

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

        }


    }


    class Palet
    {
        public string numePalet;
        public string codPalet;
        public string numeClient;
        public string borderou;
        public string cantitate;
        public string returnat;
        public string dataRetur;
    }

}