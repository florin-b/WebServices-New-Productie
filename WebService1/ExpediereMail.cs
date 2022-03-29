

using System;
using System.Data;
using System.Data.OracleClient;
using System.Net.Mail;
using System.IO;

namespace WebService1
{
    public class ExpediereMail : System.Web.Services.WebService
    {

        public string sendOfertaGedMail(string nrComanda, string adresaMail)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;



            try
            {
                using (StreamReader reader = File.OpenText(Server.MapPath("~/TemplateOferta.html")))
                {
                    string tableBody = "";



                    //antet oferta
                    string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    cmd = connection.CreateCommand();
                    cmd.CommandText = " select b.nume, c.nume, a.pers_contact,a.telefon, b.nrtel, a.ul, a.nume_client, b.email from sapprd.zcomhead_tableta a, agenti b, clienti c where " +
                                      " id =:idCmd and b.cod = a.cod_agent and c.cod = a.cod_client ";


                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idCmd", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = nrComanda;
                    oReader = cmd.ExecuteReader();

                    string numeClient = "", persContact = "", telContact = "", numeAgent = "", telAgent = "", unitLog = "", mail = "";
                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        numeAgent = oReader.GetString(0);
                        numeClient = oReader.GetString(6).Trim() != "" ? oReader.GetString(6) : oReader.GetString(1);
                        persContact = oReader.GetString(2);
                        telContact = oReader.GetString(3);
                        telAgent = oReader.GetString(4);
                        unitLog = oReader.GetString(5);

                        if (oReader.GetString(7).Trim() != "")
                            mail = ", email: " + oReader.GetString(7);
                    }


                    //date filiala
                    string adresaUnitLog = "", telUnitLog = "", faxUnitLog = "", bancaUnitLog = "", contUnitLog = "";
                    string[] tokAdrUnitLog = General.AddressUtils.getAdrUnitLog(unitLog.Substring(0, 2) + "1" + unitLog.Substring(3, 1)).Split('#');
                    adresaUnitLog = tokAdrUnitLog[0];
                    telUnitLog = tokAdrUnitLog[1];
                    faxUnitLog = tokAdrUnitLog[2];
                    bancaUnitLog = tokAdrUnitLog[3];
                    contUnitLog = tokAdrUnitLog[4];



                    //articole oferta
                    cmd.CommandText = "select decode(length(a.cod),18,substr(a.cod,-8),a.cod), b.nume, a.cant_umb, a.valoare, a.umb from sapprd.zcomdet_tableta a, articole b where " +
                                      " a.id =:idCmd and a.cod = b.cod ";

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idCmd", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = nrComanda;
                    oReader = cmd.ExecuteReader();

                    tableBody = "<tbody> ";

                    int nrArt = 1;
                    string oLinie = "", altColor = "";
                    decimal tva = 0.19M;
                    decimal totalVal = 0, totalTVA = 0, totalGen = 0, valArt = 0, valTVA = 0;


                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            altColor = "class='alt'";
                            if ((nrArt + 1) % 2 == 0)
                                altColor = "";

                            valArt = oReader.GetDecimal(3) * oReader.GetDecimal(2);
                            valTVA = oReader.GetDecimal(3) * oReader.GetDecimal(2) * tva;

                            oLinie = "<tr " + altColor + " ><td align='center'>" + nrArt.ToString() + ". </td><td align='left'>" + oReader.GetString(1) + " (" + oReader.GetString(0) + ")" +
                                     "</td><td align='center'>" + oReader.GetString(4) + " </td><td align='center'>" + oReader.GetDecimal(2) + "</td> " +
                                     "<td align='right'>" + String.Format("{0:0.00}", oReader.GetDecimal(3)) + " </td><td align = 'right'>" +
                                     String.Format("{0:0.00}", valArt) + "</td> " +
                                     "<tr>";

                            tableBody += oLinie;

                            totalVal += decimal.Parse(String.Format("{0:0.00}", valArt));
                            totalTVA += decimal.Parse(String.Format("{0:0.00}", valTVA));

                            nrArt++;
                        }
                    }

                    totalGen = totalVal + totalTVA;

                    tableBody += "</tbody>";

                    oReader.Close();
                    oReader.Dispose();

