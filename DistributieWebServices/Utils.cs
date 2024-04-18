using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributieWebServices
{
    public class Utils
    {

        public static string getDepartName(String departCode)
        {
            string retVal = "";

            if (departCode.Equals("01"))
                retVal = "lemnoase";

            if (departCode.Equals("02"))
                retVal = "feronerie";

            if (departCode.Equals("03"))
                retVal = "parchet";

            if (departCode.Equals("04") || departCode.Equals("040") || departCode.Equals("041"))
                retVal = "materiale grele";

            if (departCode.Equals("05"))
                retVal = "electrice";

            if (departCode.Equals("06"))
                retVal = "gips";

            if (departCode.Equals("07"))
                retVal = "chimice";

            if (departCode.Equals("08"))
                retVal = "instalatii";

            if (departCode.Equals("09"))
                retVal = "hidroizolatii";

            if (departCode.Equals("11"))
                retVal = "magazin";

            return retVal;
        }

        public static bool itsTimeToSendSmsAlert()
        {

            DateTime t1 = DateTime.Now;

            DateTime t2 = Convert.ToDateTime("07:00:00 AM");

            DateTime t3 = Convert.ToDateTime("04:30:00 PM");

            int i = DateTime.Compare(t1, t2);

            int i1 = DateTime.Compare(t1, t3);

            if (i < 0 || i1 > 0)
                return false;


            return true;
        }


        public static string formatDateFromSap(string strData)
        {
            return strData.Substring(6, 2) + "." + strData.Substring(4, 2) + "." + strData.Substring(0, 4);
        }

        public static bool itsProgramFinished()
        {
            DateTime t1 = DateTime.Now;

            DateTime t3 = Convert.ToDateTime("04:30:00 PM");

            int i1 = DateTime.Compare(t1, t3);

            if (i1 > 0)
                return true;

            return false;
        }


        public static string getUser()
        {
            return "USER_RFC";
        }

        public static string getPass()
        {
            return "2rfc7tes3";
        }


        public static string getCurrentDate()
        {
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            string nowDate = year + month + day;
            return nowDate;
        }


        public static string getCurrentTime()
        {
            DateTime cDate = DateTime.Now;
            string hour = cDate.Hour.ToString("00");
            string minute = cDate.Minute.ToString("00");
            string sec = cDate.Second.ToString("00");
            string nowTime = hour + minute + sec;
            return nowTime;
        }

        public static string getCleanStrada(string numeStrada)
        {
            return numeStrada.ToLower().Replace("piata", "").Replace("strada", "").Replace("str", "").Replace("str.", "").Replace("bulevardul", "")
                .Replace("b-dul", "").Replace("blvd", "").Replace("calea", "").Replace("intrarea", "").Replace("aleea", "");
        }

    }
}