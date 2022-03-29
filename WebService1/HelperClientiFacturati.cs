using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1
{
    public class ClientFacturat
    {
        public string nume;
        public string cod;
        public string luna1;
        public string luna2;
        public string luna3;
        public string luna4;
        public string luna5;
        public string luna6;
        public string luna7;
    }


    public class DetaliiFactura
    {
        public string nrComanda;
        public string dataEmitere;
        public string valoare;
    }
}