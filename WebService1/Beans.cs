using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1
{


    public class DocumentCLP
    {
        public string nrDocument;
        public string numeClient;
        public string numeAgent;
        public string dataDocument;
        public string unitLog;
        public string nrDocumentSap;
        public string statusDocument;
        public string depozit;
        public string furnizor;
        public string observatii;
    }


    public class ObiectivKA
    {
        public string idObiectiv;
        public string numeObiectiv;
        public string dataObiectiv;
        public string statusObiectiv;
        public string orasObiectiv;
        public string stradaObiectiv;
        public string agentObiectiv;
    }

    public class FilialaTCLI
    {
        public string nume;
        public string werks;
        public List<string> depozite;
    }

    public class Depozit
    {
        public string name;
        public string vstel;
        public string lgort;
        public string werks;
    }


    public class Client
    {
        public string numeClient;
        public string codClient;
        public string tipClient;
        public string agenti;
        public string codAgent;
        public string numeAgent;
        public List<string> termenPlata;
        public string codCUI;
        public bool clientBlocat;
        public string tipPlata;
        public string codJudet;
        public string localitate;
        public string strada;
        public string diviziiClient;
    }

    public class ClientIP : Client
    {
        public string tipClientIP;
        public string filiala;
    }

    public class ClientAlocat
    {
        public string numeClient;
        public string tipClient01;
        public string tipClient02;
        public string tipClient03;
        public string tipClient04;
        public string tipClient05;
        public string tipClient06;
        public string tipClient07;
        public string tipClient08;
        public string tipClient09;

    }


    public class DetaliiClient
    {
        public string regiune;
        public string oras;
        public string strada;
        public string nrStrada;
        public double limitaCredit;
        public double restCredit;
        public string stare;
        public string persContact;
        public string telefon;
        public string filiala;
        public string motivBlocare;
        public string cursValutar;
        public string termenPlata;
        public string tipClient;
        public string isFurnizor;
        public string divizii;
        public string tipPlata;
        public string email;
    }




    public class Furnizor
    {
        public string numeFurnizor;
        public string codFurnizor;
    }

    public class FurnizorProduse
    {
        public string numeFurnizorProduse;
        public string codFurnizorProduse;
    }

    public class AdresaLivrareClient
    {
        public string oras;
        public string strada;
        public string nrStrada;
        public string codJudet;
        public string codAdresa;
        public string tonaj;
        public string coords;
        public bool isOras;
        public int razaKm;
        public string coordsCentru;
        public string codPostal;
    }


    public class ArticolVanzari
    {
        public string codArticol;
        public string numeArticol;
        public string cantitateArticol;
        public string valoareArticol;
    }


    public class ArticolComanda
    {
        public int nrCrt;
        public string numeArticol;
        public string codArticol;
        public string depozit;
        public double cantitate;
        public string um;
        public double pret;
        public string moneda;
        public double procent;
        public string observatii;
        public bool conditie;
        public int promotie;
        public double procentFact;
        public double pretUnit;
        public double discClient;
        public string tipAlert;
        public double procAprob;
        public double multiplu;
        public string infoArticol;
        public string cantUmb;
        public string Umb;
        public string alteValori;
        public string depart;
        public string tipArt;
        public double taxaVerde;
        public double pretUnitarPonderat;
        public double pretUnitarClient;
        public int ponderare;
        public string departAprob;
        public string filialaSite;
        public string istoricPret;
        public string valTransport;
        public string procTransport;
        public string listCabluri;
        public string tipTransport;
        public string greutate;
        public string dataExp;
        public string greutateBruta;
        public string lungimeArt;
        public string tipMarfa;
        public string cantitateInit;
        public double cantitate50;
        public string um50;

        public override string ToString()
        {
            return "ArticolComanda [nrCrt=" + nrCrt + ", numeArticol=" + numeArticol + ", codArticol=" + codArticol
                    + ", depozit=" + depozit + ", cantitate=" + cantitate + ", um=" + um + ", pret=" + pret + ", moneda="
                    + moneda + ", procent=" + procent + ", observatii=" + observatii + ", conditie=" + conditie
                    + ", promotie=" + promotie + ", procentFact=" + procentFact + ", pretUnit=" + pretUnit + ", discClient="
                    + discClient + ", tipAlert=" + tipAlert + ", procAprob=" + procAprob + ", multiplu=" + multiplu
                    + ", infoArticol=" + infoArticol + ", cantUmb=" + cantUmb + ", Umb=" + Umb + ", alteValori="
                    + alteValori + ", depart=" + depart + ", tipArt=" + tipArt + ", taxaVerde=" + taxaVerde
                    + ", pretUnitarPonderat=" + pretUnitarPonderat + ", pretUnitarClient=" + pretUnitarClient
                    + ", ponderare=" + ponderare + "]";
        }



    }


    public class ArticolComandaRap : ArticolComanda
    {
        public string status;
        public double cmp;
        public double discountAg;
        public double discountSd;
        public double discountDv;
        public string permitSubCmp;
        public string unitLogAlt;
        public string pretMediu;
        public string adaosMediu;
        public string unitMasPretMediu;
        public string departSintetic;
        public string coefCorectie;
        public string vechime;
        public double procT1 = 0;
        public double valT1 = 0;
        public string sintetic;
        public double lungime = 0;
        public string umPalet;
        public double aczcDeLivrat = 0;
        public double aczcLivrat = 0;
    }




    public class ComandaVanzare
    {
        public string codClient;
        public string numeClient;
        public string persoanaContact;
        public string telefon;
        public string cantarire;
        public string metodaPlata;
        public string tipTransport;
        public string comandaBlocata;
        public string nrCmdSap;
        public string alerteKA;
        public string factRedSeparat;
        public string filialaAlternativa;
        public string userSite;
        public string userSiteMail;
        public string isValIncModif;
        public string codJ;
        public string adresaLivrareGed;
        public string adresaLivrare;
        public string valoareIncasare;
        public string conditieID;
        public string cnpClient;
        public string canalDistrib;
        public string necesarAprobariCV;
        public string valTransportSap;
        public string parrentId;

    }

    public class DateLivrare
    {

        public string codJudet = "";
        public string numeJudet = "";
        public string Oras = "";
        public string Strada = "";
        public string persContact = "";
        public string nrTel = "";
        public string redSeparat = "R";
        public string Cantar = "";
        public string tipPlata = "";
        public string Transport = "";
        public string dateLivrare = "";
        public string termenPlata = "";
        public string obsLivrare = "";
        
        public string dataLivrare = "";
        public bool adrLivrNoua = false;
        public string tipDocInsotitor = "1";
        public string obsPlata = " ";
        public string addrNumber = " ";
        public string valoareIncasare = "0";
        public bool isValIncModif = false;
        public string mail = " ";
        public string totalComanda;
        public string unitLog;
        public string codAgent;
        public string factRed;
        public string macara;
        public string idObiectiv;
        public bool isAdresaObiectiv;
        public string coordonateGps;
        public string tonaj;
        public string prelucrare;
        public string clientRaft;
        public string meserias = "";
        public bool factPaletiSeparat;
        public string furnizorMarfa;
        public string furnizorProduse;
        public bool isCamionDescoperit;
        public string diviziiClient;
        public string codSuperAgent;
        public string programLivrare;
        public string livrareSambata;
        public string blocScara;
        public string filialaCLP;
        public string numeDelegat = "";
        public string ciDelegat = "";
        public string autoDelegat = "";
        public string refClient = "";
        public string costTransportMathaus;
        public bool isComandaACZC;
        public string tipPersAgent;
        public string filialaPlata;
        public string codPostal;
        public string isComandaCustodie;
    }


    public class DateLivrareCmd : DateLivrare
    {
        public string tipPersClient;
        public string adresaD;
        public string orasD;
        public string codJudetD;
        public string numeClient;
        public string cnpClient;
        public double marjaT1 = 0;
        public double procentT1 = 0;
        public double marjaT1Tot = 0;
        public double procentT1Tot = 0;
        public double mCantCmd = 0;
        public double mCant30 = 0;
        public double marjaBrutaPalVal = 0;
        public double marjaBrutaCantVal = 0;
        public double marjaBrutaPalProc = 0;
        public double marjaBrutaCantProc = 0;
        public bool isClientBlocat = false;
        public double limitaCredit = 0;
        public string nrCmdClp;
        public double marjaBruta = 0;
        public double procMarjaBruta = 0;
        public double marjaBrutaTot = 0;
        public double procMarjaBrutaTot = 0;

    }





    public class FacturaNeincasata
    {
        public string numeClient;
        public string codClient;
        public string referinta;
        public string emitere;
        public string scadenta;
        public string valoare;
        public string incasat;
        public string rest;
        public string acoperit;
        public string scadentaBO;
        public string tipPlata;
        public string numeAgent;

    }

    public class ClientInactiv
    {
        public string numeClient;
        public string codClient;
        public string codAgent;
        public string stareClient;
        public string tipClient;
    }


    public class AdresaLivrareCV
    {
        public string codJudet;
        public string oras;
        public string strada;
    }


    public class InfoVenituri
    {
        public string id;
        public string venitNetP;
        public string mP;
        public string venitNetP040;
        public string mP040;
        public string venitNetP041;
        public string mP041;
    }


    public class Comanda
    {
        public string idComanda;
        public string numeClient;
        public string codClient;
        public string dataComanda;
        public string sumaComanda;
        public string stareComanda;
        public string monedaComanda;
        public string sumaTVA;
        public string monedaTVA;
        public string cmdSap;
        public string tipClient;
        public string divizieAgent;
        public string canalDistrib;
        public string filiala;
        public string factRed;
        public string accept1;
        public string accept2;
        public string numeAgent;
        public string termenPlata;
        public string cursValutar;
        public string docInsotitor;
        public string adresaNoua;
        public string adresaLivrare;
        public string divizieComanda;
        public string pondere30;
        public string aprobariNecesare;
        public string aprobariPrimite;
        public string codClientGenericGed;
        public string conditiiImpuse;
        public string telAgent;
        public string avans;
        public string clientRaft;
        public string tipComanda;
        public string isCmdInstPublica;
        public double bazaSalariala = 0;
        public string tipClientInstPublica;
        public bool isAprobatDistrib;
        public bool isComandaACZC;

    }

    public class DateClientInstPublica
    {
        public bool isClientInstPublica;
        public string tipClientInstPublica;
    }

    public class AdresaGpsClient
    {
        public string codClient;
        public string codAgent;
        public string tipLocatie;
        public string dateGps;
        public string data;
        public string ora;
        public string adresa;
    }



    public class Clienti
    {
        public string codClient;
        public string numeClient;
        public string tipClient;

    }

    public class MaterialNecesar
    {
        public string codArticol;
        public string numeArticol;
        public string codSintetic;
        public string numeSintetic;
        public string consum30;
        public string stoc;
        public string propunereNecesar;
        public string CA;
        public string interval1;
        public string interval2;
        public string interval3;
    }


    public class Agent
    {
        public string nume;
        public string cod;
    }


    public class VanzariAgentiParam
    {
        public string tipArticol;
        public string agent;
        public string filiala;
        public string departament;
        public string startInterval;
        public string stopInterval;
        public string tipComanda;
        public string articole;
        public string clienti;
        public string tipUserSap;
    }


    public class ArticolCautare
    {
        public string cod;
        public string nume;
        public string sintetic;
        public string nivel1;
        public string umVanz;
        public string umVanz10;
        public string tipAB;
        public string depart;
        public string departAprob;
        public string umPalet;
        public string stoc;
        public string categorie;
        public string lungime;

    }

    public class GreutateArticol
    {
        public string codArticol;
        public double greutate;
        public string um;
        public string umCantitate;
    }

    public class Header
    {
        public string text1;
        public string text2;
    }

    public class Details
    {
        public string text1;
        public string text2;
    }

    public class ComandaCLP
    {
        public DateLivrareCLP dateLivrare;
        public List<ArticolCLP> articole;
    }


    public class DateLivrareCLP
    {
        public string persContact;
        public string telefon;
        public string adrLivrare;
        public string oras;
        public string codJudet;
        public string data;
        public string tipMarfa;
        public string masa;
        public string tipCamion;
        public string tipIncarcare;
        public string tipPlata;
        public string mijlocTransport;
        public string aprobatOC;
        public string deSters;
        public string statusAprov;
        public string valComanda;
        public string obsComanda;
        public string valTransp;
        public string procTransp;
        public string acceptDV;
        public string dataIncarcare;
        public string nrCT;

    }

    public class ComandaAprobata : ComandaVanzare
    {
        public string oras;
        public string codJudet;
        public string termenPlata;
        public string unitLog;
        public string obsLivrare;
        public string tipPersClient;
        public string articole;
        public string conditiiComanda;
    }

    public class ArticolCLP
    {
        public string cod;
        public string nume;
        public string cantitate;
        public string umBaza;
        public string depozit;
        public string status;
    }

    public class ArticolFurnizor : ArticolVanzari
    {
        public string umAprov;
    }


    public class Conditii
    {
        public ConditiiHeader header;
        public List<ConditiiArticole> articole;
    }


    public class ConditiiPrimite
    {
        public string header;
        public string articole;
    }

    public class ConditiiHeader
    {
        public int id;
        public double conditiiCalit;
        public int nrFact;
        public string observatii;
        public string codAgent;

    }

    public class ConditiiArticole
    {
        public string cod;
        public string nume;
        public double cantitate;
        public string um;
        public double valoare;
        public double multiplu;

    }


    public class ArticolComandaAfis
    {
        public DateLivrareCmd dateLivrare;
        public List<ArticolComandaRap> articoleComanda;
        public Conditii conditii;
    }


    public class DateReturClient
    {
        public string listaDocumente;
        public string adreseLivrare;
        public string persoaneContact;
    }


    public class DocumentRetur
    {
        public string numar;
        public string data;
        public string tipTransport;
        public string dataLivrare;
        public string extraDate;
        public bool isCmdACZC;
    }

    public class StareDocumentRetur
    {
        public string nrDocument;
        public string stare;
    }

    public class ExtraDate
    {
        public string codJudet;
        public string localitate;
        public string strada;
        public string codAdresa;
        public string telContact;
        public string numeContact;
    }


    public class ComandaReturAfis
    {
        public string id;
        public string nrDocument;
        public string numeClient;
        public string dataCreare;
        public string status;
        public string numeAgent;
    }



    public class ComandaRetur
    {
        public string id;
        public string nrDocument;
        public string codClient;
        public string numeClient;
        public string dataLivrare;
        public string tipTransport;
        public string codAgent;
        public string tipAgent;
        public string motivRetur;
        public string numePersContact;
        public string telPersContact;
        public string adresaCodJudet;
        public string adresaOras;
        public string adresaStrada;
        public string adresaCodAdresa;
        public string listaArticole;
        public string observatii;
        public string dataCreare;
        public string status;
        public string transpBack;
        public string inlocuire;
        public string listStari;
    }

    public class ArticolRetur
    {
        public string nume;
        public string cod;
        public string cantitate;
        public string um;
        public string cantitateRetur;
        public string pretUnitPalet;
        public string motivRespingere;
        public bool inlocuire;
        public string pozeArticol;
    }

    public class PozaArticol
    {
        public string nume;
        public string strData;
    }

    public class MotivRespingereRetur
    {
        public string cod;
        public string nume;
    }

    public class PersoanaContact
    {
        public string nume;
        public string telefon;
    }


    public class ConditiiComanda
    {
        public string condCalitative;
        public string nrFacturi;
        public string observatii;
        public string articole;

    }



    public class PretArticolGed
    {
        public string pret;
        public string um;
        public string faraDiscount;
        public string codArticolPromo;
        public string cantitateArticolPromo;
        public string pretArticolPromo;
        public string umArticolPromo;
        public string pretLista;
        public string cantitate;
        public string conditiiPret;
        public string multiplu;
        public string cantitateUmBaza;
        public string umBaza;
        public string cmp;
        public string pretMediu;
        public string adaosMediu;
        public string umPretMediu;
        public string coefCorectie;
        public string procTransport;
        public string discMaxAV;
        public string discMaxSD;
        public string discMaxDV;
        public string discMaxKA;
        public string impachetare;
        public string istoricPret;
        public string valTrap;
        public string errMsg;
        public string procReducereCmp;
        public string pretFaraTva;
        public string greutate;
        public string dataExp;
        public string articoleRecomandate;
        public string tipMarfa;
        public string greutateBruta;
        public string lungime;
        public string um50;
        public string cantitate50;


    }

    public class FacturaNeincasataLite
    {
        public string nrFactura;
        public string dataEmitere;
        public string anDocument;
        public string restPlata;
        public string nrDocument;
    }


    public class DeviceInfo
    {
        public string sdkVer;
        public string man;
        public string model;
        public string appName;
        public string appVer;
    }

    public class Articol
    {
        public string cod;
        public string nume;
    }

    public class ArticolProps
    {
        public string tipMarfa;
        public string lungime;

        public ArticolProps()
        {

        }

        public ArticolProps(string tipMarfa, string lungime)
        {
            this.tipMarfa = tipMarfa;
            this.lungime = lungime;
        }
    }

}