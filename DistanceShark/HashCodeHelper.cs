
namespace Distance.Shark
{
    public static class HashCodeHelper
    {
           /// <summary>
        /// Computes a hash code for the byte array. Implementation is according to OpenJDK:
        /// http://hg.openjdk.java.net/jdk8u/jdk8u/jdk/file/be44bff34df4/src/share/classes/java/util/Arrays.java
        /// </summary>
        public static int ArrayHashCode(byte[] data)
        {
            if (data == null) return 0;
            int result = 1;
            foreach (var element in data)
                result = 31 * result + element;

            return result;
        }

        public static int CombineHashCodes(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }
    }
}