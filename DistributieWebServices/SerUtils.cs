using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace DistributieWebServices
{
    public class SerUtils
    {
        public static string serializeObject(Object objectToSer)
        {
            return new JavaScriptSerializer().Serialize(objectToSer);
        }
    }
}