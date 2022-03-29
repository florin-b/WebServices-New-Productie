using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace WebService1.General
{
    public class GeneralUtils
    {
        public static string serializeObject(Object customObject)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(customObject);
        }


        public static DateTime getDateFromString(string strDate)
        {
            return DateTime.ParseExact(strDate, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string formatStrDate(string strDate)
        {
            DateTime dt = DateTime.ParseExact(strDate, "yyyymmdd", System.Globalization.CultureInfo.InvariantCulture);
            return dt.ToString("dd.mm.yyyy");

        }

        public static string addDays(string date, int nrDays)
        {
            return Convert.ToDateTime(date).AddDays(nrDays).ToString("dd MMMM yyyy", CultureInfo.CreateSpecificCulture("ro"));
        }

        public static long getCurrentMillis()
        {
            DateTime Jan1St2016 = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)((DateTime.UtcNow - Jan1St2016).TotalMilliseconds);
        }

        public static string formatIstoricPret(string istoricPret)
        {

            if (istoricPret == null)
                return " ";

            if (istoricPret.Trim().Length == 0)
                return " ";

            if (istoricPret.Contains("#"))
                return istoricPret.Replace("#", "\n");

            return istoricPret;
        }


        public static int getLineNumber(Exception ex)
        {
            var lineNumber = 0;
            const string lineSearch = ":line ";
            var index = ex.StackTrace.LastIndexOf(lineSearch);
            if (index != -1)
            {
                var lineNumberText = ex.StackTrace.Substring(index + lineSearch.Length);
                if (int.TryParse(lineNumberText, out lineNumber))
                {
                }
            }
            return lineNumber;
        }


        public static string getUniqueIdFromCode(string codAgent)
        {
            DateTime cDate = DateTime.Now;
            string nowTime = cDate.Hour.ToString("00") + cDate.Minute.ToString("00") + cDate.Second.ToString("00");
            string substrCodAgent = codAgent.Substring(3, 5);
            string uniqueId = nowTime + substrCodAgent;
            return uniqueId;

        }

        public static bool isFilialaExtensie02(string filiala)
        {
            string fil = filiala.ToUpper();

            return fil.Equals("GALATI") || fil.Equals("IASI") || fil.Contains("BAIA") || fil.StartsWith("GL") || fil.StartsWith("IS") || fil.StartsWith("MM");

        }

        public static string getTipPlata(string tipPlata)
        {

            if (tipPlata.ToUpper().Equals("ING") || tipPlata.ToUpper().Equals("BRD") || tipPlata.ToUpper().Equals("UNI") || tipPlata.ToUpper().Equals("CBTR") || tipPlata.ToUpper().Equals("CGRB") || tipPlata.ToUpper().Equals("CRFZ") || tipPlata.ToUpper().Equals("CCTL") || tipPlata.ToUpper().Equals("CAVJ") || tipPlata.ToUpper().Equals("INS"))
                return "CB";
            else
                return tipPlata;
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

    }
}