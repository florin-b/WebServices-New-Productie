using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebService1.Salarizare2019;
using System.Web.Script.Serialization;


namespace WebService1
{
    public class SalarizareAgenti2019
    {

        public string getSalarizareAgent(string codAgent, string ul, string divizie, string an, string luna)
        {

            DatePrincipale datePrincipale = new DatePrincipale();
            List<DetaliiBaza> listDetaliiBaza = new List<DetaliiBaza>();
            DetaliiTCF detaliiTCF = new DetaliiTCF();
            DetaliiCorectie detaliiCorectie = new DetaliiCorectie();
            List<DetaliiIncasari08> listDetaliiIncasari08 = new List<DetaliiIncasari08>();
            SalarizareAgent salarizare = new SalarizareAgent();
            List<DetaliiMalus1> listDetaliiMalus1 = new List<DetaliiMalus1>();

            Salarizare2019.ZWS_SALARIZARE_2019 salarizareService = new ZWS_SALARIZARE_2019();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            salarizareService.Credentials = nc;
            salarizareService.Timeout = 300000;

            Salarizare2019.ZgetSalav2019 inputParams = new ZgetSalav2019();

            inputParams.Pernr = codAgent;
            inputParams.Ul = ul;
            inputParams.Divizie = divizie;
            inputParams.An = an;
            inputParams.Luna = luna;

            Salarizare2019.ZsalBazaMatkl[] gtBazaclExp = new ZsalBazaMatkl[1];
            Salarizare2019.ZsalCorrInc[] gtCorrInc = new ZsalCorrInc[1];
            Salarizare2019.ZsalCorr08[] gtInc08Ex = new ZsalCorr08[1];
            Salarizare2019.ZsalCorrMalus[] gtIncmaEx = new ZsalCorrMalus[1];
            Salarizare2019.ZsalFactMalus[] gtMalEx = new ZsalFactMalus[1];
            Salarizare2019.Zsalav19[] gtOuttabAv = new Zsalav19[1];
            Salarizare2019.ZsalNtcf[] gtTcf = new ZsalNtcf[1];


            inputParams.GtBazaclExp = gtBazaclExp;
            inputParams.GtCorrInc = gtCorrInc;
            inputParams.GtInc08Ex = gtInc08Ex;
            inputParams.GtIncmaEx = gtIncmaEx;
            inputParams.GtMalEx = gtMalEx;
            inputParams.GtOuttabAv = gtOuttabAv;
            inputParams.GtTcf = gtTcf;

            Salarizare2019.ZgetSalav2019Response response = salarizareService.ZgetSalav2019(inputParams);



            if (response.GtOuttabAv.Length > 0)
            {

                datePrincipale.venitMJ_T1 = Double.Parse(response.GtOuttabAv[0].Baza.ToString());
                datePrincipale.venitTCF = Double.Parse(response.GtOuttabAv[0].Venittcf.ToString());
                datePrincipale.corectieIncasare = Double.Parse(response.GtOuttabAv[0].CorectIncas.ToString());
                datePrincipale.venitFinal = Double.Parse(response.GtOuttabAv[0].Venitfinal.ToString());

                ZsalBazaMatkl[] respDetaliiBaza = response.GtBazaclExp;

                for (int i = 0; i < respDetaliiBaza.Length; i++)
                {
                    DetaliiBaza detaliiBaza = new DetaliiBaza();
                    detaliiBaza.numeClient = respDetaliiBaza[i].Name1;
                    detaliiBaza.codSintetic = respDetaliiBaza[i].Matkl;
                    detaliiBaza.numeSintetic = respDetaliiBaza[i].Wgbez;
                    detaliiBaza.valoareNeta = Double.Parse(respDetaliiBaza[i].ValNet.ToString());
                    detaliiBaza.T0 = Double.Parse(respDetaliiBaza[i].t0.ToString());
                    detaliiBaza.T1A = Double.Parse(respDetaliiBaza[i].T1a.ToString());
                    detaliiBaza.T1D = Double.Parse(respDetaliiBaza[i].T1d.ToString());
                    detaliiBaza.T1 = Double.Parse(respDetaliiBaza[i].t1.ToString());
                    detaliiBaza.venitBaza = Double.Parse(respDetaliiBaza[i].VenitBaza.ToString());

                    listDetaliiBaza.Add(detaliiBaza);

                }

                listDetaliiBaza = listDetaliiBaza.OrderBy(o => o.numeClient).ThenBy(o => o.numeSintetic).ToList();

                ZsalNtcf respDetaliiNtcf = response.GtTcf[0];

                detaliiTCF.venitBaza = Double.Parse(respDetaliiNtcf.VenitBaza.ToString());
                detaliiTCF.clientiAnterior = Int32.Parse(respDetaliiNtcf.NrclAnterior).ToString();
                detaliiTCF.target = Int32.Parse(respDetaliiNtcf.Target).ToString();
                detaliiTCF.clientiCurent = Int32.Parse(respDetaliiNtcf.NrclCurent).ToString();
                detaliiTCF.coeficient = Double.Parse(respDetaliiNtcf.Coef.ToString());
                detaliiTCF.venitTcf = Double.Parse(respDetaliiNtcf.Venittcf.ToString());


                ZsalCorrInc respDetaliiCor = response.GtCorrInc[0];

                detaliiCorectie.venitBaza = Double.Parse(respDetaliiCor.Baza.ToString());
                detaliiCorectie.incasari08 = Double.Parse(respDetaliiCor.Incas08.ToString());
                detaliiCorectie.malus = Double.Parse(respDetaliiCor.Malus.ToString());
                detaliiCorectie.venitCorectat = Double.Parse(respDetaliiCor.VenCorInc.ToString());

                ZsalCorr08[] respDetaliiCorectie08 = response.GtInc08Ex;


                for (int i = 0; i < respDetaliiCorectie08.Length; i++)
                {
                    DetaliiIncasari08 incasari08 = new DetaliiIncasari08();
                    incasari08.numeClient = respDetaliiCorectie08[i].Name1;
                    incasari08.valoareIncasare = Double.Parse(respDetaliiCorectie08[i].Incas08.ToString());
                    incasari08.venitCorectat = Double.Parse(respDetaliiCorectie08[i].VenCorInc.ToString());
                    listDetaliiIncasari08.Add(incasari08);
                }

                listDetaliiIncasari08 = listDetaliiIncasari08.OrderBy(o => o.numeClient).ToList();

                ZsalFactMalus[] respDetaliiMalus = response.GtMalEx;


                for (int i = 0; i < respDetaliiMalus.Length; i++)
                {
                    DetaliiMalus1 detaliiMalus = new DetaliiMalus1();
                    detaliiMalus.numeClient = respDetaliiMalus[i].Name1;
                    detaliiMalus.valoareFactura = Double.Parse(respDetaliiMalus[i].ValFact.ToString());
                    detaliiMalus.penalizare = Double.Parse(respDetaliiMalus[i].Malus.ToString());
                    detaliiMalus.codClient = respDetaliiMalus[i].Kunnr;

                    detaliiMalus.nrFactura = respDetaliiMalus[i].Xblnr;
                    detaliiMalus.dataFactura = formatDateRo(respDetaliiMalus[i].BudatFac);
                    detaliiMalus.tpFact = Int32.Parse(respDetaliiMalus[i].TpFact.ToString());
                    detaliiMalus.tpAgreat = Int32.Parse(respDetaliiMalus[i].TpAgreat.ToString());
                    detaliiMalus.tpIstoric = Int32.Parse(respDetaliiMalus[i].TpIst.ToString());
                    detaliiMalus.valIncasare = Double.Parse(respDetaliiMalus[i].ValInc.ToString());
                    detaliiMalus.dataIncasare = formatDateRo(respDetaliiMalus[i].BudatInc);
                    detaliiMalus.zileIntarziere = Int32.Parse(respDetaliiMalus[i].ZileInt.ToString());
                    detaliiMalus.coefPenalizare = Double.Parse(respDetaliiMalus[i].CoefY.ToString());

                    listDetaliiMalus1.Add(detaliiMalus);
                }


            }


            salarizareService.Dispose();

            salarizare.datePrincipale = datePrincipale;
            salarizare.detaliiBaza = listDetaliiBaza;
            salarizare.detaliiTCF = detaliiTCF;
            salarizare.detaliiCorectie = detaliiCorectie;
            salarizare.detaliiIncasari08 = listDetaliiIncasari08;
            salarizare.detaliiMalus = listDetaliiMalus1;

            return new JavaScriptSerializer().Serialize(salarizare);


        }


