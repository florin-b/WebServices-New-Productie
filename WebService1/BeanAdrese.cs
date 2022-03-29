using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1
{
    public class BeanAdreseJudet
    {
        public String listLocalitati;
        public String listStrazi;
    }


    public class Adresa
    {
        public string codJudet;
        public string localitate;
        public string strada;
        public string latitude;
        public string longitude;
        public string codAdresa;

        public override string ToString()
        {
            return "Adresa [codJudet=" + codJudet + ", localitate=" + localitate + ", strada=" + strada + ", latitude=" + latitude + ", longitude=" + longitude + ", codAdresa=" + codAdresa + "]";
        }


    }

    public class DateLivrareClient
    {
        public string codJudet;
        public string localitate;
        public string strada;
        public string nrStrada;
        public string numePersContact;
        public string telPersContact;
        public string termenPlata;

    }

    public class Localitate
    {
        public string localitate;
        public bool isOras;
        public int razaKm;
        public string coordonate;
    }

}