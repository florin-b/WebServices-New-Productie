


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using WebService1.SapWsSalarizare;

namespace WebService1
{
    public class SalarizareVanzari
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

            SapWsSalarizare.ZWS_SALARIZARE_24 salarizareVanzari = new SapWsSalarizare.ZWS_SALARIZARE_24();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            salarizareVanzari.Credentials = nc;
            salarizareVanzari.Timeout = 1200000;

            SapWsSalarizare.ZgetSalav24 inParams = new SapWsSalarizare.ZgetSalav24();


            try
            {


                inParams.Pernr = codAgent;
                inParams.Ul = ul;
                inParams.Divizie = divizie;
                inParams.An = an;
                inParams.Luna = luna;

                SapWsSalarizare.ZsalBazaMatkl[] gtBazaclExp = new ZsalBazaMatkl[1];
                SapWsSalarizare.ZsalCorrInc[] gtCorrInc = new ZsalCorrInc[1];
                SapWsSalarizare.ZsalCorr08[] gtInc08Ex = new ZsalCorr08[1];
                SapWsSalarizare.ZsalCorrMalus[] gtIncmaEx = new ZsalCorrMalus[1];
                SapWsSalarizare.ZsalFactMalus[] gtMalEx = new ZsalFactMalus[1];
                SapWsSalarizare.Zsalav20[] gtOuttabAv = new Zsalav20[1];
                SapWsSalarizare.ZsalNtcf[] gtTcf = new ZsalNtcf[1];
                SapWsSalarizare.ZSUM_VANZ_VS[] gtVenVs = new ZSUM_VANZ_VS[1];
                SapWsSalarizare.ZclIncrAlocat[] gtVenInc = new ZclIncrAlocat[1];

                inParams.GtBazaclExp = gtBazaclExp;
                inParams.GtCorrInc = gtCorrInc;
                inParams.GtInc08Ex = gtInc08Ex;
                inParams.GtIncmaEx = gtIncmaEx;
                inParams.GtMalEx = gtMalEx;
                inParams.GtOuttabAv = gtOuttabAv;
                inParams.GtTcf = gtTcf;
                inParams.GtVenVs = gtVenVs;
                inParams.GtVenIncr = gtVenInc;

                SapWsSalarizare.ZgetSalav24Response response = salarizareVanzari.ZgetSalav24(inParams);

                if (response.GtOuttabAv.Length > 0)
                {

                    datePrincipale.venitMJ_T1 = Double.Parse(response.GtOuttabAv[0].Baza.ToString());
                    datePrincipale.venitTCF = Double.Parse(response.GtOuttabAv[0].Venittcf.ToString());
                    datePrincipale.corectieIncasare = Double.Parse(response.GtOuttabAv[0].CorectIncas.ToString());
                    datePrincipale.venitFinal = Double.Parse(response.GtOuttabAv[0].Venitfinal.ToString());
                    datePrincipale.venitStocNociv = Double.Parse(response.GtOuttabAv[0].VenitVanzVs.ToString());
                    datePrincipale.venitIncrucisate = Double.Parse(response.GtOuttabAv[0].VenitIncrucis.ToString());


                    ZsalBazaMatkl[] respDetaliiBaza = response.GtBazaclExp;

                    for (int i = 0; i < respDetaliiBaza.Length; i++)
                    {
                        DetaliiBaza detaliiBaza = new DetaliiBaza();
                        detaliiBaza.numeClient = respDetaliiBaza[i].Name1;
                        detaliiBaza.codSintetic = respDetaliiBaza[i].Matkl;
                        detaliiBaza.numeSintetic = respDetaliiBaza[i].Wgbez.Trim();
                        detaliiBaza.valoareNeta = Double.Parse(respDetaliiBaza[i].ValNet.ToString());
                        detaliiBaza.T0 = Double.Parse(respDetaliiBaza[i].t0.ToString());
                        detaliiBaza.T1A = Double.Parse(respDetaliiBaza[i].T1a.ToString());
                        detaliiBaza.T1D = Double.Parse(respDetaliiBaza[i].T1d.ToString());
                        detaliiBaza.T1 = Double.Parse(respDetaliiBaza[i].t1.ToString());
                        detaliiBaza.venitBaza = Double.Parse(respDetaliiBaza[i].VenitBaza.ToString());

                        listDetaliiBaza.Add(detaliiBaza);

                    }

                    listDetaliiBaza = listDetaliiBaza.OrderBy(o => o.numeClient).ThenBy(o => o.numeSintetic).ToList();

                    if (response.GtTcf != null && response.GtTcf.Length > 0)
                    {

                        ZsalNtcf respDetaliiNtcf = response.GtTcf[0];

                        detaliiTCF.venitBaza = Double.Parse(respDetaliiNtcf.VenitBaza.ToString());
                        detaliiTCF.clientiAnterior = Int32.Parse(respDetaliiNtcf.NrclAnterior).ToString();
                        detaliiTCF.target = Int32.Parse(respDetaliiNtcf.Target).ToString();
                        detaliiTCF.clientiCurent = Int32.Parse(respDetaliiNtcf.NrclCurent).ToString();
                        detaliiTCF.coeficient = Double.Parse(respDetaliiNtcf.Coef.ToString());
                        detaliiTCF.venitTcf = Double.Parse(respDetaliiNtcf.Venittcf.ToString());
                    }

                    if (response.GtCorrInc != null && response.GtCorrInc.Length > 0)
                    {

                        ZsalCorrInc respDetaliiCor = response.GtCorrInc[0];

                        detaliiCorectie.venitBaza = Double.Parse(respDetaliiCor.Baza.ToString());
                        detaliiCorectie.incasari08 = Double.Parse(respDetaliiCor.Incas08.ToString());
                        detaliiCorectie.malus = Double.Parse(respDetaliiCor.Malus.ToString());
                        detaliiCorectie.venitCorectat = Double.Parse(respDetaliiCor.VenCorInc.ToString());

                    }

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


                salarizareVanzari.Dispose();

                salarizare.datePrincipale = datePrincipale;
                salarizare.detaliiBaza = listDetaliiBaza;
                salarizare.detaliiTCF = detaliiTCF;
                salarizare.detaliiCorectie = detaliiCorectie;
                salarizare.detaliiIncasari08 = listDetaliiIncasari08;
                salarizare.detaliiMalus = listDetaliiMalus1;
                salarizare.detaliiVanzariVS = response.GtVenVs.Cast<ZSUM_VANZ_VS>().ToList();
                salarizare.detaliiIncrAlocat = response.GtVenIncr.Cast<ZclIncrAlocat>().ToList();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("getSarizareAgent: \n\n" + ex.ToString() + "\n\n" + new JavaScriptSerializer().Serialize(inParams));
            }


            return new JavaScriptSerializer().Serialize(salarizare);

        }


