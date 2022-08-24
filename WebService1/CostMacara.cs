using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1
{

    public class ComandaCalculDescarcare
    {
        public string filiala;
        public string listArticole;
    }

    public class CostDescarcare
    {
        public string filiala;
        public List<ArticolDescarcare> articoleDescarcare;
        public bool sePermite;
        public List<ArticolPalet> articolePaleti;

    }


    public class ArticolDescarcare
    {
        public string cod;
        public string depart;
        public string valoare;
        public string cantitate;
        public string valoareMin;
    }


    public class ArticolCalculDesc
    {
        public string cod;
        public double cant;
        public string um;
        public string depoz;
    }

    public class ArticolPalet
    {
        public string codPalet;
        public string numePalet;
        public string depart;
        public string cantitate;
        public string pretUnit;
        public string furnizor;
        public string codArticol;
        public string numeArticol;
        public string cantArticol;
        public string umArticol;

        public override string ToString()
        {
            return "ArticolPalet [codPalet=" + codPalet + ", numePalet=" + numePalet + ", depart=" + depart + ", cantitate=" + cantitate + ", pretUnit=" + pretUnit + ", codArticol=" + codArticol + "]";
        }
    }

    public class CalculDescarcare
    {
        public string filiala;
        public List<ArticolCalculDesc> listArticole;

    }
}