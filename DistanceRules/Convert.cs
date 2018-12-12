using System;
using System.Collections.Generic;
using System.Text;

namespace Distance.Rules
{
    public static class Convert
    {
        public static int ToInt(this string value, int defval = 0)
        {
            if (int.TryParse(value, out int result))
                return result;
            else
                return defval;
        }
        public static long ToLong(this string value, long defval=0)
        {
            if (long.TryParse(value, out long result))
                return result;
            else
                return defval;
        }
        public static double ToDouble(this string value, double defval = 0)
        {
            if (double.TryParse(value, out double result))
                return result;
            else
                return defval;
        }

    }
}