        public string getSalarizareSD(string codAgent, string ul, string divizie, string an, string luna)
        {

            SalarizareSD salarizare = new SalarizareSD();
            DatePrincipale datePrincipale = new DatePrincipale();
            List<DetaliiBaza> listDetaliiBaza = new List<DetaliiBaza>();
            DetaliiTCF detaliiTCF = new DetaliiTCF();
            DetaliiCorectie detaliiCorectie = new DetaliiCorectie();
            List<DetaliiIncasari08> listDetaliiIncasari08 = new List<DetaliiIncasari08>();
            List<DetaliiCVS> listDetaliiCvs = new List<DetaliiCVS>();
            List<DetaliiMalus1> listDetaliiMalus1 = new List<DetaliiMalus1>();

            Salarizare2019.ZWS_SALARIZARE_2019 salarizareService = new ZWS_SALARIZARE_2019();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            salarizareService.Credentials = nc;
            salarizareService.Timeout = 300000;

            Salarizare2019.ZgetSalsd2019 inputParams = new ZgetSalsd2019();

            inputParams.Pernr = codAgent;
            inputParams.Ul = ul;
            inputParams.Divizie = divizie;
            inputParams.An = an;
            inputParams.Luna = luna;

            Salarizare2019.ZsalBazaMatklSd[] gtBazaclExp = new ZsalBazaMatklSd[1];
            Salarizare2019.ZSAL_CORR_INC[] gtCorrInc = new ZSAL_CORR_INC[1];
            Salarizare2019.ZSAL_CORR_08[] gtInc08Ex = new ZSAL_CORR_08[1];
            Salarizare2019.ZSAL_CORR_MALUS[] gtIncmaEx = new ZSAL_CORR_MALUS[1];
            Salarizare2019.ZSAL_FACT_MALUS[] gtMalEx = new ZSAL_FACT_MALUS[1];
            Salarizare2019.Zsalsd19[] gtOuttabSd = new Zsalsd19[1];
            Salarizare2019.ZSAL_NTCF[] gtTcf = new ZSAL_NTCF[1];
            Salarizare2019.Zcvss2019[] gtCvss = new Zcvss2019[1];

            inputParams.GtBazaclExp = gtBazaclExp;
            inputParams.GtCorrInc = gtCorrInc;
            inputParams.GtInc08Ex = gtInc08Ex;
            inputParams.GtIncmaEx = gtIncmaEx;
            inputParams.GtMalEx = gtMalEx;
            inputParams.GtOuttabSd = gtOuttabSd;
            inputParams.GtTcf = gtTcf;
            inputParams.GtCvss = gtCvss;


            Salarizare2019.ZgetSalsd2019Response response = salarizareService.ZgetSalsd2019(inputParams);

            if (response.GtOuttabSd.Length > 0)
            {

                datePrincipale.venitMJ_T1 = Double.Parse(response.GtOuttabSd[0].Baza.ToString());
                datePrincipale.venitTCF = Double.Parse(response.GtOuttabSd[0].Venittcf.ToString());
                datePrincipale.corectieIncasare = Double.Parse(response.GtOuttabSd[0].CorectIncas.ToString());
                datePrincipale.venitCVS = Double.Parse(response.GtOuttabSd[0].VenitCvs.ToString());
                datePrincipale.venitFinal = Double.Parse(response.GtOuttabSd[0].Venitfinal.ToString());

                ZsalBazaMatklSd[] respDetaliiBaza = response.GtBazaclExp;

                for (int i = 0; i < respDetaliiBaza.Length; i++)
                {
                    DetaliiBaza detaliiBaza = new DetaliiBaza();
                    detaliiBaza.numeClient = respDetaliiBaza[i].Name1;
                    detaliiBaza.codSintetic = respDetaliiBaza[i].Matkl;
                    detaliiBaza.numeSintetic = respDetaliiBaza[i].Wgbez;
                    detaliiBaza.valoareNeta = Double.Parse(respDetaliiBaza[i].ValNet.ToString());
                    detaliiBaza.T0 = Double.Parse(respDetaliiBaza[i].t0.ToString());
                    detaliiBaza.T1A = Double.Parse(respDetaliiBaza[i].T1a.ToString());
                    detaliiBaza.T1D = Double.Parse(respDetaliiBaza[i].T1d.ToString());
                    detaliiBaza.T1 = Double.Parse(respDetaliiBaza[i].t1.ToString());
                    detaliiBaza.venitBaza = Double.Parse(respDetaliiBaza[i].VenitBaza.ToString());

                    listDetaliiBaza.Add(detaliiBaza);

                }

                listDetaliiBaza = listDetaliiBaza.OrderBy(o => o.numeClient).ThenBy(o => o.numeSintetic).ToList();


                ZSAL_NTCF respDetaliiNtcf = response.GtTcf[0];

                detaliiTCF.venitBaza = Double.Parse(respDetaliiNtcf.VENIT_BAZA.ToString());
                detaliiTCF.clientiAnterior = Int32.Parse(respDetaliiNtcf.NRCL_ANTERIOR).ToString();
                detaliiTCF.target = Int32.Parse(respDetaliiNtcf.TARGET).ToString();
                detaliiTCF.clientiCurent = Int32.Parse(respDetaliiNtcf.NRCL_CURENT).ToString();
                detaliiTCF.coeficient = Double.Parse(respDetaliiNtcf.COEF.ToString());
                detaliiTCF.venitTcf = Double.Parse(respDetaliiNtcf.VENITTCF.ToString());


                ZSAL_CORR_INC respDetaliiCor = response.GtCorrInc[0];

                detaliiCorectie.venitBaza = Double.Parse(respDetaliiCor.BAZA.ToString());
                detaliiCorectie.incasari08 = Double.Parse(respDetaliiCor.INCAS_0_8.ToString());
                detaliiCorectie.malus = Double.Parse(respDetaliiCor.MALUS.ToString());
                detaliiCorectie.venitCorectat = Double.Parse(respDetaliiCor.VEN_COR_INC.ToString());

                ZSAL_CORR_08[] respDetaliiCorectie08 = response.GtInc08Ex;


                for (int i = 0; i < respDetaliiCorectie08.Length; i++)
                {
                    DetaliiIncasari08 incasari08 = new DetaliiIncasari08();
                    incasari08.numeClient = respDetaliiCorectie08[i].NAME1;
                    incasari08.valoareIncasare = Double.Parse(respDetaliiCorectie08[i].INCAS_0_8.ToString());
                    incasari08.venitCorectat = Double.Parse(respDetaliiCorectie08[i].VEN_COR_INC.ToString());
                    listDetaliiIncasari08.Add(incasari08);
                }

                listDetaliiIncasari08 = listDetaliiIncasari08.OrderBy(o => o.numeClient).ToList();

                Zcvss2019[] respGtCvss = response.GtCvss;

                for (int i = 0; i < respGtCvss.Length; i++)
                {
                    DetaliiCVS detaliiCvs = new DetaliiCVS();
                    
                    listDetaliiCvs.Add(detaliiCvs);

                }


            }


            ZSAL_FACT_MALUS[] respDetaliiMalus = response.GtMalEx;


            for (int i = 0; i < respDetaliiMalus.Length; i++)
            {
                DetaliiMalus1 detaliiMalus = new DetaliiMalus1();
                detaliiMalus.numeClient = respDetaliiMalus[i].NAME1;
                detaliiMalus.valoareFactura = Double.Parse(respDetaliiMalus[i].VAL_FACT.ToString());
                detaliiMalus.penalizare = Double.Parse(respDetaliiMalus[i].MALUS.ToString());
                detaliiMalus.codClient = respDetaliiMalus[i].KUNNR;

                detaliiMalus.nrFactura = respDetaliiMalus[i].XBLNR;
                detaliiMalus.dataFactura = formatDateRo(respDetaliiMalus[i].BUDAT_FAC);
                detaliiMalus.tpFact = Int32.Parse(respDetaliiMalus[i].TP_FACT.ToString());
                detaliiMalus.tpAgreat = Int32.Parse(respDetaliiMalus[i].TP_AGREAT.ToString());
                detaliiMalus.tpIstoric = Int32.Parse(respDetaliiMalus[i].TP_IST.ToString());
                detaliiMalus.valIncasare = Double.Parse(respDetaliiMalus[i].VAL_INC.ToString());
                detaliiMalus.dataIncasare = formatDateRo(respDetaliiMalus[i].BUDAT_INC);
                detaliiMalus.zileIntarziere = Int32.Parse(respDetaliiMalus[i].ZILE_INT.ToString());
                detaliiMalus.coefPenalizare = Double.Parse(respDetaliiMalus[i].COEF_Y.ToString());

                listDetaliiMalus1.Add(detaliiMalus);
            }


            salarizareService.Dispose();

            salarizare.datePrincipale = datePrincipale;
            salarizare.detaliiBaza = listDetaliiBaza;
            salarizare.detaliiTCF = detaliiTCF;
            salarizare.detaliiCorectie = detaliiCorectie;
            salarizare.detaliiIncasari08 = listDetaliiIncasari08;
            salarizare.detaliiCVS = listDetaliiCvs;
            salarizare.detaliiMalus = listDetaliiMalus1;


            return new JavaScriptSerializer().Serialize(salarizare);
        }




