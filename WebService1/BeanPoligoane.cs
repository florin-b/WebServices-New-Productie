﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1
{
    public class LatLng
    {
        public double lat;
        public double lon;

        public LatLng()
        {

        }

        public LatLng(double lat, double lon)
        {
            this.lat = lat;
            this.lon = lon;
        }
    }

    public class Poligon
    {
        public string tipPoligon;
        public string numeFisier;
        public string filiala;
        public string tonaj;
        public string nume;
        public string interzis;
    }

    public class DatePoligon
    {

        public string filialaPrincipala;
        public string filialaSecundara;
        public string tipZona;
        public string limitareTonaj;
        public string nume;
        public string isRestrictionat;

        public DatePoligon()
        {

        }

        public DatePoligon(string filialaPrincipala, string filialaSecundara, string tipZona, string limitareTonaj, string nume, string isRestrictionat)
        {
            this.filialaPrincipala = filialaPrincipala;
            this.filialaSecundara = filialaSecundara;
            this.tipZona = tipZona;
            this.limitareTonaj = limitareTonaj;
            this.nume = nume;
            this.isRestrictionat = isRestrictionat;
        }


    }
}