        public string getSalarizareDepartament(string ul, string divizie, string an, string luna)
        {

            List<SalarizareAgentAfis> listAgenti = new List<SalarizareAgentAfis>();
            ZWS_SALARIZARE_24 salarizareVanzari = new ZWS_SALARIZARE_24();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            salarizareVanzari.Credentials = nc;
            salarizareVanzari.Timeout = 1200000;

            SapWsSalarizare.ZgetSalav24 inParams = new SapWsSalarizare.ZgetSalav24();

            inParams.Pernr = "00000000";
            inParams.Ul = ul;
            inParams.Divizie = divizie;
            inParams.An = an;
            inParams.Luna = luna;

            ZsalBazaMatkl[] gtBazaclExp = new ZsalBazaMatkl[1];
            ZsalCorrInc[] gtCorrInc = new ZsalCorrInc[1];
            ZsalCorr08[] gtInc08Ex = new ZsalCorr08[1];
            ZsalCorrMalus[] gtIncmaEx = new ZsalCorrMalus[1];
            ZsalFactMalus[] gtMalEx = new ZsalFactMalus[1];
            Zsalav20[] gtOuttabAv = new Zsalav20[1];
            ZsalNtcf[] gtTcf = new ZsalNtcf[1];
            ZSUM_VANZ_VS[] gtVenVs = new ZSUM_VANZ_VS[1];
            ZclIncrAlocat[] gtVenInc = new ZclIncrAlocat[1];

            inParams.GtBazaclExp = gtBazaclExp;
            inParams.GtCorrInc = gtCorrInc;
            inParams.GtInc08Ex = gtInc08Ex;
            inParams.GtIncmaEx = gtIncmaEx;
            inParams.GtMalEx = gtMalEx;
            inParams.GtOuttabAv = gtOuttabAv;
            inParams.GtTcf = gtTcf;
            inParams.GtVenVs = gtVenVs;
            inParams.GtVenIncr = gtVenInc;

            ZgetSalav24Response response = salarizareVanzari.ZgetSalav24(inParams);

            if (response.GtOuttabAv.Length > 0)
            {
                Zsalav20[] respDatePrinc = response.GtOuttabAv;

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
                    datePrincipale.venitStocNociv = Double.Parse(respDatePrinc[i].VenitVanzVs.ToString());
                    datePrincipale.venitIncrucisate = Double.Parse(respDatePrinc[i].VenitIncrucis.ToString());



                    salarizareAgent.datePrincipale = datePrincipale;
                    listAgenti.Add(salarizareAgent);

                }


                listAgenti = listAgenti.OrderBy(o => o.numeAgent).ToList();

            }