        public string getSalarizareDepartament(string ul, string divizie, string an, string luna)
        {

            List<SalarizareAgentAfis> listAgenti = new List<SalarizareAgentAfis>();
            Salarizare2019.ZWS_SALARIZARE_2019 salarizareService = new ZWS_SALARIZARE_2019();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            salarizareService.Credentials = nc;
            salarizareService.Timeout = 300000;

            Salarizare2019.ZgetSalav2019 inputParams = new ZgetSalav2019();

            inputParams.Pernr = "00000000";
            inputParams.Ul = ul;
            inputParams.Divizie = divizie;
            inputParams.An = an;
            inputParams.Luna = luna;

            Salarizare2019.ZsalBazaMatkl[] gtBazaclExp = new ZsalBazaMatkl[1];
            Salarizare2019.ZsalCorrInc[] gtCorrInc = new ZsalCorrInc[1];
            Salarizare2019.ZsalCorr08[] gtInc08Ex = new ZsalCorr08[1];
            Salarizare2019.ZsalCorrMalus[] gtIncmaEx = new ZsalCorrMalus[1];
            Salarizare2019.ZsalFactMalus[] gtMalEx = new ZsalFactMalus[1];
            Salarizare2019.Zsalav19[] gtOuttabAv = new Zsalav19[1];
            Salarizare2019.ZsalNtcf[] gtTcf = new ZsalNtcf[1];

            inputParams.GtBazaclExp = gtBazaclExp;
            inputParams.GtCorrInc = gtCorrInc;
            inputParams.GtInc08Ex = gtInc08Ex;
            inputParams.GtIncmaEx = gtIncmaEx;
            inputParams.GtMalEx = gtMalEx;
            inputParams.GtOuttabAv = gtOuttabAv;
            inputParams.GtTcf = gtTcf;

            Salarizare2019.ZgetSalav2019Response response = salarizareService.ZgetSalav2019(inputParams);

            if (response.GtOuttabAv.Length > 0)
            {
                Zsalav19[] respDatePrinc = response.GtOuttabAv;

                for (int i = 0; i < respDatePrinc.Length; i++)
                {
                    SalarizareAgentAfis salarizareAgent = new SalarizareAgentAfis();

                    salarizareAgent.numeAgent = respDatePrinc[i].Ename;
                    salarizareAgent.codAgent = respDatePrinc[i].Pernr;

                    DatePrincipale datePrincipale = new DatePrincipale();
                    datePrincipale.venitMJ_T1 = Double.Parse(respDatePrinc[i].Baza.ToString());
                    datePrincipale.venitTCF = Double.Parse(respDatePrinc[i].Venittcf.ToString());
                    datePrincipale.corectieIncasare = Double.Parse(respDatePrinc[i].CorectIncas.ToString());
                    datePrincipale.venitFinal = Double.Parse(respDatePrinc[i].Venitfinal.ToString());

                    salarizareAgent.datePrincipale = datePrincipale;
                    listAgenti.Add(salarizareAgent);

                }

                listAgenti = listAgenti.OrderBy(o => o.numeAgent).ToList();

            }

            salarizareService.Dispose();

            return new JavaScriptSerializer().Serialize(listAgenti);
        }


