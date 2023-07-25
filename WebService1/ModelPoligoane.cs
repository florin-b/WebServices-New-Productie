using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService1
{
    public class ModelPoligoane
    {
        public static bool containsPoint(LatLng point, List<LatLng> polygon, bool geodesic)
        {
            int size = polygon.Count();
            if (size == 0)
            {
                return false;
            }
            double lat3 = deg2rad(point.lat);
            double lng3 = deg2rad(point.lon);
            LatLng prev = polygon.ElementAt(size - 1);
            double lat1 = deg2rad(prev.lat);
            double lng1 = deg2rad(prev.lon);
            int nIntersect = 0;
            foreach (LatLng point2 in polygon)
            {
                double dLng3 = wrap(lng3 - lng1, -Math.PI, Math.PI);

                if (lat3 == lat1 && dLng3 == 0)
                {
                    return true;
                }
                double lat2 = deg2rad(point2.lat);
                double lng2 = deg2rad(point2.lon);

                if (intersects(lat1, lat2, wrap(lng2 - lng1, -Math.PI, Math.PI), lat3, dLng3, geodesic))
                {
                    ++nIntersect;
                }
                lat1 = lat2;
                lng1 = lng2;
            }
            return (nIntersect & 1) != 0;
        }
        static double wrap(double n, double min, double max)
        {
            return (n >= min && n < max) ? n : (mod(n - min, max - min) + min);
        }

        static double mod(double x, double m)
        {
            return ((x % m) + m) % m;
        }

        private static bool intersects(double lat1, double lat2, double lng2, double lat3, double lng3,
        bool geodesic)
        {

            if ((lng3 >= 0 && lng3 >= lng2) || (lng3 < 0 && lng3 < lng2))
            {
                return false;
            }

            if (lat3 <= -Math.PI / 2)
            {
                return false;
            }

            if (lat1 <= -Math.PI / 2 || lat2 <= -Math.PI / 2 || lat1 >= Math.PI / 2 || lat2 >= Math.PI / 2)
            {
                return false;
            }
            if (lng2 <= -Math.PI)
            {
                return false;
            }
            double linearLat = (lat1 * (lng2 - lng3) + lat2 * lng3) / lng2;

            if (lat1 >= 0 && lat2 >= 0 && lat3 < linearLat)
            {
                return false;
            }

            if (lat1 <= 0 && lat2 <= 0 && lat3 >= linearLat)
            {
                return true;
            }

            if (lat3 >= Math.PI / 2)
            {
                return true;
            }

            return geodesic ? Math.Tan(lat3) >= tanLatGC(lat1, lat2, lng2, lng3)
                    : mercator(lat3) >= mercatorLatRhumb(lat1, lat2, lng2, lng3);
        }
        private static double tanLatGC(double lat1, double lat2, double lng2, double lng3)
        {
            return (Math.Tan(lat1) * Math.Sin(lng2 - lng3) + Math.Tan(lat2) * Math.Sin(lng3)) / Math.Sin(lng2);
        }

        static double mercator(double lat)
        {
            return Math.Log(Math.Tan(lat * 0.5 + Math.PI / 4));
        }

        private static double mercatorLatRhumb(double lat1, double lat2, double lng2, double lng3)
        {
            return (mercator(lat1) * (lng2 - lng3) + mercator(lat2) * lng3) / lng2;
        }

        private static double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }
    }
}