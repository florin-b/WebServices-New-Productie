using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebService1.ClientiSemiactivi;
using System.Web.Script.Serialization;

namespace WebService1
{
    public class OperatiiSemiactivi
    {

        public string getClientiSemiactivi(string codAgent, string codDepart, string filiala)
        {


            ClientiSemiactivi.ZWBS_SEMIACTIVI wsSemiactivi = null;
            List<BeanClientSemiactiv> listClienti = new List<BeanClientSemiactiv>();

            try
            {

                wsSemiactivi = new ClientiSemiactivi.ZWBS_SEMIACTIVI();

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Auth.getUser(), Auth.getPass());
                wsSemiactivi.Credentials = nc;
                wsSemiactivi.Timeout = 300000;

                ClientiSemiactivi.ZsemiActivi1 inParam = new ClientiSemiactivi.ZsemiActivi1();

                inParam.ItData = new WebService1.ClientiSemiactivi.ZsemiActivi[1];
                inParam.PPernr = codAgent;
                inParam.PVkgrp = codDepart;
                inParam.PWerks = filiala;

                BeanClientSemiactiv client = null;

                ClientiSemiactivi.ZsemiActiviResponse resp = wsSemiactivi.ZsemiActivi(inParam);

                for (int i = 0; i < resp.ItData.Length; i++)
                {
                    client = new BeanClientSemiactiv();
                    client.numeClient = resp.ItData[i].Name1;
                    client.codClient = resp.ItData[i].Kunnr;
                    client.judet = resp.ItData[i].DenRegio;
                    client.localitate = resp.ItData[i].Ort01;
                    client.strada = resp.ItData[i].Stras;
                    client.numePersContact = resp.ItData[i].PersCont;
                    client.telPersContact = resp.ItData[i].Telf1;
                    client.vanzMedie = resp.ItData[i].CaMedie.ToString();
                    client.vanz03 = resp.ItData[i].Lun03.ToString();
                    client.vanz06 = resp.ItData[i].Lun06.ToString();
                    client.vanz07 = resp.ItData[i].Lun07.ToString();
                    client.vanz09 = resp.ItData[i].Lun09.ToString();
                    client.vanz040 = resp.ItData[i].Lun040.ToString();
                    client.vanz041 = resp.ItData[i].Lun041.ToString();
                    listClienti.Add(client);

                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (wsSemiactivi != null)
                    wsSemiactivi.Dispose();
            }


            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(listClienti);


        }



        public string getIstoricVanzari(string codClient)
        {

            ClientiSemiactivi.ZWBS_SEMIACTIVI wsSemiactivi = null;
            List<IstoricClient> listIstoric = new List<IstoricClient>();



            try
            {

                wsSemiactivi = new ClientiSemiactivi.ZWBS_SEMIACTIVI();

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Auth.getUser(), Auth.getPass());
                wsSemiactivi.Credentials = nc;
                wsSemiactivi.Timeout = 300000;

                /*

                ClientiSemiactivi.ZistoricVanz inParam = new ZistoricVanz();

                inParam.ItData = new LiteSFATestWebService.ClientiSemiactivi.ZclVanzIst[1];
                inParam.VKunnr = codClient;

                ClientiSemiactivi.ZistoricVanzResponse response = wsSemiactivi.ZistoricVanz(inParam);

                IstoricClient istoric = null;
                for (int i = 0; i < response.ItData.Length; i++)
                {
                    istoric = new IstoricClient();
                    istoric.codClient = codClient;
                    istoric.an = response.ItData[i].Anv;
                    istoric.luna = response.ItData[i].Luna;
                    istoric.vanz03 = response.ItData[i].Dep03.ToString();
                    istoric.vanz040 = response.ItData[i].Dep040.ToString();
                    istoric.vanz041 = response.ItData[i].Dep041.ToString();
                    istoric.vanz06 = response.ItData[i].Dep06.ToString();
                    istoric.vanz07 = response.ItData[i].Dep07.ToString();
                    istoric.vanz09 = response.ItData[i].Dep09.ToString();
                    listIstoric.Add(istoric);

                }
                
                 * */

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (wsSemiactivi != null)
                    wsSemiactivi.Dispose();
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(listIstoric);

        }


        private class IstoricClient
        {
            public string codClient;
            public string an;
            public string luna;
            public string vanz03;
            public string vanz06;
            public string vanz07;
            public string vanz09;
            public string vanz040;
            public string vanz041;

        }


    }
}