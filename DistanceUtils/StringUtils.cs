using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Distance.Utils
{
    public static class StringUtils
    {

        public static string ToCamelCase(this string text)
        {
            return String.Join("", text.Split(' ', '.', '_')
            .Select(i => Char.ToUpper(i[0]) + i.Substring(1)));
        }

        static TextInfo m_textInfo = new CultureInfo("en-US", false).TextInfo;
        public static string ToTitleCase(this string fieldName)
        {
            var name2 = m_textInfo.ToTitleCase(fieldName).Replace(".", "");
            return name2;
        }
        public static string ToString<T>(T value)
        {
            return value?.ToString()?? String.Empty;
        }

        public static string ToString<T>(T[] array)
        {
            if (array == null) return String.Empty;
            return String.Join(",", array.Select(x => StringUtils.ToString(x)));
        }
    }
}
