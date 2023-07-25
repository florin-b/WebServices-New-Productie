using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1
{
    public class ParametruPretGed
    {
        public string client;
        public string articol;
        public string cantitate;
        public string depart;
        public string um;
        public string ul;
        public string depoz;
        public string codUser;
        public string canalDistrib;
        public string tipUser;
        public string metodaPlata;
        public string termenPlata;
        public string codJudet;
        public string localitate;
        public string filialaAlternativa;
        public string codClientParavan;
        public string filialaClp;
        public string tipTransport;
    }

    public class ArticolPretTransportGed
    {
        public string cod;
        public string promo;
        public string valoare;
    }

    public class ArticolStoc
    {
        public string cod;
        public string depozit;
        public string unitLog;
        public string depart;
        public double stoc;
        public string um;
    }
}