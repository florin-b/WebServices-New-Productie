using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1
{
    public class PierderiVanzariTotal
    {
        public string ul;
        public int nrClientiIstoric;
        public int nrClientiPrezent;
        public int nrClientiRest;
    }


    public class PierderiVanzariDep
    {
        public string codAgent;
        public string numeAgent;
        public int nrClientiIstoric;
        public int nrClientiPrezent;
        public int nrClientiRest;
    }


    public class PierderiVanzariAV
    {
        public List<PierderiAvHeader1> pierderiAvHeader;
        public List<PierderiTipClient> pierderiTipClient;
        public List<PierderiNivel1> pierderiNivel1;
    }

    public class PierderiAvHeader1
    {
        public string codTipClient;
        public string numeTipClient;
        public int nrClientiIstoric;
        public int nrClientiPrezent;
        public int nrClientiRest;

    }


    public class PierderiTipClient
    {
        public string codTipClient;
        public string numeClient;
        public double venitLC;
        public double venitLC1;
        public double venitLC2;
    }


    public class PierderiNivel1
    {
        public string numeClient;
        public string numeNivel1;
        public double venitLC;
        public double venitLC1;
        public double venitLC2;
    }
}