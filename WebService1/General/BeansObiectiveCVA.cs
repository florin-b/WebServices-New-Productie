using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1
{
    public class Objective
    {
        public string oraid;
        public string id;
        public string typeId; // Mandatory
        public string cvaCode; // Mandatory
        public string regionID; // Mandatory
        public string name; // Mandatory
        public string creationDate; // Mandatory
        public string beneficiaryId; // Mandatory
        public string beneficiaryType; // Mandatory
        public string authorizationStart; // Mandatory
        public string authorizationEnd; // Mandatory
        public string estimationValue;
        public string address;
        public string zip;
        public string gps;
        public string stageId; // Mandatory
        public string phaseId; // Mandatory
        public string expirationPhase; // Mandatory
        public string status;
        public string statusId;
        public string categoryId;
        public string numeExecutant;
        public string cuiExecutant;
        public string nrcExecutant;
        public string telBenef;
        public string filiala;
        public string numeMeserias;
        public string prenMeserias;
        public string telMeserias;

    }

    public class Beneficiar
    {
        public string oraid;
        public string id;
        public string cui;
        public string region_id;
        public string name;
        public string type;
        public string nr_rc;
        public string cnp;
        public string status;
        public string cvaCode;
        

    }

    public class TabeleObiectiveCVA
    {
        public string obiective;
        public string beneficiari;
        public string stadii;

    }

    public class ObjectivePhase
    {
        public long id;
        public long phase_id;
        public long objective_id;
        public int days_nr;
        public string phase_start;
        public string phase_end;
        public string cvaCode;
    }


}