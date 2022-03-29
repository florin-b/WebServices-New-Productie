using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using System.Data.OracleClient;
using System.Data.Common;
using System.Net.Mail;

namespace TabletManagerWebServices
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://TabletManager.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Service1 : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public string HelloWorld1(string nume)
        {
            return "Hello World" + nume;
        }

        [WebMethod]
        public string userLogon(string userId, string userPass)
        {



            string retVal = "";
            OracleConnection connection = null;
            OracleCommand cmd = new OracleCommand();
            string connectionString = "";
            try
            {
                connectionString = GetConnectionString_prd();

                connection = new OracleConnection();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();
                cmd.CommandText = "web_pkg.wlogin";
                cmd.CommandType = CommandType.StoredProcedure;

                OracleParameter user = new OracleParameter("usname", OracleType.VarChar);
                user.Direction = ParameterDirection.Input;
                user.Value = userId;
                cmd.Parameters.Add(user);

                OracleParameter pass = new OracleParameter("uspass", OracleType.VarChar);
                pass.Direction = ParameterDirection.Input;
                pass.Value = userPass;
                cmd.Parameters.Add(pass);

                OracleParameter resp = new OracleParameter("x", OracleType.Number);
                resp.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(resp);

                OracleParameter depart = new OracleParameter("z", OracleType.VarChar, 5);
                depart.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(depart);

                OracleParameter comp = new OracleParameter("w", OracleType.VarChar, 12);
                comp.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(comp);

                OracleParameter tipAcces = new OracleParameter("k", OracleType.Number, 2);
                tipAcces.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(tipAcces);

                OracleParameter ipAddr = new OracleParameter("ipAddr", OracleType.VarChar, 15);
                ipAddr.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(ipAddr);

                OracleParameter idAg = new OracleParameter("agentID", OracleType.Number, 5);
                idAg.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(idAg);

                OracleParameter userName = new OracleParameter("numeUser", OracleType.VarChar, 128);
                userName.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(userName);

                OracleParameter usrId = new OracleParameter("userID", OracleType.Number, 5);
                usrId.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(usrId);

                cmd.ExecuteNonQuery();
                retVal = resp.Value.ToString() + "#" + depart.Value.ToString() + "#" + comp.Value.ToString() + "#" + userName.Value.ToString() + "#" + idAg.Value.ToString() + "#" + tipAcces.Value.ToString();

               

            }
            catch (Exception ex)
            {
                retVal = "-1";
                sendErrorToMail(ex.ToString() + " userid = " + userId);
                retVal = ex.ToString();
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }

            return retVal;
        }


        [WebMethod]
        public string updateTablet(string imei, string pin, string nrtel, string creatde, string activ)
        {

           
            string retVal = "", query = "";
            OracleConnection connection = new OracleConnection();

            try
            {
                string connectionString = GetConnectionString();

                connection.ConnectionString = connectionString;
                connection.Open();
                OracleCommand cmd = connection.CreateCommand();


                query = " update sapprd.ztablete_nrtel set cod_cartela = :codCartela, nr_telefon = :nrTel " +
                        " ,creat_de = :creatDe, activ = :activ where imei =:codImei ";
                            

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codImei", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = imei;

                cmd.Parameters.Add(":codCartela", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = pin;

                cmd.Parameters.Add(":nrTel", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = nrtel;

                cmd.Parameters.Add(":creatDe", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = creatde;

                cmd.Parameters.Add(":activ", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = activ;

                retVal = cmd.ExecuteNonQuery().ToString();


            }
            catch (Exception ex)
            {
                retVal = "-1";
                sendErrorToMail(ex.ToString());
            }

            finally
            {
                connection.Close();
                connection.Dispose();
            }



            return retVal;
        }


        [WebMethod]
        public string addNewTablet(string imei, string pin, string nrtel, string creatde)
        {
            string retVal = "0", query = "";
            bool exista = false;

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            string nowDate = year + month + day;

            try
            {
                string connectionString = GetConnectionString();

                connection.ConnectionString = connectionString;
                connection.Open();
                OracleCommand cmd = connection.CreateCommand();

                //verificare existenta
                cmd.CommandText = " select imei from sapprd.ztablete_nrtel where imei =:codImei  ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codImei", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = imei;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    exista = true;
                    retVal = "1";
                }
                oReader.Close();
                oReader.Dispose();

                //sf. verificare


                if (!exista)
                {
                    query = " insert into sapprd.ztablete_nrtel(mandt,imei,cod_cartela,nr_telefon,creat_de, data_mod, activ) " +
                            " values ('900',:codImei,:codCartela,:nrTel,:creatDe, :dataM, '1' )";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":codImei", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = imei;

                    cmd.Parameters.Add(":codCartela", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = pin;

                    cmd.Parameters.Add(":nrTel", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = nrtel;

                    cmd.Parameters.Add(":creatDe", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = creatde;

                    cmd.Parameters.Add(":dataM", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = nowDate;

                    cmd.ExecuteNonQuery();
                }

            }
            catch (Exception ex)
            {
                retVal = "-1";
                sendErrorToMail(ex.ToString());
            }

            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return retVal;
        }


        [WebMethod]
        public string searchImei(string imei)
        {
            string retVal = "", cod_cartela = "", nr_tel = "", activ = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = GetConnectionString();

                connection.ConnectionString = connectionString;
                connection.Open();
                OracleCommand cmd = connection.CreateCommand();

                //verificare existenta
                cmd.CommandText = " select cod_cartela,nr_telefon, activ from sapprd.ztablete_nrtel where imei =:codImei  ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codImei", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = imei;

                oReader = cmd.ExecuteReader();

                int i = 0;
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        cod_cartela = oReader.GetString(0);
                        nr_tel = oReader.GetString(1);
                        activ = oReader.GetString(2);
                        i++;
                    }
                }
                oReader.Close();
                oReader.Dispose();

                if (i == 0)
                {
                    retVal = "0";
                }
                if (i == 1)
                {
                    retVal = cod_cartela + "#" + nr_tel + "#" + activ;
                }
               
                


                

            }
            catch (Exception ex)
            {
                retVal = "-1";
                sendErrorToMail(ex.ToString());
            }

            finally
            {
                connection.Close();
                connection.Dispose();
            }



            return retVal;
        }

        [WebMethod]
        public DataSet getAgenti(string filiale, string departamente, bool tipAV, bool tipSD, bool tipKA)
        {
            DataSet retVal = new DataSet();
           

            OracleConnection connection = new OracleConnection();
            

            try
            {
                string connectionString = GetConnectionString();

                string condAV = "", condKA = " or b.divizie = '' ", condDepart = " b.divizie in ('1') ";

                if (tipAV)
                    condAV = "  and b.tip = 'AV' ";

                if (tipSD)
                    condAV = "  and b.tip = 'SD' ";

                if (tipAV && tipSD)
                {   
                    condAV = " and b.tip in  ('AV','SD') ";
                }

                if (tipKA)
                    condKA = " b.divizie = '10' ";


                if (!departamente.Equals(""))
                    condDepart = " b.divizie in (" + departamente + ") ";
               


                connection.ConnectionString = connectionString;
                connection.Open();
                OracleCommand cmd = connection.CreateCommand();

                string sqlStringAG = " select web_pkg.decrypt(a.useru)||', '||b.nume||', '||decode(b.tip,'SD','SD',decode(b.divizie,'10','KA','AG'))||', '||b.divizie||', '||b.filiala " + 
                                     " from web_users a, agenti b where  " +
                                     " b.cod = a.agentid and b.filiala in (" + filiale + ") and ( " + condDepart + " ) " + condAV + "  and b.activ = 1  ";

                string sqlStringKA = "";
                if (tipKA)
                {
                    sqlStringKA = " select web_pkg.decrypt(a.useru)||', '||b.nume||', '||decode(b.tip,'SD','SD',decode(b.divizie,'10','KA','AG'))||', '||b.divizie||', '||b.filiala from web_users a, agenti b where  " +
                                  " b.cod = a.agentid and b.filiala in (" + filiale + ") and ( " + condKA + " ) " +
                                  "  and b.activ = 1  ";
                }

                string sqlStringFinal = "";
                if (!tipAV && !tipSD)
                    sqlStringFinal = sqlStringKA;
                else
                {
                    if (tipKA)
                        sqlStringFinal = sqlStringAG + " union " + sqlStringKA;
                    else
                        sqlStringFinal = sqlStringAG ;

                }


                cmd.CommandText = sqlStringFinal;


                

                

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                adapter.Fill(retVal);
                




            }
            catch (Exception ex)
            {
                retVal = null;
                sendErrorToMail(ex.ToString());
            }

            finally
            {
                connection.Close();
                connection.Dispose();
            }



            return retVal;

            
        }


        static private string GetConnectionString()
        {
            //prd
            return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                              " (HOST = 10.1.3.93)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = PRD))); " +
                              " User Id = WEBSAP; Password = 2INTER7;";



        }


        static private string GetConnectionString_prd()
        {

            return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                   " (HOST = 10.1.3.93)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = PRD))); " +
                   " User Id = WEBSAP; Password = 2INTER7;";

        }


        private void sendErrorToMail(string errMsg)
        {
            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress("Android.WebService@arabesque.ro");
                message.To.Add(new MailAddress("florin.brasoveanu@arabesque.ro"));
                message.Subject = "Tablet Manager Error";
                message.Body = errMsg;
                SmtpClient client = new SmtpClient("mail.arabesque.ro");
                client.Send(message);
            }
            catch (Exception)
            {

            }

        }


    }
}
