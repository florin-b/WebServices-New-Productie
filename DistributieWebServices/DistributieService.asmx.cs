using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;


namespace DistributieWebServices
{

    [WebService(Namespace = "http://distributie.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class Service1 : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            System.Threading.Thread.Sleep(2000);
            return "Hello World from ASP.Net";
        }


        [WebMethod]
        public string getBorderouri(string codSofer, string interval, string tip)
        {
            OperatiiDocumente opDocumente = new OperatiiDocumente();
            return opDocumente.getBorderouri(codSofer, interval, tip);
        }

        [WebMethod]
        public string getBorderouriTEST(string codSofer, string interval, string tip)
        {
            OperatiiDocumente opDocumente = new OperatiiDocumente();
            return opDocumente.getBorderouriTEST(codSofer, interval, tip);
        }


        [WebMethod]
        public string getBorderouriMasina(string nrMasina, string codSofer)
        {
            OperatiiDocumente opDocumente = new OperatiiDocumente();
            return opDocumente.getBorderouriMasina(nrMasina, codSofer);
        }

        [WebMethod]
        public string getBorderouriMasinaTEST(string nrMasina, string codSofer)
        {
            OperatiiDocumente opDocumente = new OperatiiDocumente();
            return opDocumente.getBorderouriMasinaTEST(nrMasina, codSofer);
        }

        [WebMethod]
        public string getEvenimenteBorderouri(string codSofer, string interval)
        {
            OperatiiEvenimente eveniment = new OperatiiEvenimente();
            return eveniment.getEvenimenteBorderouri(codSofer, interval);
        }

        [WebMethod]
        public string getFacturiBorderou(string nrBorderou, string tipBorderou)
        {
            OperatiiDocumente opDocumente = new OperatiiDocumente();
            return opDocumente.getFacturiBorderou(nrBorderou, tipBorderou);
        }

        [WebMethod]
        public string getArticoleBorderou(string nrBorderou, string codClient, string codAdresa)
        {
            OperatiiDocumente opDocumente = new OperatiiDocumente();
            return opDocumente.getArticoleBorderouWS(nrBorderou, codClient, codAdresa);
        }


        [WebMethod]
        public string getArticoleBorderouDistributie(string nrBorderou, string codClient, string codAdresa)
        {
            OperatiiDocumente opDocumente = new OperatiiDocumente();
            return opDocumente.getArticoleBorderouDistributie(nrBorderou, codClient, codAdresa);
        }


        [WebMethod]
        public string getEvenimenteBorderou(string nrBorderou)
        {
            OperatiiEvenimente eveniment = new OperatiiEvenimente();
            return eveniment.getEvenimenteBorderou(nrBorderou);
        }

        [WebMethod]
        public string saveNewEvent(string serializedEvent, string serializedEtape)
        {
            OperatiiEvenimente eveniment = new OperatiiEvenimente();
            if (serializedEtape != null && serializedEtape != "")
                eveniment.saveOrdineEtape(serializedEtape, serializedEvent);

            return eveniment.saveNewEvent(serializedEvent);
        }



        [WebMethod]
        public string saveStop(string idEveniment, string codSofer, string codBorderou, string codEveniment)
        {
            OperatiiEvenimente eveniment = new OperatiiEvenimente();
            return eveniment.saveNewStop(idEveniment, codSofer, codBorderou, codEveniment);
        }


        [WebMethod]
        public string saveEvenimentStopIncarcare(string document, string codSofer)
        {
            OperatiiEvenimente eveniment = new OperatiiEvenimente();
            return eveniment.saveEvenimentStopIncarcare(document, codSofer);
        }

        [WebMethod]
        public string getEvenimentStopIncarcare(string document, string codSofer)
        {
            OperatiiEvenimente eveniment = new OperatiiEvenimente();
            return eveniment.getEvenimentStopIncarcare(document, codSofer);
        }


        [WebMethod]
        public string cancelEvent(string tipEveniment, string nrDocument, string codClient, string codSofer)
        {
            OperatiiEvenimente eveniment = new OperatiiEvenimente();
            return eveniment.cancelEvent(tipEveniment, nrDocument, codClient, codSofer);
        }

        [WebMethod]
        public string getPozitieCurenta(string nrBorderou)
        {
            OperatiiEvenimente eveniment = new OperatiiEvenimente();
            return eveniment.getPozitieCurenta(nrBorderou);
        }



        [WebMethod]
        public string getDocEvents(string nrDoc, string tipEv)
        {
            OperatiiDocumente opDocumente = new OperatiiDocumente();
            return opDocumente.getDocEvents(nrDoc, tipEv);
        }



        [WebMethod]
        public string userLogon(string userId, string userPass, string ipAdr)
        {
            Logon logon = new Logon();
            return logon.userLogon(userId, userPass, ipAdr);
        }

        [WebMethod]
        public string getCodSofer(string codTableta, string deviceInfo)
        {
            Logon logon = new Logon();
            return logon.getCodSofer(codTableta,  deviceInfo);
        }


        [WebMethod]
        public string getSoferi()
        {
            OperatiiSoferi opSoferi = new OperatiiSoferi();
            return opSoferi.getSoferi();
        }

        [WebMethod]
        public string getMasinaSofer(string codSofer)
        {
            return new OperatiiSoferi().getMasinaSofer(codSofer);
        }

        [WebMethod]
        public string getMasiniFiliala(string codFiliala)
        {
            return new OperatiiSoferi().getMasiniFiliala(codFiliala);
        }



        [WebMethod]
        public string getKmMasina(string nrMasina)
        {
            return new OperatiiSoferi().getKmMasina(nrMasina);
        }

        [WebMethod]
        public bool getKmMasinaDeclarati(string nrMasina)
        {
            return new OperatiiMasina().getKmMasinaDeclarati(nrMasina);
        }

        [WebMethod]
        public bool valideazaKmMasina(string nrMasina, string nrKm)
        {
            return new OperatiiMasina().valideazaKmMasina(nrMasina, nrKm);
        }


        [WebMethod]
        public string verificaKmSalvati(string nrAuto, string kmNoi)
        {
            return new OperatiiMasina().verificaKmSalvati(nrAuto, kmNoi);
        }


        [WebMethod]
        public void adaugaKmMasina(string codAngajat, string nrAuto, string km)
        {
             new OperatiiMasina().adaugaKmMasina(codAngajat, nrAuto, km);
        }

        [WebMethod]
        public void sendSmsClientiFromBuffer()
        {
            new Notificari().sendSmsFromBuffer();
        }

        [WebMethod]
        public void getMasiniCursa()
        {
            new OperatiiEvenimente().getMasiniPlecateInCursa();
        }


        [WebMethod]
        public string getArticoleBorderouService(string codBorderou, string codAdresa)
        {
            return new OperatiiDocumente().getArticoleBordMathaus(codBorderou, codAdresa);
        }

        [WebMethod]
        public string getPaletiNereturnati(string codSofer)
        {
            return new OperatiiPaleti().getPaletiNereturnati(codSofer);
        }

        [WebMethod]
        public string afisFoaieParcurs(string codSofer, string nrMasina, string an, string luna)
        {
            return new OperatiiFoaieParcurs().afisFoaieParcurs(codSofer, nrMasina, an, luna);
        }

        [WebMethod]
        public string getFoaieParcurs(string codSofer, string nrMasina, string an, string luna, string tipOp)
        {
            return new OperatiiFoaieParcurs().getFoaieParcurs(codSofer, nrMasina, an, luna, tipOp);
        }

        [WebMethod]
        public string saveFoaieParcurs(string foaieData, string codSofer, string nrMasina)
        {
            return new OperatiiFoaieParcurs().saveFoaieParcurs(foaieData, codSofer, nrMasina);
        }

        [WebMethod]
        public string inchideLunaFP(string codSofer, string nrMasina, string an, string luna)
        {
            return new OperatiiFoaieParcurs().inchideLunaFP(codSofer, nrMasina, an, luna);
        }

        [WebMethod]
        public string getAlimentare(string codSofer, string nrMasina, string data)
        {
            return new OperatiiMasina().getAlimentare(codSofer, nrMasina, data);
        }

        [WebMethod]
        public string adaugaAlimentare(string strData)
        {
            return new OperatiiMasina().adaugaAlimentare(strData);
        }

        [WebMethod]
        public string adaugaSiroco(string strData)
        {
            return new OperatiiMasina().adaugaSiroco(strData);
        }

        [WebMethod]
        public string getSiroco(string codSofer, string nrMasina, string data)
        {
            return new OperatiiMasina().getSiroco(codSofer, nrMasina, data);
        }

        [WebMethod]
        public void testNrClienti()
        {
            OracleConnection connection = new OracleConnection();

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();
            connection.ConnectionString = connectionString;
            connection.Open();


            new OperatiiEvenimente().getClientsPhoneNumber(connection, "0002229905");


            //new OperatiiEvenimente().getDateComanda(connection, "0002219761", "4119000033", "9013948700");
                


            connection.Close();
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



        private string getCurrentTime()
        {
            string mTime = "";
            DateTime cDate = DateTime.Now;
            mTime = cDate.Hour.ToString("00") + cDate.Minute.ToString("00") + cDate.Second.ToString("00");
            return mTime;
        }




        private string getKMFromFMS(string fmsString)
        {
            string kmValue = "0";

            if (!fmsString.Equals("0"))
            {
                string[] fmsToken = fmsString.Split('#');
                string[] kmToken1 = fmsToken[5].Split(',');
                string[] kmToken2 = kmToken1[1].Split('*');

                kmValue = kmToken2[0];
            }
            return kmValue;
        }


        [WebMethod]
        public string GetLocationAddress(string lat, string lng)
        {
            string currentAddress = "";
            HttpWebRequest request = default(HttpWebRequest);
            HttpWebResponse response = null;
            StreamReader reader = default(StreamReader);
            string json = null;

            try
            {

                request = (HttpWebRequest)WebRequest.Create("http://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + ", " + lng + "&sensor=false");

                response = (HttpWebResponse)request.GetResponse();

                reader = new StreamReader(response.GetResponseStream());
                json = reader.ReadToEnd();
                response.Close();

                if (json.Contains("ZERO_RESULTS"))
                {
                    currentAddress = "Adresa indisponibila";
                };
                if (json.Contains("formatted_address"))
                {

                    int start = json.IndexOf("formatted_address");
                    int end = json.IndexOf(", Romania");
                    string AddStart = json.Substring(start + 21);
                    string EndStart = json.Substring(end);
                    string FinalAddress = AddStart.Replace(EndStart, "");

                    currentAddress = FinalAddress;


                };
            }
            catch (Exception ex)
            {
                string Message = "Error: " + ex.ToString();
                currentAddress = "Adresa indisponibila";
            }
            finally
            {
                if ((response != null))
                    response.Close();
            }

            return currentAddress;

        }


        private void sendErrorToMail(string errMsg)
        {

            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress("Distributie.WebService@arabesque.ro");
                message.To.Add(new MailAddress("florin.brasoveanu@arabesque.ro"));
                message.Subject = "Distributie WebService Error";
                message.Body = errMsg;
                SmtpClient client = new SmtpClient("mail.arabesque.ro");
                client.Send(message);
            }
            catch (Exception)
            {

            }

        }

        [WebMethod]
        public string getNrTelefonClienti()
        {
            return new Sms().getTelClienti();
        }

    }
}