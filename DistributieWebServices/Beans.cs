using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributieWebServices
{

    public class Borderouri
    {
        public string numarBorderou;
        public string dataEmiterii;
        public string evenimentBorderou;
        public string tipBorderou;
        public string bordParent;
        public string agentDTI;
        public string nrAuto;
        public string codSofer;

    }


    public class FacturiBorderou
    {
        public string codFurnizor;
        public string numeFurnizor;
        public string adresaFurnizor;
        public string codAdresaFurnizor;
        public string sosireFurnizor;
        public string plecareFurnizor;


        public string codClient;
        public string numeClient;
        public string adresaClient;
        public string codAdresaClient;
        public string sosireClient;
        public string plecareClient;

        public string dataStartCursa;

        public string pozitie;
        public string nrFactura;

    }


    public class EvenimentBorderou
    {
        public string numeClient;
        public string codClient;
        public string codAdresa;
        public string adresa;
        public string oraStartCursa;
        public string kmStartCursa;
        public string oraSosireClient;
        public string kmSosireClient;
        public string oraPlecare;
    }


    public class Eveniment
    {
        public string eveniment;
        public string data;
        public string ora;
        public string distantaKM;
    }


    public class EvenimentNou
    {
        public string codSofer;
        public string document;
        public string client;
        public string codAdresa;
        public string eveniment;
        public string truckData;
        public string tipEveniment;
        public string data;
        public string ora;
        public string bordParent;
        public string evBord;

        public override string ToString()
        {
            return "EvenimentNou: [ codSofer " + codSofer + ", document " + document + ", client " + client + ", codAdresa " + codAdresa + " , eveniment " + eveniment + " , evBord " + evBord + " ]";
        }


    }


    public class ArticoleFactura
    {
        public string nume;
        public string cantitate;
        public string umCant;
        public string tipOperatiune;
        public string departament;
        public string greutate;
        public string umGreutate;
    }


    public class ArticoleBorderou
    {
        public string nume;
        public string cantitate;
        public string um;
    }

    public class User
    {
        public string status;
        public string nume;
        public string id;
        public string filiala;
        public string departament;
        public string tipAcces;
        public InitStatus initStatus;
        public string dti;

    }

    public class InitStatus
    {
        public string document;
        public string client;
        public string eveniment;
        public string tipDocument;
    }


    public class EvenimentStop
    {
        public string idEveniment;
        public string codSofer;
        public string codBorderou;
        public string codEveniment;
    }

    public class EvenimentStopIncarcare
    {
        public string document;
        public string codSofer;
        public string data;
        public string ora;
    }

    public class Etapa
    {
        public string borderou;
        public string client;
        public string codAdresa;
        public string pozitie;
        public string document;
    }

    public class NotificareClient
    {

        public string codClient;
        public string nrTelefon;
        public DateComanda dateComanda;
        public string codAdresa;
        public string poz;

        public override bool Equals(object obj)
        {
            NotificareClient em = (NotificareClient)obj;
            return this.nrTelefon == em.nrTelefon
                && dateComanda.emitere == em.dateComanda.emitere
                && dateComanda.departament == em.dateComanda.departament;

        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 23;

                hash = nrTelefon == null ? hash * 11 : hash * 17 + nrTelefon.GetHashCode();
                hash = dateComanda.emitere == null ? hash * 21 : hash * 17 + dateComanda.emitere.GetHashCode();
                hash = dateComanda.departament == null ? hash * 23 : hash * 17 + dateComanda.departament.GetHashCode();
                return hash;
            }
        }


        public override string ToString()
        {
            return "NotificareClient: [ codClient " + codClient + ", nrTelefon " + nrTelefon + ", dateComanda " + dateComanda.ToString() + ", codAdresa " + codAdresa + " ]";
        }
    }



    public class DateComanda
    {
        
        public string emitere;
        public string departament;

        public override string ToString()
        {
            return "DateComanda: [ emitere " + emitere + ", departament " + departament + " ]";
        }

    }


    public class Sofer
    {
        public string nume;
        public string filiala;
        public string codTableta;
    }


    public class StareMasina
    {
        public String kmGps;
        public String kmDeclarati;
    }

    public class StareValidareKm
    {
        public bool isKmValid;
        public string statusMsg;
        public int statusId;
    }

    public class FoaieParcursItem
    {
        public string id;
        public string locatie;
        public string data;
        public string ora;
        public string minut;
        public string km;
        public string status;
    }

    public class FoaieParcurs
    {
        public string nrBorderou;
        public List<FoaieParcursItem> items;
    }

    public class Alimentare
    {
        public string id;
        public string nrAuto;
        public string codSofer;
        public string data;
        public string litri;
        public string kmBord;
    }

    public class AfisFoaieParcurs
    {
        public FoaieParcurs foaieParcurs;
        public string kmStart;
        public string kmStop;
        public List<Alimentare> alimentariComb;
        public List<Alimentare> consumSiroco;
    }

    public class DeviceInfo
    {
        public string sdkVer;
        public string man;
        public string model;
        public string appName;
        public string appVer;
    }
}