using System;
using System.Collections.Generic;
using System.Text;

namespace Distance.Utils
{
    public static class Convert
    {
        public static T To<T>(this object value)
        {
            if (value is T) return (T)value;
            if (typeof(T) == typeof(int)) return (T)(object)ToInt(value.ToString());
            if (typeof(T) == typeof(long)) return (T)(object)ToLong(value.ToString());
            if (typeof(T) == typeof(double)) return (T)(object)ToDouble(value.ToString());
            if (typeof(T) == typeof(string)) return (T)(object)value.ToString();
            return default(T);
        }

        public static bool ToBool(this string value)
        {
            if (bool.TryParse(value, out bool boolResult))
            {
                return boolResult;
            }
            if (int.TryParse(value, out int intResult))
            {
                return intResult != 0;
            }
            return false;
        }


        public static int ToInt(this string value) => ToInt(value, 0);
        public static int ToInt(this string value, int defval)
        {
            if (int.TryParse(value, out int result))
                return result;
            else
                return defval;
        }
        public static long ToLong(this string value) => ToLong(value, 0L);
        public static long ToLong(this string value, long defval)
        {
            if (long.TryParse(value, out long result))
                return result;
            else
                return defval;
        }
        public static double ToDouble(this string value) => ToDouble(value, 0D);
        public static double ToDouble(this string value, double defval)
        {
            if (double.TryParse(value, out double result))
                return result;
            else
                return defval;
        }

    }
}
