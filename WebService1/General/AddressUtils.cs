using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1.General
{
    public class AddressUtils
    {
        public static string getAdrUnitLog(string unitLog)
        {

           

            string retVal = "";
            switch (unitLog)
            {
                case "BC10":
                    retVal = "Str. Constantin Musat, nr. 1, cod 600092, Bacau#0334-401008#0334-401013#ING BANK#RO89INGB0008000005128917";
                    break;
                case "MM10":
                    retVal = "Str. Independentei, nr. 80, cod 430071, Baia Mare#0262-250155#0362-404931#ING BANK#RO60INGB0021000030428911";
                    break;
                case "BV10":
                    retVal = "Str. Bucegi, nr. 1, cod 500053, Brasov#0268-470171#0368-401147#UNICREDIT TIRIAC BANK#RO51BACX0000004568925021";
                    break;
                case "CJ10":
                    retVal = "Str. Calea Floresti, nr. 147-153, cod 400397, Cluj#0364-402345#0364-402347#ING BANK#RO59INGB0003000005128931";
                    break;
                case "CT10":
                    retVal = "B-dul Aurel Vlaicu, nr. 171, cod 900330, Constanta#0241-695303#0241-545850#ING BANK#RO88INGB0004000005128941";
                    break;
                case "DJ10":
                    retVal = "B-dul Decebal, nr. 111A, cod 200746, Craiova#0251-438597#0251-435196#ING BANK#RO22INGB0012000023928911";
                    break;
                case "VN10":
                    retVal = "B-dul Bucuresti, nr. 12, cod 620144, Focsani#0237-216110#0237-216111#UNICREDIT TIRIAC BANK#RO77BACX0000004568925038";
                    break;
                case "GL10":
                    retVal = "Str. Drumul de Centura, nr. 39, cod 800248, Galati#0236-416122#0336-401231#ING BANK#RO73INGB0010000005128911";
                    break;
                case "IS10":
                    retVal = "Comuna Miroslava, Sat Uricani, Trup Izolat, nr. 1, cod 707316, Iasi#0750-210102#0750-210222#ING BANK#RO59INGB0014000023928921";
                    break;
                case "NT10":
                    retVal = "Comuna Savinesti, Str. Uzinei, nr. 1, cod 610070, Piatra Neamt#0333-401004#0233-237326#ING BANK#RO12INGB0018000027028911";
                    break;
                case "BH10":
                    retVal = "Str. Calea Santandrei, nr. 3A, cod 410238, Oradea#0359-407630#0359-407631#ING BANK#RO02INGB0007000005128981";
                    break;
                case "AG10":
                    retVal = "Comuna Bradu, DN 65B, cod 117140, Pitesti#0348-457231#0348-445748#UNICREDIT TIRIAC BANK#RO07BACX0000004568925037";
                    break;
                case "PH10":
                    retVal = "Str. Poligonului, nr. 5, cod 100070, Ploiesti#0244-567312#0244-567312#ING BANK#RO47INGB0005009051289101";
                    break;
                case "MS10":
                    retVal = "Str. Depozitelor, nr. 26, cod 540240, Targu Mures#0750-211601#0365-430524#ING BANK#RO82INGB0011000032268911";
                    break;
                case "TM10":
                    retVal = "Str. Calea Sagului, nr. 205, cod 300517, Timisoara#0256-291023#0256-274745#UNICREDIT TIRIAC BANK#RO35BACX0000004568925018";
                    break;
                case "BU13":
                    retVal = "Soseaua Andronache, nr. 203, Sector 2, cod 022524, Bucuresti#021-2405591#021-2405591#BRD#RO61BRDE445SV44133934450";
                    break;
                case "BU10":
                    retVal = "Str. Drumul intre Tarlale, nr. 61A, sector 3, cod 032982, Bucuresti#031-8056632#031-8056633#UNICREDIT TIRIAC BANK#RO78BACX0000004568925020";
                    break;
                case "BU12":
                    retVal = "Str. Aleea Teisani, nr. 3-21, Sector 1, cod 014034, Bucuresti#031-4250255#031-4250253#UNICREDIT TIRIAC BANK#RO08BACX0000004568925019";
                    break;
                case "BU11":
                    retVal = "Str. Drumul Osiei, nr. 8-16, Sector 6, cod 062395, Bucuresti#021-3172196#021-3172154#UNICREDIT TIRIAC BANK#RO34BACX0000004568925036";
                    break;
                case "BZ10":
                    retVal = "Soseaua Nordului, nr. 12A, Buzau#0238 710 007#0238 710 020#UNICREDIT BANK#RO35 BACX 0000 0045 6892 5212";
                    break;
            }


            return retVal;

        }


        public static string getCurrentDate()
        {
            string mDate = "";
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            mDate = year + month + day;
            return mDate;
        }



       


        public static string getCurrentDate_YY_MM_DD()
        {
            string mDate = "";
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            mDate = year + "-" + month + "-" + day;
            return mDate;
        }

        public static string getCurrentDate_YYDDMM()
        {
            string mDate = "";
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            mDate = year + day + month;
            return mDate;
        }

        public static string getCurrentDate_YYMMDD()
        {
            string mDate = "";
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            mDate = year + month + day;
            return mDate;
        }


        public static string getMonthDate(int months)
        {
            string mDate = "";
            DateTime cDate = DateTime.Now.AddMonths(months);
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            mDate = year + month + day;
            return mDate;
        }


        public static string getStartMonthDate(int months)
        {
            string mDate = "";
            DateTime cDate = DateTime.Now.AddMonths(months);
            string year = cDate.Year.ToString();
            string day = "01";
            string month = cDate.Month.ToString("00");
            mDate = year + month + day;
            return mDate;
        }

        public static string getMonth(int months)
        {
            string mDate = "";
            DateTime cDate = DateTime.Now.AddMonths(months);
            string year = cDate.Year.ToString();
            string month = cDate.Month.ToString("00");
            mDate = year + month;
            return mDate;
        }


    }
}