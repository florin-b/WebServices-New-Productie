using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;

namespace WebService1
{
    public class DatabaseConnections
    {

        public static string ConnectToProdEnvironment()
        {

            
            return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                  " (HOST = 10.1.3.76)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = PRD))); " +
                   " User Id = WEBSAP; Password = 2INTER7; ";
            

            /*
            return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                   " (HOST = 10.1.3.81)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = PRD))); " +
                   " User Id = WEBSAP; Password = 2INTER7; ";
                   */

        }

        static public string getUser()
        {
            return "USER_RFC";
        }

        static public string getPass()
        {
            return "2rfc7tes3";
        }

        public static void CloseConnections(OracleDataReader reader, OracleCommand command, OracleConnection connection)
        {

            try
            {

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                if (command != null)
                {
                    command.Dispose();
                }


                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }


            }
            catch (Exception)
            {

            }


        }

        public static void CloseConnections(OracleDataReader reader, OracleCommand command)
        {

            try
            {

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                if (command != null)
                {
                    command.Dispose();
                }


            }
            catch (Exception)
            {

            }


        }

        public static void CloseConnections(OracleCommand command, OracleConnection connection)
        {

            try
            {

                if (command != null)
                {
                    command.Dispose();
                }


                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }


            }
            catch (Exception)
            {

            }


        }


        public static OracleConnection createPRODConnection()
        {
            string connectionString = DatabaseConnections.ConnectToProdEnvironment();

            OracleConnection connection = new OracleConnection();
            connection.ConnectionString = connectionString;
            connection.Open();

            return connection;
        }


    }
}