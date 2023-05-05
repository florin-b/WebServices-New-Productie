using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1
{
    public class BeanClientSemiactiv
    {

        public string numeClient;
        public string codClient;
        public string judet;
        public string localitate;
        public string strada;
        public string numePersContact;
        public string telPersContact;
        public string vanzMedie;
        public string vanz03;
        public string vanz06;
        public string vanz07;
        public string vanz09;
        public string vanz040;
        public string vanz041;


    }

    public class BeanDatePersonale
    {
        public string cnp;
        public string nume;
        public string codjudet;
        public string localitate;
        public string strada;
        public List<string> termenPlata;
        public bool clientBlocat;
        public string tipPlata;
        public string codClient;
    }

    public class CreditClient
    {
        public double limitaCredit;
        public double restCredit;
        public bool isBlocat;
        public string motivBlocat;
    }
}