        private static string formatDateRo(string dateEn)
        {
            string[] dateArray = dateEn.Split('-');
            return dateArray[2] + "-" + dateArray[1] + "-" + dateArray[0];

        }


        public string getSalarizareKA(string codAgent, string ul, string an, string luna)
        {
            DatePrincipale datePrincipale = new DatePrincipale();
            List<DetaliiBaza> listDetaliiBaza = new List<DetaliiBaza>();
            DetaliiTCF detaliiTCF = new DetaliiTCF();
            DetaliiCorectie detaliiCorectie = new DetaliiCorectie();
            List<DetaliiIncasari08> listDetaliiIncasari08 = new List<DetaliiIncasari08>();
            SalarizareAgent salarizare = new SalarizareAgent();
            List<DetaliiMalus1> listDetaliiMalus1 = new List<DetaliiMalus1>();


            SalarizareKA.ZWS_SAL_KA_2019 salarizareService = new SalarizareKA.ZWS_SAL_KA_2019();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            salarizareService.Credentials = nc;
            salarizareService.Timeout = 300000;

            SalarizareKA.ZgetSalka2019 inputParams = new SalarizareKA.ZgetSalka2019();

            inputParams.Pernr = codAgent;
            inputParams.Ul = ul;
            inputParams.An = an;
            inputParams.Luna = luna;

            SalarizareKA.ZsalBazaMatkl[] gtBazaclExp = new SalarizareKA.ZsalBazaMatkl[1];
            SalarizareKA.ZsalCorrInc[] gtCorrInc = new SalarizareKA.ZsalCorrInc[1];
            SalarizareKA.ZsalCorr08[] gtInc08Ex = new SalarizareKA.ZsalCorr08[1];
            SalarizareKA.ZsalCorrMalus[] gtIncmaEx = new SalarizareKA.ZsalCorrMalus[1];
            SalarizareKA.ZsalFactMalus[] gtMalEx = new SalarizareKA.ZsalFactMalus[1];
            SalarizareKA.Zsalav19[] gtOuttabAv = new SalarizareKA.Zsalav19[1];




            inputParams.GtBazaclExp = gtBazaclExp;
            inputParams.GtCorrInc = gtCorrInc;
            inputParams.GtInc08Ex = gtInc08Ex;
            inputParams.GtIncmaEx = gtIncmaEx;
            inputParams.GtMalEx = gtMalEx;
            inputParams.GtOuttabAv = gtOuttabAv;

            SalarizareKA.ZgetSalka2019Response response = salarizareService.ZgetSalka2019(inputParams);

            if (response.GtOuttabAv.Length > 0)
            {

                datePrincipale.venitMJ_T1 = Double.Parse(response.GtOuttabAv[0].Baza.ToString());
                datePrincipale.venitTCF = Double.Parse(response.GtOuttabAv[0].Venittcf.ToString());
                datePrincipale.corectieIncasare = Double.Parse(response.GtOuttabAv[0].CorectIncas.ToString());
                datePrincipale.venitFinal = Double.Parse(response.GtOuttabAv[0].Venitfinal.ToString());


                SalarizareKA.ZsalBazaMatkl[] respDetaliiBaza = response.GtBazaclExp;

                for (int i = 0; i < respDetaliiBaza.Length; i++)
                {
                    DetaliiBaza detaliiBaza = new DetaliiBaza();
                    detaliiBaza.numeClient = respDetaliiBaza[i].Name1;
                    detaliiBaza.codSintetic = respDetaliiBaza[i].Matkl;
                    detaliiBaza.numeSintetic = respDetaliiBaza[i].Wgbez;
                    detaliiBaza.valoareNeta = Double.Parse(respDetaliiBaza[i].ValNet.ToString());
                    detaliiBaza.T0 = Double.Parse(respDetaliiBaza[i].t0.ToString());
                    detaliiBaza.T1A = Double.Parse(respDetaliiBaza[i].T1a.ToString());
                    detaliiBaza.T1D = Double.Parse(respDetaliiBaza[i].T1d.ToString());
                    detaliiBaza.T1 = Double.Parse(respDetaliiBaza[i].t1.ToString());
                    detaliiBaza.venitBaza = Double.Parse(respDetaliiBaza[i].VenitBaza.ToString());

                    listDetaliiBaza.Add(detaliiBaza);

                }

                listDetaliiBaza = listDetaliiBaza.OrderBy(o => o.numeClient).ThenBy(o => o.numeSintetic).ToList();

                SalarizareKA.ZsalCorrInc respDetaliiCor = response.GtCorrInc[0];

                detaliiCorectie.venitBaza = Double.Parse(respDetaliiCor.Baza.ToString());
                detaliiCorectie.incasari08 = Double.Parse(respDetaliiCor.Incas08.ToString());
                detaliiCorectie.malus = Double.Parse(respDetaliiCor.Malus.ToString());
                detaliiCorectie.venitCorectat = Double.Parse(respDetaliiCor.VenCorInc.ToString());


                SalarizareKA.ZsalCorr08[] respDetaliiCorectie08 = response.GtInc08Ex;


                for (int i = 0; i < respDetaliiCorectie08.Length; i++)
                {
                    DetaliiIncasari08 incasari08 = new DetaliiIncasari08();
                    incasari08.numeClient = respDetaliiCorectie08[i].Name1;
                    incasari08.valoareIncasare = Double.Parse(respDetaliiCorectie08[i].Incas08.ToString());
                    incasari08.venitCorectat = Double.Parse(respDetaliiCorectie08[i].VenCorInc.ToString());
                    listDetaliiIncasari08.Add(incasari08);
                }

                listDetaliiIncasari08 = listDetaliiIncasari08.OrderBy(o => o.numeClient).ToList();

                SalarizareKA.ZsalFactMalus[] respDetaliiMalus = response.GtMalEx;


                for (int i = 0; i < respDetaliiMalus.Length; i++)
                {
                    DetaliiMalus1 detaliiMalus = new DetaliiMalus1();
                    detaliiMalus.numeClient = respDetaliiMalus[i].Name1;
                    detaliiMalus.valoareFactura = Double.Parse(respDetaliiMalus[i].ValFact.ToString());
                    detaliiMalus.penalizare = Double.Parse(respDetaliiMalus[i].Malus.ToString());
                    detaliiMalus.codClient = respDetaliiMalus[i].Kunnr;

                    detaliiMalus.nrFactura = respDetaliiMalus[i].Xblnr;
                    detaliiMalus.dataFactura = formatDateRo(respDetaliiMalus[i].BudatFac);
                    detaliiMalus.tpFact = Int32.Parse(respDetaliiMalus[i].TpFact.ToString());
                    detaliiMalus.tpAgreat = Int32.Parse(respDetaliiMalus[i].TpAgreat.ToString());
                    detaliiMalus.tpIstoric = Int32.Parse(respDetaliiMalus[i].TpIst.ToString());
                    detaliiMalus.valIncasare = Double.Parse(respDetaliiMalus[i].ValInc.ToString());
                    detaliiMalus.dataIncasare = formatDateRo(respDetaliiMalus[i].BudatInc);
                    detaliiMalus.zileIntarziere = Int32.Parse(respDetaliiMalus[i].ZileInt.ToString());
                    detaliiMalus.coefPenalizare = Double.Parse(respDetaliiMalus[i].CoefY.ToString());

                    listDetaliiMalus1.Add(detaliiMalus);
                }

            }

            salarizareService.Dispose();

            salarizare.datePrincipale = datePrincipale;
            salarizare.detaliiBaza = listDetaliiBaza;
            salarizare.detaliiTCF = detaliiTCF;
            salarizare.detaliiCorectie = detaliiCorectie;
            salarizare.detaliiIncasari08 = listDetaliiIncasari08;
            salarizare.detaliiMalus = listDetaliiMalus1;

            return new JavaScriptSerializer().Serialize(salarizare);


        }



