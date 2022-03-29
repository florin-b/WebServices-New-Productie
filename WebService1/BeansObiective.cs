using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1
{

    public class ObiectivGeneral
    {
        public string id;
        public string codAgent;
        public string numeAgent;
        public string unitLog;
        public string dataCreare;
        public string nrObiectiv;
        public string numeObiectiv;
        public string codStadiuObiectiv;
        public string codMotivSuspendare;
        public string adresaObiectiv;
        public string numeBeneficiar;
        public string numeAntreprenorGeneral;
        public string codAntreprenorGeneral;
        public string numeArhitect;
        public string codCategorieObiectiv;
        public string valoareObiectiv;
        public string nrAutorizatieConstructie;
        public string dataEmitereAutorizatie;
        public string dataExpirareAutorizatie;
        public string primariaEmitenta;
        public string valoareFundatie;
        public string stadii;
        public string constructori;
        public string inchis;
        public string evenimente;

    }


    public class ObiectivConstructor
    {
        public string codClient;
        public string codDepart;
        public string numeClient;
        public string stare;

    }

    public class ObiectivStadii
    {
        public string codDepart;
        public string codStadiu;

    }

    public class BeanObiectiv
    {
        public string id;
        public string nume;
        public string data;
        public string beneficiar;
        public string codStatus;
        public string adresa;
        public string numeAgent;
    }


    public class Eveniment
    {
        public string idObiectiv;
        public string codClient;
        public string codEveniment;
        public string codDepart;
        public string data;
        public string observatii;
    }


    public class ObiectivDepartament
    {
        public string id;
        public string nume;
        public string beneficiar;
        public string dataCreare;
        public string adresa;

    }

    public class ObiectivConsilier
    {

        public string id;
        public string beneficiar;
        public string dataCreare;
        public string adresa;
        public string codJudet;
        public string coordGps;


    }

}