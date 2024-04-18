using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace WebService1
{
    public class SalarizareConsilieri
    {
        public string getSalarizareConsilieri(string codAgent, string unitLog, string an, string luna)
        {

            SalarizareCVA.ZWS_SAL_CVA webService = new SalarizareCVA.ZWS_SAL_CVA();
            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;

            SalarizareCVA.ZGET_SALCVA inParams = new SalarizareCVA.ZGET_SALCVA();
            inParams.PERNR = codAgent;
            inParams.UL = unitLog;
            inParams.AN = an;
            inParams.LUNA = luna;

            SalarizareCVA.ZSAL_BAZA_MATKL[] baza1 = new SalarizareCVA.ZSAL_BAZA_MATKL[1];
            inParams.GT_BAZACL_EXP = baza1;

            SalarizareCVA.ZSAL_NRUF[] nruf = new SalarizareCVA.ZSAL_NRUF[1];
            inParams.GT_EXP = nruf;

            SalarizareCVA.ZSALAV19[] salav19 = new SalarizareCVA.ZSALAV19[1];
            inParams.GT_OUTTAB_AV = salav19;

            SalarizareCVA.ZGET_SALCVAResponse resp = webService.ZGET_SALCVA(inParams);

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            return serializer.Serialize(resp);
        }
    }
}