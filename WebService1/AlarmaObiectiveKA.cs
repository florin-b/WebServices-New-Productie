using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using System.ComponentModel;
using System.Reflection;
using System.Globalization;
using System.Net.Mail;

namespace WebService1
{
    public class AlarmaObiectiveKA
    {


        public void sendAlarmObiectiv(string dateObiectiv)
        {


            string stadiuObiectiv = "";
            string categorieObiectiv = "";
            string numeJudet = "";
            string alarmText = "";
            string[] tokenAdresa;
            string adresa = "";
            string numeAntreprenor = "";
            string numeAgent = "";
            string departFinisaje = "";
            string stadiuSubantrep = "";
            string departInterior = "";

            try
            {

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                ObiectivGeneral obiectiv = serializer.Deserialize<ObiectivGeneral>(dateObiectiv);
                List<ObiectivConstructor> listConstructori = serializer.Deserialize<List<ObiectivConstructor>>(obiectiv.constructori);
                List<ObiectivStadii> listStadii = serializer.Deserialize<List<ObiectivStadii>>(obiectiv.stadii);
                List<string> departDirectori = new List<string>();

                tokenAdresa = obiectiv.adresaObiectiv.Split('/');

                stadiuObiectiv = HelperObiectiveKA.getEnumDescription((HelperObiectiveKA.EnumStadiuObiectiv)Int32.Parse(obiectiv.codStadiuObiectiv));
                categorieObiectiv = HelperObiectiveKA.getEnumDescription((HelperObiectiveKA.EnumCategorieObiectiv)Int32.Parse(obiectiv.codCategorieObiectiv));
                numeJudet = HelperObiectiveKA.getEnumDescription((HelperObiectiveKA.EnumJudete)Int32.Parse(tokenAdresa[0]));

                adresa = numeJudet + "/" + tokenAdresa[1] + (tokenAdresa.Length == 3 ? "/" + tokenAdresa[2] : "").ToString();
                numeAntreprenor = getNumeClient(obiectiv.codAntreprenorGeneral);
                numeAgent = getNumeAgent(obiectiv.codAgent);

                alarmText = " \n A fost creat un obiectiv cu urmatoarele date: \n\n";
                alarmText += " Ka\t " + numeAgent + " (" + obiectiv.unitLog + ") \n\n";
                alarmText += " Nume obiectiv\t\t: " + obiectiv.numeObiectiv + "\n";
                alarmText += " Stare obiectiv\t\t: " + stadiuObiectiv + "\n";
                alarmText += " Adresa\t\t\t: " + adresa + "\n";
                alarmText += " Beneficiar\t\t: " + obiectiv.numeBeneficiar + "\n";
                alarmText += " Antreprenor general\t: " + numeAntreprenor + "\n";
                alarmText += " Categorie\t\t: " + categorieObiectiv + "\n";
                alarmText += " Valoare\t\t: " + Double.Parse(obiectiv.valoareObiectiv).ToString("N", new CultureInfo("en-US")) + "\n\n";

                alarmText += " Stadii:\t\t\n";

                foreach (ObiectivStadii stadiu in listStadii)
                {
                    departFinisaje = HelperObiectiveKA.getEnumDescription((HelperObiectiveKA.EnumDepartFinisaje)Int32.Parse(stadiu.codDepart));
                    stadiuSubantrep = HelperObiectiveKA.getEnumDescription((HelperObiectiveKA.EnumStadiuSubantrep)Int32.Parse(stadiu.codStadiu));

                    alarmText += "\n " + departFinisaje + "\t: " + stadiuSubantrep + "\n";

                    foreach (ObiectivConstructor constructor in listConstructori)
                    {
                        if (!stadiu.codStadiu.Equals("0"))
                        {
                            if (!departDirectori.Contains(constructor.codDepart))
                                departDirectori.Add(constructor.codDepart);

                        }

                        if (stadiu.codDepart.Equals("00"))
                        {
                            if (constructor.codDepart.Equals("03") || constructor.codDepart.Equals("06") || constructor.codDepart.Equals("07"))
                            {
                                departInterior = HelperObiectiveKA.getEnumDescription((HelperObiectiveKA.EnumDepartInterioare)Int32.Parse(constructor.codDepart));
                                alarmText += " Client\t\t: " + getNumeClient(constructor.codClient) + " (" + departInterior + ") \n";

                            }
                        }
                        else if (stadiu.codDepart.Equals(constructor.codDepart))
                        {
                            departInterior = HelperObiectiveKA.getEnumDescription((HelperObiectiveKA.EnumDepartFinisaje)Int32.Parse(constructor.codDepart));
                            alarmText += " Client\t\t: " + getNumeClient(constructor.codClient) + "\n";

                        }


                    }

                }


                sendMailNotification(departDirectori, alarmText, obiectiv.unitLog);


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {

            }


            return;

        }


        private void sendMailNotification(List<string> listDepart, string alertText, string unitLog)
        {

            string depDir = "";

            foreach (String str in listDepart)
            {
                if (depDir.Equals(""))
                    depDir = "'" + str + "'";
                else
                    depDir += "," + "'" + str + "'";
            }

            depDir = "(" + depDir + ")";


            if (depDir.Equals("()"))
                return;

            string sqlString = "select mail from sapprd.zfil_dv where prctr ='" + unitLog + "' and spart in " + depDir;
            string[] listaAdrese = getDatabaseValues(sqlString).Split(',');


            foreach (String adresa in listaAdrese)
            {

                MailMessage message = new MailMessage();
                message.From = new MailAddress("notificari.tableta@arabesque.ro");
                message.To.Add(new MailAddress(adresa));
                message.Subject = "Informare obiectiv ka " ;
                message.Body = alertText;
                SmtpClient client = new SmtpClient("mail.arabesque.ro");
                client.Send(message);

                message.Dispose();

            }


        }


        private string getNumeClient(string codClient)
        {
            return getDatabaseValues("select nume from clienti where cod ='" + codClient + "' ");
        }

        private string getNumeAgent(string codAgent)
        {
            return getDatabaseValues("select nume from agenti where cod ='" + codAgent + "' and activ = 1 ");
        }


       

        private string getDatabaseValues(string sqlString)
        {

            string retVal = "";
            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();
            OracleCommand cmd = connection.CreateCommand();

            try
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                cmd.CommandText = sqlString;
                cmd.CommandType = CommandType.Text;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        if (retVal.Equals(""))
                            retVal = oReader.GetString(0);
                        else
                            retVal += "," + oReader.GetString(0);
                    }
                }

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
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