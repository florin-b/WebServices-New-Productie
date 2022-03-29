using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace WebService1
{
    public class PierderiVanzari
    {

        public string getVanzariTotal(string codDepart)
        {

            List<PierderiVanzariTotal> pierderiTotal = new List<PierderiVanzariTotal>();

            SapWSPierderiVanzari.ZWS_CL_FACT_AVSD webService = new SapWSPierderiVanzari.ZWS_CL_FACT_AVSD();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(DatabaseConnections.getUser(), DatabaseConnections.getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;

            SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[] ITAB_CLIENT1 = new SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[1];
            SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[] ITAB_CLI_NIVELE1 = new SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[1];
            SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[] ITAB_TIPCL1 = new SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[1];
            SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[] ITAB_SDAV = new SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[1];
            SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[] ITAB_DVDD = new SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[1];

            SapWSPierderiVanzari.ZCL_FACT_AVSD inParam = new SapWSPierderiVanzari.ZCL_FACT_AVSD();

            inParam.LV_ANGAJ = "00000000";
            inParam.LV_PRCTR = " ";
            inParam.LV_SPART = codDepart.Substring(0,2);
            inParam.CU_DVDD = "X";

            inParam.ITAB_DVDD = ITAB_DVDD;
            inParam.ITAB_CLIENT = ITAB_CLIENT1;
            inParam.ITAB_CLI_NIVELE = ITAB_CLI_NIVELE1;
            inParam.ITAB_TIPCL = ITAB_TIPCL1;
            inParam.ITAB_SDAV = ITAB_SDAV;

            SapWSPierderiVanzari.ZCL_FACT_AVSDResponse response = new SapWSPierderiVanzari.ZCL_FACT_AVSDResponse();
            response = webService.ZCL_FACT_AVSD(inParam);

            for (int i = 0; i < response.ITAB_DVDD.Count(); i++)
            {
                PierderiVanzariTotal pTotal = new PierderiVanzariTotal();
                pTotal.ul = response.ITAB_DVDD[i].PRCTR;
                pTotal.nrClientiIstoric = Int32.Parse(response.ITAB_DVDD[i].NRCLFACT_TOTAL.ToString());
                pTotal.nrClientiPrezent = Int32.Parse(response.ITAB_DVDD[i].NRCLFACT_LNCRT.ToString());
                pTotal.nrClientiRest = Int32.Parse(response.ITAB_DVDD[i].NRCLFACT_DEFACT.ToString());
                pierderiTotal.Add(pTotal);
            }

            return new JavaScriptSerializer().Serialize(pierderiTotal);
        }


        public string getVanzariDepartament(string ul, string codDepart)
        {

            List<PierderiVanzariDep> pierderiDep = new List<PierderiVanzariDep>();

            SapWSPierderiVanzari.ZWS_CL_FACT_AVSD webService = new SapWSPierderiVanzari.ZWS_CL_FACT_AVSD();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(DatabaseConnections.getUser(), DatabaseConnections.getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;

            SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[] ITAB_CLIENT1 = new SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[1];
            SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[] ITAB_CLI_NIVELE1 = new SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[1];
            SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[] ITAB_TIPCL1 = new SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[1];
            SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[] ITAB_SDAV = new SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[1];

            SapWSPierderiVanzari.ZCL_FACT_AVSD inParam = new SapWSPierderiVanzari.ZCL_FACT_AVSD();

            string departVanz = codDepart.Substring(0, 2);
            string opt_all_avsd = " ";
            string opt_only_sd_1fil = "X";
            if (codDepart.Equals("10"))
            {
                departVanz = " ";
                opt_all_avsd = "X";
                opt_only_sd_1fil = " ";
            }

            inParam.LV_ANGAJ = "00000000";
            inParam.LV_PRCTR = ul;
            inParam.LV_SPART = departVanz;

            inParam.ITAB_CLIENT = ITAB_CLIENT1;
            inParam.ITAB_CLI_NIVELE = ITAB_CLI_NIVELE1;
            inParam.ITAB_TIPCL = ITAB_TIPCL1;
            inParam.ITAB_SDAV = ITAB_SDAV;
            inParam.ONLY_SD_1FIL = opt_only_sd_1fil;
            inParam.ALL_AVSD_1FIL = opt_all_avsd;
            inParam.CU_DVDD = " ";


            SapWSPierderiVanzari.ZCL_FACT_AVSDResponse response = new SapWSPierderiVanzari.ZCL_FACT_AVSDResponse();
            response = webService.ZCL_FACT_AVSD(inParam);

            for (int i = 0; i < response.ITAB_SDAV.Length; i++)
            {
                PierderiVanzariDep pDep = new PierderiVanzariDep();

                pDep.codAgent = response.ITAB_SDAV[i].COD_AGENT;
                pDep.numeAgent = response.ITAB_SDAV[i].NUME_AGENT;
                pDep.nrClientiIstoric = Int32.Parse(response.ITAB_SDAV[i].NRCLFACT_TOTAL.ToString());
                pDep.nrClientiPrezent = Int32.Parse(response.ITAB_SDAV[i].NRCLFACT_LNCRT.ToString());
                pDep.nrClientiRest = Int32.Parse(response.ITAB_SDAV[i].NRCLFACT_DEFACT.ToString());

                pierderiDep.Add(pDep);

            }

            pierderiDep = pierderiDep.OrderBy(o => o.numeAgent).ToList();

            return new JavaScriptSerializer().Serialize(pierderiDep);
        }




        public string getVanzariAv(string codAgent, string ul, string codDepart)
        {


            PierderiVanzariAV pierderiAV = new PierderiVanzariAV();

            List<PierderiAvHeader1> listPierderi = new List<PierderiAvHeader1>();
            List<PierderiTipClient> listPierderiTip = new List<PierderiTipClient>();
            List<PierderiNivel1> listPierderiNivel = new List<PierderiNivel1>();

            SapWSPierderiVanzari.ZWS_CL_FACT_AVSD webService = new SapWSPierderiVanzari.ZWS_CL_FACT_AVSD();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(DatabaseConnections.getUser(), DatabaseConnections.getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;

            SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[] ITAB_CLIENT1 = new SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[1];
            SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[] ITAB_CLI_NIVELE1 = new SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[1];
            SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[] ITAB_TIPCL1 = new SapWSPierderiVanzari.ZCL_FACT_AVSD_STR[1];

            SapWSPierderiVanzari.ZCL_FACT_AVSD inParam = new SapWSPierderiVanzari.ZCL_FACT_AVSD();

            string departVanz = codDepart.Substring(0, 2);
            string opt_all_avsd = " ";
            string opt_only_sd_1fil = "X";
            if (codDepart.Equals("10"))
            {
                departVanz = " ";
                opt_all_avsd = "X";
                opt_only_sd_1fil = " ";
            }

            inParam.LV_ANGAJ = codAgent;
            inParam.LV_PRCTR = ul;
            inParam.LV_SPART = departVanz;
            inParam.ONLY_SD_1FIL = opt_only_sd_1fil;
            inParam.ALL_AVSD_1FIL = opt_all_avsd;
            inParam.CU_DVDD = " ";



            inParam.ITAB_CLIENT = ITAB_CLIENT1;
            inParam.ITAB_CLI_NIVELE = ITAB_CLI_NIVELE1;
            inParam.ITAB_TIPCL = ITAB_TIPCL1;

            SapWSPierderiVanzari.ZCL_FACT_AVSDResponse response = new SapWSPierderiVanzari.ZCL_FACT_AVSDResponse();
            response = webService.ZCL_FACT_AVSD(inParam);

            for (int i = 0; i < response.ITAB_TIPCL.Length; i++)
            {
                PierderiAvHeader1 pVanz = new PierderiAvHeader1();
                pVanz.codTipClient = response.ITAB_TIPCL[i].KDGRP;
                pVanz.numeTipClient = response.ITAB_TIPCL[i].NUMEKDGRP;
                pVanz.nrClientiIstoric = Int32.Parse(response.ITAB_TIPCL[i].NRCLFACT_TOTAL.ToString());
                pVanz.nrClientiPrezent = Int32.Parse(response.ITAB_TIPCL[i].NRCLFACT_LNCRT.ToString());
                pVanz.nrClientiRest = Int32.Parse(response.ITAB_TIPCL[i].NRCLFACT_DEFACT.ToString());
                listPierderi.Add(pVanz);
            }

            listPierderi = listPierderi.OrderBy(o => o.numeTipClient).ToList();

            for (int i = 0; i < response.ITAB_CLIENT.Length; i++)
            {
                PierderiTipClient pTip = new PierderiTipClient();
                pTip.codTipClient = response.ITAB_CLIENT[i].KDGRP;
                pTip.numeClient = response.ITAB_CLIENT[i].NUMECLIENT;
                pTip.venitLC = Double.Parse(response.ITAB_CLIENT[i].VENITNET_LUNACRT.ToString());
                pTip.venitLC1 = Double.Parse(response.ITAB_CLIENT[i].VENITNET_LUNACRT_1.ToString());
                pTip.venitLC2 = Double.Parse(response.ITAB_CLIENT[i].VENITNET_LUNACRT_2.ToString());
                listPierderiTip.Add(pTip);

            }

            listPierderiTip = listPierderiTip.OrderBy(o => o.numeClient).ToList();

            for (int i = 0; i < response.ITAB_CLI_NIVELE.Length; i++)
            {
                PierderiNivel1 pNivel1 = new PierderiNivel1();
                pNivel1.numeClient = response.ITAB_CLI_NIVELE[i].NUMECLIENT;
                pNivel1.numeNivel1 = response.ITAB_CLI_NIVELE[i].DENNIV1.ToUpper();
                pNivel1.venitLC = Double.Parse(response.ITAB_CLI_NIVELE[i].VENITNET_LUNACRT.ToString());
                pNivel1.venitLC1 = Double.Parse(response.ITAB_CLI_NIVELE[i].VENITNET_LUNACRT_1.ToString());
                pNivel1.venitLC2 = Double.Parse(response.ITAB_CLI_NIVELE[i].VENITNET_LUNACRT_2.ToString());
                listPierderiNivel.Add(pNivel1);

            }

            listPierderiNivel = listPierderiNivel.OrderBy(o => o.numeNivel1).ToList();

            pierderiAV.pierderiAvHeader = listPierderi;
            pierderiAV.pierderiTipClient = listPierderiTip;
            pierderiAV.pierderiNivel1 = listPierderiNivel;

            return new JavaScriptSerializer().Serialize(pierderiAV);

        }



    }
}