            salarizareVanzari.Dispose();


            return new JavaScriptSerializer().Serialize(listAgenti);
        }


        public string getSalarizareDepartKA(string ul, string an, string luna)
        {

            List<SalarizareAgentAfis> listAgenti = new List<SalarizareAgentAfis>();
            SapWsSalarizare.ZWS_SALARIZARE_24 salarizareVanzari = new SapWsSalarizare.ZWS_SALARIZARE_24();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            salarizareVanzari.Credentials = nc;
            salarizareVanzari.Timeout = 1200000;

            SapWsSalarizare.ZgetSalka24 inParams = new ZgetSalka24();

            try
            {


                inParams.Pernr = "00000000";
                inParams.Ul = ul;
                inParams.An = an;
                inParams.Luna = luna;

                ZsalBazaMatkl[] gtBazaclExp = new ZsalBazaMatkl[1];
                ZsalCorrInc[] gtCorrInc = new ZsalCorrInc[1];
                ZsalCorr08[] gtInc08Ex = new ZsalCorr08[1];
                ZsalCorrMalus[] gtIncmaEx = new ZsalCorrMalus[1];
                ZsalFactMalus[] gtMalEx = new ZsalFactMalus[1];
                Zsalav19[] gtOuttabAv = new Zsalav19[1];
                ZSUM_VANZ_VS[] gtVenVs = new ZSUM_VANZ_VS[1];

                inParams.GtBazaclExp = gtBazaclExp;
                inParams.GtCorrInc = gtCorrInc;
                inParams.GtInc08Ex = gtInc08Ex;
                inParams.GtIncmaEx = gtIncmaEx;
                inParams.GtMalEx = gtMalEx;
                inParams.GtOuttabAv = gtOuttabAv;
                inParams.GtVenVs = gtVenVs;

                ZgetSalka24Response response = salarizareVanzari.ZgetSalka24(inParams);

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
                        datePrincipale.venitStocNociv = Double.Parse(respDatePrinc[i].VenitVanzVs.ToString());

                        salarizareAgent.datePrincipale = datePrincipale;
                        listAgenti.Add(salarizareAgent);

                    }


                    listAgenti = listAgenti.OrderBy(o => o.numeAgent).ToList();

                }

                salarizareVanzari.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + "\n\n" + new JavaScriptSerializer().Serialize(inParams));
            }

            return new JavaScriptSerializer().Serialize(listAgenti);
        }


        public string getSalarizareDepartCVA(string unitLog, string an, string luna)
        {
            SapWsSalarizare.ZWS_SALARIZARE_24 salarizareVanzari = new SapWsSalarizare.ZWS_SALARIZARE_24();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            salarizareVanzari.Credentials = nc;
            salarizareVanzari.Timeout = 1200000;

            ZgetSalcva24 inParams = new ZgetSalcva24();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            String response = "";


            try
            {

                inParams.Pernr = "00000000";
                inParams.Ul = unitLog;
                inParams.An = an;
                inParams.Luna = luna;


                ZsalBazaMatkl[] baza1 = new ZsalBazaMatkl[1];
                inParams.GtBazaclExp = baza1;

                ZsalNruf[] nruf = new ZsalNruf[1];
                inParams.GtExp = nruf;

                Zsalav19[] salav19 = new Zsalav19[1];
                inParams.GtOuttabAv = salav19;

                ZSUM_VANZ_VS[] gtVenVs = new ZSUM_VANZ_VS[1];
                inParams.GtVenVs = gtVenVs;

                ZgetSalcva24Response resp = salarizareVanzari.ZgetSalcva24(inParams);

                response = serializer.Serialize(resp.GtOuttabAv);

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("getSalarizareDepartCVA: \n\n" + ex.ToString() + "\n\n" + serializer.Serialize(inParams));
            }

            return response;
        }


        public string getSalarizareDepartCVIP(string unitLog, string an, string luna)
        {
            SapWsSalarizare.ZWS_SALARIZARE_24 salarizareVanzari = new SapWsSalarizare.ZWS_SALARIZARE_24();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            salarizareVanzari.Credentials = nc;
            salarizareVanzari.Timeout = 1200000;

            ZgetSalcvip24 inParams = new ZgetSalcvip24();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            String response = "";


            try
            {

                inParams.Pernr = "00000000";
                inParams.Ul = unitLog;
                inParams.An = an;
                inParams.Luna = luna;


                ZsalBazaMatkl[] baza1 = new ZsalBazaMatkl[1];
                inParams.GtBazaclExp = baza1;

                Zsalav19[] salav19 = new Zsalav19[1];
                inParams.GtOuttabAv = salav19;

                ZSUM_VANZ_VS[] gtVenVs = new ZSUM_VANZ_VS[1];
                inParams.GtVenVs = gtVenVs;

                ZgetSalcvip24Response resp = salarizareVanzari.ZgetSalcvip24(inParams);

                response = serializer.Serialize(resp.GtOuttabAv);

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("getSalarizareDepartCVIP: \n\n" + ex.ToString() + "\n\n" + serializer.Serialize(inParams));
            }

            return response;
        }



        public string getSalarizareSD(string codAgent, string ul, string divizie, string an, string luna)
        {
            string salarizareResp = "";

            SalarizareSD salarizare = new SalarizareSD();
            DatePrincipale datePrincipale = new DatePrincipale();
            List<DetaliiBaza> listDetaliiBaza = new List<DetaliiBaza>();
            DetaliiTCF detaliiTCF = new DetaliiTCF();
            DetaliiCorectie detaliiCorectie = new DetaliiCorectie();
            List<DetaliiIncasari08> listDetaliiIncasari08 = new List<DetaliiIncasari08>();
            List<DetaliiCVS> listDetaliiCvs = new List<DetaliiCVS>();
            List<DetaliiMalus1> listDetaliiMalus1 = new List<DetaliiMalus1>();

            SapWsSalarizare.ZWS_SALARIZARE_24 salarizareVanzari = new SapWsSalarizare.ZWS_SALARIZARE_24();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            salarizareVanzari.Credentials = nc;
            salarizareVanzari.Timeout = 1200000;

            SapWsSalarizare.ZgetSalsd24 inParams = new SapWsSalarizare.ZgetSalsd24();

            try
            {

                inParams.Pernr = codAgent;
                inParams.Ul = ul;
                inParams.Divizie = divizie;
                inParams.An = an;
                inParams.Luna = luna;

                SapWsSalarizare.ZsalBazaMatklSd[] gtBazaclExp = new ZsalBazaMatklSd[1];
                SapWsSalarizare.ZsalCorrInc[] gtCorrInc = new ZsalCorrInc[1];
                SapWsSalarizare.ZsalCorr08[] gtInc08Ex = new ZsalCorr08[1];
                SapWsSalarizare.ZsalCorrMalus[] gtIncmaEx = new ZsalCorrMalus[1];
                SapWsSalarizare.ZsalFactMalus[] gtMalEx = new ZsalFactMalus[1];
                SapWsSalarizare.Zsalsd24[] gtOuttabSd = new Zsalsd24[1];
                SapWsSalarizare.ZsalNtcf[] gtTcf = new ZsalNtcf[1];
                SapWsSalarizare.ZSUM_VANZ_VS[] gtVenVs = new ZSUM_VANZ_VS[1];
                SapWsSalarizare.ZclIncrAlocat[] gtVenInc = new ZclIncrAlocat[1];
                SapWsSalarizare.Zcvss2024[] gtCVss = new Zcvss2024[1];

                inParams.GtBazaclExp = gtBazaclExp;
                inParams.GtCorrInc = gtCorrInc;
                inParams.GtInc08Ex = gtInc08Ex;
                inParams.GtIncmaEx = gtIncmaEx;
                inParams.GtMalEx = gtMalEx;
                inParams.GtOuttabSd = gtOuttabSd;
                inParams.GtTcf = gtTcf;
                inParams.GtVenVs = gtVenVs;
                inParams.GtVenIncr = gtVenInc;
                inParams.GtCvss = gtCVss;

                SapWsSalarizare.ZgetSalsd24Response response = salarizareVanzari.ZgetSalsd24(inParams);

                if (response.GtOuttabSd.Length > 0)
                {

                    datePrincipale.venitMJ_T1 = Double.Parse(response.GtOuttabSd[0].Baza.ToString());
                    datePrincipale.venitTCF = Double.Parse(response.GtOuttabSd[0].Venittcf.ToString());
                    datePrincipale.corectieIncasare = Double.Parse(response.GtOuttabSd[0].CorectIncas.ToString());
                    datePrincipale.venitCVS = Double.Parse(response.GtOuttabSd[0].VenitCvs.ToString());
                    datePrincipale.venitFinal = Double.Parse(response.GtOuttabSd[0].Venitfinal.ToString());
                    datePrincipale.venitStocNociv = Double.Parse(response.GtOuttabSd[0].VenitVanzVs.ToString());
                    datePrincipale.venitIncrucisate = Double.Parse(response.GtOuttabSd[0].VenitIncrucis.ToString());

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

                    if (response.GtTcf != null && response.GtTcf.Length > 0)
                    {

                        ZsalNtcf respDetaliiNtcf = response.GtTcf[0];

                        detaliiTCF.venitBaza = Double.Parse(respDetaliiNtcf.VenitBaza.ToString());
                        detaliiTCF.clientiAnterior = Int32.Parse(respDetaliiNtcf.NrclAnterior).ToString();
                        detaliiTCF.target = Int32.Parse(respDetaliiNtcf.Target).ToString();
                        detaliiTCF.clientiCurent = Int32.Parse(respDetaliiNtcf.NrclCurent).ToString();
                        detaliiTCF.coeficient = Double.Parse(respDetaliiNtcf.Coef.ToString());
                        detaliiTCF.venitTcf = Double.Parse(respDetaliiNtcf.Venittcf.ToString());
                    }

                    if (response.GtCorrInc != null && response.GtCorrInc.Length > 0)
                    {
                        ZsalCorrInc respDetaliiCor = response.GtCorrInc[0];

                        detaliiCorectie.venitBaza = Double.Parse(respDetaliiCor.Baza.ToString());
                        detaliiCorectie.incasari08 = Double.Parse(respDetaliiCor.Incas08.ToString());
                        detaliiCorectie.malus = Double.Parse(respDetaliiCor.Malus.ToString());
                        detaliiCorectie.venitCorectat = Double.Parse(respDetaliiCor.VenCorInc.ToString());
                    }

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

                    Zcvss2024[] respGtCvss = response.GtCvss;

                    for (int i = 0; i < respGtCvss.Length; i++)
                    {
                        DetaliiCVS detaliiCvs = new DetaliiCVS();
                        detaliiCvs.agent = respGtCvss[i].Sname;
                        detaliiCvs.venitBaza = Double.Parse(respGtCvss[i].Baza.ToString());
                        detaliiCvs.venitCvs = Double.Parse(respGtCvss[i].Venitcvs.ToString());
                        detaliiCvs.prag = Double.Parse(respGtCvss[i].Prag.ToString());
                        detaliiCvs.procent = Double.Parse(respGtCvss[i].Procent.ToString());
                        detaliiCvs.valNociv = Double.Parse(respGtCvss[i].ValNociv.ToString());
                        detaliiCvs.valTotal = Double.Parse(respGtCvss[i].ValTotal.ToString());

                        listDetaliiCvs.Add(detaliiCvs);

                    }


                }

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


                salarizareVanzari.Dispose();


                salarizare.datePrincipale = datePrincipale;
                salarizare.detaliiBaza = listDetaliiBaza;
                salarizare.detaliiTCF = detaliiTCF;
                salarizare.detaliiCorectie = detaliiCorectie;
                salarizare.detaliiIncasari08 = listDetaliiIncasari08;
                salarizare.detaliiCVS = listDetaliiCvs;
                salarizare.detaliiMalus = listDetaliiMalus1;
                salarizare.detaliiVanzariVS = response.GtVenVs.Cast<ZSUM_VANZ_VS>().ToList();
                salarizare.detaliiIncrAlocat = response.GtVenIncr.Cast<ZclIncrAlocat>().ToList();



                salarizareResp = new JavaScriptSerializer().Serialize(salarizare);
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("getSarizareSD: \n\n" + ex.ToString() + "\n\n" + new JavaScriptSerializer().Serialize(inParams));
            }

            return salarizareResp;
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

            SapWsSalarizare.ZWS_SALARIZARE_24 salarizareVanzari = new SapWsSalarizare.ZWS_SALARIZARE_24();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            salarizareVanzari.Credentials = nc;
            salarizareVanzari.Timeout = 1200000;

            SapWsSalarizare.ZgetSalka24 inParams = new ZgetSalka24();

            try
            {

                inParams.Pernr = codAgent;
                inParams.Ul = ul;
                inParams.An = an;
                inParams.Luna = luna;

                ZsalBazaMatkl[] gtBazaclExp = new ZsalBazaMatkl[1];
                ZsalCorrInc[] gtCorrInc = new ZsalCorrInc[1];
                ZsalCorr08[] gtInc08Ex = new ZsalCorr08[1];
                ZsalCorrMalus[] gtIncmaEx = new ZsalCorrMalus[1];
                ZsalFactMalus[] gtMalEx = new ZsalFactMalus[1];
                Zsalav19[] gtOuttabAv = new Zsalav19[1];
                ZSUM_VANZ_VS[] gtVenVs = new ZSUM_VANZ_VS[1];


                inParams.GtBazaclExp = gtBazaclExp;
                inParams.GtCorrInc = gtCorrInc;
                inParams.GtInc08Ex = gtInc08Ex;
                inParams.GtIncmaEx = gtIncmaEx;
                inParams.GtMalEx = gtMalEx;
                inParams.GtOuttabAv = gtOuttabAv;
                inParams.GtVenVs = gtVenVs;


                ZgetSalka24Response response = salarizareVanzari.ZgetSalka24(inParams);

                if (response.GtOuttabAv.Length > 0)
                {

                    datePrincipale.venitMJ_T1 = Double.Parse(response.GtOuttabAv[0].Baza.ToString());
                    datePrincipale.venitTCF = Double.Parse(response.GtOuttabAv[0].Venittcf.ToString());
                    datePrincipale.corectieIncasare = Double.Parse(response.GtOuttabAv[0].CorectIncas.ToString());
                    datePrincipale.venitFinal = Double.Parse(response.GtOuttabAv[0].Venitfinal.ToString());
                    datePrincipale.venitStocNociv = Double.Parse(response.GtOuttabAv[0].VenitVanzVs.ToString());


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

                    if (response.GtCorrInc != null && response.GtCorrInc.Length > 0)
                    {
                        ZsalCorrInc respDetaliiCor = response.GtCorrInc[0];

                        detaliiCorectie.venitBaza = Double.Parse(respDetaliiCor.Baza.ToString());
                        detaliiCorectie.incasari08 = Double.Parse(respDetaliiCor.Incas08.ToString());
                        detaliiCorectie.malus = Double.Parse(respDetaliiCor.Malus.ToString());
                        detaliiCorectie.venitCorectat = Double.Parse(respDetaliiCor.VenCorInc.ToString());
                    }



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

                salarizareVanzari.Dispose();

                salarizare.datePrincipale = datePrincipale;
                salarizare.detaliiBaza = listDetaliiBaza;
                salarizare.detaliiTCF = detaliiTCF;
                salarizare.detaliiCorectie = detaliiCorectie;
                salarizare.detaliiIncasari08 = listDetaliiIncasari08;
                salarizare.detaliiMalus = listDetaliiMalus1;
                salarizare.detaliiVanzariVS = response.GtVenVs.Cast<ZSUM_VANZ_VS>().ToList();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + "\n\n" + new JavaScriptSerializer().Serialize(inParams));
            }

            return new JavaScriptSerializer().Serialize(salarizare);


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

            SapWsSalarizare.ZWS_SALARIZARE_24 salarizareVanzari = new SapWsSalarizare.ZWS_SALARIZARE_24();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            salarizareVanzari.Credentials = nc;
            salarizareVanzari.Timeout = 1200000;

            ZgetSalsdka24 inParams = new ZgetSalsdka24();

            inParams.Pernr = codAgent;
            inParams.Ul = ul;
            inParams.An = an;
            inParams.Luna = luna;

            ZsalBazaMatklSd[] gtBazaclExp = new ZsalBazaMatklSd[1];
            ZsalCorrInc[] gtCorrInc = new ZsalCorrInc[1];
            ZsalCorr08[] gtInc08Ex = new ZsalCorr08[1];
            ZsalCorrMalus[] gtIncmaEx = new ZsalCorrMalus[1];
            ZsalFactMalus[] gtMalEx = new ZsalFactMalus[1];
            Zsalsd19[] gtOuttabSd = new Zsalsd19[1];
            ZsumVanzVs[] gtVenVs = new ZsumVanzVs[1];

            inParams.GtBazaclExp = gtBazaclExp;
            inParams.GtCorrInc = gtCorrInc;
            inParams.GtInc08Ex = gtInc08Ex;
            inParams.GtIncmaEx = gtIncmaEx;
            inParams.GtMalEx = gtMalEx;
            inParams.GtOuttabSd = gtOuttabSd;
            inParams.GtVenVs = gtVenVs;

            ZgetSalsdka24Response response = salarizareVanzari.ZgetSalsdka24(inParams);

            if (response.GtOuttabSd.Length > 0)
            {
                datePrincipale.venitMJ_T1 = Double.Parse(response.GtOuttabSd[0].Baza.ToString());
                datePrincipale.venitTCF = Double.Parse(response.GtOuttabSd[0].Venittcf.ToString());
                datePrincipale.corectieIncasare = Double.Parse(response.GtOuttabSd[0].CorectIncas.ToString());
                datePrincipale.venitFinal = Double.Parse(response.GtOuttabSd[0].Venitfinal.ToString());
                datePrincipale.venitStocNociv = Double.Parse(response.GtOuttabSd[0].VenitVanzVs.ToString());

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

            salarizareVanzari.Dispose();

            salarizare.datePrincipale = datePrincipale;
            salarizare.detaliiBaza = listDetaliiBaza;
            salarizare.detaliiTCF = detaliiTCF;
            salarizare.detaliiCorectie = detaliiCorectie;
            salarizare.detaliiIncasari08 = listDetaliiIncasari08;
            salarizare.detaliiMalus = listDetaliiMalus1;
            salarizare.detaliiCVS = listDetaliiCvs;
            salarizare.detaliiVanzariVS = response.GtVenVs.Cast<ZSUM_VANZ_VS>().ToList();


            return new JavaScriptSerializer().Serialize(salarizare);

        }

        public string getSalarizareCVA(string codAgent, string unitLog, string an, string luna)
        {

            SapWsSalarizare.ZWS_SALARIZARE_24 salarizareVanzari = new SapWsSalarizare.ZWS_SALARIZARE_24();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            salarizareVanzari.Credentials = nc;
            salarizareVanzari.Timeout = 1200000;

            ZgetSalcva24 inParams = new ZgetSalcva24();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            String response = "";


            try
            {

                inParams.Pernr = codAgent;
                inParams.Ul = unitLog;
                inParams.An = an;
                inParams.Luna = luna;


                ZsalBazaMatkl[] baza1 = new ZsalBazaMatkl[1];
                inParams.GtBazaclExp = baza1;

                ZsalNruf[] nruf = new ZsalNruf[1];
                inParams.GtExp = nruf;

                Zsalav19[] salav19 = new Zsalav19[1];
                inParams.GtOuttabAv = salav19;

                ZSUM_VANZ_VS[] gtVenVs = new ZSUM_VANZ_VS[1];
                inParams.GtVenVs = gtVenVs;

                ZgetSalcva24Response resp = salarizareVanzari.ZgetSalcva24(inParams);

                response = serializer.Serialize(resp);

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("getSalarizareCVA: \n\n" + ex.ToString() + "\n\n" + serializer.Serialize(inParams));
            }

            return response;
        }


        public string getSalarizareCVIP(string codAgent, string unitLog, string an, string luna)
        {

            SapWsSalarizare.ZWS_SALARIZARE_24 salarizareVanzari = new SapWsSalarizare.ZWS_SALARIZARE_24();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            salarizareVanzari.Credentials = nc;
            salarizareVanzari.Timeout = 1200000;

            ZgetSalcvip24 inParams = new ZgetSalcvip24();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            String response = "";


            try
            {

                inParams.Pernr = codAgent;
                inParams.Ul = unitLog;
                inParams.An = an;
                inParams.Luna = luna;

                ZsalBazaMatkl[] baza1 = new ZsalBazaMatkl[1];
                inParams.GtBazaclExp = baza1;

                Zsalav19[] salav19 = new Zsalav19[1];
                inParams.GtOuttabAv = salav19;

                ZSUM_VANZ_VS[] gtVenVs = new ZSUM_VANZ_VS[1];
                inParams.GtVenVs = gtVenVs;

                ZgetSalcvip24Response resp = salarizareVanzari.ZgetSalcvip24(inParams);

                response = serializer.Serialize(resp);

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("getSalarizareCVIP: \n\n" + ex.ToString() + "\n\n" + serializer.Serialize(inParams));
            }

            return response;
        }






        private static string formatDateRo(string dateEn)
        {
            string[] dateArray = dateEn.Split('-');
            return dateArray[2] + "-" + dateArray[1] + "-" + dateArray[0];
        }

    }
}