        public string getSalarizareDepartamentKA(string ul, string an, string luna)
        {

            List<SalarizareAgentAfis> listAgenti = new List<SalarizareAgentAfis>();
            SalarizareKA.ZWS_SAL_KA_2019 salarizareService = new SalarizareKA.ZWS_SAL_KA_2019();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            salarizareService.Credentials = nc;
            salarizareService.Timeout = 300000;

            SalarizareKA.ZgetSalka2019 inputParams = new SalarizareKA.ZgetSalka2019();

            inputParams.Pernr = "00000000";
            inputParams.Ul = ul;
            inputParams.An = an;
            inputParams.Luna = luna;

            SalarizareKA.ZsalBazaMatkl[] gtBazaclExp = new SalarizareKA.ZsalBazaMatkl[1];
            SalarizareKA.ZsalCorrInc[] gtCorrInc = new SalarizareKA.ZsalCorrInc[1];
            SalarizareKA.ZsalCorr08[] gtInc08Ex = new SalarizareKA.ZsalCorr08[1];
            SalarizareKA.ZsalCorrMalus[] gtIncmaEx = new SalarizareKA.ZsalCorrMalus[1];
            SalarizareKA.ZsalFactMalus[] gtMalEx = new SalarizareKA.ZsalFactMalus[1];
            SalarizareKA.Zsalav19[] gtOuttabAv = new SalarizareKA.Zsalav19[1];

            inputParams.GtBazaclExp = gtBazaclExp;
            inputParams.GtCorrInc = gtCorrInc;
            inputParams.GtInc08Ex = gtInc08Ex;
            inputParams.GtIncmaEx = gtIncmaEx;
            inputParams.GtMalEx = gtMalEx;
            inputParams.GtOuttabAv = gtOuttabAv;

            SalarizareKA.ZgetSalka2019Response response = salarizareService.ZgetSalka2019(inputParams);

            if (response.GtOuttabAv.Length > 0)
            {
                SalarizareKA.Zsalav19[] respDatePrinc = response.GtOuttabAv;

                for (int i = 0; i < respDatePrinc.Length; i++)
                {
                    SalarizareAgentAfis salarizareAgent = new SalarizareAgentAfis();

                    salarizareAgent.numeAgent = respDatePrinc[i].Ename;
                    salarizareAgent.codAgent = respDatePrinc[i].Pernr;

                    DatePrincipale datePrincipale = new DatePrincipale();
                    datePrincipale.venitMJ_T1 = Double.Parse(respDatePrinc[i].Baza.ToString());
                    datePrincipale.venitTCF = Double.Parse(respDatePrinc[i].Venittcf.ToString());
                    datePrincipale.corectieIncasare = Double.Parse(respDatePrinc[i].CorectIncas.ToString());
                    datePrincipale.venitFinal = Double.Parse(respDatePrinc[i].Venitfinal.ToString());

                    salarizareAgent.datePrincipale = datePrincipale;
                    listAgenti.Add(salarizareAgent);

                }

                listAgenti = listAgenti.OrderBy(o => o.numeAgent).ToList();

            }

            salarizareService.Dispose();

            return new JavaScriptSerializer().Serialize(listAgenti);
        }




