using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TiparireDocumente
{
    public class UserInfo
    {
        public string logonStatus;
        public string departament;
        public string filiala;
        public string tipAcces;
        public string codUser;
        public string numeUser;
        public string depozit;
        public string extraDep;

    }


    public class Document
    {
        public string id;
        public string client;
        public string emitere;
        public string codArticol;
        public string numeArticol;
        public string pozitieArticol;
        public string cantitate;
        public string um;
        public string isPregatit;
        public string isTiparit;
        public string tip;
        public string depozit;
        public string numeSofer;
        public string nrMasina;
        public string modificare;
        public string cantitateModificata;
        public string infoStatus;
        public string comandaVeche;
        public string tipTransport;
        public string cantFractie = "0";
        public string nrPaleti = "0";
        public string umPaleti ="PL1";
        public string palBuc = "0";
    }



    public class DocumentTiparit
    {
        public String id;
        public String dataEmitere;
        public String client;
        public String departament;
        public String filiala;
        public String seTipareste;
    }


    public class Livrare
    {
        public string nrComanda;
        public string nrLivrare;
        public string numeClient;
        public string emitere;
        public string pregatire;
        public string tiparit;
        public List<Articol> listArticole;
    }


    public class Articol
    {
        public string numeArticol;
        public string codArticol;
        public double cantitate;
        public string um;
        public string poz;
        public string depozit;
        public string transport;
        
    }


}