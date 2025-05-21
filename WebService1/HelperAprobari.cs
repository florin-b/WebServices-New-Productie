using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1
{
    public class HelperAprobari
    {
        public static bool isAprobareDV(List<ArticolComanda> listArticole, string tipUser, List<TaxaComanda> taxeComanda, DateLivrare dateLivrare, ComandaVanzare comandaVanzare)
        {

            if (tipUser.Equals("AV") || tipUser.Equals("SD") || tipUser.Equals("KA") || tipUser.Equals("CVA") || tipUser.Equals("SDCVA"))
                return isAprobareDV_1(listArticole, tipUser, comandaVanzare, dateLivrare);
            else
                return isAprobareDV_2(listArticole, tipUser, comandaVanzare, taxeComanda, dateLivrare);
        }


        private static bool isAprobareDV_1(List<ArticolComanda> listArticole, string tipUser, ComandaVanzare comandaVanzare, DateLivrare dateLivrare)
        {

            bool isAprobare = false;
            HashSet<string> setAprobari = new HashSet<string>();
            double pretMinimUnitar = 0;

            foreach (ArticolComanda articol in listArticole)
            {
                if (articol.pretMinim == 0)
                    continue;


                pretMinimUnitar = articol.pretMinim;

                if (tipUser.Equals("CVA") || tipUser.Equals("SDCVA") || Utils.isUnitLogGed(dateLivrare.unitLog))
                    pretMinimUnitar = articol.pretMinim / articol.multiplu;

                if (articol.pretUnit < pretMinimUnitar)
                {
                    isAprobare = true;
                    setAprobari.Add(articol.depart.Substring(0, 2));
                }
            }

            if (comandaVanzare.canalDistrib.Equals("20"))
                comandaVanzare.necesarAprobariCV = " ";

            if ((tipUser.Equals("CVA") || tipUser.Equals("SDCVA")) && setAprobari.Count > 0)
                comandaVanzare.necesarAprobariCV = string.Join(",", setAprobari);

            if (Utils.isUnitLogGed(dateLivrare.unitLog) && (tipUser.Equals("AV") || tipUser.Equals("SD") || tipUser.Equals("KA")) && setAprobari.Count > 0)
                comandaVanzare.necesarAprobariCV = string.Join(",", setAprobari);


            return isAprobare;

        }


        private static bool isAprobareDV_2(List<ArticolComanda> listArticole, string tipUser, ComandaVanzare comandaVanzare, List<TaxaComanda> taxeComanda, DateLivrare dateLivrare)
        {
            bool marjaCmdPozitiva = true;
            bool articolSubCmp = false;

            double totalAdaos = 0;
            double adaosArticol = 0;
            double cmpCorectatUnit = 0;
            HashSet<string> setAprobari = new HashSet<string>();

            comandaVanzare.necesarAprobariCV = " ";

            foreach (ArticolComanda articol in listArticole)
            {

                if (articol.pretMinim == 0 || articol.cmpCorectat == 0)
                    continue;

                adaosArticol = (articol.pretUnit - articol.pretMinim / articol.multiplu) * articol.cantitate;


                if (adaosArticol < 0)
                {
                    if (isUserAprobariCV(tipUser))
                        setAprobari.Add(articol.depart.Substring(0, 2));
                    else if (isUserAprobari_11(tipUser))
                        setAprobari.Add("11");
                }

                totalAdaos += adaosArticol;

                cmpCorectatUnit = (articol.cmpCorectat / Double.Parse(articol.cantUmb));

                if (articol.pretUnit < cmpCorectatUnit)
                {
                    articolSubCmp = true;

                    if (isUserAprobariCV(tipUser))
                        setAprobari.Add(articol.depart.Substring(0, 2));
                    else if (isUserAprobari_11(tipUser))
                        setAprobari.Add("11");
                }
            }


            if ((isUserAprobariCV(tipUser) || isUserAprobari_11(tipUser)) && setAprobari.Count > 0)
                comandaVanzare.necesarAprobariCV = string.Join(",", setAprobari);

            double pragAprobare = 0;

            if (tipUser.Contains("IP") && taxeComanda != null)
                pragAprobare = getPragAprobare(taxeComanda, dateLivrare);

            marjaCmdPozitiva = totalAdaos >= pragAprobare;

            if (!marjaCmdPozitiva && tipUser.Contains("IP") && setAprobari.Count == 0)
            {
                comandaVanzare.necesarAprobariCV = "11";
            }

            return !marjaCmdPozitiva || articolSubCmp;
        }

        private static bool isUserAprobariCV(string tipUser)
        {
            return tipUser.Contains("VR") || tipUser.Contains("VO") || tipUser.Contains("W") || tipUser.Contains("VS");
        }

        private static bool isUserAprobari_11(string tipUser)
        {
            return tipUser.Contains("IP") || tipUser.Equals("SMR") || tipUser.Equals("SSCM") || tipUser.Equals("CONSCOL");
        }


        private static double getPragAprobare(List<TaxaComanda> taxeComanda, DateLivrare dateLivrare)
        {
            double pragAprobare = 0;

            string unitLog = dateLivrare.unitLog;

            if (dateLivrare.filialaCLP != null && dateLivrare.filialaCLP.Trim().Length == 4)
                unitLog = dateLivrare.filialaCLP;

            foreach (TaxaComanda taxa in taxeComanda)
            {
                if (taxa.filiala.Equals(unitLog))
                    pragAprobare += taxa.valoare;

            }

            return pragAprobare;
        }
    }
}