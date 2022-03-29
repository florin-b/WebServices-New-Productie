using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;

namespace FlotaService
{
    public class ErrorHandling
    {

        public static void sendErrorToMail(string errMsg)
        {

            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress("Android.WebService@arabesque.ro");
                message.To.Add(new MailAddress("florin.brasoveanu@arabesque.ro"));
                message.Subject = "Android WebService Error";
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