using System;
using System.Collections.Generic;
using System.Text;

namespace Distance.Utils
{
    public static class HashFunction
    {
        public static int GetHashCode(params object[] array)
        {
            if (array == null) return 0;
            int hash = 17;
            for (int i = 0; i < array.Length; i++)
                hash = 31 * hash + array[i]?.GetHashCode() ?? 0;
            return hash;
        }
    }
}
