using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1
{
    public class ComandaActiva
    {
        public string idCmdSap;
        public string codClient;
        public string numeClient;
        public string valoare;
        public string localitate;
        public string strada;
        public string codBorderou;
        public string nrMasina;
        public string codJudet;
        public string numeSofer;
        public string telSofer;
        public string stareComanda;
        public string telClient;

    }


    public class ClientBorderou
    {
        public string codClient;
        public string pozitie;
        public string dataEveniment;
    }


    public class ClientComanda
    {
        public string codClient;
        public string idComandaSap;
        public string codAdresa;

        public override string ToString()
        {
            return "ClientComanda [codClient=" + codClient + ", idComandaSap=" + idComandaSap + ", codAdresa=" + codAdresa + "]";
        }
    }

}