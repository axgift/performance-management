using System;
using System.Collections.Generic;
using System.Text;

namespace Key
{
    public class Key
    {
        public static string authorizedweb()
        {
            return "192.168.1.241";
            //return "wallbuys.com";
        }

        public static bool isSaveurl(string url)
        {
            bool returnvalue = true;
            if (url.IndexOf(authorizedweb()) < 0)
                returnvalue = true;
            return returnvalue;
        }

        public static string GetKEY_64()
        {
            return "woshinia";
        }

        public static string IV_64()
        {
            return "woshinia";
        }

        public static bool isEffective()
        {
            bool returnValue = false;
            DateTime EndTime = Convert.ToDateTime("2018-01-01 00:00:00.001");
            DateTime NowTime = DateTime.Now;

            if (DateTime.Compare(EndTime, NowTime) > 0)
            {
                returnValue = true;
            }
            return returnValue;
        }
    }
}
