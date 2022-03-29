using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlotaService
{
    public class DatabaseConnections
    {

        static public string ConnectToTestEnvironment()
        {

            //TES
            return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                    " (HOST = 10.1.3.89)(PORT = 1527)))(CONNECT_DATA = (SERVICE_NAME = TES))); " +
                    " User Id = WEBSAP; Password = 2INTER7; ";

        }

        static public string ConnectToProdEnvironment()
        {

            //PRD

            return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                   " (HOST = 10.1.3.95)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = TABLET) )); " +
                   " User Id = WEBSAP; Password = 2INTER7;";

        }



    }
}