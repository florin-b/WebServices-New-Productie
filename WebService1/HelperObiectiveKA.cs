using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.ComponentModel;

namespace WebService1
{
    public class HelperObiectiveKA
    {


        public enum EnumStadiuObiectiv
        {
            [Description("In constructie")]
            IN_CONSTRUCTIE = 0,
            [Description("Neinceput")]
            NEINCEPUT = 1,
            [Description("Suspendat")]
            SUSPENDAT = 2
        }


        public enum EnumStadiuSubantrep
        {
            [Description("Terminat")]
            TERMINAT = 0,
            [Description("In constructie")]
            IN_CONSTRUCTIE = 1,
            [Description("Neinceput")]
            NEINCEPUT = 2
        }


        public enum EnumCategorieObiectiv
        {
            [Description("Civil")]
            CIVIL = 1,
            [Description("Industrial")]
            INDUSTRIAL = 2,
            [Description("Infrastructura")]
            INFRASTRUCTURA = 3,
            [Description("Special")]
            SPECIAL = 4

        }

        public enum EnumDepartFinisaje
        {
            [Description("Fundatie si suprastructura")]
            FUNDATIE = 4,
            [Description("Acoperisuri")]
            ACOPERIS = 9,
            [Description("Instalatii")]
            INSTALATII = 8,
            [Description("Electrice")]
            ELECTRICE = 5,
            [Description("Interioare")]
            INTERIOARE = 0
        }


        public enum EnumDepartInterioare
        {
            [Description("Parchet")]
            PARCHET = 3,
            [Description("Gips")]
            GIPS = 6,
            [Description("Chimice")]
            CHIMICE = 7

        }



        public enum EnumJudete
        {
            [Description("ALBA")]
            ALBA = 1,

            [Description("ARAD")]
            ARAD = 2,

            [Description("ARGES")]
            ARGES = 3,

            [Description("BACAU")]
            BACAU = 4,

            [Description("BIHOR")]
            BIHOR = 5,

            [Description("BISTRITA-NASAUD")]
            BISTRITA_NASAUD = 6,

            [Description("BOTOSANI")]
            BOTOSANI = 7,

            [Description("BRAILA")]
            BRAILA = 9,

            [Description("BRASOV")]
            BRASOV = 8,

            [Description("BUCURESTI")]
            BUCURESTI = 40,

            [Description("BUZAU")]
            BUZAU = 10,

            [Description("CALARASI")]
            CALARASI = 51,

            [Description("CARAS-SEVERIN")]
            CARAS_SEVERIN = 11,

            [Description("CLUJ")]
            CLUJ = 12,

            [Description("CONSTANTA")]
            CONSTANTA = 13,

            [Description("COVASNA")]
            COVASNA = 14,

            [Description("DAMBOVITA")]
            DAMBOVITA = 15,

            [Description("DOLJ")]
            DOLJ = 16,

            [Description("GALATI")]
            GALATI = 17,

            [Description("GIURGIU")]
            GIURGIU = 52,

            [Description("GORJ")]
            GORJ = 18,

            [Description("HARGHITA")]
            HARGHITA = 19,

            [Description("HUNEDOARA")]
            HUNEDOARA = 20,

            [Description("IALOMITA")]
            IALOMITA = 21,

            [Description("IASI")]
            IASI = 22,

            [Description("ILFOV")]
            ILFOV = 23,

            [Description("MARAMURES")]
            MARAMURES = 23,

            [Description("MEHEDINTI")]
            MEHEDINTI = 24,

            [Description("MURES")]
            MURES = 25,

            [Description("NEAMT")]
            NEAMT = 26,

            [Description("OLT")]
            OLT = 27,

            [Description("PRAHOVA")]
            PRAHOVA = 28,

            [Description("SALAJ")]
            SALAJ = 29,

            [Description("SATU-MARE")]
            SATU_MARE = 30,

            [Description("SIBIU")]
            SIBIU = 31,

            [Description("SUCEAVA")]
            SUCEAVA = 32,

            [Description("TELEORMAN")]
            TELEORMAN = 33,

            [Description("TIMIS")]
            TIMIS = 34,

            [Description("TULCEA")]
            TULCEA = 35,

            [Description("VALCEA")]
            VALCEA = 36,

            [Description("VASLUI")]
            VASLUI = 37,

            [Description("VRANCEA")]
            VRANCEA = 37
        }

        public static string getEnumDescription(Enum en)
        {
            Type type = en.GetType();

            MemberInfo[] memInfo = type.GetMember(en.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return en.ToString();
        }



    }
}