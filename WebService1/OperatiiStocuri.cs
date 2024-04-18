using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebService1.SAPWebServices;

namespace WebService1
{
    public class OperatiiStocuri
    {
        public string getStocSap(string codArticol, string um, string filiala, string tipUser)
        {

            string retVal = "";

            try {

                SAPWebServices.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE();

                SAPWebServices.ZstocSfa inParam = new ZstocSfa();
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
                webService.Credentials = nc;
                webService.Timeout = 300000;

                inParam.IpMatnr = codArticol;
                inParam.IpPlant = filiala;
                inParam.Meins = um;

                SAPWebServices.ZstocSfaResponse outParams = webService.ZstocSfa(inParam);

                retVal = outParams.EpStoc + "#" + outParams.Meins + "#1";
            }
            catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = "0#BUC#1";
            }

            return retVal;

        }
    }
}