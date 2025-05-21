using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1
{
    public class CategorieMathaus
    {
        public string cod;
        public string nume;
        public string codHybris;
        public string codParinte;

    }


    public class ArticolMathaus : ArticolCautare
    {
        public string adresaImg;
        public string adresaImgMare;
        public string descriere;
        public string catMathaus;
        public string pretUnitar;
        public string tip1;
        public string tip2;
        public bool isLocal;
        public bool isArticolSite;
        public string planificator;
    }

    public class RezultatArtMathaus
    {
        public string nrTotalArticole;
        public List<ArticolMathaus> listArticole;
    }

    public class AntetCmdMathaus
    {
        public string localitate;
        public string codJudet;
        public string codClient;
        public string tipPers;
        public string depart;
        public string codPers;
        public string tipTransp;
        public string camionDescoperit;
        public string macara;
        public string tipCamion;
        public string greutateComanda;
        public string tipComandaCamion;
        public string isComandaDL;
        public string nrCmdSap;
        public string strada;
        public string codFurnizor;

    }

    public class DateTransportMathaus
    {
        public List<CostTransportMathaus> listCostTransport;
        public List<DepozitArticolTransport> listDepozite;
        public List<TaxaMasina> taxeMasini;
        public List<ArticolPalet> listPaleti;
    }

    public class TaxaMasina
    {
        public string werks;
        public string vstel;
        public string macara;
        public string lift;
        public string camionIveco;
        public string camionScurt;
        public string camionOricare;
        public string taxaMacara;
        public string matnrMacara;
        public string maktxMacara;
        public string nrPaleti;
        public string matnrUsor;
        public string maktxUsor;
        public string taxaUsor;
        public string matnrZona;
        public string maktxZona;
        public string taxaZona;
        public string matnrAcces;
        public string maktxAcces;
        public string taxaAcces;
        public string matnrTransport;
        public string maktxTransport;
        public string taxaTransport;
        public string spart;
        public string traty;
        public List<TaxaMasina> taxeDivizii;
    }

    public class CostTransportMathaus
    {
        public string filiala;
        public string tipTransp;
        public string valTransp;
        public string codArtTransp;
        public string depart;
        public string numeCost;
    }

    public class DepozitArticolTransport
    {
        public string codArticol;
        public string filiala;
        public string depozit;
        public string cmpCorectat;
    }

    public class ComandaMathaus
    {
        public string sellingPlant;
        public string countyCode;
        public string deliveryZoneType;
        public List<DateArticolMathaus> deliveryEntryDataList;
    }

    public class LivrareMathaus
    {
        public ComandaMathaus comandaMathaus;
        public List<CostTransportMathaus> costTransport;
        public List<TaxaMasina> taxeMasini;
        public List<ArticolPalet> listPaleti;
    }

    public class DateArticolMathaus
    {
        public string deliveryWarehouse;
        public string productCode;
        public double quantity;
        public string unit;
        public double valPoz;
        public string tip2;
        public string depozit;
        public string ulStoc;
        public string greutate;
        public double quantity50;
        public string unit50;
        public double cantUmb;
        public string tipStoc;
        public string cmpCorectat;

    }

    public class StockMathaus
    {
        public string plant;
        public string deliveryZoneType;
        public List<StockEntryDataList> stockEntryDataList;
    }

    public class StockEntryDataList
    {
        public string productCode;
        public double availableQuantity;
        public string warehouse;
    }

    public class OptiuneCamion
    {
        public string nume;
        public bool exista;
        public bool selectat;
    }
}