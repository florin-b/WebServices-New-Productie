using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1
{
    public class FtpHelper
    {

        public static string getLocalFtpIp(string filiala)
        {
            string ftpIp = "0.0.0.0";

            switch (filiala)
            {
                case "ANDRONACHE":
                    ftpIp = "10.2.8.1";
                    break;
                case "GLINA":
                    ftpIp = "10.2.32.1";
                    break;
                case "MILITARI":
                    ftpIp = "10.2.0.1";
                    break;
                case "OTOPENI":
                    ftpIp = "10.2.40.1";
                    break;
                case "BACAU":
                    ftpIp = "10.9.16.1";
                    break;
                case "BAIA":
                    ftpIp = "10.14.0.1";
                    break;
                case "BRASOV":
                    ftpIp = "10.3.0.1";
                    break;
                case "CLUJ":
                    ftpIp = "10.5.0.1";
                    break;
                case "CONSTANTA":
                    ftpIp = "10.8.0.1";
                    break;
                case "CRAIOVA":
                    ftpIp = "10.4.0.1";
                    break;
                case "FOCSANI":
                    ftpIp = "10.17.0.1";
                    break;
                case "GALATI":
                    ftpIp = "10.1.8.1";
                    break;
                case "IASI":
                    //ftpIp = "10.11.8.1";
                    ftpIp = "0.0.0.0";
                    break;
                case "MURES":
                    ftpIp = "10.16.0.1";
                    break;
                case "ORADEA":
                    ftpIp = "10.10.0.1";
                    break;
                case "PIATRA":
                    ftpIp = "10.7.8.1";
                    break;
                case "PITESTI":
                    ftpIp = "10.15.0.1";
                    break;
                case "PLOIESTI":
                    ftpIp = "10.12.0.1";
                    break;
                case "TIMISOARA":
                    ftpIp = "10.6.0.1";
                    break;
                case "DEVA":
                    ftpIp = "10.20.0.1";
                    break;
                case "BUZAU":
                    ftpIp = "10.19.0.1";
                    break;
                case "SIBIU":
                    ftpIp = "10.21.0.1";
                    break;


            }


            return ftpIp;
        }


    }
}