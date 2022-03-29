using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlotaService
{
    public class Sofer
    {
        public string nume;
        public string cod;
    }

    public class Masina
    {
        public string nrAuto;
        public string deviceId;
    }


    public class ActivitateBorderou
    {
        public string nr;
        public string data;
        public string km;
        public string durata;
    }

    public class DetaliiBorderou
    {
        public string client;
        public string oraSosire;
        public string locatieSosire;
        public string oraPlecare;
        public string locatiePlecare;
        public string durataStationare;
        public string distanta;

    }


    public class TabletaSofer
    {
        public string idTableta;
        public string dataInreg;
        public string stare;
    }


    public class PozitieActualaMasina
    {
        public string deviceId;
        public string codSofer;
        public string nrAuto;
        public string latitudine;
        public string longitudine;
        public string data;
        public string viteza;
    }

    public class Borderou
    {
        public string cod;
        public string dataEmitere;
    }

    public class PozitieClient
    {
        public string codClient;
        public string numeClient;
        public string latitudine;
        public string longitudine;
    }

    public class TraseuBorderou
    {
        public string dataInreg;
        public string latitudine;
        public string longitudine;
        public string km;
        public string viteza;

    }



}