                    string htmlFile = "<html><body><style type='text/css'>" +
                    ".datagrid table    {       border-collapse: collapse;       text-align: left; width: 100%;    } " +
                    ".datagrid    {     font: normal 13px/150% Courier New, Courier, monospace; " +
                    " background: #fff;overflow: hidden;border: 1px solid #006699;-webkit-border-radius: 3px;" +
                    "-moz-border-radius: 3px; border-radius: 3px; } " +
                    ".datagrid table td, .datagrid table th { padding: 6px 6px; font-size: 15px; } " +
                    ".datagrid table thead th { background-color:#006699;color:#EECFA1; font-size: 14px;border-left: 1px solid #0070A8;}" +
                    ".datagrid table thead th:first-child{border: none;}" +
                    ".datagrid table tbody td {color:#008B45;border-left: 1px solid #E1EEF4;font-size: 15px;font-weight: normal;}" +
                    ".datagrid table tbody .alt td {background: #E1EEF4;color: #008B45;}" +
                    ".datagrid table tbody td:first-child{border-left: none;}" +
                    ".datagrid table tbody tr:last-child td{border-bottom: none;}" +
                    ".datagrid table tfoot td { background-color:#006699;color:#EECFA1; font-weight:bold; font-size: 14px;border-left: 1px solid #0070A8;padding: 2px;} " +
                    ".customText1{font-family: Verdana;font-weight: normal;font-size: 14px;color: #68838B;}" +
                    ".customText2{font-family: Candara;font-weight: normal;font-size: 14px;color: #4F94CD;}" +
                    ".customText3{font-family: Serif;font-weight: bold;font-size: 32px;color: #A66D33;}" +
                    ".customText4{font-family: 'Bookman Old Style', serif;font-weight: normal;font-size: 14px;color: #A66D33;}" +
                    ".customText5{font-family: 'Bookman Old Style', serif;font-weight: bold;font-size: 14px;color: #473C8B;}" +
                    ".customText6{font-family: Verdana;font-weight: normal;font-size: 14px;color: #008B45;} </style>" +
                    " <table align='center' width='90%'>" +
                    "<tr><td>" +
                    "</td></tr>" +
                    "<tr><td><table class='customText2'><tr><td>Sediu</td><td>" + adresaUnitLog + "</td>" +
                    "</tr><tr><td>Telefon</td><td>" + telUnitLog + "</td></tr>" +
                    "<tr><td>Fax</td><td>" + faxUnitLog + "</td></tr><tr><td>Contul</td><td>" + contUnitLog + "</td></tr><tr><td>" +
                    "Banca</td><td>" + bancaUnitLog + "</td></tr><tr><td>Cota TVA</td><td>19%</td></tr>" +
                    " </table>" +
                    " </td> " +
                    " <td align = 'left' valign='top'><table class='customText' ><tr><td align='center' class='customText3' colspan='2'>" +
                    "  Oferta</td></tr><tr><td class='customText4'>Nr.</td><td class='customText4'>" + nrComanda + "</td></tr><tr>" +
                    " <td class='customText4'>Din data</td><td class='customText4'>" + getCurrentDate() + "</td></tr><tr><td class='customText4'>" +
                    "  Valabila pana la</td><td class='customText4'>" + addDaysToCurrentDate(3) + "</td></tr></table></td>" +
                    "<td valign='top'><table class='customText2'  align='right'><tr><td>Client</td><td>" + numeClient + "</td>" +
                    "</tr><tr><td>Pers. de contact</td><td>" + persContact + "</td></tr><tr><td>Telefon</td><td>" + telContact + "</td></tr>" +
                    " </table></td></tr><tr><td colspan='3'><br><div class='datagrid'>" +
                    "<table ><thead><tr><th width='3%' align='center'>Nr.<br>crt</th><th width='30%' align='center'>Denumirea produselor sau a serviciilor</th>" +
                    "<th width='5%' align='center'>U.M.</th><th width='10%' align='center'>Cant.</th><th width='10%' align='center'>Pretul unitar</th>" +
                    "<th width='10%' align='center'>Valoarea</th>" +
                    "</tr><tr><th width='5%' align='center'>0</th><th width='30%' align='center'>1</th><th width='5%' align='center'>2</th><th width='10%' align='center'>3</th>" +
                    "<th width='10%' align='center'>4</th><th width='10%' align='center'>5(3x4)</th>" +
                    "</tr></thead>" + tableBody + "<tfoot><tr><td colspan='5' align='right' >Total:</td><td align='right'>" +
                    "" + String.Format("{0:0.00}", totalVal) + "</td>" +
                    "</tr></tfoot></table></div></td></tr><tr><td colspan='3'><table><tr><td class='customText5'><br>Reprezentant vanzari:" +
                    "</td></tr><tr><td class='customText6'>" + numeAgent + ", tel. " + telAgent + mail + "</td></tr></table>" +
                    "</td></tr></table>";


                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlFile, null, "text/html");

                    MailMessage message = new MailMessage();
                    message.From = new MailAddress("Oferta.materiale@arabesque.ro");
                    message.To.Add(new MailAddress(adresaMail));


                    message.Subject = "Oferta materiale si servicii SC Arabesque SRL";

                    message.AlternateViews.Add(htmlView);
                    SmtpClient client = new SmtpClient("mail.arabesque.ro");
                    client.Send(message);

                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }


            return "0";


        }


        private string getCurrentDate()
        {
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            return day + "." + month + "." + year;
        }


        private string addDaysToCurrentDate(int nrd)
        {
            DateTime dataMod = DateTime.Today.AddDays(nrd);
            string year = dataMod.Year.ToString();
            string day = dataMod.Day.ToString("00");
            string month = dataMod.Month.ToString("00");
            return day + "." + month + "." + year;

        }




    }
}