        public string getSalarizareSDKA(string codAgent, string ul, string an, string luna)
        {
            SalarizareSD salarizare = new SalarizareSD();
            DatePrincipale datePrincipale = new DatePrincipale();
            List<DetaliiBaza> listDetaliiBaza = new List<DetaliiBaza>();
            DetaliiTCF detaliiTCF = new DetaliiTCF();
            DetaliiCorectie detaliiCorectie = new DetaliiCorectie();
            List<DetaliiIncasari08> listDetaliiIncasari08 = new List<DetaliiIncasari08>();
            List<DetaliiCVS> listDetaliiCvs = new List<DetaliiCVS>();
            List<DetaliiMalus1> listDetaliiMalus1 = new List<DetaliiMalus1>();


            SalarizareKA.ZWS_SAL_KA_2019 salarizareService = new SalarizareKA.ZWS_SAL_KA_2019();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            salarizareService.Credentials = nc;
            salarizareService.Timeout = 300000;

            SalarizareKA.ZgetSalsdka2019 inputParams = new SalarizareKA.ZgetSalsdka2019();

            inputParams.Pernr = codAgent;
            inputParams.Ul = ul;
            inputParams.An = an;
            inputParams.Luna = luna;

            SalarizareKA.ZsalBazaMatklSd[] gtBazaclExp = new SalarizareKA.ZsalBazaMatklSd[1];
            SalarizareKA.ZSAL_CORR_INC[] gtCorrInc = new SalarizareKA.ZSAL_CORR_INC[1];
            SalarizareKA.ZSAL_CORR_08[] gtInc08Ex = new SalarizareKA.ZSAL_CORR_08[1];
            SalarizareKA.ZSAL_CORR_MALUS[] gtIncmaEx = new SalarizareKA.ZSAL_CORR_MALUS[1];
            SalarizareKA.ZSAL_FACT_MALUS[] gtMalEx = new SalarizareKA.ZSAL_FACT_MALUS[1];
            SalarizareKA.Zsalsd19[] gtOuttabSd = new SalarizareKA.Zsalsd19[1];

            inputParams.GtBazaclExp = gtBazaclExp;
            inputParams.GtCorrInc = gtCorrInc;
            inputParams.GtInc08Ex = gtInc08Ex;
            inputParams.GtIncmaEx = gtIncmaEx;
            inputParams.GtMalEx = gtMalEx;
            inputParams.GtOuttabSd = gtOuttabSd;

            SalarizareKA.ZgetSalsdka2019Response response = salarizareService.ZgetSalsdka2019(inputParams);

            if (response.GtOuttabSd.Length > 0)
            {
                datePrincipale.venitMJ_T1 = Double.Parse(response.GtOuttabSd[0].Baza.ToString());
                datePrincipale.venitTCF = Double.Parse(response.GtOuttabSd[0].Venittcf.ToString());
                datePrincipale.corectieIncasare = Double.Parse(response.GtOuttabSd[0].CorectIncas.ToString());
                datePrincipale.venitFinal = Double.Parse(response.GtOuttabSd[0].Venitfinal.ToString());

                SalarizareKA.ZsalBazaMatklSd[] respDetaliiBaza = response.GtBazaclExp;

                for (int i = 0; i < respDetaliiBaza.Length; i++)
                {
                    DetaliiBaza detaliiBaza = new DetaliiBaza();
                    detaliiBaza.numeClient = respDetaliiBaza[i].Name1;
                    detaliiBaza.codSintetic = respDetaliiBaza[i].Matkl;
                    detaliiBaza.numeSintetic = respDetaliiBaza[i].Wgbez;
                    detaliiBaza.valoareNeta = Double.Parse(respDetaliiBaza[i].ValNet.ToString());
                    detaliiBaza.T0 = Double.Parse(respDetaliiBaza[i].t0.ToString());
                    detaliiBaza.T1A = Double.Parse(respDetaliiBaza[i].T1a.ToString());
                    detaliiBaza.T1D = Double.Parse(respDetaliiBaza[i].T1d.ToString());
                    detaliiBaza.T1 = Double.Parse(respDetaliiBaza[i].t1.ToString());
                    detaliiBaza.venitBaza = Double.Parse(respDetaliiBaza[i].VenitBaza.ToString());

                    listDetaliiBaza.Add(detaliiBaza);

                }

                SalarizareKA.ZSAL_CORR_INC respDetaliiCor = response.GtCorrInc[0];

                detaliiCorectie.venitBaza = Double.Parse(respDetaliiCor.BAZA.ToString());
                detaliiCorectie.incasari08 = Double.Parse(respDetaliiCor.INCAS_0_8.ToString());
                detaliiCorectie.malus = Double.Parse(respDetaliiCor.MALUS.ToString());
                detaliiCorectie.venitCorectat = Double.Parse(respDetaliiCor.VEN_COR_INC.ToString());

                SalarizareKA.ZSAL_CORR_08[] respDetaliiCorectie08 = response.GtInc08Ex;


                for (int i = 0; i < respDetaliiCorectie08.Length; i++)
                {
                    DetaliiIncasari08 incasari08 = new DetaliiIncasari08();
                    incasari08.numeClient = respDetaliiCorectie08[i].NAME1;
                    incasari08.valoareIncasare = Double.Parse(respDetaliiCorectie08[i].INCAS_0_8.ToString());
                    incasari08.venitCorectat = Double.Parse(respDetaliiCorectie08[i].VEN_COR_INC.ToString());
                    listDetaliiIncasari08.Add(incasari08);
                }

                SalarizareKA.ZSAL_FACT_MALUS[] respDetaliiMalus = response.GtMalEx;

                for (int i = 0; i < respDetaliiMalus.Length; i++)
                {
                    DetaliiMalus1 detaliiMalus = new DetaliiMalus1();
                    detaliiMalus.numeClient = respDetaliiMalus[i].NAME1;
                    detaliiMalus.valoareFactura = Double.Parse(respDetaliiMalus[i].VAL_FACT.ToString());
                    detaliiMalus.penalizare = Double.Parse(respDetaliiMalus[i].MALUS.ToString());
                    detaliiMalus.codClient = respDetaliiMalus[i].KUNNR;

                    detaliiMalus.nrFactura = respDetaliiMalus[i].XBLNR;
                    detaliiMalus.dataFactura = formatDateRo(respDetaliiMalus[i].BUDAT_FAC);
                    detaliiMalus.tpFact = Int32.Parse(respDetaliiMalus[i].TP_FACT.ToString());
                    detaliiMalus.tpAgreat = Int32.Parse(respDetaliiMalus[i].TP_AGREAT.ToString());
                    detaliiMalus.tpIstoric = Int32.Parse(respDetaliiMalus[i].TP_IST.ToString());
                    detaliiMalus.valIncasare = Double.Parse(respDetaliiMalus[i].VAL_INC.ToString());
                    detaliiMalus.dataIncasare = formatDateRo(respDetaliiMalus[i].BUDAT_INC);
                    detaliiMalus.zileIntarziere = Int32.Parse(respDetaliiMalus[i].ZILE_INT.ToString());
                    detaliiMalus.coefPenalizare = Double.Parse(respDetaliiMalus[i].COEF_Y.ToString());

                    listDetaliiMalus1.Add(detaliiMalus);
                }

            }

            salarizareService.Dispose();

            salarizare.datePrincipale = datePrincipale;
            salarizare.detaliiBaza = listDetaliiBaza;
            salarizare.detaliiTCF = detaliiTCF;
            salarizare.detaliiCorectie = detaliiCorectie;
            salarizare.detaliiIncasari08 = listDetaliiIncasari08;
            salarizare.detaliiMalus = listDetaliiMalus1;
            salarizare.detaliiCVS = listDetaliiCvs;

            return new JavaScriptSerializer().Serialize(salarizare);

        }





    }
}