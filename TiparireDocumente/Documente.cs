using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TiparireDocumente
{
    public class Documente
    {

        public List<Document> getModificariComanda(string filiala, string departament)
        {

            List<Document> listaDocumente = new List<Document>();

            List<Livrare> livrariModificate = new Service1().getLivrariModificate(filiala, departament);

            foreach (Livrare livrareModificata in livrariModificate)
            {
                List<Articol> articoleNoi = new Service1().getComandaNoua(livrareModificata.nrComanda);


                bool comandaStearsa = true;
                foreach (Articol articolSters in livrareModificata.listArticole)
                {
                    bool articolGasit = false;
                    Articol articolCurent = new Articol();
                    foreach (Articol articolNou in articoleNoi)
                    {
                        if (articolSters.codArticol.Equals(articolNou.codArticol))
                        {
                            articolGasit = true;
                            comandaStearsa = false;
                            articolCurent = articolNou;
                            break;
                        }

                    }

                    string depozit = "";
                    if (comandaStearsa)
                        depozit = new Service1().getDepozitLivrareStearsa(livrareModificata.nrComanda);

                    if (!articolGasit)
                    {
                        Document docNou = new Document();
                        docNou.client = livrareModificata.numeClient;
                        docNou.id = livrareModificata.nrLivrare;
                        docNou.emitere = livrareModificata.emitere;
                        docNou.isPregatit = "-1";
                        docNou.isTiparit = livrareModificata.tiparit;
                        docNou.codArticol = articolSters.codArticol;
                        docNou.cantitate = articolSters.cantitate.ToString();
                        docNou.um = articolSters.um;
                        docNou.numeArticol = articolSters.numeArticol;
                        docNou.pozitieArticol = articolSters.poz;
                        docNou.depozit = depozit;
                        docNou.modificare = "Articol sters";
                        docNou.tip = "D";
                        docNou.nrMasina = " ";
                        docNou.numeSofer = " ";
                        docNou.cantitateModificata = "0";
                        if (comandaStearsa)
                            docNou.infoStatus = "Livrare stearsa";
                        else
                            docNou.infoStatus = "Livrare modificata";
                        listaDocumente.Add(docNou);
                    }
                    else
                    {
                        Document docNou = new Document();
                        docNou.client = livrareModificata.numeClient;
                        docNou.id = livrareModificata.nrLivrare;
                        docNou.emitere = livrareModificata.emitere;
                        docNou.isPregatit = "-1";
                        docNou.isTiparit = livrareModificata.tiparit;
                        docNou.codArticol = articolCurent.codArticol;
                        docNou.cantitate = articolCurent.cantitate.ToString();
                        docNou.um = articolCurent.um;
                        docNou.numeArticol = articolCurent.numeArticol;
                        docNou.pozitieArticol = articolCurent.poz;
                        docNou.depozit = depozit;
                        docNou.modificare = "";
                        docNou.tip = "D";
                        docNou.nrMasina = " ";
                        docNou.numeSofer = " ";
                        docNou.cantitateModificata = "0";
                        docNou.infoStatus = "Livrare modificata";
                        listaDocumente.Add(docNou);
                    }
                }


            }

            return listaDocumente;

        }



    }
}