using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1
{
    public class CompanieConcurenta
    {
        public string cod;
        public string nume;
    }

    public class PretConcurenta
    {
        public string articol;
        public string valoare;
        public string data;
    }

    public class ArticolConcurenta
    {
        public string cod;
        public string nume;
        public string umVanz;
        public string valoare;
        public string dataValoare;
        public string observatii;

    }


    public class NewPretConcurenta
    {
        public string cod;
        public string concurent;
        public string valoare;
        public string observatii;
    }
}