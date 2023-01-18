using System.Text;

namespace TCP_client
{
    public static class Utils
    {
        public enum Lang
        {
            En,
            Ru
        }

        public static byte[] ToBytes(string str, Encoding encoding) => encoding.GetBytes(str);
        public static byte[] ToBytes(string str, Lang language) => (language is Lang.Ru) ? Encoding.UTF8.GetBytes(str) : Encoding.ASCII.GetBytes(str);

        public static string ToString(byte[] bytes, int byteRcv, Encoding encoding) => encoding.GetString(bytes, 0, byteRcv);
        public static string ToString(byte[] bytes, int byteRcv, Lang language) => (language is Lang.Ru) ? Encoding.UTF8.GetString(bytes, 0, byteRcv) : Encoding.ASCII.GetString(bytes, 0, byteRcv);
    }
}
